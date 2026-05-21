using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedGame.Attributes.Constants;
using OxDb.SharedGame.Attributes.Settings;

namespace OxDb.DataUtils.Importers.Gameplay
{
    public class GameplayDebuffImporter : ParentChildImporter<GameplayDebuffSettings, GameplayDebuff>
    {
        protected override void ImportChildSubObject(EditorGameState gs, GameplayDebuff current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn == "debuffeffect")
            {
                Effect effect = _importService.ImportLine<Effect>(gs, row, headers, rowWords);

                if (!_attributeService.EntityTypeHasValIndex(effect.EntityTypeId, EAttributeValIndex.Buff))
                {
                    throw new Exception($"Buff Importer row{row} has non-buff entity type in it's effect.");
                }

                current.Effects.Add(effect);
            }
        }
    }
}
