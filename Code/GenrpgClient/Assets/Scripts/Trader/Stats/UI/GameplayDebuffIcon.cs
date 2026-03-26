using Assets.Scripts.Assets.Sprites.Services;
using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Attributes.Settings;
using Genrpg.Shared.Entities.Constants;
using System;

namespace Assets.Scripts.Trader.Stats.UI
{
    public class GameplayDebuffIcon : BaseBehaviour
    {
        public GImage Icon;

        public GText DaysLeftText;

        private ISpriteService _spriteService = null;

        private GameplayDebuff _debuff = null;
        private GameplayDebuffStatus _status = null;

        public void SetData(GameplayDebuff buff, GameplayDebuffStatus status, long currentDebuffDays)
        {
            _debuff = buff;
            _status = status;
            _spriteService.SetEntityIcon(EntityTypes.GameplayDebuff, buff.IdKey, Icon, GetToken());

            ShowDaysLeft(currentDebuffDays);
        }

        public long GetDebuffId()
        {
            return _debuff?.IdKey ?? 0;
        }

        public GameplayDebuffStatus GetDebuffStatus()
        {
            return _status;
        }

        public GameplayDebuff GetDebuff()
        {
            return _debuff;
        }

        public void ShowDaysLeft(long currentDebuffDays)
        {
            long daysLeft = ((_status?.EndDebuffPlayCount ?? 0) - currentDebuffDays);
            if (daysLeft <= 0)
            {
                _uiService.SetText(DaysLeftText, "");
            }
            else
            {
                _uiService.SetText(DaysLeftText, daysLeft + " Day" + (daysLeft > 1 ? "s" : ""));
            }
        }

        public long DaysLeft(long currentDebuffDays)
        {
            return Math.Max(0, (_status?.EndDebuffPlayCount ?? currentDebuffDays));
        }
    }
}
