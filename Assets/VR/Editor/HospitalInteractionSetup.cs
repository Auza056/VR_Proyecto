using System;
using HospitalVR.Interaction;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace HospitalVR.Editor
{
    public static class HospitalInteractionSetup
    {
        const string Materials = "Assets/VR/Interaction/Materials";

        public static string Apply()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/HospitalWard.unity")
                throw new InvalidOperationException("Open HospitalWard first.");
            System.IO.Directory.CreateDirectory(Materials);
            AssetDatabase.Refresh();
            var dark = Material("Grafito", new Color(.035f,.055f,.075f));
            var white = Material("Blanco", new Color(.88f,.94f,.95f));
            var teal = Material("Boton", new Color(.05f,.65f,.55f));
            var off = Material("LamparaApagada", new Color(.12f,.14f,.16f));
            var on = Material("LamparaEncendida", new Color(.82f,.95f,1f));
            on.EnableKeyword("_EMISSION");
            on.SetColor("_EmissionColor", new Color(.82f,.95f,1f)*2f);
            var manager = UnityEngine.Object.FindAnyObjectByType<XRInteractionManager>();
            if (!manager) manager = new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
            var rig = GameObject.Find("XR Origin (VR)");
            foreach (var side in new[]{"Left","Right"})
            {
                var controller = rig.transform.Find("Camera Offset/"+side+"HandController");
                var direct = controller.Find("AgarreDirecto");
                if (direct) continue;
                var go = new GameObject("AgarreDirecto");
                go.transform.SetParent(controller, false);
                go.transform.localPosition = new Vector3(0,-.025f,.04f);
                var sphere = go.AddComponent<SphereCollider>();
                sphere.radius = .10f; sphere.isTrigger = true;
                var interactor = go.AddComponent<XRDirectInteractor>();
                interactor.interactionManager = manager;
                interactor.improveAccuracyWithSphereCollider = true;
                interactor.handedness = side == "Left" ? InteractorHandedness.Left : InteractorHandedness.Right;
                interactor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.State;
                interactor.selectInput = Reader(side,"gripPressed","grip");
                interactor.activateInput = Reader(side,"triggerPressed","trigger");
                EditorUtility.SetDirty(interactor);
            }
            string[] names = {"Samuel","Alejandro","Vicente","Alexander"};
            for (int i=0;i<4;i++)
            {
                var room = GameObject.Find("Habitacion_"+(i+1).ToString("00")).transform;
                var commode = room.Find("02_Mobiliario/MesitaCabecera/Commode").GetComponent<Renderer>().bounds;
                if (!room.Find("NombrePaciente"))
                {
                    var label = new GameObject("NombrePaciente"); label.transform.SetParent(room,false);
                    label.transform.localPosition = new Vector3(.15f,1.02f,-.68f);
                    Part("Placa",label.transform,Vector3.zero,new Vector3(.72f,.22f,.025f),dark);
                    Text("Nombre",label.transform,new Vector3(0,0,-.016f),names[i],1.1f,new Vector2(.69f,.19f));
                }
                var nameText=room.Find("NombrePaciente/Nombre").GetComponent<TextMeshPro>();
                nameText.fontSize=1.1f; nameText.textWrappingMode=TextWrappingModes.NoWrap; nameText.ForceMeshUpdate();
                if (i==0 && !room.Find("ControlRemoto"))
                {
                    var remote = new GameObject("ControlRemoto"); remote.transform.SetParent(room,false);
                    remote.transform.position = new Vector3(commode.center.x,commode.max.y+.022f,commode.center.z);
                    Part("Carcasa",remote.transform,Vector3.zero,new Vector3(.065f,.032f,.16f),dark);
                    var button = Part("BotonCentral",remote.transform,new Vector3(0,.021f,0),new Vector3(.028f,.01f,.028f),teal,PrimitiveType.Cylinder);
                    var front = Part("EmisorFrontal",remote.transform,new Vector3(0,0,.081f),new Vector3(.022f,.012f,.004f),off);
                    var origin = new GameObject("OrigenRayo").transform; origin.SetParent(remote.transform,false); origin.localPosition = new Vector3(0,0,.087f);
                    Grab(remote,new Vector3(.065f,.042f,.166f),.15f,manager);
                    var behaviour = remote.AddComponent<LightRemote>(); behaviour.rayOrigin=origin; behaviour.buttonVisual=button.transform;
                }
                if (i>0 && !room.Find("PortapapelesClinico"))
                {
                    var clipboard = new GameObject("PortapapelesClinico"); clipboard.transform.SetParent(room,false);
                    clipboard.transform.position = new Vector3(commode.center.x,commode.max.y+.015f,commode.center.z);
                    Part("Tabla",clipboard.transform,Vector3.zero,new Vector3(.19f,.018f,.27f),dark);
                    Part("Hoja",clipboard.transform,new Vector3(0,.010f,-.01f),new Vector3(.165f,.002f,.23f),white);
                    Part("Pinza",clipboard.transform,new Vector3(0,.016f,.115f),new Vector3(.07f,.012f,.02f),teal);
                    Grab(clipboard,new Vector3(.19f,.028f,.27f),.22f,manager);
                }
                // Existing luminaires are the physical targets for their respective fill lights.
                var lighting = room.Find("04_Iluminacion");
                for (int j=1;j<=2;j++)
                {
                    var fixture=lighting.Find("Luminaria_"+j);
                    if (!fixture.GetComponent<RemoteLightTarget>())
                    {
                        var bounds=fixture.GetComponentInChildren<Renderer>().bounds;
                        var collider=fixture.gameObject.AddComponent<BoxCollider>();
                        collider.center=fixture.InverseTransformPoint(bounds.center);
                        var target=fixture.gameObject.AddComponent<RemoteLightTarget>();
                        target.controlledLight=lighting.Find("RellenoClinico_"+j).GetComponent<Light>();
                        target.lens=fixture.Find("Light2").GetComponent<Renderer>(); target.offMaterial=off;
                        target.lens.sharedMaterial=on;
                    }
                    // Convert the world-aligned visible bounds to the rotated, scaled prefab space.
                    var world=fixture.GetComponentInChildren<Renderer>().bounds;
                    world.Expand(new Vector3(.03f,.03f,.04f));
                    var local=new Bounds(fixture.InverseTransformPoint(world.center),Vector3.zero);
                    for(int x=-1;x<=1;x+=2) for(int y=-1;y<=1;y+=2) for(int z=-1;z<=1;z+=2)
                        local.Encapsulate(fixture.InverseTransformPoint(world.center+Vector3.Scale(world.extents,new Vector3(x,y,z))));
                    var lampCollider=fixture.GetComponent<BoxCollider>(); lampCollider.center=local.center; lampCollider.size=local.size;
                }
                // The directional light is above the ceiling: give it an explicit visible fixture below it.
                if (!lighting.Find("LuminariaPrincipalRemota"))
                {
                    var fixture=Part("LuminariaPrincipalRemota",lighting,new Vector3(0,2.74f,0),new Vector3(.46f,.06f,.46f),on);
                    var target=fixture.AddComponent<RemoteLightTarget>();
                    fixture.AddComponent<BoxCollider>();
                    target.controlledLight=lighting.Find("LuzClinicaPrincipal").GetComponent<Light>();
                    target.lens=fixture.GetComponent<Renderer>(); target.offMaterial=off;
                }
            }
            var corridor=GameObject.Find("Pasillo_Hospitalizacion").transform;
            for(int i=0;i<4;i++)
            {
                if(corridor.Find("LamparaRemota_"+i)) continue;
                var light=corridor.Find("LuzPasillo_"+i).GetComponent<Light>();
                var fixture=Part("LamparaRemota_"+i,corridor,new Vector3(i*6.5f,2.72f,-3.85f),new Vector3(.45f,.07f,.3f),on);
                fixture.AddComponent<BoxCollider>();
                var target=fixture.AddComponent<RemoteLightTarget>(); target.controlledLight=light;
                target.lens=fixture.GetComponent<Renderer>(); target.offMaterial=off;
            }
            foreach(var target in UnityEngine.Object.FindObjectsByType<RemoteLightTarget>())
                if(target.lens) target.lens.sharedMaterial=target.controlledLight.isActiveAndEnabled?on:off;
            HospitalPresentationSetup.Configure();
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets();
            return "Saved: 2 direct interactors, remote, 3 clipboards, 4 names, 16 lamp targets.";
        }

        static XRInputButtonReader Reader(string side,string button,string value)
        {
            string device="<XRController>{"+side+"Hand}/";
            return new XRInputButtonReader {inputSourceMode=XRInputButtonReader.InputSourceMode.InputAction,
                inputActionPerformed=new InputAction(side+button,InputActionType.Button,device+button),
                inputActionValue=new InputAction(side+value,InputActionType.Value,device+value)};
        }
        static void Grab(GameObject go,Vector3 size,float mass,XRInteractionManager manager)
        {
            go.AddComponent<BoxCollider>().size=size;
            var rb=go.AddComponent<Rigidbody>(); rb.mass=mass; rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation=RigidbodyInterpolation.Interpolate;
            var grab=go.AddComponent<XRGrabInteractable>(); grab.interactionManager=manager;
            grab.useDynamicAttach=true; grab.movementType=XRBaseInteractable.MovementType.VelocityTracking;
            grab.throwOnDetach=true;
        }
        static Material Material(string name,Color color)
        {
            var path=Materials+"/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!mat) {mat=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat,path);}
            mat.color=color; return mat;
        }
        static GameObject Part(string name,Transform parent,Vector3 position,Vector3 scale,Material mat,PrimitiveType type=PrimitiveType.Cube)
        {
            var go=GameObject.CreatePrimitive(type); go.name=name;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent,false); go.transform.localPosition=position; go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=mat; return go;
        }
        static void Text(string name,Transform parent,Vector3 position,string value,float size,Vector2 bounds)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false); go.transform.localPosition=position;
            var text=go.AddComponent<TextMeshPro>(); text.text=value; text.fontSize=size;
            text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            text.alignment=TextAlignmentOptions.Center; text.color=Color.white;
            text.rectTransform.sizeDelta=bounds; text.ForceMeshUpdate();
        }
    }
}
