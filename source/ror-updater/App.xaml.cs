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
using System.Windows.Threading;

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
        public string str_updater_version = "1.0.0.5";
        public string str_online_devbuild = "0";
        public string str_local_devbuild = "0";
        private string str_updater_online_version;

        public int listCount;

        public bool b_DevBuilds;
        bool b_skipUpdates;
        bool b_Init = false;
        bool b_SelfUpdating = false;

        XmlDocument xml_ListFile;
        XmlNodeList elemList;

        PageSwitcher pageSwitcher;
        WebClient webClient;
        private Dispatcher _dispatcher;
        public BackgroundWorker ProcessUpdateWorker;
        private BackgroundWorker InitDialog = new BackgroundWorker();

        StartupForm sForm;

        private FileIniDataParser fileIniData;
        private IniData data;

        public void InitApp(object sender, StartupEventArgs e)
        {
            //Clean up first
            File.Delete("Updater_log.txt");
            File.Delete("updater.exe"); //We don't need this anymore
            File.Delete("ror-updater_selfupdate.exe");

            LOG("Info| RoR_Updater ver:" + str_updater_version);

            _dispatcher = Dispatcher.CurrentDispatcher;

            //Show something so users don't get confused
            InitDialog.DoWork += InitDialog_DoWork;
            InitDialog.RunWorkerAsync();

            LOG("Info| Creating Web Handler");
            webClient = new WebClient();
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            LOG("Info| Done.");

            LOG("Info| Creating INI handler");

            //Proceed
            fileIniData = new FileIniDataParser();
            fileIniData.Parser.Configuration.CommentString = "#";

            //Dirty code incoming!
            try
            {
                data = fileIniData.ReadFile("./updater.ini", System.Text.Encoding.ASCII);
            } 
            catch (Exception ex)
            {
                ProcessBadConfig(ex);
            }

            Thread.Sleep(100); //Wait a bit

            try
            {
                b_DevBuilds = bool.Parse(data["Main"]["DevBuilds"]);
                b_skipUpdates = bool.Parse(data["Dev"]["SkipUpdates"]);
                str_local_devbuild = data["Dev"]["DevBuildVer"].ToString();
            } 
            catch (Exception ex)
            {
                ProcessBadConfig(ex);
            }

            LOG("Info| Done.");
            LOG("Info| DevBuilds: " + b_DevBuilds.ToString() + " Skip_updates: " + b_skipUpdates.ToString() + " DevBuildNum: " + str_local_devbuild);

            //Get app version
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

                //Temporary
                //Forced dev mode
                if (str_online_version.EndsWith("-dev"))
                    b_DevBuilds = true;

                if (b_DevBuilds)
                    str_online_devbuild = elemList[i].Attributes["dev"].Value;
            }

            LOG("Succes| Initialization done!");

#if (!DEBUG)
            if (str_updater_version != str_updater_online_version && !b_skipUpdates)
                processSelfUpdate();      
#endif

            b_Init = true;

            InitDialog = null; //We don't need it anymore.. :3
   
            pageSwitcher = new PageSwitcher(this);
            pageSwitcher.Show();
            pageSwitcher.Activate();
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
            b_SelfUpdating = true;

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

            if (!checkGameStructureAndFix())
                return;

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

                if (b_DevBuilds)
                {
                    data["Dev"]["DevBuildVer"] = str_online_devbuild;
                    str_local_devbuild = str_online_devbuild;
                }

                //Always save ini
                fileIniData.WriteFile("updater.ini", data);
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
            Process.GetCurrentProcess().Kill();
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _dispatcher.BeginInvoke(new Action(delegate
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
        private void InitDialog_DoWork(object sender, DoWorkEventArgs e)
        {
            // Very dirty way to do this. :/
            sForm = new StartupForm();
            sForm.Show();
            
            while(!b_Init)
            {
                //meh?
                Thread.Sleep(500);
                if(b_SelfUpdating)
                    sForm.label1.Text = "Updating...";

                sForm.Update();
            }

            sForm.Hide();
            sForm = null;
        }

        private bool checkGameStructureAndFix()
        {
            try
            {
                LOG("Info| start checkGameStructureAndFix");
                string[] dirs = Directory.GetDirectories(@"./resources");
                foreach (string dir in dirs)
                {
                    if (!dir.EndsWith("managed_materials")) //delete all other folders except managed_materials
                    {
                        if (Directory.Exists(dir))
                        {
                            LOG("Info| Deleted: " + dir.ToString());
                            Directory.Delete(dir, true);
                        }
                    }
                }
            } catch (Exception ex)
            {
                LOG("Error| Something bad happened: " + ex.ToString());
                MessageBox.Show("Something bad happened!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }

            LOG("Info| done checkGameStructureAndFix");
            return true;
        }

        void ProcessBadConfig(Exception ex)
        {
            LOG("Error| Failed to read ini file, downloading new updater.ini.");
            LOG(ex.ToString());
            
            File.Delete("updater.ini");

            webClient.DownloadFile(str_server_url + "updater.ini", @"./updater.ini");

            MessageBox.Show("Please restart the updater!"); 

            //Kill it
            Quit();
        }
    }
}
