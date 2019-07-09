﻿using dijkstraAPI;
using gameAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// handles data validation at game start. Optional toggle in GameManager to run, or not
/// Generates '[Val]' log tag entries for any failed validation checks (Not errors as you want it to complete the run and check all)
/// </summary>
public class ValidationManager : MonoBehaviour
{

    //fast access
    GlobalSide globalAuthority;
    GlobalSide globalResistance;

    #region InitialiseFastAccess
    private void InitialiseFastAccess()
    {
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
    }
    #endregion

    /// <summary>
    /// Master control for all validations
    /// </summary>
    public void Initialise(GameState state)
    {
        InitialiseFastAccess();
        ValidateTargets();
        ValidateGear();
        ValidateMissions();
        ValidateTextLists();
        ValidateCities();
    }

    #region ValidateTargets
    /// <summary>
    /// Checks targets
    /// </summary>
    private void ValidateTargets()
    {
        Dictionary<string, Target> dictOfTargets = GameManager.instance.dataScript.GetDictOfTargets();
        /*for (int index = 0; index < dictOfTargets.Count; index++)*/
        foreach (var target in dictOfTargets)
        {
            if (target.Value != null)
            {
                //Good effects
                if (target.Value.listOfGoodEffects.Count > 0)
                {
                    foreach (Effect effect in target.Value.listOfGoodEffects)
                    {
                        if (effect != null)
                        {
                            //should be duration 'Single'
                            if (effect.duration.name.Equals("Single", StringComparison.Ordinal) == false)
                            { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Good effect \"{1}\" NOT Single", target.Value.targetName, effect.name); }
                        }
                        else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Good effect (Null)", target.Value.targetName); }
                    }
                }
                //Bad effects
                if (target.Value.listOfBadEffects.Count > 0)
                {
                    foreach (Effect effect in target.Value.listOfBadEffects)
                    {
                        if (effect != null)
                        {
                            //should be duration 'Single'
                            if (effect.duration.name.Equals("Single", StringComparison.Ordinal) == false)
                            { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Bad effect \"{1}\" NOT Single", target.Value.targetName, effect.name); }
                        }
                        else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Bad effect (Null)", target.Value.targetName); }
                    }
                }
                //Fail effects
                if (target.Value.listOfFailEffects.Count > 0)
                {
                    foreach (Effect effect in target.Value.listOfFailEffects)
                    {
                        if (effect != null)
                        {
                            //should be duration 'Single'
                            if (effect.duration.name.Equals("Single", StringComparison.Ordinal) == false)
                            { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Fail effect \"{1}\" NOT Single", target.Value.targetName, effect.name); }
                        }
                        else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Fail effect (Null)", target.Value.targetName); }
                    }
                }
                //Ongoing effects
                if (target.Value.OngoingEffect != null)
                {
                    //should be duration 'Ongoing'
                    if (target.Value.OngoingEffect.duration.name.Equals("Ongoing", StringComparison.Ordinal) == false)
                    { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" <b>ongoing effect \"{1}\" NOT Ongoing</b>", target.Value.targetName, target.Value.OngoingEffect.name); }
                }
                //Target gear present and shouldn't be infiltration
                if (target.Value.gear != null)
                {
                    if (target.Value.gear.name.Equals("Infiltration", StringComparison.Ordinal) == true)
                    { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" <b>is using INFILTRATION gear</b> (doubled up as can be used automatically on any target)", target.Value.targetName); }
                }
                else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Gear (Null)", target.Value.targetName); }
            }
            else { Debug.LogErrorFormat("Invalid target (Null) for target {0}", target.Key); }
        }
    }
    #endregion

    #region ValidateGear
    /// <summary>
    /// Checks Gear
    /// </summary>
    private void ValidateGear()
    {
        Dictionary<string, Gear> dictOfGear = GameManager.instance.dataScript.GetDictOfGear();
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
                        foreach (var effect in gear.Value.listOfPersonalEffects)
                        {
                            if (effect != null)
                            {
                                //Can't be ONGOING
                                if (effect.duration.name.Equals("Ongoing", StringComparison.Ordinal) == true)
                                { Debug.LogFormat("[Val] ValidateGear: Gear \"{0}\"  invalid Personal effect (ONGOING)", gear.Value.name); }
                            }
                            else { Debug.LogFormat("[Val] ValidateGear: Gear \"{0}\"  invalid effect (Null)", gear.Value.name); }
                        }
                        //AI hacking effects
                        if (gear.Value.aiHackingEffect != null)
                        {
                            //Can't be ONGOING
                            if (gear.Value.aiHackingEffect.duration.name.Equals("Ongoing", StringComparison.Ordinal) == true)
                            { Debug.LogFormat("[Val] ValidateGear: Gear \"{0}\"  invalid aiHacking effect (ONGOING)", gear.Value.name); }
                        }
                    }
                }
                else { Debug.LogFormat("[Val] ValidateGar: Invalid Gear (Null) for gearID {0}", gear.Key); }
            }
        }
    }
    #endregion

    #region ValidateMissions
    /// <summary>
    /// check for unique targets and objectives and check that all ObjectveTargets have matching targets/objectives present
    /// </summary>
    private void ValidateMissions()
    {
        Mission[] arrayOfMissions = GameManager.instance.loadScript.arrayOfMissions;
        if (arrayOfMissions != null)
        {
            List<string> listOfNames = new List<string>();
            List<Target> listOfTargets = new List<Target>();
            //loop missions
            foreach (Mission mission in arrayOfMissions)
            {
                if (mission != null)
                {
                    //
                    // - - - Targets
                    //
                    listOfTargets.Clear();
                    //populate list with targets
                    if (mission.targetBaseCityHall != null)
                    { listOfTargets.Add(mission.targetBaseCityHall); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseCityHall{1}", mission.name, "\n"); }
                    if (mission.targetBaseIcon != null)
                    { listOfTargets.Add(mission.targetBaseIcon); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseIcon{1}", mission.name, "\n"); }
                    if (mission.targetBaseAirport != null)
                    { listOfTargets.Add(mission.targetBaseAirport); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseAirport{1}", mission.name, "\n"); }
                    if (mission.targetBaseHarbour != null)
                    { listOfTargets.Add(mission.targetBaseHarbour); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseHarbour{1}", mission.name, "\n"); }
                    if (mission.targetBaseVIP != null)
                    { listOfTargets.Add(mission.targetBaseVIP); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseVIP{1}", mission.name, "\n"); }
                    if (mission.targetBaseStory != null)
                    { listOfTargets.Add(mission.targetBaseStory); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseStory{1}", mission.name, "\n"); }
                    if (mission.targetBaseGoal != null)
                    { listOfTargets.Add(mission.targetBaseGoal); }
                    else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMissions: {0} mission has no targetBaseGoal{1}", mission.name, "\n"); }
                    //create a list of target.name
                    listOfNames.Clear();
                    for (int i = 0; i < listOfTargets.Count; i++)
                    { listOfNames.Add(listOfTargets[i].name); }
                    //check for duplicates
                    CheckListForDuplicates(listOfNames, "Mission", mission.name, "Targets");
                    //
                    // - - - Objectives
                    //
                    if (mission.listOfObjectives.Count > 0)
                    {
                        //check for duplicates
                        listOfNames.Clear();
                        for (int i = 0; i < mission.listOfObjectives.Count; i++)
                        { listOfNames.Add(mission.listOfObjectives[i].name); }
                        //check for duplicates
                        CheckListForDuplicates(listOfNames, "Mission", mission.name, "Objectives");
                    }
                    //
                    // - - - ObjectiveTargets
                    //
                    if (mission.listOfObjectiveTargets.Count > 0)
                    {
                        //check for duplicates
                        listOfNames.Clear();
                        for (int i = 0; i < mission.listOfObjectiveTargets.Count; i++)
                        { listOfNames.Add(mission.listOfObjectiveTargets[i].name); }
                        //check for duplicates
                        CheckListForDuplicates(listOfNames, "Mission", mission.name, "ObjectiveTargets");
                        //check no more than there are objectives
                        if (mission.listOfObjectiveTargets.Count > mission.listOfObjectives.Count)
                        {
                            Debug.LogFormat("[Val] ValidationManager.cs -> ValidateMssion: Mission \"{0}\", there are more ObjectiveTargets than Objectives ({1} vs. {2}){3}", mission.name,
                                mission.listOfObjectiveTargets.Count, mission.listOfObjectives.Count, "\n");
                        }
                        //check for presence of related data
                        foreach (ObjectiveTarget objTarget in mission.listOfObjectiveTargets)
                        {
                            if (objTarget != null)
                            {
                                //corresponding target present
                                if (listOfTargets.Exists(x => x.name.Equals(objTarget.target.name, StringComparison.Ordinal)) == false)
                                {
                                    Debug.LogFormat("[Val] ValidationMission: Mission \"{0}\", objectiveTarget \"{1}\", target \"{2}\", has no corresponding target{3}", mission.name, objTarget.name,
                                        objTarget.target.name, "\n");
                                }
                                //corresponding objective present
                                if (mission.listOfObjectives.Exists(x => x.name.Equals(objTarget.objective.name, StringComparison.Ordinal)) == false)
                                {
                                    Debug.LogFormat("[Val] ValidationMission: Mission \"{0}\", objectiveTarget \"{1}\", objective \"{2}\", has no corresponding objective{3}", mission.name, objTarget.name,
                                        objTarget.objective.name, "\n");
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid objectiveTarget (Null) for mission {0}", mission.name); }
                        }

                    }
                }
                else { Debug.LogError("Invalid mission (Null) in arrayOfMissions"); }
            }
        }
        else { Debug.LogError("Invalid arrayOfMissions (Null)"); }
    }
    #endregion

    #region ValidateTextLists
    /// <summary>
    /// checks all textlists for duplicates
    /// </summary>
    private void ValidateTextLists()
    {
        //combine all text list arrays into a single list for validation checks
        List<TextList> listOfAllTextLists = new List<TextList>();
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfContactTextLists);
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfNameTextLists);
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfDistrictTextLists);
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfShortTextLists);
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfFactorTextLists);
        //NOTE: add extra text lists here (as above)
        TextList[] arrayOfTextLists = listOfAllTextLists.ToArray();
        //loop textlists
        List<string> findList = new List<string>();
        int count;
        for (int i = 0; i < arrayOfTextLists.Length; i++)
        {
            TextList textList = arrayOfTextLists[i];
            if (textList != null)
            {
                //check for duplicates
                List<string> tempList = new List<string>(textList.randomList);
                if (tempList != null)
                {
                    //loop temp list and check against master text list
                    foreach (String item in tempList)
                    {
                        findList.Clear();
                        findList = textList.randomList.FindAll(x => x == item);
                        count = findList.Count;
                        //always one copy present, shouldn't be any more
                        if (count > 1)
                        {
                            //ignore first, legit, copy
                            count--;
                            Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTextList: {0} Duplicate{1} exist for \"{2}\" in textList {3}{4}", count, count != 1 ? "s" : "", item, textList.name, "\n");
                        }
                    }
                }
                else { Debug.LogErrorFormat("Invalid randomList (Null) for textList {0}", textList.descriptor); }
            }
            else { Debug.LogErrorFormat("Invalid textList (Null) for arrayOfTextLists[{0}]", i); }
        }
    }

    #endregion

    #region ValidateCities
    //checks that city speciality district names (icon, airport, harbour) aren't present in the cities list of DistrictNames
    private void ValidateCities()
    {
        City[] arrayOfCities = GameManager.instance.loadScript.arrayOfCities;
        if (arrayOfCities != null)
        {
            for (int i = 0; i < arrayOfCities.Length; i++)
            {
                City city = arrayOfCities[i];
                if (city != null)
                {
                    if (city.iconDistrict != null)
                    {
                        if (city.districtNames.CheckItemPresent(city.iconDistrict) == true)
                        { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCities: {0} iconDistrict \"{1}\", present in textList {2}{3}", city.tag, city.iconDistrict, city.districtNames.name, "\n"); }
                    }
                    if (city.airportDistrict != null)
                    {
                        if (city.districtNames.CheckItemPresent(city.airportDistrict) == true)
                        { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCities: {0} airportDistrict \"{1}\", present in textList {2}{3}", city.tag, city.airportDistrict, city.districtNames.name, "\n"); }
                    }
                    if (city.harbourDistrict != null)
                    {
                        if (city.districtNames.CheckItemPresent(city.harbourDistrict) == true)
                        { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateCities: {0} harbourDistrict \"{1}\", present in textList {2}{3}", city.tag, city.harbourDistrict, city.districtNames.name, "\n"); }
                    }
                }
                else { Debug.LogErrorFormat("Invalid city (Null) in arrayOfCities[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid arrayOfCities (Null)"); }
    }
    #endregion



#if (UNITY_EDITOR)

    #region ValidateSO
    /// <summary>
    /// optional (GameManager.cs toggle) program to run to check SO's loaded in LoadManager.cs arrays vs. those found by an Asset search (editor only)
    /// Designed to pick up SO's that might have been added in the editor but not added to the arrays (ignored by game if this is the case).
    /// </summary>
    public void ValidateSO(GameState state)
    {
        //GlobalMeta
        ValidateSOGeneric<GlobalMeta>(GameManager.instance.loadScript.arrayOfGlobalMeta);
        //GlobalChance
        ValidateSOGeneric<GlobalChance>(GameManager.instance.loadScript.arrayOfGlobalChance);
        //GlobalType
        ValidateSOGeneric<GlobalType>(GameManager.instance.loadScript.arrayOfGlobalType);
        //GlobalSide
        ValidateSOGeneric<GlobalSide>(GameManager.instance.loadScript.arrayOfGlobalSide);
        //GlobalWho
        ValidateSOGeneric<GlobalWho>(GameManager.instance.loadScript.arrayOfGlobalWho);
        //EffectApply
        ValidateSOGeneric<EffectApply>(GameManager.instance.loadScript.arrayOfEffectApply);
        //EffectCriteria
        ValidateSOGeneric<EffectCriteria>(GameManager.instance.loadScript.arrayOfEffectCriteria);
        //EffectDuration
        ValidateSOGeneric<EffectDuration>(GameManager.instance.loadScript.arrayOfEffectDuration);
        //EffectOperator
        ValidateSOGeneric<EffectOperator>(GameManager.instance.loadScript.arrayOfEffectOperator);
        //ContactType
        ValidateSOGeneric<ContactType>(GameManager.instance.loadScript.arrayOfContactTypes);
        //TargetType
        ValidateSOGeneric<TargetType>(GameManager.instance.loadScript.arrayOfTargetTypes);
        //TargetTrigger
        ValidateSOGeneric<TargetTrigger>(GameManager.instance.loadScript.arrayOfTargetTriggers);
        //TargetProfile
        ValidateSOGeneric<TargetProfile>(GameManager.instance.loadScript.arrayOfTargetProfiles);
        //Quality
        ValidateSOGeneric<Quality>(GameManager.instance.loadScript.arrayOfQualities);
        //Condition
        ValidateSOGeneric<Condition>(GameManager.instance.loadScript.arrayOfConditions);
        //Cure
        ValidateSOGeneric<Cure>(GameManager.instance.loadScript.arrayOfCures);
        //TraitCategory
        ValidateSOGeneric<TraitCategory>(GameManager.instance.loadScript.arrayOfTraitCategories);
        //TraitEffect
        ValidateSOGeneric<TraitEffect>(GameManager.instance.loadScript.arrayOfTraitEffects);
        //SecretType
        ValidateSOGeneric<SecretType>(GameManager.instance.loadScript.arrayOfSecretTypes);
        //CityArc
        ValidateSOGeneric<CityArc>(GameManager.instance.loadScript.arrayOfCityArcs);
        //City
        ValidateSOGeneric<City>(GameManager.instance.loadScript.arrayOfCities);
        //CitySize
        ValidateSOGeneric<CitySize>(GameManager.instance.loadScript.arrayOfCitySize);
        //CitySpacing
        ValidateSOGeneric<CitySpacing>(GameManager.instance.loadScript.arrayOfCitySpacing);
        //CityConnections
        ValidateSOGeneric<CityConnections>(GameManager.instance.loadScript.arrayOfCityConnections);
        //CitySecurity
        ValidateSOGeneric<CitySecurity>(GameManager.instance.loadScript.arrayOfCitySecurity);
        //Damage
        ValidateSOGeneric<Damage>(GameManager.instance.loadScript.arrayOfDamages);
        //Challenge
        ValidateSOGeneric<Challenge>(GameManager.instance.loadScript.arrayOfChallenges);
        //Nemesis
        ValidateSOGeneric<Nemesis>(GameManager.instance.loadScript.arrayOfNemesis);
        //Rebel Leaders
        ValidateSOGeneric<RebelLeader>(GameManager.instance.loadScript.arrayOfRebelLeaders);
        //Scenario
        ValidateSOGeneric<Scenario>(GameManager.instance.loadScript.arrayOfScenarios);
        //Campaign
        ValidateSOGeneric<Campaign>(GameManager.instance.loadScript.arrayOfCampaigns);
        //NameSet
        ValidateSOGeneric<NameSet>(GameManager.instance.loadScript.arrayOfNameSets);
        //NodeDatapoint
        ValidateSOGeneric<NodeDatapoint>(GameManager.instance.loadScript.arrayOfNodeDatapoints);
        //NodeArc
        ValidateSOGeneric<NodeArc>(GameManager.instance.loadScript.arrayOfNodeArcs);
        //NodeCrisis
        ValidateSOGeneric<NodeCrisis>(GameManager.instance.loadScript.arrayOfNodeCrisis);
        //Trait
        ValidateSOGeneric<Trait>(GameManager.instance.loadScript.arrayOfTraits);
        //ActorArc
        ValidateSOGeneric<ActorArc>(GameManager.instance.loadScript.arrayOfActorArcs);
        //Effect
        ValidateSOGeneric<Effect>(GameManager.instance.loadScript.arrayOfEffects);
        //Action
        ValidateSOGeneric<Action>(GameManager.instance.loadScript.arrayOfActions);
        //TeamArc
        ValidateSOGeneric<TeamArc>(GameManager.instance.loadScript.arrayOfTeamArcs);
        //Gear
        ValidateSOGeneric<Gear>(GameManager.instance.loadScript.arrayOfGear);
        //GearRarity
        ValidateSOGeneric<GearRarity>(GameManager.instance.loadScript.arrayOfGearRarity);
        //GearType
        ValidateSOGeneric<GearType>(GameManager.instance.loadScript.arrayOfGearType);
        //ManageActor
        ValidateSOGeneric<ManageActor>(GameManager.instance.loadScript.arrayOfManageActors);
        //ManageAction
        ValidateSOGeneric<ManageAction>(GameManager.instance.loadScript.arrayOfManageActions);
        //ActorConflict
        ValidateSOGeneric<ActorConflict>(GameManager.instance.loadScript.arrayOfActorConflicts);
        //Secret
        ValidateSOGeneric<Secret>(GameManager.instance.loadScript.arrayOfSecrets);
        //Faction
        ValidateSOGeneric<Faction>(GameManager.instance.loadScript.arrayOfFactions);
        //Objective
        ValidateSOGeneric<Objective>(GameManager.instance.loadScript.arrayOfObjectives);
        //Organisation
        ValidateSOGeneric<Organisation>(GameManager.instance.loadScript.arrayOfOrganisations);
        //Mayor
        ValidateSOGeneric<Mayor>(GameManager.instance.loadScript.arrayOfMayors);
        //DecisionAI
        ValidateSOGeneric<DecisionAI>(GameManager.instance.loadScript.arrayOfDecisionAI);
        //Mission
        ValidateSOGeneric<Mission>(GameManager.instance.loadScript.arrayOfMissions);
        //
        // - - - TextList - - -
        //
        //combine all text list arrays into a single list for validation checks
        List<TextList> listOfAllTextLists = new List<TextList>();
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfContactTextLists);
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfNameTextLists);
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfDistrictTextLists);
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfShortTextLists);
        listOfAllTextLists.AddRange(GameManager.instance.loadScript.arrayOfFactorTextLists);
        //NOTE: add extra text lists here (as above)
        TextList[] arrayOfTextLists = listOfAllTextLists.ToArray();
        ValidateSOGeneric<TextList>(arrayOfTextLists);
        //
        // - - - Target - - -
        //
        //combine all text list arrays into a single list for validation checks
        List<Target> listOfTargets = new List<Target>();
        listOfTargets.AddRange(GameManager.instance.loadScript.arrayOfTargetsGeneric);
        listOfTargets.AddRange(GameManager.instance.loadScript.arrayOfTargetsCity);
        listOfTargets.AddRange(GameManager.instance.loadScript.arrayOfTargetsVIP);
        listOfTargets.AddRange(GameManager.instance.loadScript.arrayOfTargetsStory);
        listOfTargets.AddRange(GameManager.instance.loadScript.arrayOfTargetsGoal);
        //NOTE: add extra target lists here (as above)
        Target[] arrayOfTargetLists = listOfTargets.ToArray();
        ValidateSOGeneric<Target>(arrayOfTargetLists);

    }
    #endregion

    #region ValidateSOGeneric
    /// <summary>
    /// Generic method to validate an SO type via an Asset Search on hard drive vs. the equivalent array in LoadManager.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arraySO"></param>
    private void ValidateSOGeneric<T>(T[] arraySO) where T : ScriptableObject
    {
        if (arraySO != null)
        {
            int numArray, numAssets;
            // 'isVerbal' is true for displaying ALL messages, false for problem messages only
            bool isVerbal = false;
            string path;
            // Validate SO
            var metaGUID = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).Name), new[] { "Assets/SO" });
            numArray = arraySO.Length;
            numAssets = metaGUID.Length;
            if (numAssets != numArray)
            {
                Debug.LogWarningFormat("[Val] ValidateSO: MISMATCH on {0}, array {1}, assets {2} records", typeof(T).Name, numArray, numAssets);
                T[] arrayTemp = arraySO;
                foreach (var guid in metaGUID)
                {
                    //get path
                    path = AssetDatabase.GUIDToAssetPath(guid);
                    //get SO
                    UnityEngine.Object metaObject = AssetDatabase.LoadAssetAtPath(path, typeof(T));
                    //get object
                    T meta = metaObject as T;
                    if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name, StringComparison.Ordinal)) == false)
                    { Debug.LogFormat("[Val] ValidateSO: array MISSING {0} \"{1}\"", typeof(T).Name, meta.name); }
                }
            }
            else
            {
                if (isVerbal == true)
                { Debug.LogFormat("[Val] ValidateSO: Checksum O.K on {0} SO, array {1}, assets {2} records", typeof(T).Name, numArray, numAssets); }
            }
        }
        else { Debug.LogWarningFormat("Invalid arraySO for {0}", typeof(T).Name); }
    }
    #endregion

