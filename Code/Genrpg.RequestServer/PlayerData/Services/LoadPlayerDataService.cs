using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayerData.LoadUpdateHelpers;
using Genrpg.RequestServer.Resets.Entities;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.RequestServer.Trader.Stats.Services;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.PlayerData.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.PlayerData.Services
{
    public interface ILoadPlayerDataService : IInitializable
    {
        Task<List<IUnitData>> LoadPlayerDataOnLogin(WebContext context, Character ch = null);
        Task UpdatePlayerAfterLoginOrLoad(WebContext context, bool isLogin);
    }

    public class LoadPlayerDataService : ILoadPlayerDataService
    {


        OrderedSetupDictionaryContainer<Type, ICharacterLoadUpdater> _characterLoadUpdateHelpers = new OrderedSetupDictionaryContainer<Type, ICharacterLoadUpdater>();
        OrderedSetupDictionaryContainer<Type, IUserLoadUpdater> _userLoadUpdateHelpers = new OrderedSetupDictionaryContainer<Type, IUserLoadUpdater>();
        private IPlayerDataService _playerDataService = null;
        private IHourlyUpdateService _periodicUpdateService = null;
        private IServerGameStatService _statService = null;
        private IServerGameDataService _gameDataService = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task<List<IUnitData>> LoadPlayerDataOnLogin(WebContext context, Character ch = null)
        {
            List<IUnitData> dataList = await _playerDataService.LoadAllPlayerData(context.rand, context.GameUserId, context.AllData(), ch);

            List<IUnitData> allData = context.AllData();
            foreach (IUnitData unitData in dataList)
            {
                IUnitData existingData = allData.FirstOrDefault(x => x.GetType() == unitData.GetType());

                if (existingData != null)
                {
                    continue;
                }

                context.Set(unitData);
            }

            if (ch != null)
            {
                foreach (IUnitData data in dataList)
                {
                    ch.Set(data);
                }

                await UpdateCharacterOnLoad(context, ch);
            }
            else
            {
                await UpdateUserOnLoad(context, dataList);
            }
            return dataList;
        }

        protected async Task UpdateCharacterOnLoad(WebContext context, Character ch)
        {
            foreach (ICharacterLoadUpdater updater in _characterLoadUpdateHelpers.OrderedItems())
            {
                await updater.Update(context, ch);
            }
        }

        protected async Task UpdateUserOnLoad(WebContext context, List<IUnitData> userUnitData)
        {
            foreach (IUserLoadUpdater updater in _userLoadUpdateHelpers.OrderedItems())
            {
                await updater.Update(context, userUnitData);
            }
        }

        public async Task UpdatePlayerAfterLoginOrLoad(WebContext context, bool isLogin)
        {
            context.AddResponseRange(_gameDataService.GetClientSettings(await context.GetAsync<CoreData>(), isLogin));
            await _periodicUpdateService.CheckHourlyCurrencyUpdate(context, new HourlyResetArgs() { OnLogin = isLogin });
            await _statService.CheckBuffs(context, isLogin);
        }
    }
}


