using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Casting;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Spells.PlayerData.Spells;
using OxDb.SharedGame.Spells.Settings.Effects;
using OxDb.SharedGame.Units.Entities;

namespace OxDb.MapServer.Spells.Services
{
    public interface IServerSpellService : IInitializable
    {
        TryCastResult TryCast(IRandom rand, Unit unit, long spellId, string targetId, bool endOfCast);

        bool FullTryStartCast(IRandom rand, Unit unit, long spellId, string targetId);


        TargetCastState GetTargetState(IRandom rand, Spell spell, string targetId);

        void SendStopCast(IRandom rand, MapObject obj);

        void SendSpell(IRandom rand, Unit caster, TryCastResult result);

        void ResendSpell(IRandom rand, Unit caster, Unit target, SendSpell sendSpell);

        void OnSendSpell(IRandom rand, Unit origTarget, SendSpell sendSpell);

        void OnSpellHit(IRandom rand, SpellHit hit);

        void ShowFX(IRandom rand, string fromUnitId, string toUnitId, long elementTypeId, string fxName, float duration);

        void ShowProjectile(IRandom rand, Unit caster, Unit target, Spell spell, string fxName, float speed);

        void ShowCombatText(Unit unit, string txt, int combatTextColorId, bool isCrit = false);

        void ApplyOneEffect(IRandom rand, ActiveSpellEffect eff);
    }
}


