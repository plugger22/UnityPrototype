﻿using toolsAPI;
using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

/// <summary>
/// Handles all matters related to ActorPool.SO and ActorDraft.SO
/// </summary>
public class ActorPoolManager : MonoBehaviour
{
    [Header("Details")]
    [Tooltip("Number of ActorDrafts in a pool -> 4 x hq hierarchy, 8 x workers, 4 x OnMap, 5 + 9 x level One, 9 x level Two, 9 x level three")]
    [Range(48, 48)] public int numOfActors = 48;
    [Tooltip("Number of ActorDrafts for HQ heirarchy (duplicate of HQManager.cs -> numOfActorsHQ")]
    [Range(4, 4)] public int numOfActorsHQ = 4;
    [Tooltip("Number of Power Factor for HQ Hierarchy (duplicate of HQManager.cs -> hqPowerFactor")]
    [Range(10, 10)] public int hqPowerFactor = 10;

    //private
    private ActorDraftSex sexMale;
    private ActorDraftSex sexFemale;
    private ActorDraftStatus statusHqBoss0;
    private ActorDraftStatus statusHqBoss1;
    private ActorDraftStatus statusHqBoss2;
    private ActorDraftStatus statusHqBoss3;
    private ActorDraftStatus statusHqWorker;
    private ActorDraftStatus statusOnMap;
    private ActorDraftStatus statusPool;
    private NameSet nameSet;
    private List<ActorArc> listOfActorArcs;
    private List<ActorArc> listOfTempArcs;                      //used for processing
    private List<Trait> listOfTraits;
    private List<Trait> listOfTempTraits;
    private List<NameSet> listOfNameSets;
     //duplicate
    private int index;
    private int counter;
    private int numOfTraits;

