using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.UnitEffects.Settings;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Info.InfoHelpers
{
    public class StatusEffectInfoHelper : BaseInfoHelper<StatusEffectSettings, StatusEffect>
    {
        public override long HelperKey => EntityTypes.StatusEffect;

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = new List<string>();

            StatusEffect child = _gameData.Get<StatusEffectSettings>(_gs.ch).Get(entityId);

            if (child != null)
            {
                lines.Add(_infoService.CreateHeaderLine(child.Name, false));
                lines.Add(" ");
                if (child is IIndexedGameItem indexedItem && !string.IsNullOrEmpty(indexedItem.Desc))
                {
                    lines.Add(indexedItem.Desc.Replace("XXXX", child.Amount.ToString()));
                }
            }

            return lines;
        }
    }
}


