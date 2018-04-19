using Caliburn.Micro;
using FolderOrganizer.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FolderOrganizer.ViewModels
{
    public class StartViewModel : Screen
    {
        private readonly INavigationService _navigationService;
        private string _folderPath;
        private string _searchPattern;
        private FolderOrganizerOption.DeleteType _searchDeleteType;
        private bool _moveUpFolder;

        public string FolderPath
        {
            get { return _folderPath; }
            set
            {
                _folderPath = value;
                NotifyOfPropertyChange(() => FolderPath);
            }
        }

        public string SearchPattern
        {
            get { return _searchPattern; }
            set
            {
                _searchPattern = value;
                NotifyOfPropertyChange(() => SearchPattern);
            }
        }

        public FolderOrganizerOption.DeleteType SearchDeleteType
        {
            get { return _searchDeleteType; }
            set
            {
                _searchDeleteType = value;
                NotifyOfPropertyChange(() => SearchDeleteType);
            }
        }

        public bool MoveUpFolder
        {
            get { return _moveUpFolder; }
            set
            {
                _moveUpFolder = value;
                NotifyOfPropertyChange(() => MoveUpFolder);
            }
        }

        public Dictionary<FolderOrganizerOption.DeleteType, string> SearchDeleteTypeNameTable
        {
            get;
            set;
        }

        public StartViewModel(INavigationService navigationService)
        {
            this._navigationService = navigationService;

            this.FolderPath = "";
            this.SearchPattern = Properties.Settings.Default.SearchPattern;
            this.SearchDeleteType = (FolderOrganizerOption.DeleteType)Properties.Settings.Default.SearchDeleteType;
            this.SearchDeleteTypeNameTable = new Dictionary<FolderOrganizerOption.DeleteType, string>()
            {
                {FolderOrganizerOption.DeleteType.None, "何もしない"},
                {FolderOrganizerOption.DeleteType.HitDelete, "対象となるファイルを削除"},
                {FolderOrganizerOption.DeleteType.OtherDelete, "対象となるファイル以外を削除"},
            };
            this.MoveUpFolder = Properties.Settings.Default.MoveUpFolder;
        }

        public void SetSelectFolder()
        {
            //https://github.com/aybe/Windows-API-Code-Pack-1.1 を使用
            var dialog = new CommonOpenFileDialog("フォルダ選択")
            {
                IsFolderPicker = true,
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.FolderPath = dialog.FileName;
            }
        }


        public void Start()
        {
            if (!Directory.Exists(this.FolderPath))
            {
                showError("指定したフォルダが見つかりません");
                return;
            }

            saveSetting();
            _navigationService.For<ProcViewModel>()
                .WithParam(v => v.FolderPath, FolderPath)
                .WithParam(v => v.Option, createOption())
                .Navigate();
        }

        private void saveSetting()
        {
            Properties.Settings.Default.SearchPattern = this.SearchPattern;
            Properties.Settings.Default.SearchDeleteType = (int)this.SearchDeleteType;
            Properties.Settings.Default.MoveUpFolder = this.MoveUpFolder;
            Properties.Settings.Default.Save();
        }

        private FolderOrganizerOption createOption()
        {
            return new FolderOrganizerOption()
            {
                SearchPattern = SearchPattern,
                SearchDeleteType = SearchDeleteType,
                MoveUpFolder = MoveUpFolder,
            };
        }

        private void showError(string message)
        {
            MessageBox.Show(message, "エラー"
                , MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
