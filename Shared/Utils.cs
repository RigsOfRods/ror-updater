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
        internal static readonly string LogPath = $"{Path.GetTempPath()}/RoR_Updater_Log.txt";

        public static void LOG(string str)
        {
            var file = new StreamWriter(LogPath, true);
            file.WriteLine(str);
            file.Close();
        }

        public static string GetFileHash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        public static bool FileIsInUse(string sFilename)
        {
            try
            {
                using var inputStream = File.Open(sFilename, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                var _ = inputStream.Length;
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}