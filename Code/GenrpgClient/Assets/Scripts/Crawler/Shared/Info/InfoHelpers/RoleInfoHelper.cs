using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Buffs.Settings;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Stats.Settings.Stats;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxDb.SharedGame.Crawler.Info.InfoHelpers
{
    public class RoleInfoHelper : BaseInfoHelper<RoleSettings, Role>
    {
        public override long HelperKey => EntityTypes.Role;

        public override List<string> GetInfoLines(long entityId)
        {

            Role role = _gameData.Get<RoleSettings>(_gs.ch).Get(entityId);

            List<string> allLines = new List<string>();

            allLines.Add(_infoService.CreateHeaderLine(role.Name, false));
            allLines.Add(role.Desc);

            StatSettings statSettings = _gameData.Get<StatSettings>(_gs.ch);


            StringBuilder statLine = new StringBuilder();
            if (role.HealthPerLevel > 0)
            {
                statLine.Append($"{role.HealthPerLevel} {_infoService.CreateInfoLink(statSettings.Get(StatTypes.Health))}/Lev");
            }

            if (role.ManaPerLevel > 0)
            {
                if (statLine.Length > 0)
                {
                    statLine.Append(", ");
                }
                statLine.Append($"{role.ManaPerLevel} {_infoService.CreateInfoLink(statSettings.Get(StatTypes.Mana))}/Lev");
            }

            if (role.CritPercent > 0)
            {
                if (statLine.Length > 0)
                {
                    statLine.Append(", ");
                }
                statLine.Append($"{role.CritPercent}% {_infoService.CreateInfoLink(statSettings.Get(StatTypes.Crit))}");
            }

            if (statLine.Length > 0)
            {
                allLines.Add(statLine.ToString());
            }

            List<RoleBonusAmount> statBonusAmounts = role.AmountBonuses.Where(x => x.EntityTypeId == EntityTypes.StatBonus)
                .OrderBy(x => x.EntityId).ToList();

            if (statBonusAmounts.Count > 0)
            {

                StringBuilder sb = new StringBuilder();

                sb.Append("Stats: ");
                for (int i = 0; i < statBonusAmounts.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(_infoService.CreateInfoLink(statSettings.Get(statBonusAmounts[i].EntityId)) + ": " +
                        (statBonusAmounts[i].Amount > 0 ? "+" : "") + statBonusAmounts[i].Amount);
                }

                allLines.Add(sb.ToString());
            }

            IReadOnlyList<RoleScalingType> scalingTypes = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).GetData();

            StringBuilder scalingBuilder = new StringBuilder();
            int roleScalingCount = 0;
            bool didShowScaling = false;
            foreach (RoleBonusAmount amount in role.AmountBonuses)
            {
                if (amount.EntityTypeId == EntityTypes.RoleScaling)
                {
                    RoleScalingType scalingType = scalingTypes.FirstOrDefault(x => x.IdKey == amount.EntityId);
                    if (scalingType != null)
                    {
                        if (!didShowScaling)
                        {
                            scalingBuilder.Append("T/Lev: ");
                        }
                        scalingBuilder.Append(_infoService.CreateInfoLink(scalingType) + ": " + amount.Amount);
                        didShowScaling = true;
                        roleScalingCount++;
                        if (roleScalingCount == 3 || roleScalingCount == 8)
                        {
                            allLines.Add(scalingBuilder.ToString());
                            scalingBuilder.Clear();
                        }
                        else
                        {
                            scalingBuilder.Append(' ');
                        }
                    }
                }
            }
            allLines.Add(scalingBuilder.ToString());

            //ShowBuffs(role, EntityTypes.Stat, allLines, "Stats", _gameData.Get<StatSettings>(null).GetData(), true);
            ShowBuffs(role, EntityTypes.PartyBuff, allLines, "Buffs", _gameData.Get<PartyBuffSettings>(null).GetData(), true);
            ShowBuffs(role, EntityTypes.CrawlerSpell, allLines, "Spells", _gameData.Get<CrawlerSpellSettings>(null).GetData(), true);


            return allLines;

        }
        protected virtual void ShowBuffs<T>(Role role, long entityTypeId, List<string> lines, string header, IReadOnlyList<T> gameDataList, bool inOneRow) where T : IIndexedGameItem
        {
            List<RoleBonusBinary> bonuses = role.BinaryBonuses.Where(x => x.EntityTypeId == entityTypeId).ToList();

            if (bonuses.Count < 1)
            {
                return;
            }

            List<T> dataItems = new List<T>();
            foreach (RoleBonusBinary bonus in bonuses)
            {
                T dataItem = gameDataList.FirstOrDefault(x => x.IdKey == bonus.EntityId);
                if (dataItem != null)
                {
                    dataItems.Add(dataItem);
                }
            }

            if (dataItems.Count < 1)
            {
                return;
            }


            if (typeof(IOrderedItem).IsAssignableFrom(typeof(T)))
            {
                List<IOrderedItem> orderedItems = dataItems.Cast<IOrderedItem>().ToList();

                orderedItems = orderedItems.OrderBy(x => x.GetOrder()).ToList();

                dataItems = orderedItems.Where(x => x.GetOrder() > 0).Cast<T>().ToList();
            }
            else
            {
                dataItems = dataItems.OrderBy(x => x.Name).ToList();
            }


            if (inOneRow)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(header + ": ");
                for (int d = 0; d < dataItems.Count; d++)
                {
                    sb.Append(_infoService.CreateInfoLink(dataItems[d]));

                    if (d < dataItems.Count - 1)
                    {
                        sb.Append(", ");
                    }
                    if (d == dataItems.Count - 1)
                    {
                        lines.Add(sb.ToString());
                        sb.Clear();
                        sb.Append(" ");
                    }
                }
            }
            else
            {
                lines.Add(header);

                foreach (T dataItem in dataItems)
                {
                    lines.Add(_infoService.CreateInfoLink(dataItem));
                }
            }
        }
    }
}


