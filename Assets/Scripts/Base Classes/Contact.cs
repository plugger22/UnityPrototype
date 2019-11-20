using gameAPI;
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
    public int turnTotal;               //total turns active (might have gone inactive or to reserves with actor for a while)
    public int usefulIntel;             //tracks number of useful intel items sourced by the contact
    public string typeName;             //ContactType.name
    public ContactStatus status;
    public ActorSex sex;
    //public bool isMale;                 //Male if true, female if false
    public bool isTurned;               //working for Authority as an informant

    [HideInInspector] public int timerInactive;    //countdown timer set when contact becomes inactive. Flips to active once timer reaches zero (ContactManager.cs -> CheckContacts)
    //stats
    [HideInInspector] public int statsRumours;     //number of target rumours learnt
    [HideInInspector] public int statsNemesis;     //number of times spotted Nemesis
    [HideInInspector] public int statsTeams;       //number of times spotted Erasure Teams
    [HideInInspector] public int statsNpc;         //number of times spotted Npc
    #endregion


    /// <summary>
    /// Use for making contact Inactive (avoid doing so directly). Handles all admin. Reason is '[due to a]...', keep short, eg. 'due to a Decision'
    /// </summary>
    public void SetInactive(string reason = "Unknown")
    {
        status = ContactStatus.Inactive;
        timerInactive = GameManager.instance.contactScript.timerInactive;
        //admin
        Debug.LogFormat("[Cnt] Contact.cs -> SetInactive: {0} {1}, {2} at nodeID {3}, actorID {4}, Status now INACTIVE ({5}){6}", nameFirst, nameLast, job, nodeID, actorID, status, "\n");
        string text = string.Format("Contact {0} {1}, {2}, nodeID {3}, actorID {4}, goes INACTIVE{5}", nameFirst, nameLast, job, nodeID, actorID, "\n");
        Actor actor = GameManager.instance.dataScript.GetActor(actorID);
        if (actor != null)
        {
            Node node = GameManager.instance.dataScript.GetNode(nodeID);
            if (node != null)
            { GameManager.instance.messageScript.ContactInactive(text, reason, actor, node, this); }
            else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", nodeID); }
        }
        else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", actorID); }
    }
}
