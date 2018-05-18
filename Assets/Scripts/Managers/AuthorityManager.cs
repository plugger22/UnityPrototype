using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Handles all Authority related matters
/// </summary>
public class AuthorityManager : MonoBehaviour
{
    //fast access fields
    private GlobalSide globalAuthority;

    public void Initialise()
    {
        //fast acess fields
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
    }

    /// <summary>
    /// Debug method that Sets a mutually exclusive AuthorityState (enum). Default (no parameter) is to reset back to normal.
    /// </summary>
    /// <param name="state"></param>
    public string DebugSetAuthorityState(AuthorityState state = AuthorityState.Normal)
    {
        GameManager.instance.turnScript.authorityState = state;
        //message
        bool isPublic = false;
        string text = "Unknown";
        switch(state)
        {
            case AuthorityState.APB:
                isPublic = true;
                text = "Authorities issue a city wide All Points Bulletin";
                break;
            case AuthorityState.SecurityAlert:
                isPublic = true;
                text = "Authorities issue a city wide Security Alert";
                break;
            default:
                text = string.Format("AuthorityState reset to {0}", state);
                break;
        }
        Message message = GameManager.instance.messageScript.DecisionGlobal(text, 0, globalAuthority, isPublic);
        GameManager.instance.dataScript.AddMessage(message);
        return text;
    }


    
}
