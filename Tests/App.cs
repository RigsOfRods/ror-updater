using System;
using System.Collections.Generic;

public class App
{
    public ReleaseInfo ReleaseInfoData = new()
    {
        Filelist = new List<PFileInfo>
        {
            new()
            {
                Name = "file1.txt",
                Directory = ".", 
                Hash = "b68088fa94cc126d0c3371eab844bec5"
            },
            new()
            {
                Name = "file2.txt",
                Directory = ".", 
                Hash = "20fc92f68d957da0717ad7dd53740ebc"
            }
        },
        Version = "1.0"
    };
    
    public Branch SelectedBranch;
    public BranchInfo BranchInfo;
    public string CDNUrl = "http://localhost:8080/test/";

    public void aaa()
    {
        
    }

    #region Singleton

    private static Lazy<App> _lazyApp= new(() => new App());

    public static App Instance => _lazyApp.Value;

    private App()
    {
         
    }

    #endregion
}