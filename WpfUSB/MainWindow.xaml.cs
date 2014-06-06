using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;

using System.Runtime.InteropServices;

namespace WpfUSB
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
         //   HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);

            if (source != null)
            {
                source.AddHook(WndProc);
            }

            base.OnSourceInitialized(e);
        }

        public class Win32
        {
            public const int WM_DEVICECHANGE = 0x0219;
            public const int WM_CHAR = 0x0102;
            public const int WM_KEYDOWN = 0x0100;
            public const int WM_KEYUP = 0x0101;

            public const int WM_INPUT = 0x00FF;

            public const int RI_KEY_MAKE = 0x00;
            public const int RI_KEY_BREAK = 0x01;

            public const int DBT_DEVICEARRIVAL = 0x8000;
            public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
            public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
            public const int DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001;
            public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x00000004;
            public const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;
            public static Guid GUID_DEVINTERFACE_HID = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030");
        }


        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;

            //如果 handled 返回真，那么就已经处理这个消息，就不往下传递
//             switch (msg)
//             {
//                 case Win32.WM_DEVICECHANGE:
//                     {
// 
//                     }
//                     break;
//                 case Win32.WM_KEYUP:
//                 case Win32.WM_KEYDOWN:
//                     {
//                         handled = true;
//                         wParam = IntPtr.Zero;
//                         lParam = IntPtr.Zero;
//                     }
//                     break;
//                 case Win32.WM_INPUT:
//                     {
//                         handled = true;
//                         wParam = IntPtr.Zero;
//                         lParam = IntPtr.Zero;
//                     }
//                     break;
//             }

            //VirtualKeyFromKey 将 wpf key 转 win32 key

                if (msg == Win32.WM_KEYDOWN)
                {
                    if (wParam.ToInt32() == KeyInterop.VirtualKeyFromKey(Key.D6))
                    {
                        handled = true;
                    }
                }

                if (msg == Win32.WM_KEYUP)
                {
                    if (wParam.ToInt32() == (int)Key.D6)
                    {
                        handled = true;
                    }
                }

            return IntPtr.Zero;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D6)
            {
            }
            
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D6)
            {
            }
        }
    }
}
