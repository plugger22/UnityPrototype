using gameAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// handles loading of all design time SO's
/// </summary>
public class LoadManager : MonoBehaviour
{

    [Header("Initialise Start -> Enums")]
    public GlobalMeta[] arrayOfGlobalMeta;
    public GlobalChance[] arrayOfGlobalChance;
    public GlobalType[] arrayOfGlobalType;
    public GlobalSide[] arrayOfGlobalSide;
    public GlobalWho[] arrayOfGlobalWho;
    public EffectApply[] arrayOfEffectApply;
    public EffectCriteria[] arrayOfEffectCriteria;
    public EffectDuration[] arrayOfEffectDuration;
    public EffectOperator[] arrayOfEffectOperator;
    public EffectOutcome[] arrayOfEffectOutcome;
    public ContactType[] arrayOfContactTypes;
    public TargetType[] arrayOfTargetTypes;
    public TargetTrigger[] arrayOfTargetTriggers;
    public TargetProfile[] arrayOfTargetProfiles;
    public Quality[] arrayOfQualities;
    public CitySize[] arrayOfCitySize;
    public CitySpacing[] arrayOfCitySpacing;
    public CityConnections[] arrayOfCityConnections;
    public CitySecurity[] arrayOfCitySecurity;
    
    
    [Header("InitialiseStart -> Second Half")]
    public Condition[] arrayOfConditions;
    public TraitCategory[] arrayOfTraitCategories;
    public TraitEffect[] arrayOfTraitEffects;
    public SecretType[] arrayOfSecretTypes;
    public SecretStatus[] arrayOfSecretStatus;
    public NodeDatapoint[] arrayOfNodeDatapoints;
    

    [Header("TextLists -> One for each Category")]
    public TextList[] arrayOfContactTextLists;
    public TextList[] arrayOfNameTextLists;
    public TextList[] arrayOfDistrictTextLists;

    [Header("TextList Related SO's")]
    public NameSet[] arrayOfNameSets;

    [Header("InitialiseEarly -> First Half")]
    public NodeArc[] arrayOfNodeArcs;
    public NodeCrisis[] arrayOfNodeCrisis;
    public Trait[] arrayOfTraits;
    public ActorArc[] arrayOfActorArcs;
    public Effect[] arrayOfEffects;
    public Action[] arrayOfActions;
    public TeamArc[] arrayOfTeamArcs;
    public Gear[] arrayOfGear;
    public GearRarity[] arrayOfGearRarity;
    public GearType[] arrayOfGearType;

    [Header("Targets")]
    public Target[] arrayOfTargetsGeneric;
    public Target[] arrayOfTargetsCity;
    public Target[] arrayOfTargetsVIP;
    public Target[] arrayOfTargetsStory;
    public Target[] arrayOfTargetsGoal;

    [Header("InitialiseEarly -> Second Half")]
    public ManageActor[] arrayOfManageActors;
    public ManageAction[] arrayOfManageActions;
    public ActorConflict[] arrayOfActorConflicts;
    public Secret[] arrayOfSecrets;
    public Faction[] arrayOfFactions;
    public CityArc[] arrayOfCityArcs;
    public City[] arrayOfCities;
    public Objective[] arrayOfObjectives;
    public Organisation[] arrayOfOrganisations;
    public Mayor[] arrayOfMayors;
    public DecisionAI[] arrayOfDecisionAI;
    public Mission[] arrayOfMissions;


