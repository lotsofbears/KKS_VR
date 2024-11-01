﻿using System;
using System.Collections.Generic;
using System.Linq;
using KKS_VR.Features;
using KKS_VR.Settings;
using UnityEngine.XR;
using VRGIN.Controls;
using VRGIN.Modes;

namespace KKS_VR
{
    /// <summary>
    /// Initialize controllers and custom tools
    /// </summary>
    internal class GameStandingMode : StandingMode
    {
        public override IEnumerable<Type> Tools { get; } = new[]
        {
            typeof(Controls.GameplayTool)
        };

        protected override IEnumerable<IShortcut> CreateShortcuts()
        {
            // Disable all VRGIN shortcuts. We'll define necessary shortcuts
            // (if any) by ourselves.
            return Enumerable.Empty<IShortcut>();
        }

        protected override Controller CreateLeftController()
        {
            var controller = base.CreateLeftController();
            AddComponents(controller, EyeSide.Left);
            controller.ToolIndex = 0;
            return controller;
        }

        protected override Controller CreateRightController()
        {
            var controller = base.CreateRightController();
            AddComponents(controller, EyeSide.Right);
            controller.ToolIndex = 0;
            return controller;
        }

        private static void AddComponents(Controller controller, EyeSide controllerSide)
        {
            controller.gameObject.AddComponent<Controls.LocationPicker>();
        }

        protected override void SyncCameras()
        {
            // Do nothing. CameraControlControl and friends take care of this.
        }

        protected override void InitializeScreenCapture()
        {
            // Don't enable CapturePanorama because it looks broken (throws an
            // exception).
        }
    }
}
