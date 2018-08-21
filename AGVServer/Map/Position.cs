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
        private double x;
        public double X { get; set; }

        /// <summary>
        /// 点的纵坐标值，单位（mm）
        /// </summary>
        private double y;
        public double Y { get; set; }

        /// <summary>
        /// 排序索引
        /// </summary>
        private int positionID;
        private object p1;
        private object p2;
        private object p3;
        public int PositionID { get; set; }
        /// <summary>
        /// 2表示噪点；1表示断点；0表示角点
        /// </summary>
        public int repeatNumber { get; set; }
        

        public Position()
        {
            this.x = 0;
            this.y = 0;
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
            double x2 = pos.X;
            double y2 = pos.Y;
            return Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
        }



    }
}
