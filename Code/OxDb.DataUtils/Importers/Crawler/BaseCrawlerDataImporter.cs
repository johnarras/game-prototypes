using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.Combat.Constants;
using OxDb.SharedGame.Crawler.Roles.Constants;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.Spells.Constants;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Spells.Constants;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Stats.Settings.Stats;
using OxDb.SharedGame.Units.Settings;

namespace OxDb.DataUtils.Importers.Crawler
{
    public abstract class BaseCrawlerDataImporter<TParent> : BaseParentDataImporter<TParent> where TParent : class, ITopLevelSettings, new()
    {
        protected override async Task<bool> UpdateAfterImport(EditorGameState gs)
        {
            RoleSettings roleSettings = gs.data.Get<RoleSettings>(null);

            List<Role> roles = roleSettings.GetData().ToList();

            CrawlerSpellSettings spellSettings = gs.data.Get<CrawlerSpellSettings>(null);

            List<CrawlerSpell> newSpells = spellSettings.GetData().ToList();

            List<long> buffStatIds =
                 roles.SelectMany(x => x.BinaryBonuses)
                 .Where(y => y.EntityTypeId == EntityTypes.Stat && (y.EntityId < StatConstants.PrimaryStatStart || y.EntityId > StatConstants.PrimaryStatEnd))
                 .Select(x => x.EntityId)
                 .Distinct().ToList();

            IReadOnlyList<StatType> stats = gs.data.Get<StatSettings>(null).GetData();

            List<long> statIds = stats.Select(x => x.IdKey).ToList();

            buffStatIds = buffStatIds.Where(x => stats.Select(x => x.IdKey).Contains(x)).ToList();


            newSpells = newSpells
                .Where(x => x.IdKey < CrawlerSpellConstants.StatBuffSpellIdOffset
            || x.IdKey >= CrawlerSpellConstants.StatBuffSpellIdOffset + StatConstants.MaxStatType)
                .ToList();

            foreach (long buffStatId in buffStatIds)
            {
                CrawlerSpell spell = new CrawlerSpell()
                {
                    IdKey = buffStatId + CrawlerSpellConstants.StatBuffSpellIdOffset,
                    Name = "Enhance " + stats.First(x => x.IdKey == buffStatId).Name,
                    BaseCost = spellSettings.StatBuffPowerCost,
                    TierCost = spellSettings.StatBuffPowerPerLevel,
                    MinRange = 0,
                    MaxRange = 100,
                    TargetTypeId = TargetTypes.Self,
                    UnlockTier = 1,
                    CombatActionId = CombatActions.Cast,
                    RoleScalingTypeId = RoleScalingTypes.Healing,
                };

                newSpells.Add(spell);

                spell.Effects.Add(new CrawlerSpellEffect()
                {
                    EntityTypeId = EntityTypes.Stat,
                    EntityId = buffStatId,
                    WeaponDamageScale = 1,
                    StatBonusDamageScale = 1,
                });

                gs.LookedAtObjects.Add(spell);
            }


            // For each role, remove crawler buff spells that no longer apply and add crawler buff spells that need to be there.
            foreach (Role role in roles)
            {
                role.BinaryBonuses = role.BinaryBonuses
                    .Where(x => x.EntityTypeId != EntityTypes.Stat
                    || x.EntityId < CrawlerSpellConstants.StatBuffSpellIdOffset
                    || x.EntityId > CrawlerSpellConstants.StatBuffSpellIdOffset + StatConstants.MaxStatType)
                    .ToList();

                List<RoleBonusBinary> bonusesToAdd = new List<RoleBonusBinary>();
                foreach (RoleBonusBinary bonus in role.BinaryBonuses)
                {
                    if (bonus.EntityTypeId == EntityTypes.Stat)
                    {
                        RoleBonusBinary spellBonus = role.BinaryBonuses
                            .Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell
                        && x.EntityId == bonus.EntityId + CrawlerSpellConstants.StatBuffSpellIdOffset)
                            .FirstOrDefault();
                        if (spellBonus == null)
                        {
                            bonusesToAdd.Add(new RoleBonusBinary()
                            {
                                EntityTypeId = EntityTypes.CrawlerSpell,
                                EntityId = bonus.EntityId + CrawlerSpellConstants.StatBuffSpellIdOffset
                            });
                        }
                    }
                }
                if (bonusesToAdd.Count > 0)
                {
                    role.BinaryBonuses.AddRange(bonusesToAdd);
                    gs.LookedAtObjects.Add(role);
                }
            }
            foreach (CrawlerSpell spell in newSpells)
            {
                spell.Roles = new SmallIndexBitList();

                List<Role> rolesKnowingThis = roles.Where(x => x.BinaryBonuses.Any(y => y.EntityTypeId == EntityTypes.CrawlerSpell && y.EntityId == spell.IdKey)).ToList();

                foreach (Role role in rolesKnowingThis)
                {
                    spell.Roles.SetBitIndex(role.IdKey);
                }
            }


            UnitTypeSettings unitTypeSettings = gs.data.Get<UnitTypeSettings>(null);

