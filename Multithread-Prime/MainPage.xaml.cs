using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Multithread_Prime {

	public sealed partial class MainPage : Page {

        // live statistics
        internal Statistics stats { get; set; } = new Statistics();

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
        }

        // *******************************************************************
        // Read all the files and queue them
        private async void InitialFilesAccess() {
            string path = "C:\\Users\\ismae\\Desktop\\Practical.Work3\\rand_files";
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);

            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync(CommonFileQuery.OrderByName, 0, 30);


            PCQueue q = new PCQueue(stats.MaxThreads);

            for (int i = 0; i < files.Count - 1; i++) {
                int ii = i;
                string text = await FileIO.ReadTextAsync(files[ii]);

                q.EnqueueItem(() => {
                    Debug.WriteLine(string.Format("*File Nº{0} queued", ii));
                    ProcessFile(files[ii].Name, text);
                });
            }

            q.Shutdown(false);
        }


        // *******************************************************************
        // Alter the thread count
		private void IncreaseThreadPool(object sender, RoutedEventArgs e) {
            stats.MaxThreads += 1;
		}

        private void DecreaseThreadPool(object sender, RoutedEventArgs e) {
            if(stats.MaxThreads > 1) {
                stats.MaxThreads -= 1;
            }
        }



        // *******************************************************************
        // File handler and prime checker
        private void ProcessFile(string fileName, string text) {
            string[] lines = text.Split('\n');
            int totalLines = lines.Length;




            QueueObj q = DispatcherHelper.ExecuteOnUIThreadAsync<QueueObj>(() => {
                QueueObj obj = new QueueObj() {
                    FilePath = fileName,
                    Running = true,
                    TotalLines = lines.Length
                };

                queueObjs.Add(obj);
                return obj;
            }).Result;


			Debug.WriteLine(string.Format("\n>>>>>>>>>>>>>> File {0} started", fileName));
            for (int i = 0; i < totalLines - 1; i++) {
                int n = int.Parse(lines[i]);
                bool prime = IsPrime(n);
                q.DoneLines += 1;
                q.Progress = (int)(((float)q.DoneLines / totalLines) * 100);

                if (prime) {
                    if (n > stats.MaxPrime) {
                        stats.MaxPrime = n;
                        Debug.WriteLine(string.Format("  - File {0} line {1}: {2} MAX", fileName, i, n));
                    }
                    if (n < stats.MinPrime) {
                        stats.MinPrime = n;
                        Debug.WriteLine(string.Format("  - File {0} line {1}: {2} MIN", fileName, i, n));
                    }
                }
                Thread.Sleep(10);

				//Debug.WriteLine(string.Format("  - File {0} line {1}: {2} prime: {3}", index, i, n, prime));
            }
            Debug.WriteLine(string.Format("<<<<<<<<<<<<<< File {0} finished", fileName));
            DispatcherHelper.ExecuteOnUIThreadAsync<int>(() => {
                queueObjs.Remove(q);
                return 1;
            });
            stats.LastFile = fileName;
            stats.Processed += 1;
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




