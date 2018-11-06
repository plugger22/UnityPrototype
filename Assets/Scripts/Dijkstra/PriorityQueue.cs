using System.Collections;
using System.Collections.Generic;


namespace dijkstraAPI
{

    public class PriorityQueue
    {
        int heapSize;
        List<NodeD> nodeList;

        public List<NodeD> NodeList
        {
            get
            {
                return nodeList;
            }
        }

        public PriorityQueue(List<NodeD> nl)
        {
            heapSize = nl.Count;
            nodeList = new List<NodeD>();

            for (int i = 0; i < nl.Count; i++)
                nodeList.Add(nl[i]);
        }

        public void exchange(int i, int j)
        {
            NodeD temp = nodeList[i];

            nodeList[i] = nodeList[j];
            nodeList[j] = temp;
        }

        public void heapify(int i)
        {
            int l = 2 * i + 1;
            int r = 2 * i + 2;
            int largest = -1;

            if (l < heapSize && nodeList[l].Distance > nodeList[i].Distance)
                largest = l;
            else
                largest = i;
            if (r < heapSize && nodeList[r].Distance > nodeList[largest].Distance)
                largest = r;
            if (largest != i)
            {
                exchange(i, largest);
                heapify(largest);
            }
        }

        public void buildHeap()
        {
            for (int i = heapSize / 2; i >= 0; i--)
                heapify(i);
        }

        int heapSearch(NodeD node)
        {
            for (int i = 0; i < heapSize; i++)
            {
                NodeD aNode = nodeList[i];

                if (node.ID == aNode.ID)
                    return i;
            }

            return -1;
        }

        public int size()
        {
            return heapSize;
        }

        public NodeD elementAt(int i)
        {
            return nodeList[i];
        }

        public void heapSort()
        {
            int temp = heapSize;

            buildHeap();

            for (int i = heapSize - 1; i >= 1; i--)
            {
                exchange(0, i);
                heapSize--;
                heapify(0);
            }

            heapSize = temp;
        }

        public NodeD extractMin()
        {
            if (heapSize < 1)
                return null;

            heapSort();

            exchange(0, heapSize - 1);
            heapSize--;
            return nodeList[heapSize];
        }

        public int find(NodeD node)
        {
            return heapSearch(node);
        }
    }

}
