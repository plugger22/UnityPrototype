using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using packageAPI;

/// <summary>
/// handles all MetaLevel matters
/// </summary>
public class MetaManager : MonoBehaviour
{
    [Header("MetaOption Arrays")]
    [Tooltip("Place metaOptions here to handle the max number of possible organisation metaOptions that may be required")]
    public MetaOption[] arrayOfOrganisationOptions;
    [Tooltip("Place metaOptions here to handle the max number of possible secret metaOptions that may be required")]
    public MetaOption[] arrayOfSecretOptions;
    [Tooltip("Place metaOptions here to handle the max number of possible investigation metaOptions that may be required")]
    public MetaOption[] arrayOfInvestigationOptions;

    [HideInInspector] public GlobalMeta metaLevel;

    private MetaGameOptions metaGameOptions;
    private MetaEffectData metaEffect;                      //for passing data from selected metaOptions (one at a time) to EffectManager

    //data for compiling available metaOptions (sent at end of a level by ProcessMeta... methods in individual classes)
    private List<Organisation> listOfOrganisations = new List<Organisation>();
    private List<Secret> listOfSecrets = new List<Secret>();
    private List<Investigation> listOfInvestigations = new List<Investigation>();
    //MetaOptions to display
    private List<MetaOption> listOfMetaOptions = new List<MetaOption>();


    public void Initialise(GameState state)
    {
        //set state
        metaLevel = GameManager.instance.globalScript.metaBottom;
        /*metaEffect = new MetaEffectData();*/
    }


    /// <summary>
    /// run at start of every metaGame
    /// </summary>
    public void InitialiseMetaGameOptions()
    {
        //debug metaGame options
        metaGameOptions = new MetaGameOptions();
        if (GameManager.instance.testScript.isValidMetaOptions == true)
        {
            metaGameOptions.isDismissed = GameManager.instance.testScript.isDismissed;
            metaGameOptions.isResigned = GameManager.instance.testScript.isResigned;
            metaGameOptions.isLowMotivation = GameManager.instance.testScript.isLowMotivation;
            metaGameOptions.isTraitor = GameManager.instance.testScript.isTraitor;
            metaGameOptions.isLevelTwo = GameManager.instance.testScript.isLevelTwo;
            metaGameOptions.isLevelThree = GameManager.instance.testScript.isLevelThree;
        }
        else
        {
            //default -> level 1, include everybody
            metaGameOptions.isDismissed = true;
            metaGameOptions.isResigned = true;
            metaGameOptions.isLowMotivation = true;
            metaGameOptions.isTraitor = true;
            metaGameOptions.isLevelTwo = false;
            metaGameOptions.isLevelThree = false;
        }
    }

    /// <summary>
    /// Metalevel master sequence
    /// </summary>
    public void ProcessMetaGame()
    {
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        InitialiseMetaGameOptions();
        ResetMetaAdmin();
        //hide top bar UI at start of meta game
        EventManager.instance.PostNotification(EventType.TopBarHide, this, null, "MetaManager.cs -> Hide TopBarUI");
        GameManager.instance.statScript.ProcessMetaStatistics();
        GameManager.instance.topicScript.ProcessMetaTopics();
        GameManager.instance.hqScript.ProcessMetaHq(playerSide);          //needs to be BEFORE MetaActors
        GameManager.instance.actorScript.ProcessMetaActors(playerSide);
        GameManager.instance.dataScript.ProcessMetaCures();
        GameManager.instance.orgScript.ProcessMetaOrgs();
        GameManager.instance.playerScript.ProcessMetaPlayer();
        //show top bar UI at completion of meta game
        EventManager.instance.PostNotification(EventType.TopBarShow, this, null, "MetaManager.cs -> Show TopBarUI");

        //Player metaGame Options choice
        InitialiseMetaOptions();
    }

    /// <summary>
    /// handles all admin matters prior to MetaGame start
    /// </summary>
    public void ResetMetaAdmin()
    {
        //clear out collections
        listOfOrganisations.Clear();
        listOfSecrets.Clear();
        listOfInvestigations.Clear();
        listOfMetaOptions.Clear();
    }

    /// <summary>
    /// returns official authority title
    /// </summary>
    /// <returns></returns>
    public AuthorityTitle GetAuthorityTitle()
    { return (AuthorityTitle)(metaLevel.level); }


    //
    // - - - MetaGameOptions
    //

    /// <summary>
    /// gets player chosen metaGame options
    /// </summary>
    /// <returns></returns>
    public MetaGameOptions GetMetaOptions()
    { return metaGameOptions; }

    /// <summary>
    /// Set metaGame setting, isDismissed
    /// </summary>
    /// <param name="setting"></param>
    public void SetMetaGameDismissed(bool setting)
    {
        metaGameOptions.isDismissed = setting;
        Debug.LogFormat("[Met] MetaManager.cs -> SetMetaOptionDismissed: Option isDismissed now {0}{1}", setting, "\n");
    }

    /// <summary>
    /// Set metaGame setting, isResigned
    /// </summary>
    /// <param name="setting"></param>
    public void SetMetaGameResigned(bool setting)
    {
        metaGameOptions.isResigned = setting;
        Debug.LogFormat("[Met] MetaManager.cs -> SetMetaOptionResigned: Option isResigned now {0}{1}", setting, "\n");
    }

