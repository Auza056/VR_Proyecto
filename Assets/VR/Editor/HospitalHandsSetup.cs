using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace HospitalVR.Editor
{
    /// <summary>Adaptación reproducible del modelo combinado, sin modificar los recursos de PolyOne.</summary>
    public static class HospitalHandsSetup
    {
        const string Source = "Assets/PolyOne/Free VR Hands/Prefabs/Free Pack - VR Hands ( Rigged ).prefab";
        const string Output = "Assets/VR/Hands";

        [MenuItem("Hospital VR/Integrar manos en el jugador existente")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Salir de Play antes de integrar las manos.");
            var origin = GameObject.Find("XR Origin (VR)");
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Source);
            if (!origin || !source) throw new InvalidOperationException("Falta el XR Origin existente o el prefab de Free Pack – VR Hands.");
            System.IO.Directory.CreateDirectory(Output);
            AssetDatabase.Refresh();
            var material = AssetDatabase.LoadAssetAtPath<Material>(Output + "/ManosURP.mat");
            if (!material)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/PolyOne/Free VR Hands/Texture/T_VRHands_Base_color.png"));
                material.SetFloat("_Smoothness", .25f);
                AssetDatabase.CreateAsset(material, Output + "/ManosURP.mat");
            }

            foreach (var side in new[] { "Left", "Right" })
            {
                var controller = origin.transform.Find("Camera Offset/" + side + "HandController");
                if (!controller) throw new InvalidOperationException("Falta el mando " + side);
                var prefabPath = Output + "/Mano" + side + ".prefab";
                // Regenerar los prefabs propios permite reaplicar ajustes sin duplicar las instancias.
                var prefab = BuildHand(source, side, material, prefabPath);
                foreach (var childName in new[] { side + "Hand", "XR Controller " + side, side + "HandQuestVisual" })
                {
                    var child = controller.Find(childName);
                    if (child) child.gameObject.SetActive(false);
                }
                var old = controller.Find("ManoAnimada_" + side);
                if (!old)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, controller);
                    instance.name = "ManoAnimada_" + side;
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                    Undo.RegisterCreatedObjectUndo(instance, "Integrar mano " + side);
                }
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(origin.scene);
            EditorSceneManager.SaveScene(origin.scene);
        }

        static GameObject BuildHand(GameObject source, string side, Material material, string path)
        {
            var holder = new GameObject("ManoAnimada_" + side);
            try
            {
                var model = UnityEngine.Object.Instantiate(source, holder.transform);
                model.name = "ModeloPolyOne";
                foreach (var animator in model.GetComponentsInChildren<Animator>()) UnityEngine.Object.DestroyImmediate(animator);
                var bones = model.GetComponentsInChildren<Transform>().ToDictionary(t => t.name);
                var renderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
                renderer.sharedMesh = SplitMesh(renderer.sharedMesh, side);
                renderer.sharedMaterial = material;
                renderer.updateWhenOffscreen = true;
                var prefix = "J_" + side + "_Hand";
                var wrist = bones[prefix];
                var forward = (bones[prefix + "Middle4"].position - wrist.position).normalized;
                var normal = Vector3.Cross(bones[prefix + "Index1"].position - wrist.position,
                    bones[prefix + "Pinky1"].position - wrist.position).normalized;
                var dorsal = side == "Left" ? -normal : normal;
                var joints = new List<ControllerHandAnimator.FingerJoint>();
                foreach (var finger in new[] { "Thumb", "Index", "Middle", "Ring", "Pinky" })
                {
                    for (var index = 1; index <= 3; index++)
                    {
                        var bone = bones[prefix + finger + index];
                        var next = bones[prefix + finger + (index + 1)];
                        var axis = bone.InverseTransformDirection(Vector3.Cross(next.position - bone.position, -dorsal).normalized);
                        var angle = finger == "Thumb" ? new[] { 25f, 40f, 35f }[index - 1] : new[] { 55f, 75f, 60f }[index - 1];
                        joints.Add(new ControllerHandAnimator.FingerJoint
                        {
                            bone = bone, openRotation = bone.localRotation,
                            closedRotation = bone.localRotation * Quaternion.AngleAxis(angle, axis),
                            indexFinger = finger == "Index"
                        });
                    }
                }
                // Un metro de Unity conserva la longitud real de 18.4 cm entre muñeca y dedo medio.
                model.transform.localRotation = Quaternion.Inverse(Quaternion.LookRotation(forward, dorsal));
                model.transform.localPosition = new Vector3(0, -.03f, -.025f) - holder.transform.InverseTransformPoint(wrist.position);
                var adapter = holder.AddComponent<ControllerHandAnimator>();
                var grip = new InputAction("Grip" + side, InputActionType.Value, "<XRController>{" + side + "Hand}/grip", expectedControlType: "Axis");
                var trigger = new InputAction("Trigger" + side, InputActionType.Value, "<XRController>{" + side + "Hand}/trigger", expectedControlType: "Axis");
                // Serializar acciones locales sin cambiar el asset de acciones del jugador.
                var data = new AdapterData { gripAction = new InputActionProperty(grip), triggerAction = new InputActionProperty(trigger), joints = joints.ToArray() };
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data), adapter);
                return PrefabUtility.SaveAsPrefabAsset(holder, path);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Serializable]
        sealed class AdapterData
        {
            public InputActionProperty gripAction;
            public InputActionProperty triggerAction;
            public ControllerHandAnimator.FingerJoint[] joints;
        }

        static Mesh SplitMesh(Mesh source, string side)
        {
            var path = Output + "/Malla" + side + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing) return existing;
            using (var read = MeshUtility.AcquireReadOnlyMeshData(source))
            {
                var input = read[0];
                using (var vertices = new NativeArray<Vector3>(input.vertexCount, Allocator.Temp))
                using (var indices = new NativeArray<int>((int)input.GetSubMesh(0).indexCount, Allocator.Temp))
                {
                    input.GetVertices(vertices);
                    input.GetIndices(indices, 0);
                    var triangles = new List<uint>();
                    var sign = side == "Left" ? -1 : 1;
                    for (int i = 0; i < indices.Length; i += 3)
                    {
                        if (vertices[indices[i]].x * sign <= 0 || vertices[indices[i + 1]].x * sign <= 0 || vertices[indices[i + 2]].x * sign <= 0) continue;
                        triangles.Add((uint)indices[i]); triangles.Add((uint)indices[i + 1]); triangles.Add((uint)indices[i + 2]);
                    }
                    if (triangles.Count == 0) throw new InvalidOperationException("No se pudo separar la malla " + side);
                    var write = Mesh.AllocateWritableMeshData(1);
                    var output = write[0];
                    output.SetVertexBufferParams(input.vertexCount, source.GetVertexAttributes());
                    for (int stream = 0; stream < source.vertexBufferCount; stream++)
                        output.GetVertexData<byte>(stream).CopyFrom(input.GetVertexData<byte>(stream));
                    output.SetIndexBufferParams(triangles.Count, IndexFormat.UInt32);
                    var targetIndices = output.GetIndexData<uint>();
                    for (int i = 0; i < triangles.Count; i++) targetIndices[i] = triangles[i];
                    output.subMeshCount = 1;
                    output.SetSubMesh(0, new SubMeshDescriptor(0, triangles.Count));
                    var mesh = new Mesh { name = "PolyOne_" + side };
                    Mesh.ApplyAndDisposeWritableMeshData(write, mesh);
                    mesh.bindposes = source.bindposes;
                    mesh.RecalculateBounds();
                    AssetDatabase.CreateAsset(mesh, path);
                    return mesh;
                }
            }
        }
    }
}
