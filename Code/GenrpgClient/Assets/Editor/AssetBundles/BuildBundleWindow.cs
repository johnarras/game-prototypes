using Assets.Scripts.Assets.Bundles;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.Environments.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.Builds
{
    public class BuildBundleWindow : EditorWindow
    {
        [MenuItem("Tools/BuildBundles")]
        public static void ShowWindow()
        {
            BuildBundleWindow window = GetWindow<BuildBundleWindow>("Build Bundles");
            int xsize = 250;
            int zsize = xsize;

            window.minSize = new Vector2(xsize, zsize);
            window.maxSize = new Vector2(xsize, zsize);
        }

        private string[] _platformNames = null;
        private int _selectedPlatform = 0;

        private List<PlatformBuildData> _platformData = null;

        private string _env = EnvNames.Dev;

        private string[] _gameModes = Enum.GetNames(typeof(EGameModes));
        private int _selectedGameMode = 0;
        private bool _uploadBundles = true;

        private BundleList _bundleList = null;


        private bool _isBuilding = false;

        private void OnGUI()
        {
            GUILayout.Label("Build Options:");

            if (_bundleList == null)
            {
                _bundleList = Resources.Load<BundleList>(ScriptableObjectUtils.ConfigResourcesFolder + "BundleList");
            }

            GUILayout.Label("BundleList sets Local vs. Remote");

            _bundleList = (BundleList)EditorGUILayout.ObjectField("Bundle List",
                _bundleList, typeof(BundleList), false);

            if (_platformNames == null)
            {
                _platformData = BuildConfiguration.GetbuildConfigs();
                _platformNames = _platformData.Select(x => x.ClientPlatform).ToArray();
            }


            _selectedPlatform = EditorGUILayout.Popup("Select Platform: ", _selectedPlatform, _platformNames);

            _env = EditorGUILayout.TextField("Env: ", _env);

            _selectedGameMode = EditorGUILayout.Popup("Select Game:", _selectedGameMode, _gameModes);

            _uploadBundles = EditorGUILayout.Toggle("Upload Bundles:", _uploadBundles);

            if (GUILayout.Button("Build Bundles"))
            {
                if (!string.IsNullOrEmpty(_env))
                {
                    _ = BuildBundlesInternal();
                }
            }

        }

        private async Awaitable BuildBundlesInternal()
        {
            if (_isBuilding)
            {
                return;
            }
            _isBuilding = true;
            await CreateAssetBundles.CreateBundles(_platformNames[_selectedPlatform], _gameModes[_selectedGameMode], _env, true, _uploadBundles);

            EditorApplication.delayCall += () =>
            {
                _isBuilding = false;
                Repaint();
            };

        }
    }
}


