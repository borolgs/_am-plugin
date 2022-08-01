using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AlfaMap.Converter2d;
using AlfaMap.DataSync;
using AlfaMap.State;

namespace AlfaMap.Coworking
{
    class UploadRoomsViewModel : ViewModelBase
    {
        private ObservableCollection<RoomItem> items;
        public ObservableCollection<RoomItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView ItemsView
        {
            get
            {
                //var view = new CollectionViewSource { Source = Items }.View;
                var view = CollectionViewSource.GetDefaultView(Items);
                //view.Filter = item => !(item as Item).Checked;
                return view;
            }
        }

        //public ICollectionView CheckedItemsView
        //{
        //    get
        //    {
        //        var view = new CollectionViewSource { Source = Items }.View;
        //        //view.Filter = item => (item as Item).Checked;
        //        return view;
        //    }
        //}

        private string search;

        public string Search
        {
            get { return search; }
            set
            {
                search = value;
                OnPropertyChanged();
                ItemsView.Refresh();
            }
        }

        private bool Filter(RoomItem item)
        {
            return Search == null
                || item.Name.IndexOf(Search, StringComparison.OrdinalIgnoreCase) != -1
                || item.Checked;
        }

        public UploadRoomsViewModel(List<ModelNode> roomNodes)
        {
            Items = new ObservableCollection<RoomItem>();
            foreach (ModelNode node in roomNodes)
            {
                // TODO!!!!
                var roomItem = new RoomItem { Name = node.NodePartialName, Room = node };
                Items.Add(roomItem);
            }
            ItemsView.Filter = new Predicate<object>(o => Filter(o as RoomItem));
            ItemsView.SortDescriptions.Add(new SortDescription("Checked", ListSortDirection.Descending));
        }

        public async Task<bool> ConvertAndUpload()
        {
            var converter = new SVGConverter();
            var client = new CoworkingClient();
            try
            {
                foreach (RoomItem item in Items.Where(item => item.Checked))
                {
                    string svg = converter.Convert(item.Room); //.Replace("\r\n", "");
                    var id = item.Room.Element.get_Parameter(Parameters.DroId.Guid)?.AsInteger() ?? 8; // TODO!
                    await client.UploadPlan(id, new PlanDTO { NodeId = id, Svg = svg });
                }
                
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public bool ConvertAndSave() {
            var converter = new SVGConverter();
            var client = new CoworkingClient();
            try {
                foreach (RoomItem item in Items.Where(item => item.Checked)) {
                    string svg = converter.Convert(item.Room);
                    string filename = item.Name + ".svg";
                    string filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);
                    File.WriteAllText(filepath, svg);
                }

                return true;
            } catch (Exception e) {
                return false;
            }
        }
    }

    public class RoomItem : ViewModelBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        private bool @checked = false;
        public bool Checked
        {
            get { return @checked; }
            set
            {
                @checked = value;
                OnPropertyChanged();
            }
        }
        public ModelNode Room { get; set; }
        public string SVG { get; set; }
    }
}
