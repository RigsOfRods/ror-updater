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

        public static void ProcessBadConfig(Exception ex)
        {
            LOG("Error| Failed to read ini file, downloading new updater.ini.");
            LOG(ex.ToString());

            File.Delete("updater.ini");
            
            new WebClient().DownloadFile(App.StrServerUrl + "updater.ini", @"./updater.ini");

            MessageBox.Show("Please restart the updater!");

            //Kill it
            App.Quit();
        }
    }
}