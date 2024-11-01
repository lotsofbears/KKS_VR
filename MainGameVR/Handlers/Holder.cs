﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static HandCtrl;
using static KKS_VR.Handlers.HandHolder;
using VRGIN.Core;
using IllusionUtility.GetUtility;
using KKS_VR.Handlers;
using Illusion.Extensions;

namespace KKS_VR.Handlers
{
    internal class Holder : MonoBehaviour
    {
        protected private Rigidbody _rigidBody;
        //protected private AudioSource _audioSource;
        protected private ItemType _activeItem;
        internal Transform Anchor => _anchor;
        protected private Transform _anchor;
        protected private static readonly Dictionary<int, AibuItem> _loadedAssetsList = [];
        internal static Material Material { get; private set; }
        internal class AnimParam
        {
            internal int[] availableLayers;
            internal int startLayer;
            internal string movingPartName;
            internal string handlerParentName;
            internal Vector3 positionOffset;
            internal Quaternion rotationOffset;
        }
        protected private static readonly List<AnimParam> _defaultAnimParamList =
            [
            new AnimParam
            {
                // Hand
                availableLayers = [4, 7, 10],
                startLayer = 10,
                movingPartName = "cf_j_handroot_",
                handlerParentName = "cf_j_handroot_",
                positionOffset = new Vector3(0f, -0.02f, -0.07f),
                rotationOffset = Quaternion.identity
            },
            new AnimParam
            {
                // Finger
                availableLayers = [1, 3, 9, 4, 6],
                startLayer = 9,
                movingPartName = "cf_j_handroot_",
                handlerParentName = "cf_j_handroot_",
                positionOffset = new Vector3(0f, -0.02f, -0.07f),
                rotationOffset = Quaternion.identity
            },
            new AnimParam
            {
                // Massager
                availableLayers = [0, 1],
                startLayer = 0,
                movingPartName = "N_massajiki_",
                handlerParentName = "_head_00",
                positionOffset = new Vector3(0f, 0f, -0.05f),
                rotationOffset = Quaternion.Euler(-90f, 180f, 0f)
            },
            new AnimParam
            {
                // Vibrator
                availableLayers = [0, 1],
                startLayer = 0,
                movingPartName = "N_vibe_Angle",
                handlerParentName = "J_vibe_03",
                positionOffset = new Vector3(0f, 0f, -0.1f),
                rotationOffset = Quaternion.Euler(-90f, 180f, 0f)
            },
            new AnimParam
            {
                // Tongue
                /*
                 *  21, 16, 18, 19   - licking haphazardly
                 *      7 - at lower angle 
                 *      9 - at lower angle very slow
                 *      10 - at higher angle
                 *      12 - at higher angle very slow 
                 *      
                 *  13 - very high angle, flopping
                 *  
                 *  15 - very high angle, back forth
                 *  1, 3, 4, 6,   - licking 
                 *  
                 *  1 - Lick
                 *  2 - Lick stop.
                 *  3 - Lick 
                 *  4 - Lick
                 *  5 - Lick stop.
                 *  6 - Lick
                 *  7 - Lick no pos move, angular same.
                 *  8 - Lick no pos move, angular same stop.
                 *  9 - Lick no pos move, slow angular
                 *  10 - Lick no pos move, fast angular
                 *  11 - Lick no pos move, fast(slow) angular, stop
                 *  12 - Lick no pos move, slow angular
                 *  13 - Doing "fish our of sea" movements
                 *  14 - Doing "fish our of sea" movements stop
                 *  15 - Tube-like back and forth
                 *  16 - Intense lick
                 *  17 - Intense lick stop
                 *  18 - Intense lick
                 *  19 - Extra intense lick
                 *  20 - Extra intense lick stop
                 *  21 - Lick
                 */
                availableLayers = [1, 7, 9, 10, 12, 13, 15, 16],
                movingPartName = "cf_j_tang_01", // cf_j_tang_01 / cf_j_tangangle
                handlerParentName = "cf_j_tang_03",
                positionOffset = new Vector3(0f, -0.04f, 0.05f),
                rotationOffset = Quaternion.identity, // Quaternion.Euler(-90f, 0f, 0f)
            }
            ];
        internal class ItemType
        {
            internal AibuItem aibuItem;
            internal ItemHandler handler;
            internal GameObject handlerParent;
            internal Transform rootPoint;
            internal Transform movingPoint;
            internal Quaternion rotationOffset;
            internal Vector3 positionOffset;
            internal int layer;
            internal int[] availableLayers;
            internal int startLayer;

