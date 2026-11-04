using HospitalVR.RCP;
using HospitalVR.Learning;
using UnityEditor;
using UnityEngine;
namespace HospitalVR.Editor
{
    public static class FinalDeliverySetup
    {
        public static string SavePrefabs()
        {
            RcpFinalSetup.Guard();
            var d=UnityEngine.Object.FindAnyObjectByType<RcpCompressionDetector>();
            var root=d.transform.parent.gameObject;
            var chest=d.chest; var left=d.leftHand; var right=d.rightHand; var rig=d.rigRoot;
            var lp=d.leftPose; var rp=d.rightPose;
            var m=UnityEngine.Object.FindAnyObjectByType<RcpMetronome>();
            var lh=m.leftHaptics; var rh=m.rightHaptics;
            var marker=d.GetComponent<RcpPanelView>().chestMarker;
            // Aplicar los controles y referencias internas a la estación reutilizable.
            PrefabUtility.RecordPrefabInstancePropertyModifications(d);
            PrefabUtility.RecordPrefabInstancePropertyModifications(d.GetComponent<RcpSession>());
            PrefabUtility.RecordPrefabInstancePropertyModifications(d.GetComponent<RcpPanelView>());
            PrefabUtility.RecordPrefabInstancePropertyModifications(m);
            PrefabUtility.ApplyPrefabInstance(root,InteractionMode.AutomatedAction);
            // Las referencias externas pertenecen a esta escena, nunca al asset del prefab.
            d.chest=chest; d.leftHand=left; d.rightHand=right; d.rigRoot=rig; d.leftPose=lp; d.rightPose=rp;
            m.leftHaptics=lh; m.rightHaptics=rh;
            d.GetComponent<RcpPanelView>().chestMarker=marker;
            foreach(var button in root.GetComponentsInChildren<RcpTouchButton>(true))
            { button.hands=new[]{left,right}; PrefabUtility.RecordPrefabInstancePropertyModifications(button); }
            var area=root.GetComponentInChildren<PracticeArea>(); area.rig=rig;
            PrefabUtility.RecordPrefabInstancePropertyModifications(area);
            PrefabUtility.RecordPrefabInstancePropertyModifications(d);
            PrefabUtility.RecordPrefabInstancePropertyModifications(d.GetComponent<RcpPanelView>());
            PrefabUtility.RecordPrefabInstancePropertyModifications(m);
            var manual=UnityEngine.Object.FindAnyObjectByType<TrainingManual>();
            PrefabUtility.SaveAsPrefabAsset(manual.gameObject,"Assets/VR/Learning/ManualIlustrado.prefab");
            RcpFinalSetup.Save();
            return "EstacionRCP.prefab y ManualIlustrado.prefab guardados; referencias de rig conservadas como overrides de escena.";
        }
    }
}
