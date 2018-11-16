using gameAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Handles all actor contact matters. Resistance contacts are modelled in detail (contact.cs), Authority contacts are simply nodes where the actor has influence (dictOfActorContacts / dictOfNodeContacts)
/// </summary>
public class ContactManager : MonoBehaviour
{
    [Header("Base Data")]
    [Tooltip("How many contacts per level of influence/connections that the actor has")]
    [Range(1,3)] public int contactsPerLevel = 2;
    [Tooltip("Number of contacts to initially seed the contact pool with")]
    [Range(50, 100)] public int numOfPoolContacts = 50;
    [Tooltip("Number of contacts remaining in the contactPool when a top up is required")]
    [Range(5, 20)] public int numOfPoolThreshold = 10;
    [Tooltip("Number of contacts to top up the pool with once the threshold is reached")]
    [Range(10, 50)] public int numOfPoolTopUp = 20;

    [Header("Confidence Levels")]
    [Tooltip("Confidence level of a High Effectiveness contact ('3'), eg. % chance of being correct")]
    [Range(1, 100)] public int confidenceHigh = 100;
    [Tooltip("Confidence level of a Medium Effectiveness contact ('2'), eg. % chance of being correct")]
    [Range(1, 100)] public int confidenceMed = 75;
    [Tooltip("Confidence level of a Low Effectiveness contact ('1'), eg. % chance of being correct")]
    [Range(1, 100)] public int confidenceLow = 50;

    [Header("Rumours")]
    [Tooltip("Chance of somebody learning about an Active target, per turn")]
    [Range(0, 10)] public int rumourTarget = 5;
    [Tooltip("Maximum number of Target Rumours allowed per turn (avoids a RNG spam)")]
    [Range(1, 5)] public int maxRumoursTarget = 2;

    private int[] arrayOfContactNetworks;   //use for determining which actor's network of contacts was used
    private Actor[] arrayOfActors;          //used for contact activity. Updated from DataManager.cs each turn

    private static int contactIDCounter = 0;              //used to sequentially number contactID's

    //fast access fields
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;
    //traits
    private int actorContactEffectHigh;
    private int actorContactEffectLow;


