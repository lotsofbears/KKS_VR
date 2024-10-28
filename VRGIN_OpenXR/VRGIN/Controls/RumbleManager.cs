using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRGIN.Core;
using VRGIN.Helpers;

namespace VRGIN.Controls
{
    public class RumbleManager : ProtectedBehaviour
    {
        private const float MILLI_TO_SECONDS = 0.001f;

        public const float MIN_INTERVAL = 0.0050000004f;

        private HashSet<IRumbleSession> _RumbleSessions = new HashSet<IRumbleSession>();

        private float _LastImpulse;

        private Controller _Controller;

        protected override void OnStart()
        {
            base.OnStart();
            _Controller = GetComponent<Controller>();
        }

        protected virtual void OnDisable()
        {
            _RumbleSessions.Clear();
        }

        // Broken.
        // What i've gather so far:
        // The input that we send to 'Unity.XR.OpenVR' seems fine, device checks out, played around with timings but for naught.
        // no errors all the way to the delegate to unmanaged code at 'Unity.XR.OpenVR' side.
        // It also has different methods with a tad different inputs for haptic feedback, but none work.
        // Atleast some delegates to unmanaged code work. As i understand unmanaged code of 'Unity.XR.OpenVR' references SteamVR. We can't get into that.
        // Haptic feedback that happens on trigger/grip doesn't produce a single call related to haptic in any library shipped with the plugin. 
        // Took a peak at those packages in unity, and it all seems VERY strange.
        // My very best bet that i'm missing something crucial right before my eyes. As SteamVR shouldn't trigger no haptic on input by default, yet we have it.


        //protected override void OnUpdate()
        //{
        //    base.OnUpdate();
        //    if (_RumbleSessions.Count <= 0) return;
        //    var rumbleSession = _RumbleSessions.Max();
        //    var num = Time.unscaledTime - _LastImpulse;
        //    if (!_Controller.Tracking.isValid || !(num >= rumbleSession.MilliInterval * 0.001f) || !(num > 0.0050000004f)) return;
        //    if (rumbleSession.IsOver)
        //    {
        //        _RumbleSessions.Remove(rumbleSession);
        //        return;
        //    }

        //    if (VR.Settings.Rumble) _Controller.Input.TriggerHapticPulse(rumbleSession.MicroDuration);
        //    _LastImpulse = Time.unscaledTime;
        //    rumbleSession.Consume();
        //}

        public void StartRumble(IRumbleSession session)
        {
            _RumbleSessions.Add(session);
        }

        internal void StopRumble(IRumbleSession session)
        {
            _RumbleSessions.Remove(session);
        }
    }
}
