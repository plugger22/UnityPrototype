﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all global SO enum equivalents
/// </summary>
public class GlobalManager : MonoBehaviour
{
    //used for quick reference -> Meta Levels where metaBottom is the lowest level and metaTop is the highest
    [HideInInspector] public GlobalMeta metaBottom;
    [HideInInspector] public GlobalMeta metaMiddle;
    [HideInInspector] public GlobalMeta metaTop;
    //globalChance
    [HideInInspector] public GlobalChance chanceLow;
    [HideInInspector] public GlobalChance chanceMedium;
    [HideInInspector] public GlobalChance chanceHigh;
    //globalType
    [HideInInspector] public GlobalType typeGood;
    [HideInInspector] public GlobalType typeNeutral;
    [HideInInspector] public GlobalType typeBad;
    //globalSide
    [HideInInspector] public GlobalSide sideAI;
    [HideInInspector] public GlobalSide sideAuthority;
    [HideInInspector] public GlobalSide sideResistance;
    //TraitCategory
    [HideInInspector] public TraitCategory categoryActor;
    [HideInInspector] public TraitCategory categoryMayor;
    [HideInInspector] public TraitCategory categoryFaction;

    public void Initialise()
    {
        //
        // - - - GlobalMeta - - -
        //
        Dictionary<string, GlobalMeta> dictOfGlobalMeta = GameManager.instance.dataScript.GetDictOfGlobalMeta();
        if (dictOfGlobalMeta != null)
        {
            foreach (var meta in dictOfGlobalMeta)
            {
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign GlobalMeta.level values (0/1/2). 
                switch (meta.Key)
                {
                    case "Local":
                        metaBottom = meta.Value;
                        meta.Value.level = 0;
                        break;
                    case "State":
                        metaMiddle = meta.Value;
                        meta.Value.level = 1;
                        break;
                    case "National":
                        metaTop = meta.Value;
                        meta.Value.level = 2;
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
        Dictionary<string, GlobalChance> dictOfGlobalChance = GameManager.instance.dataScript.GetDictOfGlobalChance();
        if (dictOfGlobalChance != null)
        {
            foreach (var chance in dictOfGlobalChance)
            {
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign GlobalChance.level values (0/1/2). 
                switch (chance.Key)
                {
                    case "Low":
                        chanceLow = chance.Value;
                        chance.Value.level = 0;
                        break;
                    case "Medium":
                        chanceMedium = chance.Value;
                        chance.Value.level = 1;
                        break;
                    case "High":
                        chanceHigh = chance.Value;
                        chance.Value.level = 2;
                        break;
                }
            }
            //error check
            if (chanceLow == null) { Debug.LogError("Invalid chanceLow (Null)"); }
            if (chanceMedium == null) { Debug.LogError("Invalid chanceMedium (Null)"); }
            if (chanceHigh == null) { Debug.LogError("Invalid chanceHigh (Null)"); }
        }
        //
        // - - - GlobalType - - -
        //
        Dictionary<string, GlobalType> dictOfGlobalType = GameManager.instance.dataScript.GetDictOfGlobalType();
        if (dictOfGlobalType != null)
        {
            foreach (var type in dictOfGlobalType)
            {
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign GlobalType.level values (0/1/2). 
                switch (type.Key)
                {
                    case "Bad":
                        typeBad = type.Value;
                        type.Value.level = 0;
                        break;
                    case "Neutral":
                        typeNeutral = type.Value;
                        type.Value.level = 1;
                        break;
                    case "Good":
                        typeGood = type.Value;
                        type.Value.level = 2;
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
        Dictionary<string, GlobalSide> dictOfGlobalSide = GameManager.instance.dataScript.GetDictOfGlobalSide();
        if (dictOfGlobalSide != null)
        {
            foreach (var side in dictOfGlobalSide)
            {
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign GlobalSide.level values (0/1). 
                switch (side.Key)
                {
                    case "AI":
                        sideAI = side.Value;
                        side.Value.level = 0;
                        break;
                    case "Authority":
                        sideAuthority = side.Value;
                        side.Value.level = 1;
                        break;
                    case "Resistance":
                        sideResistance = side.Value;
                        side.Value.level = 2;
                        break;
                }
            }
            //error check
            if (sideAI == null) { Debug.LogError("Invalid sideAI (Null)"); }
            if (sideAuthority == null) { Debug.LogError("Invalid sideAuthority (Null)"); }
            if (sideResistance == null) { Debug.LogError("Invalid sideResistance (Null)"); }
        }
        //
        // - - - Trait Categories - - -
        //
        categoryActor = GameManager.instance.dataScript.GetTraitCategory("Actor");
        categoryMayor = GameManager.instance.dataScript.GetTraitCategory("Mayor");
        categoryFaction = GameManager.instance.dataScript.GetTraitCategory("Faction");
        Debug.Assert(categoryActor != null, "Invalid categoryActor");
        Debug.Assert(categoryMayor != null, "Invalid categoryMayor");
        Debug.Assert(categoryFaction != null, "Invalid categoryFaction");
    }


    //new methods above here
}
