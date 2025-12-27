using Genrpg.Shared.MapMods.MapObjects;
using Genrpg.Shared.MapObjects.Messages;

namespace Assets.Scripts.GroundObjects
{
    public class MapModObject : InteractableObject
    {
        private OnSpawn _spawn;
        private MapMod _mod;
        public void Init(OnSpawn spawn)
        {
            _spawn = spawn;

            _mod = new MapMod()
            {
                EntityTypeId = spawn.EntityTypeId,
                EntityId = spawn.EntityId,
                AddonBits = spawn.AddonBits,
            };
        }

        public MapMod GetMod()
        {
            return _mod;
        }

        protected override void _RightClick(float distance)
        {
            if (!CanInteract())
            {
                return;
            }

        }

        protected override void _OnPointerEnter()
        {
            _cursorService.SetCursor(CursorNames.Interact);
        }
    }
}


