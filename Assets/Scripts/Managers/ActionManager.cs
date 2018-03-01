

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using modalAPI;
using gameAPI;
using packageAPI;
using System.Text;
using delegateAPI;

namespace delegateAPI
{

    public delegate void manageDelegate(ModalActionDetails details);
}

/// <summary>
/// Handles all action related matters
/// </summary>
public class ActionManager : MonoBehaviour
{

    public Sprite errorSprite;
    public Sprite targetSprite;

    //colour palette for Modal Outcome
    private string colourNormal;
    private string colourError;
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourAlert;
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
                ProcessLieLowAction(detailsLieLow);
                break;
            case EventType.ActivateAction:
                ModalActionDetails detailsActivate = Param as ModalActionDetails;
                ProcessActivateAction(detailsActivate);
                break;
            case EventType.GiveGearAction:
                ModalActionDetails detailsGiveGear = Param as ModalActionDetails;
                ProcessGiveGearAction(detailsGiveGear);
                break;
            case EventType.GenericHandleActor:
                GenericReturnData returnDataRecall = Param as GenericReturnData;
                ProcessHandleActor(returnDataRecall);
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
        outcomeDetails.sprite = errorSprite;
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
                                    dataInput.ongoingID = GameManager.instance.effectScript.GetOngoingEffectID();

                                    //NOTE: A standard node action can use ongoing effects but there is no way of linking it. The node effects will time out and that's it
                                    
