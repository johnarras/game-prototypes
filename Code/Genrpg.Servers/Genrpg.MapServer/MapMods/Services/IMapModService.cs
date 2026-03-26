using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapMods.MapObjects;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.MapMods.Services
{
    public interface IMapModService : IInjectable
    {
        void Process(IRandom rand, MapMod mapMod);
    }
}


