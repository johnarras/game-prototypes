using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Info.InfoHelpers;
using OxDb.SharedGame.Inventory.Settings.Ranks;
using System.Collections.Generic;

namespace Assets.Scripts.Crawler.Shared.Info.InfoHelpers
{
    public class LootRankInfoHelper : BaseInfoHelper<LootRankSettings, LootRank>
    {
        public override long HelperKey => EntityTypes.LootRank;

        public override bool OrderByName => false;

        protected override bool MakeEntityNamePlural() { return false; }
        public override List<string> GetInfoLines(long entityId)
        {
            List<string> startList = base.GetInfoLines(entityId);


            LootRankSettings settings = _gameData.Get<LootRankSettings>(_gs.ch);

            LootRank rank = settings.Get(entityId);

            startList.Add("Danage Scale: " + (int)(rank.DamageScale * 100) + "%");

            startList.Add("Defense Scale: " + (int)(rank.DefenseScale * 100) + "%");

            startList.Add("Cost Scale: " + (int)(rank.CostScale * 100) + "%");


            long approxLevel = (int)(rank.IdKey * settings.LevelsPerQuality);

            startList.Add("Found around level " + approxLevel + ".");

            return startList;

        }
    }
}
