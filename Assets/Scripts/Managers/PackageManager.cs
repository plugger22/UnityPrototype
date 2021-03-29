using gameAPI;
using System;
using System.Collections.Generic;
using UnityEngine;

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
        public Trait trait;
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
        public StoryType storyType;                 //optional (Story Topics)
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
        public EffectSource source;                                          //source of the effect, eg. target, topic, gear etc.
        public int ongoingID;                                                //used only if there are going to be ongoing effects, ignore otherwise
        public string ongoingText;                                           //used only if there are going to be ongoing effects, ignore otherwise
        public string originText;                                            //name of thing that caused the effect, eg. gear name
        public int data0;                                                    //multipurpose datapoint   NOTE: be careful when using data0/1 that you keep them separate and don't overwrite
        public int data1;                                                    //multipurpose datapoint
        public int dataSpecial;                                              //multipurpose datapoint, used for passing enums, eg. StoryType
        public string dataName;                                              //multipurpose data string, eg. organisation name for revealed secret
        public TutorialQueryType queryType;                                  //What type of Tutorial Query is it? Only required for tutorial Options, ignore otherwise


        public EffectDataInput()
        {
            ongoingID = -1; ongoingText = "";
            side = GameManager.i.sideScript.PlayerSide;
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
        public bool isLargeText;                                    //true if you want the effect to be shown in larger font size (115%), default false. Optional
        public int ongoingID;                                       //only needed if there is an ongoing effect (any value > -1, if '-1' ignore)
        public List<string> listOfHelpTags = new List<string>();    //Effects can returns specific help strings to show in the outcome dialogue. Only the first four are taken into account (UI limit)
        public List<Node> listOfNodes = new List<Node>();           //optional -> for 'Show Me' purposes in ModalOutcome
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
        public bool isLargeText;                                    //true if you want the effect to be shown in larger font size (110%), default false. Optional
        public List<Node> listOfNodes = new List<Node>();           //optional -> for 'Show Me' purposes in ModalOutcome
    }

    /// <summary>
    /// sends a package of data to the node for processing of one off or ongoing effects on a node field
    /// ProcessEffect -> subMethod -> EffectDataProcess -> Node.cs -> ProcessNodeEffect(EffectDataProcess)
    /// </summary>
    public class EffectDataProcess
    {
        public EffectOutcome outcome;
        public EffectDataOngoing effectOngoing;                        //only used if an ongoing effect, ignore otherwise 
        public int value;                                                   //how much the field changes, eg. +1, -1, etc.
        public string text;                                                 //tooltip description for the temporary effect

        public EffectDataProcess()
        { effectOngoing = null; }
    }

    /// <summary>
    /// used to pass data back to Node for an ongoing effect
    /// </summary>
    [System.Serializable]
    public class EffectDataOngoing
    {
        public int ongoingID;                                        //links back to a central registry to enable cancelling of ongoing effect at a later point
        public string text;
        public string description;                                        //description of effect (used for InfoApp)
        public string nodeTooltip;                                        //descriptor used for node tooltip
        public string reason;                                             //reason why (used for InfoApp)
        public int value;                                                 //how much the field changes, eg. +1, -1, etc.
        public int timer;                                                 //how long does the effect last for?
        public int nodeID;                                           //originating node (used for InfoApp, use only if effect is node based), default -1
        public string effectOutcome;
        public int typeLevel;                                             //(GlobalType.level) benefit, or otherwise, of effect from POV of Resistance
        public string effectApply;
        public int sideLevel;                                             //GlobalSide.level


        public EffectDataOngoing()
        {
            nodeID = -1;
            ongoingID = -1;
            timer = GameManager.i.effectScript.ongoingEffectTimer;
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
        public bool isChangeInvisibility;
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
        public int ongoingID;                                      //ongoing effects only, ignore otherwise, default -1

        public ActionAdjustment()
        { ongoingID = -1; }
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
        public int help = -1;                       //key to dictOfHelp for info button down at bottom (can ignore) -> will display help button if present (make sure tag's ae set below for specific topics
        public string tag0;                         //help topic, provide tag or leave Null if none. NOTE: add help topics in sequence -> 0 / 1 / 2 / 3, and make sure help > 0 
        public string tag1;
        public string tag2;
        public string tag3;
        public bool isDisplay;               //toggle to enable player to switch on/off certain message types

        public ItemData()
        { isDisplay = true; }
    }

    /// <summary>
    /// MetaInfoData package for MetaGameUI
    /// </summary>
    public class MetaInfoData
    {
        //NOTE: Don't forget to add any new collections/fields to 'Reset'
        public List<MetaData>[] arrayOfMetaData = new List<MetaData>[(int)MetaTabSide.Count];           //array of lists, one per MetaGameUI.cs tab excluding 'help'
        public List<MetaData> listOfStatusData = new List<MetaData>();                                  //includes all metaData with metaOption.isPlayerStatus true
        public List<MetaData> listOfRecommended = new List<MetaData>();                                             //all 'isRecommended' true metaOptions sorted by priority
        public MetaData selectedDefault = new MetaData();                                               //default metaData option for Selected tab

        public MetaInfoData()
        {
            //initialise arrayOfLists
            for (int i = 0; i < (int)MetaTabSide.Count; i++)
            { arrayOfMetaData[i] = new List<MetaData>(); }
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="dataCopy"></param>
        public MetaInfoData(MetaInfoData dataCopy)
        {
            for (int i = 0; i < (int)MetaTabSide.Count; i++)
            { arrayOfMetaData[i] = new List<MetaData>(dataCopy.arrayOfMetaData[i]); }
            listOfStatusData.AddRange(dataCopy.listOfStatusData);
            listOfRecommended.AddRange(dataCopy.listOfRecommended);
            selectedDefault = dataCopy.selectedDefault;
        }

        /// <summary>
        /// Empties out all data
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < (int)MetaTabSide.Count; i++)
            { arrayOfMetaData[i].Clear(); }
            listOfStatusData.Clear();
            listOfRecommended.Clear();
            selectedDefault = new MetaData();
        }

        /// <summary>
        /// Add metaData to appropriate list
        /// </summary>
        /// <param name="metaData"></param>
        public void AddMetaData(MetaData metaData)
        {
            if (metaData != null)
            { arrayOfMetaData[(int)metaData.tabSide].Add(metaData); }
            else { Debug.LogWarning("Invalid metaData (Null)"); }
        }
    }

    /// <summary>
    /// ItemData equivalent for MetaGameUI
    /// </summary>
    public class MetaData
    {
        public string metaName;                     //metaOption.name for reference and debugging purposes
        public string itemText;                     //what is shown for the item
        public string textSelect;                   //top text RHS if not yet selected ("Cost 2 Power")
        public string textDeselect;                 //top text RHS if already selected and can be deselected ("Gain 2 Power")
        public string textInsufficient;             //top text RHS if can't afford the power required ("Not enough power (need 2)")
        public string bottomText;
        public string inactiveText;                 //text shown if option isActive.False
        public Sprite sprite;                       //ItemData must have a sprite.
        public string spriteName;                       //used for serialization (store name and access sprite from dictOfSprites on load), ignore otherwise
        public MetaPriority priority;
        public MetaTabSide tabSide;                 //specify a side or top tab
        public MetaTabTop tabTop;                   //specify a side or top tab
        public int powerCost;                      //cost for option based on priority
        public int sideLevel;                       //GlobalSide.level
        public bool isActive;                       //displayed greyed out if not (for metaOption.isAlways = true)
        public bool isRecommended;                  //if true part of recommended selection of options
        public bool isCriteria;                     //true if any criteria involved, false otherwise
        public bool isSelected;                     //used within MetaGameUI (if true has been selected by player)
        public bool isPowerGain;                   //true if there is a power GAIN, not cost (gain equal to 'powerCost')
        public MetaPriority recommendedPriority;    //recommendations selected on priority until power runs out
        public List<Effect> listOfEffects;          //effects that happen as a result of metaData being selected
        public int data;                            //used to iplement outcome (where a number is needed) 
        public string dataName;                     //used to implement outcome (where a name is needed)
        public string dataTag;                      //used to implement outcome (optional, eg. name of a secret/investigation/organisation)
        public int help;                            //key to dictOfHelp for info button down at bottom (can ignore) -> will display help button if present (make sure tag's ae set below for specific topics
        public string tag0;                         //help topic, provide tag or leave Null if none. NOTE: add help topics in sequence -> 0 / 1 / 2 / 3, and make sure help > 0 
        public string tag1;
        public string tag2;
        public string tag3;

        public MetaData()
        {
            listOfEffects = new List<Effect>();
            help = -1;
            data = -1;
        }
    }

    /// <summary>
    /// Used to return data from ActorManager.cs -> GetManagePowerCost for extra costs incurred due to actor knowing secrets and/ or threatening player
    /// </summary>
    public class ManagePowerCost
    {
        public int powerCost;
        public string tooltip;
    }

    /// <summary>
    /// Data package passed to GUIManager.cs -> SetShowMe to provide a return event to be called when user returns to calling UI element
    /// </summary>
    public class ShowMeData
    {
        public EventType restoreEvent;
        public int nodeID;                      //if > -1 the relevant node is highlighted in ModalState.ShowMe
        public int connID;                      //if > -1 the relevant connection is highlighted in ModalState.ShowMe
        public List<Node> listOfNodes;          //optional -> if provided will override the single nodeID 
        //only if underlying UI Element
        public EventType hideOtherEvent;        //optional -> used only when there is an underlying UI element, eg. outcome window on top of a Gear Inventory, hides underlying UI element
        public EventType restoreOtherEvent;     //optional -> used only when there is an underlying UI element -> handles restoring of underlying UI element

        public ShowMeData()
        {
            nodeID = -1;
            connID = -1;
            listOfNodes = new List<Node>();
        }
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
        public string nodeName;
        public bool isHighlight;                    //if true record is shown in yellow highlight in ModalTabbedUI.cs -> Move History. Use for starting locations at each level
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
    /// history of actor's opinion changes
    /// </summary>
    [System.Serializable]
    public class HistoryOpinion
    {
        public int change;              //positive change is good, negative bad
        public int turn;
        public int opinion;             //opinion AFTER then change
        public int scenarioIndex;
        public bool isNormal;           //True if opinion change occured normally, False if negated by actor's compatibility with player
        public string descriptor;       //FORMATTED string, eg. 'Gear HoloPorn Given +1' displayed in green, red if negative, grey if negated

        public HistoryOpinion()
        {
            scenarioIndex = GameManager.i.campaignScript.GetScenarioIndex();
            turn = GameManager.i.turnScript.Turn;
        }
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
        public int scenarioIndex;
        public string descriptor;       //FORMATTED string, eg. "Disposed of FIXER -2"
        public string factor;           //name of factor that determined the change (If no factor applies then supply a blank string "" instead)
        public bool isStressed;         //true if mood has dropped below zero and player gained the Stressed condition
        public bool isHighlight;        //if true item shown as a different highlight colour (eg. start of a new level entry), default false

        public HistoryMood()
        {
            scenarioIndex = GameManager.i.campaignScript.GetScenarioIndex();
            turn = GameManager.i.turnScript.Turn;
        }
    }

    /// <summary>
    /// history of the selected topics (one per turn max)
    /// </summary>
    [Serializable]
    public class HistoryTopic
    {
        public int turn;
        public int numSelect;           //number of topicTypes in topicManager.cs -> listOfTopicTypesTurn from which a selection will be made
        public string topicType;        //name of topicType
        public string topicSubType;     //name of topicSubType
        public string topic;            //name of topic
        public string option;           //name of option selected
    }

    /// <summary>
    /// history of major actor and player events. NOTE: turn, city  and cityTag are handled automatically by the constructor, you only need to add text
    /// </summary>
    [Serializable]
    public class HistoryActor
    {
        public string text;             //what happened
        public bool isHighlight;        //if true item shown as a different highlight colour (eg. start of a new level entry), default false
        //automatic
        public int turn;
        public int scenarioIndex;
        public string city;             //search city name, eg. 'NewYork', done automatically in constructor
        public string cityTag;          //display city name, eg. 'New York', onde automatically in constructor
        public string district;         //district name, optional

        public HistoryActor()
        {
            //bypass if load at start
            if (GameManager.i.inputScript.GameState != GameState.LoadAtStart)
            {
                turn = GameManager.i.turnScript.Turn;
                scenarioIndex = GameManager.i.campaignScript.GetScenarioIndex();
                city = GameManager.i.cityScript.GetCity().name;
                cityTag = GameManager.i.cityScript.GetCityName();
            }
        }
    }

    /// <summary>
    /// History of your relationships with MegaCorps
    /// </summary>
    [Serializable]
    public class HistoryMegaCorp
    {
        public MegaCorpType megaCorp;
        public string megaCorpName;
        public int change;                      //amount relationship changed by
        public int relationshipNow;             //relationship level AFTER change
        public string text;                     //reason why
        public bool isHighlight;                //displays history in yellow highlight (used for initialisation records, eg. 'Starting Relationship')
        //automatic
        public int turn;
        public string city;                     //city name, eg. 'NewYork'
        public string cityTag;                  //city tag

        public HistoryMegaCorp()
        {
            //bypass if load at start
            if (GameManager.i.inputScript.GameState != GameState.LoadAtStart)
            {
                turn = GameManager.i.turnScript.Turn;
                city = GameManager.i.cityScript.GetCity().name;
                cityTag = GameManager.i.cityScript.GetCityName();
            }
        }
    }


    /// <summary>
    /// History of a level (once completed)
    /// </summary>
    [Serializable]
    public class HistoryLevel
    {
        //base data
        public int scenarioIndex;
        public string scenarioDescriptor;
        public string cityName;
        //start data
        public int cityLoyaltyStart;
        public int hqApprovalStart;
        //end data
        public int turns;                       //number of turns spent in level
        public int cityLoyaltyEnd;
        public int hqApprovalEnd;
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
        { taskID = GameManager.i.aiScript.aiTaskCounter++; }
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
        public string powerDecision;
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
        public string bottomText;           //eg. Power cost to hack or 'X' if not possible, greyed out if not enough power
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
    /// topic UI initialisation data (Decision topics)
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
        public TopicDecisionType uiType;                                    //Type of decision, default 'Normal'
        public TopicBase baseType;                                          //Type of base UI, default 'Normal'
        public Color colour;                                                //background colour (set to default blue in constructor)
        public Sprite spriteMain;                                           //Sprite for Main topic 
        public Sprite spriteBoss;                                           //Sprite for Boss (if 'isBoss' true, ignore otherwise)
        public List<TopicOption> listOfOptions = new List<TopicOption>();
        public List<Effect> listOfIgnoreEffects = new List<Effect>();
        public List<StoryHelp> listOfStoryHelp = new List<StoryHelp>();     //list of story Help. Optional
        public List<string> listOfHelp = new List<string>();                //list of help tags for, optional, second help mouseover down the bottom (not shown if listOfHelp is empty)

        public TopicUIData()
        { colour = GameManager.i.uiScript.TopicNormal; }

        public void Reset()
        {
            listOfOptions.Clear();
            listOfIgnoreEffects.Clear();
            listOfStoryHelp.Clear();
            listOfHelp.Clear();
        }
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
        public int actorOtherID;            //other actor in a dual actor effect, eg. relationship
        public bool isHqActors;             //if true, actors are from HQ, default false
        public int nodeID;
        public int teamID;
        public int contactID;
        public string secret;               //name of secret, not tag
        public string orgName;              //name of organisation, not tag, eg. Blue Angel Cult
        public string investigationRef;     //reference name of investigation
        public string gearName;             //name of gear, not tag
        public ActorRelationship relation;  //used for ActorTopic relationships
        public Target target;               //used for story targets
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
            timer = GameManager.i.newsScript.timerMaxItemTurns;
        }
    }

    /// <summary>
    /// Used to pass data to CheckNewsText
    /// </summary>
    public class CheckTextData
    {
        public string text;                                                 //text to check
        public bool isValidate = false;                                     //if true, validation check only for correct text tags being present, default false for normal text tag replacement operation      
        public string objectName;                                           //name of object passing text, used for error messages
        public Node node;                                                   //nodeID if relevant (ignore otherwise)
        public int actorID;                                                 //actor, if relevant (ignore otherwise)

        public CheckTextData()
        { objectName = "Unknown"; }
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
        public int actorID;
        public Node node = null;
        public Condition condition = null;
        public string help0;                                                //NOTE: help not needed for condition, EffectManager.cs -> SetConditionHelp handles this
        public string help1;
        public string help2;
        public string help3;

        public ActiveEffectData()
        { actorID = -1; }
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
            timer = GameManager.i.actorScript.timerRelations;
            arrayOfCompatibility = new int[GameManager.i.actorScript.maxNumOfOnMapActors];
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
    /// Tracks anything that changes an HQ actors power
    /// </summary>
    [System.Serializable]
    public class HqPowerData
    {
        public int turn;
        public int scenarioIndex;           //taken care of automatically in constructor
        public int change;                  //amount power is changing by
        public int newPower;               //what power is after change
        public string reason;               //if a metaGame event then 'due to ...'

        public HqPowerData()
        { scenarioIndex = GameManager.i.campaignScript.GetScenarioIndex(); }
    }

    /// <summary>
    /// Option set derived from MetaGame decisions that drive the new level's lineup of OnMap actors
    /// </summary>
    [System.Serializable]
    public class MetaGameOptions
    {
        public bool isDismissed;            //listOfActorsDismissed: true -> included, false -> excluded          
        public bool isResigned;             //listOfActorsResigned: true -> included, false -> excluded
        public bool isLowOpinion;            //true -> includes any actor, false -> excludes any actor with opinion < 2
        public bool isTraitor;              //true -> include traitorous actors, actor.isTraitor, false -> exclude
        public bool isLevelTwo;             //true -> all actors will be level 2, or better, false -> base pool of actors will be level 1
    }

    /// <summary>
    /// data package passed to EffectManager.cs for MetaGame effects
    /// </summary>
    public class MetaEffectData
    {
        public string metaOptionName;
    }

    /// <summary>
    /// Basic class to record time by turn and scenario
    /// </summary>
    [System.Serializable]
    public class TimeStamp
    {
        public int turn;
        public int scenario;                        //scenario index

        public TimeStamp()
        { turn = -1; scenario = -1; }
    }

    /// <summary>
    /// Data package for PopUpDynamic
    /// </summary>
    public class PopUpDynamicData
    {
        public Vector3 position;                    //objectOfInterest.transform.position
        public int x_offset;
        public int y_offset;
        public string text;                         //text to display
    }

    /// <summary>
    /// Transition UI initialisation data package
    /// </summary>
    public class TransitionInfoData
    {
        //
        // - - - Main
        //

        #region EndLevel
        //
        // - - - End Level
        //
        public string objectiveStatus;
        public List<EndLevelData> listOfEndLevelData = new List<EndLevelData>();
        #endregion

        #region HQ Status
        //
        // - - - HQ Status
        //
        //NOTE: indexes of all relevant lists refer to the same actor
        public List<Sprite> listOfHqSprites = new List<Sprite>();
        public List<string> listOfHqPower = new List<string>();
        public List<string> listOfHqTitles = new List<string>();
        public List<TooltipData> listOfHqTooltips = new List<TooltipData>();
        public List<Sprite> listOfWorkerSprites = new List<Sprite>();
        public List<string> listOfWorkerPower = new List<string>();
        public List<string> listOfWorkerArcs = new List<string>();
        public List<TooltipData> listOfWorkerTooltips = new List<TooltipData>();
        public List<string> listOfHqEvents = new List<string>();
        #endregion

        #region playerStatus
        //
        // - - - Player Status
        //
        public string playerStatus;
        #endregion

        #region Briefing One
        //
        // - - - BriefingOne
        //
        public string briefingOne;
        #endregion

        #region Briefing Two
        //
        // - - - BriefingTwo
        //
        public string briefingTwo;
        #endregion

        /// <summary>
        /// default constructor
        /// </summary>
        public TransitionInfoData()
        { }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="data"></param>
        public TransitionInfoData(TransitionInfoData data)
        {
            if (data != null)
            {
                #region EndLevel
                objectiveStatus = data.objectiveStatus;
                listOfEndLevelData.AddRange(data.listOfEndLevelData);
                #endregion

                #region HQ Status
                listOfHqSprites.AddRange(data.listOfHqSprites);
                listOfHqPower.AddRange(data.listOfHqPower);
                listOfHqTitles.AddRange(data.listOfHqTitles);
                listOfHqTooltips.AddRange(data.listOfHqTooltips);
                listOfWorkerSprites.AddRange(data.listOfWorkerSprites);
                listOfWorkerPower.AddRange(data.listOfWorkerPower);
                listOfWorkerArcs.AddRange(data.listOfWorkerArcs);
                listOfWorkerTooltips.AddRange(data.listOfWorkerTooltips);
                listOfHqEvents.AddRange(data.listOfHqEvents);
                #endregion

                #region Player Status
                playerStatus = data.playerStatus;
                #endregion

                #region Briefing One
                briefingOne = data.briefingOne;
                #endregion

                #region Briefing Two
                briefingTwo = data.briefingTwo;
                #endregion
            }
            else { Debug.LogError("Invalid transitionInfoData parameter (Null)"); }
        }

        /// <summary>
        /// Full data reset
        /// </summary>
        public void Reset()
        {
            #region Main

            #endregion

            #region End Level
            objectiveStatus = "";
            listOfEndLevelData.Clear();
            #endregion

            #region HQ status
            listOfHqSprites.Clear();
            listOfHqPower.Clear();
            listOfHqTitles.Clear();
            listOfHqTooltips.Clear();
            listOfWorkerSprites.Clear();
            listOfWorkerPower.Clear();
            listOfWorkerArcs.Clear();
            listOfWorkerTooltips.Clear();
            listOfHqEvents.Clear();
            #endregion

            #region Player Status
            playerStatus = "";
            #endregion

            #region Briefing One
            briefingOne = "";
            #endregion

            #region Briefing Two
            briefingTwo = "";
            #endregion
        }

    }

    /// <summary>
    /// Subclass to provide endLevel data for EndLevel page of TransitionUI
    /// </summary>
    [System.Serializable]
    public class EndLevelData
    {
        public string assessmentText;                               //Assessment text
        public EndlLevelMedal medal;                                //type of medal awarded
        public int power;                                          //power given
        public TooltipData tooltipPortrait = new TooltipData();     //Hq portrait tooltip data
        public TooltipData tooltipMedal = new TooltipData();        //Medal tooltip data

    }


    /// <summary>
    /// Global class to pass generic tooltip details
    /// </summary>
    [System.Serializable]
    public class TooltipData
    {
        public string header;
        public string main;
        public string details;

        //default constructor
        public TooltipData()
        { }

        //copy constructor
        public TooltipData(TooltipData data)
        {
            header = data.header;
            main = data.main;
            details = data.details;
        }
    }

    /// <summary>
    /// Used to pass saved Game state to ControlManager.cs -> ProcessLoadGame via FileManager.cs -> LoadSaveData
    /// </summary>
    public class LoadGameState
    {
        public GameState gameState;
        public RestorePoint restorePoint;
    }

    /// <summary>
    /// Data package for different types of 'Cars' (aerial traffic background animations)
    /// </summary>
    public class CarData
    {
        public float cruiseAltitude;
        public float verticalSpeed;
        public float horizontalSpeed;
        public float hoverDelay;
        public float sirenFlashInterval;
        public bool isSiren;
        //specific to Surveillance (surveilCar, ignore for rest)
        public float surveilAltitude;
        public float searchlightLimit;
        public float searchlightFactor;
        public float searchlightRandom;
        public float searchlightSpeed;
        //applies to all cars
        public float decelerationHorizontal;
        public float decelerationVertical;
        public float speedLimit;
        public float rotationSpeed;
        public float scaleDownTime;
    }


    /// <summary>
    /// Data package for PersonalityManager.cs ->  GetPlayerLikes
    /// </summary>
    public class PlayerLikesData
    {
        public List<string> listOfLikes = new List<string>();
        public List<string> listOfDislikes = new List<string>();
        public List<string> listOfStrongLikes = new List<string>();
        public List<string> listOfStrongDislikes = new List<string>();
    }


    /// <summary>
    /// Data package to track player's progress through a tutorial
    /// </summary>
    [Serializable]
    public class TutorialData
    {
        public string tutorialName;                 //name of Tutorial.SO
        public int index;                           //set index where player is currently at
    }

    /// <summary>
    /// Used to track the progress of Tutorial Goals
    /// </summary>
    public class GoalTracker
    {
        public string startTop;                     //message (top text) given to player at goal initiation
        public string startBottom;                  //message (bottom text) given to player at goal initiation
        public string finishTop;                    //message (top text) given to player upon goal completion
        public string finishBottom;                 //message (bottom text) given to player upon goal completion
        public string goalName;                     //tutorialGoal.name (reference only)
        //primary goal
        public GoalType goal0;                      //type of goal associated with data0 and target0
        public int data0;                           //current value of first data point to be tracked, default -1
        public int target0;                         //target value of first data point in order for goal to be achieved
        //secondary, optional, goal
        public GoalType goal1;                      //type of goal associated with data1 and target1
        public int data1;                           //current value of second data point to be tracked (if any), default -1
        public int target1;                         //target value of second data point in order for goal to be achieved
    }


    //new classes above here
}
