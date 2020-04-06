using System.Collections;
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
        public string specialName;                                      //eg. airport name, icon name, etc. Ignore if not relevant.
        public string type;
        public bool isTargetKnown;
        public bool isActiveContact;                                          //specific for current side, at least one active contact with an active parent actor must be present
        public bool isContactKnown;                                     //resistance contact known to authority?
        public bool isTracer;
        /*public bool isTracerActive;*/
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
        public GenericTooltipType tooltipType;

        public GenericTooltipData()
        {
            header = null;
            details = null;
            tooltipType = GenericTooltipType.Any;
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
        public int actorHqID;                       //optional
        public int teamArcID;                       //optional
        public string orgName;                      //optional (Organisation Name)
        public List<Criteria> listOfCriteria;

        public CriteriaDataInput()
        {
            nodeID = -1;
            actorSlotID = -1;
            actorHqID = -1;
            teamArcID = -1;
        }
    }

    /// <summary>
    /// used to pass data into ProcessEffect
    /// </summary>
    public class EffectDataInput
    {
        public GlobalSide side;                                              //used to determine colouring of good/bad effects & condition parameter
        public int ongoingID;                                                //used only if there are going to be ongoing effects, ignore otherwise
        public string ongoingText;                                           //used only if there are going to be ongoing effects, ignore otherwise
        public string originText;                                            //name of thing that caused the effect, eg. gear name
        public int data;                                                     //multipurpose datapoint, eg. gearID
        public string dataName;                                                 //multipurpose data string, eg. organisation name for revealed secret


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
        public bool isAction;                                       //true if effect is considered an action
        public int ongoingID;                                       //only needed if there is an ongoing effect (any value > -1, if '-1' ignore)
        public List<string> listOfHelpTags = new List<string>();    //Effects can returns specific help strings to show in the outcome dialogue. Only the first four are taken into account (UI limit)
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
    [System.Serializable]
    public class EffectDataOngoing
    {
        public int ongoingID = -1;                                        //links back to a central registry to enable cancelling of ongoing effect at a later point
        public string text;
        public string description;                                        //description of effect (used for InfoApp)
        public string nodeTooltip;                                        //descriptor used for node tooltip
        public string reason;                                             //reason why (used for InfoApp)
        public int value;                                                 //how much the field changes, eg. +1, -1, etc.
        public int timer;                                                 //how long does the effect last for?
        public int nodeID = -1;                                           //originating node (used for InfoApp, use only if effect is node based), default -1
        public string effectOutcome;
        public int typeLevel;                                             //(GlobalType.level) benefit, or otherwise, of effect from POV of Resistance
        public string effectApply;
        public int sideLevel;                                             //GlobalSide.level
        

        public EffectDataOngoing()
        {
            timer = GameManager.instance.effectScript.ongoingEffectTimer;
        }
    }


    /// <summary>
    /// used to pass data from Node to NodeTooltip.cs for display of Ongoing effects in the correct colour
    /// </summary>
    public class EffectDataTooltip
    {
        public string text;
        public int typeLevel;                                           //GlobalType.level
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
    [System.Serializable]
    public class ActionAdjustment
    {
        public int sideLevel;                                           //GlobalSide.level
        public string descriptor;                                       //short (two word) text descriptor used in Turn tooltip
        public int value;                                               //change in normal action allocation (use Mathf.ABS value, eg. 1 for both plus and minus)
        public int timer;                                               //number of turns that the effect lasts for (decremented down to zero), set to 999 for continuous (use intended value +1, starts next turn)
        public int turnStart;                                           //turn number where effect commences (added automatically, used for actions tooltip). Ignore.
        public int ongoingID = -1;                                      //ongoing effects only, ignore otherwise, default -1
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
        public string tickerText;                                                                   //ticker text ready news string
        public List<string> listOfNews = new List<string>();                                        //ticker text news string in individual snippets (excludes advert)
        public List<string> listOfAdverts = new List<string>();                                     //ticker text news string ADVERTS in individual snippets                           

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
    [System.Serializable]
    public class ItemData
    {
        public string itemText;                     //what is shown for the item
        public string topText;
        public string bottomText;
        [System.NonSerialized] public Sprite sprite;    //ItemData must have a sprite.
        public string spriteName;                       //used for serialization (store name and access sprite from dictOfSprites on load), ignore otherwise
        public ItemPriority priority;
        public ItemTab tab;
        public int sideLevel;                       //GlobalSide.level
        public MessageType type;                    //main category
        public MessageSubType subType;              //sub type of main category
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
        public bool isDisplay = true;               //toggle to enable player to switch on/off certain message types
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
        public int[] pathArray;                //nodeID index, path back to source node via lookup
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
    [System.Serializable]
    public class LoiterData
    {
        #region Save Data Compatible
        public int nodeID = -1;                  //nodeID of nearest loiter node, if same as host node then is a loiter node
        public int distance = -1;                //distance to nearest loiter node, if '0' then is a loiter node
        public int neighbourID = -1;             //neighbouring nodeID that forms the shortest route (unweighted) to the nearest loiter node, if same as host node then is a loiter node
        #endregion
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

    //
    // - - - History packages
    //

    /// <summary>
    /// tracks Resistance player or AI. One entry made every time player moves (eg. per action expended)
    /// </summary>
    [System.Serializable]
    public class HistoryRebelMove
    {
        public int turn;
        public int playerNodeID;                    //location at end of move
        public int invisibility;                    //Invisibility at end of move
        public int nemesisNodeID;                   
    }

    /// <summary>
    /// tracks Nemesis. One entry whenever nemesis moves
    /// </summary>
    [System.Serializable]
    public class HistoryNemesisMove
    {
        public int turn;
        public int nemesisNodeID;
        public NemesisMode mode;
        public NemesisGoal goal;
        public int targetNodeID;
        public int searchRating;
        public int playerNodeID;
    }

    /// <summary>
    /// Tracks Npc movement. One entry per turn, only while VIP has Active status (OnMap)
    /// </summary>
    [System.Serializable]
    public class HistoryNpcMove
    {
        public int turn;
        public int currentNodeID;
        public int endNodeID;
        public int timer;
    }

    /// <summary>
    /// history of actor's motivational changes
    /// </summary>
    [System.Serializable]
    public class HistoryMotivation
    {
        public int change;              //positive change is good, negative bad
        public int turn;
        public int motivation;          //motivation AFTER then change
        public bool isNormal;           //True if motivation change occured normally, False if negated by actor's compatibility with player
        public string descriptor;       //FORMATTED string, eg. 'Gear HoloPorn Given +1' displayed in green, red if negative, grey if negated
    }

    /// <summary>
    /// history of player's mood changes
    /// </summary>
    [System.Serializable]
    public class HistoryMood
    {
        public int change;              //positive change is good, negative bad
        public int turn;
        public int mood;                //mood AFTER the change
        public string descriptor;       //FORMATTED string, eg. "Disposed of FIXER -2"
        public string factor;           //name of factor that determined the change
        public bool isStressed;         //true if mood has dropped below zero and player gained the Stressed conditoin
    }

    /// <summary>
    /// history of the selected topics (one per turn max)
    /// </summary>
    [System.Serializable]
    public class HistoryTopic
    {
        public int turn;
        public int numSelect;           //number of topicTypes in topicManager.cs -> listOfTopicTypesTurn from which a selection will be made
        public string topicType;        //name of topicType
        public string topicSubType;     //name of topicSubType
        public string topic;            //name of topic
        public string option;           //name of option selected
    }

    //
    // - - - Save / Load packages
    //

    /// <summary>
    /// data class to pass onto topWidget to update with saved data
    /// </summary>
    public class TopWidgetData
    {
        public GlobalSide side;
        public int turn;
        public int actionPoints;
        public int cityLoyalty;
        public int factionSupport;
        public float objectiveOne;
        public float objectiveTwo;
        public float objectiveThree;
        public bool isSecurityFlash;
    }

    /// <summary>
    /// class to pass TurnManager.cs private field data for serialization (to and from)
    /// </summary>
    [System.Serializable]
    public class TurnActionData
    {
        public int turn;
        public int actionsCurrent;
        public int actionsLimit;
        public int actionsAdjust;
        public int actionsTotal;
    }

    /// <summary>
    /// used to save AIRebelManager.cs private fields holding dynamic data
    /// </summary>
    [System.Serializable]
    public class SaveAIRebelClass
    {
        public int actionAllowance;
        public int actionsUsed;
        public int gearPool;
        public int gearPointsUsed;
        public int targetIntel;
        public int targetIntelUsed;
        public int targetNodeID;
        public int cureNodeID;
        public int aiPlayerStartNodeID;
        public bool isConnectionsChanged;
        public bool isPlayer;
        public bool isCureNeeded;
        public bool isCureCritical;
        public bool isPlayerStressed;
        public int stressedActorID;
        public int questionableID;
    }

    /// <summary>
    /// Used to save AIManager.cs private fields
    /// </summary>
    [System.Serializable]
    public class SaveAIClass
    {
        public bool isStressed;
        public bool isLowHQApproval;
        public int stressedActorID;
        //ai countermeasure flags
        public bool isOffline;
        public bool isTraceBack;
        public bool isScreamer;
        public bool isPolicy;
        //ai countermeasures
        public int timerTraceBack;
        public int aiSecurityProtocolLevel;
        public int timerScreamer;
        public int timerOffline;
        public int timerHandout;
        public int timerPolicy;
        public string policyName;
        public string policyTag;
        public int policyEffectCrisis;
        public int policyEffectLoyalty;
        //hacking
        public int detectModifierMayor;
        public int detectModifierFaction;
        public int detectModifierGear;
        //factions
        public string authorityPreferredArc;         
        public int actionsPerTurn;
        //player target (Nemesis / Erasure teams)
        public int playerTargetNodeID;
        //decision data
        public float connSecRatio;
        public float teamRatio;
        public int erasureTeamsOnMap;
        public bool isInsufficientResources;
        public int numOfUnsuccessfulResourceRequests;
        public int numOfSuccessfulResourceRequests;
    }

    /// <summary>
    /// Used to save NemesisManager.cs private fields
    /// </summary>
    [System.Serializable]
    public class NemesisSaveClass
    {
        public bool hasMoved;
        public bool hasActed;
        public bool hasWarning;
        public bool isFirstNemesis;

        //Resistance Player
        public SideState resistancePlayer;

        //Nemesis AI
        public NemesisMode mode;
        public NemesisGoal goal;
        public int durationGoal;
        public int durationDelay;
        public int nemesisNodeID;
        public AITracker trackerDebug;

        //player tracking info
        public int targetNodeID;
        public int moveToNodeID;
        public int targetDistance;
        public bool isImmediate;

        //Authority player control
        public bool isPlayerControl;
        public int controlNodeID;
        public int controlTimer;
        public int controlCooldownTimer;
        public NemesisGoal controlGoal;
    }

    //
    // - - - AIManager.cs
    //

    /// <summary>
    /// AI data package used to collate all node info where a node has degraded in some form
    /// </summary>
    public class AINodeData
    {
        public int nodeID;
        public int score;                       //used by spider & others to give the node a rating. Higher the score, the more likely it is to be chosen
        public NodeData type;
        public NodeArc arc;
        public int difference;                  //shows difference between current and start values
        public int current;                     //shows current value
        public bool isPreferred;                //true if preferred type of node for that side's faction
    }

    /// <summary>
    /// AI data package detailing an Authority task that is ready to be executed next turn
    /// </summary>
    [System.Serializable]
    public class AITask
    {
        public int taskID;                     //automatically assigned
        public int data0;                      //could be node, connection ID or teamID
        public int data1;                      //teamArcID, decision cost in resources
        public int data2;                      //gearPoolTopUp etc.
        public string dataName;                //aiDecision.name if a decision, otherwise ignored
        public string name0;                   //node arc name, aiDecision.tag
        public string name1;                   //could be target or team arc name, eg. 'CIVIL'
        public Priority priority;
        public AITaskType type;                //what type of task
        public int chance;                     //dynamically added by ProcessTasksFinal (for display to player of % chance of this task being chosen)

        public AITask()
        { taskID = GameManager.instance.aiScript.aiTaskCounter++; }
    }

    /// <summary>
    /// extracted data from AI messages (at time of AI becoming aware of them)
    /// </summary>
    [System.Serializable]
    public class AITracker
    {
        public int data0;                       //node or connectionID
        public int data1;                       //optional, can ignore, contact effectiveness for nemesis sightings
        public int data2;                       //optional, can ignore, moveNumber for nemesis making multiple moves within a single turn
        public int turn;                        //turn occurred

        public AITracker(int data, int turn)    //NOTE: Constructor doesn't include data1 (default -1), add manually if required
        { data0 = data; data1 = -1; data2 = 0; this.turn = turn; }
    }

    /// <summary>
    /// data package to populate AIDisplayUI
    /// </summary>
    public class AIDisplayData
    {
        public int rebootTimer;                 //AIDisplayUI will only open (allow hacking attempts) if timer = 0 (which infers that isRebooting = false)
        public string task_1_textUpper;
        public string task_1_textLower;
        public string task_1_chance;
        public string task_1_tooltipMain;
        public string task_1_tooltipDetails;
        public string task_2_textUpper;
        public string task_2_textLower;
        public string task_2_chance;
        public string task_2_tooltipMain;
        public string task_2_tooltipDetails;
        public string task_3_textUpper;
        public string task_3_textLower;
        public string task_3_chance;
        public string task_3_tooltipMain;
        public string task_3_tooltipDetails;
        public string aiDetails;
        public string renownDecision;
        public int nodeID_1;                   //used for highlighting node or connection referred to by task
        public int connID_1;
        public int nodeID_2;
        public int connID_2;
        public int nodeID_3;
        public int connID_3;

        public AIDisplayData()
        {
            task_1_textUpper = ""; task_1_textLower = ""; task_1_chance = ""; task_1_tooltipMain = ""; task_1_tooltipDetails = "";
            task_2_textUpper = ""; task_2_textLower = ""; task_2_chance = ""; task_2_tooltipMain = ""; task_2_tooltipDetails = "";
            task_3_textUpper = ""; task_3_textLower = ""; task_3_chance = ""; task_3_tooltipMain = ""; task_3_tooltipDetails = "";
            nodeID_1 = -1; nodeID_2 = -1; nodeID_3 = -1;
            connID_1 = -1; connID_2 = -1; connID_3 = -1;
        }

    }

    /// <summary>
    /// data package to populate AIDisplayUI (needs to be separate because it's dynamic data whereas AIDisplayData is sent once during AIEndOfTurn)
    /// </summary>
    public class AIHackingData
    {
        public string hackingStatus;            //combined string of AI Alert Status and number of hacking attempts
        public string tooltipHeader;
        public string tooltipMain;              //combined string for tooltip
        public string tooltipDetails;
    }

    /// <summary>
    /// data package to populate AISideTabUI
    /// </summary>
    public class AISideTabData
    {
        public string topText;              //eg. 'A.I' but colour formatted
        public string bottomText;           //eg. Renown cost to hack or 'X' if not possible, greyed out if not enough renown
        public string tooltipMain;
        public string tooltipDetails;
        public HackingStatus status;        //used to determine what happens when player clicks AI Side Tab UI
    }

    /// <summary>
    /// dynamic data packages for TopicType and TopicSubType
    /// </summary>
    [System.Serializable]
    public class TopicTypeData
    {
        public string type;                         //SO.name of TopicType/SubType that the package refers to 
        public string parent;                       //SO.name of TopicType that the package refers to (used for SubTypes to indicate which type they belong to)
        public bool isAvailable;                    //True if the TopicSubType has decisions available, false if not (determined at level start)
        public int turnLastUsed;                    //turn # when last used
        public int minInterval;                     //minimum turn interval that must elapse prior to the next occurrence
        //stats
        public int timesUsedLevel;                  //tally of times used (for this level)
        public int timesUsedCampaign;               //cumulative total for campaign
    }

    /// <summary>
    /// every Node action by player or actor generates a node action data package
    /// NOTE: Team auto time out are covered by TeamActionData whereas actor Team Deploy / Team Recall is covered by NodeActionData
    /// </summary>
    [System.Serializable]
    public class NodeActionData
    {
        public int turn;
        public int actorID;
        public int nodeID;
        public int teamID;                          //specific to authority player, ignore otherwise
        public string dataName;                     //general purpose data, eg. if recruited actor then name of actor recruited, name of gear gained, etc. Can be ignored
        public NodeAction nodeAction;               //type of nodeAction (compulsory)
    }

    /// <summary>
    /// every Team action (when team autotimes out and is recalled) by Authority Actor generates a team action data package
    /// NOTE: Team deploy and recall early actions by actors are covered by NodeActionData
    /// </summary>
    [System.Serializable]
    public class TeamActionData
    {
        public int turn;
        public int actorID;
        public int nodeID;
        public int teamID;                          
        public string dataName;                     //general purpose data, eg. if recruited actor then name of actor recruited, name of gear gained, etc. Can be ignored
        public TeamAction teamAction;               //type of team Action (compulsory)
    }

    /// <summary>
    /// topic UI initialisation data
    /// </summary>
    public class TopicUIData
    {
        public string topicName;
        public string header;
        public string text;
        public string ignoreTooltipHeader;                                  //ignore button tooltip 
        public string ignoreTooltipMain;                                    //ignore button tooltip
        public string ignoreTooltipDetails;                                 //ignore button tooltip
        public string imageTooltipHeader;                                   //image button tooltip 
        public string imageTooltipMain;                                     //image button tooltip
        public string imageTooltipDetails;                                  //image button tooltip
        public string bossTooltipHeader;                                    //boss tooltip
        public string bossTooltipMain;                                      //boss tooltip
        public string bossTooltipDetails;                                   //boss tooltip
        public int nodeID;                                                  //needed to toggle 'ShowMe' button, ignore if not relevant
        public bool isBoss;                                                 //True if HQ boss present (image + tooltip opinion)
        public Color colour;                                                 //background colour (set to default blue in constructor)
        public Sprite spriteMain;                                           //Sprite for Main topic 
        public Sprite spriteBoss;                                           //Sprite for Boss (if 'isBoss' true, ignore otherwise)
        public List<TopicOption> listOfOptions = new List<TopicOption>();
        public List<Effect> listOfIgnoreEffects = new List<Effect>();
        public List<string> listOfHelp = new List<string>();                //list of help tags for, optional, second help mouseover down the bottom (not shown if listOfHelp is empty)

        public TopicUIData()
        { colour = GameManager.instance.guiScript.colourTopicNormal; }
    }

    /// <summary>
    /// used to pass topic data across to MessageManager.cs
    /// </summary>
    public class TopicMessageData
    {
        public string topicName;
        public string optionName;
        public Sprite sprite;
        public string spriteName;
        public string text;
        public int actorID;
        public int nodeID;
        public string outcome;
    }

    /// <summary>
    /// data passed from TopicManager.cs -> EffectManager.cs to customise topic effect outcomes
    /// </summary>
    public class TopicEffectData
    {
        public int actorID;
        public int actorOtherID;           //other actor in a dual actor effect, eg. relationship
        public bool isHqActors;            //if true, actors are from HQ, default false
        public int nodeID;
        public int teamID;
        public int contactID;
        public string secret;               //name of secret, not tag
        public string orgName;              //name of organisation, not tag, eg. Blue Angel Cult
        public string investigationRef;     //reference name of investigation
        public string gearName;             //name of gear, not tag
        public ActorRelationship relation;   //used for ActorTopic relationships
    }

    /// <summary>
    /// news snippet used for ticker tape newsfeed on mainInfoApp
    /// </summary>
    [System.Serializable]
    public class NewsItem
    {
        public int timer;                                                   //countdown timer before item is removed from selection pool
        public string text;                                                 //unformatted text

        public NewsItem()
        {
            timer = GameManager.instance.newsScript.timerMaxItemTurns;
        }
    }

    /// <summary>
    /// Used to pass data to CheckNewsText
    /// </summary>
    public class CheckTextData
    {
        public string text;                                                 //text to check
        public bool isValidate = false;                                     //if true, validation check only for correct text tags being present, default false for normal text tag replacement operation      
        public string objectName = "Unknown";                               //name of object passing text, used for error messages
        public Node node;                                                   //nodeID if relevant (ignore otherwise)
        public int actorID;                                                 //actor, if relevant (ignore otherwise)

    }

    /// <summary>
    /// used to pass data to ActiveEffect message
    /// NOTE: Handles actor and node details automatically
    /// NOTE: Need to provide colour for test. Auto bold for top and bottom
    /// </summary>
    public class ActiveEffectData
    {
        public string text;                                                 //doubles up as item.text
        public string topText;
        public string detailsTop;
        public string detailsBottom;
        public Sprite sprite;
        public int actorID = -1;
        public Node node = null;
        public Condition condition = null;
        public string help0;                                                //NOTE: help not needed for condition, EffectManager.cs -> SetConditionHelp handles this
        public string help1;
        public string help2;
        public string help3;
    }


    /// <summary>
    /// used to track services provided by Organisations to the Player during a level
    /// </summary>
    [System.Serializable]
    public class OrgData
    {
        public string text;
        public int turn;
    }

    /// <summary>
    /// Used for DataManager.cs -> dictOfRelationships to track relationships between subordinate actors
    /// </summary>
    [System.Serializable]
    public class RelationshipData
    {
        public int slotID;                                                  //slotID of actor with whom the current actor has a relationship with
        public int actorID;                                                 //actorID of actor with whom the current actor has a relationship with
        public int timer;                                                   //relationships can't be changed while timer > 0
        public ActorRelationship relationship;                              //friend or enemy? (default 'none')
        public int[] arrayOfCompatibility;                                  //compatibility with other actors, index corresponds to slotID, default values 0

        public RelationshipData()
        {
            slotID = -1;
            actorID = -1;
            relationship = ActorRelationship.None;
            timer = GameManager.instance.actorScript.timerRelations;
            arrayOfCompatibility = new int[GameManager.instance.actorScript.maxNumOfOnMapActors];
        }
    }

    /// <summary>
    /// Used to pass selected actors for an Actor Politics topic back to TopicManager.cs -> GetActorPoliticsTopics
    /// </summary>
    public class RelationSelectData
    {
        public int actorFirstID;            //actorID
        public int actorSecondID;           //actorID
        public int compatibility;

        public RelationSelectData()
        {
            actorFirstID = -1;
            actorSecondID = -1;
        }
    }

    /// <summary>
    /// Tracks commendations and blackmarks
    /// </summary>
    [System.Serializable]
    public class AwardData
    {
        public int turn;
        public string reason;
    }

    /// <summary>
    /// Tracks anything that changes an HQ actors renown
    /// </summary>
    [System.Serializable]
    public class HqRenownData
    {
        public int turn;
        public int change;                  //amount renown is changing by
        public int newRenown;               //what renown is after change
        public string reason;
    }

    //new classes above here
}
