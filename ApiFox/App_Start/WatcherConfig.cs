using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ApiFox
{
    public class WatcherConfig
    {
        public static void RegisterFolder(string folder)
        {
            string[] filters = { "*.csv", "*.url" }; 
            List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

            foreach (string f in filters)
            {
                var watcher = new FileSystemWatcher();

                watcher.Path = folder;
                /* Watch for changes in LastAccess and LastWrite times, and
                   the renaming of files or directories. */
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Only watch text files.
                watcher.Filter = f;

                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Deleted += new FileSystemEventHandler(OnChanged);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);

                // Begin watching.
                watcher.EnableRaisingEvents = true;
            }
        }

        // Define the event handlers. 
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);

            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                var fileInfo = new FileInfo(e.FullPath);
                switch (fileInfo.Extension)
                {
                    case ".url":
                        //create the json file
                        Task.Run(() =>  Helpers.GetJsonFromGSheets(e.FullPath));
                        break;
                    case ".csv":
                        //create the json file
                        var dt = Helpers.GetDataTabletFromCSVFile(e.FullPath);
                        string jsonText = Helpers.GetJson(dt);
                        if (!string.IsNullOrEmpty(jsonText) && jsonText != "[]")
                            File.WriteAllText(e.FullPath.Replace(".csv", ".json"), jsonText, new UTF8Encoding());
                        break;
                    default:
                        break;
                }
            }
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
    }
}