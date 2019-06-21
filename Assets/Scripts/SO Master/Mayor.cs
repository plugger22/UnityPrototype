using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for City Mayors. Name of SO is the name of the Mayor (surname), eg. "Mayor Bean"
/// </summary>
[CreateAssetMenu(menuName = "Game / Mayor")]
public class Mayor : ScriptableObject
{
    [Header("Assorted")]
    /*[Tooltip("Faction that the Mayor belongs to. If Mayor in a city this is will be the faction of the city")]
    public Faction faction;*/
    [Tooltip("Name used in game")]
    public string mayorName;
    [Tooltip("Motto of Mayor in 6 words or less")]
    public string motto;
    [Tooltip("Portrait sprite (152 x 160 png)")]
    public Sprite sprite;
    [Tooltip("Unique characteristic (trait) of the Mayor")]
    public Trait trait;

    [Header("Node Arcs")]
    [Tooltip("Node type that faction wants to protect (authority) or turn a blind eye to (resistance). Leave blank if none.")]
    public NodeArc preferredArc;
    /*[Tooltip("Node type that faction wants to destroy (resistance) or ignore (authority). Leave blank if none. Currently not used")]
    public NodeArc hostileArc;*/

    [Header("Mechanics")]
    [Tooltip("How many actions the AI Mayor can carry out per turn (base amount) Default 2")]
    [Range(1, 3)] public int actionsPerTurn = 2;
    [Tooltip("The starting pool of AI Resources")]
    [Range(1, 10)] public int resourcesStarting = 5;





    [Tooltip("Used for testing purposes only. If 'ON' the Mayor is ignored (DataManager.cs -> GetRandomMayor). Leave as OFF")]
    public bool isTestOff = false;

    private List<string> listOfTraitEffects = new List<string>();             //list of all traitEffect.name's

    /// <summary>
    /// initialisation
    /// </summary>
    private void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(mayorName) == false, "Invalid mayorName (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(motto) == false, "Invalid motto (Null or Empty) for {0}", name);
        Debug.AssertFormat(trait != null, "Invalid trait (Null) for {0}", name);
        Debug.AssertFormat(sprite != null, "Invalid sprite (Null) for {0}", name);
        Debug.AssertFormat(actionsPerTurn == 2, "Invalid actionsPerTurn (should be 2) for {0}", name);
        Debug.AssertFormat(resourcesStarting > 0 && resourcesStarting < 20, "Invalid resourcesStarting (should be between 0 and 20) for {0}", name);
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
            { listOfTraitEffects.Add(traitEffect.name); }
        }
        else { Debug.LogError("Invalid trait (Null)"); }
    }

    /// <summary>
    /// returns true if a particular trait effect is present, false otherwise
    /// </summary>
    /// <param name="traitEffectID"></param>
    /// <returns></returns>
    public bool CheckTraitEffect(string effectName)
    {
        if (string.IsNullOrEmpty(effectName) == false)
        {
            if (listOfTraitEffects != null)
            { return listOfTraitEffects.Exists(x => x == effectName); }
            else { Debug.LogError("Invalid listOfTraitEffects (Null)"); }
        }
        return false;
    }
}
