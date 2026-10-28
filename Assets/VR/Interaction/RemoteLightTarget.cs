using UnityEngine;

namespace HospitalVR.Interaction
{
    /// <summary>Explicit one-to-one link between a physical lamp collider and its light.</summary>
    public sealed class RemoteLightTarget : MonoBehaviour
    {
        public Light controlledLight;
        public Renderer lens;
        public Material offMaterial;

        public bool SwitchOff()
        {
            if (controlledLight == null || !controlledLight.enabled) return false;
            controlledLight.enabled = false;
            if (lens != null && offMaterial != null) lens.sharedMaterial = offMaterial;
            return true;
        }
    }
}