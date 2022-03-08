using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

using DataTools.MathTools.PolarMath;

using Microsoft.VisualBasic.CompilerServices;
using System.Windows.Controls;

namespace TrippLite
{
    [Serializable]
    public partial class LoadBar 
    {
        private Size _refSize = new Size(0d, 0d);
        private double _angle = 0d;
        private int _maxSections = 28;
        private List<Polygon> _polyCache = new List<Polygon>();
        public static readonly DependencyProperty BluntingProperty = DependencyProperty.Register("Blunting", typeof(double), typeof(LoadBar), new PropertyMetadata(null));
        public static readonly DependencyProperty SectionSpacingProperty = DependencyProperty.Register("SectionSpacing", typeof(double), typeof(LoadBar), new PropertyMetadata(null));
        public static readonly DependencyProperty SectionsProperty = DependencyProperty.Register("Sections", typeof(uint), typeof(LoadBar), new PropertyMetadata(null));

        // Public Shared ReadOnly OutlinePointsProperty As DependencyProperty = DependencyProperty.Register("OutlinePoints",
        // GetType(PointCollection), GetType(LoadBar),
        // New PropertyMetadata(Nothing))

        public static readonly DependencyProperty EffectLevelProperty = DependencyProperty.Register("EffectLevel", typeof(double), typeof(LoadBar), new PropertyMetadata(null));

        // ' Load value

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public double LoadValue
        {
            get
            {
                double LoadValueRet = default;
                LoadValueRet = (double)this.GetValue(LoadValueProperty);
                return LoadValueRet;
            }

            set
            {
                this.SetValue(LoadValueProperty, value);
            }
        }

        public static double GetLoadValue(DependencyObject element)
        {
            if (element is null)
            {
                throw new ArgumentNullException("element");
            }

            return Conversions.ToDouble(element.GetValue(LoadValueProperty));
        }

