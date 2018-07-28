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
        public string type;
        public bool isTargetKnown;
        public bool isContact;
        public bool isContactKnown;
        public bool isTracer;
        public bool isTracerActive;
        public bool isTracerKnown;
        public bool isTeamKnown;
        public bool isSpiderKnown;                                      //displays Spider images at tooltip top if spider present in node and known
        public int spiderTimer;                                         //ignore timers if spider or tracer aren't present
        public int tracerTimer;
        public int[] arrayOfStats;
        public List<string> listOfCrisis;
        public List<string> listOfActive;
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
        public string textOrigin;                                            //name of thing that caused the effect, eg. gear name


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
        public int value;                                                 //how much the field changes, eg. +1, -1, etc.
        public int timer;                                                 //how long does the effect last for?
        public EffectOutcome outcome;
        public GlobalType type;                                           //benefit, or otherwise, of effect from POV of Resistance
        public EffectApply apply;
        public GlobalSide side;

        public EffectDataOngoing()
        { timer = GameManager.instance.effectScript.ongoingEffectTimer; }
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
        public List<string> listOfData_0;         //list of strings (one per line) for main tab
        public List<string> listOfData_1;         //list of strings (one per line) for HQ tab
        public List<string> listOfData_2;         //list of strings (one per line) for People tab
        public List<ItemData> listOfData_3;         //list of ItemData (one per line) for Random tab
        public List<string> listOfData_4;         //list of strings (one per line) for Ongoing Effects tab

        public MainInfoData()
        {
            listOfData_0 = new List<string>();
            listOfData_1 = new List<string>();
            listOfData_2 = new List<string>();
            listOfData_3 = new List<ItemData>();
            listOfData_4 = new List<string>();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="dataCopy"></param>
        public MainInfoData(MainInfoData dataCopy)
        {
            listOfData_0 = new List<string>(dataCopy.listOfData_0);
            listOfData_1 = new List<string>(dataCopy.listOfData_1);
            listOfData_2 = new List<string>(dataCopy.listOfData_2);
            listOfData_3 = new List<ItemData>(dataCopy.listOfData_3);
            listOfData_4 = new List<string>(dataCopy.listOfData_4);
        }

        /// <summary>
        /// Empties out all data
        /// </summary>
        public void Reset()
        {
            listOfData_0.Clear();
            listOfData_1.Clear();
            listOfData_2.Clear();
            listOfData_3.Clear();
            listOfData_4.Clear();
        }
    }

    /// <summary>
    /// multipurpose class to handle message equivalents, requests and meeting decision options for MainInfoData packages
    /// NOTE: Don't add any methods as it needs to be memory efficient
    /// </summary>
    public class ItemData
    {
        public string itemText;
        public string topText;
        public string bottomText;
        public Image sprite;
        public ItemPriority priority;
    }
}
