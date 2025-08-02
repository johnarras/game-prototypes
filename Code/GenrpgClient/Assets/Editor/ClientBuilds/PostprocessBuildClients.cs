
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets.Editor.Builds
{
    public class PostprocessBuildClients : UnityEditor.Build.IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
        }
    }
}
