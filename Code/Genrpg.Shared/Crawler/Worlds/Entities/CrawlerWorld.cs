using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Worlds.Entities
{

    public class CrawlerWorld : IStringId, IIdName
    {
        public string Id { get; set; }
        public long IdKey { get; set; }
        public string Name { get; set; }

        public List<CrawlerMap> Maps { get; set; } = new List<CrawlerMap>();

        public List<WorldQuestItem> QuestItems { get; set; } = new List<WorldQuestItem>();

        public long Seed { get; set; }

        public List<CrawlerNpc> Npcs { get; set; } = new List<CrawlerNpc>();

        public List<CrawlerQuest> Quests { get; set; } = new List<CrawlerQuest>();


        public void ClearCache()
        {
            _mapCache = null;
            _mapQuests = null;
            _questCache = null;

        }

        public long GetMaxMapId()
        {
            if (Maps.Count < 1)
            {
                return 0;
            }
            return Maps.Max(x => x.IdKey);
        }

        private Dictionary<long, List<CrawlerQuest>> _mapQuests = null;
        private Dictionary<long, CrawlerQuest> _questCache = null;
        public List<CrawlerQuest> GetQuestsForMap(long mapId)
        {
            SetupQuestCache();
            CrawlerMap map = GetMap(mapId);

            if (map == null)
            {
                return new List<CrawlerQuest>();
            }
            if (_mapQuests.TryGetValue(map.BaseCrawlerMapId, out List<CrawlerQuest> list))
            {
                return list;
            }

            return new List<CrawlerQuest>();
        }

        public CrawlerNpc GetNpc(long crawlerNpcId)
        {
            return Npcs.FirstOrDefault(x => x.IdKey == crawlerNpcId);
        }

        public void AddQuest(CrawlerQuest quest)
        {
            Quests.Add(quest);
            ClearCache();
        }

        public CrawlerQuest GetQuest(long questId)
        {
            SetupQuestCache();
            if (_questCache.TryGetValue(questId, out CrawlerQuest quest))
            {
                return quest;
            }
            return null;
        }

        private void SetupQuestCache()
        {
            if (_mapQuests == null || _questCache == null)
            {
                _mapQuests = new Dictionary<long, List<CrawlerQuest>>();
                _questCache = new Dictionary<long, CrawlerQuest>();

                foreach (CrawlerQuest quest in Quests)
                {
                    _questCache[quest.IdKey] = quest;
                    CrawlerMap map = GetMap(quest.CrawlerMapId);

                    if (map != null)
                    {
                        if (!_mapQuests.ContainsKey(map.BaseCrawlerMapId))
                        {
                            _mapQuests[map.BaseCrawlerMapId] = new List<CrawlerQuest>();
                        }
                        _mapQuests[map.BaseCrawlerMapId].Add(quest);
                    }
                }
            }
        }

        public void AddMap(CrawlerMap map)
        {
            Maps.Add(map);
            ClearCache();
        }

        private Dictionary<long, CrawlerMap> _mapCache = null;
        public CrawlerMap GetMap(long mapId)
        {
            SetupMapCache();

            if (_mapCache.TryGetValue(mapId, out CrawlerMap map))
            {
                return map;
            }
            return null;
        }

        private void SetupMapCache()
        {
            if (_mapCache == null)
            {
                _mapCache = new Dictionary<long, CrawlerMap>();
                foreach (CrawlerMap map in Maps)
                {
                    _mapCache[map.IdKey] = map;
                }
            }
        }
    }
}


