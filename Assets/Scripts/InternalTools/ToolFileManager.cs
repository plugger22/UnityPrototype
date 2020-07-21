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
/// Save all tools data. Story is a custom class to get around nested lists in ToolPackageManager.cs
/// </summary>
[System.Serializable]
public class SaveToolData
{
    public List<Story> listOfStories = new List<Story>();
    public List<ConstantPlotpoint> listOfConstantPlotPoints = new List<ConstantPlotpoint>();
}
#endregion


/// <summary>
/// handles all file save/load functionality. Doubles up as Save.cs and FileManager.cs equivalents
/// </summary>
public class ToolFileManager : MonoBehaviour
{
    private static readonly string JSON_FILE = "toolfile.json";
    private static readonly string TEXT_FILE = "exportfile.txt";
    private static readonly string ORG_FILE = "orgfile.txt";

    private SaveTools writeJSON;
    private SaveTools readJSON;
    private string writeTEXT;
    private string filenameTools;
    private string filenameExport;
    private string filenameOrgExport;
    private string jsonWrite;
    private string jsonRead;

    #region Initialise
    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        filenameTools = Path.Combine(Application.persistentDataPath, JSON_FILE);
        filenameExport = Path.Combine(Application.persistentDataPath, TEXT_FILE);
        filenameOrgExport = Path.Combine(Application.persistentDataPath, ORG_FILE);
    }
    #endregion

    // - - - JSON File

    #region Write Tool Data
    /// <summary>
    /// copy inGame data to saveData
    /// </summary>
    public void WriteToolData()
    {
        writeJSON = new SaveTools();
        //Sequentially write data
        WriteStories();
        WriteConstantPlotpoints();
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
                { writeJSON.toolData.listOfStories.Add(story.Value); }
                else { Debug.LogErrorFormat("Invalid story (Null) for \"{0}\"", story.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfStories (Null)"); }
    }
    #endregion

    #region WriteConstantPlotpoints
    /// <summary>
    /// write dictOfConstantPlotpoints to file
    /// </summary>
    private void WriteConstantPlotpoints()
    {
        Dictionary<string, ConstantPlotpoint> dictOfConstantPlotpoints = ToolManager.i.toolDataScript.GetDictOfConstantPlotpoints();
        if (dictOfConstantPlotpoints != null)
        {
            foreach (var plotpoint in dictOfConstantPlotpoints)
            {
                if (plotpoint.Value != null)
                { writeJSON.toolData.listOfConstantPlotPoints.Add(plotpoint.Value); }
                else { Debug.LogErrorFormat("Invalid constantPlotpoint (Null) for \"{0}\"", plotpoint.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfConstantPlotpoints (Null)"); }
    }
    #endregion

    #region SaveToolsToFile
    /// <summary>
    /// Save tools method
    /// </summary>
    public void SaveToolsToFile()
    {
        if (writeJSON != null)
        {
            if (string.IsNullOrEmpty(filenameTools) == false)
            {
                //convert to Json (NOTE: second, optional, parameter gives pretty output for debugging purposes)
                jsonWrite = JsonUtility.ToJson(writeJSON, true);

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
                        readJSON = JsonUtility.FromJson<SaveTools>(jsonRead);
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
        if (readJSON != null)
        {
            ReadStories();
            ReadConstantPlotpoints();
        }
        else { Debug.LogError("Invalid read (Null)"); }
    }

    #endregion

    #region ReadStories
    /// <summary>
    /// Read saved data back into dictOfStories
    /// </summary>
    private void ReadStories()
    { ToolManager.i.toolDataScript.SetStories(readJSON.toolData.listOfStories); }
    #endregion

    #region ReadConstantPlotpoints
    /// <summary>
    /// Read saved data back into dictOfConstantPlotpoints
    /// </summary>
    private void ReadConstantPlotpoints()
    { ToolManager.i.toolDataScript.SetConstantPlotpoints(readJSON.toolData.listOfConstantPlotPoints); }
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

    // - - - Export File (Write only -> text -> dataDump)

    #region ExportToolData
    /// <summary>
    /// Take in-game data and convert to dataDamp export format suitable for a cut and paste into the Keep
    /// </summary>
    public void ExportToolData()
    {
        writeTEXT = ToolManager.i.adventureScript.CreateExportDataDump();
    }
    #endregion

    #region SaveExportToFile
    /// <summary>
    /// write export dataDump to file
    /// </summary>
    public void SaveExportToFile()
    {
        if (writeTEXT != null)
        {
            //file present? If so delete
            if (File.Exists(filenameExport) == true)
            {
                try { File.Delete(filenameExport); }
                catch (Exception e) { Debug.LogErrorFormat("Failed to DELETE FILE, error \"{0}\"", e.Message); }
            }
            //create new file
            try { File.WriteAllText(filenameExport, writeTEXT); }
            catch (Exception e) { Debug.LogErrorFormat("Failed to write TEXT FROM FILE, error \"{0}\"", e.Message); }
            Debug.LogFormat("[Fil] FileManager.cs -> SaveExportToFile: ExportData SAVED to \"{0}\"{1}", filenameExport, "\n");
        }
        else { Debug.LogError("Invalid writeTEXT (Null)"); }
    }
    #endregion

    // - - - Organisation File (Write only -> text -> 'x' number of Orgs)

    #region ExportOrgData
    /// <summary>
    /// Take in-game Org data and convert to dataDamp export format suitable for a cut and paste into the Keep
    /// </summary>
    public void ExportOrgData()
    {
        writeTEXT = ToolManager.i.adventureScript.GetExportOrganisations(10);
    }
    #endregion

    #region SaveOrgExportToFile
    /// <summary>
    /// write export dataDump to file
    /// </summary>
    public void SaveOrgExportToFile()
    {
        if (writeTEXT != null)
        {
            //file present? If so delete
            if (File.Exists(filenameOrgExport) == true)
            {
                try { File.Delete(filenameOrgExport); }
                catch (Exception e) { Debug.LogErrorFormat("Failed to DELETE FILE, error \"{0}\"", e.Message); }
            }
            //create new file
            try { File.WriteAllText(filenameOrgExport, writeTEXT); }
            catch (Exception e) { Debug.LogErrorFormat("Failed to write TEXT FROM FILE, error \"{0}\"", e.Message); }
            Debug.LogFormat("[Fil] FileManager.cs -> SaveOrgExportToFile: OrgExportData SAVED to \"{0}\"{1}", filenameExport, "\n");
        }
        else { Debug.LogError("Invalid writeTEXT (Null)"); }
    }
    #endregion

    //new methods above here
}

#endif
