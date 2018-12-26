﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using UnityEngine.UI;

namespace packageAPI
{

    /// <summary>
    /// Package Manager
    /// used to handle all data package classes
    /// </summary>

    //
    // - - - Tooltip Data Packages - - -
    //

    /// <summary>
    /// used to pass data package to node Tooltip
    /// </summary>
    public class NodeTooltipData
    {
        public string nodeName;
        public string specialName;                              //eg. airport name, icon name, etc. Ignore if not relevant.
        public string type;
        public bool isTargetKnown;
        public bool isContact;                                   //specific for current side
        public bool isContactKnown;                                     //resistance contact known to authority?
        public bool isTracer;
        public bool isTracerActive;
        public bool isTracerKnown;
        public bool isTeamKnown;
        public bool isSpiderKnown;                                      //displays Spider images at tooltip top if spider present in node and known
        public int spiderTimer;                                         //ignore timers if spider or tracer aren't present
        public int tracerTimer;
        public int[] arrayOfStats;
        public List<string> listOfCrisis;
        public List<string> listOfContactsCurrent;                      //current side
        public List<string> listOfContactsOther;                        //non-current side
        public List<EffectDataTooltip> listOfEffects;
        public List<string> listOfTeams;
        public List<string> listOfTargets;
        public List<string> listOfActivity;                             //used to display activity data when NodeManager.cs -> activityState > 'None'. Ignore otherwise
        public Vector3 tooltipPos;
    }

    /// <summary>
    /// used to pass data package to actor Tooltip
    /// </summary>
    public class ActorTooltipData
    {
        public Vector3 tooltipPos;
        public Actor actor;
        public Action action;
        public Gear gear;
        public List<string> listOfSecrets; 
        public string[] arrayOfQualities;
        public int[] arrayOfStats;
    }

    /// <summary>
    /// data package for Generic Tooltip
    /// </summary>
    public class GenericTooltipData
    {
        public Vector3 screenPos;
        public string main;                                      //required, header and details are optional
        public string header;
        public string details;

        public GenericTooltipData()
        {
            header = null;
            details = null;
        }
    }

    //
    // - - - Effect Data Packages - - -
    //

    /// <summary>
    /// used to pass data to CheckCriteria (EffectManager.cs)
    /// </summary>
    public class CriteriaDataInput
    {
        public int nodeID;                          //optional
        public int actorSlotID;                     //optional
        public int teamArcID;                       //optional
        public List<Criteria> listOfCriteria;

        public CriteriaDataInput()
        {
            nodeID = -1;
            actorSlotID = -1;
            teamArcID = -1;
        }
    }

    /// <summary>
    /// used to pass data into ProcessEffect
    /// </summary>
    public class EffectDataInput
    {
        public GlobalSide side;                                              //used to determine colouring of good/bad effects
        public int ongoingID;                                                //used only if there are going to be ongoing effects, ignore otherwise
        public string ongoingText;                                           //used only if there are going to be ongoing effects, ignore otherwise
        public string originText;                                            //name of thing that caused the effect, eg. gear name
        public int data;                                                     //multipurpose datapoint, eg. gearID


        public EffectDataInput()
        {
            ongoingID = -1; ongoingText = "";
            side = GameManager.instance.sideScript.PlayerSide;
        }
    }

    /// <summary>
    /// used to return results of effects to calling method
    /// ActionManager.cs -> <EffectDataReturn> ProcessNode... -> ProcessEffect -> EffectDataReturn
    /// </summary>
    public class EffectDataReturn
    {
        public string topText { get; set; }
        public string bottomText { get; set; }
        public bool errorFlag { get; set; }
        public bool isAction;                       //true if effect is considered an action
        public int ongoingID;                       //only needed if there is an ongoing effect (any value > -1, if '-1' ignore)
    }

    /// <summary>
    /// used for returning text data from effect resolution sub methods. 
    /// ProcessEffect -> <EffectDataResolve> subMethod -> EffectDataResolve returned to ProcessEffect
    /// </summary>
    public class EffectDataResolve
    {
        public string topText;
        public string bottomText;
        public bool isError;
    }

