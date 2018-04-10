using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;
using System.Text;

/// <summary>
/// handles all player related data and methods
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public Sprite sprite;

    [HideInInspector] public int numOfRecruits;
    [HideInInspector] public int invisibility;
    [HideInInspector] public int actorID = 999;
    [HideInInspector] public ActorStatus status;
    [HideInInspector] public ActorTooltip tooltipStatus;    //Actor sprite shows a relevant tooltip if tooltipStatus > None (Breakdown, etc)
    [HideInInspector] public ActorInactive inactiveStatus;  //reason actor is inactive
    [HideInInspector] public bool isBreakdown;              //enforces a minimum one turn gap between successive breakdowns

    private List<int> listOfGear = new List<int>();                     //gearID's of all gear items in inventory
    private List<Condition> listOfConditions = new List<Condition>();   //list of all conditions currently affecting the actor

    //private backing fields, need to track separately to handle AI playing both sides
    private int _renownResistance;
    private int _renownAuthority;

    //for fast access
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;

    //Note: There is no ActorStatus for the player as the 'ResistanceState' handles this -> EDIT: Nope, status does

    public int Renown
    {
        get
        {
            if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level) { return _renownResistance; }
            else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level) { return _renownAuthority; }
            else
            {
                //AI control of both side
                if (GameManager.instance.turnScript.currentSide.level == globalResistance.level) { return _renownResistance; }
                else {return _renownAuthority; }
            }
        }
        set
        {
            if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level) { _renownResistance = value; }
            else if (GameManager.instance.sideScript.PlayerSide.level == globalAuthority.level) { _renownAuthority = value; }
            else
            {
                //AI control of both side
                if (GameManager.instance.turnScript.currentSide.level == globalResistance.level) { _renownResistance = value; }
                else { _renownAuthority = value; }
            }
        }
    }



    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        int nodeID = 0;
        //place Player in a random start location (Sprawl node)
        int nodeArcID = GameManager.instance.dataScript.GetNodeArcID("SPRAWL");
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
        //fast acess fields (BEFORE set stats below)
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        //set stats
        Renown = 0;
        invisibility = 3;
        numOfRecruits = GameManager.instance.actorScript.maxNumOfOnMapActors;

        //message
        string text = string.Format("Player commences at \"{0}\", {1}, ID {2}", node.nodeName, node.Arc.name, node.nodeID);
        Message message = GameManager.instance.messageScript.PlayerMove(text, nodeID);
        if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
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
                    if (gear.type.name.Equals(gearType.name) == true)
                    {
                        //is it a better piece of gear (higher rarity) than already found?
                        if (gear.rarity.level > rarity)
                        {
                            rarity = gear.rarity.level;
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
    public int CheckNumOfGear()
    { return listOfGear.Count; }


    public List<int> GetListOfGear()
    { return listOfGear; }

    /// <summary>
    /// returns a list of all gear that matches the specified type, null if none
    /// </summary>
    /// <param name="gearType"></param>
    /// <returns></returns>
    public List<Gear> GetListOfGearType(GearType gearType)
    {
        Debug.Assert(gearType != null, "Invalid gearType (Null)");
        List<Gear> listOfSpecificGear = null;
        if (listOfGear.Count > 0)
        {
            //loop through looking for best piece of gear that matches the type
            for (int i = 0; i < listOfGear.Count; i++)
            {
                Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                if (gear != null)
                {
                    if (gear.type.name.Equals(gearType.name) == true)
                    {
                        //initialise list if haven't already done so
                        if (listOfSpecificGear == null)
                        { listOfSpecificGear = new List<Gear>(); }
                        //add gear to list
                        listOfSpecificGear.Add(gear);
                    }
                }
            }
        }
        return listOfSpecificGear;
    }

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
                Debug.LogWarning(string.Format("Gear |'{0}\", gearID {1} is already present in inventory", gear.name, gearID));
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
    // - - - Conditions - - -
    //

    /// <summary>
    /// Add a new condition to list provided it isn't already present, ignored if it is
    /// </summary>
    /// <param name="condition"></param>
    public void AddCondition(Condition condition)
    {
        if (condition != null)
        {
            //check that condition isn't already present
            if (CheckConditionPresent(condition) == false)
            {
                listOfConditions.Add(condition);
                //message
                string msgText = string.Format("Player gains condition \"{0}\"", condition.name);
                Message message = GameManager.instance.messageScript.ActorCondition(msgText, actorID, GameManager.instance.sideScript.PlayerSide);
                GameManager.instance.dataScript.AddMessage(message);
            }
        }
        else { Debug.LogError("Invalid condition (Null)"); }
    }

    /// <summary>
    /// Checks if actor has a specified condition 
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public bool CheckConditionPresent(Condition condition)
    {
        if (condition != null)
        {
            if (listOfConditions.Count > 0)
            {
                foreach (Condition checkCondition in listOfConditions)
                {
                    if (checkCondition.name.Equals(condition.name) == true)
                    { return true; }
                }
            }
        }
        else { Debug.LogError("Invalid condition (Null)"); }
        return false;
    }

    /// <summary>
    /// Removes a specified condition if present, ignored if it isn't
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public bool RemoveCondition(Condition condition)
    {
        if (condition != null)
        {
            if (listOfConditions.Count > 0)
            {
                //reverse loop -> delete and return if found
                for (int i = listOfConditions.Count - 1; i >= 0; i--)
                {
                    if (listOfConditions[i].name.Equals(condition.name) == true)
                    {
                        listOfConditions.RemoveAt(i);
                        //message
                        string msgText = string.Format("Player condition \"{0}\" removed", condition.name);
                        Message message = GameManager.instance.messageScript.ActorCondition(msgText, actorID, GameManager.instance.sideScript.PlayerSide);
                        GameManager.instance.dataScript.AddMessage(message);
                        return true;
                    }
                }
            }
        }
        else { Debug.LogError("Invalid condition (Null)"); }
        return false;
    }

    public List<Condition> GetListOfConditions()
    { return listOfConditions; }

    public int CheckNumOfConditions()
    { return listOfConditions.Count; }

    //
    // - - - Debug
    //

    /// <summary>
    /// Debug function to display all player related stats
    /// </summary>
    /// <returns></returns>
    public string DisplayPlayerStats()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(string.Format(" Player Stats{0}{1}", "\n", "\n"));
        builder.Append(string.Format("- Stats{0}", "\n"));
        if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
        { builder.Append(string.Format(" Invisibility {0}{1}", invisibility, "\n")); }
        builder.Append(string.Format(" Renown {0}{1}", Renown, "\n"));
        builder.Append(string.Format("{0}- Conditions{1}", "\n", "\n"));
        if (listOfConditions.Count > 0)
        {
            for (int i = 0; i < listOfConditions.Count; i++)
            { builder.Append(string.Format(" {0}{1}", listOfConditions[i].name, "\n")); }
        }
        builder.Append(string.Format("{0}- States{1}", "\n", "\n"));
        builder.Append(string.Format(" Player {0}{1}", status, "\n"));
        builder.Append(string.Format("{0}- Global{1}", "\n", "\n"));
        builder.Append(string.Format(" Resistance Cause  {0} of {1}{2}", GameManager.instance.rebelScript.resistanceCause,
            GameManager.instance.rebelScript.resistanceCauseMax, "\n"));
        builder.Append(string.Format(" resistanceState {0}{1}", GameManager.instance.turnScript.resistanceState, "\n"));
        builder.Append(string.Format(" authorityState {0}{1}", GameManager.instance.turnScript.authorityState, "\n"));
        builder.Append(string.Format("{0}- Reserve Pool{1}", "\n", "\n"));
        builder.Append(string.Format(" NumOfRecruits {0} + {1}{2}", numOfRecruits, GameManager.instance.dataScript.CheckNumOfActorsInReserve(), "\n"));
        if (GameManager.instance.sideScript.PlayerSide.level == globalResistance.level)
        {
            builder.Append(string.Format("{0}- Gear{1}", "\n", "\n"));
            if (listOfGear.Count > 0)
            {
                for (int i = 0; i < listOfGear.Count; i++)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                    if (gear != null)
                    { builder.Append(string.Format(" {0}, ID {1}, {2}{3}", gear.name, gear.gearID, gear.type.name, "\n")); }
                }
            }
            else { builder.Append(" No gear in inventory"); }
        }
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
                if (gear.metaLevel != null)
                { builder.Append(string.Format(" Metalevel \"{0}\"", gear.metaLevel.name)); }
                else { builder.Append(" MetaLevel \"None\""); }
                builder.AppendLine();
                builder.Append(string.Format(" gearID {0}", gear.gearID));
                builder.AppendLine();
                builder.Append(string.Format(" rarity \"{0}\"", gear.rarity.name));
                builder.AppendLine();
                builder.Append(string.Format(" gearType \"{0}\"", gear.type.name));
                builder.AppendLine();
                builder.Append(string.Format(" data {0}", gear.data));
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Debug function to manually add gear to the player's inventory.Returns a message to display
    /// </summary>
    /// <param name="gearname"></param>
    public string DebugAddGear(string gearName)
    {
        string text = string.Format("{0} has NOT been added to the Player's inventory{1}Press ESC to exit", gearName, "\n");
        int gearID = -1;
        if (string.IsNullOrEmpty(gearName) == false)
        {
            //find gear in dictionary
            Dictionary<int, Gear> dictOfGear = GameManager.instance.dataScript.GetAllGear();
            if (dictOfGear != null)
            {
                //loop dictionary looking for gear
                foreach(var gear in dictOfGear)
                {
                    if (gear.Value.name.Equals(gearName) == true)
                    {
                        gearID = gear.Value.gearID;
                        break;
                    }
                }
                if (gearID > -1)
                {
                    //add gear to player's inventory
                    if (AddGear(gearID) == true)
                    {
                        text = string.Format("{0} has been added to the Player's inventory{1}Press ESC to exit", gearName, "\n");
                        //message
                        Message message = GameManager.instance.messageScript.GearObtained(string.Format("{0} added (DEBUG)", gearName), GameManager.instance.nodeScript.nodePlayer, gearID);
                        GameManager.instance.dataScript.AddMessage(message);
                    }
                }
            }
        }
        return text;
    }

    //place new methods above here
}
