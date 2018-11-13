using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to create a national / cultural / geographic name set containing a Textlist for male and female first names as well as another for last names
/// </summary>
[CreateAssetMenu(menuName = "Contact / NameSet")]
public class NameSet : ScriptableObject
{

    [Tooltip("TextList for Male first names. Need 50 records")]
    public TextList firstMaleNames;
    [Tooltip("TextList for Female first names. Need 50 records")]
    public TextList firstFemaleNames;
    [Tooltip("TextList for LAST names. Need 100 records")]
    public TextList lastNames;

    public void OnEnable()
    {
        /*Debug.Assert(firstMaleNames != null, "Invalid firstMaleNames (Null)");
        Debug.Assert(firstFemaleNames != null, "Invalid firstFemaleNames (Null)");
        Debug.Assert(lastNames != null, "Invalid lastNames (Null)");*/

    }

}
