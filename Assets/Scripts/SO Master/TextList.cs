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
    /// checks if a given item string is present in list. True if so, false otherwise
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool CheckItemPresent(string item)
    { return randomList.Exists(x => x.Equals(item, System.StringComparison.Ordinal)); }

}
