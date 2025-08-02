using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Spells.Interfaces;
using System.Linq;
using Genrpg.Shared.Entities.Constants;

namespace Genrpg.Shared.Tiles.Settings
{
    [MessagePackObject]
    public class TileTypeSettings : ParentSettings<TileType>
    {
        [Key(0)] public override string Id { get; set; }


        private Dictionary<long, Dictionary<long, TileType>> _effectsDict = new Dictionary<long, Dictionary<long, TileType>>();

        public override void SetData(List<TileType> data)
        {
            base.SetData(data);

            Dictionary<long, Dictionary<long, TileType>> tempDict = new Dictionary<long, Dictionary<long, TileType>>();

            foreach (TileType tileType in data)
            {
                foreach (TileEffect effect in tileType.Effects)
                {
                    if (!tempDict.ContainsKey(effect.EntityTypeId))
                    {
                        tempDict[effect.EntityTypeId] = new Dictionary<long, TileType>();
                    }
                    Dictionary<long,TileType> effectDict = tempDict[effect.EntityTypeId];

                    if (effectDict.ContainsKey(effect.EntityId))
                    {
                        effectDict.Remove(effect.EntityId);
                    }
                    effectDict[effect.EntityId] = tileType;
                }
            }

            _effectsDict = tempDict;
        }

        public TileType GetEffectTileType(long entityTypeId, long entityId)
        {
            if (_effectsDict.TryGetValue(entityTypeId, out Dictionary<long,TileType> dict))
            { 
                if (dict.ContainsKey(entityId))
                {
                    return dict[entityId];
                }
            }

            return null;
        }
    }

    [MessagePackObject]
    public class TileEffect : IEffect
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long Quantity { get; set; }
        [Key(2)] public long EntityId { get; set; }
    }


    [MessagePackObject]
    public class TileUpgradeReagent
    {
        [Key(0)] public long UserCoinTypeId { get; set; }
        [Key(1)] public long Quantity { get; set; }
    }


    [MessagePackObject]
    public class TileType : ChildSettings, IIndexedGameItem, IWeightedItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public double Weight { get; set; }
        [Key(9)] public long MinLevel { get; set; }

        [Key(10)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        [Key(11)] public List<SpawnItem> Rewards { get; set; } = new List<SpawnItem>();

        [Key(12)] public List<TileEffect> Effects { get; set; } = new List<TileEffect>();

        [Key(13)] public List<TileUpgradeReagent> UpgradeReagents { get; set; } = new List<TileUpgradeReagent>();

        public bool CanUpgrade()
        {
            return UpgradeReagents != null && UpgradeReagents.Count > 0;
        }

        public int GetEffectQuantity(long entityTypeId, long entityId)
        {
            TileEffect eff = Effects.FirstOrDefault(x=>x.EntityTypeId == entityTypeId && x.EntityId == entityId);
            return eff != null ? (int)eff.Quantity : 0;
        }

    }

    public class TileTypeSettingsDto : ParentSettingsDto<TileTypeSettings, TileType> { }

    public class TileTypeSettingsLoader : ParentSettingsLoader<TileTypeSettings, TileType> { }

    public class TileTypeSettingsMapper : ParentSettingsMapper<TileTypeSettings, TileType, TileTypeSettingsDto> { }
}
