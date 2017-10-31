using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using GraphAPI;
using gameAPI;
using System.Text;

public class LevelManager : MonoBehaviour
{
    public GameObject node;             //node prefab
    public GameObject connection;       //connection prefab
    public LayerMask blockingLayer;     //nodes are on the blocking layer, not connections
    public int numOfNodes;              //number of nodes (adjusted after use in InitialiseNodes() to reflect actual number)
    public float minSpacing;            //minimum spacing (world units) between nodes (>=)
    [Tooltip("Random % chance of a node having additional connections")]
    public int randomConnection;        //% chance of a node gaining a random additional connection after the MST set-up      

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

    //collections (all indexes correspond throughout, eg. listOfNodes[2] = listOfCoordinates[2] = listOfSortedDistances[2])
    //Note: collections initialised here as GameManager.SetUpScene will run prior to the Awake callback here
    private List<GameObject> listOfNodeObjects = new List<GameObject>();
    private List<Node> listOfNodes = new List<Node>();                              //mirror list to listOfNodeObjects but Nodes instead of GO's for speed of use
    private List<GameObject> listOfConnections = new List<GameObject>();
    private List<Vector3> listOfCoordinates = new List<Vector3>();                  //used to provide a lookup to check spacing of nodes
    private List<List<int>> listOfSortedNodes = new List<List<int>>();              //each node has a sorted (closest to furthest) list of nodeID's of neighouring nodes
    private List<List<float>> listOfSortedDistances = new List<List<float>>();    //companion list to listOfSortedNodes (identical indexes) -> contains distances to node in other list in world units    
     
    private List<List<GameObject>> listOfActorNodesAuthority = new List<List<GameObject>>();        //list containing sublists, one each of all the active nodes for each actor in the level
    private List<List<GameObject>> listOfActorNodesResistance = new List<List<GameObject>>();       //need a separate list for each side   

    private int[] arrayOfNodeTypeTotals;                                                    //array of how many of each node type there is on the map
    private bool[,,] arrayOfActiveNodes;                                                  //[total nodes, side, total actors] true if node active for that actor
    
