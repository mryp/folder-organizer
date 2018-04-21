using Caliburn.Micro;
using FolderOrganizer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FolderOrganizer.ViewModels
{
    public class ProcViewModel : Screen
    {
        private const string BUTTON_NAME_CANCEL = "キャンセル";
        private const string BUTTON_NAME_BACK = "戻る";

        private readonly INavigationService _navigationService;
        private string _cancelButtonName;
        private int _progressMax;
        private int _progressValue;
        private object _selectedLogItem;
        private FolderOrganizerManager _organizer;

        public string FolderPath
        {
            get;
            set;
        }

        public FolderOrganizerOption Option
        {
            get;
            set;
        }

        public string CancelButtonName
        {
            get { return _cancelButtonName; }
            set
            {
                _cancelButtonName = value;
                NotifyOfPropertyChange(() => CancelButtonName);
            }
        }

        public int ProgressMax
        {
            get { return _progressMax; }
            set
            {
                _progressMax = value;
                NotifyOfPropertyChange(() => ProgressMax);
            }
        }

        public int ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                NotifyOfPropertyChange(() => ProgressValue);
            }
        }

        public object SelectedLogItem
        {
            get { return _selectedLogItem; }
            set
            {
                _selectedLogItem = value;
                NotifyOfPropertyChange(() => SelectedLogItem);
            }
        }

        public BindableCollection<ResultItem> ProgressItemList
        {
            get;
            set;
        }

        public ProcViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            this.CancelButtonName = BUTTON_NAME_CANCEL;
            this.ProgressItemList = new BindableCollection<ResultItem>();
        }

        public void Loaded()
        {
            _organizer = new FolderOrganizerManager(this.FolderPath, this.Option);
            _organizer.Loaded += organizer_Loaded;
            _organizer.ProgressChanged += organizer_ProgressChanged;
            _organizer.Completed += organizer_Completed;
            _organizer.OrganizeAsync();
        }

        public void Cancel()
        {
            if (this.CancelButtonName == BUTTON_NAME_CANCEL)
            {
                _organizer.Cancel();
            }
            else
            {
                _navigationService.For<StartViewModel>().Navigate();
            }
        }

        public void CopyLogItem()
        {
            if (SelectedLogItem is ResultItem)
            {
                ResultItem item = (ResultItem)SelectedLogItem;
                Clipboard.SetData(DataFormats.Text, item.FilePath);
                MessageBox.Show(Path.GetFileName(item.FilePath) + "のパスをクリップボードにコピーしました。");
            }
        }

        private void organizer_Loaded(object sender, int fileCount)
        {
            this.ProgressValue = 0;
            this.ProgressMax = fileCount;
        }

        private void organizer_ProgressChanged(object sender, ResultItem item)
        {
            Debug.WriteLine(item);
            this.ProgressValue = this.ProgressValue + 1;
            ProgressItemList.Add(item);
        }

        private void organizer_Completed(object sender, FolderOrganizerCompletedStatus result)
        {
            switch (result)
            {
                case FolderOrganizerCompletedStatus.Completed:
                    MessageBox.Show("すべてのフォルダの処理が完了しました");
                    break;
                case FolderOrganizerCompletedStatus.Untreated:
                    MessageBox.Show("処理を行うフォルダがありません");
                    break;
                case FolderOrganizerCompletedStatus.Cancel:
                    MessageBox.Show("キャンセルされました");
                    break;
            }
            this.CancelButtonName = BUTTON_NAME_BACK;
        }
    }

}
