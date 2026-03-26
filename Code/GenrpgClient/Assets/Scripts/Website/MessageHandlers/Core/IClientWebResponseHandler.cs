using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Login.Messages
{
    public interface IClientWebResponseHandler : ISetupDictionaryItem<Type>
    {

        int Priority();
        Awaitable Process(IWebResponse result, CancellationToken token);
    }
}


