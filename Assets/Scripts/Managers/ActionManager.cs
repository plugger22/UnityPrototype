using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using modalAPI;
using gameAPI;
using System.Text;

/// <summary>
/// Handles all action related matters
/// </summary>
public class ActionManager : MonoBehaviour
{

    public Sprite errorSprite;
    public Sprite targetSprite;

    //colour palette for Modal Outcome
    private string colourOutcome1; //good effect Rebel / bad effect Authority
    private string colourOutcome2; //bad effect Authority / bad effect Rebel
    private string colourOutcome3; //used when node is EqualsTo, eg. reset
    private string colourNormal;
    private string colourDefault;
    private string colourError;
    private string colourEnd;

    public void Initialise()
    {
        //register listener
        EventManager.instance.AddListener(EventType.NodeAction, OnEvent);
        EventManager.instance.AddListener(EventType.TargetAction, OnEvent);
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
                ModalActionDetails details = Param as ModalActionDetails;
                ProcessNodeAction(details);
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
        switch(GameManager.instance.optionScript.PlayerSide)
        {
            case Side.Rebel:
                colourOutcome1 = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
                colourOutcome2 = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
                break;
            case Side.Authority:
                colourOutcome1 = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
                colourOutcome2 = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
                break;
        }
        colourOutcome3 = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourError = GameManager.instance.colourScript.GetColour(ColourType.error);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
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
        outcomeDetails.textTop = string.Format("{0}What, nothing happened?{1}", colourError, colourEnd);
        outcomeDetails.textBottom = string.Format("{0}No effect{1}", colourError, colourEnd);
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
                            //two builders for top and bottom texts
                            StringBuilder builderTop = new StringBuilder();
                            StringBuilder builderBottom = new StringBuilder();
                            builderTop.Append(string.Format("{0}{1}, ID {2}{3}", colourNormal, node.NodeName, node.NodeID, colourEnd));
                            builderTop.AppendLine();
                            builderTop.AppendLine();
                            //
                            // - - - Process effects
                            //
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
                                                builderTop.Append(string.Format("{0}The security system has been swept and strengthened{1}", colourDefault, colourEnd));
                                                builderBottom.Append(string.Format("{0}Node Security +{1}{2}", colourOutcome2, effect.effectValue, colourEnd));
                                                break;
                                            case Result.Subtract:
                                                node.Security -= effect.effectValue;
                                                node.Security = Mathf.Max(0, node.Security);
                                                builderTop.Append(string.Format("{0}The security system has been successfully hacked{1}", colourDefault, colourEnd));
                                                builderBottom.Append(string.Format("{0}Node Security -{1}{2}", colourOutcome1, effect.effectValue, colourEnd));
                                                break;
                                            case Result.EqualTo:
                                                //keep within allowable parameters
                                                effect.effectValue = Mathf.Min(3, effect.effectValue);
                                                effect.effectValue = Mathf.Max(0, effect.effectValue);
                                                node.Security = effect.effectValue;
                                                builderTop.Append(string.Format("{0}The security system has been reset{1}", colourDefault, colourEnd));
                                                builderBottom.Append(string.Format("{0}Node Security now {1}{2}", colourOutcome3, node.Security, colourEnd));
                                                break;
                                            default:
                                                Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.effectResult));
                                                errorFlag = true;
                                                break;
                                        }
                                        outcomeDetails.sprite = actor.arc.actionSprite;
                                        break;
                                    case EffectOutcome.NodeStability:
                                        switch (effect.effectResult)
                                        {
                                            case Result.Add:
                                                node.Stability += effect.effectValue;
                                                node.Stability = Mathf.Min(3, node.Stability);
                                                builderTop.Append(string.Format("{0}Law Enforcement teams have stabilised the situation{1}", colourDefault, colourEnd));
                                                builderBottom.Append(string.Format("{0}Node Stability +{1}{2}", colourOutcome2, effect.effectValue, colourEnd));
                                                break;
                                            case Result.Subtract:
                                                node.Stability -= effect.effectValue;
                                                node.Stability = Mathf.Max(0, node.Stability);
                                                builderTop.Append(string.Format("{0}Civil unrest and instability is spreading throughout{1}", colourDefault, colourEnd));
                                                builderBottom.Append(string.Format("{0}Node Stability -{1}{2}", colourOutcome1, effect.effectValue, colourEnd));
                                                break;
                                            case Result.EqualTo:
                                                //keep within allowable parameters
                                                effect.effectValue = Mathf.Min(3, effect.effectValue);
                                                effect.effectValue = Mathf.Max(0, effect.effectValue);
                                                node.Stability = effect.effectValue;
                                                builderTop.Append(string.Format("{0}Civil obedience has been reset to a new level{1}", colourDefault, colourEnd));
                                                builderBottom.Append(string.Format("{0}Node Stability now {1}{2}", colourOutcome3, node.Stability, colourEnd));
                                                break;
                                            default:
                                                Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.effectResult));
                                                errorFlag = true;
                                                break;
                                        }
                                        outcomeDetails.sprite = actor.arc.actionSprite;
                                        break;
                                    case EffectOutcome.NodeSupport:
                                        switch (effect.effectResult)
                                        {
                                            case Result.Add:
                                                node.Support += effect.effectValue;
                                                node.Support = Mathf.Min(3, node.Support);
                                                builderTop.Append(string.Format("{0}People are flocking to the Rebel cause{1}", colourDefault, colourEnd));
                                                builderBottom.Append(string.Format("{0}Node Support +{1}{2}", colourOutcome1, effect.effectValue, colourEnd));
                                                break;
                                            case Result.Subtract:
                                                node.Support -= effect.effectValue;
                                                node.Support = Mathf.Max(0, node.Support);
                                                builderTop.Append(string.Format("{0}The Rebel cause is losing popularity{1}", colourDefault, colourEnd));
                                                builderBottom.Append(string.Format("{0}Node Support -{1}{2}", colourOutcome2, effect.effectValue, colourEnd));
                                                break;
                                            case Result.EqualTo:
                                                //keep within allowable parameters
                                                effect.effectValue = Mathf.Min(3, effect.effectValue);
                                                effect.effectValue = Mathf.Max(0, effect.effectValue);
                                                node.Support = effect.effectValue;
                                                builderTop.Append(string.Format("{0}Support for te Rebel cause has been reset to a new level{1}", colourDefault, colourEnd));
                                                builderBottom.Append(string.Format("{0}Node Support now {1}{2}", colourOutcome3, node.Support, colourEnd));
                                                break;
                                            default:
                                                Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.effectResult));
                                                errorFlag = true;
                                                break;
                                        }
                                        outcomeDetails.sprite = actor.arc.actionSprite;
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
                                    case EffectOutcome.SpreadInstability:

                                        break;
                                    default:
                                        Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.effectOutcome));
                                        errorFlag = true;
                                        break;
                                }
                                if (errorFlag == true) { break; }
                                else
                                {
                                    //
                                    // - - - Renown - - - 
                                    //
                                    if (details.RenownEffect != null)
                                    {
                                        if (effect.effectOutcome > EffectOutcome.None)
                                        {
                                            switch (details.RenownEffect.effectOutcome)
                                            {
                                                case EffectOutcome.PlayerRenown:
                                                    switch (details.RenownEffect.effectResult)
                                                    {
                                                        case Result.Add:
                                                            GameManager.instance.playerScript.Renown++;
                                                            builderBottom.AppendLine();
                                                            builderBottom.Append(string.Format("{0}{1}{2}", colourOutcome1, details.RenownEffect.description, colourEnd));
                                                            break;
                                                        case Result.Subtract:
                                                            if (GameManager.instance.playerScript.Renown >= details.RenownEffect.effectValue)
                                                            { GameManager.instance.playerScript.Renown -= details.RenownEffect.effectValue; }
                                                            builderBottom.AppendLine();
                                                            builderBottom.Append(string.Format("{0}{1}{2}", colourOutcome2, details.RenownEffect.description, colourEnd));
                                                            break;
                                                    }
                                                    break;
                                                case EffectOutcome.ActorRenown:
                                                    switch (details.RenownEffect.effectResult)
                                                    {
                                                        case Result.Add:
                                                            actor.Renown++;
                                                            builderBottom.AppendLine();
                                                            builderBottom.Append(string.Format("{0}{1} {2}{3}", colourOutcome2, actor.Name, details.RenownEffect.description, colourEnd));
                                                            break;
                                                        case Result.Subtract:
                                                            if (actor.Renown >= details.RenownEffect.effectValue)
                                                            { actor.Renown -= details.RenownEffect.effectValue; }
                                                            builderBottom.AppendLine();
                                                            builderBottom.Append(string.Format("{0}{1} {2}{3}", colourOutcome1, actor.Name, details.RenownEffect.description, colourEnd));
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    Debug.LogError(string.Format("Invalid Renown Effect \"{0}\"", details.RenownEffect.effectOutcome));
                                                    errorFlag = true;
                                                    break;
                                            }
                                        }
                                        else
                                        { Debug.LogError("EffectOutcome invalid (\"None\")"); errorFlag = true;}
                                    }
                                    else
                                    { Debug.LogError("Invalid RenownEffect (null)"); errorFlag = true; }
                                    //texts
                                    outcomeDetails.textTop = builderTop.ToString();
                                    outcomeDetails.textBottom = builderBottom.ToString();
                                }
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
