using dijkstraAPI;
using gameAPI;
using packageAPI;
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


    /// <summary>
    /// Not called for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                ValidateTargets();
                ValidateGear();
                ValidateMissions();
                ValidateTextLists();
                ValidateCities();
                ValidateTopics();
                break;
            case GameState.FollowOnInitialisation:
            //do nothing
            //break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
    }
    #endregion

    #endregion


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
                if (target.Value.ongoingEffect != null)
                {
                    //should be duration 'Ongoing'
                    if (target.Value.ongoingEffect.duration.name.Equals("Ongoing", StringComparison.Ordinal) == false)
                    { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" <b>ongoing effect \"{1}\" NOT Ongoing</b>", target.Value.targetName, target.Value.ongoingEffect.name); }
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

    #region ValidateTopics
    /// <summary>
    /// checks topicTypes and topicSubTypes are correctly set up
    /// </summary>
    private void ValidateTopics()
    {
        int count, countSubType;
        //
        // - - - TopicTypes and TopicSubTypes
        //
        TopicType[] arrayOfTopicTypes = GameManager.instance.loadScript.arrayOfTopicTypes;
        if (arrayOfTopicTypes != null)
        {
            TopicSubType[] arrayOfTopicSubTypes = GameManager.instance.loadScript.arrayOfTopicSubTypes;
            if (arrayOfTopicSubTypes != null)
            {
                List<TopicSubType> listOfSubTypes = new List<TopicSubType>();
                count = arrayOfTopicTypes.Length;
                //loop topicTypes
                for (int i = 0; i < count; i++)
                {
                    TopicType type = arrayOfTopicTypes[i];
                    if (type != null)
                    {
                        listOfSubTypes.Clear();
                        listOfSubTypes.AddRange(type.listOfSubTypes);
                        countSubType = arrayOfTopicSubTypes.Length;
                        //get all subTypes in arrayOfSubTypes that match the topicType and check that they present in their parent TopicType's listOfSubTypes
                        for (int j = 0; j < countSubType; j++)
                        {
                            TopicSubType subType = arrayOfTopicSubTypes[j];
                            if (subType != null)
                            {
                                if (subType.type.name.Equals(type.name, StringComparison.Ordinal) == true)
                                {
                                    if (listOfSubTypes.Remove(subType) == false)
                                    { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: topicSubType \"{0}\" not in topicType \"{1}\" listOfSubTopics{2}", subType.tag, type.tag, "\n"); }
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid topicSubType (Null) for topic \"{0}\"", type.name); }
                        }
                        if (listOfSubTypes.Count > 0)
                        {
                            //any remaining SubTypes in list must be duplicates
                            foreach (TopicSubType topicSubType in listOfSubTypes)
                            {
                                if (topicSubType != null)
                                {
                                    Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: topicSubType \"{0}\" in list but not present in arrayOfTopicSubTypes (Duplicate or Mismatch)",
                                      topicSubType.tag, "\n");
                                }
                                else { Debug.LogWarningFormat("Invalid topicSubType (Null) for Topic \"{0}\"", type.name); }
                            }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid TopicType (Null) in listOfTopicTypes[{0}]", i); }
                }
            }
            else { Debug.LogError("Invalid arrayOfTopicSubTypes (Null)"); }
        }
        else { Debug.LogError("Invalid arrayOfTopicTypes (Null)"); }
        //
        // - - - Topics and TopicOptions
        //
        Topic[] arrayOfTopics = GameManager.instance.loadScript.arrayOfTopics;
        TopicOption[] arrayOfTopicOptions = GameManager.instance.loadScript.arrayOfTopicOptions;
        if (arrayOfTopics != null)
        {
            if (arrayOfTopicOptions != null)
            {
                //create a list of option names that you can progressively delete from to check if any are left over at the end
                List<string> listOfOptionNames = arrayOfTopicOptions.Select(x => x.name).ToList();
                string topicName;
                //loop topics
                for (int i = 0; i < arrayOfTopics.Length; i++)
                {
                    Topic topic = arrayOfTopics[i];
                    if (topic != null)
                    {
                        topicName = topic.name;

                        //listOfOptions
                        if (topic.listOfOptions != null)
                        {
                            if (topic.listOfOptions.Count > 0)
                            {
                                //loop options and check that they aren't null and have the correct topic name
                                foreach (TopicOption option in topic.listOfOptions)
                                {
                                    if (option.topic.name.Equals(topicName, StringComparison.Ordinal) == false)
                                    {
                                        Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: option \"{0}\" for topic \"{1}\" has a mismatching topic (\"{2}\"){3}",
                                          option.name, topicName, option.topic.name, "\n");
                                    }
                                    //delete option from list
                                    if (listOfOptionNames.Remove(option.name) == false)
                                    { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: option \"{0}\" for topic \"{1}\" not found in arrayOfTopicOptions{2}", option.name, topicName, "\n"); }
                                }
                            }
                            else { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: topic \"{0}\" has an Empty listOfOptions{1}", topicName, "\n"); }
                        }
                        else { Debug.LogWarningFormat("Invalid listOfOptions (Null) for topic \"{0}\"", topic.name); }

                        //subType matches type
                        TopicType topicType = GameManager.instance.dataScript.GetTopicType(topic.type.name);
                        if (topicType != null)
                        {
                            //check topic subType is on the listOfSubTypes
                            if (topicType.listOfSubTypes.Find(x => x.name.Equals(topic.subType.name, StringComparison.Ordinal)) == false)
                            { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: topic \"{0}\" has a mismatch with it's subType \"{1}\"{2}", topicName, topic.subType.name, "\n"); }
                        }
                        else { Debug.LogErrorFormat("Invalid topicType (Null) for topic.type.name \"{0}\"", topic.type.name); }

                    }
                    else { Debug.LogWarningFormat("Invalid topic (Null) for arrayOfTopics[{0}]", i); }
                }
                //unused topicOptions
                if (listOfOptionNames.Count > 0)
                {
                    foreach (string optionName in listOfOptionNames)
                    { Debug.LogFormat("[Val] ValidationManager.cs -> ValidateTopics: option \"{0}\" NOT part of any Topic's listOfOptions{1}", optionName, "\n"); }
                }
            }
            else { Debug.LogError("Invalid arrayOfTopicOptions (Null)"); }
        }
        else { Debug.LogError("Invalid arrayOfTopics (Null)"); }
        //
        // - - - Topic Pools
        //
        TopicPool[] arrayOfTopicPools = GameManager.instance.loadScript.arrayOfTopicPools;
        if (arrayOfTopicPools != null)
        {
            count = arrayOfTopicPools.Length;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    TopicPool pool = arrayOfTopicPools[i];

                    if (pool.listOfTopics.Count > 0)
                    {
                        foreach (Topic topic in pool.listOfTopics)
                        {
                            //Check it has the correct TopicType/SubType to match the TopicPool
                        }
                    }
                    else { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: topicPool \"{0}\", has no listOfTopics{1}", pool.name, "\n"); }

                }
            }
            else { Debug.LogFormat("[Val] ValidationManager.cs->ValidateTopics: arrayOfTopicPools  has No Pools (Empty){1}", "\n"); }
        }
        else { Debug.LogError("Invalid arrayOfTopicPools (Null)"); }
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
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGlobalMeta);
        //GlobalChance
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGlobalChance);
        //GlobalType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGlobalType);
        //GlobalSide
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGlobalSide);
        //GlobalWho
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGlobalWho);
        //Criteria
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCriteria);
        //EffectApply
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfEffectApply);
        //EffectCriteria
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfEffectCriteria);
        //EffectDuration
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfEffectDuration);
        //EffectOperator
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfEffectOperator);
        //ContactType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfContactTypes);
        //TargetType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTargetTypes);
        //TargetTrigger
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTargetTriggers);
        //TargetProfile
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTargetProfiles);
        //Quality
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfQualities);
        //Condition
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfConditions);
        //Cure
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCures);
        //TraitCategory
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTraitCategories);
        //TraitEffect
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTraitEffects);
        //SecretType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfSecretTypes);
        //CityArc
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCityArcs);
        //City
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCities);
        //CitySize
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCitySize);
        //CitySpacing
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCitySpacing);
        //CityConnections
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCityConnections);
        //CitySecurity
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCitySecurity);
        //Damage
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfDamages);
        //Challenge
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfChallenges);
        //Nemesis
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfNemesis);
        //Rebel Leaders
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfRebelLeaders);
        //Scenario
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfScenarios);
        //Campaign
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfCampaigns);
        //NameSet
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfNameSets);
        //NodeDatapoint
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfNodeDatapoints);
        //NodeArc
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfNodeArcs);
        //NodeCrisis
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfNodeCrisis);
        //Trait
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTraits);
        //ActorArc
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfActorArcs);
        //Effect
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfEffects);
        //Action
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfActions);
        //TeamArc
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTeamArcs);
        //Gear
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGear);
        //GearRarity
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGearRarity);
        //GearType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfGearType);
        //TopicType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopicTypes);
        //TopicSubType
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopicSubTypes);
        //TopicOption
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopicOptions);
        //TopicPool
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopicPools);
        //Topics
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfTopics);
        //ManageActor
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfManageActors);
        //ManageAction
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfManageActions);
        //ActorConflict
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfActorConflicts);
        //Secret
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfSecrets);
        //Faction
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfFactions);
        //Objective
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfObjectives);
        //Organisation
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfOrganisations);
        //Mayor
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfMayors);
        //DecisionAI
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfDecisionAI);
        //Mission
        ValidateSOGeneric(GameManager.instance.loadScript.arrayOfMissions);
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
        int highestNodeID = GameManager.instance.nodeScript.nodeIDCounter;
        int highestConnID = GameManager.instance.nodeScript.connIDCounter;
        int highestSlotID = GameManager.instance.actorScript.maxNumOfOnMapActors - 1;
        int highestContactID = GameManager.instance.contactScript.contactIDCounter;
        int highestMessageID = GameManager.instance.messageScript.messageIDCounter;
        int highestTeamID = GameManager.instance.teamScript.teamIDCounter;
        int highestTurn = GameManager.instance.turnScript.Turn;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //debug checks
        Debug.Assert(highestActorID > 0, "Invalid highestActor (Zero or less)");
        Debug.Assert(highestNodeID > 0, "Invalid highestNodeID (Zero or less)");
        Debug.Assert(highestConnID > 0, "Invalid highestConnID (Zero or less)");
        Debug.Assert(highestSlotID > 0, "Invalid highestSlotID (Zero or less)");
        Debug.Assert(highestMessageID > 0, "Invalid highestMessageID (Zero or less)");
        Debug.Assert(highestContactID > 0, "Invalid highestContactID (Zero or less)");
        Debug.Assert(highestTeamID > 0, "Invalid highestTeamID (Zero or less)");
        Debug.Assert(playerSide != null, "Invalid playerSide (Null)");

        //run checks
        if (string.IsNullOrEmpty(prefix) == false)
        {
            Debug.LogFormat("{0}ExecuteIntegrityCheck: Commence checks...{1}", prefix, "\n");
            CheckNodeData(prefix, highestNodeID);
            CheckActorData(prefix, highestActorID, highestNodeID, highestSlotID);
            CheckTargetData(prefix, highestNodeID, highestContactID, highestTurn);
            CheckTeamData(prefix, highestNodeID, highestTeamID, highestSlotID, highestTurn, playerSide);
            CheckGearData(prefix);
            CheckConnectionData(prefix, highestNodeID, highestConnID);
            CheckMessageData(prefix, highestMessageID, highestTurn);
            CheckSecretData(prefix, highestActorID, highestTurn);
            CheckMainInfoData(prefix, highestTurn);
            CheckContactData(prefix, highestContactID, highestNodeID, highestActorID, highestTurn, playerSide);
            CheckPlayerData(prefix);
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
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
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
        int maxNumOfOnMapActors = GameManager.instance.actorScript.maxNumOfOnMapActors;
        int maxStatValue = GameManager.instance.actorScript.maxStatValue;
        int maxCompatibility = GameManager.instance.personScript.maxCompatibilityWithPlayer;
        int minCompatibility = GameManager.instance.personScript.minCompatibilityWithPlayer;
        int maxFactor = GameManager.instance.personScript.maxPersonalityFactor;
        int minFactor = GameManager.instance.personScript.minPersonalityFactor;
        /*int numOfActorArcs = GameManager.instance.dataScript.GetNumOfActorArcs();*/

        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfActors
        //
        Dictionary<int, Actor> dictOfActors = GameManager.instance.dataScript.GetDictOfActors();
        if (dictOfActors != null)
        {
            foreach (var actor in dictOfActors)
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
                        CheckList(personality.GetListOfDescriptors(), "personality.listOfDescriptors", tag);
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
        //
        // - - - arrayOfActors
        //
        Actor[,] arrayOfActors = GameManager.instance.dataScript.GetArrayOfActors();
        if (arrayOfActors != null)
        {
            bool[,] arrayOfActorsPresent = GameManager.instance.dataScript.GetArrayOfActorsPresent();
            if (arrayOfActorsPresent != null)
            {
                //check for actors being, or not being, present (eg. cross check both arrays to see if in synch)
                for (int outer = 1; outer < 3; outer++)
                {
                    for (int inner = 0; inner < maxNumOfOnMapActors; inner++)
                    {
                        if (arrayOfActorsPresent[outer, inner] == true)
                        {
                            //actor present
                            if (arrayOfActors[outer, inner] == null)
                            { Debug.LogFormat("{0}Invalid Actor (Null, shouldn't be as TRUE in arrayOfActorsPresent, in arrayOfActors[{1}, {2}]{3}", tag, outer, inner, "\n"); }
                        }
                        else
                        {
                            //no actor
                            if (arrayOfActors[outer, inner] != null)
                            { Debug.LogFormat("{0}Invalid Actor (should be Null as FALSE in arrayOfActorsPresent, in arrayOfActors[{1}, {2}]{3}", tag, outer, inner, "\n"); }
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid arrayOfActorsPresent (Null)"); }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
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
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfTargets
        //
        Dictionary<string, Target> dictOfTargets = GameManager.instance.dataScript.GetDictOfTargets();
        if (dictOfTargets != null)
        {
            foreach (var target in dictOfTargets)
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
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfTeams
        //
        Dictionary<int, Team> dictOfTeams = GameManager.instance.dataScript.GetDictOfTeams();
        if (dictOfTeams != null)
        {
            foreach (var team in dictOfTeams)
            {
                key = team.Key.ToString();
                CheckDictRange(team.Value.teamID, 0, highestTeamID, "teamID", tag, key);
                CheckDictString(team.Value.teamName, "teamName", tag, key);
                CheckDictObject(team.Value.arc, "arc", tag, key);
                if (team.Value.pool == TeamPool.OnMap)
                {
                    //OnMap team should have actor assigned only if Authority player side, default '-1' if playerSide
                    if (playerSide.level == globalAuthority.level)
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

    #region CheckGearData
    /// <summary>
    /// Integrity check for all gear related conditions
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckGearData(string prefix)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckGearData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfGear
        //
        Dictionary<string, Gear> dictOfGear = GameManager.instance.dataScript.GetDictOfGear();
        if (dictOfGear != null)
        {
            foreach (var gear in dictOfGear)
            {
                key = gear.Key;
                CheckDictObject(gear.Value, "Gear.Value", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfGear (Null)"); }
        //
        // - - - Gear lists
        //
        CheckList(GameManager.instance.dataScript.GetListOfGearRarity(), "listOfGearRarity", tag);
        CheckList(GameManager.instance.dataScript.GetListOfGearType(), "listOfGearType", tag);
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfCurrentGear(), "Gear", "gearName", "listOfCurrentGear");
        CheckListForDuplicates(GameManager.instance.dataScript.GetListOfLostGear(), "Gear", "gearName", "listOfLostGear");
    }
    #endregion

    #region CheckConnectionData
    /// <summary>
    /// Integrity check for all connection related collections
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckConnectionData(string prefix, int highestNodeID, int highestConnID)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckConnectionData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfConnections
        //
        Dictionary<int, Connection> dictOfConnections = GameManager.instance.dataScript.GetDictOfConnections();
        if (dictOfConnections != null)
        {
            foreach (var conn in dictOfConnections)
            {
                key = conn.Key.ToString();
                CheckDictRange(conn.Value.connID, 0, highestConnID, "connID", tag, key);
                CheckDictRange(conn.Value.node1.nodeID, 0, highestNodeID, "node1.nodeID", tag, key);
                CheckDictRange(conn.Value.node2.nodeID, 0, highestNodeID, "node2.nodeID", tag, key);
                CheckDictRange(conn.Value.VerticeOne, 0, highestNodeID, "VerticeOne", tag, key);
                CheckDictRange(conn.Value.VerticeTwo, 0, highestNodeID, "VerticeTwo", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfConnections (Null)"); }
        //
        // - - - listOfConnections
        //
        CheckList(GameManager.instance.dataScript.GetListOfConnections(), "listOfConnections", tag, highestConnID);
    }
    #endregion

    #region CheckMessageData
    /// <summary>
    /// Integrity check for all message related collections
    /// </summary>
    private void CheckMessageData(string prefix, int highestMessageID, int highestTurn)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckMessageData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfArchiveMessages
        //
        Dictionary<int, Message> dictOfArchiveMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Archive);
        if (dictOfArchiveMessages != null)
        {
            foreach (var message in dictOfArchiveMessages)
            {
                key = message.Key.ToString();
                CheckDictRange(message.Value.msgID, 0, highestMessageID, "msgID", tag, key);
                CheckDictRange(message.Value.turnCreated, 0, highestTurn, "turnCreated", tag, key);
                CheckDictString(message.Value.text, "text", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfArchiveMessages (Null)"); }
        //
        // - - - dictOfPendingMessages
        //
        Dictionary<int, Message> dictOfPendingMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Pending);
        if (dictOfPendingMessages != null)
        {
            foreach (var message in dictOfPendingMessages)
            {
                key = message.Key.ToString();
                CheckDictRange(message.Value.msgID, 0, highestMessageID, "msgID", tag, key);
                CheckDictRange(message.Value.turnCreated, 0, highestTurn, "turnCreated", tag, key);
                CheckDictString(message.Value.text, "text", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfPendingMessages (Null)"); }
        //
        // - - - dictOfCurrentMessages
        //
        Dictionary<int, Message> dictOfCurrentMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Current);
        if (dictOfCurrentMessages != null)
        {
            foreach (var message in dictOfCurrentMessages)
            {
                key = message.Key.ToString();
                CheckDictRange(message.Value.msgID, 0, highestMessageID, "msgID", tag, key);
                CheckDictRange(message.Value.turnCreated, 0, highestTurn, "turnCreated", tag, key);
                CheckDictString(message.Value.text, "text", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfCurrentMessages (Null)"); }
        //
        // - - - dictOfAIMessages
        //
        Dictionary<int, Message> dictOfAIMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.AI);
        if (dictOfAIMessages != null)
        {
            foreach (var message in dictOfAIMessages)
            {
                key = message.Key.ToString();
                CheckDictRange(message.Value.msgID, 0, highestMessageID, "msgID", tag, key);
                CheckDictRange(message.Value.turnCreated, 0, highestTurn, "turnCreated", tag, key);
                CheckDictString(message.Value.text, "text", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfAIMessages (Null)"); }
        //
        // - - - Duplicate check all dictionaries
        //
        CheckListForDuplicates(GameManager.instance.dataScript.GetMessageListOfID(MessageCategory.Archive), "Message", "msgID", "dictOfArchiveMessages");
        CheckListForDuplicates(GameManager.instance.dataScript.GetMessageListOfID(MessageCategory.Pending), "Message", "msgID", "dictOfPendingMessages");
        CheckListForDuplicates(GameManager.instance.dataScript.GetMessageListOfID(MessageCategory.Current), "Message", "msgID", "dictOfCurrentMessages");
        CheckListForDuplicates(GameManager.instance.dataScript.GetMessageListOfID(MessageCategory.AI), "Message", "msgID", "dictOfAIMessages");
    }
    #endregion

    #region CheckSecretData
    /// <summary>
    /// Integrity check for all secret related collections
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="highestActorID"></param>
    /// <param name="highestTurn"></param>
    private void CheckSecretData(string prefix, int highestActorID, int highestTurn)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckSecretData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfSecrets
        //
        Dictionary<string, Secret> dictOfSecrets = GameManager.instance.dataScript.GetDictOfSecrets();
        if (dictOfSecrets != null)
        {
            foreach (var secret in dictOfSecrets)
            {
                key = secret.Key;
                switch (secret.Value.status)
                {
                    case gameAPI.SecretStatus.Inactive:

                        break;
                    case gameAPI.SecretStatus.Active:
                        CheckDictRange(secret.Value.gainedWhen, 0, highestTurn, "gainedWhen", tag, key);
                        break;
                    case gameAPI.SecretStatus.Revealed:
                        CheckDictRange(secret.Value.gainedWhen, 0, highestTurn, "gainedWhen", tag, key);
                        CheckDictRange(secret.Value.revealedWhen, 0, highestTurn, "revealedWhen (turn)", tag, key);
                        CheckDictRange(secret.Value.revealedWho, 0, highestActorID, "revealedWho (actor)", tag, key);
                        break;
                    case gameAPI.SecretStatus.Deleted:
                        CheckDictRange(secret.Value.gainedWhen, 0, highestTurn, "gainedWhen", tag, key);
                        CheckDictRange(secret.Value.deletedWhen, 0, highestTurn, "deletedWhen", tag, key);
                        break;
                    default:
                        Debug.LogWarningFormat("Unrecognised secret.status \"{0}\" for {1}", secret.Value.status, key);
                        break;
                }
                CheckDictListBounds(secret.Value.GetListOfActors(), "listOfActors", tag, key, 0, highestActorID);
            }
        }
        else { Debug.LogError("Invalid dictOfSecrets (Null)"); }
        //
        // - - - lists Of Secrets
        //
        CheckList(GameManager.instance.dataScript.GetListOfPlayerSecrets(), "listOfPlayerSecrets", tag);
        CheckList(GameManager.instance.dataScript.GetListOfRevealedSecrets(), "listOfRevealedSecrets", tag);
        CheckList(GameManager.instance.dataScript.GetListOfDeletedSecrets(), "listOfDeletedSecrets", tag);
    }
    #endregion

    #region CheckMainInfoData
    /// <summary>
    /// Integrity check for all MainInfoData
    /// </summary>
    private void CheckMainInfoData(string prefix, int highestTurn)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckMainInfoData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfHistory
        //
        Dictionary<int, MainInfoData> dictOfHistory = GameManager.instance.dataScript.GetDictOfHistory();
        if (dictOfHistory != null)
        {
            foreach (var info in dictOfHistory)
            {
                key = info.Key.ToString();
                CheckDictRange(info.Key, 0, highestTurn, "turn", tag, key);
                CheckDictObject(info.Value, "MainInfoData", tag, key);
                CheckDictArray(info.Value.arrayOfItemData, "arrayOfItemData", tag, key, true);
                CheckDictString(info.Value.tickerText, "tickerText", tag, key);
            }
        }
    }
    #endregion

    #region CheckContactData
    /// <summary>
    /// Integrity check for all contact related collections
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="highestContactID"></param>
    /// <param name="highestNodeID"></param>
    /// <param name="highestActorID"></param>
    /// <param name="highestTurn"></param>
    private void CheckContactData(string prefix, int highestContactID, int highestNodeID, int highestActorID, int highestTurn, GlobalSide playerSide)
    {
        string key;
        string tag = string.Format("{0}{1}", prefix, "CheckContactData: ");
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        //
        // - - - dictOfContacts
        //
        Dictionary<int, Contact> dictOfContacts = GameManager.instance.dataScript.GetDictOfContacts();
        if (dictOfContacts != null)
        {
            foreach (var contact in dictOfContacts)
            {
                key = contact.Key.ToString();
                CheckDictRange(contact.Value.contactID, 0, highestContactID, "contactID", tag, key);
                CheckDictString(contact.Value.nameFirst, "nameFirst", tag, key);
                CheckDictString(contact.Value.nameLast, "nameLast", tag, key);
                CheckDictString(contact.Value.job, "job", tag, key);
                CheckDictRange(contact.Value.statsRumours, 0, 99, "statsRumour", tag, key);
                CheckDictRange(contact.Value.statsNemesis, 0, 99, "statsNemesis", tag, key);
                CheckDictRange(contact.Value.statsTeams, 0, 99, "statsTeams", tag, key);
                switch (contact.Value.status)
                {
                    case ContactStatus.Active:
                        CheckDictRange(contact.Value.actorID, 0, highestActorID, "actorID", tag, key);
                        CheckDictRange(contact.Value.nodeID, 0, highestNodeID, "nodeID", tag, key);
                        CheckDictRange(contact.Value.turnStart, 0, highestTurn, "turnStart", tag, key);
                        CheckDictString(contact.Value.typeName, "typeName", tag, key);
                        break;
                    case ContactStatus.Inactive:
                        CheckDictRange(contact.Value.turnFinish, 0, highestTurn, "turnFinish", tag, key);
                        CheckDictString(contact.Value.typeName, "typeName", tag, key);
                        break;
                }
                CheckDictRange(contact.Value.effectiveness, 1, 3, "effectiveness", tag, key);
            }
        }
        else { Debug.LogError("Invalid dictOfContacts (Null)"); }
        //
        // - - - Resistance only
        //
        if (playerSide.level == globalResistance.level)
        {
            CheckListForDuplicates(GameManager.instance.dataScript.GetContactPool(), "Contact", "contactID", "contactPool");
            //dictOfActorContacts
            Dictionary<int, List<int>> dictOfActorContacts = GameManager.instance.dataScript.GetDictOfActorContacts();
            if (dictOfActorContacts != null)
            {
                foreach (var record in dictOfActorContacts)
                {
                    key = record.Key.ToString();
                    CheckDictRange(record.Key, 0, highestActorID, "actorID", tag, key);
                    CheckDictListBounds(record.Value, "dictOfActorContacts.List<int>", tag, key, 0, highestNodeID);
                    CheckListForDuplicates(record.Value, "Node", "nodeID", string.Format("dictOfActorContacts for actorID {0}", record.Key));

                }
            }
            else { Debug.LogError("Invalid dictOfActorContacts (Null)"); }
            //dictOfNodeContactsResistance
            Dictionary<int, List<int>> dictOfNodeContactsResistance = GameManager.instance.dataScript.GetDictOfNodeContacts(globalResistance);
            if (dictOfNodeContactsResistance != null)
            {
                foreach (var record in dictOfNodeContactsResistance)
                {
                    key = record.Key.ToString();
                    CheckDictRange(record.Key, 0, highestNodeID, "nodeID", tag, key);
                    CheckDictListBounds(record.Value, "dictOfNodeContactsResistance.List<int>", tag, key, 0, highestActorID);
                    CheckListForDuplicates(record.Value, "Actor", "actorID", string.Format("dictOfNodeContactsResistance for nodeID {0}", record.Key));
                }
            }
            else { Debug.LogError("Invalid dictOfNodeContactsResistance (Null)"); }
            //dictOfContactsByNodeResistance
            Dictionary<int, List<Contact>> dictOfContactsByNodeResistance = GameManager.instance.dataScript.GetDictOfContactsByNodeResistance();
            if (dictOfContactsByNodeResistance != null)
            {
                foreach (var record in dictOfContactsByNodeResistance)
                {
                    key = record.Key.ToString();
                    CheckDictRange(record.Key, 0, highestNodeID, "nodeID", tag, key);
                    CheckDictList(record.Value, "dictOfNodeContactsResistance.List<Contact>", tag, key);
                }
            }
            else { Debug.LogError("Invalid dictOfContactsByNodeResistance (Null)"); }
        }
        //
        // - - - Both sides
        //
        //dictOfNodeContactsAuthority
        Dictionary<int, List<int>> dictOfNodeContactsAuthority = GameManager.instance.dataScript.GetDictOfNodeContacts(globalAuthority);
        if (dictOfNodeContactsAuthority != null)
        {
            foreach (var record in dictOfNodeContactsAuthority)
            {
                key = record.Key.ToString();
                List<int> tempList = record.Value;
                CheckListForDuplicates(tempList, "Actor", "actorID", string.Format("dictOfNodeContactsAuthority for nodeID {0}", record.Key));
            }
        }
        else { Debug.LogError("Invalid dictOfNodeContactsAuthority (Null)"); }
    }
    #endregion

    #region CheckPlayerData
    /// <summary>
    /// Integrity check of all dynamic Player data
    /// </summary>
    /// <param name="prefix"></param>
    private void CheckPlayerData(string prefix)
    {
        string tag = string.Format("{0}{1}", prefix, "CheckPlayerData: ");
        int maxFactor = GameManager.instance.personScript.maxPersonalityFactor;
        int minFactor = GameManager.instance.personScript.minPersonalityFactor;
        //Debug.LogFormat("{0}checking . . . {1}", tag, "\n");
        CheckRange(GameManager.instance.playerScript.GetMood(), 0, GameManager.instance.playerScript.moodMax, "mood", tag);
        CheckRange(GameManager.instance.playerScript.actorID, 999, 999, "actorID", tag);
        //personality
        Personality personality = GameManager.instance.playerScript.GetPersonality();
        if (personality != null)
        {
            CheckArrayBounds(personality.GetFactors(), "arrayOfFactors", tag, minFactor, maxFactor);
            if (personality.GetProfile() != null)
            {
                CheckString(personality.GetProfileDescriptor(), "profileDescriptor", tag);
                CheckString(personality.GetProfileExplanation(), "profileExplanation", tag);
                CheckList(personality.GetListOfDescriptors(), "personality.listOfDescriptors", tag);
                CheckList(personality.GetListOfMotivation(), "personality.listOfMotivation", tag);
            }
        }
        else { Debug.LogFormat("{0}Invalid personality (Null) for Player {1}", tag, "\n"); }
        //collections
        CheckList(GameManager.instance.playerScript.GetListOfGear(), "listOfGear", tag);
        CheckListForDuplicates(GameManager.instance.playerScript.GetListOfGear(), "Gear", "gearName", "ListOfGear");
        CheckList(GameManager.instance.playerScript.GetListOfConditions(globalAuthority), "listOfConditionsAuthority", tag);
        CheckList(GameManager.instance.playerScript.GetListOfConditions(globalResistance), "listOfConditionsResistance", tag);
        CheckList(GameManager.instance.playerScript.GetListOfSecrets(), "listOfSecrets", tag);
        CheckList(GameManager.instance.playerScript.GetListOfMoodHistory(), "listOfModdHistory", tag);

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
        { Debug.LogFormat("{0}Invalid {1} (Null or Empty) for dictKey {2}{3}", tag, stringName, key, "\n"); }
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
                foreach (var record in dict)
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

    /// <summary>
    /// Check a string for Null or Empty
    /// </summary>
    /// <param name="text"></param>
    /// <param name="stringName"></param>
    /// <param name="tagy"></param>
    private void CheckString(string text, string stringName, string tagy)
    {
        if (string.IsNullOrEmpty(text) == true)
        { Debug.LogFormat("{0}Invalid {1} (Null or Empty){2}", tag, stringName, "\n"); }
    }

    /// <summary>
    /// Check an int is within a range (inclusive)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="varName"></param>
    /// <param name="tag"></param>
    private void CheckRange(int value, int lower, int upper, string varName, string tag)
    {
        if (value < lower || value > upper)
        { Debug.LogFormat("{0} variable \"{1}\", value {2}, outside of range ({3} to {4}){5}", tag, varName, value, lower, upper, "\n"); }
    }

    /// <summary>
    /// Bounds checks a dict array of Int's (each element checked if within lower and upper, inclusive) 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayName"></param>
    /// <param name="tag"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    private void CheckArrayBounds(int[] array, string arrayName, string tag, int lower = 0, int upper = 0)
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
        else { Debug.LogFormat("{0}\"{1}\" Invalid (Null){2}", tag, arrayName, "\n"); }
    }


    #endregion





    //new methods above here
}
