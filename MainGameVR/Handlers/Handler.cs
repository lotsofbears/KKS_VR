using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using VRGIN.Core;
using VRGIN.Controls;
using VRGIN.Helpers;
using HarmonyLib;
using UnityEngine;
using KKS_VR.Interpreters;
using KKS_VR.Settings;
using KKS_VR.Fixes;
using KKS_VR.Features;
using KKS_VR.Controls;
using RootMotion.FinalIK;
using static HandCtrl;
using KKS_VR.Caress;
using ADV.Commands.Game;
using KKS_VR.Trackers;

namespace KKS_VR.Handlers
{
    internal class Handler : MonoBehaviour
    {
        protected virtual Tracker Tracker { get; set; }
        /// <summary>
        /// True if something is being tracked. Track for recently blacklisted items continues, but new ones don't get added.
        /// </summary>
        internal virtual bool IsBusy => Tracker.IsBusy;
        /// <summary>
        /// Can be true only after 'UpdateNoBlacks()' if every item in track is blacklisted.
        /// </summary>
        internal bool InBlack => Tracker.colliderInfo == null;
        internal Transform GetTrackTransform => Tracker.colliderInfo.collider.transform;
        internal ChaControl GetChara => Tracker.colliderInfo.chara;


        protected virtual void OnDisable()
        {
            Tracker.ClearTracker();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            Tracker.AddCollider(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            Tracker.RemoveCollider(other);
        }
        internal void ClearBlacks()
        {
            Tracker.RemoveBlacks();
        }
        internal void ClearTracker()
        {
            Tracker.ClearTracker();
        }
        internal void UpdateTrackerNoBlacks()
        {
            Tracker.SetSuggestedInfoNoBlacks();
        }
        internal void UpdateTracker(ChaControl tryToAvoid = null)
        {
            Tracker.SetSuggestedInfo(tryToAvoid);
            Tracker.DebugShowActive();
        }

    }
}