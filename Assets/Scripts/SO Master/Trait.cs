using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Actor trait
/// </summary>
[CreateAssetMenu(menuName = "Actor / Trait")]
public class Trait : ScriptableObject
{
    [HideInInspector] public int traitID;           //unique #, zero based -> assigned automatically by DataManager.Initialise

    [Tooltip("Is the trait good, neutral or bad (value assigned dynamically 2 / 1 / 0)")]
    public GlobalType typeOfTrait;
    public GlobalSide side;

}
