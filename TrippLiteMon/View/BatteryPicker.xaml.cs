using System;
using System.Collections.Generic;
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

using TrippLite.ViewModel;

namespace TrippLite
{
    /// <summary>
    /// Interaction logic for BatteryPicker.xaml
    /// </summary>
    public partial class BatteryPicker : Window
    {

        private BatteryPickerViewModel vm;

        public BatteryPicker(bool multiSelect)
        {
            vm = new BatteryPickerViewModel(multiSelect);
            DataContext = vm;

            InitializeComponent();
            this.Loaded += BatteryPicker_Loaded;
        }

        public BatteryPicker() : this(false)
        {
        }
                
        private void BatteryPicker_Loaded(object sender, RoutedEventArgs e)
        {
            vm.SetEnabled(Settings.PowerDevices);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {

            Settings.PowerDevices = vm.GetEnabled().ToArray();
            
            DialogResult = vm.OneSelected;
            Close();
        }
    }
}
