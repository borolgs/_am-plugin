using System;

namespace AlfaMap.DataSync {
    public enum DataSyncExceptionCode {
        NotFound,
        App,
        Unexpected
    }

    public class DataSyncException : Exception {
        public DataSyncExceptionCode Code = DataSyncExceptionCode.Unexpected;


        public DataSyncException(string message) : base(message) {
            Code = DataSyncExceptionCode.App;
        }

        public DataSyncException(string message, DataSyncExceptionCode code) : base(message) {
            Code = code;
        }
        public DataSyncException(string message, Exception inner, DataSyncExceptionCode code = DataSyncExceptionCode.Unexpected) : base(message, inner) {
            Code = code;
        }
        public DataSyncException(Exception inner) : base("Unexpected DataSync Exception", inner) {
            Code = DataSyncExceptionCode.Unexpected;
        }
    }
}
