using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderOrganizer.Models
{
    delegate void FolderOrganizerLoadedEventHandler(object sender, int fileCount);
    delegate void FolderOrganizerProgressChangedEventHandler(object sender, ResultItem item);
    delegate void FolderOrganizerCompletedEventHandler(object sender, FolderOrganizerCompletedStatus result);

    enum FolderOrganizerCompletedStatus
    {
        Completed,
        Untreated,
        Cancel,
    }

    class FolderOrganizerManager
    {
        private string _dirPath;
        private bool _isCancel = false;
        private Task _extractTask;

        public event FolderOrganizerLoadedEventHandler Loaded;
        public event FolderOrganizerProgressChangedEventHandler ProgressChanged;
        public event FolderOrganizerCompletedEventHandler Completed;

        public FolderOrganizerManager(string dirPath)
        {
            _dirPath = dirPath;
        }

        public void ExtractAsync()
        {
            if (_extractTask != null)
            {
                if (!_extractTask.IsCompleted)
                {
                    return;
                }
            }

            _isCancel = false;
            _extractTask = Task.Run(() =>
            {
                if (!Directory.Exists(_dirPath))
                {
                    Completed?.Invoke(this, FolderOrganizerCompletedStatus.Untreated);
                    return;
                }
                string[] dirs = Directory.GetDirectories(_dirPath);
                if (dirs.Length == 0)
                {
                    Completed?.Invoke(this, FolderOrganizerCompletedStatus.Untreated);
                    return;
                }

                Loaded?.Invoke(this, dirs.Length);
                foreach (string dir in dirs)
                {
                    if (_isCancel)
                    {
                        Completed?.Invoke(this, FolderOrganizerCompletedStatus.Cancel);
                        return;
                    }
                    ResultItem item = organize(dir);
                    ProgressChanged?.Invoke(this, item);
                }

                Completed?.Invoke(this, FolderOrganizerCompletedStatus.Completed);
            });
        }

        public void Cancel()
        {
            _isCancel = true;
        }

        private ResultItem organize(string dirPath)
        {


            return new ResultItem()
            {
                FilePath = dirPath,
                Message = "成功",
                IsSuccess = true,
            };
        }
    }
}
