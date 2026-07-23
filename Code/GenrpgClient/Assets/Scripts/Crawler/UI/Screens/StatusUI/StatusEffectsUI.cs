using OxDb.SharedCore.Utils;
using OxDb.SharedGame.UnitEffects.Settings;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace OxDb.Client.Crawler.UI.StatusUI
{
    public class StatusEffectsUI : BaseBehaviour
    {

        public GameObject IconAnchor;

        public StatusEffectIcon IconPrefab;

        private List<StatusEffectIcon> _effectIcons = new List<StatusEffectIcon>();

        private long _currentStatusEffects = 0;
        public void SetData(Unit unit)
        {

            long newStatusEffects = unit.StatusEffects.Bits[0];

            if (newStatusEffects == _currentStatusEffects)
            {
                return;
            }

            IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

            List<StatusEffectIcon> removeList = new List<StatusEffectIcon>();

            foreach (StatusEffectIcon effectIcon in _effectIcons)
            {
                if (!unit.StatusEffects.HasBitIndex(effectIcon.GetStatusEffectId()))
                {
                    removeList.Add(effectIcon);
                }
            }

            foreach (StatusEffectIcon effectIcon in removeList)
            {
                _clientEntityService.Destroy(effectIcon.gameObject);
                _effectIcons.Remove(effectIcon);
            }

            foreach (StatusEffect effect in effects)
            {
                if (unit.StatusEffects.HasBitIndex(effect.IdKey))
                {
                    if (!_effectIcons.FastAny(x => x.GetStatusEffectId() == effect.IdKey))
                    {
                        StatusEffectIcon newIcon = _clientEntityService.FullInstantiate<StatusEffectIcon>(IconPrefab);
                        _clientEntityService.AddToParent(newIcon, IconAnchor);
                        _effectIcons.Add(newIcon);
                        newIcon.SetData(effect.IdKey);
                    }
                }
            }
        }
    }
}


