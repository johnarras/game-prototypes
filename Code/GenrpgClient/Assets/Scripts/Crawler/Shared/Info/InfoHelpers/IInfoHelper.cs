using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Info.InfoHelpers
{
    public interface IInfoHelper : ISetupDictionaryItem<long>
    {
        List<string> GetInfoLines(long entityId);
        string GetTypeName();
        List<IIdName> GetInfoChildren();
        bool OverviewTypeNameIsPlural();
    }
}


