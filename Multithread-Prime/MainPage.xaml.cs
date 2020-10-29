using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Multithread_Prime {

	public class QueueObj{
        public int Index { get; set; }
        public int TotalLines { get; set; }
        public int DoneLines { get; set; } = 0;
        public int Progress { get; set; } = 0;
        public string FilePath { get; set; } = "none";
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
                string text = await FileIO.ReadTextAsync(files[i]);

                q.EnqueueItem(() => {
                    Debug.WriteLine(string.Format("> FILE Nº{0} QUEUED", ii));
                    ProcessFile(files[ii].Name, text, ii);
                    queueObjs.Add(new QueueObj() {
                        FilePath = "test",
                        Index = 0,
                        Progress = 20,
                        TotalLines = 100,
                        DoneLines = 20
                    });
                });
            }

            for (int i = 0; i < _statistics.MaxThreads; i++) {
                q.EnqueueItem(null);
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
        private void ProcessFile(string name, string text, int index) {
            string[] lines = text.Split('\n');

            Debug.WriteLine(string.Format(">>>>>>>>>>>>>> File {0} started", index));
            for (int i = 0; i < 100 - 1; i++) { // lines.Length
                int n = int.Parse(lines[i]);
                bool prime = IsPrime(n);

				if (prime) {
                    if (n > _statistics.MaxPrime) {
                        _statistics.MaxPrime = n;
                        Debug.WriteLine(string.Format("  - File {0} line {1}: {2} MAX", name, i, n));
                    }
                    if (n < _statistics.MinPrime) {
                        _statistics.MinPrime = n;
                        Debug.WriteLine(string.Format("  - File {0} line {1}: {2} MIN", name, i, n));
                        //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        //    MinTextBlock.Text = string.Format("Min: {0}", n);
                        //});
                    }
                }
                Thread.Sleep(10);
				//thrObj.Progress = (int)(((float)thrObj.DoneLines / thrObj.TotalLines) * 100);

				//Debug.WriteLine(string.Format("  - File {0} line {1}: {2} prime: {3}", index, i, n, prime));
            }
            Debug.WriteLine(string.Format("<<<<<<<<<<<<<< File {0} finished", index));
            _statistics.LastFile = name;
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




