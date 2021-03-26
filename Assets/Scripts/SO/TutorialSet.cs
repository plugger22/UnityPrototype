using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tutorial's consist of multiple tutorialSets
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialSet")]
public class TutorialSet : ScriptableObject
{
    [Tooltip("provide a reference to which tutorial this set is associated with to enable validation")]
    public Tutorial tutorial;

    [Tooltip("Developer notes only, not used in-game")]
    [TextArea]public string notes;

    [Tooltip("Where the set sits within the tutorial sequence (zero based)")]
    [Range(0, 10)] public int index;

    [Header("Features OFF")]
    [Tooltip("List of features switched OFF for this tutorial set")]
    public List<TutorialFeature> listOfFeaturesOff;

    [Header("GUI Elements OFF")]
    [Tooltip("List of GUI elements switched OFF for this tutorial set")]
    public List<TutorialGUIFeature> listOfGUIOff;

    [Header("Items")]
    [Tooltip("List of Tutorial items for this set. ORDER MATTERS. Maximum of 10 items")]
    public List<TutorialItem> listOfTutorialItems;


    public void OnEnable()
    {
        Debug.AssertFormat(tutorial != null, "Invalid tutorial (Null) for {0}", name);
    }
}
