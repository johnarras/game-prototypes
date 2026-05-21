using OxDb.SharedCore.Website.Responses.Interfaces;

namespace OxDb.SharedGame.PlayMultiplier.WebApi
{
    public class SetPlayMultResponse : IWebResponse
    {
        public bool Success { get; set; }
        public int NewPlayMult { get; set; }
        public int MultBonusSpeed { get; set; }
    }
}


