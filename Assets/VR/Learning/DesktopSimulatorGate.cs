using System.Collections;
using UnityEngine;
using UnityEngine.XR;
namespace HospitalVR.Learning
{
    /// <summary>
    /// Activa el XR Device Simulator solo si no hay visor: así la demo funciona también en un PC
    /// sin Rift (por ejemplo, si el visor falla el día de la presentación). Con visor nunca se activa.
    /// </summary>
    public sealed class DesktopSimulatorGate : MonoBehaviour
    {
        public GameObject simulator;
        [Tooltip("Activar el simulador automáticamente si no se detecta un visor XR al iniciar.")]
        public bool autoEnableWithoutHeadset = true;
        [Tooltip("Tiempo de espera para que el runtime OpenXR inicie el visor.")]
        public float headsetWaitSeconds = 3f;

        IEnumerator Start()
        {
            if(!autoEnableWithoutHeadset || !simulator) yield break;
            float until=Time.realtimeSinceStartup+headsetWaitSeconds;
            while(Time.realtimeSinceStartup<until)
            {
                if(XRSettings.isDeviceActive) yield break;
                yield return null;
            }
            if(!XRSettings.isDeviceActive)
            {
                Debug.Log("Sin visor XR: se activa el simulador de escritorio.");
                EnableSimulator();
            }
        }

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
