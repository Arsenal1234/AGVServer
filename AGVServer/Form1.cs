
using AGVClient.Database;
using AGVServer.Model;
using Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Drawing.Drawing2D;

namespace AGVServer
{
    public partial class Form1 : Form
    {
        
        public static CarIp CarIP;
        /// <summary>
        /// 心跳包掉线时间
        /// </summary>
        public static int OverTime = 5;                        
        public IPAddress ip;
        public static Form1 form1;
        /// <summary>
        /// 命令集合
        /// </summary>
        //public static List<LoadCommand> loadCommandList;

        /// <summary>
        /// 单条指令集合
        /// </summary>
        public static List<OneCommand> OneCommandList;
        /// <summary>
        /// 判断是否可以发送命令
        /// </summary>
        public bool BoolSend = true;
        /// <summary>
        /// 小车客户端集合
        /// </summary>
        public static Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>();        
        
        /// <summary>
        /// 安卓客户端集合
        /// </summary>
        public static Dictionary<string, Socket> AndroiddicSocket = new Dictionary<string, Socket>();         
        //      public static Dictionary<string, ClientController> ControllerList = new Dictionary<string, ClientController>();
        
        /// <summary>
        /// 心跳集合
        /// </summary>
        public static List<SocketHeart> HeartList = new List<SocketHeart>();

        /// <summary>
        /// 移除客户端
        /// </summary>
        public List<string> RemoveSocketName = new List<string>(); 

        /// <summary>
        /// 小车命令集合
        /// </summary>
        public static Dictionary<string, List<SocketCommand>> CommandList = new Dictionary<string, List<SocketCommand>>(); 
        
        /// <summary>
        /// 小车Ip集合
        /// </summary>
        public static List<CarIp> CarIpList = new List<CarIp>();

        /// <summary>
        /// 判断是否要更新XML文件
        /// </summary>
        bool Valuechange = true;

        /// <summary>
        /// 存放的IP集合
        /// </summary>
        public static List<Label> LabelList = new List<Label>();
        
        /// <summary>
        /// 延时执行命令的集合
        /// </summary>
       public static List<WaitSocket> WaitList = new List<WaitSocket>();

        /// <summary>
        /// 完成度集合
        /// </summary>
       public static Dictionary<string, string> PlanList = new Dictionary<string, string>();

        /// <summary>
        /// 小车端反馈信息的集合
        /// </summary>
       public static Dictionary<string, ReceiveIn> ReceiveList = new Dictionary<string, ReceiveIn>();

        /// <summary>
        /// 命令接收的集合    这个主要是用来接收另一台电脑发送的命令控制小车
        /// </summary>
       public static List<ReceiveCommand> ReceiveCommandList = new List<ReceiveCommand>();

       public List<Position> PositionList = new List<Position>();

       public List<DirectedEdge> DirectedEdgeList = new List<DirectedEdge>();
        /// <summary>
        /// Bool键盘
        /// </summary>
       public bool KeyBoard = false;

       private SQLDeal sql;

       private EdgeWeightedDigraph graph;

       private Graphics g;

        public Form1()
        {
            form1 = this;
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            Addlabel();
            SocketStart(null, null);
            comboBox1.Items.Add("启动");
            comboBox1.Items.Add("停止");
            comboBox1.Items.Add("暂停");
            
            if (LoadOneCommandXml())       //判断是否读取命令文件
            {
                OneCommand oneCommand = new OneCommand();
                oneCommand.CommandNum = "";
                treeview(oneCommand);
                treeView1.SelectedNode = treeView1.Nodes[0];   //程序默认选定第一个节点
            }
            else
            {
                InitOneCommandXml();
                button1_Click(null, null);
            }

            if(LoadReceiveCommandXml())
            {
                int j = 0;
                for (int i = 0; i < ReceiveCommandList.Count; i++)
                {
                    if (ReceiveCommandList[i].ReceiveNum != "")
                    {
                        DataGridViewRow row = new DataGridViewRow();

                        DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell();
                        textboxcell.Value = "";
                        row.Cells.Add(textboxcell); 
                        dataGridView3.Rows.Add(row);
                        dataGridView3[0, j].Value = ReceiveCommandList[i].ReceiveNum;

                        dataGridView3[1, j].Value = ReceiveCommandList[i].ReceiveText;
                        dataGridView3[2, j].Value = ReceiveCommandList[i].ReceiveType;
                        dataGridView3[3, j].Value = ReceiveCommandList[i].CommandNum;
                        j++;
                    }
                }
            }
            else
            {
                InitReceiveCommandXml();
            }

            if (LoadCarIpXml())       //判断是否读取命令文件
            {
                int j = 0;
                for (int i = 0; i < CarIpList.Count; i++)
                {
                    if (CarIpList[i].Carnum != "")
                    {
                        DataGridViewRow row = new DataGridViewRow();

                        DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell();
                        textboxcell.Value = "";
                        row.Cells.Add(textboxcell);
                        DataGridViewRow row1 = new DataGridViewRow();

                        DataGridViewTextBoxCell textboxcell1 = new DataGridViewTextBoxCell();
                        textboxcell1.Value = "";
                        row1.Cells.Add(textboxcell1);
                        dataGridView2.Rows.Add(row);
                        dataGridView1.Rows.Add(row1);
                        dataGridView2[0, j].Value = CarIpList[i].Carnum;
                        
                        dataGridView2[1, j].Value = CarIpList[i].Carip;
                        dataGridView1[0, j].Value = CarIpList[i].Carnum;
                        j++;
                    }
                }
            }
            else
            {
                InitCarIpXml();
            }
            Valuechange = false;

            //初始化数据库
            sql = new SQLDeal();
            try
            {
                sql.TestConnection();
            }
            catch (Exception ex)
            {
                label41.Text = "数据库连接失败";
            }

            //初始化地图
            graph = null;

            //初始化画图块
            g = pictureBoxMap.CreateGraphics();

           

        }


        #region xml命令文件

        /// <summary>
        /// 添加到LabelList集合中,这些label是为了保存Socket.RemoteEndPoint.ToString()
        /// </summary>
        void Addlabel()
        {
            LabelList.Add(label4);
            LabelList.Add(label5);
            LabelList.Add(label6);
            LabelList.Add(label7);
            LabelList.Add(label8);
            LabelList.Add(label9);
            LabelList.Add(label10);
            LabelList.Add(label11);
            LabelList.Add(label14);
            LabelList.Add(label15);
        }

