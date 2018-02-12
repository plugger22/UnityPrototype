using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using modalAPI;
using gameAPI;
using packageAPI;
using System.Text;

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
    private string colourEnd;

    public void Initialise()
    {
        //register listener
        EventManager.instance.AddListener(EventType.NodeAction, OnEvent);
        EventManager.instance.AddListener(EventType.TargetAction, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.InsertTeamAction, OnEvent);
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
            case EventType.InsertTeamAction:
                ModalActionDetails detailsTeam = Param as ModalActionDetails;
                ProcessTeamAction(detailsTeam);
                break;
            case EventType.TargetAction:
                ProcessNodeTarget((int)Param);
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
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourError = GameManager.instance.colourScript.GetColour(ColourType.error);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
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
            Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.ActorSlotID, GameManager.instance.sideScript.PlayerSide);
            if (actor != null)
            {
                //get node
                GameObject nodeObject = GameManager.instance.dataScript.GetNodeObject(details.NodeID);
                if (nodeObject != null)
                {
                    Node node = nodeObject.GetComponent<Node>();
                    if (node != null)
                    {
                        //check for Resistance player/actor getting captured prior to carrying out action
                        if (details.side == SideEnum.Resistance)
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
                                effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actor);
                                if (effectReturn != null)
                                {
                                    outcomeDetails.sprite = actor.arc.actionSprite;
                                    //update stringBuilder texts
                                    if (effectReturn.topText.Length > 0)
                                    {
                                        builderTop.AppendLine();
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
                    int slotID = GameManager.instance.dataScript.CheckActorPresent(target.actorArc.ActorArcID, SideEnum.Resistance);
                    if (slotID > -1)
                    {
                        //get actor
                        actor = GameManager.instance.dataScript.GetCurrentActor(slotID, SideEnum.Resistance);
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
                // - - - Process target - - -  TO DO
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
                        //add to register
                        string registerDetails = string.Format("Target \"{0}\", ID {1}, at {2}, ID {3}, t{4}",  target.name, target.targetID, node.Arc.name.ToUpper(), 
                            node.nodeID, GameManager.instance.turnScript.Turn);
                        GameManager.instance.dataScript.AddOngoingIDToDict(target.ongoingID, registerDetails);
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
                    outcomeDetails.side = SideEnum.Resistance;
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


    //methods above here
}
