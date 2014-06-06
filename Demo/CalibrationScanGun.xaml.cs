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
using System.Windows.Shapes;

using System.Windows.Threading;
using System.Windows.Interop;
using RawInput_dll;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace Demo
{
    /// <summary>
    /// CalibrationScanGun.xaml 的交互逻辑
    /// </summary>
    public partial class CalibrationScanGun : Window
    {
        private Dispatcher dispatcher;
        private static RawKeyboard _keyboardDriver;
        private static readonly Guid DeviceInterfaceHid = new Guid("4D1E55B2-F86F-11CF-88CB-001111234530");
        //临时储存 扫描仪 的硬件信息
        private string str_ScanGunID = string.Empty;

        //本窗体的句柄
        private IntPtr _wpfHwnd = IntPtr.Zero;
        //输入队列
        private bool _isMonitoring = true;

        private Queue<RawInput_dll.Win32.KeyAndState> _eventQ = new Queue<RawInput_dll.Win32.KeyAndState>();

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

        public CalibrationScanGun()
        {
            InitializeComponent();
        }

        //返回 设备名称
        new public string ShowDialog()
        {
            base.ShowDialog();
            return str_ScanGunID;
        }

        //这里是为了截获 消息，主要是为了获取 deviceChange 的消息，因为在之前获取有问题
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.dispatcher == null)
            {
                this.dispatcher = this.Dispatcher;
            }

            //获取本窗体的句柄
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            _wpfHwnd = wndHelper.Handle;

            _keyboardDriver = new RawKeyboard(_wpfHwnd);
            _keyboardDriver.CaptureOnlyIfTopMostWindow = false;
            _keyboardDriver.EnumerateDevices();

            //之所以不在 WndProc 进行消息的拦截，是因为···在 WPF 中，消息到 WndProc 的时候，都已经显示在界面上了
            //当然，在 WinForm 程序上还是有效的，所以在这里，就 在这个消息中截取
            System.Windows.Interop.ComponentDispatcher.ThreadFilterMessage +=
                    new System.Windows.Interop.ThreadMessageEventHandler(ComponentDispatcher_ThreadFilterMessage);
        }

        void ComponentDispatcher_ThreadFilterMessage(ref System.Windows.Interop.MSG msg, ref bool handled)
        {
            switch (msg.message)
            {
                //这里只能以 INPUT 来截取，不支持 KEYDOWN 来截取，不然后面的 RawInput 获取值的时候无效
                case Win32.WM_INPUT:
                    {
                        // Should never get here if you are using PreMessageFiltering
                        KeyPressEvent keyPressEvent;
                        _keyboardDriver.ProcessRawInput(msg.lParam, out keyPressEvent);

                        if (KeyInterop.KeyFromVirtualKey(keyPressEvent.VKey) == Key.Enter)
                        {
                            _isMonitoring = false;
                            Btn_OK.IsEnabled = true;
                        }

                        // 回车 作为结束标志
                        if (_isMonitoring && keyPressEvent.KeyPressState == "BREAK")
                        {
                            //存储 Win32 按键的int值
                            int key = keyPressEvent.VKey;
                            byte[] state = new byte[256];
                            Win32.GetKeyboardState(state);
                            _eventQ.Enqueue(new RawInput_dll.Win32.KeyAndState(key, state));
                        }

                        if (_isMonitoring == false)
                        {
                            str_ScanGunID = keyPressEvent.DeviceName;
                        }
                    }
                    break;
                //这里截获这个消息有问题，替代方法是放到 WndProc 中去获取
                case Win32.WM_USB_DEVICECHANGE:
                    {

                    }
                    break;
                default:
                    break;
            }
            return;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;

            //如果 handled 返回真，那么就已经处理这个消息，就不往下传递
            switch (msg)
            {
                case Win32.WM_USB_DEVICECHANGE:
                    {
                        //如果有设备变动，那么就重新枚举一次设备
                        _keyboardDriver.EnumerateDevices();
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            //清空队列
            _eventQ.Clear();
            //设置确认按钮不可点击
            Btn_OK.IsEnabled = false;
            //设置允许监听添加队列
            _isMonitoring = true;

            textBox_KeyData.Focus();
        }

        private void Btn_OK_Click(object sender, RoutedEventArgs e)
        {
            string str_Out = string.Empty;

            ThreadPool.QueueUserWorkItem((o) =>
            {
                while (_eventQ.Count > 0)
                {
                    RawInput_dll.Win32.KeyAndState keyAndState = _eventQ.Dequeue();

                    str_Out += Chr(keyAndState.Key).ToString();

                    System.Threading.Thread.Sleep(5); // might need adjustment
                }

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    textBox_KeyData.Text = str_Out;
                }));
            });
        }

        public static string Chr(int asciiCode)
        {
            if (asciiCode >= 0 && asciiCode <= 255)
            {
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                byte[] byteArray = new byte[] { (byte)asciiCode };
                string strCharacter = asciiEncoding.GetString(byteArray);
                return (strCharacter);
            }
            else
            {
                throw new Exception("ASCII Code is not valid.");
            }
        }
    }
}
