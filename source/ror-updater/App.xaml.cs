using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using System.Threading;
using System.Net;
using System.IO;
using IniParser;
using IniParser.Model;
using System.ComponentModel;
using System.Security.Cryptography;

namespace ror_updater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string str_server_url = "http://192.223.29.127/rigsofrods/ror_updater/";
        public string str_local_version;
        public string str_online_version;
        public string str_updater_version = "1.0.0.1";
        private string str_updater_online_version;
        public BackgroundWorker ProcessUpdateWorker;
        public int listCount;
        bool DevBuilds;

        XmlDocument xml_ListFile;
        XmlNodeList elemList;

        PageSwitcher pageSwitcher;
        WebClient webClient;

        public void InitApp(object sender, StartupEventArgs e)
        {
            //Clean up first
            File.Delete("Updater_log.txt");
            File.Delete("ror-updater_selfupdate.exe");

            LOG("Info| RoR_Updater ver:" + str_updater_version);

            LOG("Info| Creating INI handler");
            //Proceed
            var fileIniData = new FileIniDataParser();
            fileIniData.Parser.Configuration.CommentString = "#";
            IniData data = fileIniData.ReadFile("./updater.ini", System.Text.Encoding.ASCII);
            LOG("Info| Done.");

            DevBuilds = bool.Parse(data["Main"]["DevBuilds"]);

            LOG("Info| DevBuilds: " + DevBuilds.ToString());

            //Get app version3
            MessageBoxResult result; 
            try 
            {
                //Use Product version instead of file version because we can use it to separate Dev version from release versions, same for TestBuilds
                var versionInfo = FileVersionInfo.GetVersionInfo("RoR.exe");
                str_local_version = versionInfo.ProductVersion;

                LOG("Info| local RoR ver: " + str_local_version);
     
            } 
            catch (Exception ex)
            {
                str_local_version = "unknown";

                //Todo: Make it install the game too.
                LOG("Error| Game Not found!");
                result = MessageBox.Show("Game not found! \nMove me to game's root folder!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                //Make the app wait and not continue
                if (result == MessageBoxResult.OK)
                    Quit();
            }

            LOG("Info| Creating Web Handler");
            webClient = new WebClient();
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            LOG("Info| Done.");

            //Download list
            LOG("Info| Downloading main list from server: " + str_server_url);
           
            try
            {
                webClient.DownloadFile(str_server_url + "List.xml", @"./List.xml");
                LOG("Info| Done.");
            }
            catch (Exception ex)
            {
                result = MessageBox.Show("Could not connect to the main server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    LOG("Error| Failed to connect to server.");
                    LOG(ex.ToString());
                    Quit();
                }
            }
            Thread.Sleep(10); //Wait a bit

            LOG("Info| Creating XML handler, reading file...");
            xml_ListFile = new XmlDocument();
            xml_ListFile.Load("List.xml");
            LOG("Info| Done.");

            elemList = xml_ListFile.GetElementsByTagName("server");

            for (int i = 0; i < elemList.Count; i++)
            {
                str_online_version = elemList[i].Attributes["ror"].Value;
                str_updater_online_version = elemList[i].Attributes["version"].Value;
            }

            LOG("Succes| Initialization done!");

            if (str_updater_version != str_updater_online_version)
                processSelfUpdate();

            pageSwitcher = new PageSwitcher(this);
            pageSwitcher.Show();
        }

        public string GetFileHash(string file)
        {
            SHA512Managed sha = new SHA512Managed();
            FileStream stream = File.OpenRead(file);
            byte[] hash = sha.ComputeHash(stream);
            stream.Close();

            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public void DownloadFile(string szfile, XmlNode node, string dir)
        {
            string szfile_url = szfile;

            if (szfile_url.StartsWith("./"))
                szfile_url = szfile_url.Replace("./", "win32/");

 
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            Thread.Sleep(100);

            try
            {
                File.Delete(szfile);
                webClient.DownloadFile(str_server_url + szfile_url, szfile);
               // webClient.DownloadFileAsync(new Uri(str_server_url + szfile_url), szfile);
            } catch (Exception ex)
            {
                LOG(ex.ToString());
                MessageBox.Show("Failed to download file:" + szfile_url, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
 
        }

        void processSelfUpdate()
        {
            webClient.DownloadFile(str_server_url + "ror-updater_new.exe", @"./ror-updater_new.exe");

            Thread.Sleep(10); //Wait a bit

            string path = "ror-updater_selfupdate.exe";
            File.WriteAllBytes(path, ror_updater.Properties.Resources.ror_updater_selfupdate);

            Thread.Sleep(100); //Wait a bit
            Process.Start(path);

            Quit();
        }

        public void preUpdate()
        {
            //To fix progress bar not moving
            elemList = xml_ListFile.GetElementsByTagName("item");
            listCount = elemList.Count;
        }

        public void ProcessUpdate()
        {
            bool b_hash = false;
            string s_FileHash = "";
            int round = 0;

            ProcessUpdateWorker.ReportProgress(0);

            foreach (XmlNode node in xml_ListFile.SelectNodes("server/files/item[@id]"))
            {
                if (ProcessUpdateWorker.CancellationPending)
                    break;

                Thread.Sleep(10);
                string FileToCheck = node.Attributes["name"].Value;

                if (File.Exists(node.Attributes["directory"].Value + FileToCheck))
                {
                    s_FileHash = GetFileHash(node.Attributes["directory"].Value + FileToCheck);
                    b_hash = true;
                }
                else b_hash = false;

                if (b_hash == true)
                {
                    if (s_FileHash.ToLower() != node.Attributes["hash"].Value.ToLower())
                    {
                        LOG("Info| Downloading file:" + node.Attributes["name"].Value);
                        DownloadFile(node.Attributes["directory"].Value + node.Attributes["name"].Value, node, node.Attributes["directory"].Value);
                        LOG("Info| Done.");
                        
                        round++;
                    }
                    else
                    {
                        LOG("Info| file up to date:" + node.Attributes["name"].Value);
                        round++; 
                    }
                }
                else if (!File.Exists(node.Attributes["directory"].Value + node.Attributes["name"].Value))
                {
                    LOG("Info| Downloading file:" + node.Attributes["name"].Value);
                    DownloadFile(node.Attributes["directory"].Value + node.Attributes["name"].Value, node, node.Attributes["directory"].Value);
                    LOG("Info| Done.");

                    round++;
                }
                else
                {
                    //Ugh?
                    LOG("Info| file up to date:" + node.Attributes["name"].Value);
                    round++; 
                }

                ProcessUpdateWorker.ReportProgress(round + 1);
            }
        }

        public void LOG(String str)
        {
            StreamWriter file = new System.IO.StreamWriter("./Updater_log.txt", true);
            file.WriteLine(str);

            file.Close();
        }
        public void Quit()
        {
            Application.Current.Shutdown();
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;

                string[] _str = null;
                int[] _int = null;

                _str[0] = "Downloaded " + e.BytesReceived + " of " + e.TotalBytesToReceive;
                //_int[0] = int.Parse(Math.Truncate(percentage).ToString());

                PageManager.pageSwitcher.sendData(_str, _int);

            }));
        }

    }
}
