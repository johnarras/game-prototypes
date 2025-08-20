
using Assets.Scripts.Assets.Textures;
using Assets.Scripts.ClientEvents;
using Assets.Scripts.Inventory.UI;
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.StateHelpers.Exploring;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Settings.Slots;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.Screens.Characters
{
    public class CrawlerCharacterScreen : CharacterScreen
    {
        protected ICrawlerStatService _crawlerStatService = null;
        protected IInfoService _infoService = null;
        protected IRoleService _roleService = null;
        protected ITextSerializer _serializer = null;
        protected ILootGenService _lootService = null;
        protected ITextService _textService = null;

        public AnimatedSprite Image;
        public GText NameText;
        public GText RaceText;
        public GText ClassLevelText;
        public GText SummonText;
        public GText TiersText;
        public GText InventoryCapacityText;

        public OtherIconTarget DropTarget;

        protected override bool CalcStatsOnEquipUnequip() { return false; }
        protected override string GetStatSubdirectory() { return "CrawlerParty"; }
        protected override bool ShowZeroStats() { return false; }

        protected PartyMember _partyMember;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            _dispatcher.AddListener<CrawlerCharacterScreenData>(OnScreenData, GetToken());

            IReadOnlyList<EquipSlot> equipSlots = _gameData.Get<EquipSlotSettings>(_gs.ch).GetData();

            foreach (EquipSlot equipSlot in equipSlots)
            {
                if (!equipSlot.IsCrawlerSlot)
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

            foreach (RoleScalingType scalingType in scalingTypes)
            {
                double tier = _roleService.GetRoleScalingLevel(party, _partyMember, scalingType.IdKey);

                sb.Append(_infoService.CreateInfoLink(scalingType) + ": " + tier + "   ");
            }

            _uiService.SetText(TiersText, sb.ToString());
        }

        protected override void OnStartClose()
        {
            _dispatcher.Dispatch(new HideInfoPanelEvent());
            base.OnStartClose();
        }

        protected override void TryEquip(Item origItem, long equipSlotId)
        {
            InventoryData inventoryData = _unit.Get<InventoryData>();

            List<Item> equipment = inventoryData.GetAllEquipment();
            if (_inventoryService.EquipItem(_unit, origItem.Id, equipSlotId, false))
            {
                _inventoryService.UnequipItem(_unit, origItem.Id, false);

                Item newItem = _serializer.MakeCopy(origItem);
                newItem.EquipSlotId = equipSlotId;
                OnEquip(new OnEquipItem() { Item = newItem, UnitId = _unit.Id });

                List<Item> removedItems = equipment.Except(inventoryData.GetAllEquipment()).ToList();

                foreach (Item item in removedItems)
                {
                    Items.InitIcon(item, _token);
                }

                CopyDataBack();
            }
        }

        protected override void ShowStats()
        {
            _crawlerStatService.CalcUnitStats(_crawlerService.GetParty(), _unit as CrawlerUnit, false);
            base.ShowStats();
        }

        protected override void TryUnequip(Item item)
        {
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
                if (party != null && Items != null && party.Inventory.Contains(dragItem.GetDataItem()))
                {
                    party.Inventory.Remove(dragItem.GetDataItem());
                    Items.RemoveIcon(dragItem.GetDataItem().Id);
                }
            }
        }
    }
}
