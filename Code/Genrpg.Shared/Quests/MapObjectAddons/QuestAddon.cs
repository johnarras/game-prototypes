using Genrpg.Shared.MapObjects.MapObjectAddons.Constants;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.Quests.WorldData;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Quests.MapObjectAddons
{
    [MessagePackObject]
    public class QuestAddon : BaseMapObjectAddon
    {
        public override long GetAddonType() { return MapObjectAddonTypes.Vendor; }

        [Key(0)] public List<QuestType> Quests { get; set; } = new List<QuestType>();
    }
}


