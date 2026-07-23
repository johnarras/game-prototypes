using OxDb.SharedCore.Interfaces;
using Unity.Mathematics;

namespace OxDb.Client.Lockstep.Presentation.Services
{
    public interface ILockstepVisualFactory : IInjectable
    {
        void SpawnMapTile(long biomeTypeId, float3 visualPos, int cellSize);
    }
    public class LockstepVisualFactory : ILockstepVisualFactory
    {

        // private IAssetService _assetService = null;
        public static LockstepVisualFactory Instance { get; private set; }
        public LockstepVisualFactory()
        {
            Instance = this;
        }

        public void SpawnMapTile(long biomeTypeId, float3 visualPos, int cellSize)
        {
        }
    }
}
