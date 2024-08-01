using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using KKS_VR.Interpreters;
using KKS_VR.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;
using VRGIN.Controls;
using VRGIN.Controls.Tools;
using VRGIN.Core;
using VRGIN.Helpers;

namespace KKS_VR.Controls
{
    public class GameplayTool : Tool
    {
        private KoikatuInterpreter _Interpreter;
        private KoikatuSettings _Settings;
        private KeySet _KeySet;
        private int _KeySetIndex;
        private bool _InHScene;
        private Controller.Lock _lock = VRGIN.Controls.Controller.Lock.Invalid;

        private MoveDirection? _touchDirection;
        private MoveDirection? _lastPressDirection;

        private ButtonsSubtool _buttonsSubtool;
        private GrabAction _grab;

        public override Texture2D Image => _image;
        private readonly Texture2D _hand1Texture = UnityHelper.LoadImage("icon_hand_1.png");
        private readonly Texture2D _hand2Texture = UnityHelper.LoadImage("icon_hand_2.png");
        private readonly Texture2D _handTexture = UnityHelper.LoadImage("icon_hand.png");
        private readonly Texture2D _image = new Texture2D(512, 512);
        private readonly Texture2D _school1Texture = UnityHelper.LoadImage("icon_school_1.png");
        private readonly Texture2D _school2Texture = UnityHelper.LoadImage("icon_school_2.png");
        private readonly Texture2D _schoolTexture = UnityHelper.LoadImage("icon_school.png");


        // When eneabled, exactly one of the below is non-null.


        private void ChangeKeySet()
        {
            var keySets = KeySets();

            _KeySetIndex = (_KeySetIndex + 1) % keySets.Count;
            _KeySet = keySets[_KeySetIndex];
            UpdateIcon();
        }

        private List<KeySet> KeySets()
        {
            return _InHScene ? _Settings.HKeySets : _Settings.KeySets;
        }

        private void ResetKeys()
        {
            SetScene(_InHScene);
        }

        private void SetScene(bool inHScene)
        {
            if (_buttonsSubtool != null)
            {
                _buttonsSubtool.Destroy();
                _buttonsSubtool = new ButtonsSubtool(_Interpreter, _Settings);
            }

            _InHScene = inHScene;
            var keySets = KeySets();
            _KeySetIndex = 0;
            _KeySet = keySets[0];
            UpdateIcon();
        }

        private void UpdateIcon()
        {
            var icon =
                _InHScene
                    ? _Settings.HKeySets.Count > 1
                        ? _KeySetIndex == 0
                            ? _hand1Texture
                            : _hand2Texture
                        : _handTexture
                    : _Settings.KeySets.Count > 1
                        ? _KeySetIndex == 0
                            ? _school1Texture
                            : _school2Texture
                        : _schoolTexture;
            Graphics.CopyTexture(icon, _image);
        }

        protected override void OnStart()
        {
            base.OnStart();

            _Settings = VR.Context.Settings as KoikatuSettings;
            SetScene(inHScene: false);
            _Settings.AddListener("KeySets", (_, _1) => ResetKeys());
            _Settings.AddListener("HKeySets", (_, _1) => ResetKeys());
        }

        protected override void OnDisable()
        {
            _buttonsSubtool?.Destroy();
            _buttonsSubtool = null;
            _grab?.Destroy();
            _grab = null;
            if (_lock.IsValid)
            {
                _lock.Release();
            }
            //if (_lock.IsValid) _lock.Release();
            _touchDirection = null;
            _lastPressDirection = null;
            base.OnDisable();
        }
        protected override void OnDestroy()
        {

        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _Interpreter = VR.Interpreter as KoikatuInterpreter;
            _buttonsSubtool = new ButtonsSubtool(_Interpreter, _Settings);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            //UpdateLock();

            var inHScene = _Interpreter.CurrentScene == KoikatuInterpreter.SceneType.HScene;
            if (inHScene != _InHScene) SetScene(inHScene);

            if (_grab != null)
            {
                if (_grab.HandleGrabbing() != GrabAction.Status.Continue)
                {
                    _grab.Destroy();
                    _grab = null;
                    _buttonsSubtool = new ButtonsSubtool(_Interpreter, _Settings);
                }
            }

            if (_buttonsSubtool != null)
            {
                HandleButtons();
            }
        }

