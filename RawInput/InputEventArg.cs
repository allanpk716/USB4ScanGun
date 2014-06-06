using System;

namespace RawInput_dll
{
    public class InputEventArg : EventArgs
    {
        public InputEventArg(KeyPressEvent arg)
        {
            KeyPressEvent = arg;
        }

        private InputEventArg() { }

        public KeyPressEvent KeyPressEvent { get; private set; }
    }
}
