using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Trait SO, can be used for Actors, Mayors, Factions
/// </summary>
[CreateAssetMenu(menuName = "Trait / Trait")]
public class Trait : ScriptableObject
{
    [Header("General")]
    [Tooltip("A one/two word trait tag that is used for the UI (NOT trait.name)")]
    public string tag;
    [Tooltip("Used as a description in the trait tooltip")]
    public string description;
    [Tooltip("Is the trait good, neutral or bad from Point of View of Resistance (value assigned dynamically  2 / 1 / 0)")]
    public GlobalType typeOfTrait;
    [Tooltip("Only add a side if the trait is specific to a particular side, otherwise leave as 'None'")]
    public GlobalSide side;
    [Tooltip("What type of trait is it? Any SO with this trait will be error checked on import to confirm that it has the correct trait category")]
    public TraitCategory category;

    [Header("HQ Traits")]
    [Tooltip("Multiplier to Power gain/loss while at HQ (default 0). NOTE: can only be one HQ trait option")]
    public int hqPowerMultiplier = 0;
    [Tooltip("Multiplier to chance of a Major event (random/leave HQ) occuring, default 0). NOTE: can only be one HQ trait option")]
    public float hqMajorMultiplier = 0f;
    [Tooltip("Multiplier to chance of a Minor Good event (success good/ fail bad event) occuring, default 0). NOTE: can only be one HQ trait option")]
    public float hqMinorMultiplier = 0f;

    [Header("Effects")]
    [Tooltip("Specific in-game effects of the trait")]
    public List<TraitEffect> listOfTraitEffects;

    [Header("Personality")]
    [Tooltip("If specified then personality factor will be this value. Leave as '99' for default where value is ignored (random value used). Range -2 to +2 otherwise")]
    public int openness = 99;
    [Tooltip("If specified then personality factor will be this value. Leave as '99' for default where value is ignored (random value used). Range -2 to +2 otherwise")]
    public int conscientiousness = 99;
    [Tooltip("If specified then personality factor will be this value. Leave as '99' for default where value is ignored (random value used). Range -2 to +2 otherwise")]
    public int extraversion = 99;
    [Tooltip("If specified then personality factor will be this value. Leave as '99' for default where value is ignored (random value used). Range -2 to +2 otherwise")]
    public int agreeableness = 99;
    [Tooltip("If specified then personality factor will be this value. Leave as '99' for default where value is ignored (random value used). Range -2 to +2 otherwise")]
    public int neuroticism = 99;

    [HideInInspector] public string tagFormatted;                       //pre-formatted (TextMeshPro) string ready for trait display (initialised on import)

    //HQ traits
    [HideInInspector] public bool isHqTrait;                            //true if trait has an hq Effect (auto assigned by LoadManager.cs)
    [HideInInspector] public string hqDescription;                      //valid if trait has an hq Effect (auto assigned by LoadManager.cs)

    private int[] arrayOfCriteria;                                      //personality criteria (allows PersonalityManager.cs -> SetPersonalityFactors to be conform to a particular trait profile, if required)


    public void OnEnable()
    {
        //set up arrayOfCriteria
        arrayOfCriteria = new int[] { openness, conscientiousness, extraversion, agreeableness, neuroticism };
        //validate data in array
        for (int i = 0; i < arrayOfCriteria.Length; i++)
        {
            if (arrayOfCriteria[i] != 99)
            {
                if (arrayOfCriteria[i] < -2 || arrayOfCriteria[i] > 2)
                {
                    Debug.LogWarningFormat("Invalid arrayOfCriteria[{0}] for \"{1}\" trait (auto clamped to correct range)", i, tag);
                    arrayOfCriteria[i] = Mathf.Clamp(arrayOfCriteria[i], -2, 2);
                }
            }
        }
        //data validation
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(description) == false, "Invalid description (Null or Empty) for {0}", name);
        Debug.AssertFormat(typeOfTrait != null, "Invalid typeOfTrait (Null) for {0}", name);
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(category != null, "Invalid category (Null) for {0}", name);
        Debug.AssertFormat(listOfTraitEffects.Count > 0, "Invalid listOfTraitEffects (Empty) for {0}", name);
    }


    /// <summary>
    /// Get arrayOfCriteria (should never be null)
    /// </summary>
    /// <returns></returns>
    public int[] GetArrayOfCriteria()
    { return arrayOfCriteria; }

}
