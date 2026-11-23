using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace HospitalVR.Interaction
{
    internal static class TrackedPoseStatus
    {
        public static bool HasTracking(TrackedPoseDriver pose, InputTrackingState required)
        {
            if (!pose) return true;
            if (!pose.isActiveAndEnabled) return false;
            if (pose.ignoreTrackingState) return true;

            var action = pose.trackingStateInput.action;
            return action != null && action.enabled && action.controls.Count > 0
                && ((InputTrackingState)action.ReadValue<int>() & required) == required;
        }
    }
}
