
using Assets.Scripts.Config;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.DataStores.Utils;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Scripts.FileUploads
{
    public static class FileUploader
    {
        private static string GetAzCopyExecutable()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "azcopy_win_32.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
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

#if !UNITY_EDITOR
            UnityEngine.Debug.LogError("Can only upload files from the editor.");
    return; 
#endif

            if (fdata.FilePatterns.Count < 1)
            {
                UnityEngine.Debug.LogError("File upload doesn't specify any file patterns.");
                return;
            }

            string uploadBaseContainer = BlobUtils.GetBlobContainerName(fdata.GamePrefix, fdata.Env, EDataCategories.Assets.ToString());
            string uploadFullContainer = uploadBaseContainer + "/" + fdata.RemoteSubfolder;

            XmlDict kvDict = XmlUtils.ExtractAppConfigData(ConfigConstants.MainAppConfigPath);

            string exeName = GetAzCopyExecutable();
            string exePath = Application.dataPath.Replace("Assets", "../Uploads/" + exeName);
            string sourcePath = fdata.LocalFolder;
            string uploadURL = kvDict[AppConfigKeys.BlobUploadURL].Replace(AppConfigKeys.PlaceholderString, uploadFullContainer);

            string command = "sync";


            foreach (string filePattern in fdata.FilePatterns)
            {

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        FileName = exePath,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    };

                    // Populate arguments cleanly
                    psi.ArgumentList.Add(command);
                    psi.ArgumentList.Add(sourcePath);
                    psi.ArgumentList.Add(uploadURL);
                    psi.ArgumentList.Add("--recursive=true");
                    psi.ArgumentList.Add($"--include-pattern={filePattern}");

                    using (Process process = Process.Start(psi))
                    {
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();

                        process.WaitForExit();

                        if (process.ExitCode == 0)
                        {
                            UnityEngine.Debug.Log("AzCopy Sync Successful:\n" + output);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("AzCopy Sync Failed:\n" + error);
                        }
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }
    }
}
