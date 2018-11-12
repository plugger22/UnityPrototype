using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Scenario Challenge (difficulty). Name of SO is descriptive only, eg. "Assassin Droid, 30 turns"
/// </summary>
[CreateAssetMenu(menuName = "Game / Challenge")]
public class Challenge : ScriptableObject
{

    [Tooltip("Scenerio Timer -> how many turns")]
    [Range(20, 100)] public int timer = 100;

    [Tooltip("Nemesis that will be chasing the player. Leave blank if none")]
    public Nemesis nemesis;


}
