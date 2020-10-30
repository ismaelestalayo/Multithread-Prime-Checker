using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Multithread_Prime {

	public class QueueObj : Page, INotifyPropertyChanged {
        public bool Running { get; set; }
        public int Index { get; set; }
        public int TotalLines { get; set; }
        public int DoneLines { get; set; }
        public int Progress { get; set; }

        private string _filePath;
        public string FilePath {
            get { return _filePath; }
            set { _filePath = value; NotifyPropertyChanged("FilePath"); }
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

	public sealed partial class MainPage : Page {

        // live statistics
        internal Statistics _statistics { get; set; } = new Statistics();
        internal QueueObject qObj { get; set; } = new QueueObject();
        internal DateTime StartTime;

        internal ObservableCollection<QueueObj> queueObjs = new ObservableCollection<QueueObj>();


        public MainPage() {
            this.InitializeComponent();

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(400, 400));
            ApplicationView.PreferredLaunchViewSize = new Size(500, 500);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }


        // *******************************************************************
        // Start button click
        private void StartToggle_Click(object sender, RoutedEventArgs e) {
            InitialFilesAccess();
            StartToggle.IsEnabled = false;
            StartTime = DateTime.Now;
        }

        // *******************************************************************
        //
        private async void InitialFilesAccess() {
            string path = "C:\\Users\\ismae\\Desktop\\Practical.Work3\\rand_files";
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);

            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync(CommonFileQuery.OrderByName, 0, 16);


            PCQueue q = new PCQueue(_statistics.MaxThreads);

            for (int i = 0; i < files.Count - 1; i++) {
                int ii = i;
                string text = await FileIO.ReadTextAsync(files[ii]);

                q.EnqueueItem(() => {
                    Debug.WriteLine(string.Format("************** FILE Nº{0} QUEUED", ii));
                    ProcessFile(files[ii].Name, text);
                });
            }

            q.Shutdown(false);
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
        private void ProcessFile(string fileName, string text) {
            string[] lines = text.Split('\n');
            int totalLines = lines.Length;

            Debug.WriteLine(string.Format(">>>>>>>>>>>>>> File {0} started", fileName));
            for (int i = 0; i < totalLines - 1; i++) { // lines.Length
                int n = int.Parse(lines[i]);
                bool prime = IsPrime(n);

                if (prime) {
                    if (n > _statistics.MaxPrime) {
                        _statistics.MaxPrime = n;
                        Debug.WriteLine(string.Format("  - File {0} line {1}: {2} MAX", fileName, i, n));
                    }
                    if (n < _statistics.MinPrime) {
                        _statistics.MinPrime = n;
                        Debug.WriteLine(string.Format("  - File {0} line {1}: {2} MIN", fileName, i, n));
                    }
                }
                Thread.Sleep(50);

				//Debug.WriteLine(string.Format("  - File {0} line {1}: {2} prime: {3}", index, i, n, prime));
            }
            Debug.WriteLine(string.Format("<<<<<<<<<<<<<< File {0} finished", fileName));
            _statistics.LastFile = fileName;
            _statistics.Processed += 1;
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




