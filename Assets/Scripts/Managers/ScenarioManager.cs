using gameAPI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Holds current scenario
/// </summary>
public class ScenarioManager : MonoBehaviour
{
    //Auto-assigned from CampaignManager.cs OR TutorialManager.cs
    [HideInInspector] public Scenario scenario;                                 //current scenario in use



    /// <summary>
    /// Debug display of current scenario
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayScenarioData()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- ScenarioData{0}{1}", "\n", "\n");
        builder.AppendFormat(" Scenario: \"{0}\", {1}{2}", scenario.tag, scenario.descriptor, "\n");
        builder.AppendFormat(" Side: {0}{1}", scenario.side.name, "\n");
        builder.AppendFormat(" City: {0}{1}", scenario.city.tag, "\n");
        builder.AppendFormat(" Seed: {0}{1}", scenario.seedCity, "\n");
        builder.AppendFormat(" Leader Resistance: {0}{1}", scenario.leaderResistance.tag, "\n");
        builder.AppendFormat(" Leader Authority: {0}{1}", scenario.leaderAuthority.mayorName, "\n");
        builder.AppendFormat(" RebelHQ Approval: {0}{1}", scenario.approvalStartRebelHQ, "\n");
        builder.AppendFormat(" AuthorityHQ Approval: {0}{1}", scenario.approvalStartAuthorityHQ, "\n");
        builder.AppendFormat(" City Start Loyalty: {0}{1}", scenario.cityStartLoyalty, "\n");
        builder.AppendFormat(" Mission Resistance: {0}{1}", scenario.missionResistance.name, "\n");
        builder.AppendFormat(" Challenge Resistance: {0}{1}", scenario.challengeResistance.name, "\n");
        builder.AppendFormat(" Number of Turns: {0}{1}", scenario.timer, "\n");

        Mission mission = scenario.missionResistance;
        builder.AppendFormat("{0}-Mission Resistance \"{1}\"{2}", "\n", mission.descriptor, "\n");
        /*foreach(Objective objective in mission.listOfObjectives)
        { builder.AppendFormat(" Objective: {0}{1}", objective.tag, "\n"); }*/
        mission.listOfObjectives.ForEach(objective => builder.AppendFormat(" Objective: {0}{1}", objective.tag, "\n"));
        builder.AppendFormat(" Generic Targets: Live {0}, Active {1}{2}", mission.targetsGenericLive, mission.targetsGenericActive, "\n");
        if (mission.targetBaseCityHall != null)
        { builder.AppendFormat(" Target City Hall: {0}{1}", mission.targetBaseCityHall.targetName, "\n"); }
        if (mission.targetBaseIcon != null)
        { builder.AppendFormat(" Target Icon: {0}{1}", mission.targetBaseIcon.targetName, "\n"); }
        if (mission.targetBaseAirport != null)
        { builder.AppendFormat(" Target Airport: {0}{1}", mission.targetBaseAirport.targetName, "\n"); }
        if (mission.targetBaseHarbour != null)
        { builder.AppendFormat(" Target Harbour: {0}{1}", mission.targetBaseHarbour.targetName, "\n"); }
        if (mission.targetBaseVIP != null)
        { builder.AppendFormat(" Target VIP: {0}{1}", mission.targetBaseVIP.targetName, "\n"); }

        /*if (mission.targetBaseStory != null)
        { builder.AppendFormat(" Target Story: {0}{1}", mission.targetBaseStory.targetName, "\n"); }
        if (mission.targetBaseGoal != null)
        { builder.AppendFormat(" Target Goal: {0}{1}", mission.targetBaseGoal.targetName, "\n"); }*/

