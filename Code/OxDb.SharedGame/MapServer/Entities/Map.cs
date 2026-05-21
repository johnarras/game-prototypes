using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.MapServer.Constants;
using OxDb.SharedGame.DataStores.Categories.WorldData;
using OxDb.SharedGame.Quests.WorldData;
using OxDb.SharedGame.Zones.WorldData;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.MapServer.Entities
{
    public class Map : BaseWorldData, IName, IMapRoot
    {
        public override void Delete(IRepositoryService repoSystem) { repoSystem.Delete(this); }
        public override string Id { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }

        public int BlockCount { get; set; }
        public float ZoneSize { get; set; }

        public long Seed { get; set; }

        public int MapVersion { get; set; }

        public int SpawnX { get; set; }
        public int SpawnY { get; set; }

        public long OverrideZoneId { get; set; }
        public float OverrideZonePercent { get; set; }

        public List<QuestType> Quests { get; set; }
        public List<QuestItem> QuestItems { get; set; }
        public List<Zone> Zones { get; set; }

        public Map()
        {
            Quests = new List<QuestType>();
            Zones = new List<Zone>();
            QuestItems = new List<QuestItem>();
            SpawnX = -1;
            SpawnY = -1;
        }

        public int GetHwid()
        {
            return BlockCount * (SharedMapConstants.TerrainPatchSize - 1) + 1;
        }

        public int GetHhgt()
        {
            return BlockCount * (SharedMapConstants.TerrainPatchSize - 1) + 1;
        }

        public virtual T Get<T>(long id) where T : class, IIdName
        {
            if (typeof(T) == typeof(Zone))
            {
                return Zones.FirstOrDefault(x => x.IdKey == id) as T;
            }
            else if (typeof(T) == typeof(QuestType))
            {
                return Quests.FirstOrDefault(x => x.IdKey == id) as T;
            }
            else if (typeof(T) == typeof(QuestItem))
            {
                return QuestItems.FirstOrDefault(x => x.IdKey == id) as T;
            }
            return default;
        }
        public virtual void ClearIndex() { }

        public int GetMapSize()
        {
            if (BlockCount < 4)
            {
                return SharedMapConstants.DefaultHeightmapSize;
            }

            return BlockCount * (SharedMapConstants.TerrainPatchSize - 1) + 1;
        }

        public bool IsSingleZone()
        {
            return ZoneSize >= BlockCount;
        }

        public List<IIdName> GetEditorListFromEntityTypeId(long entityTypeId)
        {
            if (entityTypeId == EntityTypes.Quest)
            {
                return Quests.Cast<IIdName>().ToList();
            }
            else if (entityTypeId == EntityTypes.QuestItem || entityTypeId == EntityTypes.GroundObject)
            {
                return QuestItems.Cast<IIdName>().ToList(); ;
            }
            else if (entityTypeId == EntityTypes.Zone)
            {
                return Zones.Cast<IIdName>().ToList();
            }

            return null;
        }

        public object GetEditorListFromName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (name.IndexOf("ZoneId") >= 0)
            {
                return Zones;
            }
            if (name.IndexOf("QuestTypeId") >= 0)
            {
                return Quests;
            }

            if (name.IndexOf("QuestItemId") >= 0)
            {
                return QuestItems;
            }

            return null;
        }

        public void CleanForClient()
        {
            foreach (Zone zone in Zones)
            {
                zone.CleanForClient();
            }
            Quests = new List<QuestType>();
        }
    }
}


