using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;

namespace Assets.Scripts.Core
{
    public interface IClientRandom : IRandomContainer, IInjectable
    {
    }
    public class ClientRandom : IClientRandom
    {

        public IRandom Rand { get; set; } = new MyRandom();

        public ClientRandom()
        {

        }

        public ClientRandom(long seed)
        {
            Rand = new MyRandom(seed);
        }
    }
}
