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
    public SaveDataData dataData = new SaveDataData();
    public SavePlayerData playerData = new SavePlayerData();
    public SaveSideData sideData = new SaveSideData();

}

//
// - - - Managers - - -
//

/// <summary>
/// DataManager.cs data
/// </summary>
[System.Serializable]
public class SaveDataData
{
    //secrets
    public List<int> listOfPlayerSecrets = new List<int>();
    public List<int> listOfRevealedSecrets = new List<int>();
    public List<int> listOfDeletedSecrets = new List<int>();
    public List<SaveSecret> listOfSecretChanges = new List<SaveSecret>();
}


/// <summary>
/// PlayerManager.cs data
/// </summary>
[System.Serializable]
public class SavePlayerData
{
    public int renown;
    public int Invisibility;
    public ActorStatus status;
    public ActorTooltip tooltipStatus;
    public ActorInactive inactiveStatus;
    public bool isBreakdown;
    public bool isEndOfTurnGearCheck;
    public bool isLieLowFirstturn;   
    public bool isStressLeave;
    public List<int> listOfGear = new List<int>();
    public List<int> listOfSecrets = new List<int>();
    public List<string> listOfConditionsResistance = new List<string>();
    public List<string> listOfConditionsAuthority = new List<string>();   
}


/// <summary>
/// SideManager.cs data
/// </summary>
[System.Serializable]
public class SaveSideData
{
    public SideState resistanceCurrent;
    public SideState authorityCurrent;
    public SideState resistanceOverall;
    public SideState authorityOverall;
    public GlobalSide playerSide;
}

//
// - - - SO's - - -
//

/// <summary>
/// Secret.S0 dynamic data
/// </summary>
[System.Serializable]
public class SaveSecret
{
    public int secretID;
    public int gainedWhen;
    public int revealedWho;
    public int revealedWhen;
    public int deleteWhen;
    public SecretStatus status;
    public List<int> listOfActors = new List<int>();
}




