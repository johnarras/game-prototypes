using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Purchasing.Settings;

namespace OxDb.DataUtils.Importers.Purchasing
{
    public class ProductSKUImporter : ParentChildImporter<ProductSkuSettings, ProductSku>
    {
        protected override void ImportSubobject(EditorGameState gs, ProductSkuSettings settings, ProductSku current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
