using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all personality related matters
/// </summary>
public class PersonalityManager : MonoBehaviour
{

    [Tooltip("Number of personality factors present (combined total of Five Factor Model and Dark Triad factors)")]
    [Range(8, 8)] public int numOfFactors = 8;
    [Tooltip("Number of Five Factor Model personality factors present")]
    [Range(5, 5)] public int numOfFiveFactorModel = 5;
    [Tooltip("Number of Dark Triad personality factors present")]
    [Range(3, 3)] public int numOfDarkTriad = 3;

    /// <summary>
    /// Set an array of personality factors. Any criteria input will be taken into account. Criteria can be -2 to +2 and acts as a DM. If no criteria ignore.
    /// </summary>
    /// <param name="arrayOfCriteria"></param>
    /// <returns></returns>
    public int[] SetPersonality(int[] arrayOfCriteria = null)
    {
        int rndNum, modifier, factorValue;
        int[] arrayOfFactors = new int[numOfFactors];
        //handle default of no criteria, convert to an array of neutral values
        if (arrayOfCriteria == null)
        { arrayOfCriteria = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };}
        //do all factors TO DO -> modify rolls for dark triad traits based on five factor model results
        for (int i = 0; i < numOfFactors; i++)
        {
            //assign a neutral value
            factorValue = 0; 
            rndNum = Random.Range(0, 5);
            modifier = arrayOfCriteria[i];
            if (modifier != 0)
            {
                //clamp modifier to a range of -2 to +2
                modifier = Mathf.Clamp(modifier, -2, 2);
            }
            rndNum += modifier;
            rndNum = Mathf.Clamp(rndNum, 0, 4);
            switch(rndNum)
            {
                case 0: factorValue = -2; break;
                case 1: factorValue = -1; break;
                case 2: factorValue = 0; break;
                case 3: factorValue = 1; break;
                case 4: factorValue = 2; break;
                default: Debug.LogWarningFormat("Unrecognised rndNum \"{0}\", default Zero value assigned", rndNum); break;
            }
            arrayOfFactors[i] = factorValue;
        }
        return arrayOfFactors;
    }
	
}
