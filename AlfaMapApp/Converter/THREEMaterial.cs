using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Converter
{
    public class THREEMaterial
    {
        public const string DefaultMaterialUuid = "7AAB18E5-FF88-4A82-8018-4DF34EDB7539";
        public const string DefaultTransparentMaterialUuid = "023BE4CF-1D67-43E0-BB94-D4A9AD0C242F";
        public string uuid { get; set; }
        public string type { get; } = "MeshLambertMaterial";
        public string color { get; set; } = "0xffffff";
        public string emissive { get; set; } = "0x000000";
        public double opacity { get; set; } = 1;

        public static THREEMaterial DefaultMaterial()
        {
            return new THREEMaterial
            {
                uuid = DefaultMaterialUuid
            };
        }

        public static THREEMaterial DefaultTransparentMaterial()
        {
            return new THREEMaterial
            {
                uuid = DefaultTransparentMaterialUuid,
                color = "0x00ffff",
                opacity = 0.5
            };
        }
    }
}
