using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all Resitance related matters
/// </summary>
public class ResistanceManager : MonoBehaviour
{
    [HideInInspector] public int rebelCauseMax;                        //level of Rebel Support. Max out to Win the level. Max level is a big part of difficulty.
    [HideInInspector] public int rebelCauseCurrent;                    //current level of Rebel Support


    public void Initialise()
    {
        rebelCauseMax = 10;
        rebelCauseCurrent = 0;
    }
}
