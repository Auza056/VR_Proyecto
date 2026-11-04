using System.Linq;
using HospitalVR.Learning;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using static HospitalVR.Editor.RcpFinalSetup;
namespace HospitalVR.Editor
{
    public static class TrainingExtensionsSetup
    {
        public static string Apply()
        {
            Guard();
            var flow=UnityEngine.Object.FindAnyObjectByType<TrainingFlow>(); var root=flow.transform;
            Hands=new[]{flow.session.detector.leftHand,flow.session.detector.rightHand};
            PolishManual(flow);
            if(!root.Find("DEA_Simulado")) Aed(root,flow);
            if(!flow.contrastCase) Contrast(root,flow);
            flow.contrastFeedback=root.Find("CasoB_Interacciones/Titulo").GetComponent<TMPro.TMP_Text>();
            flow.Refresh(); Save();
            PrefabUtility.SaveAsPrefabAsset(flow.manual.gameObject,"Assets/VR/Learning/ManualIlustrado.prefab");
            return "DEA y contraste integrados; manual de seis páginas guardado.";
        }
        static void PolishManual(TrainingFlow flow)
        {
            var manual=flow.manual;
            manual.body.fontSize=.27f; manual.body.rectTransform.sizeDelta=new Vector2(.47f,.23f);
            manual.body.transform.localPosition=new Vector3(0,-.055f,-.025f); manual.body.alignment=TMPro.TextAlignmentOptions.Top;
            manual.confirmation.fontSize=.18f; manual.footer.fontSize=.18f;
            manual.title.fontSize=.45f; manual.title.enableAutoSizing=true; manual.title.fontSizeMin=.29f; manual.title.fontSizeMax=.45f;
            var lines=AssetDatabase.LoadAssetAtPath<Material>("Assets/VR/Learning/TrazoManual.mat");
            if(!lines) { lines=new Material(Shader.Find("Universal Render Pipeline/Unlit")); lines.color=Color.white; AssetDatabase.CreateAsset(lines,"Assets/VR/Learning/TrazoManual.mat"); }
            foreach(var line in manual.GetComponentsInChildren<LineRenderer>(true)) line.sharedMaterial=lines;
            if(flow.content.pages.Length==5)
            {
                var pages=flow.content.pages.ToList();
                pages.Add(new ManualPage{title="6 · DEA simulado",illustration=5,instruction="Enciende y sigue al DEA. Coloca ambos parches en el tórax descubierto según el dibujo.\nNadie toca durante análisis o descarga. Reanuda RCP cuando lo indique.\nGuion simulado: revisar con instructor."});
                flow.content.pages=pages.ToArray();
                manual.illustrations=manual.illustrations.Concat(new[]{ManualIllustrations.Make(manual.transform,5)}).ToArray();
                foreach(var line in manual.illustrations[5].GetComponentsInChildren<LineRenderer>()) line.sharedMaterial=lines;
            }
            manual.RefreshDisplay(); EditorUtility.SetDirty(flow.content); EditorUtility.SetDirty(manual);
        }
        static void Aed(Transform root,TrainingFlow flow)
        {
            var stand=new GameObject("SoporteDEA").transform; stand.SetParent(root,false); stand.position=new Vector3(.3f,0,-3.05f);
            Part("Base",stand,new Vector3(0,.035f,0),new Vector3(.55f,.07f,.5f),Dark).AddComponent<BoxCollider>();
            Part("Columna",stand,new Vector3(0,.5f,0),new Vector3(.07f,1f,.07f),Accent).AddComponent<BoxCollider>();
            Part("Mesa",stand,new Vector3(0,.98f,0),new Vector3(.75f,.04f,.5f),Dark).AddComponent<BoxCollider>();
            var unit=new GameObject("DEA_Simulado").transform; unit.SetParent(root,false); unit.position=new Vector3(.3f,1.08f,-3.05f);
            Part("Carcasa",unit,Vector3.zero,new Vector3(.34f,.15f,.25f),Accent);
            Grab(unit.gameObject,new Vector3(.35f,.16f,.26f),.6f);
            unit.gameObject.AddComponent<TrainingRecoverable>();
            var aed=unit.gameObject.AddComponent<TrainingAed>(); aed.hands=Hands;
            var ui=new GameObject("Indicaciones").transform; ui.SetParent(unit,false); ui.localPosition=new Vector3(0,.34f,0);
            Part("Fondo",ui,Vector3.zero,new Vector3(.72f,.48f,.02f),Dark);
            aed.message=Text("Mensajes",ui,new Vector3(0,.065f,-.02f),"",.26f,new Vector2(.67f,.31f));
            Button(ui,"ENCENDER",new Vector3(-.18f,-.16f,0),aed.PowerOn,.30f);
            Button(ui,"DESCARGA",new Vector3(.18f,-.16f,0),aed.Shock,.30f);
            var patient=flow.collapseCase.Find("AdultoColapsado");
            aed.chest=new GameObject("ToraxDescubiertoEsquema").transform; aed.chest.SetParent(patient,false); aed.chest.position=patient.position+new Vector3(0,.25f,.38f);
            var skin=new Material(Shader.Find("Universal Render Pipeline/Lit")); skin.color=new Color(.64f,.40f,.27f); AssetDatabase.CreateAsset(skin,"Assets/VR/Learning/PielReferencia.mat");
            Part("ToraxDeReferencia",aed.chest,Vector3.zero,new Vector3(.48f,.016f,.60f),skin);
            var info=Text("Esquema",aed.chest,new Vector3(0,.012f,-.1f),"TÓRAX VIRTUAL\nDESCUBIERTO",.19f,new Vector2(.28f,.12f)); info.transform.localRotation=Quaternion.Euler(90,0,0);
            aed.targets=new Transform[2]; aed.pads=new XRGrabInteractable[2];
            for(int i=0;i<2;i++)
            {
                var target=new GameObject("DestinoParche"+(i+1)).transform; target.SetParent(aed.chest,false);
                target.localPosition=i==0?new Vector3(-.13f,.018f,.17f):new Vector3(.17f,.018f,-.16f); aed.targets[i]=target;
                Part("Guia",target,Vector3.zero,new Vector3(.095f,.008f,.13f),Accent);
                var pad=new GameObject("ParcheDEA_"+(i+1)).transform; pad.SetParent(root,false); pad.position=new Vector3(.1f+i*.38f,1.025f,-3.23f);
                Part("Parche",pad,Vector3.zero,new Vector3(.09f,.008f,.12f),White);
                var label=Text("Numero",pad,new Vector3(0,.008f,0),(i+1).ToString(),.32f,new Vector2(.07f,.09f)); label.color=Color.black; label.transform.localRotation=Quaternion.Euler(90,0,0);
                Grab(pad.gameObject,new Vector3(.10f,.02f,.13f),.03f); pad.gameObject.AddComponent<TrainingRecoverable>();
                aed.pads[i]=pad.GetComponent<XRGrabInteractable>();
            }
            Button(stand,"REINICIAR DEA",new Vector3(0,.85f,-.27f),aed.ResetDevice,.40f);
        }
        static void Grab(GameObject go,Vector3 size,float mass)
        {
            go.AddComponent<BoxCollider>().size=size;
            var rb=go.AddComponent<Rigidbody>(); rb.mass=mass; rb.interpolation=RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            var grab=go.AddComponent<XRGrabInteractable>(); grab.interactionManager=UnityEngine.Object.FindAnyObjectByType<XRInteractionManager>();
            grab.useDynamicAttach=true; grab.movementType=XRBaseInteractable.MovementType.VelocityTracking;
        }
        static void Contrast(Transform root,TrainingFlow flow)
        {
            TrainingSetup.Case(root,flow,true);
            var p=new GameObject("CasoB_Interacciones").transform; p.SetParent(root,false); p.position=new Vector3(11.4f,1.4f,.5f); p.rotation=Quaternion.Euler(0,90,0);
            Part("Panel",p,Vector3.zero,new Vector3(1.10f,.68f,.03f),Dark);
            Text("Titulo",p,new Vector3(0,.23f,-.025f),"CASO B · PERSONA QUE RESPONDE\nHabla, respira normalmente, pide ayuda.\nObserva y elige ayuda sin iniciar compresiones.",.29f,new Vector2(1.04f,.20f));
            Button(p,"ABRIR CASO B",new Vector3(0,.055f,0),flow.BeginContrast,.80f);
            Button(p,"ENTORNO",new Vector3(-.34f,-.055f,0),flow.ObserveSafety,.30f);
            Button(p,"RESPUESTA",new Vector3(0,-.055f,0),flow.CheckResponse,.30f);
            Button(p,"RESPIRACIÓN",new Vector3(.34f,-.055f,0),flow.CheckBreathing,.30f);
            Button(p,"UBICACIÓN",new Vector3(-.34f,-.165f,0),flow.ProvideLocation,.30f);
            Button(p,"SITUACIÓN",new Vector3(0,-.165f,0),flow.DescribeSituation,.30f);
            Button(p,"PEDIR AYUDA",new Vector3(.34f,-.165f,0),flow.ContinueToPractice,.30f);
            Text("Nota",p,new Vector3(0,-.28f,-.025f),"Sin respuesta o respiración anormal: el caso A. Este caso B no inicia RCP.",.23f,new Vector2(1.04f,.06f));
        }
    }
}
