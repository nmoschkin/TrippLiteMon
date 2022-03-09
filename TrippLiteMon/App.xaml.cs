using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private PowerMon __Main;

        private PowerMon _Main
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return __Main;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (__Main != null)
                {
                    __Main.OpenCoolWindow -= _Main_OpenCoolWindow;
                    __Main.LocationChanged -= _Main_LocationChanged;
                    __Main.SizeChanged -= _Main_SizeChanged;
                }

                __Main = value;
                if (__Main != null)
                {
                    __Main.OpenCoolWindow += _Main_OpenCoolWindow;
                    __Main.LocationChanged += _Main_LocationChanged;
                    __Main.SizeChanged += _Main_SizeChanged;
                }
            }
        }

        private DesktopWindow __Cool;

        private DesktopWindow _Cool
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return __Cool;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (__Cool != null)
                {
                    __Cool.OpenMainWindow -= _Cool_OpenMainWindow;
                    __Cool.LocationChanged -= _Cool_LocationChanged;
                    __Cool.SizeChanged -= _Cool_SizeChanged;
                }

                __Cool = value;
                if (__Cool != null)
                {
                    __Cool.OpenMainWindow += _Cool_OpenMainWindow;
                    __Cool.LocationChanged += _Cool_LocationChanged;
                    __Cool.SizeChanged += _Cool_SizeChanged;
                }
            }
        }

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            System.Windows.Forms.Application.EnableVisualStyles();

            if (e.Args.Contains("/reset"))
            {
                Settings.CoolWindowBounds = new System.Drawing.RectangleF(0, 0, 0, 0);
                Settings.PrimaryWindowBounds = new System.Drawing.RectangleF(0, 0, 0, 0);
                Settings.LastWindow = LastWindowType.Main;
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

        private void SwitchToMain()
        {
            var rcM = Settings.PrimaryWindowBounds;
            _Main = new PowerMon();
            _Main.Show();
            Settings.LastWindow = LastWindowType.Main;
            if (_Cool is object)
            {
                _Cool.ViewModel.StopWatching();
                _Cool.Close();
                _Cool.ViewModel.Dispose();
                _Cool = null;
            }

            if (rcM.Width != 0f && rcM.Height != 0f)
            {
                _Main.Left = rcM.Left;
                _Main.Top = rcM.Top;
                // _Main.Width = rcM.Width
                // _Main.Height = rcM.Height
            }

            System.Threading.Thread.Sleep(100);
            GC.Collect(2);
        }

        private void SwitchToCool()
        {
            var rcC = Settings.CoolWindowBounds;
            _Cool = new DesktopWindow();
            _Cool.Show();
            Settings.LastWindow = LastWindowType.Cool;
            if (_Main is object)
            {
                _Main.ViewModel.StopWatching();
                _Main.Close();
                _Main.ViewModel.Dispose();
                _Main = null;
            }

            if (rcC.Width != 0f && rcC.Height != 0f)
            {
                _Cool.Left = rcC.Left;
                _Cool.Top = rcC.Top;
                // _Cool.Width = rcC.Width
                // _Cool.Height = rcC.Height
            }

            System.Threading.Thread.Sleep(100);
            GC.Collect(2);
        }

        private void _Cool_OpenMainWindow(object sender, EventArgs e)
        {
            SwitchToMain();
        }

        private void _Main_OpenCoolWindow(object sender, EventArgs e)
        {
            SwitchToCool();
        }

        private void _Cool_LocationChanged(object sender, EventArgs e)
        {
            Settings.CoolWindowBounds = new System.Drawing.RectangleF((float)_Cool.Left, (float)_Cool.Top, (float)_Cool.Width, (float)_Cool.Height);
        }

        private void _Cool_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Settings.CoolWindowBounds = new System.Drawing.RectangleF((float)_Cool.Left, (float)_Cool.Top, (float)_Cool.Width, (float)_Cool.Height);
        }

        private void _Main_LocationChanged(object sender, EventArgs e)
        {
            Settings.PrimaryWindowBounds = new System.Drawing.RectangleF((float)_Main.Left, (float)_Main.Top, (float)_Main.Width, (float)_Main.Height);
        }

        private void _Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Settings.PrimaryWindowBounds = new System.Drawing.RectangleF((float)_Main.Left, (float)_Main.Top, (float)_Main.Width, (float)_Main.Height);
        }
    }
}
