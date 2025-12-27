using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.Services
{
    public interface IPurchasingService : IInjectable
    {
        PlayerStoreOffer CreatePlayerStoreOffer(IFilteredObject user, StoreOffer storeOffer);
    }


    public class PurchasingService : IPurchasingService
    {
        private IGameData _gameData = null;

        public PlayerStoreOffer CreatePlayerStoreOffer(IFilteredObject user, StoreOffer storeOffer)
        {
            ProductSkuSettings productSkuSettings = _gameData.Get<ProductSkuSettings>(user);
            StoreFeatureSettings storeFeatureSettings = _gameData.Get<StoreFeatureSettings>(user);
            StoreSlotSettings slotSettings = _gameData.Get<StoreSlotSettings>(user);
            StoreSlot slot = slotSettings.Get(storeOffer.StoreSlotId);
            StoreFeature feature = storeFeatureSettings.Get(storeOffer.StoreFeatureId);

            if (slot == null || feature == null)
            {
                return null;
            }

            StoreBundleSet bundleSet = _gameData.Get<StoreBundleSetSettings>(user).Get(storeOffer.StoreBundleSetId);

            if (bundleSet == null || bundleSet.Bundles.Count < 1)
            {
                return null;
            }


            PlayerStoreOffer playerStoreOffer = new PlayerStoreOffer()
            {
                StoreFeatureId = storeOffer.StoreFeatureId,
                StoreSlotId = storeOffer.StoreSlotId,
                StoreThemeId = storeOffer.StoreThemeId,
                EndDate = storeOffer.EndDate,
                Art = storeOffer.Art,
                Desc = storeOffer.Desc,
                Icon = storeOffer.Icon,
                IdKey = storeOffer.IdKey,
                Name = storeOffer.Name,
                OfferId = storeOffer.OfferId,
            };

            for (int p = 0; p < bundleSet.Bundles.Count; p++)
            {
                StoreBundle storeBundle = bundleSet.Bundles[p];
                ProductSku sku = productSkuSettings.Get(storeBundle.ProductSkuId);

                if (storeBundle.Rewards != null && storeBundle.Rewards.Count > 0 && sku != null)
                {
                    PlayerBundle playerBundle = new PlayerBundle()
                    {
                        Index = p,
                        ProductSkuId = sku.IdKey,
                        UniqueId = HashUtils.NewUUId(),
                        BundleId = storeBundle.BundleId
                    };

                    playerBundle.Rewards = new List<Reward>(storeBundle.Rewards);

                    playerStoreOffer.Bundles.Add(playerBundle);
                }
            }


            if (playerStoreOffer.Bundles.Count > 0)
            {
                return playerStoreOffer;
            }
            return null;
        }
    }
}


