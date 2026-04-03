using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;

namespace Assets.Scripts.Core
{
    public interface IClientRandom : IRandom, IInjectable
    {
    }
    public class ClientRandom : MyRandom, IClientRandom
    {
        public ClientRandom() : base() { }

        public ClientRandom(long seed) : base(seed) { }
    }
}
