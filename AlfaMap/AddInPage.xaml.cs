using AlfaMap.Common;
using Autodesk.Revit.UI;
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
    public partial class AddInPage : Page, IDockablePaneProvider
    {
        private readonly MainViewModel viewModel;
        private SettingsWindow settingsWindow;
        private bool settingsWindowOpened = false;

        public AddInPage(MainViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = this.viewModel;
            ReloadAppIcon.Source = ImageUtils.GetImage(Properties.Resources.renew.GetHbitmap());
            SettingsIcon.Source = ImageUtils.GetImage(Properties.Resources.settings.GetHbitmap());
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this as FrameworkElement;
            data.InitialState = new DockablePaneState();
            data.InitialState.DockPosition = DockPosition.Tabbed;
            data.InitialState.TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e) {
            if (settingsWindow == null || !settingsWindow.IsActive) {
                settingsWindow = new SettingsWindow(viewModel);
                settingsWindow.Show();
                settingsWindowOpened = true;
            }
        }
    }
}
