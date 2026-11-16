using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

namespace HospitalVR.Interaction
{
    /// <summary>
    /// Corrige la altura del jugador con visor real. El Rift ya informa la altura desde el suelo, por lo que
    /// un desplazamiento fijo del Camera Offset duplicaba la estatura y obligaba a agacharse hasta perder
    /// el seguimiento de los Touch. Con simulador de escritorio solo aplica el ajuste manual del usuario.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class PlayerHeightCalibrator : MonoBehaviour
    {
        public XROrigin origin;
        [Tooltip("Simulador de escritorio. Mientras esté activo no se recalibra automáticamente.")]
        public GameObject desktopSimulator;
        [Tooltip("Altura de ojos usada solo si el visor entrega posiciones relativas a la cabeza y no al suelo.")]
        public float fallbackEyeHeight = 1.6f;
        [Tooltip("Por encima de esta altura local se considera que el visor ya informa la altura real desde el suelo.")]
        public float floorTrackingThreshold = 0.6f;
        public float step = 0.05f;
        public float maxAdjust = 0.6f;

        const string PrefKey = "HospitalVR.AjusteAltura";
        public float UserAdjust { get; private set; }
        public bool HeadsetActive => XRSettings.isDeviceActive && (desktopSimulator == null || !desktopSimulator.activeInHierarchy);
        public float EyeHeight => origin && origin.Camera ? origin.Camera.transform.position.y - origin.transform.position.y : 0f;

        [Tooltip("Al detectar el visor, coloca al jugador en el punto de inicio mirando al menú.")]
        public bool recenterOnStart = true;

        float baseOffset, desktopOffset, trackedSince = -1f;
        Vector3 spawnPosition, spawnForward = Vector3.forward;
        bool calibrated;
        Transform Offset => origin && origin.CameraFloorOffsetObject ? origin.CameraFloorOffsetObject.transform : null;

        void Reset() => origin = GetComponent<XROrigin>();

        void Awake()
        {
            if (!origin) origin = GetComponent<XROrigin>();
            try { UserAdjust = Mathf.Clamp(PlayerPrefs.GetFloat(PrefKey, 0f), -maxAdjust, maxAdjust); } catch { UserAdjust = 0f; }
            if (Offset) desktopOffset = Offset.localPosition.y;
            if (origin) { spawnPosition = origin.transform.position; spawnForward = origin.transform.forward; }
        }

        void Start()
        {
            // Con visor se solicita espacio de suelo: la altura del jugador es la real.
            if (HeadsetActive && origin) origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
        }

        void LateUpdate()
        {
            if (!Offset) return;
            if (!HeadsetActive)
            {
                trackedSince = -1f; calibrated = false;
                if (!Mathf.Approximately(UserAdjust, 0f)) SetOffset(desktopOffset + UserAdjust);
                return;
            }
            if (trackedSince < 0f) trackedSince = Time.unscaledTime;
            // Esperar a que el runtime entregue la primera pose estable antes de medir.
            if (!calibrated && Time.unscaledTime - trackedSince > 0.75f) { Recalibrate(); if (recenterOnStart) Recenter(); }
            if (calibrated) SetOffset(baseOffset + UserAdjust);
        }

        /// <summary>Vuelve a medir la cabeza: con seguimiento de suelo no añade desplazamiento.</summary>
        public void Recalibrate()
        {
            if (!origin || !origin.Camera) return;
            float head = origin.Camera.transform.localPosition.y;
            baseOffset = head > floorTrackingThreshold ? 0f : Mathf.Max(0f, fallbackEyeHeight - head);
            calibrated = true;
        }

        /// <summary>
        /// Coloca la cabeza del jugador en el punto de inicio mirando al menú, sin importar dónde esté
        /// dentro de su espacio real. Útil al pasar el visor a otro participante.
        /// </summary>
        public void Recenter()
        {
            if (!origin || !origin.Camera) return;
            origin.MatchOriginUpCameraForward(Vector3.up, spawnForward);
            var cam = origin.Camera.transform.position;
            origin.MoveCameraToWorldLocation(new Vector3(spawnPosition.x, cam.y, spawnPosition.z));
        }

        public void Raise() => SetAdjust(UserAdjust + step);
        public void Lower() => SetAdjust(UserAdjust - step);
        public void ResetAdjust() { SetAdjust(0f); if (HeadsetActive) Recalibrate(); }

        void SetAdjust(float value)
        {
            UserAdjust = Mathf.Clamp(Mathf.Round(value * 100f) / 100f, -maxAdjust, maxAdjust);
            try { PlayerPrefs.SetFloat(PrefKey, UserAdjust); PlayerPrefs.Save(); } catch { }
            if (!HeadsetActive && Offset) SetOffset(desktopOffset + UserAdjust);
        }

        void SetOffset(float y)
        {
            var p = Offset.localPosition;
            if (Mathf.Abs(p.y - y) > 0.0005f) Offset.localPosition = new Vector3(p.x, y, p.z);
        }

        public string Describe()
        {
            string adjust = Mathf.Approximately(UserAdjust, 0f) ? "sin ajuste" : (UserAdjust > 0 ? "+" : "−") + Mathf.Abs(UserAdjust * 100f).ToString("0") + " cm";
            return $"Altura de ojos {EyeHeight:0.00} m · {adjust}";
        }
    }
}
