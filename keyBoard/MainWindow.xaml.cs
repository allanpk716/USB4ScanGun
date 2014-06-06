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
using System.Windows.Interop;
using System.Diagnostics;
using System.Globalization;

namespace keyBoard
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private RawInput _rawinput;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnKeyPressed(object sender, InputEventArg e)
        {
//             lbHandle.Text = e.KeyPressEvent.DeviceHandle.ToString();
//             lbType.Text = e.KeyPressEvent.DeviceType;
//             lbName.Text = e.KeyPressEvent.DeviceName;
//             lbDescription.Text = e.KeyPressEvent.Name;
//             lbKey.Text = e.KeyPressEvent.VKey.ToString(CultureInfo.InvariantCulture);
//             lbNumKeyboards.Text = _rawinput.NumberOfKeyboards.ToString(CultureInfo.InvariantCulture);
//             lbVKey.Text = e.KeyPressEvent.VKeyName;
//             lbSource.Text = e.KeyPressEvent.Source;
//             lbKeyPressState.Text = e.KeyPressEvent.KeyPressState;
//             lbMessage.Text = string.Format("0x{0:X4} ({0})", e.KeyPressEvent.Message);

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

        private static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            if (null == ex) return;

            // Log this error. Logging the exception doesn't correct the problem but at least now
            // you may have more insight as to why the exception is being thrown.
            Debug.WriteLine("Unhandled Exception: " + ex.Message);
            Debug.WriteLine("Unhandled Exception: " + ex);
            MessageBox.Show(ex.Message);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _rawinput.KeyPressed -= new RawKeyboard.DeviceEventHandler(OnKeyPressed);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            IntPtr wpfHwnd = wndHelper.Handle;

            _rawinput = new RawInput(wpfHwnd);
            _rawinput.CaptureOnlyIfTopMostWindow = false;    // Otherwise default behavior is to capture always
        //    _rawinput.AddMessageFilter();                   // Adding a message filter will cause keypresses to be handled
        //    _rawinput.KeyPressed += OnKeyPressed;

            _rawinput.KeyPressed += new RawKeyboard.DeviceEventHandler(OnKeyPressed);

            Win32.DeviceAudit();                            // Writes a file DeviceAudit.txt to the current directory
        }
    }
}
