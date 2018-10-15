﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using GraphAPI;
using gameAPI;
using System.Text;

public enum NodeArcTally { Current, Required, Count };   //used for indexing of arrayOfNodeArcTotals

public class LevelManager : MonoBehaviour
{
    public GameObject node;             //node prefab
    public GameObject connection;       //connection prefab
    public LayerMask blockingLayer;     //nodes are on the blocking layer, not connections
    [Header("Default City Setup")]
    [Range(10, 30)] public int numOfNodesDefault = 20;              //number of nodes (adjusted after use in InitialiseNodes() to reflect actual number)
    [Range(1f, 3f)] public float minSpacingDefault = 1.5f;            //minimum spacing (world units) between nodes (>=)
    [Header("Connections")]
    [Tooltip("Random % chance of a node having additional connections")]
    [Range(0, 100)] public int connectionFrequencyDefault = 50;
    [Tooltip("Chance of a connection having a high security level (more than 'None')")]
    [Range(0, 100)] public int connectionSecurityDefault = 25;

    //actual values used (city values or default, if none)
    private int numOfNodes;
    private float minSpacing;
    private int connectionFrequency;
    private int connectionSecurity;

    private GameObject instance;
    private GameObject instanceConnection;
    private Transform nodeHolder;
    private Transform connectionHolder;
    private Node nodeStart;
    private Node nodeEnd;
    private Graph graph;                //used for analysis and pathing
    private EdgeWeightedGraph ewGraph;  //used to generate connection undirectional graph (with an MST foundation)

    private BoxCollider boxColliderStart;
    private BoxCollider boxColliderEnd;

    private List<NodeArc> listOfOneConnArcs = new List<NodeArc>();
    private List<NodeArc> listOfTwoConnArcs = new List<NodeArc>();
    private List<NodeArc> listOfThreeConnArcs = new List<NodeArc>();
    private List<NodeArc> listOfFourConnArcs = new List<NodeArc>();
    private List<NodeArc> listOfFiveConnArcs = new List<NodeArc>();

    //collections (all indexes correspond throughout, eg. listOfNodes[2] = listOfCoordinates[2] = listOfSortedDistances[2])
    //Note: collections initialised here as GameManager.SetUpScene will run prior to the Awake callback here
    private List<GameObject> listOfNodeObjects = new List<GameObject>();
    private List<Node> listOfNodes = new List<Node>();                              //mirror list to listOfNodeObjects but Nodes instead of GO's for speed of use
    private List<GameObject> listOfConnections = new List<GameObject>();
    private List<Vector3> listOfCoordinates = new List<Vector3>();                  //used to provide a lookup to check spacing of nodes
    private List<List<int>> listOfSortedNodes = new List<List<int>>();              //each node has a sorted (closest to furthest) list of nodeID's of neighouring nodes
    private List<List<float>> listOfSortedDistances = new List<List<float>>();    //companion list to listOfSortedNodes (identical indexes) -> contains distances to node in other list in world units    
     
    /*private List<List<GameObject>> listOfActorNodesAuthority = new List<List<GameObject>>();        //list containing sublists, one each of all the active nodes for each actor in the level
    private List<List<GameObject>> listOfActorNodesResistance = new List<List<GameObject>>();       //need a separate list for each side  */ 

    private int[,] arrayOfNodeArcTotals;            //array of how many of each node type there is on the map, index = node.Arc.nodeArcID
    /*private bool[,,] arrayOfActiveNodes;            //[total nodes, side, total actors] true if node active for that actor*/
    
