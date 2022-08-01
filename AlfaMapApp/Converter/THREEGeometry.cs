using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Converter
{
    public class THREEGeometry
    {
        public string uuid { get; set; }
        public readonly string type = "BufferGeometry";
        public Dictionary<string, object> data { get; set; }
    }

    public class THREEGeometryGroup
    {
        public int start { get; set; }
        public int materialIndex { get; set; }
        public int count { get; set; }
    }

    public class THREEGeometryVectorArray
    {
        public readonly int itemSize = 3;
        public readonly string type = "Float32Array";
        public List<double> array { get; set; }
    }
}
