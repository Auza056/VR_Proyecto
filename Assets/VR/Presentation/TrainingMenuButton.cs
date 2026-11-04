using HospitalVR.RCP;
using TMPro;
using UnityEngine;

namespace HospitalVR.Presentation
{
    public enum MenuCommand { Home, Settings, Results, StartCase, Practice, Challenge, Reset, RecoverManual, Finish, Duration30, Duration60, Audio, Haptics, Visual, Hand, IntensityDown, IntensityUp, VolumeDown, VolumeUp, Metronome, Marker }
    public sealed class TrainingMenuButton : MonoBehaviour
    {
        public TrainingMenu menu;
        public MenuCommand command;
        public RcpTouchButton touch;
        public TMP_Text label;
        public string caption;
        public void Execute() { if(menu) menu.Execute(command); }
    }
}
