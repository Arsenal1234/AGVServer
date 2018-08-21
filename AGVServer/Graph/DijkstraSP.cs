using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sorting;

namespace Graph
{
    /// <summary>
    /// 最短路径的Dijkstra算法
    /// </summary>
    public class DijkstraSP
    {
        private DirectedEdge[] edgeTo;//已知大小用数组
        private double[] distTo;
        private IndexMinPQ<Double> pq;

        /// <summary>
        /// 构造函数: 创建最短路径树并计算最短路径的长度
        /// </summary>
        /// <param name="G"></param>
        /// <param name="s"></param>
        public DijkstraSP(EdgeWeightedDigraph G, int s)
        {
            edgeTo = new DirectedEdge[G.GetV()];
            distTo = new double[G.GetV()];
            pq = new IndexMinPQ<double>(G.GetV());
            for (int v = 0; v < G.GetV(); v++)
            {
                distTo[v] = Double.MaxValue;
            }
            distTo[s] = 0.0;

            pq.insert(s, 0.0);
            while (!pq.isEmpty())
            {
                relax(G, pq.delMin());
            }
        }

        /// <summary>
        /// 从顶点s到v的距离，如果不存在则路径为无穷大
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double getDistTo(int v)
        {
            return distTo[v];
        }

        /// <summary>
        /// 是否存在从s到v的路径
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool hasPathTo(int v)
        {
            return distTo[v] < Double.MaxValue;
        }

        /// <summary>
        /// 从顶点s到v的路径，如果不存在则为null
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Stack<DirectedEdge> pathTo(int v)
        {
            if (!hasPathTo(v)) return null;
            Stack<DirectedEdge> path = new Stack<DirectedEdge>();
            for (DirectedEdge e = edgeTo[v]; e != null; e = edgeTo[e.from()])
            {
                path.Push(e);
            }
            return path;
        }

        /// <summary>
        /// 边e的松弛
        /// </summary>
        /// <param name="e"></param>
        private void relax(DirectedEdge e)
        {
            int v = e.from();
            int w = e.to();
            if (distTo[w] > distTo[v] + e.GetWeight())
            {
                distTo[w] = distTo[v] + e.GetWeight();
                edgeTo[w] = e;
            }
        }

        /// <summary>
        /// 顶点v的松弛
        /// </summary>
        /// <param name="G"></param>
        /// <param name="v"></param>
        private void relax(EdgeWeightedDigraph G, int v)
        {
            foreach (DirectedEdge e in G.GetAdj(v))
            //relax(e);
            {
                int w = e.to();
                if (distTo[w] > distTo[v] + e.GetWeight())
                {
                    distTo[w] = distTo[v] + e.GetWeight();
                    edgeTo[w] = e;
                    if (pq.contains(w)) pq.change(w, distTo[w]);
                    else pq.insert(w, distTo[w]);

                }
            }//<=>relax(e)
        }

    }

}
