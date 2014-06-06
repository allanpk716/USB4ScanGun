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

using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Threading;
using System.Threading.Tasks;

namespace HideKeyEvent
{
    /// <summary>
    /// Structure that defines key input with modifier keys
    /// </summary>
    public struct KeyAndState
    {
        public int Key;
        public byte[] KeyboardState;

        public KeyAndState(int key, byte[] state)
        {
            Key = key;
            KeyboardState = state;
        }
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WM_KEYDOWN = 0x0100;

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern bool SetKeyboardState(byte[] lpKeyState);

        private IntPtr _handle;
        private bool _isMonitoring = true;

        private Queue<KeyAndState> _eventQ = new Queue<KeyAndState>();

        public MainWindow()
        {
            InitializeComponent();

            this.Focusable = true;
            this.Loaded += KeyEventQueueDemo_Loaded;
            this.PreviewKeyDown += KeyEventQueueDemo_PreviewKeyDown;
            this.btn.Click += (s, e) => ReplayKeyEvents();
        }

        void KeyEventQueueDemo_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus(); // necessary to detect previewkeydown event
            SetFocusable(false); // for demo purpose only, so controls do not get focus at tab key

            // getting window handle
            HwndSource source = (HwndSource)HwndSource.FromVisual(this);
            _handle = source.Handle;
        }

        /// <summary>
        /// Get key and keyboard state (modifier keys), store them in a queue
        /// </summary>
        void KeyEventQueueDemo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_isMonitoring)
            {
                int key = KeyInterop.VirtualKeyFromKey(e.Key);
                byte[] state = new byte[256];
                GetKeyboardState(state);
                _eventQ.Enqueue(new KeyAndState(key, state));
            }
        }

        /// <summary>
        /// Replay key events from queue
        /// </summary>
        private void ReplayKeyEvents()
        {
            _isMonitoring = false; // no longer add to queue
            SetFocusable(true); // allow controls to take focus now (demo purpose only)

            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)); // set focus to first control

            // thread the dequeueing, because the sequence of inputs is not preserved 
            // unless a small delay between them is introduced. Normally the effect this
            // produces should be very acceptable for an UI.
            //Task.Run(() =>
            ThreadPool.QueueUserWorkItem((o) =>
            {
                while (_eventQ.Count > 0)
                {
                    KeyAndState keyAndState = _eventQ.Dequeue();

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        SetKeyboardState(keyAndState.KeyboardState); // set stored keyboard state
                        PostMessage(_handle, WM_KEYDOWN, keyAndState.Key, 0);
                    }));

                    System.Threading.Thread.Sleep(5); // might need adjustment
                }
            });
        }

        /// <summary>
        /// Prevent controls from getting focus and taking the input until requested
        /// </summary>
        private void SetFocusable(bool isFocusable)
        {
            tbOne.Focusable = isFocusable;
            tbTwo.Focusable = isFocusable;
            btn.Focusable = isFocusable;
        }
    }
}
