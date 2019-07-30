using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using gameAPI;
using packageAPI;

/// <summary>
/// handles loading of all design time SO's
/// </summary>
public class LoadManager : MonoBehaviour
{

    #region Arrays
    [Header("Initialise Start Enums")]
    public GlobalMeta[] arrayOfGlobalMeta;
    public GlobalChance[] arrayOfGlobalChance;
    public GlobalType[] arrayOfGlobalType;
    public GlobalSide[] arrayOfGlobalSide;
    public GlobalWho[] arrayOfGlobalWho;
    public Criteria[] arrayOfCriteria;
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
    public Colour[] arrayOfColours;
    public CitySize[] arrayOfCitySize;
    public CitySpacing[] arrayOfCitySpacing;
    public CityConnections[] arrayOfCityConnections;
    public CitySecurity[] arrayOfCitySecurity;
    public Damage[] arrayOfDamages;
    public Challenge[] arrayOfChallenges;
    public Nemesis[] arrayOfNemesis;
    public RebelLeader[] arrayOfRebelLeaders;
    
    [Header("InitialiseStart")]
    public Condition[] arrayOfConditions;
    public Cure[] arrayOfCures;
    public TraitCategory[] arrayOfTraitCategories;
    public TraitEffect[] arrayOfTraitEffects;
    public SecretType[] arrayOfSecretTypes;
    public NodeDatapoint[] arrayOfNodeDatapoints;
    
    [Header("TextLists")]
    public TextList[] arrayOfContactTextLists;
    public TextList[] arrayOfNameTextLists;
    public TextList[] arrayOfDistrictTextLists;
    public TextList[] arrayOfShortTextLists;
    public TextList[] arrayOfFactorTextLists;

    [Header("Consolidated TextLists")]
    public NameSet[] arrayOfNameSets;

    [Header("Personality Factors - ORDER MATTERS")]
    public Factor[] arrayOfFiveFactorModel;

    [Header("Personality Profiles")]
    public PersonProfile[] arrayOfPersonProfiles;

    [Header("InitialiseEarly")]
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

    [Header("Topics")]
    public TopicType[] arrayOfTopicTypes;
    public TopicSubType[] arrayOfTopicSubTypes;
    public TopicOption[] arrayOfTopicOptions;
    public TopicPool[] arrayOfTopicPools;
    public Topic[] arrayOfTopics;

    [Header("Targets")]
    public Target[] arrayOfTargetsGeneric;
    public Target[] arrayOfTargetsCity;
    public Target[] arrayOfTargetsVIP;
    public Target[] arrayOfTargetsStory;
    public Target[] arrayOfTargetsGoal;

    [Header("Sprites")]
    public Sprite[] arrayOfGearSprites;
    public Sprite[] arrayOfGlobalSprites;
    public Sprite[] arrayOfNodeArcSprites;
    public Sprite[] arrayOfPortraitSprites;
    public Sprite[] arrayOfTargetSprites;
    public Sprite[] arrayOfTeamSprites;

    [Header("InitialiseEarly")]
    public ManageActor[] arrayOfManageActors;
    public ManageAction[] arrayOfManageActions;
    public ActorConflict[] arrayOfActorConflicts;
    public Secret[] arrayOfSecrets;
    public Faction[] arrayOfFactions;
    public CityArc[] arrayOfCityArcs;
    public City[] arrayOfCities;
    public Objective[] arrayOfObjectives;
    public ObjectiveTarget[] arrayOfObjectiveTargets;
    public Organisation[] arrayOfOrganisations;
    public Mayor[] arrayOfMayors;
    public DecisionAI[] arrayOfDecisionAI;
    public Mission[] arrayOfMissions;
    public Scenario[] arrayOfScenarios;
    public Campaign[] arrayOfCampaigns;
    #endregion

    #region InitialiseStart

