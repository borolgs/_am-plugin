using AlfaMap.Common;
using Autodesk.Revit.UI;
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

namespace AlfaMap
{
    /// <summary>
    /// Interaction logic for AddIn.xaml
    /// </summary>
    public partial class HostPage : Page, IDockablePaneProvider
    {
        private readonly HostViewModel viewModel;

        public HostPage(HostViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = this.viewModel;
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this as FrameworkElement;
            data.InitialState = new DockablePaneState();
            data.InitialState.DockPosition = DockPosition.Tabbed;
            data.InitialState.TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser;
        }
    }
}
