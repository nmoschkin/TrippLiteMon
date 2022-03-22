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
    public enum TabSides
    {
        Top,
        Left,
        Bottom,
        Right,
    }

    /// <summary>
    /// Interaction logic for QuickTabs.xaml
    /// </summary>
    public partial class QuickTabs : UserControl
    {

        public QuickTabs()
        {
            InitializeComponent();

            this.Unloaded += QuickTabs_Unloaded;
            ReTab();
            WireAndUnwire(Items, null);

        }

        private void QuickTabs_Unloaded(object sender, RoutedEventArgs e)
        {
            int c = Items_AREA.Children.Count;
            int i;

            for (i = 0; i < c; i++)
            {
                Items[i].Header.MouseUp -= Header_MouseUp;
            }

            Content_AREA.Children.Clear();
            Items_AREA.Children.Clear();
        }

        public int SelectedIndex        
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(QuickTabs), new PropertyMetadata(-1, OnSelectedIndexChanged));

        private static void OnSelectedIndexChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is QuickTabs owner)
            {
                if (e.NewValue is int value)
                {
                    owner.SetSelectedIndex(value);
                }
            }
        }

        public QuickTabCollection Items
        {
            get { return (QuickTabCollection)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(QuickTabCollection), typeof(QuickTabs), new PropertyMetadata(new QuickTabCollection(), OnItemsChanged));

        private static void OnItemsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is QuickTabs owner)
            {
                owner.WireAndUnwire(e.NewValue as QuickTabCollection ?? null, e.OldValue as QuickTabCollection ?? null);
            }
        }

        private void SetSelectedIndex(int index)
        {
            int c = Items.Count;
            int i;

            for (i = 0; i < c; i++)
            {
                var child = Items[i];

                if (i == index)
                {
                    if (!child.IsSelected || child.Visibility != Visibility.Visible)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            child.Visibility = Visibility.Visible;
                            child.IsSelected = true;
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

        private void CollectItems()
        {
            int c = Items_AREA.Children.Count;
            int i;

            for (i = 0; i < c; i++)
            {
                Items[i].Header.MouseUp -= Header_MouseUp;
            }

            var itemsBefore = c != 0;

            Content_AREA.Children.Clear();
            Items_AREA.Children.Clear();

            c = Items.Count;

            var itemsAfter = c != 0;
            var selIdx = SelectedIndex;

            for (i = 0; i < c; i++)
            {
                Content_AREA.Children.Add(Items[i]);
                Items_AREA.Children.Add(Items[i].Header);

                Items[i].Header.MouseUp += Header_MouseUp;
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
            else
            {
                SetSelectedIndex(selIdx);
            }
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

        private void TabItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectItems();
        }

        public TabSides TabsPosition
        {
            get { return (TabSides)GetValue(TabsPositionProperty); }
            set { SetValue(TabsPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TabsPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabsPositionProperty =
            DependencyProperty.Register("TabsPosition", typeof(TabSides), typeof(QuickTabs), new PropertyMetadata(TabSides.Top, OnTabsPositionChanged));

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
    }

    public class QuickTab : ContentControl
    {
        public Control Header
        {
            get { return (Control)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(Control), typeof(QuickTab), new PropertyMetadata(null, OnHeaderChanged));

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

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(QuickTab), new PropertyMetadata("", OnTextChanged));

        private static void OnTextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is QuickTab owner)
            {
                if (e.NewValue is string value)
                {

                }
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(QuickTab), new PropertyMetadata(false, OnIsSelectedChanged));

        private static void OnIsSelectedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is QuickTab owner)
            {
                if (e.NewValue is bool value)
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
                    }
                }
            }
        }

        private static Label NewDefaultControl()
        {
            var lbl = new Label()
            {
                Style = (Style)Application.Current.Resources["DefaultHeaderStyle"]
            };

            lbl.SetBinding(Label.ContentProperty, new Binding("Text"));
            return lbl;
        }

        public QuickTab() : base()
        {
            Header = NewDefaultControl();            
            Header.DataContext = this;
        }
    }

    public class QuickTabCollection : ObservableCollection<QuickTab>
    {

        public QuickTabCollection() : base()
        {
        }
    }

}
