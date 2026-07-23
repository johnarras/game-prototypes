using OxDb.Client.Crawler.Maps.Loading;
using OxDb.Client.Crawler.Maps.Services;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Maps.Settings;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Inventory.Settings.ItemTypes;
using OxDb.SharedGame.Names.Settings;
using OxDb.SharedGame.Riddles.Constants;
using OxDb.SharedGame.Riddles.Entities;
using OxDb.SharedGame.Riddles.EntranceRiddleHelpers;
using OxDb.SharedGame.Riddles.Settings;
using OxDb.SharedGame.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Riddles.Services
{
    public interface IRiddleService : IInitializable
    {
        Task GenerateRiddles(PartyData party, List<CrawlerMap> floors, CrawlerMapGenType genType, IRandom rand);
        bool ShouldDrawProp(PartyData party, int x, int z);
        void SetPropPosition(object obj, CrawlerObjectLoadData data, CancellationToken token);

    }

    public class RiddleService : IRiddleService
    {
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private ICrawlerWorldService _worldService = null;
        private ICrawlerService _crawlerService = null;
        private ICrawlerOptionsService _optionService = null;

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

                foreach (WeightedName word in itype.GetNames())
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
                    if (!FlagUtils.MatchesAnyBits(letterBits, (1 << l)))
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

        public void SetPropPosition(object obj, CrawlerObjectLoadData data, CancellationToken token)
        {
            PartyData party = _crawlerService.GetParty();
            IRiddleTypeHelper helper = GetHelper(party.CurrPos.MapId);
            if (helper != null)
            {
                helper.SetPropPosition(obj, data, token);
            }
        }


        public async Task GenerateRiddles(PartyData party, List<CrawlerMap> floors, CrawlerMapGenType genType, IRandom rand)
        {
            if (!_optionService.HasOption(party, CrawlerOptions.Puzzles))
            {
                return;
            }

            InitWords();
            long minFloor = Math.Max(2, floors.Min(x => x.MapFloor));
            long maxFloor = floors.Max(x => x.MapFloor);

            IReadOnlyList<RiddleType> riddleTypes = _gameData.Get<RiddleTypeSettings>(_gs.ch).GetData();

            if (floors.FastAny(x => x.Level <= party.GetUpgradePointsLevel(UpgradeReasons.CompleteDungeon, true)))
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

                if (prevFloor == null || prevFloor.Details.FastAny(x => x.EntityTypeId == EntityTypes.Riddle))
                {
                    continue;
                }

                List<Point2I> openPoints = new List<Point2I>();

                for (int x = 0; x < prevFloor.Width; x++)
                {
                    for (int z = 0; z < prevFloor.Height; z++)
                    {
                        if (!prevFloor.IsValidEmptyCell(x, z))
                        {
                            continue;
                        }

                        openPoints.Add(new Point2I(x, z));
                    }
                }

                if (openPoints.Count < 20)
                {
                    continue;
                }

                RiddleType riddleType = RandUtils.GetRandomElement(riddleTypes, rand);

                if (_riddleTypeHelpers.TryGetValue(riddleType.IdKey, out IRiddleTypeHelper helper))
                {
                    await helper.AddRiddle(_lookup, lockedFloor, prevFloor, openPoints, rand);
                }
            }

            await Task.CompletedTask;
        }

    }
}

