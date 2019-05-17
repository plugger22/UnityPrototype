using gameAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// data class to pass onto topWidget to update with saved data
/// </summary>
public class TopWidgetData
{
    public GlobalSide side;
    public int turn;
    public int actionPoints;
    public int cityLoyalty;
    public int factionSupport;
    public float objectiveOne;
    public float objectiveTwo;
    public float objectiveThree;
    public bool isSecurityFlash;
}

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

    #region Initialise
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
    #endregion

    //
    // - - - Master Methods - - -
    //

    #region Write Game Data
    /// <summary>
    /// copy inGame data to saveData
    /// </summary>
    public void WriteSaveData()
    {
        write = new Save();
        //Sequentially write data
        WriteDataData();
        WriteCampaignData();
        WriteOptionData();
        WritePlayerData();
        WriteNemesisData();
        WriteGameData();
        WriteActorData();
        WriteNodeData();
        WriteGearData();
        WriteAIData();
    }
    #endregion

    #region SaveGame
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
    #endregion


    #region Read Save Data
    /// <summary>
    /// Read Save method, returns true if successful, false otherise
    /// </summary>
    public bool ReadSaveData()
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
    #endregion

    #region Load Save Data
    /// <summary>
    /// Load data from Save file back into game
    /// </summary>
    public void LoadSaveData()
    {
        if (read != null)
        {
            //side (player) at start
            ReadOptionData();
            ReadGameData();          
            ReadDataData();
            ReadCampaignData();
            //set up level based on loaded current scenario seed
            GameManager.instance.InitialiseLoadGame(read.gameData.playerSide.level);
            ReadNodeData();
            ReadNemesisData();
            ReadGearData();
            ReadAIData();
            ReadActorData();
            ValidateActorData();
            ReadPlayerData();
            ValidatePlayerData();
            UpdateGUI();
            Debug.LogFormat("[Fil] FileManager.cs -> LoadSaveData: Saved Game Data has been LOADED{0}", "\n");
        }
    }
    #endregion

    //
    // - - - Write - - -
    //

    #region Write Campaign Data
    /// <summary>
    /// CampaignManager.cs data write to file
    /// </summary>
    private void WriteCampaignData()
    {
        write.campaignData.campaignID = GameManager.instance.campaignScript.campaign.campaignID;
        write.campaignData.scenarioIndex = GameManager.instance.campaignScript.GetScenarioIndex();
        write.campaignData.arrayOfStoryStatus = GameManager.instance.campaignScript.GetArrayOfStoryStatus();
    }
    #endregion


    #region Write Option Data
    /// <summary>
    /// OptionManager.cs data write to file
    /// </summary>
    private void WriteOptionData()
    {
        write.optionData.autoGearResolution = GameManager.instance.optionScript.autoGearResolution;
        write.optionData.fogOfWar = GameManager.instance.optionScript.fogOfWar;
        write.optionData.debugData = GameManager.instance.optionScript.debugData;
        write.optionData.noAI = GameManager.instance.optionScript.noAI;
        write.optionData.showContacts = GameManager.instance.optionScript.showContacts;
        write.optionData.showRenown = GameManager.instance.optionScript.showRenown;
        write.optionData.connectorTooltips = GameManager.instance.optionScript.connectorTooltips;
        write.optionData.colourScheme = GameManager.instance.optionScript.ColourOption;
    }
    #endregion


    #region Write Nemesis Data
    /// <summary>
    /// NemesisManager.cs data write to file
    /// </summary>
    private void WriteNemesisData()
    {
        write.nemesisData.saveData = GameManager.instance.nemesisScript.WriteSaveData();
        Debug.Assert(write.nemesisData.saveData != null, "Invalid Nemesis saveData (Null)");
    }
    #endregion


    #region Write Player data
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
    #endregion


    #region Write Game Data
    /// <summary>
    /// Important Game data write to file
    /// </summary>
    private void WriteGameData()
    {
        //sideManager.cs
        write.gameData.resistanceCurrent = GameManager.instance.sideScript.resistanceCurrent;
        write.gameData.authorityCurrent = GameManager.instance.sideScript.authorityCurrent;
        write.gameData.resistanceOverall = GameManager.instance.sideScript.resistanceOverall;
        write.gameData.authorityOverall = GameManager.instance.sideScript.authorityOverall;
        write.gameData.playerSide = GameManager.instance.sideScript.PlayerSide;
        //turnManager.cs
        write.gameData.turn = GameManager.instance.turnScript.Turn;
        write.gameData.actionPoints = GameManager.instance.turnScript.GetActionsAvailable();
        //cityManager.cs
        write.gameData.cityLoyalty = GameManager.instance.cityScript.CityLoyalty;
        //factionManager.cs
        write.gameData.factionSupportAuthority = GameManager.instance.factionScript.ApprovalAuthority;
        write.gameData.factionSupportResistance = GameManager.instance.factionScript.ApprovalResistance;
        //top widget
        write.gameData.isSecurityFlash = GameManager.instance.widgetTopScript.CheckSecurityFlash();
    }
    #endregion


    #region Write Data Data
    /// <summary>
    /// DataManager.cs data write to file
    /// </summary>
    private void WriteDataData()
    {

        #region secrets
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
        #endregion
        #region contacts
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
                IntListWrapper listOfContacts = new IntListWrapper();
                listOfContacts.myList.AddRange(contactList.Value);
                if (listOfContacts.myList != null)
                {
                    write.dataData.listOfActorContactsValue.Add(listOfContacts);
                    write.dataData.listOfActorContactsKey.Add(contactList.Key);
                }
                else { Debug.LogWarningFormat("Invalid listOfContacts (Null) for actorID {0}", contactList.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfActorContacts (Null)"); }
        //dictOfNodeContactsResistance
        Dictionary<int, List<int>> dictOfNodeContactsResistance = GameManager.instance.dataScript.GetDictOfNodeContacts(globalResistance);
        if (dictOfNodeContactsResistance != null)
        {
            foreach (var actorList in dictOfNodeContactsResistance)
            {
                IntListWrapper listOfActors = new IntListWrapper();
                listOfActors.myList.AddRange(actorList.Value);
                if (listOfActors.myList != null)
                {
                    write.dataData.listOfNodeContactsByResistanceValue.Add(listOfActors);
                    write.dataData.listOfNodeContactsByResistanceKey.Add(actorList.Key);
                }
                else { Debug.LogWarningFormat("Invalid listOfActors (Null) for nodeID {0}", actorList.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfNodeContactsResistance (Null)"); }
        //dictOfNodeContactsAuthority
        Dictionary<int, List<int>> dictOfNodeContactsAuthority = GameManager.instance.dataScript.GetDictOfNodeContacts(globalAuthority);
        if (dictOfNodeContactsAuthority != null)
        {
            foreach (var actorList in dictOfNodeContactsAuthority)
            {
                IntListWrapper listOfActors = new IntListWrapper();
                listOfActors.myList.AddRange(actorList.Value);
                if (listOfActors.myList != null)
                {
                    write.dataData.listOfNodeContactsByAuthorityValue.Add(listOfActors);
                    write.dataData.listOfNodeContactsByAuthorityKey.Add(actorList.Key);
                }
                else { Debug.LogWarningFormat("Invalid listOfActors (Null) for nodeID {0}", actorList.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfNodeContactsAuthority (Null)"); }
        //dictOfNodeContactsResistance
        Dictionary<int, List<Contact>> dictOfContactsByNodeResistance = GameManager.instance.dataScript.GetDictOfContactsByNodeResistance();
        if (dictOfContactsByNodeResistance != null)
        {
            foreach (var contactList in dictOfContactsByNodeResistance)
            {
                ContactListWrapper listOfContacts = new ContactListWrapper();
                listOfContacts.myList.AddRange(contactList.Value);
                if (listOfContacts.myList != null)
                {
                    write.dataData.listOfContactsByNodeResistanceValue.Add(listOfContacts);
                    write.dataData.listOfContactsByNodeResistanceKey.Add(contactList.Key);
                }
                else { Debug.LogWarningFormat("Invalid listOfContacts (Null) for nodeID {0}", contactList.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfContactsByNodeResistance (Null)"); }
        #endregion
        #region teams
        //
        // - - - Teams
        //
        Dictionary<int, Team> dictOfTeams = GameManager.instance.dataScript.GetDictOfTeams();
        if (dictOfTeams != null)
        {
            foreach(var record in dictOfTeams)
            {
                if (record.Value != null)
                {
                    //create SaveTeam and copy data across
                    SaveTeam saveTeam = new SaveTeam();
                    saveTeam.teamID = record.Value.teamID;
                    saveTeam.teamName = record.Value.teamName;
                    saveTeam.pool = record.Value.pool;
                    saveTeam.arcID = record.Value.arc.TeamArcID;
                    saveTeam.actorSlotID = record.Value.actorSlotID;
                    saveTeam.nodeID = record.Value.nodeID;
                    saveTeam.timer = record.Value.timer;
                    saveTeam.turnDeployed = record.Value.turnDeployed;
                    //add to list
                    write.dataData.listOfTeams.Add(saveTeam);
                }
                else { Debug.LogWarningFormat("Invalid team (Null) for teamID {0}", record.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfTeams (Null)"); }
        //arrayOfTeams
        int[,] arrayOfTeams = GameManager.instance.dataScript.GetArrayOfTeams();
        if (arrayOfTeams != null)
        {
            //check array dimensions (similar check on load)
            int numOfArcs = GameManager.instance.dataScript.CheckNumOfNodeArcs();
            int numOfInfo = (int)TeamInfo.Count;
            int size0 = arrayOfTeams.GetUpperBound(0) + 1;
            int size1 = arrayOfTeams.GetUpperBound(1) + 1;
            Debug.AssertFormat(numOfArcs == size0, "Mismatch on array size, rank 0 (Arcs) should be {0}, is {1}", numOfArcs, size0);
            Debug.AssertFormat(numOfInfo == size1, "Mismatch on arraySize, rank 1 (TeamInfo) should be {0}, is {1}", numOfInfo, size1);
            //write data
            for (int i = 0; i < size0; i++)
            {
                for (int j = 0; j < size1; j++)
                { write.dataData.listOfArrayOfTeams.Add(arrayOfTeams[i, j]);  }
            }
        }
        else { Debug.LogError("Invalid arrayOfTeams (Null)"); }
        //list -> reserves
        List<int> tempList = GameManager.instance.dataScript.GetTeamPool(TeamPool.Reserve);
        if (tempList != null)
        { write.dataData.listOfTeamPoolReserve.AddRange(tempList); }
        else { Debug.LogError("Invalid teamPoolReserve list (Null)"); }
        //list -> OnMap
        tempList = GameManager.instance.dataScript.GetTeamPool(TeamPool.OnMap);
        if (tempList != null)
        { write.dataData.listOfTeamPoolOnMap.AddRange(tempList); }
        else { Debug.LogError("Invalid teamPoolOnMap list (Null)"); }
        //list -> InTransit
        tempList = GameManager.instance.dataScript.GetTeamPool(TeamPool.InTransit);
        if (tempList != null)
        { write.dataData.listOfTeamPoolInTransit.AddRange(tempList); }
        else { Debug.LogError("Invalid teamPoolInTransit list (Null)"); }
        //counter
        write.dataData.teamCounter = GameManager.instance.teamScript.teamCounter;
        #endregion
        #region statistics
        Dictionary<StatType, int> dictOfStatistics = GameManager.instance.dataScript.GetDictOfStatistics();
        if (dictOfStatistics != null)
        {
            //only need to save stats as StatType Key's are sequentially numbered enums
            foreach(var stat in dictOfStatistics)
            { write.dataData.listOfStatistics.Add(stat.Value); }
        }
        else { Debug.LogError("Invalid dictOfStatistics (Null)"); }
        #endregion
        #region AI
        //array -> AI Resources
        int[] arrayOfAIResources = GameManager.instance.dataScript.GetArrayOfAIResources();
        if (arrayOfAIResources != null)
        { write.dataData.listOfArrayOfAIResources.AddRange(arrayOfAIResources.ToList()); }
        else { Debug.LogError("Invalid arrayOfAIResources (Null)"); }
        //queue -> recentNodes
        Queue<AITracker> queueOfRecentNodes = GameManager.instance.dataScript.GetRecentNodesQueue();
        if (queueOfRecentNodes != null)
        { write.dataData.listOfRecentNodes.AddRange(queueOfRecentNodes.ToList()); }
        else { Debug.LogError("Invalid queueOfRecentNodes (Null)"); }
        //queue -> recentConnections
        Queue<AITracker> queueOfRecentConnections = GameManager.instance.dataScript.GetRecentConnectionsQueue();
        if (queueOfRecentConnections != null)
        { write.dataData.listOfRecentConnections.AddRange(queueOfRecentConnections.ToList()); }
        else { Debug.LogError("Invalid queueOfRecentConnections (Null)"); }
        #endregion
        #region messages
        //archive
        Dictionary<int, Message> dictOfArchiveMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Archive);
        if (dictOfArchiveMessages != null)
        {
            write.dataData.listOfArchiveMessagesKey.AddRange(dictOfArchiveMessages.Keys.ToList());
            write.dataData.listOfArchiveMessagesValue.AddRange(dictOfArchiveMessages.Values.ToList());
        }
        else { Debug.LogError("Invalid dictOfArchiveMessages (Null)"); }
        //pending
        Dictionary<int, Message> dictOfPendingMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Pending);
        if (dictOfPendingMessages != null)
        {
            write.dataData.listOfPendingMessagesKey.AddRange(dictOfPendingMessages.Keys.ToList());
            write.dataData.listOfPendingMessagesValue.AddRange(dictOfPendingMessages.Values.ToList());
        }
        else { Debug.LogError("Invalid dictOfPendingMessages (Null)"); }
        //current
        Dictionary<int, Message> dictOfCurrentMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Current);
        if (dictOfCurrentMessages != null)
        {
            write.dataData.listOfCurrentMessagesKey.AddRange(dictOfCurrentMessages.Keys.ToList());
            write.dataData.listOfCurrentMessagesValue.AddRange(dictOfCurrentMessages.Values.ToList());
        }
        else { Debug.LogError("Invalid dictOfCurrentMessages (Null)"); }
        //ai
        Dictionary<int, Message> dictOfAIMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.AI);
        if (dictOfAIMessages != null)
        {
            write.dataData.listOfAIMessagesKey.AddRange(dictOfAIMessages.Keys.ToList());
            write.dataData.listOfAIMessagesValue.AddRange(dictOfAIMessages.Values.ToList());
        }
        else { Debug.LogError("Invalid dictOfAIMessages (Null)"); }
        //id counter
        write.dataData.messageCounter = GameManager.instance.messageScript.messageCounter;
        #endregion
    }
    #endregion


    #region Write Actor data
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
        //write data to lists (not multidimensional arrays split into single lists for serialization)
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
    #endregion


    #region Write Node data
    /// <summary>
    /// NodeManager.cs data to file
    /// </summary>
    private void WriteNodeData()
    {
        write.nodeData.crisisPolicyModifier = GameManager.instance.nodeScript.crisisPolicyModifier;
        write.nodeData.nodeCounter = GameManager.instance.nodeScript.nodeCounter;
        write.nodeData.connCounter = GameManager.instance.nodeScript.connCounter;
        write.nodeData.nodeHighlight = GameManager.instance.nodeScript.nodeHighlight;
        write.nodeData.nodePlayer = GameManager.instance.nodeScript.nodePlayer;
        write.nodeData.nodeNemesis = GameManager.instance.nodeScript.nodeNemesis;
        write.nodeData.nodeCounter = GameManager.instance.nodeScript.nodeCaptured;
        //
        // - - - Node.cs
        //
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        if (dictOfNodes != null)
        {
            foreach (var record in dictOfNodes)
            {
                if (record.Value != null)
                {
                    //create new SaveNode
                    SaveNode saveNode = new SaveNode();
                    //copy across dynamic node data
                    saveNode.nodeID = record.Value.nodeID;
                    saveNode.security = record.Value.Security;
                    saveNode.stability = record.Value.Stability;
                    saveNode.support = record.Value.Support;
                    saveNode.isTracerKnown = record.Value.isTracerKnown;
                    saveNode.isSpiderKnown = record.Value.isSpiderKnown;
                    saveNode.isContactKnown = record.Value.isContactKnown;
                    saveNode.isTeamKnown = record.Value.isTeamKnown;
                    saveNode.isTargetKnown = record.Value.isTargetKnown;
                    saveNode.isTracer = record.Value.isTracer;
                    saveNode.isSpider = record.Value.isSpider;
                    saveNode.isContactResistance = record.Value.isContactResistance;
                    saveNode.isContactAuthority = record.Value.isContactAuthority;
                    saveNode.isPreferredAuthority = record.Value.isPreferredAuthority;
                    saveNode.isPreferredResistance = record.Value.isPreferredResistance;
                    saveNode.isCentreNode = record.Value.isCentreNode;
                    saveNode.isLoiterNode = record.Value.isLoiterNode;
                    saveNode.isConnectedNode = record.Value.isConnectedNode;
                    saveNode.isChokepointNode = record.Value.isChokepointNode;
                    saveNode.targetID = record.Value.targetID;
                    saveNode.spiderTimer = record.Value.spiderTimer;
                    saveNode.tracerTimer = record.Value.tracerTimer;
                    saveNode.activityCount = record.Value.activityCount;
                    saveNode.activityTime = record.Value.activityTime;
                    saveNode.isStabilityTeam = record.Value.isStabilityTeam;
                    saveNode.isSecurityTeam = record.Value.isSecurityTeam;
                    saveNode.isSupportTeam = record.Value.isSupportTeam;
                    saveNode.isProbeTeam = record.Value.isProbeTeam;
                    saveNode.isSpiderTeam = record.Value.isSpiderTeam;
                    saveNode.isDamageTeam = record.Value.isDamageTeam;
                    saveNode.isErasureTeam = record.Value.isErasureTeam;
                    saveNode.crisisTimer = record.Value.crisisTimer;
                    saveNode.waitTimer = record.Value.waitTimer;
                    saveNode.crisis = record.Value.crisis;
                    //cure
                    if (record.Value.cure == null)
                    { saveNode.cureID = -1; }
                    else { saveNode.cureID = record.Value.cure.cureID; }
                    saveNode.loiter = record.Value.loiter;
                    //teams
                    List<Team> listOfTeams = record.Value.GetListOfTeams();
                    if (listOfTeams != null)
                    {
                        for (int i = 0; i < listOfTeams.Count; i++)
                        {
                            Team team = listOfTeams[i];
                            if (team != null)
                            { saveNode.listOfTeams.Add(team.teamID); }
                            else { Debug.LogWarningFormat("Invalid team (Null) in listOfTeams[{0}]", i); }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid listOfTeams (Null) for nodeID {0}", record.Key); }
                    //ongoing effects
                    List<EffectDataOngoing> listOfOngoing = record.Value.GetListOfOngoingEffects();
                    if (listOfOngoing != null)
                    {
                        for (int i = 0; i < listOfOngoing.Count; i++)
                        {
                            EffectDataOngoing effectOngoing = listOfOngoing[i];
                            if (effectOngoing != null)
                            { saveNode.listOfOngoingEffects.Add(effectOngoing.ongoingID); }
                            else { Debug.LogWarningFormat("Invalid effectOngoing (Null) for listOfOngoing[{0}]", i); }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid listOfOngoingEffects (Null) for nodeID {0}", record.Key); }
                    //add to list
                    write.nodeData.listOfNodes.Add(saveNode);
                    
                }
                else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0} in dictOfNodes", record.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfNodes (Null)"); }
        //
        // - - - Crisis Nodes
        //
        List<Node> listOfCrisisNodes = GameManager.instance.dataScript.GetListOfCrisisNodes();
        if (listOfCrisisNodes != null)
        {
                //save nodeID's
                for (int i = 0; i < listOfCrisisNodes.Count; i++)
                {
                    Node node = listOfCrisisNodes[i];
                    if (node != null)
                    { write.nodeData.listOfCrisisNodes.Add(node.nodeID); }
                    else { Debug.LogWarningFormat("Invalid node (Null) in listOfCrisisNodes[{0}]", i); }
                }
        }
        else { Debug.LogError("Invalid listOfCrisisNodes (Null)"); }
        //
        // - - - Cure Nodes
        //
        List<Node> listOfCureNodes = GameManager.instance.dataScript.GetListOfCureNodes();
        if (listOfCureNodes != null)
        {
            //save nodeID's
            for (int i = 0; i < listOfCureNodes.Count; i++)
            {
                Node node = listOfCureNodes[i];
                if (node != null)
                { write.nodeData.listOfCureNodes.Add(node.nodeID); }
                else { Debug.LogWarningFormat("Invalid node (Null) in listOfCureNodes[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfCureNodes (Null)"); }
    }
    #endregion


    #region Write Gear Data
    /// <summary>
    /// GearManager.cs data to file
    /// </summary>
    private void WriteGearData()
    {
        Dictionary<int, Gear> dictOfGear = GameManager.instance.dataScript.GetDictOfGear();
        if (dictOfGear != null)
        {
            foreach(var gear in dictOfGear)
            {
                if (gear.Value != null)
                {
                    //create new SaveGear object and copy across all dynamic data
                    SaveGear saveGear = new SaveGear();
                    saveGear.gearID = gear.Value.gearID;
                    saveGear.timesUsed = gear.Value.timesUsed;
                    saveGear.isCompromised = gear.Value.isCompromised;
                    saveGear.reasonUsed = gear.Value.reasonUsed;
                    saveGear.chanceOfCompromise = gear.Value.chanceOfCompromise;
                    saveGear.statTurnObtained = gear.Value.statTurnObtained;
                    saveGear.statTurnLost = gear.Value.statTurnLost;
                    saveGear.statTimesUsed = gear.Value.statTimesUsed;
                    saveGear.statTimesGiven = gear.Value.statTimesGiven;
                    saveGear.statTimesCompromised = gear.Value.statTimesCompromised;
                    saveGear.statTimesSaved = gear.Value.statTimesSaved;
                    saveGear.statRenownSpent = gear.Value.statRenownSpent;
                    //add to list
                    write.gearData.listOfGear.Add(saveGear);
                }
                else { Debug.LogWarningFormat("Invalid gear (Null) for gearID {0}", gear.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfGear (Null)"); }
        //list -> listOfCommonGear
        List<int> tempList = GameManager.instance.dataScript.GetListOfGear(GameManager.instance.gearScript.gearCommon);
        if (tempList != null)
        { write.gearData.listOfCommonGear.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfCommonGear (Null)"); }
        //list -> listOfRareGear
        tempList = GameManager.instance.dataScript.GetListOfGear(GameManager.instance.gearScript.gearRare);
        if (tempList != null)
        { write.gearData.listOfRareGear.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfRareGear (Null)"); }
        //list -> listOfUniqueGear
        tempList = GameManager.instance.dataScript.GetListOfGear(GameManager.instance.gearScript.gearUnique);
        if (tempList != null)
        { write.gearData.listOfUniqueGear.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfUniqueGear (Null)"); }
        //list -> listOfLostGear
        tempList = GameManager.instance.dataScript.GetListOfLostGear();
        if (tempList != null)
        { write.gearData.listOfLostGear.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfLostGear (Null)"); }
        //list -> listOfCurrentGear
        tempList = GameManager.instance.dataScript.GetListOfCurrentGear();
        if (tempList != null)
        { write.gearData.listOfCurrentGear.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfCurrentGear (Null)"); }
        //GearManager fields
        write.gearData.gearSaveCurrentCost = GameManager.instance.gearScript.GetGearSaveCurrentCost();
        write.gearData.listOfCompromisedGear.AddRange(GameManager.instance.gearScript.GetListOfCompromisedGear());
    }
    #endregion


    #region Write AI Data
    /// <summary>
    /// AI/Rebel Manager.cs write data to file
    /// </summary>
    private void WriteAIData()
    {
        //list -> tasks final
        List<AITask> listOfTasksFinal = GameManager.instance.aiScript.GetListOfTasksFinal();
        if (listOfTasksFinal != null)
        { write.aiData.listOfTasksFinal.AddRange(listOfTasksFinal); }
        else { Debug.LogError("Invalid listOfTaskFinal (Null)"); }
        //list -> player effects
        List<string> listOfPlayerEffects = GameManager.instance.aiScript.GetListOfPlayerEffects();
        if (listOfPlayerEffects != null)
        { write.aiData.listOfPlayerEffects.AddRange(listOfPlayerEffects); }
        else { Debug.LogError("Invalid listOfPlayerEffects (Null)"); }
        //list -> player effect descriptors
        List<string> listOfPlayerEffectDescriptors = GameManager.instance.aiScript.GetListOfPlayerEffectDescriptors();
        if (listOfPlayerEffectDescriptors != null)
        { write.aiData.listOfPlayerEffectDescriptors.AddRange(listOfPlayerEffectDescriptors); }
        else { Debug.LogError("Invalid listOfPlayerEffectDescriptors (Null)"); }
        //AIRebelManager.cs private fields
        write.aiData.saveRebel = GameManager.instance.aiRebelScript.WriteSaveData();
        if (write.aiData.saveRebel == null)
        { Debug.LogError("Invalid AIRebelManager.cs saveRebel data (Null)"); }
        //AIRebelManager -> Nemesis Reports
        List<AITracker> tempListNemesis = GameManager.instance.aiRebelScript.GetListOfNemesisReports();
        if (tempListNemesis != null)
        { write.aiData.listOfNemesisReports.AddRange(tempListNemesis);}
        else { Debug.LogError("Invalid listOfNemesisReports (Null)"); }
        //AIRebelManager -> Erasure Reports
        List<AITracker> tempListErasure = GameManager.instance.aiRebelScript.GetListOfErasureReports();
        if (tempListErasure != null)
        { write.aiData.listOfErasureReports.AddRange(tempListErasure); }
        else { Debug.LogError("Invalid listOfErasureReports (Null)"); }
    }
    #endregion

    //
    // - - - Read - - -
    //

    #region Read Campaign Data
    /// <summary>
    /// CampaignManager.cs Data
    /// </summary>
    private void ReadCampaignData()
    {
        //campaign
        Campaign campaign = GameManager.instance.dataScript.GetCampaign(read.campaignData.campaignID);
        if (campaign != null)
        { GameManager.instance.campaignScript.campaign = campaign; }
        else { Debug.LogErrorFormat("Invalid campaign (Null) for campaignID {0}", read.campaignData.campaignID); }
        //scenario
        GameManager.instance.campaignScript.SetScenario(read.campaignData.scenarioIndex);
        //arrayOfStoryStatus
        GameManager.instance.campaignScript.SetArrayOfStoryStatus(read.campaignData.arrayOfStoryStatus);
    }
    #endregion


    #region Read Option Data
    /// <summary>
    /// OptionManager.cs Data
    /// </summary>
    private void ReadOptionData()
    {
        GameManager.instance.optionScript.autoGearResolution = read.optionData.autoGearResolution;
        GameManager.instance.optionScript.fogOfWar = read.optionData.fogOfWar;
        GameManager.instance.optionScript.debugData = read.optionData.debugData;
        GameManager.instance.optionScript.noAI = read.optionData.noAI;
        GameManager.instance.optionScript.showContacts = read.optionData.showContacts;
        GameManager.instance.optionScript.showRenown = read.optionData.showRenown;
        GameManager.instance.optionScript.connectorTooltips = read.optionData.connectorTooltips;
        GameManager.instance.optionScript.ColourOption = read.optionData.colourScheme;
        //Debug button texts
        if (read.optionData.autoGearResolution == true)
        { GameManager.instance.debugScript.optionAutoGear = "Auto Gear OFF"; }
        if (read.optionData.fogOfWar == true)
        { GameManager.instance.debugScript.optionFogOfWar = "Fog Of War OFF"; }
        if (read.optionData.connectorTooltips == true)
        { GameManager.instance.debugScript.optionConnectorTooltips = "Conn tooltips OFF"; }
        if (read.optionData.debugData  == true)
        { GameManager.instance.debugScript.optionDebugData = "Debug Data OFF"; }
        if (read.optionData.showRenown == false)
        { GameManager.instance.debugScript.optionRenownUI = "Renown UI ON"; }
        if (read.optionData.showContacts == true)
        { GameManager.instance.debugScript.optionContacts = "Contacts OFF"; }
    }
    #endregion


    #region Read Player Data
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
    #endregion


    #region Read Game Data
    /// <summary>
    /// important Game data
    /// </summary>
    private void ReadGameData()
    {
        //Sidemanager.cs
        GameManager.instance.sideScript.resistanceCurrent = read.gameData.resistanceCurrent;
        GameManager.instance.sideScript.authorityCurrent = read.gameData.authorityCurrent;
        GameManager.instance.sideScript.resistanceOverall = read.gameData.resistanceOverall;
        GameManager.instance.sideScript.authorityOverall = read.gameData.authorityOverall;
        GameManager.instance.sideScript.PlayerSide = read.gameData.playerSide;
        //turnManager.cs
        GameManager.instance.turnScript.Turn = read.gameData.turn;

    }
    #endregion


    #region Read Data Data
    /// <summary>
    /// DataManager.cs data (need to clear collections prior to adding to them)
    /// NOTE: happens BEFORE Datamanager.cs -> ResetLoadGame so make sure any collections here are not in that method otherwise they'll be updated here and then cleared there.
    /// </summary>
    private void ReadDataData()
    {
        #region statistics
        Dictionary<StatType, int> dictOfStatistics = GameManager.instance.dataScript.GetDictOfStatistics();
        if (dictOfStatistics != null)
        {
            
            int count = read.dataData.listOfStatistics.Count;
            Debug.AssertFormat(count == dictOfStatistics.Count, "Mismatch on count (should be list {0}, is dict {1}", count, dictOfStatistics.Count);
            StatType statType;
            for (int i = 0; i < count; i++)
            {
                /*try
                { dictOfStatistics.Add((StatType)i, read.dataData.listOfStatistics[i]); }
                catch (ArgumentException)
                { Debug.LogWarningFormat("Duplicate stat type \"{0}\" in dictOfStatistics", (StatType)i); }*/

                //update dictionary 
                statType = (StatType)i;
                if (dictOfStatistics.ContainsKey(statType) == true)
                { dictOfStatistics[statType] = read.dataData.listOfStatistics[i]; }
                else
                {
                    //StatType not present, add to dictionary
                    try
                    { dictOfStatistics.Add(statType, read.dataData.listOfStatistics[i]); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate StatType \"{0\", entry not added", statType); }
                }
            }
        }
        else { Debug.LogError("Invalid dictOfStatistics (Null)"); }
        #endregion
        #region secrets
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
        #endregion
        #region contacts
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
        //dictOfActorContacts
        Dictionary<int, List<int>> dictOfActorContacts = GameManager.instance.dataScript.GetDictOfActorContacts();
        if (dictOfActorContacts != null)
        {
            //clear dictionary
            dictOfActorContacts.Clear();
            //copy saved data across
            for (int i = 0; i < read.dataData.listOfActorContactsKey.Count; i++)
            {
                List<int> contactList = read.dataData.listOfActorContactsValue[i].myList;
                if (contactList != null)
                {
                    try
                    { dictOfActorContacts.Add(read.dataData.listOfActorContactsKey[i], contactList); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Duplicate actorID {0} in read.dataData.listOfActorContactsKey", i); }
                }
                else { Debug.LogWarningFormat("Invalid contactList (Null) for actorID {0}", read.dataData.listOfActorContactsKey[i]); }
            }
        }
        else { Debug.LogError("Invalid dictOfActorContacts (Null)"); }
        //dictOfNodeContactsResistance
        Dictionary<int, List<int>> dictOfNodeContactsResistance = GameManager.instance.dataScript.GetDictOfNodeContacts(globalResistance);
        if (dictOfNodeContactsResistance != null)
        {
            //clear dictionary
            dictOfNodeContactsResistance.Clear();
            //copy saved data across
            for (int i = 0; i < read.dataData.listOfNodeContactsByResistanceKey.Count; i++)
            {
                List<int> listOfActors = read.dataData.listOfNodeContactsByResistanceValue[i].myList;
                if (listOfActors != null)
                {
                    try
                    { dictOfNodeContactsResistance.Add(read.dataData.listOfNodeContactsByResistanceKey[i], listOfActors); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Duplicate nodeID {0} in read.dataData.listOfNodeContactsByResistanceKey", read.dataData.listOfNodeContactsByResistanceKey[i]); }
                }
                else { Debug.LogWarningFormat("Invalid listOfContacts (Null) for nodeID {0}", read.dataData.listOfNodeContactsByResistanceKey[i]); }
            }
        }
        else { Debug.LogError("Invalid dictOfNodeContactsResistance (Null)"); }
        //dictOfNodeContactsAuthority
        Dictionary<int, List<int>> dictOfNodeContactsAuthority = GameManager.instance.dataScript.GetDictOfNodeContacts(globalAuthority);
        if (dictOfNodeContactsAuthority != null)
        {
            //clear dictionary
            dictOfNodeContactsAuthority.Clear();
            //copy saved data across
            for (int i = 0; i < read.dataData.listOfNodeContactsByAuthorityKey.Count; i++)
            {
                List<int> listOfActors = read.dataData.listOfNodeContactsByAuthorityValue[i].myList;
                if (listOfActors != null)
                {
                    try
                    { dictOfNodeContactsAuthority.Add(read.dataData.listOfNodeContactsByAuthorityKey[i], listOfActors); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Duplicate nodeID {0} in read.dataData.listOfNodeContactsByAuthorityKey", read.dataData.listOfNodeContactsByAuthorityKey[i]); }
                }
                else { Debug.LogWarningFormat("Invalid listOfContacts (Null) for nodeID {0}", read.dataData.listOfNodeContactsByAuthorityKey[i]); }
            }
        }
        else { Debug.LogError("Invalid dictOfNodeContactsAuthority (Null)"); }
        //dictOfContactsByNodeResistance
        Dictionary<int, List<Contact>> dictOfContactsByNodeResistance = GameManager.instance.dataScript.GetDictOfContactsByNodeResistance();
        if (dictOfContactsByNodeResistance != null)
        {
            //clear dictionary
            dictOfContactsByNodeResistance.Clear();
            //copy save data across
            for (int i = 0; i < read.dataData.listOfContactsByNodeResistanceKey.Count; i++)
            {
                List<Contact> listOfContacts = read.dataData.listOfContactsByNodeResistanceValue[i].myList;
                if (listOfContacts != null)
                {
                    try
                    { dictOfContactsByNodeResistance.Add(read.dataData.listOfContactsByNodeResistanceKey[i], listOfContacts); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Duplicate nodeID {0} in read.dataData.listOfContactsByNodeResistanceKey", read.dataData.listOfContactsByNodeResistanceKey[i]); }
                }
                else { Debug.LogWarningFormat("Invalid listOfContacts (Null) for nodeID {0}", read.dataData.listOfContactsByNodeResistanceKey[i]); }
            }
        }
        else { Debug.LogError("Invalid dictOfContactsByNodeResistance (Null)"); }
        #endregion
        #region teams
        //
        // - - - Teams
        //
        Dictionary<int, Team> dictOfTeams = GameManager.instance.dataScript.GetDictOfTeams();
        if (dictOfTeams != null)
        {
            //clear dict
            dictOfTeams.Clear();
            for (int i = 0; i < read.dataData.listOfTeams.Count; i++)
            {
                SaveTeam saveTeam = read.dataData.listOfTeams[i];
                if (saveTeam != null)
                {
                    Team team = new Team();
                    team.teamID = saveTeam.teamID;
                    team.teamName = saveTeam.teamName;
                    team.pool = saveTeam.pool;
                    team.actorSlotID = saveTeam.actorSlotID;
                    team.nodeID = saveTeam.nodeID;
                    team.timer = saveTeam.timer;
                    team.turnDeployed = saveTeam.turnDeployed;
                    //team arc
                    TeamArc arc = GameManager.instance.dataScript.GetTeamArc(saveTeam.arcID);
                    if (arc != null)
                    { team.arc = arc; }
                    else { Debug.LogErrorFormat("Invalid teamArc (Null) for arcID {0}", saveTeam.arcID); }
                    //add to dictionary
                    try
                    { dictOfTeams.Add(team.teamID, team); }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Duplicate team entry for teamID {0}", team.teamID); }
                }
                else { Debug.LogWarningFormat("Invalid SaveTeam (Null) in read.dataData.listOfTeams[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfTeams (Null)"); }
        //arrayOfTeams
        int[,] arrayOfTeams = GameManager.instance.dataScript.GetArrayOfTeams();
        if (arrayOfTeams != null)
        {
            //check bounds match are the same as the ones the save data used
            int index;
            int numOfArcs = GameManager.instance.dataScript.CheckNumOfNodeArcs();
            int numOfInfo = (int)TeamInfo.Count;
            int size0 = arrayOfTeams.GetUpperBound(0) + 1;
            int size1 = arrayOfTeams.GetUpperBound(1) + 1;
            Debug.AssertFormat(numOfArcs == size0, "Mismatch on array size, rank 0 (Arcs) should be {0}, is {1}", numOfArcs, size0);
            Debug.AssertFormat(numOfInfo == size1, "Mismatch on arraySize, rank 1 (TeamInfo) should be {0}, is {1}", numOfInfo, size1);
            //clear array
            Array.Clear(arrayOfTeams, 0, arrayOfTeams.Length);
            //copy saved data into array
            for (int i = 0; i < numOfArcs; i++)
            {
                for (int j = 0; j < numOfInfo; j++)
                {
                    index = (i * numOfInfo) + j;
                    arrayOfTeams[i, j] = read.dataData.listOfArrayOfTeams[index];
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfTeams (Null)"); }
        //list -> Reserves
        List<int> teamPool = read.dataData.listOfTeamPoolReserve;
        if (teamPool != null)
        {
            //clear list and copy across data
            GameManager.instance.dataScript.SetTeamPool(TeamPool.Reserve, teamPool);
        }
        else { Debug.LogError("Invalid teampPoolReserve list (Null)"); }
        //list -> OnMap
        teamPool = read.dataData.listOfTeamPoolOnMap;
        if (teamPool != null)
        {
            //clear list and copy across data
            GameManager.instance.dataScript.SetTeamPool(TeamPool.OnMap, teamPool);
        }
        else { Debug.LogError("Invalid teampPoolReserve list (Null)"); }
        //list -> InTransit
        teamPool = read.dataData.listOfTeamPoolInTransit;
        if (teamPool != null)
        {
            //clear list and copy across data
            GameManager.instance.dataScript.SetTeamPool(TeamPool.InTransit, teamPool);
        }
        else { Debug.LogError("Invalid teampPoolReserve list (Null)"); }
        //team counter
        GameManager.instance.teamScript.teamCounter = read.dataData.teamCounter;
        #endregion
        #region AI
        //array -> AI Resources
        int[] arrayOfAIResources = GameManager.instance.dataScript.GetArrayOfAIResources();
        if (arrayOfAIResources != null)
        {
            int lengthOfArray = arrayOfAIResources.Length;
            //clear array & copy data across
            Array.Clear(arrayOfAIResources, 0, lengthOfArray);
            Debug.AssertFormat(lengthOfArray == read.dataData.listOfArrayOfAIResources.Count, "Mismatch on size, array {0}, list {1}", lengthOfArray, read.dataData.listOfArrayOfAIResources.Count);
            for (int i = 0;  i < lengthOfArray;  i++)
            { arrayOfAIResources[i] = read.dataData.listOfArrayOfAIResources[i]; }
        }
        else { Debug.LogError("Invalid arrayOfAIResources (Null)"); }
        //queue -> recentNodes
        Queue<AITracker> queueOfRecentNodes = GameManager.instance.dataScript.GetRecentNodesQueue();
        if (queueOfRecentNodes != null)
        {
            //clear queue and copy data across
            queueOfRecentNodes.Clear();
            for (int i = 0; i < read.dataData.listOfRecentNodes.Count; i++)
            { queueOfRecentNodes.Enqueue(read.dataData.listOfRecentNodes[i]); }
        }
        else { Debug.LogError("Invalid queueOfRecentNodes (Null)"); }
        //queue -> recentConnections
        Queue<AITracker> queueOfRecentConnections = GameManager.instance.dataScript.GetRecentConnectionsQueue();
        if (queueOfRecentConnections != null)
        {
            //clear queue and copy data across
            queueOfRecentConnections.Clear();
            for (int i = 0; i < read.dataData.listOfRecentConnections.Count; i++)
            { queueOfRecentConnections.Enqueue(read.dataData.listOfRecentConnections[i]); }
        }
        else { Debug.LogError("Invalid queueOfRecentConnections (Null)"); }
        #endregion
        #region messages
        //archive
        Dictionary<int, Message> dictOfArchiveMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Archive);
        if (dictOfArchiveMessages != null)
        {
            //clear dict and copy across data
            dictOfArchiveMessages.Clear();
            int countKey = read.dataData.listOfArchiveMessagesKey.Count;
            int countValue = read.dataData.listOfArchiveMessagesValue.Count;
            Debug.AssertFormat(countKey == countValue, "Mismatch on count for Archive Message Lists ->  Keys {0} and Values {1}", countKey, countValue);
            for (int i = 0; i < countKey; i++)
            {
                Message message = read.dataData.listOfArchiveMessagesValue[i];
                if (message != null)
                {
                    //add to dictionary
                    try { dictOfArchiveMessages.Add(read.dataData.listOfArchiveMessagesKey[i], message); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate message key exists for messageID {0}", read.dataData.listOfArchiveMessagesKey[i]); }
                }
                else { Debug.LogWarningFormat("Invalid message (Null) for messageID {0}", read.dataData.listOfArchiveMessagesKey[i]); }
            }
        }
        else { Debug.LogError("Invalid dictOfArchiveMessages (Null)"); }
        //pending
        Dictionary<int, Message> dictOfPendingMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Pending);
        if (dictOfPendingMessages != null)
        {
            //clear dict and copy across data
            dictOfPendingMessages.Clear();
            int countKey = read.dataData.listOfPendingMessagesKey.Count;
            int countValue = read.dataData.listOfPendingMessagesValue.Count;
            Debug.AssertFormat(countKey == countValue, "Mismatch on count for Pending Message Lists ->  Keys {0} and Values {1}", countKey, countValue);
            for (int i = 0; i < countKey; i++)
            {
                Message message = read.dataData.listOfPendingMessagesValue[i];
                if (message != null)
                {
                    //add to dictionary
                    try { dictOfPendingMessages.Add(read.dataData.listOfPendingMessagesKey[i], message); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate message key exists for messageID {0}", read.dataData.listOfPendingMessagesKey[i]); }
                }
                else { Debug.LogWarningFormat("Invalid message (Null) for messageID {0}", read.dataData.listOfPendingMessagesKey[i]); }
            }
        }
        else { Debug.LogError("Invalid dictOfPendingMessages (Null)"); }
        //current
        Dictionary<int, Message> dictOfCurrentMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.Current);
        if (dictOfCurrentMessages != null)
        {
            //clear dict and copy across data
            dictOfCurrentMessages.Clear();
            int countKey = read.dataData.listOfCurrentMessagesKey.Count;
            int countValue = read.dataData.listOfCurrentMessagesValue.Count;
            Debug.AssertFormat(countKey == countValue, "Mismatch on count for Current Message Lists ->  Keys {0} and Values {1}", countKey, countValue);
            for (int i = 0; i < countKey; i++)
            {
                Message message = read.dataData.listOfCurrentMessagesValue[i];
                if (message != null)
                {
                    //add to dictionary
                    try { dictOfCurrentMessages.Add(read.dataData.listOfCurrentMessagesKey[i], message); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate message key exists for messageID {0}", read.dataData.listOfCurrentMessagesKey[i]); }
                }
                else { Debug.LogWarningFormat("Invalid message (Null) for messageID {0}", read.dataData.listOfCurrentMessagesKey[i]); }
            }
        }
        else { Debug.LogError("Invalid dictOfCurrentMessages (Null)"); }
        //ai
        Dictionary<int, Message> dictOfAIMessages = GameManager.instance.dataScript.GetMessageDict(MessageCategory.AI);
        if (dictOfAIMessages != null)
        {
            //clear dict and copy across data
            dictOfAIMessages.Clear();
            int countKey = read.dataData.listOfAIMessagesKey.Count;
            int countValue = read.dataData.listOfAIMessagesValue.Count;
            Debug.AssertFormat(countKey == countValue, "Mismatch on count for AI Message Lists ->  Keys {0} and Values {1}", countKey, countValue);
            for (int i = 0; i < countKey; i++)
            {
                Message message = read.dataData.listOfAIMessagesValue[i];
                if (message != null)
                {
                    //add to dictionary
                    try { dictOfAIMessages.Add(read.dataData.listOfAIMessagesKey[i], message); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate message key exists for messageID {0}", read.dataData.listOfAIMessagesKey[i]); }
                }
                else { Debug.LogWarningFormat("Invalid message (Null) for messageID {0}", read.dataData.listOfAIMessagesKey[i]); }
            }
        }
        else { Debug.LogError("Invalid dictOfAIMessages (Null)"); }
        //message counter
        GameManager.instance.messageScript.messageCounter = read.dataData.messageCounter;
        #endregion
    }
    #endregion


    #region Read Actor Data
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
                        index = (i * actorNum) + j;
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
                    index = (i * actorNum) + j;
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
    #endregion


    #region Read Node Data
    /// <summary>
    /// NodeManager.cs data to game
    /// </summary>
    private void ReadNodeData()
    {
        //NodeManager.cs dynamic data
        GameManager.instance.nodeScript.crisisPolicyModifier = read.nodeData.crisisPolicyModifier;
        GameManager.instance.nodeScript.nodeCounter = read.nodeData.nodeCounter;
        GameManager.instance.nodeScript.connCounter = read.nodeData.connCounter;
        GameManager.instance.nodeScript.nodeHighlight = read.nodeData.nodeHighlight;
        GameManager.instance.nodeScript.nodePlayer = read.nodeData.nodePlayer;
        GameManager.instance.nodeScript.nodeNemesis = read.nodeData.nodeNemesis;
        GameManager.instance.nodeScript.nodeCaptured = read.nodeData.nodeCaptured;
        //update node data
        for (int i = 0; i < read.nodeData.listOfNodes.Count; i++)
        {
            SaveNode saveNode = read.nodeData.listOfNodes[i];
            if (saveNode != null)
            {
                //find equivalent inGame node
                Node node = GameManager.instance.dataScript.GetNode(saveNode.nodeID);
                if (node != null)
                {
                    //copy save data over to node
                    node.Security = saveNode.security;
                    node.Stability = saveNode.stability;
                    node.Support = saveNode.support;
                    node.isTracerKnown = saveNode.isTracerKnown;
                    node.isSpiderKnown = saveNode.isSpiderKnown;
                    node.isContactKnown = saveNode.isContactKnown;
                    node.isTeamKnown = saveNode.isTeamKnown;
                    node.isTargetKnown = saveNode.isTargetKnown;
                    node.isTracer = saveNode.isTracer;
                    node.isSpider = saveNode.isSpider;
                    node.isContactResistance = saveNode.isContactResistance;
                    node.isContactAuthority = saveNode.isContactAuthority;
                    node.isPreferredAuthority = saveNode.isPreferredAuthority;
                    node.isPreferredResistance = saveNode.isPreferredResistance;
                    node.isCentreNode = saveNode.isCentreNode;
                    node.isLoiterNode = saveNode.isLoiterNode;
                    node.isConnectedNode = saveNode.isConnectedNode;
                    node.isChokepointNode = saveNode.isChokepointNode;
                    node.targetID = saveNode.targetID;
                    node.spiderTimer = saveNode.spiderTimer;
                    node.tracerTimer = saveNode.tracerTimer;
                    node.activityCount = saveNode.activityCount;
                    node.activityTime = saveNode.activityTime;
                    node.isStabilityTeam = saveNode.isStabilityTeam;
                    node.isSecurityTeam = saveNode.isSecurityTeam;
                    node.isSupportTeam = saveNode.isSupportTeam;
                    node.isProbeTeam = saveNode.isProbeTeam;
                    node.isSpiderTeam = saveNode.isSpiderTeam;
                    node.isDamageTeam = saveNode.isDamageTeam;
                    node.isErasureTeam = saveNode.isErasureTeam;
                    node.crisisTimer = saveNode.crisisTimer;
                    node.waitTimer = saveNode.waitTimer;
                    node.crisis = saveNode.crisis;
                    //cure
                    if (saveNode.cureID == -1)
                    { node.cure = null; }
                    else
                    {
                        //get cure from dictionary
                        Cure cure = GameManager.instance.dataScript.GetCure(saveNode.cureID);
                        if (cure != null)
                        { node.cure = cure; }
                        else { Debug.LogWarningFormat("Invalid cure for cureID {0}", saveNode.cureID); }
                    }
                    node.loiter = saveNode.loiter;
                    //teams
                    int count = saveNode.listOfTeams.Count;
                    node.GetListOfTeams().Clear();
                    if (count > 0)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            Team team = GameManager.instance.dataScript.GetTeam(saveNode.listOfTeams[j]);
                            if (team != null)
                            { node.LoadAddTeam(team); }
                            else { Debug.LogWarningFormat("Invalid team (Null) for listOfTeams[{0}]", j); }
                        }
                    }
                    //ongoing effects
                    count = saveNode.listOfOngoingEffects.Count;
                    node.GetListOfOngoingEffects().Clear();
                    if (count > 0)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            EffectDataOngoing effectOngoing = GameManager.instance.dataScript.GetOngoingEffect(saveNode.listOfOngoingEffects[j]);
                            if (effectOngoing != null)
                            { node.LoadAddOngoingEffect(effectOngoing); }
                            else { Debug.LogWarningFormat("Invalid effectOngoing (Null) for listOfOngoingEffects[{0}]", j); }
                        }
                    }
                 }
                else { Debug.LogWarningFormat("Invalid node (Null) for saveNode.nodeID {0}", saveNode.nodeID); }
            }
            else { Debug.LogWarningFormat("Invalid saveNode in read.nodeData.listOfNodes[{0}]", i); }
        }
        //
        // - - - Crisis Nodes
        //
        List<Node> listOfCrisisNodes = GameManager.instance.dataScript.GetListOfCrisisNodes();
        if (listOfCrisisNodes != null)
        {
            //clear list
            listOfCrisisNodes.Clear();
            int count = read.nodeData.listOfCrisisNodes.Count;
            if (count > 0)
            {
                //repopulate list with save data
                for (int i = 0; i < count; i++)
                {
                    Node node = GameManager.instance.dataScript.GetNode(read.nodeData.listOfCrisisNodes[i]);
                    if (node != null)
                    { listOfCrisisNodes.Add(node); }
                    else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", read.nodeData.listOfCrisisNodes[i]); }
                }
                //activate smoke
                GameManager.instance.nodeScript.ProcessLoadNodeCrisis();
            }
        }
        else { Debug.LogError("Invalid listOfCrisisNodes (Null)"); }
        //
        // - - - Cure Nodes
        //
        List<Node> listOfCureNodes = GameManager.instance.dataScript.GetListOfCureNodes();
        if (listOfCureNodes != null)
        {
            //clear list
            listOfCureNodes.Clear();
            int count = read.nodeData.listOfCureNodes.Count;
            if (count > 0)
            {
                //repopulate list with save data
                for (int i = 0; i < count; i++)
                {
                    Node node = GameManager.instance.dataScript.GetNode(read.nodeData.listOfCureNodes[i]);
                    if (node != null)
                    { listOfCureNodes.Add(node); }
                    else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", read.nodeData.listOfCureNodes[i]); }
                }
            }
        }
        else { Debug.LogError("Invalid listOfCureNodes (Null)"); }
    }
    #endregion


    #region ReadGearData
    /// <summary>
    /// GearManager.cs & gear.cs dynamic data
    /// </summary>
    private void ReadGearData()
    {
        Dictionary<int, Gear> dictOfGear = GameManager.instance.dataScript.GetDictOfGear();
        if (dictOfGear != null)
        {
            for (int i = 0; i < read.gearData.listOfGear.Count; i++)
            {
                SaveGear saveGear = read.gearData.listOfGear[i];
                if (saveGear != null)
                {
                    //find in dictionary
                    if (dictOfGear.ContainsKey(saveGear.gearID) == true)
                    {
                        Gear gear = dictOfGear[saveGear.gearID];
                        if (gear != null)
                        {
                            //copy over dynamic data
                            gear.timesUsed = saveGear.timesUsed;
                            gear.isCompromised = saveGear.isCompromised;
                            gear.reasonUsed = saveGear.reasonUsed;
                            gear.chanceOfCompromise = saveGear.chanceOfCompromise;
                            gear.statTurnObtained = saveGear.statTurnObtained;
                            gear.statTurnLost = saveGear.statTurnLost;
                            gear.statTimesUsed = saveGear.statTimesUsed;
                            gear.statTimesGiven = saveGear.statTimesGiven;
                            gear.statTimesCompromised = saveGear.statTimesCompromised;
                            gear.statTimesSaved = saveGear.statTimesSaved;
                            gear.statRenownSpent = saveGear.statRenownSpent;
                        }
                        else { Debug.LogWarningFormat("Invalid gear (Null) in dict for gearID {0}", saveGear.gearID); }
                    }
                    else { Debug.LogWarningFormat("Gear not found in dictionary for gearID {0}", saveGear.gearID); }
                }
                else { Debug.LogWarningFormat("Invalid saveGear (Null) for read.gearData.listOfGear[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfGear (Null)"); }
        //list -> Common gear
        GearRarity rarity = GameManager.instance.gearScript.gearCommon;
        List<int> tempList = GameManager.instance.dataScript.GetListOfGear(rarity);
        if (tempList != null)
        { GameManager.instance.dataScript.SetGearList(read.gearData.listOfCommonGear, rarity); }
        else { Debug.LogError("Invalid listOfCommonGear (Null)"); }
        //list -> Rare gear
         rarity = GameManager.instance.gearScript.gearRare;
        tempList = GameManager.instance.dataScript.GetListOfGear(rarity);
        if (tempList != null)
        { GameManager.instance.dataScript.SetGearList(read.gearData.listOfRareGear, rarity); }
        else { Debug.LogError("Invalid listOfRareGear (Null)"); }
        //list -> Unique gear
        rarity = GameManager.instance.gearScript.gearUnique;
        tempList = GameManager.instance.dataScript.GetListOfGear(rarity);
        if (tempList != null)
        { GameManager.instance.dataScript.SetGearList(read.gearData.listOfUniqueGear, rarity); }
        else { Debug.LogError("Invalid listOfUniqueGear (Null)"); }
        //list -> Current gear
        tempList = GameManager.instance.dataScript.GetListOfCurrentGear();
        if (tempList != null)
        { GameManager.instance.dataScript.SetListOfGearCurrent(read.gearData.listOfCurrentGear); }
        else { Debug.LogError("Invalid listOfCurrentGear (Null)"); }
        //list -> Lost gear
        tempList = GameManager.instance.dataScript.GetListOfLostGear();
        if (tempList != null)
        { GameManager.instance.dataScript.SetListOfGearLost(read.gearData.listOfLostGear); }
        else { Debug.LogError("Invalid listOfLostGear (Null)"); }
        //GearManager.cs fields
        GameManager.instance.gearScript.SetGearSaveCurrentCost(read.gearData.gearSaveCurrentCost);
        GameManager.instance.gearScript.SetListOfCompromisedGear(read.gearData.listOfCompromisedGear);
    }
    #endregion


    #region Read Nemesis Data
    /// <summary>
    /// NemesisManager.cs data to game
    /// </summary>
    private void ReadNemesisData()
    {
        if (read.nemesisData.saveData != null)
        { GameManager.instance.nemesisScript.ReadSaveData(read.nemesisData.saveData); }
        else { Debug.LogError("Invalid read.nemesisData.saveData (Null)"); }
    }
    #endregion


    #region Read AI Data
    /// <summary>
    /// read AI/Rebel Manager.cs
    /// </summary>
    private void ReadAIData()
    {
        //list -> tasks final
        GameManager.instance.aiScript.SetListOfTasksFinal(read.aiData.listOfTasksFinal);
        //list -> player effects
        GameManager.instance.aiScript.SetListOfPlayerEffects(read.aiData.listOfPlayerEffects);
        //list -> player effects description
        GameManager.instance.aiScript.SetListOfPlayerEffectDescriptors(read.aiData.listOfPlayerEffectDescriptors);
        //
        // - - - update displays -> load game could have been called from a hot key or the main menu
        //
        GameState controlState = GameManager.instance.controlScript.GetExistingGameState();
        //resistance player only
        if (controlState == GameState.PlayGame && GameManager.instance.sideScript.PlayerSide.level == 2)
        { 
            GameManager.instance.aiScript.UpdateTaskDisplayData();
            GameManager.instance.aiScript.UpdateSideTabData();
            GameManager.instance.aiScript.UpdateBottomTabData();
        }
        //AIRebelManager.cs -> private fields
        GameManager.instance.aiRebelScript.ReadSaveData(read.aiData.saveRebel);
        //AIRebelManager -> Nemesis Reports
        GameManager.instance.aiRebelScript.SetListOfNemesisReports(read.aiData.listOfNemesisReports);
        //AIRebelManager -> Erasure Reports
        GameManager.instance.aiRebelScript.SetListOfErasureReports(read.aiData.listOfErasureReports);
    }
    #endregion

    //
    // - - -  Validate - - -
    //

    #region Validate Player Data
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
    #endregion


    #region Validate Actor Data
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
    #endregion

    //
    // - - - GUI - - -
    //

    /// <summary>
    /// if loaded in player mode, update all relevent GUI elements (others may be updated by class specific methods above)
    /// </summary>
    private void UpdateGUI()
    {
        //Top Widget UI
        TopWidgetData widget = new TopWidgetData();
        widget.side = read.gameData.playerSide;
        widget.turn = read.gameData.turn;
        widget.actionPoints = read.gameData.actionPoints;
        widget.cityLoyalty = read.gameData.cityLoyalty;
        //faction support depends on side
        switch (read.gameData.playerSide.level)
        {
            case 1: widget.factionSupport = read.gameData.factionSupportAuthority; break;
            case 2: widget.factionSupport = read.gameData.factionSupportResistance; break;
            default: Debug.LogError("Unrecognised {0}", read.gameData.playerSide); break;
        }
        widget.isSecurityFlash = read.gameData.isSecurityFlash;

        //objectives -> TO DO

        //Update top widget UI
        GameManager.instance.widgetTopScript.LoadSavedData(widget);
    }

    //new methods above here
}
