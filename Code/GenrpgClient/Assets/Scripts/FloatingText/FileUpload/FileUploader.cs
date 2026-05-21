
using Assets.Scripts.Config;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.DataStores.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

public class FileUploader
{

    private static string GetAzCopyExecutable()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows is almost exclusively x64 for development
            return "azcopy_win_32.exe";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Check if we are on Apple Silicon (M1/M2/M3) or Intel
            if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                return "azcopy_osx_arm";
            }
            else
            {
                return "azcopy_osx_amd";
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "azcopy_linux_x64";
        }
        return "azcopy_win_32.exe";
    }


    public static async Awaitable UploadFolder(FolderUploadArgs fdata, string expectedFilename)
    {


        if (fdata.FilePatterns.Count < 1)
        {
            UnityEngine.Debug.LogError("File upload doesn't specify any file patterns.");
            return;
        }

        string uploadBaseContainer = BlobUtils.GetBlobContainerName(EDataCategories.Assets.ToString(), fdata.GamePrefix, fdata.Env);

        string uploadFullContainer = uploadBaseContainer + "/" + fdata.RemoteSubfolder;

        // Feel free to change this to something else you use in your jenkins or whatever build.
        Dictionary<string, string> kvDict = XmlUtils.ExtractAppConfigData(ConfigConstants.MainAppConfigPath);

        string exeName = GetAzCopyExecutable();

        string exePath = Application.dataPath.Replace("Assets", "../Uploads/" + exeName);
        string sourcePath = fdata.LocalFolder;

        string uploadURL = kvDict[AppConfigKeys.BlobUploadURL].Replace(AppConfigKeys.PlaceholderString, uploadFullContainer);


        ClientWebService webService = new ClientWebService();

        ResponseEnvelope<string> responseEnvelope = await webService.SendRawWebRequest<string>(uploadURL + "/" + expectedFilename, HttpMethod.Get);

        string command = "sync";

        if (!string.IsNullOrEmpty(responseEnvelope.ErrorMessage))
        {
            command = "copy";
        }

        uploadURL = $"{uploadURL}";
        sourcePath = $"{sourcePath}";
        string recursiveVal = "--recursive=true";

        foreach (string filePattern in fdata.FilePatterns)
        {
            string finalSourcePath = sourcePath + filePattern;

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = exePath,
                    ArgumentList = { command, finalSourcePath, uploadURL, recursiveVal },
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode == 0)
                        UnityEngine.Debug.Log("AzCopy Sync Successful: " + output);
                    else
                        UnityEngine.Debug.LogError("AzCopy Sync Failed: " + error);
                }

                await Task.Delay(1000);
            }

            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
    }
}


