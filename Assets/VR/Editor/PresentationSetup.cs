using System.Collections.Generic;
using System.Linq;
using HospitalVR.Learning;
using HospitalVR.Presentation;
using MenuCommand = HospitalVR.Presentation.MenuCommand;
using HospitalVR.RCP;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using static HospitalVR.Editor.RcpFinalSetup;

namespace HospitalVR.Editor
{
    public static class PresentationSetup
    {
        const string Folder="Assets/VR/Presentation";
        static Material ink,teal,light;
        static List<TrainingMenuButton> commands;
        static TrainingMenu menu;
        static Material Mat(string name,Color color)
        {
            string path=Folder+"/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m) {m=new Material(Shader.Find("Universal Render Pipeline/Unlit"));AssetDatabase.CreateAsset(m,path);}
            m.color=color; return m;
        }
        [MenuItem("Hospital VR/Preparar presentación")]
        public static void ApplyFromMenu()=>Apply();
        public static string Apply()
        {
            Guard();
            ink=Mat("PanelTinta",new Color(.022f,.046f,.065f));
            teal=Mat("BotonTurquesa",new Color(.035f,.32f,.33f));
            light=Mat("BordeClaro",new Color(.15f,.70f,.64f));
            var flow=UnityEngine.Object.FindAnyObjectByType<TrainingFlow>();
            Hands=new[]{flow.session.detector.leftHand,flow.session.detector.rightHand};
            var p=flow.transform.Find("Recorrido");
            foreach(var child in p.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
            menu=p.GetComponent<TrainingMenu>(); if(!menu)menu=p.gameObject.AddComponent<TrainingMenu>(); menu.flow=flow;
            commands=new List<TrainingMenuButton>();
            p.position=new Vector3(-.43f,1.47f,-1.22f);p.rotation=Quaternion.Euler(0,90,0);
            Part("Marco",p,Vector3.zero,new Vector3(1.29f,1.18f,.045f),light);
            Part("Panel",p,new Vector3(0,0,-.012f),new Vector3(1.27f,1.16f,.03f),ink);
            Label(p,"Marca",new Vector3(0,.49f,-.038f),"APRENDE A ACTUAR",.65f,new Vector2(1.17f,.10f));
            menu.subtitle=Label(p,"Subtitulo",new Vector3(0,.405f,-.038f),"",.25f,new Vector2(1.17f,.055f));
            Cmd(p,MenuCommand.Home,"INICIO",new Vector3(-.40f,.30f,0),.37f);
            Cmd(p,MenuCommand.Settings,"AJUSTES",new Vector3(0,.30f,0),.37f);
            Cmd(p,MenuCommand.Results,"RESULTADOS",new Vector3(.40f,.30f,0),.37f);
            menu.steps=new TMP_Text[6];
            for(int i=0;i<6;i++) menu.steps[i]=Label(p,"Paso"+i,new Vector3(-.49f+i*.196f,.20f,-.04f),"",.19f,new Vector2(.19f,.065f));
            var home=Page(p,"Inicio");menu.homePage=home.gameObject;
            flow.flowText=Label(home,"PasoActual",new Vector3(0,.07f,-.04f),"",.34f,new Vector2(1.14f,.14f));
            Cmd(home,MenuCommand.StartCase,"INICIAR CASO",new Vector3(-.30f,-.065f,0),.55f);
            Cmd(home,MenuCommand.Practice,"PRÁCTICA DIRECTA",new Vector3(.30f,-.065f,0),.55f);
            Cmd(home,MenuCommand.Challenge,"INICIAR RETO",new Vector3(-.30f,-.18f,0),.55f);
            Cmd(home,MenuCommand.Finish,"FINALIZAR INTENTO",new Vector3(.30f,-.18f,0),.55f);
            Label(home,"AyudaInicio",new Vector3(0,-.285f,-.04f),"Caso: recepción del pasillo. Práctica: camilla con zona azul.\nEl reto se habilita después de terminar la práctica.",.27f,new Vector2(1.15f,.09f));
            var settings=Page(p,"Ajustes"); menu.settingsPage=settings.gameObject;
            menu.settingsText=Label(settings,"EstadoAjustes",new Vector3(0,.08f,-.04f),"",.29f,new Vector2(1.15f,.09f));
            Cmd(settings,MenuCommand.Duration30,"DEMO · 30 s",new Vector3(-.30f,-.02f,0),.55f);
            Cmd(settings,MenuCommand.Duration60,"PRÁCTICA · 60 s",new Vector3(.30f,-.02f,0),.55f);
            Cmd(settings,MenuCommand.Audio,"AUDIO",new Vector3(-.40f,-.12f,0),.36f);
            Cmd(settings,MenuCommand.Haptics,"VIBRACIÓN",new Vector3(0,-.12f,0),.36f);
            Cmd(settings,MenuCommand.Visual,"PULSO VISUAL",new Vector3(.40f,-.12f,0),.36f);
            Cmd(settings,MenuCommand.VolumeDown,"VOLUMEN −",new Vector3(-.40f,-.22f,0),.36f);
            Cmd(settings,MenuCommand.Hand,"CAMBIAR TOUCH",new Vector3(0,-.22f,0),.36f);
            Cmd(settings,MenuCommand.VolumeUp,"VOLUMEN +",new Vector3(.40f,-.22f,0),.36f);
            Cmd(settings,MenuCommand.IntensityDown,"INTENSIDAD −",new Vector3(-.40f,-.32f,0),.36f);
            Cmd(settings,MenuCommand.Marker,"MARCADOR",new Vector3(0,-.32f,0),.36f);
            Cmd(settings,MenuCommand.IntensityUp,"INTENSIDAD +",new Vector3(.40f,-.32f,0),.36f);
            var results=Page(p,"Resultados");menu.resultsPage=results.gameObject;
            Label(results,"TituloPractica",new Vector3(-.30f,.10f,-.04f),"PRÁCTICA GUIADA",.32f,new Vector2(.56f,.06f));
            Label(results,"TituloReto",new Vector3(.30f,.10f,-.04f),"RETO",.32f,new Vector2(.56f,.06f));
            menu.practiceText=Label(results,"DatosPractica",new Vector3(-.30f,-.09f,-.04f),"",.26f,new Vector2(.55f,.31f));menu.practiceText.alignment=TextAlignmentOptions.TopLeft;
            menu.challengeText=Label(results,"DatosReto",new Vector3(.30f,-.09f,-.04f),"",.26f,new Vector2(.55f,.31f));menu.challengeText.alignment=TextAlignmentOptions.TopLeft;
            menu.comparisonText=Label(results,"Comparacion",new Vector3(0,-.30f,-.04f),"",.23f,new Vector2(1.15f,.09f));
            menu.messageText=Label(p,"Mensaje",new Vector3(0,-.385f,-.04f),"",.24f,new Vector2(1.14f,.075f));
            Cmd(p,MenuCommand.RecoverManual,"RECUPERAR MANUAL",new Vector3(-.30f,-.48f,0),.55f);
            Cmd(p,MenuCommand.Reset,"REINICIAR RECORRIDO",new Vector3(.30f,-.48f,0),.55f);
            Label(p,"Alcance",new Vector3(0,-.557f,-.04f),"Simulación educativa · No mide fuerza, profundidad real ni competencia clínica.",.20f,new Vector2(1.17f,.04f));
            menu.buttons=commands.ToArray();
            foreach(var b in menu.buttons)
                if(b.command==MenuCommand.StartCase) b.name="Tocar_INICIAR CASO";
            RhythmSign(flow);
            StationScreen(flow);
            PolishSurfaces(flow);
            Checklists(flow);
            Wayfinding(flow);
            flow.Refresh(); menu.RefreshNow();
            var rig=flow.session.detector.rigRoot;
            rig.SetPositionAndRotation(new Vector3(-1.8f,0,-1.22f),Quaternion.Euler(0,90,0));
            EditorUtility.SetDirty(rig);
            EditorUtility.SetDirty(flow);EditorUtility.SetDirty(menu);
            PrefabUtility.SaveAsPrefabAsset(menu.gameObject,Folder+"/MenuAprendizaje.prefab");
            Save(); return "Menú de tres páginas, comparación de resultados, ajustes, señalización y botones integrados.";
        }
        static Transform Page(Transform p,string name) {var t=new GameObject(name).transform;t.SetParent(p,false);return t;}
        static TextMeshPro Label(Transform p,string name,Vector3 pos,string value,float size,Vector2 bounds)
        {
            var t=Text(name,p,pos,value,size,bounds); t.enableAutoSizing=true;t.fontSizeMin=size*.82f;t.fontSizeMax=size;
            t.overflowMode=TextOverflowModes.Ellipsis;return t;
        }
        static void Cmd(Transform p,MenuCommand command,string caption,Vector3 pos,float width)
        {
            var b=Button(p,caption,pos,menu.RefreshNow,width); b.onPressed=new UnityEngine.Events.UnityEvent();
            var link=b.gameObject.AddComponent<TrainingMenuButton>();link.menu=menu;link.command=command;link.touch=b;link.caption=caption;link.label=b.GetComponentInChildren<TMP_Text>();
            link.label.enableAutoSizing=true;link.label.fontSizeMin=.22f;link.label.fontSizeMax=.33f;
            b.triggerSize=new Vector3(width,.078f,.075f);b.pressDirection=Vector3.forward;
            b.feedbackRenderer=b.pressVisual.GetComponent<Renderer>();b.feedbackRenderer.sharedMaterial=teal;
            var v=UnityEngine.Object.FindAnyObjectByType<RcpPanelView>();b.feedbackSource=v.feedbackSource;
            var existing=UnityEngine.Object.FindObjectsByType<RcpTouchButton>(FindObjectsInactive.Include).FirstOrDefault(x=>x.pressClip);
            if(existing)b.pressClip=existing.pressClip;
            UnityEventTools.AddPersistentListener(b.onPressed,link.Execute);commands.Add(link);
        }
        static void RhythmSign(TrainingFlow flow)
        {
            var old=flow.session.transform.parent.Find("AjustesRitmo");
            var sign=flow.session.transform.parent.Find("PulsoRitmo");if(sign)UnityEngine.Object.DestroyImmediate(sign.gameObject);
            sign=new GameObject("PulsoRitmo").transform;sign.SetParent(flow.session.transform.parent,false);sign.position=new Vector3(-.43f,1.35f,-.25f);sign.rotation=Quaternion.Euler(0,90,0);
            Part("Fondo",sign,Vector3.zero,new Vector3(.45f,.29f,.025f),ink);
            Label(sign,"Titulo",new Vector3(0,.095f,-.02f),"PULSO OBJETIVO",.31f,new Vector2(.41f,.06f));
            flow.metronome.visualIndicator=Part("Pulso",sign,new Vector3(0,.005f,-.025f),new Vector3(.10f,.045f,.012f),light).transform;
            Label(sign,"Nota",new Vector3(0,-.09f,-.02f),"Inicia el descenso.\nAjustes en el menú.",.25f,new Vector2(.41f,.08f));
            flow.metronome.channelLabel=null;if(old)old.gameObject.SetActive(false);EditorUtility.SetDirty(flow.metronome);
        }
        static void StationScreen(TrainingFlow flow)
        {
            var root=flow.session.transform.parent.Find("PantallaRCP/Panel");
            root.localPosition=new Vector3(0,1.50f,0);
            root.Find("Fondo").localScale=new Vector3(1.10f,.95f,.035f);
            var view=flow.rcpPanel;
            Format(view.stateText,new Vector3(0,.37f,-.025f),new Vector2(1.02f,.12f),.62f);
            Format(view.metricsText,new Vector3(0,.06f,-.025f),new Vector2(1.02f,.43f),.35f);
            Format(view.hintText,new Vector3(0,-.255f,-.025f),new Vector2(1.02f,.15f),.33f);
            Format(view.noteText,new Vector3(0,-.405f,-.025f),new Vector2(1.02f,.10f),.24f);
            root.Find("Fondo").GetComponent<Renderer>().sharedMaterial=ink;
            foreach(var x in new[]{view.stateText,view.metricsText,view.hintText,view.noteText})EditorUtility.SetDirty(x);
        }
        static void Format(TMP_Text t,Vector3 pos,Vector2 bounds,float size)
        {
            t.transform.localPosition=pos;t.rectTransform.sizeDelta=bounds;t.fontSize=size;t.enableAutoSizing=true;t.fontSizeMin=size*.78f;t.fontSizeMax=size;
            t.overflowMode=TextOverflowModes.Ellipsis;t.alignment=TextAlignmentOptions.Center;
        }
        static void PolishSurfaces(TrainingFlow flow)
        {
            foreach(var b in UnityEngine.Object.FindObjectsByType<RcpTouchButton>(FindObjectsInactive.Include))
            {
                if(b.transform.IsChildOf(menu.transform))continue;
                b.feedbackRenderer=b.pressVisual?b.pressVisual.GetComponent<Renderer>():null;
                if(b.feedbackRenderer)b.feedbackRenderer.sharedMaterial=teal;
                if(b.name.StartsWith("Tocar_"))b.pressDirection=Vector3.forward;
                EditorUtility.SetDirty(b);
            }
            foreach(string path in new[]{"Reconocer/Panel","TelefonoSimulado/Telefono","CasoB_Interacciones/Panel","DEA_Simulado/Indicaciones/Fondo"})
            {var t=flow.transform.Find(path);if(t)t.GetComponent<Renderer>().sharedMaterial=ink;}
            foreach(var t in new[]{flow.caseText,flow.phoneText,flow.contrastFeedback})
            {t.enableAutoSizing=true;t.fontSizeMin=.24f;t.fontSizeMax=.38f;t.overflowMode=TextOverflowModes.Ellipsis;EditorUtility.SetDirty(t);}
        }
        static void Wayfinding(TrainingFlow flow)
        {
            var old=flow.transform.Find("OrientacionDemo");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var guide=new GameObject("OrientacionDemo").transform;guide.SetParent(flow.transform,false);
            var card=new GameObject("Bienvenida").transform;card.SetParent(guide,false);card.position=new Vector3(1.6f,1.85f,-3f);
            Part("Fondo",card,Vector3.zero,new Vector3(1.3f,.24f,.018f),ink);
            Label(card,"Ruta",new Vector3(0,0,-.015f),"RECEPCIÓN SIMULADA\n01 Reconoce  /  02 Pide ayuda  /  03 Practica en habitación 1",.30f,new Vector2(1.24f,.19f));
            var local=Label(guide,"ConsultaManual",new Vector3(-.58f,1.86f,1.90f),"MANUAL ILUSTRADO\nAgarrar: grip · Botones: tocar",.30f,new Vector2(.65f,.15f));local.transform.rotation=Quaternion.Euler(0,90,0);
        }
        static void Checklists(TrainingFlow flow)
        {
            foreach(var b in flow.GetComponentsInChildren<RcpTouchButton>(true))
            {
                if(b.transform.IsChildOf(menu.transform) || b.onPressed.GetPersistentEventCount()==0)continue;
                string method=b.onPressed.GetPersistentMethodName(0);
                ChecklistStep step;
                switch(method)
                {
                    case "ObserveSafety":step=ChecklistStep.Safety;break;
                    case "CheckResponse":step=ChecklistStep.Response;break;
                    case "CheckBreathing":step=ChecklistStep.Breathing;break;
                    case "ProvideLocation":step=ChecklistStep.Location;break;
                    case "DescribeSituation":step=ChecklistStep.Situation;break;
                    case "AskForAed":step=ChecklistStep.Aed;break;
                    case "ContinueToPractice":step=ChecklistStep.Continue;break;
                    default:continue;
                }
                var c=b.GetComponent<TrainingChecklist>();if(!c)c=b.gameObject.AddComponent<TrainingChecklist>();
                c.flow=flow;c.touch=b;c.step=step;c.label=b.GetComponentInChildren<TMP_Text>(true);
                if(string.IsNullOrEmpty(c.caption))c.caption=c.label.text;
                c.contrastOnly=b.transform.IsChildOf(flow.transform.Find("CasoB_Interacciones"));
                c.label.enableAutoSizing=true;c.label.fontSizeMin=.19f;c.label.fontSizeMax=.31f;
                EditorUtility.SetDirty(c);
            }
            var aed=flow.GetComponentInChildren<TrainingAed>(true);
            for(int i=0;i<aed.targets.Length;i++)
            {
                var t=aed.targets[i];var old=t.Find("NumeroColocacion");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
                var n=Label(t,"NumeroColocacion",new Vector3(0,.008f,0),(i+1).ToString(),.32f,new Vector2(.07f,.09f));
                n.transform.localRotation=Quaternion.Euler(90,0,0);
            }
        }
    }
}
