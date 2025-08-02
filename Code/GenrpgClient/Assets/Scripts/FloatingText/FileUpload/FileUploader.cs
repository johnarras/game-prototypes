
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class FileUploader
{
    public static void UploadFolder(FolderUploadArgs fdata)
    {

        string json = JsonConvert.SerializeObject(fdata);
        byte[] bytes = System.Text.ASCIIEncoding.UTF8.GetBytes(json);
        string base64 = System.Convert.ToBase64String(bytes);

        UnityEngine.Debug.Log(base64);

        ProcessStartInfo psi = new ProcessStartInfo();
        string filePath = Application.dataPath.Replace("Assets", "../FileUploader/Output/FileUploader.exe");

        psi.FileName = filePath;

        string env = fdata.Env;

        psi.Arguments = base64;
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        psi.CreateNoWindow = true;
        Process process = Process.Start(psi);

        process.WaitForExit();

        string outputTextFilePath = Application.dataPath + "/../AssetBundles/upload_output.txt";

        string outputText = "";
        if (File.Exists(outputTextFilePath))
        {
            outputText = File.ReadAllText(outputTextFilePath);   
        }

        UnityEngine.Debug.Log(outputText + " Files Sent from " + fdata.LocalFolder + " to " + fdata.RemoteSubfolder);
    }
}
