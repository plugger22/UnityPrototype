using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Global who (Player or Actor enum). Name of SO indicates who, eg. "Player"
/// </summary>
[CreateAssetMenu(menuName = "Global / Global Who")]
public class GlobalWho : ScriptableObject
{

    public string descriptor;

    [HideInInspector] public int level;         //assigned dynamically during DataManager.initialise (0 -> Player / 1 -> Actor)
}
