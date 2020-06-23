using gameAPI;
using modalAPI;
using packageAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;


/// <summary>
/// Handles all Save / Load functionality
/// </summary>
public class FileManager : MonoBehaviour
{

    private static readonly string SAVE_FILE = "savefile.json";
    private static readonly string AUTO_FILE = "autoSaveFile.json";
    private Save write;
    private Save read;
    private string filenamePlayer;
    private string filenameAuto;
    private string jsonWrite;
    private string jsonRead;
    private byte[] soupWrite;               //encryption out
    private byte[] soupRead;                //encryption in

    private string codeKey;

    //fast access
    GlobalSide globalAuthority;
    GlobalSide globalResistance;
    float alphaActive = -1;
    float alphaInactive = -1;
    Sprite defaultSprite;

    #region Initialise
    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise(GameState state)
    {
        filenamePlayer = Path.Combine(Application.persistentDataPath, SAVE_FILE);
        filenameAuto = Path.Combine(Application.persistentDataPath, AUTO_FILE);
        codeKey = "#kJ83DAl50$*@.<__'][90{4#dDA'a?~";                         //needs to be 32 characters long exactly
        //fast access
        globalAuthority = GameManager.i.globalScript.sideAuthority;
        globalResistance = GameManager.i.globalScript.sideResistance;
        alphaActive = GameManager.i.guiScript.alphaActive;
        alphaInactive = GameManager.i.guiScript.alphaInactive;
        defaultSprite = GameManager.i.guiScript.alertInformationSprite;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(alphaActive > -1, "Invalid alphaActive (-1)");
        Debug.Assert(alphaInactive > -1, "Invalid alphaInactive (-1)");
        Debug.Assert(defaultSprite != null, "Invalid defaultSprite (Null)");
    }
    #endregion

    //
    // - - - Master Methods - - -
    //

    #region Write Game Data
    /// <summary>
    /// copy inGame data to saveData
    /// </summary>
    public void WriteSaveData(LoadGameState loadGameState)
    {
        write = new Save();
        //Sequentially write data
        WriteGameStatus(loadGameState);
        WriteSeedData();
        WriteDataData();
        WriteCampaignData();
        WriteOptionData();
        WritePlayerData();
        WriteNemesisData();
        WriteGameData();
        WriteScenarioData();
        WriteActorData();
        WriteNodeData();
        WriteConnectionData();
        WriteGearData();
        WriteContactData();
        WriteAIData();
        WriteTopicData();
        WriteTargetData();
        WriteStatisticsData();
        WriteGUIData();
        //only do so if gameState.MetaGame
        if (write.gameStatus.gameState == GameState.MetaGame)
        { WriteMetaData(); }
    }
    #endregion


