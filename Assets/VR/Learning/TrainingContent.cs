using System;
using UnityEngine;
namespace HospitalVR.Learning
{
    [Serializable] public class ManualPage
    {
        public string title;
        [TextArea(3,8)] public string instruction;
        public int illustration;
    }
    [CreateAssetMenu(menuName="Hospital VR/Contenido educativo")]
    public sealed class TrainingContent : ScriptableObject
    {
        [TextArea] public string scope="Práctica educativa: registra movimiento de mandos. No mide fuerza, profundidad real del tórax ni competencia clínica.";
        [TextArea] public string instructorReview="Un instructor local debe revisar reconocimiento, guion de ayuda y DEA, número de emergencias, accesibilidad y adaptación al público antes de usar en enseñanza.";
        public string location="Recepción universitaria, acceso principal, planta baja";
        public string contrastLocation="Sala de descanso universitaria, escenario B, habitación 3";
        public string emergencyLabel="Emergencias — simulación";
        public ManualPage[] pages;
    }
}
