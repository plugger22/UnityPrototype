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


    public void OnEnable()
    {
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(scenario != null, "Invalid scenario (Null) for {0}", name);
        Debug.AssertFormat(pool != null, "Invalid actorPoolFinal (Null) for {0}", name);
    }

}
