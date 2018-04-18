using Caliburn.Micro;
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

        public string FolderPath
        {
            get { return _folderPath; }
            set
            {
                _folderPath = value;
                NotifyOfPropertyChange(() => FolderPath);
            }
        }

        public StartViewModel(INavigationService navigationService)
        {
            this._navigationService = navigationService;

            this.FolderPath = "";
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

            _navigationService.For<ProcViewModel>()
                .WithParam(v => v.FolderPath, FolderPath)
                .Navigate();
        }

        private void showError(string message)
        {
            MessageBox.Show(message, "エラー"
                , MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
