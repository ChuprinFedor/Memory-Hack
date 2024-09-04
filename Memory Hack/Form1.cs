using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using ExternalCheat;
using ExternalCheat.Memory;
using ExternalCheat.Memory.Pattern;

namespace Memory_Hack
{
    public partial class Form1 : Form
    {
        public static int id = 0;
        public static bool process_status = false;
        private static int options = 0;
        public static string process_name;
        private static int lbsi;
        public static bool pbar;
        private static int cbsi;
        public Form1()
        {
            InitializeComponent();
            Status();
            comboBox1.SelectedItem = comboBox1.Items[0];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            processes prcs = new processes();
            prcs.Show();
        }

        async void Process_Manager()
        {
            await Task.Run(() =>
            {
                IntPtr processHandle = WinAPI.OpenProcess(WinAPI.ProcessAccessFlags.All, false, id);
                if (processHandle == IntPtr.Zero)
                {
                    MessageBox.Show("Открытие процесса завершилось с ошибкой!");
                    id = 0;
                    process_status = false;
                    return;
                }
                var memory = new MemoryManager(processHandle);
                ToLabel("Текущий процесс: " + process_name);
                List<IntPtr> wholeProcess = new List<IntPtr>();
                while (process_status == true)
                {
                    if (!Process.GetProcessesByName(process_name).Any()) process_status = false;
                    if (options == 1)
                    {
                        pbar = true;
                        ListBoxClear(listBox1);
                        wholeProcess = new List<IntPtr>();
                        ComboBoxSelected(comboBox1);
                        var addrs = memory.PatternScan(new MemoryPattern(textBox1.Text, cbsi));
                        barSet(addrs.Count);
                        if (addrs.Count != 0)
                        {
                            foreach (var addr in addrs)
                            {
                                if (pbar == false) break;
                                try
                                {
                                    foreach (var i in addr)
                                    {
                                        if (pbar == false) break;
                                        if (cbsi == 0) ToListBox("Адрес: 0x" + Convert.ToString(i.ToInt64(), 16) + " Значение: " + memory.ReadInt32(i).ToString());
                                        else if (cbsi == 1) ToListBox("Адрес: 0x" + Convert.ToString(i.ToInt64(), 16) + " Значение: " + memory.ReadFloat(i).ToString());
                                        else if (cbsi == 2) ToListBox("Адрес: 0x" + Convert.ToString(i.ToInt64(), 16) + " Значение: " + memory.ReadInt64(i).ToString());
                                        else if (cbsi == 3) ToListBox("Адрес: 0x" + Convert.ToString(i.ToInt64(), 16) + " Значение: " + memory.ReadString(i, textBox1.Text.Length));
                                        else if (cbsi == 4) ToListBox("Адрес: 0x" + Convert.ToString(i.ToInt64(), 16) + " Значение: " + memory.ReadByte(i).ToString());
                                        wholeProcess.Add(i);
                                    }
                                    Progress(progressBar1);
                                }
                                catch { }
                            }
                        }
                        else MessageBox.Show("Значения не найдены!");
                        if (pbar == false)
                        {
                            ListBoxClear(listBox1);
                            barSet(1);
                        }
                        else pbar = false;
                        options = 0;
                    }
                    else if (options == 2)
                    {
                        List<IntPtr> addrs = new List<IntPtr>();
                        for (int i = 0; i < wholeProcess.Count; i++)
                        {
                            if (cbsi == 0)
                            {
                                if (memory.ReadInt32(wholeProcess[i]) == Convert.ToInt32(textBox1.Text)) addrs.Add(wholeProcess[i]);
                            }
                            else if (cbsi == 1)
                            {
                                if (memory.ReadFloat(wholeProcess[i]) == Convert.ToSingle(textBox1.Text)) addrs.Add(wholeProcess[i]);
                            }
                            else if (cbsi == 2)
                            {
                                if (memory.ReadInt64(wholeProcess[i]) == Convert.ToInt64(textBox1.Text)) addrs.Add(wholeProcess[i]);
                            }
                            else if (cbsi == 3)
                            {
                                if (memory.ReadString(wholeProcess[i], textBox1.Text.Length) == textBox1.Text) addrs.Add(wholeProcess[i]);
                            }
                            else if (cbsi == 4)
                            {
                                if (memory.ReadByte(wholeProcess[i]) == (byte)Convert.ToInt32(textBox1.Text)) addrs.Add(wholeProcess[i]);
                            }
                        }
                        ListBoxClear(listBox1);
                        foreach (var addr in addrs)
                        {
                            if (cbsi == 0) ToListBox("Адрес: 0x" + Convert.ToString(addr.ToInt64(), 16) + " Значение: " + memory.ReadInt32(addr).ToString());
                            else if (cbsi == 1) ToListBox("Адрес: 0x" + Convert.ToString(addr.ToInt64(), 16) + " Значение: " + memory.ReadFloat(addr).ToString());
                            else if (cbsi == 2) ToListBox("Адрес: 0x" + Convert.ToString(addr.ToInt64(), 16) + " Значение: " + memory.ReadInt64(addr).ToString());
                            else if (cbsi == 3) ToListBox("Адрес: 0x" + Convert.ToString(addr.ToInt64(), 16) + " Значение: " + memory.ReadString(addr, textBox1.Text.Length));
                            else if (cbsi == 4) ToListBox("Адрес: 0x" + Convert.ToString(addr.ToInt64(), 16) + " Значение: " + memory.ReadByte(addr).ToString());
                        }
                        wholeProcess = addrs;
                        options = 0;
                    }
                    else if (options == 3)
                    {
                        ListBoxSelected(listBox1);
                        if (cbsi == 0)
                        {
                            memory.WriteInt32(wholeProcess[lbsi], Convert.ToInt32(textBox1.Text));
                            ReplaceListBox("Адрес: 0x" + Convert.ToString(wholeProcess[lbsi].ToInt64(), 16) + " Значение: " + memory.ReadInt32(wholeProcess[lbsi]).ToString());
                        }
                        else if (cbsi == 1)
                        {
                            memory.WriteFloat(wholeProcess[lbsi], Convert.ToSingle(textBox1.Text));
                            ReplaceListBox("Адрес: 0x" + Convert.ToString(wholeProcess[lbsi].ToInt64(), 16) + " Значение: " + memory.ReadFloat(wholeProcess[lbsi]).ToString());
                        }
                        else if (cbsi == 2)
                        {
                            memory.WriteInt64(wholeProcess[lbsi], Convert.ToInt64(textBox1.Text));
                            ReplaceListBox("Адрес: 0x" + Convert.ToString(wholeProcess[lbsi].ToInt64(), 16) + " Значение: " + memory.ReadInt64(wholeProcess[lbsi]).ToString());
                        }
                        else if (cbsi == 3)
                        {
                            memory.WriteString(wholeProcess[lbsi], textBox1.Text);
                            ReplaceListBox("Адрес: 0x" + Convert.ToString(wholeProcess[lbsi].ToInt64(), 16) + " Значение: " + memory.ReadString(wholeProcess[lbsi], textBox1.Text.Length));
                        }
                        else if (cbsi == 4)
                        {
                            memory.WriteByte(wholeProcess[lbsi], (byte)Convert.ToInt32(textBox1.Text));
                            ReplaceListBox("Адрес: 0x" + Convert.ToString(wholeProcess[lbsi].ToInt64(), 16) + " Значение: " + memory.ReadByte(wholeProcess[lbsi]).ToString());
                        }
                        options = 0;
                    }
                }
                ListBoxClear(listBox1);
                ToLabel("Процесс не выбран");
                WinAPI.CloseHandle(processHandle);
            });
        }

