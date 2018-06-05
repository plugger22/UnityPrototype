using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using gameAPI;
using packageAPI;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// handles all turn to turn related matters
/// Note events don't handle this as sequence is important and sequence can't be guaranteed with events
/// </summary>
public class TurnManager : MonoBehaviour
{
    [Tooltip("Base number of actions that the Resistance side can carry out each turn")]
    [Range(1, 4)] public int actionsResistance = 2;
    [Tooltip("Base number of actions that the Authority side can carry out each turn")]
    [Range(1, 4)] public int actionsAuthority = 2;

    public float showSplashTimeout = 2.0f;


    [HideInInspector] public ResistanceState resistanceState;
    [HideInInspector] public AuthoritySecurityState authoritySecurityState;
    [HideInInspector] public GlobalSide currentSide;         //which side is it who is currently taking their turn (Resistance or Authority regardless of Player / AI)

    [SerializeField, HideInInspector]
    private int _turn;
    private int _actionsCurrent;                                                //cumulative actions taken by the player for the current turn
    private int _actionsLimit;                                                  //set to the actions cap for the player's current side
    private int _actionsAdjust;                                                 //any temporary adjustments that apply to the actions Limit
    private int _actionsTotal;                                                  //total number of actions available to the player this turn (adjustments + limit)

    private bool allowQuitting = false;

