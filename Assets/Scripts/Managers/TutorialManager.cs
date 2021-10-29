using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Handles Tutorial
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Tutorials")]
    public Tutorial resistanceTutorial;
    public Tutorial authorityTutorial;

    [Header("Globals")]
    [Tooltip("Maximum number of Tutorial Items allowed per TutorialSet")]
    [Range(5, 15)] public int maxNumOfItems = 10;
    [Tooltip("Mininum number of TopicOptions for a Query type TutorialItem in listOfOptions")]
    [Range(4, 4)] public int minNumOfOptions = 4;

    [Header("Queries")]
    [Tooltip("% chance of a query option tooltip being a Random, textList, based one versus a fixed one")]
    [Range(0, 100)] public int queryOptionTooltipChance = 25;

    [Header("Text Lists")]
    [Tooltip("Reasons for query tooltips")]
    public TextList textListGeneric;
    public TextList textListJob;

    #region save data compatibile
    [HideInInspector] public Tutorial tutorial;
    [HideInInspector] public TutorialSet set;
    [HideInInspector] public int index;                         //index that tracks player's progress (set #) through current tutorial
    #endregion

    #region Other...

    //TutorialItem.SO Query option data (takes data from TutorialOption.SO)
    [HideInInspector] public string option0Tag;
    [HideInInspector] public string option1Tag;
    [HideInInspector] public string option2Tag;
    [HideInInspector] public string option3Tag;
    [HideInInspector] public string optionIgnoreTag;
    [HideInInspector] public TutorialQueryType queryType;

    #endregion

    #region Initialisation...
    /// <summary>
    /// Master Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
                SubInitialiseAsserts();
                SubInitialiseFastAccess();
                SubInitialiseReset();
                SubInitialiseTutorial();
                SubInitialiseEvents();
                break;
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
            case GameState.LoadGame:
            case GameState.StartUp:
            case GameState.FollowOnInitialisation:
                //do nothing
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region SubInitialiseAsserts
    private void SubInitialiseAsserts()
    {
        Debug.Assert(textListGeneric != null, "Invalid textListName (Null)");
        Debug.Assert(textListJob != null, "Invalid textListJob (Null)");
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        Debug.Assert(resistanceTutorial != null, "Invalid resistanceTutorial (Null)");
        /*Debug.Assert(authorityTutorial != null, "Invalid authorityTutorial (Null)");*/  //EDIT -> TO DO -> switch back on for Authority side
    }
    #endregion

    #region SubInitialiseTutorial
    private void SubInitialiseTutorial()
    {
        InitialiseTutorial();
    }
    #endregion

    #region SubInitialiseReset
    private void SubInitialiseReset()
    {
        /*
        //reset all tutorialItem.SO's back to 'isQueryDone' -> false
        TutorialItem[] arrayOfItems = GameManager.i.loadScript.arrayOfTutorialItems;
        if (arrayOfItems != null)
        {
            for (int i = 0; i < arrayOfItems.Length; i++)
            {
                if (arrayOfItems[i] != null)
                { arrayOfItems[i].isQueryDone = false; }
                else { Debug.LogWarningFormat("Invalid TopicItem (Null) in arrayOfItems[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid arrayOfTutorialItems (Null)"); }
        */
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.TutorialPreviousSet, OnEvent, "TutorialManager.cs");
        EventManager.i.AddListener(EventType.TutorialNextSet, OnEvent, "TutorialManager.cs");

    }
    #endregion

    #endregion

    #region OnEvent
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
            case EventType.TutorialPreviousSet:
                SetPreviousSet();
                break;
            case EventType.TutorialNextSet:
                SetNextSet();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion

    #region InitialiseTutorial
    /// <summary>
    /// Set up tutorial prior to running
    /// </summary>
    public void InitialiseTutorial()
    {
        //If an existing tutorial already done during session or a loaded save game tutorial then tutorial won't be none
        if (tutorial == null)
        {
            //Debug -> default tutorial if none present
            tutorial = resistanceTutorial;
        }
        Debug.LogFormat("[Tut] TutorialManager.cs -> InitialiseTutorial: tutorial \"{0}\" loaded{1}", tutorial.name, "\n");
        //set actor pool option (prevents randomly generated hq actors)
        GameManager.i.optionScript.isActorPool = true;
        //set scenario
        if (tutorial.scenario != null)
        {
            GameManager.i.scenarioScript.scenario = tutorial.scenario;
            if (tutorial.scenario.city != null)
            {
                //set city
                GameManager.i.cityScript.SetCity(tutorial.scenario.city);
                Debug.LogFormat("[Tut] TutorialManager.cs -> InitialiseTutorial: city \"{0}\" loaded{1}", tutorial.scenario.city.tag, "\n");
                //set mayor
                GameManager.i.cityScript.InitialiseTutorial(tutorial.scenario);
                //get index
                if (GameManager.i.tutorialStartLevel > -1)
                {
                    //Development override -> if value > max allowed, use max
                    index = Mathf.Min(GameManager.i.tutorialStartLevel, tutorial.listOfSets.Count);
                }
                else { index = GameManager.i.dataScript.GetTutorialIndex(tutorial.name); }
                if (index > -1)
                {
                    Debug.LogFormat("[Tut] TutorialManager.cs -> InitialiseTutorial: index \"{0}\" loaded{1}", index, "\n");
                    //get set
                    if (tutorial.listOfSets.Count > index)
                    {
                        set = tutorial.listOfSets[index];
                        if (set != null)
                        {
                            //Initialise tutorial SET
                            InitialiseTutorialSet(set);
                        }
                        else { Debug.LogErrorFormat("Invalid tutorialSet (Null) for index {0}", index); }
                    }
                    else { Debug.LogErrorFormat("Invalid tutorialIndex (index {0}, there are {1} sets in tutorial.listOfSets)", index, tutorial.listOfSets.Count); }
                }
                else { Debug.LogError("Invalid tutorial index (-1)"); }
            }
            else { Debug.LogError("Invalid tutorial city (Null)"); }
        }
        else { Debug.LogError("Invalid tutorial Scenario (Null)"); }
    }
    #endregion

    #region InitialiseTutorialSet
    /// <summary>
    /// Initialises a tutorial set within a tutorial (could be called by starting a new tutorial or from bouncing around within a tutorial)
    /// </summary>
    /// <param name="set"></param>
    private void InitialiseTutorialSet(TutorialSet set)
    {
        Debug.LogFormat("[Tut] TutorialManager.cs -> - - - InitialiseTutorialSet: set \"{0}\" loaded{1}", set.name, "\n");
        // Features toggle on/off
        UpdateFeatures(set.listOfFeaturesOff, set.listOfGUIOff);
        //Goals reset
        GameManager.i.dataScript.ClearListOfTutorialGoals();
        //Targets reset
        GameManager.i.dataScript.ClearTargets();
        //Teams reset
        GameManager.i.teamScript.ResetTeams();
        //Hide items (spiders/tracers) reset
        GameManager.i.dataScript.ResetHideNodes();
        //Make sure Player is Active
        if (GameManager.i.playerScript.status != ActorStatus.Active)
        {
            GameManager.i.playerScript.status = ActorStatus.Active;
            GameManager.i.playerScript.inactiveStatus = ActorInactive.None;
            GameManager.i.playerScript.tooltipStatus = ActorTooltip.None;
            //alpha
            GameManager.i.actorPanelScript.UpdatePlayerAlpha(GameManager.i.guiScript.alphaActive);
        }
        if (GameManager.i.isSession == true)
        {
            //Hide New turn button (may not be required but do so regardless)
            EventManager.i.PostNotification(EventType.NewTurnHide, this, null, "TutorialManager.cs -> InitialiseTutorialSet");
        }
    }
    #endregion

    #region UpdateFeatures
    /// <summary>
    /// Update features prior for current tutorial set. Note: this will override anything set in GameManager prefab -> FeatureManager
    /// </summary>
    /// <param name="listOfFeatures"></param>
    public void UpdateFeatures(List<TutorialFeature> listOfFeaturesToToggleOff, List<TutorialGUIFeature> listOfGUIFeaturesToToggleOff)
    {
        #region Features
        //
        // - - - Features
        //
        if (listOfFeaturesToToggleOff != null)
        {
            //set all features true
            GameManager.i.optionScript.isAI = true;
            GameManager.i.optionScript.isNemesis = true;
            GameManager.i.optionScript.isFogOfWar = true;
            GameManager.i.optionScript.isDecisions = true;
            GameManager.i.optionScript.isMainInfoApp = true;
            GameManager.i.optionScript.isNPC = true;
            GameManager.i.optionScript.isSubordinates = true;
            GameManager.i.optionScript.isReviews = true;
            GameManager.i.optionScript.isObjectives = true;
            GameManager.i.optionScript.isOrganisations = true;
            GameManager.i.optionScript.isTargets = true;
            GameManager.i.optionScript.isGear = true;
            GameManager.i.optionScript.isRecruit = true;
            GameManager.i.optionScript.isMoveSecurity = true;
            GameManager.i.optionScript.isActions = true;
            //turn OFF any features in list
            for (int i = 0; i < listOfFeaturesToToggleOff.Count; i++)
            {
                TutorialFeature feature = listOfFeaturesToToggleOff[i];
                if (feature != null)
                {
                    switch (feature.name)
                    {
                        case "AI":
                            GameManager.i.optionScript.isAI = false;
                            GameManager.i.debugScript.optionNoAI = "AI ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: AI toggled Off{0}", "\n");
                            break;
                        case "Decisions":
                            GameManager.i.optionScript.isDecisions = false;
                            GameManager.i.debugScript.optionDecisions = "Decisions ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Decisions toggled Off{0}", "\n");
                            break;
                        case "FOW":
                            GameManager.i.optionScript.isFogOfWar = false;
                            GameManager.i.debugScript.optionFogOfWar = "FOW ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Fog of War toggled Off{0}", "\n");
                            break;
                        case "MainInfoApp":
                            GameManager.i.optionScript.isMainInfoApp = false;
                            GameManager.i.debugScript.optionMainInfoApp = "InfoApp ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: MainInfoApp toggled Off{0}", "\n");
                            break;
                        case "Nemesis":
                            GameManager.i.optionScript.isNemesis = false;
                            GameManager.i.debugScript.optionNemesis = "Nemesis ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Nemesis toggled Off{0}", "\n");
                            break;
                        case "NPC":
                            GameManager.i.optionScript.isNPC = false;
                            GameManager.i.debugScript.optionNPC = "NPC ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: NPC toggled Off{0}", "\n");
                            break;
                        case "Objectives":
                            GameManager.i.optionScript.isObjectives = false;
                            GameManager.i.debugScript.optionObjectives = "Objectives ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Objectives toggled Off{0}", "\n");
                            break;
                        case "Reviews":
                            GameManager.i.optionScript.isReviews = false;
                            GameManager.i.debugScript.optionReviews = "Reviews ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Reviews toggled Off{0}", "\n");
                            break;
                        case "Subordinates":
                            GameManager.i.optionScript.isSubordinates = false;
                            GameManager.i.debugScript.optionSubordinates = "Subordinates ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Subordinates toggled Off{0}", "\n");
                            break;
                        case "Organisations":
                            GameManager.i.optionScript.isOrganisations = false;
                            GameManager.i.debugScript.optionOrganisations = "Organisations ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Organisations toggled Off{0}", "\n");
                            break;
                        case "Gear":
                            GameManager.i.optionScript.isGear = false;
                            GameManager.i.debugScript.optionGear = "Gear ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Gear toggled Off{0}", "\n");
                            break;
                        case "Recruiting":
                            GameManager.i.optionScript.isRecruit = false;
                            GameManager.i.debugScript.optionRecruit = "Recruiting ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Recruiting toggled Off{0}", "\n");
                            break;
                        case "MoveSecurity":
                            GameManager.i.optionScript.isMoveSecurity = false;
                            GameManager.i.debugScript.optionMoveSecurity = "Move Sec ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Move Security toggled Off{0}", "\n");
                            break;
                        case "Actions":
                            GameManager.i.optionScript.isActions = false;
                            GameManager.i.debugScript.optionActions = "Actions ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Actions toggled Off{0}", "\n");
                            break;
                        case "Targets":
                            GameManager.i.optionScript.isTargets = false;
                            //auto off if targets are off
                            GameManager.i.optionScript.isOrganisations = false;
                            GameManager.i.optionScript.isObjectives = false;
                            GameManager.i.debugScript.optionTargets = "Targets ON";
                            GameManager.i.debugScript.optionOrganisations = "Organisations ON";
                            GameManager.i.debugScript.optionObjectives = "Objectives ON";
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Targets, Organisations and Objectives all toggled Off{0}", "\n");
                            break;
                        default: Debug.LogWarningFormat("Unrecognised feature.name \"{0}\"", feature.name); break;
                    }
                }
                else { Debug.LogWarningFormat("Invalid feature (Null) for listOfFeaturesToToggleOff[{0}]", i); }
            }
            //Set subordinates -> not at the start, though as other stuff not yet initialised
            if (GameManager.i.inputScript.GameState != GameState.TutorialOptions)
            {
                //activate / deactivate on map actors
                float alpha;
                GlobalSide globalResistance = GameManager.i.globalScript.sideResistance;
                if (GameManager.i.optionScript.isSubordinates == true)
                { alpha = GameManager.i.guiScript.alphaActive; }
                else { alpha = GameManager.i.guiScript.alphaInactive; }
                Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActorsFixed(globalResistance);
                if (arrayOfActors != null)
                {
                    for (int i = 0; i < arrayOfActors.Length; i++)
                    {
                        //check actor is present in slot (not vacant)
                        if (GameManager.i.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                        {
                            Actor actor = arrayOfActors[i];
                            if (actor != null)
                            {
                                //update actor panel
                                GameManager.i.actorPanelScript.UpdateActorAlpha(actor.slotID, alpha);
                            }
                        }
                    }
                }
                else { Debug.LogError("Invalid arrayOfActors (Null)"); }
            }
        }
        else { Debug.LogError("Invalid listOfFeaturesToToggleOff (Null)"); }
        #endregion

        #region Special Cases
        //
        // - - - AI (includes sideTab)
        //
        //discretionary GUI elements toggled off if required (those not directly effected by option settings)
        if (GameManager.i.optionScript.isAI == true)
        { GameManager.i.featureScript.ToggleAISideWidget(true); }
        else { GameManager.i.featureScript.ToggleAISideWidget(false); }
        #endregion

        #region GUI
        //
        // - - - GUI
        //
        if (listOfGUIFeaturesToToggleOff != null)
        {
            //set all features true
            GameManager.i.optionScript.isActorLeftMenu = true;
            GameManager.i.optionScript.isActorRightMenu = true;
            GameManager.i.optionScript.isPlayerLeftMenu = true;
            GameManager.i.optionScript.isPlayerRightMenu = true;
            GameManager.i.optionScript.isNodeLeftMenu = true;
            GameManager.i.optionScript.isNodeRightMenu = true;
            GameManager.i.optionScript.isTopWidget = true;
            //turn OFF any features in list
            for (int i = 0; i < listOfGUIFeaturesToToggleOff.Count; i++)
            {
                TutorialGUIFeature feature = listOfGUIFeaturesToToggleOff[i];
                if (feature != null)
                {
                    switch (feature.name)
                    {
                        case "ActorLeftMenu":
                            GameManager.i.optionScript.isActorLeftMenu = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: ActorLeftMenu Off{0}", "\n");
                            break;
                        case "ActorRightMenu":
                            GameManager.i.optionScript.isActorRightMenu = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: ActorRightMenu toggled Off{0}", "\n");
                            break;
                        case "PlayerLeftMenu":
                            GameManager.i.optionScript.isPlayerLeftMenu = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: PlayerLeftMenu toggled Off{0}", "\n");
                            break;
                        case "PlayerRightMenu":
                            GameManager.i.optionScript.isPlayerRightMenu = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: PlayerRightMenu toggled Off{0}", "\n");
                            break;
                        case "NodeLeftMenu":
                            GameManager.i.optionScript.isNodeLeftMenu = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: NodeLeftMenu toggled Off{0}", "\n");
                            break;
                        case "NodeRightMenu":
                            GameManager.i.optionScript.isNodeRightMenu = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: NodeRightMenu toggled Off{0}", "\n");
                            break;
                        case "TopWidget":
                            //note: placed here for message purposes only -> dealt with below
                            GameManager.i.optionScript.isTopWidget = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: TopWidget toggled Off{0}", "\n");
                            break;
                        default: Debug.LogWarningFormat("Unrecognised feature.name \"{0}\"", feature.name); break;
                    }
                }
                else { Debug.LogWarningFormat("Invalid feature (Null) for listOfGUIFeaturesToToggleOff[{0}]", i); }
            }
            //Top widget (toggle either on/off but do so after option has been set above)
            GameManager.i.widgetTopScript.SetWidget(GameManager.i.optionScript.isTopWidget);
        }
        else { Debug.LogError("Invalid listOfGUIFeaturesToToggleOff (Null)"); }
        #endregion

    }
    #endregion

    #region Goals...

    #region UpdateGoal
    /// <summary>
    /// Convert and Load a player selected goal (they've clicked on a tutorial goal button, RHS) from the current TutorialSet into DM -> listOfCurrentGoals
    /// </summary>
    public void UpdateGoal(TutorialGoal goal, int index)
    {
        if (goal != null)
        {
            List<GoalTracker> listOfGoals = GameManager.i.dataScript.GetListOfTutorialGoals();
            if (listOfGoals != null)
            {
                GoalType goalPrimary = GetGoalType(goal.goal0);
                if (goalPrimary != GoalType.None)
                {
                    //check TutorialGoal not already present
                    if (listOfGoals.Exists(x => x.goalName.Equals(goal.name, StringComparison.Ordinal)) == true)
                    {
                        Debug.LogWarningFormat("TutorialManager.cs -> UpdateGoal: Can't add goal \"{0}\",  as already present in listOfGoals -> INFO only{1}", goal.name, "\n");
                        //warning message
                        GameManager.i.guiScript.SetAlertMessageModalOne(AlertType.TutorialGoal);
                    }
                    else
                    {
                        //secondary goal (optional)
                        GoalType goalSecondary = GoalType.None;
                        if (goal.goal1 != null)
                        { GetGoalType(goal.goal1); }
                        //Convert to a GoalTracker
                        GoalTracker tracker = new GoalTracker()
                        {
                            startTop = goal.startTopText,
                            startBottom = goal.startBottomText,
                            finishTop = goal.finishTopText,
                            finishBottom = goal.finishBottomText,
                            index = index,
                            goalName = goal.name,
                            goal0 = goalPrimary,
                            goal1 = goalSecondary,
                            data0 = GetGoalValue(goalPrimary),
                            data1 = GetGoalValue(goalSecondary),
                            target0 = goal.target0,
                            target1 = goal.target1,
                            eventType = GetGoalEvent(goalPrimary)
                        };

                        listOfGoals.Add(tracker);
                        Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateGoals: goal \"{0}\" added to list (goal0 {1}, data0 {2}, target0 {3} goal1 {4} -> data1 {5}, target1 {6}){7}", tracker.goalName,
                            tracker.goal0, tracker.data0, tracker.target0, tracker.goal1, tracker.data1, tracker.target1, "\n");
                        //open special outcome window
                        ModalOutcomeDetails details = new ModalOutcomeDetails()
                        {
                            side = GameManager.i.sideScript.PlayerSide,
                            textTop = GameManager.Formatt(goal.startTopText, ColourType.moccasinText),
                            textBottom = goal.startBottomText,
                            sprite = GameManager.i.tutorialScript.tutorial.sprite,
                            isAction = false,
                            isSpecial = true,
                            isSpecialGood = true
                        };
                        EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
                    }
                }
                else { Debug.LogWarning("Invalid goalType (GoalType.None)"); }
            }
            else { Debug.LogError("Invalid listOfTutorialGoals (Null)"); }
        }
        else { Debug.LogError("Invalid (Tutorial) goal (Null)"); }
    }
    #endregion

    #region CheckGoals
    /// <summary>
    /// Check any current tutorial goals to see if they have been achieved
    /// </summary>
    public void CheckGoals()
    {
        if (GameManager.i.dataScript.CheckNumberOfCurrentGoals() > 0)
        {
            int currentValue;
            bool isGoalAchieved = false;
            //loop list of goals
            List<GoalTracker> listOfGoals = GameManager.i.dataScript.GetListOfTutorialGoals();
            if (listOfGoals != null)
            {
                //reverse loop as may have to remove completed goals
                for (int i = listOfGoals.Count - 1; i >= 0; i--)
                {
                    GoalTracker tracker = listOfGoals[i];
                    if (tracker != null)
                    {
                        //check primary goal
                        currentValue = GetGoalValue(tracker.goal0);
                        if (currentValue > -1)
                        {
                            if (currentValue - tracker.data0 >= tracker.target0)
                            {
                                //check secondary goal, if present
                                if (tracker.goal1 != GoalType.None)
                                {
                                    currentValue = GetGoalValue(tracker.goal1);
                                    if (currentValue > -1)
                                    {
                                        if (currentValue - tracker.data1 >= tracker.target1)
                                        { isGoalAchieved = true; }
                                    }
                                    else { Debug.LogWarningFormat("Invalid currentValue (-1) for goal \"{0}\", Secondary goal {1}{2}", tracker.goalName, tracker.goal1, "\n"); }
                                }
                                else { isGoalAchieved = true; }
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid currentValue (-1) for goal \"{0}\", Primary goal {1}{2}", tracker.goalName, tracker.goal0, "\n"); }
                        //goal achieved
                        if (isGoalAchieved == true)
                        {
                            Debug.LogFormat("[Tut] TutorialManager.cs -> CheckGoals: Goal \"{0}\" COMPLETED (goal0 {1}, data0 {2}, target0 {3}, goal1 {4}, data1 {5}, target1 {6}){7}",
                                tracker.goalName, tracker.goal0, tracker.data0, tracker.target0, tracker.goal1, tracker.data1, tracker.target1, "\n");
                            //activate event (eg. close an underlying UI before showing outcome)
                            if (tracker.eventType != EventType.None)
                            { EventManager.i.PostNotification(tracker.eventType, this, null); }
                            //open special outcome window
                            ModalOutcomeDetails details = new ModalOutcomeDetails()
                            {
                                side = GameManager.i.sideScript.PlayerSide,
                                textTop = GameManager.Formatt(tracker.finishTop, ColourType.moccasinText),
                                textBottom = tracker.finishBottom,
                                sprite = GameManager.i.tutorialScript.tutorial.sprite,
                                isAction = false,
                                isSpecial = true,
                                isSpecialGood = true,
                                isTutorial = true
                            };
                            EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
                            //update tutorial item on RHS
                            GameManager.i.tutorialUIScript.SetGoalDone(tracker.index);
                            //remove goal
                            Debug.LogFormat("[Tut] DataManager.cs -> RemoveCurrentGoal: goal \"{0}\" REMOVED from listOfCurrentGoals{1}", tracker.goalName, "\n");
                            listOfGoals.RemoveAt(i);
                        }
                        else
                        {
                            Debug.LogFormat("[Tut] TutorialManager.cs -> CheckGoals: Goal \"{0}\" In Progress (goal0 {1}, data0 {2}, target0 {3}, goal1 {4}, data1 {5}, target1 {6}){7}",
                                tracker.goalName, tracker.goal0, tracker.data0, tracker.target0, tracker.goal1, tracker.data1, tracker.target1, "\n");
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid goalTracker (Null) in listOfGoals[{0}]", i); }
                }
            }
            else { Debug.LogError("Invalid listOfCurrentGoals (Null)"); }
        }
    }
    #endregion

    #region GetGoalType
    /// <summary>
    /// Converts a TutorialGoal.SO primary or secondary goal into a GoalType.enum. Returns GoalType.None if a problem
    /// </summary>
    /// <param name="goal"></param>
    /// <returns></returns>
    private GoalType GetGoalType(TutorialGoalType goal)
    {
        GoalType goalType = GoalType.None;
        if (goal != null)
        {
            switch (goal.name)
            {
                case "MoveBasic": goalType = GoalType.Move; break;
                case "MoveSecurity": goalType = GoalType.MoveSecurity; break;
                case "SubordinateNodeAction": goalType = GoalType.SubordinateNodeActions; break;
                case "SubordinateRecruit": goalType = GoalType.SubordinateRecruit; break;
                case "SubordinateDismiss": goalType = GoalType.SubordinateDismiss; break;
                case "SubordinateActivate": goalType = GoalType.SubordinateActivate; break;
                case "PlannerInfo": goalType = GoalType.SubordinatePlanner; break;
                case "SubordinateGear": goalType = GoalType.SubordinateGear; break;
                case "PlayerInvisibility": goalType = GoalType.PlayerInvisibility; break;
                case "PlayerNodeAction": goalType = GoalType.PlayerNodeActions; break;
                case "PlayerRecruit": goalType = GoalType.PlayerRecruit; break;
                case "PlayerTarget": goalType = GoalType.PlayerTarget; break;
                case "GiveGear": goalType = GoalType.PlayerGiveGear; break;
                case "SandboxRescue": goalType = GoalType.SandboxRescue; break;
                case "SandboxEvacuate": goalType = GoalType.SandboxEvacuate; break;
                default: Debug.LogWarningFormat("Unrecognised goal \"{0}\"", goal); break;
            }
        }
        else { Debug.LogError("Invalid TutorialGoalType (Null)"); }
        return goalType;
    }
    #endregion

    #region GetGoalValue
    /// <summary>
    /// Returns current value of a goalType, -1 if a problem
    /// </summary>
    /// <param name="goalType"></param>
    /// <returns></returns>
    private int GetGoalValue(GoalType goalType)
    {
        int goalValue = -1;
        switch (goalType)
        {
            case GoalType.Move:
                //number of times player has moved
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerMoveActions);
                break;
            case GoalType.MoveSecurity:
                //moved over a secure connection
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerMoveSecureConnections);
                break;
            case GoalType.SubordinateNodeActions:
                //subordinate node actions (excluding target attempts)
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.SubordinateNodeActions);
                break;
            case GoalType.SubordinateRecruit:
                //subordinate recruits somebody
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.ActorsRecruited);
                break;
            case GoalType.SubordinateGear:
                //subordinate sources gear
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.GearTotal);
                break;
            case GoalType.SubordinateDismiss:
                //subordinate is dismissed from onMap lineUp
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerManageReserve);
                break;
            case GoalType.SubordinateActivate:
                //subordinate is activated from Reserves to OnMap
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.SubordinateActivate);
                break;
            case GoalType.SubordinatePlanner:
                //planner gains target info
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.TargetInfo);
                break;
            case GoalType.PlayerInvisibility:
                //player lost invisibility
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerInvisibilityLost);
                break;
            case GoalType.PlayerNodeActions:
                //player node actions (excluding target attempts)
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerNodeActions);
                break;
            case GoalType.PlayerRecruit:
                //player recruits somebody
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.ActorsRecruited);
                break;
            case GoalType.PlayerGiveGear:
                //player gives gear to a subordinate
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerGiveGear);
                break;
            case GoalType.PlayerTarget:
                //player attempts target
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.PlayerTargetAttempts);
                break;
            case GoalType.SandboxRescue:
                //player attempts first sandbox goal target (only 1 target on map)
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.TargetSuccesses);
                break;
            case GoalType.SandboxEvacuate:
                //player attempts second sandbox goal target (only 1 target on map)
                goalValue = GameManager.i.dataScript.StatisticGetLevel(StatType.TargetSuccesses);
                break;
            case GoalType.None: break;
            default: Debug.LogWarningFormat("Unrecognised goalType \"{0}\"", goalType); break;
        }
        return goalValue;
    }
    #endregion

    #region GetGoalEvent
    /// <summary>
    /// Allows you to specify an event that will run prior to the goal outcome dialogue. Used to close an underlying UI. Defaults to none.
    /// NOTE: Be careful here. Don't run any events that are going to cause a conflict with ModalOutcome.
    /// </summary>
    /// <param name="goalType"></param>
    /// <returns></returns>
    private EventType GetGoalEvent(GoalType goalType)
    {
        EventType eventType = EventType.None;
        switch (goalType)
        {
            case GoalType.SubordinateActivate:
            case GoalType.PlayerGiveGear:
                eventType = EventType.InventoryCloseUI;
                break;
        }
        return eventType;
    }
    #endregion

    #endregion

    #region QueryOptions
    //
    // - - - TutorialItem -> Query -> Options
    //

    #region ProcessTutorialOption
    /// <summary>
    /// Process selected option from TopicUI.cs
    /// </summary>
    /// <param name="option"></param>
    public void ProcessTutorialOption(TopicOption option)
    {
        bool isErrorFlag = false;
        if (option != null)
        {
            string topText = "Unknown";
            string bottomText = "Unknown";
            EffectDataReturn effectReturn = new EffectDataReturn();
            //needed for EffectManager.cs code
            EffectDataInput dataInput = new EffectDataInput();
            dataInput.queryType = queryType;
            //Process Effect (Only the first effect in listOfGoodEffects is processed, the rest are ignored)
            Effect effect = option.listOfGoodEffects[0];
            if (effect != null)
            {
                //use player node (need a node for EffectManager.cs code)
                Node node = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
                if (node != null)
                {
                    //process effect
                    effectReturn = GameManager.i.effectScript.ProcessEffect(effect, node, dataInput);
                    if (effectReturn != null)
                    {
                        //top text
                        if (string.IsNullOrEmpty(effectReturn.topText) == false)
                        { topText = effectReturn.topText; }
                        //bottom text
                        if (string.IsNullOrEmpty(effectReturn.bottomText) == false)
                        { bottomText = string.Format("{0}", effectReturn.bottomText); }
                    }
                    else
                    {
                        Debug.LogWarningFormat("Invalid effectReturn (Null)");
                        isErrorFlag = true;
                    }
                }
                else { Debug.LogWarningFormat("Effect \"{0}\" not processed as invalid Node (Null) for option \"{1}\"", effect.name, option.name); }
            }
            else { Debug.LogError("Invalid effect (Null) for option.listOfGoodEffects[0]"); }
            //Output
            if (isErrorFlag == false)
            {
                /*
                //repeat prevention
                option.tutorialItem.isQueryDone = true;
                */

                //Outcome
                ModalOutcomeDetails details = new ModalOutcomeDetails()
                {
                    side = GameManager.i.sideScript.PlayerSide,
                    textTop = topText,
                    textBottom = bottomText,
                    sprite = tutorial.sprite,
                    isSpecial = true,
                    isSpecialGood = true
                };
                EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
            }
            else
            {
                //no outcome -> Error
                Debug.LogWarningFormat("No Outcome for option \"{0}\" (isErrorFlag true)", option.name);
            }

        }
        else { Debug.LogError("Invalid option (Null)"); }
    }
    #endregion

    #region ProcessTutorialIgnoreOption
    /// <summary>
    /// Process Ignore option from TopicUI.cs
    /// </summary>
    /// <param name="option"></param>
    public void ProcessTutorialIgnoreOption(List<Effect> listOfIgnoreEffects)
    {
        bool isErrorFlag = false;
        if (listOfIgnoreEffects != null)
        {
            string topText = "Unknown";
            string bottomText = "Unknown";
            EffectDataReturn effectReturn = new EffectDataReturn();
            //needed for EffectManager.cs code
            EffectDataInput dataInput = new EffectDataInput();
            dataInput.queryType = queryType;
            //Process Effect (Only the first effect in listOfIgnoreEffects is processed, the rest are ignored)
            Effect effect = listOfIgnoreEffects[0];
            if (effect != null)
            {
                //use player node (need a node for EffectManager.cs code)
                Node node = GameManager.i.dataScript.GetNode(GameManager.i.nodeScript.GetPlayerNodeID());
                if (node != null)
                {
                    //process effect
                    effectReturn = GameManager.i.effectScript.ProcessEffect(effect, node, dataInput);
                    if (effectReturn != null)
                    {
                        //top text
                        if (string.IsNullOrEmpty(effectReturn.topText) == false)
                        { topText = effectReturn.topText; }
                        //bottom text
                        if (string.IsNullOrEmpty(effectReturn.bottomText) == false)
                        { bottomText = string.Format("{0}", effectReturn.bottomText); }
                    }
                    else
                    {
                        Debug.LogWarningFormat("Invalid effectReturn (Null)");
                        isErrorFlag = true;
                    }
                }
                else { Debug.LogWarningFormat("Effect \"{0}\" not processed as invalid Node (Null) for IGNORE", effect.name); }
            }
            else { Debug.LogError("Invalid effect (Null) for option.listOfIgnoreEffects[0]"); }
            //Output
            if (isErrorFlag == false)
            {
                /*
                //repeat prevention
                option.tutorialItem.isQueryDone = true;
                */

                //Outcome
                ModalOutcomeDetails details = new ModalOutcomeDetails()
                {
                    side = GameManager.i.sideScript.PlayerSide,
                    textTop = topText,
                    textBottom = bottomText,
                    sprite = tutorial.sprite,
                    isSpecial = true,
                    isSpecialGood = true
                };
                EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
            }
            else
            {
                //no outcome -> Error
                Debug.LogWarning("No Outcome for IGNORE option  (isErrorFlag true)");
            }

        }
        else { Debug.LogError("Invalid listOfIgnoreEffects (Null)"); }
    }
    #endregion

    #region GetTutorialTooltip
    /// <summary>
    /// Get a tooltip.main text for a TutorialOption
    /// </summary>
    /// <returns></returns>
    public string GetTutorialTooltip(TutorialQueryType queryType)
    {
        string tooltip = "Unknown";
        switch (queryType.name)
        {
            case "Sex": tooltip = textListGeneric.GetRandomRecord(); break;
            case "Name":
                if (Random.Range(0, 100) < queryOptionTooltipChance)
                { tooltip = textListGeneric.GetRandomRecord(); }
                else { tooltip = "You'll have the opportunity to change this later"; }
                break;
            case "Job":
                if (Random.Range(0, 100) < queryOptionTooltipChance)
                { tooltip = textListGeneric.GetIndexedRecord(); }
                else { tooltip = "You'll have the opportunity to change this later"; }
                break;
            case "Pet":
            case "PetName":
                if (Random.Range(0, 100) < queryOptionTooltipChance)
                { tooltip = textListGeneric.GetRandomRecord(); }
                else { tooltip = "You'll have the opportunity to change this later"; }
                break;
            case "Secret":
                if (Random.Range(0, 100) < queryOptionTooltipChance)
                { tooltip = textListGeneric.GetRandomRecord(); }
                else { tooltip = "We all have things that we'd prefer to keep private"; }
                break;
            case "Personality":
                if (Random.Range(0, 100) < queryOptionTooltipChance)
                { tooltip = textListGeneric.GetRandomRecord(); }
                else { tooltip = "Our Psychiatrist will be examining you"; }
                break;
            case "Reason":
                if (Random.Range(0, 100) < queryOptionTooltipChance)
                { tooltip = textListGeneric.GetRandomRecord(); }
                else { tooltip = "Rest easy, we'll take anyone"; }
                break;
            default: Debug.LogWarningFormat("Unrecognised queryType \"{0}\"", queryType.name); break;
        }
        return tooltip;
    }
    #endregion

    #endregion

    #region SetTutorialCondition
    /// <summary>
    /// Sets up a condition (could be anything you can do via code) activated when item is triggered
    /// </summary>
    /// <param name="condition"></param>
    public void SetTutorialCondition(TutorialCondition condition)
    {
        if (condition != null)
        {
            switch (condition.name)
            {
                case "Sandbox":
                    Debug.LogFormat("[Tst] SetTutorialCondition -> SANDBOX");
                    break;
                default: Debug.LogWarningFormat("Unrecognised condition \"{0}\"", condition.name); break;
            }
        }
        else { Debug.LogError("Invalid condition (Null)"); }
    }
    #endregion

    #region Widget Interaction...

    #region SetPreviousSet
    /// <summary>
    /// Back to previous tutorialSet (left arrow of tutorial Widget pressed)
    /// </summary>
    private void SetPreviousSet()
    {
        //flush input buffer
        Input.ResetInputAxes();
        if (GameManager.i.inputScript.ModalState == ModalState.Normal)
        {

            Debug.LogFormat("[Tut] TutorialManager.cs -> SetPreviousSet: Go BACK one TutorialSet{0}", "\n");
            if (index > 0)
            {
                index--;
                set = tutorial.listOfSets[index];
                if (set != null)
                {
                    InitialiseTutorialSet(set);
                    //reset contact dictionaries
                    GameManager.i.dataScript.TutorialResetContacts();
                    //configure player
                    GameManager.i.playerScript.ResetTutorialPlayer();
                    if (set.playerConfig != null)
                    { GameManager.i.playerScript.ConfigureTutorialPlayer(set.playerConfig); }
                    //configure actors
                    GameManager.i.actorScript.ConfigureTutorialActors(true);
                    //configure targets
                    if (set.targetConfig != null)
                    { GameManager.i.targetScript.ConfigureTutorialTargets(set.targetConfig); }
                    //configure teams
                    if (set.teamConfig != null)
                    { GameManager.i.teamScript.ConfigureTutorialTeams(set.teamConfig); }
                    //configure spiders and tracers
                    if (set.hideConfig != null)
                    { GameManager.i.nodeScript.ConfigureTutorialHideItems(set.hideConfig); }
                    //reset nodes and close AlertUI
                    GameManager.i.alertScript.CloseAlertUI(true);
                    //activate tutorialUI
                    EventManager.i.PostNotification(EventType.TutorialOpenUI, this, set, "TutorialManager.cs -> SetPreviousSet");
                }
                else { Debug.LogErrorFormat("Invalid set (Null) for tutorial \"{0}\" listOfSets[{1}]", tutorial.name, index); }
            }
            else
            {
                //at the beginning of the tutorial -> Message
                ModalOutcomeDetails details = new ModalOutcomeDetails()
                {
                    side = GameManager.i.sideScript.PlayerSide,
                    textTop = string.Format("{0}", GameManager.Formatt("We're back where we started", ColourType.neutralText)),
                    textBottom = string.Format("Nothing wrong with that, it's good to review what you've learnt<br><br>{0}", GameManager.Formatt("You are free to move around the Tutorial", ColourType.salmonText)),
                    sprite = tutorial.sprite,
                    isSpecial = true,
                    isSpecialGood = true
                };
                EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
            }
        }
    }
    #endregion

    #region SetNextSet
    /// <summary>
    /// Go to the Next tutorialSet (right arrow of tutorial Widget pressed)
    /// </summary>
    private void SetNextSet()
    {
        //flush input buffer
        Input.ResetInputAxes();
        if (GameManager.i.inputScript.ModalState == ModalState.Normal)
        {
            Debug.LogFormat("[Tut] TutorialManager.cs -> SetNextSet: Go FORWARD one TutorialSet{0}", "\n");
            index++;
            if (index < tutorial.listOfSets.Count)
            {
                set = tutorial.listOfSets[index];
                if (set != null)
                {
                    InitialiseTutorialSet(set);
                    //reset contact dictionaries
                    GameManager.i.dataScript.TutorialResetContacts();
                    //configure player
                    GameManager.i.playerScript.ResetTutorialPlayer();
                    if (set.playerConfig != null)
                    { GameManager.i.playerScript.ConfigureTutorialPlayer(set.playerConfig); }
                    //configure actors
                    GameManager.i.actorScript.ConfigureTutorialActors(true);
                    //configure targets
                    if (set.targetConfig != null)
                    { GameManager.i.targetScript.ConfigureTutorialTargets(set.targetConfig); }
                    //configure teams
                    if (set.teamConfig != null)
                    { GameManager.i.teamScript.ConfigureTutorialTeams(set.teamConfig); }
                    //configure spiders and tracers
                    if (set.hideConfig != null)
                    { GameManager.i.nodeScript.ConfigureTutorialHideItems(set.hideConfig); }
                    //reset nodes and close AlertUI
                    GameManager.i.alertScript.CloseAlertUI(true);
                    //activate tutorialUI
                    EventManager.i.PostNotification(EventType.TutorialOpenUI, this, set, "TutorialManager.cs -> SetNextSet");
                }
                else { Debug.LogErrorFormat("Invalid set (Null) for tutorial \"{0}\" listOfSets[{1}]", tutorial.name, index); }
            }
            else
            {
                //maxxed out sets
                index--;
                //Message
                ModalOutcomeDetails details = new ModalOutcomeDetails()
                {
                    side = GameManager.i.sideScript.PlayerSide,
                    textTop = string.Format("Well that's it. Consider yourself {0}", GameManager.Formatt("trained and ready", ColourType.neutralText)),
                    textBottom = string.Format("Don't go embarrassing me now and get yourself killed<br><br>{0}", GameManager.Formatt("Press ESC to return to the Main Menu", ColourType.salmonText)),
                    sprite = tutorial.sprite,
                    isSpecial = true,
                    isSpecialGood = true
                };
                EventManager.i.PostNotification(EventType.OutcomeOpen, this, details);
            }
        }
    }
    #endregion

    #endregion

    #region Utilities
    //
    // - - - Utilities
    //
    #region GetActorPool
    /// <summary>
    /// ActorManager.cs -> LoadActorPool calls this to access relevant pool
    /// </summary>
    /// <returns></returns>
    public ActorPoolFinal GetActorPool()
    { return tutorial.pool; }
    #endregion

    #region GetActorConfiguration
    /// <summary>
    /// returns actorConfig.SO for current tutorial set. Null if none present (possible)
    /// </summary>
    /// <returns></returns>
    public TutorialActorConfig GetActorConfiguration()
    { return set.actorConfig; }

    #region GetTutorialSide
    /// <summary>
    /// Tutorial side
    /// </summary>
    /// <returns></returns>
    public GlobalSide GetTutorialSide()
    { return tutorial.side; }
    #endregion

    #endregion

    #region GetListOfDefaultActorArcs
    /// <summary>
    /// returns list of actor Arcs for the default onMap line up in the absence of a specific tutorialSet.actorConfig
    /// </summary>
    /// <returns></returns>
    public List<ActorArc> GetListOfDefaultActorArcs()
    { return tutorial.listOfDefaultArcs; }
    #endregion

    #region DebugDisplayTutorialData
    /// <summary>
    /// Display all relevant tutorial data
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTutorialData()
    {
        if (GameManager.i.inputScript.GameState == GameState.Tutorial)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("-Tutorial Data{0}{1}", "\n", "\n");
            builder.AppendFormat("tutorial: {0}{1}", tutorial.name, "\n");
            builder.AppendFormat("tutorialSet: {0}{1}", set.name, "\n");
            builder.AppendFormat("index: {0}{1}", index, "\n");

            //current tutorialSet -> Features Off
            builder.AppendFormat("{0}-features OFF for \"{1}\"{2}", "\n", set.name, "\n");
            for (int i = 0; i < set.listOfFeaturesOff.Count; i++)
            { builder.AppendFormat(" {0}{1}", set.listOfFeaturesOff[i].name, "\n"); }

            //current tutorialSet -> GUI Features Off
            builder.AppendFormat("{0}-GUI features OFF for \"{1}\"{2}", "\n", set.name, "\n");
            for (int i = 0; i < set.listOfGUIOff.Count; i++)
            { builder.AppendFormat(" {0}{1}", set.listOfGUIOff[i].name, "\n"); }

            //tutorialSet -> current Goals
            List<GoalTracker> listOfGoals = GameManager.i.dataScript.GetListOfTutorialGoals();
            if (listOfGoals != null)
            {
                builder.AppendFormat("{0}-ACTIVE Goals for \"{1}\"{2}", "\n", set.name, "\n");
                int count = listOfGoals.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        GoalTracker goal = listOfGoals[i];
                        if (goal != null)
                        {
                            builder.AppendFormat(" {0} -> {1}{2}  goal0 {3}, data0 {4}, target {5}{6}  goal1 {7} -> data1 {8}, target1 {9}{10} ",
                                goal.goalName, goal.startTop, "\n", goal.goal0, goal.data0, goal.target0, "\n", goal.goal1, goal.data1, goal.target1, "\n");
                        }
                        else { Debug.LogErrorFormat("Invalid goal (Null) for listOfGoals[{0}]", i); }
                    }
                }
                else { builder.AppendFormat(" No goals currently active for this set{0}", "\n"); }
            }
            else { Debug.LogError("Invalid listOfTutorialGoals (Null)"); }

            //tutorialSet -> Goals
            builder.AppendFormat("{0}-Goals for \"{1}\"{2}", "\n", set.name, "\n");
            for (int i = 0; i < set.listOfTutorialItems.Count; i++)
            {
                TutorialItem item = set.listOfTutorialItems[i];
                if (item.tutorialType.name.Equals("Goal", StringComparison.Ordinal) == true)
                {
                    TutorialGoal goal = item.goal;
                    builder.AppendFormat(" {0} -> {1}, Tgt {2} -> {3}, Tgt {4}{5}", goal.name, goal.goal0.name, goal.target0, goal.goal1 == null ? "None" : goal.goal1.name,
                        goal.target1 == -1 ? "n.a" : Convert.ToString(goal.target1), "\n");
                }
            }

            //current tutorial -> Sets
            builder.AppendFormat("{0}-tutorialSets for \"{1}\"{2}", "\n", tutorial.name, "\n");
            for (int i = 0; i < tutorial.listOfSets.Count; i++)
            { builder.AppendFormat(" {0} -> index {1}{2}", tutorial.listOfSets[i].name, i, "\n"); }

            //dictOfTutorialData
            Dictionary<string, TutorialData> dictOfData = GameManager.i.dataScript.GetDictOfTutorialData();
            if (dictOfData != null)
            {
                builder.AppendFormat("{0}-dictOfTutorialData{1}", "\n", "\n");
                if (dictOfData.Count > 0)
                {
                    foreach (var data in dictOfData)
                    { builder.AppendFormat(" {0} -> index {1}{2}", data.Value.tutorialName, data.Value.index, "\n"); }
                }
                else { builder.AppendLine("No records present"); }
            }
            else { Debug.LogError("Invalid dictOfTutorialData (Null)"); }
            return builder.ToString();
        }
        else { return "You must be in Tutorial mode to access this information"; }
    }
    #endregion

    #region GetNumberOfSets
    /// <summary>
    /// returns total number of sets within current tutorial
    /// </summary>
    /// <returns></returns>
    public int GetNumberOfSets()
    { return tutorial.listOfSets.Count; }
    #endregion

    #endregion

    //new methods above here
}
