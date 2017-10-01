using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;

/// <summary>
/// handles Actor related data and methods
/// </summary>
public class ActorManager : MonoBehaviour
{

    [HideInInspector] public int numOfActorsCurrent;    //NOTE -> Not hooked up yet (need to show blank actors for any that aren't currently in use)
    public int numOfActorsTotal = 4;      //if you increase this then GUI elements and GUIManager will need to be changed to accomodate it, default value 4 

    private Actor[] arrayOfActors;

    /// <summary>
    /// Constructor
    /// </summary>
    private void Awake ()
    {

    }


    /// <summary>
    /// Set up number of required actors (minions supporting play)
    /// </summary>
    /// <param name="num"></param>
    public void InitialiseActors(int num)
    {
        //number of actors, default 4
        numOfActorsTotal = numOfActorsTotal == 4 ? numOfActorsTotal : 4;
        numOfActorsCurrent = numOfActorsCurrent < 1 ? 1 : numOfActorsCurrent;
        numOfActorsCurrent = numOfActorsCurrent > numOfActorsTotal ? numOfActorsTotal : numOfActorsCurrent;

        arrayOfActors = new Actor[numOfActorsTotal];
        if (num > 0)
        {
            //get a list of random actorArcs
            List<ActorArc> tempActorArcs = GameManager.instance.dataScript.GetRandomActorArcs(num);
            //Create actors
            for (int i = 0; i < num; i++)
            {
                Actor actor = new Actor()
                {
                    arc = tempActorArcs[i],
                    Name = tempActorArcs[i].actorName,
                    SlotID = i,
                    Connections = Random.Range(1, 4),
                    Motivation = Random.Range(1, 4),
                    Invisibility = Random.Range(1, 4),
                    trait = GameManager.instance.dataScript.GetRandomTrait()
                };
                arrayOfActors[i] = actor;

                Debug.Log("Actor added -> " + actor.arc.actorName + ", Ability " + actor.Connections + "\n");
            }                
            //add actions to dictionary
            GameManager.instance.dataScript.AddActions(arrayOfActors);
        }
        else { Debug.LogWarning("Invalid number of Actors (Zero, or less)"); }

    }

    /// <summary>
    /// Get array of actors
    /// </summary>
    /// <returns></returns>
    public Actor[] GetActors()
    { return arrayOfActors; }

    /// <summary>
    /// returns type of Actor, eg. 'Fixer', based on slotID (0 to 3)
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public string GetActorType(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        return arrayOfActors[slotID].arc.name;
    }

    /// <summary>
    /// returns array of Stats -> [0] Connections, [1] Motivation, [2] Invisibility
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public int[] GetActorStats(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        int[] arrayOfStats = new int[]{ arrayOfActors[slotID].Connections, arrayOfActors[slotID].Motivation, arrayOfActors[slotID].Invisibility};
        return arrayOfStats;
    }

    /// <summary>
    /// return a specific actor's name
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public string GetActorName(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        return arrayOfActors[slotID].arc.actorName;
    }

    /// <summary>
    /// return a specific actor's trait
    /// </summary>
    /// <param name="slotID"></param>
    /// <returns></returns>
    public string GetActorTrait(int slotID)
    {
        Debug.Assert(slotID > -1 && slotID < numOfActorsTotal, "Invalid slotID input");
        return arrayOfActors[slotID].trait.name;
    }

    /// <summary>
    /// Returns a list of all relevant Actor Actions for the  node to enable a ModalActionMenu to be put together (one button per action). Max 4 Actor + 1 Target actions.
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public List<EventButtonDetails> GetActorActions(int nodeID)
    {
        List<EventButtonDetails> tempList = new List<EventButtonDetails>();

        //Debug
        EventButtonDetails details1 = new EventButtonDetails()
        {
            buttonTitle = "Test Action",
            buttonTooltip = "Test tooltip for this very special button",
            action = GameManager.instance.actionMenuScript.CloseActionMenu
        };
        tempList.Add(details1);
        EventButtonDetails details2 = new EventButtonDetails()
        {
            buttonTitle = string.Format("Node ID {0}", nodeID),
            buttonTooltip = "Another test tooltip for this very special button",
            action = GameManager.instance.actionMenuScript.CloseActionMenu
        };
        tempList.Add(details2);


        return tempList;
    }

}
