using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System.Text;

/// <summary>
/// used to return results of effects to calling method
/// </summary>
public class EffectReturn
{
    public string topText { get; set; }
    public string bottomText { get; set; }
    public bool errorFlag { get; set; }
}



/// <summary>
/// handles Effect related matters (Actions and Targets)
/// Effects are assumed to generate text and outcomes for a Modal Outcome window
/// </summary>
public class EffectManager : MonoBehaviour
{

    //colour palette for Modal Outcome
    private string colourOutcome1; //good effect Rebel / bad effect Authority
    private string colourOutcome2; //bad effect Authority / bad effect Rebel
    private string colourOutcome3; //used when node is EqualsTo, eg. reset
    private string colourNormal;
    private string colourDefault;
    private string colourEnd;



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
        switch (GameManager.instance.optionScript.PlayerSide)
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
                        actor = GameManager.instance.dataScript.GetActor(actorSlotID, Side.Authority);
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
                        switch (GameManager.instance.optionScript.PlayerSide)
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
                                            compareTip = ComparisonCheck(criteria.criteriaValue, GameManager.instance.playerScript.NumOfRecruits, criteria.criteriaCompare);
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
                                            compareTip = ComparisonCheck(criteria.criteriaValue, node.NumOfTracers, criteria.criteriaCompare);
                                            if (compareTip != null)
                                            {
                                                BuildString(result, "Tracers already present");
                                            }
                                            break;
                                        case EffectCriteria.TargetInfo:
                                            compareTip = ComparisonCheck(criteria.criteriaValue, node.TargetID, criteria.criteriaCompare);
                                            if (compareTip != null)
                                            {
                                                BuildString(result, "Full Info already");
                                            }
                                            break;
                                        case EffectCriteria.NumGear:
                                            compareTip = ComparisonCheck(criteria.criteriaValue, GameManager.instance.playerScript.NumOfGear, criteria.criteriaCompare);
                                            if (compareTip != null)
                                            {
                                                BuildString(result, "maxxed Gear Allowance");
                                            }
                                            break;
                                        case EffectCriteria.RebelCause:
                                            compareTip = ComparisonCheck(criteria.criteriaValue, GameManager.instance.playerScript.RebelCauseCurrent, criteria.criteriaCompare);
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
                                            {
                                                BuildString(result, "Too many teams present");
                                            }
                                            break;
                                        case EffectCriteria.ActorAbility:
                                            //actor can only have a number of teams OnMap equal to their ability at any time
                                            if (actor.CheckNumOfTeams() >= actor.Datapoint2)
                                            { BuildString(result, "Actor Ability exceeded"); }
                                            break;
                                        case EffectCriteria.TeamIdentical:
                                            //there can only be one team of a type at a node
                                            if (node.CheckTeamPresent(teamArcID) == true)
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
                                            Debug.LogError(string.Format("Invalid Authority effect.criteriaEffect \"{0}\"", criteria.criteriaEffect));
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
                                Debug.LogError(string.Format("Invalid Side \"{0}\" -> effect criteria check cancelled", GameManager.instance.optionScript.PlayerSide));
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
    /// Processes effects and returns results in a class
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public EffectReturn ProcessEffect(Effect effect, Node node, Actor actor)
    {
        int teamID, teamArcID;
        EffectReturn effectReturn = new EffectReturn();
        //set default values
        effectReturn.errorFlag = false;
        effectReturn.topText = "";
        effectReturn.bottomText = "";
        //valid effect?
        if (effect != null)
        {
            switch (effect.effectOutcome)
            {
                //
                // - - - Resistance effects
                //
                case EffectOutcome.NodeSecurity:
                    if (node != null)
                    {
                        switch (effect.effectResult)
                        {
                            case Result.Add:
                                node.Security += effect.effectValue;
                                node.Security = Mathf.Min(3, node.Security);
                                effectReturn.topText = string.Format("{0}The security system has been swept and strengthened{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Security +{1}{2}", colourOutcome2, effect.effectValue, colourEnd);
                                break;
                            case Result.Subtract:
                                node.Security -= effect.effectValue;
                                node.Security = Mathf.Max(0, node.Security);
                                effectReturn.topText = string.Format("{0}The security system has been successfully hacked{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Security -{1}{2}", colourOutcome1, effect.effectValue, colourEnd);
                                break;
                            case Result.EqualTo:
                                //keep within allowable parameters
                                effect.effectValue = Mathf.Min(3, effect.effectValue);
                                effect.effectValue = Mathf.Max(0, effect.effectValue);
                                node.Security = effect.effectValue;
                                effectReturn.topText = string.Format("{0}The security system has been reset{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Security now {1}{2}", colourOutcome3, node.Security, colourEnd);
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.effectResult));
                                effectReturn.errorFlag = true;
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.effectOutcome));
                        effectReturn.errorFlag = true;
                    }
                    break;
                case EffectOutcome.NodeStability:
                    if (node != null)
                    {
                        switch (effect.effectResult)
                        {
                            case Result.Add:
                                node.Stability += effect.effectValue;
                                node.Stability = Mathf.Min(3, node.Stability);
                                effectReturn.topText = string.Format("{0}Law Enforcement teams have stabilised the situation{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Stability +{1}{2}", colourOutcome2, effect.effectValue, colourEnd);
                                break;
                            case Result.Subtract:
                                node.Stability -= effect.effectValue;
                                node.Stability = Mathf.Max(0, node.Stability);
                                effectReturn.topText = string.Format("{0}Civil unrest and instability is spreading throughout{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Stability -{1}{2}", colourOutcome1, effect.effectValue, colourEnd);
                                break;
                            case Result.EqualTo:
                                //keep within allowable parameters
                                effect.effectValue = Mathf.Min(3, effect.effectValue);
                                effect.effectValue = Mathf.Max(0, effect.effectValue);
                                node.Stability = effect.effectValue;
                                effectReturn.topText = string.Format("{0}Civil obedience has been reset to a new level{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Stability now {1}{2}", colourOutcome3, node.Stability, colourEnd);
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.effectResult));
                                effectReturn.errorFlag = true;
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.effectOutcome));
                        effectReturn.errorFlag = true;
                    }
                    break;
                case EffectOutcome.NodeSupport:
                    if (node != null)
                    {
                        switch (effect.effectResult)
                        {
                            case Result.Add:
                                node.Support += effect.effectValue;
                                node.Support = Mathf.Min(3, node.Support);
                                effectReturn.topText = string.Format("{0}There is a surge of support for the Rebels{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Support +{1}{2}", colourOutcome1, effect.effectValue, colourEnd);
                                break;
                            case Result.Subtract:
                                node.Support -= effect.effectValue;
                                node.Support = Mathf.Max(0, node.Support);
                                effectReturn.topText = string.Format("{0}The Rebels are losing popularity{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Support -{1}{2}", colourOutcome2, effect.effectValue, colourEnd);
                                break;
                            case Result.EqualTo:
                                //keep within allowable parameters
                                effect.effectValue = Mathf.Min(3, effect.effectValue);
                                effect.effectValue = Mathf.Max(0, effect.effectValue);
                                node.Support = effect.effectValue;
                                effectReturn.topText = string.Format("{0}Rebel sentiment has been reset to a new level{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Support now {1}{2}", colourOutcome3, node.Support, colourEnd);
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.effectResult));
                                effectReturn.errorFlag = true;
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.effectOutcome));
                        effectReturn.errorFlag = true;
                    }
                    break;
                case EffectOutcome.RebelCause:
                    int rebelCause = GameManager.instance.playerScript.RebelCauseCurrent;
                    int maxCause = GameManager.instance.playerScript.RebelCauseMax;
                    switch (effect.effectResult)
                    {
                        case Result.Add:
                            rebelCause += effect.effectValue;
                            rebelCause = Mathf.Min(maxCause, rebelCause);
                            GameManager.instance.playerScript.RebelCauseCurrent = rebelCause;
                            effectReturn.topText = string.Format("{0}The Rebel Cause gains traction{1}", colourDefault, colourEnd);
                            effectReturn.bottomText = string.Format("{0}Rebel Cause +{1}{2}", colourOutcome1, effect.effectValue, colourEnd);
                            break;
                        case Result.Subtract:
                            rebelCause -= effect.effectValue;
                            rebelCause = Mathf.Max(0, rebelCause);
                            GameManager.instance.playerScript.RebelCauseCurrent = rebelCause;
                            effectReturn.topText = string.Format("{0}The Rebel Cause is losing ground{1}", colourDefault, colourEnd);
                            effectReturn.bottomText = string.Format("{0}Rebel Cause -{1}{2}", colourOutcome2, effect.effectValue, colourEnd);
                            break;
                        case Result.EqualTo:
                            //keep within allowable parameters
                            effect.effectValue = Mathf.Min(maxCause, effect.effectValue);
                            effect.effectValue = Mathf.Max(0, effect.effectValue);
                            rebelCause = effect.effectValue;
                            GameManager.instance.playerScript.RebelCauseCurrent = rebelCause;
                            effectReturn.topText = string.Format("{0}The Rebel Cause adjusts to a new level{1}", colourDefault, colourEnd);
                            effectReturn.bottomText = string.Format("{0}Rebel Cause now {1}{2}", colourOutcome3, rebelCause, colourEnd);
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.effectResult));
                            effectReturn.errorFlag = true;
                            break;
                    }
                    break;
                case EffectOutcome.Recruit:

                    break;
                case EffectOutcome.AddTracer:

                    break;
                case EffectOutcome.GetGear:

                    break;
                case EffectOutcome.GetTargetInfo:

                    break;
                case EffectOutcome.NeutraliseTeam:

                    break;
                case EffectOutcome.SpreadInstability:

                    break;
                case EffectOutcome.Renown:
                    if (node != null)
                    {
                        if (actor != null)
                        {
                            if (node.NodeID == GameManager.instance.nodeScript.nodePlayer)
                            {
                                //Player effect
                                switch (effect.effectResult)
                                {
                                    case Result.Add:
                                        GameManager.instance.playerScript.Renown++;
                                        effectReturn.bottomText = string.Format("{0}Player {1}{2}", colourOutcome1, effect.description, colourEnd);
                                        break;
                                    case Result.Subtract:
                                        if (GameManager.instance.playerScript.Renown >= effect.effectValue)
                                        { GameManager.instance.playerScript.Renown -= effect.effectValue; }
                                        effectReturn.bottomText = string.Format("{0}Player {1}{2}", colourOutcome2, effect.description, colourEnd);
                                        break;
                                }
                            }
                            else
                            {
                                //Actor effect
                                switch (effect.effectResult)
                                {
                                    case Result.Add:
                                        actor.Renown++;
                                        effectReturn.bottomText = string.Format("{0}{1} {2}{3}", colourOutcome2, actor.Name, effect.description, colourEnd);
                                        break;
                                    case Result.Subtract:
                                        if (actor.Renown >= effect.effectValue)
                                        { actor.Renown -= effect.effectValue; }
                                        effectReturn.bottomText = string.Format("{0}{1} {2}{3}", colourOutcome1, actor.Name, effect.description, colourEnd);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Debug.LogError(string.Format("Invalid Actor (null) for EffectOutcome \"{0}\"", effect.effectOutcome));
                            effectReturn.errorFlag = true;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.effectOutcome));
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
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.SlotID, node);
                    //return texts
                    effectReturn.topText = string.Format("{0}Civil Team inserted{1}", colourDefault, colourEnd);
                    effectReturn.bottomText = string.Format("{0}{1} has deployed {2} of {3} teams{4}", colourOutcome3, actor.Arc.name, 
                        actor.CheckNumOfTeams(), actor.Datapoint2, colourEnd);
                    break;
                case EffectOutcome.ControlTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Control");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.SlotID, node);
                    //return texts
                    effectReturn.topText = string.Format("{0}Control Team inserted{1}", colourDefault, colourEnd);
                    effectReturn.bottomText = string.Format("{0}{1} has deployed {2} of {3} teams{4}", colourOutcome3, actor.Arc.name,
                        actor.CheckNumOfTeams(), actor.Datapoint2, colourEnd);
                    break;
                case EffectOutcome.DamageTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Damage");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.SlotID, node);
                    //return texts
                    effectReturn.topText = string.Format("{0}Damage Team inserted{1}", colourDefault, colourEnd);
                    effectReturn.bottomText = string.Format("{0}{1} has deployed {2} of {3} teams{4}", colourOutcome3, actor.Arc.name,
                        actor.CheckNumOfTeams(), actor.Datapoint2, colourEnd);
                    break;
                case EffectOutcome.ErasureTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Erasure");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.SlotID, node);
                    //return texts
                    effectReturn.topText = string.Format("{0}Erasure Team inserted{1}", colourDefault, colourEnd);
                    effectReturn.bottomText = string.Format("{0}{1} has deployed {2} of {3} teams{4}", colourOutcome3, actor.Arc.name,
                        actor.CheckNumOfTeams(), actor.Datapoint2, colourEnd);
                    break;
                case EffectOutcome.MediaTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Media");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.SlotID, node);
                    //return texts
                    effectReturn.topText = string.Format("{0}Media Team inserted{1}", colourDefault, colourEnd);
                    effectReturn.bottomText = string.Format("{0}{1} has deployed {2} of {3} teams{4}", colourOutcome3, actor.Arc.name,
                        actor.CheckNumOfTeams(), actor.Datapoint2, colourEnd);
                    break;
                case EffectOutcome.ProbeTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Probe");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.SlotID, node);
                    //return texts
                    effectReturn.topText = string.Format("{0}Probe Team inserted{1}", colourDefault, colourEnd);
                    effectReturn.bottomText = string.Format("{0}{1} has deployed {2} of {3} teams{4}", colourOutcome3, actor.Arc.name,
                        actor.CheckNumOfTeams(), actor.Datapoint2, colourEnd);
                    break;
                case EffectOutcome.SpiderTeam:
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("Spider");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.SlotID, node);
                    //return texts
                    effectReturn.topText = string.Format("{0}Spider Team inserted{1}", colourDefault, colourEnd);
                    effectReturn.bottomText = string.Format("{0}{1} has deployed {2} of {3} teams{4}", colourOutcome3, actor.Arc.name,
                        actor.CheckNumOfTeams(), actor.Datapoint2, colourEnd);
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.effectOutcome));
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


    //place methods above here
}
