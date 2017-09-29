using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GraphAPI
{

    /// <summary>
    /// Used for shortest path calculations -> Graph / EdgeWeightedGraph equivalent
    /// </summary>
    public class EdgeWeightedDigraph
    {
        private int V_total;                                    //number of Vertices
        private int E_total;                                    //number of edges
        private List<List<DirectedEdge>> listOfAdjacencies;     //adjacency lists

        public int Vertices { get { return V_total; } set { V_total = value; } }
        public int Edges { get { return E_total; } set { E_total = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="numVertices"></param>
        public EdgeWeightedDigraph(int numVertices)
        {
            V_total = numVertices;
            E_total = 0;
            listOfAdjacencies = new List<List<DirectedEdge>>();
            //initialise Lists of Adjacencies
            for (int v = 0; v < numVertices; v++)
            {
                List<DirectedEdge> tempList = new List<DirectedEdge>();
                listOfAdjacencies.Add(tempList);
            }
        }

        /// <summary>
        /// Add an edge to the adjacencies list
        /// </summary>
        /// <param name="edge"></param>
        public void AddEdge(DirectedEdge edge)
        {
            listOfAdjacencies[edge.From()].Add(edge);
            E_total++;
        }


        public List<DirectedEdge> GetAdjacency(int v)
        {
            return listOfAdjacencies[v];
        }


        public List<DirectedEdge> GetEdges()
        {
            List<DirectedEdge> tempList = new List<DirectedEdge>();
            for (int v = 0; v < V_total; v++)
            {
                foreach (DirectedEdge edge in listOfAdjacencies[v])
                { tempList.Add(edge); }
            }
            return tempList;
        }

    }

}
