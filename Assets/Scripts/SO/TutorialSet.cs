using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tutorial's consist of multiple tutorialSets
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialSet")]
public class TutorialSet : ScriptableObject
{
    [Header("Main")]
    [Tooltip("provide a reference to which tutorial this set is associated with to enable validation")]
    public Tutorial tutorial;
    [Tooltip("Single word descriptor for tutorial set (shown at top of tutorial items under '4 of 7' progress tracker)")]
    public string descriptor;
    [Tooltip("Developer notes only, not used in-game")]
    [TextArea]public string notes;
    [Tooltip("Where the set sits within the tutorial sequence (zero based)")]
    [Range(0, 10)] public int index;

    [Header("Configurations")]
    [Tooltip("Specifies the Player state at the start of the set. Optional")]
    public TutorialPlayerConfig playerConfig;
    [Tooltip("Specifies the actor line up, if needed. Optional")]
    public TutorialActorConfig actorConfig;
    [Tooltip("Specifies targets for the set (Live). Optional")]
    public TutorialTargetConfig targetConfig;
    [Tooltip("Specifies teams OnMap at start of Set. Optional")]
    public TutorialTeamConfig teamConfig;
    [Tooltip("Specifies spiders and tracers OnMap at start of Set. Optional")]
    public TutorialHideConfig hideConfig;

    [Header("Features OFF")]
    [Tooltip("List of features switched OFF for this tutorial set")]
    public List<TutorialFeature> listOfFeaturesOff;

    [Header("GUI Elements OFF")]
    [Tooltip("List of GUI elements switched OFF for this tutorial set")]
    public List<TutorialGUIFeature> listOfGUIOff;

    [Header("Items")]
    [Tooltip("List of Tutorial items for this set. ORDER MATTERS. Maximum of 10 items")]
    public List<TutorialItem> listOfTutorialItems;

    [Header("Help Messages")]
    [Tooltip("Help Messages are only shown if there are some present in the list. Ignored otherwise")]
    public List<HelpMessage> listOfHelpMessages;


    public void OnEnable()
    {
        Debug.AssertFormat(tutorial != null, "Invalid tutorial (Null) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
    }
}
