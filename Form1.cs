using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;

namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private Flippable[] flippable;
        private void Form1_Load(object sender, EventArgs e)
        {
            WindowsPrincipal pricipal = new(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!hasAdministrativeRight)
            {
                RunElevated(Application.ExecutablePath);
                this.Close();
                Application.Exit();
            }

            //probably only want to flip mice.
            flippable = GetFlippable("hid.mousedevice");
            dgv_flippable.DataSource = flippable;
            foreach (var col in dgv_flippable.Columns.OfType<DataGridViewCheckBoxColumn>())
            {
                col.TrueValue = true;
                col.FalseValue = false;
                col.IndeterminateValue = null;
            }
        }
        private static bool RunElevated(string fileName)
        {
            //MessageBox.Show("Run: " + fileName);
            ProcessStartInfo processInfo = new()
            {
                UseShellExecute = true,
                Verb = "runas",
                FileName = fileName
            };
            try
            {
                Process.Start(processInfo);
                return true;
            }
            catch (Win32Exception)
            {
                //Do nothing. Probably the user canceled the UAC window
            }
            return false;
        }

        private Flippable[] GetFlippable(string filter)
        {
            List<Flippable> flips = [];
            using (RegistryKey hid = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\HID\", false))
            {
                foreach (string devicekn in hid.GetSubKeyNames())
                {
                    using RegistryKey device = hid.OpenSubKey(devicekn, false);
                    foreach (string devicekn2 in device.GetSubKeyNames())
                    {
                        using RegistryKey device2 = device.OpenSubKey(devicekn2, false);
                        using RegistryKey devparam = device2.OpenSubKey("Device Parameters", true);
                        if (devparam != null)
                            flips.Add(new Flippable([devicekn, devicekn2], device2, devparam, tmr_popup));
                    }
                }
            }
            if (filter != null)
                return flips.Where(f => f.Name.Contains(filter)).ToArray();
            return [.. flips];
        }

        private void dgv_flippable_MouseUp(object sender, MouseEventArgs e)
        {
            dgv_flippable.EndEdit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            flippable = GetFlippable(null);
            dgv_flippable.DataSource = flippable;
        }

        private void btn_flip_Click(object sender, EventArgs e)
        {
            foreach (var f in flippable)
            {
                f.Vertical = true;
                f.Horizontal = true;
            }
            dgv_flippable.DataSource = null;
            dgv_flippable.DataSource = flippable;
        }

        private void btn_normal_Click(object sender, EventArgs e)
        {
            foreach (var f in flippable)
            {
                f.Vertical = false;
                f.Horizontal = false;
            }
            dgv_flippable.DataSource = null;
            dgv_flippable.DataSource = flippable;
        }

        private void tmr_popup_Tick(object sender, EventArgs e)
        {
            tmr_popup.Enabled = false;
            notifyIcon1.ShowBalloonTip(99999999);
        }
    }
}
