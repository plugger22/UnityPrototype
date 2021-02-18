using gameAPI;
using modalAPI;
using packageAPI;
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
    [Range(2, 10)] public int secretBlackmailTimer = 5;
    [Tooltip("Max number of secrets allowed for a Player or Actor.Determined by capacity of InventoryUI")]
    [Range(0, 4)] public int secretMaxNum = 4;
    [Tooltip("Max number of effects each secret can have (apply if secret revealed). Any effects in excess of this are ignored by code")]
    [Range(1, 4)] public int secretMaxEffects = 4;


    //globals
    [HideInInspector] public SecretType secretTypePlayer;
    [HideInInspector] public SecretType secretTypeOrganisation;
    [HideInInspector] public SecretType secretTypeStory;
    /*[HideInInspector] public SecretStatus secretStatusActive;
    [HideInInspector] public SecretStatus secretStatusInactive;
    [HideInInspector] public SecretStatus secretStatusRevealed;
    [HideInInspector] public SecretStatus secretStatusDeleted;*/

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
    /// Initialise Secrets. Not for GameState.Load
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseLevelStart();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseLevelStart();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //
        // - - - SecretTypes - - -
        //
        Dictionary<string, SecretType> dictOfSecretTypes = GameManager.i.dataScript.GetDictOfSecretTypes();
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
                    case "Organisation":
                        secretType.Value.level = 1;
                        secretTypeOrganisation = secretType.Value;
                        break;
                    case "Story":
                        secretType.Value.level = 2;
                        secretTypeStory = secretType.Value;
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid secretType \"{0}\"", secretType.Key);
                        break;
                }
            }
            //error check
            Debug.Assert(secretTypePlayer != null, "Invalid secretTypePlayer (Null)");
            Debug.Assert(secretTypeOrganisation != null, "Invalid secretTypeOrganisation (Null)");
        }
        else { Debug.LogWarning("Invalid dictOfSecretTypes (Null)"); }
        //
        // - - - - Secrets - - - 
        //
        Dictionary<string, Secret> dictOfSecrets = GameManager.i.dataScript.GetDictOfSecrets();
        List<Secret> listOfPlayerSecrets = GameManager.i.dataScript.GetListOfPlayerSecrets();
        List<Secret> listOfOrganisationSecrets = GameManager.i.dataScript.GetListOfOrganisationSecrets();
        List<Secret> listOfStorySecrets = GameManager.i.dataScript.GetListOfStorySecrets();

        int playerLevel = GameManager.i.sideScript.PlayerSide.level;
        if (dictOfSecrets != null)
        {
            if (listOfPlayerSecrets != null)
            {
                if (listOfOrganisationSecrets != null)
                {
                    if (listOfStorySecrets != null)
                    {
                        //add to appropriate lists
                        foreach (var secret in dictOfSecrets)
                        {
                            if (secret.Value != null)
                            {
                                //set all key secret SO data to default settings (otherwise will carry over data between sessions)
                                secret.Value.Initialise();
                                //Only add those of the same side as the player)
                                if (secret.Value.side.level == playerLevel)
                                {
                                    switch (secret.Value.type.level)
                                    {
                                        case 0:
                                            //Player secrets
                                            listOfPlayerSecrets.Add(secret.Value);
                                            break;
                                        case 1:
                                            //Organisation measures secrets (desperate)
                                            listOfOrganisationSecrets.Add(secret.Value);
                                            break;
                                        case 2:
                                            //Story secrets
                                            listOfStorySecrets.Add(secret.Value);
                                            break;
                                    }
                                }
                            }
                            else { Debug.LogWarning("Invalid secret (Null) in dictOfSecrets"); }
                        }
                        Debug.LogFormat("[Loa] SecretManager.cs -> listOfPlayerSecrets has {0} entries{1}", listOfPlayerSecrets.Count, "\n");
                        Debug.Assert(listOfPlayerSecrets.Count > 0, "No records in listOfPlayerSecrets");
                    }
                    else { Debug.LogWarning("Invalid listOfStorySecrets (Null)"); }
                }
                else { Debug.LogWarning("Invalid listOfOrganisationSecrets (Null)"); }
            }
            else { Debug.LogWarning("Invalid listOfPlayerSecrets (Null)"); }
        }
        else { Debug.LogWarning("Invalid dictOfSecrets (Null)"); }
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        //Initialise always (Level based) -> reset all player secrets to known 0
        List<Secret> listOfSecrets = GameManager.i.dataScript.GetListOfPlayerSecrets();
        if (listOfSecrets != null)
        {
            foreach (Secret secret in listOfSecrets)
            {
                if (secret != null)
                { secret.ResetFollowOnLevel(); }
                else { Debug.LogError("Invalid secret (Null) in listOfPlayerSecrets"); }
            }

        }
        else { Debug.LogError("Invalid listOfPlayerSecrets (Null)"); }
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //Fast Access
        conditionBlackmail = GameManager.i.dataScript.GetCondition("BLACKMAILER");
        Debug.Assert(conditionBlackmail != null, "Invalid conditionBlackmail (Null)");
    }
    #endregion

    #endregion


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
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.whiteText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.salmonText);
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.blueText);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badText);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralText);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }*/


    #region RemoveSecretFromAll
    /// <summary>
    /// Removes a given secret from all actors (OnMap and Reserve) and player. If calling for a deleted secret then set to true, otherwise, for a normal revealed secret situation, default false
    /// This ensures that if a secret is deleted from an actor who is currently blackmailing then their blackmailer status is removed if they end up with no secrets remaining
    /// Revealed secrets may trigger an investigation, deleted ones not
    /// Returns true if successfully removed secret/s, false otherwise
    /// </summary>
    /// <param name="secretID"></param>
    public bool RemoveSecretFromAll(string secretName, bool isDeletedSecret = false)
    {
        GlobalSide side = GameManager.i.sideScript.PlayerSide;
        Secret secret = GameManager.i.dataScript.GetSecret(secretName);
        bool isSuccess = true;
        if (secret != null)
        {
            //remove from player
            GameManager.i.playerScript.RemoveSecret(secretName);
            if (isDeletedSecret == true)
            {
                //message
                string playerMsg = string.Format("Player loses secret \"{0}\"", secret.tag);
                GameManager.i.messageScript.PlayerSecret(playerMsg, secret, false);
            }
            //remove actors from secret list
            secret.RemoveAllActors();
            //Create a list of all current actors plus all actors in Reserve
            List<Actor> listOfActors = new List<Actor>();
            //add current actors
            Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActorsFixed(side);
            if (arrayOfActors != null)
            {
                for (int i = 0; i < arrayOfActors.Length; i++)
                {
                    //check actor is present in slot (not vacant)
                    if (GameManager.i.dataScript.CheckActorSlotStatus(i, side) == true)
                    { listOfActors.Add(arrayOfActors[i]); }
                }
            }
            else { Debug.LogWarning("Invalid arrayOfActors (Null)"); }
            //add reserve actors
            List<int> listOfReserveActors = GameManager.i.dataScript.GetActorList(GameManager.i.sideScript.PlayerSide, ActorList.Reserve);
            if (listOfReserveActors.Count > 0)
            {
                for (int i = 0; i < listOfReserveActors.Count; i++)
                {
                    Actor actor = GameManager.i.dataScript.GetActor(listOfReserveActors[i]);
                    if (actor != null)
                    { listOfActors.Add(actor); }
                    else { Debug.LogWarningFormat("Invalid actor (Null) for actorID {0}", listOfReserveActors[i]); }
                }
            }
            //loop all actors
            foreach (Actor actor in listOfActors)
            {
                if (actor != null)
                {
                    actor.RemoveSecret(secretName);
                    //blackmail check -> if actor is blackmailing and they end up with zero secrets then the condition is removed
                    if (isDeletedSecret == true)
                    {
                        //message (any situation where a blackmail check is needed is going to be a deleted secret, hence the need for a message
                        string msgText = string.Format("{0} loses secret \"{1}\"", actor.arc.name, secret.tag);
                        GameManager.i.messageScript.ActorSecret(msgText, actor, secret, false);
                        if (actor.CheckConditionPresent(conditionBlackmail) == true)
                        {
                            if (actor.CheckNumOfSecrets() == 0)
                            {
                                actor.RemoveCondition(conditionBlackmail, "Secret no longer has any effect");
                                //additional explanatory message (why has condition gone?)
                                string blackText = string.Format("{0} can no longer Blackmail (no Secret)", actor.arc.name);
                                string reason = "The secret they hold has no value";
                                GameManager.i.messageScript.ActorBlackmail(blackText, actor, secret, true, reason);
                            }
                        }
                    }
                }
            }
            //chance Investigation launched
            if (GameManager.i.playerScript.CheckInvestigationPossible() == true)
            {
                //revealed secrets only (no investigation possible for deleted secrets)
                if (isDeletedSecret == false)
                {
                    string text;
                    int rnd = Random.Range(0, 100);
                    int chance = GameManager.i.playerScript.chanceInvestigation;
                    int gameTurn = GameManager.i.turnScript.Turn;
                    if (rnd < chance)
                    {
                        //create a new investigation
                        Investigation invest = new Investigation()
                        {
                            reference = string.Format("{0}{1}", gameTurn, secret.name),
                            tag = secret.investigationTag,
                            evidence = secret.investigationEvidence,
                            turnStart = gameTurn,
                            lead = GameManager.i.hqScript.GetRandomHqPosition(),
                            city = GameManager.i.cityScript.GetCityName(),
                            status = InvestStatus.Ongoing,
                            outcome = InvestOutcome.None
                        };
                        //add to player's list
                        GameManager.i.playerScript.AddInvestigation(invest);
                        //stats
                        GameManager.i.dataScript.StatisticIncrement(StatType.InvestigationsLaunched);
                        //msgs
                        Debug.LogFormat("[Rnd] SecretManager.cs -> RemoveSecretFromAll: INVESTIGATION commences, need < {0}, rolled {1}{2}", chance, rnd, "\n");
                        text = "INVESTIGATION commences";
                        GameManager.i.messageScript.GeneralRandom(text, "Investigation", chance, rnd, true, "rand_4");
                        text = string.Format("Investigation into Player {0} launched by {1}", invest.tag, invest.lead);
                        GameManager.i.messageScript.InvestigationNew(text, invest);
                        //history
                        GameManager.i.dataScript.AddHistoryPlayer(new HistoryActor() { text = string.Format("Investigation Launched into your conduct ({0})", secret.investigationTag) });
                        //outcome (message pipeline)
                        text = string.Format("<size=120%>INVESTIGATION</size>{0}Launched into your{1}{2}", "\n", "\n", GameManager.Formatt(invest.tag, ColourType.neutralText));
                        string bottomText = "Unknown";
                        Actor actor = GameManager.i.dataScript.GetHqHierarchyActor(invest.lead);
                        if (actor == null)
                        {
                            Debug.LogErrorFormat("Invalid HQ actor for ActorHQ invest.lead \"{0}\"", GameManager.i.hqScript.GetHqTitle(invest.lead));
                            bottomText = string.Format("HQ have assigned their {0} to lead the investigation{1}",
                            actor.actorName, GameManager.Formatt(GameManager.i.hqScript.GetHqTitle(invest.lead), ColourType.salmonText).ToUpper(), "\n");
                        }
                        else
                        {
                            bottomText = string.Format("HQ have assigned{0}{1}, {2}{3}to lead the investigation{4}", "\n", actor.actorName,
                                GameManager.Formatt(GameManager.i.hqScript.GetHqTitle(invest.lead), ColourType.salmonText).ToUpper(), "\n", "\n");
                        }
                        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails
                        {
                            textTop = text,
                            textBottom = bottomText,
                            sprite = GameManager.i.spriteScript.investigationSprite,
                            isAction = false,
                            side = GameManager.i.sideScript.PlayerSide,
                            type = MsgPipelineType.InvestigationLaunched,
                            help0 = "invest_10"
                        };
                        if (GameManager.i.guiScript.InfoPipelineAdd(outcomeDetails) == false)
                        { Debug.LogWarningFormat("Investigation Launched InfoPipeline message FAILED to be added to dictOfPipeline"); }
                    }
                    else
                    {
                        //msgs
                        Debug.LogFormat("[Rnd] SecretManager.cs -> RemoveSecretFromAll: No Investigation, need < {0}, rolled {1}{2}", chance, rnd, "\n");
                        text = "No INVESTIGATION";
                        GameManager.i.messageScript.GeneralRandom(text, "Investigation", chance, rnd, false, "rand_4");
                    }
                }
            }
            else { Debug.LogFormat("[Inv] SecretManager.cs -> RemoveSecretFromAll: Max number of investigations already, new Investigation not possible{0}", "\n"); }
        }
        else
        {
            isSuccess = false;
            Debug.LogWarningFormat("Invalid secret (Null) for secret {0} -> Not removed", secretName);
        }
        return isSuccess;
    }
    #endregion


    #region RemoveSecretsFromActor
    /// <summary>
    /// used when actor leaves map -> fired / dismissed / disposed off. Called by DataManager.cs -> RemoveActorAdmin
    /// </summary>
    /// <param name="actor"></param>
    public void RemoveAllSecretsFromActor(Actor actor)
    {
        if (actor != null)
        {
            //lose any secrets
            if (actor.CheckNumOfSecrets() > 0)
            {
                List<Secret> listOfSecrets = actor.GetListOfSecrets();
                if (listOfSecrets != null)
                {
                    foreach (Secret secret in listOfSecrets)
                    {
                        if (secret != null)
                        {
                            //remove actor from secret list
                            secret.RemoveActor(actor.actorID);
                        }
                        else { Debug.LogWarning("Invalid secret (Null)"); }
                    }
                    //delete all secrets from actor
                    actor.RemoveAllSecrets();
                }
                else { Debug.LogWarning("Invalid listOfSecrets (Null)"); }
            }
        }
        else { Debug.LogWarning("Invalid actor (Null)"); }
    }
    #endregion

    //
    // - - - Debug - - -
    //

    /// <summary>
    /// debug method to display data
    /// </summary>
    /// <returns></returns>
    public string DebugDisplaySecretData()
    {
        StringBuilder builder = new StringBuilder();
        GlobalSide side = GameManager.i.sideScript.PlayerSide;
        Dictionary<string, Secret> dictOfSecrets = GameManager.i.dataScript.GetDictOfSecrets();
        builder.AppendFormat(" Secret Data {0}{1}", "\n", "\n");
        //main dictionary data
        builder.Append("- dictOfSecrets");
        if (dictOfSecrets != null)
        {
            if (dictOfSecrets.Count > 0)
            {
                foreach (var secret in dictOfSecrets)
                { builder.AppendFormat("{0} {1} (\"{2}\"), {3}, Known: {4}", "\n", secret.Key, secret.Value.tag, secret.Value.status, secret.Value.CheckNumOfActorsWhoKnow()); }
            }
            else { builder.AppendFormat("{0} No records", "\n"); }
        }
        else { Debug.LogWarning("Invalid dictOfSecrets (Null)"); }
        //player secrets data
        builder.AppendFormat("{0}{1}- listOfPlayerSecrets", "\n", "\n");
        builder.Append(DisplaySecretList(GameManager.i.dataScript.GetListOfPlayerSecrets()));
        //organisation secrets data
        builder.AppendFormat("{0}{1}- listOfOrganisationSecrets", "\n", "\n");
        builder.Append(DisplaySecretList(GameManager.i.dataScript.GetListOfOrganisationSecrets()));
        //story secrets data
        builder.AppendFormat("{0}{1}- listOfStorySecrets", "\n", "\n");
        builder.Append(DisplaySecretList(GameManager.i.dataScript.GetListOfStorySecrets()));
        //revealed secrets data
        builder.AppendFormat("{0}{1}- listOfRevealedSecrets", "\n", "\n");
        builder.Append(DisplaySecretList(GameManager.i.dataScript.GetListOfRevealedSecrets()));
        //deleted secrets data
        builder.AppendFormat("{0}{1}- listOfDeletedSecrets", "\n", "\n");
        builder.Append(DisplaySecretList(GameManager.i.dataScript.GetListOfDeletedSecrets()));
        //player data
        builder.AppendFormat("{0}{1}- PLAYER", "\n", "\n");
        builder.Append(GameManager.i.playerScript.DebugDisplaySecrets());
        //actor data
        for (int i = 0; i < GameManager.i.actorScript.maxNumOfOnMapActors; i++)
        {
            //actor present
            if (GameManager.i.dataScript.CheckActorSlotStatus(i, side) == true)
            {
                Actor actor = GameManager.i.dataScript.GetCurrentActor(i, side);
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
                    if (secret.revealedWhen.turn > -1)
                    { builderTemp.AppendFormat("{0} {1} ({2}), {3} t {4} at {5}, {6}", "\n", secret.name, secret.tag, secret.revealedWho, secret.revealedWhen.turn, 
                        GameManager.i.campaignScript.GetScenario(secret.revealedWhen.scenario).city.tag, secret.status); }
                    else if (secret.deletedWhen.turn > -1)
                    { builderTemp.AppendFormat("{0} {1}, {2} t {3} at {4}, {5}", "\n", secret.name, secret.tag, secret.deletedWhen.turn, 
                        GameManager.i.campaignScript.GetScenario(secret.deletedWhen.scenario).city.tag, secret.status); }
                    else { builderTemp.AppendFormat("{0} {1} ({2}) {3}", "\n", secret.name, secret.tag, secret.status ); }
                }
            }
            else { builderTemp.AppendFormat("{0} No records", "\n"); }
        }
        else { Debug.LogWarning("Invalid listOfRevealedSecrets (Null)"); }
        return builderTemp.ToString();
    }

}
