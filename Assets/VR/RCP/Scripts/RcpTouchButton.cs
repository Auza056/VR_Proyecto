using UnityEngine;
using UnityEngine.Events;

namespace HospitalVR.RCP
{
    /// <summary>
    /// Botón físico que se activa al introducir la mano (posición del mando) en su volumen.
    /// No requiere pulsar botones del mando ni interactores. Para volver a activarse, la mano debe salir.
    /// </summary>
    public sealed class RcpTouchButton : MonoBehaviour
    {
        [Tooltip("Mandos ya seguidos por el jugador.")]
        public Transform[] hands = new Transform[2];
        [Tooltip("Punto de contacto respecto al mando, en su espacio local.")]
        public Vector3 contactOffset = Vector3.zero;
        [Tooltip("Volumen sensible en el espacio local del botón (metros, sin escala).")]
        public Vector3 triggerSize = new Vector3(0.14f, 0.1f, 0.12f);
        public Vector3 triggerCenter = new Vector3(0f, 0.03f, 0f);
        [Min(0.1f)] public float cooldown = 0.6f;
        public Transform pressVisual;
        public float pressDepth = 0.012f;
        public AudioSource feedbackSource;
        public AudioClip pressClip;
        public UnityEvent onPressed = new UnityEvent();

        bool armed = true;
        float nextAllowed;
        Vector3 visualRest;

        void Awake()
        {
            if (pressVisual != null) visualRest = pressVisual.localPosition;
        }

        void OnEnable()
        {
            armed = false; // si la mano ya está dentro al aparecer, no se activa hasta que salga
            nextAllowed = Time.time + 0.3f;
        }

        void Update()
        {
            bool inside = false;
            for (int i = 0; i < hands.Length && !inside; i++)
            {
                Transform hand = hands[i];
                if (hand == null || !hand.gameObject.activeInHierarchy) continue;
                Vector3 local = Quaternion.Inverse(transform.rotation) * (hand.TransformPoint(contactOffset) - transform.position) - triggerCenter;
                inside = Mathf.Abs(local.x) <= triggerSize.x * 0.5f
                      && Mathf.Abs(local.y) <= triggerSize.y * 0.5f
                      && Mathf.Abs(local.z) <= triggerSize.z * 0.5f;
            }

            if (pressVisual != null) pressVisual.localPosition = inside ? visualRest - Vector3.up * pressDepth : visualRest;

            if (!inside)
            {
                armed = true;
                return;
            }
            if (!armed || Time.time < nextAllowed) return;

            armed = false;
            nextAllowed = Time.time + cooldown;
            Press();
        }

        /// <summary>Activa el botón y reproduce su sonido.</summary>
        public void Press()
        {
            if (feedbackSource != null && pressClip != null) feedbackSource.PlayOneShot(pressClip);
            onPressed.Invoke();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.4f);
            Gizmos.DrawWireCube(triggerCenter, triggerSize);
        }
    }
}
