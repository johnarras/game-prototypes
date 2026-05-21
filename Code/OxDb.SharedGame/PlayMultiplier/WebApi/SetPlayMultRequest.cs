using OxDb.SharedCore.Website.Interfaces;

namespace OxDb.SharedGame.PlayMultiplier.WebApi
{
    public class SetPlayMultRequest : IClientUserRequest
    {
        public int PlayMult { get; set; }
    }
}


