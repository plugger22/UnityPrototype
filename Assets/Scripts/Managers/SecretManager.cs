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
    [HideInInspector] public SecretType secretTypePlayer;
    [HideInInspector] public SecretType secretTypeDesperate;
    [HideInInspector] public SecretStatus secretStatusActive;
    [HideInInspector] public SecretStatus secretStatusInactive;
    [HideInInspector] public SecretStatus secretStatusRevealed;
    [HideInInspector] public SecretStatus secretStatusDeleted;

    //fast access
    private Condition conditionBlackmail;

    /*//colours
    string colourDefault;
    string colourNormal;
    string colourAlert;
    string colourResistance;
    string colourBad;
    string colourNeutral;
    string colourGood;
    string colourEnd;*/

    /// <summary>
    /// Initialise Secrets
    /// </summary>
    public void Initialise()
    {
        //
        // - - - SecretTypes - - -
        //
        Dictionary<string, SecretType> dictOfSecretTypes = GameManager.instance.dataScript.GetDictOfSecretTypes();
        if (dictOfSecretTypes != null)
        {
            foreach (var secretType in dictOfSecretTypes)
            {
                //pick out and assign the ones required for fast access. 
                //Also dynamically assign SecretType.level values (0/1/2). 
                switch (secretType.Key)
                {
                    case "Player":
                        secretType.Value.level = 0;
                        secretTypePlayer = secretType.Value;
                        break;
                    case "Desperate":
                        secretType.Value.level = 1;
                        secretTypeDesperate = secretType.Value;
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid secretType \"{0}\"", secretType.Key);
                        break;
                }
            }
            //error check
            Debug.Assert(secretTypePlayer != null, "Invalid secretTypePlayer (Null)");
            Debug.Assert(secretTypeDesperate != null, "Invalid secretTypeDesperate (Null)");
        }
        else { Debug.LogWarning("Invalid dictOfSecretTypes (Null)"); }
        //
        // - - - SecretStatus - - -
        //
        Dictionary<string, SecretStatus> dictOfSecretStatus = GameManager.instance.dataScript.GetDictOfSecretStatus();
        if (dictOfSecretStatus != null)
        {
            foreach (var secretStatus in dictOfSecretStatus)
            {
                //pick out and assign the ones required for fast access. 
                //Also dynamically assign SecretStatus.level values (0/1/2). 
                switch (secretStatus.Key)
                {
                    case "Inactive":
                        secretStatus.Value.level = 0;
                        secretStatusInactive = secretStatus.Value;
                        break;
                    case "Active":
                        secretStatus.Value.level = 1;
                        secretStatusActive = secretStatus.Value;
                        break;
                    case "Revealed":
                        secretStatus.Value.level = 2;
                        secretStatusRevealed = secretStatus.Value;
                        break;
                    case "Deleted":
                        secretStatus.Value.level = 3;
                        secretStatusDeleted = secretStatus.Value;
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid secretStatus \"{0}\"", secretStatus.Key);
                        break;
                }
            }
            //error check
            Debug.Assert(secretStatusActive != null, "Invalid secretStatusActive (Null)");
            Debug.Assert(secretStatusInactive != null, "Invalid secretStatusInactive (Null)");
            Debug.Assert(secretStatusRevealed != null, "Invalid secretStatusRevealed (Null)");
            Debug.Assert(secretStatusDeleted != null, "Invalid secretStatusDeleted (Null)");
        }
        else { Debug.LogWarning("Invalid dictOfSecretTypes (Null)"); }
        //
        // - - - - Secrets - - - 
        //
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
        //
        // - - - Fast Access
        //
        conditionBlackmail = GameManager.instance.dataScript.GetCondition("BLACKMAILER");
        Debug.Assert(conditionBlackmail != null, "Invalid conditionBlackmail (Null)");
        //
        // - - - Event listeners
        //
        /*SetColours();
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "SecretManager");*/
    }

    /*/// <summary>
    /// event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //detect event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }*/

    /*/// <summary>
    /// Set colour palette for tooltip
    /// </summary>
    public void SetColours()
    {
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }*/




    /// <summary>
    /// Removes a given secret from all actors and player. If calling for a deleted secret then set to true, otherwise, for a normal revealed secret situation, default false
    /// This ensures that if a secret is deleted from an actor who is currently blackmailing then their blackmailer status is removed if they end up with no secrets remaining
    /// Returns true if successfully removed secret/s, false otherwise
    /// </summary>
    /// <param name="secretID"></param>
    public bool RemoveSecretFromAll(int secretID, bool isDeletedSecret = false)
    {
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        Secret secret = GameManager.instance.dataScript.GetSecret(secretID);
        bool isSuccess = true;
        if (secret != null)
        {
            //remove from player
            GameManager.instance.playerScript.RemoveSecret(secretID);
            if (isDeletedSecret == true)
            {
                //message
                string playerMsg = string.Format("Player loses secret \"{0}\"", secret.tag);
                GameManager.instance.messageScript.PlayerSecret(playerMsg, secret, false);
            }
            //remove actors from secret list
            secret.RemoveAllActors();
            //loop actors
            Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(side);
            if (arrayOfActors != null)
            {
                for (int i = 0; i < arrayOfActors.Length; i++)
                {
                    //check actor is present in slot (not vacant)
                    if (GameManager.instance.dataScript.CheckActorSlotStatus(i, side) == true)
                    {
                        Actor actor = arrayOfActors[i];
                        if (actor != null)
                        {
                            actor.RemoveSecret(secretID);
                            //blackmail check -> if actor is blackmailing and they end up with zero secrets then the condition is removed
                            if (isDeletedSecret == true)
                            {
                                //message (any situation where a blackmail check is needed is going to be a deleted secret, hence the need for a message
                                string msgText = string.Format("{0} loses secret \"{1}\"", actor.arc.name, secret.tag);
                                GameManager.instance.messageScript.ActorSecret(msgText, actor, secret, false);
                                if (actor.CheckConditionPresent(conditionBlackmail) == true)
                                {
                                    if (actor.CheckNumOfSecrets() == 0)
                                    {
                                        actor.RemoveCondition(conditionBlackmail);
                                        //additional explanatory message (why has condition gone?)
                                        string blackText = string.Format("{0} can no longer Blackmail (no Secret)", actor.arc.name);
                                        string reason = "The secret they hold has no value";
                                        GameManager.instance.messageScript.ActorBlackmail(blackText, actor, secret.secretID, true, reason);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            isSuccess = false;
            Debug.LogWarningFormat("Invalid secret (Null) for secretID {0} -> Not removed", secretID);
        }
        return isSuccess;
    }


    //
    // - - - Debug - - -
    //

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
                    builder.AppendFormat("{0} ID {1}, {2} ({3}), {4}, Known: {5}", "\n", secret.Value.secretID, secret.Value.name, secret.Value.tag, secret.Value.status.name,
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
                        builderTemp.AppendFormat("{0} ID {1}, {2} ({3}), {4} turn {5}, {6}", "\n", secret.secretID, secret.name, secret.tag,
                            GameManager.instance.dataScript.GetActor(secret.revealedWho).arc.name, secret.revealedWhen, secret.status.name);
                    }
                    else
                    { builderTemp.AppendFormat("{0} ID {1}, {2} ({3}) {4}", "\n", secret.secretID, secret.name, secret.tag, secret.status.name ); }
                }
            }
            else { builderTemp.AppendFormat("{0} No records", "\n"); }
        }
        else { Debug.LogWarning("Invalid listOfRevealedSecrets (Null)"); }
        return builderTemp.ToString();
    }

}
