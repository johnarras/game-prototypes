using OxDb.SharedCore.Entities.Settings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Spells.Settings.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxDb.SharedGame.Crawler.Info.SpellEffectHelpers
{
    public abstract class BaseNumericSpellEffectHelper : BaseSpellEffectHelper
    {

        virtual protected string TierSuffix() { return $"{_infoService.CreateInfoLink(_gameData.Get<RoleScalingTypeSettings>(_gs.ch).Get(HelperKey))} per Tier"; }

        public override string ShowEffectInfo(CrawlerSpell spell, CrawlerSpellEffect effect)
        {
            StringBuilder sb = new StringBuilder();
            if (effect.WeaponDamageScale > 0)
            {
                sb.Append(" " + effect.WeaponDamageScale * 100 + "% Wpn ");
            }
            if (effect.StatBonusDamageScale > 0)
            {
                sb.Append(" " + effect.StatBonusDamageScale * 100 + "% Stat ");
            }
            ElementType elemType = _gameData.Get<ElementTypeSettings>(_gs.ch).Get(effect.ElementTypeId);

            if (elemType != null)
            {
                sb.Append(_infoService.CreateInfoLink(elemType) + " ");
            }

            EntityType etype = _gameData.Get<EntitySettings>(_gs.ch).Get(effect.EntityTypeId);
            if (etype != null)
            {
                sb.Append(etype.Name + " ");

                List<IIdName> children = _entityService.GetChildList(_gs.ch, etype.IdKey);

                IIdName child = children.FirstOrDefault(x => x.IdKey == effect.EntityId);

                if (child != null)
                {
                    sb.Append(_infoService.CreateInfoLink(child) + " ");
                }
            }
            sb.Append(GetRoleScalingText(spell, effect));
            return sb.ToString();
        }
    }
}


