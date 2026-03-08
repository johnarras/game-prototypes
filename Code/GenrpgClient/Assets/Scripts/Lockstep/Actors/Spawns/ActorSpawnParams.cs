using Assets.Scripts.Lockstep.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Lockstep.Actors.Spawns
{
    public class ActorSpawnParams
    {
        public int MapId;
        public Vector2Fixed Pos;
        public FixedPoint64 Angle;
        public FixedPoint64 Speed;
        // We can add things like "LifeSpan" later
    }
}
