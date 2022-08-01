using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Converter
{
    public enum ElementType
    {
        LevelGroup,
        MultiLevelGroup,
        Room,
        Workplace,
        Entity,
        StructuralElement,
        Element,
        RoomGroup,
        WorkplaceGroup,
        EntityGroup,
        ElementGroup,
        StructuralElementGroup
    }

    public abstract class THREEUserData
    {
        public abstract ElementType type { get; set; }
        public abstract double? elevation { get; set; }
        public abstract double[] origin { get; set; }
    }

    public class THREEGroupData : THREEUserData
    {
        public override ElementType type { get; set; }
        public override double? elevation { get; set; } = null;
        public override double[] origin { get; set; } = null;
    }


    public class THREEEntityData: THREEUserData
    {
        public override ElementType type { get; set; } = ElementType.Room;
        public override double? elevation { get; set; } = null;
        public override double[] origin { get; set; } = null;
        public string name { get; set; }
    }
}
