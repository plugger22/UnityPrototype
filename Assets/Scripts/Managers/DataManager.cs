using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using gameAPI;
using Random = UnityEngine.Random;


/// <summary>
/// Data repositry class
/// </summary>
public class DataManager : MonoBehaviour
{
    //master info array
    private int[,] arrayOfNodes;                                                                //info array that uses -> index[NodeArcID, NodeInfo enum]
    private int[,] arrayOfTeams;                                                                //info array that uses -> index[TeamArcID, TeamInfo enum]
    private string[,] arrayOfQualities;                                                         //tags for actor qualities -> index[(int)Side, 3 Qualities]
    private List<List<Node>> listOfNodesByType = new List<List<Node>>();                        //List containing Lists of Nodes by type -> index[NodeArcID]

    //master lists 
    private List<ActorArc> listOfAllActorArcs = new List<ActorArc>();
    private List<Trait> listOfAllTraits = new List<Trait>();

    //for fast access
    private List<Target> listOfPossibleTargets = new List<Target>();                        //nodes that don't currently have any target
    private List<Target> listOfActiveTargets = new List<Target>();
    private List<Target> listOfLiveTargets = new List<Target>();
    private List<List<GameObject>> listOfActorNodes = new List<List<GameObject>>();         //sublists, one each of all the active nodes for each actor in the level

    //node choices (random archetypes) based on number of connections. O.K to have multiple instances of the same archetype in a list in order to tweak the probabilities.
    public List<NodeArc> listOfOneConnArcs = new List<NodeArc>();
    public List<NodeArc> listOfTwoConnArcs = new List<NodeArc>();
    public List<NodeArc> listOfThreeConnArcs = new List<NodeArc>();
    public List<NodeArc> listOfFourConnArcs = new List<NodeArc>();
    public List<NodeArc> listOfFiveConnArcs = new List<NodeArc>();

    //dictionaries
    private Dictionary<int, GameObject> dictOfNodeObjects = new Dictionary<int, GameObject>();      //Key -> nodeID, Value -> Node gameObject
    private Dictionary<int, Node> dictOfNodes = new Dictionary<int, Node>();                        //Key -> nodeID, Value -> Node
    private Dictionary<int, NodeArc> dictOfNodeArcs = new Dictionary<int, NodeArc>();               //Key -> nodeArcID, Value -> NodeArc
    private Dictionary<string, int> dictOfLookUpNodeArcs = new Dictionary<string, int>();           //Key -> nodeArc name, Value -> nodeArcID
    private Dictionary<int, ActorArc> dictOfActorArcs = new Dictionary<int, ActorArc>();            //Key -> actorArcID, Value -> ActorArc
    private Dictionary<int, Trait> dictOfTraits = new Dictionary<int, Trait>();                     //Key -> traitID, Value -> Trait
    private Dictionary<int, Action> dictOfActions = new Dictionary<int, Action>();                  //Key -> ActionID, Value -> Action
    private Dictionary<int, Effect> dictOfEffects = new Dictionary<int, Effect>();                  //Key -> effectID, Value -> ActionEffect
    private Dictionary<int, Target> dictOfTargets = new Dictionary<int, Target>();                  //Key -> targetID, Value -> Target
    private Dictionary<int, TeamArc> dictOfTeamArcs = new Dictionary<int, TeamArc>();               //Key -> teamID, Value -> Team
    private Dictionary<string, int> dictOfLookupTeamArcs = new Dictionary<string, int>();           //Key -> teamArc name, Value -> TeamArcID
    private Dictionary<int, Team> dictOfTeams = new Dictionary<int, Team>();                        //Key -> teamID, Value -> Team