    /// <summary>
    /// Master method that drives a level
    /// </summary>
    public void Initialise()
    {
        InitialiseNodes(numOfNodes, minSpacing);
        InitialiseSortedDistances();
        RemoveInvalidNodes();
        InitialiseGraph();
        AssignNodeArcs();
        AssignSecurityLevels();
        InitialiseArrayOfActiveNodes();
        AssignActorsToNodes(Side.Authority);
        AssignActorsToNodes(Side.Resistance);
        EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Redraw);
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
                nodeTemp.NodeID = GameManager.instance.nodeScript.nodeCounter++;
                //add to node list & add to coord list for lookups
                listOfNodeObjects.Add(instance);
                listOfNodes.Add(nodeTemp);
                listOfCoordinates.Add(randomPos);
                //add to dictionary of Nodes
                GameManager.instance.dataScript.AddNodeObject(nodeTemp.NodeID, instance);
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
        if (nodeStart.CheckNeighbours(posEnd) == true) { return true; }
        if (nodeEnd.CheckNeighbours(posStart) == true) { return true; }
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
        //add to list
        listOfConnections.Add(instanceConnection);
        //add to neighbours list
        nodeStart = listOfNodeObjects[node1].GetComponent<Node>();
        nodeEnd = listOfNodeObjects[node2].GetComponent<Node>();
        nodeStart.AddNeighbour(pos2);
        nodeEnd.AddNeighbour(pos1);
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
            if (Random.Range(0, 100) <= randomConnection)
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
                            PlaceConnection(v, other, vOne, vTwo, ConnectionType.HighSec);
                        }
                    }
                }
            }
        }
    }
    #endregion

    //
    // --- Show Active Nodes
    //
    #region Show Nodes




    #endregion

    //
    // - - - Redraw methods (GameManager)
    //
    #region Redraw and Rest graph






    /*public void RedrawConnections()
    {
        Renderer renderer;
        foreach(GameObject obj in listOfConnections)
        {
            Connection connection = obj.GetComponent<Connection>();
            renderer = obj.GetComponent<Renderer>();
            renderer.material = connection.
        }
    }*/

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
        for (int i = 0; i < GameManager.instance.dataScript.GetNumNodeArcs(); i++)
        {
            NodeArc arc = GameManager.instance.dataScript.GetNodeArc(i);
            builder.Append(string.Format("{0}  {1}{2}", arc.name, arrayOfNodeTypeTotals[i], "\n"));
        }
        return builder.ToString();
    }

    /// <summary>
    /// returns a string made up of Actors data
    /// </summary>
    /// <returns></returns>
    public string GetActorAnalysis(Side side)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("Actor Analysis" + "\n\n");
        Actor[] arrayOfActors = GameManager.instance.actorScript.GetActors(GameManager.instance.optionScript.PlayerSide);
        //loop actors
        foreach (Actor actor in arrayOfActors)
        {
            builder.Append(string.Format("{0}   C: {1} star{2}", actor.arc.actorName, actor.Datapoint0, "\n"));
            switch (side)
            {
                case Side.Authority:
                    builder.Append(string.Format("P: {0}  E: {1}  A: {2}{3}", actor.arc.listPrefPrimary[0].name, actor.arc.listPrefExclude[0].name,
                        listOfActorNodesAuthority[actor.SlotID].Count, "\n\n"));
                    break;
                case Side.Resistance:
                    builder.Append(string.Format("P: {0}  E: {1}  A: {2}{3}", actor.arc.listPrefPrimary[0].name, actor.arc.listPrefExclude[0].name,
                        listOfActorNodesResistance[actor.SlotID].Count, "\n\n"));
                    break;
                default:
                    Debug.LogError(string.Format("Invalid side \"{0}\"", side));
                    break;
            }
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
                conn.ChangeSecurityLevel(ConnectionType.Neutral);
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
                conn.ChangeSecurityLevel(ConnectionType.Neutral);
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
            PlaceConnection(idTwo, idOne, vOne, vTwo, ConnectionType.LowSec);
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
        int numRecords = GameManager.instance.dataScript.GetNumNodeArcs();
        //array of counters
        arrayOfNodeTypeTotals = new int[numRecords];
        //loop list of nodes
        foreach (GameObject obj in listOfNodeObjects)
        {
            Node node = obj.GetComponent<Node>();
            int numNodes = node.GetNumOfNeighbours();
            //node name
            node.NodeName = "Placeholder";
            //get random node Arc from appropriate list
            node.arc = GameManager.instance.dataScript.GetRandomNodeArc(numNodes);
            //provide base level stats 
            node.Stability = node.arc.Stability;
            node.Support = node.arc.Support;
            node.Security = node.arc.Security;
            //keep within range of 0 to 3
            node.Stability = node.Stability > 3 ? 3 : node.Stability;
            node.Stability = node.Stability < 0 ? 0 : node.Stability;
            node.Support = node.Support > 3 ? 3 : node.Support;
            node.Support = node.Support < 0 ? 0 : node.Support;
            node.Security = node.Security > 3 ? 3 : node.Security;
            node.Security = node.Security < 0 ? 0 : node.Security;
            //target -> none
            node.TargetID = -1;
            //keep a tally of how many of each type have been generated
            index = node.arc.NodeArcID;
            if (index < numRecords)
            { arrayOfNodeTypeTotals[index]++; }
            else { Debug.LogError(string.Format("Number of NodeArcs exceeded by nodeArcID {0} for Node {1}", index, node.arc.name)); }
        }
        //Display stats
        string name;
        int num;
        Debug.Log("Node Summary" + "\n");
        for (int i = 0; i < arrayOfNodeTypeTotals.Length; i++)
        {
            num = arrayOfNodeTypeTotals[i];
            if (num > 0)
            {
                name = GameManager.instance.dataScript.GetNodeArc(i).name;
                Debug.Log(string.Format("Node {0} total {1}{2}", name, num, "\n"));
            }
        }
    }

    /// <summary>
    /// sets up array prior to use (needed because AssignActorsToNodes is called for each side and each instance would overwrite the previous sides data)
    /// </summary>
    private void InitialiseArrayOfActiveNodes()
    {
        //initialise arrayOfActiveNodes prior to use
        arrayOfActiveNodes = new bool[listOfNodeObjects.Count, (int)Side.Count, GameManager.instance.actorScript.numOfActorsTotal];
    }

    /// <summary>
    /// sets up nodes to be active, or not, for each of the selected actors
    /// </summary>
    /// <param name="arrayOfActors"></param>
    private void AssignActorsToNodes(Side side)
    {
        Actor[] arrayOfActors = GameManager.instance.actorScript.GetActors(side);           //the four, or less, available actors for the level
        int[] arrayOfArcs = new int[GameManager.instance.dataScript.GetNumNodeArcs()];    //array of int's, one for each possible NodeArcID, used to contain % chance of node being active for each node type
        List<NodeArc> listOfNodeArcs = new List<NodeArc>();                 //temp list of NodeArcs used for an Actors primary and exclude nodeArc preference

        int primary = GameManager.instance.nodeScript.nodePrimaryChance;               //% chance, times actor.Ability, of node being active for this actor -> Primary Node
        int secondary = primary / 2;                                        //% chance, times actor.Ability, of node being active for this actor -> Secondary Node
        int minimumNumOfNodes = GameManager.instance.nodeScript.nodeActiveMinimum;     //minimum number of active nodes that should be on map for any given actor type
        int chance;                                                         //% chance, primary, or secondary, times actor ability
        int counter;                                                        //counts number of active nodes for this actor
        int nodeIndex, actorIndex;
        int length = arrayOfActors.Length;
        

        if (arrayOfActors != null)
        {
            if (length > 0)
            {
                //initialise array of Node lists
                List<GameObject>[] arrayOfNodeLists = new List<GameObject>[length];
                for (int i = 0; i < length; i++)
                { arrayOfNodeLists[i] = new List<GameObject>(); }

                //loop Actors
                for (actorIndex = 0; actorIndex < arrayOfActors.Length; actorIndex++)
                {
                    Actor actor = arrayOfActors[actorIndex];
                    //populate arrayOfArcs with default secondary chance of being active
                    chance = secondary * actor.Datapoint0;
                    for (int j = 0; j < arrayOfArcs.Length; j++)
                    { arrayOfArcs[j] = chance; }
                    //get actors Primary NodeArc preferences
                    listOfNodeArcs.Clear();
                    listOfNodeArcs.AddRange(actor.arc.listPrefPrimary);
                    foreach(NodeArc arc in listOfNodeArcs)
                    {
                        chance = primary * actor.Datapoint0;
                        arrayOfArcs[arc.NodeArcID] = chance;
                        Debug.Log(string.Format("{0}  primary NodeArc \"{1}\" -> nodeArcID {2}, side {3}{4}", actor.arc.actorName, arc.name, arc.NodeArcID, side, "\n"));
                    }
                    //get actors Exclusion NodeArc preferences
                    listOfNodeArcs.Clear();
                    listOfNodeArcs.AddRange(actor.arc.listPrefExclude);
                    foreach (NodeArc arc in listOfNodeArcs)
                    {
                        arrayOfArcs[arc.NodeArcID] = 0;
                        Debug.Log(string.Format("{0}  Exclusion NodeArc \"{1}\" -> nodeArcID {2}, side {3}{4}", actor.arc.actorName, arc.name, arc.NodeArcID, side, "\n"));
                    }
                    //debug summary
                    Debug.Log(string.Format("{0} {1} - {2} - {3} - {4} - {5} - {6}, side {7}{8}", actor.arc.actorName, arrayOfArcs[0], arrayOfArcs[1], arrayOfArcs[2], 
                        arrayOfArcs[3], arrayOfArcs[4], arrayOfArcs[5], side, "\n"));

                    //loop nodes and check if any are active
                    counter = 0;
                    for(nodeIndex = 0; nodeIndex < listOfNodeObjects.Count; nodeIndex++)
                    {
                        Node node = listOfNodeObjects[nodeIndex].GetComponent<Node>();
                        //node active?
                        if (Random.Range(0, 100) < arrayOfArcs[node.arc.NodeArcID])
                        {
                            counter++;
                            arrayOfActiveNodes[nodeIndex, (int)side, actorIndex] = true;
                            //add to list of Active nodes for that actor
                            arrayOfNodeLists[actorIndex].Add(listOfNodeObjects[nodeIndex]);
                        }
                    }
                    Debug.Log(string.Format("First Pass -> {0} is active in {1} nodes, side {2}{3}", actor.arc.actorName, counter, side, "\n"));
                    bool checkFlag;
                    int modifier = 0;
                    //check minimum number of active nodes has been achieved
                    if (counter < minimumNumOfNodes)
                    {
                        //need to add more active nodes -> keep looping until minimum obtained
                        do
                        {
                            //prevents endless loop situation where there are no viable nodes to test
                            checkFlag = false;
                            for (nodeIndex = 0; nodeIndex < listOfNodeObjects.Count; nodeIndex++)
                            {
                                Node node = listOfNodeObjects[nodeIndex].GetComponent<Node>();
                                //check node not already active
                                if (arrayOfActiveNodes[nodeIndex, (int)side, actorIndex] == false)
                                {
                                    //chance of node being active (increase the odds of this happening)
                                    if ((Random.Range(0, 100) - modifier) < arrayOfArcs[node.arc.NodeArcID])
                                    {
                                        counter++;
                                        arrayOfActiveNodes[nodeIndex, (int)side, actorIndex] = true;
                                        //add to list of Active ndoes for that actor
                                        arrayOfNodeLists[actorIndex].Add(listOfNodeObjects[nodeIndex]);
                                    }
                                    else
                                    {
                                        //failed test but there was at least one viable node
                                        checkFlag = true;
                                        //gradually increase the odds of a positive outcome
                                        modifier += 2;
                                    }
                                }
                                //reached the minimum?
                                if (counter >= minimumNumOfNodes) { break; }
                            }

                            //message once per complete node loop
                            if (counter < minimumNumOfNodes)
                            { Debug.LogWarning(string.Format("One complete loop of Nodes completed trying to attain the minimum numer of Nodes, modifier {0}, side {1}{2}", 
                                modifier, side, "\n")); }
                        }
                        while (counter < minimumNumOfNodes && checkFlag == true);

                        Debug.Log(string.Format("Later Passes -> {0} is active in {1} nodes, side {2}{3}", actor.arc.actorName, counter, side, "\n"));
                    }
                }
                //transfer lists of nodes across
                for (int i = 0; i < length; i++)
                {
                    switch (side)
                    {
                        case Side.Authority:
                            listOfActorNodesAuthority.Add(new List<GameObject>(arrayOfNodeLists[i]));
                            break;
                        case Side.Resistance:
                            listOfActorNodesResistance.Add(new List<GameObject>(arrayOfNodeLists[i]));
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid side \"{0}\"", side));
                            break;
                    }
                }
            }
            else { Debug.LogError(string.Format("No Actors within arrayOfActors -> Nodes not initialised for Actor use, side {0}", side)); }
        }
        else { Debug.LogError(string.Format("Invalid arrayOfActors (Null) -> Nodes not initialised for Actor use, side {0}", side)); }
    }

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
        int chance = GameManager.instance.connScript.connectionSecurityChance;
        int node1, node2, security = 0;       //node ID's either end of connection
        //set all to default level
        ChangeAllConnections(ConnectionType.Neutral);
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



    /// <summary>
    /// returns true if a given nodeID is active for the specified actor slotID (0 to 3)
    /// </summary>
    /// <param name="nodeID"></param>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public bool CheckNodeActive(int nodeID, Side side, int slotID)
    {
        Debug.Assert(nodeID > -1 && nodeID < numOfNodes, "Invalid nodeID input");
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.numOfActorsTotal, "Invalid slotID input");
        return arrayOfActiveNodes[nodeID, (int)side, slotID];
    }

    public int[] GetNodeTypeTotals()
    { return arrayOfNodeTypeTotals; }

    public List<Node> GetListOfNodes()
    { return listOfNodes; }

    /// <summary>
    /// returns an empty list if an incorrect side is provided
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public List<List<GameObject>> GetListOfActorNodes(Side side)
    {
        List<List<GameObject>> tempList = new List<List<GameObject>>();
        switch (side)
        {
            case Side.Authority:
                tempList = listOfActorNodesAuthority;
                break;
            case Side.Resistance:
                tempList = listOfActorNodesResistance;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side));
                break;
        }
        return tempList;
    }

}
