using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles tooltip for player name in ActorUI
/// </summary>
public class PlayerTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private int offset;
    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
    private RectTransform rectTransform;
    private GameObject parent;

    private string colourNeutral;
    private string colourAlert;
    private string colourEnd;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parent = transform.parent.gameObject;
    }


    /// <summary>
    /// constructor -> needs to be Start as GameManager hasn't initialised prior to this (UI elements initialise before normal gameObjects?)
    /// </summary>
    private void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
        //halve fade in time as a canvas tool tip appears to be a lot slower than a scene one
        mouseOverFade /= 2;
        offset = GameManager.instance.tooltipScript.tooltipOffset;
        //event listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
    }


    /// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// set colour palette for Generic Tool tip
    /// </summary>
    public void SetColours()
    {
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }




    /// <summary>
    /// Mouse Over event -> Shows Player tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        onMouseFlag = true;
        StartCoroutine(ShowPlayerTooltip());

    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseFlag = false;
        StopCoroutine(ShowPlayerTooltip());
        GameManager.instance.tooltipPlayerScript.CloseTooltip();
    }

    /// <summary>
    /// Player tooltip
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowPlayerTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over player text
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.instance.tooltipPlayerScript.CheckTooltipActive() == false)
            {
                GameManager.instance.tooltipPlayerScript.SetTooltip(parent.transform.position);
                yield return null;
            }
            //fade in
            float alphaCurrent;
            while (GameManager.instance.tooltipPlayerScript.GetOpacity() < 1.0)
            {
                alphaCurrent = GameManager.instance.tooltipPlayerScript.GetOpacity();
                alphaCurrent += Time.deltaTime / mouseOverFade;
                GameManager.instance.tooltipPlayerScript.SetOpacity(alphaCurrent);
                yield return null;
            }
        }
    }



}
