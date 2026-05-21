using OxDb.DataUtils.Entities.Core;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Attributes.Constants;
using OxDb.SharedGame.Trader.CaravanMembers.Settings;

namespace OxDb.DataUtils.Importers.Trader
{
    public class CaravanMemberImporter : BaseTraderDataImporter<CaravanMemberSettings, CaravanMember>
    {
        protected override void ImportChildSubObject(EditorGameState gs, CaravanMember current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn == "membereffect")
            {
                Effect effect = _importService.ImportLine<Effect>(gs, row, headers, rowWords);

                if (!_attributeService.EntityTypeHasValIndex(effect.EntityTypeId, EAttributeValIndex.Buff))
                {
                    throw new Exception($"Buff Importer row{row} has non-buff entity type in it's effect.");
                }

                current.Effects.Add(effect);
            }
        }

        protected override async Task<bool> UpdateAfterImport(EditorGameState gs)
        {
            await base.UpdateAfterImport(gs);

            IReadOnlyList<CaravanMember> members = gs.data.Get<CaravanMemberSettings>(null).GetData();

            IReadOnlyList<SkinType> skinTypes = gs.data.Get<SkinTypeSettings>(null).GetData();

            foreach (CaravanMember member in members)
            {
                if (member.DefaultSkinTypeId > 0)
                {
                    continue;
                }

                SkinType skinType = skinTypes.FirstOrDefault(x => StrUtils.NormalizeWord(x.Name) == StrUtils.NormalizeWord(member.Name));

                if (skinType != null)
                {
                    member.DefaultSkinTypeId = skinType.IdKey;
                }
            }

            return true;
        }
    }
}


