using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Purchasing.Services;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Serialization.Interfaces;

namespace Genrpg.DataUtils.Importers.Purchasing
{
    /// <summary>
    /// Used to import an entire set of default stores for the player (needs ab test id)
    /// </summary>
    public abstract class BaseStoreOfferImporter<TParent, TChild> : ParentChildImporter<TParent, TChild> where TParent : ParentSettings<TChild>, new() where TChild : ChildSettings, IIdName, new()
    {
        protected override bool IsIncrementalImporter()
        {
            return true;
        }
        protected ITextSerializer _serializer = null;
        protected IPurchasingService _purchasingService = null;

        protected override async Task<bool> UpdateAfterImport(EditorGameState gs)
        {
            StoreOfferSettings offerSettings = gs.data.Get<StoreOfferSettings>(null);
            StoreBundleSetSettings bundleSettings = gs.data.Get<StoreBundleSetSettings>(null);


            DefaultStoreOfferSettings defaultSettings = gs.data.Get<DefaultStoreOfferSettings>(null);

            defaultSettings.Offers = new List<PlayerStoreOffer>();

            Dictionary<long, StoreOffer> defaultOffers = new Dictionary<long, StoreOffer>();
            foreach (StoreOffer offer in offerSettings.GetData())
            {
                if (offer.IsDefaultOffer)
                {
                    defaultOffers[offer.StoreSlotId] = offer;
                }
            }

            foreach (StoreOffer offer in defaultOffers.Values)
            {
                PlayerStoreOffer playerOffer = _purchasingService.CreatePlayerStoreOffer(null, offer);

                if (playerOffer != null)
                {
                    defaultSettings.Offers.Add(playerOffer);
                }
            }

            gs.LookedAtObjects.Add(defaultSettings);


            await Task.CompletedTask;
            return true;
        }
    }
}


