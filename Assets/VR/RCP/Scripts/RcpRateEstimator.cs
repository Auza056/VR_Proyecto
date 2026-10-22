using UnityEngine;

namespace HospitalVR.RCP
{
    /// <summary>
    /// Estima el ritmo con las últimas compresiones. Usa un búfer circular fijo, sin asignaciones por frame.
    /// </summary>
    public sealed class RcpRateEstimator
    {
        readonly float[] times;
        int count, next;

        public RcpRateEstimator(int capacity = 16)
        {
            times = new float[Mathf.Max(3, capacity)];
        }

        public void Clear()
        {
            count = 0;
            next = 0;
        }

        public void Add(float time)
        {
            times[next] = time;
            next = (next + 1) % times.Length;
            if (count < times.Length) count++;
        }

        /// <summary>
        /// Compresiones por minuto con hasta <paramref name="window"/> marcas recientes.
        /// Devuelve 0 si hay menos de tres o si la última es más antigua que <paramref name="maxAge"/>.
        /// </summary>
        public float Rate(float now, int window, float maxAge)
        {
            int n = Mathf.Min(Mathf.Max(3, window), count);
            if (n < 3) return 0f;
            float last = times[(next - 1 + times.Length) % times.Length];
            if (now - last > maxAge) return 0f;
            float first = times[(next - n + times.Length) % times.Length];
            float span = last - first;
            return span > 0f ? (n - 1) * 60f / span : 0f;
        }
    }
}
