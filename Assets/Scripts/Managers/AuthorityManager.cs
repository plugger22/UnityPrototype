using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Handles all Authority related matters
/// </summary>
public class AuthorityManager : MonoBehaviour
{

    public void Initialise()
    {
        
    }

    /// <summary>
    /// Sets a mutually exclusive AuthorityState (enum). Default (no parameter) is to reset back to normal.
    /// </summary>
    /// <param name="state"></param>
    public void SetAuthorityState(AuthorityState state = AuthorityState.Normal)
    {
        GameManager.instance.turnScript.authorityState = state;
        //message

    }


    
}
