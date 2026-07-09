using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedGame.Inventory.Settings.ItemTypes;

namespace OxDb.DataUtils.Importers.Items
{
    public class ItemTypeSettingsImporter : ParentChildImporter<ItemTypeSettings, ItemType>
    {
        protected override void ImportSubobject(EditorGameState gs, ItemTypeSettings settings, ItemType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn.ToLower() == typeof(Effect).Name.ToLower())
            {
                current.Effects.Add(_importService.ImportLine<Effect>(gs, row, headers, rowWords));
            }
        }
    }
}
