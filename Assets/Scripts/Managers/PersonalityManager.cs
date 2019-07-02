using gameAPI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all personality related matters
/// </summary>
public class PersonalityManager : MonoBehaviour
{
    [Header("Factors")]
    [Tooltip("Number of personality factors present")]
    [Range(5, 5)] public int numOfFactors = 5;
    [Tooltip("Difference threshold less than or equal for a profile to make it into the temporary dictionary from which a partial match is chosen from (lowest difference)")]
    [Range(4, 4)] public int profileThreshold = 4;

    [Header("Compatibility Effect")]
    [Tooltip("% chance of actor letting a motivational change go by the boards due to their compatibility with the player assuming it's +/- 1")]
    [Range(1, 100)] public int compatibilityChanceOne = 20;
    [Tooltip("% chance of actor letting a motivational change go by the boards due to their compatibility with the player assuming it's +/- 2")]
    [Range(1, 100)] public int compatibilityChanceTwo = 40;
    [Tooltip("% chance of actor letting a motivational change go by the boards due to their compatibility with the player assuming it's +/- 3")]
    [Range(1, 100)] public int compatibilityChanceThree = 60;

    //Fast access
    private Factor[] arrayOfFactors;
    private string[] arrayOfFactorTags;
    private Personality playerPersonality;
    private GlobalSide globalResistance;
    private GlobalSide globalAuthority;

    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseLevelStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", state);
                break;
        }
    }

    #region Initialisation Sub Methods

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        arrayOfFactors = GameManager.instance.dataScript.GetArrayOfFactors();
        arrayOfFactorTags = GameManager.instance.dataScript.GetArrayOfFactorTags();
        globalResistance = GameManager.instance.globalScript.sideResistance;
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        playerPersonality = GameManager.instance.playerScript.GetPersonality();
        Debug.Assert(arrayOfFactors != null, "Invalid arrayOfFactors (Null)");
        Debug.Assert(arrayOfFactorTags != null, "Invalid arrayOfFactorTags (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(playerPersonality != null, "Invalid playerPersonality (Null)");
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        //set compatibility of all actors in level right at level start
        SetAllActorsPersonality();
    }
    #endregion

    #endregion

    /// <summary>
    /// Set an array of personality factors. Criteria input can be -2 to +2 and, if present, define the value ('99' ignored). If no criteria present ('99') then Randomise.
    /// </summary>
    /// <param name="arrayOfCriteria"></param>
    /// <returns></returns>
    public int[] SetPersonalityFactors(int[] arrayOfCriteria)
    {
        int rndNum, factorValue;
        int[] arrayOfFactors = new int[numOfFactors];
        if (arrayOfCriteria != null)
        {
            //do all factors
            for (int index = 0; index < numOfFactors; index++)
            {
                //valid criteria present
                if (arrayOfCriteria[index] != 99)
                { factorValue = arrayOfCriteria[index]; }
                //invalid criteria (ignore) -> use random number instead
                else
                {
                    rndNum = Random.Range(0, 5);
                    factorValue = 0;
                    switch (rndNum)
                    {
                        case 0: factorValue = -2; break;
                        case 1: factorValue = -1; break;
                        case 2: factorValue = 0; break;
                        case 3: factorValue = 1; break;
                        case 4: factorValue = 2; break;
                        default: Debug.LogWarningFormat("Unrecognised rndNum \"{0}\", default Zero value assigned", rndNum); break;
                    }
                }
                arrayOfFactors[index] = factorValue;
            }
        }
        else { Debug.LogError("Invalid arrayOfCriteria (Null)"); }
        return arrayOfFactors;
    }


    /// <summary>
    /// Checks listOfFactors for any that are +2/-2 and adds a descriptor to personality.listOfDescriptors
    /// NOTE: personality checked for Null by calling method
    /// </summary>
    /// <param name="personality"></param>
    public void SetDescriptors(Personality personality)
    {
        string descriptor;
        int[] arrayOfFactors = personality.GetFactors();
        if (arrayOfFactors != null)
        {
            for (int i = 0; i < arrayOfFactors.Length; i++)
            {
                descriptor = null;
                switch (arrayOfFactors[i])
                {
                    case 2:
                    case -2:
                        descriptor = GetDescriptor(i, arrayOfFactors[i]);
                        break;
                }
                if (string.IsNullOrEmpty(descriptor) == false)
                { personality.AddDescriptor(descriptor); }
            }
        }
        else { Debug.LogError("Invalid listOfFactors (Null)"); }
    }

    /// <summary>
    /// subMethod for PersonalityManager.cs -> SetDescriptors. Gets a random descriptor from the appropriate factor textlist which is then added to Personality.listOfDescriptors.
    /// Index if personality factor index (eg. 0 to 4) and value is value of the factor, eg. +2 or -2 (shouldn't be called otherwise)
    /// </summary>
    /// <param name="index"></param>
    /// <param name=""></param>
    private string GetDescriptor(int index, int value)
    {
        string descriptor = "Unknown";
        if (index > -1 && index < numOfFactors)
        {
            //get factor
            Factor factor = arrayOfFactors[index];
            if (factor != null)
            {
                //+2 or -2
                switch (value)
                {
                    case 2: descriptor = factor.positiveDescriptor.GetRandomRecord(); break;
                    case -2: descriptor = factor.negativeDescriptor.GetRandomRecord(); break;
                }
            }
            else { Debug.LogErrorFormat("Invalid factor (Null) for arrayOfFactors[{0}]", index); }
        }
        else { Debug.LogFormat("Invalid index \"{0}\" (should be within 0 to 4)", index); }
        return descriptor;
    }

    /// <summary>
    /// loop through entire suite of actors in a level (even those in reserve pools) and set their compatibility with the player and their factor descriptors 
    /// Note: factors are determined by ActorManager.cs -> CreateActor
    /// </summary>
    private void SetAllActorsPersonality()
    {
        Dictionary<int, Actor> dictOfActors = GameManager.instance.dataScript.GetDictOfActors();
        Dictionary<string, PersonProfile> dictOfProfiles = GameManager.instance.dataScript.GetDictOfProfiles();
        if (dictOfProfiles != null)
        {
            if (dictOfActors != null)
            {
                foreach (var actor in dictOfActors)
                {
                    if (actor.Value != null)
                    {
                        int compatibility;
                        Personality personality = actor.Value.GetPersonality();
                        if (personality != null)
                        {
                            //compatibility with Player
                            compatibility = CheckCompatibilityWithPlayer(personality.GetFactors());
                            personality.SetCompatibilityWithPlayer(compatibility);
                            //descriptors
                            SetDescriptors(personality);
                            //profile
                            CheckPersonalityProfile(dictOfProfiles, personality);
                        }
                        else { Debug.LogWarningFormat("Invalid personality (Null) for {0}, actorID {1}", actor.Value.actorName, actor.Value.actorID); }
                    }
                    else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actor.Key); }
                }
            }
            else { Debug.LogError("Invalid dictOfActors (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfProfiles (Null)"); }
    }

    /// <summary>
    /// Set individual personality (excluding factors, assumed to have already been done). Not performant. Use sparingly.
    /// </summary>
    /// <param name="personality"></param>
    public void SetIndividualPersonality(Personality personality)
    {
        int compatibility;
        if (personality != null)
        {
            //compatibility with Player
            compatibility = CheckCompatibilityWithPlayer(personality.GetFactors());
            personality.SetCompatibilityWithPlayer(compatibility);
            //descriptors
            SetDescriptors(personality);
            //profile
            CheckPersonalityProfile(GameManager.instance.dataScript.GetDictOfProfiles(), personality);
        }
        else { Debug.LogError("Invalid personality (Null)"); }
    }


    /// <summary>
    /// Checks Player personality with another for compatibility and returns a value from -3 (extremely incompatible) to +3 (extremely compatible) based on a mathematical comparison of the first 5 factors
    /// NOTE: Currently the dark triad has an no effect, only the standard Five Factor Model ones are taken into account
    /// </summary>
    /// <param name="arrayOfCompareFactors"></param>
    /// <returns></returns>
    public int CheckCompatibilityWithPlayer(int[] arrayOfCompareFactors)
    {
        int difference;
        int compatibility = 0;
        int tally = 0;
        int[] arrayOfFactors = playerPersonality.GetFactors();
        if (arrayOfFactors != null)
        {
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
                        compatibility = 3; break;
                    case 2:
                    case 3:
                        compatibility = 2; break;
                    case 4:
                    case 5:
                    case 6:
                        compatibility = 1; break;
                    case 7:
                    case 8:
                        compatibility = 0; break;
                    case 9:
                    case 10:
                    case 11:
                        compatibility = -1; break;
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        compatibility = -2; break;
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                        compatibility = -3; break;
                    default:
                        Debug.LogWarningFormat("Invalid compatibility \"{0}\" (band, should be between 0 and 20) ", compatibility);
                        break;
                }
            }
        }
        else { Debug.LogError("Invalid playerPersonality.arrayOfFactors (Null)"); }
        return compatibility;
    }

    /// <summary>
    /// Searches dictOfProfiles for a match
    /// </summary>
    /// <param name="personality"></param>
    public void CheckPersonalityProfile(Dictionary<string, PersonProfile> dictOfProfiles, Personality personality)
    {
        if (personality != null)
        {
            if (dictOfProfiles != null)
            {
                bool isProceed;
                bool hasProfile = false;
                int difference = 0;
                int[] arrayOfProfilePrimary;
                int[] arrayOfProfileSecondary;
                int[] arrayOfFactors = personality.GetFactors();
                Dictionary<string, int> dictOfDifferences = new Dictionary<string, int>();
                //calculate alpha & beta of personality
                int alpha = personality.GetAlpha();
                int beta = personality.GetBeta();


                //loop dictionary trying to find a profile match
                foreach (var profile in dictOfProfiles)
                {
                    difference = 0;
                    isProceed = true;
                    arrayOfProfilePrimary = profile.Value.GetArrayOfPrimaryFactors();
                    arrayOfProfileSecondary = profile.Value.GetArrayOfSecondaryFactors();
                    if (arrayOfProfilePrimary != null)
                    {
                        if (arrayOfProfileSecondary != null)
                        {
                            //attempt to match primary criteria
                            for (int i = 0; i < arrayOfProfilePrimary.Length; i++)
                            {
                                //primary Zero
                                if (arrayOfProfilePrimary[i] == 0)
                                {
                                    //no primary, secondary requirement exists
                                    if (arrayOfProfileSecondary[i] != 0)
                                    {
                                        //should be a positive value if criteria > 0, negative if < 0
                                        if (arrayOfProfileSecondary[i] > 0)
                                        {
                                            if (arrayOfFactors[i] < 1)
                                            { isProceed = false; difference += Mathf.Abs(arrayOfFactors[i]); }
                                        }
                                        else if (arrayOfFactors[i] > -1)
                                        { isProceed = false; difference += Mathf.Abs(arrayOfFactors[i]); }
                                    }
                                    else
                                    {
                                        //No secondary, primary requirement to equal Zero
                                        if (arrayOfFactors[i] != 0)
                                        { isProceed = false; difference += Mathf.Abs(arrayOfFactors[i]); }
                                    }
                                }
                                else
                                {
                                    //non zero primary, must be a match, calculate difference otherwise
                                    if (arrayOfProfilePrimary[i] != arrayOfFactors[i])
                                    { isProceed = false; difference += Mathf.Abs(arrayOfProfilePrimary[i] - arrayOfFactors[i]); }
                                }
                            }
                            //match found
                            if (isProceed == true)
                            {
                                /*Debug.LogFormat("[Tst] PersonalityManager.cs -> CheckPersonalityProfile: match for \"{0}\"", profile.Value.name);*/
                                personality.SetProfile(profile.Value.name);
                                personality.SetProfileDescriptor(string.Format("Has a strongly defined {0}", profile.Value.tag));
                                personality.SetProfileExplanation(profile.Value.descriptor);
                                hasProfile = true;
                            }
                            //is there a match for alpha or beta?
                            else if (alpha == profile.Value.alpha || beta == profile.Value.beta)
                            {
                                //if has either an alpha or beta match and difference is less than or equal to the threshold then place in dictionary for sorting and possible selection at end of loop
                                if (difference <= profileThreshold)
                                {
                                    dictOfDifferences.Add(profile.Value.name, difference);
                                    /*Debug.LogFormat("[Tst] PersonalityManager.cs -> CheckPersonalityProfile: add to dictOfDifferences, \"{0}\", difference {1}{2}", profile.Value.name, difference, "\n");*/
                                }
                            }
                        }
                        else { Debug.LogWarningFormat("Invalid arrayOfSecondaryFactors (Null) for {0}", profile.Value.name); }
                    }
                    else { Debug.LogWarningFormat("Invalid arrayOfPrimaryFactors (Null) for {0}", profile.Value.name); }
                }

                //if records in dict then find nearest match
                if (hasProfile == false)
                {
                    if (dictOfDifferences.Count > 0)
                    {
                        string profileName = "Unknown";
                        string prefix = "Unknown";
                        //sort with the smallest difference at top
                        var sortedDict = from entry in dictOfDifferences orderby entry.Value ascending select entry;
                        //get top most record (smallest difference)
                        foreach (var record in sortedDict)
                        {
                            switch (record.Value)
                            {
                                case 0:
                                    prefix = "Definite indicators of";
                                    profileName = record.Key;
                                    break;
                                case 1:
                                    prefix = "Strong indicators of";
                                    profileName = record.Key;
                                    break;
                                case 2:
                                    prefix = "Moderate indicators of";
                                    profileName = record.Key;
                                    break;
                                case 3:
                                    prefix = "Weak indicators of";
                                    profileName = record.Key;
                                    break;
                                case 4:
                                    prefix = "Faint indicators of";
                                    profileName = record.Key;
                                    break;
                                default:
                                    Debug.LogWarningFormat("Unrecognised difference \"{0}\" for {1} profile", record.Value, record.Key);
                                    break;
                            }
                            //only want the first entry in sorted dict
                            break;
                        }
                        //Get profile and add to personality
                        PersonProfile profile = GameManager.instance.dataScript.GetProfile(profileName);
                        if (profile != null)
                        {
                            personality.SetProfile(profileName);
                            personality.SetProfileDescriptor(string.Format("{0} {1} {2}", prefix, profile.isAn == true ? "an" : "a", profile.tag));
                            personality.SetProfileExplanation(profile.descriptor);
                        }
                        else { Debug.LogWarningFormat("Invalid profile (Null) for \"{0}\"", profileName); }
                    }
                    /*else { Debug.LogFormat("[Tst] PersonalityManager.cs -> CheckPersonalityProfile: NO MATCH{0}", "\n"); }*/
                }
            }
            else { Debug.LogError("Invalid dictOfProfiles (Null)"); }
        }
        else { Debug.LogError("Invalid personality (Null)"); }
    }

    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Display personalities of Player and all OnMap and Reserve actors
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayAllPersonalities()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("-Personalities{0}{1}", "\n", "\n");
        //player
        builder.AppendFormat("-Player {0}{1}", GameManager.instance.playerScript.PlayerName, "\n");
        builder.Append(DebugDisplayIndividualPersonality(GameManager.instance.playerScript.GetPersonality()));
        builder.AppendLine();
        //OnMap actors
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    {
                        //display actor personality
                        builder.AppendFormat("-{0}, {1}, ID {2}, On Map{3}", actor.actorName, actor.arc.name, actor.actorID, "\n");
                        Trait trait = actor.GetTrait();
                        if (trait != null)
                        {
                            int[] arrayOfCriteria = trait.GetArrayOfCriteria();
                            if (arrayOfCriteria != null)
                            {
                                builder.AppendFormat(" \"{0}\" trait, criteria | {1}{2} | {3}{4} | {5}{6} | {7}{8} | {9}{10} | {11}", 
                                    trait.tag, 
                                    arrayOfCriteria[0] != 99 && arrayOfCriteria[0] > 0 ? "+" : "", arrayOfCriteria[0] != 99 ? arrayOfCriteria[0].ToString() : ".",
                                    arrayOfCriteria[1] != 99 && arrayOfCriteria[1] > 0 ? "+" : "", arrayOfCriteria[1] != 99 ? arrayOfCriteria[1].ToString() : ".", 
                                    arrayOfCriteria[2] != 99 && arrayOfCriteria[2] > 0 ? "+" : "", arrayOfCriteria[2] != 99 ? arrayOfCriteria[2].ToString() : ".",
                                    arrayOfCriteria[3] != 99 && arrayOfCriteria[3] > 0 ? "+" : "", arrayOfCriteria[3] != 99 ? arrayOfCriteria[3].ToString() : ".", 
                                    arrayOfCriteria[4] != 99 && arrayOfCriteria[4] > 0 ? "+" : "", arrayOfCriteria[4] != 99 ? arrayOfCriteria[4].ToString() : ".", 
                                    "\n");
                            }
                            else { Debug.LogErrorFormat("Invalid arrayOfCriteria (Null) for {0} trait", trait.tag); }
                        }
                        else
                        {
                            builder.AppendFormat(" ERROR: INVALID TRAIT (NULL){0}", "\n");
                            Debug.LogWarningFormat("Invalid trait (Null) for {0}, {1}, actorID {2}", actor.actorName, actor.arc.name, actor.actorID);
                        }
                        builder.Append(DebugDisplayIndividualPersonality(actor.GetPersonality()));
                        builder.AppendLine();
                    }
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        /*//Reserve actors
        List<int> listOfReservePool = GameManager.instance.dataScript.GetListOfReserveActors(GameManager.instance.sideScript.PlayerSide);
        if (listOfReservePool != null)
        {
            int count = listOfReservePool.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Actor actor = GameManager.instance.dataScript.GetActor(listOfReservePool[i]);
                    if (actor != null)
                    {
                        builder.AppendFormat("-{0}, {1}, ID {2}, RESERVE{3}", actor.actorName, actor.arc.name, actor.actorID, "\n");
                        builder.Append(DebugDisplayIndividualPersonality(actor.GetPersonality()));
                        builder.AppendLine();
                    }
                    else { Debug.LogWarningFormat("Invalid actor (Null) for listOfReservePool[{0}]", i); }
                }
            }
        }
        else { Debug.LogError("Invalid listOfReservePool (Null)"); }*/
        return builder.ToString();
    }

    /// <summary>
    /// Display a personality (player or actor)
    /// </summary>
    /// <param name="personality"></param>
    /// <returns></returns>
    public string DebugDisplayIndividualPersonality(Personality personality)
    {
        StringBuilder builder = new StringBuilder();
        int count;
        List<string> listOfDescriptors;
        if (personality != null)
        {
            int[] arrayOfFactors = personality.GetFactors();
            if (arrayOfFactors != null)
            {
                //factors
                for (int i = 0; i < arrayOfFactors.Length; i++)
                { builder.AppendFormat(" {0} {1}{2}", arrayOfFactorTags[i].Substring(0, 6), arrayOfFactors[i] > 0 ? "+" : "", arrayOfFactors[i]); }
                builder.AppendLine();
                //compatibility
                int compatibility = personality.GetCompatibilityWithPlayer();
                builder.AppendFormat(" Compatibility with Player {0}{1}{2}", compatibility > 0 ? "+" : "", compatibility, "\n");
                //descriptors
                listOfDescriptors = personality.GetListOfDescriptors();
                if (listOfDescriptors != null)
                {
                    count = listOfDescriptors.Count;
                    if (count > 0)
                    {
                        foreach (string item in listOfDescriptors)
                        { builder.AppendFormat("   {0}{1}", item, "\n"); }
                    }
                }
                else { Debug.LogWarning("Invalid listOfDescriptors (Null)"); }
                if (string.IsNullOrEmpty(personality.GetProfile()) == false)
                {
                    //profile
                    builder.AppendFormat(" {0} personality{1}", personality.GetProfileDescriptor(), "\n");
                    //profile explanation
                    builder.AppendFormat("  \"{0}\"{1}", personality.GetProfileExplanation(), "\n");
                }
                else { builder.AppendFormat(" No matching Personality Profile{0}", "\n"); }
            }
            else { Debug.LogError("Invalid arrayOfFactors (Null)"); }
        }
        else { Debug.LogError("Invalid personality (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// display all actors in dictionary along with compatibility with player for balancing purposes
    /// </summary>
    /// <returns></returns>
    public string DebugCheckActorCompatibilityRange()
    {
        StringBuilder builder = new StringBuilder();
        Dictionary<int, Actor> dictOfActors = GameManager.instance.dataScript.GetDictOfActors();
        if (dictOfActors != null)
        {
            float tally = 0.0f;
            int compatibility;
            builder.AppendFormat("- Compatibility with Player (All Actors){0}", "\n");
            foreach (var actor in dictOfActors)
            {
                compatibility = actor.Value.GetPersonality().GetCompatibilityWithPlayer();
                tally += compatibility;
                builder.AppendFormat(" {0}, {1}, Compatibility: {2}{3}", actor.Value.actorName, actor.Value.arc.name, compatibility, "\n");
            }
            //average value
            builder.AppendFormat(" {0}{1}Average Compatibility: {2}", "\n", "\n", tally / dictOfActors.Count);
        }
        else { Debug.LogError("Invalid dictOfActors (Null)"); }
        return builder.ToString();
    }

}
