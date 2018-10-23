using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Handles mission and level set-up
/// </summary>
public class MissionManager : MonoBehaviour
{

    public Mission mission;

    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        Debug.Assert(mission != null, "Invalid mission (Null)");
        GameManager.instance.targetScript.AssignTargets(mission);
    }

 



}