        //private void UpdateLock()
        //{
        //    var wantLock = /*_grab != null ||*/ _buttonsSubtool?.WantLock() == true;
        //    if (wantLock && !_lock.IsValid)
        //        _lock = Owner.AcquireFocus( /*keepTool: true*/);
        //    else if (!wantLock && _lock.IsValid) _lock.Release();
        //}
        private void UpdateLock()
        {
            bool wantLock = _grab != null || _buttonsSubtool?.WantLock() == true;
            if (wantLock && !_lock.IsValid)
            {
                _lock = Owner.AcquireFocus(keepTool: true);
            }
            else if (!wantLock && _lock.IsValid)
            {
                _lock.Release();
            }
        }
        private void HandleButtons()
        {
            var device = this.Controller;

            if (device.GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger))
            {
                InputDown(_KeySet.Trigger, EVRButtonId.k_EButton_SteamVR_Trigger);
            }

            else if (device.GetPressUp(EVRButtonId.k_EButton_SteamVR_Trigger))
            {
                InputUp(_KeySet.Trigger);
            }

            if (device.GetPressDown(EVRButtonId.k_EButton_Grip))
            {
                InputDown(_KeySet.Grip, EVRButtonId.k_EButton_Grip);
            }

            else if (device.GetPressUp(EVRButtonId.k_EButton_Grip))
            {
                InputUp(_KeySet.Grip);
            }

            if (device.GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad))
            {
                var axis = Controller.GetAxis();
                var dir = GetTrackpadDirection(axis);
                var fun = GetTrackpadFunction(dir);
                if (RequiresPress(fun))
                {
                    _lastPressDirection = dir;
                    InputDown(fun, EVRButtonId.k_EButton_SteamVR_Touchpad);
                }
            }

            if (_buttonsSubtool == null) return;

            // 上げたときの位置によらず、押したボタンを離す
            // Release the pressed button regardless of the position when it is raised
            if (device.GetPressUp(EVRButtonId.k_EButton_SteamVR_Touchpad) && _lastPressDirection.HasValue)
            {
                InputUp(GetTrackpadFunction(_lastPressDirection.Value));
                _lastPressDirection = null;
            }

            // Handle touchpad actions that don't require a press, only touch
            var newTouchDirection = device.GetTouch(EVRButtonId.k_EButton_SteamVR_Touchpad)
                ? GetTrackpadDirection(Controller.GetAxis())
                : (MoveDirection?)null;
            if (_touchDirection != newTouchDirection)
            {
                //Console.WriteLine("changed to " + newTouchDirection);
                if (_touchDirection.HasValue)
                {
                    var oldFun = GetTrackpadFunction(_touchDirection.Value);
                    if (!RequiresPress(oldFun))
                    {
                        //Console.WriteLine("up " + oldFun);
                        InputUp(oldFun);
                    }
                }

                if (newTouchDirection.HasValue)
                {
                    var newFun = GetTrackpadFunction(newTouchDirection.Value);
                    if (!RequiresPress(newFun))
                    {
                        //Console.WriteLine("down " + newFun);
                        InputDown(newFun, EVRButtonId.k_EButton_SteamVR_Touchpad);
                    }
                }

                _touchDirection = newTouchDirection;
            }

