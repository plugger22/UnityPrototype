using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Scenario Challenge (difficulty). Currently Nemesis set-up. Name of SO is descriptive only, eg. "Assassin Droid"
/// </summary>
[CreateAssetMenu(menuName = "Game / Challenge")]
public class Challenge : ScriptableObject
{

    [Header("First Nemesis")]
    [Tooltip("Nemesis that will be chasing the player at game start. Leave blank if none")]
    public Nemesis nemesisFirst;
    [Tooltip("Number of turns before Nemesis comes 'Online' and starts hunting down Player")]
    [Range(0, 20)] public int gracePeriodFirst = 0;

    [Header("Second Nemesis")]
    [Tooltip("Nemesis that will be chasing the player AFTER the first nemesis catches him. Leave blank if none")]
    public Nemesis nemesisSecond;
    [Tooltip("Number of turns before Nemesis comes 'Online' and starts hunting down Player")]
    [Range(0, 20)] public int gracePeriodSecond = 0;


}
