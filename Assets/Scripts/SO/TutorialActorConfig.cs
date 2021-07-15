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
    [Tooltip("You need to specify FOUR actorArcs and levels which will constitute the OnMap lineUp. Optional. If left empty (or not exactly four) the default tutorial.SO specified lvl 1 lineUp will be used")]
    public List<TutorialActorType> listOfOnMapArcs;


    [Header("Reserves")]
    [Tooltip("You can specify UP TO four actorArcs and levels which will fill up the reserves. Optional. If none specified then the reserves will start empty")]
    public List<TutorialActorType> listOfReserveArcs;


}