    #region Initialise
    /// <summary>
    /// Initialisation. Called only when needed, eg. CreateActorPool
    /// </summary>
    public void Initialise()
    {
        int num;
        //
        // - - - ActorDraftSex globals
        //
        ActorDraftSex[] arrayOfActorSex = ToolManager.i.jointScript.arrayOfActorDraftSex;
        num = arrayOfActorSex.Length;
        if (num > 0)
        {
            for (int i = 0; i < num; i++)
            {
                ActorDraftSex assetSO = arrayOfActorSex[i];
                switch (assetSO.name)
                {
                    case "Male": sexMale = assetSO; break;
                    case "Female": sexFemale = assetSO; break;
                    default: Debug.LogWarningFormat("Unrecognised ActorDraftSex \"{0}\"", assetSO.name); break;
                }
            }
            //error check
            if (sexMale == null) { Debug.LogError("Invalid sexMale (Null)"); }
            if (sexFemale == null) { Debug.LogError("Invalid sexFemale (Null)"); }
        }
        //
        // - - - ActorDraftStatus globals
        //
        ActorDraftStatus[] arrayOfActorStatus = ToolManager.i.jointScript.arrayOfActorDraftStatus;
        num = arrayOfActorStatus.Length;
        if (num > 0)
        {
            for (int i = 0; i < num; i++)
            {
                ActorDraftStatus assetSO = arrayOfActorStatus[i];
                switch (assetSO.name)
                {
                    case "HqBoss0": statusHqBoss0 = assetSO; break;
                    case "HqBoss1": statusHqBoss1 = assetSO; break;
                    case "HqBoss2": statusHqBoss2 = assetSO; break;
                    case "HqBoss3": statusHqBoss3 = assetSO; break;
                    case "HqWorker": statusHqWorker = assetSO; break;
                    case "OnMap": statusOnMap = assetSO; break;
                    case "Pool": statusPool = assetSO; break;
                    default: Debug.LogWarningFormat("Unrecognised ActorDraftStatus \"{0}\"", assetSO.name); break;
                }
            }
            //error check
            if (statusHqBoss0 == null) { Debug.LogError("Invalid statusHqBoss0 (Null)"); }
            if (statusHqBoss1 == null) { Debug.LogError("Invalid statusHqBoss1 (Null)"); }
            if (statusHqBoss2 == null) { Debug.LogError("Invalid statusHqBoss2 (Null)"); }
            if (statusHqBoss3 == null) { Debug.LogError("Invalid statusHqBoss3 (Null)"); }
            if (statusHqWorker == null) { Debug.LogError("Invalid statusHqWorker (Null)"); }
            if (statusOnMap == null) { Debug.LogError("Invalid statusOnMap (Null)"); }
            if (statusPool == null) { Debug.LogError("Invalid statusPool (Null)"); }
        }
        //
        // - - - Traits (no Mayor traits in array)
        //
        listOfTraits = new List<Trait>();
        Trait[] arrayOfTraits = ToolManager.i.jointScript.arrayOfTraits;
        Trait trait;
        if (arrayOfTraits != null)
        {
            //loop traits and place those that apply to the resistance, or both, into the listOfTraits
            for (int i = 0; i < arrayOfTraits.Length; i++)
            {
                trait = arrayOfTraits[i];
                if (trait != null)
                {
                    switch (trait.side.name)
                    {
                        case "Both":
                        case "Resistance":
                            listOfTraits.Add(trait);
                            break;
                    }
                }
                else { Debug.LogWarningFormat("Invalid trait (Null) for arrayOfTraits[{0}]", i); }
            }
            if (listOfTraits.Count == 0)
            { Debug.LogError("Invalid listOfTraits (Empty)"); }
            else
            {
                numOfTraits = listOfTraits.Count;
                //initialise listOfTempTraits
                listOfTempTraits = new List<Trait>();
                listOfTempTraits.AddRange(listOfTraits);
            }
        }
        else { Debug.LogError("Invalid arrayOfTraits (Null)"); }


        //
        // - - - Nameset (All)
        //
        listOfNameSets = ToolManager.i.jointScript.GetListOfNameSets();
        if (listOfNameSets != null)
        {
            if (listOfNameSets.Count == 0) { Debug.LogError("Invalid listOfNameSets (Empty)"); }
        }
        else { Debug.LogError("Invalid listOfNameSets (Null)"); }
        

        //
        // - - - Collections
        //
        listOfActorArcs = ToolManager.i.jointScript.GetActorArcs(ToolManager.i.jointScript.sideResistance);
        if (listOfActorArcs == null) { Debug.LogError("Invalid listOfActorArcs (Null)"); }
        else
        {
            //initialise temp list of Arcs by Value (not reference as I'll be deleting some)
            listOfTempArcs = new List<ActorArc>();
            listOfTempArcs.AddRange(listOfActorArcs);
        }       
    }
    #endregion

    #region CreateActorPool
    /// <summary>
    /// Master program
    /// </summary>
    public void CreateActorPool(ActorPoolData data)
    {
        Debug.Assert(data != null, "Invalid ActorPoolData (Null)");
        Initialise();
        InitialiseActorPool(data);
    }
    #endregion

    #region InitialiseActorPool
    /// <summary>
    /// Creates an actorPool and populates with ActorDraft.SO's. 
    /// Note: data checked by parent method
    /// </summary>
    private void InitialiseActorPool(ActorPoolData data)
    {
        string pathPool, pathDraft;
        //
        // - - - ActorPool
        //
        ActorPool poolAsset = ScriptableObject.CreateInstance<ActorPool>();
        //path with unique asset name for each
        pathPool = string.Format("Assets/SO/ActorPools/{0}.asset", data.poolName);
        //how many actors required to populate pool
        int numToCreate = numOfActors;
        poolAsset.nameSet = data.nameSet;
        poolAsset.tag = data.tag;
        poolAsset.side = data.side;
        poolAsset.author = data.author;
        poolAsset.dateCreated = data.dateCreated;
        //
        // - - - ActorDrafts
        //
        for (int i = 0; i < numToCreate; i++)
        {
            //create SO object
            ActorDraft actorAsset = ScriptableObject.CreateInstance<ActorDraft>();
            //overwrite default data
            UpdateActorDraft(poolAsset, actorAsset, i);
            //path with unique asset name for each (poolName + counter)
            pathDraft = string.Format("Assets/SO/ActorDrafts/{0}{1}.asset", data.poolName, i);
            //delete any existing asset at the same location (if same named asset presnet, new asset won't overwrite)
            AssetDatabase.DeleteAsset(pathDraft);
            //Add asset to file and give it a name ('actor')
            AssetDatabase.CreateAsset(actorAsset, pathDraft);
        }
        //delete any existing asset at the same location (if same named asset presnet, new asset won't overwrite)
        AssetDatabase.DeleteAsset(pathPool);
        //initialise pool
        AssetDatabase.CreateAsset(poolAsset, pathPool);
        //Save assets to disk
        AssetDatabase.SaveAssets();       
    }
    #endregion

