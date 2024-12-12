using UnityEngine;
using static VRGIN.Controls.Controller;
using Valve.VR;

namespace KK_VR.Interpreters
{
    internal class InputWait
    {
        internal InputWait(int _index, TrackpadDirection _direction, bool _manipulateSpeed, float _duration)
        {
            index = _index;
            direction = _direction;
            manipulateSpeed = _manipulateSpeed;
            SetDuration(_duration);
        }
        internal InputWait(int _index, EVRButtonId _button, float _duration)
        {
            index = _index;
            button = _button;
            SetDuration(_duration);
        }
        private void SetDuration(float _duration)
        {
            duration = _duration;
            timestamp = Time.time;
            finish = Time.time + _duration;
        }
        internal int index;
        internal TrackpadDirection direction;
        internal EVRButtonId button;
        internal bool manipulateSpeed;
        internal float timestamp;
        internal float duration;
        internal float finish;
    }
}
