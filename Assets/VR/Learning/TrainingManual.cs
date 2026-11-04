using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
namespace HospitalVR.Learning
{
    [RequireComponent(typeof(XRGrabInteractable),typeof(Rigidbody))]
    public sealed class TrainingManual : MonoBehaviour
    {
        public TrainingContent content;
        public TMP_Text title,body,footer,confirmation;
        public Transform[] illustrations;
        public Transform pulseDemo;
        public Transform dock;
        public int Page {get;private set;}
        public bool GrabTest {get;private set;}
        public bool ReleaseTest {get;private set;}
        public bool ButtonTest {get;private set;}
        XRGrabInteractable grab;
        Rigidbody rb;
        void Awake() { grab=GetComponent<XRGrabInteractable>(); rb=GetComponent<Rigidbody>(); }
        void OnEnable() { if(!grab) grab=GetComponent<XRGrabInteractable>(); grab.selectEntered.AddListener(OnGrab); grab.selectExited.AddListener(OnRelease); }
        void OnDisable() { grab.selectEntered.RemoveListener(OnGrab); grab.selectExited.RemoveListener(OnRelease); }
        void Start() => ShowPage();
        public void RefreshDisplay() => ShowPage();
        void OnGrab(SelectEnterEventArgs args) { GrabTest=true; Confirm(); }
        void OnRelease(SelectExitEventArgs args) { ReleaseTest=true; Confirm(); }
        public void Next() { Page=(Page+1)%content.pages.Length; ShowPage(); }
        public void Previous() { Page=(Page+content.pages.Length-1)%content.pages.Length; ShowPage(); }
        public void TestButton() { ButtonTest=true; Confirm(); }
        void Confirm() { if(confirmation) confirmation.text=$"Agarrar {(GrabTest?"SÍ":"--")}  Soltar {(ReleaseTest?"SÍ":"--")}  Tocar {(ButtonTest?"SÍ":"--")}"; }
        public void Recover()
        {
            if(!dock || grab.isSelected) return;
            transform.SetPositionAndRotation(dock.position,dock.rotation);
            rb.linearVelocity=Vector3.zero; rb.angularVelocity=Vector3.zero;
        }
        void Update()
        {
            if(dock && !grab.isSelected && (transform.position.y<.2f || Vector3.Distance(transform.position,dock.position)>2.5f)) Recover();
            if(pulseDemo && Page==4) pulseDemo.localScale=Vector3.one*(1f+.12f*Mathf.Max(0,1f-(Time.unscaledTime%(60f/110f))/.12f));
        }
        public void ResetManual() { Page=0; GrabTest=ReleaseTest=ButtonTest=false; Recover(); ShowPage(); }
        void ShowPage()
        {
            if(!content || content.pages==null || content.pages.Length==0) return;
            var page=content.pages[Page]; title.text=page.title; body.text=page.instruction;
            footer.text=$"{Page+1}/{content.pages.Length} · {content.scope}";
            for(int i=0;i<illustrations.Length;i++) if(illustrations[i]) illustrations[i].gameObject.SetActive(i==page.illustration);
            Confirm();
        }
    }
}
