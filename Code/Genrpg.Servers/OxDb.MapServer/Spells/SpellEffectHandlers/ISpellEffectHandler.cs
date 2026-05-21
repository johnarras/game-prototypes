using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Spells.Settings.Effects;
using System.Collections.Generic;

namespace OxDb.MapServer.Spells.SpellEffectHandlers
{
    /// <summary>
    /// Use this to convery the effect a spell has into a SpellEffect on the SpellHitData
    /// </summary>
    public interface ISpellEffectHandler : ISetupDictionaryItem<long>
    {
        bool IsModifyStatEffect();
        bool UseStatScaling();
        float GetTickLength();

        List<ActiveSpellEffect> CreateEffects(IRandom rand, SpellHit spellHit);

        bool HandleEffect(IRandom rand, ActiveSpellEffect eff);

    }
}


