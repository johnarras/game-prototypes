using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Rewards.Entities;
using System.Linq;

namespace Genrpg.Editor.Importers.Purchasing
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
    public class BundleSetImporter : BaseStoreOfferImporter<StoreBundleSetSettings, StoreBundleSet>
    {
        public override string ImportDataFilename => "StoreBundleSetImport.csv";

        public override EImportTypes HelperKey => EImportTypes.StoreBundles;

        protected override void ImportChildSubObject(EditorGameState gs, StoreBundleSet current, int line, string firstColumn, string[] headers, string[] rowWords)
        {

            if (firstColumn == typeof(StoreBundle).Name.ToLower())
            {
                StoreBundleImportRow row = _importService.ImportLine<StoreBundleImportRow>(gs, line, rowWords, headers);

                ProductSku sku = gs.data.Get<ProductSkuSettings>(null).GetData().FirstOrDefault(x => x.DollarPrice == row.Price);

                StoreBundle bundle = _serializer.ConvertType<StoreBundleImportRow, StoreBundle>(row);

                bundle.ProductSkuId = sku.IdKey;

                current.Bundles.Add(bundle);
            }
            else if (firstColumn == "reward")
            {
                current.Bundles.Last().Rewards.Add(_importService.ImportLine<Reward>(gs, line, rowWords, headers));
            }
        }
    }
}


