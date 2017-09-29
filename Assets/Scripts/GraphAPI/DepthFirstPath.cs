using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GraphAPI
{

    /// <summary>
    /// Shows path from one node to another (but shows all relevant paths, not just the shortest one)
    /// </summary>
    public class DepthFirstPath
    {
        private bool[] arrayOfMarked;        //has DepthFirstSearch() been called for this vertex?
        private int[] arrayOfEdgeTo;         //last vertex on known path to this vertex
        private int source;                  //source vertex
        private Graph graph;

        public DepthFirstPath(Graph G, int s)
        {
            graph = G;
            arrayOfMarked = new bool[graph.Vertices];
            arrayOfEdgeTo = new int[graph.Vertices];
            source = s;
            DepthFirstSearch(source);
        }


        /// <summary>
        /// recursive DFS algorithm
        /// </summary>
        /// <param name="v"></param>
        private void DepthFirstSearch(int v)
        {
            arrayOfMarked[v] = true;
            List<int> listTemp = graph.GetAdjacencies(v);
            for (int w = 0; w < listTemp.Count; w++)
            {
                if (IsMarked(listTemp[w]) == false)
                {
                    arrayOfEdgeTo[listTemp[w]] = v;
                    DepthFirstSearch(listTemp[w]);
                }
            }
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
        /// returns true if there is a valid path from source vertex to indicated one
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool HasPathTo(int v)
        {
            return arrayOfMarked[v];
        }

        //returns a list showing all vertices in path
        public List<int> GetPathTo(int v)
        {
            if (HasPathTo(v) == false)
            { return null; }
            else
            {
                Stack<int> stackOfPath = new Stack<int>();
                for (int x = v; x != source; x = arrayOfEdgeTo[x])
                {
                    stackOfPath.Push(x);
                }
                stackOfPath.Push(source);
                return stackOfPath.ToList();
            }
        }
    }

}
