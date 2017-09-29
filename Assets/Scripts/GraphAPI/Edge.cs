using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphAPI
{
    /// <summary>
    /// Minimum Spanning Tree
    /// </summary>
    public class Edge : IComparable
    {

        private float weight;
        private int v;              //nodeID
        private int w;              //nodeID

        /// <summary>
        /// default constructor that takes two nodeID's (both ends of the edge) and the weighting
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <param name="weight"></param>
        public Edge(int v, int w, float weight)
        {
            this.v = v;
            this.w = w;
            this.weight = weight;
        }

        /// <summary>
        /// Get one nodeID out of the two available
        /// </summary>
        /// <returns></returns>
        public int GetEither()
        { return v; }


        /// <summary>
        /// Get the other node, provided you already know the first
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public int GetOther(int vertex)
        {
            if (vertex == v) { return w; }
            else if (vertex == w) { return v; }
            else { Debug.LogError("Inconsistent Edge -> Invalid vertex " + "\"" + vertex + "\""); }
            return 0;
        }

        public float GetWeight()
        { return weight; }


        /// <summary>
        /// IComparable method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj == null) { return 1; }
            Edge otherEdge = obj as Edge;
            if (otherEdge != null)
            {
                if (this.GetWeight() < otherEdge.GetWeight()) { return -1; }
                else if (this.GetWeight() > otherEdge.GetWeight()) { return +1; }
                else return 0;
            }
            else { Debug.LogError("Object is not an Edge"); }
            return 0;
        }

        /// <summary>
        /// returns a string representation of the edge
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            return string.Format("Edge NodeID's {0} - {1}, weight {2:F2}", v, w, weight);
        }

    }

}
