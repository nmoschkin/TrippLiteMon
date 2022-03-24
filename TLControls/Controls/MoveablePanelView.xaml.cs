using System.Windows;

namespace TrippLite
{
    public partial class MoveablePanelView
    {
        public MoveablePanelView()
        {
            InitializeComponent();
        }

        public object Items
        {
            get
            {
                return this.GetValue(ItemsProperty);
            }

            set
            {
                this.SetValue(ItemsProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(object), typeof(MoveablePanelView), new PropertyMetadata(null));


    }
}