using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Handles all Save / Load functionality
/// </summary>
public class FileManager : MonoBehaviour
{

    private static readonly string SAVE_FILE = "savefile.json";
    private Save saveData;
    private Save readData;
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

    /// <summary>
    /// copy inGame data to saveData
    /// </summary>
    public void CreateSaveData()
    {
        saveData = new Save();
        saveData.playerRenown = GameManager.instance.playerScript.Renown;
        saveData.playerStatus = GameManager.instance.playerScript.status;
        saveData.listOfPlayerGear = GameManager.instance.playerScript.GetListOfGear();
    }

    /// <summary>
    /// Save game method
    /// </summary>
    public void SaveGame()
    {
        if (saveData != null)
        {
            //convert to Json
            json = JsonUtility.ToJson(saveData);
            //file present? If so delete
            if (File.Exists(filename) == true)
            { File.Delete(filename); }
            //create new file
            File.WriteAllText(filename, json);
            Debug.LogFormat("[Ctrl] FileManager.cs -> SaveGame: GAME SAVED to \"{0}\"{1}",filename, "\n");
        }
        else { Debug.LogError("Invalid saveData (Null)"); }
    }

    /// <summary>
    /// Read game method
    /// </summary>
    public void ReadGame()
    {
        if (File.Exists(filename) == true)
        {
            jsonFromFile = File.ReadAllText(filename);
            /*Debug.LogFormat("[Ctrl] FileManager.cs -> ReadGame: Save Game READ from file{0}", "\n");*/
            //read data from File
            jsonFromFile = File.ReadAllText(filename);
            //read to Save file
            readData = JsonUtility.FromJson<Save>(jsonFromFile);
        }
        else { Debug.LogErrorFormat("File \"{0}\" not found", filename); }
    }

    /// <summary>
    /// Load data from Save file back into game
    /// </summary>
    public void LoadSaveData()
    {
        if (readData != null)
        {
            GameManager.instance.playerScript.Renown = readData.playerRenown;
            GameManager.instance.playerScript.status = readData.playerStatus;
            List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
            listOfGear.Clear();
            listOfGear.AddRange(readData.listOfPlayerGear);
            Debug.LogFormat("[Ctrl] FileManager.cs -> LoadSaveData: Saved Game Data has been LOADED{0}", "\n");
        }
    }

    //new methods above here
}
