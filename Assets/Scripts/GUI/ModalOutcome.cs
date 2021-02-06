﻿using gameAPI;
using modalAPI;
using packageAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles Modal Outcome window.
/// Uses ModalOutcomeDetails (ModalUtility.cs) to pass the appropriate details
/// </summary>
public class ModalOutcome : MonoBehaviour
{
    public GameObject outcomeObject;
    public Canvas outcomeCanvas;

    //Normal
    public Image panelNormal;
    public Image portraitNormal;
    public TextMeshProUGUI topTextNormal;
    public TextMeshProUGUI bottomTextNormal;
    public Button confirmButtonNormal;
    public Button showMeButtonNormal;
    public Button helpButtonNormal;
    //Special
    public Canvas canvasSpecial;
    public Image panelSpecial;
    public Image portraitSpecial;
    public Image blackBar;
    public TextMeshProUGUI topTextSpecial;
    public TextMeshProUGUI bottomTextSpecial;
    



    private RectTransform rectTransform;
    private RectTransform blackBarTransform;
    private Image background;
    private ButtonInteraction interactConfirm;
    private ButtonInteraction interactShowMe;
    /*private CanvasGroup canvasGroup;*/
    private GenericHelpTooltipUI help;
    /*private float fadeInTime;*/
    private int modalLevel;                              //modal level of menu, passed in by ModalOutcomeDetails in SetModalOutcome
    private ModalSubState modalState;                       //modal state to return to once outcome window closed (handles modalLevel 2+ cases, ignored for rest)
    private string reason;                               //reason outcome window is being used (passed on via CloseOutcomeWindow event to UseAction event for debugging
    private List<Node> listOfShowMeNodes = new List<Node>();    //used to store data passed in for any 'ShowMe' use
    private EventType hideOtherEvent;                   //event to call to hide underlying UI if 'ShowMe'. Can ignore
    private EventType restoreOtherEvent;                //event to call to restore underlying UI if 'ShowMe'. Can ignore
    private EventType triggerEvent;                     //optional event that triggers when Outcome Window closes
    private bool isAction;                              //triggers 'UseAction' event on confirmation button click if true (passed in to method by ModalOutcomeDetails)


    #region Static Instance
    private static ModalOutcome modalOutcome;

    /// <summary>
    /// provide a static reference to ModalOutcome that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalOutcome Instance()
    {
        if (!modalOutcome)
        {
            modalOutcome = FindObjectOfType(typeof(ModalOutcome)) as ModalOutcome;
            if (!modalOutcome)
            { Debug.LogError("There needs to be one active modalOutcome script on a GameObject in your scene"); }
        }
        return modalOutcome;
    }
    #endregion


    #region Initialisation...
    /// <summary>
    /// Master Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.LoadGame:
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                //do nothing
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    private void SubInitialiseAsserts()
    {
        //Overall
        Debug.Assert(outcomeObject != null, "Invalid outcomeObject (Null)");
        Debug.Assert(outcomeCanvas != null, "Invalid outcomeCanvas (Null)");

        //Normal
        Debug.Assert(panelNormal != null, "Invalid panelNormal (Null)");
        Debug.Assert(portraitNormal != null, "Invalid portraitNormal (Null)");
        Debug.Assert(topTextNormal != null, "Invalid topTextNormal (Null)");
        Debug.Assert(bottomTextNormal != null, "Invalid bottomTextNormal (Null)");
        Debug.Assert(helpButtonNormal != null, "Invalid GenericHelpTooltipUI (Null)");
        Debug.Assert(confirmButtonNormal != null, "Invalid confirmButton (Null)");
        Debug.Assert(showMeButtonNormal != null, "Invalid showMeButton (Null)");

        //Special
        Debug.Assert(canvasSpecial != null, "Invalid canvasSpecial (Null)");
        Debug.Assert(panelSpecial != null, "Invalid panelSpecial (Null)");
        Debug.Assert(portraitSpecial != null, "Invalid portraitSpecial (Null)");
        Debug.Assert(topTextSpecial != null, "Invalid topTextSpecial (Null)");
        Debug.Assert(bottomTextSpecial != null, "Invalid bottomTextSpecial (Null)");
        Debug.Assert(blackBar != null, "Invalid blackBar (Null)");
    }

