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
    public SaveActorData actorData = new SaveActorData();

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

/// <summary>
/// ActorManager.cs
/// </summary>
[System.Serializable]
public class SaveActorData
{
    public List<SaveActor> listOfDictActors = new List<SaveActor>();
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

//
// - - - Base Classes - - -
//


/// <summary>
/// Actor.cs full data set
/// </summary>
[System.Serializable]
public class SaveActor
{
    public ActorStatus status;
    public int actorID;
    public int datapoint0;
    public int datapoint1;
    public int datapoint2;
    public GlobalSide side;
    public int slotID;
    public int level;
    public int nodeCaptured;
    public int unhappyTimer;
    public int blackmailTimer;
    public int captureTimer;
    public int numOfTimesBullied;
    public int numOfTimesCaptured;
    public int departedNumOfSecrets;
    public bool isPromised;
    public bool isNewRecruit;
    public bool isReassured;
    public bool isThreatening;
    public bool isComplaining;
    public bool isBreakdown;
    public bool isLieLowFirstturn;
    public bool isStressLeave;
    public bool isTraitor;
    public bool isMale;
    public string actorName;
    public string firstName;
    public int arcID;
    public ActorTooltip tooltipStatus;
    public ActorInactive inactiveStatus;
    public Trait trait;
    public int Renown;
    //gear
    public int gearID;
    public int gearTimer;
    public int gearTimesTaken;
    //collections
    public List<int> listOfTeams = new List<int>();
    public List<int> listOfSecrets = new List<int>();
    public List<string> listOfConditions = new List<string>();
    
    public List<int> listOfContactNodes = new List<int>();
    public List<int> listOfContacts = new List<int>();
    //trait effects generated dynamically when loading trait
    /*public List<int> listOfTraitEffects = new List<int>();*/
}