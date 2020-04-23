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
    [Tooltip("Option text, self contained, short")]
    public string text;
    [Tooltip("Tooltip description")]
    public string descriptor;
    [Tooltip("Base cost level of option in renown prior to any adjustments")]
    public GlobalChance renownCost;   
    [Tooltip("HQ actor who handles this option")]
    public HqPosition hqPosition;  

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
    [HideInInspector] public bool isRecommended;
    [HideInInspector] public string dataName;                               //generic data point, eg. organisation/secret/investigation name
    [HideInInspector] public string dataTag;                                //generic data point, eg. organisation/secret/investigation tag (used in metaOption text to replace '*' symbol)

    public void Reset()
    {
        isRecommended = false;
        dataName = "";
        dataTag = "";
    }


    //
    //NOTE - - -> ValidationManager.cs -> ValidateMetaOptions handles error states for text / renownCost / hqPosition and effects (should be at least one)
    //
}
