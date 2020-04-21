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

    private MetaGameOptions metaOptions;
    private MetaEffectData metaEffect;


    public void Initialise(GameState state)
    {
        //set state
        metaLevel = GameManager.instance.globalScript.metaBottom;
        metaEffect = new MetaEffectData();
        //debug metaGame options
        metaOptions = new MetaGameOptions();
        if (GameManager.instance.testScript.isValidMetaOptions == true)
        {
            metaOptions.isDismissed = GameManager.instance.testScript.isDismissed;
            metaOptions.isResigned = GameManager.instance.testScript.isResigned;
            metaOptions.isLowMotivation = GameManager.instance.testScript.isLowMotivation;
            metaOptions.isTraitor = GameManager.instance.testScript.isTraitor;
            metaOptions.isLevelTwo = GameManager.instance.testScript.isLevelTwo;
            metaOptions.isLevelThree = GameManager.instance.testScript.isLevelThree;
        }
        else
        {
            //default -> level 1, include everybody
            metaOptions.isDismissed = true;
            metaOptions.isResigned = true;
            metaOptions.isLowMotivation = true;
            metaOptions.isTraitor = true;
            metaOptions.isLevelTwo = false;
            metaOptions.isLevelThree = false;
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
    { return metaOptions; }

    /// <summary>
    /// Set metaGame setting, isDismissed
    /// </summary>
    /// <param name="setting"></param>
    public void SetMetaGameDismissed(bool setting)
    {
        metaOptions.isDismissed = setting;
        Debug.LogFormat("[Met] MetaManager.cs -> SetMetaOptionDismissed: Option isDismissed now {0}{1}", setting, "\n");
    }

    /// <summary>
    /// Set metaGame setting, isResigned
    /// </summary>
    /// <param name="setting"></param>
    public void SetMetaGameResigned(bool setting)
    {
        metaOptions.isResigned = setting;
        Debug.LogFormat("[Met] MetaManager.cs -> SetMetaOptionResigned: Option isResigned now {0}{1}", setting, "\n");
    }

    /// <summary>
    /// Set metaGame setting, isTraitor
    /// </summary>
    /// <param name="setting"></param>
    public void SetMetaGameTraitor(bool setting)
    {
        metaOptions.isTraitor = setting;
        Debug.LogFormat("[Met] MetaManager.cs -> SetMetaOptionTraitor: Option isTraitor now {0}{1}", setting, "\n");
    }

    /// <summary>
    /// Set metaGame setting, isLowMotivation
    /// </summary>
    /// <param name="setting"></param>
    public void SetMetaGameMotivation(bool setting)
    {
        metaOptions.isLowMotivation = setting;
        Debug.LogFormat("[Met] MetaManager.cs -> SetMetaOptionMotivation: Option isLowMotivation now {0}{1}", setting, "\n");
    }

    /// <summary>
    /// Set metaGame setting, isLevelTwo
    /// </summary>
    /// <param name="setting"></param>
    public void SetMetaGameLevelTwo(bool setting)
    {
        metaOptions.isLevelTwo = setting;
        Debug.LogFormat("[Met] MetaManager.cs -> SetMetaOptionLevelTwo: Option isLevelTwo now {0}{1}", setting, "\n");
    }

    /// <summary>
    /// Set metaGame setting, isLevelThree
    /// </summary>
    /// <param name="setting"></param>
    public void SetMetaGameLevelThree(bool setting)
    {
        metaOptions.isLevelThree = setting;
        Debug.LogFormat("[Met] MetaManager.cs -> SetMetaOptionLevelThree: Option isLevelThree now {0}{1}", setting, "\n");
    }

    /// <summary>
    /// Metalevel master sequence
    /// </summary>
    public void ProcessMetaGame()
    {
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
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
