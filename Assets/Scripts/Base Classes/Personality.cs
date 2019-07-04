using packageAPI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base class representing a player/actors personality expressed as 5 factors (-2 to +2) in the 'Five Factor Model' and 3 factors (-2 to +2) in the 'Dark Triad' model
/// Indexes correspond to DataManager.cs -> arrayOfFactors[Factor.SO] which provide descriptors for each factor
/// </summary>
public class Personality
{
    private int[] arrayOfFactors;
    private int compatibilityWithPlayer;
    private string profile;                                             //dict key name of profile
    private string profileDescriptor;                                   //in-game descriptor, eg. 'Weak indication of a AntiSocial
    private string profileExplanation;                                  //psychological explanation of profile
    //collections
    private List<string> listOfDescriptors = new List<string>();                                //list of personality descriptors
    private List<HistoryMotivation> listOfMotivation = new List<HistoryMotivation>();           //list of all motivational changes

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


    public string GetProfile()
    { return profile; }

    public string GetProfileDescriptor()
    { return profileDescriptor; }

    public string GetProfileExplanation()
    { return profileExplanation; }

    /// <summary>
    /// sets profile
    /// </summary>
    /// <param name="listOfProfiles"></param>
    public void SetProfile(string profile)
    {
        if (string.IsNullOrEmpty(profile) == false)
        { this.profile = profile; }
        else { Debug.LogError("Invalid profile (Null)"); }
    }

    /// <summary>
    /// sets profile descriptor
    /// </summary>
    /// <param name="listOfProfiles"></param>
    public void SetProfileDescriptor(string descriptor)
    {
        if (string.IsNullOrEmpty(descriptor) == false)
        { profileDescriptor = descriptor; }
        else { Debug.LogError("Invalid profileDescriptor (Null)"); }
    }

    /// <summary>
    /// sets profile explanation
    /// </summary>
    /// <param name="listOfProfiles"></param>
    public void SetProfileExplanation(string explanation)
    {
        if (string.IsNullOrEmpty(explanation) == false)
        { profileExplanation = explanation; }
        else { Debug.LogError("Invalid profileExplanation (Null)"); }
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


    public List<HistoryMotivation> GetListOfMotivation()
    { return listOfMotivation; }


    /// <summary>
    /// returns list of pre-formatted strings, each being a item of motivational history, eg. 'Give HoloPorn gear +1' (coloured)
    /// </summary>
    /// <returns></returns>
    public List<string> GetMotivationDescriptors()
    {
        return listOfMotivation
            .Select(x => x.descriptor)
            .ToList();
    }

    /// <summary>
    /// Add a record to the listOfMotivation (History)
    /// </summary>
    /// <param name="history"></param>
    public void AddMotivation(HistoryMotivation history)
    {
        if (history != null)
        { listOfMotivation.Add(history); }
        else { Debug.LogError("Invalid history (Null)"); }
    }

    /// <summary>
    /// set listOfMotivation (History) from saved load game data. Clears any existing data beforehand.
    /// </summary>
    /// <param name="listOfMotivation"></param>
    public void SetMotivation(List<HistoryMotivation> listOfMotivation)
    {
        if (listOfMotivation != null)
        {
            this.listOfMotivation.Clear();
            this.listOfMotivation.AddRange(listOfMotivation);
        }
        else { Debug.LogError("Invalid listOfMotivation (Null)"); }
    }

}
