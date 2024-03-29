﻿using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// place fields here that need to be initialised prior to their main use by their appropriate Manager
/// Make duplicate fields in their Manager and '[HideInInspector]' and initialise by getting value from same named field here.
/// NOTE: Fields here are ONLY to overcome sequencing issues and the main, duplicate, field in the appropriate Manager is the one that is used normally (but it's value is tied to what's here)
/// </summary>
public class PreLoadManager : MonoBehaviour
{

    [Header("PlayerManager Defaults")]
    [Tooltip("Name of Human controlled Authority Player")]
    public string nameAuthority;
    [Tooltip("Name of Human controlled Resistance Player")]
    public string nameResistance;
    [Tooltip("First name of Human Player")]
    public string nameFirst;
    [Tooltip("Sex")]
    public ActorDraftSex playerSex;
    [Tooltip("actorID of Player")]
    public int playerActorID = 999;

    [Header("Backstory Defaults")]
    [Tooltip("Previous Job")]
    public string playerJob;
    [Tooltip("Favourite Pet (animal type)")]
    public string playerPet;
    [Tooltip("Favourite Pet's name")]
    public string playerPetName;
    [Tooltip("Player Secret")]
    public string playerSecret;
    [Tooltip("Reason for joining the Resistance. 'I joined because I believe in [...]'")]
    public string playerReason;


    
    //place fields here that need to be initialised at game start, actual fields in the correct manager can access them later
    [Header("ActorManager.cs")]
    [Tooltip("The maximum number of stats (Qualities) that an actor can have")]
    [Range(2, 4)] public int numOfQualities = 3;

    /// <summary>
    /// Master Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
            case GameState.LoadGame:
            case GameState.StartUp:
                SubInitialiseAsserts();
                break;
            case GameState.FollowOnInitialisation:
                //do nothing
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    private void SubInitialiseAsserts()
    {
        Debug.Assert(string.IsNullOrEmpty(nameAuthority) == false, "Invalid nameAuthority (Null)");
        Debug.Assert(string.IsNullOrEmpty(nameResistance) == false, "Invalid nameResistance (Null)");
        Debug.Assert(string.IsNullOrEmpty(nameFirst) == false, "Invalid nameFirst (Null)");
        Debug.Assert(playerSex != null, "Invalid playerSex (Null)");
        Debug.Assert(string.IsNullOrEmpty(playerJob) == false, "Invalid playerJob (Null)");
        Debug.Assert(string.IsNullOrEmpty(playerPet) == false, "Invalid playerPet (Null)");
        Debug.Assert(string.IsNullOrEmpty(playerPetName) == false, "Invalid playerPetName (Null)");
        Debug.Assert(string.IsNullOrEmpty(playerSecret) == false, "Invalid playerSecret (Null)");
    }



}
