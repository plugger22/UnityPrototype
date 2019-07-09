﻿using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resistance Contact, non-Unity data class
/// NOTE: Class is serialized directly
/// </summary>
[System.Serializable]
public class Contact
{
    #region Save Compatabile data
    public int contactID;
    public string nameFirst;
    public string nameLast;
    public string job;
    public int actorID;
    public int nodeID;
    public int effectiveness;           //1 to 3 effectiveness in gaining new info (3 best, 1 worst)
    public int turnStart;               //turn started as a contact
    public int turnFinish;              //turn finished as a contact
    public int usefulIntel;             //tracks number of useful intel items sourced by the contact
    public string typeName;             //ContactType.name
    public ContactStatus status;
    /*public string statusName;*/
    public bool isMale;                 //Male if true, female if false
    public bool isTurned;               //working for Authority as an informant

    //stats
    [HideInInspector] public int statsRumours;     //number of target rumours learnt
    [HideInInspector] public int statsNemesis;     //number of times spotted Nemesis
    [HideInInspector] public int statsTeams;       //number of times spotted Erasure Teams
    #endregion

}
