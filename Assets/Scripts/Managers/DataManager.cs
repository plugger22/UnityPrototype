using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using gameAPI;
using Random = UnityEngine.Random;


/// <summary>
/// Data repositry class
/// </summary>
public class DataManager : MonoBehaviour
{
    //master lists 
    private List<ActorArc> listOfAllActorArcs = new List<ActorArc>();
    private List<Trait> listOfAllTraits = new List<Trait>();

    //for fast access
    private Effect[] arrayOfRenownEffects = new Effect[] { null, null, null, null }; //indexes correspond to enum 'RenownEffect'
    
    //node choices (random archetypes) based on number of connections. O.K to have multiple instances of the same archetype in a list in order to tweak the probabilities.
    public List<NodeArc> listOfOneConnArcs = new List<NodeArc>();
    public List<NodeArc> listOfTwoConnArcs = new List<NodeArc>();
    public List<NodeArc> listOfThreeConnArcs = new List<NodeArc>();
    public List<NodeArc> listOfFourConnArcs = new List<NodeArc>();
    public List<NodeArc> listOfFiveConnArcs = new List<NodeArc>();

    //dictionaries
    private Dictionary<int, GameObject> dictOfNodes = new Dictionary<int, GameObject>();            //Key -> nodeID, Value -> Node gameObject
    private Dictionary<int, NodeArc> dictOfNodeArcs = new Dictionary<int, NodeArc>();               //Key -> nodeArcID, Value -> NodeArc
    private Dictionary<int, ActorArc> dictOfActorArcs = new Dictionary<int, ActorArc>();            //Key -> actorArcID, Value -> ActorArc
    private Dictionary<int, Trait> dictOfTraits = new Dictionary<int, Trait>();                     //Key -> traitID, Value -> Trait
    private Dictionary<int, Action> dictOfActions = new Dictionary<int, Action>();                  //Key -> ActionID, Value -> Action
    private Dictionary<int, Effect> dictOfEffects = new Dictionary<int, Effect>();      //Key -> effectID, Value -> ActionEffect

