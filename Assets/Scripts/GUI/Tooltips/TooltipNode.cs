﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using gameAPI;
using packageAPI;


/// <summary>
/// Node based tooltip
/// </summary>
public class TooltipNode : MonoBehaviour
{
    public TextMeshProUGUI nodeName;
    public TextMeshProUGUI nodeType;
    public TextMeshProUGUI nodeActive;
    public TextMeshProUGUI nodeStatsFixed;
    public TextMeshProUGUI nodeStatsVar;
    public TextMeshProUGUI ongoingEffects;
    public TextMeshProUGUI nodeTeams;
    public TextMeshProUGUI nodeTarget;

    public Image dividerTop;                   //Side specific sprites for tooltips
    public Image dividerUpperMiddle;
    public Image dividerLowerMiddle;
    public Image dividerStats;
    public Image dividerBottom;

    public GameObject tooltipNodeObject;

    private Image background;
    private static TooltipNode tooltipNode;
    RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float fadeInTime;
    private int offset;



    //colour Palette
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourActive;
    private string colourDefault;
    private string colourEnd;
    private string colourTeam;

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        canvasGroup = tooltipNodeObject.GetComponent<CanvasGroup>();
        rectTransform = tooltipNodeObject.GetComponent<RectTransform>();
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
        offset = GameManager.instance.tooltipScript.tooltipOffset;
        //event listener
        EventManager.instance.AddListener(EventType.ChangeColour, this.OnEvent);
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
    }

    /// <summary>
    /// provide a static reference to tooltipNode that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TooltipNode Instance()
    {
        if (!tooltipNode)
        {
            tooltipNode = FindObjectOfType(typeof(TooltipNode)) as TooltipNode;
            if (!tooltipNode)
            { Debug.LogError("There needs to be one active TooltipNode script on a GameObject in your scene"); }
        }
        return tooltipNode;
    }

    //Called when events happen
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.ChangeSide:
                InitialiseTooltip((GlobalSide)Param);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Set colour palette for tooltip
    /// </summary>
    public void SetColours()
    {
        //output colours for good and bad depend on player side
        switch (GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Resistance":
                colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
                colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
                break;
            case "Authority":
                colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
                colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
                break;
            default:
                Debug.LogError(string.Format("Invalid playerSide \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                break;
        }
        //colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.dataNeutral);
        //colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourActive = GameManager.instance.colourScript.GetColour(ColourType.nodeActive);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourTeam = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// Initialise node Tool tip
    /// </summary>
    /// <param name="name">Name of Node, eg. 'Downtown Manhattan'</param>
    /// <param name="type">Type of Node, eg. Sprawl, government, corporate (auto converted to CAPS)</param>
    /// <param name="listOfActive">place actor type here if node is active for them, eg. 'Hacker'. No limit to actors</param>
    /// <param name="arrayOfStats">Give stats as Ints[3] in order Stability - Support - Security</param>
    /// <param name="listOfTeams">List of Authority teams present at node</param>
    /// <param name="listOfTarget">place target info here, a blank list if none</param>
    /// <param name="pos">Position of tooltip originator -> Use a transform position (world units), not an Input.MousePosition (screen units)</param>
    //public void SetTooltip(string name, string type, List<string> listOfActive, int[] arrayOfStats, List<string> listOfTeams, List<string> listOfTarget, Vector3 pos)
    public void SetTooltip(NodeTooltipData data)
    {
        bool proceedFlag;
        GlobalSide globalResistance = GameManager.instance.globalScript.sideResistance;
        GlobalSide globalAuthority = GameManager.instance.globalScript.sideAuthority;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //open panel at start
        tooltipNodeObject.SetActive(true);
        //set opacity to zero (invisible)
        SetOpacity(0f);
        //set state of all items in tooltip window
        nodeName.gameObject.SetActive(true);
        nodeType.gameObject.SetActive(true);
        nodeStatsVar.gameObject.SetActive(true);
        nodeStatsFixed.gameObject.SetActive(true);
        ongoingEffects.gameObject.SetActive(false);
        dividerTop.gameObject.SetActive(true);
        //show only if node active for at least one actor
        nodeActive.gameObject.SetActive(false);
        dividerUpperMiddle.gameObject.SetActive(false);
        dividerLowerMiddle.gameObject.SetActive(true);
        dividerStats.gameObject.SetActive(false);
        //show only if node has a target
        nodeTarget.gameObject.SetActive(true);
        dividerBottom.gameObject.SetActive(true);
        //set up tooltipNode object
        nodeName.text = string.Format("{0}{1}{2}", colourDefault, data.nodeName, colourEnd);
        nodeType.text = string.Format("{0}{1}{2}", colourDefault, data.type, colourEnd);
        //
        // - - - Actor Contacts - - -
        //
        //set tooltip elements to show
        nodeActive.gameObject.SetActive(true);
        dividerUpperMiddle.gameObject.SetActive(true);
        //build string
        StringBuilder builderActor = new StringBuilder();
        builderActor.Append(colourActive);
        //ascertain whether actors shown or not
        proceedFlag = false;
        if (playerSide.level == globalResistance.level) { proceedFlag = true; }
        else if (playerSide.level == globalAuthority.level)
        {
            if (GameManager.instance.optionScript.fogOfWar == true)
            {
                if (data.isActorKnown == true)
                { proceedFlag = true; }
            }
            else { proceedFlag = true; }
        }
        //contacts are present
        if (data.listOfActive.Count > 0)
        {
            //FOW off or Resistance side
            if (proceedFlag == true)
            {
                if (playerSide.level == globalResistance.level)
                {
                    for (int i = 0; i < data.listOfActive.Count; i++)
                    {
                        if (i > 0) { builderActor.AppendLine(); }
                        builderActor.Append(string.Format("{0}{1}{2}", colourNeutral, data.listOfActive[i], colourEnd));
                    }
                }
                else if (playerSide.level == globalAuthority.level)
                { builderActor.Append(string.Format("{0}Resistance Contacts present{1}", colourNeutral, colourEnd)); }
            }
            //FOW On and Authority player has no knowledge of actor contacts at node
            else
            { builderActor.Append(string.Format("{0}Resistance Contacts unknown{1}", colourBad, colourEnd)); }
        }
        else
        {
            //no actor contacts present at node -> FOW off or Resistance side
            if (proceedFlag == true)
            {
                if (playerSide.level == globalResistance.level)
                { builderActor.Append(string.Format("{0}<size=90%>No Actors have Contacts</size>{1}", colourDefault, colourEnd)); }
                else if (playerSide.level == globalAuthority.level)
                { builderActor.Append(string.Format("{0}<size=90%>No Resistance Contacts present</size>{1}", colourDefault, colourEnd)); }
            }
            //FOW On and Authority player has no knowledge of actor contacts at node
            else
            { builderActor.Append(string.Format("{0}Resistance Contacts unknown{1}", colourBad, colourEnd)); }
        }
        nodeActive.text = builderActor.ToString();
        //
        // - - - Ongoing effects - - - 
        //
        if (data.listOfEffects.Count > 0)
        {
            string effectText = "Unknown";
            ongoingEffects.gameObject.SetActive(true);
            dividerStats.gameObject.SetActive(true);
            StringBuilder effectBuilder = new StringBuilder();
            effectBuilder.Append(string.Format("{0}Ongoing Effects{1}", colourDefault, colourEnd));
            for (int i = 0; i < data.listOfEffects.Count; i++)
            {
                switch(data.listOfEffects[i].type.name)
                {
                    case "Good":
                        effectText = string.Format("{0}{1}{2}", colourGood, data.listOfEffects[i].text, colourEnd);
                        break;
                    case "Neutral":
                        effectText = string.Format("{0}{1}{2}", colourNeutral, data.listOfEffects[i].text, colourEnd);
                        break;
                    case "Bad":
                        effectText = string.Format("{0}{1}{2}", colourBad, data.listOfEffects[i].text, colourEnd);
                        break;
                    default:
                        effectText = string.Format("{0}{1}{2}", colourDefault, data.listOfEffects[i].text, colourEnd);
                        Debug.LogError(string.Format("Invalid ongoingEffect.type \"{0}\"", data.listOfEffects[i].type.name));
                        break;
                }
                effectBuilder.AppendLine();
                effectBuilder.Append(effectText);
            }
            ongoingEffects.text = effectBuilder.ToString();
        }
        else { ongoingEffects.text = ""; }
        //
        // - - - Teams  - - -
        //
        proceedFlag = false;
        if (playerSide.level == globalAuthority.level) { proceedFlag = true; }
        else if (playerSide.level == globalResistance.level)
        {
            if (GameManager.instance.optionScript.fogOfWar == true)
            {
                if (data.isTracerActive == true || data.isActor == true || data.isTeamKnown == true)
                { proceedFlag = true; }
            }
            else { proceedFlag = true; }
        }
        //show teams show only (Resistance) if node within tracer coverage or actor has a contact there or isTeamKnown true (if FOW option 'true')
        if (proceedFlag == true)
        {
            if (data.listOfTeams.Count > 0)
            {
                StringBuilder teamBuilder = new StringBuilder();
                foreach (String teamText in data.listOfTeams)
                {
                    if (teamBuilder.Length > 0) { teamBuilder.AppendLine(); }
                    teamBuilder.Append(string.Format("{0}{1}{2}", colourTeam, teamText, colourEnd));
                }
                nodeTeams.text = teamBuilder.ToString();
            }
            else { nodeTeams.text = string.Format("{0}{1}{2}", colourDefault, "<size=90%>No Teams present</size>", colourEnd); }
        }
        else { nodeTeams.text = string.Format("{0}Team Info unavailable{1}{2}{3}<size=90%>requires Tracer or Actor</size>{4}", colourBad, colourEnd, "\n", colourDefault, colourEnd); }
        //
        // - - Target - - -
        //
        if (data.listOfTargets.Count > 0)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(colourDefault);
            //foreach (string target in listOfTarget)
            for (int i = 0; i < data.listOfTargets.Count; i++)
            {
                if (i > 0) { builder.AppendLine(); }
                builder.Append(data.listOfTargets[i]);
            }
            builder.Append(colourEnd);
            nodeTarget.text = builder.ToString();
        }
        else
        { nodeTarget.text = string.Format("{0}{1}{2}", colourDefault, "<size=90%>No Target present</size>", colourEnd); }
        //
        // - - - Stats - - - 
        //
        int checkCounter = 0;
        int statData;
        if (data.arrayOfStats.Length > 0)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < data.arrayOfStats.Length; i++)
            {
                statData = data.arrayOfStats[i];
                if (i > 0) { builder.AppendLine(); }
                builder.Append(string.Format("{0}{1}{2}", GameManager.instance.colourScript.GetValueColour(statData), statData, colourEnd));
                //idiot check to handle case of being too many stats
                checkCounter++;
                if (checkCounter >= 3) { break; }
            }
            nodeStatsVar.text = builder.ToString();
            nodeStatsFixed.text = string.Format("{0}{1}\n{2}\n{3}{4}", colourDefault, "Stability", "Support", "Security", colourEnd);
        }

        //convert coordinates
        Vector3 screenPos = Camera.main.WorldToScreenPoint(data.tooltipPos);
        Canvas.ForceUpdateCanvases();
        rectTransform = tooltipNodeObject.GetComponent<RectTransform>();
        //get dimensions of dynamic tooltip
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        //calculate offset - height (default above)
        if (screenPos.y + height + offset < Screen.height)
        { screenPos.y += height / 2 + offset; }
        else { screenPos.y -= height / 2 + offset; }
        //width - default centred
        if (screenPos.x + width / 2 >= Screen.width)
        { screenPos.x -= width / 2 + screenPos.x - Screen.width; }
        else if (screenPos.x - width / 2 <= 0)
        { screenPos.x += width / 2 - screenPos.x; }
        //set new position
        tooltipNodeObject.transform.position = screenPos;
        Debug.Log(string.Format("UI: Open -> TooltipNode \"{0}\"{1}", data.nodeName, "\n"));
    }

    /// <summary>
    /// fade in tooltip over time
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeInTooltip()
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime / fadeInTime;
            yield return null;
        }
    }

    /// <summary>
    /// returns true if tooltip is active and visible
    /// </summary>
    /// <returns></returns>
    public bool CheckTooltipActive()
    { return tooltipNodeObject.activeSelf; }

    /// <summary>
    /// close tool tip
    /// </summary>
    public void CloseTooltip()
    {
        Debug.Log("UI: Close -> TooltipNode" + "\n");
        tooltipNodeObject.SetActive(false);
    }


    public void SetOpacity(float opacity)
    { canvasGroup.alpha = opacity; }

    public float GetOpacity()
    { return canvasGroup.alpha; }


    /// <summary>
    /// set up sprites on node tooltip for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    public void InitialiseTooltip(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = tooltipNodeObject.GetComponent<Image>();
        //assign side specific sprites
        switch (side.name)
        {
            case "Authority":
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundAuthority;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerStats.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerUpperMiddle.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerLowerMiddle.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerAuthority;
                break;
            case "Resistance":
                background.sprite = GameManager.instance.sideScript.toolTip_backgroundRebel;
                dividerTop.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerStats.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerUpperMiddle.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerLowerMiddle.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                dividerBottom.sprite = GameManager.instance.sideScript.toolTip_dividerRebel;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side.name));
                break;
        }
    }

}
