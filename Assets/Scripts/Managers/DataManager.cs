using gameAPI;
using packageAPI;
using dijkstraAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


/// <summary>
/// Data repositry class
/// </summary>
public class DataManager : MonoBehaviour
{

    //NOTE: some arrays are initialised by ImportManager.cs making a call to DataManager methods due to sequencing issues
    //master info array
    private int[,] arrayOfNodes;                                                                //info array that uses -> index[NodeArcID, NodeInfo enum]
    private int[,] arrayOfTeams;                                                                //info array that uses -> index[TeamArcID, TeamInfo enum]
    private Actor[,] arrayOfActors;                                                             //array with two sets of 4 actors, one for each side (Side.None->4 x Null)
    private bool[,] arrayOfActorsPresent;                                                       //array determining if an actorSlot is filled (True) or vacant (False)
    private string[,] arrayOfStatTags;                                                          //tags for actor stats -> index[(int)Side, 3 Qualities]

    //Nodes
    private List<Node> listOfNodes = new List<Node>();                                          //main list of nodes used for iteration (rather than dictOfNodes)
    private List<List<Node>> listOfNodesByType = new List<List<Node>>();                        //List containing Lists of Nodes by type -> index[NodeArcID]
    private List<Node> listOfMostConnectedNodes = new List<Node>();                             //top connected nodes (3+ connections), used by AI for ProcessSpiderTeam
    private List<Node> listOfDecisionNodes = new List<Node>();                                  //dynamic list of nodes used for connection security level decisions
    private List<Node> listOfLoiterNodes = new List<Node>();                                    //nodes where the nemesis can go to and wait until something happens
    private List<Node> listOfCureNodes = new List<Node>();
    private List<Node> listOfCrisisNodes = new List<Node>();
    private List<NodeCrisis> listOfCrisisSecurity = new List<NodeCrisis>();
    private List<NodeCrisis> listOfCrisisSupport = new List<NodeCrisis>();
    private List<NodeCrisis> listOfCrisisStability = new List<NodeCrisis>();

    //Connections
    private List<Connection> listOfConnections = new List<Connection>();                       //main list of connections used for iteration (rather than dictOfConnections)

    //Actors
    private List<TextMeshProUGUI> listOfActorTypes = new List<TextMeshProUGUI>();               //actors (not player)
    private List<Image> listOfActorPortraits = new List<Image>();                               //actors (not player)

    //actor quality input arrays (used to populate arrayOfQualities)
    private Quality[] authorityQualities;
    private Quality[] resistanceQualities;

    //team pools
    private List<int> teamPoolReserve = new List<int>();
    private List<int> teamPoolOnMap = new List<int>();
    private List<int> teamPoolInTransit = new List<int>();

    //actor pools
    private List<int> authorityActorPoolLevelOne = new List<int>();
    private List<int> authorityActorPoolLevelTwo = new List<int>();
    private List<int> authorityActorPoolLevelThree = new List<int>();
    private List<int> authorityActorReserve = new List<int>();
    private List<int> authorityActorDismissed = new List<int>();
    private List<int> authorityActorPromoted = new List<int>();
    private List<int> authorityActorDisposedOf = new List<int>();
    private List<int> authorityActorResigned = new List<int>();

    private List<int> resistanceActorPoolLevelOne = new List<int>();
    private List<int> resistanceActorPoolLevelTwo = new List<int>();
    private List<int> resistanceActorPoolLevelThree = new List<int>();
    private List<int> resistanceActorReserve = new List<int>();
    private List<int> resistanceActorDismissed = new List<int>();
    private List<int> resistanceActorPromoted = new List<int>();
    private List<int> resistanceActorDisposedOf = new List<int>();
    private List<int> resistanceActorResigned = new List<int>();

    //target pools
    private List<int>[] arrayOfGenericTargets;                                          //indexed by NodeArc.nodeArcID, list Of targetID's for each nodeArc type. All level one targets
    //private List<Target> possibleTargetsPool = new List<Target>();                        //level 1 target and node of the correct type available
    private List<Target> targetPoolActive = new List<Target>();                         //targets onMap but not yet visible to resistance player
    private List<Target> targetPoolLive = new List<Target>();                           //targets OnMap and visible to resistance player
    private List<Target> targetPoolOutstanding = new List<Target>();                       //completed targets that authority has not yet contained (shuts down onging Effects)
    private List<Target> targetPoolDone = new List<Target>();                    //targets done with (no longer coming back to the map, various reasons)
    private List<int> listOfNodesWithTargets = new List<int>();                         //list of all nodes which currently have non-dormant targets

    //contacts (resistance)
    private List<int> contactPool = new List<int>();

    //master lists 
    private List<ActorArc> authorityActorArcs = new List<ActorArc>();
    private List<ActorArc> resistanceActorArcs = new List<ActorArc>();
    private List<Trait> listOfAllTraits = new List<Trait>();

    private List<int> listOfMoveNodes = new List<int>();                                    //nodeID's of all valid node move options from player's current position

    //node choices (random archetypes) based on number of connections. O.K to have multiple instances of the same archetype in a list in order to tweak the probabilities.
    //NOTE: Public because data is added in Editor -> default version (each city has it's own set, use default if cities version is null)
    [Header("Default Node Mix")]
    public List<NodeArc> listOfOneConnArcsDefault = new List<NodeArc>();
    public List<NodeArc> listOfTwoConnArcsDefault = new List<NodeArc>();
    public List<NodeArc> listOfThreeConnArcsDefault = new List<NodeArc>();
    public List<NodeArc> listOfFourConnArcsDefault = new List<NodeArc>();
    public List<NodeArc> listOfFiveConnArcsDefault = new List<NodeArc>();
    //which nodeArcs like which connections (no dupes in a single list but a nodeArc can be in multiple lists)
    [Header("Preferred Connections for NodeArcs")]
    public List<NodeArc> listOfOneConnArcsPreferred = new List<NodeArc>();
    public List<NodeArc> listOfTwoConnArcsPreferred = new List<NodeArc>();
    public List<NodeArc> listOfThreeConnArcsPreferred = new List<NodeArc>();
    public List<NodeArc> listOfFourConnArcsPreferred = new List<NodeArc>();
    public List<NodeArc> listOfFiveConnArcsPreferred = new List<NodeArc>();

    //manage actor choices
    private List<ManageAction> listOfActorHandle = new List<ManageAction>();
    private List<ManageAction> listOfActorReserve = new List<ManageAction>();
    private List<ManageAction> listOfActorDismiss = new List<ManageAction>();
    private List<ManageAction> listOfActorDispose = new List<ManageAction>();

    //gear lists (available gear for this level) -> gearID's
    private List<GearRarity> listOfGearRarity = new List<GearRarity>();
    private List<GearType> listOfGearType = new List<GearType>();
    private List<int> listOfCommonGear = new List<int>();
    private List<int> listOfRareGear = new List<int>();
    private List<int> listOfUniqueGear = new List<int>();
    private List<int> listOfLostGear = new List<int>();
    private List<int> listOfCurrentGear = new List<int>();                                          //gear held by OnMap resistance player or actors

    //secret lists
    private List<Secret> listOfPlayerSecrets = new List<Secret>();
    private List<Secret> listOfRevealedSecrets = new List<Secret>();
    private List<Secret> listOfDeletedSecrets = new List<Secret>();                                 //secrets that have been scrubbed without being revealed

    //AI persistant data
    private int[] arrayOfAIResources = new int[3];                                                  //Global side index [0] none, [1] Authority, [2] Resistance
    private Queue<AITracker> queueRecentNodes = new Queue<AITracker>();
    private Queue<AITracker> queueRecentConnections = new Queue<AITracker>();

    //ItemData
    private MainInfoData currentInfoData = new MainInfoData();                                      //rolling current turn MainInfoData package
    private List<ItemData>[,] arrayOfItemDataByPriority = new List<ItemData>[(int)ItemTab.Count, 3];
    private List<ItemData> listOfDelayedItemData = new List<ItemData>();

    //Adjustments
    private List<ActionAdjustment> listOfActionAdjustments = new List<ActionAdjustment>();

    //Tracker data
    private List<HistoryRebelMove> listOfHistoryRebelMove = new List<HistoryRebelMove>();
    private List<HistoryNemesisMove> listOfHistoryNemesisMove = new List<HistoryNemesisMove>();
    private List<string> listOfHistoryAutoRun = new List<string>();


    //dictionaries
    private Dictionary<int, GameObject> dictOfNodeObjects = new Dictionary<int, GameObject>();      //Key -> nodeID, Value -> Node gameObject
    private Dictionary<int, Node> dictOfNodes = new Dictionary<int, Node>();                        //Key -> nodeID, Value -> Node
    private Dictionary<int, NodeD> dictOfNodeDUnweighted = new Dictionary<int, NodeD>();            //Key -> id, Value -> NodeD (Dijkstra API), Unweighted
    private Dictionary<int, NodeD> dictOfNodeDWeighted = new Dictionary<int, NodeD>();              //Key -> id, Value -> NodeD (Dijkstra API), Weighted
    private Dictionary<int, NodeArc> dictOfNodeArcs = new Dictionary<int, NodeArc>();               //Key -> nodeArcID, Value -> NodeArc
    private Dictionary<string, int> dictOfLookUpNodeArcs = new Dictionary<string, int>();           //Key -> nodeArc name, Value -> nodeArcID
    private Dictionary<int, PathData> dictOfDijkstraUnweighted = new Dictionary<int, PathData>();   //Key -> nodeID, Value -> PathData
    private Dictionary<int, PathData> dictOfDijkstraWeighted = new Dictionary<int, PathData>();     //Key -> nodeID, Value -> PathData
    private Dictionary<int, ActorArc> dictOfActorArcs = new Dictionary<int, ActorArc>();            //Key -> actorArcID, Value -> ActorArc
    private Dictionary<string, int> dictOfLookUpActorArcs = new Dictionary<string, int>();          //Key -> actorArc name, Value -> actorArcID
    private Dictionary<int, Actor> dictOfActors = new Dictionary<int, Actor>();                     //Key -> actorID, Value -> Actor
    private Dictionary<int, Trait> dictOfTraits = new Dictionary<int, Trait>();                     //Key -> traitID, Value -> Trait
    private Dictionary<int, TraitEffect> dictOfTraitEffects = new Dictionary<int, TraitEffect>();   //Key -> teffID, Value -> TraitEffect
    private Dictionary<string, int> dictOfLookUpTraitEffects = new Dictionary<string, int>();       //Key -> TraitEffect name, Value -> teffID
    private Dictionary<int, Action> dictOfActions = new Dictionary<int, Action>();                  //Key -> ActionID, Value -> Action
    private Dictionary<string, ManageAction> dictOfManageActions = new Dictionary<string, ManageAction>(); //Key -> ManageAction.name, Value -> ManageAction
    private Dictionary<string, int> dictOfLookUpActions = new Dictionary<string, int>();            //Key -> action name, Value -> actionID
    private Dictionary<int, Effect> dictOfEffects = new Dictionary<int, Effect>();                  //Key -> effectID, Value -> ActionEffect
    private Dictionary<int, Target> dictOfTargets = new Dictionary<int, Target>();                  //Key -> targetID, Value -> Target
    private Dictionary<int, TeamArc> dictOfTeamArcs = new Dictionary<int, TeamArc>();               //Key -> teamID, Value -> Team
    private Dictionary<string, int> dictOfLookUpTeamArcs = new Dictionary<string, int>();           //Key -> teamArc name, Value -> TeamArcID
    private Dictionary<int, Team> dictOfTeams = new Dictionary<int, Team>();                        //Key -> teamID, Value -> Team
    private Dictionary<int, Gear> dictOfGear = new Dictionary<int, Gear>();                         //Key -> gearID, Value -> Gear
    private Dictionary<int, Connection> dictOfConnections = new Dictionary<int, Connection>();      //Key -> connID, Value -> Connection
    private Dictionary<int, Message> dictOfArchiveMessages = new Dictionary<int, Message>();        //Key -> msgID, Value -> Message
    private Dictionary<int, Message> dictOfPendingMessages = new Dictionary<int, Message>();        //Key -> msgID, Value -> Message
    private Dictionary<int, Message> dictOfCurrentMessages = new Dictionary<int, Message>();        //Key -> msgID, Value -> Message
    private Dictionary<int, Message> dictOfAIMessages = new Dictionary<int, Message>();             //Key -> msgID, Value -> Message
    private Dictionary<int, EffectDataOngoing> dictOfOngoingID = new Dictionary<int, EffectDataOngoing>();  //Key -> ongoingID, Value -> Ongoing effect details
    private Dictionary<int, Faction> dictOfFactions = new Dictionary<int, Faction>();               //Key -> factionID, Value -> Faction
    private Dictionary<int, City> dictOfCities = new Dictionary<int, City>();                       //Key -> cityID, Value -> City
    private Dictionary<int, CityArc> dictOfCityArcs = new Dictionary<int, CityArc>();               //Key -> cityArcID, Value -> CityArc
    private Dictionary<int, Objective> dictOfObjectives = new Dictionary<int, Objective>();         //Key -> objectiveID, Value -> Objective
    private Dictionary<int, Organisation> dictOfOrganisations = new Dictionary<int, Organisation>();//Key -> orgID, Value -> Organisation
    private Dictionary<int, Mayor> dictOfMayors = new Dictionary<int, Mayor>();                     //Key -> mayorID, Value -> Mayor
    private Dictionary<int, DecisionAI> dictOfAIDecisions = new Dictionary<int, DecisionAI>();      //Key -> aiDecID, Value -> DecisionAI
    private Dictionary<string, int> dictOfLookUpAIDecisions = new Dictionary<string, int>();        //Key -> DecisionAI.name, Value -> DecisionAI.aiDecID
    private Dictionary<int, ActorConflict> dictOfActorConflicts = new Dictionary<int, ActorConflict>();   //Key -> actBreakID, Value -> ActorBreakdown
    private Dictionary<int, Secret> dictOfSecrets = new Dictionary<int, Secret>();                  //Key -> secretID, Value -> Secret
    private Dictionary<string, SecretType> dictOfSecretTypes = new Dictionary<string, SecretType>();      //Key -> SecretType.name, Value -> SecretType
    private Dictionary<string, SecretStatus> dictOfSecretStatus = new Dictionary<string, SecretStatus>(); //Key -> SecretStatus.name, Value -> SecretStatus
    private Dictionary<int, NodeCrisis> dictOfNodeCrisis = new Dictionary<int, NodeCrisis>();        //Key -> nodeCrisisID, Value -> NodeCrisis
    private Dictionary<int, MainInfoData> dictOfHistory = new Dictionary<int, MainInfoData>();       //Key -> turn, Value -> MainInfoData set for turn
    private Dictionary<int, Contact> dictOfContacts = new Dictionary<int, Contact>();                //Key -> contactID, Value -> Contact
    private Dictionary<int, List<int>> dictOfActorContacts = new Dictionary<int, List<int>>();       //Key -> ActorID, Value -> list of nodeID's where actor has contacts
    private Dictionary<int, List<int>> dictOfNodeContactsResistance = new Dictionary<int, List<int>>();   //Key -> NodeID, Value -> list of actorID's who have a contact at node
    private Dictionary<int, List<int>> dictOfNodeContactsAuthority = new Dictionary<int, List<int>>();    //Key -> NodeID, Value -> list of actorID's who have a contact at node
    private Dictionary<int, List<Contact>> dictOfContactsByNodeResistance = new Dictionary<int, List<Contact>>(); //Key -> NodeID, Value -> list of Contacts at the node (resistance only)
    private Dictionary<int, Mission> dictOfMissions = new Dictionary<int, Mission>();                //Key -> missionID, Value -> Mission
    private Dictionary<string, HelpData> dictOfHelpData = new Dictionary<string, HelpData>();        //Key -> tag, Value -> HelpData
    private Dictionary<StatType, int> dictOfStatistics = new Dictionary<StatType, int>();                      //Key -> (int)StatType, Value -> statistic

    //global SO's (enum equivalents)
    private Dictionary<string, Condition> dictOfConditions = new Dictionary<string, Condition>();           //Key -> Condition.name, Value -> Condition
    private Dictionary<string, Cure> dictOfCures = new Dictionary<string, Cure>();                          //Key -> Cure.name, Value -> Cure
    private Dictionary<string, TraitCategory> dictOfTraitCategories = new Dictionary<string, TraitCategory>();  //Key -> Category.name, Value -> TraitCategory

    //
    // - - - Initialisation - - -
    //

    /// <summary>
    /// Stuff that is done after level Manager.SetUp
    /// </summary>
    public void InitialiseLate()
    {
        Debug.Assert(listOfOneConnArcsDefault != null, "Invalid listOfOneConnArcsDefault (Null)");
        Debug.Assert(listOfTwoConnArcsDefault != null, "Invalid listOfTwoConnArcsDefault (Null)");
        Debug.Assert(listOfThreeConnArcsDefault != null, "Invalid listOfThreeConnArcsDefault (Null)");
        Debug.Assert(listOfFourConnArcsDefault != null, "Invalid listOfFourConnArcsDefault (Null)");
        Debug.Assert(listOfFiveConnArcsDefault != null, "Invalid listOfFiveConnArcsDefault (Null)");
        Debug.Assert(listOfOneConnArcsPreferred != null, "Invalid listOfOneConnArcsPreferred (Null)");
        Debug.Assert(listOfTwoConnArcsPreferred != null, "Invalid listOfTwoConnArcsPreferred (Null)");
        Debug.Assert(listOfThreeConnArcsPreferred != null, "Invalid listOfThreeConnArcsPreferred (Null)");
        Debug.Assert(listOfFourConnArcsPreferred != null, "Invalid listOfFourConnArcsPreferred (Null)");
        Debug.Assert(listOfFiveConnArcsPreferred != null, "Invalid listOfFiveConnArcsPreferred (Null)");
        //arrayOfNodes -> contains all relevant info on nodes by type
        int[] tempArray = GameManager.instance.levelScript.GetNodeTypeTotals();
        arrayOfNodes = new int[tempArray.Length, (int)NodeInfo.Count];
        for (int i = 0; i < tempArray.Length; i++)
        { arrayOfNodes[i, 0] = tempArray[i]; }
        //List of Nodes by Types -> each index has a list of all nodes of that NodeArc type
        int limit = CheckNumOfNodeArcs();
        for (int i = 0; i < limit; i++)
        {
            List<Node> tempList = new List<Node>();
            listOfNodesByType.Add(tempList);
        }
        //Populate List of lists -> place node in the correct list
        foreach (var node in dictOfNodes)
        { listOfNodesByType[node.Value.Arc.nodeArcID].Add(node.Value); }
        //Node Crisis placed into pick lists
        if (dictOfNodeCrisis != null)
        {
            foreach (var crisis in dictOfNodeCrisis)
            {
                if (crisis.Value != null)
                { AddNodeCrisisToList(crisis.Value); }
                else { Debug.LogWarningFormat("Invalid nodeCrisis \"{0}\" (Null)", crisis.Key); }
            }
            Debug.LogFormat("[Imp] DataManager.cs -> InitialiseLate: listOfCrisisStability has {0} records", listOfCrisisStability.Count);
            Debug.LogFormat("[Imp] DataManager.cs -> InitialiseLate: listOfCrisisSupport has {0} records", listOfCrisisSupport.Count);
            Debug.LogFormat("[Imp] DataManager.cs -> InitialiseLate: listOfCrisisSecurity has {0} records", listOfCrisisSecurity.Count);
        }
        else { Debug.LogWarning("Invalid dictOfNodeCrisis (Null)"); }
        //array Of ItemData
        for (int outer = 0; outer < (int)ItemTab.Count; outer++)
        {
            for (int inner = 0; inner < (int)ItemPriority.Count; inner++)
            { arrayOfItemDataByPriority[outer, inner] = new List<ItemData>(); }
        }
    }

    //
    // - - - Info Flow (Notifications)- - - 
    //

    /// <summary>
    /// Add itemData to arrayOfCurrentItemData according to tab and priorty
    /// </summary>
    /// <param name="data"></param>
    public void AddItemData(ItemData data)
    {
        if (data != null)
        {
            Debug.Assert(data.side != null, "Invalid ItemData side (Null)");
            GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
            //only take those from the same side or those aimed at both sides
            if (data.side.level == playerSide.level || data.side.level == GameManager.instance.globalScript.sideBoth.level)
            {
                if (data.delay == 0)
                { arrayOfItemDataByPriority[(int)data.tab, (int)data.priority].Add(data); }
                else { listOfDelayedItemData.Add(data); }
            }
            /*if (data.delay == 0) //used for testing authority itemData's
            { arrayOfItemDataByPriority[(int)data.tab, (int)data.priority].Add(data); }
            else { listOfDelayedItemData.Add(data); }*/
            /*else { Debug.LogFormat("[Tst] ItemData NOT retained -> {0}", data.itemText); }*/
        }
        else { Debug.LogWarning("Invalid ItemData (Null)"); }
    }

    /// <summary>
    /// NOTE: Use this ONLY to access already processed data, eg. you want to display the MainInfoApp during a turn (UpdateCurrentInfoData has already been run at turn start)
    /// </summary>
    /// <returns></returns>
    public MainInfoData GetCurrentInfoData()
    { return currentInfoData; }

    /// <summary>
    /// Master method to take all ItemData's for the turn, add them to the array of Lists by tab and priority, package them into a MainInfoData ready for MainInfoUI.cs
    /// </summary>
    /// <returns></returns>
    public MainInfoData UpdateCurrentItemData()
    {
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        List<ItemData> tempList = new List<ItemData>();
        //empty out data package prior to updating
        currentInfoData.Reset();
        //package up all three priorities for each tab into a single list and add to currentInfoData
        for (int outer = 0; outer < (int)ItemTab.Count; outer++)
        {
            //add in order of priority -> High (top) / Med / Low (bottom)
            tempList.AddRange(arrayOfItemDataByPriority[outer, (int)ItemPriority.High]);
            tempList.AddRange(arrayOfItemDataByPriority[outer, (int)ItemPriority.Medium]);
            tempList.AddRange(arrayOfItemDataByPriority[outer, (int)ItemPriority.Low]);
            //add to current info data
            currentInfoData.arrayOfItemData[outer].AddRange(tempList);
            //check list length -> need to know if too long so I can adjust to accommodate max. possible
            if (tempList.Count > 20)
            { Debug.LogWarningFormat("tempList has {0} records for tab {1}", tempList.Count, outer); }
            //empty out temp list ready for next tab data set
            tempList.Clear();
        }
        //empty out array lists ready for next turn
        for (int outer = 0; outer < (int)ItemTab.Count; outer++)
        {
            for (int inner = 0; inner < (int)ItemPriority.Count; inner++)
            { arrayOfItemDataByPriority[outer, inner].Clear(); }
        }
        // archive to History dict
        if (dictOfHistory != null)
        {
            //pass by value, not reference (otherwise duplicate data for each turn)
            MainInfoData historyData = new MainInfoData(currentInfoData);
            int turn = GameManager.instance.turnScript.Turn;
            try
            { dictOfHistory.Add(turn, historyData); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid currentInfoData (Null)"); }
            catch (ArgumentException)
            { Debug.LogErrorFormat("Duplicate record exists for MainInfoData turn {0}", turn); }
        }
        //Get news
        currentInfoData.tickerText = GameManager.instance.newsScript.GetNews();
        return currentInfoData;
    }

    /// <summary>
    /// returns a notification data package for a specific turn. Returns Null if not found and defaults to current turn if no parameter set
    /// </summary>
    /// <param name="turnNumber"></param>
    /// <returns></returns>
    public MainInfoData GetNotifications(int turnNumber = -1)
    {
        MainInfoData data = null;
        //if no turn number provided, or turn number 0, use current turn
        if (turnNumber < 1) { turnNumber = GameManager.instance.turnScript.Turn; }
        //get data set
        if (dictOfHistory.ContainsKey(turnNumber))
        { data = dictOfHistory[turnNumber]; }
        else { Debug.LogWarningFormat("Record not found in dictOfHistory for turn number {0}", turnNumber); }
        //return data set
        return data;
    }


    public List<ItemData> GetListOfDelayItemData()
    { return listOfDelayedItemData; }

    //
    // - - - NodeArcs - - -
    //


    /// <summary>
    /// returns a list of Default Node Arcs based on number of node connections (1 to 5). Null if a problem.
    /// </summary>
    /// <param name="numConnections"></param>
    /// <returns></returns>
    public List<NodeArc> GetDefaultNodeArcList(int numConnections)
    {
        Debug.Assert(numConnections > 0 && numConnections < 6, string.Format("Invalid numConnections \"{0}\"", numConnections));
        List<NodeArc> tempList = null;
        switch (numConnections)
        {
            case 1: tempList = listOfOneConnArcsDefault; break;
            case 2: tempList = listOfTwoConnArcsDefault; break;
            case 3: tempList = listOfThreeConnArcsDefault; break;
            case 4: tempList = listOfFourConnArcsDefault; break;
            case 5: tempList = listOfFiveConnArcsDefault; break;
            default:
                Debug.LogError("Invalid number of Connections " + numConnections);
                break;
        }
        return tempList;
    }

    /// <summary>
    /// returns a list of Preferred Node Arcs based on number of node connections (1 to 5). Null if a problem.
    /// </summary>
    /// <param name="numConnections"></param>
    /// <returns></returns>
    public List<NodeArc> GetPreferredNodeArcList(int numConnections)
    {
        Debug.Assert(numConnections > 0 && numConnections < 6, string.Format("Invalid numConnections \"{0}\"", numConnections));
        List<NodeArc> tempList = null;
        switch (numConnections)
        {
            case 1: tempList = listOfOneConnArcsPreferred; break;
            case 2: tempList = listOfTwoConnArcsPreferred; break;
            case 3: tempList = listOfThreeConnArcsPreferred; break;
            case 4: tempList = listOfFourConnArcsPreferred; break;
            case 5: tempList = listOfFiveConnArcsPreferred; break;
            default:
                Debug.LogError("Invalid number of Connections " + numConnections);
                break;
        }
        return tempList;
    }

    /// <summary>
    /// Get number of NodeArcs in dictionary
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfNodeArcs()
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
        else { Debug.LogWarning("Not found in Dict > nodeArcID " + nodeArcID); }
        return null;
    }

