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

    [Header("Mechanics")]
    [Tooltip("How many actions the AI Rebel Leader can carry out per turn (base amount)")]
    [Range(1, 3)] public int actionsPerTurn = 2;
    [Tooltip("The starting pool of AI Resources")]
    [Range(1, 10)] public int resourcesStarting = 5;

    [Header("Chances")]
    [Tooltip("Chance of moving in a survival situation. A high number (75%) gets the leader moving around a lot and more likely to be captured, a middle number (50%) has them lying low more often")]
    [Range(0, 100)] public int moveChance = 50;
    [Tooltip("Chance of the Player taking an ActorArc action at their current node rather than an Actor")]
    [Range(0, 100)] public int playerChance = 30;    

    [Header("Target Attempts")]
    [Tooltip("The minimum % odds for a target that is required for the Player/Actor AI to attempt the target")]
    [Range(0, 100)] public int targetAttemptMinOdds = 50;
    [Tooltip("Chance of a player attempting a target if one present")]
    [Range(0, 100)] public int targetAttemptPlayerChance = 100;
    [Tooltip("Chance of an actor attempting a target if one present")]
    [Range(0, 100)] public int targetAttemptActorChance = 100;

    [Header("Gear")]
    [Tooltip("Amount of starting gear points in the gear pool (NOTE: will automatically be adjusted downwards if greater than the maximum allowable amount)")]
    public int gearPoints = 2;

    [Header("Priorities (Low/Med/High)")]
    [Tooltip("Player going on stress leave. Default Medium")]
    public GlobalChance stressLeavePlayer;
    [Tooltip("Actor going on stress leave. Default Low")]
    public GlobalChance stressLeaveActor;
    [Tooltip("Player Moving to target. Default Medium")]
    public GlobalChance movePriority;
    [Tooltip("Raise faction Approval task. Default Low")]
    public GlobalChance approvalPriority;
    [Tooltip("Player Idling. Default Low")]
    public GlobalChance idlePriority;

    [Header("Task Priorities (Low/Med/High)")]
    [Tooltip("Priority for any actor / player Anarchist Task")]
    public GlobalChance taskAnarchist;
    [Tooltip("Priority for any actor / player Blogger Task")]
    public GlobalChance taskBlogger;
    [Tooltip("Priority for any actor / player Fixer Task")]
    public GlobalChance taskFixer;
    [Tooltip("Priority for any actor / player Hacker Task")]
    public GlobalChance taskHacker;
    [Tooltip("Priority for any actor / player Heavy Task")]
    public GlobalChance taskHeavy;
    [Tooltip("Priority for any actor / player Observer Task")]
    public GlobalChance taskObserver;
    [Tooltip("Priority for any actor / player Operator Task")]
    public GlobalChance taskOperator;
    [Tooltip("Priority for any actor / player Planner Task")]
    public GlobalChance taskPlanner;
    [Tooltip("Priority for any actor / player Recruiter Task")]
    public GlobalChance taskRecruiter;

    [Header("Target Priorities (Low/Med/High)")]
    [Tooltip("Player priority for resolving targets")]
    public GlobalChance targetPlayer;
    [Tooltip("Actor priority for resolving targets")]
    public GlobalChance targetActor;

    [Header("Management Priorities (Low/Med/High)")]
    [Tooltip("The number of reserve actors that will be added at end of autoRun. Low gives none, Med one, High two")]
    public GlobalChance manageReserve;
    [Tooltip("Priority given to dismissing subordinates with the Questionable trait (Low/Med/High)")]
    public GlobalChance manageQuestionable;

    /// <summary>
    /// initialisation
    /// </summary>
    private void OnEnable()
    {
        Debug.Assert(actionsPerTurn == 2, "Invalid actionsPerTurn (should be 2)");
    }

}
