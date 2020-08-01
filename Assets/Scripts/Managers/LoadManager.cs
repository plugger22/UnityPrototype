using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// handles loading of all design time SO's
/// </summary>
public class LoadManager : MonoBehaviour
{

    #region Arrays
    [Header("Initialise Start")]
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
    public Npc[] arrayOfNpcs;
    public NpcNode[] arrayOfNpcNodes;
    public NpcAction[] arrayOfNpcActions;
    public OrgType[] arrayOfOrgTypes;
    public HqPosition[] arrayOfHqPositions;
    public CaptureTool[] arrayOfCaptureTools;
    public StoryModule[] arrayOfStoryModules;

    [Header("InitialiseStart")]
    public Condition[] arrayOfConditions;
    public Cure[] arrayOfCures;
    public TraitCategory[] arrayOfTraitCategories;
    public TraitEffect[] arrayOfTraitEffects;
    public SecretType[] arrayOfSecretTypes;
    public NodeDatapoint[] arrayOfNodeDatapoints;

    [Header("TextLists")]       //NOTE: need to add any new arrays to arrayOfTextLists in InitialiseEarly
    public TextList[] arrayOfContactTextLists;
    public TextList[] arrayOfNameTextLists;
    public TextList[] arrayOfDistrictTextLists;
    public TextList[] arrayOfShortTextLists;
    public TextList[] arrayOfFactorTextLists;
    public TextList[] arrayOfAdvertTextLists;
    public TextList[] arrayOfCrisisTextLists;

    [Header("Consolidated TextLists")]
    public NameSet[] arrayOfNameSets;

    [Header("Personality Factors - ORDER MATTERS")]
    public Factor[] arrayOfFiveFactorModel;

    [Header("Personality")]
    public PersonProfile[] arrayOfPersonProfiles;
    public Belief[] arrayOfBeliefs;

    [Header("Effects")]
    public Effect[] arrayOfEffectsAI;
    public Effect[] arrayOfEffectsAuthority;
    public Effect[] arrayOfEffectsAutoRecall;
    public Effect[] arrayOfEffectsConditions;
    public Effect[] arrayOfEffectsGeneral;
    public Effect[] arrayOfEffectsGroup;
    public Effect[] arrayOfEffectsManage;
    public Effect[] arrayOfEffectsMetaOptions;
    public Effect[] arrayOfEffectsPersonality;
    public Effect[] arrayOfEffectsPlayer;
    public Effect[] arrayOfEffectsResistance;
    public Effect[] arrayOfEffectsTeams;
    public Effect[] arrayOfEffectsTopics;

    [Header("InitialiseEarly")]
    public NodeArc[] arrayOfNodeArcs;
    public NodeCrisis[] arrayOfNodeCrisis;
    public Trait[] arrayOfTraits;
    public ActorArc[] arrayOfActorArcs;
    public Action[] arrayOfActions;
    public TeamArc[] arrayOfTeamArcs;
    public Gear[] arrayOfGear;
    public GearRarity[] arrayOfGearRarity;
    public GearType[] arrayOfGearType;

    [Header("Topics")]
    public TopicType[] arrayOfTopicTypes;
    public TopicSubType[] arrayOfTopicSubTypes;
    public TopicSubSubType[] arrayOfTopicSubSubTypes;
    public TopicProfile[] arrayOfTopicProfiles;
    public TopicPool[] arrayOfTopicPools;
    public TopicScope[] arrayOfTopicScopes;
    public TopicGroupType[] arrayOfTopicGroupTypes;
    public Topic[] arrayOfTopics;

