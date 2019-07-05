using gameAPI;
using System;
using System.Collections;
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
    /// <summary>
    /// Master control for all validations
    /// </summary>
    public void Initialise(GameState state)
    {
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
        foreach(var target in dictOfTargets)
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
                else {Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Gear (Null)", target.Value.targetName); }
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
                        foreach(var effect in gear.Value.listOfPersonalEffects)
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
                else{ Debug.LogFormat("[Val] ValidateGar: Invalid Gear (Null) for gearID {0}", gear.Key); }
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
            foreach(Mission mission in arrayOfMissions)
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
                        foreach(ObjectiveTarget objTarget in mission.listOfObjectiveTargets)
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
                    foreach(String item in tempList)
                    {
                        findList.Clear();
                        findList = textList.randomList.FindAll(x => x == item);
                        count = findList.Count;
                        //always one copy present, shouldn't be any more
                        if (count > 1)
                        {
                            //ignore first, legit, copy
                            count--;
                            Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTextList: {0} Duplicate{1} exist for \"{2}\" in textList {3}{4}", count, count != 1 ? "s" : "", item, textList.name, "\n");                }
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

    #region Utilities

    #region CheckListForDuplicates
    /// <summary>
    /// generic method that takes any list of IComparables, eg. int, string, but NOT SO's, and checks for duplicates. Debug.LogFormat("[Val]"...) messages generated for dupes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="listToCheck"></param>
    private void CheckListForDuplicates<T>(List<T> listToCheck, string typeOfObject = "Unknown", string nameOfObject = "Unknown", string nameOfList = "Unknown")  where T : IComparable
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
                    foreach(var item in query)
                    {
                        numOfDuplicates = item.Counter - 1;
                        Debug.LogFormat("[Val] ValidationManager.cs -> CheckListForDuplicates: {0} \"{1}\" , list \"{2}\", record \"{3}\" has {4} duplicate{5}{6}", typeOfObject, nameOfObject, nameOfList, 
                            item.Element, numOfDuplicates, numOfDuplicates != 1 ? "s" : "", "\n"); }
                }
            }
        }
        else { Debug.LogError("Invalid listToCheck (Null)"); }
    }
    #endregion

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


    //
    // - - - Integrity Check - - -
    //

    /// <summary>
    /// Master method to run all data Integrity checks
    /// </summary>
    public void ExecuteIntegrityCheck()
    {
        string prefix = "[Val] ValidationManager.cs -> ";
        if (string.IsNullOrEmpty(prefix) == false)
        {
            Debug.LogFormat("{0}ExecuteIntegrityCheck: Commence checks - - - - - - {1}", prefix, "\n");
            CheckNodeData(prefix);
        }
    }

    /// <summary>
    /// Integrity check on all node related data collections
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckNodeData(string prefix)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckNodeData: ");
        int highestNode = GameManager.instance.nodeScript.nodeCounter;
        Debug.LogFormat("{0}commencing checks . . . {1}", tag, "\n");
        //check dictionaries all have the same number of entries
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        if (dictOfNodes != null)
        {
            //check count
            if (dictOfNodes.Count != highestNode)
            { Debug.LogFormat("{0}Incorrect count, dictOfNodes has {1} records, highestNode {2}{3}", tag, dictOfNodes.Count, highestNode, "\n"); }
            foreach(var node in dictOfNodes)
            {
                //null check record
                if (node.Value != null)
                {
                    key = node.Key.ToString();
                    //range check nodeID
                    CheckRange(node.Key, 0, highestNode, tag, key);
                    CheckString(node.Value.nodeName, tag, key);
                    CheckObject(node.Value.Arc, tag, key);
                    CheckObject(node.Value.launcher, tag, key);
                    CheckObject(node.Value.nodePosition, tag, key);
                    CheckObject(node.Value.loiter, tag, key);
                    CheckList(node.Value.GetListOfTeams(), tag, key);
                    CheckList(node.Value.GetListOfOngoingEffects(), tag, key);
                    CheckList(node.Value.GetNeighbouringNodes(), tag, key);
                    CheckList(node.Value.GetNearNeighbours(), tag, key);
                    CheckList(node.Value.GetListOfConnections(), tag, key);
                }
                else { Debug.LogFormat("{0}Invalid entry (Null) in dictOfNodes for nodeID {1}{2}", node.Key, "\n"); }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
    }


    //
    // - - - SubMethods
    //

    /// <summary>
    /// Checks that an int value is inside specified range (inclusive)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    private void CheckRange(int value, int lower, int upper, string tag, string key)
    {
        if (value < lower || value > upper)
        { Debug.LogFormat("{0}dictKey \"{1}\" outside of range ({2} to {3}){4}", tag, key, lower, upper, key, "\n"); }
    }

    /// <summary>
    /// Checks string for Null and Empty
    /// </summary>
    /// <param name="text"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    private void CheckString(string text, string tag, string key)
    {
        if (string.IsNullOrEmpty(text) == true)
        { Debug.LogFormat("{0}Invalid {1} (Null) for dictKey {2}{3}", tag, text, key, "\n"); }
    }

    /// <summary>
    /// Checks any object for null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="thing"></param>
    /// <param name="tag"></param>
    /// <param name="key"></param>
    private void CheckObject<T>(T thing, string tag, string key)
    {
        if (thing == null)
        { Debug.LogFormat("{0}Invalid {1} (Null) for dictKey {2}{3}", tag, nameof(T), key, "\n"); }
    }

    /// <summary>
    /// Check list for Null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="key"></param>
    /// <param name="tag"></param>
    private void CheckList<T>(List<T> list, string key, string tag)
    {
        if (list == null)
        { Debug.LogFormat("{0}Invalid {1} (Null) for dictKey {2}{3}", tag, nameof(T), key, "\n"); }
    }

    //new methods above here
}
