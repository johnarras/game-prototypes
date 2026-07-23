using System;

namespace OxDb.Client.Assets.ObjectPools
{

    public interface IDestroyCallback
    {
        void SetDestroyCallback(Action action);
    }
}


