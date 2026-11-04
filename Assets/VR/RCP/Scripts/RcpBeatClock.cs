using System;
namespace HospitalVR.RCP
{
    /// <summary>Calendario compartido; descarta pulsos atrasados más de 120 ms.</summary>
    public sealed class RcpBeatClock
    {
        public double NextBeat { get; private set; }
        public bool Running { get; private set; }
        double interval;
        public void Start(double now, double bpm)
        {
            interval = 60.0 / Math.Max(60.0, Math.Min(140.0, bpm));
            NextBeat = now + .05; Running = true;
        }
        public void Stop() { Running = false; NextBeat = 0; }
        public bool Tick(double now)
        {
            if (!Running || now < NextBeat) return false;
            if (now - NextBeat > .12)
            {
                NextBeat += (Math.Floor((now - NextBeat) / interval) + 1) * interval;
                return false;
            }
            NextBeat += interval; return true;
        }
    }
}
