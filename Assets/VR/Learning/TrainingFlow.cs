using System.Collections.Generic;
using HospitalVR.RCP;
using TMPro;
using UnityEngine;
namespace HospitalVR.Learning
{
    public sealed class TrainingFlow : MonoBehaviour
    {
        public TrainingContent content;
        public TrainingManual manual;
        public RcpSession session;
        public RcpMetronome metronome;
        public RcpPanelView rcpPanel;
        public TMP_Text flowText,caseText,phoneText;
        public TMP_Text contrastFeedback;
        public GameObject recognitionControls,phoneControls;
        public Transform collapseCase,contrastCase;
        public bool guidedHints=true;
        public float hintDelay=12f;
        public TrainingProgress Progress {get;}=new TrainingProgress();
        public readonly List<string> Actions=new List<string>();
        public RcpResults PracticeResults {get;private set;}
        public RcpResults ChallengeResults {get;private set;}
        public bool HasPracticeResults {get;private set;}
        public bool HasChallengeResults {get;private set;}
        public float ComparisonDuration {get;private set;}
        float lastAction;
        bool applying;
        void OnEnable() { if(session) session.StateChanged+=OnSession; }
        void OnDisable() { if(session) session.StateChanged-=OnSession; }
        void Start() { lastAction=Time.time; Refresh(); }
        void Update()
        {
            if(session.challenge && (session.State==RcpSessionState.Preparacion || session.State==RcpSessionState.Activa) && (metronome.isOn || rcpPanel.markerAid)) session.aidsUsed=true;
            if(flowText && guidedHints && Time.time-lastAction>hintDelay && (Progress.Stage==TrainingStage.Reconocer || Progress.Stage==TrainingStage.PedirAyuda))
                flowText.text="Pista: "+Progress.MissingStep;
        }
        void Record(string value) { Actions.Add(Time.time.ToString("0.0")+" · "+value); lastAction=Time.time; Refresh(); }
        public void BeginCase() { ResetObjects(); Progress.BeginCase(); Record("Iniciar caso: adulto colapsado"); }
        public void BeginContrast() { ResetObjects(); Progress.BeginContrast(); Record("Iniciar contraste: persona que responde"); }
        public void ObserveSafety() { Progress.ObserveSafety(); Record("Observó el entorno"); }
        public void CheckResponse() { Progress.CheckResponse(); Record(Progress.Responding?"Responde: me siento mareado":"Sin respuesta al hablar y tocar hombros"); }
        public void CheckBreathing() { Progress.CheckBreathing(); Record(Progress.Responding?"Respira normalmente":"No respira normalmente; jadeos aislados"); }
        string CurrentLocation => Progress.Responding ? content.contrastLocation : content.location;
        public void ProvideLocation() { Progress.ProvideLocation(); Record("Indicó ubicación: "+CurrentLocation); }
        public void DescribeSituation() { Progress.DescribeSituation(); Record(Progress.Responding?"Describió persona consciente que pide ayuda":"Describió adulto sin respuesta y respiración anormal"); }
        public void AskForAed() { Progress.AskForAed(); Record("Pidió a un testigo traer DEA"); }
        public void ContinueToPractice()
        {
            if(Progress.Responding)
            {
                if(Progress.FinishContrast()) Record("Ayuda solicitada; permanecer junto a persona que responde. Sin compresiones automáticas.");
                else Record("Completa observación y llamada");
                return;
            }
            if(!Progress.BeginPractice()) { Record(Progress.MissingStep); return; }
            Configure(false); session.ResetPractice(); Record("Práctica guiada en habitación 1");
        }
        public void DirectPractice()
        {
            applying=true; session.Cancel(); applying=false;
            ClearComparison();
            Progress.DirectPractice(); Configure(false); session.ResetPractice(); Record("Acceso directo a estación");
        }
        public void StartChallenge()
        {
            if(Progress.Stage!=TrainingStage.Reto) { Record("Finaliza primero la práctica o usa el acceso directo"); return; }
            if(Progress.ChallengeRunning) return;
            applying=true; session.Cancel(); applying=false;
            session.sessionDuration=ComparisonDuration;
            Progress.BeginChallenge(); Configure(true); session.ResetPractice(); Record("Reto iniciado sin ayudas");
        }
        void Configure(bool challenge)
        {
            session.challenge=challenge; session.aidsUsed=!challenge;
            metronome.isOn=!challenge; rcpPanel.markerAid=!challenge; metronome.StopClock();
        }
        public void ToggleMarker() { rcpPanel.markerAid=!rcpPanel.markerAid; if(session.challenge && rcpPanel.markerAid) session.aidsUsed=true; Record("Marcador "+rcpPanel.markerAid); }
        public void ResetCase() { ResetObjects(); Progress.Reset(); Actions.Clear(); Record("Caso reiniciado"); }
        void ClearComparison() { PracticeResults=default; ChallengeResults=default; HasPracticeResults=HasChallengeResults=false; ComparisonDuration=session.sessionDuration; }
        void ResetObjects()
        {
            applying=true; session.Cancel(); applying=false; Configure(false);
            ClearComparison();
            if(manual) manual.ResetManual();
            foreach(var item in GetComponentsInChildren<TrainingRecoverable>(true)) item.Recover();
            var aed=GetComponentInChildren<TrainingAed>(true); if(aed) aed.ResetDevice();

        }
        void OnSession(RcpSessionState state)
        {
            if(applying) return;
            if(state==RcpSessionState.Preparacion && !session.challenge) ComparisonDuration=session.sessionDuration;
            if(state==RcpSessionState.Preparacion && !session.challenge && Progress.Stage!=TrainingStage.Practicar)
            { Progress.DirectPractice(); ClearComparison(); Configure(false); }
            if(state!=RcpSessionState.Finalizada) return;
            if(session.challenge)
            {
                ChallengeResults=session.Results; HasChallengeResults=true;
                if(string.IsNullOrEmpty(session.Results.stopReason)) Progress.FinishChallenge();
                else Progress.CancelChallenge();
            }
            else
            {
                PracticeResults=session.Results; HasPracticeResults=true;
                if(string.IsNullOrEmpty(session.Results.stopReason)) Progress.FinishPractice();
            }
            Record("Sesión finalizada: "+session.Compressions+" compresiones detectadas");
        }
        public void Refresh()
        {
            if(recognitionControls) recognitionControls.SetActive(Progress.Stage==TrainingStage.Reconocer);
            if(phoneControls) phoneControls.SetActive(Progress.Stage==TrainingStage.PedirAyuda);
            if(collapseCase) collapseCase.gameObject.SetActive(!Progress.Responding);
            if(contrastCase) contrastCase.gameObject.SetActive(Progress.Responding);
            if(flowText) flowText.text=Progress.Stage switch
            {
                TrainingStage.Manual=>"1 · Consulta el manual. Después toca INICIAR CASO.\nTambién puedes usar PRÁCTICA DIRECTA en la estación.",
                TrainingStage.Reconocer=>"2 · Reconocer: observa, comprueba respuesta y respiración.\n"+Progress.MissingStep,
                TrainingStage.PedirAyuda=>"3 · Pedir ayuda: completa el teléfono simulado.\n"+Progress.MissingStep,
                TrainingStage.Practicar=>"4 · Practicar: ve al marcador de la habitación 1.\nColoca las manos y practica "+session.sessionDuration+" segundos.",
                TrainingStage.Reto=>Progress.ChallengeRunning?"5 · Reto en curso. Sin marcador ni metrónomo por defecto.":"Práctica terminada. Consulta resultados en la estación y toca INICIAR RETO.",
                _=>Progress.Responding?"Caso de contraste completo: pide ayuda y observa a la persona. No se iniciaron compresiones.":"6 · Resultados en la estación. Compara tus intentos y elige una sugerencia para repetir."
            };
            if(caseText) caseText.text=Progress.Responding
                ?"OTRO CASO · PERSONA QUE RESPONDE\n"+(Progress.Response?"Dice: me siento mareado.":"Acércate y comprueba si responde.")+"\n"+(Progress.Breathing?"Respira normalmente.":"Observa su respiración.")
                :"RECEPCIÓN UNIVERSITARIA · CASO SIMULADO\nUn adulto acaba de colapsar sobre suelo firme.\n"+(Progress.Response?"Sin respuesta. ":"")+(Progress.Breathing?"Respiración anormal: jadeos.":"");
            if(contrastFeedback) contrastFeedback.text=Progress.Responding ?
                "CASO B · "+(Progress.Response ? "Responde: me siento mareado. " : "Comprueba si responde. ")+
                (Progress.Breathing ? "Respira normalmente. " : "Observa respiración. ")+
                (Progress.Stage==TrainingStage.Resultados ? "Ayuda solicitada. Permanece junto a la persona; no se inició RCP." : Progress.MissingStep)
                : "CASO B · Otro adulto está recostado en la sala de descanso. Toca ABRIR CASO B para observarlo.";
            if(phoneText) phoneText.text=content.emergencyLabel+"\n"+CurrentLocation+"\nUbicación: "+(Progress.Location?"indicada":"pendiente")+" · Situación: "+(Progress.Situation?"descrita":"pendiente")+"\n"+(Progress.Responding?"Pide ayuda y permanece junto a la persona.":"Testigo con DEA: "+(Progress.Aed?"solicitado":"pendiente"))+"\nInteracción interna del juego: no realiza llamadas.";
        }
    }
}

