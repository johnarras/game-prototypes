using MessagePack;
using OxDb.SharedGame.MapObjects.MapObjectAddons.Constants;
using OxDb.SharedGame.MapObjects.MapObjectAddons.Entities;
using OxDb.SharedGame.Quests.WorldData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Quests.MapObjectAddons
{
    [MessagePackObject]
    public class QuestAddon : BaseMapObjectAddon
    {
        public override long GetAddonType() { return MapObjectAddonTypes.Vendor; }

        [Key(0)] public List<QuestType> Quests { get; set; } = new List<QuestType>();
    }
}