            List<UnitType> unitTypes = unitTypeSettings.GetData().ToList();


            IReadOnlyList<UnitKeyword> keywordList = gs.data.Get<UnitKeywordSettings>(null).GetData();

            foreach (UnitType utype in unitTypes)
            {

                List<Effect> allEffects = new List<Effect>();

                TribeType ttype = gs.data.Get<TribeSettings>(null).Get(utype.TribeTypeId);

                bool shouldSaveUnitType = false;

                if (ttype != null)
                {
                    UnitKeyword tribeKeyword = keywordList.FirstOrDefault(x => StrUtils.IsLowercaseEqual(x.Name, ttype.Name));

                    if (tribeKeyword != null)
                    {
                        allEffects.AddRange(tribeKeyword.Effects.Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell &&
                        x.EntityId >= CrawlerSpellConstants.MinPlaceholderSpellId && x.EntityId <= CrawlerSpellConstants.MaxPlaceholderSpellId));
                    }
                }

                string[] nameWords = utype.Name.Split(' ');


                foreach (string nword in nameWords)
                {
                    UnitKeyword nameKeyword = keywordList.FirstOrDefault(x => StrUtils.IsLowercaseEqual(x.Name, nword));

                    if (nameKeyword != null)
                    {
                        allEffects.AddRange(nameKeyword.Effects.Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell &&
                        x.EntityId >= CrawlerSpellConstants.MinPlaceholderSpellId && x.EntityId <= CrawlerSpellConstants.MaxPlaceholderSpellId));
                    }
                }

                foreach (Effect effect in allEffects)
                {
                    if (effect.EntityId == CrawlerSpellConstants.SelfSummonPlaceholderSpellId)
                    {
                        long spellId = effect.EntityId + CrawlerSpellConstants.MonsterSummonSpellIdOffset;

                        Effect currEffect = utype.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.CrawlerSpell && x.EntityId >= spellId);

                        if (currEffect == null)
                        {
                            utype.Effects.Add(new Effect() { EntityTypeId = EntityTypes.CrawlerSpell, EntityId = spellId, Quantity = 1 });
                            shouldSaveUnitType = true;
                        }
                    }
                    else if (effect.EntityId == CrawlerSpellConstants.BaseSummonPlaceholderSpellId)
                    {
                        int index = unitTypes.IndexOf(utype);

                        if (index > 0)
                        {
                            for (int idx = index - 1; idx > 0; idx--)
                            {
                                UnitType prevUnitType = unitTypes[idx];

                                if (utype.Name.Contains(prevUnitType.Name))
                                {
                                    long spellId = prevUnitType.IdKey + CrawlerSpellConstants.MonsterSummonSpellIdOffset;
                                    Effect currEffect = utype.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.CrawlerSpell && x.EntityId >= spellId);

                                    if (currEffect == null)
                                    {
                                        utype.Effects.Add(new Effect() { EntityTypeId = EntityTypes.CrawlerSpell, EntityId = spellId, Quantity = 1 });

                                        shouldSaveUnitType = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                if (shouldSaveUnitType && !gs.LookedAtObjects.Contains(utype))
                {
                    gs.LookedAtObjects.Add(utype);
                }
            }

            foreach (UnitType utype in unitTypes)
            {
                long elementTypeId = ElementTypes.Arcane;

                CrawlerSpell currSpell = newSpells.FirstOrDefault(x => x.IdKey == utype.IdKey + CrawlerSpellConstants.MonsterSummonSpellIdOffset);

                if (currSpell != null)
                {
                    newSpells.Remove(currSpell);
                }

                Effect effect = utype.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Resist);

                if (effect != null && effect.EntityId > 0)
                {
                    elementTypeId = effect.EntityId;
                }

                CrawlerSpell newSpell = new CrawlerSpell()
                {
                    IdKey = utype.IdKey + CrawlerSpellConstants.MonsterSummonSpellIdOffset,
                    Name = "Monster Call " + utype.Name,
                    BaseCost = 100,
                    TierCost = 1,
                    MinRange = 0,
                    MaxRange = 100,
                    TargetTypeId = TargetTypes.Self,
                    UnlockTier = 1,
                    CombatActionId = CombatActions.Cast,
                    RoleScalingTypeId = RoleScalingTypes.Summon,
                };

                newSpell.Effects.Add(new CrawlerSpellEffect()
                {
                    EntityTypeId = EntityTypes.Unit,
                    EntityId = utype.IdKey,
                    WeaponDamageScale = 1,
                    StatBonusDamageScale = 1,
                    ElementTypeId = elementTypeId,
                });

                gs.LookedAtObjects.Add(newSpell);
                newSpells.Add(newSpell);
            }

            spellSettings.SetData(newSpells);
            gs.LookedAtObjects.Add(spellSettings);
            gs.LookedAtObjects.Add(roleSettings);
            gs.LookedAtObjects.Add(unitTypeSettings);

            await _importService.CleanOldObjects(newSpells);
            await _importService.CleanOldObjects(unitTypes);
            await _importService.CleanOldObjects(roles);

            await Task.CompletedTask;
            return true;
        }


    }
}


