using System.Collections.Generic;
using HospitalVR.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace HospitalVR.RCP
{
    /// <summary>
    /// Botón físico que admite contacto del mando y selección con el puntero.
    /// </summary>
    public sealed class RcpTouchButton : MonoBehaviour
    {
        internal static readonly HashSet<RcpTouchButton> ActiveButtons = new HashSet<RcpTouchButton>();

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
        public Vector3 pressDirection = Vector3.down;
        public bool interactable = true;
        public Renderer feedbackRenderer;
        public Color normalColor = new Color(.035f,.32f,.33f);
        public Color hoverColor = new Color(.08f,.56f,.58f);
        public Color disabledColor = new Color(.10f,.14f,.17f);
        public Color pressedColor = new Color(.15f,.72f,.53f);
        public AudioSource feedbackSource;
        public AudioClip pressClip;
        [Tooltip("Vibración breve en el Touch que pulsa el botón (0 = sin vibración).")]
        [Range(0f, 1f)] public float hapticAmplitude = 0.35f;
        public float hapticDuration = 0.04f;
        public UnityEvent onPressed = new UnityEvent();

        bool armed = true;
        float nextAllowed;
        Vector3 visualRest;
        MaterialPropertyBlock feedbackBlock;
        float confirmedUntil;
        int pointerHoverCount;
        public int PressCount { get; private set; }

        void Awake()
        {
            if (pressVisual != null) visualRest = pressVisual.localPosition;
            feedbackBlock = new MaterialPropertyBlock();
        }

        void OnEnable()
        {
            ActiveButtons.Add(this);
            armed = false; // si la mano ya está dentro al aparecer, no se activa hasta que salga
            nextAllowed = Time.unscaledTime + 0.3f;
        }

        void OnDisable() => ActiveButtons.Remove(this);

        void Update()
        {
            bool inside = false;
            Transform touching = null;
            for (int i = 0; i < hands.Length && !inside; i++)
            {
                Transform hand = hands[i];
                if (hand == null || !hand.gameObject.activeInHierarchy) continue;
                Vector3 local = Quaternion.Inverse(transform.rotation) * (hand.TransformPoint(contactOffset) - transform.position) - triggerCenter;
                inside = IsInside(local) && TrackedPoseStatus.HasTracking(hand.GetComponent<TrackedPoseDriver>(), InputTrackingState.Position);
                if (inside) touching = hand;
            }

            if (pressVisual != null) pressVisual.localPosition = inside && interactable ? visualRest + pressDirection * pressDepth : visualRest;
            if (feedbackRenderer != null)
            {
                Color color = !interactable ? disabledColor : Time.unscaledTime < confirmedUntil ? pressedColor : inside || pointerHoverCount > 0 ? hoverColor : normalColor;
                if (feedbackRenderer is SpriteRenderer sprite) { sprite.color = color; }
                else
                {
                    feedbackBlock.SetColor("_BaseColor", color);
                    feedbackBlock.SetColor("_Color", color);
                    feedbackRenderer.SetPropertyBlock(feedbackBlock);
                }
            }
            if (!interactable) { armed = false; return; }

            if (!inside)
            {
                armed = true;
                return;
            }
            if (armed) PressFromHand(touching);
        }

        bool IsInside(Vector3 local) => Mathf.Abs(local.x) <= triggerSize.x * 0.5f
            && Mathf.Abs(local.y) <= triggerSize.y * 0.5f
            && Mathf.Abs(local.z) <= triggerSize.z * 0.5f;

        internal bool TryAim(Ray ray, Transform hand, out float distance)
        {
            distance = 0f;
            if (System.Array.IndexOf(hands, hand) < 0) return false;
            var normal = transform.rotation * (triggerCenter == Vector3.zero ? -pressDirection : triggerCenter.normalized);
            if (Vector3.Dot(normal, ray.direction) >= -0.001f) return false;

            var center = transform.position + transform.rotation * triggerCenter;
            var plane = new Plane(normal, center);
            return plane.Raycast(ray, out distance)
                && IsInside(Quaternion.Inverse(transform.rotation) * (ray.GetPoint(distance) - center));
        }

        internal void SetPointerHover(bool hovering) => pointerHoverCount += hovering ? 1 : -1;

        internal void PressFromPointer(Transform hand) => PressFromHand(hand);

        void PressFromHand(Transform hand)
        {
            if (!interactable || !isActiveAndEnabled || Time.unscaledTime < nextAllowed) return;
            armed = false;
            nextAllowed = Time.unscaledTime + cooldown;
            Press();
            Buzz(hand);
        }

        /// <summary>Activa el botón y reproduce su sonido.</summary>
        public void Press()
        {
            if (!interactable || !isActiveAndEnabled) return;
            PressCount++;
            confirmedUntil = Time.unscaledTime + .22f;
            if (feedbackSource != null && pressClip != null) feedbackSource.PlayOneShot(pressClip);
            onPressed.Invoke();
        }

        void Buzz(Transform hand)
        {
            if (hand == null || hapticAmplitude <= 0f) return;
            var player = hand.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticImpulsePlayer>();
            if (player != null) player.SendHapticImpulse(hapticAmplitude, hapticDuration);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.4f);
            Gizmos.DrawWireCube(triggerCenter, triggerSize);
        }
    }
}
