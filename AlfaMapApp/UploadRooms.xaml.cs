using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using AlfaMap.Converter2d;
using AlfaMap.Coworking;

namespace AlfaMap
{
    public partial class UploadRooms : Window
    {
        private readonly INotifyPropertyChanged viewModel;
        public UploadRooms(INotifyPropertyChanged vm)
        {
            InitializeComponent();
            viewModel = vm;
            this.DataContext = viewModel;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            //bool success = await (viewModel as UploadRoomsViewModel).ConvertAndUpload();
            bool success = (viewModel as UploadRoomsViewModel).ConvertAndSave();
            if(success)
                Close();
        }
    }
}
