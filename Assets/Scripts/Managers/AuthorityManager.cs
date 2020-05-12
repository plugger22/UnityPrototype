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
    private string securityAPB;
    private string securityAlert;
    private string securityCrackdown;

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
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        //fast access fields
        globalAuthority = GameManager.i.globalScript.sideAuthority;
        globalBoth = GameManager.i.globalScript.sideBoth;
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(globalBoth != null, "Invalid globalBoth (Null)");
        //decisions
        securityAPB = "APB";
        securityAlert = "SecAlert";
        securityCrackdown = "SurvCrackdwn";
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
        string decName;
        string itemText = "Unknown";
        if (string.IsNullOrEmpty(descriptor) == false)
        {
            //set state
            GameManager.i.turnScript.authoritySecurityState = state;
            //message
            switch (state)
            {
                case AuthoritySecurityState.APB:
                    isDone = true;
                    itemText = "Authority implements an ALL POINTS BULLETIN";
                    decName = securityAPB;
                    break;
                case AuthoritySecurityState.SecurityAlert:
                    isDone = true;
                    itemText = "Authority implements a SECURITY ALERT";
                    decName = securityAlert;
                    break;
                case AuthoritySecurityState.SurveillanceCrackdown:
                    isDone = true;
                    itemText = "Authority implements a SURVEILLANCE CRACKDOWN";
                    decName = securityCrackdown;
                    break;
                default:
                    itemText = "Authority reverts SECURITY back to Normal";
                    decName = "NORMAL";  //has no effect but isn't Null or Empty which would trigger an Assert in MessageManager.cs -> DecisionGlobal
                    break;
            }
            //message
            GameManager.i.messageScript.DecisionGlobal(descriptor, itemText, warning, decName);
        }
        else { Debug.LogWarning("AuthorityManager.cs -> SetAuthorityState: Invalid descriptor (Null or empty)"); }
        return isDone;
    }




}
