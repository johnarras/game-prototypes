using Genrpg.ServerShared.GameSettings.Services;

namespace Genrpg.DataUtils.GameSettings.Services
{
    public class EditorGameDataService : ServerGameDataService
    {
        protected override bool CreateMissingData => true;
    }
}


