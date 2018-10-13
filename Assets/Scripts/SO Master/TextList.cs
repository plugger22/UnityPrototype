using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all kinds of text data in the form of a list. Used for creating drop-in random pick lists for other SO's
/// </summary>
[CreateAssetMenu(menuName = "TextList")]
public class TextList : ScriptableObject
{
    [Tooltip("Descriptor only, not used in-game")]
    public string descriptor;
    [Tooltip("A list of strings (related) used to create a random pick list")]
    public List<string> randomList;

    /// <summary>
    /// returns a randomly selected record from the list, null if a problem
    /// </summary>
    /// <returns></returns>
    public string GetRandomRecord()
    {
        Debug.Assert(randomList != null, "Invalid pickList (Null)");
        int numOfRecords = randomList.Count;
        if (numOfRecords > 0)
        { return randomList[Random.Range(0, numOfRecords)]; }
        else { Debug.LogErrorFormat("Invalid TextList Count (zero) for {0}", this.name); }
        return null;
    }
}
