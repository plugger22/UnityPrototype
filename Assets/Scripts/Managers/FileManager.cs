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

    public void CreateSaveData()
    {
        saveData = new Save();
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
            Debug.LogFormat("[Ctrl] FileManager.cs -> SaveGame: GAME SAVED{0}", "\n");
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
            Debug.LogFormat("[Ctrl] FileManager.cs -> LoadGame: GAME READ{0}", "\n");
        }
        else { Debug.LogErrorFormat("File \"{0}\" not found", filename); }
    }



    //new methods above here
}
