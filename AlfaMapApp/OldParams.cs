#region Namespaces
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Resources;
using System.Reflection;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using WPF = System.Windows;
using System.Linq;
using AlfaMap.Revit;
#endregion

namespace AlfaMap
{

    public static class OldParams
    {
        #region Tree Parameters
        public static readonly ParamData OfficeKey = new ParamData
        {
            Guid = new Guid("f754059b-2ff0-434e-afa2-73fb59eee8e4"),
            Name = "AB_Office_Key",
            Description = "Наименование офиса из ДРО. Пример: НГ-2. Если не заполнен берется из родительского объекта.",
            Categories = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_ProjectInformation,
                BuiltInCategory.OST_Levels,
                BuiltInCategory.OST_Rooms,
                BuiltInCategory.OST_Furniture,
            },
            Modifiable = true,
        };
        public static readonly ParamData BuildingId = new ParamData
        {
            Guid = new Guid("65add508-b8fb-4023-af29-4a55b0d22aad"),
            Name = "AB_Building_Id",
            Description = "ID Здания. Заполняется автоматически.",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_ProjectInformation },
            AllowVary = false,
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
        public static readonly ParamData WorkplaceNumber = new ParamData
        {
            Guid = new Guid("5223019d-5db1-441b-bad9-aa4cc62bf25e"),
            Description = "Номер рабочего места. Пример: 430а. Обязателен для заполнения.",
            Name = "AB_W_Number",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Furniture },
            Modifiable = true,
        };
        public static readonly ParamData LevelNumber = new ParamData
        {
            Guid = new Guid("8B1D254E-EE69-438D-8983-2ED3ADDBB6F3"),
            Description = "Номер этажа. Пример: для 'ВЦ-1-Этаж' параметр должен быть заполнен как '1'. Если не заполнен - будет исользоваться порядковый номер по отметке.",
            Name = "AB_L_Number",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Levels },
            Modifiable = true,
        };
        public static readonly ParamData IsWorkplaces = new ParamData
        {
            Guid = new Guid("451f8937-ab5f-40cb-afc2-28217a952ff7"),
            Description = "Да если типоразмер семейтсва является рабочим местом. Обязателен для заполнения.",
            Instance = false,
            Name = "AB_W_IsWorkplace",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Furniture },
            Type = ParameterType.YesNo,
            Modifiable = true,
        };
        #endregion
        #region Analysis
        public static readonly ParamData IsConnected = new ParamData
        {
            Guid = new Guid("CF4E2BB7-C561-402C-948D-A563699A9448"),
            Name = "AB_Connected",
            Description = "Данные получены",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Levels, BuiltInCategory.OST_Furniture },
            Type = ParameterType.YesNo
        };
        public static readonly ParamData CurrentEmployeeCount = new ParamData
        {
            Guid = new Guid("641ec324-2076-4016-bae1-955ee0b1fc57"),
            Name = "AB_DRO_EmployeeCount",
            Description = "Кол-во сотрудников согласно данным из BW. Заполняется автоматически.",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Levels, BuiltInCategory.OST_Furniture },
            Type = ParameterType.Integer
        };
        public static readonly ParamData CurrentArea = new ParamData
        {
            Guid = new Guid("c1d2a380-a1bb-4f0a-82b5-c745a332bdda"),
            Name = "AB_DRO_Area",
            Description = "Площадь согласно данным из BW. Заполняется автоматически.",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Levels },
            Type = ParameterType.Area
        };
        public static readonly ParamData CurrentWorkplaceCount = new ParamData
        {
            Guid = new Guid("33ebd88d-1980-457f-9689-44d4ed21dfe9"),
            Name = "AB_DRO_WorkplaceCount",
            Description = "Кол-во РМ согласно данным из BW. Заполняется автоматически.",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Levels },
            Type = ParameterType.Integer
        };
        public static readonly ParamData ModelEmployeeCount = new ParamData
        {
            Guid = new Guid("D4CA9DA4-D49D-42F2-AF5E-7AE2C4EDF8FC"),
            Name = "AB_Model_EmployeeCount",
            Description = "Кол-во сотрудников. Заполняется автоматически.",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Levels, BuiltInCategory.OST_Furniture },
            Type = ParameterType.Integer
        };
        public static readonly ParamData ModelArea = new ParamData
        {
            Guid = new Guid("8013B1D2-FEBD-4EC2-ABD7-9238B0CCDCE8"),
            Name = "AB_Model_Area",
            Description = "Площадь. Заполняется автоматически.",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Levels },
            Type = ParameterType.Area
        };
        public static readonly ParamData ModelWorkplaceCount = new ParamData
        {
            Guid = new Guid("87950C97-0A3B-4636-83E7-C331BEA476FC"),
            Name = "AB_Model_WorkplaceCount",
            Description = "Кол-во РМ. Заполняется автоматически.",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Levels },
            Type = ParameterType.Integer
        };

        public static readonly ParamData UserNames = new ParamData
        {
            Guid = new Guid("6d357a51-1421-4a51-9aeb-ddea50e5fca5"),
            Name = "AB_W_UNames",
            Description = "Список имен сотрудников закрепленных за этим РМ. Заполняется автоматически.",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Furniture },
            Type = ParameterType.Text
        };
        public static readonly ParamData UserLogins = new ParamData
        {
            Guid = new Guid("B1246776-567A-4C07-8671-00337E3D684D"),
            Name = "AB_W_ULogins",
            Description = "Список логинов сотрудников закрепленных за этим РМ. Заполняется автоматически.",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Furniture },
            Type = ParameterType.Text
        };
        public static readonly ParamData UserDepartments = new ParamData
        {
            Guid = new Guid("aef1a5d7-4b11-47ca-9d23-789035059a11"),
            Name = "AB_W_UDepartments",
            Description = "Список отделов сотрудников закрепленных за этим РМ. Заполняется автоматически.",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Furniture },
            Type = ParameterType.Text
        };
        #endregion
        public static readonly ParamData ElementData = new ParamData
        {
            Guid = new Guid("49ED11AD-7316-41C4-9F99-42669BAA5BE4"),
            Name = "AB_ElementData",
            Description = "Serialised element data from BW",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Levels, BuiltInCategory.OST_Furniture },
            Type = ParameterType.Text,
            Visible = true,
            AllowVary = true,
            Modifiable = false
        };

        public static readonly ParamData CoworkingType = new ParamData
        {
            Guid = new Guid("5CFE2565-459A-4F7B-865B-B4AA779C4238"),
            Description = "Тип коворкинга: 1 - Коворкинг, 2 - хот-деск",
            Instance = true,
            Name = "AB_W_CoworkingType",
            Categories = new List<BuiltInCategory> { BuiltInCategory.OST_Furniture },
            Type = ParameterType.Integer,
            Modifiable = true, // TEMP
        };
        public static List<ParamData> GetAll()
        {
            return typeof(OldParams)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(f => f.FieldType == typeof(ParamData))
                .Select(f => f.GetValue(null) as ParamData).ToList();
        }
    }

}