    /// <summary>
    /// Session Start Initialisation
    /// </summary>
    private void SubInitialiseSessionStart()
    {
        /*canvasGroup = modalOutcomeWindow.GetComponent<CanvasGroup>();*/
        /*fadeInTime = GameManager.instance.tooltipScript.tooltipFade;*/

        //Assignments
        rectTransform = outcomeObject.GetComponent<RectTransform>();
        blackBarTransform = blackBar.GetComponent<RectTransform>();
        Debug.Assert(rectTransform != null, "Invalid rectTransform (Null)");
        Debug.Assert(blackBarTransform != null, "Invalid blackBarTransform (Null)");
        help = helpButtonNormal.GetComponent<GenericHelpTooltipUI>();
        if (help == null) { Debug.LogError("Invalid help script (Null)"); }
        //button interactions
        interactConfirm = confirmButtonNormal.GetComponent<ButtonInteraction>();
        if (interactConfirm != null)
        { interactConfirm.SetButton(EventType.OutcomeClose); }
        else { Debug.LogError("Invalid interactConfirm (Null)"); }
        interactShowMe = showMeButtonNormal.GetComponent<ButtonInteraction>();
        if (interactShowMe != null)
        { interactShowMe.SetButton(EventType.OutcomeShowMe); }
        else { Debug.LogError("Invalid interactShowMe (Null)"); }
        //Set Main elements
        outcomeObject.SetActive(true);
        outcomeCanvas.gameObject.SetActive(false);
        panelNormal.gameObject.SetActive(false);
        canvasSpecial.gameObject.SetActive(false);
        panelSpecial.gameObject.SetActive(true);
    }

    /// <summary>
    /// Initialise Event listeners
    /// </summary>
    private void SubInitialiseEvents()
    {
        //register a listener
        EventManager.i.AddListener(EventType.OutcomeOpen, OnEvent, "ModalOutcome");
        EventManager.i.AddListener(EventType.OutcomeClose, OnEvent, "ModalOutcome");
        EventManager.i.AddListener(EventType.OutcomeShowMe, OnEvent, "ModalOutcome");
        EventManager.i.AddListener(EventType.OutcomeRestore, OnEvent, "ModalOutcome");
        EventManager.i.AddListener(EventType.ChangeSide, OnEvent, "ModalOutcome");
    }
    #endregion


