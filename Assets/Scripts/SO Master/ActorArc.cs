using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Actor archetype, eg. 'Fixer'
/// </summary>
[CreateAssetMenu(menuName = "Actor / ActorArc")]
public class ActorArc : ScriptableObject
{
    public int ActorArcID { get; set; }               //unique #, zero based -> assigned automatically by DataManager.Initialise
    public GlobalSide side;
    public string description;
    public string actorName;

    [Tooltip("Face of actor -> NOTE: should be a list with a few variations, perhaps?")]
    public Sprite sprite;
    [Tooltip("One action for interacting with nodes")]
    public Action nodeAction;
    [Tooltip("Preferred team (applies to authority actors only")]
    public TeamArc preferredTeam;

    //Preferences
    [Tooltip("The type of Gear the actor prefers (you gain a renown transfer from them for giving them this type of gear)")]
    public GearType preferredGear;


    /// <summary>
    /// initialisation
    /// </summary>
    private void OnEnable()
    {
        Debug.Assert(sprite != null, "Invalid sprite (Null)");
        Debug.Assert(nodeAction != null, "Invalid nodeAction (Null)");
        Debug.Assert(side != null, "Invalid side (Null)");
    }

}
