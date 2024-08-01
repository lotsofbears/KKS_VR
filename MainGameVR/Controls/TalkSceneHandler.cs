using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Valve.VR;
using VRGIN.Controls;
using VRGIN.Core;
using VRGIN.Helpers;

namespace KKS_VR.Controls
{
    /// <summary>
    /// A handler component to be attached to a controller, providing touch/look
    /// functionalities in talk scenes.
    /// This component is meant to remain disabled outside talk scenes.
    /// </summary>
    internal class TalkSceneHandler : ProtectedBehaviour
    {
        private Controller _controller;

        private readonly HashSet<Collider> _currentlyIntersecting
            = new HashSet<Collider>();

        private Controller.Lock _lock; // null or valid
        private TalkScene _talkScene;

        protected override void OnStart()
        {
            base.OnStart();

            _controller = GetComponent<Controller>();
            _talkScene = FindObjectOfType<TalkScene>();
            if (_talkScene == null) VRLog.Warn("TalkSceneHandler: TalkScene not found");
        }

        protected void OnDisable()
        {
            _currentlyIntersecting.Clear();
            UpdateLock();
        }

        protected override void OnUpdate()
        {
            if (_lock != null) HandleTrigger();
        }

        private void HandleTrigger()
        {
            var device = _controller.Input; //SteamVR_Controller.Input((int)_controller.Tracking.index);
            if (device.GetPressUp(EVRButtonId.k_EButton_SteamVR_Trigger)) PerformAction();
        }

        private void PerformAction()
        {
            // Find the nearest intersecting collider.
            var nearest = _currentlyIntersecting
                .OrderBy(_col => (_col.transform.position - transform.position).sqrMagnitude)
                .FirstOrDefault();
            if (nearest == null) return;
            var kind = Extensions.StripPrefix("Com/Hit/", nearest.tag);
            if (kind != null) new Traverse(_talkScene).Method("TouchFunc", new[] { typeof(string), typeof(Vector3) }).GetValue(kind, Vector3.zero);
        }

        protected void OnTriggerEnter(Collider other)
        {
            var wasIntersecting = _currentlyIntersecting.Count > 0;
            if (other.tag.StartsWith("Com/Hit/"))
            {
                _currentlyIntersecting.Add(other);
                if (!wasIntersecting) _controller.StartRumble(new RumbleImpulse(1000));
                UpdateLock();
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (_currentlyIntersecting.Remove(other)) UpdateLock();
        }

        private void UpdateLock()
        {
            if (_currentlyIntersecting.Count > 0 && _lock == null)
            {
                _controller.TryAcquireFocus(out _lock);
            }
            else if (_currentlyIntersecting.Count == 0 && _lock != null)
            {
                _lock.Release();
                _lock = null;
            }
        }
    }
}
