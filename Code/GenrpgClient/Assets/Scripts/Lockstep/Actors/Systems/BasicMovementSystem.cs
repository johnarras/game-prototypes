using Assets.Scripts.Lockstep.Actors.Components;
using Assets.Scripts.Lockstep.Game;
using Assets.Scripts.Lockstep.Math;
using Assets.Scripts.Lockstep.Systems;
using Assets.Scripts.Lockstep.Systems.Constants;
using Unity.Burst;
using Unity.Entities;

[DisableAutoCreation]
[BurstCompile]
public partial struct BasicMovementSystem : ISeededSystem, ISystem
{
    public uint SystemId => SystemSeeds.BaseMovement;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // This call automatically ensures any previous systems 
        // writing to LockstepGlobalState are finished.
        LockstepGlobalState global = SystemAPI.GetSingleton<LockstepGlobalState>();

        MovementJob moveJob = new MovementJob
        {
            DeltaTime = global.DeltaTime
        };

        // By passing state.Dependency into the schedule method, 
        // we chain this job to whatever the previous system was doing.
        state.Dependency = moveJob.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    [WithNone(typeof(PendingRemoval))]
    public partial struct MovementJob : IJobEntity
    {
        public FixedPoint64 DeltaTime;

        public void Execute(ref ActorPosition pos, in ActorRotation rot, in ActorSpeed speed)
        {
            FixedPoint64 cos = FixedPointMath.Cos(rot.Angle);
            FixedPoint64 sin = FixedPointMath.Sin(rot.Angle);

            pos.Pos.X += cos * speed.Speed * DeltaTime;
            pos.Pos.Z += sin * speed.Speed * DeltaTime;
        }
    }
}