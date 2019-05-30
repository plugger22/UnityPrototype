using gameAPI;
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
    public void Initialise(GameState state)
    {
        ValidateTargets();
        ValidateGear();
    }

    #region ValidateTargets
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
                if (string.IsNullOrEmpty(target.descriptorResistance) == true)
                { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Invalid description field (Null or Empty)", target.targetName); }
                if (target.profile.activation == null)
                { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Invalid activation field (Null)", target.targetName); }
                if (target.nodeArc == null)
                { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Invalid NodeArc field (Null)", target.targetName); }
                if (target.actorArc == null)
                { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Invalid ActorArc field (Null)", target.targetName); }
                if (target.gear == null)
                { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Invalid Gear field (Null)", target.targetName); }
                if (target.sprite == null)
                { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Invalid sprite field (Null)", target.targetName); }
                //Good effects
                if (target.listOfGoodEffects.Count > 0)
                {
                    foreach (Effect effect in target.listOfGoodEffects)
                    {
                        if (effect != null)
                        {
                            //should be duration 'Single'
                            if (effect.duration.name.Equals("Single") == false)
                            { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Good effect \"{1}\" NOT Single", target.targetName, effect.name); }
                        }
                        else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Good effect (Null)", target.targetName); }
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
                            { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Bad effect \"{1}\" NOT Single", target.targetName, effect.name); }
                        }
                        else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Bad effect (Null)", target.targetName); }
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
                            { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" Fail effect \"{1}\" NOT Single", target.targetName, effect.name); }
                        }
                        else { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Fail effect (Null)", target.targetName); }
                    }
                }
                //Ongoing effects
                if (target.OngoingEffect != null)
                {
                    //should be duration 'Ongoing'
                    if (target.OngoingEffect.duration.name.Equals("Ongoing") == false)
                    { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" <b>ongoing effect \"{1}\" NOT Ongoing</b>", target.targetName, target.OngoingEffect.name); }
                }
                //Target gear present and shouldn't be infiltration
                if (target.gear != null)
                {
                    if (target.gear.name.Equals("Infiltration") == true)
                    { Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\" <b>is using INFILTRATION gear</b> (doubled up as can be used automatically on any target)", target.targetName); }
                }
                else {Debug.LogFormat("[Val] ValidateTargets: Target \"{0}\"  invalid Gear (Null)", target.targetName); }
            }
            else { Debug.LogErrorFormat("Invalid target (Null) for targetID {0}", index); }
        }
    }
    #endregion

    #region ValidateGear
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
        //SecretStatus
        ValidateSOGeneric<SecretStatus>(GameManager.instance.loadScript.arrayOfSecretStatus);
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
                    if (Array.Exists(arrayTemp, element => element.name.Equals(meta.name)) == false)
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

    //new methods above here
}
