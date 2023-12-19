using AlfaMap.Common;
using Microsoft.Win32;
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
    public partial class SettingsWindow : Window
    {
        private readonly MainViewModel viewModel;

        public SettingsWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = this.viewModel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void SelectPathButton_Click(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Libraries (*.dll)|*.*";

            if(dialog.ShowDialog() == true) {
                viewModel.ConfigAppPath = dialog.FileName;
            }
        }

        private void LoadAppButton_Click(object sender, RoutedEventArgs e) {
            viewModel.RunReloadAppCommand.Execute(null);
            Close();
        }
    }
}
