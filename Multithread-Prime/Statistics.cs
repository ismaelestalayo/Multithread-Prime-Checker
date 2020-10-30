using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Multithread_Prime {
	public class Statistics : Page, INotifyPropertyChanged {
        private int _maxThreads = 2;
        internal int MaxThreads {
            get { return _maxThreads; }
            set { _maxThreads = value; NotifyPropertyChanged("MaxThreads"); }
        }

        private int _maxPrime = 0;
        internal int MaxPrime {
            get { return _maxPrime; }
            set { _maxPrime = value; NotifyPropertyChanged("MaxPrime"); }
        }

        private int _minPrime = 999999999;
        internal int MinPrime {
            get { return _minPrime; }
            set { _minPrime = value; NotifyPropertyChanged("MinPrime"); }
        }

        private int _processed = 0;
        internal int Processed {
            get { return _processed; }
            set { _processed = value; NotifyPropertyChanged("Processed"); }
        }

        private string _lastFile = "_";
        internal string LastFile {
            get { return _lastFile; }
            set { _lastFile = value; NotifyPropertyChanged("LastFile"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                 var _ = Dispatcher.RunAsync(CoreDispatcherPriority.High, () => {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }
    }
}
