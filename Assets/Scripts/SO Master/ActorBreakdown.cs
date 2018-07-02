using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gear SO. Name of SO is the name of the Breakdown, eg. "Resign"
/// </summary>
[CreateAssetMenu(menuName = "Actor / Breakdown")]
public class ActorBreakdown : ScriptableObject
{
    [Tooltip("Not used in-game, descriptive only")]
    public string description;
    [Tooltip("Outcome text (outcome dialogue, single line, keep short) in format  '[arc.name] ...'")]
    public string outcomeText;
    [Tooltip("Which side does it apply too (or 'Both')")]
    public GlobalSide side;
    [Tooltip("Determines how many entries are placed in the selection pool for this actorBreakdown")]
    public GlobalChance chance;
    [Tooltip("Who does the effect apply to?")]
    public GlobalWho who;
    [Tooltip("There can only be a single effect. Leave empty if for 'Nothing happens'")]
    public Effect effect;

    [HideInInspector] public int actBreakID;

}