    /// <summary>
    /// sends a package of data to the node for processing of one off or ongoing effects on a node field
    /// ProcessEffect -> subMethod -> EffectDataProcess -> Node.cs -> ProcessNodeEffect(EffectDataProcess)
    /// </summary>
    public class EffectDataProcess
    {
        public EffectOutcome outcome;
        public EffectDataOngoing effectOngoing = null;                      //only used if an ongoing effect, ignore otherwise 
        public int value;                                                   //how much the field changes, eg. +1, -1, etc.
        public string text;                                                 //tooltip description for the temporary effect
    }

    /// <summary>
    /// used to pass data back to Node for an ongoing effect
    /// </summary>
    public class EffectDataOngoing
    {
        public int ongoingID = -1;                                        //links back to a central registry to enable cancelling of ongoing effect at a later point
        public string text;
        public string description;                                        //description of effect (used for InfoApp)
        public string nodeTooltip;                                        //descriptor used for node tooltip
        public string reason;                                             //reason why (used for InfoApp)
        public string gearName;                                           //originating gear (used for InfoApp, use only if effect is gear based)
        public int value;                                                 //how much the field changes, eg. +1, -1, etc.
        public int timer;                                                 //how long does the effect last for?
        public int gearID;                                                //gearID (used for InfoApp, use only if effect is gear based), default -1
        public Node node;                                                 //originating node (used for InfoApp, use only if effect is node based)
        public EffectOutcome outcome;
        public GlobalType type;                                           //benefit, or otherwise, of effect from POV of Resistance
        public EffectApply apply;
        public GlobalSide side;

        public EffectDataOngoing()
        {
            gearID = -1;
            timer = GameManager.instance.effectScript.ongoingEffectTimer;
        }
    }


    /// <summary>
    /// used to pass data from Node to NodeTooltip.cs for display of Ongoing effects in the correct colour
    /// </summary>
    public class EffectDataTooltip
    {
        public string text;
        public GlobalType type;
    }

    //
    // - - - Other Packages - - -
    //

    /// <summary>
    /// used to return data from ModalDiceUI.cs -> ProcessDiceOutcome to NodeManager.cs -> ProcessMoveOutcome
    /// </summary>
    public class MoveReturnData
    {
        public Node node;
        public string text;
    }


    /// <summary>
    /// used to track adjustments to either sides per turn action allowance
    /// </summary>
    public class ActionAdjustment
    {
        public GlobalSide side;
        public string descriptor;                                       //short (two word) text descriptor used in Turn tooltip
        public int value;                                               //change in normal action allocation (use Mathf.ABS value, eg. 1 for both plus and minus)
        public int timer;                                               //number of turns that the effect lasts for (decremented down to zero), set to 999 for continuous
        public int turnStart;                                           //turn number where effect commences (added automatically, used for actions tooltip). Ignore.
        public EffectDataOngoing ongoing;                               //Ongoing effects only, ignore otherwise
    }


    /// <summary>
    /// Used to pass data for Capturing Resistance actors & player
    /// </summary>
    public class CaptureDetails
    {
        public Node node;
        public Team team;
        public Actor actor;
        public string effects;                  //carry over effects for a combined outcome window, eg insert tracer and then get captured
    }

    /// <summary>
    /// Main Info App data package
    /// </summary>
    public class MainInfoData
    {

        public List<ItemData>[] arrayOfItemData = new List<ItemData>[(int)ItemTab.Count];           //array of lists, one per MainInfoUI.cs tab excluding 'help'
        public string tickerText;

