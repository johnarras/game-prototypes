using Assets.Scripts.Logalytics.Utils;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.Environments.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.Builds
{
    public class BuildClientWindow : EditorWindow
    {
        [MenuItem("Tools/BuildClients")]
        public static void ShowWindow()
        {
            BuildClientWindow window = GetWindow<BuildClientWindow>("Build Clients");
            int xsize = 250;
            int ysize = xsize;

            window.minSize = new Vector2(xsize, ysize);
            window.maxSize = new Vector2(xsize, ysize);
        }

        private string[] _envNames = new string[] { EnvNames.Local, EnvNames.Dev, EnvNames.Test, EnvNames.Prod };
        private int _selectedEnv = 0;

        private string[] _gameModes = Enum.GetNames(typeof(EGameModes));
        private int _selectedGameMode = 0;

        private string[] _platformNames = null;
        private int _selectedPlatform = 0;

        private List<PlatformBuildData> _platformData = null;

        private bool _selfContainedClient = true;
        private bool _developmentBuild = false;
        private bool _exportGameData = false;
        private bool _encryptExportedData = false;

        private bool _isBuilding = false;

        private void OnGUI()
        {
            GUILayout.Label("Build Options:");

            if (_platformNames == null)
            {
                _platformData = BuildConfiguration.GetbuildConfigs();
                _platformNames = _platformData.Select(x => x.ClientPlatform).ToArray();
            }

            _selectedEnv = EditorGUILayout.Popup("Select Env:", _selectedEnv, _envNames);

            _selectedGameMode = EditorGUILayout.Popup("Select Game:", _selectedGameMode, _gameModes);

            _selectedPlatform = EditorGUILayout.Popup("Select Platform: ", _selectedPlatform, _platformNames);

            _selfContainedClient = EditorGUILayout.Toggle("Self-Contained:", _selfContainedClient);

            _exportGameData = EditorGUILayout.Toggle("Export Game Data:", _exportGameData);

            _encryptExportedData = EditorGUILayout.Toggle("Encrypt Game Data:", _encryptExportedData);

            GUILayout.Label("-------------------");

            _developmentBuild = EditorGUILayout.Toggle("Development Build:", _developmentBuild);

            if (GUILayout.Button("Build In Editor"))
            {
                _ = BuildWithArgs(false);
            }

            if (GUILayout.Button("Cloud Build"))
            {
                _ = BuildWithArgs(true);
            }
        }

        private async Awaitable BuildWithArgs(bool cloudBuild)
        {
            if (_isBuilding)
            {
                return;
            }

            string logalyticsConnectionString = LogalyticsUtils.GetLogConnectionString(ScriptableObjectUtils.LoadDefault<ClientConfig>());

            _isBuilding = true;
            await RunBuilds.PlayerBuilder.BuildWithArgs(
                    _envNames[_selectedEnv],
                    _gameModes[_selectedGameMode],
                    _platformNames[_selectedPlatform],
                    _selfContainedClient,
                    _exportGameData,
                    _encryptExportedData,
                    cloudBuild,
                    _developmentBuild,
                    logalyticsConnectionString);


            EditorApplication.delayCall += () =>
            {
                _isBuilding = false;
                Repaint();
            };
        }
    }
}


