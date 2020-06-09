using gameAPI;
using modalAPI;
using packageAPI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// handles all MetaLevel matters
/// </summary>
public class MetaManager : MonoBehaviour
{

    [Header("MetaOption core data")]
    [Tooltip("The max number of metaOption choices you can choose (UI restrictions displaying more than these, also good to have a cap)")]
    [Range(1, 10)] public int numOfChoices = 6;
    [Tooltip("Minimum amount of renown required before Recommendations (button) can be made in MetaGameUI")]
    [Range(0, 10)] public int renownRecommendMin = 2;
    [Tooltip("Maximum amount of Special Gear available during MetaGame (priority order of enum.ActorHQ.Boss on down)")]
    [Range(0, 3)] public int maxNumOfGear = 3;
    [Tooltip("Maximum amount of Interrogation Devices available during MetaGame (priority order of enum.ActorHQ.Boss on down")]
    [Range(0, 3)] public int maxNumOfDevices = 3;

    [Header("MetaOption Arrays")]
    [Tooltip("Place metaOptions here to handle the max number of possible organisation metaOptions that may be required")]
    public MetaOption[] arrayOfOrganisationOptions;
    [Tooltip("Place metaOptions here to handle the max number of possible secret metaOptions that may be required")]
    public MetaOption[] arrayOfSecretOptions;
    [Tooltip("Place metaOptions here to handle the max number of possible investigation metaOptions that may be required")]
    public MetaOption[] arrayOfInvestigationOptions;
    [Tooltip("Place metaOptions here to handle special gear from each member of the HQ hierarchy (1 for each), in order of hierarchy, eg. Boss first, subBoss1 second, etc.")]
    public MetaOption[] arrayOfGearOptions;
    [Tooltip("Place metaOptions here to handle interrogation devices for each member of HQ (1 for each), in order of Hierarchy, eg. Boss first, subBoss1 second, etc.")]
    public MetaOption[] arrayOfDeviceOptions;

    [Header("Renown cost of MetaOptions")]
    [Range(0, 10)] public int costLowPriority = 2;
    [Range(0, 10)] public int costMediumPriority = 4;
    [Range(0, 10)] public int costHighPriority = 6;
    [Range(0, 10)] public int costExtremePriority = 10;


    //NOTE: the above arrays are checked for various error states in ValidationManager.cs -> ValidateMetaOptions

    [HideInInspector] public GlobalMeta metaLevel;

    //data for compiling available metaOptions (sent at end of a level by ProcessMeta... methods in individual classes)
    private List<Organisation> listOfOrganisations = new List<Organisation>();
    private List<Secret> listOfSecrets = new List<Secret>();
    private List<Investigation> listOfInvestigations = new List<Investigation>();

    //MetaOptions to display 
    private MetaGameOptions metaGameOptions = new MetaGameOptions();
    private List<MetaOption> listOfMetaOptions = new List<MetaOption>();        //metaOptions to be converted to MetaData
    private MetaInfoData metaInfoData = new MetaInfoData();                     //package to send to MetaGameUI
    private TransitionInfoData transitionInfoData = new TransitionInfoData();   //package to send to TransitionUI

    private bool isTestLog;                                                     //enables toggling of [Tst] log messages

    public void Initialise(GameState state)
    {
        //set state
        metaLevel = GameManager.i.globalScript.metaBottom;
        isTestLog = GameManager.i.testScript.isMetaGame;
        /*metaEffect = new MetaEffectData();*/
        Debug.AssertFormat(arrayOfOrganisationOptions.Length == GameManager.i.loadScript.arrayOfOrgTypes.Length, "Invalid arrayOfOrganisationOptions (has {0} records, should be {1})",
            arrayOfOrganisationOptions.Length, GameManager.i.loadScript.arrayOfOrgTypes.Length);
        Debug.AssertFormat(arrayOfSecretOptions.Length == GameManager.i.secretScript.secretMaxNum, "Invalid arrayOfSecretOptions (has {0} records, should be {1})",
            arrayOfSecretOptions.Length, GameManager.i.secretScript.secretMaxNum);
        Debug.AssertFormat(arrayOfInvestigationOptions.Length == GameManager.i.playerScript.maxInvestigations, "Invalid arrayOfInvestigations (has {0} records, should be {1})",
            arrayOfInvestigationOptions.Length, GameManager.i.playerScript.maxInvestigations);
        Debug.AssertFormat(arrayOfGearOptions.Length == GameManager.i.hqScript.numOfActorsHQ, "Invalid arrayOfGearOptions (has {0} records, should be {1}",
            arrayOfGearOptions.Length, GameManager.i.hqScript.numOfActorsHQ);
        Debug.AssertFormat(arrayOfDeviceOptions.Length == GameManager.i.hqScript.numOfActorsHQ, "Invalid arrayOfDeviceOptions (has {0} records, should be {1})",
            arrayOfDeviceOptions.Length, GameManager.i.hqScript.numOfActorsHQ);
    }


