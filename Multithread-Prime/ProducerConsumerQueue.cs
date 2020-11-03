using System;
using System.Collections.Generic;
using System.Threading;

namespace Multithread_Prime {
	class PCQueue {

        readonly object _locker = new object();
        List<Thread> _workers;
        Queue<Action> _itemQ = new Queue<Action>();

        public PCQueue(int workerCount) {
            _workers = new List<Thread>(workerCount);

            // Create and start a separate thread for each worker
            for (int i = 0; i < workerCount; i++) {
                _workers.Add(new Thread(Consume));
                _workers[i].Start();
            }
                
            //(_workers[i] = new Thread(Consume)).Start();
        }

        public void Shutdown(bool waitForWorkers) {
            // Enqueue one null item per worker to make each exit.
            foreach (Thread worker in _workers)
                EnqueueItem(null);

            // Wait for workers to finish
            if (waitForWorkers)
                foreach (Thread worker in _workers)
                    worker.Join();
        }

        public void EnqueueItem(Action item) {
            lock (_locker) {
                _itemQ.Enqueue(item);           // We must pulse because we're
                Monitor.Pulse(_locker);         // changing a blocking condition.
            }
        }

        public void AddWorker() {
            _workers.Add(new Thread(Consume));
            _workers[_workers.Count - 1].Start();
        }

        public void RemoveWorker() {
            lock (_locker) {
                if (_itemQ.Count == 1) {
                    _itemQ.Clear();
                }
                else {
                    var items = _itemQ.ToArray();
                    _itemQ.Clear();
                    _itemQ.Enqueue(null);
                    foreach (var item in items)
                        _itemQ.Enqueue(item);
                }
            }
        }

        void Consume() {
            while (true)                        // Keep consuming until
            {                                   // told otherwise.
                Action item;
                lock (_locker) {
                    while (_itemQ.Count == 0) Monitor.Wait(_locker);
                    item = _itemQ.Dequeue();
                }
                if (item == null) return;         // This signals our exit.
                item();                           // Execute item.
            }
        }
    }
}
