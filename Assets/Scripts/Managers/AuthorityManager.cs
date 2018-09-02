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
    private GlobalSide globalBoth;

    public void Initialise()
    {
        //fast acess fields
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalBoth = GameManager.instance.globalScript.sideBoth;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalBoth != null, "Invalid globalBoth (Null)");
    }

    /// <summary>
    /// method that Sets a mutually exclusive AuthorityState (enum). Default (no parameter) is to reset back to normal.
    /// Descriptor is the decision.descriptor
    /// Method generates a DecisionGlobal message using descriptor
    /// </summary>
    /// <param name="state"></param>
    public bool SetAuthoritySecurityState(string descriptor, string warning, AuthoritySecurityState state = AuthoritySecurityState.Normal)
    {
        bool isDone = false;
        string itemText = "Unknown";
        if (string.IsNullOrEmpty(descriptor) == false)
        {
            //set state
            GameManager.instance.turnScript.authoritySecurityState = state;
            //message
            switch (state)
            {
                case AuthoritySecurityState.APB:
                case AuthoritySecurityState.SecurityAlert:
                case AuthoritySecurityState.SurveillanceCrackdown:
                    isDone = true;
                    itemText = string.Format("Authority implements a {0}", state);
                    break;
                default:
                    itemText = "Authority reverts SECURITY back to Normal";
                    break;
            }
            //message
            GameManager.instance.messageScript.DecisionGlobal(descriptor, itemText, warning, -1);
        }
        else { Debug.LogWarning("AuthorityManager.cs -> SetAuthorityState: Invalid descriptor (Null or empty)"); }
        return isDone;
    }



    
}
