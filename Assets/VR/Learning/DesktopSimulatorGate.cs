#pragma warning disable CS0618 // Se conserva XR Device Simulator por requisito del proyecto.
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
namespace HospitalVR.Learning
{
    /// <summary>Conserva el simulador para escritorio sin inyectar dispositivos junto al Rift.</summary>
    public sealed class DesktopSimulatorGate : MonoBehaviour
    {
        public GameObject simulator;
        public bool enableWithoutHeadset=true;
        IEnumerator Start()
        {
            // Los loaders XR se inicializan antes de cargar la escena; esperar una actualización de poses.
            yield return null;
            var existing=XRDeviceSimulator.instance;
            if(existing && existing.gameObject!=simulator)
            {
                // El simulador de XRI puede persistir en DontDestroyOnLoad al recargar un caso.
                if(simulator) simulator.SetActive(false);
                existing.gameObject.SetActive(enableWithoutHeadset && !XRSettings.isDeviceActive);
                yield break;
            }
            if(simulator) simulator.SetActive(enableWithoutHeadset && !XRSettings.isDeviceActive);
        }
    }
}
