using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all Objective related matters (both sides)
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
    [Tooltip("The maximum number of objectives in a level (determined by top widget UI)")]
    [Range(0, 3)] public int maxNumOfObjectives = 3;

    [HideInInspector] public int objectivesAuthority;            //how many objectives the Authority side has this level in total
    [HideInInspector] public int objectivesResistance;           //how many objectives the Resistance side has this level in total

    [HideInInspector] public int objectivesCurrentAuthority;        //how many objectives the Authority side currently has remaining to be completed   
    [HideInInspector] public int objectivesCurrentResistance;        //how many objectives the Resistance side currently has remaining to be completed

}