    public void Initialise()
    {
        //collections
        int numOfOnMapActors = GameManager.instance.actorScript.maxNumOfOnMapActors;
        arrayOfContactNetworks = new int[numOfOnMapActors];
        arrayOfActors = new Actor[numOfOnMapActors];
        //seed contact pool
        CreateContacts(numOfPoolContacts);
        //fast acess fields
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        actorContactEffectHigh = GameManager.instance.dataScript.GetTraitEffectID("ActorContactEffectHigh");
        actorContactEffectLow = GameManager.instance.dataScript.GetTraitEffectID("ActorContactEffectLow");
        //check O.K
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(actorContactEffectHigh > -1, "Invalid actorContactEffectHigh (-1)");
        Debug.Assert(actorContactEffectLow > -1, "Invalid actorContactEffectLow (-1)");
        //event Listeners
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent, "MessageManager");
    }

    /// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.StartTurnLate:
                StartTurnLate();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Start Turn Late events
    /// </summary>
    private void StartTurnLate()
    {
        InitialiseNetworkArrays();
        CheckTargetRumours();
        GameManager.instance.nemesisScript.CheckNemesisContactSighting();
    }

    /// <summary>
    /// create new Resistance contacts and place them dictionary, unassigned
    /// </summary>
    public void CreateContacts(int numOfContacts)
    {
        Debug.Assert(numOfContacts > 0, "Invalid numOfContacts (zero, or less)");
        Dictionary<int, Contact> dictOfContacts = GameManager.instance.dataScript.GetDictOfContacts();
        List<int> contactPool = GameManager.instance.dataScript.GetContactPool();
        //weighted array for randomly determining effectiveness (weighted for an average outcome)
        int[] arrayOfEffectiveness = new int[] { 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 3 };
        int numEffectiveness = arrayOfEffectiveness.Length;
        if (dictOfContacts != null)
        {
            if (contactPool != null)
            {
                //Get NameSet SO from Country
                NameSet nameSet = null;
                City city = GameManager.instance.cityScript.GetCity();
                if (city != null)
                {
                    nameSet = city.country.nameSet;
                    if (nameSet == null) { Debug.LogErrorFormat("Invalid NameSet (Null) for {0} in {1}", city.name, city.country.name); }
                }
                else { Debug.LogError("Invalid City (Null)"); }
                int counter = 0;
                for (int i = 0; i < numOfContacts; i++)
                {
                    Contact contact = new Contact();
                    //initialise contact
                    contact.contactID = contactIDCounter++;
                    contact.job = string.Format("Job_{0}", contact.contactID);
                    contact.status = ContactStatus.ContactPool;
                    contact.actorID = -1;
                    contact.nodeID = -1;
                    contact.isTurned = false;
                    contact.turnStart = -1;
                    contact.turnFinish = -1;
                    contact.usefulIntel = 0;
                    //effectiveness (random 1 to 3, weighted normal)
                    contact.effectiveness = arrayOfEffectiveness[Random.Range(0, numEffectiveness)];
                    //name & sex
                    if (nameSet != null)
                    {
                        //50% chance of being either sex
                        if (Random.Range(0, 100) < 50)
                        {
                            contact.isMale = true;
                            contact.nameFirst = nameSet.firstMaleNames.GetRandomRecord();
                        }
                        else
                        {
                            contact.isMale = false;
                            contact.nameFirst = nameSet.firstFemaleNames.GetRandomRecord();
                        }
                        contact.nameLast = nameSet.lastNames.GetRandomRecord();
                    }
                    else { contact.nameFirst = "Unknown"; contact.nameLast = "Unknown"; contact.isMale = true; }
                    //add to dictionary
                    try
                    {
                        dictOfContacts.Add(contact.contactID, contact);
                        counter++;
                        //add to list
                        contactPool.Add(contact.contactID);
                    }
                    catch (ArgumentException)
                    { Debug.LogErrorFormat("Invalid entry in dictOfContacts for contact {0}, ID {1}", contact.nameFirst, contact.contactID); }
                }
                Debug.LogFormat("[Cont] ContactManager.cs -> CreateContacts: {0} out of {1} contacts created and added to pool", counter, numOfContacts);
                Debug.LogFormat("[Cont] ContactManager.cs -> CreateContacts: contactPool has {0} records", contactPool.Count);
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
                //assign type and job
                Node node = GameManager.instance.dataScript.GetNode(nodeID);
                Debug.Assert(node.Arc != null, string.Format("Invalid node.Arc for nodeID {0}", node.nodeID));
                if (node != null)
                {
                    ContactType contactType = node.Arc.GetRandomContactType();
                    if (contactType != null)
                    {
                        contact.type = contactType;
                        contact.job = contactType.pickList.GetRandomRecord();
                        if (contact.job == null) { Debug.LogErrorFormat("Invalid contactType job for {0}", contactType.pickList.name); }
                    }
                    else { Debug.LogErrorFormat("Invalid contactType (Null) for nodeID {0}, {1}", nodeID, node.Arc.name); }
                }
                else { Debug.LogErrorFormat("Invalid node (Null) for nodeID {0}", nodeID); }
                //traits affecting effectiveness (do at end of initialisation, eg. need type)
                Actor actor = GameManager.instance.dataScript.GetActor(actorID);
                if (actor != null)
                {
                    if (actor.CheckTraitEffect(actorContactEffectHigh) == true)
                    {
                        contact.effectiveness++;
                        GameManager.instance.actorScript.TraitLogMessage(actor, string.Format("for <b>{0}</b> contact, <b>{1}</b>", contact.type.name, contact.nameFirst), "to raise their Effectiveness +1");
                    }
                    if (actor.CheckTraitEffect(actorContactEffectLow) == true)
                    {
                        contact.effectiveness--;
                        GameManager.instance.actorScript.TraitLogMessage(actor, string.Format("for <b>{0}</b> contact, <b>{1}</b>", contact.type.name, contact.nameFirst), "to lower their Effectiveness -1");
                    }
                    //keep effectiveness with bounds
                    contact.effectiveness = Mathf.Clamp(contact.effectiveness, 1, 3);
                }
                else { Debug.LogErrorFormat("Invalid actor (Null) for actorID {0}", actorID); }
            }
            else
                { Debug.LogErrorFormat("Invalid contact (Null) for contactID {0}", contactID); }
            //check if pool requires topping up
            if (contactPool.Count <= numOfPoolThreshold)
            {
                //top up pool
                CreateContacts(numOfPoolTopUp);
                Debug.LogFormat("[Cont] ContactManager.cs -> AssignContact: ContactPool topped up, now has {0} records", contactPool.Count);
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
                                    Debug.LogWarningFormat("Contact {0}, {1}, ID {2} nodeID {3} not found in listOfNodes & deletec", contact.Value.nameFirst, contact.Value.job, contact.Value.contactID,
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
                                        Debug.LogFormat("[Cont] ContactManager.cs ->SetActorContact: ADDED {0}, {1}, actorID {2}, nodeID {3}, {4} {5}, ID {6}, E {7}, {8}", actor.actorName, actor.arc.name, 
                                            actor.actorID, listOfContactNodes[i], contact.type.name, contact.nameFirst, contact.contactID, contact.effectiveness, "\n");
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
                                    builder.AppendFormat(" Id {0}, {1} {2}, {3}, nodeID {4}, E {5}, {6}{7}", contact.Value.contactID, contact.Value.nameFirst, contact.Value.nameLast,
                                        contact.Value.job, contact.Value.nodeID, contact.Value.effectiveness, contact.Value.status, "\n");
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
                                    builder.AppendFormat(" Id {0}, {1} {2}, {3}, nodeID {4}, E {5}, {6}{7}", contact.Value.contactID, contact.Value.nameFirst, contact.Value.nameLast,
                                        contact.Value.job, contact.Value.nodeID, contact.Value.effectiveness, contact.Value.status, "\n");
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

    /// <summary>
    /// each turn updates arrays needed for contact activities
    /// </summary>
    private void InitialiseNetworkArrays()
    {
        //Need to set up actors and contacts ready for assignment
        arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
        //loop actors and create an int array containing tally of their contact network effectiveness, indexed to actorSlotID
        if (arrayOfActors != null)
        {
            for (int i = 0; i < arrayOfActors.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                {
                    Actor actor = arrayOfActors[i];
                    if (actor != null)
                    { arrayOfContactNetworks[i] = actor.GetContactNetworkEffectiveness(); }
                }
                else { arrayOfContactNetworks[i] = 0; }
            }
        }
        else { Debug.LogError("Invalid arrayOfActors (Null)"); }
    }

    /// <summary>
    /// Checks all active targets, per turn, for possible rumours. Called by TargetManager.cs -> StartTurnLate
    /// </summary>
    public void CheckTargetRumours()
    {
        int numOfTargets;
        int numOfRumours = 0;
        List<Target> listOfActiveTargets = GameManager.instance.dataScript.GetTargetPool(Status.Active);
        List<Target> listOfRumourTargets = new List<Target>();  //temp list to hold targets that have triggered a rumour
        if (listOfActiveTargets != null)
        {
            numOfTargets = listOfActiveTargets.Count;
            if (numOfTargets > 0)
            {
                //loop targets, check for a rumour
                for (int i = 0; i < numOfTargets; i++)
                {
                    Target target = listOfActiveTargets[i];
                    if (target != null)
                    {
                        if (Random.Range(0, 100) < rumourTarget)
                        { listOfRumourTargets.Add(target); }
                    }
                    else { Debug.LogWarningFormat("Invalid target (Null) for listOfActiveTargets[{0}]", i); }
                }
                numOfTargets = listOfRumourTargets.Count;
                Debug.LogFormat("[Cont] ContactManager.cs -> CheckTargetRumours: {0} rumours triggered for Active Targets this turn", numOfTargets);
                //have any rumours been triggered?
                if (numOfTargets > 0)
                {
                    //
                    // - - - loop targets and assign to actors and their contacts
                    //
                    int slotID;
                    for (int i = 0; i < numOfTargets; i++)
                    {
                        Target target = listOfRumourTargets[i];
                        //determine which actor's network of contacts heard the rumour (could be anyone, not worth checking for contact at specific node as player could game system with this knowledge)
                        slotID = CheckWhichContactNetwork(arrayOfContactNetworks);
                        if (slotID > -1)
                        {
                            Actor actor = arrayOfActors[slotID];
                            if (actor != null)
                            {
                                //get a random (weighted by effectiveness) contact from the actor's contact network
                                Contact contact = actor.GetRandomContact(target.listOfRumourContacts);
                                if (contact != null)
                                {
                                    numOfRumours++;
                                    //check max num of target rumours per turn not exceeded
                                    if (numOfRumours == maxRumoursTarget)
                                    {
                                        /*Debug.LogFormat("[Tst] ContactManager.cs -> CheckTargetRumour: MAXIMUM num of target rumours reach {0}", numOfRumours);*/
                                        break;
                                    }
                                    //add contact to target list
                                    target.AddContactRumour(contact.contactID);

                                    /*Debug.LogFormat("[Tst] ContactManager.cs -> CheckTargetRumour: {0} {1}, id {2} learns of target {3}, id {4}{5}", contact.nameFirst, contact.nameLast,
                                        contact.contactID, target.targetName, target.targetID, "\n");*/

                                    //correct node used? -> depends on contact effectiveness
                                    Node node = null;
                                    //city targets always have the correct node (would be silly to have an airport target not at the airport)
                                    if (target.targetType.name.Equals("City") == true)
                                    { node = GameManager.instance.dataScript.GetNode(target.nodeID); }
                                    else
                                    { node = GameManager.instance.dataScript.GetNode(target.nodeID); }
                                    //if valid node generate message
                                    if (node != null)
                                    {
                                        Node nodeTemp = node;
                                        //unreliable report, use a random neighbouring node
                                        if (CheckContactIsReliable(contact) == false)
                                        { nodeTemp = node.GetRandomNeighbour(); }
                                        if (nodeTemp != null)
                                        {
                                            string text = string.Format("Contact {0} {1}, {2} learns of rumour about target {3}", contact.nameFirst, contact.nameLast, contact.job, target.targetName);
                                            GameManager.instance.messageScript.ContactTargetRumour(text, actor, nodeTemp, contact, target);
                                        }
                                        else { Debug.LogWarningFormat("Invalid nodeTemp (Null) neighbouring node to {0}, {1}, id {2}", node.nodeName, node.Arc.name, node.nodeID); }
                                    }
                                    else { Debug.LogWarning("Invalid node (Null)"); }
                                }
                                else { Debug.LogFormat("No random contact (Null) for Actor {0}, {1}, id {2}{3}", actor.actorName, actor.arc.name, actor.actorID, "\n"); }
                            }
                            else { Debug.LogWarningFormat("Invalid actor (Null) for SlotID {0}", slotID); }
                        }
                        else { Debug.LogWarning("Invalid slotID (-1) for Target"); }
                    }
                }
            }
        }
        else { Debug.LogWarningFormat("Invalid listOfActiveTargets (Null) -> No target rumours generated"); }
    }

    /// <summary>
    /// Takes an array of each actors contact network (effectiveness tally) and randomly (weighted) returns the slotID of the chosen actor (their network of contacts has sourced the info), -1 if a problem
    /// </summary>
    /// <param name="arrayOfNetworks"></param>
    /// <returns></returns>
    private int CheckWhichContactNetwork(int[] arrayOfNetworks)
    {
        int slotID = -1;
        int totalEffectiveness = arrayOfNetworks.Sum();
        //Randomly (weighted) determine which actor's network of contacts found the info (loop array until a rolling tally of arraydata exceeds rndNum)
        int rndNum = Random.Range(0, totalEffectiveness);
        int tally = 0;
        for (int i = 0; i < arrayOfNetworks.Length; i++)
        {
            tally += arrayOfNetworks[i];
            if (tally - rndNum >= 0)
            {
                //found the correct slot
                slotID = i;
                break;
            }
        }
        return slotID;
    }

    /// <summary>
    /// Contacts have a % chance of telling the correct information based on their effectiveness. Returns true if contact is correct, false otherwise.
    /// NOTE: Contact checked for Null by calling method
    /// </summary>
    /// <param name="contact"></param>
    /// <returns></returns>
    public bool CheckContactIsReliable(Contact contact)
    {
        bool isCorrect = false;
        int rndNum = Random.Range(0, 100);
        switch(contact.effectiveness)
        {
            case 3: if (rndNum < confidenceHigh) { isCorrect = true; } break;
            case 2: if (rndNum < confidenceMed) { isCorrect = true; } break;
            case 1: if (rndNum < confidenceLow) { isCorrect = true; } break;
            default: Debug.LogWarningFormat("Invalid contact effectiveness {0}", contact.effectiveness); break;
        }
        //log
        if (isCorrect == false)
        {
            Debug.LogFormat("[Con] ContactManager.cs -> CheckContactIsReliable: Contact {0}, {1}, id {2}, Effect {3}, FAILED reliability test{4}", contact.nameFirst, contact.job, contact.contactID, 
                contact.effectiveness, "\n");
        }
        return isCorrect;
    }




    //new methods above here
}
