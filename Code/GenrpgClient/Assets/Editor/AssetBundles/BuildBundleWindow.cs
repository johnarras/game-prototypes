using Assets.Scripts.Assets.Bundles;
using Genrpg.Shared.Constants;
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
            int ysize = xsize;

            window.minSize = new Vector2(xsize, ysize);
            window.maxSize = new Vector2(xsize, ysize);
        }

        private string[] _platformNames = null;
        private int _selectedPlatform = 0;

        private List<PlatformBuildData> _platformData = null;

        private string _env = EnvNames.Dev;

        private bool _uploadBundles = true;

        private BundleList _bundleList = null;

        private void OnGUI()
        {
            GUILayout.Label("Build Options:");

            if (_platformNames == null)
            {
                _platformData = BuildConfiguration.GetbuildConfigs(null);
                _platformNames = _platformData.Select(x => x.ClientPlatform).ToArray();
            }

            _selectedPlatform = EditorGUILayout.Popup("Select Platform: ", _selectedPlatform, _platformNames);
            _env = EditorGUILayout.TextField("Env: ", _env);

            _uploadBundles = EditorGUILayout.Toggle("Upload Bundles:", _uploadBundles);

            if (GUILayout.Button("Build Bundles"))
            {
                if (!string.IsNullOrEmpty(_env))
                {
                    CreateAssetBundles.CreateBundles(_platformNames[_selectedPlatform], _env, true, _uploadBundles);
                }
            }

            if (_bundleList == null)
            {
                _bundleList = Resources.Load<BundleList>("Config/BundleList");
            }

            GUILayout.Label("BundleList sets Local vs. Remote");

            _bundleList = (BundleList)EditorGUILayout.ObjectField("Bundle List",
                _bundleList, typeof(BundleList), false);
        }
    }
}
