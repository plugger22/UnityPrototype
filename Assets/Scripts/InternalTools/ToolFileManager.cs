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
                Debug.LogFormat("[Fil] FileManager.cs -> SaveToolsToFile: ToolData SAVED to \"{0}\"{1}", filenameTools, "\n");

            }
            else { Debug.LogError("Invalid fileName (Null or Empty)"); }
        }
        else { Debug.LogError("Invalid saveData (Null)"); }
    }
    #endregion

    #region ReadToolsFromFile
    /// <summary>
    /// read tools method. Returns true if successful, false otherwise
    /// </summary>
    public bool ReadToolsFromFile()
    {
        bool isSuccess = false;
        if (string.IsNullOrEmpty(filenameTools) == false)
        {
            if (File.Exists(filenameTools) == true)
            {

                //read data from File
                try { jsonRead = File.ReadAllText(filenameTools); }
                catch (Exception e) { Debug.LogErrorFormat("Failed to read TEXT FROM FILE, error \"{0}\"", e.Message); }
                isSuccess = true;
                if (isSuccess == true)
                {
                    //read to Save file
                    try
                    {
                        read = JsonUtility.FromJson<SaveTools>(jsonRead);
                        Debug.LogFormat("[Fil] FileManager.cs -> ReadToolsFromFile: GAME LOADED from \"{0}\"{1}", filenameTools, "\n");
                        return true;
                    }
                    catch (Exception e)
                    { Debug.LogErrorFormat("Failed to read Json, error \"{0}\"", e.Message); }
                }
            }
            else { Debug.LogWarningFormat("File \"{0}\" not found", filenameTools); }
        }
        else { Debug.LogError("Invalid filename (Null or Empty)"); }
        return false;
    }
    #endregion

    #region ReadToolData
    /// <summary>
    /// load tool data back into toolDataManager.cs
    /// </summary>
    public void ReadToolData()
    {
        if (read != null)
        {
            ReadStories();
        }
        else { Debug.LogError("Invalid read (Null)"); }
    }

    #endregion

    #region ReadStories
    /// <summary>
    /// Read saved data back into dictOfStories
    /// </summary>
    private void ReadStories()
    {
        if (read.toolData.listOfStories != null)
        {
            for (int i = 0; i < read.toolData.listOfStories.Count; i++)
            {
                Story story = read.toolData.listOfStories[i];
                if (story != null)
                { ToolManager.i.toolDataScript.AddStory(story); }
                else { Debug.LogErrorFormat("Invalid story (Null) for listOfStories[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid toolData.listOfStories (Null)"); }
    }

    #endregion

    #region DeleteToolsFile
    /// <summary>
    /// Deletes file
    /// </summary>
    public void DeleteToolsFile()
    {
        if (File.Exists(filenameTools) == true)
        {
            File.Delete(filenameTools);
            Debug.LogFormat("[Fil] FileManager.cs -> DeleteToolsFile: file at \"{0}\" DELETED{1}", filenameTools, "\n");
        }
    }
    #endregion

    //new methods above here
}

#endif
