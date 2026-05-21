using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Trader.CaravanMembers.Settings;
using OxDb.SharedGame.Trader.Holdings.PlayerData;

namespace OxDb.SharedGame.Trader.CaravanMembers.Services
{

    public interface ICaravanMemberService : IInjectable
    {
        void AddCaravanMemberToHoldings(CoreData coreData, HoldingsData holdings, long caravanMemberId);
        void AddSkinToHoldings(CoreData coreData, HoldingsData holdings, long skinTypeId);

        long GetCaravanMemberQuantity(HoldingsData holdings, long caravanMemberId);

        long GetSkinQuantity(HoldingsData holdings, long skinTypeId);
    }

    public class CaravanMemberService : ICaravanMemberService
    {
        private IGameData _gameData = null;

        public void AddCaravanMemberToHoldings(CoreData coreData, HoldingsData holdings, long caravanMemberId)
        {
            if (!holdings.CaravanMembersOwned.HasBitIndex(caravanMemberId))
            {
                holdings.CaravanMembersOwned.SetBitIndex(caravanMemberId);
                CaravanMember member = _gameData.Get<CaravanMemberSettings>(coreData).Get(caravanMemberId);
                if (member != null && member.DefaultSkinTypeId > 0)
                {
                    AddSkinToHoldings(coreData, holdings, member.DefaultSkinTypeId);
                }
            }
        }

        public void AddSkinToHoldings(CoreData coreData, HoldingsData holdings, long skinTypeId)
        {
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

        public long GetCaravanMemberQuantity(HoldingsData holdings, long caravanMemberId)
        {
            return holdings.CaravanMembersOwned.HasBitIndex(caravanMemberId) ? 1 : 0;
        }

        public long GetSkinQuantity(HoldingsData holdings, long skinTypeId)
        {
            return holdings.SkinsOwned.HasBitIndex(skinTypeId) ? 1 : 0;
        }
    }
}
