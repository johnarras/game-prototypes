using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayerFiltering.Utils;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.GameSettings.Settings
{
    // MessagePackIgnore
    public abstract class BaseDataOverrideSettings<TChild> : ParentSettings<TChild> where TChild : ChildSettings, IPlayerFilter, new()
    {
        // Temp internal data to make updating configs cheaper.
        [JsonIgnore][IgnoreMember] private DateTime _prevUpdateTime { get; set; } = DateTime.MinValue;
        [JsonIgnore][IgnoreMember] private DateTime _nextUpdateTime { get; set; } = DateTime.MaxValue;
        [IgnoreMember] private bool _didSetPrevNextUpdateTime = false;

        public override void SetData(List<TChild> data)
        {
            data = data.OrderBy(x => x.IdKey).ToList();
            foreach (TChild group in data)
            {
                group.OrderSelf();
            }

            _didSetPrevNextUpdateTime = false;

            base.SetData(data);
        }

        public DateTime GetNextUpdateTime(DateTime currTime)
        {
            if (!_didSetPrevNextUpdateTime)
            {
                SetPrevNextUpdateTimes(currTime);
            }
            return _nextUpdateTime;
        }

        public DateTime GetPrevUpdateTime(DateTime currTime)
        {
            if (!_didSetPrevNextUpdateTime)
            {
                SetPrevNextUpdateTimes(currTime);
            }
            return _prevUpdateTime;
        }


        public void SetPrevNextUpdateTimes(DateTime currTime)
        {

            List<TChild> data = _data.ToList();

            List<DateTime> _allUpdateTimes = data.Select(x => PlayerFilterUtils.GetNextStartDate(x, currTime))
                       .Union(data.Select(x => PlayerFilterUtils.GetNextEndDate(x, currTime)))
                       .Distinct().OrderBy(x => x).ToList();

            DateTime tempPrevUpdateTime = DateTime.MinValue;
            DateTime tempNextUpdateTime = DateTime.MaxValue;

            List<DateTime> updates = _allUpdateTimes;

            if (updates.Any(x => x <= currTime))
            {
                tempPrevUpdateTime = updates.Last(x => x <= currTime);
            }
            else
            {
                tempPrevUpdateTime = DateTime.MinValue;
            }

            if (updates.Any(x => x > currTime))
            {
                tempNextUpdateTime = updates.First(x => x > currTime);
            }
            else
            {
                tempNextUpdateTime = DateTime.MaxValue;
            }

            _prevUpdateTime = tempPrevUpdateTime;
            _nextUpdateTime = tempNextUpdateTime;
            _didSetPrevNextUpdateTime = true;
        }
    }
}

