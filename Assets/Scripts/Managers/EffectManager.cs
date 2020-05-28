using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// handles Effect related matters (Actions and Targets)
/// Effects are assumed to generate text and outcomes for a Modal Outcome window
/// </summary>
public class EffectManager : MonoBehaviour
{
    [Header("Ongoing Effects")]
    [Tooltip("How long do ongoing effects last for? Global setting")]
    [Range(3, 20)] public int ongoingEffectTimer = 10;

    [Header("Statistic Criteria")]
    [Tooltip("Minimum number of days actors/player spent lying low required to trigger criteria")]
    [Range(0, 20)] public int statDaysLieLowMin = 10;
    [Tooltip("Minimum number of gear items acquired needed to trigger criteria")]
    [Range(0, 20)] public int statGearItemsMin = 4;
    [Tooltip("Minimum number of node actions by actors/player required to trigger criteria")]
    [Range(0, 20)] public int statNodeActionsMin = 4;
    [Tooltip("Minimum number of target attempts by actors/player needed to trigger criteria")]
    [Range(0, 20)] public int statTargetAttemptsMin = 4;

    [Header("Ratio Criteria")]
    [Tooltip("This number or Less of Player Node Actions / Total Node Actions to trigger criteria")]
    [Range(0, 1)] public float ratioPlayNodeActLow = 0.3f;
    [Tooltip("This number or More of Player Node Actions / Total Node Actions to trigger criteria")]
    [Range(0, 1)] public float ratioPlayNodeActHigh = 0.8f;
    [Tooltip("This number or Less of Player Target Attempts / Total Target Attempts to trigger criteria")]
    [Range(0, 1)] public float ratioPlayTargetAttLow = 0.3f;
    [Tooltip("This number or More of Player Target Attempts / Total Target Attempts to trigger criteria")]
    [Range(0, 1)] public float ratioPlayTargetAttHigh = 0.8f;
    [Tooltip("This number or Less of Player Move Actions / Turns to trigger criteria")]
    [Range(0, 1)] public float ratioPlayMoveActLow = 0.4f;
    [Tooltip("This number or More of Player Move Actions / Turns to trigger criteria")]
    [Range(0, 1)] public float ratioPlayMoveActHigh = 0.9f;
    [Tooltip("This number or Less of Player Days Lie Low / Total Days Lie Low to trigger criteria")]
    [Range(0, 1)] public float ratioPlayLieLowLow = 0.3f;
    [Tooltip("This number or More of Player Days Lie Low / Total Days Lie Low to trigger criteria")]
    [Range(0, 1)] public float ratioPlayLieLowHigh = 0.8f;
    [Tooltip("This number or Less of Gear Items Given (by Player to actor) / Total Gear Items acquired to trigger criteria")]
    [Range(0, 1)] public float ratioGiveGearLow = 0.3f;
    [Tooltip("This number or More of Gear Items Given (by Player to actor) / Total Gear Items acquired to trigger criteria")]
    [Range(0, 1)] public float ratioGiveGearHigh = 0.7f;
    [Tooltip("This number or Less of ManageActions / Turns  to trigger criteria")]
    [Range(0, 1)] public float ratioPlayManageActLow = 0.15f;
    [Tooltip("This number or More of ManageActions / Turns  to trigger criteria")]
    [Range(0, 1)] public float ratioPlayManageActHigh = 0.45f;
    [Tooltip("This number or Less of Do Nothing actions / Turn to trigger criteria")]
    [Range(0, 1)] public float ratioPlayDoNothingLow = 0.3f;
    [Tooltip("This number or More of Do Nothing actions / Turns  to trigger criteria")]
    [Range(0, 1)] public float ratioPlayDoNothingHigh = 0.7f;
    [Tooltip("This number or Less of Player Addicted Days / Turns to trigger criteria")]
    [Range(0, 1)] public float ratioPlayAddictLow = 0.2f;
    [Tooltip("This number or More of Player Addicted Days / Turns to trigger criteria")]
    [Range(0, 1)] public float ratioPlayAddictHigh = 0.7f;

    //hard coded renown amounts that correspond to effect Criteria equivalents (1/2/3/5)
    private int renownLow = 1;
    private int renownMed = 2;
    private int renownHigh = 3;
    private int renownExtreme = 5;

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
    private string actorStressedOverInvisibility;
    private string actorDoubleRenown;
    private string actorBlackmailNone;
    private string actorConflictPoison;
    private string actorConflictKill;
    private string actorNeverResigns;
    private string actorReserveTimerDoubled;
    private string actorReserveTimerHalved;
    //fast access -> assorted
    private int maxTargetInfo = -1;
    private int neutralStatValue = -1;
    private int maxStatValue = -1;
    private int maxSecretsAllowed = -1;
    private int chanceMotivationShift = -1;
    //fast access -> conditions
    private Condition conditionStressed;
    private Condition conditionCorrupt;
    private Condition conditionIncompetent;
    private Condition conditionQuestionable;
    private Condition conditionBlackmailer;
    private Condition conditionStar;
    private Condition conditionTagged;
    private Condition conditionWounded;
    private Condition conditionImaged;
    private Condition conditionDoomed;
    private Condition conditionAddicted;


    //colour palette for Modal Outcome
    private string colourGoodSide; //good effect Resisance / bad effect Authority
    private string colourBadSide; //bad effect Authority / bad effect Resistance
    private string colourGood;  //standard colour Good
    private string colourBad;   //standard colour Bad
    private string colourNeutral; //used when node is EqualsTo, eg. reset, or for Team Names
    private string colourNormal;
    private string colourCancel;
    private string colourDefault;
    private string colourAlert;
    private string colourGrey;
    private string colourActor;
    private string colourEnd;

    [HideInInspector] private static int ongoingEffectIDCounter = 0;              //used to sequentially number ongoing Effect ID's

    /// <summary>
    /// Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access
        delayNoSpider = GameManager.i.nodeScript.nodeNoSpiderDelay;
        delayYesSpider = GameManager.i.nodeScript.nodeYesSpiderDelay;
        actorStressedOverInvisibility = "ActorInvisibilityStress";
        actorDoubleRenown = "ActorDoubleRenown";
        actorBlackmailNone = "ActorBlackmailNone";
        actorConflictPoison = "ActorConflictPoison";
        actorConflictKill = "ActorConflictKill";
        actorNeverResigns = "ActorResignNone";
        actorReserveTimerDoubled = "ActorReserveTimerDoubled";
        actorReserveTimerHalved = "ActorReserveTimerHalved";
        conditionStressed = GameManager.i.dataScript.GetCondition("STRESSED");
        conditionCorrupt = GameManager.i.dataScript.GetCondition("CORRUPT");
        conditionIncompetent = GameManager.i.dataScript.GetCondition("INCOMPETENT");
        conditionQuestionable = GameManager.i.dataScript.GetCondition("QUESTIONABLE");
        conditionBlackmailer = GameManager.i.dataScript.GetCondition("BLACKMAILER");
        conditionStar = GameManager.i.dataScript.GetCondition("STAR");
        conditionTagged = GameManager.i.dataScript.GetCondition("TAGGED");
        conditionWounded = GameManager.i.dataScript.GetCondition("WOUNDED");
        conditionImaged = GameManager.i.dataScript.GetCondition("IMAGED");
        conditionDoomed = GameManager.i.dataScript.GetCondition("DOOMED");
        conditionAddicted = GameManager.i.dataScript.GetCondition("ADDICTED");
        Debug.Assert(conditionStressed != null, "Invalid conditionStressed (Null)");
        Debug.Assert(conditionCorrupt != null, "Invalid conditionCorrupt (Null)");
        Debug.Assert(conditionIncompetent != null, "Invalid conditionIncompetent (Null)");
        Debug.Assert(conditionQuestionable != null, "Invalid conditionQuestionable (Null)");
        Debug.Assert(conditionBlackmailer != null, "(Invalid conditionBlackmailer (Null)");
        Debug.Assert(conditionStar != null, "Invalid conditionStar (Null)");
        Debug.Assert(conditionTagged != null, "Invalid conditionTagged (Null)");
        Debug.Assert(conditionImaged != null, "Invalid conditionImaged (Null)");
        Debug.Assert(conditionWounded != null, "Invalid conditionWounded (Null)");
        Debug.Assert(conditionDoomed != null, "Invalid conditionDoomed (Null)");
        Debug.Assert(conditionAddicted != null, "Invalid conditionAddicted (Null)");
        //fast access -> teams
        teamArcCivil = GameManager.i.dataScript.GetTeamArcID("CIVIL");
        teamArcControl = GameManager.i.dataScript.GetTeamArcID("CONTROL");
        teamArcMedia = GameManager.i.dataScript.GetTeamArcID("MEDIA");
        teamArcProbe = GameManager.i.dataScript.GetTeamArcID("PROBE");
        teamArcSpider = GameManager.i.dataScript.GetTeamArcID("SPIDER");
        teamArcDamage = GameManager.i.dataScript.GetTeamArcID("DAMAGE");
        teamArcErasure = GameManager.i.dataScript.GetTeamArcID("ERASURE");
        //fast access -> other
        maxTargetInfo = GameManager.i.targetScript.maxTargetInfo;
        maxStatValue = GameManager.i.actorScript.maxStatValue;
        maxSecretsAllowed = GameManager.i.secretScript.secretMaxNum;
        neutralStatValue = GameManager.i.actorScript.neutralStatValue;
        chanceMotivationShift = GameManager.i.actorScript.chanceRelationShift;
        Debug.Assert(teamArcCivil > -1, "Invalid teamArcCivil (-1)");
        Debug.Assert(teamArcControl > -1, "Invalid teamArcControl (-1)");
        Debug.Assert(teamArcMedia > -1, "Invalid teamArcMedia (-1)");
        Debug.Assert(teamArcProbe > -1, "Invalid teamArcProbe (-1)");
        Debug.Assert(teamArcSpider > -1, "Invalid teamArcSpider (-1)");
        Debug.Assert(teamArcDamage > -1, "Invalid teamArcDamage (-1)");
        Debug.Assert(teamArcErasure > -1, "Invalid teamArcErasure (-1)");
        Debug.Assert(maxTargetInfo > -1, "Invalid maxTargetInfo (-1)");
        Debug.Assert(maxStatValue > -1, "Invalid maxStatValue (-1)");
        Debug.Assert(maxSecretsAllowed > -1, "Invalid maxSecretsAllowed (-1)");
        Debug.Assert(neutralStatValue > -1, "Invalid neutralStatValue (-1)");
        Debug.Assert(chanceMotivationShift > -1, "Invalid chanceMotivationShift (-1)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "EffectManager");
    }
    #endregion

    #endregion


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

