using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Website.Responses.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Login.Messages
{
    public interface IClientWebResponseHandler : ISetupDictionaryItem<Type>
    {

        int Priority();
        ValueTask Process(IWebResponse result, CancellationToken token);
    }
}


