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
        //
        // - - - EffectCriteria - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:EffectCriteria", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfEffectCriteria.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on EffectCriteria SO, array {0}, assets {1} records", numArray, numAssets);
            EffectCriteria[] arrayTemp = GameManager.instance.loadScript.arrayOfEffectCriteria;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(EffectCriteria));
                //get object
                EffectCriteria meta = metaObject as EffectCriteria;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING EffectCriteria \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on EffectCriteria SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - EffectDuration - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:EffectDuration", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfEffectDuration.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on EffectDuration SO, array {0}, assets {1} records", numArray, numAssets);
            EffectDuration[] arrayTemp = GameManager.instance.loadScript.arrayOfEffectDuration;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(EffectDuration));
                //get object
                EffectDuration meta = metaObject as EffectDuration;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING EffectDuration \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on EffectDuration SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - EffectOperator - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:EffectOperator", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfEffectOperator.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on EffectOperator SO, array {0}, assets {1} records", numArray, numAssets);
            EffectOperator[] arrayTemp = GameManager.instance.loadScript.arrayOfEffectOperator;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(EffectOperator));
                //get object
                EffectOperator meta = metaObject as EffectOperator;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING EffectOperator \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on EffectOperator SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - ContactType - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:ContactType", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfContactTypes.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on ContactType SO, array {0}, assets {1} records", numArray, numAssets);
            ContactType[] arrayTemp = GameManager.instance.loadScript.arrayOfContactTypes;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(ContactType));
                //get object
                ContactType meta = metaObject as ContactType;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING ContactType \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on ContactType SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - TargetType - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:TargetType", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfTargetTypes.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on TargetType SO, array {0}, assets {1} records", numArray, numAssets);
            TargetType[] arrayTemp = GameManager.instance.loadScript.arrayOfTargetTypes;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(TargetType));
                //get object
                TargetType meta = metaObject as TargetType;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING TargetType \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on TargetType SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Quality - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Quality", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfQualities.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Quality SO, array {0}, assets {1} records", numArray, numAssets);
            Quality[] arrayTemp = GameManager.instance.loadScript.arrayOfQualities;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Quality));
                //get object
                Quality meta = metaObject as Quality;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Quality \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Quality SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Condition - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Condition", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfConditions.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Condition SO, array {0}, assets {1} records", numArray, numAssets);
            Condition[] arrayTemp = GameManager.instance.loadScript.arrayOfConditions;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Condition));
                //get object
                Condition meta = metaObject as Condition;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Condition \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Condition SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - TraitCategory - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:TraitCategory", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfTraitCategories.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on TraitCategory SO, array {0}, assets {1} records", numArray, numAssets);
            TraitCategory[] arrayTemp = GameManager.instance.loadScript.arrayOfTraitCategories;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(TraitCategory));
                //get object
                TraitCategory meta = metaObject as TraitCategory;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING TraitCategory \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on TraitCategory SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - TraitEffect - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:TraitEffect", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfTraitEffects.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on TraitEffect SO, array {0}, assets {1} records", numArray, numAssets);
            TraitEffect[] arrayTemp = GameManager.instance.loadScript.arrayOfTraitEffects;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(TraitEffect));
                //get object
                TraitEffect meta = metaObject as TraitEffect;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING TraitEffect \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on TraitEffect SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - SecretType - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:SecretType", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfSecretTypes.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on SecretType SO, array {0}, assets {1} records", numArray, numAssets);
            SecretType[] arrayTemp = GameManager.instance.loadScript.arrayOfSecretTypes;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(SecretType));
                //get object
                SecretType meta = metaObject as SecretType;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING SecretType \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on SecretType SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - SecretStatus - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:SecretStatus", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfSecretStatus.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on SecretStatus SO, array {0}, assets {1} records", numArray, numAssets);
            SecretStatus[] arrayTemp = GameManager.instance.loadScript.arrayOfSecretStatus;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(SecretStatus));
                //get object
                SecretStatus meta = metaObject as SecretStatus;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING SecretStatus \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on SecretStatus SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - CitySize - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:CitySize", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfCitySize.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on CitySize SO, array {0}, assets {1} records", numArray, numAssets);
            CitySize[] arrayTemp = GameManager.instance.loadScript.arrayOfCitySize;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(CitySize));
                //get object
                CitySize meta = metaObject as CitySize;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING CitySize \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on CitySize SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - CitySpacing - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:CitySpacing", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfCitySpacing.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on CitySpacing SO, array {0}, assets {1} records", numArray, numAssets);
            CitySpacing[] arrayTemp = GameManager.instance.loadScript.arrayOfCitySpacing;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(CitySpacing));
                //get object
                CitySpacing meta = metaObject as CitySpacing;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING CitySpacing \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on CitySpacing SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - CityConnections - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:CityConnections", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfCityConnections.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on CityConnection SO, array {0}, assets {1} records", numArray, numAssets);
            CityConnections[] arrayTemp = GameManager.instance.loadScript.arrayOfCityConnections;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(CityConnections));
                //get object
                CityConnections meta = metaObject as CityConnections;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING CityConnections \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on CityConnections SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - CitySecurity - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:CitySecurity", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfCitySecurity.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on CityConnection SO, array {0}, assets {1} records", numArray, numAssets);
            CitySecurity[] arrayTemp = GameManager.instance.loadScript.arrayOfCitySecurity;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(CitySecurity));
                //get object
                CitySecurity meta = metaObject as CitySecurity;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING CitySecurity \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on CitySecurity SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - TextList - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:TextList", new[] { "Assets/SO" });
        
        //combine all text list arrays into a single list for validation checks
        List<TextList> listOfAllTextLists = new List<TextList>();
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfContactTextLists);
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfNameTextLists);
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfDistrictTextLists);
        //NOTE: add extra text lists here (as above)
        numArray = listOfAllTextLists.Count;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on TextList SO, array {0}, assets {1} records", numArray, numAssets);
            //convert to array
            TextList[] arrayTemp = listOfAllTextLists.ToArray();
            
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(TextList));
                //get object
                TextList meta = metaObject as TextList;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING TextList \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on TextList SO, array {0}, assets {1} records", numArray, numAssets); }        
        //
        // - - - NameSets - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:NameSet", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfNameSets.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on NameSet SO, array {0}, assets {1} records", numArray, numAssets);
            NameSet[] arrayTemp = GameManager.instance.loadScript.arrayOfNameSets;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(NameSet));
                //get object
                NameSet meta = metaObject as NameSet;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING NameSet \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on NameSet SO, array {0}, assets {1} records", numArray, numAssets); }

        //
        // - - - NodeDatapoint - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:NodeDatapoint", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfNodeDatapoints.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on NodeDatapoint SO, array {0}, assets {1} records", numArray, numAssets);
            NodeDatapoint[] arrayTemp = GameManager.instance.loadScript.arrayOfNodeDatapoints;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(NodeDatapoint));
                //get object
                NodeDatapoint meta = metaObject as NodeDatapoint;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING NodeDatapoint \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on NodeDatapoint SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - NodeArc - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:NodeArc", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfNodeArcs.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on NodeArc SO, array {0}, assets {1} records", numArray, numAssets);
            NodeArc[] arrayTemp = GameManager.instance.loadScript.arrayOfNodeArcs;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(NodeArc));
                //get object
                NodeArc meta = metaObject as NodeArc;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING NodeArc \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on NodeArc SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - NodeCrisis - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:NodeCrisis", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfNodeCrisis.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on NodeCrisis SO, array {0}, assets {1} records", numArray, numAssets);
            NodeCrisis[] arrayTemp = GameManager.instance.loadScript.arrayOfNodeCrisis;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(NodeCrisis));
                //get object
                NodeCrisis meta = metaObject as NodeCrisis;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING NodeCrisis \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on NodeCrisis SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Trait - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Trait", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfTraits.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Trait SO, array {0}, assets {1} records", numArray, numAssets);
            Trait[] arrayTemp = GameManager.instance.loadScript.arrayOfTraits;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Trait));
                //get object
                Trait meta = metaObject as Trait;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Trait \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Trait SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - ActorArc - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:ActorArc", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfActorArcs.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on ActorArc SO, array {0}, assets {1} records", numArray, numAssets);
            ActorArc[] arrayTemp = GameManager.instance.loadScript.arrayOfActorArcs;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(ActorArc));
                //get object
                ActorArc meta = metaObject as ActorArc;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING ActorArc \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on ActorArc SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Effect - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Effect", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfEffects.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Effect SO, array {0}, assets {1} records", numArray, numAssets);
            Effect[] arrayTemp = GameManager.instance.loadScript.arrayOfEffects;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Effect));
                //get object
                Effect meta = metaObject as Effect;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Effect \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Effect SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Target - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Target", new[] { "Assets/SO" });
        List<Target> listOfTargets = new List<Target>();
        listOfTargets.AddRange(GameManager.instance.loadScript.arrayOfTargetsGeneric);
        listOfTargets.AddRange(GameManager.instance.loadScript.arrayOfTargetsCity);
        listOfTargets.AddRange(GameManager.instance.loadScript.arrayOfTargetsVIP);
        listOfTargets.AddRange(GameManager.instance.loadScript.arrayOfTargetsStory);
        listOfTargets.AddRange(GameManager.instance.loadScript.arrayOfTargetsGoal);
        numArray = listOfTargets.Count;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Target SO, array {0}, assets {1} records", numArray, numAssets);
            /*Target[] arrayTemp = GameManager.instance.loadScript.arrayOfTargets;*/
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Target));
                //get object
                Target meta = metaObject as Target;
                if (listOfTargets.Exists( element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Target \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Target SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Action - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Action", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfActions.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Action SO, array {0}, assets {1} records", numArray, numAssets);
            Action[] arrayTemp = GameManager.instance.loadScript.arrayOfActions;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Action));
                //get object
                Action meta = metaObject as Action;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Action \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Action SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - TeamArc - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:TeamArc", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfTeamArcs.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on TeamArc SO, array {0}, assets {1} records", numArray, numAssets);
            TeamArc[] arrayTemp = GameManager.instance.loadScript.arrayOfTeamArcs;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(TeamArc));
                //get object
                TeamArc meta = metaObject as TeamArc;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING TeamArc \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on TeamArc SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Gear - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Gear", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfGear.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Gear SO, array {0}, assets {1} records", numArray, numAssets);
            Gear[] arrayTemp = GameManager.instance.loadScript.arrayOfGear;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Gear));
                //get object
                Gear meta = metaObject as Gear;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Gear \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Gear SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - GearRarity - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:GearRarity", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfGearRarity.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on GearRarity SO, array {0}, assets {1} records", numArray, numAssets);
            GearRarity[] arrayTemp = GameManager.instance.loadScript.arrayOfGearRarity;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(GearRarity));
                //get object
                GearRarity meta = metaObject as GearRarity;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING GearRarity \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on GearRarity SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - GearType - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:GearType", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfGearType.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on GearType SO, array {0}, assets {1} records", numArray, numAssets);
            GearType[] arrayTemp = GameManager.instance.loadScript.arrayOfGearType;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(GearType));
                //get object
                GearType meta = metaObject as GearType;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING GearType \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on GearType SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - ManageActor - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:ManageActor", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfManageActors.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on ManageActor SO, array {0}, assets {1} records", numArray, numAssets);
            ManageActor[] arrayTemp = GameManager.instance.loadScript.arrayOfManageActors;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(ManageActor));
                //get object
                ManageActor meta = metaObject as ManageActor;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING ManageActor \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on ManageActor SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - ManageAction - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:ManageAction", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfManageActions.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on ManageAction SO, array {0}, assets {1} records", numArray, numAssets);
            ManageAction[] arrayTemp = GameManager.instance.loadScript.arrayOfManageActions;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(ManageAction));
                //get object
                ManageAction meta = metaObject as ManageAction;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING ManageAction \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on ManageAction SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - ActorConflict - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:ActorConflict", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfActorConflicts.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on ActorConflict SO, array {0}, assets {1} records", numArray, numAssets);
            ActorConflict[] arrayTemp = GameManager.instance.loadScript.arrayOfActorConflicts;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(ActorConflict));
                //get object
                ActorConflict meta = metaObject as ActorConflict;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING ActorConflict \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on ActorConflict SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Secret - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Secret", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfSecrets.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Secret SO, array {0}, assets {1} records", numArray, numAssets);
            Secret[] arrayTemp = GameManager.instance.loadScript.arrayOfSecrets;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Secret));
                //get object
                Secret meta = metaObject as Secret;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Secret \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Secret SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Faction - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Faction", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfFactions.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Faction SO, array {0}, assets {1} records", numArray, numAssets);
            Faction[] arrayTemp = GameManager.instance.loadScript.arrayOfFactions;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Faction));
                //get object
                Faction meta = metaObject as Faction;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Faction \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Faction SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - CityArc - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:CityArc", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfCityArcs.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on CityArc SO, array {0}, assets {1} records", numArray, numAssets);
            CityArc[] arrayTemp = GameManager.instance.loadScript.arrayOfCityArcs;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(CityArc));
                //get object
                CityArc meta = metaObject as CityArc;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING CityArc \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on CityArc SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - City - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:City", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfCities.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on City SO, array {0}, assets {1} records", numArray, numAssets);
            City[] arrayTemp = GameManager.instance.loadScript.arrayOfCities;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(City));
                //get object
                City meta = metaObject as City;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING City \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on City SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Objective - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Objective", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfObjectives.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Objective SO, array {0}, assets {1} records", numArray, numAssets);
            Objective[] arrayTemp = GameManager.instance.loadScript.arrayOfObjectives;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Objective));
                //get object
                Objective meta = metaObject as Objective;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Objective \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Objective SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Organisation - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Organisation", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfOrganisations.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Organisation SO, array {0}, assets {1} records", numArray, numAssets);
            Organisation[] arrayTemp = GameManager.instance.loadScript.arrayOfOrganisations;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Organisation));
                //get object
                Organisation meta = metaObject as Organisation;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Organisation \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Organisation SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Mayor - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Mayor", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfMayors.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Mayor SO, array {0}, assets {1} records", numArray, numAssets);
            Mayor[] arrayTemp = GameManager.instance.loadScript.arrayOfMayors;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Mayor));
                //get object
                Mayor meta = metaObject as Mayor;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Mayor \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Mayor SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - DecisionAI - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:DecisionAI", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfDecisionAI.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on DecisionAI SO, array {0}, assets {1} records", numArray, numAssets);
            DecisionAI[] arrayTemp = GameManager.instance.loadScript.arrayOfDecisionAI;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(DecisionAI));
                //get object
                DecisionAI meta = metaObject as DecisionAI;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING DecisionAI \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on DecisionAI SO, array {0}, assets {1} records", numArray, numAssets); }
        //
        // - - - Mission - - -
        //
        metaGUID = AssetDatabase.FindAssets("t:Mission", new[] { "Assets/SO" });
        numArray = GameManager.instance.loadScript.arrayOfMissions.Length;
        numAssets = metaGUID.Length;
        if (numAssets != numArray)
        {
            Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on Mission SO, array {0}, assets {1} records", numArray, numAssets);
            Mission[] arrayTemp = GameManager.instance.loadScript.arrayOfMissions;
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(Mission));
                //get object
                Mission meta = metaObject as Mission;
                if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
                { Debug.LogFormat("[Val] ValidateSO: array MISSING Mission \"{0}\"", meta.name); }
            }
        }
        else { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on Mission SO, array {0}, assets {1} records", numArray, numAssets); }

    }


}
