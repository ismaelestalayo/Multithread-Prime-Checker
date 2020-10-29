using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System.Threading;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Multithread_Prime {

	public class QueueObj{
        public Thread Thread { get; set; }
        public int Index { get; set; }
        public int TotalLines { get; set; }
        public int DoneLines { get; set; } = 0;
        public int Progress { get; set; } = 0;
        public string FilePath { get; set; } = "none";
        public string[] Lines { get; set; }
    }

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

        private int _minPrime = 999999;
        internal int MinPrime {
            get { return _minPrime; }
            set { _minPrime = value; NotifyPropertyChangedAsync("MinPrime"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
		public async Task NotifyPropertyChangedAsync([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }
	}

	public sealed partial class MainPage : Page {

        // statistics to show
        internal int DoneFiles = 0;
        internal string LastFile = "";

        internal Statistics _statistics { get; set; } = new Statistics();


        private Thread[] _workers;
        private static ThreadPoolTimer PeriodicTimer;

        public MainPage() {
            this.InitializeComponent();

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(400, 400));
            ApplicationView.PreferredLaunchViewSize = new Size(500, 500);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            InitialFilesAccess();
        }


        // *******************************************************************
        private async void InitialFilesAccess() {
            string path = "C:\\Users\\ismae\\Desktop\\Practical.Work3\\rand_files";
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);

            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync(CommonFileQuery.OrderByName, 0, 16);


            PCQueue q = new PCQueue(2);


            for (int i = 0; i < files.Count - 1; i++) {
                int itemNumber = i;
                string text = await FileIO.ReadTextAsync(files[i]);

                q.EnqueueItem(() => {
                    Debug.WriteLine(string.Format("> FILE Nº{0} QUEUED", itemNumber));
                    ProcessFileAsync(files[i].Path, text, itemNumber);
                });
            }


            q.Shutdown(true);
            Debug.WriteLine("> Workers complete!");
        }


        // *******************************************************************
        // Alter the thread count
		private void IncreaseThreadPool(object sender, RoutedEventArgs e) {
            _statistics.MaxThreads += 1;
		}

        private void DecreaseThreadPool(object sender, RoutedEventArgs e) {
            if(_statistics.MaxThreads > 1) {
                _statistics.MaxThreads -= 1;
            }
        }



        // *******************************************************************
        // File handler and prime checker
        private async Task ProcessFileAsync(string name, string text, int index) {
            string[] lines = text.Split('\n');

            Debug.WriteLine(string.Format(">>>>>>>>>>>>>> File {0} started", index));
            for (int i = 0; i < 20 - 1; i++) { // lines.Length
                int n = int.Parse(lines[i]);
                bool prime = IsPrime(n);

				if (prime) {
                    if (n > _statistics.MaxPrime) {
                        _statistics.MaxPrime = n;
						//await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
      //                      MaxTextBlock.Text = string.Format("Max: {0}", n);
      //                  });
                    }
                    if (n < _statistics.MinPrime) {
                        _statistics.MinPrime = n;
                        //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        //    MinTextBlock.Text = string.Format("Min: {0}", n);
                        //});
                    }
                }
				//thrObj.Progress = (int)(((float)thrObj.DoneLines / thrObj.TotalLines) * 100);

				Debug.WriteLine(string.Format("  - File {0} line {1}: {2} prime: {3}", index, i, n, prime));
            }
            Debug.WriteLine(string.Format("<<<<<<<<<<<<<< File {0} finished", index));
            LastFile = name;
        }

        private bool IsPrime(int number) {
            int max = number / 2;
            var isPrime = false;

            for (int i = 2; i <= max; i++) {
                if (number % i == 0) {
                    isPrime = true;
                    break;
                }
            }

            return isPrime;
        }
    }

}


//TimeSpan period = TimeSpan.FromMilliseconds(500);
//PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) => {
//    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {

//    });
//}, period);




