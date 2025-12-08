using Genrpg.Shared.Crawler.Info.InfoHelpers;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Animals.Settings;
using System.Collections.Generic;

namespace Assets.Scripts.Trader.Info.Helpers
{
    public class AnimalInfoHelper : BaseInfoHelper<AnimalSettings, Animal>
    {
        public override long HelperKey => EntityTypes.Animal;

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = base.GetInfoLines(entityId);


            return lines;

        }
    }
}
