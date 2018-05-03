using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// top screen centre widget
/// </summary>
public class WidgetTopUI : MonoBehaviour
{

    public Sprite widgetTopSprite;
    public TextMeshProUGUI actionPoints;


    private static WidgetTopUI widgetTopUI;


    public void Start()
    {
        //event listener
        EventManager.instance.AddListener(EventType.ChangeActionPoints, OnEvent);
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


    //new methods above here
}
