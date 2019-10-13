using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all global SO enum equivalents and key Global strings
/// </summary>
public class GlobalManager : MonoBehaviour
{
    [Header("Global strings")]
    [Tooltip("Name of overarching AI Protagonist that drives the Authority side. Player's main enemy")]
    public string tagGlobalAIName = "GOLIATH";
    [Tooltip("Name of drug that the ADDICTED Condition refers to, eg. 'Player addicted to ...'")]
    public string tagGlobalDrug = "Moon Dust";

    //used for quick reference -> Meta Levels where metaBottom is the lowest level and metaTop is the highest
    [HideInInspector] public GlobalMeta metaBottom;
    [HideInInspector] public GlobalMeta metaMiddle;
    [HideInInspector] public GlobalMeta metaTop;
    //globalChance
    [HideInInspector] public GlobalChance chanceLow;
    [HideInInspector] public GlobalChance chanceMedium;
    [HideInInspector] public GlobalChance chanceHigh;
    [HideInInspector] public GlobalChance chanceExtreme;
    //globalType
    [HideInInspector] public GlobalType typeGood;
    [HideInInspector] public GlobalType typeNeutral;
    [HideInInspector] public GlobalType typeBad;
    //globalSide
    [HideInInspector] public GlobalSide sideAI;
    [HideInInspector] public GlobalSide sideAuthority;
    [HideInInspector] public GlobalSide sideResistance;
    [HideInInspector] public GlobalSide sideBoth;
    //globalWho
    [HideInInspector] public GlobalWho whoPlayer;
    [HideInInspector] public GlobalWho whoActor;
    //NodeDatapoint
    [HideInInspector] public NodeDatapoint nodeStability;
    [HideInInspector] public NodeDatapoint nodeSupport;
    [HideInInspector] public NodeDatapoint nodeSecurity;
    //TraitCategory
    [HideInInspector] public TraitCategory categoryActor;
    [HideInInspector] public TraitCategory categoryMayor;

