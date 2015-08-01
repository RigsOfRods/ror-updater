using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

namespace ror_updater_list_maker
{
    public partial class MainWindow : Form
    {
        string xml_header;
        string xml_header1 = @"<server version='1.0.0.0' ror='"; 
        string xml_header2 = @"'>" + System.Environment.NewLine + "<files>" + System.Environment.NewLine;

        string xml_footer = @"</files>" + System.Environment.NewLine + "</server>";

        string xmlloop;

        //Get the file's hash in SHA512
        public string GetFileHash(string file)
        {
            SHA512Managed sha = new SHA512Managed();
            FileStream stream = File.OpenRead(file);
            byte[] hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] filePaths = Directory.GetFiles(@"./win32/", "*.*", SearchOption.AllDirectories);
            label1.Text = "0/" + filePaths.Count().ToString();
            progressBar1.Maximum = filePaths.Count();
            int i = 1;
            foreach (string fileName in filePaths)
            {
                Thread.Sleep(10);

                FileInfo fileInfo = new FileInfo(fileName);
                string File_d = fileInfo.Directory.ToString();
                string s = File_d.Substring(File_d.LastIndexOf((char)92 + "win32") + 1);
                s = s.Replace("win32", ".");
                s = s.Replace("" + (char)92, "/");

                if (s == ".")
                    s = s.Replace(".", "./");
                else
                    s = s + "/";

                xmlloop += "  <item id='" + i + "'  directory='" + s + "' name='" + fileInfo.Name + "' hash='" + GetFileHash(fileInfo.FullName) + "'/>" + System.Environment.NewLine;
                i++;
                label1.Text = (i - 1) + "/" + filePaths.Count().ToString();

                progressBar1.Value = (i - 1);
                Application.DoEvents();
            }

            var versionInfo = FileVersionInfo.GetVersionInfo("win32/RoR.exe");
            string str_local_version = versionInfo.ProductVersion;

            xml_header = xml_header1 + str_local_version + xml_header2;

            File.WriteAllText("./List.xml", xml_header + xmlloop + xml_footer);
            xmlloop = "";
        }
    }
}