    /// <summary>
    /// default constructor
    /// </summary>
    public void InitialiseEarly()
    {
        int counter = 0;
        int length;
        string path;
        //
        // - - - Node Arcs - - -
        //
        counter = 0;
        //get GUID of all SO Node Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var nodeArcGUID = AssetDatabase.FindAssets("t:NodeArc");
        foreach (var guid in nodeArcGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object nodeArcObject = AssetDatabase.LoadAssetAtPath(path, typeof(NodeArc));
            //assign a zero based unique ID number
            NodeArc nodeArc = nodeArcObject as NodeArc;
            nodeArc.NodeArcID = counter++;
            //generate a four letter (first 4 of name in CAPS) as a short form tag
            length = nodeArc.name.Length;
            length = length >= 4 ? 4 : length;
            nodeArc.NodeArcTag = nodeArc.name.Substring(0, length).ToUpper();
            //add to dictionary
            try
            { dictOfNodeArcs.Add(nodeArc.NodeArcID, nodeArc); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid NodeArc (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid NodeArc (duplicate) ID \"{0}\" for  \"{1}\"", counter, nodeArc.name)); }
            //add to lookup dictionary
            try
            { dictOfLookUpNodeArcs.Add(nodeArc.name, nodeArc.NodeArcID); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid NodeArc (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid NodeArc (duplicate) Name \"{0}\" for ID \"{1}\"", nodeArc.name, nodeArc.NodeArcID)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfNodeArcs has {0} entries{1}", counter, "\n"));
        //
        // - - - Traits - - -
        //
        counter = 0;
        //get GUID of all SO Trait Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var traitGUID = AssetDatabase.FindAssets("t:Trait");
        foreach (var guid in traitGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object traitObject = AssetDatabase.LoadAssetAtPath(path, typeof(Trait));
            //assign a zero based unique ID number
            Trait trait = traitObject as Trait;
            trait.TraitID = counter++;
            //add to dictionary
            try
            {
                dictOfTraits.Add(trait.TraitID, trait);
                //add to list
                listOfAllTraits.Add(trait);
            }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Trait (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Trait (duplicate) ID \"{0}\" for \"{1}\"", counter, trait.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfTraits has {0} entries{1}", counter, "\n"));
        //
        // - - - Actor Arcs - - 
        //
        counter = 0;
        //get GUID of all SO ActorArc Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var arcGUID = AssetDatabase.FindAssets("t:ActorArc");
        foreach (var guid in arcGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object arcObject = AssetDatabase.LoadAssetAtPath(path, typeof(ActorArc));
            //assign a zero based unique ID number
            ActorArc arc = arcObject as ActorArc;
            arc.ActorArcID = counter++;
            //generate a four letter (first 4 of name in CAPS) as a short form tag
            length = arc.actorName.Length;
            length = length >= 4 ? 4 : length;
            arc.ActorTag = arc.actorName.Substring(0, length).ToUpper();
            //add to dictionary
            try
            {
                dictOfActorArcs.Add(arc.ActorArcID, arc);
                //add to list
                listOfAllActorArcs.Add(arc);

            }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Actor Arc (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid actorArc (duplicate) ID \"{0}\" for \"{1}\"", counter, arc.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfActorArcs has {0} entries{1}", counter, "\n"));
        //
        // - - - Effects - - -
        //
        counter = 0;
        //get GUID of all SO Effect Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var effectsGUID = AssetDatabase.FindAssets("t:Effect");
        foreach (var guid in effectsGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object effectObject = AssetDatabase.LoadAssetAtPath(path, typeof(Effect));
            //assign a zero based Unique ID number
            Effect effect = effectObject as Effect;
            effect.EffectID = counter++;
            //add to dictionary
            try
            { dictOfEffects.Add(effect.EffectID, effect); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Effect (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Effect (duplicate) effectID \"{0}\" for \"{1}\"", counter, effect.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfEffects has {0} entries{1}", counter, "\n"));
        //
        // - - - Targets - - -
        //
        counter = 0;
        //get GUID of all SO Target Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var targetGUID = AssetDatabase.FindAssets("t:Target");
        foreach (var guid in targetGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object targetObject = AssetDatabase.LoadAssetAtPath(path, typeof(Target));
            //assign a zero based unique ID number
            Target target = targetObject as Target;
            //set data
            target.TargetID = counter++;
            target.TargetStatus = Status.Dormant;
            target.Timer = -1;
            target.InfoLevel = 1;
            target.IsKnownByAI = false;
            target.NodeID = -1;
            //add to dictionary
            try
            { dictOfTargets.Add(target.TargetID, target); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Target (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Target (duplicate) ID \"{0}\" for \"{1}\"", counter, target.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfTargets has {0} entries{1}", counter, "\n"));
        //
        // - - - Actions - - -
        //
        counter = 0;
        //get GUID of all SO Action Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var actionGUID = AssetDatabase.FindAssets("t:Action");
        foreach (var guid in actionGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object actionObject = AssetDatabase.LoadAssetAtPath(path, typeof(Action));
            //assign a zero based unique ID number
            Action action = actionObject as Action;
            //set data
            action.ActionID = counter++;
            //add to dictionary
            try
            { dictOfActions.Add(action.ActionID, action); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Action Arc (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid (duplicate) ActionID \"{0}\" for Action \"{1}\"", action.ActionID, action.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfActions has {0} entries{1}", counter, "\n"));
        //
        // - - - Team Arcs - - -
        //
        counter = 0;
        //get GUID of all SO Team Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var teamGUID = AssetDatabase.FindAssets("t:TeamArc");
        foreach (var guid in teamGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object teamObject = AssetDatabase.LoadAssetAtPath(path, typeof(TeamArc));
            //assign a zero based unique ID number
            TeamArc teamArc = teamObject as TeamArc;
            //set data
            teamArc.TeamArcID = counter++;
            //add to dictionary
            try
            { dictOfTeamArcs.Add(teamArc.TeamArcID, teamArc); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid TeamArc (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid TeamArc (duplicate) ID \"{0}\" for \"{1}\"", counter, teamArc.name)); }
            //add to lookup dictionary
            try
            { dictOfLookupTeamArcs.Add(teamArc.name, teamArc.TeamArcID); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Lookup TeamArc (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Lookup TeamArc (duplicate) ID \"{0}\" for \"{1}\"", counter, teamArc.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfTeamArcs has {0} entries{1}", counter, "\n"));
        //arrayOfTeams
        arrayOfTeams = new int[counter, (int)TeamInfo.Count];
        //
        // - - - Actor Qualities - - -
        //
        int numOfQualities = GameManager.instance.actorScript.numOfQualities;
        arrayOfQualities = new string[(int)Side.Count, numOfQualities];
        arrayOfQualities[(int)Side.Authority, 0] = "Influence";
        arrayOfQualities[(int)Side.Authority, 1] = "Support";
        arrayOfQualities[(int)Side.Authority, 2] = "Ability";
        arrayOfQualities[(int)Side.Resistance, 0] = "Connections";
        arrayOfQualities[(int)Side.Resistance, 1] = "Motivation";
        arrayOfQualities[(int)Side.Resistance, 2] = "Invisibility";
    }


    /// <summary>
    /// Stuff that is done after level Manager.SetUp
    /// </summary>
    public void InitialiseLate()
    {
        //arrayOfNodes -> contains all relevant info on nodes by type
        int[] tempArray = GameManager.instance.levelScript.GetNodeTypeTotals();
        arrayOfNodes = new int[tempArray.Length, (int)NodeInfo.Count];
        for (int i = 0; i < tempArray.Length; i++)
        {
            arrayOfNodes[i, 0] = tempArray[i];
        }
        //List of Nodes by Types -> each index has a list of all nodes of that NodeArc type
        int limit = GetNumOfNodeTypes();
        for(int i = 0; i < limit; i++)
        {
            List<Node> tempList = new List<Node>();
            listOfNodesByType.Add(tempList);
        }
        //Populate List of lists -> place node in the correct list
        foreach(var nodeObj in dictOfNodeObjects)
        {
            Node node = nodeObj.Value.GetComponent<Node>();
            listOfNodesByType[node.arc.NodeArcID].Add(node);
        }
        //
        // - - - Nodes - - -
        //
        int counter = 0;
        List<Node> tempNodeList = GameManager.instance.levelScript.GetListOfNodes();
        if (tempNodeList != null)
        {
            foreach (Node node in tempNodeList)
            {
                //add to dictionary
                try
                { dictOfNodes.Add(node.NodeID, node); counter++; }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Node (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Node (duplicate) ID \"{0}\" for  \"{1}\"", node.NodeID, node.name)); }
            }
            Debug.Log(string.Format("DataManager: Initialise -> dictOfNodes has {0} entries{1}", counter, "\n"));
        }
        else { Debug.LogError("Invalid listOfNodes (Null) from LevelManager"); }
        //
        // - - - Possible Targets - - -
        //
        foreach (var target in dictOfTargets)
        {
            //add to list pf Possible targets if a level 1 target & nodes of the required type are available
            if (target.Value.targetLevel == 1)
            {
                //add to list of Possible targets
                if (GetNodeInfo(target.Value.nodeArc.NodeArcID, NodeInfo.Number) > 0)
                { listOfPossibleTargets.Add(target.Value); }
                else
                {
                    Debug.Log(string.Format("DataManager: {0} has been ignored as there are no required node types present (\"{1}\"){2}",
                        target.Value.name, target.Value.nodeArc.name, "\n"));
                }
            }
        }
        //Actor Nodes
        UpdateActorNodes();
        //event listener
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
    }

    /// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeSide:
                UpdateActorNodes();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// sets up list of active nodes for each actor slot
    /// </summary>
    public void UpdateActorNodes()
    { listOfActorNodes = GameManager.instance.levelScript.GetListOfActorNodes(GameManager.instance.optionScript.PlayerSide);}

    //
    // - - - NodeArcs - - -
    //

    /// <summary>
    /// returns a random Arc from the appropriate list based on the number of Connections that the node has
    /// </summary>
    /// <param name="numConnections"></param>
    /// <returns></returns>
    public NodeArc GetRandomNodeArc(int numConnections)
    {
        NodeArc tempArc = null;
        List<NodeArc> tempList = null;
        switch(numConnections)
        {
            case 1:
                tempList = listOfOneConnArcs;
                break;
            case 2:
                tempList = listOfTwoConnArcs;
                break;
            case 3:
                tempList = listOfThreeConnArcs;
                break;
            case 4:
                tempList = listOfFourConnArcs;
                break;
            case 5:
            case 6:
            case 7:
            case 8:
                tempList = listOfFiveConnArcs;
                break;
            default:
                Debug.LogError("Invalid number of Connections " + numConnections);
                break;
        }
        tempArc = tempList[Random.Range(0, tempList.Count)];
        return tempArc;
    }

    /// <summary>
    /// Get number of NodeArcs in dictionary
    /// </summary>
    /// <returns></returns>
    public int GetNumNodeArcs()
    { return dictOfNodeArcs.Count; }

    /// <summary>
    /// returns NodeArc based on ID search of dict, Null if not found
    /// </summary>
    /// <param name="nodeArcID"></param>
    /// <returns></returns>
    public NodeArc GetNodeArc(int nodeArcID)
    {
        if (dictOfNodeArcs.ContainsKey(nodeArcID))
        { return dictOfNodeArcs[nodeArcID]; }
        else { Debug.LogWarning("Not found in Dict > nodeArcID " + nodeArcID);}
        return null;
    }

    /// <summary>
    /// returns nodeArcID for specified nodeArc name, eg. "Corporate". Returns '-1' if not found in lookup dictionary
    /// </summary>
    /// <param name="nodeArcName"></param>
    /// <returns></returns>
    public int GetNodeArcID(string nodeArcName)
    {
        if (dictOfLookUpNodeArcs.ContainsKey(nodeArcName))
        { return dictOfLookUpNodeArcs[nodeArcName]; }
        else { Debug.LogWarning(string.Format("Not found in Lookup NodeArcID dict \"{0}\"{1}", nodeArcName, "\n")); }
        return -1;
    }

    //
    // - - - Actor Related - - - 
    //

    /// <summary>
    /// returns a number of randomly selected ActorArcs
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public List<ActorArc> GetRandomActorArcs(int num, Side side)
    {
        //create a separate copy of ActorArcs list to use for randomly selection

        //List<ActorArc> tempMaster = new List<ActorArc>(listOfAllActorArcs);

        //filter for the required side
        List<ActorArc> tempMaster = new List<ActorArc>();
        IEnumerable<ActorArc> selectedActorArcs =
            from arc in listOfAllActorArcs
            where arc.side == side
            select arc;
        tempMaster = selectedActorArcs.ToList();

        //temp list for results
        List < ActorArc > tempList = new List<ActorArc>();
        //randomly select
        int index;
        for(int i = 0; i < num; i++)
        {
            index = Random.Range(0, tempMaster.Count);
            tempList.Add(tempMaster[index]);
            //remove from list to prevent being selected again
            tempMaster.RemoveAt(index);
        }
        return tempList;
    }


    /// <summary>
    /// return a random trait (could be good or bad)
    /// </summary>
    /// <returns></returns>
    public Trait GetRandomTrait()
    {
        return listOfAllTraits[Random.Range(0, listOfAllTraits.Count)];
    }

    //
    // - - - Actions - - -
    //

    /*/// <summary>
    /// add Actors and effects to dictionaries
    /// </summary>
    /// <param name="arrayOfActors"></param>
    public void AddActions(Actor[] arrayOfActors)
    {
        int counter = 0;
        foreach (Actor actor in arrayOfActors)
        {
            //add nodeAction
            if (actor.arc.nodeAction != null)
            {
                //assign dynamic ID
                actor.arc.nodeAction.ActionID = counter++;
                //add to dictionary (only adding actions that are present in the level with the current selection of Actors)
                try
                { dictOfActions.Add(actor.arc.nodeAction.ActionID, actor.arc.nodeAction); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Action Arc (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid (duplicate) ActionID \"{0}\" for Action \"{1}\"", actor.arc.nodeAction.ActionID, actor.arc.nodeAction.name)); }
            }
            //add webAction
            if (actor.arc.webAction != null)
            {
                //assign dynamic ID
                actor.arc.webAction.ActionID = counter++;
                //add to dictionary (only adding actions that are present in the level with the current selection of Actors)
                try
                { dictOfActions.Add(actor.arc.webAction.ActionID, actor.arc.webAction); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Action Arc (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid (duplicate) ActionID \"{0}\" for Action \"{1}\"", actor.arc.webAction.ActionID, actor.arc.webAction.name)); }
            }
        }
    }*/

    //
    // - - - Nodes - - -
    //

    /// <summary>
    /// returns a GameObject node from dictionary based on nodeID, Null if not found
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public GameObject GetNodeObject(int nodeID)
    {
        GameObject obj = null;
        if (dictOfNodeObjects.TryGetValue(nodeID, out obj))
        {
            return obj;
        }
        return null;
    }

    /// <summary>
    /// returns a Node from dictionary based on nodeID, Null if not found
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public Node GetNode(int nodeID)
    {
        Node node = null;
        if (dictOfNodes.TryGetValue(nodeID, out node))
        {
            return node;
        }
        return null;
    }

    /// <summary>
    /// Add node GameObject to dictOfNodeObjects
    /// </summary>
    /// <param name="nodeID"></param>
    /// <param name="nodeObj"></param>
    public void AddNodeObject(int nodeID, GameObject nodeObj)
    {
        try
        { dictOfNodeObjects.Add(nodeID, nodeObj); }
        catch (ArgumentNullException)
        { Debug.LogError("Invalid Node Object (Null)"); }
        catch (ArgumentException)
        { Debug.LogError(string.Format("Invalid (duplicate) nodeID \"{0}\" for Node \"{1}\"", nodeID, nodeObj.name)); }
    }


    public Dictionary<int, Node> GetAllNodes()
    { return dictOfNodes; }

    /// <summary>
    /// Get int data from Master node array
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public int GetNodeInfo(int nodeIndex, NodeInfo info)
    {
        Debug.Assert(nodeIndex > -1 && nodeIndex < GetNumOfNodeTypes(), "Invalid nodeIndex");
        return arrayOfNodes[nodeIndex, (int)info];
    }

    /// <summary>
    /// Change data in node info array
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="info"></param>
    /// <param name="newData"></param>
    public void SetNodeInfo(int nodeIndex, NodeInfo info, int newData)
    {
        Debug.Assert(nodeIndex > -1 && nodeIndex < GetNumOfNodeTypes(), "Invalid nodeIndex");
        arrayOfNodes[nodeIndex, (int)info] = newData;
    }

    /// <summary>
    /// return total number of nodes in the level
    /// </summary>
    /// <returns></returns>
    public int GetNumOfNodes()
    { return dictOfNodeObjects.Count; }

    /// <summary>
    /// returns number of different node arc types on level, eg. "Corporate" + "Utility" would return 2
    /// </summary>
    /// <returns></returns>
    public int GetNumOfNodeTypes()
    { return arrayOfNodes.Length; }


    /// <summary>
    /// return a list of Nodes, all of which are the same type (nodeArcID)
    /// </summary>
    /// <param name="nodeArcID"></param>
    /// <returns></returns>
    public List<Node> GetListOfNodesByType(int nodeArcID)
    {
        Debug.Assert(nodeArcID > -1 && nodeArcID < GetNumOfNodeTypes(), "Invalid nodeArcID parameter");
        return listOfNodesByType[nodeArcID];
    }

    /// <summary>
    /// returns a Random node of a particular NodeArc type, or (by default) ANY random node. Returns null if a problem.
    /// </summary>
    /// <param name="nodeArcID"></param>
    /// <returns></returns>
    public Node GetRandomNode(int nodeArcID = -1)
    {
        Node node = null;
        int key;
        if (nodeArcID == -1)
        {
            //return a Random Node (ANY)
            List<int> keyList = new List<int>(dictOfNodes.Keys);
            key = keyList[Random.Range(0, keyList.Count)];
            node = GetNode(key);
        }
        else
        {
            //return a random node of a specific nodeArc type
            List<Node> nodeList = GetListOfNodesByType(nodeArcID);
            //no go if no nodes of that type present in scene
            if (nodeList != null && nodeList.Count > 0)
            {
                //return a Random Node (specific nodeArc type)
                node = nodeList[Random.Range(0, nodeList.Count)];
            }
            else
            {
                //return a Random Node (ANY)
                List<int> keyList = new List<int>(dictOfNodes.Keys);
                key = keyList[Random.Range(0, keyList.Count)];
                node = GetNode(key);
                Debug.LogWarning(string.Format("Alert: nodeList is either Null or Count Zero for nodeArcID \"{0}\", {1}{2}", 
                    nodeArcID, GetNodeArc(nodeArcID), "\n"));
            }
        }
        return node;
    }


    //
    // - - - Targets - -  -
    //

    /// <summary>
    /// returns a Target from dictionary based on TargetID key, null if not found
    /// </summary>
    /// <param name="targetID"></param>
    /// <returns></returns>
    public Target GetTarget(int targetID)
    {
        Target target = null;
        if (dictOfTargets.TryGetValue(targetID, out target))
        {
            return target;
        }
        return null;
    }

    public List<Target> GetPossibleTargets()
    { return listOfPossibleTargets; }

    public int GetNumOfPossibleTargets()
    { return listOfPossibleTargets.Count; }

    public Dictionary<int, Target> GetDictOfTargets()
    { return dictOfTargets; }


    public List<Target> GetLiveTargets()
    { return listOfLiveTargets; }


    public void AddActiveTarget(Target target)
    {
        if (target != null)
        { listOfActiveTargets.Add(target); }
        else { Debug.LogError("Invalid Active Target parameter (Null)"); }
    }

    public void AddLiveTarget(Target target)
    {
        if (target != null)
        { listOfLiveTargets.Add(target); }
        else { Debug.LogError("Invalid Live Target parameter (Null)"); }
    }

    //
    // - - - Actor Nodes - - -
    //

    /// <summary>
    /// return a list of all nodes where an actor (slotID) is active
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public List<GameObject> GetListOfActorNodes(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.numOfActorsTotal, "Invalid slotID");
        return listOfActorNodes[slotID];
    }

    //
    // - - - Teams & TeamArcs - - -
    //

    public int GetNumOfTeamTypes()
    { return dictOfTeamArcs.Count; }

    /// <summary>
    /// returns int data from arrayOfTeams based on teamArcID and TeamInfo enum
    /// </summary>
    /// <param name="teamArcID"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public int GetTeamInfo(int teamArcID, TeamInfo info)
    {
        Debug.Assert(teamArcID > -1 && teamArcID < GetNumOfTeamTypes(), "Invalid teamArcID");
        return arrayOfTeams[teamArcID, (int)info];
    }

    /// <summary>
    /// change a data point to a new value in array based on teamArcID and TeamInfo enum
    /// </summary>
    /// <param name="teamArcID"></param>
    /// <param name="info"></param>
    /// <param name="newData">new value of data</param>
    public void SetTeamInfo(int teamArcID, TeamInfo info, int newData)
    {
        Debug.Assert(teamArcID > -1 && teamArcID < GetNumOfTeamTypes(), "Invalid teamArcID");
        arrayOfTeams[teamArcID, (int)info] = newData;
    }

    /// <summary>
    /// adjust a data point by the input amount, eg. +1, -2, etc. Min capped at 0.
    /// </summary>
    /// <param name="teamArcID"></param>
    /// <param name="info"></param>
    /// <param name="adjustment"></param>
    public void AdjustTeamInfo(int teamArcID, TeamInfo info, int adjustment)
    {
        Debug.Assert(teamArcID > -1 && teamArcID < GetNumOfTeamTypes(), "Invalid teamArcID");
        int afterValue = arrayOfTeams[teamArcID, (int)info] + adjustment;
        arrayOfTeams[teamArcID, (int)info] = Math.Max(0, afterValue);
    }

    /// <summary>
    /// returns TeamArcID of named teamArc type. returns '-1' if not found in dict
    /// </summary>
    /// <param name="teamArcName"></param>
    /// <returns></returns>
    public int GetTeamArcID(string teamArcName)
    {
        if (dictOfLookupTeamArcs.ContainsKey(teamArcName))
        { return dictOfLookupTeamArcs[teamArcName]; }
        else { Debug.LogWarning(string.Format("Not found in Lookup TeamArcID dict \"{0}\"{1}", teamArcName, "\n")); }
        return -1;
    }

    /// <summary>
    /// returns TeamArc based on teamArcID, null if not found in dictionary
    /// </summary>
    /// <param name="teamArcID"></param>
    /// <returns></returns>
    public TeamArc GetTeamArc(int teamArcID)
    {
        if (dictOfTeamArcs.ContainsKey(teamArcID))
        { return dictOfTeamArcs[teamArcID]; }
        else { Debug.LogWarning(string.Format("Not found inTeamArcID {0}, in dict {1}", teamArcID, "\n")); }
        return null;
    }

    /// <summary>
    /// Add team to dictOfTeams
    /// </summary>
    /// <param name="team"></param>
    public void AddTeam(Team team)
    {
        //add to dictionary
        try
        { dictOfTeams.Add(team.TeamID, team); }
        catch (ArgumentNullException)
        { Debug.LogError("Invalid Team (Null)"); }
        catch (ArgumentException)
        { Debug.LogError(string.Format("Invalid Team (duplicate) TeamID \"{0}\" for {1} \"{2}\"{3}", team.TeamID, team.arc.name, team.Name, "\n")); }
    }

    /// <summary>
    /// returns and array of strings for actor quality tags, eg. "Connections, Invisibility" etc.
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public string[] GetQualities(Side side)
    {
        int numOfQualities = GameManager.instance.actorScript.numOfQualities;
        string[] tempArray = new string[numOfQualities];
        for (int i = 0; i < numOfQualities; i++)
        {
            tempArray[i] = arrayOfQualities[(int)side, i];
        }
        return tempArray;
    }

    /// <summary>
    /// returns a single string quality tag, eg. "Invisibility". Corresponds to side and qualityNumber, eg. Datapoint0 = 0, Datapoint1 = 1, Datapoint2 = 2
    /// </summary>
    /// <param name="side"></param>
    /// <param name="qualityNum"></param>
    /// <returns></returns>
    public string GetQuality(Side side, int qualityNum)
    {
        Debug.Assert(qualityNum > -1 && qualityNum < GameManager.instance.actorScript.numOfQualities, "Invalid qualityNum");
        return arrayOfQualities[(int)side, qualityNum];
    }

   //new methods above here
}


