using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.PlayMultiplier.WebApi
{
    public class SetPlayMultResponse : IWebResponse
    {
        public bool Success { get; set; }
        public int NewPlayMult { get; set; }
        public int MultBonusSpeed { get; set; }
    }
}


