using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// SO archetype for Trait Effect. Name of SO indicates in-game effect, eg. "ActorInvisibilityHigh" 
/// NOTE: Keep names as one intergrated word
/// NOTE: TraitEffects are Enums and are either boolean 'good' or 'bad' (no neutral) -> use 'High/Low', 'On/Off'
/// </summary>
[CreateAssetMenu(menuName = "Trait / Trait Effect")]
public class TraitEffect : ScriptableObject
{
    [Tooltip("Specifically mention here what the effect does (used in tooltips)")]
    public string descriptor;

    [HideInInspector] public int teffID;
}
