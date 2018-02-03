﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System.Text;

//
// - - - Effect Data Packages - - -
//

/// <summary>
/// used to pass data into ProcessEffect
/// </summary>
public class EffectDataInput
{
    public Side side;                                                    //used to determine colouring of good/bad effects
    public int ongoingID = -1;                                           //used only if there are going to be ongoing effects, ignore otherwise
    public string ongoingText;                                           //used only if there are going to be ongoing effects, ignore otherwise

    public EffectDataInput()
    { side = GameManager.instance.sideScript.PlayerSide; }
}

/// <summary>
/// used to pass data back to Node for an ongoing effect
/// </summary>
public class EffectDataOngoing
{
    public int ongoingID = -1;                                             //links back to a central registry to enable cancelling of ongoing effect at a later point
    public string text;
    public int value;                                                 //how much the field changes, eg. +1, -1, etc.
    public EffectOutcome outcome;
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



//
// - - - Effect Class - - -
//

/// <summary>
/// handles Effect related matters (Actions and Targets)
/// Effects are assumed to generate text and outcomes for a Modal Outcome window
/// </summary>
public class EffectManager : MonoBehaviour
{

    //colour palette for Modal Outcome
    private string colourOutcome1; //good effect Rebel / bad effect Authority
    private string colourOutcome2; //bad effect Authority / bad effect Rebel
    private string colourOutcome3; //used when node is EqualsTo, eg. reset, or for Team Names
    private string colourNormal;
    private string colourDefault;
    private string colourGood;
    private string colourBad;
    private string colourActor;
    private string colourEnd;

    [HideInInspector] private static int ongoingEffectIDCounter = 0;              //used to sequentially number ongoing Effect ID's

