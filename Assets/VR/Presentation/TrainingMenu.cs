using HospitalVR.Interaction;
using HospitalVR.Learning;
using HospitalVR.RCP;
using TMPro;
using UnityEngine;

namespace HospitalVR.Presentation
{
    /// <summary>Presentación de la demo; usa los mismos eventos que los botones de la estación.</summary>
    public sealed class TrainingMenu : MonoBehaviour
    {
        public TrainingFlow flow;
        public const int HomePage=0, SettingsPage=1, ResultsPage=2, MainPage=3, HelpPage=4;
        public GameObject homePage, settingsPage, resultsPage, mainPage, helpPage;
        [Tooltip("Pestañas, progreso y botones inferiores: se ocultan en la portada.")]
        public GameObject chrome;
        public PlayerHeightCalibrator height;
        public TMP_Text subtitle, settingsText, practiceText, challengeText, comparisonText, messageText;
        public TMP_Text[] steps;
        public TrainingMenuButton[] buttons;
        public Color readyColor=new Color(.11f,.36f,.42f), mutedColor=new Color(.09f,.14f,.19f);
        public Color currentColor=new Color(.18f,.77f,.71f);
        public Color primaryColor=new Color(.12f,.56f,.52f);
        public bool startOnMainPage=true;
        public int Page {get;private set;}
        void Awake() { if(startOnMainPage) Page=MainPage; }
        float nextRefresh, resetUntil;
        string notice;
        RcpSession Session=>flow.session;
        bool Running=>Session.State==RcpSessionState.Activa || Session.State==RcpSessionState.Preparacion;
        public bool CanChangeDuration=>!Running && !flow.HasPracticeResults && !flow.HasChallengeResults;
        void Start()=>RefreshNow();
        void Update() { if(Time.unscaledTime>=nextRefresh) {nextRefresh=Time.unscaledTime+.15f; RefreshNow();} }
        public bool CanExecute(MenuCommand command)
        {
            switch(command)
            {
                case MenuCommand.Challenge: return flow.Progress.Stage==TrainingStage.Reto && !flow.Progress.ChallengeRunning && !Running;
                case MenuCommand.StartCase: return !Running;
                case MenuCommand.Practice: return !Running;
                case MenuCommand.Finish: return Running;
                case MenuCommand.Duration30:
                case MenuCommand.Duration60: return CanChangeDuration;
                default: return true;
            }
        }
        public void Execute(MenuCommand command)
        {
            if(!CanExecute(command)) return;
            if(command!=MenuCommand.Reset) resetUntil=0;
            notice=null;
            switch(command)
            {
                case MenuCommand.Home: Page=HomePage; break;
                case MenuCommand.MainMenu: Page=MainPage; break;
                case MenuCommand.HowTo: Page=HelpPage; break;
                case MenuCommand.BeginTour: Page=HomePage; notice="Paso 1: lee el manual junto a la camilla. Después toca INICIAR CASO."; break;
                case MenuCommand.HeightUp: if(height) height.Raise(); break;
                case MenuCommand.HeightDown: if(height) height.Lower(); break;
                case MenuCommand.HeightReset: if(height) height.ResetAdjust(); notice="Altura real restablecida."; break;
                case MenuCommand.Recenter: if(height) height.Recenter(); notice="Vista centrada frente al menú."; break;
                case MenuCommand.Quit:
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying=false;
#else
                    Application.Quit();
#endif
                    break;
                case MenuCommand.Settings: Page=SettingsPage; break;
                case MenuCommand.Results: Page=ResultsPage; break;
                case MenuCommand.StartCase: flow.BeginCase(); notice="Ve a la recepción del pasillo. Empieza por observar el entorno."; break;
                case MenuCommand.Practice: flow.DirectPractice(); Page=HomePage; notice="Colócate junto a la camilla, frente a la zona azul del tórax."; break;
                case MenuCommand.Challenge: flow.StartChallenge(); notice="Reto preparado. Mantén las manos en la zona para empezar."; break;
                case MenuCommand.Reset:
                    if(Time.unscaledTime>resetUntil || resetUntil==0) {resetUntil=Time.unscaledTime+4f; notice="Vuelve a pulsar REINICIAR para confirmar.";}
                    else {flow.ResetCase(); resetUntil=0; Page=MainPage; if(height){height.ResetAdjust(); height.Recenter();} notice="Todo listo para un nuevo participante.";}
                    break;
                case MenuCommand.RecoverManual: flow.manual.Recover(); notice="El manual vuelve al atril al estar suelto."; break;
                case MenuCommand.Finish: Session.Finish(); Page=ResultsPage; break;
                case MenuCommand.Duration30: Session.sessionDuration=30; notice="Demo breve: práctica y reto de 30 s cada uno."; break;
                case MenuCommand.Duration60: Session.sessionDuration=60; notice="Práctica y reto de 60 s cada uno."; break;
                case MenuCommand.Audio: flow.metronome.ToggleAudio(); break;
                case MenuCommand.Haptics: flow.metronome.ToggleHaptics(); break;
                case MenuCommand.Visual: flow.metronome.ToggleVisual(); break;
                case MenuCommand.Hand: flow.metronome.CycleHand(); break;
                case MenuCommand.IntensityDown: flow.metronome.IntensityDown(); break;
                case MenuCommand.IntensityUp: flow.metronome.IntensityUp(); break;
                case MenuCommand.VolumeDown: flow.metronome.VolumeDown(); break;
                case MenuCommand.VolumeUp: flow.metronome.VolumeUp(); break;
                case MenuCommand.Metronome: flow.metronome.Toggle(); break;
                case MenuCommand.Marker: flow.ToggleMarker(); break;
            }
            RefreshNow();
        }
        static string Rate(RcpResults r)=>r.meanRate>0 ? r.meanRate.ToString("0")+" /min" : "Datos insuficientes";
        static string Percent(int n,int d)=>d>0 ? (100f*n/d).ToString("0")+"% ("+n+"/"+d+")" : "Datos insuficientes";
        public static string Report(RcpResults r)
        {
            return (string.IsNullOrEmpty(r.stopReason)?"Intento finalizado":"Interrumpido: "+r.stopReason)+
                "\nDuración  "+r.duration.ToString("0")+" s\nDetectadas  "+r.compressions+
                "\nRitmo  "+Rate(r)+"\nEn 100–120/min  "+Percent(r.inTargetRate,r.ratedCompressions)+
                "\nRetorno del mando  "+Percent(r.fullReturns,r.evaluatedReturns)+
                "\nPausa mayor  "+r.longestPause.ToString("0.0")+" s\nSeguimiento perdido  "+r.trackingLostTime.ToString("0.0")+" s"+
                "\nAyudas  "+(r.aidsUsed?"SÍ":"NO");
        }
        public void RefreshNow()
        {
            if(!flow) return;
            if(homePage) homePage.SetActive(Page==HomePage);
            if(settingsPage) settingsPage.SetActive(Page==SettingsPage);
            if(resultsPage) resultsPage.SetActive(Page==ResultsPage);
            if(mainPage) mainPage.SetActive(Page==MainPage);
            if(helpPage) helpPage.SetActive(Page==HelpPage);
            if(chrome) chrome.SetActive(Page!=MainPage);
            if(subtitle) subtitle.text=Page switch {HomePage=>"RECORRIDO", SettingsPage=>"AJUSTES", ResultsPage=>"RESULTADOS", HelpPage=>"CÓMO SE JUEGA", _=>""};
            if(settingsText) settingsText.text=$"<color=#9FB3C8>Intento</color> {Session.sessionDuration:0} s   <color=#9FB3C8>Ritmo</color> {flow.metronome.beatsPerMinute:0}/min   <color=#9FB3C8>Volumen</color> {flow.metronome.volume:P0}   <color=#9FB3C8>Vibración</color> {flow.metronome.hapticAmplitude:P0} · {flow.metronome.hapticHand}"+(height?"\n<color=#9FB3C8>"+height.Describe()+"</color>":"");
            if(practiceText) practiceText.text=flow.HasPracticeResults ? Report(flow.PracticeResults) : "Tu práctica aparecerá aquí.\n\nEmpieza desde RECORRIDO o completa el caso de recepción.";
            if(challengeText) challengeText.text=flow.HasChallengeResults ? Report(flow.ChallengeResults) : "Tu reto aparecerá aquí.\n\nTermina la práctica para desbloquearlo. Se inicia sin ayudas.";
            if(comparisonText) comparisonText.text=flow.HasPracticeResults && flow.HasChallengeResults
                ? Mathf.Abs(flow.PracticeResults.duration-flow.ChallengeResults.duration)>2f
                    ? "Los tiempos observados son diferentes. Repite ambos intentos completos para comparar."
                    : "Compara ritmo, pausas y retorno. Las interrupciones o ayudas cambian las condiciones del intento."
                : "Los porcentajes usan solo muestras evaluables. No se calcula una calificación clínica.";
            if(resetUntil>0 && Time.unscaledTime>resetUntil) {resetUntil=0;notice=null;}
            if(messageText) messageText.text=notice ?? (CanChangeDuration ? "Apunta y pulsa el gatillo, o toca con la mano." : "Duración fijada para comparar intentos. Reinicia el recorrido para cambiarla.");
            int stage=(int)flow.Progress.Stage;
            for(int i=0;i<steps.Length;i++) if(steps[i])
            {
                string[] names={"MANUAL","RECONOCER","AYUDA","PRÁCTICA","RETO","RESULTADOS"};
                steps[i].text=(i+1).ToString("00")+"\n"+names[i];
                steps[i].color=i==stage?new Color(.35f,1f,.84f):i<stage?new Color(.80f,.90f,.95f):new Color(.45f,.55f,.62f);
                var dot=steps[i].transform.parent ? steps[i].transform.parent.Find("Punto"+i) : null;
                if(dot && dot.TryGetComponent<SpriteRenderer>(out var sr)) sr.color=i==stage?currentColor:i<stage?primaryColor:mutedColor;
            }
            foreach(var b in buttons)
            {
                if(!b || !b.touch) continue;
                b.touch.interactable=CanExecute(b.command);
                bool selected=b.command==MenuCommand.Home && Page==HomePage || b.command==MenuCommand.Settings && Page==SettingsPage || b.command==MenuCommand.Results && Page==ResultsPage || b.command==MenuCommand.HowTo && Page==HelpPage;
                bool primary=b.command==MenuCommand.BeginTour || b.command==MenuCommand.StartCase || b.command==MenuCommand.Practice || b.command==MenuCommand.Challenge;
                b.touch.normalColor=selected?currentColor:primary?primaryColor:readyColor;
                b.touch.disabledColor=mutedColor;
                if(b.label)
                {
                    string state=b.command switch
                    {
                        MenuCommand.Audio=>flow.metronome.audioOn?"SÍ":"NO",
                        MenuCommand.Haptics=>flow.metronome.hapticsOn?"SÍ":"NO",
                        MenuCommand.Visual=>flow.metronome.visualOn?"SÍ":"NO",
                        MenuCommand.Metronome=>flow.metronome.isOn?"SÍ":"NO",
                        MenuCommand.Marker=>flow.rcpPanel.markerAid?"SÍ":"NO", _=>null
                    };
                    b.label.text=b.command==MenuCommand.Reset && Time.unscaledTime<resetUntil ? "CONFIRMAR REINICIO" : b.caption+(state!=null ? " · "+state : "");
                    b.label.color=b.touch.interactable?Color.white:new Color(.48f,.57f,.62f);
                }
            }
        }
    }
}
