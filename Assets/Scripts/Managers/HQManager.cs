﻿using gameAPI;
using packageAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// handles all HQ related matters for both sides
/// </summary>
public class HQManager : MonoBehaviour
{
    [Header("HQ Data")]
    [Tooltip("Approval for both sides HQ range from 0 to this amount")]
    [Range(0, 10)] public int maxHqApproval = 10;
    [Tooltip("How much Power will the HQ give per turn if they decide to support the Player")]
    [Range(1, 3)] public int powerPerTurn = 1;
    [Tooltip("How many actors in the HQ line up. Needs to correspond with enum.ActorHQ. Determines size of DataManager.cs -> arrayOfActorsHQl NOTE: ActorPoolManager.cs duplicate")]
    [Range(1, 6)] public int numOfActorsHQ = 4;
    [Tooltip("Size cap on HQ worker pool")]
    [Range(0, 10)] public int maxNumOfWorkers = 8;
    [Tooltip("Multiplier for HQ hierarchy Actors Power (Boss gets highest Power, subBoss's progressively less) forumla Power = (numOfActorsHQ + 2 - enum.StatusHQ) * powerFactor. NOTE: ActorPoolManager.cs duplicate")]
    [Range(1, 20)] public int powerFactor = 10;

    [Header("HQ Assessments (End Level page)")]
    [Tooltip("Weighting for HQ Boss (Power granted per Overall star)")]
    [Range(1, 5)] public int factorBoss = 4;
    [Tooltip("Weighting for HQ SubBoss1 (Power granted per Overall star)")]
    [Range(1, 5)] public int factorSubBoss1 = 3;
    [Tooltip("Weighting for HQ SubBoss2 (Power granted per Overall star)")]
    [Range(1, 5)] public int factorSubBoss2 = 2;
    [Tooltip("Weighting for HQ SubBoss3 (Power granted per Overall star)")]
    [Range(1, 5)] public int factorSubBoss3 = 1;
    [Tooltip("Weighting for first criteria (number of stars * weighting")]
    [Range(1, 5)] public int factorFirst = 3;
    [Tooltip("Weighting for second criteria (number of stars * weighting")]
    [Range(1, 5)] public int factorSecond = 2;
    [Tooltip("Weighting for third criteria (number of stars * weighting")]
    [Range(1, 5)] public int factorThird = 1;

    [Header("MetaGame")]
    [Tooltip("Chance of an HQ actor suffering a random event that effects their Power")]
    [Range(0, 100)] public int chanceOfRandomEvent = 25;
    [Tooltip("Chance of an HQ actor Major random event (eg. leave), if failed then a random event")]
    [Range(0, 100)] public int chanceOfMajorEvent = 25;
    [Tooltip("Max number of random events that can affect HQ actors - no more checks made after cap reached")]
    [Range(0, 10)] public int maxNumOfEvents = 4;
    [Tooltip("Base amount of Power gained or lost due to a Minor event")]
    [Range(0, 10)] public int basePowerChange = 6;
    [Tooltip("HQ hierarchy multiplier to basePowerChange")]
    [Range(0, 3)] public int hierarchyFactor = 2;

    [Header("Actor Influence")]
    [Tooltip("Amount HQ Approval drops by whenever an Actor resigns for whatever reason")]
    [Range(0, 3)] public int hQApprovalActorResigns = 1;

    [Header("HQ Matters")]
    [Tooltip("Timer set when HQ approval is first zero. Decrements each turn and when zero the Player is fired. Reset if approval rises above zero")]
    [Range(1, 10)] public int hQFirePlayerTimer = 3;

    [Header("HQ Relocation")]
    [Tooltip("Number of turns needed for HQ to successfully relocate (HQ services unavialable during relocation")]
    [Range(1, 10)] public int timerRelocationBase = 5;

    [Header("Text Lists")]
    [Tooltip("Major event reasons for an HQ actor to Leave")]
    public TextList hQMajorEvent;
    [Tooltip("Minor event (good) reasons for an HQ Hierarchy actor to gain Power")]
    public TextList hQMinorEventHierarchyGood;
    [Tooltip("Minor event (bad) reasons for an HQ Hierarchy actor to lose Power")]
    public TextList hQMinorEventHierarchyBad;
    [Tooltip("Minor event (good) reasons for an HQ Worker to gain Power")]
    public TextList hQMinorEventWorkerGood;
    [Tooltip("Minor event (bad) reasons for an HQ Worker to lose Power")]
    public TextList hQMinorEventWorkerBad;

    [Header("HQ Positions -> Authority")]
    [Tooltip("HQ Position that corresponds to the enum.ActorHQ equivalent. Transferred over to DataManager.cs if playerSide")]
    public HqPosition bossAut;
    public HqPosition subBoss1Aut;
    public HqPosition subBoss2Aut;
    public HqPosition subBoss3Aut;

    [Header("HQ Positions -> Resistance")]
    [Tooltip("HQ Position that corresponds to the enum.ActorHQ equivalent. Transferred over to DataManager.cs if playerSide")]
    public HqPosition bossRes;
    public HqPosition subBoss1Res;
    public HqPosition subBoss2Res;
    public HqPosition subBoss3Res;

    [HideInInspector] public Hq hQAuthority;
    [HideInInspector] public Hq hQResistance;



    #region SaveDataCompatible
    private int bossOpinion;                                //opinion of HQ boss (changes depending on topic decisions made, if in alignment with Boss's view or not)
    private int _approvalAuthority;                         //level of HQ approval (out of 10) enjoyed by authority side (Player/AI)
    private int _approvalResistance;                        //level of HQ approval (out of 10) enjoyed by resistance side (Player/AI)
    private int approvalZeroTimer;                          //countdown timer once approval at zero. Player fired when timer reaches zero.
    private int timerHqRelocating;                          //activated when relocation occurs, counts down to zero when relocation is considered done
    [HideInInspector] public bool isHqRelocating;           //true if HQ relocating. Disallows HQ support + Gear/Recruit/Lie Low actions
    #endregion

    private bool isZeroTimerThisTurn;                       //only the first zero timer event per turn is processed
    private bool isMetaLogs;                                //toggles all MetaGame related [Tst] logs via TestManager.cs setting

    //fast access
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;



    private string colourRebel;
    private string colourAuthority;
    private string colourNeutral;
    private string colourNormal;
    private string colourGood;
    private string colourBad;
    /*private string colourGrey;*/
    private string colourAlert;
    /*private string colourSide;*/
    private string colourEnd;