    #region Initialise
    public void Initialise(GameState state)
    {
        //field checks
        Debug.Assert(string.IsNullOrEmpty(tagGlobalAIName) == false, "Invalid tagGlobalAIName (Null or Empty)");
        Debug.Assert(string.IsNullOrEmpty(tagGlobalDrug) == false, "Invalid tagGlobalDrug (Null or Empty)");
        //main
        int num;
        GlobalMeta[] arrayOfGlobalMeta = GameManager.instance.loadScript.arrayOfGlobalMeta;
        num = arrayOfGlobalMeta.Length;
        if (num > 0)
        {
            for (int i = 0; i < num; i++)
            {
                GlobalMeta assetSO = arrayOfGlobalMeta[i];
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign GlobalMeta.level values (0/1/2). 
                switch (assetSO.name)
                {
                    case "Local":
                        metaBottom = assetSO;
                        assetSO.level = 0;
                        break;
                    case "State":
                        metaMiddle = assetSO;
                        assetSO.level = 1;
                        break;
                    case "National":
                        metaTop = assetSO;
                        assetSO.level = 2;
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid meta \"{0}\"", assetSO.name);
                        break;
                }
            }
            //error check
            if (metaBottom == null) { Debug.LogError("Invalid metaBottom (Null)"); }
            if (metaMiddle == null) { Debug.LogError("Invalid metaMiddle (Null)"); }
            if (metaTop == null) { Debug.LogError("Invalid metaTop (Null)"); }
        }
        //
        // - - - GlobalChance - - -
        //
        GlobalChance[] arrayOfGlobalChance = GameManager.instance.loadScript.arrayOfGlobalChance;
        num = arrayOfGlobalChance.Length;
        if (num > 0)
        {
            for (int i = 0; i < num; i++)
            {
                GlobalChance assetSO = arrayOfGlobalChance[i];
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign GlobalChance.level values (0/1/2). 
                switch (assetSO.name)
                {
                    case "Low":
                        chanceLow = assetSO;
                        assetSO.level = 0;
                        break;
                    case "Medium":
                        chanceMedium = assetSO;
                        assetSO.level = 1;
                        break;
                    case "High":
                        chanceHigh = assetSO;
                        assetSO.level = 2;
                        break;
                    case "Extreme":
                        chanceExtreme = assetSO;
                        assetSO.level = 3;
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid chance \"{0}\"", assetSO.name);
                        break;
                }
            }
            //error check
            if (chanceLow == null) { Debug.LogError("Invalid chanceLow (Null)"); }
            if (chanceMedium == null) { Debug.LogError("Invalid chanceMedium (Null)"); }
            if (chanceHigh == null) { Debug.LogError("Invalid chanceHigh (Null)"); }
            if (chanceExtreme == null) { Debug.LogError("Invalid chanceCritical (Null)"); }
        }
        //
        // - - - GlobalWho - - -
        //
        GlobalWho[] arrayOfGlobalWho = GameManager.instance.loadScript.arrayOfGlobalWho;
        num = arrayOfGlobalWho.Length;
        if (num > 0)
        {
            for (int i = 0; i < num; i++)
            {
                GlobalWho assetSO = arrayOfGlobalWho[i];
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign GlobalWho.level values (0/1/2). 
                switch (assetSO.name)
                {
                    case "Player":
                        whoPlayer = assetSO;
                        assetSO.level = 0;
                        break;
                    case "Actor":
                        whoActor = assetSO;
                        assetSO.level = 1;
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid GlobalWho \"{0}\"", assetSO.name);
                        break;
                }
            }
            //error check
            if (whoPlayer == null) { Debug.LogError("Invalid whoPlayer (Null)"); }
            if (whoActor == null) { Debug.LogError("Invalid whoActor (Null)"); }
        }
        //
        // - - - GlobalType - - -
        //
        GlobalType[] arrayOfGlobalType = GameManager.instance.loadScript.arrayOfGlobalType;
        num = arrayOfGlobalType.Length;
        if (num > 0)
        {
            for (int i = 0; i < num; i++)
            {
                GlobalType assetSO = arrayOfGlobalType[i];
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign GlobalType.level values (0/1/2). 
                switch (assetSO.name)
                {
                    case "Bad":
                        typeBad = assetSO;
                        assetSO.level = 0;
                        break;
                    case "Neutral":
                        typeNeutral = assetSO;
                        assetSO.level = 1;
                        break;
                    case "Good":
                        typeGood = assetSO;
                        assetSO.level = 2;
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid type \"{0}\"", assetSO.name);
                        break;
                }
            }
            //error check
            if (typeBad == null) { Debug.LogError("Invalid typeBad (Null)"); }
            if (typeNeutral == null) { Debug.LogError("Invalid typeNeutral (Null)"); }
            if (typeGood == null) { Debug.LogError("Invalid typeGood (Null)"); }
        }
        //
        // - - - GlobalSide - - -
        //
        GlobalSide[] arrayOfGlobalSide = GameManager.instance.loadScript.arrayOfGlobalSide;
        num = arrayOfGlobalSide.Length;
        if (num > 0)
        {
            for (int i = 0; i < num; i++)
            {
                GlobalSide assetSO = arrayOfGlobalSide[i];
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign GlobalSide.level values (0/1/2). 
                switch (assetSO.name)
                {
                    case "AI":
                        sideAI = assetSO;
                        assetSO.level = 0;
                        break;
                    case "Authority":
                        sideAuthority = assetSO;
                        assetSO.level = 1;
                        break;
                    case "Resistance":
                        sideResistance = assetSO;
                        assetSO.level = 2;
                        break;
                    case "Both":
                        sideBoth = assetSO;
                        assetSO.level = 3;
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid side \"{0}\"", assetSO.name);
                        break;
                }
            }
            //error check
            Debug.Assert(sideAI != null, "Invalid sideAI (Null)");
            Debug.Assert(sideAuthority != null, "Invalid sideAuthority (Null)");
            Debug.Assert(sideResistance != null, "Invalid sideResistance (Null)");
            Debug.Assert(sideBoth != null, "Invalid sideBoth (Null)");
        }
        NodeDatapoint[] arrayOfNodeDatapoints = GameManager.instance.loadScript.arrayOfNodeDatapoints;
        num = arrayOfNodeDatapoints.Length;
        if (num > 0)
        {
            for (int i = 0; i < num; i++)
            {
                NodeDatapoint assetSO = arrayOfNodeDatapoints[i];
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign NodeDatapoint.level values (0/1/2). 
                switch (assetSO.name)
                {
                    case "Stability":
                        nodeStability = assetSO;
                        assetSO.level = 0;
                        break;
                    case "Support":
                        nodeSupport = assetSO;
                        assetSO.level = 1;
                        break;
                    case "Security":
                        nodeSecurity = assetSO;
                        assetSO.level = 2;
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid NodeDatapoint \"{0}\"", assetSO.name);
                        break;
                }
            }
            //error check
            Debug.Assert(nodeStability != null, "Invalid nodeStability (Null)");
            Debug.Assert(nodeSupport != null, "Invalid nodeSupport (Null)");
            Debug.Assert(nodeSecurity != null, "Invalid nodeSecurity (Null)");
        }
        //
        // - - - Trait Categories - - -
        //
        categoryActor = GameManager.instance.dataScript.GetTraitCategory("Actor");
        categoryMayor = GameManager.instance.dataScript.GetTraitCategory("Mayor");
        Debug.Assert(categoryActor != null, "Invalid categoryActor");
        Debug.Assert(categoryMayor != null, "Invalid categoryMayor");
    }
    #endregion

    //new methods above here
}
