using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
        ValidateGear();
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

    /// <summary>
    /// Checks Gear
    /// </summary>
    private void ValidateGear()
    {
        Dictionary<int, Gear> dictOfGear = GameManager.instance.dataScript.GetDictOfGear();
        if (dictOfGear != null)
        {
            foreach (var gear in dictOfGear)
            {
                if (gear.Value != null)
                {
                    //check any effect
                    if (gear.Value.listOfPersonalEffects.Count > 0)
                    {
                        //personal effects
                        foreach(var effect in gear.Value.listOfPersonalEffects)
                        {
                            if (effect != null)
                            {
                                //Can't be ONGOING
                                if (effect.duration.name.Equals("Ongoing") == true)
                                { Debug.LogFormat("[Val] ValidateGear: Gear \"{0}\"  invalid Personal effect (ONGOING)", gear.Value.name); }
                            }
                            else { Debug.LogFormat("[Val] ValidateGear: Gear \"{0}\"  invalid effect (Null)", gear.Value.name); }
                        }
                        //AI hacking effects
                        if (gear.Value.aiHackingEffect != null)
                        {
                            //Can't be ONGOING
                            if (gear.Value.aiHackingEffect.duration.name.Equals("Ongoing") == true)
                            { Debug.LogFormat("[Val] ValidateGear: Gear \"{0}\"  invalid aiHacking effect (ONGOING)", gear.Value.name); }
                        }
                    }
                }
                else{ Debug.LogFormat("[Val] ValidateGar: Invalid Gear (Null) for gearID {0}", gear.Key); }
            }
        }
    }


    /// <summary>
    /// optional (GameManager.cs toggle) program to run to check SO's loaded in LoadManager.cs arrays vs. those found by an Asset search (editor only)
    /// Designed to pick up SO's that might have been added in the editor but not added to the arrays (ignored by game if this is the case).
    /// </summary>
    public void ValidateSO()
    {
        int numArray, numAssets;
        string path;
        //
        // - - - GlobalMeta - - -
        //
        var metaGUID = AssetDatabase.FindAssets("t:GlobalMeta", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfGlobalMeta.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on GlobalMeta SO, array {0}, assets {1} records", numArray, numAssets);
            GlobalMeta[] arrayTemp = GameManager.instance.loadScript.arrayOfGlobalMeta;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalMeta));
                //get object
                GlobalMeta meta = metaObject as GlobalMeta;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING GlobalMeta \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on GlobalMeta SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - GlobalChance - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:GlobalChance", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfGlobalChance.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on GlobalChance SO, array {0}, assets {1} records", numArray, numAssets);
            GlobalChance[] arrayTemp = GameManager.instance.loadScript.arrayOfGlobalChance;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalChance));
                //get object
                GlobalChance meta = metaObject as GlobalChance;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING GlobalChance \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on GlobalChance SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - GlobalType - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:GlobalType", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfGlobalType.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on GlobalType SO, array {0}, assets {1} records", numArray, numAssets);
            GlobalType[] arrayTemp = GameManager.instance.loadScript.arrayOfGlobalType;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalType));
                //get object
                GlobalType meta = metaObject as GlobalType;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING GlobalType \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on GlobalType SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - GlobalSide - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:GlobalSide", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfGlobalSide.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on GlobalSide SO, array {0}, assets {1} records", numArray, numAssets);
            GlobalSide[] arrayTemp = GameManager.instance.loadScript.arrayOfGlobalSide;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalSide));
                //get object
                GlobalSide meta = metaObject as GlobalSide;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING GlobalSide \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on GlobalSide SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - GlobalWho - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:GlobalWho", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfGlobalWho.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on GlobalWho SO, array {0}, assets {1} records", numArray, numAssets);
            GlobalWho[] arrayTemp = GameManager.instance.loadScript.arrayOfGlobalWho;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalWho));
                //get object
                GlobalWho meta = metaObject as GlobalWho;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING GlobalWho \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on GlobalWho SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - EffectApply - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:EffectApply", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfEffectApply.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on EffectApply SO, array {0}, assets {1} records", numArray, numAssets);
            EffectApply[] arrayTemp = GameManager.instance.loadScript.arrayOfEffectApply;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(EffectApply));
                //get object
                EffectApply meta = metaObject as EffectApply;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING EffectApply \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on EffectApply SO, array {0}, assets {1} records", numArray, numAssets); }
    }


}
