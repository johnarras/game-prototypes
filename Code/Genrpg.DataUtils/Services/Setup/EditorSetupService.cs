
using Genrpg.ServerShared.Setup;

namespace Genrpg.DataUtils.Services.Setup
{
    public class EditorSetupService : BaseServerSetupService
    {
        public override bool CreateMissingGameData() { return true; } 
    }
}


