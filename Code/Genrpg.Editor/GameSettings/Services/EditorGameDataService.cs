using Genrpg.ServerShared.GameSettings.Services;

namespace Genrpg.Editor.GameSettings.Services
{
    public class EditorGameDataService : ServerGameDataService
    {
        protected override bool CreateMissingData => true;
    }
}


