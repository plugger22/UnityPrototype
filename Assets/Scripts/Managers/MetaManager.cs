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
    [HideInInspector] public GlobalMeta metaLevel;

    private MetaGameOptions metaGameOptions;
    private MetaEffectData metaEffect;


    public void Initialise(GameState state)
    {
        //set state
        metaLevel = GameManager.instance.globalScript.metaBottom;
        metaEffect = new MetaEffectData();
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
    /// returns official authority title
    /// </summary>
    /// <returns></returns>
    public AuthorityTitle GetAuthorityTitle()
    { return (AuthorityTitle)(metaLevel.level); }


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

    /// <summary>
    /// Metalevel master sequence
    /// </summary>
    public void ProcessMetaGame()
    {
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        InitialiseMetaGameOptions();
        //hide top bar UI at start of meta game
        EventManager.instance.PostNotification(EventType.TopBarHide, this, null, "MetaManager.cs -> Hide TopBarUI");
        GameManager.instance.statScript.ProcessMetaStatistics();
        GameManager.instance.topicScript.ProcessMetaTopics();
        GameManager.instance.hqScript.ProcessMetaHq(playerSide);          //needs to be BEFORE MetaActors
        GameManager.instance.actorScript.ProcessMetaActors(playerSide);
        GameManager.instance.dataScript.ProcessMetaCures();
        //show top bar UI at completion of meta game
        EventManager.instance.PostNotification(EventType.TopBarShow, this, null, "MetaManager.cs -> Show TopBarUI");
    }


    public MetaEffectData GetMetaEffectData()
    { return metaEffect; }


}
