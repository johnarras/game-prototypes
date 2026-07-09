using OxDb.DataUtils.Entities.Core;
using OxDb.SharedGame.Purchasing.Settings;

namespace OxDb.DataUtils.Importers.Purchasing
{
    /// <summary>
    /// Used to import things that are not in default stores.
    /// </summary>
    public class StoreOfferSettingsImporter : BaseStoreOfferImporter<StoreOfferSettings, StoreOffer>
    {
        protected override void ImportSubobject(EditorGameState gs, StoreOfferSettings settings, StoreOffer current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}