    public Dictionary<int, NodeArc> GetDictOfNodeArcs()
    { return dictOfNodeArcs; }

    public Dictionary<string, int> GetDictOfLookUpNodeArcs()
    { return dictOfLookUpNodeArcs; }


    /*public Dictionary<string, NodeDatapoint> GetDictOfNodeDatapoints()
    { return dictOfNodeDatapoints; }*/


    /// <summary>
    /// returns nodeArcID for specified nodeArc name, eg. "Corporate". Returns '-1' if not found in lookup dictionary. Must be in CAPS
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
    // - - - Action Related - - -
    //

    /// <summary>
    /// returns ActionID for a specified Action name, eg. "Any Team". Returns '-1' if not found in lookup dictionary
    /// </summary>
    /// <param name="actionName"></param>
    /// <returns></returns>
    public int GetActionID(string actionName)
    {
        if (dictOfLookUpActions.ContainsKey(actionName))
        { return dictOfLookUpActions[actionName]; }
        else { Debug.LogWarning(string.Format("Not found in Lookup Action dict \"{0}\"{1}", actionName, "\n")); }
        return -1;
    }

    /// <summary>
    /// returns
    /// </summary>
    /// <param name="actionID"></param>
    /// <returns></returns>
    public Action GetAction(int actionID)
    {
        if (dictOfActions.ContainsKey(actionID))
        { return dictOfActions[actionID]; }
        else { Debug.LogWarning("Not found in DictOfActions " + actionID); }
        return null;
    }

    public Dictionary<int, Action> GetDictOfActions()
    { return dictOfActions; }

    public Dictionary<string, int> GetDictOfLookUpActions()
    { return dictOfLookUpActions; }

    //
    // - - - Actor Arcs - - - 
    //

    /// <summary>
    /// returns a number of randomly selected ActorArcs. Returns null if a problem.
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public List<ActorArc> GetRandomActorArcs(int num, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        //filter for the required side
        List<ActorArc> tempMaster = new List<ActorArc>();
        if (side.level == GameManager.instance.globalScript.sideAuthority.level) { tempMaster.AddRange(authorityActorArcs); }
        else if (side.level == GameManager.instance.globalScript.sideResistance.level) { tempMaster.AddRange(resistanceActorArcs); }

        if (tempMaster.Count > 0)
        {
            //temp list for results
            List<ActorArc> tempList = new List<ActorArc>();
            //randomly select
            int index;
            int limit = Math.Min(num, tempMaster.Count);
            for (int i = 0; i < limit; i++)
            {
                index = Random.Range(0, tempMaster.Count);
                tempList.Add(tempMaster[index]);
                //remove from list to prevent being selected again
                tempMaster.RemoveAt(index);
            }
            return tempList;
        }
        else
        { Debug.LogWarning(string.Format("Invalid side \"{0}\"", side)); }
        return null;
    }

    /// <summary>
    /// returns list of ActorArcs by side, null if a problem
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public List<ActorArc> GetActorArcs(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        if (side.level == GameManager.instance.globalScript.sideAuthority.level) { return authorityActorArcs; }
        else if (side.level == GameManager.instance.globalScript.sideResistance.level) { return resistanceActorArcs; }
        else { Debug.LogWarning(string.Format("Invalid side \"{0}\"", side.name)); }
        return null;
    }

    /// <summary>
    /// Gets specified actor Arc, returns null if not found
    /// </summary>
    /// <param name="actorArcID"></param>
    /// <returns></returns>
    public ActorArc GetActorArc(int actorArcID)
    {
        ActorArc arc = null;
        if (dictOfActorArcs.TryGetValue(actorArcID, out arc))
        { return arc; }
        return null;
    }

    /// <summary>
    /// Gets actor arc based on actor arc name (upper case), eg. "FIXER", returns null if not found
    /// </summary>
    /// <param name="actorArcName"></param>
    /// <returns></returns>
    public ActorArc GetActorArc(string arcName)
    {
        ActorArc arc = null;
        if (string.IsNullOrEmpty(arcName) == false)
        {
            //get ID
            if (dictOfLookUpActorArcs.ContainsKey(arcName) == true)
            {
                int actorArcID = dictOfLookUpActorArcs[arcName];
                //get ActorArc
                if (dictOfActorArcs.TryGetValue(actorArcID, out arc))
                { return arc; }
            }
        }
        else { Debug.LogWarning("Invalid arcName parameter (Null)"); }
        return arc;
    }

    public Dictionary<int, ActorArc> GetDictOfActorArcs()
    { return dictOfActorArcs; }

    public Dictionary<string, int> GetDictOfLookUpActorArcs()
    { return dictOfLookUpActorArcs; }

    public List<ActorArc> GetListOfAuthorityActorArcs()
    { return authorityActorArcs; }

    public List<ActorArc> GetListOfResistanceActorArcs()
    { return resistanceActorArcs; }

    //
    // - - -  Actor Breakdowns
    //

    public Dictionary<int, ActorConflict> GetDictOfActorConflicts()
    { return dictOfActorConflicts; }

    /// <summary>
    /// Gets specified actor Conflict, returns null if not found
    /// </summary>
    /// <param name="actorArcID"></param>
    /// <returns></returns>
    public ActorConflict GetActorConflict(int conflictID)
    {
        ActorConflict conflict = null;
        if (dictOfActorConflicts.TryGetValue(conflictID, out conflict))
        { return conflict; }
        return null;
    }

    //
    // - - - Traits - - -
    //

    /// <summary>
    /// return a random trait (could be good, bad or neutral) specific to a category of traits. Null if a problem
    /// include a GlobalSide if required for 'Actor' category (eg. Authority traits only -> this would also include traits with side 'None')
    /// </summary>
    /// <returns></returns>
    public Trait GetRandomTrait(TraitCategory category, GlobalSide side = null)
    {
        Trait trait = null;
        List<Trait> tempList = new List<Trait>();
        //filter list by category
        if (side == null)
        { tempList = listOfAllTraits.FindAll(x => x.category.name == category.name); }
        //filter list by category and side (includes traits with no side specified which are assummed to apply to all
        else
        {
            IEnumerable<Trait> traitList =
                from traitTemp in listOfAllTraits
                where traitTemp.category.name == category.name
                where traitTemp.side == null || traitTemp.side.level == side.level
                select traitTemp;
            tempList = traitList.ToList();
        }
        //get trait
        if (tempList.Count > 0)
        { trait = tempList[Random.Range(0, tempList.Count)]; }
        return trait;
    }

    /// <summary>
    /// returns the matching trait to the input string. Null if not found.
    /// </summary>
    /// <param name="traitText"></param>
    /// <returns></returns>
    public Trait GetTrait(string traitText)
    {
        Trait trait = null;
        foreach (var record in dictOfTraits)
        {
            if (record.Value.tag.Equals(traitText) == true)
            {
                trait = record.Value;
                break;
            }
        }
        return trait;
    }

    /*/// <summary>
    /// Gets specified Trait by trait ID, returns null if not found
    /// </summary>
    /// <param name="traitID"></param>
    /// <returns></returns>
    public Trait GetTrait(int traitID)
    {
        Trait trait = null;
        if (dictOfTraits.TryGetValue(traitID, out trait))
        { return trait; }
        return null;
    }*/

    public Dictionary<int, Trait> GetDictOfTraits()
    { return dictOfTraits; }

    public List<Trait> GetListOfAllTraits()
    { return listOfAllTraits; }

    public Dictionary<string, TraitCategory> GetDictOfTraitCategories()
    { return dictOfTraitCategories; }

    /// <summary>
    /// Returns TraitCategory SO, Null if not found in dictionary
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public TraitCategory GetTraitCategory(string category)
    {
        TraitCategory traitCategory = null;
        if (string.IsNullOrEmpty(category) == false)
        {
            if (dictOfTraitCategories.ContainsKey(category))
            { return dictOfTraitCategories[category]; }
            else { Debug.LogWarning(string.Format("TraitCategory \"{0}\" not found in dictOfTraitCategories{1}", category, "\n")); }
        }
        else { Debug.LogError("Invalid category (Null or Empty)"); }
        return traitCategory;
    }

    public Dictionary<int, TraitEffect> GetDictOfTraitEffects()
    { return dictOfTraitEffects; }

    public Dictionary<string, int> GetDictOfLookUpTraitEffects()
    { return dictOfLookUpTraitEffects; }

    /// <summary>
    /// Gets specified TraitEffect, returns null if not found
    /// </summary>
    /// <param name="actorArcID"></param>
    /// <returns></returns>
    public TraitEffect GetTraitEffect(int traitEffectID)
    {
        TraitEffect traitEffect = null;
        if (dictOfTraitEffects.TryGetValue(traitEffectID, out traitEffect))
        {
            return traitEffect;
        }
        return null;
    }

    /// <summary>
    /// Returns TraitEffect teffID, -1 if not found in dictionary
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public int GetTraitEffectID(string traitEffect)
    {
        int teffID = -1;
        if (string.IsNullOrEmpty(traitEffect) == false)
        {
            if (dictOfLookUpTraitEffects.ContainsKey(traitEffect))
            { return dictOfLookUpTraitEffects[traitEffect]; }
            else { Debug.LogWarning(string.Format("TraitEffect \"{0}\" not found in dictOfLookUpTraitEffects{1}", traitEffect, "\n")); }
        }
        else { Debug.LogError("Invalid traitEffect (Null or Empty)"); }
        return teffID;
    }

    //
    // - - - Contacts - - - 
    //

    /// <summary>
    /// list of unassigned, pre-initialised, resistance contactID's
    /// </summary>
    /// <returns></returns>
    public List<int> GetContactPool()
    { return contactPool; }

    public Dictionary<int, Contact> GetDictOfContacts()
    { return dictOfContacts; }

    /// <summary>
    /// Returns a contact, Null if not found
    /// </summary>
    /// <param name="contactID"></param>
    /// <returns></returns>
    public Contact GetContact(int contactID)
    {
        Debug.Assert(contactID > -1, "Invalid contactID (less than Zero)");
        if (dictOfContacts.ContainsKey(contactID) == true)
        { return dictOfContacts[contactID]; }
        return null;
    }

    /// <summary>
    /// Add contact to dictionary (Resistance side only)
    /// </summary>
    /// <param name="contact"></param>
    public void AddResistanceContact(Contact contact)
    {
        if (contact != null)
        {
            try
            { dictOfContacts.Add(contact.contactID, contact); }
            catch (ArgumentException)
            { Debug.LogErrorFormat("Invalid entry in dictOfContacts for contact {0}, ID {1}", contact.nameFirst, contact.contactID); }
        }
        else { Debug.LogError("Invalid contact (Null)"); }
    }

    /// <summary>
    /// returns the appropriate dict based on, default, current side (unless 'false')
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, List<int>> GetDictOfNodeContacts(bool isCurrentSide = true)
    {
        if (isCurrentSide == true)
        {
            //Current side
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
            { return dictOfNodeContactsAuthority; }
            else { return dictOfNodeContactsResistance; }
        }
        else
        {
            //Non current side
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
            { return dictOfNodeContactsResistance; }
            else { return dictOfNodeContactsAuthority; }
        }
    }

    /// <summary>
    /// Overloaded. Based on side. Returns null if not either Authority or Resistance
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public Dictionary<int, List<int>> GetDictOfNodeContacts(GlobalSide side)
    {
        switch (side.level)
        {
            case 1:
                //Authority
                return dictOfNodeContactsAuthority;
            case 2:
                //Resistance
                return dictOfNodeContactsResistance;
            default:
                Debug.LogWarningFormat("Invalid side.level {0}", side.level);
                return null;
        }
    }



