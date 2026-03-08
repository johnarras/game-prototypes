
using Assets.Scripts.Trader.ClientEvents;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Trader.Stats.UI
{
    public class TraderBuffsUI : BaseBehaviour
    {

        public GameObject IconAnchor;
        public TraderBuffIcon IconPrefab;

        long _currentUnixEpochSeconds = 0;
        private List<TraderBuffIcon> _icons = new List<TraderBuffIcon>();
        public override void Init()
        {
            base.Init();
            _dispatcher.AddListener<UpdateTraderHUD>(OnUpdateTraderStatsHandler, GetToken());
            ShowBuffs();
            _currentUnixEpochSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            _updateService.AddUpdate(this, UpdateBuffTimers, UpdateTypes.Regular, GetToken());
        }

        private void OnUpdateTraderStatsHandler(UpdateTraderHUD update)
        {
            ShowBuffs();
        }

        private void ShowBuffs()
        {
            TraderStatData statData = _gs.ch.Get<TraderStatData>();

            IReadOnlyList<TraderBuff> buffs = _gameData.Get<TraderBuffSettings>(_gs.ch).GetData();

            DateTime nowTime = DateTime.UtcNow;

            List<TraderBuffIcon> removeIcons = new List<TraderBuffIcon>();

            long currBuffs = _gs.ch.Get<CoreData>().Vars[TraderVars.BuffBits];

            foreach (TraderBuff buff in buffs)
            {
                TraderBuffIcon currIcon = _icons.FirstOrDefault(x => x.GetBuffId() == buff.IdKey);


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
                        TraderBuffIcon newIcon = _clientEntityService.FullInstantiate(IconPrefab);
                        _clientEntityService.AddToParent(newIcon, IconAnchor);
                        newIcon.SetData(buff, statData.Buffs[buff.IdKey]);
                        _icons.Add(newIcon);    
                    }
                }
            }

            foreach (TraderBuffIcon icon in removeIcons)
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
