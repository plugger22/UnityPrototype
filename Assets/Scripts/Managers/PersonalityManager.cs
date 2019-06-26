using gameAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all personality related matters
/// </summary>
public class PersonalityManager : MonoBehaviour
{

    [Tooltip("Number of personality factors present")]
    [Range(5, 5)] public int numOfFactors = 5;
    /*[Tooltip("Number of Five Factor Model personality factors present")]
    [Range(5, 5)] public int numOfFiveFactorModel = 5;
    [Tooltip("Number of Dark Triad personality factors present")]
    [Range(3, 3)] public int numOfDarkTriad = 3;*/

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
        SetAllActorsCompatibilityWithPlayer();
    }
    #endregion

    #endregion

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
        { arrayOfCriteria = new int[] { 0, 0, 0, 0, 0 }; }
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
            switch (rndNum)
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

    /// <summary>
    /// loop through entire suite of actors in a level (even those in reserve pools) and set their compatibility with the player
    /// </summary>
    private void SetAllActorsCompatibilityWithPlayer()
    {
        Dictionary<int, Actor> dictOfActors = GameManager.instance.dataScript.GetDictOfActors();
        if (dictOfActors != null)
        {
            int compatibility;
            foreach(var actor in dictOfActors)
            {
                if (actor.Value != null)
                {
                    compatibility = CheckCompatibilityWithPlayer(actor.Value.GetPersonality().GetFactors());
                    actor.Value.GetPersonality().SetCompatibilityWithPlayer(compatibility);
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actor.Key); }
            }
        }
        else { Debug.LogError("Invalid dictOfActors (Null)"); }
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
        }
        else { Debug.LogError("Invalid playerPersonality.arrayOfFactors (Null)"); }
        return compatibility;
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
                        builder.Append(DebugDisplayIndividualPersonality(actor.GetPersonality()));
                        builder.AppendLine();
                    }
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        //Reserve actors
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
        else { Debug.LogError("Invalid listOfReservePool (Null)"); }
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
        if (personality != null)
        {
            int[] arrayOfFactors = personality.GetFactors();
            if (arrayOfFactors != null)
            {
                for (int i = 0; i < arrayOfFactors.Length; i++)
                { builder.AppendFormat(" {0} {1}{2}", arrayOfFactorTags[i].Substring(0, 6), arrayOfFactors[i] > 0 ? "+" : "", arrayOfFactors[i]); }
                builder.AppendLine();
                int compatibility = personality.GetCompatibilityWithPlayer();
                builder.AppendFormat(" Compatibility with Player {0}{1}{2}", compatibility > 0 ? "+" : "", compatibility, "\n");
            }
            else { Debug.LogError("Invalid arrayOfFactors (Null)"); }
        }
        else { Debug.LogError("Invalid personality (Null)"); }
        return builder.ToString();
    }

}
