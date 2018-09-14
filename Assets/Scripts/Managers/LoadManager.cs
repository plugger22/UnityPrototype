using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles loading of all design time SO's
/// </summary>
public class LoadManager : MonoBehaviour
{
    [Header("Initialise Start")]
    public GlobalMeta[] arrayOfGlobalMeta;
    public GlobalChance[] arrayOfGlobalChance;
    public GlobalType[] arrayOfGlobalType;
    public GlobalSide[] arrayOfGlobalSide;
    public GlobalWho[] arrayOfGlobalWho;

    public void InitialiseStart()
    {
        int numArray, numDict;
        //
        // - - - GlobalMeta - - -
        //
        numArray = arrayOfGlobalMeta.Length;
        if (numArray > 0)
        {
            Dictionary<string, GlobalMeta> dictOfGlobalMeta = GameManager.instance.dataScript.GetDictOfGlobalMeta();
            for (int i = 0; i < numArray; i++)
            {
                GlobalMeta assetSO = arrayOfGlobalMeta[i];
                //add to dictionary
                try
                { dictOfGlobalMeta.Add(assetSO.name, assetSO); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid GlobalMeta (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid GlobalMeta (duplicate) \"{0}\"", assetSO.name)); }
            }
            numDict = dictOfGlobalMeta.Count;
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfGlobalMeta has {0} entries{1}", numDict, "\n");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on GlobalMeta Load -> array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalMeta present"); }
        //
        // - - - GlobalChance - - -
        //
        numArray = arrayOfGlobalChance.Length;
        if (numArray > 0)
        {
            Dictionary<string, GlobalChance> dictOfGlobalChance = GameManager.instance.dataScript.GetDictOfGlobalChance();
            for (int i = 0; i < numArray; i++)
            {
                GlobalChance assetSO = arrayOfGlobalChance[i];
                //add to dictionary
                try
                { dictOfGlobalChance.Add(assetSO.name, assetSO); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid GlobalChance (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid GlobalChance (duplicate) \"{0}\"", assetSO.name)); }
            }
            numDict = dictOfGlobalChance.Count;
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfGlobalChance has {0} entries{1}", numDict, "\n");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on GlobalChance Load -> array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalChance present"); }
        //
        // - - - GlobalType - - -
        //

        //
        // - - - GlobalSide - - -
        //

        //
        // - - - GlobalWho - - -
        //

        /*
        //
        // - - - Conditions - - -
        //
        Dictionary<string, Condition> dictOfConditions = GameManager.instance.dataScript.GetDictOfConditions();
        if (dictOfConditions != null)
        {
            var conditionGUID = AssetDatabase.FindAssets("t:Condition", new[] { "Assets/SO" });
            foreach (var guid in conditionGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object conditionObject = AssetDatabase.LoadAssetAtPath(path, typeof(Condition));
                //assign a zero based unique ID number
                Condition condition = conditionObject as Condition;
                //add to dictionary
                try
                { dictOfConditions.Add(condition.name, condition); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Condition (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Condition (duplicate) \"{0}\"", condition.name)); }
                Debug.Assert(dictOfGlobalMeta.Count > 0, "No conditions in dictOfConditions");
            }
            Debug.Log(string.Format("[Imp] InitialiseStart -> dictOfConditions has {0} entries{1}", dictOfConditions.Count, "\n"));
        }
        else { Debug.LogError("Invalid dictOfConditions (Null) -> Import failed"); }
        //
        // - - - TraitCategories - - -
        //
        Dictionary<string, TraitCategory> dictOfTraitCategories = GameManager.instance.dataScript.GetDictOfTraitCategories();
        if (dictOfTraitCategories != null)
        {
            var categoryGUID = AssetDatabase.FindAssets("t:TraitCategory", new[] { "Assets/SO" });
            foreach (var guid in categoryGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object categoryObject = AssetDatabase.LoadAssetAtPath(path, typeof(TraitCategory));
                //assign a zero based unique ID number
                TraitCategory category = categoryObject as TraitCategory;
                //add to dictionary
                try
                { dictOfTraitCategories.Add(category.name, category); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid trait category (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid trait category (duplicate) \"{0}\"", category.name)); }
            }
            Debug.Log(string.Format("[Imp] InitialiseStart -> dictOfTraitCategories has {0} entries{1}", dictOfTraitCategories.Count, "\n"));
            Debug.Assert(dictOfTraitCategories.Count > 0, "No Trait categories in dictOfTraitCategories");
        }
        else { Debug.LogError("Invalid dictOfTraitCategories (Null) -> Import failed"); }
        //
        // - - - TraitEffects - - -
        //
        Dictionary<int, TraitEffect> dictOfTraitEffects = GameManager.instance.dataScript.GetDictOfTraitEffects();
        Dictionary<string, int> dictOfLookUpTraitEffects = GameManager.instance.dataScript.GetDictOfLookUpTraitEffects();
        if (dictOfTraitCategories != null)
        {
            if (dictOfLookUpTraitEffects != null)
            {
                counter = 0;
                var traitEffectGUID = AssetDatabase.FindAssets("t:TraitEffect", new[] { "Assets/SO" });
                foreach (var guid in traitEffectGUID)
                {
                    //get path
                    path = AssetDatabase.GUIDToAssetPath(guid);
                    //get SO
                    UnityEngine.Object traitEffectObject = AssetDatabase.LoadAssetAtPath(path, typeof(TraitEffect));
                    //assign a zero based unique ID number
                    TraitEffect traitEffect = traitEffectObject as TraitEffect;
                    traitEffect.teffID = counter;
                    counter++;
                    //add to dictionaries
                    try
                    { dictOfTraitEffects.Add(traitEffect.teffID, traitEffect); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid trait Effect (Null)"); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Invalid trait Effect (duplicate ID) \"{0}\"", traitEffect.name); }
                    try
                    { dictOfLookUpTraitEffects.Add(traitEffect.name, traitEffect.teffID); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid traitEffect.name (Null)"); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Invalid traitEffect.name (duplicate) \"{0}\"", traitEffect.name); }
                }
                Debug.LogFormat("[Imp] InitialiseStart -> dictOfTraitEffects has {0} entries{1}", dictOfTraitEffects.Count, "\n");
                Debug.LogFormat("[Imp] InitialiseStart -> dictOfLookUpTraitEffects has {0} entries{1}", dictOfLookUpTraitEffects.Count, "\n");
                Debug.Assert(dictOfTraitEffects.Count == counter, "Mismatch on count");
                Debug.Assert(dictOfLookUpTraitEffects.Count == counter, "Mismatch on count");
                Debug.Assert(dictOfTraitEffects.Count > 0, "No Trait Effects imported to dictionary");
                Debug.Assert(dictOfLookUpTraitEffects.Count > 0, "No Trait Effects in Lookup dictionary");
            }
            else { Debug.LogError("Invalid dictOfLookUpTraitEffects (Null) -> Import failed"); }
        }
        else { Debug.LogError("Invalid dictOfTraitEffects (Null) -> Import failed"); }
        //
        // - - - Secret Type - - -
        //
        Dictionary<string, SecretType> dictOfSecretTypes = GameManager.instance.dataScript.GetDictOfSecretTypes();
        if (dictOfSecretTypes != null)
        {
            var secretTypeGUID = AssetDatabase.FindAssets("t:SecretType", new[] { "Assets/SO" });
            foreach (var guid in secretTypeGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object secretTypeObject = AssetDatabase.LoadAssetAtPath(path, typeof(SecretType));
                //assign a zero based unique ID number
                SecretType secretType = secretTypeObject as SecretType;
                //add to dictionary
                try
                { dictOfSecretTypes.Add(secretType.name, secretType); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Secret Type (Null)"); }
                catch (ArgumentException)
                { Debug.LogErrorFormat("Invalid SecretType (duplicate) \"{0}\"", secretType.name); }
            }
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfSecretTypes has {0} entries{1}", dictOfSecretTypes.Count, "\n");
            Debug.Assert(dictOfSecretTypes.Count > 0, "No SecretTypes in dictOfSecretTypes");
        }
        else { Debug.LogError("Invalid dictOfecretTypes (Null) -> Import failed"); }
        //
        // - - - Secret Status - - -
        //
        Dictionary<string, SecretStatus> dictOfSecretStatus = GameManager.instance.dataScript.GetDictOfSecretStatus();
        if (dictOfSecretStatus != null)
        {
            var secretStatusGUID = AssetDatabase.FindAssets("t:SecretStatus", new[] { "Assets/SO" });
            foreach (var guid in secretStatusGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object secretStatusObject = AssetDatabase.LoadAssetAtPath(path, typeof(SecretStatus));
                //assign a zero based unique ID number
                SecretStatus secretStatus = secretStatusObject as SecretStatus;
                //add to dictionary
                try
                { dictOfSecretStatus.Add(secretStatus.name, secretStatus); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Secret Status (Null)"); }
                catch (ArgumentException)
                { Debug.LogErrorFormat("Invalid SecretStatus (duplicate) \"{0}\"", secretStatus.name); }
            }
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfSecretStatus has {0} entries{1}", dictOfSecretStatus.Count, "\n");
            Debug.Assert(dictOfSecretStatus.Count > 0, "No SecretStatus in dictOfSecretStatus");
        }
        else { Debug.LogError("Invalid dictOfSecretStatus (Null) -> Import failed"); }
        //
        // - - - Node Datapoints - - -
        //
        Dictionary<string, NodeDatapoint> dictOfNodeDatapoints = GameManager.instance.dataScript.GetDictOfNodeDatapoints();
        if (dictOfNodeDatapoints != null)
        {
            var datapointGUID = AssetDatabase.FindAssets("t:NodeDatapoint", new[] { "Assets/SO" });
            foreach (var guid in datapointGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object datapointObject = AssetDatabase.LoadAssetAtPath(path, typeof(NodeDatapoint));
                //assign a zero based unique ID number
                NodeDatapoint datapoint = datapointObject as NodeDatapoint;
                //add to dictionary
                try
                { dictOfNodeDatapoints.Add(datapoint.name, datapoint); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Node Datapoint (Null)"); }
                catch (ArgumentException)
                { Debug.LogErrorFormat("Invalid Node Datapoint (duplicate) \"{0}\"", datapoint.name); }
            }
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfNodeDatapoints has {0} entries{1}", dictOfNodeDatapoints.Count, "\n");
            Debug.Assert(dictOfNodeDatapoints.Count > 0, "No datapoints in dictOfNodeDatapoints");
        }
        else { Debug.LogError("Invalid dictOfNodeDatapoints (Null) -> Import failed"); }
        */
    }

}
