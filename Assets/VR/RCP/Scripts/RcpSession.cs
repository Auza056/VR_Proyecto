using System;
using UnityEngine;

namespace HospitalVR.RCP
{
    public enum RcpSessionState
    {
        Inactiva,
        Preparacion,
        Activa,
        Finalizada
    }

    public struct RcpResults
    {
        public float duration;
        public int compressions;
        public float meanRate;
        public int ratedCompressions;
        public int inTargetRate;
        public int evaluatedReturns;
        public int fullReturns;
        public float longestPause;
        public float handsOffTime;
        public float trackingLostTime;
        public int trackingInterruptions;
        public bool challenge;
        public bool aidsUsed;
        public string stopReason;
    }

    /// <summary>
    /// Estado de la práctica y métricas. No lee entradas ni dibuja nada: recibe eventos del detector
    /// y expone datos para la presentación.
    /// </summary>
    public sealed class RcpSession : MonoBehaviour
    {
        public RcpCompressionDetector detector;
        public bool challenge;
        public bool aidsUsed;

        [Header("Flujo")]
        [Tooltip("Tiempo que las manos deben permanecer bien colocadas para empezar.")]
        [Min(0.2f)] public float preparationHoldTime = 1.5f;
        [Tooltip("Duración de la sesión en segundos. 0 = sin límite (se termina con Finalizar).")]
        [Min(0f)] public float sessionDuration = 60f;

        [Header("Ritmo")]
        [Tooltip("Número de compresiones recientes usadas para estimar el ritmo.")]
        [Range(3, 16)] public int rateWindow = 6;
        [Tooltip("Si no hay compresiones en este tiempo, el ritmo se muestra como no disponible.")]
        [Min(0.5f)] public float rateMaxAge = 2.5f;
        [Tooltip("Criterio educativo de ritmo (ERC 2025 / AHA 2025: 100-120 por minuto).")]
        public float targetRateMin = 100f;
        public float targetRateMax = 120f;

        public event Action<RcpSessionState> StateChanged;
        public event Action CompressionCounted;

        public RcpSessionState State { get; private set; } = RcpSessionState.Inactiva;
        public float Elapsed => State == RcpSessionState.Activa ? Time.time - startTime : results.duration;
        public float Remaining => sessionDuration > 0f ? Mathf.Max(0f, sessionDuration - Elapsed) : 0f;
        public int Compressions { get; private set; }
        public float PreparationProgress => State == RcpSessionState.Preparacion ? Mathf.Clamp01(holdTimer / preparationHoldTime) : 0f;
        public float CurrentRate => State == RcpSessionState.Activa ? rate.Rate(Time.time, rateWindow, rateMaxAge) : 0f;
        public RcpResults Results => results;

        readonly RcpRateEstimator rate = new RcpRateEstimator();
        RcpResults results;
        float startTime, holdTimer, lastCompressionTime, intervalSum;
        int intervalCount;
        bool trackingWasLost;
        bool continuousInterval;

        void Awake()
        {
            if (detector != null) detector.enabled = false;
        }

        void OnEnable()
        {
            if (detector == null) return;
            detector.CompressionDetected += OnCompression;
            detector.ReturnEvaluated += OnReturn;
        }

        void OnDisable()
        {
            if (State == RcpSessionState.Activa || State == RcpSessionState.Preparacion) Suspend("Estación desactivada");
            if (detector == null) return;
            detector.CompressionDetected -= OnCompression;
            detector.ReturnEvaluated -= OnReturn;
        }

        public void StartPractice()
        {
            if (State != RcpSessionState.Inactiva) return;
            EnterPreparation();
        }

        public void Finish()
        {
            if (State == RcpSessionState.Preparacion)
            {
                SetState(RcpSessionState.Inactiva);
                return;
            }
            if (State != RcpSessionState.Activa) return;

            float now = Time.time;
            if (detector != null) detector.CompletePendingReturn();
            results.duration = now - startTime;
            results.compressions = Compressions;
            results.meanRate = intervalCount >= 2 && intervalSum > 0f ? intervalCount * 60f / intervalSum : 0f;
            results.challenge = challenge;
            results.aidsUsed = aidsUsed;
            results.longestPause = Mathf.Max(results.longestPause, now - lastCompressionTime);
            SetState(RcpSessionState.Finalizada);
        }

        public void ResetPractice()
        {
            if (State == RcpSessionState.Activa) Finish();
            EnterPreparation();
        }

        void EnterPreparation()
        {
            results = default;
            Compressions = 0;
            intervalSum = 0f;
            intervalCount = 0;
            continuousInterval = false;
            trackingWasLost = false;
            holdTimer = 0f;
            rate.Clear();
            SetState(RcpSessionState.Preparacion);
        }

        void Update()
        {
            if (detector == null) return;
            bool handsReady = detector.Status == RcpHandStatus.EnZona;

            if (State == RcpSessionState.Preparacion)
            {
                holdTimer = handsReady ? holdTimer + Time.deltaTime : 0f;
                if (holdTimer >= preparationHoldTime)
                {
                    startTime = Time.time;
                    lastCompressionTime = startTime;
                    SetState(RcpSessionState.Activa);
                }
            }
            else if (State == RcpSessionState.Activa)
            {
                bool trackingLost = detector.Status == RcpHandStatus.SinSeguimiento;
                if (trackingLost)
                {
                    results.trackingLostTime += Time.deltaTime;
                    if (!trackingWasLost) results.trackingInterruptions++;
                    rate.Clear(); continuousInterval = false;
                }
                else if (!handsReady) results.handsOffTime += Time.deltaTime;
                trackingWasLost = trackingLost;
                if (sessionDuration > 0f && Time.time - startTime >= sessionDuration) Finish();
            }
        }

        void OnCompression(RcpCompression compression)
        {
            if (State != RcpSessionState.Activa) return;

            float gap = compression.time - lastCompressionTime;
            if (gap > results.longestPause) results.longestPause = gap;
            // Solo los intervalos continuos (sin pausas largas) cuentan para el ritmo medio.
            if (continuousInterval && Compressions > 0 && gap <= rateMaxAge)
            {
                intervalSum += gap;
                intervalCount++;
            }
            lastCompressionTime = compression.time;
            continuousInterval = true;
            Compressions++;
            rate.Add(compression.time);

            float current = rate.Rate(compression.time, rateWindow, rateMaxAge);
            if (current > 0f)
            {
                results.ratedCompressions++;
                if (current >= targetRateMin && current <= targetRateMax) results.inTargetRate++;
            }
            CompressionCounted?.Invoke();
        }

        void OnReturn(bool complete)
        {
            if (State != RcpSessionState.Activa) return;
            results.evaluatedReturns++;
            if (complete) results.fullReturns++;
        }

        void SetState(RcpSessionState next)
        {
            State = next;
            if (detector != null) detector.enabled = next == RcpSessionState.Preparacion || next == RcpSessionState.Activa;
            StateChanged?.Invoke(next);
        }

        public void Suspend(string reason)
        {
            if (State == RcpSessionState.Activa) { results.stopReason = reason; Finish(); }
            else if (State == RcpSessionState.Preparacion) SetState(RcpSessionState.Inactiva);
        }
        public void Cancel()
        {
            if (State == RcpSessionState.Activa) Suspend("Intento cancelado");
            SetState(RcpSessionState.Inactiva);
            results = default; Compressions = 0; rate.Clear();
        }
        void OnApplicationFocus(bool focused) { if (!focused) Suspend("Se perdió el foco de la aplicación"); }
        void OnApplicationPause(bool paused) { if (paused) Suspend("Aplicación en pausa"); }
    }
}
