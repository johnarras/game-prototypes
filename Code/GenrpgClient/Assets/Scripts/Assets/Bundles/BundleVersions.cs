using System.Collections.Generic;

public class BundleVersions
{
    public string ClientPlatform { get; set; }
    public BundleUpdateInfo UpdateInfo { get; set; }
    public Dictionary<string, BundleVersion> Versions { get; set; } = new Dictionary<string, BundleVersion>();
}

