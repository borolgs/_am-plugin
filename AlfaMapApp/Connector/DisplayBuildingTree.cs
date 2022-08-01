using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlfaMap.DataSync;
using AlfaMap.State;

namespace AlfaMap.Connector
{
    
    public class DisplayBuildingTree : ViewModelBase
    {
        private ObservableCollection<DisplayNode> nodes = new ObservableCollection<DisplayNode>();
        public ObservableCollection<DisplayNode> Nodes
        {
            get { return nodes; }
            set
            {
                nodes = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DisplayOffice> offices = new ObservableCollection<DisplayOffice>();
        public ObservableCollection<DisplayOffice> Offices
        {
            get { return offices; }
            set
            {
                offices = value;
                OnPropertyChanged();
            }
        }

        private bool groupByOffice = false;
        public bool GroupByOffice
        {
            get { return groupByOffice; }
            set
            {
                groupByOffice = value;
                OnPropertyChanged();
            }
        }

        private readonly string[] colors = new string[]
        {
            "#ffadad",
            "#ffd6a5",
            "#fdffb6",
            "#caffbf",
            "#9bf6ff",
            "#a0c4ff"
        };

        public void Update(BuildingTree tree)
        {
            GroupByOffice = false;
            Offices.Clear();
            for (int i = 0; i < tree.Offices.Count; i++)
            {
                Offices.Add(new DisplayOffice { Name = tree.Offices[i], Color = colors[i % colors.Length] });
            }

            Nodes.Clear();

            var queue = new Queue<DisplayNode>();
            var displayRootNode = new DisplayNode { Node = tree.Root };
            Nodes.Add(displayRootNode);
            queue.Enqueue(displayRootNode);
            while(queue.Count > 0)
            {
                DisplayNode node = queue.Dequeue();

               /* if (node.Node.OfficeId != null)
                {
                    var color = colors[tree.Offices.IndexOf(node.Node.OfficeId) % colors.Length];
                    node.Color = color;
                }*/
                
                foreach (ModelNode child in node.Node.Children)
                {
                    var childDisplayNode = new DisplayNode { Node = child };
                    queue.Enqueue(childDisplayNode);
                    node.Children.Add(childDisplayNode);
                }
            }
        }

        public void UpdateWithOffices(BuildingTree tree)
        {
            GroupByOffice = true;
            Nodes.Clear();

            ModelNode building = tree.Root;

            var displayBuildingNode = new DisplayNode { Node = building };
            Nodes.Add(displayBuildingNode);


            foreach (string officeId in tree.Offices)
            {
                var displayOfficeNode = new DisplayNode { Name = officeId };
                displayBuildingNode.Children.Add(displayOfficeNode);
            }


            var queue = new Queue<ModelNode>();

            foreach (ModelNode level in building.Children)
                queue.Enqueue(level);


            while (queue.Count > 0)
            {
                ModelNode node = queue.Dequeue();
                AddNode(node);


                foreach (ModelNode child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }

            void AddNode(ModelNode node)
            {
                var parents = node.GetParents();

                /*var displayNode = FindOffice(node.OfficeId);
                if (displayNode == null) return; //TODO*/

/*                foreach (ModelNode parent in parents.AsEnumerable().Reverse())
                {
                    //if (parent.Type == NodeType.Building) continue;
                    var parentDisplayNode = FindChild(displayNode, parent.Uuid);
                    if (parentDisplayNode == null)
                    {
                        parentDisplayNode = new DisplayNode { Node = parent };
                        displayNode.Children.Add(parentDisplayNode);
                    }
                    displayNode = parentDisplayNode;
                }
                if (displayNode == null) return; //TODO
                if(FindChild(displayNode, node.Uuid) == null)
                {
                    displayNode.Children.Add(new DisplayNode { Node = node });
                }*/
            }

            DisplayNode FindChild(DisplayNode displayNode, string key)
            {
                foreach (DisplayNode child in displayNode.Children)
                    if (child.Node.Uuid == key)
                        return child;
                return null;
            }

            DisplayNode FindOffice(string key)
            {
                foreach (DisplayNode child in displayBuildingNode.Children)
                    if (child.Name == key)
                        return child;
                return null;
            }
        }
    }

    public class DisplayNode: ViewModelBase
    {
        private string name;
        public string Name
        {
            get
            {
                if(Node == null)
                {
                    return name;
                }
                return "TODO";
/*                switch(Node.Type)
                {
                    case NodeType.Building:
                        return "Здание";
                    case NodeType.Level:
                        var level = Node.Element as Level;
                        return $"{level.Name}  {Math.Round(UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_MILLIMETERS))}";
                    case NodeType.Room:
                        return (Node.Element as Room).Name;
                    case NodeType.Workplace:
                        return Node.Element.get_Parameter(Parameters.WorkplaceNumber.Guid).AsString();
                    default:
                        return "?";
                }*/
            }
            set
            {
                name = value;
            }
        }

        public string Color { get; set; } = "#FFFFFF";

        public bool Connected => Node?.Element?.get_Parameter(OldParams.IsConnected.Guid)?.AsInteger() == 1;


        public ModelNode Node { get; set; } = null;
        public ObservableCollection<DisplayNode> Children { get; set; } = new ObservableCollection<DisplayNode>();
    }

    public class DisplayOffice
    {
        public string Color { get; set; } = "#FFFFFF";
        public string Name { get; set; }
    }
}
