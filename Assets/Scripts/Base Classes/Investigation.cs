using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Investigations into the Player's behaviour. Normal base class (not a MonoBehaviour)
/// </summary>
[System.Serializable]
public class Investigation
{
    #region Save Compatabile data
    [HideInInspector] public string reference;              //reference name for dictionary key, searching etc. -> turn # + secretName
    [HideInInspector] public string tag;                    //public display name
    [HideInInspector] public int turnStart;                 //turn investigation commenced
    [HideInInspector] public int turnFinish;                //turn investigation completed
    [HideInInspector] public int evidence = 2;              //evidence is a standard 3 star setup with 0 (incriminating) being bad for the player and 3 being good (exonerating)
    [HideInInspector] public int timer = -1;                //count down timer activated once evidence boundary exceeded. Investigation resolved when timer at zero, default -1
    [HideInInspector] public ActorHQ lead;                  //HQ actor position in charge of resolving the investigation
    #endregion

}
