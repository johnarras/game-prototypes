using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayMultiplier.Settings;
using System.Collections.Generic;

namespace Genrpg.Shared.PlayMultiplier.Services
{
    public interface ISharedPlayMultService : IInjectable
    {
        long GetMaxMult(IFilteredObject obj, long level, long energy);

        List<PlayMult> GetValidMults(IFilteredObject obj, long level, long energy);
    }
}