        if (mission.profileGenericLive != null)
        { builder.AppendFormat(" Profile Generic Live: {0}{1}", mission.profileGenericLive.name, "\n"); }
        if (mission.profileGenericActive != null)
        { builder.AppendFormat(" Profile Generic Active: {0}{1}", mission.profileGenericActive.name, "\n"); }
        if (mission.profileGenericFollowOn != null)
        { builder.AppendFormat(" Profile Generic FollowOn: {0}{1}", mission.profileGenericFollowOn.name, "\n"); }
        /*foreach(ObjectiveTarget objectiveTarget in mission.listOfObjectiveTargets)
        { builder.AppendFormat(" ObjectiveTarget: {0}{1}", objectiveTarget.name, "\n"); }*/
        mission.listOfObjectiveTargets.ForEach(objectiveTarget => builder.AppendFormat(" ObjectiveTarget: {0}{1}", objectiveTarget.name, "\n"));
        //Npc
        if (GameManager.i.missionScript.mission.npc != null)
        {
            builder.AppendFormat("{0}{1}-Npc{2}", "\n", "\n", "\n");
            builder.AppendFormat(" Npc Name: {0}{1}", mission.npc.tag, "\n");
            builder.AppendFormat(" startTurn: {0}{1}", mission.npc.startTurn, "\n");
            builder.AppendFormat(" startChance: {0}{1}", mission.npc.startChance, "\n");
            builder.AppendFormat(" stealthRating: {0}{1}", mission.npc.stealthRating, "\n");
            builder.AppendFormat(" Start NpcNode: {0}{1}", mission.npc.nodeStart.name, "\n");
            Node node = mission.npc.currentStartNode;
            if (node != null)
            { builder.AppendFormat(" currentStartNode: {0}, {1}, ID {2}{3}", node.nodeName, node.Arc.name, node.nodeID, "\n"); }
            else { builder.AppendFormat(" currentStartNode: Invalid{0}", "\n"); }
            builder.AppendFormat(" End NpcNode: {0}{1}", mission.npc.nodeEnd.name, "\n");
            node = mission.npc.currentEndNode;
            if (node != null)
            { builder.AppendFormat(" currentEndNode: {0}, {1}, ID {2}{3}", node.nodeName, node.Arc.name, node.nodeID, "\n"); }
            else { builder.AppendFormat(" currentEndNode: Invalid{0}", "\n"); }
            builder.AppendFormat(" Status: {0}{1}", mission.npc.status, "\n");
            if (mission.npc.status == NpcStatus.Active)
            {
                builder.AppendFormat(" currentNode: {0}, {1}, ID {2}{3}", mission.npc.currentNode.nodeName, mission.npc.currentNode.Arc.name, mission.npc.currentNode.nodeID, "\n");
                builder.AppendFormat(" timerTurns: {0} (start {1}){2}", mission.npc.timerTurns, mission.npc.maxTurns, "\n");
                builder.AppendFormat(" moveChance: {0}{1}", mission.npc.moveChance, "\n");
                builder.AppendFormat(" isRepeat: {0}{1}", mission.npc.isRepeat, "\n");
                builder.AppendFormat(" isIgnoreInvisible: {0}{1}", mission.npc.isIgnoreInvisible, "\n");
            }
            //npc invisible nodes
            builder.AppendFormat("{0}- Npc ListOfInvisibleNodes{1}", "\n", "\n");
            List<int> listOfNodes = GameManager.i.missionScript.mission.npc.listOfInvisibleNodes;
            if (listOfNodes != null)
            {
                int count = listOfNodes.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Node nodeStealth = GameManager.i.dataScript.GetNode(listOfNodes[i]);
                        if (nodeStealth != null)
                        { builder.AppendFormat(" {0}, {1}, nodeID {2}{3}", nodeStealth.nodeName, nodeStealth.Arc.name, nodeStealth.nodeID, "\n"); }
                        else { Debug.LogWarningFormat("Invalid node (Null) for nodeID {0}, listOfStealthNodes[{1}]", listOfNodes[i], i); }
                    }
                }
                else { builder.AppendFormat("  No nodes present{0}", "\n"); }
            }
            else { Debug.LogError("Invalid listOfInvisibleNodes (Null)"); }

        }
        return builder.ToString();
    }
}
