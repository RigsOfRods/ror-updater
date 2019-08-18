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
using IniParser;
using IniParser.Model;
using Newtonsoft.Json;

namespace ror_updater
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string StrServerUrl = "https://a-random-vps.cf/";
        public static List<RoRUpdaterItem> FilesInfo;

        public static UpdateChoice Choice;
        
        private bool _bInit;
        private bool _bSkipUpdates;

        private BackgroundWorker _initDialog = new BackgroundWorker();

        private string _jsonInfoFile;

        private PageSwitcher _pageSwitcher;

        private StartupForm _sForm;
        private string _downloadLink;

        private FileIniDataParser _iniDataParser;
        private IniData _iniSettingsData;

        public string StrLocalVersion;
        public string StrOnlineVersion = "0";
        private string _strUpdaterOnlineVersion;

        private string _strUpdaterVersion;

        private WebClient _webClient;


        public void InitApp(object sender, StartupEventArgs e)
        {
            File.WriteAllText(@"./Updater_log.txt", "");
            
            //Show something so users don't get confused
            _initDialog.DoWork += InitDialog_DoWork;
            _initDialog.RunWorkerAsync();

            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            _strUpdaterVersion = fileVersionInfo.ProductVersion;
            Utils.LOG($"Info| Updater version: {_strUpdaterVersion}");

            Utils.LOG("Info| Creating Web Handler");
            _webClient = new WebClient();
            Utils.LOG("Info| Done.");

            Utils.LOG("Info| Creating INI handler");

            //Proceed
            _iniDataParser = new FileIniDataParser();
            _iniDataParser.Parser.Configuration.CommentString = "#";

            //Dirty code incoming!
            try
            {
                _iniSettingsData = _iniDataParser.ReadFile("./updater.ini", Encoding.ASCII);
            }
            catch (Exception ex)
            {
                Utils.ProcessBadConfig(ex);
            }

            Thread.Sleep(100); //Wait a bit

            try
            {
                _bSkipUpdates = bool.Parse(_iniSettingsData["Dev"]["SkipUpdates"]);
            }
            catch (Exception ex)
            {
                Utils.ProcessBadConfig(ex);
            }

            Utils.LOG("Info| Done.");
            Utils.LOG($"Info| Skip_updates: {_bSkipUpdates}");

            //Get app version
            MessageBoxResult result;

            //Download list
            Utils.LOG($"Info| Downloading main list from server: {StrServerUrl}fileList");

            try
            {
                _jsonInfoFile = _webClient.DownloadString($"{StrServerUrl}fileList");

                var jsonVersionInfo = _webClient.DownloadString($"{StrServerUrl}version");
                var versionInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonVersionInfo);

                if (!versionInfo.TryGetValue("Updater", out _strUpdaterOnlineVersion) ||
                    !versionInfo.TryGetValue("UpdaterDL", out _downloadLink) ||
                    !versionInfo.TryGetValue("RoRVersion", out StrOnlineVersion))
                    throw new ApplicationException("Failed to get Versioninfo.");
                

                Utils.LOG($"Info| Updater: {_strUpdaterOnlineVersion}");
                Utils.LOG($"Info| Rigs-of-Rods: {StrOnlineVersion}");
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


            if (_strUpdaterVersion != _strUpdaterOnlineVersion && !_bSkipUpdates)
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

            _pageSwitcher = new PageSwitcher();
            _pageSwitcher.Show();
            _pageSwitcher.Activate();
        }

        private void ProcessSelfUpdate()
        {
            var result = MessageBox.Show("New version available.", "Update", MessageBoxButton.OK,
                MessageBoxImage.Information);
            if (result != MessageBoxResult.OK) return;
            Utils.LOG($"Update| New version available: {_strUpdaterOnlineVersion}");
            try
            {
                Process.Start(_downloadLink);
            }
            catch
            {
                // ignored
            }

            Quit();
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
        
        #region Singleton

        private static Lazy<App> _lazyApp;

        public static App Instance => _lazyApp.Value;

        private App()
        {
            _lazyApp = new Lazy<App>(() => this);
        }

        #endregion
    }

    public class RoRUpdaterItem
    {
        public string directory;
        public string fileHash;
        public string fileName;
        public string dlLink;
    }

    public enum UpdateChoice
    {
        INSTALL,
        UPDATE,
        REPAIR
    }
}