using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
namespace HospitalVR.Learning
{
    /// <summary>Guion educativo fijo de DEA. No analiza ritmos reales ni produce descarga.</summary>
    public sealed class TrainingAed : MonoBehaviour
    {
        public Transform[] hands;
        public Transform chest;
        public Transform[] targets;
        public XRGrabInteractable[] pads;
        public TMP_Text message;
        public float placementDistance=.10f;
        public float clearDistance=.45f;
        public float analysisDuration=3f;
        public AedSequence Sequence {get;}=new AedSequence();
        public int SimulatedShocks {get;private set;}
        Vector3[] homes;
        Quaternion[] rotations;
        float analysisStarted;
        void Awake()
        {
            homes=new Vector3[pads.Length]; rotations=new Quaternion[pads.Length];
            for(int i=0;i<pads.Length;i++) {homes[i]=pads[i].transform.position; rotations[i]=pads[i].transform.rotation;}
        }
        void Start()=>Refresh();
        bool HandsClear()
        {
            if(!chest || hands==null || hands.Length<2) return false;
            foreach(var hand in hands)
            {
                if(!hand || !hand.gameObject.activeInHierarchy || Vector3.Distance(hand.position,chest.position)<clearDistance) return false;
                var pose=hand.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                if(pose && !pose.ignoreTrackingState)
                {
                    var a=pose.trackingStateInput.action;
                    if(a==null || !a.enabled || a.controls.Count==0 || (a.ReadValue<int>()&1)==0) return false;
                }
            }
            return true;
        }
        public void PowerOn() {Sequence.PowerOn(); Refresh();}
        public void Shock()
        {
            if(Sequence.DeliverIndicatedShock(HandsClear())) SimulatedShocks++;
            Refresh();
        }
        void Update()
        {
            if(Sequence.Stage==AedStage.Parches)
            {
                for(int i=0;i<pads.Length;i++)
                {
                    bool placed=i==0?Sequence.FirstPad:Sequence.SecondPad;
                    if(!placed && !pads[i].isSelected && Vector3.Distance(pads[i].transform.position,targets[i].position)<placementDistance)
                    {
                        pads[i].transform.SetPositionAndRotation(targets[i].position,targets[i].rotation);
                        pads[i].GetComponent<Rigidbody>().isKinematic=true; pads[i].enabled=false; Sequence.PlacePad(i); Refresh();
                    }
                }
                if(Sequence.BeginAnalysis(HandsClear())) {analysisStarted=Time.time; Refresh();}
            }
            else if(Sequence.Stage==AedStage.Analizando)
            {
                if(!HandsClear()) {Sequence.InterruptAnalysis(); Refresh();}
                else if(Time.time-analysisStarted>=analysisDuration) {Sequence.AnalysisComplete(); Refresh();}
            }
        }
        public void ResetDevice()
        {
            Sequence.Reset(); SimulatedShocks=0;
            if(homes!=null) for(int i=0;i<pads.Length;i++)
            {
                if(pads[i].isSelected) continue;
                pads[i].enabled=true;
                var rb=pads[i].GetComponent<Rigidbody>(); rb.isKinematic=false;
                pads[i].transform.SetPositionAndRotation(homes[i],rotations[i]); rb.linearVelocity=Vector3.zero; rb.angularVelocity=Vector3.zero;
            }
            GetComponent<TrainingRecoverable>()?.Recover(); Refresh();
        }
        void Refresh()
        {
            if(!message) return;
            message.text=Sequence.Stage switch
            {
                AedStage.Apagado=>"DEA · SIMULACIÓN\nToca ENCENDER. No produce descargas reales.",
                AedStage.Parches=>$"Expón el pecho virtual y coloca los parches según el dibujo.\nParche 1: {(Sequence.FirstPad?"colocado":"pendiente")} · Parche 2: {(Sequence.SecondPad?"colocado":"pendiente")}\nAparta ambas manos para analizar.",
                AedStage.Analizando=>"ANALIZANDO (guion simulado)\nNadie debe tocar al paciente. Si acercas las manos, el análisis vuelve a empezar.",
                AedStage.Apartarse=>"DESCARGA INDICADA POR EL GUION\nComprueba que nadie toque al paciente. Aparta las manos y toca DESCARGA.",
                _=>"DESCARGA SIMULADA COMPLETADA\nSigue la indicación del equipo y reanuda la práctica de RCP.\nGuion pendiente de revisión por instructor."
            };
        }
    }
}
