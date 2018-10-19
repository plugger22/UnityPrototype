using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using GraphAPI;
using gameAPI;
using System.Text;

public enum NodeArcTally { Current, Minimum, Count };   //used for indexing of arrayOfNodeArcTotals

public class LevelManager : MonoBehaviour
{
    public GameObject node;             //node prefab
    public GameObject connection;       //connection prefab
    public LayerMask blockingLayer;     //nodes are on the blocking layer, not connections
    [Header("Default City Setup")]
    [Tooltip("number of nodes (adjusted after use in InitialiseNodes() to reflect actual number). Set to CitySize 'Normal'")]
    [Range(10, 30)] public int numOfNodesDefault = 20; 
    [Tooltip("minimum spacing (world units) between nodes (>=). Set to CitySpacing 'Normal'")]
    [Range(1f, 3f)] public float minSpacingDefault = 1.5f;
    [Tooltip("Random % chance of a node having additional connections, set to CityConnection 'Normal' value")]
    [Range(0, 100)] public int connectionFrequencyDefault = 50;
    [Tooltip("Chance of a connection having a high security level (more than 'None'). Set to CitySecurity 'Normal' value")]
    [Range(0, 100)] public int connectionSecurityDefault = 25;

    //How many different node connection totals are there in sequential order going upwards from 1 to the specified number. Used to set up arrays, etc. DO NOT CHANGE
    private int maxConnections = 5;

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

    private List<NodeArc>[] listOfConnArcsDefault;      //Note that the array is indexed from 0 and that the node Connetions are from 1 (so -1 to numOfConnections to access correct list in array)
    private List<NodeArc>[] listOfConnArcsPreferred;    //Note that the array is indexed from 0 and that the node Connetions are from 1 (so -1 to numOfConnections to access correct list in array)

    //collections (all indexes correspond throughout, eg. listOfNodes[2] = listOfCoordinates[2] = listOfSortedDistances[2])
    //Note: collections initialised here as GameManager.SetUpScene will run prior to the Awake callback here
    private List<GameObject> listOfNodeObjects = new List<GameObject>();
    private List<Node> listOfNodes = new List<Node>();                              //mirror list to listOfNodeObjects but Nodes instead of GO's for speed of use
    private List<GameObject> listOfConnections = new List<GameObject>();
    private List<Vector3> listOfCoordinates = new List<Vector3>();                  //used to provide a lookup to check spacing of nodes
    private List<List<int>> listOfSortedNodes = new List<List<int>>();              //each node has a sorted (closest to furthest) list of nodeID's of neighouring nodes
    private List<List<float>> listOfSortedDistances = new List<List<float>>();    //companion list to listOfSortedNodes (identical indexes) -> contains distances to node in other list in world units    
     
    private int[,] arrayOfNodeArcTotals;            //array of how many of each node type there is on the map, index -> [(int)NodeArcTally, nodeArc.nodeArcID]

    //fast access
    private City city;
    
