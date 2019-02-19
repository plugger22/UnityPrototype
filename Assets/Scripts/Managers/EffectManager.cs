using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using packageAPI;
using System.Text;
using System;
using Random = UnityEngine.Random;

/// <summary>
/// handles Effect related matters (Actions and Targets)
/// Effects are assumed to generate text and outcomes for a Modal Outcome window
/// </summary>
public class EffectManager : MonoBehaviour
{
    [Tooltip("How long do ongoing effects last for? Global setting")]
    [Range(3,20)] public int ongoingEffectTimer = 10;

    //fast access -> spiders
    private int delayNoSpider;
    private int delayYesSpider;
    //fast access -> teams
    private int teamArcCivil = -1;
    private int teamArcControl = -1;
    private int teamArcMedia = -1;
    private int teamArcProbe = -1;
    private int teamArcSpider = -1;
    private int teamArcDamage = -1;
    private int teamArcErasure = -1;
    //fast access -> traits
    private int actorStressedOverInvisibility;
    private int actorDoubleRenown;
    private int actorBlackmailNone;
    private int actorConflictPoison;
    private int actorConflictKill;
    private int actorNeverResigns;
    private int actorReserveTimerDoubled;
    private int actorReserveTimerHalved;

    //fast access -> conditions
    private Condition conditionStressed;
    private Condition conditionCorrupt;
    private Condition conditionIncompetent;
    private Condition conditionQuestionable;
    private Condition conditionBlackmailer;
    private Condition conditionStar;
    private Condition conditionTagged;
    

    //colour palette for Modal Outcome
    private string colourGoodSide; //good effect Resisance / bad effect Authority
    private string colourBadSide; //bad effect Authority / bad effect Resistance
    private string colourGood;  //standard colour Good
    private string colourBad;   //standard colour Bad
    private string colourNeutral; //used when node is EqualsTo, eg. reset, or for Team Names
    private string colourNormal;
    private string colourDefault;
    private string colourAlert;
    private string colourActor;
    private string colourEnd;

    [HideInInspector] private static int ongoingEffectIDCounter = 0;              //used to sequentially number ongoing Effect ID's

