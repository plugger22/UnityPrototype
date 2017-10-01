using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using Random = UnityEngine.Random;


/// <summary>
/// Data repositry class
/// </summary>
public class DataManager : MonoBehaviour
{
    //master lists 
    public List<ActorArc> listOfAllActorArcs = new List<ActorArc>();
    public List<NodeArc> listOfAllNodeArcs = new List<NodeArc>();
    public List<Trait> listOfAllTraits = new List<Trait>();
    
    //node choices (random archetypes) based on number of connections. O.K to have multiple instances of the same archetype in a list in order to tweak the probabilities.
    public List<NodeArc> listOfOneConnArcs = new List<NodeArc>();
    public List<NodeArc> listOfTwoConnArcs = new List<NodeArc>();
    public List<NodeArc> listOfThreeConnArcs = new List<NodeArc>();
    public List<NodeArc> listOfFourConnArcs = new List<NodeArc>();
    public List<NodeArc> listOfFiveConnArcs = new List<NodeArc>();
    
    //dictionaries
    private Dictionary<int, NodeArc> dictOfNodeArcs = new Dictionary<int, NodeArc>();               //Key -> nodeArcID, Value -> NodeArc
    private Dictionary<int, ActorArc> dictOfActorArcs = new Dictionary<int, ActorArc>();            //Key -> actorArcID, Value -> ActorArc
    private Dictionary<int, Trait> dictOfTraits = new Dictionary<int, Trait>();                     //Key -> traitID, Value -> Trait
    private Dictionary<int, Action> dictOfActions = new Dictionary<int, Action>();                  //Key -> ActionID, Value -> Action
    //private Dictionary<int, ActionEffect> dictOfEffects = new Dictionary<int, ActionEffect>();      //Key -> effectID, Value -> ActionEffect

    /// <summary>
    /// default constructor
    /// </summary>
    public void Initialise()
    {
        int counter = 0;
        int length;
        //Node Arcs
        foreach(NodeArc arc in listOfAllNodeArcs)
        {
            //assign a unique zero based number & get first four letters of name, in CAPS, as the tag
            arc.NodeArcID = counter++;
            length = arc.name.Length;
            length = length >= 4 ? 4 : length;
            arc.NodeTag = arc.name.Substring(0, length).ToUpper();
            
            /*Debug.Log(string.Format("Node {0}, nodeArcID {1}, nodeTag {2}{3}", arc.name, arc.nodeArcID, arc.nodeTag, "\n"));*/

            //add to dictionary
            try
            { dictOfNodeArcs.Add(arc.NodeArcID, arc); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Node Arc (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid (duplicate) nodeArcID \"{0}\" for Node \"{1}\"", arc.NodeArcID, arc.name)); }
        }
        Debug.Log("Node Arcs total -> " + listOfAllNodeArcs.Count + "\n");
        GameManager.instance.nodeScript.numOfNodeArcs = counter;
        //Traits
        counter = 0;
        foreach(Trait trait in listOfAllTraits)
        {
            trait.TraitID = counter++;
        }
        counter = 0;
        //Actor Arcs
        foreach (ActorArc arc in listOfAllActorArcs)
        {
            //assign a zero based unique number & get first four letters of name, in CAPS, as the tag
            arc.actorArcID = counter++;
            length = arc.actorName.Length;
            length = length >= 4 ? 4 : length;
            arc.actorTag = arc.actorName.Substring(0, length).ToUpper();

            /*Debug.Log(string.Format("Actor {0}, actorArcID {1}, actorTag {2}{3}", arc.name, arc.actorArcID, arc.actorTag, "\n"));*/

            //add to dictionary
            try
            { dictOfActorArcs.Add(arc.actorArcID, arc); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Actor Arc (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid (duplicate) actorArcID \"{0}\" for Actor \"{1}\"", arc.actorArcID, arc.actorName)); }
        }
        Debug.Log("Actor Arcs total -> " + listOfAllActorArcs.Count + "\n");
    }


    /// <summary>
    /// returns a random Arc from the appropriate list based on the number of Connections that the node has
    /// </summary>
    /// <param name="numConnections"></param>
    /// <returns></returns>
    public NodeArc GetRandomNodeArc(int numConnections)
    {
        NodeArc tempArc = null;
        List<NodeArc> tempList = null;
        switch(numConnections)
        {
            case 1:
                tempList = listOfOneConnArcs;
                break;
            case 2:
                tempList = listOfTwoConnArcs;
                break;
            case 3:
                tempList = listOfThreeConnArcs;
                break;
            case 4:
                tempList = listOfFourConnArcs;
                break;
            case 5:
            case 6:
            case 7:
            case 8:
                tempList = listOfFiveConnArcs;
                break;
            default:
                Debug.LogError("Invalid number of Connections " + numConnections);
                break;
        }
        tempArc = tempList[Random.Range(0, tempList.Count)];
        return tempArc;
    }

    /// <summary>
    /// Get number of NodeArcs in dictionary
    /// </summary>
    /// <returns></returns>
    public int GetNumNodeArcs()
    { return dictOfNodeArcs.Count; }

    /// <summary>
    /// returns NodeArc based on ID search of dict, Null if not found
    /// </summary>
    /// <param name="nodeArcID"></param>
    /// <returns></returns>
    public NodeArc GetNodeArc(int nodeArcID)
    {
        if (dictOfNodeArcs.ContainsKey(nodeArcID))
        { return dictOfNodeArcs[nodeArcID]; }
        else { Debug.LogWarning("Not found in Dict > nodeArcID " + nodeArcID);}
        return null;
    }

    /// <summary>
    /// returns a number of randomly selected ActorArcs
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public List<ActorArc> GetRandomActorArcs(int num)
    {
        //create a separate copy of ActorArcs list to use for randomly selection
        List<ActorArc> tempMaster = new List<ActorArc>(listOfAllActorArcs);
        //temp list for results
        List<ActorArc> tempList = new List<ActorArc>();
        //randomly select
        int index;
        for(int i = 0; i < num; i++)
        {
            index = Random.Range(0, tempMaster.Count);
            tempList.Add(tempMaster[index]);
            //remove from list to prevent being selected again
            tempMaster.RemoveAt(index);
        }
        return tempList;
    }


    /// <summary>
    /// return a random trait (could be good or bad)
    /// </summary>
    /// <returns></returns>
    public Trait GetRandomTrait()
    {
        return listOfAllTraits[Random.Range(0, listOfAllTraits.Count)];
    }

    /// <summary>
    /// add Actors and effects to dictionaries
    /// </summary>
    /// <param name="arrayOfActors"></param>
    public void AddActions(Actor[] arrayOfActors)
    {
        int counter = 0;
        List<Action> listOfActions = new List<Action>();
        foreach(Actor actor in arrayOfActors)
        {
            listOfActions.Clear();
            listOfActions.AddRange(actor.arc.listOfActions);
            //loop actions
            foreach(Action action in listOfActions)
            {
                //assign dynamic ID
                action.ActionID = counter++;
                //add to dictionary (only adding actions that are present in the level with the current selection of Actors)
                try
                { dictOfActions.Add(action.ActionID, action); Debug.Log(string.Format("Action Added -> {0}, ID {1}{2}", action.name, action.ActionID, "\n")); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Action Arc (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid (duplicate) ActionID \"{0}\" for Action \"{1}\"", action.ActionID, action.name)); }
            }
        }
    }
}


