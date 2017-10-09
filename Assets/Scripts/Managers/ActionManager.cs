using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using modalAPI;
using gameAPI;

/// <summary>
/// Handles all action related matters
/// </summary>
public class ActionManager : MonoBehaviour
{

    public Sprite errorSprite;
    public Sprite targetSprite;


    private void Start()
    {
        //register listener
        EventManager.instance.AddListener(EventType.NodeAction, OnEvent);
        EventManager.instance.AddListener(EventType.TargetAction, OnEvent);
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
                ModalActionDetails details = Param as ModalActionDetails;
                ProcessNodeAction(details);
                break;
            case EventType.TargetAction:
                ProcessNodeTarget((int)Param);
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
    /// checks whether effect criteria is valid. Returns 'null' if O.K and a tooltip explanation string if not
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public string CheckEffectCriteria(ActionEffect effect, int nodeID = -1)
    {
        string result = null;
        string compareTip = null;
        Node node = null;


        if (effect.criteriaEffect != EffectCriteria.None)
        {
            //Get node regardless of whether the effect is node related or not
            if (nodeID > -1)
            {
                GameObject objNode = GameManager.instance.dataScript.GetNodeObject(nodeID);
                if (objNode != null)
                {
                    node = objNode.GetComponent<Node>();
                }
                if (node != null)
                {
                    //effect type
                    switch (effect.criteriaEffect)
                    {
                        case EffectCriteria.NodeSecurity:
                            compareTip = ComparisonCheck(effect.criteriaValue, node.Security, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "Security ";
                                result += compareTip;
                            }
                            break;
                        case EffectCriteria.NodeStability:
                            compareTip = ComparisonCheck(effect.criteriaValue, node.Stability, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "Stability ";
                                result += compareTip;
                            }
                            break;
                        case EffectCriteria.NodeSupport:
                            compareTip = ComparisonCheck(effect.criteriaValue, node.Support, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "Support ";
                                result += compareTip;
                            }
                            break;
                        case EffectCriteria.NumRecruits:
                            compareTip = ComparisonCheck(effect.criteriaValue, GameManager.instance.playerScript.NumOfRecruits, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "maxxed Recruit allowance";
                            }
                            break;
                        case EffectCriteria.NumTeams:
                            compareTip = ComparisonCheck(effect.criteriaValue, node.NumOfTeams, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "no Teams present";
                            }
                            break;
                        case EffectCriteria.NumTracers:
                            compareTip = ComparisonCheck(effect.criteriaValue, node.NumOfTracers, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "Tracers already present";
                            }
                            break;
                        case EffectCriteria.TargetInfo:
                            compareTip = ComparisonCheck(effect.criteriaValue, node.TargetID, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "Full Info already";
                            }
                            break;
                        case EffectCriteria.NumGear:
                            compareTip = ComparisonCheck(effect.criteriaValue, GameManager.instance.playerScript.NumOfGear, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "maxxed Gear Allowance";
                            }
                            break;
                        default:
                            result = "Error!";
                            Debug.LogError("Invalid effect.criteriaEffect");
                            break;
                    }
                }
                else { Debug.LogError("Invalid node (null)"); }
            }
            //player related

        
        }
        return result;
    }


    /// <summary>
    /// returns null if all O.K and a tool tip string if not giving criteria, eg. "< 1"
    /// </summary>
    /// <param name="criteriaValue"></param>
    /// <param name="actualValue"></param>
    /// <param name="comparison"></param>
    /// <returns></returns>
    private string ComparisonCheck(int criteriaValue, int actualValue, Comparison comparison)
    {
        string result = null;
        switch(comparison)
        {
            case Comparison.LessThan:
                if (actualValue >= criteriaValue)
                { result = string.Format("< {0}, currently {1}", criteriaValue, actualValue); }
                break;
            case Comparison.GreaterThan:
                if (actualValue <= criteriaValue)
                { result = string.Format("> {0}, currently {1}", criteriaValue, actualValue); }
                break;
            case Comparison.EqualTo:
                if (criteriaValue != actualValue)
                { result = string.Format("{0}, currently {1}", criteriaValue, actualValue); }
                break;
            default:
                result = "Error!";
                Debug.LogError("Invalid Comparison enum");
                break;
        }
        return result;
    }

    /// <summary>
    /// Processes node actor actions
    /// </summary>
    /// <param name="details"></param>
    public void ProcessNodeAction(ModalActionDetails details)
    {
        bool errorFlag = false;
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        //default data 
        outcomeDetails.textTop = "What, nothing happened?";
        outcomeDetails.textBottom = "No effect";
        outcomeDetails.sprite = errorSprite;
        //resolve action
        if (details != null)
        {
            //get Actor
            Actor actor = GameManager.instance.actorScript.GetActor(details.ActorSlotID);
            if (actor != null)
            {
                //get node
                GameObject nodeObject = GameManager.instance.dataScript.GetNodeObject(details.NodeID);
                if (nodeObject != null)
                {
                    Node node = nodeObject.GetComponent<Node>();
                    if (node != null)
                    {
                        //Get Action & Effects
                        Action action = actor.arc.nodeAction;
                        List<ActionEffect> listOfEffects = action.GetEffects();
                        if (listOfEffects.Count > 0)
                        {
                            //process effects
                            foreach (ActionEffect effect in listOfEffects)
                            {
                                switch(effect.effectOutcome)
                                {
                                    case EffectOutcome.NodeSecurity:
                                        switch (effect.effectResult)
                                        {
                                            case Result.Add:
                                                node.Security += effect.effectValue;
                                                node.Security = Mathf.Min(3, node.Security);
                                                outcomeDetails.textTop = string.Format("The security system has been swept and strengthened");
                                                outcomeDetails.textBottom = string.Format("Node Security +{0}", effect.effectValue);
                                                break;
                                            case Result.Subtract:
                                                node.Security -= effect.effectValue;
                                                node.Security = Mathf.Max(0, node.Security);
                                                outcomeDetails.textTop = string.Format("The security system has been successfully hacked", node.NodeName);
                                                outcomeDetails.textBottom = string.Format("Node Security -{0}", effect.effectValue);
                                                break;
                                            case Result.EqualTo:
                                                //keep within allowable parameters
                                                effect.effectValue = Mathf.Min(3, effect.effectValue);
                                                effect.effectValue = Mathf.Max(0, effect.effectValue);
                                                node.Security = effect.effectValue;
                                                outcomeDetails.textTop = string.Format("The security system has been reset", node.NodeName);
                                                outcomeDetails.textBottom = string.Format("Node Security now {0}", node.Security);
                                                break;
                                            default:
                                                Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.effectResult));
                                                errorFlag = true;
                                                break;
                                        }
                                        outcomeDetails.sprite = actor.arc.actionSprite;
                                        break;
                                    case EffectOutcome.NodeStability:
                                        node.Stability--;
                                        break;
                                    case EffectOutcome.NodeSupport:
                                        node.Support--;
                                        break;
                                    case EffectOutcome.Recruit:

                                        break;
                                    case EffectOutcome.AddTracer:

                                        break;
                                    case EffectOutcome.GetGear:

                                        break;
                                    case EffectOutcome.GetTargetInfo:

                                        break;
                                    case EffectOutcome.NeutraliseTeam:

                                        break;
                                    case EffectOutcome.SpreadChaos:

                                        break;
                                    default:
                                        Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.effectOutcome));
                                        errorFlag = true;
                                        break;
                                }
                                if (errorFlag == true) { break; }
                            }
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
            //fault, pass default data to window
            outcomeDetails.textTop = "There is a glitch in the system. Something has gone wrong";
            outcomeDetails.textBottom = "Bad, all Bad";
            outcomeDetails.sprite = errorSprite;
        }
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
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        //default data 
        outcomeDetails.textTop = "Target is in our sights!";
        outcomeDetails.textBottom = "Fire when ready";
        outcomeDetails.sprite = targetSprite;

        //Process target - - -  TO DOs

        if (errorFlag == true)
        {
            //fault, pass default data to window
            outcomeDetails.textTop = "There is a fault in the system. Target not responding";
            outcomeDetails.textBottom = "Target Acquition Failed";
            outcomeDetails.sprite = errorSprite;
        }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }



    //methods above here
}
