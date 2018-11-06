using System;
using System.Collections.Generic;


namespace dijkstraAPI
{
    /// <summary>
    /// Node class for Dijkstra algo
    /// </summary>
    public class NodeD
    {
        int distance, id;
        string name;
        List<int> weights;
        List<NodeD> adjacency;

        public NodeD(int distance, int id, string name)
        {
            this.distance = distance;
            this.id = id;
            this.name = name;
            weights = new List<int>();
            adjacency = new List<NodeD>();
        }

        public int Distance
        {
            get
            {
                return distance;
            }
            set
            {
                distance = value;
            }
        }

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public List<NodeD> Adjacency
        {
            get
            {
                return adjacency;
            }
            set
            {
                adjacency = value;
            }
        }

        public List<int> Weights
        {
            get
            {
                return weights;
            }
            set
            {
                weights = value;
            }
        }
    }

}


