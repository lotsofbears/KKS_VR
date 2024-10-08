using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;
using VRGIN.Controls.Tools;
using VRGIN.Core;

namespace VRGIN.Modes
{
    public class StandingMode : ControlMode
    {
        public override IEnumerable<Type> Tools => base.Tools.Concat(new Type[2]
        {
            typeof(MenuTool),
            typeof(WarpTool)
        });

        public override ETrackingUniverseOrigin TrackingOrigin => ETrackingUniverseOrigin.TrackingUniverseStanding;

        public override void Impersonate(IActor actor, ImpersonationMode mode)
        {
            base.Impersonate(actor, mode);
            MoveToPosition(actor.Eyes.position, actor.Eyes.rotation, mode == ImpersonationMode.Approximately);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnStart()
        {
            base.OnStart();
            VR.Camera.SteamCam.origin.position = Vector3.zero;
            VR.Camera.SteamCam.origin.rotation = Quaternion.identity;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (VRCamera.Instance.HasValidBlueprint) SyncCameras();
        }

        protected virtual void SyncCameras()
        {
            VRCamera.Instance.Blueprint.transform.position = VR.Camera.SteamCam.head.position;
            VRCamera.Instance.Blueprint.transform.rotation = VR.Camera.SteamCam.head.rotation;
        }
    }
}
