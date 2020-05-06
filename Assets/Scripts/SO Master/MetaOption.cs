using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Metagame options for Player. Name is the referenceID so make sure it's unique and has no spaces
/// </summary>
[CreateAssetMenu(menuName = "Game / MetaOption")]
public class MetaOption : ScriptableObject
{
    [Header("Core Data")]
    [Tooltip("Template that includes a '*' for dynamic text (eg. name of secret/organisation/investigation). Used to populate text field, ignore if dynamic text isn't required")]
    public string template;
    [Tooltip("Left hand side item text. Keep short and self contained")]
    public string text;
    [Tooltip("Header text in MetaGameUI RHS. Keep short, max 3 words")]
    public string header;
    [Tooltip("Right hand side item details. Multi-line O.K")]
    [TextArea] public string descriptor;
    [Tooltip("Base cost level of option in renown prior to any adjustments")]
    public GlobalChance renownCost;   
    [Tooltip("HQ actor who handles this option")]
    public HqPosition hqPosition;
    [Tooltip("Sprite used in RHS of metaGameUI")]
    public Sprite sprite;

    [Header("Recommendation")]
    [Tooltip("True if recommended (default choice). Actual recommended options will be selected based on recommendPriority order until no more renown remains")]
    public bool isRecommended;
    [Tooltip("Priority order of recommendation selections. Higher the priority, more likely it is to be selected over other recommendations")]
    public GlobalChance recommendPriority;

    [Header("Special Cases")]
    [Tooltip("If true this represent a Player condition, eg. Secret/Org/Investigation/Condition that is listed under the MetaGameUI top 'Status' tab, setting ignored if false")]
    public bool isPlayerStatus;
    [Tooltip("If true, option is always displayed, regardless of valid criteria, or not. NOTE: If false there must be Criteria unless it's a Special case")]
    public bool isAlways;
    [Tooltip("Must be present if isAlways is TRUE and CRITERIA present (ignore otherwise) as a self contained explanation as to why option can't be selected if isActive False due to failed criteria")]
    [TextArea] public string textInactive;

    [Header("Criteria")]
    [Tooltip("Any criteria needed for option to be valid. Can be ignored")]
    public List<Criteria> listOfCriteria;

    [Header("Effects")]
    [Tooltip("Effects of option, if selected. Should be at least one effect present")]
    public List<Effect> listOfEffects;

    #region Save Data compatible
    [HideInInspector] public int statTimesSelected;
    #endregion

    //data reset prior to use
    [HideInInspector] public bool isActive;                                 //set true or false depending on whether it passed criteria checks (if any)
    [HideInInspector] public string dataName;                               //generic data point, eg. organisation/secret/investigation name
    [HideInInspector] public string dataTag;                                //generic data point, eg. organisation/secret/investigation tag (used in metaOption text to replace '*' symbol)

    public void Reset()
    {
        isActive = false;
        dataName = "";
        dataTag = "";
    }


    //
    //NOTE - - -> ValidationManager.cs -> ValidateMetaOptions handles error states for text / renownCost / hqPosition and effects (should be at least one)
    //
}
