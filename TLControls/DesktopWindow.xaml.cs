using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

using DataTools.Scheduler;

using Microsoft.VisualBasic.CompilerServices;

namespace TrippLite
{
    public partial class DesktopWindow
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
                    __ViewModel.ViewModelInitialized -= _ViewModel_ViewModelInitialized;
                    __ViewModel.PowerStateChanged -= _ViewModel_PowerStateChanged;
                }

                __ViewModel = value;
                if (__ViewModel != null)
                {
                    __ViewModel.ViewModelInitialized += _ViewModel_ViewModelInitialized;
                    __ViewModel.PowerStateChanged += _ViewModel_PowerStateChanged;
                }
            }
        }

        public event OpenMainWindowEventHandler OpenMainWindow;

        public delegate void OpenMainWindowEventHandler(object sender, EventArgs e);

        public TrippLiteViewModel ViewModel
        {
            get
            {
                return (TrippLiteViewModel)this.GetValue(ViewModelProperty);
            }

            internal set
            {
                this.SetValue(ViewModelProperty, value);
                _ViewModel = value;
            }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(TrippLiteViewModel), typeof(DesktopWindow), new PropertyMetadata(null));

        public string ModelId
        {
            get
            {
                return Conversions.ToString(this.GetValue(ModelProperty));
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
                Thickness ItemSpacingRet = default;
                ItemSpacingRet = DesktopWindow.GetItemSpacing(this);
                return ItemSpacingRet;
            }

            set
            {
                DesktopWindow.SetItemSpacing(this, value);
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

        public ObservableCollection<TrippLiteCodes> DisplayCodes
        {
            get
            {
                ObservableCollection<TrippLiteCodes> DisplayCodesRet = default;
                DisplayCodesRet = DesktopWindow.GetDisplayCodes(this);
                return DisplayCodesRet;
            }

            set
            {
                DesktopWindow.SetDisplayCodes(this, value);
            }
        }

        public static ObservableCollection<TrippLiteCodes> GetDisplayCodes(DependencyObject element)
        {
            if (element is null)
            {
                throw new ArgumentNullException("element");
            }

            return (ObservableCollection<TrippLiteCodes>)element.GetValue(DisplayCodesProperty);
        }

        public static void SetDisplayCodes(DependencyObject element, ObservableCollection<TrippLiteCodes> value)
        {
            if (element is null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(DisplayCodesProperty, value);
        }

        public static readonly DependencyProperty DisplayCodesProperty = DependencyProperty.RegisterAttached("DisplayCodes", typeof(ObservableCollection<TrippLiteCodes>), typeof(DesktopWindow), new PropertyMetadata(PropertyChanged));

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public TrippLiteUPS TrippLite
        {
            get
            {
                TrippLiteUPS TrippLiteRet = default;
                TrippLiteRet = _ViewModel.TrippLite;
                return TrippLiteRet;
            }
        }

        public DesktopWindow(TrippLiteViewModel vm)
        {
            DisplayCodes = new ObservableCollection<TrippLiteCodes>();
            ViewModel = vm;
            _ViewModel = vm;

            // This call is required by the designer.
            this.InitializeComponent();

            // Add any initialization after the InitializeComponent() call.

            if (_ViewModel.Initialized == false)
            {
                _ViewModel.Initialize();
            }
            else
            {
                this.DataContext = ViewModel;
            }

            foreach (var pr in _ViewModel.Properties)
            {
                switch (pr.Code)
                {
                    case TrippLiteCodes.InputVoltage:
                    case TrippLiteCodes.OutputVoltage:
                    case TrippLiteCodes.OutputLoad:
                    case TrippLiteCodes.OutputPower:
                    case TrippLiteCodes.OutputCurrent:
                        {
                            break;
                        }

                    default:
                        {
                            pr.IsActiveProperty = false;
                            break;
                        }
                }
            }
        }

        public DesktopWindow()
        {
            DisplayCodes = new ObservableCollection<TrippLiteCodes>();
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
            
            RunStart.IsChecked = TaskTool.GetIsEnabled();

            _ViewModel.Initialize();
            foreach (var pr in _ViewModel.Properties)
            {
                switch (pr.Code)
                {
                    case TrippLiteCodes.InputVoltage:
                    case TrippLiteCodes.OutputVoltage:
                    case TrippLiteCodes.OutputLoad:
                    case TrippLiteCodes.OutputPower:
                    case TrippLiteCodes.OutputCurrent:
                        {
                            break;
                        }

                    default:
                        {
                            pr.IsActiveProperty = false;
                            break;
                        }
                }
            }
        }

        private void RunStart_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            _ViewModel.RunOnStartup = !_ViewModel.RunOnStartup;

            Dispatcher.BeginInvoke(() =>
            {
                RunStart.IsChecked = TaskTool.GetIsEnabled();
            });

        }

        private void _ViewModel_ViewModelInitialized(object sender, EventArgs e)
        {
            this.DataContext = ViewModel;
            foreach (var dc in _ViewModel.TrippLite.PropertyBag)
            {
                switch (dc.Code)
                {
                    case TrippLiteCodes.InputVoltage:
                    case TrippLiteCodes.OutputVoltage:
                        {
                            continue;
                            break;
                        }

                    default:
                        {
                            DisplayCodes.Add(dc.Code);
                            break;
                        }
                }
            }

            _ViewModel.TrippLite.RefreshData();
            this.SetValue(ModelPropertyKey, _ViewModel.ModelId);
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                _ViewModel.StartWatching();
            }

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _ViewModel.StopWatching();
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
            if (_ViewModel is object)
            {
                _ViewModel.Dispose();
                this.SetValue(ViewModelProperty, null);
                _ViewModel = null;
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
                _ViewModel.MakeLoadBarProperty(_ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.ChargeRemaining));
                _ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.OutputLoad).IsActiveProperty = false;
                psuA.psu = true;
            }
            else
            {
                _ViewModel.MakeLoadBarProperty(_ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.OutputLoad));
                _ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.ChargeRemaining).IsActiveProperty = false;
                psuA.psu = false;
            }
        }

        private void _ViewModel_PowerStateChanged(object sender, PowerStateChangedEventArgs e)
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