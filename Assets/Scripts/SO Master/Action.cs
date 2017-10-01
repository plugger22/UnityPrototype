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

    public string description;
    public ActorArc intendedActor;              //for info purposes only to aid placing correct action with correct ActorArc
    public List<ActionEffect> listOfEffects;    //effects of action

    public int CoolDown { get; set; }          //if '0' then ready to go, otherwise # of turns remaining

}
