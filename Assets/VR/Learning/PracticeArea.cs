using HospitalVR.RCP;
using TMPro;
using UnityEngine;
namespace HospitalVR.Learning
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PracticeArea : MonoBehaviour
    {
        public Transform rig;
        public RcpSession session;
        public RcpMetronome metronome;
        public TMP_Text orientation;
        public int EntryCount {get;private set;}
        public int ExitCount {get;private set;}
        bool Matches(Collider other) => other is CharacterController && other.transform==rig;
        void OnTriggerEnter(Collider other)
        {
            if(!Matches(other)) return; EntryCount++; metronome.practiceAreaPresent=true;
            if(orientation) orientation.text="Estación RCP · Toca INICIAR. Para practicar, coloca ambas manos junto al marcador. Puedes salir y repetir cuando quieras.";
        }
        void OnTriggerExit(Collider other)
        {
            if(!Matches(other)) return; ExitCount++; metronome.practiceAreaPresent=false;
            session.Suspend("Saliste del área de práctica"); metronome.StopClock();
        }
        void OnDisable() { if(metronome) { metronome.practiceAreaPresent=false; metronome.StopClock(); } if(session) session.Suspend("Área desactivada"); }
    }
}
