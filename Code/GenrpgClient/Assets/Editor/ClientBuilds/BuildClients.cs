using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Linq;
using Scripts.Assets.Assets.Constants;
using Genrpg.Shared.Setup.Services;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Core.Constants;
using System.Net;
using UnityEditor.SceneManagement;

public class BuildClients
{
    public static void BuildClient (string env, string gameModeStr, string platformName,
        bool selfContainedClient, bool rebuildBundles)
    {
        IClientGameState gs = EditorGameDataUtils.GetEditorGameState();

        if (string.IsNullOrEmpty(env))
        {
            Debug.LogError ("No environment set");
            return;
        }

        if (!Enum.TryParse (gameModeStr, out EGameModes gameMode))
        {
            Debug.LogError("Invalid game mode: " + gameModeStr);
            return;
        }

        List<PlatformBuildData> buildDataList = BuildConfiguration.GetbuildConfigs(gs);

        PlatformBuildData buildData = buildDataList.FirstOrDefault(x=>x.ClientPlatform == platformName);

        if (buildData == null)
        {
            Debug.LogError("Invalid build platform: " + platformName);
            return;
        }

        IClientConfigContainer _configContainer = gs.loc.Get<IClientConfigContainer>();
        ILogService logService = gs.loc.Get<ILogService>();
        ITextSerializer serializer = gs.loc.Get<ITextSerializer>();
        IAnalyticsService analyicsService = gs.loc.Get<IAnalyticsService>();
        IClientAppService appService = gs.loc.Get<IClientAppService>();

        string oldEnv = _configContainer.Config.Env;
        EGameModes oldGameMode = _configContainer.Config.GameMode;
        bool oldPlayerContainsAllAssets = _configContainer.Config.SelfContainedClient;
        _configContainer.Config.Env = env;
        _configContainer.Config.SelfContainedClient = selfContainedClient;    
        _configContainer.Config.GameMode = gameMode;
        string gamePrefix = gameMode.ToString();
        string lowerPrefix = gamePrefix.ToLower();
        EditorUtility.SetDirty(_configContainer.Config);
        AssetDatabase.SaveAssets();

        BundleVersions currentBundleVersions = CreateAssetBundles.CreateBundles(buildData.ClientPlatform, env, rebuildBundles,
            rebuildBundles && !selfContainedClient);

        EditorBuildSettingsScene mainScene = EditorBuildSettings.scenes.FirstOrDefault(x => x.path.IndexOf("GameMain") >= 0);

        if (mainScene == null)
        {
            Debug.Log("MainScene: GameMain is missing");
            return;
        }

        List<string> scenePaths = new List<string>();

        scenePaths.Add(mainScene.path);

        List<PlatformBuildData> configs = BuildConfiguration.GetbuildConfigs(gs);

        string[] sceneArray = new string[scenePaths.Count];
        for (int s = 0; s < scenePaths.Count; s++)
        {
            sceneArray[s] = scenePaths[s];
        }
        int oldVersion = 1;
        int version = 1;
        ClientBuildVersionSettings clientSettings = ClientBuildVersionSettings.GetClientVersionFile(env);
        if (clientSettings != null)
        {
            oldVersion = clientSettings.Version;
            clientSettings.Version++;
            version = clientSettings.Version;
        }


        ClientBuildVersionSettings.UpdateVersionFile(clientSettings, env);
        Debug.Log("Version: " + version);

        string outputZipFolder = "../../../Build/" + lowerPrefix + "/zips/";
        if (!Directory.Exists(outputZipFolder))
        {
            Directory.CreateDirectory(outputZipFolder);
        }

        Assembly servicesAssembly = Assembly.GetAssembly(typeof(SetupService));

        string lowerEnv = env.ToLower();
      
        string platformString = buildData.ClientPlatform.ToString();
        string appsuffix = buildData.ApplicationSuffix;
        string outputFilesFolder = "../../../Build/" + lowerPrefix + "/" + platformString + "/" + lowerEnv + "/";
        string outputPath = outputFilesFolder + lowerPrefix + appsuffix;
        string localFolderPath = appService.DataPath + "/" + outputFilesFolder;

        if (!Directory.Exists(outputFilesFolder))
        {
            Directory.CreateDirectory(outputFilesFolder);
        }
        BuildOptions options = BuildOptions.CompressWithLz4HC;

        if (Directory.Exists(appService.StreamingAssetsPath))
        {
            Directory.Delete(appService.StreamingAssetsPath, true);
        }
        string bundleOutputPath = buildData.GetBundleOutputPath();
        string[] files = Directory.GetFiles(bundleOutputPath);


        string versionFilePath = outputZipFolder + PatcherUtils.GetPatchVersionFilename();
        File.WriteAllText(versionFilePath, String.Empty);
        File.WriteAllText(versionFilePath, version.ToString());
        string localVersionPath =   appService.DataPath + "/../" + versionFilePath;
        string remoteVersionPath = PatcherUtils.GetPatchClientPrefix(gamePrefix, env, PlatformAssetPrefixes.Win, version) + PatcherUtils.GetPatchVersionFilename();

        foreach (BundleVersion bversion in currentBundleVersions.Versions.Values)
        {
            string origFilename = bundleOutputPath + "/" + bversion.Name;
            string newFilename = origFilename.Replace(bundleOutputPath, "");
            newFilename = newFilename.Replace("\\", "");

            if (newFilename == AssetConstants.BundleVersionsFile)
            {
                File.Copy(origFilename, "Assets/Resources/Config/" + newFilename, true);
            }
            else if (selfContainedClient || bversion.IsLocal)
            {
                if (!Directory.Exists(appService.StreamingAssetsPath))
                {
                    Directory.CreateDirectory(appService.StreamingAssetsPath);
                }
                File.Copy(origFilename, appService.StreamingAssetsPath + "/" + newFilename, true);
            }
        }

        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(i)); 
        }
        // Maybe do some stuff with debug symbols here.

        BuildPipeline.BuildPlayer(sceneArray, outputPath, buildData.Target, options);

        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(i));
        }
        _configContainer.Config.Env = oldEnv;
        _configContainer.Config.SelfContainedClient = oldPlayerContainsAllAssets;
        _configContainer.Config.GameMode = oldGameMode;
        EditorUtility.SetDirty(_configContainer.Config);
        AssetDatabase.SaveAssets();

        Debug.Log($"Finished building E: {env} G: {gameModeStr} P: {platformString} SC: {selfContainedClient} RB: {rebuildBundles}");   

    }
}
