using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace HospitalVR.Interaction
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class LightRemote : MonoBehaviour
    {
        public Transform rayOrigin;
        [Min(0.1f)] public float maximumDistance = 12f;
        public LayerMask rayMask = Physics.DefaultRaycastLayers;
        public Transform buttonVisual;
        public float pressDepth = .003f;
        public AudioSource buttonAudio;
        public AudioClip clickClip;
        XRGrabInteractable grab;
        Vector3 buttonRest;
        float pressedUntil;

        public int ActivationCount { get; private set; }
        public Light LastSwitchedOff { get; private set; }

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            if (buttonVisual != null) buttonRest = buttonVisual.localPosition;
        }

        void OnEnable()
        {
            if (grab == null) grab = GetComponent<XRGrabInteractable>();
            grab.activated.AddListener(OnActivated);
        }

        void OnDisable()
        {
            if (grab != null) grab.activated.RemoveListener(OnActivated);
            if (buttonVisual != null) buttonVisual.localPosition = buttonRest;
        }

        void Update()
        {
            if (buttonVisual != null)
                buttonVisual.localPosition = buttonRest +
                    (Time.time < pressedUntil ? Vector3.down * pressDepth : Vector3.zero);
        }

        void OnActivated(ActivateEventArgs args)
        {
            // XRI routes the activate input of the selecting controller to this object.
            if (!grab.isSelected) return;
            ActivationCount++;
            pressedUntil = Time.time + .15f;
            if (buttonAudio != null && clickClip != null) buttonAudio.PlayOneShot(clickClip);
            TrySwitchOff();
        }

        public bool TrySwitchOff()
        {
            LastSwitchedOff = null;
            if (rayOrigin == null) return false;
            // Sort all hits so the held remote can be ignored without shooting through walls.
            var hits = Physics.RaycastAll(rayOrigin.position, rayOrigin.forward,
                maximumDistance, rayMask, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(transform)) continue;
                var target = hit.collider.GetComponentInParent<RemoteLightTarget>();
                if (target == null || !target.SwitchOff()) return false;
                LastSwitchedOff = target.controlledLight;
                return true; // Only the nearest solid target can be switched off.
            }
            return false;
        }

        void OnDrawGizmosSelected()
        {
            if (rayOrigin == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(rayOrigin.position, rayOrigin.forward * maximumDistance);
        }
    }
}
