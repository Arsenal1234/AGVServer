using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVServer.Model
{
    /// <summary>
    /// 每条指令中包含的信息
    /// </summary>
    public class OneCommand
    {

        /// <summary>
        /// 命令序号
        /// </summary>
        public string CommandNum { get; set; }


        /// <summary>
        /// 命令名字
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// 关联的小车中的移动方式
        /// </summary>
        public List<Command> command { get; set; }
 

        /// <summary>
        /// 是否为根结点
        /// </summary>
        public bool BoolFather { get; set; }


        /// <summary>
        /// 命令根结点
        /// </summary>
        public string FatherPoint { get; set; }


        /// <summary>
        /// 完成度
        /// </summary>
        public string Finish { get; set; }


        /// <summary>
        /// 等待时间
        /// </summary>
        public string WaitTime { get; set; }

        /// <summary>
        /// 等待车号
        /// </summary>
        public string WaitCar { get; set; }
   
    }
}
