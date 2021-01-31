using System;
using System.Collections.Generic;
using System.IO;
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
    /*public string arcName;*/
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

    private SaveActorPool savePool;
    private SaveActorPool readPool;
    private string fileName;
    private string filePath;
    private string jsonWrite;
    private string jsonRead;



    #region WriteActorPool
    /// <summary>
    /// Converts actor pool, turns into JSON, saves to file
    /// </summary>
    public void WriteActorPool(ActorPool pool)
    {
        if (pool != null)
        {
            //convert SO to save file format
            savePool = ConvertFromActorPool(pool);
            if (savePool != null)
            {
                fileName = savePool.poolName + ".txt";
                filePath = Path.Combine(Application.persistentDataPath, fileName);
                    if (string.IsNullOrEmpty(filePath) == false)
                    {
                        //convert to Json (NOTE: second, optional, parameter gives pretty output for debugging purposes)
                        jsonWrite = JsonUtility.ToJson(savePool, true);

                        //file present? If so delete
                        if (File.Exists(filePath) == true)
                        {
                            try { File.Delete(filePath); }
                            catch (Exception e) { Debug.LogErrorFormat("Failed to DELETE FILE, error \"{0}\"", e.Message); }
                        }

                        //create new file
                        try { File.WriteAllText(filePath, jsonWrite); }
                        catch (Exception e) { Debug.LogErrorFormat("Failed to write TEXT FROM FILE, error \"{0}\"", e.Message); }
                        Debug.LogFormat("[Fil] FileManager.cs -> WriteActorPool: pool Data SAVED to \"{0}\"{1}", filePath, "\n");

                    }
                    else { Debug.LogError("Invalid fileName (Null or Empty)"); }
            }
            else { Debug.LogErrorFormat("Invalid savePool (Null) for ActorPool \"{0}\"", pool.name); }
        }
        else { Debug.LogError("Invalid actorPool (Null)"); }
    }
    #endregion

    #region ReadActorPool
    /// <summary>
    /// Read json file and update actorPool.SO
    /// </summary>
    /// <param name="pool"></param>
    public void ReadActorPool(ActorPool pool)
    {
        if (pool != null)
        {
            //get filepath
            fileName = pool.name + ".txt";
            filePath = Path.Combine(Application.persistentDataPath, fileName);
            bool isSuccess = false;
            if (string.IsNullOrEmpty(filePath) == false)
            {
                if (File.Exists(filePath) == true)
                {

                    //read data from File
                    try { jsonRead = File.ReadAllText(filePath); }
                    catch (Exception e) { Debug.LogErrorFormat("Failed to read TEXT FROM FILE, error \"{0}\"", e.Message); }
                    isSuccess = true;
                    if (isSuccess == true)
                    {
                        //read to Save file
                        try
                        {
                            readPool = JsonUtility.FromJson<SaveActorPool>(jsonRead);
                            Debug.LogFormat("[Fil] FileManager.cs -> ReadActorPool: POOL loaded from \"{0}\"{1}", filePath, "\n");
                            //update ActorPool.SO with loaded file data
                            ConvertToActorPool(readPool, pool);
                        }
                        catch (Exception e)
                        { Debug.LogErrorFormat("Failed to read Json, error \"{0}\"", e.Message); }
                    }
                }
                else
                {
                    //file doesn't exist, creat one
                    WriteActorPool(pool);
                }
            }
            else { Debug.LogError("Invalid filename (Null or Empty)"); }


        }
        else { Debug.LogError("Invalid actorPool (Null)"); }
    }
    #endregion

    #region DeletePoolFile
    /// <summary>
    /// Delete an actor pool json file (done when ActorPool.SO deleted)
    /// </summary>
    /// <param name="poolName"></param>
    public void DeletePoolFile(string poolName)
    {
        if (string.IsNullOrEmpty(poolName) == false)
        {
            fileName = poolName + ".txt";
            filePath = Path.Combine(Application.persistentDataPath, fileName);
            try
            { File.Delete(filePath); }
            catch (Exception e)
            { Debug.LogErrorFormat("Failed to delete file at \"{0}\", error \"{1}\"", filePath, e.Message); }
        }
        else { Debug.LogError("Invalid poolName (Null or Empty)"); }
    }
    #endregion

    #region Utilities...

    #region ConvertFromActorPool
    /// <summary>
    /// Takes an ActorPool.So and converts to a SaveActorPool.cs file suitable for serialising to file
    /// </summary>
    /// <param name="pool"></param>
    /// <returns></returns>
    private SaveActorPool ConvertFromActorPool(ActorPool pool)
    {
        SaveActorPool poolSave = new SaveActorPool();
        if (pool != null)
        {
            poolSave.poolName = pool.name;

            poolSave.hqBoss0 = ConvertFromActorDraft(pool.hqBoss0);
            poolSave.hqBoss1 = ConvertFromActorDraft(pool.hqBoss1);
            poolSave.hqBoss2 = ConvertFromActorDraft(pool.hqBoss2);
            poolSave.hqBoss3 = ConvertFromActorDraft(pool.hqBoss3);
            //hq workers
            for (int i = 0; i < pool.listHqWorkers.Count; i++)
            {
                SaveActorDraft draft = ConvertFromActorDraft(pool.listHqWorkers[i]);
                if (draft != null)
                { poolSave.listHqWorkers.Add(draft); }
                else { Debug.LogErrorFormat("Invalid ActorDraft (Null) for listOfHqWorkers[{0}]", i); }
            }
            //onMap
            for (int i = 0; i < pool.listOnMap.Count; i++)
            {
                SaveActorDraft draft = ConvertFromActorDraft(pool.listOnMap[i]);
                if (draft != null)
                { poolSave.listOnMap.Add(draft); }
                else { Debug.LogErrorFormat("Invalid ActorDraft (Null) for listOfOnMap[{0}]", i); }
            }
            //level 1
            for (int i = 0; i < pool.listLevelOne.Count; i++)
            {
                SaveActorDraft draft = ConvertFromActorDraft(pool.listLevelOne[i]);
                if (draft != null)
                { poolSave.listLevelOne.Add(draft); }
                else { Debug.LogErrorFormat("Invalid ActorDraft (Null) for listOfLevelOne[{0}]", i); }
            }
            //level 2
            for (int i = 0; i < pool.listLevelTwo.Count; i++)
            {
                SaveActorDraft draft = ConvertFromActorDraft(pool.listLevelTwo[i]);
                if (draft != null)
                { poolSave.listLevelTwo.Add(draft); }
                else { Debug.LogErrorFormat("Invalid ActorDraft (Null) for listOfLevelTwo[{0}]", i); }
            }
            //level 3
            for (int i = 0; i < pool.listLevelThree.Count; i++)
            {
                SaveActorDraft draft = ConvertFromActorDraft(pool.listLevelThree[i]);
                if (draft != null)
                { poolSave.listLevelThree.Add(draft); }
                else { Debug.LogErrorFormat("Invalid ActorDraft (Null) for listOfLevelThree[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid ActorPool (Null)"); }
        return poolSave;
    }
    #endregion

    #region ConvertFromActorDraft
    /// <summary>
    /// Takes an ActorDraft.SO and converts to a SaveActorDraft.cs file suitable for serialising to file.
    /// </summary>
    /// <param name="actorDraft"></param>
    /// <returns></returns>
    private SaveActorDraft ConvertFromActorDraft(ActorDraft actorDraft)
    {
        SaveActorDraft saveDraft = new SaveActorDraft();
        if (actorDraft != null)
        {
            saveDraft.draft = actorDraft.name;
            saveDraft.actorName = actorDraft.actorName;
            saveDraft.firstName = actorDraft.firstName;
            saveDraft.lastName = actorDraft.lastName;
            saveDraft.spriteName = actorDraft.sprite.name;
            /*saveDraft.arcName = actorDraft.arc.name;*/
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

    #region ConvertToActorPool
    /// <summary>
    /// Takes a SaveActorPool.cs object and converts to an ActorPool.SO
    /// </summary>
    /// <param name="pool"></param>
    private void ConvertToActorPool(SaveActorPool savePool, ActorPool pool )
    {
        //hq hierarchy
        ConvertToActorDraft(savePool.hqBoss0, pool.hqBoss0);
        ConvertToActorDraft(savePool.hqBoss1, pool.hqBoss1);
        ConvertToActorDraft(savePool.hqBoss2, pool.hqBoss2);
        ConvertToActorDraft(savePool.hqBoss3, pool.hqBoss3);
        //hq workers
        if (savePool.listHqWorkers.Count == pool.listHqWorkers.Count)
        {
            for (int i = 0; i < savePool.listHqWorkers.Count; i++)
            { ConvertToActorDraft(savePool.listHqWorkers[i], pool.listHqWorkers[i]); }
        }
        else { Debug.LogWarningFormat("Mismatch on count -> savePool.listHqWorkers has {0} records, ActorPool.listHqWorkers has {1} records", savePool.listHqWorkers.Count, pool.listHqWorkers.Count); }
        //OnMap
        if (savePool.listOnMap.Count == pool.listOnMap.Count)
        {
            for (int i = 0; i < savePool.listOnMap.Count; i++)
            { ConvertToActorDraft(savePool.listOnMap[i], pool.listOnMap[i]); }
        }
        else { Debug.LogWarningFormat("Mismatch on count -> savePool.listOnMap has {0} records, ActorPool.listOnMap has {1} records", savePool.listOnMap.Count, pool.listOnMap.Count); }
        //Level One
        if (savePool.listLevelOne.Count == pool.listLevelOne.Count)
        {
            for (int i = 0; i < savePool.listLevelOne.Count; i++)
            { ConvertToActorDraft(savePool.listLevelOne[i], pool.listLevelOne[i]); }
        }
        else { Debug.LogWarningFormat("Mismatch on count -> savePool.listLevelOne has {0} records, ActorPool.listLevelOne has {1} records", savePool.listLevelOne.Count, pool.listLevelOne.Count); }
        //Level Two
        if (savePool.listLevelTwo.Count == pool.listLevelTwo.Count)
        {
            for (int i = 0; i < savePool.listLevelTwo.Count; i++)
            { ConvertToActorDraft(savePool.listLevelTwo[i], pool.listLevelTwo[i]); }
        }
        else { Debug.LogWarningFormat("Mismatch on count -> savePool.listLevelTwo has {0} records, ActorPool.listLevelTwo has {1} records", savePool.listLevelTwo.Count, pool.listLevelTwo.Count); }
        //Level Three
        if (savePool.listLevelThree.Count == pool.listLevelThree.Count)
        {
            for (int i = 0; i < savePool.listLevelThree.Count; i++)
            { ConvertToActorDraft(savePool.listLevelThree[i], pool.listLevelThree[i]); }
        }
        else { Debug.LogWarningFormat("Mismatch on count -> savePool.listLevelThree has {0} records, ActorPool.listLevelThree has {1} records", savePool.listLevelThree.Count, pool.listLevelThree.Count); }
    }
    #endregion

    #region ConvertToActorDraft
    /// <summary>
    /// Takes a SaveActorDraft.cs object and converts it to an ActorDraft.SO
    /// </summary>
    private void ConvertToActorDraft(SaveActorDraft saveDraft, ActorDraft draft)
    {
        if (saveDraft != null)
        {
            if (draft != null)
            {
                draft.actorName = saveDraft.actorName;
                draft.firstName = saveDraft.firstName;
                draft.lastName = saveDraft.lastName;

                //Sprite -> TO DO

                draft.level = saveDraft.level;
                draft.power = saveDraft.power;
                draft.backstory0 = saveDraft.backstory0;
                draft.backstory1 = saveDraft.backstory1;
                //trait
                Trait trait = ToolManager.i.toolDataScript.GetTrait(saveDraft.traitName);
                if (trait != null)
                { draft.trait = trait; }
                else { Debug.LogWarningFormat("Invalid trait (Null) for saveDraft.traitName \"{0}\"", saveDraft.traitName); }
            }
            else { Debug.LogError("Invalid ActorDraft.SO (Null)"); }
        }
        else { Debug.LogError("Invalid saveDraft (Null)"); }
    }
    #endregion

    #endregion

    //new methods above here
}
