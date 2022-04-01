using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using DataTools.MessageBoxEx;
using DataTools.Scheduler;

namespace TrippLite
{
    public class TrippLiteViewModel : DependencyObject, INotifyPropertyChanged, IDisposable
    {
        private bool initialized = false;
        private Thread waitInit;
        private TrippLiteUPS trippLite;

        public TrippLiteUPS SyncModel
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
        public static readonly DependencyProperty RunOnStartupProperty = DependencyProperty.Register("RunOnStartup", typeof(bool), typeof(TrippLiteViewModel), new PropertyMetadata(TaskTool.GetIsEnabled(), OnRunOnStartupChanged));


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

        public TrippLiteViewModel() : this(false, null)
        {
        }

        public TrippLiteViewModel(string devicePath) : this(false, devicePath)
        {
        }

        public TrippLiteViewModel(bool init, string? devicePath)
        {


            Properties = new TrippLitePropertyBagViewModel(this);
            ProminentProperties = new TrippLitePropertyBagViewModel(this);
            LoadProperties = new TrippLitePropertyBagViewModel(this);

            if (init)
                Initialize(devicePath);
        }

        public bool Initialize(string? devicePath = null)
        {


            if (initialized)
                return false;
            try
            {
                SyncModel = new TrippLiteUPS(false);
                if (SyncModel != null && SyncModel.Connected == false)
                {
                    waitInit = new Thread(() =>
                    {
                        for (var i = 0; i < MaxTries; i++)
                        {
                            Thread.Sleep(100);
                            try
                            {
                                SyncModel.Connect();
                            }
                            catch (Exception ex)
                            {
                                MessageBoxEx.Show(
                                    $"Error Opening HID Battery: {ex.Message}",
                                    "Initialization Failure",
                                    MessageBoxExType.OK,
                                    MessageBoxExIcons.Exclamation);
                            }

                            if (SyncModel is object && SyncModel.Connected)
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
                if (p.Code != BatteryPropertyCodes.InputVoltage && p.Code != BatteryPropertyCodes.OutputVoltage)
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

            PromoteToLoad(BatteryPropertyCodes.OutputPower);
            
            MakeLoadBarProperty(FindProperty(BatteryPropertyCodes.OutputLoad));

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

        public TrippLitePropertyViewModel FindProperty(BatteryPropertyCodes c)
        {
            foreach (var m in Properties)
            {
                if (m.Code == c)
                    return m;
            }

            return null;
        }

        public void PromoteToLoad(BatteryPropertyCodes code, bool removeFromList = false)
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

        public void DemoteFromLoad(BatteryPropertyCodes code)
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

        public void PromoteProperty(BatteryPropertyCodes code)
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

        public void DemoteProperty(BatteryPropertyCodes code)
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

    
}