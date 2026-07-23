using OxDb.Client.ProcGen.Materials.Constants;
using OxDb.SharedCore.Interfaces;
using UnityEngine;

namespace OxDb.Client.ProcGen.Materials.MaterialGenHelpers
{
    public interface IMaterialGenHelper : ISetupDictionaryItem<EMaterialGenTypes>
    {
        Awaitable<Texture2D> GenerateTexture(MaterialGenState state);
    }
}
