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
        private int v;
        private int w;
        private double weight;

        public int ID { get; set; }
        public int V { get; set; }
        public int W { get; set; }
        public double Weight { get; set; }


        public DirectedEdge(int v, int w, double weight)
        {
            this.v = v;
            this.w = w;
            this.weight = weight;
        }

        public DirectedEdge(Position pos1, Position pos2)
        {
            v = pos1.PositionID;
            w = pos2.PositionID;
            weight = pos1.DistanceToPoint(pos2);
        }

        public double GetWeight()
        {
            return weight;
        }

        /// <summary>
        /// 指出这条边的顶点
        /// </summary>
        /// <returns></returns>
        public int from()
        {
            return v;
        }

        public int to()
        {
            return w;
        }

        public string toString()
        {
            return string.Format("%d-%d %.2f", v, w, weight);
        }


    }
}
