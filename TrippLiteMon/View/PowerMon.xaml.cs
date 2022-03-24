using DataTools.Scheduler;

using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace TrippLite
{
    public partial class PowerMon : Window
    {
        private TrippLiteViewModel vm;

        public TrippLiteViewModel ViewModel
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return vm;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (vm != null)
                {
                    vm.PowerStateChanged -= OnPowerStateChanged;
                    vm.ViewModelInitialized -= OnViewModelInitialized;
                }

                vm = value;
                if (vm != null)
                {
                    vm.PowerStateChanged += OnPowerStateChanged;
                    vm.ViewModelInitialized += OnViewModelInitialized;
                }
            }
        }

        public event OpenCoolWindowEventHandler OpenCoolWindow;

        public delegate void OpenCoolWindowEventHandler(object sender, EventArgs e);

        public TrippLiteUPS TrippLite
        {
            get
            {
                return Monitor.TrippLite;
            }
        }

        public PowerMon()
        {
            this.InitializeComponent();
            
            ViewModel = Monitor.ViewModel;
            
            OpenPower.MouseUp += OpenPowerClick;
            OpenCool.MouseUp += OpenCoolClick;
            
            StartupCheck.IsChecked = TaskTool.GetIsEnabled();

            StartupCheck.Click += StartupCheck_Click;
            StartupCheck.Checked += StartupCheck_Checked;
            StartupCheck.Unchecked += StartupCheck_Unchecked;
            
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;

            this.Left = screenWidth / 2d - windowWidth / 2d;
            this.Top = screenHeight / 2d - windowHeight / 2d;

            if (ViewModel.Initialized)
            {
                initVars();
            }
        }

        private void StartupCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            e.Handled = true; 

        }

        private void StartupCheck_Checked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void StartupCheck_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ViewModel.RunOnStartup = !ViewModel.RunOnStartup;

            Dispatcher.BeginInvoke(() =>
            {
                StartupCheck.IsChecked = ViewModel.RunOnStartup;
            });
        }

        private void OnPowerStateChanged(object sender, PowerStateChangedEventArgs e)
        {
            if (e.NewState == PowerStates.Utility)
            {
                ViewModel.MakeLoadBarProperty(ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.OutputLoad));
            }
            else
            {
                ViewModel.MakeLoadBarProperty(ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.ChargeRemaining));
            }
        }

        private void OnViewModelInitialized(object sender, EventArgs e)
        {
            initVars();
        }

        private void initVars()
        {
            DataContext = ViewModel;
        }

        private void OpenPowerClick(object sender, MouseButtonEventArgs e)
        {
            var h = new System.Windows.Interop.WindowInteropHelper(this);
            TrippLiteUPS.OpenSystemPowerOptions(h.Handle);
        }

        private void OpenCoolClick(object sender, MouseButtonEventArgs e)
        {
            OpenCoolWindow?.Invoke(this, new EventArgs());
        }
    }
}