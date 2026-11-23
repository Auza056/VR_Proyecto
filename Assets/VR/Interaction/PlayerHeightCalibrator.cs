using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace HospitalVR.Interaction
{
    /// <summary>
    /// Configura la altura del XR Origin según el espacio de seguimiento del visor.
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
        bool calibrated, requestedFloor;
        TrackingOriginModeFlags calibratedMode;
        TrackedPoseDriver headPose;
        Transform Offset => origin && origin.CameraFloorOffsetObject ? origin.CameraFloorOffsetObject.transform : null;

        void Reset() => origin = GetComponent<XROrigin>();

        void Awake()
        {
            if (!origin) origin = GetComponent<XROrigin>();
            try { UserAdjust = Mathf.Clamp(PlayerPrefs.GetFloat(PrefKey, 0f), -maxAdjust, maxAdjust); } catch { UserAdjust = 0f; }
            if (Offset) desktopOffset = Offset.localPosition.y;
            if (origin) { spawnPosition = origin.transform.position; spawnForward = origin.transform.forward; }
            if (origin && origin.Camera) headPose = origin.Camera.GetComponent<TrackedPoseDriver>();
        }

        void LateUpdate()
        {
            if (!Offset) return;
            if (!HeadsetActive)
            {
                trackedSince = -1f; calibrated = false;
                requestedFloor = false;
                SetOffset(desktopOffset + UserAdjust);
                return;
            }
            if (!requestedFloor)
            {
                requestedFloor = true;
                origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            }

            if (origin.CurrentTrackingOriginMode == TrackingOriginModeFlags.Floor) SetOffset(UserAdjust);
            if (!HasTrackedHead())
            {
                trackedSince = -1f;
                return;
            }
            if (trackedSince < 0f) trackedSince = Time.unscaledTime;
            if (!calibrated && Time.unscaledTime - trackedSince > .35f)
            {
                Recalibrate();
                if (recenterOnStart) Recenter();
            }
            else if (calibrated && calibratedMode != origin.CurrentTrackingOriginMode)
            {
                Recalibrate();
            }
            if (calibrated) SetOffset(baseOffset + UserAdjust);
        }

        bool HasTrackedHead() => headPose &&
            TrackedPoseStatus.HasTracking(headPose, InputTrackingState.Position | InputTrackingState.Rotation);

        /// <summary>Vuelve a medir la altura a partir de una pose válida.</summary>
        public void Recalibrate()
        {
            if (!origin || !origin.Camera || !HasTrackedHead()) return;
            calibratedMode = origin.CurrentTrackingOriginMode;
            float head = origin.Camera.transform.localPosition.y;
            baseOffset = calibratedMode switch
            {
                TrackingOriginModeFlags.Floor => 0f,
                TrackingOriginModeFlags.Device => fallbackEyeHeight,
                _ => head > floorTrackingThreshold ? 0f : Mathf.Max(0f, fallbackEyeHeight - head)
            };
            calibrated = true;
        }

        /// <summary>
        /// Coloca la cabeza del jugador en el punto de inicio mirando al menú, sin importar dónde esté
        /// dentro de su espacio real. Útil al pasar el visor a otro participante.
        /// </summary>
        public void Recenter()
        {
            if (!origin || !origin.Camera || (HeadsetActive && !HasTrackedHead())) return;
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
