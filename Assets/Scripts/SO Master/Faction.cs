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

    [Header("Node Arcs")]
    [Tooltip("Node type that faction wants to protect (authority) or turn a blind eye to (resistance). Leave blank if none.")]
    public NodeArc preferredArc; 
    [Tooltip("Node type that faction wants to destroy (resistance) or ignore (authority). Leave blank if none.")]
    public NodeArc hostileArc;

    [Header("AI Tasks")]
    /*[Tooltip("How many actions the AI faction can carry out per turn")]
    [Range(1,3)] public int actionsTaskPerTurn = 3;*/
    [Header("AI Resources")]
    [Tooltip("The number of AI Resources granted per turn (Resistance side only) provided Faction decides to provide support (dependant on faction level as per normal)")]
    [Range(1, 3)] public int resourcesAllowance = 1;
    /*[Tooltip("The starting pool of AI Resources used for Decisions")]
    [Range(1, 10)] public int resourcesStarting = 5;*/
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
