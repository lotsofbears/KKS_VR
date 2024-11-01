﻿using KKS_VR.Fixes;
using KKS_VR.Handlers;
using KKS_VR.Interactors;
using KKS_VR.Trackers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VRGIN.Controls;
using VRGIN.Helpers;
using static KKS_VR.Interactors.GraspController;

namespace KKS_VR.Handlers
{
    internal class BodyPartGuide : Handler
    {
        /// <summary>
        /// Limb index this component is associated with.
        /// </summary>
        /// 
        private HandHolder _hand;
        private BodyPart _bodyPart;
        /// <summary>
        /// Chara.transform
        /// </summary>
        private Transform _parent;
        private bool _wasBusy;
        private Rigidbody _rigidBody;
        private float _timer;
        private bool _unwind;
        private bool _attach;

        private Transform _target;
        private Vector3 _offsetPos;
        private Quaternion _offsetRot;
        private Vector3 _origScale;
        private void Awake()
        {
            _origScale = transform.localScale;
            _rigidBody = gameObject.AddComponent<Rigidbody>();
            _rigidBody.isKinematic = true;
            _rigidBody.freezeRotation = true;

            // Implement accurate, controlled play with this.
            _rigidBody.useGravity = false;

            // Default primitive collider - Trigger.
            var colliderTrigger = gameObject.GetComponent<SphereCollider>();
            colliderTrigger.isTrigger = true;

            // RigidBody's slave.
            var sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = false;
            sphere.radius = Mathf.Round(1000f * (colliderTrigger.radius * 0.7f)) * 0.001f;
            Tracker = new Tracker();
            gameObject.SetActive(false);
        }
        private void OnEnable()
        {
            _wasBusy = false;
        }
        internal void Init(BodyPart bodyPart, ChaControl chara)
        {
            _bodyPart = bodyPart;
            _parent = chara.transform;
        }
        internal void Follow(Transform target = null, HandHolder hand = null)
        {
            _hand = hand;
            _attach = false;
            _rigidBody.velocity = Vector3.zero;
            if (!gameObject.activeSelf)
            {
                _bodyPart.anchor.parent = null;
                SetBodyPartCollidersToTrigger(true);
                transform.SetPositionAndRotation(_bodyPart.afterIK.position, _bodyPart.afterIK.rotation);
                gameObject.SetActive(true);
                VRPlugin.Logger.LogDebug($"{_bodyPart.name} guide wakes up");
                transform.parent = _parent;
                _bodyPart.anchor.parent = transform;
            }
            if (target == null)
            {
                _target = _bodyPart.beforeIK;
            }
            else
            {
                _target = target;
                Tracker.SetBlacklistDic(hand.Grasp.GetBlacklistDic);
                ClearBlacks();
            }
            _bodyPart.visual.SetColor(IsBusy);
            _wasBusy = false;
            // Default offsets based on target provided, through which we consequently interpret the orientation we move towards.
            _offsetRot = Quaternion.Inverse(_target.rotation) * transform.rotation;
            _offsetPos = _target.InverseTransformPoint(transform.position);
        }
        internal void Stay()
        {
            VRPlugin.Logger.LogDebug($"{_bodyPart.name} guide goes to sleep");
            _target = null;
            _attach = false;
            gameObject.SetActive(false);
            transform.SetParent(_bodyPart.afterIK, false);
            transform.localScale = _origScale;
            SetBodyPartCollidersToTrigger(false);
        }
        internal void Attach(Transform target)
        {
            VRPlugin.Logger.LogDebug($"{_bodyPart.name} guide follows orientation of {target.name}");
            _hand = null;
            _attach = true;
            _target = target;
            SetBodyPartCollidersToTrigger(false);
            //transform.SetPositionAndRotation(_bodyPart.afterIK.position, _bodyPart.afterIK.rotation);
            transform.parent = _parent;
            _offsetPos = target.InverseTransformPoint(transform.position);
            _offsetRot = Quaternion.Inverse(target.rotation) * transform.rotation;
        }
        internal void SetBodyPartCollidersToTrigger(bool active)
        {
            // To let rigidBody run free. Currently on hold, we use kinematic for a moment.
            foreach (var kv in _bodyPart.colliders)
            {
                kv.Key.isTrigger = active || kv.Value;
                VRPlugin.Logger.LogDebug($"{_bodyPart.name} set {kv.Key.name}.Trigger = {kv.Key.isTrigger}[{kv.Value}]");
            }
        }

        // Not pretty, through controller will be much easier.
        //private void OnCollisionEnter(Collision collision)
        //{
        //    //VRPlugin.Logger.LogDebug($"OnCollisionEnter:{collision.gameObject.name}");
        //}

        protected override void OnTriggerEnter(Collider other)
        {
            if (Tracker.AddCollider(other))
            {
                if (!_wasBusy)
                {
                    _wasBusy = true;
                    if (_hand != null)
                    {
                        _hand.Controller.StartRumble(new RumbleImpulse(500));
                        _bodyPart.visual.SetColor(true);
                    }
                }
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (Tracker.RemoveCollider(other))
            {
                if (!IsBusy)
                {
                    _wasBusy = false;
                    _unwind = true;
                    _timer = 1f;
                    if (_hand != null)
                    {
                        _bodyPart.visual.SetColor(false);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (_unwind)
            {
                _timer = Mathf.Clamp01(_timer - Time.deltaTime);
                _rigidBody.velocity *= _timer;
                if (_timer == 0f)
                {
                    _unwind = false;
                }
            }
            if (_attach)
            {
                transform.SetPositionAndRotation(
                    _target.TransformPoint(_offsetPos),
                    _target.rotation * _offsetRot
                    );
            }
        }

        private void FixedUpdate()
        {
            if (!_attach)
            {
                _rigidBody.MovePosition(_target.TransformPoint(_offsetPos));
                _rigidBody.MoveRotation(_target.rotation * _offsetRot);
            }
        }
    }
}
