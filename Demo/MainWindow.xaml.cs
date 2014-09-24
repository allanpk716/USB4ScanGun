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
using RawInput_dll;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Threading;
using Microsoft.Win32;

namespace Demo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dispatcher dispatcher;
        private static RawKeyboard _keyboardDriver;
        private static readonly Guid DeviceInterfaceHid = new Guid("4D1E55B2-F86F-11CF-88CB-001111234531");
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

        public MainWindow()
        {
            InitializeComponent();

            str_ScanGunID = Properties.Settings.Default.MyScanGun;

            textBox_ScanGunInfoCali.Text = str_ScanGunID;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CalibrationScanGun win = new CalibrationScanGun();
            win.Owner = Window.GetWindow(sender as Button);
            //窗体关闭后，获取 ScanGun ID
            str_ScanGunID = win.ShowDialog();

            //如果返回的ID有值，那么就保存
            if (str_ScanGunID != string.Empty)
            {
                Properties.Settings.Default.MyScanGun = str_ScanGunID;
                Properties.Settings.Default.Save();

                textBox_ScanGunInfoCali.Text = str_ScanGunID;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MyScanGun = str_ScanGunID = string.Empty;
            Properties.Settings.Default.Save();
            textBox_ScanGunInfoCali.Text = string.Empty;
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

            DisplayMemory();
            Console.WriteLine();
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("--- New Listener #{0} ---", i + 1);

                var listener = new TestListener(new TestClassHasEvent());
                ////listener = null; //可有可无    

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                DisplayMemory();

            }
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

                        textBox_ScanGunInfoNow.Text = keyPressEvent.DeviceName;

                        //只处理一次事件，不然有按下和弹起事件
                        if (keyPressEvent.KeyPressState == "MAKE" && keyPressEvent.DeviceName == str_ScanGunID && str_ScanGunID != string.Empty)
                        {
                            textBox_Output.Focus();

                            //找到结尾标志的时候，就不加入队列了，然后就发送到界面上赋值
                            if (KeyInterop.KeyFromVirtualKey(keyPressEvent.VKey) == Key.Enter)
                            {
                                _isMonitoring = false;

                                string str_Out = string.Empty;

                                ThreadPool.QueueUserWorkItem((o) =>
                                {
                                    while (_eventQ.Count > 0)
                                    {
                                        RawInput_dll.Win32.KeyAndState keyAndState = _eventQ.Dequeue();

                                        str_Out += CalibrationScanGun.Chr(keyAndState.Key).ToString();

                                        System.Threading.Thread.Sleep(5); // might need adjustment
                                    }

                                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                    {
                                        textBox_Output.Text = str_Out;
                                    }));

                                    _eventQ.Clear();

                                    _isMonitoring = true;
                                });
                            }

                            // 回车 作为结束标志
                            if (_isMonitoring)
                            {
                                //存储 Win32 按键的int值
                                int key = keyPressEvent.VKey;
                                byte[] state = new byte[256];
                                Win32.GetKeyboardState(state);
                                _eventQ.Enqueue(new RawInput_dll.Win32.KeyAndState(key, state));
                            }
                        }
                    }
                    break;
                case Win32.WM_KEYDOWN:
                    {
                        KeyPressEvent keyPressEvent;
                        _keyboardDriver.ProcessRawInput(msg.lParam, out keyPressEvent);

                        if (keyPressEvent.DeviceName == str_ScanGunID && str_ScanGunID != string.Empty)
                        {
                            handled = true;
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



        static void DisplayMemory()
        {
            Console.WriteLine("Total memory: {0:###,###,###,##0} bytes", GC.GetTotalMemory(true));
        }

        class TestClassHasEvent
        {
            public delegate void TestEventHandler(object sender, EventArgs e);
            public event TestEventHandler YourEvent;
            protected void OnYourEvent(EventArgs e)
            {
                if (YourEvent != null) YourEvent(this, e);
            }
        }

        class TestListener
        {
            byte[] m_ExtraMemory = new byte[1000000];

            private TestClassHasEvent _inject;

            public TestListener(TestClassHasEvent inject)
            {
                SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);
            }

            void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
            {

            } 

            void _inject_YourEvent(object sender, EventArgs e)
            {

            }
        }
    }
}
