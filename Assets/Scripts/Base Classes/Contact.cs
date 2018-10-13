using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resistance Contact, non-Unity data class
/// </summary>
public class Contact
{
    [HideInInspector] public int contactID;
    [HideInInspector] public string contactName;
    [HideInInspector] public string job;
    [HideInInspector] public string dataText0;              //multipurpose, could be level of job
    [HideInInspector] public string dataText1;              //multipurpose, could be name of corporation
    [HideInInspector] public int actorID;
    [HideInInspector] public int nodeID;
    [HideInInspector] public int turnStart;               //turn started as a contact
    [HideInInspector] public int turnFinish;              //turn finished as a contact
    [HideInInspector] public ContactType type;            //job category
    [HideInInspector] public ContactStatus status;
    [HideInInspector] public bool isTurned;               //working for Authority as an informant
    

}
