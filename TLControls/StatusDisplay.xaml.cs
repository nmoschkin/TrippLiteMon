using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace TrippLite
{
    public partial class StatusDisplay
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
                }

                __ViewModel = value;
                if (__ViewModel != null)
                {
                    __ViewModel.ViewModelInitialized += _ViewModel_ViewModelInitialized;
                }
            }
        }

        public TrippLiteViewModel ViewModel
        {
            get
            {
                return (TrippLiteViewModel)this.GetValue(ViewModelProperty);
            }
        }

        private static readonly DependencyPropertyKey ViewModelPropertyKey = DependencyProperty.RegisterReadOnly("ViewModel", typeof(TrippLiteViewModel), typeof(StatusDisplay), new PropertyMetadata(new PropertyChangedCallback((sender, e) => { return; })));



        public static readonly DependencyProperty ViewModelProperty = ViewModelPropertyKey.DependencyProperty;

        [Browsable(true)]
        [Category("Layout")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Thickness ItemSpacing
        {
            get
            {
                Thickness ItemSpacingRet = default;
                ItemSpacingRet = StatusDisplay.GetItemSpacing(this);
                return ItemSpacingRet;
            }

            set
            {
                StatusDisplay.SetItemSpacing(this, value);
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

        public static readonly DependencyProperty ItemSpacingProperty = DependencyProperty.RegisterAttached("ItemSpacing", typeof(Thickness), typeof(StatusDisplay), new PropertyMetadata(PropertyChanged));



        public ObservableCollection<TrippLiteCodes> DisplayCodes
        {
            get
            {
                ObservableCollection<TrippLiteCodes> DisplayCodesRet = default;
                DisplayCodesRet = StatusDisplay.GetDisplayCodes(this);
                return DisplayCodesRet;
            }

            set
            {
                StatusDisplay.SetDisplayCodes(this, value);
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

        public static readonly DependencyProperty DisplayCodesProperty = DependencyProperty.RegisterAttached("DisplayCodes", typeof(ObservableCollection<TrippLiteCodes>), typeof(StatusDisplay), new PropertyMetadata(PropertyChanged));



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

        public StatusDisplayViews DisplayView
        {
            get
            {
                return (StatusDisplayViews)this.GetValue(DisplayViewProperty);
            }

            set
            {
                this.SetValue(DisplayViewProperty, value);
            }
        }

        public static readonly DependencyProperty DisplayViewProperty = DependencyProperty.Register("DisplayView", typeof(StatusDisplayViews), typeof(StatusDisplay), new PropertyMetadata(null));



        public StatusDisplay()
        {
            DisplayView = StatusDisplayViews.Medium;
            DisplayCodes = new ObservableCollection<TrippLiteCodes>();
            _ViewModel = new TrippLiteViewModel();
            this.SetValue(ViewModelPropertyKey, _ViewModel);
            _ViewModel.Initialize();

            // This call is required by the designer.
            this.InitializeComponent();
        }

        private int _rc = 0;
        private int _cc = 0;

        private UIElement CreateItem(TrippLitePropertyViewModel prop, DataTemplate t)
        {
            StackPanel gr;
            gr = new StackPanel();
            gr.Children.Add((UIElement)t.LoadContent());
            gr.HorizontalAlignment = HorizontalAlignment.Left;
            gr.DataContext = prop;
            gr.Margin = ItemSpacing;
            return gr;
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

            if (DesignerProperties.GetIsInDesignMode(this) == true)
            {
                _ViewModel.TrippLite.RefreshData();
            }
            else
            {
                _ViewModel.StartWatching();
            }
        }

        private void StatusDisplay_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_ViewModel is object)
            {
                _ViewModel.Dispose();
                this.SetValue(ViewModelPropertyKey, null);
                _ViewModel = null;
            }
        }
    }

    [Flags]
    public enum StatusDisplayViews
    {
        Small = 1,
        Medium = 2,
        Large = 4,
        Desktop = 0x80
    }
}