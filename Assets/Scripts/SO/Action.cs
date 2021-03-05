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
    /*public int ActionID { get; set; }*/

    //NOTE -> name of Scriptable Object is node action button title
    [Tooltip("Tag referred to (name with spaces)")]
    public string tag;
    [Tooltip("In game tooltip descriptor")]
    public string tooltipText;                  //descriptive text
    [Tooltip("For info purposes only to aid placing correct action with correct ActorArc")]
    public ActorArc intendedActor;

    [Header("Effects")]
    [Tooltip("Effects of Action")]
    public List<Effect> listOfEffects; 

    [Header("Resistance Special Actions")]
    [Tooltip("Applies to Resistance actions only, use default 'none' if none of these apply")]
    public ActionSpecial special;               //special resistance (node) action (eg. "GetGear/GetRecruit/NeutraliseTeam"), null if none

    [HideInInspector] public int coolDown;         //if '0' then ready to go, otherwise # of turns remaining


    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(tooltipText) == false, "Invalid tooltipText for {0}", name);
        
    }


    /// <summary>
    /// get list of effects
    /// </summary>
    /// <returns></returns>
    public List<Effect> GetEffects()
    { return listOfEffects; }
}