    public void Initialise()
    {
        //fast access
        delayNoSpider = GameManager.instance.nodeScript.nodeNoSpiderDelay;
        delayYesSpider = GameManager.instance.nodeScript.nodeYesSpiderDelay;
        actorStressedOverInvisibility = GameManager.instance.dataScript.GetTraitEffectID("ActorInvisibilityStress");
        actorDoubleRenown = GameManager.instance.dataScript.GetTraitEffectID("ActorDoubleRenown");
        actorBlackmailNone = GameManager.instance.dataScript.GetTraitEffectID("ActorBlackmailNone");
        actorConflictPoison = GameManager.instance.dataScript.GetTraitEffectID("ActorConflictPoison");
        actorConflictKill = GameManager.instance.dataScript.GetTraitEffectID("ActorConflictKill");
        actorNeverResigns = GameManager.instance.dataScript.GetTraitEffectID("ActorResignNone");
        actorReserveTimerDoubled = GameManager.instance.dataScript.GetTraitEffectID("ActorReserveTimerDoubled");
        actorReserveTimerHalved = GameManager.instance.dataScript.GetTraitEffectID("ActorReserveTimerHalved");
        conditionStressed = GameManager.instance.dataScript.GetCondition("STRESSED");
        conditionCorrupt = GameManager.instance.dataScript.GetCondition("CORRUPT");
        conditionIncompetent = GameManager.instance.dataScript.GetCondition("INCOMPETENT");
        conditionQuestionable = GameManager.instance.dataScript.GetCondition("QUESTIONABLE");
        conditionBlackmailer = GameManager.instance.dataScript.GetCondition("BLACKMAILER");
        conditionStar = GameManager.instance.dataScript.GetCondition("STAR");
        conditionTagged = GameManager.instance.dataScript.GetCondition("TAGGED");
        Debug.Assert(actorStressedOverInvisibility > -1, "Invalid actorStressedOverInvisibility (-1)");
        Debug.Assert(actorDoubleRenown > -1, "Invalid actorDoubleRenown (-1)");
        Debug.Assert(actorBlackmailNone > -1, "Invalid actorBlackmailNone (-1)");
        Debug.Assert(actorConflictPoison > -1, "Invalid actorPoisonYes (-1)");
        Debug.Assert(actorConflictKill > -1, "Invalid actorConflictKill (-1)");
        Debug.Assert(actorNeverResigns > -1, "Invalid actorNeverResigns (-1)");
        Debug.Assert(actorReserveTimerDoubled > -1, "Invalid actorReserveTimerDoubled (-1) ");
        Debug.Assert(actorReserveTimerHalved > -1, "Invalid actorReserveTimerHalved (-1) ");
        Debug.Assert(conditionStressed != null, "Invalid conditionStressed (Null)");
        Debug.Assert(conditionCorrupt != null, "Invalid conditionCorrupt (Null)");
        Debug.Assert(conditionIncompetent != null, "Invalid conditionIncompetent (Null)");
        Debug.Assert(conditionQuestionable != null, "Invalid conditionQuestionable (Null)");
        Debug.Assert(conditionBlackmailer != null, "(Invalid conditionBlackmailer (Null)");
        Debug.Assert(conditionStar != null, "Invalid conditionStar (Null)");
        Debug.Assert(conditionTagged != null, "Invalid conditionTagged (Null)");
        //fast access -> teams
        teamArcCivil = GameManager.instance.dataScript.GetTeamArcID("CIVIL");
        teamArcControl = GameManager.instance.dataScript.GetTeamArcID("CONTROL");
        teamArcMedia = GameManager.instance.dataScript.GetTeamArcID("MEDIA");
        teamArcProbe = GameManager.instance.dataScript.GetTeamArcID("PROBE");
        teamArcSpider = GameManager.instance.dataScript.GetTeamArcID("SPIDER");
        teamArcDamage = GameManager.instance.dataScript.GetTeamArcID("DAMAGE");
        teamArcErasure = GameManager.instance.dataScript.GetTeamArcID("ERASURE");
        Debug.Assert(teamArcCivil > -1, "Invalid teamArcCivil");
        Debug.Assert(teamArcControl > -1, "Invalid teamArcControl");
        Debug.Assert(teamArcMedia > -1, "Invalid teamArcMedia");
        Debug.Assert(teamArcProbe > -1, "Invalid teamArcProbe");
        Debug.Assert(teamArcSpider > -1, "Invalid teamArcSpider");
        Debug.Assert(teamArcDamage > -1, "Invalid teamArcDamage");
        Debug.Assert(teamArcErasure > -1, "Invalid teamArcErasure");
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "EffectManager");
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
        //output colours for good and bad depend on player side
        switch (GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Resistance":
                colourGoodSide = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
                colourBadSide = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
                break;
            case "Authority":
                colourGoodSide = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
                colourBadSide = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                break;
        }
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourActor = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// checks whether effect criteria is valid. Returns "Null" if O.K and a tooltip explanation string if not. Player side only.
    /// Can be used independently of Effects provided you have a listOfCriteria
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public string CheckCriteria(CriteriaDataInput data)
    {
        int val;
        StringBuilder result = new StringBuilder();
        string compareTip = null;
        Node node = null;
        bool errorFlag = false;
        Actor actor = null;
        TeamArc teamArc = null;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        if (data.listOfCriteria != null && data.listOfCriteria.Count > 0)
        {
            //
            // - - - access necessary data prior to loop
            //
            //Get node if required (eg., if "CurrentActor" then not)
            if (data.nodeID > -1)
            {
                node = GameManager.instance.dataScript.GetNode(data.nodeID);
                if (node == null)
                { Debug.LogError("Invalid node (null)"); errorFlag = true; }
            }
            //Get actor if required
            if (data.actorSlotID != -1)
            {
                //get actor
                actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, playerSide);
                if (actor == null)
                {
                    Debug.LogError("Invalid actorSlotID -> Criteria Check cancelled");
                    errorFlag = true;
                }
            }
            //Get TeamArc if required
            if (data.teamArcID > -1)
            {
                //get team
                teamArc = GameManager.instance.dataScript.GetTeamArc(data.teamArcID);
                if (teamArc == null)
                {
                    Debug.LogError(string.Format("Invalid TeamArc (null) for teamArcID \"{0}\" -> Criteria check cancelled", data.teamArcID));
                    errorFlag = true;
                }
            }
            //O.K to proceed?
            if (errorFlag == false)
            {
                foreach (Criteria criteria in data.listOfCriteria)
                {
                    if (criteria != null)
                    {
                        if (criteria.apply != null)
                        {
                            switch (criteria.apply.name)
                            {
                                //
                                // - - - Current Node - - -
                                //
                                case "NodeCurrent":
                                    if (node != null)
                                    {
                                        if (criteria.effectCriteria != null)
                                        {
                                            switch (criteria.effectCriteria.name)
                                            {
                                                case "NodeSecurityMin":
                                                    val = GameManager.instance.nodeScript.minNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Security, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Security " + compareTip); }
                                                    break;
                                                case "NodeStabilityMin":
                                                    val = GameManager.instance.nodeScript.minNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Stability, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Stability " + compareTip); }
                                                    break;
                                                case "NodeSupportMax":
                                                    val = GameManager.instance.nodeScript.maxNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Support, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Support " + compareTip); }
                                                    break;
                                                case "NumTeamsMin":
                                                    val = GameManager.instance.teamScript.minTeamsAtNode;
                                                    compareTip = ComparisonCheck(val, node.CheckNumOfTeams(), criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "no Teams present"); }
                                                    break;
                                                case "NumTeamsMax":
                                                    val = GameManager.instance.teamScript.maxTeamsAtNode;
                                                    compareTip = ComparisonCheck(val, node.CheckNumOfTeams(), criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Max teams present"); }
                                                    break;
                                                case "NumTracers":
                                                    if (node.isTracer == true)
                                                    { BuildString(result, "Tracer already present"); }
                                                    break;
                                                case "TargetInfoMax":
                                                    val = GameManager.instance.targetScript.maxTargetInfo;
                                                    compareTip = ComparisonCheck(val, node.targetID, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Full Info already"); }
                                                    break;
                                                case "TargetPresent":
                                                    //check that a target is present at the node
                                                    if (node.targetID < 0)
                                                    { BuildString(result, "No Target present"); }
                                                    break;
                                                case "TeamActorAbility":
                                                    //actor can only have a number of teams OnMap equal to their ability at any time
                                                    if (actor != null)
                                                    {
                                                        if (actor.CheckNumOfTeams() >= actor.datapoint2)
                                                        { BuildString(result, "Actor Ability exceeded"); }
                                                    }
                                                    else
                                                    { Debug.LogError(string.Format("Invalid Actor (Null) for criteria \"{0}\"", criteria.name)); errorFlag = true; }
                                                    break;
                                                case "TeamIdentical":
                                                    //there can only be one team of a type at a node
                                                    if (node.CheckTeamPresent(data.teamArcID) > -1)
                                                    { BuildString(result, string.Format(" {0} Team already present", teamArc.name)); }
                                                    break;
                                                case "TeamPreferred":
                                                    //there must be a spare team in the reserve pool of the actors preferred type
                                                    if (teamArc != null)
                                                    {
                                                        if (GameManager.instance.dataScript.CheckTeamInfo(data.teamArcID, TeamInfo.Reserve) < 1)
                                                        { BuildString(result, string.Format("No {0} Team available", teamArc.name)); }
                                                    }
                                                    else
                                                    { Debug.LogError(string.Format("Invalid teamArc (null) for criteria \"{0}\"", criteria.name)); errorFlag = true; }
                                                    break;
                                                case "TeamAny":
                                                    //there must be a spare team of any type in the reserve pool
                                                    if (teamArc != null)
                                                    {
                                                        if (GameManager.instance.dataScript.CheckTeamPoolCount(TeamPool.Reserve) < 1)
                                                        { BuildString(result, string.Format("No Teams available", teamArc.name)); }
                                                    }
                                                    else
                                                    { Debug.LogError(string.Format("Invalid teamArc (null) for criteria \"{0}\"", criteria.name)); errorFlag = true; }
                                                    break;
                                                case "TeamCivilNo":
                                                    //A Civil team can't be present
                                                    if (node != null)
                                                    {
                                                        if (node.CheckTeamPresent(teamArcCivil) > -1)
                                                        { BuildString(result, string.Format("{0}CIVIL team present{1}", colourBadSide, colourEnd)); }
                                                    }
                                                    break;
                                                case "TeamProbeNo":
                                                    //A Probe team can't be present
                                                    if (node != null)
                                                    {
                                                        if (node.CheckTeamPresent(teamArcProbe) > -1)
                                                        { BuildString(result, string.Format("{0}PROBE team present{1}", colourBadSide, colourEnd)); }
                                                    }
                                                    break;
                                                case "TeamControlNo":
                                                    //A Control team can't be present
                                                    if (node != null)
                                                    {
                                                        if (node.CheckTeamPresent(teamArcControl) > -1)
                                                        { BuildString(result, string.Format("{0}CONTROL team present{1}", colourBadSide, colourEnd)); }
                                                    }
                                                    break;
                                                case "TeamSpiderNo":
                                                    //A Spider team can't be present
                                                    if (node != null)
                                                    {
                                                        if (node.CheckTeamPresent(teamArcSpider) > -1)
                                                        { BuildString(result, string.Format("{0}SPIDER team present{1}", colourBadSide, colourEnd)); }
                                                    }
                                                    break;
                                                case "TeamMediaNo":
                                                    //A Media team can't be present
                                                    if (node != null)
                                                    {
                                                        if (node.CheckTeamPresent(teamArcMedia) > -1)
                                                        { BuildString(result, string.Format("{0}MEDIA team present{1}", colourBadSide, colourEnd)); }
                                                    }
                                                    break;
                                                default:
                                                    BuildString(result, "Error!");
                                                    Debug.LogWarning(string.Format("NodeCurrent: Invalid criteriaEffect \"{0}\"", criteria.effectCriteria.name));
                                                    errorFlag = true;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogError(string.Format("Invalid criteria.effectCriteria (Null) for criteria {0}", criteria.name));
                                            errorFlag = true;
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError(string.Format("Invalid node (null) for criteria \"{0}\"", criteria.name));
                                        errorFlag = true;
                                    }
                                    break;
                                //
                                // - - - Neighbouring nodes - - -
                                //
                                case "NodeNeighbours":
                                    if (node != null)
                                    {
                                        if (criteria.effectCriteria != null)
                                        {
                                            switch (criteria.effectCriteria.name)
                                            {
                                                case "NodeStabilityMin":
                                                    //at least one neighbouring node must have stability > 0
                                                    val = GameManager.instance.nodeScript.minNodeValue;
                                                    List<Node> listOfNeighbouringNodes = node.GetNeighbouringNodes();
                                                    if (listOfNeighbouringNodes != null)
                                                    {
                                                        bool atLeastOneOK = false;
                                                        //loop neighbouring nodes (current node excluded)
                                                        foreach (Node nearNode in listOfNeighbouringNodes)
                                                        {
                                                            compareTip = ComparisonCheck(val, nearNode.Stability, criteria.comparison);
                                                            if (compareTip == null)
                                                            { atLeastOneOK = true; break; }
                                                        }
                                                        if (atLeastOneOK == false)
                                                        { BuildString(result, "Near Nodes Stability > 0"); }
                                                    }
                                                    else { Debug.LogError("Invalid listOfNeighbouringNodes (Null)"); errorFlag = true; }
                                                    break;
                                                default:
                                                    BuildString(result, "Error!");
                                                    Debug.LogWarning(string.Format("NodeNeighbours: Invalid effect.criteriaEffect \"{0}\"", criteria.effectCriteria.name));
                                                    errorFlag = true;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogError(string.Format("Invalid criteria.effectCriteria (Null) for criteria {0}", criteria.name));
                                            errorFlag = true;
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError(string.Format("Invalid node (null) for criteria \"{0}\"", criteria.name));
                                        errorFlag = true;
                                    }
                                    break;
                                //
                                // - - - Current Actor / Player - - -
                                //
                                case "ActorCurrent":
                                    if (criteria.effectCriteria != null)
                                    {
                                        int playerRenown;
                                        switch (criteria.effectCriteria.name)
                                        {
                                            case "ConditionStressedYes":
                                                //actor has the 'Stressed' condition
                                                if (conditionStressed != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        if (actor.CheckConditionPresent(conditionStressed) == false)
                                                        { BuildString(result, string.Format(" {0} isn't {1}STRESSED{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.instance.playerScript.CheckConditionPresent(conditionStressed, playerSide) == false)
                                                        { BuildString(result, string.Format(" Player isn't {0}STRESSED{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionStressed (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionStressedNo":
                                                //actor / player does NOT have the 'Stressed' condition
                                                if (conditionStressed != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        //actor
                                                        if (actor.CheckConditionPresent(conditionStressed) == true)
                                                        { BuildString(result, string.Format(" {0} is {1}STRESSED{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.instance.playerScript.CheckConditionPresent(conditionStressed, playerSide) == true)
                                                        { BuildString(result, string.Format(" Player is {0}STRESSED{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionStressed (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionCorruptYes":
                                                if (conditionCorrupt != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        //actor
                                                        if (actor.CheckConditionPresent(conditionCorrupt) == false)
                                                        { BuildString(result, string.Format(" {0} isn't {1}CORRUPT{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.instance.playerScript.CheckConditionPresent(conditionCorrupt, playerSide) == false)
                                                        { BuildString(result, string.Format(" Player isn't {0}CORRUPT{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionCorrupt (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionIncompetentYes":
                                                if (conditionIncompetent != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        //actor
                                                        if (actor.CheckConditionPresent(conditionIncompetent) == false)
                                                        { BuildString(result, string.Format(" {0} isn't {1}INCOMPETENT{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.instance.playerScript.CheckConditionPresent(conditionIncompetent, playerSide) == false)
                                                        { BuildString(result, string.Format(" Player isn't {0}INCOMPETENT{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionIncompetent (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionQuestionableYes":
                                                if (conditionQuestionable != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        if (actor.CheckConditionPresent(conditionQuestionable) == false)
                                                        { BuildString(result, string.Format(" {0} isn't {1}QUESTIONABLE{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.instance.playerScript.CheckConditionPresent(conditionQuestionable, playerSide) == false)
                                                        { BuildString(result, string.Format(" Player isn't {0}QUESTIONABLE{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionQuestionable (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionStarYes":
                                                if (conditionStar != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        if (actor.CheckConditionPresent(conditionStar) == false)
                                                        { BuildString(result, string.Format(" {0} isn't a {1}STAR{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.instance.playerScript.CheckConditionPresent(conditionStar, playerSide) == false)
                                                        { BuildString(result, string.Format(" Player isn't a {0}STAR{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionStar (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionBlackmailerNo":
                                                //actor only  does NOT have the 'Blackmailer' condition
                                                if (conditionBlackmailer != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        //actor
                                                        if (actor.CheckConditionPresent(conditionBlackmailer) == true)
                                                        { BuildString(result, string.Format(" {0} already {1}BLACKMAILING{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    { Debug.LogWarning("Invalid actor (Null) for ConditionBlackmailerNo"); }
                                                }
                                                else { Debug.LogWarning("Invalid conditionStressed (Null)"); errorFlag = true; }
                                                break;
                                            case "TraitBlackmailNoneNo":
                                                if (actor != null)
                                                {
                                                    //actor -> doesn't have 'Honorable' trait
                                                    if (actor.CheckTraitEffect(actorBlackmailNone) == true)
                                                    { BuildString(result, string.Format(" {0} has {1}{2}{3}", actor.actorName, colourNeutral, actor.GetTrait().tag, colourEnd)); }
                                                }
                                                else
                                                { Debug.LogWarning("Invalid actor (Null) for TraitBlackmailNone"); }
                                                break;
                                            case "TraitConflictPoisonYes":
                                                if (actor != null)
                                                {
                                                    //actor -> HAS 'Snake' trait
                                                    if (actor.CheckTraitEffect(actorConflictPoison) == false)
                                                    { BuildString(result, string.Format(" {0} doesn't have required trait", actor.actorName)); }
                                                }
                                                else
                                                { Debug.LogWarning("Invalid actor (Null) for TraitConflictPoisonYes"); }
                                                break;
                                            case "TraitConflictKillYes":
                                                if (actor != null)
                                                {
                                                    //actor -> HAS 'Psychopath' trait
                                                    if (actor.CheckTraitEffect(actorConflictKill) == false)
                                                    { BuildString(result, string.Format(" {0} doesn't have required trait", actor.actorName));  }
                                                }
                                                else
                                                { Debug.LogWarning("Invalid actor (Null) for TraitConflictKillYes"); }
                                                break;
                                            case "RenownReserveMin":
                                                //player
                                                int renownReserve = GameManager.instance.actorScript.manageReserveRenown;
                                                playerRenown = GameManager.instance.playerScript.Renown;
                                                if (playerRenown < renownReserve)
                                                {
                                                    BuildString(result, string.Format("You need at least {0}{1}{2}{3} Renown {4}(currently {5}{6}{7})", "\n", colourNeutral, renownReserve,
                                                      colourEnd, "\n", colourNeutral, playerRenown, colourEnd));
                                                }
                                                break;
                                            case "RenownDismissMin":
                                                //player -> extra cost for actor knowing secrets and threatening player
                                                ManageRenownCost manageDismissCost = GameManager.instance.actorScript.GetManageRenownCost(actor, GameManager.instance.actorScript.manageDismissRenown);
                                                int renownDismiss = manageDismissCost.renownCost;
                                                playerRenown = GameManager.instance.playerScript.Renown;
                                                if (playerRenown < renownDismiss)
                                                {
                                                    BuildString(result, string.Format("You need at least {0}{1}{2}{3} Renown {4}(currently {5}{6}{7})", "\n", colourNeutral, renownDismiss,
                                                      colourEnd, "\n", colourNeutral, playerRenown, colourEnd));
                                                    if (String.IsNullOrEmpty(manageDismissCost.tooltip) == false)
                                                    { result.Append(manageDismissCost.tooltip); }
                                                }
                                                break;
                                            case "RenownDisposeMin":
                                                //player -> extra cost for actor knowing secrets and threatening player
                                                ManageRenownCost manageDisposeCost = GameManager.instance.actorScript.GetManageRenownCost(actor, GameManager.instance.actorScript.manageDisposeRenown);
                                                int renownDispose = manageDisposeCost.renownCost;
                                                playerRenown = GameManager.instance.playerScript.Renown;
                                                if (playerRenown < renownDispose)
                                                {
                                                    BuildString(result, string.Format("You need at least {0}{1}{2}{3} Renown {4}(currently {5}{6}{7})", "\n", colourNeutral, renownDispose,
                                                      colourEnd, "\n", colourNeutral, playerRenown, colourEnd));
                                                    if (String.IsNullOrEmpty(manageDisposeCost.tooltip) == false)
                                                    { result.Append(manageDisposeCost.tooltip); }
                                                }
                                                break;
                                            case "RenownNOTZero":
                                                //Player / Actor
                                                playerRenown = GameManager.instance.playerScript.Renown;
                                                if (playerRenown == 0)
                                                { BuildString(result, "Renown Zero"); }
                                                break;
                                            case "NumRecruitsCurrent":
                                                //check max. number of recruits in reserve pool not exceeded
                                                val = GameManager.instance.dataScript.CheckNumOfActorsInReserve();
                                                compareTip = ComparisonCheck(GameManager.instance.actorScript.maxNumOfReserveActors, val, criteria.comparison);
                                                if (compareTip != null)
                                                { BuildString(result, "maxxed Recruit allowance"); }
                                                break;
                                            case "InvisibilityNOTMax":
                                                //check invisibility is less than the max value -> Actor / Player
                                                if (actor != null)
                                                { val = actor.datapoint2; }
                                                else { val = GameManager.instance.playerScript.Invisibility; }
                                                if (val == GameManager.instance.actorScript.maxStatValue)
                                                { BuildString(result, "Invisibility at Max"); }
                                                break;
                                            case "InvisibilityNOTZero":
                                                //check invisibility greater than Zero -> Actor / Player
                                                if (actor != null)
                                                { val = actor.datapoint2; }
                                                else { val = GameManager.instance.playerScript.Invisibility; }
                                                if (val == 0)
                                                { BuildString(result, "Invisibility Zero"); }
                                                break;
                                            case "SecretsNOTZero":
                                                //check num of secrets greater than Zero -> Actor / Player
                                                if (actor != null)
                                                { val = actor.CheckNumOfSecrets(); }
                                                else { val = GameManager.instance.playerScript.CheckNumOfSecrets(); }
                                                if (val == 0)
                                                { BuildString(result, "Secrets Zero"); }
                                                break;
                                            case "NumGearMax":
                                                //Note: effect criteria value is ignored in this case
                                                val = GameManager.instance.gearScript.maxNumOfGear;
                                                compareTip = ComparisonCheck(val, GameManager.instance.playerScript.CheckNumOfGear(), criteria.comparison);
                                                if (compareTip != null)
                                                { BuildString(result, "maxxed Gear Allowance"); }
                                                break;
                                            case "GearAvailability":
                                                //checks to see if at least 1 piece of unused common gear is available
                                                List<int> tempCommonGear = new List<int>(GameManager.instance.dataScript.GetListOfGear(GameManager.instance.gearScript.gearCommon));
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
                                            default:
                                                BuildString(result, "Error!");
                                                Debug.LogWarning(string.Format("ActorCurrent: Invalid effect.criteriaEffect \"{0}\"", criteria.effectCriteria.name));
                                                errorFlag = true;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError(string.Format("Invalid criteria.effectCriteria (Null) for criteria {0}", criteria.name));
                                        errorFlag = true;
                                    }
                                    break;
                                default:
                                    BuildString(result, "Error!");
                                    Debug.LogWarning(string.Format("Invalid effect.criteria.apply \"{0}\"", criteria.apply.name));
                                    errorFlag = true;
                                    break;
                            }
                        }
                        else
                        {
                            Debug.LogError(string.Format("Invalid criteria.apply (null) for criteria {0}", criteria.name));
                            errorFlag = true;
                        }
                    }
                    else
                    {
                        Debug.LogError("Invalid criteria (Null)");
                        errorFlag = true;
                    }
                    //exit on error
                    if (errorFlag == true)
                    { break; }
                }
            }
        }
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
    private string ComparisonCheck(int criteriaValue, int actualValue, EffectOperator comparison)
    {
        string result = null;
        switch (comparison.name)
        {
            case "LessThan":
                if (actualValue >= criteriaValue)
                { result = string.Format("< {0}, currently {1}", criteriaValue, actualValue); }
                break;
            case "GreaterThan":
                if (actualValue <= criteriaValue)
                { result = string.Format("> {0}, currently {1}", criteriaValue, actualValue); }
                break;
            case "EqualTo":
                if (criteriaValue != actualValue)
                { result = string.Format("{0}, currently {1}", criteriaValue, actualValue); }
                break;
            default:
                result = "Error!";
                Debug.LogError(string.Format("Invalid Comparison \"{0}\"", comparison.name));
                break;
        }
        return result;
    }


    /// <summary>
    /// Processes effects and returns results in a class. Leave actor as Null for (Resistance?) Player effect
    /// Use player node if an issue but check to see if node is used (often to determine who is affected, player or actor)
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="node"></param>
    /// <param name="dataInput">data package from calling method</param>
    /// <param name="actor">if Resistance and Player effects then go with default actor = null, otherwise supply an actor</param>
    /// <returns></returns>
    public EffectDataReturn ProcessEffect(Effect effect, Node node, EffectDataInput dataInput, Actor actor = null)
    {
        int teamID, teamArcID, dataBefore;
        EffectDataReturn effectReturn = new EffectDataReturn();
        //set default values
        effectReturn.errorFlag = false;
        effectReturn.topText = "";
        effectReturn.bottomText = "";
        effectReturn.isAction = false;
        //valid effect?
        if (effect != null)
        {
            //sort out colour based on type (which is effect benefit from POV of Resistance)
            string colourEffect = colourDefault;
            string colourText = colourDefault;
            if (effect.typeOfEffect != null)
            {
                switch (effect.typeOfEffect.level)
                {
                    case 2:
                        //good
                        colourEffect = colourGoodSide;
                        break;
                    case 1:
                        //neutral
                        colourEffect = colourNeutral;
                        colourText = colourNeutral;
                        break;
                    case 0:
                        //bad
                        colourEffect = colourBadSide;
                        colourText = colourAlert;
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                        break;
                }
            }
            else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name)); }
            //what's affected?
            switch (effect.outcome.name)
            {
                //
                // - - - Node Data effects (both sides)
                //
                case "StatusSpiders":
                case "StatusTracers":
                case "StatusTeams":
                case "StatusContacts":
                case "NodeSecurity":
                case "NodeStability":
                case "NodeSupport":
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
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                    }
                    break;
                //
                // - - - Conditions (both sides) - - -
                //
                case "ConditionStressed":
                case "ConditionIncompetent":
                case "ConditionCorrupt":
                case "ConditionQuestionable":
                case "ConditionBlackmailer":
                case "ConditionStar":
                case "ConditionGroupBad":
                case "ConditionGroupGood":
                    if (node != null)
                    {
                        //it's OK to pass a null actor provided it's a player condition
                        EffectDataResolve resolve = ResolveConditionData(effect, node, dataInput, actor);
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
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                    }
                    break;
                //
                // - - - Player Actions - - -
                //
                case "PlayerActions":
                    EffectDataResolve resolvePlayer = ResolvePlayerData(effect, dataInput);
                    if (resolvePlayer.isError == true)
                    { effectReturn.errorFlag = true; }
                    else
                    {
                        effectReturn.topText = resolvePlayer.topText;
                        effectReturn.bottomText = resolvePlayer.bottomText;
                        effectReturn.isAction = true;
                    }
                    break;
                //
                // - - - Manage - - -
                //
                case "ActorDismissed":
                case "ActorDisposedOff":
                case "ActorPromoted":
                case "ActorToReserves":
                case "ManageDismissRenown":
                case "ManageDisposeRenown":
                case "ManageReserveRenown":
                case "UnhappyTimerCurrent":
                case "ActorPromised":
                    if (actor != null)
                    {
                        EffectDataResolve resolve = ResolveManageData(effect, actor);
                        if (resolve.isError == true)
                        { effectReturn.errorFlag = true; }
                        else
                        {
                            effectReturn.topText = resolve.topText;
                            effectReturn.bottomText = resolve.bottomText;
                            effectReturn.isAction = true;
                        }
                    }
                    break;
                //
                // - - - Special Actor Effects - - -
                //
                case "ActorResigns":
                case "ActorKills":
                    if (actor != null)
                    {
                        EffectDataResolve resolve = ResolveSpecialActorEffect(effect, actor);
                        if (resolve.isError == true)
                        { effectReturn.errorFlag = true; }
                        else
                        {
                            effectReturn.topText = resolve.topText;
                            effectReturn.bottomText = resolve.bottomText;
                            effectReturn.isAction = true;
                        }
                    }
                    break;

                //
                // - - - Secrets - - -
                //
                case "Secret":
                    //Remove a random secret from Player (removes all instances) or Actor (removes local instance only)
                    if (actor == null)
                    {
                        //Player
                        Secret secret = GameManager.instance.playerScript.GetRandomCurrentSecret();
                        if (secret != null)
                        {
                            secret.status = GameManager.instance.secretScript.secretStatusDeleted;
                            secret.deletedWhen = GameManager.instance.turnScript.Turn;
                            //remove secret from game
                            if (GameManager.instance.secretScript.RemoveSecretFromAll(secret.secretID, true) == true)
                            { effectReturn.bottomText = string.Format("{0}\"{1}\" secret deleted{2}", colourGood, secret.tag, colourEnd); }
                            else { effectReturn.bottomText = string.Format("{0}\"{1}\" secret NOT deleted{2}", colourBad, secret.tag, colourEnd); }
                            effectReturn.isAction = true;
                        }
                    }
                    else
                    {
                        //Actor
                        Secret secret = actor.GetRandomCurrentSecret();
                        if (secret != null)
                        {
                            //removes secret from actor only (could still be with other actors and will always be with the player)
                            actor.RemoveSecret(secret.secretID);
                            effectReturn.bottomText = string.Format("{0}\"{1}\" secret deleted from {2}{3}", colourGood, secret.tag, actor.arc.name, colourEnd);
                            effectReturn.isAction = true;
                        }
                    }
                    break;
                //
                // - - - Other - - -
                //
                case "Recruit":
                    //no effect, handled directly elsewhere (check ActorManager.cs -> GetNodeActions
                    break;
                case "ConnectionSecurity":
                    if (node != null)
                    {
                        EffectDataResolve resolve = ResolveConnectionData(effect, node, dataInput);
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
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                    }
                    break;
                case "Motivation":
                    switch (effect.operand.name)
                    {
                        case "Add":
                            switch (effect.apply.name)
                            {
                                case "ActorCurrent":
                                    actor.datapoint1 += effect.value;
                                    actor.datapoint1 = Mathf.Min(GameManager.instance.actorScript.maxStatValue, actor.datapoint1);
                                    effectReturn.bottomText = string.Format("{0}{1} {2}{3}", colourEffect, actor.arc.name, effect.description, colourEnd);
                                    break;
                                case "ActorAll":
                                    //all actors have their motivation raised
                                    ResolveGroupActorEffect(effect, actor);
                                    effectReturn.bottomText = string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd);
                                    break;
                                default:
                                    Debug.LogWarningFormat("Invalid effect.apply \"{0}\"", effect.apply.name);
                                    break;
                            }
                            break;
                        case "Subtract":
                            switch (effect.apply.name)
                            {
                                case "ActorCurrent":
                                    actor.datapoint1 -= effect.value;
                                    actor.datapoint1 = Mathf.Max(0, actor.datapoint1);
                                    effectReturn.bottomText = string.Format("{0}{1} {2}{3}", colourEffect, actor.arc.name, effect.description, colourEnd);
                                    break;
                                case "ActorAll":
                                    //all actors have their motivation lowered
                                    ResolveGroupActorEffect(effect, actor);
                                    effectReturn.bottomText = string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd);
                                    break;
                                default:
                                    Debug.LogWarningFormat("Invalid effect.apply \"{0}\"", effect.apply.name);
                                    break;
                            }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                            effectReturn.errorFlag = true;
                            break;
                    }

                    break;
                case "CityLoyalty":
                    //depends on player side
                    int cityLoyalty = GameManager.instance.cityScript.CityLoyalty;
                    switch (dataInput.side.level)
                    {
                        case 1:
                            //Authority
                            switch (effect.typeOfEffect.level)
                            {
                                case 2:
                                    //Good 
                                    cityLoyalty += effect.value;
                                    cityLoyalty = Mathf.Min(GameManager.instance.cityScript.maxCityLoyalty, cityLoyalty);
                                    GameManager.instance.cityScript.CityLoyalty = cityLoyalty;
                                    effectReturn.topText = string.Format("{0}The City grows closer to Authority{1}", colourText, colourEnd);
                                    effectReturn.bottomText = string.Format("{0}City Loyalty +{1}{2}", colourGood, effect.value, colourEnd);
                                    break;
                                case 0:
                                    //Bad 
                                    cityLoyalty -= effect.value;
                                    cityLoyalty = Mathf.Max(0, cityLoyalty);
                                    GameManager.instance.cityScript.CityLoyalty = cityLoyalty;
                                    effectReturn.topText = string.Format("{0}The City grows closer to the Resistance{1}", colourText, colourEnd);
                                    effectReturn.bottomText = string.Format("{0}City Loyalty -{1}{2}", colourBad, effect.value, colourEnd);
                                    break;
                                default:
                                    Debug.LogWarningFormat("Invalid typeOfEffect \"{0}\"", effect.typeOfEffect.name);
                                    effectReturn.errorFlag = true;
                                    break;
                            }
                            break;
                        case 2:
                            //Resistance
                            switch (effect.typeOfEffect.level)
                            {
                                case 2:
                                    //Good
                                    cityLoyalty -= effect.value;
                                    cityLoyalty = Mathf.Max(0, cityLoyalty);
                                    GameManager.instance.cityScript.CityLoyalty = cityLoyalty;
                                    effectReturn.topText = string.Format("{0}The City grows closer to the Resistance{1}", colourText, colourEnd);
                                    effectReturn.bottomText = string.Format("{0}City Loyalty -{1}{2}", colourGood, effect.value, colourEnd);
                                    break;
                                case 0:
                                    //Bad
                                    cityLoyalty += effect.value;
                                    cityLoyalty = Mathf.Min(GameManager.instance.cityScript.maxCityLoyalty, cityLoyalty);
                                    GameManager.instance.cityScript.CityLoyalty = cityLoyalty;
                                    effectReturn.topText = string.Format("{0}The City grows closer to Authority{1}", colourText, colourEnd);
                                    effectReturn.bottomText = string.Format("{0}City Loyalty +{1}{2}", colourBad, effect.value, colourEnd);
                                    break;
                                default:
                                    Debug.LogWarningFormat("Invalid typeOfEffect \"{0}\"", effect.typeOfEffect.name);
                                    effectReturn.errorFlag = true;
                                    break;
                            }
                            break;
                        default:
                            Debug.LogWarningFormat("Invalid side \"{0}\"", dataInput.side.name);
                            effectReturn.errorFlag = true;
                            break;
                    }
                    effectReturn.isAction = true;
                    break;
                case "FactionApproval":
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    GameManager.instance.factionScript.ChangeFactionApproval(effect.value, dataInput.side, dataInput.originText);
                                    effectReturn.topText = string.Format("{0}The {1} have a better opinion of you{2}", colourText,
                                        GameManager.instance.factionScript.factionAuthority.name, colourEnd);
                                    effectReturn.bottomText = string.Format("{0}Faction Approval +{1}{2}", colourGood, effect.value, colourEnd);
                                    break;
                                case "Subtract":
                                    GameManager.instance.factionScript.ChangeFactionApproval(effect.value, dataInput.side, dataInput.originText);
                                    effectReturn.topText = string.Format("{0}The {1}'s opinion of you has diminished{2}", colourText,
                                        GameManager.instance.factionScript.factionAuthority.name, colourEnd);
                                    effectReturn.bottomText = string.Format("{0}Faction Approval -{1}{2}", colourBad, effect.value, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                                    effectReturn.errorFlag = true;
                                    break;
                            }
                            effectReturn.isAction = true;
                            break;
                //
                // - - - Resistance effects
                //
                case "Tracer":
                    if (node != null)
                    {
                        node.AddTracer();

                        /*node.isTracerActive = true;
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
                        else { Debug.LogError("Invalid listOfNeighbours (Null)"); }*/

                        //dialogue
                        effectReturn.topText = string.Format("{0}A Tracer has been successfully inserted{1}", colourText, colourEnd);
                        effectReturn.bottomText = string.Format("{0}Any Spider at the district is revealed{1}", colourEffect, colourEnd);
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                    }
                    break;
                case "Gear":
                    //no effect, handled directly elsewhere (check ActorManager.cs -> GetNodeActions
                    break;
                case "TargetInfo":
                    //TO DO
                    break;
                case "NeutraliseTeam":
                    //no effect, handled directly elsewhere (check ActorManager.cs -> GetNodeActions
                    break;
                case "Invisibility":
                    //raise/lower invisibility of actor or Player
                    if (node != null)
                    {
                        bool isGearUsed = false;
                        string reason = dataInput.originText;
                        if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                        {
                            //
                            // - - - Player Invisibility effect - - -
                            //
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    if (GameManager.instance.playerScript.Invisibility < GameManager.instance.actorScript.maxStatValue)
                                    {
                                        //adds a variable amount
                                        int invis = GameManager.instance.playerScript.Invisibility;
                                        invis += effect.value;
                                        invis = Mathf.Min(GameManager.instance.actorScript.maxStatValue, invis);
                                        GameManager.instance.playerScript.Invisibility = invis;
                                    }
                                    effectReturn.bottomText = string.Format("{0}Player {1}{2}", colourEffect, effect.description, colourEnd);
                                    break;
                                case "Subtract":
                                    //does player have any invisibility type gear?
                                    int gearID = GameManager.instance.playerScript.CheckGearTypePresent(GameManager.instance.gearScript.typeInvisibility);
                                    if (gearID > -1)
                                    {
                                        //gear present -> No drop in Invisibility
                                        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                                        if (gear != null)
                                        {
                                            GameManager.instance.gearScript.SetGearUsed(gear, "stay Invisible");
                                            effectReturn.bottomText = string.Format("{0}{1}{2}{3} used to remain Invisible{4}", colourNeutral, gear.name.ToUpper(),
                                                colourEnd, colourNormal, colourEnd);
                                        }
                                        else { Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", gearID)); }
                                    }
                                    else
                                    {
                                        //No gear present
                                        int invisibility = GameManager.instance.playerScript.Invisibility;
                                        //condition TAGGED
                                        if (GameManager.instance.playerScript.CheckConditionPresent(conditionTagged, dataInput.side) == true)
                                        {
                                            //invisibility auto Zero
                                            invisibility = 0;
                                            //Immediate notification. AI flag set. Auto applies when TAGGED
                                            effectReturn.bottomText =
                                                string.Format("{0}Player Invisibility 0 (TAGGED){1}{2}{3}{4}<size=110%>Authority will know immediately</size>{5}",
                                                colourAlert, colourEnd, "\n", "\n", colourBadSide, colourEnd);
                                            reason = "TAGGED condition";
                                            GameManager.instance.aiScript.immediateFlagResistance = true;
                                        }
                                        //NOT Tagged
                                        else
                                        {
                                            //double effect if spider is present
                                            if (node.isSpider == true)
                                            {
                                                invisibility -= 2;
                                                if (invisibility >= 0)
                                                { effectReturn.bottomText = string.Format("{0}Player Invisibility -2 (Spider){1}", colourEffect, colourEnd); }
                                                else
                                                {
                                                    //Immediate notification. AI flag set. Applies even if player invis was 1 before action (spider effect)
                                                    effectReturn.bottomText =
                                                        string.Format("{0}Player Invisibility -2 (Spider){1}{2}{3}{4}<size=110%>Authority will know immediately</size>{5}",
                                                        colourAlert, colourEnd, "\n", "\n", colourBadSide, colourEnd);
                                                    GameManager.instance.aiScript.immediateFlagResistance = true;
                                                }
                                            }
                                            else
                                            {
                                                invisibility -= 1;
                                                if (invisibility >= 0)
                                                { effectReturn.bottomText = string.Format("{0}Player {1}{2}", colourEffect, effect.description, colourEnd); }
                                                else
                                                {
                                                    //immediate notification. AI flag set. Applies if player invis was 0 before action taken
                                                    effectReturn.bottomText = string.Format("{0}Player {1}{2}{3}{4}{5}<size=110%>Authority will know immediately</size>{6}",
                                                        colourAlert, effect.description, colourEnd, "\n", "\n", colourBadSide, colourEnd);
                                                    GameManager.instance.aiScript.immediateFlagResistance = true;
                                                }

                                            }
                                        }
                                        //mincap zero
                                        invisibility = Mathf.Max(0, invisibility);
                                        GameManager.instance.playerScript.Invisibility = invisibility;
                                    }
                                    //AI activity message
                                    if (isGearUsed == false)
                                    {
                                        int delay;
                                        if (node.isSpider == true) { delay = delayYesSpider; }
                                        else { delay = delayNoSpider; }
                                        GameManager.instance.messageScript.AINodeActivity(string.Format("Resistance Activity \"{0}\" (Player)",
                                            dataInput.originText), node, GameManager.instance.playerScript.actorID, delay);
                                        //AI Immediate message
                                        if (GameManager.instance.aiScript.immediateFlagResistance == true)
                                        {
                                            GameManager.instance.messageScript.AIImmediateActivity(string.Format("Immediate Activity \"{0}\" (Player)",
                                                dataInput.originText), reason, node.nodeID, -1);
                                        }
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
                                int invisibility = actor.datapoint2;
                                dataBefore = actor.datapoint2;
                                switch (effect.operand.name)
                                {
                                    case "Add":
                                        if (actor.datapoint2 < GameManager.instance.actorScript.maxStatValue)
                                        {
                                            //adds a variable amount
                                            invisibility += effect.value;
                                            invisibility = Mathf.Min(GameManager.instance.actorScript.maxStatValue, invisibility);
                                            actor.datapoint2 = invisibility;
                                            Debug.LogFormat("[Sta] -> EffectManger.cs: {0} {1} Invisibility changed from {2} to {3}{4}", actor.arc.name, actor.arc.name,
                                                dataBefore, invisibility, "\n");
                                        }
                                        effectReturn.bottomText = string.Format("{0}{1} {2}{3}", colourEffect, actor.arc.name, effect.description, colourEnd);
                                        break;
                                    case "Subtract":
                                        //double effect if spider is present
                                        if (node.isSpider == true)
                                        {
                                            invisibility -= 2;
                                            if (invisibility >= 0)
                                            { effectReturn.bottomText = string.Format("{0}{1} Invisibility -2 (Spider){2}", colourEffect, actor.arc.name, colourEnd); }
                                            else
                                            {
                                                //immediate notification. AI flag set. Applies if actor invis was 1 (spider effect) or 0 before action taken
                                                effectReturn.bottomText = string.Format("{0}{1} {2}{3}{4}<size=110%>Authority will know immediately</size>{5}",
                                                    colourEffect, actor.arc.name, effect.description, "\n", "\n", colourEnd);
                                                GameManager.instance.aiScript.immediateFlagResistance = true;
                                            }
                                        }
                                        else
                                        {
                                            invisibility -= 1;
                                            if (invisibility >= 0)
                                            { effectReturn.bottomText = string.Format("{0}{1} {2}{3}", colourEffect, actor.arc.name, effect.description, colourEnd); }
                                            else
                                            {
                                                //immediate notification. AI flag set. Applies if actor invis was 0 before action taken
                                                effectReturn.bottomText = string.Format("{0}{1} {2}{3}{4}<size=110%>Authority will know immediately</size>{5}",
                                                    colourEffect, actor.arc.name, effect.description, "\n", "\n", colourEnd);
                                                GameManager.instance.aiScript.immediateFlagResistance = true;
                                            }
                                        }
                                        //mincap zero
                                        invisibility = Mathf.Max(0, invisibility);
                                        actor.datapoint2 = invisibility;
                                        Debug.LogFormat("[Sta] -> EffectManger.cs: {0} {1} Invisibility changed from {2} to {3}{4}", actor.arc.name, actor.arc.name,
                                            dataBefore, invisibility, "\n");
                                        //AI activity message
                                        int delay;
                                        if (node.isSpider == true) { delay = delayYesSpider; }
                                        else { delay = delayNoSpider; }
                                        GameManager.instance.messageScript.AINodeActivity(string.Format("Resistance Activity \"{0}\" ({1})",
                                            dataInput.originText, actor.arc.name), node, actor.actorID, delay);
                                        //AI Immediate message
                                        if (GameManager.instance.aiScript.immediateFlagResistance == true)
                                        {
                                            GameManager.instance.messageScript.AIImmediateActivity(string.Format("Immediate Activity \"{0}\" ({1})",
                                                dataInput.originText, actor.arc.name), dataInput.originText, node.nodeID, -1, actor.actorID);
                                        }
                                        //Coward trait -> gets Stressed everytime they lose invisibility
                                        if (actor.CheckTraitEffect(actorStressedOverInvisibility) == true)
                                        {
                                            Condition conditionStressed = GameManager.instance.dataScript.GetCondition("STRESSED");
                                            if (conditionStressed != null)
                                            {
                                                if (actor.CheckConditionPresent(conditionStressed) == false)
                                                {
                                                    actor.AddCondition(conditionStressed, string.Format("Acquired due to {0} trait", actor.GetTrait().tag));
                                                    GameManager.instance.actorScript.TraitLogMessage(actor, "for a Stress check", "to become STRESSED due to a loss of Invisibility");
                                                    StringBuilder builder = new StringBuilder();
                                                    builder.Append(effectReturn.bottomText);

                                                    /*builder.AppendFormat("{0}{1}{2}Gains {3}{4}STRESSED{5}{6} condition due to {7}{8}Coward{9}{10} trait{11}", "\n", "\n",
                                                        colourBadSide, colourEnd, colourAlert, colourEnd, colourBadSide, colourEnd, colourNeutral, colourEnd, colourBadSide, colourEnd);*/
                                                    builder.AppendLine(); builder.AppendLine();
                                                    builder.AppendFormat("{0}{1} gains {2}{3}STRESSED{4}{5} condition due to {6}{7}{8}{9}{10} trait{11}", colourBad, actor.arc.name, colourEnd,
                                                        colourAlert, colourEnd, colourBad, colourEnd, colourAlert, actor.GetTrait().tag.ToUpper(), colourEnd, colourBad, colourEnd);
                                                    effectReturn.bottomText = builder.ToString();
                                                }
                                            }
                                            else { Debug.LogWarning("Invalid condition STRESSED (Null)"); }
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                Debug.LogError(string.Format("Invalid Actor (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                                effectReturn.errorFlag = true;
                            }

                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                    }
                    break;
                //Note: Renown can be in increments of > 1
                case "Renown":
                    if (node != null)
                    {

                        if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                        {
                            int playerRenown = GameManager.instance.playerScript.Renown;
                            //Player effect
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    playerRenown += effect.value;
                                    effectReturn.bottomText = string.Format("{0}Player {1}{2}", colourGoodSide, effect.description, colourEnd);
                                    break;
                                case "Subtract":
                                    playerRenown -= effect.value;
                                    playerRenown = Mathf.Max(0, playerRenown);
                                    effectReturn.bottomText = string.Format("{0}Player {1}{2}", colourBadSide, effect.description, colourEnd);
                                    break;
                            }
                            GameManager.instance.playerScript.Renown = playerRenown;
                        }
                        else
                        {
                            //Actor effect
                            if (actor != null)
                            {

                                dataBefore = actor.Renown;
                                switch (effect.operand.name)
                                {
                                    case "Add":
                                        actor.Renown += effect.value;
                                        if (actor.CheckTraitEffect(actorDoubleRenown) == true)
                                        {
                                            //trait -> renown doubled (only for Add renown)
                                            actor.Renown += effect.value;
                                            effectReturn.bottomText = string.Format("{0}{1} Renown +{2}{3} {4}({5}){6}", colourBadSide, actor.arc.name, effect.value * 2, colourEnd,
                                                colourNeutral, actor.GetTrait().tag, colourEnd);
                                            //logger
                                            GameManager.instance.actorScript.TraitLogMessage(actor, "for increasing Renown", "to gain DOUBLE renown");
                                        }
                                        else
                                        {
                                            //no trait
                                            effectReturn.bottomText = string.Format("{0}{1} {2}{3}", colourBadSide, actor.arc.name, effect.description, colourEnd);
                                        }
                                        break;
                                    case "Subtract":
                                        actor.Renown -= effect.value;
                                        actor.Renown = Mathf.Max(0, actor.Renown);
                                        effectReturn.bottomText = string.Format("{0}{1} {2}{3}", colourGoodSide, actor.arc.name, effect.description, colourEnd);
                                        break;
                                }
                                Debug.LogFormat("[Sta] -> EffectManager.cs: {0} {1} Renown changed from {2} to {3}{4}", actor.arc.name, actor.arc.name, dataBefore, actor.Renown, "\n");
                            }
                            else
                            {
                                Debug.LogError(string.Format("Invalid Actor (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                                effectReturn.errorFlag = true;
                            }

                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                    }
                    break;
                //
                // - - - Authority Effects
                //
                case "AnyTeam":
                    // Not needed as handled by a different process, keep for reference
                    break;
                case "CivilTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("CIVIL");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    //insert team
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    //action
                    effectReturn.isAction = true;
                    break;
                case "ControlTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("CONTROL");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    effectReturn.isAction = true;
                    break;
                case "DamageTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("DAMAGE");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    effectReturn.isAction = true;
                    break;
                case "ErasureTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("ERASURE");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    effectReturn.isAction = true;
                    break;
                case "MediaTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("MEDIA");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    effectReturn.isAction = true;
                    break;
                case "ProbeTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("PROBE");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    effectReturn.isAction = true;
                    break;
                case "SpiderTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("SPIDER");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    effectReturn.isAction = true;
                    break;

                default:
                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
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
    /// Formats string for details.TopText (Authority), returns "unknown" if a problem. Also used by ModalTeamPicker.cs -> ProcessTeamChoice and TeamNanager.cs
    /// Can be used for both Inserting and Recalling a team
    /// </summary>
    /// <param name="team"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public string SetTopTeamText(int teamID, bool isInserted = true)
    {
        //get team
        Team team = GameManager.instance.dataScript.GetTeam(teamID);
        if (team != null)
        {
            //get node
            Node node = GameManager.instance.dataScript.GetNode(team.nodeID);
            if (node != null)
            {
                string operation = "inserted";
                if (isInserted == false) { operation = "recalled"; }
                return string.Format("{0}{1}{2}{3} {4} have been {5} at {6}{7}{8}{9}{10} {11}{12}", 
                    colourNeutral, team.arc.name, colourEnd, 
                    colourNormal, team.teamName, operation, colourEnd,  
                    colourBadSide, node.Arc.name, colourEnd, colourNormal, node.nodeName, colourEnd);
            }
            else
            {
                Debug.LogError(string.Format("Invalid node (Null) for team.NodeID {0}", team.nodeID));
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
    /// Formats string for details.BottomText (Authority), returns "unknown" if parameter is null. Also used by ModalTeamPicker.cs -> ProcessTeamChoice
    /// </summary>
    /// <returns></returns>
    public string SetBottomTeamText(Actor actor)
    {
        if (actor != null)
        {
            string colourNumbers = colourGoodSide;
            if (actor.CheckNumOfTeams() == actor.datapoint2)
            { colourNumbers = colourBadSide; }
            return string.Format("{0}, {1} of {2}{3}{4} has now deployed {5}{6}{7} of {8}{9}{10} teams",
                actor.actorName, GameManager.instance.metaScript.GetAuthorityTitle(), colourActor, actor.arc.name, colourEnd,
                colourNumbers, actor.CheckNumOfTeams(), colourEnd, colourNumbers, actor.datapoint2, colourEnd);
        }
        else
        {
            Debug.LogError("Invalid actor (Null)");
            return "Unknown";
        }
    }

    /// <summary>
    /// Sub method to process group actor effects, eg. All actors Motivation +1. If actor != null then this actor is excluded from the effect
    /// NOTE: Effect and Actor are checked for null by the calling method
    /// </summary>
    /// <param name="datapoint"></param>
    /// <param name="value"></param>
    /// <param name="actor"></param>
    private void ResolveGroupActorEffect(Effect effect, Actor actorExclude = null)
    {
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(side);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, side) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    {
                        bool isProceed = true;
                        //don't do actorExclude if provided
                        if (actorExclude != null)
                        {
                           if (actorExclude.actorID == actor.actorID)
                            { isProceed = false; }
                        }
                        //adjust actor
                        if (isProceed == true)
                        {
                            switch (effect.outcome.name)
                            {
                                case "Motivation":
                                    switch (effect.operand.name)
                                    {
                                        case "Add":
                                            actor.datapoint1 += effect.value;
                                            actor.datapoint1 = Mathf.Min(GameManager.instance.actorScript.maxStatValue, actor.datapoint1);
                                            break;
                                        case "Subtract":
                                            actor.datapoint1 -= effect.value;
                                            actor.datapoint1 = Mathf.Max(GameManager.instance.actorScript.minStatValue, actor.datapoint1);
                                            break;
                                        default:
                                            Debug.LogWarningFormat("Invalid effect.operand \"{0}\"", effect.operand.name);
                                            break;
                                    }
                                    break;
                                default:
                                    Debug.LogWarningFormat("Invalid effect.outcome \"{0}\"", effect.outcome.name);
                                    break;
                            }
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid actor (Null) for arrayOfActors[{0}]", i); }
                }
            }
        }
        else { Debug.LogWarning("Invalid arrayOfActors (Null)"); }
    }

    /// <summary>
    /// Sub method to process Node Stability/Security/Support
    /// Note: Effect and Node checked for null by the calling method
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveNodeData(Effect effect, Node node, EffectDataInput effectInput)
    {
        int value = 0;
        //sort out colour based on type (which is effect benefit from POV of Resistance)
        string colourEffect = colourDefault;
        string colourText = colourDefault;
        if (effect.typeOfEffect != null)
        {
            switch (effect.typeOfEffect.name)
            {
                case "Good":
                    colourEffect = colourGoodSide;
                    break;
                case "Neutral":
                    colourEffect = colourNeutral;
                    colourText = colourNeutral;
                    break;
                case "Bad":
                    colourEffect = colourBadSide;
                    colourText = colourAlert;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                    break;
            }
        }
        else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name)); }
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //data package to send to node for field processing
        EffectDataProcess effectProcess = new EffectDataProcess() { outcome = effect.outcome };
        //Extent of effect
        switch (effect.apply.name)
        {
            //Current Node only
            case "NodeCurrent":
                if (effect.operand != null)
                {
                    switch (effect.operand.name)
                    {
                        case "Add":
                            value = effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}The security system has been swept and strengthened{1}", colourText, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Law Enforcement teams have stabilised the situation{1}", colourText, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}There is a surge of support for the Resistance{1}", colourText, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}The district security system has been scanned for intruders{1}", colourText, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}A Spider has been covertly inserted into the district security system{1}", colourText, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Listening bots have been deployed to the district{1}", colourText, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The local district grapevine is alive and well{1}", colourText, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        case "Subtract":
                            value = -1 * effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}The security system has been successfully hacked{1}", colourText, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Civil unrest and instability is spreading throughout{1}", colourText, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}The Resistance is losing popularity{1}", colourText, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}The district security system has been spoofed to conceal Tracers{1}", colourText, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}ICE has been inserted into the district security system to conceal Spiders{1}", colourText, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Listening bots have been muted to conceal Contacts{1}", colourText, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The local district grapevine has been shut down to conceal Teams{1}", colourText, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                            effectResolve.isError = true;
                            break;
                    }
                }
                else
                {
                    //null operand -> Reveal 'x' operation
                    Debug.LogWarning(string.Format("Invalid operand (Null) \"{0}\"", effect.outcome.name));
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, node, value); }
                //Process Node effect
                node.ProcessNodeEffect(effectProcess);
                break;
            //Neighbouring Nodes
            case "NodeNeighbours":
                if (effect.operand != null)
                {
                    switch (effect.operand.name)
                    {
                        case "Add":
                            value = effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}Neighbouring security systems have been swept and strengthened{1}", colourText, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Law Enforcement teams have stabilised neighbouring districts{1}", colourText, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}There is a surge of support for the Resistance in neighbouring districts{1}", colourText, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}The neighbouring security systems have been scanned for intruders{1}", colourText, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}A Tracer has been covertly inserted into the neighbouring security systems{1}", colourText, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Listening bots have been deployed{1}", colourText, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The local neighbourhood grapevine is alive and well{1}", colourText, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        case "Subtract":
                            value = -1 * effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}Neighbouring security systems have been successfully hacked{1}", colourText, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Civil unrest and instability is spreading throughout the neighbouring districts{1}", colourText, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}The Resistance is losing popularity in neighbouring districts{1}", colourText, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}The neighbouring security systems have been spoofed to hide Tracers{1}", colourText, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}ICE has been inserted into the neighbouring security systems to conceal Spiders{1}", colourText, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Listening bots have been muted to conceal Contacts{1}", colourText, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The local neighbourhood grapevine has been shut down to conceal Teams{1}", colourText, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                            effectResolve.isError = true;
                            break;
                    }
                }
                else
                {
                    //null operand -> Reveal 'x' operation
                    Debug.LogWarning(string.Format("Invalid operand (Null) \"{0}\"", effect.outcome.name));
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, node, value); }
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
            case "NodeAll":
                if (effect.operand != null)
                {
                    switch (effect.operand.name)
                    {
                        case "Add":
                            value = effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}All security systems have been swept and strengthened{1}", colourText, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Law Enforcement teams have stabilised the city wide situation{1}", colourText, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}There is a surge of support for the Resistance throughout the city{1}", colourText, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}The cities security systems have been scanned for intruders{1}", colourText, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}A Tracer has been covertly inserted into the cities security system{1}", colourText, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Listening bots have been deployed throughout the city{1}", colourText, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The city grapevine is alive and well{1}", colourText, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        case "Subtract":
                            value = -1 * effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}All security systems have been successfully hacked{1}", colourText, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Civil unrest and instability is spreading throughout the city{1}", colourText, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}The Resistance is losing popularity throughout the city{1}", colourText, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}Countermeasures have been deployed to conceal all Tracers within the City{1}", colourText, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}ICE has been deployed to conceal all Spiders within the City{1}", colourText, colourEnd);
                                    break;
                                case "StatusContacs":
                                    effectResolve.topText = string.Format("{0}Countermeasures have been deployed to conceal all Contacts within the City{1}", colourText, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}ICE has been deployed to conceal all Teams within the City{1}", colourText, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                            effectResolve.isError = true;
                            break;
                    }
                }
                else
                {
                    //null operand -> Reveal 'x' operation
                    Debug.LogWarning(string.Format("Invalid operand (Null) \"{0}\"", effect.outcome.name));
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, node, value); }
                //Process Node effect for current node
                node.ProcessNodeEffect(effectProcess);
                //Process Node effect for all nodes
                List<Node> listOfAllNodes = GameManager.instance.dataScript.GetListOfAllNodes();
                if (listOfAllNodes != null)
                {
                    foreach (Node nodeTemp in listOfAllNodes)
                    {
                        //exclude current node
                        if (nodeTemp.nodeID != node.nodeID)
                        { nodeTemp.ProcessNodeEffect(effectProcess); }
                    }
                }
                else { Debug.LogError("Invalid dictOfAllNodes (Null)"); }
                break;
            //Nodes of the Same Arc type
            case "NodeSameArc":
                if (effect.operand != null)
                {
                    switch (effect.operand.name)
                    {
                        case "Add":
                            value = effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}Security systems in {1} districts have been swept and strengthened{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Law Enforcement teams have stabilised the {1} situation{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}There is a surge of support for the Resistance in {1} districts{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}{1} security systems have been scanned for intruders{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}A Tracer has been covertly inserted into {1} security systems{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Listening bots have been deployed throughout {1} districts{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The {1} grapevine is alive and well{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        case "Subtract":
                            value = -1 * effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}Security systems in {1} districts have been successfully hacked{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Civil unrest and instability is spreading throughout {1} districts{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}The Resistance is losing popularity throughout {1} districts{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}{1} security systems have been spoofed to conceal Tracers{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}ICE has been inserted into {1} security systems to conceal Spiders{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Countermeasures have been taken in {1} districts to conceal Contacts{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The {1} grapevine has been shut down to conceal Teams{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                            effectResolve.isError = true;
                            break;
                    }
                }
                else
                {
                    //null operand -> Reveal 'x' operation
                    Debug.LogWarning(string.Format("Invalid operand (Null) \"{0}\"", effect.outcome.name));
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, node, value); }
                //Process Node effect for current node
                node.ProcessNodeEffect(effectProcess);
                //Process Node effect for all neighbouring nodes
                List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
                if (listOfNodes != null)
                {
                    foreach (var nodeTemp in listOfNodes)
                    {
                        //same node Arc type and not the current node?
                        if (nodeTemp.Arc.nodeArcID == node.Arc.nodeArcID && nodeTemp.nodeID != node.nodeID)
                        { nodeTemp.ProcessNodeEffect(effectProcess); }
                    }
                }
                else { Debug.LogError("Invalid listOfNodes (Null)"); }
                break;
            default:
                Debug.LogError(string.Format("Invalid effect.apply \"{0}\"", effect.apply.name));
                break;
        }
        //bottom text
        effectResolve.bottomText = string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd);
        //return data to calling method (ProcessEffect)
        return effectResolve;
    }

    /// <summary>
    /// Sub method to process Node Stability/Security/Support
    /// Note: effect and node checked for Null by calling method
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="node"></param>
    /// <param name="effectInput"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveConnectionData(Effect effect, Node node, EffectDataInput effectInput)
    {
        int value = 0;
        //sort out colour based on type (which is effect benefit from POV of Resistance)
        string colourEffect = colourDefault;
        string colourText = colourDefault;
        if (effect.typeOfEffect != null)
        {
            switch (effect.typeOfEffect.name)
            {
                case "Good":
                    colourEffect = colourGoodSide;
                    break;
                case "Neutral":
                    colourEffect = colourNeutral;
                    colourText = colourNeutral;
                    break;
                case "Bad":
                    colourEffect = colourBadSide;
                    colourText = colourAlert;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                    break;
            }
        }
        else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name)); }
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //data package to send to node for field processing
        EffectDataProcess effectProcess = new EffectDataProcess() { outcome = effect.outcome };

        //Extent of effect
        switch (effect.apply.name)
        {
            //Current Node only
            case "ConnectionCurrent":
                switch (effect.operand.name)
                {
                    case "Add":
                        value = effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}Immediate Connections have raised their security{1}", colourText, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security +{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    case "Subtract":
                        value = -1 * effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}Immediate Connections have lowered their security{1}", colourText, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security -{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                        effectResolve.isError = true;
                        break;
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, node, value); }
                //Process Connection effect for current node
                node.ProcessConnectionEffect(effectProcess);
                break;
            //Neighbouring Nodes
            case "ConnectionNeighbours":
                switch (effect.operand.name)
                {
                    case "Add":
                        value = effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}Neighbouring Connections have raised their security{1}", colourText, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security +{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    case "Subtract":
                        value = -1 * effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}Neighbouring Connections have lowered their security{1}", colourText, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security -{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                        effectResolve.isError = true;
                        break;
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                {
                    ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, node, value);
                    /*//DEBUG -> remove when finished with testing
                    effectProcess.effectOngoing.ongoingID = GetOngoingEffectID();*/
                }
                //Process Connection effect
                List<Node> listOfNeighbours = node.GetNeighbouringNodes();
                if (listOfNeighbours != null)
                {
                    //clear all connection flags first to prevent double dipping
                    GameManager.instance.connScript.SetAllFlagsToFalse();
                    //process current node
                    node.ProcessConnectionEffect(effectProcess);
                    //process all neighbouring nodes
                    foreach (Node nodeTemp in listOfNeighbours)
                    { nodeTemp.ProcessConnectionEffect(effectProcess); }
                }
                else { Debug.LogError("Invalid listOfNeighours (Null)"); }
                break;
            //Same Node Arc
            case "ConnectionSameArc":
                switch (effect.operand.name)
                {
                    case "Add":
                        value = effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}{1} Connections have raised their security{2}", colourText, node.Arc.name, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security +{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    case "Subtract":
                        value = -1 * effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}{1} Connections have lowered their security{2}", colourText, node.Arc.name, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security -{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                        effectResolve.isError = true;
                        break;
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                {
                    ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, node, value);
                    /*//DEBUG -> remove when finished with testing
                    effectProcess.effectOngoing.ongoingID = GetOngoingEffectID();
                    GameManager.instance.dataScript.AddOngoingIDToDict(effectProcess.effectOngoing.ongoingID, "Connection Security");*/
                }
                //Process Connection effect
                List<Node> listOfAllNodes = GameManager.instance.dataScript.GetListOfAllNodes();
                if (listOfAllNodes != null)
                {
                    //clear all connection flags first to prevent double dipping
                    GameManager.instance.connScript.SetAllFlagsToFalse();
                    //current node
                    node.ProcessConnectionEffect(effectProcess);
                    foreach (var nodeTemp in listOfAllNodes)
                    {
                        //same node Arc type and not the current node?
                        if (nodeTemp.Arc.nodeArcID == node.Arc.nodeArcID && nodeTemp.nodeID != node.nodeID)
                        { nodeTemp.ProcessConnectionEffect(effectProcess); }
                    }
                }
                else { Debug.LogError("Invalid listOfAllNodes (Null)"); }
                break;
            //All Nodes
            case "ConnectionAll":
                switch (effect.operand.name)
                {
                    case "Add":
                        value = effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}All Connections have raised their security{1}", colourText, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security +{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    case "Subtract":
                        value = -1 * effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}All Connections have lowered their security{1}", colourText, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security -{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                        effectResolve.isError = true;
                        break;
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                {
                    ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, node, value);
                }
                //Process Connection effect
                List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
                if (listOfNodes != null)
                {                
                    //clear all connection flags first to prevent double dipping
                    GameManager.instance.connScript.SetAllFlagsToFalse();
                    foreach (Node nodeTemp in listOfNodes)
                    { nodeTemp.ProcessConnectionEffect(effectProcess); }
                }
                else { Debug.LogError("Invalid listOfNodes (Null)"); }
                break;
        }
        //return data to calling method (ProcessEffect)
        return effectResolve;
    }

    /// <summary>
    /// Sub method to process Actor Condition. Can pass a Null actor (default) provided it is a Player condition
    /// Note: effect and actor checked for null by the calling method.
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveConditionData(Effect effect, Node node, EffectDataInput dataInput, Actor actor = null)
    {
        //sort out colour based on type (which is effect benefit from POV of Resistance But is SAME for both sides when it comes to Conditions)
        string colourEffect = colourDefault;
        Condition condition = null;
        if (effect.typeOfEffect != null)
        {
            switch (effect.typeOfEffect.name)
            {
                case "Good":
                    colourEffect = colourGood;
                    break;
                case "Neutral":
                    colourEffect = colourNeutral;
                    break;
                case "Bad":
                    colourEffect = colourBad;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                    break;
            }
        }
        else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name)); }
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();

        //choose big picture (Random / Any) or specific condition
        switch (effect.outcome.name)
        {
            //specific Conditions
            case "ConditionStressed":
            case "ConditionIncompetent":
            case "ConditionCorrupt":
            case "ConditionQuestionable":
            case "ConditionBlackmailer":
            case "ConditionStar":
                //get condition
                switch (effect.outcome.name)
                {
                    case "ConditionStressed":
                        condition = conditionStressed;
                        break;
                    case "ConditionIncompetent":
                        condition = conditionIncompetent;
                        break;
                    case "ConditionCorrupt":
                        condition = conditionCorrupt;
                        break;
                    case "ConditionQuestionable":
                        condition = conditionQuestionable;
                        break;
                    case "ConditionBlackmailer":
                        condition = conditionBlackmailer;
                        break;
                    case "ConditionStar":
                        condition = conditionStar;
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effect.outcome \"{0}\"", effect.outcome.name));
                        break;
                }
                //resolve effect outcome
                if (condition != null)
                {
                    //assign condition to player if at their node, otherwise actor
                    if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                    {
                        //Player Condition
                        switch (effect.operand.name)
                        {
                            case "Add":
                                //only add condition if NOT already present
                                if (GameManager.instance.playerScript.CheckConditionPresent(condition, dataInput.side) == false)
                                {
                                    GameManager.instance.playerScript.AddCondition(condition, dataInput.side, string.Format("Due to {0}", dataInput.originText));
                                    effectResolve.bottomText = string.Format("{0}Player gains condition {1}{2}", colourEffect, condition.name, colourEnd);
                                }
                                break;
                            case "Subtract":
                                //only remove  condition if present
                                if (GameManager.instance.playerScript.CheckConditionPresent(condition, dataInput.side) == true)
                                {
                                    GameManager.instance.playerScript.RemoveCondition(condition, dataInput.side, string.Format("Due to {0}", dataInput.originText));
                                    effectResolve.bottomText = string.Format("{0}Player condition {1} removed{2}", colourEffect, condition.name, colourEnd);
                                }
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                                break;
                        }

                    }
                    else
                    {
                        if (actor != null)
                        {
                            //Actor Condition
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    //only add condition if NOT already present
                                    if (actor.CheckConditionPresent(condition) == false)
                                    {
                                        actor.AddCondition(condition, string.Format("Due to {0}", dataInput.originText));
                                        effectResolve.bottomText = string.Format("{0}{1} condition gained{2}", colourEffect, condition.name, colourEnd);
                                    }
                                    break;
                                case "Subtract":
                                    //only remove  condition if present
                                    if (actor.CheckConditionPresent(condition) == true)
                                    {
                                        actor.RemoveCondition(condition, string.Format("Due to {0}", dataInput.originText));
                                        effectResolve.bottomText = string.Format("{0}{1} condition removed{2}", colourEffect, condition.name, colourEnd);
                                    }
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                                    break;
                            }
                        }
                        else { Debug.LogError("Invalid actor (Null)"); }
                    }
                }
                else { Debug.LogError(string.Format("Invalid condition (Null) for outcome \"{0}\"", effect.outcome.name)); }
                break;

            //add or subtract a group condition
            case "ConditionGroupGood":
            case "ConditionGroupBad":
                List<Condition> listOfConditions;
                Condition conditionRandom;
                string effectType = "Good";
                string colourConditionAdd = colourGoodSide;
                string colourConditionRemove = colourBadSide;
                GlobalType type = GameManager.instance.globalScript.typeGood;
                //get type
                switch (effect.outcome.name)
                {
                    case "ConditionGroupGood":
                        //default condition, no need to reassign
                        break;
                    case "ConditionGroupBad":
                        type = GameManager.instance.globalScript.typeBad;
                        effectType = "Bad";
                        colourConditionAdd = colourBadSide;
                        colourConditionRemove = colourGoodSide;
                        break;
                    default:
                        Debug.LogWarning(string.Format("Invalid outcome name \"{0}\"", effect.outcome.name));
                        break;
                }
                //Player
                if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                {
                    if (GameManager.instance.playerScript.CheckNumOfConditions(dataInput.side) > 0)
                    {
                        listOfConditions = GameManager.instance.playerScript.GetListOfConditions(dataInput.side);
                        switch (effect.apply.name)
                        {
                            case "ConditionRandom":
                                switch (effect.operand.name)
                                {
                                    case "Add":
                                        //add a new condition
                                        conditionRandom = GetRandomCondition(listOfConditions, type, effect.operand.name);
                                        if (conditionRandom != null)
                                        {
                                            GameManager.instance.playerScript.AddCondition(conditionRandom, dataInput.side, string.Format("Due to {0}", dataInput.originText));
                                            effectResolve.bottomText = string.Format("{0}{1} condition gained{2}", colourConditionAdd, conditionRandom.name, colourEnd);
                                        }
                                        else
                                        { effectResolve.bottomText = string.Format("{0}There are no {1} conditions present{2}", colourAlert, effectType, colourEnd); }
                                        break;
                                    case "Subtract":
                                        //remove an existing condition
                                        conditionRandom = GetRandomCondition(listOfConditions, type, effect.operand.name);
                                        if (conditionRandom != null)
                                        {
                                            //remove condition
                                            GameManager.instance.playerScript.RemoveCondition(conditionRandom, dataInput.side, string.Format("Due to {0}", dataInput.originText));
                                            effectResolve.bottomText = string.Format("{0}{1} condition removed{2}", colourConditionRemove, conditionRandom.name, colourEnd);
                                        }
                                        else
                                        { effectResolve.bottomText = string.Format("{0}There are no {1} conditions present{2}", colourAlert, effectType, colourEnd); }
                                        break;
                                    default:
                                        Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                                        break;
                                }
                                break;
                            case "ConditionAll":
                                string typeCompare;
                                int counter = 0;
                                switch (effect.outcome.name)
                                {
                                    case "ConditionGroupGood":
                                        //remove all good conditions
                                        if (listOfConditions.Count > 0)
                                        {
                                            typeCompare = GameManager.instance.globalScript.typeGood.name;
                                            for (int i = listOfConditions.Count - 1; i >= 0; i--)
                                            {
                                                if (listOfConditions[i].type.name.Equals(typeCompare) == true)
                                                { listOfConditions.RemoveAt(i); counter++; }
                                            }
                                            effectResolve.bottomText = string.Format("{0}All ({1}) Good conditions removed{2}", colourBadSide, counter, colourEnd);
                                        }
                                        break;
                                    case "ConditionGroupBad":
                                        //remove all bad conditions
                                        if (listOfConditions.Count > 0)
                                        {
                                            typeCompare = GameManager.instance.globalScript.typeBad.name;
                                            for (int i = listOfConditions.Count - 1; i >= 0; i--)
                                            {
                                                if (listOfConditions[i].type.name.Equals(typeCompare) == true)
                                                { listOfConditions.RemoveAt(i); counter++; }
                                            }
                                            effectResolve.bottomText = string.Format("{0}All ({1}) Bad conditions removed{2}", colourGoodSide, counter, colourEnd);
                                        }
                                        break;
                                    default:
                                        Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                                        break;
                                }
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid effect.apply \"{0}\" for effect outcome \"{1}\"", effect.apply.name, effect.outcome.name));
                                break;
                        }
                    }
                    else
                    { effectResolve.bottomText = string.Format("{0}There are no {1} conditions present{2}", colourAlert, effectType, colourEnd); }
                }
                //Actor
                else
                {
                    if (actor != null)
                    {
                        if (actor.CheckNumOfConditions() > 0)
                        {
                            listOfConditions = actor.GetListOfConditions();
                            switch (effect.apply.name)
                            {
                                case "ConditionRandom":
                                    switch (effect.operand.name)
                                    {
                                        case "Add":
                                            //add a new condition
                                            conditionRandom = GetRandomCondition(listOfConditions, type, effect.operand.name);
                                            if (conditionRandom != null)
                                            {
                                                actor.AddCondition(conditionRandom, string.Format("Due to {0}", dataInput.originText));
                                                effectResolve.bottomText = string.Format("{0}{1} condition gained{2}", colourConditionAdd, conditionRandom.name, colourEnd);
                                            }
                                            else
                                            { effectResolve.bottomText = string.Format("{0}There are no {1} conditions present{2}", colourAlert, effectType, colourEnd); }
                                            break;
                                        case "Subtract":
                                            //remove an existing condition
                                            conditionRandom = GetRandomCondition(listOfConditions, type, effect.operand.name);
                                            if (conditionRandom != null)
                                            {
                                                //remove condition
                                                actor.RemoveCondition(conditionRandom, string.Format("Due to {0}", dataInput.originText));
                                                effectResolve.bottomText = string.Format("{0}{1} condition removed{2}", colourConditionRemove, conditionRandom.name, colourEnd);
                                            }
                                            else
                                            { effectResolve.bottomText = string.Format("{0}There are no {1} conditions present{2}", colourAlert, effectType, colourEnd); }
                                            break;
                                        default:
                                            Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                                            break;
                                    }
                                    break;
                                case "ConditionAll":
                                    string typeCompare;
                                    int counter = 0;
                                    switch (effect.outcome.name)
                                    {
                                        case "ConditionGroupGood":
                                            //remove all good conditions
                                            if (listOfConditions.Count > 0)
                                            {
                                                typeCompare = GameManager.instance.globalScript.typeGood.name;
                                                for (int i = listOfConditions.Count - 1; i >= 0; i--)
                                                {
                                                    if (listOfConditions[i].type.name.Equals(typeCompare) == true)
                                                    { listOfConditions.RemoveAt(i); counter++; }
                                                }
                                                effectResolve.bottomText = string.Format("{0}All ({1}) Good conditions removed{2}", colourBadSide, counter, colourEnd);
                                            }
                                            break;
                                        case "ConditionGroupBad":
                                            //remove all bad conditions
                                            if (listOfConditions.Count > 0)
                                            {
                                                typeCompare = GameManager.instance.globalScript.typeBad.name;
                                                for (int i = listOfConditions.Count - 1; i >= 0; i--)
                                                {
                                                    if (listOfConditions[i].type.name.Equals(typeCompare) == true)
                                                    { listOfConditions.RemoveAt(i); counter++; }
                                                }
                                                effectResolve.bottomText = string.Format("{0}All ({1}) Bad conditions removed{2}", colourGoodSide, counter, colourEnd);
                                            }
                                            break;
                                        default:
                                            Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                                            break;
                                    }
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effect.apply \"{0}\" for effect outcome \"{1}\"", effect.apply.name, effect.outcome.name));
                                    break;
                            }
                        }
                        else
                        { effectResolve.bottomText = string.Format("{0}There are no {1} conditions present{2}", colourAlert, effectType, colourEnd); }
                    }
                    else { Debug.LogError("Invalid actor (Null)"); }
                }
                break;

            default:
                Debug.LogError(string.Format("Invalid effect.outcome \"{0}\"", effect.outcome.name));
                break;
        }
        return effectResolve;
    }

    /// <summary>
    /// Given a list of conditions it chooses a random one of the specified type (Good/Bad). Returns null if not found. Sub method for ResolveConditionData
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private Condition GetRandomCondition(List<Condition> listOfConditions, GlobalType type, string operand)
    {
        Condition conditionSelected = null;
        if (listOfConditions != null && listOfConditions.Count > 0)
        {
            if (string.IsNullOrEmpty(operand) == false)
            {
                //create a new list by value, not reference
                List<Condition> tempList = new List<Condition>();
                switch (operand)
                {
                    case "Remove":
                        //randomly select a condition of the required type to remove from the provided list
                        foreach (Condition condition in listOfConditions)
                        {
                            //add to list if the correct type
                            if (condition.type.name.Equals(type.name) == true)
                            { tempList.Add(condition); }
                        }
                        //select a condition
                        int numOfConditions = tempList.Count;
                        if (numOfConditions > 0)
                        { conditionSelected = tempList[Random.Range(0, numOfConditions)]; }
                        break;
                    case "Add":
                        //randomly select a new condition of the required type that is NOT on the provided list
                        Dictionary<string, Condition> dictOfConditionsByType = GameManager.instance.dataScript.GetDictOfConditionsByType(type);
                        if (dictOfConditionsByType.Count > 0)
                        {
                            //loop listOfConditions and remove any identical entries in the temp dict
                            foreach(Condition condition in listOfConditions)
                            {
                                if (dictOfConditionsByType.ContainsKey(condition.name))
                                { dictOfConditionsByType.Remove(condition.name); }
                            }
                            //place remaining conditions (in temp dict) in a list
                            if (dictOfConditionsByType.Count > 0)
                            {
                                foreach (var condTemp in dictOfConditionsByType)
                                { tempList.Add(condTemp.Value); }
                                //randomly select a condition
                                conditionSelected = tempList[Random.Range(0, tempList.Count)];
                            }
                        }
                        else { Debug.LogWarning(string.Format("dictOfConditionByType returns Empty for type \"{0}\"", type.name)); }
                        break;
                    default:
                        Debug.LogWarning(string.Format("Invalid operand \"{0}\"", operand));
                        break;
                }
            }
            else { Debug.LogWarning("Invalid operand (Null or Empty)"); }
        }
        else { Debug.LogWarning("Invalid listOfConditios (Null or Empty)"); }
        return conditionSelected;
    }

    /// <summary>
    /// subMethod to handle all manage effects
    /// Note: Actor has been checked for null by the calling method
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveManageData(Effect effect, Actor actor)
    {
        int data;
        //sort out colour based on type (which is effect benefit from POV of Resistance)
        string colourEffect = colourDefault;
        if (effect.typeOfEffect != null)
        {
            switch (effect.typeOfEffect.name)
            {
                case "Good":
                    colourEffect = colourGoodSide;
                    break;
                case "Neutral":
                    colourEffect = colourNeutral;
                    break;
                case "Bad":
                    colourEffect = colourBadSide;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                    break;
            }
        }
        else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name)); }
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //get condition
        switch (effect.outcome.name)
        {
            case "ActorToReserves":
                effectResolve.bottomText = string.Format("{0}{1} moved to the Reserves{2}", colourEffect, actor.actorName, colourEnd);
                break;
            case "ActorPromoted":
                effectResolve.bottomText = string.Format("{0}{1} promoted and will join {2} HQ{3}", colourEffect, actor.actorName, 
                    GameManager.instance.factionScript.GetCurrentFaction(), colourEnd);
                break;
            case "ActorDismissed":
                effectResolve.bottomText = string.Format("{0}{1} dismissed{2}", colourEffect, actor.actorName, colourEnd);
                break;
            case "ActorDisposedOff":
                effectResolve.bottomText = string.Format("{0}{1} killed{2}", colourEffect, actor.actorName, colourEnd);
                break;
            case "ManageReserveRenown":
                data = GameManager.instance.actorScript.manageReserveRenown;
                GameManager.instance.playerScript.Renown -= data;
                effectResolve.bottomText = string.Format("{0}Player Renown -{1}{2}", colourEffect, data, colourEnd);
                break;
            case "ManageDismissRenown":
                ManageRenownCost manageDismissCost = GameManager.instance.actorScript.GetManageRenownCost(actor, GameManager.instance.actorScript.manageDismissRenown);
                data = manageDismissCost.renownCost;
                GameManager.instance.playerScript.Renown -= data;
                StringBuilder builderDismiss = new StringBuilder();
                builderDismiss.AppendFormat("{0}Player Renown -{1}{2}", colourEffect, data, colourEnd);
                if (String.IsNullOrEmpty(manageDismissCost.tooltip) == false)
                { builderDismiss.Append(manageDismissCost.tooltip); }
                effectResolve.bottomText = builderDismiss.ToString();
                break;
            case "ManageDisposeRenown":
                ManageRenownCost manageDisposeCost = GameManager.instance.actorScript.GetManageRenownCost(actor, GameManager.instance.actorScript.manageDisposeRenown);
                data = manageDisposeCost.renownCost;
                GameManager.instance.playerScript.Renown -= data;
                StringBuilder builderDispose = new StringBuilder();
                builderDispose.AppendFormat("{0}Player Renown -{1}{2}", colourEffect, data, colourEnd);
                if (String.IsNullOrEmpty(manageDisposeCost.tooltip) == false)
                { builderDispose.Append(manageDisposeCost.tooltip); }
                effectResolve.bottomText = builderDispose.ToString();
                break;
            case "UnhappyTimerCurrent":
                data = GameManager.instance.actorScript.currentReserveTimer;
                //traits that affect unhappy timer
                string traitText = "";
                if (actor.CheckTraitEffect(actorReserveTimerDoubled) == true)
                {
                    data *= 2; traitText = string.Format(" ({0})", actor.GetTrait().tag);
                    GameManager.instance.actorScript.TraitLogMessage(actor, "for their willingness to wait", "to DOUBLE Reserve Unhappy Timer");
                }
                else if (actor.CheckTraitEffect(actorReserveTimerHalved) == true)
                {
                    data /= 2; data = Mathf.Max(1, data); traitText = string.Format(" ({0})", actor.GetTrait().tag);
                    GameManager.instance.actorScript.TraitLogMessage(actor, "for their reluctance to wait", "to HALVE Reserve Unhappy Timer");
                }
                //set timer
                actor.unhappyTimer = data;
                effectResolve.bottomText = string.Format("{0}{1}'s Unhappy Timer set to {2} turn{3}{4}{5}", colourEffect, actor.actorName, data,
                    data != 1 ? "s" : "", traitText, colourEnd);
                break;
            case "ActorPromised":
                actor.isPromised = true;
                effectResolve.bottomText = string.Format("{0}{1} has been Promised{2}", colourEffect, actor.actorName, colourEnd);
                break;
            default:
                Debug.LogError(string.Format("Invalid effect.outcome \"{0}\"", effect.outcome.name));
                break;
        }
        return effectResolve;
    }

    /// <summary>
    /// subMethod to handle special actor effects
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveSpecialActorEffect(Effect effect, Actor actor)
    {
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //get condition
        switch (effect.outcome.name)
        {
            case "ActorResigns":
                //NOTE: Not a Manage option
                if (actor.CheckTraitEffect(actorNeverResigns) == false)
                {
                    if (GameManager.instance.dataScript.RemoveCurrentActor(GameManager.instance.sideScript.PlayerSide, actor, ActorStatus.Resigned) == true)
                    { effectResolve.bottomText = string.Format("{0}{1} Resigns{2}", colourBadSide, actor.arc.name, colourEnd); }
                }
                else
                {
                    //trait actorResignNone "Loyal"
                    GameManager.instance.actorScript.TraitLogMessage(actor, "for a Resignation check", "to AVOID Resigning");
                    effectResolve.bottomText = string.Format("{0}{1} has {2}{3}{4}{5}{6} trait{7}", colourAlert, actor.arc.name, colourEnd,
                        colourNeutral, actor.GetTrait().tag, colourEnd, colourAlert, colourEnd);
                }
                break;
            case "ActorKills":
                //NOTE: Not a Manage option
                effectResolve.bottomText = GameManager.instance.actorScript.ProcessKillRandomActor(actor);
                break;
            default:
                Debug.LogError(string.Format("Invalid effect.outcome \"{0}\"", effect.outcome.name));
                break;
        }

        return effectResolve;
    }

    /// <summary>
    /// subMethod to process special player effects
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    private EffectDataResolve ResolvePlayerData(Effect effect, EffectDataInput dataInput)
    {
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        //sort out colour based on type (which is effect benefit from POV of Resistance)
        string colourEffect = colourDefault;
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        if (effect.typeOfEffect != null)
        {
            switch (effect.typeOfEffect.name)
            {
                case "Good":
                    colourEffect = colourGoodSide;
                    break;
                case "Neutral":
                    colourEffect = colourNeutral;
                    break;
                case "Bad":
                    colourEffect = colourBadSide;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                    break;
            }

            //get condition
            switch (effect.outcome.name)
            {
                case "PlayerActions":
                    //single or ongoing
                    ActionAdjustment actionAdjustment = new ActionAdjustment();
                    actionAdjustment.side = side;
                    switch (effect.duration.name)
                    {
                        case "Single":
                            //once off effect -> Note: timer is 2 because it will be immediately knocked down to 1 at the end of this turn
                            actionAdjustment.timer = 2;
                            actionAdjustment.descriptor = dataInput.originText;
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    actionAdjustment.value = effect.value;
                                    GameManager.instance.dataScript.AddActionAdjustment(actionAdjustment);
                                    effectResolve.bottomText = string.Format("{0}Player gains {1}{2}{3}{4}{5} extra action{6} {7}{8}NEXT TURN{9}", colourEffect, colourEnd,
                                        colourNeutral, effect.value, colourEnd, colourEffect, effect.value != 1 ? "s" : "", colourEnd, colourNeutral, colourEnd);                                    
                                    break;
                                case "Subtract":
                                    actionAdjustment.value = effect.value * -1;
                                    GameManager.instance.dataScript.AddActionAdjustment(actionAdjustment);
                                    effectResolve.bottomText = string.Format("{0}Player loses {1}{2}{3}{4}{5} extra action{6} {7}{8}NEXT TURN{9}", colourEffect, colourEnd,
                                        colourNeutral, effect.value, colourEnd, colourEffect, effect.value != 1 ? "s" : "", colourEnd, colourNeutral, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effect.operand \"{0}\"", effect.operand.name));
                                    break;
                            }
                            break;
                        case "Ongoing":
                            //NOTE: Ongoing effects are handled differently here than a standard ongoing effect (there is also an extra +1 due to decrement at end of turn)
                            actionAdjustment.timer = GameManager.instance.effectScript.ongoingEffectTimer + 1;
                            actionAdjustment.descriptor = dataInput.originText;
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    actionAdjustment.ongoing = AddOngoingEffectToDict(effect, dataInput, effect.value);
                                    actionAdjustment.value = effect.value;
                                    GameManager.instance.dataScript.AddActionAdjustment(actionAdjustment);
                                    effectResolve.bottomText = string.Format("{0}Player gains {1}{2}{3}{4}{5} extra action{6} for {7}{8}{9}{10}{11} turns commencing {12}{13}NEXT TURN{14}", 
                                        colourEffect, colourEnd, colourNeutral, effect.value, colourEnd, colourEffect, effect.value != 1 ? "s" : "", colourEnd, colourNeutral,
                                        actionAdjustment.timer - 1, colourEnd, colourEffect, colourEnd, colourNeutral, colourEnd);
                                    
                                    break;
                                case "Subtract":
                                    actionAdjustment.ongoing = AddOngoingEffectToDict(effect, dataInput, effect.value * -1);
                                    actionAdjustment.value = effect.value * -1;
                                    GameManager.instance.dataScript.AddActionAdjustment(actionAdjustment);
                                    effectResolve.bottomText = string.Format("{0}Player loses {1}{2}{3}{4}{5} extra action{6} for {7}{8}{9}{10}{11} turns commencing {12}{13}NEXT TURN{14}",
                                        colourEffect, colourEnd, colourNeutral, effect.value, colourEnd, colourEffect, effect.value != 1 ? "s" : "", colourEnd, colourNeutral,
                                        actionAdjustment.timer - 1, colourEnd, colourEffect, colourEnd, colourNeutral, colourEnd);
                                    
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effect.operand \"{0}\"", effect.operand.name));
                                    break;
                            }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effect.duration \"{0}\"", effect.duration.name));
                            break;
                    }
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.outcome \"{0}\"", effect.outcome.name)); 
                    break;
            }
        }
        else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name));}
        return effectResolve;
    }

