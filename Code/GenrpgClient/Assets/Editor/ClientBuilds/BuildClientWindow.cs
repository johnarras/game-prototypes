using Genrpg.Shared.Constants;
using Genrpg.Shared.Core.Constants;
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
        private bool _rebuildBundles = true;

        private void OnGUI()
        {
            GUILayout.Label("Build Options:");

            if (_platformNames == null)
            {
                _platformData = BuildConfiguration.GetbuildConfigs(null);
                _platformNames = _platformData.Select(x => x.ClientPlatform).ToArray();
            }

            _selectedEnv = EditorGUILayout.Popup("Select Env:", _selectedEnv, _envNames);

            _selectedGameMode = EditorGUILayout.Popup("Select Game:", _selectedGameMode, _gameModes);

            _selectedPlatform = EditorGUILayout.Popup("Select Platform: ", _selectedPlatform, _platformNames);

            _selfContainedClient = EditorGUILayout.Toggle("Self-Contained:", _selfContainedClient);

            _rebuildBundles = EditorGUILayout.Toggle("Rebuild Bundles:", _rebuildBundles);

            if (GUILayout.Button("Build Clients"))
            {
                BuildClients.BuildClient(
                    _envNames[_selectedEnv],
                    _gameModes[_selectedGameMode],
                    _platformNames[_selectedPlatform],
                    _selfContainedClient,
                    _rebuildBundles);
            }
        }
    }
}
