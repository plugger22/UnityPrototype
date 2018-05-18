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
    /// method that Sets a mutually exclusive AuthorityState (enum). Default (no parameter) is to reset back to normal.
    /// Descriptor is the decision.descriptor
    /// Method generates a DecisionGlobal message using descriptor
    /// </summary>
    /// <param name="state"></param>
    public bool SetAuthorityState(string descriptor, AuthorityState state = AuthorityState.Normal)
    {
        bool isPublic = false;
        bool isDone = false;
        if (string.IsNullOrEmpty(descriptor) == false)
        {
            //set state
            GameManager.instance.turnScript.authorityState = state;
            //message
            switch (state)
            {
                case AuthorityState.APB:
                case AuthorityState.SecurityAlert:
                case AuthorityState.SurvellianceCrackdown:
                    isPublic = true; isDone = true;
                    break;
                default:
                    break;
            }
            Message message = GameManager.instance.messageScript.DecisionGlobal(descriptor, 0, globalAuthority, isPublic);
            GameManager.instance.dataScript.AddMessage(message);
        }
        else { Debug.LogWarning("AuthorityManager.cs -> SetAuthorityState: Invalid descriptor (Null or empty)"); }
        return isDone;
    }



    
}
