using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Rebel AI opponent. Name of SO could be anything (it's not used), eg. "Herbert the Hermit"
/// </summary>
[CreateAssetMenu(menuName = "Game / Rebel Leader")]
public class RebelLeader : ScriptableObject
{
    [Header("Base Stats")]
    [Tooltip("Short nickname of Leader")]
    public string tag;
    [Tooltip("First and Second Name")]
    public string leaderName;
    [Tooltip("Description used in game")]
    [TextArea] public string descriptor;
    [Tooltip("Description of AI personality profile, not used in game")]
    [TextArea] public string designNotes;

    [Header("Survival")]
    [Tooltip("Chance of moving in a survival situation. A high number (75%) gets the leader moving around a lot and more likely to be captured, a middle number (50%) has them lying low more often")]
    public int moveChance = 50;

    [Header("Priorities (can only be Low / med / high)")]
    [Tooltip("Player going on stress leave. Default Medium")]
    public GlobalChance stressLeavePlayer;
    [Tooltip("Actor going on stress leave. Default Low")]
    public GlobalChance stressLeaveActor;
    [Tooltip("Player Moving to target. Default Medium")]
    public GlobalChance movePriority;
    [Tooltip("Player Idling. Default Low")]
    public GlobalChance idlePriority;
}
