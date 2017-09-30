using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Actor archetype, eg. 'Fixer'
/// </summary>
[CreateAssetMenu(menuName = "Actor / Archetype")]
public class ActorArc : ScriptableObject
{
    public string actorTag { get; set; }              //4 letter tag, UPPERCASE eg. 'FIXE', 'HACK' -> assigned automatically by DataManager.Initialise
    public int actorArcID { get; set; }               //unique #, zero based -> assigned automatically by DataManager.Initialise
    public Sprite sprite;                             //face of actor -> NOTE: should be a list with a few variations, perhaps?
    public string actorName;

    public List <Action> listOfActions = new List<Action>();            //list of actions able to be carried out by actor at any node except excluded

    //node preferences (which nodes are liable to be active on the map for this Actor type)
    public List<NodeArc> listPrefPrimary = new List<NodeArc>();         //full chance of node being active
    public List<NodeArc> listPrefExclude = new List<NodeArc>();         //NO chance of node being active
}
