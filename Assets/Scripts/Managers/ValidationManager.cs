using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles data validation at game start. Optional toggle in GameManager to run, or not
/// Generates '[Val]' log tag entries for any failed validation checks (Not errors as you want it to complete the run and check all)
/// </summary>
public class ValidationManager : MonoBehaviour
{
    /// <summary>
    /// Master control for all validations
    /// </summary>
    public void Initialise()
    {
        ValidateTargets();
    }

    /// <summary>
    /// Checks targets
    /// </summary>
    private void ValidateTargets()
    {
        Dictionary<int, Target> dictOfTargets = GameManager.instance.dataScript.GetDictOfTargets();
        for (int index = 0; index < dictOfTargets.Count; index++)
        {
            Target target = dictOfTargets[index];
            if (target != null)
            {
                //fields
                if (string.IsNullOrEmpty(target.description) == true)
                { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Invalid description field (Null or Empty)", target.name); }
                if (target.activation == null)
                { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Invalid activation field (Null)", target.name); }
                if (target.nodeArc == null)
                { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Invalid NodeArc field (Null)", target.name); }
                if (target.actorArc == null)
                { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Invalid ActorArc field (Null)", target.name); }
                if (target.gear == null)
                { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Invalid Gear field (Null)", target.name); }
                if (target.sprite == null)
                { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Invalid sprite field (Null)", target.name); }
                //Good effects
                if (target.listOfGoodEffects.Count > 0)
                {
                    foreach (Effect effect in target.listOfGoodEffects)
                    {
                        if (effect != null)
                        {
                            //should be duration 'Single'
                            if (effect.duration.name.Equals("Single") == false)
                            { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Good effect \"{1}\" NOT Single", target.name, effect.name); }
                        }
                        else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Good effect (Null)", target.name); }
                    }
                }
                //Bad effects
                if (target.listOfBadEffects.Count > 0)
                {
                    foreach (Effect effect in target.listOfBadEffects)
                    {
                        if (effect != null)
                        {
                            //should be duration 'Single'
                            if (effect.duration.name.Equals("Single") == false)
                            { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Bad effect \"{1}\" NOT Single", target.name, effect.name); }
                        }
                        else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Bad effect (Null)", target.name); }
                    }
                }
                //Fail effects
                if (target.listOfFailEffects.Count > 0)
                {
                    foreach (Effect effect in target.listOfFailEffects)
                    {
                        if (effect != null)
                        {
                            //should be duration 'Single'
                            if (effect.duration.name.Equals("Single") == false)
                            { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Fail effect \"{1}\" NOT Single", target.name, effect.name); }
                        }
                        else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Fail effect (Null)", target.name); }
                    }
                }
                //Ongoing effects
                if (target.OngoingEffect != null)
                {
                    //should be duration 'Ongoing'
                    if (target.OngoingEffect.duration.name.Equals("Ongoing") == false)
                    { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" ongoing effect \"{1}\" NOT Ongoing", target.name, target.OngoingEffect.name); }
                }
            }
            else { Debug.LogErrorFormat("Invalid target (Null) for targetID {0}", index); }
        }
    }

}
