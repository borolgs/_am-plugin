using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Converter
{
    public abstract class THREEObject
    {
        public abstract string type { get; }
        public abstract string name { get; set; }
        public abstract string uuid { get; set; }
        public abstract double[] matrix { get; set; }
       //public abstract Dictionary<string, object> userData { get; set; }
        public abstract THREEUserData userData { get; set; }
    }
}
