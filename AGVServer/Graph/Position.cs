using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graph
{
    /// <summary>
    /// 坐标（点）的数据结构
    /// </summary>
    public class Position
    {
        /// <summary>
        /// 点的横坐标值，单位（mm）
        /// </summary>
        public double x { get; private set; }

        /// <summary>
        /// 点的纵坐标值，单位（mm）
        /// </summary>
        public double y { get; private set; }

        /// <summary>
        /// 排序索引
        /// </summary>
        public int PositionID { get; private set; }


        public Position(double x, double y)
        {
            this.x = x;
            this.y = y;
        }


        public Position(int id,double x, double y)
        {
            this.PositionID = id;
            this.x = x;
            this.y = y;
        }

        

        public double DistanceToPoint(Position pos){
            double x1=this.x;
            double y1=this.y;
            double x2 = pos.x;
            double y2 = pos.y;
            return Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
        }



    }
}
