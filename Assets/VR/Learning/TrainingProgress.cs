namespace HospitalVR.Learning
{
    public enum TrainingStage { Manual, Reconocer, PedirAyuda, Practicar, Reto, Resultados }
    /// <summary>Acciones educativas del caso. No produce llamadas ni interpreta datos clínicos.</summary>
    public sealed class TrainingProgress
    {
        public TrainingStage Stage { get; private set; } = TrainingStage.Manual;
        public bool Safety { get; private set; }
        public bool Response { get; private set; }
        public bool Breathing { get; private set; }
        public bool Location { get; private set; }
        public bool Situation { get; private set; }
        public bool Aed { get; private set; }
        public bool Responding { get; private set; }
        public bool ChallengeRunning { get; private set; }
        public void Reset() { Stage=TrainingStage.Manual; Safety=Response=Breathing=Location=Situation=Aed=Responding=ChallengeRunning=false; }
        public void BeginCase() { Reset(); Stage=TrainingStage.Reconocer; }
        public void BeginContrast() { BeginCase(); Responding=true; }
        public void ObserveSafety() { if(Stage==TrainingStage.Reconocer) { Safety=true; AdvanceRecognition(); } }
        public void CheckResponse() { if(Stage==TrainingStage.Reconocer) { Response=true; AdvanceRecognition(); } }
        public void CheckBreathing() { if(Stage==TrainingStage.Reconocer) { Breathing=true; AdvanceRecognition(); } }
        void AdvanceRecognition() { if(Safety && Response && Breathing) Stage=TrainingStage.PedirAyuda; }
        public void ProvideLocation() { if(Stage==TrainingStage.PedirAyuda) Location=true; }
        public void DescribeSituation() { if(Stage==TrainingStage.PedirAyuda) Situation=true; }
        public void AskForAed() { if(Stage==TrainingStage.PedirAyuda && !Responding) Aed=true; }
        public bool BeginPractice()
        {
            if(Stage!=TrainingStage.PedirAyuda || Responding || !Location || !Situation || !Aed) return false;
            Stage=TrainingStage.Practicar; return true;
        }
        public void DirectPractice() { Reset(); Stage=TrainingStage.Practicar; }
        public void FinishPractice() { if(Stage==TrainingStage.Practicar) Stage=TrainingStage.Reto; }
        public void BeginChallenge() { if(Stage==TrainingStage.Reto) ChallengeRunning=true; }
        public void CancelChallenge() { ChallengeRunning=false; }
        public void FinishChallenge() { if(Stage==TrainingStage.Reto && ChallengeRunning) { Stage=TrainingStage.Resultados; ChallengeRunning=false; } }
        public bool FinishContrast()
        {
            if(!Responding || Stage!=TrainingStage.PedirAyuda || !Location || !Situation) return false;
            Stage=TrainingStage.Resultados; return true;
        }
        public string MissingStep => !Safety ? "Observa si el entorno es seguro." : !Response ? "Comprueba si responde." : !Breathing ? "Comprueba si respira normalmente." :
            !Location ? "Indica la ubicación en el teléfono." : !Situation ? "Describe lo que observaste." : !Responding && !Aed ? "Pide a un testigo que traiga un DEA." : "Listo para continuar.";
    }
}
