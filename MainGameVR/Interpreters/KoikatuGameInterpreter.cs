using System;
using System.Collections;
using Funly.SkyStudio;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKS_VR.Camera;
using KKS_VR.Features;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRGIN.Core;

namespace KKS_VR.Interpreters
{
    internal class KoikatuInterpreter : GameInterpreter
    {
        public enum SceneType
        {
            OtherScene,
            ActionScene,
            TalkScene,
            HScene,
            NightMenuScene,
            CustomScene
        }

        public SceneType CurrentScene { get; private set; }
        public SceneInterpreter SceneInterpreter;

        private Fixes.Mirror.Manager _mirrorManager;
        private int _kkapiCanvasHackWait;
        private Canvas _kkSubtitlesCaption;
        private GameObject _sceneObjCache;

        protected override void OnAwake()
        {
            base.OnAwake();

            CurrentScene = SceneType.OtherScene;
            SceneInterpreter = new OtherSceneInterpreter();
            SceneManager.sceneLoaded += OnSceneLoaded;
            _mirrorManager = new Fixes.Mirror.Manager();
            VR.Camera.gameObject.AddComponent<VREffector>();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            UpdateScene();
            SceneInterpreter.OnUpdate();
        }

        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
            if (_kkSubtitlesCaption != null) FixupKkSubtitles();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var reflection in FindObjectsOfType<MirrorReflection>()) _mirrorManager.Fix(reflection);

