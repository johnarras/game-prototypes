using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Dungeons
{
    [Serializable]
    public class DungeonDoorPanel
    {
        public int AnimateFrames = 3;
        public GameObject Anchor;
        public List<MeshRenderer> Renderers = new List<MeshRenderer>();

        public Vector3 EndAngle = Vector3.zero;
        public Vector3 EndPos = Vector3.zero;


        private Vector3 _startPos = Vector3.zero;
        private Vector3 _startAngle = Vector3.zero;

        private bool _didSetInitialData = false;
        public async Awaitable AnimateOpening(bool opening, bool upperRightOfDoor)
        {

            if (Anchor == null)
            {
                return;
            }

            if (AnimateFrames < 1)
            {
                AnimateFrames = 1;
            }
            if (!_didSetInitialData)
            {
                _startPos = Anchor.transform.localPosition;
                _startAngle = Anchor.transform.localEulerAngles;
                _didSetInitialData = true;
            }

            if (!opening)
            {
                Anchor.transform.localPosition = _startPos;
                Anchor.transform.localEulerAngles = _startAngle;
                return;
            }

            Vector3 currEndAngle = EndAngle;

            if (upperRightOfDoor)
            {
                currEndAngle = -currEndAngle;
            }

            for (int frame = 0; frame <= AnimateFrames; frame++)
            {

                int effectiveFrame = frame;

                if (!opening)
                {
                    effectiveFrame = AnimateFrames - frame;
                }


                float finalFramePos = Mathf.SmoothStep(0, 1, effectiveFrame * 1.0f / AnimateFrames);
                Vector3 pos = _startPos + Vector3.Lerp(Vector3.zero, EndPos, finalFramePos);
                Vector3 angle = _startAngle + Vector3.Lerp(Vector3.zero, currEndAngle, finalFramePos);

                Anchor.transform.localPosition = pos;
                Anchor.transform.localEulerAngles = angle;
                await Awaitable.NextFrameAsync();
            }

            await Awaitable.NextFrameAsync();
            await Task.CompletedTask;
        }
    }
}
