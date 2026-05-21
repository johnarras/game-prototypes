using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Entities.Helpers;
using OxDb.SharedGame.Quests.WorldData;
namespace OxDb.SharedGame.Quests.Helpers
{
    public class QuestItemHelper : BaseMapEntityHelper<QuestItem>
    {
        public override long HelperKey => EntityTypes.QuestItem;
    }
}


