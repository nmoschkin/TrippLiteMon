using System;
using System.Collections.ObjectModel;
using System.Collections;

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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrippLite
{
    public delegate void TabSelectionChangedEventHandler(object sender, TabSelectionChangedEventArgs e);

    public enum TabSides
    {
        Top,
        Left,
        Bottom,
        Right,
    }

    public class QuickTab : ContentControl
    {

        #region Public Fields

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(Control), typeof(QuickTab), new PropertyMetadata(null, OnHeaderChanged));

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(QuickTab), new PropertyMetadata(false, OnIsSelectedChanged));

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(QuickTab), new PropertyMetadata("", OnTextChanged));

        #endregion Public Fields

        #region Public Constructors

        public QuickTab() : base()
        {
            Header = NewDefaultControl();
            Header.DataContext = this;
        }

        #endregion Public Constructors

        #region Public Events

        public event TabSelectionChangedEventHandler? TabSelectionChanged;

        #endregion Public Events

        #region Public Properties

        public Control Header
        {
            get { return (Control)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion Public Properties

        public override string ToString()
        {
            return Text ?? base.ToString();
        }

        #region Private Methods

        private static Label NewDefaultControl()
        {
            var lbl = new Label()
            {
                Style = (Style)Application.Current.Resources["DefaultHeaderStyle"]
            };

            lbl.SetBinding(Label.ContentProperty, new Binding("Text"));
            return lbl;
        }

        private static void OnHeaderChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is QuickTab owner)
            {
                if (e.NewValue != e.OldValue)
                {
                    if (e.NewValue is Control value)
                    {
                        value.DataContext = owner;
                    }
                    else if (e.NewValue == null)
                    {
                        value = NewDefaultControl();
                        value.DataContext = owner;

                        owner.Header = value;
                    }
                }
            }
        }
        private static void OnIsSelectedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is QuickTab owner)
            {
                if (e.NewValue is bool value && (bool)e.NewValue != (bool)e.OldValue)
                {
                    if (owner.Header is Label lbl)
                    {
                        if (value)
                        {
                            lbl.Style = (Style)Application.Current.Resources["DefaultSelectedHeaderStyle"];
                        }
                        else
                        {
                            var dhs = (Style)Application.Current.Resources["DefaultHeaderStyle"];

                            lbl.Style = dhs;
                            lbl.Foreground = dhs.Setters.Where((s) => (s is Setter ss) && ss.Property.Name == "Foreground").Select((s2) => s2 is Setter ss2 ? ss2.Value as SolidColorBrush : null).FirstOrDefault();
                        }

                        owner.FireTabSelectionChanged(value);
                    }
                }
            }
        }

        

        private static void OnTextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is QuickTab owner)
            {
                if (e.NewValue is string value)
                {

                }
            }
        }
        private void FireTabSelectionChanged(bool isSel)
        {
            TabSelectionChanged?.Invoke(this, new TabSelectionChangedEventArgs(this, isSel));
        }

        #endregion Private Methods
    }

    public class QuickTabCollection : ObservableCollection<QuickTab>
    {

        #region Public Constructors

        public QuickTabCollection() : base()
        {
        }

        #endregion Public Constructors
    }

    /// <summary>
    /// Interaction logic for QuickTabs.xaml
    /// </summary>
    public partial class QuickTabs : UserControl
    {
        #region Public Fields

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(QuickTabCollection), typeof(QuickTabs), new PropertyMetadata(new QuickTabCollection(), OnItemsChanged));

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(QuickTabs), new PropertyMetadata(-1, OnSelectedIndexChanged));

        // Using a DependencyProperty as the backing store for TabsPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabsPositionProperty =
            DependencyProperty.Register("TabsPosition", typeof(TabSides), typeof(QuickTabs), new PropertyMetadata(TabSides.Top, OnTabsPositionChanged));

        #endregion Public Fields

        #region Public Constructors

        public QuickTabs()
        {
            InitializeComponent();

            this.Unloaded += QuickTabs_Unloaded;
            this.IsVisibleChanged += QuickTabs_IsVisibleChanged;
            ReTab();
            WireAndUnwire(Items, null);

        }

        private void QuickTabs_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility != Visibility.Visible)
            {
                Clear();
            }
        }

        #endregion Public Constructors

        #region Public Events

        public event TabSelectionChangedEventHandler? TabSelectionChanged;

        #endregion Public Events

        #region Public Properties

        public QuickTabCollection Items
        {
            get { return (QuickTabCollection)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public TabSides TabsPosition
        {
            get { return (TabSides)GetValue(TabsPositionProperty); }
            set { SetValue(TabsPositionProperty, value); }
        }

        #endregion Public Properties

        #region Private Methods

        private static void OnItemsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is QuickTabs owner)
            {
                owner.WireAndUnwire(e.NewValue as QuickTabCollection ?? null, e.OldValue as QuickTabCollection ?? null);
            }
        }

        private static void OnSelectedIndexChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is QuickTabs owner)
            {
                if ((e.NewValue is int value) && ((int)e.NewValue != (int)e.OldValue))
                {
                    owner.SetSelectedIndex(value);
                }
            }
        }

        private static void OnTabsPositionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is QuickTabs owner)
            {
                if (e.NewValue is TabSides value && e.OldValue is TabSides oldValue)
                {
                    if (oldValue != value)
                    {
                        owner.ReTab();
                    }
                }
            }
        }

        private void CollectItems()
        {
            int c = Items_AREA.Children.Count;
            int i;

            for (i = 0; i < c; i++)
            {
                Items[i].Header.MouseUp -= Header_MouseUp;
                Items[i].TabSelectionChanged -= QuickTabs_TabSelectionChanged;

            }

            var itemsBefore = c != 0;

            Content_AREA.Children.Clear();
            Items_AREA.Children.Clear();

            GC.Collect();

            c = Items.Count;

            var itemsAfter = c != 0;
            var selIdx = SelectedIndex;

            for (i = 0; i < c; i++)
            {
                Content_AREA.Children.Add(Items[i]);
                Items_AREA.Children.Add(Items[i].Header);

                Items[i].Header.MouseUp += Header_MouseUp;
                Items[i].TabSelectionChanged += QuickTabs_TabSelectionChanged;

                Items[i].Visibility = Visibility.Collapsed;
            }

            if (selIdx >= c)
            {
                SelectedIndex = c - 1;
            }
            else if (!itemsBefore && itemsAfter)
            {
                SelectedIndex = 0;
            }
            else if (itemsBefore && !itemsAfter)
            {
                SelectedIndex = -1;
            }
            else if (selIdx != SelectedIndex)
            {
                SetSelectedIndex(selIdx);
            }
        }

        private void FireTabSelectionChanged(QuickTab item, bool selState, int selIdx)
        {
            TabSelectionChanged?.Invoke(this, new TabSelectionChangedEventArgs(item, selState, selIdx));
        }

        private void Header_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int c = Items.Count;
            int i;

            int selIdx = -1;

            for (i = 0; i < c; i++)
            {
                if (sender == Items[i].Header)
                {
                    selIdx = i;
                    break;
                }
            }

            SelectedIndex = selIdx;
        }

        private void QuickTabs_TabSelectionChanged(object sender, TabSelectionChangedEventArgs e)
        {
            var i = Items.IndexOf(e.Tab);

            if (e.IsSelected && i != -1 && i != SelectedIndex)
            {
                SelectedIndex = i;
            }
        }

        private void Clear()
        {
            int c = Items_AREA.Children.Count;
            int i;

            for (i = 0; i < c; i++)
            {
                try
                {
                    Items[i].Header.MouseUp -= Header_MouseUp;
                }
                catch { }
            }

            Content_AREA.Children.Clear();
            Items_AREA.Children.Clear();
        }

        private void QuickTabs_Unloaded(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        ~QuickTabs()
        {
            Clear();
        }

        private void ReTab()
        {
            switch (TabsPosition)
            {
                case TabSides.Top:

                    Items_AREA.Orientation = Orientation.Horizontal;

                    Items_AREA.SetValue(Grid.RowProperty, 0);
                    Items_AREA.SetValue(Grid.ColumnProperty, 0);
                    Items_AREA.SetValue(Grid.RowSpanProperty, 1);
                    Items_AREA.SetValue(Grid.ColumnSpanProperty, 3);


                    Content_AREA.SetValue(Grid.RowProperty, 1);
                    Content_AREA.SetValue(Grid.ColumnProperty, 0);
                    Content_AREA.SetValue(Grid.RowSpanProperty, 2);
                    Content_AREA.SetValue(Grid.ColumnSpanProperty, 3);

                    break;

                case TabSides.Bottom:

                    Items_AREA.Orientation = Orientation.Horizontal;

                    Items_AREA.SetValue(Grid.RowProperty, 2);
                    Items_AREA.SetValue(Grid.ColumnProperty, 0);
                    Items_AREA.SetValue(Grid.RowSpanProperty, 1);
                    Items_AREA.SetValue(Grid.ColumnSpanProperty, 3);


                    Content_AREA.SetValue(Grid.RowProperty, 0);
                    Content_AREA.SetValue(Grid.ColumnProperty, 0);
                    Content_AREA.SetValue(Grid.RowSpanProperty, 2);
                    Content_AREA.SetValue(Grid.ColumnSpanProperty, 3);

                    break;

                case TabSides.Left:

                    Items_AREA.Orientation = Orientation.Vertical;

                    Items_AREA.SetValue(Grid.RowProperty, 0);
                    Items_AREA.SetValue(Grid.ColumnProperty, 0);
                    Items_AREA.SetValue(Grid.RowSpanProperty, 3);
                    Items_AREA.SetValue(Grid.ColumnSpanProperty, 1);


                    Content_AREA.SetValue(Grid.RowProperty, 0);
                    Content_AREA.SetValue(Grid.ColumnProperty, 1);
                    Content_AREA.SetValue(Grid.RowSpanProperty, 2);
                    Content_AREA.SetValue(Grid.ColumnSpanProperty, 1);

                    break;


                case TabSides.Right:

                    Items_AREA.Orientation = Orientation.Vertical;

                    Items_AREA.SetValue(Grid.RowProperty, 0);
                    Items_AREA.SetValue(Grid.ColumnProperty, 2);
                    Items_AREA.SetValue(Grid.RowSpanProperty, 3);
                    Items_AREA.SetValue(Grid.ColumnSpanProperty, 1);


                    Content_AREA.SetValue(Grid.RowProperty, 0);
                    Content_AREA.SetValue(Grid.ColumnProperty, 0);
                    Content_AREA.SetValue(Grid.RowSpanProperty, 2);
                    Content_AREA.SetValue(Grid.ColumnSpanProperty, 1);

                    break;

            }
        }

        private void SetSelectedIndex(int index)
        {
            int c = Items.Count;
            int i;
            bool selDone = false;

            for (i = 0; i < c; i++)
            {
                var child = Items[i];

                if (i == index)
                {
                    if (!selDone && !child.IsSelected || child.Visibility != Visibility.Visible)
                    {
                        selDone = true;
                        var selIdx = i;

                        Dispatcher.BeginInvoke(() =>
                        {
                            child.Visibility = Visibility.Visible;
                            child.IsSelected = true;

                            FireTabSelectionChanged(child, true, selIdx);
                        });
                    }

                }
                else
                {
                    if (child.IsSelected || child.Visibility != Visibility.Collapsed)
                    {

                        Dispatcher.BeginInvoke(() =>
                        {
                            child.Visibility = Visibility.Collapsed;
                            child.IsSelected = false;
                        });
                    }
                }
            }
        }

        private void TabItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectItems();
        }

        private void WireAndUnwire(QuickTabCollection? newItem, QuickTabCollection? oldItem)
        {
            if (newItem == null)
            {
                Items = new QuickTabCollection();
                Items.CollectionChanged += TabItemsChanged;

                return;
            }

            if (newItem != oldItem)
            {
                if (oldItem != null)
                {
                    oldItem.CollectionChanged -= TabItemsChanged;
                }

                newItem.CollectionChanged += TabItemsChanged;
                CollectItems();
            }
        }

        #endregion Private Methods
    }

    public class TabSelectionChangedEventArgs : EventArgs
    {

        #region Public Constructors

        public TabSelectionChangedEventArgs(QuickTab sender, bool isSelected, int selIdx = -1)
        {
            Tab = sender;
            IsSelected = isSelected;
            SelectedIndex = selIdx;
        }

        #endregion Public Constructors

        #region Public Properties

        public bool IsSelected { get; private set; }
        public int SelectedIndex { get; private set; }
        public QuickTab Tab { get; private set; }

        #endregion Public Properties
    }
}
