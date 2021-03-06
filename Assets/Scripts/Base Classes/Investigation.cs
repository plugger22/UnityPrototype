﻿using gameAPI;
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
    [HideInInspector] public string city;                   //city where investigation commenced
    [HideInInspector] public string tag;                    //public display name
    [HideInInspector] public int turnStart = -1;            //turn investigation commenced
    [HideInInspector] public int turnFinish = -1;           //turn investigation completed
    [HideInInspector] public int evidence = 2;              //evidence is a standard 3 star setup with 0 (incriminating) being bad for the player and 3 being good (exonerating)
    [HideInInspector] public int previousEvidence = 2;      //whenever evidence changes this records what the evidence was prior to the change (used for evidence message)
    [HideInInspector] public int timer = -1;                //count down timer activated once evidence boundary exceeded. Investigation resolved when timer at zero, default -1
    [HideInInspector] public bool isOrgHQNormal;            //flag set true if OrgHQ has intervened (topic, not outcome) during the course of a normal (non 'timer') investigation
    [HideInInspector] public bool isOrgHQTimer;             //flag set true if OrgHQ has intervented (topic, not outcome) during the course of an investigation's resolution timer period 
    [HideInInspector] public ActorHQ lead;                  //HQ actor position in charge of resolving the investigation
    [HideInInspector] public InvestStatus status;           //status of investigation
    [HideInInspector] public InvestOutcome outcome;         //outcome once investigation resolved, default 'None'
    #endregion

}
