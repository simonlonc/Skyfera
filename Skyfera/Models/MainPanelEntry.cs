using System;

namespace Skyfera.Models
{
    public class MainPanelEntry
    {
        public Windows.Storage.FileAttributes Attributes { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public MainPanelEntry(Windows.Storage.FileAttributes Attributes_, DateTimeOffset DateCreated_, string Name_, string Path_)
        {
            this.Attributes = Attributes_;
            this.DateCreated = DateCreated_;
            this.Name = Name_;
            this.Path = Path_;
        }
    }
}
