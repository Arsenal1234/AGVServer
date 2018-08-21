using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph
{    
    /// <summary>
    /// 加权有向边的数据类型
    /// </summary>
    public class DirectedEdge
    {
        public int ID { get; private set; }
        public int V { get; private set; }
        public int W { get; private set; }
        public double Weight { get; private set; }


        public DirectedEdge(int v, int w, double weight)
        {
            this.V = v;
            this.W = w;
            this.Weight = weight;
        }


        public DirectedEdge(Position pos1, Position pos2)
        {
            V = pos1.PositionID;
            W = pos2.PositionID;
            Weight = pos1.DistanceToPoint(pos2);
        }

        public double GetWeight()
        {
            return Weight;
        }

        /// <summary>
        /// 指出这条边的顶点
        /// </summary>
        /// <returns></returns>
        public int from()
        {
            return V;
        }

        public int to()
        {
            return W;
        }

        public string toString()
        {
            return V + "->" + W + " " + Weight.ToString("f2");
        }


    }
}
