using gameAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Handles all actor contact matters. Resistance contacts are modelled in detail (contact.cs), Authority contacts are simply nodes where the actor has influence (dictOfActorContacts / dictOfNodeContacts)
/// </summary>
public class ContactManager : MonoBehaviour
{
    [Tooltip("How many contacts per level of influence/connections that the actor has")]
    [Range(1,3)] public int contactsPerLevel = 2;
    [Tooltip("Number of contacts to initially seed the contact pool with")]
    [Range(50, 100)] public int numOfPoolContacts = 50;
    [Tooltip("Number of contacts remaining in the contactPool when a top up is required")]
    [Range(5, 20)] public int numOfPoolThreshold = 10;
    [Tooltip("Number of contacts to top up the pool with once the threshold is reached")]
    [Range(10, 50)] public int numOfPoolTopUp = 20;

    private static int contactIDCounter = 0;              //used to sequentially number contactID's

    //fast access fields
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;


    public void Initialise()
    {
        //seed contact pool
        CreateContacts(numOfPoolContacts);
        //fast acess fields
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        //check O.K
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
    }

    /// <summary>
    /// create new Resistance contacts and place them dictionary, unassigned
    /// </summary>
    public void CreateContacts(int numOfContacts)
    {
        Debug.Assert(numOfContacts > 0, "Invalid numOfContacts (zero, or less)");
        Dictionary<int, Contact> dictOfContacts = GameManager.instance.dataScript.GetDictOfContacts();
        List<int> contactPool = GameManager.instance.dataScript.GetContactPool();
        if (dictOfContacts != null)
        {
            if (contactPool != null)
            {
                int counter = 0;
                for (int i = 0; i < numOfContacts; i++)
                {
                    Contact contact = new Contact();
                    //initialise contact
                    contact.contactID = contactIDCounter++;
                    contact.contactName = string.Format("PersonName_{0}", contact.contactID);
                    contact.job = string.Format("Job_{0}", contact.contactID);
                    contact.dataText0 = string.Format("MegaCorp_{0}", contact.contactID);
                    contact.dataText1 = string.Format("NeedyCharacter_{0}", contact.contactID);
                    contact.status = ContactStatus.ContactPool;
                    contact.actorID = -1;
                    contact.nodeID = -1;
                    contact.isTurned = false;
                    contact.turnStart = -1;
                    contact.turnFinish = -1;
                    //add to dictionary
                    try
                    {
                        dictOfContacts.Add(contact.contactID, contact);
                        counter++;
                        //add to list
                        contactPool.Add(contact.contactID);
                    }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Invalid entry in dictOfContacts for contact {0}, ID {1}", contact.contactName, contact.contactID); }
                }
                Debug.LogFormat("[Tst] ContactManager.cs -> CreateContacts: {0} out of {1} contacts created and added to pool", counter, numOfContacts);
                Debug.LogFormat("[Tst] ContactManager.cs -> CreateContacts: contactPool has {0} records", contactPool.Count);
            }
            else { Debug.LogError("Invalid contactPool list (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfContacts (Null)"); }
    }

    /// <summary>
    /// Called whenever a new contact is needed. A randomly selected contactID is chosen from contactPool and the contact returned to the actor requiring it. Returns Null if a problem
    /// </summary>
    /// <returns></returns>
    public Contact AssignContact(int actorID, int nodeID)
    {
        Debug.Assert(actorID > -1, "Invalid actorID (less than Zero)");
        Debug.Assert(nodeID > -1, "Invalid nodeID (Less than Zero)");
        Contact contact = null;
        int contactID = -1;
        List<int> contactPool = GameManager.instance.dataScript.GetContactPool();
        if (contactPool != null)
        {
            int index = Random.Range(0, contactPool.Count);
            contactID = contactPool[index];
            //delete record from pool to prevent duplicates
            contactPool.RemoveAt(index);
            //get contact
            contact = GameManager.instance.dataScript.GetContact(contactID);
            if (contact != null)
            {
                //initialise data
                contact.actorID = actorID;
                contact.nodeID = nodeID;
                contact.status = ContactStatus.Active;
                contact.turnStart = GameManager.instance.turnScript.Turn;
            }
            else
                { Debug.LogErrorFormat("Invalid contact (Null) for contactID {0}", contactID); }
            //check if pool requires topping up
            if (contactPool.Count <= numOfPoolThreshold)
            {
                //top up pool
                CreateContacts(numOfPoolTopUp);
                Debug.LogFormat("[Tst] ContactManager.cs -> AssignContact: ContactPool topped up, now has {0} records", contactPool.Count);
            }
        }
        else { Debug.LogError("Invalid contactPool list (Null)"); }
        return contact;
    }

    /// <summary>
    /// Initialises contacts for a new actor
    /// </summary>
    /// <param name="actor"></param>
    public void SetActorContacts(Actor actor)
    {
        int index, nodeID, numOfNodes, numOfContacts;
        if (actor != null)
        {
            //check if actor already has been OnMap and has a previous set of contacts
            Dictionary<int, List<int>> dictOfActorContacts = GameManager.instance.dataScript.GetDictOfActorContacts();
            if (dictOfActorContacts != null)
            {
                //actor in dictionary?
                if (dictOfActorContacts.ContainsKey(actor.actorID) == true)
                {
                    //previous contacts exist
                    List<int> listOfNodes = dictOfActorContacts[actor.actorID];
                    if (listOfNodes != null)
                    {
                        //add to dictOfNodeContacts only (already an entry present in dictOfActorContacts)
                        GameManager.instance.dataScript.AddContacts(actor.actorID, listOfNodes, false);
                        //reactivate (set status -> 'active') all relevant actor contacts
                        Dictionary<int, Contact> dictOfContacts = actor.GetDictOfContacts();
                        if (dictOfContacts != null)
                        {
                            foreach(var contact in dictOfContacts)
                            {
                                nodeID = contact.Value.nodeID;
                                //check nodeID in list
                                if (listOfNodes.Exists(x => x == nodeID) == true)
                                {
                                    //reactivate contact
                                    contact.Value.status = ContactStatus.Active;
                                }
                                else
                                {
                                    //contact not found in list, warning message
                                    Debug.LogWarningFormat("Contact {0}, {1}, ID {2} nodeID {3} not found in listOfNodes & deletec", contact.Value.contactName, contact.Value.job, contact.Value.contactID,
                                        contact.Value.nodeID);
                                }
                            }
                        }
                        else { Debug.LogErrorFormat("Invalid dictOfContacts for actor {0}, {1}, ID {2}", actor.actorName, actor.arc.name, actor.actorID); }
                    }
                    else { Debug.LogWarningFormat("Invalid listOfNodes (Null) for actorID {0}", actor.actorID); }
                }
                else
                {
                    //generate a new set of contacts for the actor
                    int contactLevel = actor.datapoint0;
                    int totalContacts = contactLevel * contactsPerLevel;
                    List<Node> listOfAllNodes = GameManager.instance.dataScript.GetListOfAllNodes();
                    List<int> listOfContactNodes = new List<int>();
                    if (listOfAllNodes != null)
                    {
                        //create a templist of nodes
                        List<Node> tempListOfNodes = new List<Node>(listOfAllNodes);
                        //randomly select nodes up to total contacts
                        for (int i = 0; i < totalContacts; i++)
                        {
                            //check minimum number of nodes left to select from
                            numOfNodes = tempListOfNodes.Count;
                            if (numOfNodes > 0)
                            {
                                index = Random.Range(0, tempListOfNodes.Count);
                                nodeID = tempListOfNodes[index].nodeID;
                                listOfContactNodes.Add(nodeID);
                                //remove chosen node to prevent duplicates
                                tempListOfNodes.RemoveAt(index);
                            }
                            else { break; }
                        }
                        //Contact nodes determined, now add to dictionaries
                        numOfContacts = listOfContactNodes.Count;
                        if (numOfContacts > 0)
                        {
                            //add to dictOfActorContacts & dictOfNodeContacts
                            GameManager.instance.dataScript.AddContacts(actor.actorID, listOfContactNodes);
                            if (actor.side.level == globalResistance.level)
                            {
                                //add to actor (Resistance side only)
                                for (int i = 0; i < numOfContacts; i++)
                                {
                                    Contact contact = AssignContact(actor.actorID, listOfContactNodes[i]);
                                    if (contact != null)
                                    {
                                        actor.AddContact(contact);
                                        Debug.LogFormat("[Tst] Contact Added: {0}, {1}, actorID {2}, nodeID {3}, {4}, contactID {5}{6}", actor.actorName, actor.arc.name, actor.actorID, listOfContactNodes[i],
                                            contact.contactName, contact.contactID, "\n");
                                    }
                                    else { Debug.LogError("Invalid contact (Null)"); }
                                }
                            }
                        }
                        else { Debug.LogWarningFormat("Actor {0}, {1} has no contacts", actor.actorName, actor.arc.name); }
                    }
                    else { Debug.LogError("Invalid listOfNodes (Null)"); }
                }
            }
            else { Debug.LogError("Invalid dictOfActorContacts (Null)"); }
        }
        else { Debug.LogError("Invalid actor (Null)"); }
    }


    /// <summary>
    /// Update Node contact status across the map whenever there is a change. Contact state updated for, default, Current side only
    /// </summary>
    public void UpdateNodeContacts(bool isCurrentSide = true)
    {
        List<Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        Dictionary<int, List<int>> dictOfNodeContacts = GameManager.instance.dataScript.GetDictOfNodeContacts(isCurrentSide);
        if (dictOfNodeContacts != null)
        {
            if (listOfNodes != null)
            {
                //player side
                bool isResistance;
                if (isCurrentSide == true)
                {
                    //player side
                    if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
                    { isResistance = false; }
                    else { isResistance = true; }
                }
                else
                {
                    //non player side
                    if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level)
                    { isResistance = true; }
                    else { isResistance = false; }
                }
                //loop all nodes
                for (int i = 0; i < listOfNodes.Count; i++)
                {
                    Node node = listOfNodes[i];
                    if (node != null)
                    {
                        //find entry in dictionary
                        if (dictOfNodeContacts.ContainsKey(node.nodeID) == true)
                        {
                            //get list of contacts for node
                            List<int> tempList = dictOfNodeContacts[node.nodeID];
                            if (tempList != null)
                            {
                                //Check if contacts present at node or not
                                if (tempList.Count > 0)
                                {
                                    if (isResistance == true)
                                    { node.isContactResistance = true; }
                                    else { node.isContactAuthority = true; }
                                }
                                else
                                {
                                    if (isResistance == true)
                                    { node.isContactResistance = false; }
                                    else { node.isContactAuthority = false; }
                                }
                            }
                            else
                            {
                                Debug.LogWarningFormat("Invalid tempList (Null) for nodeID {0}", node.nodeID);
                                //no contacts (default)
                                if (isResistance == true)
                                { node.isContactResistance = false; }
                                else { node.isContactAuthority = false; }
                            }
                        }
                        else
                        {
                            //No contacts at node
                            if (isResistance == true)
                            { node.isContactResistance = false; }
                            else { node.isContactAuthority = false; }
                        }
                    }
                    else
                    { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}", listOfNodes[i]); }
                }
            }
            else { Debug.LogError("Invalid listOfNodes (Null)"); }
        }
        else { Debug.LogError("Invalid dictOfNodeContacts (Null)"); }
    }