    //fast access
    private int teamArcErasure = -1;

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
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        //fast access
        teamArcErasure = GameManager.instance.dataScript.GetTeamArcID("ERASURE");
        Debug.Assert(teamArcErasure > -1, "Invalid teamArcErasure");
        //actions
        UpdateActionsLimit(GameManager.instance.sideScript.PlayerSide);
        //states
        resistanceState = ResistanceState.Normal;
        authoritySecurityState = AuthoritySecurityState.Normal;
        //current side
        currentSide = GameManager.instance.sideScript.PlayerSide;
        //event Listeners
        EventManager.instance.AddListener(EventType.NewTurn, OnEvent, "TurnManager");
        EventManager.instance.AddListener(EventType.UseAction, OnEvent, "TurnManager");
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "TurnManager");
        EventManager.instance.AddListener(EventType.ExitGame, OnEvent, "TurnManager");
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "TurnManager");
    }


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
            case EventType.ExitGame:
                Quit();
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
    /// master method that handles sequence for ending a turn and commencing a new one
    /// </summary>
    private void ProcessNewTurn()
    {
        Debug.LogFormat("[Trn] TurnManager: New Turn {0} -> Player: {1}, Current: {2}{3}",
            _turn, GameManager.instance.sideScript.PlayerSide.name, currentSide.name, "\n");
        bool finishedProcessing = false;
        int safetyCircuit = 0;
        //only process a new turn if game state is normal (eg. not in the middle of a modal window operation
        if (GameManager.instance.inputScript.GameState == GameState.Normal)
        {
            //continue processing turns until game over if AI vs. AI or single turn process in Player is involved
            do
            {
                //end the current turn
                EndTurnAI();
                EndTurnFinal();
                //start the new turn
                StartTurnEarly();
                StartTurnLate();
                if (StartTurnFinal() == false)
                {
                    //safety switch to prevent endless loop -> debug only
                    safetyCircuit++;
                    if (safetyCircuit > 10)
                    {
                        finishedProcessing = true;
                        Debug.LogError("TurnManagers.cs -> ProcessNewTurn -> SafetyCircuit triggered");
                        Quit();
                    }
                }
                else
                { finishedProcessing = true; }
            }
            while (finishedProcessing == false);
            //switch off any node Alerts
            GameManager.instance.alertScript.CloseAlertUI(true);
        }
    }


    /// <summary>
    /// all pre-start matters are handled here
    /// </summary>
    private void StartTurnEarly()
    {
        //increment turn counter
        _turn++;
        Debug.LogFormat("TurnManager: - - - StartTurnEarly - - - turn {0}{1}", _turn, "\n");
        EventManager.instance.PostNotification(EventType.StartTurnEarly, this);
        //reset nodes and connections if not in normal state
        if (GameManager.instance.nodeScript.activityState != ActivityUI.None)
        { GameManager.instance.nodeScript.ResetAll(); }
        //update turn in top widget UI
        EventManager.instance.PostNotification(EventType.ChangeTurn, this, _turn);
    }

    /// <summary>
    /// all general post-start matters are handled here
    /// </summary>
    private void StartTurnLate()
    {
        Debug.LogFormat("TurnManager: - - - StartTurnLate - - - turn {0}{1}", _turn, "\n");
        EventManager.instance.PostNotification(EventType.StartTurnLate, this);
        UpdateStates();
    }

    /// <summary>
    /// Special event for admin control (doesn't generate an event for other methods). Returns true if a player interaction phase to come, otherwise false (AI vs AI)
    /// </summary>
    private bool StartTurnFinal()
    {
        bool playerInteraction = true;
        Debug.LogFormat("TurnManager: - - - StartTurnFinal - - - turn {0}{1}", _turn, "\n");
        switch (GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Resistance":
                currentSide = GameManager.instance.globalScript.sideResistance;
                break;
            case "Authority":
                currentSide = GameManager.instance.globalScript.sideAuthority;
                break;
            case "AI":
                //It's an AI vs. AI game so you need to go straight to EndTurnAI as there will be no player interaction
                playerInteraction = false;
                break;
            default:
                Debug.LogErrorFormat("Invalid player Side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name);
                break;
        }
        return playerInteraction;
    }

    /// <summary>
    /// all AI end of turn matters are handled here
    /// </summary>
    /// <returns></returns>
    private void EndTurnAI()
    {
        Debug.LogFormat("TurnManager: - - - EndTurnAI - - - turn {0}{1}", _turn, "\n");
        switch (GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Resistance":
                //process Authority AI
                currentSide = GameManager.instance.globalScript.sideAuthority;
                if (GameManager.instance.sideScript.authorityOverall == SideState.AI)
                { GameManager.instance.aiScript.ProcessAISideAuthority(); }
                if (GameManager.instance.sideScript.resistanceOverall == SideState.AI)
                { GameManager.instance.aiScript.ProcessAISideResistance(); }
                break;
            case "Authority":
                //process Resistance AI
                currentSide = GameManager.instance.globalScript.sideResistance;
                if (GameManager.instance.sideScript.authorityOverall == SideState.AI)
                { GameManager.instance.aiScript.ProcessAISideAuthority(); }
                if (GameManager.instance.sideScript.resistanceOverall == SideState.AI)
                { GameManager.instance.aiScript.ProcessAISideResistance(); }
                break;
            case "AI":
                //Process both sides AI, resistance first
                currentSide = GameManager.instance.globalScript.sideResistance;
                GameManager.instance.aiScript.ProcessAISideResistance();
                currentSide = GameManager.instance.globalScript.sideAuthority;
                GameManager.instance.aiScript.ProcessAISideAuthority();
                break;
            default:
                Debug.LogErrorFormat("Invalid player Side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name);
                break;
        }

    }

    /// <summary>
    /// all general end of turn matters are handled here
    /// </summary>
    /// <returns></returns>
    private void EndTurnFinal()
    {
        //decrement any action adjustments
        GameManager.instance.dataScript.UpdateActionAdjustments();
        //actions
        _actionsCurrent = 0;
        _actionsTotal = _actionsLimit + GameManager.instance.dataScript.GetActionAdjustment(GameManager.instance.sideScript.PlayerSide);
        _actionsTotal = Mathf.Max(0, _actionsTotal);
        Debug.LogFormat("TurnManager: - - - EndTurnFinal - - - turn {0}{1}", _turn, "\n");
        EventManager.instance.PostNotification(EventType.ChangeActionPoints, this, _actionsTotal);
        EventManager.instance.PostNotification(EventType.EndTurnFinal, this);
    }


    public void ChangeSide(GlobalSide side)
    {
        UpdateActionsLimit(side);
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
        _actionsCurrent++;
        Debug.LogFormat("[Act] TurnManager: \"{0}\", {1} of {2} actions, turn {3}{4}", text, _actionsCurrent, _actionsTotal, 
            GameManager.instance.turnScript.Turn, "\n");
        //exceed action limit? (total includes any temporary adjustments)
        remainder = _actionsTotal - _actionsCurrent;
        /*//cached actions (so player can't keep regenerating new gear picks within an action)
        GameManager.instance.gearScript.ResetCachedGearPicks();*/
        if (remainder < 0)
        { Debug.LogError("_actionsTotal exceeded by _actionsCurrent"); }
        else
        { EventManager.instance.PostNotification(EventType.ChangeActionPoints, this, remainder); }
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
                    if (actionAdjustment.side.level == side.level && actionAdjustment.turnStart <= _turn)
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
                        builder.AppendFormat("{0}All Points Bulletin in place{1}", colourGood, colourEnd);
                        builder.AppendFormat("{0}{1}Erasure Teams capture on Invisibility 1 or less{2}", "\n", colourNormal, colourEnd);
                        builder.AppendFormat("{0}{1}Actors with{2} {3}Spooked{4}{5} Trait will refuse to do anything (Resistance){6}", "\n", colourAlert, colourEnd,
                            colourNeutral, colourEnd, colourAlert, colourEnd);
                        break;
                    case AuthoritySecurityState.SecurityAlert:
                        builder.AppendFormat("{0}Security Alert in place{1}", colourGood, colourEnd);
                        builder.AppendFormat("{0}{1}Erasure Teams capture in <b>ADJACENT</b> districts{2}", "\n", colourNormal, colourEnd);
                        builder.AppendFormat("{0}{1}Actors with{2} {3}Spooked{4}{5} Trait will refuse to do anything (Resistance){6}", "\n", colourAlert, colourEnd,
                            colourNeutral, colourEnd, colourAlert, colourEnd);
                        break;
                    case AuthoritySecurityState.SurveillanceCrackdown:
                        builder.AppendFormat("{0}Surveillance Crackdown in place{1}", colourGood, colourEnd);
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
                        builder.AppendFormat("{0}All Points Bulletin in place{1}", colourBad, colourEnd);
                        builder.AppendFormat("{0}{1}Erasure Teams capture on Invisibility 1 or less{2}", "\n", colourNormal, colourEnd);
                        builder.AppendFormat("{0}{1}Actors with{2} {3}Spooked{4}{5} Trait will refuse to do anything{6}", "\n", colourAlert, colourEnd,
                            colourNeutral, colourEnd, colourAlert, colourEnd);
                        break;
                    case AuthoritySecurityState.SecurityAlert:
                        builder.AppendFormat("{0}Security Alert in place{1}", colourBad, colourEnd);
                        builder.AppendFormat("{0}{1}Erasure Teams capture in <b>ADJACENT</b> districts{2}", "\n", colourNormal, colourEnd);
                        builder.AppendFormat("{0}{1}Actors with{2} {3}Spooked{4}{5} Trait will refuse to do anything{6}", "\n", colourAlert, colourEnd,
                            colourNeutral, colourEnd, colourAlert, colourEnd);
                        break;
                    case AuthoritySecurityState.SurveillanceCrackdown:
                        builder.AppendFormat("{0}Surveillance Crackdown in place{1}", colourBad, colourEnd);
                        builder.AppendFormat("{0}{1}Lying Low isn't possible{2}", "\n", colourNormal, colourEnd);
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
    /// Quit 
    /// </summary>
    private void Quit()
    {
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

    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.ExitGame);
    }

    /// <summary>
    /// debug method to change game states
    /// category is 'a' -> AuthorityState, 'r' -> ResistanceState
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
                        { EventManager.instance.PostNotification(EventType.StartSecurityFlash, this);}
                        //set state
                        GameManager.instance.authorityScript.SetAuthoritySecurityState(text, AuthoritySecurityState.APB);
                        
                        break;
                    case "sec":
                    case "SEC":
                        //security alert 
                        text = "Authorities issue a city wide Security Alert";
                        //start flashing red alarm (top WidgetUI) if not already going
                        if (authoritySecurityState == AuthoritySecurityState.Normal)
                        { EventManager.instance.PostNotification(EventType.StartSecurityFlash, this); }
                        //set state
                        GameManager.instance.authorityScript.SetAuthoritySecurityState(text, AuthoritySecurityState.SecurityAlert);
                        break;
                    case "sur":
                    case "SUR":
                        //surveillance crackdown
                        text = "Authorities declare a city wide Survelliance Crackdown";
                        //start flashing red alarm (top WidgetUI) if not already going
                        if (authoritySecurityState == AuthoritySecurityState.Normal)
                        { EventManager.instance.PostNotification(EventType.StartSecurityFlash, this); }
                        //set state
                        GameManager.instance.authorityScript.SetAuthoritySecurityState(text, AuthoritySecurityState.SurveillanceCrackdown);
                        break;
                    case "nor":
                    case "NOR":
                        //reset back to normal
                        text = string.Format("AuthorityState reset to {0}", state);
                        GameManager.instance.authorityScript.SetAuthoritySecurityState(text);
                        //stop flashing red alarm
                        EventManager.instance.PostNotification(EventType.StopSecurityFlash, this);
                        break;
                }
                break;
            case "r":
            case "R":
                //ResistanceState
                break;
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
                text = "The city wide Survelliance Crackdown has been cancelled";
                break;
        }
        if (string.IsNullOrEmpty(text) == false)
        {
            //if no Erasure teams currently on map, revert state to normal
            if (GameManager.instance.dataScript.CheckTeamInfo(teamArcErasure, TeamInfo.OnMap) <= 0)
            {
                GameManager.instance.authorityScript.SetAuthoritySecurityState(text);
                //switch off flashing red indicator on top widget UI
                EventManager.instance.PostNotification(EventType.StopSecurityFlash, this);
            }
        }
    }

    //new methods above here
}
