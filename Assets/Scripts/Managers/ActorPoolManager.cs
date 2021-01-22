using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Handles all matters related to ActorPool.SO and ActorDraft.SO
/// </summary>
public class ActorPoolManager : MonoBehaviour
{
    private ActorDraftSex sexMale;
    private ActorDraftSex sexFemale;
    private ActorDraftStatus statusHqBoss0;
    private ActorDraftStatus statusHqBoss1;
    private ActorDraftStatus statusHqBoss2;
    private ActorDraftStatus statusHqBoss3;
    private ActorDraftStatus statusHqWorker;
    private ActorDraftStatus statusOnMap;
    private ActorDraftStatus statusPool;
    private ActorDraftStatus statusReserves;
    private NameSet nameSet;
    private List<ActorArc> listOfActorArcs;
    private int numOfActorsHQ;
    private int hqPowerFactor;

    /// <summary>
    /// Initialisation
    /// </summary>
    private void Initialise()
    {
        int num;
        //
        // - - - ActorDraftSex globals
        //
        ActorDraftSex[] arrayOfActorSex = GameManager.i.loadScript.arrayOfActorDraftSex;
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
        ActorDraftStatus[] arrayOfActorStatus = GameManager.i.loadScript.arrayOfActorDraftStatus;
        num = arrayOfActorStatus.Length;
        if (num > 0)
        {
            for (int i = 0; i < num; i++)
            {
                ActorDraftStatus assetSO = arrayOfActorStatus[i];
                switch (assetSO.name)
                {
                    case "HQBoss0": statusHqBoss0 = assetSO; break;
                    case "HQBoss1": statusHqBoss1 = assetSO; break;
                    case "HQBoss2": statusHqBoss2 = assetSO; break;
                    case "HQBoss3": statusHqBoss3 = assetSO; break;
                    case "HQWorker": statusHqWorker = assetSO; break;
                    case "OnMap": statusOnMap = assetSO; break;
                    case "Pool": statusPool = assetSO; break;
                    case "Reserves": statusReserves = assetSO; break;
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
            if (statusReserves == null) { Debug.LogError("Invalid statusReserves (Null)"); }
        }
        //
        // - - - Nameset
        //
        nameSet = GameManager.i.cityScript.GetNameSet();
        if (nameSet == null)
        { Debug.LogError("Invalid nameSet (Null)"); }
        //
        // - - - Collections
        //
        listOfActorArcs = GameManager.i.dataScript.GetActorArcs(GameManager.i.sideScript.PlayerSide);
        if (listOfActorArcs == null) { Debug.LogError("Invalid listOfActorArcs (Null)"); }
        //
        // - - - Fast Access
        //
        numOfActorsHQ = GameManager.i.hqScript.numOfActorsHQ;
        hqPowerFactor = GameManager.i.hqScript.powerFactor;
    }

    /// <summary>
    /// Master program
    /// </summary>
    public void CreateActorPool()
    {
        Initialise();
        CreateActorDraft(11);
    }

    /// <summary>
    /// Creates an ActorDraft.SO and places in folder
    /// </summary>
    private void CreateActorDraft(int numToCreate)
    {
        string path;
        for (int i = 0; i < numToCreate; i++)
        {
            //create SO object
            ActorDraft actorAsset = ScriptableObject.CreateInstance<ActorDraft>();
            //overwrite default data
            UpdateActorDraft(actorAsset, i);
            //path with unique asset name for each
            path = string.Format("Assets/SO/ActorDrafts/draft{0}.asset", i);
            //delete any existing asset at the same location (if same named asset presnet, new asset won't overwrite)
            AssetDatabase.DeleteAsset(path);
            //Add asset to file and give it a name ('actor')
            AssetDatabase.CreateAsset(actorAsset, path);
        }
        //Save assets to disk
        AssetDatabase.SaveAssets();       
    }


    /// <summary>
    /// Initialises data for a newly created ActorDraft
    /// </summary>
    /// <param name="actor"></param>
    private void UpdateActorDraft(ActorDraft actor, int num)
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
            // - - - status, arc, level and power
            //
            switch (num)
            {
                case 0: actor.status = statusHqBoss0; actor.arc = listOfActorArcs[Random.Range(0, listOfActorArcs.Count)]; actor.power = (numOfActorsHQ + 2 - (int)ActorHQ.Boss) * hqPowerFactor; break;
                case 1: actor.status = statusHqBoss1; actor.arc = listOfActorArcs[Random.Range(0, listOfActorArcs.Count)]; actor.power = (numOfActorsHQ + 2 - (int)ActorHQ.SubBoss1) * hqPowerFactor; break;
                case 2: actor.status = statusHqBoss2; actor.arc = listOfActorArcs[Random.Range(0, listOfActorArcs.Count)]; actor.power = (numOfActorsHQ + 2 - (int)ActorHQ.SubBoss2) * hqPowerFactor; break;
                case 3: actor.status = statusHqBoss3; actor.arc = listOfActorArcs[Random.Range(0, listOfActorArcs.Count)]; actor.power = (numOfActorsHQ + 2 - (int)ActorHQ.SubBoss3) * hqPowerFactor; break;
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11: actor.status = statusHqWorker; actor.arc = listOfActorArcs[Random.Range(0, listOfActorArcs.Count)]; actor.power = Random.Range(1, 6); break;
                default: Debug.LogWarningFormat("Unrecognised num \"{0}\"", num); break;
            }

        }
        else { Debug.LogError("Invalid actorDraft (Null)"); }
    }
           
    //new methods above here 
}
