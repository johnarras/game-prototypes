using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.TimedEvents.Entities;
using Genrpg.Shared.TimedEvents.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.TimedEvents.Helpers
{
    public abstract class BaseActivityHelper<TCurrent, TThemeSettings, TTheme, TTierSettings, TTierList> :
        ITimedEventTypeHelper
        where TCurrent : IGameSettings, ICurrentTimedEventSettings, new()
        where TTierSettings : ParentSettings<TTierList>, ITimedEventTierSettings, new()
        where TTierList : ChildSettings, ITimedEventTierList, new()
        where TThemeSettings : ParentSettings<TTheme>, ITimedEventThemeSettings, new()
        where TTheme : ChildSettings, ITimedEventTheme, new()
    {
        protected IGameData _gameData = null;

        public abstract long UserCoinTypeId { get; }
        public abstract long Key { get; }

        public ICurrentTimedEventSettings GetCurrent(IFilteredObject obj)
        {
            return _gameData.Get<TCurrent>(obj);
        }

        public ITimedEventTierSettings GetTierSettings(IFilteredObject obj)
        {
            return _gameData.Get<TTierSettings>(obj);
        }

        public ITimedEventTierList GetTierList(IFilteredObject obj)
        {
            return GetTierSettings(obj).GetTierList(GetCurrent(obj).GetTierListId());
        }

        public ITimedEventThemeSettings GetThemeSettings(IFilteredObject obj)
        {
            return _gameData.Get<TThemeSettings>(obj);
        }

        public ITimedEventTheme GetTheme(IFilteredObject obj)
        {
            return GetThemeSettings(obj).GetTheme(GetCurrent(obj).GetThemeId());
        }

    }
}
