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
    public partial class LinkBulding : Window
    {
        private MainViewModel viewModel;
        public LinkBulding(MainViewModel vm)
        {
            InitializeComponent();
            viewModel = vm;
            viewModel.LinkForm.Window = this;
            this.DataContext = viewModel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.LinkForm.Loading = true;
        }

        private void GenerateGuidButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.LinkForm.Generate();
        }
    }
}
