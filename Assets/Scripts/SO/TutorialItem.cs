using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Swiss Army knife SO that combines multiple types (defined by TutorialType.SO) of tutorial items into one, 
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialItem")]
public class TutorialItem : ScriptableObject
{
    [Header("Global")]
    [Tooltip("Type of tutorialItem (determines which fields will be accessed, the rest will be ignored")]
    public TutorialType tutorialType;
    [Tooltip("Text that appears on tutorial side bar object tooltip (two word limit)")]
    public string tag;
    [Tooltip("Parent TutorialSet for this item, for validation purposes only")]
    public TutorialSet set;

    [Header("Dialogue")]
    [Tooltip("Top text in dialogue (larger size")]
    [TextArea] public string topText;
    [Tooltip("Bottom text")]
    [TextArea] public string bottomText;


    [Header("Information")]
    [Tooltip("GameHelp.SO specifies data and layout")]
    public GameHelp gameHelp;

    [Header("Goal")]
    [Tooltip("Goal that you want the player to achieve")]
    public TutorialGoal goal;

    [Header("Question")]
    [Tooltip("Type of query, eg. what is the query about? (SO enum that is required by EffectManager.cs to correctly handle a chosen template topic Option")]
    public TutorialQueryType queryType;
    [Tooltip("Map to Topic header and text")]
    public string queryHeader;
    [TextArea(2, 3)] public string queryText;
    [Tooltip("If false then standard option restrictions apply (min 2 max 4). If true then any number allowed and the max number (if present) are chosen randomly from the list to provide replayability")]
    public bool isRandomOptions;
    [Tooltip("Maximum of 4 topic Options, mininum of 2, sourced from Topics / TopicOptions / OptionsTutorial. Compulsory")]
    public List<TutorialOption> listOfOptions;
    [Tooltip("Option/s that apply if the player selects 'Ignore', only the first one is used. Compulsory")]
    public List<TutorialOption> listOfIgnoreOptions;
    [Tooltip("Alternative listOfOptions, eg. male/female names. Used only if there is code in place, based off queryType, ignore otherwise")]
    public List<TutorialOption> listOfOptionsAlt;
    [Tooltip("Alternative option, used only if there is code in place, based off queryType, ignore otherwise. Only first option is used")]
    public List<TutorialOption> listOfIgnoreOptionsAlt;


    /*[HideInInspector] public bool isQueryDone;*/

    public void OnEnable()
    {
        Debug.AssertFormat(tutorialType != null, "Invalid tutorialType (Null) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        //type specific asserts
        switch (tutorialType.name)
        {
            case "Dialogue":
                Debug.AssertFormat(topText != null, "Invalid topText (Null) for {0}", name);
                Debug.AssertFormat(bottomText != null, "Invalid bottomText (Null) for {0}", name);
                break;
            case "Information":
                Debug.AssertFormat(gameHelp != null, "Invalid gameHelp (Null) for {0}", name);
                break;
            case "Goal":
                Debug.AssertFormat(goal != null, "Invalid goal (Null) for {0}", name);
                break;
            case "Question":
                Debug.AssertFormat(queryType != null, "Invalid queryType (Null) for {0}", name);
                Debug.AssertFormat(queryHeader != null, "Invalid queryHeader (Null) for {0}", name);
                Debug.AssertFormat(queryText != null, "Invalid queryText (Null) for {0}", name);
                Debug.AssertFormat(listOfOptions.Count > 0, "Invalid listOfOptions (Empty) for {0}", name);
                Debug.AssertFormat(listOfOptions.Count > 0, "Invalid listOfIgnoreOptions (Empty) for {0}", name);
                //Alt options -> optional, but if present then there must be an alt IgnoreOption
                if (listOfOptionsAlt.Count > 0)
                { Debug.AssertFormat(listOfIgnoreOptionsAlt.Count > 0, "Invalid listOfIgnoreOptionsAlt (Empty) for {0}", name); }
                break;
            default: Debug.LogWarningFormat("Unrecognised tutorialType \"{0}\" for {1}", tutorialType.name, name); break;

        }
    }

}
