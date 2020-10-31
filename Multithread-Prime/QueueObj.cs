using Microsoft.Toolkit.Uwp.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Multithread_Prime {
	public class QueueObj : INotifyPropertyChanged {
        public bool Running { get; set; } = false;
        public int TotalLines { get; set; } = 0;

        private int _doneLines = 0;
        public int DoneLines {
            get { return _doneLines; }
            set { _doneLines = value; NotifyPropertyChanged("DoneLines"); }
        }

        private int _progress = 0;
        public int Progress {
            get { return _progress; }
            set { _progress = value; NotifyPropertyChanged("Progress"); }
        }

        private string _filePath;
        public string FilePath {
            get { return _filePath; }
            set { _filePath = value; NotifyPropertyChanged("FilePath"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                DispatcherHelper.ExecuteOnUIThreadAsync<int>(() => {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    return 1;
                });
            }
        }
    }
}
