using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Skyfera.Models;
using Windows.Storage;

namespace Skyfera.Helpers
{
    public class MainPanelShow
    {
        ObservableCollection<MainPanelEntry> entry = new ObservableCollection<MainPanelEntry>();
        public static string PathToDirectory;
        public async Task<ObservableCollection<MainPanelEntry>> GitItemsInDirAsync(string path)
        {
            PathToDirectory = path;
            StorageFolder Folder = await StorageFolder.GetFolderFromPathAsync($@"{path}");
            IReadOnlyCollection<IStorageItem> itemsReadonly = await Folder.GetItemsAsync();
            ObservableCollection<MainPanelEntry> items = new ObservableCollection<MainPanelEntry>();
            foreach (IStorageItem a in itemsReadonly) items.Add(new MainPanelEntry(a.Attributes, a.DateCreated, a.Name, a.Path));
            return items;
        }
        public ObservableCollection<MainPanelEntry> NonAsyncGitItemsFromPathAsync(string path)
        {
            entry.Clear();
            Task t = ItemsFromNonAsync(path);
            return entry;
        }
        private async Task ItemsFromNonAsync(string path)
        {
            PathToDirectory = path;
            StorageFolder Folder = await StorageFolder.GetFolderFromPathAsync($@"{path}");
            IReadOnlyCollection<IStorageItem> itemsReadonly = await Folder.GetItemsAsync();
            foreach (IStorageItem a in itemsReadonly) entry.Add(new MainPanelEntry(a.Attributes, a.DateCreated, a.Name, a.Path));
        }
    }
}
