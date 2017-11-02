using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;


/// <summary>
/// used to return results of effects to calling method
/// </summary>
public class EffectReturn
{
    public string topText { get; set; }
    public string bottomText { get; set; }
    public bool errorFlag { get; set; }
}



/// <summary>
/// handles Effect related matters (Actions and Targets)
/// Effects are assumed to generate text and outcomes for a Modal Outcome window
/// </summary>
public class EffectManager : MonoBehaviour
{

    //colour palette for Modal Outcome
    private string colourOutcome1; //good effect Rebel / bad effect Authority
    private string colourOutcome2; //bad effect Authority / bad effect Rebel
    private string colourOutcome3; //used when node is EqualsTo, eg. reset
    private string colourNormal;
    private string colourDefault;
    private string colourEnd;



    public void Initialise()
    {
        //register listener
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
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        switch (GameManager.instance.optionScript.PlayerSide)
        {
            case Side.Resistance:
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
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// checks whether effect criteria is valid. Returns 'null' if O.K and a tooltip explanation string if not
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public string CheckEffectCriteria(Effect effect, int nodeID = -1)
    {
        string result = null;
        string compareTip = null;
        Node node = null;


        if (effect.criteriaEffect != EffectCriteria.None)
        {
            //Get node regardless of whether the effect is node related or not
            if (nodeID > -1)
            {
                node = GameManager.instance.dataScript.GetNode(nodeID);
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
                            compareTip = ComparisonCheck(effect.criteriaValue, node.CheckNumOfTeams(), effect.criteriaCompare);
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
                        case EffectCriteria.RebelCause:
                            compareTip = ComparisonCheck(effect.criteriaValue, GameManager.instance.playerScript.RebelCauseCurrent, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "Rebel Cause ";
                                result += compareTip;
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
        switch (comparison)
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
    /// Processes effects and returns results in a class
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public EffectReturn ProcessEffect(Effect effect, Node node, Actor actor)
    {
        EffectReturn effectReturn = new EffectReturn();
        //set default values
        effectReturn.errorFlag = false;
        effectReturn.topText = "";
        effectReturn.bottomText = "";
        //valid effect?
        if (effect != null)
        {
            switch (effect.effectOutcome)
            {
                case EffectOutcome.NodeSecurity:
                    if (node != null)
                    {
                        switch (effect.effectResult)
                        {
                            case Result.Add:
                                node.Security += effect.effectValue;
                                node.Security = Mathf.Min(3, node.Security);
                                effectReturn.topText = string.Format("{0}The security system has been swept and strengthened{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Security +{1}{2}", colourOutcome2, effect.effectValue, colourEnd);
                                break;
                            case Result.Subtract:
                                node.Security -= effect.effectValue;
                                node.Security = Mathf.Max(0, node.Security);
                                effectReturn.topText = string.Format("{0}The security system has been successfully hacked{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Security -{1}{2}", colourOutcome1, effect.effectValue, colourEnd);
                                break;
                            case Result.EqualTo:
                                //keep within allowable parameters
                                effect.effectValue = Mathf.Min(3, effect.effectValue);
                                effect.effectValue = Mathf.Max(0, effect.effectValue);
                                node.Security = effect.effectValue;
                                effectReturn.topText = string.Format("{0}The security system has been reset{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Security now {1}{2}", colourOutcome3, node.Security, colourEnd);
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.effectResult));
                                effectReturn.errorFlag = true;
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.effectOutcome));
                        effectReturn.errorFlag = true;
                    }
                    break;
                case EffectOutcome.NodeStability:
                    if (node != null)
                    {
                        switch (effect.effectResult)
                        {
                            case Result.Add:
                                node.Stability += effect.effectValue;
                                node.Stability = Mathf.Min(3, node.Stability);
                                effectReturn.topText = string.Format("{0}Law Enforcement teams have stabilised the situation{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Stability +{1}{2}", colourOutcome2, effect.effectValue, colourEnd);
                                break;
                            case Result.Subtract:
                                node.Stability -= effect.effectValue;
                                node.Stability = Mathf.Max(0, node.Stability);
                                effectReturn.topText = string.Format("{0}Civil unrest and instability is spreading throughout{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Stability -{1}{2}", colourOutcome1, effect.effectValue, colourEnd);
                                break;
                            case Result.EqualTo:
                                //keep within allowable parameters
                                effect.effectValue = Mathf.Min(3, effect.effectValue);
                                effect.effectValue = Mathf.Max(0, effect.effectValue);
                                node.Stability = effect.effectValue;
                                effectReturn.topText = string.Format("{0}Civil obedience has been reset to a new level{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Stability now {1}{2}", colourOutcome3, node.Stability, colourEnd);
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.effectResult));
                                effectReturn.errorFlag = true;
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.effectOutcome));
                        effectReturn.errorFlag = true;
                    }
                    break;
                case EffectOutcome.NodeSupport:
                    if (node != null)
                    {
                        switch (effect.effectResult)
                        {
                            case Result.Add:
                                node.Support += effect.effectValue;
                                node.Support = Mathf.Min(3, node.Support);
                                effectReturn.topText = string.Format("{0}There is a surge of support for the Rebels{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Support +{1}{2}", colourOutcome1, effect.effectValue, colourEnd);
                                break;
                            case Result.Subtract:
                                node.Support -= effect.effectValue;
                                node.Support = Mathf.Max(0, node.Support);
                                effectReturn.topText = string.Format("{0}The Rebels are losing popularity{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Support -{1}{2}", colourOutcome2, effect.effectValue, colourEnd);
                                break;
                            case Result.EqualTo:
                                //keep within allowable parameters
                                effect.effectValue = Mathf.Min(3, effect.effectValue);
                                effect.effectValue = Mathf.Max(0, effect.effectValue);
                                node.Support = effect.effectValue;
                                effectReturn.topText = string.Format("{0}Rebel sentiment has been reset to a new level{1}", colourDefault, colourEnd);
                                effectReturn.bottomText = string.Format("{0}Node Support now {1}{2}", colourOutcome3, node.Support, colourEnd);
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.effectResult));
                                effectReturn.errorFlag = true;
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.effectOutcome));
                        effectReturn.errorFlag = true;
                    }
                    break;
                case EffectOutcome.RebelCause:
                    int rebelCause = GameManager.instance.playerScript.RebelCauseCurrent;
                    int maxCause = GameManager.instance.playerScript.RebelCauseMax;
                    switch (effect.effectResult)
                    {
                        case Result.Add:
                            rebelCause += effect.effectValue;
                            rebelCause = Mathf.Min(maxCause, rebelCause);
                            GameManager.instance.playerScript.RebelCauseCurrent = rebelCause;
                            effectReturn.topText = string.Format("{0}The Rebel Cause gains traction{1}", colourDefault, colourEnd);
                            effectReturn.bottomText = string.Format("{0}Rebel Cause +{1}{2}", colourOutcome1, effect.effectValue, colourEnd);
                            break;
                        case Result.Subtract:
                            rebelCause -= effect.effectValue;
                            rebelCause = Mathf.Max(0, rebelCause);
                            GameManager.instance.playerScript.RebelCauseCurrent = rebelCause;
                            effectReturn.topText = string.Format("{0}The Rebel Cause is losing ground{1}", colourDefault, colourEnd);
                            effectReturn.bottomText = string.Format("{0}Rebel Cause -{1}{2}", colourOutcome2, effect.effectValue, colourEnd);
                            break;
                        case Result.EqualTo:
                            //keep within allowable parameters
                            effect.effectValue = Mathf.Min(maxCause, effect.effectValue);
                            effect.effectValue = Mathf.Max(0, effect.effectValue);
                            rebelCause = effect.effectValue;
                            GameManager.instance.playerScript.RebelCauseCurrent = rebelCause;
                            effectReturn.topText = string.Format("{0}The Rebel Cause adjusts to a new level{1}", colourDefault, colourEnd);
                            effectReturn.bottomText = string.Format("{0}Rebel Cause now {1}{2}", colourOutcome3, rebelCause, colourEnd);
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectResult \"{0}\"", effect.effectResult));
                            effectReturn.errorFlag = true;
                            break;
                    }
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
                case EffectOutcome.Renown:
                    if (node != null)
                    {
                        if (actor != null)
                        {
                            if (node.NodeID == GameManager.instance.nodeScript.nodePlayer)
                            {
                                //Player effect
                                switch (effect.effectResult)
                                {
                                    case Result.Add:
                                        GameManager.instance.playerScript.Renown++;
                                        effectReturn.bottomText = string.Format("{0}Player {1}{2}", colourOutcome1, effect.description, colourEnd);
                                        break;
                                    case Result.Subtract:
                                        if (GameManager.instance.playerScript.Renown >= effect.effectValue)
                                        { GameManager.instance.playerScript.Renown -= effect.effectValue; }
                                        effectReturn.bottomText = string.Format("{0}Player {1}{2}", colourOutcome2, effect.description, colourEnd);
                                        break;
                                }
                            }
                            else
                            {
                                //Actor effect
                                switch (effect.effectResult)
                                {
                                    case Result.Add:
                                        actor.Renown++;
                                        effectReturn.bottomText = string.Format("{0}{1} {2}{3}", colourOutcome2, actor.Name, effect.description, colourEnd);
                                        break;
                                    case Result.Subtract:
                                        if (actor.Renown >= effect.effectValue)
                                        { actor.Renown -= effect.effectValue; }
                                        effectReturn.bottomText = string.Format("{0}{1} {2}{3}", colourOutcome1, actor.Name, effect.description, colourEnd);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Debug.LogError(string.Format("Invalid Actor (null) for EffectOutcome \"{0}\"", effect.effectOutcome));
                            effectReturn.errorFlag = true;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.effectOutcome));
                        effectReturn.errorFlag = true;
                    }
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.effectOutcome));
                    effectReturn.errorFlag = true;
                    break;
            }
        }
        else
        {
            //No effect paramater
            Debug.LogError("Invalid Effect (null)");
            effectReturn = null;
        }

        return effectReturn;
    }


    //place methods above here
}
