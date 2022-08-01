using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Converter
{
    public class THREERoot
    {
        public readonly Dictionary<string, object> metadata = new Dictionary<string, object>()
        {
            { "version", 4.3 },
            { "type", "Object" },
            { "generator", "RVTtoJSON" }
        };
        public List<THREEGeometry> geometries { get; set; } = new List<THREEGeometry>();
        public List<THREEMaterial> materials { get; set; } = new List<THREEMaterial>
        {
            THREEMaterial.DefaultMaterial(),
            THREEMaterial.DefaultTransparentMaterial(),
        };

        public readonly THREEGroup @object = new THREEGroup {
            name = "building",
            matrix = Matrix.Identity().ToArray(), // Matrix.RotationY(Math.PI/2).ToArray()
        };
    }
}
