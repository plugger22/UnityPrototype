using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all faction related matters for both sides
/// </summary>
public class FactionManager : MonoBehaviour
{

    [HideInInspector] public Faction factionAuthority;
    [HideInInspector] public Faction factionResistance;


    public void Initialise()
    {
        factionAuthority = GameManager.instance.dataScript.GetRandomFaction(GameManager.instance.globalScript.sideAuthority);
        factionResistance = GameManager.instance.dataScript.GetRandomFaction(GameManager.instance.globalScript.sideResistance);
        Debug.Log(string.Format("FactionManager: currentResistanceFaction \"{0}\", currentAuthorityFaction \"{1}\"{2}", 
            factionResistance, factionAuthority, "\n"));
    }

    /// <summary>
    /// returns current faction for player side, Null if not found
    /// </summary>
    /// <returns></returns>
    public Faction GetCurrentFaction()
    {
        Faction faction = null;
        switch(GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Authority":
                faction = factionAuthority;
                break;
            case "Resistance":
                faction = factionResistance;
                break;
            default:
                Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                break;
        }
        return faction;
    }


    /// <summary>
    /// Debug method to display faction info
    /// </summary>
    /// <returns></returns>
    public string DisplayFactions()
    {
        StringBuilder builder = new StringBuilder();
        //authority
        builder.AppendFormat(" AUTHORITY{0}", "\n");
        builder.AppendFormat(" {0} faction{1}", factionAuthority.name, "\n");
        builder.AppendFormat(" {0}{1}", factionAuthority.descriptor, "\n");
        builder.AppendFormat(" Preferred Nodes: {0},  Hostile Nodes: {1}{2}",
            factionAuthority.preferredArc != null ? factionAuthority.preferredArc.name : "None",
            factionAuthority.hostileArc != null ? factionAuthority.hostileArc.name : "None", "\n");
        //resistance
        builder.AppendFormat("{0} RESISTANCE{1}", "\n", "\n");
        builder.AppendFormat(" {0} faction{1}", factionResistance.name, "\n");
        builder.AppendFormat(" {0}{1}", factionResistance.descriptor, "\n");
        builder.AppendFormat(" Preferred Nodes: {0}, Hostile Nodes: {1}{2}",
            factionResistance.preferredArc != null ? factionResistance.preferredArc.name : "None",
            factionResistance.hostileArc != null ? factionResistance.hostileArc.name : "None", "\n");
        return builder.ToString();
    }
}