    /// <summary>
    /// run at start of every metaGame
    /// </summary>
    public void InitialiseMetaGameOptions()
    {
        /*if (GameManager.i.testScript.isValidMetaOptions == true)
        {
            metaGameOptions.isDismissed = GameManager.i.testScript.isDismissed;
            metaGameOptions.isResigned = GameManager.i.testScript.isResigned;
            metaGameOptions.isLowMotivation = GameManager.i.testScript.isLowMotivation;
            metaGameOptions.isTraitor = GameManager.i.testScript.isTraitor;
            metaGameOptions.isLevelTwo = GameManager.i.testScript.isLevelTwo;
            metaGameOptions.isLevelThree = GameManager.i.testScript.isLevelThree;
        }
        else
        {
            //default -> level 1, include everybody
            metaGameOptions.isDismissed = true;
            metaGameOptions.isResigned = true;
            metaGameOptions.isLowMotivation = true;
            metaGameOptions.isTraitor = true;
            metaGameOptions.isLevelTwo = false;
        }*/
        metaGameOptions.isDismissed = true;
        metaGameOptions.isResigned = true;
        metaGameOptions.isLowMotivation = true;
        metaGameOptions.isTraitor = true;
        metaGameOptions.isLevelTwo = false;
        //Debug
        if (GameManager.i.testScript.bonusRenown > 0)
        {
            GameManager.i.playerScript.Renown += GameManager.i.testScript.bonusRenown;
            Debug.LogFormat("[Met] MetaManager.cs -> InitialiseMetaGame: Player gains {0} bonus Renown (Debug){1}", GameManager.i.testScript.bonusRenown, "\n");
        }
    }

