using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapMods.MapObjects;

namespace OxDb.MapServer.MapMods.Services
{
    public interface IMapModService : IInjectable
    {
        void Process(IRandom rand, MapMod mapMod);
    }
}


