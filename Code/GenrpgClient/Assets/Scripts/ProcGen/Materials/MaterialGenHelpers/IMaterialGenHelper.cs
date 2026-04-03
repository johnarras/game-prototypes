using Assets.Scripts.ProcGen.Materials.Constants;
using Genrpg.Shared.Interfaces;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials.MaterialGenHelpers
{
    public interface IMaterialGenHelper : ISetupDictionaryItem<EMaterialGenTypes>
    {
        Awaitable<Texture2D> GenerateTexture(MaterialGenState state);
    }
}
