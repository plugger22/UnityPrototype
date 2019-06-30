using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Personality Profiles (fixed)
/// </summary>
[CreateAssetMenu(menuName = "Actor / Personality Profile")]
public class PersonProfile : ScriptableObject
{
    [Tooltip("In game name")]
    public string tag;
    [Tooltip("Exhibits signs of [a/an] personality, True if 'an' needed instead of 'a'")]
    public bool isAn;
    [Tooltip("In game descriptor")]
    [TextArea] public string descriptor;

    [Header("Primary Factors, eg '++'")]
    [Tooltip("Only specify if != Neutral (leave as default 0 value) according to Personality Profile")]
    [Range(-2, 2)] public int AgreePrimary = 0;
    [Tooltip("Only specify if != Neutral (leave as default 0 value) according to Personality Profile")]
    [Range(-2, 2)] public int ConscPrimary = 0;
    [Tooltip("Only specify if != Neutral (leave as default 0 value) according to Personality Profile")]
    [Range(-2, 2)] public int NeuroPrimary = 0;
    [Tooltip("Only specify if != Neutral (leave as default 0 value) according to Personality Profile")]
    [Range(-2, 2)] public int ExtraPrimary = 0;
    [Tooltip("Only specify if != Neutral (leave as default 0 value) according to Personality Profile")]
    [Range(-2, 2)] public int OpenPrimary = 0;

    [Header("Secondary Factors, eg. '(+)'")]
    [Tooltip("Only specify if positive value (put 1) or negative (-1)")]
    [Range(-1, 1)] public int AgreeSecondary = 0;
    [Tooltip("Only specify if positive value (put 1) or negative (-1)")]
    [Range(-1, 1)] public int ConscSecondary = 0;
    [Tooltip("Only specify if positive value (put 1) or negative (-1)")]
    [Range(-1, 1)] public int NeuroSecondary = 0;
    [Tooltip("Only specify if positive value (put 1) or negative (-1)")]
    [Range(-1, 1)] public int ExtraSecondary = 0;
    [Tooltip("Only specify if positive value (put 1) or negative (-1)")]
    [Range(-1, 1)] public int OpenSecondary = 0;


    [HideInInspector] public int alpha;                     //A + C + reversed N
    [HideInInspector] public int beta;                      //E + 0             

    private int[] arrayOfFactorsPrimary;                    //must be identical
    private int[] arrayOfFactorsSecondary;                  //must be positive (+1/+2) or negative (-1/-2)


    public void OnEnable()
    {
        //auto calc alpha and beta
        alpha = AgreePrimary + AgreeSecondary + ConscPrimary + ConscSecondary - NeuroPrimary - NeuroSecondary;
        beta = ExtraPrimary + ExtraSecondary + OpenPrimary + OpenSecondary;
        //error check
        Debug.Assert(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty)");
        Debug.Assert(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty)");
        //assemble arrayOfFactors
        arrayOfFactorsPrimary = new int[] { OpenPrimary, ConscPrimary, ExtraPrimary, AgreePrimary, NeuroPrimary };
        arrayOfFactorsSecondary = new int[] { OpenSecondary, ConscSecondary, ExtraSecondary, AgreeSecondary, NeuroSecondary };
    }


    public int[] GetArrayOfPrimaryFactors()
    { return arrayOfFactorsPrimary; }

    public int[] GetArrayOfSecondaryFactors()
    { return arrayOfFactorsSecondary; }

}
