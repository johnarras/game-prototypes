using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.Quests.WorldData;
namespace Genrpg.Shared.Quests.Helpers
{
    public class QuestItemHelper : BaseMapEntityHelper<QuestItem>
    {
        public override long Key => EntityTypes.QuestItem;
    }
}
