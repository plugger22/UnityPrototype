using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Scenario Challenge (difficulty). Name of SO is descriptive only, eg. "Assassin Droid, 30 turns"
/// </summary>
[CreateAssetMenu(menuName = "Game / Challenge")]
public class Challenge : ScriptableObject
{


    [Tooltip("Nemesis that will be chasing the player. Leave blank if none")]
    public Nemesis nemesis;

    [Tooltip("Number of turns before Nemesis comes 'Online' and starts hunting down Player")]
    [Range(0, 20)] public int gracePeriod = 0;


}
