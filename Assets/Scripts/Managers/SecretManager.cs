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
    [Tooltip("Max number of secrets allowed for a Player or Actor.Determined by capacity of InventoryUI")]
    [Range(0, 4)] public int secretMaxNum = 4;


    //globals
    [HideInInspector] public int secretTypePlayer = -1;
    [HideInInspector] public int secretTypeDesperate = -1;


    public void Initialise()
    {
        //initialise SecretTypes
        Dictionary<string, SecretType> dictOfSecretTypes = GameManager.instance.dataScript.GetDictOfSecretTypes();
        if (dictOfSecretTypes != null)
        {
            foreach (var secretType in dictOfSecretTypes)
            {
                //pick out and assign the ones required for fast access. 
                //Add to lists where appropriate
                //Also dynamically assign SecretType.level values (0/1/2). 
                switch (secretType.Key)
                {
                    case "Player":
                        secretType.Value.level = 0;
                        secretTypePlayer = secretType.Value.level;
                        break;
                    case "Desperate":
                        secretType.Value.level = 1;
                        secretTypeDesperate = secretType.Value.level;
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid secretType \"{0}\"", secretType.Key);
                        break;
                }
            }
            //error check
            if (secretTypePlayer == -1) { Debug.LogError("Invalid secretTypePlayer (-1)"); }
            if (secretTypeDesperate == -1) { Debug.LogError("Invalid secretTypeDesperate (-1)"); }
        }
        else { Debug.LogWarning("Invalid dictOfSecretTypes (Null)"); }
        //initialise Secrets
        Dictionary<int, Secret> dictOfSecrets = GameManager.instance.dataScript.GetDictOfSecrets();
        List<Secret> listOfPlayerSecrets = GameManager.instance.dataScript.GetListOfPlayerSecrets();
        if (dictOfSecrets != null)
        {
            if (listOfPlayerSecrets != null)
            {
                foreach (var secret in dictOfSecrets)
                {
                    if (secret.Value != null)
                    {
                        //set all key secret data to default settings (otherwise will carry over data between sessions)
                        secret.Value.Initialise();
                        //add to appropriate lists
                        switch (secret.Value.type.level)
                        {
                            case 0:
                                //Player secrets
                                listOfPlayerSecrets.Add(secret.Value);
                                break;
                        }
                    }
                    else { Debug.LogWarning("Invalid secret (Null) in dictOfSecrets"); }
                }
                Debug.LogFormat("[Imp] SecretManager.cs -> listOfPlayerSecrets has {0} entries{1}", listOfPlayerSecrets.Count, "\n");
                Debug.Assert(listOfPlayerSecrets.Count > 0, "No records in listOfPlayerSecrets");
            }
            else { Debug.LogWarning("Invalid listOfPlayerSecrets (Null)"); }
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
        //player secrets data
        builder.AppendFormat("{0}{1}- listOfPlayerSecrets", "\n", "\n");
        builder.Append(DisplaySecretList(GameManager.instance.dataScript.GetListOfPlayerSecrets()));
        //revealed secrets data
        builder.AppendFormat("{0}{1}- listOfRevealedSecrets", "\n", "\n");
        builder.Append(DisplaySecretList(GameManager.instance.dataScript.GetListOfRevealedSecrets()));
        //deleted secrets data
        builder.AppendFormat("{0}{1}- listOfDeletedSecrets", "\n", "\n");
        builder.Append(DisplaySecretList(GameManager.instance.dataScript.GetListOfDeletedSecrets()));
        //player data
        builder.AppendFormat("{0}{1}- PLAYER", "\n", "\n");
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
                    builder.AppendFormat("{0}{1}- {2} ID {3}", "\n", "\n", actor.arc.name, actor.actorID);
                    builder.Append(actor.DebugDisplaySecrets());
                }
                else { Debug.LogWarningFormat("Invalid actor (Null) for actorSlotID {0}", i); }
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// subMethod used by DisplaySecretData to process Secret Lists
    /// </summary>
    /// <param name="tempList"></param>
    /// <returns></returns>
    private string DisplaySecretList(List<Secret> tempList)
    {
        StringBuilder builderTemp = new StringBuilder();
        if (tempList != null)
        {
            int numSecrets = tempList.Count;
            if (numSecrets > 0)
            {
                foreach (Secret secret in tempList)
                {
                    if (secret.revealedWho > -1)
                    {
                        builderTemp.AppendFormat("{0} ID {1}, {2} ({3}), ({4} turn {5})", "\n", secret.secretID, secret.name, secret.tag,
                            GameManager.instance.dataScript.GetActor(secret.revealedWho).arc.name, secret.revealedWhen);
                    }
                    else
                    { builderTemp.AppendFormat("{0} ID {1}, {2} ({3})", "\n", secret.secretID, secret.name, secret.tag ); }
                }
            }
            else { builderTemp.AppendFormat("{0} No records", "\n"); }
        }
        else { Debug.LogWarning("Invalid listOfRevealedSecrets (Null)"); }
        return builderTemp.ToString();
    }

}