            internal ItemType(AibuItem asset, AnimParam animParam)
            {
                aibuItem = asset;
                rootPoint = asset.obj.transform;
                rootPoint.transform.SetParent(VR.Manager.transform, false);
                positionOffset = animParam.positionOffset;
                rotationOffset = animParam.rotationOffset;
                availableLayers = animParam.availableLayers;
                startLayer = animParam.startLayer;
                movingPoint = rootPoint.GetComponentsInChildren<Transform>()
                    .Where(t => t.name.StartsWith(animParam.movingPartName, StringComparison.Ordinal))
                    .FirstOrDefault();
                handlerParent = rootPoint.GetComponentsInChildren<Transform>()
                    .Where(t => t.name.StartsWith(animParam.handlerParentName, StringComparison.Ordinal)
                    || t.name.EndsWith(animParam.handlerParentName, StringComparison.Ordinal))
                    .FirstOrDefault().gameObject;

            }
            internal ItemType()
            {
                rotationOffset = Quaternion.identity;
                positionOffset = Vector3.zero;
                //positionOffset = new Vector3(0f, -0.02f, -0.07f);
                handlerParent = new GameObject("EmptyItem");
                handlerParent.transform.SetParent(VR.Manager.transform, worldPositionStays: false);
                rootPoint = handlerParent.transform;
                handlerParent.SetActive(false);
            }


        }


        // CopyPaste from game.
        internal static readonly List<string> loadLists =
            [
            "h/list/00.unity3d",
            "h/list/01.unity3d",
            "h/list/30.unity3d",
            "h/list/31.unity3d",
            "h/list/60.unity3d",
            "h/list/70.unity3d",
            "h/list/71.unity3d",
            "h/list/81.unity3d",
            "h/list/91.unity3d",
            "h/list/bfwapartment1.unity3d",
            "h/list/hs2suite.unity3d",
            "h/list/mmbath.unity3d",
            "h/list/mmhouse01.unity3d",
            "h/list/poolmap.unity3d",
            "h/list/sakuraroom.unity3d",
            ];

