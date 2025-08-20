using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public interface IInfoHelper : ISetupDictionaryItem<long>
    {
        List<string> GetInfoLines(long entityId);
        string GetTypeName();
        List<IIdName> GetInfoChildren();
    }
}
