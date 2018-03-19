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

    public Sprite targetSprite;

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
        //register listener
        EventManager.instance.AddListener(EventType.NodeAction, OnEvent);
        EventManager.instance.AddListener(EventType.NodeGearAction, OnEvent);
        EventManager.instance.AddListener(EventType.TargetAction, OnEvent);
        EventManager.instance.AddListener(EventType.LieLowAction, OnEvent);
        EventManager.instance.AddListener(EventType.ActivateAction, OnEvent);
        EventManager.instance.AddListener(EventType.ManageActorAction, OnEvent);
        EventManager.instance.AddListener(EventType.GiveGearAction, OnEvent);
        EventManager.instance.AddListener(EventType.InsertTeamAction, OnEvent);
        EventManager.instance.AddListener(EventType.GenericHandleActor, OnEvent);
        EventManager.instance.AddListener(EventType.GenericReserveActor, OnEvent);
        EventManager.instance.AddListener(EventType.GenericDismissActor, OnEvent);
        EventManager.instance.AddListener(EventType.GenericDisposeActor, OnEvent);
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
            case EventType.LieLowAction:
                ModalActionDetails detailsLieLow = Param as ModalActionDetails;
                ProcessLieLowActorAction(detailsLieLow);
                break;
            case EventType.ActivateAction:
                ModalActionDetails detailsActivate = Param as ModalActionDetails;
                ProcessActivateActorAction(detailsActivate);
                break;
            case EventType.GiveGearAction:
                ModalActionDetails detailsGiveGear = Param as ModalActionDetails;
                ProcessGiveGearAction(detailsGiveGear);
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
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
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
        AIDetails aiDetails = null;
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
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorSlotID, GameManager.instance.sideScript.PlayerSide);
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
                            aiDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, actorID);
                        }
                        if (aiDetails != null)
                        {
                            //Captured. Mission a wipe.
                            aiDetails.effects = string.Format("{0}Mission at \"{1}\" aborted{2}", colourNeutral, node.nodeName, colourEnd);
                            EventManager.instance.PostNotification(EventType.Capture, this, aiDetails);
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
                                    outcomeDetails.sprite = actor.arc.actionSprite;
                                    //update stringBuilder texts
                                    if (string.IsNullOrEmpty(effectReturn.topText) == false)
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
                            Debug.LogError(string.Format("There are no Effects for this \"{0}\" Action", action.name));
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
        { outcomeDetails.isAction = true; }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
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
        int renownCost = GameManager.instance.actorScript.renownCostGear;
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
                                if (effectReturn.topText.Length > 0)
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
                        Debug.LogError(string.Format("There are no Effects for this \"{0}\" Action", action.name));
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
        { outcomeDetails.isAction = true; }
        //no error -> PROCEED to some form of dice outcome for gear use
        if (errorFlag == false)
        {
            Gear gear = GameManager.instance.dataScript.GetGear(details.gearID);

            if (gear != null)
            {
                //chance of Gear being Compromised
                ModalDiceDetails diceDetails = new ModalDiceDetails();
                diceDetails.chance = GameManager.instance.gearScript.GetChanceOfCompromise(gear.gearID);
                diceDetails.renownCost = renownCost;
                if (action != null)
                {
                    diceDetails.topText = string.Format("{0}{1}{2} used to {3}{4}{5}{6}{7}% Chance{8} of it being compromised and lost", colourNeutral,
                        gear.name, colourEnd, action.tooltipText, "\n", "\n", colourBad, diceDetails.chance, colourEnd);
                }
                else { diceDetails.topText = string.Format("Missing Data{0}{1}{2}% Chance{3} of it being compromised and lost", "\n", colourBad,
                    diceDetails.chance, colourEnd);
                }
                if (renownBefore >= renownCost) { diceDetails.isEnoughRenown = true; }
                else { diceDetails.isEnoughRenown = false; }
                //as gear involved data will be needed to be passed through from this method
                PassThroughDiceData passThroughData = new PassThroughDiceData();
                passThroughData.nodeID = node.nodeID;
                passThroughData.gearID = gear.gearID;
                passThroughData.renownCost = renownCost;
                /*passThroughData.text = builder.ToString();*/
                passThroughData.type = DiceType.Gear;
                passThroughData.outcome = outcomeDetails;
                diceDetails.passData = passThroughData;
                //go straight to an outcome dialogue if not enough renown and option set to ignore dice roller
                if (diceDetails.isEnoughRenown == false && GameManager.instance.optionScript.autoGearResolution == true)
                { EventManager.instance.PostNotification(EventType.DiceBypass, this, diceDetails); }
                //roll dice
                else
                { EventManager.instance.PostNotification(EventType.OpenDiceUI, this, diceDetails); }
            }
            else
            {
                Debug.LogError(string.Format("Invalid Gear (Null) for gearID {0}", gear.gearID));
                errorFlag = true;
            }
        }
        //ERROR ->  go straight to outcome window
        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
            //generate a create modal window event
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
    }


    /// <summary>
    /// Process Manage actor action (first of the nested Manage actor menu's -> provides 'Reserve', 'Dismiss' & 'Dispose' options
    /// </summary>
    /// <param name="details"></param>
    private void ProcessManageActorAction(ModalActionDetails details)
    {
        Debug.Log(string.Format("Memory: total {0}{1}", System.GC.GetTotalMemory(false), "\n"));
        bool errorFlag = false;
        string title;
        string colourSide;
        string criteriaText;
        bool isResistance = true;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //Initialise nested option windows & sisable back button as you are on the top level of the nested options
        GameManager.instance.genericPickerScript.InitialiseNestedOptions(details);
        GameManager.instance.genericPickerScript.SetBackButton(EventType.None);
        //color code for button tooltip header text, eg. "Operator"ss
        if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; isResistance = false; }
        else { colourSide = colourResistance; isResistance = true; }
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorSlotID, playerSide);
        if (actor != null)
        {
            //ActorHandle
            List<ManageAction> listOfManageOptions = GameManager.instance.dataScript.GetListOfActorHandle();
            if (listOfManageOptions != null)
            {
                genericDetails.returnEvent = EventType.GenericHandleActor;
                genericDetails.side = playerSide;
                genericDetails.actorSlotID = details.actorSlotID;
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
                            optionID = details.actorSlotID,
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
                    else { Debug.LogError(string.Format("Invalid manageAction (Null) for listOfManageOptions[{0}]", i)); }
                }
                //add options to picker data package
                genericDetails.arrayOfOptions = arrayOfGenericOptions;
                genericDetails.arrayOfTooltips = arrayOfTooltips;
            }
            else { Debug.LogError("Invalid listOfManageOptions (Null)"); errorFlag = true; }

        }
        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", details.actorSlotID)); errorFlag = true; }

        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = playerSide;
            outcomeDetails.textTop = string.Format("{0}You are unable to send anyone to the Reserve Pool at this time{1}", colourAlert, colourEnd);
            outcomeDetails.textBottom = "Why is that so? Nobody knows.";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
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
        bool isResistance = true;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"
        if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; isResistance = false; }
        else { colourSide = colourResistance; isResistance = true; }
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorSlotID, playerSide);
        if (actor != null)
        {
            //ActorHandle
            List<ManageAction> listOfManageOptions = GameManager.instance.dataScript.GetListOfActorReserve();
            if (listOfManageOptions != null)
            {
                genericDetails.returnEvent = EventType.GenericReserveActor;
                genericDetails.side = playerSide;
                genericDetails.actorSlotID = details.actorSlotID;
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
                            optionID = details.actorSlotID,
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
                                { tooltip.textDetails = string.Format("{0}{1}{2}Player Renown -{3}{4}", tooltipText, "\n", colourBad, renownCost, colourEnd); }
                                else
                                { tooltip.textDetails = string.Format("{0}{1}{2}No Renown Cost{3}", tooltipText, "\n", colourGood, colourEnd);
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
                            { tooltip.textDetails = string.Format("{0}{1}{2}Player Renown -{3}{4}", tooltipText, "\n", colourBad, renownCost, colourEnd); }
                            else
                            { tooltip.textDetails = string.Format("{0}{1}{2}No Renown Cost{3}", tooltipText, "\n", colourGood, colourEnd); }
                        }
                        //add to arrays
                        arrayOfGenericOptions[i] = option;
                        arrayOfTooltips[i] = tooltip;
                    }
                    else { Debug.LogError(string.Format("Invalid manageAction (Null) for listOfManageOptions[{0}]", i)); }

                }
                //add options to picker data package
                genericDetails.arrayOfOptions = arrayOfGenericOptions;
                genericDetails.arrayOfTooltips = arrayOfTooltips;
            }
            else { Debug.LogError("Invalid listOfManageOptions (Null)"); errorFlag = true; }

        }
        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", details.actorSlotID)); errorFlag = true; }

        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = playerSide;
            outcomeDetails.textTop = string.Format("{0}You are unable to send anyone to the Reserve pool at this time{1}", colourAlert, colourEnd);
            outcomeDetails.textBottom = "Why is it so? Nobody knows.";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
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
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorSlotID, playerSide);
        if (actor != null)
        {
            //ActorHandle
            List<ManageAction> listOfManageOptions = GameManager.instance.dataScript.GetListOfActorDismiss();
            if (listOfManageOptions != null)
            {
                genericDetails.returnEvent = EventType.GenericDismissActor;
                genericDetails.side = playerSide;
                genericDetails.actorSlotID = details.actorSlotID;
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
                            optionID = details.actorSlotID,
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
                                { tooltip.textDetails = string.Format("{0}{1}{2}Player Renown -{3}{4}", tooltipText, "\n", colourBad, renownCost, colourEnd); }
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
                    else { Debug.LogError(string.Format("Invalid manageAction (Null) for listOfManageOptions[{0}]", i)); }

                }
                //add options to picker data package
                genericDetails.arrayOfOptions = arrayOfGenericOptions;
                genericDetails.arrayOfTooltips = arrayOfTooltips;
            }
            else { Debug.LogError("Invalid listOfManageOptions (Null)"); errorFlag = true; }

        }
        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", details.actorSlotID)); errorFlag = true; }

        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = playerSide;
            outcomeDetails.textTop = string.Format("{0}You are unable to Dismiss of anyone at this time{1}", colourAlert, colourEnd);
            outcomeDetails.textBottom = "Why this is so is under investigation";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
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
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorSlotID, playerSide);
        if (actor != null)
        {
            //ActorHandle
            List<ManageAction> listOfManageOptions = GameManager.instance.dataScript.GetListOfActorDispose();
            if (listOfManageOptions != null)
            {
                genericDetails.returnEvent = EventType.GenericDisposeActor;
                genericDetails.side = playerSide;
                genericDetails.actorSlotID = details.actorSlotID;
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
                            optionID = details.actorSlotID,
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
                                { tooltip.textDetails = string.Format("{0}{1}{2}Player Renown -{3}{4}", tooltipText, "\n", colourBad, renownCost, colourEnd); }
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
                    else { Debug.LogError(string.Format("Invalid manageAction (Null) for listOfManageOptions[{0}]", i)); }

                }
                //add options to picker data package
                genericDetails.arrayOfOptions = arrayOfGenericOptions;
                genericDetails.arrayOfTooltips = arrayOfTooltips;
            }
            else { Debug.LogError("Invalid listOfManageOptions (Null)"); errorFlag = true; }

        }
        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", details.actorSlotID)); errorFlag = true; }

        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = playerSide;
            outcomeDetails.textTop = string.Format("{0}You are unable to Dispose of anyone at this time{1}", colourAlert, colourEnd);
            outcomeDetails.textBottom = "Why this is so is under investigation";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
        }
    }

    /// <summary>
    /// Process Lie Low actor action (Resistance only)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessLieLowActorAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        StringBuilder builder = new StringBuilder();
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        if (details != null)
        {
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorSlotID, details.side);
            if (actor != null)
            {
                actor.Status = ActorStatus.Inactive;
                int numOfTurns = 3 - actor.datapoint2;
                outcomeDetails.textTop = string.Format(" {0} {1} has been ordered to Lie Low", actor.arc.name, actor.actorName);
                builder.Append(string.Format("{0}{1} will be Inactive for {2} turn{3} or until Activated{4}", colourNeutral, actor.actorName,
                    numOfTurns, numOfTurns != 1 ? "s" : "", colourEnd));
                builder.AppendLine(); builder.AppendLine();
                builder.Append(string.Format("{0}Invisibility +1 each turn Inactive{1}", colourGood, colourEnd));
                builder.AppendLine(); builder.AppendLine();
                builder.Append(string.Format("{0}Any Stress will be removed once Invisibility recovered{1}", colourGood, colourEnd));
                builder.AppendLine(); builder.AppendLine();
                builder.Append(string.Format("{0}All contacts and abilities will be unavailable while Inactive{1}", colourBad, colourEnd));
                //message
                string text = string.Format("{0} {1}, is lying Low. Status: {2}", actor.arc.name, actor.actorName, actor.Status);
                Message message = GameManager.instance.messageScript.ActorStatus(text, actor.actorID, details.side);
                GameManager.instance.dataScript.AddMessage(message);
            }
            else { Debug.LogError(string.Format("Invalid actor (Null) for details.actorSlotID {0}", details.actorSlotID)); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }

        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        }
        else
        {
            //change alpha of actor to indicate inactive status
            GameManager.instance.guiScript.UpdateActorAlpha(details.actorSlotID, GameManager.instance.guiScript.alphaInactive);
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false)
        {
            outcomeDetails.isAction = true;
            outcomeDetails.textBottom = builder.ToString();
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
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
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorSlotID, details.side);
            if (actor != null)
            {
                string title = "";
                if (details.side == GameManager.instance.globalScript.sideAuthority)
                { title = string.Format(" {0} ", GameManager.instance.metaScript.GetAuthorityTitle()); }

                //Reactivate actor
                actor.Status = ActorStatus.Active;
                outcomeDetails.textTop = string.Format(" {0} {1} has been Recalled", actor.arc.name, actor.actorName);
                outcomeDetails.textBottom = string.Format("{0}{1}{2} is now fully Activated{3}", colourNeutral, actor.actorName, title, colourEnd);
                //message
                string text = string.Format("{0} {1} has been Recalled. Status: {2}", actor.arc.name, actor.actorName, actor.Status);
                Message message = GameManager.instance.messageScript.ActorStatus(text, actor.actorID, details.side);
                GameManager.instance.dataScript.AddMessage(message);
            }
            else { Debug.LogError(string.Format("Invalid actor (Null) for details.actorSlotID {0}", details.actorSlotID)); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }

        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        }
        else
        {
            //change alpha of actor to indicate inactive status
            GameManager.instance.guiScript.UpdateActorAlpha(details.actorSlotID, GameManager.instance.guiScript.alphaActive);
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false)
        { outcomeDetails.isAction = true; }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }


    /// <summary>
    /// Process Give Gear actor action (Resistance only)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessGiveGearAction(ModalActionDetails details)
    {
        int benefit = 0;
        int renownGiven = 0;
        bool errorFlag = false;
        bool preferredFlag = false;
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        Gear gear = null;
        Actor actor = null;
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        if (details != null)
        {
            actor = GameManager.instance.dataScript.GetCurrentActor(details.actorSlotID, details.side);
            if (actor != null)
            {
                gear = GameManager.instance.dataScript.GetGear(details.gearID);
                if (gear != null)
                {
                    //Give Gear
                    outcomeDetails.textTop = string.Format("{0} {1} thanks you for the {2}{3}{4}", actor.arc.name, actor.actorName, colourNeutral, gear.name, colourEnd);
                    outcomeDetails.modalLevel = details.modalLevel;
                    //get actor's preferred gear
                    GearType preferredGear = actor.arc.preferredGear;
                    if (preferredGear != null)
                    {
                        switch (gear.rarity.name)
                        {
                            case "Common": benefit = GameManager.instance.gearScript.gearBenefitCommon; break;
                            case "Rare": benefit = GameManager.instance.gearScript.gearBenefitRare; break;
                            case "Unique": benefit = GameManager.instance.gearScript.gearBenefitUnique; break;
                            default:
                                benefit = 0;
                                Debug.LogError(string.Format("Invalid gear rarity \"{0}\"", gear.rarity.name));
                                break;
                        }
                        if (preferredGear.name.Equals(gear.type.name) == true)
                        {
                            //Preferred gear (renown transfer)
                            outcomeDetails.textBottom = string.Format("{0}{1} no longer available{2}{3}{4}{5}{6} Motivation +{7}{8}{9}Player Renown +{10}{11}{12}{13} Renown -{14}{15}",
                              colourBad, gear.name, colourEnd, "\n", "\n", colourGood, actor.actorName, benefit, "\n", "\n", benefit, "\n", "\n", actor.actorName, benefit, colourEnd);
                            preferredFlag = true;
                        }
                        else
                        {
                            //Not preferred gear (motivation boost only)
                            outcomeDetails.textBottom = string.Format("{0}{1} no longer available{2}{3}{4}{5}{6} Motivation +{7}{8}",
                              colourBad, gear.name, colourEnd, "\n", "\n", colourGood, actor.actorName, benefit, colourEnd);
                        }
                    }
                    else
                    { Debug.LogError(string.Format("Invalid preferredGear (Null) for actor Arc {0}", actor.arc.name)); errorFlag = true; }

                }
                else { Debug.LogError(string.Format("Invalid gear (Null) for details.gearID {0}", details.gearID)); errorFlag = true; }
            }
            else { Debug.LogError(string.Format("Invalid actor (Null) for details.actorSlotID {0}", details.actorSlotID)); errorFlag = true; }
        }
        else { Debug.LogError("Invalid ModalActionDetails (Null)"); errorFlag = true; }

        if (errorFlag == true)
        {
            //fault, pass default data to Outcome window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
        }
        else
        {
            //Remove Gear
            if (gear != null)
            { GameManager.instance.playerScript.RemoveGear(gear.gearID); }
            //give actor motivation boost
            actor.datapoint1 += benefit;
            actor.datapoint1 = Mathf.Min(GameManager.instance.actorScript.maxStatValue, actor.datapoint1);
            //if preferred transfer renown
            if (preferredFlag == true)
            {
                if (actor.renown > 0)
                {
                    //take from actor
                    renownGiven = Mathf.Min(benefit, actor.renown);
                    actor.renown -= benefit;
                    actor.renown = Mathf.Max(0, actor.renown);
                    //give to Player
                    GameManager.instance.playerScript.Renown += renownGiven;
                }
            }
            //message
            string text = string.Format("{0} ({1}) given to {2} {3}", gear.name, gear.rarity.name, actor.arc.name, actor.actorName);
            Message message = GameManager.instance.messageScript.GiveGear(text, actor.actorID, gear.gearID, renownGiven);
            GameManager.instance.dataScript.AddMessage(message);
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false)
        { outcomeDetails.isAction = true; }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }


    /// <summary>
    /// Process node Target
    /// </summary>
    /// <param name="details"></param>
    public void ProcessNodeTarget(int nodeID)
    {
        bool errorFlag = false;
        bool isAction = false;
        bool isSuccessful = false;
        int targetID;
        int actorID = 999;
        string text;
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        AIDetails details = new AIDetails();
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
                { details = GameManager.instance.captureScript.CheckCaptured(nodeID, actorID); }
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
                        }
                        else
                        { Debug.LogError(string.Format("Invalid actor (Null) for slotID {0}", slotID)); errorFlag = true; }
                    }
                    else
                    { Debug.LogError(string.Format("Invalid slotID (-1) for target.actorArc.ActorArcID {0}", target.actorArc.ActorArcID)); }
                }
                //Player/Actor captured (provided no errors, otherwise bypass)
                if (errorFlag == false)
                {
                    if (details != null)
                    {
                        //Target aborted, deal with Capture
                        details.effects = string.Format("{0} Attempt on Target \"{1}\" misfired{2}", colourNeutral, target.name, colourEnd);
                        EventManager.instance.PostNotification(EventType.Capture, this, details);
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
                // - - - Process target - - -  
                //
                int tally = GameManager.instance.targetScript.GetTargetTally(target.targetID);
                int chance = GameManager.instance.targetScript.GetTargetChance(tally);
                Debug.Log(string.Format(" Target: {0} - - - {1}", target.name, "\n"));
                int roll = Random.Range(0, 100);
                if (roll <= chance)
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
                    Message message = GameManager.instance.messageScript.TargetAttempt(text, node.nodeID, actorID, target.targetID);
                    GameManager.instance.dataScript.AddMessage(message);
                }
                Debug.Log(string.Format("Target Resolution: chance {0}  roll {1}  isSuccess {2}{3}", chance, roll, isSuccessful, "\n"));

                //
                // - - - Effects - - - (only apply if target attempted successfully)
                //
                if (isSuccessful == true)
                {
                    List<Effect> listOfEffects = new List<Effect>();
                    //return class
                    EffectDataReturn effectReturn = new EffectDataReturn();

                    //target success
                    builderTop.Append(string.Format("Target {0} successfully attempted", target.name));

                    //combine all effects into one list for processing
                    listOfEffects.AddRange(target.listOfGoodEffects);
                    listOfEffects.AddRange(target.listOfBadEffects);
                    listOfEffects.AddRange(target.listOfOngoingEffects);
                    //pass through data package
                    EffectDataInput dataInput = new EffectDataInput();
                    //handle any Ongoing effects of target completed
                    if (target.listOfOngoingEffects.Count > 0)
                    {
                        dataInput.ongoingID = GameManager.instance.effectScript.GetOngoingEffectID();
                        dataInput.ongoingText = "Target";
                        //add to target so it can link to effects
                        target.ongoingID = dataInput.ongoingID;
                        /*// add to register
                        string registerDetails = string.Format("Target \"{0}\", ID {1}, at {2}, ID {3}, t{4}",  target.name, target.targetID, node.Arc.name, 
                            node.nodeID, GameManager.instance.turnScript.Turn);
                        GameManager.instance.dataScript.AddOngoingEffectToDict(target.ongoingID, registerDetails);*/
                    }
                    //any effects to process?
                    if (listOfEffects.Count > 0)
                    {
                        foreach (Effect effect in listOfEffects)
                        {
                            effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actor);
                            if (effectReturn != null)
                            {
                                outcomeDetails.sprite = targetSprite;
                                //update stringBuilder texts (Bottom only)
                                if (builderBottom.Length > 0)
                                {
                                    builderBottom.AppendLine();
                                    builderBottom.AppendLine();
                                }
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
                    }
                }
                else
                {
                    //target attempt UNSUCCESSFUL
                    builderTop.Append(string.Format("Failed attempt at Target {0}", target.name));
                    text = string.Format("Target \"{0}\" unsuccessfully attempted", target.name, "\n");
                    Message message = GameManager.instance.messageScript.TargetAttempt(text, node.nodeID, actorID, target.targetID);
                    GameManager.instance.dataScript.AddMessage(message);
                }
                //
                // - - - Outcome - - -
                //                        
                //action (if valid) expended -> must be BEFORE outcome window event
                outcomeDetails.isAction = isAction;
                if (errorFlag == false)
                {
                    //outcome
                    outcomeDetails.side = GameManager.instance.globalScript.sideResistance;
                    outcomeDetails.textTop = builderTop.ToString();
                    outcomeDetails.textBottom = builderBottom.ToString();
                    outcomeDetails.sprite = targetSprite;
                }
                else
                {
                    //fault, pass default data to window
                    outcomeDetails.textTop = "There is a fault in the system. Target not responding";
                    outcomeDetails.textBottom = "Target Acquition Failed";
                    outcomeDetails.sprite = GameManager.instance.guiScript.errorSprite;
                }
                //generate a create modal window event
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
            }
            else { Debug.LogError(string.Format("Invalid Target (Null) for node.targetID {0}", nodeID)); }
        }
        else { Debug.LogError(string.Format("Invalid node (Null) for nodeID {0}", nodeID)); }

    }

    /// <summary>
    /// processes final selection for a Reserve Actor Action (both sides)
    /// </summary>
    private void ProcessReserveActorAction(GenericReturnData data)
    {
        bool successFlag = true;
        string msgText = "Unknown";
        int numOfTeams = 0;
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
                    //add actor to reserve pool
                    if (GameManager.instance.dataScript.RemoveCurrentActor(playerSide, actor, ActorStatus.Reserve) == true)
                    {
                        //sprite of recruited actor
                        sprite = actor.arc.baseSprite;
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
                                    builderTop.Append(string.Format("{0}{1} {2} understands the need for Rest{3}", colourNormal, actor.arc.name,
                                        actor.actorName, colourEnd));
                                    msgText = "Resting";
                                    break;
                                case "ReservePromise":
                                    builderTop.Append(string.Format("{0}{1} {2} accepts your word that they will be recalled within a reasonable time period{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd));
                                    msgText = "Promised";
                                    break;
                                case "ReserveNoPromise":
                                    builderTop.Append(string.Format("{0}{1} {2} is confused and doesn't understand why they are being cast aside{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd));
                                    msgText = "No Promise";
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid data.optionText \"{0}\"", data.optionText));
                                    break;
                            }
                            //teams
                            if (numOfTeams > 0)
                            {
                                if (builderBottom.Length > 0)
                                { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                builderBottom.Append(string.Format("{0}{1} related Team{2} sent to the Reserve Pool{3}", colourBad, numOfTeams,
                                numOfTeams != 1 ? "s" : "", colourEnd));
                            }
                        }
                        else
                        {
                            //default data for missing outcome
                            Debug.LogWarning(string.Format("Invalid optionText (Null or empty) for {0} {1}", actor.actorName, actor.arc.name));
                            builderTop.Append(string.Format("{0} {1} is sent to the Reserves", actor.arc.name, actor.actorName));
                        }
                        //message
                        string text = string.Format("{0} {1} moved to the Reserves ({2})", actor.arc.name, actor.actorName, msgText);
                        Message message = GameManager.instance.messageScript.ActorStatus(text, actor.actorID, playerSide);
                        GameManager.instance.dataScript.AddMessage(message);
                        //Process any other effects, if move to the Reserve pool was successful, ignore otherwise
                        ManageAction manageAction = GameManager.instance.dataScript.GetManageAction(data.optionText);
                        if (manageAction != null)
                        {
                            List<Effect> listOfEffects = manageAction.listOfEffects;
                            if (listOfEffects.Count > 0)
                            {
                                EffectDataInput dataInput = new EffectDataInput();

                                foreach (Effect effect in listOfEffects)
                                {
                                    if (effect.ignoreEffect == false)
                                    {
                                        EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, null, dataInput, actor);
                                        if (effectReturn != null)
                                        {
                                            if (!string.IsNullOrEmpty(effectReturn.topText) && builderTop.Length > 0)
                                            { builderTop.AppendLine(); }
                                            builderTop.Append(effectReturn.topText);
                                            if (builderBottom.Length > 0)
                                            { builderBottom.AppendLine(); builderBottom.AppendLine(); }
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
                            Debug.LogError(string.Format("Invalid ManageAction (Null) for data.optionText \"{0}\"", data.optionText));
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
                    Debug.LogWarning(string.Format("Invalid Actor (Null) for actorSlotID {0}", data.optionID));
                    successFlag = false;
                }

            }
            else
            { Debug.LogWarning(string.Format("Invalid actorSlotID {0}", data.actorSlotID)); }
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
        { outcomeDetails.isAction = true; }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }

    /// <summary>
    /// processes final selection for a Dismiss Actor Action
    /// </summary>
    private void ProcessDismissActorAction(GenericReturnData data)
    {
        bool successFlag = true;
        string msgTextStatus = "Unknown";
        string msgTextMain = "Who Knows?";
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
                            sprite = actor.arc.baseSprite;
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
                                    builderTop.Append(string.Format("{0}{1} {2} shakes your hand and heads off to bigger things{3}", colourNormal, actor.arc.name,
                                        actor.actorName, colourEnd));
                                    msgTextStatus = "Promoted";
                                    msgTextMain = string.Format("{0} {1} has been Promoted ({2})", actor.arc.name, actor.actorName, 
                                        GameManager.instance.factionScript.GetCurrentFaction().name);
                                    break;
                                case "DismissIncompetent":
                                    builderTop.Append(string.Format("{0}{1} {2} scowls and curses before stomping off{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd));
                                    msgTextStatus = "Incompetent";
                                    msgTextMain = string.Format("{0} {1} has been Dismissed ({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    break;
                                case "DismissUnsuited":
                                    builderTop.Append(string.Format("{0}{1} {2} lets you know that they won't forget this{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd));
                                    msgTextStatus = "Unsuited";
                                    msgTextMain = string.Format("{0} {1} has been Dismissed ({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid data.optionText \"{0}\"", data.optionText));
                                    break;
                            }
                            //teams
                            if (numOfTeams > 0)
                            {
                                if (builderBottom.Length > 0)
                                { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                builderBottom.Append(string.Format("{0}{1} related Team{2} sent to the Reserve Pool{3}", colourBad, numOfTeams,
                                numOfTeams != 1 ? "s" : "", colourEnd));
                            }
                            //message
                            Message message = GameManager.instance.messageScript.ActorStatus(msgTextMain, actor.actorID, playerSide);
                            GameManager.instance.dataScript.AddMessage(message);
                            //Process any other effects, if move to the Reserve pool was successful, ignore otherwise
                            ManageAction manageAction = GameManager.instance.dataScript.GetManageAction(data.optionText);
                            if (manageAction != null)
                            {
                                List<Effect> listOfEffects = manageAction.listOfEffects;
                                if (listOfEffects.Count > 0)
                                {
                                    EffectDataInput dataInput = new EffectDataInput();

                                    foreach (Effect effect in listOfEffects)
                                    {
                                        if (effect.ignoreEffect == false)
                                        {
                                            EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, null, dataInput, actor);
                                            if (effectReturn != null)
                                            {
                                                if (!string.IsNullOrEmpty(effectReturn.topText) && builderTop.Length > 0)
                                                { builderTop.AppendLine(); }
                                                builderTop.Append(effectReturn.topText);
                                                if (builderBottom.Length > 0)
                                                { builderBottom.AppendLine(); builderBottom.AppendLine(); }
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
                                Debug.LogError(string.Format("Invalid ManageAction (Null) for data.optionText \"{0}\"", data.optionText));
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
                        Debug.LogWarning(string.Format("Invalid Actor (Null) for actorSlotID {0}", data.optionID));
                        successFlag = false;
                    }
                }
                else
                { Debug.LogWarning(string.Format("Invalid actorSlotID {0}", data.actorSlotID)); }
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
        { outcomeDetails.isAction = true; }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }

    /// <summary>
    /// processes final selection for a Dispose Actor Action
    /// </summary>
    private void ProcessDisposeActorAction(GenericReturnData data)
    {
        bool successFlag = true;
        string msgTextStatus = "Unknown";
        string msgTextMain = "Who Knows?";
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
                            sprite = actor.arc.baseSprite;
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
                                    builderTop.Append(string.Format("{0}{1} {2} vehemently denies being disployal but nobody is listening{3}", colourNormal, actor.arc.name,
                                        actor.actorName, colourEnd));
                                    msgTextStatus = "Loyalty";
                                    msgTextMain = string.Format("{0} {1} has been killed ({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    break;
                                case "DisposeCorrupt":
                                    builderTop.Append(string.Format("{0}{1} {2} protests their innocence but don't they all?{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd));
                                    msgTextStatus = "Corrupt";
                                    msgTextMain = string.Format("{0} {1} has been killed({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    break;
                                case "DisposeHabit":
                                    builderTop.Append(string.Format("{0}{1} {2} smiles and says that they will be waiting for you in hell{3}",
                                        colourNormal, actor.arc.name, actor.actorName, colourEnd));
                                    msgTextStatus = "Habit";
                                    msgTextMain = string.Format("{0} {1} has been killed ({2})", actor.arc.name, actor.actorName, msgTextStatus);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid data.optionText \"{0}\"", data.optionText));
                                    break;
                            }
                            //teams
                            if (numOfTeams > 0)
                            {
                                if (builderBottom.Length > 0)
                                { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                builderBottom.Append(string.Format("{0}{1} related Team{2} sent to the Reserve Pool{3}", colourBad, numOfTeams,
                                numOfTeams != 1 ? "s" : "", colourEnd));
                            }
                            //message
                            Message message = GameManager.instance.messageScript.ActorStatus(msgTextMain, actor.actorID, playerSide);
                            GameManager.instance.dataScript.AddMessage(message);
                            //Process any other effects, if move to the Reserve pool was successful, ignore otherwise
                            ManageAction manageAction = GameManager.instance.dataScript.GetManageAction(data.optionText);
                            if (manageAction != null)
                            {
                                List<Effect> listOfEffects = manageAction.listOfEffects;
                                if (listOfEffects.Count > 0)
                                {
                                    EffectDataInput dataInput = new EffectDataInput();

                                    foreach (Effect effect in listOfEffects)
                                    {
                                        if (effect.ignoreEffect == false)
                                        {
                                            EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, null, dataInput, actor);
                                            if (effectReturn != null)
                                            {
                                                if (!string.IsNullOrEmpty(effectReturn.topText) && builderTop.Length > 0)
                                                { builderTop.AppendLine(); }
                                                builderTop.Append(effectReturn.topText);
                                                if (builderBottom.Length > 0)
                                                { builderBottom.AppendLine(); builderBottom.AppendLine(); }
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
                                Debug.LogError(string.Format("Invalid ManageAction (Null) for data.optionText \"{0}\"", data.optionText));
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
                        Debug.LogWarning(string.Format("Invalid Actor (Null) for actorSlotID {0}", data.optionID));
                        successFlag = false;
                    }
                }
                else
                { Debug.LogWarning(string.Format("Invalid actorSlotID {0}", data.actorSlotID)); }
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
        { outcomeDetails.isAction = true; }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }

    /// <summary>
    /// Handles Authority "ANY TEAM" action
    /// </summary>
    /// <param name="details"></param>
    public void ProcessTeamAction(ModalActionDetails details)
    {
        GameManager.instance.teamPickerScript.SetTeamPicker(details);
        EventManager.instance.PostNotification(EventType.OpenTeamPicker, this, details);
    }

    /// <summary>
    /// 'Hande' Actor action. Branches to specific Generic Picker window depending on option selected
    /// </summary>
    /// <param name="teamID"></param>
    public void ProcessHandleActor(GenericReturnData data)
    {
        bool errorFlag = false;
        string colourSide;
        bool isResistance = true;
        manageDelegate handler = null;
        ModalActionDetails details = new ModalActionDetails();
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; isResistance = false; }
        else { colourSide = colourResistance; isResistance = true; }
        if (data != null)
        {
            //pass through data
            details.actorSlotID = data.actorSlotID;
            details.side = playerSide;
            if (string.IsNullOrEmpty(data.optionText) == false)
            {
                switch (data.optionText)
                {
                    case "HandleReserve":
                        Debug.Log(string.Format("ProcessHandleActor: \"{0}\" selected{1}", data.optionText, "\n"));
                        handler = InitialiseReserveActorAction;
                        details.eventType = EventType.GenericReserveActor;
                        break;
                    case "HandleDismiss":
                        Debug.Log(string.Format("ProcessHandleActor: \"{0}\" selected{1}", data.optionText, "\n"));
                        handler = InitialiseDismissActorAction;
                        details.eventType = EventType.GenericDismissActor;
                        break;
                    case "HandleDispose":
                        Debug.Log(string.Format("ProcessHandleActor: \"{0}\" selected{1}", data.optionText, "\n"));
                        handler = InitialiseDisposeActorAction;
                        details.eventType = EventType.GenericDisposeActor;
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid data.optionText \"{0}\"", data.optionText));
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
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
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

    //methods above here
}
