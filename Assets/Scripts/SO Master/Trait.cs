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
    [Tooltip("A one/two word trait tag that is used for the UI (NOT trait.name)")]
    public string tag;
    [Tooltip("Used as a description in the trait tooltip")]
    public string description;
    [Tooltip("Is the trait good, neutral or bad from Point of View of Resistance (value assigned dynamically  2 / 1 / 0)")]
    public GlobalType typeOfTrait;
    [Tooltip("Only add a side if the trait is specific to a particular side, otherwise leave as 'None'")]
    public GlobalSide side;
    [Tooltip("What type of trait is it? Any SO with this trait will be error checked on import to confirm that it has the correct trait category")]
    public TraitCategory category;
    [Tooltip("Specific in-game effects of the trait")]
    public List<TraitEffect> listOfTraitEffects;

    [HideInInspector] public string tagFormatted;                     //pre-formatted (TextMeshPro) string ready for trait display (initialised on import)
   
}