        async void Status()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (process_status == true)
                    {
                        Process_Manager();
                        while (process_status == true) { }
                    }
                }
            });
        }

        private void ToLabel(string text)
        {
            if (InvokeRequired)
                Invoke((Action<string>)ToLabel, text);
            else
                label1.Text = text;
        }

        private void ToListBox(string text)
        {
            if (InvokeRequired)
                Invoke((Action<string>)ToListBox, text);
            else
                listBox1.Items.Add(text);
        }

        private void ListBoxClear(ListBox listBox)
        {
            if (InvokeRequired)
                Invoke((Action<ListBox>)ListBoxClear, listBox);
            else
                listBox1.Items.Clear();
        }

        private void ReplaceListBox(string text)
        {
            if (InvokeRequired)
                Invoke((Action<string>)ReplaceListBox, text);
            else
                listBox1.Items[listBox1.SelectedIndex] = text;
        }

        private void ListBoxSelected(ListBox listBox)
        {
            if (InvokeRequired)
                Invoke((Action<ListBox>)ListBoxSelected, listBox);
            else
                lbsi = listBox1.SelectedIndex;
        }

        public void barSet(int i)
        {
            if (InvokeRequired)
                Invoke((Action<int>)barSet, i);
            else
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = i;
                progressBar1.Value = 0;
                progressBar1.Step = 1;
            }
        }

        private void Progress(ProgressBar progress)
        {
            if (InvokeRequired)
                Invoke((Action<ProgressBar>)Progress, progress);
            else
                progressBar1.Value += 1;
        }

        private void ComboBoxSelected(ComboBox comboBox)
        {
            if (InvokeRequired)
                Invoke((Action<ComboBox>)ComboBoxSelected, comboBox);
            else
                cbsi = comboBox1.SelectedIndex;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (process_status == true && textBox1.Text != null && textBox1.Text != "") options = 1;
            else MessageBox.Show("Процесс должен быть выбран, поле должно быть не пустым, и содержать только числа!");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (process_status == true && listBox1.Items.Count != 0 && textBox1.Text != null && textBox1.Text != "") options = 2;
            else MessageBox.Show("Процесс должен быть выбран, список адресов не должен быть пуст, поле должно быть не пустым, и содержать только числа!");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (process_status == true && listBox1.Items.Count != 0 && listBox1.SelectedIndex != -1 && textBox1.Text != null && textBox1.Text != "") options = 3;
            else MessageBox.Show("Процесс должен быть выбран, список адресов не должен быть пуст, адрес из списка должен быть выбран, поле должно быть не пустым, и содержать только числа!");
        }
    }
}