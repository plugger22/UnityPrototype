using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO archetype for Trait Category. Name of SO indicates category, eg. "Actor"
/// </summary>
[CreateAssetMenu(menuName = "Trait / Trait Category")]
public class TraitCategory : ScriptableObject
{
    public string descriptor;


    [HideInInspector] public int tcatID;


}
