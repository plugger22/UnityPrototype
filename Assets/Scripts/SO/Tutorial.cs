using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Master tutorial SO for a specific side
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / Tutorial")]
public class Tutorial : ScriptableObject
{

    public GlobalSide side;
    public Scenario scenario;
    public ActorPoolFinal pool;

    [Tooltip("sprite of character who is conducting tutorial")]
    public Sprite sprite;

    [Tooltip("List of TutorialSets that make up the tutorial in order of use")]
    public List<TutorialSet> listOfSets;

    [Tooltip("Template options for use with all TutorialItem Queries. Required to be FOUR")]
    public TopicOption[] arrayOfOptions = new TopicOption[4];

    [Tooltip("Template option for use with tutorialItem Queries where the IGNORE button is pressed. Required")]
    public TopicOption ignoreOption;

    [Tooltip("List of four ActorArc types that form the default actor line-up if a set doesn't specify an OnMap line-up. NOTE: Should be four, one for each OnMap slot")]
    public List<ActorArc> listOfDefaultArcs;


    public void OnEnable()
    {
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(scenario != null, "Invalid scenario (Null) for {0}", name);
        Debug.AssertFormat(pool != null, "Invalid actorPoolFinal (Null) for {0}", name);
        Debug.AssertFormat(listOfDefaultArcs.Count == 4, "Invalid listOfDefaultArcs (is {0} arcs, should be 4)", listOfDefaultArcs.Count);
    }

}
