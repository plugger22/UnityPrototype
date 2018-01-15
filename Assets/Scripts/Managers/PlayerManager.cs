using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using System.Text;

/// <summary>
/// handles all player related data and methods
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public int rebelCauseMax;                         //level of Rebel Support. Max out to Win the level. Max level is a big part of difficulty.
    [HideInInspector] public int rebelCauseCurrent;                    //current level of Rebel Support
    [HideInInspector] public int renown;
    [HideInInspector] public int numOfRecruits;
    [HideInInspector] public int invisibility;

    [Range(1,3)] public int renownCostGear = 1;

    private List<int> listOfGear = new List<int>();                 //gearID's of all gear items in inventory



    public void Initialise()
    {
        int nodeID = 0;
        //place Player in a random start location (Sprawl node)
        int nodeArcID = GameManager.instance.dataScript.GetNodeArcID("Sprawl");
        Node node = GameManager.instance.dataScript.GetRandomNode(nodeArcID);
        if (node != null)
        {
            nodeID = node.nodeID;
            //initialise move list
            node.SetMoveNodes();
        }
        else
        { Debug.LogWarning("PlayerManager: Invalid node (Null). Player placed in node '0' by default"); }
        //set player node
        GameManager.instance.nodeScript.nodePlayer = nodeID;
        //set stats
        renown = 0;
        invisibility = 3;
        numOfRecruits = GameManager.instance.actorScript.numOfOnMapActors;
        rebelCauseMax = 10;
        rebelCauseCurrent = 0;
        //message
        string text = string.Format("Player commences at \"{0}\", {1}, ID {2}", node.Name, node.Arc.name.ToUpper(), node.nodeID);
        Message message = GameManager.instance.messageScript.PlayerMove(text, nodeID);
        if (message != null) { GameManager.instance.dataScript.AddMessageNew(message); }
    }

    //
    /// - - - Gear - - -
    /// 

    /// <summary>
    /// returns true if GearID present in player's inventory
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public bool CheckGearPresent(int gearID)
    { return listOfGear.Exists(id => id == gearID); }

    /// <summary>
    /// returns gearID of best piece of gear that matches type, '-1' if none
    /// </summary>
    /// <param name="gearType"></param>
    /// <returns></returns>
    public int CheckGearTypePresent(GearType gearType)
    {
        int gearID = -1;
        int rarity = -1;
        if (listOfGear.Count > 0)
        {
            //loop through looking for best piece of gear that matches the type
            for (int i = 0; i < listOfGear.Count; i++)
            {
                Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                if (gear != null)
                {
                    if (gear.type == gearType)
                    {
                        //is it a better piece of gear (higher rarity) than already found?
                        if ((int)gear.rarity > rarity)
                        {
                            rarity = (int)gear.rarity;
                            gearID = gear.gearID;
                        }
                    }
                }
                else
                { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
            }
            return gearID;
        }
        else { return -1; }
    }

    /// <summary>
    /// returns the amount of gear the player has in their inventory
    /// </summary>
    /// <returns></returns>
    public int GetNumOfGear()
    { return listOfGear.Count; }


    public List<int> GetListOfGear()
    { return listOfGear; }

    /// <summary>
    /// add gear to player's inventory. Checks for duplicates and null Gear. Returns true if successful.
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public bool AddGear(int gearID)
    {
        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
        if (gear != null)
        {
            //check gear not already in inventory
            if (CheckGearPresent(gearID) == false)
            {
                listOfGear.Add(gearID);
                Debug.Log(string.Format("PlayerManager: Gear \"{0}\", gearID {1}, added to inventory{2}", gear.name, gearID, "\n"));
                return true;
            }
            else
            {
                Debug.LogError(string.Format("Gear |'{0}\", gearID {1} is already present in inventory", gear.name, gearID));
                return false;
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", gearID));
            return false;
        }
    }

    /// <summary>
    /// remove gear from player's inventory
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public bool RemoveGear(int gearID)
    {
        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
        if (gear != null)
        {
            //check gear not already in inventory
            if (CheckGearPresent(gearID) == true)
            {
                listOfGear.Remove(gearID);
                Debug.Log(string.Format("PlayerManager: Gear \"{0}\", gearID {1}, removed from inventory{2}", gear.name, gearID, "\n"));
                return true;
            }
            else
            {
                Debug.LogError(string.Format("Gear \"{0}\", gearID {1} NOT removed from inventory", gear.name, gearID));
                return false;
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", gearID));
            return false;
        }
    }

    //
    // - - - Other
    //

    /// <summary>
    /// Debug function to display all player related stats
    /// </summary>
    /// <returns></returns>
    public string DisplayPlayerStats()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(string.Format(" Player Stats{0}{1}", "\n", "\n"));
        builder.Append(string.Format(" Invisibility {0}{1}", invisibility, "\n"));
        builder.Append(string.Format(" Renown {0}{1}", renown, "\n"));
        builder.Append(string.Format(" NumOfRecruits {0}{1}{2}", numOfRecruits, "\n", "\n"));
        builder.Append(string.Format(" Resistance Cause  {0} of {1}", rebelCauseCurrent, rebelCauseMax));
        builder.Append(string.Format("{0}{1} Gear{2}", "\n", "\n", "\n"));
        if (listOfGear.Count > 0)
        {
            for (int i = 0; i < listOfGear.Count; i++)
            {
                Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                if (gear != null)
                { builder.Append(string.Format(" {0}, ID {1}{2}", gear.name, gear.gearID, "\n"));}
            }
        }
        else { builder.Append("No gear in inventory"); }
        return builder.ToString();
    }


    /// <summary>
    /// DEBUG method to show players gear in lieu of a working UI element
    /// </summary>
    /// <returns></returns>
    public string DisplayGear()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(" Player's Gear");
        builder.AppendLine();
        foreach (int gearID in listOfGear)
        {
            Gear gear = GameManager.instance.dataScript.GetGear(gearID);
            if (gear != null)
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.Append(string.Format(" {0}", gear.name.ToUpper()));
                builder.AppendLine();
                builder.Append(string.Format(" Metalevel \"{0}\"", gear.metaLevel));
                builder.AppendLine();
                builder.Append(string.Format(" gearID {0}", gear.gearID));
                builder.AppendLine();
                builder.Append(string.Format(" rarity \"{0}\"", gear.rarity));
                builder.AppendLine();
                builder.Append(string.Format(" gearType \"{0}\"", gear.type));
                builder.AppendLine();
                builder.Append(string.Format(" data {0}", gear.data));
            }
        }
        return builder.ToString();
    }

    //place new methods above here
}
