using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Actor archetype, eg. 'Fixer'
/// </summary>
[CreateAssetMenu(menuName = "ActorArc")]
public class ActorArc : ScriptableObject
{
    public string ActorTag { get; set; }              //4 letter tag, UPPERCASE eg. 'FIXE', 'HACK' -> assigned automatically by DataManager.Initialise
    public int ActorArcID { get; set; }               //unique #, zero based -> assigned automatically by DataManager.Initialise
    [Tooltip("Face of actor -> NOTE: should be a list with a few variations, perhaps?")]
    public Sprite baseSprite;
    [Tooltip("Sprite used when carrying out an action")]
    public Sprite actionSprite;
    public string description;
    public string actorName;
    [Tooltip("One action for interacting with nodes")]
    public Action nodeAction;

    
    [Tooltip("Preferred team (applies to authority actors only")]
    public TeamArc preferredTeam;
    public GlobalSide side;

    //Preferences
    [Tooltip("Node preferences (which nodes are liable to be active on the map for this Actor type) -> Full chance of node being active")]
    public List<NodeArc> listPrefPrimary = new List<NodeArc>();
    [Tooltip("Node preferences (which nodes are liable to be active on the map for this Actor type) -> NO chance of node being active")]
    public List<NodeArc> listPrefExclude = new List<NodeArc>();
    [Tooltip("The type of Gear the actor prefers (you gain a renown transfer from them for giving them this type of gear)")]
    public GearType preferredGear;



}
