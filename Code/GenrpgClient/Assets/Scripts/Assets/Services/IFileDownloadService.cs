using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Interfaces;
using System;
using System.Threading;

public interface IFileDownloadService : IInitializable
{
    void DownloadFile(string url, DownloadFileData data, CancellationToken token);
    void DownloadTypedFile<T>(string url, Action<T> handler, EDataCategories category, CancellationToken token) where T : class;
}


