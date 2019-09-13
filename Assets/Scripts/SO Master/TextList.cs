using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// handles all kinds of text data in the form of a list. Used for creating drop-in random pick lists for other SO's
/// </summary>
[CreateAssetMenu(menuName = "TextList / TextList")]
public class TextList : ScriptableObject
{
    [Tooltip("Descriptor only, not used in-game")]
    [TextArea] public string descriptor;
    [Tooltip("Category of TextList")]
    public TextListType category;
    [Tooltip("A list of strings (related) used to create a random pick list")]
    public List<string> randomList;

    [Header("Testing")]
    [Tooltip("If true randomList is tested for duplicates (use for Textlists that contain short strings, avoid for longer strings), not if false")]
    public bool isTestForDuplicates;
    [Tooltip("If true test for valid Text Tags using NewsManager.cs -> CheckNewsText, ignore if false")]
    public bool isTestForTextTags;

    #region Save Data Compatible
    private int index;
    #endregion


    public void OnEnable()
    {
        //constructor
        isTestForDuplicates = false;
        isTestForTextTags = true;
        //asserts
        Debug.AssertFormat(category != null, "Invalid category (Null) for {0}", name);
        Debug.AssertFormat(randomList != null && randomList.Count > 0, "Invalid randomList (Null or Empty) for {0}", name);
    }


    /// <summary>
    /// returns a randomly selected record from the list, null if a problem (default) but if 'isNull' is false returns string 'Unknown'
    /// </summary>
    /// <returns></returns>
    public string GetRandomRecord(bool isNull = true)
    {
        Debug.Assert(randomList != null, "Invalid pickList (Null)");
        int numOfRecords = randomList.Count;
        if (numOfRecords > 0)
        { return randomList[Random.Range(0, numOfRecords)]; }
        else { Debug.LogErrorFormat("Invalid TextList Count (zero) for {0}", name); }
        if (isNull == false)
        { return "Unknown"; }
        return null;
    }

    /// <summary>
    /// returns next record and increments index. Used to cycle through entire lists one at a time to give maximum expose to all records. Alternate method to GetRandomRecord
    /// </summary>
    /// <param name="isNull"></param>
    /// <returns></returns>
    public string GetIndexedRecord(bool isNull = true)
    {
        string text = "";
        if (index > -1)
        {
            text = randomList[index];
            //increment index
            index++;
            //rollover
            if (index == randomList.Count)
            { index = 0; }
        }
        //invalid record
        if (string.IsNullOrEmpty(text) == true)
        {
            if (isNull == true)
            { text = null; }
            else { text = "Unknown"; }
        }
        return text;
    }

    /// <summary>
    /// sets index for randomlist to a random value at start of a new game
    /// </summary>
    public void InitialiseIndex()
    {
        if (randomList != null && randomList.Count > 0)
        {
            index = Random.Range(0, randomList.Count);
            /*Debug.LogFormat("[Tst] TextList.SO -> InitialiseIndex: index {0}, out of {1} records, for {2}{3}", index, randomList.Count, name, "\n");*/
        }
        else { index = -1; }
    }

    /// <summary>
    /// checks if a given item string is present in list. True if so, false otherwise
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool CheckItemPresent(string item)
    { return randomList.Exists(x => x.Equals(item, System.StringComparison.Ordinal)); }

}