    [Header("Topic Options")]
    public TopicOption[] arrayOfOptionsActorContact;
    public TopicOption[] arrayOfOptionsActorDistrict;
    public TopicOption[] arrayOfOptionsActorMatch;
    public TopicOption[] arrayOfOptionsActorGear;
    public TopicOption[] arrayOfOptionsActorPolitics;
    public TopicOption[] arrayOfOptionsAuthorityTeams;
    public TopicOption[] arrayOfOptionsCity;
    public TopicOption[] arrayOfOptionsHQ;
    public TopicOption[] arrayOfOptionsCapture;
    public TopicOption[] arrayOfOptionsPlayerConditions;
    public TopicOption[] arrayOfOptionsPlayerDistrict;
    public TopicOption[] arrayOfOptionsPlayerGeneral;
    public TopicOption[] arrayOfOptionsPlayerGear;
    public TopicOption[] arrayOfOptionsPlayerStats;
    public TopicOption[] arrayOfOptionsOrgCure;
    public TopicOption[] arrayOfOptionsOrgContract;
    public TopicOption[] arrayOfOptionsOrgHQ;
    public TopicOption[] arrayOfOptionsOrgEmergency;
    public TopicOption[] arrayOfOptionsOrgInfo;
    public TopicOption[] arrayOfOptionsStoryBrother;
    public TopicOption[] arrayOfOptionsStoryDrugs;
    public TopicOption[] arrayOfOptionsStoryImperator;
    public TopicOption[] arrayOfOptionsStoryLunaHub;
    public TopicOption[] arrayOfOptionsStoryParents;
    public TopicOption[] arrayOfOptionsStoryPurplePig;

    [Header("Targets")]
    public Target[] arrayOfTargetsGeneric;
    public Target[] arrayOfTargetsCity;
    public Target[] arrayOfTargetsVIP;
    public Target[] arrayOfTargetsStory;
    public Target[] arrayOfTargetsGoal;
    public Target[] arrayOfTargetsOrg;

    [Header("Sprites")]
    public Sprite[] arrayOfGearSprites;
    public Sprite[] arrayOfGlobalSprites;
    public Sprite[] arrayOfNodeArcSprites;
    public Sprite[] arrayOfPortraitSprites;
    public Sprite[] arrayOfTargetSprites;
    public Sprite[] arrayOfTeamSprites;

    [Header("MetaOptions")]
    public MetaOption[] arrayOfBossOptions;
    public MetaOption[] arrayOfSubBoss1Options;
    public MetaOption[] arrayOfSubBoss2Options;
    public MetaOption[] arrayOfSubBoss3Options;

    [Header("InitialiseEarly")]
    public ManageActor[] arrayOfManageActors;
    public ManageAction[] arrayOfManageActions;
    public ActorConflict[] arrayOfActorConflicts;
    public Secret[] arrayOfSecrets;
    public Hq[] arrayOfHQs;
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


