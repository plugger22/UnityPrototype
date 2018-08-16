using System.Collections.Generic;
using UnityEngine;
using modalAPI;
using gameAPI;
using packageAPI;
using System.Text;
using delegateAPI;



/// <summary>
/// Handles all action related matters
/// </summary>
public class ActionManager : MonoBehaviour
{
    
    //fast access -> target
    private int failedTargetChance;
    //gear
    private int gearGracePeriod = -1;
    private int gearSwapBaseAmount = -1;
    private int gearSwapPreferredAmount = -1;
    //traits
    private int actorStressedDuringSecurity = -1;
    private int actorKeepGear = -1;

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
    private string colourEnd;

    public void Initialise()
    {
        //fast access fields
        failedTargetChance = GameManager.instance.aiScript.targetAttemptChance;
        actorStressedDuringSecurity = GameManager.instance.dataScript.GetTraitEffectID("ActorStressSecurity");
        actorKeepGear = GameManager.instance.dataScript.GetTraitEffectID("ActorKeepGear");
        gearGracePeriod = GameManager.instance.gearScript.actorGearGracePeriod;
        gearSwapBaseAmount = GameManager.instance.gearScript.gearSwapBaseAmount;
        gearSwapPreferredAmount = GameManager.instance.gearScript.gearSwapPreferredAmount;
        Debug.Assert(failedTargetChance > 0, string.Format("Invalid failedTargetChance {0}", failedTargetChance));
        Debug.Assert(actorStressedDuringSecurity > -1, "Invalid actorStressedDuringSecurity (-1) ");
        Debug.Assert(actorKeepGear > -1, "Invalid actorKeepGear (-1)");
        Debug.Assert(gearGracePeriod > -1, "Invalid gearGracePeriod (-1)");
        Debug.Assert(gearSwapBaseAmount > -1, "Invalid gearSwapBaseAmount (-1)");
        Debug.Assert(gearSwapPreferredAmount > -1, "Invalid gearSwapPreferredAmount (-1)");
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
            case EventType.ActivateActorAction:
                ModalActionDetails detailsActivateActor = Param as ModalActionDetails;
                ProcessActivateActorAction(detailsActivateActor);
                break;
            case EventType.ActivatePlayerAction:
                ModalActionDetails detailsActivatePlayer = Param as ModalActionDetails;
                ProcessActivatePlayerAction(detailsActivatePlayer);
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
                ProcessThreatenActor(detailsThreaten);
                break;
            case EventType.InventoryLetGo:
                ModalActionDetails detailsLetGo = Param as ModalActionDetails;
                ProcessLetGoActor(detailsLetGo);
                break;
            case EventType.InventoryFire:
                ModalActionDetails detailsFire = Param as ModalActionDetails;
                ProcessFireActor(detailsFire);
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
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourError = GameManager.instance.colourScript.GetColour(ColourType.error);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.cancelHighlight);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
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
        CaptureDetails captureDetails = null;
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
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
                            if (node.nodeID == GameManager.instance.nodeScript.nodePlayer) { actorID = 999; }
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
                            dataInput.originText = action.name;
                            //
                            // - - - Process effects
                            //
                            foreach (Effect effect in listOfEffects)
                            {
                                //ongoing effect?
                                if (effect.duration.name == "Ongoing")
                                {
                                    //NOTE: A standard node action can use ongoing effects but there is no way of linking it. The node effects will time out and that's it
                                    dataInput.ongoingID = GameManager.instance.effectScript.GetOngoingEffectID();
                                }
                                else { dataInput.ongoingID = -1; dataInput.ongoingText = ""; }
                                effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actor);
                                if (effectReturn != null)
                                {
                                    outcomeDetails.sprite = actor.arc.sprite;
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
        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
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
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        //renown (do prior to effects as Player renown will change)
        int renownBefore = GameManager.instance.playerScript.Renown;
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
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
                        dataInput.originText = "Node Gear";
                        //
                        // - - - Process effects
                        //
                        foreach (Effect effect in listOfEffects)
                        {
                            //no ongoing effect allowed for nodeGear actions
                            dataInput.ongoingID = -1;
                            dataInput.ongoingText = "";
                            effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput);
                            if (effectReturn != null)
                            {
                                outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
                                //update stringBuilder texts
                                if (effectReturn.topText.Length > 0) { builderTop.AppendLine(); builderTop.AppendLine(); }
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
        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
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
            Gear gear = GameManager.instance.dataScript.GetGear(details.gearID);
            if (gear != null)
            {
                GameManager.instance.gearScript.SetGearUsed(gear, string.Format("affect {0} district", node.nodeName));
                //generate a create modal outcome dialogue
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessNodeGearAction");
            }
            else
            {
                Debug.LogErrorFormat("Invalid Gear (Null) for gearID {0}", gear.gearID);
                errorFlag = true;
            }
        }
        //ERROR ->  go straight to outcome window
        else
        {
            //fault, pass default data to a modal outcome dialogue
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
            //generate a create modal window event
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessNodeGearAction");
        }
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
                                actorSlotID = actor.actorSlotID,
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
        int unhappyTimer = GameManager.instance.actorScript.currentReserveTimer;
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
                                actorSlotID = actor.actorSlotID,
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
                                tooltipText = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                                if (manageAction.isRenownCost == true)
                                { tooltip.textDetails = string.Format("{0}{1}{2}Player Renown -{3}{4}{5}{6}Unhappy in {7} turns{8}", tooltipText, "\n", colourBad, renownCost, colourEnd, 
                                    "\n", colourAlert, unhappyTimer, colourEnd); }
                                else
                                { tooltip.textDetails = string.Format("{0}{1}{2}No Renown Cost{3}{4}{5}Unhappy in {6} turns{7}", tooltipText, "\n", colourGood, colourEnd, "\n", colourAlert, unhappyTimer, colourEnd); }
                            }
                            else
                            {
                                //option DEACTIVATED
                                option.isOptionActive = false;
                                //tooltip
                                tooltip.textHeader = string.Format("{0}Option Unavailable{1}", colourSide, colourEnd);
                                tooltip.textMain = manageAction.tooltipMain;
                                if (manageAction.isRenownCost == true)
                                { tooltip.textDetails = string.Format("{0}{1}{2}", colourInvalid, criteriaText, colourEnd); }
                                else
                                { tooltip.textDetails = string.Format("{0}{1}{2}{3}{4}No Renown Cost{5}", colourInvalid, criteriaText, colourEnd, "\n", colourGood,
                                    colourEnd); }
                            }
                        }
                        else
                        {
                            //no criteria, option automatically activated
                            option.isOptionActive = true;
                            //tooltip
                            tooltip.textHeader = string.Format("{0}Send to the RESERVES{1}", colourSide, colourEnd);
                            tooltip.textMain = manageAction.tooltipMain;
                            tooltipText = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                            if (manageAction.isRenownCost == true)
                            {
                                tooltip.textDetails = string.Format("{0}{1}{2}Player Renown -{3}{4}{5}{6}Unhappy in {7} turns{8}", tooltipText, "\n", colourBad, renownCost, colourEnd,
                                  "\n", colourAlert, unhappyTimer, colourEnd);
                            }
                            else
                            { tooltip.textDetails = string.Format("{0}{1}{2}No Renown Cost{3}{4}{5}Unhappy in {6} turns{7}", tooltipText, "\n", colourGood, colourEnd, "\n", colourAlert, unhappyTimer, colourEnd); }
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
                                actorSlotID = actor.actorSlotID,
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
                                tooltipText = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                                if (manageAction.isRenownCost == true)
                                {
                                    StringBuilder builder = new StringBuilder();
                                    ManageRenownCost manageRenownCost = GameManager.instance.actorScript.GetManageRenownCost(actor, renownCost);
                                    builder.AppendFormat("{0}{1}{2}Player Renown -{3}{4}", tooltipText, "\n", colourBad, manageRenownCost.renownCost, colourEnd);
                                    if (string.IsNullOrEmpty(manageRenownCost.tooltip) == false)
                                    { builder.Append(manageRenownCost.tooltip); }
                                    tooltip.textDetails = builder.ToString();
                                }
                                else
                                { tooltip.textDetails = string.Format("{0}{1}{2}No Renown Cost{3}", tooltipText, "\n", colourGood, colourEnd); }
                            }
                            else
                            {
                                //option DEACTIVATED
                                option.isOptionActive = false;
                                //tooltip (effectmanager handles extras)
                                tooltip.textHeader = string.Format("{0}Option Unavailable{1}", colourSide, colourEnd);
                                tooltip.textMain = manageAction.tooltipMain;
                                if (manageAction.isRenownCost == true)
                                { tooltip.textDetails = string.Format("{0}{1}{2}", colourInvalid, criteriaText, colourEnd);  }
                                else
                                {
                                    tooltip.textDetails = string.Format("{0}{1}{2}{3}{4}No Renown Cost{5}", colourInvalid, criteriaText, colourEnd, "\n", colourGood,
                                      colourEnd);
                                }
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
                            if (manageAction.isRenownCost == true)
                            { tooltip.textDetails = string.Format("{0}{1}{2}Player Renown -{3}{4}", tooltipText, "\n", colourBad, renownCost, colourEnd); }
                            else
                            { tooltip.textDetails = string.Format("{0}{1}{2}No Renown Cost{3}", tooltipText, "\n", colourGood, colourEnd); }
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
                                actorSlotID = actor.actorSlotID,
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
                                tooltipText = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                                if (manageAction.isRenownCost == true)
                                {
                                    StringBuilder builder = new StringBuilder();
                                    ManageRenownCost manageRenownCost = GameManager.instance.actorScript.GetManageRenownCost(actor, renownCost);
                                    builder.AppendFormat("{0}{1}{2}Player Renown -{3}{4}", tooltipText, "\n", colourBad, manageRenownCost.renownCost, colourEnd);
                                    if (string.IsNullOrEmpty(manageRenownCost.tooltip) == false)
                                    { builder.Append(manageRenownCost.tooltip); }
                                    tooltip.textDetails = builder.ToString();
                                }
                                else
                                { tooltip.textDetails = string.Format("{0}{1}{2}No Renown Cost{3}", tooltipText, "\n", colourGood, colourEnd); }
                            }
                            else
                            {
                                //option DEACTIVATED
                                option.isOptionActive = false;
                                //tooltip
                                tooltip.textHeader = string.Format("{0}Option Unavailable{1}", colourSide, colourEnd);
                                tooltip.textMain = manageAction.tooltipMain;
                                if (manageAction.isRenownCost == true)
                                { tooltip.textDetails = string.Format("{0}{1}{2}", colourInvalid, criteriaText, colourEnd); }
                                else
                                {
                                    tooltip.textDetails = string.Format("{0}{1}{2}{3}{4}No Renown Cost{5}", colourInvalid, criteriaText, colourEnd, "\n", colourGood,
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
                            tooltipText = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd);
                            if (manageAction.isRenownCost == true)
                            { tooltip.textDetails = string.Format("{0}{1}{2}Player Renown -{3}{4}", tooltipText, "\n", colourBad, renownCost, colourEnd); }
                            else
                            { tooltip.textDetails = string.Format("{0}{1}{2}No Renown Cost{3}", tooltipText, "\n", colourGood, colourEnd); }
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
    /// Process Lie Low actor action (Resistance only)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessLieLowActorAction(ModalActionDetails details)
    {
        Debug.Assert(GameManager.instance.turnScript.authoritySecurityState == AuthoritySecurityState.Normal, string.Format("Invalid authoritySecurityState {0}",
            GameManager.instance.turnScript.authoritySecurityState));
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        int numOfTurns = 0;
        string actorName = "Unknown";
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        if (details != null)
        {
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, details.side);
            if (actor != null)
            {
                actorName = string.Format("{0}, {1}, ", actor.actorName, actor.arc.name);
                actor.Status = ActorStatus.Inactive;
                actor.inactiveStatus = ActorInactive.LieLow;
                actor.tooltipStatus = ActorTooltip.LieLow;
                actor.isLieLowFirstturn = true;
                numOfTurns = 3 - actor.datapoint2;
                outcomeDetails.textTop = string.Format(" {0} {1} has been ordered to Lie Low", actor.arc.name, actor.actorName);
                outcomeDetails.sprite = actor.arc.sprite;
                //message
                string text = string.Format("{0} {1}, is lying Low. Status: {2}", actor.arc.name, actor.actorName, actor.Status);
                GameManager.instance.messageScript.ActorStatus(text, "Lying Low", "is Lying Low", actor.actorID, details.side);
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorSlotID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }

        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
        }
        else
        {
            //change alpha of actor to indicate inactive status
            GameManager.instance.actorPanelScript.UpdateActorAlpha(details.actorDataID, GameManager.instance.guiScript.alphaInactive);
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Actor Lie Low";
            outcomeDetails.textBottom = GetLieLowMessage(numOfTurns, actorName);
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessLieLowActorAction");
    }

    /// <summary>
    /// Process Lie Low Player action (Resistance only)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessLieLowPlayerAction(ModalActionDetails details)
    {
        Debug.Assert(GameManager.instance.turnScript.authoritySecurityState == AuthoritySecurityState.Normal, string.Format("Invalid authoritySecurityState {0}",
            GameManager.instance.turnScript.authoritySecurityState));
        string playerName = GameManager.instance.playerScript.PlayerName;
        int invis = GameManager.instance.playerScript.Invisibility;
        int numOfTurns = 3 - invis;
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        if (details != null)
        {
            GameManager.instance.playerScript.status = ActorStatus.Inactive;
            GameManager.instance.playerScript.inactiveStatus = ActorInactive.LieLow;
            GameManager.instance.playerScript.tooltipStatus = ActorTooltip.LieLow;
            GameManager.instance.playerScript.isLieLowFirstturn = true;
            outcomeDetails.textTop = string.Format(" {0} will go to ground and Lie Low", playerName);
            outcomeDetails.sprite = GameManager.instance.playerScript.sprite;
            //message
            string text = string.Format("{0} is lying Low. Status: {1}", playerName, GameManager.instance.playerScript.status);
            GameManager.instance.messageScript.ActorStatus(text, "Lying Low", "is Lying Low", GameManager.instance.playerScript.actorID, details.side);
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }

        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
        }
        else
        {
            //change alpha of player to indicate inactive status
            GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaInactive);
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.reason = "Player Lie Low";
            outcomeDetails.textBottom = GetLieLowMessage(numOfTurns, playerName);
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessLieLowPlayerAction");
    }

    /// <summary>
    /// formatted message string for both player and actor Lie Low action
    /// </summary>
    /// <returns></returns>
    private string GetLieLowMessage(int numOfTurns, string actorName)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0}{1} will be Inactive for {2}{3}{4} FULL{5}{6} turn{7} or until Activated{8}", colourNeutral, actorName,
                colourEnd, colourAlert, numOfTurns, colourEnd, colourNeutral, numOfTurns != 1 ? "s" : "", colourEnd);
        builder.AppendLine(); builder.AppendLine();
        builder.AppendFormat("{0}Invisibility {1}{2}+1{3}{4} each turn Inactive{5}", colourGood, colourEnd, colourAlert, colourEnd, colourGood, colourEnd);
        builder.AppendLine(); builder.AppendLine();
        builder.AppendFormat("{0}Any {1}{2}Stress{3}{4} will be removed once Invisibility is fully recovered{5}", colourGood, colourEnd,
            colourAlert, colourEnd, colourGood, colourEnd);
        builder.AppendLine(); builder.AppendLine();
        builder.AppendFormat("{0}All contacts and abilities will be unavailable while Inactive{1}", colourBad, colourEnd);
        return builder.ToString();
    }


    /// <summary>
    /// Process Activate actor action (Resistance only at present but set up for both sides)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessActivateActorAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
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
                outcomeDetails.sprite = actor.arc.sprite;
                //message
                string text = string.Format("{0} {1} has been Recalled. Status: {2}", actor.arc.name, actor.actorName, actor.Status);
                GameManager.instance.messageScript.ActorStatus(text, "Recalled", "has been Recalled", actor.actorID, details.side);
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorSlotID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }

        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
        }
        else
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
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
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

        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
        }
        else
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
    /// Process Give Gear actor action (Resistance only)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessGiveGearAction(ModalActionDetails details)
    {
        int motivationBoost = 0;
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        Gear gear = null;
        Actor actor = null;
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        outcomeDetails.modalLevel = details.modalLevel;
        outcomeDetails.modalState = details.modalState;
        StringBuilder builder = new StringBuilder();
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, details.side);
            if (actor != null)
            {
                gear = GameManager.instance.dataScript.GetGear(details.gearID);
                if (gear != null)
                {
                    //Give Gear
                    outcomeDetails.textTop = string.Format("{0}, {1}, thanks you for the {2}{3}{4}{5}", actor.arc.name, actor.actorName, "\n", colourNeutral, gear.name, colourEnd);
                    //update actor details
                    string textGear = actor.AddGear(gear.gearID);
                    //gear stats
                    gear.statTimesGiven++;
                    //get actor's preferred gear
                    GearType preferredGear = actor.arc.preferredGear;
                    if (preferredGear != null)
                    {
                        motivationBoost = gearSwapBaseAmount;
                        builder.AppendFormat("{0}{1} no longer available{2}", colourBad, gear.name, colourEnd);
                        //motivation loss more if preferred gear
                        if (preferredGear.name.Equals(gear.type.name) == true)
                        {
                            //Preferred gear (extra motivation)
                            motivationBoost += gearSwapPreferredAmount;
                            builder.AppendFormat("{0}{1}{2}{3} Motivation +{4} {5}{6}(Preferred Gear){7}", "\n", "\n", colourGood, actor.arc.name, motivationBoost, colourEnd,
                                colourNeutral, colourEnd);
                        }
                        else
                        { builder.AppendFormat("{0}{1}{2}{3} Motivation +{4}{5}", "\n", "\n", colourGood, actor.arc.name, gearSwapBaseAmount, colourEnd); }
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
                else { Debug.LogErrorFormat("Invalid gear (Null) for details.gearID {0}", details.gearID); errorFlag = true; }
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorSlotID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }

        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
        }
        else
        {
            //Remove Gear
            if (gear != null)
            { GameManager.instance.playerScript.RemoveGear(gear.gearID); }
            outcomeDetails.sprite = actor.arc.sprite;
            outcomeDetails.textBottom = builder.ToString();
            //give actor motivation boost
            actor.datapoint1 += motivationBoost;
            actor.datapoint1 = Mathf.Min(GameManager.instance.actorScript.maxStatValue, actor.datapoint1);
            //message
            string text = string.Format("{0} ({1}) given to {2}, {3}", gear.name, gear.rarity.name, actor.arc.name, actor.actorName);
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
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        Gear gear = null;
        Actor actor = null;
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        outcomeDetails.modalLevel = details.modalLevel;
        outcomeDetails.modalState = details.modalState;
        StringBuilder builder = new StringBuilder();
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, details.side);
            if (actor != null)
            {
                gear = GameManager.instance.dataScript.GetGear(details.gearID);
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
                        "\n", colourNeutral, gear.name, colourEnd);
                    //get actor's preferred gear
                    GearType preferredGear = actor.arc.preferredGear;
                    if (preferredGear != null)
                    {
                        builder.AppendFormat("{0}{1} is available{2}", colourGood, gear.name, colourEnd);
                        if (preferredGear.name.Equals(gear.type.name) == true)
                        {
                            //Preferred gear (extra motivation)
                            motivationCost += gearSwapPreferredAmount;
                            builder.AppendFormat("{0}{1}{2}{3} Motivation -{4}{5}{6}{7}(Preferred Gear){8}", "\n", "\n", colourBad, actor.arc.name, motivationCost, colourEnd, "\n",
                            colourNeutral, colourEnd);
                        }
                        else
                        { builder.AppendFormat("{0}{1}{2}{3} Motivation -{4}{5}", "\n", "\n", colourBad, actor.arc.name, motivationCost, colourEnd); }
                    }
                    else
                    { Debug.LogErrorFormat("Invalid preferredGear (Null) for actor Arc {0}", actor.arc.name); errorFlag = true; }
                }
                else { Debug.LogErrorFormat("Invalid gear (Null) for details.gearID {0}", details.gearID); errorFlag = true; }
            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorSlotID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }

        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
        }
        else
        {
            //Transfer Gear
            if (errorFlag == false)
            {
                //remove gear from actor
                actor.RemoveGear(true);
                //Give Gear to Player
                GameManager.instance.playerScript.AddGear(gear.gearID);
                //deduct motivation from actor
                if (motivationCost > 0)
                {                    
                    //relationship Conflict  (ActorConflict)
                    if (actor.datapoint1 < motivationCost)
                    {
                        builder.AppendFormat("{0}{1}{2}{3} Motivation too Low!{4}", "\n", "\n", colourAlert, actor.arc.name, colourEnd);
                        builder.AppendFormat("{0}{1}RELATIONSHIP CONFLICT{2}", "\n", colourBad, colourEnd);
                        builder.AppendFormat("{0}{1}{2}", "\n", "\n", GameManager.instance.actorScript.ProcessActorConflict(actor));
                    }
                    //deduct motivation
                    actor.datapoint1 -= motivationCost;
                    actor.datapoint1 = Mathf.Max(0, actor.datapoint1);
                }
                else
                {
                    //no motivation cost
                    builder.AppendFormat("{0}{1} loses {2}{3}No{4}{5} Motivation{6}", colourGood, actor.arc.name, colourEnd, colourNeutral, colourEnd, colourGood, colourEnd);
                }
            } 
            outcomeDetails.sprite = gear.sprite;
            outcomeDetails.textBottom = builder.ToString();
            //message
            string text = string.Format("{0} ({1}) taken back from {2}, {3}", gear.name, gear.rarity.name, actor.arc.name, actor.actorName);
            GameManager.instance.messageScript.GearTakeOrGive(text, actor, gear, motivationCost, false);
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false)
        {
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
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        outcomeDetails.modalLevel = details.modalLevel;
        outcomeDetails.modalState = details.modalState;
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        //default data 
        outcomeDetails.side = details.side;
        Node node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
        if (node != null)
        {
            //resolve action
            if (details != null)
            {
                //Get Gear
                Gear gear = GameManager.instance.dataScript.GetGear(details.gearID);
                if (gear != null)
                {
                    List<Effect> listOfEffects = gear.listOfPersonalEffects;
                    if (listOfEffects != null && listOfEffects.Count > 0)
                    {
                        //return class
                        EffectDataReturn effectReturn = new EffectDataReturn();
                        //pass through data package
                        EffectDataInput dataInput = new EffectDataInput();
                        dataInput.originText = gear.name;
                        //
                        // - - - Process effects
                        //
                        foreach (Effect effect in listOfEffects)
                        {
                            //no ongoing effect allowed for use gear actions
                            dataInput.ongoingID = -1;
                            dataInput.ongoingText = "";
                            effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput);
                            if (effectReturn != null)
                            {
                                outcomeDetails.sprite = GameManager.instance.playerScript.sprite;
                                builderTop.AppendFormat("{0} has been used by the Player", gear.name);
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
                        Debug.LogErrorFormat("There are no USE Effects for this \"{0}\" gear", gear.name);
                        errorFlag = true;
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Invalid gear (Null) for gearID {0}", details.gearID);
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
        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            //generate a create modal window event
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ActionManager.cs -> ProcessUseGearAction");
        }
        else
        {
            //action (if valid) expended -> must be BEFORE outcome window event
            if (isAction == true)
            {
                outcomeDetails.isAction = true;
                outcomeDetails.reason = "Use Gear (Personal Use)";
            }
            //Gear Used            
            Gear gear = GameManager.instance.dataScript.GetGear(details.gearID);
            if (gear != null)
            { GameManager.instance.gearScript.SetGearUsed(gear, "provide Player with a benefit");  }
            else
            {
                Debug.LogErrorFormat("Invalid Gear (Null) for gearID {0}", gear.gearID);
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
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        Actor actor = null;
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}Goodness me, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        outcomeDetails.modalLevel = details.modalLevel;
        outcomeDetails.modalState = details.modalState;
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetActor(details.actorDataID);
            if (actor != null)
            {
                outcomeDetails.textTop = string.Format("{0} {1} has been reassured that they will be the next person called for active duty",
                    actor.arc.name, actor.actorName);
                outcomeDetails.textBottom = string.Format("{0}{1} Unhappy timer +{2}{3}{4}{5}{6}{7} can't be Reassured again{8}", colourGood, actor.actorName,
                    benefit, colourEnd, "\n", "\n", colourNeutral, actor.actorName, colourEnd);
                outcomeDetails.sprite = actor.arc.sprite;
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
        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        else
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
    /// Reserve pool actor is Threatened via the right click action menu
    /// NOTE: calling method checks unhappyTimer > 0 & that sufficient renown onhand
    /// </summary>
    /// <param name="actorID"></param>
    private void ProcessThreatenActor(ModalActionDetails details)
    {
        int benefit = GameManager.instance.actorScript.unhappyThreatenBoost;
        int renownCost = GameManager.instance.actorScript.renownCostThreaten;
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        Actor actor = null;
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}Wow, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        outcomeDetails.modalLevel = details.modalLevel;
        outcomeDetails.modalState = details.modalState;
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetActor(details.actorDataID);
            if (actor != null)
            {
                outcomeDetails.textTop = string.Format("{0} {1} has been pulled into line and told to smarten up their attitude",
                    actor.arc.name, actor.actorName);
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("{0}{1}'s Unhappy timer +{2}{3}", colourGood, actor.actorName, benefit, colourEnd);
                builder.AppendLine(); builder.AppendLine();
                builder.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownCost, colourEnd);
                builder.AppendLine(); builder.AppendLine();
                builder.AppendFormat("{0}{1} can be Threatened again (not if Unhappy){2}", colourNeutral, actor.actorName, colourEnd);
                outcomeDetails.textBottom = builder.ToString();
                outcomeDetails.sprite = actor.arc.sprite;
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
        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a spike in the circuit. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        else
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
        StringBuilder builder = new StringBuilder();
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        Actor actor = null;
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}We whack me with a feather, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        outcomeDetails.modalLevel = details.modalLevel;
        outcomeDetails.modalState = details.modalState;
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
                actor.datapoint1 -= motivationLoss;
                actor.datapoint1 = Mathf.Max(0, actor.datapoint1);
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
               
                /*//remove actor from reserve list
                List<int> reservePoolList = GameManager.instance.dataScript.GetActorList(details.side, ActorList.Reserve);
                if (reservePoolList != null)
                {
                    if (reservePoolList.Remove(actor.actorID) == false)
                    { Debug.LogWarningFormat("Actor \"{0}\", ID {1}, not found in reservePoolList", actor.actorName, actor.actorID); }
                }
                else { Debug.LogErrorFormat("Invalid reservePoolList (Null) for GlobalSide {0}", details.side); }*/

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
                outcomeDetails.textTop = string.Format("{0} {1} reluctantly returns to the recruitment pool and asks that you keep them in mind", actor.arc.name,
                    actor.actorName);
                outcomeDetails.textBottom = builder.ToString();
                outcomeDetails.sprite = actor.arc.sprite;
                //message
                string text = string.Format("{0} {1} has been Let Go (Reserve Pool)", actor.arc.name, actor.actorName);
                GameManager.instance.messageScript.ActorStatus(text, "Let Go", "has been Let Go", actor.actorID, details.side);

            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorDataID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        //outcome
        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        else
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
    private void ProcessFireActor(ModalActionDetails details)
    {
        /*int motivationLoss = GameManager.instance.actorScript.motivationLossFire;*/
        bool errorFlag = false;
        int numOfTeams = 0;
        int renownCost = details.renownCost;
        Debug.Assert(details.renownCost > 0, "Invalid renownCost (zero)");
        StringBuilder builder = new StringBuilder();
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        Actor actor = null;
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}Hit me with your rythmn stick and tell me what happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        outcomeDetails.modalLevel = details.modalLevel;
        outcomeDetails.modalState = details.modalState;
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
                //pay Player renown cost (passed in by data package)
                int playerRenown = GameManager.instance.playerScript.Renown;
                playerRenown -= renownCost;
                playerRenown = Mathf.Max(0, playerRenown);
                GameManager.instance.playerScript.Renown = playerRenown;
                builder.AppendFormat("{0}Player Renown -{1}{2}", colourBad, renownCost, colourEnd);
                if (actor.isThreatening == true)
                {
                    builder.AppendFormat("{0} (Double Cost as {1} was Threatening Player{2}", colourAlert, actor.actorName, colourEnd);
                    builder.AppendLine(); builder.AppendLine();
                    builder.AppendFormat("{0}{1} is no longer a threat{2}", colourGood, actor.actorName, colourEnd);
                }
                builder.AppendLine(); builder.AppendLine();

                /*//lower actors motivation
                actor.datapoint1 -= motivationLoss;
                actor.datapoint1 = Mathf.Max(0, actor.datapoint1);
                builder.AppendFormat("{0}{1} Motivation -{2}{3}", colourBad, actor.actorName, motivationLoss, colourEnd);*/

                //change actors status
                actor.Status = ActorStatus.Dismissed;
                actor.ResetStates();

                /*//place actor back in the appropriate recruit pool [Edit] same as dismissed -> actor gone from level
                List<int> recruitPoolList = GameManager.instance.dataScript.GetActorRecruitPool(actor.level, details.side);
                if (recruitPoolList != null)
                {
                    recruitPoolList.Add(actor.actorID);
                    builder.AppendLine(); builder.AppendLine();
                    builder.AppendFormat("{0}{1} can be recruited later{2}", colourNeutral, actor.actorName, colourEnd);
                }
                else { Debug.LogErrorFormat("Invalid recruitPoolList (Null) for actor.level {0} & GlobalSide {1}", actor.level, details.side); }*/

                //remove actor from reserve list
                /*List<int> reservePoolList = GameManager.instance.dataScript.GetActorList(details.side, ActorList.Reserve);
                if (reservePoolList != null)
                {
                    if (reservePoolList.Remove(actor.actorID) == false)
                    { Debug.LogWarningFormat("Actor \"{0}\", ID {1}, not found in reservePoolList", actor.actorName, actor.actorID); }
                }
                else { Debug.LogErrorFormat("Invalid reservePoolList (Null) for GlobalSide {0}", details.side); }*/

                //remove actor from reserve list
                GameManager.instance.dataScript.RemoveActorFromReservePool(details.side, actor);
                //remove actor from map & handle admin
                if (GameManager.instance.dataScript.RemoveCurrentActor(details.side, actor, actor.Status) == false)
                { Debug.LogErrorFormat("Actor \"{0}\", {1}, actorID {2}, not removed", actor.actorName, actor.arc.name, actor.actorID); }
                //teams
                if (numOfTeams > 0)
                {
                    if (builder.Length > 0)
                    { builder.AppendLine(); builder.AppendLine(); }
                    builder.AppendFormat("{0}{1} related Team{2} sent to the Reserve Pool{3}", colourBad, numOfTeams,
                    numOfTeams != 1 ? "s" : "", colourEnd);
                }
                outcomeDetails.textTop = string.Format("{0} {1} curses and spits at your feet before walking out the door", actor.arc.name,
                    actor.actorName);
                outcomeDetails.textBottom = builder.ToString();
                outcomeDetails.sprite = actor.arc.sprite;
                //message
                string text = string.Format("{0} {1} has been Dismissed (Reserve Pool)", actor.arc.name, actor.actorName);
                GameManager.instance.messageScript.ActorStatus(text, "Dismissed", "has been Fired from the Reserves", actor.actorID, details.side);

            }
            else { Debug.LogErrorFormat("Invalid actor (Null) for details.actorDataID {0}", details.actorDataID); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }
        //outcome
        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        else
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
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        Actor actor = null;
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}Well bugger me, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        outcomeDetails.modalLevel = details.modalLevel;
        outcomeDetails.modalState = details.modalState;
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetActor(details.actorDataID);
            if (actor != null)
            {
                int actorSlotID = GameManager.instance.dataScript.CheckForSpareActorSlot(details.side);
                if (actorSlotID > -1)
                {
                    //raise actors motivation
                    actor.datapoint1 += motivationGain;
                    actor.datapoint1 = Mathf.Min(GameManager.instance.actorScript.maxStatValue, actor.datapoint1);
                    builder.AppendFormat("{0}{1} Motivation +{2}{3}", colourGood, actor.actorName, motivationGain, colourEnd);
                    //was actor threatening
                    if (actor.isThreatening == true)
                    {
                        builder.AppendLine(); builder.AppendLine();
                        builder.AppendFormat("{0}{1} is no longer Threatening{2}", colourGood, actor.actorName, colourEnd);
                    }
                    Condition condition = GameManager.instance.dataScript.GetCondition("UNHAPPY");
                    if (condition != null)
                    {
                        if (GameManager.instance.playerScript.RemoveCondition(condition, string.Format("{0} is no longer Unhappy", actor.actorName)) == true)
                        {
                            builder.AppendLine(); builder.AppendLine();
                            builder.AppendFormat("{0}{1}'s is no longer Unhappy{2}", colourGood, actor.actorName, colourEnd);
                        }
                    }
                    else
                    { Debug.LogError("Unhappy condition not found (Null)"); errorFlag = true; }
                    //place actor on Map (reset states)
                    GameManager.instance.dataScript.AddCurrentActor(details.side, actor, actorSlotID);

                    /*//remove actor from reserve list
                    List<int> reservePoolList = GameManager.instance.dataScript.GetActorList(details.side, ActorList.Reserve);
                    if (reservePoolList != null)
                    {
                        if (reservePoolList.Remove(actor.actorID) == false)
                        { Debug.LogWarningFormat("Actor \"{0}\", ID {1}, not found in reservePoolList", actor.actorName, actor.actorID); }
                    }
                    else { Debug.LogErrorFormat("Invalid reservePoolList (Null) for GlobalSide {0}", details.side); errorFlag = true; }*/

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
                                    GameManager.instance.messageScript.TeamAdd(msgText, team, true);
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
                    outcomeDetails.sprite = actor.arc.sprite;
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
        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a nasty bug in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        else
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
        int targetID;
        int actorID = GameManager.instance.playerScript.actorID; ;
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
            targetID = node.targetID;
            Target target = GameManager.instance.dataScript.GetTarget(targetID);
            if (target != null)
            {
                //
                // - - - Actor/Player captured beforehand (target is safe if captured) -> if so exit - - -
                //

                //Player
                if (nodeID == GameManager.instance.nodeScript.nodePlayer)
                {
                    details = GameManager.instance.captureScript.CheckCaptured(nodeID, actorID);
                    if (GameManager.instance.playerScript.Invisibility == 0)
                    { isZeroInvisibility = true; }
                }
                //Actor
                else
                {
                    //check correct actor arc for target is present in line up
                    int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.ActorArcID, GameManager.instance.globalScript.sideResistance);
                    if (slotID > -1)
                    {
                        //get actor
                        actor = GameManager.instance.dataScript.GetCurrentActor(slotID, GameManager.instance.globalScript.sideResistance);
                        if (actor != null)
                        {
                            details = GameManager.instance.captureScript.CheckCaptured(nodeID, actor.actorID);
                            actorID = actor.actorID;
                            if (actor.datapoint2 == 0)
                            { isZeroInvisibility = true; }
                        }
                        else
                        { Debug.LogErrorFormat("Invalid actor (Null) for slotID {0}", slotID); errorFlag = true; }
                    }
                    else
                    { Debug.LogErrorFormat("Invalid slotID (-1) for target.actorArc.ActorArcID {0}", target.actorArc.ActorArcID); }
                }
                //Player/Actor captured (provided no errors, otherwise bypass)
                if (errorFlag == false)
                {
                    if (details != null)
                    {
                        //Target aborted, deal with Capture
                        details.effects = string.Format("{0} Attempt on Target \"{1}\" misfired{2}", colourNeutral, target.name, colourEnd);
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
                int tally = GameManager.instance.targetScript.GetTargetTally(target.targetID, true);
                int chance = GameManager.instance.targetScript.GetTargetChance(tally);
                Debug.LogFormat("[Tar] TargetManager.cs -> ProcessNodeTarget: Target {0}{1}", target.name, "\n");
                int roll = Random.Range(0, 100);
                if (roll < chance)
                {
                    //Success
                    isSuccessful = true;
                    //target admin
                    target.targetStatus = Status.Completed;
                    GameManager.instance.dataScript.RemoveTargetFromPool(target, Status.Live);
                    //if ongoing effects then target moved to completed pool
                    if (target.listOfOngoingEffects.Count > 0)
                    { GameManager.instance.dataScript.AddTargetToPool(target, Status.Completed); }
                    //no ongoing effects -> target contained and done with. 
                    else
                    {
                        target.targetStatus = Status.Contained;
                        GameManager.instance.dataScript.AddTargetToPool(target, Status.Contained);
                        //Node cleared out ready for next target
                        node.targetID = -1;
                    }
                    text = string.Format("Target \"{0}\" successfully attempted", target.name, "\n");
                    GameManager.instance.messageScript.TargetAttempt(text, node.nodeID, actorID, target);
                    //random roll
                    Debug.LogFormat("[Rnd] TargetManager.cs -> ProcessNodeTarget: Target attempt SUCCESS need < {0}, rolled {1}{2}", chance, roll, "\n");
                    text = string.Format("Target {0} attempt SUCCESS", target.name);
                    GameManager.instance.messageScript.GeneralRandom(text, "Target Attempt", chance, roll);
                }
                else
                {
                    Debug.LogFormat("[Rnd] TargetManager.cs -> ProcessNodeTarget: Target attempt FAILED need < {0}, rolled {1}{2}", chance, roll, "\n");
                    text = string.Format("Target {0} attempt FAILED", target.name);
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
                            text = string.Format("Target {0} attempt KNOWN", target.name);
                            GameManager.instance.messageScript.GeneralRandom(text, "Attempt Known", failedTargetChance, roll, true);
                        }
                        else
                        {
                            Debug.LogFormat("[Rnd] TargetManager.cs -> ProcessNodeTarget: Target attempt UNDETECTED need < {0}, rolled {1}{2}", failedTargetChance, roll, "\n");
                            text = string.Format("Target {0} attempt UNDETECTED", target.name);
                            GameManager.instance.messageScript.GeneralRandom(text, "Attempt Known", failedTargetChance, roll, true);
                        }
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
                    builderTop.AppendFormat("{0}{1}{2}{3}Attempt <b>Successful</b>", colourNeutral, target.name, colourEnd, "\n");
                    //combine all effects into one list for processing
                    listOfEffects.AddRange(target.listOfGoodEffects);
                    listOfEffects.AddRange(target.listOfBadEffects);
                    listOfEffects.AddRange(target.listOfOngoingEffects);
                }
                else
                {
                    //FAILED target attempt
                    listOfEffects.AddRange(target.listOfFailEffects);
                    builderTop.AppendFormat("{0}{1}{2}{3}Attempt Failed!", colourNeutral, target.name, colourEnd, "\n");
                    if (isZeroInvisibility == false)
                    { builderBottom.AppendFormat("{0}There is a {1} % chance of the Authority becoming aware of the attempt{2}", colourAlert, failedTargetChance, colourEnd); }
                    else
                    { builderBottom.AppendFormat("{0}Authorities are aware of the attempt (due to Zero Invisibility){1}", colourBad, colourEnd); }
                    text = string.Format("Target \"{0}\" unsuccessfully attempted", target.name, "\n");
                    GameManager.instance.messageScript.TargetAttempt(text, node.nodeID, actorID, target);
                }
                //Process effects
                EffectDataReturn effectReturn = new EffectDataReturn();
                //pass through data package
                EffectDataInput dataInput = new EffectDataInput();
                dataInput.originText = "Target";
                //handle any Ongoing effects of target completed -> only if target Successful
                if (isSuccessful == true && target.listOfOngoingEffects.Count > 0)
                {
                    dataInput.ongoingID = GameManager.instance.effectScript.GetOngoingEffectID();
                    dataInput.ongoingText = "Target";
                    //add to target so it can link to effects
                    target.ongoingID = dataInput.ongoingID;
                }
                //any effects to process?
                if (listOfEffects.Count > 0)
                {
                    foreach (Effect effect in listOfEffects)
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
                }
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
                else
                {
                    //fault, pass default data to window
                    outcomeDetails.textTop = "There is a fault in the system. Target not responding";
                    outcomeDetails.textBottom = "Target Acquition Failed";
                    outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
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
        int gearID;
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
                    gearID = actor.GetGearID();
                    //add actor to reserve pool
                    if (GameManager.instance.dataScript.RemoveCurrentActor(playerSide, actor, ActorStatus.Reserve) == true)
                    {
                        //sprite of recruited actor
                        sprite = actor.arc.sprite;
                        //authority actor?
                        if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                        {
                            //remove all active teams connected with this actor
                            numOfTeams = GameManager.instance.teamScript.TeamCleanUp(actor);
                        }
                        builderTop.AppendLine();
                        //actor successfully moved to reserve
                        if (string.IsNullOrEmpty(data.optionText) == false)
                        {
                            switch (data.optionText)
                            {
                                case "ReserveRest":
                                    builderTop.AppendFormat("{0}{1} {2} understands the need for Rest{3}", colourNormal, actor.arc.name,
                                        actor.actorName, colourEnd);
                                    msgText = "Resting";
                                    break;
                                case "ReservePromise":
                                    builderTop.AppendFormat("{0}{1} {2} accepts your word that they will be recalled within a reasonable time period{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd);
                                    msgText = "Promised";
                                    break;
                                case "ReserveNoPromise":
                                    builderTop.AppendFormat("{0}{1} {2} is confused and doesn't understand why they are being cast aside{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd);
                                    msgText = "No Promise";
                                    break;
                                default:
                                    Debug.LogErrorFormat("Invalid data.optionText \"{0}\"", data.optionText);
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
                            if (gearID > -1)
                            {
                                Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                                if (gear != null)
                                {
                                    if (builderBottom.Length > 0)
                                    { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                    builderBottom.AppendFormat("{0}{1} gear Lost{2}", colourBad, gear.name, colourEnd);
                                    //message
                                    string gearText = string.Format("{0} gear lost by {1}, {2}", gear.name, actor.actorName, actor.arc.name);
                                    GameManager.instance.messageScript.GearLost(gearText, gear, actor);
                                }
                                else { Debug.LogWarningFormat("Invalid Gear (Null) for gearID {0}", gearID); }
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
                        ManageAction manageAction = GameManager.instance.dataScript.GetManageAction(data.optionText);
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
                                            if (!string.IsNullOrEmpty(effectReturn.topText) && builderTop.Length > 0)  { builderTop.AppendLine(); }
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
                            Debug.LogErrorFormat("Invalid ManageAction (Null) for data.optionText \"{0}\"", data.optionText);
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
        int numOfTeams = 0;
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        Sprite sprite = GameManager.instance.guiScript.errorSprite;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        ActorStatus status = ActorStatus.Dismissed;
        if (data != null)
        {
            if (string.IsNullOrEmpty(data.optionText) == false)
            {
                if (data.actorSlotID > -1)
                {
                    //find actor
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, playerSide);
                    if (actor != null)
                    {
                        if (data.optionText.Equals("DismissPromote") == true)
                        { status = ActorStatus.Promoted; }
                        //add actor to the dismissed or promoted lists
                        if (GameManager.instance.dataScript.RemoveCurrentActor(playerSide, actor, status) == true)
                        {
                            //sprite of recruited actor
                            sprite = actor.arc.sprite;
                            //authority actor?
                            if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                            {
                                //remove all active teams connected with this actor
                                numOfTeams = GameManager.instance.teamScript.TeamCleanUp(actor);
                            }
                            builderTop.AppendLine();
                            //actor successfully dismissed or promoted
                            switch (data.optionText)
                            {
                                case "DismissPromote":
                                    builderTop.AppendFormat("{0}{1} {2} shakes your hand and heads off to bigger things{3}", colourNormal, actor.arc.name,
                                        actor.actorName, colourEnd);
                                    msgTextStatus = "Promoted";
                                    msgReason = "Promoted";
                                    msgTextMain = string.Format("{0} {1} has been Promoted ({2})", actor.arc.name, actor.actorName, 
                                        GameManager.instance.factionScript.GetCurrentFaction().name);
                                    break;
                                case "DismissIncompetent":
                                    builderTop.AppendFormat("{0}{1} {2} scowls and curses before stomping off{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd);
                                    msgTextStatus = "Incompetent";
                                    msgReason = "dismissed for Incompetence";
                                    msgTextMain = string.Format("{0} {1} has been Dismissed ({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    break;
                                case "DismissUnsuited":
                                    builderTop.AppendFormat("{0}{1} {2} lets you know that they won't forget this{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd);
                                    msgTextStatus = "Unsuited";
                                    msgReason = "dismissed as Unsuited for role";
                                    msgTextMain = string.Format("{0} {1} has been Dismissed ({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    break;
                                default:
                                    Debug.LogErrorFormat("Invalid data.optionText \"{0}\"", data.optionText);
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
                            GameManager.instance.messageScript.ActorStatus(msgTextMain, msgReason, string.Format("has been ", msgReason), actor.actorID, playerSide);
                            //Process any other effects, if move to the Reserve pool was successful, ignore otherwise
                            ManageAction manageAction = GameManager.instance.dataScript.GetManageAction(data.optionText);
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
                                Debug.LogErrorFormat("Invalid ManageAction (Null) for data.optionText \"{0}\"", data.optionText);
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
        int numOfTeams = 0;
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        Sprite sprite = GameManager.instance.guiScript.errorSprite;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        if (data != null)
        {
            if (string.IsNullOrEmpty(data.optionText) == false)
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
                            sprite = actor.arc.sprite;
                            //authority actor?
                            if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
                            {
                                //remove all active teams connected with this actor
                                numOfTeams = GameManager.instance.teamScript.TeamCleanUp(actor);
                            }
                            builderTop.AppendLine();
                            //actor successfully dismissed or promoted
                            switch (data.optionText)
                            {
                                case "DisposeLoyalty":
                                    builderTop.AppendFormat("{0}{1} {2} vehemently denies being disployal but nobody is listening{3}", colourNormal, actor.arc.name,
                                        actor.actorName, colourEnd);
                                    msgTextStatus = "Loyalty";
                                    msgReason = "Disloyal";
                                    msgTextMain = string.Format("{0} {1} has been killed ({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    break;
                                case "DisposeCorrupt":
                                    builderTop.AppendFormat("{0}{1} {2} protests their innocence but don't they all?{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd);
                                    msgTextStatus = "Corrupt";
                                    msgReason = "Corrupt";
                                    msgTextMain = string.Format("{0} {1} has been killed({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    break;
                                case "DisposeHabit":
                                    builderTop.AppendFormat("{0}{1} {2} smiles and says that they will be waiting for you in hell{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd);
                                    msgTextStatus = "Habit";
                                    msgReason = "Bad Habit";
                                    msgTextMain = string.Format("{0} {1} has been killed ({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    break;
                                default:
                                    Debug.LogErrorFormat("Invalid data.optionText \"{0}\"", data.optionText);
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
                            GameManager.instance.messageScript.ActorStatus(msgTextMain, "Disposed Off",  string.Format("has been disposed off ({0})", msgReason), actor.actorID, playerSide);
                            //Process any other effects, if move to the Reserve pool was successful, ignore otherwise
                            ManageAction manageAction = GameManager.instance.dataScript.GetManageAction(data.optionText);
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
                                Debug.LogErrorFormat("Invalid ManageAction (Null) for data.optionText \"{0}\"", data.optionText);
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
            if (string.IsNullOrEmpty(data.optionText) == false)
            {
                switch (data.optionText)
                {
                    case "HandleReserve":
                        Debug.LogFormat("ProcessHandleActor: \"{0}\" selected{1}", data.optionText, "\n");
                        handler = InitialiseReserveActorAction;
                        details.eventType = EventType.GenericReserveActor;
                        break;
                    case "HandleDismiss":
                        Debug.LogFormat("ProcessHandleActor: \"{0}\" selected{1}", data.optionText, "\n");
                        handler = InitialiseDismissActorAction;
                        details.eventType = EventType.GenericDismissActor;
                        break;
                    case "HandleDispose":
                        Debug.LogFormat("ProcessHandleActor: \"{0}\" selected{1}", data.optionText, "\n");
                        handler = InitialiseDisposeActorAction;
                        details.eventType = EventType.GenericDisposeActor;
                        break;
                    default:
                        Debug.LogErrorFormat("Invalid data.optionText \"{0}\"", data.optionText);
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

    //methods above here
}