        public MainInfoData()
        {
            //initialise arrayOfLists
            for (int i = 0; i < (int)ItemTab.Count; i++)
            { arrayOfItemData[i] = new List<ItemData>(); }
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="dataCopy"></param>
        public MainInfoData(MainInfoData dataCopy)
        {
            for (int i = 0; i < (int)ItemTab.Count; i++)
            { arrayOfItemData[i] = new List<ItemData>(dataCopy.arrayOfItemData[i]); }
        }

        /// <summary>
        /// Empties out all data
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < (int)ItemTab.Count; i++)
            { arrayOfItemData[i].Clear(); }
        }
    }

    /// <summary>
    /// multipurpose class to handle message equivalents, requests and meeting decision options for MainInfoData packages
    /// NOTE: Don't add any methods as it needs to be memory efficient
    /// </summary>
    public class ItemData
    {
        public string itemText;                     //what is shown for the item
        public string topText;
        public string bottomText;
        public Sprite sprite;
        public ItemPriority priority;
        public ItemTab tab;
        public GlobalSide side;
        public int delay = 0;                       //allows for a delay in itemData showing, delay is in turns, default zero
        public int nodeID = -1;                     //if > -1 then 'Show Me' button will activate and user can press to see where the node is on the map
        public int connID = -1;                     //if > -1 then 'Show Me' button will activate and user can press to see where the connection is on teh map (can be used together with nodeID)
        public int buttonData;                      //data to send when button pressed (can ignore) -> Must have both buttonData AND buttonEvent for a button to display
        public EventType buttonEvent;               //event to trigger when button pressed (can ignore) -> Must have both buttonData AND buttonEvent for a button to display
        public int help = -1;                       //key to dictOfHelp for info button down at bottom (can ignore) -> wil. display help button if present (make sure tag's ae set below for specific topics
        public string tag0;                         //help topic, provide tag or leave Null if none. NOTE: add help topics in sequence -> 0 / 1 / 2 / 3, and make sure help > 0 
        public string tag1;
        public string tag2;
        public string tag3;
    }

    /// <summary>
    /// Used to return data from ActorManager.cs -> GetManageRenownCost for extra costs incurred due to actor knowing secrets and/ or threatening player
    /// </summary>
    public class ManageRenownCost
    {
        public int renownCost;
        public string tooltip;
    }

    /// <summary>
    /// Data package passed to GUIManager.cs -> SetShowMe to provide a return event to be called when user returns to calling UI element
    /// </summary>
    public class ShowMeData
    {
        public EventType restoreEvent;
        public int nodeID = -1;             //if > -1 the relevant node is highlighted in ModalState.ShowMe
        public int connID = -1;             //if > -1 the relevant connection is highlighted in ModalState.ShowMe
    }


    /// <summary>
    /// used for storing Dijkstra data in dictOfDijkstra (unweighted path calcs)
    /// </summary>
    public class PathData
    {
        public int[] pathArray;                  //nodeID index, path back to source node via lookup
        public int[] distanceArray;            //nodeID index, distance back to source node assuming unweighted (1 each) connections

        /// <summary>
        /// Constructor called with array size (number of nodes on map)
        /// </summary>
        /// <param name="numOfNodes"></param>
        public PathData(int numOfNodes)
        {
            pathArray = new int[numOfNodes];
            distanceArray = new int[numOfNodes];
        }

    }

    /// <summary>
    /// Nemesis specific data stored by each node on the map at game start to enable nemesis to quickly move to the nearest loiter node without the need for calculations
    /// </summary>
    public class LoiterData
    {
        public int nodeID = -1;                  //nodeID of nearest loiter node, if same as host node then is a loiter node
        public int distance = -1;                //distance to nearest loiter node, if '0' then is a loiter node
        public int neighbourID = -1;             //neighbouring nodeID that forms the shortest route (unweighted) to the nearest loiter node, if same as host node then is a loiter node
    }

    /// <summary>
    /// help tooltip data package
    /// </summary>
    public class HelpData
    {
        public string tag;
        public string header;
        public string text;
    }

    /// <summary>
    /// tracks Resistance player or AI. One entry made every time player moves (eg. per action expended)
    /// </summary>
    public class TrackerRebelMove
    {
        public int turn;
        public int playerNodeID;                    //location at end of move
        public int invisibility;                    //Invisibility at end of move
        public int nemesisNodeID;                   
    }

    /// <summary>
    /// tracks Nemesis. One entry whenever nemesis moves
    /// </summary>
    public class TrackerNemesisMove
    {
        public int turn;
        public int nemesisNodeID;
        public NemesisMode mode;
        public int searchRating;
        public int playerNodeID;
    }

    //new classes above here
}
