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

namespace AlfaMap
{
    public partial class ErrorDialog : Window
    {
        private AppViewModel viewModel;
        public ErrorDialog(AppViewModel vm)
        {
            InitializeComponent();
            viewModel = vm;
            this.DataContext = viewModel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SendReportButton_Click(object sender, RoutedEventArgs e)
        {
            string subject = HttpUtility.UrlEncode($"AK Error. {viewModel.DocName}");
            string body = HttpUtility.UrlEncode($"{viewModel.RunException.Message}\n\n{viewModel.RunException.StackTrace}");
            string url = $"mailto:oborodatov@alfabank.ru?subject={subject}&body={body}";
            Process.Start(url);
            Close();
        }
    }
}
