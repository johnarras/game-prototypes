using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Purchasing.Settings;

namespace Genrpg.Editor.Importers.Purchasing
{
    /// <summary>
    /// Used to import things that are not in default stores.
    /// </summary>
    public class StoreOfferSettingsImporter : BaseStoreOfferImporter<StoreOfferSettings, StoreOffer>
    {
        protected override void ImportChildSubObject(EditorGameState gs, StoreOffer current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}


