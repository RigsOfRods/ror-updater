// This file is part of ror-updater
// 
// Copyright (c) 2016 AnotherFoxGuy
// 
// ror-updater is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 3, as
// published by the Free Software Foundation.
// 
// ror-updater is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ror-updater. If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using IniParser;
using IniParser.Model;
using Microsoft.HockeyApp;
using Newtonsoft.Json;

namespace ror_updater
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IniData IniSettingsData;

        public FileIniDataParser IniDataParser;

        public static string StrServerUrl = "http://update.rigsofrods.org/";
        public static List<RoRUpdaterItem> FilesInfo;

        public static UpdateChoise Choise;
        private bool _bInit;
        private bool _bSkipUpdates;

        private BackgroundWorker _initDialog = new BackgroundWorker();

        private string _jsonInfoFile;

        private PageSwitcher _pageSwitcher;

        private StartupForm _sForm;

        public static bool BDevBuilds;

        public string StrLocalVersion;
        public string StrOnlineVersion = "0";

        public string StrUpdaterVersion;
        public string StrUpdaterOnlineVersion;
        public string DownloadLink;

        public WebClient WebClient;


        public void InitApp(object sender, StartupEventArgs e)
        {
           
            HockeyClient.Current.Configure("b1a60e07ff0e462a927012fdb07f1c72");

            //Clean up first
            File.Delete("Updater_log.txt");
            File.Delete("updater.exe"); //We don't need this anymore

            //Show something so users don't get confused
            _initDialog.DoWork += InitDialog_DoWork;
            _initDialog.RunWorkerAsync();


            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            StrUpdaterVersion = fileVersionInfo.ProductVersion;
            Utils.LOG("Info| Updater version: " + StrUpdaterVersion);

            Utils.LOG("Info| Creating Web Handler");
            WebClient = new WebClient();
            Utils.LOG("Info| Done.");

            Utils.LOG("Info| Creating INI handler");

            //Proceed
            IniDataParser = new FileIniDataParser();
            IniDataParser.Parser.Configuration.CommentString = "#";

            //Dirty code incoming!
            try
            {
                IniSettingsData = IniDataParser.ReadFile("./updater.ini", Encoding.ASCII);
            }
            catch (Exception ex)
            {
                Utils.ProcessBadConfig(ex);
            }

            Thread.Sleep(100); //Wait a bit

            try
            {
                BDevBuilds = bool.Parse(IniSettingsData["Main"]["DevBuilds"]);
                _bSkipUpdates = bool.Parse(IniSettingsData["Dev"]["SkipUpdates"]);
            }
            catch (Exception ex)
            {
                Utils.ProcessBadConfig(ex);
            }

            Utils.LOG("Info| Done.");
            Utils.LOG("Info| DevBuilds: " + BDevBuilds + " Skip_updates: " + _bSkipUpdates);

            //Get app version
            MessageBoxResult result;

            //Download list
            Utils.LOG("Info| Downloading main list from server: " + StrServerUrl + "update-list.php?DevBuilds=" + BDevBuilds);

            try
            {
                _jsonInfoFile = WebClient.DownloadString(StrServerUrl + "update-list.php?DevBuilds=" + BDevBuilds);

                var jsonVersionInfo = WebClient.DownloadString(StrServerUrl + "/versions.php");
                var versionInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonVersionInfo);

                if (!versionInfo.TryGetValue("Updater", out StrUpdaterOnlineVersion) ||
                    !versionInfo.TryGetValue("Updater-DL", out DownloadLink))              
                                      throw new ApplicationException("Failed to get Versioninfo.");

                if (BDevBuilds)
                {
                    if(!versionInfo.TryGetValue("Rigs-of-Rods", out StrOnlineVersion))
                    throw new ApplicationException("Failed to get Versioninfo.");
                }
                else
                {
                    if (!versionInfo.TryGetValue("Rigs-of-Rods-Dev", out StrOnlineVersion))
                    throw new ApplicationException("Failed to get Versioninfo.");
                }

                Utils.LOG("Info| Updater: " + StrUpdaterOnlineVersion);
                Utils.LOG("Info| Rigs-of-Rods: " + StrOnlineVersion);
                Utils.LOG("Info| Done.");
            }
            catch (Exception ex)
            {
                result = MessageBox.Show("Could not connect to the main server.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    Utils.LOG("Error| Failed to connect to server.");
                    Utils.LOG(ex.ToString());
                    Quit();
                }
            }

            

            if (StrUpdaterVersion != StrUpdaterOnlineVersion && !_bSkipUpdates)
                ProcessSelfUpdate();

            Thread.Sleep(10); //Wait a bit

            Utils.LOG("Info| Reading file...");

            try
            {
                FilesInfo = JsonConvert.DeserializeObject<List<RoRUpdaterItem>>(_jsonInfoFile);
            }
            catch (Exception ex)
            {
                result = MessageBox.Show("Could not read file.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    Utils.LOG("Error| Failed to read file.");
                    Utils.LOG(ex.ToString());
                    Quit();
                }
            }
            try
            {
                //Use Product version instead of file version because we can use it to separate Dev version from release versions, same for TestBuilds
                var versionInfo = FileVersionInfo.GetVersionInfo("RoR.exe");
                StrLocalVersion = versionInfo.ProductVersion;

                Utils.LOG("Info| local RoR ver: " + StrLocalVersion);
            }
            catch
            {
                StrLocalVersion = "unknown";

                Utils.LOG("Info| Game Not found!");
            }

            Utils.LOG("Info| Done.");
            Utils.LOG("Succes| Initialization done!");

            _bInit = true;

            _initDialog = null; //We don't need it anymore.. :3

            _pageSwitcher = new PageSwitcher(this);
            _pageSwitcher.Show();
            _pageSwitcher.Activate();
        }

        private void ProcessSelfUpdate()
        {
            var result = MessageBox.Show("New version available.", "Update", MessageBoxButton.OK,
                MessageBoxImage.Information);
            if (result == MessageBoxResult.OK)
            {
                Utils.LOG("Update| New version available: " + StrUpdaterOnlineVersion);

                try
                {
                    Process.Start(DownloadLink);
                }
                catch 
                {                 
                }

                Quit();
            }
        }

        public static void Quit()
        {
            Process.GetCurrentProcess().Kill();
        }

        private void InitDialog_DoWork(object sender, DoWorkEventArgs e)
        {
            // Very dirty way to do this. :/
            _sForm = new StartupForm();
            _sForm.Show();

            
            while (!_bInit)
            {
                //meh?
                Thread.Sleep(500);
                
                _sForm.Update();
            }

            _sForm.Hide();
            _sForm = null;
        }
    }

    public class RoRUpdaterItem
    {
        public string directory;
        public string fileHash;
        public string fileName;
        public int id;
    }

    public enum UpdateChoise
    {
        INSTALL,
        UPDATE,
        REPAIR
    }
}