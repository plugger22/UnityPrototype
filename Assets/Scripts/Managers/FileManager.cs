using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Handles all Save / Load functionality
/// </summary>
public class FileManager
{

    private static readonly string SAVE_FILE = "savefile.json";
    private Save write;
    private Save read;
    private string filename;
    private string json;
    private string jsonFromFile;


    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        filename = Path.Combine(Application.persistentDataPath, SAVE_FILE);
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
            json = JsonUtility.ToJson(write);

            //file present? If so delete
            if (File.Exists(filename) == true)
            { File.Delete(filename); }

            if (GameManager.instance.isEncrypted == false)
            {
                //create new file
                File.WriteAllText(filename, json);
                Debug.LogFormat("[Fil] FileManager.cs -> SaveGame: GAME SAVED to \"{0}\"{1}", filename, "\n");
            }
            else
            {
                //encrypt save file
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
        if (File.Exists(filename) == true)
        {
            if (GameManager.instance.isEncrypted == false)
            {
                jsonFromFile = File.ReadAllText(filename);
                //read data from File
                jsonFromFile = File.ReadAllText(filename);
                //read to Save file
                read = JsonUtility.FromJson<Save>(jsonFromFile);
                return true;
            }
            else
            {
                //encrypted file
                return true;
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
            ReadPlayerData();
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
        write.playerData.listOfGear = GameManager.instance.playerScript.GetListOfGear();
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
        GameManager.instance.playerScript.status = read.playerData.status;
        List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
        listOfGear.Clear();
        listOfGear.AddRange(read.playerData.listOfGear);
    }

    //new methods above here
}