    //Consolidated arrays
    //NOTE: consolidation needs to happen in InitialiseStart for sequencing reasons
    //NOTE: consolidate arrays into the master array in InitialiseStart and do the processing later in InitialiseEarly
    [HideInInspector] public Effect[] arrayOfEffects;
    [HideInInspector] public TextList[] arrayOfTextLists;
    [HideInInspector] public Target[] arrayOfTargets;
    [HideInInspector] public TopicOption[] arrayOfTopicOptions;
    [HideInInspector] public MetaOption[] arrayOfMetaOptions;
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
        GameManager.i.dataScript.SetFactorArrays(listOfFactors);
        Debug.LogFormat("[Loa] LoadManager.cs -> InitialiseStart: listOfFactors has {0} entries{1}", listOfFactors.Count, "\n");
        //
        // - - - PersonProfile
        //
        Dictionary<string, PersonProfile> dictOfProfiles = GameManager.i.dataScript.GetDictOfProfiles();
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
        // - - - Beliefs
        //
        Dictionary<string, int> dictOfBeliefs = GameManager.i.dataScript.GetDictOfBeliefs();
        if (dictOfBeliefs != null)
        {
            numArray = arrayOfBeliefs.Length;
            if (numArray > 0)
            { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfBeliefs has {0} entries{1}", numArray, "\n"); }
            else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No Beliefs present"); }
            //add to dictionary
            counter = 0;
            for (int i = 0; i < numArray; i++)
            {
                Belief belief = arrayOfBeliefs[i];
                if (belief != null)
                {
                    try
                    { dictOfBeliefs.Add(belief.name, 0); counter++; }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate record exists in dictOfBeliefs for {0}", belief); }
                }
                else { Debug.LogWarningFormat("Invalid Belief (Null) for arrayOfBelief[{0}]", i); }
            }
            numDict = dictOfBeliefs.Count;
            Debug.LogFormat("[Loa] InitialiseStart -> dictOfBeliefs has {0} entries{1}", numDict, "\n");
            Debug.Assert(dictOfBeliefs.Count == counter, "Mismatch on count");
            Debug.Assert(dictOfBeliefs.Count > 0, "No Beliefs imported to dictionary");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in Beliefs count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfBeliefs (Null)"); }
        //
        // - - - Topic Options (consolidate arrays)
        //
        List<TopicOption> listOfOptions = new List<TopicOption>();
        listOfOptions.AddRange(arrayOfOptionsActorContact);
        listOfOptions.AddRange(arrayOfOptionsActorDistrict);
        listOfOptions.AddRange(arrayOfOptionsActorMatch);
        listOfOptions.AddRange(arrayOfOptionsActorGear);
        listOfOptions.AddRange(arrayOfOptionsActorPolitics);
        listOfOptions.AddRange(arrayOfOptionsAuthorityTeams);
        listOfOptions.AddRange(arrayOfOptionsCity);
        listOfOptions.AddRange(arrayOfOptionsHQ);
        listOfOptions.AddRange(arrayOfOptionsCapture);
        listOfOptions.AddRange(arrayOfOptionsPlayerConditions);
        listOfOptions.AddRange(arrayOfOptionsPlayerDistrict);
        listOfOptions.AddRange(arrayOfOptionsPlayerGeneral);
        listOfOptions.AddRange(arrayOfOptionsPlayerStats);
        listOfOptions.AddRange(arrayOfOptionsPlayerGear);
        listOfOptions.AddRange(arrayOfOptionsOrgCure);
        listOfOptions.AddRange(arrayOfOptionsOrgContract);
        listOfOptions.AddRange(arrayOfOptionsOrgHQ);
        listOfOptions.AddRange(arrayOfOptionsOrgEmergency);
        listOfOptions.AddRange(arrayOfOptionsOrgInfo);
        listOfOptions.AddRange(arrayOfOptionsStoryBrother);
        listOfOptions.AddRange(arrayOfOptionsStoryDrugs);
        listOfOptions.AddRange(arrayOfOptionsStoryImperator);
        listOfOptions.AddRange(arrayOfOptionsStoryLunaHub);
        listOfOptions.AddRange(arrayOfOptionsStoryParents);
        listOfOptions.AddRange(arrayOfOptionsStoryPurplePig);
        arrayOfTopicOptions = listOfOptions.ToArray();
        //
        // - - - MetaOptions (consolidate arrays)
        //
        List<MetaOption> listOfMetaOptions = new List<MetaOption>();
        listOfMetaOptions.AddRange(arrayOfBossOptions);
        listOfMetaOptions.AddRange(arrayOfSubBoss1Options);
        listOfMetaOptions.AddRange(arrayOfSubBoss2Options);
        listOfMetaOptions.AddRange(arrayOfSubBoss3Options);
        arrayOfMetaOptions = listOfMetaOptions.ToArray();
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
        Dictionary<string, GlobalType> dictOfGlobalType = GameManager.i.dataScript.GetDictOfGlobalType();
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
        Dictionary<string, GlobalSide> dictOfGlobalSide = GameManager.i.dataScript.GetDictOfGlobalSide();
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
        List<Effect> listOfEffects = new List<Effect>();
        //consolidate all effect arrays into the single master array (which is used for validation and code purposes)
        listOfEffects.AddRange(arrayOfEffectsAI);
        listOfEffects.AddRange(arrayOfEffectsAuthority);
        listOfEffects.AddRange(arrayOfEffectsAutoRecall);
        listOfEffects.AddRange(arrayOfEffectsConditions);
        listOfEffects.AddRange(arrayOfEffectsGeneral);
        listOfEffects.AddRange(arrayOfEffectsGroup);
        listOfEffects.AddRange(arrayOfEffectsManage);
        listOfEffects.AddRange(arrayOfEffectsMetaOptions);
        listOfEffects.AddRange(arrayOfEffectsPersonality);
        listOfEffects.AddRange(arrayOfEffectsPlayer);
        listOfEffects.AddRange(arrayOfEffectsResistance);
        listOfEffects.AddRange(arrayOfEffectsTeams);
        listOfEffects.AddRange(arrayOfEffectsTopics);
        arrayOfEffects = listOfEffects.ToArray();
        numArray = arrayOfEffects.Length;
        Debug.LogFormat("[Loa] InitialiseStart -> arrayOfEffects has {0} entries{1}", numArray, "\n");
        //add to dictOfEffects
        Dictionary<string, Effect> dictOfEffects = GameManager.i.dataScript.GetDictOfEffects();
        if (dictOfEffects != null)
        {
            for (int i = 0; i < numArray; i++)
            {
                Effect effect = arrayOfEffects[i];
                if (effect != null)
                {
                    try { dictOfEffects.Add(effect.name, effect); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Duplicate record exists for arrayOfEffects[{0}], effect.name \"{1}\"", i, effect.name); }
                }
                else { Debug.LogErrorFormat("Invalid effect (Null) for arrayOfEffects[{0}]", i); }
            }
            Debug.AssertFormat(dictOfEffects.Count == numArray, "Mismatch for dictOfEffects ({0} records) and arrayOfEffects ({1} records)", dictOfEffects.Count, numArray);
        }
        else { Debug.LogError("Invalid dictOfEffects (Null)"); }
        //check master array
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
        // - - - Target (master array consolidate)
        //
        List<Target> listOfTargets = new List<Target>();
        listOfTargets.AddRange(arrayOfTargetsGeneric);
        listOfTargets.AddRange(arrayOfTargetsCity);
        listOfTargets.AddRange(arrayOfTargetsVIP);
        listOfTargets.AddRange(arrayOfTargetsStory);
        listOfTargets.AddRange(arrayOfTargetsGoal);
        listOfTargets.AddRange(arrayOfTargetsOrg);
        numArray = listOfTargets.Count;
        //master array
        arrayOfTargets = listOfTargets.ToArray();
        Debug.LogFormat("[Loa] InitialiseEarly: arrayOfTargets has {0} entries{1}", arrayOfTargets.Length, "\n");
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
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No FactorTextLists present"); }
        //
        // - - - Text Lists Advertisments (not stored in a collection)
        //
        numArray = arrayOfAdvertTextLists.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfAdvertTextLists has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No AdvertTextLists present"); }
        //
        // - - - Text Lists Advertisments (not stored in a collection)
        //
        numArray = arrayOfCrisisTextLists.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfCrisisTextLists has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No CrisisTextLists present"); }
        //
        // - - - Text Lists -> Consolidate into master array
        //
        List<TextList> listOfTextLists = new List<TextList>();
        //consolidate all into single master array (used for validation and code purposes)
        listOfTextLists.AddRange(arrayOfContactTextLists);
        listOfTextLists.AddRange(arrayOfNameTextLists);
        listOfTextLists.AddRange(arrayOfDistrictTextLists);
        listOfTextLists.AddRange(arrayOfShortTextLists);
        listOfTextLists.AddRange(arrayOfFactorTextLists);
        listOfTextLists.AddRange(arrayOfAdvertTextLists);
        listOfTextLists.AddRange(arrayOfCrisisTextLists);
        arrayOfTextLists = listOfTextLists.ToArray();
        //check master array
        numArray = arrayOfTextLists.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfTextLists has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning("LoadManager.cs -> InitialiseStart: No TextLists present"); }
        //add to dictOfTextsLists
        Dictionary<string, TextList> dictOfTextLists = GameManager.i.dataScript.GetDictOfTextList();
        foreach (TextList textList in arrayOfTextLists)
        {
            if (textList != null)
            {
                try { dictOfTextLists.Add(textList.name, textList); }
                catch (ArgumentException)
                { Debug.LogWarningFormat("Duplicate TextList {0} in dictOfTextLists", textList.name); }
            }
            else { Debug.LogWarning("Invalid textList (Null) in arrayOfTextLists"); }
        }
        Debug.AssertFormat(dictOfTextLists.Count == numArray, "Mismatch on Textlists. Array has {0}, dict has {1}", numArray, dictOfTextLists.Count);
        Debug.LogFormat("[Loa] InitialiseStart -> dictOfTextLists has {0} records{1}", dictOfTextLists.Count, "\n");
        //
        // - - - NameSets (not stored in a collection)
        //
        Dictionary<string, NameSet> dictOfNameSet = GameManager.i.dataScript.GetDictOfNameSet();
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
        // - - - Npc's (not stored in a collection)
        //
        numArray = arrayOfNpcs.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfNpcs has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No Npc's present"); }
        //
        // - - - NpcNodes (not stored in a collection)
        //
        numArray = arrayOfNpcNodes.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfNpcNodes has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No NpcNodes present"); }
        //
        // - - - NpcActions (not stored in a collection)
        //
        numArray = arrayOfNpcActions.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfNpcActions has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No NpcActions present"); }
        //
        // - - - OrgTypes (not stored in a collection)
        //
        numArray = arrayOfOrgTypes.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfOrgTypes has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No OrgTypes present"); }
        //
        // - - - HqPositions (not stored in a collection)
        //
        numArray = arrayOfHqPositions.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfHqPositions has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No HqPositions present"); }
        //
        // - - - CaptureTools (not stored in a collection)
        //
        numArray = arrayOfCaptureTools.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfCaptureTools has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No CaptureTools present"); }
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
            int numOfQualities = GameManager.i.preloadScript.numOfQualities;
            //resistance
            if (listResistance.Count == numOfQualities)
            {
                //order list then pass to dataManager.array
                IEnumerable<Quality> sortedList = listResistance.OrderBy(o => o.order);
                GameManager.i.dataScript.InitialiseResistanceQualities(sortedList);
            }
            else { Debug.LogWarning("Invalid listResistance (size different to numOfQualities)"); }
            //authority
            if (listAuthority.Count == numOfQualities)
            {
                //order list then pass to dataManager.array
                IEnumerable<Quality> sortedList = listAuthority.OrderBy(o => o.order);
                GameManager.i.dataScript.InitialiseAuthorityQualities(sortedList);
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
            Dictionary<string, Condition> dictOfConditions = GameManager.i.dataScript.GetDictOfConditions();
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
            Dictionary<string, Cure> dictOfCures = GameManager.i.dataScript.GetDictOfCures();
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
        Dictionary<string, TraitCategory> dictOfTraitCategories = GameManager.i.dataScript.GetDictOfTraitCategories();
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
        Dictionary<string, TraitEffect> dictOfTraitEffects = GameManager.i.dataScript.GetDictOfTraitEffects();
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
        Dictionary<string, SecretType> dictOfSecretTypes = GameManager.i.dataScript.GetDictOfSecretTypes();
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
        //
        // - - - Story Modules (not stored in a collection)
        //
        numArray = arrayOfStoryModules.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfStoryModules has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No StoryModules present"); }
    }
    #endregion

