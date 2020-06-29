using System;
using System.Collections.Generic;
using System.IO;
using toolsAPI;
using UnityEngine;

#if (UNITY_EDITOR)

#region SaveTools
/// <summary>
/// Save.cs equivalent
/// </summary>
public class SaveTools
{
    public SaveToolData toolData = new SaveToolData();
}
#endregion

#region SaveToolData
/// <summary>
/// Save all tools data
/// </summary>
[System.Serializable]
public class SaveToolData
{
    public List<Story> listOfStories = new List<Story>();
}
#endregion

/// <summary>
/// handles all file save/load functionality. Doubles up as Save.cs and FileManager.cs equivalents
/// </summary>
public class ToolFileManager : MonoBehaviour
{
    private static readonly string SAVE_FILE = "toolfile.json";
    private SaveTools write;
    private SaveTools read;
    private string filenameTools;
    private string jsonWrite;
    private string jsonRead;

    #region Initialise
    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        filenameTools = Path.Combine(Application.persistentDataPath, SAVE_FILE);
    }
    #endregion

    #region Write Tool Data
    /// <summary>
    /// copy inGame data to saveData
    /// </summary>
    public void WriteToolData()
    {
        write = new SaveTools();
        //Sequentially write data
        WriteStories();

    }
    #endregion

    #region WriteStories
    /// <summary>
    /// write story dict to file
    /// </summary>
    private void WriteStories()
    {
        Dictionary<string, Story> dictOfStories = ToolManager.i.toolDataScript.GetDictOfStories();
        if (dictOfStories != null)
        {
            foreach (var story in dictOfStories)
            {
                if (story.Value != null)
                { write.toolData.listOfStories.Add(story.Value); }
                else { Debug.LogErrorFormat("Invalid story (Null) for \"{0}\"", story.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfStories (Null)"); }
    }
    #endregion

    #region SaveToolsToFile
    /// <summary>
    /// Save tools method
    /// </summary>
    public void SaveToolsToFile()
    {
        if (write != null)
        {
            if (string.IsNullOrEmpty(filenameTools) == false)
            {
                //convert to Json (NOTE: second, optional, parameter gives pretty output for debugging purposes)
                jsonWrite = JsonUtility.ToJson(write, true);

                //file present? If so delete
                if (File.Exists(filenameTools) == true)
                {
                    try { File.Delete(filenameTools); }
                    catch (Exception e) { Debug.LogErrorFormat("Failed to DELETE FILE, error \"{0}\"", e.Message); }
                }

                //create new file
                try { File.WriteAllText(filenameTools, jsonWrite); }
                catch (Exception e) { Debug.LogErrorFormat("Failed to write TEXT FROM FILE, error \"{0}\"", e.Message); }
                Debug.LogFormat("[Fil] FileManager.cs -> SaveToolsToFile: GAME SAVED to \"{0}\"{1}", filenameTools, "\n");

            }
            else { Debug.LogError("Invalid fileName (Null or Empty)"); }
        }
        else { Debug.LogError("Invalid saveData (Null)"); }
    }
    #endregion




    //new methods above here
}

#endif
