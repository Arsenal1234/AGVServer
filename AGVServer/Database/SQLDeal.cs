using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Graph;

namespace AGVClient.Database
{
    public class SQLDeal
    {
        private string conStr = "server=DESKTOP-TLT9BB9;database=AGV;uid=sa;password=1122;";
        //string conStr = "server=SKY-20180329XOQ;uid=sa;password=123456;Persist Security Info=True;Initial Catalog=employees;database=AGV;Data Source=192.168.0.198";
        private SqlConnection conn;
        private SqlCommand cmd;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="conInfo">连接信息，包括server;database;uid;code</param>
        public SQLDeal()
        {
            conn = new SqlConnection(conStr);
            cmd = null;
        }
        //public SQLDeal(string conInfo)
        //{
        //    this.conStr = conInfo;
        //    conn = new SqlConnection(conInfo);
        //    cmd = null;
        //}
        

        /// <summary>
        /// 创建数据库中的表
        /// </summary>
        public void CreateTablesOfAGV()
        {           
            //建立PositionOfV表
            string sqlStr = "create table AGV.dbo.PositionOfV(ID_P int not null primary key,x decimal(18,4) not null,y decimal(18,4) not null);";
            executeSQLCommand(sqlStr);
            //建立PositionOfW表
            sqlStr = "create table AGV.dbo.PositionOfW(ID_P int not null primary key,x decimal(18,4) not null,y decimal(18,4) not null);";
            executeSQLCommand(sqlStr);
            //建立DirectedEdge表
            sqlStr = "create table DirectedEdge(ID_E int not null primary key identity,v int not null,w int not null,weight decimal(18,4) not null)";
            executeSQLCommand(sqlStr);
            //增加外键
            sqlStr = "alter table AGV.dbo.DirectedEdge add CONSTRAINT fk_v FOREIGN KEY (v) REFERENCES AGV.dbo.PositionOfV(ID_P) ON DELETE CASCADE on update cascade;" +
                    "alter table AGV.dbo.DirectedEdge add CONSTRAINT fk_w FOREIGN KEY (w)REFERENCES AGV.dbo.PositionOfW(ID_P) ON DELETE CASCADE on update cascade;";
            executeSQLCommand(sqlStr);
         }


        /// <summary>
        /// 尝试打开和关闭连接
        /// </summary>
        public void TestConnection()
        {
            conn.Open();
            conn.Close();
        }

//-------------------------------------------增-------------------------------------------------------------------------

        /// <summary>
        /// 添加一个坐标点，不可插入坐标相同的点
        /// </summary>
        /// <param name="p">添加成功返回1，否则0</param>
        public int AddPosition(Position p)
        {
            List<Position> positions = SelectPositions();
            foreach (Position p_i in positions)
            {
                if (p_i.x == p.x && p_i.y == p.y)
                    return 0;
            }
            string sqlStr = "insert into AGV.dbo.PositionOfV values (" + p.PositionID + "," + p.x + "," + p.y + ")" +
                             "insert into AGV.dbo.PositionOfW values (" + p.PositionID + "," + p.x + "," + p.y + ")";
            int changRank = executeSQLCommand(sqlStr);
            if (changRank == 0)
                return 0;
            else
                return 1;
        }

        /// <summary>
        /// 添加一条边到DirectedEdge表中，不可插入重复的边
        /// </summary>
        /// <param name="e">要添加的边</param>
        public int AddEdge(DirectedEdge e)
        {
            List<DirectedEdge> edges = SelectEdges();
            foreach (DirectedEdge e_i in edges)
            {
                if (e_i.V == e.V && e_i.W == e.W)
                    return 0;
            }
            string sqlStr = "insert into DirectedEdge values (" + e.V + "," + e.W + "," + e.Weight + ")";
            int changRank = executeSQLCommand(sqlStr);
            if (changRank == 0)
                Trace.WriteLine("插入数据失败");
            return changRank;
        }


//------------------------------------删----------------------------------------------------------

        /// <summary>
        /// 删除一个坐标点
        /// </summary>
        /// <param name="p">要删除的点</param>
        public void DeletePosition(Position p)
        {
            string sqlStr = "delete from AGV.dbo.PositionOfV where ID_P=" + p.PositionID +
                             "delete from AGV.dbo.PositionOfW where ID_P=" + p.PositionID;
            int changRank = executeSQLCommand(sqlStr);
            if (changRank == 0)
                Trace.WriteLine("删除数据失败，可能不存在此条数据");
        }

        /// <summary>
        /// 删除一条边
        /// </summary>
        /// <param name="e">要删除的边</param>
        public void DeleteEdge(DirectedEdge e)
        {
            string sqlStr = "delete from AGV.dbo.DirectedEdge where ID_E=" + e.ID;
            int changRank = executeSQLCommand(sqlStr);
            if (changRank == 0)
                Trace.WriteLine("删除数据失败，可能不存在此条数据");
        }

