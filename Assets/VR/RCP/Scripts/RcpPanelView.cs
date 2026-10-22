using TMPro;
using UnityEngine;

namespace HospitalVR.RCP
{
    /// <summary>
    /// Presentación de la práctica: pantalla de mundo, marcador del tórax y botones visibles.
    /// Refresca el texto a una frecuencia fija y solo cambia cadenas cuando cambia la situación.
    /// </summary>
    public sealed class RcpPanelView : MonoBehaviour
    {
        [Header("Modelo")]
        public RcpSession session;
        public RcpCompressionDetector detector;
        public RcpMetronome metronome;

        [Header("Pantalla")]
        public TMP_Text stateText;
        public TMP_Text metricsText;
        public TMP_Text hintText;
        public TMP_Text noteText;
        [Min(0.05f)] public float refreshInterval = 0.1f;

        [Header("Consola")]
        public GameObject startButton;
        public GameObject finishButton;
        public GameObject resetButton;
        public TMP_Text metronomeLabel;
        public TMP_Text volumeLabel;

        [Header("Marcador del tórax")]
        public Renderer chestMarker;
        public Color markerIdle = new Color(0.25f, 0.55f, 1f, 0.35f);
        public Color markerReady = new Color(0.2f, 0.85f, 0.35f, 0.5f);
        public Color markerPressed = new Color(1f, 0.85f, 0.2f, 0.65f);
        public Color markerWarning = new Color(1f, 0.45f, 0.1f, 0.5f);

        [Header("Sonidos")]
        public AudioSource feedbackSource;
        public AudioClip startClip;
        public AudioClip finishClip;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        MaterialPropertyBlock block;
        Color currentMarkerColor;
        float nextRefresh;
        int hintId = -1;
        bool metronomeShown;
        float volumeShown = -1f;

        const string Note = "Simulación educativa sin resistencia física: el movimiento del mando no mide profundidad ni fuerza reales. No es una evaluación clínica.";

        void OnEnable()
        {
            if (session != null) session.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            if (session != null) session.StateChanged -= OnStateChanged;
        }

        void Start()
        {
            block = new MaterialPropertyBlock();
            if (noteText != null) noteText.text = Note;
            currentMarkerColor = Color.clear;
            ApplyState(session != null ? session.State : RcpSessionState.Inactiva, false);
            RefreshMetronomeLabels(true);
        }

        void Update()
        {
            if (session == null) return;
            RefreshMetronomeLabels(false);
            UpdateMarker();

            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + refreshInterval;

            switch (session.State)
            {
                case RcpSessionState.Preparacion:
                    metricsText.SetText("Manos colocadas: {0:0}%", session.PreparationProgress * 100f);
                    SetHint(HintForPlacement());
                    break;
                case RcpSessionState.Activa:
                    float rate = session.CurrentRate;
                    bool timed = session.sessionDuration > 0f;
                    if (rate > 0f)
                        metricsText.SetText(timed
                            ? "Tiempo: {0:0} s (quedan {1:0} s)\nCompresiones: {2:0}\nRitmo estimado: {3:0} /min"
                            : "Tiempo: {0:0} s\nCompresiones: {2:0}\nRitmo estimado: {3:0} /min",
                            session.Elapsed, session.Remaining, session.Compressions, rate);
                    else
                        metricsText.SetText(timed
                            ? "Tiempo: {0:0} s (quedan {1:0} s)\nCompresiones: {2:0}\nRitmo estimado: --"
                            : "Tiempo: {0:0} s\nCompresiones: {2:0}\nRitmo estimado: --",
                            session.Elapsed, session.Remaining, session.Compressions);
                    SetHint(HintForActive(rate));
                    break;
            }
        }

        void OnStateChanged(RcpSessionState state) => ApplyState(state, true);

        void ApplyState(RcpSessionState state, bool playSound)
        {
            if (startButton != null) startButton.SetActive(state == RcpSessionState.Inactiva);
            if (finishButton != null) finishButton.SetActive(state == RcpSessionState.Preparacion || state == RcpSessionState.Activa);
            if (resetButton != null) resetButton.SetActive(state == RcpSessionState.Finalizada);
            hintId = -1;
            nextRefresh = 0f;

            switch (state)
            {
                case RcpSessionState.Inactiva:
                    stateText.text = "Práctica de RCP";
                    metricsText.text = "Toca INICIAR en la consola con la mano.\nNo hace falta pulsar botones del mando.";
                    hintText.text = "Colócate en la zona azul marcada junto a la camilla.";
                    break;
                case RcpSessionState.Preparacion:
                    stateText.text = "Preparación";
                    hintText.text = "";
                    if (playSound) Play(startClip);
                    break;
                case RcpSessionState.Activa:
                    stateText.text = "Sesión activa";
                    if (playSound) Play(startClip);
                    break;
                case RcpSessionState.Finalizada:
                    stateText.text = "Resultados";
                    ShowResults(session.Results);
                    if (playSound) Play(finishClip);
                    break;
            }
        }

