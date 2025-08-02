using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.Shared.Spells.Helpers
{

    public class SpellRewardHelper : IRewardHelper
    {
        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData, RewardParams rp)
        {
            return true;
        }
        public long Key => EntityTypes.Spell;
    }
}
