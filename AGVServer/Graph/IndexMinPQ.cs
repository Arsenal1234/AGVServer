using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorting
{

    /// <summary>
    /// 索引优先队列
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    public class IndexMinPQ<Key> where Key : IComparable<Key>
    {
        private int N;//PQ中的元素数量
        private int[] pq;//索引二叉堆，由1开始
        private int[] qp;//逆序：qp[pq[i]]=pq[qp[i]]=i
        private Key[] keys;//有优先级之分的元素

        public IndexMinPQ(int maxN)
        {
            keys = new Key[maxN + 1];
            pq = new int[maxN + 1];
            qp = new int[maxN + 1];
            for (int i = 0; i <= maxN; i++) qp[i] = -1;
        }

        public void insert(int k, Key key)
        {
            N++;
            pq[N] = k;
            qp[k] = N;
            keys[k] = key;
            swim(N);
        }

        public void change(int k, Key key)
        {
            keys[k] = key;
            swim(qp[k]);
            sink(qp[k]);
        }

        public bool contains(int k)
        {
            return qp[k] != -1;
        }

        /// <summary>
        /// 删去索引k及其相关联的元素
        /// </summary>
        /// <param name="k"></param>
        public void delete(int k)
        {
            int index = qp[k];
            exch(index, N--);
            swim(index);
            sink(index);
            keys[k] = default(Key);
            qp[k] = -1;
        }

        public Key min()
        {
            return keys[pq[1]];
        }

        public int minIndex()
        {
            return pq[1];
        }

        public int delMin()
        {
            int indexOfMin = pq[1];
            exch(1, N--);
            sink(1);
            keys[pq[N + 1]] = default(Key);
            qp[pq[N + 1]] = -1;
            return indexOfMin;
        }

        public bool isEmpty()
        {
            return N == 0;

        }

        /// <summary>
        /// 第j个元素是否小于第i个元素
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        private bool less(int i, int j)
        {
            return pq[i].CompareTo(pq[j]) > 0;
        }

        /// <summary>
        /// 交换第i和第j的元素
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void exch(int i, int j)
        {
            int t = pq[i];
            pq[i] = pq[j];
            pq[j] = t;
        }




        /// <summary>
        /// 上浮pq中第k个元素
        /// </summary>
        /// <param name="k"></param>
        private void swim(int k)
        {
            while (k > 1 && less(k / 2, k))
            {
                exch(k / 2, k);
                k = k / 2;
            }

        }

        /// <summary>
        /// 下沉pq中第k个元素
        /// </summary>
        /// <param name="k"></param>
        private void sink(int k)
        {
            while (2 * k <= N)
            {
                int j = 2 * k;
                if (j < N && less(j, j + 1))
                {
                    j++;
                }
                if (!less(k, j)) break;
                exch(k, j);
                k = j;
            }
        }


    }

   
   
    
}
