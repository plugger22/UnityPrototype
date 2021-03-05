using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test personalities for Player used for development
/// </summary>
[CreateAssetMenu(menuName = "Actor / Test Personality")]
public class TestPersonality : ScriptableObject
{
    [Tooltip("In game name")]
    public string tag;

    [Header("Personality")]
    [Tooltip("If specified then personality factor will be this value. Leave as '0' for default neutral. Range -2 to +2 otherwise")]
    public int openness = 0;
    [Tooltip("If specified then personality factor will be this value. Leave as '99' for default neutral. Range -2 to +2 otherwise")]
    public int conscientiousness = 0;
    [Tooltip("If specified then personality factor will be this value. Leave as '99' for default neutral. Range -2 to +2 otherwise")]
    public int extraversion = 0;
    [Tooltip("If specified then personality factor will be this value. Leave as '99' for default neutral. Range -2 to +2 otherwise")]
    public int agreeableness = 0;
    [Tooltip("If specified then personality factor will be this value. Leave as '99' for default neutral. Range -2 to +2 otherwise")]
    public int neuroticism = 0;

    private int[] arrayOfFactors;

    /// <summary>
    /// OnEnable
    /// </summary>
    public void OnEnable()
    {
        //set up arrayOfCriteria
        arrayOfFactors = new int[] { openness, conscientiousness, extraversion, agreeableness, neuroticism };
        //validate data in array
        for (int i = 0; i < arrayOfFactors.Length; i++)
        {
            if (arrayOfFactors[i] < -2 || arrayOfFactors[i] > 2)
            {
                Debug.LogWarningFormat("Invalid arrayOfFactors[{0}] for \"{1}\" test Personality (auto clamped to correct range)", i, tag);
                arrayOfFactors[i] = Mathf.Clamp(arrayOfFactors[i], -2, 2);
            }
        }
        //data validation
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
    }


    public int[] GetFactors()
    { return arrayOfFactors; }

}
