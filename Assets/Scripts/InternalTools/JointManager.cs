using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Holds SO's that are used jointly by GameManager.cs and ToolManager.cs
/// Enables a single source for both Managers to access and prevents duplicates
/// </summary>
public class JointManager : MonoBehaviour
{
    #region Globals
    [HideInInspector] public GlobalSide sideAuthority;
    [HideInInspector] public GlobalSide sideResistance;

    #endregion

    #region Arrays
    [Header("Duplicates Shared by Game and Tool Managers")]
    [Tooltip("Resistance and Authority only")]
    public GlobalSide[] arrayOfGlobalSide;
    [Tooltip("All")]
    public NameSet[] arrayOfNameSets;
    [Tooltip("Both sides)")]
    public ActorArc[] arrayOfActorArcs;
    [Tooltip("Excludes Mayor traits")]
    public Trait[] arrayOfTraits;

    [Header("ActorDrafts -> ToolManager only")]
    public ActorDraftSex[] arrayOfActorDraftSex;
    public ActorDraftStatus[] arrayOfActorDraftStatus;
    #endregion

    #region Initialise
    public void Initialise()
    {
        int numArray;
        //
        // - - - ActorDraftSex (not stored in a collection)
        //
        numArray = arrayOfActorDraftSex.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] Initialise -> arrayOfActorDraftSex has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" JointManager.cs -> InitialiseStart: No ActorDraftSex present"); }
        //
        // - - - ActorDraftStatus (not stored in a collection)
        //
        numArray = arrayOfActorDraftStatus.Length;
        if (numArray > 0)
        { Debug.LogFormat("[Loa] Initialise -> arrayOfActorDraftStatus has {0} entries{1}", numArray, "\n"); }
        else { Debug.LogWarning(" JointManager.cs -> InitialiseStart: No ActorDraftStatus present"); }
        //
        // - - - GlobalSide - - -
        //
        int num = arrayOfGlobalSide.Length;
        if (num > 0)
        {
            for (int i = 0; i < num; i++)
            {
                GlobalSide assetSO = arrayOfGlobalSide[i];
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign GlobalSide.level values (0/1/2). 
                switch (assetSO.name)
                {
                    case "Authority":
                        sideAuthority = assetSO;
                        assetSO.level = 1;
                        break;
                    case "Resistance":
                        sideResistance = assetSO;
                        assetSO.level = 2;
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid side \"{0}\"", assetSO.name);
                        break;
                }
                
            }
            //error check
            Debug.Assert(sideAuthority != null, "Invalid sideAuthority (Null)");
            Debug.Assert(sideResistance != null, "Invalid sideResistance (Null)");
        }
        //
        // - - - Traits
        //
        Dictionary<string, Trait> dictOfTraits = ToolManager.i.toolDataScript.GetDictOfTraits();
        if (dictOfTraits != null)
        {
            numArray = arrayOfTraits.Length;
            if (numArray > 0)
            { Debug.LogFormat("[Loa] Initialise -> arrayOfTraits has {0} entries{1}", numArray, "\n"); }
            else { Debug.LogWarning(" LoadManager.cs -> Initialise: No Trait present"); }
            //add to dictionary
            for (int i = 0; i < numArray; i++)
            {
                Trait trait = arrayOfTraits[i];
                if (trait != null)
                {
                    try
                    { dictOfTraits.Add(trait.name, trait); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate record exists in dictOfTraits for {0}", trait); }
                }
                else { Debug.LogWarningFormat("Invalid Trait (Null) for arrayOfTraits[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfTraits (Null)"); }
        //
        // - - - ActorArcs
        //
        Dictionary<string, ActorArc> dictOfActorArcs = ToolManager.i.toolDataScript.GetDictOfActorArcs();
        if (dictOfActorArcs != null)
        {
            numArray = arrayOfActorArcs.Length;
            if (numArray > 0)
            { Debug.LogFormat("[Loa] Initialise -> arrayOfActorArcs has {0} entries{1}", numArray, "\n"); }
            else { Debug.LogWarning(" LoadManager.cs -> Initialise: No ActorArc present"); }
            //add to dictionary
            for (int i = 0; i < numArray; i++)
            {
                ActorArc arc = arrayOfActorArcs[i];
                if (arc != null)
                {
                    try
                    { dictOfActorArcs.Add(arc.name, arc); }
                    catch (ArgumentException)
                    { Debug.LogWarningFormat("Duplicate record exists in dictOfActorArcs for {0}", arc); }
                }
                else { Debug.LogWarningFormat("Invalid ActorArc (Null) for arrayOfActorArcs[{0}]", i); }
            }
        }
        else { Debug.LogError("Invalid dictOfActorArcs (Null)"); }
    }
    #endregion

    #region Utilities
    //
    // - - - Utilities
    //

    /// <summary>
    /// returns list of ActorArcs by side, null if a problem
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public List<ActorArc> GetActorArcs(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        List<ActorArc> listOfArcs = null;
        if (arrayOfActorArcs != null)
        {
            switch (side.name)
            {
                case "Authority":
                    listOfArcs = arrayOfActorArcs.Where(x => string.Equals(x.side.name, "Authority", System.StringComparison.Ordinal) == true).ToList();
                    break;
                case "Resistance":
                    listOfArcs = arrayOfActorArcs.Where(x => string.Equals(x.side.name, "Resistance", System.StringComparison.Ordinal) == true).ToList();
                    break;
                default: Debug.LogWarningFormat("Unrecognised globalSide.name \"{0}\"", side.name); break;
            }
        }
        else { Debug.LogError("Invalid arrayOfActorArcs (Null)"); }
        return listOfArcs;
    }

    /// <summary>
    /// Return a list of NameSets
    /// </summary>
    /// <returns></returns>
    public List<NameSet> GetListOfNameSets()
    {
        return arrayOfNameSets.Select(x => x).ToList();
    }

    #endregion



    //new methods above here
}
