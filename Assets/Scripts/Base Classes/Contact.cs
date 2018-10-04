using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resistance Contact
/// </summary>
public class Contact : MonoBehaviour
{
    [HideInInspector] public int contactID;
    [HideInInspector] public string contactName;
    [HideInInspector] public string job;
    [HideInInspector] public string detail0;              //multipurpose, could be level of job
    [HideInInspector] public string detail1;              //multipurpose, could be name of corporation
    [HideInInspector] public int actorID;
    [HideInInspector] public int nodeID;
    [HideInInspector] public int turnStart;
    [HideInInspector] public int turnFinish;
    [HideInInspector] public ContactStatus status;
    [HideInInspector] public bool isTurned;               //working for Authority as an informant
    

}
