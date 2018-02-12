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

    //NOTE -> name of Scriptable Object is button title

    public SideEnum side;                           //which side the action applies to
    public string tooltipText;                  //descriptive text
    public ActorArc intendedActor;              //for info purposes only to aid placing correct action with correct ActorArc
    public List<Effect> listOfEffects;          //effects of action
    [Tooltip("Normally a 'Node' type of action but if a GenericModalPicker is needed then select an appropriate type. Can be left at 'None' for all Authority actions")]
    public ActionType type;                     //broad category of actions (used to identify special cases that require the ModalGenericPicker)

    public int CoolDown { get; set; }           //if '0' then ready to go, otherwise # of turns remaining

    /// <summary>
    /// get list of effects
    /// </summary>
    /// <returns></returns>
    public List<Effect> GetEffects()
    { return listOfEffects; }
}
