using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphAPI
{

    public class Search
    {
        private Graph graph;
        public int Count { get; set; }    //how many vertices are connected to s?
        private bool[] arrayOfMarked;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="g"></param>
        public Search(Graph g, int s)
        {
            graph = g;
            arrayOfMarked = new bool[graph.Vertices];
            Count = 0;
            DepthFirstSearch(s);
        }

        /// <summary>
        /// vertice is marked?
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private bool IsMarked(int w)
        {
            return arrayOfMarked[w];
        }

        /// <summary>
        /// recursive DFS algorithm
        /// </summary>
        /// <param name="v"></param>
        private void DepthFirstSearch(int v)
        {
            arrayOfMarked[v] = true;
            Count++;
            List<int> listTemp = graph.GetAdjacencies(v);
            for (int w = 0; w < listTemp.Count; w++)
            {
                if (IsMarked(listTemp[w]) == false)
                {
                    DepthFirstSearch(listTemp[w]);
                }
            }
        }

        /// <summary>
        /// return array of marked nodes for processing by Unity related methods
        /// </summary>
        /// <returns></returns>
        public bool[] GetMarkedArray()
        { return arrayOfMarked; }

    }
}
