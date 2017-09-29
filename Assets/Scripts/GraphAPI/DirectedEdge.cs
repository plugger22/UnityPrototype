using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphAPI
{

    /// <summary>
    /// Used for Shortest Path calc's
    /// </summary>
    public class DirectedEdge
    {

        private int v;              //edge source
        private int w;              //edge target
        public float Weight { get; private set; }      //edge weight

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <param name="weight"></param>
        public DirectedEdge(int v, int w, float weight)
        {
            this.v = v;
            this.w = w;
            Weight = weight;
        }

        public int From()
        { return v; }

        public int To()
        { return w; }

        public string GetDirectedEdgeString()
        { return string.Format("{0} -> {1], weight {2:F3}", v, w, Weight); }

    }

}
