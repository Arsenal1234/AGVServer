using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AGVServer.Model;

namespace AGVServer
{
    public partial class Drive : Form
    {
        public bool Sendtype = true;
        public Drive()
        {
            InitializeComponent();
            foreach(CarIp CarIp in Form1.CarIpList)
            {
                if(CarIp.Carnum != "" && CarIp.Carip != "")
                {
                    comboBox1.Items.Add(CarIp.Carnum);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] buffer = new byte[8];
            buffer[3] = 7;
            try
            {
                if (comboBox1.Text != "")
                {
                    for (int i = 0; i < Form1.form1.dataGridView2.Rows.Count; i++)
                    {
                        if (comboBox1.Text == Form1.form1.dataGridView2.Rows[i].Cells[0].Value.ToString())
                        {
                            foreach (Label label in Form1.LabelList)
                            {
                                if (label.Text != "" && label.Text.Contains(Form1.form1.dataGridView2.Rows[i].Cells[1].Value.ToString()))
                                {
                                    Form1.dicSocket[label.Text].Send(buffer);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("请选中要发送的小车");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Sendtype)
            {
                byte[] buffer = new byte[8];
                buffer[3] = 8;
                try
                {
                    if (comboBox1.Text != "")
                    {
                        for (int i = 0; i < Form1.form1.dataGridView2.Rows.Count; i++)
                        {
                            if (comboBox1.Text == Form1.form1.dataGridView2.Rows[i].Cells[0].Value.ToString())
                            {
                                foreach (Label label in Form1.LabelList)
                                {
                                    if (label.Text != "" && label.Text.Contains(Form1.form1.dataGridView2.Rows[i].Cells[1].Value.ToString()))
                                    {
                                        Form1.dicSocket[label.Text].Send(buffer);
                                        button2.Text = "驱动开启";
                                        Sendtype = false;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("请选中要发送的小车");
                }
            }
            else
            {
                byte[] buffer = new byte[8];
                buffer[3] = 9;
                try
                {
                    if (comboBox1.Text != "")
                    {
                        for (int i = 0; i < Form1.form1.dataGridView2.Rows.Count; i++)
                        {
                            if (comboBox1.Text == Form1.form1.dataGridView2.Rows[i].Cells[0].Value.ToString())
                            {
                                foreach (Label label in Form1.LabelList)
                                {
                                    if (label.Text != "" && label.Text.Contains(Form1.form1.dataGridView2.Rows[i].Cells[1].Value.ToString()))
                                    {
                                        Form1.dicSocket[label.Text].Send(buffer);
                                        button2.Text = "驱动关闭";
                                        Sendtype = true;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("请选中要发送的小车");
                }
            }
        }
    }
}