                                    /*//add to register
                                    string registerDetails = string.Format("Effect \"{0}\", ID {1}, at {2}, ID {3}, t{4}", action.name, effect.effectID, node.Arc.name,
                                        node.nodeID, GameManager.instance.turnScript.Turn);
                                    GameManager.instance.dataScript.AddOngoingEffectToDict(dataInput.ongoingID, registerDetails);*/
                                }
                                else { dataInput.ongoingID = -1; dataInput.ongoingText = ""; }
                                effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actor);
                                if (effectReturn != null)
                                {
                                    outcomeDetails.sprite = actor.arc.actionSprite;
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
            outcomeDetails.sprite = errorSprite;
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
        int renownCost = GameManager.instance.playerScript.renownCostGear;
        int renownBefore = GameManager.instance.playerScript.Renown;
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = errorSprite;
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
                                outcomeDetails.sprite = errorSprite;
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
            outcomeDetails.sprite = errorSprite;
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
            outcomeDetails.sprite = errorSprite;
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
        bool errorFlag = false;
        string title;
        string colourSide;
        bool isResistance = true;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; isResistance = false; }
        else { colourSide = colourResistance; isResistance = true; }
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorSlotID, playerSide);
        if (actor != null)
        {
            //ActorHandle
            List<ManageAction> listOfHandleOptions = GameManager.instance.dataScript.GetListOfActorHandle();
            if (listOfHandleOptions != null)
            {
                genericDetails.returnEvent = EventType.GenericHandleActor;
                genericDetails.side = playerSide;
                //genericDetails.nodeID = -1;
                genericDetails.actorSlotID = details.actorSlotID;
                title = string.Format("{0}", isResistance ? "" : GameManager.instance.metaScript.GetAuthorityTitle().ToString() + " ");
                //picker text
                genericDetails.textTop = string.Format("{0}Manage{1} {2}{3} {4}{5}{6}", colourNeutral, colourEnd, colourNormal, actor.arc.name, title,
                    actor.actorName, colourEnd);
                genericDetails.textMiddle = string.Format("{0}You have a range of managerial options at your disposal. Be prepared to justify your decision{1}",
                    colourNormal, colourEnd);
                genericDetails.textBottom = "Click on an option to Select. Press CONFIRM once done. Mouseover options for more information.";
                //Create a set of options for the picker -> take only the first three options (picker can't handle more)
                GenericOptionDetails[] arrayOfGenericOptions = new GenericOptionDetails[3];
                GenericTooltipDetails[] arrayOfTooltips = new GenericTooltipDetails[3];
                int numOfOptions = Mathf.Min(3, listOfHandleOptions.Count);
                for(int i = 0; i < numOfOptions; i++)
                {
                    ManageAction manageAction = listOfHandleOptions[i];
                    if (manageAction != null)
                    {
                        GenericOptionDetails option = new GenericOptionDetails()
                        {
                            text = manageAction.optionTitle,
                            optionID = details.actorSlotID,
                            optionText = manageAction.name,
                            sprite = manageAction.sprite
                        };
                        GenericTooltipDetails tooltip = new GenericTooltipDetails()
                        {
                            textHeader = string.Format("{0}INFO{1}", colourSide, colourEnd),
                            textMain = manageAction.tooltipMain,
                            textDetails = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd)
                        };
                        //add to arrays
                        arrayOfGenericOptions[i] = option;
                        arrayOfTooltips[i] = tooltip;
                    }
                    else { Debug.LogError(string.Format("Invalid manageAction (Null) for listOfHandleOptions[{0}]", i)); }
                }
                //add options to picker data package
                genericDetails.arrayOfOptions = arrayOfGenericOptions;
                genericDetails.arrayOfTooltips = arrayOfTooltips;
            }
            else { Debug.LogError("Invalid listOfHandleOptions (Null)"); errorFlag = true; }

        }
        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", details.actorSlotID)); errorFlag = true; }

        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
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
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
        }
    }

    /// <summary>
    /// Process Manage -> Send to Reserve -> actor action (second level of the nested Manage actor menus)
    /// </summary>
    /// <param name="details"></param>
    private void ProcessReserveActorAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        string title;
        string colourSide;
        bool isResistance = true;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if (playerSide.level == GameManager.instance.globalScript.sideAuthority.level)
        { colourSide = colourAuthority; isResistance = false; }
        else { colourSide = colourResistance; isResistance = true; }
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorSlotID, playerSide);
        if (actor != null)
        {
            //ActorHandle
            List<ManageAction> listOfHandleOptions = GameManager.instance.dataScript.GetListOfActorHandle();
            if (listOfHandleOptions != null)
            {
                genericDetails.returnEvent = EventType.GenericHandleActor;
                genericDetails.side = playerSide;
                //genericDetails.nodeID = -1;
                genericDetails.actorSlotID = details.actorSlotID;
                title = string.Format("{0}", isResistance ? "" : GameManager.instance.metaScript.GetAuthorityTitle().ToString() + " ");
                //picker text
                genericDetails.textTop = string.Format("{0}Manage{1} {2}{3} {4}{5}{6}", colourNeutral, colourEnd, colourNormal, actor.arc.name, title,
                    actor.actorName, colourEnd);
                genericDetails.textMiddle = string.Format("{0}You have a range of managerial options at your disposal. Be prepared to justify your decision{1}",
                    colourNormal, colourEnd);
                genericDetails.textBottom = "Click on an option to Select. Press CONFIRM once done. Mouseover options for more information.";
                //Create a set of options for the picker -> take only the first three options (picker can't handle more)
                GenericOptionDetails[] arrayOfGenericOptions = new GenericOptionDetails[3];
                GenericTooltipDetails[] arrayOfTooltips = new GenericTooltipDetails[3];
                int numOfOptions = Mathf.Min(3, listOfHandleOptions.Count);
                for (int i = 0; i < numOfOptions; i++)
                {
                    ManageAction manageAction = listOfHandleOptions[i];
                    if (manageAction != null)
                    {
                        GenericOptionDetails option = new GenericOptionDetails()
                        {
                            text = manageAction.optionTitle,
                            optionID = details.actorSlotID,
                            optionText = manageAction.name,
                            sprite = manageAction.sprite
                        };
                        GenericTooltipDetails tooltip = new GenericTooltipDetails()
                        {
                            textHeader = string.Format("{0}INFO{1}", colourSide, colourEnd),
                            textMain = manageAction.tooltipMain,
                            textDetails = string.Format("{0}{1} {2}{3}", colourNeutral, actor.actorName, manageAction.tooltipDetails, colourEnd)
                        };
                        //add to arrays
                        arrayOfGenericOptions[i] = option;
                        arrayOfTooltips[i] = tooltip;
                    }
                    else { Debug.LogError(string.Format("Invalid manageAction (Null) for listOfHandleOptions[{0}]", i)); }
                }
                //add options to picker data package
                genericDetails.arrayOfOptions = arrayOfGenericOptions;
                genericDetails.arrayOfTooltips = arrayOfTooltips;
            }
            else { Debug.LogError("Invalid listOfHandleOptions (Null)"); errorFlag = true; }

        }
        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", details.actorSlotID)); errorFlag = true; }

        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
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
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
        }
    }


    /// <summary>
    /// Process Lie Low actor action (Resistance only)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessLieLowAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = errorSprite;
        if (details != null)
        {
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorSlotID, details.side);
            if (actor != null)
            {
                /*string title = "";
                if (details.side == GameManager.instance.globalScript.sideAuthority)
                { title = string.Format(" {0}", GameManager.instance.metaScript.GetAuthorityTitle()); }*/
                actor.Status = ActorStatus.Inactive;
                int numOfTurns = 3 - actor.datapoint2;
                outcomeDetails.textTop = string.Format(" {0} {1} has been ordered to Lie Low", actor.arc.name, actor.actorName );
                outcomeDetails.textBottom = string.Format("{0}{1} will be Inactive for {2} turn{3} or until Activated{4}", colourNeutral, actor.actorName, 
                    numOfTurns, numOfTurns != 1 ? "s" : "", colourEnd);
                //message
                string text = string.Format("{0} {1}, is lying Low. Status: {2}", actor.arc.name, actor.actorName, actor.Status); 
                Message message = GameManager.instance.messageScript.ActorStatus(text, actor.actorID);
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
            outcomeDetails.sprite = errorSprite;
        }
        else
        {
            //change alpha of actor to indicate inactive status
            GameManager.instance.guiScript.UpdateActorAlpha(details.actorSlotID, GameManager.instance.guiScript.alphaInactive);
        }
        //action (if valid) expended -> must be BEFORE outcome window event
        if (errorFlag == false)
        { outcomeDetails.isAction = true; }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }


    /// <summary>
    /// Process Activate actor action (Resistance only at present but set up for both sides)
    /// </summary>
    /// <param name="details"></param>
    public void ProcessActivateAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        //default data 
        outcomeDetails.side = details.side;
        outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
        outcomeDetails.sprite = errorSprite;
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
                Message message = GameManager.instance.messageScript.ActorStatus(text, actor.actorID);
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
            outcomeDetails.sprite = errorSprite;
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
        outcomeDetails.sprite = errorSprite;
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
                              colourBad, gear.name, colourEnd, "\n","\n", colourGood, actor.actorName, benefit, "\n", "\n", benefit, "\n", "\n", actor.actorName, benefit, colourEnd);
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
            outcomeDetails.sprite = errorSprite;
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
                    outcomeDetails.sprite = errorSprite;
                }
                //generate a create modal window event
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
            }
            else { Debug.LogError(string.Format("Invalid Target (Null) for node.targetID {0}", nodeID)); }
        }
        else { Debug.LogError(string.Format("Invalid node (Null) for nodeID {0}", nodeID)); }

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
            if (string.IsNullOrEmpty(data.optionText) == false)
            {
                switch (data.optionText)
                {
                    case "HandleReserve":
                        Debug.Log(string.Format("ProcessHandleActor: \"{0}\" selected{1}", data.optionText, "\n"));
                        handler = ProcessReserveActorAction(details);
                        break;
                    case "HandleDismiss":
                        Debug.Log(string.Format("ProcessHandleActor: \"{0}\" selected{1}", data.optionText, "\n"));
                        break;
                    case "HandleDispose":
                        Debug.Log(string.Format("ProcessHandleActor: \"{0}\" selected{1}", data.optionText, "\n"));
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

        }
    }

    //methods above here
}
