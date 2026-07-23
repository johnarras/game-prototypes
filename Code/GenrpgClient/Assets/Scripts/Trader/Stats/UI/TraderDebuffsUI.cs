using OxDb.Client.Trader.ClientEvents;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Attributes.PlayerData;
using OxDb.SharedGame.Attributes.Settings;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Trader.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OxDb.Client.Trader.Stats.UI
{
    public class GameplayDebuffsUI : BaseBehaviour
    {

        public GameObject IconAnchor;
        public GameplayDebuffIcon IconPrefab;

        long _currentUnixEpochSeconds = 0;
        private List<GameplayDebuffIcon> _icons = new List<GameplayDebuffIcon>();
        public override void Init()
        {
            base.Init();
            _dispatcher.AddListener<UpdateTraderHUD>(OnUpdateGameplayStatsHandler, GetToken());
            ShowDebuffs();
            _currentUnixEpochSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void OnUpdateGameplayStatsHandler(UpdateTraderHUD update)
        {
            ShowDebuffs();
        }

        private void ShowDebuffs()
        {
            AttributesData AttributesData = _gs.ch.Get<AttributesData>();

            IReadOnlyList<GameplayDebuff> debuffs = _gameData.Get<GameplayDebuffSettings>(_gs.ch).GetData();

            DateTime nowTime = DateTime.UtcNow;

            List<GameplayDebuffIcon> removeIcons = new List<GameplayDebuffIcon>();

            CoreData coreData = _gs.ch.Get<CoreData>();

            long currDebuffs = coreData.Vars[TraderVars.DebuffBits];

            long debuffDaysPlayed = coreData.Vars[TraderVars.DebuffDaysPlayed];

            foreach (GameplayDebuff debuff in debuffs)
            {
                GameplayDebuffIcon currIcon = _icons.FirstOrDefault(x => x.GetDebuffId() == debuff.IdKey);

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
                        GameplayDebuffIcon newIcon = _clientEntityService.FullInstantiate(IconPrefab);
                        _clientEntityService.AddToParent(newIcon, IconAnchor);
                        newIcon.SetData(debuff, AttributesData.Debuffs[debuff.IdKey], debuffDaysPlayed);
                        _icons.Add(newIcon);
                    }
                }
            }

            foreach (GameplayDebuffIcon icon in removeIcons)
            {
                _clientEntityService.Destroy(icon);
            }

            _icons = _icons.Except(removeIcons).OrderBy(x => x.GetDebuffStatus().EndDebuffPlayCount).ToList();
        }
    }
}
