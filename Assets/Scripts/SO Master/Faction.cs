using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Factions. Name of SO is the name of the faction, eg. "Nihilists"
/// </summary>
[CreateAssetMenu(menuName = "Game / Faction")]
public class Faction : ScriptableObject
{
    public string descriptor;
    public GlobalSide side;

    /*[Header("Node Arcs")]
    [Tooltip("Node type that faction wants to protect (authority) or turn a blind eye to (resistance). Leave blank if none.")]
    public NodeArc preferredArc; 
    [Tooltip("Node type that faction wants to destroy (resistance) or ignore (authority). Leave blank if none.")]
    public NodeArc hostileArc;*/

    [Tooltip("Pictorial representation of faction (152 x 160 png)")]
    public Sprite sprite;

    
    [HideInInspector] public int factionID;         //dynamically assigned by DataManager.cs on import

    private Trait trait;
    private List<int> listOfTraitEffects = new List<int>();             //list of all traitEffect.teffID's

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
