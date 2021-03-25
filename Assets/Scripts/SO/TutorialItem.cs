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
    [Tooltip("tutorialDialogue.SO data specifies content, only needed if tutorialType.Dialogue")]
    public TutorialDialogue dialogue;

    [Header("Information")]
    [Tooltip("GameHelp.SO specifies data and layout")]
    public GameHelp gameHelp;

    [Header("Goal")]
    [Tooltip("Goal that you want the player to achieve")]
    public TutorialGoal goal;

    [Header("Query")]
    public TutorialQuery query;


    /*[HideInInspector] public bool isQueryDone;*/

    public void OnEnable()
    {
        Debug.AssertFormat(tutorialType != null, "Invalid tutorialType (Null) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        //type specific asserts
        switch (tutorialType.name)
        {
            case "Dialogue":
                Debug.AssertFormat(dialogue != null, "Invalid dialogue (Null) for {0}", name);
                break;
            case "Information":
                Debug.AssertFormat(gameHelp != null, "Invalid gameHelp (Null) for {0}", name);
                break;
            case "Goal":
                Debug.AssertFormat(goal != null, "Invalid goal (Null) for {0}", name);
                break;
            case "Query":
                Debug.AssertFormat(query != null, "Invalid query (Null) for {0}", name);
                break;
            default: Debug.LogWarningFormat("Unrecognised tutorialType \"{0}\" for {1}", tutorialType.name, name); break;

        }
    }

}
