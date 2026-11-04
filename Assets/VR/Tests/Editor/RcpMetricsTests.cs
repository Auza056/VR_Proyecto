using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace HospitalVR.Tests
{
    public class RcpMetricsTests
    {
        static Type T(string n) => AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("HospitalVR.RCP."+n)).First(t=>t!=null);
        static object Call(object o,string n,params object[] args) => o.GetType().GetMethod(n).Invoke(o,args);
        [TestCase(.75f,80f)] [TestCase(.5454545f,110f)] [TestCase(.4f,150f)]
        public void RateDistinguishesSlowTargetAndFast(float gap,float expected)
        {
            var rate=Activator.CreateInstance(T("RcpRateEstimator"),new object[]{16});
            Call(rate,"Add",0f); Call(rate,"Add",gap); Call(rate,"Add",gap*2);
            Assert.That((float)Call(rate,"Rate",gap*2,6,2.5f),Is.EqualTo(expected).Within(.01));
        }
        [Test] public void LongPauseDoesNotPolluteNewRateWindow()
        {
            var rate=Activator.CreateInstance(T("RcpRateEstimator"),new object[]{16});
            foreach(float t in new[]{0f,.5f,1f,10f,10.5f}) Call(rate,"Add",t);
            Assert.That((float)Call(rate,"Rate",10.5f,6,2.5f),Is.EqualTo(0f));
            Call(rate,"Add",11f);
            Assert.That((float)Call(rate,"Rate",11f,6,2.5f),Is.EqualTo(120f).Within(.01));
        }
        [TestCase(.08f,.08f,-.08f,.18f,"EnZona")]
        [TestCase(.25f,.08f,-.25f,.08f,"ManosSeparadas")]
        [TestCase(.3f,.08f,.31f,.10f,"FueraDeZona")]
        public void PlacementChecksBothHandsAndAllowsStackedTouch(float lx,float ly,float rx,float ry,string expected)
        {
            var go=new GameObject("test");
            try
            {
                var chest=go.AddComponent(T("RcpChestReference"));
                var detector=go.AddComponent(T("RcpCompressionDetector"));
                detector.GetType().GetField("chest").SetValue(detector,chest);
                var method=detector.GetType().GetMethod("EvaluatePlacement");
                Assert.That(method,Is.Not.Null,"Falta validación individual de manos");
                Assert.That(method.Invoke(detector,new object[]{new Vector3(lx,ly,0),new Vector3(rx,ry,0)}).ToString(),Is.EqualTo(expected));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        [TestCase(.195f,1)] [TestCase(.15f,0)]
        public void EndingIncludesOnlyAlreadyConfirmedPendingReturn(float peak,int expectedEvaluations)
        {
            var go=new GameObject("return-test");
            try
            {
                var detector=go.AddComponent(T("RcpCompressionDetector")); int evaluations=0;
                Action<bool> handler=value=>{Assert.That(value,Is.True); evaluations++;};
                detector.GetType().GetEvent("ReturnEvaluated").AddEventHandler(detector,handler);
                var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
                detector.GetType().GetField("returnPending",flags).SetValue(detector,true);
                detector.GetType().GetField("returnReference",flags).SetValue(detector,.2f);
                detector.GetType().GetField("returnPeak",flags).SetValue(detector,peak);
                Call(detector,"CompletePendingReturn"); Call(detector,"CompletePendingReturn");
                Assert.That(evaluations,Is.EqualTo(expectedEvaluations));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}

