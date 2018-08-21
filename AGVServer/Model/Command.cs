using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVServer.Model
{
    /// <summary>
    /// 小车走的移动方式
    /// </summary>
    public class Command
    {
        public string Carname { get; set; }
        /// <summary>
        /// 命令方向
        /// </summary>
        public string CommandFx { get; set; }

        /// <summary>
        /// 命令距离
        /// </summary>
        public string CommandRange { get; set; }

        /// <summary>
        /// 命令速度
        /// </summary>
        public string CommandSpeed { get; set; }

        public string Angle { get; set; }//角度
    }
}