    public void InitialiseStart(GameState state)
    {
        int numArray, numDict, counter;
        //
        // - - - Personality Factors
        //
        List<Factor> listOfFactors = new List<Factor>();
        listOfFactors.AddRange(arrayOfFiveFactorModel);
        /*listOfFactors.AddRange(arrayOfDarkTriad);*/
        GameManager.instance.dataScript.SetFactorArrays(listOfFactors);
        Debug.LogFormat("[Loa] LoadManager.cs -> InitialiseStart: listOfFactors has {0} entries{1}", listOfFactors.Count, "\n");
        //
        // - - - PersonProfile
        //
        Dictionary<string, PersonProfile> dictOfProfiles = GameManager.instance.dataScript.GetDictOfProfiles();
        if (dictOfProfiles != null)
        {
            numArray = arrayOfPersonProfiles.Length;
            if (numArray > 0)
            { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfPersonProfiles has {0} entries{1}", numArray, "\n"); }
            else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No PersonProfile present"); }
            //add to dictionary
            counter = 0;
            for (int i = 0; i < numArray; i++)
            {
                PersonProfile profile = arrayOfPersonProfiles[i];
                if (profile != null)
                {
                    try
                    { dictOfProfiles.Add(profile.name, profile); counter++; }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate record exists in dictOfProfiles for {0}", profile); }
                }
                else { Debug.LogWarningFormat("Invalid Profile (Null) for arrayOfProfile[{0}]", i); }
            }
            numDict = dictOfProfiles.Count;
            Debug.LogFormat("[Loa] InitialiseStart -> dictOfProfiles has {0} entries{1}", numDict, "\n");
            Debug.Assert(dictOfProfiles.Count == counter, "Mismatch on count");
            Debug.Assert(dictOfProfiles.Count > 0, "No PersonProfiles imported to dictionary");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in PersonProfiles count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfProfiles (Null)"); }
        //
        // - - - GlobalMeta (not stored in a collection)
        //
        numArray = arrayOfGlobalMeta.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfGlobalMeta has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No GlobalMeta present"); }
        //
        // - - - GlobalChance (not stored in a collection)
        //
        numArray = arrayOfGlobalChance.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfGlobalChance has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No GlobalChance present"); }
        //
        // - - - GlobalType
        //
        Dictionary<string, GlobalType> dictOfGlobalType = GameManager.instance.dataScript.GetDictOfGlobalType();
        if (dictOfGlobalType != null)
        {
            numArray = arrayOfGlobalType.Length;
            if (numArray > 0)
            { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfGlobalType has {0} entries{1}", numArray, "\n"); }
            else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No GlobalType present"); }
            //add to dictionary
            for (int i = 0; i < numArray; i++)
            {
                GlobalType type = arrayOfGlobalType[i];
                if (type != null)
                {
                    try
                    { dictOfGlobalType.Add(type.name, type); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate record exists in dictOfGlobalType for {0}", type); }
                }
                else { Debug.LogWarningFormat("Invalid GlobalType (Null) for arrayOfGlobalType[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfGlobalType (Null)"); }

        //
        // - - - GlobalSide
        //
        Dictionary<string, GlobalSide> dictOfGlobalSide = GameManager.instance.dataScript.GetDictOfGlobalSide();
        if (dictOfGlobalSide != null)
        {
            numArray = arrayOfGlobalSide.Length;
            if (numArray > 0)
            { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfGlobalSide has {0} entries{1}", numArray, "\n"); }
            else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No GlobalSide present"); }
            //add to dictionary
            for (int i = 0; i < numArray; i++) 
            {
                GlobalSide side = arrayOfGlobalSide[i];
                if (side != null)
                {
                    try
                    { dictOfGlobalSide.Add(side.name, side); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate record exists in dictOfGlobalSide for {0}", side); }
                }
                else { Debug.LogWarningFormat("Invalid GlobalSide (Null) for arrayOfGlobalSide[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfGlobalSide (Null)"); }
        //
        // - - - GlobalWho (not stored in a collection)
        //
        numArray = arrayOfGlobalWho.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfGlobalWho has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("LoadManager.cs -> InitialiseStart: No GlobalWho present"); }
        //
        // - - - Effect (not stored in a collection)
        //
        numArray = arrayOfEffects.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfEffects has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("LoadManager.cs -> InitialiseStart: No Effects present"); }
        //
        // - - - Criteria (not stored in a collection)
        //
        numArray = arrayOfCriteria.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfCriteria has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("LoadManager.cs -> InitialiseStart: No Criteria present"); }
        //
        // - - - EffectApply (not stored in a collection)
        //
        numArray = arrayOfEffectApply.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfEffectApply has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No EffectApply present"); }
        //
        // - - - EffectCriteria (not stored in a collection)
        //
        numArray = arrayOfEffectCriteria.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfEffectCriteria has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No EffectCriteria present"); }
        //
        // - - - EffectDuration (not stored in a collection)
        //
        numArray = arrayOfEffectDuration.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfEffectDuration has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No EffectDuration present"); }
        //
        // - - - EffectOperator (not stored in a collection)
        //
        numArray = arrayOfEffectOperator.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfEffectOperator has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No EffectOperator present"); }
        //
        // - - - EffectOutcome (not stored in a collection)
        //
        numArray = arrayOfEffectOutcome.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfEffectOutcome has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No EffectOutcome present"); }

        //
        // - - - ObjectiveTargets (not stored in a collection)
        //
        numArray = arrayOfObjectiveTargets.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfObjectiveTargets has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No ObjectiveArcs present"); }
        //
        // - - - ContactType (not stored in a collection)
        //
        numArray = arrayOfContactTypes.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfContactTypes has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No ContactTypes present"); }
        //
        // - - - TargetType (not stored in a collection)
        //
        numArray = arrayOfTargetTypes.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfTargetTypes has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No TargetTypes present"); }
        //
        // - - - TargetTriggers (not stored in a collection)
        //
        numArray = arrayOfTargetTriggers.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfTargetTriggers has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No TargetTriggers present"); }
        //
        // - - - TargetProfile (not stored in a collection)
        //
        numArray = arrayOfTargetProfiles.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfTargetProfiles has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No TargetProfiles present"); }
        //
        // - - - Text Lists Contacts (not stored in a collection)
        //
        numArray = arrayOfContactTextLists.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfContactTextLists has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No ContactTextLists present"); }
        //
        // - - - Text Lists Names (not stored in a collection)
        //
        numArray = arrayOfNameTextLists.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfNameTextLists has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No NameTextLists present"); }
        //
        // - - - Text Lists Districts (not stored in a collection)
        //
        numArray = arrayOfDistrictTextLists.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfDistrictTextLists has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No DistrictTextLists present"); }
        //
        // - - - Text Lists Shorts (not stored in a collection)
        //
        numArray = arrayOfShortTextLists.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfShortTextLists has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No ShortTextLists present"); }
        //
        // - - - Text Lists Factors (not stored in a collection)
        //
        numArray = arrayOfFactorTextLists.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfFactorTextLists has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No ShortTextLists present"); }
        //
        // - - - NameSets (not stored in a collection)
        //
        Dictionary<string, NameSet> dictOfNameSet = GameManager.instance.dataScript.GetDictOfNameSet();
        if (dictOfNameSet != null)
        {
            numArray = arrayOfNameSets.Length;
            if (numArray > 0)
            { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfNameSets has {0} entries{1}", numArray, "\n"); }
            else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No NameSets present"); }
            for (int i = 0; i < numArray; i++)
            {
                NameSet set = arrayOfNameSets[i];
                if (set != null)
                {
                    try
                    { dictOfNameSet.Add(set.name, set); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate NameSet {0}", set); }
                }
                else { Debug.LogWarningFormat("Invalid nameSet (Null) for arrayOfNameSets[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfNameSet (Null)"); }
        //
        // - - - City Arcs (not stored in a collection)
        //
        numArray = arrayOfCityArcs.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfCityArcs has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No CityArcs present"); }
        //
        // - - - CitySize (not stored in a collection)
        //
        numArray = arrayOfCitySize.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfCitySize has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No CitySize present"); }
        //
        // - - - CitySpacing (not stored in a collection)
        //
        numArray = arrayOfCitySpacing.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfCitySpacing has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No CitySpacing present"); }
        //
        // - - - CityConnections (not stored in a collection)
        //
        numArray = arrayOfCityConnections.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfCityConnections has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No CityConnections present"); }
        //
        // - - - CitySecurity (not stored in a collection)
        //
        numArray = arrayOfCitySecurity.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfCitySecurity has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No CitySecurity present"); }
        //
        // - - - Scenario (not stored in a collection)
        //
        numArray = arrayOfScenarios.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfScenarios has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No Scenario present"); }
        //
        // - - - Damage (not stored in a collection)
        //
        numArray = arrayOfDamages.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfDamages has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No Damages present"); }
        //
        // - - - Challenge (not stored in a collection)
        //
        numArray = arrayOfChallenges.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfChallenges has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No Challenges present"); }
        //
        // - - - Nemesis (not stored in a collection)
        //
        numArray = arrayOfNemesis.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfNemesis has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No Nemesis present"); }
        //
        // - - - Rebel Leaders (not stored in a collection)
        //
        numArray = arrayOfRebelLeaders.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfRebelLeaders has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No RebelLeaders present"); }
        //
        // - - - Mission (not stored in a collection)
        //
        numArray = arrayOfMissions.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfMissions has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No Missions present"); }
        //
        // - - - Quality
        //
        numArray = arrayOfQualities.Length;
        if (numArray > 0)
        {
            Debug.LogFormat("[Loa] InitialiseStart -> arrayOfQualities has {0} entries{1}", numArray, "\n");
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
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No Qualities present"); }
        //
        // - - - Colours (not stored in a collection)
        //
        numArray = arrayOfColours.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfColours has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No colours present"); }        
        //
        // - - - Mission (not stored in a collection)
        //
        numArray = arrayOfMissions.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfMissions has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No Missions present"); }
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
                { dictOfConditions.Add(condition.tag, condition); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Condition (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Condition (duplicate) \"{0}\"", condition.tag)); }
            }
            numDict = dictOfConditions.Count;
            Debug.LogFormat("[Loa] InitialiseStart -> dictOfConditions has {0} entries{1}", numDict, "\n");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on Condition Load -> array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No Conditions present"); }
        //
        // - - - Cures - - -
        //
        counter = 0;
        numArray = arrayOfCures.Length;
        if (numArray > 0)
        {
            Dictionary<string, Cure> dictOfCures = GameManager.instance.dataScript.GetDictOfCures();
            for (int i = 0; i < numArray; i++)
            {
                //assign a zero based unique ID number
                Cure cure = arrayOfCures[i];
                if (cure != null)
                {
                    counter++;
                    //add to dictionary
                    try
                    { dictOfCures.Add(cure.name, cure); }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid Cure (duplicate) \"{0}\"", cure.name)); }
                }
                else { Debug.LogErrorFormat("Invalid Cure (Null) for arrayOfCures[\"{0}\"]", i); }
            }
            numDict = dictOfCures.Count;
            Debug.LogFormat("[Loa] InitialiseStart -> dictOfCures has {0} entries{1}", numDict, "\n");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on Cure Load -> array {0}, dict {1}", numArray, numDict));
            Debug.Assert(counter == numDict, string.Format("Mismatch on Cure Load -> counter {0}, dict {1}", counter, numDict));
        }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No Cures present"); }
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
            Debug.LogFormat("[Loa] InitialiseStart -> dictOfTraitCategories has {0} entries{1}", numDict, "\n");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on TraitCategory Load -> array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No TraitCategories present"); }
        //
        // - - - TraitEffects - - -
        //
        Dictionary<string, TraitEffect> dictOfTraitEffects = GameManager.instance.dataScript.GetDictOfTraitEffects();
        if (dictOfTraitCategories != null)
        {

                counter = 0;
                numArray = arrayOfTraitEffects.Length;
                if (numArray > 0)
                {
                    for (int i = 0; i < numArray; i++)
                    {
                        TraitEffect traitEffect = arrayOfTraitEffects[i];
                        counter++;
                        //add to dictionaries
                        try
                        { dictOfTraitEffects.Add(traitEffect.name, traitEffect); }
                        catch (ArgumentNullException)
                        { Debug.LogError("Invalid trait Effect (Null)"); }
                        catch (ArgumentException)
                        { Debug.LogErrorFormat("Invalid trait Effect (duplicate ID) \"{0}\"", traitEffect.name); }
                    }
                }
                numDict = dictOfTraitEffects.Count;
                Debug.LogFormat("[Loa] InitialiseStart -> dictOfTraitEffects has {0} entries{1}", numDict, "\n");
                Debug.Assert(dictOfTraitEffects.Count == counter, "Mismatch on count");
                Debug.Assert(dictOfTraitEffects.Count > 0, "No Trait Effects imported to dictionary");
                Debug.Assert(numArray == numDict, string.Format("Mismatch in TraitEffect count, array {0}, dict {1}", numArray, numDict));

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
            Debug.LogFormat("[Loa] InitialiseStart -> dictOfSecretTypes has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict > 0, "No SecretTypes in dictOfSecretTypes");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on SecretType count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfecretTypes (Null) -> Import failed"); }
        //
        // - - - Node Datapoints - - -
        //
        numArray = arrayOfNodeDatapoints.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfNodeDatapoints has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No NodeDatapoints present"); }
    }
    #endregion

    #region InitialiseEarly

    /// <summary>
    /// default main constructor
    /// </summary>
    public void InitialiseEarly(GameState state)
    {
        int numArray, numDict, counter;
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
                    //assign a zero based unique ID number (Hard coded so that it's the same for every session / save game file)
                    NodeArc nodeArc = arrayOfNodeArcs[i];
                    switch (nodeArc.name)
                    {
                        case "CORPORATE":
                            nodeArc.nodeArcID = 0;
                            break;
                        case "GATED":
                            nodeArc.nodeArcID = 1;
                            break;
                        case "GOVERNMENT":
                            nodeArc.nodeArcID = 2;
                            break;
                        case "INDUSTRIAL":
                            nodeArc.nodeArcID = 3;
                            break;
                        case "RESEARCH":
                            nodeArc.nodeArcID = 4;
                            break;
                        case "SPRAWL":
                            nodeArc.nodeArcID = 5;
                            break;
                        case "UTILITY":
                            nodeArc.nodeArcID = 6;
                            break;
                        default:
                            Debug.LogErrorFormat("Unrecognised nodeArc \"{0}\"", nodeArc.name);
                            break;
                    }
                    counter++;
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
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfNodeArcs has {0} entries{1}", numDict, "\n");
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfLookUpNodeArcs has {0} entries{1}", dictOfLookUpNodeArcs.Count, "\n");
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
        Dictionary<string, NodeCrisis> dictOfNodeCrisis = GameManager.instance.dataScript.GetDictOfNodeCrisis();
        if (dictOfNodeCrisis != null)
        {
            counter = 0;
            numArray = arrayOfNodeCrisis.Length;
            if (numArray > 0)
            {
                for (int i = 0; i < numArray; i++)
                {
                    NodeCrisis nodeCrisis = arrayOfNodeCrisis[i];
                    counter++;
                    //add to dictionary
                    try
                    { dictOfNodeCrisis.Add(nodeCrisis.name, nodeCrisis); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid NodeCrisis (Null)"); counter--; }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid NodeCrisis (duplicate) \"{0}\" for  \"{1}\"", counter, nodeCrisis.name)); counter--; }
                }
            }
            numDict = dictOfNodeCrisis.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfNodeCrisis has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No Node Crisis have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch on NodeCrisis count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfNodeCrisis (Null) -> Import failed"); }
        //
        // - - - Traits - - -
        //
        Dictionary<string, Trait> dictOfTraits = GameManager.instance.dataScript.GetDictOfTraits();
        List<Trait> listOfAllTraits = GameManager.instance.dataScript.GetListOfAllTraits();
        if (dictOfTraits != null)
        {
            if (listOfAllTraits != null)
            {
                counter = 0;
                numArray = arrayOfTraits.Length;
                for (int i = 0; i < numArray; i++)
                {
                    Trait trait = arrayOfTraits[i];
                    counter++;
                    //add to dictionary
                    try
                    {
                        dictOfTraits.Add(trait.name, trait);
                        //add to list
                        listOfAllTraits.Add(trait);
                    }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid Trait (Null)"); counter--; }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid Trait (duplicate) ID \"{0}\" for \"{1}\"", counter, trait.name)); counter--; }
                }
                numDict = dictOfTraits.Count;
                Debug.LogFormat("[Loa] InitialiseEarly -> dictOfTraits has {0} entries{1}", numDict, "\n");
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
        Dictionary<string, ActorArc> dictOfActorArcs = GameManager.instance.dataScript.GetDictOfActorArcs();
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
                        counter++;
                        //add to dictionary
                        try
                        {
                            dictOfActorArcs.Add(arc.name, arc);
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
                    Debug.LogFormat("[Loa] InitialiseEarly -> dictOfActorArcs has {0} entries{1}", numDict, "\n");
                    Debug.LogFormat("[Loa] InitialiseEarly -> listOfAuthorityActorArcs has {0} entries{1}", authorityActorArcs.Count, "\n");
                    Debug.LogFormat("[Loa] InitialiseEarly -> listOfResistanceActorArcs has {0} entries{1}", resistanceActorArcs.Count, "\n");
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
        // - - - Targets - - -
        //            
        Debug.Assert(arrayOfTargetsGeneric.Length > 0, "Invalid arrayOfTargetsGeneric (no records)");
        Debug.Assert(arrayOfTargetsCity.Length > 0, "Invalid arrayOfTargetsCity (no records)");
        Debug.Assert(arrayOfTargetsVIP.Length > 0, "Invalid arrayOfTargetsVIP (no records)");
        Debug.Assert(arrayOfTargetsStory.Length > 0, "Invalid arrayOfTargetsStory (no records)");
        Debug.Assert(arrayOfTargetsGoal.Length > 0, "Invalid arrayOfTargetsGoal (no records)");
        Dictionary<string, Target> dictOfTargets = GameManager.instance.dataScript.GetDictOfTargets();
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
                Target target = listOfTargets[i];
                //set data
                counter++;
                target.targetStatus = Status.Dormant;
                //target.timer = -1;
                target.intel = 1;
                target.isKnownByAI = false;
                target.nodeID = -1;
                target.ongoingID = -1;
                //assign baseProfile to working profile
                target.profile = target.profileBase;
                //add to dictionary
                try
                { dictOfTargets.Add(target.name, target); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Target (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Target (duplicate) ID \"{0}\" for \"{1}\"", counter, target.targetName)); counter--; }
            }
            numDict = dictOfTargets.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfTargets has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on count");
            Debug.Assert(numDict > 0, "No Targets have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Targets count, array {0}, dict {1}", numArray, numDict));

            /*//initialise Generic target array -> EDIT: moved to TargetManager.cs -> Initialise -> InitialiseGenericTargetArray*/

        }
        else { Debug.LogError("Invalid dictOfTargets (Null) -> Import failed"); }
        //
        // - - - Sprites - - -
        //
        Debug.Assert(arrayOfGearSprites.Length > 0, "Invalid arrayOfGearSprites (No records)");
        Debug.Assert(arrayOfGlobalSprites.Length > 0, "Invalid arrayOfGlobalSprites (No records)");
        Debug.Assert(arrayOfTeamSprites.Length > 0, "Invalid arrayOfTeamSprites (No records)");
        Debug.Assert(arrayOfNodeArcSprites.Length > 0, "Invalid arrayOfNodeArcSprites (No records)");
        Debug.Assert(arrayOfTargetSprites.Length > 0, "Invalid arrayOfTargetSprites (No records)");
        Debug.Assert(arrayOfPortraitSprites.Length > 0, "Invalid arrayOfPortraitSprites (No records)");
        Dictionary<string, Sprite> dictOfSprites = GameManager.instance.dataScript.GetDictOfSprites();
        if (dictOfSprites != null)
        {
            counter = 0;
            //all sprites that need to be serialized (their names are serialized and they are retrieved from the dict on load)
            List<Sprite> listOfSprites = new List<Sprite>();
            listOfSprites.AddRange(arrayOfGearSprites);
            listOfSprites.AddRange(arrayOfGlobalSprites);
            listOfSprites.AddRange(arrayOfTeamSprites);
            listOfSprites.AddRange(arrayOfNodeArcSprites);
            listOfSprites.AddRange(arrayOfTargetSprites);
            listOfSprites.AddRange(arrayOfPortraitSprites);
            numArray = listOfSprites.Count;
            for (int i = 0; i < numArray; i++)
            {
                //add to dictionary
                Sprite sprite = listOfSprites[i];
                if (sprite != null)
                {
                    try
                    { dictOfSprites.Add(sprite.name, sprite); counter++; }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Duplicate sprite name \"{0}\" for listOfSprites[{1}]", sprite.name, i); }
                }
                else { Debug.LogWarningFormat("Invalid sprite (Null) for listOfSprites[{0}]", i); }
            }
            numDict = dictOfSprites.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfSprites has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on count");
            Debug.Assert(numDict > 0, "No Sprites have been imported");
            Debug.AssertFormat(numArray == numDict, "Mismatch in Sprites count, array {0}, dict {1}", numArray, numDict);
        }
        else { Debug.LogError("Invalid dictOfSprites (Null)"); }

        //
        // - - - Actions - - -
        //
        Dictionary<string, Action> dictOfActions = GameManager.instance.dataScript.GetDictOfActions();
        if (dictOfActions != null)
        {
                counter = 0;
                numArray = arrayOfActions.Length;
                for (int i = 0; i < numArray; i++)
                {
                    Action action = arrayOfActions[i];
                    counter++;
                    //add to dictionary
                    try
                    { dictOfActions.Add(action.name, action); }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid Action Arc (Null)"); counter--; }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid (duplicate) for Action \"{0}\"", action.name)); counter--; }
                }
                numDict = dictOfActions.Count;
                Debug.LogFormat("[Loa] InitialiseEarly -> dictOfActions has {0} entries{1}", numDict, "\n");
                Debug.Assert(numDict == counter, "Mismatch on count");
                Debug.Assert(numDict > 0, "No Actions have been imported");
                Debug.Assert(numArray == numDict, string.Format("Mismatch in Action count, array {0}, dict {1}", numArray, numDict));
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
                    //assign a zero based unique ID number (hard coded)
                    TeamArc teamArc = arrayOfTeamArcs[i];
                    //set data teamArcID
                    switch (teamArc.name)
                    {
                        case "CIVIL":
                            teamArc.TeamArcID = 0;
                            break;
                        case "CONTROL":
                            teamArc.TeamArcID = 1;
                            break;
                        case "DAMAGE":
                            teamArc.TeamArcID = 2;
                            break;
                        case "ERASURE":
                            teamArc.TeamArcID = 3;
                            break;
                        case "MEDIA":
                            teamArc.TeamArcID = 4;
                            break;
                        case "PROBE":
                            teamArc.TeamArcID = 5;
                            break;
                        case "SPIDER":
                            teamArc.TeamArcID = 6;
                            break;
                    }
                    counter++;
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
                Debug.LogFormat("[Loa] InitialiseEarly -> dictOfTeamArcs has {0} entries{1}", numDict, "\n");
                Debug.LogFormat("[Loa] InitialiseEarly -> dictOfLookUpTeamArcs has {0} entries{1}", dictOfLookUpTeamArcs.Count, "\n");
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
        Dictionary<string, Gear> dictOfGear = GameManager.instance.dataScript.GetDictOfGear();
        if (dictOfGear != null)
        {
            counter = 0;
            numArray = arrayOfGear.Length;
            for (int i = 0; i < numArray; i++)
            {
                Gear gear = arrayOfGear[i];
                counter++;
                //add to dictionary
                try
                { dictOfGear.Add(gear.name, gear); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Gear (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Gear (duplicate) \"{0}\" for \"{1}\"", counter, gear.name)); counter--; }
            }
            numDict = dictOfGear.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfGear has {0} entries{1}", numDict, "\n");
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
            Debug.Log(string.Format("[Loa] InitialiseEarly -> listOfGearRarity has {0} entries{1}", numDict, "\n"));
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
            Debug.Log(string.Format("[Loa] InitialiseEarly -> listOfGearType has {0} entries{1}", numDict, "\n"));
            Debug.Assert(numArray == numDict, string.Format("Mismatch in GearType count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid listOfGearType (Null) -> Import failed"); }
        //
        // - - - Topic Types - - -
        //
        Dictionary<string, TopicTypeData> dictOfTopicTypes = GameManager.instance.dataScript.GetDictOfTopicTypeData();
        List<TopicType> listOfTopicTypes = GameManager.instance.dataScript.GetListOfTopicTypes();
        if (dictOfTopicTypes != null)
        {
            if (listOfTopicTypes != null)
            {
                counter = 0;
                numArray = arrayOfTopicTypes.Length;
                for (int i = 0; i < numArray; i++)
                {
                    TopicType topicType = arrayOfTopicTypes[i];
                    counter++;
                    //create dataPackage (minInterval assigned during topicManager.cs -> subInitialiseStartUp
                    TopicTypeData data = new TopicTypeData()
                    {
                        type = topicType.name,
                        isAvailable = true,
                        turnLastUsed = 0,
                        minInterval = 0,
                        timesUsedLevel = 0,
                        timesUsedCampaign = 0
                    };
                    //add to dictionary & list
                    try
                    {
                        dictOfTopicTypes.Add(topicType.name, data);
                        listOfTopicTypes.Add(topicType);
                    }
                    catch (ArgumentNullException)
                    { Debug.LogError("Invalid TopicType (Null)"); counter--; }
                    catch (ArgumentException)
                    { Debug.LogError(string.Format("Invalid TopicType.name (duplicate) \"{0}\" for \"{1}\"", counter, topicType.name)); counter--; }
                }
                numDict = dictOfTopicTypes.Count;
                Debug.LogFormat("[Loa] InitialiseEarly -> dictOfTopicTypes has {0} entries{1}", numDict, "\n");
                Debug.Assert(numDict == counter, "Mismatch on Count");
                Debug.Assert(numDict > 0, "No TopicType has been imported");
                Debug.Assert(numArray == numDict, string.Format("Mismatch in TopicType count, array {0}, dict {1}", numArray, numDict));
            }
            else { Debug.LogError("Invalid listOfTopicTypes (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfTopicType (Null) -> Import failed"); }
        //
        // - - - Topic SubTypes - - -
        //
        Dictionary<string, TopicTypeData> dictOfTopicSubTypes = GameManager.instance.dataScript.GetDictOfTopicSubTypeData();
        if (dictOfTopicSubTypes != null)
        {
            counter = 0;
            numArray = arrayOfTopicSubTypes.Length;
            for (int i = 0; i < numArray; i++)
            {
                TopicSubType topicSubType = arrayOfTopicSubTypes[i];
                counter++;
                //create dataPackage
                TopicTypeData data = new TopicTypeData()
                {
                    type = topicSubType.name,
                    parent = topicSubType.type.name,
                    isAvailable = true,
                    turnLastUsed = 0,
                    minInterval = topicSubType.minIntervalFactor,
                    timesUsedLevel = 0,
                    timesUsedCampaign = 0
                };
                //add to dictionary
                try
                { dictOfTopicSubTypes.Add(topicSubType.name, data); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid TopicSubType (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid TopicSubType.name (duplicate) \"{0}\" for \"{1}\"", counter, topicSubType.name)); counter--; }
            }
            numDict = dictOfTopicSubTypes.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfTopicSubTypes has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No TopicSubType has been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in TopicSubType count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfTopicSubType (Null) -> Import failed"); }
        //
        // - - - Topic Options - - -
        //
        Dictionary<string, TopicOption> dictOfTopicOptions = GameManager.instance.dataScript.GetDictOfTopicOptions();
        if (dictOfTopicOptions != null)
        {
            counter = 0;
            numArray = arrayOfTopicOptions.Length;
            for (int i = 0; i < numArray; i++)
            {
                TopicOption topicOption = arrayOfTopicOptions[i];
                counter++;
                //add to dictionary
                try
                { dictOfTopicOptions.Add(topicOption.name, topicOption); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid TopicOption (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid TopicOption.name (duplicate) \"{0}\" for \"{1}\"", counter, topicOption.name)); counter--; }
            }
            numDict = dictOfTopicOptions.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfTopicOptions has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No TopicOption has been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in TopicOption count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfTopicOptions (Null)"); }
        //
        // - - - Topic Options (not stored in a collection)
        //
        numArray = arrayOfTopicOptions.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfTopicOptions has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No TopicOptions present"); }
        //
        // - - - Topic Pools (not stored in a collection)
        //
        numArray = arrayOfTopicPools.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfTopicPools has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No TopicPools present"); }
        //
        // - - - Topics - - -
        //
        Dictionary<string, Topic> dictOfTopics = GameManager.instance.dataScript.GetDictOfTopics();
        if (dictOfTopics != null)
        {
            counter = 0;
            numArray = arrayOfTopics.Length;
            for (int i = 0; i < numArray; i++)
            {
                Topic topic = arrayOfTopics[i];
                counter++;
                //add to dictionary
                try
                { dictOfTopics.Add(topic.name, topic); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Topic (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Topic.name (duplicate) \"{0}\" for \"{1}\"", counter, topic.name)); counter--; }
            }
            numDict = dictOfTopics.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfTopics has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No Topic has been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Topic count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfTopics (Null)"); }
        //
        // - - - Manage Actors - - -
        //
        numArray = arrayOfManageActors.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfManageActors has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No ManageActors present"); }
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
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfManageActions has {0} entries{1}", dictOfManageActions.Count, "\n");
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
        Dictionary<string, ActorConflict> dictOfActorConflicts = GameManager.instance.dataScript.GetDictOfActorConflicts();
        if (dictOfActorConflicts != null)
        {
            counter = 0;
            numArray = arrayOfActorConflicts.Length;
            for (int i = 0; i < numArray; i++)
            {
                ActorConflict conflict = arrayOfActorConflicts[i];
                counter++;
                //add to dictionary
                try
                { dictOfActorConflicts.Add(conflict.name, conflict); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid ActorConflict (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid ActorConflict (duplicate) \"{0}\" for \"{1}\"", counter, conflict.name)); counter--; }
            }
            numDict = dictOfActorConflicts.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfActorConflicts has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch in count");
            Debug.Assert(numDict > 0, "No ActorConflicts imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in ActorConflict count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfActorConflicts (Null) -> Import failed"); }

        //
        // - - - Secrets - - -
        //
        Dictionary<string, Secret> dictOfSecrets = GameManager.instance.dataScript.GetDictOfSecrets();
        if (dictOfSecrets != null)
        {
            counter = 0;
            numArray = arrayOfSecrets.Length;
            for (int i = 0; i < numArray; i++)
            {
                Secret secret = arrayOfSecrets[i];
                counter++;
                //add to dictionary
                try
                { dictOfSecrets.Add(secret.name, secret); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Secret (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Secret (duplicate) \"{0}\" for \"{1}\"", counter, secret.name)); counter--; }
                //add to list
            }
            numDict = dictOfSecrets.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfSecrets has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch in count");
            Debug.Assert(numDict > 0, "No Secrets imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Secrets count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfSecrets (Null) -> Import failed"); }
        //
        // - - - Factions - - - (done higher as no dictionary requirement)
        //
        Dictionary<string, Faction> dictOfFactions = GameManager.instance.dataScript.GetDictOfFactions();
        if (dictOfFactions != null)
        {
            counter = 0;
            numArray = arrayOfFactions.Length;
            for (int i = 0; i < numArray; i++)
            {
                Faction faction = arrayOfFactions[i];
                counter++;
                //add to dictionary
                try
                { dictOfFactions.Add(faction.name, faction); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Faction (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Faction (duplicate) \"{0}\"", counter, faction.name)); counter--; }
            }
            numDict = dictOfFactions.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfFactions has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on count");
            Debug.Assert(numDict > 0, "No Factions have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Faction count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfFactions (Null) -> Import failed"); }
        //
        // - - - Cities - - -
        //
        Dictionary<string, City> dictOfCities = GameManager.instance.dataScript.GetDictOfCities();
        if (dictOfCities != null)
        {
            counter = 0;
            numArray = arrayOfCities.Length;
            for (int i = 0; i < numArray; i++)
            {
                City city = arrayOfCities[i];
                counter++;
                //add to dictionary
                try
                { dictOfCities.Add(city.name, city); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid City (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid City (duplicate) ID \"{0}\" for \"{1}\"", counter, city.name)); counter--; }
            }
            numDict = dictOfCities.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfCities has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No cities have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in City count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfCities (Null) -> Import failed"); }
        //
        // - - - Objectives - - -
        //
        Dictionary<string, Objective> dictOfObjectives = GameManager.instance.dataScript.GetDictOfObjectives();
        if (dictOfObjectives != null)
        {
            counter = 0;
            numArray = arrayOfObjectives.Length;
            for (int i = 0; i < numArray; i++)
            {
                Objective objective = arrayOfObjectives[i];
                counter++;
                //add to dictionary
                try
                { dictOfObjectives.Add(objective.name, objective); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Objective (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Objective (duplicate) ID \"{0}\" for \"{1}\"", counter, objective.name)); counter--; }
            }
            numDict = dictOfObjectives.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfObjectives has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No Objectives have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Objective count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfObjectives (Null) -> Import failed"); }
        //
        // - - - Organisations - - -
        //
        Dictionary<string, Organisation> dictOfOrganisations = GameManager.instance.dataScript.GetDictOfOrganisations();
        if (dictOfOrganisations != null)
        {
            counter = 0;
            numArray = arrayOfOrganisations.Length;
            for (int i = 0; i < numArray; i++)
            {
                Organisation organisation = arrayOfOrganisations[i];
                //set data
                counter++;
                //add to dictionary
                try
                { dictOfOrganisations.Add(organisation.name, organisation); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Organisation (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Organisation (duplicate) \"{0}\" for \"{1}\"", counter, organisation.name)); counter--; }
            }
            numDict = dictOfOrganisations.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfOrganisations has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No Organisations have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Organisation count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfOrganisations (Null) -> Import failed"); }
        //
        // - - - Mayors - - -
        //
        Dictionary<string, Mayor> dictOfMayors = GameManager.instance.dataScript.GetDictOfMayors();
        if (dictOfMayors != null)
        {
            counter = 0;
            numArray = arrayOfMayors.Length;
            for (int i = 0; i < numArray; i++)
            {
                Mayor mayor = arrayOfMayors[i];
                //set data
                counter++;
                //add to dictionary
                try
                {
                    dictOfMayors.Add(mayor.name, mayor);
                    //initialise trait
                    if (mayor.trait != null)
                    { mayor.AddTrait(mayor.trait); }
                }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Mayor (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Mayor (duplicate) \"{0}\" for \"{1}\"", counter, mayor.mayorName)); counter--; }
            }
            numDict = dictOfMayors.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfMayors has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch in count");
            Debug.Assert(numDict > 0, "No Mayors have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Mayor count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfMayors (Null) -> Import failed"); }
        //
        // - - - AI Decisions - - -
        //
        Dictionary<string, DecisionAI> dictOfAIDecisions = GameManager.instance.dataScript.GetDictOfAIDecisions();
        if (dictOfAIDecisions != null)
        {
            counter = 0;
            numArray = arrayOfDecisionAI.Length;
            for (int i = 0; i < numArray; i++)
            {
                DecisionAI decisionAI = arrayOfDecisionAI[i];
                counter++;
                //add to main dictionary
                try
                { dictOfAIDecisions.Add(decisionAI.name, decisionAI); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid decisionAI (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid decisionAI (duplicate) ID \"{0}\" for \"{1}\"", counter, decisionAI.name)); counter--; }
            }
            numDict = dictOfAIDecisions.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfAIDecisions has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch in count");
            Debug.Assert(numDict > 0, "No AI Decisions have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in DecisionAI count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfAIDecisions (Null) -> Import failed"); }
        //
        // - - - Campaigns - - -
        //
        Dictionary<string, Campaign> dictOfCampaigns = GameManager.instance.dataScript.GetDictOfCampaigns();
        if (dictOfCampaigns != null)
        {
            counter = 0;
            numArray = arrayOfCampaigns.Length;
            for (int i = 0; i < numArray; i++)
            {
                Campaign campaign = arrayOfCampaigns[i];
                counter++;
                //add to dictionary
                try
                { dictOfCampaigns.Add(campaign.name, campaign); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Campaign (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid Campaign (duplicate) ID \"{0}\" for \"{1}\"", counter, campaign.name)); counter--; }
            }
            numDict = dictOfCampaigns.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfCampaigns has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No Campaigns have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Campaign count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfCampaigns (Null) -> Import failed"); }
    }

    #endregion

    #region InitialiseLate

    /// <summary>
    /// Stuff that is done after LevelManager.SetUp
    /// Note: DataManager.cs InitialiseLate runs immediately prior to this and sets up node arrays and lists
    /// </summary>
    public void InitialiseLate(GameState state)
    {
        int counter;
        //
        // - - - Nodes - - -
        //
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        if (dictOfNodes != null)
        {
            counter = 0;
            /*//clear dictionary (may be a followOn level) -> done in DataManager.Reset
            dictOfNodes.Clear();*/
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
                Debug.LogFormat("[Loa] InitialiseLate -> dictOfNodes has {0} entries{1}", dictOfNodes.Count, "\n");
                Debug.Assert(dictOfNodes.Count == counter, "Mismatch in Count");
                Debug.Assert(dictOfNodes.Count > 0, "No Nodes have been imported");
                //create List Of Nodes for iteration purposes
                if (listOfNodes != null)
                {
                    /*listOfNodes.Clear();*/
                    listOfNodes.AddRange(dictOfNodes.Values.ToList());
                    Debug.LogFormat("[Loa] InitialiseLate -> listOfNodes has {0} entries{1}", listOfNodes.Count, "\n");
                    Debug.Assert(dictOfNodes.Count == listOfNodes.Count, "Mismatch on count between dictOfNodes and listOfNodes");
                }
                else { Debug.LogError("Invalid listOfNodes (Null)"); }
            }
            else { Debug.LogError("Invalid listOfNodes (Null) from LevelManager"); }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null) -> Import failed"); }
        //
        // - - - Help - - -
        //
        Dictionary<string, HelpData> dictOfHelp = GameManager.instance.dataScript.GetDictOfHelpData();
        //load only if not already present
        if (dictOfHelp.Count == 0)
        {
            List<HelpData> listOfHelp = GameManager.instance.helpScript.CreateItemDataHelp();
            int count = listOfHelp.Count;
            if (count > 0)
            {
                counter = 0;
                for (int index = 0; index < count; index++)
                {
                    //add to dictionary
                    try { dictOfHelp.Add(listOfHelp[index].tag, listOfHelp[index]); counter++; }
                    catch (ArgumentNullException)
                    { Debug.LogErrorFormat("Invalid HelpData (Null) for listOfHelp[{0}]", index); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Invalid HelpData (duplicate) tag \"{0}\" for listOfHelp[\"{1}\"]", listOfHelp[index].tag, index); }
                }
                Debug.Assert(dictOfHelp.Count == count, "Mismatch on count between dictOfHelp and listOfHelp");
                Debug.LogFormat("[Loa] InitialiseLate -> listOfHelp has {0} entries{1}", counter, "\n");
            }
            else { Debug.LogError("Invalid listOfHelp (Empty)"); }
        }
    }

    #endregion

    //new methods above here
}
