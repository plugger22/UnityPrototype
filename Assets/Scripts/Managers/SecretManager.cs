using gameAPI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all Secret related matters
/// </summary>
public class SecretManager : MonoBehaviour
{

    [Tooltip("Base chance of an actor learning of a secret per turn (2x if secret Medium, 3x for High)")]
    [Range(0, 100)] public int secretLearnBaseChance = 25;
    [Tooltip("Number of turns to set the actor 'Blackmail -> threatening to reveal secret' timer")]
    [Range(0, 10)] public int secretBlackmailTimer = 5;


    public void Initialise()
    {
        Dictionary<int, Secret> dictOfSecrets = GameManager.instance.dataScript.GetDictOfSecrets();
        if (dictOfSecrets != null)
        {
            //set all key secret data to default settings
            foreach(var secret in dictOfSecrets)
            {
                if (secret.Value != null)
                { secret.Value.Initialise(); }
                else { Debug.LogWarning("Invalid secret (Null) in dictOfSecrets"); }
            }
        }
        else { Debug.LogWarning("Invalid dictOfSecrets (Null)"); }
    }

    /// <summary>
    /// debug method to display data
    /// </summary>
    /// <returns></returns>
    public string DisplaySecretData()
    {
        StringBuilder builder = new StringBuilder();
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        Dictionary<int, Secret> dictOfSecrets = GameManager.instance.dataScript.GetDictOfSecrets();
        builder.AppendFormat(" Secret Data {0}{1}", "\n", "\n");
        //main dictionary data
        builder.Append("- dictOfSecrets");
        if (dictOfSecrets != null)
        {
            if (dictOfSecrets.Count > 0)
            {
                foreach (var secret in dictOfSecrets)
                {
                    builder.AppendFormat("{0} ID {1}, {2} ({3}), isActive: {4}, Known: {5}", "\n", secret.Value.secretID, secret.Value.name, secret.Value.tag, secret.Value.isActive,
                        secret.Value.CheckNumOfActorsWhoKnow());
                }
            }
            else { builder.AppendFormat("{0} No records", "\n"); }
        }
        else { Debug.LogWarning("Invalid dictOfSecrets (Null)"); }
        //player data
        builder.AppendFormat("{0}{1}- Player listOfSecrets", "\n", "\n");
        builder.Append(GameManager.instance.playerScript.DebugDisplaySecrets());
        //actor data
        for (int i = 0; i < GameManager.instance.actorScript.maxNumOfOnMapActors; i++)
        {
            //actor present
            if (GameManager.instance.dataScript.CheckActorSlotStatus(i, side) == true)
            {
                Actor actor = GameManager.instance.dataScript.GetCurrentActor(i, side);
                if (actor != null)
                {
                    builder.AppendFormat("{0}{1}- {2} ID {3} listOfSecrets", "\n", "\n", actor.arc.name, actor.actorID);
                    builder.Append(actor.DebugDisplaySecrets());
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorSlotID {0}", i); }
            }
        }
        return builder.ToString();
    }

}
