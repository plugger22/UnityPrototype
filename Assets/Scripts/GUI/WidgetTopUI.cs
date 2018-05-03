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
    public TextMeshProUGUI actionPoints;

    private RectTransform transformCity;
    private RectTransform transformFaction;
    private static WidgetTopUI widgetTopUI;


    public void Start()
    {
        //cache components
        transformCity = barCity.GetComponent<RectTransform>();
        transformFaction = barFaction.GetComponent<RectTransform>();
        Debug.Assert(transformCity != null, "Invalid transformCity (Null)");
        Debug.Assert(transformFaction != null, "Invalid transformFaction (Null)");
        //event listener
        EventManager.instance.AddListener(EventType.ChangeActionPoints, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeCityBar, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeFactionBar, OnEvent);
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
                UpdateActionPoints((int)Param);
                break;
            case EventType.ChangeCityBar:
                SetCityBar((int)Param);
                break;
            case EventType.ChangeFactionBar:
                SetFactionBar((int)Param);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Changes action point display
    /// </summary>
    /// <param name="points"></param>
    private void UpdateActionPoints(int points)
    {
        Debug.Assert(points > -1 && points < 5, "Invalid action points");
        actionPoints.text = Convert.ToString(points);
    }

    /// <summary>
    /// Sets length and colour of city bar based on size (%)
    /// </summary>
    /// <param name="size"></param>
    private void SetCityBar(int size)
    {
        Debug.Assert(size > -1 && size < 101, string.Format("Invalid size {0}", size));
        transformCity.sizeDelta = new Vector2(size, transformCity.sizeDelta.y);
    }

    /// <summary>
    /// Sets length and colour of faction bar based on size (%)
    /// </summary>
    /// <param name="size"></param>
    private void SetFactionBar(int size)
    {
        Debug.Assert(size > -1 && size < 101, string.Format("Invalid size {0}", size));
        transformFaction.sizeDelta = new Vector2(size, transformFaction.sizeDelta.y);
    }


    //new methods above here
}
