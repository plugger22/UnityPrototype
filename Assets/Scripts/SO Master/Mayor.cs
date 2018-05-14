﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for City Mayors. Name of SO is the name of the Mayor (surname), eg. "Mayor Bean"
/// </summary>
[CreateAssetMenu(menuName = "Game / Mayor")]
public class Mayor : ScriptableObject
{
    [Tooltip("Faction that the Mayor belongs to. If Mayor in a city this is will be the faction of the city")]
    public Faction faction;
    [Tooltip("Motto of Mayor in 6 words or less")]
    public string motto;

    [HideInInspector] public int mayorID;

    private Trait trait;
    private List<int> listOfTraitEffects = new List<int>();             //list of all traitEffect.teffID's

    //
    // - - -  Trait - - -
    //

    public Trait GetTrait()
    { return trait; }

    /// <summary>
    /// Replaces any existing trait and overwrites listofTraitEffects with new data. Max one trait at a time.
    /// </summary>
    /// <param name="trait"></param>
    public void AddTrait(Trait trait)
    {
        if (trait != null)
        {
            listOfTraitEffects.Clear();
            this.trait = trait;
            foreach (TraitEffect traitEffect in trait.listOfTraitEffects)
            { listOfTraitEffects.Add(traitEffect.teffID); }
        }
        else { Debug.LogError("Invalid trait (Null)"); }
    }

    /// <summary>
    /// returns true if a particular trait effect is present, false otherwise
    /// </summary>
    /// <param name="traitEffectID"></param>
    /// <returns></returns>
    public bool CheckTraitEffect(int traitEffectID)
    {
        if (listOfTraitEffects != null)
        { return listOfTraitEffects.Exists(x => x == traitEffectID); }
        else { return false; }
    }
}
