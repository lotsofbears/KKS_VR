using HarmonyLib;
using VRGIN.Core;

namespace KKS_VR.Features
{
    /// <summary>
    /// A component to be attached to every male character.
    /// </summary>
    internal class HideMaleHead : ProtectedBehaviour
    {
        public static bool ForceHideHead { get; set; }

        private ChaControl _control;

        protected override void OnAwake()
        {
            _control = GetComponent<ChaControl>();
        }

        protected override void OnLateUpdate()
        {
            // Hide the head if the VR camera is inside it.
            // This also essentially negates the effect of scenairo-controlled
            // head hiding, which is found in some ADV scenes.
            var head = _control.objHead?.transform;
            if (_control.objTop?.activeSelf == true && head != null)
            {
                var wasVisible = _control.fileStatus.visibleHeadAlways;
                var vrEye = VR.Camera.transform;
                var headCenter = head.TransformPoint(0, 0.12f, -0.04f);
                var sqrDistance = (vrEye.position - headCenter).sqrMagnitude;
                var visible = !ForceHideHead && 0.0361f < sqrDistance; // 19 centimeters
                _control.fileStatus.visibleHeadAlways = visible;
                if (wasVisible && !visible)
                {
                    // The VR camera may have just teleported into the head. In
                    // this case, it's important that the head disappears with
                    // 0 frame delay, so we proactively deactive it here.
                    _control.objHead.SetActive(false);
                    foreach (var hair in _control.objHair)
                    {
                        hair.SetActive(false);
                    }
                }
            }
            else
            {
                _control.fileStatus.visibleHeadAlways = true;
            }
        }
    }

    internal class HideMaleHeadPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.Initialize))]
        static void PostInitialize(ChaControl __instance)
        {
            if (__instance.sex == 0 && __instance.transform.parent.name.Equals("HScene"))
            {
                // In H we do this more lazily/appropriately through POV.
                __instance.GetOrAddComponent<HideMaleHead>();
            }
        }
    }
}
