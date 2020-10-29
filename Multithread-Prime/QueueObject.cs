using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multithread_Prime {
	class QueueObject {
        public int Index { get; set; }
        public int TotalLines { get; set; }
        public int DoneLines { get; set; } = 0;
        public int Progress { get; set; } = 0;
        public string FilePath { get; set; } = "none";
    }
}
