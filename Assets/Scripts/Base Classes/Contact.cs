using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resistance Contact, non-Unity data class
/// </summary>
public class Contact
{
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
    public ContactType type;            //job category
    public ContactStatus status;
    public bool isMale;                 //Male if true, female if false
    public bool isTurned;               //working for Authority as an informant
    
    

}
