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

        private TrippLiteViewModel _ViewModel
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
                    vm.PowerStateChanged -= _ViewModel_PowerStateChanged;
                    vm.ViewModelInitialized -= _ViewModel_ViewModelInitialized;
                }

                vm = value;
                if (vm != null)
                {
                    vm.PowerStateChanged += _ViewModel_PowerStateChanged;
                    vm.ViewModelInitialized += _ViewModel_ViewModelInitialized;
                }
            }
        }

        public event OpenCoolWindowEventHandler OpenCoolWindow;

        public delegate void OpenCoolWindowEventHandler(object sender, EventArgs e);

        public TrippLiteViewModel ViewModel
        {
            get
            {
                TrippLiteViewModel ViewModelRet = default;
                ViewModelRet = this.Monitor.ViewModel;
                return ViewModelRet;
            }
        }

        public TrippLiteUPS TrippLite
        {
            get
            {
                TrippLiteUPS TrippLiteRet = default;
                TrippLiteRet = this.Monitor.TrippLite;
                return TrippLiteRet;
            }
        }

        public PowerMon()
        {
            this.InitializeComponent();
            _ViewModel = ViewModel;
            OpenPower.MouseUp += OpenPower_MouseUp;
            OpenCool.MouseUp += OpenCool_MouseUp;
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
            if (_ViewModel.Initialized)
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
            _ViewModel.RunOnStartup = !_ViewModel.RunOnStartup;

            Dispatcher.BeginInvoke(() =>
            {
                StartupCheck.IsChecked = _ViewModel.RunOnStartup;
            });
        }

        private void _ViewModel_PowerStateChanged(object sender, PowerStateChangedEventArgs e)
        {
            if (e.NewState == PowerStates.Utility)
            {
                _ViewModel.MakeLoadBarProperty(_ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.OutputLoad));
            }
            else
            {
                _ViewModel.MakeLoadBarProperty(_ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.ChargeRemaining));
            }
        }

        private void _ViewModel_ViewModelInitialized(object sender, EventArgs e)
        {
            initVars();
        }

        private void initVars()
        {
            this.DataContext = ViewModel;
        }

        private void OpenPower_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var h = new System.Windows.Interop.WindowInteropHelper(this);
            TrippLiteUPS.OpenSystemPowerOptions(h.Handle);
        }

        private void OpenCool_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenCoolWindow?.Invoke(this, new EventArgs());
        }
    }
}