            _buttonsSubtool.Update();
        }

        private AssignableFunction GetTrackpadFunction(MoveDirection dir)
        {
            switch (dir)
            {
                case MoveDirection.Left: return _KeySet.Left;
                case MoveDirection.Up: return _KeySet.Up;
                case MoveDirection.Right: return _KeySet.Right;
                case MoveDirection.Down: return _KeySet.Down;
                case MoveDirection.None: return _KeySet.Center;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private static MoveDirection GetTrackpadDirection(Vector2 dir)
        {
            const float deadzone = 0.5f;
            var x = dir.x;
            var y = dir.y;
            if (Mathf.Abs(x) < deadzone && y > deadzone) return MoveDirection.Up;
            if (Mathf.Abs(x) < deadzone && y < -deadzone) return MoveDirection.Down;
            if (x < -deadzone && Mathf.Abs(y) < deadzone) return MoveDirection.Left;
            if (x > deadzone && Mathf.Abs(y) < deadzone) return MoveDirection.Right;
            return MoveDirection.None;
        }

        /// <summary>
        /// When this function is assigned to trackpad, does it require a press
        /// or does a touch suffice?
        /// </summary>
        private static bool RequiresPress(AssignableFunction fun)
        {
            switch (fun)
            {
                case AssignableFunction.SCROLLDOWN:
                case AssignableFunction.SCROLLUP:
                case AssignableFunction.LROTATION:
                case AssignableFunction.RROTATION:
                    return false;
                default:
                    return true;
            }
        }

        private void InputDown(AssignableFunction fun, EVRButtonId buttonMask)
        {
            switch (fun)
            {
                case AssignableFunction.NEXT:
                    break;
                case AssignableFunction.GRAB:
                    _buttonsSubtool.Destroy();
                    _buttonsSubtool = null;
                    _grab = new GrabAction(Owner, buttonMask);
                    break;

                case AssignableFunction.SCROLLUP:
                case AssignableFunction.SCROLLDOWN:
                case AssignableFunction.LBUTTON:
                case AssignableFunction.MBUTTON:
                case AssignableFunction.RBUTTON:
                    // Move the cursor to the bottom right corner so buttons/scrolling affect the H speed control
                    // Extremely fiddly but what can you do
                    if (_InHScene) VR.Input.Mouse.MoveMouseBy(Screen.width - 10, Screen.height - 10);

                    // Force focus the window here so the cursor doesn't go off into the desktop or click the window that's currently on top of the game window
                    WindowTools.BringWindowToFront();
                    goto default;

                default:
                    _buttonsSubtool.ButtonDown(fun);
                    break;
            }
        }

        private void InputUp(AssignableFunction fun)
        {
            switch (fun)
            {
                case AssignableFunction.NEXT:
                    ChangeKeySet();
                    break;
                case AssignableFunction.GRAB:
                    break;
                default:
                    _buttonsSubtool.ButtonUp(fun);
                    break;
            }
        }

        public override List<HelpText> GetHelpTexts()
        {
            return new List<HelpText>(new[]
            {
                ToolUtil.HelpTrigger(Owner, DescriptionFor(_KeySet.Trigger)),
                ToolUtil.HelpGrip(Owner, DescriptionFor(_KeySet.Grip)),
                ToolUtil.HelpTrackpadCenter(Owner, DescriptionFor(_KeySet.Center)),
                ToolUtil.HelpTrackpadLeft(Owner, DescriptionFor(_KeySet.Left)),
                ToolUtil.HelpTrackpadRight(Owner, DescriptionFor(_KeySet.Right)),
                ToolUtil.HelpTrackpadUp(Owner, DescriptionFor(_KeySet.Up)),
                ToolUtil.HelpTrackpadDown(Owner, DescriptionFor(_KeySet.Down))
            }.Where(x => x != null));
        }

        private static string DescriptionFor(AssignableFunction fun)
        {
            var member = typeof(AssignableFunction).GetMember(fun.ToString()).FirstOrDefault();
            var descr = member?.GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>().FirstOrDefault()?.Description;
            return descr ?? fun.ToString();
        }
    }
}
