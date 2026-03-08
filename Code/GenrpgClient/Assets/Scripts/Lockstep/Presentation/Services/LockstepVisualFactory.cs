using Assets.Scripts.Lockstep.Game.Services;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;

namespace Assets.Scripts.Lockstep.Presentation.Services
{
    public interface ILockstepVisualFactory : IInjectable
    {
        void SpawnMapTile(long biomeTypeId, float3 visualPos, int cellSize);
    }
    public class LockstepVisualFactory : ILockstepVisualFactory  
    {

        // private IAssetService _assetService = null;
        public static LockstepVisualFactory Instance { get; private set;  }
        public LockstepVisualFactory()
        {
            Instance = this;
        }

        public void SpawnMapTile(long biomeTypeId, float3 visualPos, int cellSize)
        {
        }
    }
}
