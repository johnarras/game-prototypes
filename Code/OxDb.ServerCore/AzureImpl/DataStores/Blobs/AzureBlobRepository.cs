using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using OxDb.ServerCore.AzureImpl.DataStores.Entities;
using OxDb.SharedCore.DataStores.Entities;
using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.DataStores.Utils;
using System.Text;

namespace OxDb.ServerCore.AzureImpl.DataStores.Blobs
{
    public class AzureBlobRepository : IRepository
    {


        private ILogService _logService = null;
        private ITextSerializer _serializer = null;

        private InitRepoArgs _args = null;

        private BlobServiceClient _serviceClient = null;
        private BlobContainerClient _container = null;

        CancellationToken _token;

        public async Task Init(InitRepoArgs args,
            BlobServiceClient serviceClient,
            ILogService logService,
            ITextSerializer serializer,
            CancellationToken token)
        {
            _token = token;
            _logService = logService;
            _serializer = serializer;
            _args = args;

            _serviceClient = serviceClient;
            string containerName = BlobUtils.GetBlobContainerName(args.Category.ToString(), args.ProductName, args.Env);
            _container = _serviceClient.GetBlobContainerClient(containerName);

            try
            {
                await _container.CreateIfNotExistsAsync(PublicAccessType.Blob, null, null);
            }
            catch (Exception ee)
            {
                _logService.Exception(ee, "CreateBlobContainer");
            }
        }

        #region Core
        private BlobClient GetBlockBlobReference(Type t, string id)
        {
            return _container.GetBlobClient(StrUtils.NormalizeTypeName(t) + "/" + id);
        }

        // Breakd LSP
        public async Task CreateIndex<T>(CreateIndexData data) where T : class, IStringId
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
        #endregion

        #region Save
        /// <summary>
        /// Save to a blob
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="verbose">This does nothing here.</param>
        /// <returns></returns>
        public async Task<bool> Save<T>(T t, RepoSaveArgs args = null) where T : IStringId
        {
            string data = _serializer.SerializeToString(t);

            BlobClient blob = GetBlockBlobReference(t.GetType(), t.Id);

            bool success = false;
            int maxTimes = 2;
            for (int times = 0; times < maxTimes; times++)
            {
                try
                {
                    using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
                    await blob.UploadAsync(stream, overwrite: true).ConfigureAwait(false);
                    success = true;
                    break;
                }
                catch (Exception e)
                {
                    if (times < maxTimes - 1)
                    {
                        await Task.Delay(100).ConfigureAwait(false);
                    }
                    _logService.Exception(e, "AzureBlobRepository.Save");
                }
            }
            return success;
        }

        #endregion

        #region Delete
        public async Task<bool> Delete<T>(T t) where T : class, IStringId
        {
            BlobClient blob = GetBlockBlobReference(t.GetType(), t.Id);

            bool success = false;
            Response response = await blob.DeleteAsync().ConfigureAwait(false);
            return !response.IsError;
        }


        #endregion

        #region Load
        public async Task<T> Load<T>(string id) where T : class, IStringId
        {
            T obj = default;
            BlobClient blob = GetBlockBlobReference(typeof(T), id);

            int maxTimes = 1;
            for (int times = 0; times < maxTimes; times++)
            {
                try
                {
                    using (BlobDownloadInfo info = await blob.DownloadAsync().ConfigureAwait(false))
                    {
                        using (StreamReader streamReader = new StreamReader(info.Content))
                        {
                            string txt = await streamReader.ReadToEndAsync();
                            if (!string.IsNullOrEmpty(txt))
                            {
                                obj = _serializer.Deserialize<T>(txt);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logService.Exception(e, "AzoreBlobReposiotry.Load");
                    if (times < maxTimes - 1)
                    {
                        await Task.Delay(100).ConfigureAwait(false);
                    }
                }

                if (obj != null)
                {
                    break;
                }
            }

            return obj;
        }

        #endregion
    }
}


