using Assets.Scripts.Assets.Sprites.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Settings;
using System;

namespace Assets.Scripts.Trader.Stats.UI
{
    public class TraderDebuffIcon : BaseBehaviour
    {
        public GImage Icon;

        public GText DaysLeftText;

        private ISpriteService _spriteService = null;

        private TraderDebuff _debuff = null;
        private TraderDebuffStatus _status = null;

        public void SetData(TraderDebuff buff, TraderDebuffStatus status, long currentDebuffDays)
        {
            _debuff = buff;
            _status = status;
            _spriteService.LoadEntityIcon(EntityTypes.TraderDebuff, buff.IdKey, Icon, GetToken());

            ShowDaysLeft(currentDebuffDays);
        }

        public long GetDebuffId()
        {
            return _debuff?.IdKey ?? 0;
        }

        public TraderDebuffStatus GetDebuffStatus()
        {
            return _status;
        }

        public TraderDebuff GetDebuff()
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
