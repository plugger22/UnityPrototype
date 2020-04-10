using gameAPI;
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
    [Tooltip("How much Renown will the HQ give per turn if they decide to support the Player")]
    [Range(1, 3)] public int renownPerTurn = 1;
    [Tooltip("How many actors in the HQ line up. Needs to correspond with enum.ActorHQ. Determines size of DataManager.cs -> arrayOfActorsHQ")]
    [Range(1, 6)] public int numOfActorsHQ = 4;
    [Tooltip("Size cap on HQ worker pool")]
    [Range(0, 10)] public int maxNumOfWorkers = 8;
    [Tooltip("Multiplier for HQ hierarchy Actors renown (Boss gets highest renown, subBoss's progressively less) forumla renown = (numOfActorsHQ + 2 - enum.StatusHQ) * renownFactor")]
    [Range(1, 20)] public int renownFactor = 10;

    [Header("MetaGame")]
    [Tooltip("Chance of an HQ actor suffering a random event that effects their renown")]
    [Range(0, 100)] public int chanceOfRandomEvent = 25;
    [Tooltip("Chance of an HQ actor Major random event (eg. leave), if failed then a random event")]
    [Range(0, 100)] public int chanceOfMajorEvent = 25;
    [Tooltip("Max number of random events that can affect HQ actors - no more checks made after cap reached")]
    [Range(0, 10)] public int maxNumOfEvents = 4;
    [Tooltip("Base amount of renown gained or lost due to a Minor event")]
    [Range(0, 10)] public int baseRenownChange = 6;
    [Tooltip("HQ hierarchy multiplier to baseRenownChange")]
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
    [Tooltip("Minor event (good) reasons for an HQ actor to gain renown")]
    public TextList hQMinorEventGood;
    [Tooltip("Minor event (bad) reasons for an HQ actor to lose renown")]
    public TextList hQMinorEventBad;

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
            _approvalAuthority = value;
            _approvalAuthority = Mathf.Clamp(_approvalAuthority, 0, maxHqApproval);
            //update top widget bar if current side is authority
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
            { EventManager.instance.PostNotification(EventType.ChangeHqBar, this, _approvalAuthority, "HqManager.cs -> ApprovalAuthority"); }
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
            _approvalResistance = value;
            _approvalResistance = Mathf.Clamp(_approvalResistance, 0, maxHqApproval);
            //update top widget bar if current side is resistance
            if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
            { EventManager.instance.PostNotification(EventType.ChangeHqBar, this, _approvalResistance, "HqManager.cs -> ApprovalResistance"); }
        }
    }

    /// <summary>
    /// Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
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
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseNewGame
    private void SubInitialiseNewGame()
    {
        //initialise HQ actors starting lineUp
        GameManager.instance.actorScript.InitialiseHqActors();
        //assign compatibility with player and descriptors
        GameManager.instance.personScript.SetHqActorsCompatibility();
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
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
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "HQManager");
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent, "HQManager");
        EventManager.instance.AddListener(EventType.EndTurnLate, OnEvent, "HQManager");
    }
    #endregion

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        //Authority HQ 
        hQAuthority = GameManager.instance.dataScript.GetHQ(GameManager.instance.globalScript.sideAuthority);
        if (hQAuthority == null)
        { Debug.LogError("Invalid HqAuthority (Null)"); }
        //Resistance HQ
        hQResistance = GameManager.instance.dataScript.GetHQ(GameManager.instance.globalScript.sideResistance);
        if (hQResistance == null)
        { Debug.LogError("Invalid HqResistance (Null)"); }
        //approval levels (if input approval is Zero then generate a random value between 2 & 8)
        int approval = GameManager.instance.campaignScript.scenario.approvalStartAuthorityHQ;
        if (approval == 0) { approval = Random.Range(2, 9); }
        ApprovalAuthority = approval;
        approval = GameManager.instance.campaignScript.scenario.approvalStartRebelHQ;
        if (approval == 0)
        { approval = Random.Range(2, 9); }
        ApprovalResistance = approval;
        Debug.LogFormat("[HQ] HQManager -> Initialise: {0}, approval {1}, {2}, approval {3}{4}",
            hQResistance, ApprovalResistance, hQAuthority, ApprovalAuthority, "\n");
        //text lists
        Debug.Assert(hQMajorEvent != null, "Invalid hqMajorEvent (Null)");
        Debug.Assert(hQMinorEventGood != null, "Invalid hqMinorEventGood (Null)");
        Debug.Assert(hQMinorEventBad != null, "Invalid hqMinorEventBad (Null)");
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
                CheckHQRenownSupport();
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
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.blueText);
        /*colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);*/
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
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
                GameManager.instance.messageScript.HqRelocates(text, "Relocation Successful");
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
                GameManager.instance.messageScript.ActiveEffect(dataEffect);
                Debug.LogFormat("[HQ] HqManager.cs -> EndTurnLate: {0}{1}", text, "\n");
            }
        }
    }

    /// <summary>
    /// checks if player given support (+1 renown) from their HQ based on a random roll vs. level of HQ approval. No support if player inactive.
    /// </summary>
    private void CheckHQRenownSupport()
    {
        bool isProceed = true;
        //ignore if autorun with both sides AI
        if (GameManager.instance.turnScript.CheckIsAutoRun() == true && GameManager.instance.autoRunTurns > 0)
        { isProceed = false; }
        //ignore if player is inactive
        if (GameManager.instance.playerScript.status == ActorStatus.Inactive)
        {
            isProceed = false;
            Debug.LogFormat("[HQ] HqManager.cs -> CheckHQRenownSupport: NO support as Player is Inactive ({0}){1}", GameManager.instance.playerScript.inactiveStatus, "\n");
        }
        //HQ relocating
        if (isHqRelocating == true)
        {
            isProceed = false;
            Hq hqFaction = GetCurrentHQ();
            if (hqFaction != null)
            {
                Debug.LogFormat("[HQ] HqManager.cs -> CheckHQRenownSupport: NO support as HQ Relocating{0}", "\n");
                string text = string.Format("HQ support unavailable as HQ is currently Relocating{0}", "\n");
                string reason = GameManager.instance.colourScript.GetFormattedString(string.Format("<b>{0} is currently Relocating</b>", hqFaction.tag), ColourType.salmonText);
                GameManager.instance.messageScript.HqSupportUnavailable(text, reason, hqFaction);
            }
            else { Debug.LogWarning("Invalid current HQ (Null)"); }
        }
        if (isProceed == true)
        {
            int side = GameManager.instance.sideScript.PlayerSide.level;
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
                            Debug.LogFormat("[Rnd] HqManager.cs -> CheckHQRenownSupport: GIVEN need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                            string msgText = string.Format("{0} provides SUPPORT (+1 Renown)", hQAuthority.tag);
                            GameManager.instance.messageScript.HqSupport(msgText, hQAuthority, _approvalAuthority, GameManager.instance.playerScript.Renown, renownPerTurn);
                            //random
                            GameManager.instance.messageScript.GeneralRandom("HQ support GIVEN", "HQ Support", threshold, rnd);
                            //Support given
                            GameManager.instance.playerScript.Renown += renownPerTurn;
                        }
                        else
                        {
                            //Support declined
                            Debug.LogFormat("[Rnd] HqManager.cs -> CheckHQRenownSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                            string msgText = string.Format("{0} declines support ({1} % chance of support)", hQAuthority.tag, threshold);
                            GameManager.instance.messageScript.HqSupport(msgText, hQAuthority, _approvalAuthority, GameManager.instance.playerScript.Renown);
                            //random
                            GameManager.instance.messageScript.GeneralRandom("HQ support DECLINED", "HQ Support", threshold, rnd);
                        }
                        break;
                    case 2:
                        //Resistance
                        threshold = _approvalResistance * 10;
                        if (rnd < threshold)
                        {
                            //Support Provided
                            Debug.LogFormat("[Rnd] HqManager.cs -> CheckHQRenownSupport: GIVEN need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                            string msgText = string.Format("{0} provides SUPPORT (+1 Renown)", hQResistance.tag);
                            GameManager.instance.messageScript.HqSupport(msgText, hQResistance, _approvalResistance, GameManager.instance.playerScript.Renown, renownPerTurn);
                            //random
                            GameManager.instance.messageScript.GeneralRandom("HQ support GIVEN", "HQ Support", threshold, rnd, false, "rand_1");
                            //Support given
                            GameManager.instance.playerScript.Renown += renownPerTurn;
                        }
                        else
                        {
                            //Support declined
                            Debug.LogFormat("[Rnd] HqManager.cs -> CheckHQRenownSupport: DECLINED need < {0}, rolled {1}{2}", threshold, rnd, "\n");
                            string msgText = string.Format("{0} declines support ({1} % chance of support)", hQResistance.tag, threshold);
                            GameManager.instance.messageScript.HqSupport(msgText, hQResistance, _approvalResistance, GameManager.instance.playerScript.Renown);
                            //random
                            GameManager.instance.messageScript.GeneralRandom("HQ support DECLINED", "HQ Support", threshold, rnd, false, "rand_1");
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
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
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
                GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "HQ Approval", reason, warning);
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
                    if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                    {
                        //Resistance side wins
                        winState = WinStateLevel.Resistance;
                    }
                    msgText = string.Format("{0} approval Zero. Player Fired. Authority wins", playerHQ.tag);
                    itemText = string.Format("{0} has LOST PATIENCE", playerHQ.tag);
                    reason = string.Format("{0} approval at ZERO for an extended period", playerHQ.tag);
                    warning = "You've been FIRED, game over";
                    GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "You're FIRED", reason, warning);
                    //Player fired -> outcome
                    string textTop = string.Format("{0}The {1} has lost faith in your abilities{2}", colourNormal, GetHqName(side), colourEnd);
                    string textBottom = string.Format("{0}You've been FIRED{1}", colourBad, colourEnd);
                    GameManager.instance.turnScript.SetWinStateLevel(winState, WinReasonLevel.HqSupportMin, textTop, textBottom);

                }
                else
                {
                    //message
                    msgText = string.Format("HQ approval Zero. HQ will FIRE you in {0} turn{1}", approvalZeroTimer, approvalZeroTimer != 1 ? "s" : "");
                    itemText = string.Format("{0} about to FIRE you", playerHQ.tag);
                    reason = string.Format("{0} are displeased with your performance", playerHQ.tag);
                    warning = string.Format("You will be FIRED in {0} turn{1}", approvalZeroTimer, approvalZeroTimer != 1 ? "s" : "");
                    GameManager.instance.messageScript.GeneralWarning(msgText, itemText, "HQ Unhappy", reason, warning);
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
    /// returns current HQ for player side, Null if not found
    /// </summary>
    /// <returns></returns>
    public Hq GetCurrentHQ()
    {
        Hq factionHQ = null;
        switch (GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Authority":
                factionHQ = hQAuthority;
                break;
            case "Resistance":
                factionHQ = hQResistance;
                break;
            default:
                Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
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
                        Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
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
                        Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
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
                    Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
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
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1: approval = _approvalAuthority; break;
            case 2: approval = _approvalResistance; break;
            default: Debug.LogWarningFormat("Unrecognised playerSide.level \"{0}\"", GameManager.instance.sideScript.PlayerSide.level); break;
        }
        return approval;
    }

    /// <summary>
    /// returns HQ sprite for current player side, returns errorSprite if a problem
    /// </summary>
    /// <returns></returns>
    public Sprite GetHqSpirte()
    {
        Sprite sprite = GameManager.instance.guiScript.errorSprite;
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1: sprite = hQAuthority.sprite; break;
            case 2: sprite = hQResistance.sprite; break;
            default: Debug.LogWarningFormat("Unrecognised playerSide.level \"{0}\"", GameManager.instance.sideScript.PlayerSide.level); break;
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
                    Debug.LogErrorFormat("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name);
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
                    Debug.LogErrorFormat("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name);
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
                    if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
                    {
                        msgText = string.Format("{0} HQ approval {1}{2}", hQAuthority.name, amountToChange > 0 ? "+" : "", amountToChange);
                        GameManager.instance.messageScript.HqApproval(msgText, reason, hQAuthority, oldApproval, amountToChange, ApprovalAuthority);
                    }
                    break;
                case 2:
                    //Resistance
                    oldApproval = ApprovalResistance;
                    ApprovalResistance += amountToChange;
                    Debug.LogFormat("[HQ] Resistance HQ Approval: change {0}{1} now {2} ({3}){4}", amountToChange > 0 ? "+" : "", amountToChange, ApprovalResistance, reason, "\n");
                    if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
                    {
                        msgText = string.Format("{0} HQ approval {1}{2}", hQResistance.name, amountToChange > 0 ? "+" : "", amountToChange);
                        GameManager.instance.messageScript.HqApproval(msgText, reason, hQResistance, oldApproval, amountToChange, ApprovalResistance);
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
        GameManager.instance.dataScript.StatisticIncrement(StatType.HQRelocations);
        //use max value of level or campaign (as campaign data isn't updated until metagame)
        int timesRelocated = Mathf.Max(GameManager.instance.dataScript.StatisticGetLevel(StatType.HQRelocations), GameManager.instance.dataScript.StatisticGetCampaign(StatType.HQRelocations));
        isHqRelocating = true;
        timerHqRelocating = timerRelocationBase * timesRelocated;
        //messages
        Debug.LogFormat("[HQ] HqManager.cs -> RelocateHQ: HQ relocates due to {0}{1}", reason, "\n");
        string text = string.Format("HQ is relocating due to {0} (will take {1} turns)", reason, timerHqRelocating);
        GameManager.instance.messageScript.HqRelocates(text, reason);
    }


    public int GetHqRelocationTimer()
    { return timerHqRelocating; }

    public void SetHqRelocationTimer(int timer)
    { timerHqRelocating = timer; }

    /// <summary>
    /// Metagame resetting of HQ data and HQ actors
    /// </summary>
    public void ProcessMetaHq()
    {
        isHqRelocating = false;
        timerHqRelocating = 0;
        //
        // - - - HQ actors
        //

        //check for random events
        int numOfEvents = 0;
        //Hq Hierarchy checked first
        Actor[] arrayOfHierarchy = GameManager.instance.dataScript.GetArrayOfActorsHQ();
        if (arrayOfHierarchy != null)
        {
            for (int i = 1; i < (int)ActorHQ.Count - 2; i++)
            {
                Actor actor = arrayOfHierarchy[i];
                if (actor != null)
                {
                    numOfEvents = ProcessHqActor(actor, numOfEvents);
                    if (numOfEvents > maxNumOfEvents)
                    { break; }
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfHierarchy[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid arrayOfActorsHQ (Null)"); }
        //Hq Workers checked last
        if (numOfEvents < maxNumOfEvents)
        {
            List<int> ListOfHqActors = GameManager.instance.dataScript.GetListOfActorHQ();
            for (int i = 0; i < ListOfHqActors.Count; i++)
            {
                Actor actor = GameManager.instance.dataScript.GetHQActor(ListOfHqActors[i]);
                if (actor != null)
                {
                    //worker?
                    if (actor.statusHQ == ActorHQ.Worker)
                    {
                        numOfEvents = ProcessHqActor(actor, numOfEvents);
                        if (numOfEvents > maxNumOfEvents)
                        { break; }
                    }
                }
                else { Debug.LogErrorFormat("Invalid Hq Actor (Null) for listOfHqActors[{0}]", i); }
            }
        }
    }

    /// <summary>
    /// subMethod for ProcesMetaHq to check for and execute any random events for HQ actors
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="numOfEvents"></param>
    /// <returns></returns>
    private int ProcessHqActor(Actor actor, int numOfEvents)
    {
        int rnd;
        //random event?
        rnd = Random.Range(0, 100);
        Debug.LogFormat("[Rnd] HQManager.cs -> ProcessHqActor: {0}, {1}, hqID {2} Event Check, need < {3}, rolled {4}{5}", actor.actorName,
            GameManager.instance.campaignScript.GetHqTitle(actor.statusHQ), actor.hqID, chanceOfRandomEvent, rnd, "\n");
        if (rnd < chanceOfRandomEvent)
        {
            int change, renownBefore;
            int chanceMajor = chanceOfMajorEvent;
            int chanceGood = 50;
            ProcessHqActorTraits(actor, ref chanceMajor, ref chanceGood);
            string reason = "Unknown";
            string text = "Unknown";
            //major event?
            rnd = Random.Range(0, 100);
            Debug.LogFormat("[Rnd] HQManager.cs -> ProcessHqActor: {0}, {1}, hqID {2} MAJOR Event Check, need < {3}, rolled {4}{5}", actor.actorName,
                GameManager.instance.campaignScript.GetHqTitle(actor.statusHQ), actor.hqID, chanceMajor, rnd, "\n");
            if (rnd < chanceMajor)
            {
                //hq actor leaves

                reason = hQMajorEvent.GetRandomRecord(false);
                Debug.LogFormat("[HQ] HQManager.cs -> ProcessHqActor:{0}, {1}, hqID {2} MAJOR EVENT{3}", actor.actorName, GameManager.instance.campaignScript.GetHqTitle(actor.statusHQ), actor.hqID, "\n");
                Debug.LogFormat("[HQ] HQManager.cs -> ProcessHqActor: {0}, {1}, leaves HQ due to {2}{3}", actor.actorName, GameManager.instance.campaignScript.GetHqTitle(actor.statusHQ),
                    reason, "\n");
            }
            else
            {
                //minor event -> change in renown
                Debug.LogFormat("[HQ] HQManager.cs -> ProcessHqActor: {0}, {1}, hqID {2} Minor Event{3}", actor.actorName, GameManager.instance.campaignScript.GetHqTitle(actor.statusHQ), actor.hqID, "\n");
                //amount
                renownBefore = actor.Renown;
                change = baseRenownChange;
                if (actor.statusHQ != ActorHQ.Worker)
                { change *= hierarchyFactor; }
                //determine renown change good or bad
                rnd = Random.Range(0, 100);
                if (rnd < chanceGood)
                {
                    //good change
                    actor.Renown += change;
                    text = hQMinorEventGood.GetRandomRecord(false);
                    reason = string.Format("gains +{0} renown because of {1} (before {2}, now {3} renown)", change, text, renownBefore, actor.Renown);
                }
                else
                {
                    //bad change
                    actor.Renown -= change;
                    text = hQMinorEventBad.GetRandomRecord(false);
                    change *= -1;
                    reason = string.Format("loses -{0} renown because of {1} (before {2}, now {3} renown)", change, text, renownBefore, actor.Renown);
                }
                Debug.LogFormat("[HQ] HQManager.cs -> ProcessHqActor: {0}, {1}, {2}{3}", actor.actorName, GameManager.instance.campaignScript.GetHqTitle(actor.statusHQ),
                    reason, "\n");
                //Renown tracking
                HqRenownData renownData = new HqRenownData()
                {
                    turn = GameManager.instance.turnScript.Turn,
                    change = change,
                    newRenown = actor.Renown,
                    reason = text
                };
                actor.AddHqRenownData(renownData);
            }
            //limit of events exceeded?
            numOfEvents++;
            Debug.LogFormat("[Tst] HQManager.cs -> ProcessHqActor: numOfEvents {0} of {1}{2}", numOfEvents, maxNumOfEvents, "\n");
        }
        return numOfEvents;
    }

    /// <summary>
    /// determines what effect various traits have on random event chances (eg. whether a major event or, if not, whether it's a good minor event)
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="chanceMajor"></param>
    /// <param name="chanceGood"></param>
    private void ProcessHqActorTraits(Actor actor, ref int chanceMajor, ref int chanceGood)
    {

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
        builder.AppendFormat(" AI Resource Pool: {0}{1}", GameManager.instance.dataScript.CheckAIResourcePool(GameManager.instance.globalScript.sideAuthority), "\n");
        builder.AppendFormat(" AI Resource Allowance: {0}{1}{2}", GameManager.instance.aiScript.resourcesGainAuthority, "\n", "\n");
        //resistance
        builder.AppendFormat("{0} RESISTANCE{1}", "\n", "\n");
        builder.AppendFormat(" {0} HQ{1}", hQResistance.name, "\n");
        builder.AppendFormat(" {0}{1}{2}", hQResistance.descriptor, "\n", "\n");
        builder.AppendFormat(" AI Resource Pool: {0}{1}", GameManager.instance.dataScript.CheckAIResourcePool(GameManager.instance.globalScript.sideResistance), "\n");
        builder.AppendFormat(" AI Resource Allowance: {0}{1}{2}", GameManager.instance.aiScript.resourcesGainResistance, "\n", "\n");
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
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetArrayOfActorsHQ();
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
                    builder.AppendFormat(" {0}, {1}, ID {2}, Mot {3}, Comp {4}, R {5}{6}", actor.actorName, actor.arc.name, actor.actorID, actor.GetDatapoint(ActorDatapoint.Motivation1),
                        actor.GetPersonality().GetCompatibilityWithPlayer(), actor.Renown, "\n");
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfActorsHQ[{0}]", i); }
            }
            //loop dictOfHQ and get Workers and LeftHQ
            List<Actor> listOfWorkers = new List<Actor>();
            List<Actor> listOfLeftHQ = new List<Actor>();
            Dictionary<int, Actor> dictOfHQ = GameManager.instance.dataScript.GetDictOfHQ();
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
                        builder.AppendFormat(" {0}, {1}, ID {2}, Mot {3}, Comp {4}, R {5}{6}", actor.actorName, actor.arc.name, actor.actorID, actor.GetDatapoint(ActorDatapoint.Motivation1),
                            actor.GetPersonality().GetCompatibilityWithPlayer(), actor.Renown, "\n");
                    }
                }
                else { builder.AppendFormat(" No records{0}", "\n"); }
                //display LeftHQ
                builder.AppendFormat("{0}- LeftHQ{1}", "\n", "\n");
                if (listOfLeftHQ.Count > 0)
                {
                    foreach (Actor actor in listOfLeftHQ)
                    {
                        builder.AppendFormat(" {0}, {1}, ID {2}, Mot {3}, Comp {4}, R {5}{6}", actor.actorName, actor.arc.name, actor.actorID, actor.GetDatapoint(ActorDatapoint.Motivation1),
                            actor.GetPersonality().GetCompatibilityWithPlayer(), actor.Renown, "\n");
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
    /// displays HQ actors renown data packages if any are present (eg. entries made everytime actor experiences a change in renown)
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayHqActorRenown()
    {
        StringBuilder builder = new StringBuilder();
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetArrayOfActorsHQ();
        if (arrayOfActors != null)
        {
            builder.AppendFormat("-HQ Hierarchy Renown Data{0}", "\n");
            //first and last indexes are blanks ('None', 'Workers', 'LeftHQ')
            for (int i = 1; i < (int)ActorHQ.Count - 2; i++)
            {
                Actor actor = arrayOfActors[i];
                if (actor != null)
                {
                    List<HqRenownData> listOfRenownData = actor.GetListOfHqRenownData();
                    if (listOfRenownData != null && listOfRenownData.Count > 0)
                    {
                        builder.AppendFormat("{0}- {1}, {2}, {3}, ID {4}{5}", "\n", actor.statusHQ, actor.actorName, GameManager.instance.campaignScript.GetHqTitle(actor.statusHQ), actor.actorID, "\n");
                        for (int j = 0; j < listOfRenownData.Count; j++)
                        {
                            HqRenownData data = listOfRenownData[j];
                            if (data != null)
                            { builder.AppendFormat(" t{0}: R {1}{2}, now {3}, {4}{5}", data.turn, data.change > 0 ? "+" : "", data.change, data.newRenown, data.reason, "\n"); }
                            else
                            {
                                Debug.LogWarningFormat("Invalid hqRenownData (Null) for listOfHqRenownData[{0}], actor {1}, {2}, ID {3}", j, actor.actorName,
                             GameManager.instance.campaignScript.GetHqTitle(actor.statusHQ), actor.actorID);
                            }
                        }
                    }
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfActorsHQ[{0}]", i); }
            }
            //loop dictOfHQ and get Workers and LeftHQ
            List<Actor> listOfWorkers = new List<Actor>();
            Dictionary<int, Actor> dictOfHQ = GameManager.instance.dataScript.GetDictOfHQ();
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
                        builder.AppendFormat(" {0}, {1}, ID {2}, Mot {3}, Comp {4}, R {5}{6}", actor.actorName, actor.arc.name, actor.actorID, actor.GetDatapoint(ActorDatapoint.Motivation1),
                            actor.GetPersonality().GetCompatibilityWithPlayer(), actor.Renown, "\n");
                        List<HqRenownData> listOfRenownData = actor.GetListOfHqRenownData();
                        if (listOfRenownData != null && listOfRenownData.Count > 0)
                        {
                            for (int j = 0; j < listOfRenownData.Count; j++)
                            {
                                HqRenownData data = listOfRenownData[j];
                                if (data != null)
                                { builder.AppendFormat("  t{0}: R {1}{2}, now {3}, {4}{5}", data.turn, data.change > 0 ? "+" : "", data.change, data.newRenown, data.reason, "\n"); }
                                else { Debug.LogWarningFormat("Invalid hqRenownData (Null) for listOfHqRenownData[{0}], actor {1}, {2}, ID {3}", j, actor.actorName, actor.arc.name, actor.actorID); }
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


    //new methods above here
}
