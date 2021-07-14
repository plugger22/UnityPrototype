using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows a specific actor configuration up at the start of a tutorial Set
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialActorConfig")]
public class TutorialActorConfig : ScriptableObject
{

    [Header("Main")]
    public GlobalSide side;
    [Tooltip("Design notes, not used in game")]
    [TextArea] public string descriptor;

    [Header("OnMap lineUp")]
    [Tooltip("Leave as 'none' if there is no need for a specific outcome, otherwise choose an actorArc and a level and a match will be drawn from the tutorial actorPool at the start of the TutorialSet")]
    public TutorialActorType map0;
    [Tooltip("Leave as 'none' if there is no need for a specific outcome, otherwise choose an actorArc and a level and a match will be drawn from the tutorial actorPool at the start of the TutorialSet")]
    public TutorialActorType map1;
    [Tooltip("Leave as 'none' if there is no need for a specific outcome, otherwise choose an actorArc and a level and a match will be drawn from the tutorial actorPool at the start of the TutorialSet")]
    public TutorialActorType map2;
    [Tooltip("Leave as 'none' if there is no need for a specific outcome, otherwise choose an actorArc and a level and a match will be drawn from the tutorial actorPool at the start of the TutorialSet")]
    public TutorialActorType map3;

    [Header("Reserves")]
    [Tooltip("Leave as 'none' if there is no need for a specific outcome, otherwise choose an actorArc and a level and a match will be drawn from the tutorial actorPool at the start of the TutorialSet")]
    public TutorialActorType reserve0;
    [Tooltip("Leave as 'none' if there is no need for a specific outcome, otherwise choose an actorArc and a level and a match will be drawn from the tutorial actorPool at the start of the TutorialSet")]
    public TutorialActorType reserve1;
    [Tooltip("Leave as 'none' if there is no need for a specific outcome, otherwise choose an actorArc and a level and a match will be drawn from the tutorial actorPool at the start of the TutorialSet")]
    public TutorialActorType reserve2;
    [Tooltip("Leave as 'none' if there is no need for a specific outcome, otherwise choose an actorArc and a level and a match will be drawn from the tutorial actorPool at the start of the TutorialSet")]
    public TutorialActorType reserve3;


}
