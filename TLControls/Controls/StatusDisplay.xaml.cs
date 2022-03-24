using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace TrippLite
{
    public partial class StatusDisplay
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
                }
                
                vm = value;
                this.DataContext = vm;

                if (!vm.Initialized)
                {
                    vm.ViewModelInitialized += OnViewModelInitialized;
                }


            }
        }

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

        public static readonly DependencyProperty ItemSpacingProperty = DependencyProperty.RegisterAttached("ItemSpacing", typeof(Thickness), typeof(StatusDisplay), new PropertyMetadata(PropertyChanged));



        public ObservableCollection<TrippLiteCodes> DisplayCodes
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

        public static readonly DependencyProperty DisplayCodesProperty = DependencyProperty.RegisterAttached("DisplayCodes", typeof(ObservableCollection<TrippLiteCodes>), typeof(StatusDisplay), new PropertyMetadata(new ObservableCollection<TrippLiteCodes>(), PropertyChanged));

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public TrippLiteUPS TrippLite
        {
            get
            {
                return vm.TrippLite;
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
                SetValue(DisplayViewProperty, value);
            }
        }

        public static readonly DependencyProperty DisplayViewProperty = DependencyProperty.Register("DisplayView", typeof(StatusDisplayViews), typeof(StatusDisplay), new PropertyMetadata(StatusDisplayViews.Medium));

        public StatusDisplay()
        {
            ViewModel = new TrippLiteViewModel();
            vm.Initialize();

            DataContext = vm;
            this.InitializeComponent();
        }

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

        private void OnViewModelInitialized(object sender, EventArgs e)
        {
            this.DataContext = vm;

            foreach (var dc in vm.TrippLite.PropertyBag)
            {
                if (new[] { TrippLiteCodes.InputVoltage, TrippLiteCodes.OutputVoltage }.Contains(dc.Code))
                {
                    continue;
                }
                else
                {
                    DisplayCodes.Add(dc.Code);
                }
            }

            if (DesignerProperties.GetIsInDesignMode(this) == true)
            {
                vm.TrippLite.RefreshData();
            }
            else
            {
                vm.StartWatching();
            }
        }

        private void StatusDisplay_Unloaded(object sender, RoutedEventArgs e)
        {
            if (vm is object)
            {
                vm.Dispose();
                vm = null;
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