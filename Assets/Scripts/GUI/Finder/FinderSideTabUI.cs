using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinderSideTabUI : MonoBehaviour
{

    private static FinderSideTabUI finderSideTabUI;

    /// <summary>
    /// provide a static reference to FinderUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static FinderSideTabUI Instance()
    {
        if (!finderSideTabUI)
        {
            finderSideTabUI = FindObjectOfType(typeof(FinderSideTabUI)) as FinderSideTabUI;
            if (!finderSideTabUI)
            { Debug.LogError("There needs to be one active FinderSideTabUI script on a GameObject in your scene"); }
        }
        return finderSideTabUI;
    }
}
