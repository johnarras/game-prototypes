using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayerData.LoadUpdateHelpers;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.HelperClasses;

namespace Genrpg.RequestServer.PlayerData.Services
{
    public class LoginPlayerDataService : ILoginPlayerDataService
    {

        OrderedSetupDictionaryContainer<Type, ICharacterLoadUpdater> _characterLoadUpdateHelpers = new OrderedSetupDictionaryContainer<Type, ICharacterLoadUpdater>();
        OrderedSetupDictionaryContainer<Type, IUserLoadUpdater> _userLoadUpdateHelpers = new OrderedSetupDictionaryContainer<Type, IUserLoadUpdater>();
        private IPlayerDataService _playerDataService = null!;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task<List<IUnitData>> LoadPlayerDataOnLogin(WebContext context, Character ch = null)
        {
            List<IUnitData> dataList = await _playerDataService.LoadAllPlayerData(context.rand, context.user, ch);

            List<IUnitData> allData = context.GetAllData();
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
            if (context.user.Level < 1)
            {
                context.user.Level = 1;
            }

            foreach (IUserLoadUpdater updater in _userLoadUpdateHelpers.OrderedItems())
            {
                await updater.Update(context, userUnitData);
            }        
        }
    }
}
