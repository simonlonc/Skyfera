namespace Skyfera.Models
{
    public class LeftPanelEntry
    {
        public string namedir_ { get; set; }
        public string path_ { get; set; }
        public LeftPanelEntry(string namedir, string path)
        {
            this.namedir_ = namedir;
            this.path_ = path;
        }
    }


}