    #region SetColours
    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        //output colours for good and bad depend on player side
        switch (GameManager.i.sideScript.PlayerSide.name)
        {
            case "Resistance":
                colourGoodSide = GameManager.i.colourScript.GetColour(ColourType.goodText);
                colourBadSide = GameManager.i.colourScript.GetColour(ColourType.badText);
                break;
            case "Authority":
                colourGoodSide = GameManager.i.colourScript.GetColour(ColourType.badText);
                colourBadSide = GameManager.i.colourScript.GetColour(ColourType.goodText);
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.i.sideScript.PlayerSide.name));
                break;
        }
        colourGood = GameManager.i.colourScript.GetColour(ColourType.goodText);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.badText);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        colourDefault = GameManager.i.colourScript.GetColour(ColourType.whiteText);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourCancel = GameManager.i.colourScript.GetColour(ColourType.moccasinText);
        colourActor = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourGrey = GameManager.i.colourScript.GetColour(ColourType.greyText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }
    #endregion

    #region CheckCriteria
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
        Organisation org = null;
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        if (data.listOfCriteria != null && data.listOfCriteria.Count > 0)
        {
            //
            // - - - access necessary data prior to loop
            //
            //Get node if required
            if (data.nodeID > -1)
            {
                node = GameManager.i.dataScript.GetNode(data.nodeID);
                if (node == null)
                { Debug.LogError("Invalid node (null)"); errorFlag = true; }
            }
            //Get actor if required
            if (data.actorSlotID > -1)
            {
                //OnMap actor
                actor = GameManager.i.dataScript.GetCurrentActor(data.actorSlotID, playerSide);
                if (actor == null)
                {
                    Debug.LogErrorFormat("Invalid actorSlotID \"{0}\" -> Criteria Check cancelled", data.actorSlotID);
                    errorFlag = true;
                }
            }
            else if (data.actorHqID > -1)
            {
                //HQ actor
                actor = GameManager.i.dataScript.GetHqActor(data.actorHqID);
                if (actor == null)
                {
                    Debug.LogErrorFormat("Invalid actorHqID \"{0}\" -> Criteria Check cancelled", data.actorHqID);
                    errorFlag = true;
                }
            }
            //Get TeamArc if required
            if (data.teamArcID > -1)
            {
                //get team
                teamArc = GameManager.i.dataScript.GetTeamArc(data.teamArcID);
                if (teamArc == null)
                {
                    Debug.LogError(string.Format("Invalid TeamArc (null) for teamArcID \"{0}\" -> Criteria check cancelled", data.teamArcID));
                    errorFlag = true;
                }
            }
            //Get Organisation if required
            if (string.IsNullOrEmpty(data.orgName) == false)
            {
                //get org
                org = GameManager.i.dataScript.GetOrganisaton(data.orgName);
                if (org == null)
                {
                    Debug.LogErrorFormat("Invalid Organisation (Null) for orgName \"{0}\"", data.orgName);
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
                        if (criteria.effectCriteria != null)
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
                                            switch (criteria.effectCriteria.name)
                                            {
                                                case "NodeSecurityNOTMin":
                                                    val = GameManager.i.nodeScript.minNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Security, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Security " + compareTip); }
                                                    break;
                                                case "NodeSecurityNOTMax":
                                                    val = GameManager.i.nodeScript.maxNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Security, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Security " + compareTip); }
                                                    break;
                                                case "NodeStabilityNOTMin":
                                                    val = GameManager.i.nodeScript.minNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Stability, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Stability " + compareTip); }
                                                    break;
                                                case "NodeStabilityLow":
                                                    val = GameManager.i.nodeScript.medNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Stability, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Stability " + compareTip); }
                                                    break;
                                                case "NodeStabilityNOTMax":
                                                    val = GameManager.i.nodeScript.maxNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Stability, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Stability " + compareTip); }
                                                    break;
                                                case "NodeSupportNOTMax":
                                                    val = GameManager.i.nodeScript.maxNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Support, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Support " + compareTip); }
                                                    break;
                                                case "NodeSupportNOTMin":
                                                    val = GameManager.i.nodeScript.minNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Support, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Support " + compareTip); }
                                                    break;
                                                case "NumTeamsMin":
                                                    val = GameManager.i.teamScript.minTeamsAtNode;
                                                    compareTip = ComparisonCheck(val, node.CheckNumOfTeams(), criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "no Teams present"); }
                                                    break;
                                                case "NumTeamsMax":
                                                    val = GameManager.i.teamScript.maxTeamsAtNode;
                                                    compareTip = ComparisonCheck(val, node.CheckNumOfTeams(), criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Max teams present"); }
                                                    break;
                                                case "NumTracers":
                                                    if (node.isTracer == true)
                                                    { BuildString(result, "Tracer already present"); }
                                                    break;
                                                case "TargetInfoMax":
                                                    val = GameManager.i.targetScript.maxTargetInfo;
                                                    Target target = GameManager.i.dataScript.GetTarget(node.targetName);
                                                    if (target != null)
                                                    {
                                                        compareTip = ComparisonCheck(val, target.intel, criteria.comparison);
                                                        if (compareTip != null)
                                                        { BuildString(result, "Full Info already"); }
                                                    }
                                                    else { Debug.LogErrorFormat("Invalid target (Null) for {0}", node.targetName); }
                                                    break;
                                                case "TargetPresent":
                                                    //check that a target is present at the node
                                                    if (string.IsNullOrEmpty(node.targetName) == true)
                                                    { BuildString(result, "No Target present"); }
                                                    break;
                                                case "TeamActorAbility":
                                                    //actor can only have a number of teams OnMap equal to their ability at any time
                                                    if (actor != null)
                                                    {
                                                        if (actor.CheckNumOfTeams() >= actor.GetDatapoint(ActorDatapoint.Ability2))
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
                                                        if (GameManager.i.dataScript.CheckTeamInfo(data.teamArcID, TeamInfo.Reserve) < 1)
                                                        { BuildString(result, string.Format("No {0} Team available", teamArc.name)); }
                                                    }
                                                    else
                                                    { Debug.LogError(string.Format("Invalid teamArc (null) for criteria \"{0}\"", criteria.name)); errorFlag = true; }
                                                    break;
                                                case "TeamAny":
                                                    //there must be a spare team of any type in the reserve pool
                                                    if (teamArc != null)
                                                    {
                                                        if (GameManager.i.dataScript.CheckTeamPoolCount(TeamPool.Reserve) < 1)
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
                                            switch (criteria.effectCriteria.name)
                                            {
                                                case "NodeStabilityNOTMin":
                                                    //at least one neighbouring node must have stability > 0
                                                    val = GameManager.i.nodeScript.minNodeValue;
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
                                            Debug.LogError(string.Format("Invalid node (null) for criteria \"{0}\"", criteria.name));
                                            errorFlag = true;
                                        }
                                        break;
                                    //
                                    // - - - All Nodes - - -
                                    //
                                    case "NodeAll":
                                        if (node != null)
                                        {
                                            switch (criteria.effectCriteria.name)
                                            {
                                                case "TargetPresent":
                                                    //There must be at least one Live target
                                                    List<Target> listOfLiveTargets = GameManager.i.dataScript.GetTargetPool(Status.Live);
                                                    if (listOfLiveTargets != null)
                                                    {
                                                        if (listOfLiveTargets.Count == 0)
                                                        { BuildString(result, "No Targets present"); }
                                                        else
                                                        {
                                                            bool isSuccess = false;
                                                            //loop targets looking for at least one who has targetInfo < max
                                                            foreach (Target target in listOfLiveTargets)
                                                            {
                                                                if (target.intel < maxTargetInfo)
                                                                { isSuccess = true; break; }
                                                            }
                                                            if (isSuccess == false)
                                                            { BuildString(result, "All Targets have MAX Info already"); }
                                                        }
                                                    }
                                                    else { Debug.LogError("Invalid listOfLiveTargets (Null)"); errorFlag = true; }
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
                                            Debug.LogError(string.Format("Invalid node (null) for criteria \"{0}\"", criteria.name));
                                            errorFlag = true;
                                        }
                                        break;
                                    //
                                    // - - - City - - -
                                    //
                                    case "City":
                                        switch (criteria.effectCriteria.name)
                                        {
                                            case "CityLoyaltyNOTMin":
                                                if (GameManager.i.cityScript.CityLoyalty == 0)
                                                { BuildString(result, string.Format(" City Loyalty can't be Zero")); }
                                                break;
                                            case "CityLoyaltyNOTMax":
                                                if (GameManager.i.cityScript.CityLoyalty >= GameManager.i.cityScript.maxCityLoyalty)
                                                { BuildString(result, string.Format(" City Loyalty can't be MAX")); }
                                                break;
                                        }
                                        break;
                                    //
                                    // - - - Current Actor / Player - - -
                                    //
                                    case "ActorCurrent":
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
                                                        if (GameManager.i.playerScript.CheckConditionPresent(conditionStressed, playerSide) == false)
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
                                                        { BuildString(result, string.Format(" {0} already {1}STRESSED{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.i.playerScript.CheckConditionPresent(conditionStressed, playerSide) == true)
                                                        { BuildString(result, string.Format(" Player already {0}STRESSED{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionStressed (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionCorruptNo":
                                                if (conditionCorrupt != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        //actor
                                                        if (actor.CheckConditionPresent(conditionCorrupt) == true)
                                                        { BuildString(result, string.Format(" {0} already {1}CORRUPT{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.i.playerScript.CheckConditionPresent(conditionCorrupt, playerSide) == true)
                                                        { BuildString(result, string.Format(" Player already {0}CORRUPT{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionCorrupt (Null)"); errorFlag = true; }
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
                                                        if (GameManager.i.playerScript.CheckConditionPresent(conditionCorrupt, playerSide) == false)
                                                        { BuildString(result, string.Format(" Player isn't {0}CORRUPT{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionCorrupt (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionIncompetentNo":
                                                if (conditionIncompetent != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        //actor
                                                        if (actor.CheckConditionPresent(conditionIncompetent) == true)
                                                        { BuildString(result, string.Format(" {0} already {1}INCOMPETENT{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.i.playerScript.CheckConditionPresent(conditionIncompetent, playerSide) == true)
                                                        { BuildString(result, string.Format(" Player already {0}INCOMPETENT{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionIncompetent (Null)"); errorFlag = true; }
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
                                                        if (GameManager.i.playerScript.CheckConditionPresent(conditionIncompetent, playerSide) == false)
                                                        { BuildString(result, string.Format(" Player isn't {0}INCOMPETENT{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionIncompetent (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionQuestionableNo":
                                                if (conditionQuestionable != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        if (actor.CheckConditionPresent(conditionQuestionable) == true)
                                                        { BuildString(result, string.Format(" {0} already {1}QUESTIONABLE{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.i.playerScript.CheckConditionPresent(conditionQuestionable, playerSide) == true)
                                                        { BuildString(result, string.Format(" Player already {0}QUESTIONABLE{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionQuestionable (Null)"); errorFlag = true; }
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
                                                        if (GameManager.i.playerScript.CheckConditionPresent(conditionQuestionable, playerSide) == false)
                                                        { BuildString(result, string.Format(" Player isn't {0}QUESTIONABLE{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionQuestionable (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionStarNo":
                                                if (conditionStar != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        if (actor.CheckConditionPresent(conditionStar) == true)
                                                        { BuildString(result, string.Format(" {0} already a {1}STAR{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.i.playerScript.CheckConditionPresent(conditionStar, playerSide) == true)
                                                        { BuildString(result, string.Format(" Player already a {0}STAR{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionStar (Null)"); errorFlag = true; }
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
                                                        if (GameManager.i.playerScript.CheckConditionPresent(conditionStar, playerSide) == false)
                                                        { BuildString(result, string.Format(" Player isn't a {0}STAR{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionStar (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionAddictedNo":
                                                if (conditionAddicted != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        if (actor.CheckConditionPresent(conditionAddicted) == true)
                                                        { BuildString(result, string.Format(" {0} already {1}ADDICTED{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.i.playerScript.CheckConditionPresent(conditionAddicted, playerSide) == true)
                                                        { BuildString(result, string.Format(" Player already {0}ADDICTED{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionAddicted (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionAddictedYes":
                                                if (conditionAddicted != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        if (actor.CheckConditionPresent(conditionAddicted) == false)
                                                        { BuildString(result, string.Format(" {0} isn't {1}ADDICTED{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    {
                                                        //player
                                                        if (GameManager.i.playerScript.CheckConditionPresent(conditionAddicted, playerSide) == false)
                                                        { BuildString(result, string.Format(" Player isn't {0}ADDICTED{1}", colourNeutral, colourEnd)); }
                                                    }
                                                }
                                                else { Debug.LogWarning("Invalid conditionAddicted (Null)"); errorFlag = true; }
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
                                                else { Debug.LogWarning("Invalid conditionBlackmailer (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionBlackmailerYes":
                                                //actor only  has the 'Blackmailer' condition
                                                if (conditionBlackmailer != null)
                                                {
                                                    if (actor != null)
                                                    {
                                                        //actor
                                                        if (actor.CheckConditionPresent(conditionBlackmailer) == false)
                                                        { BuildString(result, string.Format(" {0} doesn't have {1}BLACKMAILING{2}", actor.actorName, colourNeutral, colourEnd)); }
                                                    }
                                                    else
                                                    { Debug.LogWarning("Invalid actor (Null) for ConditionBlackmailerNo"); }
                                                }
                                                else { Debug.LogWarning("Invalid conditionBlackmailer (Null)"); errorFlag = true; }
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
                                                    { BuildString(result, string.Format(" {0} doesn't have required trait", actor.actorName)); }
                                                }
                                                else
                                                { Debug.LogWarning("Invalid actor (Null) for TraitConflictKillYes"); }
                                                break;
                                            case "MotivationNeutralMin":
                                                //Actor motivation is neutral (2) or better
                                                if (actor != null)
                                                {
                                                    if (actor.GetDatapoint(ActorDatapoint.Motivation1) < neutralStatValue)
                                                    { BuildString(result, string.Format(" {0} Low Motivation (need 2+)", actor.arc.name)); }
                                                }
                                                else { Debug.LogWarning("Invalid actor (Null) for MotivationNeutralMin"); }
                                                break;
                                            case "MotivationNOTZero":
                                                //Actor motivation is 1+
                                                if (actor != null)
                                                {
                                                    if (actor.GetDatapoint(ActorDatapoint.Motivation1) == 0)
                                                    { BuildString(result, string.Format(" {0} Motivation Zero", actor.arc.name)); }
                                                }
                                                else { Debug.LogWarning("Invalid actor (Null) for MotivationNOTZero"); }
                                                break;
                                            case "RenownReserveMin":
                                                //player
                                                int renownReserve = GameManager.i.actorScript.manageReserveRenown;
                                                playerRenown = GameManager.i.playerScript.Renown;
                                                if (playerRenown < renownReserve)
                                                {
                                                    BuildString(result, string.Format("You need at least {0}{1}{2}{3} Renown {4}(currently {5}{6}{7})", "\n", colourNeutral, renownReserve,
                                                      colourEnd, "\n", colourNeutral, playerRenown, colourEnd));
                                                }
                                                break;
                                            case "RenownDismissMin":
                                                //player -> extra cost for actor knowing secrets and threatening player
                                                ManageRenownCost manageDismissCost = GameManager.i.actorScript.GetManageRenownCost(actor, GameManager.i.actorScript.manageDismissRenown);
                                                int renownDismiss = manageDismissCost.renownCost;
                                                playerRenown = GameManager.i.playerScript.Renown;
                                                if (playerRenown < renownDismiss)
                                                {
                                                    BuildString(result, string.Format("You need at least {0}{1}{2}{3} Renown {4}(currently {5}{6}{7})", "\n", colourNeutral, renownDismiss,
                                                      colourEnd, "\n", colourNeutral, playerRenown, colourEnd));
                                                    if (string.IsNullOrEmpty(manageDismissCost.tooltip) == false)
                                                    { result.Append(manageDismissCost.tooltip); }
                                                }
                                                break;
                                            case "RenownDisposeMin":
                                                //player -> extra cost for actor knowing secrets and threatening player
                                                ManageRenownCost manageDisposeCost = GameManager.i.actorScript.GetManageRenownCost(actor, GameManager.i.actorScript.manageDisposeRenown);
                                                int renownDispose = manageDisposeCost.renownCost;
                                                playerRenown = GameManager.i.playerScript.Renown;
                                                if (playerRenown < renownDispose)
                                                {
                                                    BuildString(result, string.Format("You need at least {0}{1}{2}{3} Renown {4}(currently {5}{6}{7})", "\n", colourNeutral, renownDispose,
                                                      colourEnd, "\n", colourNeutral, playerRenown, colourEnd));
                                                    if (string.IsNullOrEmpty(manageDisposeCost.tooltip) == false)
                                                    { result.Append(manageDisposeCost.tooltip); }
                                                }
                                                break;
                                            case "RenownNOTZeroActor":
                                                if (actor != null)
                                                {
                                                    if (actor.Renown == 0)
                                                    { BuildString(result, "Renown Zero"); }
                                                }
                                                break;
                                            case "NumRecruitsCurrent":
                                                //check max. number of recruits in reserve pool not exceeded
                                                val = GameManager.i.dataScript.CheckNumOfActorsInReserve();
                                                compareTip = ComparisonCheck(GameManager.i.actorScript.maxNumOfReserveActors, val, criteria.comparison);
                                                if (compareTip != null)
                                                { BuildString(result, "maxxed Recruit allowance"); }
                                                break;
                                            case "InvisibilityNOTMax":
                                                //check invisibility is less than the max value -> Actor / Player
                                                if (actor != null)
                                                { val = actor.GetDatapoint(ActorDatapoint.Invisibility2); }
                                                else { val = GameManager.i.playerScript.Invisibility; }
                                                if (val == GameManager.i.actorScript.maxStatValue)
                                                { BuildString(result, "Invisibility at Max"); }
                                                break;
                                            case "InvisibilityNOTZero":
                                                //check invisibility greater than Zero -> Actor / Player
                                                if (actor != null)
                                                { val = actor.GetDatapoint(ActorDatapoint.Invisibility2); }
                                                else { val = GameManager.i.playerScript.Invisibility; }
                                                if (val == 0)
                                                { BuildString(result, "Invisibility Zero"); }
                                                break;
                                            case "InvisibilityLow":
                                                //check invisibility Less than 2 -> Actor / Player
                                                if (actor != null)
                                                { val = actor.GetDatapoint(ActorDatapoint.Invisibility2); }
                                                else { val = GameManager.i.playerScript.Invisibility; }
                                                if (val > 1)
                                                { BuildString(result, "Invisibility > 1"); }
                                                break;
                                            case "SecretsNOTMin":
                                                //check num of secrets greater than Zero -> Actor / Player
                                                if (actor != null)
                                                { val = actor.CheckNumOfSecrets(); }
                                                else { val = GameManager.i.playerScript.CheckNumOfSecrets(); }
                                                if (val == 0)
                                                { BuildString(result, "Secrets Zero"); }
                                                break;
                                            case "SecretsNOTMax":
                                                //check num of secrets less than Max allowed -> Actor / Player
                                                if (actor != null)
                                                { val = actor.CheckNumOfSecrets(); }
                                                else { val = GameManager.i.playerScript.CheckNumOfSecrets(); }
                                                if (val >= maxSecretsAllowed)
                                                { BuildString(result, "Secrets MAX"); }
                                                break;
                                            case "ActiveActorSlot0":
                                                //check there is an active actor present onMap at slot 0
                                                if (GameManager.i.dataScript.CheckActiveActorPresent(0, playerSide) == false)
                                                { BuildString(result, "No Active Subordinate"); }
                                                break;
                                            case "ActiveActorSlot1":
                                                //check there is an active actor present onMap at slot 1
                                                if (GameManager.i.dataScript.CheckActiveActorPresent(1, playerSide) == false)
                                                { BuildString(result, "No Active Subordinate"); }
                                                break;
                                            case "ActiveActorSlot2":
                                                //check there is an active actor present onMap at slot 2
                                                if (GameManager.i.dataScript.CheckActiveActorPresent(2, playerSide) == false)
                                                { BuildString(result, "No Active Subordinate"); }
                                                break;
                                            case "ActiveActorSlot3":
                                                //check there is an active actor present onMap at slot 3
                                                if (GameManager.i.dataScript.CheckActiveActorPresent(3, playerSide) == false)
                                                { BuildString(result, "No Active Subordinate"); }
                                                break;
                                            default:
                                                BuildString(result, "Error!");
                                                Debug.LogWarning(string.Format("ActorCurrent: Invalid effect.criteriaEffect \"{0}\"", criteria.effectCriteria.name));
                                                errorFlag = true;
                                                break;
                                        }
                                        break;
                                    //
                                    // - - - Player (new, older ones are in Current Actor / Player above) - - -
                                    //
                                    case "Player":
                                        switch (criteria.effectCriteria.name)
                                        {
                                            case "ConditionTaggedNo":
                                                //player only  does NOT have the 'Tagged' condition
                                                if (conditionTagged != null)
                                                {
                                                    if (GameManager.i.playerScript.CheckConditionPresent(conditionTagged, playerSide) == true)
                                                    { BuildString(result, string.Format(" Player already {0}TAGGED{1}", colourNeutral, colourEnd)); }
                                                }
                                                else { Debug.LogWarning("Invalid conditionTagged (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionTaggedYes":
                                                //player only has the 'Tagged' condition
                                                if (conditionTagged != null)
                                                {
                                                    if (GameManager.i.playerScript.CheckConditionPresent(conditionTagged, playerSide) == false)
                                                    { BuildString(result, string.Format(" Player not {0}TAGGED{1}", colourNeutral, colourEnd)); }
                                                }
                                                else { Debug.LogWarning("Invalid conditionTagged (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionWoundedNo":
                                                //player only  does NOT have the 'Wounded' condition
                                                if (conditionWounded != null)
                                                {
                                                    if (GameManager.i.playerScript.CheckConditionPresent(conditionWounded, playerSide) == true)
                                                    { BuildString(result, string.Format(" Player already {0}WOUNDED{1}", colourNeutral, colourEnd)); }
                                                }
                                                else { Debug.LogWarning("Invalid conditionWounded (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionWoundedYes":
                                                //player only has the 'Wounded' condition
                                                if (conditionWounded != null)
                                                {
                                                    if (GameManager.i.playerScript.CheckConditionPresent(conditionWounded, playerSide) == false)
                                                    { BuildString(result, string.Format(" Player not {0}WOUNDED{1}", colourNeutral, colourEnd)); }
                                                }
                                                else { Debug.LogWarning("Invalid conditionWounded (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionImagedNo":
                                                //player only  does NOT have the 'Imaged' condition
                                                if (conditionImaged != null)
                                                {
                                                    if (GameManager.i.playerScript.CheckConditionPresent(conditionImaged, playerSide) == true)
                                                    { BuildString(result, string.Format(" Player already {0}IMAGED{1}", colourNeutral, colourEnd)); }
                                                }
                                                else { Debug.LogWarning("Invalid conditionImaged (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionImagedYes":
                                                //player only has the 'Imaged' condition
                                                if (conditionImaged != null)
                                                {
                                                    if (GameManager.i.playerScript.CheckConditionPresent(conditionImaged, playerSide) == false)
                                                    { BuildString(result, string.Format(" Player not {0}IMAGED{1}", colourNeutral, colourEnd)); }
                                                }
                                                else { Debug.LogWarning("Invalid conditionImaged (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionDoomedNo":
                                                //player only  does NOT have the 'Doomed' condition
                                                if (conditionDoomed != null)
                                                {
                                                    if (GameManager.i.playerScript.CheckConditionPresent(conditionDoomed, playerSide) == true)
                                                    { BuildString(result, string.Format(" Player already {0}DOOMED{1}", colourNeutral, colourEnd)); }
                                                }
                                                else { Debug.LogWarning("Invalid conditionDoomed (Null)"); errorFlag = true; }
                                                break;
                                            case "ConditionDoomedYes":
                                                //player only has the 'Doomed' condition
                                                if (conditionDoomed != null)
                                                {
                                                    if (GameManager.i.playerScript.CheckConditionPresent(conditionDoomed, playerSide) == false)
                                                    { BuildString(result, string.Format(" Player not {0}DOOMED{1}", colourNeutral, colourEnd)); }
                                                }
                                                else { Debug.LogWarning("Invalid conditionDoomed (Null)"); errorFlag = true; }
                                                break;
                                            case "NodeActionsNOTZero":
                                                //player has listOfNodeActions.Count > 0
                                                if (GameManager.i.playerScript.CheckPlayerSpecial(PlayerCheck.NodeActionsNOTZero) == false)
                                                { BuildString(result, "Player has NO NodeActions"); }
                                                break;
                                            case "RenownPlayerLow":
                                                //Player has Renown Low or better
                                                if (GameManager.i.playerScript.Renown < renownLow)
                                                { BuildString(result, string.Format("Not enough Renown{0}(need {1})", "\n", renownLow)); }
                                                break;
                                            case "RenownPlayerMed":
                                                //Player has Renown Med or better
                                                if (GameManager.i.playerScript.Renown < renownMed)
                                                { BuildString(result, string.Format("Not enough Renown{0}(need {1})", "\n", renownMed)); }
                                                break;
                                            case "RenownPlayerHigh":
                                                //Player has Renown High or better
                                                if (GameManager.i.playerScript.Renown < renownHigh)
                                                { BuildString(result, string.Format("Not enough Renown{0}(need {1})", "\n", renownHigh)); }
                                                break;
                                            case "RenownPlayerExt":
                                                //Player has Renown Low or better
                                                if (GameManager.i.playerScript.Renown < renownExtreme)
                                                { BuildString(result, string.Format("Not enough Renown{0}(need {1})", "\n", renownExtreme)); }
                                                break;
                                            case "RenownNOTZeroPlayer":
                                                //Player
                                                if (GameManager.i.playerScript.Renown == 0)
                                                { BuildString(result, "Renown Zero"); }
                                                break;
                                            case "InvestigationNormal":
                                                //Player has a current, normal, ongoing investigation that hasn't had intervention by orgHQ
                                                if (GameManager.i.playerScript.CheckInvestigationNormal() == false)
                                                { BuildString(result, string.Format("No valid Investigation present{0}", "\n")); }
                                                break;
                                            case "InvestigationTimer":
                                                //Player has a current, resolution phase, guilty verdict imminent, investigation that hasn't had intervention by orgHQ
                                                if (GameManager.i.playerScript.CheckInvestigationTimer() == false)
                                                { BuildString(result, string.Format("No valid Investigation present{0}", "\n")); }
                                                break;
                                            case "PlayerCapturedNo":
                                                //Player (Human) not captured
                                                if (GameManager.i.playerScript.status == ActorStatus.Captured)
                                                { BuildString(result, string.Format("Player Captured{0}", "\n")); }
                                                break;
                                            case "PlayerCapturedYes":
                                                //Player (Human) not captured
                                                if (GameManager.i.playerScript.status != ActorStatus.Captured)
                                                { BuildString(result, string.Format("Player not Captured{0}", "\n")); }
                                                break;
                                            case "InnocenceHigh":
                                                //Player Innocence at 3 stars exactly
                                                if (GameManager.i.playerScript.Innocence != 3)
                                                { BuildString(result, string.Format("Innocence not 3 stars{0}", "\n")); }
                                                break;
                                            case "InnocenceMedium":
                                                //Player Innocence at 2 stars exactly
                                                if (GameManager.i.playerScript.Innocence != 2)
                                                { BuildString(result, string.Format("Innocence not 2 stars{0}", "\n")); }
                                                break;
                                            case "InnocenceLow":
                                                //Player Innocence at 1 star exactly
                                                if (GameManager.i.playerScript.Innocence != 1)
                                                { BuildString(result, string.Format("Innocence not 1 star{0}", "\n")); }
                                                break;
                                            case "InnocenceZero":
                                                //Player Innocence at 0 stars exactly
                                                if (GameManager.i.playerScript.Innocence != 0)
                                                { BuildString(result, string.Format("Innocence not 0 stars{0}", "\n")); }
                                                break;
                                            case "InnocenceNOTMin":
                                                //Player Innocence > 0
                                                if (GameManager.i.playerScript.Innocence == 0)
                                                { BuildString(result, string.Format("Innocence is Zero{0}", "\n")); }
                                                break;
                                            case "InnocenceNOTMax":
                                                //Player Innocence < Max
                                                if (GameManager.i.playerScript.Innocence == 3)
                                                { BuildString(result, string.Format("Innocence is Maxxed{0}", "\n")); }
                                                break;
                                            case "CaptureTool0":
                                                //Player has CaptureTool for innocence level 0 incarceration in their possession
                                                if (GameManager.i.playerScript.CheckCaptureToolPresent(0) == false)
                                                { BuildString(result, string.Format("Player doesn't have Item{0}", "\n")); }
                                                break;
                                            case "CaptureTool1":
                                                //Player has CaptureTool for innocence level 1 incarceration in their possession
                                                if (GameManager.i.playerScript.CheckCaptureToolPresent(1) == false)
                                                { BuildString(result, string.Format("Player doesn't have Item{0}", "\n")); }
                                                break;
                                            case "CaptureTool2":
                                                //Player has CaptureTool for innocence level 2 incarceration in their possession
                                                if (GameManager.i.playerScript.CheckCaptureToolPresent(2) == false)
                                                { BuildString(result, string.Format("Player doesn't have Item{0}", "\n")); }
                                                break;
                                            case "CaptureTool3":
                                                //Player has CaptureTool for innocence level 1 incarceration in their possession
                                                if (GameManager.i.playerScript.CheckCaptureToolPresent(3) == false)
                                                { BuildString(result, string.Format("Player doesn't have Item{0}", "\n")); }
                                                break;
                                            default:
                                                BuildString(result, "Error!");
                                                Debug.LogWarning(string.Format("Invalid criteria.effectcriteria.name \"{0}\"", criteria.effectCriteria.name));
                                                errorFlag = true;
                                                break;
                                        }
                                        break;
                                    //
                                    // - - - All Actors
                                    //
                                    case "ActorAll":
                                        switch (criteria.effectCriteria.name)
                                        {
                                            case "ActiveActorsNOTZero":
                                                //at least one active, onMap actor preesent
                                                if (GameManager.i.dataScript.CheckNumOfActiveActors(playerSide) == 0)
                                                { BuildString(result, "No Active Actors OnMap"); }
                                                break;
                                            case "ActiveActorsMinTwo":
                                                //at least two active, onMap actor preesent
                                                if (GameManager.i.dataScript.CheckNumOfActiveActors(playerSide) < 2)
                                                { BuildString(result, "Less than two active actors OnMap"); }
                                                break;
                                            case "ActiveActorsConflict":
                                                //at least one active, OnMap actor who has had a relationship conflict with Player
                                                if (GameManager.i.dataScript.CheckNumOfActiveActorsSpecial(ActorCheck.ActorConflictNOTZero, playerSide) == 0)
                                                { BuildString(result, "No actors with conflicts"); }
                                                break;
                                            case "ActiveActorGear":
                                                //at least one active actor has personal gear
                                                if (GameManager.i.dataScript.CheckNumOfActiveActorsSpecial(ActorCheck.PersonalGearYes, playerSide) == 0)
                                                { BuildString(result, "No Active Actors with Gear"); }
                                                break;
                                            case "ActiveActorGearNo":
                                                //at least one active actor does NOT have personal gear
                                                if (GameManager.i.dataScript.CheckNumOfActiveActorsSpecial(ActorCheck.PersonalGearNo, playerSide) == 0)
                                                { BuildString(result, "No Active Actors WITHOUT Gear"); }
                                                break;
                                            case "ActorCompatibilityNOTZero":
                                                //at least one actor with Compatibility NOT Zero (+/- 1 or 2)
                                                if (GameManager.i.dataScript.CheckNumOfActiveActorsSpecial(ActorCheck.CompatibilityNOTZero, playerSide) == 0)
                                                { BuildString(result, "No Compatibility NOT Zero Actors OnMap"); }
                                                break;
                                            case "NodeActionsNOTZero":
                                                //at least one actor has listOfNodeActions.Count > 0 with valid topics present
                                                if (GameManager.i.dataScript.CheckNumOfActiveActorsSpecial(ActorCheck.NodeActionsNOTZero, playerSide) == 0)
                                                { BuildString(result, "No actors with NodeActions OnMap"); }
                                                break;
                                            case "TeamActionsNOTZero":
                                                //at least one actor has listOfTeamActions.Count > 0 with valid topics present
                                                if (GameManager.i.dataScript.CheckNumOfActiveActorsSpecial(ActorCheck.TeamActionsNOTZero, playerSide) == 0)
                                                { BuildString(result, "No actors with TeamActions OnMap"); }
                                                break;
                                            case "ContactsActorMin":
                                                //at least one actor present who has one active contact
                                                if (GameManager.i.dataScript.CheckNumOfActiveActorsSpecial(ActorCheck.ActorContactMin, playerSide) == 0)
                                                { BuildString(result, "No actors with NodeActions OnMap"); }
                                                break;
                                            case "ContactsActorNOTMax":
                                                //at least one actor has less than the max. allowed number of contacts
                                                if (GameManager.i.dataScript.CheckNumOfActiveActorsSpecial(ActorCheck.ActorContactNOTMax, playerSide) == 0)
                                                { BuildString(result, "No actors with NodeActions OnMap"); }
                                                break;
                                            case "RenownActorsMore":
                                                //at least one actor has more renown than player
                                                if (GameManager.i.dataScript.CheckNumOfActiveActorsSpecial(ActorCheck.RenownMore, playerSide) == 0)
                                                { BuildString(result, "No actors with more Renown than Player"); }
                                                break;
                                            case "RenownActorsLess":
                                                //at least one actor has less renown than player
                                                if (GameManager.i.dataScript.CheckNumOfActiveActorsSpecial(ActorCheck.RenownLess, playerSide) == 0)
                                                { BuildString(result, "No actors with less Renown than Player"); }
                                                break;
                                            case "ActorsKnowSecret":
                                                //at least one actor knows at least one of the player's secrets
                                                if (GameManager.i.dataScript.CheckNumOfActiveActorsSpecial(ActorCheck.KnowsSecret, playerSide) == 0)
                                                { BuildString(result, "No actors know any Player Secrets"); }
                                                break;
                                            case "ActorsKnowNothing":
                                                //at least one actor knows none of the player's secrets
                                                if (GameManager.i.dataScript.CheckNumOfActiveActorsSpecial(ActorCheck.KnowsNothing, playerSide) == 0)
                                                { BuildString(result, "All actors know Player Secrets"); }
                                                break;
                                            case "RelationshipPossible":
                                                //1+ actors present onMap who are active and don't have a current relationship (or do and their timer is zero)
                                                if (GameManager.i.dataScript.CheckIfRelationPossible() == false)
                                                { BuildString(result, "Relationship isn't possible"); }
                                                break;
                                            case "DismissedActorsNOTZero":
                                                List<int> tempDismissedActors = new List<int>(GameManager.i.dataScript.GetListOfDismissedActors(playerSide));
                                                if (tempDismissedActors.Count == 0)
                                                { BuildString(result, "no Dismissed Subordinates"); }
                                                break;
                                            case "ResignedActorsNOTZero":
                                                List<int> tempResignedActors = new List<int>(GameManager.i.dataScript.GetListOfResignedActors(playerSide));
                                                if (tempResignedActors.Count == 0)
                                                { BuildString(result, "no Resigned Subordinates"); }
                                                break;
                                            default:
                                                BuildString(result, "Error!");
                                                Debug.LogWarning(string.Format("Invalid criteria.effectcriteria.name \"{0}\"", criteria.effectCriteria.name));
                                                errorFlag = true;
                                                break;
                                        }
                                        break;
                                    //
                                    // - - - Gear Inventory - - -
                                    //
                                    case "GearInventory":
                                        switch (criteria.effectCriteria.name)
                                        {
                                            case "GearInventoryMin":
                                                //at least one item in player's gear inventory
                                                if (GameManager.i.playerScript.CheckNumOfGear() == 0)
                                                { BuildString(result, "no gear in Inventory"); }
                                                break;
                                            case "GearInventoryNOTMax":
                                                //space remaining in Player's gear inventory
                                                if (GameManager.i.playerScript.CheckNumOfGear() >= GameManager.i.gearScript.maxNumOfGear)
                                                { BuildString(result, "maxxed Gear Allowance"); }
                                                break;
                                            case "GearAvailableCommon":
                                                //checks to see if at least 1 piece of unused Common gear is available
                                                List<string> tempCommonGear = new List<string>(GameManager.i.dataScript.GetListOfGear(GameManager.i.gearScript.gearCommon));
                                                if (tempCommonGear.Count == 0)
                                                { BuildString(result, "no Common Gear available"); }
                                                break;
                                            case "GearAvailableRare":
                                                //checks to see if at least 1 piece of unused Rare gear is available
                                                List<string> tempRareGear = new List<string>(GameManager.i.dataScript.GetListOfGear(GameManager.i.gearScript.gearRare));
                                                if (tempRareGear.Count == 0)
                                                { BuildString(result, "no Rare Gear available"); }
                                                break;
                                            case "GearAvailableUnique":
                                                //checks to see if at least 1 piece of unused Unique gear is available
                                                List<string> tempUniqueGear = new List<string>(GameManager.i.dataScript.GetListOfGear(GameManager.i.gearScript.gearUnique));
                                                if (tempUniqueGear.Count == 0)
                                                { BuildString(result, "no Unique Gear available"); }
                                                break;
                                            default:
                                                BuildString(result, "Error!");
                                                Debug.LogWarning(string.Format("Invalid criteria.effectcriteria.name \"{0}\"", criteria.effectCriteria.name));
                                                errorFlag = true;
                                                break;
                                        }
                                        break;
                                    //
                                    // - - - Topics - - - 
                                    //
                                    case "Topic":
                                        //uses topicType from ValidationManager.cs to run checks in TopicManager.cs
                                        bool isValid = false;
                                        int turn = GameManager.i.turnScript.Turn;
                                        switch (criteria.effectCriteria.name)
                                        {
                                            case "TopicActor":
                                                isValid = GameManager.i.topicScript.CheckTopicsAvailable(GameManager.i.validateScript.actorType, turn);
                                                break;
                                            case "TopicCampaign":
                                                isValid = GameManager.i.topicScript.CheckTopicsAvailable(GameManager.i.validateScript.campaignType, turn);
                                                break;
                                            case "TopicCity":
                                                isValid = GameManager.i.topicScript.CheckTopicsAvailable(GameManager.i.validateScript.cityType, turn);
                                                break;
                                            case "TopicFamily":
                                                isValid = GameManager.i.topicScript.CheckTopicsAvailable(GameManager.i.validateScript.familyType, turn);
                                                break;
                                            case "TopicHQ":
                                                isValid = GameManager.i.topicScript.CheckTopicsAvailable(GameManager.i.validateScript.hqType, turn);
                                                break;
                                            case "TopicCapture":
                                                isValid = GameManager.i.topicScript.CheckTopicsAvailable(GameManager.i.validateScript.captureType, turn);
                                                break;
                                            case "TopicRebel":
                                                isValid = GameManager.i.topicScript.CheckTopicsAvailable(GameManager.i.validateScript.resistanceType, turn);
                                                break;
                                            case "TopicAuthority":
                                                isValid = GameManager.i.topicScript.CheckTopicsAvailable(GameManager.i.validateScript.authorityType, turn);
                                                break;
                                            case "TopicPlayer":
                                                isValid = GameManager.i.topicScript.CheckTopicsAvailable(GameManager.i.validateScript.playerType, turn);
                                                break;
                                            case "TopicOrganisation":
                                                isValid = GameManager.i.topicScript.CheckTopicsAvailable(GameManager.i.validateScript.organisationType, turn);
                                                break;
                                            default:
                                                Debug.LogWarning(string.Format("Topic: Invalid effect.criteriaEffect \"{0}\"", criteria.effectCriteria.name));
                                                errorFlag = true;
                                                break;
                                        }
                                        if (isValid == false)
                                        { BuildString(result, "FAILED topic"); }
                                        break;
                                    //
                                    // - - - Statistics - - - 
                                    //
                                    case "Statistics":
                                        switch (criteria.effectCriteria.name)
                                        {
                                            //
                                            // - - - Base Stats
                                            //
                                            case "StatDaysLieLowMin":
                                                //auto success if testManager.cs -> testRatioPlayLieLow != 0
                                                if (GameManager.i.testScript.testRatioPlayLieLow == 0)
                                                {
                                                    if (GameManager.i.dataScript.StatisticGetLevel(StatType.LieLowDaysTotal) < statDaysLieLowMin)
                                                    { BuildString(result, "Insufficient days Lying Low"); }
                                                }
                                                break;
                                            case "StatGearItemsMin":
                                                //auto success if testManager.cs -> testRatioPlayGiveGear != 0
                                                if (GameManager.i.testScript.testRatioPlayGiveGear == 0)
                                                {
                                                    if (GameManager.i.dataScript.StatisticGetLevel(StatType.GearTotal) < statGearItemsMin)
                                                    { BuildString(result, "Insufficient Gear items acquired"); }
                                                }
                                                break;
                                            case "StatNodeActionsMin":
                                                //auto success if testManager.cs -> testRatioPlayNodeAct != 0
                                                if (GameManager.i.testScript.testRatioPlayNodeAct == 0)
                                                {
                                                    if (GameManager.i.dataScript.StatisticGetLevel(StatType.NodeActionsResistance) < statNodeActionsMin)
                                                    { BuildString(result, "Insufficient Node Actions"); }
                                                }
                                                break;
                                            case "StatTargetAttemptsMin":
                                                //auto success if testManager.cs -> testRatioTargetAttempts != 0
                                                if (GameManager.i.testScript.testRatioPlayTargetAtt == 0)
                                                {
                                                    if (GameManager.i.dataScript.StatisticGetLevel(StatType.TargetAttempts) < statTargetAttemptsMin)
                                                    { BuildString(result, "Insufficient Target Attempts"); }
                                                }
                                                break;
                                            case "StatOrgCuresNOTZero":
                                                if (GameManager.i.dataScript.StatisticGetLevel(StatType.OrgCures) == 0)
                                                { BuildString(result, "Org has provided no cures"); }
                                                break;
                                            case "StatOrgContractsNOTZero":
                                                if (GameManager.i.dataScript.StatisticGetLevel(StatType.OrgContractHits) == 0)
                                                { BuildString(result, "Org has provided no contract Hits"); }
                                                break;
                                            case "StatOrgInfoNOTZero":
                                                if (GameManager.i.dataScript.StatisticGetLevel(StatType.OrgInfoHacks) == 0)
                                                { BuildString(result, "Org has provided no Info Services"); }
                                                break;
                                            case "StatOrgHQNOTZero":
                                                if (GameManager.i.dataScript.StatisticGetLevel(StatType.OrgHQDropped) == 0)
                                                { BuildString(result, "Org has dropped no investigations"); }
                                                break;
                                            case "StatOrgEmergencyNOTZero":
                                                if (GameManager.i.dataScript.StatisticGetLevel(StatType.OrgEscapes) == 0)
                                                { BuildString(result, "Org hasn't helped you Escape"); }
                                                break;
                                            //
                                            // - - - Ratios
                                            //
                                            case "RatioPlayNodeActLow":
                                                if (GameManager.i.statScript.ratioPlayerNodeActions > ratioPlayNodeActLow)
                                                { BuildString(result, "To many Player Node Actions for Low"); }
                                                break;
                                            case "RatioPlayNodeActHigh":
                                                if (GameManager.i.statScript.ratioPlayerNodeActions < ratioPlayNodeActHigh)
                                                { BuildString(result, "Insufficient Player Node Actions for High"); }
                                                break;
                                            case "RatioPlayTargetAttLow":
                                                if (GameManager.i.statScript.ratioPlayerTargetAttempts > ratioPlayTargetAttLow)
                                                { BuildString(result, "To many Player Attempts for Low"); }
                                                break;
                                            case "RatioPlayTargetAttHigh":
                                                if (GameManager.i.statScript.ratioPlayerTargetAttempts < ratioPlayTargetAttHigh)
                                                { BuildString(result, "Insufficient Player Attempts for High"); }
                                                break;
                                            case "RatioPlayMoveActLow":
                                                if (GameManager.i.statScript.ratioPlayerMoveActions > ratioPlayMoveActLow)
                                                { BuildString(result, "To many Player Move Actions for Low"); }
                                                break;
                                            case "RatioPlayMoveActHigh":
                                                if (GameManager.i.statScript.ratioPlayerMoveActions < ratioPlayMoveActHigh)
                                                { BuildString(result, "Insufficient Player Move Actions for High"); }
                                                break;
                                            case "RatioPlayLieLowLow":
                                                if (GameManager.i.statScript.ratioPlayerLieLowDays > ratioPlayLieLowLow)
                                                { BuildString(result, "To many Player Days Lie Low for Low"); }
                                                break;
                                            case "RatioPlayLieLowHigh":
                                                if (GameManager.i.statScript.ratioPlayerLieLowDays < ratioPlayLieLowHigh)
                                                { BuildString(result, "Insufficient Player Days Lie Low for High"); }
                                                break;
                                            case "RatioPlayGiveGearLow":
                                                if (GameManager.i.statScript.ratioPlayerGiveGear > ratioGiveGearLow)
                                                { BuildString(result, "To many Give Gear actions for Low"); }
                                                break;
                                            case "RatioPlayGiveGearHigh":
                                                if (GameManager.i.statScript.ratioPlayerGiveGear < ratioGiveGearHigh)
                                                { BuildString(result, "Insufficient Give Gear actions for High"); }
                                                break;
                                            case "RatioPlayManageActLow":
                                                if (GameManager.i.statScript.ratioPlayerManageActions > ratioPlayManageActLow)
                                                { BuildString(result, "To many Player Manage actions for Low"); }
                                                break;
                                            case "RatioPlayManageActHigh":
                                                if (GameManager.i.statScript.ratioPlayerManageActions < ratioPlayManageActHigh)
                                                { BuildString(result, "Insufficient Player Manage actions for High"); }
                                                break;
                                            case "RatioPlayDoNothingLow":
                                                if (GameManager.i.statScript.ratioPlayerDoNothing > ratioPlayDoNothingLow)
                                                { BuildString(result, "To many Player Do Nothing actions for Low"); }
                                                break;
                                            case "RatioPlayDoNothingHigh":
                                                if (GameManager.i.statScript.ratioPlayerDoNothing < ratioPlayDoNothingHigh)
                                                { BuildString(result, "Insufficient Player Do Nothing actions for High"); }
                                                break;
                                            case "RatioPlayAddictLow":
                                                if (GameManager.i.statScript.ratioPlayerAddictedDays > ratioPlayAddictLow)
                                                { BuildString(result, "To many Player Addicted Days for Low"); }
                                                break;
                                            case "RatioPlayAddictHigh":
                                                if (GameManager.i.statScript.ratioPlayerAddictedDays < ratioPlayAddictHigh)
                                                { BuildString(result, "Insufficient Player Addicted Days for High"); }
                                                break;
                                            default:
                                                BuildString(result, "Error!");
                                                Debug.LogWarning(string.Format("Invalid criteria.effectcriteria.name \"{0}\"", criteria.effectCriteria.name));
                                                errorFlag = true;
                                                break;
                                        }
                                        break;
                                    //
                                    // - - - Organisation - - -
                                    //
                                    case "Organisation":
                                        switch (criteria.effectCriteria.name)
                                        {
                                            //Organisation Known to Player?
                                            case "OrganisationKnown":
                                                if (org != null)
                                                {
                                                    if (org.isContact == false)
                                                    { BuildString(result, "No contact with Organisation"); }
                                                    if (org.isCutOff == true)
                                                    { BuildString(result, "Org refuses to deal with you"); }
                                                }
                                                else { BuildString(result, "Invalid Organisation"); }
                                                /*else { BuildString(result, string.Format("Invalid Organisation - orgKnown, orgName \"{0}\"", data.orgName)); }*/

                                                break;
                                            //Freedom Not MIN
                                            case "OrganisationFreeNOTMin":
                                                if (org != null)
                                                {
                                                    if (org.GetFreedom() == 0)
                                                    { BuildString(result, "Freedom is Zero"); }
                                                }
                                                else { BuildString(result, "Invalid Organisation"); }
                                                break;
                                            //Freedom Not MAX
                                            case "OrganisationFreeNOTMax":
                                                if (org != null)
                                                {
                                                    if (org.GetFreedom() == maxStatValue)
                                                    { BuildString(result, "Freedom is MAX"); }
                                                }
                                                else { BuildString(result, "Invalid Organisation"); }
                                                /*else { BuildString(result, "Invalid Organisation - OrgFreeNotMax"); }*/
                                                break;

                                            //Reputation MIN
                                            case "OrganisationRepMin":
                                                if (org != null)
                                                {
                                                    if (org.GetReputation() > 0)
                                                    { BuildString(result, "Reputation Not MIN"); }
                                                }
                                                else { BuildString(result, "Invalid Organisation - OrgRepMin"); }
                                                break;
                                            //OrgInfo not currently active (may or may not be known)
                                            case "OrgInfoNOTActive":
                                                if (GameManager.i.dataScript.CheckOrgInfoActive() == true)
                                                { BuildString(result, "Org providing Direct Feed"); }
                                                break;
                                            //Player knows Organisation secret
                                            case "SecretOrgCureYes":
                                            case "SecretOrgContYes":
                                            case "SecretOrgHQYes":
                                            case "SecretOrgEmerYes":
                                            case "SecretOrgInfoYes":
                                                if (org != null)
                                                {
                                                    if (org.isSecretKnown == false)
                                                    { BuildString(result, string.Format("Secret NOT known")); }
                                                }
                                                else { BuildString(result, "Invalid Organisation"); }
                                                /*else { BuildString(result, "Invalid Organisation - SecretOrgYes"); }*/
                                                break;
                                            //Player DOESN'T knows Organisation secret
                                            case "SecretOrgCureNo":
                                            case "SecretOrgContNo":
                                            case "SecretOrgHQNo":
                                            case "SecretOrgEmerNo":
                                            case "SecretOrgInfoNo":
                                                if (org != null)
                                                {
                                                    if (org.isSecretKnown == true)
                                                    { BuildString(result, string.Format("Secret IS known")); }
                                                }
                                                else { BuildString(result, "Invalid Organisation"); }
                                                /*else { BuildString(result, "Invalid Organisation - SecretOrgNo"); }*/
                                                break;
                                            //Player knows Org secret and secret is status Active (hasn't yet been Revealed)
                                            case "SecretOrgCureActive":
                                            case "SecretOrgContActive":
                                            case "SecretOrgHQActive":
                                            case "SecretOrgEmerActive":
                                            case "SecretOrgInfoActive":
                                                if (org != null)
                                                {
                                                    if (org.secret.status != gameAPI.SecretStatus.Active)
                                                    { BuildString(result, string.Format("Secret Revealed")); }
                                                }
                                                else { BuildString(result, "Invalid Organisation"); }
                                                /*else { BuildString(result, "Invalid Organisation - SecretOrgActive"); }*/
                                                break;
                                            default:
                                                BuildString(result, "Error!");
                                                Debug.LogWarning(string.Format("Invalid criteria.effectcriteria.name \"{0}\"", criteria.effectCriteria.name));
                                                errorFlag = true;
                                                break;
                                        }
                                        break;
                                    //
                                    // - - - HQ - - -
                                    //
                                    case "HQ":
                                        switch (criteria.effectCriteria.name)
                                        {
                                            case "HqRelocatingNo":
                                                //HQ not currently relocating
                                                if (GameManager.i.hqScript.isHqRelocating == true)
                                                { BuildString(result, "HQ is Relocating"); }
                                                break;
                                            case "HqApprovalNOTZero":
                                                //Player HQ Approval 1+
                                                if (GameManager.i.hqScript.GetHqApproval() == 0)
                                                { BuildString(result, "HQ Approval Zero"); }
                                                break;
                                            case "MotNeutMinHqBoss":
                                                //Boss of HQ has motivation 2+
                                                Actor actorBoss = GameManager.i.dataScript.GetHqHierarchyActor(ActorHQ.Boss);
                                                if (actorBoss != null)
                                                {
                                                    if (actorBoss.GetDatapoint(ActorDatapoint.Motivation1) < 2)
                                                    { BuildString(result, string.Format("{0} Motivation < 2", GameManager.i.hqScript.GetHqTitle(ActorHQ.Boss))); }
                                                }
                                                else { Debug.LogWarning("Invalid HQ Boss (Null)"); }
                                                break;
                                            case "MotNeutMinHqSubBoss1":
                                                //subBoss 1 of HQ has motivation 2+
                                                Actor actorSubBoss1 = GameManager.i.dataScript.GetHqHierarchyActor(ActorHQ.SubBoss1);
                                                if (actorSubBoss1 != null)
                                                {
                                                    if (actorSubBoss1.GetDatapoint(ActorDatapoint.Motivation1) < 2)
                                                    { BuildString(result, string.Format("{0} Motivation < 2", GameManager.i.hqScript.GetHqTitle(ActorHQ.SubBoss1))); }
                                                }
                                                else { Debug.LogWarning("Invalid HQ SubBoss1 (Null)"); }
                                                break;
                                            case "MotNeutMinHqSubBoss2":
                                                //subBoss 2 of HQ has motivation 2+
                                                Actor actorSubBoss2 = GameManager.i.dataScript.GetHqHierarchyActor(ActorHQ.SubBoss2);
                                                if (actorSubBoss2 != null)
                                                {
                                                    if (actorSubBoss2.GetDatapoint(ActorDatapoint.Motivation1) < 2)
                                                    { BuildString(result, string.Format("{0} Motivation < 2", GameManager.i.hqScript.GetHqTitle(ActorHQ.SubBoss2))); }
                                                }
                                                else { Debug.LogWarning("Invalid HQ SubBoss2 (Null)"); }
                                                break;
                                            case "MotNeutMinHqSubBoss3":
                                                //subBoss 3 of HQ has motivation 2+
                                                Actor actorSubBoss3 = GameManager.i.dataScript.GetHqHierarchyActor(ActorHQ.SubBoss3);
                                                if (actorSubBoss3 != null)
                                                {
                                                    if (actorSubBoss3.GetDatapoint(ActorDatapoint.Motivation1) < 2)
                                                    { BuildString(result, string.Format("{0} Motivation < 2", GameManager.i.hqScript.GetHqTitle(ActorHQ.SubBoss3))); }
                                                }
                                                else { Debug.LogWarning("Invalid HQ SubBoss3 (Null)"); }
                                                break;
                                            default:
                                                BuildString(result, "Error!");
                                                Debug.LogWarningFormat("Invalid criteria.effectcriteria.name \"{0}\"", criteria.effectCriteria.name);
                                                errorFlag = true;
                                                break;
                                        }
                                        break;
                                    //
                                    // - - - Special - - -
                                    //
                                    case "Special":
                                        switch (criteria.effectCriteria.name)
                                        {
                                            case "CureAddictedNo":
                                                //No active cure is present OnMap for Addicted Condition
                                                if (GameManager.i.dataScript.CheckCurePresent(conditionAddicted.cure) == true)
                                                { BuildString(result, "Addicted Cure unavailable"); }
                                                break;
                                            case "CureTaggedNo":
                                                //No active cure is present OnMap for Tagged Condition
                                                if (GameManager.i.dataScript.CheckCurePresent(conditionTagged.cure) == true)
                                                { BuildString(result, "Tagged Cure unavailable"); }
                                                break;
                                            case "CureWoundedNo":
                                                //No active cure is present OnMap for Wounded Condition
                                                if (GameManager.i.dataScript.CheckCurePresent(conditionWounded.cure) == true)
                                                { BuildString(result, "Wounded Cure unavailable"); }
                                                break;
                                            case "CureImagedNo":
                                                //No active cure is present OnMap for Imaged Condition
                                                if (GameManager.i.dataScript.CheckCurePresent(conditionImaged.cure) == true)
                                                { BuildString(result, "Imaged Cure unavailable"); }
                                                break;
                                            case "CureDoomedNo":
                                                //No active cure is present OnMap for Doomed Condition
                                                if (GameManager.i.dataScript.CheckCurePresent(conditionDoomed.cure) == true)
                                                { BuildString(result, "Doomed Cure unavailable"); }
                                                break;
                                            case "CureQuestionableNo":
                                                //No active cure is present OnMap for Questionable Condition
                                                if (GameManager.i.dataScript.CheckCurePresent(conditionQuestionable.cure) == true)
                                                { BuildString(result, "Questionable Cure unavailable"); }
                                                break;
                                            case "NemesisActive":
                                                //Nemesis is present and active
                                                if (GameManager.i.nemesisScript.nemesis == null)
                                                { BuildString(result, "Nemesis not present"); }
                                                else if (GameManager.i.nemesisScript.GetMode() == NemesisMode.Inactive)
                                                { BuildString(result, "Nemesis inactive"); }
                                                break;
                                            case "NpcActive":
                                                //Npc is present and active
                                                if (GameManager.i.missionScript.mission.npc == null)
                                                { BuildString(result, "Special Character not present"); }
                                                else if (GameManager.i.missionScript.mission.npc.status != NpcStatus.Active)
                                                { BuildString(result, "Special Character not Active"); }
                                                break;
                                            default:
                                                BuildString(result, "Error!");
                                                Debug.LogWarning(string.Format("Invalid criteria.effectcriteria.name \"{0}\"", criteria.effectCriteria.name));
                                                errorFlag = true;
                                                break;
                                        }
                                        break;
                                    //
                                    // - - - global default
                                    //
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
                            Debug.LogError(string.Format("Invalid criteria.effectCriteria (Null) for criteria {0}", criteria.name));
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
    #endregion

    #region BuildString
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
    #endregion

    #region ComparisonCheck
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
    #endregion

    #region ProcessEffect
    /// <summary>
    /// Processes effects and returns results in a class. Leave actor as Null for (Resistance?) Player effect (Invisibility, Renown etc. auto checks node for Player being present, so can provide Actor)
    /// Use player node if an issue but check to see if node is used (often to determine who is affected, player or actor)
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
        EffectDataResolve effectResolve = new EffectDataResolve();
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
            //Topic effects
            if (effect.apply.name.Equals("Topic", StringComparison.Ordinal) == true)
            {
                effectResolve = ResolveTopicData(effect, dataInput);
                effectReturn = ConvertEffectResolveToReturn(effectResolve, effectReturn);

                /*if (effectResolve.isError == true)
                { effectReturn.errorFlag = true; }
                else
                {
                    effectReturn.topText = effectResolve.topText;
                    effectReturn.bottomText = effectResolve.bottomText;
                    effectReturn.isAction = false;
                }*/
            }
            else if (effect.apply.name.Equals("MetaGame", StringComparison.Ordinal) == true)
            {
                //MetaGame effects
                effectResolve = ResolveMetaGame(effect, dataInput);
                effectReturn = ConvertEffectResolveToReturn(effectResolve, effectReturn);
            }
            //Non-Topic effect
            else
            {
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
                            effectResolve = ResolveNodeData(effect, node, dataInput);
                            effectReturn = ConvertEffectResolveToReturn(effectResolve, effectReturn);
                            effectReturn.isAction = true;
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
                    case "ConditionBad":
                    case "ConditionGroupGood":
                    case "ConditionAddicted":
                    case "ConditionDoomed":
                    case "ConditionImaged":
                    case "ConditionWounded":
                    case "ConditionStressed":
                    case "ConditionIncompetent":
                    case "ConditionCorrupt":
                    case "ConditionQuestionable":
                    case "ConditionBlackmailer":
                    case "ConditionStar":
                        if (node != null)
                        {
                            //it's OK to pass a null actor provided it's a player condition
                            effectResolve = ResolveConditionData(effect, node, dataInput, actor);
                            effectReturn = ConvertEffectResolveToReturn(effectResolve, effectReturn);
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
                    case "Innocence":
                    case "PlayerActions":
                        effectResolve = ResolvePlayerData(effect, dataInput);
                        effectReturn = ConvertEffectResolveToReturn(effectResolve, effectReturn);
                        break;
                    //
                    // - - - Capture Tools - - -
                    case "CaptureTool":
                        effectReturn.bottomText = ExecuteCaptureTool(effect, dataInput);
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
                            effectResolve = ResolveManageData(effect, actor);
                            effectReturn = ConvertEffectResolveToReturn(effectResolve, effectReturn);
                        }
                        break;
                    //
                    // - - - Player Mood effects - - -
                    //
                    case "AgreeablenessBad":
                    case "AgreeablenessGood":
                    case "ConscientiousnessBad":
                    case "ConscientiousnessGood":
                    case "ExtroversionBad":
                    case "ExtroversionGood":
                    case "NeurotiscismBad":
                    case "NeurotiscismGood":
                    case "OpennessBad":
                    case "OpennessGood":
                        effectResolve = ResolveMoodData(effect, dataInput);
                        effectReturn = ConvertEffectResolveToReturn(effectResolve, effectReturn);
                        break;
                    //
                    // - - - Special Actor Effects - - -
                    //
                    case "ActorResigns":
                    case "ActorKills":
                        if (actor != null)
                        {
                            effectResolve = ResolveSpecialActorEffect(effect, actor);
                            effectReturn = ConvertEffectResolveToReturn(effectResolve, effectReturn);
                        }
                        break;
                    //
                    // - - - Investigations
                    //
                    case "InvestigationDropped":
                        //Remove an investigation into the Player
                        effectReturn.bottomText = GameManager.i.playerScript.DropInvestigation(dataInput.dataName);
                        break;
                    //
                    // - - - Secrets - - -
                    //
                    case "SecretRandom":
                        //Remove a random secret from Player (removes all instances) or Actor (removes local instance only)
                        if (actor == null)
                        {
                            //Player
                            Secret secret = GameManager.i.playerScript.GetRandomCurrentSecret();
                            if (secret != null)
                            {
                                secret.status = gameAPI.SecretStatus.Deleted;
                                secret.deletedWhen = GameManager.SetTimeStamp();
                                //remove secret from game
                                if (GameManager.i.secretScript.RemoveSecretFromAll(secret.name, true) == true)
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
                                actor.RemoveSecret(secret.name);
                                effectReturn.bottomText = string.Format("{0}\"{1}\" secret deleted from {2}{3}", colourGood, secret.tag, actor.arc.name, colourEnd);
                                effectReturn.isAction = true;
                            }
                        }
                        break;
                    case "Secret":
                        //Remove a specific secret from Player (removes all instances)
                        Secret secretPlayer = GameManager.i.playerScript.GetSecret(dataInput.dataName);
                        if (secretPlayer != null)
                        {
                            secretPlayer.status = gameAPI.SecretStatus.Deleted;
                            secretPlayer.deletedWhen = GameManager.SetTimeStamp();
                            //remove secret from game
                            if (GameManager.i.secretScript.RemoveSecretFromAll(secretPlayer.name, true) == true)
                            { effectReturn.bottomText = string.Format("{0}\"{1}\" secret deleted{2}", colourGood, secretPlayer.tag, colourEnd); }
                            else { effectReturn.bottomText = string.Format("{0}\"{1}\" secret NOT deleted{2}", colourBad, secretPlayer.tag, colourEnd); }
                            effectReturn.isAction = true;
                        }
                        break;
                    //
                    // - - - Org Secrets (break off contact once revealed)
                    //
                    case "OrgContact":
                        if (string.IsNullOrEmpty(dataInput.dataName) == false)
                        {
                            Organisation org = GameManager.i.dataScript.GetOrganisaton(dataInput.dataName);
                            if (org != null)
                            {
                                switch (effect.operand.name)
                                {
                                    case "Add":
                                        //Contact established
                                        org.isContact = true;
                                        effectReturn.bottomText = string.Format("{0}{1} make contact{2}", colourGood, org.tag, colourEnd);
                                        effectReturn.listOfHelpTags.Add("org_0");
                                        effectReturn.listOfHelpTags.Add("org_1");
                                        /*Debug.LogFormat("[Org] EffectManager.cs -> ProcessEffect: Organisation \"{0}\" no longer in contact with Player (secret revealed){1}", org.tag, "\n");*/
                                        Debug.LogFormat("[Org] EffectManager.cs -> ProcessEffect: Organisation \"{0}\" now in CONTACT with Player{1}", org.tag, "\n");
                                        break;
                                    case "Subtract":
                                        //break off contact
                                        org.isContact = false;
                                        effectReturn.bottomText = string.Format("{0}{1} break off contact{2}", colourBad, org.tag, colourEnd);
                                        effectReturn.listOfHelpTags.Add("org_2");
                                        /*Debug.LogFormat("[Org] EffectManager.cs -> ProcessEffect: Organisation \"{0}\" in contact with Player{1}", org.tag, "\n");*/
                                        Debug.LogFormat("[Org] EffectManager.cs -> ProcessEffect: Organisation \"{0}\" no longer in contact with Player{1}", org.tag, "\n");
                                        break;
                                    default:
                                        Debug.LogWarningFormat("Unrecognised effect.operand \"{0}\"", effect.operand.name); break;
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid org (Null) for dataInput.dataName \"{0}\"", dataInput.dataName); }
                        }
                        else { Debug.LogWarning("Invalid dataInput.dataName (Null or Empty) for 'OrgContact' effectOutcome"); }
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
                            effectResolve = ResolveConnectionData(effect, node, dataInput);
                            effectReturn = ConvertEffectResolveToReturn(effectResolve, effectReturn);
                        }
                        else
                        {
                            Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                            effectReturn.errorFlag = true;
                        }
                        break;
                    case "Motivation":
                        switch (effect.apply.name)
                        {
                            case "ActorCurrent":
                                //current actor has their motivation changed
                                effectReturn.bottomText = ExecuteActorMotivation(effect, actor, dataInput);
                                break;
                            case "ActorAll":
                                //all actors have their motivation changed
                                if (ResolveGroupActorEffect(effect, dataInput, actor) == true)
                                { effectReturn.bottomText = string.Format("{0}{1}{2}", colourEffect, effect.description, colourEnd); }
                                else { effectReturn.errorFlag = true; }
                                break;
                            default:
                                Debug.LogWarningFormat("Invalid effect.apply \"{0}\"", effect.apply.name);
                                break;
                        }
                        break;
                    case "CityLoyalty":
                        //depends on player side
                        int cityLoyalty = GameManager.i.cityScript.CityLoyalty;
                        switch (dataInput.side.level)
                        {
                            case 1:
                                //Authority
                                switch (effect.typeOfEffect.level)
                                {
                                    case 2:
                                        //Good 
                                        cityLoyalty += effect.value;
                                        cityLoyalty = Mathf.Min(GameManager.i.cityScript.maxCityLoyalty, cityLoyalty);
                                        GameManager.i.cityScript.CityLoyalty = cityLoyalty;
                                        effectReturn.topText = string.Format("{0}The City grows closer to the Authority{1}", colourText, colourEnd);
                                        effectReturn.bottomText = string.Format("{0}City Loyalty +{1}{2}", colourGood, effect.value, colourEnd);
                                        GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopCentre, $"City Loyalty +{effect.value}");
                                        break;
                                    case 0:
                                        //Bad 
                                        cityLoyalty -= effect.value;
                                        cityLoyalty = Mathf.Max(0, cityLoyalty);
                                        GameManager.i.cityScript.CityLoyalty = cityLoyalty;
                                        effectReturn.topText = string.Format("{0}The City grows closer to the Resistance{1}", colourText, colourEnd);
                                        effectReturn.bottomText = string.Format("{0}City Loyalty -{1}{2}", colourBad, effect.value, colourEnd);
                                        GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopCentre, $"City Loyalty -{effect.value}");
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
                                        GameManager.i.cityScript.CityLoyalty = cityLoyalty;
                                        effectReturn.topText = string.Format("{0}The City grows closer to the Resistance{1}", colourText, colourEnd);
                                        effectReturn.bottomText = string.Format("{0}City Loyalty -{1}{2}", colourGood, effect.value, colourEnd);
                                        GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopCentre, $"City Loyalty -{effect.value}");
                                        break;
                                    case 0:
                                        //Bad
                                        cityLoyalty += effect.value;
                                        cityLoyalty = Mathf.Min(GameManager.i.cityScript.maxCityLoyalty, cityLoyalty);
                                        GameManager.i.cityScript.CityLoyalty = cityLoyalty;
                                        effectReturn.topText = string.Format("{0}The City grows closer to the Authority{1}", colourText, colourEnd);
                                        effectReturn.bottomText = string.Format("{0}City Loyalty +{1}{2}", colourBad, effect.value, colourEnd);
                                        GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopCentre, $"City Loyalty +{effect.value}");
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
                    case "HQApproval":
                        switch (effect.operand.name)
                        {
                            case "Add":
                                GameManager.i.hqScript.ChangeHqApproval(effect.value, dataInput.side, dataInput.originText);
                                effectReturn.topText = string.Format("{0}HQ have a better opinion of you{1}", colourText, colourEnd);
                                effectReturn.bottomText = string.Format("{0}HQ Approval +{1}{2}", colourGood, effect.value, colourEnd);
                                GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopCentre, $"HQ Approval +{effect.value}");
                                break;
                            case "Subtract":
                                GameManager.i.hqScript.ChangeHqApproval(effect.value * -1, dataInput.side, dataInput.originText);
                                effectReturn.topText = string.Format("{0}HQ's opinion of you has diminished{1}", colourText, colourEnd);
                                effectReturn.bottomText = string.Format("{0}HQ Approval -{1}{2}", colourBad, effect.value, colourEnd);
                                GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopCentre, $"HQ Approval -{effect.value}");
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
                            effectReturn.isAction = true;
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
                        //no effect, handled elsewhere
                        break;
                    case "NeutraliseTeam":
                        //no effect, handled directly elsewhere (check ActorManager.cs -> GetNodeActions
                        break;
                    case "Invisibility":
                        //raise/lower invisibility of actor or Player
                        if (node != null)
                        {
                            //player
                            if (node.nodeID == GameManager.i.nodeScript.GetPlayerNodeID())
                            { effectReturn.bottomText = ExecutePlayerInvisibility(effect, dataInput, node); }
                            else
                            {
                                //actor
                                if (actor != null)
                                { effectReturn.bottomText = ExecuteActorInvisibility(effect, dataInput, actor, node); }
                                else
                                {
                                    Debug.LogError(string.Format("Invalid Actor (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                                    effectReturn.errorFlag = true;
                                }
                            }
                        }
                        else
                        {
                            Debug.LogErrorFormat("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name);
                            effectReturn.errorFlag = true;
                        }
                        break;
                    //Note: Renown can be in increments of > 1
                    case "Renown":
                        if (node != null)
                        {
                            //Player
                            if (node.nodeID == GameManager.i.nodeScript.GetPlayerNodeID())
                            { effectReturn.bottomText = ExecutePlayerRenown(effect); }
                            else
                            {
                                //Actor effect
                                if (actor != null)
                                { effectReturn.bottomText = ExecuteActorRenown(effect, actor, dataInput); }
                                else
                                {
                                    Debug.LogErrorFormat("Invalid Actor (null) for EffectOutcome \"{0}\"", effect.outcome.name);
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
                        teamArcID = GameManager.i.dataScript.GetTeamArcID("CIVIL");
                        teamID = GameManager.i.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                        //insert team
                        GameManager.i.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                        //return texts
                        effectReturn.topText = SetTopTeamText(teamID);
                        effectReturn.bottomText = SetBottomTeamText(actor);
                        GameManager.i.popUpFixedScript.SetData(actor.slotID, "Insert Team");
                        //action
                        effectReturn.isAction = true;
                        break;
                    case "ControlTeam":
                        teamArcID = GameManager.i.dataScript.GetTeamArcID("CONTROL");
                        teamID = GameManager.i.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                        GameManager.i.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                        //return texts
                        effectReturn.topText = SetTopTeamText(teamID);
                        effectReturn.bottomText = SetBottomTeamText(actor);
                        GameManager.i.popUpFixedScript.SetData(actor.slotID, "Insert Team");
                        effectReturn.isAction = true;
                        break;
                    case "DamageTeam":
                        teamArcID = GameManager.i.dataScript.GetTeamArcID("DAMAGE");
                        teamID = GameManager.i.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                        GameManager.i.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                        //return texts
                        effectReturn.topText = SetTopTeamText(teamID);
                        effectReturn.bottomText = SetBottomTeamText(actor);
                        effectReturn.isAction = true;
                        GameManager.i.popUpFixedScript.SetData(actor.slotID, "Insert Team");
                        break;
                    case "ErasureTeam":
                        teamArcID = GameManager.i.dataScript.GetTeamArcID("ERASURE");
                        teamID = GameManager.i.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                        GameManager.i.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                        //return texts
                        effectReturn.topText = SetTopTeamText(teamID);
                        effectReturn.bottomText = SetBottomTeamText(actor);
                        effectReturn.isAction = true;
                        GameManager.i.popUpFixedScript.SetData(actor.slotID, "Insert Team");
                        break;
                    case "MediaTeam":
                        teamArcID = GameManager.i.dataScript.GetTeamArcID("MEDIA");
                        teamID = GameManager.i.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                        GameManager.i.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                        //return texts
                        effectReturn.topText = SetTopTeamText(teamID);
                        effectReturn.bottomText = SetBottomTeamText(actor);
                        effectReturn.isAction = true;
                        GameManager.i.popUpFixedScript.SetData(actor.slotID, "Insert Team");
                        break;
                    case "ProbeTeam":
                        teamArcID = GameManager.i.dataScript.GetTeamArcID("PROBE");
                        teamID = GameManager.i.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                        GameManager.i.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                        //return texts
                        effectReturn.topText = SetTopTeamText(teamID);
                        effectReturn.bottomText = SetBottomTeamText(actor);
                        effectReturn.isAction = true;
                        GameManager.i.popUpFixedScript.SetData(actor.slotID, "Insert Team");
                        break;
                    case "SpiderTeam":
                        teamArcID = GameManager.i.dataScript.GetTeamArcID("SPIDER");
                        teamID = GameManager.i.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                        GameManager.i.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.slotID, node);
                        //return texts
                        effectReturn.topText = SetTopTeamText(teamID);
                        effectReturn.bottomText = SetBottomTeamText(actor);
                        effectReturn.isAction = true;
                        GameManager.i.popUpFixedScript.SetData(actor.slotID, "Insert Team");
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                        break;
                }
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
    #endregion

    #region ConvertEffectResolveToReturn
    /// <summary>
    /// subMethod for ProcessEffect to convert EffectDataResolve into EffectDataReturn
    /// </summary>
    /// <param name="dataResolve"></param>
    /// <param name="dataReturn"></param>
    private EffectDataReturn ConvertEffectResolveToReturn(EffectDataResolve effectResolve, EffectDataReturn effectReturn)
    {
        if (effectResolve != null)
        {
            if (effectReturn != null)
            {
                if (effectResolve.isError == true)
                { effectReturn.errorFlag = true; }
                else
                {
                    effectReturn.topText = effectResolve.topText;
                    effectReturn.bottomText = effectResolve.bottomText;
                    effectReturn.isAction = false;
                }
            }
            else { Debug.LogError("Invalid EffectDataReturn (Null)"); }
        }
        else { Debug.LogError("Invalid EffectDataResolve (Null)"); }
        return effectReturn;
    }
    #endregion

    #region SetTopTeamText
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
        Team team = GameManager.i.dataScript.GetTeam(teamID);
        if (team != null)
        {
            //get node
            Node node = GameManager.i.dataScript.GetNode(team.nodeID);
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
    #endregion

    #region SetBottomTeamText
    /// <summary>
    /// Formats string for details.BottomText (Authority), returns "unknown" if parameter is null. Also used by ModalTeamPicker.cs -> ProcessTeamChoice
    /// </summary>
    /// <returns></returns>
    public string SetBottomTeamText(Actor actor)
    {
        if (actor != null)
        {
            string colourNumbers = colourGoodSide;
            if (actor.CheckNumOfTeams() == actor.GetDatapoint(ActorDatapoint.Ability2))
            { colourNumbers = colourBadSide; }
            return string.Format("{0}, {1} of {2}{3}{4} has now deployed {5}{6}{7} of {8}{9}{10} teams",
                actor.actorName, GameManager.i.metaScript.GetAuthorityTitle(), colourActor, actor.arc.name, colourEnd,
                colourNumbers, actor.CheckNumOfTeams(), colourEnd, colourNumbers, actor.GetDatapoint(ActorDatapoint.Ability2), colourEnd);
        }
        else
        {
            Debug.LogError("Invalid actor (Null)");
            return "Unknown";
        }
    }
    #endregion


    /// <summary>
    /// Sub method to process group actor effects, eg. All actors Motivation +1. If actor != null then this actor is excluded from the effect. Returns true if successful, false otherwise
    /// NOTE: Effect, dataInput and Actor are checked for null by the calling method
    /// </summary>
    /// <param name="datapoint"></param>
    /// <param name="value"></param>
    /// <param name="actor"></param>
    private bool ResolveGroupActorEffect(Effect effect, EffectDataInput dataInput, Actor actorExclude = null)
    {
        bool isSuccess = true;
        int change;
        GlobalSide side = GameManager.i.sideScript.PlayerSide;
        Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActors(side);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                if (GameManager.i.dataScript.CheckActorSlotStatus(i, side) == true)
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
                                    int motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                                    int dataBefore = actor.GetDatapoint(ActorDatapoint.Motivation1);
                                    switch (effect.operand.name)
                                    {
                                        case "Add":
                                            motivation += effect.value;
                                            motivation = Mathf.Min(GameManager.i.actorScript.maxStatValue, motivation);
                                            actor.SetDatapoint(ActorDatapoint.Motivation1, motivation, dataInput.originText);
                                            break;
                                        case "Subtract":
                                            motivation -= effect.value;
                                            motivation = Mathf.Max(GameManager.i.actorScript.minStatValue, motivation);
                                            actor.SetDatapoint(ActorDatapoint.Motivation1, motivation, dataInput.originText);
                                            break;
                                        default:
                                            Debug.LogWarningFormat("Invalid effect.operand \"{0}\"", effect.operand.name);
                                            break;
                                    }
                                    //log
                                    if (motivation != dataBefore)
                                    {
                                        Debug.LogFormat("[Sta] -> EffectManger.cs: {0} {1} Motivation changed from {2} to {3}{4}", actor.actorName, actor.arc.name,
                                            dataBefore, motivation, "\n");
                                        //onMap actor -> popUp
                                        change = dataBefore - motivation;
                                        GameManager.i.popUpFixedScript.SetData(actor.slotID, string.Format("Motivation {0}{1}", change > 0 ? "+" : "", change));
                                    }
                                    break;
                                case "Renown":
                                    int renown = actor.Renown;
                                    switch (effect.operand.name)
                                    {
                                        case "Add":
                                            renown += effect.value;
                                            actor.Renown = renown;
                                            GameManager.i.popUpFixedScript.SetData(actor.slotID, $"Renown +{effect.value}");
                                            break;
                                        case "Subtract":
                                            renown -= effect.value;
                                            renown = Mathf.Max(0, renown);
                                            actor.Renown = renown;
                                            GameManager.i.popUpFixedScript.SetData(actor.slotID, $"Renown -{effect.value}");
                                            break;
                                        default:
                                            Debug.LogWarningFormat("Invalid effect.operand \"{0}\"", effect.operand.name);
                                            break;
                                    }
                                    break;
                                case "Invisibility":
                                    int invisibility = actor.GetDatapoint(ActorDatapoint.Invisibility2);
                                    switch (effect.operand.name)
                                    {
                                        case "Add":
                                            invisibility += effect.value;
                                            actor.SetDatapoint(ActorDatapoint.Invisibility2, invisibility, dataInput.originText);
                                            GameManager.i.popUpFixedScript.SetData(actor.slotID, $"Invisibility +{effect.value}");
                                            break;
                                        case "Subtract":
                                            invisibility -= effect.value;
                                            invisibility = Mathf.Max(0, invisibility);
                                            actor.SetDatapoint(ActorDatapoint.Invisibility2, invisibility, dataInput.originText);
                                            GameManager.i.popUpFixedScript.SetData(actor.slotID, $"Invisibiilty -{effect.value}");
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
            //motivation shift, update topBarUI item
            if (effect.outcome.name.Equals("Motivation") == true)
            { GameManager.i.topBarScript.UpdateConflicts(GameManager.i.actorScript.CheckNumOfConflictActors()); }
        }
        else { Debug.LogWarning("Invalid arrayOfActors (Null)"); isSuccess = false; }
        return isSuccess;
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
                if (effect.duration.name.Equals("Ongoing", StringComparison.Ordinal))
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
                if (effect.duration.name.Equals("Ongoing", StringComparison.Ordinal))
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
                if (effect.duration.name.Equals("Ongoing", StringComparison.Ordinal))
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, node, value); }
                //Process Node effect for current node
                node.ProcessNodeEffect(effectProcess);
                //Process Node effect for all nodes
                List<Node> listOfAllNodes = GameManager.i.dataScript.GetListOfAllNodes();
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
                if (effect.duration.name.Equals("Ongoing", StringComparison.Ordinal))
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, node, value); }
                //Process Node effect for current node
                node.ProcessNodeEffect(effectProcess);
                //Process Node effect for all same ARC nodes
                List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
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
    /// Sub method to process Connection Security
    /// Note: effect and node checked for Null by calling method
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="node"></param>
    /// <param name="effectInput"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveConnectionData(Effect effect, Node node, EffectDataInput effectInput)
    {

        /*Debug.LogFormat("[Tst] EffectManager.cs -> ResolveConnectionData: \"{0}\", nodeID {1}, duration {2}{3}", effect.apply.name, node.nodeID, effect.duration.name, "\n");*/

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
                if (effect.duration.name.Equals("Ongoing", StringComparison.Ordinal))
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
                    GameManager.i.connScript.SetAllFlagsToFalse();
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
                if (effect.duration.name.Equals("Ongoing", StringComparison.Ordinal))
                {
                    ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, node, value);
                    /*//DEBUG -> remove when finished with testing
                    effectProcess.effectOngoing.ongoingID = GetOngoingEffectID();
                    GameManager.instance.dataScript.AddOngoingIDToDict(effectProcess.effectOngoing.ongoingID, "Connection Security");*/
                }
                //Process Connection effect
                List<Node> listOfAllNodes = GameManager.i.dataScript.GetListOfAllNodes();
                if (listOfAllNodes != null)
                {
                    //clear all connection flags first to prevent double dipping
                    GameManager.i.connScript.SetAllFlagsToFalse();
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
                if (effect.duration.name.Equals("Ongoing", StringComparison.Ordinal))
                {
                    ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, node, value);
                }
                //Process Connection effect
                List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
                if (listOfNodes != null)
                {
                    //clear all connection flags first to prevent double dipping
                    GameManager.i.connScript.SetAllFlagsToFalse();
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
        colourEffect = GetColourEffect(effect.typeOfEffect);
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //choose big picture (Random / Any) or specific condition
        switch (effect.outcome.name)
        {
            //specific Conditions
            case "ConditionAddicted":
            case "ConditionDoomed":
            case "ConditionImaged":
            case "ConditionWounded":
            case "ConditionStressed":
            case "ConditionIncompetent":
            case "ConditionCorrupt":
            case "ConditionQuestionable":
            case "ConditionBlackmailer":
            case "ConditionStar":
                //get condition
                condition = GetCondition(effect.outcome);
                //resolve effect outcome
                if (condition != null)
                {
                    //assign condition to player if at their node, otherwise actor
                    if (node.nodeID == GameManager.i.nodeScript.GetPlayerNodeID())
                    {
                        //Player Condition
                        switch (effect.operand.name)
                        {
                            case "Add":
                                //only add condition if NOT already present
                                if (GameManager.i.playerScript.CheckConditionPresent(condition, dataInput.side) == false)
                                {
                                    GameManager.i.playerScript.AddCondition(condition, dataInput.side, string.Format("due to {0}", dataInput.originText));
                                    effectResolve.bottomText = string.Format("{0}Player gains condition {1}{2}", colourEffect, condition.tag, colourEnd);
                                    GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"Gains {condition.tag}");
                                }
                                break;
                            case "Subtract":
                                //only remove  condition if present
                                if (GameManager.i.playerScript.CheckConditionPresent(condition, dataInput.side) == true)
                                {
                                    GameManager.i.playerScript.RemoveCondition(condition, dataInput.side, string.Format("due to {0}", dataInput.originText));
                                    effectResolve.bottomText = string.Format("{0}Your {1} condition is removed{2}", colourEffect, condition.tag, colourEnd);
                                    GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"Loses {condition.tag}");
                                }
                                else
                                {
                                    //condition not present -> explanatory message 
                                    effectResolve.bottomText = string.Format("{0}Your {1} condition would have be removed, if present{2}", colourNeutral, condition.tag, colourEnd);
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
                                        actor.AddCondition(condition, string.Format("due to {0}", dataInput.originText));
                                        effectResolve.bottomText = string.Format("{0}{1} condition gained{2}", colourEffect, condition.tag, colourEnd);
                                        GameManager.i.popUpFixedScript.SetData(actor.slotID, $"Gains {condition.tag}");
                                    }
                                    break;
                                case "Subtract":
                                    //only remove  condition if present
                                    if (actor.CheckConditionPresent(condition) == true)
                                    {
                                        actor.RemoveCondition(condition, string.Format("due to {0}", dataInput.originText));
                                        effectResolve.bottomText = string.Format("{0}{1} condition removed{2}", colourEffect, condition.tag, colourEnd);
                                        GameManager.i.popUpFixedScript.SetData(actor.slotID, $"Loses {condition.tag}");
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
                GlobalType type = GameManager.i.globalScript.typeGood;
                //get type
                switch (effect.outcome.name)
                {
                    case "ConditionGroupGood":
                        //default condition, no need to reassign
                        break;
                    case "ConditionGroupBad":
                        type = GameManager.i.globalScript.typeBad;
                        effectType = "Bad";
                        colourConditionAdd = colourBadSide;
                        colourConditionRemove = colourGoodSide;
                        break;
                    default:
                        Debug.LogWarning(string.Format("Invalid outcome name \"{0}\"", effect.outcome.name));
                        break;
                }
                //Player
                if (node.nodeID == GameManager.i.nodeScript.GetPlayerNodeID())
                {
                    if (GameManager.i.playerScript.CheckNumOfConditions(dataInput.side) > 0)
                    {
                        listOfConditions = GameManager.i.playerScript.GetListOfConditions(dataInput.side);
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
                                            GameManager.i.playerScript.AddCondition(conditionRandom, dataInput.side, string.Format("due to {0}", dataInput.originText));
                                            effectResolve.bottomText = string.Format("{0}{1} condition gained{2}", colourConditionAdd, conditionRandom.tag, colourEnd);
                                            GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"Gains {conditionRandom.tag}");
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
                                            GameManager.i.playerScript.RemoveCondition(conditionRandom, dataInput.side, string.Format("due to {0}", dataInput.originText));
                                            effectResolve.bottomText = string.Format("{0}{1} condition removed{2}", colourConditionRemove, conditionRandom.tag, colourEnd);
                                            GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"Loses {conditionRandom.tag}");
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
                                            typeCompare = GameManager.i.globalScript.typeGood.name;
                                            for (int i = listOfConditions.Count - 1; i >= 0; i--)
                                            {
                                                if (listOfConditions[i].type.name.Equals(typeCompare, StringComparison.Ordinal) == true)
                                                {
                                                    GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"Loses {listOfConditions[i].tag}");
                                                    listOfConditions.RemoveAt(i); counter++;
                                                }
                                            }
                                            effectResolve.bottomText = string.Format("{0}All ({1}) Good conditions removed{2}", colourBadSide, counter, colourEnd);
                                        }
                                        break;
                                    case "ConditionGroupBad":
                                        //remove all bad conditions
                                        if (listOfConditions.Count > 0)
                                        {
                                            typeCompare = GameManager.i.globalScript.typeBad.name;
                                            for (int i = listOfConditions.Count - 1; i >= 0; i--)
                                            {
                                                if (listOfConditions[i].type.name.Equals(typeCompare, StringComparison.Ordinal) == true)
                                                {
                                                    GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"Loses {listOfConditions[i].tag}");
                                                    listOfConditions.RemoveAt(i); counter++;
                                                }
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
                                                actor.AddCondition(conditionRandom, string.Format("due to {0}", dataInput.originText));
                                                effectResolve.bottomText = string.Format("{0}{1} condition gained{2}", colourConditionAdd, conditionRandom.tag, colourEnd);
                                                GameManager.i.popUpFixedScript.SetData(actor.slotID, $"Gains {conditionRandom.tag}");
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
                                                actor.RemoveCondition(conditionRandom, string.Format("due to {0}", dataInput.originText));
                                                effectResolve.bottomText = string.Format("{0}{1} condition removed{2}", colourConditionRemove, conditionRandom.name, colourEnd);
                                                GameManager.i.popUpFixedScript.SetData(actor.slotID, $"Loses {conditionRandom.tag}");
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
                                                typeCompare = GameManager.i.globalScript.typeGood.name;
                                                for (int i = listOfConditions.Count - 1; i >= 0; i--)
                                                {
                                                    if (listOfConditions[i].type.name.Equals(typeCompare, StringComparison.Ordinal) == true)
                                                    {
                                                        GameManager.i.popUpFixedScript.SetData(actor.slotID, $"Loses {listOfConditions[i].tag}");
                                                        listOfConditions.RemoveAt(i); counter++;
                                                    }
                                                }
                                                effectResolve.bottomText = string.Format("{0}All ({1}) Good conditions removed{2}", colourBadSide, counter, colourEnd);
                                            }
                                            break;
                                        case "ConditionGroupBad":
                                            //remove all bad conditions
                                            if (listOfConditions.Count > 0)
                                            {
                                                typeCompare = GameManager.i.globalScript.typeBad.name;
                                                for (int i = listOfConditions.Count - 1; i >= 0; i--)
                                                {
                                                    if (listOfConditions[i].type.name.Equals(typeCompare, StringComparison.Ordinal) == true)
                                                    {
                                                        GameManager.i.popUpFixedScript.SetData(actor.slotID, $"Loses {listOfConditions[i].tag}");
                                                        listOfConditions.RemoveAt(i); counter++;
                                                    }
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
                            if (condition.type.name.Equals(type.name, StringComparison.Ordinal) == true)
                            { tempList.Add(condition); }
                        }
                        //select a condition
                        int numOfConditions = tempList.Count;
                        if (numOfConditions > 0)
                        { conditionSelected = tempList[Random.Range(0, numOfConditions)]; }
                        break;
                    case "Add":
                        //randomly select a new condition of the required type that is NOT on the provided list
                        Dictionary<string, Condition> dictOfConditionsByType = GameManager.i.dataScript.GetDictOfConditionsByType(type);
                        if (dictOfConditionsByType.Count > 0)
                        {
                            //loop listOfConditions and remove any identical entries in the temp dict
                            foreach (Condition condition in listOfConditions)
                            {
                                if (dictOfConditionsByType.ContainsKey(condition.tag))
                                { dictOfConditionsByType.Remove(condition.tag); }
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
                GameManager.i.popUpFixedScript.SetData(actor.slotID, "To Reserves");
                break;
            case "ActorPromoted":
                effectResolve.bottomText = string.Format("{0}{1} promoted and will join {2}{3}", colourEffect, actor.actorName,
                    GameManager.i.hqScript.GetCurrentHQ().tag, colourEnd);
                GameManager.i.popUpFixedScript.SetData(actor.slotID, "Promoted");
                break;
            case "ActorDismissed":
                effectResolve.bottomText = string.Format("{0}{1} dismissed{2}", colourEffect, actor.actorName, colourEnd);
                GameManager.i.popUpFixedScript.SetData(actor.slotID, "Dismissed");
                break;
            case "ActorDisposedOff":
                effectResolve.bottomText = string.Format("{0}{1} killed{2}", colourEffect, actor.actorName, colourEnd);
                GameManager.i.popUpFixedScript.SetData(actor.slotID, "Disposed Off");
                break;
            case "ManageReserveRenown":
                data = GameManager.i.actorScript.manageReserveRenown;
                GameManager.i.playerScript.Renown -= data;
                effectResolve.bottomText = string.Format("{0}Player Renown -{1}{2}", colourEffect, data, colourEnd);
                GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"Renown -{data}");
                break;
            case "ManageDismissRenown":
                ManageRenownCost manageDismissCost = GameManager.i.actorScript.GetManageRenownCost(actor, GameManager.i.actorScript.manageDismissRenown);
                data = manageDismissCost.renownCost;
                GameManager.i.playerScript.Renown -= data;
                StringBuilder builderDismiss = new StringBuilder();
                if (string.IsNullOrEmpty(manageDismissCost.tooltip) == false)
                { builderDismiss.AppendLine(manageDismissCost.tooltip); builderDismiss.AppendLine(); }
                builderDismiss.AppendFormat("{0}Player Renown -{1}{2}", colourEffect, data, colourEnd);
                effectResolve.bottomText = builderDismiss.ToString();
                GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"Renown -{data}");
                break;
            case "ManageDisposeRenown":
                ManageRenownCost manageDisposeCost = GameManager.i.actorScript.GetManageRenownCost(actor, GameManager.i.actorScript.manageDisposeRenown);
                data = manageDisposeCost.renownCost;
                GameManager.i.playerScript.Renown -= data;
                StringBuilder builderDispose = new StringBuilder();
                if (string.IsNullOrEmpty(manageDisposeCost.tooltip) == false)
                { builderDispose.AppendLine(manageDisposeCost.tooltip); builderDispose.AppendLine(); }
                builderDispose.AppendFormat("{0}Player Renown -{1}{2}", colourEffect, data, colourEnd);
                effectResolve.bottomText = builderDispose.ToString();
                GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"Renown -{data}");
                break;
            case "UnhappyTimerCurrent":
                data = GameManager.i.actorScript.currentReserveTimer;
                //traits that affect unhappy timer
                string traitText = "";
                if (actor.CheckTraitEffect(actorReserveTimerDoubled) == true)
                {
                    data *= 2; traitText = string.Format(" ({0})", actor.GetTrait().tag);
                    GameManager.i.actorScript.TraitLogMessage(actor, "for their willingness to wait", "to DOUBLE Reserve Unhappy Timer");
                }
                else if (actor.CheckTraitEffect(actorReserveTimerHalved) == true)
                {
                    data /= 2; data = Mathf.Max(1, data); traitText = string.Format(" ({0})", actor.GetTrait().tag);
                    GameManager.i.actorScript.TraitLogMessage(actor, "for their reluctance to wait", "to HALVE Reserve Unhappy Timer");
                }
                //set timer
                actor.unhappyTimer = data;
                effectResolve.bottomText = string.Format("{0}{1}'s Unhappy Timer set to {2} turn{3}{4}{5}", colourEffect, actor.actorName, data,
                    data != 1 ? "s" : "", traitText, colourEnd);
                break;
            case "ActorPromised":
                actor.isPromised = true;
                effectResolve.bottomText = string.Format("{0}{1} has been Promised{2}", colourEffect, actor.actorName, colourEnd);
                GameManager.i.popUpFixedScript.SetData(actor.slotID, "To Reserves");
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
                    if (GameManager.i.dataScript.RemoveCurrentActor(GameManager.i.sideScript.PlayerSide, actor, ActorStatus.Resigned) == true)
                    {
                        effectResolve.bottomText = string.Format("{0}{1} Resigns{2}", colourBadSide, actor.arc.name, colourEnd);
                        GameManager.i.popUpFixedScript.SetData(actor.slotID, "Resigns");
                    }
                }
                else
                {
                    //trait actorResignNone "Loyal"
                    GameManager.i.actorScript.TraitLogMessage(actor, "for a Resignation check", "to AVOID Resigning");
                    effectResolve.bottomText = string.Format("{0}{1} has {2}{3}{4}{5}{6} trait{7}", colourAlert, actor.arc.name, colourEnd,
                        colourNeutral, actor.GetTrait().tag, colourEnd, colourAlert, colourEnd);
                }
                break;
            case "ActorKills":
                //NOTE: Not a Manage option
                effectResolve.bottomText = GameManager.i.actorScript.ProcessKillRandomActor(actor);
                GameManager.i.popUpFixedScript.SetData(actor.slotID, "Kills");
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
        GlobalSide side = GameManager.i.sideScript.PlayerSide;
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
                    actionAdjustment.sideLevel = side.level;
                    string appliesWhen = "NEXT TURN";
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
                                    GameManager.i.dataScript.AddActionAdjustment(actionAdjustment);
                                    effectResolve.bottomText = string.Format("{0}Player gains {1}{2}{3}{4}{5} extra action{6} {7}{8}{9}{10}", colourEffect, colourEnd,
                                        colourNeutral, effect.value, colourEnd, colourEffect, effect.value != 1 ? "s" : "", colourEnd, colourNeutral, appliesWhen, colourEnd);
                                    GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"+{effect.value} Action");
                                    break;
                                case "Subtract":
                                    actionAdjustment.value = effect.value * -1;
                                    GameManager.i.dataScript.AddActionAdjustment(actionAdjustment);
                                    effectResolve.bottomText = string.Format("{0}Player loses {1}{2}{3}{4}{5} action{6} {7}{8}{9}{10}", colourEffect, colourEnd,
                                        colourNeutral, effect.value, colourEnd, colourEffect, effect.value != 1 ? "s" : "", colourEnd, colourNeutral, appliesWhen, colourEnd);
                                    GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"{effect.value} Action");
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effect.operand \"{0}\"", effect.operand.name));
                                    break;
                            }
                            break;
                        case "Ongoing":
                            //NOTE: Ongoing effects are handled differently here than a standard ongoing effect (there is also an extra +1 due to decrement at end of turn)
                            actionAdjustment.timer = GameManager.i.effectScript.ongoingEffectTimer + 1;
                            actionAdjustment.descriptor = dataInput.originText;
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    actionAdjustment.ongoingID = AddOngoingEffectToDict(effect, dataInput, effect.value);
                                    actionAdjustment.value = effect.value;
                                    GameManager.i.dataScript.AddActionAdjustment(actionAdjustment);
                                    effectResolve.bottomText = string.Format("{0}Player gains {1}{2}{3}{4}{5} extra action{6} for {7}{8}{9}{10}{11} turns commencing {12}{13}{14}{15}",
                                        colourEffect, colourEnd, colourNeutral, effect.value, colourEnd, colourEffect, effect.value != 1 ? "s" : "", colourEnd, colourNeutral,
                                        actionAdjustment.timer - 1, colourEnd, colourEffect, colourEnd, colourNeutral, appliesWhen, colourEnd);
                                    GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"+{effect.value} Action (Ongoing)");
                                    break;
                                case "Subtract":
                                    actionAdjustment.ongoingID = AddOngoingEffectToDict(effect, dataInput, effect.value * -1);
                                    actionAdjustment.value = effect.value * -1;
                                    GameManager.i.dataScript.AddActionAdjustment(actionAdjustment);
                                    effectResolve.bottomText = string.Format("{0}Player loses {1}{2}{3}{4}{5} action{6} for {7}{8}{9}{10}{11} turns commencing {12}{13}{14}{15}",
                                        colourEffect, colourEnd, colourNeutral, effect.value, colourEnd, colourEffect, effect.value != 1 ? "s" : "", colourEnd, colourNeutral,
                                        actionAdjustment.timer - 1, colourEnd, colourEffect, colourEnd, colourNeutral, appliesWhen, colourEnd);
                                    GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"{effect.value} Action (Ongoing)");
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
                case "Innocence":
                    switch (effect.operand.name)
                    {
                        case "Add":
                            GameManager.i.playerScript.Innocence++;
                            effectResolve.bottomText = string.Format("{0}Player Innocence +1{1}", colourEffect, colourEnd);
                            GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Innocence +1");
                            GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopBarLeft, "Innocence +1");
                            break;
                        case "Subtract":
                            GameManager.i.playerScript.Innocence--;
                            effectResolve.bottomText = string.Format("{0}Player Innocence -1{1}", colourEffect, colourEnd);
                            GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Innocence -1");
                            GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopBarLeft, "Innocence -1");
                            break;
                    }
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.outcome \"{0}\"", effect.outcome.name));
                    break;
            }
        }
        else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name)); }
        return effectResolve;
    }

    /// <summary>
    /// subMethod to process player mood personality related effects
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveMoodData(Effect effect, EffectDataInput dataInput)
    {
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //dataInput.Data is topic.option.isIgnoreMood
        if (effect.belief != null)
        {
            if (dataInput.data == 0)
            { effectResolve.bottomText = GameManager.i.personScript.UpdateMood(effect.belief, dataInput.originText); }
            else
            {
                //topic.option.isIgnoreMood only applies if Player is Stressed
                if (GameManager.i.playerScript.isStressed == false)
                { effectResolve.bottomText = GameManager.i.personScript.UpdateMood(effect.belief, dataInput.originText); }
                else { effectResolve.bottomText = string.Format("{0}No Effect on Player Mood{1}", colourGrey, colourEnd); }
            }
        }
        else { effectResolve.bottomText = string.Format("{0}No Effect on Player Mood{1}", colourGrey, colourEnd); }
        return effectResolve;
    }


    /// <summary>
    /// subMethod to handle Ongoing Effects for Gear (Personal Use -> Actions +/-)
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    private int AddOngoingEffectToDict(Effect effect, EffectDataInput effectInput, int value)
    {
        EffectDataOngoing effectOngoing = new EffectDataOngoing();
        effectOngoing.effectOutcome = effect.outcome.name;
        effectOngoing.ongoingID = effectInput.ongoingID;
        effectOngoing.typeLevel = effect.typeOfEffect.level;
        effectOngoing.effectApply = effect.apply.name;
        effectOngoing.sideLevel = effectInput.side.level;
        effectOngoing.value = effect.value;
        effectOngoing.nodeID = -1;
        effectOngoing.reason = effectInput.ongoingText;
        effectOngoing.description = effect.description;
        effectOngoing.text = string.Format("{0} ({1} turn{2})", effect.description, effectOngoing.timer, effectOngoing.timer != 1 ? "s" : "");
        effectOngoing.nodeTooltip = effect.ongoingTooltip;
        //add to register & create message
        GameManager.i.dataScript.AddOngoingEffectToDict(effectOngoing);
        return effectOngoing.ongoingID;
    }

    /// <summary>
    /// subMethod to handle Ongoing effects for Nodes
    /// </summary>
    private void ProcessOngoingEffect(Effect effect, EffectDataProcess effectProcess, EffectDataResolve effectResolve, EffectDataInput effectInput, Node node, int value)
    {
        EffectDataOngoing effectOngoing = new EffectDataOngoing();
        effectOngoing.effectOutcome = effect.outcome.name;
        effectOngoing.ongoingID = effectInput.ongoingID;
        effectOngoing.value = value;
        effectOngoing.typeLevel = effect.typeOfEffect.level;
        effectOngoing.effectApply = effect.apply.name;
        effectOngoing.sideLevel = effectInput.side.level;
        effectOngoing.reason = effectInput.ongoingText;
        effectOngoing.description = effect.description;
        effectOngoing.nodeID = node.nodeID;
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

    //
    // - - - MetaGame effects - - -
    //

    public EffectDataResolve ResolveMetaGame(Effect effect, EffectDataInput dataInput)
    {
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        effectResolve.bottomText = "Unknown";
        string colourEffect = colourGood;
        if (effect.typeOfEffect.name.Equals("Bad", StringComparison.Ordinal) == true) { colourEffect = colourBad; }
        switch (effect.outcome.name)
        {
            case "MetaOptionDismissed":
                GameManager.i.metaScript.SetMetaGameDismissed(false);
                effectResolve.bottomText = string.Format("{0}Dismissed Subordinates are excluded{1}", colourEffect, colourEnd);
                break;
            case "MetaOptionResigned":
                GameManager.i.metaScript.SetMetaGameResigned(false);
                effectResolve.bottomText = string.Format("{0}Subordinates who Resigned are excluded{1}", colourEffect, colourEnd);
                break;
            case "MetaOptionTraitor":
                GameManager.i.metaScript.SetMetaGameTraitor(false);
                effectResolve.bottomText = string.Format("{0}Traitorous Subordinates are excluded{1}", colourEffect, colourEnd);
                break;
            case "MetaOptionMotivation":
                GameManager.i.metaScript.SetMetaGameMotivation(false);
                effectResolve.bottomText = string.Format("{0}Low Motivation Subordinates are excluded{1}", colourEffect, colourEnd);
                break;
            case "MetaOptionLevel2":
                //level 2 and 3 are binary choices
                GameManager.i.metaScript.SetMetaGameLevelTwo(true);
                effectResolve.bottomText = string.Format("{0}All Subordinates will be level 2{1}", colourEffect, colourEnd);
                break;
            case "MetaOptionGear":
                //special gear available because HQ actor has a good opinion of player
                Gear gear = GameManager.i.dataScript.GetGear(dataInput.dataName);
                if (gear != null)
                {
                    //place in listOfSpecialGear
                    if (GameManager.i.dataScript.AddSpecialGear(gear) == true)
                    { effectResolve.bottomText = string.Format("{0}gained {1} gear{2}", colourEffect, gear.tag, colourEnd); }
                }
                break;

            default: Debug.LogWarningFormat("Invalid MetaGame effect.outcome.name \"{0}\"", effect.outcome.name); break;
        }
        return effectResolve;
    }

    //
    // - - - Topic Effects - - -
    //

    /// <summary>
    /// subMethod to process topic specific effets. ColourEffect/Text are side specific colours for effects that vary good/bad depending on side, eg. city loyalty
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <returns></returns>
    public EffectDataResolve ResolveTopicData(Effect effect, EffectDataInput dataInput)
    {
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //get topic data
        TopicEffectData data = GameManager.i.topicScript.GetTopicEffectData();
        if (data != null)
        {
            //topic effects use a specific naming convention, eg. 'X_...'
            char key = effect.name[0];
            //error check
            if (effect.name[1].Equals('_') == true)
            {
                //text
                switch (key)
                {
                    case 'A':
                        //Actor
                        if (data.actorID > -1)
                        {
                            Actor actor = null;
                            //check for HQ actor
                            if (data.isHqActors == false)
                            { actor = GameManager.i.dataScript.GetActor(data.actorID); }
                            else { actor = GameManager.i.dataScript.GetHqActor(data.actorID); }
                            if (actor != null)
                            { effectResolve = ResolveTopicActorEffect(effect, dataInput, data, actor); }
                            else
                            {
                                Debug.LogWarningFormat("Invalid actor (Null) for effect \"{0}\", data.actorID {1}", effect.name, data.actorID);
                                effectResolve.isError = true;
                            }
                        }
                        else
                        {
                            Debug.LogWarningFormat("Invalid data.actorID (less than Zero) for effect \"{0}\", data.actorID {1}", effect.name, data.actorID);
                            effectResolve.isError = true;
                        }
                        break;
                    case 'B':
                        //dual actor effect
                        if (data.actorID > -1)
                        {
                            if (data.actorOtherID > -1)
                            {
                                Actor actor = null;
                                //check for HQ actors
                                if (data.isHqActors == false)
                                { actor = GameManager.i.dataScript.GetActor(data.actorID); }
                                else { actor = GameManager.i.dataScript.GetHqActor(data.actorID); }
                                if (actor != null)
                                {
                                    Actor actorOther = null;
                                    if (data.isHqActors == false)
                                    { actorOther = GameManager.i.dataScript.GetActor(data.actorOtherID); }
                                    else { actorOther = GameManager.i.dataScript.GetHqActor(data.actorOtherID); }
                                    if (actorOther != null)
                                    { effectResolve = ResolveTopicDualActorEffect(effect, dataInput, data, actor, actorOther); }
                                    else
                                    {
                                        Debug.LogWarningFormat("Invalid actorOther (Null) for effect \"{0}\", data.actorOtherID {1}", effect.name, data.actorOtherID);
                                        effectResolve.isError = true;
                                    }
                                }
                                else
                                {
                                    Debug.LogWarningFormat("Invalid actor (Null) for effect \"{0}\", data.actorID {1}", effect.name, data.actorID);
                                    effectResolve.isError = true;
                                }
                            }
                            else
                            {
                                Debug.LogWarningFormat("Invalid data.actorOtherID (less than Zero) for effect \"{0}\", data.actorOtherID {1}", effect.name, data.actorOtherID);
                                effectResolve.isError = true;
                            }
                        }
                        else
                        {
                            Debug.LogWarningFormat("Invalid data.actorID (less than Zero) for effect \"{0}\", data.actorID {1}", effect.name, data.actorID);
                            effectResolve.isError = true;
                        }
                        break;
                    case 'R':
                        //Random Active Actor (if resistance chooses one with the highest invisibility)
                        Actor actorHighest = GameManager.i.dataScript.GetActiveActorHighestInvisibility(dataInput.side);
                        if (actorHighest != null)
                        { effectResolve = ResolveTopicActorEffect(effect, dataInput, data, actorHighest); }
                        else
                        {
                            Debug.LogWarningFormat("Invalid Random actor (Null) for effect \"{0}\"", effect.name);
                            effectResolve.isError = true;
                        }
                        break;
                    case 'L':
                        //All Actors
                        if (ResolveGroupActorEffect(effect, dataInput) == true)
                        {
                            switch (effect.typeOfEffect.name)
                            {
                                case "Good": effectResolve.bottomText = string.Format("{0}{1}{2}", colourGood, effect.description, colourEnd); break;
                                case "Bad": effectResolve.bottomText = string.Format("{0}{1}{2}", colourBad, effect.description, colourEnd); break;
                                default: Debug.LogWarningFormat("Unrecognised effect.typeOfEffect \"{0}\"", effect.typeOfEffect); effectResolve.bottomText = effect.description; break;
                            }
                        }
                        else { effectResolve.isError = true; }
                        break;
                    case 'P':
                        //Player
                        effectResolve = ResolveTopicPlayerEffect(effect, dataInput, data);
                        break;
                    case 'N':
                        //Node
                        effectResolve = ResolveTopicNodeEffect(effect, dataInput, data);
                        break;
                    case 'C':
                        //City
                        effectResolve = ResolveTopicCityEffect(effect, dataInput, data);
                        break;
                    case 'O':
                        //Organisation
                        effectResolve = ResolveTopicOrganisationEffect(effect, dataInput, data);
                        break;
                    case 'H':
                        //HQ (Faction)
                        effectResolve = ResolveTopicHQEffect(effect, dataInput, data);
                        break;
                    case 'T':
                        //Target
                        effectResolve = ResolveTopicTargetEffect(effect, dataInput, data);
                        break;
                    default: Debug.LogWarningFormat("Unrecognised key \"{0}\" for effect {1}", key, effect.name); break;
                }
            }
            else
            {
                Debug.LogWarningFormat("Invalid topic effect \"{0}\" (Incorrect prefix, should be 'X_...')", effect.name);
                effectResolve.isError = true;
            }
        }
        else { Debug.LogWarning("Invalid TopicEffectData (Null)"); }
        return effectResolve;
    }

    /// <summary>
    /// private subMethod for ResolveTopicData that handles all topic Actor effects. Returns an EffectDataResolve data package in all cases (default data if a problem)
    /// NOTE: parent method has checked all parameters for null
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <param name="dataTopic"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveTopicActorEffect(Effect effect, EffectDataInput dataInput, TopicEffectData dataTopic, Actor actor)
    {
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //default data
        effectResolve.topText = "Unknown effect";
        effectResolve.bottomText = "Unknown effect";
        effectResolve.isError = false;
        //get node (sometimes won't be needed)
        Node node = null;
        if (dataTopic.nodeID > -1)
        { node = GameManager.i.dataScript.GetNode(dataTopic.nodeID); }
        //outcome
        switch (effect.outcome.name)
        {
            case "Renown":
                effectResolve.bottomText = ExecuteActorRenown(effect, actor, dataInput, dataTopic.isHqActors);
                break;
            case "ContactGainLose":
            case "ContactStatus":
            case "ContactEffectiveness":
                effectResolve.bottomText = ExecuteActorContact(effect, actor, dataTopic, dataInput);
                break;
            case "Motivation":
                effectResolve.bottomText = ExecuteActorMotivation(effect, actor, dataInput, dataTopic);
                break;
            case "Invisibility":
                if (node != null)
                { effectResolve.bottomText = ExecuteActorInvisibility(effect, dataInput, actor, node); }
                else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", dataTopic.nodeID); }
                break;
            case "ConditionStressed":
            case "ConditionIncompetent":
            case "ConditionCorrupt":
            case "ConditionQuestionable":
            case "ConditionBlackmailer":
            case "ConditionStar":
                if (node != null)
                { effectResolve.bottomText = ExecuteActorCondition(effect, dataInput, actor); }
                else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", dataTopic.nodeID); }
                break;
            case "Secret":
                //remove a secret from actor
                Secret secret = GameManager.i.dataScript.GetSecret(dataTopic.secret);
                if (secret != null)
                {
                    if (actor.RemoveSecret(dataTopic.secret) == true)
                    {
                        effectResolve.bottomText = string.Format("{0}{1} secret lost{2}", colourGood, secret.tag, colourEnd);
                        GameManager.i.popUpFixedScript.SetData(actor.slotID, "Secret Lost");
                    }
                }
                else { Debug.LogWarningFormat("Invalid secret (Null) for dataTopic.secret \"{0}\"", dataTopic.secret); }
                break;
            case "SecretRandom":
                secret = actor.GetRandomCurrentSecret();
                if (secret != null)
                {
                    //removes secret from actor only (could still be with other actors and will always be with the player)
                    actor.RemoveSecret(secret.name);
                    effectResolve.bottomText = string.Format("{0}\"{1}\" secret deleted from {2}{3}", colourGood, secret.tag, actor.arc.name, colourEnd);
                    GameManager.i.popUpFixedScript.SetData(actor.slotID, "Secret Lost");
                }
                break;
            case "Gear":
                //remove gear from actor
                if (actor.RemoveGear(GearRemoved.Decision) == true)
                {
                    effectResolve.bottomText = string.Format("{0}{1}, {2} loses {3} gear{4}", colourBad, actor.actorName, actor.arc.name, dataTopic.gearName, colourEnd);
                    GameManager.i.popUpFixedScript.SetData(actor.slotID, "Gear Lost");
                }
                break;
            case "ActorDismissed":
                //fire actor
                if (GameManager.i.dataScript.RemoveCurrentActor(dataInput.side, actor, ActorStatus.Dismissed) == true)
                {
                    effectResolve.bottomText = string.Format("{0}{1}, {2}, Fired{3}", colourBad, actor.actorName, actor.arc.name, colourEnd);
                    GameManager.i.popUpFixedScript.SetData(actor.slotID, "Dismissed");
                }
                break;
            case "ActorToReserves":
                //move actor to reserves
                if (GameManager.i.dataScript.RemoveCurrentActor(dataInput.side, actor, ActorStatus.Reserve) == true)
                {
                    effectResolve.bottomText = string.Format("{0}{1}, {2}, moved to Reserves{3}", colourBad, actor.actorName, actor.arc.name, colourEnd);
                    GameManager.i.popUpFixedScript.SetData(actor.slotID, "To Reserves");
                }
                break;
            case "ActorKilledOrg":
                //OrgContract kills an actor
                if (GameManager.i.dataScript.RemoveCurrentActor(dataInput.side, actor, ActorStatus.Killed) == true)
                {
                    effectResolve.bottomText = string.Format("{0}{1}, {2}, Killed{3}", colourBad, actor.actorName, actor.arc.name, colourEnd);
                    OrgData data = new OrgData() { text = actor.arc.name, turn = GameManager.i.turnScript.Turn };
                    GameManager.i.dataScript.AddOrgData(data, OrganisationType.Contract);
                    GameManager.i.dataScript.StatisticIncrement(StatType.OrgContractHits);
                    GameManager.i.popUpFixedScript.SetData(actor.slotID, "Contract Hit");
                }
                break;
            case "ActorKilledCapture":
                //Player reveals location of actor while in capture
                if (GameManager.i.dataScript.RemoveCurrentActor(dataInput.side, actor, ActorStatus.Killed) == true)
                {
                    effectResolve.bottomText = string.Format("{0}{1}, {2}, Killed (Betrayed by Player){3}{4}", colourBad, actor.actorName, actor.arc.name, colourEnd, "\n");
                    GameManager.i.popUpFixedScript.SetData(actor.slotID, "Betrayed");
                }
                break;
            default: Debug.LogWarningFormat("Unrecognised effect.outcome \"{0}\" for effect {1}", effect.outcome.name, effect.name); break;
        }

        return effectResolve;
    }

    /// <summary>
    /// private subMethod for ResolveTopicData that handles all custom dual actor effects (eg. ActorPolitic topics -> Friend/Enemy relationship, HQ topics -> actorOther Motivation shift)
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <param name="dataTopic"></param>
    /// <param name="actor"></param>
    /// <param name="actorOther"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveTopicDualActorEffect(Effect effect, EffectDataInput dataInput, TopicEffectData dataTopic, Actor actor, Actor actorOther)
    {
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //default data
        effectResolve.topText = "Unknown effect";
        effectResolve.bottomText = "Unknown effect";
        effectResolve.isError = false;
        switch (effect.outcome.name)
        {
            case "Relationship":
                //assign relationship
                ActorRelationship relationship = dataTopic.relation;
                if (GameManager.i.dataScript.AddRelationship(actor.slotID, actorOther.slotID, actor.actorID, actorOther.actorID, relationship) == true)
                {
                    effectResolve.bottomText = string.Format("{0}{1} and {2} are now {3}{4}", colourBad, actor.arc.name, actorOther.arc.name,
                      relationship == ActorRelationship.Friend ? "Friends" : "Enemies", colourEnd);
                    //history
                    actor.AddHistory(new HistoryActor()
                    {
                        text = string.Format("Has become {0} with {1}, {2}", relationship == ActorRelationship.Friend ? "Friends" : "Enemies",
                        actorOther.actorName, actorOther.arc.name)
                    });
                    actorOther.AddHistory(new HistoryActor()
                    {
                        text = string.Format("Has become {0} with {1}, {2}", relationship == ActorRelationship.Friend ? "Friends" : "Enemies",
                        actor.actorName, actor.arc.name)
                    });
                    GameManager.i.popUpFixedScript.SetData(actor.slotID, "Friends");
                    GameManager.i.popUpFixedScript.SetData(actorOther.slotID, "Friends");
                }
                break;
            case "Motivation":
                //actor Other changes motivation
                effectResolve.bottomText = ExecuteActorMotivation(effect, actorOther, dataInput, dataTopic);
                break;
            case "Renown":
                //actor Other changes Renown
                effectResolve.bottomText = ExecuteActorRenown(effect, actorOther, dataInput, dataTopic.isHqActors);
                break;
        }
        return effectResolve;
    }


    /// <summary>
    /// private subMethod for ResolveTopicData that handles all topic Player effects. Returns an EffectDataResolve data package in all cases (default data if a problem)
    /// NOTE: parent method has checked all parameters for null
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <param name="dataTopic"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveTopicPlayerEffect(Effect effect, EffectDataInput dataInput, TopicEffectData dataTopic)
    {
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //default data
        effectResolve.topText = "Unknown effect";
        effectResolve.bottomText = "Unknown effect";
        effectResolve.isError = false;
        //get node (sometimes won't be needed)
        Node node = null;
        if (dataTopic.nodeID > -1)
        { node = GameManager.i.dataScript.GetNode(dataTopic.nodeID); }
        //outcome
        switch (effect.outcome.name)
        {
            case "Renown":
                effectResolve.bottomText = ExecutePlayerRenown(effect);
                break;
            case "Invisibility":
                if (node == null)
                {
                    //get player node for a general loss of invisibility effect, if none present
                    node = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
                }
                if (node != null)
                { effectResolve.bottomText = ExecutePlayerInvisibility(effect, dataInput, node); }
                else { Debug.LogError("Invalid node (Null)"); }
                break;
            case "ConditionStressed":
            case "ConditionIncompetent":
            case "ConditionCorrupt":
            case "ConditionQuestionable":
            case "ConditionBlackmailer":
            case "ConditionAddicted":
            case "ConditionStar":
            case "ConditionTagged":
                /*if (node != null)
                { effectResolve.bottomText = ExecutePlayerCondition(effect, dataInput, node); }
                else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", dataTopic.nodeID); }*/
                effectResolve.bottomText = ExecutePlayerCondition(effect, dataInput);
                break;
            case "CaptureTool":
                effectResolve.bottomText = ExecuteCaptureTool(effect, dataInput);
                break;
            case "CureAddicted":
            case "CureTagged":
            case "CureWounded":
            case "CureImaged":
            case "CureDoomed":
            case "CureQuestionable":
                effectResolve.bottomText = ExecutePlayerCure(effect, dataInput, dataTopic);
                break;
            case "Drugs":
                effectResolve.bottomText = ExecutePlayerTakeDrugs(effect);
                break;
            case "Gear":
            case "GearSpecific":
                effectResolve.bottomText = ExecutePlayerLoseGear(effect, dataTopic);
                break;
            case "PlayerActions":
                //player gains or loses and action next turn
                effectResolve = ResolvePlayerData(effect, dataInput);
                break;
            case "ChanceAddictedLow":
            case "ChanceAddictedMed":
            case "ChanceAddictedHigh":
                effectResolve.bottomText = ExecutePlayerChanceAddicted(effect, dataInput);
                break;
            case "InvestigationNormal":
                effectResolve.bottomText = GameManager.i.playerScript.SetInvestigationFlagNormal(dataTopic.investigationRef);
                break;
            case "InvestigationTimer":
                effectResolve.bottomText = GameManager.i.playerScript.SetInvestigationFlagTimer(dataTopic.investigationRef);
                break;
            case "InvestigationDropped":
                effectResolve.bottomText = GameManager.i.playerScript.DropInvestigation(dataTopic.investigationRef);
                GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopBarLeft, "Investigation Dropped");
                break;
            case "Innocence":
                effectResolve = ResolvePlayerData(effect, dataInput);
                break;
            case "CaptureRelease":
                //player released from captivity
                effectResolve.bottomText = ExecutePlayerRelease(effect, dataInput);
                break;
            case "CaptureEscape":
                //player escapes from captivity
                effectResolve.bottomText = ExecutePlayerEscape(effect, dataInput);
                break;
            case "CapturePermanent":
                //player locked up oermanently, end of campaign
                effectResolve.bottomText = ExecuteGameOver();
                break;
            default: Debug.LogWarningFormat("Unrecognised effect.outcome \"{0}\" for effect {1}", effect.outcome.name, effect.name); break;
        }
        return effectResolve;
    }

    /// <summary>
    /// private subMethod for ResolveTopicData that handles all topic Node effects. Returns an EffectDataResolve data package in all cases (default data if a problem)
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <param name="dataTopic"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveTopicNodeEffect(Effect effect, EffectDataInput dataInput, TopicEffectData dataTopic)
    {
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //default data
        effectResolve.topText = "Unknown effect";
        effectResolve.bottomText = "Unknown effect";
        effectResolve.isError = false;
        Node node = GameManager.i.dataScript.GetNode(dataTopic.nodeID);
        if (node != null)
        {
            //data process package to send to node
            EffectDataProcess dataProcess = new EffectDataProcess() { outcome = effect.outcome };
            switch (effect.operand.name)
            {
                case "Add": dataProcess.value = effect.value; break;
                case "Subtract": dataProcess.value = effect.value * -1; break;
                default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
            }
            switch (effect.topicApply.name)
            {
                case "NodeCurrent":
                    //current, single, node
                    switch (effect.outcome.name)
                    {
                        case "NodeSecurity":
                            switch (effect.operand.name)
                            {
                                case "Add": node.ProcessNodeEffect(dataProcess); effectResolve.bottomText = string.Format("{0}District Security +1{1}", colourBad, colourEnd); break;
                                case "Subtract": node.ProcessNodeEffect(dataProcess); effectResolve.bottomText = string.Format("{0}District Security -1{1}", colourGood, colourEnd); break;
                                default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                            }
                            break;
                        case "NodeStability":
                            switch (effect.operand.name)
                            {
                                case "Add": node.ProcessNodeEffect(dataProcess); effectResolve.bottomText = string.Format("{0}District Stability +1{1}", colourBad, colourEnd); break;
                                case "Subtract": node.ProcessNodeEffect(dataProcess); effectResolve.bottomText = string.Format("{0}District Stability -1{1}", colourGood, colourEnd); break;
                                default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                            }
                            break;
                        case "NodeSupport":
                            switch (effect.operand.name)
                            {
                                case "Add": node.ProcessNodeEffect(dataProcess); effectResolve.bottomText = string.Format("{0}District Support +1{1}", colourGood, colourEnd); break;
                                case "Subtract": node.ProcessNodeEffect(dataProcess); effectResolve.bottomText = string.Format("{0}District Support -1{1}", colourBad, colourEnd); break;
                                default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                            }
                            break;
                        case "Tracer":
                            switch (effect.operand.name)
                            {
                                case "Subtract": node.RemoveTracer(); effectResolve.bottomText = string.Format("{0}Tracer Removed{1}", colourBad, colourEnd); break;
                                default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                            }
                            break;
                        default: Debug.LogWarningFormat("Unrecognised effect.outcome \"{0}\" for effect {1}", effect.outcome.name, effect.name); break;
                    }
                    break;
                case "NodeSameArc":
                    //all nodes of same Arc type
                    switch (effect.outcome.name)
                    {
                        case "NodeSecurity":
                            switch (effect.operand.name)
                            {
                                case "Add": effectResolve.bottomText = string.Format("{0}Similar Districts Security +1{1}", colourBad, colourEnd); break;
                                case "Subtract": node.ProcessNodeEffect(dataProcess); effectResolve.bottomText = string.Format("{0}Similar Districts Security -1{1}", colourGood, colourEnd); break;
                                default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                            }
                            break;
                        case "NodeStability":
                            switch (effect.operand.name)
                            {
                                case "Add": effectResolve.bottomText = string.Format("{0}Similar Districts Stability +1{1}", colourBad, colourEnd); break;
                                case "Subtract": effectResolve.bottomText = string.Format("{0}Similar Districts Stability -1{1}", colourGood, colourEnd); break;
                                default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                            }
                            break;
                        case "NodeSupport":
                            switch (effect.operand.name)
                            {
                                case "Add": effectResolve.bottomText = string.Format("{0}Similar Districts Support +1{1}", colourGood, colourEnd); break;
                                case "Subtract": effectResolve.bottomText = string.Format("{0}Similar Districts Support -1{1}", colourBad, colourEnd); break;
                                default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                            }
                            break;
                        default: Debug.LogWarningFormat("Unrecognised effect.outcome \"{0}\" for effect {1}", effect.outcome.name, effect.name); break;
                    }
                    //Process Node effect for all same ARC nodes
                    List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
                    if (listOfNodes != null)
                    {
                        foreach (var nodeTemp in listOfNodes)
                        {
                            //same node Arc type and not the current node?
                            if (nodeTemp.Arc.nodeArcID == node.Arc.nodeArcID && nodeTemp.nodeID != node.nodeID)
                            { nodeTemp.ProcessNodeEffect(dataProcess); }
                        }
                    }
                    else { Debug.LogError("Invalid listOfNodes (Null)"); }
                    break;
                default: Debug.LogWarningFormat("Unrecognised effect.topicApply \"{0}\" for effect {1}", effect.topicApply.name, effect.name); break;
            }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", dataTopic.nodeID); }
        return effectResolve;
    }

    /// <summary>
    /// private subMethod for ResolveTopicData that handles all topic HQ effects. Returns an EffectDataResolve data package in all cases (default data if a problem)
    /// NOTE: parent method has checked all parameters for null
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <param name="dataTopic"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveTopicHQEffect(Effect effect, EffectDataInput dataInput, TopicEffectData dataTopic)
    {
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //default data
        effectResolve.topText = "Unknown effect";
        effectResolve.bottomText = "Unknown effect";
        effectResolve.isError = false;
        //outcome
        switch (effect.outcome.name)
        {
            case "HQApproval":
                //adjusts by value amount
                switch (effect.operand.name)
                {
                    case "Add":
                        GameManager.i.hqScript.ChangeHqApproval(effect.value, GameManager.i.sideScript.PlayerSide, dataInput.originText);
                        effectResolve.bottomText = string.Format("{0}HQ Approval +{1}{2}", colourGood, effect.value, colourEnd);
                        GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopCentre, $"HQ Approval +{effect.value}");
                        break;
                    case "Subtract":
                        GameManager.i.hqScript.ChangeHqApproval(effect.value * -1, GameManager.i.sideScript.PlayerSide, dataInput.originText);
                        effectResolve.bottomText = string.Format("{0}HQ Approval -{1}{2}", colourBad, effect.value, colourEnd);
                        GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopCentre, $"HQ Approval -{effect.value}");
                        break;
                    default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                }
                break;
            case "HQRelocate":
                //HQ relocates (takes time, services unavailable during relocation)
                GameManager.i.hqScript.RelocateHq("Player reveals Location");
                effectResolve.bottomText = string.Format("{0}HQ forced to Relocate{1}{2}{3}Location Compromised{4}{5}", colourBad, colourEnd, "\n", colourAlert, colourEnd, "\n");
                GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopCentre, "HQ Relocates");
                break;
            default: Debug.LogWarningFormat("Unrecognised effect.outcome \"{0}\" for effect {1}", effect.outcome.name, effect.name); break;
        }
        return effectResolve;
    }

    /// <summary>
    /// private subMethod for ResolveTopicData that handles all Organisation effects. Returns an EffectDataResolve data package in all cases (default data if a problem)
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <param name="dataTopic"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveTopicOrganisationEffect(Effect effect, EffectDataInput dataInput, TopicEffectData dataTopic)
    {
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //default data
        effectResolve.topText = "Unknown effect";
        effectResolve.bottomText = "Unknown effect";
        effectResolve.isError = false;
        //Organisation
        Organisation org = GameManager.i.dataScript.GetOrganisaton(dataTopic.orgName);
        if (org != null)
        {
            //outcome
            switch (effect.outcome.name)
            {
                case "OrgFreedom":
                    switch (effect.operand.name)
                    {
                        case "Add":
                            org.ChangeFreedom(effect.value);
                            effectResolve.bottomText = string.Format("{0}{1} Freedom +{2}{3}", colourGood, org.tag, effect.value, colourEnd);
                            break;
                        case "Subtract":
                            org.ChangeFreedom(effect.value * -1);
                            effectResolve.bottomText = string.Format("{0}{1} Freedom -{2}{3}", colourBad, org.tag, effect.value, colourEnd);
                            break;
                        default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                    }
                    break;
                case "OrgReputation":
                    switch (effect.operand.name)
                    {
                        case "Add":
                            org.ChangeReputation(effect.value);
                            effectResolve.bottomText = string.Format("{0}{1} Reputation +{2}{3}", colourGood, org.tag, effect.value, colourEnd);
                            break;
                        case "Subtract":
                            org.ChangeReputation(effect.value * -1);
                            effectResolve.bottomText = string.Format("{0}{1} Reputation -{2}{3}", colourBad, org.tag, effect.value, colourEnd);
                            break;
                        default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                    }
                    break;
                case "OrgContact":
                    switch (effect.operand.name)
                    {
                        case "Add":
                            //Org makes contact with Player
                            org.isContact = true;
                            effectResolve.bottomText = string.Format("{0}{1} Contact made{2}", colourGood, org.tag, colourEnd);
                            Debug.LogFormat("[Org] EffectManager.cs -> ResolveTopicOrganisationEffect: {0} makes contact{1}", org.tag, "\n");
                            break;
                        case "Subtract":
                            //Org cuts player off from all contact
                            org.isContact = false;
                            org.isCutOff = true;
                            effectResolve.bottomText = string.Format("{0}The {1} will no longer deal with you{2}", colourBad, org.tag, colourEnd);
                            Debug.LogFormat("[Org] EffectManager.cs -> ResolveTopicOrganisationEffect: {0} ends all contact{1}", org.tag, "\n");
                            GameManager.i.dataScript.ResetOrgInfoArray(false);
                            break;
                        default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                    }
                    break;
                case "OrgSecretGain":
                    //Player gains org secret
                    if (org.secret != null)
                    {
                        if (GameManager.i.playerScript.AddSecret(org.secret) == true)
                        {
                            org.isSecretKnown = true;
                            effectResolve.bottomText = string.Format("{0}You gain the {1} Secret{2}", colourBad, org.secret.tag, colourEnd);
                        }
                    }
                    else { Debug.LogWarning("Invalid org.Secret (Null)"); }
                    break;
                case "OrgSecretReveal":
                    //Player's org secret revealed
                    if (org.secret != null)
                    { effectResolve.bottomText = ExecuteRevealOrgSecret(org.secret, org); }
                    else { Debug.LogWarning("Invalid org.Secret (Null)"); }
                    break;
                case "OrgTrackNemesis":
                    //OrgInfo provides a direct feed of Nemesis's location to Player
                    GameManager.i.dataScript.SetOrgInfoType(OrgInfoType.Nemesis, true);
                    GameManager.i.dataScript.StatisticIncrement(StatType.OrgInfoHacks);
                    GameManager.i.dataScript.AddOrgData(new OrgData() { text = "Nemesis", turn = GameManager.i.turnScript.Turn }, OrganisationType.Info);
                    effectResolve.bottomText = string.Format("{0}Nemesis will be tracked for {1}{2}{3}{4}{5} days{6}", colourGood, colourEnd, colourCancel, GameManager.i.orgScript.timerOrgInfoMax,
                        colourEnd, colourGood, colourEnd);
                    break;
                case "OrgTrackErasure":
                    //OrgInfo provides a direct feed of Erasure teams locations to Player
                    GameManager.i.dataScript.SetOrgInfoType(OrgInfoType.ErasureTeams, true);
                    GameManager.i.dataScript.StatisticIncrement(StatType.OrgInfoHacks);
                    GameManager.i.dataScript.AddOrgData(new OrgData() { text = "Erasure Teams", turn = GameManager.i.turnScript.Turn }, OrganisationType.Info);
                    effectResolve.bottomText = string.Format("{0}Erasure Teams will be tracked for {1}{2}{3}{4}{5} days{6}", colourGood, colourEnd, colourCancel, GameManager.i.orgScript.timerOrgInfoMax,
                        colourEnd, colourGood, colourEnd);
                    break;
                case "OrgTrackNpc":
                    //OrgInfo provides a direct feed of Npc's location to Player
                    GameManager.i.dataScript.SetOrgInfoType(OrgInfoType.Npc, true);
                    string npcName = "Unknown";
                    if (GameManager.i.missionScript.mission.npc != null)
                    { npcName = GameManager.i.missionScript.mission.npc.tag; }
                    GameManager.i.dataScript.StatisticIncrement(StatType.OrgInfoHacks);
                    GameManager.i.dataScript.AddOrgData(new OrgData() { text = npcName, turn = GameManager.i.turnScript.Turn }, OrganisationType.Info);
                    effectResolve.bottomText = string.Format("{0}{1} will be tracked for {2}{3}{4}{5}{6} days{7}", colourGood, npcName, colourEnd, colourCancel, GameManager.i.orgScript.timerOrgInfoMax,
                        colourEnd, colourGood, colourEnd);
                    break;
                default: Debug.LogWarningFormat("Unrecognised effect.outcome \"{0}\" for effect {1}", effect.outcome.name, effect.name); break;
            }
        }
        else
        {
            Debug.LogErrorFormat("Invalid organisation (Null) for orgName \"{0}\"", dataTopic.orgName);
            effectResolve.isError = true;
        }
        return effectResolve;
    }

    /// <summary>
    /// private subMethod for ResolveTopicData that handles all topic Target effects. Returns an EffectDataResolve data package in all cases (default data if a problem)
    /// NOTE: parent method has checked all parameters for null
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <param name="dataTopic"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveTopicTargetEffect(Effect effect, EffectDataInput dataInput, TopicEffectData dataTopic)
    {
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //default data
        effectResolve.topText = "Unknown effect";
        effectResolve.bottomText = "Unknown effect";
        effectResolve.isError = false;
        //outcome
        switch (effect.outcome.name)
        {
            case "TargetInfo":
                switch (effect.operand.name)
                {
                    case "Add":
                        //all live targets gain effect.value target info
                        if (GameManager.i.targetScript.ChangeTargetInfoAll(effect.value) == true)
                        { effectResolve.bottomText = string.Format("{0}All Target Info +1{1}", colourGood, colourEnd); }
                        break;
                    case "Subtract":
                        //all live targets lose effect.value target info
                        if (GameManager.i.targetScript.ChangeTargetInfoAll(effect.value, false) == true)
                        { effectResolve.bottomText = string.Format("{0}All Target Info -1{1}", colourBad, colourEnd); }
                        break;
                    default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                }
                break;
            default: Debug.LogWarningFormat("Unrecognised effect.outcome \"{0}\" for effect {1}", effect.outcome.name, effect.name); break;
        }
        return effectResolve;
    }


    /// <summary>
    /// private subMethod for ResolveTopicData that handles all topic City effects. Returns an EffectDataResolve data package in all cases (default data if a problem)
    /// NOTE: parent method has checked all parameters for null
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <param name="dataTopic"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveTopicCityEffect(Effect effect, EffectDataInput dataInput, TopicEffectData dataTopic)
    {
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //default data
        effectResolve.topText = "Unknown effect";
        effectResolve.bottomText = "Unknown effect";
        effectResolve.isError = false;
        int change = 0;
        //outcome
        switch (effect.outcome.name)
        {
            case "CityLoyalty":
                int cityLoyalty = GameManager.i.cityScript.CityLoyalty;
                switch (effect.operand.name)
                {
                    case "Add":
                        cityLoyalty += effect.value;
                        cityLoyalty = Mathf.Min(GameManager.i.cityScript.maxCityLoyalty, cityLoyalty);
                        GameManager.i.cityScript.CityLoyalty = cityLoyalty;
                        change = effect.value;
                        //good for authority, bad for resistance
                        if (GameManager.i.sideScript.PlayerSide.level == 1)
                        { effectResolve.bottomText = string.Format("{0}City Loyalty +{1}{2}", colourGood, effect.value, colourEnd); }
                        else { effectResolve.bottomText = string.Format("{0}City Loyalty +{1}{2}", colourBad, effect.value, colourEnd); }
                        GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopCentre, $"City Loyalty +{effect.value}");
                        break;
                    case "Subtract":
                        cityLoyalty -= effect.value;
                        cityLoyalty = Mathf.Max(0, cityLoyalty);
                        change = effect.value * -1;
                        GameManager.i.cityScript.CityLoyalty = cityLoyalty;
                        //bad for authority, good for resistance
                        if (GameManager.i.sideScript.PlayerSide.level == 1)
                        { effectResolve.bottomText = string.Format("{0}City Loyalty -{1}{2}", colourBad, effect.value, colourEnd); }
                        else { effectResolve.bottomText = string.Format("{0}City Loyalty -{1}{2}", colourGood, effect.value, colourEnd); }
                        GameManager.i.popUpFixedScript.SetData(PopUpPosition.TopCentre, $"City Loyalty -{effect.value}");
                        break;
                    default: Debug.LogWarningFormat("Unrecognised operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                }
                //message
                if (change != 0)
                { GameManager.i.messageScript.CityLoyalty(effectResolve.bottomText, dataInput.originText, cityLoyalty, change); }
                break;
            default: Debug.LogWarningFormat("Unrecognised effect.outcome \"{0}\" for effect {1}", effect.outcome.name, effect.name); break;
        }
        return effectResolve;
    }

    //
    // - - - SubMethods - - - 
    //

    /// <summary>
    /// handles player renown, returns string for EffectDataResolve.bottomText
    /// NOTE: effect checked for Null by parent method (child of several parents, ProcessEffect and ResolveTopicPlayerEffect)
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    private string ExecutePlayerRenown(Effect effect)
    {
        string bottomText = "Unknown";
        string popText = "Unknown";
        int playerRenown = GameManager.i.playerScript.Renown;
        //Player effect
        switch (effect.operand.name)
        {
            case "Add":
                playerRenown += effect.value;
                popText = $"Renown +{effect.value}";
                bottomText = string.Format("{0}Player {1}{2}", colourGoodSide, effect.description, colourEnd);
                break;
            case "Subtract":
                playerRenown -= effect.value;
                playerRenown = Mathf.Max(0, playerRenown);
                popText = $"Renown -{effect.value}";
                bottomText = string.Format("{0}Player {1}{2}", colourBadSide, effect.description, colourEnd);
                break;
            default: Debug.LogWarningFormat("Unrecognised effect.operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
        }
        GameManager.i.playerScript.Renown = playerRenown;
        //popUp
        GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, popText);
        return bottomText;
    }

    /// <summary>
    /// handles Actor renown, returns string for EffectDataResolve.bottomText
    /// NOTE: effect and actor checked for Null by parent method (child of several parents, ProcessEffect and ResolveTopicActorEffect)
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    private string ExecuteActorRenown(Effect effect, Actor actor, EffectDataInput dataInput, bool isHqActor = false)
    {
        string bottomText = "Unknown";
        string popText = "Unknown";
        int dataBefore = actor.Renown;
        int changeAmount = 0;
        switch (effect.operand.name)
        {
            case "Add":
                actor.Renown += effect.value;
                changeAmount += effect.value;
                if (actor.CheckTraitEffect(actorDoubleRenown) == true)
                {
                    //trait -> renown doubled (only for Add renown)
                    changeAmount += effect.value;
                    actor.Renown += effect.value;
                    bottomText = string.Format("{0}{1} Renown +{2}{3} ({4}{5}{6} trait)", isHqActor == false ? colourBad : colourGood,
                        isHqActor == false ? actor.arc.name : GameManager.i.hqScript.GetHqTitle(actor.statusHQ),
                        effect.value * 2, colourEnd, colourNeutral, actor.GetTrait().tag, colourEnd);
                    //logger
                    GameManager.i.actorScript.TraitLogMessage(actor, "for increasing Renown", "to gain DOUBLE renown");
                }
                else
                {
                    //no trait
                    bottomText = string.Format("{0}{1} {2}{3}", isHqActor == false ? colourBad : colourGood,
                        isHqActor == false ? actor.arc.name : GameManager.i.hqScript.GetHqTitle(actor.statusHQ),
                        effect.description, colourEnd);
                }
                popText = $"Renown +{changeAmount}";
                break;
            case "Subtract":
                actor.Renown -= effect.value;
                changeAmount -= effect.value;
                actor.Renown = Mathf.Max(0, actor.Renown);
                bottomText = string.Format("{0}{1} {2}{3}", isHqActor == false ? colourGood : colourBad,
                    isHqActor == false ? actor.arc.name : GameManager.i.hqScript.GetHqTitle(actor.statusHQ),
                    effect.description, colourEnd);
                popText = $"Renown {changeAmount}";
                break;
            default: Debug.LogWarningFormat("Unrecognised effect.operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
        }
        //hq actor
        if (isHqActor == true && changeAmount != 0)
        {
            HqRenownData dataRenown = new HqRenownData()
            {
                turn = GameManager.i.turnScript.Turn,
                change = changeAmount,
                newRenown = actor.Renown,
                reason = dataInput.originText
            };
            actor.AddHqRenownData(dataRenown);
        }
        else
        {
            //onMap actor -> popUp
            GameManager.i.popUpFixedScript.SetData(actor.slotID, popText);
        }
        //admin
        Debug.LogFormat("[Sta] -> EffectManager.cs: {0} {1} Renown changed from {2} to {3}{4}", actor.actorName, isHqActor == false ? actor.arc.name : GameManager.i.hqScript.GetHqTitle(actor.statusHQ),
            dataBefore, actor.Renown, "\n");
        return bottomText;
    }

    /// <summary>
    /// Gain or lose an actor contact (gain/lose based on operand Add/Subtract)
    /// NOTE: All parameters checked for Null by parent method
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="actor"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    private string ExecuteActorContact(Effect effect, Actor actor, TopicEffectData data, EffectDataInput dataInput)
    {
        string bottomText = "Unknown";
        if (data.nodeID > -1)
        {
            switch (effect.outcome.name)
            {
                case "ContactGainLose":
                    //Gain or lose a contact
                    switch (effect.operand.name)
                    {
                        case "Add":
                            int newNodeID = GameManager.i.contactScript.GetNewContactNodeID(actor);
                            //viable node found -> add contact
                            if (newNodeID > -1)
                            {
                                GameManager.i.dataScript.AddContactSingle(actor.actorID, newNodeID);
                                bottomText = string.Format("{0}{1} gains new Contact{2}", colourGoodSide, actor.arc.name, colourEnd);
                                GameManager.i.popUpFixedScript.SetData(actor.slotID, "New Contact");
                            }
                            break;
                        case "Subtract":
                            if (GameManager.i.dataScript.RemoveContactSingle(actor.actorID, data.nodeID) == true)
                            {
                                bottomText = string.Format("{0}{1} loses Contact{2}", colourBadSide, actor.arc.name, colourEnd);
                                GameManager.i.popUpFixedScript.SetData(actor.slotID, "Loses Contact");
                            }
                            else { Debug.LogWarningFormat("{0}, {1}, ID {2} unable to remove contact at nodeID {3}", actor.actorName, actor.arc.name, actor.actorID, data.nodeID); }
                            break;
                        default: Debug.LogWarningFormat("Unrecognised effect.operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                    }
                    break;
                case "ContactEffectiveness":
                    //Contact effectiveness changes to Max or Min
                    if (data.contactID > -1)
                    {
                        Contact contact = GameManager.i.dataScript.GetContact(data.contactID);
                        if (contact != null)
                        {
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    contact.effectiveness = GameManager.i.contactScript.maxEffectiveness;
                                    bottomText = string.Format("{0}Contact Effectiveness now MAX{1}", colourGood, colourEnd);
                                    Debug.LogFormat("[Cnt] EffectManager.cs -> ExecuteActorContact: {0} {1}, {2} at nodeID {3}, actorID {4}, effectiveness now MAX ({5}){6}", contact.nameFirst, contact.nameLast,
                                        contact.job, contact.nodeID, contact.actorID, contact.effectiveness, "\n");
                                    GameManager.i.popUpFixedScript.SetData(actor.slotID, "Contacts MAX");
                                    break;
                                case "Subtract":
                                    contact.effectiveness = GameManager.i.contactScript.minEffectiveness;
                                    bottomText = string.Format("{0}Contact Effectiveness now MIN{1}", colourBad, colourEnd);
                                    Debug.LogFormat("[Cnt] EffectManager.cs -> ExecuteActorContact: {0} {1}, {2} at nodeID {3}, actorID {4}, effectiveness now MIN ({5}){6}", contact.nameFirst, contact.nameLast,
                                        contact.job, contact.nodeID, contact.actorID, contact.effectiveness, "\n");
                                    GameManager.i.popUpFixedScript.SetData(actor.slotID, "Contacts MIN");
                                    break;
                                default: Debug.LogWarningFormat("Unrecognised effect.operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid contact (Null) for contactID \"{0}\"", data.contactID); }
                    }
                    else { Debug.LogWarningFormat("Invalid data.contactID \"{0}\"", data.contactID); }
                    break;
                case "ContactStatus":
                    //Contact status changed to Inactive
                    if (data.contactID > -1)
                    {
                        Contact contact = GameManager.i.dataScript.GetContact(data.contactID);
                        if (contact != null)
                        {
                            switch (effect.operand.name)
                            {
                                case "Subtract":
                                    contact.SetInactive(string.Format("Decision{0}{1}{2}{3}", "\n", colourNeutral, dataInput.originText, colourEnd));
                                    bottomText = string.Format("{0}Contact now Silent{1}", colourBad, colourEnd);
                                    GameManager.i.popUpFixedScript.SetData(actor.slotID, "Contact Silent");
                                    break;
                                default: Debug.LogWarningFormat("Unrecognised effect.operand \"{0}\" for effect {1}", effect.operand.name, effect.name); break;
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid contact (Null) for contactID \"{0}\"", data.contactID); }
                    }
                    else { Debug.LogWarningFormat("Invalid data.contactID \"{0}\"", data.contactID); }
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised effect.outcome \"{0}\" for effect {1}", effect.outcome.name, effect.name); break;
            }
        }
        else { Debug.LogWarningFormat("Invalid topicEffectData.nodeID \"{0}\"", data.nodeID); }
        return bottomText;
    }

    /// <summary>
    /// Current Actor gains or loses motivation.
    /// NOTE: Parameters checked for null by parent method
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    private string ExecuteActorMotivation(Effect effect, Actor actor, EffectDataInput dataInput, TopicEffectData dataTopic = null)
    {
        string bottomText = "Unknown";
        //only applies to OnMap actors, not HQ ones
        if (dataTopic == null || dataTopic.isHqActors == false)
        {
            bottomText = ProcessActorMotivation(actor, effect.value, effect.operand.name, dataInput.originText, effect.description);
            //relationship motivational shift in a friend or enemy
            RelationshipData data = GameManager.i.dataScript.GetRelationshipData(actor.slotID);
            if (data != null)
            {
                if (data.relationship != ActorRelationship.None)
                { bottomText = ExecuteActorRelationMotivation(actor, data, effect.operand.name, bottomText); }
            }
            else { Debug.LogWarningFormat("Invalid RelationshipData (Null) for {0}, {1}, ID {2}, slotID {3}", actor.actorName, actor.arc.name, actor.actorID, actor.slotID); }
            //update topBarUI conflict
            GameManager.i.topBarScript.UpdateConflicts(GameManager.i.actorScript.CheckNumOfConflictActors());
        }
        else
        {
            //HQ actor
            bottomText = ProcessActorMotivation(actor, effect.value, effect.operand.name, dataInput.originText, effect.description, dataTopic.isHqActors);
        }
        return bottomText;
    }


    /// <summary>
    /// handles motivation shifts for actor in the other end of a friend/enemy relationship
    /// Originating actor is the actor with relationship who sparked the motivational shift in the RelationshipData actor
    /// NOTE: originatingActor and data checked for Null by parent method, ExecuteActorMotivation
    /// </summary>
    /// <param name="data"></param>
    /// <param name="bottomText"></param>
    /// <returns></returns>
    private string ExecuteActorRelationMotivation(Actor originatingActor, RelationshipData data, string operandName, string bottomText)
    {
        string text = "";
        bool isGood = true;
        if (data.actorID > -1)
        {
            Actor actor = GameManager.i.dataScript.GetActor(data.actorID);
            if (actor != null)
            {
                //other actor must be active to be affected
                if (actor.Status == ActorStatus.Active)
                {
                    string description = "Unknown";
                    string newOperand = operandName;
                    switch (data.relationship)
                    {
                        case ActorRelationship.Friend:
                            switch (operandName)
                            {
                                case "Add": description = "Motivation +1"; isGood = false; break;
                                case "Subtract": description = "Motivation -1"; isGood = true; break;
                                default: Debug.LogWarningFormat("Unrecognised operandName \"{0}\"", operandName); break;
                            }
                            break;
                        case ActorRelationship.Enemy:
                            switch (operandName)
                            {
                                case "Add": description = "Motivation -1"; newOperand = "Subtract"; isGood = true; break;
                                case "Subtract": description = "Motivation +1"; newOperand = "Add"; isGood = false; break;
                                default: Debug.LogWarningFormat("Unrecognised operandName \"{0}\"", operandName); break;
                            }
                            break;
                        default: Debug.LogWarningFormat("Unrecognised relationship \"{0}\"", data.relationship); break;
                    }
                    //there is a chance of a motivational shift
                    int rnd = Random.Range(0, 100);
                    //successful roll
                    if (rnd < chanceMotivationShift)
                    {
                        //process motivation of other actor in the relationship
                        text = ProcessActorMotivation(actor, 1, newOperand, $"{data.relationship} relationship with {actor.arc.name}", description);
                        if (chanceMotivationShift < 100)
                        { Debug.LogFormat("[Rnd] EffectManager.cs -> ExecuteActorRelationMotivation: SUCCEEDED relation check, need < {0}, rolled {1}{2}", chanceMotivationShift, rnd, "\n"); }
                    }
                    else
                    {
                        //failed roll
                        if (chanceMotivationShift < 100)
                        { Debug.LogFormat("[Rnd] EffectManager.cs -> ExecuteActorRelationMotivation: FAILED relation check, need < {0}, rolled {1}{2}", chanceMotivationShift, rnd, "\n"); }
                    }
                    //random msg
                    if (chanceMotivationShift < 100)
                    {
                        string msgText = string.Format("{0} is {1} {2}", actor.arc.name, data.relationship == ActorRelationship.Friend ? "Friends with" : "an Enemy of", originatingActor.arc.name);
                        GameManager.i.messageScript.GeneralRandom(msgText, $"{data.relationship} Motivation", chanceMotivationShift, rnd, isGood, "rand_6");
                    }
                }
            }
            else { Debug.LogWarningFormat("Invalid actor (Null) for data.actorID {0}", data.actorID); }
        }
        else { Debug.LogWarningFormat("No valid RelationshipData.actorID \"{0}\"", data.actorID); }
        return string.Format("{0}{1}{2}", bottomText, text.Length > 0 ? "\n" : "", text);
    }


    /// <summary>
    /// subMethod for ExecuteActorMotivation/ExecuteActorRelationMotivation to handle motivation shift processing
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="amount"></param>
    /// <param name="operandName"></param>
    /// <param name="originText"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    private string ProcessActorMotivation(Actor actor, int amount, string operandName, string originText, string description, bool isHqActor = false)
    {
        string bottomText = "Unknown";
        int dataBefore = actor.GetDatapoint(ActorDatapoint.Motivation1);
        int motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
        switch (operandName)
        {
            case "Add":
                motivation += Mathf.Abs(amount);
                motivation = Mathf.Min(GameManager.i.actorScript.maxStatValue, motivation);
                actor.SetDatapoint(ActorDatapoint.Motivation1, motivation, originText);
                bottomText = string.Format("{0}{1} {2}{3}", colourGood, isHqActor == false ? actor.arc.name : GameManager.i.hqScript.GetHqTitle(actor.statusHQ), description, colourEnd);
                break;
            case "Subtract":
                motivation -= Mathf.Abs(amount);
                if (motivation < 0)
                {
                    if (isHqActor == false)
                    {
                        //relationship Conflict  (ActorConflict) -> Motivation change passes compatibility test
                        StringBuilder builder = new StringBuilder();
                        builder.AppendFormat("{0}{1}{2} Motivation too Low!{3}", "\n", colourAlert, actor.arc.name, colourEnd);
                        builder.AppendFormat("{0}{1}RELATIONSHIP CONFLICT{2}", "\n", colourBad, colourEnd);
                        builder.AppendFormat("{0}{1}{2}", "\n", "\n", GameManager.i.actorScript.ProcessActorConflict(actor));
                        motivation = Mathf.Max(0, motivation);
                        bottomText = builder.ToString();
                        GameManager.i.popUpFixedScript.SetData(actor.slotID, "CONFLICT!");
                    }
                    else
                    {
                        motivation = Mathf.Max(0, motivation);
                        bottomText = string.Format("{0}{1} {2}{3}", colourBad, isHqActor == false ? actor.arc.name : GameManager.i.hqScript.GetHqTitle(actor.statusHQ), description, colourEnd);

                    }
                }
                else
                { bottomText = string.Format("{0}{1} {2}{3}", colourBad, isHqActor == false ? actor.arc.name : GameManager.i.hqScript.GetHqTitle(actor.statusHQ), description, colourEnd); }
                actor.SetDatapoint(ActorDatapoint.Motivation1, motivation, originText);
                break;
            default: Debug.LogWarningFormat("Unrecognised operandName \"{0}\"", operandName); break;
        }
        //log entry
        if (motivation != dataBefore)
        {
            Debug.LogFormat("[Sta] -> EffectManger.cs: {0} {1} Motivation changed from {2} to {3}{4}", actor.actorName, isHqActor == false ? actor.arc.name : GameManager.i.hqScript.GetHqTitle(actor.statusHQ),
                dataBefore, motivation, "\n");
            if (isHqActor == false)
            { GameManager.i.popUpFixedScript.SetData(actor.slotID, string.Format("Motivation {0}{1}", string.Equals(operandName, "Add") == true ? "+" : "-", amount)); }
        }
        return bottomText;
    }


    /// <summary>
    /// Player gains or loses invisibility (variable amount)
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    private string ExecutePlayerInvisibility(Effect effect, EffectDataInput dataInput, Node node)
    {
        bool isGearUsed = false;
        string reason = dataInput.originText;
        string bottomText = "Unknown";
        string popText = "Unknown";
        switch (effect.operand.name)
        {
            case "Add":
                if (GameManager.i.playerScript.Invisibility < GameManager.i.actorScript.maxStatValue)
                {
                    //adds a variable amount
                    int invis = GameManager.i.playerScript.Invisibility;
                    invis += effect.value;
                    invis = Mathf.Min(GameManager.i.actorScript.maxStatValue, invis);
                    popText = $"Invisibility +{effect.value}";
                    GameManager.i.playerScript.Invisibility = invis;
                }
                bottomText = string.Format("{0}Player {1}{2}", colourGoodSide, effect.description, colourEnd);
                break;
            case "Subtract":
                //does player have any invisibility type gear?
                string gearName = GameManager.i.playerScript.CheckGearTypePresent(GameManager.i.gearScript.typeInvisibility);
                if (string.IsNullOrEmpty(gearName) == false)
                {
                    //gear present -> No drop in Invisibility
                    Gear gear = GameManager.i.dataScript.GetGear(gearName);
                    if (gear != null)
                    {
                        GameManager.i.gearScript.SetGearUsed(gear, "stay Invisible");
                        popText = "Stays Invisible";
                        bottomText = string.Format("{0}{1}{2}{3} used to remain Invisible{4}", colourNeutral, gear.tag.ToUpper(),
                            colourEnd, colourNormal, colourEnd);
                    }
                    else { Debug.LogError(string.Format("Invalid gear (Null) for gear {0}", gearName)); }
                }
                else
                {
                    //No gear present
                    int invisibility = GameManager.i.playerScript.Invisibility;
                    //condition TAGGED
                    if (GameManager.i.playerScript.CheckConditionPresent(conditionTagged, dataInput.side) == true)
                    {
                        //invisibility auto Zero
                        invisibility = 0;
                        //Immediate notification. AI flag set. Auto applies when TAGGED
                        bottomText =
                            string.Format("{0}Player Invisibility 0 (TAGGED){1}{2}{3}{4}<size=110%>Authority will know immediately</size>{5}",
                            colourAlert, colourEnd, "\n", "\n", colourBadSide, colourEnd);
                        reason = "TAGGED condition";
                        GameManager.i.aiScript.immediateFlagResistance = true;
                        popText = "POSITION KNOWN";
                        //Npc Invisible node
                        GameManager.i.missionScript.mission.npc.AddInvisibleNode(node.nodeID);
                    }
                    //NOT Tagged
                    else
                    {
                        //double effect if spider is present
                        if (node.isSpider == true)
                        {
                            invisibility -= 2;
                            popText = "Invisibility -2";
                            if (invisibility >= 0)
                            { bottomText = string.Format("{0}Player Invisibility -2 (Spider){1}", colourBadSide, colourEnd); }
                            else
                            {
                                //Immediate notification. AI flag set. Applies even if player invis was 1 before action (spider effect)
                                bottomText =
                                    string.Format("{0}Player Invisibility -2 (Spider){1}{2}{3}{4}<size=110%>Authority will know immediately</size>{5}",
                                    colourAlert, colourEnd, "\n", "\n", colourBadSide, colourEnd);
                                popText = string.Format("{0}{1}POSITION KNOWN", popText, "\n");
                                GameManager.i.aiScript.immediateFlagResistance = true;
                                //Npc stealth node
                                GameManager.i.missionScript.mission.npc.AddInvisibleNode(node.nodeID);
                            }
                        }
                        else
                        {
                            invisibility -= 1;
                            popText = "Invisibility -1";
                            if (invisibility >= 0)
                            { bottomText = string.Format("{0}Player {1}{2}", colourBadSide, effect.description, colourEnd); }
                            else
                            {
                                //immediate notification. AI flag set. Applies if player invis was 0 before action taken
                                bottomText = string.Format("{0}Player {1}{2}{3}{4}{5}<size=110%>Authority will know immediately</size>{6}",
                                    colourAlert, effect.description, colourEnd, "\n", "\n", colourBadSide, colourEnd);
                                popText = string.Format("{0}{1}POSITION KNOWN", popText, "\n");
                                GameManager.i.aiScript.immediateFlagResistance = true;
                                //Npc Invisible node
                                GameManager.i.missionScript.mission.npc.AddInvisibleNode(node.nodeID);
                            }

                        }
                    }
                    //mincap zero
                    invisibility = Mathf.Max(0, invisibility);
                    GameManager.i.playerScript.Invisibility = invisibility;
                }
                //AI activity message
                if (isGearUsed == false)
                {
                    int delay;
                    if (node.isSpider == true) { delay = delayYesSpider; }
                    else { delay = delayNoSpider; }
                    GameManager.i.messageScript.AINodeActivity(string.Format("Resistance Activity \"{0}\" (Player)",
                        dataInput.originText), node, GameManager.i.playerScript.actorID, delay);
                    //AI Immediate message
                    if (GameManager.i.aiScript.immediateFlagResistance == true)
                    {
                        GameManager.i.messageScript.AIImmediateActivity(string.Format("Immediate Activity \"{0}\" (Player)",
                            dataInput.originText), reason, node.nodeID, -1);
                    }
                }
                break;
        }
        //popUp
        GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, popText);
        return bottomText;
    }

    /// <summary>
    /// Actor gains or loses invisibility
    /// NOte: parameers checked for null by parent methods
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    private string ExecuteActorInvisibility(Effect effect, EffectDataInput dataInput, Actor actor, Node node)
    {
        string bottomText = "Unknown";
        string popText = "Unknown";
        int invisibility = actor.GetDatapoint(ActorDatapoint.Invisibility2);
        int dataBefore = actor.GetDatapoint(ActorDatapoint.Invisibility2);
        switch (effect.operand.name)
        {
            case "Add":
                if (invisibility < GameManager.i.actorScript.maxStatValue)
                {
                    //adds a variable amount
                    invisibility += effect.value;
                    invisibility = Mathf.Min(GameManager.i.actorScript.maxStatValue, invisibility);
                    popText = $"Invisibility +{effect.value}";
                    actor.SetDatapoint(ActorDatapoint.Invisibility2, invisibility);
                    Debug.LogFormat("[Sta] -> EffectManger.cs: {0} {1} Invisibility changed from {2} to {3}{4}", actor.actorName, actor.arc.name,
                        dataBefore, invisibility, "\n");
                }
                bottomText = string.Format("{0}{1} Invisibility +{2}{3}", colourGoodSide, actor.arc.name, effect.value, colourEnd);
                break;
            case "Subtract":
                //double effect if spider is present
                if (node.isSpider == true)
                {
                    int totalLoss = effect.value + 1;
                    popText = $"Invisibility -{totalLoss}";
                    totalLoss = Mathf.Min(3, totalLoss);
                    invisibility -= totalLoss;
                    if (invisibility >= 0)
                    { bottomText = string.Format("{0}{1} Invisibility -{2} (Spider){3}", colourBadSide, actor.arc.name, totalLoss, colourEnd); }
                    else
                    {
                        //immediate notification. AI flag set. Applies if actor invis was 1 (spider effect) or 0 before action taken
                        bottomText = string.Format("{0}{1} Invisibility -{2}{3}{4}<size=110%>Authority will know immediately</size>{5}{6}",
                            colourBadSide, actor.arc.name, effect.value, "\n", "\n", colourEnd, "\n");
                        popText = string.Format("{0}{1}POSITION KNOWN", popText, "\n");
                        GameManager.i.aiScript.immediateFlagResistance = true;
                    }
                }
                else
                {
                    invisibility -= effect.value;
                    popText = $"Invisibility -{effect.value}";
                    if (invisibility >= 0)
                    { bottomText = string.Format("{0}{1} Invisibility -{2}{3}", colourBadSide, actor.arc.name, effect.value, colourEnd); }
                    else
                    {
                        //immediate notification. AI flag set. Applies if actor invis was 0 before action taken
                        bottomText = string.Format("{0}{1} Invisibility -{2}{3}{4}<size=110%>Authority will know immediately</size>{5}{6}",
                            colourBadSide, actor.arc.name, effect.value, "\n", "\n", colourEnd, "\n");
                        popText = string.Format("{0}{1}POSITION KNOWN", popText, "\n");
                        GameManager.i.aiScript.immediateFlagResistance = true;
                    }
                }
                //mincap zero
                invisibility = Mathf.Max(0, invisibility);
                actor.SetDatapoint(ActorDatapoint.Invisibility2, invisibility);
                Debug.LogFormat("[Sta] -> EffectManger.cs: {0} {1} Invisibility changed from {2} to {3}{4}", actor.actorName, actor.arc.name,
                    dataBefore, invisibility, "\n");
                //AI activity message
                int delay;
                if (node.isSpider == true) { delay = delayYesSpider; }
                else { delay = delayNoSpider; }
                GameManager.i.messageScript.AINodeActivity(string.Format("Resistance Activity \"{0}\" ({1})",
                    dataInput.originText, actor.arc.name), node, actor.actorID, delay);
                //AI Immediate message
                if (GameManager.i.aiScript.immediateFlagResistance == true)
                {
                    GameManager.i.messageScript.AIImmediateActivity(string.Format("Immediate Activity \"{0}\" ({1})",
                        dataInput.originText, actor.arc.name), dataInput.originText, node.nodeID, -1, actor.actorID);
                }
                //Coward trait -> gets Stressed everytime they lose invisibility
                if (actor.CheckTraitEffect(actorStressedOverInvisibility) == true)
                {
                    Condition conditionStressed = GameManager.i.dataScript.GetCondition("STRESSED");
                    if (conditionStressed != null)
                    {
                        if (actor.CheckConditionPresent(conditionStressed) == false)
                        {
                            actor.AddCondition(conditionStressed, string.Format("Acquired due to {0} trait", actor.GetTrait().tag));
                            GameManager.i.actorScript.TraitLogMessage(actor, "for a Stress check", "to become STRESSED due to a loss of Invisibility");
                            StringBuilder builder = new StringBuilder();
                            builder.Append(bottomText);

                            /*builder.AppendFormat("{0}{1}{2}Gains {3}{4}STRESSED{5}{6} condition due to {7}{8}Coward{9}{10} trait{11}", "\n", "\n",
                                colourBadSide, colourEnd, colourAlert, colourEnd, colourBadSide, colourEnd, colourNeutral, colourEnd, colourBadSide, colourEnd);*/
                            builder.AppendLine(); builder.AppendLine();
                            builder.AppendFormat("{0}{1} gains {2}{3}STRESSED{4}{5} condition due to {6}{7}{8}{9}{10} trait{11}{12}", colourBad, actor.arc.name, colourEnd,
                                colourAlert, colourEnd, colourBad, colourEnd, colourAlert, actor.GetTrait().tag.ToUpper(), colourEnd, colourBad, colourEnd, "\n");
                            bottomText = builder.ToString();
                        }
                    }
                    else { Debug.LogWarning("Invalid condition STRESSED (Null)"); }
                }
                break;
        }
        GameManager.i.popUpFixedScript.SetData(actor.slotID, popText);
        return bottomText;
    }

    /// <summary>
    /// add or remove a condition to or from the Player
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    private string ExecutePlayerCondition(Effect effect, EffectDataInput dataInput/*, Node node*/)
    {
        string bottomText = "Unknown";
        //sort out colour based on type (which is effect benefit from POV of Resistance But is SAME for both sides when it comes to Conditions)
        string colourEffect = colourDefault;
        Condition condition = null;
        colourEffect = GetColourEffect(effect.typeOfEffect);
        //get condition
        condition = GetCondition(effect.outcome);
        //resolve effect outcome
        if (condition != null)
        {
            /*//assign condition to player if at their node, otherwise actor
            if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
            {*/

            //Player Condition
            switch (effect.operand.name)
            {
                case "Add":
                    //only add condition if NOT already present
                    if (GameManager.i.playerScript.CheckConditionPresent(condition, dataInput.side) == false)
                    {
                        GameManager.i.playerScript.AddCondition(condition, dataInput.side, string.Format("due to {0}", dataInput.originText));
                        bottomText = string.Format("{0}Player gains {1} condition{2}", colourEffect, condition.tag, colourEnd);
                        GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"Gains {condition.tag}");
                    }
                    else { bottomText = string.Format("{0}Player already has {1} condition{2}", colourNeutral, condition.tag, colourEnd); }
                    break;
                case "Subtract":
                    //only remove condition if present
                    if (GameManager.i.playerScript.CheckConditionPresent(condition, dataInput.side) == true)
                    {
                        GameManager.i.playerScript.RemoveCondition(condition, dataInput.side, string.Format("due to {0}", dataInput.originText));
                        bottomText = string.Format("{0}Player loses {1} condition{2}", colourEffect, condition.tag, colourEnd);
                        GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, $"Loses {condition.tag}");
                    }
                    else { bottomText = string.Format("{0}Player doesn't have {1} condition{2}", colourNeutral, condition.tag, colourEnd); }
                    break;
                default:
                    Debug.LogErrorFormat("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name);
                    break;
            }

            /*}*/
        }
        else { Debug.LogErrorFormat("Invalid condition (Null) for outcome \"{0}\"", effect.outcome.name); }
        return bottomText;
    }

    /// <summary>
    /// Activate or Deactivate a cure
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <returns></returns>
    private string ExecutePlayerCure(Effect effect, EffectDataInput dataInput, TopicEffectData dataTopic)
    {
        string bottomText = "Unknown";
        //sort out colour based on type (which is effect benefit from POV of Resistance But is SAME for both sides when it comes to Conditions)
        string colourEffect = colourDefault;
        Condition condition = null;
        colourEffect = GetColourEffect(effect.typeOfEffect);
        //get correct condition
        switch (effect.outcome.name)
        {
            case "CureAddicted": condition = conditionAddicted; break;
            case "CureWounded": condition = conditionWounded; break;
            case "CureTagged": condition = conditionTagged; break;
            case "CureImaged": condition = conditionImaged; break;
            case "CureDoomed": condition = conditionDoomed; break;
            case "CureQuestionable": condition = conditionQuestionable; break;
            default: Debug.LogWarningFormat("Unrecognised effect.outcome \"{0}\"", effect.outcome.name); break;
        }
        if (condition != null)
        {
            //who handled cure?
            bool isOrgCure = false;
            if (dataTopic.actorID < 0) { isOrgCure = true; }
            //Turn cure on / off
            switch (effect.operand.name)
            {
                case "Add":
                    //activate cure
                    if (GameManager.i.dataScript.SetCureNodeStatus(condition.cure, true, isOrgCure) == true)
                    {
                        GameManager.i.dataScript.StatisticIncrement(StatType.OrgCures);
                        bottomText = string.Format("{0}Cure for {1} available{2}", colourEffect, condition.tag, colourEnd);
                        GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Cure Available");
                        //org data
                        OrgData data = new OrgData()
                        {
                            text = condition.cure.cureName,
                            turn = GameManager.i.turnScript.Turn
                        };
                        GameManager.i.dataScript.AddOrgData(data, OrganisationType.Cure);
                    }
                    break;
                case "Subtract":
                    //deactivate cure
                    if (GameManager.i.dataScript.SetCureNodeStatus(condition.cure, false, isOrgCure) == true)
                    {
                        bottomText = string.Format("{0}Cure for {1} gone{2}", colourEffect, condition.tag, colourEnd);
                        GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Cure Gone");
                    }
                    break;
                default:
                    Debug.LogErrorFormat("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name);
                    break;
            }
        }
        else { Debug.LogErrorFormat("Invalid condition (Null) for effect.outcome \"{0}\"", effect.outcome.name); }
        return bottomText;
    }

    /// <summary>
    /// Player takes drugs (may, or may not, be Addicted). Removes stress and provides immunity
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <returns></returns>
    private string ExecutePlayerTakeDrugs(Effect effect)
    {
        GameManager.i.playerScript.TakeDrugs();
        GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Take Drugs");
        return string.Format("{0}You take {1}{2}STRESSED Condtion removed{3}You gain Stress Immunity{4}", colourGood, GameManager.i.globalScript.tagGlobalDrug, "\n", "\n", colourEnd);
    }

    /// <summary>
    /// Player loses a Random, or specific, piece of gear from inventory
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    private string ExecutePlayerLoseGear(Effect effect, TopicEffectData dataTopic)
    {
        string gearName = "Unknown";
        switch (effect.outcome.name)
        {
            case "Gear":
                //Player loses a random piece of Gear
                List<string> listOfGear = GameManager.i.playerScript.GetListOfGear();
                if (listOfGear != null)
                {
                    int count = listOfGear.Count;
                    if (count > 0)
                    {
                        gearName = listOfGear[Random.Range(0, count)];
                        if (string.IsNullOrEmpty(gearName) == false)
                        {
                            //remove gear (lost forever)
                            GameManager.i.playerScript.RemoveGear(gearName, true);
                            GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Gear Lost");
                        }
                        else
                        { Debug.LogWarning("Invalid gearName (Null or Empty)"); }
                    }
                    else { Debug.LogWarning("Invalid listOfGear (Empty)"); }
                }
                else
                { Debug.LogWarning("Invalid listOFGear (Null)"); }
                break;
            case "GearSpecific":
                //Player loses a specific piece of gear
                if (string.IsNullOrEmpty(dataTopic.gearName) == false)
                {
                    gearName = dataTopic.gearName;
                    //remove gear (lost forever)
                    GameManager.i.playerScript.RemoveGear(gearName, true);
                    GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Gear Lost");
                }
                else
                { Debug.LogWarning("Invalid dataTopic.gearName (Null or Empty)"); }
                break;
        }
        return string.Format("{0}Player loses {1} gear{2}", colourBad, gearName, colourEnd);
    }

    /// <summary>
    /// chance (L/M/H) of Player becoming addicted and gaining ADDICTED condition
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <returns></returns>
    private string ExecutePlayerChanceAddicted(Effect effect, EffectDataInput dataInput)
    {
        string bottomText = string.Format("{0}Player Not Addicted{1}", colourGood, colourEnd);
        int numNeeded = 0;
        int rnd = Random.Range(0, 100);
        switch (effect.outcome.name)
        {
            case "ChanceAddictedLow": numNeeded = 25; break;
            case "ChanceAddictedMed": numNeeded = 50; break;
            case "ChanceAddictedHigh": numNeeded = 75; break;
            default: Debug.LogWarningFormat("Unrecognised effect.outcome \"{0}\"", effect.outcome.name); break;
        }
        if (rnd < numNeeded)
        {
            //add Addicted Condition -> check not already present
            if (GameManager.i.playerScript.CheckConditionPresent(conditionAddicted, dataInput.side) == false)
            {
                GameManager.i.playerScript.AddCondition(conditionAddicted, dataInput.side, string.Format("Due to {0}", dataInput.originText));
                bottomText = string.Format("{0}Player becomes {1}{2}", colourBad, conditionAddicted.name, colourEnd);
                GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "ADDICTED");
            }
            //random message
            Debug.LogFormat("[Rnd] EffectManager.cs -> ExecutePlayerChanceAddicted: SUCEEDED need < {0}, rolled {1}{2}", numNeeded, rnd, "\n");
            GameManager.i.messageScript.GeneralRandom("Addiction Check SUCCEEDED", "Addiciton", numNeeded, rnd, true, "rand_3");
        }
        else
        {
            //random message
            Debug.LogFormat("[Rnd] EffectManager.cs -> ExecutePlayerChanceAddicted: FAILED need < {0}, rolled {1}{2}", numNeeded, rnd, "\n");
            GameManager.i.messageScript.GeneralRandom("Addiction Check FAILED", "Addiciton", numNeeded, rnd, true, "rand_3");
            GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Not Addicted");
        }

        //return
        return bottomText;
    }

    /// <summary>
    /// Player released from capture
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    private string ExecutePlayerRelease(Effect effect, EffectDataInput data)
    {
        GameManager.i.captureScript.ReleasePlayer(false);
        GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Released by Authority");
        return string.Format("{0}Player Released by the Authority{1}{2}", colourGood, colourEnd, "\n");
    }

    /// <summary>
    /// Player escapes from capture with help of OrgEmergency
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    private string ExecutePlayerEscape(Effect effect, EffectDataInput data)
    {
        //org data (do prior to escape) -> use name of node as place of incarceration from which the player escaped from
        string nodeName = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.nodeCaptured).nodeName;
        if (string.IsNullOrEmpty(nodeName) == false)
        {
            OrgData dataOrg = new OrgData()
            {
                text = nodeName,
                turn = GameManager.i.turnScript.Turn
            };
            GameManager.i.dataScript.AddOrgData(dataOrg, OrganisationType.Emergency);
        }
        else { Debug.LogWarningFormat("Invalid nodeName (Null or Empty) for nodeCaptured ID {0}", GameManager.i.nodeScript.nodeCaptured); }
        //escape
        GameManager.i.captureScript.ReleasePlayer(false, false);
        //stat
        GameManager.i.dataScript.StatisticIncrement(StatType.OrgEscapes);
        GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Escaped");
        return string.Format("{0}Player Escapes from Captivity{1}{2}", colourGood, colourEnd, "\n");
    }

    /// <summary>
    /// Reveals one of Player's secrets, eg. Org
    /// </summary>
    /// <param name="secret"></param>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <returns></returns>
    private string ExecuteRevealOrgSecret(Secret secret, Organisation org)
    {
        StringBuilder builder = new StringBuilder();
        if (secret != null)
        {
            if (org != null)
            {
                //check Player has secret (revealWho is the player 'cause they didn't meet org's demand)
                if (GameManager.i.playerScript.CheckSecretPresent(secret) == true)
                {
                    secret.revealedWho = org.tag;
                    secret.revealedWhen = GameManager.SetTimeStamp();
                    secret.status = gameAPI.SecretStatus.Revealed;
                    //carry out effects
                    if (secret.listOfEffects != null)
                    {
                        Debug.LogFormat("[Sec] EffectManager.cs -> ExecuteRevealSecret: secret \"{0}\" revealed by {1}{2}", secret.tag, org.tag, "\n");
                        //data packages
                        EffectDataReturn effectReturn = new EffectDataReturn();
                        EffectDataInput effectInput = new EffectDataInput();
                        effectInput.originText = "Org Reveals Secret";
                        effectInput.dataName = secret.org.name;
                        Node node = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
                        if (node != null)
                        {
                            if (secret.listOfEffects.Count > 0)
                            { builder.AppendFormat("{0}{1} Reveals Secret{2}", colourNormal, org.tag, colourEnd); }
                            //loop effects
                            foreach (Effect effect in secret.listOfEffects)
                            {
                                effectReturn = GameManager.i.effectScript.ProcessEffect(effect, node, effectInput);
                                if (builder.Length > 0) { builder.AppendLine(); }
                                builder.Append(effectReturn.bottomText);
                            }

                            //message
                            string text = string.Format("{0} revealed \"{1}\" secret{2}", org.tag, secret.tag, "\n");
                            GameManager.i.messageScript.OrganisationRevealSecret(text, org, secret, "You refused to cooperate");
                        }
                        else { Debug.LogWarning("Invalid player node (Null)"); }

                    }
                    //remove secret from all actors and player
                    GameManager.i.secretScript.RemoveSecretFromAll(secret.name);
                }
                else
                { Debug.LogWarningFormat("Invalid Org Secret {0} (Player doesn't have secret)"); builder.Append("Secret NOT FOUND"); }
            }
            else
            { Debug.LogWarning("Invalid org (Null)"); builder.Append("Invalid Organisation"); }
        }
        else { Debug.LogWarning("Invalid secret (Null)"); builder.Append("Invalid Secret"); }
        return builder.ToString();
    }

    /// <summary>
    /// add or remove a condition to or from an Actor
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    private string ExecuteActorCondition(Effect effect, EffectDataInput dataInput, Actor actor/*, Node node*/)
    {
        string bottomText = "Unknown";
        //sort out colour based on type (which is effect benefit from POV of Resistance But is SAME for both sides when it comes to Conditions)
        string colourEffect = colourDefault;
        Condition condition = null;
        colourEffect = GetColourEffect(effect.typeOfEffect);
        //get condition
        condition = GetCondition(effect.outcome);
        //resolve effect outcome
        if (condition != null)
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
                            actor.AddCondition(condition, string.Format("due to {0}", dataInput.originText));
                            bottomText = string.Format("{0}{1} condition gained{2}", colourEffect, condition.tag, colourEnd);
                            GameManager.i.popUpFixedScript.SetData(actor.slotID, $"Gains {condition.tag}");
                        }
                        break;
                    case "Subtract":
                        //only remove  condition if present
                        if (actor.CheckConditionPresent(condition) == true)
                        {
                            actor.RemoveCondition(condition, string.Format("due to {0}", dataInput.originText));
                            bottomText = string.Format("{0}{1} condition removed{2}", colourEffect, condition.tag, colourEnd);
                            GameManager.i.popUpFixedScript.SetData(actor.slotID, $"Loses {condition.tag}");
                        }
                        break;
                    default:
                        Debug.LogErrorFormat("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name);
                        break;
                }
            }
            else { Debug.LogWarning("Invalid actor (Null)"); }
        }
        else { Debug.LogWarningFormat("Invalid condition (Null) for outcome \"{0}\"", effect.outcome.name); }
        return bottomText;
    }

    /// <summary>
    /// add or remove a Capture Tool to the Player's inventory
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="dataInput"></param>
    /// <returns></returns>
    private string ExecuteCaptureTool(Effect effect, EffectDataInput dataInput)
    {
        string bottomText = "Unknown";
        string colourEffect = GetColourEffect(effect.typeOfEffect);
        int innocenceLevel = effect.value;
        CaptureTool tool = GameManager.i.captureScript.GetCaptureTool(effect.value);
        if (tool != null)
        {
            //Actor Condition
            switch (effect.operand.name)
            {
                case "Add":
                    //Add item to Player's inventory -> check if an appropriate level capture tool is even feasible (may not be present)
                    if (GameManager.i.captureScript.CheckIfCaptureToolPresent(innocenceLevel) == true)
                    {
                        //check if player already has this particular capture tool
                        if (GameManager.i.playerScript.CheckCaptureToolPresent(innocenceLevel) == false)
                        {
                            if (GameManager.i.playerScript.AddCaptureTool(innocenceLevel) == true)
                            {
                                bottomText = string.Format("{0}{1} gained{2}", colourEffect, tool.tag, colourEnd);
                                GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Gains Capture Tool");
                            }
                            else
                            {
                                bottomText = string.Format("{0}{1} NOT gained{2}", colourEffect, tool.tag, colourEnd);
                                Debug.LogWarningFormat("CaptureTool NOT added to player's inventory for effect.value \"{0}\"", innocenceLevel);
                            }
                        }
                        else
                        { bottomText = string.Format("{0}{1} already present{2}", colourEffect, tool.tag, colourEnd); }
                    }
                    else { Debug.LogWarningFormat("Capture tool not present for innocenceLevel \"{0}\"", innocenceLevel); }
                    break;
                case "Subtract":
                    //Remove item from Player's inventory
                    if (GameManager.i.playerScript.CheckCaptureToolPresent(innocenceLevel) == true)
                    {
                        if (GameManager.i.playerScript.RemoveCaptureTool(innocenceLevel) == true)
                        {
                            bottomText = string.Format("{0}{1} no longer effective{2}", colourEffect, tool.tag, colourEnd);
                            GameManager.i.popUpFixedScript.SetData(PopUpPosition.Player, "Loses Capture Tool");
                        }
                        else
                        {
                            Debug.LogWarningFormat("Capture Tool for innocenceLevel \"{0}\" Failed Removal", innocenceLevel);
                            bottomText = string.Format("{0}{1} no longer effective{2}", colourEffect, tool.tag, colourEnd);
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("Player does not have Capture Tool for innocenceLevel \"{0}\"", innocenceLevel);
                        bottomText = string.Format("{0}{1} no longer effective{2}", colourEffect, tool.tag, colourEnd);
                    }
                    break;
                default:
                    Debug.LogErrorFormat("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name);
                    break;
            }
        }
        else { Debug.LogWarningFormat("Invalid captureTool (Null) for effect.value \"{0}\"", effect.value); }
        return bottomText;
    }

    /// <summary>
    /// Player captured, identified as leader of Rebels in city and locked up permanently as a result. Campaign over.
    /// </summary>
    /// <returns></returns>
    private string ExecuteGameOver()
    {
        //fail state for Campaign
        string text = "Unknown";
        switch (GameManager.i.sideScript.PlayerSide.level)
        {
            case 1: text = GameManager.GetFormattedString("You have identified and incarcerated the leader of the Resistance in the City", ColourType.goodText); break;
            case 2: text = GameManager.GetFormattedString("You have been identified and incarcerated permanently", ColourType.badText); break;
            default: Debug.LogWarningFormat("Unrecognised playerSide {0}", GameManager.i.sideScript.PlayerSide.name); break;
        }
        GameManager.i.turnScript.SetWinStateCampaign(WinStateCampaign.Authority, WinReasonCampaign.Innocence, "Authority Locks up Rebel Leader", text);
        return "Player locked up permanently";
    }

    /// <summary>
    /// subMethod for topic condition methods to return a colour of effect (good/bad/neutral) string
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private string GetColourEffect(GlobalType type)
    {
        string colourEffect = colourDefault;
        if (type != null)
        {
            switch (type.name)
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
                    Debug.LogWarningFormat("Unrecognised effect.typeOfEffect (GlobalType) \"{0}\"", type.name);
                    break;
            }
        }
        else { Debug.LogWarning("Invalid GlobalType (Null)"); }
        return colourEffect;
    }

    /// <summary>
    /// subMethod to return a condition. Returns null if not found
    /// </summary>
    /// <param name="outcome"></param>
    /// <returns></returns>
    private Condition GetCondition(EffectOutcome outcome)
    {
        Condition condition = null;
        if (outcome != null)
        {
            //get condition
            switch (outcome.name)
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
                case "ConditionAddicted":
                    condition = conditionAddicted;
                    break;
                case "ConditionWounded":
                    condition = conditionWounded;
                    break;
                case "ConditionTagged":
                    condition = conditionTagged;
                    break;
                case "ConditionDoomed":
                    condition = conditionDoomed;
                    break;
                case "ConditionImaged":
                    condition = conditionImaged;
                    break;
                default:
                    Debug.LogWarningFormat("Unrecognised effect.outcome \"{0}\"", outcome.name);
                    break;
            }
        }
        else { Debug.LogWarning("Invalid effectOutcome (Null)"); }
        return condition;
    }

    //place methods above here
}