            if (scene.name == "Title" || scene.name == "FreeH" || scene.name == "Uploader" || scene.name == "Downloader")
                LoadTitleSkybox();
        }

        private static void LoadTitleSkybox()
        {
            if (GameObject.FindObjectOfType<TimeOfDayController>()) return;

            try
            {
                var stockSkyProfiles = new string[]
                {
                    "_morning_stu",
                    "_daytime_stu",
                    "_evening_stu",
                    "_night_stu",
                };
                // Use either day or night skybox depending on real world time to reduce eye abuse at night
                // morning and evening skyboxes are not worth using for title screen
                var timeNow = DateTime.Now;
                var isNight = timeNow.Hour < 6 || timeNow.Hour > 20;
                var skyType = isNight ? stockSkyProfiles[3] : stockSkyProfiles[1];
                // var skyType = stockSkyProfiles[UnityEngine.Random.RandomRangeInt(0, stockSkyProfiles.Length)];

                VRLog.Info($"Loading skybox {skyType}...");

                var skyProfile = CommonLib.LoadAsset<SkyProfile>(@"studio\sky\01.unity3d", "SkyProfile" + skyType, true, null, true);
                var skyMaterial = CommonLib.LoadAsset<Material>(@"studio\sky\01.unity3d", "SkyboxMaterial" + skyType, true, null, true);
                if (skyProfile != null && skyMaterial != null)
                {
                    VRLog.Info($"SkyProfile: {skyProfile}   SkyboxMaterial: {skyMaterial}");

                    var instanceGameObject = new GameObject("KKSVR_Skybox");
                    var skyController = instanceGameObject.AddComponent<TimeOfDayController>();

                    // Need to add dummy sun and mun objects or the controller will refuse to work
                    var sun = new GameObject("Sun");
                    sun.transform.parent = instanceGameObject.transform;
                    new GameObject("Position", typeof(RotateBody)).transform.parent = sun.transform;
                    skyController.sunOrbit = sun.AddComponent<OrbitingBody>();

                    var mun = new GameObject("Moon");
                    mun.transform.parent = instanceGameObject.transform;
                    new GameObject("Position", typeof(RotateBody)).transform.parent = mun.transform;
                    skyController.moonOrbit = mun.AddComponent<OrbitingBody>();

                    // For some reason the profile doesn't come with the material, which is required
                    skyProfile.skyboxMaterial = skyMaterial;
                    // This applies the skybox right away
                    skyController.skyProfile = skyProfile;
                }
                else
                {
                    VRLog.Warn("Skybox not found! Missing CharaStudio assets?");
                }
            }
            catch (Exception e)
            {
                VRLog.Error("Failed to load Skybox! Error: " + e);
            }
        }

        /// <summary>
        /// Fix up scaling of subtitles added by KK_Subtitles. See
        /// https://github.com/IllusionMods/KK_Plugins/pull/91 for details.
        /// </summary>
        private void FixupKkSubtitles()
        {
            foreach (Transform child in _kkSubtitlesCaption.transform)
                if (child.localScale != Vector3.one)
                {
                    VRLog.Info($"Fixing up scale for {child}");
                    child.localScale = Vector3.one;
                }
        }

        public override bool IsIgnoredCanvas(Canvas canvas)
        {
            if (PrivacyScreen.IsOwnedCanvas(canvas))
            {
                return true;
            }
            else if (canvas.name == "Canvas_BackGround")
            {
                BackgroundDisplayer.Instance.TakeCanvas(canvas);
                return true;
            }
            else if (canvas.name == "CvsMenuTree")
            {
                // Here, we attempt to avoid some unfortunate conflict with
                // KKAPI.
                //
                // In order to support plugin-defined subcategories in Maker,
                // KKAPI clones some UI elements out of CvsMenuTree when the
                // canvas is created, then uses them as templates for custom
                // UI items.
                //
                // At the same time, VRGIN attempts to claim the canvas by
                // setting its mode to ScreenSpaceCamera, which changes
                // localScale of the canvas by a factor of 100 or so. If this
                // happens between KKAPI's cloning out and cloning in, the
                // resulting UI items will have the wrong scale, 72x the correct
                // size to be precise.
                //
                // So our solution here is to hide the canvas from VRGIN for a
                // couple of frames. Crude but works.

                if (_kkapiCanvasHackWait == 0)
                {
                    _kkapiCanvasHackWait = 3;
                    return true;
                }
                else
                {
                    _kkapiCanvasHackWait -= 1;
                    return 0 < _kkapiCanvasHackWait;
                }
            }
            else if (canvas.name == "KK_Subtitles_Caption")
            {
                _kkSubtitlesCaption = canvas;
            }

            return false;
        }

        // 前回とSceneが変わっていれば切り替え処理をする
        private void UpdateScene()
        {
            var nextSceneType = DetectScene();

            if (nextSceneType != CurrentScene)
            {
                VRLog.Info($"Load interpreter for new scene type: {nextSceneType}");
                SceneInterpreter.OnDisable();

                CurrentScene = nextSceneType;
                SceneInterpreter = CreateSceneInterpreter(nextSceneType);
                SceneInterpreter.OnStart();
            }
        }

        private SceneType DetectScene()
        {
            if (GameAPI.InsideHScene) return SceneType.HScene;
            if (MakerAPI.InsideMaker) return SceneType.CustomScene;
            if (TalkScene.isPaly) return SceneType.TalkScene;

            var stack = Manager.Scene.NowSceneNames;
            foreach (var name in stack)
            {
                //if (name == "H" && SceneObjPresent("HScene"))
                //    return SceneType.HScene;
                if (ActionScene.initialized && name == "Action")
                    return SceneType.ActionScene;
                //if (name == "Talk" && SceneObjPresent("TalkScene"))
                //    return SceneType.TalkScene;
                if (name == "NightMenu" && SceneObjPresent("NightMenuScene"))
                    return SceneType.NightMenuScene;
                //if (name == "CustomScene" && SceneObjPresent("CustomScene"))
                //    return SceneType.CustomScene;
            }

            return SceneType.OtherScene;
        }

        private bool SceneObjPresent(string name)
        {
            if (_sceneObjCache != null && _sceneObjCache.name == name) return true;
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                _sceneObjCache = obj;
                return true;
            }

            return false;
        }

        private static SceneInterpreter CreateSceneInterpreter(SceneType ty)
        {
            switch (ty)
            {
                case SceneType.OtherScene:
                    return new OtherSceneInterpreter();
                case SceneType.ActionScene:
                    return new ActionSceneInterpreter();
                case SceneType.CustomScene:
                    return new CustomSceneInterpreter();
                case SceneType.NightMenuScene:
                    return new NightMenuSceneInterpreter();
                case SceneType.HScene:
                    return new HSceneInterpreter();
                case SceneType.TalkScene:
                    return new TalkSceneInterpreter();
                default:
                    VRLog.Warn($"Unknown scene type: {ty}");
                    return new OtherSceneInterpreter();
            }
        }

        protected override CameraJudgement JudgeCameraInternal(UnityEngine.Camera camera)
        {
            if (camera.CompareTag("MainCamera")) StartCoroutine(HandleMainCameraCo(camera));
            return base.JudgeCameraInternal(camera);
        }

        /// <summary>
        /// A coroutine to be called when a new main camera is detected.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        private IEnumerator HandleMainCameraCo(UnityEngine.Camera camera)
        {
            // Unity might have messed with the camera transform for this frame,
            // so we wait for the next frame to get clean data.
            yield return null;

            if (camera.name == "ActionCamera" || camera.name == "FrontCamera")
            {
                VRLog.Info("Adding ActionCameraControl");
                camera.gameObject.AddComponent<ActionCameraControl>();
            }
            else if (camera.GetComponent<CameraControl_Ver2>() != null)
            {
                VRLog.Info("New main camera detected: moving to {0} {1}", camera.transform.position, camera.transform.eulerAngles);
                VRCameraMover.Instance.MoveTo(camera.transform.position, camera.transform.rotation, false);
                VRLog.Info("moved to {0} {1}", VR.Camera.Head.position, VR.Camera.Head.eulerAngles);
                VRLog.Info("Adding CameraControlControl");
                camera.gameObject.AddComponent<CameraControlControl>();
            }
            else
            {
                VRLog.Warn($"Unknown kind of main camera was added: {camera.name}");
            }
        }

        //public override bool ApplicationIsQuitting => Manager.Scene.isGameEnd;
    }
}
