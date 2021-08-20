using System.Windows;

namespace TrippLite
{
    public partial class DisplayConfig
    {
        private TrippLiteViewModel _ViewModel;

        public TrippLiteViewModel ViewModel
        {
            get
            {
                return (TrippLiteViewModel)this.GetValue(ViewModelProperty);
            }

            set
            {
                this.SetValue(ViewModelProperty, value);
                _ViewModel = value;
            }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(TrippLiteViewModel), typeof(DisplayConfig), new PropertyMetadata(null));



        public DisplayConfig(TrippLiteViewModel m)
        {
            this.InitializeComponent();
            ViewModel = m;
        }
    }
}