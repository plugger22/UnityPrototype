using System.Collections;
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

    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        filename = Path.Combine(Application.persistentDataPath, SAVE_FILE);
        cipherKey = "#kJ83DAl50$*@.<__'][90{4#dDA'a";
        //fast access
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
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
        WritePlayerData();
    }

    /// <summary>
    /// Save game method
    /// </summary>
    public void SaveGame()
    {
        if (write != null)
        {
            //convert to Json
            jsonWrite = JsonUtility.ToJson(write);

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

            //secrets before player

            ReadPlayerData();
            ValidatePlayerData();
            Debug.LogFormat("[Fil] FileManager.cs -> LoadSaveData: Saved Game Data has been LOADED{0}", "\n");
        }
    }

    //
    // - - - Write - - -
    //

    /// <summary>
    /// Player Data
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
        foreach(Condition condition in listOfConditions)
        {
            if (condition != null)
            { write.playerData.listOfConditionsResistance.Add(condition.name); }
            else { Debug.LogWarning("Invalid Resistance condition (Null)"); }
        }
        listOfConditions = GameManager.instance.playerScript.GetListOfConditions(globalAuthority);
        foreach (Condition condition in listOfConditions)
        {
            if (condition != null)
            { write.playerData.listOfConditionsAuthority.Add(condition.name); }
            else { Debug.LogWarning("Invalid Authority condition (Null)"); }
        }
    }

    //
    // - - - Read - - -
    //

    /// <summary>
    /// Player Data
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

    //
    // - - -  Validate - - -
    //

    /// <summary>
    /// Validate player data (logic checks) and Initialise gfx's where needed
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

    //new methods above here
}
