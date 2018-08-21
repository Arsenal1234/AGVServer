using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVServer.Model
{
    public class SocketCommand
    {
        public string Speed { get; set; }
        public string Forward { get; set; }      //前进
        public string Back { get; set; }         //后退
        public string Turn { get; set; }//       逆向旋转
        public string Oblique { get; set; }      //正向运动
        public string SForward { get; set; }     //等待
        public string RightShift { get; set; }//右平移
        public string LeftShift { get; set; }//左平移
        public string ObliqueLU { get; set; } //左上斜线
        public string ObliqueLD { get; set; }//左下斜线
        public string ObliqueRU { get; set; }//右上斜线
        public string ObliqueRD { get; set; }//右下斜线
        public string Angle { get; set; }//角度

        public string CommandNum { get; set; }//命令号

        public string FLShift { get; set; }//前左转
        public string FRShift { get; set; }//前右转
        public string BLShift { get; set; }//后左转
        public string BRShift { get; set; }//后右转
    }
}
