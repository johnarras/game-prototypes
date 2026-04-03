using Unity.Entities;

namespace Assets.Scripts.Lockstep.Game
{
    [DisableAutoCreation]
    public partial class LockstepGroup : ComponentSystemGroup
    {

        protected override void OnUpdate()
        {
            // Simply call the base, but since we control AddSystemToUpdateList, 
            // the order is preserved.
            base.OnUpdate();
        }
    }
}