    #region UpdateActorDraft
    /// <summary>
    /// Initialises data for a newly created ActorDraft. 
    /// </summary>
    /// <param name="actor"></param>
    private void UpdateActorDraft(ActorPool pool, ActorDraft actor, int num)
    {
        if (actor != null)
        {
            //
            // - - - name and sex
            //
            if (Random.Range(0, 100) < 50)
            {
                actor.sex = sexMale;
                actor.firstName = nameSet.firstMaleNames.GetRandomRecord();
            }
            else
            {
                actor.sex = sexFemale;
                actor.firstName = nameSet.firstFemaleNames.GetRandomRecord();
            }
            actor.lastName = nameSet.lastNames.GetRandomRecord();
            actor.actorName = string.Format("{0} {1}", actor.firstName, actor.lastName);
            //
            // - - - status, arc, level and power -> assign to actorPool
            //
            switch (num)
            {
                case 0:
                    actor.status = statusHqBoss0;
                    actor.arc = listOfActorArcs[Random.Range(0, listOfActorArcs.Count)];
                    actor.sprite = actor.arc.sprite;
                    actor.power = (numOfActorsHQ + 2 - (int)ActorHQ.Boss) * hqPowerFactor;
                    actor.level = 3;
                    actor.trait = GetTrait();
                    //add to pool (do after all data initialisation)
                    pool.hqBoss0 = actor;
                    break;
                case 1:
                    actor.status = statusHqBoss1;
                    actor.arc = listOfActorArcs[Random.Range(0, listOfActorArcs.Count)];
                    actor.sprite = actor.arc.sprite;
                    actor.power = (numOfActorsHQ + 2 - (int)ActorHQ.SubBoss1) * hqPowerFactor;
                    actor.level = 2;
                    actor.trait = GetTrait();
                    pool.hqBoss1 = actor;
                    break;
                case 2:
                    actor.status = statusHqBoss2;
                    actor.arc = listOfActorArcs[Random.Range(0, listOfActorArcs.Count)];
                    actor.sprite = actor.arc.sprite;
                    actor.power = (numOfActorsHQ + 2 - (int)ActorHQ.SubBoss2) * hqPowerFactor;
                    actor.level = 2;
                    actor.trait = GetTrait();
                    pool.hqBoss2 = actor;
                    break;
                case 3:
                    actor.status = statusHqBoss3;
                    actor.arc = listOfActorArcs[Random.Range(0, listOfActorArcs.Count)];
                    actor.sprite = actor.arc.sprite;
                    actor.power = (numOfActorsHQ + 2 - (int)ActorHQ.SubBoss3) * hqPowerFactor;
                    actor.level = 2;
                    actor.trait = GetTrait();
                    pool.hqBoss3 = actor;
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    //workers
                    actor.status = statusHqWorker;
                    actor.arc = listOfActorArcs[Random.Range(0, listOfActorArcs.Count)];
                    actor.sprite = actor.arc.sprite;
                    actor.power = Random.Range(1, 6);
                    actor.level = 2;
                    actor.trait = GetTrait();
                    pool.listHqWorkers.Add(actor);
                    break;
                case 12:
                case 13:
                case 14:
                case 15:
                    //OnMap
                    actor.status = statusOnMap;
                    index = Random.Range(0, listOfTempArcs.Count);
                    actor.arc = listOfTempArcs[index];
                    actor.sprite = actor.arc.sprite;
                    //remove arc from temp list to prevent dupes
                    listOfTempArcs.RemoveAt(index);
                    actor.power = 0;
                    actor.level = 1;
                    actor.trait = GetTrait();
                    pool.listOnMap.Add(actor);
                    //set counter to 0 for remaining level 1 actors using left over temp Arcs
                    if (num == 15) { counter = 0; }
                    break;
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                    //Level one, remaining temp arcs
                    actor.status = statusPool;
                    actor.arc = listOfTempArcs[counter++];
                    actor.sprite = actor.arc.sprite;
                    actor.power = 0;
                    actor.level = 1;
                    actor.trait = GetTrait();
                    pool.listLevelOne.Add(actor);
                    //reset counter ready for a full arc set of level one actors
                    if (num == 20) { counter = 0; }
                    break;
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                    //Level one, an additional full set of 9
                    actor.status = statusPool;
                    actor.arc = listOfActorArcs[counter++];
                    actor.sprite = actor.arc.sprite;
                    actor.power = 0;
                    actor.level = 1;
                    actor.trait = GetTrait();
                    pool.listLevelOne.Add(actor);
                    //reset counter ready for a full arc set of level two actors
                    if (num == 29) { counter = 0; }
                    break;
                case 30:
                case 31:
                case 32:
                case 33:
                case 34:
                case 35:
                case 36:
                case 37:
                case 38:
                    //Level two, a full set of 9
                    actor.status = statusPool;
                    actor.arc = listOfActorArcs[counter++];
                    actor.sprite = actor.arc.sprite;
                    actor.power = 0;
                    actor.level = 2;
                    actor.trait = GetTrait();
                    pool.listLevelTwo.Add(actor);
                    //reset counter ready for a full arc set of level three actors
                    if (num == 38) { counter = 0; }
                    break;
                case 39:
                case 40:
                case 41:
                case 42:
                case 43:
                case 44:
                case 45:
                case 46:
                case 47:
                    //Level three, a full set of 9
                    actor.status = statusPool;
                    actor.arc = listOfActorArcs[counter++];
                    actor.sprite = actor.arc.sprite;
                    actor.power = 0;
                    actor.level = 3;
                    actor.trait = GetTrait();
                    pool.listLevelThree.Add(actor);
                    break;
                default: Debug.LogWarningFormat("Unrecognised num \"{0}\"", num); break;
            }
        }
        else { Debug.LogError("Invalid actorDraft (Null)"); }
    }
    #endregion

    #region GetTrait
    /// <summary>
    /// Returns a trait. Two stage process. First returns a random trait from tempList to ensure that all traits are used at least once. Then, once tempList exhausted, returns a random trait from listOfTraits (dupes possible)
    /// Return null if a problem but generates an error within method if so
    /// </summary>
    /// <returns></returns>
    private Trait GetTrait()
    {
        Trait trait = null;
        if (listOfTempTraits.Count > 0)
        {
            index = Random.Range(0, listOfTempTraits.Count);
            trait = listOfTempTraits[index];
            listOfTempTraits.RemoveAt(index);
        }
        else
        { trait = listOfTraits[Random.Range(0, numOfTraits)]; }
        //error check
        if (trait == null)
        { Debug.LogErrorFormat("Invalid trait (Null), listOfTempTraits.Count {0}, numOfTraits {1}", listOfTempTraits.Count, numOfTraits); }
        return trait;
    }
    #endregion

    #region Utilities...

    /// <summary>
    /// returns listOfTraits for the current side and traits that apply to all sides
    /// </summary>
    /// <returns></returns>
    public List<Trait> GetListOfTraits()
    { return listOfTraits; }


    #endregion

    //new methods above here 
}
