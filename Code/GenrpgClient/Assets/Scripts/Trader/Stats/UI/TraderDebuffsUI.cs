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
    public class TraderDebuffsUI : BaseBehaviour
    {

        public GameObject IconAnchor;
        public TraderDebuffIcon IconPrefab;

        long _currentUnixEpochSeconds = 0;
        private List<TraderDebuffIcon> _icons = new List<TraderDebuffIcon>();
        public override void Init()
        {
            base.Init();
            _dispatcher.AddListener<UpdateTraderHUD>(OnUpdateTraderStatsHandler, GetToken());
            ShowDebuffs();
            _currentUnixEpochSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void OnUpdateTraderStatsHandler(UpdateTraderHUD update)
        {
            ShowDebuffs();
        }

        private void ShowDebuffs()
        {
            TraderStatData statData = _gs.ch.Get<TraderStatData>();

            IReadOnlyList<TraderDebuff> debuffs = _gameData.Get<TraderDebuffSettings>(_gs.ch).GetData();

            DateTime nowTime = DateTime.UtcNow;

            List<TraderDebuffIcon> removeIcons = new List<TraderDebuffIcon>();

            CoreData coreData = _gs.ch.Get<CoreData>();

            long currDebuffs = coreData.Vars[TraderVars.DebuffBits];

            long debuffDaysPlayed = coreData.Vars[TraderVars.DebuffDaysPlayed];

            foreach (TraderDebuff debuff in debuffs)
            {
                TraderDebuffIcon currIcon = _icons.FirstOrDefault(x => x.GetDebuffId() == debuff.IdKey);

                if (currIcon != null)
                {
                    if (currIcon.DaysLeft(debuffDaysPlayed) <= 0)
                    {
                        removeIcons.Add(currIcon);
                        continue;
                    }
                    else
                    {
                        currIcon.ShowDaysLeft(debuffDaysPlayed);
                    }
                }
                else // No icon so add it.
                {
                    if (FlagUtils.HasBitIndex(currDebuffs, debuff.IdKey))
                    {
                        TraderDebuffIcon newIcon = _clientEntityService.FullInstantiate(IconPrefab);
                        _clientEntityService.AddToParent(newIcon, IconAnchor);
                        newIcon.SetData(debuff, statData.Debuffs[debuff.IdKey], debuffDaysPlayed);
                        _icons.Add(newIcon);
                    }
                }
            }

            foreach (TraderDebuffIcon icon in removeIcons)
            {
                _clientEntityService.Destroy(icon);
            }

            _icons = _icons.Except(removeIcons).OrderBy(x => x.GetDebuffStatus().EndDebuffPlayCount).ToList();
        }
    }
}
