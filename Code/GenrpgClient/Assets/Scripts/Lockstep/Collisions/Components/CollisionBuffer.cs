using OxDb.Client.Lockstep.Math;
using Unity.Entities;

namespace OxDb.Client.Lockstep.Collisions.Components
{
    [InternalBufferCapacity(16)]
    public struct CollisionBuffer : IBufferElementData
    {
        public Entity CollidedWith;
        public Vector2Fixed Normal;      // Direction to push
        public FixedPoint64 Penetration; // How far to push
    }
}
