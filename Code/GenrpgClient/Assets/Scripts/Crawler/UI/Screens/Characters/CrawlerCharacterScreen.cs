
using OxDb.Client.Assets.Textures;
using OxDb.Client.ClientEvents;
using OxDb.Client.Crawler.Shared.Crafting.Services;
using OxDb.Client.Crawler.UI.Screens.Characters.Upgrades;
using OxDb.Client.FloatingText.ClientEvents;
using OxDb.Client.Inventory.UI;
using OxDb.Client.UI.Constants;
using OxDb.Client.UI.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedGame.Crawler.Info.Services;
using OxDb.SharedGame.Crawler.Loot.Services;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.Roles.Constants;
using OxDb.SharedGame.Crawler.Roles.Services;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.States.StateHelpers.Exploring;
using OxDb.SharedGame.Crawler.Stats.Services;
using OxDb.SharedGame.Inventory.Messages;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Inventory.Settings.Slots;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Units.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.UI.Screens.Characters
{
    public class CrawlerCharacterScreen : CharacterScreen
    {
        protected ICrawlerStatService _crawlerStatService = null;
        protected IInfoService _infoService = null;
        protected IRoleService _roleService = null;
        protected ITextSerializer _serializer = null;
        protected ILootGenService _lootService = null;
        protected ITextService _textService = null;
        protected IPartyService _partyService = null;
        protected ICrawlerOptionsService _optionsService = null;
        protected ICrawlerCraftingService _craftingService = null;

        public AnimatedSprite Image;
        public GText NameText;
        public GText RaceText;
        public GText ClassLevelText;
        public GText SummonText;
        public GText TiersText;
        public GText InventoryCapacityText;

        public OtherIconTarget DropTarget;

        public MemberUpgradesUI Upgrades;

        protected override bool CalcStatsOnEquipUnequip() { return false; }
        protected override string GetStatSubdirectory() { return "CrawlerParty"; }
        protected override bool ShowZeroStats() { return false; }

        protected PartyMember _partyMember;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            _dispatcher.AddListener<CrawlerCharacterScreenData>(OnScreenData, GetToken());

            IReadOnlyList<EquipSlot> equipSlots = _gameData.Get<EquipSlotSettings>(_gs.ch).GetData();

            bool allSlotsOk = false;

            PartyData party = _crawlerService.GetParty();

            if (_optionsService.HasOption(party, CrawlerOptions.AllEquipmentSlots))
            {
                allSlotsOk = true;
            }


            foreach (EquipSlot equipSlot in equipSlots)
            {
                if (!allSlotsOk && !equipSlot.IsCrawlerSlot)
                {
                    EquipSlotIcon icon = EquipmentIcons.FirstOrDefault(x => x.EquipSlotId == equipSlot.IdKey);
                    if (icon != null)
                    {
                        _clientEntityService.SetActive(icon, false);
                    }
                }
            }

            _initialOpen = true;
            if (data is CrawlerCharacterScreenData csd)
            {
                OnScreenData(csd);
            }
            _initialOpen = false;
            await base.OnStartOpen(data, token);
        }

        private bool _initialOpen = false;
        private void OnScreenData(CrawlerCharacterScreenData csd)
        {
            if (_partyMember == csd.Unit)
            {
                return;
            }
            _unit = csd.Unit;
            _partyMember = csd.Unit;

            PartyData party = _crawlerService.GetParty();

            InventoryData idata = _partyMember.Get<InventoryData>();

            Upgrades.SetData(_partyMember);

            idata.SetInvenEquip(party.Inventory, _partyMember.Equipment);

            if (!_initialOpen)
            {
                SetEquipment();
            }
            Image.SetImage(_partyMember.PortraitName);
            _uiService.SetText(NameText, _unit.Name);

            List<Role> allRoles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(_unit.Roles);

            Role raceRole = allRoles.FirstOrDefault(x => x.RoleCategoryId == RoleCategories.Origin);

            if (raceRole != null)
            {
                _uiService.SetText(RaceText, "Race: " + _infoService.CreateInfoLink(raceRole));
            }


            List<Role> classRoles = allRoles.Where(x => x.RoleCategoryId == RoleCategories.Class).ToList();

            StringBuilder sb = new StringBuilder();
            sb.Append("Levels: ");
            foreach (Role classRole in classRoles)
            {
                UnitRole urole = _unit.Roles.FirstOrDefault(x => x.RoleId == classRole.IdKey);

                if (urole != null)
                {
                    sb.Append(" (" + _infoService.CreateInfoLink(classRole) + " " + urole.Level + ") ");
                }
            }

            _uiService.SetText(ClassLevelText, sb.ToString());

            sb.Clear();
            sb.Append("Summons: ");
            if (_partyMember.Summons.Count > 0)
            {
                foreach (PartySummon summon in _partyMember.Summons)
                {
                    sb.Append(_infoService.CreateInfoLink(_gameData.Get<UnitTypeSettings>(_gs.ch).Get(summon.UnitTypeId)) + " ");
                }
            }
            _uiService.SetText(SummonText, sb.ToString());


            IReadOnlyList<RoleScalingType> scalingTypes = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).GetData();

            sb.Clear();
            sb.Append("Tiers: ");


            int roleScalingsShown = 0;
            foreach (RoleScalingType scalingType in scalingTypes)
            {
                double tier = _roleService.GetRoleScalingLevel(party, _partyMember, scalingType.IdKey);


                sb.Append(_infoService.CreateInfoLink(scalingType) + ": " + tier + "   ");

                roleScalingsShown++;
                if (roleScalingsShown % 4 == 0)
                {
                    sb.Append("\n");
                }
            }

            _uiService.SetText(TiersText, sb.ToString());
        }

        protected override void OnStartClose()
        {
            _dispatcher.Dispatch(new HideInfoPanelEvent());
            base.OnStartClose();
        }

        protected override async ValueTask TryEquipAsync(Item origItem, long equipSlotId)
        {

            if (!CanManageInventoryNow())
            {
                return;
            }


            InventoryData inventoryData = _unit.Get<InventoryData>();

            List<Item> equipment = inventoryData.GetAllEquipment();
            if (await _inventoryService.EquipItem(_unit, origItem.Id, equipSlotId, false))
            {
                await _inventoryService.UnequipItem(_unit, origItem.Id, false);

                Item newItem = _serializer.MakeCopy(origItem);
                newItem.EquipSlotId = equipSlotId;
                OnEquip(new OnEquipItem() { Item = newItem, UnitId = _unit.Id });

                List<Item> removedItems = equipment.Except(inventoryData.GetAllEquipment()).ToList();

                foreach (Item item in removedItems)
                {
                    Items.InitIcon(item, GetToken());
                }

                CopyDataBack();
            }
        }

        protected override void ShowStats()
        {
            _partyService.UpdateItemBuffs(_crawlerService.GetParty());
            _crawlerStatService.CalcUnitStats(_crawlerService.GetParty(), _unit as CrawlerUnit, false);
            base.ShowStats();
        }


        private bool CanManageInventoryNow()
        {
            PartyData party = _crawlerService.GetParty();

            if (party == null)
            {
                return false;
            }

            if (party.Combat != null)
            {
                _dispatcher.Dispatch(new ShowFloatingText("You cannot manage items in combat!", EFloatingTextArt.Error));
                return false;
            }
            return true;

        }
        protected override void TryUnequip(Item item)
        {

            if (!CanManageInventoryNow())
            {
                return;
            }

            OnUnequip(new OnUnequipItem() { UnitId = _unit.Id, ItemId = item.Id });
            CopyDataBack();
        }

        private void CopyDataBack()
        {
            PartyData party = _crawlerService.GetParty();
            PartyMember member = _unit as PartyMember;

            InventoryData invenData = member.Get<InventoryData>();

            party.Inventory = invenData.GetAllInventory();
            member.Equipment = invenData.GetAllEquipment();

            ShowStats();
        }

        public override void OnUpdateChild(object childObject)
        {
            base.OnUpdateChild(childObject);

            PartyData party = _crawlerService.GetParty();
            long inventorySize = _lootService.GetPartyInventorySize(party);
            int inventoryCount = party.Inventory.Count;

            string color = (inventoryCount < inventorySize - 5 ? TextColors.ColorWhite :
                inventoryCount <= inventorySize ? TextColors.ColorYellow :
                TextColors.ColorRed);

            _uiService.SetText(InventoryCapacityText,
                _textService.HighlightText("Inventory: " + inventoryCount + "/" + inventorySize, color));
        }

        protected override void HandleOtherTarget(ItemIconScreen startSc, ItemIcon dragItem, OtherIconTarget otherTarget, GameObject finalObjectHit)
        {
            if (otherTarget == DropTarget)
            {
                PartyData party = _crawlerService.GetParty();

                if (!CanManageInventoryNow())
                {
                    return;
                }

                if (party != null && Items != null && party.Inventory.Contains(dragItem.GetDataItem()))
                {
                    if (_craftingService.ScrapItem(party, dragItem.GetDataItem(), DropTarget.transform.position))
                    {
                        Items.RemoveIcon(dragItem.GetDataItem().Id);
                    }
                }
            }
        }

        protected override long GetStatModifier(long statTypeId)
        {
            if (statTypeId < StatConstants.PrimaryStatStart || statTypeId > StatConstants.PrimaryStatEnd)
            {
                return 0;
            }

            return _crawlerStatService.GetStatBonus(_crawlerService.GetParty(), _partyMember, statTypeId);
        }
    }
}


