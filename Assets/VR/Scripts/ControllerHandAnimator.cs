using System;
using HospitalVR.Interaction;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace HospitalVR
{
    /// <summary>Gestos visuales de mandos; el seguimiento lo proporciona el padre con TrackedPoseDriver.</summary>
    public sealed class ControllerHandAnimator : MonoBehaviour
    {
        const float NearFaceDistance = .36f;

        [Serializable]
        public struct FingerJoint
        {
            public Transform bone;
            public Quaternion openRotation;
            public Quaternion closedRotation;
            public bool indexFinger;
        }

        [SerializeField] InputActionProperty gripAction;
        [SerializeField] InputActionProperty triggerAction;
        [SerializeField, Min(0.1f)] float gestureSpeed = 10f;
        [SerializeField] FingerJoint[] joints = Array.Empty<FingerJoint>();

        public float GripValue { get; private set; }
        public float TriggerValue { get; private set; }

        TrackedPoseDriver controllerPose;
        Camera playerCamera;
        Renderer[] handRenderers;

        void Awake()
        {
            controllerPose = GetComponentInParent<TrackedPoseDriver>();
            var origin = GetComponentInParent<XROrigin>();
            if (origin) playerCamera = origin.Camera;
            handRenderers = GetComponentsInChildren<Renderer>(true);
        }

        void OnEnable()
        {
            // Las acciones locales pertenecen a este componente; las referencias externas, al jugador.
            if (gripAction.reference == null) gripAction.action?.Enable();
            if (triggerAction.reference == null) triggerAction.action?.Enable();
            UpdateVisibility();
        }

        void OnDisable()
        {
            if (gripAction.reference == null) gripAction.action?.Disable();
            if (triggerAction.reference == null) triggerAction.action?.Disable();
            GripValue = TriggerValue = 0f;
            ApplyPose();
            foreach (var hand in handRenderers) hand.forceRenderingOff = false;
        }

        void LateUpdate()
        {
            GripValue = Mathf.MoveTowards(GripValue, ReadValue(gripAction), gestureSpeed * Time.deltaTime);
            TriggerValue = Mathf.MoveTowards(TriggerValue, ReadValue(triggerAction), gestureSpeed * Time.deltaTime);
            ApplyPose();
            UpdateVisibility();
        }

        void UpdateVisibility()
        {
            // Los modelos cerca de la cara pueden cubrir el menú completo. El puntero sigue activo.
            bool nearFace = playerCamera && controllerPose
                && (playerCamera.transform.position - controllerPose.transform.position).sqrMagnitude < NearFaceDistance * NearFaceDistance;
            bool tracked = TrackedPoseStatus.HasTracking(controllerPose, InputTrackingState.Position | InputTrackingState.Rotation);
            foreach (var hand in handRenderers) hand.forceRenderingOff = nearFace || !tracked;
        }

        static float ReadValue(InputActionProperty input)
        {
            return input.action != null && input.action.enabled
                ? Mathf.Clamp01(input.action.ReadValue<float>()) : 0f;
        }

        void ApplyPose()
        {
            foreach (var joint in joints)
            {
                if (joint.bone == null) continue;
                joint.bone.localRotation = Quaternion.Slerp(joint.openRotation, joint.closedRotation,
                    joint.indexFinger ? TriggerValue : GripValue);
            }
        }
    }
}
