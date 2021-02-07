using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

var tmp = $"{Path.GetTempPath()}/ror-updater";
var dest = Directory.GetCurrentDirectory();
var zipPath = $"{Path.GetTempPath()}/patch.zip";

try
{
    Console.WriteLine("Waiting for update");
    Thread.Sleep(1000); //Sleep a bit before doing anything
    Console.WriteLine("Starting selfpatch...");
    if (File.Exists(zipPath))
    {
        if (Directory.Exists(tmp))
            Directory.Delete(tmp, true);

        Directory.CreateDirectory(tmp);
        Console.WriteLine("Extracting zip");
        ZipFile.ExtractToDirectory(zipPath, tmp);

        //Now Create all of the directories
        var dirs = Directory.GetDirectories(tmp, "*", SearchOption.AllDirectories);
        foreach (var dirPath in dirs)
        {
            Directory.CreateDirectory(dirPath.Replace(tmp, dest));
        }
    
        //Copy all the files & Replaces any files with the same name
        var files = Directory.GetFiles(tmp, "*.*", SearchOption.AllDirectories);
        foreach (var newPath in files)
        {
            var filename = newPath.Replace(tmp, dest);
            Console.WriteLine($"Copying {filename}");
            File.Copy(newPath, filename, true);
        }

        Console.WriteLine("Done");
    }
    
    Process.Start($"{dest}/ror-updater.exe");
    
    Thread.Sleep(2500); //Sleep a bit before doing anything
}
catch(Exception exception)
{
    File.WriteAllText($"{dest}/selfpatch.log", $"{exception.Message}\n{exception.StackTrace}");
}