using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Services.Reflection;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Interfaces;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers.Crawler
{

    public class UnitImportRow
    {
        public long Idkey { get; set; } // These are explicit in the imports so that we can remove and not mess them up.
        public string Name { get; set; }
        public string PluralName { get; set; }
        public string TribeName { get; set; }
        public int MinLevel { get; set; }
        public int MinRange { get; set; }
        public string StatPercents { get; set; }
        public string Stats { get; set; }
        public float SpawnQuantityScale { get; set; }
        public string Vulns { get; set; }
        public string Resists { get; set; }
        public string Summons { get; set; }
        public string Procs { get; set; }
        public string Spells { get; set; }
        public string CommonSpawns { get; set; }
        public string UncommonSpawns { get; set; }
        public string RareSpawns { get; set; }
        public double Weight { get; set; }
        public int Tier { get; set; }
        public string KeywordNames { get; set; }

    }

    public class UnitSummons
    {
        public IUnitRole UnitType { get; set; }
        public string Summons { get; set; }
    }

    public abstract class BaseUnitDataImporter<TParent, TChild> : BaseCrawlerDataImporter where TParent : ParentSettings<TChild> where TChild : ChildSettings, IUnitRole, new()
    {

        IEditorReflectionService _reflectionService = null;
        protected ITextSerializer _serializer = null;

        public abstract override string ImportDataFilename { get; }

        public abstract override EImportTypes Key { get; }

        public abstract long GetEntityTypeId();

        protected override async Task<bool> ParseInputFromLines(WindowBase window, EditorGameState gs, List<string[]> lines)
        {
            string[] firstLine = lines[0];

            List<UnitSummons> summons = new List<UnitSummons>();

            TParent settings = gs.data.Get<TParent>(null);

            IReadOnlyList<TChild> startUnitTypes = settings.GetData();

            IReadOnlyList<CrawlerSpell> crawlerSpells = gs.data.Get<CrawlerSpellSettings>(null).GetData();

            IReadOnlyList<StatusEffect> statusEffects = gs.data.Get<StatusEffectSettings>(null).GetData();

            IReadOnlyList<ElementType> elementTypes = gs.data.Get<ElementTypeSettings>(null).GetData();

            IReadOnlyList<TribeType> tribes = gs.data.Get<TribeSettings>(null).GetData();

            IReadOnlyList<UnitType> unitTypes = gs.data.Get<UnitTypeSettings>(null).GetData();

            IReadOnlyList<UnitKeyword> allUnitKeywords = gs.data.Get<UnitKeywordSettings>(null).GetData();

            IReadOnlyList<ZoneType> zoneTypes = gs.data.Get<ZoneTypeSettings>(null).GetData();

            foreach (ZoneType zoneType in zoneTypes)
            {
                zoneType.ZoneUnitSpawns = new List<ZoneUnitSpawn>();
                gs.LookedAtObjects.Add(zoneType);
            }

            gs.LookedAtObjects.Add(gs.data.Get<TParent>(null));

            List<TChild> newList = new List<TChild>();



            for (int l = 1; l < lines.Count; l++)
            {
                string[] words = lines[l];

                UnitImportRow importRow = _importService.ImportLine<UnitImportRow>(gs, l, words, firstLine, null, true);

                if (string.IsNullOrEmpty(importRow.Name))
                {
                    continue;
                }

                long tribeTypeId = 0;
                TribeType tribeType = null;
                if (!string.IsNullOrEmpty(importRow.TribeName))
                {
                    tribeType = tribes.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == StrUtils.NormalizeWord(importRow.TribeName));
                    if (tribeType != null)
                    {
                        tribeTypeId = tribeType.IdKey;
                    }
                }

                TChild child = _serializer.ConvertType<UnitImportRow, TChild>(importRow);
                newList.Add(child);

                child.Icon = child.Name.Replace(" ", "");
                child.Art = child.Name.Replace(" ", "");

                child.Icon = child.Name.Replace(" ", "");
                child.Art = child.Icon;

                List<UnitKeyword> keywords = new List<UnitKeyword>();

                if (child is IKeywordList ikl && !string.IsNullOrEmpty(importRow.KeywordNames))
                {
                    string[] names = importRow.KeywordNames.Split(',');

                    foreach (string name in names)
                    {
                        UnitKeyword keyword = allUnitKeywords.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == StrUtils.NormalizeWord(name));

                        if (keyword != null)
                        {
                            keywords.Add(keyword);
                        }
                    }
                    foreach (UnitKeyword kw in keywords)
                    {
                        ikl.Keywords.Add(new CurrentUnitKeyword() { UnitKeywordId = kw.IdKey });
                    }
                }


                if (importRow.Summons != null)
                {
                    List<string> summonList = importRow.Summons.Split(',').ToList();

                    foreach (string summon in summonList)
                    {
                        string lowerSummon = summon.Trim();
                        if (!string.IsNullOrEmpty(lowerSummon))
                        {
                            UnitSummons newSummons = new UnitSummons()
                            {
                                Summons = importRow.Summons,
                                UnitType = child,
                            };
                        }
                    }
                }

                if (tribeTypeId > 0)
                {
                    _reflectionService.SetObjectValue(child, "TribeTypeId", tribeTypeId);

                }
                if (tribeType != null)
                {

                    UnitKeyword tribeKeyword = allUnitKeywords.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == StrUtils.NormalizeWord(tribeType.Name));
                    if (tribeKeyword != null)
                    {
                        keywords.Add(tribeKeyword);
                    }
                }


                child.Effects = new List<UnitEffect>();

                _importService.AddEffectList<TParent, StatSettings, StatType, UnitEffect>(gs, l, "StatPercents", EntityTypes.StatPct, child.Effects, importRow.StatPercents);
                _importService.AddEffectList<TParent, StatSettings, StatType, UnitEffect>(gs, l, "Stats", EntityTypes.Stat, child.Effects, importRow.Stats);
                _importService.AddEffectList<TParent, ElementTypeSettings, ElementType, UnitEffect>(gs, l, "Vulns", EntityTypes.Vulnerability, child.Effects, importRow.Vulns);
                _importService.AddEffectList<TParent, ElementTypeSettings, ElementType, UnitEffect>(gs, l, "Resists", EntityTypes.Resist, child.Effects, importRow.Resists);
                _importService.AddEffectList<TParent, StatusEffectSettings, StatusEffect, UnitEffect>(gs, l, "Procs", EntityTypes.StatusEffect, child.Effects, importRow.Procs);
                _importService.AddEffectList<TParent, CrawlerSpellSettings, CrawlerSpell, UnitEffect>(gs, l, "Spells", EntityTypes.CrawlerSpell, child.Effects, importRow.Spells);


                ImportSpawns(gs, GetEntityTypeId(), child.IdKey, 1, importRow.RareSpawns);
                ImportSpawns(gs, GetEntityTypeId(), child.IdKey, 10, importRow.UncommonSpawns);
                ImportSpawns(gs, GetEntityTypeId(), child.IdKey, 100, importRow.CommonSpawns);
            }

            List<UnitType> newUnits = new List<UnitType>();

            foreach (TChild child in newList)
            {
                if (child is UnitType utype)
                {
                    newUnits.Add(utype);
                }
            }

            foreach (UnitSummons summon in summons)
            {
                if (!string.IsNullOrEmpty(summon.Summons))
                {
                    string[] slist = summon.Summons.Split(' ');

                    foreach (string word in slist)
                    {
                        string lowerWord = StrUtils.NormalizeWord(word);

                        long summonIdkey = 0;

                        if (lowerWord == "self")
                        {
                            summonIdkey = summon.UnitType.IdKey;
                        }
                        else if (lowerWord == "base")
                        {
                            TChild child = newList.FirstOrDefault(x => summon.UnitType.Name.Contains(x.Name));
                            if (child != null && child != summon.UnitType)
                            {
                                summonIdkey = child.IdKey;
                            }
                        }
                        else
                        {
                            UnitType namedType = unitTypes.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == lowerWord);

                            if (namedType != null)
                            {
                                summonIdkey = namedType.IdKey;
                            }
                            else
                            {
                                namedType = newUnits.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == lowerWord);

                                if (namedType != null)
                                {
                                    summonIdkey = namedType.IdKey;
                                }
                            }
                        }

                        if (summonIdkey > 0)
                        {
                            summon.UnitType.Effects.Add(new UnitEffect() { EntityTypeId = EntityTypes.CrawlerSpell, EntityId = summonIdkey + CrawlerSpellConstants.MonsterSummonSpellIdOffset, Quantity = 1 });
                        }

                    }
                }
            }

            settings.SetData(newList);
            foreach (TChild utype in newList)
            {
                gs.LookedAtObjects.Add(utype);
            }
            await Task.CompletedTask;
            return true;
        }

        private void ImportSpawns(EditorGameState gs, long entityTypeId, long entityId, double spawnWeight, string spawnZoneList)
        {

            if (string.IsNullOrEmpty(spawnZoneList))
            {
                return;
            }

            string[] words = spawnZoneList.Split(',');


            IReadOnlyList<ZoneType> zoneTypes = gs.data.Get<ZoneTypeSettings>(null).GetData();

            IReadOnlyList<ZoneCategory> zoneCategories = gs.data.Get<ZoneCategorySettings>(null).GetData();

            for (int w = 0; w < words.Length; w++)
            {
                string word = StrUtils.NormalizeWord(words[w]);

                ZoneType ztype = zoneTypes.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == word);

                if (ztype != null)
                {
                    AddSpawnWeight(entityTypeId, entityId, spawnWeight, ztype);
                }
                else
                {
                    ZoneCategory category = zoneCategories.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == word);

                    if (category != null)
                    {
                        List<ZoneType> categoryZones = zoneTypes.Where(x => x.ZoneCategoryId == category.IdKey).ToList();

                        foreach (ZoneType zoneType in categoryZones)
                        {
                            AddSpawnWeight(entityTypeId, entityId, spawnWeight, zoneType);
                        }
                    }
                }
            }
        }

        private void AddSpawnWeight(long entityTypeId, long entityId, double weight, ZoneType zoneType)
        {

            if (entityTypeId == EntityTypes.Unit)
            {
                ZoneUnitSpawn currSpawn = zoneType.ZoneUnitSpawns.FirstOrDefault(x => x.UnitTypeId == entityId);

                if (currSpawn == null)
                {
                    currSpawn = new ZoneUnitSpawn() { UnitTypeId = entityId };
                    zoneType.ZoneUnitSpawns.Add(currSpawn);
                }

                currSpawn.Weight = Math.Max(currSpawn.Weight, weight);
            }
            else if (entityTypeId == EntityTypes.UnitKeyword)
            {
                ZoneUnitKeyword keyword = zoneType.UnitKeyWords.FirstOrDefault(x => x.UnitKeywordId == entityId);

                if (keyword == null)
                {
                    keyword = new ZoneUnitKeyword() { UnitKeywordId = entityId };
                    zoneType.UnitKeyWords.Add(keyword);
                }

                keyword.Weight = Math.Max(keyword.Weight, weight);
            }
        }
    }
}
