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
        private readonly AppViewModel viewModel;

        public SettingsWindow(AppViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = this.viewModel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
