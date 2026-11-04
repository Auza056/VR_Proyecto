using HospitalVR.Learning;
using HospitalVR.RCP;
using TMPro;
using UnityEngine;
namespace HospitalVR.Presentation
{
    public enum ChecklistStep { Safety, Response, Breathing, Location, Situation, Aed, Continue }
    public sealed class TrainingChecklist : MonoBehaviour
    {
        public TrainingFlow flow;
        public RcpTouchButton touch;
        public TMP_Text label;
        public ChecklistStep step;
        public string caption;
        public bool contrastOnly;
        float next;
        void Update()
        {
            if(!flow || Time.unscaledTime<next) return;next=Time.unscaledTime+.12f;
            var p=flow.Progress;
            bool done=step switch {ChecklistStep.Safety=>p.Safety,ChecklistStep.Response=>p.Response,ChecklistStep.Breathing=>p.Breathing,
                ChecklistStep.Location=>p.Location,ChecklistStep.Situation=>p.Situation,ChecklistStep.Aed=>p.Aed,_=>false};
            bool valid=step<=ChecklistStep.Breathing?p.Stage==TrainingStage.Reconocer:p.Stage==TrainingStage.PedirAyuda;
            if(contrastOnly && !p.Responding) valid=false;
            bool callComplete=p.Location && p.Situation && (p.Responding || p.Aed);
            touch.interactable=valid && !done && (step!=ChecklistStep.Continue || callComplete);
            touch.normalColor=done?new Color(.11f,.45f,.35f):new Color(.035f,.32f,.33f);
            touch.disabledColor=done?new Color(.07f,.28f,.22f):new Color(.10f,.14f,.17f);
            label.text=caption+(done?"\nLISTO":"");label.color=done?new Color(.54f,1f,.77f):touch.interactable?Color.white:new Color(.52f,.60f,.65f);
        }
    }
}
