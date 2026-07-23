namespace OxDb.Client.Repository.Constants
{
    public static class ClientRepositoryConstants
    {
        // Used to allow editor + player builds using same data folder.
        // If you need multiple copies of unity running locally, you could 
        // change this per client install. No configuration for this at
        // this point since that's really an edge case.
        // Or you could use this as a directory prefix to split
        // the data per install.
        private const string EditorPathPrefix = "Editor";

        private const string DataFolderPathPrefix = "Data";

        public const string WorldPathPrefix = "/World";

        private static string GetEditorPathPrefix()
        {
#if UNITY_EDITOR
            return EditorPathPrefix;
#else
            return "";
#endif
        }

        public static string GetDataPathPrefix()
        {
            return "/" + GetEditorPathPrefix() + DataFolderPathPrefix;
        }
    }
}


