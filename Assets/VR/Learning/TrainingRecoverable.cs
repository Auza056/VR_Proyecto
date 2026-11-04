using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
namespace HospitalVR.Learning
{
    public sealed class TrainingRecoverable : MonoBehaviour
    {
        Vector3 home; Quaternion rotation;
        XRGrabInteractable grab; Rigidbody rb;
        void Awake() { home=transform.position; rotation=transform.rotation; grab=GetComponent<XRGrabInteractable>(); rb=GetComponent<Rigidbody>(); }
        public void Recover()
        {
            if(grab && grab.isSelected) return;
            transform.SetPositionAndRotation(home,rotation);
            if(rb) { rb.linearVelocity=Vector3.zero; rb.angularVelocity=Vector3.zero; }
        }
        void Update() { if(transform.position.y<-.2f || Vector3.Distance(transform.position,home)>4f) Recover(); }
    }
}
