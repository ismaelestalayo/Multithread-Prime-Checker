using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Multithread_Prime {

	public sealed partial class MainPage : Page {

        // live statistics
        internal Statistics stats { get; set; } = new Statistics();
        private DateTime StartTime, StopTime;
        private int TotalFileNumber;
        
        // Queue of threads as a List<Thread> inside
        internal PCQueue queue;

        // List of threads/files to keep track of progress
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
            StartTime = DateTime.Now;
        }

        // *******************************************************************
        // Read all the files and queue them
        private async void InitialFilesAccess() {
            string path = "C:\\Users\\ismae\\Desktop\\Practical.Work3\\rand_files";

			try {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync(CommonFileQuery.OrderByName);
                TotalFileNumber = files.Count;
                StartToggle.IsEnabled = false;


                queue = new PCQueue(stats.MaxThreads);
                for (int i = 0; i < files.Count; i++) {
                    int ii = i;
                    string text = await FileIO.ReadTextAsync(files[ii]);

                    queue.EnqueueItem(() => {
                        Debug.WriteLine(string.Format("*File Nº{0} queued", ii));
                        ProcessFile(files[ii].Name, text);
                    });
                }

                // False not to wait for workers to finish
                queue.Shutdown(false);
            } catch(Exception ex) {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }


        // *******************************************************************
        // Alter the thread count
		private void IncreaseThreadPool(object sender, RoutedEventArgs e) {
            stats.MaxThreads += 1;
            if (queue != null)
                queue.AddWorker();
		}

        private void DecreaseThreadPool(object sender, RoutedEventArgs e) {
            if(stats.MaxThreads > 1) {
                stats.MaxThreads -= 1;
                if (queue != null)
                    queue.RemoveWorker();
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
                //Thread.Sleep(10);
            }
            
            Debug.WriteLine(string.Format("<<<<<<<<<<<<<< File {0} finished", fileName));
            DispatcherHelper.ExecuteOnUIThreadAsync<int>(() => {
                queueObjs.Remove(q);
                return 1;
            });
            stats.LastFile = fileName;
            stats.Processed += 1;

            // If all files have been processed
            if (stats.Processed == TotalFileNumber) {
                StopTime = DateTime.Now;
                TimeSpan offset = StopTime - StartTime;
                DispatcherHelper.ExecuteOnUIThreadAsync<int>(() => {
                    string message = string.Format("Start: {0} \n", StartTime);
                    message += string.Format("Finish: {0} \n", StopTime);
                    message += string.Format("Offset: {0} seconds\n", offset.TotalSeconds);
                    var _ = new MessageDialog(
                        message,
                        string.Format("Processed {0} files with {1} threads", TotalFileNumber, stats.MaxThreads)
                    ).ShowAsync();
                    return 1;
                });
                for (int i = 0; i < stats.MaxThreads; i++)
                    queue.RemoveWorker();

            }
                
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


