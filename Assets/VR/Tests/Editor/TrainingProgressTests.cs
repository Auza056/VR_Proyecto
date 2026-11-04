using System;
using System.Linq;
using NUnit.Framework;
namespace HospitalVR.Tests
{
    public class TrainingProgressTests
    {
        object progress; Type type;
        [SetUp] public void Setup()
        {
            type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("HospitalVR.Learning.TrainingProgress")).FirstOrDefault(t=>t!=null);
            Assert.That(type,Is.Not.Null,"Falta el recorrido de aprendizaje"); progress=Activator.CreateInstance(type);
        }
        object Call(string n,params object[] args)=>type.GetMethod(n).Invoke(progress,args);
        string Stage=>type.GetProperty("Stage").GetValue(progress).ToString();
        [Test] public void CannotSkipRecognitionOrIncompleteCall()
        {
            Call("BeginCase"); Assert.That((bool)Call("BeginPractice"),Is.False);
            Call("ObserveSafety"); Call("CheckResponse"); Call("CheckBreathing");
            Assert.That(Stage,Is.EqualTo("PedirAyuda"));
            Call("ProvideLocation"); Call("DescribeSituation"); Assert.That((bool)Call("BeginPractice"),Is.False);
            Call("AskForAed"); Assert.That((bool)Call("BeginPractice"),Is.True);
            Assert.That(Stage,Is.EqualTo("Practicar"));
        }
        [Test] public void PracticeThenChallengeThenResultsAndReset()
        {
            Call("DirectPractice"); Call("FinishPractice"); Assert.That(Stage,Is.EqualTo("Reto"));
            Call("BeginChallenge"); Call("FinishChallenge"); Assert.That(Stage,Is.EqualTo("Resultados"));
            Call("Reset"); Assert.That(Stage,Is.EqualTo("Manual"));
            Assert.That((bool)Call("BeginPractice"),Is.False);
        }
        [Test] public void RespondingCaseNeverEntersCompressionPractice()
        {
            Call("BeginContrast"); Call("ObserveSafety"); Call("CheckResponse"); Call("CheckBreathing");
            Call("ProvideLocation"); Call("DescribeSituation"); Call("AskForAed");
            Assert.That((bool)Call("BeginPractice"),Is.False);
        }
    }
}