    /// <summary>
    /// Master method that drives a level
    /// </summary>
    public void Initialise()
    {
        InitialiseData();
        InitialiseNodes(numOfNodes, minSpacing);
        InitialiseSortedDistances();
        RemoveInvalidNodes();
        InitialiseGraph();
        AssignNodeArcs();
        AssignSecurityLevels();
        /*InitialiseArrayOfActiveNodes();
        AssignActorsToNodes(GameManager.instance.globalScript.sideAuthority);
        AssignActorsToNodes(GameManager.instance.globalScript.sideResistance);*/
        EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Redraw, "LevelManager.cs -> Initialise");
    }


    //
    // --- Graph construction ---
    //
    #region InitialiseNodes
    /// <summary>
    /// place 'x' num nodes randomly within a 10 x 10 grid (world units) with a minimum spacing between them all
    /// </summary>
    /// <param name="number"></param>
    /// <param name="spacing"></param>
    private void InitialiseNodes(int number, float spacing)
    {
        int loopCtr;
        float distance;
        Vector3 randomPos;
        bool validPos;
        nodeHolder = new GameObject("MasterNode").transform;
        Node nodeTemp;

        //loop for number of nodes required (10 x 10 grid with coords ranging from -5 to +5)
        for (int i = 0; i < number; i++)
        {
            loopCtr = 0;
            //continue generating random positions until a valid one is found (max ten iterations then skip node if not successful)
            do
            {
                loopCtr++;
                validPos = true;
                //set up Vector3
                randomPos.x = Random.Range(0, 11) - 5;
                randomPos.y = 0.5f;
                randomPos.z = Random.Range(0, 11) - 5;

                if (listOfCoordinates.Count == 0)
                { /*Debug.Log("RandomPos: " + randomPos);*/ }
                else
                {
                    //loop list and check min spacing requirement
                    for (int j = 0; j < listOfCoordinates.Count; j++)
                    {
                        distance = Vector3.Distance(listOfCoordinates[j], randomPos);
                        
                        /*Debug.Log("list[j]: " + listOfCoordinates[j] + "  randomPos: " + randomPos);
                        Debug.Log("distance: " + distance);*/

                        //identical to an existing position?
                        if (listOfCoordinates[j] == randomPos) { validPos = false; break; }
                        //fails minimum spacing test
                        else if (distance <= spacing) { validPos = false;  break; }
                    }
                    //prevent endless iterations
                    if (loopCtr >= 20)
                    { break; }
                }
            }
            while (validPos == false);
            //successful node
            if (validPos == true)
            {
                //create node from prefab
                instance = Instantiate(node, randomPos, Quaternion.identity) as GameObject;
                instance.transform.SetParent(nodeHolder);
                //assign nodeID
                nodeTemp = instance.GetComponent<Node>();
                nodeTemp.nodeID = GameManager.instance.nodeScript.nodeCounter++;
                //add to node list & add to coord list for lookups
                listOfNodeObjects.Add(instance);
                listOfNodes.Add(nodeTemp);
                listOfCoordinates.Add(randomPos);
                //add to dictionary of Nodes
                GameManager.instance.dataScript.AddNodeObject(nodeTemp.nodeID, instance);
            }
        }
        //update Number of Nodes as there could be less than anticipated due to spacing requirements
        numOfNodes = listOfNodeObjects.Count;
    }
    #endregion
    #region InitialiseSortedDistances
    /// <summary>
    /// populates listOfSortedDistances (list of lists for each node of all other nodes sorted by distance, nearest to furthest)
    /// </summary>
    private void InitialiseSortedDistances()
    {
        float distance;
        Vector3 currentPos;
        //temp collections for sorting
        Dictionary<int, float> tempDict = new Dictionary<int, float>();     //key -> index, value -> distance
                                       //stores relevant index to listOfNodes / listOfCoordinates

        //loop list of Nodes
        for (int index = 0; index < listOfNodeObjects.Count; index++)
        {
            //create a duplicate list
            List<Vector3> tempList = new List<Vector3>(listOfCoordinates);
            currentPos = listOfCoordinates[index];
            //loop tempList, adding dictionary entries as you go
            for (int t = 0; t < tempList.Count; t++)
            {
                //add new dict entry, ignoring current index
                if (t != index)
                {
                    distance = Vector3.Distance(currentPos, tempList[t]);
                    //add to dictionary (key -> index, value -> distance)
                    tempDict.Add(t, distance);
                }
            }
            //sort dictionary by distance
            var sorted = from dist in tempDict
                         orderby dist.Value ascending
                         select dist;
            //add entries to listOfSortedDistances
            List<int> subListNodes = new List<int>();
            List<float> subListDistances = new List<float>();
            foreach (var pair in sorted)
            {
                //split data between the two list
                subListNodes.Add(pair.Key);
                subListDistances.Add(pair.Value);
            }
            //add sublists to main list 
            listOfSortedNodes.Add(subListNodes);
            listOfSortedDistances.Add(subListDistances);
            //Debug.Log("ListOfSortedDistance -> Index " + index + " added " + subList.Count + " entries -> " + subList[0] + ", " + subList[1] + ", " + subList[2] + ", " + subList[3] + ", " + subList[4]);
            //clear collections
            tempDict.Clear();
        }
    }
    #endregion
    #region CheckValidConnection
    /// <summary>
    /// Checks connection for validity based on Collisions and duplicate nodes. Returns true if all O.K
    /// </summary>
    /// <param name="indexStart"></param>
    /// <param name="indexEnd"></param>
    /// <param name="posStart"></param>
    /// <param name="posEnd"></param>
    /// <returns></returns>
    private bool CheckValidConnection(int indexStart, int indexEnd, Vector3 posStart, Vector3 posEnd)
    {
        if (CheckForDuplicateConnection(indexStart, indexEnd, posStart, posEnd) == true)
        {
            Debug.LogWarning("Duplicate connection exists between nodes " + indexStart + " and " + indexEnd);
            return false;
        }
        //check for Collisions
        if (CheckForNodeCollision(indexStart, indexEnd, posStart, posEnd) == true)
        {
            Debug.LogWarning("Collision! start: " + posStart + "  End: " + posEnd);
            return false;
        }
        //passed all checks
        return true;
    }
    #endregion
    #region CheckForDuplicateConnection
    /// <summary>
    /// Checks for an existing duplicate connection, returns false if no duplicate exist
    /// </summary>
    /// <param name="indexStart"></param>
    /// <param name="indexEnd"></param>
    /// <param name="posStart"></param>
    /// <param name="posEnd"></param>
    /// <returns></returns>
    private bool CheckForDuplicateConnection(int indexStart, int indexEnd, Vector3 posStart, Vector3 posEnd)
    {
        //check for duplicates -> get node script component references
        nodeStart = listOfNodeObjects[indexStart].GetComponent<Node>();
        nodeEnd = listOfNodeObjects[indexEnd].GetComponent<Node>();
        //check if a connection is not already present
        if (nodeStart.CheckNeighbourPosition(posEnd) == true) { return true; }
        if (nodeEnd.CheckNeighbourPosition(posStart) == true) { return true; }
        return false;
    }
    #endregion
    #region CheckForNodeCollision
    /// <summary>
    /// Does a line cast along projected connection path and returns false if path is clear of nodes, true if not
    /// </summary>
    /// <param name="indexStart"></param>
    /// <param name="indexEnd"></param>
    /// <param name="posStart"></param>
    /// <param name="posEnd"></param>
    /// <returns></returns>
    private bool CheckForNodeCollision(int indexStart, int indexEnd, Vector3 posStart, Vector3 posEnd)
    {
        bool checkRoute;
        //disable the box collider on both nodes
        boxColliderStart = listOfNodeObjects[indexStart].GetComponent<BoxCollider>();
        boxColliderEnd = listOfNodeObjects[indexEnd].GetComponent<BoxCollider>();
        boxColliderStart.enabled = false;
        boxColliderEnd.enabled = false;
        //line cast to check that no nodes are intersected
        checkRoute = Physics.Linecast(posStart, posEnd, blockingLayer);
        //re-enable Node boxColliders
        boxColliderStart.enabled = true;
        boxColliderEnd.enabled = true;
        return checkRoute;
    }
    #endregion
    #region PlaceConnection
    /// <summary>
    /// Create a connection between two nodes using a Cylinder prefab using positions and nodeID's
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <param name="rotation"></param>
    private void PlaceConnection(int node1, int node2, Vector3 pos1, Vector3 pos2, ConnectionType secLvl)
    {
        //get the mid point between the two nodes
        Vector3 connectionPosition = Vector3.Lerp(pos1, pos2, 0.50f);
        //get the distance between the two nodes (cylinders are two units high and one unit wide, hence, if laid on side, need to divide distance by 2 to get actual scale y_length)
        float distance = Vector3.Distance(pos1, pos2) / 2;
        //determine rotation of connection (runs from one node to the next)
        Vector3 direction = pos1 - pos2;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
        //copy connection prefab
        instanceConnection = Instantiate(connection, connectionPosition, rotation);
        //tweak length 
        Vector3 instanceScale = instanceConnection.transform.localScale;
        instanceConnection.transform.localScale = new Vector3(instanceScale.x, distance, instanceScale.z);
        //set up Connection fields
        Connection connectionTemp = instanceConnection.GetComponent<Connection>();
        connectionTemp.connID = GameManager.instance.nodeScript.connCounter++;
        connectionTemp.InitialiseConnection(node1, node2);
        //add an edge to Graph
        graph.AddEdge(node1, node2);
        //tweak Security Level & color
        connectionTemp.ChangeSecurityLevel(secLvl);
        //set parent
        instanceConnection.transform.SetParent(connectionHolder);
        //add to collections
        if (GameManager.instance.dataScript.AddConnection(connectionTemp) == true)
        {
            listOfConnections.Add(instanceConnection);
            //add to neighbours list
            nodeStart = listOfNodeObjects[node1].GetComponent<Node>();
            nodeEnd = listOfNodeObjects[node2].GetComponent<Node>();
            nodeStart.AddNeighbourPosition(pos2);
            nodeStart.AddNeighbourNode(nodeEnd);
            nodeStart.AddConnection(connectionTemp);
            nodeEnd.AddNeighbourPosition(pos1);
            nodeEnd.AddNeighbourNode(nodeStart);
            nodeEnd.AddConnection(connectionTemp);
            //connection
            connectionTemp.node1 = nodeStart;
            connectionTemp.node2 = nodeEnd;
        }
        else { Debug.LogError(string.Format("Invalid Connection, ID {0} -> Not added to collections", connectionTemp.connID)); }
    }
    #endregion

    #region ChangeAllConnections
    /// <summary>
    /// debug function to change all connection security levels (called from DebugGUI menu)
    /// </summary>
    /// <param name="secLvl"></param>
    public void ChangeAllConnections(ConnectionType secLvl)
    {
        foreach( GameObject connObject in listOfConnections)
        {
            Connection connection = connObject.GetComponent<Connection>();
            connection.ChangeSecurityLevel(secLvl);
        }
    }
    #endregion

    #region RemoveInvalidNodes
    /// <summary>
    /// loops listOfSortedNodes and removes any node connections that are invalid due to collisions
    /// </summary>
    private void RemoveInvalidNodes()
    {
        List<int> listOfIndexes = new List<int>();          //used to store indexes of all nodes to remove from lists
        int start, end;
        for (int v = 0; v < listOfSortedNodes.Count; v++)
        {
            //clear out index every pass
            listOfIndexes.Clear();
            //loop through sublist looking for invalid nodes
            for (int w = 0; w < listOfSortedNodes[v].Count; w++)
            {
                start = v;
                end = listOfSortedNodes[v][w];
                if (CheckForNodeCollision(start, end, listOfCoordinates[start], listOfCoordinates[end]) == true)
                {
                    //invalid node
                    listOfIndexes.Add(w);
                }
            }
            //check if any records need removing
            if (listOfIndexes.Count > 0)
            {
                //reverse loop removing indexes
                for (int i = listOfIndexes.Count - 1; i >= 0; i--)
                {
                    //Debug.Log("Invalid Node Removed " + v + " to " + listOfSortedNodes[v][listOfIndexes[i]]);
                    listOfSortedNodes[v].RemoveAt(listOfIndexes[i]);
                    listOfSortedDistances[v].RemoveAt(listOfIndexes[i]);
                }
            }
        }
        //debug printout remaining nodes -> very poor memory management code!
        /*string debugOutput;
        for (int i = 0; i < listOfSortedNodes.Count; i++)
        {
            debugOutput = "Node " + i + " -> ";
            for (int j = 0; j < listOfSortedNodes[i].Count; j++)
            {
                debugOutput += listOfSortedNodes[i][j] + " ";
            }
            Debug.Log(debugOutput);
        }*/
    }
    #endregion
    #region AddRandomConnections
    /// <summary>
    /// Adds additional random connections using one of a nodes three shortest connections. Not all nodes get an extra connection.
    /// </summary>
    private void AddRandomConnections()
    {
        int other, num;
        Vector3 vOne, vTwo;
        for (int v = 0; v < numOfNodes; v++)
        {
            //each node has a % chance of gaining an additional connection
            if (Random.Range(0, 100) <= connectionFrequency)
            {
                num = listOfSortedNodes[v].Count;
                if (num >= 3)
                {
                    //loop through the three shortest connections and place any that aren't duplicates
                    for (int i = 0; i < 3; i++)
                    {
                        other = listOfSortedNodes[v][i];
                        vOne = listOfCoordinates[v];
                        vTwo = listOfCoordinates[other];
                        //check that this connection doesn't already exist (listOfSortedNodes is already pre-checked for Node collisions)
                        if (CheckForDuplicateConnection(v, other, vOne, vTwo) == false)
                        {
                            //create new connection
                            PlaceConnection(v, other, vOne, vTwo, ConnectionType.HIGH);
                        }
                    }
                }
            }
        }
    }
    #endregion


    //
    // - - - Graph related methods - - -
    //
    #region Graph Analysis Methods
    /// <summary>
    /// returns a string made up of basic GraphAPI analysis methods
    /// </summary>
    /// <returns></returns>
    public string GetGraphAnalysis()
    {
        string analysis = "Graph Analysis" + "\n\n";
        //graphAPI analysis data
        if (graph != null)
        {
            analysis += "MaxDegree:  " + Convert.ToString(graph.CalcMaxDegree()) + "\n";
            analysis += "AvgDegree:  " + Convert.ToString(graph.CalcAvgDegree()) + "\n";
            analysis += "SelfLoops:    " + Convert.ToString(graph.CalcSelfLoops()) + "\n\n";
        }
        else
        { Debug.LogError("Graph is Null -> no analysis available"); }
        //base stats
        analysis += "NumNodes:  " + Convert.ToString(listOfNodeObjects.Count) + "\n";
        analysis += "NumConns:  " + Convert.ToString(listOfConnections.Count) + "\n\n";
        analysis += TestSearch();
        return analysis;
    }

   
    /// <summary>
    /// returns a string made up of Node type distribution
    /// </summary>
    /// <returns></returns>
    public string GetNodeAnalysis()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("Node Analysis" + "\n\n");
        for (int i = 0; i < GameManager.instance.dataScript.CheckNumOfNodeArcs(); i++)
        {
            NodeArc arc = GameManager.instance.dataScript.GetNodeArc(i);
            builder.Append(string.Format("{0}  {1}{2}", arc.name, arrayOfNodeArcTotals[(int)NodeArcTally.Current, i], "\n"));
        }
        return builder.ToString();
    }

    /// <summary>
    /// Test Search that determines if the graph is connected or not
    /// </summary>
    /// <returns></returns>
    private string TestSearch()
    {
        string searchResult = "IS Connected";
        if (graph != null)
        {
            Search search = new Search(graph, 0);
            if (search.Count != graph.Vertices && search.Count != graph.Vertices -1)
                { searchResult = "Not Connected"; }
        }
        return searchResult;
    }
    #endregion

    //
    // - - - Graph Related Search Methods - - -
    //
    #region Graph related Search Methods
    /// <summary>
    /// Input a node and set all linked connections to that node to neutral colour
    /// </summary>
    /// <param name="node"></param>
    public void CheckConnected(int node)
    {
        int node1, node2;
        Search search = new Search(graph, node);
        bool[] tempArray = search.GetMarkedArray();
        //loop all connections and recolour any that can trace a path back to the source node with a neutral colour
        foreach (GameObject obj in listOfConnections)
        {
            Connection conn = obj.GetComponent<Connection>();
            node1 = conn.VerticeOne;
            node2 = conn.VerticeTwo;
            if (tempArray[node1] == true && tempArray[node2] == true)
            {
                //reset connection color to neutral
                conn.ChangeSecurityLevel(ConnectionType.None);
            }
        }
    }

    /// <summary>
    /// displays Path from source node 0 to selected node
    /// </summary>
    /// <param name="node"></param>
    public void CheckPath(int node)
    {
        int node1, node2;
        DepthFirstPath path = new DepthFirstPath(graph, 0);
        List<int> pathList = path.GetPathTo(node);
        for(int i = 0; i < pathList.Count; i++)
        {
            Debug.Log(string.Format("Path -> {0}", pathList[i]));
        }
        //loop all connections and recolour any that are on the path
        foreach(GameObject obj in listOfConnections)
        {
            Connection conn = obj.GetComponent<Connection>();
            node1 = conn.VerticeOne;
            node2 = conn.VerticeTwo;
            //are the two nodes that make up the connection in the path list?
            if (pathList.Exists(v => v == node1) && pathList.Exists(v => v == node2))
            {
                //reset connection color to nuetral
                conn.ChangeSecurityLevel(ConnectionType.None);
            }
        }
    }
    #endregion

    //
    // - - - MST related methods - - -
    //
    #region InitialiseGraph, assign Node and Actor arcs
    /// <summary>
    /// Set up an Edge Weighted graph ready for an MST
    /// </summary>
    private void InitialiseGraph()
    {
        ewGraph = new EdgeWeightedGraph(numOfNodes);
        graph = new Graph(numOfNodes);  //keeps a duplicate data structure -> used for pathing and analysis
        int idOne, idTwo;
        Vector3 vOne, vTwo;
        float weight;
        connectionHolder = new GameObject("MasterConnection").transform;

        //add edges to graph
        for (int v = 0; v < numOfNodes; v++)
        {
            for (int w = 0; w < listOfSortedNodes[v].Count; w++)
            {
                idOne = listOfSortedNodes[v][w];
                weight = listOfSortedDistances[v][w];
                Edge e = new Edge(v, idOne, weight);
                ewGraph.AddEdge(e);
                //Debug.Log("Added -> " + e.GetString());
            }
        }
        Debug.Log("Nodes " + numOfNodes + "  Edges " + ewGraph.E_total);
        //create MST
        LazyPrimMST mst = new LazyPrimMST(ewGraph);
        //List of Edges
        List<Edge> tempList = mst.GetEdges();
        //loop list of Edges and draw connections
        foreach(Edge edge in tempList)
        {
            //get the two nodeID's
            idTwo = edge.GetEither();
            idOne = edge.GetOther(idTwo);
            //get the two node positions
            vOne = listOfCoordinates[idTwo];
            vTwo = listOfCoordinates[idOne];
            //draw connection
            PlaceConnection(idTwo, idOne, vOne, vTwo, ConnectionType.LOW);
        }
        //add in random additional connections
        AddRandomConnections();
    }

    /// <summary>
    /// loops through a sorted list of nodes (by # of connections) and randomly assigning archetypes
    /// </summary>
    private void AssignNodeArcs()
    {
        int index;
        int numRecords = GameManager.instance.dataScript.CheckNumOfNodeArcs();
        //array of counters
        arrayOfNodeArcTotals = new int[(int)NodeArcTally.Count, numRecords];
        //loop list of nodes
        foreach(Node node in listOfNodes)
        {
            int numConnections = node.GetNumOfNeighbours();
            //node name
            node.nodeName = "Placeholder";
            //get random node Arc from appropriate list
            node.Arc = GetRandomNodeArc(numConnections);
            //provide base level stats 
            node.Stability = node.Arc.Stability;
            node.Support = node.Arc.Support;
            node.Security = node.Arc.Security;
            //keep within range of 0 to 3
            int maxNodeValue = GameManager.instance.nodeScript.maxNodeValue;
            node.Stability = Mathf.Clamp(node.Stability, 0, maxNodeValue);
            node.Security = Mathf.Clamp(node.Security, 0, maxNodeValue);
            node.Support = Mathf.Clamp(node.Support, 0, maxNodeValue);
            //position
            node.nodePosition = node.transform.position;
            //target -> none
            node.targetID = -1;
            //keep a tally of how many of each type have been generated
            index = node.Arc.nodeArcID;
            if (index < numRecords)
            { arrayOfNodeArcTotals[(int)NodeArcTally.Current, index]++; }
            else { Debug.LogError(string.Format("Number of NodeArcs exceeded by nodeArcID {0} for Node {1}", index, node.Arc.name)); }
        }
        //Display stats
        string name;
        int num;
        Debug.Log("Node Summary" + "\n");
        for (int i = 0; i < numRecords; i++)
        {
            num = arrayOfNodeArcTotals[(int)NodeArcTally.Current, i];
            if (num > 0)
            {
                name = GameManager.instance.dataScript.GetNodeArc(i).name;
                Debug.Log(string.Format("Node {0} total {1}{2}", name, num, "\n"));
            }
        }
    }

    /// <summary>
    /// Returns a node Arc based on the numConnections that node has the requirements of the City (which can overide default data)
    /// </summary>
    /// <param name="numConnections"></param>
    /// <returns></returns>
    private NodeArc GetNodeArc(int numConnections) 
    {
        NodeArc tempArc = null; 
        List<NodeArc> tempList = null;
        switch (numConnections)
        {
            case 1:
                tempList = listOfOneConnArcs; break;
            case 2:
                tempList = listOfTwoConnArcs; break;
            case 3:
                tempList = listOfThreeConnArcs; break;
            case 4:
                tempList = listOfFourConnArcs; break;
            case 5:
            case 6:
            case 7:
            case 8:
                tempList = listOfFiveConnArcs; break;
            default:
                Debug.LogError("Invalid number of Connections " + numConnections);
                break;
        }
        tempArc = tempList[Random.Range(0, tempList.Count)];
        return tempArc;
    }

    /// <summary>
    /// populates lists of node arcs by connection number. First checks City data and, if none, uses DataManager.cs default set
    /// Also initialises level set-up data using the city first, default second approach
    /// </summary>
    private void InitialiseData()
    {
        City city = GameManager.instance.cityScript.GetCity();
        Debug.Assert(city != null && city.Arc != null, "City or City.Arc is Null");
        if (city != null && city.Arc != null)
        {
            //
            // - - - Node Arc Lists - - -
            //
            //one connection
            if (city.Arc.listOfOneConnArcs != null && city.Arc.listOfOneConnArcs.Count > 0)
            { listOfOneConnArcs = city.Arc.listOfOneConnArcs; }
            else { listOfOneConnArcs = GameManager.instance.dataScript.GetDefaultNodeArcList(1); }
            //two connection
            if (city.Arc.listOfTwoConnArcs != null && city.Arc.listOfTwoConnArcs.Count > 0)
            { listOfTwoConnArcs = city.Arc.listOfTwoConnArcs; }
            else { listOfTwoConnArcs = GameManager.instance.dataScript.GetDefaultNodeArcList(2); }
            //three connection
            if (city.Arc.listOfThreeConnArcs != null && city.Arc.listOfThreeConnArcs.Count > 0)
            { listOfThreeConnArcs = city.Arc.listOfThreeConnArcs; }
            else { listOfThreeConnArcs = GameManager.instance.dataScript.GetDefaultNodeArcList(3); }
            //four connection
            if (city.Arc.listOfFourConnArcs != null && city.Arc.listOfFourConnArcs.Count > 0)
            { listOfFourConnArcs = city.Arc.listOfFourConnArcs; }
            else { listOfFourConnArcs = GameManager.instance.dataScript.GetDefaultNodeArcList(4); }
            //five connection
            if (city.Arc.listOfFiveConnArcs != null && city.Arc.listOfFiveConnArcs.Count > 0)
            { listOfFiveConnArcs = city.Arc.listOfFiveConnArcs; }
            else { listOfFiveConnArcs = GameManager.instance.dataScript.GetDefaultNodeArcList(5); }
            //asserts
            Debug.Assert(listOfOneConnArcs != null && listOfOneConnArcs.Count > 0, "Invalid listOfOneConnArcs");
            Debug.Assert(listOfTwoConnArcs != null && listOfTwoConnArcs.Count > 0, "Invalid listOfTwoConnArcs");
            Debug.Assert(listOfThreeConnArcs != null && listOfThreeConnArcs.Count > 0, "Invalid listOfThreeConnArcs");
            Debug.Assert(listOfFourConnArcs != null && listOfFourConnArcs.Count > 0, "Invalid listOfFourConnArcs");
            Debug.Assert(listOfFiveConnArcs != null && listOfFiveConnArcs.Count > 0, "Invalid listOfFiveConnArcs");
            //
            // - - - Set up data - - -
            //
            if (city.Arc.numOfNodes >= 10 && city.Arc.numOfNodes < 40)
            { numOfNodes = city.Arc.numOfNodes; }
            else { numOfNodes = numOfNodesDefault; }
            if (city.Arc.minNodeSpacing >= 1.0f && city.Arc.minNodeSpacing <= 3.0f)
            { minSpacing = city.Arc.minNodeSpacing; }
            else { minSpacing = minSpacingDefault; }
            if (city.Arc.connectionFrequency >= 0 && city.Arc.connectionFrequency <= 100)
            { connectionFrequency = city.Arc.connectionFrequency; }
            else { connectionFrequency = connectionFrequencyDefault; }
            if (city.connectionSecurityChance >= 0 && city.connectionSecurityChance <= 100)
            { connectionSecurity = city.connectionSecurityChance; }
            else { connectionSecurity = connectionSecurityDefault; }
        }
        else
        {
            //no valid city found
            Debug.LogWarning("Invalid City or City Arc (Null)");
            //Node Arc lists
            listOfOneConnArcs = GameManager.instance.dataScript.GetDefaultNodeArcList(1);
            listOfTwoConnArcs = GameManager.instance.dataScript.GetDefaultNodeArcList(2);
            listOfThreeConnArcs = GameManager.instance.dataScript.GetDefaultNodeArcList(3);
            listOfFourConnArcs = GameManager.instance.dataScript.GetDefaultNodeArcList(4);
            listOfFiveConnArcs = GameManager.instance.dataScript.GetDefaultNodeArcList(5);
            //Set up Data
            numOfNodes = numOfNodesDefault;
            minSpacing = minSpacingDefault;
            connectionFrequency = connectionFrequencyDefault;
            connectionSecurity = connectionSecurityDefault;
        }

    }

    /// <summary>
    /// returns a random Arc from the appropriate list based on the number of Connections that the node has
    /// </summary>
    /// <param name="numConnections"></param>
    /// <returns></returns>
    private NodeArc GetRandomNodeArc(int numConnections)
    {
        NodeArc tempArc = null;
        List<NodeArc> tempList = null;
        switch (numConnections)
        {
            case 1:
                tempList = listOfOneConnArcs; break;
            case 2:
                tempList = listOfTwoConnArcs; break;
            case 3:
                tempList = listOfThreeConnArcs; break;
            case 4:
                tempList = listOfFourConnArcs;  break;
            case 5:
            case 6:
            case 7:
            case 8:
                tempList = listOfFiveConnArcs; break;
            default:
                Debug.LogError("Invalid number of Connections " + numConnections);
                break;
        }
        tempArc = tempList[Random.Range(0, tempList.Count)];
        return tempArc;
    }

    /*/// <summary>
    /// sets up array prior to use (needed because AssignActorsToNodes is called for each side and each instance would overwrite the previous sides data)
    /// </summary>
    private void InitialiseArrayOfActiveNodes()
    {
        //initialise arrayOfActiveNodes prior to use
        arrayOfActiveNodes = new bool[listOfNodeObjects.Count, GameManager.instance.dataScript.GetNumOfGlobalSide(), GameManager.instance.actorScript.maxNumOfOnMapActors];
    }*/

    

    #endregion


    //
    // - - - Assign Security Levels to Connections
    //
    #region AssignSecurityLevels
    /// <summary>
    /// assigns Connection security levels depending on node sec lvl and a random roll. Default is Low Sec
    /// </summary>
    private void AssignSecurityLevels()
    {
        int chance = connectionSecurity;
        int node1, node2, security = 0;       //node ID's either end of connection
        //set all to default level
        ChangeAllConnections(ConnectionType.None);
        //loop connections
        foreach (GameObject obj in listOfConnections)
        {
            Connection connection = obj.GetComponent<Connection>();
            node1 = connection.GetNode1();
            node2 = connection.GetNode2();
            //get the highest node security level of the two
            GameObject nodeObj1 = GameManager.instance.dataScript.GetNodeObject(node1);
            if (nodeObj1 != null)
            {
                Node node = nodeObj1.GetComponent<Node>();
                security = node.Security;
            }
            GameObject nodeObj2 = GameManager.instance.dataScript.GetNodeObject(node2);
            if (nodeObj2 != null)
            {
                Node node = nodeObj2.GetComponent<Node>();
                security = node.Security < security ? node.Security : security;
            }
            if (security > 0)
            {
                //percentage chance of higher security level than default
                if (Random.Range(0, 100) < chance)
                { connection.ChangeSecurityLevel((ConnectionType)security); }
            }
        }
    }

#endregion



    /*/// <summary>
    /// returns true if a given nodeID is active for the specified actor slotID (0 to 3)
    /// </summary>
    /// <param name="nodeID"></param>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public bool CheckNodeActive(int nodeID, GlobalSide side, int slotID)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(nodeID > -1 && nodeID < numOfNodes, "Invalid nodeID input");
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid slotID input");
        return arrayOfActiveNodes[nodeID, side.level, slotID];
    }*/

    /// <summary>
    /// returns totals of all node arcs in an array format
    /// </summary>
    /// <returns></returns>
    public int[] GetNodeTypeTotals()
    {
        int length = arrayOfNodeArcTotals.GetLength(1);
        int[] tempArray = new int[length];
        for (int i = 0; i < length; i++)
        { tempArray[i] = arrayOfNodeArcTotals[0, i]; }
        return tempArray;
    }

    public List<Node> GetListOfNodes()
    { return listOfNodes; }

}