    //
    // - - - Debug - - -
    //

    /// <summary>
    /// toggles node's isContactKnown property on / off
    /// </summary>
    /// <param name="textInput_0"></param>
    /// <returns></returns>
    public string DebugToggleIsContactKnown(string textInput_0)
    {
        Debug.Assert(string.IsNullOrEmpty(textInput_0) == false, "Invalid textInput_0 string (Null or Empty)");
        string resultText = "Unknown";
        int nodeID = Convert.ToInt32(textInput_0);
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            if (node.isContactKnown == true)
            {
                node.isContactKnown = false;
                resultText = string.Format("{0}, {1}, nodeID {2}{3}isContactKnown FALSE", node.nodeName, node.Arc.name, node.nodeID, "\n");
            }
            else
            {
                node.isContactKnown = true;
                resultText = string.Format("{0}, {1}, nodeID {2}{3}isContactKnown TRUE", node.nodeName, node.Arc.name, node.nodeID, "\n");
            }
        }
        else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", nodeID); }
        return resultText;
    }

    /// <summary>
    /// Debug Display resistance contacts by actor
    /// </summary>
    /// <returns></returns>
    public string DisplayContacts()
    {
        StringBuilder builder = new StringBuilder();
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
        if (arrayOfActors != null)
        {
            Dictionary<int, Contact> dictOfContacts;
            builder.AppendFormat("- Resistance Actor Contacts{0}{1}", "\n", "\n");
            int numOfContacts;
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    {
                        builder.AppendFormat(" {0}, {1}, actorID {2}{3}", actor.actorName, actor.arc.name, actor.actorID, "\n");
                        dictOfContacts = actor.GetDictOfContacts();
                        if (dictOfContacts != null)
                        {
                            numOfContacts = dictOfContacts.Count;
                            if (numOfContacts > 0)
                            {
                                foreach(var contact in dictOfContacts)
                                {
                                    Debug.Assert(contact.Value.actorID == actor.actorID, string.Format("Contact.actorID {0} doesn't match actorID {1}", contact.Value.contactID, actor.actorID));
                                    builder.AppendFormat(" Id {0}, {1}, {2}, nodeID {3}, {4}{5}", contact.Value.contactID, contact.Value.contactName, contact.Value.job, contact.Value.nodeID, 
                                        contact.Value.status, "\n");
                                }
                            }
                            else { builder.AppendFormat("No Contacts present{0}", "\n"); }
                        }
                        else { builder.AppendFormat("Invalid dictOfContacts (Null){0}", "\n"); }
                    }
                    else { Debug.LogErrorFormat("Invalid actor (Null) for arrayOfActors[{0}]", i); }
                }
                builder.AppendLine();
                builder.AppendLine();
            }
            //Reserve actors
            List<int> listOfReserveActors = GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Reserve);
            if (listOfReserveActors != null)
            {
                for (int i = 0; i < listOfReserveActors.Count; i++)
                {
                    Actor actor = GameManager.instance.dataScript.GetActor(listOfReserveActors[i]);
                    if (actor != null)
                    {
                        builder.AppendFormat("RESERVE {0}, {1}, actorID {2}{3}", actor.actorName, actor.arc.name, actor.actorID, "\n");
                        dictOfContacts = actor.GetDictOfContacts();
                        if (dictOfContacts != null)
                        {
                            numOfContacts = dictOfContacts.Count;
                            if (numOfContacts > 0)
                            {
                                foreach (var contact in dictOfContacts)
                                {
                                    Debug.Assert(contact.Value.actorID == actor.actorID, string.Format("Contact.actorID {0} doesn't match actorID {1}", contact.Value.contactID, actor.actorID));
                                    builder.AppendFormat(" Id {0}, {1}, {2}, nodeID {3}, {4}{5}", contact.Value.contactID, contact.Value.contactName, contact.Value.job, contact.Value.nodeID, 
                                        contact.Value.status, "\n");
                                }
                            }
                            else { builder.AppendFormat("No Contacts present{0}", "\n"); }
                        }
                        else { builder.AppendFormat("Invalid dictOfContacts (Null){0}", "\n"); }
                    }
                    else { Debug.LogWarningFormat("Invalid Reserve actor (Null) for actorID {0}", listOfReserveActors[i]); }
                    builder.AppendLine();
                    builder.AppendLine();
                }
            }
            else { Debug.LogError("Invalid listOfReserveActors (Null)"); }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
        return builder.ToString();
    }
}