    /// <summary>
    /// use ChangeHqApproval to set
    /// </summary>
    public int ApprovalAuthority
    {
        get { return _approvalAuthority; }
        private set
        {
            int previous = _approvalAuthority;
            _approvalAuthority = value;
            _approvalAuthority = Mathf.Clamp(_approvalAuthority, 0, maxHqApproval);
            Debug.LogFormat("[HQ] HqManager.cs: HQ Approval Authority now {0}, was {1}{2}", _approvalAuthority, previous, "\n");
            //update top widget bar if current side is authority
            if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideAuthority.level)
            { EventManager.i.PostNotification(EventType.ChangeHqBar, this, _approvalAuthority, "HqManager.cs -> ApprovalAuthority"); }
        }
    }

    /// <summary>
    /// use ChangeHqApproval to set
    /// </summary>
    public int ApprovalResistance
    {
        get { return _approvalResistance; }
        private set
        {
            int previous = _approvalResistance;
            _approvalResistance = value;
            _approvalResistance = Mathf.Clamp(_approvalResistance, 0, maxHqApproval);
            Debug.LogFormat("[HQ] HqManager.cs: HQ Approval Resistance now {0}, was {1}{2}", _approvalResistance, previous, "\n");
            //update top widget bar if current side is resistance
            if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideResistance.level)
            { EventManager.i.PostNotification(EventType.ChangeHqBar, this, _approvalResistance, "HqManager.cs -> ApprovalResistance"); }
        }
    }

    /// <summary>
    /// Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                SubInitialiseNewGame();
                SubInitialiseFastAccess();
                SubInitialiseColours();
                SubInitialiseEvents();
                SubInitialiseAll();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseAll();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseColours();
                SubInitialiseEvents();
                SubInitialiseAll();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseNewGame
    private void SubInitialiseNewGame()
    {
        //initialise HQ actors starting lineUp
        if (GameManager.i.optionScript.isActorPool == false)
        {
            GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
            GameManager.i.actorScript.InitialiseHqActors(playerSide);
            //add workers (possible testManager.cs override)
            if (GameManager.i.testScript.numOfWorkers > -1)
            { GameManager.i.actorScript.InitialiseHqWorkers(GameManager.i.testScript.numOfWorkers, playerSide); }
            else { GameManager.i.actorScript.InitialiseHqWorkers(numOfActorsHQ, playerSide); }

        }
        //assign compatibility with player and descriptors
        GameManager.i.personScript.SetHqActorsCompatibility();
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access
        globalAuthority = GameManager.i.globalScript.sideAuthority;
        globalResistance = GameManager.i.globalScript.sideResistance;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
    }
    #endregion

    #region SubInitialiseColours
    private void SubInitialiseColours()
    { SetColours(); }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "HQManager");
        EventManager.i.AddListener(EventType.StartTurnEarly, OnEvent, "HQManager");
        EventManager.i.AddListener(EventType.EndTurnLate, OnEvent, "HQManager");
    }
    #endregion

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        //Authority HQ 
        hQAuthority = GameManager.i.dataScript.GetHq(GameManager.i.globalScript.sideAuthority);
        if (hQAuthority == null)
        { Debug.LogError("Invalid HqAuthority (Null)"); }
        //Resistance HQ
        hQResistance = GameManager.i.dataScript.GetHq(GameManager.i.globalScript.sideResistance);
        if (hQResistance == null)
        { Debug.LogError("Invalid HqResistance (Null)"); }
        //approval levels (if input approval is Zero then generate a random value between 2 & 8)
        int approval = GameManager.i.scenarioScript.scenario.approvalStartAuthorityHQ;
        if (approval == 0) { approval = Random.Range(2, 9); }
        ApprovalAuthority = approval;
        approval = GameManager.i.scenarioScript.scenario.approvalStartRebelHQ;
        if (approval == 0)
        { approval = Random.Range(2, 9); }
        ApprovalResistance = approval;
        Debug.LogFormat("[HQ] HQManager -> Initialise: {0}, approval {1}, {2}, approval {3}{4}",
            hQResistance, ApprovalResistance, hQAuthority, ApprovalAuthority, "\n");
        //text lists
        Debug.Assert(hQMajorEvent != null, "Invalid hqMajorEvent (Null)");
        Debug.Assert(hQMinorEventHierarchyGood != null, "Invalid hqMinorEventHierarchyGood (Null)");
        Debug.Assert(hQMinorEventHierarchyBad != null, "Invalid hqMinorEventHierarchyBad (Null)");
        Debug.Assert(hQMinorEventWorkerGood != null, "Invalid hqMinorEventWorkerGood (Null)");
        Debug.Assert(hQMinorEventWorkerBad != null, "Invalid hqMinorEventWorkerBad (Null)");
        //hq titles
        Debug.Assert(bossAut != null, "Invalid bossAut (Null)");
        Debug.Assert(subBoss1Aut != null, "Invalid subBoss1Aut (Null)");
        Debug.Assert(subBoss2Aut != null, "Invalid subBoss2Aut (Null)");
        Debug.Assert(subBoss3Aut != null, "Invalid subBoss3Aut (Null)");
        Debug.Assert(bossRes != null, "Invalid bossRes (Null)");
        Debug.Assert(subBoss1Res != null, "Invalid subBoss1Res (Null)");
        Debug.Assert(subBoss2Res != null, "Invalid subBoss2Res (Null)");
        Debug.Assert(subBoss3Res != null, "Invalid subBoss3Res (Null)");
        //logging
        isMetaLogs = GameManager.i.testScript.isMetaGame;
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
            case EventType.EndTurnLate:
                EndTurnLate();
                break;
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.StartTurnEarly:
                CheckHQPowerSupport();
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
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourAuthority = GameManager.i.colourScript.GetColour(ColourType.badText);
        colourRebel = GameManager.i.colourScript.GetColour(ColourType.blueText);
        /*colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);*/
        colourGood = GameManager.i.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.dataBad);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
        /*if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; }
        else { colourSide = colourRebel; }*/
    }

    /// <summary>
    /// End turn late event
    /// </summary>
    private void EndTurnLate()
    {
        //reset flag ready for next turn
        isZeroTimerThisTurn = false;
        //HQ relocating
        if (timerHqRelocating > 0)
        {
            string text;
            timerHqRelocating--;
            if (timerHqRelocating <= 0)
            {
                //relocation complete
                isHqRelocating = false;
                timerHqRelocating = 0;
                text = "HQ has successfully Relocated";
                GameManager.i.messageScript.HqRelocates(text, "Relocation Successful");
                Debug.LogFormat("[HQ] HqManager.cs -> EndTurnLate: HQ has successfully Relocated{0}", "\n");
            }
            else
            {
                //relocation ongoing - effects tab
                text = string.Format("<b>HQ is relocating ({0}{1}{2} turn{3} to go</b>)", colourNeutral, timerHqRelocating, colourEnd, timerHqRelocating != 1 ? "s" : "");
                ActiveEffectData dataEffect = new ActiveEffectData()
                {
                    text = "HQ is Relocating",
                    topText = "Relocation Underway",
                    detailsTop = text,
                    detailsBottom = string.Format("{0}<b>HQ Services are temporarily Unavailable</b>{1}", colourBad, colourEnd),
                    sprite = GetCurrentHQ().sprite,
                    help0 = "hq_0",
                    help1 = "hq_1",
                    help2 = "hq_2"
                };
                GameManager.i.messageScript.ActiveEffect(dataEffect);
                Debug.LogFormat("[HQ] HqManager.cs -> EndTurnLate: {0}{1}", text, "\n");
            }
        }
    }

    /// <summary>
    /// checks if player given support (+1 Power) from their HQ based on a random roll vs. level of HQ approval. No support if player inactive.
    /// </summary>
    private void CheckHQPowerSupport()
    {
        bool isProceed = true;
        //ignore if autorun with both sides AI
        if (GameManager.i.turnScript.CheckIsAutoRun() == true && GameManager.i.autoRunTurns > 0)
        { isProceed = false; }
        //ignore if player is inactive
        if (GameManager.i.playerScript.Status == ActorStatus.Inactive)
        {
            isProceed = false;
            Debug.LogFormat("[HQ] HqManager.cs -> CheckHQPowerSupport: NO support as Player is Inactive ({0}){1}", GameManager.i.playerScript.InactiveStatus, "\n");
        }
        //HQ relocating
        if (isHqRelocating == true)
        {
            isProceed = false;
            Hq hqFaction = GetCurrentHQ();
            if (hqFaction != null)
            {
                Debug.LogFormat("[HQ] HqManager.cs -> CheckHQPowerSupport: NO support as HQ Relocating{0}", "\n");
                string text = string.Format("HQ support unavailable as HQ is currently Relocating{0}", "\n");
                string reason = GameManager.Formatt(string.Format("<b>{0} is currently Relocating</b>", hqFaction.tag), ColourType.salmonText);
                GameManager.i.messageScript.HqSupportUnavailable(text, reason, hqFaction);
            }
            else { Debug.LogWarning("Invalid current HQ (Null)"); }
        }
        if (isProceed == true)
        {
            int side = GameManager.i.sideScript.PlayerSide.level;
            if (side > 0)
            {
                int rnd = Random.Range(0, 100);
                int threshold;
                switch (side)
                {
                    case 1:
                        //Authority
                        threshold = _approvalAuthority * 10;
                        if (rnd < threshold)
                        {
                            //Support Provided
                            Debug.LogFormat("[Rnd] HqManager.cs -> CheckHQPowerSupport: GIVEN need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                            string msgText = string.Format("{0} provides SUPPORT (+1 Power)", hQAuthority.tag);
                            GameManager.i.messageScript.HqSupport(msgText, hQAuthority, _approvalAuthority, GameManager.i.playerScript.Power, powerPerTurn);
                            //random
                            GameManager.i.messageScript.GeneralRandom("HQ support GIVEN", "HQ Support", threshold, rnd);
                            //Support given
                            GameManager.i.playerScript.Power += powerPerTurn;
                        }
                        else
                        {
                            //Support declined
                            Debug.LogFormat("[Rnd] HqManager.cs -> CheckHQPowerSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                            string msgText = string.Format("{0} declines support ({1} % chance of support)", hQAuthority.tag, threshold);
                            GameManager.i.messageScript.HqSupport(msgText, hQAuthority, _approvalAuthority, GameManager.i.playerScript.Power);
                            //random
                            GameManager.i.messageScript.GeneralRandom("HQ support DECLINED", "HQ Support", threshold, rnd);
                        }
                        break;
                    case 2:
                        //Resistance
                        threshold = _approvalResistance * 10;
                        if (rnd < threshold)
                        {
                            //Support Provided
                            Debug.LogFormat("[Rnd] HqManager.cs -> CheckHQPowerSupport: GIVEN need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                            string msgText = string.Format("{0} provides SUPPORT (+1 Power)", hQResistance.tag);
                            GameManager.i.messageScript.HqSupport(msgText, hQResistance, _approvalResistance, GameManager.i.playerScript.Power, powerPerTurn);
                            //random
                            GameManager.i.messageScript.GeneralRandom("HQ support GIVEN", "HQ Support", threshold, rnd, false, "rand_1");
                            //Support given
                            GameManager.i.playerScript.Power += powerPerTurn;
                        }
                        else
                        {
                            //Support declined
                            Debug.LogFormat("[Rnd] HqManager.cs -> CheckHQPowerSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                            string msgText = string.Format("{0} declines support ({1} % chance of support)", hQResistance.tag, threshold);
                            GameManager.i.messageScript.HqSupport(msgText, hQResistance, _approvalResistance, GameManager.i.playerScript.Power);
                            //random
                            GameManager.i.messageScript.GeneralRandom("HQ support DECLINED", "HQ Support", threshold, rnd, false, "rand_1");
                        }
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid side \"{0}\"", side);
                        break;
                }
            }
        }
    }

    public int GetApprovalZeroTimer()
    { return approvalZeroTimer; }

    /// <summary>
    /// Used by Save/Load to set data
    /// </summary>
    /// <param name="timer"></param>
    public void LoadApprovalZeroTimer(int timer)
    { approvalZeroTimer = timer; }

    /// <summary>
    /// Checks if approval zero, sets timer, counts down each turn and fires player when timer expired. Timer cancelled if approval rises
    /// </summary>
    public void CheckHqFirePlayer()
    {
        string msgText, itemText, reason, warning;
        //get approval
        int approval = -1;
        GlobalSide side = GameManager.i.sideScript.PlayerSide;
        Hq playerHQ = null;
        switch (side.level)
        {
            case 1:
                //Authority
                approval = _approvalAuthority;
                playerHQ = hQAuthority;
                break;
            case 2:
                //Resistance
                approval = _approvalResistance;
                playerHQ = hQResistance;
                break;
            default:
                Debug.LogWarningFormat("Invalid side \"{0}\"", side);
                break;
        }
        Debug.Assert(playerHQ != null, "Invalid playerHQ (Null)");
        //only check once per turn
        if (approval == 0 && isZeroTimerThisTurn == false)
        {
            isZeroTimerThisTurn = true;
            if (approvalZeroTimer == 0)
            {
                //set timer
                approvalZeroTimer = hQFirePlayerTimer;
                //message
                msgText = string.Format("HQ approval Zero. HQ will FIRE you in {0} turn{1}", approvalZeroTimer, approvalZeroTimer != 1 ? "s" : "");
                itemText = string.Format("{0} HQ approval at ZERO", playerHQ.name);
                reason = string.Format("{0} HQ have lost faith in you", playerHQ.name);
                warning = string.Format("You will be FIRED in {0} turn{1}", approvalZeroTimer, approvalZeroTimer != 1 ? "s" : "");
                GameManager.i.messageScript.GeneralWarning(msgText, itemText, "HQ Approval", reason, warning);
            }
            else
            {
                //decrement timer
                approvalZeroTimer--;
                //fire player at zero
                if (approvalZeroTimer == 0)
                {
                    WinStateLevel winState = WinStateLevel.Authority;
                    //you lost, opposite side won
                    if (GameManager.i.sideScript.PlayerSide.level == GameManager.i.globalScript.sideAuthority.level)
                    {
                        //Resistance side wins
                        winState = WinStateLevel.Resistance;
                    }
                    msgText = string.Format("{0} approval Zero. Player Fired. Authority wins", playerHQ.tag);
                    itemText = string.Format("{0} has LOST PATIENCE", playerHQ.tag);
                    reason = string.Format("{0} approval at ZERO for an extended period", playerHQ.tag);
                    warning = "You've been FIRED, game over";
                    GameManager.i.messageScript.GeneralWarning(msgText, itemText, "You're FIRED", reason, warning);
                    //Player fired -> outcome
                    string textTop = string.Format("{0}The {1} has lost faith in your abilities{2}", colourNormal, GetHqName(side), colourEnd);
                    string textBottom = string.Format("{0}You've been FIRED{1}", colourBad, colourEnd);
                    GameManager.i.turnScript.SetWinStateLevel(winState, WinReasonLevel.HqSupportMin, textTop, textBottom);

                }
                else
                {
                    //message
                    msgText = string.Format("HQ approval Zero. HQ will FIRE you in {0} turn{1}", approvalZeroTimer, approvalZeroTimer != 1 ? "s" : "");
                    itemText = string.Format("{0} about to FIRE you", playerHQ.tag);
                    reason = string.Format("{0} are displeased with your performance", playerHQ.tag);
                    warning = string.Format("You will be FIRED in {0} turn{1}", approvalZeroTimer, approvalZeroTimer != 1 ? "s" : "");
                    GameManager.i.messageScript.GeneralWarning(msgText, itemText, "HQ Unhappy", reason, warning);
                }
            }
        }
        else
        {
            //timer set to default (approval > 0)
            approvalZeroTimer = 0;
        }
    }


    /// <summary>
    /// returns official HQ title (string, eg. 'Treasurer') for specified enum and Playerside. Returns 'Unknown' if a problem
    /// </summary>
    /// <param name="hqPosition"></param>
    /// <returns></returns>
    public string GetHqTitle(ActorHQ hqPosition)
    {
        string title = "Unknown";
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        switch (playerSide.level)
        {
            case 1:
                //authority
                switch (hqPosition)
                {
                    case ActorHQ.Boss: title = bossAut.tag; break;
                    case ActorHQ.SubBoss1: title = subBoss1Aut.tag; break;
                    case ActorHQ.SubBoss2: title = subBoss2Aut.tag; break;
                    case ActorHQ.SubBoss3: title = subBoss3Aut.tag; break;
                    case ActorHQ.Worker: title = "Worker"; break;
                    case ActorHQ.LeftHQ: title = "Left HQ"; break;
                    default: Debug.LogWarningFormat("Unrecognised Authority hqPosition \"{0}\"", hqPosition); break;
                }
                break;
            case 2:
                //resistance
                switch (hqPosition)
                {
                    case ActorHQ.Boss: title = bossRes.tag; break;
                    case ActorHQ.SubBoss1: title = subBoss1Res.tag; break;
                    case ActorHQ.SubBoss2: title = subBoss2Res.tag; break;
                    case ActorHQ.SubBoss3: title = subBoss3Res.tag; break;
                    case ActorHQ.Worker: title = "Worker"; break;
                    case ActorHQ.LeftHQ: title = "Left HQ"; break;
                    default: Debug.LogWarningFormat("Unrecognised Resistance hqPosition \"{0}\"", hqPosition); break;
                }
                break;
            default: Debug.LogWarningFormat("Unrecognised playerSide.level {0}", playerSide.level); break;
        }
        return title;
    }


    /// <summary>
    /// returns current HQ for player side, Null if not found
    /// </summary>
    /// <returns></returns>
    public Hq GetCurrentHQ()
    {
        Hq factionHQ = null;
        switch (GameManager.i.sideScript.PlayerSide.name)
        {
            case "Authority":
                factionHQ = hQAuthority;
                break;
            case "Resistance":
                factionHQ = hQResistance;
                break;
            default:
                Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.i.sideScript.PlayerSide.name));
                break;
        }
        return factionHQ;
    }

    /// <summary>
    /// returns current HQ for the specified side in a colour formatted string. 'isOversized' gives name at 115% (default), false for normal.
    /// </summary>
    /// <returns></returns>
    public string GetHqName(GlobalSide side, bool isOversized = true)
    {
        string description = "Unknown";
        if (side != null)
        {
            if (isOversized == true)
            {
                //oversized text
                switch (side.level)
                {
                    case 1:
                        description = string.Format("<b><size=115%>{0}{1}{2}</size></b>", colourAuthority, hQAuthority.tag, colourEnd);
                        break;
                    case 2:
                        description = string.Format("<b><size=115%>{0}{1}{2}</size></b>", colourRebel, hQResistance.tag, colourEnd);
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.i.sideScript.PlayerSide.name));
                        break;
                }
            }
            else
            {
                //normal sized text
                switch (side.level)
                {
                    case 1:
                        description = string.Format("<b>{0}</b>", hQAuthority.name);
                        break;
                    case 2:
                        description = string.Format("<b>{0}</b>", hQResistance.name);
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.i.sideScript.PlayerSide.name));
                        break;
                }
            }
        }
        else { Debug.LogWarning("Invalid side (Null)"); }
        return description;
    }


    /// <summary>
    /// returns current HQ description for specified side in colour formatted string
    /// </summary>
    /// <returns></returns>
    public string GetHqDescription(GlobalSide side)
    {
        string description = "Unknown";
        if (side != null)
        {
            switch (side.level)
            {
                case 1:
                    description = string.Format("{0}{1}{2}", colourNormal, hQAuthority.descriptor, colourEnd);
                    break;
                case 2:
                    description = string.Format("{0}{1}{2}", colourNormal, hQResistance.descriptor, colourEnd);
                    break;
                default:
                    Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.i.sideScript.PlayerSide.name));
                    break;
            }
        }
        else { Debug.LogWarning("Invalid side (Null)"); }
        return description;
    }

    /// <summary>
    /// returns HQ approval level for current side, returns -1 if a problem
    /// </summary>
    /// <returns></returns>
    public int GetHqApproval()
    {
        int approval = -1;
        switch (GameManager.i.sideScript.PlayerSide.level)
        {
            case 1: approval = _approvalAuthority; break;
            case 2: approval = _approvalResistance; break;
            default: Debug.LogWarningFormat("Unrecognised playerSide.level \"{0}\"", GameManager.i.sideScript.PlayerSide.level); break;
        }
        return approval;
    }

    /// <summary>
    /// returns HQ sprite for current player side, returns errorSprite if a problem
    /// </summary>
    /// <returns></returns>
    public Sprite GetHqMainSpirte()
    {
        Sprite sprite = GameManager.i.spriteScript.errorSprite;
        switch (GameManager.i.sideScript.PlayerSide.level)
        {
            case 1: sprite = hQAuthority.sprite; break;
            case 2: sprite = hQResistance.sprite; break;
            default: Debug.LogWarningFormat("Unrecognised playerSide.level \"{0}\"", GameManager.i.sideScript.PlayerSide.level); break;
        }
        return sprite;
    }



    /// <summary>
    /// returns current HQ approval level for specified side in colour formatted string
    /// </summary>
    /// <returns></returns>
    public string GetHqApprovalLevel(GlobalSide side)
    {
        string description = "Unknown";
        if (side != null)
        {
            switch (side.level)
            {
                case 1:
                    description = string.Format("{0}<size=130%>{1}{2} out of {3}</size>", colourNeutral, _approvalAuthority, colourEnd, maxHqApproval);
                    break;
                case 2:
                    description = string.Format("{0}<size=130%>{1}{2} out of {3}</size>", colourNeutral, _approvalResistance, colourEnd, maxHqApproval);
                    break;
                default:
                    Debug.LogErrorFormat("Invalid player side \"{0}\"", GameManager.i.sideScript.PlayerSide.name);
                    break;
            }
        }
        else { Debug.LogWarning("Invalid side (Null)"); }
        return description;
    }

    /// <summary>
    /// returns current HQ details for specified side in colour formatted string. 'Unknown' if an problem. 'isBarTooltip' shows default text if true, otherwise zip unless critical
    /// </summary>
    /// <returns></returns>
    public string GetHqDetails(GlobalSide side, bool isBarTooltip = false)
    {
        /*string colourNode = colourGrey;*/
        string text = "";
        if (side != null)
        {
            switch (side.level)
            {
                case 1:
                    //Authority
                    if (_approvalAuthority > 0)
                    {
                        if (isBarTooltip == true)
                        { text = string.Format("At {0}Zero{1}{2}{3}You are FIRED{4}{5}After a {6}Countdown{7}", colourNeutral, colourEnd, "\n", colourAlert, colourEnd, "\n", colourNeutral, colourEnd); }
                    }
                    else
                    { text = string.Format("{0}You will be FIRED{1}{2}in {3}{4}{5} turn{6}", colourBad, colourEnd, "\n", colourNeutral, approvalZeroTimer, colourEnd, approvalZeroTimer != 1 ? "s" : ""); }
                    break;
                case 2:
                    //Resistance
                    if (_approvalResistance > 0)
                    {
                        if (isBarTooltip == true)
                        { text = string.Format("At {0}Zero{1}{2}{3}You are FIRED{4}{5}After a {6}Countdown{7}", colourNeutral, colourEnd, "\n", colourAlert, colourEnd, "\n", colourNeutral, colourEnd); }
                    }
                    else
                    { text = string.Format("{0}You will be FIRED{1}{2}in {3}{4}{5} turn{6}", colourBad, colourEnd, "\n", colourNeutral, approvalZeroTimer, colourEnd, approvalZeroTimer != 1 ? "s" : ""); }
                    break;
                default:
                    Debug.LogErrorFormat("Invalid player side \"{0}\"", GameManager.i.sideScript.PlayerSide.name);
                    break;
            }
        }
        else { Debug.LogWarning("Invalid side (Null)"); }
        return text;
    }



    /// <summary>
    /// use this to adjust HQ approval level (auto checks for various HQ mechanics and generates a message) 'Reason' is self contained. Amount to change should be negative to lower Approval
    /// </summary>
    /// <param name="amount"></param>
    public void ChangeHqApproval(int amountToChange, GlobalSide side, string reason)
    {
        if (side != null)
        {
            int oldApproval;
            string msgText;
            if (string.IsNullOrEmpty(reason) == true) { reason = "Unknown"; }
            switch (side.level)
            {
                case 1:
                    //Authority
                    oldApproval = ApprovalAuthority;
                    ApprovalAuthority += amountToChange;
                    Debug.LogFormat("[HQ] <b>Authority</b> HQ Approval: change {0}{1} now {2} ({3}){4}", amountToChange > 0 ? "+" : "", amountToChange, ApprovalAuthority, reason, "\n");
                    if (GameManager.i.sideScript.PlayerSide.level == globalAuthority.level)
                    {
                        msgText = string.Format("{0} HQ approval {1}{2}", hQAuthority.name, amountToChange > 0 ? "+" : "", amountToChange);
                        GameManager.i.messageScript.HqApproval(msgText, reason, hQAuthority, oldApproval, amountToChange, ApprovalAuthority);
                    }
                    break;
                case 2:
                    //Resistance
                    oldApproval = ApprovalResistance;
                    ApprovalResistance += amountToChange;
                    Debug.LogFormat("[HQ] Resistance HQ Approval: change {0}{1} now {2} ({3}){4}", amountToChange > 0 ? "+" : "", amountToChange, ApprovalResistance, reason, "\n");
                    if (GameManager.i.sideScript.PlayerSide.level == globalResistance.level)
                    {
                        msgText = string.Format("{0} HQ approval {1}{2}", hQResistance.name, amountToChange > 0 ? "+" : "", amountToChange);
                        GameManager.i.messageScript.HqApproval(msgText, reason, hQResistance, oldApproval, amountToChange, ApprovalResistance);
                    }
                    break;
                default:
                    Debug.LogWarningFormat("Invalid PlayerSide \"{0}\"", side);
                    break;
            }
        }
        else { Debug.LogError("Invalid side (Null)"); }
    }

    /// <summary>
    /// Set HQ approval levels directly when loading saved game data (use ChangeHqApproval otherwise)
    /// </summary>
    /// <param name="side"></param>
    /// <param name="approval"></param>
    public void LoadSetHqApproval(GlobalSide side, int approval)
    {
        if (side != null)
        {
            Debug.AssertFormat(approval > -1 && approval <= 10, "Invalid approval \"{0}\"");
            switch (side.level)
            {
                case 1: ApprovalAuthority = approval; break;
                case 2: ApprovalResistance = approval; break;
                default: Debug.LogWarningFormat("Unrecognised {0}", side); break;
            }
        }
        else { Debug.LogError("Invalid side (Null)"); }
    }


    //
    // - - - Boss Opinion
    //

    public int GetBossOpinion()
    { return bossOpinion; }

    /// <summary>
    /// change boss opinion to new value, generate message, reason is '[due to] [reason]'
    /// </summary>
    /// <param name="newValue"></param>
    /// <param name="reason"></param>
    public void SetBossOpinion(int newValue, string reason)
    {
        int oldValue = bossOpinion;
        bossOpinion = newValue;
        Debug.LogFormat("[HQ] HqManager.cs -> SetBossOpinion: Opinion now {0} (was {1}), due to {2}{3}", newValue, oldValue, reason, "\n");
    }

    /// <summary>
    /// returns a colour formatted string showing boss's current opinion represented as a text, eg. "Poor" (red)
    /// </summary>
    /// <returns></returns>
    public string GetBossOpinionFormatted()
    {
        string opinion = "Unknown";
        if (bossOpinion >= 4) { opinion = string.Format("{0}Very Good{1}", colourGood, colourEnd); }
        else if (bossOpinion <= -4) { opinion = string.Format("{0}Very Poor{1}", colourBad, colourEnd); }
        else
        {
            switch (bossOpinion)
            {
                case 3:
                case 2: opinion = string.Format("{0}Good{1}", colourGood, colourEnd); break;
                case 1:
                case 0:
                case -1: opinion = string.Format("{0}Neutral{1}", colourNeutral, colourEnd); break;
                case -2:
                case -3: opinion = string.Format("{0}Poor{1}", colourBad, colourEnd); break;
                default: Debug.LogWarningFormat("Unrecognised bossOpinion \"{0}\"", bossOpinion); break;
            }
        }
        return opinion;
    }

    /// <summary>
    /// returns a random ranking HQ position
    /// </summary>
    /// <returns></returns>
    public ActorHQ GetRandomHqPosition()
    {
        List<ActorHQ> listOfHQPositions = new List<ActorHQ>() { ActorHQ.Boss, ActorHQ.SubBoss1, ActorHQ.SubBoss2, ActorHQ.SubBoss3 };
        return listOfHQPositions[Random.Range(0, listOfHQPositions.Count)];
    }

    /// <summary>
    /// HQ relocating. Takes a number of turns. Services unavailable while relocating. Handles all admin. Time to relocate increases with every instance
    /// </summary>
    /// <param name="reason"></param>
    public void RelocateHq(string reason = "Unknown")
    {
        //stats (do first as needed for calcs below) 
        GameManager.i.dataScript.StatisticIncrement(StatType.HQRelocations);
        //use max value of level or campaign (as campaign data isn't updated until metagame)
        int timesRelocated = Mathf.Max(GameManager.i.dataScript.StatisticGetLevel(StatType.HQRelocations), GameManager.i.dataScript.StatisticGetCampaign(StatType.HQRelocations));
        isHqRelocating = true;
        timerHqRelocating = timerRelocationBase * timesRelocated;
        //messages
        Debug.LogFormat("[HQ] HqManager.cs -> RelocateHQ: HQ relocates due to {0}{1}", reason, "\n");
        string text = string.Format("HQ is relocating due to {0} (will take {1} turns)", reason, timerHqRelocating);
        GameManager.i.messageScript.HqRelocates(text, reason);
    }


    public int GetHqRelocationTimer()
    { return timerHqRelocating; }

    public void SetHqRelocationTimer(int timer)
    { timerHqRelocating = timer; }

    /// <summary>
    /// Metagame resetting of HQ data and HQ actors
    /// </summary>
    public void ProcessMetaHq(GlobalSide playerSide)
    {
        isHqRelocating = false;
        timerHqRelocating = 0;
        //
        // - - - HQ actors
        //

        //check for random events
        int numOfEvents = 0;
        //Hq Hierarchy checked first
        Actor[] arrayOfHierarchy = GameManager.i.dataScript.GetArrayOfActorsHQ();
        if (arrayOfHierarchy != null)
        {
            //HQ workers present in pool (No Major event allowed if none 'cause how do you replace a vacant hierarchy slot?)
            int numOfWorkers = GameManager.i.dataScript.CheckHqWorkers();
            //should be at least same number as hierarchy actors (to cover for any departures due to Major events)
            if (numOfWorkers < numOfActorsHQ)
            {
                //add new workers (create new actors)
                GameManager.i.actorScript.InitialiseHqWorkers(numOfActorsHQ - numOfWorkers, playerSide);
            }

            for (int i = 1; i < (int)ActorHQ.Count - 2; i++)
            {
                Actor actor = arrayOfHierarchy[i];
                if (actor != null)
                {
                    numOfEvents = ProcessHqHierarchy(actor, numOfEvents);
                    if (numOfEvents > maxNumOfEvents)
                    { break; }
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfHierarchy[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid arrayOfActorsHQ (Null)"); }
        //Hq Workers checked last -> Minor (Power) events only
        if (numOfEvents < maxNumOfEvents)
        {
            List<int> ListOfHqActors = GameManager.i.dataScript.GetListOfActorHq();
            //reverse loop as workers may be 'removed' due to Power dropping below zero
            for (int i = ListOfHqActors.Count - 1; i >= 0; i--)
            {
                Actor actor = GameManager.i.dataScript.GetHqActor(ListOfHqActors[i]);
                if (actor != null)
                {
                    //worker?
                    if (actor.statusHQ == ActorHQ.Worker)
                    {
                        numOfEvents = ProcessHqWorkers(actor, numOfEvents);
                        if (numOfEvents > maxNumOfEvents)
                        { break; }
                    }
                }
                else { Debug.LogErrorFormat("Invalid Hq Actor (Null) for listOfHqActors[{0}]", i); }
            }
        }
        //check hierarchy positions (may change due to Power)
        CheckHqHierarchy();
    }

    /// <summary>
    /// subMethod for ProcessMetaHq to check for and execute any random events for HQ Hierarchy actors
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="numOfEvents"></param>
    /// <returns></returns>
    private int ProcessHqHierarchy(Actor actor, int numOfEvents)
    {
        int rnd;
        //random event?
        rnd = Random.Range(0, 100);
        Debug.LogFormat("[Rnd] HQManager.cs -> ProcessHqHierarchy: {0}, {1}, hqID {2} Event Check, need < {3}, rolled {4}{5}", actor.actorName,
            GetHqTitle(actor.statusHQ), actor.hqID, chanceOfRandomEvent, rnd, "\n");
        if (rnd < chanceOfRandomEvent)
        {
            int change, powerBefore;
            float changeMultiplier = 1.0f;
            float chanceMajor = chanceOfMajorEvent;
            float chanceGood = 50;
            ProcessHqTraits(actor, ref chanceMajor, ref chanceGood, ref changeMultiplier);
            string reason = "Unknown";
            string text = "Unknown";
            string eventText = "Unknown";
            rnd = Random.Range(0, 100);
            Debug.LogFormat("[Rnd] HQManager.cs -> ProcessHqHierarchy: {0}, {1}, hqID {2} MAJOR Event Check, need < {3}, rolled {4}{5}", actor.actorName,
                GetHqTitle(actor.statusHQ), actor.hqID, chanceMajor, rnd, "\n");
            if (rnd < chanceMajor)
            {
                //
                // - - - MAJOR event -> hq actor leaves
                //
                reason = hQMajorEvent.GetRandomRecord(false);
                actor.AddHistory(new HistoryActor() { text = string.Format("Leaves HQ due to {0}, ex-{1}", reason, GameManager.i.cityScript.GetCityName()) });
                Debug.LogFormat("[HQ] HQManager.cs -> ProcessHqHierarchy:{0}, {1}, hqID {2} MAJOR EVENT{3}", actor.actorName, GetHqTitle(actor.statusHQ), actor.hqID, "\n");
                Debug.LogFormat("[HQ] HQManager.cs -> ProcessHqHierarchy: {0}, {1}, leaves HQ due to {2}{3}", actor.actorName, GetHqTitle(actor.statusHQ),
                    reason, "\n");
                //add to list
                eventText = string.Format("{0}, {1}{2}{3}{4}{5}LEAVES HQ{6} because of {7}", actor.actorName, colourNeutral, GetHqTitle(actor.statusHQ), colourEnd, "\n", colourBad, colourEnd, reason);
                GameManager.i.dataScript.AddHqEvent(eventText);
                //remove actor from hierarcy and hq current list (permanent departure)
                GameManager.i.dataScript.RemoveHqActor(actor.hqID);
            }
            else
            {
                //
                // - - - Minor event -> change in Power
                //
                Debug.LogFormat("[HQ] HQManager.cs -> ProcessHqHierarchy: {0}, {1}, hqID {2} Minor Event{3}", actor.actorName, GetHqTitle(actor.statusHQ), actor.hqID, "\n");
                //amount
                powerBefore = actor.Power;
                change = (int)(basePowerChange * changeMultiplier);
                if (actor.statusHQ != ActorHQ.Worker)
                { change *= hierarchyFactor; }
                //determine Power change good or bad
                rnd = Random.Range(0, 100);
                Debug.LogFormat("[Rnd] HQManager.cs -> ProcessHqHierarchy: {0}, {1}, hqID {2} Minor Good Event Check, need < {3}, rolled {4}{5}", actor.actorName,
                    GetHqTitle(actor.statusHQ), actor.hqID, chanceGood, rnd, "\n");
                if (rnd < chanceGood)
                {
                    //good change
                    actor.Power += change;
                    text = hQMinorEventHierarchyGood.GetRandomRecord(false);
                    reason = string.Format("gains +{0} Power because of {1} (before {2}, now {3} Power)", change, text, powerBefore, actor.Power);
                    actor.AddHistory(new HistoryActor() { text = string.Format("Gains Power at HQ because of {0}, ex-{1}", text, GameManager.i.cityScript.GetCityName()) });
                    //add to list
                    eventText = string.Format("{0}{1}{2}, {3}{4}{5}{6}gains {7}+{8} Power{9} due to {10}", colourNormal, actor.actorName, colourEnd, colourNeutral, GetHqTitle(actor.statusHQ),
                        colourEnd, "\n", colourGood, change, colourEnd, text);
                    GameManager.i.dataScript.AddHqEvent(eventText);
                }
                else
                {
                    //bad change
                    actor.Power -= change;
                    text = hQMinorEventHierarchyBad.GetRandomRecord(false);
                    change *= -1;
                    reason = string.Format("loses {0} Power because of {1} (before {2}, now {3} Power)", change, text, powerBefore, actor.Power);
                    actor.AddHistory(new HistoryActor() { text = string.Format("Loses Power at HQ because of {0}, ex-{1}", text, GameManager.i.cityScript.GetCityName()) });
                    //add to list
                    eventText = string.Format("{0}{1}{2}, {3}{4}{5}{6}loses {7}{8} Power{9} because of {10}", colourNormal, actor.actorName, colourEnd, colourNeutral, GetHqTitle(actor.statusHQ),
                        colourEnd, "\n", colourBad, change, colourEnd, text);
                    GameManager.i.dataScript.AddHqEvent(eventText);
                }
                Debug.LogFormat("[HQ] HQManager.cs -> ProcessHqHierarchy: {0}, {1}, {2}{3}", actor.actorName, GetHqTitle(actor.statusHQ),
                    reason, "\n");
                //Power tracking
                HqPowerData powerData = new HqPowerData()
                {
                    turn = 0,
                    scenarioIndex = GameManager.i.campaignScript.GetScenarioIndex() + 1,
                    change = change,
                    newPower = actor.Power,
                    reason = eventText
                };
                actor.AddHqPowerData(powerData);
            }
            //limit of events exceeded?
            numOfEvents++;
            if (isMetaLogs)
            { Debug.LogFormat("[Tst] HQManager.cs -> ProcessHqHierarchy: numOfEvents {0} of {1}{2}", numOfEvents, maxNumOfEvents, "\n"); }
        }
        return numOfEvents;
    }

    /// <summary>
    /// subMethod for ProcessMetaHq to check for and execute any random events for HQ Worker actors
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="numOfEvents"></param>
    /// <returns></returns>
    private int ProcessHqWorkers(Actor actor, int numOfEvents)
    {
        int rnd, powerBefore;
        //random event?
        rnd = Random.Range(0, 100);
        Debug.LogFormat("[Rnd] HQManager.cs -> ProcessHqWorkers: {0}, {1}, hqID {2} Event Check, need < {3}, rolled {4}{5}", actor.actorName,
            GetHqTitle(actor.statusHQ), actor.hqID, chanceOfRandomEvent, rnd, "\n");
        if (rnd < chanceOfRandomEvent)
        {
            int change;
            float changeMultiplier = 1.0f;
            float chanceMajor = chanceOfMajorEvent;
            float chanceGood = 50;
            ProcessHqTraits(actor, ref chanceMajor, ref chanceGood, ref changeMultiplier);
            string reason = "Unknown";
            string text = "Unknown";
            string eventText = "Unknown";
            //
            // - - - Minor events only -> change in Power
            //
            Debug.LogFormat("[HQ] HQManager.cs -> ProcessHqWorkers: {0}, {1}, hqID {2} Minor Event{3}", actor.actorName, GetHqTitle(actor.statusHQ), actor.hqID, "\n");
            //amount
            powerBefore = actor.Power;
            change = (int)(basePowerChange * changeMultiplier);
            if (actor.statusHQ != ActorHQ.Worker)
            { change *= hierarchyFactor; }
            //determine Power change good or bad
            rnd = Random.Range(0, 100);
            Debug.LogFormat("[Rnd] HQManager.cs -> ProcessHqWorkers: {0}, {1}, hqID {2} Minor Good Event Check, need < {3}, rolled {4}{5}", actor.actorName,
                GetHqTitle(actor.statusHQ), actor.hqID, chanceGood, rnd, "\n");
            if (rnd < chanceGood)
            {
                //good change
                actor.Power += change;
                text = hQMinorEventWorkerGood.GetRandomRecord(false);
                reason = string.Format("gains +{0} Power because of {1} (before {2}, now {3} Power)", change, text, powerBefore, actor.Power);
                actor.AddHistory(new HistoryActor() { text = string.Format("Gains Power at HQ because of {0}, ex-{1}", text, GameManager.i.cityScript.GetCityName()) });
                //add to list
                eventText = string.Format("{0}{1}{2}, {3}{4}{5}{6}gains {7}+{8} Power{9} due to {10}, ex-{11}", colourNormal, actor.actorName, colourEnd, colourAlert, GetHqTitle(actor.statusHQ),
                    colourEnd, "\n", colourGood, change, colourEnd, text, GameManager.i.cityScript.GetCityName());
                GameManager.i.dataScript.AddHqEvent(eventText);
            }
            else
            {
                //bad change
                actor.Power -= change;
                text = hQMinorEventWorkerBad.GetRandomRecord(false);
                change *= -1;
                reason = string.Format("loses {0} Power because of {1} (before {2}, now {3} Power)", change, text, powerBefore, actor.Power);
                actor.AddHistory(new HistoryActor() { text = string.Format("Loses Power at HQ because of {0}", text) });
                //add to list
                text = string.Format("{0}{1}{2}, {3}{4}{5}{6}loses {7}{8} Power{9} due to {10}", colourNormal, actor.actorName, colourEnd, colourAlert, GetHqTitle(actor.statusHQ), colourEnd,
                    "\n", colourBad, change, colourEnd, text);
                GameManager.i.dataScript.AddHqEvent(text);
            }
            Debug.LogFormat("[HQ] HQManager.cs -> ProcessHqWorkers: {0}, {1}, {2}{3}", actor.actorName, GetHqTitle(actor.statusHQ),
                reason, "\n");
            //Power tracking
            HqPowerData powerData = new HqPowerData()
            {
                turn = 0,
                scenarioIndex = GameManager.i.campaignScript.GetScenarioIndex() + 1,
                change = change,
                newPower = actor.Power,
                reason = eventText
            };
            actor.AddHqPowerData(powerData);
            //limit of events exceeded?
            numOfEvents++;
            if (isMetaLogs)
            { Debug.LogFormat("[Tst] HQManager.cs -> ProcessHqWorkers: numOfEvents {0} of {1}{2}", numOfEvents, maxNumOfEvents, "\n"); }
            //Power dropped below Zero, remove actor from hq
            if (powerBefore + change < 0)
            {
                //record event
                text = string.Format("{0}{1}{2}, {3}{4}{5}{6}dismissed from HQ", colourNormal, actor.actorName, colourEnd, colourAlert, GetHqTitle(actor.statusHQ),
                    colourEnd, "\n");
                GameManager.i.dataScript.AddHqEvent(text);
                //remove actor
                GameManager.i.dataScript.RemoveHqActor(actor.hqID);
                Debug.LogFormat("[HQ] HQManager.cs -> ProcessHqWorker: {0}, {1}, hqID {2}, Worker, forcefully removed from HQ (Poewr < 0){3}", actor.actorName, actor.arc.name, actor.hqID, "\n");
            }
        }
        return numOfEvents;
    }


    /// <summary>
    /// determines what effect various traits have on random event chances (eg. whether a major event or, if not, whether it's a good minor event)
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="chanceMajor"></param>
    /// <param name="chanceGood"></param>
    private void ProcessHqTraits(Actor actor, ref float chanceMajor, ref float chanceGood, ref float changeMultiplier)
    {
        Trait trait = actor.GetTrait();
        if (trait != null)
        {
            if (trait.hqPowerMultiplier > 0)
            { changeMultiplier = trait.hqPowerMultiplier; }
            if (trait.hqMajorMultiplier > 0)
            { chanceMajor *= trait.hqMajorMultiplier; }
            if (trait.hqMinorMultiplier > 0)
            { chanceGood *= trait.hqMinorMultiplier; }
        }
        else { Debug.LogErrorFormat("Invalid trait (Null) for actor {0}, {1}, hqID {2}", actor.actorName, GetHqTitle(actor.statusHQ), actor.hqID); }
    }

    /// <summary>
    /// Checks hq hierarchy and shuffles / assigns actors into vacant slots in order of descending Power, eg. Boss has the highest Power. Workers can take over hierarchy slots with enough Power
    /// </summary>
    private void CheckHqHierarchy()
    {
        Actor[] arrayOfHqActors = GameManager.i.dataScript.GetArrayOfActorsHQ();
        List<int> listOfHqActors = GameManager.i.dataScript.GetListOfActorHq();
        if (arrayOfHqActors != null)
        {
            if (listOfHqActors != null)
            {
                int limitPower = 0;
                ActorHQ currentStatus;
                ActorHQ previousStatus;
                for (int index = 1; index < (int)ActorHQ.Count - 2; index++)
                {
                    //accomodate empty HQ hierarchy slots
                    Actor currentActor = arrayOfHqActors[index];
                    if (currentActor != null) { currentStatus = currentActor.statusHQ; }
                    else { currentStatus = (ActorHQ)index; }
                    //check no actor has more Power -> NOTE: code assumes that slots will be filled progressively from highest rank to lowest
                    switch (currentStatus)
                    {
                        case ActorHQ.Boss: limitPower = 999; break;
                        case ActorHQ.SubBoss1: limitPower = arrayOfHqActors[(int)ActorHQ.Boss].Power; break;
                        case ActorHQ.SubBoss2: limitPower = arrayOfHqActors[(int)ActorHQ.SubBoss1].Power; break;
                        case ActorHQ.SubBoss3: limitPower = arrayOfHqActors[(int)ActorHQ.SubBoss2].Power; break;
                    }
                    //current actor holding down position
                    if (currentActor != null)
                    {
                        Actor newActor = GetActorWithHighestPower(listOfHqActors, currentActor.Power, limitPower);
                        if (newActor != null)
                        {
                            previousStatus = newActor.statusHQ;
                            Debug.LogFormat("[HQ] HQManager.cs -> CheckHqHierarchy: {0}, {1}, r{2}, Replaced by {3}, {4}, r{5}{6}",
                                currentActor.actorName, GetHqTitle(currentActor.statusHQ), currentActor.Power,
                                newActor.actorName, GetHqTitle(newActor.statusHQ), newActor.Power, "\n");
                            //check if existing hierarchy actor
                            if (newActor.statusHQ != ActorHQ.Worker)
                            {
                                //empty existing position
                                arrayOfHqActors[(int)newActor.statusHQ] = null;
                            }
                            //replace actor -> new actor into Hierarchy slot, current actor back to hqPool
                            newActor.statusHQ = (ActorHQ)index;
                            arrayOfHqActors[index] = newActor;
                            //bump current actor back to work status (they can then compete for lower level hierarchy positions)
                            currentActor.statusHQ = ActorHQ.Worker;
                            //history
                            currentActor.AddHistory(new HistoryActor()
                            {
                                text = string.Format("Demoted from {0}{1}{2} position at HQ, ex-{3}", colourAlert, GetHqTitle(newActor.statusHQ), colourEnd,
                                GameManager.i.cityScript.GetCityName())
                            });
                            newActor.AddHistory(new HistoryActor()
                            {
                                text = string.Format("Promoted to {0}{1}{2} position at HQ (previously {3}{4}{5}), ex-{6}",
                                colourAlert, GetHqTitle(newActor.statusHQ), colourEnd, colourAlert, GetHqTitle(previousStatus), colourEnd, GameManager.i.cityScript.GetCityName())
                            });
                        }
                        else
                        {
                            Debug.LogFormat("[HQ] HQManager.cs -> CheckHqHierarchy: {0}, {1}, Power {2} is secure in their position{3}", currentActor.actorName,
                                GetHqTitle(currentActor.statusHQ), currentActor.Power, "\n");
                            if (GameManager.i.campaignScript.GetScenarioIndex() > GameManager.i.scenarioStartLevel)
                            {
                                currentActor.AddHistory(new HistoryActor()
                                {
                                    text = string.Format("Position secure at HQ as {0}{1}{2}, ex-{3}", colourAlert, GetHqTitle(currentActor.statusHQ), colourEnd,
                                  GameManager.i.cityScript.GetCityName())
                                });
                            }
                        }
                    }
                    else
                    {
                        //new actor needed, get one with highest Power (null current actor, hence 0 Power)
                        Actor newActor = GetActorWithHighestPower(listOfHqActors, 0, limitPower);
                        if (newActor != null)
                        {
                            previousStatus = newActor.statusHQ;
                            //replace actor -> new actor into Hierarchy slot, current actor back to hqPool
                            arrayOfHqActors[index] = newActor;
                            Debug.LogFormat("[HQ] HQManager.cs -> CheckHqHierarchy: {0}, {1}, r{2}, assumes vacant role of {3}{4}",
                                newActor.actorName, GetHqTitle(newActor.statusHQ), newActor.Power, GetHqTitle((ActorHQ)index), "\n");
                            //check if existing hierarchy actor
                            if (newActor.statusHQ != ActorHQ.Worker)
                            {
                                //empty existing position
                                arrayOfHqActors[(int)newActor.statusHQ] = null;
                            }
                            //assign new position
                            newActor.statusHQ = (ActorHQ)index;
                            if (newActor.Power > 15 && newActor.statusHQ == ActorHQ.Worker)
                            {
                                //had a higher position previously (can't say what it was 'cause when they were bumped they were sent back to the Worker pool)
                                newActor.AddHistory(new HistoryActor()
                                {
                                    text = string.Format("Reasssigned to {0}{1}{2} position at HQ, ex-{3}", colourAlert, GetHqTitle(newActor.statusHQ), colourEnd,
                                    GameManager.i.cityScript.GetCityName())
                                });
                            }
                            else
                            {
                                newActor.AddHistory(new HistoryActor()
                                {
                                    text = string.Format("Promoted to {0}{1}{2} position at HQ (previously {3}{4}{5}), ex-{6}",
                             colourAlert, GetHqTitle(newActor.statusHQ), colourEnd, colourAlert, GetHqTitle(previousStatus), colourEnd, GameManager.i.cityScript.GetCityName())
                                });
                            }

                        }
                        else { Debug.LogErrorFormat("No actor found suitable for vacant slot {0}", (ActorHQ)index); }
                    }
                }
            }
            else { Debug.LogError("Invalid listOfHqActors (Null)"); }
        }
        else { Debug.LogError("Invalid arrayOfHqActors (Null)"); }
    }

    /// <summary>
    /// Finds actor with highest Power, > currentPower (Power of actor currently in that position) and <  limitPower eg. actor in next highest position. Returns Null if none found
    /// ListOfActors is uptodate listOfHqActors and is null checked by parent method
    /// </summary>
    /// <param name="hierarchy"></param>
    /// <returns></returns>
    private Actor GetActorWithHighestPower(List<int> listOfActors, int currentPower, int limitPower)
    {
        Actor actorTemp = null;
        int hqID;
        int candidateHqID = -1;
        int candidateHqPower = currentPower;
        for (int i = 0; i < listOfActors.Count; i++)
        {
            hqID = listOfActors[i];
            actorTemp = GameManager.i.dataScript.GetHqActor(hqID);
            if (actorTemp != null)
            {
                if (actorTemp.Power > candidateHqPower && actorTemp.Power < limitPower)
                {
                    //viable candidate -> check if higher Power than any existing candidate
                    if (actorTemp.Power > candidateHqPower)
                    {
                        candidateHqID = actorTemp.hqID;
                        candidateHqPower = actorTemp.Power;
                    }
                }
            }
            else { Debug.LogWarningFormat("Invalid actor (Null) for hqID {0}", hqID); }
        }
        Actor actorReturn = null;
        if (candidateHqID > -1)
        { actorReturn = GameManager.i.dataScript.GetHqActor(candidateHqID); }
        return actorReturn;
    }

    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Debug method to display HQ info
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayHq()
    {
        StringBuilder builder = new StringBuilder();
        //authority
        builder.AppendFormat(" AUTHORITY{0}", "\n");
        builder.AppendFormat(" {0} HQ{1}", hQAuthority.name, "\n");
        builder.AppendFormat(" {0}{1}{2}", hQAuthority.descriptor, "\n", "\n");
        builder.AppendFormat(" AI Resource Pool: {0}{1}", GameManager.i.dataScript.CheckAIResourcePool(GameManager.i.globalScript.sideAuthority), "\n");
        builder.AppendFormat(" AI Resource Allowance: {0}{1}{2}", GameManager.i.aiScript.resourcesGainAuthority, "\n", "\n");
        //resistance
        builder.AppendFormat("{0} RESISTANCE{1}", "\n", "\n");
        builder.AppendFormat(" {0} HQ{1}", hQResistance.name, "\n");
        builder.AppendFormat(" {0}{1}{2}", hQResistance.descriptor, "\n", "\n");
        builder.AppendFormat(" AI Resource Pool: {0}{1}", GameManager.i.dataScript.CheckAIResourcePool(GameManager.i.globalScript.sideResistance), "\n");
        builder.AppendFormat(" AI Resource Allowance: {0}{1}{2}", GameManager.i.aiScript.resourcesGainResistance, "\n", "\n");
        //HQ data
        builder.AppendFormat("{0}- HQ Hierarchy{1}", "\n", "\n");
        builder.AppendFormat(" bossOpinion {0}{1}, {2}{3}", bossOpinion > 0 ? "+" : "", bossOpinion, GetBossOpinionFormatted(), "\n");
        //Relocation
        builder.AppendFormat("{0}- HQ Relocation{1}", "\n", "\n");
        builder.AppendFormat(" isHqRelocating: {0}{1}", isHqRelocating, "\n");
        builder.AppendFormat(" timerHqRelocating: {0}", timerHqRelocating);
        return builder.ToString();
    }

    /// <summary>
    /// Debug method to display HQ Actors
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayHqActors()
    {
        StringBuilder builder = new StringBuilder();
        Actor[] arrayOfActors = GameManager.i.dataScript.GetArrayOfActorsHQ();
        if (arrayOfActors != null)
        {
            builder.AppendFormat("-HQ Hierarchy{0}", "\n");
            //first and last indexes are blanks ('None', 'Workers', 'LeftHQ')
            for (int i = 1; i < (int)ActorHQ.Count - 2; i++)
            {
                Actor actor = arrayOfActors[i];
                if (actor != null)
                {
                    builder.AppendFormat("{0}- {1}{2}", "\n", actor.statusHQ, "\n");
                    builder.AppendFormat(" {0}, {1}, ID {2}, Mot {3}, Comp {4}, R {5}{6}", actor.actorName, actor.arc.name, actor.actorID, actor.GetDatapoint(ActorDatapoint.Opinion1),
                        actor.GetPersonality().GetCompatibilityWithPlayer(), actor.Power, "\n");
                    builder.AppendFormat("  trait: {0}{1}  {2}{3}", actor.GetTrait().tag, "\n", actor.GetTrait().hqDescription, "\n");
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfActorsHQ[{0}]", i); }
            }
            //loop dictOfHQ and get Workers and LeftHQ
            List<Actor> listOfWorkers = new List<Actor>();
            List<Actor> listOfLeftHQ = new List<Actor>();
            Dictionary<int, Actor> dictOfHQ = GameManager.i.dataScript.GetDictOfHq();
            if (dictOfHQ != null)
            {
                foreach (var actor in dictOfHQ)
                {
                    if (actor.Value != null)
                    {
                        switch (actor.Value.statusHQ)
                        {
                            case ActorHQ.Worker: listOfWorkers.Add(actor.Value); break;
                            case ActorHQ.LeftHQ: listOfLeftHQ.Add(actor.Value); break;
                        }
                    }
                    else { Debug.LogWarning("Invalid actor (Null) in dictOfHQ"); }
                }
                //display Workers
                builder.AppendFormat("{0}- Workers{1}", "\n", "\n");
                if (listOfWorkers.Count > 0)
                {
                    foreach (Actor actor in listOfWorkers)
                    {
                        builder.AppendFormat(" {0}, {1}, ID {2}, Mot {3}, Comp {4}, R {5}{6}", actor.actorName, actor.arc.name, actor.actorID, actor.GetDatapoint(ActorDatapoint.Opinion1),
                            actor.GetPersonality().GetCompatibilityWithPlayer(), actor.Power, "\n");
                    }
                }
                else { builder.AppendFormat(" No records{0}", "\n"); }
                //display LeftHQ
                builder.AppendFormat("{0}- LeftHQ{1}", "\n", "\n");
                if (listOfLeftHQ.Count > 0)
                {
                    foreach (Actor actor in listOfLeftHQ)
                    {
                        builder.AppendFormat(" {0}, {1}, ID {2}, Mot {3}, Comp {4}, R {5}{6}", actor.actorName, actor.arc.name, actor.actorID, actor.GetDatapoint(ActorDatapoint.Opinion1),
                            actor.GetPersonality().GetCompatibilityWithPlayer(), actor.Power, "\n");
                    }
                }
                else { builder.AppendFormat(" No records{0}", "\n"); }
            }
            else { Debug.LogError("Invalid dictOfHQ (Null)"); }
        }
        else { Debug.LogError("Invalid arrayOfActorsHQ (Null)"); }
        return builder.ToString();
    }


    /// <summary>
    /// displays HQ actors Power data packages if any are present (eg. entries made everytime actor experiences a change in Power)
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayHqActorPower()
    {
        StringBuilder builder = new StringBuilder();
        Actor[] arrayOfActors = GameManager.i.dataScript.GetArrayOfActorsHQ();
        if (arrayOfActors != null)
        {
            builder.AppendFormat("-HQ Hierarchy Power Data{0}", "\n");
            //first and last indexes are blanks ('None', 'Workers', 'LeftHQ')
            for (int i = 1; i < (int)ActorHQ.Count - 2; i++)
            {
                Actor actor = arrayOfActors[i];
                if (actor != null)
                {
                    List<HqPowerData> listOfPowerData = actor.GetListOfHqPowerData();
                    if (listOfPowerData != null && listOfPowerData.Count > 0)
                    {
                        builder.AppendFormat("{0}- {1}, {2}, {3}, ID {4}{5}", "\n", actor.statusHQ, actor.actorName, GetHqTitle(actor.statusHQ), actor.actorID, "\n");
                        builder.AppendFormat("  trait: {0}{1}  {2}{3}", actor.GetTrait().tag, "\n", actor.GetTrait().hqDescription, "\n");
                        for (int j = 0; j < listOfPowerData.Count; j++)
                        {
                            HqPowerData data = listOfPowerData[j];
                            if (data != null)
                            { builder.AppendFormat("  s{0}, t{1}: R {2}{3}, now {4}, {5}{6}", data.scenarioIndex, data.turn, data.change > 0 ? "+" : "", data.change, data.newPower, data.reason, "\n"); }
                            else
                            { Debug.LogWarningFormat("Invalid hqPowerData (Null) for listOfHqPowerData[{0}], actor {1}, {2}, ID {3}", j, actor.actorName, GetHqTitle(actor.statusHQ), actor.actorID); }
                        }
                    }
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfActorsHQ[{0}]", i); }
            }
            //loop dictOfHQ and get Workers and LeftHQ
            List<Actor> listOfWorkers = new List<Actor>();
            Dictionary<int, Actor> dictOfHQ = GameManager.i.dataScript.GetDictOfHq();
            if (dictOfHQ != null)
            {
                foreach (var actor in dictOfHQ)
                {
                    if (actor.Value != null)
                    {
                        switch (actor.Value.statusHQ)
                        { case ActorHQ.Worker: listOfWorkers.Add(actor.Value); break; }
                    }
                    else { Debug.LogWarning("Invalid actor (Null) in dictOfHQ"); }
                }
                //display Workers
                builder.AppendFormat("{0}- Workers{1}", "\n", "\n");
                if (listOfWorkers.Count > 0)
                {
                    foreach (Actor actor in listOfWorkers)
                    {
                        builder.AppendFormat(" {0}, {1}, ID {2}, Mot {3}, Comp {4}, R {5}{6}", actor.actorName, actor.arc.name, actor.actorID, actor.GetDatapoint(ActorDatapoint.Opinion1),
                            actor.GetPersonality().GetCompatibilityWithPlayer(), actor.Power, "\n");
                        List<HqPowerData> listOfPowerData = actor.GetListOfHqPowerData();
                        if (listOfPowerData != null && listOfPowerData.Count > 0)
                        {
                            for (int j = 0; j < listOfPowerData.Count; j++)
                            {
                                HqPowerData data = listOfPowerData[j];
                                if (data != null)
                                { builder.AppendFormat("  s{0}, t{1}: R {2}{3}, now {4}, {5}{6}", data.scenarioIndex, data.turn, data.change > 0 ? "+" : "", data.change, data.newPower, data.reason, "\n"); }
                                else { Debug.LogWarningFormat("Invalid hqPowerData (Null) for listOfHqPowerData[{0}], actor {1}, {2}, ID {3}", j, actor.actorName, actor.arc.name, actor.actorID); }
                            }
                        }
                    }
                }
                else { builder.AppendFormat(" No records{0}", "\n"); }
            }
            else { Debug.LogError("Invalid dictOfHQ (Null)"); }
        }
        else { Debug.LogError("Invalid arrayOfActorsHQ (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// Gets EndLevelData for a specified HQ actor for End Level transitionUI page. Returns empty data package if a problem.
    /// </summary>
    /// <param name="actorHQ"></param>
    /// <returns></returns>
    public EndLevelData GetEndLevelData(ActorHQ actorHQ)
    {
        EndLevelData data = new EndLevelData();
        Actor actor = GameManager.i.dataScript.GetHqHierarchyActor(actorHQ);
        if (actor != null)
        {
            int tally = 0;
            int overall = 0;
            int num = 0;
            int times = 0;
            int count = 0;
            string factorOne, factorTwo, factorThree;
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}<align=\"center\"><b>{1}</b></align>{2}{3}{4}", colourAlert, GetHqTitle(actorHQ), colourEnd, "\n", "\n");
            switch (actorHQ)
            {
                case ActorHQ.Boss:
                    factorOne = "Objectives";
                    factorTwo = "City Loyalty";
                    factorThree = "Opinion";
                    //
                    // - - - first factor -> Objectives
                    //
                    List<Objective> listOfObjectives = GameManager.i.objectiveScript.GetListOfObjectives();
                    if (listOfObjectives != null)
                    {
                        count = listOfObjectives.Count;
                        if (count > 0)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                Objective objective = listOfObjectives[i];
                                if (objective != null)
                                {
                                    num += objective.progress;
                                    if (isMetaLogs)
                                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} Objective \"{1}\", progress {2}, num {3}{4}", actorHQ, objective.tag, objective.progress, num, "\n"); }
                                }
                                else { Debug.LogWarningFormat("Invalid objective (Null) for listOfObjectives[{0}]", i); }
                            }
                        }
                        else { Debug.LogWarning("Invalid listOfObjectives (Empty)"); }
                    }
                    else { Debug.LogWarning("Invalid listOfObjectives (Null)"); }
                    //adjust tally to a 0 to 3 range
                    if (count > 0)
                    {
                        num = num / count;
                        if (isMetaLogs)
                        { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} Objectives num RndToInt {1}{2}", actorHQ, num, "\n"); }
                        num = GetTenFactor(num / 10);
                    }
                    if (isMetaLogs)
                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} Objectives  TenFactor result {1}{2}", actorHQ, num, "\n"); }
                    builder.Append(GetFactorString(factorOne, num));
                    tally += num * factorFirst;
                    //
                    // - - - second factor -> City Loyalty
                    //
                    num = GetTenFactor(GameManager.i.cityScript.CityLoyalty);
                    if (isMetaLogs)
                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} City Loyalty {1}{2}", actorHQ, num, "\n"); }
                    builder.Append(GetFactorString(factorTwo, num));
                    tally += num * factorSecond;
                    //
                    // - - - third factor -> Personal Opinion
                    //
                    num = actor.GetDatapoint(ActorDatapoint.Opinion1);
                    if (isMetaLogs)
                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} Opinion {1}{2}", actorHQ, num, "\n"); }
                    builder.Append(GetFactorString(factorThree, num));
                    tally += num * factorThird;
                    //overall
                    overall = GetOverall(tally);
                    data.power = overall * factorBoss;
                    data.medal = (EndlLevelMedal)overall;
                    //tooltips
                    data.tooltipPortrait = GetEndLevelPortraitTooltip(actor, data.power, factorOne, factorTwo, factorThree);
                    data.tooltipMedal = GetEndLevelMedalTooltip(data.medal);
                    break;
                case ActorHQ.SubBoss1:
                    factorOne = "Targets";
                    factorTwo = "Exposure";
                    factorThree = "Opinion";
                    //
                    // - - - first factor -> Targets completed -> 3+ targets is 3 stars (capped)
                    //
                    num = GameManager.i.dataScript.StatisticGetLevel(StatType.TargetSuccesses);
                    if (isMetaLogs)
                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} Targets (succeeded) {1}{2}", actorHQ, num, "\n"); }
                    num = Mathf.Min(num, 3);
                    builder.Append(GetFactorString(factorOne, num));
                    tally += num * factorFirst;
                    //
                    // - - - second factor -> Exposure (Innocence)
                    //
                    num = GameManager.i.playerScript.Innocence;
                    if (isMetaLogs)
                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} Innocence {1}{2}", actorHQ, num, "\n"); }
                    builder.Append(GetFactorString(factorTwo, num));
                    tally += num * factorSecond;
                    //
                    // - - - third factor -> Personal Opinion
                    //
                    actor = GameManager.i.dataScript.GetHqHierarchyActor(actorHQ);
                    if (actor != null)
                    { num = actor.GetDatapoint(ActorDatapoint.Opinion1); }
                    else
                    {
                        Debug.LogWarning("Invalid actor (Null) for ActorHQ.Boss");
                        num = 0;
                    }
                    if (isMetaLogs)
                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} Opinion {1}{2}", actorHQ, num, "\n"); }
                    builder.Append(GetFactorString(factorThree, num));
                    tally += num * factorThird;
                    //overall
                    overall = GetOverall(tally);
                    data.power = overall * factorSubBoss1;
                    data.medal = (EndlLevelMedal)overall;
                    //tooltips
                    data.tooltipPortrait = GetEndLevelPortraitTooltip(actor, data.power, factorOne, factorTwo, factorThree);
                    data.tooltipMedal = GetEndLevelMedalTooltip(data.medal);
                    break;
                case ActorHQ.SubBoss2:
                    factorOne = "District Crisis";
                    factorTwo = "HQ Approval";
                    factorThree = "Opinion";
                    //
                    // - - - first factor -> Exploding Crisis (one star for each instance, capped at 3)
                    //
                    times = GameManager.i.dataScript.StatisticGetLevel(StatType.NodeCrisisExplodes);
                    num = Mathf.Min(times, 3);
                    if (isMetaLogs)
                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} NodeCrisisExploded {1}, stars {2}{3}", actorHQ, times, num, "\n"); }
                    builder.Append(GetFactorString(factorOne, num));
                    tally += num * factorFirst;
                    //
                    // - - - second factor -> HQ Approval
                    //
                    num = GetTenFactor(GetHqApproval());
                    if (isMetaLogs)
                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} HQ Approval {1}{2}", actorHQ, num, "\n"); }
                    builder.Append(GetFactorString(factorTwo, num));
                    tally += num * factorSecond;
                    //
                    // - - - third factor -> Personal Opinion
                    //
                    actor = GameManager.i.dataScript.GetHqHierarchyActor(actorHQ);
                    if (actor != null)
                    { num = actor.GetDatapoint(ActorDatapoint.Opinion1); }
                    else
                    {
                        Debug.LogWarning("Invalid actor (Null) for ActorHQ.Boss");
                        num = 0;
                    }
                    if (isMetaLogs)
                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} Opinion {1}{2}", actorHQ, num, "\n"); }
                    builder.Append(GetFactorString(factorThree, num));
                    tally += num * factorThird;
                    //overall
                    overall = GetOverall(tally);
                    data.power = overall * factorSubBoss2;
                    data.medal = (EndlLevelMedal)overall;
                    //tooltips
                    data.tooltipPortrait = GetEndLevelPortraitTooltip(actor, data.power, factorOne, factorTwo, factorThree);
                    data.tooltipMedal = GetEndLevelMedalTooltip(data.medal);
                    break;
                case ActorHQ.SubBoss3:
                    factorOne = "Reviews";
                    factorTwo = "Investigations";
                    factorThree = "Opinion";
                    //
                    // - - - first factor -> Reviews
                    //
                    times = GameManager.i.dataScript.StatisticGetLevel(StatType.ReviewCommendations) - GameManager.i.dataScript.StatisticGetLevel(StatType.ReviewBlackmarks);
                    if (isMetaLogs)
                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} Reviews commendations - blackmarks {1}{2}", actorHQ, times, "\n"); }
                    num = Mathf.Clamp(times, 0, 3);
                    builder.Append(GetFactorString(factorOne, num));
                    tally += num * factorFirst;
                    //
                    // - - - second factor -> Investigations (3 stars if no Investigations launched, 0 stars if so)
                    //
                    times = GameManager.i.dataScript.StatisticGetLevel(StatType.InvestigationsLaunched);
                    if (times == 0) { num = 3; } else { num = 0; }
                    if (isMetaLogs)
                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} TimesInvestigated {1}, stars {2}{3}", actorHQ, times, num, "\n"); }
                    builder.Append(GetFactorString(factorTwo, num));
                    tally += num * factorSecond;
                    //
                    // - - - third factor -> Personal Opinion
                    //
                    actor = GameManager.i.dataScript.GetHqHierarchyActor(actorHQ);
                    if (actor != null)
                    { num = actor.GetDatapoint(ActorDatapoint.Opinion1); }
                    else
                    {
                        Debug.LogWarning("Invalid actor (Null) for ActorHQ.Boss");
                        num = 0;
                    }
                    if (isMetaLogs)
                    { Debug.LogFormat("[Tst] HQManager.cs -> GetEndLevelData: {0} Opinion {1}{2}", actorHQ, num, "\n"); }
                    builder.Append(GetFactorString(factorThree, num));
                    tally += num * factorThird;
                    //overall
                    overall = GetOverall(tally);
                    data.power = overall * factorSubBoss3;
                    data.medal = (EndlLevelMedal)overall;
                    //tooltips
                    data.tooltipPortrait = GetEndLevelPortraitTooltip(actor, data.power, factorOne, factorTwo, factorThree);
                    data.tooltipMedal = GetEndLevelMedalTooltip(data.medal);
                    break;
                default: Debug.LogWarningFormat("Unrecognised actorHQ \"{0}\"", actorHQ); break;
            }

            if (overall >= 0 && overall <= 3)
            {
                builder.AppendFormat("{0}{1}<b>Overall</b><pos=65%>{2}{3}", "\n", colourAlert, colourEnd, GameManager.i.guiScript.GetNormalStars(overall));
                data.assessmentText = builder.ToString();
            }
            else
            {
                Debug.LogWarningFormat("Invalid overall \"{0}\"", overall);
                builder.AppendFormat("{0}{1}<b>Overall</b><pos=65%>{2}{3}", "\n", colourAlert, colourEnd, GameManager.i.guiScript.GetNormalStars(0));
                data.assessmentText = builder.ToString();
            }
        }
        else { Debug.LogErrorFormat("Invalid actor (Null) for actorHQ \"{0}\"", actorHQ); }
        return data;
    }

    /// <summary>
    /// Returns a colour Formatted string for a factor
    /// </summary>
    /// <param name="factorName"></param>
    /// <param name="stars"></param>
    /// <returns></returns>
    private string GetFactorString(string factorName, int stars)
    {
        if (string.IsNullOrEmpty(factorName) == false)
        {
            if (stars >= 0 && stars <= 3)
            { return string.Format("<b><size=90%>{0}</size></b><pos=65%>{1}{2}", factorName, GameManager.i.guiScript.GetNormalStars(stars), "\n"); }
            else
            { Debug.LogWarningFormat("Invalid stars \"{0}\"", stars); }
        }
        else
        { Debug.LogWarning("Invalid factorName (Null or Empty)"); }
        return "Unknown";
    }

    /// <summary>
    /// returns a 0 - 3 star rating for anything that has a 0 - 10 value range, eg. City Support, HQ Approval
    /// </summary>
    /// <param name="factor"></param>
    /// <returns></returns>
    private int GetTenFactor(int factor)
    {
        int num = 0;
        switch (factor)
        {
            case 10:
            case 9:
                num = 3; break;
            case 8:
            case 7:
            case 6:
                num = 2; break;
            case 5:
            case 4:
            case 3:
                num = 1; break;
            case 2:
            case 1:
            case 0:
                num = 0; break;
            default: Debug.LogWarningFormat("Invalid factor \"{0}\" (should be 0 to 10)", factor); break;
        }
        return num;
    }

    /// <summary>
    /// returns a 0 to 3 star rating based on a 0 to 18 input (18 if all three factors are at 3 stars)
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    private int GetOverall(int score)
    {
        int num = 0;
        if (score >= 0 && score <= 18)
        {
            switch (score)
            {
                case 18:
                case 17:
                case 16:
                case 15:
                case 14:
                    num = 3; break;
                case 13:
                case 12:
                case 11:
                case 10:
                case 9:
                    num = 2; break;
                case 8:
                case 7:
                case 6:
                case 5:
                case 4:
                    num = 1; break;
                case 3:
                case 2:
                case 1:
                case 0:
                    num = 0; break;
            }
        }
        else
        {
            Debug.LogWarningFormat("Invalid score \"{0}\" (should be between 0 and 18)", score);
            num = 0;
        }
        return num;
    }


    private TooltipData GetEndLevelPortraitTooltip(Actor actor, int power, string factorOne, string factorTwo, string factorThree)
    {
        TooltipData data = new TooltipData();
        int factor = 0;
        string pos = "<pos=70%>";
        //header
        data.header = string.Format("{0}{1}{2}<size=120%>{3}</size>{4}{5}Grants {6}{7}{8} Power", actor.actorName, "\n", colourAlert, GetHqTitle(actor.statusHQ), colourEnd,
                      "\n", colourNeutral, power, colourEnd);
        //main
        StringBuilder builder = new StringBuilder();
        switch (actor.statusHQ)
        {
            case ActorHQ.Boss:
                factor = factorBoss;
                builder.AppendFormat("{0}Weighting{1}{2}", colourAlert, colourEnd, "\n");
                builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}", colourNormal, factorOne, colourEnd, pos, colourNeutral, factorFirst, colourEnd, "\n");
                builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}", colourNormal, factorTwo, colourEnd, pos, colourNeutral, factorSecond, colourEnd, "\n");
                builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}", colourNormal, factorThree, colourEnd, pos, colourNeutral, factorThird, colourEnd);
                break;
            case ActorHQ.SubBoss1:
                factor = factorSubBoss1;
                builder.AppendFormat("{0}Weighting{1}{2}", colourAlert, colourEnd, "\n");
                builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}", colourNormal, factorOne, colourEnd, pos, colourNeutral, factorFirst, colourEnd, "\n");
                builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}", colourNormal, factorTwo, colourEnd, pos, colourNeutral, factorSecond, colourEnd, "\n");
                builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}", colourNormal, factorThree, colourEnd, pos, colourNeutral, factorThird, colourEnd);
                break;
            case ActorHQ.SubBoss2:
                factor = factorSubBoss2;
                builder.AppendFormat("{0}Weighting{1}{2}", colourAlert, colourEnd, "\n");
                builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}", colourNormal, factorOne, colourEnd, pos, colourNeutral, factorFirst, colourEnd, "\n");
                builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}", colourNormal, factorTwo, colourEnd, pos, colourNeutral, factorSecond, colourEnd, "\n");
                builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}", colourNormal, factorThree, colourEnd, pos, colourNeutral, factorThird, colourEnd);
                break;
            case ActorHQ.SubBoss3:
                factor = factorSubBoss3;
                builder.AppendFormat("{0}Weighting{1}{2}", colourAlert, colourEnd, "\n");
                builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}", colourNormal, factorOne, colourEnd, pos, colourNeutral, factorFirst, colourEnd, "\n");
                builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}", colourNormal, factorTwo, colourEnd, pos, colourNeutral, factorSecond, colourEnd, "\n");
                builder.AppendFormat("{0}{1}{2}{3}{4}{5}{6}", colourNormal, factorThree, colourEnd, pos, colourNeutral, factorThird, colourEnd);
                break;
        }
        data.main = builder.ToString();
        //details
        data.details = string.Format("{0}Overall{1}{2} Power per Star{3}{4}{5}{6}", colourAlert, colourEnd, "\n", pos, colourNeutral, factor, colourEnd);
        return data;
    }


    private TooltipData GetEndLevelMedalTooltip(EndlLevelMedal medal)
    {
        TooltipData data = new TooltipData();
        switch (medal)
        {
            case EndlLevelMedal.Gold:
                data.header = string.Format("{0}<size=120%>Gold Medal</size>{1}", colourAlert, colourEnd);
                data.main = string.Format("Your performance has been<br>{0}<size=120%>Outstanding</size>{1}", colourGood, colourEnd);
                data.details = string.Format("Requires {0}3{1} stars", colourNeutral, colourEnd);
                break;
            case EndlLevelMedal.Silver:
                data.header = string.Format("{0}<size=120%>Silver Medal</size>{1}", colourAlert, colourEnd);
                data.main = string.Format("Your performance has been<br>{0}<size=120%>Solid</size>{1}", colourGood, colourEnd);
                data.details = string.Format("Requires {0}2{1} stars", colourNeutral, colourEnd);
                break;
            case EndlLevelMedal.Bronze:
                data.header = string.Format("{0}<size=120%>Bronze Medal</size>{1}", colourAlert, colourEnd);
                data.main = string.Format("Your performance has been<br>{0}<size=120%>{1}</size>{2}", colourNeutral, Random.Range(0, 100) < 50 ? "Barely Passable" : "Lacking", colourEnd);
                data.details = string.Format("Requires {0}1{1} star", colourNeutral, colourEnd);
                break;
            case EndlLevelMedal.DeadDuck:
                data.header = string.Format("{0}<size=120%>Dead Duck Award</size>{1}", colourAlert, colourEnd);
                if (Random.Range(0, 100) < 50)
                { data.main = string.Format("What the h*ll went wrong? This is a<br>{0}<size=120%>Disgrace</size>{1}", colourBad, colourEnd); }
                else { data.main = string.Format("How could this happen? This is an<br>{0}<size=120%>Embarrassment</size>{1}", colourBad, colourEnd); }
                data.details = string.Format("Awarded for {0}0{1} stars", colourNeutral, colourEnd);
                break;
            default: Debug.LogWarningFormat("Unrecognised medal \"{0}\"", medal); break;

        }
        return data;
    }




    //new methods above here
}
