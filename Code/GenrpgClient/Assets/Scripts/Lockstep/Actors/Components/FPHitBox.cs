using Assets.Scripts.Lockstep.Math;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Actors.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct FPHitbox : IComponentData
    {
        // Half-extents are usually easier for math: 
        // A 1x1 tile would have Extents of (0.5, 0.5)
        public Vector2Fixed HalfExtents;

        // You might want a layer mask for "Player", "Enemy", "Wall"
        public uint CollisionMask;

        public static FPHitbox Create(int width, int height, uint mask)
        {
            return new FPHitbox
            {
                // We divide by 2 to get half-extents
                HalfExtents = new Vector2Fixed(
                    FixedPoint64.FromInt(width) / FixedPoint64.FromInt(2),
                    FixedPoint64.FromInt(height) / FixedPoint64.FromInt(2)
                ),
                CollisionMask = mask
            };
        }
    }
}