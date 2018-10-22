using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Mission setup (each city level has a mission). Name of SO is name of mission
/// </summary>
[CreateAssetMenu(menuName = "Game / Mission")]
public class Mission : ScriptableObject
{
    [Header("Main Mission parameters")]
    [Tooltip("Used as a tooltip")]
    public string descriptor;
    [Tooltip("Player briefing")]
    [TextArea] public string briefing;
    [Tooltip("Mission Timer -> how many turns")]
    [Range(20, 100)] public int timer = 100;


    [Header("Objectives")]
    [Tooltip("Primary objective (one) and optional secondary objectives (2 & 3)")]
    public Objective objectiveOne;
    public Objective objectiveTwo;
    public Objective objectiveThree;


    [Header("Targets (all Level 1)")]
    [Tooltip("Number of initial Live (visible) targets at start")]
    public int activeTargets;
    [Tooltip("Number of initial Active (hidden) targets at start")]
    public int liveTargets;
    [Tooltip("City Icon specific target, can ignore")]
    public Target iconTarget;
    [Tooltip("City airport specific target, can ignore")]
    public Target airportTarget;
    [Tooltip("City harbour specific target, can ignore")]
    public Target harbourTarget;
    [Tooltip("VIP target, can ignore")]
    public Target vipTarget;
    [Tooltip("Story target, can ignore")]
    public Target storyTarget;
    [Tooltip("Goal target, can ignore")]
    public Target goalTarget;

    [HideInInspector] public int missionID;
}
