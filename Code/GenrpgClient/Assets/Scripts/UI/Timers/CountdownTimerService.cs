using Assets.Scripts.GameObjects;
using OxDb.SharedCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Timers
{

    public interface ICountdownTimerService : IInitializable
    {
        void AddCountdownTimer(CountdownTimer timer);
    }


    public class CountdownTimerService : ICountdownTimerService
    {
        private IClientEntityService _clientEntityService = null;
        private IClientUpdateService _updateService = null;
        private List<CountdownTimer> _timers = new List<CountdownTimer>();
        private long _lastUnixEpochUpdateSeconds = 0;

        public void AddCountdownTimer(CountdownTimer timer)
        {
            _timers.Add(timer);
            _clientEntityService.RegisterDestroyCallback(timer, () => { _timers.Remove(timer); });
        }

        public async Task Initialize(CancellationToken token)
        {
            _lastUnixEpochUpdateSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _updateService.AddUpdate(this, OnUpdateTimers, UpdateTypes.Regular, token);
            await Task.CompletedTask;
        }

        private void OnUpdateTimers()
        {
            long currUnixEpochUpdateSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (currUnixEpochUpdateSeconds <= _lastUnixEpochUpdateSeconds)
            {
                return;
            }
            foreach (CountdownTimer timer in _timers)
            {
                if (timer.IsExpired())
                {
                    continue;
                }
                timer.UpdateTime(currUnixEpochUpdateSeconds);
            }
        }
    }
}
