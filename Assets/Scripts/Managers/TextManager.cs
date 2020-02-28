using gameAPI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Development class than enables text to be passed from and to SO TextList objects via JSON to allow editing outside of Unity
/// </summary>
public class TextManager : MonoBehaviour
{
    [Tooltip("Max number of SO's that can be handled at one time (each one has it's own file)")]
    [Range(1, 10)] public int maxNumOfSO = 5;

    [Tooltip("Add SO TextLists here. Only the maxNumOfSO files in the list will be accomodated, any extras will be ignored")]
    public List<TextList> listOfTextLists;

    private static readonly string SAVE_FILE = "textfile";
    private string filename;
    private string jsonWrite;
    private string jsonRead;

    private List<string> listOfFileNames = new List<string>();

    #region Initialise
    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise(GameState state)
    {
        for (int i = 0; i < maxNumOfSO; i++)
        {
            //set up list of filenames in format '[SAVE_FILE][#][.json]'
            listOfFileNames.Add(Path.Combine(Application.persistentDataPath, string.Format("{0}{1}{2}", SAVE_FILE, i, ".json")));
        }
    }
    #endregion

    /// <summary>
    /// Export SO  method
    /// </summary>
    public void Export()
    {
        TextList textList;
        //don't exceed number of files present or maxCap
        int limit = Mathf.Min(maxNumOfSO, listOfTextLists.Count);
        for (int i = 0; i < limit; i++)
        {
            //valid file present
            if (listOfTextLists[i] != null)
            {
                textList = listOfTextLists[i];
                filename = listOfFileNames[i];
                if (string.IsNullOrEmpty(filename) == false)
                {
                    //convert to Json
                    jsonWrite = JsonUtility.ToJson(textList, true);
                    //file present? If so delete
                    if (File.Exists(filename) == true)
                    {
                        try { File.Delete(filename); }
                        catch (Exception e) { Debug.LogErrorFormat("Failed to DELETE FILE \"{0}\", error \"{1}\"", filename, e.Message); }
                    }
                    //create new file
                    try
                    {
                        File.WriteAllText(filename, jsonWrite);
                        Debug.LogFormat("[Fil] TextManager.cs -> Export: EXPORTED to \"{0}\"{1}", filename, "\n");
                    }
                    catch (Exception e) { Debug.LogErrorFormat("Failed to write TEXT FROM FILE \"{0}\", error \"{1}\"", filename, e.Message); }
                }
                else { Debug.LogWarningFormat("Invalid fileName (Null or Empty) for listOfFileNames[{0}]", i); }
            }
            else { Debug.LogWarningFormat("Invalid textList (Null) in listOfTextLists[{0}]", i); }
        }
    }


    /// <summary>
    /// Import external JSON data back into list of SO TextLists
    /// </summary>
    public void Import()
    {
        bool isSuccess = false;
        TextList textList;
        //don't exceed number of files present or maxCap
        int limit = Mathf.Min(maxNumOfSO, listOfTextLists.Count);
        for (int i = 0; i < limit; i++)
        {
            //valid file present
            if (listOfTextLists[i] != null)
            {
                jsonRead = "";
                textList = listOfTextLists[i];
                filename = listOfFileNames[i];
                if (string.IsNullOrEmpty(filename) == false)
                {
                    if (File.Exists(filename) == true)
                    {
                        //read data from File
                        try { jsonRead = File.ReadAllText(filename); }
                        catch (Exception e) { Debug.LogErrorFormat("Failed to read TEXT FROM FILE \"{0}\", error \"{1}\"", filename, e.Message); }
                        isSuccess = true;

                        if (isSuccess == true)
                        {
                            //Overwrite existing SO with JSON data
                            try
                            {
                                JsonUtility.FromJsonOverwrite(jsonRead, textList);
                                Debug.LogFormat("[Fil] TextManager.cs -> Import: IMPORTED json data from \"{0}\"{1}", filename, "\n");
                            }
                            catch (Exception e)
                            { Debug.LogErrorFormat("Failed to Overwrite Json data, file \"{0}\", error \"{1}\"", filename, e.Message); }
                        }
                    }
                    else { Debug.LogErrorFormat("File \"{0}\" not found", filename); }
                }
                else { Debug.LogWarningFormat("Invalid fileName (Null or Empty) for listOfFileNames[{0}]", i); }
            }
            else { Debug.LogWarningFormat("Invalid textList (Null) in listOfTextLists[{0}]", i); }
        }
    }

    /// <summary>
    /// Export all topic Options (specific fields only) to a file
    /// </summary>
    public void ExportTopicOptions()
    {
        TopicOption[] arrayOfOptions = GameManager.instance.loadScript.arrayOfTopicOptions;
        if (arrayOfOptions != null)
        {
            int count = arrayOfOptions.Length;
            //create list of data
            string[] arrayOfData = new string[count * 2];
            for (int i = 0; i < count; i++)
            {
                TopicOption option = arrayOfOptions[i];
                if (option != null)
                {
                    arrayOfData[i * 2] = option.name;
                    arrayOfData[i * 2 + 1] = option.news;
                }
                else { Debug.LogErrorFormat("Invalid topicOption (Null) in arrayOfOptions[{0}]", i); }
            }
            //export list of data
            filename = "TopicOptionData.txt";
            if (File.Exists(filename) == true)
            {
                try { File.Delete(filename); }
                catch (Exception e) { Debug.LogErrorFormat("Failed to DELETE FILE \"{0}\', error \"{1}\"", filename, e.Message); }
            }
            //create new file
            try
            {
                File.WriteAllLines(filename, arrayOfData);
                Debug.LogFormat("[Fil] TextManager.cs -> ExportTopicOptions: \"{0}\" Exported{1}", filename, "\n");
            }
            catch (Exception e) { Debug.LogErrorFormat("Failed to write TEXT TO FILE \"{0}\", error \"{1}\"", filename, e.Message); }
        }
        else { Debug.LogError("Invalid arrayOfOptions (Null)"); }
    }

    //new method above here
}
