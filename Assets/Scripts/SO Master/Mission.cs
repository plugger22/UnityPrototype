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

    [Header("Generic Target Profile Overrides")]
    [Tooltip("If a profile is specified here it will override the Generic Target's profile. Can be ignored")]
    public TargetProfile profileGenericLive;
    [Tooltip("If a profile is specified here it will override the Generic Target's profile. Can be ignored")]
    public TargetProfile profileGenericActive;
    [Tooltip("All followOn Generic targets will use this profile. If ignored, the targets own profile will be used")]
    public TargetProfile profileGenericFollowOn;

    /*[Tooltip("If a profile is specified here it will override the Target's profile. Can be ignored")]
    public TargetProfile profileBaseCityHall;
    [Tooltip("If a profile is specified here it will override the Target's profile. Can be ignored")]
    public TargetProfile profileBaseIcon;
    [Tooltip("If a profile is specified here it will override the Target's profile. Can be ignored")]
    public TargetProfile profileBaseAirport;
    [Tooltip("If a profile is specified here it will override the Target's profile. Can be ignored")]
    public TargetProfile profileBaseHarbour;
    [Tooltip("If a profile is specified here it will override the Target's profile. Can be ignored")]
    public TargetProfile profileBaseVIP;
    [Tooltip("If a profile is specified here it will override the Target's profile. Can be ignored")]
    public TargetProfile profileBaseStory;
    [Tooltip("If a profile is specified here it will override the Target's profile. Can be ignored")]
    public TargetProfile profileBaseGoal;

    [Header("Follow-On Target Overrides")]
    [Tooltip("If a target is specified here it will override any Target's follow-on. Can be ignored")]
    public Target targetFollowCityHall;
    [Tooltip("If a target is specified here it will override any Target's follow-on. Can be ignored")]
    public Target targetFollowIcon;
    [Tooltip("If a target is specified here it will override any Target's follow-on. Can be ignored")]
    public Target targetFollowAirport;
    [Tooltip("If a target is specified here it will override any Target's follow-on. Can be ignored")]
    public Target targetFollowHarbour;
    [Tooltip("If a target is specified here it will override any Target's follow-on. Can be ignored")]
    public Target targetFollowStory;
    [Tooltip("If a target is specified here it will override any Target's follow-on. Can be ignored")]
    public Target targetFollowGoal;

    [Header("Target Follow-On Profile Overrides")]
    [Tooltip("If a profile is specified here it will override the Follow-On Target's profile. Can be ignored")]
    public TargetProfile profileFollowGenericLive;
    [Tooltip("If a profile is specified here it will override the Follow-On Target's profile. Can be ignored")]
    public TargetProfile profileFollowGenericActive;
    [Tooltip("If a profile is specified here it will override the Follow-On Target's profile. Can be ignored")]
    public TargetProfile profileFollowCityHall;
    [Tooltip("If a profile is specified here it will override the Follow-On Target's profile. Can be ignored")]
    public TargetProfile profileFollowIcon;
    [Tooltip("If a profile is specified here it will override the Follow-On Target's profile. Can be ignored")]
    public TargetProfile profileFollowAirport;
    [Tooltip("If a profile is specified here it will override the Follow-On Target's profile. Can be ignored")]
    public TargetProfile profileFollowHarbour;
    [Tooltip("If a profile is specified here it will override the Follow-On Target's profile. Can be ignored")]
    public TargetProfile profileFollowVIP;
    [Tooltip("If a profile is specified here it will override the Follow-On Target's profile. Can be ignored")]
    public TargetProfile profileFollowStory;
    [Tooltip("If a profile is specified here it will override the Follow-On Target's profile. Can be ignored")]
    public TargetProfile profileFollowGoal;*/

    [HideInInspector] public int missionID;
}
