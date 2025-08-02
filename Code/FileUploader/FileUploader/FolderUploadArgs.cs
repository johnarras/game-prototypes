using System.Collections.Generic;

public class FolderUploadArgs
{
    public string GamePrefix;
    public string Env;
    public string LocalFolder;
    public string RemoteSubfolder;
    public bool IsWorldData; 
    public List<string> OverwriteIfExistsFiles = new List<string>();
}