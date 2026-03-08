using Assets.Scripts.Lockstep.Collisions.Constants;
using Assets.Scripts.Lockstep.Math;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Collisions.Components
{
    public struct CollisionShape : IComponentData
    {
        public ECollisionShapeType ShapeType;

        // For Circle: X is Radius.
        // For Rectangle: X is HalfWidth, Y is HalfHeight (Extents).
        public Vector2Fixed Extents;

        public static CollisionShape CreateCircle(FixedPoint64 radius)
        {
            return new CollisionShape
            {
                ShapeType = ECollisionShapeType.Circle,
                Extents = new Vector2Fixed(radius, FixedPoint64.FromInt(0))
            };
        }

        public static CollisionShape CreateRect(FixedPoint64 halfWidth, FixedPoint64 halfHeight)
        {
            return new CollisionShape
            {
                ShapeType = ECollisionShapeType.Rectangle,
                Extents = new Vector2Fixed(halfWidth, halfHeight)
            };
        }
    }
}
