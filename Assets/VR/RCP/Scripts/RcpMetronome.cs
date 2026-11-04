using System;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace HospitalVR.RCP
{
    public enum RcpHapticHand { Izquierdo, Derecho, Ambos }
    [RequireComponent(typeof(AudioSource))]
    public sealed class RcpMetronome : MonoBehaviour
    {
        public RcpSession session;
        public AudioClip clip;
        [Range(60f, 140f)] public float beatsPerMinute = 110f;
        public bool isOn = true;
        public bool audioOn = true, hapticsOn = true, visualOn = true;
        [Range(0f, 1f)] public float volume = .5f;
        [Range(.05f, .5f)] public float volumeStep = .1f;
        [Range(0f, 1f)] public float hapticAmplitude = .25f;
        [Range(.01f, .1f)] public float hapticDuration = .045f;
        public RcpHapticHand hapticHand = RcpHapticHand.Derecho;
        public HapticImpulsePlayer leftHaptics, rightHaptics;
        public Transform visualIndicator;
        public TMP_Text channelLabel;
        public bool practiceAreaPresent = true;
        /// <summary>Despachos solicitados, incluso sin hardware; no demuestra vibración física.</summary>
        public int HapticDispatchCount { get; private set; }
        public int HapticSuccessCount { get; private set; }
        public int BeatCount { get; private set; }
        public event Action<double> BeatDispatched;
        public event Action<RcpHapticHand, float, float> HapticDispatched;
        readonly RcpBeatClock clock = new RcpBeatClock();
        AudioSource source;
        double scheduledBeat = -1;
        float visualUntil, runningBpm;
        Vector3 restScale;
        bool focused = true;
        void Awake()
        {
            source = GetComponent<AudioSource>(); source.playOnAwake = false; source.loop = false;
            if (visualIndicator) restScale = visualIndicator.localScale;
        }
        void OnEnable() { if (session) session.StateChanged += OnSessionState; }
        void OnDisable() { if (session) session.StateChanged -= OnSessionState; StopClock(); }
        void OnApplicationFocus(bool value) { focused = value; if (!value) StopClock(); }
        void OnApplicationPause(bool value) { if (value) StopClock(); }
        void OnSessionState(RcpSessionState value) { if (value != RcpSessionState.Activa) StopClock(); }
        void Update()
        {
            bool active = isOn && focused && practiceAreaPresent && session && session.isActiveAndEnabled && session.State == RcpSessionState.Activa;
            if (!active) { StopClock(); RefreshLabel(); return; }
            double now = AudioSettings.dspTime;
            if (!clock.Running || !Mathf.Approximately(runningBpm, beatsPerMinute))
            {
                StopClock(); clock.Start(now, beatsPerMinute); runningBpm = beatsPerMinute;
            }
            double due = clock.NextBeat;
            if (clock.Tick(now))
            {
                BeatCount++;
                if (hapticsOn)
                {
                    HapticDispatchCount++;
                    HapticDispatched?.Invoke(hapticHand, hapticAmplitude, hapticDuration);
                    if (hapticHand != RcpHapticHand.Derecho) Send(leftHaptics);
                    if (hapticHand != RcpHapticHand.Izquierdo) Send(rightHaptics);
                }
                if (visualOn) visualUntil = Time.unscaledTime + .12f;
                BeatDispatched?.Invoke(due);
            }
            if (!audioOn || !clip)
            {
                if (scheduledBeat >= 0) source.Stop(); scheduledBeat = -1;
            }
            else if (clock.NextBeat != scheduledBeat && clock.NextBeat - now <= .1)
            {
                source.clip = clip; source.volume = volume;
                source.PlayScheduled(clock.NextBeat); scheduledBeat = clock.NextBeat;
            }
            if (visualIndicator)
            {
                visualIndicator.gameObject.SetActive(visualOn);
                visualIndicator.localScale = restScale * (1f + .12f * Mathf.Clamp01((visualUntil - Time.unscaledTime) / .12f));
            }
            RefreshLabel();
        }
        void Send(HapticImpulsePlayer player)
        {
            // XRI resuelve los controles de la acción cada vez: admite desconexión y reconexión.
            if (player && player.SendHapticImpulse(Mathf.Clamp01(hapticAmplitude), hapticDuration)) HapticSuccessCount++;
        }
        public void StopClock()
        {
            clock.Stop(); scheduledBeat = -1; visualUntil = 0;
            if (source) source.Stop();
            if (visualIndicator) { visualIndicator.localScale = restScale; visualIndicator.gameObject.SetActive(false); }
        }
        public void Toggle() { isOn = !isOn; if (!isOn) StopClock(); else if (session && session.challenge) session.aidsUsed = true; }
        public void ToggleAudio() { audioOn = !audioOn; if (!audioOn && source) source.Stop(); }
        public void ToggleHaptics() => hapticsOn = !hapticsOn;
        public void ToggleVisual() { visualOn = !visualOn; if (!visualOn && visualIndicator) visualIndicator.gameObject.SetActive(false); }
        public void CycleHand() => hapticHand = (RcpHapticHand)(((int)hapticHand + 1) % 3);
        public void IntensityUp() => hapticAmplitude = Mathf.Clamp01(hapticAmplitude + .05f);
        public void IntensityDown() => hapticAmplitude = Mathf.Clamp01(hapticAmplitude - .05f);
        public void VolumeUp() => SetVolume(volume + volumeStep);
        public void VolumeDown() => SetVolume(volume - volumeStep);
        void SetVolume(float value) { volume = Mathf.Clamp01(value); if (source) source.volume = volume; }
        void RefreshLabel()
        {
            if (channelLabel) channelLabel.text = $"Audio {(audioOn ? "SÍ" : "NO")} · Vibración {(hapticsOn ? "SÍ" : "NO")} · Visual {(visualOn ? "SÍ" : "NO")}\nTouch: {hapticHand} · Intensidad {hapticAmplitude:P0}";
        }
    }
}
