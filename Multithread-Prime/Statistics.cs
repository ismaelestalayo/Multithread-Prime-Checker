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
            set { _maxThreads = value; NotifyPropertyChangedAsync("MaxThreads"); }
        }

        private int _maxPrime = 0;
        internal int MaxPrime {
            get { return _maxPrime; }
            set { _maxPrime = value; NotifyPropertyChangedAsync("MaxPrime"); }
        }

        private int _minPrime = 999999999;
        internal int MinPrime {
            get { return _minPrime; }
            set { _minPrime = value; NotifyPropertyChangedAsync("MinPrime"); }
        }

        private int _processed = 0;
        internal int Processed {
            get { return _processed; }
            set { _processed = value; NotifyPropertyChangedAsync("Processed"); }
        }

        private string _lastFile = "";
        internal string LastFile {
            get { return _lastFile; }
            set { _lastFile = value; NotifyPropertyChangedAsync("LastFile"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public async Task NotifyPropertyChangedAsync([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }
    }
}
