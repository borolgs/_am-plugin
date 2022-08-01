#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
#endregion

namespace AlfaMap.Revit
{

    public class ParamCreator
    {
        private readonly DefinitionFile definitionFile;
        private readonly Document doc;
        public ParamCreator(Document doc)
        {
            this.doc = doc;
            definitionFile = doc.Application.OpenSharedParameterFile();
        }

        public void Create(ParamData paramData)
        {
            if (definitionFile == null) throw new Exception("No def file!");

            DefinitionGroup group = definitionFile.Groups.get_Item(paramData.SharedGroupName);
            if (group == null) definitionFile.Groups.Create(paramData.SharedGroupName);

            Definition definition;
            if (group.Definitions.Contains(group.Definitions.get_Item(paramData.Name)))
            {
                definition = group.Definitions.get_Item(paramData.Name);
            }
            else
            {
                var createOptions = new ExternalDefinitionCreationOptions(paramData.Name, paramData.Type);
                createOptions.Visible = paramData.Visible;
                createOptions.UserModifiable = paramData.Modifiable;
                createOptions.Description = paramData.Description;

                if (paramData.Guid != null)
                {
                    createOptions.GUID = paramData.Guid;
                }
                try
                {
                    definition = group.Definitions.Create(createOptions);
                }
                catch (Exception e)
                {
                    throw new CreateParamException($"Duplicate GUID: {paramData.Guid}", e);
                }
            }

            CategorySet cats = doc.Application.Create.NewCategorySet();
            foreach (BuiltInCategory category in paramData.Categories)
            {
                cats.Insert(doc.Settings.Categories.get_Item(category));
            }

            ElementBinding bind;
            if (paramData.Instance)
            {
                bind = doc.Application.Create.NewInstanceBinding(cats);
            }
            else
            {
                bind = doc.Application.Create.NewTypeBinding(cats);
            }

            doc.ParameterBindings.Insert(definition, bind, paramData.ParameterGroup);

            if (paramData.AllowVary)
            {
                var guid = ((ExternalDefinition)group.Definitions.get_Item(paramData.Name)).GUID;
                SharedParameterElement sharedParam = SharedParameterElement.Lookup(doc, guid);
                sharedParam.GetDefinition().SetAllowVaryBetweenGroups(doc, true);
            }
        }
    }

    public class CreateParamException : Exception
    {
        public CreateParamException() { }
        public CreateParamException(string message) : base(message) { }
        public CreateParamException(string message, Exception inner) : base(message, inner) { }
    }

    public class ParamData
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public List<BuiltInCategory> Categories { get; set; }
        public string SharedGroupName { get; set; } = "AB";
        public ParameterType Type { get; set; } = ParameterType.Text;
        public string Description { get; set; } = "";
        public BuiltInParameterGroup ParameterGroup { get; set; } = BuiltInParameterGroup.PG_TEXT;
        public bool Instance { get; set; } = true;
        public bool AllowVary { get; set; } = false;
        public bool Modifiable { get; set; } = false;
        public bool Visible { get; set; } = true;
    }
}
