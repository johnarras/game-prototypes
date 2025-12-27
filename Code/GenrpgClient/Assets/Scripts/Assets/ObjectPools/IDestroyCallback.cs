using System;

namespace Assets.Scripts.Assets.ObjectPools
{

    public interface IDestroyCallback
    {
        void SetDestroyCallback(Action action);
    }
}


