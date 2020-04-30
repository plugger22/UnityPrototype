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
    [Tooltip("Option text, self contained, short")]
    public string text;
    [Tooltip("Header text in MetaGameUI RHS. Keep short, max 3 words")]
    public string header;
    [Tooltip("Tooltip description")]
    public string descriptor;
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
    [Tooltip("If true, option is always displayed, regardless of valid criteria, or not")]
    public bool isAlways;

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
