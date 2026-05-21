using Assets.Scripts.Config;
using Newtonsoft.Json;
using OxDb.SharedCore.Client.Contants;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.Names.Entities;
using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace RunBuilds
{
    public class PlayerBuilder
    {

        public static async Awaitable BuildWithArgs(string env, string gameModeStr, string platformName,
            bool selfContainedClient, bool exportGameData, bool encryptExportedData,
            bool isCloudBuild, bool developmentBuild)
        {
            Dictionary<string, string> dict = SetupEnvironmentVariableDictionary(env, gameModeStr, platformName,
                selfContainedClient, exportGameData, encryptExportedData, developmentBuild);

            if (isCloudBuild)
            {
                await CloudBuildWithEnvVars(dict);
                // Send dict to Unity to set env vars
                return;
            }
            else // These should just use environment variables set in the above.
            {
                SetBuildEnvironmentVariables(dict);
                await PreExport();
                await LocalBuildPlayer();
                await PostExport();
                ClearBuildEnvironmentVariables();
            }
        }

        public class CloudBuildArgs
        {
            public bool clean { get; set; }
            public int delay { get; set; }
            public string commit { get; set; }
            public string scmBranch { get; set; }
            public Dictionary<string, string> env { get; set; } = new Dictionary<string, string>();
            public string scriptDefineSymbols { get; set; }
            public string comment { get; set; }
        }

        private static async Awaitable<string> SendDevOpsRequest(string requestSuffix, HttpMethod method, string target, object requestData)
        {

            await Awaitable.MainThreadAsync();
            Dictionary<string, string> kvDict = XmlUtils.ExtractAppConfigData(ConfigConstants.MainAppConfigPath);

            string apiKey = kvDict[AppConfigKeys.UnityCloudBuildApiKey];

            string mainURL = kvDict[AppConfigKeys.UnityCloudBuildMainURL];
            mainURL = mainURL.Replace(AppConfigKeys.UnityOrgId, kvDict[AppConfigKeys.UnityOrgId]);
            mainURL = mainURL.Replace(AppConfigKeys.UnityProjectId, kvDict[AppConfigKeys.UnityProjectId]);
            mainURL = mainURL.Replace(AppConfigKeys.UnityTargetId, target);

            mainURL += requestSuffix;

            string authString = System.Convert.ToBase64String(Encoding.ASCII.GetBytes(apiKey + ":"));

            ClientWebService webService = new ClientWebService();

            SecurityData security = new SecurityData()
            {
                BasicAuthToken = authString,
            };

            ResponseEnvelope<string> responseEnvelope = await webService.SendRawWebRequest<string>(mainURL, method, requestData, security);

            return responseEnvelope.ResponseData;
        }

        private static async Awaitable CloudBuildWithEnvVars(Dictionary<string, string> envVars)
        {
            await Awaitable.MainThreadAsync();

            string target = envVars[ClientBuildVars.GAME_MODE].ToLower();

            await SendDevOpsRequest(AppConfigKeys.UnityCloudEnvVarsSuffix, HttpMethod.Put, target, envVars);

            CloudBuildArgs buildArgs = new CloudBuildArgs()
            {
                clean = false,
                delay = 0,
            };

            await SendDevOpsRequest(AppConfigKeys.UnityCloudBuildSuffix, HttpMethod.Post, target, buildArgs);
        }



        public static Dictionary<string, string> SetupEnvironmentVariableDictionary(string env, string gameModeStr, string platformName,
            bool selfContainedClient, bool exportGameData, bool encryptExportedData, bool developmentBuild)
        {

            Dictionary<string, string> retval = new Dictionary<string, string>();

            List<PlatformBuildData> buildDataList = BuildConfiguration.GetbuildConfigs();

            PlatformBuildData buildData = buildDataList.FirstOrDefault(x => x.ClientPlatform == platformName);

            // Feel free to change this to something else you use in your jenkins or whatever build.
            Dictionary<string, string> kvDict = XmlUtils.ExtractAppConfigData(ConfigConstants.MainAppConfigPath);

            SetDictionaryValue(ClientBuildVars.ENV, env, retval);
            SetDictionaryValue(ClientBuildVars.SELF_CONTAINED_CLIENT, selfContainedClient.ToString(), retval);
            SetDictionaryValue(ClientBuildVars.EXPORT_GAME_DATA, exportGameData.ToString(), retval);
            SetDictionaryValue(ClientBuildVars.ENCRYPT_EXPORTED_DATA, encryptExportedData.ToString(), retval);
            SetDictionaryValue(ClientBuildVars.GAME_MODE, gameModeStr, retval);
            SetDictionaryValue(ClientBuildVars.WEB_SERVER_URL, kvDict[AppConfigKeys.WebServerURL], retval);
            SetDictionaryValue(ClientBuildVars.CONTENT_ROOT, kvDict[AppConfigKeys.ContentRoot], retval);
            SetDictionaryValue(ClientBuildVars.IOS_SECRET, kvDict[AppConfigKeys.IOSSecret], retval);
            SetDictionaryValue(ClientBuildVars.GOOGLE_SECRET, kvDict[AppConfigKeys.GooglePlaySecret], retval);
            SetDictionaryValue(ClientBuildVars.PACKAGE_NAME, kvDict[AppConfigKeys.PackageName], retval);
            SetDictionaryValue(ClientBuildVars.WORLDS_ENV, GetEnvName(kvDict[EDataCategories.Worlds.ToString() + AppConfigKeys.EnvSuffix], env), retval);
            SetDictionaryValue(ClientBuildVars.ASSETS_ENV, GetEnvName(kvDict[EDataCategories.Assets.ToString() + AppConfigKeys.EnvSuffix], env), retval);

            ClientConfig config = ScriptableObjectUtils.LoadDefault<ClientConfig>();
            SetDictionaryValue(ClientBuildVars.OLD_GAME_MODE, config.GameMode.ToString(), retval);
            SetDictionaryValue(ClientBuildVars.OLD_ENV, config.Env, retval);
            SetDictionaryValue(ClientBuildVars.OLD_SELF_CONTAINED_CLIENT, config.SelfContainedClient.ToString(), retval);
            SetDictionaryValue(ClientBuildVars.OLD_EXPORT_GAME_DATA, config.ExportGameData.ToString(), retval);
            SetDictionaryValue(ClientBuildVars.OLD_ENCRYPT_EXPORTED_DATA, config.EncryptExportedData.ToString(), retval);
            SetDictionaryValue(ClientBuildVars.IS_DEVELOPMENT_BUILD, developmentBuild.ToString(), retval);
            SetDictionaryValue(ClientBuildVars.NAMED_BUILD_TARGET, buildData.NamedTarget.TargetName, retval);
            SetDictionaryValue(ClientBuildVars.BUILD_TARGET, buildData.Target.ToString(), retval);
            SetDictionaryValue(ClientBuildVars.CLIENT_PLATFORM, buildData.ClientPlatform, retval);
            SetDictionaryValue(ClientBuildVars.APPLICATION_SUFFIX, buildData.ApplicationSuffix, retval);
            SetDictionaryValue(ClientBuildVars.BUNDLE_OUTPUT_PATH, buildData.GetBundleOutputPath(), retval);

            BuildOptions options = BuildOptions.CompressWithLz4HC;

            if (developmentBuild)
            {
                options |= BuildOptions.Development;
            }

            SetDictionaryValue(ClientBuildVars.UNITY_BUILD_OPTIONS, options.ToString(), retval);


            string lowerEnv = env;

            string lowerGameModeStr = gameModeStr.ToLower();
            string dataPath = Application.dataPath;
            string streamingAssetsPath = Application.streamingAssetsPath;

            string platformString = buildData.ClientPlatform;
            string appsuffix = buildData.ApplicationSuffix;
            string outputFilesFolder = "../../../Build/" + lowerGameModeStr + "/" + platformString + "/" + lowerEnv + "/";
            string outputPath = outputFilesFolder + lowerGameModeStr + appsuffix;

            SetDictionaryValue(ClientBuildVars.UNITY_OUTPUT_BUILD_PATH, outputPath, retval);

            return retval;
        }

        private static string GetEnvName(string envName, string defaultEnvName)
        {
            if (string.IsNullOrEmpty(envName) || envName == AppConfigKeys.Default)
            {
                return defaultEnvName;
            }

            if (envName == EnvNames.Local)
            {
                envName = EnvNames.Dev.ToLower();
            }
            return envName;
        }


        private static void SetBuildEnvironmentVariables(Dictionary<string, string> envVars)
        {
            foreach (string key in envVars.Keys)
            {
                SetVar(key, envVars[key]);
            }
        }

        private static void ClearBuildEnvironmentVariables()
        {
            List<KeyValue> envVarNames = ConstantUtils.GetStringConstants(typeof(ClientBuildVars));

            foreach (KeyValue kv in envVarNames)
            {
                SetVar(kv.Val, null);
            }
        }

        private static void SetDictionaryValue(string key, string val, Dictionary<string, string> dict)
        {
            dict.Add(key, val);
        }

        public static async Awaitable PreExport()
        {
            Debug.Log("PreExport 1");
            string env = GetVar(ClientBuildVars.ENV);

            if (string.IsNullOrEmpty(env))
            {
                string comment = GetVar(ClientBuildVars.UNITY_BUILD_COMMENT);

                Debug.Log("Comment: " + comment);
                if (!String.IsNullOrEmpty(comment))
                {
                    try
                    {
                        Dictionary<string, string> envVars = JsonConvert.DeserializeObject<Dictionary<string, string>>(comment);

                        if (envVars.Count > 0)
                        {
                            SetBuildEnvironmentVariables(envVars);
                        }
                    }
                    catch (Exception ee)
                    {
                        Debug.Log(ee.Message + " Build Comment was not env values");
                    }
                }

                env = GetVar(ClientBuildVars.ENV);
                if (string.IsNullOrEmpty(env))
                {
                    Debug.Log("PreExport: NeedInit");
                    SetupBuildFromConfig();
                }
            }

            string gameModeStr = GetVar(ClientBuildVars.GAME_MODE);
            string lowerGameModeStr = gameModeStr.ToLower();
            bool selfContainedClient = bool.Parse(GetVar(ClientBuildVars.SELF_CONTAINED_CLIENT));
            ClientConfig config = ScriptableObjectUtils.LoadDefault<ClientConfig>();

            config.Env = GetVar(ClientBuildVars.ENV);
            config.SelfContainedClient = bool.Parse(GetVar(ClientBuildVars.SELF_CONTAINED_CLIENT));
            config.GameMode = (EGameModes)Enum.Parse(typeof(EGameModes), GetVar(ClientBuildVars.GAME_MODE));
            config.BaseWebEndpoint = GetVar(ClientBuildVars.WEB_SERVER_URL);
            config.ContentEndpoint = GetVar(ClientBuildVars.CONTENT_ROOT);

            string packageName = GetVar(ClientBuildVars.PACKAGE_NAME);

            packageName = packageName.Replace(AppConfigKeys.PlaceholderString, GetVar(ClientBuildVars.GAME_MODE).ToLower());

            NamedBuildTarget namedTarget = NamedBuildTarget.Unknown;
            string namedBuildTarget = GetVar(ClientBuildVars.NAMED_BUILD_TARGET);
            if (Enum.TryParse(namedBuildTarget, true, out BuildTargetGroup group))
            {
                namedTarget = NamedBuildTarget.FromBuildTargetGroup(group);
            }

            string streamingAssetsPath = Application.streamingAssetsPath;
            if (Directory.Exists(streamingAssetsPath))
            {
                Directory.Delete(streamingAssetsPath, true);
            }


            PlayerSettings.SetApplicationIdentifier(namedTarget, packageName);

            config.WorldsEnv = GetVar(ClientBuildVars.WORLDS_ENV);
            config.IOSSecret = GetVar(ClientBuildVars.IOS_SECRET);
            config.GooglePlaySecret = GetVar(ClientBuildVars.GOOGLE_SECRET);
            config.AssetsEnv = GetVar(ClientBuildVars.ASSETS_ENV);
            EditorUtility.SetDirty(config);

            string clientPlatform = GetVar(ClientBuildVars.CLIENT_PLATFORM);

            BundleVersions currentBundleVersions = await CreateAssetBundles.CreateBundles(clientPlatform, gameModeStr, env, true,
                true);

            string bundleOutputPath = GetVar(ClientBuildVars.BUNDLE_OUTPUT_PATH);
            string[] files = Directory.GetFiles(bundleOutputPath);

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
                    if (!Directory.Exists(streamingAssetsPath))
                    {
                        Directory.CreateDirectory(streamingAssetsPath);
                    }
                    File.Copy(origFilename, streamingAssetsPath + "/" + newFilename, true);
                }
            }

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(i));
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }

        public static async Awaitable LocalBuildPlayer()
        {
            await Task.CompletedTask;
            Debug.Log("LocalBuild 1");
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

            for (int s = 0; s < EditorBuildSettings.scenes.Length; s++)
            {
                scenes.Add(EditorBuildSettings.scenes[s]);
            }

            string[] sceneArray = scenes.Select(x => x.path).ToArray();

            foreach (EditorBuildSettingsScene scene in scenes)
            {
                Debug.Log("LocalBuild AllScenePath: " + scene.path);
            }

            foreach (string sceneName in sceneArray)
            {
                Debug.Log("LocalBuild Scene: " + sceneName);
            }
            if (Enum.TryParse<BuildTarget>(GetVar(ClientBuildVars.BUILD_TARGET), out BuildTarget targ))
            {

                BuildOptions buildOptions = (BuildOptions)Enum.Parse(typeof(BuildOptions), GetVar(ClientBuildVars.UNITY_BUILD_OPTIONS));
                BuildReport report = BuildPipeline.BuildPlayer(sceneArray, GetVar(ClientBuildVars.UNITY_OUTPUT_BUILD_PATH), targ, buildOptions);
                Debug.Log("ErrorSummary: " + report.SummarizeErrors());
            }
            else
            {
                Debug.LogError("Failed to find BuildTarget: " + GetVar(ClientBuildVars.BUILD_TARGET));
            }

            Debug.Log("LocalBuild 3");
        }


        public static async Awaitable PostExport()
        {

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(i));
            }

            string env = GetVar(ClientBuildVars.ENV);
            string gameModeStr = GetVar(ClientBuildVars.GAME_MODE);
            string lowerGameModeStr = gameModeStr.ToLower();
            string clientPlatform = GetVar(ClientBuildVars.CLIENT_PLATFORM);
            bool selfContainedClient = bool.Parse(GetVar(ClientBuildVars.SELF_CONTAINED_CLIENT));

            ClientConfig config = ScriptableObjectUtils.LoadDefault<ClientConfig>();

            config.Env = GetVar(ClientBuildVars.OLD_ENV);
            config.SelfContainedClient = bool.Parse(GetVar(ClientBuildVars.OLD_SELF_CONTAINED_CLIENT));
            config.ExportGameData = bool.Parse(GetVar(ClientBuildVars.OLD_EXPORT_GAME_DATA));
            config.EncryptExportedData = bool.Parse(GetVar(ClientBuildVars.OLD_ENCRYPT_EXPORTED_DATA));
            config.GameMode = (EGameModes)Enum.Parse(typeof(EGameModes), GetVar(ClientBuildVars.OLD_GAME_MODE));
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

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

            string outputZipFolder = "../../../Build/" + lowerGameModeStr + "/zips/";
            if (!Directory.Exists(outputZipFolder))
            {
                Directory.CreateDirectory(outputZipFolder);
            }

            string lowerEnv = GetVar(ClientBuildVars.ENV);

            string dataPath = Application.dataPath;
            string streamingAssetsPath = Application.streamingAssetsPath;

            string platformString = clientPlatform;
            string outputFilesFolder = "../../../Build/" + lowerGameModeStr + "/" + platformString + "/" + lowerEnv + "/";

            string localFolderPath = dataPath + "/" + outputFilesFolder;

            if (!Directory.Exists(outputFilesFolder))
            {
                Directory.CreateDirectory(outputFilesFolder);
            }
            string versionFilePath = outputZipFolder + PatcherUtils.GetPatchVersionFilename();
            File.WriteAllText(versionFilePath, String.Empty);
            File.WriteAllText(versionFilePath, version.ToString());
            string localVersionPath = dataPath + "/../" + versionFilePath;

            Debug.Log("Version: " + version);

            Debug.Log($"Finished building E: {env} G: {gameModeStr} P: {platformString} SC: {selfContainedClient}");

            await Task.CompletedTask;
        }

        private static string GetVar(string key)
        {
            Debug.Log("GetEnvVar: " + key + " Val: " + System.Environment.GetEnvironmentVariable(key));
            return Environment.GetEnvironmentVariable(key);
        }

        private static void SetVar(string key, string val)
        {
            System.Environment.SetEnvironmentVariable(key, val);
        }

        private static void SetupBuildFromConfig()
        {
            ClearBuildEnvironmentVariables();

            ClientConfig config = ScriptableObjectUtils.LoadDefault<ClientConfig>();

            string env = config.Env;
            string assetEnv = config.AssetsEnv ?? env;
            string worldsEnv = config.WorldsEnv ?? env;
            string gameModeStr = config.GameMode.ToString();
            bool selfContainedClient = config.SelfContainedClient;
            bool exportGameData = config.ExportGameData;
            bool encryptExportedData = config.EncryptExportedData;
            string platformName = ClientPlatformNames.Win;

            Dictionary<string, string> dict = SetupEnvironmentVariableDictionary(env, gameModeStr, platformName, selfContainedClient,
                exportGameData, encryptExportedData, false);

            dict[ClientBuildVars.ASSETS_ENV] = assetEnv;
            dict[ClientBuildVars.WORLDS_ENV] = worldsEnv;

            SetBuildEnvironmentVariables(dict);
        }
    }
}


