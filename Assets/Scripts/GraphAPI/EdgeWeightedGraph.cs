using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphAPI
{
    /// <summary>
    /// Minimum Spanning Tree
    /// </summary>
    public class EdgeWeightedGraph
    {
        public int V_total { get; private set; }                    //number of Vertices
        public int E_total { get; private set; }                    //number of edges
        private List<List<Edge>> listOfAdj;                         //list of Edge lists (adjacency lists)

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="numVertices"></param>
        public EdgeWeightedGraph(int numVertices)
        {
            V_total = numVertices;
            E_total = 0;
            E_total = 0;
            listOfAdj = new List<List<Edge>>();
            for (int v = 0; v < V_total; v++)
            {
                listOfAdj.Add(new List<Edge>());
            }
        }

        /// <summary>
        /// add an edge to the list of Adjacencies
        /// </summary>
        /// <param name="edge"></param>
        public void AddEdge(Edge edge)
        {
            int v = edge.GetEither();
            //int w = edge.GetOther(v);
            //add edge to two lists, one for each node end
            listOfAdj[v].Add(edge);
            //listOfAdj[w].Add(edge);
            //increment total number of edges
            E_total++;
        }

        /// <summary>
        /// Get a nodes list of Adjacencies
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public List<Edge> GetAdjList(int v)
        { return listOfAdj[v]; }


        /// <summary>
        /// return a list of All edges, ignoring any self loops
        /// </summary>
        /// <returns></returns>
        public List<Edge> GetAllEdges()
        {
            List<Edge> tempListOfEdges = new List<Edge>();
            for (int v = 0; v < V_total; v++)
            {
                foreach(Edge edge in listOfAdj[v])
                {
                    if (edge.GetOther(v) > v)
                    { tempListOfEdges.Add(edge); }
                }
            }
            return tempListOfEdges;
        }

    }

}
