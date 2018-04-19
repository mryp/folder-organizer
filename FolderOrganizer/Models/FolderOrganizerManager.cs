using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private FolderOrganizerOption _option;

        public event FolderOrganizerLoadedEventHandler Loaded;
        public event FolderOrganizerProgressChangedEventHandler ProgressChanged;
        public event FolderOrganizerCompletedEventHandler Completed;

        public FolderOrganizerManager(string dirPath, FolderOrganizerOption option)
        {
            _dirPath = dirPath;
            _option = option;
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
            bool isDeleteSuccess;
            switch (_option.SearchDeleteType)
            {
                case FolderOrganizerOption.DeleteType.HitDelete:
                    isDeleteSuccess = deleteSerchFile(dirPath, createExtList(_option.SearchPattern));
                    break;
                case FolderOrganizerOption.DeleteType.OtherDelete:
                    isDeleteSuccess = deleteNonSearchFile(dirPath, createExtList(_option.SearchPattern));
                    break;
                default:
                    isDeleteSuccess = true;  //削除なし
                    break;
            }
            if (!isDeleteSuccess)
            {
                return new ResultItem()
                {
                    FilePath = dirPath,
                    Message = "ファイル削除失敗",
                    IsSuccess = false,
                };
            }

            if (_option.MoveUpFolder)
            {
                if (!moveUpFolder(dirPath))
                {
                    new ResultItem()
                    {
                        FilePath = dirPath,
                        Message = "フォルダ移動処理失敗",
                        IsSuccess = false,
                    };
                }
            }

            return new ResultItem()
            {
                FilePath = dirPath,
                Message = "成功",
                IsSuccess = true,
            };
        }

        private string[] createExtList(string pattern)
        {
            return pattern.Split(';');
        }

        /// <summary>
        /// 検索してヒットしたファイルを削除する
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="searchExtList"></param>
        /// <returns></returns>
        private bool deleteSerchFile(string dirPath, string[] searchExtList)
        {
            foreach (string ext in searchExtList)
            {
                string[] files = Directory.GetFiles(dirPath, ext, SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        Debug.WriteLine("[DEBUG]ファイル削除:" + file);
                        File.Delete(file);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"ファイル削除失敗 file={file} e={e.Message}");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 検索してヒットしなかったファイルを削除する
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="searchExtList"></param>
        /// <returns></returns>
        private bool deleteNonSearchFile(string dirPath, string[] searchExtList)
        {
            List<string> searchFileList = new List<string>();
            foreach (string ext in searchExtList)
            {
                searchFileList.AddRange(Directory.GetFiles(dirPath, ext, SearchOption.AllDirectories));
            }

            string[] allFiles = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
            foreach (string file in allFiles)
            {
                if (!searchFileList.Contains(file))
                {
                    try
                    {
                        Debug.WriteLine("[DEBUG]ファイル削除:" + file);
                        File.Delete(file);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"ファイル削除失敗 file={file} e={e.Message}");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 指定したフォルダにあるフォルダ1つだけの階層の時は下のファイル・フォルダを上階層に上げる
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        private bool moveUpFolder(string dirPath, int level=0)
        {
            string[] pathList = Directory.GetFileSystemEntries(dirPath);
            if (pathList.Length == 1)
            {
                FileInfo info = new FileInfo(pathList[0]);
                if (info.Attributes == FileAttributes.Directory)
                {
                    if (!moveUpFolder(pathList[0], level + 1))
                    {
                        return false;
                    }
                    pathList = Directory.GetFileSystemEntries(dirPath);
                }
            }

            if (level != 0)
            {
                string parentDirPath = Path.GetDirectoryName(dirPath);
                foreach (string path in pathList)
                {
                    try
                    {
                        FileInfo info = new FileInfo(path);
                        if (info.Attributes == FileAttributes.Directory)
                        {
                            Directory.Move(path, Path.Combine(parentDirPath, info.Name));
                        }
                        else
                        {
                            File.Move(path, Path.Combine(parentDirPath, info.Name));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"ファイル移動失敗 path={path} e={e.Message}");
                        return false;
                    }
                }

                try
                {
                    Debug.WriteLine("[DEBUG]フォルダ削除:" + dirPath);
                    Directory.Delete(dirPath, false);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"フォルダ削除失敗 dir={dirPath} e={e.Message}");
                    return false;
                }
            }

            return true;
        }
    }
}
