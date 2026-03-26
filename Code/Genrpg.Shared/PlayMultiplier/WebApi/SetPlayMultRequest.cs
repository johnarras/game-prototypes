using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.PlayMultiplier.WebApi
{
    public class SetPlayMultRequest : IClientUserRequest
    {
        public int PlayMult { get; set; }
    }
}