#endif

    #region Data Integrity Checks
    //
    // - - - Integrity Check - - -
    //

    #region ExecuteIntegrityCheck
    /// <summary>
    /// Master method to run all data Integrity checks
    /// </summary>
    public void ExecuteIntegrityCheck()
    {
        string prefix = "[Val] ValidationManager.cs -> ";
        //range limits
        int highestActorID = GameManager.instance.actorScript.actorIDCounter;
        int highestNodeID = GameManager.instance.nodeScript.nodeCounter;
        int highestSlotID = GameManager.instance.actorScript.maxNumOfOnMapActors - 1;
        int highestContactID = GameManager.instance.contactScript.contactIDCounter;
        int highestTeamID = GameManager.instance.teamScript.teamIDCounter;
        int highestTurn = GameManager.instance.turnScript.Turn;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //debug checks
        Debug.Assert(highestActorID > 0, "Invalid highestActor (Zero or less)");
        Debug.Assert(highestNodeID > 0, "Invalid highestNode (Zero or less)");
        Debug.Assert(highestSlotID > 0, "Invalid highestSlotID (Zero or less)");
        Debug.Assert(highestContactID > 0, "Invalid highestContactID (Zero or less)");
        Debug.Assert(highestTeamID > 0, "Invalid highestTeamID (Zero or less)");
        Debug.Assert(playerSide != null, "Invalid playerSide (Null)");

        //run checks
        if (string.IsNullOrEmpty(prefix) == false)
        {
            Debug.LogFormat("{0}ExecuteIntegrityCheck: Commence checks - - - {1}", prefix, "\n");
            CheckNodeData(prefix, highestNodeID);
            CheckActorData(prefix, highestActorID, highestNodeID, highestSlotID);
            CheckTargetData(prefix, highestNodeID, highestContactID, highestTurn);
            CheckTeamData(prefix, highestNodeID, highestTeamID, highestSlotID, highestTurn, playerSide);
        }
    }
    #endregion

    #region CheckNodeData
    /// <summary>
    /// Integrity check on all node related data collections
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckNodeData(string prefix, int highestNodeID)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckNodeData: ");
        
        int maxStatValue = GameManager.instance.nodeScript.maxNodeValue;
        Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfNodes
        //
        //check dictionaries all have the same number of entries
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        if (dictOfNodes != null)
        {
            //check count
            if (dictOfNodes.Count != highestNodeID)
            { Debug.LogFormat("{0}Incorrect count, dictOfNodes has {1} records, highestNode {2}{3}", tag, dictOfNodes.Count, highestNodeID, "\n"); }
            foreach (var node in dictOfNodes)
            {
                //null check record
                if (node.Value != null)
                {
                    key = node.Key.ToString();
                    //range checks
                    CheckDictRange(node.Key, 0, highestNodeID, "nodeID", tag, key);
                    CheckDictRange(node.Value.Stability, 0, maxStatValue, "Stability", tag, key);
                    CheckDictRange(node.Value.Support, 0, maxStatValue, "Support", tag, key);
                    CheckDictRange(node.Value.Security, 0, maxStatValue, "Security", tag, key);
                    //null checks
                    CheckDictString(node.Value.nodeName, "nodeName", tag, key);
                    CheckDictObject(node.Value._Material, "_Material", tag, key);
                    CheckDictObject(node.Value.gameObject, "gameObject", tag, key);
                    CheckDictObject(node.Value.Arc, "node.Arc", tag, key);
                    CheckDictObject(node.Value.launcher, "launcher", tag, key);
                    CheckDictObject(node.Value.nodePosition, "nodePosition", tag, key);
                    CheckDictObject(node.Value.loiter, "loiter", tag, key);
                    CheckDictList(node.Value.GetListOfTeams(), "dictOfNodes: listOfTeams", tag, key);
                    CheckDictList(node.Value.GetListOfOngoingEffects(), "dictOfNodes: listOfOngoingEffects", tag, key);
                    CheckDictList(node.Value.GetNeighbouringNodes(), "dictOfNodes: listOfNeighbouringNodes", tag, key);
                    CheckDictList(node.Value.GetNearNeighbours(), "dictOfNodes: listOfNearNeighbours", tag, key);
                    CheckDictList(node.Value.GetListOfConnections(), "dictOfNodes: listOfConnections", tag, key);
                    //duplicate checks
                    CheckListForDuplicates(node.Value.GetListOfTeamID(), string.Format("dictOfNodes: dictKey {0}", key), "TeamID", "listOfTeams");
                    CheckListForDuplicates(node.Value.GetListOfOngoingID(), string.Format("dictOfNodes: dictKey {0}", key), "OngoingID", "listOfOngoingEffects");
                }
                else { Debug.LogFormat("{0}Invalid entry (Null) in dictOfNodes for nodeID {1}{2}", node.Key, "\n"); }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
        //
        // - - - dictOfNodeObjects
        //
        Dictionary<int, GameObject> dictOfNodeObjects = GameManager.instance.dataScript.GetDictOfNodeObjects();
        if (dictOfNodeObjects != null)
        {
            foreach (var node in dictOfNodeObjects)
            {
                key = node.Key.ToString();
                CheckDictRange(node.Key, 0, highestNodeID, "nodeID", tag, key);
                CheckDictObject(node.Value, "node", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfNodeObjects (Null)"); }
        //
        // - - - dictOfNodeArcs
        //
        Dictionary<int, NodeArc> dictOfNodeArcs = GameManager.instance.dataScript.GetDictOfNodeArcs();
        if (dictOfNodeArcs != null)
        {
            foreach (var arc in dictOfNodeArcs)
            {
                key = arc.Key.ToString();
                CheckDictRange(arc.Value.nodeArcID, 0, 99, "nodeArcID", tag, key);
                CheckDictObject(arc.Value.sprite, "sprite", tag, key);
                CheckDictArray(arc.Value.contactTypes, "arrayOfContactTypes", tag, key, true);
            }
        }
        else { Debug.LogError("Invalid dictOfNodeArcs (Null)"); }
        //
        // - - - dictOfNodeDUnweighted
        //
        Dictionary<int, NodeD> dictOfNodeUnweighted = GameManager.instance.dataScript.GetDictOfNodeDUnweighted();
        if (dictOfNodeUnweighted != null)
        {
            foreach (var nodeD in dictOfNodeUnweighted)
            {
                key = nodeD.Key.ToString();
                CheckDictRange(nodeD.Key, 0, highestNodeID, "nodeD.ID", tag, key);
                CheckDictObject(nodeD.Value, "nodeD", tag, key);
                CheckDictRange(nodeD.Value.Distance, 0, 99, "Distance", tag, key);
                CheckDictString(nodeD.Value.Name, "Name", tag, key);
                CheckDictList(nodeD.Value.Weights, "NodeDUnWeighted: weights", tag, key, false);
                CheckDictList(nodeD.Value.Adjacency, "NodeDUnWeighted: adjaceny", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfNodeUnweighted (Null)"); }
        //
        // - - - dictOfNodeDWeighted
        //
        Dictionary<int, NodeD> dictOfNodeWeighted = GameManager.instance.dataScript.GetDictOfNodeDWeighted();
        if (dictOfNodeWeighted != null)
        {
            foreach (var nodeD in dictOfNodeWeighted)
            {
                key = nodeD.Key.ToString();
                CheckDictRange(nodeD.Key, 0, highestNodeID, "nodeD.ID", tag, key);
                CheckDictObject(nodeD.Value, "nodeD", tag, key);
                CheckDictRange(nodeD.Value.Distance, 0, 99, "Distance", tag, key);
                CheckDictString(nodeD.Value.Name, "Name", tag, key);
                CheckDictList(nodeD.Value.Weights, "NodeDWeighted: weights", tag, key, false);
                CheckDictList(nodeD.Value.Adjacency, "NodeDWeighted: adjaceny", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfNodeWeighted (Null)"); }
        //
        // - - - Node lists
        //
        CheckList(GameManager.instance.dataScript.GetListOfAllNodes(), tag, "listOfNodes", highestNodeID);
        CheckList(GameManager.instance.dataScript.GetListOfMostConnectedNodes(), "listOfMostConnectedNodes", tag);
        CheckList(GameManager.instance.dataScript.GetListOfDecisionNodes(), "listOfDecisionNodes", tag);
        CheckList(GameManager.instance.dataScript.GetListOfLoiterNodes(), "listOfLoiterNodes", tag);
        CheckList(GameManager.instance.dataScript.GetListOfCureNodes(), "listOfCureNodes", tag);
        //duplicate checks
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfNodeID(), "Node", "nodeID", "listOfNodes");
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfMostConnectedNodeID(), "Node", "nodeID", "listOfMostConnectedNodes");
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfDecisionNodeID(), "Node", "nodeID", "listOfDecisionNodes");
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfLoiterNodeID(), "Node", "nodeID", "listOfLoiterNodes");
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfCureNodeID(), "Node", "nodeID", "listOfCureNodes");
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfCrisisNodeID(), "Node", "nodeID", "listOfCrisisNodes");
    }
    #endregion

    #region CheckActorData
    /// <summary>
    /// Integrity check on all actor related collections
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="highestActorID"></param>
    private void CheckActorData(string prefix, int highestActorID, int highestNodeID, int highestSlotID)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckActorData: ");
        int maxStatValue = GameManager.instance.actorScript.maxStatValue;
        int maxCompatibility = GameManager.instance.personScript.maxCompatibilityWithPlayer;
        int minCompatibility = GameManager.instance.personScript.minCompatibilityWithPlayer;
        int maxFactor = GameManager.instance.personScript.maxPersonalityFactor;
        int minFactor = GameManager.instance.personScript.minPersonalityFactor;
        /*int numOfActorArcs = GameManager.instance.dataScript.GetNumOfActorArcs();*/

        Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfActors
        //
        Dictionary<int, Actor> dictOfActors = GameManager.instance.dataScript.GetDictOfActors();
        if (dictOfActors != null)
        {
            foreach(var actor in dictOfActors)
            {
                key = actor.Key.ToString();
                CheckDictRange(actor.Key, 0, highestActorID, "actorID", tag, key);
                CheckDictRange(actor.Value.slotID, -1, highestSlotID, "slotID", tag, key);
                CheckDictRange(actor.Value.level, 1, 3, "level", tag, key);
                CheckDictString(actor.Value.actorName, "actorName", tag, key);
                CheckDictString(actor.Value.firstName, "firstName", tag, key);
                CheckDictString(actor.Value.spriteName, "spriteName", tag, key);
                CheckDictObject(actor.Value.side, "side", tag, key);
                CheckDictObject(actor.Value.arc, "arc", tag, key);
                CheckDictObject(actor.Value.sprite, "sprite", tag, key);
                CheckDictObject(actor.Value.GetTrait(), "trait", tag, key);
                CheckDictRange(actor.Value.GetDatapoint(ActorDatapoint.Datapoint0), 0, maxStatValue, "datapoint0", tag, key);
                CheckDictRange(actor.Value.GetDatapoint(ActorDatapoint.Datapoint1), 0, maxStatValue, "datapoint1", tag, key);
                CheckDictRange(actor.Value.GetDatapoint(ActorDatapoint.Datapoint2), 0, maxStatValue, "datapoint2", tag, key);
                CheckDictRange(actor.Value.Renown, 0, 99, "Renown", tag, key);
                CheckDictList(actor.Value.GetListOfTeams(), "listOfTeams", tag, key, false);
                CheckDictList(actor.Value.GetListOfSecrets(), "listOfSecrets", tag, key);
                CheckDictList(actor.Value.GetListOfConditions(), "listOfConditions", tag, key);
                CheckDictList(actor.Value.GetListOfTraitEffects(), "listOfTraitEffects", tag, key);
                CheckDictDict(actor.Value.GetDictOfContacts(), "dictOfContacts", tag, key);
                //personality
                Personality personality = actor.Value.GetPersonality();
                if (personality != null)
                {
                    CheckDictArrayBounds(personality.GetFactors(), "arrayOfFactors", tag, key, minFactor, maxFactor);
                    CheckDictRange(personality.GetCompatibilityWithPlayer(), minCompatibility, maxCompatibility, "compatibilityWithPlayer", tag, key);
                    if (personality.GetProfile() != null)
                    {
                        CheckDictString(personality.GetProfileDescriptor(), "profileDescriptor", tag, key);
                        CheckDictString(personality.GetProfileExplanation(), "profileExplanation", tag, key);
                        CheckList(personality.GetListOfDescriptors(), ",personality.listOfDescriptors", tag);
                        CheckList(personality.GetListOfMotivation(), "personality.listOfMotivation", tag);
                    }
                }
                else { Debug.LogFormat("{0}Invalid personality (Null) for actorID {1}{2}", tag, actor.Key, "\n"); }
            }
        }
        else { Debug.LogError("Invalid dictOfActors (Null)"); }
        //
        // - - - Other Actor Collections
        //
        CheckList(GameManager.instance.dataScript.GetListOfActorTypes(), "listOfActorTypes", tag);
        CheckList(GameManager.instance.dataScript.GetListOfActorPortraits(), "listOfActorPortraits", tag);
        //authority
        CheckList(GameManager.instance.dataScript.GetActorRecruitPool(1, globalAuthority), "authorityActorPoolLevelOne", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorRecruitPool(2, globalAuthority), "authorityActorPoolLevelTwo", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorRecruitPool(3, globalAuthority), "authorityActorPoolLevelThree", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Reserve), "authorityActorReserve", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Dismissed), "authorityActorDismissed", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Promoted), "authorityActorPromoted", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Disposed), "authorityActorDisposedOff", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Resigned), "authorityActorResigned", tag, 0, false);
        //resistance
        CheckList(GameManager.instance.dataScript.GetActorRecruitPool(1, globalResistance), "resistanceActorPoolLevelOne", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorRecruitPool(2, globalResistance), "resistanceActorPoolLevelTwo", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorRecruitPool(3, globalResistance), "resistanceActorPoolLevelThree", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Reserve), "resistanceActorReserve", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Dismissed), "resistanceActorDismissed", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Promoted), "resistanceActorPromoted", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Disposed), "resistanceActorDisposedOff", tag, 0, false);
        CheckList(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Resigned), "resistanceActorResigned", tag, 0, false);
        //authority duplicates
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorRecruitPool(1, globalAuthority), "actorID", "actor", "authorityActorPoolLevelOne");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorRecruitPool(2, globalAuthority), "actorID", "actor", "authorityActorPoolLevelTwo");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorRecruitPool(3, globalAuthority), "actorID", "actor", "authorityActorPoolLevelThree");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Reserve), "actorID", "actor", "authorityActorReserve");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Dismissed), "actorID", "actor", "authorityActorDismissed");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Promoted), "actorID", "actor", "authorityActorPromoted");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Disposed), "actorID", "actor", "authorityActorDisposedOff");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Resigned), "actorID", "actor", "authorityActorResigned");
        //resistence duplicates
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorRecruitPool(1, globalResistance), "actorID", "actor", "resistanceActorPoolLevelOne");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorRecruitPool(2, globalResistance), "actorID", "actor", "resistanceActorPoolLevelTwo");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorRecruitPool(3, globalResistance), "actorID", "actor", "resistanceActorPoolLevelThree");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Reserve), "actorID", "actor", "resistanceActorReserve");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Dismissed), "actorID", "actor", "resistanceActorDismissed");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Promoted), "actorID", "actor", "resistanceActorPromoted");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Disposed), "actorID", "actor", "resistanceActorDisposedOff");
        CheckListForDuplicates(GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Resigned), "actorID", "actor", "resistanceActorResigned");
    }
    #endregion

    #region CheckTargetData
    /// <summary>
    /// Integrity check on all target related collections
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckTargetData(string prefix, int highestNodeID, int highestContactID, int highestTurn)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckTargetData: ");
        int maxTargetIntel = GameManager.instance.targetScript.maxTargetInfo;
        Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfTargets
        //
        Dictionary<string, Target> dictOfTargets = GameManager.instance.dataScript.GetDictOfTargets();
        if (dictOfTargets != null)
        {
            foreach(var target in dictOfTargets)
            {
                key = target.Key;
                CheckDictRange(target.Value.intel, 0, maxTargetIntel, "intel", tag, key);
                if (target.Value.targetStatus != Status.Dormant && target.Value.targetStatus != Status.Done)
                { CheckDictRange(target.Value.nodeID, 0, highestNodeID, "nodeID", tag, key); }
                if (target.Value.targetStatus == Status.Done)
                { CheckDictRange(target.Value.turnDone, 0, highestTurn, "turnDone", tag, key); }
                CheckDictRange(target.Value.distance, 0, 20, "distance", tag, key);
                CheckDictRange(target.Value.numOfAttempts, 0, 20, "numOfAtttempts", tag, key);
                CheckDictList(target.Value.listOfRumourContacts, "listOfRumourContacts", tag, key, false);
                CheckDictListBounds(target.Value.listOfRumourContacts, "listOfRumourContacts", tag, key, 0, highestContactID);
            }
        }
        else { Debug.LogError("Invalid dictOfTargets (Null)"); }
        //
        // - - - Target lists
        //
        CheckList(GameManager.instance.dataScript.GetTargetPool(Status.Active), "targetPoolActive", tag);
        CheckList(GameManager.instance.dataScript.GetTargetPool(Status.Live), "targetPoolLive", tag);
        CheckList(GameManager.instance.dataScript.GetTargetPool(Status.Outstanding), "targetPoolOutstanding", tag);
        CheckList(GameManager.instance.dataScript.GetTargetPool(Status.Done), "targetPoolDone", tag);
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfNodesWithTargets(), "nodes", "nodeID", "listOfNodesWithTargets");
    }
    #endregion

    #region CheckTeamData
    /// <summary>
    /// Integrity check all team related collections
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="highestNodeID"></param>
    /// <param name="highestTeamID"></param>
    private void CheckTeamData(string prefix, int highestNodeID, int highestTeamID, int highestSlotID, int highestTurn, GlobalSide playerSide)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckTeamData: ");
        Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfTeams
        //
        Dictionary<int, Team> dictOfTeams = GameManager.instance.dataScript.GetDictOfTeams();
        if (dictOfTeams != null)
        {
            foreach(var team in dictOfTeams)
            {
                key = team.Key.ToString();
                CheckDictRange(team.Value.teamID, 0, highestTeamID, "teamID", tag, key);
                CheckDictString(team.Value.teamName, "teamName", tag, key);
                CheckDictObject(team.Value.arc, "arc", tag, key);
                if (team.Value.pool == TeamPool.OnMap)
                {
                    //OnMap team should have actor assigned only if Authority player side, default '-1' if playerSide
                    if (playerSide.level == 1)
                    { CheckDictRange(team.Value.actorSlotID, 0, highestSlotID, "actorSlotID", tag, key); }
                    else { CheckDictRange(team.Value.actorSlotID, -1, -1, "actorSlotID", tag, key); }
                    CheckDictRange(team.Value.nodeID, 0, highestNodeID, "nodeID", tag, key);
                    CheckDictRange(team.Value.turnDeployed, 0, highestTurn, "turnDeployed", tag, key);
                }
                else
                {
                    //if Reserve or Intransit values should all be set to default
                    CheckDictRange(team.Value.actorSlotID, -1, -1, "actorSlotID", tag, key);
                    CheckDictRange(team.Value.nodeID, -1, -1, "nodeID", tag, key);
                    CheckDictRange(team.Value.turnDeployed, -1, -1, "turnDeployed", tag, key);
                }
            }
        }
        else { Debug.LogError("Invalid dictOfTeams (Null)"); }
        //
        // - - - Team pools
        //
        CheckListForDuplicates(GameManager.instance.dataScript.GetTeamPool(TeamPool.Reserve), "team", "teamID", "teamPoolReserve");
        CheckListForDuplicates(GameManager.instance.dataScript.GetTeamPool(TeamPool.OnMap), "team", "teamID", "teamPoolOnMap");
        CheckListForDuplicates(GameManager.instance.dataScript.GetTeamPool(TeamPool.InTransit), "team", "teamID", "teamPoolInTransit");
    }
    #endregion

    #endregion


    #region Utilities
    //
    // - - - Utilities - - - 
    //

    #region CheckListForDuplicates
    /// <summary>
    /// generic method that takes any list of IComparables, eg. int, string, but NOT SO's, and checks for duplicates. Debug.LogFormat("[Val]"...) messages generated for dupes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="listToCheck"></param>
    private void CheckListForDuplicates<T>(List<T> listToCheck, string typeOfObject = "Unknown", string nameOfObject = "Unknown", string nameOfList = "Unknown") where T : IComparable
    {
        if (listToCheck != null)
        {
            if (listToCheck.Count > 0)
            {
                var query = listToCheck.GroupBy(x => x)
                                .Where(g => g.Count() > 1)
                                .Select(y => new { Element = y.Key, Counter = y.Count() })
                                .ToList();
                if (query.Count > 0)
                {
                    //generate message for each set of duplicates in list
                    int numOfDuplicates = 0;
                    foreach (var item in query)
                    {
                        numOfDuplicates = item.Counter - 1;
                        Debug.LogFormat("[Val] ValidationManager.cs -> CheckListForDuplicates: {0} \"{1}\" , list \"{2}\", record \"{3}\" has {4} duplicate{5}{6}", typeOfObject, nameOfObject, nameOfList,
                            item.Element, numOfDuplicates, numOfDuplicates != 1 ? "s" : "", "\n");
                    }
                }
            }
        }
        else { Debug.LogFormat("[Val] ValidationManager.cs -> CheckListForDuplicates: Invalid listToCheck {0} (Null){1}", nameOfList, "\n"); }
    }
    #endregion


    /// <summary>
    /// Checks that a dictionary int value is inside specified range (inclusive)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    private void CheckDictRange(int value, int lower, int upper, string varName, string tag, string key)
    {
        if (value < lower || value > upper)
        { Debug.LogFormat("{0}dictKey \"{1}\", variable {2}, value {3}, outside of range ({4} to {5}){6}", tag, key, varName, value, lower, upper, "\n"); }
    }

    /// <summary>
    /// Checks dictionary string for Null and Empty
    /// </summary>
    /// <param name="text"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    private void CheckDictString(string text, string stringName, string tag, string key)
    {
        if (string.IsNullOrEmpty(text) == true)
        { Debug.LogFormat("{0}Invalid {1} (Null) for dictKey {2}{3}", tag, stringName, key, "\n"); }
    }

    /// <summary>
    /// Checks any dictionary object/field for null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="thing"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    private void CheckDictObject<T>(T thing, string objectName, string tag, string key)
    {
        if (thing == null)
        { Debug.LogFormat("{0}Invalid {1} (Null) for dictKey {2}{3}", tag, objectName, key, "\n"); }
    }

    /// <summary>
    /// Check dictionary list for Null and check elements in list for Null (optional, default true)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="key"></param>
    /// <param name="tag"></param>
    private void CheckDictList<T>(List<T> list, string listName, string tag, string key, bool isNullCheckContents = true)
    {
        if (list == null)
        { Debug.LogFormat("{0}Invalid {1} (Null) for dictKey {2}{3}", tag, listName, key, "\n"); }
        else
        {
            if (isNullCheckContents == true)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == null)
                    { Debug.LogFormat("{0}\"{1}\"[{2] invalid (Null) for dictKey {3}{4}", tag, listName, i, key, "\n"); }
                }
            }
        }
    }

    /// <summary>
    /// Bounds checks a dict list of Int's (each element checked if within lower and upper, inclusive) 
    /// </summary>
    /// <param name="list"></param>
    /// <param name="listName"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    private void CheckDictListBounds(List<int> list, string listName, string tag, string key, int lower = 0, int upper = 0)
    {
        if (list != null)
        {
            if (lower != 0 || upper != 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] < lower || list[i] > upper)
                    { Debug.LogFormat("{0}\"{1}\"[{2}] failed bounds check, value {3) (should be between {4} and {5}){6}", tag, listName, i, list[i], lower, upper, "\n"); }
                }
            }
            else { Debug.LogWarningFormat("Invalid CheckDictListBounds for {0} check as both lower and upper bounds are Zero{1}", listName, "\n"); }
        }
        else { Debug.LogFormat("{0}\"{1}\" Invalid (Null) for dictKey {2}{3}", tag, listName, key, "\n"); }
    }

    /// <summary>
    /// Check an encapsulated Dictionary within a dict object for null and all elements in list for Null (optional ,default true)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="listName"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    /// <param name="isNullCheckContents"></param>
    private void CheckDictDict<K, T>(Dictionary<K, T> dict, string dictName, string tag, string key, bool isNullCheckContents = true)
    {
        if (dict == null)
        { Debug.LogFormat("{0}Invalid {1} (Null) for dictKey {2}{3}", tag, dictName, key, "\n"); }
        else
        {
            if (isNullCheckContents == true)
            {
                foreach(var record in dict)
                {
                    if (record.Value == null)
                    { Debug.LogFormat("{0}\"{1}\"[{2}] invalid (Null) for dictKey {3}{4}", tag, dictName, record.Key, key, "\n"); }
                }
            }
        }
    }

    /// <summary>
    /// Check a Dict array for null. Elements checked for null if 'isNullCheckContents' true
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="tag"></param>
    private void CheckDictArray<T>(T[] array, string arrayName, string tag, string key, bool isNullCheckContents = false)
    {
        if (array != null)
        {
            for (int i = 0; i < array.Length; i++)
            {
                //null checks on elements
                if (isNullCheckContents == true)
                {
                    T item = array[0];
                    if (item == null)
                    { Debug.LogFormat("{0}{1}[{2}] is invalid (Null) for dictKey {3}{4}", tag, arrayName, i, key, "\n"); }
                }
            }
        }
        else { Debug.LogFormat("{0}\"{1}\" Invalid (Null) for dictKey {2}{3}", tag, arrayName, key, "\n"); }
    }

    /// <summary>
    /// Bounds checks a dict array of Int's (each element checked if within lower and upper, inclusive) 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayName"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    private void CheckDictArrayBounds(int[] array, string arrayName, string tag, string key, int lower = 0, int upper = 0)
    {
        if (array != null)
        {
            if (lower != 0 || upper != 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] < lower || array[i] > upper)
                    { Debug.LogFormat("{0}\"{1}\"[{2}] failed bounds check, value {3) (should be between {4} and {5}){6}", tag, arrayName, i, array[i], lower, upper, "\n"); }
                }
            }
            else { Debug.LogWarningFormat("Invalid CheckDictArrayBounds for {0} check as both lower and upper bounds are Zero{1}", arrayName, "\n"); }
        }
        else { Debug.LogFormat("{0}\"{1}\" Invalid (Null) for dictKey {2}{3}", tag, arrayName, key, "\n"); }
    }


    /// <summary>
    /// Check a standalone (not enscapsulated within a class) list for null and check all entries in list for null (optional, default true)
    /// expectedCount used if you expect the list to have 'x' amount of records. Ignore otherwise.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="tag"></param>
    private void CheckList<T>(List<T> list, string listName, string tag, int expectedCount = 0, bool isNullCheckContents = true)
    {
        if (list != null)
        {
            //check number of records in list (optional)
            if (expectedCount > 0)
            {
                if (list.Count != expectedCount)
                { Debug.LogFormat("{0}Mismatch for {1} on count (was {2}, expected {3}){4}", tag, listName, list.Count, expectedCount, "\n"); }
            }
            //check for null records in list
            if (isNullCheckContents == true)
            {
                foreach (T item in list)
                {
                    if (item == null)
                    { Debug.LogFormat("{0}Invalid {1} (Null) in {2}{3}", tag, nameof(T), listName, "\n"); }
                }
            }
        }
        else { Debug.LogFormat("{0}\"{1}\" Invalid (Null){2}", tag, listName, "\n"); }
    }



    #endregion





    //new methods above here
}
