using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace HospitalVR.Tests
{
    public class PresentationTests
    {
        static Type T(string n)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(n)).First(t=>t!=null);
        [Test] public void DisabledTouchButtonCannotInvokeActions()
        {
            var g=new GameObject("disabled-test");
            try
            {
                var b=g.AddComponent(T("HospitalVR.RCP.RcpTouchButton"));int count=0;
                var evt=(UnityEngine.Events.UnityEvent)b.GetType().GetField("onPressed").GetValue(b);evt.AddListener(()=>count++);
                b.GetType().GetField("interactable").SetValue(b,false);b.GetType().GetMethod("Press").Invoke(b,null);
                Assert.That(count,Is.EqualTo(0));
                b.GetType().GetField("interactable").SetValue(b,true);b.GetType().GetMethod("Press").Invoke(b,null);
                Assert.That(count,Is.EqualTo(1));
            }
            finally{UnityEngine.Object.DestroyImmediate(g);}
        }
        [Test] public void InterruptedChallengeCanBeRetriedWithoutCompletingResults()
        {
            var p=Activator.CreateInstance(T("HospitalVR.Learning.TrainingProgress"));
            void Call(string n)=>p.GetType().GetMethod(n).Invoke(p,null);
            Call("DirectPractice");Call("FinishPractice");Call("BeginChallenge");Call("CancelChallenge");
            Assert.That(p.GetType().GetProperty("Stage").GetValue(p).ToString(),Is.EqualTo("Reto"));
            Assert.That(p.GetType().GetProperty("ChallengeRunning").GetValue(p),Is.False);
            Call("BeginChallenge");Assert.That(p.GetType().GetProperty("ChallengeRunning").GetValue(p),Is.True);
        }
    }
}
