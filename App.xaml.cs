using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace MouseEnhanceToggler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [Flags]
        public enum SPIF
        {
            None = 0x00,
            /// <summary>Writes the new system-wide parameter setting to the user profile.</summary>
            SPIF_UPDATEINIFILE = 0x01,
            /// <summary>Broadcasts the WM_SETTINGCHANGE message after updating the user profile.</summary>
            SPIF_SENDCHANGE = 0x02,
            /// <summary>Same as SPIF_SENDCHANGE.</summary>
            SPIF_SENDWININICHANGE = 0x02
        }

        // http://stackoverflow.com/questions/24737775/toggle-enhance-pointer-precision
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = true)]
        public static extern bool SystemParametersInfoGet(uint action, uint param, IntPtr vparam, SPIF fWinIni);
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = true)]
        public static extern bool SystemParametersInfoSet(uint action, uint param, IntPtr vparam, SPIF fWinIni);

        public const UInt32 SPI_GETMOUSE = 0x0003;
        public const UInt32 SPI_SETMOUSE = 0x0004;

        public static bool ToggleEnhancePointerPrecision(bool b)
        {
            int[] mouseParams = new int[3];
            // Get the current values.
            SystemParametersInfoGet(SPI_GETMOUSE, 0, GCHandle.Alloc(mouseParams, GCHandleType.Pinned).AddrOfPinnedObject(), 0);
            // Modify the acceleration value as directed.
            mouseParams[2] = b ? 1 : 0;
            // Update the system setting.
            return SystemParametersInfoSet(SPI_SETMOUSE, 0, GCHandle.Alloc(mouseParams, GCHandleType.Pinned).AddrOfPinnedObject(), SPIF.SPIF_SENDCHANGE);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "MouseEnhanceToggler",
                System.Reflection.Assembly.GetExecutingAssembly().Location);

            mouseIconEnabled = System.Drawing.Icon.FromHandle(MouseEnhanceToggler.Properties.Resources.MouseIconEnabled.GetHicon());
            mouseIconDisabled = System.Drawing.Icon.FromHandle(MouseEnhanceToggler.Properties.Resources.MouseIconDisabled.GetHicon());

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Click += NotifyIcon_Click;

            var menuItems = new System.Windows.Forms.MenuItem[1];
            menuItems[0] = new System.Windows.Forms.MenuItem("Quit", (obj, evt) => { Application.Current.Shutdown(); });
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(menuItems);
            UpdateNotifyIcon();
            notifyIcon.Visible = true;

            base.OnStartup(e);
        }

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Drawing.Icon mouseIconEnabled;
        private System.Drawing.Icon mouseIconDisabled;

        public void UpdateNotifyIcon()
        {
            notifyIcon.Icon = IsMouseEnhanceActive() ? mouseIconEnabled : mouseIconDisabled;
        }

        public bool IsMouseEnhanceActive()
        {
            int[] mouseParams = new int[3];
            SystemParametersInfoGet(SPI_GETMOUSE, 0, GCHandle.Alloc(mouseParams, GCHandleType.Pinned).AddrOfPinnedObject(), 0);
            return mouseParams[2] == 1;
        }

        public void EnabledMouseEnhance()
        {
            ToggleEnhancePointerPrecision(true);
        }

        public void DisableMouseEnhance()
        {
            ToggleEnhancePointerPrecision(false);
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            var me = e as System.Windows.Forms.MouseEventArgs;
            if (me.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (IsMouseEnhanceActive())
                {
                    DisableMouseEnhance();
                }
                else
                {
                    EnabledMouseEnhance();
                }
                UpdateNotifyIcon();
            }
        }

    }
}
