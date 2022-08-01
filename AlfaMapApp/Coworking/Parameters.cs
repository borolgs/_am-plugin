using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AlfaMap.Revit;

namespace AlfaMap.Coworking {
    public static class Parameters {
        public static readonly ParamData PlaceId = new ParamData {
            Guid = new Guid("2BC5F85E-83E4-4E7F-B8F7-EA00AA3E5AE9"),
            Name = "AB_PlaceId",
            Description = "ID Площадки",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_ProjectInformation },
            AllowVary = false,
            Modifiable = true,
        };
        public static readonly ParamData NodeId = new ParamData {
            Guid = new Guid("2D5E66B9-BAD7-4B01-8542-DF3EA500852E"),
            Description = "ID узла справочника территорий.",
            Name = "AB_NodeId",
            Type = ParameterType.Integer,
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Furniture, BuiltInCategory.OST_Rooms },
            Modifiable = true,
        };
        public static readonly ParamData WorkplaceNumber = new ParamData {
            Guid = new Guid("5223019d-5db1-441b-bad9-aa4cc62bf25e"),
            Description = "Номер рабочего места. Пример: 430а. Обязателен для заполнения.",
            Name = "AB_W_Number",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Furniture },
            Modifiable = true,
        };
        
        public static readonly ParamData IsWorkplaces = new ParamData {
            Guid = new Guid("451f8937-ab5f-40cb-afc2-28217a952ff7"),
            Description = "Да если типоразмер семейтсва является рабочим местом. Обязателен для заполнения.",
            Instance = false,
            Name = "AB_W_IsWorkplace",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Furniture },
            Type = ParameterType.YesNo,
            Modifiable = true,
        };
        public static readonly ParamData DroId = new ParamData {
            Guid = new Guid("2D5E66B9-BAD7-4B01-8542-DF3EA500852E"),
            Description = "Номер рабочего места. Пример: 430а. Обязателен для заполнения.",
            Name = "AB_DRO_Id",
            Type = ParameterType.Integer,
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Furniture, BuiltInCategory.OST_Rooms },
            Modifiable = true,
        };
        public static List<ParamData> GetAll() {
            return typeof(Parameters)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(f => f.FieldType == typeof(ParamData))
                .Select(f => f.GetValue(null) as ParamData).ToList();
        }
    }
}
