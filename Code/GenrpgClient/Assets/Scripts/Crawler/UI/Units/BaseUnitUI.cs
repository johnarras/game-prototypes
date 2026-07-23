using OxDb.Client.Crawler.Combat;
using OxDb.Client.Crawler.UI.StatusUI;
using OxDb.Client.UI.CombatTexts;
using UnityEngine;

namespace OxDb.Client.Crawler.UI.Units
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