        /// <summary>
        /// 更新显示整个节点
        /// </summary>
        /// <param name="oneCommand"></param>
        public  void treeview(OneCommand oneCommand)
        {
            for (int i = 0; i < OneCommandList.Count; i++)
            {
                if (OneCommandList[i].CommandNum != "" && OneCommandList[i].FatherPoint == oneCommand.CommandNum)
                {
                    if (oneCommand.CommandNum == "")
                    {
                        TreeNode node = new TreeNode();
                        node.Text = OneCommandList[i].CommandNum + " - " + OneCommandList[i].CommandName;           
                        node.Name = OneCommandList[i].CommandNum;
                        treeView1.Nodes.Add(node);
                        comboBox2.Items.Add(node.Name);
                        for (int j = 0; j < OneCommandList[i].command.Count; j++)
                        {
                            Command command = new Command();
                            command = OneCommandList[i].command[j];
                            TreeNode sonnode = new TreeNode();
                            sonnode.Text = command.Carname + " " + command.CommandFx + " " + command.CommandRange + "mm  速度" + command.CommandSpeed + "mm/s 角度" + command.Angle;
                            sonnode.EndEdit(true);
                            sonnode.ImageIndex = 1;
                            node.Nodes.Add(sonnode);
                        }
                        treeview(OneCommandList[i]);
                    }
                    else
                    {
                        TreeNode node = new TreeNode();
                        node.Text = OneCommandList[i].CommandNum + " - " + OneCommandList[i].CommandName;
                        node.Name = OneCommandList[i].CommandNum;                    
                        foreach(TreeNode treenode in treeView1.Nodes)
                        {
                            TreeNode temp = FindNode(treenode, oneCommand.CommandNum);
                            if (temp != null)
                            {
                                temp.Nodes.Add(node);
                                comboBox2.Items.Add(node.Name);
                                for (int j = 0; j < OneCommandList[i].command.Count; j++)
                                {
                                    Command command = new Command();
                                    command = OneCommandList[i].command[j];
                                    TreeNode sonnode = new TreeNode();
                                    sonnode.Text = command.Carname + " " + command.CommandFx + " " + command.CommandRange + "mm  速度" + command.CommandSpeed + "mm/s 角度" + command.Angle;
                                    sonnode.ImageIndex = 1;
                                    node.Nodes.Add(sonnode);
                                }
                                treeview(OneCommandList[i]);


                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 递归查询,找到返回该节点 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <returns>查找到的节点</returns>
        public TreeNode FindNode(TreeNode node, string name) 
        { 
            //接受返回的节点 
            TreeNode ret = null;
            if (string.Equals(node.Name, name))
            {
                return node;
            } 
            //循环查找 
            foreach (TreeNode temp in node.Nodes) 
            { 
                //是否有子节点 
                if (temp.Nodes.Count != 0) 

                { 
                    //如果找到 

                    if ((ret = FindNode(temp, name)) != null)
                    {
                        return ret;
                    }

                } 
                //如果找到 
                if (string.Equals(temp.Text,name)) 
                { 
                    return temp; 
                } 
            } 
            return ret; 
        }



        /// <summary>
        /// 初始化小车命令文件
        /// </summary>
        public void InitOneCommandXml()
        {
            OneCommand oneCommand;                   

            OneCommandList = new List<OneCommand>();
            for (int i = 1; i <= 100; i++)
            {
                oneCommand = new OneCommand();
                oneCommand.CommandName = "";
                oneCommand.CommandNum = "";
                oneCommand.command = new List<Command>();
                oneCommand.FatherPoint = "";
                oneCommand.BoolFather = true;
                oneCommand.Finish = "";
                oneCommand.WaitTime = "";
                oneCommand.WaitCar = "";
                OneCommandList.Add(oneCommand);
            }
            try
            {
                XmlSerializer serializer = new XmlSerializer(OneCommandList.GetType());
                TextWriter writer = new StreamWriter("OneCommandConfig.xml");
                serializer.Serialize(writer, OneCommandList);
                writer.Close();
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 读取指令文件
        /// </summary>
        public static bool LoadOneCommandXml()
        {
            try
            {
                XmlSerializer serizer = new XmlSerializer(new List<OneCommand>().GetType());
                FileStream stream = new FileStream(Application.StartupPath + "\\OneCommandConfig.xml", FileMode.Open);
                OneCommandList = (List<OneCommand>)serizer.Deserialize(stream);
                stream.Close();
                if (OneCommandList != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 初始化接收文件
        /// </summary>
        public void InitReceiveCommandXml()
        {
            ReceiveCommand receiveCommand;                   //四辆车的50个命令初始化

            ReceiveCommandList = new List<ReceiveCommand>();
            for (int i = 1; i <= 50; i++)
            {
                receiveCommand = new ReceiveCommand();
                receiveCommand.ReceiveNum = "";
                receiveCommand.ReceiveText = "";
                receiveCommand.ReceiveType = "";
                receiveCommand.CommandNum = "";
                ReceiveCommandList.Add(receiveCommand);
            }
            try
            {
                XmlSerializer serializer = new XmlSerializer(ReceiveCommandList.GetType());
                TextWriter writer = new StreamWriter("ReceiveCommandConfig.xml");
                serializer.Serialize(writer, ReceiveCommandList);
                writer.Close();
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 读取指令文件
        /// </summary>
        public static bool LoadReceiveCommandXml()
        {
            try
            {
                XmlSerializer serizer = new XmlSerializer(new List<ReceiveCommand>().GetType());
                FileStream stream = new FileStream(Application.StartupPath + "\\ReceiveCommandConfig.xml", FileMode.Open);
                ReceiveCommandList = (List<ReceiveCommand>)serizer.Deserialize(stream);
                stream.Close();
                if (ReceiveCommandList != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// 初始化小车Ip
        /// </summary>
        public void InitCarIpXml()
        {
            CarIp carIp;                   //四辆车的24个命令初始化

            CarIpList = new List<CarIp>();
            for (int i = 1; i <= 10; i++)
            {

                carIp = new CarIp();
                carIp.Carnum = "";
                carIp.Carip = "";
                carIp.CommandFx = "";
                carIp.CommandRange = "";
                carIp.CommandSpeed = "";
                carIp.Angle = "";
                CarIpList.Add(carIp);
            }
            try
            {
                XmlSerializer serializer = new XmlSerializer(CarIpList.GetType());
                TextWriter writer = new StreamWriter("CarIpConfig.xml");
                serializer.Serialize(writer, CarIpList);
                writer.Close();
            }
            catch (Exception)
            {
            }
        }


        /// <summary>
        /// 读取小车Ip
        /// </summary>
        /// <returns></returns>
        public static bool LoadCarIpXml()
        {                                               
            try
            {
                XmlSerializer serizer = new XmlSerializer(new List<CarIp>().GetType());
                FileStream stream = new FileStream(Application.StartupPath + "\\CarIpConfig.xml", FileMode.Open);
                CarIpList = (List<CarIp>)serizer.Deserialize(stream);
                stream.Close();
                if (CarIpList != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 保存单条指令
        /// </summary>
        public static void SaveOneCommand()
        {
            try
            {                                       //每个车的命令保存
                XmlSerializer serializer = new XmlSerializer(OneCommandList.GetType());
                TextWriter writer = new StreamWriter("OneCommandConfig.xml");
                serializer.Serialize(writer, OneCommandList);
                writer.Close();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 保存接收指令
        /// </summary>
        public static void SaveReceiveCommand()
        {
            try
            {                                       
                XmlSerializer serializer = new XmlSerializer(ReceiveCommandList.GetType());
                TextWriter writer = new StreamWriter("ReceiveCommandConfig.xml");
                serializer.Serialize(writer, ReceiveCommandList);
                writer.Close();
            }
            catch (Exception)
            {
            }
        }


        /// <summary>
        /// 保存某个车的Ip
        /// </summary>
        public static void SaveCarIp()
        {
            try
            {                                       //每个车的命令保存
                XmlSerializer serializer = new XmlSerializer(CarIpList.GetType());
                TextWriter writer = new StreamWriter("CarIpConfig.xml");
                serializer.Serialize(writer, CarIpList);
                writer.Close();
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region socket通讯
        /// <summary>
        /// TCP通讯，监听安卓与小车端
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SocketStart(object sender, EventArgs e)
        {
            try
            {
                System.Net.IPAddress addr;
                // 获得本机局域网IP地址  
                addr = IPAddress.Parse(textBox2.Text);
                // addr = IPAddress.Parse("192.168.0.119"); 
                //    addr = new System.Net.IPAddress(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].Address);
                ip = addr;
                //当点击开始监听的时候 在服务器端创建一个负责监IP地址跟端口号的Socket
                Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket AndroidWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint AndroidPoint = new IPEndPoint(ip, Convert.ToInt32("50001"));
                AndroidWatch.Bind(AndroidPoint);
                AndroidWatch.Listen(10);
                Thread AndroidTh = new Thread(AndroidListen);
                AndroidTh.IsBackground = true;
                AndroidTh.Start(AndroidWatch);

                //创建端口号对象
                IPEndPoint point = new IPEndPoint(ip, 50000);
                //监听
                socketWatch.Bind(point);
                socketWatch.Listen(10);
                Thread th = new Thread(Listen);
                th.IsBackground = true;
                th.Start(socketWatch);
                label1.Visible = true;
                label12.Visible = false;
            }
            catch
            {
                label12.Visible = true;
                label1.Visible = false;
            }
        }
        Socket socketSend;
        //将远程连接的客户端的IP地址和Socket存入集合中
        delegate void ShowNewController(string SocketName);         //委托创建小车控制器界面
        private void CreateControllerForm(string SocketName)
        {

            for(int i = 0; i < dataGridView2.Rows.Count -1; i++)
            {
                try
                {
                    if (dataGridView2.Rows[i].Cells[1].Value.ToString() != "" && SocketName.Contains(dataGridView2.Rows[i].Cells[1].Value.ToString()))
                    {
                        Valuechange = true;
                        dataGridView2.Rows[i].Cells[2].Value = "在线";
                        break;
                    }
                }
                catch { }
            }
            Valuechange = false;
        }


        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                //在客户端创建一个负责跟服务端通信使用的Socket
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //获得要连接的服务器的IP地址

                IPAddress ip = IPAddress.Parse(textBox4.Text);
                //获得要连接的服务器的端口号
                IPEndPoint point = new IPEndPoint(ip , Convert.ToInt32(textBox5.Text));

                //客户端负责通信的Socket去连接服务端的IP地址跟端口号
                socket.Bind(point);
                socket.Listen(10);
                Thread th = new Thread(ListenWatch);
                th.IsBackground = true;
                th.Start(socket);
                label29.Text = "服务已开启";
            }

            catch 
            {

                label29.Text = "服务未开启";
            }
        }

        void ListenWatch(object o)                   //小车的监听
        {
            Socket socketWatch = o as Socket;
            
            while (true)
            {
                try
                {
                    //负责跟客户端通信的Socket
                    socketSend = socketWatch.Accept();                                     
                    richTextBox2.Text += DateTime.Now.ToShortTimeString() + "：" + socketSend.RemoteEndPoint.ToString() + "上线\r";
                    //将远程连接的客户端的IP地址和端口号存储下拉框中         
                    Thread th = new Thread(Rec);
                    th.IsBackground = true;
                    th.Start(socketSend);
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// 从远程电脑接收到的命令
        /// </summary>
        /// <param name="o"></param>
        void Rec(object o)
        {
            Socket socket = o as Socket;
            string RecStr = "";
            while (true)
            {
                try
                {
                    //客户端接收服务端发来的数据
                    byte[] buffer = new byte[10];

                    byte[] newbuffer = new byte[100];
                    int r = socket.Receive(buffer);

                    if(r == 0)
                    {
                        break;
                    }
                    RecStr = (buffer[1] - 48).ToString();
                    richTextBox2.Text += DateTime.Now.ToShortTimeString() + "：  " + RecStr + "\r\n";
                    for(int i = 0; i < ReceiveCommandList.Count; i++)
                    {
                        if(ReceiveCommandList[i].ReceiveText == RecStr)
                        {
                            if(ReceiveCommandList[i].ReceiveType == "启动")
                            {
                                if (WaitOf())
                                {
                                    foreach (OneCommand oneCommand in OneCommandList)
                                    {
                                        if (oneCommand.CommandNum == ReceiveCommandList[i].CommandNum)
                                        {
                                            SendCommand(oneCommand);
                                            Wait(ReceiveCommandList[i].CommandNum);
                                            break;
                                        }
                                    }

                                }
                            }
                            break;
                        }
                    }
                    
                }
                catch {
                    label29.Text = "连接异常";
                }
            }
        }

        /// <summary>
        /// 为LabelList中的Label赋值
        /// </summary>
        /// <param name="SocketName"></param>
        private void HeartLabel(string SocketName)
        {

            for (int i = 0; i < dataGridView2.Rows.Count -1; i++)
            {
                try
                {
                    if (dataGridView2.Rows[i].Cells[1].Value.ToString() != "" && SocketName.Contains(dataGridView2.Rows[i].Cells[1].Value.ToString()))
                    {
                        LabelList[i].Text = SocketName;
                        break;
                    }
                }
                catch { }
            }
        }


        void AndroidListen(object o)                                //安卓的监听 
        {
            Socket socketWatch = o as Socket;
            while (true)
            {
                try
                {
                    //负责跟客户端通信的Socket
                    socketSend = socketWatch.Accept();
                    //将远程连接的客户端的IP地址和Socket存入集合中
                    AndroiddicSocket.Add(socketSend.RemoteEndPoint.ToString(), socketSend);
                    socketSend.Send(Encoding.Default.GetBytes("ConnectOk"));
                    //将远程连接的客户端的IP地址和端口号存储下拉框中
                    Thread th = new Thread(AndroidRecive);
                    th.IsBackground = true;
                    th.Start(socketSend);
                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// 安卓命令接受处理
        /// </summary>
        /// <param name="o"></param>
        void AndroidRecive(object o)      
        {
            //   buffer.Skip(DataReceiveCount).Take(4)
            Socket socketSend = o as Socket;
            byte[] HeartBuffer = new byte[10 * 10 * 2];
            byte[] CarBuffer = new byte[10 * 10 * 2];
            HeartBuffer[3] = 169;
            while (true)
            {
                try
                {
                    //客户端连接成功后，服务器应该接受客户端发来的消息
                    byte[] buffer = new byte[10 * 10 * 3];
                    //实际接受到的有效字节数
                    int r = socketSend.Receive(buffer);

                    if (r == 0)
                    {
                        break;
                    }

                    if (buffer[0] == 120)               //请求发送socket列表
                    {
                        string userName = "";
                        foreach (var item in dicSocket)
                        {
                            if (userName == "")
                            {
                                userName += item.Key;
                            }
                            else
                            {
                                userName += "|" + item.Key;
                            }

                        }
                        userName += "|end";
                        HeartBuffer = System.Text.Encoding.UTF8.GetBytes(userName);
                        socketSend.Send(HeartBuffer);               //发送socket列表
                    }
                    else
                    {
                        string str = Encoding.UTF8.GetString(buffer, 0, 100);           //解析发送的小车名字
                        str = str.Replace("\0", "");
                        CarBuffer = buffer.Skip(100).Take(200).ToArray();

                        richTextBox2.Text += CarBuffer.ToString() + "  ";

                        if (dicSocket[str].Send(CarBuffer) != 0)  //发送给指定小车
                        {
                            byte[] bufferClient = System.Text.Encoding.UTF8.GetBytes("CommandSendOk" + "end");
                            socketSend.Send(bufferClient);
                        }
                        else
                        {
                            byte[] bufferClient = System.Text.Encoding.UTF8.GetBytes("CommandSendFalse" + "end");
                            socketSend.Send(bufferClient);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "远程主机强迫关闭了一个现有的连接。" || ex.Message == "无法访问已释放的对象。\r\n对象名:“System.Net.Sockets.Socket”。")
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 等待小车端的请求连接
        /// </summary>
        /// <param name="o"></param>
        void Listen(object o)                   
        {
            Socket socketWatch = o as Socket;
            SocketHeart model;//心跳包集合元素
            //等待客户端的连接 并且创建一个负责通信的Socket
            while (true)
            {
                try
                {
                    //负责跟客户端通信的Socket
                    socketSend = socketWatch.Accept();
                    //将远程连接的客户端的IP地址和Socket存入集合中
                    dicSocket.Add(socketSend.RemoteEndPoint.ToString(), socketSend);
                    PlanList.Add(socketSend.RemoteEndPoint.ToString(), "0");
                    ReceiveList.Add(socketSend.RemoteEndPoint.ToString(), new ReceiveIn());
                    ShowNewController sh = new ShowNewController(CreateControllerForm); //调用委托来创建窗体
                    this.Invoke(sh, socketSend.RemoteEndPoint.ToString());
                    model = new SocketHeart();
                    model.SocketName = socketSend.RemoteEndPoint.ToString();
                    HeartList.Add(model);
                    CommandList.Add(socketSend.RemoteEndPoint.ToString(), new List<SocketCommand>());       //小车命令集
                    richTextBox1.Text += DateTime.Now.ToShortTimeString() + "：" + model.SocketName + "上线\r";
                    ShowNewController sc = new ShowNewController(HeartLabel); //调用委托来创建窗体
                    this.Invoke(sc, socketSend.RemoteEndPoint.ToString());
                    //将远程连接的客户端的IP地址和端口号存储下拉框中         
                    Thread th = new Thread(Receive);
                    th.IsBackground = true;
                    th.Start(socketSend);
                }
                catch 
                {

                }
            }
        }


        void Receive(object o)               //接收消息的方法
        {
            Socket socketSend = o as Socket;
            byte[] HeartBuffer = new byte[10 * 10 * 2];
            HeartBuffer[3] = 169;
            while (true)
            {
                Thread.Sleep(100);
                try
                {
                    //客户端连接成功后，服务器应该接受客户端发来的消息
                    byte[] buffer = new byte[1024 * 1024 * 2];
                    //实际接受到的有效字节数
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        socketSend.Send(HeartBuffer);
                    }
                    if (buffer[0] == 111)               //心跳包数据
                    {
                        HeartList.Where(u => u.SocketName == socketSend.RemoteEndPoint.ToString()).FirstOrDefault().LastConnectTime = DateTime.Now;
                        ReceiveIn receive = new ReceiveIn();
                        receive.FX = System.Text.Encoding.Unicode.GetString(buffer, 8, 8);
                        receive.percent = BitConverter.ToDouble(buffer, 16).ToString("F2");
                        receive.speed = BitConverter.ToDouble(buffer, 24).ToString("F2");
                        if (Convert.ToDouble(receive.speed) > 1000)
                        {
                            WaitList.Clear();
                            foreach (var item in Form1.dicSocket)
                            {
                                byte[] sendBuffer = new byte[5];
                                buffer[3] = 170;
                                Form1.dicSocket[item.Key].Send(sendBuffer);
                            }
                        }
                        receive.ines = BitConverter.ToDouble(buffer, 32).ToString("F2");
                        receive.alarm = new byte[10];
                        Array.Copy(buffer, 40, receive.alarm, 0, 4);
                        receive.CommandNum = BitConverter.ToInt32(buffer, 48).ToString();
                        receive.wdAlarm = new byte[10];
                        Array.Copy(buffer, 52, receive.alarm, 0, 4);
                        label31.Text = BitConverter.ToDouble(buffer, 56).ToString("F2");
                        label33.Text = BitConverter.ToDouble(buffer, 64).ToString("F2");

                        ReceiveList[socketSend.RemoteEndPoint.ToString()] = receive;
                        ShowNewController sh = new ShowNewController(Create);
                        this.Invoke(sh, socketSend.RemoteEndPoint.ToString());
                    }
                }
                catch
                {
                }
                try
                {
                }
                catch { }
            }
        }

        /// <summary>
        /// 此处显示从小车反馈的信息
        /// </summary>
        /// <param name="SocketName"></param>
        void Create(string SocketName)
        {
            PlanList[SocketName] = ReceiveList[SocketName].percent;
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                try
                {
                    if (dataGridView2.Rows[i].Cells[1].Value.ToString() != "" && SocketName.Contains(dataGridView2.Rows[i].Cells[1].Value.ToString()))
                    {
                        Valuechange = true;
                        byte[] buffer = ReceiveList[SocketName].alarm;
                        dataGridView2.Rows[i].Cells[3].Value = ReceiveList[SocketName].FX;
                        dataGridView2.Rows[i].Cells[4].Value = ReceiveList[SocketName].percent;
                        dataGridView2.Rows[i].Cells[5].Value = ReceiveList[SocketName].speed;
                        dataGridView2.Rows[i].Cells[6].Value = ReceiveList[SocketName].ines;
                        dataGridView2.Rows[i].Cells[7].Value = buffer[0].ToString() + buffer[1].ToString() + buffer[2].ToString() + buffer[3].ToString();
                        buffer = ReceiveList[SocketName].wdAlarm;
                        dataGridView2.Rows[i].Cells[8].Value = buffer[0].ToString() + buffer[1].ToString() + buffer[2].ToString() + buffer[3].ToString();
                        Valuechange = false;
                        break;
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 将小车的命令集转为byte[]以便于发送
        /// </summary>
        /// <param name="SocketName"></param>
        /// <returns></returns>
        public static byte[] CommandToByte(string SocketName)
        {           
            byte[] buffer = new byte[10 * 10];
            List<byte> byteList = new List<byte>();         //将指令转为四位Byte同时加入到btye集合
            foreach (var item in CommandList[SocketName])
            {
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.Speed)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.Forward)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.Back)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.Turn)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.Oblique)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.SForward)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.RightShift)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.LeftShift)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.ObliqueLU)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.ObliqueLD)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.ObliqueRU)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.ObliqueRD)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.Angle)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.CommandNum)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.FLShift)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.FRShift)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.BLShift)));
                byteList.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.BRShift)));
            }
            buffer = byteList.ToArray();
            return buffer;
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="oneCommand"></param>
        public void SendCommand(OneCommand oneCommand)
        {
            List<Command> commandList = new List<Command>();
            commandList = oneCommand.command;
            foreach (Command command in commandList)
            {
                SocketCommand socketCommand = new SocketCommand();
                switch (command.CommandFx)
                {
                    case "前进":
                        socketCommand.Forward = command.CommandRange;
                        break;
                    case "后退":
                        socketCommand.Back = command.CommandRange;
                        break;
                    case "逆转":
                        socketCommand.Turn = command.CommandRange;
                        break;
                    case "正转":
                        socketCommand.Oblique = command.CommandRange;
                        break;
                    case "等待":
                        socketCommand.SForward = command.CommandRange;
                        break;
                    case "右平移":
                        socketCommand.RightShift = command.CommandRange;
                        break;
                    case "左平移":
                        socketCommand.LeftShift = command.CommandRange;
                        break;
                    case "左上斜线":
                        socketCommand.ObliqueLU = command.CommandRange;
                        break;
                    case "左下斜线":
                        socketCommand.ObliqueLD = command.CommandRange;
                        break;
                    case "右上斜线":
                        socketCommand.ObliqueRU = command.CommandRange;
                        break;
                    case "右下斜线":
                        socketCommand.ObliqueRD = command.CommandRange;
                        break;
                    case "左前画圆":
                        socketCommand.FLShift = command.CommandRange;
                        break;
                    case "右前画圆":
                        socketCommand.FRShift = command.CommandRange;
                        break;
                    case "左后画圆":
                        socketCommand.BLShift = command.CommandRange;
                        break;
                    case "右后画圆":
                        socketCommand.BRShift = command.CommandRange;
                        break;
                    default:
                        break;
                }
                socketCommand.Speed = command.CommandSpeed.Trim();
                socketCommand.Angle = command.Angle.Trim();
                string num = oneCommand.CommandNum.Substring(2);
                socketCommand.CommandNum = num;
                for (int i = 0; i < CarIpList.Count; i++)
                {
                    try
                    {
                        if (CarIpList[i].Carnum == command.Carname)
                        {
                            if (LabelList[i].Text != "")
                            {
                                CommandList[LabelList[i].Text].Add(socketCommand);
                                byte[] buffer = new byte[10 * 10];
                                buffer = CommandToByte(LabelList[i].Text);             //命令集转为Byte[]数组
                                dicSocket[LabelList[i].Text].Send(buffer);
                              //  richTextBox1.Text += DateTime.Now + "  :" + i + "号车  " + command.CommandFx + "\r\n";
                                CommandList[LabelList[i].Text].Clear();
                            }
                            break;
                        }
                    }
                    catch 
                    {
                        MessageBox.Show("请检查绑定的数据是否有误");
                    }
                }

            }
        }
        #endregion

        /// <summary>
        /// 添加小车
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            Valuechange = true;
            dataGridView2.Rows.Clear();                                 //清空datagridView控件内的数据
            dataGridView1.Rows.Clear();
            
            int i = 0;
            int j = 0;
            try
            {

                for (i = 0; i < CarIpList.Count; i++)
                {
                    if (CarIpList[i].Carnum == "")
                    {
                        CarIpList[i].Carnum = i+1 + "号车";
                        break;
                    }
                }
            }
            catch { }
            SaveCarIp();                                                //保存新的车号到CarIpList

            try
            {
                for (i = 0; i < CarIpList.Count; i++)
                {
                    if (CarIpList[i].Carnum == "")
                    {                       
                    }
                    else
                    {
                        DataGridViewRow row = new DataGridViewRow();

                        DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell();
                        textboxcell.Value = "";
                        row.Cells.Add(textboxcell);
                        DataGridViewRow row1 = new DataGridViewRow();

                        DataGridViewTextBoxCell textboxcell1 = new DataGridViewTextBoxCell();
                        textboxcell1.Value = "";
                        row1.Cells.Add(textboxcell1);
                        dataGridView2.Rows.Add(row);
                        dataGridView2[0, j].Value = CarIpList[i].Carnum;
                        dataGridView2[1, j].Value = CarIpList[i].Carip;
                        dataGridView1.Rows.Add(row1);
                        dataGridView1[0, j].Value = CarIpList[i].Carnum;                    //将小车的信息显示在datagridView控件内
                        //dataGridView1[1, j].Value = CarIpList[i].CommandFx;
                        //dataGridView1[2, j].Value = CarIpList[i].CommandRange;
                        //dataGridView1[3, j].Value = CarIpList[i].CommandSpeed;
                        //dataGridView1[4, j].Value = CarIpList[i].Angle;
                        j++;
                    }
                }
            }
            catch { }
            Valuechange = false;
        }




        private void Form1_Load(object sender, EventArgs e)
        {
            skinEngine1.SkinFile = Application.StartupPath + @"\Skin\office2007.ssk";
            //Bitmap bitM = new Bitmap(Image.FromFile(Application.StartupPath + @"\map.jpg", false));          
        }

        /// <summary>
        /// 实际坐标（单位m）与图像坐标（单位像素）转换函数
        /// </summary>
        /// <param name="x">实际x坐标，单位m</param>
        /// <param name="y">实际y坐标，单位m</param>
        private void CoordinateTransformation(float carXCoordinate, float carYCoordinate, out float X, out float Y)
        {
            /* 通过调整相乘系数和相加系数，调整地图模型显示
             * carXCoordinate与carYCoordinate均为负值，表示XY坐标轴完全相反；
             */
            X = 270 + 10 * carXCoordinate;
            Y = 410 - 10 * carYCoordinate;
        }

        #region 心跳包检测
        void Heartcheck(string SocketName)
        {
            dicSocket.Remove(SocketName);
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {

                if (dataGridView2.Rows[i].Cells[1].Value.ToString() != "" && SocketName.Contains(dataGridView2.Rows[i].Cells[1].Value.ToString()))
                {
                    dataGridView2.Rows[i].Cells[2].Value = "离线";
                    break;
                }
            }
            RemoveSocketName.Add(SocketName);
            CommandList.Remove(SocketName);
            PlanList.Remove(SocketName);
        }

        /// <summary>
        /// 心跳包检测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeartCheck_Tick(object sender, EventArgs e)
        {            
            TimeSpan span = new TimeSpan();
            RemoveSocketName = new List<string>();
            try
            {
                foreach (var item in HeartList)
                {
                    try
                    {
                        Valuechange = true;
                        for (int i = 0; i < dataGridView2.Rows.Count -1; i++)
                        {
                            if (dataGridView2.Rows[i].Cells[1].Value.ToString() == "")
                            {
                                dataGridView2.Rows[i].Cells[2].Value = "";
                            }
                            else
                            {
                                if (item.SocketName.Contains(dataGridView2.Rows[i].Cells[1].Value.ToString()))
                                {
                                    dataGridView2.Rows[i].Cells[2].Value = "在线";
                                    break;
                                }
                            }
                        }
                        Valuechange = false;
                    }
                    catch { }
                    ShowNewController sc = new ShowNewController(HeartLabel); //调用委托来创建窗体
                    this.Invoke(sc, item.SocketName);

                    span = DateTime.Now - item.LastConnectTime;         //心跳包检测
                    if (span.TotalSeconds > OverTime)                            //超时时间
                    {
                        dicSocket.Where(u => u.Key == item.SocketName).FirstOrDefault().Value.Close();


                        dicSocket.Remove(item.SocketName);
                        for (int i = 0; i < dataGridView2.Rows.Count; i++)
                        {

                            if (dataGridView2.Rows[i].Cells[1].Value.ToString() != "" && item.SocketName.Contains(dataGridView2.Rows[i].Cells[1].Value.ToString()))
                            {
                                dataGridView2.Rows[i].Cells[2].Value = "离线";
                                LabelList[i].Text = "";
                                break;
                            }
                        }
                        RemoveSocketName.Add(item.SocketName);
                        CommandList.Remove(item.SocketName);
                        PlanList.Remove(item.SocketName);
                        //ShowNewController sh = new ShowNewController(Heartcheck); //调用委托来创建窗体
                        //this.Invoke(sh, item.SocketName);
                    }
                }
            }
            catch { }
            foreach (var item in RemoveSocketName)                  //移除心跳包集合
            {
                HeartList.Remove(HeartList.Where(u => u.SocketName == item).FirstOrDefault());
            }
        }
        #endregion


        /// <summary>
        /// CarIp改变时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (Valuechange == false)
            {
                try
                {
                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        if (dataGridView2.Rows[i].Cells[0].Value.ToString() != "")
                        {
                            CarIpList[i].Carnum = dataGridView2.Rows[i].Cells[0].Value.ToString();

                            CarIpList[i].Carip = dataGridView2.Rows[i].Cells[1].Value.ToString();
                        }
                    }
                }
                catch { }
                SaveCarIp();
            }
        }


        /// <summary>
        /// 小车指令变化时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellValueChanged_1(object sender, DataGridViewCellEventArgs e)
        {

            if (Valuechange == false)
            {
                List<Command> CommandList = new List<Command>();
                try
                {

                    foreach (OneCommand OneCommand in OneCommandList)
                    {

                        if (OneCommand.CommandNum == treeView1.SelectedNode.Name)
                        {
                            CommandList = OneCommand.command;
                            break;
                        }

                    }
                }
                catch
                {

                }
                try
                {
                                                              
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() != "")
                        {
                            foreach(Command Command in CommandList)
                            {
                                if(Command.Carname == dataGridView1.Rows[i].Cells[0].Value.ToString())
                                {
                                    Command.CommandFx = dataGridView1.Rows[i].Cells[1].Value.ToString();
                                    Command.CommandRange = dataGridView1.Rows[i].Cells[2].Value.ToString();
                                    Command.CommandSpeed = dataGridView1.Rows[i].Cells[3].Value.ToString();
                                    Command.Angle = dataGridView1.Rows[i].Cells[4].Value.ToString();
                                    for (int j = 0; j < treeView1.SelectedNode.Nodes.Count; j++)
                                    {
                                        if(Command.Carname.Length == 3)
                                        {
                                            if(treeView1.SelectedNode.Nodes[j].Text.Substring(0,3) == Command.Carname)
                                            {
                                                treeView1.SelectedNode.Nodes[j].Text = Command.Carname + " " + Command.CommandFx + " " + Command.CommandRange + "mm  速度" + Command.CommandSpeed + "mm/s 角度" + Command.Angle;
                                                break;                                            
                                            }
                                        }
                                        if (Command.Carname.Length == 4)
                                        {
                                            if (treeView1.SelectedNode.Nodes[j].Text.Substring(0, 4) == Command.Carname)
                                            {
                                                treeView1.SelectedNode.Nodes[j].Text = Command.Carname + " " + Command.CommandFx + " " + Command.CommandRange + "mm  速度" + Command.CommandSpeed + "mm/s 角度" + Command.Angle;
                                                break;
                                            }
                                        }

                                    }
                                    SaveOneCommand();
                                    break;
                                }
                            }
                        }
                    }
                }
                catch { }
               // SaveCarIp();
                
                //string ID = treeView1.SelectedNode.Name;
                //form1.treeView1.Nodes.Clear();
                //OneCommand Comm = new OneCommand();
                //Comm.CommandNum = "";
                //treeview(Comm);
                //try
                //{
                //    foreach (TreeNode treenode in treeView1.Nodes)
                //    {
                //        TreeNode temp = FindNode(treenode, ID);
                //        if (temp != null)
                //        {
                //            if (temp.Name == ID)
                //            {
                //                temp.BackColor = Color.Orange;
                //                treeView1.SelectedNode = temp;
                //                treeView1.SelectedNode.Expand();
                //            }
                //            else
                //            {
                //                temp.BackColor = Color.White;
                //            }

                //        }
                //    }
                //}
                //catch { }
            }
        }

        /// <summary>
        /// 新建命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            string NodeName = "";
            for(int i = 0; i < OneCommandList.Count; i++)
            {
                if (OneCommandList[i].CommandNum == "")
                {
                    OneCommandList[i].CommandNum = "命令" + (i + 1);
                    OneCommandList[i].CommandName = textBox3.Text;
                    //treeView1.Nodes.Add("命令" + (i + 1)+ " - " + textBox3.Text);
                    TreeNode node = new TreeNode();
                    node.Text = OneCommandList[i].CommandNum + " - " + OneCommandList[i].CommandName;
                    node.Name = OneCommandList[i].CommandNum;
                    treeView1.Nodes.Add(node);
                    NodeName = OneCommandList[i].CommandNum;
                    break;

                }
            }
            SaveOneCommand();
            //treeView1.Nodes.Clear();
            //OneCommand oneCommand = new OneCommand();
            //oneCommand.CommandNum = "";
            //treeview(oneCommand);

            foreach (TreeNode treenode in treeView1.Nodes)
            {
                TreeNode temp = FindNode(treenode, NodeName);
                if (temp != null)
                {
                    if (temp.Name == NodeName)
                    {
                        treeView1.SelectedNode = temp;

                    }
                }
            }

           // treeView1.SelectedNode = treeView1.Nodes[0];
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        /// <summary>
        /// 删除命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {            
            RemoveBox(treeView1.SelectedNode.Name);

            for (int i = 0; i < OneCommandList.Count; i++)
            {
                if (OneCommandList[i].CommandNum == treeView1.SelectedNode.Name)
                {
                    OneCommandList[i].CommandNum = "";
                    OneCommandList[i].CommandName = "";
                    OneCommandList[i].BoolFather = true;
                    OneCommandList[i].FatherPoint = "";
                    OneCommandList[i].Finish = "";
                    OneCommandList[i].WaitTime = "";
                    OneCommandList[i].WaitCar = "";
                    OneCommandList[i].command = new List<Command>();
                    SaveOneCommand();
                    treeView1.SelectedNode.Remove();
                    break;
                }
            }
            //treeView1.Nodes.Clear();
            //OneCommand oneCommand = new OneCommand();
            //oneCommand.CommandNum = "";
            //treeview(oneCommand);
            //treeView1.SelectedNode = treeView1.Nodes[0];
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FatherPoint"></param>
        void RemoveBox(string FatherPoint)
        {
            for (int i = 0; i < Form1.OneCommandList.Count; i++)
            {
                if (OneCommandList[i].CommandNum != "" && OneCommandList[i].FatherPoint == FatherPoint)
                {                   
                    RemoveBox(OneCommandList[i].CommandNum);
                    OneCommandList[i].CommandNum = "";
                    OneCommandList[i].CommandName = "";
                    OneCommandList[i].BoolFather = true;
                    OneCommandList[i].FatherPoint = "";
                    OneCommandList[i].Finish = "";
                    OneCommandList[i].WaitTime = "";
                    OneCommandList[i].WaitCar = "";
                    OneCommandList[i].command = new List<Command>();
                    SaveOneCommand();
                }
            }
        }

        /// <summary>
        /// 修改命令名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < OneCommandList.Count; i++)
            {

                if (OneCommandList[i].CommandNum == treeView1.SelectedNode.Name && treeView1.SelectedNode.Name != "")
                {
                    OneCommandList[i].CommandName = textBox3.Text;

                    SaveOneCommand();
                    treeView1.SelectedNode.Text = OneCommandList[i].CommandNum + " - " + OneCommandList[i].CommandName;
                    break;
                }
            }
        }

        /// <summary>
        /// 为选中的命令添加色彩
        /// </summary>
        /// <param name="oneCommand"></param>
        public void color(OneCommand oneCommand)
        {
            foreach (TreeNode treenode in treeView1.Nodes)
            {
                TreeNode temp = FindNode(treenode, oneCommand.CommandNum);
                if (temp != null)
                {
                    if (temp.Name == treeView1.SelectedNode.Name)
                    {
                        temp.BackColor = Color.Orange;
                    }
                    else
                    {
                        temp.BackColor = Color.White;
                    }
                }
            }
        }


        /// <summary>
        /// 点击节点显示节点命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode.Name == "")
            {
              //  treeView1.SelectedNode.Name = treeView1.SelectedNode.Parent.Name;
                treeView1.SelectedNode = treeView1.SelectedNode.Parent;
            }
            else
            {
                textBox1.Text = treeView1.SelectedNode.Name;
            }
            treeView1.SelectedNode.Expand();
            foreach(OneCommand OneCommand in OneCommandList)
            {
                color(OneCommand);
                if(OneCommand.CommandNum == treeView1.SelectedNode.Name)
                {
                    Valuechange = true;
                    textBox3.Text = OneCommand.CommandName;

                    List<Command> CommandList = OneCommand.command;
                    for(int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                    {
                        for (int j = 0; j < CommandList.Count; j++)
                        {
                            if(dataGridView1.Rows[i].Cells[0].Value.ToString() == CommandList[j].Carname)
                            {
                                dataGridView1.Rows[i].Cells[1].Value = CommandList[j].CommandFx;
                                dataGridView1.Rows[i].Cells[2].Value = CommandList[j].CommandRange;
                                dataGridView1.Rows[i].Cells[3].Value = CommandList[j].CommandSpeed;
                                dataGridView1.Rows[i].Cells[4].Value = CommandList[j].Angle;
                                break;
                            }
                            if(j == CommandList.Count - 1)
                            {
                                dataGridView1.Rows[i].Cells[1].Value = "";
                                dataGridView1.Rows[i].Cells[2].Value = "";
                                dataGridView1.Rows[i].Cells[3].Value = "";
                                dataGridView1.Rows[i].Cells[4].Value = "";
                            }
                        }

                    }
                    Valuechange = false;                    
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Nest nest = new Nest();
            nest.Show();
        }

        public bool WaitOf()
        {
            double value = 0;
            if (PlanList != null)
            {
                foreach (var item in PlanList)
                {
                    if(item.Value == "非数字")
                    {
                        value = 0;
                    }
                    else
                    {
                        value = Convert.ToDouble(item.Value);
                    }
                    if (value > 0 && value < 100)
                    {
                        return false;                       
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            if (WaitOf())
            {
                richTextBox3.Text = "";
                foreach(OneCommand oneCommand in OneCommandList)
                {
                    if(oneCommand.CommandNum == treeView1.SelectedNode.Name)
                    {
                        SendCommand(oneCommand);
                        Wait(treeView1.SelectedNode.Name);                        
                        break;
                    }
                }                
            }        
        }

        /// <summary>
        /// 等待集合
        /// </summary>
        /// <param name="FatherPoint"></param>
        void Wait(string FatherPoint)
        {
            for (int i = 0; i < OneCommandList.Count; i++)
            {
                if (OneCommandList[i].CommandNum != "" && OneCommandList[i].FatherPoint == FatherPoint)
                {
                   if(OneCommandList[i].Finish == "" && OneCommandList[i].WaitTime == "")
                   {
                       if (OneCommandList[i].WaitCar == "0")
                       {
                            SendCommand(OneCommandList[i]);
                             Wait(OneCommandList[i].CommandNum); 
                       }
                       else
                       {
                           WaitSocket WaitSocket = new WaitSocket();
                           WaitSocket.Booltime = "2";
                           try
                           {
                               foreach (OneCommand OneCommand in OneCommandList)
                               {
                                   if (OneCommand.CommandNum == FatherPoint)
                                   {
                                       List<Command> commandList = OneCommand.command;
                                       for (int j = 0; j < CarIpList.Count; j++)
                                       {
                                           foreach (Command command in commandList)
                                           {
                                               if (command.Carname == CarIpList[j].Carnum && LabelList[j].Text != "")
                                               {
                                                   WaitSocket.WaitSocetName = LabelList[j].Text;
                                                   WaitSocket.FatherPoint = FatherPoint;
                                                   WaitSocket.OneCommand = OneCommandList[i];
                                                   WaitList.Add(WaitSocket);
                                                   return;
                                               }
                                           }
                                       }
                                       break;
                                   }
                               }
                           }
                           catch { }
                       }
                      // SendCommand(OneCommandList[i]);
                     //  Wait(OneCommandList[i].CommandNum);              ///如果是普通嵌套，直接发送命令
                                                                        ///并将该命令递归找到下一个发送的子节点                       
                   }
                   else if (OneCommandList[i].Finish != "")
                   {

                       WaitSocket WaitSocket = new WaitSocket();
                       WaitSocket.Booltime = "0";
                       WaitSocket.OneCommand = OneCommandList[i];              //完成度嵌套，新加WaitSocket
                       //Booltime = 0,
                       for (int j = 0; j < CarIpList.Count; j++)
                       {
                           if (CarIpList[j].Carnum == OneCommandList[i].WaitCar)
                           {
                               WaitSocket.WaitSocetName = LabelList[j].Text;
                               WaitSocket.FatherPoint = FatherPoint;
                               break;
                           }
                       }
                       WaitList.Add(WaitSocket);                   
                   }
                   else if (OneCommandList[i].WaitTime != "")
                   {
                       WaitSocket WaitSocket = new WaitSocket();
                       WaitSocket.Booltime = "1";
                       WaitSocket.OneCommand = OneCommandList[i];
                       WaitSocket.Time = DateTime.Now;
                       WaitList.Add(WaitSocket);
                   }
                }
            }
        }

        /// <summary>
        /// 该timer用来执行在等待集合里的命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            
            try
            {
                foreach (WaitSocket WaitSocket in WaitList)
                {
                    if (WaitSocket.Booltime == "1")                   ///延时类的嵌套
                    {
                        TimeSpan TS;
                        if (WaitSocket.Stime.Seconds > 0)
                        {
                            TS = DateTime.Now - WaitSocket.Time + WaitSocket.Stime;
                        }
                        else
                        {
                            TS = DateTime.Now - WaitSocket.Time;
                        }
                        if (TS.Seconds >= Convert.ToInt32(WaitSocket.OneCommand.WaitTime))
                        {
                            SendCommand(WaitSocket.OneCommand);
                            Thread.Sleep(200);
                            Wait(WaitSocket.OneCommand.CommandNum);
                            WaitList.Remove(WaitSocket);
                        }
                    }
                    else if (WaitSocket.Booltime == "0")                ///完成度的嵌套
                    {
                        try
                        {
                            string Num = ReceiveList[WaitSocket.WaitSocetName].CommandNum;
                            if (PlanList[WaitSocket.WaitSocetName] != null && Convert.ToDouble(PlanList[WaitSocket.WaitSocetName]) >= Convert.ToInt32(WaitSocket.OneCommand.Finish))
                            {
                                if (WaitSocket.FatherPoint.Substring(2) == Num)
                                {
                                    SendCommand(WaitSocket.OneCommand);
                                    Thread.Sleep(200);
                                    Wait(WaitSocket.OneCommand.CommandNum);
                                    WaitList.Remove(WaitSocket);
                                }
                            }
                        }
                        catch { }
                    }
                    else if (WaitSocket.Booltime == "2")                ///嵌套
                    {
                        try
                        {   
                            string Num = ReceiveList[WaitSocket.WaitSocetName].CommandNum;
                            if (PlanList[WaitSocket.WaitSocetName] != null && Convert.ToDouble(PlanList[WaitSocket.WaitSocetName]) >= 100)
                            {
                                if (WaitSocket.FatherPoint.Substring(2) == Num)
                                {
                                    SendCommand(WaitSocket.OneCommand);
                                    Thread.Sleep(200);
                                    Wait(WaitSocket.OneCommand.CommandNum);
                                    WaitList.Remove(WaitSocket);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch
            {

            }
            pictureBoxMap_Paint(null, null);
        }

        /// <summary>
        /// 急停与惯导清零的点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 9 && LabelList[e.RowIndex].Text != "")
                {

                    byte[] buffer = new byte[5];
                    buffer[3] = 170;                //急停
                    dicSocket[LabelList[e.RowIndex].Text].Send(buffer);
                }
                else if (e.ColumnIndex == 6 && LabelList[e.RowIndex].Text != "")
                {
                    DialogResult QuitForm = MessageBox.Show("该车惯导是否归零", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (QuitForm == DialogResult.OK)
                    {
                        byte[] buffer = new byte[5];
                        buffer[3] = 10;                //惯导清零
                        dicSocket[LabelList[e.RowIndex].Text].Send(buffer);
                    }
                }
                else if(e.ColumnIndex == 1)
                {
                }
                else
                {
                    dataGridView2.Rows[e.RowIndex].Selected = true;
                }
            }
            catch { }

        }

        /// <summary>
        /// 全部急停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                WaitList.Clear();
                foreach (var item in Form1.dicSocket)
                {
                    byte[] buffer = new byte[5];
                    buffer[3] = 170;
                    Form1.dicSocket[item.Key].Send(buffer);

                }
                foreach (var item in Form1.dicSocket)
                {
                    byte[] buffer = new byte[5];
                    buffer[3] = 10;
                    Form1.dicSocket[item.Key].Send(buffer);

                }
                MessageBox.Show("发送成功！");
            }
            catch (Exception)
            {
                MessageBox.Show("发送失败！");
            }
        }

        /// <summary>
        /// 键盘发送
        /// </summary>
        /// <param name="buffer"></param>
        void button_down(byte [] buffer)
        {
            try
            {
                if (dataGridView2.SelectedRows[0].Cells[1].Value.ToString() != "")
                {
                    foreach (Label label in LabelList)
                    {
                        if (label.Text != "" && label.Text.Contains(dataGridView2.SelectedRows[0].Cells[1].Value.ToString()))
                        {
                            dicSocket[label.Text].Send(buffer);
                            BoolSend = false;
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

        /// <summary>
        /// 键盘控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(!KeyBoard)
            {
                return;
            }
            if (e.KeyData == (Keys.Control | Keys.F1))                      //强制发送
            {

                foreach (OneCommand oneCommand in OneCommandList)
                {
                    if (oneCommand.CommandNum == treeView1.SelectedNode.Name)
                    {
                        SendCommand(oneCommand);
                        Wait(treeView1.SelectedNode.Name);
                        break;
                    }
                }
            }
            else if (e.KeyValue == 112)                                   ///F1    发送
            {
                button6_Click_1(null, null);
            }
            else if (e.KeyValue == 113)                              //F2    暂停
            {
                //foreach (OneCommand oneCommand in OneCommandList)
                //{
                //    if (oneCommand.CommandNum == treeView1.SelectedNode.Name)
                //    {
                //        byte[] buffer = new byte[5];
                //        buffer[3] = 180;
                //        PauseTree(oneCommand,buffer);
                //        break;
                //    }
                //}
                foreach (var item in Form1.dicSocket)
                {
                    byte[] buffer = new byte[5];
                    buffer[3] = 180;
                    Form1.dicSocket[item.Key].Send(buffer);

                }
                timer1.Enabled = false;
                foreach (WaitSocket Waitsocket in WaitList)
                {
                    if (Waitsocket.Booltime == "1")
                    {
                        Waitsocket.Stime = DateTime.Now - Waitsocket.Time;
                    }
                }
            }
            else if (e.KeyValue == 114)                              //F3    解除暂停
            {
                //foreach (OneCommand oneCommand in OneCommandList)
                //{
                //    if (oneCommand.CommandNum == treeView1.SelectedNode.Name)
                //    {
                //        byte[] buffer = new byte[5];
                //        buffer[3] = 181;
                //        PauseTree(oneCommand, buffer);
                //        break;
                //    }
                //}
                foreach (var item in Form1.dicSocket)
                {
                    byte[] buffer = new byte[5];
                    buffer[3] = 181;
                    Form1.dicSocket[item.Key].Send(buffer);

                }
                foreach (WaitSocket Waitsocket in WaitList)
                {
                    if (Waitsocket.Booltime == "1")
                    {
                        Waitsocket.Time = DateTime.Now;
                    }
                }
                timer1.Enabled = true;

            }
            else if (e.KeyValue == 115)                              //F4    急停
            {
                WaitList.Clear();
                foreach (var item in Form1.dicSocket)
                {
                    byte[] buffer = new byte[5];
                    buffer[3] = 170;
                    Form1.dicSocket[item.Key].Send(buffer);

                }
                //foreach (OneCommand oneCommand in OneCommandList)
                //{
                //    if (oneCommand.CommandNum == treeView1.SelectedNode.Name)
                //    {
                       
                //        byte[] buffer = new byte[5];
                //        buffer[3] = 170;
                //        PauseTree(oneCommand, buffer);
                        
                //        break;
                //    }
                //}
                timer1.Enabled = true;
            }
            else
            {
                if (BoolSend)
                {
                    byte[] buffer = new byte[8];
                    try
                    {
                        if (Convert.ToInt32(speedtext.Text) > 0)
                        {
                            BitConverter.GetBytes(Convert.ToInt32(speedtext.Text)).CopyTo(buffer, 4);
                        }
                        else
                        {
                            BitConverter.GetBytes(50).CopyTo(buffer, 4);
                        }
                    }
                    catch 
                    {
                        BitConverter.GetBytes(50).CopyTo(buffer, 4);
                    }
                    if (e.KeyValue == 87)
                    {
                        buffer[3] = 1;
                        button_down(buffer);
                    }
                    else if (e.KeyValue == 83)
                    {

                        buffer[3] = 2;       //后退微调
                        button_down(buffer);
                    }
                    else if (e.KeyValue == 65)
                    {
                        buffer[3] = 3;       //左平移微调
                        button_down(buffer);
                    }
                    else if (e.KeyValue == 68)
                    {
                        buffer[3] = 4;       //右平移微调
                        button_down(buffer);
                    }
                    else if (e.KeyValue == 69)
                    {
                        buffer[3] = 5;         //正转微调
                        button_down(buffer);
                    }
                    else if (e.KeyValue == 81)
                    {
                        buffer[3] = 6;        //逆转微调
                        button_down(buffer);
                    }

                }
                
            }
        }

        /// <summary>
        /// 键盘发送指令
        /// </summary>
        /// <param name="oneCommand"></param>
        /// <param name="buffer"></param>
        public void PauseTree(OneCommand oneCommand,byte [] buffer)
        {
            List<Command> commandList = new List<Command>();
            commandList = oneCommand.command;
            foreach (Command command in commandList)
            {
                for (int i = 0; i < CarIpList.Count; i++)
                {
                    if (CarIpList[i].Carnum == command.Carname)
                    {
                        if (LabelList[i].Text != "")
                        {
                            dicSocket[LabelList[i].Text].Send(buffer);
                        }                        
                        break;
                    }
                }
            }
            Pause(oneCommand.CommandNum, buffer);
        }
        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="FatherPoint"></param>
        void Pause(string FatherPoint, byte [] buffer)
        {
            for (int i = 0; i < Form1.OneCommandList.Count; i++)
            {
                if (OneCommandList[i].CommandNum != "" && OneCommandList[i].FatherPoint == FatherPoint)
                {
                    PauseTree(OneCommandList[i], buffer);
                }
            }
        }

        /// <summary>
        /// 急停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if(!KeyBoard)
            {
                return;
            }
            try
            {
                if (e.KeyValue == 81 || e.KeyValue == 69 || e.KeyValue == 68 || e.KeyValue == 65 || e.KeyValue == 83 || e.KeyValue == 87)
                {
                    if (dataGridView2.SelectedRows[0].Cells[1].Value.ToString() != "")
                    {
                        foreach (Label label in LabelList)
                        {
                            if (label.Text != "" && label.Text.Contains(dataGridView2.SelectedRows[0].Cells[1].Value.ToString()))
                            {
                                byte[] buffer = new byte[10 * 10 * 2];
                                buffer[3] = 171;                //急停
                                dicSocket[label.Text].Send(buffer);
                                Thread.Sleep(200);
                                dicSocket[label.Text].Send(buffer);
                                BoolSend = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 新增接受命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            if(textBox6.Text == "")
            {
                MessageBox.Show("未填写文本！");
                return;
            }
            if (comboBox1.Text == "")
            {
                MessageBox.Show("未选择类型！");
                return;
            }
            if (comboBox2.Text == "")
            {
                MessageBox.Show("未选择命令！");
                return;
            }
            try
            {
                if (Convert.ToInt32(textBox6.Text) < 0 || Convert.ToInt32(textBox6.Text) > 100)
                {
                    MessageBox.Show("文本设置不符");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("文本应为整数");
                return;
            }
            for(int i = 0; i  < ReceiveCommandList.Count; i++)
            {
                if(ReceiveCommandList[i].ReceiveNum == "")
                {
                    ReceiveCommandList[i].ReceiveNum = (i + 1) + "号";
                    ReceiveCommandList[i].ReceiveText = textBox6.Text;
                    ReceiveCommandList[i].ReceiveType = comboBox1.Text;
                    ReceiveCommandList[i].CommandNum = comboBox2.Text;
                    SaveReceiveCommand();
                    break;
                }
            }
            dataGridView3.Rows.Clear();
            int j = 0;
            for (int i = 0; i < ReceiveCommandList.Count; i++)
            {
                if (ReceiveCommandList[i].ReceiveNum != "")
                {
                    DataGridViewRow row = new DataGridViewRow();

                    DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell();
                    textboxcell.Value = "";
                    row.Cells.Add(textboxcell);

                    dataGridView3.Rows.Add(row);
                    dataGridView3[0, j].Value = ReceiveCommandList[i].ReceiveNum;

                    dataGridView3[1, j].Value = ReceiveCommandList[i].ReceiveText;
                    dataGridView3[2, j].Value = ReceiveCommandList[i].ReceiveType;
                    dataGridView3[3, j].Value = ReceiveCommandList[i].CommandNum;

                    j++;
                }
            }

        }


        /// <summary>
        /// 键盘开启与否
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button12_Click(object sender, EventArgs e)
        {
            if (!KeyBoard)
            {
                KeyBoard = true;
                button12.Text = "键盘关闭";
            }
            else
            {
                KeyBoard = false;
                button12.Text = "键盘开启";
            }
        }

        /// <summary>
        /// 删除接受命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ReceiveCommandList.Count; i++)
            {
                if(dataGridView3.SelectedRows.Count != 0 )
                {
                    if(dataGridView3.SelectedRows[0].Cells[0].Value.ToString() == ReceiveCommandList[i].ReceiveNum)
                    {
                        ReceiveCommandList[i].ReceiveNum = "";
                        ReceiveCommandList[i].ReceiveText = "";
                        ReceiveCommandList[i].ReceiveType = "";
                        ReceiveCommandList[i].CommandNum = "";
                        SaveReceiveCommand();
                        break;
                    }
                }
                else
                {
                    MessageBox.Show("请选中要删除的行");
                    return;
                }
            }
            dataGridView3.Rows.Clear();
            int j = 0;
            for (int i = 0; i < ReceiveCommandList.Count; i++)
            {
                if (ReceiveCommandList[i].ReceiveNum != "")
                {
                    DataGridViewRow row = new DataGridViewRow();

                    DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell();
                    textboxcell.Value = "";
                    row.Cells.Add(textboxcell);

                    dataGridView3.Rows.Add(row);
                    dataGridView3[0, j].Value = ReceiveCommandList[i].ReceiveNum;

                    dataGridView3[1, j].Value = ReceiveCommandList[i].ReceiveText;
                    dataGridView3[2, j].Value = ReceiveCommandList[i].ReceiveType;
                    dataGridView3[3, j].Value = ReceiveCommandList[i].CommandNum;

                    j++;
                }
            }
        }

        /// <summary>
        /// 删除小车
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button13_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < CarIpList.Count; i++)
            {
                if (dataGridView2.SelectedRows.Count != 0)
                {
                    if (dataGridView2.SelectedRows[0].Cells[0].Value.ToString() == CarIpList[i].Carnum)
                    {
                        CarIpList[i].Carnum = "";
                        CarIpList[i].Carip = "";
                        CarIpList[i].CommandFx = "";
                        CarIpList[i].CommandRange = "";
                        CarIpList[i].CommandSpeed = "";
                        CarIpList[i].Angle = "";
                        SaveCarIp();
                        break;
                    }
                }
                else
                {
                    MessageBox.Show("请选中要删除的行");
                    return;
                }
            }

            Valuechange = true;
            dataGridView2.Rows.Clear();
            dataGridView1.Rows.Clear();

            int j = 0;
            try
            {

                for (int i = 0; i < CarIpList.Count; i++)
                {
                    if (CarIpList[i].Carnum == "")
                    {

                    }
                    else
                    {
                        DataGridViewRow row = new DataGridViewRow();

                        DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell();
                        textboxcell.Value = "";
                        row.Cells.Add(textboxcell);
                        DataGridViewRow row1 = new DataGridViewRow();

                        DataGridViewTextBoxCell textboxcell1 = new DataGridViewTextBoxCell();
                        textboxcell1.Value = "";
                        row1.Cells.Add(textboxcell1);
                        dataGridView2.Rows.Add(row);
                        dataGridView2[0, j].Value = CarIpList[i].Carnum;
                        dataGridView2[1, j].Value = CarIpList[i].Carip;
                        dataGridView1.Rows.Add(row1);
                        dataGridView1[0, j].Value = CarIpList[i].Carnum;
                        j++;
                    }
                }
            }
            catch { }
            Valuechange = false;
        }

        private void button14_Click(object sender, EventArgs e)
        {            
            Drive drive = new Drive();
            drive.Show();
        }

        private void speedtext_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(speedtext.Text) < 0 || Convert.ToInt32(speedtext.Text) > 500)
                {
                    speedtext.Text = 150.ToString();
                    MessageBox.Show("速度超出界限！");
                }
            }
            catch
            {
                speedtext.Text = 150.ToString();
            }
        }


        private void button16_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult QuitForm = MessageBox.Show("是否关机", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (QuitForm == DialogResult.OK)
                {
                    foreach (var item in Form1.dicSocket)
                    {
                        byte[] buffer = new byte[5];
                        buffer[3] = 170;
                        Form1.dicSocket[item.Key].Send(buffer);

                    }
                    foreach (var item in Form1.dicSocket)
                    {
                        byte[] buffer = new byte[5];
                        buffer[3] = 12;
                        Form1.dicSocket[item.Key].Send(buffer);

                    }
                    MessageBox.Show("发送成功！");
                    //System.Diagnostics.Process.Start("cmd.exe", "/cshutdown -s -t 0");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("发送失败！");
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var item in Form1.dicSocket)
                {
                    byte[] buffer = new byte[1024];
                    string StartMove = "start" + textBox7.Text;
                    buffer = Encoding.Default.GetBytes(StartMove);
                    Form1.dicSocket[item.Key].Send(buffer);
                }
            }
            catch { }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            try
            {
                double Y = Convert.ToDouble(textBox8.Text);
                textBox7.Text += textBox8.Text + "|";
            }
            catch { }
        }

        /// <summary>
        /// 保存坐标到数据库,textbox9:id label31:x label33:y，失败显示在label36,成功在label38
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savePosition_Click(object sender, EventArgs e)
        {
            Add_Position_OK.Visible = false;
            Add_Position_OK.Text = "添加点失败";
            try
            {
                int id = Convert.ToInt32(textBox9.Text);

                //double x = Convert.ToDouble(label31.Text);
                //double y = Convert.ToDouble(label33.Text);

                double x = Convert.ToDouble(textBox10.Text);
                double y = Convert.ToDouble(textBox11.Text);
                Position p = new Position(id, x, y);
                if (sql.AddPosition(p) == 1)
                    Add_Position_OK.Text = "添加点成功";
                Add_Position_OK.Visible = true;
            }
            catch (Exception ex)
            {
                Add_Position_OK.Visible = true;
                Debug.WriteLine(ex);
            }
        }


        private void button15_Click_2(object sender, EventArgs e)
        {
            try
            {
                sql.CreateTablesOfAGV();
            }
            catch
            {
                MessageBox.Show("创建失败，请检查数据库连接或是否重复创建");
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            sql.DeleteAllTables();
        }



        


        private void pictureBoxMap_Paint(object sender, PaintEventArgs e)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Pen AGVPen = new Pen(Color.Green, 1);                     // 正常情况下AGV画笔为绿色
            SolidBrush AGVBrush = new SolidBrush(Color.Green);
            Font AGVdrawFont = new Font("Arial", 8);                    // 定义端点文本格式为"Arial"
            SolidBrush AGVdrawBrushNode = new SolidBrush(Color.Black);   // 定义端点文本颜色

            AdjustableArrowCap arrow = new AdjustableArrowCap(5, 5);
            Pen LinePen = new Pen(Color.Black, 1);

            LinePen.CustomEndCap = arrow;
            SolidBrush LineBrush = new SolidBrush(Color.Orange);
            SolidBrush WorkBrush = new SolidBrush(Color.Red);

            Graphics DynamicMenu = g;  //小车的画板
            //绘制小车
            DynamicMenu.Clear(Color.LightSkyBlue);
                float AGVX, AGVY;
                CoordinateTransformation(Convert.ToSingle(label31.Text), Convert.ToSingle(label33.Text), out AGVX, out AGVY);
                //DynamicMenu.DrawRectangle(AGVPen, AGVX - 10, AGVY - 10, 10, 10);
                
                DynamicMenu.FillRectangle(AGVBrush, AGVX - 10, AGVY - 10, 20, 20);
                DynamicMenu.DrawString(label31.Text + "," + label33.Text, AGVdrawFont, AGVdrawBrushNode, AGVX - 3, AGVY - 5);
                

            PositionList = sql.SelectPositions();
            foreach(Position position in PositionList)
            {      
                float WorkX,WorkY;
                CoordinateTransformation((float)position.x, (float)position.y, out WorkX, out WorkY);
                DynamicMenu.FillEllipse(WorkBrush, WorkX - 10, WorkY - 10, 20, 20);
                DynamicMenu.DrawString(position.x + "," + position.y+"("+ position.PositionID+")", AGVdrawFont, AGVdrawBrushNode, WorkX - 3, WorkY - 5);
            }

            DirectedEdgeList = sql.SelectEdges();

            foreach(DirectedEdge dir in DirectedEdgeList)
            {                
                drawLine(g, dir,LinePen);
            }
        }

        /// <summary>
        /// 当点击添加边时触发，若添加数据库中已存在的边则添加失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_AddLine_Click(object sender, EventArgs e)
        {
            Add_Edge_OK.Visible = false;
            Add_Edge_OK.Text = "添加边失败";
            try
            {                
                int id_V = Convert.ToInt32(v_textBox.Text), id_W = Convert.ToInt32(w_textBox.Text);
                Position posV = sql.SelectPosition(id_V);
                Position posW = sql.SelectPosition(id_W);
                DirectedEdge Dir = new DirectedEdge(posV, posW);
                int changeRank = sql.AddEdge(Dir);
                if (changeRank == 1)
                    Add_Edge_OK.Text = "添加边成功";
                Add_Edge_OK.Visible = true;
            }
            catch 
            {
                Add_Edge_OK.Visible = true;
            }
        }

        
        /// <summary>
        /// 初始化地图->用来查找路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button20_Click(object sender, EventArgs e)
        {
            createGraph();
        }


        /// <summary>
        /// 点击button21 -> 查找路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button21_Click(object sender, EventArgs e)
        {
            if (graph == null)
                createGraph();
            try
            {
                Pen pen = new Pen(Color.Yellow, 3);
                int fromID = Convert.ToInt32(textBox_startID.Text), toID = Convert.ToInt32(textBox_endID.Text);
                DijkstraSP sp = new DijkstraSP(graph, fromID);
                foreach (DirectedEdge edge in sp.pathTo(toID))
                {
                    drawLine(g, edge, pen);
                }
                label_findPath_OK.Text = "  ";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                label_findPath_OK.Text = "查找失败";
            }



        }

        /// <summary>
        /// 创建地图
        /// </summary>
        private void createGraph()
        {
            int maxPosition = sql.maxPositionID();
            graph = new EdgeWeightedDigraph(maxPosition + 1);
            List<DirectedEdge> edges = sql.SelectEdges();
            foreach (DirectedEdge e in edges)
            {
                graph.AddEdge(e);
            }
        }


        /// <summary>
        /// 在graphics上画边
        /// </summary>
        /// <param name="g">需要画边的图</param>       
        /// <param name="e">边</param>
        /// <param name="pen">画笔信息</param>
        private void drawLine(Graphics g,DirectedEdge e,Pen pen)
        {
            float p1_x, p1_y, p2_x, p2_y;
            Position from=sql.SelectPosition(e.V),to=sql.SelectPosition(e.W);
            CoordinateTransformation((float)from.x, (float)from.y, out p1_x, out p1_y);
            CoordinateTransformation((float)to.x, (float)to.y, out p2_x, out p2_y);
            g.DrawLine(pen, (float)p1_x, (float)p1_y, (float)p2_x, (float)p2_y);
        }




    }
}





