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
using System.Web;

namespace AlfaMap.Batch {
    public partial class BatchConvertDialog : Window
    {
        private BatchConvertViewModel viewModel;
        public BatchConvertDialog(BatchConvertViewModel vm)
        {
            InitializeComponent();
            viewModel = vm;
            this.DataContext = viewModel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //Topmost = true;
        }

        private void SelectAllCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var room in viewModel.Coworkings)
            {
                room.Selected = true;
            }
        }

        private void SelectAllCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var room in viewModel.Coworkings)
            {
                room.Selected = false;
            }
        }
    }
}
