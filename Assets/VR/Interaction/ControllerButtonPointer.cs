using HospitalVR.RCP;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace HospitalVR.Interaction
{
    /// <summary>Apunta con la pose del mando a los botones físicos del simulador.</summary>
    [RequireComponent(typeof(TrackedPoseDriver))]
    public sealed class ControllerButtonPointer : MonoBehaviour
    {
        [SerializeField] bool leftHand;
        [SerializeField] XRDirectInteractor grabInteractor;
        [SerializeField] Material beamMaterial;
        [SerializeField, Min(0.1f)] float reach = 4f;

        TrackedPoseDriver pose;
        InputAction trigger;
        LineRenderer beam;
        RcpTouchButton hovered;
        bool triggerReady;
        readonly RaycastHit[] obstacles = new RaycastHit[12];

        void Awake()
        {
            pose = GetComponent<TrackedPoseDriver>();
            string handUsage = leftHand ? "LeftHand" : "RightHand";
            trigger = new InputAction("Pulsar botón", InputActionType.Button, $"<XRController>{{{handUsage}}}/trigger");

            if (!beamMaterial) return;
            var visual = new GameObject("Rayo de menú");
            visual.transform.SetParent(transform, false);
            beam = visual.AddComponent<LineRenderer>();
            beam.sharedMaterial = beamMaterial;
            beam.useWorldSpace = true;
            beam.positionCount = 2;
            beam.startWidth = .004f;
            beam.endWidth = .009f;
            var color = new MaterialPropertyBlock();
            color.SetColor("_BaseColor", new Color(.25f, .9f, 1f, .9f));
            beam.SetPropertyBlock(color);
            beam.shadowCastingMode = ShadowCastingMode.Off;
            beam.receiveShadows = false;
            beam.enabled = false;
        }

        void OnEnable() => trigger.Enable();

        void OnDisable()
        {
            trigger.Disable();
            triggerReady = false;
            SetHover(null);
            if (beam) beam.enabled = false;
        }

        void OnDestroy() => trigger.Dispose();

        void LateUpdate()
        {
            bool pressed = triggerReady && trigger.WasPressedThisFrame();
            triggerReady = !trigger.IsPressed();

            SetHover(FindTarget(out Vector3 hit));
            if (beam)
            {
                beam.enabled = hovered && hovered.interactable;
                if (beam.enabled)
                {
                    beam.SetPosition(0, transform.position);
                    beam.SetPosition(1, hit);
                }
            }

            if (hovered && pressed) hovered.PressFromPointer(transform);
        }

        RcpTouchButton FindTarget(out Vector3 hit)
        {
            hit = default;
            if ((grabInteractor && grabInteractor.hasSelection)
                || !TrackedPoseStatus.HasTracking(pose, InputTrackingState.Position | InputTrackingState.Rotation))
                return null;

            var ray = new Ray(transform.position, transform.forward);
            RcpTouchButton target = null;
            float distance = reach;
            foreach (var button in RcpTouchButton.ActiveButtons)
            {
                if (button.TryAim(ray, transform, out float candidate) && candidate < distance)
                {
                    target = button;
                    distance = candidate;
                }
            }
            if (!target) return null;

            // Ignora la cápsula del propio jugador y los triggers. Las paredes y muebles sí tapan el rayo.
            int hits = Physics.RaycastNonAlloc(ray, obstacles, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits; i++)
            {
                if (!obstacles[i].collider.transform.IsChildOf(transform.root)
                    && !target.transform.IsChildOf(obstacles[i].collider.transform))
                    return null;
            }
            hit = ray.GetPoint(distance);
            return target;
        }

        void SetHover(RcpTouchButton target)
        {
            if (hovered == target) return;
            if (hovered) hovered.SetPointerHover(false);
            hovered = target;
            if (hovered) hovered.SetPointerHover(true);
        }
    }
}
