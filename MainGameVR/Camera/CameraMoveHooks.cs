using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using IllusionUtility.SetUtility;
using KKS_VR.Caress;
using KKS_VR.Features;
using UnityEngine;
using VRGIN.Core;

// This file is a collection of hooks to move the VR camera at appropriate
// points of the game.

namespace KKS_VR.Camera
{
    [HarmonyPatch(typeof(ADV.TextScenario))]
    internal class TextScenarioPatches1
    {
        [HarmonyPatch("ADVCameraSetting")]
        [HarmonyPostfix]
        private static void PostADVCameraSetting(ADV.TextScenario __instance)
        {
            if (Manager.Scene.initialized && Manager.Scene.NowSceneNames[0] == "Talk")
                // Talk scenes are handled separately.
                return;

            VRLog.Debug("PostADVCameraSetting");
            var backTrans = __instance.BackCamera?.transform;
            if (backTrans == null)
                // backTrans can be null in Roaming. We don't want to move the
                // camera anyway in that case.
                return;
            VRCameraMover.Instance.MaybeMoveADV(__instance, backTrans.position, backTrans.rotation, false);
        }
    }

    [HarmonyPatch]
    internal class RequestNextLinePatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            // In some versions of the base game, MainScenario._RequestNextLine
            // duplicates the logic found in TextScenario._RequestNextLine.
            // In other versions, the former simply calls the latter.
            // We want to patch both methods or the latter alone depending on
            // the version.
            yield return AccessTools.Method(typeof(ADV.TextScenario), "_RequestNextLine");
            if (AccessTools.Field(typeof(ADV.MainScenario), "textHash") == null) yield return AccessTools.Method(typeof(ADV.MainScenario), "_RequestNextLine");
        }

        private static void Postfix(ADV.TextScenario __instance, ref IEnumerator __result)
        {
            if (Manager.Scene.initialized && Manager.Scene.NowSceneNames[0] == "Talk")
                // Talk scenes are handled separately.
                return;

            if (__instance.advScene == null)
                // Outside ADV scene (probably roaming), ignore.
                return;

            __result = new[] { __result, Postfix() }.GetEnumerator();

            IEnumerator Postfix()
            {
                VRCameraMover.Instance.HandleTextScenarioProgress(__instance);
                yield break;
            }
        }
    }

    [HarmonyPatch(typeof(ADV.Program))]
    internal class ProgramPatches1
    {
        [HarmonyPatch(nameof(ADV.Program.SetNull))]
        [HarmonyPostfix]
        private static void PostSetNull(Transform transform)
        {
            VRLog.Debug("PostSetNull");
            VRCameraMover.Instance.MaybeMoveTo(transform.position, transform.rotation, false);
        }
    }

    [HarmonyPatch(typeof(HSceneProc))]
    internal class HSceneProcPatches
    {

        [HarmonyPatch("ChangeAnimator")]
        [HarmonyPrefix]
        public static void ChangeAnimatorPrefix(HSceneProc __instance)
        {
            __instance.ctrlObi.solver.gameObject.SetActive(true);
        }

        [HarmonyPatch("ChangeAnimator")]
        [HarmonyPostfix]
        public static void PostChangeAnimator(HSceneProc.AnimationListInfo _nextAinmInfo, bool _isForceCameraReset, HSceneProc __instance, List<ChaControl> ___lstFemale)
        {
            if (VRMouth.Instance != null)
            {
                VRMouth.Instance.OnPositionChange(_nextAinmInfo);
            }
            //else if (VRMoverH.Instance != null && VRMoverH.Instance._settings.FlyInH)
            //{
            //if (_isForceCameraReset)
            {
                UpdateVRCamera(__instance, ___lstFemale, null);
            }
            Fixes.ObiCtrlFix.SetFluidsState(false);
        }

        //[HarmonyPatch("ChangeCategory")]
        //[HarmonyPrefix]
        //public static void PreChangeCategory(List<ChaControl> ___lstFemale, out float __state)
        //{
        //    __state = ___lstFemale[0].objTop.transform.position.y;
        //}

        [HarmonyPatch("ChangeCategory")]
        [HarmonyPostfix]
        public static void PostChangeCategory(HSceneProc __instance, List<ChaControl> ___lstFemale)//, float __state)
        {
            if (PoV.Instance != null)
            {
                PoV.Instance.OnSpotChange();
            }

            UpdateVRCamera(__instance, ___lstFemale, null);// __state);
            
            Fixes.ObiCtrlFix.SetFluidsState(false);
        }
        [HarmonyPatch(nameof(HSceneProc.GotoPointMoveScene))]
        [HarmonyPostfix]
        public static void GotoPointMoveScenePostfix()
        {
            if (VRMoverH.Instance != null)
            {
                VRMoverH.Instance.MakeUpright();
            }
        }

        /// <summary>
        /// Update the transform of the VR camera.
        /// </summary>
        private static void UpdateVRCamera(HSceneProc instance, List<ChaControl> lstFemale, float? previousFemaleY)
        {
            var baseTransform = lstFemale[0].objTop.transform;
            var camDat = instance.flags.ctrlCamera.CamDat;// new Traverse(instance.flags.ctrlCamera).Field<BaseCameraControl_Ver2.CameraData>("CamDat").Value;
            var cameraRotation = baseTransform.rotation * Quaternion.Euler(camDat.Rot);
            Vector3 dir;
            switch (instance.flags.mode)
            {
                case HFlag.EMode.masturbation:
                case HFlag.EMode.peeping:
                case HFlag.EMode.lesbian:
                    // Use the default distance for 3rd-person scenes.
                    dir = camDat.Dir;
                    break;
                default:
                    // Start closer otherwise.
                    dir = Vector3.back * 0.8f;
                    break;
            }

            var cameraPosition = cameraRotation * dir + baseTransform.TransformPoint(camDat.Pos);
            //if (previousFemaleY is float prevY)
            //{
            //    // Keep the relative Y coordinate from the female.
            //    var cameraHeight = VR.Camera.transform.position.y + baseTransform.position.y - prevY;
            //    var destination = new Vector3(cameraPosition.x, cameraHeight, cameraPosition.z);

            //    if (VRMoverH.Instance != null && VRMoverH.Instance._settings.FlyInH)
            //    {
            //        VRMoverH.Instance.MoveToInH(position: destination);
            //    }
            //    else
            //    {
            //        VRCameraMover.Instance.MaybeMoveTo(destination, cameraRotation, false);
            //    }
            //}
            //else
            {
                // We are starting from scratch.
                // TODO: the height calculation below assumes standing mode.

                if (VRMoverH.Instance != null && VRMoverH.Instance._settings.FlyInH)
                {
                    VRMoverH.Instance.MoveToInH(cameraPosition, cameraRotation, previousFemaleY == null, instance.flags.mode);
                }
                else
                {
                    var cameraHeight = lstFemale[0].transform.position.y + VR.Camera.transform.localPosition.y;
                    var destination = new Vector3(cameraPosition.x, cameraHeight, cameraPosition.z);
                    VRCameraMover.Instance.MoveTo(destination, cameraRotation, false);
                }

                
            }
        }
    }
}
