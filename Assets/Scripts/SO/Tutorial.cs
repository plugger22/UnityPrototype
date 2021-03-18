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

    [Tooltip("sprite of character who is conducting tutorial")]
    public Sprite sprite;

    [Tooltip("List of TutorialSets that make up the tutorial in order of use")]
    public List<TutorialSet> listOfSets;

    [Tooltip("Template options for use with all TutorialItem Queries. Required to be FOUR")]
    public TopicOption[] arrayOfOptions = new TopicOption[4];


    public void OnEnable()
    {
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(scenario != null, "Invalid scenario (Null) for {0}", name);
    }

}
