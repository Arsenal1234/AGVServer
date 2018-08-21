using AGVServer.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AGVServer
{
    public partial class Nest : Form
    {
        public Nest()
        {
            InitializeComponent();
            label2.Text = Form1.form1.treeView1.SelectedNode.Name;
            InitComBox();
            radioButton5.Checked = true;
        }

        /// <summary>
        /// 添加选项到父指令选择框
        /// </summary>
        void InitComBox()
        {
            for(int i = 0; i < Form1.OneCommandList.Count; i++)
            {
                if (Form1.OneCommandList[i].CommandNum != "" && Form1.OneCommandList[i].CommandNum != Form1.form1.treeView1.SelectedNode.Name)
                {
                    comboBox1.Items.Add(Form1.OneCommandList[i].CommandNum);           //
                }
            }
            
            RemoveBox(Form1.form1.treeView1.SelectedNode.Name);
        }

        /// <summary>
        /// 移除选择框中的父指令
        /// </summary>
        /// <param name="FatherPoint"></param>
        void RemoveBox(string FatherPoint)
        {
            for (int i = 0; i < Form1.OneCommandList.Count; i++)
            {
                if (Form1.OneCommandList[i].CommandNum != "" && Form1.OneCommandList[i].FatherPoint == FatherPoint)
                {
                    comboBox1.Items.Remove(Form1.OneCommandList[i].CommandNum);
                    RemoveBox(Form1.OneCommandList[i].CommandNum);
                }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Enabled = true;
            groupBox3.Enabled = false;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Enabled = false;
            groupBox3.Enabled = true;
        }

        /// <summary>
        /// 逻辑嵌套
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                TreeNode trNode = Form1.form1.treeView1.SelectedNode;
                if (comboBox1.Text != "" || radioButton4.Checked)
                {
                    if (radioButton1.Checked)   //普通嵌套
                    {

                        foreach (OneCommand oneCommand in Form1.OneCommandList)
                        {
                            if (oneCommand.CommandNum == Form1.form1.treeView1.SelectedNode.Name)
                            {
                                oneCommand.FatherPoint = comboBox1.Text;
                                oneCommand.Finish = "";
                                oneCommand.WaitTime = "";
                                oneCommand.WaitCar = "";
                                Form1.SaveOneCommand();                               
                                break;
                            }
                        }

                    }
                    else if (radioButton2.Checked)  //完成度嵌套
                    {
                        foreach (OneCommand oneCommand in Form1.OneCommandList)
                        {
                            if (oneCommand.CommandNum == Form1.form1.treeView1.SelectedNode.Name)
                            {
                                if (Convert.ToInt32(textBox1.Text) >= 0 && Convert.ToInt32(textBox1.Text) <= 100 && comboBox2.Text != "")
                                {
                                    oneCommand.FatherPoint = comboBox1.Text;
                                    oneCommand.Finish = textBox1.Text;
                                    oneCommand.WaitTime = "";
                                    oneCommand.WaitCar = comboBox2.Text;
                                    Form1.SaveOneCommand();
                                    break;
                                }
                            }
                        }
                    }
                    else if (radioButton3.Checked)      //延时嵌套
                    {
                        foreach (OneCommand oneCommand in Form1.OneCommandList)
                        {
                            if (oneCommand.CommandNum == Form1.form1.treeView1.SelectedNode.Name)
                            {
                                if (Convert.ToInt32(textBox2.Text) >= 0)
                                {
                                    oneCommand.FatherPoint = comboBox1.Text;
                                    oneCommand.Finish = "";
                                    oneCommand.WaitCar = "";
                                    oneCommand.WaitTime = textBox2.Text;
                                    Form1.SaveOneCommand();
                                    break;
                                }
                            }
                        }
                    }
                    else if (radioButton4.Checked)       //设为根指令
                    {
                        foreach (OneCommand oneCommand in Form1.OneCommandList)
                        {
                            if (oneCommand.CommandNum == Form1.form1.treeView1.SelectedNode.Name)
                            {
                                oneCommand.FatherPoint = "";
                                oneCommand.Finish = "";
                                oneCommand.WaitTime = "";
                                oneCommand.WaitCar = "";
                                Form1.SaveOneCommand();
                                break;
                            }
                        }
                    }
                    else if(radioButton5.Checked)
                    {
                        foreach (OneCommand oneCommand in Form1.OneCommandList)
                        {
                            if (oneCommand.CommandNum == Form1.form1.treeView1.SelectedNode.Name)
                            {
                                oneCommand.FatherPoint = comboBox1.Text;
                                oneCommand.Finish = "";
                                oneCommand.WaitTime = "";
                                oneCommand.WaitCar = "0";
                                Form1.SaveOneCommand();
                                break;
                            }
                        }
                    }

                    Form1.form1.treeView1.Nodes.Clear();
                    OneCommand Command = new OneCommand();
                    Command.CommandNum = "";
                    Form1.form1.treeview(Command);
                   // Form1.form1.treeView1.SelectedNode = trNode;
                    foreach (TreeNode treenode in Form1.form1.treeView1.Nodes)
                    {
                        TreeNode temp = Form1.form1.FindNode(treenode, label2.Text);
                        if (temp != null)
                        {
                            if (temp.Name == label2.Text)
                            {
                                Form1.form1.treeView1.SelectedNode = temp;
                            }

                        }
                    }
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("请选择父指令！");
                }
            }
            catch { }
        }

        /// <summary>
        /// 父指令变化时，可选完成度的小车也变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            for (int i = 0; i < Form1.OneCommandList.Count; i++)
            {
                if (Form1.OneCommandList[i].CommandNum != "" && Form1.OneCommandList[i].CommandNum == comboBox1.Text)
                {
                    List<Command> CommandList = new List<Command>();
                    CommandList = Form1.OneCommandList[i].command;
                    foreach(Command Command in CommandList)
                    {
                        comboBox2.Items.Add(Command.Carname);
                    }
                    break;
                }
            }
        }
    }
}
