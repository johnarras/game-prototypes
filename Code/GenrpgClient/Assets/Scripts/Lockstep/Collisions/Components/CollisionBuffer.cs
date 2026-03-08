using Assets.Scripts.Lockstep.Math;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Collisions.Components
{
    [InternalBufferCapacity(16)]
    public struct CollisionBuffer : IBufferElementData
    {
        public Entity CollidedWith; 
        public Vector2Fixed Normal;      // Direction to push
        public FixedPoint64 Penetration; // How far to push
    }
}
