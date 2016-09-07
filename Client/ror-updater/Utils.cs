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
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;

namespace ror_updater
{
    internal class Utils
    {
        public static void LOG(string str)
        {
            var file = new StreamWriter("./Updater_log.txt", true);
            file.WriteLine(str);

            file.Close();
        }

        public static string GetFileHash(string filename)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var buffer = md5.ComputeHash(File.ReadAllBytes(filename));
                var sb = new StringBuilder();
                for (var i = 0; i < buffer.Length; i++)
                    sb.Append(buffer[i].ToString("x2"));
                return sb.ToString();
            }
        }

        public static void DownloadFile(WebClient client, string szfile, string dir)
        {
            var szfileUrl = szfile;

            if (!App.BDevBuilds)
            {
                if (szfileUrl.StartsWith("./"))
                    szfileUrl = szfileUrl.Replace("./", "win32/");
            }
            else
            {
                if (szfileUrl.StartsWith("./"))
                    szfileUrl = szfileUrl.Replace("./", "win32-dev/");
            }


            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            Thread.Sleep(100);

            try
            {
                File.Delete(szfile);
                LOG("Info| ULR: " + App.StrServerUrl + szfileUrl);
                LOG("Info| File: " + szfile);
                client.DownloadFileAsync(new Uri(App.StrServerUrl + szfileUrl), szfile);
            }
            catch (Exception ex)
            {
                LOG(ex.ToString());
                MessageBox.Show("Failed to download file:" + szfileUrl, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public static bool CheckGameStructureAndFix()
        {
            try
            {
                if (!App.BDevBuilds)
                {
                    LOG("Info| start checkGameStructureAndFix");
                    var dirs = Directory.GetDirectories(@"./resources");
                    foreach (var dir in dirs)
                        if (!dir.EndsWith("managed_materials")) //delete all other folders except managed_materials
                            if (Directory.Exists(dir))
                            {
                                LOG("Info| Deleted: " + dir);
                                Directory.Delete(dir, true);
                            }
                }
                else
                {
                    LOG("Info| start checkGameStructureAndFix for Dev-builds");
                    var files = Directory.GetFiles(@"./resources");
                    foreach (var file in files)
                        if (File.Exists(file))
                        {
                            LOG("Info| Deleted: " + file);//delete all resources zips
                            File.Delete(file);
                        }
                }
            }
            catch (Exception ex)
            {
                LOG("Error| Something bad happened: " + ex);
                MessageBox.Show("Something bad happened!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }

            LOG("Info| done checkGameStructureAndFix");
            return true;
        }

        public static void ProcessBadConfig(Exception ex)
        {
            LOG("Error| Failed to read ini file, downloading new updater.ini.");
            LOG(ex.ToString());

            File.Delete("updater.ini");

            var webc = new WebClient();
            webc.DownloadFile(App.StrServerUrl + "updater.ini", @"./updater.ini");

            MessageBox.Show("Please restart the updater!");

            //Kill it
            App.Quit();
        }
    }
}