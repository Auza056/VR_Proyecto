using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace HospitalVR.RCP
{
    public enum RcpHandStatus
    {
        SinSeguimiento,
        FueraDeZona,
        ManosSeparadas,
        Desplazandose,
        EnZona
    }

    public struct RcpCompression
    {
        public float time;
        /// <summary>Recorrido vertical del mando en metros. No es profundidad torácica real.</summary>
        public float controllerTravel;
    }

    /// <summary>
    /// Detecta ciclos de descenso y retorno de las manos sobre el tórax usando las poses de los
    /// mandos ya seguidos por el XR Origin. No lee botones ni modifica el jugador.
    /// Todas las medidas se expresan en el marco del tórax, por lo que desplazar el jugador no
    /// altera la altura relativa; además, se suspende la detección mientras el rig se desplaza o gira.
    /// </summary>
    public sealed class RcpCompressionDetector : MonoBehaviour
    {
        enum Phase { Arriba, Abajo }

        [Header("Referencias")]
        public RcpChestReference chest;
        public Transform leftHand;
        public Transform rightHand;
        [Tooltip("Opcional. Si el driver expone estado de seguimiento, se descartan las muestras sin posición válida.")]
        public TrackedPoseDriver leftPose;
        public TrackedPoseDriver rightPose;
        [Tooltip("Raíz del jugador (XR Origin). Solo se lee para suspender la detección mientras se desplaza.")]
        public Transform rigRoot;
        [Tooltip("Punto de contacto respecto al mando, en su espacio local (metros).")]
        public Vector3 contactOffset = Vector3.zero;

        [Header("Colocación")]
        public bool requireBothHands = true;
        [Tooltip("Distancia máxima entre ambos puntos de contacto para considerarlas superpuestas.")]
        [Min(0.05f)] public float maxHandSeparation = 0.2f;

        [Header("Filtrado")]
        [Tooltip("Frecuencia de corte del filtro paso bajo aplicado a la altura de las manos.")]
        [Min(1f)] public float smoothingCutoffHz = 10f;
        [Tooltip("Velocidad máxima plausible de la mano respecto al tórax. Saltos mayores se tratan como datos inválidos.")]
        [Min(0.5f)] public float maxHandSpeed = 3f;

        [Header("Ciclo de compresión (desplazamiento del mando)")]
        [Tooltip("Descenso mínimo desde el punto alto para iniciar una compresión.")]
        [Min(0.005f)] public float pressThreshold = 0.035f;
        [Tooltip("Subida mínima desde el punto bajo para cerrar el ciclo y contarlo (histéresis).")]
        [Min(0.005f)] public float releaseThreshold = 0.025f;
        [Tooltip("Margen respecto al punto alto anterior para considerar que el retorno fue completo.")]
        [Min(0f)] public float fullReturnTolerance = 0.012f;
        [Tooltip("Tiempo mínimo entre dos conteos. Evita duplicados por temblor.")]
        [Min(0.1f)] public float minCycleInterval = 0.27f;
        [Tooltip("Si las manos permanecen abajo más tiempo, el ciclo se descarta (apoyo sostenido).")]
        [Min(0.2f)] public float maxPressDuration = 1.5f;
        [Tooltip("Velocidad con la que la referencia alta sigue cambios lentos de postura (m/s).")]
        [Min(0f)] public float topDecayRate = 0.02f;
        [Tooltip("Tiempo tras una compresión para evaluar si el retorno fue completo.")]
        [Min(0.1f)] public float returnWindow = 0.9f;

        [Header("Desplazamiento del jugador")]
        [Min(0.01f)] public float maxRigSpeed = 0.15f;
        [Min(1f)] public float maxRigAngularSpeed = 20f;
        [Min(0f)] public float rigSettleTime = 0.5f;

        public event Action<RcpCompression> CompressionDetected;
        /// <summary>Se emite tras cada compresión indicando si el retorno fue completo.</summary>
        public event Action<bool> ReturnEvaluated;

        public RcpHandStatus Status { get; private set; } = RcpHandStatus.SinSeguimiento;
        public bool IsPressed => phase == Phase.Abajo;
        /// <summary>Descenso actual del mando respecto a la referencia alta, en metros.</summary>
        public float CurrentTravel { get; private set; }
        public float LastTravel { get; private set; }
        public bool LastReturnComplete { get; private set; } = true;

        Phase phase;
        bool filterReady;
        float filteredHeight, topHeight, bottomHeight, cycleTop, pressStartTime, lastCountTime = -10f;
        bool returnPending;
        float returnReference, returnPeak, releaseTime;

        bool hasRigSample;
        Vector3 lastRigPosition;
        float lastRigYaw, rigMovingUntil;

        bool leftHasPrev, rightHasPrev;
        Vector3 leftPrev, rightPrev;

        void OnDisable()
        {
            ResetCycle();
            hasRigSample = false;
            leftHasPrev = rightHasPrev = false;
            Status = RcpHandStatus.SinSeguimiento;
        }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f || chest == null) return;
            float now = Time.time;

            UpdateRigMotion(now, dt);

            bool leftOk = TrySample(leftHand, leftPose, dt, ref leftPrev, ref leftHasPrev, out Vector3 left);
            bool rightOk = TrySample(rightHand, rightPose, dt, ref rightPrev, ref rightHasPrev, out Vector3 right);

            RcpHandStatus status;
            Vector3 point = Vector3.zero;
            if (leftOk && rightOk)
            {
                point = (left + right) * 0.5f;
                status = (left - right).sqrMagnitude > maxHandSeparation * maxHandSeparation
                    ? RcpHandStatus.ManosSeparadas
                    : RcpHandStatus.EnZona;
            }
            else if (!requireBothHands && (leftOk || rightOk))
            {
                point = leftOk ? left : right;
                status = RcpHandStatus.EnZona;
            }
            else
            {
                status = RcpHandStatus.SinSeguimiento;
            }

            if (status == RcpHandStatus.EnZona && !chest.IsInside(point)) status = RcpHandStatus.FueraDeZona;
            if (status == RcpHandStatus.EnZona && now < rigMovingUntil) status = RcpHandStatus.Desplazandose;

            Status = status;
            if (status != RcpHandStatus.EnZona)
            {
                ResetCycle();
                return;
            }

            float alpha = 1f - Mathf.Exp(-2f * Mathf.PI * smoothingCutoffHz * dt);
            if (!filterReady)
            {
                filteredHeight = point.y;
                topHeight = filteredHeight;
                filterReady = true;
            }
            else
            {
                filteredHeight += alpha * (point.y - filteredHeight);
            }

            UpdateCycle(now, dt);
        }

        void UpdateCycle(float now, float dt)
        {
            if (phase == Phase.Arriba)
            {
                topHeight = filteredHeight > topHeight
                    ? filteredHeight
                    : Mathf.Max(filteredHeight, topHeight - topDecayRate * dt);

                if (returnPending)
                {
                    returnPeak = Mathf.Max(returnPeak, filteredHeight);
                    if (now - releaseTime >= returnWindow) FinishReturn();
                }

                CurrentTravel = Mathf.Max(0f, topHeight - filteredHeight);
                if (topHeight - filteredHeight >= pressThreshold)
                {
                    if (returnPending) FinishReturn();
                    phase = Phase.Abajo;
                    cycleTop = topHeight;
                    bottomHeight = filteredHeight;
                    pressStartTime = now;
                }
                return;
            }

            if (filteredHeight < bottomHeight) bottomHeight = filteredHeight;
            CurrentTravel = Mathf.Max(0f, cycleTop - filteredHeight);

            if (now - pressStartTime > maxPressDuration)
            {
                // Apoyo sostenido: no es un ciclo. Se toma la posición actual como nueva referencia.
                phase = Phase.Arriba;
                topHeight = filteredHeight;
                return;
            }

            if (filteredHeight - bottomHeight < releaseThreshold) return;

            phase = Phase.Arriba;
            topHeight = filteredHeight;
            if (now - lastCountTime < minCycleInterval) return;

            lastCountTime = now;
            LastTravel = cycleTop - bottomHeight;
            returnPending = true;
            returnReference = cycleTop;
            returnPeak = filteredHeight;
            releaseTime = now;
            CompressionDetected?.Invoke(new RcpCompression { time = now, controllerTravel = LastTravel });
        }

        void FinishReturn()
        {
            returnPending = false;
            LastReturnComplete = returnPeak >= returnReference - fullReturnTolerance;
            ReturnEvaluated?.Invoke(LastReturnComplete);
        }

        void ResetCycle()
        {
            phase = Phase.Arriba;
            filterReady = false;
            returnPending = false;
            CurrentTravel = 0f;
        }

        void UpdateRigMotion(float now, float dt)
        {
            if (rigRoot == null) return;
            Vector3 position = rigRoot.position;
            float yaw = rigRoot.eulerAngles.y;
            if (hasRigSample)
            {
                float speed = (position - lastRigPosition).magnitude / dt;
                float angularSpeed = Mathf.Abs(Mathf.DeltaAngle(lastRigYaw, yaw)) / dt;
                if (speed > maxRigSpeed || angularSpeed > maxRigAngularSpeed) rigMovingUntil = now + rigSettleTime;
            }
            lastRigPosition = position;
            lastRigYaw = yaw;
            hasRigSample = true;
        }

        bool TrySample(Transform hand, TrackedPoseDriver pose, float dt, ref Vector3 previous, ref bool hasPrevious, out Vector3 local)
        {
            local = Vector3.zero;
            if (hand == null || !hand.gameObject.activeInHierarchy || !HasPositionTracking(pose))
            {
                hasPrevious = false;
                return false;
            }

            Vector3 world = hand.TransformPoint(contactOffset);
            if (float.IsNaN(world.x) || float.IsNaN(world.y) || float.IsNaN(world.z))
            {
                hasPrevious = false;
                return false;
            }

            local = chest.ToLocal(world);
            bool plausible = !hasPrevious || (local - previous).magnitude / dt <= maxHandSpeed;
            previous = local;
            hasPrevious = true;
            return plausible;
        }

        static bool HasPositionTracking(TrackedPoseDriver pose)
        {
            if (pose == null || pose.ignoreTrackingState) return true;
            var action = pose.trackingStateInput.action;
            if (action == null || !action.enabled || action.controls.Count == 0) return true;
            // InputTrackingState.Position = 1
            return (action.ReadValue<int>() & 1) != 0;
        }
    }
}
