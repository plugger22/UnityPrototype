using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class representing a player/actors personality expressed as 5 factors (-2 to +2) in the 'Five Factor Model' and 3 factors (-2 to +2) in the 'Dark Triad' model
/// Indexes correspond to DataManager.cs -> arrayOfFactors[Factor.SO] which provide descriptors for each factor
/// </summary>
public class Personality
{
    private int[] arrayOfFactors = new int[8];


    public int[] GetFactors()
    { return arrayOfFactors; }


    /// <summary>
    /// Set personality factor matrix to an input set with appropriate error and range checks to prevent dirty data
    /// </summary>
    /// <param name="arrayOfSetFactors"></param>
    public void SetFactors(int[] arrayOfSetFactors)
    {
        if (arrayOfSetFactors != null)
        {
            int factorValue;
            int length = arrayOfSetFactors.Length;
            if (length != arrayOfFactors.Length)
            { Debug.LogErrorFormat("Invalid arrayOfSetFactors (incorrect length \"{0}\")", length); }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    factorValue = arrayOfSetFactors[i];
                    //range check factor
                    if (factorValue < -2 || factorValue > 2)
                    {
                        Debug.LogWarningFormat("Invalid factorValue \"{0}\" in arrayOfSetFactors[{1}] (auto Clamped to range)", factorValue, i);
                        factorValue = Mathf.Clamp(factorValue, -2, 2);
                    }
                    //assign factor
                    arrayOfFactors[i] = factorValue;
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfSetFactors (Null)"); }
    }


    /// <summary>
    /// Checks this personality with another for compatibility and returns a value from -3 (extremely incompatible) to +3 (extremely compatible) based on a mathematical comparison of the first 5 factors
    /// NOTE: Currently the dark triad has an no effect, only the standard Five Factor Model ones are taken into account
    /// </summary>
    /// <param name="arrayOfCompareFactors"></param>
    /// <returns></returns>
    public int CheckCompatibility(int[] arrayOfCompareFactors)
    {
        int difference;
        int compatibility = 0;
        int tally = 0;
        //both arrays must be off the same length
        int length = arrayOfCompareFactors.Length;
        if (length != arrayOfFactors.Length)
        { Debug.LogErrorFormat("Invalid arrayOfCompareFactors (incorrect length \"{0}\")", length); }
        else
        {
            //tally up differences (
            for (int i = 0; i < 5; i++)
            {
                //difference is ABS value where the closer a personality factor is to each other, the more compatible they are and the further apart they are the more incompatible they are.
                difference = Mathf.Abs(arrayOfFactors[i] - arrayOfCompareFactors[i]);
                tally += difference;
            }
            //convert compatibility into one of 7 bands
            switch (tally)
            {
                case 0:
                case 1:
                case 2:
                    compatibility = 3; break;
                case 3:
                case 4:
                case 5:
                    compatibility = 2; break;
                case 6:
                case 7:
                case 8:
                    compatibility = 1; break;
                case 9:
                case 10:
                case 11:
                    compatibility = 0; break;
                case 12:
                case 13:
                case 14:
                    compatibility = -1; break;
                case 15:
                case 16:
                case 17:
                    compatibility = -2; break;
                case 18:
                case 19:
                case 20:
                    compatibility = -3; break;
                default:
                    Debug.LogWarningFormat("Invalid compatibility \"{0}\" (band, should be between 0 and 20) ", compatibility);
                    break;
            }
        }
        return compatibility;
    }

}
