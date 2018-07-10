using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gear SO. Name of SO is the name of the Conflict, eg. "Resign"
/// </summary>
[CreateAssetMenu(menuName = "Actor / Conflict")]
public class ActorConflict : ScriptableObject
{
    [Tooltip("Not used in-game, descriptive only")]
    public string description;
    [Tooltip("Outcome text (outcome dialogue, single line, keep short) in format  '[arc.name] ...'")]
    public string outcomeText;
    [Tooltip("Which side does it apply too (or 'Both') Used internally by ActorManager.cs -> GetActorConflict to correctly set up call to EffectManager.cs -> ProcessEffect")]
    public GlobalSide side;
    [Tooltip("Determines how many entries are placed in the selection pool for this actorConflict")]
    public GlobalChance chance;
    [Tooltip("Who does the effect apply to?")]
    public GlobalWho who;
    [Tooltip("Criteria. NOTE: Separate from effect cause some effects don't have criteria when I want them. Any effect criteria are ignored. Criteria is for 'who' (Player or Actor")]
    public List<Criteria> listOfCriteria;
    [Tooltip("Is it a good or bad outcome from the Player's POV")]
    public GlobalType type;
    [Tooltip("There can only be a single effect. Leave empty if for 'Nothing happens'")]
    public Effect effect;
    [Tooltip("Used for testing purposes only. If ON the conflict is ignored (fails criteria check). Leave as OFF")]
    public bool isTestOff = false;

    [HideInInspector] public int conflictID;

}
