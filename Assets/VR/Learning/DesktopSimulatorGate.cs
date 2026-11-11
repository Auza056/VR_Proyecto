using UnityEngine;
namespace HospitalVR.Learning
{
    /// <summary>Permite activar el simulador manualmente sin detectar el casco ni modificarlo al iniciar.</summary>
    public sealed class DesktopSimulatorGate : MonoBehaviour
    {
        public GameObject simulator;

        [ContextMenu("Activar simulador")]
        public void EnableSimulator()
        {
            if(simulator) simulator.SetActive(true);
        }

        [ContextMenu("Desactivar simulador")]
        public void DisableSimulator()
        {
            if(simulator) simulator.SetActive(false);
        }
    }
}
