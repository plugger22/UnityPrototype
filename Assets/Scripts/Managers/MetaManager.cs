using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// handles all MetaLevel matters
/// </summary>
public class MetaManager : MonoBehaviour
{
    [HideInInspector] public GlobalMeta metaLevel;



    public void Initialise(GameState state)
    {
        
        //set state
        metaLevel = GameManager.instance.globalScript.metaBottom;
    }

    /// <summary>
    /// returns official authority title
    /// </summary>
    /// <returns></returns>
    public AuthorityTitle GetAuthorityTitle()
    { return (AuthorityTitle)(metaLevel.level); }

    /// <summary>
    /// Metalevel master sequence
    /// </summary>
    public void ProcessMetaGame()
    {
        //statistics
        GameManager.instance.statScript.ProcessMetaStatistics();
    }



}
