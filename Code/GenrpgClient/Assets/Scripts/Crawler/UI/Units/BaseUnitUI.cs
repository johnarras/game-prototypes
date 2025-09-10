using Assets.Scripts.Crawler.Combat;
using Assets.Scripts.Crawler.UI.StatusUI;
using Assets.Scripts.UI.CombatTexts;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.Units
{
    public class BaseUnitUI : BaseBehaviour
    {
        public FastCombatTextUI FastCombatTextUI;
        public CombatEffectUI CombatEffectUI;
        public StatusEffectsUI StatusEffectsUI;

        public Vector3 GetHitPosition()
        {
            if (CombatEffectUI != null && CombatEffectUI.DooberTarget != null)
            {
                return CombatEffectUI.DooberTarget.transform.position;
            }
            return transform.position;
        }
    }
}
