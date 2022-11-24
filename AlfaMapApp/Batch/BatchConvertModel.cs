using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Batch {
    public class ConvertFileResult {
        public string FilePath { get; set; }
        public int BuildingId { get; set; }

        private Exception error;
        public Exception Error {
            get {
                return error;
            }
            set {
                error = value;
                Success = false;
            } 
        }

        public string ErrorMessage => Error?.Message;

        public bool Success { get; set; }
        public bool Skip { get; set; }
        public string RvtVersion { get; set; }
        public DateTime UpdatedAt { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }
}
