#pragma warning disable CS0618 // Se conserva XR Device Simulator por requisito del proyecto.
using System.Linq;
using HospitalVR.Learning;
using HospitalVR.RCP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using static HospitalVR.Editor.RcpFinalSetup;
namespace HospitalVR.Editor
{
    public static class TrainingSetup
    {
        public static string Apply()
        {
            Guard();
            var old=GameObject.Find("RecorridoEducativo");
            if(old) return "Recorrido ya instalado; se conservan referencias y ajustes locales.";
            var root=new GameObject("RecorridoEducativo").transform;
            var flow=root.gameObject.AddComponent<TrainingFlow>();
            var gate=root.gameObject.AddComponent<DesktopSimulatorGate>();
            gate.simulator=UnityEngine.Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRDeviceSimulator>(FindObjectsInactive.Include).Single().gameObject;
            gate.simulator.SetActive(false);
            var session=UnityEngine.Object.FindAnyObjectByType<RcpSession>();
            flow.session=session; flow.metronome=UnityEngine.Object.FindAnyObjectByType<RcpMetronome>(); flow.rcpPanel=session.GetComponent<RcpPanelView>();
            var rig=GameObject.Find("XR Origin (VR)").transform;
            var capsule=rig.GetComponent<CharacterController>();
            capsule.height=1.65f; capsule.center=new Vector3(0,.905f,0); capsule.radius=.30f;
            Hands=new[]{rig.Find("Camera Offset/LeftHandController"),rig.Find("Camera Offset/RightHandController")};
            flow.content=Content();
            Manual(root,flow);
            Navigation(root,flow);
            Case(root,flow,false);
            Phone(root,flow);
            var area=new GameObject("AreaPractica_CharacterController"); area.transform.SetParent(session.transform.parent,false);
            area.transform.position=new Vector3(-.4f,1.2f,.9f);
            var box=area.AddComponent<BoxCollider>(); box.size=new Vector3(2.8f,3f,3.6f); box.isTrigger=true;
            var body=area.AddComponent<Rigidbody>(); body.isKinematic=true; body.useGravity=false;
            var a=area.AddComponent<PracticeArea>(); a.rig=rig; a.session=session; a.metronome=flow.metronome;
            a.orientation=Text("Orientacion",root,new Vector3(-1f,1.8f,2.6f),"Entra en el área azul para conocer la estación.",.30f,new Vector2(.8f,.2f));
            // Ampliar el informe para que todas las métricas sean visibles.
            flow.rcpPanel.metricsText.fontSize=.29f; flow.rcpPanel.metricsText.rectTransform.sizeDelta=new Vector2(.86f,.39f);
            flow.rcpPanel.metricsText.transform.localPosition+=new Vector3(0,-.04f,0);
            flow.rcpPanel.hintText.fontSize=.27f;
            flow.rcpPanel.hintText.transform.localPosition+=new Vector3(0,-.11f,0);
            flow.rcpPanel.noteText.transform.localPosition+=new Vector3(0,-.15f,0);
            flow.Refresh(); Save();
            PrefabUtility.SaveAsPrefabAsset(flow.manual.gameObject,"Assets/VR/Learning/ManualIlustrado.prefab");
            return "Guardado: manual agarrable, atril, recepción, teléfono, navegación y trigger de CharacterController.";
        }
        static TrainingContent Content()
        {
            const string path="Assets/VR/Learning/ContenidoEducativo.asset";
            var c=AssetDatabase.LoadAssetAtPath<TrainingContent>(path); if(c) return c;
            c=ScriptableObject.CreateInstance<TrainingContent>();
            c.pages=new[]{
                new ManualPage{title="1 · Controles VR",illustration=0,instruction="Prueba agarrar con GRIP y soltar al liberarlo. Acerca la otra mano a PRUEBA para tocar un botón.\nConsulta el manual en el atril o llévalo contigo. RECUPERAR lo devuelve al soporte."},
                new ManualPage{title="2 · Reconocer la emergencia",illustration=1,instruction="Observa que el entorno sea seguro. Habla al adulto y toca sus hombros. Comprueba si respira normalmente.\nSi no responde y no respira normalmente o solo jadea, sospecha un paro y pide ayuda."},
                new ManualPage{title="3 · Pedir ayuda",illustration=2,instruction="Activa emergencias. Indica ubicación y lo observado. Pide a un testigo que traiga un DEA.\nEn el juego, completa el teléfono: ubicación, situación y testigo. La llamada es simulada. Sigue las indicaciones del operador real."},
                new ManualPage{title="4 · Colocar las manos",illustration=3,instruction="Coloca una mano sobre la otra en el centro del pecho, sobre la mitad inferior del esternón.\nEn la estación: alinea los Touch junto al marcador azul; brazos extendidos y hombros sobre las manos. La camilla es solo para practicar mandos."},
                new ManualPage{title="5 · Seguir el ritmo",illustration=4,instruction="Inicia el descenso con cada pulso. Deja volver las manos entre ciclos. Objetivo: 100–120/min.\nPráctica: 60 s; después reto sin ayudas. Esta animación demuestra 110/min. El contador registra ciclos detectados."}
            };
            AssetDatabase.CreateAsset(c,path); return c;
        }
        static void Manual(Transform root,TrainingFlow flow)
        {
            var board=GameObject.Find("Habitacion_02/PortapapelesClinico");
            board.transform.SetParent(root,true); board.name="ManualIlustrado";
            foreach(Transform child in board.transform.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
            board.transform.SetPositionAndRotation(new Vector3(-.55f,1.30f,1.90f),Quaternion.Euler(0,90,0));
            var collider=board.GetComponent<BoxCollider>(); collider.size=new Vector3(.52f,.80f,.035f);
            var manual=board.AddComponent<TrainingManual>(); manual.content=flow.content; flow.manual=manual;
            Part("PortapapelesOriginal",board.transform,Vector3.zero,new Vector3(.52f,.80f,.025f),Dark);
            Part("Pinza",board.transform,new Vector3(0,.385f,-.018f),new Vector3(.12f,.025f,.018f),Accent);
            manual.title=Text("Titulo",board.transform,new Vector3(0,.32f,-.025f),"Manual",.52f,new Vector2(.49f,.10f));
            manual.body=Text("Instruccion",board.transform,new Vector3(0,-.025f,-.025f),"",.39f,new Vector2(.47f,.25f));
            manual.confirmation=Text("PruebaControles",board.transform,new Vector3(0,-.18f,-.025f),"",.22f,new Vector2(.48f,.04f));
            manual.footer=Text("Alcance",board.transform,new Vector3(0,-.33f,-.025f),"",.22f,new Vector2(.48f,.10f));
            Button(board.transform,"Anterior",new Vector3(-.165f,-.24f,0),manual.Previous,.15f);
            Button(board.transform,"PRUEBA",new Vector3(0,-.24f,0),manual.TestButton,.15f);
            Button(board.transform,"Siguiente",new Vector3(.165f,-.24f,0),manual.Next,.15f);
            manual.illustrations=Enumerable.Range(0,5).Select(i=>ManualIllustrations.Make(board.transform,i)).ToArray();
            manual.pulseDemo=new GameObject("Demostracion110").transform; manual.pulseDemo.SetParent(manual.illustrations[4],false);
            Part("Pulso",manual.pulseDemo,new Vector3(0,0,-.002f),new Vector3(.025f,.025f,.003f),Accent,PrimitiveType.Sphere);
            var stand=new GameObject("AtrilManual").transform; stand.SetParent(root,false); stand.position=new Vector3(-.55f,0,1.9f); stand.rotation=Quaternion.Euler(0,90,0);
            Part("Base",stand,new Vector3(0,.03f,0),new Vector3(.55f,.06f,.4f),Dark).AddComponent<BoxCollider>();
            Part("Columna",stand,new Vector3(0,.46f,.08f),new Vector3(.06f,.88f,.06f),Accent).AddComponent<BoxCollider>();
            Part("Apoyo",stand,new Vector3(0,.895f,0),new Vector3(.56f,.04f,.15f),Dark).AddComponent<BoxCollider>();
            var dock=new GameObject("PosicionManual").transform; dock.SetParent(stand,false); dock.localPosition=new Vector3(0,1.3f,0); manual.dock=dock;
            Button(stand,"RECUPERAR",new Vector3(0,.82f,-.10f),manual.Recover,.30f);
            board.GetComponent<Rigidbody>().useGravity=true;
            manual.RefreshDisplay();
        }
        static void Navigation(Transform root,TrainingFlow flow)
        {
            var p=new GameObject("Recorrido").transform; p.SetParent(root,false); p.position=new Vector3(-.43f,1.55f,-1.20f); p.rotation=Quaternion.Euler(0,90,0);
            Part("Panel",p,Vector3.zero,new Vector3(.98f,.68f,.025f),Dark);
            Text("Titulo",p,new Vector3(0,.28f,-.02f),"APRENDE · PRACTICA · REPITE",.43f,new Vector2(.93f,.07f));
            flow.flowText=Text("PasoActual",p,new Vector3(0,.15f,-.02f),"",.31f,new Vector2(.92f,.19f));
            Button(p,"INICIAR CASO",new Vector3(-.24f,-.015f,0),flow.BeginCase,.43f);
            Button(p,"PRÁCTICA DIRECTA",new Vector3(.24f,-.015f,0),flow.DirectPractice,.43f);
            Button(p,"INICIAR RETO",new Vector3(-.24f,-.12f,0),flow.StartChallenge,.43f);
            Button(p,"MARCADOR SÍ/NO",new Vector3(.24f,-.12f,0),flow.ToggleMarker,.43f);
            Button(p,"REPETIR CASO",new Vector3(-.24f,-.23f,0),flow.ResetCase,.43f);
            Button(p,"RECUPERAR MANUAL",new Vector3(.24f,-.23f,0),flow.manual.Recover,.43f);
        }
        public static void Case(Transform root,TrainingFlow flow,bool contrast)
        {
            var p=new GameObject(contrast?"CasoQueResponde":"CasoColapsoRecepcion").transform; p.SetParent(root,false);
            var patient=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VR/Patients/PacienteRemyAcostado.prefab"));
            patient.name=contrast?"PersonaQueResponde":"AdultoColapsado"; patient.transform.SetParent(p,false);
            patient.transform.position=contrast?new Vector3(11.5f,.1f,1f):new Vector3(3.2f,.1f,-3.75f);
            var bounds=patient.GetComponentsInChildren<Renderer>().First().bounds;
            foreach(var r in patient.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            patient.transform.position+=Vector3.up*(.07f-bounds.min.y);
            var floor=Part("SuperficieFirme",p,new Vector3(patient.transform.position.x,.03f,patient.transform.position.z),new Vector3(1.6f,.04f,2.2f),Accent);
            Text("Identificacion",p,new Vector3(patient.transform.position.x,1.9f,patient.transform.position.z+.7f),contrast?"CASO B · RESPONDE":"RECEPCIÓN UNIVERSITARIA\nCASO A · ADULTO COLAPSADO",.43f,new Vector2(1.6f,.3f));
            if(contrast) {flow.contrastCase=p; p.gameObject.SetActive(false); return;}
            flow.collapseCase=p;
            var panel=new GameObject("Reconocer").transform; panel.SetParent(root,false); panel.position=new Vector3(2.9f,1.38f,-3f);
            Part("Panel",panel,Vector3.zero,new Vector3(1.32f,.62f,.03f),Dark);
            flow.caseText=Text("Caso",panel,new Vector3(0,.15f,-.025f),"",.34f,new Vector2(1.25f,.22f));
            var actions=new GameObject("Acciones").transform; actions.SetParent(panel,false); flow.recognitionControls=actions.gameObject;
            Button(actions,"ENTORNO SEGURO",new Vector3(-.42f,-.08f,0),flow.ObserveSafety,.38f);
            Button(actions,"COMPROBAR RESPUESTA",new Vector3(0,-.08f,0),flow.CheckResponse,.38f);
            Button(actions,"OBSERVAR RESPIRACIÓN",new Vector3(.42f,-.08f,0),flow.CheckBreathing,.38f);
            Text("Alcance",panel,new Vector3(0,-.23f,-.025f),"Acciones de observación simulada · Ve al teléfono para pedir ayuda",.27f,new Vector2(1.25f,.08f));
        }
        static void Phone(Transform root,TrainingFlow flow)
        {
            var p=new GameObject("TelefonoSimulado").transform; p.SetParent(root,false); p.position=new Vector3(1.25f,1.32f,-3f);
            Part("Telefono",p,Vector3.zero,new Vector3(1.10f,.72f,.05f),Dark);
            Part("Auricular",p,new Vector3(-.49f,.13f,-.05f),new Vector3(.05f,.25f,.05f),White);
            flow.phoneText=Text("Llamada",p,new Vector3(.02f,.16f,-.035f),"",.28f,new Vector2(.93f,.31f));
            var controls=new GameObject("PasosLlamada").transform; controls.SetParent(p,false); flow.phoneControls=controls.gameObject;
            Button(controls,"UBICACIÓN",new Vector3(-.28f,-.08f,0),flow.ProvideLocation,.45f);
            Button(controls,"SITUACIÓN",new Vector3(.25f,-.08f,0),flow.DescribeSituation,.45f);
            Button(controls,"TESTIGO + DEA",new Vector3(-.28f,-.20f,0),flow.AskForAed,.45f);
            Button(controls,"CONTINUAR",new Vector3(.25f,-.20f,0),flow.ContinueToPractice,.45f);
        }
    }
}
