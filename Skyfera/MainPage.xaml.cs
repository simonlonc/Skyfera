using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Skyfera.Helpers;
using Skyfera.Models;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace Skyfera
{
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<LeftPanelEntry> favoriteitems_;
        private ObservableCollection<LeftPanelEntry> drives_;
        private ObservableCollection<LeftPanelEntry> smb_;
        private ObservableCollection<MainPanelEntry> folderinside_;
        private ObservableCollection<MainPanelEntry> fileinfo_;

        public ObservableCollection<LeftPanelEntry> Favoriteitems { get { return favoriteitems_; } set { favoriteitems_ = value; } }
        public ObservableCollection<LeftPanelEntry> Drive { get { return drives_; } set { drives_ = value; } }
        public ObservableCollection<LeftPanelEntry> SMB { get { return smb_; } set { smb_ = value; } }
        public ObservableCollection<MainPanelEntry> FilesInfo { get { return fileinfo_; } set { fileinfo_ = value; } }
        public ObservableCollection<MainPanelEntry> FolderInside { get { return folderinside_; } set { folderinside_ = value; } }

        Stack<string> previouspaths = new Stack<string>();
        Stack<string> forwardpaths = new Stack<string>();
        public MainPage()
        {
            LeftPanelFill LPF = new LeftPanelFill();
            var coreTitle = CoreApplication.GetCurrentView().TitleBar;  // Nastavenie Custom TitleBaru
            coreTitle.ExtendViewIntoTitleBar = false;
            var coreTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            coreTitleBar.BackgroundColor = (Windows.UI.Color?)Application.Current.Resources["SystemAccentColorDark2"];
            coreTitleBar.ForegroundColor = (Windows.UI.Color?)Application.Current.Resources["SystemAccentColorLight3"];

            this.InitializeComponent();
            MainPanelShow.PathToDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Recent); //Nastavenie uvodnej zlozky

            FolderInside = new ObservableCollection<MainPanelEntry>(); // inicializacia kolekcii
            favoriteitems_ = LeftPanelFill.LeftFavorites();
            fileinfo_ = new ObservableCollection<MainPanelEntry>();
            drives_ = LeftPanelFill.filldrive();
            smb_ = LeftPanelFill.fillserver();

            PathToDirectory.Text = MainPanelShow.PathToDirectory;

            FavoriteLeftGrid.ItemsSource = Favoriteitems; //DataBinding kolekcii
            Drives.ItemsSource = Drive;
            MainRightGrid.ItemsSource = FolderInside;
            FileInfo.ItemsSource = FilesInfo;
            Network.ItemsSource = SMB;
            Task t1 = startup(); // Inicializacia uvodnej zlozky
            

        }
        private async void FavoriteLeftGrid_ItemClickAsync(object sender, ItemClickEventArgs e)  // Kliknutie na lavy stlpec pre rychli presun medzi predvolenymi zlozkami
        {
            string temp = $@"{((LeftPanelEntry)e.ClickedItem).path_}";
            if ((temp != null) && (temp != "") && (temp != " "))
            {
                forwardpaths.Clear();
                previouspaths.Push(PathToDirectory.Text);
                ChangePathToDir(temp);
                FolderInside.Clear();
                MainPanelShow mPS = new MainPanelShow();
                ObservableCollection<MainPanelEntry> items_ = await mPS.GitItemsInDirAsync(temp);
                foreach (MainPanelEntry a in items_) FolderInside.Add(new MainPanelEntry(a.Attributes, a.DateCreated, a.Name, a.Path));
            }
            testforbuttons();
        }

        private async void MainRightGrid_ItemClicked(object sender, ItemClickEventArgs e) // Prehladavanie zloziek
        {
            string testifDirectory = ((MainPanelEntry)e.ClickedItem).Attributes.ToString();
            if ((testifDirectory == "Directory") || (testifDirectory == "ReadOnly, Directory"))
            {
                forwardpaths.Clear();
                string temp = $@"{((MainPanelEntry)e.ClickedItem).Path}";
                previouspaths.Push(PathToDirectory.Text);
                ChangePathToDir(temp);
                FolderInside.Clear();
                MainPanelShow mPS = new MainPanelShow();
                ObservableCollection<MainPanelEntry> items_ = await mPS.GitItemsInDirAsync(temp);
                foreach (MainPanelEntry a in items_) FolderInside.Add(new MainPanelEntry(a.Attributes, a.DateCreated, a.Name, a.Path));
            }
            else // Otvorenie suboru
            {
                FilesInfo.Clear();
                IStorageFile file = await StorageFile.GetFileFromPathAsync(((MainPanelEntry)e.ClickedItem).Path);

                string[] subs = ((MainPanelEntry)e.ClickedItem).Path.Split('.');
                int lastone = subs.Length - 1;
                System.Diagnostics.Debug.WriteLine($"{subs[lastone]}");
                if (subs[lastone] == "jpeg" || subs[lastone] == "png" || subs[lastone] == "ico" || subs[lastone] == "bmp" || subs[lastone] == "gif" || subs[lastone] == "tiff" || subs[lastone] == "svg" || subs[lastone] == "jpg")
                {

                    using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                         await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                    {
                        Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                        bitmapImage.SetSource(fileStream);
                        Preview.Source = bitmapImage;
                    }

                    System.Diagnostics.Debug.WriteLine("done");
                }
                else
                {
                    var open = await Windows.System.Launcher.LaunchFileAsync(file);
                    if (!open)
                    {
                        System.Diagnostics.Debug.WriteLine($"Cannot open {file.Name}, because it is not supported yet");
                    }
                }
                string[] item = ((MainPanelEntry)e.ClickedItem).Path.Split('\\');
                int lastone_ = item.Length - 1;
                string name = item[lastone_];
                item[lastone_] = null;
                string path = String.Join("\\",item);
                StorageFolder Folder = await StorageFolder.GetFolderFromPathAsync($@"{path}");
                IReadOnlyCollection<IStorageItem> itemsReadonly = await Folder.GetItemsAsync();
                foreach (IStorageItem a in itemsReadonly)
                {
                    if(a.Name == name)
                    {
                        FilesInfo.Add(new MainPanelEntry(a.Attributes, a.DateCreated, a.Name, a.Path));
                    }    
                }
            }
            testforbuttons();
        }
        private async Task startup()  // Zaplnenie hlavneho panelu z prvotnej zlozky
        {
            string temp = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
            ChangePathToDir(temp);
            FolderInside.Clear();
            MainPanelShow mPS = new MainPanelShow();
            ObservableCollection<MainPanelEntry> items_ = await mPS.GitItemsInDirAsync(temp);
            foreach (MainPanelEntry a in items_) FolderInside.Add(new MainPanelEntry(a.Attributes, a.DateCreated, a.Name, a.Path));
        }

        private void PathToDirectory_TextChanged(object sender, RoutedEventArgs e) //vratenie zmeni cesty pri nestlaceni enteru
        {
            PathToDirectory.Text = MainPanelShow.PathToDirectory;
        }
        public void ChangePathToDir(string path) // zmena cesti pri navigovani
        {
            PathToDirectory.Text = path;
        }

        private void PathToDirectory_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            Task t1 = PTD_KDAsync(sender, e);
        }
        private async Task PTD_KDAsync(object sender, KeyRoutedEventArgs e) // Zmena cesty pri stlaceni enteru
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (Directory.Exists(PathToDirectory.Text))
                {
                    forwardpaths.Clear();
                    previouspaths.Push(MainPanelShow.PathToDirectory);
                    FolderInside.Clear();
                    MainPanelShow mPS = new MainPanelShow();
                    ObservableCollection<MainPanelEntry> items_ = await mPS.GitItemsInDirAsync(PathToDirectory.Text);
                    foreach (MainPanelEntry a in items_) FolderInside.Add(new MainPanelEntry(a.Attributes, a.DateCreated, a.Name, a.Path));
                }
            }
            testforbuttons();
        }

        private async void UpAFolder(object sender, RoutedEventArgs e) // vyjde o uroven vyssie z cesty
        {
            forwardpaths.Clear();
            List<string> vs = new List<string>(PathToDirectory.Text.Split("\\").ToList());
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < vs.Count() - 1; i++)
            {
                sb.Append(vs[i].ToString());
                if (i < vs.Count - 2) sb.Append("\\");
            }
            string newpath = sb.ToString();

            FolderInside.Clear();
            MainPanelShow mps = new MainPanelShow();
            ObservableCollection<MainPanelEntry> items_ = null;
            try
            {
                previouspaths.Push(PathToDirectory.Text);
                ChangePathToDir(newpath);
                items_ = await mps.GitItemsInDirAsync(newpath);
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine(ex);
                sb.Clear();
                for (int i = 0; i < 1; i++)
                {
                    sb.Append(vs[i].ToString());
                    if (i < vs.Count - 2) sb.Append("\\");
                }
                newpath = sb.ToString();
                previouspaths.Push(PathToDirectory.Text);
                ChangePathToDir(newpath);
                FolderInside.Clear();
                items_ = await mps.GitItemsInDirAsync(newpath);
            }
            foreach (MainPanelEntry a in items_) FolderInside.Add(new MainPanelEntry(a.Attributes, a.DateCreated, a.Name, a.Path));
            testforbuttons();
        }

        private async void GoBack(object sender, RoutedEventArgs e) //vrati sa na predchadzajucu poziciu v ceste
        {
            string temp = previouspaths.Peek();
            FolderInside.Clear();
            MainPanelShow mPS = new MainPanelShow();
            ObservableCollection<MainPanelEntry> items_ = new ObservableCollection<MainPanelEntry>();
            try
            {
                FolderInside.Clear();
                items_ = await mPS.GitItemsInDirAsync(temp);
                forwardpaths.Push(PathToDirectory.Text);
                ChangePathToDir(temp);
                temp = previouspaths.Pop();
            }
            catch (System.UnauthorizedAccessException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                FolderInside.Clear();
                items_ = await mPS.GitItemsInDirAsync(PathToDirectory.Text);
            }

            foreach (MainPanelEntry a in items_) FolderInside.Add(new MainPanelEntry(a.Attributes, a.DateCreated, a.Name, a.Path));
            testforbuttons();
        }

        private async void GoForward(object sender, RoutedEventArgs e) // obnovi sa predchadzajuca cesta
        {
            previouspaths.Push(PathToDirectory.Text);
            string temp = forwardpaths.Pop();
            ChangePathToDir(temp);
            FolderInside.Clear();
            MainPanelShow mPS = new MainPanelShow();
            ObservableCollection<MainPanelEntry> items_ = await mPS.GitItemsInDirAsync(temp);
            foreach (MainPanelEntry a in items_) FolderInside.Add(new MainPanelEntry(a.Attributes, a.DateCreated, a.Name, a.Path));
            testforbuttons();
        }
        private void testforbuttons() // povolenie pre stacenie tlacidiel
        {
            if (forwardpaths.Count > 0) GoForwardBtn.IsEnabled = true;
            else GoForwardBtn.IsEnabled = false;
            if (previouspaths.Count > 0) GoBackBtn.IsEnabled = true;
            else GoBackBtn.IsEnabled = false;
        }

        private async void ConfigFile_ItemClick(object sender, ItemClickEventArgs e)
        {
            string dir = ApplicationData.Current.LocalFolder.Path;
            string configfile = ApplicationData.Current.LocalFolder.Path + "\\config.txt";
            IStorageFile file = await StorageFile.GetFileFromPathAsync(configfile);
            var open = await Windows.System.Launcher.LaunchFileAsync(file);
            if (!open)
            {
                System.Diagnostics.Debug.WriteLine($"Cannot open {file.Name}, because it is not supported yet");
            }
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            LeftPanelFill LPF = new LeftPanelFill();
            while (LeftPanelFill.IsExecuted == false) Thread.Sleep(200);
            favoriteitems_ = LeftPanelFill.LeftFavorites();
            smb_ = LeftPanelFill.fillserver();
            drives_ = LeftPanelFill.filldrive();
            FavoriteLeftGrid.ItemsSource = Favoriteitems;
            Network.ItemsSource = SMB;
            Drives.ItemsSource = Drive;
        }
    }
}
