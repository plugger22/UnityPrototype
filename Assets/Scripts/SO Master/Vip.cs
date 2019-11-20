using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for V.I.P's who move around the map (abstracted) and with whom the player can interact with
/// </summary>
[CreateAssetMenu(menuName = "Game / VIP / V.I.P")]
public class Vip : ScriptableObject
{
    [Header("Texts")]
    public string tag;
    [Tooltip("Ingame descriptor, keep short")]
    public string descriptor;
    [TextArea] public string devNotes;

    [Header("Sprite")]
    public Sprite sprite;

    [Header("Stealth")]
    [Tooltip("A resistance contact in the same node will detect presence of VIP if it's effectiveness is >= VIP's stealth rating")]
    [Range(0, 3)] public int stealthRating = 1;

    [Header("Entry")]
    [Tooltip("Earliest possible turn that rolls will being to see if VIP enters map")]
    [Range(0, 20)] public int startTurn;
    [Tooltip("Percentage chance made each turn (from startTurn) to determine if VIP enters map (true if roll less than %)")]
    [Range(0, 100)] public int startChance;

    [Header("Pathing")]
    [Tooltip("Start node")]
    public VipNode nodeStart;
    [Tooltip("Finish node (move from start node to here")]
    public VipNode nodeEnd;

    [Header("Move Profile")]
    [Tooltip("Percentage chance that, in any given turn, the VIP will move to the next node in it's path")]
    [Range(1, 100)] public int moveChance;
    [Tooltip("Max number of turns VIP will be on map (when expired it will finish it's current path and then leave (no more repeats)")]
    [Range(1, 50)] public int maxTurns;
    [Tooltip("Will VIP generate a new path (based on start/finish nodes) on completion?")]
    public bool isRepeat;

    [Header("Action")]
    [Tooltip("Action taken once player in same node as VIP and has found them")]
    public VipAction action;


    [HideInInspector] public VipStatus status;              //current status
    [HideInInspector] public bool isKnown;                  //true if VIP has been found by Player

    [HideInInspector] public int timerTurns;                //counts down from maxTurns to zero (max turns allowed on map, if zero will leave map upon reaching currentEndNode)

    [HideInInspector] public Node currentStartNode;
    [HideInInspector] public Node currentEndNode;
    [HideInInspector] public Node currentNode;              //where VIP is now


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(sprite != null, "Invalid sprite (Null) for {0}", name);
        Debug.AssertFormat(nodeStart != null, "Invalid nodeStart (Null) for {0}", name);
        Debug.AssertFormat(nodeEnd != null, "Invalid nodeEnd (Null) for {0}", name);
        Debug.AssertFormat(action != null, "Invalid action (Null) for {0}", name);
    }
}
