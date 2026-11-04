namespace HospitalVR.Learning
{
    public enum AedStage { Apagado, Parches, Analizando, Apartarse, Reanudar }
    public sealed class AedSequence
    {
        public AedStage Stage {get;private set;}
        public bool FirstPad {get;private set;}
        public bool SecondPad {get;private set;}
        public void PowerOn() { if(Stage==AedStage.Apagado) Stage=AedStage.Parches; }
        public void PlacePad(int index) { if(Stage!=AedStage.Parches) return; if(index==0) FirstPad=true; else if(index==1) SecondPad=true; }
        public bool BeginAnalysis(bool handsClear)
        {
            if(Stage!=AedStage.Parches || !FirstPad || !SecondPad || !handsClear) return false;
            Stage=AedStage.Analizando; return true;
        }
        public void InterruptAnalysis() { if(Stage==AedStage.Analizando) Stage=AedStage.Parches; }
        public void AnalysisComplete() { if(Stage==AedStage.Analizando) Stage=AedStage.Apartarse; }
        public bool DeliverIndicatedShock(bool handsClear)
        {
            if(Stage!=AedStage.Apartarse || !handsClear) return false;
            Stage=AedStage.Reanudar; return true;
        }
        public void Reset() {Stage=AedStage.Apagado; FirstPad=SecondPad=false;}
    }
}
