using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVServer.Model
{
     public class WaitSocket
    {
         /// <summary>
         /// 是否为等待时间
         /// </summary>
         public string Booltime {get; set;}

         /// <summary>
         /// 等待的指令
         /// </summary>
         public OneCommand OneCommand { get; set; }

         public string FatherPoint { get; set; }
         /// <summary>
         /// 等待的IP
         /// </summary>
         public string WaitSocetName { get; set; }

         public DateTime Time { get; set; }

         public TimeSpan Stime { get; set; }

         public string Finish { get; set; }
         public int  Num { get; set; }
    }
}
