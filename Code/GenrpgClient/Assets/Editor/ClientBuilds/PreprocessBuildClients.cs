
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets.Editor.Builds
{
    public class PreprocessBuildClients : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
        }
    }
}
