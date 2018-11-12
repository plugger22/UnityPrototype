using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Handles mission and level set-up
/// </summary>
public class MissionManager : MonoBehaviour
{
    [HideInInspector] public Mission mission;

    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        Debug.Assert(mission != null, "Invalid Mission (Null)");
        //initialise and assign targets
        GameManager.instance.targetScript.Initialise();
        GameManager.instance.targetScript.AssignTargets(mission);
    }

 



}
