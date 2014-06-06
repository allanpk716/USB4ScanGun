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

using System.Windows.Threading;
using System.Windows.Interop;
using RawInput_dll;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WpfGetWLParam
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static RawKeyboard _keyboardDriver;
        private IntPtr _devNotifyHandle;
        private static readonly Guid DeviceInterfaceHid = new Guid("4D1E55B2-F86F-11CF-88CB-001111000030");

        public event RawKeyboard.DeviceEventHandler KeyPressed
        {
            add { _keyboardDriver.KeyPressed += value; }
            remove { _keyboardDriver.KeyPressed -= value; }
        }

        public int NumberOfKeyboards
        {
            get { return _keyboardDriver.NumberOfKeyboards; }
        }

        public bool CaptureOnlyIfTopMostWindow
        {
            get { return _keyboardDriver.CaptureOnlyIfTopMostWindow; }
            set { _keyboardDriver.CaptureOnlyIfTopMostWindow = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        ~MainWindow()
        {
            if (_devNotifyHandle != null)
            {
                Win32.UnregisterDeviceNotification(_devNotifyHandle);
            }
        }

        Dispatcher dispatcher;
        DispatcherHooks dispatcherHooks;

        const int WM_KEYDOWN = 0x100;
        const int WM_SYSKEYDOWN = 0x0104;
        const int WM_USB_DEVICECHANGE = 0x0219;


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.dispatcher == null)
            {
                this.dispatcher = this.Dispatcher;
                this.dispatcherHooks = this.dispatcher.Hooks;
            }

            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            IntPtr wpfHwnd = wndHelper.Handle;

            _keyboardDriver = new RawKeyboard(wpfHwnd);
            _keyboardDriver.EnumerateDevices();
            _devNotifyHandle = RegisterForDeviceNotifications(wpfHwnd);

            System.Windows.Interop.ComponentDispatcher.ThreadFilterMessage +=
                    new System.Windows.Interop.ThreadMessageEventHandler(ComponentDispatcher_ThreadFilterMessage);
            System.Windows.Interop.ComponentDispatcher.ThreadPreprocessMessage +=
                    new System.Windows.Interop.ThreadMessageEventHandler(ComponentDispatcher_ThreadPreprocessMessage);

            this.KeyPressed += new RawKeyboard.DeviceEventHandler(OnKeyPressed);
        }

        private void OnKeyPressed(object sender, InputEventArg e)
        {
            switch (e.KeyPressEvent.Message)
            {
                case Win32.WM_KEYDOWN:
                    /*Debug.WriteLine(e.KeyPressEvent.KeyPressState);*/
                    break;
                case Win32.WM_KEYUP:
                    textBox.Text += e.KeyPressEvent.VKeyName.ToString() + "\n";
                    textBox.ScrollToEnd();
                    break;
            }
        }

        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            switch (msg.message)
            {
                case Win32.WM_INPUT:
                    {
                        handled = true;
                    }
                    break;

                case WM_USB_DEVICECHANGE:
                    {
                        Debug.WriteLine("USB Device Arrival / Removal");
                        _keyboardDriver.EnumerateDevices();
                    }
                    break;
                default:
                    break;
            }
            return;
        }
        void ComponentDispatcher_ThreadFilterMessage(ref System.Windows.Interop.MSG msg, ref bool handled)
        {
            switch (msg.message)
            {
                case Win32.WM_INPUT:
                    {
                        // Should never get here if you are using PreMessageFiltering
                        _keyboardDriver.ProcessRawInput(msg.lParam);
                        this.OnWM_MESSAGE256(ref msg, ref handled);//handled = 
                    }
                    break;

                case WM_USB_DEVICECHANGE:
                    {
                        Debug.WriteLine("USB Device Arrival / Removal");
                        _keyboardDriver.EnumerateDevices();
                    }
                    break;
                default:
                    break;
            }
            return;
        }

        bool OnWM_MESSAGE256(ref System.Windows.Interop.MSG msg, ref bool handled)
        {
            // // add extra variables to observe with Debugger // 
            IntPtr HWND, WParam, LParam;

            HWND = msg.hwnd;
            WParam = msg.wParam;
            LParam = msg.lParam;
            this.txt_HWND.Text = HWND.ToString(); 
            this.txt_WParam.Text = WParam.ToString();
            this.txt_LParam.Text = LParam.ToString();
            this.txt_KeyData.Text = KeyInterop.KeyFromVirtualKey(WParam.ToInt32()).ToString();//keyData.ToString();

            return true;
        }

        static IntPtr RegisterForDeviceNotifications(IntPtr parent)
        {
            var usbNotifyHandle = IntPtr.Zero;
            var bdi = new BroadcastDeviceInterface();
            bdi.dbcc_size = Marshal.SizeOf(bdi);
            bdi.BroadcastDeviceType = BroadcastDeviceType.DBT_DEVTYP_DEVICEINTERFACE;
            bdi.dbcc_classguid = DeviceInterfaceHid;

            var mem = IntPtr.Zero;
            try
            {
                mem = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BroadcastDeviceInterface)));
                Marshal.StructureToPtr(bdi, mem, false);
                usbNotifyHandle = Win32.RegisterDeviceNotification(parent, mem, DeviceNotification.DEVICE_NOTIFY_WINDOW_HANDLE);
            }
            catch (Exception e)
            {
                Debug.Print("Registration for device notifications Failed. Error: {0}", Marshal.GetLastWin32Error());
                Debug.Print(e.StackTrace);
            }
            finally
            {
                Marshal.FreeHGlobal(mem);
            }

            if (usbNotifyHandle == IntPtr.Zero)
            {
                Debug.Print("Registration for device notifications Failed. Error: {0}", Marshal.GetLastWin32Error());
            }

            return usbNotifyHandle;
        }

    }
}
