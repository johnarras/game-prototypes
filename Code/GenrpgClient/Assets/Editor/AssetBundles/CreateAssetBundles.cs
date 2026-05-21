using Assets.Scripts.Assets.Bundles;
using Newtonsoft.Json;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.Serialization.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.DataStores.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundles
{


    public static async Awaitable<BundleVersions> CreateBundles(string platformString, string gameName, string env, bool rebuildBundles, bool uploadBundles)
    {
        DateTime startTime = DateTime.UtcNow;

        List<PlatformBuildData> targets = BuildConfiguration.GetbuildConfigs();

        PlatformBuildData target = targets.FirstOrDefault(x => x.ClientPlatform == platformString);

        if (target == null)
        {
            Debug.LogError("Bad platform sent to bundle build: " + platformString);
            return null;
        }

        string basePath = target.GetBundleOutputPath();
        string textFilePath = target.GetTextFileOutputPath();
        string bundleVersionPath = textFilePath + "/" + AssetConstants.BundleVersionsFile;
        string bundleUploadTimePath = textFilePath + "/" + AssetConstants.BundleUpdateTimeFile;
        string localBundleVersionsPath = Application.dataPath + "/Resources/Config/" + AssetConstants.BundleVersionsFile;
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        if (!rebuildBundles)
        {
            try
            {
                string bundleVersionText = File.ReadAllText(bundleVersionPath);
                BundleVersions existingVersions = JsonConvert.DeserializeObject<BundleVersions>(bundleVersionText);
                if (existingVersions != null)
                {
                    return existingVersions;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message + " Failed to load existing bundles, so rebuiling ");
                rebuildBundles = false;
            }
            rebuildBundles = true;
        }

        BundleList blist = SetupBundles.SetupAll();

        DirectoryInfo di = new DirectoryInfo(basePath);

        BundleUpdateInfo updateData = new BundleUpdateInfo() { ClientVersion = Application.version, UpdateTime = DateTime.UtcNow };

        BundleVersions versions = new BundleVersions() { UpdateInfo = updateData, ClientPlatform = target.ClientPlatform };

        BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.RecurseDependencies;

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
        ClearHashedFilenames(basePath);

        AssetBundleManifest manifest = null;
        try
        {
            manifest = BuildPipeline.BuildAssetBundles(basePath, options, target.Target);
        }
        catch (Exception e)
        {
            Debug.Log("Asset bundle exception: " + e.Message + "\n" + e.StackTrace);
        }

        // if the manifest exists, loop through all bundles and any that have 
        // changes, increment their versions.
        if (manifest == null)
        {
            Debug.LogError("Failed to build asset bundles for platform " + platformString);
            return null;
        }

        string[] bundles = manifest.GetAllAssetBundles();

        string[] files = Directory.GetFiles(basePath);

        foreach (string bundle in bundles)
        {
            string hash = manifest.GetAssetBundleHash(bundle).ToString();
            string bundle2 = bundle.Replace(hash, "").Replace("_", "");

            if (!versions.Versions.TryGetValue(bundle2, out var version))
            {

                BundleInfo bitem = blist.Bundles.FirstOrDefault(x => x.BundleName == bundle2);

                if (bitem == null)
                {
                    Debug.Log("Missing bundle list item for " + bundle2);
                }

                versions.Versions[bundle2] = new BundleVersion() { Name = bundle2, IsLocal = bitem?.IsLocal ?? false };
            }

            List<string> dependencies = manifest.GetAllDependencies(bundle2).ToList();

            if (dependencies.IndexOf("dungeonsmaterials") >= 0)
            {
                Debug.Log("Found DungeonMaterials");
            }

            versions.Versions[bundle2].ChildDependencies = dependencies;

            string filename = files.FirstOrDefault(x => x.Contains(bundle));

            if (filename != null)
            {
                string newFilename = filename + "_" + hash;
                File.Move(filename, newFilename);
            }

            BundleVersion bvd = versions.Versions[bundle2];
            bvd.Hash = hash;

        }

        List<string> loopDependencies = new List<string>();

        foreach (BundleVersion version in versions.Versions.Values)
        {
            foreach (string depName in version.ChildDependencies)
            {
                List<string> depList = GetLoopDependencies(versions, version.Name, depName);

                if (depList.Count > 0)
                {
                    depList.Add(version.Name);
                    StringBuilder sb = new StringBuilder();
                    foreach (string dep in depList)
                    {
                        sb.Append(dep + " ");
                    }
                    loopDependencies.Add(sb.ToString());
                }
            }
        }

        if (loopDependencies.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string depName in loopDependencies)
            {
                sb.Append(depName + "\n");
            }

            Debug.LogError("Loop Dependencies: " + sb.ToString());
        }


        // Now clean up bundle list.

        List<BundleInfo> extraInfos = new List<BundleInfo>();

        foreach (BundleInfo info in blist.Bundles)
        {
            if (!bundles.FastAny(x => x.ToLower() == info.BundleName.ToLower()))
            {
                extraInfos.Add(info);
            }
        }

        blist.Bundles = blist.Bundles.Except(extraInfos).ToList();

        blist.Bundles = blist.Bundles.OrderBy(x => x.BundleName).ToList();

        List<string> extraBundles = new List<string>();

        foreach (string bundleName in bundles)
        {
            if (!blist.Bundles.FastAny(x => x.BundleName.ToLower() == bundleName.ToLower()))
            {
                extraBundles.Add(bundleName);
            }
        }

        Debug.Log("Extra BundleInfo Count: " + extraInfos.Count + " Bundles: " + extraBundles.Count);

        foreach (BundleVersion version in versions.Versions.Values)
        {
            BundleInfo info = blist.Bundles.FirstOrDefault(x => x.BundleName.ToLower() == version.Name.ToLower());
            if (info != null)
            {
                version.IsLocal = info.IsLocal;
            }
        }

        try
        {
            NewtonsoftTextSerializer serializer = new NewtonsoftTextSerializer();
            File.WriteAllText(bundleVersionPath, serializer.PrettyPrint(versions));
            File.WriteAllText(bundleUploadTimePath, serializer.SerializeToString(updateData));
            File.WriteAllText(localBundleVersionsPath, serializer.PrettyPrint(versions));

        }
        catch (Exception e)
        {
            Debug.Log("Failed to write bundle version: " + e.Message);
        }

        if (uploadBundles)
        {
            await UploadAll(platformString, gameName, env);
        }


        ClearHashedFilenames(basePath);

        double seconds = (DateTime.UtcNow - startTime).TotalSeconds;

        Debug.Log("Bundle Creation Time: " + (int)seconds);

        return versions;
    }

    private static List<string> GetLoopDependencies(BundleVersions bundleVersions, string startBundleName, string currBundleName)
    {
        if (currBundleName == startBundleName)
        {
            return new List<string>() { currBundleName };
        }

        BundleVersion currBundle = bundleVersions.Versions[currBundleName];

        foreach (string depName in currBundle.ChildDependencies)
        {
            List<string> deps = GetLoopDependencies(bundleVersions, startBundleName, depName);

            if (deps.Count > 0)
            {
                deps.Add(currBundleName);
                return deps;
            }
        }
        return new List<string>();
    }

    private static async Awaitable UploadAll(string platformName, string gameName, string env)
    {
        if (string.IsNullOrEmpty(env) || env == EnvNames.Local)
        {
            env = EnvNames.Dev;
        }


        await InnerUploadFiles(platformName, gameName, env);
    }


    private static async Awaitable InnerUploadFiles(string platformName, string gameName, string env)
    {


        List<PlatformBuildData> targets = BuildConfiguration.GetbuildConfigs();

        for (int t = 0; t < targets.Count; t++)
        {
            if (targets[t].ClientPlatform != platformName)
            {
                continue;
            }

            string pathRoot = targets[t].GetTextFileOutputPath();

            string bundleVersionPath = targets[t].GetTextFileOutputPath() + "/" + AssetConstants.BundleVersionsFile;

            string txt = File.ReadAllText(bundleVersionPath);

            BundleVersions currentData = JsonConvert.DeserializeObject<BundleVersions>(txt);

            FolderUploadArgs uploadData = new FolderUploadArgs()
            {
                LocalFolder = Application.dataPath + "/../" + BuildConfiguration.AssetBundleRoot + targets[t].FilePath + "/",
                RemoteSubfolder = BlobUtils.GetBlobSubfolder(env, Application.version, targets[t].FilePath),
                IsWorldData = false,
                Env = env,
                GamePrefix = gameName,
            };

            uploadData.FilePatterns.Add("*_*");
            uploadData.FilePatterns.Add(AssetConstants.BundleVersionsFile);
            uploadData.FilePatterns.Add(AssetConstants.BundleUpdateTimeFile);

            await FileUploader.UploadFolder(uploadData, AssetConstants.BundleUpdateTimeFile);


        }
    }

    private static void ClearHashedFilenames(string basePath)
    {

        string[] startFiles = Directory.GetFiles(basePath);

        foreach (string startFile in startFiles)
        {
            if (startFile.IndexOf(".manifest") >= 0)
            {
                continue;
            }

            if (startFile.IndexOf("_") > 0)
            {
                int lastUnderscoreIndex = startFile.LastIndexOf('_');

                string newFile = startFile.Substring(0, lastUnderscoreIndex);

                File.Move(startFile, newFile);
            }
        }
    }

}