    /// <summary>
    /// default constructor
    /// </summary>
    public void Initialise()
    {
        int counter = 0;
        int length;
        string path;
        //
        // - - - Node Arcs - - -
        //
        counter = 0;
        //get GUID of all SO Node Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var nodeArcGUID = AssetDatabase.FindAssets("t:NodeArc");
        foreach (var guid in nodeArcGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object nodeArcObject = AssetDatabase.LoadAssetAtPath(path, typeof(NodeArc));
            //assign a zero based unique ID number
            NodeArc nodeArc = nodeArcObject as NodeArc;
            nodeArc.NodeArcID = counter++;
            //generate a four letter (first 4 of name in CAPS) as a short form tag
            length = nodeArc.name.Length;
            length = length >= 4 ? 4 : length;
            nodeArc.NodeArcTag = nodeArc.name.Substring(0, length).ToUpper();
            //add to dictionary
            try
            { dictOfNodeArcs.Add(nodeArc.NodeArcID, nodeArc); }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid NodeArc (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid NodeArc (duplicate) ID \"{0}\" for  \"{1}\"", counter, nodeArc.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfNodeArcs has {0} entries{1}", counter, "\n"));

        //
        // - - - Traits - - -
        //
        counter = 0;
        //get GUID of all SO Trait Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var traitGUID = AssetDatabase.FindAssets("t:Trait");
        foreach (var guid in traitGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object traitObject = AssetDatabase.LoadAssetAtPath(path, typeof(Trait));
            //assign a zero based unique ID number
            Trait trait = traitObject as Trait;
            trait.TraitID = counter++;
            //add to dictionary
            try
            {
                dictOfTraits.Add(trait.TraitID, trait);
                //add to list
                listOfAllTraits.Add(trait);
            }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Trait (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid Trait (duplicate) ID \"{0}\" for \"{1}\"", counter, trait.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfTraits has {0} entries{1}", counter, "\n"));
        //
        // - - - Actor Arcs - - 
        //
        counter = 0;
        //get GUID of all SO ActorArc Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var arcGUID = AssetDatabase.FindAssets("t:ActorArc");
        foreach (var guid in arcGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object arcObject = AssetDatabase.LoadAssetAtPath(path, typeof(ActorArc));
            //assign a zero based unique ID number
            ActorArc arc = arcObject as ActorArc;
            arc.ActorArcID = counter++;
            //generate a four letter (first 4 of name in CAPS) as a short form tag
            length = arc.actorName.Length;
            length = length >= 4 ? 4 : length;
            arc.ActorTag = arc.actorName.Substring(0, length).ToUpper();
            //add to dictionary
            try
            {
                dictOfActorArcs.Add(arc.ActorArcID, arc);
                //add to list
                listOfAllActorArcs.Add(arc);

            }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Actor Arc (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid actorArc (duplicate) ID \"{0}\" for \"{1}\"", counter, arc.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfActorArcs has {0} entries{1}", counter, "\n"));
        //
        // - - - Action Effects - - -
        //
        counter = 0;
        //get GUID of all SO ActionEffect Objects -> Note that I'm searching the entire database here so it's not folder dependant
        var effectsGUID = AssetDatabase.FindAssets("t:Effect");
        foreach (var guid in effectsGUID)
        {
            //get path
            path = AssetDatabase.GUIDToAssetPath(guid);
            //get SO
            UnityEngine.Object effectObject = AssetDatabase.LoadAssetAtPath(path, typeof(Effect));
            //assign a zero based Unique ID number
            Effect effect = effectObject as Effect;
            effect.EffectID = counter++;
            //add to dictionary
            try
            {
                dictOfEffects.Add(effect.EffectID, effect);
                //identify special Renown Effects for fast access (need 4, Player +/- & Actor +/-). Store Effects in arrayOfRenownEffects. Enum RenownEffect.
                switch(effect.effectOutcome)
                {
                    case EffectOutcome.ActorRenown:
                        switch(effect.effectResult)
                        {
                            case Result.Add:
                                arrayOfRenownEffects[(int)RenownEffect.ActorRaise] = effect;
                                break;
                            case Result.Subtract:
                                arrayOfRenownEffects[(int)RenownEffect.ActorLower] = effect;
                                break;
                        }
                        break;
                    case EffectOutcome.PlayerRenown:
                        switch (effect.effectResult)
                        {
                            case Result.Add:
                                arrayOfRenownEffects[(int)RenownEffect.PlayerRaise] = effect;
                                break;
                            case Result.Subtract:
                                arrayOfRenownEffects[(int)RenownEffect.PlayerLower] = effect;
                                break;
                        }
                        break;
                }

            }
            catch (ArgumentNullException)
            { Debug.LogError("Invalid Action Effect (Null)"); }
            catch (ArgumentException)
            { Debug.LogError(string.Format("Invalid ActionEffect (duplicate) effectID \"{0}\" for \"{1}\"", counter, effect.name)); }
        }
        Debug.Log(string.Format("DataManager: Initialise -> dictOfEffects (ActionEffects) has {0} entries{1}", counter, "\n"));
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
        foreach (Actor actor in arrayOfActors)
        {
            //add nodeAction
            if (actor.arc.nodeAction != null)
            {
                //assign dynamic ID
                actor.arc.nodeAction.ActionID = counter++;
                //add to dictionary (only adding actions that are present in the level with the current selection of Actors)
                try
                { dictOfActions.Add(actor.arc.nodeAction.ActionID, actor.arc.nodeAction); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Action Arc (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid (duplicate) ActionID \"{0}\" for Action \"{1}\"", actor.arc.nodeAction.ActionID, actor.arc.nodeAction.name)); }
            }
            //add webAction
            if (actor.arc.webAction != null)
            {
                //assign dynamic ID
                actor.arc.webAction.ActionID = counter++;
                //add to dictionary (only adding actions that are present in the level with the current selection of Actors)
                try
                { dictOfActions.Add(actor.arc.webAction.ActionID, actor.arc.webAction); }
                catch (ArgumentNullException)
                { Debug.LogError("Invalid Action Arc (Null)"); }
                catch (ArgumentException)
                { Debug.LogError(string.Format("Invalid (duplicate) ActionID \"{0}\" for Action \"{1}\"", actor.arc.webAction.ActionID, actor.arc.webAction.name)); }
            }
        }
    }


    /// <summary>
    /// returns a GameObject node from dictionary based on nodeID, Null if not found
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public GameObject GetNodeObject(int nodeID)
    {
        GameObject obj = null;
        if (dictOfNodes.TryGetValue(nodeID, out obj))
        {
            return obj;
        }
        return null;
    }


    public void AddNodeObject(int nodeID, GameObject nodeObj)
    {
        try
        { dictOfNodes.Add(nodeID, nodeObj); }
        catch (ArgumentNullException)
        { Debug.LogError("Invalid Node Object (Null)"); }
        catch (ArgumentException)
        { Debug.LogError(string.Format("Invalid (duplicate) nodeID \"{0}\" for Node \"{1}\"", nodeID, nodeObj.name)); }
    }

    /// <summary>
    /// returns one of four specific Renown Effects (select 
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public Effect GetRenownEffect(RenownEffect effect)
    {
        return arrayOfRenownEffects[(int)effect];
    }


}


