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

namespace AlfaMap
{
    public partial class UploadModelDialog : Window
    {
        private AppViewModel viewModel;
        public UploadModelDialog(AppViewModel vm)
        {
            InitializeComponent();
            viewModel = vm;
            viewModel.LinkForm.Window = this;
            this.DataContext = viewModel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void UploadModelButton_Click(object sender, RoutedEventArgs e) {
            this.viewModel.RunWorkplaceCommand.Execute(WorkplaceCommand.CreateOrOverrideActiveVersion);
            Close();
        }
    }
}
