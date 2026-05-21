
using OxDb.ServerCore.Setup;

namespace OxDb.DataUtils.Services.Setup
{
    public class EditorSetupService : BaseServerSetupService
    {
        public override bool CreateMissingGameData() { return true; }
    }
}


