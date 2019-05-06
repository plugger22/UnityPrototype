﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using gameAPI;

/// <summary>
/// Handles all Save / Load functionality
/// </summary>
public class FileManager : MonoBehaviour
{

    private static readonly string SAVE_FILE = "savefile.json";
    private Save write;
    private Save read;
    private string filename;
    private string jsonWrite;
    private string jsonRead;
    private byte[] soupWrite;               //encryption out
    private byte[] soupRead;                //encryption in

    private string cipherKey;

    //fast access
    GlobalSide globalAuthority;
    GlobalSide globalResistance;
    float alphaActive = -1;
    float alphaInactive = -1;

    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        filename = Path.Combine(Application.persistentDataPath, SAVE_FILE);
        cipherKey = "#kJ83DAl50$*@.<__'][90{4#dDA'a?~";                         //needs to be 32 characters long exactly
        //fast access
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        alphaActive = GameManager.instance.guiScript.alphaActive;
        alphaInactive = GameManager.instance.guiScript.alphaInactive;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(alphaActive > -1, "Invalid alphaActive (-1)");
        Debug.Assert(alphaInactive > -1, "Invalid alphaInactive (-1)");
    }

    //
    // - - - Master Methods - - -
    //

    /// <summary>
    /// copy inGame data to saveData
    /// </summary>
    public void WriteGameData()
    {
        write = new Save();
        //Sequentially write data
        WriteDataData();
        WritePlayerData();
        WriteSideData();
        WriteActorData();
    }

    /// <summary>
    /// Save game method
    /// </summary>
    public void SaveGame()
    {
        if (write != null)
        {
            //convert to Json (NOTE: second, optional, parameter gives pretty output for debugging purposes)
            jsonWrite = JsonUtility.ToJson(write, true);

            //file present? If so delete
            if (File.Exists(filename) == true)
            {
                try { File.Delete(filename); }
                catch (Exception e) { Debug.LogErrorFormat("Failed to DELETE FILE, error \"{0}\"", e.Message); }
            }

            if (GameManager.instance.isEncrypted == false)
            {
                //create new file
                try { File.WriteAllText(filename, jsonWrite); }
                catch (Exception e) { Debug.LogErrorFormat("Failed to write TEXT FROM FILE, error \"{0}\"", e.Message); }
                Debug.LogFormat("[Fil] FileManager.cs -> SaveGame: GAME SAVED to \"{0}\"{1}", filename, "\n");
            }
            else
            {
                //encrypt save file
                Rijndael crypto = new Rijndael();
                soupWrite = crypto.Encrypt(jsonWrite, cipherKey);
                try { File.WriteAllBytes(filename, soupWrite); }
                catch (Exception e) { Debug.LogErrorFormat("Failed to write BYTES TO FILE, error \"{0}\"", e.Message); }
                Debug.LogFormat("[Fil] FileManager.cs -> SaveGame: Encrypted GAME SAVED to \"{0}\"{1}", filename, "\n");
            }
        }
        else { Debug.LogError("Invalid saveData (Null)"); }
    }

    /// <summary>
    /// Read game method, returns true if successful, false otherise
    /// </summary>
    public bool ReadGameData()
    {
        bool isSuccess = false;
        if (File.Exists(filename) == true)
        {
            if (GameManager.instance.isEncrypted == false)
            {
                //read data from File
                try { jsonRead = File.ReadAllText(filename); }
                catch (Exception e) { Debug.LogErrorFormat("Failed to read TEXT FROM FILE, error \"{0}\"", e.Message); }
                isSuccess = true;
            }
            else
            {
                //encrypted file
                Rijndael crypto = new Rijndael();
                try { soupRead = File.ReadAllBytes(filename); }
                catch (Exception e) { Debug.LogErrorFormat("Failed to read BYTES FROM FILE, error \"{0}\"", e.Message); }
                jsonRead = crypto.Decrypt(soupRead, cipherKey);
                isSuccess = true;
            }
            if (isSuccess == true)
            {
                //read to Save file
                try
                {
                    read = JsonUtility.FromJson<Save>(jsonRead);
                    return true;
                }
                catch (Exception e)
                { Debug.LogErrorFormat("Failed to read Json, error \"{0}\"", e.Message); }
            }
        }
        else { Debug.LogErrorFormat("File \"{0}\" not found", filename); }
        return false;
    }

    /// <summary>
    /// Load data from Save file back into game
    /// </summary>
    public void LoadSaveData()
    {
        if (read != null)
        {
            //Sequentially read data back into game

            //side (player) at start
            ReadSideData();
            ReadDataData();

            //secrets before player
            ReadActorData();
            ValidateActorData();
            ReadPlayerData();
            ValidatePlayerData();
            Debug.LogFormat("[Fil] FileManager.cs -> LoadSaveData: Saved Game Data has been LOADED{0}", "\n");
        }
    }

    //
    // - - - Write - - -
    //

    /// <summary>
    /// PlayerManager.cs Data write to file
    /// </summary>
    private void WritePlayerData()
    {
        write.playerData.renown = GameManager.instance.playerScript.Renown;
        write.playerData.status = GameManager.instance.playerScript.status;
        write.playerData.Invisibility = GameManager.instance.playerScript.Invisibility;
        write.playerData.tooltipStatus = GameManager.instance.playerScript.tooltipStatus;
        write.playerData.inactiveStatus = GameManager.instance.playerScript.inactiveStatus;
        write.playerData.listOfGear = GameManager.instance.playerScript.GetListOfGear();
        write.playerData.isBreakdown = GameManager.instance.playerScript.isBreakdown;
        write.playerData.isEndOfTurnGearCheck = GameManager.instance.playerScript.isEndOfTurnGearCheck;
        write.playerData.isLieLowFirstturn = GameManager.instance.playerScript.isLieLowFirstturn;
        write.playerData.isStressLeave = GameManager.instance.playerScript.isStressLeave;
        //secrets
        List<Secret> listOfSecrets = GameManager.instance.playerScript.GetListOfSecrets();
        foreach (Secret secret in listOfSecrets)
        {
            if (secret != null)
            { write.playerData.listOfSecrets.Add(secret.secretID); }
            else { Debug.LogWarning("Invalid secret (Null)"); }
        }
        //conditions
        List<Condition> listOfConditions = GameManager.instance.playerScript.GetListOfConditions(globalResistance);
        foreach (Condition condition in listOfConditions)
        {
            if (condition != null)
            { write.playerData.listOfConditionsResistance.Add(condition.tag); }
            else { Debug.LogWarning("Invalid Resistance condition (Null)"); }
        }
        listOfConditions = GameManager.instance.playerScript.GetListOfConditions(globalAuthority);
        foreach (Condition condition in listOfConditions)
        {
            if (condition != null)
            { write.playerData.listOfConditionsAuthority.Add(condition.tag); }
            else { Debug.LogWarning("Invalid Authority condition (Null)"); }
        }
    }

    /// <summary>
    /// SideManager.cs data write to file
    /// </summary>
    private void WriteSideData()
    {
        write.sideData.resistanceCurrent = GameManager.instance.sideScript.resistanceCurrent;
        write.sideData.authorityCurrent = GameManager.instance.sideScript.authorityCurrent;
        write.sideData.resistanceOverall = GameManager.instance.sideScript.resistanceOverall;
        write.sideData.authorityOverall = GameManager.instance.sideScript.authorityOverall;
        write.sideData.playerSide = GameManager.instance.sideScript.PlayerSide;
    }

    /// <summary>
    /// DataManager.cs data write to file
    /// </summary>
    private void WriteDataData()
    {
        //
        // - - - Secrets
        //
        //individual secret SO dynamic data for Secrets in dictOfSecrets
        Dictionary<int, Secret> dictOfSecrets = GameManager.instance.dataScript.GetDictOfSecrets();
        if (dictOfSecrets != null)
        {
            foreach (var secret in dictOfSecrets)
            {
                if (secret.Value != null)
                {
                    //create a new SaveSecret & pass across dynamic data
                    SaveSecret saveSecret = new SaveSecret();
                    saveSecret.secretID = secret.Key;
                    saveSecret.gainedWhen = secret.Value.gainedWhen;
                    saveSecret.revealedWho = secret.Value.revealedWho;
                    saveSecret.revealedWhen = secret.Value.revealedWhen;
                    saveSecret.deleteWhen = secret.Value.deletedWhen;
                    saveSecret.status = secret.Value.status;
                    saveSecret.listOfActors.AddRange(secret.Value.GetListOfActors());
                    //add to listOfSecretsChanges
                    write.dataData.listOfSecretChanges.Add(saveSecret);
                }
                else { Debug.LogError("Invalid secret (Null) in dictOfSecrets"); }
            }
        }
        else { Debug.LogError("Invalid dictOfSecrets (Null)"); }
        //secret list -> PlayerSecrets
        List<Secret> listOfSecrets = GameManager.instance.dataScript.GetListOfPlayerSecrets();
        if (listOfSecrets != null)
        {
            for (int i = 0; i < listOfSecrets.Count; i++)
            { write.dataData.listOfPlayerSecrets.Add(listOfSecrets[i].secretID); }
        }
        else { Debug.LogError("Invalid listOfPlayerSecrets (Null)"); }
        //secret list -> RevealSecrets
        listOfSecrets = GameManager.instance.dataScript.GetListOfRevealedSecrets();
        if (listOfSecrets != null)
        {
            for (int i = 0; i < listOfSecrets.Count; i++)
            { write.dataData.listOfRevealedSecrets.Add(listOfSecrets[i].secretID); }
        }
        else { Debug.LogError("Invalid listOfRevealedSecrets (Null)"); }
        //secret list -> DeletedSecrets
        listOfSecrets = GameManager.instance.dataScript.GetListOfDeletedSecrets();
        if (listOfSecrets != null)
        {
            for (int i = 0; i < listOfSecrets.Count; i++)
            { write.dataData.listOfDeletedSecrets.Add(listOfSecrets[i].secretID); }
        }
        else { Debug.LogError("Invalid listOfDeletedSecrets (Null)"); }
        //
        // - - - Contacts
        //
        //main contact dictionary (do first)
        Dictionary<int, Contact> dictOfContacts = GameManager.instance.dataScript.GetDictOfContacts();
        if (dictOfContacts != null)
        {
            foreach(var contact in dictOfContacts)
            {
                if (contact.Value != null)
                { write.dataData.listOfContacts.Add(contact.Value); }
                else { Debug.LogWarningFormat("Invalid contact (Null) in dictOfContacts for contactID {0}", contact.Key); }
            }
        }
        //contact pool
        List<int> listOfContactPool = GameManager.instance.dataScript.GetContactPool();
        if (listOfContactPool != null)
        { write.dataData.listOfContactPool.AddRange(listOfContactPool); }
        else { Debug.LogError("Invalid listOfContactPool (Null)"); }
        //contact dict's of lists -> dictOfActorContacts
        Dictionary<int, List<int>> dictOfActorContacts = GameManager.instance.dataScript.GetDictOfActorContacts();
        if (dictOfActorContacts != null)
        {
            foreach(var contactList in dictOfActorContacts)
            {
                IntListWrapper listOfContacts = new IntListWrapper(contactList.Value);
                if (listOfContacts != null)
                {
                    write.dataData.listOfActorContactsValue.Add(listOfContacts);
                    write.dataData.contactLists.listOfActorContactsKey.Add(contactList.Key);
                }
                else { Debug.LogWarningFormat("Invalid listOfContacts (Null) for actorID {0}", contactList.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfActorContacts (Null)"); }
    }


    /// <summary>
    /// Actor.cs (dict and lists) full data set write to file
    /// </summary>
    private void WriteActorData()
    {
        //
        // - - - Main dictionary
        //
        Dictionary<int, Actor> dictOfActors = GameManager.instance.dataScript.GetDictOfActors();
        if (dictOfActors != null)
        {
            //loop dictOfActors
            foreach (var actor in dictOfActors)
            {
                if (actor.Value != null)
                {
                    SaveActor saveActor = WriteIndividualActor(actor.Value);
                    if (saveActor != null)
                    { write.actorData.listOfDictActors.Add(saveActor); }
                }
                else { Debug.LogWarning("Invalid actor (Null) in dictOfActors"); }
            }
        }
        else { Debug.LogError("Invalid dictOfActors (Null)"); }
        //
        // - - - Actor arrays
        int sideNum = GameManager.instance.dataScript.GetNumOfGlobalSide();
        int actorNum = GameManager.instance.actorScript.maxNumOfOnMapActors;
        Debug.Assert(sideNum > 0, "Invalid sideNum (Zero or less)");
        Debug.Assert(actorNum > 0, "Invalid actorNum (Zero or less)");
        //write data to arrays (not multidimensional arrays split into single arrays for serialization)
        Actor[,] arrayOfActors = GameManager.instance.dataScript.GetArrayOfActors();
        Debug.Assert(arrayOfActors.GetUpperBound(0) + 1 == sideNum, "Invalid arrayOfActors dimension [x, ]");
        Debug.Assert(arrayOfActors.GetUpperBound(1) + 1 == actorNum, "Invalid arrayOfActors dimension [ ,x]");
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < arrayOfActors.GetUpperBound(1) + 1; j++)
                {
                    Actor actor = arrayOfActors[i, j];
                    //array has lots of null actors for sides other than Authority and Resistance
                    if (actor != null)
                    { write.actorData.listOfActors.Add(arrayOfActors[i, j].actorID); }
                    else { write.actorData.listOfActors.Add(-1); }
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        bool[,] arrayOfActorsPresent = GameManager.instance.dataScript.GetArrayOfActorsPresent();
        if (arrayOfActorsPresent != null)
        {
            for (int i = 0; i < arrayOfActorsPresent.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < arrayOfActorsPresent.GetUpperBound(1) + 1; j++)
                { write.actorData.listOfActorsPresent.Add(arrayOfActorsPresent[i, j]); }
            }
        }
        else { Debug.LogError("Invalid arrayOfActorsPresent (Null)"); }
        //
        // - - - Pool lists
        //
        write.actorData.authorityActorPoolLevelOne.AddRange(GameManager.instance.dataScript.GetActorRecruitPool(1, globalAuthority));
        write.actorData.authorityActorPoolLevelTwo.AddRange(GameManager.instance.dataScript.GetActorRecruitPool(2, globalAuthority));
        write.actorData.authorityActorPoolLevelThree.AddRange(GameManager.instance.dataScript.GetActorRecruitPool(3, globalAuthority));
        write.actorData.resistanceActorPoolLevelOne.AddRange(GameManager.instance.dataScript.GetActorRecruitPool(1, globalResistance));
        write.actorData.resistanceActorPoolLevelTwo.AddRange(GameManager.instance.dataScript.GetActorRecruitPool(2, globalResistance));
        write.actorData.resistanceActorPoolLevelThree.AddRange(GameManager.instance.dataScript.GetActorRecruitPool(3, globalResistance));
       
        write.actorData.authorityActorReserve.AddRange(GameManager.instance.dataScript.GetListOfReserveActors(globalAuthority));
        write.actorData.authorityActorDismissed.AddRange(GameManager.instance.dataScript.GetListOfDismissedActors(globalAuthority));
        write.actorData.authorityActorPromoted.AddRange(GameManager.instance.dataScript.GetListOfPromotedActors(globalAuthority));
        write.actorData.authorityActorDisposedOf.AddRange(GameManager.instance.dataScript.GetListOfDisposedOfActors(globalAuthority));
        write.actorData.authorityActorResigned.AddRange(GameManager.instance.dataScript.GetListOfResignedActors(globalAuthority));
        
        write.actorData.resistanceActorReserve.AddRange(GameManager.instance.dataScript.GetListOfReserveActors(globalResistance));
        write.actorData.resistanceActorDismissed.AddRange(GameManager.instance.dataScript.GetListOfDismissedActors(globalResistance));
        write.actorData.resistanceActorPromoted.AddRange(GameManager.instance.dataScript.GetListOfPromotedActors(globalResistance));
        write.actorData.resistanceActorDisposedOf.AddRange(GameManager.instance.dataScript.GetListOfDisposedOfActors(globalResistance));
        write.actorData.resistanceActorResigned.AddRange(GameManager.instance.dataScript.GetListOfResignedActors(globalResistance));

        //
        // - - - Actor.cs fast access fields
        //
        write.actorData.actorStressNone = GameManager.instance.dataScript.GetTraitEffectID("ActorStressNone");
        write.actorData.actorCorruptNone = GameManager.instance.dataScript.GetTraitEffectID("ActorCorruptNone");
        write.actorData.actorUnhappyNone = GameManager.instance.dataScript.GetTraitEffectID("ActorUnhappyNone");
        write.actorData.actorBlackmailNone = GameManager.instance.dataScript.GetTraitEffectID("ActorBlackmailNone");
        write.actorData.actorBlackmailTimerHigh = GameManager.instance.dataScript.GetTraitEffectID("ActorBlackmailTimerHigh");
        write.actorData.actorBlackmailTimerLow = GameManager.instance.dataScript.GetTraitEffectID("ActorBlackmailTimerLow");
        write.actorData.maxNumOfSecrets = GameManager.instance.secretScript.secretMaxNum;
        Debug.Assert(write.actorData.actorStressNone > -1, "Invalid actorStressNone (-1)");
        Debug.Assert(write.actorData.actorStressNone > -1, "Invalid actorCorruptNone (-1)");
        Debug.Assert(write.actorData.actorUnhappyNone > -1, "Invalid actorUnhappyNone (-1)");
        Debug.Assert(write.actorData.actorBlackmailNone > -1, "Invalid actorBlackmailNone (-1)");
        Debug.Assert(write.actorData.actorBlackmailTimerHigh > -1, "Invalid actorBlackmailTimerHigh (-1)");
        Debug.Assert(write.actorData.actorBlackmailTimerLow > -1, "Invalid actorBlackmailTimerLow (-1)");
        Debug.Assert(write.actorData.maxNumOfSecrets > -1, "Invalid maxNumOfSecrets (-1)");
    }

    /// <summary>
    /// subMethod for WriteActorData to handle serializing an individual actor. Returns null if a problem
    /// Note: actor checked for null by parent method
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    private SaveActor WriteIndividualActor(Actor actor)
    {
        bool isSuccess = true;
        SaveActor saveActor = new SaveActor();
        //
        // - - - base data
        //
        saveActor.status = actor.Status;
        saveActor.actorID = actor.actorID;
        saveActor.datapoint0 = actor.datapoint0;
        saveActor.datapoint1 = actor.datapoint1;
        saveActor.datapoint2 = actor.datapoint2;
        saveActor.side = actor.side;
        saveActor.slotID = actor.slotID;
        saveActor.level = actor.level;
        saveActor.nodeCaptured = actor.nodeCaptured;
        saveActor.gearID = actor.GetGearID();
        saveActor.isMale = actor.isMale;
        saveActor.actorName = actor.actorName;
        saveActor.firstName = actor.firstName;
        saveActor.arcID = actor.arc.ActorArcID;
        saveActor.trait = actor.GetTrait();

        //data which can be ignored (default values O.K) if actor is in the recruit pool
        if (actor.Status != ActorStatus.RecruitPool)
        {
            saveActor.Renown = actor.Renown;
            saveActor.unhappyTimer = actor.unhappyTimer;
            saveActor.blackmailTimer = actor.blackmailTimer;
            saveActor.captureTimer = actor.captureTimer;
            saveActor.numOfTimesBullied = actor.numOfTimesBullied;
            saveActor.numOfTimesCaptured = actor.numOfTimesCaptured;
            saveActor.departedNumOfSecrets = actor.departedNumOfSecrets;
            saveActor.isPromised = actor.isPromised;
            saveActor.isNewRecruit = actor.isNewRecruit;
            saveActor.isReassured = actor.isReassured;
            saveActor.isThreatening = actor.isThreatening;
            saveActor.isComplaining = actor.isComplaining;
            saveActor.isBreakdown = actor.isBreakdown;
            saveActor.isLieLowFirstturn = actor.isLieLowFirstturn;
            saveActor.isStressLeave = actor.isStressLeave;
            saveActor.isTraitor = actor.isTraitor;
            saveActor.tooltipStatus = actor.tooltipStatus;
            saveActor.inactiveStatus = actor.inactiveStatus;
            saveActor.gearTimer = actor.GetGearTimer();
            saveActor.gearTimesTaken = actor.GetGearTimesTaken();

            //
            // - - - Collections
            //

            //teams
            saveActor.listOfTeams.AddRange(actor.GetListOfTeams());
            //secrets
            List<Secret> listOfSecrets = actor.GetListOfSecrets();
            if (listOfSecrets != null)
            {
                for (int i = 0; i < listOfSecrets.Count; i++)
                {
                    Secret secret = listOfSecrets[i];
                    if (secret != null)
                    { saveActor.listOfSecrets.Add(secret.secretID); }
                    else { Debug.LogWarningFormat("Invalid secret in listOfSecrets[{0}]", i); }
                }
            }
            else { Debug.LogError("Invalid listOfSecrets (Null)"); }
            //conditions
            List<Condition> listOfConditions = actor.GetListOfConditions();
            if (listOfConditions != null)
            {
                for (int i = 0; i < listOfConditions.Count; i++)
                {
                    Condition condition = listOfConditions[i];
                    if (condition != null)
                    { saveActor.listOfConditions.Add(condition.tag); }
                    else { Debug.LogWarningFormat("Invalid listOfConditons[{0}]", i); }
                }
            }
            else { Debug.LogError("Invalid listOfCondtions (Null)"); }
            //contacts
            Dictionary<int, Contact> dictOfContacts = actor.GetDictOfContacts();
            if (dictOfContacts != null)
            {
                foreach (var record in dictOfContacts)
                {
                    if (record.Value != null)
                    {
                        saveActor.listOfContactNodes.Add(record.Key);
                        saveActor.listOfContacts.Add(record.Value.contactID);
                    }
                    else { Debug.LogWarning("Invalid contact (Null) in dictOfContacts"); }
                }
            }
            else { Debug.LogError("Invalid dictOfContacts (Null)"); }
        }
        //
        // - - - Success check
        //
        if (isSuccess == false)
        {
            saveActor = null;
            Debug.LogWarningFormat("Failed to serialize {0}, {1}, actorID {2}", actor.actorName, actor.arc.name, actor.actorID);
        }
        return saveActor;
    }

    //
    // - - - Read - - -
    //

    /// <summary>
    /// PlayerManager.cs Data
    /// </summary>
    private void ReadPlayerData()
    {
        GameManager.instance.playerScript.Renown = read.playerData.renown;
        GameManager.instance.playerScript.Invisibility = read.playerData.Invisibility;
        GameManager.instance.playerScript.status = read.playerData.status;
        GameManager.instance.playerScript.tooltipStatus = read.playerData.tooltipStatus;
        GameManager.instance.playerScript.inactiveStatus = read.playerData.inactiveStatus;
        GameManager.instance.playerScript.isBreakdown = read.playerData.isBreakdown;
        GameManager.instance.playerScript.isEndOfTurnGearCheck = read.playerData.isEndOfTurnGearCheck;
        GameManager.instance.playerScript.isLieLowFirstturn = read.playerData.isLieLowFirstturn;
        GameManager.instance.playerScript.isStressLeave = read.playerData.isStressLeave;
        //gear
        List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
        listOfGear.Clear();
        listOfGear.AddRange(read.playerData.listOfGear);
        //secrets
        List<Secret> listOfSecrets = new List<Secret>();
        for (int i = 0; i < read.playerData.listOfSecrets.Count; i++)
        {
            Secret secret = GameManager.instance.dataScript.GetSecret(read.playerData.listOfSecrets[i]);
            if (secret != null)
            { listOfSecrets.Add(secret); }
        }
        GameManager.instance.playerScript.SetSecrets(listOfSecrets);
        //conditions -> Resistance
        List<Condition> listOfConditions = new List<Condition>();
        for (int i = 0; i < read.playerData.listOfConditionsResistance.Count; i++)
        {
            Condition condition = GameManager.instance.dataScript.GetCondition(read.playerData.listOfConditionsResistance[i]);
            if (condition != null)
            { listOfConditions.Add(condition); }
        }
        GameManager.instance.playerScript.SetConditions(listOfConditions, globalResistance);
        //conditions -> Authority
        listOfConditions = new List<Condition>();
        for (int i = 0; i < read.playerData.listOfConditionsAuthority.Count; i++)
        {
            Condition condition = GameManager.instance.dataScript.GetCondition(read.playerData.listOfConditionsAuthority[i]);
            if (condition != null)
            { listOfConditions.Add(condition); }
        }
        GameManager.instance.playerScript.SetConditions(listOfConditions, globalAuthority);
    }

    /// <summary>
    /// SideManager.cs data
    /// </summary>
    private void ReadSideData()
    {
        GameManager.instance.sideScript.resistanceCurrent = read.sideData.resistanceCurrent;
        GameManager.instance.sideScript.authorityCurrent = read.sideData.authorityCurrent;
        GameManager.instance.sideScript.resistanceOverall = read.sideData.resistanceOverall;
        GameManager.instance.sideScript.authorityOverall = read.sideData.authorityOverall;
        GameManager.instance.sideScript.PlayerSide = read.sideData.playerSide;
    }

    /// <summary>
    /// DataManager.cs data
    /// </summary>
    private void ReadDataData()
    {
        //
        // - - - Secrets
        //
        //Copy any dynamic data into dictOfSecrets
        Dictionary<int, Secret> dictOfSecrets = GameManager.instance.dataScript.GetDictOfSecrets();
        if (dictOfSecrets != null)
        {
            //loop saved list of secret changes
            for (int i = 0; i < read.dataData.listOfSecretChanges.Count; i++)
            {
                SaveSecret secretData = read.dataData.listOfSecretChanges[i];
                if (secretData != null)
                {
                    //find record in dict
                    if (dictOfSecrets.ContainsKey(secretData.secretID) == true)
                    {
                        Secret secret = dictOfSecrets[secretData.secretID];
                        if (secret != null)
                        {
                            //copy data across
                            secret.status = secretData.status;
                            secret.gainedWhen = secretData.gainedWhen;
                            secret.revealedWho = secretData.revealedWho;
                            secret.revealedWhen = secretData.revealedWhen;
                            secret.deletedWhen = secretData.deleteWhen;
                            secret.SetListOfActors(secretData.listOfActors);
                        }
                        else { Debug.LogErrorFormat("Invalid secret (Null) for secretID {0}", secretData.secretID); }
                    }
                    else { Debug.LogWarningFormat("Secret ID {0} not found in dictionary, record not updated for dynamic data", secretData.secretID); }
                }
                else { Debug.LogErrorFormat("Invalid SaveSecret (Null) for listOfSecretChanges[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfSecrets (Null)"); }

        //Secrets List -> PlayerSecrets
        List<Secret> listOfSecrets = new List<Secret>();
        for (int i = 0; i < read.dataData.listOfPlayerSecrets.Count; i++)
        {
            Secret secret = GameManager.instance.dataScript.GetSecret(read.dataData.listOfPlayerSecrets[i]);
            if (secret != null)
            { listOfSecrets.Add(secret); }
        }
        GameManager.instance.dataScript.SetListOfPlayerSecrets(listOfSecrets);
        //Secrets List -> RevealedSecrets
        listOfSecrets = new List<Secret>();
        for (int i = 0; i < read.dataData.listOfRevealedSecrets.Count; i++)
        {
            Secret secret = GameManager.instance.dataScript.GetSecret(read.dataData.listOfRevealedSecrets[i]);
            if (secret != null)
            { listOfSecrets.Add(secret); }
        }
        GameManager.instance.dataScript.SetListOfRevealedSecrets(listOfSecrets);
        //Secrets List -> DeletedSecrets
        listOfSecrets = new List<Secret>();
        for (int i = 0; i < read.dataData.listOfDeletedSecrets.Count; i++)
        {
            Secret secret = GameManager.instance.dataScript.GetSecret(read.dataData.listOfDeletedSecrets[i]);
            if (secret != null)
            { listOfSecrets.Add(secret); }
        }
        GameManager.instance.dataScript.SetListOfDeletedSecrets(listOfSecrets);
        //
        // - - - Contacts
        //
        //main contact dictionary (do first)
        Dictionary<int, Contact> dictOfContacts = GameManager.instance.dataScript.GetDictOfContacts();
        if (dictOfContacts != null)
        {
            //clear dictionary
            dictOfContacts.Clear();
            //copy saved data across
            for (int i = 0; i < read.dataData.listOfContacts.Count; i++)
            {
                Contact contact = read.dataData.listOfContacts[i];
                if (contact != null)
                {
                    try
                    { dictOfContacts.Add(contact.contactID, contact); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Duplicate contactID {0} in dictOfContacts", contact.contactID); }
                }
                else { Debug.LogWarning("Invalid contact (Null) in read.dataData.listOfContacts"); }
            }
        }
        else { Debug.LogError("Invalid dictOfContacts (Null)"); }
        //contact Pool
        List<int> listOfContactPool = GameManager.instance.dataScript.GetContactPool();
        if (listOfContactPool != null)
        {
            listOfContactPool.Clear();
            listOfContactPool.AddRange(read.dataData.listOfContactPool);
        }
        else { Debug.LogError("Invalid listOfContactPool (Null)"); }
    }

    /// <summary>
    /// Actor datasets for DataManager.cs collections
    /// </summary>
    private void ReadActorData()
    {
        //
        // - - - dictOfActors (load first)
        // 
        Dictionary<int, Actor> dictOfActors = GameManager.instance.dataScript.GetDictOfActors();
        if (dictOfActors != null)
        {
            if (read.actorData.listOfDictActors != null)
            {
                //clear dictionary
                dictOfActors.Clear();
                //loop list of saved actors
                for (int i = 0; i < read.actorData.listOfDictActors.Count; i++)
                {
                    //
                    // - - - Recreate actor
                    //
                    Actor actor = new Actor();
                    SaveActor readActor = read.actorData.listOfDictActors[i];
                    //copy over data from saveActor

                    actor.actorID = readActor.actorID;
                    actor.datapoint0 = readActor.datapoint0;
                    actor.datapoint1 = readActor.datapoint1;
                    actor.datapoint2 = readActor.datapoint2;
                    actor.side = readActor.side;
                    actor.slotID = readActor.slotID;
                    actor.level = readActor.level;
                    actor.nodeCaptured = readActor.nodeCaptured;
                    actor.SetGear(readActor.gearID);
                    actor.isMale = readActor.isMale;
                    actor.actorName = readActor.actorName;
                    actor.firstName = readActor.firstName;
                    actor.arc = GameManager.instance.dataScript.GetActorArc(readActor.arcID);
                    actor.AddTrait(readActor.trait);
                    actor.Status = readActor.status; //needs to be after SetGear

                    //fast access
                    actor.actorStressNone = read.actorData.actorStressNone;
                    actor.actorCorruptNone = read.actorData.actorCorruptNone;
                    actor.actorUnhappyNone = read.actorData.actorUnhappyNone;
                    actor.actorBlackmailNone = read.actorData.actorBlackmailNone;
                    actor.actorBlackmailTimerHigh = read.actorData.actorBlackmailTimerHigh;
                    actor.actorBlackmailTimerLow = read.actorData.actorBlackmailTimerLow;
                    actor.maxNumOfSecrets = read.actorData.maxNumOfSecrets;

                    //data which can be ignored (default values O.K) if actor is in the Recruit Pool
                    if (actor.Status != ActorStatus.RecruitPool)
                    {
                        actor.Renown = readActor.Renown;
                        actor.unhappyTimer = readActor.unhappyTimer;
                        actor.blackmailTimer = readActor.blackmailTimer;
                        actor.captureTimer = readActor.captureTimer;
                        actor.numOfTimesBullied = readActor.numOfTimesBullied;
                        actor.numOfTimesCaptured = readActor.numOfTimesCaptured;
                        actor.departedNumOfSecrets = readActor.departedNumOfSecrets;
                        actor.isPromised = readActor.isPromised;
                        actor.isNewRecruit = readActor.isNewRecruit;
                        actor.isReassured = readActor.isReassured;
                        actor.isThreatening = readActor.isThreatening;
                        actor.isComplaining = readActor.isComplaining;
                        actor.isBreakdown = readActor.isBreakdown;
                        actor.isLieLowFirstturn = readActor.isLieLowFirstturn;
                        actor.isStressLeave = readActor.isStressLeave;
                        actor.isTraitor = readActor.isTraitor;
                        actor.tooltipStatus = readActor.tooltipStatus;
                        actor.inactiveStatus = readActor.inactiveStatus;
                        actor.SetGearTimer(readActor.gearTimer);
                        actor.SetGearTimesTaken(readActor.gearTimesTaken);
                        //teams
                        List<int> listOfTeams = actor.GetListOfTeams();
                        if (listOfTeams != null)
                        {
                            listOfTeams.Clear();
                            listOfTeams.AddRange(readActor.listOfTeams);
                        }
                        else { Debug.LogError("Invalid listOfTeams (Null)"); }
                        //secrets
                        List<Secret> listOfSecrets = actor.GetListOfSecrets();
                        listOfSecrets.Clear();
                        if (listOfSecrets != null)
                        {
                            for (int j = 0; j < readActor.listOfSecrets.Count; j++)
                            {
                                Secret secret = GameManager.instance.dataScript.GetSecret(readActor.listOfSecrets[j]);
                                if (secret != null)
                                { listOfSecrets.Add(secret); }
                                else { Debug.LogWarningFormat("Invalid secret in readActor.listOfSecrets[{0}]", j); }
                            }
                        }
                        else { Debug.LogError("Invalid listOfSecrets (Null)"); }
                        //contacts
                        Dictionary<int, Contact> dictOfContacts = actor.GetDictOfContacts();
                        if (dictOfContacts != null)
                        {
                            dictOfContacts.Clear();
                            for (int j = 0; j < readActor.listOfContactNodes.Count; j++)
                            {
                                Contact contact = GameManager.instance.dataScript.GetContact(readActor.listOfContacts[j]);
                                if (contact != null)
                                {
                                    //add to dictionary
                                    try
                                    { dictOfContacts.Add(readActor.listOfContactNodes[j], contact); }
                                    catch (ArgumentException)
                                    { Debug.LogErrorFormat("Duplicate contact, ID \"{0}\"{1}", contact.contactID, "\n"); }

                                }
                                else { Debug.LogWarningFormat("Invalid contact (Null) for readActor.listOfContacts[{0}]", j); }
                            }
                        }
                        else { Debug.LogError("Invalid dictOfContacts (Null)"); }
                    }
                    //
                    // - - - Add to dictionary
                    //
                    try
                    { dictOfActors.Add(actor.actorID, actor); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Duplicate actor, ID \"{0}\"{1}", actor.actorID, "\n"); }
                }
            }
            else { Debug.LogError("Invalid saveData.listOfDictActors (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfActors (Null)"); }
        //
        // - - - Actor arrays
        //
        int index;
        int maxIndex;
        int sideNum = GameManager.instance.dataScript.GetNumOfGlobalSide();
        int actorNum = GameManager.instance.actorScript.maxNumOfOnMapActors;
        if (read.actorData.listOfActors != null)
        {
            //arrayOfActors
            Actor[,] arrayOfActors = GameManager.instance.dataScript.GetArrayOfActors();
            if (arrayOfActors != null)
            {
                maxIndex = arrayOfActors.Length;
                //empty out array
                Array.Clear(arrayOfActors, 0, arrayOfActors.Length);
                int actorID;
                Actor actor;
                //repopulate with save data
                for (int i = 0; i < sideNum; i++)
                {
                    for (int j = 0; j < actorNum; j++)
                    {
                        index = (i * sideNum) + j;
                        Debug.Assert(index < maxIndex, string.Format("Index {0} >= maxIndex {1}", index, maxIndex));
                        actorID = read.actorData.listOfActors[index];
                        if (actorID == -1)
                        { actor = null; }
                        else
                        {
                            actor = GameManager.instance.dataScript.GetActor(actorID);
                            if (actor == null)
                            { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
                        }
                        //add to array
                        arrayOfActors[i, j] = actor;
                    }
                }
            }
            else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        }
        else { Debug.LogError("Invalid read.actorData.listOfActorsActor (Nul)"); }
        //arrayOfActorsPresent
        bool[,] arrayOfActorsPresent = GameManager.instance.dataScript.GetArrayOfActorsPresent();
        if (arrayOfActorsPresent != null)
        {
            //empty out array
            Array.Clear(arrayOfActorsPresent, 0, arrayOfActorsPresent.Length);
            for (int i = 0; i < sideNum; i++)
            {
                for (int j = 0; j < actorNum; j++)
                {
                    index = (i * sideNum) + j;
                    arrayOfActorsPresent[i, j] = read.actorData.listOfActorsPresent[index];
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfActorsPresent (Null)"); }
        //
        // - - - Actor Lists
        //
        List<int> authorityActorPoolLevelOne = GameManager.instance.dataScript.GetActorRecruitPool(1, globalAuthority);
        List<int> authorityActorPoolLevelTwo = GameManager.instance.dataScript.GetActorRecruitPool(2, globalAuthority);
        List<int> authorityActorPoolLevelThree = GameManager.instance.dataScript.GetActorRecruitPool(3, globalAuthority);
        List<int> resistanceActorPoolLevelOne = GameManager.instance.dataScript.GetActorRecruitPool(1, globalResistance);
        List<int> resistanceActorPoolLevelTwo = GameManager.instance.dataScript.GetActorRecruitPool(2, globalResistance);
        List<int> resistanceActorPoolLevelThree = GameManager.instance.dataScript.GetActorRecruitPool(3, globalResistance);
        List<int> authorityActorReserve = GameManager.instance.dataScript.GetListOfReserveActors(globalAuthority);
        List<int> authorityActorDismissed = GameManager.instance.dataScript.GetListOfDismissedActors(globalAuthority);
        List<int> authorityActorPromoted = GameManager.instance.dataScript.GetListOfPromotedActors(globalAuthority);
        List<int> authorityActorDisposedOf = GameManager.instance.dataScript.GetListOfDisposedOfActors(globalAuthority);
        List<int> authorityActorResigned = GameManager.instance.dataScript.GetListOfResignedActors(globalAuthority);
        List<int> resistanceActorReserve = GameManager.instance.dataScript.GetListOfReserveActors(globalResistance);
        List<int> resistanceActorDismissed = GameManager.instance.dataScript.GetListOfDismissedActors(globalResistance);
        List<int> resistanceActorPromoted = GameManager.instance.dataScript.GetListOfPromotedActors(globalResistance);
        List<int> resistanceActorDisposedOf = GameManager.instance.dataScript.GetListOfDisposedOfActors(globalResistance);
        List<int> resistanceActorResigned = GameManager.instance.dataScript.GetListOfResignedActors(globalResistance);
        //null checks
        if (authorityActorPoolLevelOne == null) { Debug.LogError("Invalid authorityActorPoolLevelOne (Null)"); }
        if (authorityActorPoolLevelTwo == null) { Debug.LogError("Invalid authorityActorPoolLevelTwo (Null)"); }
        if (authorityActorPoolLevelThree == null) { Debug.LogError("Invalid authorityActorPoolLevelThree (Null)"); }
        if (resistanceActorPoolLevelOne == null) { Debug.LogError("Invalid resistanceActorPoolLevelOne (Null)"); }
        if (resistanceActorPoolLevelTwo == null) { Debug.LogError("Invalid resistanceActorPoolLevelTwo (Null)"); }
        if (resistanceActorPoolLevelThree == null) { Debug.LogError("Invalid resistanceActorPoolLevelThree (Null)"); }
        if (authorityActorReserve == null) { Debug.LogError("Invalid authorityActorReserve (Null)"); }
        if (authorityActorDismissed == null) { Debug.LogError("Invalid authorityActorDismissed (Null)"); }
        if (authorityActorPromoted == null) { Debug.LogError("Invalid authorityActorPromoted (Null)"); }
        if (authorityActorDisposedOf == null) { Debug.LogError("Invalid authorityActorDsposedOf (Null)"); }
        if (authorityActorResigned == null) { Debug.LogError("Invalid authorityActorResigned (Null)"); }
        if (resistanceActorReserve == null) { Debug.LogError("Invalid resistanceActorReserve (Null)"); }
        if (resistanceActorDismissed == null) { Debug.LogError("Invalid resistanceActorDismissed (Null)"); }
        if (resistanceActorPromoted == null) { Debug.LogError("Invalid resistanceActorPromoted (Null)"); }
        if (resistanceActorDisposedOf == null) { Debug.LogError("Invalid resistanceActorDsposedOf (Null)"); }
        if (resistanceActorResigned == null) { Debug.LogError("Invalid resistanceActorResigned (Null)"); }
        //clear lists
        authorityActorPoolLevelOne.Clear();
        authorityActorPoolLevelTwo.Clear();
        authorityActorPoolLevelThree.Clear();
        resistanceActorPoolLevelOne.Clear();
        resistanceActorPoolLevelTwo.Clear();
        resistanceActorPoolLevelThree.Clear();
        authorityActorReserve.Clear();
        authorityActorDismissed.Clear();
        authorityActorPromoted.Clear();
        authorityActorDisposedOf.Clear();
        authorityActorResigned.Clear();
        resistanceActorReserve.Clear();
        resistanceActorDismissed.Clear();
        resistanceActorPromoted.Clear();
        resistanceActorDisposedOf.Clear();
        resistanceActorResigned.Clear();
        //add saved data
        authorityActorPoolLevelOne.AddRange(read.actorData.authorityActorPoolLevelOne);
        authorityActorPoolLevelTwo.AddRange(read.actorData.authorityActorPoolLevelTwo);
        authorityActorPoolLevelThree.AddRange(read.actorData.authorityActorPoolLevelThree);
        resistanceActorPoolLevelOne.AddRange(read.actorData.resistanceActorPoolLevelOne);
        resistanceActorPoolLevelTwo.AddRange(read.actorData.resistanceActorPoolLevelTwo);
        resistanceActorPoolLevelThree.AddRange(read.actorData.resistanceActorPoolLevelThree);
        authorityActorReserve.AddRange(read.actorData.authorityActorReserve);
        authorityActorDismissed.AddRange(read.actorData.authorityActorDismissed);
        authorityActorPromoted.AddRange(read.actorData.authorityActorPromoted);
        authorityActorDisposedOf.AddRange(read.actorData.authorityActorDisposedOf);
        authorityActorResigned.AddRange(read.actorData.authorityActorResigned);
        resistanceActorReserve.AddRange(read.actorData.resistanceActorReserve);
        resistanceActorDismissed.AddRange(read.actorData.resistanceActorDismissed);
        resistanceActorPromoted.AddRange(read.actorData.resistanceActorPromoted);
        resistanceActorDisposedOf.AddRange(read.actorData.resistanceActorDisposedOf);
        resistanceActorResigned.AddRange(read.actorData.resistanceActorResigned);

    }


    //
    // - - -  Validate - - -
    //

    /// <summary>
    /// Validate PlayerManager.cs data (logic checks) and Initialise gfx's where needed
    /// </summary>
    private void ValidatePlayerData()
    {
        ActorStatus status = read.playerData.status;
        ActorTooltip tooltipStatus = read.playerData.tooltipStatus;
        ActorInactive inactiveStatus = read.playerData.inactiveStatus;
        //Secondary fields must match primary field
        if (status == ActorStatus.Active)
        {
            if (tooltipStatus != ActorTooltip.None)
            {
                Debug.LogWarningFormat("[Fil] FileManager.cs -> ValidatePlayerData: tooltipStatus \"{0}\" invalid, changed to 'None'{1}", tooltipStatus, "\n");
                GameManager.instance.playerScript.tooltipStatus = ActorTooltip.None;
            }
            if (inactiveStatus != ActorInactive.None)
            {
                Debug.LogWarningFormat("Player status Active -> inactiveStatus \"{0}\" invalid, changed to 'None'{1}", inactiveStatus, "\n");
                GameManager.instance.playerScript.inactiveStatus = ActorInactive.None;
            }
        }
        else if (status == ActorStatus.Inactive)
        {
            //secondary field doesn't match primary. Revert all to primary state.
            if (tooltipStatus == ActorTooltip.None || inactiveStatus == ActorInactive.None)
            {
                Debug.LogWarningFormat("Player INACTIVE -> tooltipStatus \"{0}\" Or inactiveStatus \"{1}\" invalid,   PlayerStatus changed to 'Active'{2}", 
                    tooltipStatus, inactiveStatus, "\n");
                GameManager.instance.playerScript.status = ActorStatus.Active;
                GameManager.instance.playerScript.tooltipStatus = ActorTooltip.None;
                GameManager.instance.playerScript.inactiveStatus = ActorInactive.None;
            }
            else
            {
                //change alpha of player to indicate inactive status
                GameManager.instance.actorPanelScript.UpdatePlayerAlpha(GameManager.instance.guiScript.alphaInactive);
            }
        }
    }

    /// <summary>
    /// Validate Actor related data and update UI gfx
    /// </summary>
    private void ValidateActorData()
    {
        //update actor UI Panel
        GameManager.instance.actorPanelScript.UpdateActorPanel();
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //loop each OnMap actor and update alpha and renown
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(playerSide);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, playerSide) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    {
                        //update alpha
                        if (actor.Status == ActorStatus.Active)
                        { GameManager.instance.actorPanelScript.UpdateActorAlpha(i, alphaActive); }
                        else
                        { GameManager.instance.actorPanelScript.UpdateActorAlpha(i, alphaInactive); }
                        //update renown
                        GameManager.instance.actorPanelScript.UpdateActorRenownUI(i, actor.Renown);
                    }
                }
                else
                {
                    //actor not present in slot, reset renown to 0
                    GameManager.instance.actorPanelScript.UpdateActorRenownUI(i, 0);
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }

    }



    //new methods above here
}
