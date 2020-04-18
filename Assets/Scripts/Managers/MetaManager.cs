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



    public void Initialise(GameState state)
    {
        //set state
        metaLevel = GameManager.instance.globalScript.metaBottom;

        //debug metaGame options
        MetaGameOptions data = new MetaGameOptions();
        if (GameManager.instance.testScript.isValidTestMetaGameOptions == true)
        {
            data.isDismissed = GameManager.instance.testScript.isDismissed;
            data.isResigned = GameManager.instance.testScript.isResigned;
            data.isLowMotivation = GameManager.instance.testScript.isLowMotivation;
            data.isLevelTwo = GameManager.instance.testScript.isLevelTwo;
            data.isLevelThree = GameManager.instance.testScript.isLevelThree;
        }
        else
        {
            //default -> level 1, include everybody
            data.isDismissed = true;
            data.isResigned = true;
            data.isLowMotivation = true;
            data.isLevelTwo = false;
            data.isLevelThree = false;
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



}
