
using Assets.Scripts.Trader.ClientEvents;
using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Attributes.Settings;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Trader.Stats.UI
{
    public class GameplayBuffsUI : BaseBehaviour
    {

        public GameObject IconAnchor;
        public GameplayBuffIcon IconPrefab;

        long _currentUnixEpochSeconds = 0;
        private List<GameplayBuffIcon> _icons = new List<GameplayBuffIcon>();
        public override void Init()
        {
            base.Init();
            _dispatcher.AddListener<UpdateTraderHUD>(OnUpdateGameplayStatsHandler, GetToken());
            ShowBuffs();
            _currentUnixEpochSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            _updateService.AddUpdate(this, UpdateBuffTimers, UpdateTypes.Regular, GetToken());
        }

        private void OnUpdateGameplayStatsHandler(UpdateTraderHUD update)
        {
            ShowBuffs();
        }

        private void ShowBuffs()
        {
            AttributeData attributeData = _gs.ch.Get<AttributeData>();

            IReadOnlyList<GameplayBuff> buffs = _gameData.Get<GameplayBuffSettings>(_gs.ch).GetData();

            DateTime nowTime = DateTime.UtcNow;

            List<GameplayBuffIcon> removeIcons = new List<GameplayBuffIcon>();

            long currBuffs = _gs.ch.Get<CoreData>().Vars[TraderVars.BuffBits];

            foreach (GameplayBuff buff in buffs)
            {
                GameplayBuffIcon currIcon = _icons.FirstOrDefault(x => x.GetBuffId() == buff.IdKey);


                if (currIcon != null)
                {
                    if (currIcon.IsExpired())
                    {
                        removeIcons.Add(currIcon);
                        continue;
                    }
                }
                else // No icon so add it.
                {
                    if (FlagUtils.HasBitIndex(currBuffs, buff.IdKey))
                    {
                        GameplayBuffIcon newIcon = _clientEntityService.FullInstantiate(IconPrefab);
                        _clientEntityService.AddToParent(newIcon, IconAnchor);
                        newIcon.SetData(buff, attributeData.Buffs[buff.IdKey]);
                        _icons.Add(newIcon);    
                    }
                }
            }

            foreach (GameplayBuffIcon icon in removeIcons)
            {
                _clientEntityService.Destroy(icon);
            }

            _icons = _icons.Except(removeIcons).OrderBy(x=>x.GetBuffStatus().EndTime).ToList(); 
        }


        private void UpdateBuffTimers()
        {
            long newUnixEpochSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (_currentUnixEpochSeconds >= newUnixEpochSeconds)
            {
                return;
            }
            _currentUnixEpochSeconds = newUnixEpochSeconds;
            if (_icons.Any(x=>x.IsExpired()))
            {
                ShowBuffs();
            }
        }
    }
}
