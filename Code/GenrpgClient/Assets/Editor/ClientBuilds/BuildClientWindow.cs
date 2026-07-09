using Assets.Scripts.Logalytics.Utils;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.Utils;
using RunBuilds;
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
            int zsize = 400;

            window.minSize = new Vector2(xsize, zsize);
            window.maxSize = new Vector2(xsize, zsize);
        }

        private string[] _envNames = new string[] { EnvNames.Local, EnvNames.Dev, EnvNames.Test, EnvNames.Prod };
        private int _selectedEnv = 0;

        private string[] _gameModes = Enum.GetNames(typeof(EGameModes));
        private int _selectedGameMode = 0;

        private string[] _platformNames = null;
        private int _selectedPlatform = 0;

        private List<PlatformBuildData> _platformData = null;

        private ClientPlayerFlags _flags = ClientPlayerFlags.None;


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

            EditorGUILayout.Space();

            foreach (ClientPlayerFlags flag in Enum.GetValues(typeof(ClientPlayerFlags)))
            {
                if (flag != ClientPlayerFlags.None)
                {
                    DrawBitToggle(flag);
                }
            }
            GUILayout.Label("=================");
            EditorGUILayout.Space();
            if (GUILayout.Button("Build Player"))
            {
                _ = BuildWithArgs();
            }

            EditorGUILayout.Space();
            GUILayout.Label("=================");
        }

        private void DrawBitToggle(ClientPlayerFlags flag)
        {
            bool newValue = EditorGUILayout.Toggle(StrUtils.SplitOnCapitalLetters(flag.ToString()), _flags.HasFlag(flag));

            if (newValue)
            {
                _flags |= flag;
            }
            else
            {
                _flags &= ~flag;
            }

        }

        private async Awaitable BuildWithArgs()
        {
            if (_isBuilding)
            {
                return;
            }
            _isBuilding = true;

            Debug.Log("Start Build With Args");
            string logalyticsConnectionString = LogalyticsUtils.GetLogConnectionString(ScriptableObjectUtils.LoadDefault<ClientConfig>());

            BuildPlayerArgs args = new BuildPlayerArgs()
            {
                Env = _envNames[_selectedEnv],
                GameModeStr = _gameModes[_selectedGameMode],
                PlatformName = _platformNames[_selectedPlatform],
                Flags = _flags,
                LogalyticsConnectionString = logalyticsConnectionString,
            };

            await PlayerBuilder.BuildWithArgs(args);

            EditorApplication.delayCall += () =>
            {
                _isBuilding = false;
                Repaint();
            };
        }
    }
}


