using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using DataTools.Scheduler;

namespace TrippLite
{
    public class TrippLiteViewModel : DependencyObject, INotifyPropertyChanged, IDisposable
    {
        private bool initialized = false;
        private Thread waitInit;
        private TrippLiteUPS trippLite;

        private TrippLiteUPS SyncModel
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return trippLite;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (trippLite != null)
                {

                    trippLite.PowerStateChanged -= OnPowerStateChanged;
                }

                trippLite = value;
                if (trippLite != null)
                {
                    trippLite.PowerStateChanged += OnPowerStateChanged;
                }
            }
        }

        private bool isRunning = false;

        public int MaxTries { get; set; } = 100;
        public int TimerInterval { get; set; } = 200;
        public int DelayStart { get; set; } = 500;
        public TrippLitePropertyBagViewModel Properties { get; set; }
        public TrippLitePropertyBagViewModel ProminentProperties { get; set; }
        public TrippLitePropertyBagViewModel LoadProperties { get; set; }

        private Thread wthread;
        private TrippLitePropertyViewModel lbProp;

        public event ViewModelInitializedEventHandler ViewModelInitialized;

        public delegate void ViewModelInitializedEventHandler(object sender, EventArgs e);

        public event PowerStateChangedEventHandler PowerStateChanged;

        public delegate void PowerStateChangedEventHandler(object sender, PowerStateChangedEventArgs e);

        private CancellationTokenSource cts;

        #region LoadBar

        public LoadBarPropertyHandler LoadBarHandler { get; set; }

        public double LoadBarValue
        {
            get
            {
                return LoadBarHandler.HandleLoadBarValue((double)GetValue(LoadBarValueProperty));
            }
        }

        public static double GetLoadBarValue(DependencyObject element)
        {
            if (element is null)
            {
                throw new ArgumentNullException("element");
            }

            return (double)element.GetValue(LoadBarValueProperty);
        }

        private static readonly DependencyPropertyKey LoadBarValuePropertyKey = DependencyProperty.RegisterAttachedReadOnly("LoadBarValue", typeof(double), typeof(TrippLiteViewModel), new PropertyMetadata(null));


        public static readonly DependencyProperty LoadBarValueProperty = LoadBarValuePropertyKey.DependencyProperty;

        public void MakeLoadBarProperty(TrippLitePropertyViewModel prop, LoadBarPropertyHandler handler = null)
        {
            ClearLoadBarProperty();
            if (handler is null)
            {
                LoadBarHandler = new LoadBarPropertyHandler(0d, 100d);
            }
            else
            {
                LoadBarHandler = handler;
            }

            lbProp = prop;
            lbProp.IsActiveProperty = true;
            Dispatcher.Invoke(() => LoadBarWatch(this, new PropertyChangedEventArgs(nameof(LoadBarValue))));
            lbProp.PropertyChanged += LoadBarWatch;
        }

        public void ClearLoadBarProperty()
        {
            if (lbProp is object)
            {
                lbProp.PropertyChanged -= LoadBarWatch;
                lbProp = null;
            }
        }

        private void LoadBarWatch(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => SetValue(LoadBarValuePropertyKey, (double)lbProp.Value)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadBarValue)));
        }

        #endregion

        #region TrippLite Device

        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
        }

        private static readonly DependencyPropertyKey TitlePropertyKey = DependencyProperty.RegisterReadOnly("Title", typeof(string), typeof(TrippLiteViewModel), new PropertyMetadata(null));


        public static readonly DependencyProperty TitleProperty = TitlePropertyKey.DependencyProperty;

        public string SerialNumber
        {
            get
            {
                return (string)GetValue(SerialNumberProperty);
            }
        }

        private static readonly DependencyPropertyKey SerialNumberPropertyKey = DependencyProperty.RegisterReadOnly("SerialNumber", typeof(string), typeof(TrippLiteViewModel), new PropertyMetadata(null));


        public static readonly DependencyProperty SerialNumberProperty = SerialNumberPropertyKey.DependencyProperty;

        public string ProductString
        {
            get
            {
                return (string)GetValue(ProductStringProperty);
            }
        }

        private static readonly DependencyPropertyKey ProductStringPropertyKey = DependencyProperty.RegisterReadOnly("ProductString", typeof(string), typeof(TrippLiteViewModel), new PropertyMetadata(null));


        public static readonly DependencyProperty ProductStringProperty = ProductStringPropertyKey.DependencyProperty;

        public string ModelId
        {
            get
            {
                return (string)GetValue(ModelIdProperty);
            }
        }

        private static readonly DependencyPropertyKey ModelIdPropertyKey = DependencyProperty.RegisterReadOnly("ModelId", typeof(string), typeof(TrippLiteViewModel), new PropertyMetadata(null));


        public static readonly DependencyProperty ModelIdProperty = ModelIdPropertyKey.DependencyProperty;

        public PowerStates PowerState
        {
            get
            {
                return (PowerStates)GetValue(PowerStateProperty);
            }
        }

        private static readonly DependencyPropertyKey PowerStatePropertyKey = DependencyProperty.RegisterReadOnly("PowerState", typeof(PowerStates), typeof(TrippLiteViewModel), new PropertyMetadata(PowerStates.Uninitialized));


        public static readonly DependencyProperty PowerStateProperty = PowerStatePropertyKey.DependencyProperty;

        public string PowerStateDescription
        {
            get
            {
                return (string)GetValue(PowerStateDescriptionProperty);
            }
        }

        private static readonly DependencyPropertyKey PowerStateDescriptionPropertyKey = DependencyProperty.RegisterReadOnly("PowerStateDescription", typeof(string), typeof(TrippLiteViewModel), new PropertyMetadata(null));


        public static readonly DependencyProperty PowerStateDescriptionProperty = PowerStateDescriptionPropertyKey.DependencyProperty;

        public string PowerStateDetail
        {
            get
            {
                return (string)GetValue(PowerStateDetailProperty);
            }
        }

        private static readonly DependencyPropertyKey PowerStateDetailPropertyKey = DependencyProperty.RegisterReadOnly("PowerStateDetail", typeof(string), typeof(TrippLiteViewModel), new PropertyMetadata(null));


        public static readonly DependencyProperty PowerStateDetailProperty = PowerStateDetailPropertyKey.DependencyProperty;

        public Color UtilityColor
        {
            get
            {
                return (Color)GetValue(UtilityColorProperty);
            }
        }

        private static readonly DependencyPropertyKey UtilityColorPropertyKey = DependencyProperty.RegisterReadOnly("UtilityColor", typeof(Color), typeof(TrippLiteViewModel), new PropertyMetadata(null));


        public static readonly DependencyProperty UtilityColorProperty = UtilityColorPropertyKey.DependencyProperty;

        public Color UtilityColorBackground
        {
            get
            {
                var v = Color.FromArgb(255, 255, 255, 255);
                try
                {
                    switch (PowerState)
                    {
                        case PowerStates.Utility:
                            return Color.FromRgb(0, 128, 0);

                        case PowerStates.Battery:
                            return Color.FromRgb(128, 128, 0);

                        default:
                            return Color.FromRgb(128, 0, 0);
                    }
                }
                catch 
                {
                    return Color.FromArgb(255, 255, 255, 255);
                }
            }
        }

        public bool RunOnStartup
        {
            get { return (bool)GetValue(RunOnStartupProperty); }
            set { SetValue(RunOnStartupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RunOnStartup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RunOnStartupProperty =
            DependencyProperty.Register("RunOnStartup", typeof(bool), typeof(TrippLiteViewModel), new PropertyMetadata(TaskTool.GetIsEnabled(), OnRunOnStartupChanged));


        protected static void OnRunOnStartupChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TrippLiteViewModel vm && ((bool)e.NewValue != TaskTool.GetIsEnabled()))
            {
                var b = (bool)e.NewValue;

                // Launch itself as administrator
                ProcessStartInfo proc = new ProcessStartInfo();
                
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\" + AppDomain.CurrentDomain.FriendlyName + ".exe";
                proc.Verb = "runas";


                if (b)
                {
                    proc.Arguments = "/erologin";

                }
                else
                {
                    proc.Arguments = "/drologin";
                }

                try
                {
                    var exec = Process.Start(proc);
                    exec?.WaitForExit();
                }
                catch
                {
                    vm.RunOnStartup = TaskTool.GetIsEnabled();
                    return;
                }
            }
        }

        #endregion

        #region Constructors

        public TrippLiteViewModel() : this(false)
        {
        }

        public TrippLiteViewModel(bool init)
        {


            Properties = new TrippLitePropertyBagViewModel(this);
            ProminentProperties = new TrippLitePropertyBagViewModel(this);
            LoadProperties = new TrippLitePropertyBagViewModel(this);

            if (init)
                Initialize();
        }

        public bool Initialize()
        {


            if (initialized)
                return false;
            try
            {
                SyncModel = new TrippLiteUPS(false);
                if (SyncModel.Connected == false)
                {
                    waitInit = new Thread(() =>
                    {
                        for (var i = 0; i < MaxTries; i++)
                        {
                            Thread.Sleep(100);
                            SyncModel.Connect();
                        
                            if (SyncModel is object && SyncModel.IsTrippLite)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    initialized = true;
                                    
                                    InternalInit();
                                    
                                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrippLite)));
                                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Properties)));
                                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProminentProperties)));
                                
                                    ViewModelInitialized?.Invoke(this, new EventArgs());
                                });
                            
                                waitInit = null;
                                
                                return;
                            }
                        }
                    });
                    
                    waitInit.SetApartmentState(ApartmentState.MTA);
                    waitInit.IsBackground = false;
                    waitInit.Start();
                    
                    return false;
                }
            }
            catch 
            {
                return false;
            }

            initialized = true;
            
            InternalInit();
            
            ViewModelInitialized?.Invoke(this, new EventArgs());
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrippLite)));
            
            return true;
        }

        private void InternalInit()
        {

            // ' build the view model property collection

            TrippLitePropertyViewModel pl;

            foreach (var p in SyncModel.PropertyBag)
            {
                if (p.Code != TrippLiteCodes.InputVoltage && p.Code != TrippLiteCodes.OutputVoltage)
                {
                    Properties.Add(new TrippLitePropertyViewModel(p, this));
                }
                else
                {
                    pl = new TrippLitePropertyViewModel(p, this);
                    pl.Prominent = true;
                    ProminentProperties.Add(pl);
                    pl = null;
                }
            }

            PromoteToLoad(TrippLiteCodes.OutputPower);
            
            MakeLoadBarProperty(FindProperty(TrippLiteCodes.OutputLoad));

            SetValue(ProductStringPropertyKey, SyncModel.Device.ProductString);

            SetValue(ModelIdPropertyKey, SyncModel.ModelId);

            SetValue(SerialNumberPropertyKey, SyncModel.Device.SerialNumber);

            SetValue(TitlePropertyKey, SyncModel.Title);
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Properties)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProminentProperties)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProductString)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModelId)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SerialNumber)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerState)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerStateDescription)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerStateDetail)));
        }

        #endregion

        #region Public Properties

        public bool Initialized
        {
            get
            {
                return initialized;
            }
        }

        public TrippLiteUPS TrippLite
        {
            get
            {
                TrippLiteUPS TrippLiteRet = default;
                TrippLiteRet = SyncModel;
                return TrippLiteRet;
            }
        }

        #endregion

        #region Public Methods

        public TrippLitePropertyViewModel FindProperty(TrippLiteCodes c)
        {
            foreach (var m in Properties)
            {
                if (m.Code == c)
                    return m;
            }

            return null;
        }

        public void PromoteToLoad(TrippLiteCodes code, bool removeFromList = false)
        {
            TrippLitePropertyViewModel pm = null;

            foreach (var currentPm in Properties)
            {
                pm = currentPm;
                if (pm.Code == code)
                    break;
                pm = null;
            }

            if (pm is null)
                return;

            if (removeFromList)
                Properties.Remove(pm);

            LoadProperties.Add(pm);
        }

        public void DemoteFromLoad(TrippLiteCodes code)
        {
            TrippLitePropertyViewModel pm = null;

            foreach (var currentPm in ProminentProperties)
            {
                pm = currentPm;

                if (pm.Code == code)
                    break;

                pm = null;
            }

            if (pm is null)
                return;

            if (LoadProperties.Contains(pm))
                LoadProperties.Remove(pm);

            Properties.Add(pm);
        }

        public void PromoteProperty(TrippLiteCodes code)
        {
            TrippLitePropertyViewModel pm = null;

            foreach (var currentPm in Properties)
            {
                pm = currentPm;

                if (pm.Code == code)
                    break;

                pm = null;
            }

            if (pm is null)
                return;

            pm.Prominent = true;

            Properties.Remove(pm);
            ProminentProperties.Add(pm);
        }

        public void DemoteProperty(TrippLiteCodes code)
        {
            TrippLitePropertyViewModel pm = null;

            foreach (var currentPm in ProminentProperties)
            {
                pm = currentPm;

                if (pm.Code == code)
                    break;

                pm = null;
            }

            if (pm is null)
                return;

            pm.Prominent = false;

            ProminentProperties.Remove(pm);
            Properties.Add(pm);
        }

        #endregion

        #region Threading and Notification

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsTimerRunning
        {
            get => isRunning;
        }

        public void StopWatching()
        {
            if (wthread is null)
                return;

            isRunning = false;

            cts?.Cancel();
            cts = null;

            wthread = null;
        }

        public void StartWatching()
        {
            if (!initialized)
                return;

            long ti = TimerInterval;

            if (wthread is object)
            {
                return;
            }

            cts = new CancellationTokenSource();

            isRunning = true;

            wthread = new Thread(async () =>
            {
                long tinc = 0L;

                await Task.Delay(DelayStart, cts.Token);

                do
                {
                    if (!isRunning || Thread.CurrentThread.ThreadState == System.Threading.ThreadState.AbortRequested)
                        return;
                    if (!SyncModel.RefreshData(this))
                    {
                        isRunning = false;
                    }

                    if (!isRunning)
                    {
                        return;
                    }

                    if (tinc >= 10000L)
                    {
                        GC.Collect(2);
                        Thread.Sleep(0);
                        await Dispatcher.BeginInvoke(new Action(() =>
                        {
                            GC.Collect(2);
                            Thread.Sleep(0);
                        }));
                        tinc = 0L;
                    }

                    tinc += ti;
                    Thread.Sleep((int)ti);
                }
                while (!cts?.IsCancellationRequested ?? false);
            });

            wthread.IsBackground = true;
            wthread.SetApartmentState(ApartmentState.STA);

            wthread.Start();
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (waitInit is object)
                {
                    waitInit.Abort();
                    waitInit = null;
                }

                StopWatching();
                SyncModel.Dispose();
                SyncModel = null;
                if (disposing)
                {
                    ClearLoadBarProperty();
                    Properties = null;
                    ProminentProperties = null;
                }
            }

            GC.Collect(0);
            disposedValue = true;
        }

        ~TrippLiteViewModel()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(false);
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void OnPowerStateChanged(object sender, PowerStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                SetValue(PowerStatePropertyKey, SyncModel.PowerState);
                SetValue(PowerStateDescriptionPropertyKey, SyncModel.PowerStateDescription);
                SetValue(PowerStateDetailPropertyKey, SyncModel.PowerStateDetail);
                SetValue(UtilityColorPropertyKey, UtilityColorBackground);
                
                PowerStateChanged?.Invoke(this, e);
        
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerState)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerStateDescription)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PowerStateDetail)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UtilityColor)));
            });
        }
    }

    /// <summary>
    /// The object that represents the TrippLite property ViewModel
    /// </summary>
    /// <remarks></remarks>
    public class TrippLitePropertyViewModel : INotifyPropertyChanged, IDisposable, IChild<TrippLiteViewModel>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private TrippLiteProperty prop;

        private TrippLiteProperty PropSync
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return prop;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (prop != null)
                {
                    prop.PropertyChanged -= _Prop_PropertyChanged;
                }

                prop = value;
                if (prop != null)
                {
                    prop.PropertyChanged += _Prop_PropertyChanged;
                }
            }
        }

        private bool changed;
        private bool isProminent;

        private TrippLiteViewModel owner;

        private string displayValue = null;

        private PropertyChangedEventArgs dispEvent = new PropertyChangedEventArgs(nameof(DisplayValue));

        /// <summary>
        /// Creates a new object with the specified base object and owner.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="owner"></param>
        /// <remarks></remarks>
        internal TrippLitePropertyViewModel(TrippLiteProperty p, TrippLiteViewModel owner)
        {
            if (p is null)
            {
                throw new ArgumentException();
            }

            this.owner = owner;
            PropSync = p;

            if (PropSync.value == -1)
            {
                PropSync.value = 0L;
            }

            displayValue = PropSync.ToString(PropSync.NumberFormat, true);
        }

        /// <summary>
        /// Gets the parent of this object (IChild)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public TrippLiteViewModel Parent
        {
            get
            {
                return owner;
            }
            internal set
            {
                owner = value;
            }
        }

        TrippLiteViewModel IChild<TrippLiteViewModel>.Parent
        {
            get => owner;
            set => owner = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this property will actively trigger change events.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsActiveProperty
        {
            get
            {
                return PropSync.IsActiveProperty;
            }
            set
            {
                PropSync.IsActiveProperty = value;

                if (value)
                    SetDisplayValue();
            }
        }

        /// <summary>
        /// Indicates whether or not this property belongs to the prominent list.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Prominent
        {
            get
            {
                return isProminent;
            }
            internal set
            {
                isProminent = value;
            }
        }

        /// <summary>
        /// Returns the raw value of the HID property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public long Value
        {
            get
            {
                return PropSync.Value;
            }
        }

        /// <summary>
        /// Returns the HID feature code that the value of this property represents.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public TrippLiteCodes Code
        {
            get
            {
                return PropSync.Code;
            }
        }

        /// <summary>
        /// Returns the header or title of this property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Header
        {
            get
            {
                return PropSync.Description;
            }
        }

        /// <summary>
        /// Returns the formatted display value of this property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string DisplayValue
        {
            get
            {
                return displayValue;
            }
        }

        /// <summary>
        /// Set the display value of the current value of the property.
        /// </summary>
        /// <remarks></remarks>
        public void SetDisplayValue()
        {
            if (isProminent)
            {
                displayValue = PropSync.ToString(PropSync.NumberFormat, true);
            }
            else
            {
                displayValue = PropSync.ToString();
            }
        }

        /// <summary>
        /// Returns the formatted display value of this property.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string ToString()
        {
            return displayValue;
        }

        private void _Prop_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!PropSync.IsActiveProperty)
                return;
            SetDisplayValue();
            PropertyChanged?.Invoke(this, dispEvent);
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                PropSync.Dispose();
            }

            disposedValue = true;
        }

        ~TrippLitePropertyViewModel()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(false);
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Operators

        public static explicit operator ulong(TrippLitePropertyViewModel val)
        {
            return (ulong)val.PropSync.value;
        }

        public static implicit operator long(TrippLitePropertyViewModel val)
        {
            return val.PropSync.value;
        }

        public static explicit operator uint(TrippLitePropertyViewModel val)
        {
            return (uint)val.PropSync.value;
        }

        public static explicit operator int(TrippLitePropertyViewModel val)
        {
            return (int)val.PropSync.value;
        }

        #endregion

    }

    public class TrippLitePropertyBagViewModel : DependencyCollectionBase<TrippLitePropertyViewModel, TrippLiteViewModel>
    {
        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public TrippLitePropertyViewModel GetPropertyByCode(TrippLiteCodes c)
        {
            foreach (var v in this)
            {
                if (v.Code == c)
                    return v;
            }

            return null;
        }

        public TrippLitePropertyBagViewModel(TrippLiteViewModel owner) : base(owner)
        {
        }
    }

    public class LoadBarPropertyHandler
    {
        /// <summary>
        /// Creates a new object with the specified get and set minimum and maximum value delegate functions and optional intial value.
        /// </summary>
        /// <param name="getMinFunc">The get minimum delegate.</param>
        /// <param name="setMinFunc">The set minimum delegate.</param>
        /// <param name="getMaxFunc">The get maximum delegate.</param>
        /// <param name="setMaxFunc">The set maximum delegate.</param>
        /// <param name="value">The optional initial value.</param>
        /// <remarks></remarks>
        public LoadBarPropertyHandler(GetMinimumValue getMinFunc, SetMinimumValue setMinFunc, GetMaximumValue getMaxFunc, SetMaximumValue setMaxFunc, double value = 0d)
        {
            setMax = setMaxFunc;
            getMax = getMaxFunc;

            setMin = setMinFunc;
            getMin = getMinFunc;

            this.value = value;
        }

        /// <summary>
        /// Creates a new object with the specified minimum, maximum and initial values.
        /// </summary>
        /// <param name="minVal">Minimum value.</param>
        /// <param name="maxVal">Maximum value.</param>
        /// <param name="value">Default value.</param>
        /// <remarks></remarks>
        public LoadBarPropertyHandler(double minVal, double maxVal, double value)
        {
            getMin = new GetMinimumValue(() => min);
            getMax = new GetMaximumValue(() => max);

            setMin = new SetMinimumValue((v) => min = v);
            setMax = new SetMaximumValue((v) => max = v);

            MinValue = minVal;
            MaxValue = maxVal;

            if (value < minVal || value > maxVal)
            {
                throw new ArgumentOutOfRangeException("Value cannot be less than minVal or greater than maxVal");
            }

            this.value = value;
        }

        private double value;
        private double min;
        private double max;

        public delegate double GetMinimumValue();

        public delegate double GetMaximumValue();

        public delegate void SetMinimumValue(double v);

        public delegate void SetMaximumValue(double v);

        private GetMinimumValue getMin;
        private GetMaximumValue getMax;
        private SetMinimumValue setMin;
        private SetMaximumValue setMax;

        /// <summary>
        /// Creates a new object with the specified minimum and maximum values.
        /// </summary>
        /// <param name="minVal">Minimum value.</param>
        /// <param name="maxVal">Maximum value.</param>
        /// <remarks></remarks>
        public LoadBarPropertyHandler(double minVal, double maxVal) : this(minVal, maxVal, minVal)
        {
        }

        /// <summary>
        /// Gets or sets the minimum load bar value.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double MinValue
        {
            get
            {
                return getMin();
            }
            internal set
            {
                setMin(value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum load bar value.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double MaxValue
        {
            get
            {
                return getMax();
            }
            internal set
            {
                setMax(value);
            }
        }

        /// <summary>
        /// Gets or sets the current load bar value.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double Value
        {
            get
            {
                return value;
            }
            internal set
            {

                // ' in other circumstances, we may want to throw an exception.
                // ' however this is to report the load bar values, and there
                // ' are some circumstances where the load bar value may
                // ' be within an accepted physical threshold (for instance, voltage)
                // ' but beyond the ability of the graphic to produce.
                if (value < MinValue)
                {
                    value = MinValue;
                }
                else if (value > MaxValue)
                {
                    value = MaxValue;
                }

                this.value = value;
            }
        }

        /// <summary>
        /// Handle the load bar value for the given value.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public double HandleLoadBarValue(double v)
        {
            value = v;
            return HandleLoadBarValue();
        }

        /// <summary>
        /// Handle the load bar value for the current value.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public double HandleLoadBarValue()
        {
            double d = MaxValue - MinValue;
            double e = Value - MinValue;
 
            if (d < 0d)
            {
                throw new InvalidConstraintException("Maximum value is less than minimum value.");
            }

            return e / d;
        }

    }
}