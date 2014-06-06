using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibUsbDotNet.DeviceNotify;

namespace Notification
{
    public partial class Form1 : Form
    {

        private IDeviceNotifier devNotifier;

        delegate void AppendNotifyDelegate(string s);


        public Form1()
        {
            InitializeComponent();

            devNotifier = DeviceNotifier.OpenDeviceNotifier();

        //    devNotifier.OnDeviceNotify += onDevNotify;

            devNotifier.OnDeviceNotify += new EventHandler<DeviceNotifyEventArgs>(onDevNotify);
        }

        private void onDevNotify(object sender, DeviceNotifyEventArgs e)
        {
            Invoke(new AppendNotifyDelegate(AppendNotifyText), new object[] { e.ToString() });
        }

        private void AppendNotifyText(string s)
        {
            tNotify.AppendText(s);
        }

        private void tNotify_DoubleClick(object sender, EventArgs e)
        {
            tNotify.Clear();
        }

        private void tNotify_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
