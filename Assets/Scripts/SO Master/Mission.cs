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
    [TextArea] public string devNotes;
    [Tooltip("Mission Timer -> how many turns")]
    [Range(20, 100)] public int timer = 100;


    [Header("Objectives")]
    [Tooltip("Primary objective (one) and optional secondary objectives (2 & 3)")]
    public Objective objectiveOne;
    public Objective objectiveTwo;
    public Objective objectiveThree;


    [Header("Targets (Level 1)")]
    [Tooltip("Number of initial Live (visible) targets at start")]
    public int targetsGenericLive;
    [Tooltip("Number of initial Active (hidden) targets at start")]
    public int targetsGenericActive;
    [Tooltip("City CityHall specific target, can ignore")]
    public Target cityHallTarget;
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

    [Header("Target Activation (Level 1)")]
    [Tooltip("Activation profile for City Hall target")]
    public TargetProfile cityHallProfile;
    [Tooltip("Activation profile for Icon target")]
    public TargetProfile iconProfile;
    [Tooltip("Activation profile for Airport target")]
    public TargetProfile airportProfile;
    [Tooltip("Activation profile for Harbour target")]
    public TargetProfile harbourProfile;
    [Tooltip("Activation profile for VIP target")]
    public TargetProfile vipProfile;
    [Tooltip("Activation profile for Story target")]
    public TargetProfile storyProfile;
    [Tooltip("Activation profile for Goal target")]
    public TargetProfile goalProfile;

    [Header("Follow-On Targets")]
    [Tooltip("City CityHall specific follow-on target, can ignore")]
    public Target cityHallFollowOnTarget;
    [Tooltip("City Icon specific follow-on target, can ignore")]
    public Target iconFollowOnTarget;
    [Tooltip("City airport specific follow-on target, can ignore")]
    public Target airportFollowOnTarget;
    [Tooltip("City harbour specific follow-on target, can ignore")]
    public Target harbourFollowOnTarget;
    [Tooltip("Story follow-on target, can ignore")]
    public Target storyFollowOnTarget;
    [Tooltip("Goal follow-on target, can ignore")]
    public Target goalFollowOnTarget;

    [HideInInspector] public int missionID;
}
