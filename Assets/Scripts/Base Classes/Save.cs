using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Master serializable class containing all game data for save / load file operations
/// NOTE: does NOT derive from Monobehaviour
/// </summary>
[System.Serializable]
public class Save
{
    public SavePlayerData playerData;

    ///
    public Save()
    {
        playerData = new SavePlayerData();
    }

}

/// <summary>
/// Player data
/// </summary>
[System.Serializable]
public class SavePlayerData
{
    public int renown;
    public ActorStatus status;
    public List<int> listOfGear;

    /// <summary>
    /// default constructor
    /// </summary>
    public SavePlayerData()
    {
        renown = 0;
        status = ActorStatus.Active;
        listOfGear = new List<int>();
    }
}




