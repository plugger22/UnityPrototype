using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// handles all team related matters
/// </summary>
public class TeamManager : MonoBehaviour
{
    [Range(1, 4)]
    [Tooltip("The maximum number of teams that may be present at a node at any one time")]
    public int maxTeamsAtNode = 3;              

    /// <summary>
    /// Set up at start
    /// </summary>
    public void Initialise()
    {
        InitialiseTeams();
    }

    /// <summary>
    /// PlaceHolder -> place a random number of security teams on the map for testing purposes
    /// </summary>
    public void InitialiseTeams()
    {
        //loop all nodes, 40% chance of getting a security team
        List<Node> listOfNodes = new List<Node>(GameManager.instance.dataScript.GetAllNodes().Values);
        foreach(Node node in listOfNodes)
        {
            if (Random.Range(0, 100) < 40)
            {
                //create a new security team
                Team team = new Team("Security");
                if (team != null)
                {
                    node.AddTeam(team);
                }
            }
        }
    }

}
