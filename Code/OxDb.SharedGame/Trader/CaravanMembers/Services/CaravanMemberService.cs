using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.CaravanMembers.Settings;
using OxDb.SharedGame.Trader.Holdings.PlayerData;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.CaravanMembers.Services
{

    public interface ICaravanMemberService : IInjectable
    {
        ValueTask AddCaravanMemberToHoldings(IUnitDataLookup lookup, long caravanMemberId);
        ValueTask AddSkinToHoldings(IUnitDataLookup lookup, long skinTypeId);

        ValueTask<long> GetCaravanMemberQuantity(IUnitDataLookup lookup, long caravanMemberId);

        ValueTask<long> GetSkinQuantity(IUnitDataLookup lookup, long skinTypeId);
    }

    public class CaravanMemberService : ICaravanMemberService
    {
        private IGameData _gameData = null;

        public async ValueTask AddCaravanMemberToHoldings(IUnitDataLookup lookup, long caravanMemberId)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();
            HoldingsData holdings = await lookup.GetAsync<HoldingsData>();
            if (!holdings.CaravanMembersOwned.HasBitIndex(caravanMemberId))
            {
                holdings.CaravanMembersOwned.SetBitIndex(caravanMemberId);
                CaravanMember member = _gameData.Get<CaravanMemberSettings>(coreData).Get(caravanMemberId);
                if (member != null && member.DefaultSkinTypeId > 0)
                {
                    AddSkinToHoldings(lookup, member.DefaultSkinTypeId);
                }
            }
        }

        public async ValueTask AddSkinToHoldings(IUnitDataLookup lookup, long skinTypeId)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();
            HoldingsData holdings = await lookup.GetAsync<HoldingsData>();
            SkinType skinType = _gameData.Get<SkinTypeSettings>(coreData).Get(skinTypeId);

            if (skinType == null)
            {
                return;
            }

            if (!holdings.SkinsOwned.HasBitIndex(skinTypeId))
            {
                holdings.SkinsOwned.SetBitIndex(skinTypeId);
            }
        }

        public async ValueTask<long> GetCaravanMemberQuantity(IUnitDataLookup lookup, long caravanMemberId)
        {
            HoldingsData holdings = await lookup.GetAsync<HoldingsData>();
            return holdings.CaravanMembersOwned.HasBitIndex(caravanMemberId) ? 1 : 0;
        }

        public async ValueTask<long> GetSkinQuantity(IUnitDataLookup lookup, long skinTypeId)
        {
            HoldingsData holdings = await lookup.GetAsync<HoldingsData>();
            return holdings.SkinsOwned.HasBitIndex(skinTypeId) ? 1 : 0;
        }
    }
}
