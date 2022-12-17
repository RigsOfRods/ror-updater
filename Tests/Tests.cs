using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using ror_updater;
using ror_updater.Tasks;

namespace tests
{
    [TestFixture]
    public class FileUpdateTests
    {
        private SimpleHTTPServer _server;

        static readonly string File1 = "file1.txt";
        static readonly string File2 = "file2.txt";
        private static readonly string BaseDir = Directory.GetCurrentDirectory() ;

        [SetUp]
        public void SetUp()
        {
            _server = new SimpleHTTPServer(BaseDir, 8080);
        }


        [TearDown]
        public void TearDown()
        {
            _server.Stop();
        }

        [Test]
        public async Task InstallGame()
        {
            cd("InstallGame");
            
            var wc = new WebClient();
            var u = new RunUpdate(write, null, wc);
            await u.InstallGame();

            Assert.True(File.Exists(File1));
            Assert.True(File.Exists(File2));
        }

        [Test]
        public async Task UpdateGame()
        {
            cd("UpdateGame");
            
            File.WriteAllText(File1, "1");
            File.WriteAllText(File2, "2");

            var wc = new WebClient();
            var u = new RunUpdate(write, null, wc);
            await u.UpdateGame();

            Assert.True(File.Exists(File1));
            Assert.AreEqual("b68088fa94cc126d0c3371eab844bec5", Utils.GetFileHash(File1));
            Assert.True(File.Exists(File2));
            Assert.AreEqual("20fc92f68d957da0717ad7dd53740ebc", Utils.GetFileHash(File2));
        }

        void write(string s)
        {
            Console.WriteLine(s);
        }

        void cd(string d)
        {
            var wd = $"{BaseDir}\\{d}";
            if (!Directory.Exists(wd))
                Directory.CreateDirectory(wd);
            Directory.SetCurrentDirectory(wd);
        }
    }
}