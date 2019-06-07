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
    private int securityAPB;
    private int securityAlert;
    private int securityCrackdown;

    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                break;
            case GameState.FollowOnInitialisation:
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access fields
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalBoth = GameManager.instance.globalScript.sideBoth;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalBoth != null, "Invalid globalBoth (Null)");
        //decisions
        securityAPB = GameManager.instance.dataScript.GetAIDecisionID("APB");
        securityAlert = GameManager.instance.dataScript.GetAIDecisionID("SecAlert");
        securityCrackdown = GameManager.instance.dataScript.GetAIDecisionID("SurvCrackdwn");
        Debug.Assert(securityAPB > -1, "Invalid securityAPB (-1)");
        Debug.Assert(securityAlert > -1, "Invalid securityAlert (-1)");
        Debug.Assert(securityCrackdown > -1, "Invalid securityCrackdown (-1)");
    }
    #endregion

    #endregion

    /// <summary>
    /// method that Sets a mutually exclusive AuthorityState (enum). Default (no parameter) is to reset back to normal.
    /// Descriptor is the decision.descriptor
    /// Method generates a DecisionGlobal message using descriptor
    /// </summary>
    /// <param name="state"></param>
    public bool SetAuthoritySecurityState(string descriptor, string warning, AuthoritySecurityState state = AuthoritySecurityState.Normal)
    {
        bool isDone = false;
        int decID = -1;
        string itemText = "Unknown";
        if (string.IsNullOrEmpty(descriptor) == false)
        {
            //set state
            GameManager.instance.turnScript.authoritySecurityState = state;
            //message
            switch (state)
            {
                case AuthoritySecurityState.APB:
                    isDone = true;
                    itemText = "Authority implements an ALL POINTS BULLETIN";
                    decID = securityAPB;
                    break;
                case AuthoritySecurityState.SecurityAlert:
                    isDone = true;
                    itemText = "Authority implements a SECURITY ALERT";
                    decID = securityAlert;
                    break;
                case AuthoritySecurityState.SurveillanceCrackdown:
                    isDone = true;
                    itemText = "Authority implements a SURVEILLANCE CRACKDOWN";
                    decID = securityCrackdown;
                    break;
                default:
                    itemText = "Authority reverts SECURITY back to Normal";
                    break;
            }
            //message
            GameManager.instance.messageScript.DecisionGlobal(descriptor, itemText, warning, decID);
        }
        else { Debug.LogWarning("AuthorityManager.cs -> SetAuthorityState: Invalid descriptor (Null or empty)"); }
        return isDone;
    }




}
