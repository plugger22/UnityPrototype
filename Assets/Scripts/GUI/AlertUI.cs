using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles the alert text UI at screen top centre (just below the Cause) which gives a message stating which nodes are being highlighted
/// </summary>
public class AlertUI : MonoBehaviour
{
    public GameObject alertObject;                                      //provides the text alert at top of map for ShowNodes
    public TextMeshProUGUI alertText;

    private static AlertUI alertUI;

    /// <summary>
    /// provide a static reference to the AlertUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static AlertUI Instance()
    {
        if (!alertUI)
        {
            alertUI = FindObjectOfType(typeof(AlertUI)) as AlertUI;
            if (!alertUI)
            { Debug.LogError("There needs to be one active AlertUI script on a GameObject in your scene"); }
        }
        return alertUI;
    }
}
