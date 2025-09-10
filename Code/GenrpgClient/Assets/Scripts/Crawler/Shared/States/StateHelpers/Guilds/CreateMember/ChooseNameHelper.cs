using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds.CreateMember
{
    public class ChooseNameHelper : BaseStateHelper
    {
        private ILootGenService _lootGenService = null;
        public override ECrawlerStates Key => ECrawlerStates.ChooseName;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            PartyData party = _crawlerService.GetParty();

            stateData.WorldSpriteName = member.PortraitName;

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.ChoosePortrait,
                extraData: member));

            stateData.AddInputField("Name: ", delegate (string text)
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                StringBuilder sb = new StringBuilder();

                for (int t = 0; t < text.Length; t++)
                {
                    if ((char)text[t] >= 32 && (char)text[t] <= 127)
                    {
                        sb.Append(text[t]);
                    }
                }
                text = sb.ToString();

                if (!string.IsNullOrEmpty(text))
                {
                    member.Name = text;
                    party.Members.Add(member);


                    IReadOnlyList<ItemType> itemTypes = _gameData.Get<ItemTypeSettings>(_gs.ch).GetData();

                    List<Role> roles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles);

                    List<RoleBonusBinary> weaponBonuses = new List<RoleBonusBinary>();

                    foreach (Role role in roles)
                    {
                        weaponBonuses.AddRange(role.BinaryBonuses.Where(x => x.EntityTypeId == EntityTypes.Item));
                    }

                    List<long> okWeaponTypes = weaponBonuses.Where(x => x.EntityTypeId == EntityTypes.Item).Select(x => x.EntityId).ToList();

                    List<ItemType> okMelee = new List<ItemType>();
                    List<ItemType> okRanged = itemTypes.Where(x => x.EquipSlotId == EquipSlots.Ranged).ToList();

                    foreach (long wt in okWeaponTypes)
                    {
                        ItemType itype = itemTypes.FirstOrDefault(x => x.IdKey == wt);
                        if (itype != null)
                        {
                            if (itype.EquipSlotId == EquipSlots.MainHand)
                            {
                                okMelee.Add(itype);
                            }
                            else if (itype.EquipSlotId == EquipSlots.Ranged)
                            {
                                okRanged.Add(itype);
                            }
                        }
                    }

                    if (okMelee.Count > 0)
                    {
                        okMelee = okMelee.OrderBy(x => (x.MinVal + x.MaxVal)).ToList();

                        ItemGenArgs igd = new ItemGenArgs()
                        {
                            Level = 0,
                            ItemTypeId = okMelee[0].IdKey,
                        };
                        Item newItem = _lootGenService.GenerateItem(igd);
                        if (newItem != null)
                        {
                            member.Equipment.Add(newItem);
                            newItem.EquipSlotId = EquipSlots.MainHand;
                        }
                    }
                    if (okRanged.Count > 0)
                    {
                        okRanged = okRanged.OrderBy(x => (x.MinVal + x.MaxVal)).ToList();

                        ItemGenArgs igd = new ItemGenArgs()
                        {
                            Level = 0,
                            ItemTypeId = okRanged[0].IdKey,
                        };
                        Item newItem = _lootGenService.GenerateItem(igd);
                        if (newItem != null)
                        {
                            member.Equipment.Add(newItem);
                            newItem.EquipSlotId = EquipSlots.Ranged;
                        }
                    }

                    _crawlerSpellService.SetupCombatData(party, member);

                    _statService.CalcUnitStats(party, member, true);


                    List<CrawlerSpell> spells = _crawlerSpellService.GetSpellsForMember(party, member);

                    List<CrawlerSpell> summonSpells = spells.Where(x => x.Effects.Any(y => y.EntityTypeId == EntityTypes.Unit) &&
                    x.RoleScalingTier == 1).ToList();


                    if (summonSpells.Count > 0)
                    {
                        CrawlerSpell summonSpell = summonSpells[_rand.Next() % summonSpells.Count];

                        _crawlerSpellService.CastSpell(party, new Spells.Entities.UnitAction() { Caster = member, Spell = summonSpell, FinalTargets = new List<CrawlerUnit>() { member } }, token);
                        _statService.CalcUnitStats(party, member, true);
                    }


                    _crawlerService.SaveGame();
                    _crawlerService.ChangeState(ECrawlerStates.GuildMain, token);
                }
            });


            await Task.CompletedTask;
            return stateData;
        }
    }
}
