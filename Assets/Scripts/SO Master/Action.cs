using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Actor Actions carried out at Nodes
/// </summary>
[CreateAssetMenu(menuName = "Action")]
public class Action : ScriptableObject
{
    private int actionID;

    public string description;
    public ActorArc intendedActor;              //for info purposes only to aid placing correct action with correct ActorArc

    public int CoolDown { get; set; }          //if '0' then ready to go, otherwise # of turns remaining

}
