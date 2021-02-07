using System.Collections.Generic;

public class ReleaseInfo
{
    public string Version { get; set; }
    public List<PFileInfo> Filelist { get; set; }
}

public class PFileInfo
{
    public string Name { get; set; }
    public string Hash { get; set; }
    public string Directory { get; set; }
}

public class Branch
{
    public string Name { get; set; }
    public string Url { get; set; }
}

public class BranchInfo
{
    public string UpdaterVersion { get; set; }
    public Dictionary<string, Branch> Branches { get; set; }
}