        internal static bool BundleCheck(string path, string targetName)
        {
            bool result = false;
            string[] allAssetName = AssetBundleCheck.GetAllAssetName(path, false, "", false);
            for (int i = 0; i < allAssetName.Length; i++)
            {
                if (allAssetName[i].Compare(targetName, true))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        protected private void LoadAssets()
        {
            // KKS HandCtrl.

            string text = "AibuItemObject";
            foreach (string text2 in loadLists)
            {
                if (AssetBundleCheck.IsFile(text2, text) && (AssetBundleCheck.IsSimulation || BundleCheck(text2, text)))
                {
                    AibuItemObjectData aibuItemObjectData = CommonLib.LoadAsset<AibuItemObjectData>(text2, text, false, "", false);
                    AssetBundleManager.UnloadAssetBundle(text2, true, null, false);
                    if (!(aibuItemObjectData == null))
                    {
                        foreach (AibuItemObjectData.Param param in aibuItemObjectData.param)
                        {
                            if (!_loadedAssetsList.TryGetValue(param.id, out var aibuItem))
                            {
                                _loadedAssetsList.Add(param.id, new AibuItem());
                                aibuItem = _loadedAssetsList[param.id];
                            }
                            aibuItem.SetID(param.id);
                            aibuItem.SetObj(CommonLib.LoadAsset<GameObject>(param.bundle, param.asset, true, param.manifest, true));
                            //this.flags.hashAssetBundle.Add(param.bundle);
                            if (!param.nomalObj.IsNullOrEmpty())
                            {
                                aibuItem.objBody = aibuItem.obj.transform.FindLoop(param.nomalObj);
                                if (aibuItem.objBody)
                                {
                                    aibuItem.renderBody = aibuItem.objBody.GetComponent<SkinnedMeshRenderer>();
                                    //aibuItem.meshRenderBody = aibuItem.objBody.GetComponent<MeshRenderer>();
                                    if (param.isMaterialGet)
                                    {
                                        if (aibuItem.renderBody != null)
                                        {
                                            aibuItem.mHand = aibuItem.renderBody.material;
                                        }
                                        //else if (aibuItem.meshRenderBody != null)
                                        //{
                                        //    aibuItem.mHand = aibuItem.meshRenderBody.material;
                                        //}
                                    }
                                }
                            }
                            aibuItem.isSilhouetteChange = param.isSilhouetteChange;
                            if (!param.objSilhouette.IsNullOrEmpty())
                            {
                                aibuItem.objSilhouette = aibuItem.obj.transform.FindLoop(param.objSilhouette);
                                if (aibuItem.objSilhouette)
                                {
                                    aibuItem.renderSilhouette = aibuItem.objSilhouette.GetComponent<SkinnedMeshRenderer>();
                                    //aibuItem.meshRenderSilhouette = aibuItem.objSilhouette.GetComponent<MeshRenderer>();
                                    if (aibuItem.renderSilhouette != null)
                                    {
                                        aibuItem.mSilhouette = aibuItem.renderSilhouette.material;
                                    }
                                    //else if (aibuItem.meshRenderSilhouette != null)
                                    //{
                                    //    aibuItem.mSilhouette = aibuItem.meshRenderSilhouette.material;
                                    //}
                                    aibuItem.renderSilhouette.enabled = false;
                                    aibuItem.SetHandColor(new Color(0.960f, 0.887f, 0.864f, 1.000f));
                                    if (!Material)
                                        Material = aibuItem.renderSilhouette.material;
                                }
                            }
                            aibuItem.SetIdObj(param.objKind);
                            aibuItem.SetIdUse(param.setKind);
                            if (aibuItem.obj != null)
                            {
                                //EliminateScale[] componentsInChildren = aibuItem.obj.GetComponentsInChildren<EliminateScale>(true);
                                //if (componentsInChildren != null && componentsInChildren.Length != 0)
                                //{
                                //    componentsInChildren[componentsInChildren.Length - 1].LoadList(aibuItem.id);
                                //}
                                foreach (var component in aibuItem.obj.transform.GetComponentsInChildren<EliminateScale>(true))
                                {
                                    UnityEngine.Component.Destroy(component);
                                }
                                aibuItem.SetAnm(aibuItem.obj.GetComponent<Animator>());
                                aibuItem.obj.SetActive(false);
                            }
                            aibuItem.pathSEAsset = param.bundleSE;
                            aibuItem.nameSEFile = param.nameSE;
                            aibuItem.saveID = param.saveID;
                            aibuItem.isVirgin = param.isVirgin;
                        }
                    }
                }
            }

        }
        //protected private void LoadAssets()
        //{
        //    // Straight from HandCtrl.
        //    var textAsset = GlobalMethod.LoadAllListText("h/list/", "AibuItemObject", null);
        //    GlobalMethod.GetListString(textAsset, out var array);
        //    for (int i = 0; i < array.GetLength(0); i++)
        //    {
        //        int num = 0;
        //        int num2 = 0;

        //        int.TryParse(array[i, num++], out num2);

        //        if (!_loadedAssetsList.TryGetValue(num2, out var aibuItem))
        //        {
        //            _loadedAssetsList.Add(num2, new AibuItem());
        //            aibuItem = _loadedAssetsList[num2];
        //        }
        //        aibuItem.SetID(num2);


        //        var manifestName = array[i, num++];
        //        var text2 = array[i, num++];
        //        var assetName = array[i, num++];
        //        aibuItem.SetObj(CommonLib.LoadAsset<GameObject>(text2, assetName, true, manifestName));
        //        //this.flags.hashAssetBundle.Add(text2);
        //        var text3 = array[i, num++];
        //        var isSilhouetteChange = array[i, num++] == "1";
        //        var flag = array[i, num++] == "1";
        //        if (!text3.IsNullOrEmpty())
        //        {
        //            aibuItem.objBody = aibuItem.obj.transform.FindLoop(text3);
        //            if (aibuItem.objBody)
        //            {
        //                aibuItem.renderBody = aibuItem.objBody.GetComponent<SkinnedMeshRenderer>();
        //                if (flag)
        //                {
        //                    aibuItem.mHand = aibuItem.renderBody.material;

        //                }
        //            }
        //        }
        //        aibuItem.isSilhouetteChange = isSilhouetteChange;
        //        text3 = array[i, num++];
        //        if (!text3.IsNullOrEmpty())
        //        {
        //            aibuItem.objSilhouette = aibuItem.obj.transform.FindLoop(text3);
        //            if (aibuItem.objSilhouette)
        //            {
        //                aibuItem.renderSilhouette = aibuItem.objSilhouette.GetComponent<SkinnedMeshRenderer>();
        //                aibuItem.mSilhouette = aibuItem.renderSilhouette.material;
        //                aibuItem.renderSilhouette.enabled = false;
        //                aibuItem.SetHandColor(new Color(0.960f, 0.887f, 0.864f, 1.000f));
        //                if (!Material)
        //                    Material = aibuItem.renderSilhouette.material;
        //            }
        //        }
        //        int.TryParse(array[i, num++], out num2);
        //        aibuItem.SetIdObj(num2);
        //        int.TryParse(array[i, num++], out num2);
        //        aibuItem.SetIdUse(num2);
        //        if (aibuItem.obj)
        //        {
        //            //EliminateScale[] componentsInChildren = aibuItem.obj.GetComponentsInChildren<EliminateScale>(true);
        //            //if (componentsInChildren != null && componentsInChildren.Length != 0)
        //            //{
        //            //    componentsInChildren[componentsInChildren.Length - 1].LoadList(aibuItem.id);
        //            //}
        //            var components = aibuItem.obj.transform.GetComponentsInChildren<EliminateScale>(true);
        //            foreach (var component in components)
        //            {
        //                //component.enabled = false;
        //                UnityEngine.Component.Destroy(component);
        //            }
        //            aibuItem.SetAnm(aibuItem.obj.GetComponent<Animator>());
        //            //aibuItem.obj.SetActive(false);
        //            //aibuItem.obj.transform.SetParent(VR.Manager.transform, false);
        //        }
        //        aibuItem.pathSEAsset = array[i, num++];
        //        aibuItem.nameSEFile = array[i, num++];
        //        aibuItem.saveID = int.Parse(array[i, num++]);
        //        aibuItem.isVirgin = (array[i, num++] == "1");
        //        aibuItem.obj.SetActive(false);
        //    }
        //}
        internal void SetItemRenderer(bool show)
        {
            if (_activeItem.aibuItem == null) return;
            _activeItem.aibuItem.objBody.GetComponent<Renderer>().enabled = show;
        }
    }
}
