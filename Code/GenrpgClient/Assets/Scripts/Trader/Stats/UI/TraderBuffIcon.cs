using Assets.Scripts.Assets.Sprites.Services;
using Assets.Scripts.UI.Timers;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Trader.Stats.UI
{
    public class TraderBuffIcon : BaseBehaviour
    {
        public GImage Icon;

        public CountdownTimer Timer;

        private ISpriteService _spriteService = null;

        private TraderBuff _buff = null;
        private TraderBuffStatus _status = null;

        public void SetData(TraderBuff buff, TraderBuffStatus status)
        {
            _buff = buff;
            _status = status;
            _spriteService.LoadEntityIcon(EntityTypes.TraderBuff, buff.IdKey, Icon, GetToken());
            Timer.SetData(_status.EndTime);
        }

        public long GetBuffId()
        {
            return _buff?.IdKey ?? 0;
        }

        public TraderBuffStatus GetBuffStatus()
        {
            return _status;
        }

        public TraderBuff GetBuff()
        {
            return _buff;
        }

        public bool IsExpired()
        {
            return Timer.IsExpired();
        }
    }
}
