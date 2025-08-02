using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileUploader
{
    class Program
    {

        const int MaxUploads = 30;
        public static async Task Main(string[] args)
        {
            StringBuilder output = new StringBuilder();

            //args = new string[1];
            //args[0] = "eyJHYW1lUHJlZml4IjoiR2VucnBnIiwiRW52IjoiZGV2IiwiTG9jYWxGb2xkZXIiOiJDOi9EYXRhL2V4cGVyaW1lbnRzL0NvZGUvR2VucnBnQ2xpZW50L0Fzc2V0cy8uLi9Bc3NldEJ1bmRsZXMvd2luLyIsIlJlbW90ZVN1YmZvbGRlciI6IjAuMC42L3dpbi8iLCJJc1dvcmxkRGF0YSI6ZmFsc2UsIk92ZXJ3cml0ZUlmRXhpc3RzRmlsZXMiOlsiYnVuZGxlVmVyc2lvbnMudHh0IiwiYnVuZGxlVXBkYXRlVGltZS50eHQiXX0=";

            CancellationTokenSource cts = new CancellationTokenSource();

            CancellationToken token = cts.Token;

            int uploadCount = 0;
            try
            {
                if (args.Length < 1)
                {
                    return;
                }

                byte[] bytes = System.Convert.FromBase64String(args[0]);
                string json = System.Text.Encoding.UTF8.GetString(bytes);
                FolderUploadArgs uploadData = JsonConvert.DeserializeObject<FolderUploadArgs>(json);

                string connectionStringVar = (args[0] == "True" ?
                    "BlobWorldsConnection" : "BlobAssetsConnection");

                var _connectionString = ConfigurationManager.AppSettings[connectionStringVar];

                if (_connectionString == "Default" || string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = ConfigurationManager.AppSettings["BlobDefaultConnection"];
                }

                BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);

                string containerName = uploadData.GamePrefix.ToLower() + uploadData.Env.ToLower();

                string remoteFolder = containerName + "/" + uploadData.RemoteSubfolder;

                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer, null, null, token);

                List<string> allFiles = Directory.GetFiles(uploadData.LocalFolder).ToList();

                List<string> noOverwriteFiles = new List<string>(allFiles);

                List<string> overwriteFiles = new List<string>();
                foreach (string delayedName in uploadData.OverwriteIfExistsFiles)
                {
                    List<String> currentDelayedFiles = noOverwriteFiles.Where(x => x.ToLower().Contains(delayedName.ToLower())).ToList();
                    foreach (string delayedFile in currentDelayedFiles)
                    {
                        overwriteFiles.Add(delayedFile);
                    }
                }

                foreach (string delayedFile in overwriteFiles)
                {
                    noOverwriteFiles.Remove(delayedFile);
                }

                noOverwriteFiles.AddRange(overwriteFiles);

              uploadCount += await UploadFileList(containerClient, uploadData, noOverwriteFiles, false, token);
              uploadCount += await UploadFileList(containerClient, uploadData, overwriteFiles, true, token);

            }
            catch (Exception e)
            {
                output.Append("Exception: " + e.Message + " " + e.StackTrace);
            }

            output.Append(uploadCount);

            output.Append(" Foo");

            File.WriteAllText("AssetBundles/upload_output.txt", output.ToString());
        }


        private static async Task<int> UploadFileList(BlobContainerClient containerClient, FolderUploadArgs uploadData, List<string> fileNames, bool overwriteIfExists,
            CancellationToken token)
        {
            List<Task<int>> uploadTasks = new List<Task<int>>();

            int uploadCount = 0;
            foreach (string localFilePath in fileNames)
            {
                int index = localFilePath.LastIndexOf('/');

                string remoteFilePath = localFilePath.Substring(index + 1);

                if (remoteFilePath.IndexOf(".meta") >= 0 || remoteFilePath.IndexOf(".manifest") >= 0)
                {
                    continue;
                }

                remoteFilePath = uploadData.RemoteSubfolder + remoteFilePath;

                BlobClient blobClient = containerClient.GetBlobClient(remoteFilePath);

                byte[] fileBytes = File.ReadAllBytes(localFilePath);

                BinaryData bdata = new BinaryData(fileBytes);

                uploadTasks.Add(UploadFilename(containerClient, localFilePath, remoteFilePath, overwriteIfExists, token));

                if (uploadTasks.Count >= MaxUploads)
                {
                    uploadCount += (await Task.WhenAll(uploadTasks)).Sum();

                    uploadTasks.Clear();
                }
            }

            if (uploadTasks.Count > 0)
            {
                uploadCount += (await Task.WhenAll(uploadTasks)).Sum();
            }

            return uploadCount;
        }

        private static async Task<int> UploadFilename(BlobContainerClient containerClient, string localFilePath, string remoteBlobPath, bool overwriteIfExists, CancellationToken token)
        {

            BlobClient blobClient = containerClient.GetBlobClient(remoteBlobPath);
            if (!overwriteIfExists && await blobClient.ExistsAsync(token))
            {
                return 0;
            }

            byte[] fileBytes = File.ReadAllBytes(localFilePath);
            BinaryData bdata = new BinaryData(fileBytes);

            await blobClient.UploadAsync(new BinaryData(fileBytes), true, token);
            return 1;
        }

    }
}
