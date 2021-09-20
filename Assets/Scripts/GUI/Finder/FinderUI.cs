using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles RHS Finder (district finder) UI
/// </summary>
public class FinderUI : MonoBehaviour
{

    private static FinderUI finderUI;


    /// <summary>
    /// provide a static reference to FinderUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static FinderUI Instance()
    {
        if (!finderUI)
        {
            finderUI = FindObjectOfType(typeof(FinderUI)) as FinderUI;
            if (!finderUI)
            { Debug.LogError("There needs to be one active FinderUI script on a GameObject in your scene"); }
        }
        return finderUI;
    }


}
