﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using gameAPI;
using UnityEngine.UI;

/// <summary>
/// Handles the alert text UI at screen top centre (just below the Cause) which gives a message stating which nodes are being highlighted
/// </summary>
public class AlertUI : MonoBehaviour
{
    public Canvas alertCanvas;
    public GameObject alertObject;                                      //provides the text alert at top of map for ShowNodes
    public Image alertBackground;
    public TextMeshProUGUI alertText;

    private Coroutine myCoroutine;
    private float timeToDisplay;                                        //how long the Alert will stay on screen (there is a global default and a manual override). Auto leaves if closed.
    private float timeDefault;                                          //default time to display alert

    private static AlertUI alertUI;

    public void OnEnable()
    {
        Debug.Assert(alertCanvas != null, "Invalid alertCanvas (Null)");
        Debug.Assert(alertObject != null, "Invalid alertObject (Null)");
        Debug.Assert(alertBackground != null, "Invalid alertBackground (Null)");
        Debug.Assert(alertText != null, "Invalid alertText (Null)");
    }

    public void Start()
    {
        timeDefault = GameManager.i.guiScript.alertDefaultTime;
        Debug.Assert(timeDefault > 0, "Invalid timeDefault (must be > Zero)");
        //set default gfx status
        alertCanvas.gameObject.SetActive(false);
        alertObject.gameObject.SetActive(true);
        alertBackground.gameObject.SetActive(true);
        alertText.gameObject.SetActive(true);
    }

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
    /// display AlertUI with appropriate text. Time is how long alert will stay on screen. Leave at default '0' for the default timer value to be used (it's greater than zero)
    /// </summary>
    /// <param name="text"></param>
    public void SetAlertUI(string text, float timeForAlertToShow = 0)
    {
        if (alertObject != null)
        {
            if (alertText != null)
            {
                if (string.IsNullOrEmpty(text) == false)
                {
                    alertText.text = text;
                    //show on screen
                    alertCanvas.gameObject.SetActive(true);
                    //start coroutine for set time
                    timeToDisplay = timeForAlertToShow;
                    if (timeToDisplay == 0f)
                    { timeToDisplay = timeDefault; }
                    myCoroutine = StartCoroutine("AlertTimer");
                    Debug.LogFormat("[UI] AlertUI.cs -> SetAlertUI{0}", "\n");
                }
                else { Debug.LogErrorFormat("Invalid text parameter (Null or empty){0}", "\n"); }
            }
            else { Debug.LogErrorFormat("Invalid alertText (Null){0}", "\n"); }
        }
        else { Debug.LogErrorFormat("Invalid GameObject alertObject (Null){0}", "\n"); }
    }

    /// <summary>
    /// close AlertUI by deactivating UI. resetFlag activates a whole node reset if true
    /// </summary>
    public void CloseAlertUI(bool resetFlag = false)
    {
        if (alertCanvas.gameObject.activeSelf == true)
        {
            Debug.LogFormat("[UI] AlertUI.cs -> CloseAlertUI{0}", "\n");
            alertCanvas.gameObject.SetActive(false);
            GameManager.i.nodeScript.NodeShowFlag = 0;
            //stop coroutine if still running
            if (myCoroutine != null)
            {
                StopCoroutine(myCoroutine);
                myCoroutine = null;
                //flashing nodes -> auto stop at same time
                if (GameManager.i.nodeScript.CheckFlashingNodes() == true)
                { EventManager.i.PostNotification(EventType.FlashNodesStop, this, null, "AlertUI.cs -> CloseAlertUI"); }
            }
            if (resetFlag == true)
            {
                //redraw to remove highlighted nodes
                EventManager.i.PostNotification(EventType.NodeDisplay, this, NodeUI.Reset, "AlertUI.cs -> CloseAlertUI");
            }
        }
    }

    /// <summary>
    /// Coroutine to ensure Alert is displayed for a set period before disappearing
    /// </summary>
    /// <returns></returns>
    private IEnumerator AlertTimer()
    {
        yield return new WaitForSeconds(timeToDisplay);
        CloseAlertUI();
    }

    /// <summary>
    /// returns true if AlertUI active, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool CheckAlertUI()
    { return myCoroutine == null ? false : true; }

    //new methods above here
}
