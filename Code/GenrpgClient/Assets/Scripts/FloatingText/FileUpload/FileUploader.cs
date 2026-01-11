
using Assets.Scripts.Config;
using Genrpg.Shared.Config.Constants;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Utils;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
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


    public static void UploadFolder(FolderUploadArgs fdata, string expectedFilename)
    {

        try
        {

            string uploadBaseContainer = BlobUtils.GetBlobContainerName(EDataCategories.Assets.ToString(), fdata.GamePrefix, fdata.Env);

            string uploadFullContainer = uploadBaseContainer + "/" + fdata.RemoteSubfolder;

            // Feel free to change this to something else you use in your jenkins or whatever build.
            Dictionary<string, string> kvDict = XmlUtils.ExtractAppConfigData(ConfigConstants.MainAppConfigPath);

            string exeName = GetAzCopyExecutable();

            string exePath = Application.dataPath.Replace("Assets", "../Uploads/" + exeName);
            string sourcePath = fdata.LocalFolder;

            string uploadURL = kvDict[AppConfigKeys.BlobUploadURL].Replace(AppConfigKeys.PlaceholderString, uploadFullContainer);

            int QuestionIndex = uploadURL.IndexOf("?");

            string uploadPrefix = uploadURL.Substring(0, QuestionIndex);

            uploadURL = uploadURL.Replace("&amp;", "&");

            ClientWebService webService = new ClientWebService();
            string txt = (webService.DownloadTextFile(uploadPrefix + "/" + expectedFilename)).ToString();

            string command = ((string.IsNullOrEmpty(txt) || txt.Contains("BlobNotFound")) ? "copy" : "sync");

            uploadURL = $"{uploadURL}";
            sourcePath = $"{sourcePath}";
            string recursiveVal = "--recursive=true";


            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = exePath,
                ArgumentList = { command, sourcePath, uploadURL, recursiveVal },
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

        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }
}


