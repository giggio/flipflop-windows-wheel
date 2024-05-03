using System.Data;
using Microsoft.Win32;
using Timer = System.Windows.Forms.Timer;

namespace WindowsFormsApplication1
{
    public class Flippable
    {
        public Flippable(string[] keyPath, RegistryKey deviceKey, RegistryKey devparam, Timer timer)
        {
            this._keyPath = keyPath;
            IEnumerable<bool?> flipValues = Flippable.valueNames
                .Select(v => OnlyIntBool(devparam.GetValue(v, null)));
            this.Name = (string)deviceKey.GetValue("DeviceDesc");
            this._vertical = flipValues.ElementAt(0);
            this._horizontal = flipValues.ElementAt(1);
            this._timer = timer;
        }

        private static readonly string[] valueNames = ["FlipFlopWheel", "FlipFlopHScroll"];

        public string Name { get; private set; }
        private readonly string[] _keyPath;
        private bool? _vertical;
        private bool? _horizontal;
        readonly Timer _timer;

        private static bool? OnlyIntBool(object value)
        {
            try
            {
                return value == null ? null : (bool?)(((int)value) != 0);
            }
            catch
            {
                return null;
            }
        }

        public bool? Vertical { set { Flip(valueNames[0], value); _vertical = value; } get => _vertical; }
        public bool? Horizontal { set { Flip(valueNames[1], value); _horizontal = value; } get => _horizontal; }

        public void Flip(string valueName, bool? value)
        {
            using RegistryKey hid = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\HID\", false);
            using RegistryKey device = hid.OpenSubKey(_keyPath[0], false);
            using RegistryKey device2 = device.OpenSubKey(_keyPath[1], false);
            using RegistryKey devparam = device2.OpenSubKey("Device Parameters", true);
            if (value == null)
            {
                devparam.DeleteValue(valueName);
            }
            else
            {
                devparam.SetValue(valueName, value == true ? 1 : 0);
                _timer.Enabled = true;
            }
        }
    }
}

