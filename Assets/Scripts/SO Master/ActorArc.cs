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
    public Sprite baseSprite;                         //face of actor -> NOTE: should be a list with a few variations, perhaps?
    public Sprite actionSprite;                       //sprite used when carrying out an action
    public string actorName;

    public Side side;                              //to separate Resistance and authority actorArcs

    //public EventType actionEvent;                       //used to trigger a relevant event when interacting with nodes

    public Action nodeAction;                           //one action for interacting with nodes

    //node preferences (which nodes are liable to be active on the map for this Actor type)
    public List<NodeArc> listPrefPrimary = new List<NodeArc>();         //full chance of node being active
    public List<NodeArc> listPrefExclude = new List<NodeArc>();         //NO chance of node being active
}
