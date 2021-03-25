using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sub SO for tutorial Items that provides data for a Query Item
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialQuery")]
public class TutorialQuery : ScriptableObject
{
    [Header("Parent")]
    public TutorialItem item;

    [Header("Details")]
    [Tooltip("Type of query, eg. what is the query about? (SO enum that is required by EffectManager.cs to correctly handle a chosen template topic Option")]
    public TutorialQueryType queryType;
    [Tooltip("Map to Topic header and text")]
    public string queryHeader;
    [TextArea(2, 3)] public string queryText;
    [Tooltip("If false then standard option restrictions apply (min 2 max 4). If true then any number allowed and the max number (if present) are chosen randomly from the list to provide replayability")]
    public bool isRandomOptions;

    [Header("Default Options")]
    [Tooltip("Maximum of 4 topic Options, mininum of 2, sourced from Topics / TopicOptions / OptionsTutorial. Compulsory")]
    public List<TutorialOption> listOfOptions;
    [Tooltip("Option/s that apply if the player selects 'Ignore', only the first one is used. Compulsory")]
    public List<TutorialOption> listOfIgnoreOptions;

    [Header("Alternative Options")]
    [Tooltip("Alternative listOfOptions, eg. male/female names. Used only if there is code in place, based off queryType, ignore otherwise")]
    public List<TutorialOption> listOfOptionsAlt;
    [Tooltip("Alternative option, used only if there is code in place, based off queryType, ignore otherwise. Only first option is used")]
    public List<TutorialOption> listOfIgnoreOptionsAlt;




    public void OnEnable()
    {
        Debug.AssertFormat(item != null, "Invalid item (Null) for {0}", name);
        Debug.AssertFormat(queryType != null, "Invalid queryType (Null) for {0}", name);
        Debug.AssertFormat(queryHeader != null, "Invalid queryHeader (Null) for {0}", name);
        Debug.AssertFormat(queryText != null, "Invalid queryText (Null) for {0}", name);
        Debug.AssertFormat(listOfOptions.Count > 0, "Invalid listOfOptions (Empty) for {0}", name);
        Debug.AssertFormat(listOfOptions.Count > 0, "Invalid listOfIgnoreOptions (Empty) for {0}", name);
        //Alt options -> optional, but if present then there must be an alt IgnoreOption
        if (listOfOptionsAlt.Count > 0)
        { Debug.AssertFormat(listOfIgnoreOptionsAlt.Count > 0, "Invalid listOfIgnoreOptionsAlt (Empty) for {0}", name); }
    }
}
