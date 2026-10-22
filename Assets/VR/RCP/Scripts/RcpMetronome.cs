using UnityEngine;

namespace HospitalVR.RCP
{
    /// <summary>
    /// Metrónomo opcional con su propia fuente de audio, independiente del ambiente.
    /// Programa cada pulso con el reloj DSP para mantener un intervalo estable.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class RcpMetronome : MonoBehaviour
    {
        public RcpSession session;
        public AudioClip clip;
        [Range(60f, 140f)] public float beatsPerMinute = 110f;
        public bool isOn = true;
        [Range(0f, 1f)] public float volume = 0.5f;
        [Range(0.05f, 0.5f)] public float volumeStep = 0.1f;

        AudioSource source;
        double nextBeat;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.clip = clip;
            source.volume = volume;
        }

        void Update()
        {
            bool shouldPlay = isOn && clip != null && session != null && session.State == RcpSessionState.Activa;
            if (!shouldPlay)
            {
                nextBeat = 0;
                return;
            }

            double dsp = AudioSettings.dspTime;
            if (nextBeat < dsp) nextBeat = dsp + 0.05;
            if (nextBeat - dsp < 0.1)
            {
                source.PlayScheduled(nextBeat);
                nextBeat += 60.0 / beatsPerMinute;
            }
        }

        public void Toggle()
        {
            isOn = !isOn;
            if (!isOn) source.Stop();
        }

        public void VolumeUp() => SetVolume(volume + volumeStep);

        public void VolumeDown() => SetVolume(volume - volumeStep);

        void SetVolume(float value)
        {
            volume = Mathf.Clamp01(Mathf.Round(value * 100f) / 100f);
            source.volume = volume;
        }
    }
}
