using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UnitEffects.Settings;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class StatusEffectInfoHelper : BaseInfoHelper<StatusEffectSettings, StatusEffect>
    {
        public override long Key => EntityTypes.StatusEffect;

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
