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
    //place fields here that need to be initialised at game start, actual fields in the correct manager can access them later
    [Header("ActorManager.cs")]
    [Tooltip("The maximum number of stats (Qualities) that an actor can have")]
    [Range(2, 4)] public int numOfQualities = 3;


    public void Initialise()
    { }
}
