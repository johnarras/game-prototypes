using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.Riddles.Constants;
using Genrpg.Shared.Riddles.Entities;
using Genrpg.Shared.Riddles.EntranceRiddleHelpers;
using Genrpg.Shared.Riddles.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Riddles.Services
{
    public interface IRiddleService : IInitializable
    {
        Task GenerateRiddles(PartyData partyData, List<CrawlerMap> floors, CrawlerMapGenType genType, IRandom rand);
        bool ShouldDrawProp(PartyData party, int x, int z);
        void SetPropPosition(object obj, object dat, CancellationToken token);

    }

    public class RiddleService : IRiddleService
    {
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private ICrawlerWorldService _worldService = null;
        private ICrawlerService _crawlerService = null;

        private SetupDictionaryContainer<long, IRiddleTypeHelper> _riddleTypeHelpers = new SetupDictionaryContainer<long, IRiddleTypeHelper>();

        private RiddleLookup _lookup = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private void InitWords()
        {
            if (_lookup != null)
            {
                return;
            }

            _lookup = new RiddleLookup();

            _lookup.LetterPositionWords = new Dictionary<int, Dictionary<char, List<string>>>();
            _lookup.WordsByLength = new Dictionary<int, List<string>>();

            IReadOnlyList<NameList> nameLists = _gameData.Get<NameSettings>(null).GetData();

            foreach (NameList nl in nameLists)
            {
                foreach (WeightedName word in nl.Names)
                {

                    string lowerword = word.Name.ToLower().Trim();

                    if (lowerword.Length >= RiddleConstants.MinWordLength && lowerword.Length <= RiddleConstants.MaxWordLength && !_lookup.AllWords.Contains(lowerword))
                    {
                        _lookup.AllWords.Add(lowerword);
                    }
                }
            }

            IReadOnlyList<ZoneType> zoneTypes = _gameData.Get<ZoneTypeSettings>(_gs.ch).GetData();

            List<String> allZoneTypeWords = new List<string>();
            foreach (ZoneType ztype in zoneTypes)
            {
                allZoneTypeWords.AddRange(ztype.CreatureNamePrefixes.Select(x => x.Name));
                allZoneTypeWords.AddRange(ztype.CreatureDoubleNamePrefixes.Select(x => x.Name));
                allZoneTypeWords.AddRange(ztype.ZoneAdjectives.Select(x => x.Name));
                allZoneTypeWords.AddRange(ztype.ZoneNames.Select(x => x.Name));
                allZoneTypeWords.AddRange(ztype.TreeTypes.Select(x => x.Name));
            }

            foreach (string ztypeWord in allZoneTypeWords)
            {
                if (ztypeWord == null)
                {
                    continue;
                }

                string normalizedName = ztypeWord.ToLower().Trim();

                if (normalizedName.Length >= RiddleConstants.MinWordLength && normalizedName.Length <= RiddleConstants.MaxWordLength &&
                    !_lookup.AllWords.Contains(normalizedName))
                {
                    _lookup.AllWords.Add(normalizedName);
                }

            }

            IReadOnlyList<ItemType> itemTypes = _gameData.Get<ItemTypeSettings>(_gs.ch).GetData();

            foreach (ItemType itype in itemTypes)
            {
                if (itype.EquipSlotId < 1)
                {
                    continue;
                }

                _lookup.ItemNames.Add(itype.Name);

                if (itype.Names == null)
                {
                    continue;
                }

                foreach (WeightedName word in itype.Names)
                {
                    string lowerword = word.Name.ToLower().Trim();

                    if (lowerword.Length >= RiddleConstants.MinWordLength && lowerword.Length <= RiddleConstants.MaxWordLength &&
                        !_lookup.AllWords.Contains(lowerword))
                    {
                        _lookup.AllWords.Add(lowerword);
                        if (!_lookup.ItemNames.Contains(word.Name))
                        {
                            _lookup.ItemNames.Add(word.Name);
                        }
                    }
                }
            }

            //////////////////////////////////////////////
            /// END OF IMPORT -- NOW CREATE DICTIONARIES
            /// /////////////////////////////////////////

            for (int i = 0; i <= RiddleConstants.MaxLetterPosition; i++)
            {
                _lookup.LetterPositionWords[i] = new Dictionary<char, List<string>>();
            }

            for (char c = 'a'; c <= 'z'; c++)
            {
                _lookup.WordsContainingLetters[c] = new List<string>();
                _lookup.WordsNotContainingLetters[c] = new List<string>();
            }

            foreach (string word in _lookup.AllWords)
            {
                if (word.Any(x => !char.IsLetterOrDigit(x)))
                {
                    continue;
                }


                int letterBits = 0;
                for (int c = 0; c < word.Length; c++)
                {
                    char lowerChar = char.ToLower(word[c]);
                    int letterOffset = lowerChar - 'a';
                    if (letterOffset >= 0 && letterOffset <= 26)
                    {
                        letterBits |= (1 << letterOffset);
                        if (!_lookup.WordsContainingLetters[lowerChar].Contains(word))
                        {
                            _lookup.WordsContainingLetters[lowerChar].Add(word);
                        }
                    }
                }

                for (int l = 0; l < 26; l++)
                {
                    if (!FlagUtils.IsSet(letterBits, (1 << l)))
                    {
                        _lookup.WordsNotContainingLetters[(char)('a' + l)].Add(word);
                    }
                }

                if (!_lookup.WordsByLength.ContainsKey(word.Length))
                {
                    _lookup.WordsByLength[word.Length] = new List<string>();
                }
                _lookup.WordsByLength[word.Length].Add(word);

                for (int i = 0; i < RiddleConstants.MaxLetterPosition - 1; i++)
                {
                    Dictionary<char, List<string>> posDict = _lookup.LetterPositionWords[i];

                    if (i < word.Length)
                    {

                        if (!posDict.ContainsKey(word[i]))
                        {
                            posDict[word[i]] = new List<string>();
                        }

                        posDict[word[i]].Add(word);
                    }
                }
            }
        }

        protected IRiddleTypeHelper GetHelper(long mapId)
        {
            CrawlerMap map = _worldService.GetMap(mapId);

            if (map == null || map.RiddleHints == null)
            {
                return null;
            }
            if (_riddleTypeHelpers.TryGetValue(map.RiddleHints.RiddleTypeId, out IRiddleTypeHelper helper))
            {
                return helper;
            }
            return null;
        }

        public bool ShouldDrawProp(PartyData party, int x, int z)
        {
            IRiddleTypeHelper helper = GetHelper(party.CurrPos.MapId);
            if (helper != null)
            {
                return helper.ShouldDrawProp(party, x, z);
            }
            return true;
        }

        public void SetPropPosition(object obj, object data, CancellationToken token)
        {
            PartyData party = _crawlerService.GetParty();
            IRiddleTypeHelper helper = GetHelper(party.CurrPos.MapId);
            if (helper != null)
            {
                helper.SetPropPosition(obj, data, token);
            }
        }


        public async Task GenerateRiddles(PartyData partyData, List<CrawlerMap> floors, CrawlerMapGenType genType, IRandom rand)
        {

            InitWords();
            long minFloor = Math.Max(2, floors.Min(x => x.MapFloor));
            long maxFloor = floors.Max(x => x.MapFloor);

            IReadOnlyList<RiddleType> riddleTypes = _gameData.Get<RiddleTypeSettings>(_gs.ch).GetData();

            if (floors.Any(x => x.Level <= partyData.GetUpgradePointsLevel(UpgradeReasons.CompleteDungeon, true)))
            {
                return;
            }

            CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

            for (long floorChosen = minFloor; floorChosen < maxFloor; floorChosen++)
            {
                if (rand.NextDouble() > mapSettings.RiddleUnlockChance)
                {
                    continue;
                }

                CrawlerMap lockedFloor = floors.FirstOrDefault(x => x.MapFloor == floorChosen);

                if (lockedFloor == null || (lockedFloor.EntranceRiddle != null && !string.IsNullOrEmpty(lockedFloor.EntranceRiddle.Text)))
                {
                    continue;
                }

                CrawlerMap prevFloor = floors.FirstOrDefault(x => x.MapFloor == floorChosen - 1);

                if (prevFloor == null || prevFloor.Details.Any(x => x.EntityTypeId == EntityTypes.Riddle))
                {
                    continue;
                }

                List<PointXZ> openPoints = new List<PointXZ>();

                for (int x = 0; x < prevFloor.Width; x++)
                {
                    for (int z = 0; z < prevFloor.Height; z++)
                    {
                        if (!prevFloor.IsValidEmptyCell(x, z))
                        {
                            continue;
                        }

                        openPoints.Add(new PointXZ(x, z));
                    }
                }

                if (openPoints.Count < 20)
                {
                    continue;
                }

                RiddleType riddleType = RandomUtils.GetRandomElement(riddleTypes, rand);

                if (_riddleTypeHelpers.TryGetValue(riddleType.IdKey, out IRiddleTypeHelper helper))
                {
                    await helper.AddRiddle(_lookup, lockedFloor, prevFloor, openPoints, rand);
                }
            }

            await Task.CompletedTask;
        }

    }
}