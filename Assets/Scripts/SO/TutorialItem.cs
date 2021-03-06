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
    public string testQuestion;


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
                //To do
                break;
            default: Debug.LogWarningFormat("Unrecognised tutorialType \"{0}\" for {1}", tutorialType.name, name); break;

        }
    }

}
