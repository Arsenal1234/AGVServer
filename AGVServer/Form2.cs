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
    public partial class Form2 : Form
    {
        string ID = Form1.form1.treeView1.SelectedNode.Name;
        List<Command> com = new List<Command>();
        public Form2()
        {
            InitializeComponent();

            for (int i = 0; i < Form1.OneCommandList.Count; i++ )
            {
                if(Form1.OneCommandList[i].CommandNum == ID)
                {
                    com = Form1.OneCommandList[i].command;                 
                    break;
                }
            }

                for (int i = 0; i < Form1.CarIpList.Count; i++)
                {
                    if (Form1.CarIpList[i].Carnum != "")
                    {
                        checkedListBox1.Items.Add(Form1.CarIpList[i].Carnum);
                        for (int j = 0; j < com.Count; j++ )
                        {
                            if(com[j].Carname == Form1.CarIpList[i].Carnum) 
                            {
                                checkedListBox1.SetItemChecked(i, true);        //将已关联的小车勾选
                            }
                        }                         
                    }
                }
            
        }

        /// <summary>
        /// 全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++ )
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        /// <summary>
        /// 反选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    checkedListBox1.SetItemChecked(i, false);
                }
                else
                {
                    checkedListBox1.SetItemChecked(i, true);
                }
            }
        }

        /// <summary>
        /// 关联
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            
            List<Command> Command = new List<Command>();
            for (int j = 0; j < Form1.OneCommandList.Count; j++)
            {
                if (Form1.OneCommandList[j].CommandNum == Form1.form1.treeView1.SelectedNode.Name)
                {
                    List<Command> OldCommand = Form1.OneCommandList[j].command;
                    for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    {
                        if (checkedListBox1.GetItemChecked(i))
                        {
                            if (OldCommand.Count > 0)
                            {
                                for (int s = 0; s < OldCommand.Count; s++)
                                {
                                    if (OldCommand[s].Carname == checkedListBox1.Items[i].ToString() && OldCommand[s].Carname != "")
                                    {
                                        Command command = new Command();
                                        command.Carname = OldCommand[s].Carname;
                                        command.CommandFx = OldCommand[s].CommandFx;
                                        command.CommandRange = OldCommand[s].CommandRange;
                                        command.CommandSpeed = OldCommand[s].CommandSpeed;
                                        command.Angle = OldCommand[s].Angle;
                                        Command.Add(command);
                                        break;
                                    }
                                    if (s == OldCommand.Count - 1)
                                    {
                                        Command command = new Command();
                                        command.Carname = checkedListBox1.Items[i].ToString();
                                        command.CommandFx = "前进";
                                        command.CommandRange = "0";
                                        command.CommandSpeed = "0";
                                        command.Angle = "0";
                                        Command.Add(command);
                                    }
                                }
                            }
                            else
                            {
                                Command command = new Command();
                                command.Carname = checkedListBox1.Items[i].ToString();
                                command.CommandFx = "前进";
                                command.CommandRange = "0";
                                command.CommandSpeed = "0";
                                command.Angle = "0";
                                Command.Add(command);
                            }
                           
                        }


                    }
                    Form1.OneCommandList[j].command = Command;
                    Form1.SaveOneCommand();
                    Form1.form1.treeView1.Nodes.Clear();
                    break;
                }

            }
            OneCommand oneCommand = new OneCommand();
            oneCommand.CommandNum = "";
            Form1.form1.treeview(oneCommand);
            foreach (TreeNode tr in Form1.form1.treeView1.Nodes)
            {
                TreeNode temp = Form1.form1.FindNode(tr, ID);
                if (temp != null)
                {
                    Form1.form1.treeView1.SelectedNode = temp;
                    temp.Expand();
                }
            }
            
            //Form1.form1.treeView1.ExpandAll();
            this.Hide();
        }
    }
}
