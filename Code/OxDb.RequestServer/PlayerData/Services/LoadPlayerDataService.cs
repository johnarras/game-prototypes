
using OxDb.RequestServer.Core;
using OxDb.RequestServer.PlayerData.LoadUpdateHelpers;
using OxDb.RequestServer.Resets.Entities;
using OxDb.RequestServer.Resets.Services;
using OxDb.RequestServer.Trader.Stats.Services;
using OxDb.ServerCore.GameSettings.Services;
using OxDb.ServerGame.PlayerData.Services;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;

namespace OxDb.RequestServer.PlayerData.Services
{
    public interface ILoadPlayerDataService : IInitializable
    {
        Task<List<IUnitData>> LoadPlayerDataOnLogin(WebContext context, Character ch = null, GameAccount currentGameAccount = null);
        Task UpdatePlayerAfterLoginOrLoad(WebContext context, bool isLogin);
    }

    public class LoadPlayerDataService : ILoadPlayerDataService
    {


        OrderedSetupDictionaryContainer<ECharacterLoadUpdateOrder, ICharacterLoadUpdater> _characterLoadUpdateHelpers = new OrderedSetupDictionaryContainer<ECharacterLoadUpdateOrder, ICharacterLoadUpdater>();
        OrderedSetupDictionaryContainer<EUserLoadUpdateOrder, IUserLoadUpdater> _userLoadUpdateHelpers = new OrderedSetupDictionaryContainer<EUserLoadUpdateOrder, IUserLoadUpdater>();
        private IPlayerDataService _playerDataService = null;
        private IHourlyUpdateService _periodicUpdateService = null;
        private IServerGameStatService _statService = null;
        private IServerGameDataService _gameDataService = null;
        private ILogService _logService = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task<List<IUnitData>> LoadPlayerDataOnLogin(WebContext context, Character ch = null, GameAccount gameAccount = null)
        {

            try
            {
                List<IUnitData> dataList = await _playerDataService.LoadAllPlayerData(context.Rand, context.GameUserId, context.AllData(), ch);

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

                if (gameAccount != null)
                {
                    long existingDataBits = gameAccount.DataBits;
                    long newDataBits = 0;
                    foreach (IUnitData unitData in dataList)
                    {
                        if (unitData is IUniquePersonalUserData personalData)
                        {
                            if (!personalData.WasEverSaved())
                            {
                                if (FlagUtils.HasBitIndex((long)existingDataBits, personalData.GetOffsetBit()))
                                {
                                    throw new Exception($"Existing Data: {personalData.GetType().Name} failed to load.");
                                }
                            }
                            else
                            {
                                newDataBits |= (long)(1 << personalData.GetOffsetBit());
                            }

                            PersonalDataAccumulation currAccumulation = gameAccount.Accumulations[personalData.GetOffsetBit()];

                            PersonalDataAccumulation newAccumulation = personalData.GetAccumulation();

                            if (currAccumulation != null)
                            {
                                if (currAccumulation.SumValues.Count > newAccumulation.SumValues.Count)
                                {
                                    throw new Exception($"Accumulation on {personalData.GetType().Name}.");
                                }

                                for (int s = 0; s < currAccumulation.SumValues.Count; s++)
                                {
                                    if (currAccumulation.SumValues[s] > newAccumulation.SumValues[s])
                                    {
                                        throw new Exception($"Accumulation lost when loading {personalData.GetType().Name}" +
                                            $" at offset {s} was {currAccumulation.SumValues[s]} is {newAccumulation.SumValues[s]}");
                                    }
                                }
                            }

                            gameAccount.Accumulations[personalData.GetOffsetBit()] = newAccumulation;
                        }
                    }

                    if ((existingDataBits & ~newDataBits) != 0)
                    {
                        throw new Exception("Existing player data was lost and failed to load.");
                    }

                    gameAccount.DataBits = newDataBits;

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
            catch (Exception ex)
            {
                _logService.Exception(ex, "LoadAllPlayerData");
                throw (ex);
            }
            return null;
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
            await _periodicUpdateService.CheckHourlyCurrencyUpdates(context, new HourlyResetArgs() { OnLogin = isLogin });
            await _statService.CheckBuffs(context, isLogin);
        }
    }
}


