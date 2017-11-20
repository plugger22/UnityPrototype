using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// handles all player related data and methods
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public int RebelCauseMax { get; set; }                         //level of Rebel Support. Max out to Win the level. Max level is a big part of difficulty.
    public int RebelCauseCurrent { get; set; }                      //current level of Rebel Support

    public int Renown { get; set; }

    public int NumOfRecruits { get; set; }
    public int NumOfGear { get; set; }

    private List<int> listOfGear = new List<int>();                 //gearID's of all gear items in inventory



    public void Initialise()
    {
        int nodeID = 0;
        //place Player in a random start location (Sprawl node)
        int nodeArcID = GameManager.instance.dataScript.GetNodeArcID("Sprawl");
        Node node = GameManager.instance.dataScript.GetRandomNode(nodeArcID);
        if (node != null)
        { nodeID = node.NodeID; }
        else
        { Debug.LogError("Invalid node (Null). Player placed in node '0' by default"); }
        //set player node
        GameManager.instance.nodeScript.nodePlayer = nodeID;
    }

    /// <summary>
    /// returns true if GearID present in player's inventory
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public bool CheckGearPresent(int gearID)
    { return listOfGear.Exists(id => id == gearID); }

    /// <summary>
    /// returns the amount of gear the player has in their inventory
    /// </summary>
    /// <returns></returns>
    public int GetNumOfGear()
    { return listOfGear.Count; }
}
