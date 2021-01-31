using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region SaveActorPool
/// <summary>
/// Save format for ActorPools
/// </summary>
[Serializable]
public class SaveActorPool
{
    public string poolName;

    /*public string tag;
    public string author;
    public string nameSet;
    public string side;
    public string dateCreated;*/

    public SaveActorDraft hqBoss0;
    public SaveActorDraft hqBoss1;
    public SaveActorDraft hqBoss2;
    public SaveActorDraft hqBoss3;

    public List<SaveActorDraft> listHqWorkers = new List<SaveActorDraft>();
    public List<SaveActorDraft> listOnMap = new List<SaveActorDraft>();
    public List<SaveActorDraft> listLevelOne = new List<SaveActorDraft>();
    public List<SaveActorDraft> listLevelTwo = new List<SaveActorDraft>();
    public List<SaveActorDraft> listLevelThree = new List<SaveActorDraft>();
}
#endregion

#region SaveActorDraft
/// <summary>
/// Save format (strings and ints only) for ActorDraft.SO's
/// </summary>
[Serializable]
public class SaveActorDraft
    {
    public string draft;

    public string actorName;
    public string firstName;
    public string lastName;

    public string spriteName;
    public string arcName;
    public string traitName;
    public int level;
    public int power;

    public string statusName;
    public string sexName;

    public string backstory0;
    public string backstory1;
    
    //NOTE: no need to save backstory prompts as these are generated once and saved automatically by Unity
}
#endregion

/// <summary>
/// handles all Actor Pool file save/load functionality. Doubles up as Save.cs and FileManager.cs equivalents
/// </summary>
public class ActorFileManager : MonoBehaviour
{

    #region WriteActorPool
    /// <summary>
    /// Converts actor pool, turns into JSON, saves to disc
    /// </summary>
    public void WriteActorPool(ActorPool pool)
    {
        if (pool != null)
        {
            SaveActorPool savePool = ConvertActorPool(pool);
            if (savePool != null)
            {

            }
            else { Debug.LogErrorFormat("Invalid savePool (Null) for ActorPool \"{0}\"", pool.name); }
        }
        else { Debug.LogError("Invalid actorPool (Null)"); }
    }
    #endregion

    #region ConvertActorPool
    /// <summary>
    /// Takes an ActorPool.So and converts to a SaveActorPool.cs file suitable for serialising to file
    /// </summary>
    /// <param name="pool"></param>
    /// <returns></returns>
    public SaveActorPool ConvertActorPool(ActorPool pool)
    {
        SaveActorPool poolSave = new SaveActorPool();
        if (pool != null)
        {
            poolSave.poolName = pool.name;
          
            poolSave.hqBoss0 = ConvertActorDraft(pool.hqBoss0);
            poolSave.hqBoss1 = ConvertActorDraft(pool.hqBoss1);
            poolSave.hqBoss2 = ConvertActorDraft(pool.hqBoss2);
            poolSave.hqBoss3 = ConvertActorDraft(pool.hqBoss3);
            //hq workers
            for (int i = 0; i < pool.listHqWorkers.Count; i++)
            {
                SaveActorDraft draft = ConvertActorDraft(pool.listHqWorkers[i]);
                if (draft != null)
                { poolSave.listHqWorkers.Add(draft); }
                else { Debug.LogErrorFormat("Invalid ActorDraft (Null) for listOfHqWorkers[{0}]", i); }
            }
            //onMap
            for (int i = 0; i < pool.listOnMap.Count; i++)
            {
                SaveActorDraft draft = ConvertActorDraft(pool.listOnMap[i]);
                if (draft != null)
                { poolSave.listOnMap.Add(draft); }
                else { Debug.LogErrorFormat("Invalid ActorDraft (Null) for listOfOnMap[{0}]", i); }
            }
            //level 1
            for (int i = 0; i < pool.listLevelOne.Count; i++)
            {
                SaveActorDraft draft = ConvertActorDraft(pool.listLevelOne[i]);
                if (draft != null)
                { poolSave.listLevelOne.Add(draft); }
                else { Debug.LogErrorFormat("Invalid ActorDraft (Null) for listOfLevelOne[{0}]", i); }
            }
            //level 2
            for (int i = 0; i < pool.listLevelTwo.Count; i++)
            {
                SaveActorDraft draft = ConvertActorDraft(pool.listLevelTwo[i]);
                if (draft != null)
                { poolSave.listLevelTwo.Add(draft); }
                else { Debug.LogErrorFormat("Invalid ActorDraft (Null) for listOfLevelTwo[{0}]", i); }
            }
            //level 3
            for (int i = 0; i < pool.listLevelThree.Count; i++)
            {
                SaveActorDraft draft = ConvertActorDraft(pool.listLevelThree[i]);
                if (draft != null)
                { poolSave.listLevelThree.Add(draft); }
                else { Debug.LogErrorFormat("Invalid ActorDraft (Null) for listOfLevelThree[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid ActorPool (Null)"); }
        return poolSave;
    }
    #endregion


    #region ConvertActorDraft
    /// <summary>
    /// Takes an ActorDraft.SO and converts to a SaveActorDraft.cs file suitable for serialising to file.
    /// </summary>
    /// <param name="actorDraft"></param>
    /// <returns></returns>
    public SaveActorDraft ConvertActorDraft(ActorDraft actorDraft)
    {
        SaveActorDraft saveDraft = new SaveActorDraft();
        if (actorDraft != null)
        {
            saveDraft.draft = actorDraft.name;
            saveDraft.actorName = actorDraft.actorName;
            saveDraft.firstName = actorDraft.firstName;
            saveDraft.lastName = actorDraft.lastName;
            saveDraft.spriteName = actorDraft.sprite.name;
            saveDraft.arcName = actorDraft.arc.name;
            saveDraft.traitName = actorDraft.trait.name;
            saveDraft.level = actorDraft.level;
            saveDraft.power = actorDraft.power;
            saveDraft.statusName = actorDraft.status.name;
            saveDraft.sexName = actorDraft.sex.name;
            saveDraft.backstory0 = actorDraft.backstory0;
            saveDraft.backstory1 = actorDraft.backstory1;
        }
        else { Debug.LogError("Invalid actorDraft (Null)"); }
        return saveDraft;
    }
    #endregion
}
