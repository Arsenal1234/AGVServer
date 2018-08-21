using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVServer.Model
{
   public  class ReceiveIn
    {
       public string percent { get; set; }

       public string FX { get; set; }

       public string speed { get; set; }

       public string ines { get; set; }

       public byte[] alarm { get; set; }


       public byte[] wdAlarm { get; set; }

       public string CommandNum { get; set; }
    }
}
