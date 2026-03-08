using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.UI.Timers
{
    public class CountdownTimer : BaseBehaviour
    {
        private ICountdownTimerService _timerService = null;

        public override void Init()
        {
            _timerService.AddCountdownTimer(this);  
        }

        public GText TimerText;

        private long _endOfTimerUnixEpochSeconds = 0;
        private long _currentUnixEpochSeconds = 0;

        public void SetData(DateTime endTime)
        {
            _endOfTimerUnixEpochSeconds = new DateTimeOffset(endTime).ToUnixTimeSeconds();
        }

        public void UpdateTime(long newUnixEpochSeconds)
        {
            if (_endOfTimerUnixEpochSeconds == 0)
            {
                return;
            }

            _currentUnixEpochSeconds = newUnixEpochSeconds;
            _uiService.SetText(TimerText, TimeUtils.PrintTime(_endOfTimerUnixEpochSeconds - _currentUnixEpochSeconds));
        }

        public bool IsExpired()
        {
            return _currentUnixEpochSeconds >= _endOfTimerUnixEpochSeconds;
        }
    }
}
