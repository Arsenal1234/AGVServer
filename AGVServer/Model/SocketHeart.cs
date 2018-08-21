using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVServer.Model
{
    public class SocketHeart
    {
        public SocketHeart()
        {
            LastConnectTime = DateTime.Now;
        }
        public string SocketName { get; set; }
        public DateTime LastConnectTime { get; set; }    //最后反馈的时间
    }
}
