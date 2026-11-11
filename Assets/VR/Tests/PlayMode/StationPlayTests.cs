#pragma warning disable CS0618 // Se conserva XR Device Simulator por requisito del proyecto.
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
namespace HospitalVR.Tests
{
    public class StationPlayTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        Component session,detector,metro,flow,manual;
        XRDeviceSimulator simulator;
        Transform left,right,chest;
        AudioClip initialClip;
        UnityEngine.InputSystem.InputSettings.BackgroundBehavior previousBackground;
        [OneTimeSetUp] public void PreserveInputEnvironment()
        {
            previousBackground=UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior;
            UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
        }
        [OneTimeTearDown] public void RestoreInputEnvironment() => UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=previousBackground;
        static Type T(string n)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(n)).First(t=>t!=null);
        static Component Find(string n)=>UnityEngine.Object.FindAnyObjectByType(T(n)) as Component;
        static object Get(object o,string n)=>o.GetType().GetProperty(n)?.GetValue(o)??o.GetType().GetField(n).GetValue(o);
        static void Set(object o,string n,object v)=>o.GetType().GetField(n).SetValue(o,v);
        static object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(o,args);
        [UnityTearDown] public IEnumerator TearDown()
        {
            // Cada caso se restablece en Setup, igual que la experiencia real, sin recargar acciones compartidas.
            yield return null;
        }
        [UnitySetUp] public IEnumerator Setup()
        {
            if(!SceneManager.GetSceneByName("HospitalWard").isLoaded) yield return SceneManager.LoadSceneAsync("HospitalWard");
            yield return null;
            session=Find("HospitalVR.RCP.RcpSession"); detector=Find("HospitalVR.RCP.RcpCompressionDetector"); metro=Find("HospitalVR.RCP.RcpMetronome");
            flow=Find("HospitalVR.Learning.TrainingFlow"); manual=Find("HospitalVR.Learning.TrainingManual");
            yield return null;
            // Estas pruebas solicitan el simulador explícitamente; la experiencia inicia sin él.
            simulator=UnityEngine.Object.FindObjectsByType<XRDeviceSimulator>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
            Assert.That(simulator,Is.Not.Null,"No se encontró el simulador de escritorio");
            simulator.gameObject.SetActive(true);
            yield return null;
            typeof(XRDeviceSimulator).GetField("m_GripInput",Private).SetValue(simulator,false);
            left=(Transform)Get(detector,"leftHand"); right=(Transform)Get(detector,"rightHand"); chest=((Component)Get(detector,"chest")).transform;
            if(!initialClip) initialClip=(AudioClip)Get(metro,"clip");
            Call(flow,"ResetCase");
            Set(metro,"clip",initialClip); Set(metro,"audioOn",true); Set(metro,"hapticsOn",true); Set(metro,"visualOn",true); Call(metro,"OnApplicationFocus",true);
            var rig=(Transform)Get(detector,"rigRoot"); rig.SetPositionAndRotation(new Vector3(-1.15f,0,.65f),Quaternion.Euler(0,90,0));
            Set(metro,"practiceAreaPresent",true);
            foreach(string side in new[]{"Left","Right"})
            {
                var tracked=typeof(XRDeviceSimulator).GetField("m_"+side+"ControllerTrackingState",Private);
                tracked.SetValue(simulator,Enum.ToObject(tracked.FieldType,3));
                typeof(XRDeviceSimulator).GetField("m_"+side+"ControllerIsTracked",Private).SetValue(simulator,true);
            }
            foreach(var manager in UnityEngine.Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager>())
                foreach(var asset in manager.actionAssets) asset.Enable();
            foreach(var hand in new[]{left,right})
            {
                var pose=hand.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                pose.positionInput.action.Enable(); pose.rotationInput.action.Enable(); pose.trackingStateInput.action.Enable();
            }
#if UNITY_EDITOR
            UnityEditor.EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
#endif
            Set(session,"sessionDuration",30f); Set(session,"preparationHoldTime",.2f);
            Pose("Left",rig.position+new Vector3(-.3f,1.1f,0)); Pose("Right",rig.position+new Vector3(.3f,1.1f,0));
            yield return new WaitForSecondsRealtime(.5f);
            float readyDeadline=Time.realtimeSinceStartup+5f;
            while((left.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>().trackingStateInput.action.ReadValue<int>()&1)==0 ||
                  (right.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>().trackingStateInput.action.ReadValue<int>()&1)==0)
            {
                Assert.That(Time.realtimeSinceStartup,Is.LessThan(readyDeadline),"El Input System no recibió seguimiento del simulador. enabled="+simulator.enabled+" active="+simulator.gameObject.activeInHierarchy);
                yield return null;
            }
        }
        void Pose(string side,Vector3 world,float grip=0)
        {
            var field=typeof(XRDeviceSimulator).GetField("m_"+side+"ControllerState",Private);
            var state=(XRSimulatedControllerState)field.GetValue(simulator);
            if(side=="Right") typeof(XRDeviceSimulator).GetField("m_GripInput",Private).SetValue(simulator,grip>.5f);
            var hand=side=="Left"?left:right;
            state.devicePosition=hand.parent.InverseTransformPoint(world); state.deviceRotation=Quaternion.identity;
            state.grip=grip; state.WithButton(ControllerButton.GripButton,grip>.5f);
            field.SetValue(simulator,state);
        }
        IEnumerator Hold(float seconds,Vector3 l,Vector3 r)
        {
            float end=Time.time+seconds;
            while(Time.time<end) {Pose("Left",l); Pose("Right",r); yield return null;}
        }
        IEnumerator StartSession()
        {
            Call(flow,"DirectPractice");
            yield return Hold(1.8f,chest.position+new Vector3(-.035f,.18f,0),chest.position+new Vector3(.035f,.23f,0));
            Assert.That(Get(session,"State").ToString(),Is.EqualTo("Activa"),"Motivo="+Get(Get(session,"Results"),"stopReason")+" Estado manos: "+Get(detector,"Status")+" L="+left.position+" R="+right.position+" pecho="+chest.position);
        }
        [UnityTest] public IEnumerator MetronomeWithoutAudioStopsImmediatelyAtFinish()
        {
            Set(metro,"audioOn",false); Set(metro,"clip",null);
            yield return StartSession(); yield return new WaitForSeconds(1.3f);
            int count=(int)Get(metro,"HapticDispatchCount");
            Assert.That(count,Is.GreaterThanOrEqualTo(2));
            Assert.That(Get(metro,"BeatCount"),Is.EqualTo(count));
            Call(session,"Finish"); int stopped=(int)Get(metro,"BeatCount");
            yield return new WaitForSeconds(.8f);
            Assert.That(Get(metro,"BeatCount"),Is.EqualTo(stopped));
            Assert.That(((Transform)Get(metro,"visualIndicator")).gameObject.activeSelf,Is.False);
        }
        [UnityTest] public IEnumerator SimulatorCyclesMeasureTargetRateAndIgnoreSeparatedHands()
        {
            yield return StartSession();
            float start=Time.time;
            while(Time.time-start<5f)
            {
                float y=.18f-.09f*(1f-Mathf.Cos((Time.time-start)*Mathf.PI*2*110/60))*.5f;
                Pose("Left",chest.position+new Vector3(-.035f,y,0)); Pose("Right",chest.position+new Vector3(.035f,y+.05f,0)); yield return null;
            }
            Assert.That((int)Get(session,"Compressions"),Is.InRange(7,10));
            Assert.That((float)Get(session,"CurrentRate"),Is.InRange(100f,120f));
            int before=(int)Get(session,"Compressions");
            yield return Hold(.8f,chest.position+new Vector3(-.3f,.12f,0),chest.position+new Vector3(.3f,.12f,0));
            Assert.That(Get(detector,"Status").ToString(),Is.EqualTo("ManosSeparadas"));
            Assert.That(Get(session,"Compressions"),Is.EqualTo(before));
        }
        [UnityTest] public IEnumerator SimulatorCanGrabReleaseAndRecoverManual()
        {
            var grab=manual.GetComponent<XRGrabInteractable>();
            var point=manual.transform.position;
            yield return Hold(.5f,point+Vector3.left*.5f,point);
            float end=Time.time+1f;
            while(Time.time<end) {point=manual.transform.position; Pose("Right",point,1f); yield return null;}
            Assert.That(grab.isSelected,Is.True,"Grip simulado debe seleccionar el portapapeles. Mano="+right.position+" manual="+manual.transform.position);
            yield return Hold(.5f,point+Vector3.left*.5f,point);
            Assert.That(grab.isSelected,Is.False);
            manual.GetComponent<Rigidbody>().position=new Vector3(0,-1,0); yield return new WaitForSeconds(.2f);
            Assert.That(Vector3.Distance(manual.transform.position,((Transform)Get(manual,"dock")).position),Is.LessThan(.1f));
        }
        [UnityTest] public IEnumerator FullFlowChallengeStartsWithoutAidsAndTracksOptIn()
        {
            Call(flow,"BeginCase"); Call(flow,"ObserveSafety"); Call(flow,"CheckResponse"); Call(flow,"CheckBreathing");
            Call(flow,"ProvideLocation"); Call(flow,"DescribeSituation"); Call(flow,"AskForAed"); Call(flow,"ContinueToPractice");
            yield return Hold(1.1f,chest.position+new Vector3(-.03f,.18f,0),chest.position+new Vector3(.03f,.23f,0));
            Call(session,"Finish"); Call(flow,"StartChallenge");
            Call(flow,"StartChallenge"); // Una segunda pulsación no convierte el reto en práctica guiada.
            Assert.That(Get(session,"challenge"),Is.True);
            Assert.That(Get(metro,"isOn"),Is.False);
            Assert.That(Get(Get(flow,"rcpPanel"),"markerAid"),Is.False);
            Call(metro,"Toggle"); yield return null;
            Assert.That(Get(session,"aidsUsed"),Is.True);
            yield return Hold(1.1f,chest.position+new Vector3(-.03f,.18f,0),chest.position+new Vector3(.03f,.23f,0));
            Call(session,"Finish");
            Assert.That(Get(Get(flow,"Progress"),"Stage").ToString(),Is.EqualTo("Resultados"));
            Assert.That(Get(Get(session,"Results"),"aidsUsed"),Is.True);
        }
        [UnityTest] public IEnumerator CharacterControllerExitStopsSessionAndZeroDataAreExplicit()
        {
            yield return StartSession();
            var rig=(Transform)Get(detector,"rigRoot");
            for(int i=0;i<35;i++) { rig.GetComponent<CharacterController>().Move(new Vector3(0,0,-.12f)); yield return null; }
            yield return new WaitForSeconds(.2f);
            Assert.That(Get(session,"State").ToString(),Is.EqualTo("Finalizada"),"Posición rig="+rig.position);
            var area=Find("HospitalVR.Learning.PracticeArea");
            Assert.That((int)Get(area,"ExitCount"),Is.GreaterThan(0));
            var view=Get(flow,"rcpPanel"); var text=(TMPro.TMP_Text)Get(view,"metricsText");
            Assert.That(text.text,Does.Contain("Datos insuficientes"));
        }
        [UnityTest] public IEnumerator TrackingLossIsSeparateFromHandsOutsideAndFocusStopsSession()
        {
            yield return StartSession();
            var tracking=typeof(XRDeviceSimulator).GetField("m_RightControllerTrackingState",Private);
            tracking.SetValue(simulator,Enum.ToObject(tracking.FieldType,0));
            yield return Hold(.65f,chest.position+new Vector3(-.03f,.18f,0),chest.position+new Vector3(.03f,.23f,0));
            Assert.That(Get(detector,"Status").ToString(),Is.EqualTo("SinSeguimiento"));
            Call(session,"OnApplicationFocus",false);
            var results=Get(session,"Results");
            Assert.That((float)Get(results,"trackingLostTime"),Is.GreaterThan(.3f));
            Assert.That((int)Get(results,"trackingInterruptions"),Is.GreaterThan(0));
            Assert.That(Get(session,"State").ToString(),Is.EqualTo("Finalizada"));
            Assert.That((float)Get(results,"handsOffTime"),Is.LessThan(.3f));
        }
        IEnumerator Touch(string name)
        {
            var b=UnityEngine.Object.FindObjectsByType(T("HospitalVR.RCP.RcpTouchButton")).Cast<Component>().First(c=>c.name==name && (bool)Get(c,"interactable"));
            Vector3 p=b.transform.TransformPoint((Vector3)Get(b,"triggerCenter"));
            yield return Hold(.4f,p+Vector3.down,p+Vector3.down);
            yield return Hold(.4f,p+Vector3.down,p);
        }
        [UnityTest] public IEnumerator RecognitionAndPhoneUseSimulatorTouchVolumes()
        {
            MenuAction("Home");
            foreach(string button in new[]{"Tocar_INICIAR CASO","Tocar_ENTORNO SEGURO","Tocar_COMPROBAR RESPUESTA","Tocar_OBSERVAR RESPIRACIÓN","Tocar_UBICACIÓN","Tocar_SITUACIÓN","Tocar_TESTIGO + DEA","Tocar_CONTINUAR"})
                yield return Touch(button);
            Assert.That(Get(Get(flow,"Progress"),"Stage").ToString(),Is.EqualTo("Practicar"));
            Assert.That(Get(session,"State").ToString(),Is.EqualTo("Preparacion"));
        }
        [UnityTest] public IEnumerator MenuTabsUseTrackedHandContact()
        {
            MenuAction("Home");yield return Touch("Tocar_AJUSTES");
            Assert.That(Get(Menu(),"Page"),Is.EqualTo(1));
            yield return Touch("Tocar_RESULTADOS");Assert.That(Get(Menu(),"Page"),Is.EqualTo(2));
            yield return Touch("Tocar_INICIO");Assert.That(Get(Menu(),"Page"),Is.EqualTo(0));
        }
        [UnityTest] public IEnumerator SlowAndFastSimulatorCyclesAreDistinguished()
        {
            foreach(float bpm in new[]{80f,140f})
            {
                yield return StartSession(); float start=Time.time;
                while(Time.time-start<10f)
                {
                    float y=.18f-.09f*(1f-Mathf.Cos((Time.time-start)*Mathf.PI*2*bpm/60))*.5f;
                    Pose("Left",chest.position+new Vector3(-.03f,y,0)); Pose("Right",chest.position+new Vector3(.03f,y+.05f,0)); yield return null;
                    if(Time.time-start>=3f && (int)Get(session,"Compressions")>=5 && (float)Get(session,"CurrentRate")>0f) break;
                }
                Assert.That((float)Get(session,"CurrentRate"),Is.EqualTo(bpm).Within(10f),"Estado="+Get(session,"State")+" motivo="+Get(Get(session,"Results"),"stopReason")+" conteo="+Get(session,"Compressions")+" manos="+Get(detector,"Status"));
                Call(session,"Finish");
            }
        }
        [UnityTest] public IEnumerator AedNeedsTwoPadsAndClearTrackedHands()
        {
            var aed=Find("HospitalVR.Learning.TrainingAed"); Assert.That(aed,Is.Not.Null);
            Set(aed,"analysisDuration",.25f); Call(aed,"PowerOn");
            var pads=(XRGrabInteractable[])Get(aed,"pads"); var targets=(Transform[])Get(aed,"targets");
            pads[0].GetComponent<Rigidbody>().position=targets[0].position; yield return null;
            Assert.That(Get(Get(aed,"Sequence"),"Stage").ToString(),Is.EqualTo("Parches"));
            pads[1].GetComponent<Rigidbody>().position=targets[1].position;
            var chestAed=(Transform)Get(aed,"chest");
            yield return Hold(.65f,chestAed.position+Vector3.left,chestAed.position+Vector3.right);
            Assert.That(Get(Get(aed,"Sequence"),"Stage").ToString(),Is.EqualTo("Apartarse"),"Parche1="+Get(Get(aed,"Sequence"),"FirstPad")+" parche2="+Get(Get(aed,"Sequence"),"SecondPad")+" L="+left.position+" R="+right.position);
            Call(aed,"Shock"); Assert.That(Get(aed,"SimulatedShocks"),Is.EqualTo(1));
            Call(aed,"ResetDevice"); Assert.That(Get(Get(aed,"Sequence"),"Stage").ToString(),Is.EqualTo("Apagado"));
        }
        [UnityTest] public IEnumerator ChallengeDoesNotCoachPlacementWithoutOptIn()
        {
            Call(flow,"DirectPractice"); Set(session,"preparationHoldTime",3f);
            Set(session,"challenge",true); Set(session,"aidsUsed",false);
            Set(metro,"isOn",false);
            var view=Get(flow,"rcpPanel"); Set(view,"markerAid",false);
            yield return Hold(.5f,chest.position+new Vector3(-.3f,.18f,0),chest.position+new Vector3(.3f,.18f,0));
            Assert.That(((TMPro.TMP_Text)Get(view,"hintText")).text,Does.Not.Contain("marcador azul"));
            Assert.That(((TMPro.TMP_Text)Get(view,"hintText")).text,Does.Not.Contain("Junta las manos"));
            Assert.That(Get(session,"aidsUsed"),Is.False);
        }
        [UnityTest] public IEnumerator RestartDirectPracticeDoesNotAdvanceToChallenge()
        {
            yield return StartSession(); Call(flow,"DirectPractice");
            Assert.That(Get(Get(flow,"Progress"),"Stage").ToString(),Is.EqualTo("Practicar"));
            Assert.That(Get(session,"State").ToString(),Is.EqualTo("Preparacion"));
        }
        [UnityTest] public IEnumerator RigTurningAndTranslationCannotCountCompressions()
        {
            yield return StartSession(); var rig=(Transform)Get(detector,"rigRoot");
            rig.Rotate(0,45,0);
            yield return Hold(.2f,chest.position+new Vector3(-.03f,.08f,0),chest.position+new Vector3(.03f,.13f,0));
            Assert.That(Get(session,"Compressions"),Is.EqualTo(0));
            Assert.That(Get(detector,"Status").ToString(),Is.EqualTo("Desplazandose"));
            rig.position+=Vector3.right*.2f;
            yield return Hold(.2f,chest.position+new Vector3(-.03f,.18f,0),chest.position+new Vector3(.03f,.23f,0));
            Assert.That(Get(session,"Compressions"),Is.EqualTo(0));
        }
        Component Menu()=>Find("HospitalVR.Presentation.TrainingMenu");
        object Command(string name)=>Enum.Parse(T("HospitalVR.Presentation.MenuCommand"),name);
        void MenuAction(string name)=>Call(Menu(),"Execute",Command(name));
        [UnityTest] public IEnumerator MenuDurationIsLockedAndChallengeUsesPracticeDuration()
        {
            MenuAction("Settings");MenuAction("Duration30");
            Assert.That(Get(session,"sessionDuration"),Is.EqualTo(30f));
            yield return StartSession();MenuAction("Duration60");
            Assert.That(Get(session,"sessionDuration"),Is.EqualTo(30f));
            Call(session,"Finish");Assert.That(Get(flow,"HasPracticeResults"),Is.True);
            Set(session,"sessionDuration",99f); Call(flow,"StartChallenge");
            Assert.That(Get(session,"sessionDuration"),Is.EqualTo(30f));
            Assert.That(Get(session,"challenge"),Is.True);
        }
        [UnityTest] public IEnumerator MenuResetRequiresConfirmationAndTabsRetainProgress()
        {
            MenuAction("Home");MenuAction("StartCase");MenuAction("Settings");MenuAction("Results");
            Assert.That(Get(Get(flow,"Progress"),"Stage").ToString(),Is.EqualTo("Reconocer"));
            MenuAction("Reset");Assert.That(Get(Get(flow,"Progress"),"Stage").ToString(),Is.EqualTo("Reconocer"));
            MenuAction("Reset");Assert.That(Get(Get(flow,"Progress"),"Stage").ToString(),Is.EqualTo("Manual"));
            Assert.That(Get(Menu(),"Page"),Is.EqualTo(0));yield return null;
        }
        [UnityTest] public IEnumerator InterruptedPracticeDoesNotUnlockChallengeAndComparisonIsVisible()
        {
            yield return StartSession();Call(session,"Suspend","Prueba de interrupción");
            Assert.That(Get(Get(flow,"Progress"),"Stage").ToString(),Is.EqualTo("Practicar"));
            Assert.That((bool)Call(Menu(),"CanExecute",Command("Challenge")),Is.False);
            MenuAction("Results");Call(Menu(),"RefreshNow");
            Assert.That(((TMPro.TMP_Text)Get(Menu(),"practiceText")).text,Does.Contain("Interrumpido"));
            Assert.That(((TMPro.TMP_Text)Get(Menu(),"practiceText")).text,Does.Contain("Datos insuficientes"));
        }
    }
}

