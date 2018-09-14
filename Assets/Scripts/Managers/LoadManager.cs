using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles loading of all design time SO's
/// </summary>
public class LoadManager : MonoBehaviour
{
    [Header("Initialise Start -> Globals")]
    public GlobalMeta[] arrayOfGlobalMeta;
    public GlobalChance[] arrayOfGlobalChance;
    public GlobalType[] arrayOfGlobalType;
    public GlobalSide[] arrayOfGlobalSide;
    public GlobalWho[] arrayOfGlobalWho;

    [Header("InitialiseStart -> Others")]
    public Condition[] arrayOfConditions;
    public TraitCategory[] arrayOfTraitCategories;
    public TraitEffect[] arrayOfTraitEffects;
    public SecretType[] arrayOfSecretTypes;
    public SecretStatus[] arrayOfSecretStatus;
    public NodeDatapoint[] arrayOfNodeDatapoints;

    public void InitialiseStart()
    {
        int numArray, numDict, counter;
        //
        // - - - GlobalMeta - - -
        //
        /*numArray = arrayOfGlobalMeta.Length;
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
            Debug.LogFormat("[Imp] InitialiseStart -> arrayOfGlobalMeta has {0} entries{1}", numArray, "\n");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on GlobalMeta Load -> array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalMeta present"); }*/

        numArray = arrayOfGlobalMeta.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfGlobalMeta has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalMeta present"); }
        //
        // - - - GlobalChance - - -
        //
        /*numArray = arrayOfGlobalChance.Length;
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
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalChance present"); }*/
        numArray = arrayOfGlobalChance.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfGlobalChance has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalChance present"); }
        //
        // - - - GlobalType - - -
        //
        /*numArray = arrayOfGlobalType.Length;
        if (numArray > 0)
        {
            Dictionary<string, GlobalType> dictOfGlobalType = GameManager.instance.dataScript.GetDictOfGlobalType();
            for (int i = 0; i < numArray; i++)
            {
                GlobalType assetSO = arrayOfGlobalType[i];
                //add to dictionary
                try
                { dictOfGlobalType.Add(assetSO.name, assetSO); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid GlobalType (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid GlobalType (duplicate) \"{0}\"", assetSO.name)); }
            }
            numDict = dictOfGlobalType.Count;
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfGlobalType has {0} entries{1}", numDict, "\n");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on GlobalType Load -> array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalType present"); }*/
        numArray = arrayOfGlobalType.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfGlobalType has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalType present"); }
        //
        // - - - GlobalSide - - -
        //
        /*numArray = arrayOfGlobalSide.Length;
        if (numArray > 0)
        {
            Dictionary<string, GlobalSide> dictOfGlobalSide = GameManager.instance.dataScript.GetDictOfGlobalSide();
            for (int i = 0; i < numArray; i++)
            {
                GlobalSide assetSO = arrayOfGlobalSide[i];
                //add to dictionary
                try
                { dictOfGlobalSide.Add(assetSO.name, assetSO); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid GlobalSide (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid GlobalSide (duplicate) \"{0}\"", assetSO.name)); }
            }
            numDict = dictOfGlobalSide.Count;
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfGlobalSide has {0} entries{1}", numDict, "\n");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on GlobalSide Load -> array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalSide present"); }*/
        numArray = arrayOfGlobalSide.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfGlobalSide has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalSide present"); }
        //
        // - - - GlobalWho - - -
        //
        /*numArray = arrayOfGlobalWho.Length;
        if (numArray > 0)
        {
            Dictionary<string, GlobalWho> dictOfGlobalWho = GameManager.instance.dataScript.GetDictOfGlobalWho();
            for (int i = 0; i < numArray; i++)
            {
                GlobalWho assetSO = arrayOfGlobalWho[i];
                //add to dictionary
                try
                { dictOfGlobalWho.Add(assetSO.name, assetSO); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid GlobalWho (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid GlobalWho (duplicate) \"{0}\"", assetSO.name)); }
            }
            numDict = dictOfGlobalWho.Count;
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfGlobalWho has {0} entries{1}", numDict, "\n");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on GlobalWho Load -> array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalWho present"); }*/
        numArray = arrayOfGlobalWho.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfGlobalWho has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalWho present"); }
        //
        // - - - Conditions - - -
        //
        numArray = arrayOfConditions.Length;
        if (numArray > 0)
        {
            Dictionary<string, Condition> dictOfConditions = GameManager.instance.dataScript.GetDictOfConditions();
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                Condition condition = arrayOfConditions[i];
                //add to dictionary
                try
                { dictOfConditions.Add(condition.name, condition); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Condition (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Condition (duplicate) \"{0}\"", condition.name)); }
            }
            numDict = dictOfConditions.Count;
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfConditions has {0} entries{1}", numDict, "\n");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on Condition Load -> array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No Conditions present"); }
        //
        // - - - TraitCategories - - -
        //
        numArray = arrayOfTraitCategories.Length;
        Dictionary<string, TraitCategory> dictOfTraitCategories = GameManager.instance.dataScript.GetDictOfTraitCategories();
        if (numArray > 0)
        {
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                TraitCategory category = arrayOfTraitCategories[i];
                //add to dictionary
                try
                { dictOfTraitCategories.Add(category.name, category); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid trait category (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid trait category (duplicate) \"{0}\"", category.name)); }
            }
            numDict = dictOfTraitCategories.Count;
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfTraitCategories has {0} entries{1}", numDict, "\n");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on TraitCategory Load -> array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No TraitCategories present"); }
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
                numArray = arrayOfTraitEffects.Length;
                if (numArray > 0)
                {
                    for (int i = 0; i < numArray; i++)
                    {
                        //assign a zero based unique ID number
                        TraitEffect traitEffect = arrayOfTraitEffects[i];
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
                }
                numDict = dictOfTraitEffects.Count;
                Debug.LogFormat("[Imp] InitialiseStart -> dictOfTraitEffects has {0} entries{1}", numDict, "\n");
                Debug.LogFormat("[Imp] InitialiseStart -> dictOfLookUpTraitEffects has {0} entries{1}", dictOfLookUpTraitEffects.Count, "\n");
                Debug.Assert(dictOfTraitEffects.Count == counter, "Mismatch on count");
                Debug.Assert(dictOfLookUpTraitEffects.Count == counter, "Mismatch on count");
                Debug.Assert(dictOfTraitEffects.Count > 0, "No Trait Effects imported to dictionary");
                Debug.Assert(dictOfLookUpTraitEffects.Count > 0, "No Trait Effects in Lookup dictionary");
                Debug.Assert(numArray == numDict, string.Format("Mismatch in TraitEffect count, array {0}, dict {1}", numArray, numDict));
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
            numArray = arrayOfSecretTypes.Length;
            if (numArray > 0)
            {
                for (int i = 0; i < numArray; i++)
                {
                    SecretType secretType = arrayOfSecretTypes[i];
                    //add to dictionary
                    try
                    { dictOfSecretTypes.Add(secretType.name, secretType); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid Secret Type (Null)"); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Invalid SecretType (duplicate) \"{0}\"", secretType.name); }
                }
            }
            numDict = dictOfSecretTypes.Count;
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfSecretTypes has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict > 0, "No SecretTypes in dictOfSecretTypes");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on SecretType count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfecretTypes (Null) -> Import failed"); }
        //
        // - - - Secret Status - - -
        //
        Dictionary<string, SecretStatus> dictOfSecretStatus = GameManager.instance.dataScript.GetDictOfSecretStatus();
        if (dictOfSecretStatus != null)
        {
            numArray = arrayOfSecretStatus.Length;
            if (numArray > 0)
            {
                for (int i = 0; i < numArray; i++)
                {
                    SecretStatus secretStatus = arrayOfSecretStatus[i];
                    //add to dictionary
                    try
                    { dictOfSecretStatus.Add(secretStatus.name, secretStatus); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid Secret Status (Null)"); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Invalid SecretStatus (duplicate) \"{0}\"", secretStatus.name); }
                }
            }
            numDict = dictOfSecretStatus.Count;
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfSecretStatus has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict > 0, "No SecretStatus in dictOfSecretStatus");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on SecretStatus count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfSecretStatus (Null) -> Import failed"); }

        
        //
        // - - - Node Datapoints - - -
        //

        /*Dictionary<string, NodeDatapoint> dictOfNodeDatapoints = GameManager.instance.dataScript.GetDictOfNodeDatapoints();
        if (dictOfNodeDatapoints != null)
        {
            numArray = arrayOfSecretStatus.Length;
            if (numArray > 0)
            {
                for (int i = 0; i < numArray; i++)
                {

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
            }
            Debug.LogFormat("[Imp] InitialiseStart -> dictOfNodeDatapoints has {0} entries{1}", dictOfNodeDatapoints.Count, "\n");
            Debug.Assert(dictOfNodeDatapoints.Count > 0, "No datapoints in dictOfNodeDatapoints");
        }
        else { Debug.LogError("Invalid dictOfNodeDatapoints (Null) -> Import failed"); }*/

        numArray = arrayOfNodeDatapoints.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfNodeDatapoints has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No NodeDatapoints present"); }
    }

}
