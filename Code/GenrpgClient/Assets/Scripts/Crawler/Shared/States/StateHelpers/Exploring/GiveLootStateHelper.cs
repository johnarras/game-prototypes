using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Interfaces;
using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Loot.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{

    public class GiveLootStateHelper : BaseStateHelper
    {

        private ILootGenService _lootService = null;
        private IAudioService _audioService;

        public override ECrawlerStates Key => ECrawlerStates.GiveLoot;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
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
            if (loot.Gold > 0)
            {
                stateData.AddText(loot.Gold + " Gold!");
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

            _audioService.PlaySound(CrawlerAudio.Treasure, null);

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
