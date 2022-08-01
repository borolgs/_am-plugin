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
using Autodesk.Revit.DB.ExtensibleStorage;
#endregion

namespace RevitWrapper
{

    public class Storage
    {
        private Guid schemaGuid;
        private DataStorage storage;
        private Schema schema;
        private Document doc;
        public Storage(Document document, Guid guid)
        {
            doc = document;
            schemaGuid = guid;
            schema = GetSchema();
            storage = GetStorage();
            InitDict();
        }

        private Schema GetSchema()
        {
            Schema schema = Schema.Lookup(schemaGuid);
            if (schema == null)
            {
                return CreateSchema();
            }
            return schema;
        }

        private Schema CreateSchema()
        {
            SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);
            schemaBuilder.SetSchemaName("MySchema");
            schemaBuilder.AddMapField("MapField", typeof(string), typeof(string));
            Schema schema = schemaBuilder.Finish();
            return schema;
        }

        private DataStorage GetStorage()
        {
            var storages = new FilteredElementCollector(doc).OfClass(typeof(DataStorage));
            foreach (var storage in storages)
            {
                var entity = storage.GetEntity(schema);
                if (entity != null && entity.IsValid())
                {
                    return storage as DataStorage;
                }
            }
            var newStorage = DataStorage.Create(doc);
            return newStorage;

        }

        private void InitDict()
        {
            IDictionary<string, string> dict;
            Entity entity = storage.GetEntity(schema);
            if (entity == null || !entity.IsValid())
            {
                entity = new Entity(schema);
                dict = new Dictionary<string, string>();
                entity.Set<IDictionary<string, string>>("MapField", dict);
                storage.SetEntity(entity);
                return;
            }
        }

        public void SetMap(Dictionary<string, string> dict)
        {
            var entity = storage.GetEntity(schema);
            entity.Set<IDictionary<string, string>>("MapField", dict);
            storage.SetEntity(entity);
        }

        public IDictionary<string, string> GetMap()
        {
            var dict = storage.GetEntity(schema).Get<IDictionary<string, string>>("MapField");
            if (dict is null)
            {
                InitDict();
                return dict;
            }
            return dict;
        }

        public void Set(string key, string value)
        {
            var entity = storage.GetEntity(schema);
            var dict = entity.Get<IDictionary<string, string>>("MapField");
            dict[key] = value;
            entity.Set<IDictionary<string, string>>("MapField", dict);
            storage.SetEntity(entity);
        }

        public string Get(string key)
        {
            var dict = GetMap();
            if (!dict.ContainsKey(key))
            {
                return null;
            }
            return dict[key];
        }

        public bool Has(string key)
        {
            var dict = GetMap();
            if (dict.ContainsKey(key))
            {
                return true;
            }
            return false;
        }

        public void Remove(string key)
        {
            var entity = storage.GetEntity(schema);
            var dict = entity.Get<IDictionary<string, string>>("MapField");
            if (dict.ContainsKey(key))
            {
                dict.Remove(key);
            }
            entity.Set<IDictionary<string, string>>("MapField", dict);
            storage.SetEntity(entity);
        }
    }

}
