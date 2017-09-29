﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Actor trait
/// </summary>
[CreateAssetMenu(menuName = "Trait")]
public class Trait : ScriptableObject
{
    public int TraitID { get; set; }               //unique #, zero based -> assigned automatically by DataManager.Initialise
    public bool isGood = false;

}
