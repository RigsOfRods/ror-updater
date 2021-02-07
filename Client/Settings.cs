namespace ror_updater
{
    public class Settings
    {
        public bool SkipUpdates { get; set; }
        public string ServerUrl { get; set; }
        public string Branch { get; set; }

        public void SetDefaults()
        {
            SkipUpdates = false;
            ServerUrl = "https://test.anotherfoxguy.com";
            Branch = "release";
        }
    }
}