    /// <summary>
    /// Set metaGame setting, isTraitor
    /// </summary>
    /// <param name="setting"></param>
    public void SetMetaGameTraitor(bool setting)
    {
        metaGameOptions.isTraitor = setting;
        Debug.LogFormat("[Met] MetaManager.cs -> SetMetaOptionTraitor: Option isTraitor now {0}{1}", setting, "\n");
    }

    /// <summary>
    /// Set metaGame setting, isLowMotivation
    /// </summary>
    /// <param name="setting"></param>
    public void SetMetaGameMotivation(bool setting)
    {
        metaGameOptions.isLowMotivation = setting;
        Debug.LogFormat("[Met] MetaManager.cs -> SetMetaOptionMotivation: Option isLowMotivation now {0}{1}", setting, "\n");
    }

    /// <summary>
    /// Set metaGame setting, isLevelTwo
    /// </summary>
    /// <param name="setting"></param>
    public void SetMetaGameLevelTwo(bool setting)
    {
        metaGameOptions.isLevelTwo = setting;
        Debug.LogFormat("[Met] MetaManager.cs -> SetMetaOptionLevelTwo: Option isLevelTwo now {0}{1}", setting, "\n");
    }

    /// <summary>
    /// Set metaGame setting, isLevelThree
    /// </summary>
    /// <param name="setting"></param>
    public void SetMetaGameLevelThree(bool setting)
    {
        metaGameOptions.isLevelThree = setting;
        Debug.LogFormat("[Met] MetaManager.cs -> SetMetaOptionLevelThree: Option isLevelThree now {0}{1}", setting, "\n");
    }

    //
    // - - - MetaEffectData
    //

    public MetaEffectData GetMetaEffectData()
    { return metaEffect; }

    //
    // - - - Collections
    //

    /// <summary>
    /// Update org's player in contact with at end of level prior to MetaGame process
    /// </summary>
    /// <param name="listOfOrgs"></param>
    public void SetMetaOrganisations(List<Organisation> listOfOrgs)
    {
        if (listOfOrgs != null)
        { listOfOrganisations.AddRange(listOfOrgs); }
        else { Debug.LogError("Invalid listOfOrganisations (Null)"); }
    }

    /// <summary>
    /// Update Player's secrets at end of level prior to MetaGame process
    /// </summary>
    /// <param name="listOfSecrets"></param>
    public void SetMetaSecrets(List<Secret> listOfSecrets)
    {
        if (listOfSecrets != null)
        { listOfSecrets.AddRange(listOfSecrets); }
        else { Debug.LogError("Invalid listOfSecrets (Null)"); }
    }

    /// <summary>
    /// Update current Investigations into the player at end of level prior to MetaGame process
    /// </summary>
    /// <param name="listOfInvestigations"></param>
    public void SetMetaInvestigations(List<Investigation> listOfInvestigations)
    {
        if (listOfInvestigations != null)
        { listOfInvestigations.AddRange(listOfInvestigations); }
        else { Debug.LogError("Invalid listOfInvestigationss (Null)"); }
    }

    //
    // - - - UI
    //

    public void InitialiseMetaOptions()
    {
        //
        // - - - Create MetaOptions
        //

        Dictionary<string, MetaOption> dictOfMetaOptions = GameManager.instance.dataScript.GetDictOfMetaOptions();
        if (dictOfMetaOptions != null)
        {
            bool isSuccess;
            string result;
            CriteriaDataInput data = new CriteriaDataInput();
            foreach(var metaOption in dictOfMetaOptions)
            {
                isSuccess = true;
                if (metaOption.Value != null)
                {
                    //isAlways false, depends on criteria
                    if (metaOption.Value.isAlways == false)
                    {
                        //check criteria (will ignore any org/secret/investigation metaOptions as these have isAlways false and NO criteria)
                        if (metaOption.Value.listOfCriteria.Count > 0)
                        {
                            data.listOfCriteria = metaOption.Value.listOfCriteria;
                            result = GameManager.instance.effectScript.CheckCriteria(data);
                            if (string.IsNullOrEmpty(result) == false)
                            {
                                isSuccess = false;
                                Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: option \"{0}\" FAILED ({1}){2}", metaOption.Key, result, "\n");
                            }
                        }
                        else
                        {
                            //isAlways false and no criteria - fail
                            isSuccess = false;
                            Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: option \"{0}\" FAILED (isAlways False and NO Criteria){1}", metaOption.Key, "\n");
                        }
                    }
                    //Add to list if viable
                    if (isSuccess == true)
                    {
                        listOfMetaOptions.Add(metaOption.Value);
                        Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: option \"{0}\" SUCCESS and added to list{1}", metaOption.Key, "\n");
                    }

                }
                else { Debug.LogWarningFormat("Invalid metaOption (Null) in dictOfMetaOptions for \"{0}\"", metaOption.Key); }
                
                // orgs/secrets/investigations -> dynamically create options, set dataName/dataTag for each, swap '*' symbol in text for dataTag

            }
            //nned to reset data -> MetaOption.Reset()

            
        }
        else { Debug.LogError("Invalid dictOfMetaOptions (Null)"); }
    }


    //new methods above here
}
