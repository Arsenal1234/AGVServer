using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVServer.Model
{
    public class LoadCommand
    {
  //      public string carName { get; set; }
        public string commandName { get; set; }

        public List<SocketCommand> socketCommand { get; set; }
    }
}