    /// <summary>
    /// subMethod to handle Ongoing Effects for Gear (Personal Use -> Actions +/-)
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    private EffectDataOngoing AddOngoingEffectToDict(Effect effect, EffectDataInput effectInput, int value)
    {
        EffectDataOngoing effectOngoing = new EffectDataOngoing();
        effectOngoing.outcome = effect.outcome;
        effectOngoing.ongoingID = effectInput.ongoingID;
        effectOngoing.type = effect.typeOfEffect;
        effectOngoing.apply = effect.apply;
        effectOngoing.side = effectInput.side;
        effectOngoing.value = effect.value;
        effectOngoing.gearName = effectInput.ongoingText;
        effectOngoing.gearID = effectInput.data;
        effectOngoing.reason = effectInput.ongoingText;
        effectOngoing.description = effect.description;
        effectOngoing.text = string.Format("{0} ({1} turn{2})", effect.description, effectOngoing.timer, effectOngoing.timer != 1 ? "s" : "");
        effectOngoing.nodeTooltip = effect.ongoingTooltip;
        //add to register & create message
        GameManager.instance.dataScript.AddOngoingEffectToDict(effectOngoing);
        return effectOngoing;
    }

    /// <summary>
    /// subMethod to handle Ongoing effects for Nodes
    /// </summary>
    private void ProcessOngoingEffect(Effect effect, EffectDataProcess effectProcess, EffectDataResolve effectResolve, EffectDataInput effectInput, Node node, int value)
    {
        EffectDataOngoing effectOngoing = new EffectDataOngoing();
        effectOngoing.outcome = effect.outcome;
        effectOngoing.ongoingID = effectInput.ongoingID;
        effectOngoing.value = value;
        effectOngoing.type = effect.typeOfEffect;
        effectOngoing.apply = effect.apply;
        effectOngoing.side = effectInput.side;
        effectOngoing.reason = effectInput.ongoingText;
        effectOngoing.description = effect.description;
        effectOngoing.node = node;
        effectOngoing.text = string.Format("{0} ({1} turn{2})", effect.description, effectOngoing.timer, effectOngoing.timer != 1 ? "s" : "");
        effectOngoing.nodeTooltip = effect.ongoingTooltip;
        //add to effectProcess
        effectProcess.effectOngoing = effectOngoing;
    }


    /// <summary>
    /// gets a unique ID for ongoing effects. All
    /// </summary>
    /// <returns></returns>
    public int GetOngoingEffectID()
    { return ongoingEffectIDCounter++; }

    //place methods above here
}
