using delegateAPI;
using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;



/// <summary>
/// Handles all action related matters
/// </summary>
public class ActionManager : MonoBehaviour
{

    [Header("Text Lists")]
    [Tooltip("List of buildings (generic) that an anarchist might Blow Up. Used for node Action / Topic immersion")]
    public TextList textlistBlowUpBuildings;

    //fast access -> target
    private int failedTargetChance;
    //lie low
    private int lieLowPeriod;
    //conditions
    private Condition conditionStressed;
    private Condition conditionUnhappy;
    //gear
    private int gearGracePeriod = -1;
    private int gearSwapBaseAmount = -1;
    private int gearSwapPreferredAmount = -1;
    //traits
    private string actorStressedDuringSecurity;
    private string actorKeepGear;
    private string actorReserveTimerDoubled;
    private string actorReserveTimerHalved;
    private string actorReserveActionDoubled;

    //colour palette for Modal Outcome
    private string colourNormal;
    private string colourError;
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourAlert;
    private string colourInvalid;
    private string colourResistance;
    private string colourAuthority;
    private string colourGrey;
    private string colourEnd;

    /// <summary>
    /// Not for GameState.Load
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseStartUp();
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                break;
            case GameState.LoadAtStart:
                SubInitialiseStartUp();
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseStartUp
    /// <summary>
    /// Startup initialisation
    /// </summary>
    private void SubInitialiseStartUp()
    {
        Debug.Assert(textlistBlowUpBuildings != null, "Invalid textListBlowUpBuildings (Null)");
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access fields
        failedTargetChance = GameManager.instance.aiScript.targetAttemptChance;
        lieLowPeriod = GameManager.instance.actorScript.lieLowCooldownPeriod;
        conditionStressed = GameManager.instance.dataScript.GetCondition("STRESSED");
        conditionUnhappy = GameManager.instance.dataScript.GetCondition("UNHAPPY");
        actorStressedDuringSecurity = "ActorStressSecurity";
        actorKeepGear = "ActorKeepGear";
        actorReserveTimerDoubled = "ActorReserveTimerDoubled";
        actorReserveTimerHalved = "ActorReserveTimerHalved";
        actorReserveActionDoubled = "ActorReserveActionDoubled";
        gearGracePeriod = GameManager.instance.gearScript.actorGearGracePeriod;
        gearSwapBaseAmount = GameManager.instance.gearScript.gearSwapBaseAmount;
        gearSwapPreferredAmount = GameManager.instance.gearScript.gearSwapPreferredAmount;
        Debug.Assert(failedTargetChance > 0, string.Format("Invalid failedTargetChance {0}", failedTargetChance));
        Debug.Assert(lieLowPeriod > 0, "Invalid lieLowCooldDownPeriod (Zero)");
        Debug.Assert(conditionStressed != null, "Invalid conditionStressed (Null)");
        Debug.Assert(conditionUnhappy != null, "Invalid conditionUnhappy (Null)");
        Debug.Assert(gearGracePeriod > -1, "Invalid gearGracePeriod (-1)");
        Debug.Assert(gearSwapBaseAmount > -1, "Invalid gearSwapBaseAmount (-1)");
        Debug.Assert(gearSwapPreferredAmount > -1, "Invalid gearSwapPreferredAmount (-1)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.instance.AddListener(EventType.NodeAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.NodeGearAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.TargetAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.LieLowActorAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.LieLowPlayerAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.ActivateActorAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.ActivatePlayerAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.ManageActorAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.GiveGearAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.TakeGearAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.UseGearAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.InsertTeamAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.GenericHandleActor, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.GenericReserveActor, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.GenericDismissActor, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.GenericDisposeActor, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.InventoryReassure, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.InventoryThreaten, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.InventoryActiveDuty, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.InventoryLetGo, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.InventoryFire, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.LeavePlayerAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.LeaveActorAction, OnEvent, "ActionManager");
        EventManager.instance.AddListener(EventType.CurePlayerAction, OnEvent, "ActionManager");
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
            case EventType.NodeAction:
                ModalActionDetails detailsNode = Param as ModalActionDetails;
                ProcessNodeAction(detailsNode);
                break;
            case EventType.NodeGearAction:
                ModalActionDetails detailsGear = Param as ModalActionDetails;
                ProcessNodeGearAction(detailsGear);
                break;
            case EventType.InsertTeamAction:
                ModalActionDetails detailsTeam = Param as ModalActionDetails;
                ProcessTeamAction(detailsTeam);
                break;
            case EventType.TargetAction:
                ProcessNodeTarget((int)Param);
                break;
            case EventType.ManageActorAction:
                ModalActionDetails detailsDismiss = Param as ModalActionDetails;
                ProcessManageActorAction(detailsDismiss);
                break;
            case EventType.LieLowActorAction:
                ModalActionDetails detailsLieLowActor = Param as ModalActionDetails;
                ProcessLieLowActorAction(detailsLieLowActor);
                break;
            case EventType.LieLowPlayerAction:
                ModalActionDetails detailsLieLowPlayer = Param as ModalActionDetails;
                ProcessLieLowPlayerAction(detailsLieLowPlayer);
                break;
            case EventType.LeavePlayerAction:
                ModalActionDetails detailsLeavePlayer = Param as ModalActionDetails;
                ProcessStressLeavePlayerAction(detailsLeavePlayer);
                break;
            case EventType.LeaveActorAction:
                ModalActionDetails detailsLeaveActor = Param as ModalActionDetails;
                ProcessStressLeaveActorAction(detailsLeaveActor);
                break;
            case EventType.ActivateActorAction:
                ModalActionDetails detailsActivateActor = Param as ModalActionDetails;
                ProcessActivateActorAction(detailsActivateActor);
                break;
            case EventType.ActivatePlayerAction:
                ModalActionDetails detailsActivatePlayer = Param as ModalActionDetails;
                ProcessActivatePlayerAction(detailsActivatePlayer);
                break;
            case EventType.CurePlayerAction:
                ModalActionDetails detailsCurePlayer = Param as ModalActionDetails;
                ProcessPlayerCure(detailsCurePlayer);
                break;
            case EventType.GiveGearAction:
                ModalActionDetails detailsGiveGear = Param as ModalActionDetails;
                ProcessGiveGearAction(detailsGiveGear);
                break;
            case EventType.TakeGearAction:
                ModalActionDetails detailsTakeGear = Param as ModalActionDetails;
                ProcessTakeGearAction(detailsTakeGear);
                break;
            case EventType.UseGearAction:
                ModalActionDetails detailsUseGear = Param as ModalActionDetails;
                ProcessUseGearAction(detailsUseGear);
                break;
            case EventType.InventoryActiveDuty:
                ModalActionDetails detailsActive = Param as ModalActionDetails;
                ProcessActiveDutyActor(detailsActive);
                break;
            case EventType.InventoryReassure:
                ModalActionDetails detailsReassure = Param as ModalActionDetails;
                ProcessReassureActor(detailsReassure);
                break;
            case EventType.InventoryThreaten:
                ModalActionDetails detailsThreaten = Param as ModalActionDetails;
                ProcessBullyActor(detailsThreaten);
                break;
            case EventType.InventoryLetGo:
                ModalActionDetails detailsLetGo = Param as ModalActionDetails;
                ProcessLetGoActor(detailsLetGo);
                break;
            case EventType.InventoryFire:
                ModalActionDetails detailsFire = Param as ModalActionDetails;
                ProcessFireReserveActor(detailsFire);
                break;
            case EventType.GenericHandleActor:
                GenericReturnData returnDataHandle = Param as GenericReturnData;
                ProcessHandleActor(returnDataHandle);
                break;
            case EventType.GenericReserveActor:
                GenericReturnData returnDataReserve = Param as GenericReturnData;
                ProcessReserveActorAction(returnDataReserve);
                break;
            case EventType.GenericDismissActor:
                GenericReturnData returnDataDismiss = Param as GenericReturnData;
                ProcessDismissActorAction(returnDataDismiss);
                break;
            case EventType.GenericDisposeActor:
                GenericReturnData returnDataDispose = Param as GenericReturnData;
                ProcessDisposeActorAction(returnDataDispose);
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
    /// deregister events
    /// </summary>
    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.OpenOutcomeWindow);
    }

    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.blueText);
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourError = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    /// <summary>
    /// Processes node actor actions (Resistance & Authority Node actions)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessNodeAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        bool isAction = false;
        bool isPlayer = false;
        CaptureDetails captureDetails = null;
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        //resolve action
        if (details != null)
        {
            //get Actor
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, GameManager.instance.sideScript.PlayerSide);
            if (actor != null)
            {
                //get node
                GameObject nodeObject = GameManager.instance.dataScript.GetNodeObject(details.nodeID);
                if (nodeObject != null)
                {
                    Node node = nodeObject.GetComponent<Node>();
                    if (node != null)
                    {
                        //check for Resistance player/actor getting captured prior to carrying out action
                        if (details.side.level == GameManager.instance.globalScript.sideResistance.level)
                        {
                            int actorID = actor.actorID;
                            if (node.nodeID == GameManager.instance.nodeScript.nodePlayer) { actorID = 999; isPlayer = true; }
                            captureDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, actorID);
                        }
                        if (captureDetails != null)
                        {
                            //Captured. Mission a wipe.
                            captureDetails.effects = string.Format("{0}Mission at \"{1}\" aborted{2}", colourNeutral, node.nodeName, colourEnd);
                            EventManager.instance.PostNotification(EventType.Capture, this, captureDetails, "ActionManager.cs -> ProcessNodeAction");
                            return;
                        }

                        //Get Action & Effects
                        Action action = actor.arc.nodeAction;
                        List<Effect> listOfEffects = action.GetEffects();
                        if (listOfEffects.Count > 0)
                        {
                            //return class
                            EffectDataReturn effectReturn = new EffectDataReturn();
                            //two builders for top and bottom texts
                            StringBuilder builderTop = new StringBuilder();
                            StringBuilder builderBottom = new StringBuilder();
                            //pass through data package
                            EffectDataInput dataInput = new EffectDataInput();
                            dataInput.originText = action.tag;
                            //
                            // - - - Process effects
                            //
                            foreach (Effect effect in listOfEffects)
                            {
                                //ongoing effect?
                                if (effect.duration.name.Equals("Ongoing", StringComparison.Ordinal) == true)
                                {
                                    //NOTE: A standard node action can use ongoing effects but there is no way of linking it. The node effects will time out and that's it
                                    dataInput.ongoingID = GameManager.instance.effectScript.GetOngoingEffectID();
                                }
                                else { dataInput.ongoingID = -1; dataInput.ongoingText = action.name; }
                                effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actor);
                                if (effectReturn != null)
                                {
                                    outcomeDetails.sprite = actor.sprite;
                                    //update stringBuilder texts
                                    if (string.IsNullOrEmpty(effectReturn.topText) == false)
                                    {
                                        if (builderTop.Length > 0) { builderTop.AppendLine(); builderTop.AppendLine(); }
                                        builderTop.Append(effectReturn.topText);
                                    }
                                    if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                    builderBottom.Append(effectReturn.bottomText);
                                    //exit effect loop on error
                                    if (effectReturn.errorFlag == true) { break; }
                                    //valid action? -> only has to be true once for an action to be valid
                                    if (effectReturn.isAction == true) { isAction = true; }
                                }
                                else
                                {
                                    builderTop.AppendLine();
                                    builderTop.Append("Error");
                                    builderBottom.AppendLine();
                                    builderBottom.Append("Error");
                                    effectReturn.errorFlag = true;
                                    break;
                                }
                            }
                            //Nervous trait gains Stressed condition if security measures are in place
                            if (actor.CheckTraitEffect(actorStressedDuringSecurity) == true && GameManager.instance.turnScript.authoritySecurityState != AuthoritySecurityState.Normal)
                            {
                                //actor gains condition stressed
                                Condition stressed = GameManager.instance.dataScript.GetCondition("STRESSED");
                                if (stressed != null)
                                {
                                    actor.AddCondition(stressed, string.Format("Acquired due to {0} trait", actor.GetTrait().tag));
                                    if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                    builderBottom.AppendFormat("{0}{1} gains {2}{3}STRESSED{4}{5} condition due to {6}{7}{8}{9}{10} trait{11}", colourBad, actor.arc.name, colourEnd,
                                        colourAlert, colourEnd, colourBad, colourEnd, colourAlert, actor.GetTrait().tag.ToUpper(), colourEnd, colourBad, colourEnd);
                                    GameManager.instance.actorScript.TraitLogMessage(actor, "for carrying out a district action", "to become STRESSED due to Security Measures");
                                }
                                else { Debug.LogWarning("Invalid condition STRESSED (Null)"); }
                            }
                            //only applies to resistance (authority node actions, eg. insert team, go through here and thence to TeamManager.cs -> MoveTeam via EffectManager.cs -> ProcessEffect)
                            if (details.side.level == GameManager.instance.globalScript.sideResistance.level)
                            {
                                //NodeActionData package -> get type
                                NodeAction nodeActionActor = NodeAction.None;
                                NodeAction nodeActionPlayer = NodeAction.None;
                                switch (actor.arc.name)
                                {
                                    case "ANARCHIST": nodeActionActor = NodeAction.ActorBlowStuffUp; nodeActionPlayer = NodeAction.PlayerBlowStuffUp; break;
                                    case "BLOGGER": nodeActionActor = NodeAction.ActorSpreadFakeNews; nodeActionPlayer = NodeAction.PlayerSpreadFakeNews; break;
                                    case "HACKER": nodeActionActor = NodeAction.ActorHackSecurity; nodeActionPlayer = NodeAction.PlayerHackSecurity; break;
                                    case "HEAVY": nodeActionActor = NodeAction.ActorCreateRiots; nodeActionPlayer = NodeAction.PlayerCreateRiots; break;
                                    case "OBSERVER": nodeActionActor = NodeAction.ActorInsertTracer; nodeActionPlayer = NodeAction.PlayerInsertTracer; break;
                                    default: Debug.LogWarningFormat("Unrecognised actor.arc \"{0}\"", actor.arc.name); break;
                                }
                                if (isPlayer == false)
                                {
                                    //actor action
                                    NodeActionData nodeActionData = new NodeActionData()
                                    {
                                        turn = GameManager.instance.turnScript.Turn,
                                        actorID = actor.actorID,
                                        nodeID = node.nodeID,
                                        nodeAction = nodeActionActor
                                    };
                                    //text list
                                    switch (actor.arc.name)
                                    {
                                        case "ANARCHIST": nodeActionData.dataName = textlistBlowUpBuildings.GetRandomRecord(); break;
                                    }
                                    //add to actor's personal list
                                    actor.AddNodeAction(nodeActionData);
                                    Debug.LogFormat("[Tst] ActionManager.cs -> ProcessNodeAction: nodeActionData added to {0}, {1}{2}", actor.actorName, actor.arc.name, "\n");
                                }
                                else
                                {
                                    //player action
                                    NodeActionData nodeActionData = new NodeActionData()
                                    {
                                        turn = GameManager.instance.turnScript.Turn,
                                        actorID = 999,
                                        nodeID = node.nodeID,
                                        nodeAction = nodeActionPlayer
                                    };
                                    //add to player's personal list
                                    GameManager.instance.playerScript.AddNodeAction(nodeActionData);
                                    Debug.LogFormat("[Tst] ActionManager.cs -> ProcessNodeAction: nodeActionData added to {0}, {1}{2}", GameManager.instance.playerScript.PlayerName, "Player", "\n");
                                }
                            }
                            //texts
                            outcomeDetails.textTop = builderTop.ToString();
                            outcomeDetails.textBottom = builderBottom.ToString();
                        }
                        else
                        {
                            Debug.LogErrorFormat("There are no Effects for this \"{0}\" Action", action.name);
                            errorFlag = true;
                        }
                    }
                    else
                    {
                        Debug.LogError("Invalid Node (null)");
                        errorFlag = true;
                    }
                }
                else
                {
                    Debug.LogError("Invalid NodeObject (null)");
                    errorFlag = true;
                }
            }
            else
            {
                Debug.LogError("Invalid Actor (null)");
                errorFlag = true;
            }
        }
        else
        {
            errorFlag = true;
            Debug.LogError("Invalid ModalActionDetails (null) as argument");
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false && isAction == true)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Node Action";
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessNodeAction");
    }

    /// <summary>
    /// Process Node Gear related actions (player's current node, Resistance)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessNodeGearAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        bool isAction = false;
        Node node = null;
        Action action = null;
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        //renown (do prior to effects as Player renown will change)
        int renownBefore = GameManager.instance.playerScript.Renown;
        //resolve action
        if (details != null)
        {
            //get node
            GameObject nodeObject = GameManager.instance.dataScript.GetNodeObject(details.nodeID);
            if (nodeObject != null)
            {
                node = nodeObject.GetComponent<Node>();
                if (node != null)
                {

                    //Get Action & Effects
                    action = details.gearAction;
                    List<Effect> listOfEffects = action.GetEffects();
                    if (listOfEffects.Count > 0)
                    {
                        //return class
                        EffectDataReturn effectReturn = new EffectDataReturn();
                        //two builders for top and bottom texts
                        StringBuilder builderTop = new StringBuilder();
                        StringBuilder builderBottom = new StringBuilder();
                        //pass through data package
                        EffectDataInput dataInput = new EffectDataInput();
                        dataInput.originText = string.Format("{0} District Action", details.gearName);
                        
                        //
                        // - - - Process effects
                        //
                        foreach (Effect effect in listOfEffects)
                        {
                            //no ongoing effect allowed for nodeGear actions
                            dataInput.ongoingID = -1;
                            dataInput.ongoingText = string.Format("{0} District Action", details.gearName);
                            effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput);
                            if (effectReturn != null)
                            {
                                outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
                                //update stringBuilder texts
                                if (String.IsNullOrEmpty(effectReturn.topText) == false) { builderTop.AppendLine(); builderTop.AppendLine(); }
                                builderTop.Append(effectReturn.topText);
                                if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                builderBottom.Append(effectReturn.bottomText);
                                //exit effect loop on error
                                if (effectReturn.errorFlag == true) { break; }
                                //valid action? -> only has to be true once for an action to be valid
                                if (effectReturn.isAction == true) { isAction = true; }
                            }
                            else
                            {
                                builderTop.AppendLine();
                                builderTop.Append("Error");
                                builderBottom.AppendLine();
                                builderBottom.Append("Error");
                                effectReturn.errorFlag = true;
                                break;
                            }
                        }
                        //texts
                        outcomeDetails.textTop = builderTop.ToString();
                        outcomeDetails.textBottom = builderBottom.ToString();
                    }
                    else
                    {
                        Debug.LogErrorFormat("There are no Effects for this \"{0}\" Action", action.name);
                        errorFlag = true;
                    }
                }
                else
                {
                    Debug.LogError("Invalid Node (null)");
                    errorFlag = true;
                }
            }
            else
            {
                Debug.LogError("Invalid NodeObject (null)");
                errorFlag = true;
            }
        }
        else
        {
            errorFlag = true;
            Debug.LogError("Invalid ModalActionDetails (null) as argument");
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false && isAction == true)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Use Gear (Node Action)";
        }
        //no error -> PROCEED to some form of dice outcome for gear use
        if (errorFlag == false)
        {
            Gear gear = GameManager.instance.dataScript.GetGear(details.gearName);
            if (gear != null)
            {
                GameManager.instance.gearScript.SetGearUsed(gear, string.Format("affect {0} district", node.nodeName));
                //generate a create modal outcome dialogue
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessNodeGearAction");
            }
            else
            {
                Debug.LogErrorFormat("Invalid Gear (Null) for {0}", details.gearName);
                errorFlag = true;
            }
        }
        //ERROR ->  go straight to outcome window
        else
        { EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessNodeGearAction"); }
    }


    /// <summary>
    /// Process Manage actor action (first of the nested Manage actor menu's -> provides 'Reserve', 'Dismiss' & 'Dispose' options
    /// </summary>
    /// <param name="details"></param>
    private void ProcessManageActorAction(ModalActionDetails details)
    {
        //Debug.Log(string.Format("Memory: total {0}{1}", System.GC.GetTotalMemory(false), "\n"));
        bool errorFlag = false;
        string title;
        string colourSide;
        string criteriaText;
        bool isResistance = true;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //Initialise nested option windows & disable back button as you are on the top level of the nested options
        GameManager.instance.genericPickerScript.InitialiseNestedOptions(details);
        GameManager.instance.genericPickerScript.SetBackButton(EventType.None);
        //color code for button tooltip header text, eg. "Operator"ss
        if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; isResistance = false; }
        else { colourSide = colourResistance; isResistance = true; }
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, playerSide);
        if (actor != null)
        {
            //ActorHandle
            List<ManageAction> listOfManageOptions = GameManager.instance.dataScript.GetListOfActorHandle();
            if (listOfManageOptions != null)
            {
                genericDetails.returnEvent = EventType.GenericHandleActor;
                genericDetails.textHeader = "Manage Subordinates";
                genericDetails.side = playerSide;
                genericDetails.actorSlotID = details.actorDataID;
                title = string.Format("{0}", isResistance ? "" : GameManager.instance.metaScript.GetAuthorityTitle().ToString() + " ");
                //picker text
                genericDetails.textTop = string.Format("{0}Manage{1} {2}{3} {4}{5}{6}", colourNeutral, colourEnd, colourNormal, actor.arc.name, title,
                    actor.actorName, colourEnd);
                genericDetails.textMiddle = string.Format("{0}You have a range of managerial options at your disposal. Be prepared to justify your decision{1}",
                    colourNormal, colourEnd);
                genericDetails.textBottom = "Click on an option toggle Select. Press CONFIRM once done. Mouseover options for more information.";
                //Create a set of options for the picker -> take only the first three options (picker can't handle more)
                GenericOptionDetails[] arrayOfGenericOptions = new GenericOptionDetails[3];
                GenericTooltipDetails[] arrayOfTooltips = new GenericTooltipDetails[3];
                int numOfOptions = Mathf.Min(3, listOfManageOptions.Count);
                //loop options
                for (int i = 0; i < numOfOptions; i++)
                {
                    ManageAction manageAction = listOfManageOptions[i];
                    if (manageAction != null)
                    {
                        GenericTooltipDetails tooltip = new GenericTooltipDetails();
                        GenericOptionDetails option = new GenericOptionDetails()
                        {
                            text = manageAction.optionTitle,
                            optionID = details.actorDataID,
                            optionText = manageAction.name,
                            sprite = manageAction.sprite,
                        };
                        //check any criteria for action is valid
                        if (manageAction.listOfCriteria.Count > 0)
                        {
                            CriteriaDataInput criteriaInput = new CriteriaDataInput()
                            {
                                actorSlotID = actor.slotID,
                                listOfCriteria = manageAction.listOfCriteria
                            };
                            criteriaText = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                            if (criteriaText == null)
                            {
                                //option activated
                                option.isOptionActive = true;
                                //tooltip
                                tooltip.textHeader = string.Format("{0}MANAGE{1}", colourSide, colourEnd);
                                tooltip.textMain = manageAction.tooltipMain;
                                tooltip.textDetails = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                            }
                            else
                            {
                                //option deactivated
                                option.isOptionActive = false;
                                //tooltip
                                tooltip.textHeader = string.Format("{0}MANAGE{1}", colourSide, colourEnd);
                                tooltip.textMain = manageAction.tooltipMain;
                                tooltip.textDetails = string.Format("{0}{1}{2}", colourInvalid, criteriaText, colourEnd);
                            }
                        }
                        else
                        {
                            //no criteria, option automatically activated
                            option.isOptionActive = true;
                            //tooltip
                            tooltip.textHeader = string.Format("{0}MANAGE{1}", colourSide, colourEnd);
                            tooltip.textMain = manageAction.tooltipMain;
                            tooltip.textDetails = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                        }
                        //add to arrays
                        arrayOfGenericOptions[i] = option;
                        arrayOfTooltips[i] = tooltip;
                    }
                    else { Debug.LogErrorFormat("Invalid manageAction (Null) for listOfManageOptions[{0}]", i); }
                }
                //add options to picker data package
                genericDetails.arrayOfOptions = arrayOfGenericOptions;
                genericDetails.arrayOfTooltips = arrayOfTooltips;
            }
            else { Debug.LogError("Invalid listOfManageOptions (Null)"); errorFlag = true; }

        }
        else { Debug.LogErrorFormat("Invalid actor (Null) for actorSlotID {0}", details.actorDataID); errorFlag = true; }

        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = playerSide;
            outcomeDetails.textTop = string.Format("{0}You are unable to send anyone to the Reserve Pool at this time{1}", colourAlert, colourEnd);
            outcomeDetails.textBottom = "Why is that so? Nobody knows.";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessManageActorAction");
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails, "ActionManager.cs -> ProcessManagerActorAction");
        }
    }

    /// <summary>
    /// Process Manage -> Send to Reserve -> actor action (second level of the nested Manage actor menus)
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseReserveActorAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        string title, colourSide, criteriaText, tooltipText;
        int renownCost = GameManager.instance.actorScript.manageReserveRenown;
        int unhappyTimerBase = GameManager.instance.actorScript.currentReserveTimer;
        int unhappyTimer;
        bool isResistance = true;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"
        if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; isResistance = false; }
        else { colourSide = colourResistance; isResistance = true; }
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, playerSide);
        if (actor != null)
        {
            //ActorHandle
            List<ManageAction> listOfManageOptions = GameManager.instance.dataScript.GetListOfActorReserve();
            if (listOfManageOptions != null)
            {
                genericDetails.returnEvent = EventType.GenericReserveActor;
                genericDetails.side = playerSide;
                genericDetails.textHeader = "Send to Reserves";
                genericDetails.actorSlotID = details.actorDataID;
                title = string.Format("{0}", isResistance ? "" : GameManager.instance.metaScript.GetAuthorityTitle().ToString() + " ");
                //picker text
                genericDetails.textTop = string.Format("{0}Send to the Reserve Pool{1}{2} {3} {4}{5}{6}", colourNeutral, colourEnd, colourNormal, actor.arc.name, title,
                    actor.actorName, colourEnd);
                genericDetails.textMiddle = string.Format("{0}{1} asks to return as soon as possible{2}",
                    colourNormal, actor.actorName, colourEnd);
                genericDetails.textBottom = "Click on an option toggle Select. Press CONFIRM once done. Mouseover options for more information.";
                //Create a set of options for the picker -> take only the first three options (picker can't handle more)
                GenericOptionDetails[] arrayOfGenericOptions = new GenericOptionDetails[3];
                GenericTooltipDetails[] arrayOfTooltips = new GenericTooltipDetails[3];
                int numOfOptions = Mathf.Min(3, listOfManageOptions.Count);
                for (int i = 0; i < numOfOptions; i++)
                {
                    unhappyTimer = unhappyTimerBase;
                    //traits that affect unhappy timer
                    string traitText = "";
                    if (actor.CheckTraitEffect(actorReserveTimerDoubled) == true)
                    { unhappyTimer *= 2; traitText = string.Format(" ({0})", actor.GetTrait().tag); }
                    else if (actor.CheckTraitEffect(actorReserveTimerHalved) == true)
                    { unhappyTimer /= 2; unhappyTimer = Mathf.Max(1, unhappyTimer); traitText = string.Format(" ({0})", actor.GetTrait().tag); }
                    //tooltip details
                    StringBuilder builder = new StringBuilder();
                    //manageAction
                    ManageAction manageAction = listOfManageOptions[i];
                    if (manageAction != null)
                    {
                        GenericTooltipDetails tooltip = new GenericTooltipDetails();
                        GenericOptionDetails option = new GenericOptionDetails()
                        {
                            text = manageAction.optionTitle,
                            optionID = details.actorDataID,
                            optionText = manageAction.name,
                            sprite = manageAction.sprite,
                        };
                        //check any criteria for action is valid
                        if (manageAction.listOfCriteria.Count > 0)
                        {
                            CriteriaDataInput criteriaInput = new CriteriaDataInput()
                            {
                                actorSlotID = actor.slotID,
                                listOfCriteria = manageAction.listOfCriteria
                            };
                            criteriaText = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                            if (criteriaText == null)
                            {
                                //option activated
                                option.isOptionActive = true;
                                //tooltip
                                tooltip.textHeader = string.Format("{0}Send to the RESERVES{1}", colourSide, colourEnd);
                                tooltip.textMain = manageAction.tooltipMain;
                                //tooltip details
                                tooltipText = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                                builder.AppendFormat("{0}{1}", tooltipText, "\n");
                                builder.AppendFormat("{0}Unhappy in {1} turn{2}{3}{4}{5}", colourAlert, unhappyTimer, unhappyTimer != 1 ? "s" : "", traitText, colourEnd, "\n");
                                if (manageAction.isRenownCost == true)
                                { builder.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownCost, colourEnd); }
                                else
                                { builder.AppendFormat("{0}No Renown Cost{1}", colourGood, colourEnd); }
                            }
                            else
                            {
                                //option DEACTIVATED
                                option.isOptionActive = false;
                                //tooltip
                                tooltip.textHeader = string.Format("{0}Option Unavailable{1}", colourSide, colourEnd);
                                tooltip.textMain = manageAction.tooltipMain;
                                if (manageAction.isRenownCost == true)
                                { builder.AppendFormat("{0}{1}{2}", colourInvalid, criteriaText, colourEnd); }
                                else
                                { builder.AppendFormat("{0}{1}{2}{3}{4}No Renown Cost{5}", colourInvalid, criteriaText, colourEnd, "\n", colourGood, colourEnd); }
                            }
                        }
                        else
                        {
                            //no criteria, option automatically activated
                            option.isOptionActive = true;
                            //tooltip
                            tooltip.textHeader = string.Format("{0}Send to the RESERVES{1}", colourSide, colourEnd);
                            tooltip.textMain = manageAction.tooltipMain;
                            //tooltip details
                            tooltipText = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                            builder.AppendFormat("{0}{1}", tooltipText, "\n");
                            builder.AppendFormat("{0}Unhappy in {1} turn{2}{3}{4}{5}", colourAlert, unhappyTimer, unhappyTimer != 1 ? "s" : "", traitText, colourEnd, "\n");
                            if (manageAction.isRenownCost == true)
                            { builder.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownCost, colourEnd); }
                            else
                            { builder.AppendFormat("{0}No Renown Cost{1}", colourGood, colourEnd); }
                        }
                        //Mood tooltip
                        string textMood = "Unknown";
                        switch (manageAction.name)
                        {
                            case "ReservePromise": textMood = GameManager.instance.personScript.GetMoodTooltip(MoodType.ReservePromise, actor.arc.name); break;
                            case "ReserveNoPromise": textMood = GameManager.instance.personScript.GetMoodTooltip(MoodType.ReserveNoPromise, actor.arc.name); break;
                            case "ReserveRest": textMood = GameManager.instance.personScript.GetMoodTooltip(MoodType.ReserveRest, actor.arc.name); break;
                            default: Debug.LogWarningFormat("Unrecognised manageAction \"{0}\"", manageAction.name); break;
                        }
                        builder.AppendFormat("{0}{1}", "\n", textMood);
                        //tooltip details finalise
                        tooltip.textDetails = builder.ToString();
                        //add to arrays
                        arrayOfGenericOptions[i] = option;
                        arrayOfTooltips[i] = tooltip;
                    }
                    else { Debug.LogErrorFormat("Invalid manageAction (Null) for listOfManageOptions[{0}]", i); }

                }
                //add options to picker data package
                genericDetails.arrayOfOptions = arrayOfGenericOptions;
                genericDetails.arrayOfTooltips = arrayOfTooltips;
            }
            else { Debug.LogError("Invalid listOfManageOptions (Null)"); errorFlag = true; }

        }
        else { Debug.LogErrorFormat("Invalid actor (Null) for actorSlotID {0}", details.actorDataID); errorFlag = true; }

        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = playerSide;
            outcomeDetails.textTop = string.Format("{0}You are unable to send anyone to the Reserve pool at this time{1}", colourAlert, colourEnd);
            outcomeDetails.textBottom = "Why is it so? Nobody knows.";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> InitialiseReserveActorAction");
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails, "ActionManager.cs -> InitialiseReserveActorAction");
        }
    }

    /// <summary>
    /// Process Manage -> Dismiss Actor -> actor action (second level of the nested Manage actor menus)
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseDismissActorAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        string title, colourSide, criteriaText, tooltipText;
        int renownCost = GameManager.instance.actorScript.manageDismissRenown;
        bool isResistance = true;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"
        if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; isResistance = false; }
        else { colourSide = colourResistance; isResistance = true; }
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, playerSide);
        if (actor != null)
        {
            //ActorHandle
            List<ManageAction> listOfManageOptions = GameManager.instance.dataScript.GetListOfActorDismiss();
            if (listOfManageOptions != null)
            {
                genericDetails.returnEvent = EventType.GenericDismissActor;
                genericDetails.side = playerSide;
                genericDetails.textHeader = "Dismiss";
                genericDetails.actorSlotID = details.actorDataID;
                title = string.Format("{0}", isResistance ? "" : GameManager.instance.metaScript.GetAuthorityTitle().ToString() + " ");
                //picker text
                genericDetails.textTop = string.Format("{0}Dismiss{1}{2} {3} {4}{5}{6}", colourNeutral, colourEnd, colourNormal, actor.arc.name, title,
                    actor.actorName, colourEnd);
                genericDetails.textMiddle = string.Format("{0}{1} demands to know why?{2}",
                    colourNormal, actor.actorName, colourEnd);
                genericDetails.textBottom = "Click on an option toggle Select. Press CONFIRM once done. Mouseover options for more information.";
                //Create a set of options for the picker -> take only the first three options (picker can't handle more)
                GenericOptionDetails[] arrayOfGenericOptions = new GenericOptionDetails[3];
                GenericTooltipDetails[] arrayOfTooltips = new GenericTooltipDetails[3];
                int numOfOptions = Mathf.Min(3, listOfManageOptions.Count);
                for (int i = 0; i < numOfOptions; i++)
                {
                    //tooltip details
                    StringBuilder builder = new StringBuilder();
                    //manageAction
                    ManageAction manageAction = listOfManageOptions[i];
                    if (manageAction != null)
                    {
                        GenericTooltipDetails tooltip = new GenericTooltipDetails();
                        GenericOptionDetails option = new GenericOptionDetails()
                        {
                            text = manageAction.optionTitle,
                            optionID = details.actorDataID,
                            optionText = manageAction.name,
                            sprite = manageAction.sprite,
                        };
                        //check any criteria for action is valid
                        if (manageAction.listOfCriteria.Count > 0)
                        {
                            CriteriaDataInput criteriaInput = new CriteriaDataInput()
                            {
                                actorSlotID = actor.slotID,
                                listOfCriteria = manageAction.listOfCriteria
                            };
                            criteriaText = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                            if (criteriaText == null)
                            {
                                //option activated
                                option.isOptionActive = true;
                                //tooltip
                                tooltip.textHeader = string.Format("{0}DISMISS{1}", colourSide, colourEnd);
                                tooltip.textMain = manageAction.tooltipMain;
                                //tooltip details
                                tooltipText = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                                builder.AppendFormat("{0}{1}", tooltipText, "\n");
                                if (manageAction.isRenownCost == true)
                                {
                                    ManageRenownCost manageRenownCost = GameManager.instance.actorScript.GetManageRenownCost(actor, renownCost);
                                    builder.AppendFormat("{0}Player Renown -{1}{2}", colourBad, manageRenownCost.renownCost, colourEnd);
                                    if (string.IsNullOrEmpty(manageRenownCost.tooltip) == false)
                                    { builder.Append(manageRenownCost.tooltip); }

                                }
                                else
                                { builder.AppendFormat("{0}No Renown Cost{1}", colourGood, colourEnd);  }
                            }
                            else
                            {
                                //option DEACTIVATED
                                option.isOptionActive = false;
                                //tooltip (effectmanager handles extras)
                                tooltip.textHeader = string.Format("{0}Option Unavailable{1}", colourSide, colourEnd);
                                tooltip.textMain = manageAction.tooltipMain;
                                if (manageAction.isRenownCost == true)
                                { builder.AppendFormat("{0}{1}{2}", colourInvalid, criteriaText, colourEnd); }
                                else
                                { builder.AppendFormat("{0}{1}{2}{3}{4}No Renown Cost{5}", colourInvalid, criteriaText, colourEnd, "\n", colourGood, colourEnd); }
                            }
                        }
                        else
                        {
                            //no criteria, option automatically activated
                            option.isOptionActive = true;
                            //tooltip
                            tooltip.textHeader = string.Format("{0}DISMISS{1}", colourSide, colourEnd);
                            tooltip.textMain = manageAction.tooltipMain;
                            tooltipText = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                            builder.AppendFormat("{0}{1}", tooltipText, "\n");
                            if (manageAction.isRenownCost == true)
                            { builder.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownCost, colourEnd); }
                            else
                            { builder.AppendFormat("{0}No Renown Cost{1}", colourGood, colourEnd); }
                        }
                        //mood details
                        string textMood = "Unknown";
                        switch (manageAction.name)
                        {
                            case "DismissIncompetent": textMood = GameManager.instance.personScript.GetMoodTooltip(MoodType.DismissIncompetent, actor.arc.name); break;
                            case "DismissPromote": textMood = GameManager.instance.personScript.GetMoodTooltip(MoodType.DismissPromote, actor.arc.name); break;
                            case "DismissUnsuited": textMood = GameManager.instance.personScript.GetMoodTooltip(MoodType.DismissUnsuited, actor.arc.name); break;
                            default: Debug.LogWarningFormat("Unrecognised manageAction \"{0}\"", manageAction.name); break;
                        }
                        builder.AppendFormat("{0}{1}", "\n", textMood);
                        //tooltip details finalise
                        tooltip.textDetails = builder.ToString();
                        //add to arrays
                        arrayOfGenericOptions[i] = option;
                        arrayOfTooltips[i] = tooltip;
                    }
                    else { Debug.LogErrorFormat("Invalid manageAction (Null) for listOfManageOptions[{0}]", i); }

                }
                //add options to picker data package
                genericDetails.arrayOfOptions = arrayOfGenericOptions;
                genericDetails.arrayOfTooltips = arrayOfTooltips;
            }
            else { Debug.LogError("Invalid listOfManageOptions (Null)"); errorFlag = true; }

        }
        else { Debug.LogErrorFormat("Invalid actor (Null) for actorSlotID {0}", details.actorDataID); errorFlag = true; }

        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = playerSide;
            outcomeDetails.textTop = string.Format("{0}You are unable to Dismiss of anyone at this time{1}", colourAlert, colourEnd);
            outcomeDetails.textBottom = "Why this is so is under investigation";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> InitialiseDismissActorAction");
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails, "ActionManager.cs -> InitialiseDismissActorAction");
        }
    }

    /// <summary>
    /// Process Manage -> Dispose of Actor -> actor action (second level of the nested Manage actor menus)
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseDisposeActorAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        string title, colourSide, criteriaText, tooltipText;
        int renownCost = GameManager.instance.actorScript.manageDisposeRenown;
        bool isResistance = true;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"
        if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; isResistance = false; }
        else { colourSide = colourResistance; isResistance = true; }
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, playerSide);
        if (actor != null)
        {
            //ActorHandle
            List<ManageAction> listOfManageOptions = GameManager.instance.dataScript.GetListOfActorDispose();
            if (listOfManageOptions != null)
            {
                genericDetails.returnEvent = EventType.GenericDisposeActor;
                genericDetails.side = playerSide;
                genericDetails.textHeader = "Dispose Off";
                genericDetails.actorSlotID = details.actorDataID;
                title = string.Format("{0}", isResistance ? "" : GameManager.instance.metaScript.GetAuthorityTitle().ToString() + " ");
                //picker text
                genericDetails.textTop = string.Format("{0}Dispose of{1}{2} {3} {4}{5}{6}", colourNeutral, colourEnd, colourNormal, actor.arc.name, title,
                    actor.actorName, colourEnd);
                genericDetails.textMiddle = string.Format("{0}HQ requests that you provide a reason{1}", colourNormal, colourEnd);
                genericDetails.textBottom = "Click on an option toggle Select. Press CONFIRM once done. Mouseover options for more information.";
                //Create a set of options for the picker -> take only the first three options (picker can't handle more)
                GenericOptionDetails[] arrayOfGenericOptions = new GenericOptionDetails[3];
                GenericTooltipDetails[] arrayOfTooltips = new GenericTooltipDetails[3];
                int numOfOptions = Mathf.Min(3, listOfManageOptions.Count);
                for (int i = 0; i < numOfOptions; i++)
                {
                    //tooltip details
                    StringBuilder builder = new StringBuilder();
                    ManageAction manageAction = listOfManageOptions[i];
                    if (manageAction != null)
                    {
                        GenericTooltipDetails tooltip = new GenericTooltipDetails();
                        GenericOptionDetails option = new GenericOptionDetails()
                        {
                            text = manageAction.optionTitle,
                            optionID = details.actorDataID,
                            optionText = manageAction.name,
                            sprite = manageAction.sprite,
                        };
                        //check any criteria for action is valid
                        if (manageAction.listOfCriteria.Count > 0)
                        {
                            CriteriaDataInput criteriaInput = new CriteriaDataInput()
                            {
                                actorSlotID = actor.slotID,
                                listOfCriteria = manageAction.listOfCriteria
                            };
                            criteriaText = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                            if (criteriaText == null)
                            {
                                //option activated
                                option.isOptionActive = true;
                                //tooltip
                                tooltip.textHeader = string.Format("{0}DISPOSE OF{1}", colourSide, colourEnd);
                                tooltip.textMain = manageAction.tooltipMain;
                                //tooltip details
                                tooltipText = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                                builder.AppendFormat("{0}{1}", tooltipText, "\n");
                                if (manageAction.isRenownCost == true)
                                {
                                    /*StringBuilder builder = new StringBuilder();
                                    ManageRenownCost manageRenownCost = GameManager.instance.actorScript.GetManageRenownCost(actor, renownCost);
                                    builder.AppendFormat("{0}{1}{2}Player Renown -{3}{4}", tooltipText, "\n", colourBad, manageRenownCost.renownCost, colourEnd);
                                    if (string.IsNullOrEmpty(manageRenownCost.tooltip) == false)
                                    { builder.Append(manageRenownCost.tooltip); }
                                    tooltip.textDetails = builder.ToString();*/
                                    ManageRenownCost manageRenownCost = GameManager.instance.actorScript.GetManageRenownCost(actor, renownCost);
                                    builder.AppendFormat("{0}Player Renown -{1}{2}", colourBad, manageRenownCost.renownCost, colourEnd);
                                    if (string.IsNullOrEmpty(manageRenownCost.tooltip) == false)
                                    { builder.Append(manageRenownCost.tooltip); }
                                }
                                else
                                {
                                    /*tooltip.textDetails = string.Format("{0}{1}{2}No Renown Cost{3}", tooltipText, "\n", colourGood, colourEnd);*/
                                    builder.AppendFormat("{0}No Renown Cost{1}", colourGood, colourEnd);
                                }
                            }
                            else
                            {
                                //option DEACTIVATED
                                option.isOptionActive = false;
                                //tooltip
                                tooltip.textHeader = string.Format("{0}Option Unavailable{1}", colourSide, colourEnd);
                                tooltip.textMain = manageAction.tooltipMain;
                                if (manageAction.isRenownCost == true)
                                { builder.AppendFormat("{0}{1}{2}", colourInvalid, criteriaText, colourEnd); }
                                else
                                {
                                    builder.AppendFormat("{0}{1}{2}{3}{4}No Renown Cost{5}", colourInvalid, criteriaText, colourEnd, "\n", colourGood,
                                      colourEnd);
                                }
                            }
                        }
                        else
                        {
                            //no criteria, option automatically activated
                            option.isOptionActive = true;
                            //tooltip
                            tooltip.textHeader = string.Format("{0}DISPOSE OF{1}", colourSide, colourEnd);
                            tooltip.textMain = manageAction.tooltipMain;
                            //tooltip details
                            tooltipText = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                            builder.AppendFormat("{0}{1}", tooltipText, "\n");
                            if (manageAction.isRenownCost == true)
                            {
                                /*tooltip.textDetails = string.Format("{0}{1}{2}Player Renown -{3}{4}", tooltipText, "\n", colourBad, renownCost, colourEnd);*/
                                builder.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownCost, colourEnd);
                            }
                            else
                            {
                                /*tooltip.textDetails = string.Format("{0}{1}{2}No Renown Cost{3}", tooltipText, "\n", colourGood, colourEnd);*/
                                builder.AppendFormat("{0}No Renown Cost{1}", colourGood, colourEnd);
                            }
                        }
                        //mood details
                        string textMood = "Unknown";
                        switch (manageAction.name)
                        {
                            case "DisposeCorrupt": textMood = GameManager.instance.personScript.GetMoodTooltip(MoodType.DisposeCorrupt, actor.arc.name); break;
                            case "DisposeHabit": textMood = GameManager.instance.personScript.GetMoodTooltip(MoodType.DisposeHabit, actor.arc.name); break;
                            case "DisposeLoyalty": textMood = GameManager.instance.personScript.GetMoodTooltip(MoodType.DisposeLoyalty, actor.arc.name); break;
                            default: Debug.LogWarningFormat("Unrecognised manageAction \"{0}\"", manageAction.name); break;
                        }
                        builder.AppendFormat("{0}{1}", "\n", textMood);
                        //tooltip details finalise
                        tooltip.textDetails = builder.ToString();
                        //add to arrays
                        arrayOfGenericOptions[i] = option;
                        arrayOfTooltips[i] = tooltip;
                    }
                    else { Debug.LogErrorFormat("Invalid manageAction (Null) for listOfManageOptions[{0}]", i); }

                }
                //add options to picker data package
                genericDetails.arrayOfOptions = arrayOfGenericOptions;
                genericDetails.arrayOfTooltips = arrayOfTooltips;
            }
            else { Debug.LogError("Invalid listOfManageOptions (Null)"); errorFlag = true; }

        }
        else { Debug.LogErrorFormat("Invalid actor (Null) for actorSlotID {0}", details.actorDataID); errorFlag = true; }

        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = playerSide;
            outcomeDetails.textTop = string.Format("{0}You are unable to Dispose of anyone at this time{1}", colourAlert, colourEnd);
            outcomeDetails.textBottom = "Why this is so is under investigation";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> InitailiseDisposeActorAction");
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails, "ActionManager.cs -> InitialiseDisposeActorAction");
        }
    }

    /// <summary>
    /// Process Lie Low actor action (Resistance only). Can't do during a Surveillance crackdown
    /// </summary>
    /// <param name="details"></param>
    public void ProcessLieLowActorAction(ModalActionDetails details)
    {
        Debug.Assert(GameManager.instance.turnScript.authoritySecurityState != AuthoritySecurityState.SurveillanceCrackdown, string.Format("Invalid authoritySecurityState {0}",
            GameManager.instance.turnScript.authoritySecurityState));
        bool errorFlag = false;
        bool isStressed = false;
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        int numOfTurns = 0;
        string actorName = "Unknown";
        string actorArc = "Unknown";
        if (details != null)
        {
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, details.side);
            if (actor != null)
            {
                actorName = actor.actorName;
                actorArc = actor.arc.name;
                actor.Status = ActorStatus.Inactive;
                actor.inactiveStatus = ActorInactive.LieLow;
                actor.tooltipStatus = ActorTooltip.LieLow;
                actor.isLieLowFirstturn = true;
                numOfTurns = 3 - actor.GetDatapoint(ActorDatapoint.Invisibility2);
                outcomeDetails.textTop = string.Format("{0}{1}{2} has been ordered to {3}Lie Low{4}", colourAlert, actor.actorName, colourEnd, colourNeutral, colourEnd);
                outcomeDetails.sprite = actor.sprite;
                isStressed = actor.CheckConditionPresent(conditionStressed);
                //message
                string text = string.Format("{0}, {1}, is lying Low. Status: {2}", actor.arc.name, actor.actorName, actor.Status);
                string reason = string.Format("is currently <b>Lying Low</b> and {0}{1}{2}<b>cut off from all communications</b>{3}", "\n", "\n", colourBad, colourEnd);
                GameManager.instance.messageScript.ActorStatus(text, "is LYING LOW", reason, actor.actorID, details.side);

                /*//update contacts
                GameManager.instance.contactScript.UpdateNodeContacts();*/
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorSlotID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        if (errorFlag == false)
        {
            //set lie low timer
            GameManager.instance.actorScript.SetLieLowTimer();
            //change alpha of actor to indicate inactive status
            GameManager.instance.actorPanelScript.UpdateActorAlpha(details.actorDataID, GameManager.instance.guiScript.alphaInactive);
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Actor Lie Low";
            outcomeDetails.textBottom = GetLieLowMessage(numOfTurns, actorName, actorArc, isStressed);
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessLieLowActorAction");
    }

    /// <summary>
    /// Process Lie Low Player action (Resistance only). Can't do during a Surveillance crackdown
    /// </summary>
    /// <param name="modalDetails"></param>
    public void ProcessLieLowPlayerAction(ModalActionDetails modalDetails)
    {
        Debug.Assert(GameManager.instance.turnScript.authoritySecurityState != AuthoritySecurityState.SurveillanceCrackdown, string.Format("Invalid authoritySecurityState {0}",
            GameManager.instance.turnScript.authoritySecurityState));
        string playerName = GameManager.instance.playerScript.PlayerName;
        int invis = GameManager.instance.playerScript.Invisibility;
        int numOfTurns = 3 - invis;
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(modalDetails);
        if (modalDetails != null)
        {
            GameManager.instance.playerScript.status = ActorStatus.Inactive;
            GameManager.instance.playerScript.inactiveStatus = ActorInactive.LieLow;
            GameManager.instance.playerScript.tooltipStatus = ActorTooltip.LieLow;
            GameManager.instance.playerScript.isLieLowFirstturn = true;
            GameManager.instance.dataScript.StatisticIncrement(StatType.PlayerLieLow);
            outcomeDetails.textTop = string.Format("{0}{1}{2} will go to ground and {3}Lie Low{4}", colourAlert, playerName, colourEnd, colourNeutral, colourEnd);
            outcomeDetails.sprite = GameManager.instance.playerScript.sprite;
            //message
            Debug.LogFormat("[Ply] ActionManager.cs -> ProcessLieLowPlayerAction: {0}, {1} Player, commences LYING LOW", GameManager.instance.playerScript.GetPlayerName(modalDetails.side), modalDetails.side.name);
            string text = string.Format("{0} is lying Low. Status: {1}", playerName, GameManager.instance.playerScript.status);
            string reason = string.Format("is currently Lying Low and {0}{1}{2}<b>cut off from all communications</b>{3}", "\n", "\n", colourBad, colourEnd);
            GameManager.instance.messageScript.ActorStatus(text, "is LYING LOW", reason, GameManager.instance.playerScript.actorID, modalDetails.side, null, HelpType.LieLow);
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        if (errorFlag == false)
        {
            //set lie low timer
            GameManager.instance.actorScript.SetLieLowTimer();
            //change alpha of player to indicate inactive status
            GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaInactive);
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false)
        {
            bool isStressed = GameManager.instance.playerScript.CheckConditionPresent(conditionStressed, GameManager.instance.globalScript.sideResistance);
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Player Lie Low";
            outcomeDetails.textBottom = GetLieLowMessage(numOfTurns, playerName, "PLAYER", isStressed);
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessLieLowPlayerAction");
    }

    /// <summary>
    /// formatted message string for both player and actor Lie Low action
    /// </summary>
    /// <returns></returns>
    private string GetLieLowMessage(int numOfTurns, string actorName, string actorArc, bool isStressed)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}, {1}{2}{3}, will be Inactive for {4}{5} turn{6}{7} or until Activated", actorName, colourAlert, actorArc, colourEnd,
            colourNeutral, numOfTurns, numOfTurns != 1 ? "s" : "", colourEnd);
        builder.AppendLine(); builder.AppendLine();
        builder.AppendFormat("{0}Invisibility +1{1} each turn {2}Inactive{3}", colourNeutral, colourEnd, colourAlert, colourEnd);
        builder.AppendLine(); builder.AppendLine();
        //only do stress text if actor/player is stressed (there's a lot of text to read already)
        if (isStressed == true)
        {
            builder.AppendFormat("{0}Stress{1} will be removed once Invisibility is {2}fully recovered{3}", colourNeutral, colourEnd, colourAlert, colourEnd);
            builder.AppendLine(); builder.AppendLine();
        }
        builder.AppendFormat("{0}All contacts and abilities will be unavailable while Inactive{1}", colourBad, colourEnd);
        builder.AppendLine(); builder.AppendLine();
        builder.AppendFormat("{0}Rebel HQ{1} will need {2}{3} turn{4}{5} before anyone else can Lie Low", colourAlert, colourEnd, colourNeutral, lieLowPeriod, lieLowPeriod != 1 ? "s" : "", colourEnd);
        return builder.ToString();
    }

    /// <summary>
    /// Process cure for a player condition (Resistance only)
    /// </summary>
    /// <param name="details"></param>
    private void ProcessPlayerCure(ModalActionDetails details)
    {
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        if (details != null)
        {
            Node node = GameManager.instance.dataScript.GetNode(details.nodeID);
            if (node != null)
            {
                Cure cure = node.cure;
                if (cure != null)
                {
                    //remove condition
                    string reason = string.Format("Cure {0}{1}{2} condition", colourBad, cure.cureName, colourEnd);
                    if (GameManager.instance.playerScript.RemoveCondition(cure.condition, details.side, reason) == true)
                    {
                        outcomeDetails.reason = reason;
                        outcomeDetails.isAction = true;
                        outcomeDetails.sprite = GameManager.instance.playerScript.sprite;
                        outcomeDetails.textTop = string.Format("{0} has cured your {1}{2}{3} condition", cure.cureName, colourBad, cure.condition.tag, colourEnd);
                        outcomeDetails.textBottom = string.Format("{0}{1}{2}", colourNormal, cure.outcomeText, colourEnd);
                    }
                    else { Debug.LogWarning("Condition Not Removed"); }
                }
                else { Debug.LogErrorFormat("Invalid cure (Null) for nodeID {0}", node.nodeID); }
            }
            else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", details.nodeID); }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessActivateActorAction");
    }


    /// <summary>
    /// Process Activate actor action (Resistance only at present but set up for both sides)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessActivateActorAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        if (details != null)
        {
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, details.side);
            if (actor != null)
            {
                string title = "";
                if (details.side == GameManager.instance.globalScript.sideAuthority)
                { title = string.Format(" {0} ", GameManager.instance.metaScript.GetAuthorityTitle()); }

                //Reactivate actor
                actor.Status = ActorStatus.Active;
                actor.inactiveStatus = ActorInactive.None;
                actor.tooltipStatus = ActorTooltip.None;
                outcomeDetails.textTop = string.Format(" {0} {1} has been Recalled", actor.arc.name, actor.actorName);
                outcomeDetails.textBottom = string.Format("{0}{1}{2} is now fully Activated{3}", colourNeutral, actor.actorName, title, colourEnd);
                outcomeDetails.sprite = actor.sprite;
                //message
                string text = string.Format("{0} {1} has been Recalled. Status: {2}", actor.arc.name, actor.actorName, actor.Status);
                GameManager.instance.messageScript.ActorStatus(text, "Recalled", "has been Recalled", actor.actorID, details.side);
                //update contacts
                GameManager.instance.contactScript.UpdateNodeContacts();
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorSlotID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        if (errorFlag == false)
        {
            //change alpha of actor to indicate inactive status
            GameManager.instance.actorPanelScript.UpdateActorAlpha(details.actorDataID, GameManager.instance.guiScript.alphaActive);
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Activate Actor";
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessActivateActorAction");
    }

    /// <summary>
    /// Process Activate Player action (Resistance only at present but set up for both sides)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessActivatePlayerAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        string playerName = GameManager.instance.playerScript.PlayerName;
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        if (details != null)
        {

            string title = "";
            if (details.side == GameManager.instance.globalScript.sideAuthority)
            { title = string.Format(" {0} ", GameManager.instance.metaScript.GetAuthorityTitle()); }

            //Reactivate Player
            GameManager.instance.playerScript.status = ActorStatus.Active;
            GameManager.instance.playerScript.inactiveStatus = ActorInactive.None;
            GameManager.instance.playerScript.tooltipStatus = ActorTooltip.None;
            outcomeDetails.textTop = string.Format(" {0} has emerged from hiding early", playerName);
            outcomeDetails.textBottom = string.Format("{0}{1} is now fully Activated{2}", colourNeutral, playerName, title, colourEnd);
            outcomeDetails.sprite = GameManager.instance.playerScript.sprite;
            //message
            string text = string.Format("{0} has emerged from hiding early. Status: {1}", playerName, GameManager.instance.playerScript.status);
            GameManager.instance.messageScript.ActorStatus(text, "Recalled", "has been Recalled", GameManager.instance.playerScript.actorID, details.side);
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        if (errorFlag == false)
        {
            //change alpha of player to indicate inactive status
            GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaActive);
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Activate Player";
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessActivatePlayerAction");
    }

    /// <summary>
    /// Process Stress Leave for Human Player (both sides)
    /// </summary>
    /// <param name="details"></param>
    private void ProcessStressLeavePlayerAction(ModalActionDetails modalDetails)
    {
        if (modalDetails != null)
        {
            GameManager.instance.playerScript.status = ActorStatus.Inactive;
            GameManager.instance.playerScript.inactiveStatus = ActorInactive.StressLeave;
            GameManager.instance.playerScript.tooltipStatus = ActorTooltip.Leave;
            GameManager.instance.playerScript.isStressLeave = true;
            //deduct renown cost
            int renown = GameManager.instance.playerScript.Renown;
            renown -= modalDetails.renownCost;
            if (renown < 0)
            {
                renown = 0;
                Debug.LogWarningFormat("Renown dropped below Zero");
            }
            GameManager.instance.playerScript.Renown = renown;
            //change alpha of actor to indicate inactive status
            GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaInactive);
            GameManager.instance.actorPanelScript.UpdatePlayerRenownUI(renown);
            //message (public)
            string playerName = GameManager.instance.playerScript.GetPlayerName(modalDetails.side);
            string text = string.Format("Player, {0}, has gone on Stress Leave", playerName);
            string itemText = "You have gone on Stress LEAVE";
            string reason = "has taken a break in order to recover from their <b>STRESS</b>";
            string details = string.Format("{0}<b>Unavailable but will recover next turn</b>{1}", colourNeutral, colourEnd);
            GameManager.instance.messageScript.ActorStatus(text, itemText, reason, modalDetails.actorDataID, modalDetails.side, details, HelpType.StressLeave);
            Debug.LogFormat("[Ply] ActionManager.cs -> ProcessLeavePlayerAction: {0}, {1} Player, commences STRESS LEAVE", GameManager.instance.playerScript.GetPlayerName(modalDetails.side),
                modalDetails.side.name);
            //statistics
            StressLeaveStatistics(modalDetails.side);
            //action (if valid) expended -> must be BEFORE outcome window event
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Player Stress Leave";
            outcomeDetails.textBottom = string.Format("You, {0}{1}{2}, have taken Stress Leave and will return a better person, {3}free of Stress{4}{5}{6}{7}You cannot be captured while on Leave{8}",
                colourAlert, playerName, colourEnd, colourNeutral, colourEnd, "\n", "\n", colourGood, colourEnd);
            //generate a create modal window event
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessLeavePlayerAction");
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); }
    }

    /// <summary>
    /// Process Stress Leave for Human Authority/Resistance Actor
    /// </summary>
    /// <param name="modalDetails"></param>
    private void ProcessStressLeaveActorAction(ModalActionDetails modalDetails)
    {
        if (modalDetails != null)
        {
            Actor actor = GameManager.instance.dataScript.GetActor(modalDetails.actorDataID);
            if (actor != null)
            {
                actor.Status = ActorStatus.Inactive;
                actor.inactiveStatus = ActorInactive.StressLeave;
                actor.tooltipStatus = ActorTooltip.Leave;
                actor.isStressLeave = true;
                //deduct renown cost
                int renown = GameManager.instance.playerScript.Renown;
                renown -= modalDetails.renownCost;
                if (renown < 0)
                {
                    renown = 0;
                    Debug.LogWarningFormat("Renown dropped below Zero");
                }
                GameManager.instance.playerScript.Renown = renown;
                //change alpha of actor to indicate inactive status
                GameManager.instance.actorPanelScript.UpdateActorAlpha(actor.slotID, GameManager.instance.guiScript.alphaInactive);
                GameManager.instance.actorPanelScript.UpdatePlayerRenownUI(renown);
                //message (public)
                string text = string.Format("{0}, {1}, has gone on Stress Leave", actor.actorName, actor.arc.name);
                string itemText = "has gone on Stress LEAVE";
                string reason = "has taken a break in order to recover from their <b>STRESS</b>";
                string details = string.Format("{0}<b>Unavailable but will recover next turn</b>{1}", colourNeutral, colourEnd);
                GameManager.instance.messageScript.ActorStatus(text, itemText, reason, actor.actorID, modalDetails.side, details, HelpType.StressLeave);
                //statistics
                StressLeaveStatistics(modalDetails.side);
                //action (if valid) expended -> must be BEFORE outcome window event
                ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
                outcomeDetails.isAction = true;
                outcomeDetails.reason = "Actor Stress Leave";
                outcomeDetails.textBottom = string.Format("{0}, {1}{2}{3}, has taken Stress Leave and will return a better person, {4}free of Stress{5}{6}{7}{8}{9} cannot be Captured while on Leave{10}",
                    actor.actorName, colourAlert, actor.arc.name, colourEnd, colourNeutral, colourEnd, "\n", "\n", colourGood, actor.actorName, colourEnd);
                //generate a create modal window event
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessLeaveActorAction");
            }
            else { Debug.LogError("Invalid actor (Null)"); }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); }
    }

    /// <summary>
    /// submethod for ProcessStressLeavePlayer/Actor to take care of statistics
    /// </summary>
    /// <param name="side"></param>
    private void StressLeaveStatistics(GlobalSide side)
    {
        switch (side.level)
        {
            case 1:
                GameManager.instance.dataScript.StatisticIncrement(StatType.StressLeaveAuthority);
                break;
            case 2:
                GameManager.instance.dataScript.StatisticIncrement(StatType.StressLeaveResistance);
                break;
            default:
                Debug.LogErrorFormat("Unrecognised side \"{0}\"", side.name);
                break;
        }
    }

    /// <summary>
    /// Process Give Gear actor action (Resistance only)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessGiveGearAction(ModalActionDetails details)
    {
        int motivationBoost = 0;
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        Gear gear = null;
        Actor actor = null;
        StringBuilder builder = new StringBuilder();
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, details.side);
            if (actor != null)
            {
                gear = GameManager.instance.dataScript.GetGear(details.gearName);
                if (gear != null)
                {
                    //Give Gear
                    outcomeDetails.textTop = string.Format("{0}, {1}, thanks you for the {2}{3}{4}{5}", actor.arc.name, actor.actorName, "\n", colourNeutral, gear.tag, colourEnd);
                    //update actor details
                    string textGear = actor.AddGear(gear.name);
                    //gear stats
                    gear.statTimesGiven++;
                    //get actor's preferred gear
                    GearType preferredGear = actor.arc.preferredGear;
                    if (preferredGear != null)
                    {
                        bool isPreferred = false;
                        motivationBoost = gearSwapBaseAmount;
                        builder.AppendFormat("{0}{1} no longer available{2}", colourBad, gear.tag, colourEnd);
                        //motivation loss more if preferred gear
                        if (preferredGear.name.Equals(gear.type.name, System.StringComparison.Ordinal) == true)
                        {
                            //Preferred gear (extra motivation)
                            motivationBoost += gearSwapPreferredAmount;
                            isPreferred = true;
                        }
                        //give actor motivation boost
                        int motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                        motivation += motivationBoost;
                        motivation = Mathf.Min(GameManager.instance.actorScript.maxStatValue, motivation);
                        string colourMotivation = colourGood;
                        if (actor.SetDatapoint(ActorDatapoint.Motivation1, motivation, string.Format("Given {0} gear", gear.tag)) == false)
                        { colourMotivation = colourGrey; }
                        //motivation message
                        if (isPreferred == true)
                        {
                            builder.AppendFormat("{0}{1}{2}{3} Motivation +{4} {5}{6}{7}Preferred Gear{8}", "\n", "\n", colourMotivation, actor.arc.name, motivationBoost, colourEnd,
                                "\n", colourNeutral, colourEnd);
                        }
                        else { builder.AppendFormat("{0}{1}{2}{3} Motivation +{4}{5}", "\n", "\n", colourMotivation, actor.arc.name, gearSwapBaseAmount, colourEnd); }
                        //mood change
                        string moodText = GameManager.instance.personScript.UpdateMood(MoodType.GiveGear, actor.arc.name);
                        builder.AppendFormat("{0}{1}{2}", "\n", "\n", moodText);
                        //grace period note
                        if (actor.CheckTraitEffect(actorKeepGear) == false)
                        {
                            builder.AppendFormat("{0}{1}After {2}{3}{4} turn{5} you can ask for the gear back", "\n", "\n", colourNeutral, gearGracePeriod, colourEnd,
                                gearGracePeriod != 1 ? "s" : "");
                        }
                        else
                        {
                            //trait -> (Pack Rat) refuses to hand back gear
                            builder.AppendFormat("{0}{1}{2} has {3}{4}{5} trait{6}{7}REFUSES to handback gear{8}", "\n", "\n", actor.arc.name, colourNeutral, actor.GetTrait().tag,
                                colourEnd, "\n", colourBad, colourEnd);
                            GameManager.instance.actorScript.TraitLogMessage(actor, "for Returning Gear", "to AVOID doing so");
                        }
                        if (string.IsNullOrEmpty(textGear) == false)
                        {
                            //gear has been lost (actor already had some gear
                            builder.AppendFormat("{0}{1}{2}{3}{4}{5} has been Lost{6}", "\n", "\n", colourNeutral, textGear, colourEnd, colourBad, colourEnd);
                        }
                    }
                    else
                    { Debug.LogErrorFormat("Invalid preferredGear (Null) for actor Arc {0}", actor.arc.name); errorFlag = true; }

                }
                else { Debug.LogErrorFormat("Invalid gear (Null) for details.gearID {0}", details.gearName); errorFlag = true; }
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorSlotID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        if (errorFlag == false)
        {
            //Remove Gear
            if (gear != null)
            { GameManager.instance.playerScript.RemoveGear(gear.name); }
            outcomeDetails.sprite = actor.sprite;
            outcomeDetails.textBottom = builder.ToString();
            //message
            string text = string.Format("{0} ({1}) given to {2}, {3}", gear.tag, gear.rarity.name, actor.arc.name, actor.actorName);
            GameManager.instance.messageScript.GearTakeOrGive(text, actor, gear, motivationBoost);
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Give Gear";
            //is there a delegate method that needs processing?
            details.handler?.Invoke();
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessGiveGearAction");
    }

    /// <summary>
    /// Process Take Gear  from actor action (Resistance only)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessTakeGearAction(ModalActionDetails details)
    {
        int motivationCost = 0;
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        Gear gear = null;
        Actor actor = null;
        StringBuilder builder = new StringBuilder();
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, details.side);
            if (actor != null)
            {
                gear = GameManager.instance.dataScript.GetGear(details.gearName);
                if (gear != null)
                {
                    //Cost to take gear
                    motivationCost = gearSwapBaseAmount;
                    //Take gear custom message -> actor gets increasingly annoyed the more you take gear off them (no game effect)
                    string emotion = "respectfully";
                    switch (actor.GetGearTimesTaken())
                    {
                        case 1: emotion = "reluctantly"; break;
                        default: emotion = "resentfully"; break;
                    }
                    outcomeDetails.textTop = string.Format("{0}, {1}, <b>{2}</b> hands over the {3}{4}{5}{6}", actor.arc.name, actor.actorName, emotion,
                        "\n", colourNeutral, gear.tag, colourEnd);
                    //get actor's preferred gear
                    GearType preferredGear = actor.arc.preferredGear;
                    if (preferredGear != null)
                    {
                        bool isPreferred = false;
                        builder.AppendFormat("{0}{1} is available{2}", colourGood, gear.tag, colourEnd);
                        if (preferredGear.name.Equals(gear.type.name, System.StringComparison.Ordinal) == true)
                        {
                            //Preferred gear (extra motivation)
                            motivationCost += gearSwapPreferredAmount;
                            isPreferred = true;
                        }
                        //deduct motivation from actor
                        if (motivationCost > 0)
                        {
                            int motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                            //deduct motivation
                            motivation -= motivationCost;
                            motivation = Mathf.Max(0, motivation);
                            string colourMotivation = colourBad;
                            if (actor.SetDatapoint(ActorDatapoint.Motivation1, motivation, string.Format("{0} gear taken", gear.tag)) == false)
                            { colourMotivation = colourGrey; }
                            else if (motivation < motivationCost)
                            {
                                //relationship Conflict  (ActorConflict) -> Motivation change passes compatibility test
                                builder.AppendFormat("{0}{1}{2}{3} Motivation too Low!{4}", "\n", "\n", colourAlert, actor.arc.name, colourEnd);
                                builder.AppendFormat("{0}{1}RELATIONSHIP CONFLICT{2}", "\n", colourBad, colourEnd);
                                builder.AppendFormat("{0}{1}{2}", "\n", "\n", GameManager.instance.actorScript.ProcessActorConflict(actor));
                            }
                            //motivation message
                            if (isPreferred == true)
                            {
                                builder.AppendFormat("{0}{1}{2}{3} Motivation -{4}{5}{6}{7}Preferred Gear{8}", "\n", "\n", colourMotivation, actor.arc.name, motivationCost, colourEnd, "\n",
                                colourNeutral, colourEnd);
                            }
                            else { builder.AppendFormat("{0}{1}{2}{3} Motivation -{4}{5}", "\n", "\n", colourMotivation, actor.arc.name, motivationCost, colourEnd); }
                        }
                        else
                        {
                            //no motivation cost
                            builder.AppendFormat("{0}{1} loses {2}{3}No{4}{5} Motivation{6}", colourGood, actor.arc.name, colourEnd, colourNeutral, colourEnd, colourGood, colourEnd);
                        }
                        //mood change
                        string moodText = GameManager.instance.personScript.UpdateMood(MoodType.TakeGear, actor.arc.name);
                        builder.AppendFormat("{0}{1}{2}", "\n", "\n", moodText);
                    }
                    else
                    { Debug.LogErrorFormat("Invalid preferredGear (Null) for actor Arc {0}", actor.arc.name); errorFlag = true; }
                }
                else { Debug.LogErrorFormat("Invalid gear (Null) for details.gear {0}", details.gearName); errorFlag = true; }
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorSlotID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        if (errorFlag == false)
        {
            //Transfer Gear -> remove gear from actor
            actor.RemoveGear(GearRemoved.Taken);
            //Give Gear to Player
            GameManager.instance.playerScript.AddGear(gear.name);
            outcomeDetails.sprite = gear.sprite;
            outcomeDetails.textBottom = builder.ToString();
            //message
            string text = string.Format("{0} ({1}) taken back from {2}, {3}", gear.tag, gear.rarity.name, actor.arc.name, actor.actorName);
            GameManager.instance.messageScript.GearTakeOrGive(text, actor, gear, motivationCost, false);
            //action (if valid) expended -> must be BEFORE outcome window event
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Take Gear";
            //is there a delegate method that needs processing?
            details.handler?.Invoke();
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessGiveGearAction");
    }

    /// <summary>
    /// Process Use Gear (Player use) action (Resistance only)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessUseGearAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        bool isAction = false;
        //two builders for top and bottom texts
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
        if (node != null)
        {
            //resolve action
            if (details != null)
            {
                //Get Gear
                Gear gear = GameManager.instance.dataScript.GetGear(details.gearName);
                if (gear != null)
                {
                    List<Effect> listOfEffects = gear.listOfPersonalEffects;
                    if (listOfEffects != null && listOfEffects.Count > 0)
                    {
                        //return class
                        EffectDataReturn effectReturn = new EffectDataReturn();
                        //pass through data package
                        EffectDataInput dataInput = new EffectDataInput();
                        dataInput.originText = "Use " + gear.tag;
                        //
                        // - - - Process effects
                        //
                        for (int i = 0; i < listOfEffects.Count; i++)
                        {
                            Effect effect = listOfEffects[i];
                            //ongoing effect for gear ONLY player actions +/- (handled differently to other ongoing effects)
                            dataInput.ongoingID = GameManager.instance.effectScript.GetOngoingEffectID(); ;
                            /*dataInput.data = gear.gearID;*/
                            dataInput.ongoingText = string.Format("{0} gear", gear.tag);
                            effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput);
                            if (effectReturn != null)
                            {
                                //header, once only
                                if (i == 0)
                                {
                                    outcomeDetails.sprite = GameManager.instance.playerScript.sprite;
                                    builderTop.AppendFormat("{0} has been used by the Player", gear.tag);
                                }
                                //update stringBuilder texts
                                if (effectReturn.topText != null && effectReturn.topText.Length > 0)
                                {
                                    builderTop.AppendLine(); builderTop.AppendLine();
                                    builderTop.Append(effectReturn.topText);
                                }
                                if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                builderBottom.Append(effectReturn.bottomText);
                                //exit effect loop on error
                                if (effectReturn.errorFlag == true) { break; }
                                //valid action? -> only has to be true once for an action to be valid
                                if (effectReturn.isAction == true) { isAction = true; }
                            }
                            else
                            {
                                builderTop.AppendLine();
                                builderTop.Append("Error");
                                builderBottom.AppendLine();
                                builderBottom.Append("Error");
                                effectReturn.errorFlag = true;
                                break;
                            }
                        }
                        //texts
                        outcomeDetails.textTop = builderTop.ToString();
                        outcomeDetails.textBottom = builderBottom.ToString();
                    }
                    else
                    {
                        Debug.LogErrorFormat("There are no USE Effects for this \"{0}\" gear", gear.tag);
                        errorFlag = true;
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Invalid gear (Null) for gearID {0}", details.gearName);
                    errorFlag = true;
                }
            }
            else
            {
                errorFlag = true;
                Debug.LogError("Invalid ModalActionDetails (null) as argument");
            }
        }
        else
        {
            errorFlag = true;
            Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", GameManager.instance.nodeScript.nodePlayer);
        }
        //
        // - - - Outcome - - -
        //
        if (errorFlag == false)
        {
            //action (if valid) expended -> must be BEFORE outcome window event
            if (isAction == true)
            {
                outcomeDetails.isAction = true;
                outcomeDetails.reason = "Use Gear (Personal Use)";
            }
            //Gear Used            
            Gear gear = GameManager.instance.dataScript.GetGear(details.gearName);
            if (gear != null)
            { GameManager.instance.gearScript.SetGearUsed(gear, "provide Player with a benefit"); }
            else
            {
                Debug.LogErrorFormat("Invalid Gear (Null) for gear {0}", details.gearName);
                errorFlag = true;
            }
            outcomeDetails.textTop = builderTop.ToString();
            outcomeDetails.textBottom = builderBottom.ToString();
            //is there a delegate method that needs processing?
            details.handler?.Invoke();
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessUseGearAction");
    }


    /// <summary>
    /// Reserve pool actor is reassured via the right click action menu
    /// NOTE: calling method checks unhappyTimer > 0 & isReassured is false
    /// </summary>
    /// <param name="actorID"></param>
    private void ProcessReassureActor(ModalActionDetails details)
    {
        int benefit = GameManager.instance.actorScript.unhappyReassureBoost;
        bool errorFlag = false;
        string moodText = "Unknown";
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        Actor actor = null;
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetActor(details.actorDataID);
            if (actor != null)
            {
                //traits
                string traitText = "";
                if (actor.CheckTraitEffect(actorReserveActionDoubled) == true)
                {
                    benefit *= 2; traitText = string.Format(" ({0})", actor.GetTrait().tag);
                    GameManager.instance.actorScript.TraitLogMessage(actor, "for being Reassured", "to DOUBLE effect of being Reassured");
                }
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("{0}{1} Unhappy timer +{2}{3}{4}{5}{6}{7}{8} can't be Reassured again{9}", colourGood, actor.actorName,
                    benefit, traitText, colourEnd, "\n", "\n", colourNeutral, actor.actorName, colourEnd);
                //mood text
                moodText = GameManager.instance.personScript.UpdateMood(MoodType.ReserveReassure, actor.arc.name);
                builder.AppendFormat("{0}{1}{2}", "\n", "\n", moodText);
                //outcome
                outcomeDetails.textTop = string.Format("{0} {1} has been reassured that they will be the next person called for active duty",
                    actor.arc.name, actor.actorName);
                outcomeDetails.textBottom = builder.ToString();
                outcomeDetails.sprite = actor.sprite;
                //Give boost to Unhappy timer
                actor.unhappyTimer += benefit;
                actor.isReassured = true;
                //message
                string text = string.Format("{0} {1} has been Reassured (Reserve Pool)", actor.arc.name, actor.actorName);
                GameManager.instance.messageScript.ActorSpokenToo(text, "Reassured", actor, benefit);
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorDataID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        //outcome
        if (errorFlag == false)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Reassure Actor";
            //is there a delegate method that needs processing?
            details.handler?.Invoke();
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessReassureActor");
    }


    /// <summary>
    /// Reserve pool actor is Bullied via the right click action menu
    /// NOTE: calling method checks unhappyTimer > 0 & that sufficient renown onhand
    /// </summary>
    /// <param name="actorID"></param>
    private void ProcessBullyActor(ModalActionDetails details)
    {
        int benefit = GameManager.instance.actorScript.unhappyBullyBoost;
        bool errorFlag = false;
        string moodText = "Unknown";
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        Actor actor = null;
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetActor(details.actorDataID);
            if (actor != null)
            {
                //renown Cost
                int renownCost = actor.numOfTimesBullied + 1;
                actor.numOfTimesBullied++;
                //traits
                string traitText = "";
                if (actor.CheckTraitEffect(actorReserveActionDoubled) == true)
                {
                    benefit *= 2; traitText = string.Format(" ({0})", actor.GetTrait().tag);
                    GameManager.instance.actorScript.TraitLogMessage(actor, "for being Bullied", "to DOUBLE effect of being Bullied");
                }
                //outcome
                outcomeDetails.textTop = string.Format("{0} {1} has been pulled into line and told to smarten up their attitude",
                    actor.arc.name, actor.actorName);
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("{0}{1}'s{2}Unhappy timer +{3}{4}{5}", colourGood, actor.actorName, "\n", benefit, traitText, colourEnd);
                builder.AppendLine(); builder.AppendLine();
                builder.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownCost, colourEnd);
                builder.AppendLine(); builder.AppendLine();
                builder.AppendFormat("{0}{1} can be Bullied again for {2} Renown{3}", colourNeutral, actor.actorName, actor.numOfTimesBullied + 1, colourEnd);
                //mood text
                moodText = GameManager.instance.personScript.UpdateMood(MoodType.ReserveBully, actor.arc.name);
                builder.AppendFormat("{0}{1}{2}", "\n", "\n", moodText);
                //outcome details
                outcomeDetails.textBottom = builder.ToString();
                outcomeDetails.sprite = actor.sprite;
                //Give boost to Unhappy timer
                actor.unhappyTimer += benefit;
                //Deduct Player renown
                GameManager.instance.playerScript.Renown -= renownCost;
                //message
                string text = string.Format("{0} {1} has been Threatened (Reserve Pool)", actor.arc.name, actor.actorName);
                GameManager.instance.messageScript.ActorSpokenToo(text, "Threatened", actor, benefit);
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorDataID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        //outcome
        if (errorFlag == false)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Threaten Actor";
            //is there a delegate method that needs processing?
            details.handler?.Invoke();
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessThreatenActor");
    }


    /// <summary>
    /// Reserve pool actor is Let Go via the right click action menu
    /// NOTE: calling method checks unhappyTimer > 0 and isNewRecruit is true
    /// </summary>
    /// <param name="actorID"></param>
    private void ProcessLetGoActor(ModalActionDetails details)
    {
        int motivationLoss = GameManager.instance.actorScript.motivationLossLetGo;
        bool errorFlag = false;
        int numOfTeams = 0;
        string moodText = "Unknown";
        StringBuilder builder = new StringBuilder();
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        Actor actor = null;
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetActor(details.actorDataID);
            if (actor != null)
            {
                //authority actor?
                if (details.side.level == GameManager.instance.globalScript.sideAuthority.level)
                {
                    //remove all active teams connected with this actor
                    numOfTeams = GameManager.instance.teamScript.TeamCleanUp(actor);
                }
                //lower actors motivation
                int motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                motivation -= motivationLoss;
                motivation = Mathf.Max(0, motivation);
                actor.SetDatapoint(ActorDatapoint.Motivation1, motivation, "Let Go from Reserves");
                builder.AppendFormat("{0}{1} Motivation -{2}{3}", colourBad, actor.actorName, motivationLoss, colourEnd);
                //change actors status
                actor.Status = ActorStatus.RecruitPool;
                actor.ResetStates();
                //place actor back in the appropriate recruit pool
                List<int> recruitPoolList = GameManager.instance.dataScript.GetActorRecruitPool(actor.level, details.side);
                if (recruitPoolList != null)
                {
                    recruitPoolList.Add(actor.actorID);
                    builder.AppendLine(); builder.AppendLine();
                    builder.AppendFormat("{0}{1} can be recruited later{2}", colourNeutral, actor.actorName, colourEnd);
                }
                else { Debug.LogErrorFormat("Invalid recruitPoolList (Null) for actor.level {0} & GlobalSide {1}", actor.level, details.side); }
                //remove actor from reserve list
                GameManager.instance.dataScript.RemoveActorFromReservePool(details.side, actor);
                //teams
                if (numOfTeams > 0)
                {
                    if (builder.Length > 0)
                    { builder.AppendLine(); builder.AppendLine(); }
                    builder.AppendFormat("{0}{1} related Team{2} sent to the Reserve Pool{3}", colourBad, numOfTeams,
                    numOfTeams != 1 ? "s" : "", colourEnd);
                }
                //mood text
                moodText = GameManager.instance.personScript.UpdateMood(MoodType.ReserveLetGo, actor.arc.name);
                builder.AppendFormat("{0}{1}{2}", "\n", "\n", moodText);
                //outcome details
                outcomeDetails.textTop = string.Format("{0} {1} reluctantly returns to the recruitment pool and asks that you keep them in mind", actor.arc.name,
                    actor.actorName);
                outcomeDetails.textBottom = builder.ToString();
                outcomeDetails.sprite = actor.sprite;
                //message
                string text = string.Format("{0} {1} has been Let Go (Reserve Pool)", actor.arc.name, actor.actorName);
                GameManager.instance.messageScript.ActorStatus(text, "Let Go", "has been Let Go", actor.actorID, details.side);

            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorDataID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        //outcome
        if (errorFlag == false)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Let Actor Go";
            //is there a delegate method that needs processing?
            details.handler?.Invoke();
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessLetGoActor");
    }

    /// <summary>
    /// Reserve pool actor is Fired via the right click action menu
    /// NOTE: calling method checks that Player has enough renown
    /// </summary>
    /// <param name="actorID"></param>
    private void ProcessFireReserveActor(ModalActionDetails details)
    {
        /*int motivationLoss = GameManager.instance.actorScript.motivationLossFire;*/
        bool errorFlag = false;
        int numOfTeams = 0;
        string moodText = "Unknown";
        Debug.Assert(details.renownCost > 0, "Invalid renownCost (zero)");
        StringBuilder builder = new StringBuilder();
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        Actor actor = null;
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetActor(details.actorDataID);
            if (actor != null)
            {
                //authority actor?
                if (details.side.level == GameManager.instance.globalScript.sideAuthority.level)
                {
                    //remove all active teams connected with this actor
                    numOfTeams = GameManager.instance.teamScript.TeamCleanUp(actor);
                }

                //change actors status
                actor.Status = ActorStatus.Dismissed;
                actor.ResetStates();
                //remove actor from reserve list
                GameManager.instance.dataScript.RemoveActorFromReservePool(details.side, actor);

                /*//remove actor from map & handle admin -> EDIT: Can't remove actor from Map as actor is in reserve pool
                if (GameManager.instance.dataScript.RemoveCurrentActor(details.side, actor, actor.Status) == false)
                { Debug.LogErrorFormat("Actor \"{0}\", {1}, actorID {2}, not removed", actor.actorName, actor.arc.name, actor.actorID); }*/

                GameManager.instance.dataScript.AddActorToDismissed(actor.actorID, details.side);
                //lose secrets (keep record of how many there were to enable accurate renown cost calc's)
                actor.departedNumOfSecrets = actor.CheckNumOfSecrets();
                GameManager.instance.secretScript.RemoveAllSecretsFromActor(actor);

                //Renown cost
                int playerRenown = GameManager.instance.playerScript.Renown;
                ManageRenownCost renownData = GameManager.instance.actorScript.GetManageRenownCost(actor, GameManager.instance.actorScript.manageDismissRenown);
                playerRenown -= renownData.renownCost;
                //min capped at Zero
                playerRenown = Mathf.Max(0, playerRenown);
                GameManager.instance.playerScript.Renown = playerRenown;
                if (renownData.tooltip.Length > 0)
                {
                    builder.AppendLine(renownData.tooltip);
                    builder.AppendLine();
                }
                builder.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownData.renownCost, colourEnd);

                /*if (actor.isThreatening == true)
                {
                    builder.AppendFormat("{0} (Double Cost as {1} was Threatening Player{2}", colourAlert, actor.actorName, colourEnd);
                    builder.AppendLine(); builder.AppendLine();
                    builder.AppendFormat("{0}{1} is no longer a threat{2}", colourGood, actor.actorName, colourEnd);
                }
                builder.AppendLine(); builder.AppendLine();*/

                //teams
                if (numOfTeams > 0)
                {
                    if (builder.Length > 0)
                    { builder.AppendLine(); builder.AppendLine(); }
                    builder.AppendFormat("{0}{1} related Team{2} sent to the Reserve Pool{3}", colourBad, numOfTeams,
                    numOfTeams != 1 ? "s" : "", colourEnd);
                }
                //mood text
                moodText = GameManager.instance.personScript.UpdateMood(MoodType.ReserveFire, actor.arc.name);
                builder.AppendFormat("{0}{1}{2}", "\n", "\n", moodText);
                //outcome details
                outcomeDetails.textTop = string.Format("{0} {1} curses and spits at your feet before walking out the door", actor.arc.name,
                    actor.actorName);
                outcomeDetails.textBottom = builder.ToString();
                outcomeDetails.sprite = actor.sprite;
                //message
                string text = string.Format("{0} {1} has been Dismissed (Reserve Pool)", actor.arc.name, actor.actorName);
                GameManager.instance.messageScript.ActorStatus(text, "Dismissed", "has been Fired from the Reserves", actor.actorID, details.side);

            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorDataID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        //outcome
        if (errorFlag == false)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Dismiss Actor";
            //is there a delegate method that needs processing?
            details.handler?.Invoke();
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessFireActor");
    }

    /// <summary>
    /// Reserve pool actor is recalled for Active Duty via the right click action menu
    /// NOTE: calling method checks unhappyTimer > 0 and isNewRecruit is true
    /// </summary>
    /// <param name="actorID"></param>
    private void ProcessActiveDutyActor(ModalActionDetails details)
    {
        int motivationGain = GameManager.instance.actorScript.motivationGainActiveDuty;
        bool errorFlag = false;
        StringBuilder builder = new StringBuilder();
        ModalOutcomeDetails outcomeDetails = SetDefaultOutcome(details);
        Actor actor = null;
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetActor(details.actorDataID);
            if (actor != null)
            {
                int actorSlotID = GameManager.instance.dataScript.CheckForSpareActorSlot(details.side);
                if (actorSlotID > -1)
                {
                    //raise actors motivation
                    int motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                    motivation += motivationGain;
                    motivation = Mathf.Min(GameManager.instance.actorScript.maxStatValue, motivation);
                    actor.SetDatapoint(ActorDatapoint.Motivation1, motivation, "Recalled for Active Duty");
                    builder.AppendFormat("{0}{1} Motivation +{2}{3}", colourGood, actor.actorName, motivationGain, colourEnd);
                    //was actor threatening
                    if (actor.isThreatening == true)
                    {
                        builder.AppendLine(); builder.AppendLine();
                        builder.AppendFormat("{0}{1} is no longer Threatening{2}", colourGood, actor.actorName, colourEnd);
                    }
                    //was actor unhappy
                    if (actor.RemoveCondition(conditionUnhappy, string.Format("{0} is no longer Unhappy", actor.actorName)) == true)
                    {
                        builder.AppendLine(); builder.AppendLine();
                        builder.AppendFormat("{0}{1}'s is no longer Unhappy{2}", colourGood, actor.actorName, colourEnd);
                    }
                    //place actor on Map (reset states)
                    GameManager.instance.dataScript.AddCurrentActor(details.side, actor, actorSlotID);
                    //remove actor from reserve list
                    GameManager.instance.dataScript.RemoveActorFromReservePool(details.side, actor);
                    //Authority Actor brings team with them (if space available)
                    if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                    {
                        //is there room available for another team?
                        if (GameManager.instance.aiScript.CheckNewTeamPossible() == true)
                        {
                            TeamArc teamArc = actor.arc.preferredTeam;
                            if (teamArc != null)
                            {
                                if (teamArc.TeamArcID > -1)
                                {
                                    //add new team to reserve pool
                                    int teamCount = GameManager.instance.dataScript.CheckTeamInfo(teamArc.TeamArcID, TeamInfo.Total);
                                    Team team = new Team(teamArc.TeamArcID, teamCount);
                                    //update team info
                                    GameManager.instance.dataScript.AdjustTeamInfo(teamArc.TeamArcID, TeamInfo.Reserve, +1);
                                    GameManager.instance.dataScript.AdjustTeamInfo(teamArc.TeamArcID, TeamInfo.Total, +1);
                                    //message
                                    string msgText = string.Format("{0} {1} added to Reserves", team.arc.name, team.teamName);
                                    string reason = string.Format("Due to {0} {1}, {2}{3}{4}, being recalled for active duty", GameManager.instance.metaScript.GetAuthorityTitle(),
                                        actor.actorName, colourAlert, actor.arc.name, colourEnd);
                                    GameManager.instance.messageScript.TeamAdd(msgText, reason, team);
                                    builder.AppendFormat(string.Format("{0}{1}{2}{3}{4}", "\n", "\n", colourGood, msgText, colourEnd));
                                }
                                else { Debug.LogWarningFormat("Invalid teamArcID {0} for {1}", teamArc.TeamArcID, teamArc.name); }
                            }
                            else
                            { Debug.LogWarningFormat("Invalid preferred team Arc (Null) for actorID {0}, {1}, \"{2}\"", actor.actorID, actor.arc.name, actor.actorName); }
                        }
                        else { builder.AppendFormat("{0}{1}{2}{3}{4}", "\n", "\n", colourAlert, "New team not available as roster is full", colourEnd); }
                    }
                    //outcome dialogue
                    outcomeDetails.textTop = string.Format("{0} {1} bounds forward and enthusiastically shakes your hand", actor.arc.name,
                        actor.actorName);
                    outcomeDetails.textBottom = builder.ToString();
                    outcomeDetails.sprite = actor.sprite;
                    //message
                    string text = string.Format("{0} {1} called for Active Duty (Reserve Pool)", actor.arc.name, actor.actorName);
                    GameManager.instance.messageScript.ActorStatus(text, "called for Active Duty", "has been called for Active Duty", actor.actorID, details.side);
                }
                else { Debug.LogError("There are no vacancies on map for the actor to be recalled to Active Duty"); errorFlag = true; }
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorDataID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        //outcome
        if (errorFlag == false)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Active Duty";
            //is there a delegate method that needs processing?
            details.handler?.Invoke();
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessActiveDutyActor");
    }

    /// <summary>
    /// Process attempt on node Target
    /// </summary>
    /// <param name="details"></param>
    public void ProcessNodeTarget(int nodeID)
    {
        bool errorFlag = false;
        bool isAction = false;
        bool isSuccessful = false;
        bool isZeroInvisibility = false;
        bool isPlayerAction = false;
        int actorID = GameManager.instance.playerScript.actorID;
        string text;
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        CaptureDetails details = new CaptureDetails();
        Actor actor = null;
        if (node != null)
        {
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            //two builders for top and bottom texts
            StringBuilder builderTop = new StringBuilder();
            StringBuilder builderBottom = new StringBuilder();
            //target
            Target target = GameManager.instance.dataScript.GetTarget(node.targetName);
            if (target != null)
            {
                //
                // - - - Actor/Player captured beforehand (target is safe if captured) -> if so exit - - -
                //

                //Player
                if (nodeID == GameManager.instance.nodeScript.nodePlayer)
                {
                    isPlayerAction = true;
                    details = GameManager.instance.captureScript.CheckCaptured(nodeID, actorID);
                    if (GameManager.instance.playerScript.Invisibility == 0)
                    { isZeroInvisibility = true; }
                }
                //Actor
                else
                {
                    //check correct actor arc for target is present in line up
                    int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.name, GameManager.instance.globalScript.sideResistance);
                    if (slotID > -1)
                    {
                        //get actor
                        actor = GameManager.instance.dataScript.GetCurrentActor(slotID, GameManager.instance.globalScript.sideResistance);
                        if (actor != null)
                        {
                            details = GameManager.instance.captureScript.CheckCaptured(nodeID, actor.actorID);
                            actorID = actor.actorID;
                            if (actor.GetDatapoint(ActorDatapoint.Invisibility2) == 0)
                            { isZeroInvisibility = true; }
                        }
                        else
                        { Debug.LogErrorFormat("Invalid actor (Null) for slotID {0}", slotID); errorFlag = true; }
                    }
                    else
                    { Debug.LogErrorFormat("Invalid slotID (-1) for target.actorArc.name {0}", target.actorArc.name); }
                }
                //Player/Actor captured (provided no errors, otherwise bypass)
                if (errorFlag == false)
                {
                    if (details != null)
                    {
                        //Target aborted, deal with Capture
                        details.effects = string.Format("{0} Attempt on Target \"{1}\" misfired{2}", colourNeutral, target.targetName, colourEnd);
                        EventManager.instance.PostNotification(EventType.Capture, this, details, "ActionManager.cs -> ProcessNodeTarget");
                        return;
                    }
                }
                else
                {
                    //reset flag
                    errorFlag = false;
                }
                //NOT captured, proceed with target

                //
                // - - - Attempt Target - - -  
                //
                isAction = true;
                int tally = GameManager.instance.targetScript.GetTargetTally(target.name, true);
                int chance = GameManager.instance.targetScript.GetTargetChance(tally);
                Debug.LogFormat("[Tar] TargetManager.cs -> ProcessNodeTarget: Target {0}{1}", target.targetName, "\n");
                target.numOfAttempts++;
                GameManager.instance.dataScript.StatisticIncrement(StatType.TargetAttempts);
                int roll = Random.Range(0, 100);
                if (roll < chance)
                {
                    //Success
                    isSuccessful = true;
                    GameManager.instance.dataScript.StatisticIncrement(StatType.TargetSuccesses);
                    target.turnSuccess = GameManager.instance.turnScript.Turn;
                    //objective
                    GameManager.instance.objectiveScript.CheckObjectiveTarget(target);
                    //Ongoing effects then target moved to completed pool
                    if (target.ongoingEffect != null)
                    {
                        GameManager.instance.dataScript.RemoveTargetFromPool(target, Status.Live);
                        GameManager.instance.dataScript.AddTargetToPool(target, Status.Outstanding);
                        target.targetStatus = Status.Outstanding;
                    }
                    else
                    {
                        //NO ongoing effects -> target  done with. 
                        GameManager.instance.targetScript.SetTargetDone(target, node);
                    }
                    text = string.Format("Target \"{0}\" successfully attempted", target.targetName, "\n");
                    GameManager.instance.messageScript.TargetAttempt(text, node, actorID, target);
                    //random roll
                    Debug.LogFormat("[Rnd] TargetManager.cs -> ProcessNodeTarget: Target attempt SUCCESS need < {0}, rolled {1}{2}", chance, roll, "\n");
                    text = string.Format("Target {0} attempt SUCCESS", target.targetName);
                    GameManager.instance.messageScript.GeneralRandom(text, "Target Attempt", chance, roll);
                }
                else
                {
                    Debug.LogFormat("[Rnd] TargetManager.cs -> ProcessNodeTarget: Target attempt FAILED need < {0}, rolled {1}{2}", chance, roll, "\n");
                    text = string.Format("Target {0} attempt FAILED", target.targetName);
                    GameManager.instance.messageScript.GeneralRandom(text, "Target Attempt", chance, roll);
                }
                //set isTargetKnown -> auto if success, % chance otherwise
                if (isSuccessful == true) { node.isTargetKnown = true; }
                else
                {
                    //chance of being known
                    if (isZeroInvisibility == false)
                    {
                        roll = Random.Range(0, 100);
                        if (roll < failedTargetChance)
                        {
                            node.isTargetKnown = true;
                            Debug.LogFormat("[Rnd] TargetManager.cs -> ProcessNodeTarget: Target attempt KNOWN need < {0}, rolled {1}{2}", failedTargetChance, roll, "\n");
                        }
                        else
                        { Debug.LogFormat("[Rnd] TargetManager.cs -> ProcessNodeTarget: Target attempt UNDETECTED need < {0}, rolled {1}{2}", failedTargetChance, roll, "\n"); }
                    }
                    //if zero invisibility then target auto known to authorities
                    else { node.isTargetKnown = true; }
                }
                Debug.LogFormat("[Tar] TargetManager.cs -> ProcessNodeTarget: Authority aware of target: {0}", node.isTargetKnown);
                //
                // - - - Effects - - - 
                //
                List<Effect> listOfEffects = new List<Effect>();
                //target SUCCESSFUL
                if (isSuccessful == true)
                {
                    builderTop.AppendFormat("{0}{1}{2}{3}Attempt <b>Successful</b>", colourNeutral, target.targetName, colourEnd, "\n");
                    //combine all effects into one list for processing (mood effect only applies if it's the player attempting the target)
                    listOfEffects.AddRange(target.listOfGoodEffects);
                    listOfEffects.AddRange(target.listOfBadEffects);
                    listOfEffects.Add(target.ongoingEffect);
                    if (isPlayerAction == true)
                    { listOfEffects.Add(target.moodEffect); }
                }
                else
                {
                    //FAILED target attempt
                    listOfEffects.AddRange(target.listOfFailEffects);
                    builderTop.AppendFormat("{0}{1}{2}{3}Attempt Failed!", colourNeutral, target.targetName, colourEnd, "\n");
                    if (isZeroInvisibility == false)
                    { builderBottom.AppendFormat("{0}There is a {1} % chance of the Authority becoming aware of the attempt{2}", colourAlert, failedTargetChance, colourEnd); }
                    else
                    { builderBottom.AppendFormat("{0}Authorities are aware of the attempt (due to Zero Invisibility){1}", colourBad, colourEnd); }
                    text = string.Format("Target \"{0}\" unsuccessfully attempted", target.targetName, "\n");
                    GameManager.instance.messageScript.TargetAttempt(text, node, actorID, target);
                }
                //Process effects
                EffectDataReturn effectReturn = new EffectDataReturn();
                //pass through data package
                EffectDataInput dataInput = new EffectDataInput();
                dataInput.originText = string.Format("{0} target", target.targetName);
                //handle any Ongoing effects of target completed -> only if target Successful
                if (isSuccessful == true && target.ongoingEffect != null)
                {
                    dataInput.ongoingID = GameManager.instance.effectScript.GetOngoingEffectID();
                    dataInput.ongoingText = target.reasonText;
                    //add to target so it can link to effects
                    target.ongoingID = dataInput.ongoingID;
                }
                //any effects to process?
                if (listOfEffects.Count > 0)
                {
                    foreach (Effect effect in listOfEffects)
                    {
                        if (effect != null)
                        {
                            effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actor);
                            if (effectReturn != null)
                            {
                                //update stringBuilder texts (Bottom only)
                                if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                builderBottom.Append(effectReturn.bottomText);
                                //exit effect loop on error
                                if (effectReturn.errorFlag == true) { break; }
                            }
                            else
                            {
                                builderTop.AppendLine();
                                builderTop.Append("Error");
                                builderBottom.AppendLine();
                                builderBottom.Append("Error");
                                effectReturn.errorFlag = true;
                                break;
                            }
                        }
                        /*else { Debug.LogWarning("Invalid effect (Null)"); }  EDIT -> No need for a warning as some effects, eg. Ongoing, mood, etc. can be null*/
                    }
                    //target has an objective?
                    string objectiveInfo = GameManager.instance.objectiveScript.CheckObjectiveInfo(target.name);
                    if (objectiveInfo != null)
                    {
                        if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                        builderBottom.AppendFormat("{0}Objective {1}{2}", colourGood, objectiveInfo, colourEnd);
                    }
                }

                //Gear -> handled by TargetManager.cs -> GetTargetTally

                //
                // - - - Outcome - - -
                //                        
                //action (if valid) expended -> must be BEFORE outcome window event
                outcomeDetails.isAction = isAction;
                if (isSuccessful == true)
                { outcomeDetails.reason = "Target Success"; }
                else { outcomeDetails.reason = "Target Fail"; }
                if (errorFlag == false)
                {
                    //outcome
                    outcomeDetails.side = GameManager.instance.globalScript.sideResistance;
                    outcomeDetails.textTop = builderTop.ToString();
                    outcomeDetails.textBottom = builderBottom.ToString();
                    //which sprite to use
                    if (isSuccessful == true) { outcomeDetails.sprite = GameManager.instance.guiScript.targetSuccessSprite; }
                    else { outcomeDetails.sprite = GameManager.instance.guiScript.targetFailSprite; }
                }
                //generate a create modal window event
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessNodeTarget");
            }
            else { Debug.LogErrorFormat("Invalid Target (Null) for node.targetID {0}", nodeID); }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", nodeID); }

    }

    /// <summary>
    /// processes final selection for a Reserve Actor Action (both sides)
    /// </summary>
    private void ProcessReserveActorAction(GenericReturnData data)
    {
        bool successFlag = true;
        string msgText = "Unknown";
        int numOfTeams = 0;
        string gearName;
        string moodText = "Unknown";
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        Sprite sprite = GameManager.instance.guiScript.errorSprite;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        if (data != null)
        {
            if (data.actorSlotID > -1)
            {
                //find actor
                Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, playerSide);
                if (actor != null)
                {
                    gearName = actor.GetGearName();
                    //add actor to reserve pool
                    if (GameManager.instance.dataScript.RemoveCurrentActor(playerSide, actor, ActorStatus.Reserve) == true)
                    {
                        //sprite of recruited actor
                        sprite = actor.sprite;
                        //authority actor?
                        if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                        {
                            //remove all active teams connected with this actor
                            numOfTeams = GameManager.instance.teamScript.TeamCleanUp(actor);
                        }
                        builderTop.AppendLine();
                        //actor successfully moved to reserve
                        if (string.IsNullOrEmpty(data.optionNested) == false)
                        {
                            switch (data.optionNested)
                            {
                                case "ReserveRest":
                                    builderTop.AppendFormat("{0}{1} {2} understands the need for Rest{3}", colourNormal, actor.arc.name,
                                        actor.actorName, colourEnd);
                                    msgText = "Resting";
                                    moodText = GameManager.instance.personScript.UpdateMood(MoodType.ReserveRest, actor.arc.name);
                                    break;
                                case "ReservePromise":
                                    builderTop.AppendFormat("{0}{1} {2} accepts your word that they will be recalled within a reasonable time period{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd);
                                    msgText = "Promised";
                                    moodText = GameManager.instance.personScript.UpdateMood(MoodType.ReservePromise, actor.arc.name);
                                    break;
                                case "ReserveNoPromise":
                                    builderTop.AppendFormat("{0}{1} {2} is confused and doesn't understand why they are being cast aside{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd);
                                    msgText = "No Promise";
                                    moodText = GameManager.instance.personScript.UpdateMood(MoodType.ReserveNoPromise, actor.arc.name);
                                    break;
                                default:
                                    Debug.LogErrorFormat("Invalid data.optionText \"{0}\"", data.optionNested);
                                    break;
                            }
                            //teams
                            if (numOfTeams > 0)
                            {
                                if (builderBottom.Length > 0)
                                { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                builderBottom.AppendFormat("{0}{1} related Team{2} sent to the Reserve Pool{3}", colourBad, numOfTeams,
                                numOfTeams != 1 ? "s" : "", colourEnd);
                            }
                            //gear
                            if (string.IsNullOrEmpty(gearName) == false)
                            {
                                Gear gear = GameManager.instance.dataScript.GetGear(gearName);
                                if (gear != null)
                                {
                                    if (builderBottom.Length > 0)
                                    { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                    builderBottom.AppendFormat("{0}{1} gear Lost{2}", colourBad, gear.tag, colourEnd);
                                    //message
                                    string gearText = string.Format("{0} gear lost by {1}, {2}", gear.tag, actor.actorName, actor.arc.name);
                                    GameManager.instance.messageScript.GearLost(gearText, gear, actor);
                                }
                                else { Debug.LogWarningFormat("Invalid Gear (Null) for gear {0}", gearName); }
                            }
                        }
                        else
                        {
                            //default data for missing outcome
                            Debug.LogWarningFormat("Invalid optionText (Null or empty) for {0} {1}", actor.actorName, actor.arc.name);
                            builderTop.AppendFormat("{0} {1} is sent to the Reserves", actor.arc.name, actor.actorName);
                        }
                        //message
                        string text = string.Format("{0} {1} moved to the Reserves ({2})", actor.arc.name, actor.actorName, msgText);
                        GameManager.instance.messageScript.ActorStatus(text, "sent to Reserves", "has been moved to the Reserves", actor.actorID, playerSide);
                        //Process any other effects, if move to the Reserve pool was successful, ignore otherwise
                        ManageAction manageAction = GameManager.instance.dataScript.GetManageAction(data.optionNested);
                        if (manageAction != null)
                        {
                            List<Effect> listOfEffects = manageAction.listOfEffects;
                            if (listOfEffects.Count > 0)
                            {
                                EffectDataInput dataInput = new EffectDataInput();
                                dataInput.originText = "Reserve Actor";
                                foreach (Effect effect in listOfEffects)
                                {
                                    if (effect.ignoreEffect == false)
                                    {
                                        EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, null, dataInput, actor);
                                        if (effectReturn != null)
                                        {
                                            if (!string.IsNullOrEmpty(effectReturn.topText) && builderTop.Length > 0) { builderTop.AppendLine(); }
                                            builderTop.Append(effectReturn.topText);
                                            if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                            builderBottom.Append(effectReturn.bottomText);
                                            //exit effect loop on error
                                            if (effectReturn.errorFlag == true) { break; }
                                        }
                                        else { Debug.LogError("Invalid effectReturn (Null)"); }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.LogErrorFormat("Invalid ManageAction (Null) for data.optionText \"{0}\"", data.optionNested);
                            successFlag = false;
                        }
                    }
                    else
                    {
                        //some issue prevents actor being added to reserve pool (full? -> probably not as a criteria checks this)
                        successFlag = false;
                    }
                }
                else
                {
                    Debug.LogWarningFormat("Invalid Actor (Null) for actorSlotID {0}", data.optionID);
                    successFlag = false;
                }

            }
            else
            { Debug.LogWarningFormat("Invalid actorSlotID {0}", data.actorSlotID); }
        }
        else
        {
            Debug.LogError("Invalid GenericReturnData (Null)");
            successFlag = false;
        }
        //failed outcome
        if (successFlag == false)
        {
            builderTop.Append("Something has gone wrong. You are unable to move anyone to the Reserve Pool at present");
            builderBottom.Append("It's the wiring. It's broken. Rats. Big ones.");
        }
        //mood info
        builderBottom.AppendFormat("{0}{1}{2}", "\n", "\n", moodText);
        //
        // - - - Outcome - - - 
        //
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        outcomeDetails.textTop = builderTop.ToString();
        outcomeDetails.textBottom = builderBottom.ToString();
        outcomeDetails.sprite = sprite;
        outcomeDetails.side = playerSide;
        //action expended automatically for manage actor
        if (successFlag == true)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Manager Reserve Actor";
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessReserveActorAction");
    }

    /// <summary>
    /// processes final selection for a Dismiss Actor Action
    /// </summary>
    private void ProcessDismissActorAction(GenericReturnData data)
    {
        bool successFlag = true;
        string msgTextStatus = "Unknown";
        string msgTextMain = "Who Knows?";
        string msgReason = "Unknown";
        string moodText = "Unknown";
        int numOfTeams = 0;
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        Sprite sprite = GameManager.instance.guiScript.errorSprite;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        ActorStatus status = ActorStatus.Dismissed;
        if (data != null)
        {
            if (string.IsNullOrEmpty(data.optionNested) == false)
            {
                if (data.actorSlotID > -1)
                {
                    //find actor
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, playerSide);
                    if (actor != null)
                    {
                        if (data.optionNested.Equals("DismissPromote", System.StringComparison.Ordinal) == true)
                        { status = ActorStatus.Promoted; }
                        //add actor to the dismissed or promoted lists
                        if (GameManager.instance.dataScript.RemoveCurrentActor(playerSide, actor, status) == true)
                        {
                            //sprite of recruited actor
                            sprite = actor.sprite;
                            //authority actor?
                            if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                            {
                                //remove all active teams connected with this actor
                                numOfTeams = GameManager.instance.teamScript.TeamCleanUp(actor);
                            }
                            builderTop.AppendLine();
                            //actor successfully dismissed or promoted
                            switch (data.optionNested)
                            {
                                case "DismissPromote":
                                    builderTop.AppendFormat("{0}{1} {2} shakes your hand and heads off to bigger things{3}", colourNormal, actor.arc.name,
                                        actor.actorName, colourEnd);
                                    msgTextStatus = "Promoted";
                                    msgReason = "Promoted";
                                    msgTextMain = string.Format("{0} {1} has been Promoted ({2})", actor.arc.name, actor.actorName,
                                        GameManager.instance.factionScript.GetCurrentFaction().name);
                                    moodText = GameManager.instance.personScript.UpdateMood(MoodType.DismissPromote, actor.arc.name);
                                    break;
                                case "DismissIncompetent":
                                    builderTop.AppendFormat("{0}{1} {2} scowls and curses before stomping off{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd);
                                    msgTextStatus = "Incompetent";
                                    msgReason = "dismissed for Incompetence";
                                    msgTextMain = string.Format("{0} {1} has been Dismissed ({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    moodText = GameManager.instance.personScript.UpdateMood(MoodType.DismissIncompetent, actor.arc.name);
                                    break;
                                case "DismissUnsuited":
                                    builderTop.AppendFormat("{0}{1} {2} lets you know that they won't forget this{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd);
                                    msgTextStatus = "Unsuited";
                                    msgReason = "dismissed as Unsuited for role";
                                    msgTextMain = string.Format("{0} {1} has been Dismissed ({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    moodText = GameManager.instance.personScript.UpdateMood(MoodType.DismissUnsuited, actor.arc.name);
                                    break;
                                default:
                                    Debug.LogErrorFormat("Invalid data.optionText \"{0}\"", data.optionNested);
                                    break;
                            }
                            //teams
                            if (numOfTeams > 0)
                            {
                                if (builderBottom.Length > 0)
                                { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                builderBottom.AppendFormat("{0}{1} related Team{2} sent to the Reserve Pool{3}", colourBad, numOfTeams,
                                numOfTeams != 1 ? "s" : "", colourEnd);
                            }
                            //message
                            GameManager.instance.messageScript.ActorStatus(msgTextMain, msgReason, string.Format("has been <b>{0}</b>", msgReason), actor.actorID, playerSide);
                            //Process any other effects, if move to the Reserve pool was successful, ignore otherwise
                            ManageAction manageAction = GameManager.instance.dataScript.GetManageAction(data.optionNested);
                            if (manageAction != null)
                            {
                                List<Effect> listOfEffects = manageAction.listOfEffects;
                                if (listOfEffects.Count > 0)
                                {
                                    EffectDataInput dataInput = new EffectDataInput();
                                    dataInput.originText = "Dismiss Actor";
                                    foreach (Effect effect in listOfEffects)
                                    {
                                        if (effect.ignoreEffect == false)
                                        {
                                            EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, null, dataInput, actor);
                                            if (effectReturn != null)
                                            {
                                                if (!string.IsNullOrEmpty(effectReturn.topText) && builderTop.Length > 0) { builderTop.AppendLine(); }
                                                builderTop.Append(effectReturn.topText);
                                                if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                                builderBottom.Append(effectReturn.bottomText);
                                                //exit effect loop on error
                                                if (effectReturn.errorFlag == true) { break; }
                                            }
                                            else { Debug.LogError("Invalid effectReturn (Null)"); }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogErrorFormat("Invalid ManageAction (Null) for data.optionText \"{0}\"", data.optionNested);
                                successFlag = false;
                            }
                        }
                        else
                        {
                            //some issue prevents actor being added to reserve pool (full? -> probably not as a criteria checks this)
                            successFlag = false;
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("Invalid Actor (Null) for actorSlotID {0}", data.optionID);
                        successFlag = false;
                    }
                }
                else
                { Debug.LogWarningFormat("Invalid actorSlotID {0}", data.actorSlotID); }
            }
            else
            { Debug.LogError("Invalid optionText (Null or empty)"); }
        }
        else
        {
            Debug.LogError("Invalid GenericReturnData (Null)");
            successFlag = false;
        }
        //failed outcome
        if (successFlag == false)
        {
            builderTop.Append("Something has gone wrong. You are unable to dismiss or promote anyone at present");
            builderBottom.Append("It's the wiring. It's broken. Rats. Big ones.");
        }
        //mood info
        builderBottom.AppendFormat("{0}{1}{2}", "\n", "\n", moodText);
        //
        // - - - Outcome - - - 
        //
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        outcomeDetails.textTop = builderTop.ToString();
        outcomeDetails.textBottom = builderBottom.ToString();
        outcomeDetails.sprite = sprite;
        outcomeDetails.side = playerSide;
        //action expended automatically for manage actor
        if (successFlag == true)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Dismiss Actor";
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessDismissActorAction");
    }

    /// <summary>
    /// processes final selection for a Dispose Actor Action
    /// </summary>
    private void ProcessDisposeActorAction(GenericReturnData data)
    {
        bool successFlag = true;
        string msgTextStatus = "Unknown";
        string msgTextMain = "Who Knows?";
        string msgReason = "Unknown";
        string moodText = "Unknown";
        int numOfTeams = 0;
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        Sprite sprite = GameManager.instance.guiScript.errorSprite;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        if (data != null)
        {
            if (string.IsNullOrEmpty(data.optionNested) == false)
            {
                if (data.actorSlotID > -1)
                {
                    //find actor
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, playerSide);
                    if (actor != null)
                    {
                        //add actor to the dismissed or promoted lists
                        if (GameManager.instance.dataScript.RemoveCurrentActor(playerSide, actor, ActorStatus.Killed) == true)
                        {
                            //sprite of recruited actor
                            sprite = actor.sprite;
                            //authority actor?
                            if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                            {
                                //remove all active teams connected with this actor
                                numOfTeams = GameManager.instance.teamScript.TeamCleanUp(actor);
                            }
                            builderTop.AppendLine();
                            //actor successfully dismissed or promoted
                            switch (data.optionNested)
                            {
                                case "DisposeLoyalty":
                                    builderTop.AppendFormat("{0}{1} {2} vehemently denies being disployal but nobody is listening{3}", colourNormal, actor.arc.name,
                                        actor.actorName, colourEnd);
                                    msgTextStatus = "Loyalty";
                                    msgReason = "Disloyal";
                                    msgTextMain = string.Format("{0} {1} has been killed ({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    moodText = GameManager.instance.personScript.UpdateMood(MoodType.DisposeLoyalty, actor.arc.name);
                                    break;
                                case "DisposeCorrupt":
                                    builderTop.AppendFormat("{0}{1} {2} protests their innocence but don't they all?{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd);
                                    msgTextStatus = "Corrupt";
                                    msgReason = "Corrupt";
                                    msgTextMain = string.Format("{0} {1} has been killed({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    moodText = GameManager.instance.personScript.UpdateMood(MoodType.DisposeCorrupt, actor.arc.name);
                                    break;
                                case "DisposeHabit":
                                    builderTop.AppendFormat("{0}{1} {2} smiles and says that they will be waiting for you in hell{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd);
                                    msgTextStatus = "Habit";
                                    msgReason = "Bad Habit";
                                    msgTextMain = string.Format("{0} {1} has been killed ({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    moodText = GameManager.instance.personScript.UpdateMood(MoodType.DisposeHabit, actor.arc.name);
                                    break;
                                default:
                                    Debug.LogErrorFormat("Invalid data.optionText \"{0}\"", data.optionNested);
                                    break;
                            }
                            //teams
                            if (numOfTeams > 0)
                            {
                                if (builderBottom.Length > 0)
                                { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                builderBottom.AppendFormat("{0}{1} related Team{2} sent to the Reserve Pool{3}", colourBad, numOfTeams,
                                numOfTeams != 1 ? "s" : "", colourEnd);
                            }
                            //message
                            GameManager.instance.messageScript.ActorStatus(msgTextMain, "Disposed Off", string.Format("has been disposed off ({0})", msgReason), actor.actorID, playerSide);
                            //Process any other effects, if move to the Reserve pool was successful, ignore otherwise
                            ManageAction manageAction = GameManager.instance.dataScript.GetManageAction(data.optionNested);
                            if (manageAction != null)
                            {
                                List<Effect> listOfEffects = manageAction.listOfEffects;
                                if (listOfEffects.Count > 0)
                                {
                                    EffectDataInput dataInput = new EffectDataInput();
                                    dataInput.originText = "Dispose Actor";
                                    foreach (Effect effect in listOfEffects)
                                    {
                                        if (effect.ignoreEffect == false)
                                        {
                                            EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, null, dataInput, actor);
                                            if (effectReturn != null)
                                            {
                                                if (!string.IsNullOrEmpty(effectReturn.topText) && builderTop.Length > 0) { builderTop.AppendLine(); }
                                                builderTop.Append(effectReturn.topText);
                                                if (builderBottom.Length > 0) { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                                builderBottom.Append(effectReturn.bottomText);
                                                //exit effect loop on error
                                                if (effectReturn.errorFlag == true) { break; }
                                            }
                                            else { Debug.LogError("Invalid effectReturn (Null)"); }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogErrorFormat("Invalid ManageAction (Null) for data.optionText \"{0}\"", data.optionNested);
                                successFlag = false;
                            }
                        }
                        else
                        {
                            //some issue prevents actor being added to reserve pool (full? -> probably not as a criteria checks this)
                            successFlag = false;
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("Invalid Actor (Null) for actorSlotID {0}", data.optionID);
                        successFlag = false;
                    }
                }
                else
                { Debug.LogWarningFormat("Invalid actorSlotID {0}", data.actorSlotID); }
            }
            else
            { Debug.LogError("Invalid optionText (Null or empty)"); }
        }
        else
        {
            Debug.LogError("Invalid GenericReturnData (Null)");
            successFlag = false;
        }
        //failed outcome
        if (successFlag == false)
        {
            builderTop.Append("Something has gone wrong. You are unable to dispose off anyone at present");
            builderBottom.Append("It's the wiring. It's broken. Rats. Big ones.");
        }
        //mood info
        builderBottom.AppendFormat("{0}{1}{2}", "\n", "\n", moodText);
        //
        // - - - Outcome - - - 
        //
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        outcomeDetails.textTop = builderTop.ToString();
        outcomeDetails.textBottom = builderBottom.ToString();
        outcomeDetails.sprite = sprite;
        outcomeDetails.side = playerSide;
        //action expended automatically for manage actor
        if (successFlag == true)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Dispose of Actor";
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessDisposeActorAction");
    }

    /// <summary>
    /// Handles Authority "ANY TEAM" action
    /// </summary>
    /// <param name="details"></param>
    public void ProcessTeamAction(ModalActionDetails details)
    {
        GameManager.instance.teamPickerScript.SetTeamPicker(details);
        EventManager.instance.PostNotification(EventType.OpenTeamPicker, this, details, "ActionManager.cs -> ProcessTeamAction");
    }

    /// <summary>
    /// 'Hande' Actor action. Branches to specific Generic Picker window depending on option selected
    /// </summary>
    /// <param name="teamID"></param>
    public void ProcessHandleActor(GenericReturnData data)
    {
        bool errorFlag = false;
        manageDelegate handler = null;
        ModalActionDetails details = new ModalActionDetails();
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if (data != null)
        {
            //pass through data
            details.actorDataID = data.actorSlotID;
            details.side = playerSide;
            if (string.IsNullOrEmpty(data.optionNested) == false)
            {
                switch (data.optionNested)
                {
                    case "HandleReserve":
                        Debug.LogFormat("ProcessHandleActor: \"{0}\" selected{1}", data.optionNested, "\n");
                        handler = InitialiseReserveActorAction;
                        details.eventType = EventType.GenericReserveActor;
                        break;
                    case "HandleDismiss":
                        Debug.LogFormat("ProcessHandleActor: \"{0}\" selected{1}", data.optionNested, "\n");
                        handler = InitialiseDismissActorAction;
                        details.eventType = EventType.GenericDismissActor;
                        break;
                    case "HandleDispose":
                        Debug.LogFormat("ProcessHandleActor: \"{0}\" selected{1}", data.optionNested, "\n");
                        handler = InitialiseDisposeActorAction;
                        details.eventType = EventType.GenericDisposeActor;
                        break;
                    default:
                        Debug.LogErrorFormat("Invalid data.optionText \"{0}\"", data.optionNested);
                        errorFlag = true;
                        break;
                }
            }
            else { Debug.LogError("Invalid data.optionText (Null or empty"); errorFlag = true; }
        }
        else { Debug.LogError("Invalid GenericReturnData (Null)"); errorFlag = true; }
        //final processing -> either branch to the next Generic picker window or kick out an error outcome window
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = playerSide;
            outcomeDetails.textTop = string.Format("{0}You are unable to conduct any Managerial actions at this point for reasons unknown{1}", colourAlert, colourEnd);
            outcomeDetails.textBottom = "Phone calls are being made but don't hold your breath";
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessHandleActor");
        }
        else
        {

            if (handler != null)
            {
                //activate Back button to enable user to flip back a window
                GameManager.instance.genericPickerScript.SetBackButton(EventType.ManageActorAction);
                //branch to the appropriate method for the second level of the Manage Generic Picker via the delegate
                handler(details);
            }
        }
    }


    public string GetPlayerActionMenuHeader()
    { return string.Format("{0}Personal Actions{1}", colourResistance, colourEnd); }


    /// <summary>
    /// Default data set for outcome if a problem
    /// </summary>
    /// <param name="details"></param>
    /// <returns></returns>
    private ModalOutcomeDetails SetDefaultOutcome(ModalActionDetails details)
    {
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        if (details != null)
        {
            //default data 
            outcomeDetails.side = details.side;
            outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
            outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
            outcomeDetails.modalLevel = details.modalLevel;
            outcomeDetails.modalState = details.modalState;
        }
        return outcomeDetails;
    }

    //methods above here
}