        void ShowResults(RcpResults r)
        {
            string ritmo = r.meanRate > 0f ? r.meanRate.ToString("0") + " /min" : "--";
            string rango = r.ratedCompressions > 0 ? (100f * r.inTargetRate / r.ratedCompressions).ToString("0") + " %" : "--";
            string retorno = r.evaluatedReturns > 0 ? (100f * r.fullReturns / r.evaluatedReturns).ToString("0") + " %" : "--";
            metricsText.text =
                "Duración: " + r.duration.ToString("0") + " s   Compresiones: " + r.compressions +
                "\nRitmo medio: " + ritmo + "   En 100-120/min: " + rango +
                "\nRetorno completo del mando: " + retorno +
                "\nPausa más larga: " + r.longestPause.ToString("0.0") + " s   Manos fuera: " + r.handsOffTime.ToString("0.0") + " s";
            hintText.text = SummaryHint(r);
        }

        static string SummaryHint(RcpResults r)
        {
            if (r.compressions == 0) return "No se detectaron compresiones. Toca REINICIAR para intentarlo de nuevo.";
            if (r.meanRate > 0f && r.meanRate < 100f) return "Ritmo medio bajo: practica con el metrónomo (110/min). Toca REINICIAR.";
            if (r.meanRate > 120f) return "Ritmo medio alto: reduce la velocidad. Toca REINICIAR.";
            if (r.longestPause > 10f) return "Hubo pausas largas: minimiza las interrupciones. Toca REINICIAR.";
            return "Buen trabajo. Recuerda dejar volver el pecho en cada ciclo. Toca REINICIAR.";
        }

        int HintForPlacement()
        {
            switch (detector.Status)
            {
                case RcpHandStatus.EnZona: return 0;
                case RcpHandStatus.ManosSeparadas: return 1;
                case RcpHandStatus.SinSeguimiento: return 2;
                case RcpHandStatus.Desplazandose: return 3;
                default: return 4;
            }
        }

        int HintForActive(float rate)
        {
            if (detector.Status != RcpHandStatus.EnZona) return HintForPlacement();
            if (!detector.LastReturnComplete) return 5;
            if (rate <= 0f) return 6;
            if (rate < session.targetRateMin) return 7;
            if (rate > session.targetRateMax) return 8;
            return 9;
        }

        static readonly string[] Hints =
        {
            "Bien colocadas. Mantén las manos sobre el marcador.",
            "Junta las manos: una encima de la otra, en el centro del pecho.",
            "Sin seguimiento de los mandos: mantenlos visibles para los sensores.",
            "Detente junto al paciente: no se cuenta mientras te desplazas o giras.",
            "Coloca ambas manos sobre el marcador azul (mitad inferior del esternón).",
            "Retorno incompleto: sube las manos del todo antes de la siguiente compresión.",
            "Comprime y deja que el pecho vuelva por completo.",
            "Más rápido: el objetivo es 100-120 por minuto.",
            "Más despacio: el objetivo es 100-120 por minuto.",
            "Buen ritmo. Sigue así y deja volver el pecho."
        };

        void SetHint(int id)
        {
            if (id == hintId) return;
            hintId = id;
            hintText.text = Hints[id];
        }

        void RefreshMetronomeLabels(bool force)
        {
            if (metronome == null) return;
            if (force || metronome.isOn != metronomeShown)
            {
                metronomeShown = metronome.isOn;
                if (metronomeLabel != null) metronomeLabel.text = metronomeShown ? "METRÓNOMO\nSÍ" : "METRÓNOMO\nNO";
            }
            if (force || !Mathf.Approximately(metronome.volume, volumeShown))
            {
                volumeShown = metronome.volume;
                if (volumeLabel != null) volumeLabel.SetText("VOLUMEN\n{0:0}%", volumeShown * 100f);
            }
        }

        void UpdateMarker()
        {
            if (chestMarker == null || detector == null) return;
            bool visible = session.State == RcpSessionState.Preparacion || session.State == RcpSessionState.Activa;
            if (chestMarker.enabled != visible) chestMarker.enabled = visible;
            if (!visible) return;

            Color target = markerIdle;
            if (detector.Status == RcpHandStatus.EnZona) target = detector.IsPressed ? markerPressed : markerReady;
            else if (detector.Status == RcpHandStatus.ManosSeparadas || detector.Status == RcpHandStatus.Desplazandose) target = markerWarning;

            if (target == currentMarkerColor) return;
            currentMarkerColor = target;
            block.SetColor(BaseColorId, target);
            chestMarker.SetPropertyBlock(block);
        }

        void Play(AudioClip clip)
        {
            if (feedbackSource != null && clip != null) feedbackSource.PlayOneShot(clip);
        }
    }
}
