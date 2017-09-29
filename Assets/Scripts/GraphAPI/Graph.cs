using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GraphAPI
{
    /// <summary>
    /// Provides graph analysis & pathing functionality
    /// </summary>
    public class Graph
    {
        private int V_total;                        //number of vertices
        private int E_total;                        //number of edges
        private List<List<int>> listOfAdj;          //list of adjacency lists

        public int Vertices { get { return V_total; } }
        public int Edges { get { return E_total; } }

        

        /// <summary>
        /// default constructor (called manually)
        /// </summary>
        /// <param name="totalVertices"></param>
        public Graph(int totalVertices)
        {
            V_total = totalVertices;
            E_total = 0;
            listOfAdj = new List<List<int>>();
            //initialise all lists to empty
            for (int v = 0; v < V_total; v++)
            { listOfAdj.Add(new List<int>()); }
        }

        /// <summary>
        /// Add an edge an update adjacency lists
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        public void AddEdge(int v, int w)
        {
            listOfAdj[v].Add(w);
            listOfAdj[w].Add(v);
            E_total++;
            //Debug.Log("Edge added  " + v + " - " + w);
        }

        /// <summary>
        /// Get a list of Adjacencies for a Vertice
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public List<int> GetAdjacencies(int v)
        { return listOfAdj[v]; }

        //
        // - - - Analysis methods - - -
        //

        public int CalcDegree(int v)
        {
            int degree = 0;
            degree += listOfAdj[v].Count;
            return degree;
        }

        public int CalcMaxDegree()
        {
            int max = 0;
            for (int v = 0; v < V_total; v++)
            {
                if (CalcDegree(v) > max)
                { max = CalcDegree(v); }
            }
            return max;
        }

        public int CalcAvgDegree()
        {
            return 2 * E_total / V_total;
        }

        public int CalcSelfLoops()
        {
            int count = 0;
            for (int v = 0; v < V_total; v++)
            {
                for (int w = 0; w < listOfAdj[v].Count; w++)
                {
                    if (listOfAdj[v][w] == v)
                    { count++; }
                }
            }
            return count / 2; //each edge counted twice
        }
               
    }

    //classes above here
}
