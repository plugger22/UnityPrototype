using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using packageAPI;
using System.Text;
using modalAPI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// handles all turn to turn related matters
/// Note events don't handle this as sequence is important and sequence can't be guaranteed with events
/// </summary>
public class TurnManager : MonoBehaviour
{
    [Header("Actions")]
    [Tooltip("Base number of actions that the Resistance side can carry out each turn")]
    [Range(1, 4)] public int actionsResistance = 2;
    [Tooltip("Base number of actions that the Authority side can carry out each turn")]
    [Range(1, 4)] public int actionsAuthority = 2;

    [Header("Timing")]
    [Tooltip("Number of turns before mission timer expires that warning messages are shown")]
    [Range(0, 20)] public int warningPeriod = 10;

    [Header("Assorted")]
    [Tooltip("Number of seconds to show finish splash screen for")]
    public float showSplashTimeout = 1.0f;

    #region Save Compatible Data
    [HideInInspector] public WinState winStateLevel = WinState.None;          //set if somebody has won
    [HideInInspector] public WinReason winReasonLevel = WinReason.None;  //why a win (from POV of winner)
    /*[HideInInspector] public ResistanceState resistanceState;*/
    [HideInInspector] public AuthoritySecurityState authoritySecurityState;
    [HideInInspector] public GlobalSide currentSide;         //which side is it who is currently taking their turn (Resistance or Authority regardless of Player / AI). Change value ONLY here in TurnManager.cs
    [HideInInspector] public bool haltExecution;             //used to stop program execution at turn end until all interactions are done prior to opening InfoApp

    /*[SerializeField, HideInInspector]*/
    private int _turn;
    private int _actionsCurrent;                                                //cumulative actions taken by the player for the current turn
    private int _actionsLimit;                                                  //set to the actions cap for the player's current side
    private int _actionsAdjust;                                                 //any temporary adjustments that apply to the actions Limit
    private int _actionsTotal;                                                  //total number of actions available to the player this turn (adjustments + limit)
    #endregion

    private bool allowQuitting = false;
    private Coroutine myCoroutineCompromisedGear;

    //autorun
    private int numOfTurns = 0;
    private bool isAutoRun;

    //win State
    private string winTextTop;                                                  //data passed via SetWinState() so that ProcessNewTurn can output an appropriate win message
    private string winTextBottom;
    private Sprite winSprite;
    private bool isLevelOver = false;                                           //set true when a win State has been achieved to prevent ProcessNewTurn continuing to chug away

    //fast access
    private int teamArcErasure = -1;
    private int scenarioTimer = -1;
    private Condition conditionWounded;


    /*private string colourRebel;
    private string colourAuthority;*/
    private string colourNeutral;
    private string colourNormal;
    private string colourGood;
    private string colourBad;
    private string colourGrey;
    /*private string colourSide;*/
    private string colourAlert;
    private string colourEnd;


    public int Turn
    {
        get { return _turn; }
        set
        {
            _turn = value;
            Debug.LogFormat("TurnManager: Turn {0}{1}", _turn, "\n");
        }
    }

