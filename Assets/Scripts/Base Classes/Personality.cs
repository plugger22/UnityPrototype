using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class representing a player/actors personality expressed as 5 factors (-2 to +2) in the 'Five Factor Model' and 3 factors (-2 to +2) in the 'Dark Triad' model
/// Indexes correspond to DataManager.cs -> arrayOfFactors[Factor.SO] which provide descriptors for each factor
/// </summary>
public class Personality
{
    private int[] arrayOfFactors;
    private int compatibilityWithPlayer;
    private List<string> listOfDescriptors = new List<string>();
    private List<string> listOfProfiles = new List<string>();                   //stored as 'an Inert', or 'a Responsible'  [personality]

    /// <summary>
    /// constructor
    /// </summary>
    public Personality()
    {
        arrayOfFactors = new int[] { 0, 0, 0, 0, 0 };
        compatibilityWithPlayer = 0;
    }


    public int[] GetFactors()
    { return arrayOfFactors; }

    public int GetCompatibilityWithPlayer()
    { return compatibilityWithPlayer; }

    /// <summary>
    /// Set compatibility with player (checks if within acceptable range of -3 to +3 and auto clamps if not)
    /// </summary>
    /// <param name="compatibility"></param>
    public void SetCompatibilityWithPlayer(int compatibility)
    {
        if (compatibility < -3 || compatibility > 3)
        {
            Debug.LogWarningFormat("Invalid compatibility \"{0}\" (needs to be within -3 to +3). Auto clamped to range", compatibility);
            compatibility = Mathf.Clamp(compatibility, -3, 3);
        }
        compatibilityWithPlayer = compatibility;
    }


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
    /// Add descriptor to listOfDescriptors
    /// </summary>
    /// <param name="item"></param>
    public void AddDescriptor(string item)
    {
        if (string.IsNullOrEmpty(item) == false)
        { listOfDescriptors.Add(item); }
        else { Debug.LogError("Invalid descriptor (Null or Empty)"); }
    }

    public List<string> GetListOfDescriptors()
    { return listOfDescriptors; }

    /// <summary>
    /// clears and adds saved load game profiles
    /// </summary>
    /// <param name="listOfDescriptors"></param>
    public void SetDescriptors(List<string> listOfDescriptors)
    {
        if (listOfDescriptors != null)
        {
            this.listOfDescriptors.Clear();
            this.listOfDescriptors.AddRange(listOfDescriptors);
        }
        else { Debug.LogError("Invalid listOfDescriptors (Null)"); }
    }

    /// <summary>
    /// Add profile to listOfProfiles
    /// </summary>
    /// <param name="item"></param>
    public void AddProfile(string item)
    {
        if (string.IsNullOrEmpty(item) == false)
        { listOfProfiles.Add(item); }
        else { Debug.LogError("Invalid profile (Null or Empty)"); }
    }

    public List<string> GetListOfProfiles()
    { return listOfProfiles; }

    /// <summary>
    /// clears and adds saved load game profiles
    /// </summary>
    /// <param name="listOfProfiles"></param>
    public void SetProfiles(List<string> listOfProfiles)
    {
        if (listOfProfiles != null)
        {
            this.listOfProfiles.Clear();
            this.listOfProfiles.AddRange(listOfProfiles);
        }
        else { Debug.LogError("Invalid listOfProfiles (Null)"); }
    }

    /// <summary>
    /// A + C + reversed N
    /// </summary>
    /// <returns></returns>
    public int GetAlpha()
    { return arrayOfFactors[3] + arrayOfFactors[1] - arrayOfFactors[4]; }


    /// <summary>
    /// E + O
    /// </summary>
    /// <returns></returns>
    public int GetBeta()
    { return arrayOfFactors[0] + arrayOfFactors[2]; }

}
