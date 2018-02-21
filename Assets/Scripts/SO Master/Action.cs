using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Actor Actions carried out at Nodes
/// </summary>
[CreateAssetMenu(menuName = "Action / Action")]
public class Action : ScriptableObject
{
    public int ActionID { get; set; }

    //NOTE -> name of Scriptable Object is node action button title

    public string tooltipText;                  //descriptive text
    [Tooltip("For info purposes only to aid placing correct action with correct ActorArc")]
    public ActorArc intendedActor;
    [Tooltip("Effects of Action")]
    public List<Effect> listOfEffects; 
    [Tooltip("Applies to Resistance actions only, use default 'none' if none of these apply")]
    public ActionSpecial special;               //special resistance (node) action (eg. "GetGear/GetRecruit/NeutraliseTeam"), null if none

    [HideInInspector] public int coolDown;         //if '0' then ready to go, otherwise # of turns remaining

    /// <summary>
    /// get list of effects
    /// </summary>
    /// <returns></returns>
    public List<Effect> GetEffects()
    { return listOfEffects; }
}
