﻿using gameAPI;
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

    [Header("MetaOption Arrays")]
    [Tooltip("Place metaOptions here to handle the max number of possible organisation metaOptions that may be required")]
    public MetaOption[] arrayOfOrganisationOptions;
    [Tooltip("Place metaOptions here to handle the max number of possible secret metaOptions that may be required")]
    public MetaOption[] arrayOfSecretOptions;
    [Tooltip("Place metaOptions here to handle the max number of possible investigation metaOptions that may be required")]
    public MetaOption[] arrayOfInvestigationOptions;

    [Header("Renown cost of MetaOptions")]
    [Range(0, 10)] public int costLowPriority = 2;
    [Range(0, 10)] public int costMediumPriority = 4;
    [Range(0, 10)] public int costHighPriority = 6;
    [Range(0, 10)] public int costExtremePriority = 10;
    

    //NOTE: the above arrays are checked for various error states in ValidationManager.cs -> ValidateMetaOptions

    [HideInInspector] public GlobalMeta metaLevel;

    private MetaGameOptions metaGameOptions;

    //data for compiling available metaOptions (sent at end of a level by ProcessMeta... methods in individual classes)
    private List<Organisation> listOfOrganisations = new List<Organisation>();
    private List<Secret> listOfSecrets = new List<Secret>();
    private List<Investigation> listOfInvestigations = new List<Investigation>();

    //MetaOptions to display
    private List<MetaOption> listOfMetaOptions = new List<MetaOption>();        //metaOptions to be converted to MetaData
    private MetaInfoData metaInfoData = new MetaInfoData();                     //package to send to MetaGameUI

    private bool isTestLog;                                                     //enables toggling of [Tst] log messages

    public void Initialise(GameState state)
    {
        //set state
        metaLevel = GameManager.i.globalScript.metaBottom;
        isTestLog = GameManager.i.testScript.isMetaManager;
        /*metaEffect = new MetaEffectData();*/
    }


    /// <summary>
    /// run at start of every metaGame
    /// </summary>
    public void InitialiseMetaGameOptions()
    {
        //debug metaGame options
        metaGameOptions = new MetaGameOptions();
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
        EventManager.instance.PostNotification(EventType.TopBarHide, this, null, "MetaManager.cs -> Hide TopBarUI");
        GameManager.i.statScript.ProcessMetaStatistics();
        GameManager.i.topicScript.ProcessMetaTopics();
        GameManager.i.hqScript.ProcessMetaHq(playerSide);          //needs to be BEFORE MetaActors
        GameManager.i.actorScript.ProcessMetaActors(playerSide);
        GameManager.i.dataScript.ProcessMetaCures();
        GameManager.i.orgScript.ProcessMetaOrgs();
        GameManager.i.playerScript.ProcessMetaPlayer();


        //Player metaGame Options choice
        InitialiseMetaOptions();
        InitialiseMetaData();
        GameManager.i.metaUIScript.SetMetaUI(metaInfoData);
        

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
    // - - - UI
    //

    /// <summary>
    /// Filter and update available MetaOptions and place in listOfMetaOptions ready for processing
    /// </summary>
    public void InitialiseMetaOptions()
    {
        //
        // - - - Create MetaOptions
        //

        Dictionary<string, MetaOption> dictOfMetaOptions = GameManager.i.dataScript.GetDictOfMetaOptions();
        if (dictOfMetaOptions != null)
        {
            int count, index;
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
            // - - - Specials -> Organisations / Secrets / Investigations
            //
            if (isTestLog)
            { Debug.LogFormat("[Tst] MetaManager.cs -> InitialiseMetaOptions: SPECIAl MetaOptions - - - {0}", "\n"); }
            //Organisations -> don't exceed max number of org options available
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
            //Secrets -> don't exceed max number of secret options available
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

            //Investigations -> don't exceed max number of investigation options available
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
        }
        else { Debug.LogError("Invalid dictOfMetaOptions (Null)"); }
    }

    /// <summary>
    /// Converts listOfMetaOptions into idividual MetaData and packages up into a MetaInfoData package ready for MetaGameUI
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
                            dataName = metaOption.dataName,
                            dataTag = metaOption.dataTag,
                            sprite = metaOption.sprite,
                            isActive = metaOption.isActive,
                            isRecommended = metaOption.isRecommended,
                            isSelected = false,
                            help = 1,
                            tag0 = "test0"
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
                            case 3: metaData.priority = MetaPriority.Extreme; cost = costExtremePriority;  break;
                            default: Debug.LogWarningFormat("Invalid metaOption.RenownCost.level \"{0}\" for metaOption {1}", metaOption.renownCost.level, metaOption.name); break;
                        }
                        //header texts
                        metaData.textSelect = $"Costs <size=130%>{GameManager.GetFormattedString(cost.ToString(), ColourType.neutralText)}</size> Renown";
                        metaData.textDeselect = $"Gain <size=130%>{GameManager.GetFormattedString(cost.ToString(), ColourType.neutralText)}</size> Renown";
                        metaData.textInsufficient = $"Not enough Renown (need <size=130%>{GameManager.GetFormattedString(cost.ToString(), ColourType.neutralText)}</size>)";
                        //RenownCost
                        metaData.renownCost = cost;
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
                            bottomText = string.Format("Your {0}<br><br><b>Does Not</b><br><br>have anything for you currently", leader),
                            sideLevel = level,
                            sprite = GameManager.i.guiScript.infoSprite,
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

    //new methods above here
}