    #region SaveGame
    /// <summary>
    /// Save game method
    /// </summary>
    public void SaveGame(bool isAutoSave = false)
    {
        string filename;
        if (isAutoSave == true) { filename = filenameAuto; }
        else { filename = filenamePlayer; }
        if (write != null)
        {
            if (string.IsNullOrEmpty(filename) == false)
            {
                //convert to Json (NOTE: second, optional, parameter gives pretty output for debugging purposes)
                jsonWrite = JsonUtility.ToJson(write, true);

                //file present? If so delete
                if (File.Exists(filename) == true)
                {
                    try { File.Delete(filename); }
                    catch (Exception e) { Debug.LogErrorFormat("Failed to DELETE FILE, error \"{0}\"", e.Message); }
                }

                if (GameManager.i.isEncrypted == false)
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
                    soupWrite = crypto.Encrypt(jsonWrite, codeKey);
                    try { File.WriteAllBytes(filename, soupWrite); }
                    catch (Exception e) { Debug.LogErrorFormat("Failed to write BYTES TO FILE, error \"{0}\"", e.Message); }
                    Debug.LogFormat("[Fil] FileManager.cs -> SaveGame: Encrypted GAME SAVED to \"{0}\"{1}", filename, "\n");
                }
            }
            else { Debug.LogError("Invalid fileName (Null or Empty)"); }
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
        string filename;
        if (GameManager.i.isLoadAutoSave == true) { filename = filenameAuto; }
        else { filename = filenamePlayer; }
        if (string.IsNullOrEmpty(filename) == false)
        {
            if (File.Exists(filename) == true)
            {
                if (GameManager.i.isEncrypted == false)
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
                    jsonRead = crypto.Decrypt(soupRead, codeKey);
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
        }
        Debug.LogError("Invalid filename (Null or Empty)");
        return false;
    }
    #endregion


    #region Load Save Data
    /// <summary>
    /// Load data from Save file back into game
    /// Returns LoadGameState to enable ControlManager.cs -> ProcessLoadGame to restore game to the correct place, eg. normal gameState.playGame or a specific restore point in the MetaGame sequence
    /// </summary>
    public LoadGameState LoadSaveData()
    {
        LoadGameState loadGameState = new LoadGameState();
        if (read != null)
        {
            GlobalSide playerSide = GameManager.i.dataScript.GetGlobalSide(read.gameData.playerSide);
            if (playerSide != null)
            {
                loadGameState.gameState = read.gameStatus.gameState;
                //side (player) at start
                ReadOptionData();
                ReadDataData();
                ReadCampaignData();
                ReadGameData(playerSide);
                ReadSeedData();
                //set up level based on loaded current scenario seed
                GameManager.i.InitialiseLoadGame(playerSide.level);
                ReadScenarioData();
                ReadNodeData();
                ReadNpcData();
                ReadConnectionData();
                ReadNemesisData();
                ReadGearData();
                ReadAIData();
                ReadTopicData();
                ReadActorData();
                ValidateActorData();
                ReadPlayerData();
                ValidatePlayerData();
                ReadContactData();
                ReadTargetData();
                ReadStatisticsData();
                UpdateGUI(playerSide);
                //only read metaGame back in if it's a metaGame save
                if (read.gameStatus.gameState == GameState.MetaGame)
                {
                    ReadMetaGameData();
                    loadGameState.restorePoint = read.gameStatus.restorePoint;
                }
                Debug.LogFormat("[Fil] FileManager.cs -> LoadSaveData: Saved Game Data has been LOADED (gameState {0}, restorePoint {1}){2}", read.gameStatus.gameState, read.gameStatus.restorePoint, "\n");
            }
            else { Debug.LogError("Invalid playerSide (Null)"); }
        }
        return loadGameState;
    }
    #endregion

    //
    // - - - Write - - -
    //

    #region WriteGameStatus
    /// <summary>
    /// records game status in order to know whether a normal save/load operation (gameState.playGame) or a MetaGame one (gameState.MetaGame and restorePoint required)
    /// </summary>
    private void WriteGameStatus(LoadGameState loadGameState)
    {
        write.gameStatus.gameState = loadGameState.gameState;
        write.gameStatus.restorePoint = loadGameState.restorePoint;
        write.gameStatus.turn = GameManager.i.turnScript.Turn;
        DateTime date1 = DateTime.Now;
        write.gameStatus.time = string.Format("{0}", date1.ToString("f", CultureInfo.CreateSpecificCulture("en-AU")));
    }
    #endregion


    #region WriteSeedData
    /// <summary>
    /// game seeds used within
    /// </summary>
    private void WriteSeedData()
    {
        write.seedData.levelSeed = GameManager.i.levelScript.seed;
        write.seedData.devSeed = GameManager.i.seedDev;
    }
    #endregion


    #region Write Campaign Data
    /// <summary>
    /// CampaignManager.cs data write to file
    /// </summary>
    private void WriteCampaignData()
    {
        write.campaignData.campaignName = GameManager.i.campaignScript.campaign.name;
        write.campaignData.scenarioIndex = GameManager.i.campaignScript.GetScenarioIndex();
        write.campaignData.arrayOfStoryStatus = GameManager.i.campaignScript.GetArrayOfStoryStatus();
        write.campaignData.commendations = GameManager.i.campaignScript.GetCommendations();
        write.campaignData.blackMarks = GameManager.i.campaignScript.GetBlackmarks();
        write.campaignData.investigationBlackMarks = GameManager.i.campaignScript.GetInvestigationBlackmarks();
        //Npc
        Npc npc = GameManager.i.campaignScript.scenario.missionResistance.npc;
        if (npc != null)
        {
            write.campaignData.isNpc = true;
            write.campaignData.npc = new SaveNpc();
            write.campaignData.npc.status = npc.status;
            write.campaignData.npc.timerTurns = npc.timerTurns;
            write.campaignData.npc.daysActive = npc.daysActive;
            //Nodes -> assign -1 if null (which is valid data)
            if (npc.currentStartNode != null)
            { write.campaignData.npc.startNodeID = npc.currentStartNode.nodeID; }
            else { write.campaignData.npc.startNodeID = -1; }
            if (npc.currentEndNode != null)
            { write.campaignData.npc.endNodeID = npc.currentEndNode.nodeID; }
            else { write.campaignData.npc.endNodeID = -1; }
            if (npc.currentNode != null)
            { write.campaignData.npc.currentNodeID = npc.currentNode.nodeID; }
            else { write.campaignData.npc.currentNodeID = -1; }
        }
        else
        {
            //Note: that this creates a valid SaveNpc JSON object but with zero for all fields. isNpc set to false to handle this situation manually (workaround)
            write.campaignData.npc = null;
            write.campaignData.isNpc = false;
        }
    }
    #endregion


    #region Write GUI Data
    /// <summary>
    /// GUI related data write to file
    /// </summary>
    private void WriteGUIData()
    {
        //TopBarUI
        write.guiData.commendationData = GameManager.i.topBarScript.commendationData;
        write.guiData.blackmarkData = GameManager.i.topBarScript.blackmarkData;
        write.guiData.investigationData = GameManager.i.topBarScript.investigationData;
        write.guiData.innocenceData = GameManager.i.playerScript.Innocence;
        write.guiData.unhappyData = GameManager.i.topBarScript.unhappyData;
        write.guiData.conflictData = GameManager.i.topBarScript.conflictData;
        write.guiData.blackmailData = GameManager.i.topBarScript.blackmailData;
        write.guiData.doomData = GameManager.i.topBarScript.doomData;
    }
    #endregion


    #region Write Option Data
    /// <summary>
    /// OptionManager.cs data write to file
    /// </summary>
    private void WriteOptionData()
    {
        write.optionData.autoGearResolution = GameManager.i.optionScript.autoGearResolution;
        write.optionData.fogOfWar = GameManager.i.optionScript.fogOfWar;
        write.optionData.debugData = GameManager.i.optionScript.debugData;
        write.optionData.noAI = GameManager.i.optionScript.noAI;
        write.optionData.showContacts = GameManager.i.optionScript.showContacts;
        write.optionData.showRenown = GameManager.i.optionScript.showRenown;
        write.optionData.connectorTooltips = GameManager.i.optionScript.connectorTooltips;
        write.optionData.colourScheme = GameManager.i.optionScript.ColourOption;
    }
    #endregion


    #region Write Nemesis Data
    /// <summary>
    /// NemesisManager.cs data write to file
    /// </summary>
    private void WriteNemesisData()
    {
        write.nemesisData.saveData = GameManager.i.nemesisScript.WriteSaveData();
        Debug.Assert(write.nemesisData.saveData != null, "Invalid Nemesis saveData (Null)");
    }
    #endregion


    #region Write Player data
    /// <summary>
    /// PlayerManager.cs Data write to file
    /// </summary>
    private void WritePlayerData()
    {
        write.playerData.renown = GameManager.i.playerScript.Renown;
        write.playerData.status = GameManager.i.playerScript.status;
        write.playerData.sex = GameManager.i.playerScript.sex;
        write.playerData.Invisibility = GameManager.i.playerScript.Invisibility;
        write.playerData.Innocence = GameManager.i.playerScript.Innocence;
        write.playerData.mood = GameManager.i.playerScript.GetMood();
        write.playerData.tooltipStatus = GameManager.i.playerScript.tooltipStatus;
        write.playerData.inactiveStatus = GameManager.i.playerScript.inactiveStatus;
        write.playerData.listOfGear = GameManager.i.playerScript.GetListOfGear();
        write.playerData.isBreakdown = GameManager.i.playerScript.isBreakdown;
        write.playerData.isEndOfTurnGearCheck = GameManager.i.playerScript.isEndOfTurnGearCheck;
        write.playerData.isLieLowFirstturn = GameManager.i.playerScript.isLieLowFirstturn;
        write.playerData.isAddicted = GameManager.i.playerScript.isAddicted;
        write.playerData.isStressLeave = GameManager.i.playerScript.isStressLeave;
        write.playerData.isStressed = GameManager.i.playerScript.isStressed;
        write.playerData.isSpecialMoveGear = GameManager.i.playerScript.isSpecialMoveGear;
        write.playerData.numOfSuperStress = GameManager.i.playerScript.numOfSuperStress;
        write.playerData.stressImmunityCurrent = GameManager.i.playerScript.stressImmunityCurrent;
        write.playerData.stressImmunityStart = GameManager.i.playerScript.stressImmunityStart;
        write.playerData.addictedTally = GameManager.i.playerScript.addictedTally;
        write.playerData.arrayOfCaptureTools = GameManager.i.playerScript.GetArrayOfCaptureTools();
        Personality personality = GameManager.i.playerScript.GetPersonality();
        if (personality != null)
        {
            write.playerData.listOfPersonalityFactors = personality.GetFactors().ToList();
            write.playerData.listOfDescriptors.AddRange(personality.GetListOfDescriptors());
            write.playerData.profile = personality.GetProfile();
            write.playerData.profileDescriptor = personality.GetProfileDescriptor();
            write.playerData.profileExplanation = personality.GetProfileExplanation();
        }
        else { Debug.LogWarning("Invalid Player personality (Null)"); }
        //secrets
        List<Secret> listOfSecrets = GameManager.i.playerScript.GetListOfSecrets();
        foreach (Secret secret in listOfSecrets)
        {
            if (secret != null)
            { write.playerData.listOfSecrets.Add(secret.name); }
            else { Debug.LogWarning("Invalid secret (Null)"); }
        }
        //investigations
        List<Investigation> listOfInvestigations = GameManager.i.playerScript.GetListOfInvestigations();
        if (listOfInvestigations != null)
        { write.playerData.listOfInvestigations.AddRange(listOfInvestigations); }
        else { Debug.LogError("Invalid listOfInvestigations (Null)"); }
        //mood history
        List<HistoryMood> listOfHistory = GameManager.i.playerScript.GetListOfMoodHistory();
        foreach (HistoryMood history in listOfHistory)
        {
            if (history != null)
            { write.playerData.listOfMoodHistory.Add(history); }
            else { Debug.LogWarning("Invalid historyMood (Null)"); }
        }
        //conditions
        List<Condition> listOfConditions = GameManager.i.playerScript.GetListOfConditions(globalResistance);
        foreach (Condition condition in listOfConditions)
        {
            if (condition != null)
            { write.playerData.listOfConditionsResistance.Add(condition.tag); }
            else { Debug.LogWarning("Invalid Resistance condition (Null)"); }
        }
        listOfConditions = GameManager.i.playerScript.GetListOfConditions(globalAuthority);
        foreach (Condition condition in listOfConditions)
        {
            if (condition != null)
            { write.playerData.listOfConditionsAuthority.Add(condition.tag); }
            else { Debug.LogWarning("Invalid Authority condition (Null)"); }
        }
        //node Actions
        write.playerData.listOfNodeActions = GameManager.i.playerScript.GetListOfNodeActions();
    }
    #endregion


    #region Write MetaData
    /// <summary>
    /// MetaGame data (transition and metaGame UI data)
    /// </summary>
    private void WriteMetaData()
    {
        //MetaGameOptions
        write.metaGameData.metaGameOptions = GameManager.i.metaScript.GetMetaOptions();

        #region TransitionInfoData
        TransitionInfoData info = GameManager.i.metaScript.GetTransitionInfoData();
        if (info != null)
        {
            //End Level
            write.metaGameData.objectiveStatus = info.objectiveStatus;
            write.metaGameData.listOfEndLevelData = info.listOfEndLevelData;
            //HQ status
            write.metaGameData.listOfHqRenown = info.listOfHqRenown;
            write.metaGameData.listOfHqTitles = info.listOfHqTitles;
            write.metaGameData.listOfHqEvents = info.listOfHqEvents;
            write.metaGameData.listOfHqTooltips = info.listOfHqTooltips;
            write.metaGameData.listOfWorkerRenown = info.listOfWorkerRenown;
            write.metaGameData.listOfWorkerArcs = info.listOfWorkerArcs;
            write.metaGameData.listOfWorkerTooltips = info.listOfWorkerTooltips;
            //sprites -> hq
            for (int i = 0; i < info.listOfHqSprites.Count; i++)
            {
                Sprite sprite = info.listOfHqSprites[i];
                if (sprite != null)
                { write.metaGameData.listOfHqSprites.Add(sprite.name); }
                else { Debug.LogErrorFormat("Invalid sprite (Null) for listOfHqSprites[{0}]", i); }
            }
            //sprites -> worker
            for (int i = 0; i < info.listOfWorkerSprites.Count; i++)
            {
                Sprite sprite = info.listOfWorkerSprites[i];
                if (sprite != null)
                { write.metaGameData.listOfWorkerSprites.Add(sprite.name); }
                else { Debug.LogErrorFormat("Invalid sprite (Null) for listOfWorkerSprites[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid transitionInfoData (Null)"); }
        #endregion

        #region MetaInfoData
        //MetaInfoData
        MetaInfoData data = GameManager.i.metaScript.GetMetaInfoData();
        if (data != null)
        {
            write.metaGameData.listOfBoss = WriteListOfMetaData(data.arrayOfMetaData[(int)MetaTabSide.Boss], "listOfBoss");
            write.metaGameData.listOfSubBoss1 = WriteListOfMetaData(data.arrayOfMetaData[(int)MetaTabSide.SubBoss1], "listOfSubBoss1");
            write.metaGameData.listOfSubBoss2 = WriteListOfMetaData(data.arrayOfMetaData[(int)MetaTabSide.SubBoss2], "listOfSubBoss2");
            write.metaGameData.listOfSubBoss3 = WriteListOfMetaData(data.arrayOfMetaData[(int)MetaTabSide.SubBoss3], "listOfSubBoss3");
            write.metaGameData.listOfStatusData = WriteListOfMetaData(data.listOfStatusData, "listOfStatusData");
            write.metaGameData.listOfRecommended = WriteListOfMetaData(data.listOfRecommended, "listOfRecommended");
            write.metaGameData.selectedDefault = WriteIndividualMetaData(data.selectedDefault);
        }
        else { Debug.LogError("Invalid metaInfoData (Null)"); }
        #endregion

    }
    #endregion


    #region Write Game Data
    /// <summary>
    /// Important Game data write to file
    /// </summary>
    private void WriteGameData()
    {
        //sideManager.cs
        write.gameData.resistanceCurrent = GameManager.i.sideScript.resistanceCurrent;
        write.gameData.authorityCurrent = GameManager.i.sideScript.authorityCurrent;
        write.gameData.resistanceOverall = GameManager.i.sideScript.resistanceOverall;
        write.gameData.authorityOverall = GameManager.i.sideScript.authorityOverall;
        write.gameData.playerSide = GameManager.i.sideScript.PlayerSide.name;
        //turnManager.cs -> private fields
        write.gameData.turnData = GameManager.i.turnScript.LoadWriteData();
        //turnManager.cs -> public fields
        write.gameData.winStateLevel = GameManager.i.turnScript.winStateLevel;
        write.gameData.winReasonLevel = GameManager.i.turnScript.winReasonLevel;
        write.gameData.winStateCampaign = GameManager.i.turnScript.winStateCampaign;
        write.gameData.winReasonCampaign = GameManager.i.turnScript.winReasonCampaign;
        write.gameData.authoritySecurity = GameManager.i.turnScript.authoritySecurityState;
        write.gameData.currentSide = GameManager.i.turnScript.currentSide.name;
        write.gameData.haltExecution = GameManager.i.turnScript.haltExecution;
        //top widget
        write.gameData.isSecurityFlash = GameManager.i.widgetTopScript.CheckSecurityFlash();
    }
    #endregion


    #region  Write Scenario Data
    /// <summary>
    /// CityManager.cs and HqManager.cs data write to file
    /// </summary>
    public void WriteScenarioData()
    {
        //cityManager.cs
        write.scenarioData.cityLoyalty = GameManager.i.cityScript.CityLoyalty;
        //HqManager.cs
        write.scenarioData.bossOpinion = GameManager.i.hqScript.GetBossOpinion();
        write.scenarioData.approvalZeroTimer = GameManager.i.hqScript.GetApprovalZeroTimer();
        write.scenarioData.hqSupportAuthority = GameManager.i.hqScript.ApprovalAuthority;
        write.scenarioData.hqSupportResistance = GameManager.i.hqScript.ApprovalResistance;
        write.scenarioData.isHqRelocating = GameManager.i.hqScript.isHqRelocating;
        write.scenarioData.timerHqRelocating = GameManager.i.hqScript.GetHqRelocationTimer();
        //objectiveManager.cs
        List<Objective> tempList = GameManager.i.objectiveScript.GetListOfObjectives();
        if (tempList != null)
        {
            for (int i = 0; i < tempList.Count; i++)
            {
                Objective objective = tempList[i];
                if (objective != null)
                {
                    write.scenarioData.listOfObjectiveNames.Add(objective.name);
                    write.scenarioData.listOfObjectiveProgress.Add(objective.progress);
                }
                else { Debug.LogWarningFormat("Invalid objective (Null) for tempList[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfObjectives (Null)"); }
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
        Dictionary<string, Secret> dictOfSecrets = GameManager.i.dataScript.GetDictOfSecrets();
        if (dictOfSecrets != null)
        {
            foreach (var secret in dictOfSecrets)
            {
                if (secret.Value != null)
                {
                    //create a new SaveSecret & pass across dynamic data
                    SaveSecret saveSecret = new SaveSecret();
                    saveSecret.secretName = secret.Key;
                    saveSecret.gainedWhen = secret.Value.gainedWhen;
                    saveSecret.revealedWho = secret.Value.revealedWho;
                    saveSecret.revealedID = secret.Value.revealedID;
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
        List<Secret> listOfSecrets = GameManager.i.dataScript.GetListOfPlayerSecrets();
        if (listOfSecrets != null)
        {
            for (int i = 0; i < listOfSecrets.Count; i++)
            { write.dataData.listOfPlayerSecrets.Add(listOfSecrets[i].name); }
        }
        else { Debug.LogError("Invalid listOfPlayerSecrets (Null)"); }
        //secret list -> OrganisationSecrets
        listOfSecrets = GameManager.i.dataScript.GetListOfOrganisationSecrets();
        if (listOfSecrets != null)
        {
            for (int i = 0; i < listOfSecrets.Count; i++)
            { write.dataData.listOfOrganisationSecrets.Add(listOfSecrets[i].name); }
        }
        else { Debug.LogError("Invalid listOfOrganisationSecrets (Null)"); }
        //secret list -> StorySecrets
        listOfSecrets = GameManager.i.dataScript.GetListOfStorySecrets();
        if (listOfSecrets != null)
        {
            for (int i = 0; i < listOfSecrets.Count; i++)
            { write.dataData.listOfStorySecrets.Add(listOfSecrets[i].name); }
        }
        else { Debug.LogError("Invalid listOfStorySecrets (Null)"); }
        //secret list -> RevealSecrets
        listOfSecrets = GameManager.i.dataScript.GetListOfRevealedSecrets();
        if (listOfSecrets != null)
        {
            for (int i = 0; i < listOfSecrets.Count; i++)
            { write.dataData.listOfRevealedSecrets.Add(listOfSecrets[i].name); }
        }
        else { Debug.LogError("Invalid listOfRevealedSecrets (Null)"); }
        //secret list -> DeletedSecrets
        listOfSecrets = GameManager.i.dataScript.GetListOfDeletedSecrets();
        if (listOfSecrets != null)
        {
            for (int i = 0; i < listOfSecrets.Count; i++)
            { write.dataData.listOfDeletedSecrets.Add(listOfSecrets[i].name); }
        }
        else { Debug.LogError("Invalid listOfDeletedSecrets (Null)"); }
        #endregion

        #region investigations
        List<Investigation> listOfInvestigations = GameManager.i.dataScript.GetListOfCompletedInvestigations();
        if (listOfInvestigations != null)
        { write.dataData.listOfInvestigations.AddRange(listOfInvestigations); }
        else { Debug.LogError("Invalid listOfCompletedInvestigations (Null)"); }
        #endregion

        #region awards
        //Commendations
        List<AwardData> listOfCommendations = GameManager.i.dataScript.GetListOfCommendations();
        if (listOfCommendations != null)
        { write.dataData.listOfCommendations.AddRange(listOfCommendations); }
        else { Debug.LogError("Invalid listOfCommendations (Null)"); }
        //Blackmarks
        List<AwardData> listOfBlackmarks = GameManager.i.dataScript.GetListOfBlackmarks();
        if (listOfBlackmarks != null)
        { write.dataData.listOfBlackmarks.AddRange(listOfBlackmarks); }
        else { Debug.LogError("Invalid listOfBlackmarks (Null)"); }
        #endregion

        #region Organisations
        List<Organisation> listOfOrgs = GameManager.i.dataScript.GetListOfCurrentOrganisations();
        if (listOfOrgs != null)
        {
            foreach (Organisation org in listOfOrgs)
            {
                if (org != null)
                {
                    write.dataData.listOfCurrentOrganisations.Add(org.name);
                    SaveOrganisation saveOrg = new SaveOrganisation();
                    saveOrg.name = org.name;
                    saveOrg.isContact = org.isContact;
                    saveOrg.isCutOff = org.isCutOff;
                    saveOrg.isSecretKnown = org.isSecretKnown;
                    saveOrg.reputation = org.GetReputation();
                    saveOrg.freedom = org.GetFreedom();
                    saveOrg.maxStat = org.maxStat;
                    saveOrg.timer = org.timer;
                    //add to list
                    write.dataData.listOfSaveOrganisations.Add(saveOrg);
                }
                else { Debug.LogError("Invalid org (Null) in listOfCurrentOrganisations"); }
            }
            write.dataData.listOfCureOrgData.AddRange(GameManager.i.dataScript.GetListOfOrgData(OrganisationType.Cure));
            write.dataData.listOfContractOrgData.AddRange(GameManager.i.dataScript.GetListOfOrgData(OrganisationType.Contract));
            write.dataData.listOfEmergencyOrgData.AddRange(GameManager.i.dataScript.GetListOfOrgData(OrganisationType.Emergency));
            write.dataData.listOfHQOrgData.AddRange(GameManager.i.dataScript.GetListOfOrgData(OrganisationType.HQ));
            write.dataData.listOfInfoOrgData.AddRange(GameManager.i.dataScript.GetListOfOrgData(OrganisationType.Info));
            //OrgInfoArray
            write.dataData.listOfOrgInfoData.AddRange(GameManager.i.dataScript.GetArrayOfOrgInfo().ToList());
        }
        else { Debug.LogError("Invalid listOfCurrentOrganisations (Null)"); }
        #endregion

        #region HQ
        if (write.dataData.listOfHqEvents != null)
        { GameManager.i.dataScript.SetListOfHqEvents(write.dataData.listOfHqEvents); }
        #endregion

        #region MetaOptions
        Dictionary<string, MetaOption> dictOfMetaOptions = GameManager.i.dataScript.GetDictOfMetaOptions();
        if (dictOfMetaOptions != null)
        {
            foreach (var metaOption in dictOfMetaOptions)
            {
                if (metaOption.Value != null)
                {
                    SaveMetaOption metaData = new SaveMetaOption()
                    {
                        optionName = metaOption.Key,
                        statTimesSelected = metaOption.Value.statTimesSelected
                    };
                    //add to list
                    write.dataData.listOfMetaOptions.Add(metaData);
                }
                else { Debug.LogWarningFormat("Invalid metaOption \"{0}\" in dictOfMetaOptions", metaOption.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfMetaOptions (Null)"); }
        #endregion

        #region Cures
        Dictionary<string, Cure> dictOfCures = GameManager.i.dataScript.GetDictOfCures();
        if (dictOfCures != null)
        {
            foreach (var record in dictOfCures)
            {
                if (record.Value != null)
                {
                    SaveCure saveCure = new SaveCure()
                    {
                        cureName = record.Value.name,
                        isActive = record.Value.isActive,
                        isOrgCure = record.Value.isOrgActivated
                    };
                    write.dataData.listOfCures.Add(saveCure);
                }
                else { Debug.LogWarningFormat("Invalid cure (Null) in dictOfCures for key \"{0}\"", record.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfCures (Null)"); }
        #endregion

        #region contacts
        //
        // - - - Contacts
        //
        //main contact dictionary (do first)
        Dictionary<int, Contact> dictOfContacts = GameManager.i.dataScript.GetDictOfContacts();
        if (dictOfContacts != null)
        {
            foreach (var contact in dictOfContacts)
            {
                if (contact.Value != null)
                { write.dataData.listOfContacts.Add(contact.Value); }
                else { Debug.LogWarningFormat("Invalid contact (Null) in dictOfContacts for contactID {0}", contact.Key); }
            }
        }
        //contact pool
        List<int> listOfContactPool = GameManager.i.dataScript.GetContactPool();
        if (listOfContactPool != null)
        { write.dataData.listOfContactPool.AddRange(listOfContactPool); }
        else { Debug.LogError("Invalid listOfContactPool (Null)"); }
        //contact dict's of lists -> dictOfActorContacts
        Dictionary<int, List<int>> dictOfActorContacts = GameManager.i.dataScript.GetDictOfActorContacts();
        if (dictOfActorContacts != null)
        {
            foreach (var contactList in dictOfActorContacts)
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
        Dictionary<int, List<int>> dictOfNodeContactsResistance = GameManager.i.dataScript.GetDictOfNodeContacts(globalResistance);
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
        Dictionary<int, List<int>> dictOfNodeContactsAuthority = GameManager.i.dataScript.GetDictOfNodeContacts(globalAuthority);
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
        Dictionary<int, List<Contact>> dictOfContactsByNodeResistance = GameManager.i.dataScript.GetDictOfContactsByNodeResistance();
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
        Dictionary<int, Team> dictOfTeams = GameManager.i.dataScript.GetDictOfTeams();
        if (dictOfTeams != null)
        {
            foreach (var record in dictOfTeams)
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
        int[,] arrayOfTeams = GameManager.i.dataScript.GetArrayOfTeams();
        if (arrayOfTeams != null)
        {
            //check array dimensions (similar check on load)
            int numOfArcs = GameManager.i.dataScript.CheckNumOfNodeArcs();
            int numOfInfo = (int)TeamInfo.Count;
            int size0 = arrayOfTeams.GetUpperBound(0) + 1;
            int size1 = arrayOfTeams.GetUpperBound(1) + 1;
            Debug.AssertFormat(numOfArcs == size0, "Mismatch on array size, rank 0 (Arcs) should be {0}, is {1}", numOfArcs, size0);
            Debug.AssertFormat(numOfInfo == size1, "Mismatch on arraySize, rank 1 (TeamInfo) should be {0}, is {1}", numOfInfo, size1);
            //write data
            for (int i = 0; i < size0; i++)
            {
                for (int j = 0; j < size1; j++)
                { write.dataData.listOfArrayOfTeams.Add(arrayOfTeams[i, j]); }
            }
        }
        else { Debug.LogError("Invalid arrayOfTeams (Null)"); }
        //list -> reserves
        List<int> tempList = GameManager.i.dataScript.GetTeamPool(TeamPool.Reserve);
        if (tempList != null)
        { write.dataData.listOfTeamPoolReserve.AddRange(tempList); }
        else { Debug.LogError("Invalid teamPoolReserve list (Null)"); }
        //list -> OnMap
        tempList = GameManager.i.dataScript.GetTeamPool(TeamPool.OnMap);
        if (tempList != null)
        { write.dataData.listOfTeamPoolOnMap.AddRange(tempList); }
        else { Debug.LogError("Invalid teamPoolOnMap list (Null)"); }
        //list -> InTransit
        tempList = GameManager.i.dataScript.GetTeamPool(TeamPool.InTransit);
        if (tempList != null)
        { write.dataData.listOfTeamPoolInTransit.AddRange(tempList); }
        else { Debug.LogError("Invalid teamPoolInTransit list (Null)"); }
        //counter
        write.dataData.teamCounter = GameManager.i.teamScript.teamIDCounter;
        #endregion

        #region statistics
        //level stats
        Dictionary<StatType, int> dictOfStatisticsLevel = GameManager.i.dataScript.GetDictOfStatisticsLevel();
        if (dictOfStatisticsLevel != null)
        {
            //only need to save stats as StatType Key's are sequentially numbered enums
            foreach (var stat in dictOfStatisticsLevel)
            { write.dataData.listOfStatisticsLevel.Add(stat.Value); }
        }
        else { Debug.LogError("Invalid dictOfStatisticsLevel (Null)"); }
        //campaign stats
        Dictionary<StatType, int> dictOfStatisticsCampaign = GameManager.i.dataScript.GetDictOfStatisticsCampaign();
        if (dictOfStatisticsCampaign != null)
        {
            //only need to save stats as StatType Key's are sequentially numbered enums
            foreach (var stat in dictOfStatisticsCampaign)
            { write.dataData.listOfStatisticsCampaign.Add(stat.Value); }
        }
        else { Debug.LogError("Invalid dictOfStatisticsCampaign (Null)"); }
        #endregion

        #region AI
        //array -> AI Resources
        int[] arrayOfAIResources = GameManager.i.dataScript.GetArrayOfAIResources();
        if (arrayOfAIResources != null)
        { write.dataData.listOfArrayOfAIResources.AddRange(arrayOfAIResources); }
        else { Debug.LogError("Invalid arrayOfAIResources (Null)"); }
        //queue -> recentNodes
        Queue<AITracker> queueOfRecentNodes = GameManager.i.dataScript.GetRecentNodesQueue();
        if (queueOfRecentNodes != null)
        { write.dataData.listOfRecentNodes.AddRange(queueOfRecentNodes); }
        else { Debug.LogError("Invalid queueOfRecentNodes (Null)"); }
        //queue -> recentConnections
        Queue<AITracker> queueOfRecentConnections = GameManager.i.dataScript.GetRecentConnectionsQueue();
        if (queueOfRecentConnections != null)
        { write.dataData.listOfRecentConnections.AddRange(queueOfRecentConnections); }
        else { Debug.LogError("Invalid queueOfRecentConnections (Null)"); }
        #endregion

        #region messages
        //archive
        Dictionary<int, Message> dictOfArchiveMessages = GameManager.i.dataScript.GetMessageDict(MessageCategory.Archive);
        if (dictOfArchiveMessages != null)
        {
            write.dataData.listOfArchiveMessagesKey.AddRange(dictOfArchiveMessages.Keys);
            write.dataData.listOfArchiveMessagesValue.AddRange(dictOfArchiveMessages.Values);
        }
        else { Debug.LogError("Invalid dictOfArchiveMessages (Null)"); }
        //pending
        Dictionary<int, Message> dictOfPendingMessages = GameManager.i.dataScript.GetMessageDict(MessageCategory.Pending);
        if (dictOfPendingMessages != null)
        {
            write.dataData.listOfPendingMessagesKey.AddRange(dictOfPendingMessages.Keys);
            write.dataData.listOfPendingMessagesValue.AddRange(dictOfPendingMessages.Values);
        }
        else { Debug.LogError("Invalid dictOfPendingMessages (Null)"); }
        //current
        Dictionary<int, Message> dictOfCurrentMessages = GameManager.i.dataScript.GetMessageDict(MessageCategory.Current);
        if (dictOfCurrentMessages != null)
        {
            write.dataData.listOfCurrentMessagesKey.AddRange(dictOfCurrentMessages.Keys);
            write.dataData.listOfCurrentMessagesValue.AddRange(dictOfCurrentMessages.Values);
        }
        else { Debug.LogError("Invalid dictOfCurrentMessages (Null)"); }
        //ai
        Dictionary<int, Message> dictOfAIMessages = GameManager.i.dataScript.GetMessageDict(MessageCategory.AI);
        if (dictOfAIMessages != null)
        {
            write.dataData.listOfAIMessagesKey.AddRange(dictOfAIMessages.Keys);
            write.dataData.listOfAIMessagesValue.AddRange(dictOfAIMessages.Values);
        }
        else { Debug.LogError("Invalid dictOfAIMessages (Null)"); }
        //id counter
        write.dataData.messageIDCounter = GameManager.i.messageScript.messageIDCounter;
        #endregion

        #region topics
        //topic types
        Dictionary<string, TopicTypeData> dictOfTopicTypes = GameManager.i.dataScript.GetDictOfTopicTypeData();
        if (dictOfTopicTypes != null)
        {
            write.dataData.listOfTopicTypeValues.AddRange(dictOfTopicTypes.Values);
            write.dataData.listOfTopicTypeKeys.AddRange(dictOfTopicTypes.Keys);
        }
        else { Debug.LogError("Invalid dictOfTopicTypes (Null)"); }
        //topic sub types
        Dictionary<string, TopicTypeData> dictOfTopicSubTypes = GameManager.i.dataScript.GetDictOfTopicSubTypeData();
        if (dictOfTopicSubTypes != null)
        {
            write.dataData.listOfTopicSubTypeValues.AddRange(dictOfTopicSubTypes.Values);
            write.dataData.listOfTopicSubTypeKeys.AddRange(dictOfTopicSubTypes.Keys);
        }
        else { Debug.LogError("Invalid dictOfTopicSubTypes (Null)"); }
        //topic history
        Dictionary<int, HistoryTopic> dictOfTopicHistory = GameManager.i.dataScript.GetDictOfTopicHistory();
        if (dictOfTopicHistory != null)
        {
            write.dataData.listOfTopicHistoryValues.AddRange(dictOfTopicHistory.Values);
            write.dataData.listOfTopicHistoryKeys.AddRange(dictOfTopicHistory.Keys);
        }
        else { Debug.LogError("Invalid dictOfTopicHistory (Null)"); }
        //listOfTopicTypesLevel
        List<TopicType> listOfTopicTypesLevel = GameManager.i.dataScript.GetListOfTopicTypesLevel();
        if (listOfTopicTypesLevel != null)
        { write.dataData.listOfTopicTypesLevel.AddRange(listOfTopicTypesLevel.Select(x => x.name).ToList()); }
        else { Debug.LogError("Invalid listOfTopicTypesLevel (Null)"); }
        //dictOfTopicPools
        Dictionary<string, List<Topic>> dictOfTopicPools = GameManager.i.dataScript.GetDictOfTopicPools();
        if (dictOfTopicPools != null)
        {
            foreach (var topicList in dictOfTopicPools)
            {
                StringListWrapper listOfTopicNames = new StringListWrapper();
                listOfTopicNames.myList.AddRange(topicList.Value.Select(x => x.name).ToList());
                if (listOfTopicNames.myList != null)
                {
                    write.dataData.listOfTopicPoolsValue.Add(listOfTopicNames);
                    write.dataData.listOfTopicPoolsKeys.Add(topicList.Key);
                }
                else { Debug.LogWarningFormat("Invalid listOfTopicNames (Null) for topicSubType \"{0}\"", topicList.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfTopicPools (Null)"); }
        #endregion

        #region relations
        //dictOfRelations
        Dictionary<int, RelationshipData> dictOfRelations = GameManager.i.dataScript.GetDictOfRelations();
        if (dictOfRelations != null)
        {
            write.dataData.listOfRelationshipKeys.AddRange(dictOfRelations.Keys);
            write.dataData.listOfRelationshipValues.AddRange(dictOfRelations.Values);
        }
        else { Debug.LogError("Invalid dictOfRelations (Null)"); }
        #endregion

        #region registers
        //Ongoing Effects
        Dictionary<int, EffectDataOngoing> dictOfOngoing = GameManager.i.dataScript.GetDictOfOngoingEffects();
        if (dictOfOngoing != null)
        {
            foreach (var ongoing in dictOfOngoing)
            {
                if (ongoing.Value != null)
                { write.dataData.listOfOngoingEffects.Add(ongoing.Value); }
                else { Debug.LogWarningFormat("Invalid ongoing.Value (Null) for ongoingID {0}", ongoing.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfOngoing (Null)"); }
        //Action adjustmehts
        List<ActionAdjustment> listOFAdjustments = GameManager.i.dataScript.GetListOfActionAdjustments();
        if (listOFAdjustments != null)
        { write.dataData.listOfActionAdjustments.AddRange(listOFAdjustments); }
        else { Debug.LogError("Invalid listOfActionAdjustments (Null)"); }
        #endregion

        /*#region moveNodes
        List<int> listOfNodes = GameManager.i.dataScript.GetListOfMoveNodes();
        if (listOfNodes != null)
        { write.dataData.listOfMoveNodes.AddRange(listOfNodes); }
        else { Debug.LogError("Invalid listOfMoveNodes (Null)"); }
        #endregion*/

        #region InfoPipeLine
        Dictionary<MsgPipelineType, ModalOutcomeDetails> dictOfPipeline = GameManager.i.guiScript.GetDictOfPipeline();
        if (dictOfPipeline != null)
        {
            if (dictOfPipeline.Count > 0)
            {
                foreach (var record in dictOfPipeline)
                {
                    if (record.Value != null)
                    {
                        //create InfoPipelineDetails object and save to list
                        SaveInfoPipeLineInfo pipe = new SaveInfoPipeLineInfo()
                        {
                            sideName = record.Value.side.name,
                            spriteName = record.Value.sprite.name,
                            textTop = record.Value.textTop,
                            textBottom = record.Value.textBottom,
                            modalLevel = record.Value.modalLevel,
                            isAction = record.Value.isAction,
                            reason = record.Value.reason,
                            help0 = record.Value.help0,
                            help1 = record.Value.help1,
                            help2 = record.Value.help2,
                            help3 = record.Value.help3,
                            modalState = record.Value.modalState,
                            type = record.Value.type
                        };
                        write.dataData.listOfInfoPipelineDetails.Add(pipe);
                    }
                    else { Debug.LogWarningFormat("Invalid dictOfPipeline[{0}] (Null)", record.Key); }
                }
            }
        }
        else { Debug.LogError("Invalid dictOfPipeline (Null)"); }
        #endregion

        #region mainInfoApp

        //delayed itemData
        List<ItemData> listOfDelayed = GameManager.i.dataScript.GetListOfDelayedItemData();
        if (listOfDelayed != null)
        { write.dataData.listOfDelayedItemData.AddRange(listOfDelayed); }
        else { Debug.LogError("Invalid listOfDelayedItemData (Null)"); }
        //currentInfoData
        MainInfoData data = GameManager.i.dataScript.GetCurrentInfoData();
        if (data != null)
        {
            //ticker text
            write.dataData.currentInfo.tickerText = data.tickerText;
            //ItemData
            for (int i = 0; i < (int)ItemTab.Count; i++)
            {
                List<ItemData> listOfItemData = data.arrayOfItemData[i];
                if (listOfItemData != null)
                {
                    //one for each MainInfoApp tab
                    switch (i)
                    {
                        case 0: write.dataData.currentInfo.listOfTab0Item.AddRange(listOfItemData); break;
                        case 1: write.dataData.currentInfo.listOfTab1Item.AddRange(listOfItemData); break;
                        case 2: write.dataData.currentInfo.listOfTab2Item.AddRange(listOfItemData); break;
                        case 3: write.dataData.currentInfo.listOfTab3Item.AddRange(listOfItemData); break;
                        case 4: write.dataData.currentInfo.listOfTab4Item.AddRange(listOfItemData); break;
                        case 5: write.dataData.currentInfo.listOfTab5Item.AddRange(listOfItemData); break;
                        default: Debug.LogErrorFormat("Unrecognised index {0}", i); break;
                    }
                }
                else { Debug.LogErrorFormat("Invalid listOfItemData (Null) for arrayOfItemData[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid MainInfoData (Null)"); }
        //dictOfHistory
        Dictionary<int, MainInfoData> dictOfHistory = GameManager.i.dataScript.GetDictOfHistory();
        if (dictOfHistory != null)
        {
            foreach (var info in dictOfHistory)
            {
                if (info.Value != null)
                {
                    //create and store a new MainInfoData package
                    SaveMainInfo mainInfo = new SaveMainInfo();
                    mainInfo.tickerText = info.Value.tickerText;
                    //ItemData
                    for (int i = 0; i < (int)ItemTab.Count; i++)
                    {
                        List<ItemData> listOfItemData = info.Value.arrayOfItemData[i];
                        if (listOfItemData != null)
                        {
                            //one for each MainInfoApp tab
                            switch (i)
                            {
                                case 0: mainInfo.listOfTab0Item.AddRange(listOfItemData); break;
                                case 1: mainInfo.listOfTab1Item.AddRange(listOfItemData); break;
                                case 2: mainInfo.listOfTab2Item.AddRange(listOfItemData); break;
                                case 3: mainInfo.listOfTab3Item.AddRange(listOfItemData); break;
                                case 4: mainInfo.listOfTab4Item.AddRange(listOfItemData); break;
                                case 5: mainInfo.listOfTab5Item.AddRange(listOfItemData); break;
                                default: Debug.LogErrorFormat("Unrecognised index {0}", i); break;
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid listOfItemData (Null) for arrayOfItemData[{0}]", i); }
                    }
                    //add to lists
                    write.dataData.listOfHistoryValue.Add(mainInfo);
                    write.dataData.listOfHistoryKey.Add(info.Key);
                }
                else { Debug.LogErrorFormat("Invalid MainInfoData (Null) for turn {0}", info.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfHistory (Null)"); }
        //arrayOfItemDataByPriority
        List<ItemData>[,] arrayOfItemDataByPriority = GameManager.i.dataScript.GetArrayOfItemDataByPriority();
        if (arrayOfItemDataByPriority != null)
        {
            for (int outer = 0; outer < (int)ItemTab.Count; outer++)
            {
                SavePriorityInfo prioritySave = new SavePriorityInfo();
                for (int inner = 0; inner < (int)ItemPriority.Count; inner++)
                {
                    switch (inner)
                    {
                        case 0: prioritySave.listOfPriorityLow.AddRange(arrayOfItemDataByPriority[outer, inner]); break;
                        case 1: prioritySave.listOfPriorityMed.AddRange(arrayOfItemDataByPriority[outer, inner]); break;
                        case 2: prioritySave.listOfPriorityHigh.AddRange(arrayOfItemDataByPriority[outer, inner]); break;
                        default: Debug.LogErrorFormat("Unrecognised inner {0}", inner); break;
                    }
                }
                //add to list
                write.dataData.listOfPriorityData.Add(prioritySave);
            }
        }
        else { Debug.LogError("Invalid arrayOfItemDataByPriority (Null)"); }
        #endregion

        #region history
        //rebel moves
        List<HistoryRebelMove> listOfHistoryRebel = GameManager.i.dataScript.GetListOfHistoryRebelMove();
        if (listOfHistoryRebel != null)
        { write.dataData.listOfHistoryRebel.AddRange(listOfHistoryRebel); }
        else { Debug.LogError("Invalid listOfHistoryRebelMove (Null)"); }
        //nemesis moves
        List<HistoryNemesisMove> listOfHistoryNemesis = GameManager.i.dataScript.GetListOfHistoryNemesisMove();
        if (listOfHistoryNemesis != null)
        { write.dataData.listOfHistoryNemesis.AddRange(listOfHistoryNemesis); }
        else { Debug.LogError("Invalid listOfHistoryNemesisMove (Null)"); }
        //VIP moves
        List<HistoryNpcMove> listOfHistoryVip = GameManager.i.dataScript.GetListOfHistoryVipMove();
        if (listOfHistoryVip != null)
        { write.dataData.listOfHistoryVip.AddRange(listOfHistoryVip); }
        else { Debug.LogError("Invalid listOfHistoryVipMove (Null)"); }
        //Player History
        List<HistoryActor> listOfHistoryPlayer = GameManager.i.dataScript.GetListOfHistoryPlayer();
        if (listOfHistoryPlayer != null)
        { write.dataData.listOfHistoryPlayer.AddRange(listOfHistoryPlayer); }
        else { Debug.LogError("Invalid listOfHistoryPlayer (Null)"); }
        //Level History
        List<HistoryLevel> listOfLevelHistory = GameManager.i.dataScript.GetDictOfCampaignHistory().Values.ToList();
        if (listOfLevelHistory != null)
        { write.dataData.listOfHistoryLevel.AddRange(listOfLevelHistory); }
        else { Debug.LogError("Invalid listOfLevelHistory (Null)"); }
        #endregion

        #region newsFeed
        //news items
        List<NewsItem> listOfNewsItems = GameManager.i.dataScript.GetListOfNewsItems();
        if (listOfNewsItems != null)
        { write.dataData.listOfNewsItems.AddRange(listOfNewsItems); }
        else { Debug.LogError("Invalid listOfNewsItems (Null)"); }
        //adverts
        List<string> listOfAdverts = GameManager.i.dataScript.GetListOfAdverts();
        if (listOfAdverts != null)
        { write.dataData.listOfAdverts.AddRange(listOfAdverts); }
        else { Debug.LogError("Invalid listOfAdverts (Null)"); }
        #endregion

        #region textLists
        //Test lists indexes
        Dictionary<string, TextList> dictOfTextLists = GameManager.i.dataScript.GetDictOfTextList();
        if (dictOfTextLists != null)
        {
            write.dataData.listOfTextListNames = dictOfTextLists.Keys.ToList();
            write.dataData.listOfTextListIndexes = dictOfTextLists.Select(x => x.Value.index).ToList();
        }
        else { Debug.LogError("Invalid dictOfTextLists (Null)"); }
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
        // - - - Main dictionarys
        //
        //dictOfActors
        Dictionary<int, Actor> dictOfActors = GameManager.i.dataScript.GetDictOfActors();
        if (dictOfActors != null)
        {
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
        //dictOfHQ
        Dictionary<int, Actor> dictOfHQ = GameManager.i.dataScript.GetDictOfHq();
        if (dictOfHQ != null)
        {
            foreach (var actor in dictOfHQ)
            {
                if (actor.Value != null)
                {
                    SaveActor saveActor = WriteIndividualActor(actor.Value);
                    if (saveActor != null)
                    { write.actorData.listOfDictHQ.Add(saveActor); }
                }
                else { Debug.LogWarning("Invalid actor (Null) in dictOfHQ"); }
            }
        }
        else { Debug.LogError("Invalid dictOfHQ (Null)"); }
        //
        // - - - Actor arrays
        //
        int sideNum = GameManager.i.dataScript.GetNumOfGlobalSide();
        int actorNum = GameManager.i.actorScript.maxNumOfOnMapActors;
        Debug.Assert(sideNum > 0, "Invalid sideNum (Zero or less)");
        Debug.Assert(actorNum > 0, "Invalid actorNum (Zero or less)");
        //write data to lists (not multidimensional arrays split into single lists for serialization)
        Actor[,] arrayOfActors = GameManager.i.dataScript.GetArrayOfActors();
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
        //arrayOfActorsPresent
        bool[,] arrayOfActorsPresent = GameManager.i.dataScript.GetArrayOfActorsPresent();
        if (arrayOfActorsPresent != null)
        {
            for (int i = 0; i < arrayOfActorsPresent.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < arrayOfActorsPresent.GetUpperBound(1) + 1; j++)
                { write.actorData.listOfActorsPresent.Add(arrayOfActorsPresent[i, j]); }
            }
        }
        else { Debug.LogError("Invalid arrayOfActorsPresent (Null)"); }
        //arrayOfActorsHQ
        Actor[] arrayOfActorsHQ = GameManager.i.dataScript.GetArrayOfActorsHQ();
        if (arrayOfActorsHQ != null)
        {
            for (int i = 0; i < arrayOfActorsHQ.Length; i++)
            {
                Actor actor = arrayOfActorsHQ[i];
                if (actor != null)
                { write.actorData.listOfActorsHQ.Add(actor.hqID); }
                else { write.actorData.listOfActorsHQ.Add(-1); }
            }
        }
        else { Debug.LogError("Invalid arrayOfActorsHQ (Null)"); }
        //
        // - - - Pool lists
        //
        write.actorData.authorityActorPoolLevelOne.AddRange(GameManager.i.dataScript.GetActorRecruitPool(1, globalAuthority));
        write.actorData.authorityActorPoolLevelTwo.AddRange(GameManager.i.dataScript.GetActorRecruitPool(2, globalAuthority));
        write.actorData.authorityActorPoolLevelThree.AddRange(GameManager.i.dataScript.GetActorRecruitPool(3, globalAuthority));
        write.actorData.resistanceActorPoolLevelOne.AddRange(GameManager.i.dataScript.GetActorRecruitPool(1, globalResistance));
        write.actorData.resistanceActorPoolLevelTwo.AddRange(GameManager.i.dataScript.GetActorRecruitPool(2, globalResistance));
        write.actorData.resistanceActorPoolLevelThree.AddRange(GameManager.i.dataScript.GetActorRecruitPool(3, globalResistance));

        write.actorData.authorityActorReserve.AddRange(GameManager.i.dataScript.GetListOfReserveActors(globalAuthority));
        write.actorData.authorityActorDismissed.AddRange(GameManager.i.dataScript.GetListOfDismissedActors(globalAuthority));
        write.actorData.authorityActorPromoted.AddRange(GameManager.i.dataScript.GetListOfPromotedActors(globalAuthority));
        write.actorData.authorityActorDisposedOf.AddRange(GameManager.i.dataScript.GetListOfDisposedOfActors(globalAuthority));
        write.actorData.authorityActorResigned.AddRange(GameManager.i.dataScript.GetListOfResignedActors(globalAuthority));

        write.actorData.resistanceActorReserve.AddRange(GameManager.i.dataScript.GetListOfReserveActors(globalResistance));
        write.actorData.resistanceActorDismissed.AddRange(GameManager.i.dataScript.GetListOfDismissedActors(globalResistance));
        write.actorData.resistanceActorPromoted.AddRange(GameManager.i.dataScript.GetListOfPromotedActors(globalResistance));
        write.actorData.resistanceActorDisposedOf.AddRange(GameManager.i.dataScript.GetListOfDisposedOfActors(globalResistance));
        write.actorData.resistanceActorResigned.AddRange(GameManager.i.dataScript.GetListOfResignedActors(globalResistance));

        write.actorData.actorHQPool.AddRange(GameManager.i.dataScript.GetListOfActorHq());
        //
        // - - - ActorManager.cs 
        //
        write.actorData.actorIDCounter = GameManager.i.actorScript.actorIDCounter;
        write.actorData.hqIDCounter = GameManager.i.actorScript.hqIDCounter;
        write.actorData.lieLowTimer = GameManager.i.actorScript.lieLowTimer;
        write.actorData.doomTimer = GameManager.i.actorScript.doomTimer;
        write.actorData.captureTimer = GameManager.i.actorScript.captureTimerPlayer;
        write.actorData.isGearCheckRequired = GameManager.i.actorScript.isGearCheckRequired;
        write.actorData.nameSet = GameManager.i.actorScript.nameSet.name;
        //
        // - - - Actor.cs fast access fields
        //
        write.actorData.actorStressNone = "ActorStressNone";
        write.actorData.actorCorruptNone = "ActorCorruptNone";
        write.actorData.actorUnhappyNone = "ActorUnhappyNone";
        write.actorData.actorBlackmailNone = "ActorBlackmailNone";
        write.actorData.actorBlackmailTimerHigh = "ActorBlackmailTimerHigh";
        write.actorData.actorBlackmailTimerLow = "ActorBlackmailTimerLow";
        write.actorData.maxNumOfSecrets = GameManager.i.secretScript.secretMaxNum;
        write.actorData.compatibilityOne = GameManager.i.personScript.compatibilityChanceOne;
        write.actorData.compatibilityTwo = GameManager.i.personScript.compatibilityChanceTwo;
        write.actorData.compatibilityThree = GameManager.i.personScript.compatibilityChanceThree;
        Debug.Assert(write.actorData.maxNumOfSecrets > -1, "Invalid maxNumOfSecrets (-1)");
        Debug.Assert(write.actorData.compatibilityOne > 0, "Invalid compatibilityOne (Zero)");
    }

    #endregion


    #region Write Node data
    /// <summary>
    /// NodeManager.cs data to file and dynamic Node.SO data
    /// </summary>
    private void WriteNodeData()
    {
        write.nodeData.crisisPolicyModifier = GameManager.i.nodeScript.crisisPolicyModifier;
        write.nodeData.nodeCounter = GameManager.i.nodeScript.nodeIDCounter;
        write.nodeData.connCounter = GameManager.i.nodeScript.connIDCounter;
        write.nodeData.nodeHighlight = GameManager.i.nodeScript.nodeHighlight;
        write.nodeData.nodePlayer = GameManager.i.nodeScript.nodePlayer;
        write.nodeData.nodeNemesis = GameManager.i.nodeScript.nodeNemesis;
        write.nodeData.nodeCaptured = GameManager.i.nodeScript.nodeCaptured;
        //
        // - - - Node.cs
        //
        Dictionary<int, Node> dictOfNodes = GameManager.i.dataScript.GetDictOfNodes();
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
                    saveNode.targetName = record.Value.targetName;
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
                    saveNode.defaultChar = record.Value.defaultChar;
                    if (record.Value.crisis != null)
                    { saveNode.nodeCrisisName = record.Value.crisis.name; }
                    else { saveNode.nodeCrisisName = ""; }
                    //cure
                    if (record.Value.cure == null)
                    { saveNode.cureName = ""; }
                    else { saveNode.cureName = record.Value.cure.name; }
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
                            else { Debug.LogWarningFormat("Invalid team (Null) in listOfTeams[{0}], nodeID {1}", i, saveNode.nodeID); }
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
                            else { Debug.LogWarningFormat("Invalid effectOngoing (Null) for listOfOngoing[{0}], nodeID {1}", i, saveNode.nodeID); }
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid listOfOngoingEffects (Null) for nodeID {0}, nodeID {1}", record.Key, saveNode.nodeID); }
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
        List<Node> listOfCrisisNodes = GameManager.i.dataScript.GetListOfCrisisNodes();
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
        List<Node> listOfCureNodes = GameManager.i.dataScript.GetListOfCureNodes();
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
        //
        // - - - Move Nodes
        //
        List<Node> listOfMoveNodes = GameManager.i.dataScript.GetListOfMoveNodes();
        if (listOfMoveNodes != null)
        {
            //save nodeID's
            for (int i = 0; i < listOfMoveNodes.Count; i++)
            {
                Node node = listOfMoveNodes[i];
                if (node != null)
                { write.nodeData.listOfMoveNodes.Add(node.nodeID); }
                else { Debug.LogWarningFormat("Invalid node (Null) in listOfMoveNodes[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfMoveNodes (Null)"); }
    }
    #endregion


    #region Write Connection Data
    /// <summary>
    /// Connection.SO dynamic data write to file
    /// </summary>
    private void WriteConnectionData()
    {
        Dictionary<int, Connection> dictOfConnections = GameManager.i.dataScript.GetDictOfConnections();
        if (dictOfConnections != null)
        {
            foreach (var conn in dictOfConnections)
            {
                if (conn.Value != null)
                {
                    SaveConnection save = new SaveConnection();
                    save.connID = conn.Value.connID;
                    save.securityLevel = conn.Value.SecurityLevel;
                    save.activityCount = conn.Value.activityCount;
                    save.activityTime = conn.Value.activityTime;
                    List<EffectDataOngoing> listOfOngoing = conn.Value.GetListOfOngoingEffects();
                    if (listOfOngoing != null)
                    { save.listOfOngoingEffects.AddRange(listOfOngoing); }
                    else { Debug.LogErrorFormat("Invalid listOfOngoingEffects (Null) for connID {0}", conn.Key); }
                    //add to list
                    write.connData.listOfConnectionData.Add(save);
                }
                else { Debug.LogWarningFormat("Invalid connection (Null) for connID {0}", conn.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfConnections (Null)"); }
    }
    #endregion


    #region Write Gear Data
    /// <summary>
    /// GearManager.cs data to file
    /// </summary>
    private void WriteGearData()
    {
        Dictionary<string, Gear> dictOfGear = GameManager.i.dataScript.GetDictOfGear();
        if (dictOfGear != null)
        {
            foreach (var gear in dictOfGear)
            {
                if (gear.Value != null)
                {
                    //create new SaveGear object and copy across all dynamic data
                    SaveGear saveGear = new SaveGear();
                    saveGear.gearName = gear.Value.name;
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
        List<string> tempList = GameManager.i.dataScript.GetListOfGear(GameManager.i.gearScript.gearCommon);
        if (tempList != null)
        { write.gearData.listOfCommonGear.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfCommonGear (Null)"); }
        //list -> listOfRareGear
        tempList = GameManager.i.dataScript.GetListOfGear(GameManager.i.gearScript.gearRare);
        if (tempList != null)
        { write.gearData.listOfRareGear.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfRareGear (Null)"); }
        //list -> listOfUniqueGear
        tempList = GameManager.i.dataScript.GetListOfGear(GameManager.i.gearScript.gearUnique);
        if (tempList != null)
        { write.gearData.listOfUniqueGear.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfUniqueGear (Null)"); }
        //list -> listOfLostGear
        tempList = GameManager.i.dataScript.GetListOfLostGear();
        if (tempList != null)
        { write.gearData.listOfLostGear.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfLostGear (Null)"); }
        //list -> listOfCurrentGear
        tempList = GameManager.i.dataScript.GetListOfCurrentGear();
        if (tempList != null)
        { write.gearData.listOfCurrentGear.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfCurrentGear (Null)"); }
        //GearManager fields
        write.gearData.gearSaveCurrentCost = GameManager.i.gearScript.GetGearSaveCurrentCost();
        write.gearData.listOfCompromisedGear.AddRange(GameManager.i.gearScript.GetListOfCompromisedGear());
    }
    #endregion


    #region Write Topic Data
    /// <summary>
    /// Topic.cs dynamic data to file
    /// </summary>
    private void WriteTopicData()
    {
        Dictionary<string, Topic> dictOfTopics = GameManager.i.dataScript.GetDictOfTopics();
        if (dictOfTopics != null)
        {
            foreach (var topic in dictOfTopics)
            {
                //only save data for current topics used in level, ignore the rest
                if (topic.Value.isCurrent == true)
                {
                    //create new SaveTopic object and copy across all dynamic data
                    SaveTopic saveTopic = new SaveTopic();
                    saveTopic.topicName = topic.Key;
                    saveTopic.status = topic.Value.status;
                    saveTopic.timerStart = topic.Value.timerStart;
                    saveTopic.timerRepeat = topic.Value.timerRepeat;
                    saveTopic.timerWindow = topic.Value.timerWindow;
                    saveTopic.turnsDormant = topic.Value.turnsDormant;
                    saveTopic.turnsActive = topic.Value.turnsActive;
                    saveTopic.turnsLive = topic.Value.turnsLive;
                    saveTopic.turnsDone = topic.Value.turnsDone;
                    //add to list
                    write.topicData.listOfTopics.Add(saveTopic);
                }
            }
        }
        else { Debug.LogError("Invalid dictOfTopics"); }
    }
    #endregion


    #region Write Contact Data
    /// <summary>
    /// ContactManager.cs data write to file
    /// </summary>
    private void WriteContactData()
    {
        write.contactData.contactIDCounter = GameManager.i.contactScript.contactIDCounter;
        //arrayOfContactNetworks
        int[] arrayOfContacts = GameManager.i.contactScript.GetArrayOfContactNetworks();
        if (arrayOfContacts != null)
        { write.contactData.listOfContactNetworks.AddRange(arrayOfContacts); }
        else { Debug.LogError("Invalid arrayOfContactNetworks (Null)"); }
        //arrayOfActors
        Actor[] arrayOfActors = GameManager.i.contactScript.GetArrayOfActors();
        if (arrayOfActors != null)
        {
            //loop array and store actorID's in list
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                Actor actor = arrayOfActors[i];
                if (actor != null)
                { write.contactData.listOfActors.Add(actor.actorID); }
                else
                {
                    write.contactData.listOfActors.Add(-1);
                    /*Debug.LogErrorFormat("Invalid actor (Null) for arrayOfActors[{0}]", i);*/
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }

    }
    #endregion


    #region Write AI Data
    /// <summary>
    /// AI/Rebel Manager.cs write data to file
    /// </summary>
    private void WriteAIData()
    {
        //list -> tasks final
        List<AITask> listOfTasksFinal = GameManager.i.aiScript.GetListOfTasksFinal();
        if (listOfTasksFinal != null)
        { write.aiData.listOfTasksFinal.AddRange(listOfTasksFinal); }
        else { Debug.LogError("Invalid listOfTaskFinal (Null)"); }
        //list -> tasks potential
        List<AITask> listOfTasksPotential = GameManager.i.aiScript.GetListOfTasksPotential();
        if (listOfTasksPotential != null)
        { write.aiData.listOfTasksPotential.AddRange(listOfTasksPotential); }
        else { Debug.LogError("Invalid listOfTaskFinal (Null)"); }
        //list -> player effects
        List<string> listOfPlayerEffects = GameManager.i.aiScript.GetListOfPlayerEffects();
        if (listOfPlayerEffects != null)
        { write.aiData.listOfPlayerEffects.AddRange(listOfPlayerEffects); }
        else { Debug.LogError("Invalid listOfPlayerEffects (Null)"); }
        //list -> player effect descriptors
        List<string> listOfPlayerEffectDescriptors = GameManager.i.aiScript.GetListOfPlayerEffectDescriptors();
        if (listOfPlayerEffectDescriptors != null)
        { write.aiData.listOfPlayerEffectDescriptors.AddRange(listOfPlayerEffectDescriptors); }
        else { Debug.LogError("Invalid listOfPlayerEffectDescriptors (Null)"); }
        //list -> arrayOfAITaskTypes (Authority)
        int[] arrayOfAITaskTypes = GameManager.i.aiScript.GetArrayOfAITaskTypes();
        if (arrayOfAITaskTypes != null)
        { write.aiData.listOfAITaskTypesAuthority.AddRange(arrayOfAITaskTypes); }
        else { Debug.LogError("Invalid arrayOfAITaskTypesAuthority (Null)"); }
        //AIManager.cs -> public fields
        write.aiData.immediateFlagAuthority = GameManager.i.aiScript.immediateFlagAuthority;
        write.aiData.immediateFlagResistance = GameManager.i.aiScript.immediateFlagResistance;
        write.aiData.resourcesGainAuthority = GameManager.i.aiScript.resourcesGainAuthority;
        write.aiData.resourcesGainResistance = GameManager.i.aiScript.resourcesGainResistance;
        write.aiData.aiTaskCounter = GameManager.i.aiScript.aiTaskCounter;
        write.aiData.hackingAttemptsTotal = GameManager.i.aiScript.hackingAttemptsTotal;
        write.aiData.hackingAttemptsReboot = GameManager.i.aiScript.hackingAttemptsReboot;
        write.aiData.hackingAttemptsDetected = GameManager.i.aiScript.hackingAttemptsDetected;
        write.aiData.hackingCurrentCost = GameManager.i.aiScript.hackingCurrentCost;
        write.aiData.hackingModifiedCost = GameManager.i.aiScript.hackingModifiedCost;
        write.aiData.isHacked = GameManager.i.aiScript.isHacked;
        write.aiData.aiAlertStatus = GameManager.i.aiScript.aiAlertStatus;
        write.aiData.isRebooting = GameManager.i.aiScript.isRebooting;
        write.aiData.rebootTimer = GameManager.i.aiScript.rebootTimer;
        write.aiData.numOfCrisis = GameManager.i.aiScript.numOfCrisis;
        write.aiData.status = GameManager.i.aiScript.status;
        write.aiData.inactiveStatus = GameManager.i.aiScript.inactiveStatus;
        //AIManager.cs -> private fields
        write.aiData.saveAI = GameManager.i.aiScript.LoadWriteData();
        //AIRebelManager.cs -> private fields
        write.aiData.saveRebel = GameManager.i.aiRebelScript.WriteSaveData();
        if (write.aiData.saveRebel == null)
        { Debug.LogError("Invalid AIRebelManager.cs saveRebel data (Null)"); }
        //AIRebelManager -> Nemesis Reports
        List<AITracker> tempListNemesis = GameManager.i.aiRebelScript.GetListOfNemesisReports();
        if (tempListNemesis != null)
        { write.aiData.listOfNemesisReports.AddRange(tempListNemesis); }
        else { Debug.LogError("Invalid listOfNemesisReports (Null)"); }
        //AIRebelManager -> Erasure Reports
        List<AITracker> tempListErasure = GameManager.i.aiRebelScript.GetListOfErasureReports();
        if (tempListErasure != null)
        { write.aiData.listOfErasureReports.AddRange(tempListErasure); }
        else { Debug.LogError("Invalid listOfErasureReports (Null)"); }
        arrayOfAITaskTypes = GameManager.i.aiRebelScript.GetArrayOfAITaskTypes();
        if (arrayOfAITaskTypes != null)
        { write.aiData.listOfAITaskTypesRebel.AddRange(arrayOfAITaskTypes); }
        else { Debug.LogError("Invalid arrayOfAITaskTypesRebel (Null)"); }
    }
    #endregion


    #region Write Target Data
    /// <summary>
    /// TargetManager.cs data write to file
    /// </summary>
    private void WriteTargetData()
    {
        write.targetData.startTargets = GameManager.i.targetScript.StartTargets;
        write.targetData.activeTargets = GameManager.i.targetScript.ActiveTargets;
        write.targetData.liveTargets = GameManager.i.targetScript.LiveTargets;
        write.targetData.maxTargets = GameManager.i.targetScript.MaxTargets;
        write.targetData.targetOrgName = GameManager.i.targetScript.targetOrgName;
        //target.SO dynamic data
        Dictionary<string, Target> dictOfTargets = GameManager.i.dataScript.GetDictOfTargets();
        if (dictOfTargets != null)
        {
            foreach (var target in dictOfTargets)
            {
                if (target.Value != null)
                {
                    SaveTarget save = new SaveTarget();
                    //copy dynamic target SO data
                    save.targetStatus = target.Value.targetStatus;
                    save.intel = target.Value.intel;
                    save.targetName = target.Value.name;
                    save.ongoingID = target.Value.ongoingID;
                    save.isKnownByAI = target.Value.isKnownByAI;
                    save.nodeID = target.Value.nodeID;
                    save.distance = target.Value.distance;
                    save.newIntel = target.Value.newIntel;
                    save.intelGain = target.Value.intelGain;
                    save.turnSuccess = target.Value.turnSuccess;
                    save.turnDone = target.Value.turnDone;
                    save.numOfAttempts = target.Value.numOfAttempts;
                    save.turnsWindow = target.Value.turnsWindow;
                    save.timerDelay = target.Value.timerDelay;
                    save.timerHardLimit = target.Value.timerHardLimit;
                    save.timerWindow = target.Value.timerWindow;
                    //add to list
                    write.targetData.listOfTargets.Add(save);
                }
                else { Debug.LogError("Invalid target (Null) in dictOfTargets"); }
            }
        }
        else { Debug.LogError("Invalid dictOfTargets (Null)"); }
        //arrayOfGenericTargets
        List<string>[] arrayOfGenericTargets = GameManager.i.dataScript.GetArrayOfGenericTargets();
        if (arrayOfGenericTargets != null)
        {
            for (int i = 0; i < arrayOfGenericTargets.Length; i++)
            {
                StringListWrapper listOfTargets = new StringListWrapper();
                listOfTargets.myList.AddRange(arrayOfGenericTargets[i]);
                if (listOfTargets.myList != null)
                { write.targetData.listOfGenericTargets.Add(listOfTargets); }
                else { Debug.LogWarningFormat("Invalid listOfTargets (Null) for arrayOfGenericTargets[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid arrayOfGenericTargets (Null)"); }
        //listOfNodesWithTargets
        List<int> listOfNodesWithTargets = GameManager.i.dataScript.GetListOfNodesWithTargets();
        if (listOfNodesWithTargets != null)
        { write.targetData.listOfNodesWithTargets.AddRange(listOfNodesWithTargets); }
        else { Debug.LogError("Invalid listOfNodesWithTargets (Null)"); }
        //targetPoolActive
        List<Target> listOfTargetPool = GameManager.i.dataScript.GetTargetPool(Status.Active);
        if (listOfTargetPool != null)
        {
            for (int i = 0; i < listOfTargetPool.Count; i++)
            {
                Target target = listOfTargetPool[i];
                if (target != null)
                { write.targetData.listOfTargetPoolActive.Add(target.name); }
                else { Debug.LogWarningFormat("Invalid target (Null) for listOfTargetPoolActive[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfTargetPoolActive (Null)"); }
        //targetPoolLive
        listOfTargetPool = GameManager.i.dataScript.GetTargetPool(Status.Live);
        if (listOfTargetPool != null)
        {
            for (int i = 0; i < listOfTargetPool.Count; i++)
            {
                Target target = listOfTargetPool[i];
                if (target != null)
                { write.targetData.listOfTargetPoolLive.Add(target.name); }
                else { Debug.LogWarningFormat("Invalid target (Null) for listOfTargetPoolLive[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfTargetPoolLive (Null)"); }
        //targetPoolOutstanding
        listOfTargetPool = GameManager.i.dataScript.GetTargetPool(Status.Outstanding);
        if (listOfTargetPool != null)
        {
            for (int i = 0; i < listOfTargetPool.Count; i++)
            {
                Target target = listOfTargetPool[i];
                if (target != null)
                { write.targetData.listOfTargetPoolOutstanding.Add(target.name); }
                else { Debug.LogWarningFormat("Invalid target (Null) for listOfTargetPoolOutstanding[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfTargetPoolOutstanding (Null)"); }
        //targetPoolDone
        listOfTargetPool = GameManager.i.dataScript.GetTargetPool(Status.Done);
        if (listOfTargetPool != null)
        {
            for (int i = 0; i < listOfTargetPool.Count; i++)
            {
                Target target = listOfTargetPool[i];
                if (target != null)
                { write.targetData.listOfTargetPoolDone.Add(target.name); }
                else { Debug.LogWarningFormat("Invalid target (Null) for listOfTargetPoolDone[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfTargetPoolDone (Null)"); }
    }
    #endregion


    #region Write Statistics Data
    /// <summary>
    /// StatisticsManager.cs write data to file
    /// </summary>
    private void WriteStatisticsData()
    {
        write.statisticsData.ratioPlayerNodeActions = GameManager.i.statScript.ratioPlayerNodeActions;
        write.statisticsData.ratioPlayerTargetAttempts = GameManager.i.statScript.ratioPlayerTargetAttempts;
        write.statisticsData.ratioPlayerMoveActions = GameManager.i.statScript.ratioPlayerMoveActions;
        write.statisticsData.ratioPlayerLieLowDays = GameManager.i.statScript.ratioPlayerLieLowDays;
        write.statisticsData.ratioPlayerGiveGear = GameManager.i.statScript.ratioPlayerGiveGear;
        write.statisticsData.ratioPlayerManageActions = GameManager.i.statScript.ratioPlayerManageActions;
        write.statisticsData.ratioPlayerDoNothing = GameManager.i.statScript.ratioPlayerDoNothing;
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
        Campaign campaign = GameManager.i.dataScript.GetCampaign(read.campaignData.campaignName);
        if (campaign != null)
        { GameManager.i.campaignScript.campaign = campaign; }
        else { Debug.LogErrorFormat("Invalid campaign (Null) for campaign {0}", read.campaignData.campaignName); }
        //scenario
        GameManager.i.campaignScript.SetScenario(read.campaignData.scenarioIndex);
        //arrayOfStoryStatus
        GameManager.i.campaignScript.SetArrayOfStoryStatus(read.campaignData.arrayOfStoryStatus);
        //mission
        GameManager.i.campaignScript.SetMission();
        //campaign status
        GameManager.i.campaignScript.SetCommendations(read.campaignData.commendations);
        GameManager.i.campaignScript.SetBlackMarks(read.campaignData.blackMarks);
        GameManager.i.campaignScript.SetInvestigationBlackmarks(read.campaignData.investigationBlackMarks);
    }
    #endregion


    #region Read Seed Data
    /// <summary>
    /// LevelManager.cs seed used to generate level
    /// </summary>
    private void ReadSeedData()
    {
        GameManager.i.levelScript.seed = read.seedData.levelSeed;
        GameManager.i.seedDev = read.seedData.devSeed;
    }
    #endregion


    #region Read Npc Data
    /// <summary>
    /// Separate from ReadCampaignData for sequencing issues (needs to be after ReadNodeData)
    /// </summary>
    private void ReadNpcData()
    {
        //npc
        if (read.campaignData.isNpc == true)
        { GameManager.i.missionScript.SetNpcData(read.campaignData.npc); }
        else { GameManager.i.missionScript.SetNpcData(); }
    }
    #endregion


    #region Read Option Data
    /// <summary>
    /// OptionManager.cs Data
    /// </summary>
    private void ReadOptionData()
    {
        GameManager.i.optionScript.autoGearResolution = read.optionData.autoGearResolution;
        GameManager.i.optionScript.fogOfWar = read.optionData.fogOfWar;
        GameManager.i.optionScript.debugData = read.optionData.debugData;
        GameManager.i.optionScript.noAI = read.optionData.noAI;
        GameManager.i.optionScript.showContacts = read.optionData.showContacts;
        GameManager.i.optionScript.showRenown = read.optionData.showRenown;
        GameManager.i.optionScript.connectorTooltips = read.optionData.connectorTooltips;
        GameManager.i.optionScript.ColourOption = read.optionData.colourScheme;
        //Debug button texts
        if (read.optionData.autoGearResolution == true)
        { GameManager.i.debugScript.optionAutoGear = "Auto Gear OFF"; }
        if (read.optionData.fogOfWar == true)
        { GameManager.i.debugScript.optionFogOfWar = "Fog Of War OFF"; }
        if (read.optionData.connectorTooltips == true)
        { GameManager.i.debugScript.optionConnectorTooltips = "Conn tooltips OFF"; }
        if (read.optionData.debugData == true)
        { GameManager.i.debugScript.optionDebugData = "Debug Data OFF"; }
        if (read.optionData.showRenown == false)
        { GameManager.i.debugScript.optionRenownUI = "Renown UI ON"; }
        if (read.optionData.showContacts == true)
        { GameManager.i.debugScript.optionContacts = "Contacts OFF"; }
    }
    #endregion


    #region Read Player Data
    /// <summary>
    /// PlayerManager.cs Data
    /// </summary>
    private void ReadPlayerData()
    {
        GameManager.i.playerScript.Renown = read.playerData.renown;
        GameManager.i.playerScript.Invisibility = read.playerData.Invisibility;
        GameManager.i.playerScript.Innocence = read.playerData.Innocence;
        GameManager.i.playerScript.sex = read.playerData.sex;
        GameManager.i.playerScript.SetMood(read.playerData.mood);
        GameManager.i.playerScript.status = read.playerData.status;
        GameManager.i.playerScript.tooltipStatus = read.playerData.tooltipStatus;
        GameManager.i.playerScript.inactiveStatus = read.playerData.inactiveStatus;
        GameManager.i.playerScript.isBreakdown = read.playerData.isBreakdown;
        GameManager.i.playerScript.isEndOfTurnGearCheck = read.playerData.isEndOfTurnGearCheck;
        GameManager.i.playerScript.isLieLowFirstturn = read.playerData.isLieLowFirstturn;
        GameManager.i.playerScript.isAddicted = read.playerData.isAddicted;
        GameManager.i.playerScript.isStressLeave = read.playerData.isStressLeave;
        GameManager.i.playerScript.isStressed = read.playerData.isStressed;
        GameManager.i.playerScript.isSpecialMoveGear = read.playerData.isSpecialMoveGear;
        GameManager.i.playerScript.numOfSuperStress = read.playerData.numOfSuperStress;
        GameManager.i.playerScript.stressImmunityCurrent = read.playerData.stressImmunityCurrent;
        GameManager.i.playerScript.stressImmunityStart = read.playerData.stressImmunityStart;
        GameManager.i.playerScript.addictedTally = read.playerData.addictedTally;
        GameManager.i.playerScript.SetArrayOfCaptureTools(read.playerData.arrayOfCaptureTools);
        Personality personality = GameManager.i.playerScript.GetPersonality();
        if (personality != null)
        {
            personality.SetFactors(read.playerData.listOfPersonalityFactors.ToArray());
            personality.SetDescriptors(read.playerData.listOfDescriptors);
            if (string.IsNullOrEmpty(read.playerData.profile) == false)
            {
                personality.SetProfile(read.playerData.profile);
                personality.SetProfileDescriptor(read.playerData.profileDescriptor);
                personality.SetProfileExplanation(read.playerData.profileExplanation);
            }
        }
        else { Debug.LogWarning("Invalid Player personality (Null)"); }
        //gear
        GameManager.i.playerScript.SetListOfGear(read.playerData.listOfGear);
        //secrets
        List<Secret> listOfSecrets = new List<Secret>();
        for (int i = 0; i < read.playerData.listOfSecrets.Count; i++)
        {
            Secret secret = GameManager.i.dataScript.GetSecret(read.playerData.listOfSecrets[i]);
            if (secret != null)
            { listOfSecrets.Add(secret); }
        }
        GameManager.i.playerScript.SetSecrets(listOfSecrets);
        //investigations
        GameManager.i.playerScript.SetListOfInvestigations(read.playerData.listOfInvestigations);
        //mood history
        GameManager.i.playerScript.SetMoodHistory(read.playerData.listOfMoodHistory);
        //conditions -> Resistance
        List<Condition> listOfConditions = new List<Condition>();
        for (int i = 0; i < read.playerData.listOfConditionsResistance.Count; i++)
        {
            Condition condition = GameManager.i.dataScript.GetCondition(read.playerData.listOfConditionsResistance[i]);
            if (condition != null)
            { listOfConditions.Add(condition); }
        }
        GameManager.i.playerScript.SetConditions(listOfConditions, globalResistance);
        //conditions -> Authority
        listOfConditions = new List<Condition>();
        for (int i = 0; i < read.playerData.listOfConditionsAuthority.Count; i++)
        {
            Condition condition = GameManager.i.dataScript.GetCondition(read.playerData.listOfConditionsAuthority[i]);
            if (condition != null)
            { listOfConditions.Add(condition); }
        }
        GameManager.i.playerScript.SetConditions(listOfConditions, globalAuthority);
        //nodeActions
        GameManager.i.playerScript.SetListOfNodeActions(read.playerData.listOfNodeActions);
    }
    #endregion


    #region Read MetaGame Data
    /// <summary>
    /// MetaGame related data
    /// </summary>
    private void ReadMetaGameData()
    {
        // - - - MetaGameOptions
        GameManager.i.metaScript.SetMetaOptions(read.metaGameData.metaGameOptions);

        #region TransitionInfoData
        //
        // - - - TransitionInfoData
        //
        TransitionInfoData info = new TransitionInfoData();

        #region End Level
        //End Level
        info.objectiveStatus = read.metaGameData.objectiveStatus;
        info.listOfEndLevelData = read.metaGameData.listOfEndLevelData;
        #endregion

        #region HQ Status
        //Hq -> standard lists
        info.listOfHqRenown = read.metaGameData.listOfHqRenown;
        info.listOfHqTitles = read.metaGameData.listOfHqTitles;
        info.listOfHqEvents = read.metaGameData.listOfHqEvents;
        info.listOfHqTooltips = read.metaGameData.listOfHqTooltips;
        info.listOfWorkerRenown = read.metaGameData.listOfWorkerRenown;
        info.listOfWorkerArcs = read.metaGameData.listOfWorkerArcs;
        info.listOfWorkerTooltips = read.metaGameData.listOfWorkerTooltips;
        //sprites -> hq
        if (read.metaGameData.listOfHqSprites != null)
            {
            for (int i = 0; i < read.metaGameData.listOfHqSprites.Count; i++)
            {
                Sprite sprite = GameManager.i.dataScript.GetSprite(read.metaGameData.listOfHqSprites[i]);
                if (sprite != null)
                { info.listOfHqSprites.Add(sprite); }
                else { Debug.LogErrorFormat("Invalid sprite (Null) for spriteName \"{0}\", listOfHqSprites[{1}]", read.metaGameData.listOfHqSprites[i], i); }
            }
        }
        else { Debug.LogError("Invalid listOfHqSprites (Null)"); }
        //sprites -> workers
        if (read.metaGameData.listOfWorkerSprites != null)
        {
            for (int i = 0; i < read.metaGameData.listOfWorkerSprites.Count; i++)
            {
                Sprite sprite = GameManager.i.dataScript.GetSprite(read.metaGameData.listOfWorkerSprites[i]);
                if (sprite != null)
                { info.listOfWorkerSprites.Add(sprite); }
                else { Debug.LogErrorFormat("Invalid sprite (Null) for spriteName \"{0}\", listOfWorkerSprites[{1}]", read.metaGameData.listOfWorkerSprites[i], i); }
            }
        }
        else { Debug.LogError("Invalid listOfWorkerSprites (Null)"); }
        #endregion

        //set TransitionInfoData
        GameManager.i.metaScript.SetTransitionInfoData(info);
        GameManager.i.transitionScript.SetTransitionInfoData(info);
        #endregion

        #region MetaInfoData
        //
        // - - - MetaInfoData
        //
        List<MetaData> listOfBoss = ReadListOfSaveMetaData(read.metaGameData.listOfBoss, "ListOfBoss");
        List<MetaData> listOfSubBoss1 = ReadListOfSaveMetaData(read.metaGameData.listOfSubBoss1, "ListOfSubBoss1");
        List<MetaData> listOfSubBoss2 = ReadListOfSaveMetaData(read.metaGameData.listOfSubBoss2, "ListOfSubBoss2");
        List<MetaData> listOfSubBoss3 = ReadListOfSaveMetaData(read.metaGameData.listOfSubBoss3, "ListOfSubBoss3");
        List<MetaData> listOfStatusData = ReadListOfSaveMetaData(read.metaGameData.listOfStatusData, "ListOfStatusData");
        List<MetaData> listOfRecommended = ReadListOfSaveMetaData(read.metaGameData.listOfRecommended, "ListOfRecommended");
        MetaData selectedDefault = ReadIndividualSaveMetaData(read.metaGameData.selectedDefault);
        //error checks
        if (listOfBoss == null) { Debug.LogError("Invalid listOfBoss (Null)"); }
        if (listOfSubBoss1 == null) { Debug.LogError("Invalid listOfSubBoss1 (Null)"); }
        if (listOfSubBoss2 == null) { Debug.LogError("Invalid listOfSubBoss2 (Null)"); }
        if (listOfSubBoss3 == null) { Debug.LogError("Invalid listOfSubBoss3 (Null)"); }
        if (listOfStatusData == null) { Debug.LogError("Invalid listOfStatusData (Null)"); }
        if (listOfRecommended == null) { Debug.LogError("Invalid listOfRecommended (Null)"); }
        if (selectedDefault == null) { Debug.LogError("Invalid selectedDefault (Null)"); }
        //populate MetaInfoData
        MetaInfoData metaInfoData = new MetaInfoData();
        metaInfoData.arrayOfMetaData[(int)MetaTabSide.Boss] = listOfBoss;
        metaInfoData.arrayOfMetaData[(int)MetaTabSide.SubBoss1] = listOfSubBoss1;
        metaInfoData.arrayOfMetaData[(int)MetaTabSide.SubBoss2] = listOfSubBoss2;
        metaInfoData.arrayOfMetaData[(int)MetaTabSide.SubBoss3] = listOfSubBoss3;
        metaInfoData.listOfStatusData = listOfStatusData;
        metaInfoData.listOfRecommended = listOfRecommended;
        metaInfoData.selectedDefault = selectedDefault;
        //Set metaInfoData
        GameManager.i.metaScript.SetMetaInfoData(metaInfoData);
        GameManager.i.metaUIScript.SetMetaInfoData(metaInfoData);
        #endregion
    }
    #endregion


    #region Read Game Data
    /// <summary>
    /// important Game data
    /// </summary>
    private void ReadGameData(GlobalSide side)
    {
        //Sidemanager.cs
        GameManager.i.sideScript.resistanceCurrent = read.gameData.resistanceCurrent;
        GameManager.i.sideScript.authorityCurrent = read.gameData.authorityCurrent;
        GameManager.i.sideScript.resistanceOverall = read.gameData.resistanceOverall;
        GameManager.i.sideScript.authorityOverall = read.gameData.authorityOverall;
        GameManager.i.sideScript.PlayerSide = side;
        //turnManager.cs -> private fields
        GameManager.i.turnScript.LoadReadData(read.gameData.turnData);
        //turnManager.cs -> public fields
        GameManager.i.turnScript.winStateLevel = read.gameData.winStateLevel;
        GameManager.i.turnScript.winReasonLevel = read.gameData.winReasonLevel;
        GameManager.i.turnScript.winStateCampaign = read.gameData.winStateCampaign;
        GameManager.i.turnScript.winReasonCampaign = read.gameData.winReasonCampaign;
        GameManager.i.turnScript.authoritySecurityState = read.gameData.authoritySecurity;
        GlobalSide currentSide = GameManager.i.dataScript.GetGlobalSide(read.gameData.currentSide);
        if (currentSide != null)
        { GameManager.i.turnScript.currentSide = currentSide; }
        else { Debug.LogError("Invalid currentSide (Null)"); }
        GameManager.i.turnScript.haltExecution = read.gameData.haltExecution;


    }
    #endregion


    #region Read Scenario Data
    /// <summary>
    /// CityManager.cs and FactionManager.cs data
    /// </summary>
    private void ReadScenarioData()
    {
        //cityManager.cs
        GameManager.i.cityScript.CityLoyalty = read.scenarioData.cityLoyalty;
        //factionManager.cs
        GameManager.i.hqScript.LoadSetHqApproval(globalAuthority, read.scenarioData.hqSupportAuthority);
        GameManager.i.hqScript.LoadSetHqApproval(globalResistance, read.scenarioData.hqSupportResistance);
        GameManager.i.hqScript.LoadApprovalZeroTimer(read.scenarioData.approvalZeroTimer);
        GameManager.i.hqScript.SetBossOpinion(read.scenarioData.bossOpinion, "Load Game");
        GameManager.i.hqScript.SetHqRelocationTimer(read.scenarioData.timerHqRelocating);
        GameManager.i.hqScript.isHqRelocating = read.scenarioData.isHqRelocating;
        //objectiveManager.cs
        List<Objective> tempList = new List<Objective>();
        for (int i = 0; i < read.scenarioData.listOfObjectiveNames.Count; i++)
        {
            Objective objective = GameManager.i.dataScript.GetObjective(read.scenarioData.listOfObjectiveNames[i]);
            if (objective != null)
            {
                objective.progress = read.scenarioData.listOfObjectiveProgress[i];
                tempList.Add(objective);
            }
            else { Debug.LogWarningFormat("Invalid objective (Null) for {0}", read.scenarioData.listOfObjectiveNames[i]); }
        }
        //update objectives
        if (tempList.Count > 0)
        { GameManager.i.objectiveScript.SetObjectives(tempList, false); }
    }
    #endregion


    #region Read Data Data
    /// <summary>
    /// DataManager.cs data (need to clear collections prior to adding to them)
    /// NOTE: happens BEFORE Datamanager.cs -> ResetLoadGame so make sure any collections here are not in that method otherwise they'll be updated here and then cleared there.
    /// </summary>
    private void ReadDataData()
    {
        int countKey, countValue;
        string itemName;

        #region statistics
        //level stats
        Dictionary<StatType, int> dictOfStatisticsLevel = GameManager.i.dataScript.GetDictOfStatisticsLevel();
        if (dictOfStatisticsLevel != null)
        {
            int count = read.dataData.listOfStatisticsLevel.Count;
            Debug.AssertFormat(count == dictOfStatisticsLevel.Count, "Mismatch on count (should be list {0}, is dict {1}", count, dictOfStatisticsLevel.Count);
            StatType statType;
            for (int i = 0; i < count; i++)
            {
                //update dictionary 
                statType = (StatType)i;
                if (dictOfStatisticsLevel.ContainsKey(statType) == true)
                { dictOfStatisticsLevel[statType] = read.dataData.listOfStatisticsLevel[i]; }
                else
                {
                    //StatType not present, add to dictionary
                    try
                    { dictOfStatisticsLevel.Add(statType, read.dataData.listOfStatisticsLevel[i]); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate StatType \"{0\", entry not added", statType); }
                }
            }
        }
        else { Debug.LogError("Invalid dictOfStatisticsLevel (Null)"); }
        //campaign stats
        Dictionary<StatType, int> dictOfStatisticsCampaign = GameManager.i.dataScript.GetDictOfStatisticsCampaign();
        if (dictOfStatisticsCampaign != null)
        {
            int count = read.dataData.listOfStatisticsCampaign.Count;
            Debug.AssertFormat(count == dictOfStatisticsCampaign.Count, "Mismatch on count (should be list {0}, is dict {1}", count, dictOfStatisticsCampaign.Count);
            StatType statType;
            for (int i = 0; i < count; i++)
            {
                //update dictionary 
                statType = (StatType)i;
                if (dictOfStatisticsCampaign.ContainsKey(statType) == true)
                { dictOfStatisticsCampaign[statType] = read.dataData.listOfStatisticsCampaign[i]; }
                else
                {
                    //StatType not present, add to dictionary
                    try
                    { dictOfStatisticsCampaign.Add(statType, read.dataData.listOfStatisticsCampaign[i]); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate StatType \"{0\", entry not added", statType); }
                }
            }
        }
        else { Debug.LogError("Invalid dictOfStatisticsCampaign (Null)"); }
        #endregion

        #region secrets
        //
        // - - - Secrets
        //
        //Copy any dynamic data into dictOfSecrets
        Dictionary<string, Secret> dictOfSecrets = GameManager.i.dataScript.GetDictOfSecrets();
        if (dictOfSecrets != null)
        {
            //loop saved list of secret changes
            for (int i = 0; i < read.dataData.listOfSecretChanges.Count; i++)
            {
                SaveSecret secretData = read.dataData.listOfSecretChanges[i];
                if (secretData != null)
                {
                    //find record in dict
                    if (dictOfSecrets.ContainsKey(secretData.secretName) == true)
                    {
                        Secret secret = dictOfSecrets[secretData.secretName];
                        if (secret != null)
                        {
                            //copy data across
                            secret.status = secretData.status;
                            secret.gainedWhen = secretData.gainedWhen;
                            secret.revealedWho = secretData.revealedWho;
                            secret.revealedID = secretData.revealedID;
                            secret.revealedWhen = secretData.revealedWhen;
                            secret.deletedWhen = secretData.deleteWhen;
                            secret.SetListOfActors(secretData.listOfActors);
                        }
                        else { Debug.LogErrorFormat("Invalid secret (Null) for secret {0}", secretData.secretName); }
                    }
                    else { Debug.LogWarningFormat("Secret {0} not found in dictionary, record not updated for dynamic data", secretData.secretName); }
                }
                else { Debug.LogErrorFormat("Invalid SaveSecret (Null) for listOfSecretChanges[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfSecrets (Null)"); }

        //Secrets List -> PlayerSecrets
        List<Secret> listOfSecrets = new List<Secret>();
        for (int i = 0; i < read.dataData.listOfPlayerSecrets.Count; i++)
        {
            Secret secret = GameManager.i.dataScript.GetSecret(read.dataData.listOfPlayerSecrets[i]);
            if (secret != null)
            { listOfSecrets.Add(secret); }
        }
        GameManager.i.dataScript.SetListOfPlayerSecrets(listOfSecrets);
        //Secrets List -> DesperateSecrets
        listOfSecrets.Clear();
        for (int i = 0; i < read.dataData.listOfOrganisationSecrets.Count; i++)
        {
            Secret secret = GameManager.i.dataScript.GetSecret(read.dataData.listOfOrganisationSecrets[i]);
            if (secret != null)
            { listOfSecrets.Add(secret); }
        }
        GameManager.i.dataScript.SetListOfOrganisationSecrets(listOfSecrets);
        //Secrets List -> Storysecrets
        listOfSecrets.Clear();
        for (int i = 0; i < read.dataData.listOfStorySecrets.Count; i++)
        {
            Secret secret = GameManager.i.dataScript.GetSecret(read.dataData.listOfStorySecrets[i]);
            if (secret != null)
            { listOfSecrets.Add(secret); }
        }
        GameManager.i.dataScript.SetListOfStorySecrets(listOfSecrets);
        //Secrets List -> RevealedSecrets
        listOfSecrets = new List<Secret>();
        for (int i = 0; i < read.dataData.listOfRevealedSecrets.Count; i++)
        {
            Secret secret = GameManager.i.dataScript.GetSecret(read.dataData.listOfRevealedSecrets[i]);
            if (secret != null)
            { listOfSecrets.Add(secret); }
        }
        GameManager.i.dataScript.SetListOfRevealedSecrets(listOfSecrets);
        //Secrets List -> DeletedSecrets
        listOfSecrets = new List<Secret>();
        for (int i = 0; i < read.dataData.listOfDeletedSecrets.Count; i++)
        {
            Secret secret = GameManager.i.dataScript.GetSecret(read.dataData.listOfDeletedSecrets[i]);
            if (secret != null)
            { listOfSecrets.Add(secret); }
        }
        GameManager.i.dataScript.SetListOfDeletedSecrets(listOfSecrets);
        #endregion

        #region investigations
        GameManager.i.dataScript.SetListOfCompletedInvestigations(read.dataData.listOfInvestigations);
        #endregion

        #region awards
        GameManager.i.dataScript.SetListOfCommendations(read.dataData.listOfCommendations);
        GameManager.i.dataScript.SetListOfBlackmarks(read.dataData.listOfBlackmarks);
        #endregion

        #region organisations
        string orgName;
        List<Organisation> listOfCurrentOrganisations = new List<Organisation>();
        for (int i = 0; i < read.dataData.listOfCurrentOrganisations.Count; i++)
        {
            orgName = read.dataData.listOfCurrentOrganisations[i];
            if (string.IsNullOrEmpty(orgName) == false)
            {
                //get org from dict
                Organisation org = GameManager.i.dataScript.GetOrganisaton(orgName);
                if (org != null)
                {
                    //get dynamic data
                    SaveOrganisation saveOrg = read.dataData.listOfSaveOrganisations.Find(x => x.name.Equals(orgName, StringComparison.Ordinal));
                    if (saveOrg != null)
                    {
                        //copy across dynamic data
                        org.isContact = saveOrg.isContact;
                        org.isCutOff = saveOrg.isCutOff;
                        org.isSecretKnown = saveOrg.isSecretKnown;
                        org.maxStat = saveOrg.maxStat;
                        org.SetReputation(saveOrg.reputation);
                        org.SetFreedom(saveOrg.freedom);
                        org.timer = saveOrg.timer;
                        //add org to list
                        listOfCurrentOrganisations.Add(org);
                    }
                    else { Debug.LogWarningFormat("Invalid saveOrg in listOfSaveOrganisations for orgName \"{0}\"", orgName); }
                }
                else { Debug.LogWarningFormat("Invalid org (Null) for orgName \"{0}\"", orgName); }
            }
            else { Debug.LogWarningFormat("Invalid orgName (Null or Empty) for listOfCurrentOrganisations[{0}]", i); }
        }
        //update DM -> listOfCurrentOrganisations
        GameManager.i.dataScript.SetListOfCurrentOrganisation(listOfCurrentOrganisations);
        //OrgData lists
        GameManager.i.dataScript.SetOrgData(read.dataData.listOfCureOrgData, OrganisationType.Cure);
        GameManager.i.dataScript.SetOrgData(read.dataData.listOfContractOrgData, OrganisationType.Contract);
        GameManager.i.dataScript.SetOrgData(read.dataData.listOfEmergencyOrgData, OrganisationType.Emergency);
        GameManager.i.dataScript.SetOrgData(read.dataData.listOfHQOrgData, OrganisationType.HQ);
        GameManager.i.dataScript.SetOrgData(read.dataData.listOfInfoOrgData, OrganisationType.Info);
        //OrgInfoArray
        GameManager.i.dataScript.SetOrgInfoArray(read.dataData.listOfOrgInfoData);
        #endregion

        #region metaOptions
        if (read.dataData.listOfMetaOptions != null)
        {
            for (int i = 0; i < read.dataData.listOfMetaOptions.Count; i++)
            {
                //copy across dynamic data
                SaveMetaOption metaOption = read.dataData.listOfMetaOptions[i];
                if (metaOption != null)
                { GameManager.i.dataScript.LoadMetaOptionData(metaOption); }
                else { Debug.LogWarningFormat("Invalid metaOption (Null) for listOfMetaOptions[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfMetaOptions (Null)"); }
        #endregion

        #region cures
        if (read.dataData.listOfCures != null)
        {
            for (int i = 0; i < read.dataData.listOfCures.Count; i++)
            {
                //copy across dynamic data
                SaveCure saveCure = read.dataData.listOfCures[i];
                if (saveCure != null)
                { GameManager.i.dataScript.LoadCureData(saveCure); }
                else { Debug.LogWarningFormat("Invalid saveCure in listOfCures[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid listOfCures (Null)"); }
        #endregion

        #region contacts
        //
        // - - - Contacts
        //
        //main contact dictionary (do first)
        Dictionary<int, Contact> dictOfContacts = GameManager.i.dataScript.GetDictOfContacts();
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
        List<int> listOfContactPool = GameManager.i.dataScript.GetContactPool();
        if (listOfContactPool != null)
        {
            listOfContactPool.Clear();
            listOfContactPool.AddRange(read.dataData.listOfContactPool);
        }
        else { Debug.LogError("Invalid listOfContactPool (Null)"); }
        //dictOfActorContacts
        Dictionary<int, List<int>> dictOfActorContacts = GameManager.i.dataScript.GetDictOfActorContacts();
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
        Dictionary<int, List<int>> dictOfNodeContactsResistance = GameManager.i.dataScript.GetDictOfNodeContacts(globalResistance);
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
        Dictionary<int, List<int>> dictOfNodeContactsAuthority = GameManager.i.dataScript.GetDictOfNodeContacts(globalAuthority);
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
        Dictionary<int, List<Contact>> dictOfContactsByNodeResistance = GameManager.i.dataScript.GetDictOfContactsByNodeResistance();
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
        Dictionary<int, Team> dictOfTeams = GameManager.i.dataScript.GetDictOfTeams();
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
                    TeamArc arc = GameManager.i.dataScript.GetTeamArc(saveTeam.arcID);
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
        int[,] arrayOfTeams = GameManager.i.dataScript.GetArrayOfTeams();
        if (arrayOfTeams != null)
        {
            //check bounds match are the same as the ones the save data used
            int index;
            int numOfArcs = GameManager.i.dataScript.CheckNumOfNodeArcs();
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
            GameManager.i.dataScript.SetTeamPool(TeamPool.Reserve, teamPool);
        }
        else { Debug.LogError("Invalid teampPoolReserve list (Null)"); }
        //list -> OnMap
        teamPool = read.dataData.listOfTeamPoolOnMap;
        if (teamPool != null)
        {
            //clear list and copy across data
            GameManager.i.dataScript.SetTeamPool(TeamPool.OnMap, teamPool);
        }
        else { Debug.LogError("Invalid teampPoolReserve list (Null)"); }
        //list -> InTransit
        teamPool = read.dataData.listOfTeamPoolInTransit;
        if (teamPool != null)
        {
            //clear list and copy across data
            GameManager.i.dataScript.SetTeamPool(TeamPool.InTransit, teamPool);
        }
        else { Debug.LogError("Invalid teampPoolReserve list (Null)"); }
        //team counter
        GameManager.i.teamScript.teamIDCounter = read.dataData.teamCounter;
        #endregion

        #region AI
        //array -> AI Resources
        int[] arrayOfAIResources = GameManager.i.dataScript.GetArrayOfAIResources();
        if (arrayOfAIResources != null)
        {
            int lengthOfArray = arrayOfAIResources.Length;
            //clear array & copy data across
            Array.Clear(arrayOfAIResources, 0, lengthOfArray);
            Debug.AssertFormat(lengthOfArray == read.dataData.listOfArrayOfAIResources.Count, "Mismatch on size, array {0}, list {1}", lengthOfArray, read.dataData.listOfArrayOfAIResources.Count);
            for (int i = 0; i < lengthOfArray; i++)
            { arrayOfAIResources[i] = read.dataData.listOfArrayOfAIResources[i]; }
        }
        else { Debug.LogError("Invalid arrayOfAIResources (Null)"); }
        //queue -> recentNodes
        Queue<AITracker> queueOfRecentNodes = GameManager.i.dataScript.GetRecentNodesQueue();
        if (queueOfRecentNodes != null)
        {
            //clear queue and copy data across
            queueOfRecentNodes.Clear();
            for (int i = 0; i < read.dataData.listOfRecentNodes.Count; i++)
            { queueOfRecentNodes.Enqueue(read.dataData.listOfRecentNodes[i]); }
        }
        else { Debug.LogError("Invalid queueOfRecentNodes (Null)"); }
        //queue -> recentConnections
        Queue<AITracker> queueOfRecentConnections = GameManager.i.dataScript.GetRecentConnectionsQueue();
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
        Dictionary<int, Message> dictOfArchiveMessages = GameManager.i.dataScript.GetMessageDict(MessageCategory.Archive);
        if (dictOfArchiveMessages != null)
        {
            //clear dict and copy across data
            dictOfArchiveMessages.Clear();
            countKey = read.dataData.listOfArchiveMessagesKey.Count;
            countValue = read.dataData.listOfArchiveMessagesValue.Count;
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
        Dictionary<int, Message> dictOfPendingMessages = GameManager.i.dataScript.GetMessageDict(MessageCategory.Pending);
        if (dictOfPendingMessages != null)
        {
            //clear dict and copy across data
            dictOfPendingMessages.Clear();
            countKey = read.dataData.listOfPendingMessagesKey.Count;
            countValue = read.dataData.listOfPendingMessagesValue.Count;
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
        Dictionary<int, Message> dictOfCurrentMessages = GameManager.i.dataScript.GetMessageDict(MessageCategory.Current);
        if (dictOfCurrentMessages != null)
        {
            //clear dict and copy across data
            dictOfCurrentMessages.Clear();
            countKey = read.dataData.listOfCurrentMessagesKey.Count;
            countValue = read.dataData.listOfCurrentMessagesValue.Count;
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
        Dictionary<int, Message> dictOfAIMessages = GameManager.i.dataScript.GetMessageDict(MessageCategory.AI);
        if (dictOfAIMessages != null)
        {
            //clear dict and copy across data
            dictOfAIMessages.Clear();
            countKey = read.dataData.listOfAIMessagesKey.Count;
            countValue = read.dataData.listOfAIMessagesValue.Count;
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
        GameManager.i.messageScript.messageIDCounter = read.dataData.messageIDCounter;
        #endregion

        #region topics
        //Topic Types
        Dictionary<string, TopicTypeData> dictOfTopicTypes = GameManager.i.dataScript.GetDictOfTopicTypeData();
        if (dictOfTopicTypes != null)
        {
            dictOfTopicTypes.Clear();
            countKey = read.dataData.listOfTopicTypeKeys.Count;
            countValue = read.dataData.listOfTopicTypeValues.Count;
            Debug.AssertFormat(countKey == countValue, "Mismatch on count for TopicType Lists ->  Keys {0} and Values {1}", countKey, countValue);
            for (int i = 0; i < countKey; i++)
            {
                TopicTypeData topicTypeData = read.dataData.listOfTopicTypeValues[i];
                itemName = read.dataData.listOfTopicTypeKeys[i];
                if (topicTypeData != null)
                {
                    if (string.IsNullOrEmpty(itemName) == false)
                    {
                        //add to dictionary
                        try { dictOfTopicTypes.Add(itemName, topicTypeData); }
                        catch (ArgumentException)
                        { Debug.LogWarningFormat("Duplicate topicType key exists for topicType \"{0}\"", itemName); }
                    }
                    else { Debug.LogWarningFormat("Invalid Topic Type Key/name (Null or Empty) for listOfTopicTypeKeys[{0}]", i); }
                }
                else { Debug.LogWarningFormat("Invalid topicTypeData (Null) for listOfTopicTypeValues[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfTopicTypes (Null)"); }
        //Topic Sub Types
        Dictionary<string, TopicTypeData> dictOfTopicSubTypes = GameManager.i.dataScript.GetDictOfTopicSubTypeData();
        if (dictOfTopicSubTypes != null)
        {
            dictOfTopicSubTypes.Clear();
            countKey = read.dataData.listOfTopicSubTypeKeys.Count;
            countValue = read.dataData.listOfTopicSubTypeValues.Count;
            Debug.AssertFormat(countKey == countValue, "Mismatch on count for TopicSubType Lists ->  Keys {0} and Values {1}", countKey, countValue);
            for (int i = 0; i < countKey; i++)
            {
                TopicTypeData topicTypeData = read.dataData.listOfTopicSubTypeValues[i];
                itemName = read.dataData.listOfTopicSubTypeKeys[i];
                if (topicTypeData != null)
                {
                    if (string.IsNullOrEmpty(itemName) == false)
                    {
                        //add to dictionary
                        try { dictOfTopicSubTypes.Add(itemName, topicTypeData); }
                        catch (ArgumentException)
                        { Debug.LogWarningFormat("Duplicate topicSubType key exists for topicSubType \"{0}\"", itemName); }
                    }
                    else { Debug.LogWarningFormat("Invalid Topic Type Key/name (Null or Empty) for listOfTopicSubTypeKeys[{0}]", i); }
                }
                else { Debug.LogWarningFormat("Invalid topicTypeData (Null) for listOfTopicSubTypeValues[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfTopicSubTypes (Null)"); }
        //Topic History
        Dictionary<int, HistoryTopic> dictOfTopicHistory = GameManager.i.dataScript.GetDictOfTopicHistory();
        if (dictOfTopicHistory != null)
        {
            dictOfTopicHistory.Clear();
            countKey = read.dataData.listOfTopicHistoryKeys.Count;
            countValue = read.dataData.listOfTopicHistoryValues.Count;
            Debug.AssertFormat(countKey == countValue, "Mismatch on count for topicHistory Lists ->  Keys {0} and Values {1}", countKey, countValue);
            for (int i = 0; i < countKey; i++)
            {
                HistoryTopic history = read.dataData.listOfTopicHistoryValues[i];
                if (history != null)
                {
                    //add to dictionary
                    try { dictOfTopicHistory.Add(read.dataData.listOfTopicHistoryKeys[i], history); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate topicHistory key exists for turn \"{0}\"", read.dataData.listOfTopicHistoryKeys[i]); }
                }
                else { Debug.LogWarningFormat("Invalid historyTopic (Null) for turn {0}", read.dataData.listOfTopicHistoryKeys[i]); }
            }
        }
        else { Debug.LogError("Invalid dictOfTopicHistory (Null)"); }
        //listOfTopicTypesLevel
        string topicTypeName;
        List<TopicType> listOfTopicTypes = new List<TopicType>();
        for (int i = 0; i < read.dataData.listOfTopicTypesLevel.Count; i++)
        {
            topicTypeName = read.dataData.listOfTopicTypesLevel[i];
            if (string.IsNullOrEmpty(topicTypeName) == false)
            {
                TopicType topicType = GameManager.i.dataScript.GetTopicType(topicTypeName);
                if (topicType != null)
                { listOfTopicTypes.Add(topicType); }
            }
            else { Debug.LogWarningFormat("Invalid topicTypeName (Null) for listOfTopicTypesLevel[{0}]", i); }
        }
        GameManager.i.dataScript.SetListOfTopicTypesLevel(listOfTopicTypes);
        //dictOfTopicPools
        Dictionary<string, List<Topic>> dictOfTopicPools = GameManager.i.dataScript.GetDictOfTopicPools();
        if (dictOfTopicPools != null)
        {
            string subTypeName;
            //clear dictionary
            dictOfTopicPools.Clear();
            List<Topic> listOfTopics = new List<Topic>();
            //copy saved data across
            for (int i = 0; i < read.dataData.listOfTopicPoolsKeys.Count; i++)
            {
                subTypeName = read.dataData.listOfTopicPoolsKeys[i];
                //clear out tempList of topics for topicSubType
                listOfTopics.Clear();
                List<string> listOfTopicNames = read.dataData.listOfTopicPoolsValue[i].myList;
                if (listOfTopicNames != null)
                {
                    countKey = listOfTopicNames.Count;
                    if (countKey > 0)
                    {
                        //refill with topics based on saved topic names
                        for (int j = 0; j < countKey; j++)
                        {
                            //Generate topics from topicNames
                            Topic topic = GameManager.i.dataScript.GetTopic(listOfTopicNames[j]);
                            if (topic != null)
                            { listOfTopics.Add(topic); }
                            else { Debug.LogWarningFormat("Invalid topic (Null) for topic \"{0}\" for topicSubType \"{1}\"", listOfTopicNames[j], subTypeName); }
                        }
                    }
                }
                else { Debug.LogWarningFormat("Invalid listOfTppicNames (Null) for topicSubType \"{0}\"", subTypeName); }
                //add to dict
                GameManager.i.dataScript.AddListOfTopicsToPool(subTypeName, listOfTopics);
            }
        }
        else { Debug.LogError("Invalid dictOfTopicPools (Null)"); }

        #endregion

        #region relations
        GameManager.i.dataScript.SetDictOfRelations(read.dataData.listOfRelationshipKeys, read.dataData.listOfRelationshipValues);
        #endregion

        #region registers
        //ongoing effects
        Dictionary<int, EffectDataOngoing> dictOfOngoing = GameManager.i.dataScript.GetDictOfOngoingEffects();
        if (dictOfOngoing != null)
        {
            //clear dict and copy data across
            dictOfOngoing.Clear();
            for (int i = 0; i < read.dataData.listOfOngoingEffects.Count; i++)
            {
                EffectDataOngoing effect = read.dataData.listOfOngoingEffects[i];
                if (effect != null)
                {
                    try
                    { dictOfOngoing.Add(effect.ongoingID, effect); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate ongoingID {0}", effect.ongoingID); }
                }
                else { Debug.LogWarningFormat("Invalid effectDataOngoing (Null) for read.dataData.listOfOngoingEffects[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfOngoing (Null)"); }
        //action adjustments
        GameManager.i.dataScript.SetListOfActionAdjustments(read.dataData.listOfActionAdjustments);

        #endregion

        /*#region moveNodes
        GameManager.i.dataScript.SetListOfMoveNodes(read.dataData.listOfMoveNodes);
        #endregion*/

        #region infoPipeLine
        if (read.dataData.listOfInfoPipelineDetails != null && read.dataData.listOfInfoPipelineDetails.Count > 0)
        { GameManager.i.guiScript.SetDictOfPipeline(read.dataData.listOfInfoPipelineDetails); }
        #endregion

        #region mainInfoApp
        GameManager.i.dataScript.SetListOfDelayedItemData(read.dataData.listOfDelayedItemData);
        //main info
        MainInfoData data = GameManager.i.dataScript.GetCurrentInfoData();
        if (data != null)
        {
            if (read.dataData.currentInfo != null)
            { TransferMainInfoData(read.dataData.currentInfo, data); }
            else { Debug.LogWarning("Invalid currentInfo (Null)"); }
        }
        else { Debug.LogError("Invalid currentInfoData (Null)"); }
        //dictOfHistory
        Dictionary<int, MainInfoData> dictOfHistory = GameManager.i.dataScript.GetDictOfHistory();
        if (dictOfHistory != null)
        {
            //clear dictionary
            dictOfHistory.Clear();
            //copy across loaded save game data
            int count = read.dataData.listOfHistoryKey.Count;
            Debug.AssertFormat(count == read.dataData.listOfHistoryValue.Count, "Mismatch on size: listOfKeys {0}, listOfValues {1}", count, read.dataData.listOfHistoryValue.Count);
            for (int i = 0; i < read.dataData.listOfHistoryKey.Count; i++)
            {
                SaveMainInfo mainInfo = read.dataData.listOfHistoryValue[i];
                if (mainInfo != null)
                {
                    MainInfoData mainData = new MainInfoData();
                    mainData.tickerText = mainInfo.tickerText;
                    TransferMainInfoData(mainInfo, mainData);
                    //Add to dictionary
                    try { dictOfHistory.Add(read.dataData.listOfHistoryKey[i], mainData); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate Entry for dictOfHistory turn {0}", read.dataData.listOfHistoryKey[i]); }
                }
                else { Debug.LogWarningFormat("Invalid SaveMainInfo (Null) for turn {0}", read.dataData.listOfHistoryKey[i]); }
            }
        }
        else { Debug.LogError("Invalid dictOfHistory (Null)"); }
        //arrayOfItemDataByPriority
        List<ItemData>[,] arrayOfItemDataByPriority = GameManager.i.dataScript.GetArrayOfItemDataByPriority();
        if (arrayOfItemDataByPriority != null)
        {
            int count;
            for (int outer = 0; outer < (int)ItemTab.Count; outer++)
            {
                SavePriorityInfo prioritySave = read.dataData.listOfPriorityData[outer];
                if (prioritySave != null)
                {
                    for (int inner = 0; inner < (int)ItemPriority.Count; inner++)
                    {
                        //clear list
                        arrayOfItemDataByPriority[outer, inner].Clear();
                        //copy across loaded save game data
                        switch (inner)
                        {
                            case 0:
                                count = prioritySave.listOfPriorityLow.Count;
                                //loop list and update sprite
                                for (int i = 0; i < count; i++)
                                {
                                    ItemData itemData = prioritySave.listOfPriorityLow[i];
                                    if (itemData != null)
                                    {
                                        //sprite
                                        itemData.sprite = GameManager.i.dataScript.GetSprite(itemData.spriteName);
                                        if (itemData.sprite == null)
                                        { itemData.sprite = defaultSprite; }
                                        //add to array list
                                        arrayOfItemDataByPriority[outer, inner].Add(itemData);
                                    }
                                    else { Debug.LogWarningFormat("Invalid itemData (Null) for listOfPriortyLow[{0}]", i); }
                                }
                                break;
                            case 1:
                                count = prioritySave.listOfPriorityMed.Count;
                                //loop list and update sprite
                                for (int i = 0; i < count; i++)
                                {
                                    ItemData itemData = prioritySave.listOfPriorityMed[i];
                                    if (itemData != null)
                                    {
                                        //sprite
                                        itemData.sprite = GameManager.i.dataScript.GetSprite(itemData.spriteName);
                                        if (itemData.sprite == null)
                                        { itemData.sprite = defaultSprite; }
                                        //add to array list
                                        arrayOfItemDataByPriority[outer, inner].Add(itemData);
                                    }
                                    else { Debug.LogWarningFormat("Invalid itemData (Null) for listOfPriortyMed[{0}]", i); }
                                }
                                break;
                            case 2:
                                count = prioritySave.listOfPriorityHigh.Count;
                                //loop list and update sprite
                                for (int i = 0; i < count; i++)
                                {
                                    ItemData itemData = prioritySave.listOfPriorityHigh[i];
                                    if (itemData != null)
                                    {
                                        //sprite
                                        itemData.sprite = GameManager.i.dataScript.GetSprite(itemData.spriteName);
                                        if (itemData.sprite == null)
                                        { itemData.sprite = defaultSprite; }
                                        //add to array list
                                        arrayOfItemDataByPriority[outer, inner].Add(itemData);
                                    }
                                    else { Debug.LogWarningFormat("Invalid itemData (Null) for listOfPriortyHigh[{0}]", i); }
                                }
                                break;
                            default: Debug.LogErrorFormat("Unrecognised inner index {0}", inner); break;
                        }
                    }
                }
                else { Debug.LogErrorFormat("Invalid SavePriorityInfo (Null) for outer index {0}", outer); }
            }
        }
        else { Debug.LogError("Invalid arrayOfItemDataByPriority (Null)"); }
        #endregion

        #region history
        GameManager.i.dataScript.SetListOfHistoryRebelMove(read.dataData.listOfHistoryRebel);
        GameManager.i.dataScript.SetListOfHistoryNemesisMove(read.dataData.listOfHistoryNemesis);
        GameManager.i.dataScript.SetListOfHistoryNpcMove(read.dataData.listOfHistoryVip);
        GameManager.i.dataScript.SetListOfHistoryPlayer(read.dataData.listOfHistoryPlayer);
        GameManager.i.dataScript.SetDictOfCampaignHistory(read.dataData.listOfHistoryLevel);
        #endregion

        #region News
        GameManager.i.dataScript.SetListOfNewsItems(read.dataData.listOfNewsItems);
        GameManager.i.dataScript.SetListOfAdverts(read.dataData.listOfAdverts);
        #endregion

        #region textLists
        //Text Lists -> update indexes
        Dictionary<string, TextList> dictOfTextLists = GameManager.i.dataScript.GetDictOfTextList();
        if (dictOfTextLists != null)
        {
            TextList textList;
            if (read.dataData.listOfTextListNames != null)
            {
                if (read.dataData.listOfTextListIndexes != null)
                {
                    for (int i = 0; i < read.dataData.listOfTextListNames.Count; i++)
                    {
                        itemName = read.dataData.listOfTextListNames[i];
                        if (string.IsNullOrEmpty(itemName) == false)
                        {
                            //find in dictionary
                            if (dictOfTextLists.ContainsKey(itemName) == true)
                            {
                                textList = dictOfTextLists[itemName];
                                if (textList != null)
                                { textList.index = read.dataData.listOfTextListIndexes[i]; }
                                else { Debug.LogWarningFormat("Invalid textList (Null) for \"{0}\"", itemName); }
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid textList name (Null or Empty) for listOfTextListNames[{0}]", i); }
                    }
                }
                else { Debug.LogWarning("Invalid listOfTextListIndexes (Null)"); }
            }
            else { Debug.LogWarning("Invalid listOfTextListNames (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfTextLists (Null)"); }
        #endregion
    }
    #endregion


    #region Read Actor Data
    /// <summary>
    /// Actor datasets for DataManager.cs collections
    /// </summary>
    private void ReadActorData()
    {
        //dictOfActors (load first)
        Dictionary<int, Actor> dictOfActors = GameManager.i.dataScript.GetDictOfActors();
        if (dictOfActors != null)
        {
            if (read.actorData.listOfDictActors != null)
            {
                //clear dictionary
                dictOfActors.Clear();
                //loop list of saved actors
                for (int i = 0; i < read.actorData.listOfDictActors.Count; i++)
                {
                    SaveActor readActor = read.actorData.listOfDictActors[i];
                    if (readActor != null)
                    {
                        Actor actor = ReadIndividualActor(readActor);
                        if (actor != null)
                        {
                            //Add to dictionary
                            try
                            { dictOfActors.Add(actor.actorID, actor); }
                            catch (ArgumentException)
                            { Debug.LogErrorFormat("Duplicate actor, ID \"{0}\"{1}", actor.actorID, "\n"); }
                        }
                        else { Debug.LogWarningFormat("Invalid actor (Null) for read.actorData.listOfDictActors[{0}]", i); }
                    }
                    else { Debug.LogWarningFormat("Invalid SaveActor (Null) for read.actorData.listOfDictActors[{0}]", i); }
                }
            }
            else { Debug.LogError("Invalid saveData.listOfDictActors (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfActors (Null)"); }
        //dictOfHQ
        Dictionary<int, Actor> dictOfHQ = GameManager.i.dataScript.GetDictOfHq();
        if (dictOfHQ != null)
        {
            if (read.actorData.listOfDictHQ != null)
            {
                //clear dictionary
                dictOfHQ.Clear();
                //loop list of saved actors
                for (int i = 0; i < read.actorData.listOfDictHQ.Count; i++)
                {
                    SaveActor readActor = read.actorData.listOfDictHQ[i];
                    if (readActor != null)
                    {
                        Actor actor = ReadIndividualActor(readActor);
                        if (actor != null)
                        {
                            //Add to dictionary
                            try
                            { dictOfHQ.Add(actor.hqID, actor); }
                            catch (ArgumentException)
                            { Debug.LogErrorFormat("Duplicate actor, hqID \"{0}\"{1}", actor.hqID, "\n"); }
                        }
                        else { Debug.LogWarningFormat("Invalid actor (Null) for read.actorData.listOfDictHQ[{0}]", i); }
                    }
                    else { Debug.LogWarningFormat("Invalid SaveActor (Null) for read.actorData.listOfDictHQ[{0}]", i); }
                }
            }
            else { Debug.LogError("Invalid saveData.listOfDictHQ (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfHQ (Null)"); }
        //
        // - - - Actor arrays
        //
        int index;
        int maxIndex;
        int actorID, hqID;
        int sideNum = GameManager.i.dataScript.GetNumOfGlobalSide();
        int actorNum = GameManager.i.actorScript.maxNumOfOnMapActors;
        if (read.actorData.listOfActors != null)
        {
            //arrayOfActors
            Actor[,] arrayOfActors = GameManager.i.dataScript.GetArrayOfActors();
            if (arrayOfActors != null)
            {
                maxIndex = arrayOfActors.Length;
                //empty out array
                Array.Clear(arrayOfActors, 0, arrayOfActors.Length);
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
                            actor = GameManager.i.dataScript.GetActor(actorID);
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
        bool[,] arrayOfActorsPresent = GameManager.i.dataScript.GetArrayOfActorsPresent();
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
        //arrayOfActorsHQ
        Actor[] arrayOfActorsHQ = GameManager.i.dataScript.GetArrayOfActorsHQ();
        if (arrayOfActorsHQ != null)
        {
            int records = arrayOfActorsHQ.Length;
            //empty out array
            Array.Clear(arrayOfActorsHQ, 0, records);
            for (int i = 0; i < records; i++)
            {
                hqID = read.actorData.listOfActorsHQ[i];
                if (hqID > -1)
                {
                    Actor actor = GameManager.i.dataScript.GetHqActor(hqID);
                    if (actor != null)
                    { arrayOfActorsHQ[i] = actor; }
                    else { Debug.LogWarningFormat("Invalid actor (Null) for hqID {0}", hqID); }
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfActorsHQ (Null)"); }
        //
        // - - - ActorManager.cs
        //
        GameManager.i.actorScript.actorIDCounter = read.actorData.actorIDCounter;
        GameManager.i.actorScript.hqIDCounter = read.actorData.hqIDCounter;
        GameManager.i.actorScript.lieLowTimer = read.actorData.lieLowTimer;
        GameManager.i.actorScript.doomTimer = read.actorData.doomTimer;
        GameManager.i.actorScript.captureTimerPlayer = read.actorData.captureTimer;
        GameManager.i.actorScript.isGearCheckRequired = read.actorData.isGearCheckRequired;
        NameSet nameSet = GameManager.i.dataScript.GetNameSet(read.actorData.nameSet);
        if (nameSet != null)
        { GameManager.i.actorScript.nameSet = nameSet; }
        else { Debug.LogWarningFormat("Invalid nameSet (Null) for \"{0}\"", read.actorData.nameSet); }
        //
        // - - - Actor Lists
        //
        List<int> authorityActorPoolLevelOne = GameManager.i.dataScript.GetActorRecruitPool(1, globalAuthority);
        List<int> authorityActorPoolLevelTwo = GameManager.i.dataScript.GetActorRecruitPool(2, globalAuthority);
        List<int> authorityActorPoolLevelThree = GameManager.i.dataScript.GetActorRecruitPool(3, globalAuthority);
        List<int> resistanceActorPoolLevelOne = GameManager.i.dataScript.GetActorRecruitPool(1, globalResistance);
        List<int> resistanceActorPoolLevelTwo = GameManager.i.dataScript.GetActorRecruitPool(2, globalResistance);
        List<int> resistanceActorPoolLevelThree = GameManager.i.dataScript.GetActorRecruitPool(3, globalResistance);
        List<int> authorityActorReserve = GameManager.i.dataScript.GetListOfReserveActors(globalAuthority);
        List<int> authorityActorDismissed = GameManager.i.dataScript.GetListOfDismissedActors(globalAuthority);
        List<int> authorityActorPromoted = GameManager.i.dataScript.GetListOfPromotedActors(globalAuthority);
        List<int> authorityActorDisposedOf = GameManager.i.dataScript.GetListOfDisposedOfActors(globalAuthority);
        List<int> authorityActorResigned = GameManager.i.dataScript.GetListOfResignedActors(globalAuthority);
        List<int> resistanceActorReserve = GameManager.i.dataScript.GetListOfReserveActors(globalResistance);
        List<int> resistanceActorDismissed = GameManager.i.dataScript.GetListOfDismissedActors(globalResistance);
        List<int> resistanceActorPromoted = GameManager.i.dataScript.GetListOfPromotedActors(globalResistance);
        List<int> resistanceActorDisposedOf = GameManager.i.dataScript.GetListOfDisposedOfActors(globalResistance);
        List<int> resistanceActorResigned = GameManager.i.dataScript.GetListOfResignedActors(globalResistance);
        List<int> actorHQPool = GameManager.i.dataScript.GetListOfActorHq();
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
        if (actorHQPool == null) { Debug.LogError("Invalid actorHQPool (Null)"); }
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
        actorHQPool.Clear();
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
        actorHQPool.AddRange(read.actorData.actorHQPool);

    }
    #endregion


    #region Read Node Data
    /// <summary>
    /// NodeManager.cs data to game
    /// </summary>
    private void ReadNodeData()
    {
        //NodeManager.cs dynamic data
        GameManager.i.nodeScript.crisisPolicyModifier = read.nodeData.crisisPolicyModifier;
        GameManager.i.nodeScript.nodeIDCounter = read.nodeData.nodeCounter;
        GameManager.i.nodeScript.connIDCounter = read.nodeData.connCounter;
        GameManager.i.nodeScript.nodeHighlight = read.nodeData.nodeHighlight;
        GameManager.i.nodeScript.nodePlayer = read.nodeData.nodePlayer;
        GameManager.i.nodeScript.nodeNemesis = read.nodeData.nodeNemesis;
        GameManager.i.nodeScript.nodeCaptured = read.nodeData.nodeCaptured;
        //update node data
        for (int i = 0; i < read.nodeData.listOfNodes.Count; i++)
        {
            SaveNode saveNode = read.nodeData.listOfNodes[i];
            if (saveNode != null)
            {
                //find equivalent inGame node
                Node node = GameManager.i.dataScript.GetNode(saveNode.nodeID);
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
                    node.targetName = saveNode.targetName;
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
                    node.defaultChar = saveNode.defaultChar;
                    //crisis
                    if (string.IsNullOrEmpty(saveNode.nodeCrisisName) == false)
                    {
                        NodeCrisis crisis = GameManager.i.dataScript.GetNodeCrisis(saveNode.nodeCrisisName);
                        if (crisis != null)
                        { node.crisis = crisis; }
                        else
                        {
                            Debug.LogWarningFormat("Invalid nodeCrisis (Null) for {0}, nodeID {1}, {2}, {3}", saveNode.nodeCrisisName, node.nodeID, node.nodeName, node.Arc.name);
                            node.crisis = null;
                        }
                    }
                    else { node.crisis = null; }
                    //cure
                    if (string.IsNullOrEmpty(saveNode.cureName) == true)
                    { node.cure = null; }
                    else
                    {
                        //get cure from dictionary
                        if (saveNode.cureName != null)
                        {
                            Cure cure = GameManager.i.dataScript.GetCure(saveNode.cureName);
                            if (cure == null)
                            { Debug.LogWarningFormat("Invalid cure (Null) for saveNode.cureName {0}", saveNode.cureName); }
                            node.cure = cure;
                        }
                        else { node.cure = null; }
                    }
                    node.loiter = saveNode.loiter;
                    //teams
                    int count = saveNode.listOfTeams.Count;
                    node.GetListOfTeams().Clear();
                    if (count > 0)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            Team team = GameManager.i.dataScript.GetTeam(saveNode.listOfTeams[j]);
                            if (team != null)
                            { node.LoadAddTeam(team); }
                            else { Debug.LogWarningFormat("Invalid team (Null) for listOfTeams[{0}] for nodeID {1}", j, saveNode.nodeID); }
                        }
                    }
                    //ongoing effects
                    count = saveNode.listOfOngoingEffects.Count;
                    node.GetListOfOngoingEffects().Clear();
                    if (count > 0)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            EffectDataOngoing effectOngoing = GameManager.i.dataScript.GetOngoingEffect(saveNode.listOfOngoingEffects[j]);
                            if (effectOngoing != null)
                            { node.LoadAddOngoingEffect(effectOngoing); }
                            else { Debug.LogWarningFormat("Invalid effectOngoing (Null) for listOfOngoingEffects[{0}] for nodeID {1}", j, saveNode.nodeID); }
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
        List<Node> listOfCrisisNodes = GameManager.i.dataScript.GetListOfCrisisNodes();
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
                    Node node = GameManager.i.dataScript.GetNode(read.nodeData.listOfCrisisNodes[i]);
                    if (node != null)
                    { listOfCrisisNodes.Add(node); }
                    else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", read.nodeData.listOfCrisisNodes[i]); }
                }
                //activate smoke
                GameManager.i.nodeScript.ProcessLoadNodeCrisis();
            }
        }
        else { Debug.LogError("Invalid listOfCrisisNodes (Null)"); }
        //
        // - - - Cure Nodes
        //
        List<Node> listOfCureNodes = GameManager.i.dataScript.GetListOfCureNodes();
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
                    Node node = GameManager.i.dataScript.GetNode(read.nodeData.listOfCureNodes[i]);
                    if (node != null)
                    { listOfCureNodes.Add(node); }
                    else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", read.nodeData.listOfCureNodes[i]); }
                }
            }
        }
        else { Debug.LogError("Invalid listOfCureNodes (Null)"); }
        //
        // - - - Move Nodes
        //
        List<Node> listOfMoveNodes = GameManager.i.dataScript.GetListOfMoveNodes();
        if (listOfMoveNodes != null)
        {
            //clear list
            listOfMoveNodes.Clear();
            int count = read.nodeData.listOfMoveNodes.Count;
            if (count > 0)
            {
                //repopulate list with save data
                for (int i = 0; i < count; i++)
                {
                    Node node = GameManager.i.dataScript.GetNode(read.nodeData.listOfMoveNodes[i]);
                    if (node != null)
                    { listOfMoveNodes.Add(node); }
                    else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", read.nodeData.listOfMoveNodes[i]); }
                }
            }
        }
        else { Debug.LogError("Invalid listOfMoveNodes (Null)"); }
    }
    #endregion'


    #region Read Connnection Data
    /// <summary>
    /// Connection.SO dynamic data
    /// </summary>
    private void ReadConnectionData()
    {
        for (int i = 0; i < read.connData.listOfConnectionData.Count; i++)
        {
            SaveConnection save = new SaveConnection();
            save = read.connData.listOfConnectionData[i];
            if (save != null)
            {
                //find record in dictionary
                Connection conn = GameManager.i.dataScript.GetConnection(save.connID);
                if (conn != null)
                {
                    //copy across loaded save game dynamic data
                    conn.ChangeSecurityLevel(save.securityLevel);
                    conn.activityCount = save.activityCount;
                    conn.activityTime = save.activityTime;
                    conn.SetListOfOngoingEffects(save.listOfOngoingEffects);
                }
                else { Debug.LogErrorFormat("Invalid connection (Null) for connID {0}", save.connID); }
            }
            else { Debug.LogWarningFormat("Invalid SaveConnection (Null) for listOfConnectionData[{0}]", i); }
        }

    }
    #endregion


    #region ReadGearData
    /// <summary>
    /// GearManager.cs & gear.cs dynamic data
    /// </summary>
    private void ReadGearData()
    {
        Dictionary<string, Gear> dictOfGear = GameManager.i.dataScript.GetDictOfGear();
        if (dictOfGear != null)
        {
            for (int i = 0; i < read.gearData.listOfGear.Count; i++)
            {
                SaveGear saveGear = read.gearData.listOfGear[i];
                if (saveGear != null)
                {
                    //find in dictionary
                    if (dictOfGear.ContainsKey(saveGear.gearName) == true)
                    {
                        Gear gear = dictOfGear[saveGear.gearName];
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
                        else { Debug.LogWarningFormat("Invalid gear (Null) in dict for gearID {0}", saveGear.gearName); }
                    }
                    else { Debug.LogWarningFormat("Gear not found in dictionary for gearID {0}", saveGear.gearName); }
                }
                else { Debug.LogWarningFormat("Invalid saveGear (Null) for read.gearData.listOfGear[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfGear (Null)"); }
        //list -> Common gear
        GearRarity rarity = GameManager.i.gearScript.gearCommon;
        List<string> tempList = GameManager.i.dataScript.GetListOfGear(rarity);
        if (tempList != null)
        { GameManager.i.dataScript.SetGearList(read.gearData.listOfCommonGear, rarity); }
        else { Debug.LogError("Invalid listOfCommonGear (Null)"); }
        //list -> Rare gear
        rarity = GameManager.i.gearScript.gearRare;
        tempList = GameManager.i.dataScript.GetListOfGear(rarity);
        if (tempList != null)
        { GameManager.i.dataScript.SetGearList(read.gearData.listOfRareGear, rarity); }
        else { Debug.LogError("Invalid listOfRareGear (Null)"); }
        //list -> Unique gear
        rarity = GameManager.i.gearScript.gearUnique;
        tempList = GameManager.i.dataScript.GetListOfGear(rarity);
        if (tempList != null)
        { GameManager.i.dataScript.SetGearList(read.gearData.listOfUniqueGear, rarity); }
        else { Debug.LogError("Invalid listOfUniqueGear (Null)"); }
        //list -> Current gear
        tempList = GameManager.i.dataScript.GetListOfCurrentGear();
        if (tempList != null)
        { GameManager.i.dataScript.SetListOfGearCurrent(read.gearData.listOfCurrentGear); }
        else { Debug.LogError("Invalid listOfCurrentGear (Null)"); }
        //list -> Lost gear
        tempList = GameManager.i.dataScript.GetListOfLostGear();
        if (tempList != null)
        { GameManager.i.dataScript.SetListOfGearLost(read.gearData.listOfLostGear); }
        else { Debug.LogError("Invalid listOfLostGear (Null)"); }
        //GearManager.cs fields
        GameManager.i.gearScript.SetGearSaveCurrentCost(read.gearData.gearSaveCurrentCost);
        GameManager.i.gearScript.SetListOfCompromisedGear(read.gearData.listOfCompromisedGear);
    }
    #endregion


    #region ReadTopicData
    /// <summary>
    /// Topic.cs dynamic data
    /// </summary>
    private void ReadTopicData()
    {
        //reset topics (isCurrent to False) prior to loading changes
        GameManager.i.dataScript.ResetTopics();
        //read in dynamic data
        Dictionary<string, Topic> dictOfTopics = GameManager.i.dataScript.GetDictOfTopics();
        if (dictOfTopics != null)
        {
            if (read.topicData.listOfTopics != null)
            {
                for (int i = 0; i < read.topicData.listOfTopics.Count; i++)
                {
                    SaveTopic saveTopic = read.topicData.listOfTopics[i];
                    if (saveTopic != null)
                    {
                        //update topic dynamic data
                        Topic topic = GameManager.i.dataScript.GetTopic(saveTopic.topicName);
                        if (topic != null)
                        {
                            topic.status = saveTopic.status;
                            topic.timerStart = saveTopic.timerStart;
                            topic.timerRepeat = saveTopic.timerRepeat;
                            topic.timerWindow = saveTopic.timerWindow;
                            topic.turnsDormant = saveTopic.turnsDormant;
                            topic.turnsActive = saveTopic.turnsActive;
                            topic.turnsLive = saveTopic.turnsLive;
                            topic.turnsDone = saveTopic.turnsDone;
                            //set topic to isCurrent true
                            topic.isCurrent = true;
                        }
                        else { Debug.LogWarningFormat("Invalid topic (Null) for saveTopic.topicName \"{0}\"", saveTopic.topicName); }
                    }
                    else { Debug.LogWarningFormat("Invalid saveTopic (Null) for listOfTopics[{0}]", i); }
                }
            }
            else { Debug.LogError("Invalid read.topicData.listOfTopics (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfTopics"); }
    }
    #endregion


    #region Read Nemesis Data
    /// <summary>
    /// NemesisManager.cs data to game
    /// </summary>
    private void ReadNemesisData()
    {
        if (read.nemesisData.saveData != null)
        { GameManager.i.nemesisScript.ReadSaveData(read.nemesisData.saveData); }
        else { Debug.LogError("Invalid read.nemesisData.saveData (Null)"); }
    }
    #endregion


    #region Read Contact Data
    /// <summary>
    /// ContactManager.cs data to game
    /// </summary>
    private void ReadContactData()
    {
        GameManager.i.contactScript.contactIDCounter = read.contactData.contactIDCounter;
        GameManager.i.contactScript.SetArrayOfContactNetworks(read.contactData.listOfContactNetworks);
        GameManager.i.contactScript.SetArrayOfActors(read.contactData.listOfActors);
    }
    #endregion


    #region Read AI Data
    /// <summary>
    /// read AI/Rebel Manager.cs
    /// </summary>
    private void ReadAIData()
    {
        //AIManager -> collection
        GameManager.i.aiScript.SetListOfTasksFinal(read.aiData.listOfTasksFinal);
        GameManager.i.aiScript.SetListOfTasksPotential(read.aiData.listOfTasksPotential);
        GameManager.i.aiScript.SetListOfPlayerEffects(read.aiData.listOfPlayerEffects);
        GameManager.i.aiScript.SetListOfPlayerEffectDescriptors(read.aiData.listOfPlayerEffectDescriptors);
        GameManager.i.aiScript.SetArrayOfAITaskTypes(read.aiData.listOfAITaskTypesAuthority);
        //
        // - - - update displays -> load game could have been called from a hot key or the main menu
        //
        GameState controlState = GameManager.i.controlScript.GetExistingGameState();
        //resistance player only
        if (controlState == GameState.PlayGame && GameManager.i.sideScript.PlayerSide.level == 2)
        {
            GameManager.i.aiScript.UpdateTaskDisplayData();
            GameManager.i.aiScript.UpdateSideTabData();
            GameManager.i.aiScript.UpdateBottomTabData();
        }
        //AIManager.cs -> public fields
        GameManager.i.aiScript.immediateFlagAuthority = read.aiData.immediateFlagAuthority;
        GameManager.i.aiScript.immediateFlagResistance = read.aiData.immediateFlagResistance;
        GameManager.i.aiScript.resourcesGainAuthority = read.aiData.resourcesGainAuthority;
        GameManager.i.aiScript.resourcesGainResistance = read.aiData.resourcesGainResistance;
        GameManager.i.aiScript.aiTaskCounter = read.aiData.aiTaskCounter;
        GameManager.i.aiScript.hackingAttemptsTotal = read.aiData.hackingAttemptsTotal;
        GameManager.i.aiScript.hackingAttemptsReboot = read.aiData.hackingAttemptsReboot;
        GameManager.i.aiScript.hackingAttemptsDetected = read.aiData.hackingAttemptsDetected;
        GameManager.i.aiScript.hackingCurrentCost = read.aiData.hackingCurrentCost;
        GameManager.i.aiScript.hackingModifiedCost = read.aiData.hackingModifiedCost;
        GameManager.i.aiScript.isHacked = read.aiData.isHacked;
        GameManager.i.aiScript.aiAlertStatus = read.aiData.aiAlertStatus;
        GameManager.i.aiScript.isRebooting = read.aiData.isRebooting;
        GameManager.i.aiScript.rebootTimer = read.aiData.rebootTimer;
        GameManager.i.aiScript.numOfCrisis = read.aiData.numOfCrisis;
        GameManager.i.aiScript.status = read.aiData.status;
        GameManager.i.aiScript.inactiveStatus = read.aiData.inactiveStatus;
        //AIManager.cs -> private fields
        GameManager.i.aiScript.LoadReadData(read.aiData.saveAI);
        //AIRebelManager.cs -> private fields
        GameManager.i.aiRebelScript.ReadSaveData(read.aiData.saveRebel);
        //AIRebelManager -> Nemesis Reports
        GameManager.i.aiRebelScript.SetListOfNemesisReports(read.aiData.listOfNemesisReports);
        //AIRebelManager -> Erasure Reports
        GameManager.i.aiRebelScript.SetListOfErasureReports(read.aiData.listOfErasureReports);
        GameManager.i.aiRebelScript.SetArrayOfAITaskTypes(read.aiData.listOfAITaskTypesRebel);
    }
    #endregion


    #region Read Target Data
    /// <summary>
    /// read TargetManager.cs data
    /// </summary>
    private void ReadTargetData()
    {
        //targetManager.cs
        GameManager.i.targetScript.StartTargets = read.targetData.startTargets;
        GameManager.i.targetScript.ActiveTargets = read.targetData.activeTargets;
        GameManager.i.targetScript.LiveTargets = read.targetData.liveTargets;
        GameManager.i.targetScript.MaxTargets = read.targetData.maxTargets;
        GameManager.i.targetScript.targetOrgName = read.targetData.targetOrgName;
        //dynamic Target.SO data
        Dictionary<string, Target> dictOfTargets = GameManager.i.dataScript.GetDictOfTargets();
        if (dictOfTargets != null)
        {
            for (int i = 0; i < read.targetData.listOfTargets.Count; i++)
            {
                SaveTarget save = read.targetData.listOfTargets[i];
                if (save != null)
                {
                    //find target in dictionary
                    Target target = GameManager.i.dataScript.GetTarget(save.targetName);
                    if (target != null)
                    {
                        //copy across loaded save game dynamic data
                        target.targetStatus = save.targetStatus;
                        target.intel = save.intel;
                        target.ongoingID = save.ongoingID;
                        target.isKnownByAI = save.isKnownByAI;
                        target.nodeID = save.nodeID;
                        target.distance = save.distance;
                        target.newIntel = save.newIntel;
                        target.intelGain = save.intelGain;
                        target.turnSuccess = save.turnSuccess;
                        target.turnDone = save.turnDone;
                        target.numOfAttempts = save.numOfAttempts;
                        target.turnsWindow = save.turnsWindow;
                        target.timerDelay = save.timerDelay;
                        target.timerHardLimit = save.timerHardLimit;
                        target.timerWindow = save.timerWindow;
                    }
                    else { Debug.LogErrorFormat("Invalid target (Null) for target {0}", save.targetName); }
                }
                else { Debug.LogErrorFormat("Invalid SaveTarget (Null) for listOfTargets[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfTargets (Null)"); }
        //arrayOfGenericTargets
        List<string>[] arrayOfGenericTargets = GameManager.i.dataScript.GetArrayOfGenericTargets();
        if (arrayOfGenericTargets != null)
        {
            int count = arrayOfGenericTargets.Length;
            //clear out array
            Array.Clear(arrayOfGenericTargets, 0, count);
            Debug.AssertFormat(count == read.targetData.listOfGenericTargets.Count, "Mismatch on size: array {0}, list {1}", count, read.targetData.listOfGenericTargets.Count);
            //copy across loaded save game lists into array
            for (int i = 0; i < count; i++)
            {
                List<string> listOfTargets = read.targetData.listOfGenericTargets[i].myList;
                if (listOfTargets != null)
                { arrayOfGenericTargets[i] = listOfTargets; }
                else { Debug.LogWarningFormat("Invalid listOfTargets (Null) for read.targetData.listOfGenericTargets[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid arrayOfGenericTargets (Null)"); }
        //listOfNodesWithTargets
        GameManager.i.dataScript.SetListOfNodesWithTargets(read.targetData.listOfNodesWithTargets);
        //target Pool -> Active
        List<Target> listOfActive = new List<Target>();
        for (int i = 0; i < read.targetData.listOfTargetPoolActive.Count; i++)
        {
            Target target = GameManager.i.dataScript.GetTarget(read.targetData.listOfTargetPoolActive[i]);
            if (target != null)
            { listOfActive.Add(target); }
            else { Debug.LogWarningFormat("Invalid target (Null) for listOfTargetPoolActive[{0}]", i); }
        }
        GameManager.i.dataScript.SetTargetPool(listOfActive, Status.Active);
        //target Pool -> Live
        List<Target> listOfLive = new List<Target>();
        for (int i = 0; i < read.targetData.listOfTargetPoolLive.Count; i++)
        {
            Target target = GameManager.i.dataScript.GetTarget(read.targetData.listOfTargetPoolLive[i]);
            if (target != null)
            { listOfLive.Add(target); }
            else { Debug.LogWarningFormat("Invalid target (Null) for listOfTargetPoolLive[{0}]", i); }
        }
        GameManager.i.dataScript.SetTargetPool(listOfLive, Status.Live);
        //target Pool -> Outstanding
        List<Target> listOfOutstanding = new List<Target>();
        for (int i = 0; i < read.targetData.listOfTargetPoolOutstanding.Count; i++)
        {
            Target target = GameManager.i.dataScript.GetTarget(read.targetData.listOfTargetPoolOutstanding[i]);
            if (target != null)
            { listOfOutstanding.Add(target); }
            else { Debug.LogWarningFormat("Invalid target (Null) for listOfTargetPoolOutstanding[{0}]", i); }
        }
        GameManager.i.dataScript.SetTargetPool(listOfOutstanding, Status.Outstanding);
        //target Pool -> Done
        List<Target> listOfDone = new List<Target>();
        for (int i = 0; i < read.targetData.listOfTargetPoolDone.Count; i++)
        {
            Target target = GameManager.i.dataScript.GetTarget(read.targetData.listOfTargetPoolDone[i]);
            if (target != null)
            { listOfDone.Add(target); }
            else { Debug.LogWarningFormat("Invalid target (Null) for listOfTargetPoolDone[{0}]", i); }
        }
        GameManager.i.dataScript.SetTargetPool(listOfDone, Status.Done);
    }
    #endregion


    #region Read Statistics Data
    /// <summary>
    /// read StatisticsManager.cs data
    /// </summary>
    private void ReadStatisticsData()
    {
        GameManager.i.statScript.ratioPlayerNodeActions = read.statisticsData.ratioPlayerNodeActions;
        GameManager.i.statScript.ratioPlayerTargetAttempts = read.statisticsData.ratioPlayerTargetAttempts;
        GameManager.i.statScript.ratioPlayerMoveActions = read.statisticsData.ratioPlayerMoveActions;
        GameManager.i.statScript.ratioPlayerLieLowDays = read.statisticsData.ratioPlayerLieLowDays;
        GameManager.i.statScript.ratioPlayerGiveGear = read.statisticsData.ratioPlayerGiveGear;
        GameManager.i.statScript.ratioPlayerManageActions = read.statisticsData.ratioPlayerManageActions;
        GameManager.i.statScript.ratioPlayerDoNothing = read.statisticsData.ratioPlayerDoNothing;
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
                GameManager.i.playerScript.tooltipStatus = ActorTooltip.None;
            }
            if (inactiveStatus != ActorInactive.None)
            {
                Debug.LogWarningFormat("Player status Active -> inactiveStatus \"{0}\" invalid, changed to 'None'{1}", inactiveStatus, "\n");
                GameManager.i.playerScript.inactiveStatus = ActorInactive.None;
            }
        }
        else if (status == ActorStatus.Inactive)
        {
            //secondary field doesn't match primary. Revert all to primary state.
            if (tooltipStatus == ActorTooltip.None || inactiveStatus == ActorInactive.None)
            {
                Debug.LogWarningFormat("Player INACTIVE -> tooltipStatus \"{0}\" Or inactiveStatus \"{1}\" invalid,   PlayerStatus changed to 'Active'{2}",
                    tooltipStatus, inactiveStatus, "\n");
                GameManager.i.playerScript.status = ActorStatus.Active;
                GameManager.i.playerScript.tooltipStatus = ActorTooltip.None;
                GameManager.i.playerScript.inactiveStatus = ActorInactive.None;
            }
            else
            {
                //change alpha of player to indicate inactive status
                GameManager.i.actorPanelScript.UpdatePlayerAlpha(GameManager.i.guiScript.alphaInactive);
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
        GameManager.i.actorPanelScript.UpdateActorPanel();
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        //loop each OnMap actor and update alpha and renown
        Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActors(playerSide);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.i.dataScript.CheckActorSlotStatus(i, playerSide) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    {
                        //update alpha
                        if (actor.Status == ActorStatus.Active)
                        { GameManager.i.actorPanelScript.UpdateActorAlpha(i, alphaActive); }
                        else
                        { GameManager.i.actorPanelScript.UpdateActorAlpha(i, alphaInactive); }
                        //update renown & compatibiliyt
                        GameManager.i.actorPanelScript.UpdateActorRenownUI(i, actor.Renown);
                        GameManager.i.actorPanelScript.UpdateActorCompatibilityUI(i, actor.GetPersonality().GetCompatibilityWithPlayer());
                    }
                }
                else
                {
                    //actor not present in slot, reset renown and compatibility to 0
                    GameManager.i.actorPanelScript.UpdateActorRenownUI(i, 0);
                    GameManager.i.actorPanelScript.UpdateActorCompatibilityUI(i, 0);
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }

    }
    #endregion

    //
    // - - - GUI - - -
    //

    #region UpdateGUI
    /// <summary>
    /// if loaded in player mode, update all relevent GUI elements (others may be updated by class specific methods above)
    /// </summary>
    private void UpdateGUI(GlobalSide side)
    {
        //
        // - - - Top Widget UI
        //
        TopWidgetData widget = new TopWidgetData();
        widget.side = side;
        widget.turn = read.gameData.turnData.turn;
        widget.actionPoints = read.gameData.turnData.actionsTotal - read.gameData.turnData.actionsCurrent;
        widget.cityLoyalty = read.scenarioData.cityLoyalty;
        //faction support depends on side
        switch (side.level)
        {
            case 1: widget.factionSupport = read.scenarioData.hqSupportAuthority; break;
            case 2: widget.factionSupport = read.scenarioData.hqSupportResistance; break;
            default: Debug.LogError("Unrecognised {0}", side); break;
        }
        widget.isSecurityFlash = read.gameData.isSecurityFlash;

        //objectives -> TO DO

        //Update top widget UI
        GameManager.i.widgetTopScript.LoadSavedData(widget);
        //
        // - - - Top Bar UI
        //
        GameManager.i.topBarScript.commendationData = read.guiData.commendationData;
        GameManager.i.topBarScript.blackmarkData = read.guiData.blackmarkData;
        GameManager.i.topBarScript.investigationData = read.guiData.investigationData;
        GameManager.i.topBarScript.innocenceData = read.guiData.innocenceData;
        GameManager.i.topBarScript.unhappyData = read.guiData.unhappyData;
        GameManager.i.topBarScript.conflictData = read.guiData.conflictData;
        GameManager.i.topBarScript.blackmailData = read.guiData.blackmailData;
        GameManager.i.topBarScript.doomData = read.guiData.doomData;
        //update top Bar with saved data
        GameManager.i.topBarScript.LoadSavedData();
    }
    #endregion

    //
    // - - - Utilities - - -
    //

    #region TransferMainInfoData
    /// <summary>
    /// subMethod to copy across SaveMainInfo data to a MainInfoData class
    /// NOTE: infoSave and data have been checked for Null by their parent methods
    /// </summary>
    private void TransferMainInfoData(SaveMainInfo infoSave, MainInfoData data)
    {
        //ticker text
        data.tickerText = infoSave.tickerText;
        //loop array and clear out each list before copying across loaded save game data
        for (int i = 0; i < (int)ItemTab.Count; i++)
        {
            List<ItemData> listOfItemData = data.arrayOfItemData[i];
            if (listOfItemData != null)
            {
                listOfItemData.Clear();
                switch (i)
                {
                    case 0:
                        listOfItemData.AddRange(infoSave.listOfTab0Item);
                        //loop items and replace sprite
                        for (int j = 0; j < listOfItemData.Count; j++)
                        {
                            ItemData item = listOfItemData[j];
                            if (item != null)
                            { item.sprite = GameManager.i.dataScript.GetSprite(item.spriteName); }
                            else
                            {
                                Debug.LogWarningFormat("Invalid ItemData (Null) in listOfItemData[{0}] for array[1]", j, i);
                                item.sprite = defaultSprite;
                            }
                        }
                        break;
                    case 1:
                        listOfItemData.AddRange(infoSave.listOfTab1Item);
                        //loop items and replace sprite
                        for (int j = 0; j < listOfItemData.Count; j++)
                        {
                            ItemData item = listOfItemData[j];
                            if (item != null)
                            { item.sprite = GameManager.i.dataScript.GetSprite(item.spriteName); }
                            else
                            {
                                Debug.LogWarningFormat("Invalid ItemData (Null) in listOfItemData[{0}] for array[1]", j, i);
                                item.sprite = defaultSprite;
                            }
                        }
                        break;
                    case 2:
                        listOfItemData.AddRange(infoSave.listOfTab2Item);
                        //loop items and replace sprite
                        for (int j = 0; j < listOfItemData.Count; j++)
                        {
                            ItemData item = listOfItemData[j];
                            if (item != null)
                            { item.sprite = GameManager.i.dataScript.GetSprite(item.spriteName); }
                            else
                            {
                                Debug.LogWarningFormat("Invalid ItemData (Null) in listOfItemData[{0}] for array[1]", j, i);
                                item.sprite = defaultSprite;
                            }
                        }
                        break;
                    case 3:
                        listOfItemData.AddRange(infoSave.listOfTab3Item);
                        //loop items and replace sprite
                        for (int j = 0; j < listOfItemData.Count; j++)
                        {
                            ItemData item = listOfItemData[j];
                            if (item != null)
                            { item.sprite = GameManager.i.dataScript.GetSprite(item.spriteName); }
                            else
                            {
                                Debug.LogWarningFormat("Invalid ItemData (Null) in listOfItemData[{0}] for array[1]", j, i);
                                item.sprite = defaultSprite;
                            }
                        }
                        break;
                    case 4:
                        listOfItemData.AddRange(infoSave.listOfTab4Item);
                        //loop items and replace sprite
                        for (int j = 0; j < listOfItemData.Count; j++)
                        {
                            ItemData item = listOfItemData[j];
                            if (item != null)
                            { item.sprite = GameManager.i.dataScript.GetSprite(item.spriteName); }
                            else
                            {
                                Debug.LogWarningFormat("Invalid ItemData (Null) in listOfItemData[{0}] for array[1]", j, i);
                                item.sprite = defaultSprite;
                            }
                        }
                        break;
                    case 5:
                        listOfItemData.AddRange(infoSave.listOfTab5Item);
                        //loop items and replace sprite
                        for (int j = 0; j < listOfItemData.Count; j++)
                        {
                            ItemData item = listOfItemData[j];
                            if (item != null)
                            { item.sprite = GameManager.i.dataScript.GetSprite(item.spriteName); }
                            else
                            {
                                Debug.LogWarningFormat("Invalid ItemData (Null) in listOfItemData[{0}] for array[1]", j, i);
                                item.sprite = defaultSprite;
                            }
                        }
                        break;
                    default: Debug.LogErrorFormat("Unrecognised index {0}", i); break;
                }
            }
            else { Debug.LogErrorFormat("Invalid listOfItemData (Null) for arrayOfItemData[{0}]", i); }
        }
    }
    #endregion

    #region writeIndividualActor
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
        saveActor.statusHQ = actor.statusHQ;
        saveActor.actorID = actor.actorID;
        saveActor.hqID = actor.hqID;
        saveActor.datapoint0 = actor.GetDatapoint(ActorDatapoint.Datapoint0);
        saveActor.datapoint1 = actor.GetDatapoint(ActorDatapoint.Datapoint1);
        saveActor.datapoint2 = actor.GetDatapoint(ActorDatapoint.Datapoint2);
        saveActor.side = actor.side.name;
        saveActor.slotID = actor.slotID;
        saveActor.level = actor.level;
        saveActor.nodeCaptured = actor.nodeCaptured;
        saveActor.gearName = actor.GetGearName();
        saveActor.sex = actor.sex;
        saveActor.actorName = actor.actorName;
        saveActor.firstName = actor.firstName;
        saveActor.spriteName = actor.spriteName;
        saveActor.arcName = actor.arc.name;
        Personality personality = actor.GetPersonality();
        if (personality != null)
        {
            saveActor.listOfPersonalityFactors = personality.GetFactors().ToList();
            saveActor.compatibilityWithPlayer = personality.GetCompatibilityWithPlayer();
            saveActor.listOfDescriptors = personality.GetListOfDescriptors();
            saveActor.listOfMotivation = personality.GetListOfMotivation();
            saveActor.profile = personality.GetProfile();
            saveActor.profileDescriptor = personality.GetProfileDescriptor();
            saveActor.profileExplanation = personality.GetProfileExplanation();
        }
        else { Debug.LogWarningFormat("Invalid personality (Null) for {0}, actorID {1}", saveActor.actorName, saveActor.actorID); }

        Trait trait = actor.GetTrait();
        if (trait != null)
        { saveActor.traitName = trait.name; }
        else { Debug.LogWarningFormat("Invalid trait (Null) for {0}, actorID {1}", saveActor.actorName, saveActor.actorID); }
        //data which can be ignored (default values O.K) if actor is in the recruit pool
        if (actor.Status != ActorStatus.RecruitPool)
        {
            saveActor.Renown = actor.Renown;
            saveActor.unhappyTimer = actor.unhappyTimer;
            saveActor.blackmailTimer = actor.blackmailTimer;
            saveActor.captureTimer = actor.captureTimer;
            saveActor.numOfTimesBullied = actor.numOfTimesBullied;
            saveActor.numOfTimesCaptured = actor.numOfTimesCaptured;
            saveActor.numOfTimesConflict = actor.numOfTimesConflict;
            saveActor.numOfTimesBreakdown = actor.numOfTimesBreakdown;
            saveActor.numOfTimesStressLeave = actor.numOfTimesStressLeave;
            saveActor.numOfDaysStressed = actor.numOfDaysStressed;
            saveActor.numOfDaysLieLow = actor.numOfDaysLieLow;
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
            saveActor.isDismissed = actor.isDismissed;
            saveActor.isResigned = actor.isResigned;
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
                    { saveActor.listOfSecrets.Add(secret.name); }
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
            //trait effects
            List<string> listOfTraitEffects = actor.GetListOfTraitEffects();
            if (listOfTraitEffects != null)
            { saveActor.listOfTraitEffects.AddRange(listOfTraitEffects); }
            else { Debug.LogError("Invalid listOfTraitEffects (Null)"); }
            //Hq renown data
            List<HqRenownData> listOfRenownData = actor.GetListOfHqRenownData();
            if (listOfRenownData != null)
            { saveActor.listOfHqRenownData.AddRange(listOfRenownData); }
            else { Debug.LogError("Invalid listOfHqRenownData (Null)"); }
            //Actor History data
            List<HistoryActor> listOfHistory = actor.GetListOfHistory();
            if (listOfHistory != null)
            { saveActor.listOfHistory.AddRange(listOfHistory); }
            else { Debug.LogError("Invalid listOfHistory (Null)"); }
            //topics
            saveActor.listOfNodeActions = actor.GetListOfNodeActions();
            saveActor.listOfTeamActions = actor.GetListOfTeamActions();
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

    #region  WriteIndividualMetaData
    /// <summary>
    /// subMethod to convert MetaData into a SaveMetaData class (excludes Sprite and Effects). Returns empty SaveMetaData if metaData parameter null or empty
    /// </summary>
    /// <param name="metaData"></param>
    /// <returns></returns>
    private SaveMetaData WriteIndividualMetaData(MetaData metaData)
    {
        SaveMetaData saveData = new SaveMetaData();
        if (metaData != null)
        {
            saveData.metaName = metaData.metaName;
            saveData.itemText = metaData.itemText;
            saveData.textSelect = metaData.textSelect;
            saveData.textDeselect = metaData.textDeselect;
            saveData.textInsufficient = metaData.textInsufficient;
            saveData.bottomText = metaData.bottomText;
            saveData.inactiveText = metaData.inactiveText;
            saveData.spriteName = metaData.spriteName;
            saveData.priority = metaData.priority;
            saveData.tabSide = metaData.tabSide;
            saveData.tabTop = metaData.tabTop;
            saveData.renownCost = metaData.renownCost;
            saveData.sideLevel = metaData.sideLevel;
            saveData.isActive = metaData.isActive;
            saveData.isRecommended = metaData.isRecommended;
            saveData.isCriteria = metaData.isCriteria;
            saveData.isSelected = metaData.isSelected;
            saveData.recommendedPriority = metaData.recommendedPriority;
            saveData.data = metaData.data;
            saveData.dataName = metaData.dataName;
            saveData.dataTag = metaData.dataTag;
            saveData.help = metaData.help;
            saveData.tag0 = metaData.tag0;
            saveData.tag1 = metaData.tag1;
            saveData.tag2 = metaData.tag2;
            saveData.tag3 = metaData.tag3;
            if (metaData.listOfEffects.Count > 0)
            {
                for (int i = 0; i < metaData.listOfEffects.Count; i++)
                {
                    Effect effect = metaData.listOfEffects[i];
                    if (effect != null)
                    { saveData.listOfEffects.Add(effect.name); }
                    else { Debug.LogWarningFormat("Invalid Effect (Null) for listOfEffects[{0}]", i); }
                }
            }
        }
        else { Debug.LogWarning("Invalid metaData (Null)"); }
        return saveData;
    }
    #endregion

    #region WriteListOfMetaData
    /// <summary>
    /// Takes a list of MetaData and returns a list of SaveMetaData. If MetaData list Null or Empty, returns an empty SaveMetaData list
    /// 'listName' is name of list for debugging purposes in case of an error
    /// </summary>
    /// <param name="listOfMetaData"></param>
    /// <returns></returns>
    private List<SaveMetaData> WriteListOfMetaData(List<MetaData> listOfMetaData, string listName)
    {
        List<SaveMetaData> listOfSaveMetaData = new List<SaveMetaData>();
        if (listOfMetaData != null)
        {
            int count = listOfMetaData.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    MetaData metaData = listOfMetaData[i];
                    if (metaData != null)
                    {
                        SaveMetaData saveData = WriteIndividualMetaData(metaData);
                        if (saveData != null)
                        { listOfSaveMetaData.Add(saveData); }
                        else { Debug.LogWarningFormat("Invalid saveData for listOfMetaData[{0}]", i); }
                    }
                    else { Debug.LogWarningFormat("Invalid metaData (Null) for listOfMetaData[{0}] for {1}", i, listName); }
                }
            }
            else { Debug.LogWarningFormat("WriteListOfMetaData -> \"{0}\" Empty, Info only", listName); }
        }
        else { Debug.LogWarningFormat("Invalid listOfMetaData (Null) for {0}", listName); }
        return listOfSaveMetaData;
    }
    #endregion

    #region ReadIndividualMetaData
    /// <summary>
    /// Takes an individual SaveMetaData and returns a MetaData object. Returns an empty metaData if saveMetaData parameter is null or empty
    /// </summary>
    /// <param name="saveMetaData"></param>
    /// <returns></returns>
    private MetaData ReadIndividualSaveMetaData(SaveMetaData saveMetaData)
    {
        MetaData metaData = new MetaData();
        if (saveMetaData != null)
        {
            metaData.metaName = saveMetaData.metaName;
            metaData.itemText = saveMetaData.itemText;
            metaData.textSelect = saveMetaData.textSelect;
            metaData.textDeselect = saveMetaData.textDeselect;
            metaData.textInsufficient = saveMetaData.textInsufficient;
            metaData.bottomText = saveMetaData.bottomText;
            metaData.inactiveText = saveMetaData.inactiveText;
            metaData.priority = saveMetaData.priority;
            metaData.tabSide = saveMetaData.tabSide;
            metaData.tabTop = saveMetaData.tabTop;
            metaData.renownCost = saveMetaData.renownCost;
            metaData.sideLevel = saveMetaData.sideLevel;
            metaData.isActive = saveMetaData.isActive;
            metaData.isRecommended = saveMetaData.isRecommended;
            metaData.isCriteria = saveMetaData.isCriteria;
            metaData.isSelected = saveMetaData.isSelected;
            metaData.recommendedPriority = saveMetaData.recommendedPriority;
            metaData.data = saveMetaData.data;
            metaData.dataName = saveMetaData.dataName;
            metaData.dataTag = saveMetaData.dataTag;
            metaData.help = saveMetaData.help;
            metaData.tag0 = saveMetaData.tag0;
            metaData.tag1 = saveMetaData.tag1;
            metaData.tag2 = saveMetaData.tag2;
            metaData.tag3 = saveMetaData.tag3;
            //sprite
            metaData.spriteName = saveMetaData.spriteName;
            metaData.sprite = GameManager.i.dataScript.GetSprite(metaData.spriteName);
            if (metaData.sprite == null) { Debug.LogErrorFormat("Invalid sprite (Null) for metaData.spriteName \"{0}\"", metaData.spriteName); }
            //effects
            if (saveMetaData.listOfEffects != null)
            {
                string effectName;
                for (int i = 0; i < saveMetaData.listOfEffects.Count; i++)
                {
                    effectName = saveMetaData.listOfEffects[i];
                    if (string.IsNullOrEmpty(effectName) == false)
                    {
                        Effect effect = GameManager.i.dataScript.GetEffect(effectName);
                        if (effect != null)
                        {
                            metaData.listOfEffects.Add(effect);
                        }
                        else { Debug.LogErrorFormat("Invalid Effect (Null) for effectName \"{0}\"", effectName); }
                    }
                    else { Debug.LogErrorFormat("Invalid effectName (Null or Empty) for saveMetaData.listOfEffects[{0}]", i); }
                }
            }
            else { Debug.LogError("Invalid saveMetaData.listOfEffects (Null)"); }
        }
        else { Debug.LogWarning("Invalid saveMetaData (Null)"); }
        return metaData;
    }
    #endregion

    #region ReadListOfSaveMetaData
    /// <summary>
    /// Takes a list of SaveMetaData and returns a list of MetaData. If listOFSaveMetaData parameter is null or Empty, returns an empty listOfMetaData
    /// 'listName' is name of list for debugging purposes in case of an error
    /// </summary>
    /// <param name="listOfSaveMetaData"></param>
    /// <returns></returns>
    private List<MetaData> ReadListOfSaveMetaData(List<SaveMetaData> listOfSaveMetaData, string listName)
    {
        List<MetaData> listOfMetaData = new List<MetaData>();
        if (listOfSaveMetaData != null)
        {
            int count = listOfSaveMetaData.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    SaveMetaData saveMetaData = listOfSaveMetaData[i];
                    if (saveMetaData != null)
                    {
                        MetaData metaData = ReadIndividualSaveMetaData(saveMetaData);
                        if (metaData != null)
                        { listOfMetaData.Add(metaData); }
                        else { Debug.LogWarningFormat("Invalid metaData (Null) for listOfSaveMetaData[{0}]", i); }
                    }
                    else { Debug.LogWarningFormat("Invalid saveMetaData (Null) for listOfSaveMetaData[{0}]", i); }
                }
            }
            else { Debug.LogWarningFormat("ReadListOfMetaData -> \"{0}\" Empty, Info only", listName); }
        }
        else { Debug.LogWarningFormat("Invalid listOfSaveMetaData (Null) for {0}", listName); }
        return listOfMetaData;
    }

    #endregion

    #region ReadIndividualActor
    /// <summary>
    /// Restore an individual saved actor ready for loading back into dictionary
    /// </summary>
    /// <param name="readActor"></param>
    /// <returns></returns>
    private Actor ReadIndividualActor(SaveActor readActor)
    {
        Actor actor = new Actor();
        //copy over data from saveActor
        actor.actorID = readActor.actorID;
        actor.hqID = readActor.hqID;
        actor.SetDatapointLoad(ActorDatapoint.Datapoint0, readActor.datapoint0);
        actor.SetDatapointLoad(ActorDatapoint.Datapoint1, readActor.datapoint1);
        actor.SetDatapointLoad(ActorDatapoint.Datapoint2, readActor.datapoint2);
        GlobalSide actorSide = GameManager.i.dataScript.GetGlobalSide(readActor.side);
        if (actorSide != null)
        { actor.side = actorSide; }
        else { Debug.LogError("Invalid actorSide (Null)"); }
        actor.slotID = readActor.slotID;
        actor.level = readActor.level;
        actor.nodeCaptured = readActor.nodeCaptured;
        actor.SetGear(readActor.gearName);
        actor.sex = readActor.sex;
        actor.actorName = readActor.actorName;
        actor.firstName = readActor.firstName;
        actor.arc = GameManager.i.dataScript.GetActorArc(readActor.arcName);
        Trait trait = GameManager.i.dataScript.GetTrait(readActor.traitName);
        if (trait != null)
        { actor.AddTrait(trait); }
        else { Debug.LogWarningFormat("Invalid trait (Null) for traitID {0}", readActor.traitName); }
        //needs to be after SetGear
        actor.Status = readActor.status;
        actor.statusHQ = readActor.statusHQ;
        //Personality
        Personality personality = actor.GetPersonality();
        if (personality != null)
        {
            personality.SetFactors(readActor.listOfPersonalityFactors.ToArray());
            personality.SetCompatibilityWithPlayer(readActor.compatibilityWithPlayer);
            personality.SetDescriptors(readActor.listOfDescriptors);
            if (string.IsNullOrEmpty(readActor.profile) == false)
            {
                personality.SetProfile(readActor.profile);
                personality.SetProfileDescriptor(readActor.profileDescriptor);
                personality.SetProfileExplanation(readActor.profileExplanation);
                personality.SetMotivation(readActor.listOfMotivation);
            }
        }
        else { Debug.LogWarningFormat("Invalid personality (Null) for {0}, actorID {1}", actor.actorName, actor.actorID); }
        //sprite
        actor.spriteName = readActor.spriteName;
        actor.sprite = GameManager.i.dataScript.GetSprite(actor.spriteName);
        if (actor.sprite == null)
        { actor.sprite = defaultSprite; }
        //fast access
        actor.actorStressNone = read.actorData.actorStressNone;
        actor.actorCorruptNone = read.actorData.actorCorruptNone;
        actor.actorUnhappyNone = read.actorData.actorUnhappyNone;
        actor.actorBlackmailNone = read.actorData.actorBlackmailNone;
        actor.actorBlackmailTimerHigh = read.actorData.actorBlackmailTimerHigh;
        actor.actorBlackmailTimerLow = read.actorData.actorBlackmailTimerLow;
        actor.maxNumOfSecrets = read.actorData.maxNumOfSecrets;
        actor.compatibilityOne = read.actorData.compatibilityOne;
        actor.compatibilityTwo = read.actorData.compatibilityTwo;
        actor.compatibilityThree = read.actorData.compatibilityThree;
        //data which can be ignored (default values O.K) if actor is in the Recruit Pool
        if (actor.Status != ActorStatus.RecruitPool)
        {
            actor.Renown = readActor.Renown;
            actor.unhappyTimer = readActor.unhappyTimer;
            actor.blackmailTimer = readActor.blackmailTimer;
            actor.captureTimer = readActor.captureTimer;
            actor.numOfTimesBullied = readActor.numOfTimesBullied;
            actor.numOfTimesCaptured = readActor.numOfTimesCaptured;
            actor.numOfTimesConflict = readActor.numOfTimesConflict;
            actor.numOfTimesBreakdown = readActor.numOfTimesBreakdown;
            actor.numOfTimesStressLeave = readActor.numOfTimesStressLeave;
            actor.numOfDaysStressed = readActor.numOfDaysStressed;
            actor.numOfDaysLieLow = readActor.numOfDaysLieLow;
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
            actor.isDismissed = readActor.isDismissed;
            actor.isResigned = readActor.isResigned;
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
            if (listOfSecrets != null)
            {
                listOfSecrets.Clear();
                for (int j = 0; j < readActor.listOfSecrets.Count; j++)
                {
                    Secret secret = GameManager.i.dataScript.GetSecret(readActor.listOfSecrets[j]);
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
                    Contact contact = GameManager.i.dataScript.GetContact(readActor.listOfContacts[j]);
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
            //trait effects
            List<string> listOfTraitEffects = actor.GetListOfTraitEffects();
            if (listOfTraitEffects != null)
            {
                listOfTraitEffects.Clear();
                listOfTraitEffects.AddRange(readActor.listOfTraitEffects);
            }
            else { Debug.LogError("Invalid listOfTraitEffects (Null)"); }
            //conditions
            List<Condition> listOfConditions = actor.GetListOfConditions();
            if (listOfConditions != null)
            {
                listOfConditions.Clear();
                for (int j = 0; j < readActor.listOfConditions.Count; j++)
                {
                    Condition condition = GameManager.i.dataScript.GetCondition(readActor.listOfConditions[j]);
                    if (condition != null)
                    { listOfConditions.Add(condition); }
                    else { Debug.LogWarningFormat("Invalid Condition in readActor.listOfConditions[{0}]", j); }
                }
            }
            else { Debug.LogError("Invalid listOfConditions (Null)"); }
            //Hq Renown data
            actor.SetHqRenownData(readActor.listOfHqRenownData);
            //Actor History
            actor.SetHistory(readActor.listOfHistory);
            //topic data
            actor.SetNodeActionData(readActor.listOfNodeActions);
            actor.SetTeamActionData(readActor.listOfTeamActions);
        }
        return actor;
    }
    #endregion


    //new methods above here
}
