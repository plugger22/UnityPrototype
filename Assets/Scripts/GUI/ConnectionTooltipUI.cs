using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;
using System.Text;
using packageAPI;
using System;

public class ConnectionTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipDetails;
    //[HideInInspector] public int connID;
    [HideInInspector] public Connection connection;             //initialised in LevelManager.cs -> PlaceConnection

    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
    private RectTransform rectTransform;
    //private GameObject parent;
    //data derived whenever parent sprite moused over (OnPointerEnter)
    
    private GlobalSide side;

    private string colourSide;
    private string colourRebel;
    private string colourAuthority;
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourNormal;
    private string colourEnd;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// constructor -> needs to be Start as GameManager hasn't initialised prior to this (UI elements initialise before normal gameObjects?)
    /// </summary>
    private void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
        /*connection = GameManager.instance.dataScript.GetConnection(connID);*/
        if (connection == null) { Debug.LogError("Invalid connection (Null)"); }
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
    }

    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
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
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// Mouse Over event -> show tooltip if actor present in slot and their tooltipStatus > ActorTooltip.None
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        onMouseFlag = true;
        side = GameManager.instance.sideScript.PlayerSide;
        //show tooltip if connectorTooltip option is ON
        if (GameManager.instance.optionScript.connectorTooltips == true && connection != null)
        { StartCoroutine(ShowConnectionTooltip()); }
    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseFlag = false;
        StopCoroutine(ShowConnectionTooltip());
        GameManager.instance.tooltipGenericScript.CloseTooltip();
    }


    IEnumerator ShowConnectionTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over button
        if (onMouseFlag == true)
        {
            //do once
            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false)
            {
                colourSide = colourRebel;
                if (side.level == GameManager.instance.globalScript.sideAuthority.level)
                { colourSide = colourAuthority; }
                //security level
                string connText;
                switch(connection.SecurityLevel)
                {
                    case ConnectionType.HIGH:
                        connText = string.Format("{0}<b>HIGH</b>{1}", colourBad, colourEnd);
                        break;
                    case ConnectionType.MEDIUM:
                        connText = string.Format("{0}<b>MEDIUM</b>{1}", colourNeutral, colourEnd);
                        break;
                    case ConnectionType.LOW:
                        connText = string.Format("{0}<b>LOW</b>{1}", colourGood, colourEnd);
                        break;
                    case ConnectionType.None:
                        connText = string.Format("<b>NONE</b>");
                        break;
                    default:
                        connText = string.Format("<b>Unknown</b>");
                        break;
                }
                //debug data
                StringBuilder builderData = new StringBuilder();
                builderData.AppendFormat("{0}activityTimeKnown      {1}{2}{3}", colourNormal, connection.activityTurnKnown, colourEnd, "\n");
                builderData.AppendFormat("{0}activityTimePossible   {1}{2}{3}", colourNormal, connection.activityTurnPossible, colourEnd, "\n");
                builderData.AppendFormat("{0}activityCountKnown     {1}{2}{3}", colourNormal, connection.activityCountKnown, colourEnd, "\n");
                builderData.AppendFormat("{0}activityCountPossible  {1}{2}{3}", colourNormal, connection.activityCountPossible, colourEnd);
                //ongoing effects
                StringBuilder builderOngoing = new StringBuilder();
                /*List<EffectDataOngoing> listOfOngoingEffects = connection.GetAllOngoingEffects();
                if (listOfOngoingEffects != null & listOfOngoingEffects.Count > 0)
                {
                    foreach (EffectDataOngoing effect in listOfOngoingEffects)
                    {
                        if (builderOngoing.Length > 0) { builderOngoing.AppendLine(); }
                        if (String.IsNullOrEmpty(effect.text) == false)
                        { builderOngoing.AppendFormat("{0}effect.text{1}{2}", colourNeutral, colourEnd, "\n"); }
                        builderOngoing.AppendFormat("Security {0}{1}{2}{3}{4}", effect.value > 0 ? colourGood : colourBad, effect.value > 0 ? "+" : "", effect.value,
                            colourEnd, "\n");
                        builderOngoing.AppendFormat("For {0}{1}{2} more turns", colourNeutral, effect.timer, colourEnd);
                    }
                }
                else { builderOngoing.Append("No Ongoing effects present"); }*/
                //tooltip data
                tooltipHeader = string.Format("{0}Connection{1}{2}Security Level {3}", colourSide, colourEnd, "\n", connText);
                tooltipMain = builderData.ToString();
                tooltipDetails = builderOngoing.ToString();
                GameManager.instance.tooltipGenericScript.SetTooltip(tooltipMain, transform.position, tooltipHeader, tooltipDetails);
                yield return null;
            }
            //fade in
            float alphaCurrent;
            while (GameManager.instance.tooltipGenericScript.GetOpacity() < 1.0)
            {
                alphaCurrent = GameManager.instance.tooltipGenericScript.GetOpacity();
                alphaCurrent += Time.deltaTime / mouseOverFade;
                GameManager.instance.tooltipGenericScript.SetOpacity(alphaCurrent);
                yield return null;
            }
        }
    }

}
