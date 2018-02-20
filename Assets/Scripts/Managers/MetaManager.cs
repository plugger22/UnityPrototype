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



    public void Initialise()
    {
        
        //set state
        metaLevel = GameManager.instance.globalScript.metaBottom;
    }

    /// <summary>
    /// returns official authority title
    /// </summary>
    /// <returns></returns>
    public AuthorityActor GetAuthorityTitle()
    { return (AuthorityActor)(metaLevel.level); }

}
