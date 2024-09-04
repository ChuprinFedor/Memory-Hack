using ExternalCheat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Memory_Hack
{
    public partial class processes : Form
    {
        public processes()
        {
            InitializeComponent();
            var processes = Process.GetProcesses();
            foreach (var a in processes.OrderBy(x => x.ProcessName))
            {
                listBox1.Items.Add(a.ProcessName);
            }
        }

        private async void select_button_Click(object sender, EventArgs e)
        {
            Form1.id = Process.GetProcessesByName(listBox1.SelectedItem.ToString()).First().Id;
            Form1.process_name = listBox1.SelectedItem.ToString();
            Form1.pbar = false;
            if (Form1.process_status == true)
            {
                Form1.process_status = false;
                await Task.Delay(100);
                Form1.process_status = true;
            }
            else Form1.process_status = true;
            Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}