using OxDb.SharedCore.GameSettings;
using OxDb.SharedGame.UnitEffects.Settings;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using System.Text;

namespace OxDb.SharedGame.UnitEffects.Services
{
    public class StatusEffectService : IStatusEffectService
    {
        private IGameData _gameData = null;

        public string ShowStatusEffects(Unit unit, bool showAbbreviations)
        {
            StringBuilder sb = new StringBuilder();
            if (unit == null)
            {
                return "";
            }

            IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(unit).GetData();

            for (int i = 0; i < effects.Count; i++)
            {
                if (unit.StatusEffects.HasBitIndex(i))
                {
                    if (showAbbreviations)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(' ');
                        }
                        sb.Append(effects[i].Abbrev);
                    }
                    else
                    {

                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(effects[i].Name);
                    }
                }
            }

            return sb.ToString();
        }
    }
}


