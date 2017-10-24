using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using gameAPI;

/// <summary>
/// Handles the alert text UI at screen top centre (just below the Cause) which gives a message stating which nodes are being highlighted
/// </summary>
public class AlertUI : MonoBehaviour
{
    public GameObject alertObject;                                      //provides the text alert at top of map for ShowNodes
    public TextMeshProUGUI alertText;

    private static AlertUI alertUI;

    /// <summary>
    /// provide a static reference to the AlertUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static AlertUI Instance()
    {
        if (!alertUI)
        {
            alertUI = FindObjectOfType(typeof(AlertUI)) as AlertUI;
            if (!alertUI)
            { Debug.LogError("There needs to be one active AlertUI script on a GameObject in your scene"); }
        }
        return alertUI;
    }

    /// <summary>
    /// display AlertUI with appropriate text
    /// </summary>
    /// <param name="text"></param>
    public void SetAlertUI(string text)
    {
        if (alertObject != null)
        {
            if (alertText != null)
            {
                if (string.IsNullOrEmpty(text) == false)
                {
                    alertText.text = text;
                    //show on screen
                    alertObject.SetActive(true);
                    Debug.Log(string.Format("UI: Open -> AlertUI{0}", "\n"));
                }
                else { Debug.LogError(string.Format("Invalid text parameter (Null or empty){0}", "\n")); }
            }
            else { Debug.LogError(string.Format("Invalid alertText (Null){0}", "\n")); }
        }
        else { Debug.LogError(string.Format("Invalid GameObject alertObject (Null){0}", "\n")); }
    }

    /// <summary>
    /// close AlertUI by deactivating UI. resetFlag activates a whole node reset if true
    /// </summary>
    public void CloseAlertUI(bool resetFlag = false)
    {
        Debug.Log("UI: Close -> AlertUI" + "\n");
        alertObject.SetActive(false);
        GameManager.instance.nodeScript.NodeShowFlag = 0;
        if (resetFlag == true)
        {
            //redraw to remove highlighted nodes
            EventManager.instance.PostNotification(EventType.NodeDisplay, this, NodeUI.Reset);
        }
    }
}
