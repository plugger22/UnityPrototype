using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Actor Actions carried out at Nodes
/// </summary>
[CreateAssetMenu(menuName = "Action / Action")]
public class Action : ScriptableObject
{
    public int ActionID { get; set; }
                                                //NOTE -> name of Scriptable Object is button title

    public string tooltipText;                  //descriptive text
    public ActorArc intendedActor;              //for info purposes only to aid placing correct action with correct ActorArc
    public List<Effect> listOfEffects;    //effects of action

    public int CoolDown { get; set; }          //if '0' then ready to go, otherwise # of turns remaining

    /// <summary>
    /// get list of effects
    /// </summary>
    /// <returns></returns>
    public List<Effect> GetEffects()
    { return listOfEffects; }
}
