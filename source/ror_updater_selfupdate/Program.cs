using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace ror_updater_selfupdate
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(100); //Sleep a bit before doing anything
            if(File.Exists("ror-updater_new.exe"))
            {
                //File.Delete("ror-updater.exe");
                File.Replace("ror-updater_new.exe", "ror-updater.exe", "ror-updater.bak"); 
            }
            System.Diagnostics.Process.Start("ror-updater.exe");
        }
    }
}
