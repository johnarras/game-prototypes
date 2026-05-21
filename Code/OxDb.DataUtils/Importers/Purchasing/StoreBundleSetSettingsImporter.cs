using OxDb.DataUtils.Entities.Core;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Purchasing.Settings;


namespace OxDb.DataUtils.Importers.Purchasing
{

    public class StoreBundleImportRow
    {
        public long Index { get; set; }
        public double Price { get; set; }
        public string Name { get; set; }
        public string BundleId { get; set; }
    }

    /// <summary>
    /// Used for importing sets of bundles for the user.
    /// </summary>
    public class StoreBundleSetSettingsImporter : BaseStoreOfferImporter<StoreBundleSetSettings, StoreBundleSet>
    {
        protected override void ImportChildSubObject(EditorGameState gs, StoreBundleSet current, int line, string firstColumn, string[] headers, string[] rowWords)
        {

            if (firstColumn == StrUtils.NormalizeTypeName<StoreBundle>())
            {
                StoreBundleImportRow row = _importService.ImportLine<StoreBundleImportRow>(gs, line, headers, rowWords);

                ProductSku sku = gs.data.Get<ProductSkuSettings>(null).GetData().FirstOrDefault(x => x.DollarPrice == row.Price);

                StoreBundle bundle = _serializer.ConvertType<StoreBundleImportRow, StoreBundle>(row);

                bundle.ProductSkuId = sku.IdKey;

                current.Bundles.Add(bundle);
            }
            else if (firstColumn == "reward")
            {
                current.Bundles.Last().Rewards.Add(_importService.ImportLine<Reward>(gs, line, headers, rowWords));
            }
        }
    }
}


