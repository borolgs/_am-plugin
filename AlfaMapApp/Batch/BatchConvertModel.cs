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

    public class CoworkingRoomEntity : ViewModelBase {
        public bool selected { get; set; }
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                OnPropertyChanged();
            }
        }
        public int Id { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public int BuildingId { get; set; }
        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                OnPropertyChanged();
            }
        }
    }
}
