using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace TrippLite
{
    public partial class PowerMon : Window
    {
        private TrippLiteViewModel __ViewModel;

        private TrippLiteViewModel _ViewModel
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return __ViewModel;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (__ViewModel != null)
                {
                    __ViewModel.PowerStateChanged -= _ViewModel_PowerStateChanged;
                    __ViewModel.ViewModelInitialized -= _ViewModel_ViewModelInitialized;
                }

                __ViewModel = value;
                if (__ViewModel != null)
                {
                    __ViewModel.PowerStateChanged += _ViewModel_PowerStateChanged;
                    __ViewModel.ViewModelInitialized += _ViewModel_ViewModelInitialized;
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