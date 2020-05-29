using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace suflor
{
    public partial class Form1 : Form
    {
        Suflor sfr = null;
        bool isTidy = false;
        Timer clk = new Timer();
        Timer b = new Timer();
        public Form1()
        {
            InitializeComponent();
            list.Columns.Add("Id", 40);
            list.Columns.Add("Text", 500);
            sfr = new Suflor();
            timer_callbacks();
        }
        int index = 0;
        void timer_callbacks()
        {
            b.Tick += (o, c) =>
            {
                buttonStart.Text = (int.Parse(buttonStart.Text) - 1).ToString();
                if (buttonStart.Text == "0")
                {
                    buttonStart.Text = "Durdur";
                    clk.Start();
                    b.Stop();
                }
            };

            clk.Tick += (o, cl) =>
            {

                IntPtr al = GetForegroundWindow();
                if (al != ptr) return;
                int sleep_time = sfr.time * 10;
                clk.Stop();
                char c = sfr.SendText[index];
                SendKeys.SendWait(convert2Keys(c));
                if (c == ' ')
                    sleep_time += (sfr.timeSpace * 10);
                if (c == '.')
                    sleep_time += (sfr.timeDot * 10);
                if (c == '\n')
                    sleep_time += (sfr.timeNew * 10);
                clk.Interval = sleep_time;
                if (index < sfr.SendText.Length - 1)
                {
                    index++;
                    clk.Start();
                }
                else
                {
                    buttonStart.Text = "Başlat";
                    index = 0;
                }
            };
        }
        IntPtr ptr = IntPtr.Zero;
        private void SendStart_Click(object sender, EventArgs e)
        {
            if (buttonStart.Text == "Durdur")
            {
                buttonStart.Text = "Başlat";
                clk.Stop();
                return;
            }

            if (sfr.SendText.Length <= 0) return;
            // buttonStart.Enabled = false;

            Process[] processes = Process.GetProcessesByName(sfr.processName);
            foreach (Process proc in processes)
            {
                // the chrome process must have a window 
                if (proc.MainWindowHandle == IntPtr.Zero)
                {
                    continue;
                }
                SetForegroundWindow(proc.MainWindowHandle);
                ptr = proc.MainWindowHandle;
                break;
            }
            if (ptr == IntPtr.Zero) return;
            /*  Process pro = Process.GetCurrentProcess();
              if (ptr != pro.MainWindowHandle)
                  MessageBox.Show(pro.ProcessName);*/

            index = 0;
            clk.Interval = sfr.time;
            b.Interval = 1000;
            buttonStart.Text = "2";
            b.Start();
        }

        string convert2Keys(char ch)
        {
            /*
             * For SHIFT use +
             * For CTRL  use ^
             * For ALT   use %
             * */
            string ret = ch.ToString();

            switch (ch)
            {
                case '\n':
                    ret = "{ENTER}";
                    break;
                case '^':
                    ret = "+3";
                    break;
                case '+':
                    ret = "+4";
                    break;
                case '%':
                    ret = "+5";
                    break;
                case '(':
                    ret = "+8";
                    break;
                case ')':
                    ret = "+9";
                    break;
                case '{':
                    ret = "^%7";
                    break;
                case '}':
                    ret = "^%0";
                    break;
                case '[':
                    ret = "^%8";
                    break;
                case ']':
                    ret = "^%9";
                    break;
            }
            return ret;
        }
        private async void SendCharAsync(object o, EventArgs e)
        {
            await Task.Run(() =>
            {
                Suflor get = (Suflor)o;

                foreach (char c in get.SendText)
                {
                    SendKeys.SendWait(convert2Keys(c));
                    Thread.Sleep(get.time * 10);
                    if (c == ' ')
                        Thread.Sleep(get.timeSpace * 10);
                    if (c == '.')
                        Thread.Sleep(get.timeDot * 10);
                    if (c == '\n')
                        Thread.Sleep(get.timeNew * 10);
                }
            });
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();// GetActiveWindow();
        private void Set_changed()
        {
            isTidy = true;
            if (!this.Text.Contains("*"))
                this.Text += " *";
        }

        private void list_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (list.SelectedItems.Count == 0)
                return;
            label.Text = list.SelectedItems[0].Text;
            textSend.Text = sfr.items[list.SelectedItems[0].Text];
            sfr.SendText = textSend.Text;
        }
        #region "File Menu"
        private void çıkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        string file_name = "";
        private void açToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isTidy)
                if (MessageBox.Show("Değişiklik yaptığınız veriler kaydedilmedi!\n Devam etmek istiyor musunuz?", "Uyarı !", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return;

            OpenFileDialog fl = new OpenFileDialog();
            fl.Filter = ".sfr|*.sfr";
            fl.DefaultExt = ".sfr";
            fl.Multiselect = false;
            if (fl.ShowDialog() == DialogResult.OK)
            {
                file_name = fl.FileName;
                sfr.Dispose();
                sfr = new Suflor(file_name);
                list.Items.Clear();
                foreach (KeyValuePair<String, String> o in sfr.items)
                {
                    string[] arr = new string[2];
                    arr[0] = o.Key;
                    arr[1] = o.Value;
                    ListViewItem item = new ListViewItem(arr);
                    list.Items.Add(item);
                }
                waitTime.Value = sfr.time;
                timeDot.Value = sfr.timeDot;
                timeSpace.Value = sfr.timeSpace;
                timeNew.Value = sfr.timeNew;
                processName.Text = sfr.processName;

                if (sfr.Font != null)
                {
                    list.Font = sfr.Font;
                    textSend.Font = sfr.Font;
                    label.Font = sfr.Font;
                }

                this.Text = "Suflör :) " + fl.SafeFileName;
                isTidy = false;
            }
        }

        private void kaydetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (file_name.Length < 8)
            {
                SaveFileDialog fl = new SaveFileDialog();
                fl.FileName = "Yeni";
                fl.Filter = ".sfr|*.sfr";
                fl.DefaultExt = ".sfr";
                if (fl.ShowDialog() == DialogResult.OK)
                {
                    file_name = fl.FileName;
                }
            }
            if (file_name.Length < 8) return;
            sfr.Save(file_name);
            isTidy = false;
            this.Text = "Suflör :) " + Path.GetFileName(file_name);
        }

        private void farklıKaydetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog fl = new SaveFileDialog();
            fl.FileName = Path.GetFileName(file_name);
            fl.Filter = ".sfr|*.sfr";
            fl.DefaultExt = ".sfr";
            if (fl.ShowDialog() == DialogResult.OK)
            {
                file_name = fl.FileName;
                sfr.Save(file_name);
            }
        }

        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isTidy)
            {
                if (MessageBox.Show("Değişiklik yaptığınız veriler kaydedilmedi!\n Verilerinizi Kaydetmek ister misiniz?", "Uyarı !", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    kaydetToolStripMenuItem.PerformClick();
            }
        }
        #region "Time Values"
        private void waitTime_ValueChanged(object sender, EventArgs e)
        {
            Set_changed();
            sfr.time = (int)waitTime.Value;
        }
        private void timeNew_ValueChanged(object sender, EventArgs e)
        {
            Set_changed();
            sfr.timeNew = (int)timeNew.Value;

        }
        private void timeDot_ValueChanged(object sender, EventArgs e)
        {
            Set_changed();
            sfr.timeDot = (int)timeDot.Value;
        }

        private void timeSpace_ValueChanged(object sender, EventArgs e)
        {
            Set_changed();
            sfr.timeSpace = (int)timeSpace.Value;
        }
        #endregion
        #region "Tidy Menu"
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string[] arr = new string[2];
            arr[0] = label.Text;
            arr[1] = textSend.Text;
            if (arr[0].Length == 0)
            {
                MessageBox.Show("Id girin");
                return;
            }
            if (sfr.items.ContainsKey(arr[0]))
            {
                sfr.items[arr[0]] = arr[1];

                list.Items[list.SelectedItems[0].Index].SubItems[1].Text = arr[1];

                Set_changed();
                tidy_menu();
                // MessageBox.Show("Ok");
                return;
            }

            ListViewItem item = new ListViewItem(arr);
            list.Items.Add(item);
            sfr.items.Add(arr[0], arr[1]);
            Set_changed();
            label.Clear();
            textSend.Clear();
            tidy_menu();
        }
        void tidy_menu(string btn = "", bool visible = false)
        {
            btnAdd.Text = btn;
            panel3.Visible = visible;
        }
        private void yeni_Click(object sender, EventArgs e)
        {
            label.Clear();
            textSend.Clear();
            tidy_menu("Ekle", true);
        }

        private void düzeltToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfr.SendText.Length > 0)
                tidy_menu("Düzelt", true);
        }

        #endregion

        private void prosesName_TextChanged(object sender, EventArgs e)
        {
            Set_changed();
            sfr.processName = processName.Text;
        }

        private void list_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            düzeltToolStripMenuItem.PerformClick();
        }

        private void list_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                tidy_menu();
            if (e.KeyCode == Keys.Enter)
                düzeltToolStripMenuItem.PerformClick();
        }

        ListViewItem heldDownItem;

        private void list_MouseDown(object sender, MouseEventArgs e)
        {
            list.AutoArrange = false;
            heldDownItem = list.GetItemAt(e.X, e.Y);

        }
        //MouseMove event handler for your listView1
        private void list_MouseMove(object sender, MouseEventArgs e)
        {
              if (heldDownItem != null)
            {
                if (Math.Abs(e.Y - heldDownItem.Position.Y) < 15) return;

                list.Items.Remove(heldDownItem);

                ListViewItem i = list.GetItemAt(e.X, e.Y);
                if (i != null)
                {
                    list.Items.Insert(i.Index, heldDownItem);
                }
                else
                {
                    list.Items.Add(heldDownItem);
                }
            }
        }
        //MouseUp event handler for your listView1
        private void list_MouseUp(object sender, MouseEventArgs e)
        {
            /* if (heldDownItem != null)
             {
                // list.Items.Remove(heldDownItem);
                 ListViewItem i = list.GetItemAt(e.X, e.Y);
                 if (i != null)
                     list.Items.Insert(i.Index, heldDownItem);
                 else
                     list.Items.Add(heldDownItem);
             }*/

            heldDownItem = null;
            list.AutoArrange = true;
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(sfr.Font!=null)
            fontDialog1.Font = sfr.Font;
           if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                list.Font = fontDialog1.Font;
                textSend.Font = fontDialog1.Font;
                label.Font = fontDialog1.Font;
                sfr.Font = fontDialog1.Font;
                Set_changed();
            }
        }
    }
}