        /// <summary>
        /// 删除次数据库中所有表
        /// </summary>
        public void DeleteAllTables()
        {
            string sqlStr = "drop table AGV.dbo.DirectedEdge;drop table AGV.dbo.PositionOfV;drop table AGV.dbo.PositionOfW;";
            executeSQLCommand(sqlStr);            
        }

//-----------------------------------------------改-----------------------------------------------------------


//-----------------------------------------------查----------------------------------------------------------------


        /// <summary>
        /// 返回所有点的list集合
        /// </summary>
        /// <returns>坐标集合</returns>
        public List<Position> SelectPositions()
        {
            List<Position> positions = new List<Position>();
            string sqlStr = "select * from dbo.PositionOfV";
            cmd = new SqlCommand(sqlStr, conn);//创建Command对象
            SqlDataReader dr;//创建DataReader对象
            try
            {
                conn.Open();//打开数据库                                
                dr = cmd.ExecuteReader();//执行查询
                while (dr.Read())//判断数据表中是否含有数据
                {
                    Position p = new Position(Convert.ToInt32(dr[0]), Convert.ToDouble(dr[1]), Convert.ToDouble(dr[2]));
                    positions.Add(p);
                    Debug.WriteLine(dr[0].ToString() + " " + dr[1].ToString() + " " + dr[2].ToString());
                }
                dr.Close();//关闭DataReader对象
            }
            catch (Exception ex)//创建检查Exception对象
            {
                Debug.WriteLine(ex);//输出错误信息
            }
            finally
            {
                conn.Close();//关闭连接
            }
            return positions;
        }


        /// <summary>
        /// 查找指定ID坐标信息
        /// </summary>
        /// <param name="sqlStr">id号</param>
        /// <returns>坐标</returns>
        public Position SelectPosition(int id)
        {
            Position p = null;
            string sqlStr = "select * from dbo.PositionOfV where ID_P=" + id;
            cmd = new SqlCommand(sqlStr, conn);//创建Command对象
            SqlDataReader dr;//创建DataReader对象
            try
            {
                conn.Open();//打开数据库                                
                dr = cmd.ExecuteReader();//执行查询
                while (dr.Read())//判断数据表中是否含有数据
                {
                    p = new Position(Convert.ToInt32(dr[0]), Convert.ToDouble(dr[1]), Convert.ToDouble(dr[2]));
                    Debug.WriteLine(dr[0].ToString() + " " + dr[1].ToString() + " " + dr[2].ToString());
                    break;
                }
                dr.Close();//关闭DataReader对象
            }
            catch (Exception ex)//创建检查Exception对象
            {
                Debug.WriteLine(ex);//输出错误信息
            }
            finally
            {
                conn.Close();//关闭连接
            }
            return p;
        }

        /// <summary>
        /// 查找并返回所有边
        /// </summary>
        /// <returns>边的list集合</returns>
        public List<DirectedEdge> SelectEdges()
        {
            List<DirectedEdge> directedEdges = new List<DirectedEdge>();
            string sqlStr = "select * from DirectedEdge";
            cmd = new SqlCommand(sqlStr, conn);//创建Command对象
            SqlDataReader dr;//创建DataReader对象
            try
            {
                conn.Open();//打开数据库                                
                dr = cmd.ExecuteReader();//执行查询
                while (dr.Read())//判断数据表中是否含有数据
                {
                    DirectedEdge edge = new DirectedEdge(Convert.ToInt32(dr[1]), Convert.ToInt32(dr[2]),Convert.ToDouble(dr[3]));
                    directedEdges.Add(edge);
                    Debug.WriteLine(dr[0].ToString() + " " + dr[1].ToString() + " " + dr[2].ToString() + " " + dr[3].ToString());
                }
                dr.Close();//关闭DataReader对象
            }
            catch (Exception ex)//创建检查Exception对象
            {
                Debug.WriteLine(ex);//输出错误信息
            }
            finally
            {
                conn.Close();//关闭连接
            }
            return directedEdges;
        }


        /// <summary>
        /// 查询最大ID
        /// </summary>
        /// <returns></returns>
        public int maxPositionID()
        {
            int numberID = 0;
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            string sqlStr = "select MAX(ID_P) from AGV.dbo.PositionOfV;";
            cmd = new SqlCommand(sqlStr, conn);
            SqlDataReader dr;
            try
            {
                conn.Open();
                dr = cmd.ExecuteReader();
                dr.Read();
                numberID = (int)dr[0];
                dr.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                conn.Close();
            }
            return numberID;
        }



        /// <summary>
        /// 执行输入的sql语句
        /// </summary>
        /// <param name="sqlStr">sql语句</param>
        /// <returns>表中被更改的行数</returns>
        private int executeSQLCommand(string sqlStr)
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
            int changeRank = 0;
            try
            {               
                cmd = new SqlCommand(sqlStr, conn);
                conn.Open();
                changeRank = cmd.ExecuteNonQuery();
                conn.Close();               
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex);
            }

            return changeRank;
        }



        







    }
}