    /// <summary>
    /// return a list of all Nodes where an actor has contacts, returns empty list if none
    /// </summary>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public List<Node> GetListOfActorContactNodes(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid slotID");
        List<Node> listOfNodes = new List<Node>();
        //get actor
        Actor actor = GetCurrentActor(slotID, GameManager.instance.sideScript.PlayerSide);
        if (actor != null)
        {
            if (dictOfActorContacts.ContainsKey(actor.actorID) == true)
            {
                List<int> listOfNodeID = new List<int>(dictOfActorContacts[actor.actorID]);
                if (listOfNodeID != null)
                {
                    //loop through list and convert nodeID's to Nodes
                    for (int i = 0; i < listOfNodeID.Count; i++)
                    {
                        Node node = GetNode(listOfNodeID[i]);
                        if (node != null)
                        {
                            //add node to return list
                            listOfNodes.Add(node);
                        }
                        else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", listOfNodeID[i]); }
                    }
                }
                else { Debug.LogErrorFormat("Invalid listOfNodeID (null) for actor {0}, {1}, actorID {2}", actor.actorName, actor.arc.name, actor.actorID); }
            }
        }
        else { Debug.LogWarningFormat("Invalid actor (Null) for slotID {0}", slotID); }
        return listOfNodes;
    }

    /// <summary>
    /// return a list of all Nodes where actor has Active contacts, returns empty list if none
    /// </summary>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public List<Node> GetListOfActorContacts(int actorID)
    {
        List<Node> listOfNodes = new List<Node>();
        bool isActiveContact;
        if (dictOfActorContacts.ContainsKey(actorID) == true)
        {
            List<int> listOfNodeID = new List<int>(dictOfActorContacts[actorID]);
            if (listOfNodeID != null)
            {
                //loop through list and convert nodeID's to Nodes
                for (int i = 0; i < listOfNodeID.Count; i++)
                {
                    //check there is an active contact for this actor at the node
                    isActiveContact = false;
                    List<Contact> listOfContacts = GetListOfNodeContacts(listOfNodeID[i]);
                    if (listOfContacts != null)
                    {
                        //loop contacts present at node
                        for (int j = 0; j < listOfContacts.Count; j++)
                        {
                            Contact contact = listOfContacts[j];
                            if (contact != null)
                            {
                                if (contact.status == ContactStatus.Active)
                                {
                                    //correct actor
                                    if (contact.actorID == actorID)
                                    {
                                        isActiveContact = true;
                                        break;
                                    }
                                }
                            }
                            else { Debug.LogErrorFormat("Invalid contact (Null) for listOfContacts[{0}]", i); }
                        }
                    }
                    //active contact for actor, add node to list
                    if (isActiveContact == true)
                    {
                        Node node = GetNode(listOfNodeID[i]);
                        if (node != null)
                        {
                            //add node to return list
                            listOfNodes.Add(node);
                        }
                        else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", listOfNodeID[i]); }
                    }
                }
            }
            else { Debug.LogErrorFormat("Invalid listOfNodeID (null) for actorID {0}", actorID); }
        }
        return listOfNodes;
    }

    /// <summary>
    /// Get a list of all contacts at a particular node, null if none
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public List<Contact> GetListOfNodeContacts(int nodeID)
    {
        if (dictOfContactsByNodeResistance.ContainsKey(nodeID) == true)
        { return dictOfContactsByNodeResistance[nodeID]; }
        return null;
    }

    /// <summary>
    /// returns a list of actors who have a contact at node, Null if none. Resistance Only
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public List<int> CheckContactResistanceAtNode(int nodeID)
    {
        List<int> tempList = null;
        if (dictOfNodeContactsResistance != null)
        {
            if (dictOfNodeContactsResistance.ContainsKey(nodeID) == true)
            { tempList = dictOfNodeContactsResistance[nodeID]; }
        }
        else { Debug.LogWarning("Invalid tempDict (Resistance or Authority) (Null)"); }
        return tempList;
    }

    /// <summary>
    /// returns true if there is at least one active contact (with an active actor) at specified node, false otherwise
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public bool CheckForActiveContact(Node node)
    {
        bool isPresent = false;
        if (node != null)
        {
            if (node.isContactResistance == true)
            {
                List<int> listOfActors = CheckContactResistanceAtNode(node.nodeID);
                if (listOfActors != null)
                {
                    //loop actors and check all relevant contacts
                    int count = listOfActors.Count;
                    if (count > 0)
                    {
                        for (int index = 0; index < count; index++)
                        {
                            Actor actor = GetActor(listOfActors[index]);
                            if (actor != null)
                            {
                                //actor must active
                                if (actor.Status == ActorStatus.Active)
                                {
                                    Contact contact = actor.GetContact(node.nodeID);
                                    if (contact != null)
                                    {
                                        if (contact.status == ContactStatus.Active)
                                        {
                                            //at least one active contact present at node
                                            isPresent = true;
                                            break;
                                        }
                                    }
                                    else { Debug.LogErrorFormat("Invalid contact (Null) for actor {0}, {1}, id {2}", actor.actorName, actor.arc.name, actor.actorID); }
                                }
                            }
                            else { Debug.LogErrorFormat("Invalid actor (Null) for listOfActors.actorID {0}", listOfActors[index]); }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid listOfActors (Empty) for nodeID {0}", node.nodeID); }
                }
                else { Debug.LogErrorFormat("Invalid listOfActors (Null) for nodeID {0}", node.nodeID); }
            }
        }
        else { Debug.LogError("Invalid node (Null)"); }
        return isPresent;
    }



    public Dictionary<int, List<int>> GetDictOfActorContacts()
    { return dictOfActorContacts; }

    /// <summary>
    /// returns true if actor has a contact at the node, false otherwise. For current side.
    /// </summary>
    /// <param name="actorID"></param>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public bool CheckActorContactPresent(int actorID, int nodeID)
    {
        if (dictOfActorContacts.ContainsKey(actorID) == true)
        {
            List<int> listOfNodes = dictOfActorContacts[actorID];
            if (listOfNodes != null)
            {
                if (listOfNodes.Exists(x => x == nodeID) == true)
                { return true; }
                else { return false; }
            }
            else
            {
                Debug.LogWarningFormat("Invalid listOfNodes (Null) for actorID {0}", actorID);
                return false;
            }
        }
        else { return false; }
    }

    /// <summary>
    /// adds mew set of Actor contacts to dictionaries. ListOfContactNodes holds nodeID's where actor has a contact. Updates node flags
    /// 'isActorsAndNodes' updates dictOfNodeContacts (Resistance/Authority) and  dictOfActorContacts. If false then only dictOfNodeContacts (eg. recalling an actor with previous contacts)
    /// </summary>
    /// <param name="actorID"></param>
    /// <param name="listOfContactNodes"></param>
    public bool AddContacts(int actorID, List<int> listOfContactNodes, bool isActorsAndNodes = true)
    {
        Debug.Assert(actorID > -1, "Invalid actorID (-1)");
        bool successFlag = true;
        int numOfContacts, nodeID;
        List<int> listOfActorID = new List<int>();
        if (listOfContactNodes != null)
        {
            numOfContacts = listOfContactNodes.Count;
            if (numOfContacts > 0)
            {
                //add to dictOfActorContacts
                if (isActorsAndNodes == true)
                {
                    try
                    { dictOfActorContacts.Add(actorID, listOfContactNodes); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Invalid entry in dictOfActorContacts for actorID {0}", actorID); successFlag = false; }
                }
                //add to dictOfNodeContacts
                Actor actor = GetActor(actorID);
                if (actor != null)
                {
                    //Add node contacts to correct dict depending on current / non-current actor side
                    Dictionary<int, List<int>> dictOfNodeContacts = new Dictionary<int, List<int>>();
                    if (actor.side.level == GameManager.instance.sideScript.PlayerSide.level)
                    { dictOfNodeContacts = GetDictOfNodeContacts(); }
                    else { dictOfNodeContacts = GetDictOfNodeContacts(false); }
                    for (int i = 0; i < numOfContacts; i++)
                    {
                        nodeID = listOfContactNodes[i];
                        if (dictOfNodeContacts.ContainsKey(nodeID) == true)
                        {
                            //existing entry, check actorID not already present
                            listOfActorID = dictOfNodeContacts[nodeID];
                            if (listOfActorID.Exists(id => id == actorID) == true)
                            {
                                //already present, warning message
                                Debug.LogWarningFormat("Duplicate actorID {0} found in dictOfContacts for nodeID {1}", actorID, nodeID);
                            }
                            else
                            {
                                //not present, add to list
                                dictOfNodeContacts[nodeID].Add(actorID);
                            }
                        }
                        else
                        {
                            //create a new entry
                            List<int> newList = new List<int>();
                            newList.Add(actorID);
                            try
                            { dictOfNodeContacts.Add(nodeID, newList); }
                            catch (ArgumentException)
                            { Debug.LogErrorFormat("Invalid entry in dictOfNodeContacts for nodeID {0}", nodeID); successFlag = false; }
                        }
                    }
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            else { Debug.LogError("No contacts in listOfContactNodes"); successFlag = false; }
        }
        else { Debug.LogError("Invalid listOfContactNodes (Null)"); successFlag = false; }
        //update node contacts -> NOTE: CreateNodeContacts isn't added here but in the calling method, ContactManager.cs -> SetActorContacts
        if (successFlag == true)
        { GameManager.instance.contactScript.UpdateNodeContacts(); }
        return successFlag;
    }

    /// <summary>
    /// Removes all contacts from a specific actor from dictOfNodeContacts (record remains in dictOfActorContacts in case the actor returns). Updates node flags
    /// </summary>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public bool RemoveContactsActor(int actorID)
    {
        Debug.Assert(actorID > -1, "Invalid actorID (-1)");
        bool successFlag = true;
        //find record in dictOfActorContacts and get listOfNodeID's
        if (dictOfActorContacts.ContainsKey(actorID) == true)
        {
            //delete actorID from all relevant entries in dictOfNodeContacts
            List<int> listOfNodes = new List<int>(dictOfActorContacts[actorID]);
            int numOfNodes = listOfNodes.Count;
            int nodeID;
            Actor actor = GetActor(actorID);
            if (actor != null)
            {
                Dictionary<int, List<int>> dictOfNodeContacts = GetDictOfNodeContacts(actor.side);
                if (numOfNodes > 0)
                {
                    //loop nodes and remove actorID from each node contact list
                    for (int i = 0; i < numOfNodes; i++)
                    {
                        nodeID = listOfNodes[i];
                        //find node entry in dictOfNodeContacts
                        if (dictOfNodeContacts.ContainsKey(nodeID) == true)
                        {
                            List<int> listOfActors = dictOfNodeContacts[nodeID];
                            if (listOfActors != null)
                            {
                                //remove actor from list
                                if (listOfActors.Remove(actorID) == false)
                                { Debug.LogWarningFormat("ActorID failed to be removed from list of NodeID {0}", nodeID); successFlag = false; }
                                else
                                {
                                    /*Debug.LogFormat("[Tst] NodeManager.cs -> RemoveContacts: actorID {0} removed from nodeID {1} list{2}", actorID, nodeID, "\n");*/
                                    if (listOfActors.Count == 0)
                                    {
                                        //if no more actors with contacts at node, delete dictionary record
                                        dictOfNodeContacts.Remove(nodeID);
                                    }
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid list<actorID> (Null) for nodeID {0}", nodeID); successFlag = false; }
                        }
                        else { Debug.LogWarningFormat("Node not found in dictOfContacts, nodeID {0}", nodeID); successFlag = false; }
                    }
                }
                else { Debug.LogWarningFormat("invalid listOfNodes (Empty) for actorID {0}", actorID); successFlag = false; }
            }
            else { Debug.LogErrorFormat("ActorID {0} not found in dictOfActorContacts", actorID); successFlag = false; }
            //update node contacts
            GameManager.instance.contactScript.UpdateNodeContacts();
            //set all actor.cs contacts to status 'inactive'
            if (successFlag == true)
            {
                //resistance only
                if (actor.side.level == GameManager.instance.globalScript.sideResistance.level)
                {
                    Dictionary<int, Contact> dictOfContacts = actor.GetDictOfContacts();
                    if (dictOfContacts != null)
                    {
                        foreach (var contact in dictOfContacts)
                        { contact.Value.status = ContactStatus.Inactive; }
                    }
                    else { Debug.LogErrorFormat("Invalid dictOfContacs for actorID {0}", actorID); }
                }
            }
        }
        else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", actorID); }
        //update node contacts
        if (successFlag == true)
        {
            GameManager.instance.contactScript.UpdateNodeContacts();
            CreateNodeContacts();
        }
        return successFlag;
    }

    /// <summary>
    /// Removes all contacts at a specific node (eg. erasure team removing Known contacts). Updates collections and node flags. Returns # of contacts removed, 0 if none
    /// To choose a specific side, put in a value for isCurrentSide (which is ignored) and specify a GlobalSide
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public int RemoveContactsNode(int nodeID, string reason, bool isCurrentSide = true, GlobalSide side = null)
    {
        Debug.Assert(nodeID > -1, "Invalid nodeID (-1)");
        int numContacts = 0;
        List<int> listOfActors;
        //get 
        Dictionary<int, List<int>> dictOfNodeContacts = null;
        if (side != null)
        { dictOfNodeContacts = GetDictOfNodeContacts(side); }
        else { dictOfNodeContacts = GetDictOfNodeContacts(isCurrentSide); }
        if (dictOfNodeContacts != null)
        {
            //find entry
            if (dictOfNodeContacts.ContainsKey(nodeID) == true)
            {
                listOfActors = new List<int>(dictOfNodeContacts[nodeID]);
                if (listOfActors != null)
                {
                    //having got the list, delete the record from dictOfNodes
                    if (dictOfNodeContacts.Remove(nodeID) == true)
                    { Debug.LogFormat("DataManager.cs -> RemoveContactsNode: dictOfNodeContacts[{0}] all contacts removed{1}", nodeID, "\n"); }
                    //loop through list and delete all references to the specific node for the actors
                    numContacts = listOfActors.Count();
                    if (numContacts > 0)
                    {
                        int actorID;
                        for (int i = 0; i < numContacts; i++)
                        {
                            actorID = listOfActors[i];
                            Actor actor = GetActor(actorID);
                            if (actor != null)
                            {
                                //find record in dictOfActors
                                if (dictOfActorContacts.ContainsKey(actorID) == true)
                                {
                                    //remove nodeID from list
                                    if (dictOfActorContacts[actorID].Remove(nodeID) == true)
                                    {
                                        Debug.LogFormat("DataManager.cs -> RemoveContactsNode: nodeID {0} removed from dictOfActorContacts[{1}]{2}", nodeID, actorID, "\n");
                                        //resistance actor, remove contact from dict
                                        if (actor.side.level == GameManager.instance.globalScript.sideResistance.level)
                                        {
                                            Contact contact = actor.GetContact(nodeID);
                                            if (contact != null)
                                            {
                                                //remove contact record from actor
                                                if (actor.RemoveContact(nodeID) == true)
                                                {
                                                    Debug.LogFormat("[Cont] DataManager.cs -> RemoveContacts: REMOVED {0}, ID {1} from {2}, {3},  nodeID {4}", contact.nameFirst, contact.contactID,
                                                    actor.actorName, actor.arc.name, nodeID);
                                                    //message
                                                    Node node = GetNode(nodeID);
                                                    if (node != null)
                                                    {
                                                        string text = string.Format("{0} Loses {1} Contact at {2}, {3}", actor.arc.name, contact.job, node.nodeName, node.Arc.name);
                                                        GameManager.instance.messageScript.ContactChange(text, actor, node, contact, false, reason);
                                                    }
                                                    else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", nodeID); }
                                                }
                                                else
                                                {
                                                    Debug.LogFormat("DataManager.cs -> Contact {0}, ID {1}, NOT Removed (FAIL), {2}, {3}, actorID {4} contact at nodeID {5}{6}", contact.nameFirst,
                                                        contact.contactID, actor.actorName, actor.arc.name, actor.actorID, nodeID, "\n");
                                                }
                                            }
                                            else { Debug.LogErrorFormat("Invalid Contact (Null) for actor {0}, {1}, ID {2} at nodeID {3}", actor.actorName, actor.arc.name, actor.actorID, nodeID); }
                                        }
                                    }
                                    else
                                    { Debug.LogWarningFormat("nodeID {0} not found in dictOfActorContacts.ListOfNodes for actorID {1}", nodeID, actorID); }

                                }
                                else { Debug.LogWarningFormat("Record not found in dictOfActorContacts for actorID {0}, nodeID {1}", actorID, nodeID); }
                            }
                            else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", actorID); }
                        }
                    }
                    else { Debug.LogWarningFormat("There are NO contacts present for nodeID {0}", nodeID); }
                }
                else { Debug.LogWarningFormat("Invalid listOfActors (Null) for nodeID {0}", nodeID); }
            }
            else { Debug.LogWarningFormat("Record not found in dictOfNodeContacts for nodeID {0}", nodeID); }
        }
        else { Debug.LogError("Invalid dictOfNodeContacts (Null)"); }
        //update node contacts
        if (numContacts > 0)
        {
            GameManager.instance.contactScript.UpdateNodeContacts();
            CreateNodeContacts();
        }
        return numContacts;
    }

    /// <summary>
    /// add a single new node contact to an actor. Update dictionaries and node flags. Current or other side, automatically detected. Updates node flags as well.
    /// </summary>
    public bool AddContactSingle(int actorID, int nodeID)
    {
        Debug.Assert(actorID > -1, "Invalid actorID (-1)");
        Debug.Assert(nodeID > -1, "Invalid nodeID (-1)");
        bool isSuccess = true;
        bool isCurrentSide = true;
        if (GameManager.instance.sideScript.PlayerSide.level != GameManager.instance.turnScript.currentSide.level)
        { isCurrentSide = false; }
        Actor actor = GetActor(actorID);
        if (actor != null)
        {
            //check actor has no existing contact at node
            if (dictOfActorContacts.ContainsKey(actorID) == true)
            {
                List<int> listOfNodes = dictOfActorContacts[actorID];
                if (listOfNodes != null)
                {
                    if (listOfNodes.Exists(id => id == nodeID) == false)
                    {
                        //all O.K, add contact to dictionaries
                        listOfNodes.Add(nodeID);
                        Dictionary<int, List<int>> dictOfNodeContacts = GetDictOfNodeContacts(isCurrentSide);
                        if (dictOfNodeContacts != null)
                        {
                            //get specific node
                            if (dictOfNodeContacts.ContainsKey(nodeID) == true)
                            {
                                //existing contacts at node, check not a duplicate
                                List<int> listOfActors = dictOfNodeContacts[nodeID];
                                if (listOfActors != null)
                                {
                                    //check actor doesn't already have a contact at node
                                    if (listOfActors.Exists(id => id == actorID) == false)
                                    {
                                        //add actor to node list
                                        listOfActors.Add(actorID);
                                    }
                                    else { Debug.LogWarningFormat("actorID {0} already present at nodeID {1}", actorID, nodeID); isSuccess = false; }
                                }
                                else { Debug.LogWarningFormat("Invalid listOfActors (Null) for nodeID {0}", nodeID); isSuccess = false; }
                            }
                            else
                            {
                                //no existing contacts at node, create new entry
                                List<int> newList = new List<int>();
                                newList.Add(actorID);
                                try
                                { dictOfNodeContacts.Add(nodeID, newList); }
                                catch (ArgumentException)
                                { Debug.LogErrorFormat("Invalid entry in dictOfNodeContacts for nodeID {0}", nodeID); }
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid dictOfNodeContacts (Null)"); isSuccess = false; }
                    }
                    else { Debug.LogWarningFormat("ActorID {0} has existing contact at nodeID {1}", actorID, nodeID); isSuccess = false; }
                }
                else { Debug.LogWarningFormat("Invalid listOfNodes (Null) for actorID {0}", actorID); isSuccess = false; }
            }
            else { Debug.LogWarningFormat("Record not found in dictOfActorContacts for actorID {0}", actorID); isSuccess = false; }
        }
        else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", actorID); isSuccess = false; }
        if (isSuccess == true)
        {
            //assign a new contact if a Resistance actor
            if (actor.side.level == GameManager.instance.globalScript.sideResistance.level)
            {
                Contact contact = GameManager.instance.contactScript.AssignContact(actor.actorID, nodeID);
                if (contact != null)
                {
                    actor.AddContact(contact);
                    Debug.LogFormat("[Cont] DatabaseManager.cs -> AddContactSingle: ADDED {0}, contactID {1} to {2}, {3}, actorID {4}, nodeID {5}, {6}", contact.nameFirst, contact.contactID,
                        actor.actorName, actor.arc.name, actor.actorID, nodeID, "\n");
                    //message
                    Node node = GetNode(nodeID);
                    if (node != null)
                    {
                        string text = string.Format("{0} Aquires {1} Contact at {2}, {3}", actor.arc.name, contact.job, node.nodeName, node.Arc.name);
                        GameManager.instance.messageScript.ContactChange(text, actor, node, contact);
                    }
                    else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", nodeID); }
                }
                else { Debug.LogError("Invalid contact (Null)"); }
            }
            else
            { Debug.LogFormat("[Cont] DataManager.cs -> AddContactSingle: ADDED {0}, {1}, actorID {2} contact at nodeID {3}{4}", actor.actorName, actor.arc.name, actor.actorID, nodeID, "\n"); }
        }
        else { Debug.LogFormat("DataManager.cs -> Contact NOT Added (FAIL): {0}, {1}, actorID {2} contact at nodeID {3}{4}", actor.actorName, actor.arc.name, actor.actorID, nodeID, "\n"); }
        //update node contact flags
        if (isSuccess == true)
        {
            GameManager.instance.contactScript.UpdateNodeContacts(isCurrentSide);
            CreateNodeContacts();
        }
        return isSuccess;
    }

    /// <summary>
    /// Remove a single contact from an actor. Update dictionaries and node flags. Current or other side, automatically detected.
    /// </summary>
    /// <param name="actorID"></param>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public bool RemoveContactSingle(int actorID, int nodeID)
    {
        Debug.Assert(actorID > -1, "Invalid actorID (-1)");
        Debug.Assert(nodeID > -1, "Invalid nodeID (-1)");
        bool isSuccess = true;
        bool isCurrentSide = true;
        if (GameManager.instance.sideScript.PlayerSide.level != GameManager.instance.turnScript.currentSide.level)
        { isCurrentSide = false; }
        Actor actor = GetActor(actorID);
        if (actor != null)
        {
            //check actor has no existing contact at node
            if (dictOfActorContacts.ContainsKey(actorID) == true)
            {
                List<int> listOfNodes = dictOfActorContacts[actorID];
                if (listOfNodes != null)
                {
                    if (listOfNodes.Exists(id => id == nodeID) == true)
                    {
                        //remove nodeID from node list
                        if (listOfNodes.Remove(nodeID) == true)
                        {
                            //get appropriate node dictionary
                            Dictionary<int, List<int>> dictOfNodeContacts = GetDictOfNodeContacts(isCurrentSide);
                            if (dictOfNodeContacts != null)
                            {
                                //get specific node
                                if (dictOfNodeContacts.ContainsKey(nodeID) == true)
                                {
                                    List<int> listOfActors = dictOfNodeContacts[nodeID];
                                    if (listOfActors != null)
                                    {
                                        //remove actor from Node list of actors
                                        if (listOfActors.Remove(actorID) == false)
                                        { Debug.LogWarningFormat("Failed to remove actorID {0} from nodeID {1} listOfActors", actorID, nodeID); isSuccess = false; }
                                        else
                                        {
                                            //if there are no other contacts, delete dictionarys record
                                            if (listOfActors.Count == 0)
                                            { dictOfNodeContacts.Remove(nodeID); }
                                        }
                                    }
                                    else { Debug.LogWarningFormat("Invalid listOfActors for nodeID {0}", nodeID); isSuccess = false; }
                                }
                                else { Debug.LogWarningFormat("nodeID {0} for actorID {1} NOT found in dictOfNodeContacts", nodeID, actorID); isSuccess = false; }
                            }
                            else { Debug.LogError("Invalid dictOfNodeContacts (Null)"); isSuccess = false; }
                        }
                        else { Debug.LogWarningFormat("Unable to remove nodeID {0}", nodeID); isSuccess = false; }
                    }
                    else { Debug.LogWarningFormat("ActorID {0} has NO Contact at nodeID {1}", actorID, nodeID); isSuccess = false; }
                }
                else { Debug.LogWarningFormat("Invalid listOfNodes (Null) for actorID {0}", actorID); isSuccess = false; }
            }
            else { Debug.LogWarningFormat("Record not found in dictOfActorContacts for actorID {0}", actorID); isSuccess = false; }
        }
        else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", actorID); isSuccess = false; }

        if (isSuccess == true)
        {
            //remove contact record from actor
            if (actor.side.level == GameManager.instance.globalScript.sideResistance.level)
            {
                Contact contact = actor.GetContact(nodeID);
                if (contact != null)
                {
                    if (actor.RemoveContact(nodeID) == true)
                    {
                        Debug.LogFormat("[Cont] DataManager.cs -> RemoveContactsSingle: REMOVED {0}, ID {1}, from {2}, {3}, actorID {4}, at nodeID {5}", contact.nameFirst, contact.contactID,
                          actor.actorName, actor.arc.name, actor.actorID, nodeID);
                        //message
                        Node node = GetNode(nodeID);
                        if (node != null)
                        {
                            string text = string.Format("{0} Loses {1} Contact at {2}, {3}", actor.arc.name, contact.job, node.nodeName, node.Arc.name);
                            GameManager.instance.messageScript.ContactChange(text, actor, node, contact, false);
                        }
                        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", nodeID); }
                    }
                    else
                    {
                        Debug.LogWarningFormat("DataManager.cs -> RemoveContactSingle: Contact {0}, ID {1}, NOT removed from {2}, {3},  nodeID {4}", contact.nameFirst, contact.contactID,
                          actor.actorName, actor.arc.name, nodeID);
                    }
                }
                else { Debug.LogErrorFormat("Invalid contact (Null) for nodeID {0}, actor {1}, {2}, ID {3}", nodeID, actor.actorName, actor.arc.name, actor.actorID); }
            }
            else
            { Debug.LogFormat("[Cont] DataManager.cs -> RemoveContactsSingle: REMOVED from {0}, {1}, actorID {2}, at nodeID {3}", actor.actorName, actor.arc.name, actor.actorID, nodeID); }
        }
        else { Debug.LogFormat("DataManager.cs -> Contact NOT Removed (FAIL): {0}, {1}, actorID {2} contact at nodeID {3}{4}", actor.actorName, actor.arc.name, actor.actorID, nodeID, "\n"); }
        //update node contact flags
        if (isSuccess == true)
        {
            GameManager.instance.contactScript.UpdateNodeContacts(isCurrentSide);
            CreateNodeContacts();
        }
        return isSuccess;
    }

    /// <summary>
    /// debug method to add a single contact to an actor at a node
    /// </summary>
    /// <param name="nodeIDString"></param>
    /// <param name="actorSlotIDString"></param>
    /// <returns></returns>
    public string DebugAddContact(string nodeIDString, string actorSlotIDString)
    {
        Debug.Assert(string.IsNullOrEmpty(nodeIDString) == false, "Invalid nodeID string (Null or empty");
        Debug.Assert(string.IsNullOrEmpty(actorSlotIDString) == false, "Invalid actorSlotID string (Null or empty");
        string contactResult = "Unknown";
        int nodeID = Convert.ToInt32(nodeIDString);
        int actorSlotID = Convert.ToInt32(actorSlotIDString);
        Actor actor = GetCurrentActor(actorSlotID, GameManager.instance.turnScript.currentSide);
        if (actor != null)
        {
            if (AddContactSingle(actor.actorID, nodeID) == true)
            {
                contactResult = "Contacted ADDED successfully";
                //update node contact
                CreateNodeContacts();
            }
            else { contactResult = "FAILED to add Contact"; }
        }
        else
        {
            Debug.LogWarningFormat("Invalid actor (Null) for actorSlotID {0}", actorSlotID);
            contactResult = "FAILED to add Contact (Invalid Actor)";
        }
        return contactResult;
    }

    /// <summary>
    /// debug method to remove a single contact from an actor at a node
    /// </summary>
    /// <param name="nodeIDString"></param>
    /// <param name="actorSlotIDString"></param>
    /// <returns></returns>
    public string DebugRemoveContact(string nodeIDString, string actorSlotIDString)
    {
        Debug.Assert(string.IsNullOrEmpty(nodeIDString) == false, "Invalid nodeID string (Null or empty");
        Debug.Assert(string.IsNullOrEmpty(actorSlotIDString) == false, "Invalid actorSlotID string (Null or empty");
        string contactResult = "Unknown";
        int nodeID = Convert.ToInt32(nodeIDString);
        int actorSlotID = Convert.ToInt32(actorSlotIDString);
        Actor actor = GetCurrentActor(actorSlotID, GameManager.instance.turnScript.currentSide);
        if (actor != null)
        {
            if (RemoveContactSingle(actor.actorID, nodeID) == true)
            { contactResult = "Contacted REMOVED successfully"; }
            else { contactResult = "FAILED to Remove Contact"; }
        }
        else
        {
            Debug.LogWarningFormat("Invalid actor (Null) for actorSlotID {0}", actorSlotID);
            contactResult = "FAILED to Remove Contact (Invalid Actor)";
        }
        return contactResult;
    }


    public Dictionary<int, List<Contact>> GetDictOfContactsByNodeResistance()
    { return dictOfContactsByNodeResistance; }

    /// <summary>
    /// loops dictOfContactsByNodeResistance and builds dict from scratch to ensure data is current. Called by other contact methods whenever there is a change in contact status.
    /// Automatically for resistance side as only resistance has contacts
    /// </summary>
    public void CreateNodeContacts()
    {
        int nodeID;
        bool isPresent;
        Contact contact;
        List<int> listOfActors = new List<int>();
        List<Contact> listOfContacts = new List<Contact>();
        //clear dictionary
        dictOfContactsByNodeResistance.Clear();
        //rebuild from scratch
        foreach (var record in dictOfNodeContactsResistance)
        {
            if (record.Value != null)
            {
                nodeID = record.Key;
                listOfActors = record.Value;
                if (listOfActors != null)
                {
                    //loop actors at node
                    for (int i = 0; i < listOfActors.Count; i++)
                    {
                        isPresent = false;
                        Actor actor = GetActor(listOfActors[i]);
                        if (actor != null)
                        {
                            contact = actor.GetContact(nodeID);
                            if (contact != null)
                            {
                                //check if contact already in dict
                                if (dictOfContactsByNodeResistance.ContainsKey(nodeID) == true)
                                {
                                    listOfContacts = dictOfContactsByNodeResistance[nodeID];
                                    //loop list and see if item in dictionary
                                    if (listOfContacts != null)
                                    {
                                        for (int j = 0; j < listOfContacts.Count; j++)
                                        {
                                            if (listOfContacts[j].contactID == contact.contactID)
                                            {
                                                isPresent = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else { Debug.LogErrorFormat("Invalid contact (Null) for actor {0}, {1}, id {2} at nodeID {3}", actor.actorName, actor.arc.name, actor.actorID, nodeID); }
                            if (isPresent == false)
                            {
                                //add a new Contact
                                if (dictOfContactsByNodeResistance.ContainsKey(nodeID) == true)
                                {
                                    //record for node exists, add contact
                                    listOfContacts = dictOfContactsByNodeResistance[nodeID];
                                    listOfContacts.Add(contact);
                                    /*Debug.LogFormat("[Tst] DataManager.cs -> CreateNodeContact: Contact {0} {1}, {2}, id {3} ADDED to nodeID {4}{5}", contact.nameFirst, contact.nameLast, contact.job, 
                                        contact.contactID, nodeID, "\n");*/
                                }
                                else
                                {
                                    //create a new record
                                    List<Contact> listOfNewContacts = new List<Contact>();
                                    listOfNewContacts.Add(contact);
                                    try { dictOfContactsByNodeResistance.Add(nodeID, listOfNewContacts); }
                                    catch (ArgumentException)
                                    { Debug.LogErrorFormat("Invalid new record (duplicate) for nodeId {0}", nodeID); }
                                }
                            }
                        }
                        else
                        { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", listOfActors[i]); }
                    }
                }


            }
            else { Debug.LogError("Invalid record (Null) in dictOfNodeContactsResistance"); }
        }
        //error check number of records
        Debug.Assert(dictOfNodeContactsResistance.Count == dictOfContactsByNodeResistance.Count, string.Format("Mismatched count between dictOfNodeContactsResistance {0} and dictOfContactsByNodeResistance {1}",
            dictOfNodeContactsResistance.Count, dictOfContactsByNodeResistance.Count));
    }

    /*/// <summary> NOTE: made redundant byGetActiveContactsAtNodeAuthority/Resistance
    /// Returns a list of ActorArc names (default Current side to enable both sides tooltips to work correctly while debugging) for all contacts at node. Returns empty string if none.
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public List<string> GetListOfNodeContactActorArcs(int nodeID, bool isCurrentSide = true)
    {
        List<string> listOfNodeContacts = new List<string>();
        //GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        GlobalSide side = GameManager.instance.turnScript.currentSide;
        //If other side, flip sides to opposite side
        if (isCurrentSide == false)
        {
            if (side.level == 1) { side = GameManager.instance.globalScript.sideResistance; }
            else if (side.level == 2) { side = GameManager.instance.globalScript.sideAuthority; }
        }
        //find node in dict
        Dictionary<int, List<int>> dictOfNodeContacts = new Dictionary<int, List<int>>();
        if (side.level == GameManager.instance.sideScript.PlayerSide.level)
        { dictOfNodeContacts = GetDictOfNodeContacts(); }
        else { dictOfNodeContacts = GetDictOfNodeContacts(false); }
        if (dictOfNodeContacts.ContainsKey(nodeID) == true)
        {
            List<int> listOfActors = dictOfNodeContacts[nodeID];
            if (listOfActors != null)
            {
                int numOfActors = listOfActors.Count;
                int actorID;
                if (numOfActors > 0)
                {
                    for (int i = 0; i < numOfActors; i++)
                    {
                        actorID = listOfActors[i];
                        Actor actor = GetActor(actorID);
                        if (actor != null)
                        {
                            //add to list if actor same side as Player
                            if (actor.side.level == side.level)
                            { listOfNodeContacts.Add(actor.arc.name); }

                            //add to list if actor same side as Current side
                            //if (actor.side.level == side.level)
                            //{ listOfNodeContacts.Add(actor.arc.name); }
                        }
                        else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}, nodeID {1}", actorID, nodeID); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid listOfActors (Empty) for nodeID {0}", nodeID); }
            }
            else { Debug.LogWarningFormat("Invalid listOfActors (Null) for nodeID {0}", nodeID); }
        }
        return listOfNodeContacts;
    }*/


    /// <summary>
    /// Returns a list of ActorArc names for all Authority contacts at node. Returns empty string if none.
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public List<string> GetActiveContactsAtNodeAuthority(int nodeID)
    {
        List<string> listOfNodeContacts = new List<string>();
        //find node in dict
        GlobalSide side = GameManager.instance.globalScript.sideAuthority;
        Dictionary<int, List<int>> dictOfNodeContacts = new Dictionary<int, List<int>>();
        dictOfNodeContacts = GetDictOfNodeContacts(side);
        if (dictOfNodeContacts.ContainsKey(nodeID) == true)
        {
            List<int> listOfActors = dictOfNodeContacts[nodeID];
            if (listOfActors != null)
            {
                int numOfActors = listOfActors.Count;
                int actorID;
                if (numOfActors > 0)
                {
                    for (int i = 0; i < numOfActors; i++)
                    {
                        actorID = listOfActors[i];
                        Actor actor = GetActor(actorID);
                        if (actor != null)
                        {
                            /*//add to list if actor same side as Player
                            if (actor.side.level == side.level)
                            { listOfNodeContacts.Add(actor.arc.name); }*/

                            //add to list if actor same side as Current side
                            if (actor.side.level == side.level)
                            { listOfNodeContacts.Add(actor.arc.name); }
                        }
                        else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}, nodeID {1}", actorID, nodeID); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid listOfActors (Empty) for nodeID {0}", nodeID); }
            }
            else { Debug.LogWarningFormat("Invalid listOfActors (Null) for nodeID {0}", nodeID); }
        }
        return listOfNodeContacts;
    }

    /// <summary>
    /// Returns a list of ActorArc names (default Current side to enable both sides tooltips to work correctly while debugging) for all contacts at node. Returns empty string if none. Resostamce side only.
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public List<string> GetActiveContactsAtNodeResistance(int nodeID)
    {
        bool isShowContacts = GameManager.instance.optionScript.showContacts;
        List<string> listOfNodeContacts = new List<string>();
        //get list of contacts at node
        List<Contact> listOfContacts = GetListOfNodeContacts(nodeID);
        if (listOfContacts != null && listOfContacts.Count > 0)
        {
            //check if contacts are active
            foreach (Contact contact in listOfContacts)
            {
                if (contact.status == ContactStatus.Active)
                {
                    //get actor and check if active
                    Actor actor = GetActor(contact.actorID);
                    if (actor != null)
                    {
                        if (actor.Status == ActorStatus.Active)
                        {
                            //add actor arc to list
                            listOfNodeContacts.Add(actor.arc.name);
                            //add contact details if option is on
                            if (isShowContacts == true)
                            {
                                //hard wired alert colour and size
                                listOfNodeContacts.Add(string.Format("<size=90%><color=#FFA07A>{0} {1}, {2}</color></size>", contact.nameFirst, contact.nameLast, contact.job));
                            }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", contact.actorID); }
                }
            }
        }
        return listOfNodeContacts;
    }

    /// <summary>
    /// returns true if the relevant side has at least one active contact with an associated active parent active present at the node
    /// </summary>
    /// <param name="nodeID"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public bool CheckActiveContactAtNode(int nodeID, GlobalSide side)
    {
        bool isActiveContact = false;
        if (side != null)
        {
            switch (side.level)
            {
                case 1:
                    //Authority -> check only if parent actor is active as contacts are automatically active
                    if (dictOfNodeContactsAuthority.ContainsKey(nodeID) == true)
                    {
                        List<int> listOfActors = dictOfNodeContactsAuthority[nodeID];
                        if (listOfActors != null)
                        {
                            for (int i = 0; i < listOfActors.Count; i++)
                            {
                                Actor actor = GetActor(listOfActors[i]);
                                if (actor != null)
                                {
                                    if (actor.Status == ActorStatus.Active)
                                    { isActiveContact = true; break; }
                                }
                                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", listOfActors[i]); }
                            }
                        }
                        else { Debug.LogError("Invalid listOfActors (Null)"); }
                    }
                    break;
                case 2:
                    //Resistance -> both contact and parent actor must be Active
                    List<Contact> listOfContacts = GetListOfNodeContacts(nodeID);
                    if (listOfContacts != null)
                    {
                        for (int i = 0; i < listOfContacts.Count; i++)
                        {
                            Contact contact = listOfContacts[i];
                            if (contact != null)
                            {
                                if (contact.status == ContactStatus.Active)
                                {
                                    //check parent actor is active
                                    Actor actor = GetActor(contact.actorID);
                                    if (actor != null)
                                    {
                                        if (actor.Status == ActorStatus.Active)
                                        { isActiveContact = true; break; }
                                    }
                                    else { Debug.LogErrorFormat("Invalid actor (Null) for contact.actorID {0}", contact.actorID); }
                                }
                            }
                            else { Debug.LogErrorFormat("Invalid contact (Null) for listOfContacts[{0}]", i); }
                        }
                    }
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised side \"{0}\"", side.name);
                    break;
            }
        }
        else { Debug.LogError("Invalid side (Null)"); }
        return isActiveContact;
    }

    /// <summary>
    /// Toggle a contact active or inactive. Returns updated status as a string or 'unknown' if an issue
    /// </summary>
    /// <param name="contactID"></param>
    public string ContactToggleActive(int contactID)
    {
        Contact contact = GetContact(contactID);
        if (contact != null)
        {
            switch (contact.status)
            {
                case ContactStatus.Active: contact.status = ContactStatus.Inactive; break;
                case ContactStatus.Inactive: contact.status = ContactStatus.Active; break;
            }
            Debug.LogFormat("[Cont] DataManager.cs -> ContactToggleActive: {0} {1}, {2}, id {3} STATUS now {4}{5}", contact.nameFirst, contact.nameLast, contact.job, contact.contactID, contact.status, "\n");
            return Convert.ToString(contact.status);
        }
        return "Unknown";
    }

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
        { return obj; }
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
        { return node; }
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


    public Dictionary<int, Node> GetDictOfNodes()
    { return dictOfNodes; }

    public Dictionary<int, NodeD> GetDictOfNodeDUnweighted()
    { return dictOfNodeDUnweighted; }

    public Dictionary<int, NodeD> GetDictOfNodeDWeighted()
    { return dictOfNodeDWeighted; }

    public Dictionary<int, PathData> GetDictOfDijkstraUnweighted()
    { return dictOfDijkstraUnweighted; }

    public Dictionary<int, PathData> GetDictOfDijkstraWeighted()
    { return dictOfDijkstraWeighted; }

    /// <summary>
    /// empties out Weighted Dijkstra collections prior to a recalculation
    /// </summary>
    public void SetWeightedDijkstraDataClear()
    {
        dictOfDijkstraWeighted.Clear();
        dictOfNodeDWeighted.Clear();
    }

    /// <summary>
    /// returns PathData package for relevant Source nodeID (Dijkstra), Null otherwise, Unweighted pathing
    /// </summary>
    /// <param name="sourceNodeID"></param>
    /// <returns></returns>
    public PathData GetDijkstraPathUnweighted(int sourceNodeID)
    {
        if (dictOfDijkstraUnweighted.ContainsKey(sourceNodeID) == true)
        { return dictOfDijkstraUnweighted[sourceNodeID]; }
        return null;
    }

    /// <summary>
    /// returns PathData package for relevant Source nodeID (Dijkstra), Null otherwise, Weighted pathing
    /// </summary>
    /// <param name="sourceNodeID"></param>
    /// <returns></returns>
    public PathData GetDijkstraPathWeighted(int sourceNodeID)
    {
        if (dictOfDijkstraWeighted.ContainsKey(sourceNodeID) == true)
        { return dictOfDijkstraWeighted[sourceNodeID]; }
        return null;
    }

    public List<Node> GetListOfAllNodes()
    { return listOfNodes; }

    public List<Node> GetListOfCrisisNodes()
    { return listOfCrisisNodes; }

    /// <summary>
    /// Get int data from Master node array
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public int CheckNodeInfo(int nodeIndex, NodeInfo info)
    {
        Debug.Assert(nodeIndex > -1 && nodeIndex < CheckNumOfNodeArcs(), "Invalid nodeIndex");
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
        Debug.Assert(nodeIndex > -1 && nodeIndex < CheckNumOfNodeArcs(), "Invalid nodeIndex");
        arrayOfNodes[nodeIndex, (int)info] = newData;
    }

    /// <summary>
    /// return total number of nodes in the level
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfNodes()
    { return dictOfNodeObjects.Count; }

    /*/// <summary>
    /// returns number of different node arc types on level, eg. "Corporate" + "Utility" would return 2
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfNodeTypes()
    { return arrayOfNodes.Length; }*/

    /// <summary>
    /// intialised in ImportManager.cs
    /// </summary>
    public void InitialiseArrayOfNodes()
    { arrayOfNodes = new int[CheckNumOfNodeArcs(), (int)NodeInfo.Count]; }

    public int[,] GetArrayOfNodes()
    { return arrayOfNodes; }


    /// <summary>
    /// return a list of Nodes, all of which are the same type (nodeArcID)
    /// </summary>
    /// <param name="nodeArcID"></param>
    /// <returns></returns>
    public List<Node> GetListOfNodesByType(int nodeArcID)
    {
        Debug.Assert(nodeArcID > -1 && nodeArcID < CheckNumOfNodeArcs(), "Invalid nodeArcID parameter");
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

    /// <summary>
    /// Update list of valid node move options for Player (clears out any previous data)
    /// </summary>
    /// <param name="listOfNodeIDs"></param>
    public void UpdateMoveNodes(List<int> listOfNodeIDs)
    {
        Debug.Assert(listOfNodeIDs != null, "Invalid listOfNodeIDs (Null)");
        listOfMoveNodes.Clear();
        listOfMoveNodes.AddRange(listOfNodeIDs);
    }

    /// <summary>
    /// returns true if nodeID exists in the list of Valid Move Nodes, false otherwise
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public bool CheckValidMoveNode(int nodeID)
    {
        return listOfMoveNodes.Exists(x => x == nodeID);
    }

    /// <summary>
    /// Debug method to display crisis nodes
    /// </summary>
    /// <returns></returns>
    public string DisplayCrisisNodes()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- listOfCrisisNodes{0}", "\n");
        if (listOfCrisisNodes.Count > 0)
        {
            foreach (Node node in listOfCrisisNodes)
            { builder.AppendFormat("{0} {1}, {2}, ID {3}, crisisTimer {4} (\"{5}\")", "\n", node.nodeName, node.Arc.name, node.nodeID, node.crisisTimer, node.crisis.tag); }
        }
        else { builder.AppendFormat("{0} No records found", "\n"); }
        return builder.ToString();
    }

    /// <summary>
    /// pass the top most connected nodes (those with 3+ connections) to the list. Used by AI
    /// </summary>
    /// <param name="listOfConnected"></param>
    public void SetConnectedNodes(List<Node> listOfConnected)
    {
        if (listOfConnected != null)
        {
            listOfMostConnectedNodes.Clear();
            listOfMostConnectedNodes.AddRange(listOfConnected);
        }
        else { Debug.LogWarning("Invalid listOfConnected (Null)"); }
    }

    public List<Node> GetListOfMostConnectedNodes()
    { return listOfMostConnectedNodes; }

    public List<Node> GetListOfLoiterNodes()
    { return listOfLoiterNodes; }

    public List<Node> GetListOfCureNodes()
    { return listOfCureNodes; }

    /// <summary>
    /// add cure to listOfCures, checking for existing duplicates first
    /// </summary>
    /// <param name="node"></param>
    public void AddCureNode(Node node)
    {
        if (node != null)
        {
            if (node.cure != null)
            {
                if (listOfCureNodes.Exists(x => x.cure.cureID == node.cure.cureID) == false)
                { listOfCureNodes.Add(node); }
                else { Debug.LogWarningFormat("Invalid {0} cure (Duplicate) in listOfCures", node.cure.cureName); }
            }
            else { Debug.LogWarningFormat("Invalid cure (Null) for node {0}, {1}, ID {2}", node.nodeName, node.Arc.name, node.nodeID); }
        }
        else { Debug.LogError("Invalid node (Null)"); }
    }

    /// <summary>
    /// returns Node for a specified cure provided it is OnMap (Does NOT check if isActive true/false). Returns Null otherwise
    /// </summary>
    /// <param name="cure"></param>
    /// <returns></returns>
    public Node GetCureNode(Cure cure)
    {
        Node node = null;
        if (cure != null)
        { node = listOfCureNodes.Find(x => x.cure.cureID == cure.cureID); }
        else { Debug.LogError("Invalid cure (Null)"); }
        return node;
    }

    /// <summary>
    /// Remove a specified cure (it's current OnMap node) from listOfCureNodes. Returns true if successful
    /// </summary>
    /// <param name="cure"></param>
    /// <returns></returns>
    public bool RemoveCureNode(Node node)
    {
        bool isSuccess = false;
        if (node != null)
        { isSuccess = listOfCureNodes.Remove(node); }
        else { Debug.LogError("Invalid cure (Null)"); }
        return isSuccess;
    }

    /// <summary>
    /// Switch all onMap cures (at nodes) to isActive 'true'
    /// </summary>
    public void DebugActivateAllCures()
    {
        for (int i = 0; i < listOfCureNodes.Count; i++)
        { listOfCureNodes[i].cure.isActive = true; }
    }

    public List<Node> GetListOfDecisionNodes()
    { return listOfDecisionNodes; }

    public Dictionary<int, NodeCrisis> GetDictOfNodeCrisis()
    { return dictOfNodeCrisis; }

    /// <summary>
    /// returns a NodeCrisis pick list, null if not found
    /// </summary>
    /// <param name="datapoint"></param>
    /// <returns></returns>
    public List<NodeCrisis> GetNodeCrisisList(NodeDatapoint datapoint)
    {
        List<NodeCrisis> tempList = null;
        if (datapoint != null)
        {
            switch (datapoint.level)
            {
                case 0:
                    //stability
                    tempList = listOfCrisisStability;
                    break;
                case 1:
                    //support
                    tempList = listOfCrisisSupport;
                    break;
                case 2:
                    //security
                    tempList = listOfCrisisSecurity;
                    break;
                default:
                    Debug.LogWarningFormat("Invalid NodeDatapoint \"{0}\"", datapoint.name);
                    break;
            }
        }
        else { Debug.LogWarning("Invalid nodeDatapoint (Null)"); }
        return tempList;
    }

    /// <summary>
    /// returns a random Node Crisis from the appropriate pick list, null if none found
    /// </summary>
    /// <param name="datapoint"></param>
    /// <returns></returns>
    public NodeCrisis GetRandomNodeCrisis(NodeDatapoint datapoint)
    {
        int count;
        NodeCrisis crisis = null;
        if (datapoint != null)
        {
            switch (datapoint.level)
            {
                case 0:
                    //stability
                    if (listOfCrisisStability != null)
                    {
                        count = listOfCrisisStability.Count;
                        if (count > 0)
                        { crisis = listOfCrisisStability[Random.Range(0, count)]; }
                    }
                    else { Debug.LogWarning("Invalid listOfCrisisStability (Null)"); }
                    break;
                case 1:
                    //support
                    if (listOfCrisisSupport != null)
                    {
                        count = listOfCrisisSupport.Count;
                        if (count > 0)
                        { crisis = listOfCrisisSupport[Random.Range(0, count)]; }
                    }
                    else { Debug.LogWarning("Invalid listOfCrisisSupport (Null)"); }
                    break;
                case 2:
                    //security
                    if (listOfCrisisSecurity != null)
                    {
                        count = listOfCrisisSecurity.Count;
                        if (count > 0)
                        { crisis = listOfCrisisSecurity[Random.Range(0, count)]; }
                    }
                    else { Debug.LogWarning("Invalid listOfCrisisSecurity (Null)"); }
                    break;
                default:
                    Debug.LogWarningFormat("Invalid NodeDatapoint \"{0}\"", datapoint.name);
                    break;
            }
        }
        else { Debug.LogWarning("Invalid nodeDatapoint (Null)"); }
        return crisis;
    }

    /// <summary>
    /// returns a specific crisis based on nodeCrisisID, null if not found
    /// </summary>
    /// <param name="nodeCrisisID"></param>
    /// <returns></returns>
    public NodeCrisis GetNodeCrisisByID(int nodeCrisisID)
    {
        NodeCrisis crisis = null;
        if (dictOfNodeCrisis.TryGetValue(nodeCrisisID, out crisis))
        {
            return crisis;
        }
        return null;
    }

    /// <summary>
    /// Game start -> add crisis to the appropriate pick lists
    /// </summary>
    /// <param name="datapoint"></param>
    public void AddNodeCrisisToList(NodeCrisis crisis)
    {
        if (crisis != null)
        {
            switch (crisis.datapoint.level)
            {
                case 0:
                    //stability
                    listOfCrisisStability.Add(crisis);
                    break;
                case 1:
                    //support
                    listOfCrisisSupport.Add(crisis);
                    break;
                case 2:
                    //security
                    listOfCrisisSecurity.Add(crisis);
                    break;
                default:
                    Debug.LogWarningFormat("Invalid crisis.datapoint \"{0}\"", crisis.datapoint.name);
                    break;
            }
        }
        else { Debug.LogWarning("Invalid crisis (Null) -> not added to list"); }
    }

    /// <summary>
    /// Centred and Most Connected nodes with at least one no security connection that isn't a dead end. Used by AI
    /// </summary>
    /// <param name="listOfConnected"></param>
    public void SetDecisionNodes(List<Node> listOfNodes)
    {
        if (listOfNodes != null)
        {
            listOfDecisionNodes.Clear();
            listOfDecisionNodes.AddRange(listOfNodes);
        }
        else { Debug.LogWarning("Invalid listOfNodes (Null)"); }
    }

    //
    // - - - Connections - - - 
    //

    public bool AddConnection(Connection connection)
    {
        bool successFlag = true;
        //add to dictionary
        try
        { dictOfConnections.Add(connection.connID, connection); }
        catch (ArgumentNullException)
        { Debug.LogError("Invalid Connection (Null)"); successFlag = false; }
        catch (ArgumentException)
        { Debug.LogError(string.Format("Invalid Connection (duplicate) connID \"{0}\"", connection.connID)); successFlag = false; }
        return successFlag;
    }

    /// <summary>
    /// returns connection with specified ID from dict, "Null" if not found
    /// </summary>
    /// <param name="connectionID"></param>
    /// <returns></returns>
    public Connection GetConnection(int connectionID)
    {
        Connection connection = null;
        if (dictOfConnections.TryGetValue(connectionID, out connection))
        { return connection; }
        return null;
    }

    public Dictionary<int, Connection> GetDictOfConnections()
    { return dictOfConnections; }

    public List<Connection> GetListOfConnections()
    { return listOfConnections; }

    /// <summary>
    /// Returns total number of connections
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfConnections()
    { return dictOfConnections.Count; }




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
        { return target; }
        return null;
    }

    public List<int>[] GetArrayOfGenericTargets()
    { return arrayOfGenericTargets; }

    /// <summary>
    /// gets random generic target for a specific nodeArcID. Returns null if one found or a problem
    /// </summary>
    /// <param name="nodeArcID"></param>
    /// <returns></returns>
    public Target GetRandomGenericTarget(int nodeArcID)
    {
        int targetID = -1;
        List<int> tempList = arrayOfGenericTargets[nodeArcID];
        if (tempList.Count > 0)
        { targetID = tempList[Random.Range(0, tempList.Count)]; }
        if (targetID > -1)
        { return GetTarget(targetID); }
        return null;
    }

    /// <summary>
    /// Initialises generic targets array. Called by LoadManager.cs -> targets (due to sequence issues with # of nodeArcs which determines size of array)
    /// </summary>
    public void InitialiseArrayOfGenericTargets()
    {
        int sizeOfArray = CheckNumOfNodeArcs();
        if (sizeOfArray > 0)
        {
            arrayOfGenericTargets = new List<int>[sizeOfArray];
            for (int i = 0; i < sizeOfArray; i++)
            {
                List<int> tempList = new List<int>();
                arrayOfGenericTargets[i] = tempList;
            }
        }
    }

    /// <summary>
    /// removes a target from a specific generic list to prevent dupes. Returns true if successful.
    /// </summary>
    /// <param name="targetID"></param>
    /// <param name="nodeArcID"></param>
    public bool RemoveTargetFromGenericList(int targetID, int nodeArcID)
    {
        if (arrayOfGenericTargets[nodeArcID].Remove(targetID) == true)
        { return true; }
        return false;
    }

    /// <summary>
    /// returns list all nodes with non-Dormant targets
    /// </summary>
    /// <returns></returns>
    public List<int> GetNodesWithTargetsList()
    { return listOfNodesWithTargets; }

    /// <summary>
    /// returns true if node on list, false otherwise
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public bool CheckNodeOnTargetList(int nodeID)
    { return listOfNodesWithTargets.Exists(x => x == nodeID); }

    /// <summary>
    /// add a nodeID to list that contains all nodes currently with non-Dormant targets. Returns true if successful, false otherwise
    /// </summary>
    /// <param name="nodeID"></param>
    public bool AddNodeToTargetList(int nodeID)
    {
        Debug.Assert(nodeID > -1, "Invalid nodeID (less than Zero)");
        if (listOfNodesWithTargets.Exists(x => x == nodeID) == false)
        { listOfNodesWithTargets.Add(nodeID); }
        else
        {
            Debug.LogWarningFormat("WARNING: nodeID {0} already present in listOfNodesWithTarget", nodeID);
            return false;
        }
        return true;
    }

    /// <summary>
    /// removes a nodeID from list of all nodes with a non-dormant/done target. Returns true if successful, false otherwise
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public bool RemoveNodeFromTargetList(int nodeID)
    { return listOfNodesWithTargets.Remove(nodeID); }

    /// <summary>
    /// gets a random Node that is NOT present in listOfNodesWithTargets. Returns null if a problem
    /// </summary>
    /// <returns></returns>
    public Node GetRandomTargetNode()
    {
        int counter = 0;
        Node node = null;
        bool isSuccess = false;
        do
        {
            counter++;
            node = GetRandomNode();
            if (listOfNodesWithTargets.Exists(x => x == node.nodeID) == false)
            { isSuccess = true; }
            if (counter == 20)
            { Debug.LogFormat("[Tst] DataManager.cs -> GetRandomTargetNode: No target found after {0} iterations{1}", counter, "\n"); }
        }
        while (isSuccess == false && counter < 20);
        //go to manual
        if (isSuccess == false)
        {
            //loop through list of nodes until you find one without a target
            for (int i = 0; i < listOfNodes.Count; i++)
            {
                node = listOfNodes[i];
                if (listOfNodesWithTargets.Exists(x => x == node.nodeID) == false)
                {
                    isSuccess = true;
                    break;
                }
            }
            if (isSuccess == false)
            { Debug.LogFormat("[Tst] DataManager.cs -> GetRandomTargetNode: No target found after a FULL loop through list of nodes{0}", "\n"); }
        }
        return node;
    }

    public Dictionary<int, Target> GetDictOfTargets()
    { return dictOfTargets; }

    /// <summary>
    /// get the specified target pool, Null if not found
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public List<Target> GetTargetPool(Status status)
    {
        List<Target> tempList = null;
        switch (status)
        {
            case Status.Active:
                tempList = targetPoolActive;
                break;
            case Status.Live:
                tempList = targetPoolLive;
                break;
            case Status.Outstanding:
                tempList = targetPoolOutstanding;
                break;
            case Status.Done:
                tempList = targetPoolDone;
                break;
            default:
                Debug.LogError(string.Format("Invalid status \"{0}\"", status));
                break;
        }
        return tempList;
    }


    /// <summary>
    /// Adds target to List (possible is dormant, active, live, completed). Returns true if target added, false otherwise
    /// </summary>
    /// <param name="target"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public bool AddTargetToPool(Target target, Status status)
    {
        bool isSuccess = false;
        if (target != null)
        {
            switch (status)
            {
                /*case Status.Dormant:
                    possibleTargetsPool.Add(target);
                    break;*/
                case Status.Active:
                    targetPoolActive.Add(target);
                    break;
                case Status.Live:
                    targetPoolLive.Add(target);
                    break;
                case Status.Outstanding:
                    targetPoolOutstanding.Add(target);
                    break;
                case Status.Done:
                    targetPoolDone.Add(target);
                    break;
                default:
                    Debug.LogError(string.Format("Invalid target status {0}", status));
                    break;
            }
        }
        else { Debug.LogError("Invalid List target parameter (Null)"); }
        return isSuccess;
    }


    /// <summary>
    /// Removes target from List (possible is dormant, active, live, completed). Returns true if target found and removed, false otherwise
    /// </summary>
    /// <param name="target"></param>
    /// <param name="status"></param>
    public bool RemoveTargetFromPool(Target target, Status status)
    {
        bool isSuccess = false;
        if (target != null)
        {
            List<Target> listOfTargets = new List<Target>();
            switch (status)
            {
                /*case Status.Dormant:
                    listOfTargets = possibleTargetsPool;
                    break;*/
                case Status.Active:
                    listOfTargets = targetPoolActive;
                    break;
                case Status.Live:
                    listOfTargets = targetPoolLive;
                    break;
                case Status.Outstanding:
                    listOfTargets = targetPoolOutstanding;
                    break;
                case Status.Done:
                    listOfTargets = targetPoolDone;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid target status {0}", status));
                    break;
            }
            //remove from list (by reference)
            for (int i = 0; i < listOfTargets.Count; i++)
            {
                Target targetList = listOfTargets[i];
                if (targetList.targetID == target.targetID)
                {
                    listOfTargets.RemoveAt(i);
                    isSuccess = true;
                    Debug.Log(string.Format("DataManager: Target \"{0}\", ID {1}, successfully removed from {2} List{3}", target.targetName, target.targetID, status, "\n"));
                    break;
                }
            }
        }
        else { Debug.LogError("Invalid List target parameter (Null)"); }
        return isSuccess;
    }

    /// <summary>
    /// Debug method to display contents of generic target array
    /// </summary>
    /// <returns></returns>
    public string DebugShowGenericTargets()
    {
        StringBuilder builder = new StringBuilder();
        Target target = null;
        List<int> tempList = new List<int>();
        builder.AppendFormat(" ArrayOfGenericTargets{0}", "\n");
        for (int i = 0; i < arrayOfGenericTargets.Length; i++)
        {
            builder.AppendFormat("{0} NodeArc -> {1}{2}", "\n", GetNodeArc(i).name, "\n");
            tempList = arrayOfGenericTargets[i];
            if (tempList != null)
            {
                if (tempList.Count > 0)
                {
                    for (int j = 0; j < tempList.Count; j++)
                    {
                        target = GetTarget(tempList[j]);
                        if (target != null)
                        {
                            builder.AppendFormat(" {0}, id {1}, level {2}, act {3}, del {4}, win {5}{6}", target.targetName, target.targetID, target.targetLevel,
                                target.profile.activation.name, target.timerDelay, target.timerWindow, "\n");
                        }
                        else { builder.AppendFormat(" INVALID Target (Null){0}", "\n"); }
                    }
                }
                else { builder.AppendFormat(" No Targets present{0}", "\n"); }
            }
            else { builder.AppendFormat(" INVALID List (Null){0}", "\n"); }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Debug method to display different target pools
    /// </summary>
    /// <returns></returns>
    public string DebugShowTargetPools()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat(" Target Pools{0}", "\n");
        builder.AppendFormat("{0} Active Targets{1}", "\n", "\n");
        builder.Append(DebugDisplayPool(targetPoolActive));
        builder.AppendFormat("{0} Live Targets{1}", "\n", "\n");
        builder.Append(DebugDisplayPool(targetPoolLive));
        builder.AppendFormat("{0} Outstanding Targets{1}", "\n", "\n");
        builder.Append(DebugDisplayPool(targetPoolOutstanding));
        builder.AppendFormat("{0} Done Targets{1}", "\n", "\n");
        builder.Append(DebugDisplayPool(targetPoolDone));
        return builder.ToString();
    }

    /// <summary>
    /// Debug submethod for DebugShowTargetPools
    /// </summary>
    /// <param name="tempList"></param>
    /// <returns></returns>
    private string DebugDisplayPool(List<Target> tempList)
    {
        StringBuilder builderTemp = new StringBuilder();
        Target target = null;
        Node node = null;
        if (tempList != null)
        {
            if (tempList.Count > 0)
            {
                for (int i = 0; i < tempList.Count; i++)
                {
                    target = tempList[i];
                    if (target != null)
                    {
                        node = GetNode(target.nodeID);
                        if (node != null)
                        {
                            builderTemp.AppendFormat(" {0}, id {1}, lvl {2}, act {3}, {4}, d {5}, w {6}, {7}, {8}, id {9}{10}", target.targetName, target.targetID, target.targetLevel, target.profile.activation.name,
                                target.targetStatus, target.timerDelay, target.timerWindow, node.nodeName, node.Arc.name, target.nodeID, "\n");
                        }
                        else
                        {
                            //no error 'cause you want to pick up targets without node data as they are 'Done' targets
                            builderTemp.AppendFormat(" {0}, id {1}, lvl {2}, act {3}, {4}, d {5}, w {6}, id {7}{8}", target.targetName, target.targetID, target.targetLevel, target.profile.activation.name,
                                target.targetStatus, target.timerDelay, target.timerWindow, target.nodeID, "\n");
                        }
                    }
                    else { builderTemp.AppendFormat(" INVALID Target (Null){0}", "\n"); }
                }
            }
            else { builderTemp.AppendFormat(" No targets present{0}", "\n"); }
        }
        return builderTemp.ToString();
    }

    /// <summary>
    /// Debug method to display target dictionary
    /// </summary>
    /// <returns></returns>
    public string DebugShowTargetDict()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat(" TargetDictionary{0}{1}", "\n", "\n");
        foreach (var target in dictOfTargets)
        {
            if (target.Value.followOnTarget != null)
            {
                if (target.Value.profile.activation != null)
                {
                    //non-dormant but has follow-on target & valid activation
                    builder.AppendFormat(" id {0}: {1}, lvl {2}, act {3}, {4}, d {5}, w {6}, nodeID {7}, follow id {8}{9}", target.Value.targetID, target.Value.name, target.Value.targetLevel,
                        target.Value.profile.activation.name, target.Value.targetStatus, target.Value.timerDelay, target.Value.timerWindow, target.Value.nodeID, target.Value.followOnTarget.targetID, "\n");
                }
                else
                {
                    //dormant ->  follow-on target & NO valid activation
                    builder.AppendFormat(" id {0}: {1}, lvl {2}, act n.a, {3}, d {4}, w {5}, nodeID {6}, follow id {7}{8}", target.Value.targetID, target.Value.name, target.Value.targetLevel,
                        target.Value.targetStatus, target.Value.timerDelay, target.Value.timerWindow, target.Value.nodeID, target.Value.followOnTarget.targetID, "\n");
                }
            }
            else
            {
                //No followOn target
                if (target.Value.profile.activation != null)
                {
                    //active / live / outstanding -> valid activation
                    builder.AppendFormat(" id {0}: {1}, lvl {2}, act {3}, {4}, d {5}, w {6}, nodeID {7}, follow id n.a{8}", target.Value.targetID, target.Value.name, target.Value.targetLevel,
                          target.Value.profile.activation.name, target.Value.targetStatus, target.Value.timerDelay, target.Value.timerWindow, target.Value.nodeID, "\n");
                }
                else
                {
                    //dormant -> no activation value
                    builder.AppendFormat(" id {0}: {1}, lvl {2}, act n.a, {3}, d {4}, w {5}, nodeID {6}, tar id n.a{7}", target.Value.targetID, target.Value.name, target.Value.targetLevel,
                          target.Value.targetStatus, target.Value.timerDelay, target.Value.timerWindow, target.Value.nodeID, "\n");
                }
            }
        }
        return builder.ToString();
    }

    //
    // - - - Teams & TeamArcs & TeamPools - - -
    //

    /// <summary>
    /// number of TeamArcs in dictOfTeamArcs
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfTeamArcs()
    { return dictOfTeamArcs.Count; }

    /// <summary>
    /// number of Teams in dictOfTeams
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfTeams()
    { return dictOfTeams.Count; }

    /// <summary>
    /// returns int data from arrayOfTeams based on teamArcID and TeamInfo enum
    /// </summary>
    /// <param name="teamArcID"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public int CheckTeamInfo(int teamArcID, TeamInfo info)
    {
        Debug.Assert(teamArcID > -1 && teamArcID < CheckNumOfTeamArcs(), "Invalid teamArcID");
        return arrayOfTeams[teamArcID, (int)info];
    }

    /// <summary>
    /// return a list of teamArc ID's from dictOfTeamArcs
    /// </summary>
    /// <returns></returns>
    public List<int> GetTeamArcIDs()
    { return new List<int>(dictOfTeamArcs.Keys); }

    /// <summary>
    /// adjust a data point by the input amount, eg. +1, -2, etc. Min capped at 0.
    /// ONLY CALL THIS WHEN FIRST SETTING UP TEAMS or adding additional teams. MoveTeam handles all interpool admin and calls this methiod internally
    /// </summary>
    /// <param name="teamArcID"></param>
    /// <param name="info"></param>
    /// <param name="adjustment"></param>
    public void AdjustTeamInfo(int teamArcID, TeamInfo info, int adjustment)
    {
        Debug.Assert(teamArcID > -1 && teamArcID < CheckNumOfTeamArcs(), "Invalid teamArcID");
        int afterValue = arrayOfTeams[teamArcID, (int)info] + adjustment;
        arrayOfTeams[teamArcID, (int)info] = Math.Max(0, afterValue);
    }

    /// <summary>
    /// returns TeamArcID of named teamArc type. returns '-1' if not found in dict. teamArcName must be in CAPS
    /// </summary>
    /// <param name="teamArcName"></param>
    /// <returns></returns>
    public int GetTeamArcID(string teamArcName)
    {
        if (dictOfLookUpTeamArcs.ContainsKey(teamArcName))
        { return dictOfLookUpTeamArcs[teamArcName]; }
        else { Debug.LogWarning(string.Format("Not found in Lookup TeamArcID dict \"{0}\"{1}", teamArcName, "\n")); }
        return -1;
    }

    /// <summary>
    /// returns dictOfTeamArcs
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, TeamArc> GetDictOfTeamArcs()
    { return dictOfTeamArcs; }

    public Dictionary<string, int> GetDictOfLookUpTeamArcs()
    { return dictOfLookUpTeamArcs; }

    /// <summary>
    /// called from ImportManager.cs
    /// </summary>
    /// <param name="numOfTeams"></param>
    /// <param name="numOfTeamInfo"></param>
    public void InitialiseArrayOfTeams(int numOfTeams, int numOfTeamInfo)
    { arrayOfTeams = new int[numOfTeams, numOfTeamInfo]; }


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
    /// Add team to dictOfTeams, returns true if successful
    /// </summary>
    /// <param name="team"></param>
    public bool AddTeamToDict(Team team)
    {
        bool successFlag = true;
        //add to dictionary
        try
        { dictOfTeams.Add(team.teamID, team); }
        catch (ArgumentNullException)
        { Debug.LogError("Invalid Team (Null)"); successFlag = false; }
        catch (ArgumentException)
        { Debug.LogError(string.Format("Invalid Team (duplicate) TeamID \"{0}\" for {1} \"{2}\"{3}", team.teamID, team.arc.name, team.teamName, "\n")); successFlag = false; }
        return successFlag;
    }

    /// <summary>
    /// Adds teamID to a particular pool
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="teamID"></param>
    public void AddTeamToPool(TeamPool pool, int teamID)
    {
        switch (pool)
        {
            case TeamPool.Reserve:
                teamPoolReserve.Add(teamID);
                break;
            case TeamPool.OnMap:
                teamPoolOnMap.Add(teamID);
                break;
            case TeamPool.InTransit:
                teamPoolInTransit.Add(teamID);
                break;
            default:
                Debug.LogError(string.Format("Invalid team pool \"{0}\"", pool));
                break;
        }
    }

    /// <summary>
    /// Remove a team from a designated pool. Returns true if successful, false otherwise
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="teamID"></param>
    public bool RemoveTeamFromPool(TeamPool pool, int teamID)
    {
        bool isSuccess = false;
        switch (pool)
        {
            case TeamPool.Reserve:
                isSuccess = teamPoolReserve.Remove(teamID);
                break;
            case TeamPool.OnMap:
                isSuccess = teamPoolOnMap.Remove(teamID);
                break;
            case TeamPool.InTransit:
                isSuccess = teamPoolInTransit.Remove(teamID);
                break;
            default:
                Debug.LogError(string.Format("Invalid team pool \"{0}\"", pool));
                break;
        }
        return isSuccess;
    }

    /// <summary>
    /// returns a list of teamID's for the specified pool. Returns null if not found
    /// </summary>
    /// <param name="pool"></param>
    /// <returns></returns>
    public List<int> GetTeamPool(TeamPool pool)
    {
        List<int> tempList = null;
        switch (pool)
        {
            case TeamPool.Reserve:
                tempList = teamPoolReserve;
                break;
            case TeamPool.OnMap:
                tempList = teamPoolOnMap;
                break;
            case TeamPool.InTransit:
                tempList = teamPoolInTransit;
                break;
            default:
                Debug.LogError(string.Format("Invalid team pool \"{0}\"", pool));
                break;
        }
        return tempList;
    }

    /// <summary>
    /// returns the teamID of the next team of the specified type (teamArc) in the specified pool, '-1' if none found
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="teamArcID"></param>
    /// <returns></returns>
    public int GetTeamInPool(TeamPool pool, int teamArcID)
    {
        List<int> tempList = new List<int>();
        switch (pool)
        {
            case TeamPool.Reserve:
                tempList.AddRange(teamPoolReserve);
                break;
            case TeamPool.OnMap:
                tempList.AddRange(teamPoolOnMap);
                break;
            case TeamPool.InTransit:
                tempList.AddRange(teamPoolInTransit);
                break;
            default:
                Debug.LogError(string.Format("Invalid team pool \"{0}\"", pool));
                break;
        }
        if (tempList.Count > 0)
        {
            //loop list of teamID's looking for a matching teamArc
            for (int i = 0; i < tempList.Count; i++)
            {
                if (GetTeam(tempList[i]).arc.TeamArcID == teamArcID)
                { return tempList[i]; }
            }
        }
        //failed search
        return -1;
    }

    /// <summary>
    /// Gets team from dictionary based on teamID, returns Null if not found
    /// </summary>
    /// <param name="teamID"></param>
    /// <returns></returns>
    public Team GetTeam(int teamID)
    {
        if (dictOfTeams.ContainsKey(teamID))
        { return dictOfTeams[teamID]; }
        else { Debug.LogWarning(string.Format("TeamID {0} not found in dictOfTeams {1}", teamID, "\n")); }
        return null;
    }

    /// <summary>
    /// return dictOfTeams
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, Team> GetDictOfTeams()
    { return dictOfTeams; }



    /// <summary>
    /// returns number of teams in each pool (lists of teamIDs), '-1' if an error
    /// </summary>
    /// <param name="pool"></param>
    /// <returns></returns>
    public int CheckTeamPoolCount(TeamPool pool)
    {
        int num = -1;
        switch (pool)
        {
            case TeamPool.Reserve: num = teamPoolReserve.Count; break;
            case TeamPool.OnMap: num = teamPoolOnMap.Count; break;
            case TeamPool.InTransit: num = teamPoolInTransit.Count; break;
            default: Debug.LogError(string.Format("Invalid pool \"{0}\'", pool)); break;
        }
        return num;
    }

    /// <summary>
    /// returns a list of available team types (arc names) for deployment for an 'ANY TEAM' situation for the button tooltip. Returns "None Available" if none
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public List<string> GetAvailableReserveTeams(Node node)
    {
        int teamArcID;
        List<string> tempList = new List<string>();                 //return list of team name strings
        List<int> tempArcs = new List<int>();                       //list of unique team arcs present at node
        List<int> duplicatesList = new List<int>();                 //prevents duplicate team names being returned

        if (node != null)
        {
            List<Team> listOfTeams = node.GetTeams();
            if (listOfTeams.Count > 0)
            {
                for (int i = 0; i < listOfTeams.Count; i++)
                {
                    teamArcID = listOfTeams[i].arc.TeamArcID;
                    //if not present in list Of Arcs (tempArcs) then add
                    if (tempArcs.Exists(x => x == teamArcID) == true)
                    { tempList.Add(listOfTeams[i].arc.name); }
                }
            }
        }
        else
        { Debug.LogError("Invalid node (Null)"); }
        if (tempList.Count == 0)
        {
            //loop reserve pool
            for (int i = 0; i < teamPoolReserve.Count; i++)
            {
                Team team = GetTeam(teamPoolReserve[i]);
                //check team not present at node
                if (tempArcs.Exists(x => x == team.arc.TeamArcID) == false)
                {
                    //check team not present in duplicatesList
                    if (duplicatesList.Exists(x => x == team.arc.TeamArcID) == false)
                    {
                        //add team type name to both return list & duplicates list
                        tempList.Add(team.arc.name);
                        duplicatesList.Add(team.arc.TeamArcID);
                    }
                }
            }
            if (tempList.Count == 0)
            { tempList.Add("No Teams available"); }
        }
        return tempList;
    }


    //
    // - - - Actors - - -
    //

    /// <summary>
    /// add a currently active actor to the arrayOfActors and update Actor status, states and actorSlotID. Also updates ActorUI
    /// </summary>
    /// <param name="side"></param>
    /// <param name="actor"></param>
    /// <param name="slotID"></param>
    public void AddCurrentActor(GlobalSide side, Actor actor, int slotID)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid slotID input");
        if (actor != null)
        {
            arrayOfActors[side.level, slotID] = actor;
            //ensure position is set to 'Filled'
            arrayOfActorsPresent[side.level, slotID] = true;
            //update actor
            actor.actorSlotID = slotID;
            actor.ResetStates();
            actor.Status = ActorStatus.Active;
            //update contacts (not for game start -> sequencing issues)
            if (GameManager.instance.turnScript.Turn > 0)
            { GameManager.instance.contactScript.SetActorContacts(actor); }
            //update actor GUI display
            GameManager.instance.actorPanelScript.UpdateActorPanel();
        }
        else { Debug.LogError("Invalid actor (null)"); }
    }

    /// <summary>
    /// Removes current actor and handles all relevant admin details. Returns true if actor removed successfully
    /// NOTE: Actor status will be updated if operation successful otherwise no change
    /// </summary>
    /// <param name="side"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    public bool RemoveCurrentActor(GlobalSide side, Actor actor, ActorStatus status)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        if (actor != null)
        {

            //admin depends on where actor is going
            switch (status)
            {
                case ActorStatus.Reserve:
                    if (AddActorToReserve(actor.actorID, side) == true)
                    {
                        RemoveActorAdmin(side, actor, status);
                        return true;
                    }
                    else
                    { Debug.LogWarning("RemoveCurrentActor: Reserve Pool is Full"); }
                    break;
                case ActorStatus.Dismissed:
                    if (AddActorToDismissed(actor.actorID, side) == true)
                    {
                        RemoveActorAdmin(side, actor, status);
                        return true;
                    }
                    break;
                case ActorStatus.Promoted:
                    if (AddActorToPromoted(actor.actorID, side) == true)
                    {
                        RemoveActorAdmin(side, actor, status);
                        return true;
                    }
                    break;
                case ActorStatus.Killed:
                    if (AddActorToDisposedOf(actor.actorID, side) == true)
                    {
                        RemoveActorAdmin(side, actor, status);
                        return true;
                    }
                    break;
                case ActorStatus.Resigned:
                    if (AddActorToResigned(actor.actorID, side) == true)
                    {
                        RemoveActorAdmin(side, actor, status);
                        return true;
                    }
                    break;
                default:
                    Debug.LogError(string.Format("Invalid Status \"{0}\" for {1}, {2}, ID {3}", status, actor.arc.name, actor.actorName, actor.actorID));
                    break;
            }
        }
        else { Debug.LogError("Invalid actor (Null)"); }
        return false;
    }

    /// <summary>
    /// subMethod for RemoveActor to handle all the admin details
    /// </summary>
    /// <param name="side"></param>
    /// <param name="actor"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    private void RemoveActorAdmin(GlobalSide side, Actor actor, ActorStatus status)
    {
        //update actor arrays
        arrayOfActors[side.level, actor.actorSlotID] = null;
        arrayOfActorsPresent[side.level, actor.actorSlotID] = false;
        actor.Status = status;
        //update node contacts
        RemoveContactsActor(actor.actorID);
        //handle special cases
        switch (status)
        {
            case ActorStatus.Resigned:
                //if actor resigned, loose -1 faction support
                int approvalChange = GameManager.instance.factionScript.factionApprovalActorResigns * -1;
                GameManager.instance.factionScript.ChangeFactionApproval(approvalChange, side, string.Format("{0}, {1} has Resigned", actor.actorName, actor.arc.name));
                //lose secrets
                GameManager.instance.secretScript.RemoveAllSecretsFromActor(actor);
                break;
            case ActorStatus.Dismissed:
            case ActorStatus.Promoted:
            case ActorStatus.Killed:
            case ActorStatus.RecruitPool:
                //lose secrets
                GameManager.instance.secretScript.RemoveAllSecretsFromActor(actor);
                break;
        }
        //update actor GUI display
        GameManager.instance.actorPanelScript.UpdateActorPanel();

    }

    /// <summary>
    /// Remove actor from reserve pool list
    /// </summary>
    /// <param name="side"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    public bool RemoveActorFromReservePool(GlobalSide side, Actor actor)
    {
        bool isSuccess = true;
        List<int> reservePoolList = GetActorList(side, ActorList.Reserve);
        if (reservePoolList != null)
        {
            if (reservePoolList.Remove(actor.actorID) == false)
            { Debug.LogWarningFormat("Actor \"{0}\", ID {1}, not found in reservePoolList", actor.actorName, actor.actorID); isSuccess = false; }
        }
        else { Debug.LogWarningFormat("Invalid reservePoolList (Null) for GlobalSide {0}", side); isSuccess = false; }
        return isSuccess;
    }

    /// <summary>
    /// Adds any actor (whether current or reserve) to dictOfActors, returns true if successful
    /// </summary>
    /// <param name="actor"></param>
    public bool AddActorToDict(Actor actor)
    {
        bool successFlag = true;
        //add to dictionary
        try
        { dictOfActors.Add(actor.actorID, actor); }
        catch (ArgumentNullException)
        { Debug.LogError("Invalid Actor (Null)"); successFlag = false; }
        catch (ArgumentException)
        { Debug.LogError(string.Format("Invalid Actor (duplicate) actorID \"{0}\" for {1} \"{2}\"", actor.actorID, actor.arc.name, actor.actorName)); successFlag = false; }
        return successFlag;
    }

    /// <summary>
    /// Adds an actor to one of the three (by level and side) pools from which actors can be recruited from
    /// </summary>
    /// <param name="level"></param>
    public void AddActorToPool(int actorID, int level, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(level > 0 && level < 4, "Invalid actor level");
        Debug.Assert(actorID > -1 && actorID < dictOfActors.Count, "Invalid actorID");
        switch (side.name)
        {
            case "Authority":
                switch (level)
                {
                    case 1: authorityActorPoolLevelOne.Add(actorID); break;
                    case 2: authorityActorPoolLevelTwo.Add(actorID); break;
                    case 3: authorityActorPoolLevelThree.Add(actorID); break;
                }
                break;
            case "Resistance":
                switch (level)
                {
                    case 1: resistanceActorPoolLevelOne.Add(actorID); break;
                    case 2: resistanceActorPoolLevelTwo.Add(actorID); break;
                    case 3: resistanceActorPoolLevelThree.Add(actorID); break;
                }
                break;
            default:
                Debug.LogWarning(string.Format("Invalid Side \"{0}\", actorID NOT added to pool", side.name));
                break;
        }
    }

    /// <summary>
    /// Removes an actor from one of the three (by level and side) pools from which actors can be recruited from
    /// </summary>
    /// <param name="actorID"></param>
    /// <param name="level"></param>
    /// <param name="side"></param>
    public void RemoveActorFromPool(int actorID, int level, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(level > 0 && level < 4, "Invalid actor level");
        Debug.Assert(actorID > -1 && actorID < dictOfActors.Count, "Invalid actorID");
        switch (side.name)
        {
            case "Authority":
                switch (level)
                {
                    case 1: authorityActorPoolLevelOne.Remove(actorID); break;
                    case 2: authorityActorPoolLevelTwo.Remove(actorID); break;
                    case 3: authorityActorPoolLevelThree.Remove(actorID); break;
                }
                break;
            case "Resistance":
                switch (level)
                {
                    case 1: resistanceActorPoolLevelOne.Remove(actorID); break;
                    case 2: resistanceActorPoolLevelTwo.Remove(actorID); break;
                    case 3: resistanceActorPoolLevelThree.Remove(actorID); break;
                }
                break;
            default:
                Debug.LogWarning(string.Format("Invalid Side \"{0}\", actorID NOT removed from pool", side.name));
                break;
        }
    }

    /// <summary>
    /// add an actor to the reserve pool for that side. Returns true if successful (checks if pool is full)
    /// </summary>
    /// <param name="actorID"></param>
    /// <param name="side"></param>
    public bool AddActorToReserve(int actorID, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(actorID > -1 && actorID < dictOfActors.Count, "Invalid actorID");
        bool successFlag = true;
        switch (side.name)
        {
            case "Authority":
                //check space in Authority reserve pool
                if (authorityActorReserve.Count < GameManager.instance.actorScript.maxNumOfReserveActors)
                { authorityActorReserve.Add(actorID); }
                else { successFlag = false; }
                break;
            case "Resistance":
                //check space in Resistance reserve pool
                if (resistanceActorReserve.Count < GameManager.instance.actorScript.maxNumOfReserveActors)
                { resistanceActorReserve.Add(actorID); }
                else { successFlag = false; }
                break;
            default:
                Debug.LogWarning(string.Format("Invalid Side \"{0}\", actorID NOT added to list", side.name));
                successFlag = false;
                break;
        }
        return successFlag;
    }

    /// <summary>
    /// add an actor to the Dismissed pool for that side. Returns ture if successful
    /// </summary>
    /// <param name="actorID"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public bool AddActorToDismissed(int actorID, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(actorID > -1 && actorID < dictOfActors.Count, "Invalid actorID");
        bool successFlag = true;
        switch (side.name)
        {
            case "Authority":
                authorityActorDismissed.Add(actorID);
                break;
            case "Resistance":
                resistanceActorDismissed.Add(actorID);
                break;
            default:
                Debug.LogWarning(string.Format("Invalid Side \"{0}\", actorID NOT added to list", side.name));
                successFlag = false;
                break;
        }
        return successFlag;
    }

    /// <summary>
    /// add an actor to the Promoted pool for that side. Returns ture if successful
    /// </summary>
    /// <param name="actorID"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public bool AddActorToPromoted(int actorID, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(actorID > -1 && actorID < dictOfActors.Count, "Invalid actorID");
        bool successFlag = true;
        switch (side.name)
        {
            case "Authority":
                authorityActorPromoted.Add(actorID);
                break;
            case "Resistance":
                resistanceActorPromoted.Add(actorID);
                break;
            default:
                Debug.LogWarning(string.Format("Invalid Side \"{0}\", actorID NOT added to list", side.name));
                successFlag = false;
                break;
        }
        return successFlag;
    }

    /// <summary>
    /// add an actor to the Disposed Of pool for that side. Returns true if successful
    /// </summary>
    /// <param name="actorID"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public bool AddActorToDisposedOf(int actorID, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(actorID > -1 && actorID < dictOfActors.Count, "Invalid actorID");
        bool successFlag = true;
        switch (side.name)
        {
            case "Authority":
                authorityActorDisposedOf.Add(actorID);
                break;
            case "Resistance":
                resistanceActorDisposedOf.Add(actorID);
                break;
            default:
                Debug.LogWarning(string.Format("Invalid Side \"{0}\", actorID NOT added to list", side.name));
                successFlag = false;
                break;
        }
        return successFlag;
    }


    public bool AddActorToResigned(int actorID, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(actorID > -1 && actorID < dictOfActors.Count, "Invalid actorID");
        bool successFlag = true;
        switch (side.name)
        {
            case "Authority":
                authorityActorResigned.Add(actorID);
                break;
            case "Resistance":
                resistanceActorResigned.Add(actorID);
                break;
            default:
                Debug.LogWarning(string.Format("Invalid Side \"{0}\", actorID NOT added to list", side.name));
                successFlag = false;
                break;
        }
        return successFlag;
    }

    /// <summary>
    /// returns number of actors currently in the relevant reserve pool (auto figures out side from optionManager.cs -> playerSide). '0' if an issue.
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfActorsInReserve()
    {
        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { return authorityActorReserve.Count; }
        else if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
        { return resistanceActorReserve.Count; }
        else
        {
            Debug.LogWarning(string.Format("Invalid Side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
            return 0;
        }
    }

    /// <summary>
    /// Get list of actors in a specific list. Returns Null if a problem
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public List<int> GetActorList(GlobalSide side, ActorList list)
    {
        List<int> listOfActors = null;
        switch (side.name)
        {
            case "Authority":
                switch (list)
                {
                    case ActorList.Reserve:
                        listOfActors = authorityActorReserve;
                        break;
                    case ActorList.Dismissed:
                        listOfActors = authorityActorDismissed;
                        break;
                    case ActorList.Promoted:
                        listOfActors = authorityActorPromoted;
                        break;
                    case ActorList.Disposed:
                        listOfActors = authorityActorDisposedOf;
                        break;
                    default:
                        Debug.LogWarning(string.Format("Invalid ActorList \"{0}\"", list));
                        break;
                }
                break;
            case "Resistance":
                switch (list)
                {
                    case ActorList.Reserve:
                        listOfActors = resistanceActorReserve;
                        break;
                    case ActorList.Dismissed:
                        listOfActors = resistanceActorDismissed;
                        break;
                    case ActorList.Promoted:
                        listOfActors = resistanceActorPromoted;
                        break;
                    case ActorList.Disposed:
                        listOfActors = resistanceActorDisposedOf;
                        break;
                    default:
                        Debug.LogWarning(string.Format("Invalid ActorList \"{0}\"", list));
                        break;
                }
                break;
            default:
                Debug.LogWarning(string.Format("Invalid side \"{0}\"", side));
                break;
        }
        return listOfActors;
    }

    /// <summary>
    /// return a list (of a specified level and side in the pick pool) of actorID's. Returns null if a problem.
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public List<int> GetActorRecruitPool(int level, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(level > 0 && level < 4, "Invalid actor level");
        if (side.level == GameManager.instance.globalScript.sideAuthority.level)
        {
            if (level == 1) { return authorityActorPoolLevelOne; }
            else if (level == 2) { return authorityActorPoolLevelTwo; }
            else { return authorityActorPoolLevelThree; }
        }
        else if (side.level == GameManager.instance.globalScript.sideResistance.level)
        {
            if (level == 1) { return resistanceActorPoolLevelOne; }
            else if (level == 2) { return resistanceActorPoolLevelTwo; }
            else { return resistanceActorPoolLevelThree; }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Side \"{0}\"", side.name));
            return null;
        }
    }

    /// <summary>
    /// Get array of OnMap (active and inactive) actors for a specified side
    /// </summary>
    /// <returns></returns>
    public Actor[] GetCurrentActors(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        int total = GameManager.instance.actorScript.maxNumOfOnMapActors;
        Actor[] tempArray = new Actor[total];
        for (int i = 0; i < total; i++)
        { tempArray[i] = arrayOfActors[side.level, i]; }
        return tempArray;
    }


    public Actor GetActor(int actorID)
    {
        Debug.Assert(actorID > -1 && actorID < dictOfActors.Count, string.Format("Invalid actorID {0}", actorID));
        if (dictOfActors.ContainsKey(actorID))
        { return dictOfActors[actorID]; }
        else { Debug.LogWarning(string.Format("Not found in actorID {0}, in dictOfActors", actorID)); }
        return null;
    }

    /// <summary>
    /// Get specific actor (OnMap, active or inactive). Run CheckActorSlotStatus first to check if actor present in slot
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public Actor GetCurrentActor(int slotID, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.maxNumOfOnMapActors, string.Format("Invalid slotID {0}", slotID));
        return arrayOfActors[side.level, slotID];
    }

    /// <summary>
    /// returns type of Actor, eg. 'Fixer', based on slotID (0 to 3). Run CheckActorSlotStatus first to check if actor present in slot
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public string GetCurrentActorType(int slotID, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid slotID input");
        return arrayOfActors[side.level, slotID].arc.name;
    }

    /// <summary>
    /// returns a list containing the actorArcID's of all current, OnMap, actors (active or inactive) for a side. Null if a problem.
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public List<int> GetAllCurrentActorArcIDs(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        List<int> tempList = new List<int>();
        for (int i = 0; i < GameManager.instance.actorScript.maxNumOfOnMapActors; i++)
        {
            if (CheckActorSlotStatus(i, side) == true)
            { tempList.Add(arrayOfActors[side.level, i].arc.ActorArcID); }
        }
        if (tempList.Count > 0) { return tempList; }
        return null;
    }

    /// <summary>
    /// returns array of Stats for an OnMap actor-> [0] dataPoint0, [1] dataPoint1 , [2] dataPoint3
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public int[] GetActorStats(int slotID, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid slotID input");
        int[] arrayOfStats = new int[]{ arrayOfActors[side.level, slotID].datapoint0, arrayOfActors[side.level, slotID].datapoint1,
            arrayOfActors[side.level, slotID].datapoint2};
        return arrayOfStats;
    }

    /// <summary>
    /// returns a specific actor's action
    /// </summary>
    /// <param name="slotID"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public Action GetActorAction(int slotID, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid slotID input");
        return arrayOfActors[side.level, slotID].arc.nodeAction;
    }

    /// <summary>
    /// returns slotID of actor if present and available (live), '-1' if not
    /// </summary>
    /// <param name="actorArcID"></param>
    /// <returns></returns>
    public int CheckActorPresent(int actorArcID, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        int slotID = -1;
        int numOfActors = GameManager.instance.actorScript.maxNumOfOnMapActors;
        for (int i = 0; i < numOfActors; i++)
        {
            //check if there is an actor in the slot (not vacant)
            if (CheckActorSlotStatus(i, side) == true)
            {
                Actor actor = arrayOfActors[side.level, i];
                if (actor.arc.ActorArcID == actorArcID && actor.Status == ActorStatus.Active)
                { return actor.actorSlotID; }
            }
        }
        return slotID;
    }

    /// <summary>
    /// returns true if specified actor Arc is present in OnMap line up and active (default -> to ignore this condition set isActiveIgnore to true), false otherwise
    /// </summary>
    /// <param name="arc"></param>
    /// <returns></returns>
    public bool CheckActorArcPresent(ActorArc arc, GlobalSide side, bool isActiveIgnore = false)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        if (arc != null)
        {
            int numOfActors = GameManager.instance.actorScript.maxNumOfOnMapActors;
            for (int i = 0; i < numOfActors; i++)
            {
                //check if there is an actor in the slot (not vacant)
                if (CheckActorSlotStatus(i, side) == true)
                {
                    Actor actor = arrayOfActors[side.level, i];
                    //check actor is of the correct type
                    if (actor.arc == arc)
                    {
                        //ignore active status condition
                        if (isActiveIgnore == true)
                        { return true; }
                        else
                        {
                            //only true if actor is active (default condition)
                            if (actor.Status == ActorStatus.Active)
                            { return true; }
                        }
                    }
                }
            }
            return false;
        }
        Debug.LogError("Invalid arc (Null)");
        return false;
    }

    /// <summary>
    /// returns true if actorSlotID is currently filled by an actor, false if the position is vacant. 
    /// Call prior to any Check/Get actorslotID methods to check if an actor is there in the first place as otherwise you'll end up with a Null actor error
    /// </summary>
    /// <param name="slotID"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    public bool CheckActorSlotStatus(int slotID, GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid slotID input");
        return arrayOfActorsPresent[side.level, slotID];
    }


    /// <summary>
    /// finds the first empty actor slot and returns actorSlotID, '-1' if none found
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public int CheckForSpareActorSlot(GlobalSide side)
    {
        int slotID = -1;
        for (int i = 0; i < GameManager.instance.actorScript.maxNumOfOnMapActors; i++)
        {
            if (arrayOfActorsPresent[side.level, i] == false)
            {
                slotID = i;
                break;
            }
        }
        return slotID;
    }



    /// <summary>
    /// called from ImportManager.cs
    /// </summary>
    public void InitialiseArrayOfActors()
    { arrayOfActors = new Actor[GetNumOfGlobalSide(), GameManager.instance.actorScript.maxNumOfOnMapActors]; }

    /// <summary>
    /// called from ImportManager.cs
    /// </summary>
    public void InitialiseArrayOfActorsPresent()
    { arrayOfActorsPresent = new bool[GetNumOfGlobalSide(), GameManager.instance.actorScript.maxNumOfOnMapActors]; }

    public Actor[,] GetArrayOfActors()
    { return arrayOfActors; }

    public bool[,] GetArrayOfActorsPresent()
    { return arrayOfActorsPresent; }

    /// <summary>
    /// returns the number of Active OnMap actors for a side
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfActiveActors(GlobalSide side)
    {
        int numOfActors = 0;
        for (int i = 0; i < GameManager.instance.actorScript.maxNumOfOnMapActors; i++)
        {
            if (arrayOfActorsPresent[side.level, i] == true)
            {
                Actor actor = arrayOfActors[side.level, i];
                if (actor != null)
                {
                    if (actor.Status == ActorStatus.Active)
                    { numOfActors++; }
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for {0}, slotID {1}", side.name, i); }
            }
        }
        return numOfActors;
    }

    /// <summary>
    /// returns the number of OnMap actors (status irrelevant) for a side. Returns Zero if none.
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public int  CheckNumOfOnMapActors(GlobalSide side)
    {
        int numOfActors = 0;
        for (int i = 0; i < GameManager.instance.actorScript.maxNumOfOnMapActors; i++)
        {
            if (arrayOfActorsPresent[side.level, i] == true)
            { numOfActors++; }
        }
        return numOfActors;
    }



    /// <summary>
    /// debug method to show contents of both sides reserve lists
    /// </summary>
    /// <returns></returns>
    public string DisplayActorLists()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(string.Format(" Actor Lists{0}{1}", "\n", "\n"));
        //authority
        builder.Append(string.Format(" - Authority Reserve List{0}", "\n"));
        builder.Append(GetActorList(authorityActorReserve));
        builder.Append(string.Format("{0} - Authority Promoted List{1}", "\n", "\n"));
        builder.Append(GetActorList(authorityActorPromoted));
        builder.Append(string.Format("{0} - Authority Dismissed List{1}", "\n", "\n"));
        builder.Append(GetActorList(authorityActorDismissed));
        builder.Append(string.Format("{0} - Authority DisposedOf List{1}", "\n", "\n"));
        builder.Append(GetActorList(authorityActorDisposedOf));
        builder.Append(string.Format("{0} - Authority Resigned List{1}", "\n", "\n"));
        builder.Append(GetActorList(authorityActorResigned));
        //resistance
        builder.Append(string.Format("{0} - Resistance Reserve List{1}", "\n", "\n"));
        builder.Append(GetActorList(resistanceActorReserve));
        builder.Append(string.Format("{0} - Resistance Promoted List{1}", "\n", "\n"));
        builder.Append(GetActorList(resistanceActorPromoted));
        builder.Append(string.Format("{0} - Resistance Dismissed List{1}", "\n", "\n"));
        builder.Append(GetActorList(resistanceActorDismissed));
        builder.Append(string.Format("{0} - Resistance DisposedOf List{1}", "\n", "\n"));
        builder.Append(GetActorList(resistanceActorDisposedOf));
        builder.Append(string.Format("{0} - Resistance Resigned List{1}", "\n", "\n"));
        builder.Append(GetActorList(resistanceActorResigned));
        return builder.ToString();
    }

    /// <summary>
    /// sub method for DisplayActorLists (returns list of actors in specified list)
    /// </summary>
    /// <param name="listOfActors"></param>
    /// <returns></returns>
    private string GetActorList(List<int> listOfActors)
    {
        StringBuilder builder = new StringBuilder();
        if (listOfActors != null)
        {
            for (int i = 0; i < listOfActors.Count; i++)
            {
                Actor actor = GetActor(listOfActors[i]);
                if (actor != null)
                {
                    builder.Append(string.Format(" {0}, ", actor.actorName));
                    builder.Append(string.Format(" ID {0}, {1}, L{2}, {3}-{4}-{5} U {6} {7}", actor.actorID, actor.arc.name, actor.level, actor.datapoint0, actor.datapoint1, actor.datapoint2, actor.unhappyTimer, "\n"));
                }
                else { builder.Append(string.Format("Error for actorID {0}", listOfActors[i])); }
            }
        }
        else { Debug.LogError("Invalid listOfActors (Null)"); }
        return builder.ToString();
    }

    //
    // - - - Actor Nodes & Qualities - - -
    //

    /// <summary>
    /// returns and array of strings for actor quality tags, eg. "Connections, Invisibility" etc.
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public string[] GetQualities(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        int numOfQualities = GameManager.instance.actorScript.numOfQualities;
        string[] tempArray = new string[numOfQualities];
        for (int i = 0; i < numOfQualities; i++)
        {
            tempArray[i] = arrayOfStatTags[side.level, i];
        }
        return tempArray;
    }

    /// <summary>
    /// returns a single string quality tag, eg. "Invisibility". Corresponds to side and qualityNumber, eg. Datapoint0 = 0, Datapoint1 = 1, Datapoint2 = 2
    /// </summary>
    /// <param name="side"></param>
    /// <param name="qualityNum"></param>
    /// <returns></returns>
    public string GetQuality(GlobalSide side, int qualityNum)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        Debug.Assert(qualityNum > -1 && qualityNum < GameManager.instance.actorScript.numOfQualities, "Invalid qualityNum");
        return arrayOfStatTags[side.level, qualityNum];
    }

    /// <summary>
    /// set up resistance array at game start
    /// </summary>
    /// <param name="listOfQualities"></param>
    public void InitialiseResistanceQualities(IEnumerable<Quality> listOfQualities)
    {
        if (listOfQualities != null)
        { resistanceQualities = listOfQualities.ToArray(); }
        else { Debug.LogError("Invalid listOfQualities for Resistance (Null)"); }
    }

    /// <summary>
    /// set up authority array at game start
    /// </summary>
    /// <param name="listOfQualities"></param>
    public void InitialiseAuthorityQualities(IEnumerable<Quality> listOfQualities)
    {
        if (listOfQualities != null)
        { authorityQualities = listOfQualities.ToArray(); }
        else { Debug.LogError("Invalid listOfQualities for Resistance (Null)"); }
    }

    public Quality[] GetArrayOfAuthorityQualities()
    { return authorityQualities; }

    public Quality[] GetArrayOfResistanceQualities()
    { return resistanceQualities; }

    /// <summary>
    /// method called from ImportManager.cs
    /// </summary>
    public void InitialiseArrayOfStatTags()
    {
        int numOfQualities = GameManager.instance.actorScript.numOfQualities;
        int numOfGlobalSides = GameManager.instance.dataScript.GetNumOfGlobalSide();
        arrayOfStatTags = new string[numOfGlobalSides, numOfQualities];
    }

    public string[,] GetArrayOfStatTags()
    { return arrayOfStatTags; }

    //
    // - - - Secrets - - -
    //

    public Dictionary<int, Secret> GetDictOfSecrets()
    { return dictOfSecrets; }

    public Dictionary<string, SecretType> GetDictOfSecretTypes()
    { return dictOfSecretTypes; }

    public Dictionary<string, SecretStatus> GetDictOfSecretStatus()
    { return dictOfSecretStatus; }

    /// <summary>
    /// returns player secrets (only those relevant to player side are loaded at game start)
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public List<Secret> GetListOfPlayerSecrets()
    { return listOfPlayerSecrets; }

    public List<Secret> GetListOfRevealedSecrets()
    { return listOfRevealedSecrets; }

    public List<Secret> GetListOfDeletedSecrets()
    { return listOfDeletedSecrets; }

    /// <summary>
    /// returns a secret, null if not fond
    /// </summary>
    /// <param name="secretID"></param>
    /// <returns></returns>
    public Secret GetSecret(int secretID)
    {
        if (dictOfSecrets.ContainsKey(secretID))
        { return dictOfSecrets[secretID]; }
        else { Debug.LogWarningFormat("Not found secretID {0}, in dictOfSecrets", secretID); }
        return null;
    }

    /// <summary>
    /// add a secret to list of Revealed Secrets (checks that has been revealed)
    /// </summary>
    /// <param name="secret"></param>
    public void AddRevealedSecret(Secret secret)
    {
        if (secret != null)
        {
            if (secret.status.level == 2)
            { listOfRevealedSecrets.Add(secret); }
            else { Debug.LogWarningFormat("Secret \"{0}\", ID {1}, has revealedWhen {2}", secret.tag, secret.secretID, secret.revealedWhen); }
        }
        else { Debug.LogWarning("Invalid Secret (Null)"); }
    }

    /// <summary>
    /// add a secret to list of deleted Secrets (checks that has been deleted)
    /// </summary>
    /// <param name="secret"></param>
    public void AddDeletedSecret(Secret secret)
    {
        if (secret != null)
        {
            if (secret.status.level == 3)
            { listOfDeletedSecrets.Add(secret); }
            else { Debug.LogWarningFormat("Secret \"{0}\", ID {1}, has deletedWhen {2}", secret.tag, secret.secretID, secret.deletedWhen); }
        }
        else { Debug.LogWarning("Invalid Secret (Null)"); }
    }

    //
    // - - - Gear - - -
    //

    /// <summary>
    /// returns dictionary of Gear (all metaLevels)
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, Gear> GetDictOfGear()
    { return dictOfGear; }

    /// <summary>
    /// returns number of rarity types (used for array sizing in GearManager.cs -> Initialise)
    /// </summary>
    /// <returns></returns>
    public int GetNumOfGearRarity()
    { return listOfGearRarity.Count; }

    public List<GearRarity> GetListOfGearRarity()
    { return listOfGearRarity; }

    public List<GearType> GetListOfGearType()
    { return listOfGearType; }

    /// <summary>
    /// returns gear type with same name or null of none
    /// </summary>
    /// <param name="gearTypeName"></param>
    /// <returns></returns>
    public GearType GetGearType(string gearTypeName)
    {
        return listOfGearType.Find(x => x.name.Equals(gearTypeName));
    }

    /// <summary>
    /// returns GearRarity for the specified level, null if not found
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public GearRarity GetGearRarity(int level)
    {
        for (int i = 0; i < listOfGearRarity.Count; i++)
        {
            if (listOfGearRarity[i].level == level)
            { return listOfGearRarity[i]; }
        }
        return null;
    }

    /// <summary>
    /// returns item of Gear, Null if not found
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public Gear GetGear(int gearID)
    {
        if (dictOfGear.ContainsKey(gearID))
        { return dictOfGear[gearID]; }
        else { Debug.LogWarningFormat("Not found in gearID {0}, in dict {1}", gearID, "\n"); }
        return null;
    }


    /// <summary>
    /// Initialise lists of gear that are available in the current level
    /// </summary>
    /// <param name="listOfGearID"></param>
    /// <param name="rarity"></param>
    public void SetGearList(List<int> listOfGearID, GearRarity rarity)
    {
        if (listOfGearID != null)
        {
            if (listOfGearID.Count > 0)
            {
                switch (rarity.level)
                {
                    case 0:
                        //common
                        listOfCommonGear.AddRange(listOfGearID);
                        break;
                    case 1:
                        //rare
                        listOfRareGear.AddRange(listOfGearID);
                        break;
                    case 2:
                        //unique
                        listOfUniqueGear.AddRange(listOfGearID);
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid rarity \"{0}\", level {1}", rarity.name, rarity.level));
                        break;
                }
                Debug.Log(string.Format("DataManager -> SetGearList {0} records for GearLevel \"{1}\"{2}", listOfGearID.Count, rarity.name, "\n"));
            }
            else { Debug.LogError("Empty listOfGearID"); }
        }
        else { Debug.LogError("Invalid listOfGearID (Null)"); }
    }

    /// <summary>
    /// returns a list of gear according to rarity that is appropriate for the current rarity
    /// </summary>
    /// <param name="rarity"></param>
    /// <returns></returns>
    public List<int> GetListOfGear(GearRarity rarity)
    {
        List<int> tempList = new List<int>();
        switch (rarity.level)
        {
            case 0:
                //common
                tempList = listOfCommonGear;
                break;
            case 1:
                //rare
                tempList = listOfRareGear;
                break;
            case 2:
                //unique
                tempList = listOfUniqueGear;
                break;
            default:
                Debug.LogError(string.Format("Invalid rarity level \"{0}\"", rarity.level));
                break;
        }
        //return list
        return tempList;
    }

    public List<int> GetListOfCurrentGear()
    { return listOfCurrentGear; }

    /// <summary>
    /// will remove a piece of gear that's been lost (for any reason) from the current gear list and add to the list Of Lost Gear
    /// </summary>
    /// <param name="gear"></param>
    /// <returns></returns>
    public bool RemoveGearLost(Gear gear)
    {
        bool isSuccess = true;
        if (gear != null)
        {
            //remove from Current list
            if (listOfCurrentGear.Remove(gear.gearID) == false)
            { Debug.LogWarningFormat("Gear \"{0}\", ID {1}, not found in listOfCurrentGear", gear.name, gear.gearID); }
            //add to Lost list
            listOfLostGear.Add(gear.gearID);
        }
        else { Debug.LogWarning("Invalid gear (Null)"); }
        return isSuccess;
    }

    /// <summary>
    /// will remove a piece of gear from one of the three pools (common/rare/unique) (for any reason)
    /// </summary>
    /// <param name="gear"></param>
    /// <returns></returns>
    public bool RemoveGearFromPool(Gear gear)
    {
        bool isSuccess = true;
        if (gear != null)
        {
            switch (gear.rarity.level)
            {
                case 0:
                    //common
                    if (listOfCommonGear.Remove(gear.gearID) == false)
                    { Debug.LogWarningFormat("Gear \"{0}\", ID {1}, not found in listOfCommonGear", gear.name, gear.gearID); }
                    break;
                case 1:
                    //rare
                    if (listOfRareGear.Remove(gear.gearID) == false)
                    { Debug.LogWarningFormat("Gear \"{0}\", ID {1}, not found in listOfRareGear", gear.name, gear.gearID); }
                    break;
                case 2:
                    //unique
                    if (listOfUniqueGear.Remove(gear.gearID) == false)
                    { Debug.LogWarningFormat("Gear \"{0}\", ID {1}, not found in listOfUniqueGear", gear.name, gear.gearID); }
                    break;
                default:
                    Debug.LogWarningFormat("Invalid Gear rarity level {0} for \"{1}\"", gear.rarity.level, gear.name);
                    isSuccess = false;
                    break;
            }
        }
        else { Debug.LogWarning("Invalid gear (Null)"); }
        return isSuccess;
    }

    /// <summary>
    /// adds gear to list of current, OnMap, in use, gear
    /// </summary>
    /// <param name="gear"></param>
    /// <returns></returns>
    public void AddGearNew(Gear gear)
    {
        if (gear != null)
        {
            if (listOfCurrentGear.Exists(x => x == gear.gearID) == false)
            {
                // NOTE: No need for error warning as there will be instances of swapping gear where gear will already exist
                listOfCurrentGear.Add(gear.gearID);
            }
        }
        else { Debug.LogWarning("Invalid gear (Null)"); }
    }

    /// <summary>
    /// Deletes gear from the common and rare pools to accomodate gear that has been already used by the Rebel AI. Called by SideManager.RevertToHumanPlayer
    /// </summary>
    /// <param name="gearUsed"></param>
    public void UpdateGearLostOnRevert(int gearUsed)
    {
        int count, index, gearID;
        Gear gear;
        bool isSuccess;
        int chanceOfRareGear = GameManager.instance.gearScript.chanceOfRareGear;
        for (int i = 0; i < gearUsed; i++)
        {
            isSuccess = false;
            //chance of rare gear
            if (Random.Range(0, 100) < chanceOfRareGear)
            {
                //get random rare gear
                count = listOfRareGear.Count;
                if (count > 0)
                {

                    index = Random.Range(0, count);
                    gearID = listOfRareGear[index];
                    if (gearID > -1)
                    {
                        //delete from rare gear pool
                        listOfRareGear.RemoveAt(index);
                        //add to Lost gear pool
                        listOfLostGear.Add(gearID);
                        //message
                        gear = GetGear(gearID);
                        if (gear != null)
                        { Debug.LogFormat("[Gea] DataManager.cs -> UpdateGearLostOnRevert: {0}, {1}, {2},  id {3} Gear Lost (used by Rebel AI){4}", gear.name, gear.type.name, gear.rarity.name, gear.gearID, "\n"); }
                        else { Debug.LogErrorFormat("Invalid gear (Null) for gearID {0}", gearID); }
                        isSuccess = true;
                    }
                    else { Debug.LogWarning("Invalid gearID (Less than Zero)"); }
                }
            }
            //if not rare gear or rare gear didn't work
            if (isSuccess == false)
            {
                //common gear
                count = listOfCommonGear.Count;
                if (count > 0)
                {
                    index = Random.Range(0, count);
                    gearID = listOfCommonGear[index];
                    if (gearID > -1)
                    {
                        //delete from common gear pool
                        listOfCommonGear.RemoveAt(index);
                        //add to Lost gear pool
                        listOfLostGear.Add(gearID);
                        //message
                        gear = GetGear(gearID);
                        if (gear != null)
                        { Debug.LogFormat("[Gea] DataManager.cs -> UpdateGearLostOnRevert: {0}, {1}, {2},  id {3} Gear Lost (used by Rebel AI){4}", gear.name, gear.type.name, gear.rarity.name, gear.gearID, "\n"); }
                        else { Debug.LogErrorFormat("Invalid gear (Null) for gearID {0}", gearID); }
                        isSuccess = true;
                    }
                    else { Debug.LogWarning("Invalid gearID (Less than Zero)"); }
                }
            }
            //break out of loop if neither rare or common gear have worked
            if (isSuccess == false)
            {
                Debug.LogWarning("Breaking out of loop due to inability to delete Rare or Common gear");
                break;
            }
        }
    }

    /// <summary>
    /// Selects gear randomly from common and rare gear pools to add as current gear to reflect what the AI Rebel Leader is using when it reverts to human control
    /// </summary>
    /// <param name="gearPoints"></param>
    public void UpdateGearCurrentOnRevert(int gearPoints)
    {
        int count, index, gearID;
        Gear gear;
        bool isSuccess;
        int chanceOfRareGear = GameManager.instance.gearScript.chanceOfRareGear;
        //put an upper limit on gear points (max. amount of gear)
        gearPoints = Mathf.Min(GameManager.instance.gearScript.maxNumOfGear, gearPoints);
        for (int i = 0; i < gearPoints; i++)
        {
            isSuccess = false;
            //chance of rare gear
            if (Random.Range(0, 100) < chanceOfRareGear)
            {
                //get random rare gear
                count = listOfRareGear.Count;
                if (count > 0)
                {
                    index = Random.Range(0, count);
                    gearID = listOfRareGear[index];
                    if (gearID > -1)
                    {
                        //delete from rare gear pool
                        listOfRareGear.RemoveAt(index);
                        //add to Current gear pool
                        listOfCurrentGear.Add(gearID);
                        //add to player's inventory
                        GameManager.instance.playerScript.AddGear(gearID);
                        //message
                        gear = GetGear(gearID);
                        if (gear != null)
                        { Debug.LogFormat("[Gea] DataManager.cs -> UpdateGearCurrentOnRevert: {0}, {1}, {2},  id {3} Gear currently in use{4}", gear.name, gear.type.name, gear.rarity.name, gear.gearID, "\n"); }
                        else { Debug.LogErrorFormat("Invalid gear (Null) for gearID {0}", gearID); }
                        isSuccess = true;
                    }
                    else { Debug.LogWarning("Invalid gearID (Less than Zero)"); }
                }
            }
            //if not rare gear or rare gear didn't work
            if (isSuccess == false)
            {
                //common gear
                count = listOfCommonGear.Count;
                if (count > 0)
                {
                    index = Random.Range(0, count);
                    gearID = listOfCommonGear[index];
                    if (gearID > -1)
                    {
                        //delete from common gear pool
                        listOfCommonGear.RemoveAt(index);
                        //add to Current gear pool
                        listOfCurrentGear.Add(gearID);
                        //add to player's inventory
                        GameManager.instance.playerScript.AddGear(gearID);
                        //message
                        gear = GetGear(gearID);
                        if (gear != null)
                        { Debug.LogFormat("[Gea] DataManager.cs -> UpdateGearCurrentOnRevert: {0}, {1}, {2},  id {3} Gear currently in use{4}", gear.name, gear.type.name, gear.rarity.name, gear.gearID, "\n"); }
                        else { Debug.LogErrorFormat("Invalid gear (Null) for gearID {0}", gearID); }
                        isSuccess = true;
                    }
                    else { Debug.LogWarning("Invalid gearID (Less than Zero)"); }
                }
            }
            //break out of loop if neither rare or common gear have worked
            if (isSuccess == false)
            {
                Debug.LogWarning("Breaking out of loop due to inability to delete Rare or Common gear");
                break;
            }
        }
    }

    /// <summary>
    /// Debug display method for all gear lists
    /// </summary>
    /// <returns></returns>
    public string DisplayGearData()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat(" Gear Data {0}{1}", "\n", "\n");
        builder.AppendFormat("- Current Gear{0}", "\n");
        builder.Append(DisplayGearList(listOfCurrentGear));
        builder.AppendFormat("{0}- Lost Gear{1}", "\n", "\n");
        builder.Append(DisplayGearList(listOfLostGear));
        builder.AppendFormat("{0}- Common Gear{1}", "\n", "\n");
        builder.Append(DisplayGearList(listOfCommonGear));
        builder.AppendFormat("{0}- Rare Gear{1}", "\n", "\n");
        builder.Append(DisplayGearList(listOfRareGear));
        builder.AppendFormat("{0}- Unique Gear{1}", "\n", "\n");
        builder.Append(DisplayGearList(listOfUniqueGear));
        return builder.ToString();
    }

    /// <summary>
    /// submethod (Debug) to turn a list of gear into a readable display item, called by DataManager.cs -> DisplayGearData
    /// </summary>
    /// <param name="listOfGear"></param>
    /// <returns></returns>
    private string DisplayGearList(List<int> listOfGear)
    {
        StringBuilder builder = new StringBuilder();
        if (listOfGear != null)
        {
            int limit = listOfGear.Count;
            if (limit > 0)
            {
                for (int index = 0; index < limit; index++)
                {
                    Gear gear = GetGear(listOfGear[index]);
                    if (gear != null)
                    { builder.AppendFormat(" {0} ({1}), ID{2}, {3}, used {4} times{5}", gear.name, gear.type.name, gear.gearID, gear.rarity.name, gear.timesUsed, "\n"); }
                    else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}", listOfGear[index]); }
                }
            }
            else
            { builder.AppendFormat(" No records{0}", "\n"); }
        }
        else { Debug.LogWarning("Invalid listOfGear (Null)"); }
        return builder.ToString();
    }

    //
    // - - - Messages - - -
    //

    /// <summary>
    /// add a New message. Auto sorted to Pending dict (isPublic = true) or Archive dict (isPublic = false)
    /// </summary>
    /// <param name="message"></param>
    public void AddMessage(Message message)
    {
        if (message != null)
        {
            //Generate a Debug Message for the log
            Debug.Log(string.Format("[Msg] {0}{1}", message.text, "\n"));
            //auto sort
            switch (message.isPublic)
            {
                case true:
                    //if isPublic true then store in Pending dictionary
                    if (message.displayDelay > 0)
                    { AddMessageExisting(message, MessageCategory.Pending); }
                    else
                    { AddMessageExisting(message, MessageCategory.Current); }
                    break;
                case false:
                    //if isPublic False then store in Archive dictionary
                    AddMessageExisting(message, MessageCategory.Archive);
                    //AI message (Pending AI messages accessed in due course)
                    if (message.type == MessageType.AI)
                    { AIMessage(message); }
                    break;
            }
            //Nemesis sighting message
            if (message.type == MessageType.CONTACT)
            {
                switch (message.subType)
                {
                    case MessageSubType.Contact_Nemesis_Spotted:
                    case MessageSubType.Tracer_Nemesis_Spotted:
                        //Extract Nemesis sighting data
                        GameManager.instance.aiRebelScript.GetAIRebelMessageData(message);
                        break;
                    case MessageSubType.Contact_Team_Spotted:
                        Team team = GetTeam(message.data3);
                        if (team != null)
                        {
                            //f an erasure team then extract sighting data
                            if (team.arc.name.Equals("ERASURE") == true)
                            { GameManager.instance.aiRebelScript.GetAIRebelMessageData(message); }
                        }
                        else { Debug.LogErrorFormat("Invalid Contact team (Null) for teamID {0}", message.data3); }
                        break;
                    case MessageSubType.Tracer_Team_Spotted:
                        team = GetTeam(message.data1);
                        if (team != null)
                        {
                            //f an erasure team then extract sighting data
                            if (team.arc.name.Equals("ERASURE") == true)
                            { GameManager.instance.aiRebelScript.GetAIRebelMessageData(message); }
                        }
                        else { Debug.LogErrorFormat("Invalid Tracer team (Null) for teamID {0}", message.data3); }
                        break;
                }
            }
        }
        else { Debug.LogError("Invalid Pending Message (Null)"); }
    }

    /// <summary>
    /// subMethod for AddMessage to place copy of message in AI dictionary and to extract AI data
    /// message checked for type = MessageType.AI by the calling procedure & that message is being sent to the Archives
    /// </summary>
    /// <param name=""></param>
    public void AIMessage(Message message)
    {
        //Add a copy of the message to AI Message dictionary 
        AddMessageExisting(message, MessageCategory.AI);
        //Extract AI data
        GameManager.instance.aiScript.GetAIMessageData(message);
    }

    /// <summary>
    /// add an Existing message to a dictionary
    /// </summary>
    /// <param name="message"></param>
    /// <param name="category"></param>
    public bool AddMessageExisting(Message message, MessageCategory category)
    {
        bool successFlag = true;
        Dictionary<int, Message> dictOfMessages = null;
        //get appropriate dictionary
        switch (category)
        {
            case MessageCategory.Archive:
                dictOfMessages = dictOfArchiveMessages;
                break;
            case MessageCategory.Pending:
                dictOfMessages = dictOfPendingMessages;
                break;
            case MessageCategory.Current:
                dictOfMessages = dictOfCurrentMessages;
                break;
            case MessageCategory.AI:
                dictOfMessages = dictOfAIMessages;
                break;
            default:
                Debug.LogError(string.Format("Invalid MessageCategory \"{0}\"", category));
                successFlag = false;
                break;
        }
        if (dictOfMessages != null)
        {
            //add to dictionary
            try
            { dictOfMessages.Add(message.msgID, message); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Message (Null)"); successFlag = false; }
            catch (ArgumentException)
            {
                Debug.LogError(string.Format("Invalid Message (duplicate) msgID \"{0}\" for {1} \"{2}\"{3}", message.msgID, message.subType, message.text, "\n"));
                successFlag = false;
            }
        }
        return successFlag;
    }

    /// <summary>
    /// Remove (delete) a message from a dictionary
    /// </summary>
    /// <param name="msgID"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    private bool RemoveMessage(int msgID, MessageCategory category)
    {
        bool successFlag = true;
        Dictionary<int, Message> dictOfMessages = null;
        //get appropriate dictionary
        switch (category)
        {
            case MessageCategory.Archive:
                dictOfMessages = dictOfArchiveMessages;
                break;
            case MessageCategory.Pending:
                dictOfMessages = dictOfPendingMessages;
                break;
            case MessageCategory.Current:
                dictOfMessages = dictOfCurrentMessages;
                break;
            case MessageCategory.AI:
                dictOfMessages = dictOfAIMessages;
                break;
            default:
                Debug.LogError(string.Format("Invalid MessageCategory \"{0}\"", category));
                successFlag = false;
                break;
        }
        if (dictOfMessages != null)
        {
            //remove from dictionary
            if (dictOfMessages.ContainsKey(msgID) == true)
            { dictOfMessages.Remove(msgID); }
            else { successFlag = false; }
        }
        return successFlag;
    }

    /// <summary>
    /// Gets a message of a specified ID from the specified dictionary (category). Returns null if not found
    /// </summary>
    /// <param name="msgID"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    public Message GetMessage(int msgID, MessageCategory category)
    {
        Dictionary<int, Message> dictOfMessages = null;
        //get appropriate dictionary
        switch(category)
        {
            case MessageCategory.Archive:
                dictOfMessages = new Dictionary<int, Message>(dictOfArchiveMessages);
                break;
            case MessageCategory.Pending:
                dictOfMessages = new Dictionary<int, Message>(dictOfPendingMessages);
                break;
            case MessageCategory.Current:
                dictOfMessages = new Dictionary<int, Message>(dictOfCurrentMessages);
                break;
            case MessageCategory.AI:
                dictOfMessages = new Dictionary<int, Message>(dictOfAIMessages);
                break;
            default:
                Debug.LogError(string.Format("Invalid MessageCategory \"{0}\"", category));
                break;
        }
        if (dictOfMessages != null)
        {
            //get msg from originating dictionary
            if (dictOfMessages.ContainsKey(msgID))
            { return dictOfMessages[msgID]; }
            else { Debug.LogWarning(string.Format("Not found in msgID {0}, in {1} dict{2}", msgID, category, "\n")); }
        }
        return null;
    }

    /// <summary>
    /// returns specified dictionary of messages, returns null if an invalid categoary
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public Dictionary<int, Message> GetMessageDict(MessageCategory category)
    {
        Dictionary<int, Message> dictOfMessages = null;
        //get appropriate dictionary
        switch (category)
        {
            case MessageCategory.Archive:
                dictOfMessages = dictOfArchiveMessages;
                break;
            case MessageCategory.Pending:
                dictOfMessages = dictOfPendingMessages;
                break;
            case MessageCategory.Current:
                dictOfMessages = dictOfCurrentMessages;
                break;
            case MessageCategory.AI:
                dictOfMessages = dictOfAIMessages;
                break;
            default:
                Debug.LogError(string.Format("Invalid MessageCategory \"{0}\"", category));
                break;
        }
        return dictOfMessages;
    }

    /// <summary>
    /// Moves a message from one category (dict) to another while removing it from the original category. Handles all admin. Returns true if successful
    /// </summary>
    /// <param name="msgID"></param>
    /// <param name="fromCategory"></param>
    /// <param name="toCategory"></param>
    public bool MoveMessage(int msgID, MessageCategory fromCategory, MessageCategory toCategory)
    {
        bool successFlag = true;
        //get message
        Message message = GetMessage(msgID, fromCategory);
        if (message != null)
        {
            //add message to new dictionary
            if (AddMessageExisting(message, toCategory) == true)
            {
                //remove message form original dictionary
                if (RemoveMessage(message.msgID, fromCategory) == false)
                { Debug.LogWarning(string.Format("Delete message ID {0}, \"{1}\" to {2} has failed", message.msgID, message.text, fromCategory)); successFlag = false; }
            }
            else { Debug.LogWarning(string.Format("Move message ID {0}, \"{1}\" to {2} has failed", message.msgID, message.text, toCategory)); successFlag = false; }
        }
        else { Debug.LogError(string.Format("Invalid message (Null) for msgID {0}, category \"{1}\"", msgID, fromCategory)); successFlag = false; }
        return successFlag;
    }

    /// <summary>
    /// debug method to display messages. Returns display string or empty if category is invalid.
    /// </summary>
    /// <returns></returns>
    public string DisplayMessages(MessageCategory category)
    {
        //which dictionary to use
        Dictionary<int, Message> tempDict = null;
        //stringbuilders (creating two separate lists, one for each side
        StringBuilder builderAuthority = new StringBuilder();
        StringBuilder builderResistance = new StringBuilder();
        StringBuilder builderOverall = new StringBuilder();
        //get the required dictionary
        switch (category)
        {
            case MessageCategory.Archive:
                tempDict = new Dictionary<int, Message>(dictOfArchiveMessages);
                builderOverall.Append(string.Format(" ARCHIVE Messages{0}{1}", "\n", "\n"));
                break;
            case MessageCategory.Pending:
                tempDict = new Dictionary<int, Message>(dictOfPendingMessages);
                builderOverall.Append(string.Format(" PENDING Messages{0}{1}", "\n", "\n"));
                break;
            case MessageCategory.Current:
                tempDict = new Dictionary<int, Message>(dictOfCurrentMessages);
                builderOverall.Append(string.Format(" CURRENT Messages{0}{1}", "\n", "\n"));
                break;
            case MessageCategory.AI:
                tempDict = new Dictionary<int, Message>(dictOfAIMessages);
                builderOverall.Append(string.Format(" AI Messages{0}{1}", "\n", "\n"));
                break;
            default:
                builderOverall.Append(string.Format(" UNKNOWN Messages{0}{1}", "\n", "\n"));
                Debug.LogError(string.Format("Invalid MessageCategory \"{0}\"", category));
                break;
        }
        if (tempDict != null)
        {
            builderResistance.Append(string.Format(" Messages -> Resistance{0}", "\n"));
            builderAuthority.Append(string.Format("{0} Messages -> Authority{1}", "\n", "\n"));
            //max # of records per side
            int numOfRecordsPerSide = 15;       //actual messages per side is double this (don't go above 15)
            int counterResistance = 0;
            int counterAuthority = 0;
            int limitResistance = numOfRecordsPerSide;
            int limitAuthority = numOfRecordsPerSide;
            bool isSingleLine = false;
            int numRecords = tempDict.Count;
            //if more than half the display, switch to single line display instead of double lines per record
            if (numRecords > numOfRecordsPerSide) { isSingleLine = true; limitResistance *= 2; limitAuthority *= 2; }
            //sort records in descending turn order
            List<Message> listOfMessages = new List<Message>();
            IEnumerable<Message> allMessages =
                from rec in tempDict
                orderby rec.Value.turnCreated descending
                select rec.Value;
            listOfMessages = allMessages.ToList<Message>();
            foreach (Message msg in listOfMessages)
            {
                if (msg.side != null)
                {
                    switch (msg.side.name)
                    {
                        case "Resistance":
                            if (counterResistance < limitResistance)
                            {
                                builderResistance.Append(string.Format(" t{0}: {1}{2}", msg.turnCreated, msg.text, "\n"));
                                if (!isSingleLine)
                                {
                                    builderResistance.Append(string.Format(" -> id {0}, type: {1} subType: {2}, data: {3} | {4} | {5}  {6} {7}{8}", msg.msgID, msg.type,
                                        msg.subType, msg.data0, msg.data1, msg.data2, msg.isPublic == true ? "del" : "",
                                        msg.isPublic == true ? msg.displayDelay.ToString() : "", "\n"));
                                }
                                counterResistance++;
                            }
                            break;
                        case "Authority":
                            if (counterAuthority < limitAuthority)
                            {
                                builderAuthority.Append(string.Format(" t{0}: {1}{2}", msg.turnCreated, msg.text, "\n"));
                                if (!isSingleLine)
                                {
                                    builderAuthority.Append(string.Format(" -> id {0}, type: {1} subType: {2}, data: {3} | {4} | {5}  {6} {7}{8}", msg.msgID, msg.type,
                                        msg.subType, msg.data0, msg.data1, msg.data2, msg.isPublic == true ? "del" : "",
                                        msg.isPublic == true ? msg.displayDelay.ToString() : "", "\n"));
                                }
                                counterAuthority++;
                            }
                            break;
                        case "Both":
                            //Resistance side
                            if (counterResistance < limitResistance)
                            {
                                builderResistance.Append(string.Format(" t{0}: {1}{2}", msg.turnCreated, msg.text, "\n"));
                                if (!isSingleLine)
                                {
                                    builderResistance.Append(string.Format(" -> id {0}, type: {1} subType: {2}, data: {3} | {4} | {5}  {6} {7}{8}", msg.msgID, msg.type,
                                        msg.subType, msg.data0, msg.data1, msg.data2, msg.isPublic == true ? "del" : "",
                                        msg.isPublic == true ? msg.displayDelay.ToString() : "", "\n"));
                                }
                                counterResistance++;
                            }
                            //Authority side
                            if (counterAuthority < limitAuthority)
                            {
                                builderAuthority.Append(string.Format(" t{0}: {1}{2}", msg.turnCreated, msg.text, "\n"));
                                if (!isSingleLine)
                                {
                                    builderAuthority.Append(string.Format(" -> id {0}, type: {1} subType: {2}, data: {3} | {4} | {5}  {6} {7}{8}", msg.msgID, msg.type,
                                        msg.subType, msg.data0, msg.data1, msg.data2, msg.isPublic == true ? "del" : "",
                                        msg.isPublic == true ? msg.displayDelay.ToString() : "", "\n"));
                                }
                                counterAuthority++;
                            }
                            break;
                        default:
                            builderAuthority.Append(string.Format("UNKNOWN side {0}, id {1}{2}", msg.side.name, msg.msgID, "\n"));
                            break;
                    }
                }
                else { Debug.LogError(string.Format("Invalid record.Value.side (Null), \"{0}\"", msg.text)); }
            }
        }
        //combine two lists
        builderOverall.Append(builderResistance.ToString());
        builderOverall.Append(builderAuthority.ToString());
        return builderOverall.ToString();
    }

    //
    // - - - Ongoing Effects - - - 
    //

    public Dictionary<int, EffectDataOngoing> GetDictOfOngoingEffects()
    { return dictOfOngoingID; }

    /// <summary>
    /// Add an ongoingID to the register (dict). No programming necessity for this other than tracking and debugging
    /// </summary>
    /// <param name="ongoingID"></param>
    /// <param name="details"></param>
    public void AddOngoingEffectToDict(EffectDataOngoing ongoing, int nodeID = -1)
    {
        if (ongoing != null)
        {
            //add new ongoing effect only if no other instance of it exists, ignore otherwise
            if (dictOfOngoingID.ContainsKey(ongoing.ongoingID) == false)
            {
                /*string text = string.Format("id {0}, {1}", ongoing.ongoingID, ongoing.text);*/
                //add to dictionary
                try
                {
                    dictOfOngoingID.Add(ongoing.ongoingID, ongoing);
                    //generate message (node effect only)
                    if (nodeID > -1)
                    { GameManager.instance.messageScript.OngoingEffectCreated(ongoing.text, nodeID); }
                }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid ongoingID (duplicate) \"{0}\" for \"{1}\"", ongoing.ongoingID, ongoing.text)); }
            }
        }
        else { Debug.LogError("Invalid Ongoing effect (Null)"); }
    }

    /// <summary>
    /// Debug method to display register
    /// </summary>
    /// <returns></returns>
    public string DisplayOngoingRegister()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(string.Format(" OngoingID Register{0}", "\n"));
        foreach(var ongoing in dictOfOngoingID)
        {
            if (ongoing.Value.node != null)
            { builder.Append(string.Format("{0} NodeID {1}, {2}, {3} turn{4} remaining", "\n", ongoing.Value.node.nodeID, ongoing.Value.description, ongoing.Value.timer, ongoing.Value.timer != 1 ? "s" : "")); }
            else
            { builder.Append(string.Format("{0} {1}, {2}, {3} turn{4} remaining", "\n", ongoing.Value.gearName, ongoing.Value.description, ongoing.Value.timer, ongoing.Value.timer != 1 ? "s" : "")); }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Remove an effect from the dictionary and, if present, generate a message for the relevant side. dataID could be NodeID or ConnID for connections
    /// </summary>
    /// <param name="ongoing"></param>
    public void RemoveOngoingEffect(EffectDataOngoing ongoing, int dataID)
    {
        if (ongoing != null)
        {
            //if entry has already been deleted, eg. for an ongoing 'NodeAll' effect then ignore. Message is generated for the first instance only.
            if (dictOfOngoingID.ContainsKey(ongoing.ongoingID))
            {
                //remove entry
                dictOfOngoingID.Remove(ongoing.ongoingID);
                //generate message
                string text = string.Format("id {0}, {1}", ongoing.ongoingID, ongoing.text);
                GameManager.instance.messageScript.OngoingEffectExpired(text, dataID);
            }
        }
        else { Debug.LogError("Invalid EffectDataOngoing (Null)"); }
    }


    /// <summary>
    /// Debug method to remove all connection security effects for all entries in the register
    /// </summary>
    public void RemoveOngoingEffects()
    {
        if (dictOfOngoingID.Count > 0)
        {
            foreach(var register in dictOfOngoingID)
            {
                GameManager.instance.connScript.RemoveOngoingEffect(register.Key);
                GameManager.instance.nodeScript.RemoveOngoingEffect(register.Key);
            }
        }
    }

    public Dictionary<int, Effect> GetDictOfEffects()
    { return dictOfEffects; }

    //
    // - - - AI - - -
    //

    /// <summary>
    /// Add an AITracker entry to queue
    /// </summary>
    /// <param name="tracker"></param>
    public void AddToRecentNodeQueue(AITracker tracker)
    {
        if (tracker != null)
        {
            queueRecentNodes.Enqueue(tracker);
            //keep queue within defined size
            if (queueRecentNodes.Count > GameManager.instance.aiScript.numOfActivitiesTracked)
            { queueRecentNodes.Dequeue(); }
        }
        else { Debug.LogWarning("Invalid tracker (Null)"); }
    }

    /// <summary>
    /// Add an AITracker entry to queue
    /// </summary>
    /// <param name="tracker"></param>
    public void AddToRecentConnectionQueue(AITracker tracker)
    {
        if (tracker != null)
        {
            queueRecentConnections.Enqueue(tracker);
            //keep queue within defined size
            if (queueRecentConnections.Count > GameManager.instance.aiScript.numOfActivitiesTracked)
            { queueRecentConnections.Dequeue(); }
        }
        else { Debug.LogWarning("Invalid tracker (Null)"); }
    }


    public Queue<AITracker> GetRecentNodesQueue()
    { return queueRecentNodes; }

    public Queue<AITracker> GetRecentConnectionsQueue()
    { return queueRecentConnections; }

    /// <summary>
    /// returns the current amount of resources in the relevant AI pool
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public int CheckAIResourcePool(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        return arrayOfAIResources[side.level];
    }

    /// <summary>
    /// sets the relevant resource pool to a specified amount
    /// </summary>
    /// <param name="side"></param>
    /// <param name="amount"></param>
    public void SetAIResources(GlobalSide side, int amount)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        arrayOfAIResources[side.level] = amount;
    }


    public Dictionary<int, DecisionAI> GetDictOfAIDecisions()
    { return dictOfAIDecisions; }

    public Dictionary<string, int> GetDictOfLookUpAIDecisions()
    { return dictOfLookUpAIDecisions; }

    /// <summary>
    /// returns DecisionAI.aiDecID from an input decision name (name of SO), -1 if not found
    /// </summary>
    /// <param name="decisionName"></param>
    /// <returns></returns>
    public int GetAIDecisionID(string decisionName)
    {
        int aiDecID = -1;
        if (string.IsNullOrEmpty(decisionName) == false)
        {
            if (dictOfLookUpAIDecisions.ContainsKey(decisionName))
            {  return dictOfLookUpAIDecisions[decisionName]; }
            else { Debug.LogWarning(string.Format("DecisionAI \"{0}\" not found in dictOfLookUpAIDecisions{1}", decisionName, "\n")); }
        }
        else { Debug.LogError("Invalid decisionName (Null or Empty)"); }
        return aiDecID;
    }

    /// <summary>
    /// returns DecisionAI from dictOfAIDecisions based on aiDecID. Null if not found.
    /// </summary>
    /// <param name="aiDecID"></param>
    /// <returns></returns>
    public DecisionAI GetAIDecision(int aiDecID)
    {
        DecisionAI decisionAI = null;
        if (dictOfAIDecisions.TryGetValue(aiDecID, out decisionAI))
        { return decisionAI; }
        return null;
    }

    //
    // - - - Manage - - -
    //

    /// <summary>
    /// Manage Action list -> Handle (first level)
    /// </summary>
    /// <returns></returns>
    public List<ManageAction> GetListOfActorHandle()
    { return listOfActorHandle; }

    /// <summary>
    /// Manage Action list -> Reserve (second level)
    /// </summary>
    /// <returns></returns>
    public List<ManageAction> GetListOfActorReserve()
    { return listOfActorReserve; }

    /// <summary>
    /// Manage Action list -> Dismiss (second level)
    /// </summary>
    /// <returns></returns>
    public List<ManageAction> GetListOfActorDismiss()
    { return listOfActorDismiss; }

    /// <summary>
    /// Manage Action list -> Dispose (second level)
    /// </summary>
    /// <returns></returns>
    public List<ManageAction> GetListOfActorDispose()
    { return listOfActorDispose; }

    /// <summary>
    /// initialise Actor Handles
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfActorHandle(List<ManageAction> tempList)
    {
        if (tempList != null)
        {
            listOfActorHandle.Clear();
            listOfActorHandle.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    /// <summary>
    /// initialise Actor Reserve
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfActorReserve(List<ManageAction> tempList)
    {
        if (tempList != null)
        {
            listOfActorReserve.Clear();
            listOfActorReserve.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    /// <summary>
    /// initialise Actor Dismiss
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfActorDismiss(List<ManageAction> tempList)
    {
        if (tempList != null)
        {
            listOfActorDismiss.Clear();
            listOfActorDismiss.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    /// <summary>
    /// initialise Actor Dispose
    /// </summary>
    /// <param name="tempList"></param>
    public void SetListOfActorDispose(List<ManageAction> tempList)
    {
        if (tempList != null)
        {
            listOfActorDispose.Clear();
            listOfActorDispose.AddRange(tempList);
        }
        else { Debug.LogError("Invalid tempList (Null)"); }
    }

    //
    // - - - Global SO's - - -
    //

    /*public Dictionary<string, GlobalMeta> GetDictOfGlobalMeta()
    { return dictOfGlobalMeta; }

    public Dictionary<string, GlobalChance> GetDictOfGlobalChance()
    { return dictOfGlobalChance; }

    public Dictionary<string, GlobalType> GetDictOfGlobalType()
    { return dictOfGlobalType; }

    public Dictionary<string, GlobalWho> GetDictOfGlobalWho()
    { return dictOfGlobalWho; }

    public Dictionary<string, GlobalSide> GetDictOfGlobalSide()
    { return dictOfGlobalSide; }*/

    public int GetNumOfGlobalSide()
    { return GameManager.instance.loadScript.arrayOfGlobalSide.Length; }

    /// <summary>
    /// Returns condition SO, Null if not found in dictionary
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public Condition GetCondition(string conditionName)
    {
        Condition condition = null;
        if (string.IsNullOrEmpty(conditionName) == false)
        {
            if (dictOfConditions.ContainsKey(conditionName))
            { return dictOfConditions[conditionName]; }
            else { Debug.LogWarning(string.Format("Condition \"{0}\" not found in dictOfConditions{1}", conditionName, "\n")); }
        }
        else { Debug.LogError("Invalid conditionName (Null or Empty)"); }
        return condition;
    }

    public Dictionary<string, Condition> GetDictOfConditions()
    { return dictOfConditions; }

    /// <summary>
    /// returns a dictionary (by value) of all conditions of the specified type, returns empty dictionary if none
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Dictionary<string, Condition> GetDictOfConditionsByType(GlobalType type)
    {
        Dictionary<string, Condition> dictOfConditionsByType = new Dictionary<string, Condition>();
        if (dictOfConditions != null)
        {
            foreach (var condition in dictOfConditions)
            {
                if (condition.Value.type.name.Equals(type.name) == true)
                { dictOfConditionsByType.Add(condition.Key, condition.Value); }
            }
        }
        else { Debug.LogError("Invalid dictOfConditions (Null)"); }
        return dictOfConditionsByType;
    }


    public Dictionary<string, Cure> GetDictOfCures()
    { return dictOfCures; }

    /// <summary>
    /// Returns ManageAction SO, Null if not found in dictionary
    /// </summary>
    /// <param name="actionName"></param>
    /// <returns></returns>
    public ManageAction GetManageAction(string actionName)
    {
            ManageAction manageAction = null;
            if (string.IsNullOrEmpty(actionName) == false)
            {
                if (dictOfManageActions.ContainsKey(actionName))
                { return dictOfManageActions[actionName]; }
                else { Debug.LogWarning(string.Format("ManageAction \"{0}\" not found in dictOfManageActions{1}", actionName, "\n")); }
            }
            else { Debug.LogError("Invalid actionName (Null or Empty)"); }
            return manageAction;
    }

    public Dictionary<string, ManageAction> GetDictOfManageActions()
    { return dictOfManageActions; }


    //
    // - - - Factions - - - 
    //

    /// <summary>
    /// returns a random faction for a side, null if a problem
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public Faction GetRandomFaction(GlobalSide sideRequired)
    {
        Faction factionReturn = null;
        List<Faction> listOfFactions = new List<Faction>();
        switch (sideRequired.name)
        {
            case "Authority":
            case "Resistance":
                IEnumerable<Faction> factions =
                    from faction in dictOfFactions
                    where faction.Value.side.name.Equals(sideRequired.name) == true
                    select faction.Value;
                listOfFactions = factions.ToList();
                factionReturn = listOfFactions[Random.Range(0, listOfFactions.Count)];
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", sideRequired));
                break;
        }
        return factionReturn;
    }

    public Dictionary<int, Faction> GetDictOfFactions()
    { return dictOfFactions; }

    //
    // - - - Cities - - -
    //

    /// <summary>
    /// returns a random City, null if a problem
    /// </summary>
    public City GetRandomCity()
    {
        City city = null;
        List<City> tempList = dictOfCities.Values.ToList();
        List<City> useList = new List<City>();
        //narrow list down to those with 'isTestOff' = false (default condition) -> Debug testing only, not for release
        foreach(City cityTemp in tempList)
        {
            if (cityTemp != null)
            { if (cityTemp.isTestOff == false) { useList.Add(cityTemp); } }
            else { Debug.LogWarning("Invalid city (Null) in tempList"); }
        }
        //select random city
        if (useList.Count > 0)
        { city = useList[Random.Range(0, useList.Count)]; }
        else { Debug.LogError("No records in useList (active Cities)"); }
        return city;
    }

    public Dictionary<int, City> GetDictOfCities()
    { return dictOfCities; }

    public Dictionary<int, CityArc> GetDictOfCityArcs()
    { return dictOfCityArcs; }

    //
    // - - - Objectives - - -
    //

    /// <summary>
    /// returns a list of random Objectives, empty if a problem
    /// </summary>
    public List<Objective> GetRandomObjectives(int num)
    {
        Debug.Assert(num > 0 && num < dictOfObjectives.Count, string.Format("Invalid number of objectives \"{0}\"", num));
        int index;
        List<Objective> listOfRandom = new List<Objective>();
        List<Objective> listOfObjectives = new List<Objective>(dictOfObjectives.Values.ToList());
        for(int i = 0; i < num; i++)
        {
            index = Random.Range(0, listOfObjectives.Count);
            Objective objective = listOfObjectives[index];
            if (objective != null)
            { listOfRandom.Add(objective); }
            else { Debug.LogWarning("Invalid Objective in listOfObjectives (Null)"); }
            //remove objective from list to prevent being selected again
            listOfObjectives.RemoveAt(index);
        }
        return listOfRandom;
    }

    public Dictionary<int, Objective> GetDictOfObjectives()
    { return dictOfObjectives; }

    //
    // - - - Organisations - - -
    //

    public Dictionary<int, Organisation> GetDictOfOrganisations()
    { return dictOfOrganisations; }

    //
    // - - - Mayors - - -
    //

    public Dictionary<int, Mayor> GetDictOfMayors()
    { return dictOfMayors; }

    /// <summary>
    /// Gets a random Mayor, returns null if none. NOTE: Debug method, Duplicates ARE NOT CHECKED FOR. Selects only for those Mayors where 'isTestOff' is false
    /// </summary>
    /// <returns></returns>
    public Mayor GetRandomMayor()
    {
        Mayor mayor = null;
        List<Mayor> useList = new List<Mayor>();
        List<Mayor> tempList = dictOfMayors.Values.ToList();
        //narrow list down to those with isTestOff = false (default condition) -> DEBUG testing only, not for release
        foreach (Mayor mayorTemp in tempList)
        {
            if (mayorTemp != null)
            { if (mayorTemp.isTestOff == false) { useList.Add(mayorTemp); } }
            else { Debug.LogWarning("Invalid mayor in tempList (Null)"); }
        }
        //select random mayor
        if (useList != null && useList.Count > 0)
        { mayor = useList[Random.Range(0, useList.Count)]; }
        return mayor;
    }

    //
    // - - - Adjustments - - -
    //

    /// <summary>
    /// add action adjustment to the list
    /// </summary>
    /// <param name="adjustment"></param>
    public void AddActionAdjustment(ActionAdjustment adjustment)
    {
        if (adjustment != null)
        {
            //set start to following turn
            adjustment.turnStart = GameManager.instance.turnScript.Turn + 1;
            listOfActionAdjustments.Add(adjustment);
        }
        else
        { Debug.LogError("Invalid ActionAdjustment (Null)"); }
    }


    public List<ActionAdjustment> GetListOfActionAdjustments()
    { return listOfActionAdjustments; }

    /// <summary>
    /// run at end of each turn (TurnManager.cs) to decrement timers and delete any adjustments that have timed out)
    /// </summary>
    public void UpdateActionAdjustments()
    {
        //loop backwards to enable deletion of timed out adjustments
        if (listOfActionAdjustments.Count > 0)
        {
            for (int i = listOfActionAdjustments.Count - 1; i >= 0; i--)
            {
                ActionAdjustment actionAdjustment = listOfActionAdjustments[i];
                if (actionAdjustment != null)
                {
                    actionAdjustment.timer--;
                    if (actionAdjustment.timer <= 0)
                    {
                        //Ongoing effect
                        if (actionAdjustment.ongoing != null)
                        { RemoveOngoingEffect(actionAdjustment.ongoing, actionAdjustment.ongoing.gearID); }
                        //delete adjustment
                        listOfActionAdjustments.RemoveAt(i);
                    }
                    else
                    {
                        //update ongoing timer, if present
                        if (actionAdjustment.ongoing != null)
                        { actionAdjustment.ongoing.timer = actionAdjustment.timer; }
                    }
                }
            }
        }
    }

    /// <summary>
    /// returns net action adjustment by talling all adjustments for the specified side
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public int GetActionAdjustment(GlobalSide side)
    {
        int netAdjustment = 0;
        if (listOfActionAdjustments.Count > 0)
        {
            foreach(ActionAdjustment actionAdjustment in listOfActionAdjustments)
            {
                if (actionAdjustment.side.name.Equals(side.name) == true)
                { netAdjustment += actionAdjustment.value; }
            }
        }
        return netAdjustment;
    }


    /// <summary>
    /// Debug function to display Action Adjustments
    /// </summary>
    /// <returns></returns>
    public string DisplayActionsRegister()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(string.Format(" Action Adjustments Register{0}", "\n"));
        foreach (ActionAdjustment actionAdjustment in listOfActionAdjustments)
        { builder.Append(string.Format("{0}\"{1}\", Side: {2} Adjust: {3} Timer: {4}", "\n", actionAdjustment.descriptor, 
            actionAdjustment.side.name, actionAdjustment.value, actionAdjustment.timer)); }
        return builder.ToString();
    }

    //
    // - - - Actor Panel UI - - -
    //

    public List<TextMeshProUGUI> GetListOfActorTypes()
    { return listOfActorTypes; }

    public List<Image> GetListOfActorPortraits()
    { return listOfActorPortraits; }

    //
    // - - - Missions - - -
    //

    public Dictionary<int, Mission> GetDictOfMissions()
    { return dictOfMissions; }

    //
    // - - - Help  - - -
    //

    public Dictionary<string, HelpData> GetDictOfHelpData()
    { return dictOfHelpData; }

    /// <summary>
    /// Get help data for a specific tag, returns Null if not found
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public HelpData GetHelpData(string tag)
    {
        if (dictOfHelpData.ContainsKey(tag))
        { return dictOfHelpData[tag]; }
        else { Debug.LogWarningFormat("Not found, tag \"{0}\", in dictOfHelpData {1}", tag, "\n"); }
        return null;
    }

    //
    // - - - Tracker Data - - -
    //

    /// <summary>
    /// Resistance AI or Player moves
    /// </summary>
    /// <param name="data"></param>
    public void AddHistoryRebelMove(HistoryRebelMove data)
    {
        if (data != null)
        { listOfHistoryRebelMove.Add(data); }
        else { Debug.LogError("Invalid Resistance Tracker data (Null)"); }
    }

    /// <summary>
    /// Nemesis AI or Player controlled moves
    /// </summary>
    /// <param name="data"></param>
    public void AddHistoryNemesisMove(HistoryNemesisMove data)
    {
        if (data != null)
        { listOfHistoryNemesisMove.Add(data); }
        else { Debug.LogError("Invalid Nemesis Tracker data (Null)"); }
    }

    /// <summary>
    /// Add a text to the listOfHistoryAutoRun. Text should be a formatted, self-contained sentence (short) summarising the event. Ignore turn # as it's added automatically. Ignore Bold as auto bold everything.
    /// </summary>
    /// <param name="text"></param>
    public void AddHistoryAutoRun(String text)
    {
        if (string.IsNullOrEmpty(text) == false)
        { listOfHistoryAutoRun.Add(string.Format("D{0}: {1}", GameManager.instance.turnScript.Turn, text)); }
        else { Debug.LogWarning("Invalid listOfHistoryAutoRun text (Null or Empty)"); }
    }


    public List<string> GetListOfHistoryAutoRun()
    { return listOfHistoryAutoRun; }


    /// <summary>
    /// Debug display of Resistance player moves
    /// </summary>
    /// <returns></returns>
    public string DebugShowRebelMoves()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("-Resistance Move History (start  nodeID {0}){1}{2}", GameManager.instance.aiRebelScript.GetStartPlayerNode(), "\n", "\n");
        int count = listOfHistoryRebelMove.Count;
        if (count > 0)
        {
            for (int index = 0; index < count; index++)
            {
                HistoryRebelMove history = listOfHistoryRebelMove[index];
                if (history != null)
                { builder.AppendFormat(" t{0}: nodeID {1},  invis {2},  nemesisID {3}{4}{5}", history.turn, history.playerNodeID, history.invisibility, history.nemesisNodeID, 
                    history.playerNodeID == history.nemesisNodeID ? " *" : "", "\n"); }
                else { Debug.LogErrorFormat("Invalid history (Null) in listOfHistoryRebelMoves[{0}]", index); }
            }
        }
        else { builder.Append(" No records present"); }
        return builder.ToString();
    }

    /// <summary>
    /// debug display of Nemesis moves
    /// </summary>
    /// <returns></returns>
    public string DebugShowNemesisMoves()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("-Nemesis Move History (start  nodeID {0}){1}{2}", GameManager.instance.cityScript.cityHallDistrictID, "\n", "\n");
        int count = listOfHistoryNemesisMove.Count;
        if (count > 0)
        {
            for (int index = 0; index < count; index++)
            {
                HistoryNemesisMove history = listOfHistoryNemesisMove[index];
                if (history != null)
                { builder.AppendFormat(" t{0}: nodeID {1}, {2} | {3}, trgtID {4}, serch {5}, aiPlyrID {6}{7}{8}", history.turn, history.nemesisNodeID, history.mode, history.goal, history.targetNodeID, 
                    history.searchRating, history.playerNodeID, history.playerNodeID == history.nemesisNodeID ? " *" : "", "\n"); }
                else { Debug.LogErrorFormat("Invalid history (Null) in listOfHistoryNemesisMoves[{0}]", index); }
            }
        }
        else { builder.Append(" No records present"); }
        return builder.ToString();
    }

    //
    // - - - Statistics - - -
    //

    /// <summary>
    /// adds a new stat to the dictionary with an initial value of statValue (default 0)
    /// </summary>
    /// <param name="statType"></param>
    /// <param name="statValue"></param>
    public void StatisticAddNew(StatType statType, int statValue = 0)
    {
        if (statType != StatType.None)
        {
            try
            { dictOfStatistics.Add(statType, statValue); }
            catch (ArgumentException)
            { Debug.LogErrorFormat("Invalid statType \"{0}\" (duplicate exists)", statType); }
        }
        else { Debug.LogError("Invalid StatType (None)"); }
    }

    /// <summary>
    /// increases stat value by an amount (default +1) for the specific Statistic Type
    /// </summary>
    /// <param name="statType"></param>
    public void StatisticIncrement(StatType statType, int amount = 1)
    {
        if (statType != StatType.None)
        {
            if (dictOfStatistics.ContainsKey(statType) == true)
            { dictOfStatistics[statType] += amount; }
            else { Debug.LogWarningFormat("StatType \"{0}\" not found in dictOfStatistics", statType); }
        }
        else { Debug.LogError("Invalid StatType (None)"); }
    }

    /// <summary>
    /// returns value of specified Statistic type, returns -1 if a problem
    /// </summary>
    /// <param name="statType"></param>
    /// <returns></returns>
    public int StatisticGet(StatType statType)
    {
        int statValue = -1;
        if (statType != StatType.None)
        {
            if (dictOfStatistics.ContainsKey(statType) == true)
            { statValue = dictOfStatistics[statType]; }
        }
        else { Debug.LogError("Invalid StatType (None)"); }
        return statValue;
    }

    //new methods above here
}