    public void Initialise()
    {
        
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
    }

    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        switch (GameManager.instance.sideScript.PlayerSide)
        {
            case Side.Resistance:
                colourOutcome1 = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
                colourOutcome2 = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
                break;
            case Side.Authority:
                colourOutcome1 = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
                colourOutcome2 = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
                break;
        }
        colourOutcome3 = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourActor = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// checks whether effect criteria is valid. Returns "Null" if O.K and a tooltip explanation string if not
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public string CheckEffectCriteria(Effect effect, int nodeID = -1, int actorSlotID = -1, int teamArcID = -1)
    {
        StringBuilder result = new StringBuilder();
        string compareTip = null;
        Node node = null;
        bool errorFlag = false;
        Actor actor = null;
        TeamArc teamArc = null;
        if (effect != null)
        {
            if (effect.listOfCriteria != null && effect.listOfCriteria.Count > 0)
            {
                //
                // - - - access necessary data prior to loop
                //
                //Get node regardless of whether the effect is node related or not
                if (nodeID > -1)
                {
                    node = GameManager.instance.dataScript.GetNode(nodeID);
                    if (node == null)
                    { Debug.LogError("Invalid node (null)"); errorFlag = true; }
                }
                else
                {
                    Debug.LogError(string.Format("Invalid nodeID \"{0}\"", nodeID));
                    errorFlag = true;
                }
                //authority specific data
                if (effect.side == Side.Authority)
                {
                    if (actorSlotID != -1)
                    {
                        //get actor
                        actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, Side.Authority);
                        if (actor != null)
                        {
                            if (teamArcID > -1)
                            {
                                //get team
                                teamArc = GameManager.instance.dataScript.GetTeamArc(teamArcID);
                                if (teamArc == null)
                                {
                                    Debug.LogError(string.Format("Invalid TeamArc (null) for teamArcID \"{0}\" -> Criteria check cancelled", teamArcID));
                                    errorFlag = true;
                                }
                            }
                            else
                            {
                                Debug.LogError(string.Format("Invalid teamArcID \"{0}\" -> Criteria check cancelled", teamArcID));
                                errorFlag = true;
                            }
                        }
                        else
                        {
                            Debug.LogError(string.Format("Invalid Actor (null) for actorSlotID \"{0}\" -> Criteria check cancelled", actorSlotID));
                            errorFlag = true;
                        }
                    }
                    else
                    {
                        Debug.LogError("Invalid actorSlotID -> Criteria Check cancelled");
                        errorFlag = true;
                    }
                }
                //O.K to proceed?
                if (errorFlag == false)
                {
                    foreach (Criteria criteria in effect.listOfCriteria)
                    {
                        switch (GameManager.instance.sideScript.PlayerSide)
                        {
                            case Side.Resistance:
                                //check effect is the correct side
                                if (effect.side == Side.Resistance)
                                {
                                    //
                                    // - - - Resistance - - - 
                                    //
                                    switch (criteria.criteriaEffect)
                                    {
                                        case EffectCriteria.NodeSecurity:
                                            compareTip = ComparisonCheck(criteria.criteriaValue, node.Security, criteria.criteriaCompare);
                                            if (compareTip != null)
                                            {
                                                BuildString(result, "Security " + compareTip);
                                            }
                                            break;
                                        case EffectCriteria.NodeStability:
                                            compareTip = ComparisonCheck(criteria.criteriaValue, node.Stability, criteria.criteriaCompare);
                                            if (compareTip != null)
                                            {
                                                BuildString(result, "Stability " + compareTip);
                                            }
                                            break;
                                        case EffectCriteria.NodeSupport:
                                            compareTip = ComparisonCheck(criteria.criteriaValue, node.Support, criteria.criteriaCompare);
                                            if (compareTip != null)
                                            {
                                                BuildString(result, "Support " + compareTip);
                                            }
                                            break;
                                        case EffectCriteria.NumRecruits:
                                            //criteria value overriden in this case
                                            criteria.criteriaValue = GameManager.instance.dataScript.GetNumOfActorsInReserve();
                                            compareTip = ComparisonCheck(GameManager.instance.actorScript.numOfReserveActors, criteria.criteriaValue, criteria.criteriaCompare);
                                            if (compareTip != null)
                                            {
                                                BuildString(result, "maxxed Recruit allowance");
                                            }
                                            break;
                                        case EffectCriteria.NumTeams:
                                            compareTip = ComparisonCheck(criteria.criteriaValue, node.CheckNumOfTeams(), criteria.criteriaCompare);
                                            if (compareTip != null)
                                            {
                                                BuildString(result, "no Teams present");
                                            }
                                            break;
                                        case EffectCriteria.NumTracers:
                                            if (node.isTracer == true)
                                            { BuildString(result, "Tracer already present"); }
                                            break;
                                        case EffectCriteria.TargetInfo:
                                            compareTip = ComparisonCheck(criteria.criteriaValue, node.targetID, criteria.criteriaCompare);
                                            if (compareTip != null)
                                            { BuildString(result, "Full Info already"); }
                                            break;
                                        case EffectCriteria.TargetPresent:
                                            //check that a target is present at the node
                                            if (node.targetID < 0)
                                            { BuildString(result, "No Target present"); }
                                            break;
                                        case EffectCriteria.NumGear:
                                            //Note: effect criteria value is ignored in this case
                                            compareTip = ComparisonCheck(GameManager.instance.gearScript.maxNumOfGear, GameManager.instance.playerScript.GetNumOfGear(), criteria.criteriaCompare);
                                            if (compareTip != null)
                                            { BuildString(result, "maxxed Gear Allowance"); }
                                            break;
                                        case EffectCriteria.GearAvailability:
                                            //checks to see if at least 1 piece of unused common gear is available
                                            List<int> tempCommonGear = new List<int>(GameManager.instance.dataScript.GetListOfGear(GearLevel.Common));
                                            if (tempCommonGear.Count > 0)
                                            {
                                                //remove from lists any gear that the player currently has
                                                List<int> tempPlayerGear = new List<int>(GameManager.instance.playerScript.GetListOfGear());
                                                int gearID;
                                                if (tempPlayerGear.Count > 0)
                                                {
                                                    for (int i = 0; i < tempPlayerGear.Count; i++)
                                                    {
                                                        gearID = tempPlayerGear[i];
                                                        if (tempCommonGear.Exists(id => id == gearID) == true)
                                                        { tempCommonGear.Remove(gearID); }
                                                    }
                                                }
                                                if (tempCommonGear.Count == 0)
                                                { BuildString(result, "No Gear available"); }
                                            }
                                            else { BuildString(result, "No Gear available"); }
                                            break;
                                        case EffectCriteria.RebelCause:
                                            compareTip = ComparisonCheck(criteria.criteriaValue, GameManager.instance.rebelScript.resistanceCause, criteria.criteriaCompare);
                                            if (compareTip != null)
                                            {
                                                BuildString(result, "Rebel Cause  " + compareTip);
                                            }
                                            break;
                                        default:
                                            BuildString(result, "Error!");
                                            Debug.LogError(string.Format("Invalid Resistance criteriaEffect \"{0}\"", criteria.criteriaEffect));
                                            errorFlag = true;
                                            break;
                                    }
                                }
                                else { Debug.LogError("EffectManager: side NOT Resistance -> Criteria check cancelled"); }
                                break;
                            case Side.Authority:
                                //check effect is the correct side
                                if (effect.side == Side.Authority)
                                {
                                    //
                                    // - - - Authority - - -
                                    //
                                    switch (criteria.criteriaEffect)
                                    {
                                        case EffectCriteria.NumTeams:
                                            //there is a maximum limit to the number of teams that can be present at a node
                                            compareTip = ComparisonCheck(criteria.criteriaValue, node.CheckNumOfTeams(), criteria.criteriaCompare);
                                            if (compareTip != null)
                                            { BuildString(result, "Too many teams present"); }
                                            break;
                                        case EffectCriteria.ActorAbility:
                                            //actor can only have a number of teams OnMap equal to their ability at any time
                                            if (actor.CheckNumOfTeams() >= actor.datapoint2)
                                            { BuildString(result, "Actor Ability exceeded"); }
                                            break;
                                        case EffectCriteria.TeamIdentical:
                                            //there can only be one team of a type at a node
                                            if (node.CheckTeamPresent(teamArcID) > -1)
                                            { BuildString(result, string.Format(" {0} Team already present", teamArc.name)); }
                                            break;
                                        case EffectCriteria.TeamPreferred:
                                            //there must be a spare team in the reserve pool of the actors preferred typ
                                            if (GameManager.instance.dataScript.CheckTeamInfo(teamArcID, TeamInfo.Reserve) < 1)
                                            { BuildString(result, string.Format("No {0} Team available", teamArc.name)); }
                                            break;
                                        case EffectCriteria.TeamAny:
                                            //there must be a spare team of any type in the reserve pool
                                            if (GameManager.instance.dataScript.CheckTeamPoolCount(TeamPool.Reserve) < 1)
                                            { BuildString(result, string.Format("No Teams available", teamArc.name)); }
                                            break;
                                        default:
                                            BuildString(result, "Error!");
                                            Debug.LogWarning(string.Format("Invalid Authority effect.criteriaEffect \"{0}\"", criteria.criteriaEffect));
                                            errorFlag = true;
                                            break;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("EffectManager: side NOT Authority -> Criteria check cancelled");
                                    errorFlag = true;
                                }
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid Side \"{0}\" -> effect criteria check cancelled", GameManager.instance.sideScript.PlayerSide));
                                errorFlag = true;
                                break;
                        }
                        //exit on error
                        if (errorFlag == true)
                        { break; }
                    }
                }
            }
        }
        else
        { Debug.LogError("Invalid Effect (Null) -> effect criteria check cancelled");  }
        if (result.Length > 0)
        { return result.ToString(); }
        else { return null; }
    }

    /// <summary>
    /// subMethod to handle stringbuilder admin for CheckEffectCriteria()
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="text1"></param>
    private void BuildString(StringBuilder builder, string text1)
    {
        if (string.IsNullOrEmpty(text1) == false)
        {
            if (builder.Length > 0)
            { builder.AppendLine(); }
            builder.Append(text1);
        }
    }

    /// <summary>
    /// returns null if all O.K and a tool tip string if not giving criteria, eg. "< 1"
    /// </summary>
    /// <param name="criteriaValue"></param>
    /// <param name="actualValue"></param>
    /// <param name="comparison"></param>
    /// <returns></returns>
    private string ComparisonCheck(int criteriaValue, int actualValue, Comparison comparison)
    {
        string result = null;
        switch (comparison)
        {
            case Comparison.LessThan:
                if (actualValue >= criteriaValue)
                { result = string.Format("< {0}, currently {1}", criteriaValue, actualValue); }
                break;
            case Comparison.GreaterThan:
                if (actualValue <= criteriaValue)
                { result = string.Format("> {0}, currently {1}", criteriaValue, actualValue); }
                break;
            case Comparison.EqualTo:
                if (criteriaValue != actualValue)
                { result = string.Format("{0}, currently {1}", criteriaValue, actualValue); }
                break;
            default:
                result = "Error!";
                Debug.LogError("Invalid Comparison enum");
                break;
        }
        return result;
    }


    /// <summary>
    /// Processes effects and returns results in a class. 
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="node"></param>
    /// <param name="dataInput">data package from calling method</param>
    /// <param name="actor">if Resistance and Player effects then go with default actor = null, otherwise supply an actor</param>
    /// <returns></returns>
    public EffectDataReturn ProcessEffect(Effect effect, Node node, EffectDataInput dataInput, Actor actor = null)
    {
        int teamID, teamArcID;
        EffectDataReturn effectReturn = new EffectDataReturn();
        //set default values
        effectReturn.errorFlag = false;
        effectReturn.topText = "";
        effectReturn.bottomText = "";
        effectReturn.isAction = false;
        //valid effect?
        if (effect != null)
        {
            switch (effect.outcome)
            {
                //
                // - - - Node Data effects (both sides)
                //
                case EffectOutcome.RevealSpiders:
                case EffectOutcome.RevealTracers:
                case EffectOutcome.RevealTeams:
                case EffectOutcome.RevealActors:
                case EffectOutcome.Security:
                case EffectOutcome.Stability:
                case EffectOutcome.Support:
                    if (node != null)
                    {
                        EffectDataResolve resolve = ResolveNodeData(effect, node, dataInput);
                        if (resolve.isError == true)
                        { effectReturn.errorFlag = true; }
                        else
                        {
                            effectReturn.topText = resolve.topText;
                            effectReturn.bottomText = resolve.bottomText;
                            effectReturn.isAction = true;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome));
                        effectReturn.errorFlag = true;
                    }
                    break;
                //
                // - - - Resistance effects
                //
                case EffectOutcome.RebelCause:
                    int rebelCause = GameManager.instance.rebelScript.resistanceCause;
                    int maxCause = GameManager.instance.rebelScript.resistanceCauseMax;
                    switch (effect.result)
                    {
                        case Result.Add:
                            rebelCause += effect.value;
                            rebelCause = Mathf.Min(maxCause, rebelCause);
                            GameManager.instance.rebelScript.resistanceCause = rebelCause;
                            effectReturn.topText = string.Format("{0}The Rebel Cause gains traction{1}", colourDefault, colourEnd);
                            effectReturn.bottomText = string.Format("{0}Rebel Cause +{1}{2}", colourOutcome1, effect.value, colourEnd);
                            break;
                        case Result.Subtract:
                            rebelCause -= effect.value;
                            rebelCause = Mathf.Max(0, rebelCause);
                            GameManager.instance.rebelScript.resistanceCause = rebelCause;
                            effectReturn.topText = string.Format("{0}The Rebel Cause is losing ground{1}", colourDefault, colourEnd);
                            effectReturn.bottomText = string.Format("{0}Rebel Cause -{1}{2}", colourOutcome2, effect.value, colourEnd);
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.result));
                            effectReturn.errorFlag = true;
                            break;
                    }
                    effectReturn.isAction = true;
                    break;
                case EffectOutcome.Recruit:
                    //no effect, handled directly elsewhere (check ActorManager.cs -> GetActorActions
                    break;
                case EffectOutcome.Tracer:
                    if (node != null)
                    {
                        node.isTracer = true;
                        node.isTracerActive = true;
                        //neighbouring nodes made active
                        List<Node> listOfNeighbours = node.GetNeighbouringNodes();
                        if (listOfNeighbours != null)
                        {
                            foreach (Node nodeNeighbour in listOfNeighbours)
                            {
                                nodeNeighbour.isTracerActive = true;
                                if (nodeNeighbour.isSpider == true)
                                { nodeNeighbour.isSpiderKnown = true; }
                            }
                            effectReturn.isAction = true;
                        }
                        else { Debug.LogError("Invalid listOfNeighbours (Null)"); }
                        //dialogue
                        effectReturn.topText = string.Format("{0}A Tracer has been successfully inserted{1}", colourDefault, colourEnd);
                        effectReturn.bottomText = string.Format("{0}All Spiders within a one node radius are revealed{1}", colourOutcome3, colourEnd);
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome));
                        effectReturn.errorFlag = true;
                    }
                    break;
                case EffectOutcome.Gear:
                    //no effect, handled directly elsewhere (check ActorManager.cs -> GetActorActions
                    break;
                case EffectOutcome.TargetInfo:
                    //TO DO
                    break;
                case EffectOutcome.NeutraliseTeam:
                    //no effect, handled directly elsewhere (check ActorManager.cs -> GetActorActions
                    break;
                case EffectOutcome.ConnectionSecurity:
                    //handled elsewhere
                    break;
                case EffectOutcome.SpreadInstability:
                    //TO DO
                    break;
                case EffectOutcome.Invisibility:
                    //raise/lower invisibility of actor or Player
                    if (node != null)
                    {

                        if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                        {
                            //
                            // - - - Player Invisibility effect - - -
                            //
                            switch (effect.result)
                            {
                                case Result.Add:
                                    if (GameManager.instance.playerScript.invisibility < 3)
                                    { GameManager.instance.playerScript.invisibility++; }
                                    effectReturn.bottomText = string.Format("{0}Player {1}{2}", colourOutcome1, effect.description, colourEnd);
                                    break;
                                case Result.Subtract:
                                    //does player have any invisibility type gear?
                                    int gearID = GameManager.instance.playerScript.CheckGearTypePresent(GearType.Invisibility);
                                    if (gearID > -1)
                                    {
                                        //gear present -> No drop in Invisibility
                                        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                                        if (gear != null)
                                        {
                                            int chance = GameManager.instance.gearScript.GetChanceOfCompromise(gearID);
                                            if (Random.Range(0, 100) <= chance)
                                            {
                                                //gear compromised
                                                effectReturn.bottomText = string.Format("{0}{1} used to stay Invisible (Compromised!){2}", colourOutcome2, gear.name,
                                                    colourEnd);
                                                //remove gear
                                                GameManager.instance.playerScript.RemoveGear(gearID);
                                            }
                                            else
                                            {
                                                //gear O.K
                                                effectReturn.bottomText = string.Format("{0}{1} used to stay Invisible (still O.K){2}", colourOutcome1, gear.name,
                                                    colourEnd);
                                            }
                                            //special invisibility gear effects
                                            switch (gear.data)
                                            {
                                                case 1:
                                                    //negates invisibility and increases it by +1 at the same time
                                                    if (GameManager.instance.playerScript.invisibility < 3)
                                                    { GameManager.instance.playerScript.invisibility++; }
                                                    effectReturn.bottomText = string.Format("{0}{1}{2}{3}Invisibility +1 ({4}){5}", effectReturn.bottomText,
                                                        "\n", "\n", colourOutcome1, gear.name, colourEnd);
                                                    break;
                                            }
                                        }
                                        else { Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", gearID)); }
                                    }
                                    else
                                    {
                                        //No gear present
                                        int invisibility = GameManager.instance.playerScript.invisibility;
                                        //double effect if spider is present
                                        if (node.isSpider == true)
                                        {
                                            invisibility -= 2;
                                            effectReturn.bottomText = string.Format("{0}Player Invisibility -2 (Spider) (Now {1}){2}", colourOutcome2,
                                                invisibility, colourEnd);
                                        }
                                        else
                                        {
                                            invisibility -= 1;
                                            effectReturn.bottomText = string.Format("{0}Player {1} (Now {2}){3}", colourOutcome2, effect.description,
                                                invisibility, colourEnd);
                                        }
                                        //mincap zero
                                        invisibility = Mathf.Max(0, invisibility);
                                        GameManager.instance.playerScript.invisibility = invisibility;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            //
                            // - - - Actor Invisibility effect - - -
                            //
                            if (actor != null)
                            {
                                switch (effect.result)
                                {
                                    case Result.Add:
                                        if (actor.datapoint2 < 3)
                                        { actor.datapoint2++; }
                                        effectReturn.bottomText = string.Format("{0}{1} {2} (Now {3}){4}", colourOutcome1, actor.actorName, effect.description,
                                            actor.datapoint2, colourEnd);
                                        break;
                                    case Result.Subtract:
                                        int invisibility = actor.datapoint2;
                                        //double effect if spider is present
                                        if (node.isSpider == true)
                                        {
                                            invisibility -= 2;
                                            effectReturn.bottomText = string.Format("{0}{1} Invisibility -2 (Spider) (Now {2}){3}", colourOutcome2, actor.actorName,
                                                invisibility, colourEnd);
                                        }
                                        else
                                        {
                                            invisibility -= 1;
                                            effectReturn.bottomText = string.Format("{0}{1} {2} (Now {3}){4}", colourOutcome2, actor.actorName, effect.description,
                                                invisibility, colourEnd);
                                        }
                                        //mincap zero
                                        invisibility = Mathf.Max(0, invisibility);
                                        actor.datapoint2 = invisibility;
                                        break;
                                }
                            }
                            else
                            {
                                Debug.LogError(string.Format("Invalid Actor (null) for EffectOutcome \"{0}\"", effect.outcome));
                                effectReturn.errorFlag = true;
                            }

                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome));
                        effectReturn.errorFlag = true;
                    }
                    break;
                //Note: Renown can be in increments of > 1
                case EffectOutcome.Renown:
                    if (node != null)
                    {

                        if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                        {
                            int playerRenown = GameManager.instance.playerScript.Renown;
                            //Player effect
                            switch (effect.result)
                            {
                                case Result.Add:
                                    playerRenown += effect.value;
                                    effectReturn.bottomText = string.Format("{0}Player {1} (Now {2}){3}", colourOutcome1, effect.description,
                                        playerRenown, colourEnd);
                                    break;
                                case Result.Subtract:
                                    playerRenown -= effect.value;
                                    playerRenown = Mathf.Max(0, playerRenown);
                                    effectReturn.bottomText = string.Format("{0}Player {1} (Now {2}){3}", colourOutcome2, effect.description,
                                        playerRenown, colourEnd);
                                    break;
                            }
                            GameManager.instance.playerScript.Renown = playerRenown;
                        }
                        else
                        {
                            //Actor effect
                            if (actor != null)
                            {
                                switch (effect.result)
                                {
                                    case Result.Add:
                                        actor.renown += effect.value;
                                        effectReturn.bottomText = string.Format("{0}{1} {2} (Now {3}){4}", colourOutcome2, actor.actorName, effect.description,
                                            actor.renown, colourEnd);
                                        break;
                                    case Result.Subtract:
                                        actor.renown -= effect.value;
                                        actor.renown = Mathf.Max(0, actor.renown);
                                        effectReturn.bottomText = string.Format("{0}{1} {2} (Now {3}){4}", colourOutcome1, actor.actorName, effect.description,
                                            actor.renown, colourEnd);
                                        break;
                                }
                            }
                            else
                            {
                                Debug.LogError(string.Format("Invalid Actor (null) for EffectOutcome \"{0}\"", effect.outcome));
                                effectReturn.errorFlag = true;
                            }

                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome));
                        effectReturn.errorFlag = true;
                    }
                    break;
                //
                // - - - Authority Effects
                //
                case EffectOutcome.AnyTeam:
                    // Not needed as handled by a different process, keep for reference
                    break;
                case EffectOutcome.CivilTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Civil");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    //insert team
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                    //return texts
                    effectReturn.topText = SetTopText(teamID);
                    effectReturn.bottomText = SetBottomText(actor);
                    //action
                    effectReturn.isAction = true;
                    break;
                case EffectOutcome.ControlTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Control");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                    //return texts
                    effectReturn.topText = SetTopText(teamID);
                    effectReturn.bottomText = SetBottomText(actor);
                    effectReturn.isAction = true;
                    break;
                case EffectOutcome.DamageTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Damage");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                    //return texts
                    effectReturn.topText = SetTopText(teamID);
                    effectReturn.bottomText = SetBottomText(actor);
                    effectReturn.isAction = true;
                    break;
                case EffectOutcome.ErasureTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Erasure");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                    //return texts
                    effectReturn.topText = SetTopText(teamID);
                    effectReturn.bottomText = SetBottomText(actor);
                    effectReturn.isAction = true;
                    break;
                case EffectOutcome.MediaTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Media");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                    //return texts
                    effectReturn.topText = SetTopText(teamID);
                    effectReturn.bottomText = SetBottomText(actor);
                    effectReturn.isAction = true;
                    break;
                case EffectOutcome.ProbeTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Probe");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                    //return texts
                    effectReturn.topText = SetTopText(teamID);
                    effectReturn.bottomText = SetBottomText(actor);
                    effectReturn.isAction = true;
                    break;
                case EffectOutcome.SpiderTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Spider");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                    //return texts
                    effectReturn.topText = SetTopText(teamID);
                    effectReturn.bottomText = SetBottomText(actor);
                    effectReturn.isAction = true;
                    break;

                default:
                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome));
                    effectReturn.errorFlag = true;
                    break;
            }
        }
        else
        {
            //No effect paramater
            Debug.LogError("Invalid Effect (null)");
            effectReturn = null;
        }

        return effectReturn;
    }

    /// <summary>
    /// Formats string for details.TopText (authority), returns "unknown" if a problem. Also used by ModalTeamPicker.cs -> ProcessTeamChoice and TeamNanager.cs
    /// Can be used for both Inserting and Recalling a team
    /// </summary>
    /// <param name="team"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public string SetTopText(int teamID, bool isInserted = true)
    {
        //get team
        Team team = GameManager.instance.dataScript.GetTeam(teamID);
        if (team != null)
        {
            //get node
            Node node = GameManager.instance.dataScript.GetNode(team.NodeID);
            if (node != null)
            {
                string operation = "inserted";
                if (isInserted == false) { operation = "recalled"; }
                return string.Format("{0}{1}{2}{3} {4} have been {5} at {6}{7}{8}{9}{10} {11}{12}", 
                    colourOutcome3, team.Arc.name.ToUpper(), colourEnd, 
                    colourNormal, team.Name, operation, colourEnd,  
                    colourOutcome2, node.Arc.name.ToUpper(), colourEnd, colourNormal, node.nodeName, colourEnd);
            }
            else
            {
                Debug.LogError(string.Format("Invalid node (Null) for team.NodeID {0}", team.NodeID));
                return "Unknown";
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", teamID));
            return "Unknown";
        }

    }

    /// <summary>
    /// Formats string for details.BottomText (authority), returns "unknown" if parameter is null. Also used by ModalTeamPicker.cs -> ProcessTeamChoice
    /// </summary>
    /// <returns></returns>
    public string SetBottomText(Actor actor)
    {
        if (actor != null)
        {
            string colourNumbers = colourGood;
            if (actor.CheckNumOfTeams() == actor.datapoint2)
            { colourNumbers = colourBad; }
            return string.Format("{0}, {1} of {2}{3}{4} has now deployed {5}{6}{7} of {8}{9}{10} teams",
                actor.actorName, (AuthorityActor)GameManager.instance.turnScript.metaLevel, colourActor, actor.arc.name, colourEnd,
                colourNumbers, actor.CheckNumOfTeams(), colourEnd, colourNumbers, actor.datapoint2, colourEnd);
        }
        else
        {
            Debug.LogError("Invalid actor (Null)");
            return "Unknown";
        }
    }

    /// <summary>
    /// Sub method to process Node Stability/Security/Support
    /// Note: Effect and Node checked for null in calling method
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveNodeData(Effect effect, Node node, EffectDataInput effectInput)
    {
        int value = 0;
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //data package to send to node for field processing
        EffectDataProcess effectProcess = new EffectDataProcess() { outcome = effect.outcome };
        //Extent of effect
        switch (effect.apply)
        {
            //Current Node only
            case EffectApply.NodeCurrent:
                switch (effect.result)
                {
                    case Result.Add:
                        value = effect.value;
                        switch (effect.outcome)
                        {
                            case EffectOutcome.Security:
                                effectResolve.topText = string.Format("{0}The security system has been swept and strengthened{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Node Security +{1}{2}", colourOutcome2, effect.value, colourEnd);
                                break;
                            case EffectOutcome.Stability:
                                effectResolve.topText = string.Format("{0}Law Enforcement teams have stabilised the situation{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Node Stability +{1}{2}", colourOutcome2, effect.value, colourEnd);
                                break;
                            case EffectOutcome.Support:
                                effectResolve.topText = string.Format("{0}There is a surge of support for the Rebels{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Node Support +{1}{2}", colourOutcome1, effect.value, colourEnd);
                                break;
                            case EffectOutcome.RevealTracers:

                                break;
                            case EffectOutcome.RevealSpiders:

                                break;
                            case EffectOutcome.RevealActors:

                                break;
                            case EffectOutcome.RevealTeams:

                                break;
                        }
                        break;
                    case Result.Subtract:
                        value = -1 * effect.value;
                        switch (effect.outcome)
                        {
                            case EffectOutcome.Security:
                                effectResolve.topText = string.Format("{0}The security system has been successfully hacked{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Node Security -{1}{2}", colourOutcome1, effect.value, colourEnd);
                                break;
                            case EffectOutcome.Stability:
                                effectResolve.topText = string.Format("{0}Civil unrest and instability is spreading throughout{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Node Stability -{1}{2}", colourOutcome1, effect.value, colourEnd);
                                break;
                            case EffectOutcome.Support:
                                effectResolve.topText = string.Format("{0}The Rebels are losing popularity{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Node Support -{1}{2}", colourOutcome2, effect.value, colourEnd);
                                break;
                        }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.result));
                        effectResolve.isError = true;
                        break;
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration == EffectDuration.Ongoing)
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, value); }
                //Process Node effect
                node.ProcessNodeEffect(effectProcess);
                break;
            //Neighbouring Nodes
            case EffectApply.NodeNeighbours:
                switch (effect.result)
                {
                    case Result.Add:
                        value = effect.value;
                        switch (effect.outcome)
                        {
                            case EffectOutcome.Security:
                                effectResolve.topText = string.Format("{0}Neighbouring security systems have been swept and strengthened{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Neighbouring Nodes Security +{1}{2}", colourOutcome2, effect.value, colourEnd);
                                break;
                            case EffectOutcome.Stability:
                                effectResolve.topText = string.Format("{0}Law Enforcement teams have stabilised neighbouring nodes{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Node and Neighbours Stability +{1}{2}", colourOutcome2, effect.value, colourEnd);
                                break;
                            case EffectOutcome.Support:
                                effectResolve.topText = string.Format("{0}There is a surge of support for the Rebels in neighbouring nodes{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Node and Neighbours Support +{1}{2}", colourOutcome1, effect.value, colourEnd);
                                break;
                        }
                        break;
                    case Result.Subtract:
                        value = -1 * effect.value;
                        switch (effect.outcome)
                        {
                            case EffectOutcome.Security:
                                effectResolve.topText = string.Format("{0}Neighbouring security systems have been successfully hacked{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Neighbouring Nodes Security -{1}{2}", colourOutcome1, effect.value, colourEnd);
                                break;
                            case EffectOutcome.Stability:
                                effectResolve.topText = string.Format("{0}Civil unrest and instability is spreading throughout the neighbouring nodes{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Node and Neighbours Stability -{1}{2}", colourOutcome1, effect.value, colourEnd);
                                break;
                            case EffectOutcome.Support:
                                effectResolve.topText = string.Format("{0}The Rebels are losing popularity in neighbouring nodes{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Node and Neighbour Support -{1}{2}", colourOutcome2, effect.value, colourEnd);
                                break;
                        }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.result));
                        effectResolve.isError = true;
                        break;
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration == EffectDuration.Ongoing)
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, value); }
                //Process Node effect for current node
                node.ProcessNodeEffect(effectProcess);
                //Process Node effect for all neighbouring nodes
                List<Node> listOfNeighbours = node.GetNeighbouringNodes();
                if (listOfNeighbours != null)
                {
                    foreach (Node nodeTemp in listOfNeighbours)
                    { nodeTemp.ProcessNodeEffect(effectProcess); }
                }
                else { Debug.LogError(string.Format("Invalid listOfNeighbours (Null) for nodeID {0}, \"{1}\"", node.nodeID, node.nodeName)); }
                break;
            //All Nodes
            case EffectApply.NodeAll:
                switch (effect.result)
                {
                    case Result.Add:
                        value = effect.value;
                        switch (effect.outcome)
                        {
                            case EffectOutcome.Security:
                                effectResolve.topText = string.Format("{0}All security systems have been swept and strengthened{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}All Nodes Security +{1}{2}", colourOutcome2, effect.value, colourEnd);
                                break;
                            case EffectOutcome.Stability:
                                effectResolve.topText = string.Format("{0}Law Enforcement teams have stabilised the city wide situation{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}All Nodes Stability +{1}{2}", colourOutcome2, effect.value, colourEnd);
                                break;
                            case EffectOutcome.Support:
                                effectResolve.topText = string.Format("{0}There is a surge of support for the Rebels throughout the city{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}All Nodes Support +{1}{2}", colourOutcome1, effect.value, colourEnd);
                                break;
                        }
                        break;
                    case Result.Subtract:
                        value = -1 * effect.value;
                        switch (effect.outcome)
                        {
                            case EffectOutcome.Security:
                                effectResolve.topText = string.Format("{0}All security systems have been successfully hacked{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}All Nodes Security -{1}{2}", colourOutcome1, effect.value, colourEnd);
                                break;
                            case EffectOutcome.Stability:
                                effectResolve.topText = string.Format("{0}Civil unrest and instability is spreading throughout the city{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}All Nodes Stability -{1}{2}", colourOutcome1, effect.value, colourEnd);
                                break;
                            case EffectOutcome.Support:
                                effectResolve.topText = string.Format("{0}The Rebels are losing popularity throughout the city{1}", colourDefault, colourEnd);
                                effectResolve.bottomText = string.Format("{0}All Nodes Support -{1}{2}", colourOutcome2, effect.value, colourEnd);
                                break;
                        }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.result));
                        effectResolve.isError = true;
                        break;
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration == EffectDuration.Ongoing)
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, value); }
                //Process Node effect for current node
                node.ProcessNodeEffect(effectProcess);
                //Process Node effect for all neighbouring nodes
                Dictionary<int, Node> dictOfAllNodes = GameManager.instance.dataScript.GetAllNodes();
                if (dictOfAllNodes != null)
                {
                    foreach (var nodeTemp in dictOfAllNodes)
                    { nodeTemp.Value.ProcessNodeEffect(effectProcess); }
                }
                else { Debug.LogError("Invalid dictOfAllNodes (Null)"); }
                break;
            case EffectApply.NodeSameArc:
                switch (effect.result)
                {
                    case Result.Add:
                        value = effect.value;
                        switch (effect.outcome)
                        {
                            case EffectOutcome.Security:
                                effectResolve.topText = string.Format("{0}Security systems in {1} nodes have been swept and strengthened{2}", colourDefault, node.Arc.name.ToUpper(), colourEnd);
                                effectResolve.bottomText = string.Format("{0}All {1} nodes Security +{2}{3}", colourOutcome2, node.Arc.name.ToUpper(), effect.value, colourEnd);
                                break;
                            case EffectOutcome.Stability:
                                effectResolve.topText = string.Format("{0}Law Enforcement teams have stabilised the {1} situation{2}", colourDefault, node.Arc.name.ToUpper(), colourEnd);
                                effectResolve.bottomText = string.Format("{0}{1}Node Stability +{2}{3}", colourOutcome2, node.Arc.name.ToUpper(), effect.value, colourEnd);
                                break;
                            case EffectOutcome.Support:
                                effectResolve.topText = string.Format("{0}There is a surge of support for the Rebels in {1} nodes{2}", colourDefault, node.Arc.name.ToUpper(), colourEnd);
                                effectResolve.bottomText = string.Format("{0}All Nodes Support +{1}{2}", colourOutcome1, effect.value, colourEnd);
                                break;
                        }
                        break;
                    case Result.Subtract:
                        value = -1 * effect.value;
                        switch (effect.outcome)
                        {
                            case EffectOutcome.Security:
                                effectResolve.topText = string.Format("{0}Security systems in {1} nodes have been successfully hacked{2}", colourDefault, node.Arc.name.ToUpper(), colourEnd);
                                effectResolve.bottomText = string.Format("{0}All {1} nodes Security -{2}{3}", colourOutcome1, node.Arc.name.ToUpper(), effect.value, colourEnd);
                                break;
                            case EffectOutcome.Stability:
                                effectResolve.topText = string.Format("{0}Civil unrest and instability is spreading throughout {1} nodes{2}", colourDefault, node.Arc.name.ToUpper(), colourEnd);
                                effectResolve.bottomText = string.Format("{0}{1} Nodes Stability -{1}{2}", colourOutcome1, node.Arc.name.ToUpper(), effect.value, colourEnd);
                                break;
                            case EffectOutcome.Support:
                                effectResolve.topText = string.Format("{0}The Rebels are losing popularity throughout {1} nodes{2}", colourDefault, node.Arc.name.ToUpper(), colourEnd);
                                effectResolve.bottomText = string.Format("{0}{1} Nodes Support -{2}{3}", colourOutcome2, node.Arc.name.ToUpper(), effect.value, colourEnd);
                                break;
                        }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.result));
                        effectResolve.isError = true;
                        break;
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration == EffectDuration.Ongoing)
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, value); }
                //Process Node effect for current node
                node.ProcessNodeEffect(effectProcess);
                //Process Node effect for all neighbouring nodes
                Dictionary<int, Node> dictOfSameNodes = GameManager.instance.dataScript.GetAllNodes();
                if (dictOfSameNodes != null)
                {
                    foreach (var nodeTemp in dictOfSameNodes)
                    {
                        //same node Arc type and not the current node?
                        if (nodeTemp.Value.Arc.NodeArcID == node.Arc.NodeArcID && nodeTemp.Value.nodeID != node.nodeID)
                        { nodeTemp.Value.ProcessNodeEffect(effectProcess); }
                    }
                }
                else { Debug.LogError("Invalid dictOfSameNodes (Null)"); }
                break;
            default:
                Debug.LogError(string.Format("Invalid effect.apply \"{0}\"", effect.apply));
                break;
        }
        //return data to calling method (ProcessEffect)
        return effectResolve;
    }

    /// <summary>
    /// subMethod to handle Ongoing effects
    /// </summary>
    private void ProcessOngoingEffect(Effect effect, EffectDataProcess effectProcess, EffectDataResolve effectResolve, EffectDataInput effectInput, int value)
    {
        EffectDataOngoing effectOngoing = new EffectDataOngoing();
        effectOngoing.outcome = effect.outcome;
        effectOngoing.ongoingID = effectInput.ongoingID;
        effectOngoing.value = value;
        effectOngoing.text = string.Format("{0}{1} {2}{3} ({4}){5}", colourGood, effect.outcome, value, value > 0 ? "+" : "", effectInput.ongoingText, colourEnd);
        //add to effectProcess
        effectProcess.effectOngoing = effectOngoing;
    }

    /*/// <summary>
    /// Creates an ongoing effect, assigns a unique ID
    /// </summary>
    /// <returns></returns>
    private EffectDataOngoing CreateOngoingEffect()
    {
        EffectDataOngoing effectOngoing = new EffectDataOngoing();
        //unique ID is 0+
        effectOngoing.ongoingID = ongoingEffectIDCounter++;
        return effectOngoing;
    }*/

    /// <summary>
    /// gets a unique ID for ongoing effects. All
    /// </summary>
    /// <returns></returns>
    public int GetOngoingEffectID()
    { return ongoingEffectIDCounter++; }

    //place methods above here
}
