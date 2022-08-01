using Autodesk.Revit.DB;
using RevitWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Converter
{
    public class THREEGroup : THREEObject
    {
        public override string type => "Group";
        public override string name { get; set; }
        public override string uuid { get; set; }
        public override double[] matrix { get; set; } = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        public override THREEUserData userData { get; set; }
        public List<THREEObject> children { get; set; } = new List<THREEObject>();

        public THREEGroup() { }

        public static THREEGroup Create(string name, ElementType type, double? elevation = null)
        {
            var group = new THREEGroup
            {
                name = name,
                userData = new THREEGroupData
                {
                    type = type,
                    elevation = elevation,
                }
            };

            return group;
        }

        public static THREEGroup FromLevel(Level level)
        {
            var group = new THREEGroup
            {
                name = level.UniqueId,
                uuid = level.UniqueId,
                userData = new THREEGroupData
                {
                    type = ElementType.LevelGroup,
                    elevation = Utils.FootToMeter(level.Elevation),
                }
            };

            return group;
        }
    }
}