    /// <summary>
    /// Metalevel master sequence
    /// </summary>
    public void ProcessMetaGame()
    {
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        InitialiseMetaGameOptions();
        ResetMetaAdmin();
        //hide top bar UI at start of meta game
        EventManager.i.PostNotification(EventType.TopBarHide, this, null, "MetaManager.cs -> Hide TopBarUI");
        GameManager.i.statScript.ProcessMetaStatistics();
        GameManager.i.topicScript.ProcessMetaTopics();
        GameManager.i.hqScript.ProcessMetaHq(playerSide);          //needs to be BEFORE MetaActors
        GameManager.i.actorScript.ProcessMetaActors(playerSide);
        GameManager.i.dataScript.ProcessMetaCures();
        GameManager.i.orgScript.ProcessMetaOrgs();
        GameManager.i.playerScript.ProcessMetaPlayer();
        //Transition data
        InitialiseEndLevel();
        InitialiseHQ();
        InitialisePlayerStatus();
        InitialiseBriefingOne();
        InitialiseBriefingTwo();
        //Player metaGame Options choice
        InitialiseMetaOptions();
        InitialiseMetaData();
        GameManager.i.metaUIScript.SetMetaInfoData(metaInfoData);
        GameManager.i.transitionScript.SetTransitionInfoData(transitionInfoData);

        //AutoSave -> Do so at now, with all processing done, so player, when save reloads, can complete MetaGame
        if (GameManager.i.isAutoSave == true)
        { GameManager.i.controlScript.ProcessAutoSave(GameState.MetaGame, RestorePoint.MetaTransition); }

        //confirm window that will open transition on closing
        ModalConfirmDetails details = new ModalConfirmDetails();
        details.topText = string.Format("You have been successfully {0} from {1}. There are a few things to take care of before your next mission",
            GameManager.Formatt("extracted", ColourType.moccasinText), GameManager.Formatt(GameManager.i.campaignScript.scenario.city.tag, ColourType.neutralText));
        details.bottomText = "Are you ready?";
        details.buttonFalse = "SAVE and EXIT";
        details.buttonTrue = "CONTINUE";
        details.eventFalse = EventType.SaveAndExit;
        details.eventTrue = EventType.TransitionOpen;
        details.modalState = ModalSubState.MetaGame;
        details.restorePoint = RestorePoint.MetaTransition;
        //open confirm
        EventManager.i.PostNotification(EventType.ConfirmOpen, this, details, "MetaManager.cs -> ProcessMetaGame");
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

    public MetaInfoData GetMetaInfoData()
    { return metaInfoData; }

    /// <summary>
    /// Used for Save/Load to recreate metaInfoData (not really needed but done for completion's sake)
    /// </summary>
    /// <param name="data"></param>
    public void SetMetaInfoData(MetaInfoData data)
    {
        if (data != null)
        { metaInfoData = data; }
        else { Debug.LogError("Invalid metaInfoData (Null)"); }
    }

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
    /// used for Save/Load 
    /// </summary>
    /// <param name="metaGameOptions"></param>
    public void SetMetaOptions(MetaGameOptions loadOptions)
    {
        if (loadOptions != null)
        {
            metaGameOptions.isDismissed = loadOptions.isDismissed;
            metaGameOptions.isResigned = loadOptions.isResigned;
            metaGameOptions.isLowMotivation = loadOptions.isLowMotivation;
            metaGameOptions.isTraitor = loadOptions.isTraitor;
            metaGameOptions.isLevelTwo = loadOptions.isLevelTwo;
        }
        else { Debug.LogError("Invalid loadOptions (Null)"); }
    }

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


    //
    // - - - Collections
    //

    /// <summary>
    /// Update org's player in contact with at end of level prior to MetaGame process
    /// </summary>
    /// <param name="tempList"></param>
    public void SetMetaOrganisations(List<Organisation> tempList)
    {
        if (tempList != null)
        { listOfOrganisations.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfOrganisations (Null)"); }
    }

    /// <summary>
    /// Update Player's secrets at end of level prior to MetaGame process
    /// </summary>
    /// <param name="tempList"></param>
    public void SetMetaSecrets(List<Secret> tempList)
    {
        if (tempList != null)
        { listOfSecrets.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfSecrets (Null)"); }
    }

    /// <summary>
    /// Update current Investigations into the player at end of level prior to MetaGame process
    /// </summary>
    /// <param name="tempList"></param>
    public void SetMetaInvestigations(List<Investigation> tempList)
    {
        if (tempList != null)
        { listOfInvestigations.AddRange(tempList); }
        else { Debug.LogError("Invalid listOfInvestigationss (Null)"); }
    }

    //
    // - - - MetaGameUI ->  populate metaInfoData package
    //

    /// <summary>
    /// Filter and update available MetaOptions and place in listOfMetaOptions ready for processing
    /// </summary>
    public void InitialiseMetaOptions()
    {
        // - - - Create MetaOptions
        Dictionary<string, MetaOption> dictOfMetaOptions = GameManager.i.dataScript.GetDictOfMetaOptions();
        if (dictOfMetaOptions != null)
        {
            int count, index, motivation;
            bool isSuccess;
            string result;
            CriteriaDataInput data = new CriteriaDataInput();
            //
            // - - - Normal
            //
            if (isTestLog)
            { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: NORMAL MetaOptions - - - {0}", "\n"); }
            foreach (var metaOption in dictOfMetaOptions)
            {
                isSuccess = true;
                if (metaOption.Value != null)
                {
                    //reset data
                    metaOption.Value.Reset();
                    //isAlways false, depends on criteria
                    if (metaOption.Value.isAlways == false)
                    {
                        //check criteria (will ignore any org/secret/investigation metaOptions as these have isAlways false and NO criteria)
                        if (metaOption.Value.listOfCriteria.Count > 0)
                        {
                            data.listOfCriteria = metaOption.Value.listOfCriteria;
                            result = GameManager.i.effectScript.CheckCriteria(data);
                            if (string.IsNullOrEmpty(result) == false)
                            {
                                isSuccess = false;
                                if (isTestLog)
                                { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: option \"{0}\" FAILED ({1}){2}", metaOption.Key, result, "\n"); }
                            }
                            else { metaOption.Value.isActive = true; }
                        }
                        else
                        {
                            //isAlways false and no criteria - fail (like this to handle specials, eg. all specials should fail this check and instead be handled by the Specials code segment)
                            isSuccess = false;
                            if (isTestLog)
                            { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: option \"{0}\" FAILED (isAlways False and NO Criteria){1}", metaOption.Key, "\n"); }
                        }
                    }
                    else
                    {
                        //isAlways true -> auto added to list but need to check any criteria to get a value for 'isActive'
                        if (metaOption.Value.listOfCriteria.Count > 0)
                        {
                            data.listOfCriteria = metaOption.Value.listOfCriteria;
                            result = GameManager.i.effectScript.CheckCriteria(data);
                            if (string.IsNullOrEmpty(result) == false)
                            { metaOption.Value.isActive = false; }
                            else { metaOption.Value.isActive = true; }
                        }
                        else { metaOption.Value.isActive = true; }
                    }
                    //Add to list if viable
                    if (isSuccess == true)
                    {
                        listOfMetaOptions.Add(metaOption.Value);
                        if (isTestLog)
                        { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: option \"{0}\" SUCCESS, isActive {1}, added to list{2}", metaOption.Key, metaOption.Value.isActive, "\n"); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid metaOption (Null) in dictOfMetaOptions for \"{0}\"", metaOption.Key); }
            }
            //
            // - - - Specials  Organisations / Secrets / Investigations / Gear / Interrogation Devices
            //
            if (isTestLog)
            { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: SPECIAl MetaOptions - - - {0}", "\n"); }
            //
            // - - - Organisations -> don't exceed max number of org options available
            //
            count = Mathf.Min(listOfOrganisations.Count, arrayOfOrganisationOptions.Length);
            if (count > 0)
            {
                index = 0;
                for (int i = 0; i < count; i++)
                {
                    Organisation org = listOfOrganisations[i];
                    if (org != null)
                    {
                        MetaOption metaSpecial = arrayOfOrganisationOptions[index];
                        metaSpecial.dataName = org.name;
                        metaSpecial.dataTag = org.tag;
                        metaSpecial.isActive = true;
                        //swap '*' for org.tag
                        metaSpecial.text = metaSpecial.template.Replace("*", org.tag); ;
                        index++;
                        //add to list
                        listOfMetaOptions.Add(metaSpecial);
                        if (isTestLog)
                        {
                            Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Org option \"{0}\", {1}, {2} added{3}", metaSpecial.name, metaSpecial.dataName, metaSpecial.dataTag, "\n");
                            Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Org option \"{0}\", {1}{2}", metaSpecial.name, metaSpecial.text, "\n");
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid organisation (Null) for listOfOrganisations[{0}]", i); }
                }
            }

            else
            {
                if (isTestLog)
                { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: No Organisations currently in contact with Player{0}", "\n"); }
            }
            //
            // - - - Secrets -> don't exceed max number of secret options available
            //
            count = Mathf.Min(listOfSecrets.Count, arrayOfSecretOptions.Length);
            if (count > 0)
            {
                index = 0;
                for (int i = 0; i < count; i++)
                {
                    Secret secret = listOfSecrets[i];
                    if (secret != null)
                    {
                        MetaOption metaSpecial = arrayOfSecretOptions[index];
                        metaSpecial.dataName = secret.name;
                        metaSpecial.dataTag = secret.tag;
                        metaSpecial.isActive = true;
                        //swap '*' for secret.tag
                        metaSpecial.text = metaSpecial.template.Replace("*", secret.tag);
                        index++;
                        //add to list
                        listOfMetaOptions.Add(metaSpecial);
                        if (isTestLog)
                        {
                            Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Secret option \"{0}\", {1}, {2} added{3}", metaSpecial.name, metaSpecial.dataName, metaSpecial.dataTag, "\n");
                            Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Secret option \"{0}\", {1}{2}", metaSpecial.name, metaSpecial.text, "\n");
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid secret (Null) for listOfSecrets[{0}]", i); }
                }
            }
            else
            {
                if (isTestLog)
                { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Player has no secrets{0}", "\n"); }
            }
            //
            // - - - Investigations -> don't exceed max number of investigation options available
            //
            count = Mathf.Min(listOfInvestigations.Count, arrayOfInvestigationOptions.Length);
            if (count > 0)
            {
                index = 0;
                for (int i = 0; i < count; i++)
                {
                    Investigation investigation = listOfInvestigations[i];
                    if (investigation != null)
                    {
                        MetaOption metaSpecial = arrayOfInvestigationOptions[index];
                        metaSpecial.dataName = investigation.reference;
                        metaSpecial.dataTag = investigation.tag;
                        metaSpecial.isActive = true;
                        //swap '*' for investigation.tag
                        metaSpecial.text = metaSpecial.template.Replace("*", investigation.tag);
                        index++;
                        //add to list
                        listOfMetaOptions.Add(metaSpecial);
                        if (isTestLog)
                        {
                            Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Investigation option \"{0}\", {1}, {2} added{3}", metaSpecial.name, metaSpecial.dataName, metaSpecial.dataTag, "\n");
                            Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Investigation option \"{0}\", {1}{2}", metaSpecial.name, metaSpecial.text, "\n");
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid Investigation (Null) for listOfInvestigations[{0}]", i); }
                }
            }
            else
            {
                if (isTestLog)
                { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Player has no outstanding investigations{0}", "\n"); }
            }
            //
            // - - - Special Gear -> offered if relevant HQ characters have a good opinion of you
            //
            count = arrayOfGearOptions.Length;
            if (count > 0)
            {
                //should be one for each HQ boss in hierarchy (assumed to be in order of hierarchy, eg. Boss -> SubBoss1 -> SubBoss2 -> SubBoss3
                if (count == GameManager.i.hqScript.numOfActorsHQ)
                {
                    int numOfGearOptions = 0;
                    Actor actor = null;
                    Gear gear = null;
                    for (int i = 0; i < count; i++)
                    {
                        ActorHQ actorHQ = ActorHQ.None;
                        switch (i)
                        {
                            case 0: actorHQ = ActorHQ.Boss; break;
                            case 1: actorHQ = ActorHQ.SubBoss1; break;
                            case 2: actorHQ = ActorHQ.SubBoss2; break;
                            case 3: actorHQ = ActorHQ.SubBoss3; break;
                        }
                        if (actorHQ != ActorHQ.None)
                        {
                            actor = GameManager.i.dataScript.GetHqHierarchyActor(actorHQ);
                            gear = GameManager.i.campaignScript.GetHqSpecialGear(actorHQ);
                            if (actor != null)
                            {
                                MetaOption metaSpecial = arrayOfGearOptions[i];
                                if (gear != null)
                                {
                                    metaSpecial.dataName = gear.name;
                                    metaSpecial.dataTag = gear.tag;
                                    motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                                    //option active and displayed only if actor has a good opinion of player
                                    if (motivation >= 2)
                                    {
                                        metaSpecial.isActive = true;
                                        numOfGearOptions++;
                                        //swap '*' for investigation.tag
                                        metaSpecial.text = metaSpecial.template.Replace("*", gear.tag);
                                        //customise descriptor
                                        metaSpecial.descriptor = string.Format("<b>{0}{1}{2}{3}</b>", GameManager.Formatt(gear.tag, ColourType.neutralText), "\n", "\n", gear.description);
                                        //modify cost according to relationship (Mot 3 -> use base cost, Mot 2 -> double base cost
                                        if (motivation == 2) { metaSpecial.relationshipModifier = 2; }
                                        else { metaSpecial.relationshipModifier = 1; }
                                        if (isTestLog)
                                        { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: \"{0}\", {1}, {2} Relationship Modifier {3}{4}",
                                              metaSpecial.name, metaSpecial.data, metaSpecial.dataTag, metaSpecial.relationshipModifier, "\n"); }
                                        //add to list
                                        listOfMetaOptions.Add(metaSpecial);
                                        if (isTestLog)
                                        {
                                            Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Gear option \"{0}\", {1}, {2} added{3}", metaSpecial.name, metaSpecial.dataName, metaSpecial.dataTag, "\n");
                                            Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Gear option \"{0}\", {1}{2}", metaSpecial.name, metaSpecial.text, "\n");
                                        }
                                        //check max number of allowed gear options hasn't been reached
                                        if (numOfGearOptions >= maxNumOfGear)
                                        {
                                            if (isTestLog)
                                            { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Max Cap on Gear Options reached (current {0}, max {1}){2}", numOfGearOptions, maxNumOfGear, "\n"); }
                                            break;
                                        }
                                    }
                                }
                                else { Debug.LogWarningFormat("Invalid gear (Null) for listOfGearOptions[{0}]", i); }
                            }
                            else { Debug.LogWarningFormat("Invalid actor (Null) for listOfGearOptions[{0}]", i); }
                        }
                        else { Debug.LogWarningFormat("Invalid actorHQ (None) for index {0}", i); }
                    }
                }
                else { Debug.LogWarningFormat("Incorrect number of arrayOfGear metaOptions (is {0}, should be {1})", count, GameManager.i.hqScript.numOfActorsHQ); }
            }
            else
            {
                if (isTestLog)
                { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Player has no gear MetaOptions in arrayOfGearOptions{0}", "\n"); }
            }
            //
            // - - - Interrogation Devices -> offered if relevant HQ characters have a good opinion of you
            //
            count = arrayOfDeviceOptions.Length;
            if (count > 0)
            {
                //double check one option for each hierarchy member
                if (count == GameManager.i.hqScript.numOfActorsHQ)
                {
                    int numOfDeviceOptions = 0;
                    Actor actor = null;
                    CaptureTool device = null;
                    for (int i = 0; i < count; i++)
                    {
                        ActorHQ actorHQ = ActorHQ.None;
                        switch (i)
                        {
                            case 0: actorHQ = ActorHQ.Boss; break;
                            case 1: actorHQ = ActorHQ.SubBoss1; break;
                            case 2: actorHQ = ActorHQ.SubBoss2; break;
                            case 3: actorHQ = ActorHQ.SubBoss3; break;
                        }
                        if (actorHQ != ActorHQ.None)
                        {
                            actor = GameManager.i.dataScript.GetHqHierarchyActor(actorHQ);
                            device = GameManager.i.captureScript.GetCaptureTool(actorHQ);
                            if (actor != null)
                            {
                                MetaOption metaSpecial = arrayOfDeviceOptions[i];
                                if (device != null)
                                {
                                    metaSpecial.data = device.innocenceLevel;
                                    metaSpecial.dataTag = device.tag;
                                    //option active and displayed only if actor has a good opinion of player
                                    motivation = actor.GetDatapoint(ActorDatapoint.Motivation1);
                                    if (motivation >= 2)
                                    {
                                        metaSpecial.isActive = true;
                                        numOfDeviceOptions++;
                                        //swap '*' for device.tag
                                        metaSpecial.text = metaSpecial.template.Replace("*", device.tag);
                                        //customise descriptor
                                        metaSpecial.descriptor = string.Format("<b>{0}{1}{2}{3}{4}{5}Innocence</b> {6}", GameManager.Formatt(device.tag, ColourType.neutralText), "\n", "\n",
                                            device.descriptor, "\n", "\n", GameManager.i.guiScript.GetNormalStars(device.innocenceLevel));
                                        //modify cost according to relationship (Mot 3 -> use base cost, Mot 2 -> double base cost
                                        if (motivation == 2)
                                        { metaSpecial.relationshipModifier = 2; }
                                        else { metaSpecial.relationshipModifier = 1; }
                                        if (isTestLog)
                                        { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: \"{0}\", {1}, {2} Relationship Modifier {3}{4}",
                                              metaSpecial.name, metaSpecial.data, metaSpecial.dataTag, metaSpecial.relationshipModifier, "\n"); }
                                        //add to list
                                        listOfMetaOptions.Add(metaSpecial);
                                        if (isTestLog)
                                        {
                                            Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Device option \"{0}\", {1}, {2} added{3}",
                                                metaSpecial.name, metaSpecial.data, metaSpecial.dataTag, "\n");
                                            Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Device option \"{0}\", {1}{2}", metaSpecial.name, metaSpecial.text, "\n");
                                        }
                                        //check max number of allowed gear options hasn't been reached
                                        if (numOfDeviceOptions >= maxNumOfDevices)
                                        {
                                            if (isTestLog)
                                            { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Max Cap on Device Options reached (current {0}, max {1}){2}",
                                                numOfDeviceOptions, maxNumOfDevices, "\n"); }
                                            break;
                                        }
                                    }
                                }
                                else { Debug.LogWarningFormat("Invalid Device (Null) for listOfDeviceOptions[{0}]", i); }
                            }
                            else { Debug.LogWarningFormat("Invalid actor (Null) for listOfDeviceOptions[{0}]", i); }
                        }
                        else { Debug.LogWarningFormat("Invalid actorHQ (None) for index {0}", i); }
                    }
                }
                else { Debug.LogWarningFormat("Incorrect number of arrayOfDevice metaOptions (is {0}, should be {1})", count, GameManager.i.hqScript.numOfActorsHQ); }
            }
            else
            {
                if (isTestLog)
                { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: Player has no device MetaOptions in arrayOfDeviceOptions{0}", "\n"); }
            }
        }
        else { Debug.LogError("Invalid dictOfMetaOptions (Null)"); }
    }

    /// <summary>
    /// Converts listOfMetaOptions into individual MetaData and packages up into a MetaInfoData package ready for MetaGameUI
    /// </summary>
    private void InitialiseMetaData()
    {
        //Take list of MetaData and populate MetaInfoData temp package
        metaInfoData.Reset();
        //process MetaOptions
        if (listOfMetaOptions != null)
        {
            int count = listOfMetaOptions.Count;
            int cost;
            string leader;
            if (count > 0)
            {
                //Convert MetaOptions to MetaData and place in a list
                int level = GameManager.i.sideScript.PlayerSide.level;
                List<MetaData> listOfMetaData = new List<MetaData>();
                for (int index = 0; index < count; index++)
                {
                    MetaOption metaOption = listOfMetaOptions[index];
                    if (metaOption != null)
                    {
                        MetaData metaData = new MetaData()
                        {
                            metaName = metaOption.name,
                            itemText = metaOption.text,
                            //topText = metaOption.header,
                            bottomText = metaOption.descriptor,
                            inactiveText = metaOption.textInactive,
                            sideLevel = level,
                            data = metaOption.data,
                            dataName = metaOption.dataName,
                            dataTag = metaOption.dataTag,
                            sprite = metaOption.sprite,
                            spriteName = metaOption.sprite.name,
                            isActive = metaOption.isActive,
                            isRecommended = metaOption.isRecommended,
                            isSelected = false,
                            help = 1,
                            tag0 = metaOption.help0,
                            tag1 = metaOption.help1,
                            tag2 = metaOption.help2,
                            tag3 = metaOption.help3,
                        };
                        //criteria
                        if (metaOption.listOfCriteria.Count > 0)
                        { metaData.isCriteria = true; }
                        else { metaData.isCriteria = false; }
                        //effects
                        metaData.listOfEffects.AddRange(metaOption.listOfEffects);
                        //priority and cost
                        cost = 0;
                        switch (metaOption.renownCost.level)
                        {
                            case 0: metaData.priority = MetaPriority.Low; cost = costLowPriority; break;
                            case 1: metaData.priority = MetaPriority.Medium; cost = costMediumPriority; break;
                            case 2: metaData.priority = MetaPriority.High; cost = costHighPriority; break;
                            case 3: metaData.priority = MetaPriority.Extreme; cost = costExtremePriority; break;
                            default: Debug.LogWarningFormat("Invalid metaOption.RenownCost.level \"{0}\" for metaOption {1}", metaOption.renownCost.level, metaOption.name); break;
                        }
                        //RenownCost (base cost * relationship modifier which is default 1 in case where this doesn't apply)
                        cost *= metaOption.relationshipModifier;
                        metaData.renownCost = cost;
                        //header texts
                        metaData.textSelect = $"Costs <size=130%>{GameManager.Formatt(cost.ToString(), ColourType.neutralText)}</size> Renown";
                        metaData.textDeselect = $"Gain <size=130%>{GameManager.Formatt(cost.ToString(), ColourType.neutralText)}</size> Renown";
                        metaData.textInsufficient = $"Not enough Renown (need <size=130%>{GameManager.Formatt(cost.ToString(), ColourType.neutralText)}</size>)";
                        //recommendation priority
                        if (metaOption.isRecommended == true)
                        {
                            switch (metaOption.recommendPriority.level)
                            {
                                case 0: metaData.recommendedPriority = MetaPriority.Low; break;
                                case 1: metaData.recommendedPriority = MetaPriority.Medium; break;
                                case 2: metaData.recommendedPriority = MetaPriority.High; break;
                                case 3: metaData.recommendedPriority = MetaPriority.Extreme; break;
                                default: Debug.LogWarningFormat("Invalid metaOption.recommendPriority.level \"{0}\" for metaOption {1}", metaOption.recommendPriority.level, metaOption.name); break;
                            }
                        }
                        //tab
                        switch (metaOption.hqPosition.level)
                        {
                            case 0: metaData.tabSide = MetaTabSide.Boss; break;
                            case 1: metaData.tabSide = MetaTabSide.SubBoss1; break;
                            case 2: metaData.tabSide = MetaTabSide.SubBoss2; break;
                            case 3: metaData.tabSide = MetaTabSide.SubBoss3; break;
                            default: Debug.LogWarningFormat("Invalid metaOption.hqPosition.level \"{0}\" for metaOption {1}", metaOption.hqPosition.level, metaOption.name); break;
                        }
                        listOfMetaData.Add(metaData);
                        //Player status metaOptions
                        if (metaOption.isPlayerStatus == true)
                        { metaInfoData.listOfStatusData.Add(metaData); }
                        //Recommended metaOptions
                        if (metaOption.isRecommended == true)
                        { metaInfoData.listOfRecommended.Add(metaData); }
                    }
                    else { Debug.LogWarningFormat("Invalid metaOption (Null) for listOfMetaOptions[{0}]", index); }
                }
                //sort listOfRecommended by RecommendedPriority
                if (metaInfoData.listOfRecommended.Count > 1)
                {
                    var ordered = from element in metaInfoData.listOfRecommended
                                  orderby element.recommendedPriority descending
                                  select element;
                    metaInfoData.listOfRecommended = ordered.ToList();
                }

                /*if (isTestLog)
                 {Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaData: metaInfoData.listOfStatusData has {0} records{1}", metaInfoData.listOfStatusData.Count, "\n");}*/

                for (int i = 0; i < listOfMetaData.Count; i++)
                {
                    MetaData metaData = listOfMetaData[i];
                    if (metaData != null)
                    { metaInfoData.AddMetaData(metaData); }
                    else { Debug.LogWarningFormat("Invalid metaData (Null) for listOfMetaData[{0}]", i); }
                }
                //temp lists for ordering by priority
                List<MetaData> listInactive = new List<MetaData>();
                List<MetaData> listLow = new List<MetaData>();
                List<MetaData> listMedium = new List<MetaData>();
                List<MetaData> listHigh = new List<MetaData>();
                List<MetaData> listExtreme = new List<MetaData>();
                for (int index = 0; index < metaInfoData.arrayOfMetaData.Length; index++)
                {
                    List<MetaData> tempList = metaInfoData.arrayOfMetaData[index];
                    //check if any tab 'page' is empty and, if so, add an explanatory metaData to page
                    if (tempList.Count == 0)
                    {
                        leader = GameManager.i.hqScript.GetHqTitle((ActorHQ)(index + 1));
                        //add default MetaData item
                        MetaData metaData = new MetaData()
                        {
                            metaName = "Default",
                            itemText = string.Format("Nothing available from your {0}", leader),
                            textSelect = "No Options",
                            bottomText = string.Format("Your {0}<br><br><b>{1}</b><br><br>have anything for you currently", leader, GameManager.Formatt("Does Not", ColourType.salmonText)),
                            sideLevel = level,
                            sprite = GameManager.i.guiScript.infoSprite,
                            spriteName = GameManager.i.guiScript.infoSprite.name,
                            isActive = false,
                            isRecommended = false,
                            isSelected = false,
                            isCriteria = false,
                            tabSide = (MetaTabSide)index,
                            priority = MetaPriority.Low,
                            help = 1,
                            tag0 = "test0"
                        };
                        metaInfoData.AddMetaData(metaData);
                    }
                    else
                    {
                        //Sort by Priority -> empty out lists prior to use
                        listInactive.Clear();
                        listLow.Clear();
                        listMedium.Clear();
                        listHigh.Clear();
                        //order page by priority, highest at the top
                        for (int i = 0; i < tempList.Count; i++)
                        {
                            MetaData metaData = tempList[i];
                            if (metaData != null)
                            {
                                if (metaData.isActive == false)
                                { listInactive.Add(metaData); }
                                else
                                {
                                    switch (metaData.priority)
                                    {
                                        case MetaPriority.Low: listLow.Add(metaData); break;
                                        case MetaPriority.Medium: listMedium.Add(metaData); break;
                                        case MetaPriority.High: listHigh.Add(metaData); break;
                                        case MetaPriority.Extreme: listExtreme.Add(metaData); break;
                                        default: Debug.LogWarningFormat("Invalid metaData.priority \"{0}\", for metaData {1}", metaData.priority, metaData.metaName); break;
                                    }
                                }
                            }
                            else { Debug.LogWarningFormat("Invalid metaData (Null) for tempList[{0}], arrayOfMetaData[{index}]", i, index); }
                        }
                        //assign sorted list back to main array
                        metaInfoData.arrayOfMetaData[index].Clear();
                        metaInfoData.arrayOfMetaData[index].AddRange(listExtreme);
                        metaInfoData.arrayOfMetaData[index].AddRange(listHigh);
                        metaInfoData.arrayOfMetaData[index].AddRange(listMedium);
                        metaInfoData.arrayOfMetaData[index].AddRange(listLow);
                        metaInfoData.arrayOfMetaData[index].AddRange(listInactive);
                    }
                }
                //defaults for top tabs
                if (metaInfoData.listOfStatusData.Count == 0)
                {
                    //default for status
                    MetaData metaDataStatus = new MetaData()
                    {
                        metaName = "Default",
                        itemText = string.Format("There are NO Ongoing options"),
                        textSelect = "No Ongoing Options ",
                        bottomText = "<b>There are no Conditions, Secrets, Investigations or contacts with Organisations that will carry over to the next city</b>",
                        sideLevel = level,
                        sprite = GameManager.i.guiScript.infoSprite,
                        spriteName = GameManager.i.guiScript.infoSprite.name,
                        isActive = false,
                        isRecommended = false,
                        isSelected = false,
                        isCriteria = false,
                        tabTop = MetaTabTop.Status,
                        priority = MetaPriority.Low,
                        help = 1,
                        tag0 = "test0"
                    };
                    metaInfoData.listOfStatusData.Add(metaDataStatus);
                }
                //create a default metaData for Selected (there'll be nothing selected at the start)
                MetaData metaDataSelected = new MetaData()
                {
                    metaName = "Default",
                    itemText = string.Format("NO options have currently been selected"),
                    textSelect = "No Options Selected",
                    bottomText = "<b>Any options that you select will be shown here</b>",
                    sideLevel = level,
                    sprite = GameManager.i.guiScript.infoSprite,
                    spriteName = GameManager.i.guiScript.infoSprite.name,
                    isActive = false,
                    isRecommended = false,
                    isSelected = false,
                    isCriteria = false,
                    tabTop = MetaTabTop.Selected,
                    priority = MetaPriority.Low,
                    help = 1,
                    tag0 = "test0"
                };
                metaInfoData.selectedDefault = metaDataSelected;
            }
            else { Debug.LogWarning("Invalid listOfMetaOptions (Empty)"); }
        }
        else { Debug.LogWarning("Invalid listOfMetaOptions (Null)"); }
    }

    //
    // - - - TransitionUI ->  populate transitionInfoData package
    //

    public TransitionInfoData GetTransitionInfoData()
    { return transitionInfoData; }

    /// <summary>
    /// populate EndLevel part of transitionInfoData package
    /// </summary>
    private void InitialiseEndLevel()
    {

    }

    /// <summary>
    /// populate HQ part of transitionInfoData package
    /// </summary>
    private void InitialiseHQ()
    {

    }

    /// <summary>
    /// populate PlayerStatus part of transitionInfoData package
    /// </summary>
    private void InitialisePlayerStatus()
    {

    }

    /// <summary>
    /// populate BriefingOne part of transitionInfoData package
    /// </summary>
    private void InitialiseBriefingOne()
    {

    }

    /// <summary>
    /// populate BriefingTwo part of transitionInfoData package
    /// </summary>
    private void InitialiseBriefingTwo()
    {

    }



    //new methods above here
}
