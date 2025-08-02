using Genrpg.Editor.Services.Importing;
using Genrpg.Editor.Services.Reflection;
using Genrpg.ServerShared.Setup;

namespace Genrpg.Editor.Services.Setup
{
    public class EditorSetupService : BaseServerSetupService
    {
        public override bool CreateMissingGameData() { return true; } 
    }
}
