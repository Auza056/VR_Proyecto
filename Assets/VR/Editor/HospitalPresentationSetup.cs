using HospitalVR.Interaction;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

namespace HospitalVR.Editor
{
    public static class HospitalPresentationSetup
    {
        const string Folder = "Assets/VR/Interaction";

        public static string Apply()
        {
            Configure();
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets();
            return "Saved: footsteps, remote click and rounded casing, four mounted hospital identification plaques.";
        }

        public static void Configure()
        {
            if(Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/HospitalWard.unity")
                throw new System.InvalidOperationException("Open HospitalWard in Edit Mode first.");
            var white = Mat("CeramicaClinica",new Color(.9f,.94f,.94f));
            var ink = Mat("TintaClinica",new Color(.035f,.10f,.13f));
            var teal = Mat("AcentoClinico",new Color(.025f,.38f,.38f));
            var metal = Mat("AceroSatinado",new Color(.50f,.56f,.59f));
            metal.SetFloat("_Metallic",.7f); metal.SetFloat("_Smoothness",.48f);
            var rubber = Mat("GomaControl",new Color(.035f,.045f,.055f));
            rubber.SetFloat("_Smoothness",.15f);
            string[] names={"Samuel","Alejandro","Vicente","Alexander"};
            for(int i=0;i<4;i++)
            {
                var room=GameObject.Find("Habitacion_"+(i+1).ToString("00")).transform;
                var plaque=room.Find("NombrePaciente");
                ClearChildren(plaque);
                // Mount against the outside of the existing footboard, rather than above the mattress.
                plaque.localPosition=new Vector3(.15f,.52f,-.585f);
                plaque.localRotation=Quaternion.identity; plaque.localScale=Vector3.one;
                Part("MarcoAcero",plaque,Vector3.zero,new Vector3(.56f,.235f,.018f),metal);
                Part("TarjetaBlanca",plaque,new Vector3(0,0,-.011f),new Vector3(.535f,.212f,.006f),white);
                Part("BandaIdentificacion",plaque,new Vector3(0,.075f,-.015f),new Vector3(.535f,.055f,.003f),teal);
                Label("Encabezado",plaque,new Vector3(0,.075f,-.018f),"IDENTIFICACION DEL PACIENTE",.19f,new Vector2(.50f,.04f),Color.white);
                Label("Nombre",plaque,new Vector3(0,.006f,-.018f),names[i],.83f,new Vector2(.50f,.10f),ink.color);
                Label("Cama",plaque,new Vector3(0,-.071f,-.018f),"HAB. "+(i+1).ToString("00")+"   /   CAMA "+(i+1).ToString("00"),.23f,new Vector2(.5f,.04f),teal.color);
                for(int side=-1;side<=1;side+=2)
                {
                    Part("Soporte_"+side,plaque,new Vector3(side*.19f,.095f,.025f),new Vector3(.022f,.12f,.025f),metal);
                    Part("Abrazadera_"+side,plaque,new Vector3(side*.19f,.15f,.055f),new Vector3(.026f,.025f,.085f),metal);
                    var screw=Part("Tornillo_"+side,plaque,new Vector3(side*.25f,0,-.015f),new Vector3(.008f,.008f,.004f),metal,PrimitiveType.Sphere);
                }
            }
            var remote=UnityEngine.Object.FindAnyObjectByType<LightRemote>();
            var root=remote.transform;
            // Preserve the root grab, collider, Rigidbody and ray origin while replacing only the visuals.
            foreach(var name in new[]{"Carcasa","BotonCentral","EmisorFrontal","DisenoControl"})
            {
                var old=root.Find(name); if(old) UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            var design=new GameObject("DisenoControl").transform; design.SetParent(root,false);
            MeshPart("CarcasaRedondeada",design,RoundedBody(),white);
            Part("BaseAntideslizante",design,new Vector3(0,-.014f,0),new Vector3(.048f,.004f,.132f),rubber);
            Part("EmisorFrontal",design,new Vector3(0,0,.079f),new Vector3(.028f,.014f,.004f),rubber);
            var ring=Part("AroBoton",design,new Vector3(0,.0168f,0),new Vector3(.034f,.002f,.034f),metal,PrimitiveType.Cylinder);
            var buttonRoot=new GameObject("BotonCentral").transform; buttonRoot.SetParent(design,false); buttonRoot.localPosition=new Vector3(0,.020f,0);
            Part("Pulsador",buttonRoot,Vector3.zero,new Vector3(.029f,.0035f,.029f),teal,PrimitiveType.Cylinder);
            var icon=new GameObject("SimboloApagado").AddComponent<LineRenderer>(); icon.transform.SetParent(buttonRoot,false);
            icon.useWorldSpace=false; icon.positionCount=29; icon.startWidth=icon.endWidth=.0012f;
            icon.sharedMaterial=white; icon.numCapVertices=4;
            for(int i=0;i<29;i++) {float a=(40+i*280f/28)*Mathf.Deg2Rad; icon.SetPosition(i,new Vector3(Mathf.Sin(a)*.007f,.0038f,Mathf.Cos(a)*.007f));}
            Part("TrazoSimbolo",buttonRoot,new Vector3(0,.0038f,.006f),new Vector3(.0013f,.0007f,.008f),white);
            var mark=Label("EtiquetaLuz",design,new Vector3(0,.0163f,-.043f),"LUZ",.055f,new Vector2(.04f,.015f),ink.color);
            mark.transform.localRotation=Quaternion.Euler(90,0,0);
            remote.buttonVisual=buttonRoot; remote.pressDepth=.002f;
            var audio=remote.GetComponent<AudioSource>(); if(!audio) audio=remote.gameObject.AddComponent<AudioSource>();
            audio.playOnAwake=false; audio.loop=false; audio.spatialBlend=1; audio.minDistance=.5f; audio.maxDistance=6;
            audio.rolloffMode=AudioRolloffMode.Linear; audio.volume=.4f; audio.dopplerLevel=0;
            remote.buttonAudio=audio;
            remote.clickClip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/RPG and Interface/kenney_interface-sounds/Audio/click_001.ogg");
            var rig=GameObject.Find("XR Origin (VR)");
            var steps=rig.GetComponent<PlayerFootsteps>(); if(!steps) steps=rig.AddComponent<PlayerFootsteps>();
            var sourceTransform=rig.transform.Find("SonidoPasos");
            if(!sourceTransform) {sourceTransform=new GameObject("SonidoPasos").transform; sourceTransform.SetParent(rig.transform,false);}
            sourceTransform.localPosition=new Vector3(0,.08f,0);
            var source=sourceTransform.GetComponent<AudioSource>(); if(!source) source=sourceTransform.gameObject.AddComponent<AudioSource>();
            source.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/Sonidos de acciones/freesound_community-indoor-footsteps-6385.mp3");
            source.playOnAwake=false; source.loop=true; source.spatialBlend=1; source.minDistance=2; source.maxDistance=8;
            source.dopplerLevel=0; source.volume=0;
            steps.source=source; steps.volume=.35f;
            EditorUtility.SetDirty(steps); EditorUtility.SetDirty(remote);
        }

        static Mesh RoundedBody()
        {
            var path=Folder+"/CarcasaRedondeada.asset";
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path); if(existing) return existing;
            var vertices=new List<Vector3>(); var triangles=new List<int>();
            float[] y={-.016f,-.012f,.011f,.016f}; float[] inset={.005f,0,0,.005f};
            const int perCorner=9; const int count=perCorner*4;
            for(int ring=0;ring<4;ring++)
                for(int corner=0;corner<4;corner++) for(int j=0;j<perCorner;j++)
                {
                    float angle=(corner*90+j*90f/(perCorner-1))*Mathf.Deg2Rad;
                    float radius=.012f-inset[ring];
                    float x=(corner==0||corner==3?1:-1)*(.0325f-.012f);
                    float z=(corner<2?1:-1)*(.08f-.012f);
                    vertices.Add(new Vector3(x+Mathf.Cos(angle)*radius,y[ring],z+Mathf.Sin(angle)*radius));
                }
            for(int ring=0;ring<3;ring++) for(int j=0;j<count;j++)
            {
                int a=ring*count+j,b=ring*count+(j+1)%count,c=(ring+1)*count+j,d=(ring+1)*count+(j+1)%count;
                triangles.AddRange(new[]{a,c,b,b,c,d});
            }
            int bottom=vertices.Count; vertices.Add(new Vector3(0,y[0],0)); int top=vertices.Count; vertices.Add(new Vector3(0,y[3],0));
            for(int j=0;j<count;j++) {triangles.AddRange(new[]{bottom,j,(j+1)%count,top,3*count+(j+1)%count,3*count+j});}
            var mesh=new Mesh{name="Carcasa biselada control hospital"}; mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,path); return mesh;
        }
        static Material Mat(string name,Color color)
        {
            var path=Folder+"/Materials/"+name+".mat"; var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!mat) {mat=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat,path);}
            mat.color=color; return mat;
        }
        static void ClearChildren(Transform root) {for(int i=root.childCount-1;i>=0;i--) UnityEngine.Object.DestroyImmediate(root.GetChild(i).gameObject);}
        static GameObject Part(string name,Transform parent,Vector3 p,Vector3 scale,Material mat,PrimitiveType type=PrimitiveType.Cube)
        {
            var go=GameObject.CreatePrimitive(type); go.name=name; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent,false); go.transform.localPosition=p; go.transform.localScale=scale; go.GetComponent<Renderer>().sharedMaterial=mat; return go;
        }
        static void MeshPart(string name,Transform parent,Mesh mesh,Material mat)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer)); go.transform.SetParent(parent,false);
            go.GetComponent<MeshFilter>().sharedMesh=mesh; go.GetComponent<Renderer>().sharedMaterial=mat;
        }
        static TextMeshPro Label(string name,Transform parent,Vector3 p,string value,float size,Vector2 bounds,Color color)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false); go.transform.localPosition=p;
            var text=go.AddComponent<TextMeshPro>(); text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            text.text=value; text.fontSize=size; text.alignment=TextAlignmentOptions.Center; text.color=color;
            text.textWrappingMode=TextWrappingModes.NoWrap; text.rectTransform.sizeDelta=bounds; text.ForceMeshUpdate(); return text;
        }
    }
}
