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
        private string str_updater_version = "1.0.0.0";
        private string str_updater_online_version = "1.0.0.0";
        public bool forceUpdate = false;
        public BackgroundWorker ProcessUpdateWorker;
        public int listCount;

        XmlDocument xml_ListFile;
        XmlNodeList elemList;

        PageSwitcher pageSwitcher;
        WebClient webClient;

        public void InitApp(object sender, StartupEventArgs e)
        {
            //Clean logs first
            File.Delete("Updater_log.txt");
            LOG("Info| RoR_Updater ver:" + str_updater_version);

            LOG("Info| Creating INI handler");
            //Proceed
            var fileIniData = new FileIniDataParser();
            fileIniData.Parser.Configuration.CommentString = "#";
            IniData data = fileIniData.ReadFile("./updater.ini", System.Text.Encoding.ASCII);
            LOG("Info| Done.");

            bool forceUpdate = bool.Parse(data["Main"]["ForceUpdate"]);
            bool DevBuilds = bool.Parse(data["Main"]["DevBuilds"]);

            LOG("Info| Force Update: " + forceUpdate.ToString() + " DevBuilds: " + DevBuilds.ToString());

            //Get app version
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
                MessageBox.Show("Game not found! \nMove me to game's root folder!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Quit();
            }

            LOG("Info| Creating Web Handler");
            webClient = new WebClient();
            LOG("Info| Done.");

            //Download list
            LOG("Info| Downloading main list from server: " + str_server_url);
            MessageBoxResult result; 
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
            } catch (Exception ex)
            {
                /* TODO: LOG */
                MessageBox.Show(ex.ToString());
            }
 
        }


        public void ProcessUpdate()
        {
            bool b_hash = false;
            string s_FileHash = "";
            int round = 0;

            elemList = xml_ListFile.GetElementsByTagName("item");
            listCount = elemList.Count;

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
                        /* TODO: LOG FILE */
                    }
                    else
                    {
                        LOG("Info| file up to date:" + node.Attributes["name"].Value);
                        round++; /* TODO: LOG FILE */
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
                    round++; /* TODO: LOG FILE */
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

    }
}
