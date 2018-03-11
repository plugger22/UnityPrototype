using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all faction related matters for both sides
/// </summary>
public class FactionManager : MonoBehaviour
{

    public Faction currentAuthorityFaction;
    public Faction currentResistanceFaction;


    public void Initialise()
    {
        currentAuthorityFaction = GameManager.instance.dataScript.GetRandomFaction(GameManager.instance.globalScript.sideAuthority);
        currentResistanceFaction = GameManager.instance.dataScript.GetRandomFaction(GameManager.instance.globalScript.sideResistance);
        Debug.Log(string.Format("FactionManager: currentResistanceFaction \"{0}\", currentAuthorityFaction \"{1}\"{2}", 
            currentResistanceFaction, currentAuthorityFaction, "\n"));
    }

    /// <summary>
    /// returns current faction for the player side, Null if not found
    /// </summary>
    /// <returns></returns>
    public Faction GetCurrentFaction()
    {
        Faction faction = null;
        switch(GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Authority":
                faction = currentAuthorityFaction;
                break;
            case "Resistance":
                faction = currentResistanceFaction;
                break;
            default:
                Debug.LogError(string.Format("Invalid player side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                break;
        }
        return faction;
    }


}
