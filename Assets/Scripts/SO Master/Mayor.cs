﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for City Mayors. Name of SO is the name of the Mayor (surname), eg. "Mayor Bean"
/// </summary>
[CreateAssetMenu(menuName = "Game / Mayor")]
public class Mayor : ScriptableObject
{
    [Header("Assorted")]
    [Tooltip("Faction that the Mayor belongs to. If Mayor in a city this is will be the faction of the city")]
    public Faction faction;
    [Tooltip("Motto of Mayor in 6 words or less")]
    public string motto;
    [Tooltip("Portrait sprite (152 x 160 png)")]
    public Sprite sprite;
    [Tooltip("Unique characteristic (trait) of the Mayor")]
    public Trait trait;

    [Header("Node Arcs")]
    [Tooltip("Node type that faction wants to protect (authority) or turn a blind eye to (resistance). Leave blank if none.")]
    public NodeArc preferredArc;
    [Tooltip("Node type that faction wants to destroy (resistance) or ignore (authority). Leave blank if none.")]
    public NodeArc hostileArc;

    [Header("AI Tasks")]
    [Tooltip("How many actions the AI Mayor can carry out per turn (base amount)")]
    [Range(2, 2)] public int actionsPerTurn = 2;
    [Header("AI Resources")]
    [Tooltip("The number of AI Resources granted per turn (for Resistance side only) provided Faction decides to provide support (dependant on faction level as per normal)")]
    [Range(1, 3)] public int resourcesAllowance = 1;
    [Tooltip("The starting pool of AI Resources used for Decisions")]
    [Range(1, 10)] public int resourcesStarting = 5;


    [HideInInspector] public int mayorID;



    [Tooltip("Used for testing purposes only. If 'ON' the Mayor is ignored (DataManager.cs -> GetRandomMayor). Leave as OFF")]
    public bool isTestOff = false;

    private List<int> listOfTraitEffects = new List<int>();             //list of all traitEffect.teffID's

    /// <summary>
    /// initialisation
    /// </summary>
    private void OnEnable()
    {
        Debug.Assert(sprite != null, "Invalid sprite (Null)");
    }

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
