using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Threading;

namespace Assets.Scripts.Login.Messages
{
    public interface IClientWebResponseHandler : ISetupDictionaryItem<Type>
    {

        int Priority();
        void Process(IWebResponse result, CancellationToken token);
    }
}


