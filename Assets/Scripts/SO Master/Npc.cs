using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Non Player Characters who move around the map (abstracted) and with whom the player can interact with
/// </summary>
[CreateAssetMenu(menuName = "Game / Npc / Npc")]
public class Npc : ScriptableObject
{
    [Header("Texts")]
    [Tooltip("The name needs to be one that can handle '[A] ...' and NOT '[An] ...'")]
    public string tag;
    [Tooltip("Ingame descriptor, keep short")]
    public string descriptor;
    [Tooltip("Item in the format '[they have]...', keep short")]
    public string item;

    [Header("Sprite")]
    public Sprite sprite;

    [Header("Stealth")]
    [Tooltip("A resistance contact in the same node will detect presence of Npc if it's effectiveness is >= Npc's stealth rating")]
    [Range(0, 3)] public int stealthRating = 1;
    [Tooltip("If true, Npc ignores Invisible Mode and will be automatically interdicted whenever at same node as player")]
    public bool isIgnoreInvisible;

    [Header("Entry")]
    [Tooltip("Earliest possible turn that rolls will being to see if Npc enters map")]
    [Range(0, 20)] public int startTurn;
    [Tooltip("Percentage chance made each turn (from startTurn) to determine if Npc enters map (true if roll less than %)")]
    [Range(0, 100)] public int startChance;

    [Header("Pathing")]
    [Tooltip("Start node")]
    public NpcNode nodeStart;
    [Tooltip("Finish node (move from start node to here")]
    public NpcNode nodeEnd;

    [Header("Move Profile")]
    [Tooltip("Percentage chance that, in any given turn, the Npc will move to the next node in it's path")]
    [Range(1, 100)] public int moveChance;
    [Tooltip("Max number of turns Npc will be on map (when expired it will finish it's current path and then leave (no more repeats)")]
    [Range(1, 50)] public int maxTurns;
    [Tooltip("Will Npc generate a new path (based on start/finish nodes) on completion?")]
    public bool isRepeat;

    [Header("Action")]
    [Tooltip("Action taken once player in same node as Npc and has found them")]
    public NpcAction action;

    [Header("Effects")]
    [Tooltip("Good effects that happen upon interacting with Npc")]
    public List<Effect> listOfGoodEffects;
    [Tooltip("Bad effects that happen if you fail to interact with Npc before they depart")]
    public List<Effect> listOfBadEffects;

    #region Save Compatible Data
    [HideInInspector] public NpcStatus status;                                      //current status
    [HideInInspector] public int timerTurns;                                        //counts down from maxTurns to zero (max turns allowed on map, if zero will leave map upon reaching currentEndNode)
    [HideInInspector] public Node currentStartNode;
    [HideInInspector] public Node currentEndNode;
    [HideInInspector] public Node currentNode;                                      //where Npc is now
    [HideInInspector] public int daysActive;                                        //tally of days spent in City
    [HideInInspector] public List<int> listOfInvisibleNodes = new List<int>();        //nodes where Player's position is known and NPC goes into invisible mode and can't be intercepted in that location
    #endregion


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(sprite != null, "Invalid sprite (Null) for {0}", name);
        Debug.AssertFormat(nodeStart != null, "Invalid nodeStart (Null) for {0}", name);
        Debug.AssertFormat(nodeEnd != null, "Invalid nodeEnd (Null) for {0}", name);
        Debug.AssertFormat(action != null, "Invalid action (Null) for {0}", name);
    }

    /// <summary>
    /// Add node where player's position was known immediately
    /// </summary>
    /// <param name="nodeID"></param>
    public void AddInvisibleNode(int nodeID)
    {
        //only add if Invisible Mode active
        if (isIgnoreInvisible == false)
        {
            if (nodeID > -1 && nodeID < GameManager.i.nodeScript.nodeIDCounter)
            {
                //check not already in list
                if (listOfInvisibleNodes.Exists(x => x == nodeID) == false)
                {
                    listOfInvisibleNodes.Add(nodeID);
                    /*Debug.LogFormat("[Npc] Npc.SO -> AddInvisibleNode: nodeID {0} added to listOfInvisibleNodes{1}", nodeID, "\n");*/
                }
            }
            else { Debug.LogWarningFormat("Npc {0}, nodeID {1} NOT added to listOfInvisibleNodes (nodeIDCounter {2}){3}", tag, currentNode.nodeID, GameManager.i.nodeScript.nodeIDCounter, "\n"); }
        }
    }

    /// <summary>
    /// returns true if Npc's currentNode.nodeID is in listofInvisibleNodes and Npc is assumed to be in Invisibile mode and can't be interdicted or spotted
    /// NOTE: if isIgnoreInvisibleMode is true then the listOfInvisibleNodes will be Empty and method will return false
    /// </summary>
    /// <returns></returns>
    public bool CheckIfInvisibleMode()
    { return listOfInvisibleNodes.Exists(x => x == currentNode.nodeID); }

    /// <summary>
    /// returns true if specified node is in listofInvisibleNodes and Npc is assumed to be in Invisible mode and can't be interdicted or spotted
    /// NOTE: if isIgnoreInvisibleMode is true then the listOfInvisibleNodes will be Empty and method will return false
    /// </summary>
    /// <returns></returns>
    public bool CheckIfInvisibleMode(int nodeID)
    { return listOfInvisibleNodes.Exists(x => x == nodeID); }

    /// <summary>
    /// Reset at start of each level including first
    /// </summary>
    public void Reset()
    {
        listOfInvisibleNodes.Clear();
    }

}

