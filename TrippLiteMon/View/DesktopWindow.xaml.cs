using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

using DataTools.Scheduler;

namespace TrippLite
{
    public partial class DesktopWindow
    {
        private TrippLiteViewModel vm;

        public TrippLiteViewModel ViewModel
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => vm;
            
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (vm != null)
                {
                    vm.ViewModelInitialized -= OnViewModelInitialized;
                    vm.PowerStateChanged -= OnPowerStateChanged;
                }

                vm = value;

                if (vm != null)
                {
                    vm.ViewModelInitialized += OnViewModelInitialized;
                    vm.PowerStateChanged += OnPowerStateChanged;
                }
            }
        }

        public event OpenMainWindowEventHandler OpenMainWindow;

        public delegate void OpenMainWindowEventHandler(object sender, EventArgs e);

        public string ModelId
        {
            get
            {
                return (string)this.GetValue(ModelProperty);
            }
        }

        private static readonly DependencyPropertyKey ModelPropertyKey = DependencyProperty.RegisterReadOnly("ModelId", typeof(string), typeof(DesktopWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty ModelProperty = ModelPropertyKey.DependencyProperty;

        [Browsable(true)]
        [Category("Layout")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Thickness ItemSpacing
        {
            get
            {
                return GetItemSpacing(this);
            }
            set
            {
                SetItemSpacing(this, value);
            }
        }

        public static Thickness GetItemSpacing(DependencyObject element)
        {
            if (element is null)
            {
                throw new ArgumentNullException("element");
            }

            return (Thickness)element.GetValue(ItemSpacingProperty);
        }

        public static void SetItemSpacing(DependencyObject element, Thickness value)
        {
            if (element is null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(ItemSpacingProperty, value);
        }

        public static readonly DependencyProperty ItemSpacingProperty = DependencyProperty.RegisterAttached("ItemSpacing", typeof(Thickness), typeof(DesktopWindow), new PropertyMetadata(PropertyChanged));

        public ObservableCollection<BatteryPropertyCode> DisplayCodes
        {
            get
            {
                return GetDisplayCodes(this);
            }
            set
            {
                SetDisplayCodes(this, value);
            }
        }

        public static ObservableCollection<BatteryPropertyCode> GetDisplayCodes(DependencyObject element)
        {
            if (element is null)
            {
                throw new ArgumentNullException("element");
            }

            return (ObservableCollection<BatteryPropertyCode>)element.GetValue(DisplayCodesProperty);
        }

        public static void SetDisplayCodes(DependencyObject element, ObservableCollection<BatteryPropertyCode> value)
        {
            if (element is null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(DisplayCodesProperty, value);
        }

        public static readonly DependencyProperty DisplayCodesProperty = DependencyProperty.RegisterAttached("DisplayCodes", typeof(ObservableCollection<BatteryPropertyCode>), typeof(DesktopWindow), new PropertyMetadata(PropertyChanged));

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public TrippLiteUPS TrippLite => ViewModel.TrippLite;

        public DesktopWindow(TrippLiteViewModel vm)
        {
            DisplayCodes = new ObservableCollection<BatteryPropertyCode>();
            ViewModel = vm;
            ViewModel = vm;

            // This call is required by the designer.
            this.InitializeComponent();

            // Add any initialization after the InitializeComponent() call.

            if (ViewModel.Initialized == false)
            {
                ViewModel.Initialize();
            }
            else
            {
                this.DataContext = ViewModel;
            }

            foreach (var pr in ViewModel.Properties)
            {
                if (new[] { vm.TrippLite.PropertyMap.InputVoltage,
                    vm.TrippLite.PropertyMap.OutputVoltage,
                    vm.TrippLite.PropertyMap.OutputLoad,
                    vm.TrippLite.PropertyMap.OutputPower,
                    vm.TrippLite.PropertyMap.OutputCurrent }.Contains(pr.Code))
                {
                    continue;
                }
                else
                {
                    pr.IsActiveProperty = false;
                }
            }
        }

        public DesktopWindow()
        {
            DisplayCodes = new ObservableCollection<BatteryPropertyCode>();
            ViewModel = new TrippLiteViewModel();
            this.InitializeComponent();

            CloseButton.Click += CloseButton_Click;

            MoveButton.PreviewMouseLeftButtonDown += MoveButton_PreviewMouseLeftButtonDown;
            MoveButton.PreviewMouseLeftButtonUp += MoveButton_PreviewMouseLeftButtonUp;
            MoveButton.PreviewMouseMove += MoveButton_PreviewMouseMove;

            this.Unloaded += DesktopWindow_Unloaded;

            OptionsButton.Click += OptionsButton_Click;
            RevertToBig.Click += RevertToBig_Click;
            SysPower.Click += SysPower_Click;
            RunStart.Click += RunStart_Click;
            Config.Click += Config_Click;

            RunStart.IsChecked = TaskTool.GetIsEnabled();

            ViewModel.Initialize();

            foreach (var pr in ViewModel.Properties)
            {
                if (new[] { vm.TrippLite.PropertyMap.InputVoltage,
                    vm.TrippLite.PropertyMap.OutputVoltage,
                    vm.TrippLite.PropertyMap.OutputLoad,
                    vm.TrippLite.PropertyMap.OutputPower,
                    vm.TrippLite.PropertyMap.OutputCurrent }.Contains(pr.Code))
                {
                    continue;
                }
                else
                {
                    pr.IsActiveProperty = false;
                }
            }
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new DisplayConfig(ViewModel);
            dlg.ShowDialog();
        }

        private void RunStart_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ViewModel.RunOnStartup = !ViewModel.RunOnStartup;

            Dispatcher.BeginInvoke(() =>
            {
                RunStart.IsChecked = TaskTool.GetIsEnabled();
            });

        }

        private void OnViewModelInitialized(object sender, EventArgs e)
        {
            this.DataContext = ViewModel;


            foreach (var dc in ViewModel.TrippLite.PropertyBag)
            {
                if (new[] { vm.TrippLite.PropertyMap.InputVoltage, vm.TrippLite.PropertyMap.OutputVoltage }.Contains(dc.Code))
                {
                    continue;
                }
                else
                {
                    DisplayCodes.Add(dc.Code);
                }

            }

            ViewModel.TrippLite.RefreshData();
            this.SetValue(ModelPropertyKey, ViewModel.ModelId);
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                ViewModel.StartWatching();
            }

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StopWatching();
            this.Close();
        }

        private bool _Moving;

        private void MoveButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _Moving = true;
            _offsetPoint = e.GetPosition(this.MoveButton);
        }

        private void MoveButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _Moving = false;
        }

        private Point _offsetPoint;

        private void MoveButton_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_Moving)
            {
                var p = e.GetPosition(this.MoveButton);
                this.Left += p.X - _offsetPoint.X;
                this.Top += p.Y - _offsetPoint.Y;
            }
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            this.OptionsMenu.IsOpen = true;
        }

        private void RevertToBig_Click(object sender, RoutedEventArgs e)
        {
            OpenMainWindow?.Invoke(this, e);
        }

        private void SysPower_Click(object sender, RoutedEventArgs e)
        {
            var h = new System.Windows.Interop.WindowInteropHelper(this);
            TrippLiteUPS.OpenSystemPowerOptions(h.Handle);
        }

        private void DesktopWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel is object)
            {
                ViewModel.Dispose();
                ViewModel = null;
            }
        }

        private void ChangePowerState(bool psu, bool reset = false)
        {
            psuA.first = false;
            if (reset)
            {
                psuA.psu = !psu;
            }

            if (psu == psuA.psu)
                return;
            if (psu == true)
            {
                ViewModel.MakeLoadBarProperty(ViewModel.Properties.GetPropertyByCode(vm.TrippLite.PropertyMap.ChargeRemaining));
                ViewModel.Properties.GetPropertyByCode(vm.TrippLite.PropertyMap.OutputLoad).IsActiveProperty = false;
                psuA.psu = true;
            }
            else
            {
                ViewModel.MakeLoadBarProperty(ViewModel.Properties.GetPropertyByCode(vm.TrippLite.PropertyMap.OutputLoad));
                ViewModel.Properties.GetPropertyByCode(vm.TrippLite.PropertyMap.ChargeRemaining).IsActiveProperty = false;
                psuA.psu = false;
            }
        }

        private void OnPowerStateChanged(object sender, PowerStateChangedEventArgs e)
        {
            if (e.NewState == PowerStates.Utility)
            {
                ChangePowerState(false, psuA.first);
            }
            else
            {
                ChangePowerState(true, psuA.first);
            }
        }
    }

    internal static class psuA
    {
        [ThreadStatic]
        public static bool psu = false;
        [ThreadStatic]
        public static bool first = true;
    }
}