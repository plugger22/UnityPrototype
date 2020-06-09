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
    [Tooltip("Which side is the mission for")]
    public GlobalSide side;
    [Tooltip("Player briefing")]
    [TextArea] public string briefing;
    [TextArea] public string devNotes;

    [Header("Objectives")]
    [Tooltip("Up to 3 possible objectives (can have more but they are ignored)")]
    public List<Objective> listOfObjectives;

    [Header("Targets (Base)")]
    [Tooltip("Number of initial Live (visible) targets at start")]
    public int targetsGenericLive;
    [Tooltip("Number of initial Active (hidden) targets at start")]
    public int targetsGenericActive;
    [Tooltip("City CityHall specific target, can ignore")]
    public Target targetBaseCityHall;
    [Tooltip("City Icon specific target, can ignore")]
    public Target targetBaseIcon;
    [Tooltip("City airport specific target, can ignore")]
    public Target targetBaseAirport;
    [Tooltip("City harbour specific target, can ignore")]
    public Target targetBaseHarbour;
    [Tooltip("VIP target, can ignore")]
    public Target targetBaseVIP;
    [Tooltip("Story target, can ignore")]
    public Target targetBaseStory;
    [Tooltip("Goal target, can ignore")]
    public Target targetBaseGoal;
    [Tooltip("Organisation target (use template target, dynamically assigned data each level). If none then no organisations will make contact in mission")]
    public Target targetOrganisation;

    [Header("Generic Target Profile Overrides")]
    [Tooltip("If a profile is specified here it will override the Generic Target's profile. Can be ignored")]
    public TargetProfile profileGenericLive;
    [Tooltip("If a profile is specified here it will override the Generic Target's profile. Can be ignored")]
    public TargetProfile profileGenericActive;
    [Tooltip("All followOn Generic targets will use this profile. If ignored, the targets own profile will be used")]
    public TargetProfile profileGenericFollowOn;

    [Header("Objective Targets")]
    [Tooltip("Targets that are connected to Objectives. A target can affect multiple objects (ObjectiveTargets)")]
    public List<ObjectiveTarget> listOfObjectiveTargets;

    [Header("Npc")]
    [Tooltip("Npc (max one) who has a walk on role for this mission. Can be ignored")]
    public Npc npc;


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Mission {0} has Invalid descriptor (Null or Empty)", name);
        Debug.AssertFormat(side != null, "Mission {0} has Invalid side (Null)", name);
        Debug.AssertFormat(listOfObjectives.Count > 0, "Mission {0} has Invalid listOfObjectives (Empty)", name);
        Debug.AssertFormat(profileGenericLive != null, "Invalid profileGenericLive for {0}", name);
        Debug.AssertFormat(profileGenericActive != null, "Invalid profileGenericActive for {0}", name);
        Debug.AssertFormat(profileGenericFollowOn != null, "Invalid profileGenericFollowOn for {0}", name);
    }

}
