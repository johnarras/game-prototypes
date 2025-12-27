using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Effects.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Tiles.Settings
{
    public class TileTypeSettings : ParentSettings<TileType>
    {
        public override string Id { get; set; }


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
                    Dictionary<long, TileType> effectDict = tempDict[effect.EntityTypeId];

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
            if (_effectsDict.TryGetValue(entityTypeId, out Dictionary<long, TileType> dict))
            {
                if (dict.ContainsKey(entityId))
                {
                    return dict[entityId];
                }
            }

            return null;
        }
    }

    public class TileEffect : IEffect
    {
        public long EntityTypeId { get; set; }
        public long Quantity { get; set; }
        public long EntityId { get; set; }
    }


    public class TileUpgradeReagent
    {
        public long UserCoinTypeId { get; set; }
        public long Quantity { get; set; }
    }


    public class TileType : ChildSettings, IIndexedGameItem, IWeightedItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public double Weight { get; set; }
        public long MinLevel { get; set; }

        public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public List<SpawnItem> Rewards { get; set; } = new List<SpawnItem>();

        public List<TileEffect> Effects { get; set; } = new List<TileEffect>();

        public List<TileUpgradeReagent> UpgradeReagents { get; set; } = new List<TileUpgradeReagent>();

        public bool CanUpgrade()
        {
            return UpgradeReagents != null && UpgradeReagents.Count > 0;
        }

        public int GetEffectQuantity(long entityTypeId, long entityId)
        {
            TileEffect eff = Effects.FirstOrDefault(x => x.EntityTypeId == entityTypeId && x.EntityId == entityId);
            return eff != null ? (int)eff.Quantity : 0;
        }

    }

    public class TileTypeSettingsDto : ParentSettingsDto<TileTypeSettings, TileType>
    {
        public override List<TileType> Children { get; set; }
        public override TileTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TileTypeSettingsLoader : ParentSettingsLoader<TileTypeSettings, TileType> { }

    public class TileTypeSettingsMapper : ParentSettingsMapper<TileTypeSettings, TileType, TileTypeSettingsDto> { }
}