    /// <summary>
    /// new Level
    /// </summary>
    public void ResetTurn()
    { _turn = 0; }

    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                SubInitialiseLevelStart();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                SubInitialiseLevelStart();
                break;
            case GameState.LoadGame:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }
    

    #region Initialise SubMethods

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        //actions
        UpdateActionsLimit(GameManager.instance.sideScript.PlayerSide);
        //states
        authoritySecurityState = AuthoritySecurityState.Normal;
        //current side
        currentSide = GameManager.instance.sideScript.PlayerSide;
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access
        teamArcErasure = GameManager.instance.dataScript.GetTeamArcID("ERASURE");
        scenarioTimer = GameManager.instance.campaignScript.scenario.timer;
        conditionWounded = GameManager.instance.dataScript.GetCondition("WOUNDED");
        Debug.Assert(teamArcErasure > -1, "Invalid teamArcErasure (-1)");
        Debug.Assert(scenarioTimer > -1, "Invalid scenarioTimer (-1)");
        Debug.Assert(conditionWounded != null, "Invalid conditionWounded (Null)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event Listeners
        EventManager.instance.AddListener(EventType.NewTurn, OnEvent, "TurnManager");
        EventManager.instance.AddListener(EventType.UseAction, OnEvent, "TurnManager");
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "TurnManager");
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "TurnManager");
    }
    #endregion

    #endregion


    /// <summary>
    /// Called when an event happens
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect Event type
        switch (eventType)
        {
            case EventType.NewTurn:
                ProcessNewTurn();
                break;
            case EventType.UseAction:
                UseAction((string)Param);
                break;
            case EventType.ChangeSide:
                ChangeSide((GlobalSide)Param);
                break;
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogErrorFormat("Invalid eventType {0}{1}", eventType, "\n");
                break;
        }
    }

    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        /*colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);*/
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
        //current Player side colour
        /*if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; }
        else { colourSide = colourRebel; }*/
    }

    /// <summary>
    /// Autoruns game for specified number of turns with the current AI/Player settings
    /// </summary>
    public void SetAutoRun(int autoTurns)
    {
        numOfTurns = autoTurns;
        GameManager.instance.inputScript.GameState = GameState.PlayGame;
        if (autoTurns > 0)
        {
            isAutoRun = true;
            Debug.LogFormat("AUTORUN for {0} turns", numOfTurns);
            do
            {
                ProcessNewTurn();
                if (winStateLevel == WinState.None)
                {
                    GameManager.instance.dataScript.UpdateCurrentItemData();
                    numOfTurns--;
                }
            }
            while (numOfTurns > 0 && winStateLevel == WinState.None);
            isAutoRun = false;
            //in case of AI vs AI revert the player side to human control
            GameManager.instance.sideScript.RevertToHumanPlayer();
            currentSide = GameManager.instance.sideScript.PlayerSide;
        }
        else { Debug.LogWarning("Invalid autoTurns (must be > 0)"); }
    }

    /// <summary>
    /// autorun active or not
    /// </summary>
    /// <returns></returns>
    public bool CheckIsAutoRun()
    { return isAutoRun; }

    /// <summary>
    /// master method that handles sequence for ending a turn and commencing a new one
    /// </summary>
    private void ProcessNewTurn()
    {
        //only process new turn if a win State hasn't already been acheived
        if (isLevelOver == false)
        {
            GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
            //only process a new turn if game state is normal (eg. not in the middle of a modal window operation
            if (GameManager.instance.inputScript.ModalState == ModalState.Normal)
            {
                
                //pre-processing admin
                haltExecution = false;
                GameManager.instance.guiScript.InfoPipelineClear();
                //end the current turn
                EndTurnAI();
                EndTurnEarly();
                EndTurnLate();
                //start the new turn
                StartTurnEarly();
                StartTurnLate();
                //Nobody has yet won
                if (winStateLevel == WinState.None)
                {
                    //only do for player
                    if (playerSide != null && currentSide.level == playerSide.level)
                    {
                        //turn on info App (only if not autorunning)
                        if (isAutoRun == false)
                        {
                            //switch off any node Alerts
                            GameManager.instance.alertScript.CloseAlertUI(true);
                            //info App displayed AFTER any end of turn Player interactions
                            myCoroutineCompromisedGear = StartCoroutine("CompromisedGear", playerSide);
                        }
                    }
                }
                else
                {
                    //There is a winner
                    isLevelOver = true;
                    ProcessLevelOver();
                }
            }
        }
    }


    /// <summary>
    /// Coroutine to wait until Compromised Gear interactive dialogue is complete
    /// </summary>
    /// <param name="playerSide"></param>
    /// <returns></returns>
    IEnumerator CompromisedGear(GlobalSide playerSide)
    {
        yield return new WaitUntil(() => haltExecution == false);
        GameManager.instance.guiScript.InfoPipelineStart(playerSide);
        yield return null;
    }

    /// <summary>
    /// Level has been won or lost
    /// </summary>
    /// <param name="winState"></param>
    private void ProcessLevelOver()
    {
        switch (winStateLevel)
        {
            case WinState.Authority:
            case WinState.Resistance:
                //outcome message
                ModalOutcomeDetails details = new ModalOutcomeDetails();
                details.textTop = winTextTop;
                details.textBottom = winTextBottom;
                details.side = GameManager.instance.sideScript.PlayerSide;
                details.sprite = winSprite;
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "TurnManager.cs -> ProcessLevelOver");
                break;
            default:
                Debug.LogWarningFormat("Invalid winState \"{0}\"", winStateLevel);
                break;
        }
    }


    /// <summary>
    /// all pre-start matters are handled here
    /// </summary>
    private void StartTurnEarly()
    {
        currentSide = GameManager.instance.sideScript.PlayerSide;
        //increment turn counter
        _turn++;
        Debug.LogFormat("[Trn] TurnManager: New Turn {0} -> Player: {1}, Current: {2}{3}",
            _turn, GameManager.instance.sideScript.PlayerSide.name, currentSide.name, "\n");
        Debug.LogFormat("TurnManager: - - - StartTurnEarly - - - turn {0}{1}", _turn, "\n");
        EventManager.instance.PostNotification(EventType.StartTurnEarly, this, null, "TurnManager.cs -> StartTurnEarly");
        //reset nodes and connections if not in normal state
        if (GameManager.instance.nodeScript.activityState != ActivityUI.None)
        { GameManager.instance.nodeScript.ResetAll(); }
        //update turn in top widget UI
        EventManager.instance.PostNotification(EventType.ChangeTurn, this, _turn, "TurnManager.cs -> StartTurnEarly");
        //check Scenario timer (winstate if expires)
        CheckScenarioTimer();
    }

    /// <summary>
    /// all general post-start matters are handled here
    /// </summary>
    private void StartTurnLate()
    {
        Debug.LogFormat("TurnManager: - - - StartTurnLate - - - turn {0}{1}", _turn, "\n");
        currentSide = GameManager.instance.sideScript.PlayerSide;
        EventManager.instance.PostNotification(EventType.StartTurnLate, this, null, "TurnManager.cs -> StartTurnLate");
        UpdateStates();
    }


    /// <summary>
    /// all AI end of turn matters are handled here
    /// </summary>
    /// <returns></returns>
    private void EndTurnAI()
    {
        Debug.LogFormat("TurnManager: - - - EndTurnAI - - - turn {0}{1}", _turn, "\n");
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1:
                //AUTHORITY Player -> process Resistance AI
                if (GameManager.instance.sideScript.resistanceOverall == SideState.AI)
                {
                    currentSide = GameManager.instance.globalScript.sideResistance;
                    GameManager.instance.aiScript.ProcessAISideResistance();
                    //have AI run nemesis even if Human Authority player
                    if (GameManager.instance.sideScript.authorityOverall == SideState.Human)
                    { GameManager.instance.aiScript.ProcessNemesis(); }
                }
                if (GameManager.instance.sideScript.authorityOverall == SideState.AI)
                {
                    currentSide = GameManager.instance.globalScript.sideAuthority;
                    GameManager.instance.aiScript.ProcessAISideAuthority();
                }
                break;
            case 2:
                //RESISTANCE Player -> process Authority AI
                if (GameManager.instance.sideScript.resistanceOverall == SideState.AI)
                {
                    currentSide = GameManager.instance.globalScript.sideResistance;
                    GameManager.instance.aiScript.ProcessAISideResistance();
                }
                if (GameManager.instance.sideScript.authorityOverall == SideState.AI)
                {
                    currentSide = GameManager.instance.globalScript.sideAuthority;
                    GameManager.instance.aiScript.ProcessAISideAuthority();
                }
                break;
            default:
                Debug.LogErrorFormat("Invalid player Side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name);
                break;
        }
        //Ongoing effects from AI Decisions
        GameManager.instance.aiScript.ProcessOngoingEffects();
    }

    /// <summary>
    /// all general end of turn early matters are handled here
    /// </summary>
    private void EndTurnEarly()
    {
        Debug.LogFormat("TurnManager: - - - EndTurnEarly - - - turn {0}{1}", _turn, "\n");
        currentSide = GameManager.instance.sideScript.PlayerSide;
        EventManager.instance.PostNotification(EventType.EndTurnEarly, this, null, "TurnManager.cs -> StartTurnLate");
    }

    /// <summary>
    /// all general end of turn matters are handled here
    /// </summary>
    /// <returns></returns>
    private void EndTurnLate()
    {
        currentSide = GameManager.instance.sideScript.PlayerSide;
        //decrement any action adjustments
        GameManager.instance.dataScript.UpdateActionAdjustments();
        //actions
        _actionsCurrent = 0;
        _actionsTotal = _actionsLimit + GameManager.instance.dataScript.GetActionAdjustment(GameManager.instance.sideScript.PlayerSide);
        _actionsTotal = Mathf.Max(0, _actionsTotal);
        Debug.LogFormat("TurnManager: - - - EndTurnLate - - - turn {0}{1}", _turn, "\n");
        EventManager.instance.PostNotification(EventType.ChangeActionPoints, this, _actionsTotal, "TurnManager.cs -> EndTurnFinal");
        EventManager.instance.PostNotification(EventType.EndTurnLate, this, null, "TurnManager.cs -> EndTurnFinal");
    }



    public void ChangeSide(GlobalSide side)
    {
        UpdateActionsLimit(side);
        currentSide = side;
    }

    /// <summary>
    /// sub-method to set up action limit based on player's current side. Run at game start and whenever change sides or action effect applied.
    /// To force and update and retain current actions value, input the current action value, otherwise leave as default zero
    /// </summary>
    /// <param name="side"></param>
    public void UpdateActionsLimit(GlobalSide side, int currentActions = 0)
    {
        _actionsCurrent = currentActions;
        switch (side.name)
        {
            case "Authority": _actionsLimit = actionsAuthority; break;
            case "Resistance": _actionsLimit = actionsResistance; break;
            default: Debug.LogErrorFormat("Invalid side {0}", side.name); break;
        }
        //calculate total actions available
        _actionsAdjust = GameManager.instance.dataScript.GetActionAdjustment(side);
        _actionsTotal = _actionsLimit + _actionsAdjust;
    }

    /// <summary>
    /// call this method (via event) everytime an action is expended by the Player. Triggers new turn if action limit reached, error if exceeded
    /// </summary>
    private void UseAction(string text = "Unknown")
    {
        int remainder;
        if (GameManager.instance.playerScript.CheckConditionPresent(conditionWounded, GameManager.instance.sideScript.PlayerSide) == true)
        {
            remainder = 0;
            _actionsCurrent = _actionsTotal;
            Debug.LogFormat("[Act] TurnManager: \"{0}\", {1} of 1 actions (condition WOUNDED), turn {2}{3}", text, _actionsCurrent, GameManager.instance.turnScript.Turn, "\n");
        }
        else
        {
            _actionsCurrent++;
            Debug.LogFormat("[Act] TurnManager: \"{0}\", {1} of {2} actions, turn {3}{4}", text, _actionsCurrent, _actionsTotal, GameManager.instance.turnScript.Turn, "\n");
            //exceed action limit? (total includes any temporary adjustments)
            remainder = _actionsTotal - _actionsCurrent;
        }
        /*//cached actions (so player can't keep regenerating new gear picks within an action)
        GameManager.instance.gearScript.ResetCachedGearPicks();*/
        if (remainder < 0)
        { Debug.LogError("_actionsTotal exceeded by _actionsCurrent"); }
        else
        { EventManager.instance.PostNotification(EventType.ChangeActionPoints, this, remainder, "TurnManager.cs -> UseAction"); }
    }

    /// <summary>
    /// returns the number of actions the player has currently used
    /// </summary>
    /// <returns></returns>
    public int GetActionsCurrent()
    { return _actionsCurrent; }

    /// <summary>
    /// returns the number of remaining actions currently available to the player for this turn
    /// </summary>
    /// <returns></returns>
    public int GetActionsAvailable()
    { return _actionsTotal - _actionsCurrent; }

    public int GetActionsTotal()
    { return _actionsTotal; }

    /// <summary>
    /// returns a  colour string in format "1 or 2 Actions available"
    /// </summary>
    /// <returns></returns>
    public string GetActionsTooltip()
    {
        return string.Format("{0}{1}{2}{3} of {4}<b>{5}</b>{6} Actions available{7}", colourNeutral, GetActionsAvailable(), colourEnd, colourNormal, colourEnd,
          _actionsTotal, colourNormal, colourEnd);
    }

    /// <summary>
    /// Returns true if the player has at least one remaining action, otherwise false
    /// </summary>
    /// <returns></returns>
    public bool CheckRemainingActions()
    {
        if (_actionsCurrent < _actionsTotal)
        { return true; }
        return false;
    }

    /// <summary>
    /// returns true if Player wounded (in TurnManager.cs because all set up with cached 'conditionWounded')
    /// </summary>
    /// <returns></returns>
    public bool CheckPlayerWounded()
    { return GameManager.instance.playerScript.CheckConditionPresent(conditionWounded, GameManager.instance.sideScript.PlayerSide); }

    /// <summary>
    /// returns a colour formatted string detailing action adjustments (if any) for the action tooltips (details). Null if none.
    /// </summary>
    /// <returns></returns>
    public string GetActionAdjustmentTooltip()
    {
        StringBuilder builder = new StringBuilder();
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        //standard action allowance
        if (side.level == 1)
        { builder.AppendFormat("{0}Base Actions{1} {2}{3}{4} per turn", colourNormal, colourEnd, colourNeutral, actionsAuthority, colourEnd); }
        else if (side.level == 2)
        { builder.AppendFormat("{0}Base Actions{1} {2}{3}{4} per turn", colourNormal, colourEnd, colourNeutral, actionsResistance, colourEnd); }
        //get adjustments
        List<ActionAdjustment> listOfAdjustments = GameManager.instance.dataScript.GetListOfActionAdjustments();
        if (listOfAdjustments != null)
        {
            if (listOfAdjustments.Count > 0)
            {
                foreach (ActionAdjustment actionAdjustment in listOfAdjustments)
                {
                    //same side & valid for this turn or before
                    if (actionAdjustment.sideLevel == side.level && actionAdjustment.turnStart <= _turn)
                    {
                        if (builder.Length > 0) { builder.AppendLine(); }
                        {
                            if (actionAdjustment.value > 0)
                            { builder.AppendFormat("{0}  {1}+{2}{3}", actionAdjustment.descriptor, colourGood, actionAdjustment.value, colourEnd); }
                            else
                            { builder.AppendFormat("{0}  {1}{2}{3}", actionAdjustment.descriptor, colourBad, actionAdjustment.value, colourEnd); }
                        }
                    }
                }
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// returns a colour string in format "Turn 2"
    /// </summary>
    /// <returns></returns>
    public string GetTurnTooltip()
    { return string.Format("Turn {0}{1}{2}", colourNeutral, _turn, colourEnd); }

    /// <summary>
    /// returns a colour string for the turn tooltip details info tip
    /// </summary>
    /// <returns></returns>
    public string GetTurnInfoTip()
    { return string.Format("{0}Press Enter for a New Turn{1}", colourAlert, colourEnd); }

    /// <summary>
    /// returns a colour formatted string detailing info on any Security Measures
    /// </summary>
    /// <returns></returns>
    public string GetSecurityMeasureTooltip()
    {
        StringBuilder builder = new StringBuilder();
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1:
                //Authority Player
                switch (GameManager.instance.turnScript.authoritySecurityState)
                {
                    case AuthoritySecurityState.Normal:
                        builder.AppendFormat("{0}No Security Measures in place{1}", colourBad, colourEnd);
                        break;
                    case AuthoritySecurityState.APB:
                        builder.AppendFormat("{0}<size=115%>All Points Bulletin</size>{1}", colourGood, colourEnd);
                        builder.AppendFormat("{0}{1}Erasure Teams capture on Invisibility 1 or less{2}", "\n", colourNormal, colourEnd);
                        builder.AppendFormat("{0}{1}Actors with{2} {3}Spooked{4}{5} Trait will refuse to do anything (Resistance){6}", "\n", colourAlert, colourEnd,
                            colourNeutral, colourEnd, colourAlert, colourEnd);
                        break;
                    case AuthoritySecurityState.SecurityAlert:
                        builder.AppendFormat("{0}<size=115%>Security Alert</size>{1}", colourGood, colourEnd);
                        builder.AppendFormat("{0}{1}Erasure Teams capture in <b>ADJACENT</b> districts{2}", "\n", colourNormal, colourEnd);
                        builder.AppendFormat("{0}{1}Actors with{2} {3}Spooked{4}{5} Trait will refuse to do anything (Resistance){6}", "\n", colourAlert, colourEnd,
                            colourNeutral, colourEnd, colourAlert, colourEnd);
                        break;
                    case AuthoritySecurityState.SurveillanceCrackdown:
                        builder.AppendFormat("{0}<size=115%>Surveillance Crackdown</size>{1}", colourGood, colourEnd);
                        builder.AppendFormat("{0}{1}Lying Low isn't possible (Resistance){2}", "\n", colourNormal, colourEnd);
                        builder.AppendFormat("{0}{1}Chance of a Nervous Breakdown doubled (Resistance){2}", "\n", colourAlert, colourEnd);
                        builder.AppendFormat("{0}{1}Actors with{2} {3}Spooked{4}{5} Trait will refuse to do anything (Resistance){6}", "\n", colourNormal, colourEnd,
                            colourNeutral, colourEnd, colourNormal, colourEnd);
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid AuthoritySecurityState \"{0}\"", GameManager.instance.turnScript.authoritySecurityState);
                        builder.AppendFormat("{0}No data available on Security Measures{1}", colourAlert, colourEnd);
                        break;
                }
                break;
            case 2:
                //Resistance Player
                switch (GameManager.instance.turnScript.authoritySecurityState)
                {
                    case AuthoritySecurityState.Normal:
                        builder.AppendFormat("{0}No Security Measures in place{1}", colourGood, colourEnd);
                        break;
                    case AuthoritySecurityState.APB:
                        builder.AppendFormat("{0}<size=115%>All Points Bulletin</size>{1}", colourBad, colourEnd);
                        builder.AppendFormat("{0}{1}Erasure Teams capture on Invisibility 1 or less{2}", "\n", colourNormal, colourEnd);
                        builder.AppendFormat("{0}{1}Actors with{2} {3}Spooked{4}{5} Trait will refuse to do anything{6}", "\n", colourAlert, colourEnd,
                            colourNeutral, colourEnd, colourAlert, colourEnd);
                        break;
                    case AuthoritySecurityState.SecurityAlert:
                        builder.AppendFormat("{0}<size=115%>Security Alert</size>{1}", colourBad, colourEnd);
                        builder.AppendFormat("{0}{1}Erasure Teams capture in <b>ADJACENT</b> districts{2}", "\n", colourNormal, colourEnd);
                        builder.AppendFormat("{0}{1}Actors with{2} {3}Spooked{4}{5} Trait will refuse to do anything{6}", "\n", colourAlert, colourEnd,
                            colourNeutral, colourEnd, colourAlert, colourEnd);
                        break;
                    case AuthoritySecurityState.SurveillanceCrackdown:
                        builder.AppendFormat("{0}<size=115%>Surveillance Crackdown</size>{1}", colourBad, colourEnd);
                        builder.AppendFormat("{0}{1}Lying Low isn't possible{2}", "\n", colourNormal, colourEnd);
                        builder.AppendFormat("{0}{1}Stress Leave isn't possible{2}", "\n", colourNormal, colourEnd);
                        builder.AppendFormat("{0}{1}Chance of a Nervous Breakdown doubled{2}", "\n", colourAlert, colourEnd);
                        builder.AppendFormat("{0}{1}Actors with{2} {3}Spooked{4}{5} Trait will refuse to do anything{6}", "\n", colourNormal, colourEnd,
                            colourNeutral, colourEnd, colourNormal, colourEnd);
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid AuthoritySecurityState \"{0}\"", GameManager.instance.turnScript.authoritySecurityState);
                        builder.AppendFormat("{0}No data available on Security Measures{1}", colourAlert, colourEnd);
                        break;
                }
                break;
        }
        return builder.ToString();
    }





    /// <summary>
    /// debug method to change game states
    /// category is 'a' -> AuthorityState, 'r' -> ResistanceState EDIT: ResistanceState redundant, 20May19
    /// state is specific, AuthorityState -> 'apb' & 'sec' & 'nor'
    /// </summary>
    /// <param name="stateCategory"></param>
    /// <param name="stateSpecific"></param>
    /// <returns></returns>
    public string DebugSetState(string category, string state)
    {
        string text = "No matching state found, nothing changed.";
        switch (category)
        {
            case "a":
            case "A":
                //AuthorityState
                switch (state)
                {
                    case "apb":
                    case "APB":
                        //all points bulletin
                        text = "Authorities issue a city wide All Points Bulletin";
                        //start flashing red alarm (top WidgetUI) if not already going
                        if (authoritySecurityState == AuthoritySecurityState.Normal)
                        { EventManager.instance.PostNotification(EventType.StartSecurityFlash, this, null, "TurnManager.cs -> DebugSetState");}
                        //set state
                        GameManager.instance.authorityScript.SetAuthoritySecurityState(text, "Debug Action", AuthoritySecurityState.APB);
                        
                        break;
                    case "sec":
                    case "SEC":
                        //security alert 
                        text = "Authorities issue a city wide Security Alert";
                        //start flashing red alarm (top WidgetUI) if not already going
                        if (authoritySecurityState == AuthoritySecurityState.Normal)
                        { EventManager.instance.PostNotification(EventType.StartSecurityFlash, this, null, "TurnManager.cs -> DebugSetState"); }
                        //set state
                        GameManager.instance.authorityScript.SetAuthoritySecurityState(text, "Debug Action", AuthoritySecurityState.SecurityAlert);
                        break;
                    case "sur":
                    case "SUR":
                        //surveillance crackdown
                        text = "Authorities declare a city wide Surveillance Crackdown";
                        //start flashing red alarm (top WidgetUI) if not already going
                        if (authoritySecurityState == AuthoritySecurityState.Normal)
                        { EventManager.instance.PostNotification(EventType.StartSecurityFlash, this, null, "TurnManager.cs -> DebugSetState"); }
                        //set state
                        GameManager.instance.authorityScript.SetAuthoritySecurityState(text, "Debug Action", AuthoritySecurityState.SurveillanceCrackdown);
                        break;
                    case "nor":
                    case "NOR":
                        //reset back to normal
                        text = string.Format("AuthorityState reset to {0}", state);
                        GameManager.instance.authorityScript.SetAuthoritySecurityState(text, "Debug Action");
                        //stop flashing red alarm
                        EventManager.instance.PostNotification(EventType.StopSecurityFlash, this, null, "TurnManager.cs -> DebugSetState");
                        break;
                }
                break;
            /*case "r":
            case "R":
                //ResistanceState
                break;*/
        }
        return string.Format("{0}{1}Press ESC to exit", text, "\n");
    }


    /// <summary>
    /// Run in Event.StartTurnLate to update Authority/Resistance States when required
    /// </summary>
    public void UpdateStates()
    {
        string text = "";
        //Authority State
        switch (authoritySecurityState)
        {
            case AuthoritySecurityState.APB:
                text = "The city wide All Points Bulletin (APB) has been cancelled";
                break;
            case AuthoritySecurityState.SecurityAlert:
                text = "The city wide Security Alert has been cancelled";
                break;
            case AuthoritySecurityState.SurveillanceCrackdown:
                text = "The city wide Surveillance Crackdown has been cancelled";
                break;
        }
        if (string.IsNullOrEmpty(text) == false)
        {
            //if no Erasure teams currently on map, revert state to normal
            if (GameManager.instance.dataScript.CheckTeamInfo(teamArcErasure, TeamInfo.OnMap) <= 0)
            {
                GameManager.instance.authorityScript.SetAuthoritySecurityState(text, "Security Measures Cancelled");
                //switch off flashing red indicator on top widget UI
                EventManager.instance.PostNotification(EventType.StopSecurityFlash, this, null, "TurnManager.cs -> UpdateStates");
            }
        }
    }

    /// <summary>
    /// called by any method whenever a win state is triggered. Used by ProcessNewTurn to populate an appropriate outcome message
    /// </summary>
    public void SetWinState(WinState win, WinReason reason, string topText, string bottomText)
    {
        Debug.Assert(string.IsNullOrEmpty(topText) == false, "Invalid topText (Null or Empty)");
        Debug.Assert(string.IsNullOrEmpty(bottomText) == false, "Invalid bottomText (Null or Empty)");
        //set autorun to false (needed to allow ProcessWinState to generate an outcome message as isAutoRun true provides a global block on all ModalOutcome.cs -> SetModalOutcome)
        isAutoRun = false;
        //get approriate sprite
        Sprite sprite = GameManager.instance.guiScript.firedSprite;
        switch (reason)
        {
            case WinReason.CityLoyaltyMax:
            case WinReason.CityLoyaltyMin:
            case WinReason.DoomTimerMin:
            case WinReason.FactionSupportMin:
            case WinReason.MissionTimerMin:
            case WinReason.ObjectivesCompleted:
                sprite = GameManager.instance.guiScript.firedSprite;
                break;
            default:
                Debug.LogWarningFormat("Invalid reason \"{0}\"", reason);
                break;
        }
        //assign winState
        if (win != WinState.None)
        {
            if (winStateLevel == WinState.None)
            {
                //generate new win state
                winStateLevel = win;
                winReasonLevel = reason;
                winTextTop = topText;
                winTextBottom = bottomText;
                winSprite = sprite;
            }
            else { Debug.LogErrorFormat("Can't assign winState as GameManager win already set to \"{0}\"", win); }
        }
        else { Debug.LogErrorFormat("Invalid winState \"{0}\"", win); }
    }

    /// <summary>
    /// Checks scenario timer every turn to see if it has expired
    /// </summary>
    private void CheckScenarioTimer()
    {
        string topText, bottomText;
        if (_turn == scenarioTimer)
        {
            topText = string.Format("Your Mission timer ({0}{1} turns{2}) has EXPIRED", colourNeutral, scenarioTimer, colourEnd);
            //win state achieved
            switch (GameManager.instance.campaignScript.scenario.missionResistance.side.level)
            {
                case 1:
                    //Authority mission, timer expired so Resistance wins
                    bottomText = string.Format("{0}The Resistance wins{1}", colourBad, colourEnd);
                    SetWinState(WinState.Resistance, WinReason.MissionTimerMin, topText, bottomText);
                    break;
                case 2:
                    //Resistance mission, timer expired so Authority wins
                    bottomText = string.Format("{0}The Authority wins{1}", colourBad, colourEnd);
                    SetWinState(WinState.Authority, WinReason.MissionTimerMin, topText, bottomText);
                    break;
                default:
                    Debug.LogErrorFormat("Invalid mission side, {0}", GameManager.instance.campaignScript.scenario.missionResistance.side.name);
                    break;
            }
        }
        else if (_turn + warningPeriod > scenarioTimer)
        {
            int turnsRemaining = scenarioTimer - _turn;
            //warning message
            string text = string.Format("TurnManager.cs -> CheckScenarioTimer: There are {0} turn{1} remaining for the Mission{2}", turnsRemaining, turnsRemaining != 1 ? "s" : "", "\n");
            string itemText = "Mission Timer EXPIRING soon";
            topText = "Mission Timer WARNING";
            string reason = string.Format("{0}You have {1}<b>{2} turn{3}</b>{4} remaining to carry out your <b>Objectives</b>", "\n", colourNeutral, turnsRemaining, turnsRemaining != 1 ? "s" : "", colourEnd);
            string warning = "You will LOSE if you do not complete your Objectives in time";
            GameManager.instance.messageScript.GeneralWarning(text, itemText, topText, reason, warning);
        }
    }


    /// <summary>
    /// Quit 
    /// </summary>
    public void Quit()
    {
        //set game state
        GameManager.instance.inputScript.GameState = GameState.ExitGame;
        //switch off main info app if running
        if (myCoroutineCompromisedGear != null)
        { StopCoroutine(myCoroutineCompromisedGear); }
        //show thank you splash screen before quitting
        if (SceneManager.GetActiveScene().name != "End_Game")
        { StartCoroutine("DelayedQuit"); }
        if (allowQuitting == false)
        {
            Debug.LogFormat("TurnManager: Quit selected but not allowed as allowQuitting is false{0}", "\n");
            //Application.CancelQuit();
        }
    }


    /// <summary>
    /// display splash screen for a short while before quitting
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedQuit()
    {
        SceneManager.LoadScene(1);
        yield return new WaitForSeconds(showSplashTimeout);
        allowQuitting = true;
        //editor quit or application quit
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }


    //
    // - - - Save / Load
    //

    /// <summary>
    /// copy data over to fileManager.cs for serialization
    /// </summary>
    /// <returns></returns>
    public TurnActionData LoadWriteData()
    {
        TurnActionData turnData = new TurnActionData();
        turnData.turn = Turn;
        turnData.actionsCurrent = _actionsCurrent;
        turnData.actionsLimit = _actionsLimit;
        turnData.actionsAdjust = _actionsAdjust;
        turnData.actionsTotal = _actionsTotal;
        return turnData;
    }

    /// <summary>
    /// copy across loaded save game data
    /// </summary>
    /// <param name="data"></param>
    public void LoadReadData(TurnActionData data)
    {
        if (data != null)
        {
            Turn = data.turn;
            _actionsCurrent = data.actionsCurrent;
            _actionsLimit = data.actionsLimit;
            _actionsAdjust = data.actionsAdjust;
            _actionsTotal = data.actionsTotal;
        }
        else { Debug.LogError("Invalid TurnActionData (Null)"); }
    }


    //new methods above here
}