    public void InitialiseStart()
    {
        int numArray, numDict, counter;
        //
        // - - - GlobalMeta (not stored in a collection)
        //
        numArray = arrayOfGlobalMeta.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfGlobalMeta has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalMeta present"); }
        //
        // - - - GlobalChance (not stored in a collection)
        //
        numArray = arrayOfGlobalChance.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfGlobalChance has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalChance present"); }
        //
        // - - - GlobalType (not stored in a collection)
        //
        numArray = arrayOfGlobalType.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfGlobalType has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalType present"); }
        //
        // - - - GlobalSide (not stored in a collection)
        //
        numArray = arrayOfGlobalSide.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfGlobalSide has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalSide present"); }
        //
        // - - - GlobalWho (not stored in a collection)
        //
        numArray = arrayOfGlobalWho.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfGlobalWho has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No GlobalWho present"); }
        //
        // - - - EffectApply (not stored in a collection)
        //
        numArray = arrayOfEffectApply.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfEffectApply has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No EffectApply present"); }
        //
        // - - - EffectCriteria (not stored in a collection)
        //
        numArray = arrayOfEffectCriteria.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfEffectCriteria has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No EffectCriteria present"); }
        //
        // - - - EffectDuration (not stored in a collection)
        //
        numArray = arrayOfEffectDuration.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfEffectDuration has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No EffectDuration present"); }
        //
        // - - - EffectOperator (not stored in a collection)
        //
        numArray = arrayOfEffectOperator.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfEffectOperator has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No EffectOperator present"); }
        //
        // - - - EffectOutcome (not stored in a collection)
        //
        numArray = arrayOfEffectOutcome.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfEffectOutcome has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No EffectOutcome present"); }
        //
        // - - - ContactType (not stored in a collection)
        //
        numArray = arrayOfContactTypes.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfContactTypes has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No ContactTypes present"); }
        //
        // - - - TargetType (not stored in a collection)
        //
        numArray = arrayOfTargetTypes.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfTargetTypes has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No TargetTypes present"); }
        //
        // - - - TargetTriggers (not stored in a collection)
        //
        numArray = arrayOfTargetTriggers.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfTargetTriggers has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No TargetTriggers present"); }
        //
        // - - - TargetProfile (not stored in a collection)
        //
        numArray = arrayOfTargetProfiles.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfTargetProfiles has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No TargetProfiles present"); }
        //
        // - - - Text Lists Contacts (not stored in a collection)
        //
        numArray = arrayOfContactTextLists.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfContactTextLists has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No ContactTextLists present"); }
        //
        // - - - Text Lists Names (not stored in a collection)
        //
        numArray = arrayOfNameTextLists.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfNameTextLists has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No NameTextLists present"); }
        //
        // - - - Text Lists Districts (not stored in a collection)
        //
        numArray = arrayOfDistrictTextLists.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfDistrictTextLists has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No DistrictTextLists present"); }
        //
        // - - - NameSets (not stored in a collection)
        //
        numArray = arrayOfNameSets.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfNameSets has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No NameSets present"); }
        //
        // - - - CitySize (not stored in a collection)
        //
        numArray = arrayOfCitySize.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfCitySize has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No CitySize present"); }
        //
        // - - - CitySpacing (not stored in a collection)
        //
        numArray = arrayOfCitySpacing.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfCitySpacing has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No CitySpacing present"); }
        //
        // - - - CityConnections (not stored in a collection)
        //
        numArray = arrayOfCityConnections.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfCityConnections has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No CityConnections present"); }
        //
        // - - - CitySecurity (not stored in a collection)
        //
        numArray = arrayOfCitySecurity.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfCitySecurity has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No CitySecurity present"); }
        //
        // - - - Quality
        //
        numArray = arrayOfQualities.Length;
        if (numArray > 0)
        {
            Debug.LogFormat("[Imp] InitialiseStart -> arrayOfQualities has {0} entries{1}", numArray, "\n");
            //copy across to DataManager.cs arrays
            List<Quality> listResistance = new List<Quality>();
            List<Quality> listAuthority = new List<Quality>();
            for (int i = 0; i < numArray; i++)
            {
                Quality quality = arrayOfQualities[i];
                switch (quality.side.name)
                {
                    case "Authority":
                        listAuthority.Add(quality);
                        break;
                    case "Resistance":
                        listResistance.Add(quality);
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid quality  \"{0}\", side \"{1}\", level {2}", quality.name, quality.side.name, quality.side.level);
                        break;
                }
            }
            int numOfQualities = GameManager.instance.preloadScript.numOfQualities;
            //resistance
            if (listResistance.Count == numOfQualities)
            {
                //order list then pass to dataManager.array
                IEnumerable<Quality> sortedList = listResistance.OrderBy(o => o.order);
                GameManager.instance.dataScript.InitialiseResistanceQualities(sortedList);
            }
            else { Debug.LogWarning("Invalid listResistance (size different to numOfQualities)"); }
            //authority
            if (listAuthority.Count == numOfQualities)
            {
                //order list then pass to dataManager.array
                IEnumerable<Quality> sortedList = listAuthority.OrderBy(o => o.order);
                GameManager.instance.dataScript.InitialiseAuthorityQualities(sortedList);
            }
            else { Debug.LogWarning("Invalid listAuthority (size different to numOfQualities)"); }
        }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No Qualities present"); }

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
        numArray = arrayOfNodeDatapoints.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfNodeDatapoints has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No NodeDatapoints present"); }
    }



    /// <summary>
    /// default main constructor
    /// </summary>
    public void InitialiseEarly()
    {
        int numArray, numDict, counter, index;
        GlobalSide globalAuthority = GameManager.instance.globalScript.sideAuthority;
        GlobalSide globalResistance = GameManager.instance.globalScript.sideResistance;
        //
        // - - - Node Arcs - - -
        //
        Dictionary<int, NodeArc> dictOfNodeArcs = GameManager.instance.dataScript.GetDictOfNodeArcs();
        Dictionary<string, int> dictOfLookUpNodeArcs = GameManager.instance.dataScript.GetDictOfLookUpNodeArcs();
        if (dictOfNodeArcs != null)
        {
            counter = 0;
            numArray = arrayOfNodeArcs.Length;
            if (numArray > 0)
            {
                for (int i = 0; i < numArray; i++)
                {
                    //assign a zero based unique ID number
                    NodeArc nodeArc = arrayOfNodeArcs[i];
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
            }
            numDict = dictOfNodeArcs.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfNodeArcs has {0} entries{1}", numDict, "\n");
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfLookUpNodeArcs has {0} entries{1}", dictOfLookUpNodeArcs.Count, "\n");
            Debug.Assert(dictOfNodeArcs.Count == counter, "Mismatch on dictOfNodeArcs Count");
            Debug.Assert(dictOfLookUpNodeArcs.Count == counter, "Mismatch on dictOfLookupNodeArcs Count");
            Debug.Assert(dictOfNodeArcs.Count > 0, "No Node Arcs have been imported");
            Debug.Assert(dictOfLookUpNodeArcs.Count > 0, "No Node Arcs in Lookup dictionary");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on NodeArcs count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfNodeArcs (Null) -> Import failed"); }
        //
        // - - - Node Crisis - - -
        //
        Dictionary<int, NodeCrisis> dictOfNodeCrisis = GameManager.instance.dataScript.GetDictOfNodeCrisis();
        if (dictOfNodeCrisis != null)
        {
            counter = 0;
            numArray = arrayOfNodeCrisis.Length;
            if (numArray > 0)
            {
                for (int i = 0; i < numArray; i++)
                {
                    //assign a zero based unique ID number
                    NodeCrisis nodeCrisis = arrayOfNodeCrisis[i];
                    nodeCrisis.nodeCrisisID = counter++;
                    //add to dictionary
                    try
                    { dictOfNodeCrisis.Add(nodeCrisis.nodeCrisisID, nodeCrisis); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid NodeCrisis (Null)"); counter--; }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid NodeCrisis (duplicate) ID \"{0}\" for  \"{1}\"", counter, nodeCrisis.name)); counter--; }
                }
            }
            numDict = dictOfNodeCrisis.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfNodeCrisis has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No Node Crisis have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on NodeCrisis count, array {0}, dict {1}", numArray, numDict));
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
                numArray = arrayOfTraits.Length;
                for (int i = 0; i < numArray; i++)
                {
                    //assign a zero based unique ID number
                    Trait trait = arrayOfTraits[i];
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
                numDict = dictOfTraits.Count;
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfTraits has {0} entries{1}", numDict, "\n");
                Debug.Assert(numDict == counter, "Mismatch on count");
                Debug.Assert(numDict > 0, "No Traits have been imported");
                Debug.Assert(numArray == numDict, string.Format("Mismatch in Trait count, array {0}, dict {1}", numArray, numDict));
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
                    numArray = arrayOfActorArcs.Length;
                    for (int i = 0; i < numArray; i++)
                    {
                        //assign a zero based unique ID number
                        ActorArc arc = arrayOfActorArcs[i];
                        arc.ActorArcID = counter++;
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
                    numDict = dictOfActorArcs.Count;
                    Debug.LogFormat("[Imp] InitialiseEarly -> dictOfActorArcs has {0} entries{1}", numDict, "\n");
                    Debug.LogFormat("[Imp] InitialiseEarly -> listOfAuthorityActorArcs has {0} entries{1}", authorityActorArcs.Count, "\n");
                    Debug.LogFormat("[Imp] InitialiseEarly -> listOfResistanceActorArcs has {0} entries{1}", resistanceActorArcs.Count, "\n");
                    Debug.Assert(numDict == counter, "Mismatch on count");
                    Debug.Assert(numDict > 0, "No Actor Arcs have been imported");
                    Debug.Assert(numArray == numDict, string.Format("Mismatch on ActorArcs count, array {0}, dict {1}", numArray, numDict));
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
            numArray = arrayOfEffects.Length;
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based Unique ID number
                Effect effect = arrayOfEffects[i];
                effect.effectID = counter++;
                //add to dictionary
                try
                { dictOfEffects.Add(effect.effectID, effect); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Effect (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Effect (duplicate) effectID \"{0}\" for \"{1}\"", counter, effect.name)); counter--; }
            }
            numDict = dictOfEffects.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfEffects has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on count");
            Debug.Assert(numDict > 0, "No Effects have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on Effects count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfEffects (Null) -> Import failed"); }
        //
        // - - - Targets - - -
        //            
        Debug.Assert(arrayOfTargetsGeneric.Length > 0, "Invalid arrayOfTargetsGeneric (no records)");
        Debug.Assert(arrayOfTargetsCity.Length > 0, "Invalid arrayOfTargetsCity (no records)");
        Debug.Assert(arrayOfTargetsVIP.Length > 0, "Invalid arrayOfTargetsVIP (no records)");
        Debug.Assert(arrayOfTargetsStory.Length > 0, "Invalid arrayOfTargetsStory (no records)");
        Debug.Assert(arrayOfTargetsGoal.Length > 0, "Invalid arrayOfTargetsGoal (no records)");
        Dictionary<int, Target> dictOfTargets = GameManager.instance.dataScript.GetDictOfTargets();
        if (dictOfTargets != null)
        {
            counter = 0;
            List<Target> listOfTargets = new List<Target>();
            listOfTargets.AddRange(arrayOfTargetsGeneric);
            listOfTargets.AddRange(arrayOfTargetsCity);
            listOfTargets.AddRange(arrayOfTargetsVIP);
            listOfTargets.AddRange(arrayOfTargetsStory);
            listOfTargets.AddRange(arrayOfTargetsGoal);
            numArray = listOfTargets.Count;

            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                Target target = listOfTargets[i];
                //set data
                target.targetID = counter++;
                target.targetStatus = Status.Dormant;
                //target.timer = -1;
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
            numDict = dictOfTargets.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfTargets has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on count");
            Debug.Assert(numDict > 0, "No Targets have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Targets count, array {0}, dict {1}", numArray, numDict));
            //initialise Generic target array
            GameManager.instance.dataScript.InitialiseArrayOfGenericTargets();
            List<int>[] arrayOfGenericTargets = GameManager.instance.dataScript.GetArrayOfGenericTargets();
            if (arrayOfGenericTargets != null)
            {
                //assign targets to pools
                foreach (var target in dictOfTargets)
                {
                    if (target.Value.targetType != null)
                    {
                        switch (target.Value.targetType.name)
                        {
                            case "Generic":
                                if (target.Value.nodeArc != null)
                                {
                                    //only level one targets placed in array
                                    if (target.Value.targetLevel == 1)
                                    {
                                        index = target.Value.nodeArc.nodeArcID;
                                        arrayOfGenericTargets[index].Add(target.Value.targetID);
                                        /*Debug.LogFormat("[Tst] LoadManager.cs -> InitialiseEarly: Target \"{0}\", nodeArcID {1}, added to arrayOfGenericTargets[{2}]{3}", target.Value.name,
                                            target.Value.nodeArc.nodeArcID, index, "\n");*/
                                    }
                                }
                                else { Debug.LogWarningFormat("Invalid nodeArc for Generic target {0}", target.Value.name); }
                                break;

                        }
                    }
                    else { Debug.LogWarningFormat("Invalid TargetType (Null) for target \"{0}\"", target.Value.name); }
                }
            }
            else { Debug.LogError("Invalid arrayOfGenericTargets (Null)"); }
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
                numArray = arrayOfActions.Length;
                for (int i = 0; i < numArray; i++)
                {
                    //assign a zero based unique ID number
                    Action action = arrayOfActions[i];
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
                numDict = dictOfActions.Count;
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfActions has {0} entries{1}", numDict, "\n");
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfLookUpActions has {0} entries{1}", dictOfLookUpActions.Count, "\n");
                Debug.Assert(numDict == counter, "Mismatch on count");
                Debug.Assert(dictOfLookUpActions.Count == counter, "Mismatch on count");
                Debug.Assert(numDict > 0, "No Actions have been imported");
                Debug.Assert(dictOfLookUpActions.Count > 0, "No Actions in lookup dictionary");
                Debug.Assert(numArray == numDict, string.Format("Mismatch in Action count, array {0}, dict {1}", numArray, numDict));
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
                numArray = arrayOfTeamArcs.Length;
                for (int i = 0; i < numArray; i++)
                {
                    //assign a zero based unique ID number
                    TeamArc teamArc = arrayOfTeamArcs[i];
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
                numDict = dictOfTeamArcs.Count;
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfTeamArcs has {0} entries{1}", numDict, "\n");
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfLookUpTeamArcs has {0} entries{1}", dictOfLookUpTeamArcs.Count, "\n");
                Debug.Assert(numDict == counter, "Mismatch on Count");
                Debug.Assert(dictOfLookUpTeamArcs.Count == counter, "Mismatch on Count");
                Debug.Assert(numDict > 0, "No Team Arcs have been imported");
                Debug.Assert(dictOfLookUpTeamArcs.Count > 0, "No Team Arcs in Lookup dictionary");
                Debug.Assert(numArray == numDict, string.Format("Mismatch in TeamArc count, array {0}, dict {1}", numArray, numDict));
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
            numArray = arrayOfGear.Length;
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                Gear gear = arrayOfGear[i];
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
            numDict = dictOfGear.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfGear has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No Gear has been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Gear count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfGear (Null) -> Import failed"); }
        //
        // - - - Gear Rarity - - -
        //
        List<GearRarity> listOfGearRarity = GameManager.instance.dataScript.GetListOfGearRarity();
        if (listOfGearRarity != null)
        {
            numArray = arrayOfGearRarity.Length;
            for (int i = 0; i < numArray; i++)
            {
                GearRarity gearRarity = arrayOfGearRarity[i];
                //add to list
                if (gearRarity != null)
                { listOfGearRarity.Add(gearRarity); }
                else { Debug.LogError("Invalid gearRarity (Null)"); }
            }
            numDict = listOfGearRarity.Count;
            Debug.Log(string.Format("[Imp] InitialiseEarly -> listOfGearRarity has {0} entries{1}", numDict, "\n"));
            Debug.Assert(numArray == numDict, string.Format("Mismatch in GearRarity count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid listOfGearRarity (Null) -> Import failed"); }
        //
        // - - - Gear Type - - -
        //
        List<GearType> listOfGearType = GameManager.instance.dataScript.GetListOfGearType();
        if (listOfGearType != null)
        {
            numArray = arrayOfGearType.Length;
            for (int i = 0; i < numArray; i++)
            {
                GearType gearType = arrayOfGearType[i];
                //add to list
                if (gearType != null)
                { listOfGearType.Add(gearType); }
                else { Debug.LogError("Invalid gearType (Null)"); }
            }
            numDict = listOfGearType.Count;
            Debug.Log(string.Format("[Imp] InitialiseEarly -> listOfGearType has {0} entries{1}", numDict, "\n"));
            Debug.Assert(numArray == numDict, string.Format("Mismatch in GearType count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid listOfGearType (Null) -> Import failed"); }
        //
        // - - - Manage Actors - - -
        //
        numArray = arrayOfManageActors.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Imp] InitialiseStart -> arrayOfManageActors has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("[Imp] LoadManager.cs -> InitialiseStart: No ManageActors present"); }
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
            numArray = arrayOfManageActions.Length;
            for (int i = 0; i < numArray; i++)
            {
                ManageAction manageAction = arrayOfManageActions[i];
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
            numDict = dictOfManageActions.Count;
            Debug.Assert(numArray == numDict, string.Format("Mismatch in ManageAction count, array {0}, dict {1}", numArray, numDict));
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
            numArray = arrayOfActorConflicts.Length;
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                ActorConflict conflict = arrayOfActorConflicts[i];
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
            numDict = dictOfActorConflicts.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfActorConflicts has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch in count");
            Debug.Assert(numDict > 0, "No ActorConflicts imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in ActorConflict count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfActorConflicts (Null) -> Import failed"); }

        //
        // - - - Secrets - - -
        //
        Dictionary<int, Secret> dictOfSecrets = GameManager.instance.dataScript.GetDictOfSecrets();
        if (dictOfSecrets != null)
        {
            counter = 0;
            numArray = arrayOfSecrets.Length;
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                Secret secret = arrayOfSecrets[i];
                //set data
                secret.secretID = counter++;
                //add to dictionary
                try
                { dictOfSecrets.Add(secret.secretID, secret); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Secret (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Secret (duplicate) ID \"{0}\" for \"{1}\"", counter, secret.secretID)); counter--; }
                //add to list
            }
            numDict = dictOfSecrets.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfSecrets has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch in count");
            Debug.Assert(numDict > 0, "No Secrets imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Secrets count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfSecrets (Null) -> Import failed"); }
        //
        // - - - Factions - - -
        //
        Dictionary<int, Faction> dictOfFactions = GameManager.instance.dataScript.GetDictOfFactions();
        if (dictOfFactions != null)
        {
            counter = 0;
            numArray = arrayOfFactions.Length;
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                Faction faction = arrayOfFactions[i];
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
            numDict = dictOfFactions.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfFactions has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on count");
            Debug.Assert(numDict > 0, "No Factions have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Faction count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfFactions (Null) -> Import failed"); }
        //
        // - - - City Arcs - - -
        //
        Dictionary<int, CityArc> dictOfCityArcs = GameManager.instance.dataScript.GetDictOfCityArcs();
        if (dictOfCityArcs != null)
        {
            counter = 0;
            numArray = arrayOfCityArcs.Length;
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                CityArc cityArc = arrayOfCityArcs[i];
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
            numDict = dictOfCityArcs.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfCityArcs has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on count");
            Debug.Assert(numDict > 0, "No City Arcs have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in CityArc count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfCityArcs (Null) -> Import failed"); }
        //
        // - - - Cities - - -
        //
        Dictionary<int, City> dictOfCities = GameManager.instance.dataScript.GetDictOfCities();
        if (dictOfCities != null)
        {
            counter = 0;
            numArray = arrayOfCities.Length;
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                City city = arrayOfCities[i];
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
            numDict = dictOfCities.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfCities has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No cities have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in City count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfCities (Null) -> Import failed"); }
        //
        // - - - Objectives - - -
        //
        Dictionary<int, Objective> dictOfObjectives = GameManager.instance.dataScript.GetDictOfObjectives();
        if (dictOfObjectives != null)
        {
            counter = 0;
            numArray = arrayOfObjectives.Length;
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                Objective objective = arrayOfObjectives[i];
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
            numDict = dictOfObjectives.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfObjectives has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No Objectives have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Objective count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfObjectives (Null) -> Import failed"); }
        //
        // - - - Organisations - - -
        //
        Dictionary<int, Organisation> dictOfOrganisations = GameManager.instance.dataScript.GetDictOfOrganisations();
        if (dictOfOrganisations != null)
        {
            counter = 0;
            numArray = arrayOfOrganisations.Length;
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                Organisation organisation = arrayOfOrganisations[i];
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
            numDict = dictOfOrganisations.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfOrganisations has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No Organisations have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Organisation count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfOrganisations (Null) -> Import failed"); }
        //
        // - - - Mayors - - -
        //
        Dictionary<int, Mayor> dictOfMayors = GameManager.instance.dataScript.GetDictOfMayors();
        if (dictOfMayors != null)
        {
            counter = 0;
            numArray = arrayOfMayors.Length;
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                Mayor mayor = arrayOfMayors[i];
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
            numDict = dictOfMayors.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfMayors has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch in count");
            Debug.Assert(numDict > 0, "No Mayors have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Mayor count, array {0}, dict {1}", numArray, numDict));
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
                numArray = arrayOfDecisionAI.Length;
                for (int i = 0; i < numArray; i++)
                {
                    //assign a zero based unique ID number
                    DecisionAI decisionAI = arrayOfDecisionAI[i];
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
                numDict = dictOfAIDecisions.Count;
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfAIDecisions has {0} entries{1}", numDict, "\n");
                Debug.LogFormat("[Imp] InitialiseEarly -> dictOfLookUpAIDecisions has {0} entries{1}", dictOfLookUpAIDecisions.Count, "\n");
                Debug.Assert(numDict == counter, "Mismatch in count");
                Debug.Assert(dictOfLookUpAIDecisions.Count == counter, "Mismatch in count");
                Debug.Assert(numDict > 0, "No AI Decisions have been imported");
                Debug.Assert(dictOfLookUpAIDecisions.Count > 0, "No AI Decisions in lookup dictionary");
                Debug.Assert(numArray == numDict, string.Format("Mismatch in DecisionAI count, array {0}, dict {1}", numArray, numDict));
            }
            else { Debug.LogError("Invalid dictOfLookUpAIDecision (Null) -> Import failed"); }
        }
        else { Debug.LogError("Invalid dictOfAIDecisions (Null) -> Import failed"); }
        //
        // - - - Missions - - -
        //
        Dictionary<int, Mission> dictOfMissions = GameManager.instance.dataScript.GetDictOfMissions();
        if (dictOfMissions != null)
        {
            counter = 0;
            numArray = arrayOfMissions.Length;
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                Mission mission = arrayOfMissions[i];
                //set data
                mission.missionID = counter++;
                //add to dictionary
                try
                { dictOfMissions.Add(mission.missionID, mission); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Mission (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Mission (duplicate) ID \"{0}\" for \"{1}\"", counter, mission.name)); counter--; }
            }
            numDict = dictOfMissions.Count;
            Debug.LogFormat("[Imp] InitialiseEarly -> dictOfMissions has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No Missions have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Mission count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfMissions (Null) -> Import failed"); }
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
        }
        else { Debug.LogError("Invalid dictOfNodes (Null) -> Import failed"); }
    }

}
