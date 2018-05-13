﻿using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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
    private List<List<Node>> listOfNodesByType = new List<List<Node>>();                        //List containing Lists of Nodes by type -> index[NodeArcID]
    private List<Node> listOfMostConnectedNodes = new List<Node>();                             //top connected nodes (3+ connections), used by AI for ProcessSpiderTeam

    //actor quality input arrays (used to populate arrayOfQualities)
    public Quality[] authorityQualities = new Quality[3];
    public Quality[] resistanceQualities = new Quality[3];

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

    private List<int> resistanceActorPoolLevelOne = new List<int>();
    private List<int> resistanceActorPoolLevelTwo = new List<int>();
    private List<int> resistanceActorPoolLevelThree = new List<int>();
    private List<int> resistanceActorReserve = new List<int>();
    private List<int> resistanceActorDismissed = new List<int>();
    private List<int> resistanceActorPromoted = new List<int>();
    private List<int> resistanceActorDisposedOf = new List<int>();

    //master lists 
    private List<ActorArc> authorityActorArcs = new List<ActorArc>();
    private List<ActorArc> resistanceActorArcs = new List<ActorArc>();
    private List<Trait> listOfAllTraits = new List<Trait>();

    //for fast access
    private List<Target> possibleTargetsPool = new List<Target>();                        //level 1 target and node of the correct type available
    private List<Target> activeTargetPool = new List<Target>();                         //targets onMap but not yet visible to resistance player
    private List<Target> liveTargetPool = new List<Target>();                           //targets OnMap and visible to resistance player
    private List<Target> completedTargetPool = new List<Target>();                       //successfully attempted targets, Status -> Completed
    private List<Target> containedTargetPool = new List<Target>();                    //completed targets that authority has contained (shuts down success Effects)

    private List<List<GameObject>> listOfActorNodes = new List<List<GameObject>>();         //sublists, one each of all the active nodes for each actor (current side)
    private List<int> listOfMoveNodes = new List<int>();                                    //nodeID's of all valid node move options from player's current position

    //node choices (random archetypes) based on number of connections. O.K to have multiple instances of the same archetype in a list in order to tweak the probabilities.
    //NOTE: Public because data is added in Editor -> default version (each city has it's own set, use default if cities version is null)
    [Header("Default Node Mix")]
    public List<NodeArc> listOfOneConnArcsDefault = new List<NodeArc>();
    public List<NodeArc> listOfTwoConnArcsDefault = new List<NodeArc>();
    public List<NodeArc> listOfThreeConnArcsDefault = new List<NodeArc>();
    public List<NodeArc> listOfFourConnArcsDefault = new List<NodeArc>();
    public List<NodeArc> listOfFiveConnArcsDefault = new List<NodeArc>();

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

    //AI persistant data
    private Queue<AITracker> queueRecentNodes = new Queue<AITracker>();
    private Queue<AITracker> queueRecentConnections = new Queue<AITracker>();

    //Adjustments
    private List<ActionAdjustment> listOfActionAdjustments = new List<ActionAdjustment>();

    //dictionaries
    private Dictionary<int, GameObject> dictOfNodeObjects = new Dictionary<int, GameObject>();      //Key -> nodeID, Value -> Node gameObject
    private Dictionary<int, Node> dictOfNodes = new Dictionary<int, Node>();                        //Key -> nodeID, Value -> Node
    private Dictionary<int, NodeArc> dictOfNodeArcs = new Dictionary<int, NodeArc>();               //Key -> nodeArcID, Value -> NodeArc
    private Dictionary<string, int> dictOfLookUpNodeArcs = new Dictionary<string, int>();           //Key -> nodeArc name, Value -> nodeArcID
    private Dictionary<int, ActorArc> dictOfActorArcs = new Dictionary<int, ActorArc>();            //Key -> actorArcID, Value -> ActorArc
    private Dictionary<int, Actor> dictOfActors = new Dictionary<int, Actor>();                     //Key -> actorID, Value -> Actor
    private Dictionary<int, Trait> dictOfTraits = new Dictionary<int, Trait>();                     //Key -> traitID, Value -> Trait
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
    private Dictionary<int, string> dictOfOngoingID = new Dictionary<int, string>();                //Key -> ongoingID, Value -> text string of details
    private Dictionary<int, Faction> dictOfFactions = new Dictionary<int, Faction>();               //Key -> factionID, Value -> Faction
    private Dictionary<int, City> dictOfCities = new Dictionary<int, City>();                       //Key -> cityID, Value -> City
    private Dictionary<int, CityArc> dictOfCityArcs = new Dictionary<int, CityArc>();               //Key -> cityArcID, Value -> CityArc
    private Dictionary<int, Objective> dictOfObjectives = new Dictionary<int, Objective>();         //Key -> objectiveID, Value -> Objective
    private Dictionary<int, Organisation> dictOfOrganisations = new Dictionary<int, Organisation>();//Key -> orgID, Value -> Organisation
    private Dictionary<int, Mayor> dictOfMayors = new Dictionary<int, Mayor>();                     //Key -> mayorID, Value -> Mayor
    

    //global SO's (enum equivalents)
    private Dictionary<string, GlobalMeta> dictOfGlobalMeta = new Dictionary<string, GlobalMeta>();         //Key -> GlobalMeta.name, Value -> GlobalMeta
    private Dictionary<string, GlobalChance> dictOfGlobalChance = new Dictionary<string, GlobalChance>();   //Key -> GlobalChance.name, Value -> GlobalChance
    private Dictionary<string, GlobalType> dictOfGlobalType = new Dictionary<string, GlobalType>();         //Key -> GlobalType.name, Value -> GlobalType
    private Dictionary<string, GlobalSide> dictOfGlobalSide = new Dictionary<string, GlobalSide>();         //Key -> GlobalSide.name, Value -> GlobalSide
    private Dictionary<string, Condition> dictOfConditions = new Dictionary<string, Condition>();           //Key -> Condition.name, Value -> Condition

    #region Archived Import Methods
    /*/// <summary>
    /// first up initialisation
    /// </summary>
    public void InitialiseStart()
    {
        string path;
        //
        // - - - GlobalMeta - - -
        //
        var metaGUID = AssetDatabase.FindAssets("t:GlobalMeta");
        foreach (var guid in metaGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalMeta));
            //assign a zero based unique ID number
            GlobalMeta meta = metaObject as GlobalMeta;
            //add to dictionary
            try
            { dictOfGlobalMeta.Add(meta.name, meta); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid GlobalMeta (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid GlobalMeta (duplicate) \"{0}\"", meta.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfGlobalMeta has {0} entries{1}", dictOfGlobalMeta.Count, "\n"));
        //
        // - - - GlobalChance - - -
        //
        var chanceGUID = AssetDatabase.FindAssets("t:GlobalChance");
        foreach (var guid in chanceGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object chanceObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalChance));
            //assign a zero based unique ID number
            GlobalChance chance = chanceObject as GlobalChance;
            //add to dictionary
            try
            { dictOfGlobalChance.Add(chance.name, chance); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid GlobalChance (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid GlobalChance (duplicate) \"{0}\"", chance.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfGlobalChance has {0} entries{1}", dictOfGlobalChance.Count, "\n"));
        //
        // - - - GlobalType - - -
        //
        var typeGUID = AssetDatabase.FindAssets("t:GlobalType");
        foreach (var guid in typeGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object typeObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalType));
            //assign a zero based unique ID number
            GlobalType type = typeObject as GlobalType;
            //add to dictionary
            try
            { dictOfGlobalType.Add(type.name, type); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid GlobalType (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid GlobalType (duplicate) \"{0}\"", type.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfGlobalType has {0} entries{1}", dictOfGlobalType.Count, "\n"));
        //
        // - - - GlobalSide - - -
        //
        var sideGUID = AssetDatabase.FindAssets("t:GlobalSide");
        foreach (var guid in sideGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object sideObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalSide));
            //assign a zero based unique ID number
            GlobalSide side = sideObject as GlobalSide;
            //add to dictionary
            try
            { dictOfGlobalSide.Add(side.name, side); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid GlobalSide (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid GlobalSide (duplicate) \"{0}\"", side.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfGlobalSide has {0} entries{1}", dictOfGlobalSide.Count, "\n"));
        //
        // - - - Conditions - - -
        //
        var conditionGUID = AssetDatabase.FindAssets("t:Condition");
        foreach (var guid in conditionGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object conditionObject = AssetDatabase.LoadAssetAtPath(path, typeof(Condition));
            //assign a zero based unique ID number
            Condition condition = conditionObject as Condition;
            //add to dictionary
            try
            { dictOfConditions.Add(condition.name, condition); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Condition (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Condition (duplicate) \"{0}\"", condition.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfConditions has {0} entries{1}", dictOfConditions.Count, "\n"));
    }

    /// <summary>
    /// default main constructor
    /// </summary>
    public void InitialiseEarly()
    {
        int counter = 0;
        int length;
        string path;
        GlobalSide globalAuthority = GameManager.instance.globalScript.sideAuthority;
        GlobalSide globalResistance = GameManager.instance.globalScript.sideResistance;
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
            nodeArc.nodeArcID = counter++;
            //add to dictionary
            try
            { dictOfNodeArcs.Add(nodeArc.nodeArcID, nodeArc); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid NodeArc (Null)"); counter--; }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid NodeArc (duplicate) ID \"{0}\" for  \"{1}\"", counter, nodeArc.name)); counter--; }
            //add to lookup dictionary
            try
            { dictOfLookUpNodeArcs.Add(nodeArc.name, nodeArc.nodeArcID); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid NodeArc (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid NodeArc (duplicate) Name \"{0}\" for ID \"{1}\"", nodeArc.name, nodeArc.nodeArcID)); }
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
            trait.traitID = counter++;
            //add to dictionary
            try
            {
                dictOfTraits.Add(trait.traitID, trait);
                //add to list
                listOfAllTraits.Add(trait);
            }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Trait (Null)"); counter--; }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Trait (duplicate) ID \"{0}\" for \"{1}\"", counter, trait.name)); counter--; }
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
            arc.ActorTag = arc.actorName.Substring(0, length);
            //add to dictionary
            try
            {
                dictOfActorArcs.Add(arc.ActorArcID, arc);
                //add to list
                if (arc.side.level == globalAuthority.level) { authorityActorArcs.Add(arc); }
                else if (arc.side.level == globalResistance.level) { resistanceActorArcs.Add(arc); }
                else { Debug.LogWarning(string.Format("Invalid side \"{0}\", actorArc \"{1}\" NOT added to list", arc.side.name, arc.name)); }
            }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Actor Arc (Null)"); counter--; }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid actorArc (duplicate) ID \"{0}\" for \"{1}\"", counter, arc.name)); counter--; }
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
            effect.effectID = counter++;
            //if a hide/reveal need to add level, eg. +1, onto end (do dynamically to prevent errors in SO's)
            switch (effect.outcome.name)
            {
                case "StatusSpiders":
                case "StatusTracers":
                case "StatusContacts":
                case "StatusTeams":
                    switch (effect.operand.name)
                    {
                        case "Add":
                            effect.textTag = string.Format("{0} +{1}", effect.description, effect.value);
                            break;
                        case "Subtract":
                            effect.textTag = string.Format("{0} -{1}", effect.description, effect.value);
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                            effect.textTag = "Unknown";
                            break;
                    }
                    break;
                //all other effects
                default:
                    effect.textTag = effect.description;
                    break;
            }
            //add to dictionary
            try
            { dictOfEffects.Add(effect.effectID, effect); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Effect (Null)"); counter--; }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Effect (duplicate) effectID \"{0}\" for \"{1}\"", counter, effect.name)); counter--; }
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
            target.targetID = counter++;
            target.targetStatus = Status.Dormant;
            target.timer = -1;
            target.infoLevel = 1;
            target.isKnownByAI = false;
            target.nodeID = -1;
            target.ongoingID = -1;
            //add to dictionary
            try
            { dictOfTargets.Add(target.targetID, target); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Target (Null)"); counter--; }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Target (duplicate) ID \"{0}\" for \"{1}\"", counter, target.name)); counter--; }
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
            { Debug.LogError("Invalid Action Arc (Null)"); counter--; }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid (duplicate) ActionID \"{0}\" for Action \"{1}\"", action.ActionID, action.name)); counter--; }
            //add to lookup dictionary
            try
            { dictOfLookUpActions.Add(action.name, action.ActionID); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Lookup Actions (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Lookup Actions (duplicate) ID \"{0}\" for \"{1}\"", counter, action.name)); }
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
            { Debug.LogError("Invalid TeamArc (Null)"); counter--; }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid TeamArc (duplicate) ID \"{0}\" for \"{1}\"", counter, teamArc.name)); counter--; }
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
        // - - - Gear - - -
        //
        counter = 0;
        //get GUID of all SO Gear Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var gearGUID = AssetDatabase.FindAssets("t:Gear");
        foreach (var guid in gearGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object gearObject = AssetDatabase.LoadAssetAtPath(path, typeof(Gear));
            //assign a zero based unique ID number
            Gear gear = gearObject as Gear;
            //set data
            gear.gearID = counter++;
            //add to dictionary
            try
            { dictOfGear.Add(gear.gearID, gear); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Gear (Null)"); counter--; }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Gear (duplicate) ID \"{0}\" for \"{1}\"", counter, gear.name)); counter--; }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfGear has {0} entries{1}", counter, "\n"));
        //
        // - - - Gear Rarity - - -
        //
        var gearRarityGUID = AssetDatabase.FindAssets("t:GearRarity");
        foreach (var guid in gearRarityGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object gearRarityObject = AssetDatabase.LoadAssetAtPath(path, typeof(GearRarity));
            GearRarity gearRarity = gearRarityObject as GearRarity;
            //add to list
            if (gearRarity != null)
            { listOfGearRarity.Add(gearRarity); }
            else { Debug.LogError("Invalid gearRarity (Null)"); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> listOfGearRarity has {0} entries{1}", listOfGearRarity.Count, "\n"));
        //
        // - - - Gear Type - - -
        //
        var gearTypeGUID = AssetDatabase.FindAssets("t:GearType");
        foreach (var guid in gearTypeGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object gearTypeObject = AssetDatabase.LoadAssetAtPath(path, typeof(GearType));
            GearType gearType = gearTypeObject as GearType;
            //add to list
            if (gearType != null)
            { listOfGearType.Add(gearType); }
            else { Debug.LogError("Invalid gearType (Null)"); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> listOfGearType has {0} entries{1}", listOfGearType.Count, "\n"));
        //
        // - - - Manage Actions - - -
        //
        var manageGUID = AssetDatabase.FindAssets("t:ManageAction");
        foreach (var guid in manageGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object manageObject = AssetDatabase.LoadAssetAtPath(path, typeof(ManageAction));
            ManageAction manageAction = manageObject as ManageAction;
            //add to dictionary
            try
            {
                dictOfManageActions.Add(manageAction.name, manageAction);
                //add to the appropriate fast access list
                switch (manageAction.manage.name)
                {
                    case "Handle":
                        listOfActorHandle.Add(manageAction);
                        break;
                    case "Reserve":
                        listOfActorReserve.Add(manageAction);
                        break;
                    case "Dismiss":
                        listOfActorDismiss.Add(manageAction);
                        break;
                    case "Dispose":
                        listOfActorDispose.Add(manageAction);
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid manage.manage.name \"{0}\"", manageAction.manage.name));
                        break;
                }
            }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid manage Action (Null)"); counter--; }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid ManageAction (duplicate name)  \"{0}\"", manageAction.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfManageActions has {0} entries{1}", dictOfManageActions.Count, "\n"));
        //sort fast access lists by order -> ActorHandle
        if (listOfActorHandle.Count > 0)
        {
            var manageActions = from element in listOfActorHandle
                                orderby element.order
                                select element;
            listOfActorHandle = manageActions.ToList();
        }
        else { Debug.LogError("There are no entries in listOfActorHandle"); }
        //ActorReserve
        if (listOfActorReserve.Count > 0)
        {
            var manageActions = from element in listOfActorReserve
                                orderby element.order
                                select element;
            listOfActorReserve = manageActions.ToList();
        }
        else { Debug.LogError("There are no entries in listOfActorReserve"); }
        //ActorDismiss
        if (listOfActorDismiss.Count > 0)
        {
            var manageActions = from element in listOfActorDismiss
                                orderby element.order
                                select element;
            listOfActorDismiss = manageActions.ToList();
        }
        else { Debug.LogError("There are no entries in listOfActorDismiss"); }
        //ActorDispose
        if (listOfActorDispose.Count > 0)
        {
            var manageActions = from element in listOfActorDispose
                                orderby element.order
                                select element;
            listOfActorDispose = manageActions.ToList();
        }
        else { Debug.LogError("There are no entries in listOfActorDispose"); }
        //
        // - - - Actor Qualities - - -
        //
        int numOfQualities = GameManager.instance.actorScript.numOfQualities;
        arrayOfStatTags = new string[GetNumOfGlobalSide(), numOfQualities];
        for (int i = 0; i < 3; i++)
        {
            //authority qualities
            if (authorityQualities[i] != null)
            {
                if (authorityQualities[i].side.level == globalAuthority.level)
                { arrayOfStatTags[globalAuthority.level, i] = authorityQualities[i].name; }
                else
                {
                    Debug.LogWarning(string.Format("Quality (\"{0}\")is the wrong side (\"{1}\"){2}", authorityQualities[i].name, authorityQualities[i].side.name, "\n"));
                    arrayOfStatTags[globalAuthority.level, i] = "Unknown";
                }
            }
            else { arrayOfStatTags[globalAuthority.level, i] = "Unknown"; }
            //resistance qualities
            if (resistanceQualities[i] != null)
            {
                if (resistanceQualities[i].side.level == globalResistance.level)
                { arrayOfStatTags[globalResistance.level, i] = resistanceQualities[i].name; }
                else
                {
                    Debug.LogWarning(string.Format("Quality (\"{0}\")is the wrong side (\"{1}\"){2}", resistanceQualities[i].name, resistanceQualities[i].side.name, "\n"));
                    arrayOfStatTags[globalResistance.level, i] = "Unknown";
                }
            }
            else { arrayOfStatTags[globalResistance.level, i] = "Unknown"; }
        }
        //arrayOfActors & Positions
        arrayOfActors = new Actor[GetNumOfGlobalSide(), GameManager.instance.actorScript.maxNumOfOnMapActors];
        arrayOfActorsPresent = new bool[GetNumOfGlobalSide(), GameManager.instance.actorScript.maxNumOfOnMapActors];
        //
        // - - - Factions - - -
        //
        counter = 0;
        //get GUID of all SO Faction Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var factionGUID = AssetDatabase.FindAssets("t:Faction");
        foreach (var guid in factionGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object factionObject = AssetDatabase.LoadAssetAtPath(path, typeof(Faction));
            //assign a zero based unique ID number
            Faction faction = factionObject as Faction;
            //set data
            faction.factionID = counter++;
            //add to dictionary
            try
            { dictOfFactions.Add(faction.factionID, faction); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Faction (Null)"); counter--; }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Faction (duplicate) ID \"{0}\" for \"{1}\"", counter, faction.name)); counter--; }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfFactions has {0} entries{1}", counter, "\n"));
        //
        // - - - Cities - - -
        //
        counter = 0;
        //get GUID of all SO City Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var cityGUID = AssetDatabase.FindAssets("t:City");
        foreach (var guid in cityGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object cityObject = AssetDatabase.LoadAssetAtPath(path, typeof(City));
            //assign a zero based unique ID number
            City city = cityObject as City;
            //set data
            city.cityID = counter++;
            //add to dictionary
            try
            { dictOfCities.Add(city.cityID, city); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid City (Null)"); counter--; }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid City (duplicate) ID \"{0}\" for \"{1}\"", counter, city.name)); counter--; }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfCities has {0} entries{1}", counter, "\n"));
        //
        // - - - Objectives - - -
        //
        counter = 0;
        //get GUID of all SO Objective Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var objectiveGUID = AssetDatabase.FindAssets("t:Objective");
        foreach (var guid in objectiveGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object objectiveObject = AssetDatabase.LoadAssetAtPath(path, typeof(Objective));
            //assign a zero based unique ID number
            Objective objective = objectiveObject as Objective;
            //set data
            objective.objectiveID = counter++;
            //add to dictionary
            try
            { dictOfObjectives.Add(objective.objectiveID, objective); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Objective (Null)"); counter--; }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Objective (duplicate) ID \"{0}\" for \"{1}\"", counter, objective.name)); counter--; }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfObjectives has {0} entries{1}", counter, "\n"));
    }
    */
    #endregion

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
        int limit = CheckNumOfNodeTypes();
        for(int i = 0; i < limit; i++)
        {
            List<Node> tempList = new List<Node>();
            listOfNodesByType.Add(tempList);
        }
        //Populate List of lists -> place node in the correct list
        foreach(var nodeObj in dictOfNodeObjects)
        {
            Node node = nodeObj.Value.GetComponent<Node>();
            listOfNodesByType[node.Arc.nodeArcID].Add(node);
        }
        //event listener
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "DataManager");
    }

    /// <summary>
    /// stuff that is done a lot later in the process (dependant on other stuff being done first). Must be after metaScript.Initialise()
    /// </summary>
    public void InitialiseFinal()
    {
        //
        // - - - Possible Targets - - - 
        //
        int currentMetaLevel = GameManager.instance.metaScript.metaLevel.level;
        foreach (var target in dictOfTargets)
        {
            //add to list pf Possible targets if a level 1 target & nodes of the required type are available
            if (target.Value.targetLevel == 1)
            {
                //check target is the correct metaLevel or that no metaLevel has been specified
                if (target.Value.metaLevel == null || target.Value.metaLevel.level == currentMetaLevel)
                {
                    //add to list of Possible targets
                    if (CheckNodeInfo(target.Value.nodeArc.nodeArcID, NodeInfo.Number) > 0)
                    { possibleTargetsPool.Add(target.Value); }
                    else
                    {
                        Debug.Log(string.Format("DataManager: {0} has been ignored as there are no required node types present (\"{1}\"){2}",
                            target.Value.name, target.Value.nodeArc.name, "\n"));
                    }
                }
            }
        }
        Debug.Log(string.Format("DataManager: Initialise -> possibleTargetPool has {0} records{1}", possibleTargetsPool.Count, "\n"));
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
    { listOfActorNodes = GameManager.instance.levelScript.GetListOfActorNodes(GameManager.instance.sideScript.PlayerSide);}

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
            case 1:
                tempList = listOfOneConnArcsDefault;
                break;
            case 2:
                tempList = listOfTwoConnArcsDefault;
                break;
            case 3:
                tempList = listOfThreeConnArcsDefault;
                break;
            case 4:
                tempList = listOfFourConnArcsDefault;
                break;
            case 5:
                tempList = listOfFiveConnArcsDefault;
                break;
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
        else { Debug.LogWarning("Not found in Dict > nodeArcID " + nodeArcID);}
        return null;
    }

    public Dictionary<int, NodeArc> GetDictOfNodeArcs()
    { return dictOfNodeArcs; }

    public Dictionary<string, int> GetDictOfLookUpNodeArcs()
    { return dictOfLookUpNodeArcs; }


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
    // - - - Actor Arcs and Traits - - - 
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
        if(side.level == GameManager.instance.globalScript.sideAuthority.level) { tempMaster.AddRange(authorityActorArcs); }
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
        {
            return arc;
        }
        return null;
    }

    public Dictionary<int, ActorArc> GetDictOfActorArcs()
    { return dictOfActorArcs; }

    public List<ActorArc> GetListOfAuthorityActorArcs()
    { return authorityActorArcs; }

    public List<ActorArc> GetListOfResistanceActorArcs()
    { return resistanceActorArcs; }

    /// <summary>
    /// return a random trait (could be good, bad or neutral)
    /// </summary>
    /// <returns></returns>
    public Trait GetRandomTrait()
    {
        return listOfAllTraits[Random.Range(0, listOfAllTraits.Count)];
    }

    public Dictionary<int, Trait> GetDictOfTraits()
    { return dictOfTraits; }

    public List<Trait> GetListOfAllTraits()
    { return listOfAllTraits; }

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


    public Dictionary<int, Node> GetDictOfNodes()
    { return dictOfNodes; }

    /// <summary>
    /// Get int data from Master node array
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public int CheckNodeInfo(int nodeIndex, NodeInfo info)
    {
        Debug.Assert(nodeIndex > -1 && nodeIndex < CheckNumOfNodeTypes(), "Invalid nodeIndex");
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
        Debug.Assert(nodeIndex > -1 && nodeIndex < CheckNumOfNodeTypes(), "Invalid nodeIndex");
        arrayOfNodes[nodeIndex, (int)info] = newData;
    }

    /// <summary>
    /// return total number of nodes in the level
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfNodes()
    { return dictOfNodeObjects.Count; }

    /// <summary>
    /// returns number of different node arc types on level, eg. "Corporate" + "Utility" would return 2
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfNodeTypes()
    { return arrayOfNodes.Length; }

    /// <summary>
    /// intialised in ImportManager.cs
    /// </summary>
    public void InitialiseArrayOfNodes()
    { arrayOfNodes = new int[GameManager.instance.levelScript.GetNodeTypeTotals().Length, (int)NodeInfo.Count]; }

    public int[,] GetArrayOfNodes()
    { return arrayOfNodes; }


    /// <summary>
    /// return a list of Nodes, all of which are the same type (nodeArcID)
    /// </summary>
    /// <param name="nodeArcID"></param>
    /// <returns></returns>
    public List<Node> GetListOfNodesByType(int nodeArcID)
    {
        Debug.Assert(nodeArcID > -1 && nodeArcID < CheckNumOfNodeTypes(), "Invalid nodeArcID parameter");
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

    public Dictionary<int, Connection> GetAllConnections()
    { return dictOfConnections; }

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

    public List<Node> GetMostConnectedNodes()
    { return listOfMostConnectedNodes; }
    

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



    public int CheckNumOfPossibleTargets()
    { return possibleTargetsPool.Count; }

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
                tempList = activeTargetPool;
                break;
            case Status.Live:
                tempList = liveTargetPool;
                break;
            case Status.Completed:
                tempList = completedTargetPool;
                break;
            case Status.Contained:
                tempList = containedTargetPool;
                break;
            default:
                Debug.LogError(string.Format("Invalid status \"{0}\"", status));
                break;
        }
        return tempList;
    }

    public List<Target> GetPossibleTargets()
    { return possibleTargetsPool; }



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
                case Status.Dormant:
                    possibleTargetsPool.Add(target);
                    break;
                case Status.Active:
                    activeTargetPool.Add(target);
                    break;
                case Status.Live:
                    liveTargetPool.Add(target);
                    break;
                case Status.Completed:
                    completedTargetPool.Add(target);
                    break;
                case Status.Contained:
                    containedTargetPool.Add(target);
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
                case Status.Dormant:
                    listOfTargets = possibleTargetsPool;
                    break;
                case Status.Active:
                    listOfTargets = activeTargetPool;
                    break;
                case Status.Live:
                    listOfTargets = liveTargetPool;
                    break;
                case Status.Completed:
                    listOfTargets = completedTargetPool;
                    break;
                case Status.Contained:
                    listOfTargets = containedTargetPool;
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
                    Debug.Log(string.Format("DataManager: Target \"{0}\", ID {1}, successfully removed from {2} List{3}", target.name, target.targetID, status, "\n"));
                    break;
                }
            }
        }
        else { Debug.LogError("Invalid List target parameter (Null)"); }
        return isSuccess;
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
    /// Remove a team from a designated pool
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="teamID"></param>
    public void RemoveTeamFromPool(TeamPool pool, int teamID)
    {
        switch (pool)
        {
            case TeamPool.Reserve:
                teamPoolReserve.Remove(teamID);
                break;
            case TeamPool.OnMap:
                teamPoolOnMap.Remove(teamID);
                break;
            case TeamPool.InTransit:
                teamPoolInTransit.Remove(teamID);
                break;
            default:
                Debug.LogError(string.Format("Invalid team pool \"{0}\"", pool));
                break;
        }
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
        switch(pool)
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
        switch(pool)
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
            for(int i = 0; i < teamPoolReserve.Count; i++)
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
            //update actor GUI display
            GameManager.instance.guiScript.UpdateActorGUI();
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
            switch(status)
            {
                case ActorStatus.ReservePool:
                    if (AddActorToReserve(actor.actorID, side) == true)
                    {
                        //update actor arrays
                        arrayOfActors[side.level, actor.actorSlotID] = null;
                        arrayOfActorsPresent[side.level, actor.actorSlotID] = false;
                        actor.Status = status;
                        //update actor GUI display
                        GameManager.instance.guiScript.UpdateActorGUI();
                        return true;
                    }
                    else
                    { Debug.LogWarning("RemoveCurrentActor: Reserve Pool is Full"); }
                    break;
                case ActorStatus.Dismissed:
                    if (AddActorToDismissed(actor.actorID, side) == true)
                    {
                        //update actor arrays
                        arrayOfActors[side.level, actor.actorSlotID] = null;
                        arrayOfActorsPresent[side.level, actor.actorSlotID] = false;
                        actor.Status = status;
                        GameManager.instance.guiScript.UpdateActorGUI();
                        return true;
                    }
                    break;
                case ActorStatus.Promoted:
                    if (AddActorToPromoted(actor.actorID, side) == true)
                    {
                        //update actor arrays
                        arrayOfActors[side.level, actor.actorSlotID] = null;
                        arrayOfActorsPresent[side.level, actor.actorSlotID] = false;
                        actor.Status = status;
                        GameManager.instance.guiScript.UpdateActorGUI();
                        return true;
                    }
                    break;
                case ActorStatus.Killed:
                    if (AddActorToDisposedOf(actor.actorID, side) == true)
                    {
                        //update actor arrays
                        arrayOfActors[side.level, actor.actorSlotID] = null;
                        arrayOfActorsPresent[side.level, actor.actorSlotID] = false;
                        actor.Status = status;
                        GameManager.instance.guiScript.UpdateActorGUI();
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
                Debug.LogWarning(string.Format("Invalid Side \"{0}\", actorID NOT added to pool", side.name));
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
                Debug.LogWarning(string.Format("Invalid Side \"{0}\", actorID NOT added to pool", side.name));
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
                Debug.LogWarning(string.Format("Invalid Side \"{0}\", actorID NOT added to pool", side.name));
                successFlag = false;
                break;
        }
        return successFlag;
    }

    /// <summary>
    /// add an actor to the Disposed Of pool for that side. Returns ture if successful
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
                Debug.LogWarning(string.Format("Invalid Side \"{0}\", actorID NOT added to pool", side.name));
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
    /// Get specific actor (OnMap, active or inactive)
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
    /// returns type of Actor, eg. 'Fixer', based on slotID (0 to 3)
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
            Actor actor = arrayOfActors[side.level, i];
            if (actor.arc.ActorArcID == actorArcID && actor.Status == ActorStatus.Active)
            { return actor.actorSlotID; }
        }
        return slotID;
    }

    /// <summary>
    /// returns true if specified actor Arc is present in line up and active, false otherwise
    /// </summary>
    /// <param name="arc"></param>
    /// <returns></returns>
    public bool CheckActorArcPresent(ActorArc arc, GlobalSide side)
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
                    //check actor is Active and of the correct type
                    if (actor.arc == arc && actor.Status == ActorStatus.Active) { return true; }
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
    {arrayOfActors = new Actor[GetNumOfGlobalSide(), GameManager.instance.actorScript.maxNumOfOnMapActors]; }

    /// <summary>
    /// called from ImportManager.cs
    /// </summary>
    public void InitialiseArrayOfActorsPresent()
    { arrayOfActorsPresent = new bool[GetNumOfGlobalSide(), GameManager.instance.actorScript.maxNumOfOnMapActors];}

    public Actor[,] GetArrayOfActors()
    { return arrayOfActors; }

    public bool[,] GetArrayOfActorsPresent()
    { return arrayOfActorsPresent; }

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
        //resistance
        builder.Append(string.Format("{0} - Resistance Reserve List{1}", "\n", "\n"));
        builder.Append(GetActorList(resistanceActorReserve));
        builder.Append(string.Format("{0} - Resistance Promoted List{1}", "\n", "\n"));
        builder.Append(GetActorList(resistanceActorPromoted));
        builder.Append(string.Format("{0} - Resistance Dismissed List{1}", "\n", "\n"));
        builder.Append(GetActorList(resistanceActorDismissed));
        builder.Append(string.Format("{0} - Resistance DisposedOf List{1}", "\n", "\n"));
        builder.Append(GetActorList(resistanceActorDisposedOf));
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
                    builder.Append(string.Format(" ID {0}, {1}, L{2}, {3}-{4}-{5} U {6} {7}", actor.actorID, actor.arc.name, actor.level,
                      actor.datapoint0, actor.datapoint1, actor.datapoint2, actor.unhappyTimer, "\n"));
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
    /// return a list of all nodes where an actor (slotID) is active
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public List<GameObject> GetListOfActorNodes(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid slotID");
        return listOfActorNodes[slotID];
    }

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
        else { Debug.LogWarning(string.Format("Not found in gearID {0}, in dict {1}", gearID, "\n")); }
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
                switch(rarity.name)
                {
                    case "Common":
                        listOfCommonGear.AddRange(listOfGearID);
                        break;
                    case "Rare":
                        listOfRareGear.AddRange(listOfGearID);
                        break;
                    case "Unique":
                        listOfUniqueGear.AddRange(listOfGearID);
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid rarity \"{0}\"", rarity.name));
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
        switch (rarity.name)
        {
            case "Common":
                tempList = listOfCommonGear;
                break;
            case "Rare":
                tempList = listOfRareGear;
                break;
            case "Unique":
                tempList = listOfUniqueGear;
                break;
            default:
                Debug.LogError(string.Format("Invalid rarity \"{0}\"", rarity.name));
                break;
        }
        //return list
        return tempList;
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
            Debug.Log(string.Format("Msg: {0}{1}", message.text, "\n"));
            //auto sort
            switch (message.isPublic)
            {
                case true:
                    //if isPublic true then store in Pending dictionary
                    AddMessageExisting(message, MessageCategory.Pending);
                    break;
                case false:
                    //if isPublic False then store in Archive dictionary
                    AddMessageExisting(message, MessageCategory.Archive);
                    if (message.type == gameAPI.MessageType.AI)
                    { AIMessage(message); }
                    break;
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
            builderAuthority.Append(string.Format("{0}{1} Messages -> Authority{2}", "\n", "\n", "\n"));
            //max # of records per side
            int numOfRecordsPerSide = 20;
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

    /// <summary>
    /// Add an ongoingID to the register (dict). No programming necessity for this other than tracking and debugging
    /// </summary>
    /// <param name="ongoingID"></param>
    /// <param name="details"></param>
    public void AddOngoingEffectToDict(EffectDataOngoing ongoing, int nodeID)
    {
        if (ongoing != null)
        {
            //add new ongoing effect only if no other instance of it exists, ignore otherwise
            if (dictOfOngoingID.ContainsKey(ongoing.ongoingID) == false)
            {
                string text = string.Format("id {0}, {1}", ongoing.ongoingID, ongoing.text);
                //add to dictionary
                try
                {
                    dictOfOngoingID.Add(ongoing.ongoingID, text);
                    //generate message
                    Message message = GameManager.instance.messageScript.OngoingEffectCreated(text, ongoing.side, nodeID);
                    GameManager.instance.dataScript.AddMessage(message);
                }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid ongoingID (duplicate) \"{0}\" for \"{1}\"", ongoing.ongoingID, text)); }
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
        { builder.Append(string.Format("{0} {1}", "\n", ongoing.Value)); }
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
                Message message = GameManager.instance.messageScript.OngoingEffectExpired(text, ongoing.side, dataID);
                GameManager.instance.dataScript.AddMessage(message);
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

    //
    // - - - Global SO's - - -
    //

    public Dictionary<string, GlobalMeta> GetDictOfGlobalMeta()
    { return dictOfGlobalMeta; }

    public Dictionary<string, GlobalChance> GetDictOfGlobalChance()
    { return dictOfGlobalChance; }

    public Dictionary<string, GlobalType> GetDictOfGlobalType()
    { return dictOfGlobalType; }

    public Dictionary<string, GlobalSide> GetDictOfGlobalSide()
    { return dictOfGlobalSide; }

    public int GetNumOfGlobalSide()
    { return dictOfGlobalSide.Count; }

    /// <summary>
    /// Returns condition SO, Null if not found in dictionary
    /// </summary>
    /// <param name="conditionName"></param>
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
        List<City> listOfCities = dictOfCities.Values.ToList();
        city = listOfCities[Random.Range(0, listOfCities.Count)];
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
    /// Gets a random Mayor, returns null if none. NOTE: Debug method, Duplicates ARE NOT CHECKED FOR
    /// </summary>
    /// <returns></returns>
    public Mayor GetRandomMayor()
    {
        Mayor mayor = null;
        List<Mayor> tempList = dictOfMayors.Values.ToList();
        if (tempList != null)
        { mayor = tempList[Random.Range(0, tempList.Count)]; }
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
                    { listOfActionAdjustments.RemoveAt(i); }
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


   

    //new methods above here
}


