using Assets.Scripts.Assets.Sprites.Services;
using Assets.Scripts.UI.Timers;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Attributes.PlayerData;
using OxDb.SharedGame.Attributes.Settings;

namespace Assets.Scripts.Trader.Stats.UI
{
    public class GameplayBuffIcon : BaseBehaviour
    {
        public GImage Icon;

        public CountdownTimer Timer;

        private ISpriteService _spriteService = null;

        private GameplayBuff _buff = null;
        private GameplayBuffStatus _status = null;

        public void SetData(GameplayBuff buff, GameplayBuffStatus status)
        {
            _buff = buff;
            _status = status;
            _spriteService.SetEntityIcon(EntityTypes.GameplayBuff, buff.IdKey, Icon, GetToken());
            Timer.SetData(_status.EndTime);
        }

        public long GetBuffId()
        {
            return _buff?.IdKey ?? 0;
        }

        public GameplayBuffStatus GetBuffStatus()
        {
            return _status;
        }

        public GameplayBuff GetBuff()
        {
            return _buff;
        }

        public bool IsExpired()
        {
            return Timer.IsExpired();
        }
    }
}
