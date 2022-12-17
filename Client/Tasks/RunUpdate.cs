using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ror_updater.Tasks;

public class RunUpdate
{
    private readonly WebClient _webClient;

    private readonly CancellationTokenSource _cancel = new();

    private Action<string> _logCallback;
    private IProgress<int> _progress;


    public RunUpdate(Action<string> callback, IProgress<int> progress, WebClient webClient)
    {
        _logCallback = callback;
        _progress = progress;
        _webClient = webClient;
    }


    public async Task InstallGame()
    {
        Utils.LOG(Utils.LogPrefix.INFO, "Installing Game...");

        var i = 0;
        foreach (var file in App.Instance.ReleaseInfoData.Filelist)
        {
            if (_cancel.IsCancellationRequested) break;
            AddToLogFile($"Downloading file: {file.Directory.TrimStart('.')}/{file.Name}");
            await DownloadFile(file.Directory, file.Name);
            _progress?.Report(i++);
        }

        Utils.LOG(Utils.LogPrefix.INFO, "Done.");
    }

    public async Task UpdateGame()
    {
        Utils.LOG(Utils.LogPrefix.INFO, "Updating Game...");

        var filesStatus = new List<FileStatus>();

        AddToLogFile("Checking for outdated files...");
        var i = 0;
        foreach (var file in App.Instance.ReleaseInfoData.Filelist)
        {
            if (_cancel.IsCancellationRequested) break;
            var fileStatus = HashFile(file);
            AddToLogFile($"Checking file: {file.Directory.TrimStart('.')}/{file.Name}");
            filesStatus.Add(new FileStatus { File = file, Status = fileStatus });
            _progress?.Report(i++);
        }

        AddToLogFile("Done, updating outdated files now...");

        i = 0;
        foreach (var item in filesStatus)
        {
            if (_cancel.IsCancellationRequested) break;
            _progress?.Report(i++);

            switch (item.Status)
            {
                case HashResult.UPTODATE:
                    Utils.LOG(Utils.LogPrefix.INFO, $"file up to date: {item.File.Name}");
                    AddToLogFile($"File up to date: {item.File.Directory.TrimStart('.')}/{item.File.Name}");
                    break;
                case HashResult.OUTOFDATE:
                    AddToLogFile($"File out of date: {item.File.Directory.TrimStart('.')}/{item.File.Name}");
                    Utils.LOG(Utils.LogPrefix.INFO, $"File out of date: {item.File.Name}");
                    await DownloadFile(item.File.Directory, item.File.Name);
                    break;
                case HashResult.NOT_FOUND:
                    Utils.LOG(Utils.LogPrefix.INFO, $"File doesnt exits: {item.File.Name}");
                    AddToLogFile(
                        $"Downloading new file: {item.File.Directory.TrimStart('.')}/{item.File.Name}");
                    await DownloadFile(item.File.Directory, item.File.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Utils.LOG(Utils.LogPrefix.INFO, "Done.");
    }

    public void Cancel()
    {
        _cancel.Cancel();
    }

    HashResult HashFile(PFileInfo item)
    {
        string sFileHash = null;
        var filePath = $"{item.Directory}/{item.Name}";

        Utils.LOG(Utils.LogPrefix.INFO, $"Checking file: {item.Name}");

        if (!File.Exists(filePath)) return HashResult.NOT_FOUND;
        sFileHash = Utils.GetFileHash(filePath);
        Utils.LOG(Utils.LogPrefix.INFO,
            $"{item.Name} Hash: Local: {sFileHash.ToLower()} Online: {item.Hash.ToLower()}");
        return sFileHash.ToLower().Equals(item.Hash.ToLower())
            ? HashResult.UPTODATE
            : HashResult.OUTOFDATE;
    }

    private async Task DownloadFile(string dir, string file)
    {
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        Thread.Sleep(100);
        var dest = $"{dir}/{file}";
        var path = dir.Replace(".", "");
        var dlLink = $"{App.Instance.CDNUrl}/{path}/{file}";

        Utils.LOG(Utils.LogPrefix.INFO, $"ULR: {dlLink}");
        Utils.LOG(Utils.LogPrefix.INFO, $"File: {dest}");
        await _webClient.DownloadFileTaskAsync(new Uri(dlLink), dest);
    }

    private void AddToLogFile(string s)
    {
        _logCallback(s);
    }

    private class FileStatus
    {
        public PFileInfo File;
        public HashResult Status;
    }

    private enum HashResult
    {
        UPTODATE,
        OUTOFDATE,
        NOT_FOUND
    }
}