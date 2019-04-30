using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// serializable class containing all game data for save / load file operations
/// NOTE: does NOT derive from Monobehaviour
/// </summary>
[System.Serializable]
public class Save
{
    public int playerRenown;
    public ActorStatus playerStatus;
    public List<int> listOfPlayerGear;

    /// <summary>
    /// default constructor
    /// </summary>
    public Save()
    {
        playerRenown = 0;
        playerStatus = ActorStatus.Active;
        listOfPlayerGear = new List<int>();
    }

}
