using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AlfaMap.Connector;
using AlfaMap.Properties;
using AlfaMap.DataSync;

namespace AlfaMap
{
    /// <summary>
    /// Interaction logic for AddIn.xaml
    /// </summary>
    public partial class UI : UserControl
    {
        public MainViewModel ViewModel { get; private set; }

        public UI(MainViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel; // new MainViewModel();
            this.DataContext = ViewModel;
            CheckImage.Source = ImageUtils.GetImage(Properties.Resources.check.GetHbitmap());
            ConnectImage.Source = ImageUtils.GetImage(Properties.Resources.connect.GetHbitmap());
            LoadImage.Source = ImageUtils.GetImage(Properties.Resources.download.GetHbitmap());
            //ReniewTreeIcon.Source = ImageUtils.GetImage(Properties.Resources.renew.GetHbitmap());
            //UploadNewVersion.Source = ImageUtils.GetImage(Properties.Resources.upload.GetHbitmap());
            UploadNewVersion2.Source = ImageUtils.GetImage(Properties.Resources.upload.GetHbitmap());
        }

        private void DeleteBuildingButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            // TODO: delete method
        }

        private void DeleteBuildingVersionsButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            // TODO: delete method
        }

        private void LoadBuildingButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            // TODO: delete method
        }

        private void SetCurrentButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO Delete button
            var button = sender as Button;
            // TODO: delete method
        }

        private void DeleteVersionButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            // TODO: delete method
        }

        private void CreateVersionButton_Click(object sender, RoutedEventArgs e)
        {
            new UploadModelDialog(this.ViewModel).Show();
            //DescriptionTextBox.Clear();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var linkForm = new LinkBulding(ViewModel);
            linkForm.Show();
        }

        private void BuildingTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            /*var tree = sender as TreeView;
            var selectedItem = tree.SelectedItem as DisplayNode;
            if (selectedItem != null)
                ViewModel.RaiseSelectNode(selectedItem.Node);*/
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem)
            {
                var treeItem = sender as TreeViewItem;
                if (!treeItem.IsSelected)
                    return;
                var node = treeItem.DataContext as DisplayNode;
                if(node != null)
                    ViewModel.RaiseSelectNodeBranch(node.Node);

            }
        }


        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tree = sender as ListBox;
            var selectedItem = tree.SelectedItem as DisplayOffice;
            if (selectedItem != null)
                ViewModel.RaiseSelectOfficeBranch(selectedItem.Name);
        }

        private void SyncResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var tree = sender as ListBox;
            var selectedItem = tree.SelectedItem as SyncNodeInfo;
            if (selectedItem != null && selectedItem.ElementId != null)
                ViewModel.RaiseSelectElement(selectedItem.ElementId);
        }

        private void NodeLinkButton_Click(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            var data = button.DataContext as SyncNodeInfo;
            if (data.NodeId.Value > 0) {
                Process.Start(@"http://s06dro.regions.alfaintra.net:3000/#/tree78/id/" + data.NodeId.Value);
            }
        }
    }
}
