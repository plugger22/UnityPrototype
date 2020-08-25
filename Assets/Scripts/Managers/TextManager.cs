using gameAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    /// Export TextList SO  method
    /// </summary>
    public void ExportTextLists()
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
    public void ImportTextLists()
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
        TopicOption[] arrayOfOptions = GameManager.i.loadScript.arrayOfTopicOptions;
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


    //
    // - - -  Export Story Module Data
    //

    /// <summary>
    /// generate a string containing all storyHelp data in an suitable format to export to a file
    /// </summary>
    /// <returns></returns>
    public string CreateStoryHelpExport()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("Story Help{0}", "\n");
        Dictionary<string, StoryHelp> dictOfStoryHelp = GameManager.i.dataScript.GetDictOfStoryHelp();
        if (dictOfStoryHelp != null)
        {
            List<string> tempList = dictOfStoryHelp.Keys.ToList();
            if (tempList != null)
            {
                tempList.Sort();
                for (int i = 0; i < tempList.Count; i++)
                {
                    StoryHelp help = GameManager.i.dataScript.GetStoryHelp(tempList[i]);
                    if (help != null)
                    {
                        builder.AppendFormat("{0}Name: {1}{2}", "\n", help.name, "\n");
                        builder.AppendFormat("Tag: {0}{1}", help.tag, "\n");
                        if (help.sprite != null)
                        { builder.AppendFormat("Sprite: {0}{1}", help.sprite.name, "\n"); }
                        else { builder.AppendFormat("Sprite: default{0}", "\n"); }
                        builder.AppendFormat("{0}{1}{2}", "\n", help.textTop, "\n");
                        builder.AppendFormat("{0}{1}{2}", "\n", help.textMiddle, "\n");
                        builder.AppendFormat("{0}{1}{2}", "\n", help.textBottom, "\n");
                        builder.AppendLine();
                    }
                    else { Debug.LogWarningFormat("Invalid storyHelp (Null) for key \"{0}\"", tempList[i]); }
                }
            }
            else { Debug.LogError("Invalid tempList (Null) for dictOfStoryHelp"); }
        }
        else { Debug.LogError("Invalid dictOfStoryHelp (Null)"); }
        return builder.ToString();
    }


    /// <summary>
    /// generate a string containing all story data in an suitable format to export to a file
    /// </summary>
    /// <returns></returns>
    public string CreateStoryDataExport()
    {
        StringBuilder builder = new StringBuilder();
        //loop storyModules
        StoryModule[] arrayOfModules = GameManager.i.loadScript.arrayOfStoryModules;
        if (arrayOfModules != null)
        {
            for (int j = 0; j < arrayOfModules.Length; j++)
            {
                StoryModule module = arrayOfModules[j];
                if (module != null)
                {
                    builder.AppendFormat("storyModule: {0}{1}", module.name, "\n");
                    builder.AppendFormat("side: {0}{1}", module.side.name, "\n");
                    builder.AppendFormat("{0}List: Campaign Stories{1}", "\n", "\n");
                    builder.Append(CheckStoryModuleList(module.listOfCampaignStories));
                    builder.AppendFormat("{0}List: Family Stories{1}", "\n", "\n");
                    builder.Append(CheckStoryModuleList(module.listOfFamilyStories));
                    builder.AppendFormat("{0}List: HQ Stories{1}", "\n", "\n");
                    builder.Append(CheckStoryModuleList(module.listOfHqStories));
                    builder.AppendLine();
                    builder.AppendLine();
                }
                else { Debug.LogWarningFormat("Invalid storyModule (Null) for arrayOfModules[{0}]", j); }
            }
        }
        else { Debug.LogError("Invalid arrayOfStoryModules (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// private subMethod for CreateStoryDataExport that handles individual lists of StoryData
    /// </summary>
    /// <param name="listOfStoryData"></param>
    /// <returns></returns>
    private string CheckStoryModuleList(List<StoryData> listOfStoryData)
    {
        StringBuilder builder = new StringBuilder();
        int count = listOfStoryData.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                StoryData data = listOfStoryData[i];
                if (data != null)
                {
                    builder.AppendFormat("name: {0}{1}", listOfStoryData[i].name, "\n");
                    builder.AppendFormat("design notes: {0}{1}", listOfStoryData[i].designNotes, "\n");
                    builder.AppendFormat("topic pool: {0}{1}", listOfStoryData[i].pool.name, "\n");
                    builder.AppendFormat("topicItems: {0} present{1}", listOfStoryData[i].listOfTopicItems.Count, "\n");
                    builder.AppendFormat("{0}{1}", CheckTopicItems(listOfStoryData[i].listOfTopicItems), "\n");
                    if (listOfStoryData[i].pool.listOfTopics.Count > 0)
                    { builder.AppendFormat("{0}{1}", CheckTopicPool(listOfStoryData[i].pool), "\n"); }
                }
                else { Debug.LogWarningFormat("Invalid storyData (Null) for listOfStoryData[{0}]", i); }
            }
        }
        else { builder.AppendFormat("No StoryData present{0}", "\n"); }
        return builder.ToString();
    }

    /// <summary>
    /// private SubMethod for CheckStoryModuleList to handles TopicItemss
    /// </summary>
    /// <param name="listOfItems"></param>
    /// <returns></returns>
    private string CheckTopicItems(List<TopicItem> listOfItems)
    {
        StringBuilder builder = new StringBuilder();
        int count = listOfItems.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                builder.AppendFormat("{0}itemName: {1}{2}", "\n", listOfItems[i].name, "\n");
                builder.AppendFormat("tag: {0}{1}", listOfItems[i].tag, "\n");
                builder.AppendFormat("descriptor: {0}{1}", listOfItems[i].descriptor, "\n");
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// private SubMethod for CheckStoryModuleList to return topics in a pools
    /// </summary>
    /// <param name="pool"></param>
    /// <returns></returns>
    private string CheckTopicPool(TopicPool pool)
    {
        StringBuilder builder = new StringBuilder();
        if (pool != null)
        {
            int count = pool.listOfTopics.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Topic topic = pool.listOfTopics[i];
                    if (topic != null)
                    {
                        builder.AppendFormat("{0}topicName: {1}{2}", "\n", topic.name, "\n");
                        builder.AppendFormat("tag: {0}{1}", topic.tag, "\n");
                        builder.AppendFormat("topicItem: {0}{1}", topic.topicItem != null ? topic.topicItem.name : "none present", "\n");
                        builder.AppendFormat("{0}{1}", CheckTopicHelp(topic.listOfStoryHelp), "\n");
                        builder.AppendFormat("{0}{1}", topic.text, "\n");
                        builder.AppendFormat("{0}{1}", "\n", CheckTopicOptions(topic.listOfOptions));
                    }
                    else { Debug.LogWarningFormat("Invalid topic (Null) for TopicPool \"{0}\", listOfTopics[{1}]", pool.name, i); }
                }
            }
        }
        else { Debug.LogWarning("Invalid topicPool (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// private SubMethod for CheckTopicPool
    /// </summary>
    /// <param name="listOfOptions"></param>
    /// <returns></returns>
    private string CheckTopicOptions(List<TopicOption> listOfOptions)
    {
        StringBuilder builder = new StringBuilder();
        if (listOfOptions != null)
        {
            int count = listOfOptions.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    TopicOption option = listOfOptions[i];
                    if (option != null)
                    {
                        builder.AppendFormat("{0}optionName: {1}{2}", "\n", option.name, "\n");
                        builder.AppendFormat("tag: {0}{1}", option.tag, "\n");
                        builder.AppendFormat("text: {0}{1}", option.text, "\n");
                        if (option.storyInfo.Length > 0)
                        { builder.AppendFormat("{0}{1}{2}", "\n", option.storyInfo, "\n"); }
                        if (option.storyTarget != null)
                        {
                            builder.AppendFormat("{0}{1}, \"{2}\", {3}, {4}{5}", "\n",
                              option.storyTarget.name, option.storyTarget.targetName, option.storyTarget.descriptorResistance, option.storyTarget.nodeArc.name, "\n");
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid topicOption (Null) for listOfOptions[{0}]", i); }
                }
            }
        }
        else { Debug.LogWarning("Invalid listOfOptions (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// private subMethod for CheckTopicPool
    /// </summary>
    /// <param name="listOfHelp"></param>
    /// <returns></returns>
    private string CheckTopicHelp(List<StoryHelp> listOfHelp)
    {
        StringBuilder builder = new StringBuilder();
        if (listOfHelp != null)
        {
            for (int i = 0; i < listOfHelp.Count; i++)
            { builder.AppendFormat("help: {0}, \"{1}\"{2}", listOfHelp[i].name, listOfHelp[i].tag, "\n"); }
        }
        else { Debug.LogWarning("Invalid listOfHelp (Null)"); }
        return builder.ToString();
    }

    //new method above here
}
