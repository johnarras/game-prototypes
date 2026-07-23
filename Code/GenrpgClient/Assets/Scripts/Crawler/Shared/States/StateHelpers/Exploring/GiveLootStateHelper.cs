using OxDb.Client.Audio.ClientEvents;
using OxDb.Client.Audio.Constants;
using OxDb.Client.Crawler.Constants;
using OxDb.Client.UI.Constants;
using OxDb.SharedGame.Crawler.Constants;
using OxDb.SharedGame.Crawler.Loot.Services;
using OxDb.SharedGame.Crawler.Loot.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Currencies.Settings;
using OxDb.SharedGame.Inventory.PlayerData;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Exploring
{

    public class GiveLootStateHelper : BaseStateHelper
    {

        private ILootGenService _lootService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.GiveLoot;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(_gs.ch);

            stateData.BGImageOnly = true;
            stateData.BGSpriteName = CrawlerClientConstants.TreasureImage;

            PartyLoot loot = null;

            PartyData party = _crawlerService.GetParty();

            LootGenData genData = action.ExtraData as LootGenData;

            if (genData == null)
            {
                return new CrawlerStateData(ECrawlerStates.ExploreWorld, true);
            }

            loot = await _lootService.GiveLoot(party, _worldService.GetMap(party.CurrPos.MapId), genData, token);

            foreach (string topMessage in loot.TopMessages)
            {
                stateData.AddText(topMessage);
            }

            stateData.AddText("Your party receives: ");

            if (loot.Exp > 0)
            {
                stateData.AddText(loot.Exp + " Exp per party member!");
            }

            IReadOnlyList<CoreCurrencyType> ctypes = _gameData.Get<CoreCurrencyTypeSettings>(_gs.ch).GetData();

            foreach (CoreCurrencyType ctype in ctypes)
            {
                if (loot.Currencies[ctype.IdKey] > 0)
                {
                    stateData.AddText(loot.Currencies[CoreCurrencyTypes.Coins] + " " + ctype.Name + "!");
                }
            }

            if (loot.Items.Count > 0)
            {
                foreach (Item item in loot.Items)
                {
                    stateData.AddText(item.Name + "!");
                }

                string textColor = (party.Inventory.Count >= loot.TotalInventorySize ? TextColors.ColorRed : TextColors.ColorYellow);

                stateData.AddText(
                    _textService.HighlightText(
                    $"Inventory {party.Inventory.Count}/{loot.TotalInventorySize}",
                    textColor));

            }

            if (loot.NewQuestItems.Count > 0)
            {
                CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

                foreach (long questItemId in loot.NewQuestItems)
                {
                    WorldQuestItem questItem = world.QuestItems.FirstOrDefault(x => x.IdKey == questItemId);
                    if (questItem != null)
                    {
                        stateData.AddText("************ QUEST ITEM: ************\n " +
                        $"{_textService.HighlightText(questItem.Name, TextColors.ColorWhite)}!\n");
                    }
                }
            }

            if (loot.ExtraMessages.Count > 0)
            {
                foreach (string message in loot.ExtraMessages)
                {
                    stateData.Actions.Add(new CrawlerStateAction(_textService.HighlightText(message, TextColors.ColorWhite)));
                }
            }

            _dispatcher.Dispatch(new PlaySound(CrawlerAudio.Treasure, AudioConstants.NoVariance));

            if (loot.NextState == ECrawlerStates.None)
            {
                loot.NextState = ECrawlerStates.ExploreWorld;
            }
            AddSpaceAction(stateData, loot.NextState, loot.NextStateData);
            await _crawlerService.SaveGame();

            return stateData;
        }
    }
}