    #region OnEvent
    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //detect event type
        switch (eventType)
        {
            case EventType.OutcomeOpen:
                ModalOutcomeDetails details = Param as ModalOutcomeDetails;
                if (details.isSpecial == true)
                { SetModalOutcomeSpecial(details); }
                else { SetModalOutcome(details); }
                break;
            case EventType.OutcomeClose:
                CloseModalOutcome();
                break;
            case EventType.OutcomeShowMe:
                ExecuteShowMe();
                break;
            case EventType.OutcomeRestore:
                ExecuteRestore();
                break;
            case EventType.ChangeSide:
                InitialiseOutcome((GlobalSide)Param);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion

    #region OnDisable
    public void OnDisable()
    {
        EventManager.i.RemoveEvent(EventType.OutcomeOpen);
        EventManager.i.RemoveEvent(EventType.OutcomeClose);
        EventManager.i.RemoveEvent(EventType.OutcomeShowMe);
        EventManager.i.RemoveEvent(EventType.OutcomeRestore);
        EventManager.i.RemoveEvent(EventType.ChangeSide);
    }
    #endregion


    #region SetModalOutcome
    /// <summary>
    /// Initiate Modal Outcome window
    /// </summary>
    /// <param name="details"></param>
    public void SetModalOutcome(ModalOutcomeDetails details)
    {
        if (details != null)
        {
            //ignore if autoRun true
            if (GameManager.i.turnScript.CheckIsAutoRun() == false)
            {
                //exit any generic or node tooltips
                GameManager.i.tooltipGenericScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcome");
                GameManager.i.tooltipNodeScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcome");
                GameManager.i.tooltipHelpScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcome");
                reason = details.reason;
                triggerEvent = details.triggerEvent;
                //set help
                List<HelpData> listOfHelpData = GameManager.i.helpScript.GetHelpData(details.help0, details.help1, details.help2, details.help3);
                if (listOfHelpData != null && listOfHelpData.Count > 0)
                {
                    helpButtonNormal.gameObject.SetActive(true);
                    help.SetHelpTooltip(listOfHelpData, 100, 200);
                }
                else { helpButtonNormal.gameObject.SetActive(false); }
                //set modal true
                GameManager.i.guiScript.SetIsBlocked(true, details.modalLevel);
                //toggle panels
                panelNormal.gameObject.SetActive(true);
                canvasSpecial.gameObject.SetActive(false);
                //register action status
                isAction = details.isAction;
                /*
                //set confirm button image and sprite states
                switch (details.side.name)
                {
                    case "Authority":
                        //set button sprites
                        confirmButton.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Authority;
                        //set sprite transitions
                        SpriteState spriteStateAuthority = new SpriteState();
                        spriteStateAuthority.highlightedSprite = GameManager.i.sideScript.button_highlight_Authority;
                        spriteStateAuthority.pressedSprite = GameManager.i.sideScript.button_Click;
                        confirmButton.spriteState = spriteStateAuthority;
                        break;
                    case "Resistance":
                        //set button sprites
                        confirmButton.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Resistance;
                        //set sprite transitions
                        SpriteState spriteStateRebel = new SpriteState();
                        spriteStateRebel.highlightedSprite = GameManager.i.sideScript.button_highlight_Resistance;
                        spriteStateRebel.pressedSprite = GameManager.i.sideScript.button_Click;
                        confirmButton.spriteState = spriteStateRebel;
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid side \"{0}\"", details.side));
                        break;
                }
                //set transition
                confirmButton.transition = Selectable.Transition.SpriteSwap;
                */

                //Show Me
                if (details.listOfNodes != null && details.listOfNodes.Count > 0)
                {

                    //showMe data
                    listOfShowMeNodes = details.listOfNodes;
                    //disable Confirm, activate Show Me button
                    confirmButtonNormal.gameObject.SetActive(false);
                    showMeButtonNormal.gameObject.SetActive(true);
                    //events to call to handle underlying UI (if any)
                    hideOtherEvent = details.hideEvent;
                    restoreOtherEvent = details.restoreEvent;
                }
                else
                {
                    listOfShowMeNodes.Clear();
                    //disable ShowMe, activate Confirm button
                    confirmButtonNormal.gameObject.SetActive(true);
                    showMeButtonNormal.gameObject.SetActive(false);
                    //default settings for events
                    hideOtherEvent = EventType.None;
                    restoreOtherEvent = EventType.None;
                }

                //set opacity to zero (invisible)
                //SetOpacity(0f);

                //set up modalOutcome elements
                topTextNormal.text = details.textTop;
                bottomTextNormal.text = details.textBottom;
                if (details.sprite != null)
                { portraitNormal.sprite = details.sprite; }

                //get dimensions of outcome window (dynamic)
                float width = rectTransform.rect.width;
                float height = rectTransform.rect.height;

                //Fixed position at screen centre
                Vector3 screenPos = new Vector3();
                screenPos.x = Screen.width / 2;
                screenPos.y = Screen.height / 2;
                //set position
                outcomeObject.transform.position = screenPos;
                //set states
                ModalStateData package = new ModalStateData() { mainState = ModalSubState.Outcome };
                GameManager.i.inputScript.SetModalState(package);
                //open Canvas
                outcomeCanvas.gameObject.SetActive(true);
                //pass through data for when the outcome window is closed
                modalLevel = details.modalLevel;
                modalState = details.modalState;
                Debug.LogFormat("[UI] ModalOutcome.cs -> SetModalOutcome{0}", "\n");
                //fixed popUps
                GameManager.i.popUpFixedScript.ExecuteFixed(0.75f);

            }
        }
        else { Debug.LogWarning("Invalid ModalOutcomeDetails package (Null)"); }
    }
    #endregion


    #region SetModalOutcomeSpecial
    /// <summary>
    /// Initialise ModalOutcome window but in special format (expanding black bars across the screen)
    /// </summary>
    /// <param name="details"></param>
    private void SetModalOutcomeSpecial(ModalOutcomeDetails details)
    {
        if (details != null)
        {
            //ignore if autoRun true
            if (GameManager.i.turnScript.CheckIsAutoRun() == false)
            {
                //exit any generic or node tooltips
                GameManager.i.tooltipGenericScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcomeSpecial");
                GameManager.i.tooltipNodeScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcomeSpecial");
                GameManager.i.tooltipHelpScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcomeSpecial");
                reason = details.reason;
                triggerEvent = details.triggerEvent;

                //set help
                List<HelpData> listOfHelpData = GameManager.i.helpScript.GetHelpData(details.help0, details.help1, details.help2, details.help3);
                if (listOfHelpData != null && listOfHelpData.Count > 0)
                {
                    helpButtonNormal.gameObject.SetActive(true);
                    help.SetHelpTooltip(listOfHelpData, 100, 200);
                }
                else { helpButtonNormal.gameObject.SetActive(false); }

                //set modal true
                GameManager.i.guiScript.SetIsBlocked(true, details.modalLevel);
                //toggle panels
                panelNormal.gameObject.SetActive(false);
                canvasSpecial.gameObject.SetActive(true);
                //register action status
                isAction = details.isAction;

                #region archive
                /*//Show Me
                if (details.listOfNodes != null && details.listOfNodes.Count > 0)
                {

                    //showMe data
                    listOfShowMeNodes = details.listOfNodes;
                    //disable Confirm, activate Show Me button
                    confirmButtonNormal.gameObject.SetActive(false);
                    showMeButtonNormal.gameObject.SetActive(true);
                    //events to call to handle underlying UI (if any)
                    hideOtherEvent = details.hideEvent;
                    restoreOtherEvent = details.restoreEvent;
                }
                else
                {
                    listOfShowMeNodes.Clear();
                    //disable ShowMe, activate Confirm button
                    confirmButtonNormal.gameObject.SetActive(true);
                    showMeButtonNormal.gameObject.SetActive(false);
                    //default settings for events
                    hideOtherEvent = EventType.None;
                    restoreOtherEvent = EventType.None;
                }*/

                //set opacity to zero (invisible)
                //SetOpacity(0f);
                #endregion

                //set up modalOutcome elements
                topTextSpecial.text = details.textTop;
                bottomTextSpecial.text = details.textBottom;
                if (details.sprite != null)
                { portraitSpecial.sprite = details.sprite; }
                //open Canvas
                outcomeCanvas.gameObject.SetActive(true);
                //get dimensions of outcome window (dynamic)
                float width = rectTransform.rect.width;
                float height = rectTransform.rect.height;
                //set blackBar to min width
                blackBarTransform.sizeDelta = new Vector2(width, blackBarTransform.sizeDelta.y);

                //Fixed position at screen centre
                Vector3 screenPos = new Vector3();
                screenPos.x = Screen.width / 2;
                screenPos.y = Screen.height / 2;
                //set position
                outcomeObject.transform.position = screenPos;
                //set states
                ModalStateData package = new ModalStateData() { mainState = ModalSubState.Outcome };
                GameManager.i.inputScript.SetModalState(package);
                //pass through data for when the outcome window is closed
                modalLevel = details.modalLevel;
                modalState = details.modalState;
                Debug.LogFormat("[UI] ModalOutcome.cs -> SetModalOutcomeSpecial{0}", "\n");
                //fixed popUps
                GameManager.i.popUpFixedScript.ExecuteFixed(0.75f);
                //grow black bars
                StartCoroutine("GrowBlackBars");
            }
        }
        else { Debug.LogWarning("Invalid ModalOutcomeDetails package (Null)"); }
    }
    #endregion

    /// <summary>
    /// Extend black bars (behind Special Outcome) to full screen width
    /// </summary>
    /// <returns></returns>
    IEnumerator GrowBlackBars()
    {
        float growTime = 1.0f;
        float growSpeed = Screen.width;
        float size = blackBarTransform.sizeDelta.x;
        while (blackBarTransform.sizeDelta.x < Screen.width)
        {
            size += Time.deltaTime / growTime * growSpeed;
            blackBarTransform.sizeDelta = new Vector2(size, blackBarTransform.sizeDelta.y);
            yield return null;
        }
    }

    /*/// <summary>
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
    }*/


    #region CheckModalOutcomeActive
    /// <summary>
    /// returns true if tooltip is active and visible
    /// </summary>
    /// <returns></returns>
    public bool CheckModalOutcomeActive()
    {
        //return modalOutcomeObject.activeSelf;
        return outcomeObject.activeSelf;
    }
    #endregion

    #region CloseModalOutcome
    /// <summary>
    /// close window
    /// </summary>
    public void CloseModalOutcome()
    {
        Debug.LogFormat("[UI] ModalOutcome.cs -> CloseModalOutcome{0}", "\n");
        //toggle canvas off
        outcomeCanvas.gameObject.SetActive(false);
        //close tooltips
        GameManager.i.guiScript.SetTooltipsOff();
        //stop coroutine
        StopCoroutine("GrowBlackBars");
        //set modal false
        GameManager.i.guiScript.SetIsBlocked(false, modalLevel);
        //set game state
        GameManager.i.inputScript.ResetStates(modalState);
        //end of turn check
        if (isAction == true)
        { EventManager.i.PostNotification(EventType.UseAction, this, reason, "ModalOutcome.cs -> CloseModalOutcome"); }
        //auto set haltExecution to false (in case execution halted at end of turn, waiting on Compromised Gear Outcome dialogue to be sorted)
        GameManager.i.turnScript.haltExecution = false;
        //auto set waitUntilDone for InfoPipeline (waiting on a message in the pipeline to be done)
        GameManager.i.guiScript.waitUntilDone = false;
        //check trigger event
        if (triggerEvent != EventType.None)
        { EventManager.i.PostNotification(triggerEvent, this, null, "ModalOutcome.cs -> CloseModalOutcome"); }
    }
    #endregion


    /*public void SetOpacity(float opacity)
    { canvasGroup.alpha = opacity; }

    public float GetOpacity()
    { return canvasGroup.alpha; }*/

    /// <summary>
    /// set up sprites on outcome window for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    private void InitialiseOutcome(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = outcomeObject.GetComponent<Image>();
        //assign side specific sprites
        switch (side.level)
        {
            case 1:
                //Authority
                background.sprite = GameManager.i.sideScript.outcome_backgroundAuthority;
                break;
            case 2:
                //Resistance
                background.sprite = GameManager.i.sideScript.outcome_backgroundRebel;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side.name));
                break;
        }
    }

    /// <summary>
    /// ShowMe button pressed (display node/s while hiding outcome window)
    /// </summary>
    private void ExecuteShowMe()
    {
        //turn of ModalOutcome window
        outcomeObject.SetActive(false);
        //pass data package to GUIManager.cs
        ShowMeData data = new ShowMeData();
        data.restoreEvent = EventType.OutcomeRestore;
        data.listOfNodes.AddRange(listOfShowMeNodes);
        data.hideOtherEvent = hideOtherEvent;
        data.restoreOtherEvent = restoreOtherEvent;
        GameManager.i.guiScript.SetShowMe(data);
    }

    /// <summary>
    /// Restore from ShowMe. Button deactivated in place of Confirm button
    /// </summary>
    private void ExecuteRestore()
    {
        //set Game State
        ModalStateData package = new ModalStateData();
        package.mainState = ModalSubState.Outcome;
        GameManager.i.inputScript.SetModalState(package);
        //alert message
        GameManager.i.alertScript.CloseAlertUI();
        //swap buttons
        confirmButtonNormal.gameObject.SetActive(true);
        showMeButtonNormal.gameObject.SetActive(false);
        //turn ModalOutcome back on
        outcomeObject.SetActive(true);
    }

    //new methods above here
}
