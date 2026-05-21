using OxDb.DataUtils.Entities.Core;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Inventory.Settings.ItemTypes;
using OxDb.SharedGame.Stats.Settings.Stats;
using System.Reflection;

namespace OxDb.DataUtils.Importers.Crawler
{
    public class RoleSettingsImporter : BaseCrawlerDataImporter<RoleSettings>
    {
        const int MaxRoles = 100;

        protected override async Task<bool> ParseInputFromLines(EditorGameState gs, List<string[]> lines)
        {
            string[] firstLine = lines[0];
            string[] secondLine = lines[1];

            string missingWords = "";

            RoleSettings roleSettings = gs.data.Get<RoleSettings>(null);
            gs.LookedAtObjects.Add(roleSettings);

            Role[] topRow = new Role[MaxRoles];

            List<Role> newRoles = new List<Role>();

            for (int s = 1; s < firstLine.Length; s++)
            {

                string roleName = firstLine[s].Trim();
                if (string.IsNullOrEmpty(roleName))
                {
                    continue;
                }
                if (roleName.Length < 3)
                {
                    missingWords += "BadRoleName:(" + roleName + ")";
                    break;
                }

                if (long.TryParse(secondLine[s], out long newRoleId))
                {
                    if (newRoleId < 1)
                    {
                        missingWords += " No Role Id Row 2 for Column " + s + " ";
                        break;
                    }

                    if (newRoles.Any(x => x.IdKey == newRoleId))
                    {
                        missingWords += " Duplicate Role Idkey: " + newRoleId + " ";
                        break;
                    }


                    Role role = new Role() { IdKey = newRoleId, Name = roleName };
                    newRoles.Add(role);
                    topRow[s] = role;
                }
                else
                {
                    missingWords += " Role Idkey column " + s + " is not a number ";
                    break;
                }
                gs.LookedAtObjects.Add(topRow[s]);
                topRow[s].BinaryBonuses = new List<RoleBonusBinary>();
                topRow[s].AmountBonuses = new List<RoleBonusAmount>();

            }

            PropertyInfo[] props = typeof(Role).GetProperties();

            for (int line = 2; line < lines.Count; line++)
            {
                string[] words = lines[line];

                if (words.Length < 2 || string.IsNullOrEmpty(words[0].Trim()))
                {
                    continue;
                }

                if (TryAddAmountBonus<StatSettings>(gs.data, EntityTypes.StatBonus, words, topRow, "Bonus"))
                {
                    continue;
                }

                if (TryAddAmountBonus<RoleScalingTypeSettings>(gs.data, EntityTypes.RoleScaling, words, topRow, "Scaling"))
                {
                    continue;
                }

                if (TryAddBinaryBonus<StatSettings>(gs.data, EntityTypes.Stat, words, topRow))
                {
                    continue;
                }
                if (TryAddBinaryBonus<CrawlerSpellSettings>(gs.data, EntityTypes.CrawlerSpell, words, topRow))
                {
                    continue;
                }
                if (TryAddBinaryBonus<ItemTypeSettings>(gs.data, EntityTypes.Item, words, topRow))
                {
                    continue;
                }



                PropertyInfo prop = props.FirstOrDefault(x => x.Name == words[0]);

                if (prop != null)
                {
                    for (int w = 0; w < words.Length && w < MaxRoles; w++)
                    {
                        if (topRow[w] != null && !string.IsNullOrEmpty(words[w]))
                        {
                            if (int.TryParse(words[w], out int ival))
                            {
                                _reflectionService.SetObjectValue(topRow[w], prop, ival);
                            }
                            else if (double.TryParse(words[w], out double dval))
                            {
                                _reflectionService.SetObjectValue(topRow[w], prop, dval);
                            }
                            else if (prop.PropertyType == typeof(string))
                            {
                                _reflectionService.SetObjectValue(topRow[w], prop, words[w]);
                            }
                            else if (prop.PropertyType == typeof(bool))
                            {
                                if (bool.TryParse(words[w], out bool bval))
                                {
                                    _reflectionService.SetObjectValue(topRow[w], prop, bval);
                                }
                            }
                        }
                    }
                    continue;
                }

                if (words[0].ToLower() != "count")
                {
                    missingWords += words[0] + " -- ";
                }
            }

            if (!string.IsNullOrWhiteSpace(missingWords))
            {
                return false;
            }

            for (int c = 0; c < topRow.Length; c++)
            {
                if (topRow[c] != null)
                {
                    topRow[c].BinaryBonuses = topRow[c].BinaryBonuses.OrderBy(x => x.EntityTypeId).ThenBy(x => x.EntityId).ToList();
                }
            }

            roleSettings.SetData(newRoles);

            await Task.CompletedTask;
            return true;
        }

        private bool TryAddAmountBonus<T>(IGameData gameData, long entityTypeId, string[] words, Role[] topRow, string removeSuffix = "") where T : ITopLevelSettings
        {
            if (!string.IsNullOrEmpty(removeSuffix) && !words[0].Contains(removeSuffix))
            {
                return false;
            }
            string normalizedWord = StrUtils.NormalizeWord(words[0].Replace(removeSuffix, ""));

            List<IIdName> children = gameData.Get<T>(null).GetChildren().Cast<IIdName>().ToList();

            IIdName child = children.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == normalizedWord);

            if (child != null)
            {
                for (int w = 1; w < words.Length && w < MaxRoles; w++)
                {
                    if (topRow[w] != null && double.TryParse(words[w], out double amount))
                    {
                        if (amount != 0)
                        {
                            topRow[w].AmountBonuses.Add(new RoleBonusAmount() { EntityTypeId = entityTypeId, EntityId = child.IdKey, Amount = amount });
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private bool TryAddBinaryBonus<T>(IGameData gameData, long entityTypeId, string[] words, Role[] topRow) where T : ITopLevelSettings
        {

            string normalizedWord = StrUtils.NormalizeWord(words[0]);
            List<IIdName> children = gameData.Get<T>(null).GetChildren().Cast<IIdName>().ToList();

            IIdName child = children.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == normalizedWord);

            if (child != null)
            {
                for (int w = 1; w < words.Length && w < MaxRoles; w++)
                {
                    if (topRow[w] != null && !string.IsNullOrEmpty(words[w]))
                    {
                        topRow[w].BinaryBonuses.Add(new RoleBonusBinary() { EntityTypeId = entityTypeId, EntityId = child.IdKey });
                    }
                }
                return true;
            }
            return false;
        }
    }
}


