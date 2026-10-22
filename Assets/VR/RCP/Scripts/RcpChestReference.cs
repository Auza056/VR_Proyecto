using UnityEngine;

namespace HospitalVR.RCP
{
    /// <summary>
    /// Punto de referencia del tórax. El origen se sitúa sobre la superficie de la mitad inferior
    /// del esternón y el eje Y local apunta hacia fuera del pecho. Ajustable desde el Inspector o
    /// moviendo el objeto en la escena.
    /// </summary>
    public sealed class RcpChestReference : MonoBehaviour
    {
        [Tooltip("Semiancho de la zona de práctica (eje X local, hacia los lados del paciente), en metros.")]
        [Min(0.02f)] public float halfWidth = 0.14f;

        [Tooltip("Semilongitud de la zona de práctica (eje Z local, de pies a cabeza), en metros.")]
        [Min(0.02f)] public float halfLength = 0.15f;

        [Tooltip("Altura máxima aceptada sobre la superficie del tórax, en metros.")]
        [Min(0.05f)] public float maxHeightAbove = 0.32f;

        [Tooltip("Penetración máxima aceptada bajo la superficie. El mando no encuentra resistencia física.")]
        [Min(0.02f)] public float maxDepthBelow = 0.15f;

        /// <summary>Convierte un punto del mundo al marco del tórax (sin escala).</summary>
        public Vector3 ToLocal(Vector3 worldPoint)
        {
            return Quaternion.Inverse(transform.rotation) * (worldPoint - transform.position);
        }

        public bool IsInside(Vector3 localPoint)
        {
            return Mathf.Abs(localPoint.x) <= halfWidth
                && Mathf.Abs(localPoint.z) <= halfLength
                && localPoint.y <= maxHeightAbove
                && localPoint.y >= -maxDepthBelow;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            float height = maxHeightAbove + maxDepthBelow;
            Gizmos.color = new Color(0.1f, 0.6f, 1f, 0.25f);
            Gizmos.DrawCube(new Vector3(0, (maxHeightAbove - maxDepthBelow) * 0.5f, 0), new Vector3(halfWidth * 2, height, halfLength * 2));
            Gizmos.color = new Color(0.1f, 0.6f, 1f, 1f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(halfWidth * 2, 0.002f, halfLength * 2));
            Gizmos.DrawLine(Vector3.zero, Vector3.up * 0.1f);
        }
    }
}
