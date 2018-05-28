﻿using gameAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// top screen centre widget
/// </summary>
public class WidgetTopUI : MonoBehaviour
{

    public Sprite widgetTopSprite;
    public Image barCity;
    public Image barFaction;
    public Image starLeft;
    public Image starMiddle;
    public Image starRight;
    public Image flashRed;
    public TextMeshProUGUI actionPoints;
    public TextMeshProUGUI turnNumber;

    private RectTransform transformCity;
    private RectTransform transformFaction;
    private bool isFading;                                  //flashing red security measure indicator, if true then opacity fading, otherwise increasing
    private Coroutine myCoroutine;

    private static WidgetTopUI widgetTopUI;


    public void Start()
    {
        //cache components
        transformCity = barCity.GetComponent<RectTransform>();
        transformFaction = barFaction.GetComponent<RectTransform>();
        Debug.Assert(transformCity != null, "Invalid transformCity (Null)");
        Debug.Assert(transformFaction != null, "Invalid transformFaction (Null)");
        //event listener
        EventManager.instance.AddListener(EventType.ChangeActionPoints, OnEvent, "WidgetTopUI");
        EventManager.instance.AddListener(EventType.ChangeCityBar, OnEvent, "WidgetTopUI");
        EventManager.instance.AddListener(EventType.ChangeFactionBar, OnEvent, "WidgetTopUI");
        EventManager.instance.AddListener(EventType.ChangeStarLeft, OnEvent, "WidgetTopUI");
        EventManager.instance.AddListener(EventType.ChangeStarMiddle, OnEvent, "WidgetTopUI");
        EventManager.instance.AddListener(EventType.ChangeStarRight, OnEvent, "WidgetTopUI");
        EventManager.instance.AddListener(EventType.ChangeTurn, OnEvent, "WidgetTopUI");
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "WidgetTopUI");
        EventManager.instance.AddListener(EventType.StartSecurityFlash, OnEvent, "WidgetTopUI");
        EventManager.instance.AddListener(EventType.StopSecurityFlash, OnEvent, "WidgetTopUI");
    }


    /// <summary>
    /// provide a static reference to widgetTopUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static WidgetTopUI Instance()
    {
        if (!widgetTopUI)
        {
            widgetTopUI = FindObjectOfType(typeof(WidgetTopUI)) as WidgetTopUI;
            if (!widgetTopUI)
            { Debug.LogError("There needs to be one active WidgetTopUI script on a GameObject in your scene"); }
        }
        return widgetTopUI;
    }


    public void Initialise()
    {
        //get correct number of action points
        SetActionPoints(GameManager.instance.turnScript.GetActionsTotal());
        //red flash opacity set to 0
        Color tempColor = flashRed.color;
        tempColor.a = 0.0f;
        flashRed.color = tempColor;
        isFading = false;
        //dim down objective stars
        SetStar(10f, UIPosition.Left);
        SetStar(10f, UIPosition.Middle);
        SetStar(10f, UIPosition.Right);
    }

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
            case EventType.ChangeActionPoints:
                SetActionPoints((int)Param);
                break;
            case EventType.ChangeTurn:
                SetTurn((int)Param);
                break;
            case EventType.ChangeSide:
                ChangeSides((GlobalSide)Param);
                break;
            case EventType.ChangeCityBar:
                SetCityBar((int)Param);
                break;
            case EventType.ChangeFactionBar:
                SetFactionBar((int)Param);
                break;
            case EventType.ChangeStarLeft:
                SetStar((float)Param, UIPosition.Left);
                break;
            case EventType.ChangeStarMiddle:
                SetStar((float)Param, UIPosition.Middle);
                break;
            case EventType.ChangeStarRight:
                SetStar((float)Param, UIPosition.Right);
                break;
            case EventType.StartSecurityFlash:
                SetSecurityFlash(true);
                break;
            case EventType.StopSecurityFlash:
                SetSecurityFlash(false);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// Set city loyalty bar colour for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    private void ChangeSides(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        SetCityBar(GameManager.instance.cityScript.CityLoyalty);
        SetActionPoints(GameManager.instance.turnScript.GetActionsTotal());
        switch(side.level)
        {
            case 1:
                SetFactionBar(GameManager.instance.factionScript.SupportAuthority);
                break;
            case 2:
                SetFactionBar(GameManager.instance.factionScript.SupportResistance);
                break;
        }
    }


    /// <summary>
    /// Changes action point display
    /// </summary>
    /// <param name="points"></param>
    private void SetActionPoints(int points)
    {
        Debug.Assert(points > -1 && points < 5, "Invalid action points");
        actionPoints.text = Convert.ToString(points);
    }

    /// <summary>
    /// Changes Turn number display
    /// </summary>
    /// <param name="points"></param>
    private void SetTurn(int turn)
    {
        Debug.Assert(turn > -1, "Invalid Turn number");
        turnNumber.text = Convert.ToString(turn);
    }

    /// <summary>
    /// Sets length and colour of city bar based on size (0 to 10)
    /// </summary>
    /// <param name="size"></param>
    private void SetCityBar(int size)
    {
        Debug.Assert(size > -1 && size < 11, string.Format("Invalid size {0}", size));
        float floatSize = (float)size;
        transformCity.sizeDelta = new Vector2(floatSize * 10f, transformCity.sizeDelta.y);
        //set colour
        float factor = floatSize * 0.1f;
        //City Loyalty color depends on side (100% is green for Authority and red for Resistance)
        switch (GameManager.instance.sideScript.PlayerSide.level)
        {
            case 1:
                barCity.color = new Color(2.0f * (1 - factor), 1.0f * factor, 0);
                break;
            case 2:
                barCity.color = new Color(2.0f * factor, 1.0f * (1 - factor), 0);
                break;
        }
    }

    /// <summary>
    /// Sets length and colour of faction bar based on size (0 to 10)
    /// </summary>
    /// <param name="size"></param>
    private void SetFactionBar(int size)
    {
        Debug.Assert(size > -1 && size < 11, string.Format("Invalid size {0}", size));
        float floatSize = (float)size;
        transformFaction.sizeDelta = new Vector2(floatSize * 10f, transformFaction.sizeDelta.y);
        //set colour
        float factor = floatSize * 0.1f;
        barFaction.color = new Color(2.0f * (1 - factor), 2.0f * factor, 0);
    }

    /// <summary>
    /// Change opacity (float, NOT int) of 3 Objective stars at top centre
    /// </summary>
    /// <param name="opacity"></param>
    /// <param name="uiPosition"></param>
    private void SetStar(float opacity, UIPosition uiPosition)
    {
        Debug.Assert(opacity >= 0 && opacity <= 100f, string.Format("Invalid opacity \"{0}\"", opacity));
        //convert opacity to 0 to 1.0
        opacity *= 0.01f;
        //change opacity of relevant star
        switch(uiPosition)
        {
            case UIPosition.Left:
                Color tempLeftColor = starLeft.color;
                tempLeftColor.a = opacity;
                starLeft.color = tempLeftColor;
                break;
            case UIPosition.Middle:
                Color tempMiddleColor = starMiddle.color;
                tempMiddleColor.a = opacity;
                starMiddle.color = tempMiddleColor;
                break;
            case UIPosition.Right:
                Color tempRightColor = starRight.color;
                tempRightColor.a = opacity;
                starRight.color = tempRightColor;
                break;
            default:
                Debug.LogWarningFormat("Invalid UIPosition \"{0}\"", uiPosition);
                break;
        }

    }


    /// <summary>
    /// Start (true) or Stop (false) the Action button red flash indicating that Security Measures are in place
    /// </summary>
    /// <param name="isStart"></param>
    private void SetSecurityFlash(bool isStart)
    {
        switch (isStart)
        {
            case true:
                myCoroutine = StartCoroutine("ShowFlashRed");
                break;
            case false:
                if (myCoroutine != null)
                { StopCoroutine(myCoroutine); }
                isFading = false;
                //reset opacity back to zero
                Color tempColor = flashRed.color;
                tempColor.a = 0.0f;
                flashRed.color = tempColor;
                break;
        }
    }


    /// <summary>
    /// Coroutine to display a flashing red alarm at action point display indicating Security Measures in place
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowFlashRed()
    {
        //infinite while loop
        while (true)
        {
            Color tempColor = flashRed.color;
            if (isFading == false)
            {
                tempColor.a += Time.deltaTime / 2.0f;
                if (tempColor.a >= 1.0f)
                { isFading = true; }
            }
            else
            {
                tempColor.a -= Time.deltaTime / 2.0f;
                if (tempColor.a <= 0.0f)
                { isFading = false; }
            }
            flashRed.color = tempColor;
            yield return null;
        }
    }


    //new methods above here
}
