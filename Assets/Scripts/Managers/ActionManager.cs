using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all action related matters
/// </summary>
public class ActionManager : MonoBehaviour
{

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


        if (effect.criteriaEffect != EffectType.None)
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
                        case EffectType.NodeSecurity:
                            compareTip = ComparisonCheck(effect.criteriaValue, node.security, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "Security ";
                                result += compareTip;
                            }
                            break;
                        case EffectType.NodeStability:
                            compareTip = ComparisonCheck(effect.criteriaValue, node.stability, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "Stability ";
                                result += compareTip;
                            }
                            break;
                        case EffectType.NodeSupport:
                            compareTip = ComparisonCheck(effect.criteriaValue, node.support, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "Support ";
                                result += compareTip;
                            }
                            break;
                        case EffectType.NumRecruits:
                            compareTip = ComparisonCheck(effect.criteriaValue, GameManager.instance.playerScript.NumOfRecruits, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "maxxed Recruit allowance";
                            }
                            break;
                        case EffectType.NumTeams:
                            compareTip = ComparisonCheck(effect.criteriaValue, node.NumOfTeams, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "no Teams present";
                            }
                            break;
                        case EffectType.NumTracers:
                            compareTip = ComparisonCheck(effect.criteriaValue, node.NumOfTracers, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "Tracers already present";
                            }
                            break;
                        case EffectType.TargetInfo:
                            compareTip = ComparisonCheck(effect.criteriaValue, node.TargetInfo, effect.criteriaCompare);
                            if (compareTip != null)
                            {
                                result = "Full Info already";
                            }
                            break;
                        case EffectType.NumGear:
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

}
