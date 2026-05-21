using OxDb.ServerCore.GameSettings.Services;

namespace OxDb.DataUtils.GameSettings.Services
{
    public class EditorGameDataService : ServerGameDataService
    {
        protected override bool CreateMissingData => true;
    }
}