    #region InitialiseEarly
    /// <summary>
    /// default main constructor
    /// </summary>
    public void InitialiseEarly(GameState state)
    {
        int numArray, numDict, counter;
        GlobalSide globalAuthority = GameManager.i.globalScript.sideAuthority;
        GlobalSide globalResistance = GameManager.i.globalScript.sideResistance;
        //
        // - - - Node Arcs - - -
        //
        Dictionary<int, NodeArc> dictOfNodeArcs = GameManager.i.dataScript.GetDictOfNodeArcs();
        Dictionary<string, int> dictOfLookUpNodeArcs = GameManager.i.dataScript.GetDictOfLookUpNodeArcs();
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
        Dictionary<string, NodeCrisis> dictOfNodeCrisis = GameManager.i.dataScript.GetDictOfNodeCrisis();
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
        Dictionary<string, Trait> dictOfTraits = GameManager.i.dataScript.GetDictOfTraits();
        List<Trait> listOfAllTraits = GameManager.i.dataScript.GetListOfAllTraits();
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
                        //update hq fields
                        if (trait.hqMajorMultiplier > 0 || trait.hqMinorMultiplier > 0 || trait.hqRenownMultiplier > 0)
                        {
                            //can only be one, takes first one found and ignores the rest
                            trait.isHqTrait = true;
                            if (trait.hqMajorMultiplier > 0)
                            {
                                trait.hqDescription = string.Format("Chance of a Major (leave) event {0} % {1}",
                                  trait.hqMajorMultiplier > 1.0 ? (trait.hqMajorMultiplier - 1.0) * 100 : trait.hqMajorMultiplier * 100, trait.hqMajorMultiplier < 1.0f ? "less" : "more");
                            }
                            else if (trait.hqMinorMultiplier > 0)
                            {
                                trait.hqDescription = string.Format("Chance of a Good (change in renown) event {0} % {1}",
                                  trait.hqMinorMultiplier > 1.0 ? (trait.hqMinorMultiplier - 1.0) * 100 : trait.hqMinorMultiplier * 100, trait.hqMinorMultiplier < 1.0f ? "less" : "more");
                            }
                            else if (trait.hqRenownMultiplier > 0)
                            {
                                trait.hqDescription = string.Format("Renown gain, or loss, from an event is {0}% {1}",
                                  trait.hqRenownMultiplier > 1.0 ? (trait.hqRenownMultiplier - 1.0) * 100 : trait.hqRenownMultiplier * 100, trait.hqRenownMultiplier < 1.0f ? "less" : "more");
                            }
                        }
                        else
                        {
                            trait.isHqTrait = false;
                            trait.hqDescription = "no effect";
                        }
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
        Dictionary<string, ActorArc> dictOfActorArcs = GameManager.i.dataScript.GetDictOfActorArcs();
        List<ActorArc> authorityActorArcs = GameManager.i.dataScript.GetListOfAuthorityActorArcs();
        List<ActorArc> resistanceActorArcs = GameManager.i.dataScript.GetListOfResistanceActorArcs();
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
        Debug.AssertFormat(arrayOfTargetsOrg.Length == 1, "Invalid arrayOfTargetOrg (should be one OrgTemplate record only, there are {0})", arrayOfTargetsOrg.Length);
        Dictionary<string, Target> dictOfTargets = GameManager.i.dataScript.GetDictOfTargets();
        if (dictOfTargets != null)
        {
            counter = 0;

            /*List<Target> listOfTargets = new List<Target>();
            listOfTargets.AddRange(arrayOfTargetsGeneric);
            listOfTargets.AddRange(arrayOfTargetsCity);
            listOfTargets.AddRange(arrayOfTargetsVIP);
            listOfTargets.AddRange(arrayOfTargetsStory);
            listOfTargets.AddRange(arrayOfTargetsGoal);
            numArray = listOfTargets.Count;*/

            //use master array (InitialiseEarly)
            List<Target> listOfTargets = arrayOfTargets.ToList();
            numArray = listOfTargets.Count;
            Debug.LogFormat("[Loa] InitialiseEarly: arrayOfTargets has {0} entries{1}", arrayOfTargets.Length, "\n");
            //loop targets
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
        Dictionary<string, Sprite> dictOfSprites = GameManager.i.dataScript.GetDictOfSprites();
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
        // - - - MetaOptions - - -
        //
        Dictionary<string, MetaOption> dictOfMetaOptions = GameManager.i.dataScript.GetDictOfMetaOptions();
        if (dictOfMetaOptions != null)
        {
            List<MetaOption> listOfMetaOptions = arrayOfMetaOptions.ToList();
            Debug.LogFormat("[Loa] InitialiseEarly: arrayOfMetaOptions has {0} entries{1}", arrayOfMetaOptions.Length, "\n");
            //loop options
            counter = 0;
            numArray = listOfMetaOptions.Count;
            for (int i = 0; i < numArray; i++)
            {
                //add to dictionary
                MetaOption metaOption = listOfMetaOptions[i];
                if (metaOption != null)
                {
                    try
                    { dictOfMetaOptions.Add(metaOption.name, metaOption); counter++; }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Duplicate metaOption name \"{0}\" for listOfMetaOptions[{1}]", metaOption.name, i); }
                }
                else { Debug.LogWarningFormat("Invalid metaOption (Null) for listOfMetaOptions[{0}]", i); }
            }
            numDict = dictOfMetaOptions.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfMetaOptions has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on count");
            Debug.Assert(numDict > 0, "No MetaOptions have been imported");
            Debug.AssertFormat(numArray == numDict, "Mismatch in MetaOptions count, array {0}, dict {1}", numArray, numDict);
        }
        else { Debug.LogError("Invalid dictOfMetaOptions (Null)"); }
        //
        // - - - Actions - - -
        //
        Dictionary<string, Action> dictOfActions = GameManager.i.dataScript.GetDictOfActions();
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
        Dictionary<int, TeamArc> dictOfTeamArcs = GameManager.i.dataScript.GetDictOfTeamArcs();
        Dictionary<string, int> dictOfLookUpTeamArcs = GameManager.i.dataScript.GetDictOfLookUpTeamArcs();
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
                GameManager.i.dataScript.InitialiseArrayOfTeams(counter, (int)TeamInfo.Count);
            }
            else { Debug.LogError("Invalid dictOfLookUpTeamArcs (Null) -> Import failed"); }
        }
        else { Debug.LogError("Invalid dictOfTeamArcs (Null) -> Import failed"); }
        //
        // - - - Gear - - -
        //
        Dictionary<string, Gear> dictOfGear = GameManager.i.dataScript.GetDictOfGear();
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
        List<GearRarity> listOfGearRarity = GameManager.i.dataScript.GetListOfGearRarity();
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
        List<GearType> listOfGearType = GameManager.i.dataScript.GetListOfGearType();
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
        // - - - Topic Types Data - - -
        //
        Dictionary<string, TopicTypeData> dictOfTopicTypes = GameManager.i.dataScript.GetDictOfTopicTypeData();
        List<TopicType> listOfTopicTypes = GameManager.i.dataScript.GetListOfTopicTypes();
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
                Debug.LogFormat("[Loa] InitialiseEarly -> dictOfTopicTypesData has {0} entries{1}", numDict, "\n");
                Debug.Assert(numDict == counter, "Mismatch on Count");
                Debug.Assert(numDict > 0, "No TopicType has been imported");
                Debug.Assert(numArray == numDict, string.Format("Mismatch in TopicTypeData count, array {0}, dict {1}", numArray, numDict));
            }
            else { Debug.LogError("Invalid listOfTopicTypes (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfTopicType (Null) -> Import failed"); }
        //
        // - - - Topic SubTypes Data - - -
        //
        Dictionary<string, TopicTypeData> dictOfTopicSubTypes = GameManager.i.dataScript.GetDictOfTopicSubTypeData();
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
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfTopicSubTypesData has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No TopicSubType has been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in TopicSubTypeData count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfTopicSubType (Null) -> Import failed"); }
        //
        // - - - Topic Options - - -
        //
        Dictionary<string, TopicOption> dictOfTopicOptions = GameManager.i.dataScript.GetDictOfTopicOptions();
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
        // - - - TopicSubSubTypes (not stored in a collection)
        //
        numArray = arrayOfTopicSubSubTypes.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfTopicSubSubTypes has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No TopicSubSubTypes present"); }
        //
        // - - - Topic Profiles (not stored in a collection)
        //
        numArray = arrayOfTopicProfiles.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfTopicProfiles has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No TopicProfiles present"); }
        //
        // - - - Topic Scopes (not stored in a collection)
        //
        numArray = arrayOfTopicScopes.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfTopicScopes has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No TopicScopes present"); }
        //
        // - - - TopicGroupTypes (not stored in a collection)
        //
        numArray = arrayOfTopicGroupTypes.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] InitialiseStart -> arrayOfTopicGroupTypes has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" LoadManager.cs -> InitialiseStart: No TopicGroupTypes present"); }
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
        Dictionary<string, Topic> dictOfTopics = GameManager.i.dataScript.GetDictOfTopics();
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
        // - - - TopicSubTypes - - -
        //
        Dictionary<string, TopicSubType> dictOfTopicSubs = GameManager.i.dataScript.GetDictOfTopicSubTypes();
        if (dictOfTopicSubs != null)
        {
            counter = 0;
            numArray = arrayOfTopicSubTypes.Length;
            for (int i = 0; i < numArray; i++)
            {
                TopicSubType subType = arrayOfTopicSubTypes[i];
                counter++;
                //add to dictionary
                try
                { dictOfTopicSubs.Add(subType.name, subType); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid TopicSubType (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid TopicSubType.name (duplicate) \"{0}\" for \"{1}\"", counter, subType.name)); counter--; }
            }
            numDict = dictOfTopicSubs.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfTopicSubTypes has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on Count");
            Debug.Assert(numDict > 0, "No TopicSubType has been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in TopicSubType count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfTopicSubs (Null)"); }
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
        Dictionary<string, ManageAction> dictOfManageActions = GameManager.i.dataScript.GetDictOfManageActions();
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
                GameManager.i.dataScript.SetListOfActorHandle(listOfActorHandle);
            }
            else { Debug.LogError("There are no entries in listOfActorHandle"); }
            //ActorReserve
            if (listOfActorReserve.Count > 0)
            {
                var manageReserve = listOfActorReserve.OrderBy(o => o.order);
                listOfActorReserve = manageReserve.ToList();
                GameManager.i.dataScript.SetListOfActorReserve(listOfActorReserve);
                Debug.Assert(listOfActorReserve.Count > 0, "Invalid count (empty) for listOfActorReserve");
            }
            else { Debug.LogError("There are no entries in listOfActorReserve"); }
            //ActorDismiss
            if (listOfActorDismiss.Count > 0)
            {
                var manageDismiss = listOfActorDismiss.OrderBy(o => o.order);
                listOfActorDismiss = manageDismiss.ToList();
                GameManager.i.dataScript.SetListOfActorDismiss(listOfActorDismiss);
                Debug.Assert(listOfActorDismiss.Count > 0, "Invalid count (empty) for listOfActorDismiss");
            }
            else { Debug.LogError("There are no entries in listOfActorDismiss"); }
            //ActorDispose
            if (listOfActorDispose.Count > 0)
            {
                var manageDispose = listOfActorDispose.OrderBy(o => o.order);
                listOfActorDispose = manageDispose.ToList();
                GameManager.i.dataScript.SetListOfActorDispose(listOfActorDispose);
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
        Quality[] authorityQualities = GameManager.i.dataScript.GetArrayOfAuthorityQualities();
        Quality[] resistanceQualities = GameManager.i.dataScript.GetArrayOfResistanceQualities();
        if (authorityQualities != null)
        {
            if (resistanceQualities != null)
            {
                //initialise StatTags array
                GameManager.i.dataScript.InitialiseArrayOfStatTags();
                string[,] arrayOfStatTags = GameManager.i.dataScript.GetArrayOfStatTags();
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
                    GameManager.i.dataScript.InitialiseActorArrays();
                }
                else { Debug.LogError("Invalid arrayOfStatTags (Null) -> Import failed"); }
            }
            else { Debug.LogError("Invalid resistanceQualities (Null) -> Import failed"); }
        }
        else { Debug.LogError("Invalid authorityQualities (Null) -> Import failed"); }
        //
        // - - - ActorConflicts - - -
        //
        Dictionary<string, ActorConflict> dictOfActorConflicts = GameManager.i.dataScript.GetDictOfActorConflicts();
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
        Dictionary<string, Secret> dictOfSecrets = GameManager.i.dataScript.GetDictOfSecrets();
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
        // - - - HQs - - - (done higher as no dictionary requirement)
        //
        Dictionary<string, Hq> dictOfHQs = GameManager.i.dataScript.GetDictOfHQs();
        if (dictOfHQs != null)
        {
            counter = 0;
            numArray = arrayOfHQs.Length;
            for (int i = 0; i < numArray; i++)
            {
                Hq factionHQ = arrayOfHQs[i];
                counter++;
                //add to dictionary
                try
                { dictOfHQs.Add(factionHQ.name, factionHQ); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid HQ (Null)"); counter--; }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid HQ (duplicate) \"{0}\"", counter, factionHQ.name)); counter--; }
            }
            numDict = dictOfHQs.Count;
            Debug.LogFormat("[Loa] InitialiseEarly -> dictOfHQs has {0} entries{1}", numDict, "\n");
            Debug.Assert(numDict == counter, "Mismatch on count");
            Debug.Assert(numDict > 0, "No HQs have been imported");
            Debug.Assert(numArray == numDict, string.Format("Mismatch in HQ count, array {0}, dict {1}", numArray, numDict));
        }
        else { Debug.LogError("Invalid dictOfHQs (Null) -> Import failed"); }
        //
        // - - - Cities - - -
        //
        Dictionary<string, City> dictOfCities = GameManager.i.dataScript.GetDictOfCities();
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
        Dictionary<string, Objective> dictOfObjectives = GameManager.i.dataScript.GetDictOfObjectives();
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
        Dictionary<string, Organisation> dictOfOrganisations = GameManager.i.dataScript.GetDictOfOrganisations();
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
        Dictionary<string, Mayor> dictOfMayors = GameManager.i.dataScript.GetDictOfMayors();
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
        Dictionary<string, DecisionAI> dictOfAIDecisions = GameManager.i.dataScript.GetDictOfAIDecisions();
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
        Dictionary<string, Campaign> dictOfCampaigns = GameManager.i.dataScript.GetDictOfCampaigns();
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
        Dictionary<int, Node> dictOfNodes = GameManager.i.dataScript.GetDictOfNodes();
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        if (dictOfNodes != null)
        {
            counter = 0;
            /*//clear dictionary (may be a followOn level) -> done in DataManager.Reset
            dictOfNodes.Clear();*/
            List<Node> tempNodeList = GameManager.i.levelScript.GetListOfNodes();
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
        Dictionary<string, HelpData> dictOfHelp = GameManager.i.dataScript.GetDictOfHelpData();
        //load only if not already present
        if (dictOfHelp.Count == 0)
        {
            List<HelpData> listOfHelp = GameManager.i.helpScript.CreateItemDataHelp();
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
