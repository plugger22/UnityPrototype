﻿using gameAPI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player based tooltip, static reference instance in GameManager
/// </summary>
public class TooltipPlayer : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerStatus;
    public TextMeshProUGUI playerConditions;
    public TextMeshProUGUI playerStats;
    public TextMeshProUGUI playerMulti_1;       //multipurpose section 1
    public TextMeshProUGUI playerMulti_2;       //multipurpose section 2
    public Image divider_1;                     //numbered from top to bottom
    public Image divider_2;
    public Image divider_3;
    public Image divider_4;
    public Image divider_5;
    public GameObject tooltipPlayerObject;
    public Canvas tooltipPlayerCanvas;

    private Image background;
    private static TooltipPlayer tooltipPlayer;
    RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float fadeInTime;
    private int offset;

    private string[] arrayOfIcons = new string[3];

    //colours
    private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourName;
    private string colourAlert;
    private string colourGrey;
    private string colourMint;
    private string colourCancel;
    private string colourEnd;


    /// <summary>
    /// needed for sequencing issues. Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
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
    /// <summary>
    /// Fast access submethod
    /// </summary>
    private void SubInitialiseFastAccess()
    {
        //node datapoint icons
        arrayOfIcons[0] = GameManager.i.guiScript.invisibilityIcon;
        arrayOfIcons[1] = GameManager.i.guiScript.opinionIcon;
        arrayOfIcons[2] = GameManager.i.guiScript.innocenceIcon;
        Debug.Assert(arrayOfIcons[0] != null, "Invalid arrayOfIcons[0] (Null)");
        Debug.Assert(arrayOfIcons[1] != null, "Invalid arrayOfIcons[1] (Null)");
        Debug.Assert(arrayOfIcons[2] != null, "Invalid arrayOfIcons[2] (Null)");
    }
    #endregion

    #endregion

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        Debug.Assert(tooltipPlayerObject != null, "Invalid tooltipPlayerObject (Null)");
        Debug.Assert(tooltipPlayerCanvas != null, "Invalid tooltipPlayerCanvas (Null)");
        Debug.Assert(playerName != null, "Invalid playerName (Null)");
        Debug.Assert(playerStatus != null, "Invalid playerStatus (Null)");
        Debug.Assert(playerConditions != null, "Invalid playerConditions (Null)");
        Debug.Assert(playerStats != null, "Invalid playerStats (Null)");
        Debug.Assert(playerMulti_1 != null, "Invalid playerMulti_1 (Null)");
        Debug.Assert(playerMulti_2 != null, "Invalid playerMulti_2 (Null)");
        Debug.Assert(divider_1 != null, "Invalid divider_1 (Null)");
        Debug.Assert(divider_2 != null, "Invalid divider_2 (Null)");
        Debug.Assert(divider_3 != null, "Invalid divider_3 (Null)");
        Debug.Assert(divider_4 != null, "Invalid divider_4 (Null)");
        Debug.Assert(divider_5 != null, "Invalid divider_5 (Null)");
        canvasGroup = tooltipPlayerObject.GetComponent<CanvasGroup>();
        rectTransform = tooltipPlayerObject.GetComponent<RectTransform>();
        fadeInTime = GameManager.i.guiScript.tooltipFade;
        offset = GameManager.i.guiScript.tooltipOffset;
        //event listener
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "TooltipPlayer");
        EventManager.i.AddListener(EventType.ChangeSide, OnEvent, "TooltipPlayer");
    }

    /// <summary>
    /// provide a static reference to tooltipPlayer that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TooltipPlayer Instance()
    {
        if (!tooltipPlayer)
        {
            tooltipPlayer = FindObjectOfType(typeof(TooltipPlayer)) as TooltipPlayer;
            if (!tooltipPlayer)
            { Debug.LogError("There needs to be one active TooltipPlayer script on a GameObject in your scene"); }
        }
        return tooltipPlayer;
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
        colourGood = GameManager.i.colourScript.GetColour(ColourType.dataGood);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.dataNeutral);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.dataBad);
        colourName = GameManager.i.colourScript.GetColour(ColourType.normalText);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourGrey = GameManager.i.colourScript.GetColour(ColourType.greyText);
        colourMint = GameManager.i.colourScript.GetColour(ColourType.mintText);
        colourCancel = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }

    #region SetToolip
    /// <summary>
    /// Initialise Player Tool tip
    /// </summary>
    /// <param name="pos">Position of tooltip originator -> note as it's a UI element transform will be in screen units, not world units</param>
    public void SetTooltip(Vector3 pos)
    {
        bool isStatus = false;
        bool isConditions = false;
        //open panel at start
        tooltipPlayerCanvas.gameObject.SetActive(true);
        tooltipPlayerObject.SetActive(true);
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        //set opacity to zero (invisible)
        SetOpacity(0f);
        //set state of all items in tooltip window (Status and Conditions are switched on later only if present)
        
        playerName.gameObject.SetActive(true);
        playerStatus.gameObject.SetActive(false);
        playerConditions.gameObject.SetActive(false);
        playerStats.gameObject.SetActive(true);
        playerMulti_1.gameObject.SetActive(true);
        playerMulti_2.gameObject.SetActive(true);
        divider_1.gameObject.SetActive(true);
        divider_2.gameObject.SetActive(false);
        divider_3.gameObject.SetActive(false);   //true if both Status and Conditions texts are present
        divider_4.gameObject.SetActive(true);
        divider_5.gameObject.SetActive(true);
        //
        // - - - Name - - -
        //
        playerName.text = string.Format("{0}<b>you</b>{1}{2}{3}<size=110%><b>{4}</b></size>{5}", colourCancel, colourEnd, "\n", colourName, GameManager.i.playerScript.PlayerName, colourEnd);
        //
        // - - - Status - - -
        //
        if (GameManager.i.playerScript.Status != ActorStatus.Active)
        {
            //activate UI components
            playerStatus.gameObject.SetActive(true);
            isStatus = true;
            switch (GameManager.i.playerScript.Status)
            {
                case ActorStatus.Inactive:
                    switch (GameManager.i.playerScript.InactiveStatus)
                    {
                        case ActorInactive.LieLow:
                            int numOfTurns = GameManager.i.actorScript.maxStatValue - GameManager.i.playerScript.Invisibility;
                            if (GameManager.i.playerScript.isLieLowFirstturn == true) { numOfTurns++; }
                            playerStatus.text = string.Format("{0}<b>LYING LOW</b>{1}{2}Back in {3} turn{4}", colourNeutral, colourEnd, "\n", numOfTurns,
                                numOfTurns != 1 ? "s" : "");
                            break;
                        case ActorInactive.Breakdown:
                            playerStatus.text = string.Format("{0}<b>BREAKDOWN (Stress)</b>{1}{2}Back next turn", colourNeutral, colourEnd, "\n");
                            break;
                    }
                    break;
                case ActorStatus.Captured:
                    playerStatus.text = string.Format("{0}<b>CAPTURED</b>{1}{2}Whereabouts unknown", colourBad, colourEnd, "\n");
                    break;
                default:
                    playerStatus.text = string.Format("{0}<b>{1}</b>{2}", colourNeutral, GameManager.i.playerScript.Status.ToString().ToUpper(), colourEnd);
                    break;
            }
        }
        //
        // - - - Conditions - - -
        //
        if (GameManager.i.playerScript.CheckNumOfConditions(playerSide) > 0)
        {
            List<Condition> listOfConditions = GameManager.i.playerScript.GetListOfConditions(playerSide);
            if (listOfConditions != null)
            {
                playerConditions.gameObject.SetActive(true);
                StringBuilder builderCondition = new StringBuilder();
                foreach (Condition condition in listOfConditions)
                {
                    if (builderCondition.Length > 0) { builderCondition.AppendLine(); }
                    switch (condition.type.name)
                    {
                        case "Good":
                            builderCondition.AppendFormat("{0}{1}{2}", colourGood, condition.tag, colourEnd);
                            break;
                        case "Bad":
                            builderCondition.AppendFormat("{0}{1}{2}", colourBad, condition.tag, colourEnd);
                            break;
                        case "Neutral":
                            builderCondition.AppendFormat("{0}{1}{2}", colourNeutral, condition.tag, colourEnd);
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid condition.type.name \"{0}\"", condition.type.name));
                            break;
                    }
                }
                isConditions = true;
                playerConditions.text = builderCondition.ToString();
            }
            else { Debug.LogWarning("Invalid listOfConditions (Null)"); }
        }
        //
        // - - - Dividers - - -
        //
        if (isStatus == true && isConditions == true) { divider_2.gameObject.SetActive(true); }
        if (isStatus == true || isConditions == true) { divider_3.gameObject.SetActive(true); }
        //
        // - - - Stats - - -
        //
        switch (playerSide.level)
        {
            case 1:
                //Authority
                StringBuilder builderAut = new StringBuilder();
                builderAut.AppendFormat("{0}Teams{1}{2}", colourAlert, colourEnd, "\n");
                builderAut.AppendFormat("<b>On Map<pos=70%>{0}</b>{1}", GameManager.i.dataScript.CheckTeamPoolCount(TeamPool.OnMap), "\n");
                builderAut.AppendFormat("<b>In Transit<pos=70%>{0}</b>{1}", GameManager.i.dataScript.CheckTeamPoolCount(TeamPool.InTransit), "\n");
                builderAut.AppendFormat("<b>Reserves<pos=70%>{0}</b>{1}", GameManager.i.dataScript.CheckTeamPoolCount(TeamPool.Reserve), "\n");
                playerStats.text = builderAut.ToString();
                break;
            case 2:
                //Resistance
                int invisibility = GameManager.i.playerScript.Invisibility;
                int mood = GameManager.i.playerScript.GetMood();
                StringBuilder builderRes = new StringBuilder();

                /*builderRes.AppendFormat("<size=110%><b>Invisibility<pos=70%>{0}{1}{2}</b></size>{3}", GameManager.instance.colourScript.GetValueColour(invisibility), invisibility, colourEnd, "\n");
                builderRes.AppendFormat("<size=110%><b>Mood<pos=70%>{0}{1}{2}</b></size>", GameManager.instance.colourScript.GetValueColour(mood), mood, colourEnd);*/

                builderRes.AppendFormat("{0} <b>Invisibility<pos=60%>{1}</b>{2}", arrayOfIcons[0], GameManager.i.guiScript.GetNormalStars(invisibility), "\n");
                builderRes.AppendFormat("{0} <b>Mood<pos=60%>{1}</b>{2}", arrayOfIcons[1], GameManager.i.guiScript.GetNormalStars(mood), "\n");
                builderRes.AppendFormat("{0} <b>Innocence<pos=60%>{1}</b>", arrayOfIcons[2], GameManager.i.guiScript.GetNormalStars(GameManager.i.playerScript.Innocence));
                playerStats.text = builderRes.ToString();
                break;
        }
        //
        // - - - MultiPurpose 1 - - - 
        //
        switch (playerSide.level)
        {
            case 1:
                //Authority
                playerMulti_1.text = "Placeholder";
                break;
            case 2:
                //Resistance -> Gear
                List<string> listOfGear = GameManager.i.playerScript.GetListOfGear();
                if (listOfGear != null && listOfGear.Count > 0)
                {
                    StringBuilder builderGear = new StringBuilder();
                    builderGear.AppendFormat("{0}<b>Gear</b>{1}", colourCancel, colourEnd);
                    //gear in inventory
                    foreach (string gearName in listOfGear)
                    {
                        Gear gear = GameManager.i.dataScript.GetGear(gearName);
                        if (gear != null)
                        { builderGear.AppendFormat("<b>{0}{1}{2}{3}</b>", "\n", colourNeutral, gear.tag, colourEnd); }
                    }
                    playerMulti_1.text = builderGear.ToString();
                }
                else { playerMulti_1.text = string.Format("{0}<size=95%>No Gear</size>{1}", colourGrey, colourEnd); }
                break;
        }
        //
        // - - - MultiPurpose 2 (Secrets) - - - 
        //
        List<Secret> listOfSecrets = GameManager.i.playerScript.GetListOfSecrets();
        if (listOfSecrets != null && listOfSecrets.Count > 0)
        {
            StringBuilder builderSecrets = new StringBuilder();
            builderSecrets.AppendFormat("{0}<b>Secrets</b>{1}", colourCancel, colourEnd);
            //loop secrets
            foreach (Secret secret in listOfSecrets)
            {
                //secrets shown as Red if known by other actors, green otherwise
                if (secret != null)
                {
                    if (secret.CheckNumOfActorsWhoKnow() > 0)
                    { builderSecrets.AppendFormat("<b>{0}{1}{2}{3}</b>", "\n", colourBad, secret.tag, colourEnd); }
                    else { builderSecrets.AppendFormat("<b>{0}{1}{2}{3}</b>", "\n", colourMint, secret.tag, colourEnd); }
                }
            }
            playerMulti_2.text = builderSecrets.ToString();
        }
        else { playerMulti_2.text = string.Format("{0}<size=95%>No Secrets</size>{1}", colourGrey, colourEnd); }
        //Coordinates -> You need to send World (object.transform) coordinates
        Vector3 worldPos = pos;
        //update required to get dimensions as tooltip is dynamic
        Canvas.ForceUpdateCanvases();
        rectTransform = tooltipPlayerObject.GetComponent<RectTransform>();
        float height = rectTransform.rect.height;
        float width = rectTransform.rect.width;
        //base y pos at zero (bottom of screen). Adjust up from there.
        worldPos.y += height + offset - 180;
        worldPos.x -= width / 10 + 180;
        //width
        if (worldPos.x + width / 2 >= Screen.width)
        { worldPos.x -= width / 2 + worldPos.x - Screen.width; }
        else if (worldPos.x - width / 2 <= 0)
        { worldPos.x += width / 2 - worldPos.x; }

        //set new position
        tooltipPlayerObject.transform.position = worldPos;
        Debug.LogFormat("[UI] TooltipPlayer.cs -> SetTooltip{0}", "\n");
    }
    #endregion

    public void SetOpacity(float opacity)
    { canvasGroup.alpha = opacity; }

    public float GetOpacity()
    { return canvasGroup.alpha; }

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
    { return tooltipPlayerObject.activeSelf; }

    /// <summary>
    /// close tool tip
    /// </summary>
    public void CloseTooltip(string callingMethod = "Unknown")
    {
        Debug.LogFormat("[UI] TooltipPlayer.cs -> CloseTooltip: calling by {0}{1}", callingMethod, "\n");
        tooltipPlayerObject.SetActive(false);
        tooltipPlayerCanvas.gameObject.SetActive(false);
    }


    /// <summary>
    /// set up sprites on actor tooltip for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    public void InitialiseTooltip(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = tooltipPlayerObject.GetComponent<Image>();
        //assign side specific sprites
        switch (side.level)
        {
            case 1:
                //Authority
                background.sprite = GameManager.i.sideScript.toolTip_backgroundAuthority;
                divider_1.sprite = GameManager.i.sideScript.toolTip_dividerAuthority;
                divider_2.sprite = GameManager.i.sideScript.toolTip_dividerAuthority;
                divider_3.sprite = GameManager.i.sideScript.toolTip_dividerAuthority;
                divider_4.sprite = GameManager.i.sideScript.toolTip_dividerAuthority;
                divider_5.sprite = GameManager.i.sideScript.toolTip_dividerAuthority;
                break;
            case 2:
                //Resistance
                background.sprite = GameManager.i.sideScript.toolTip_backgroundRebel;
                divider_1.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                divider_2.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                divider_3.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                divider_4.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                divider_5.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side.name));
                break;
        }
    }


}
