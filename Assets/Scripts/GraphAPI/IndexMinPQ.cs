using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphAPI
{
    /// <summary>
    /// Indexed Minimum Priority Queue -> NOTE: Doubtful whether this would work. Untested and some dodgy workarounds
    /// </summary>
    public class IndexMinPQ
    {
        private double[] pq;         //heap ordered complete binary tree
        private int N = 0;      //in pq[1...N] with pq[0] unused

        public void MaxPQ(int maxN)
        { pq = new double[maxN + 1]; }

        public bool CheckIsEmpty()
        { return N == 0; }

        public int Size()
        { return N; }

        public void Insert(double v)
        {
            pq[++N] = v;
            Swim(N);
        }

        public double delMax()
        {
            //retrieve max key from top
            double max = pq[1];
            //exchange with last Item
            Exchange(1, N--);
            //Avoid loitering
            pq[N + 1] = 0;
            //Restore heap property
            Sink(1);
            return max;
        }

        private bool Less(int i, int j)
        {
            if (pq[i] < pq[j]) { return true; }
            return false; 
        }

        private void Exchange(int i, int j)
        {
            double t = pq[i];
            pq[i] = pq[j];
            pq[j] = t;
        }

        private void Swim(int k)
        {
            while (k > 1 && Less(k/2, k))
            {
                Exchange(k / 2, k);
                k = k / 2;
            }
        }
        
        private void Sink(int k)
        {
            while (2 * k <= N)
            {
                int j = 2 * k;
                if (j < N && Less(j, j + 1)) { j++; }
                if (!Less(k, j)) { break; }
                Exchange(k, j);
                k = j;
            }
        }

    }
}

