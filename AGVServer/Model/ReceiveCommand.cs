using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVServer.Model
{
    public class ReceiveCommand
    {
        /// <summary>
        /// 序号
        /// </summary>
        public string ReceiveNum { get; set; }

        /// <summary>
        /// 接收的命令文本
        /// </summary>
        public string ReceiveText { get; set; }


        /// <summary>
        /// 接收的命令类型
        /// </summary>
        public string ReceiveType { get; set; }

        /// <summary>
        /// 命令序号
        /// </summary>
        public string CommandNum { get; set; }
    }
}
