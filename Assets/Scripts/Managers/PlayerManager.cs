using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all player related data and methods
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public int RebelCauseMax { get; set; }                         //level of Rebel Support. Max out to Win the level. Max level is a big part of difficulty.
    public int RebelCauseCurrent { get; set; }                      //current level of Rebel Support

    public int Renown { get; set; }

    public int NumOfRecruits { get; set; }
    public int NumOfGear { get; set; }

}
