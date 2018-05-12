using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Trait SO, can be used for Actors, Mayors, Factions
/// </summary>
[CreateAssetMenu(menuName = "Trait / Trait")]
public class Trait : ScriptableObject
{
    [HideInInspector] public int traitID;           //unique #, zero based -> assigned automatically by DataManager.Initialise
    public string description;
    [Tooltip("Is the trait good, neutral or bad (value assigned dynamically 2 / 1 / 0)")]
    public GlobalType typeOfTrait;
    public GlobalSide side;
    [Tooltip("What type of trait is it? Any SO with this trait will be error checked on import to confirm that it has the correct trait category")]
    public TraitCategory category;

}
