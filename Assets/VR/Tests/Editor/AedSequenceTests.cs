using System;
using System.Linq;
using NUnit.Framework;
namespace HospitalVR.Tests
{
    public class AedSequenceTests
    {
        object model; Type type;
        [SetUp] public void Setup()
        {
            type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("HospitalVR.Learning.AedSequence")).FirstOrDefault(t=>t!=null);
            Assert.That(type,Is.Not.Null,"Falta la secuencia DEA"); model=Activator.CreateInstance(type);
        }
        object Call(string n,params object[] args)=>type.GetMethod(n).Invoke(model,args);
        string Stage=>type.GetProperty("Stage").GetValue(model).ToString();
        [Test] public void CannotAnalyzeBeforeBothPadsOrWhileTouchingPatient()
        {
            Call("PowerOn"); Call("PlacePad",0); Assert.That((bool)Call("BeginAnalysis",true),Is.False);
            Call("PlacePad",1); Assert.That((bool)Call("BeginAnalysis",false),Is.False);
            Assert.That((bool)Call("BeginAnalysis",true),Is.True); Assert.That(Stage,Is.EqualTo("Analizando"));
        }
        [Test] public void SimulatedShockRequiresAnalysisAndClearHands()
        {
            Call("PowerOn"); Assert.That((bool)Call("DeliverIndicatedShock",true),Is.False);
            Call("PlacePad",0); Call("PlacePad",1); Call("BeginAnalysis",true); Call("AnalysisComplete");
            Assert.That((bool)Call("DeliverIndicatedShock",false),Is.False);
            Assert.That((bool)Call("DeliverIndicatedShock",true),Is.True); Assert.That(Stage,Is.EqualTo("Reanudar"));
            Call("Reset"); Assert.That(Stage,Is.EqualTo("Apagado"));
        }
    }
}