        public static void SetLoadValue(DependencyObject element, double value)
        {
            if (element is null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(LoadValueProperty, value);
        }

        public static readonly DependencyProperty LoadValueProperty = DependencyProperty.RegisterAttached("LoadValue", typeof(double), typeof(LoadBar), new PropertyMetadata(new PropertyChangedCallback(DepPropChanged)));

        protected static void DepPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LoadBar lb = (LoadBar)d;
            lb.ChangeSectionsValues();
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public double Blunting
        {
            get
            {
                double BluntingRet = default;
                BluntingRet = (double)this.GetValue(BluntingProperty);
                return BluntingRet;
            }

            set
            {
                this.SetValue(BluntingProperty, value);
                Recalculate();
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public uint Sections
        {
            get
            {
                uint SectionsRet = default;
                SectionsRet = (uint)this.GetValue(SectionsProperty);
                return SectionsRet;
            }

            set
            {
                if (value < 1L || value > _maxSections)
                {
                    throw new ArgumentOutOfRangeException("Value must be a postive non-zero integer value less than or equal to " + _maxSections + ".");
                }

                this.SetValue(SectionsProperty, value);
                Recalculate();
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public double SectionSpacing
        {
            get
            {
                double SectionSpacingRet = default;
                SectionSpacingRet = (double)this.GetValue(SectionSpacingProperty);
                return SectionSpacingRet;
            }

            set
            {
                this.SetValue(SectionSpacingProperty, value);
                Recalculate();
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public double EffectLevel
        {
            get
            {
                double EffectLevelRet = default;
                EffectLevelRet = (double)this.GetValue(EffectLevelProperty);
                return EffectLevelRet;
            }

            set
            {
                this.SetValue(EffectLevelProperty, value);
                ChangeSectionsValues(true);
            }
        }

        // <Browsable(True), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
        // Public Property OutlinePoints As PointCollection
        // Get
        // OutlinePoints = DirectCast(GetValue(OutlinePointsProperty), PointCollection)
        // End Get
        // Set(value As PointCollection)
        // SetValue(OutlinePointsProperty, value)
        // End Set
        // End Property

        /// <summary>
    /// Returns the maximum number of sections supported for this instance.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public int MaxSections
        {
            get
            {
                return _maxSections;
            }
        }

        /// <summary>
    /// Perform a complete recalculation of the control.
    /// </summary>
    /// <remarks></remarks>
        private void Recalculate()
        {
            if (this.DrawingArea is null)
                return;
            _refSize = this.DrawingArea.RenderSize;
            if (_refSize.Height == 0d)
                return;

            // UpdateOutline()

            // ' this is the current angle calculated from the current box dimensions
            var pol = PolarCoordinates.ToPolarCoordinates(_refSize.Width, _refSize.Height);
            _angle = pol.Arc;
            CalculateSections();
        }

        // ''' <summary>
        // ''' Update the rectangular outline of the control.
        // ''' </summary>
        // ''' <remarks></remarks>
        // Private Sub UpdateOutline()
        // Dim s As Size = _refSize
        // Dim pc As New PointCollection

        // pc.Add(New Point(0, s.Height))
        // pc.Add(New Point(s.Width, s.Height))
        // pc.Add(New Point(s.Width, 0))

        // OutlinePoints = pc
        // End Sub

        /// <summary>
    /// Calculate only the lighted bars and backing effect.
    /// </summary>
    /// <param name="effectOnly"></param>
    /// <remarks></remarks>
        private void ChangeSectionsValues(bool effectOnly = false)
        {
            if (effectOnly)
            {
                for (long i = 0L, loopTo = Sections - 1L; i <= loopTo; i++)
                    _polyCache[(int)i].Effect = this.BackingBar.Effect;
            }
            else
            {
                int cblin = CalcBars(LoadValue, (int)Sections) - 1;
                int i;
                Polygon p;
                var loopTo1 = (int)(Sections - 1L);
                for (i = 0; i <= loopTo1; i++)
                {
                    p = _polyCache[i];
                    p.Effect = this.BackingBar.Effect;
                    if (cblin < i)
                    {
                        if (p.Opacity != 0.33d)
                            p.Opacity = 0.33d;
                    }
                    else if (p.Opacity != 1d)
                        p.Opacity = 1d;
                }
            }
        }

        /// <summary>
    /// Calculate and position the polygon sections, based on geometry, for the current size, position, and number of bars.
    /// </summary>
    /// <remarks></remarks>
        private void CalculateSections()
        {
            var s = _refSize;
            PointCollection pc;
            int d = (int)(Sections - 1L);
            Polygon p;
            double wHeight = s.Height - Blunting;
            double wWidth = s.Width;
            SolidColorBrush bc = (SolidColorBrush)Application.Current.Resources["TrippLiteLcdType"];
            // ' the width of each section will be calculated here, subtracting initial width by the spacing
            var pol = PolarCoordinates.ToPolarCoordinates(wWidth, wHeight);
            double w = (wWidth - SectionSpacing * (Sections - 1L)) / Sections;
            double sw = SectionSpacing;
            LinearCoordinates pt;
            int i;
            double x = 0d;
            // ' calculate from hypoteneuse side.
            double fr = pol.Radius / wWidth;
            int cblin = CalcBars(LoadValue, (int)Sections) - 1;
            this.BarsArea.Children.Clear();
            System.Threading.Thread.Sleep(0);
            var loopTo = (int)(Sections - 1L);
            for (i = 0; i <= loopTo; i++)
            {
                // ' fresh point collection
                pc = new PointCollection();
                p = _polyCache[i];
                pol.Radius = x + w;
                pol.Radius *= fr;
                pt = PolarCoordinates.ToLinearCoordinates(pol);
                pc.Add(new Point(x, s.Height));
                pc.Add(new Point(pt.X, s.Height));
                pc.Add(new Point(pt.X, Math.Max(0d, wHeight - pt.Y)));
                pol.Radius = x;
                pol.Radius *= fr;
                pt = PolarCoordinates.ToLinearCoordinates(pol);
                pc.Add(new Point(pt.X, Math.Max(0d, wHeight - pt.Y)));
                pc.Add(new Point(pt.X, s.Height));
                p.Points = pc;
                p.Fill = bc;
                p.Effect = this.BackingBar.Effect;
                if (cblin < i)
                {
                    p.Opacity = 0.33d;
                }
                else
                {
                    p.Opacity = 1d;
                }

                this.BarsArea.Children.Add(p);
                x += w;
                x += sw;
            }
        }

        /// <summary>
    /// Calculates the number of bars that should be lit for the given value.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="numBars"></param>
    /// <returns></returns>
    /// <remarks></remarks>
        private int CalcBars(double value, int numBars)
        {
            return (int)Math.Round(Math.Ceiling(value / (100d / numBars)));
        }

        public LoadBar(int maxSections)
        {
            this.InitializeComponent();

            this.SizeChanged += LoadBar_SizeChanged;

            _maxSections = maxSections;
            InstanceInit();
        }

        public LoadBar()
        {
            // This call is required by the designer.
            this.InitializeComponent();

            this.SizeChanged += LoadBar_SizeChanged;

            InstanceInit();
        }

        /// <summary>
        /// Initialize the instance of the load bar.
        /// </summary>
        /// <remarks></remarks>
        private void InstanceInit()
        {
            for (int i = 1, loopTo = _maxSections; i <= loopTo; i++)
                _polyCache.Add(new Polygon());
            EffectLevel = 0.0d;
            LoadValue = 100d;
            Sections = 10U;
            SectionSpacing = 1d;
            Blunting = 3d;
        }

        /// <summary>
    /// do the obvious.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <remarks></remarks>
        private void LoadBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Recalculate();
        }
    }
}