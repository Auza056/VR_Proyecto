using System;
using HospitalVR.RCP;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace HospitalVR.Editor
{
    public static class RcpFinalSetup
    {
        public static Transform[] Hands;
        public static Material Dark => AssetDatabase.LoadAssetAtPath<Material>("Assets/VR/Interaction/Materials/TintaClinica.mat");
        public static Material Accent => AssetDatabase.LoadAssetAtPath<Material>("Assets/VR/Interaction/Materials/AcentoClinico.mat");
        public static Material White => AssetDatabase.LoadAssetAtPath<Material>("Assets/VR/Interaction/Materials/Blanco.mat");
        public static string Apply()
        {
            Guard();
            var rig=GameObject.Find("XR Origin (VR)").transform;
            Hands=new[]{rig.Find("Camera Offset/LeftHandController"),rig.Find("Camera Offset/RightHandController")};
            var session=UnityEngine.Object.FindAnyObjectByType<RcpSession>();
            var metro=UnityEngine.Object.FindAnyObjectByType<RcpMetronome>();
            metro.session=session;
            for(int i=0;i<2;i++)
            {
                var h=Hands[i].GetComponent<HapticImpulsePlayer>();
                if(!h) h=Hands[i].gameObject.AddComponent<HapticImpulsePlayer>();
                h.hapticOutput=new XRInputHapticImpulseProvider {inputSourceMode=XRInputHapticImpulseProvider.InputSourceMode.InputAction,
                    inputAction=new InputAction("Pulso "+i,InputActionType.PassThrough,"<XRController>{"+(i==0?"LeftHand":"RightHand")+"}/haptic",expectedControlType:"Haptic")};
                if(i==0) metro.leftHaptics=h; else metro.rightHaptics=h;
                EditorUtility.SetDirty(h);
            }
            var station=session.transform.parent;
            if(station.Find("AjustesRitmo")) UnityEngine.Object.DestroyImmediate(station.Find("AjustesRitmo").gameObject);
            var panel=new GameObject("AjustesRitmo").transform; panel.SetParent(station,false);
            panel.position=new Vector3(-.43f,1.38f,-.25f); panel.rotation=Quaternion.Euler(0,90,0);
            Part("Fondo",panel,Vector3.zero,new Vector3(.76f,.53f,.025f),Dark);
            Text("Titulo",panel,new Vector3(0,.205f,-.02f),"RITMO OBJETIVO · 110/min",.35f,new Vector2(.72f,.06f));
            metro.channelLabel=Text("Canales",panel,new Vector3(0,.13f,-.02f),"Canales independientes",.25f,new Vector2(.72f,.09f));
            Button(panel,"Audio",new Vector3(-.24f,.02f,0),metro.ToggleAudio);
            Button(panel,"Vibración",new Vector3(0,.02f,0),metro.ToggleHaptics);
            Button(panel,"Visual",new Vector3(.24f,.02f,0),metro.ToggleVisual);
            Button(panel,"Intensidad −",new Vector3(-.24f,-.09f,0),metro.IntensityDown);
            Button(panel,"Touch",new Vector3(0,-.09f,0),metro.CycleHand);
            Button(panel,"Intensidad +",new Vector3(.24f,-.09f,0),metro.IntensityUp);
            var pulse=Part("PulsoObjetivo",panel,new Vector3(0,-.20f,-.03f),new Vector3(.065f,.04f,.015f),Accent);
            metro.visualIndicator=pulse.transform;
            Text("PulsoNota",panel,new Vector3(0,-.20f,-.045f),"INICIA EL DESCENSO",.19f,new Vector2(.63f,.04f));
            EditorUtility.SetDirty(metro); Save();
            return "Metrónomo integrado: HapticImpulsePlayer por mando, controles y señal objetivo independientes.";
        }
        public static void Guard()
        {
            if(Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/HospitalWard.unity")
                throw new InvalidOperationException("Abra HospitalWard fuera de Play Mode.");
        }
        public static void Save()
        {
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets();
        }
        public static GameObject Part(string name,Transform parent,Vector3 position,Vector3 scale,Material mat,PrimitiveType shape=PrimitiveType.Cube)
        {
            var go=GameObject.CreatePrimitive(shape); go.name=name; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent,false); go.transform.localPosition=position; go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=mat; return go;
        }
        public static TextMeshPro Text(string name,Transform parent,Vector3 pos,string content,float size,Vector2 bounds)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false); go.transform.localPosition=pos;
            var t=go.AddComponent<TextMeshPro>();
            t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            t.text=content; t.fontSize=size; t.color=Color.white; t.alignment=TextAlignmentOptions.Center;
            t.rectTransform.sizeDelta=bounds; t.textWrappingMode=TextWrappingModes.Normal; return t;
        }
        public static RcpTouchButton Button(Transform parent,string label,Vector3 pos,UnityAction action,float width=.21f)
        {
            var root=new GameObject("Tocar_"+label).transform; root.SetParent(parent,false); root.localPosition=pos;
            var visual=Part("Confirmacion",root,new Vector3(0,0,-.025f),new Vector3(width,.075f,.025f),Accent);
            Text("Texto",root,new Vector3(0,0,-.041f),label,.26f,new Vector2(width-.01f,.065f));
            var b=root.gameObject.AddComponent<RcpTouchButton>(); b.hands=Hands;
            b.triggerCenter=new Vector3(0,0,-.035f); b.triggerSize=new Vector3(width,.075f,.08f);
            b.pressVisual=visual.transform; b.pressDepth=.004f;
            UnityEventTools.AddPersistentListener(b.onPressed,action); return b;
        }
    }
}

