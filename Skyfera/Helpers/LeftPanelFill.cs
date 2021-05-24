using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Skyfera.Models;
using Windows.Storage;
namespace Skyfera.Helpers
{
    class LeftPanelFill
    {
        public static List<LeftPanelEntry> panelFavoritesLeft = new List<LeftPanelEntry>();
        public static List<LeftPanelEntry> drivesLeft = new List<LeftPanelEntry>();
        public static List<LeftPanelEntry> smbLeft = new List<LeftPanelEntry>();
        private List<char> lettersavalibe = new List<char>();
        public static bool IsExecuted;
        public LeftPanelFill()
        {
            IsExecuted = false;
            lettersavalibe.Clear();
            panelFavoritesLeft.Clear();
            drivesLeft.Clear();
            smbLeft.Clear();
            string alpha = "abcdefghijklmnopqrstuvqxyz";
            foreach (char c in alpha)
            {
                lettersavalibe.Add(c);
            }
            Task t = defaultsAsync();
            drives();
        }


        private async Task defaultsAsync()
        {
            panelFavoritesLeft.Add(new LeftPanelEntry("Desktop", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)));
            panelFavoritesLeft.Add(new LeftPanelEntry("Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
            panelFavoritesLeft.Add(new LeftPanelEntry("Downloads", Syroot.Windows.IO.KnownFolders.Downloads.Path));
            panelFavoritesLeft.Add(new LeftPanelEntry("Music", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)));
            panelFavoritesLeft.Add(new LeftPanelEntry("Pictures", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)));
            panelFavoritesLeft.Add(new LeftPanelEntry("Videos", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)));
            await Config();
        }
        private async Task Config()
        {
            string dir= ApplicationData.Current.LocalFolder.Path;
            Directory.CreateDirectory(dir);
            var packageFolder = StorageFolder.GetFolderFromPathAsync(dir);
            string configfile =dir+"\\config.txt";
            if (File.Exists(configfile))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(configfile))
                    {
                        int section = 0; // 1 => Favorites , 2 => Samba
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (!line.Contains("#"))
                            {
                                if (line == "[Favorite]")
                                {
                                    section = 1;
                                }
                                else if (line == "[Samba]")
                                {
                                    section = 2;
                                }
                                if (section == 1)
                                {
                                    if (line != "[Favorite]")
                                    {
                                        string[] split = line.Split("'");
                                        if (split.Count() >= 5)
                                        {
                                            panelFavoritesLeft.Add(new LeftPanelEntry(split[1], split[3]));
                                        }
                                    }
                                }
                                if(section == 2)
                                {
                                    if (line != "[Samba]")
                                    {
                                        string[] split = line.Split("'");
                                        if (split.Count() >= 5)
                                        {
                                            smbLeft.Add(new LeftPanelEntry(split[1], $"\\\\{split[3]}"));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }
            else
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("config file not found ... making one");
                    using (StreamWriter sw = File.CreateText(configfile))
                    {
                        sw.WriteLine("[Favorite]");
                        sw.WriteLine("# ['Name'] ['Path to directory']");
                        sw.WriteLine("# 'MyFolder' 'C:\\Program files\\MyDirectory'");
                        sw.WriteLine("");
                        sw.WriteLine("[Samba]");
                        sw.WriteLine("# ['Name'] ['IP of server'] ");
                        sw.WriteLine("# 'Home' 'ServerIP\\Folder' ");
                    }
                    System.Diagnostics.Debug.WriteLine("Made a config file");
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("the file could not be created:");
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }
        private void drives()
        {
            string[] drives = System.IO.Directory.GetLogicalDrives();

            foreach (string str in drives)
            {
                string[] subs = str.Split(":");
                drivesLeft.Add(new LeftPanelEntry($"Drive {subs[0]} :/", str));
                lettersavalibe.Remove(char.Parse(subs[0]));
            }
        }
        public static ObservableCollection<LeftPanelEntry> filldrive()
        {
            ObservableCollection<LeftPanelEntry> drive = new ObservableCollection<LeftPanelEntry>();
            for (int i = 0; i < LeftPanelFill.drivesLeft.Count; i++)
            {
                drive.Add(new LeftPanelEntry(LeftPanelFill.drivesLeft[i].namedir_, LeftPanelFill.drivesLeft[i].path_));
            }
            return drive;

        }
        public static ObservableCollection<LeftPanelEntry> LeftFavorites()
        {
            ObservableCollection<LeftPanelEntry> favorites = new ObservableCollection<LeftPanelEntry>();
            for (int i = 0; i < LeftPanelFill.panelFavoritesLeft.Count(); i++)
            {
                favorites.Add(new LeftPanelEntry(LeftPanelFill.panelFavoritesLeft[i].namedir_, LeftPanelFill.panelFavoritesLeft[i].path_));
                System.Diagnostics.Debug.WriteLine(LeftPanelFill.panelFavoritesLeft[i].namedir_ + " " + LeftPanelFill.panelFavoritesLeft[i].path_);
            }
            return favorites;
        }
        public static ObservableCollection<LeftPanelEntry> fillserver()
        {
            ObservableCollection<LeftPanelEntry> smb = new ObservableCollection<LeftPanelEntry>();
            for (int i = 0; i < LeftPanelFill.smbLeft.Count; i++)
            {
                smb.Add(new LeftPanelEntry(LeftPanelFill.smbLeft[i].namedir_, LeftPanelFill.smbLeft[i].path_));
            }
            return smb;
        }
    }
}
