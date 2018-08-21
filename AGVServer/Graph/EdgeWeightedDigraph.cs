using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Graph
{
    /// <summary>
    /// 加权有向图的数据类型
    /// </summary>
    public class EdgeWeightedDigraph
    {
        private int V;
        private int E;
        private List<DirectedEdge>[] adj;

        /// <summary>
        /// 创建一幅含有V个顶点的空有向图
        /// </summary>
        /// <param name="V"></param>
        public EdgeWeightedDigraph(int V)
        {
            this.V = V;
            adj = new List<DirectedEdge>[V];
            for (int v = 0; v < V; v++)
            {
                adj[v] = new List<DirectedEdge>();
            }
        }

        public EdgeWeightedDigraph(string fileName)
        {
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                this.V = Convert.ToInt32(sr.ReadLine());
                this.E = 0;
                adj = new List<DirectedEdge>[V];//创建邻接表
                for (int v = 0; v < V; v++)
                {
                    adj[v] = new List<DirectedEdge>();
                }
                int E = Convert.ToInt32(sr.ReadLine());
                for (int i = 0; i < E; i++)
                {
                    string[] data = sr.ReadLine().Split(' ');
                    int v = Convert.ToInt32(data[0]);
                    int w = Convert.ToInt32(data[1]);
                    double weight = Convert.ToDouble(data[2]);
                    DirectedEdge e = new DirectedEdge(v, w, weight);
                    AddEdge(e);
                }
            }
            catch (IOException e)
            {
                Debug.WriteLine(e);
            }
        }

        public int GetV()
        {
            return V;
        }

        public int GetE()
        {
            return E;
        }

        public void AddEdge(DirectedEdge e)
        {
            adj[e.from()].Add(e);
            E++;
        }

        public List<DirectedEdge> GetAdj(int v)
        {
            return adj[v];
        }

        /// <summary>
        /// 返回图的所有边
        /// </summary>
        /// <returns></returns>
        public List<DirectedEdge> GetEdges()
        {
            List<DirectedEdge> list = new List<DirectedEdge>();
            for (int v = 0; v < V; v++)
            {
                foreach (DirectedEdge e in adj[v])
                    list.Add(e);
            }
            return list;
        }

        public string toString()
        {
            return null;
        }
    }

}
