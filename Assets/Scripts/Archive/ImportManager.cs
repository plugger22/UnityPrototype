using gameAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// handles all data import related matters
/// </summary>
public class ImportManager : MonoBehaviour
{





    /*public void InitialiseStart()
    {
        int counter;
        string path;

        //
        // - - - GlobalMeta - - -
        //
        Dictionary<string, GlobalMeta> dictOfGlobalMeta = GameManager.instance.dataScript.GetDictOfGlobalMeta();
        if (dictOfGlobalMeta != null)
        {
            var metaGUID = AssetDatabase.FindAssets("t:GlobalMeta", new[] {"Assets/SO"} );
            foreach (var guid in metaGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalMeta));
                //assign a zero based unique ID number
                GlobalMeta meta = metaObject as GlobalMeta;
                //add to dictionary
                try
                { dictOfGlobalMeta.Add(meta.name, meta); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid GlobalMeta (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid GlobalMeta (duplicate) \"{0}\"", meta.name)); }
            }
            Debug.Log(string.Format("[Imp] InitialiseStart -> dictOfGlobalMeta has {0} entries{1}", dictOfGlobalMeta.Count, "\n"));
            Debug.Assert(dictOfGlobalMeta.Count > 0, "No GlobalMeta in dictOfGlobaMeta");
        }
        else { Debug.LogError("Invalid dictOfGlobalMeta (Null) -> Import failed"); }
        //
        // - - - GlobalChance - - -
        //
        Dictionary<string, GlobalChance> dictOfGlobalChance = GameManager.instance.dataScript.GetDictOfGlobalChance();
        if (dictOfGlobalChance != null)
        {
            var chanceGUID = AssetDatabase.FindAssets("t:GlobalChance", new[] { "Assets/SO" });
            foreach (var guid in chanceGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object chanceObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalChance));
                //assign a zero based unique ID number
                GlobalChance chance = chanceObject as GlobalChance;
                //add to dictionary
                try
                { dictOfGlobalChance.Add(chance.name, chance); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid GlobalChance (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid GlobalChance (duplicate) \"{0}\"", chance.name)); }
            }
            Debug.Log(string.Format("[Imp] InitialiseStart -> dictOfGlobalChance has {0} entries{1}", dictOfGlobalChance.Count, "\n"));
            Debug.Assert(dictOfGlobalChance.Count > 0, "No GlobalChance in dictOfGlobalChance");
        }
        else { Debug.LogError("Invalid dictOfGlobalChance (Null) -> Import failed"); }
        //
        // - - - GlobalType - - -
        //
        Dictionary<string, GlobalType> dictOfGlobalType = GameManager.instance.dataScript.GetDictOfGlobalType();
        if (dictOfGlobalType != null)
        {
            var typeGUID = AssetDatabase.FindAssets("t:GlobalType", new[] { "Assets/SO" });
            foreach (var guid in typeGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object typeObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalType));
                //assign a zero based unique ID number
                GlobalType type = typeObject as GlobalType;
                //add to dictionary
                try
                { dictOfGlobalType.Add(type.name, type); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid GlobalType (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid GlobalType (duplicate) \"{0}\"", type.name)); }
                Debug.Assert(dictOfGlobalType.Count > 0, "No GlobalType in dictOfGlobalType");
            }
            Debug.Log(string.Format("[Imp] InitialiseStart -> dictOfGlobalType has {0} entries{1}", dictOfGlobalType.Count, "\n"));
        }
        else { Debug.LogError("Invalid dictOfGlobalType (Null) -> Import failed"); }
        //
        // - - - GlobalSide - - -
        //
        Dictionary<string, GlobalSide> dictOfGlobalSide = GameManager.instance.dataScript.GetDictOfGlobalSide();
        if (dictOfGlobalSide != null)
        {
            var sideGUID = AssetDatabase.FindAssets("t:GlobalSide", new[] { "Assets/SO" });
            foreach (var guid in sideGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object sideObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalSide));
                //assign a zero based unique ID number
                GlobalSide side = sideObject as GlobalSide;
                //add to dictionary
                try
                { dictOfGlobalSide.Add(side.name, side); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid GlobalSide (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid GlobalSide (duplicate) \"{0}\"", side.name)); }
            }
            Debug.Log(string.Format("[Imp] InitialiseStart -> dictOfGlobalSide has {0} entries{1}", dictOfGlobalSide.Count, "\n"));
            Debug.Assert(dictOfGlobalSide.Count > 0, "No GlobalSide in dictOfGlobalSide");
        }
        else { Debug.LogError("Invalid dictOfGlobalSide (Null) -> Import failed"); }
        //
        // - - - GlobalWho - - -
        //
        Dictionary<string, GlobalWho> dictOfGlobalWho = GameManager.instance.dataScript.GetDictOfGlobalWho();
        if (dictOfGlobalWho != null)
        {
            var whoGUID = AssetDatabase.FindAssets("t:GlobalWho", new[] { "Assets/SO" });
            foreach (var guid in whoGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object whoObject = AssetDatabase.LoadAssetAtPath(path, typeof(GlobalWho));
                //assign a zero based unique ID number
                GlobalWho who = whoObject as GlobalWho;
                //add to dictionary
                try
                { dictOfGlobalWho.Add(who.name, who); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid GlobalWho (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid GlobalWho (duplicate) \"{0}\"", who.name)); }
            }
            Debug.Log(string.Format("[Imp] InitialiseStart -> dictOfGlobalWho has {0} entries{1}", dictOfGlobalWho.Count, "\n"));
            Debug.Assert(dictOfGlobalWho.Count > 0, "No GlobalWho in dictOfGlobalWho");
        }
        else { Debug.LogError("Invalid dictOfGlobalWho (Null) -> Import failed"); }

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
                Debug.Assert(dictOfConditions.Count > 0, "No conditions in dictOfConditions");
            }
            Debug.Log(string.Format("[Imp] InitialiseStart -> dictOfConditions has {0} entries{1}", dictOfConditions.Count, "\n"));
        }
        else { Debug.LogError("Invalid dictOfConditions (Null) -> Import failed"); }*/
        //
        // - - - TraitCategories - - -
        //
        /*Dictionary<string, TraitCategory> dictOfTraitCategories = GameManager.instance.dataScript.GetDictOfTraitCategories();
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
        else { Debug.LogError("Invalid dictOfTraitCategories (Null) -> Import failed"); }*/
        //
        // - - - TraitEffects - - -
        //
        /*Dictionary<int, TraitEffect> dictOfTraitEffects = GameManager.instance.dataScript.GetDictOfTraitEffects();
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
    }


    /// <summary>
    /// default main constructor
    /// </summary>
    public void InitialiseEarly()
    {
        int counter = 0;
        int length;
        string path;
        GlobalSide globalAuthority = GameManager.instance.globalScript.sideAuthority;
        GlobalSide globalResistance = GameManager.instance.globalScript.sideResistance;
        //
        // - - - Node Arcs - - -
        //
        Dictionary<int, NodeArc> dictOfNodeArcs = GameManager.instance.dataScript.GetDictOfNodeArcs();
        Dictionary<string, int> dictOfLookUpNodeArcs = GameManager.instance.dataScript.GetDictOfLookUpNodeArcs();
        if (dictOfNodeArcs != null)
        {
            if (dictOfLookUpNodeArcs != null)
            {
                counter = 0;
                //get GUID of all SO Node Objects -> Note that I'm searching the entire database here so it's not folder dependant
                var nodeArcGUID = AssetDatabase.FindAssets("t:NodeArc", new[] { "Assets/SO" });
                foreach (var guid in nodeArcGUID)
                {
                    //get path
                    path = AssetDatabase.GUIDToAssetPath(guid);
                    //get SO
                    UnityEngine.Object nodeArcObject = AssetDatabase.LoadAssetAtPath(path, typeof(NodeArc));
                    //assign a zero based unique ID number
                    NodeArc nodeArc = nodeArcObject as NodeArc;
                    nodeArc.nodeArcID = counter++;
                    //add to dictionary
                    try
                    { dictOfNodeArcs.Add(nodeArc.nodeArcID, nodeArc); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid NodeArc (Null)"); counter--; }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid NodeArc (duplicate) ID \"{0}\" for  \"{1}\"", counter, nodeArc.name)); counter--; }
                    //add to lookup dictionary
                    try
                    { dictOfLookUpNodeArcs.Add(nodeArc.name, nodeArc.nodeArcID); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid NodeArc (Null)"); }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid NodeArc (duplicate) Name \"{0}\" for ID \"{1}\"", nodeArc.name, nodeArc.nodeArcID)); }
                }
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfNodeArcs has {0} entries{1}", dictOfNodeArcs.Count, "\n");
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfLookUpNodeArcs has {0} entries{1}", dictOfLookUpNodeArcs.Count, "\n");
                Debug.Assert(dictOfNodeArcs.Count == counter, "Mismatch on Count");
                Debug.Assert(dictOfLookUpNodeArcs.Count == counter, "Mismatch on Count");
                Debug.Assert(dictOfNodeArcs.Count > 0, "No Node Arcs have been imported");
                Debug.Assert(dictOfLookUpNodeArcs.Count > 0, "No Node Arcs in Lookup dictionary");
            }
            else { Debug.LogError("Invalid dictOfLookUpNodeArcs (Null) -> Import failed"); }
        }
        else { Debug.LogError("Invalid dictOfNodeArcs (Null) -> Import failed"); }
        //
        // - - - Node Crisis - - -
        //
        Dictionary<int, NodeCrisis> dictOfNodeCrisis = GameManager.instance.dataScript.GetDictOfNodeCrisis();
        if (dictOfNodeCrisis != null)
        {
            counter = 0;
            //get GUID of all SO Node Objects -> Note that I'm searching the entire database here so it's not folder dependant
            var nodeCrisisGUID = AssetDatabase.FindAssets("t:NodeCrisis", new[] { "Assets/SO" });
            foreach (var guid in nodeCrisisGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object nodeCrisisObject = AssetDatabase.LoadAssetAtPath(path, typeof(NodeCrisis));
                //assign a zero based unique ID number
                NodeCrisis nodeCrisis = nodeCrisisObject as NodeCrisis;
                nodeCrisis.nodeCrisisID = counter++;
                //add to dictionary
                try
                { dictOfNodeCrisis.Add(nodeCrisis.nodeCrisisID, nodeCrisis); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid NodeCrisis (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid NodeCrisis (duplicate) ID \"{0}\" for  \"{1}\"", counter, nodeCrisis.name)); counter--; }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfNodeCrisis has {0} entries{1}", dictOfNodeCrisis.Count, "\n");
            Debug.Assert(dictOfNodeCrisis.Count == counter, "Mismatch on Count");
            Debug.Assert(dictOfNodeCrisis.Count > 0, "No Node Crisis have been imported");
        }
        else { Debug.LogError("Invalid dictOfNodeCrisis (Null) -> Import failed"); }
        //
        // - - - Traits - - -
        //
        Dictionary<int, Trait> dictOfTraits = GameManager.instance.dataScript.GetDictOfTraits();
        List<Trait> listOfAllTraits = GameManager.instance.dataScript.GetListOfAllTraits();
        if (dictOfTraits != null)
        {
            if (listOfAllTraits != null)
            {
                counter = 0;
                //get GUID of all SO Trait Objects -> Note that I'm searching the entire database here so it's not folder dependant
                var traitGUID = AssetDatabase.FindAssets("t:Trait", new[] { "Assets/SO" });
                foreach (var guid in traitGUID)
                {
                    //get path
                    path = AssetDatabase.GUIDToAssetPath(guid);
                    //get SO
                    UnityEngine.Object traitObject = AssetDatabase.LoadAssetAtPath(path, typeof(Trait));
                    //assign a zero based unique ID number
                    Trait trait = traitObject as Trait;
                    trait.traitID = counter++;
                    //add to dictionary
                    try
                    {
                        dictOfTraits.Add(trait.traitID, trait);
                        //add to list
                        listOfAllTraits.Add(trait);
                    }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid Trait (Null)"); counter--; }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid Trait (duplicate) ID \"{0}\" for \"{1}\"", counter, trait.name)); counter--; }
                }
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfTraits has {0} entries{1}", dictOfTraits.Count, "\n");
                Debug.Assert(dictOfTraits.Count == counter, "Mismatch on count");
                Debug.Assert(dictOfTraits.Count > 0, "No Traits have been imported");
            }
            else { Debug.LogError("Invalid listOfAllTraits (Null) -> Import failed"); }
        }
        else { Debug.LogError("Invalid dictOfTraits (Null) -> Import failed"); }
        //
        // - - - Actor Arcs - - 
        //
        Dictionary<int, ActorArc> dictOfActorArcs = GameManager.instance.dataScript.GetDictOfActorArcs();
        List<ActorArc> authorityActorArcs = GameManager.instance.dataScript.GetListOfAuthorityActorArcs();
        List<ActorArc> resistanceActorArcs = GameManager.instance.dataScript.GetListOfResistanceActorArcs();
        if (dictOfActorArcs != null)
        {
            if (authorityActorArcs != null)
            {
                if (resistanceActorArcs != null)
                {
                    counter = 0;
                    //get GUID of all SO ActorArc Objects -> Note that I'm searching the entire database here so it's not folder dependant
                    var arcGUID = AssetDatabase.FindAssets("t:ActorArc", new[] { "Assets/SO" });
                    foreach (var guid in arcGUID)
                    {
                        //get path
                        path = AssetDatabase.GUIDToAssetPath(guid);
                        //get SO
                        UnityEngine.Object arcObject = AssetDatabase.LoadAssetAtPath(path, typeof(ActorArc));
                        //assign a zero based unique ID number
                        ActorArc arc = arcObject as ActorArc;
                        arc.ActorArcID = counter++;
                        //generate a four letter (first 4 of name in CAPS) as a short form tag
                        length = arc.actorName.Length;
                        length = length >= 4 ? 4 : length;
                        arc.ActorTag = arc.actorName.Substring(0, length);
                        //add to dictionary
                        try
                        {
                            dictOfActorArcs.Add(arc.ActorArcID, arc);
                            //add to list
                            if (arc.side.level == globalAuthority.level) { authorityActorArcs.Add(arc); }
                            else if (arc.side.level == globalResistance.level) { resistanceActorArcs.Add(arc); }
                            else { Debug.LogWarning(string.Format("Invalid side \"{0}\", actorArc \"{1}\" NOT added to list", arc.side.name, arc.name)); }
                        }
                        catch (ArgumentNullException)
                        { Debug.LogError("Invalid Actor Arc (Null)"); counter--; }
                        catch (ArgumentException)
                        { Debug.LogError(string.Format("Invalid actorArc (duplicate) ID \"{0}\" for \"{1}\"", counter, arc.name)); counter--; }
                    }
                    Debug.LogFormat("[Imp] InitialiseEarly -> dictOfActorArcs has {0} entries{1}", dictOfActorArcs.Count, "\n");
                    Debug.LogFormat("[Imp] InitialiseEarly -> listOfAuthorityActorArcs has {0} entries{1}", authorityActorArcs.Count, "\n");
                    Debug.LogFormat("[Imp] InitialiseEarly -> listOfResistanceActorArcs has {0} entries{1}", resistanceActorArcs.Count, "\n");
                    Debug.Assert(dictOfActorArcs.Count == counter, "Mismatch on count");
                    Debug.Assert(dictOfActorArcs.Count > 0, "No Actor Arcs have been imported");
                }
                else { Debug.LogError("Invalid resistanceActorArcs (Null) -> Import failed"); }
            }
            else { Debug.LogError("Invalid authorityActorArcs (Null) -> Import failed"); }
        }
        else { Debug.LogError("Invalid dictOfActorArcs (Null) -> Import failed"); }
        //
        // - - - Effects - - -
        //
        Dictionary<int, Effect> dictOfEffects = GameManager.instance.dataScript.GetDictOfEffects();
        if (dictOfEffects != null)
        {
            counter = 0;
            //get GUID of all SO Effect Objects -> Note that I'm searching the entire database here so it's not folder dependant
            var effectsGUID = AssetDatabase.FindAssets("t:Effect", new[] { "Assets/SO" });
            foreach (var guid in effectsGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object effectObject = AssetDatabase.LoadAssetAtPath(path, typeof(Effect));
                //assign a zero based Unique ID number
                Effect effect = effectObject as Effect;
                effect.effectID = counter++;
                //add to dictionary
                try
                { dictOfEffects.Add(effect.effectID, effect); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Effect (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Effect (duplicate) effectID \"{0}\" for \"{1}\"", counter, effect.name)); counter--; }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfEffects has {0} entries{1}", dictOfEffects.Count, "\n");
            Debug.Assert(dictOfEffects.Count == counter, "Mismatch on count");
            Debug.Assert(dictOfEffects.Count > 0, "No Effects have been imported");
        }
        else { Debug.LogError("Invalid dictOfEffects (Null) -> Import failed"); }
        //
        // - - - Targets - - -
        //
        Dictionary<int, Target> dictOfTargets = GameManager.instance.dataScript.GetDictOfTargets();
        if (dictOfTargets != null)
        {
            counter = 0;
            //get GUID of all SO Target Objects -> Note that I'm searching the entire database here so it's not folder dependant
            var targetGUID = AssetDatabase.FindAssets("t:Target", new[] { "Assets/SO" });
            foreach (var guid in targetGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object targetObject = AssetDatabase.LoadAssetAtPath(path, typeof(Target));
                //assign a zero based unique ID number
                Target target = targetObject as Target;
                //set data
                target.targetID = counter++;
                target.targetStatus = Status.Dormant;
                target.timer = -1;
                target.infoLevel = 1;
                target.isKnownByAI = false;
                target.nodeID = -1;
                target.ongoingID = -1;
                //add to dictionary
                try
                { dictOfTargets.Add(target.targetID, target); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Target (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Target (duplicate) ID \"{0}\" for \"{1}\"", counter, target.targetName)); counter--; }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfTargets has {0} entries{1}", dictOfTargets.Count, "\n");
            Debug.Assert(dictOfTargets.Count == counter, "Mismatch on count");
            Debug.Assert(dictOfTargets.Count > 0, "No Targets have been imported");
        }
        else { Debug.LogError("Invalid dictOfTargets (Null) -> Import failed"); }
        //
        // - - - Actions - - -
        //
        Dictionary<int, Action> dictOfActions = GameManager.instance.dataScript.GetDictOfActions();
        Dictionary<string, int> dictOfLookUpActions = GameManager.instance.dataScript.GetDictOfLookUpActions();
        if (dictOfActions != null)
        {
            if (dictOfLookUpActions != null)
            {
                counter = 0;
                //get GUID of all SO Action Objects -> Note that I'm searching the entire database here so it's not folder dependant
                var actionGUID = AssetDatabase.FindAssets("t:Action", new[] { "Assets/SO" });
                foreach (var guid in actionGUID)
                {
                    //get path
                    path = AssetDatabase.GUIDToAssetPath(guid);
                    //get SO
                    UnityEngine.Object actionObject = AssetDatabase.LoadAssetAtPath(path, typeof(Action));
                    //assign a zero based unique ID number
                    Action action = actionObject as Action;
                    //set data
                    action.ActionID = counter++;
                    //add to dictionary
                    try
                    { dictOfActions.Add(action.ActionID, action); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid Action Arc (Null)"); counter--; }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid (duplicate) ActionID \"{0}\" for Action \"{1}\"", action.ActionID, action.name)); counter--; }
                    //add to lookup dictionary
                    try
                    { dictOfLookUpActions.Add(action.name, action.ActionID); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid Lookup Actions (Null)"); }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid Lookup Actions (duplicate) ID \"{0}\" for \"{1}\"", counter, action.name)); }
                }
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfActions has {0} entries{1}", dictOfActions.Count, "\n");
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfLookUpActions has {0} entries{1}", dictOfLookUpActions.Count, "\n");
                Debug.Assert(dictOfActions.Count == counter, "Mismatch on count");
                Debug.Assert(dictOfLookUpActions.Count == counter, "Mismatch on count");
                Debug.Assert(dictOfActions.Count > 0, "No Actions have been imported");
                Debug.Assert(dictOfLookUpActions.Count > 0, "No Actions in lookup dictionary");
            }
            else { Debug.LogError("Invalid dictOfLookUpActions (Null) -> Import failed"); }
        }
        else { Debug.LogError("Invalid dictOfActions (Null) -> Import failed"); }
        //
        // - - - Team Arcs - - -
        //
        Dictionary<int, TeamArc> dictOfTeamArcs = GameManager.instance.dataScript.GetDictOfTeamArcs();
        Dictionary<string, int> dictOfLookUpTeamArcs = GameManager.instance.dataScript.GetDictOfLookUpTeamArcs();
        if (dictOfTeamArcs != null)
        {
            if (dictOfLookUpTeamArcs != null)
            {
                counter = 0;
                //get GUID of all SO Team Objects -> Note that I'm searching the entire database here so it's not folder dependant
                var teamGUID = AssetDatabase.FindAssets("t:TeamArc", new[] { "Assets/SO" });
                foreach (var guid in teamGUID)
                {
                    //get path
                    path = AssetDatabase.GUIDToAssetPath(guid);
                    //get SO
                    UnityEngine.Object teamObject = AssetDatabase.LoadAssetAtPath(path, typeof(TeamArc));
                    //assign a zero based unique ID number
                    TeamArc teamArc = teamObject as TeamArc;
                    //set data
                    teamArc.TeamArcID = counter++;
                    //add to dictionary
                    try
                    { dictOfTeamArcs.Add(teamArc.TeamArcID, teamArc); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid TeamArc (Null)"); counter--; }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid TeamArc (duplicate) ID \"{0}\" for \"{1}\"", counter, teamArc.name)); counter--; }
                    //add to lookup dictionary
                    try
                    { dictOfLookUpTeamArcs.Add(teamArc.name, teamArc.TeamArcID); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid Lookup TeamArc (Null)"); }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid Lookup TeamArc (duplicate) ID \"{0}\" for \"{1}\"", counter, teamArc.name)); }
                }
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfTeamArcs has {0} entries{1}", dictOfTeamArcs.Count, "\n");
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfLookUpTeamArcs has {0} entries{1}", dictOfLookUpTeamArcs.Count, "\n");
                Debug.Assert(dictOfTeamArcs.Count == counter, "Mismatch on Count");
                Debug.Assert(dictOfLookUpTeamArcs.Count == counter, "Mismatch on Count");
                Debug.Assert(dictOfTeamArcs.Count > 0, "No Team Arcs have been imported");
                Debug.Assert(dictOfLookUpTeamArcs.Count > 0, "No Team Arcs in Lookup dictionary");
                //arrayOfTeams
                GameManager.instance.dataScript.InitialiseArrayOfTeams(counter, (int)TeamInfo.Count);
            }
            else { Debug.LogError("Invalid dictOfLookUpTeamArcs (Null) -> Import failed"); }
        }
        else { Debug.LogError("Invalid dictOfTeamArcs (Null) -> Import failed"); }
        //
        // - - - Gear - - -
        //
        Dictionary<int, Gear> dictOfGear = GameManager.instance.dataScript.GetDictOfGear();
        if (dictOfGear != null)
        {
            counter = 0;
            //get GUID of all SO Gear Objects -> Note that I'm searching the entire database here so it's not folder dependant
            var gearGUID = AssetDatabase.FindAssets("t:Gear", new[] { "Assets/SO" });
            foreach (var guid in gearGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object gearObject = AssetDatabase.LoadAssetAtPath(path, typeof(Gear));
                //assign a zero based unique ID number
                Gear gear = gearObject as Gear;
                //set data
                gear.gearID = counter++;
                //add to dictionary
                try
                { dictOfGear.Add(gear.gearID, gear); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Gear (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Gear (duplicate) ID \"{0}\" for \"{1}\"", counter, gear.name)); counter--; }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfGear has {0} entries{1}", dictOfGear.Count, "\n");
            Debug.Assert(dictOfGear.Count == counter, "Mismatch on Count");
            Debug.Assert(dictOfGear.Count > 0, "No Gear has been imported");
        }
        else { Debug.LogError("Invalid dictOfGear (Null) -> Import failed"); }
        //
        // - - - Gear Rarity - - -
        //
        List<GearRarity> listOfGearRarity = GameManager.instance.dataScript.GetListOfGearRarity();
        if (listOfGearRarity != null)
        {
            var gearRarityGUID = AssetDatabase.FindAssets("t:GearRarity", new[] { "Assets/SO" });
            foreach (var guid in gearRarityGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object gearRarityObject = AssetDatabase.LoadAssetAtPath(path, typeof(GearRarity));
                GearRarity gearRarity = gearRarityObject as GearRarity;
                //add to list
                if (gearRarity != null)
                { listOfGearRarity.Add(gearRarity); }
                else { Debug.LogError("Invalid gearRarity (Null)"); }
            }
            Debug.Log(string.Format("[Imp] InitialiseEarly -> listOfGearRarity has {0} entries{1}", listOfGearRarity.Count, "\n"));
        }
        else { Debug.LogError("Invalid listOfGearRarity (Null) -> Import failed"); }
        //
        // - - - Gear Type - - -
        //
        List<GearType> listOfGearType = GameManager.instance.dataScript.GetListOfGearType();
        if (listOfGearType != null)
        {
            var gearTypeGUID = AssetDatabase.FindAssets("t:GearType", new[] { "Assets/SO" });
            foreach (var guid in gearTypeGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object gearTypeObject = AssetDatabase.LoadAssetAtPath(path, typeof(GearType));
                GearType gearType = gearTypeObject as GearType;
                //add to list
                if (gearType != null)
                { listOfGearType.Add(gearType); }
                else { Debug.LogError("Invalid gearType (Null)"); }
            }
            Debug.Log(string.Format("[Imp] InitialiseEarly -> listOfGearType has {0} entries{1}", listOfGearType.Count, "\n"));
        }
        else { Debug.LogError("Invalid listOfGearType (Null) -> Import failed"); }
        //
        // - - - Manage Actions - - -
        //
        Dictionary<string, ManageAction> dictOfManageActions = GameManager.instance.dataScript.GetDictOfManageActions();
        List<ManageAction> listOfActorHandle = new List<ManageAction>();
        List<ManageAction> listOfActorReserve = new List<ManageAction>();
        List<ManageAction> listOfActorDismiss = new List<ManageAction>();
        List<ManageAction> listOfActorDispose = new List<ManageAction>();
        if (dictOfManageActions != null)
        {
            var manageGUID = AssetDatabase.FindAssets("t:ManageAction", new[] { "Assets/SO" });
            foreach (var guid in manageGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object manageObject = AssetDatabase.LoadAssetAtPath(path, typeof(ManageAction));
                ManageAction manageAction = manageObject as ManageAction;
                //add to dictionary
                try
                {
                    dictOfManageActions.Add(manageAction.name, manageAction);
                    //add to the appropriate fast access list
                    switch (manageAction.manage.name)
                    {
                        case "Handle":
                            listOfActorHandle.Add(manageAction);
                            break;
                        case "Reserve":
                            listOfActorReserve.Add(manageAction);
                            break;
                        case "Dismiss":
                            listOfActorDismiss.Add(manageAction);
                            break;
                        case "Dispose":
                            listOfActorDispose.Add(manageAction);
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid manage.manage.name \"{0}\"", manageAction.manage.name));
                            break;
                    }
                }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid manage Action (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid ManageAction (duplicate name)  \"{0}\"", manageAction.name)); }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfManageActions has {0} entries{1}", dictOfManageActions.Count, "\n");
            //sort fast access lists by order -> ActorHandle
            if (listOfActorHandle.Count > 0)
            {
                var manageHandle = listOfActorHandle.OrderBy(o => o.order);
                listOfActorHandle = manageHandle.ToList();
                Debug.Assert(listOfActorHandle.Count > 0, "Invalid count (empty) for listOfActorHandle");
                GameManager.instance.dataScript.SetListOfActorHandle(listOfActorHandle);
            }
            else { Debug.LogError("There are no entries in listOfActorHandle"); }
            //ActorReserve
            if (listOfActorReserve.Count > 0)
            {
                var manageReserve = listOfActorReserve.OrderBy(o => o.order);
                listOfActorReserve = manageReserve.ToList();
                GameManager.instance.dataScript.SetListOfActorReserve(listOfActorReserve);
                Debug.Assert(listOfActorReserve.Count > 0, "Invalid count (empty) for listOfActorReserve");
            }
            else { Debug.LogError("There are no entries in listOfActorReserve"); }
            //ActorDismiss
            if (listOfActorDismiss.Count > 0)
            {
                var manageDismiss = listOfActorDismiss.OrderBy(o => o.order);
                listOfActorDismiss = manageDismiss.ToList();
                GameManager.instance.dataScript.SetListOfActorDismiss(listOfActorDismiss);
                Debug.Assert(listOfActorDismiss.Count > 0, "Invalid count (empty) for listOfActorDismiss");
            }
            else { Debug.LogError("There are no entries in listOfActorDismiss"); }
            //ActorDispose
            if (listOfActorDispose.Count > 0)
            {
                var manageDispose = listOfActorDispose.OrderBy(o => o.order);
                listOfActorDispose = manageDispose.ToList();
                GameManager.instance.dataScript.SetListOfActorDispose(listOfActorDispose);
                Debug.Assert(listOfActorDispose.Count > 0, "Invalid count (empty) for listOfActorDispose");
            }
            else { Debug.LogError("There are no entries in listOfActorDispose"); }
        }
        else { Debug.LogError("Invalid dictOfManageActions (Null) -> Import failed"); }
        //
        // - - - Actor Qualities - - -
        //
        Quality[] authorityQualities = GameManager.instance.dataScript.GetArrayOfAuthorityQualities();
        Quality[] resistanceQualities = GameManager.instance.dataScript.GetArrayOfResistanceQualities();
        if (authorityQualities != null)
        {
            if (resistanceQualities != null)
            {
                //initialise StatTags array
                GameManager.instance.dataScript.InitialiseArrayOfStatTags();
                string[,] arrayOfStatTags = GameManager.instance.dataScript.GetArrayOfStatTags();
                if (arrayOfStatTags != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //authority qualities
                        if (authorityQualities[i] != null)
                        {
                            if (authorityQualities[i].side.level == globalAuthority.level)
                            { arrayOfStatTags[globalAuthority.level, i] = authorityQualities[i].name; }
                            else
                            {
                                Debug.LogWarning(string.Format("Quality (\"{0}\")is the wrong side (\"{1}\"){2}", authorityQualities[i].name, authorityQualities[i].side.name, "\n"));
                                arrayOfStatTags[globalAuthority.level, i] = "Unknown";
                            }
                        }
                        else { arrayOfStatTags[globalAuthority.level, i] = "Unknown"; }
                        //resistance qualities
                        if (resistanceQualities[i] != null)
                        {
                            if (resistanceQualities[i].side.level == globalResistance.level)
                            { arrayOfStatTags[globalResistance.level, i] = resistanceQualities[i].name; }
                            else
                            {
                                Debug.LogWarning(string.Format("Quality (\"{0}\")is the wrong side (\"{1}\"){2}", resistanceQualities[i].name, resistanceQualities[i].side.name, "\n"));
                                arrayOfStatTags[globalResistance.level, i] = "Unknown";
                            }
                        }
                        else { arrayOfStatTags[globalResistance.level, i] = "Unknown"; }
                    }
                    //Initialise Actor Arrays
                    GameManager.instance.dataScript.InitialiseArrayOfActors();
                    GameManager.instance.dataScript.InitialiseArrayOfActorsPresent();
                }
                else { Debug.LogError("Invalid arrayOfStatTags (Null) -> Import failed"); }
            }
            else { Debug.LogError("Invalid resistanceQualities (Null) -> Import failed"); }
        }
        else { Debug.LogError("Invalid authorityQualities (Null) -> Import failed"); }
        //
        // - - - ActorConflicts - - -
        //
        Dictionary<int, ActorConflict> dictOfActorConflicts = GameManager.instance.dataScript.GetDictOfActorConflicts();
        if (dictOfActorConflicts != null)
        {
            counter = 0;
            //get GUID of all SO ActorConflict Objects -> Note that I'm searching the entire database here so it's not folder dependant
            var actorConflictGUID = AssetDatabase.FindAssets("t:ActorConflict", new[] { "Assets/SO" });
            foreach (var guid in actorConflictGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object actorConflictObject = AssetDatabase.LoadAssetAtPath(path, typeof(ActorConflict));
                //assign a zero based unique ID number
                ActorConflict conflict = actorConflictObject as ActorConflict;
                //set data
                conflict.conflictID = counter++;
                //add to dictionary
                try
                { dictOfActorConflicts.Add(conflict.conflictID, conflict); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid ActorConflict (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid ActorConflict (duplicate) ID \"{0}\" for \"{1}\"", counter, conflict.conflictID)); counter--; }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfActorConflicts has {0} entries{1}", dictOfActorConflicts.Count, "\n");
            Debug.Assert(dictOfActorConflicts.Count == counter, "Mismatch in count");
            Debug.Assert(dictOfActorConflicts.Count > 0, "No ActorConflicts imported");
        }
        else { Debug.LogError("Invalid dictOfActorConflicts (Null) -> Import failed"); }
        //
        // - - - Secrets - - -
        //
        Dictionary<int, Secret> dictOfSecrets = GameManager.instance.dataScript.GetDictOfSecrets();
        if (dictOfSecrets != null)
        {
            counter = 0;
            //get GUID of all SO Secret Objects -> Note that I'm searching the entire database here so it's not folder dependant
            var secretGUID = AssetDatabase.FindAssets("t:Secret", new[] { "Assets/SO" });
            foreach (var guid in secretGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object secretObject = AssetDatabase.LoadAssetAtPath(path, typeof(Secret));
                //assign a zero based unique ID number
                Secret secret = secretObject as Secret;
                //set data
                secret.secretID = counter++;
                //add to dictionary
                try
                { dictOfSecrets.Add(secret.secretID, secret); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Secret (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Secret (duplicate) ID \"{0}\" for \"{1}\"", counter, secret.secretID)); counter--; }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfSecrets has {0} entries{1}", dictOfSecrets.Count, "\n");
            Debug.Assert(dictOfSecrets.Count == counter, "Mismatch in count");
            Debug.Assert(dictOfSecrets.Count > 0, "No Secrets imported");
        }
        else { Debug.LogError("Invalid dictOfSecrets (Null) -> Import failed"); }
        //
        // - - - Factions - - -
        //
        Dictionary<int, Faction> dictOfFactions = GameManager.instance.dataScript.GetDictOfFactions();
        if (dictOfFactions != null)
        {
            counter = 0;
            //get GUID of all SO Faction Objects -> Note that I'm searching the entire database here so it's not folder dependant
            var factionGUID = AssetDatabase.FindAssets("t:Faction", new[] { "Assets/SO" });
            foreach (var guid in factionGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object factionObject = AssetDatabase.LoadAssetAtPath(path, typeof(Faction));
                //assign a zero based unique ID number
                Faction faction = factionObject as Faction;
                //set data
                faction.factionID = counter++;
                //add to dictionary
                try
                { dictOfFactions.Add(faction.factionID, faction); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Faction (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Faction (duplicate) ID \"{0}\" for \"{1}\"", counter, faction.name)); counter--; }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfFactions has {0} entries{1}", dictOfFactions.Count, "\n");
            Debug.Assert(dictOfFactions.Count == counter, "Mismatch on count");
            Debug.Assert(dictOfFactions.Count > 0, "No Factions have been imported");
        }
        else { Debug.LogError("Invalid dictOfFactions (Null) -> Import failed"); }
        //
        // - - - City Arcs - - -
        //
        Dictionary<int, CityArc> dictOfCityArcs = GameManager.instance.dataScript.GetDictOfCityArcs();
        if (dictOfCityArcs != null)
        {
            counter = 0;
            //get GUID of all SO CityArc Objects -> Note that I'm searching the entire database here so it's not folder dependant
            var cityArcGUID = AssetDatabase.FindAssets("t:CityArc", new[] { "Assets/SO" });
            foreach (var guid in cityArcGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object cityArcObject = AssetDatabase.LoadAssetAtPath(path, typeof(CityArc));
                //assign a zero based unique ID number
                CityArc cityArc = cityArcObject as CityArc;
                //set data
                cityArc.cityArcID = counter++;
                //add to dictionary
                try
                { dictOfCityArcs.Add(cityArc.cityArcID, cityArc); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid CityArc (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid CityArc (duplicate) ID \"{0}\" for \"{1}\"", counter, cityArc.name)); counter--; }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfCityArcs has {0} entries{1}", dictOfCityArcs.Count, "\n");
            Debug.Assert(dictOfCityArcs.Count == counter, "Mismatch on count");
            Debug.Assert(dictOfCityArcs.Count > 0, "No City Arcs have been imported");
        }
        else { Debug.LogError("Invalid dictOfCityArcs (Null) -> Import failed"); }
        //
        // - - - Cities - - -
        //
        Dictionary<int, City> dictOfCities = GameManager.instance.dataScript.GetDictOfCities();
        if (dictOfCities != null)
        {
            counter = 0;
            //get GUID of all SO City Objects -> Note that I'm searching the entire database here so it's not folder dependant
            var cityGUID = AssetDatabase.FindAssets("t:City", new[] { "Assets/SO" });
            foreach (var guid in cityGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object cityObject = AssetDatabase.LoadAssetAtPath(path, typeof(City));
                //assign a zero based unique ID number
                City city = cityObject as City;
                //set data
                city.cityID = counter++;
                //add to dictionary
                try
                { dictOfCities.Add(city.cityID, city); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid City (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid City (duplicate) ID \"{0}\" for \"{1}\"", counter, city.name)); counter--; }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfCities has {0} entries{1}", dictOfCities.Count, "\n");
            Debug.Assert(dictOfCities.Count == counter, "Mismatch on Count");
            Debug.Assert(dictOfCities.Count > 0, "No cities have been imported");
        }
        else { Debug.LogError("Invalid dictOfCities (Null) -> Import failed"); }
        //
        // - - - Objectives - - -
        //
        Dictionary<int, Objective> dictOfObjectives = GameManager.instance.dataScript.GetDictOfObjectives();
        if (dictOfObjectives != null)
        {
            counter = 0;
            //get GUID of all SO Objective Objects -> Note that I'm searching the entire database here so it's not folder dependant
            var objectiveGUID = AssetDatabase.FindAssets("t:Objective", new[] { "Assets/SO" });
            foreach (var guid in objectiveGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object objectiveObject = AssetDatabase.LoadAssetAtPath(path, typeof(Objective));
                //assign a zero based unique ID number
                Objective objective = objectiveObject as Objective;
                //set data
                objective.objectiveID = counter++;
                //add to dictionary
                try
                { dictOfObjectives.Add(objective.objectiveID, objective); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Objective (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Objective (duplicate) ID \"{0}\" for \"{1}\"", counter, objective.name)); counter--; }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfObjectives has {0} entries{1}", dictOfObjectives.Count, "\n");
            Debug.Assert(dictOfObjectives.Count == counter, "Mismatch on Count");
            Debug.Assert(dictOfObjectives.Count > 0, "No Objectives have been imported");
        }
        else { Debug.LogError("Invalid dictOfObjectives (Null) -> Import failed"); }
        //
        // - - - Organisations - - -
        //
        Dictionary<int, Organisation> dictOfOrganisations = GameManager.instance.dataScript.GetDictOfOrganisations();
        if (dictOfOrganisations != null)
        {
            counter = 0;
            //get GUID of all SO Organisation Objects -> Note that I'm searching the entire database here so it's not folder dependant
            var organisationGUID = AssetDatabase.FindAssets("t:Organisation", new[] { "Assets/SO" });
            foreach (var guid in organisationGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object organisationObject = AssetDatabase.LoadAssetAtPath(path, typeof(Organisation));
                //assign a zero based unique ID number
                Organisation organisation = organisationObject as Organisation;
                //set data
                organisation.orgID = counter++;
                //add to dictionary
                try
                { dictOfOrganisations.Add(organisation.orgID, organisation); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Organisation (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Organisation (duplicate) ID \"{0}\" for \"{1}\"", counter, organisation.name)); counter--; }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfOrganisations has {0} entries{1}", dictOfOrganisations.Count, "\n");
            Debug.Assert(dictOfOrganisations.Count == counter, "Mismatch on Count");
            Debug.Assert(dictOfOrganisations.Count > 0, "No Organisations have been imported");
        }
        else { Debug.LogError("Invalid dictOfOrganisations (Null) -> Import failed"); }
        //
        // - - - Mayors - - -
        //
        Dictionary<int, Mayor> dictOfMayors = GameManager.instance.dataScript.GetDictOfMayors();
        if (dictOfMayors != null)
        {
            counter = 0;
            //get GUID of all SO Mayor Objects -> Note that I'm searching the entire database here so it's not folder dependant
            var mayorGUID = AssetDatabase.FindAssets("t:Mayor", new[] { "Assets/SO" });
            foreach (var guid in mayorGUID)
            {
                //get path
                path = AssetDatabase.GUIDToAssetPath(guid);
                //get SO
                UnityEngine.Object mayorObject = AssetDatabase.LoadAssetAtPath(path, typeof(Mayor));
                //assign a zero based unique ID number
                Mayor mayor = mayorObject as Mayor;
                //set data
                mayor.mayorID = counter++;
                //add to dictionary
                try
                {
                    dictOfMayors.Add(mayor.mayorID, mayor);
                    //initialise trait
                    if (mayor.trait != null)
                    { mayor.AddTrait(mayor.trait); }
                }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Mayor (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Mayor (duplicate) ID \"{0}\" for \"{1}\"", counter, mayor.name)); counter--; }
            }
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfMayors has {0} entries{1}", dictOfMayors.Count, "\n");
            Debug.Assert(dictOfMayors.Count == counter, "Mismatch in count");
            Debug.Assert(dictOfMayors.Count > 0, "No Mayors have been imported");
        }
        else { Debug.LogError("Invalid dictOfMayors (Null) -> Import failed"); }
        //
        // - - - AI Decisions - - -
        //
        Dictionary<int, DecisionAI> dictOfAIDecisions = GameManager.instance.dataScript.GetDictOfAIDecisions();
        Dictionary<string, int> dictOfLookUpAIDecisions = GameManager.instance.dataScript.GetDictOfLookUpAIDecisions();
        if (dictOfAIDecisions != null)
        {
            if (dictOfLookUpAIDecisions != null)
            {
                counter = 0;
                //get GUID of all SO DecisionAI Objects -> Note that I'm searching the entire database here so it's not folder dependant
                var decisionAIGUID = AssetDatabase.FindAssets("t:DecisionAI", new[] { "Assets/SO" });
                foreach (var guid in decisionAIGUID)
                {
                    //get path
                    path = AssetDatabase.GUIDToAssetPath(guid);
                    //get SO
                    UnityEngine.Object decisionAIObject = AssetDatabase.LoadAssetAtPath(path, typeof(DecisionAI));
                    //assign a zero based unique ID number
                    DecisionAI decisionAI = decisionAIObject as DecisionAI;
                    //set data
                    decisionAI.aiDecID = counter++;
                    //add to main dictionary
                    try
                    { dictOfAIDecisions.Add(decisionAI.aiDecID, decisionAI); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid decisionAI (Null)"); counter--; }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid decisionAI (duplicate) ID \"{0}\" for \"{1}\"", counter, decisionAI.name)); counter--; }
                    //add to Lookup dictionary
                    try
                    { dictOfLookUpAIDecisions.Add(decisionAI.name, decisionAI.aiDecID); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid decisionAI (Null)"); counter--; }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid decisionAI.name (duplicate) \"{0}\" for aiDecID \"{1}\"", decisionAI.name, counter)); counter--; }
                }
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfAIDecisions has {0} entries{1}", dictOfAIDecisions.Count, "\n");
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfLookUpAIDecisions has {0} entries{1}", dictOfLookUpAIDecisions.Count, "\n");
                Debug.Assert(dictOfAIDecisions.Count == counter, "Mismatch in count");
                Debug.Assert(dictOfLookUpAIDecisions.Count == counter, "Mismatch in count");
                Debug.Assert(dictOfAIDecisions.Count > 0, "No AI Decisions have been imported");
                Debug.Assert(dictOfLookUpAIDecisions.Count > 0, "No AI Decisions in lookup dictionary");
            }
            else { Debug.LogError("Invalid dictOfLookUpAIDecision (Null) -> Import failed"); }
        }
        else { Debug.LogError("Invalid dictOfAIDecisions (Null) -> Import failed"); }
    }


    /// <summary>
    /// Stuff that is done after LevelManager.SetUp
    /// Note: DataManager.cs InitialiseLate runs immediately prior to this and sets up node arrays and lists
    /// </summary>
    public void InitialiseLate()
    {
        //
        // - - - Nodes - - -
        //
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (dictOfNodes != null)
        {
            int counter = 0;
            List<Node> tempNodeList = GameManager.instance.levelScript.GetListOfNodes();
            if (tempNodeList != null)
            {
                foreach (Node node in tempNodeList)
                {
                    //add to dictionary
                    try
                    { dictOfNodes.Add(node.nodeID, node); counter++; }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid Node (Null)"); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Invalid Node (duplicate) ID \"{0}\" for  \"{1}\"", node.nodeID, node.name); }
                }
                Debug.LogFormat("[Imp] InitialiseLate -> dictOfNodes has {0} entries{1}", dictOfNodes.Count, "\n");
                Debug.Assert(dictOfNodes.Count == counter, "Mismatch in Count");
                Debug.Assert(dictOfNodes.Count > 0, "No Nodes have been imported");
                //create List Of Nodes for iteration purposes
                if (listOfNodes != null)
                {
                    listOfNodes.Clear();
                    listOfNodes.AddRange(dictOfNodes.Values.ToList());
                    Debug.LogFormat("[Imp] InitialiseLate -> listOfNodes has {0} entries{1}", listOfNodes.Count, "\n");
                    Debug.Assert(dictOfNodes.Count == listOfNodes.Count, "Mismatch on count between dictOfNodes and listOfNodes");
                }
                else { Debug.LogError("Invalid listOfNodes (Null)"); }
            }
            else { Debug.LogError("Invalid listOfNodes (Null) from LevelManager"); }
            //Actor Nodes
            GameManager.instance.dataScript.UpdateActorNodes();
        }
        else { Debug.LogError("Invalid dictOfNodes (Null) -> Import failed"); }
        
    }*/


    //new methods above here
}
