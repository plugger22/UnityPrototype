using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

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
        public bool isActor;
        public bool isActorKnown;
        public bool isTracerActive;
        public bool isTeamKnown;
        public int[] arrayOfStats;
        public List<string> listOfActive;
        public List<EffectDataTooltip> listOfEffects;
        public List<string> listOfTeams;
        public List<string> listOfTargets;
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
        public string[] arrayOfQualities;
        public int[] arrayOfStats;
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
        public int ongoingID = -1;                                           //used only if there are going to be ongoing effects, ignore otherwise
        public string ongoingText;                                           //used only if there are going to be ongoing effects, ignore otherwise

        public EffectDataInput()
        { side = GameManager.instance.sideScript.PlayerSide; }
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
        public int adjustment;                                          //change in normal action allocation, eg. +1, -1
        public int timer;                                               //number of turns that the effect lasts for (decremented down to zero)
    }

}
