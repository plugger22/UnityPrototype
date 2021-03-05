using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Authority Nemesis. Name of SO is the name of the nemesis, eg. "Assassin Droid"
/// </summary>
[CreateAssetMenu(menuName = "Authority / Nemesis")]
public class Nemesis : ScriptableObject
{
    [Tooltip("In game tooltip descriptor")]
    [TextArea] public string descriptor;

    [Header("Base Stats")]
    [Tooltip("The number of districts the droid can move per turn")]
    [Range(0, 3)] public int movement = 1;
    [Tooltip("Will detect Player if at the same node if search rating >= Player's current invisibility")]
    [Range(0, 3)] public int searchRating = 1;
    [Tooltip("A resistance contact in the same node will detect presence of nemesis if it's effectiveness is >= droid's stealth rating")]
    [Range(0, 3)] public int stealthRating = 1;
    [Tooltip("Type of damage dealt out if the nemesis catches up with the Player")]
    public Damage damage;


    public void OnEnable()
    {
        Debug.Assert(damage != null, string.Format("Invalid damage (Null) for nenesis {0}", this.name));
    }

}