    /// <summary>
    /// Master method that drives a level
    /// </summary>
    public void Initialise()
    {
        //fast access
        city = GameManager.instance.cityScript.GetCity();
        Debug.Assert(city != null, "Invalid city (Null)");
        //ProcGen level
        InitialiseData();
        InitialiseNodes(numOfNodes, minSpacing);
        InitialiseSortedDistances();
        RemoveInvalidNodes();
        InitialiseNodeArcArray();
        InitialiseGraph();
        InitialiseNodeArcs();
        /*AssignNodeArcs();*/
        AssignSecurityLevels();
        InitialiseDistrictNames();
        EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Redraw, "LevelManager.cs -> Initialise");
    }

    /// <summary>
    /// populates lists of node arcs by connection number. First checks City data and, if none, uses DataManager.cs default set
    /// Also initialises level set-up data using the city first, default second approach
    /// </summary>
    private void InitialiseData()
    {
        //NodeArc arrays
        listOfConnArcsDefault = new List<NodeArc>[maxConnections];
        listOfConnArcsPreferred = new List<NodeArc>[maxConnections];
        //initialise arrays (independent off city being valid or not)
        for (int index = 0; index < maxConnections; index++)
        {
            List<NodeArc> tempListDefault = GameManager.instance.dataScript.GetDefaultNodeArcList(index + 1);
            if (tempListDefault != null) { listOfConnArcsDefault[index] = tempListDefault; }
            List<NodeArc> tempListPreferred = GameManager.instance.dataScript.GetPreferredNodeArcList(index + 1);
            if (tempListPreferred != null) { listOfConnArcsPreferred[index] = tempListPreferred; }

        }
        if (city.Arc != null)
        {
            // Set up data
            numOfNodes = city.Arc.size.numOfNodes;
            minSpacing = city.Arc.spacing.minDistance;
            connectionFrequency = city.Arc.connections.frequency;
            connectionSecurity = city.Arc.security.chance;
        }
        else
        {
            //no valid city Arc found
            Debug.LogWarning("Invalid City Arc (Null)");
            //Set up Data
            numOfNodes = numOfNodesDefault;
            minSpacing = minSpacingDefault;
            connectionFrequency = connectionFrequencyDefault;
            connectionSecurity = connectionSecurityDefault;
        }

    }

    //
    // --- Graph construction ---
    //
    /// <summary>
    /// place 'x' num nodes randomly within a 11 x 11 grid (world units) with a minimum spacing between them all
    /// </summary>
    /// <param name="number"></param>
    /// <param name="spacing"></param>
    private void InitialiseNodes(int number, float spacing)
    {
        int outerLoopCtr, innerLoopCtr, random_x, random_z;
        int size = 11;
        int numOfTimesToCheck = 20;
        Vector3 randomPos = Vector3.zero; //default value
        bool validPos;
        nodeHolder = new GameObject("MasterNode").transform;
        Node nodeTemp;
        //array of mirror cells to grid where if true, a node exists. Used for optimising placement routine.
        bool[,] arrayOfFlags = new bool[size, size];
        //zero out flag array
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            { arrayOfFlags[i, j] = false; }
        }
        //loop for number of nodes required (11 x 11 grid with coords ranging from -5 to +5)
        for (int i = 0; i < number; i++)
        {
            outerLoopCtr = 0;
            //continue generating random positions until a valid one is found (max 20 iterations then go to manual if not successful)
            do
            {
                outerLoopCtr++;
                validPos = true;
                //set up Vector3
                random_x = Random.Range(0, size);
                random_z = Random.Range(0, size);
                //check array first
                if (arrayOfFlags[random_x, random_z] == false)
                {
                    randomPos.x = random_x - 5;
                    randomPos.y = 0.5f;
                    randomPos.z = random_z - 5;
                    //empty cell, loop list and check min spacing requirement
                    validPos = CheckPositionValid(randomPos, spacing);
                }
                else { validPos = false; }
                //Timed out, do it the hard way by stepping through the array, cell by cell, looking for any empty one
                if (outerLoopCtr >= numOfTimesToCheck)
                {
                    innerLoopCtr = 0;
                    Debug.LogFormat("[Tst] LevelManager.cs -> InitialiseNodes: failed random placement (20 times), index {0}", i);
                    //random position to kick off
                    random_x = Random.Range(0, size);
                    random_z = Random.Range(0, size);
                    //find a valid position by traversing array, step by step
                    do
                    {
                        innerLoopCtr++;
                        //not a duplicate and are immediate orthagonal neighbours blanks?
                        if (arrayOfFlags[random_x, random_z] == false && CheckForOrthagonalNeighbours(arrayOfFlags, size - 1, random_x, random_z) == false)
                        {
                            randomPos.x = random_x - 5;
                            randomPos.y = 0.5f;
                            randomPos.z = random_z - 5;
                            //empty cell, loop list and check min spacing requirement
                            validPos = CheckPositionValid(randomPos, spacing);
                            //invalid position, increment array, roll over if need be, for next position to try
                            if (validPos == false)
                            {
                                if (Random.Range(0, 100) < 50)
                                { random_x++; if (random_x >= size) { random_x = 0; } }
                                else
                                { random_z++; if (random_z >= size) { random_z = 0; } }
                            }
                        }
                        else
                        {
                            //Invalid position, increment array, roll over if need be, for next position to try
                            validPos = false;
                            if (Random.Range(0, 100) < 50)
                            { random_x++; if (random_x >= size) { random_x = 0; } }
                            else
                            { random_z++; if (random_z >= size) { random_z = 0; } }
                        }
                        if (validPos == true)
                        { Debug.LogFormat("[Tst] LevelManager.cs -> InitaliseNodes: Manual array stepping WORKED for index {0}, looped {1} times", i, innerLoopCtr); }
                    }
                    while (validPos == false && innerLoopCtr < 100);
                }
            }
            while (validPos == false && outerLoopCtr < numOfTimesToCheck);
            //successful node
            if (validPos == true)
            {
                //update array if required
                arrayOfFlags[random_x, random_z] = true;
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
        /*numOfNodes = listOfNodeObjects.Count;*/
        numOfNodes = listOfNodes.Count;
        if (numOfNodes != number)
        { Debug.LogFormat("[Tst] LevelManager.cs -> InitialiseNodes: Mismatch on InitialiseNodes, {0} Nodes short", number - numOfNodes); }
    }

    /// <summary>
    /// subMethod for InitialiseNodes to check a randomPosition against all existing nodes for minimum spacing. Returns true if O.K, false if not
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="spacing"></param>
    /// <returns></returns>
    private bool CheckPositionValid(Vector3 pos, float spacing)
    {
        bool isValidPos = true;
        float distance;
        //loop existing nodes
        for (int j = 0; j < listOfCoordinates.Count; j++)
        {
            distance = Vector3.Distance(listOfCoordinates[j], pos);

            /*//identical to an existing position?
            if (listOfCoordinates[j] == pos) { isValidPos = false; break; }*/

            //fails minimum spacing test
            if (distance <= spacing) { isValidPos = false; break; }
        }
        return isValidPos;
    }

    /// <summary>
    /// subMethod for InitialiseNodes to quickly check if any there are any immediate orthagonal neighbours using array (distance would be 1.0 which is too close for all spacings, returns true)
    /// </summary>
    /// <param name="random_x"></param>
    /// <param name="random_z"></param>
    /// <returns></returns>
    private bool CheckForOrthagonalNeighbours(bool[,] arrayOfFlags, int max, int random_x, int random_z)
    {
        if (random_x > 0)
        { if (arrayOfFlags[random_x - 1, random_z] == true) { return true; } }
        if (random_x < max)
        { if (arrayOfFlags[random_x + 1, random_z] == true) { return true; } }
        if (random_z > 0)
        { if (arrayOfFlags[random_x, random_z - 1] == true) { return true; } }
        if (random_z < max)
        { if (arrayOfFlags[random_x, random_z + 1] == true) { return true; } }
        return false;
    }

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
        /*for (int index = 0; index < listOfNodeObjects.Count; index++)*/
        for (int index = 0; index < listOfNodes.Count; index++)
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

    /*/// <summary>
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
    }*/

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
    }

    /// <summary>
    /// run once nodes have been finalised. Sets up array prior to assigning NodeArcs
    /// </summary>
    private void InitialiseNodeArcArray()
    {
        int minValue = 0;
        //initialiseArray
        int numRecords = GameManager.instance.dataScript.CheckNumOfNodeArcs();
        arrayOfNodeArcTotals = new int[(int)NodeArcTally.Count, numRecords];
        //get minimum number of each type of NodeArc
        minValue = city.Arc.size.minNum;
        Debug.Assert(minValue > 0, "Invalid minValue (Zero)");
        //loop nodeArcs
        Dictionary<int, NodeArc> dictOfNodeArcs = GameManager.instance.dataScript.GetDictOfNodeArcs();
        if (dictOfNodeArcs != null)
        {
            //assign a minimum number of nodes that must be this nodeArc type
            foreach(var nodeArc in dictOfNodeArcs)
            {  arrayOfNodeArcTotals[(int)NodeArcTally.Minimum, nodeArc.Key] = minValue; }
        }
        else { Debug.LogError("Invalid dictOfNodeArcs (Null)"); }
    }

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

    //
    // - - - Graph related methods - - -
    //



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

    //
    // - - - Graph Related Search Methods - - -
    //

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

    //
    // - - - MST related methods - - -
    //

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
    /// Assign nodeArcs to nodes according to city data & in sequential order so that minimum and priority requirements are met. Any leftover nodes are random.
    /// </summary>
    private void InitialiseNodeArcs()
    {
        int index, rndIndex, current, minimum, numConnections, remainingNodes;
        bool isRepeat;
        int numRecords = GameManager.instance.dataScript.CheckNumOfNodeArcs();
        current = (int)NodeArcTally.Current;
        minimum = (int)NodeArcTally.Minimum;
        //create a temporary list of all nodes
        List<Node> tempListOfNodes = new List<Node>(listOfNodes);
        List<NodeArc> tempListOfNodeArcs = new List<NodeArc>();
        /*Debug.LogFormat("LevelManager.cs -> InitialiseNodeArcs: START tempListOfNodes has {0} records{1}", tempListOfNodes.Count, "\n");*/
        //
        // - - - MINIMIMUM FIRST
        //
        for (int i = tempListOfNodes.Count - 1; i >= 0; i--)
        {
            Node node = tempListOfNodes[i];
            if (node != null)
            {
                numConnections = node.GetNumOfNeighbours();
                numConnections = Mathf.Min(5, numConnections) - 1;
                //get list of NodeArcs for this number of connections (Preferred)
                tempListOfNodeArcs = listOfConnArcsPreferred[numConnections];
                //loop list looking for unassigned nodeArcs
                foreach (NodeArc nodeArc in tempListOfNodeArcs)
                {
                    index = nodeArc.nodeArcID;
                    //check if this NodeArc still requires nodes
                    if (arrayOfNodeArcTotals[current, index] < arrayOfNodeArcTotals[minimum, index] == true)
                    {
                        if (nodeArc != null)
                        {
                            //assign node arc, set node details and adjust count
                            SetNodeDetails(node, nodeArc);
                            arrayOfNodeArcTotals[current, index]++;
                            //delete node from tempList and go onto the next
                            tempListOfNodes.RemoveAt(i);
                            break;
                        }
                        else { Debug.LogError("Invalid nodeArc (Null)"); }
                    }
                }
            }
            else { Debug.LogErrorFormat("Invalid node (Null) from tempListOfNodes[{0}]", i); }
        }
        //Display stats
        DisplayNodeStats("MINIMUM (First Pass)", numRecords);
        /*Debug.LogFormat("LevelManager.cs -> InitialiseNodeArcs: MINIMUM (First) tempListOfNodes has {0} records{1}", tempListOfNodes.Count, "\n");*/
        //
        // - - - MINIMUM FINAL (makes up any deficit)
        //
        for (int i = 0; i < numRecords; i++)
        {
            isRepeat = false;
            do
            {
                if (arrayOfNodeArcTotals[current, i] < arrayOfNodeArcTotals[minimum, i] == true)
                {
                    isRepeat = true;
                    //assign random node
                    rndIndex = Random.Range(0, tempListOfNodes.Count);
                    Node node = tempListOfNodes[rndIndex];
                    if (node != null)
                    {
                        //assign node arc, set node details and adjust count
                        NodeArc nodeArc = GameManager.instance.dataScript.GetNodeArc(i);
                        if (nodeArc != null)
                        {
                            SetNodeDetails(node, nodeArc);
                            arrayOfNodeArcTotals[current, i]++;
                            //delete node from tempList
                            tempListOfNodes.RemoveAt(rndIndex);
                        }
                        else { Debug.LogErrorFormat("Invalid NodeArc (Null) for nodeArcID {0}", i); }
                    }
                    else { Debug.LogErrorFormat("Invalid node (Null) from tempListOfNodes[{0}]", i); }
                    //check if minimimum requirement met
                    if (arrayOfNodeArcTotals[current, i] < arrayOfNodeArcTotals[minimum, i] == false)
                    { isRepeat = false; }
                    //check there is at least one unassigned node remaining
                    if (tempListOfNodeArcs.Count == 0) { isRepeat = false; }
                }
            }
            while (isRepeat == true);
        }
        //Display stats
        DisplayNodeStats("MINIMUM (Final Pass)", numRecords);
        /*Debug.LogFormat("LevelManager.cs -> InitialiseNodeArcs: MINIMUM (Final) tempListOfNodes has {0} records{1}", tempListOfNodes.Count, "\n");*/
        //
        // - - - PRIORITY nodeArcs, if any
        //
        remainingNodes = tempListOfNodes.Count;
        if (remainingNodes > 0)
        {
            NodeArc arcPriority = city.Arc.priority;
            if (arcPriority != null)
            {
                index = arcPriority.nodeArcID;
                int numToAssign = remainingNodes / 2;
                Debug.LogFormat("LevelManager.cs -> InitialiseNodeArcs: Priority NodeArc \"{0}\", nodeArcID {1}, numToAssign {2}{3}", arcPriority.name, arcPriority.nodeArcID, numToAssign, "\n");
                //randomly assign half the remaining node arcs to the priority NodeArc
                int counter = Mathf.Min(numToAssign, remainingNodes);
                for (int i = 0; i < counter; i++)
                {
                    rndIndex = Random.Range(0, tempListOfNodes.Count);
                    Node node = tempListOfNodes[rndIndex];
                    if (node != null)
                    {
                        //assign node arc, set node details and adjust count
                        NodeArc nodeArc = arcPriority;
                        if (nodeArc != null)
                        {
                            SetNodeDetails(node, nodeArc);
                            arrayOfNodeArcTotals[current, arcPriority.nodeArcID]++;
                            //delete node from tempList
                            tempListOfNodes.RemoveAt(rndIndex);
                        }
                        else { Debug.LogErrorFormat("Invalid NodeArc (Null) for nodeArcID {0}", arcPriority.nodeArcID); }
                    }
                    else { Debug.LogErrorFormat("Invalid node (Null) from tempListOfNodes[{0}]", i); }
                }
            }
            else { Debug.Log("LevelManager.cs -> InitialiseNodeArcs: There is NO cityArc Priority"); }
        }
        else { Debug.LogWarning("NO more nodes available, tempListOfNodeArcs is Empty"); }
        //Display stats
        DisplayNodeStats("PRIORITY", numRecords);
        /*Debug.LogFormat("LevelManager.cs -> InitialiseNodeArcs: PRIORITY tempListOfNodes has {0} records{1}", tempListOfNodes.Count, "\n");*/
        //
        // - - - RANDOM
        //
        remainingNodes = tempListOfNodes.Count;
        if (remainingNodes > 0)
        {
            for (int i = 0; i < remainingNodes; i++)
            {
                Node node = tempListOfNodes[i];
                if (node != null)
                {
                    numConnections = node.GetNumOfNeighbours();
                    //get random node Arc from appropriate list
                    NodeArc nodeArc = GetNodeArcRandom(numConnections);
                    if (nodeArc != null)
                    {
                        //assign data to node
                        SetNodeDetails(node, nodeArc);
                        //keep a tally of how many of each type have been generated
                        index = node.Arc.nodeArcID;
                        arrayOfNodeArcTotals[current, index]++;
                    }
                    else { Debug.LogError("Invalid nodeArc (Null)"); }
                }
                else { Debug.LogErrorFormat("Invalid node (Null) for tempListOfNodes[{0}]", i); }
            }
        }
        //Display stats
        DisplayNodeStats("FINAL (Random defaul for remaining)", numRecords);
        /*Debug.LogFormat("LevelManager.cs -> InitialiseNodeArcs: RANDOM tempListOfNodes has {0} records{1}", tempListOfNodes.Count, "\n");*/
    }

    /// <summary>
    /// Submethod for InitialiseNodeArcs to set up a node based on it's assigned NodeArc
    /// </summary>
    /// <param name="node"></param>
    /// <param name="nodeArc"></param>
    private void SetNodeDetails(Node nodeTemp, NodeArc nodeArc)
    {
        //find node in listOfNodes & update details (currently in a tempList and will be deleted)
        Node node = listOfNodes.Find(x => x.nodeID == nodeTemp.nodeID);
        if (node != null)
        {
            node.Arc = nodeArc;
            //node name
            node.nodeName = "Placeholder";
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
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", node.nodeID); }
    }

    /// <summary>
    /// Assign names to all nodes
    /// </summary>
    private void InitialiseDistrictNames()
    {
        TextList districtNames = city.districtNames;
        Dictionary<Node, float> tempDict = new Dictionary<Node, float>();
        int index, nodeID, counter;
        bool isSuccess;
        float distance;
        if (districtNames != null)
        {
            List<string> listOfNames = new List<string>(districtNames.randomList);
            if (listOfNames != null)
            {
                if (listOfNames.Count >= city.Arc.size.numOfNodes)
                {
                    //randomly assign names
                    foreach (Node node in listOfNodes)
                    {
                        index = Random.Range(0, listOfNames.Count);
                        if (string.IsNullOrEmpty(listOfNames[index]) == false)
                        {
                            node.nodeName = listOfNames[index];
                            //delete record to avoid dupes
                            listOfNames.RemoveAt(index);
                        }
                        else { Debug.LogWarningFormat("Invalid name (Null or Empty) for listOfNames[{0}]", index); }
                    }
                    Vector3 refPos = new Vector3(0f, 0.5f, 0);
                    //loop nodes -> place in dict with node Key and distance from node to grid centre (0,0) as value
                    foreach (Node node in listOfNodes)
                    {
                        distance = Vector3.Distance(node.nodePosition, refPos);
                        //store record in dict
                        try
                        { tempDict.Add(node, distance); }
                        catch (ArgumentException)
                        { Debug.LogErrorFormat("Invalid entry (duplicate) for node {0}, {1}, ID {2}", node.nodeName, node.Arc.name, node.nodeID); }
                    }
                    if (tempDict.Count > 0)
                    {
                        //Mayors Office -> sort dictionary from closest to grid centre to furtherst
                        var sortedDictMayor = from entry in tempDict orderby entry.Value ascending select entry;
                        
                        /*//debug printout
                        foreach (var record in sortedDictMayor)
                        { Debug.LogFormat("Sorted -> Mayor: {0}, {1}, ID{2}, distance {3}{4}", record.Key.nodeName, record.Key.Arc.name, record.Key.nodeID, record.Value, "\n"); }*/

                        //find the first government node for the mayor's office (which will be closest to the centre)
                        foreach (var record in sortedDictMayor)
                        {
                            if (record.Key.Arc.name.Equals("GOVERNMENT") == true)
                            {
                                Debug.LogFormat("LevelManager.cs -> InitialiseDistrictNames: Mayor's office at {0}, {1}, ID {2}, distance {3}{4}", record.Key.nodeName, record.Key.Arc.name, record.Key.nodeID, record.Value, "\n");
                                record.Key.nodeName = "City Centre";
                                record.Key.specialName = "Town Hall";
                                GameManager.instance.cityScript.mayorDistrictID = record.Key.nodeID;
                                break;
                            }
                        }
                        //Airport and Seaport -> sort dictionary from furtherst from grid centre to closest
                        var sortedDictPort = from entry in tempDict orderby entry.Value descending select entry;

                        /*//debug printout
                        foreach (var record in sortedDictPort)
                        { Debug.LogFormat("Sorted -> Port: {0}, {1}, ID{2}, distance {3}{4}", record.Key.nodeName, record.Key.Arc.name, record.Key.nodeID, record.Value, "\n"); }*/

                        //find first couple of entries for ports (furtherst from centre)
                        counter = 0;
                        foreach (var record in sortedDictPort)
                        {
                            if (counter == 0)
                            {
                                //Airport
                                if (string.IsNullOrEmpty(city.airportDistrict) == false)
                                {
                                    Debug.LogFormat("LevelManager.cs -> InitialiseDistrictNames: Airport at {0}, {1}, ID {2}, distance {3}{4}", record.Key.nodeName, record.Key.Arc.name, record.Key.nodeID, record.Value, "\n");
                                    record.Key.nodeName = city.airportDistrict;
                                    record.Key.specialName = "Airport";
                                    GameManager.instance.cityScript.airportDistrictID = record.Key.nodeID;
                                }
                                else { Debug.LogWarning("Missing airportDistrict name"); }
                            }
                            if (counter == 1)
                            {
                                //Harbour
                                if (string.IsNullOrEmpty(city.harbourDistrict) == false)
                                {
                                    Debug.LogFormat("LevelManager.cs -> InitialiseDistrictNames: Harbour at {0}, {1}, ID {2}, distance {3}{4}", record.Key.nodeName, record.Key.Arc.name, record.Key.nodeID, record.Value, "\n");
                                    record.Key.nodeName = city.harbourDistrict;
                                    record.Key.specialName = "Harbour";
                                    GameManager.instance.cityScript.harbourDistrictID = record.Key.nodeID;
                                }
                                else { Debug.LogWarning("Missing harbourDistrict name (City may not have a Harbour)"); }
                                break;
                            }
                            counter++;
                        }
                        //Icon district
                        if (string.IsNullOrEmpty(city.iconDistrict) == false)
                        {
                            List<int> listOfSpecialDistricts = new List<int>();
                            nodeID = GameManager.instance.cityScript.mayorDistrictID;
                            if (nodeID > -1) { listOfSpecialDistricts.Add(nodeID); }
                            nodeID = GameManager.instance.cityScript.airportDistrictID;
                            if (nodeID > -1) { listOfSpecialDistricts.Add(nodeID); }
                            nodeID = GameManager.instance.cityScript.harbourDistrictID;
                            if (nodeID > -1) { listOfSpecialDistricts.Add(nodeID); }
                            isSuccess = false;
                            //randomly assign but check not an existing special district
                            do
                            {
                                Node node = listOfNodes[Random.Range(0, listOfNodes.Count)];
                                //check not a special node
                                if (listOfSpecialDistricts.Exists(x => x == node.nodeID) == false)
                                {
                                    //valid node for icon district (not a special district)
                                    Debug.LogFormat("LevelManager.cs -> InitialiseDistrictNames: Icon \"{0}\" district at {1}, {2}, ID {3}{4}", city.iconName, node.nodeName, node.Arc.name, node.nodeID, "\n");
                                    node.nodeName = city.iconDistrict;
                                    node.specialName = city.iconName;
                                    GameManager.instance.cityScript.iconDistrictID = node.nodeID;
                                    break;
                                }
                                counter++;
                                if (counter == 20)
                                { Debug.LogWarningFormat("LevelManager.cs -> InitialiseDistrictNames: Invalid icon District (Timed out on a count of {0})", counter); }
                            }
                            while (isSuccess == false && counter < 20);
                        }
                        else { Debug.LogWarning("Missing IconDistrict name"); }
                    }
                    else { Debug.LogError("Invalid tempDict (No records)"); }
                }
                else { Debug.LogError("Not enough district names for city"); }
            }
            else { Debug.LogError("Invalid DistrictNames.randomList (Null)"); }
        }
        else { Debug.LogError("Invalid TextList districtNames (Null)"); }
    }

    /// <summary>
    /// private subMethod for InitialiseNodeArcs to display node stats at various stages of initialisation. 'Header' gives text at start of display
    /// </summary>
    /// <param name="header"></param>
    private void DisplayNodeStats(string header, int numRecords)
    {
        string name;
        int num;
        Debug.LogFormat("LevelManager.cs -> InitialiseNodeArc: {0}{1}", header, "\n");
        for (int i = 0; i < numRecords; i++)
        {
            num = arrayOfNodeArcTotals[(int)NodeArcTally.Current, i];
            name = GameManager.instance.dataScript.GetNodeArc(i).name;
            Debug.Log(string.Format("  Node {0} total {1}{2}", name, num, "\n"));
        }
    }

    /// <summary>
    /// Returns a Random node Arc based on the numConnections that node has the requirements of the City (which can overide default data). Returns Null if a problem
    /// </summary>
    /// <param name="numConnections"></param>
    /// <returns></returns>
    private NodeArc GetNodeArcRandom(int numConnections) 
    {
        Debug.Assert(numConnections > 0, string.Format("Invalid nunConnections ({0})", numConnections));
        NodeArc tempArc = null; 
        List<NodeArc> tempList = new List<NodeArc>();
        int adjustedConnections = Mathf.Min(5, numConnections) - 1;
        tempList = listOfConnArcsDefault[adjustedConnections];
        tempArc = tempList[Random.Range(0, tempList.Count)];
        return tempArc;
    }


    //
    // - - - Assign Security Levels to Connections
    //

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


    //
    // - - - Debug - - -
    //

    /// <summary>
    /// city analysis
    /// </summary>
    /// <returns></returns>
    public string GetLevelAnalysis()
    {
        StringBuilder builder = new StringBuilder();
        //City data
        builder.AppendFormat(" {0}, {1}{2}{3}", city.name, city.country.name, "\n", "\n");
        builder.AppendFormat(" Size {0}, {1} districts ({2} rqd min #){3}", city.Arc.size.name, city.Arc.size.numOfNodes, city.Arc.size.minNum, "\n");
        builder.AppendFormat(" Spacing {0} ({1} min distance btwn nodes){2}", city.Arc.spacing.name, city.Arc.spacing.minDistance, "\n");
        builder.AppendFormat(" Connections {0} ({1}% chance of extra conn){2}", city.Arc.connections.name, city.Arc.connections.frequency, "\n");
        builder.AppendFormat(" Security {0} ({1}% chance of higher Security){2}{3}", city.Arc.security.name, city.Arc.security.chance, "\n", "\n");
        if (city.Arc.priority != null)
        { builder.AppendFormat(" Priority NodeArc {0} (50% of remaining){1}", city.Arc.priority.name, "\n"); }
        else { builder.AppendFormat(" Priority NONE (remaining all Random){0}", "\n"); }
        //node analysis
        builder.AppendFormat("{0} Node Analysis{1}{2}", "\n", "\n", "\n");
        for (int i = 0; i < GameManager.instance.dataScript.CheckNumOfNodeArcs(); i++)
        {
            NodeArc arc = GameManager.instance.dataScript.GetNodeArc(i);
            builder.Append(string.Format(" {0}  {1}{2}", arc.name, arrayOfNodeArcTotals[(int)NodeArcTally.Current, i], "\n"));
        }
        //graph analysis
        builder.AppendFormat("{0}{1} Graph Analysis{2}{3}", "\n", "\n", "\n", "\n");
        //graphAPI analysis data
        if (graph != null)
        {
            builder.Append(" MaxDegree:  " + Convert.ToString(graph.CalcMaxDegree()) + "\n");
            builder.Append(" AvgDegree:  " + Convert.ToString(graph.CalcAvgDegree()) + "\n");
            builder.Append(" SelfLoops:    " + Convert.ToString(graph.CalcSelfLoops()) + "\n\n");
        }
        else
        { Debug.LogError(" Graph is Null -> no analysis available"); }
        //base stats
        builder.Append(" NumNodes:  " + Convert.ToString(listOfNodes.Count) + "\n");
        builder.Append(" NumConns:  " + Convert.ToString(listOfConnections.Count) + "\n\n");
        builder.AppendFormat(" {0}", TestSearch());
        return builder.ToString();
    }

    
}
