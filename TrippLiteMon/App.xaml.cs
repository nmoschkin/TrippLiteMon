using DataTools.Scheduler;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;

namespace TrippLite
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {

            // Application-level events, such as Startup, Exit, and DispatcherUnhandledException
            // can be handled in this file.
            this.SessionEnding += Application_SessionEnding;
            this.Startup += Application_Startup;
        }
                
        new public static App Current
        {
            get => (App)Application.Current;
        }


        private PowerMon mainBigWindow;

        internal PowerMon PowerMonWindow
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return mainBigWindow;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (mainBigWindow != null)
                {
                    mainBigWindow.OpenCoolWindow -= OnRequestOpenCoolWindow;
                    mainBigWindow.LocationChanged -= OnMainWindowLocationChanged;
                    mainBigWindow.SizeChanged -= OnMainWindowSizeChanged;
                }

                mainBigWindow = value;

                if (mainBigWindow != null)
                {
                    mainBigWindow.OpenCoolWindow += OnRequestOpenCoolWindow;
                    mainBigWindow.LocationChanged += OnMainWindowLocationChanged;
                    mainBigWindow.SizeChanged += OnMainWindowSizeChanged;
                }
            }
        }

        private DesktopWindow mainCoolWindow;

        internal DesktopWindow CoolWindow
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return mainCoolWindow;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (mainCoolWindow != null)
                {
                    mainCoolWindow.OpenMainWindow -= OnRequestOpenMainWindow;
                    mainCoolWindow.LocationChanged -= OnCoolWindowLocationChanged;
                    mainCoolWindow.SizeChanged -= OnCoolWindowSizeChanged;
                }

                mainCoolWindow = value;
                if (mainCoolWindow != null)
                {
                    mainCoolWindow.OpenMainWindow += OnRequestOpenMainWindow;
                    mainCoolWindow.LocationChanged += OnCoolWindowLocationChanged;
                    mainCoolWindow.SizeChanged += OnCoolWindowSizeChanged;
                }
            }
        }

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Contains("/erologin"))
            {
                TaskTool.EnableOnStartup();
                Environment.Exit(0);
            }
            else if (e.Args.Contains("/drologin"))
            {
                TaskTool.DisableOnStartup();
                Environment.Exit(0);
            }

            System.Windows.Forms.Application.EnableVisualStyles();

            if (e.Args.Contains("/reset"))
            {
                Settings.CoolWindowBounds = new System.Drawing.RectangleF(0, 0, 0, 0);
                Settings.PrimaryWindowBounds = new System.Drawing.RectangleF(0, 0, 0, 0);
                Settings.LastWindow = LastWindowType.Main;
                Settings.PowerDevices = new PowerDeviceIdEntry[0];
            }

            if (Settings.LastWindow == LastWindowType.Cool)
            {
                SwitchToCool();
            }
            else
            {
                SwitchToMain();
            }
        }

        internal bool CheckBattery(bool invokePicker = false)
        {
            var devices = Settings.PowerDevices;
            if (devices == null || devices.Length == 0)
            {
                if (invokePicker)
                {

                    return ShowBatteryPicker();

                }

                return false;
            }

            return true;
        }

        internal bool ShowBatteryPicker()
        {
            var picker = new BatteryPicker();

            picker.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            picker.ShowDialog();

            if (picker.DialogResult != null)
            {
                return (bool)picker.DialogResult;
            }

            System.Threading.Thread.Sleep(100);
            GC.Collect(2);

            return false;
        }

        private void SwitchToMain()
        {
            var rcM = Settings.PrimaryWindowBounds;
            PowerMonWindow = new PowerMon();
            Application.Current.MainWindow = PowerMonWindow;

            PowerMonWindow.Show();
            Settings.LastWindow = LastWindowType.Main;

            if (CoolWindow is object)
            {
                CoolWindow.ViewModel.StopWatching();
                CoolWindow.Close();
                CoolWindow.ViewModel.Dispose();
                CoolWindow = null;
            }

            if (rcM.Width != 0f && rcM.Height != 0f)
            {
                PowerMonWindow.Left = rcM.Left;
                PowerMonWindow.Top = rcM.Top;
                // _Main.Width = rcM.Width
                // _Main.Height = rcM.Height
            }

            System.Threading.Thread.Sleep(100);
            GC.Collect(2);

            if (!CheckBattery())
            {
                PowerMonWindow.Hide();
                if (!ShowBatteryPicker()) Environment.Exit(0);
                PowerMonWindow.Show();
            }
        }

        private void SwitchToCool()
        {
            var rcC = Settings.CoolWindowBounds;

            CoolWindow = new DesktopWindow();
            Application.Current.MainWindow = PowerMonWindow;

            CoolWindow.Show();
            Settings.LastWindow = LastWindowType.Cool;

            if (PowerMonWindow is object)
            {
                PowerMonWindow.ViewModel.StopWatching();
                PowerMonWindow.Close();
                PowerMonWindow.ViewModel.Dispose();
                PowerMonWindow = null;
            }

            if (rcC.Width != 0f && rcC.Height != 0f)
            {
                CoolWindow.Left = rcC.Left;
                CoolWindow.Top = rcC.Top;
                // _Cool.Width = rcC.Width
                // _Cool.Height = rcC.Height
            }

            System.Threading.Thread.Sleep(100);
            GC.Collect(2);

            if (!CheckBattery())
            {
                CoolWindow.Hide();
                if (!ShowBatteryPicker()) Environment.Exit(0);
                CoolWindow.Show();
            }
        }

        private void OnRequestOpenMainWindow(object sender, EventArgs e)
        {
            SwitchToMain();
        }

        private void OnRequestOpenCoolWindow(object sender, EventArgs e)
        {
            SwitchToCool();
        }

        private void OnCoolWindowLocationChanged(object sender, EventArgs e)
        {
            Settings.CoolWindowBounds = new System.Drawing.RectangleF((float)CoolWindow.Left, (float)CoolWindow.Top, (float)CoolWindow.Width, (float)CoolWindow.Height);
        }

        private void OnCoolWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Settings.CoolWindowBounds = new System.Drawing.RectangleF((float)CoolWindow.Left, (float)CoolWindow.Top, (float)CoolWindow.Width, (float)CoolWindow.Height);
        }

        private void OnMainWindowLocationChanged(object sender, EventArgs e)
        {
            Settings.PrimaryWindowBounds = new System.Drawing.RectangleF((float)PowerMonWindow.Left, (float)PowerMonWindow.Top, (float)PowerMonWindow.Width, (float)PowerMonWindow.Height);
        }

        private void OnMainWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Settings.PrimaryWindowBounds = new System.Drawing.RectangleF((float)PowerMonWindow.Left, (float)PowerMonWindow.Top, (float)PowerMonWindow.Width, (float)PowerMonWindow.Height);
        }
    }
}
