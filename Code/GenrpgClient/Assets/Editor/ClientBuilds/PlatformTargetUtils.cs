using System.IO;
using UnityEditor;

namespace Assets.Editor.ClientBuilds
{
    public static class PlatformTargetUtils
    {
        public static void DeepCleanAndSwitch(BuildTarget target, BuildTargetGroup group)
        {
            // 1. Switch the platform first so Unity updates its target settings
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);

            // 2. Target specific internal subdirectories of the Library folder
            string projectRoot = Path.GetDirectoryName(UnityEngine.Application.dataPath);
            string libraryPath = Path.Combine(projectRoot, "Library");

            string[] directoriesToNuke = new string[]
            {
                Path.Combine(libraryPath, "Artifacts"),
                Path.Combine(libraryPath, "ShaderCache"),
                Path.Combine(libraryPath, "ScriptCompilation"),
                Path.Combine(libraryPath, "ScriptAssemblies")
            };

            foreach (string dir in directoriesToNuke)
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                    UnityEngine.Debug.Log($"Cleared corruptible build artifact cache: {dir}");
                }
            }

            // 3. Force Unity to notice the missing internal data and rebuild the pipeline cleanly
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
    }
}