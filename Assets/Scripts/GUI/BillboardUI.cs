﻿using gameAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// End of turn Billboard UI prior to infoPipeline
/// </summary>
public class BillboardUI : MonoBehaviour
{
    public Canvas billCanvas;
    public GameObject billObject;
    public Image billLeft;
    public Image billRight;
    public Image billPanelOuter;
    public Image billPanelInner;
    public TextMeshProUGUI billText;

    private RectTransform billTransformLeft;
    private RectTransform billTransformRight;

    private float halfScreenWidth;
    /*private float width;*/
    private int speed;
    private int counter;
    private float distance;
    private bool isFading;
    private Color outerColour;
    private float flashNeon;
    private float offset;                   //offset distance to get panels off screen during development

    private static BillboardUI billboardUI;

    /// <summary>
    /// Static instance so the billboardUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static BillboardUI Instance()
    {
        if (!billboardUI)
        {
            billboardUI = FindObjectOfType(typeof(BillboardUI)) as BillboardUI;
            if (!billboardUI)
            { Debug.LogError("There needs to be one active billboardUI script on a GameObject in your scene"); }
        }
        return billboardUI;
    }


    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.StartUp:
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
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
        flashNeon = GameManager.i.guiScript.flashBillboardTime;
        Debug.Assert(flashNeon > 0.0f, "Invalid flashNeon (Zero)");
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        Debug.Assert(billCanvas != null, "Invalid billCanvas (Null)");
        Debug.Assert(billObject != null, "Invalid billObject (Null)");
        Debug.Assert(billLeft != null, "Invalid billLeft (Null)");
        Debug.Assert(billRight != null, "Invalid billRight (Null)");
        Debug.Assert(billPanelInner != null, "Invalid billPanel (Null)");
        Debug.Assert(billPanelOuter != null, "Invalid billPanel (Null)");
        Debug.Assert(billText != null, "Invalid billText (Null)");
        //initialise components
        billTransformLeft = billLeft.GetComponent<RectTransform>();
        billTransformRight = billRight.GetComponent<RectTransform>();
        Debug.Assert(billTransformLeft != null, "Invalid billTransformLeft (Null)");
        Debug.Assert(billTransformRight != null, "Invalid billTransformRight (Null)");
        //initialise billboard
        InitialiseBillboard();
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.i.AddListener(EventType.BillboardClose, OnEvent, "BillboardUI");
    }
    #endregion

    #endregion

    /// <summary>
    /// Event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.BillboardClose:
                CloseBillboard();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// Set billboard parameters and start position
    /// </summary>
    private void InitialiseBillboard()
    {
        outerColour = billPanelOuter.color;
        flashNeon = 1.0f;
        //measurements
        halfScreenWidth = Screen.width / 2;
        offset = 135;
        distance = halfScreenWidth + offset;
        speed = 15;
        //activate
        billCanvas.gameObject.SetActive(true);
        //Reset panels at start
        ResetBillboard();
        /*Debug.LogFormat("[Tst] BillboardUI.cs -> halfScreenWidth {0}, panelWidth {1}, distance {2}{3}", halfScreenWidth, width, distance, "\n");*/
    }

    /// <summary>
    /// reset panels just off screen
    /// </summary>
    public void ResetBillboard()
    {
        billPanelOuter.gameObject.SetActive(false);
        billPanelInner.gameObject.SetActive(false);
        billLeft.transform.localPosition = new Vector3(-distance, 0, 0);
        billRight.transform.localPosition = new Vector3(distance, 0, 0);
        Debug.LogFormat("[UI] BillboardUI.cs -> Reset: Reset Billboard{0}", "\n");
    }

    /// <summary>
    /// Main billboard controller
    /// </summary>
    public void RunBillboard()
    {
        Debug.LogFormat("[UI] BillboardUI.cs -> RunBillboard: Start Billboard{0}", "\n");
        string displayText = GameManager.i.newsScript.GetAdvert();
        StartCoroutine("BillOpen", displayText);
    }

    /// <summary>
    /// coroutine to slide panels together then display billboard (strobes the neon border)
    /// </summary>
    /// <returns></returns>
    private IEnumerator BillOpen(string textToDisplay)
    {
        counter = 0;
        billText.text = textToDisplay;
        GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.Billboard });
        while (counter < distance)
        {
            counter += speed;
            billLeft.transform.localPosition = new Vector3(-distance + counter, 0, 0);
            billRight.transform.localPosition = new Vector3(distance - counter, 0, 0);
            yield return null;
        }
        billPanelInner.gameObject.SetActive(true);
        billPanelOuter.gameObject.SetActive(true);
        //indefinitely strobe outer panel (cyan neon borders)
        isFading = true;
        while (true)
        {
            outerColour = billPanelOuter.color;
            if (isFading == false)
            {
                outerColour.a += Time.deltaTime / flashNeon;
                if (outerColour.a >= 1.0f)
                { isFading = true; }
            }
            else
            {
                outerColour.a -= Time.deltaTime / flashNeon;
                if (outerColour.a <= 0.0f)
                { isFading = false; }
            }
            billPanelOuter.color = outerColour;
            yield return null;
        }
    }


    /// <summary>
    /// Close billboard controller
    /// </summary>
    public void CloseBillboard()
    {
        Debug.LogFormat("[UI] BillboardUI.cs -> CloseBillboard: Close Billboard{0}", "\n");
        StartCoroutine("BillClose");
    }

    /// <summary>
    /// Slides billboard panels back out of view after turning off billboard display
    /// </summary>
    /// <returns></returns>
    private IEnumerator BillClose()
    {
        counter = Convert.ToInt32(halfScreenWidth);
        GameManager.i.inputScript.ResetStates();
        billPanelInner.gameObject.SetActive(false);
        billPanelOuter.gameObject.SetActive(false);
        while (counter > 0)
        {
            counter -= speed;
            billLeft.transform.localPosition = new Vector3(-distance + counter, 0, 0);
            billRight.transform.localPosition = new Vector3(distance - counter, 0, 0);
            yield return null;
        }

    }


        //events above here
}
