using System;
using System.Windows.Forms;

namespace Multitool.NTInterop
{
    internal class DummyWindow : NativeWindow
    {
        public DummyWindow()
        {
            CreateHandle(new CreateParams());
        }

        public event EventHandler<Message> WndProcCalled;

        protected override void WndProc(ref Message m)
        {
            WndProcCalled?.Invoke(this, m);
            base.WndProc(ref m);
        }
    }
}
