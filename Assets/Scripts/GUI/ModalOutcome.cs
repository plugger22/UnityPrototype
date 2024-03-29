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
    public Image highlight;
    public TextMeshProUGUI topTextSpecial;
    public TextMeshProUGUI bottomTextSpecial;



    //private
    private RectTransform specialTransform;
    private RectTransform blackBarTransform;
    private RectTransform highlightTransform;
    private Image background;
    private ButtonInteraction interactConfirm;
    private ButtonInteraction interactShowMe;
    /*private CanvasGroup canvasGroup;*/
    private GenericHelpTooltipUI helpNormal;
    /*private float fadeInTime;*/
    private int modalLevel;                              //modal level of menu, passed in by ModalOutcomeDetails in SetModalOutcome
    private ModalSubState modalState;                       //modal state to return to once outcome window closed (handles modalLevel 2+ cases, ignored for rest)
    private string reason;                               //reason outcome window is being used (passed on via CloseOutcomeWindow event to UseAction event for debugging
    private List<Node> listOfShowMeNodes = new List<Node>();    //used to store data passed in for any 'ShowMe' use
    private EventType hideOtherEvent;                   //event to call to hide underlying UI if 'ShowMe'. Can ignore
    private EventType restoreOtherEvent;                //event to call to restore underlying UI if 'ShowMe'. Can ignore
    private EventType triggerEvent;                     //optional event that triggers when Outcome Window closes
    private bool isAction;                              //triggers 'UseAction' event on confirmation button click if true (passed in to method by ModalOutcomeDetails)
    private bool isSpecial;                             //true if a special outcome
    private bool isTutorial;                            //true if a tutorial dialogue (one that, when closed, moves the RHS arrow)
    private float specialWidth = -1.0f;
    private float blackBarTimeGrow;                     //time span to grow black bars
    private float blackBarTimeShrink;
    private float blackBarSpeed;
    private float blackBarSize;
    private float highlightMin;
    private float highlightMax;
    private float highlightTime;
    private float highlightPause;
    private float highlightHeight;
    private float highlightFade;
    private bool isHighlightGrow;
    //coroutines
    private Coroutine myCoroutineBarGrow;
    private Coroutine myCoroutineBarShrink;
    private Coroutine myCoroutineHighlights;
    private Coroutine myCoroutineFadeHighlights;
    //fast access
    private Color colourGood;
    private Color colourBad;



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
            case GameState.TutorialOptions:
            case GameState.LoadGame:
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
                SubInitialiseAsserts();
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

    #region SubInitialiseAsserts
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
        Debug.Assert(helpButtonNormal != null, "Invalid helpButtonNormal (Null)");
        Debug.Assert(confirmButtonNormal != null, "Invalid confirmButton (Null)");
        Debug.Assert(showMeButtonNormal != null, "Invalid showMeButton (Null)");

        //Special
        Debug.Assert(canvasSpecial != null, "Invalid canvasSpecial (Null)");
        Debug.Assert(panelSpecial != null, "Invalid panelSpecial (Null)");
        Debug.Assert(portraitSpecial != null, "Invalid portraitSpecial (Null)");
        Debug.Assert(topTextSpecial != null, "Invalid topTextSpecial (Null)");
        Debug.Assert(bottomTextSpecial != null, "Invalid bottomTextSpecial (Null)");
        Debug.Assert(blackBar != null, "Invalid blackBar (Null)");
        Debug.Assert(highlight != null, "Invalid highlight1 (Null)");
    }
    #endregion

    #region SubInitialiseSessionStart
    /// <summary>
    /// Session Start Initialisation
    /// </summary>
    private void SubInitialiseSessionStart()
    {
        /*canvasGroup = modalOutcomeWindow.GetComponent<CanvasGroup>();*/
        /*fadeInTime = GameManager.instance.tooltipScript.tooltipFade;*/

        //Assignments
        specialTransform = panelSpecial.GetComponent<RectTransform>();
        blackBarTransform = blackBar.GetComponent<RectTransform>();
        highlightTransform = highlight.GetComponent<RectTransform>();
        specialWidth = specialTransform.rect.width;
        Debug.Assert(specialTransform != null, "Invalid specialTransform (Null)");
        Debug.Assert(blackBarTransform != null, "Invalid blackBarTransform (Null)");
        Debug.Assert(highlightTransform != null, "Invalid highlight1Transform (Null)");
        Debug.Assert(specialWidth > 0.0f, "Invalid specialWidth (Zero or less)");
        helpNormal = helpButtonNormal.GetComponent<GenericHelpTooltipUI>();
        if (helpNormal == null) { Debug.LogError("Invalid helpNormal (Null)"); }
        //button interactions
        interactConfirm = confirmButtonNormal.GetComponent<ButtonInteraction>();
        if (interactConfirm != null)
        { interactConfirm.SetButton(EventType.OutcomeClose); }
        else { Debug.LogError("Invalid interactConfirm (Null)"); }
        interactShowMe = showMeButtonNormal.GetComponent<ButtonInteraction>();
        if (interactShowMe != null)
        { interactShowMe.SetButton(EventType.OutcomeShowMe); }
        else { Debug.LogError("Invalid interactShowMe (Null)"); }
        //colours
        colourGood = GameManager.i.uiScript.outcomeGood;
        colourBad = GameManager.i.uiScript.outcomeBad;
        panelSpecial.color = GameManager.i.uiScript.outcomeSpecial;
        blackBar.color = GameManager.i.uiScript.outcomeBlackBars;
        //Fixed position at screen centre
        Vector3 screenPos = new Vector3();
        screenPos.x = Screen.width / 2;
        screenPos.y = Screen.height / 2;
        //set position
        outcomeObject.transform.position = screenPos;
        //Blackbar (special outcome)
        blackBarTimeGrow = GameManager.i.guiScript.outcomeBarTimerGrow;
        blackBarTimeShrink = GameManager.i.guiScript.outcomeBarTimerShrink;
        blackBarSpeed = Screen.width;
        blackBarSize = specialTransform.sizeDelta.x;
        //highlights (special Outcome)
        highlightMax = GameManager.i.guiScript.outcomeHighlightMax;
        highlightTime = GameManager.i.guiScript.outcomeHighlightTimer;
        highlightPause = GameManager.i.guiScript.outcomeHighlightPause;
        highlightFade = GameManager.i.guiScript.outcomeHighlightFade;
        highlightMin = specialTransform.sizeDelta.x;
        highlightHeight = highlightTransform.sizeDelta.y;
        //Set Main elements
        outcomeObject.SetActive(true);
        outcomeCanvas.gameObject.SetActive(false);
        panelNormal.gameObject.SetActive(false);
        canvasSpecial.gameObject.SetActive(false);
        panelSpecial.gameObject.SetActive(true);
    }
    #endregion

    #region SubInitialiseEvents
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
    }
    #endregion

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
                if (isSpecial == true)
                { StartCoroutine(RunCloseSequence()); }
                else { CloseModalOutcome(); }
                break;
            case EventType.OutcomeShowMe:
                ExecuteShowMe();
                break;
            case EventType.OutcomeRestore:
                ExecuteRestore();
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
            isSpecial = false;
            //ignore if autoRun true
            if (GameManager.i.turnScript.CheckIsAutoRun() == false)
            {
                //reset input
                Input.ResetInputAxes();
                //exit any generic or node tooltips
                GameManager.i.tooltipGenericScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcome");
                GameManager.i.tooltipNodeScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcome");
                GameManager.i.tooltipHelpScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcome");
                reason = details.reason;
                triggerEvent = details.triggerEvent;
                isTutorial = details.isTutorial;
                //set help
                List<HelpData> listOfHelpData = GameManager.i.helpScript.GetHelpData(details.help0, details.help1, details.help2, details.help3);
                if (listOfHelpData != null && listOfHelpData.Count > 0)
                {
                    helpButtonNormal.gameObject.SetActive(true);
                    helpNormal.SetHelpTooltip(listOfHelpData, 100, 200);
                }
                else { helpButtonNormal.gameObject.SetActive(false); }
                //set modal true
                GameManager.i.guiScript.SetIsBlocked(true, details.modalLevel);
                //toggle panels
                panelNormal.gameObject.SetActive(true);
                canvasSpecial.gameObject.SetActive(false);
                //register action status
                isAction = details.isAction;
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

                /*//get dimensions of outcome window (dynamic)
                float width = rectTransform.rect.width;
                float height = rectTransform.rect.height;*/

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

    #region Black Bars (special) ...

    #region SetModalOutcomeSpecial
    /// <summary>
    /// Initialise ModalOutcome window but in special format (expanding black bars across the screen)
    /// </summary>
    /// <param name="details"></param>
    private void SetModalOutcomeSpecial(ModalOutcomeDetails details)
    {
        if (details != null)
        {
            isSpecial = true;
            //ignore if autoRun true
            if (GameManager.i.turnScript.CheckIsAutoRun() == false)
            {
                if (CheckModalOutcomeActive() == false)
                {
                    //reset input
                    Input.ResetInputAxes();
                    //exit any generic or node tooltips
                    GameManager.i.tooltipGenericScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcomeSpecial");
                    GameManager.i.tooltipNodeScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcomeSpecial");
                    GameManager.i.tooltipHelpScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcomeSpecial");
                    reason = details.reason;
                    triggerEvent = details.triggerEvent;
                    isTutorial = details.isTutorial;
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
                    /*bottomTextSpecial.text = string.Format("{0}{1}{2}<size=80%>Press ANY Key to exit</size>", details.textBottom, "\n", "\n");*/
                    if (details.sprite != null)
                    { portraitSpecial.sprite = details.sprite; }
                    //open Canvas
                    outcomeCanvas.gameObject.SetActive(true);

                    //set blackBar to min width
                    blackBarTransform.sizeDelta = new Vector2(specialWidth, blackBarTransform.sizeDelta.y);
                    //highlight colour
                    if (details.isSpecialGood == true)
                    { highlight.color = colourGood; }
                    else { highlight.color = colourBad; }
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
                    myCoroutineBarGrow = StartCoroutine("GrowBlackBar");
                    myCoroutineHighlights = StartCoroutine("RunHighlights");
                }
                else { Debug.LogWarning("Can't start a new Modal Outcome as an instance is already active"); }
            }
        }
        else { Debug.LogWarning("Invalid ModalOutcomeDetails package (Null)"); }
    }
    #endregion

    #region GrowBlackBars
    /// <summary>
    /// Extend black bars (behind Special Outcome) to full screen width
    /// </summary>
    /// <returns></returns>
    IEnumerator GrowBlackBar()
    {
        blackBarSize = blackBarTransform.sizeDelta.x;
        while (blackBarTransform.sizeDelta.x < Screen.width)
        {
            blackBarSize += Time.deltaTime / blackBarTimeGrow * blackBarSpeed;
            blackBarTransform.sizeDelta = new Vector2(blackBarSize, blackBarTransform.sizeDelta.y);
            yield return null;
        }
    }
    #endregion

    #region ShrinkBlackBars
    /// <summary>
    /// Shrink black bars (behind Special Outcome) to width of Special Outcome panel. Used when closing
    /// </summary>
    /// <returns></returns>
    IEnumerator ShrinkBlackBar()
    {
        blackBarSize = blackBarTransform.sizeDelta.x;
        while (blackBarTransform.sizeDelta.x > specialWidth)
        {
            blackBarSize -= Time.deltaTime / blackBarTimeShrink * blackBarSpeed;
            blackBarTransform.sizeDelta = new Vector2(blackBarSize, blackBarTransform.sizeDelta.y);
            yield return null;
        }
    }
    #endregion

    #region RunHighlights
    /// <summary>
    /// grow/shrink coloured line highlights along black bars behind the specialOutcome panel
    /// </summary>
    /// <returns></returns>
    IEnumerator RunHighlights()
    {
        //initialise
        highlightTransform.sizeDelta = new Vector2(highlightMin, highlightHeight);
        isHighlightGrow = true;
        float size = highlightTransform.sizeDelta.x;
        //continuous
        while (true)
        {
            if (isHighlightGrow == true)
            {
                //grow
                size += Time.deltaTime / highlightTime * highlightMax;
                highlightTransform.sizeDelta = new Vector2(size, highlightHeight);
                //max size check
                if (size >= highlightMax)
                { isHighlightGrow = false; }
            }
            else
            {
                //shrink
                size -= Time.deltaTime / highlightTime * highlightMax;
                highlightTransform.sizeDelta = new Vector2(size, highlightHeight);
                //min size check
                if (size <= highlightMin)
                {
                    isHighlightGrow = true;
                    //pause
                    yield return new WaitForSeconds(highlightPause);
                }
            }
            yield return null;
        }
    }
    #endregion

    #region FadeHighlights
    /// <summary>
    /// Fade out highlight on close
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeHighlights()
    {
        Color highlightColour;
        while (highlight.color.a > 0)
        {
            highlightColour = highlight.color;
            highlightColour.a -= Time.deltaTime / highlightFade;
            highlight.color = highlightColour;
            yield return null;
        }
    }
    #endregion

    #region RunCloseSequence
    /// <summary>
    /// Close sequence for outcome special (shrinks black bars first)
    /// </summary>
    /// <returns></returns>
    IEnumerator RunCloseSequence()
    {
        //deal with highlights first
        if (myCoroutineHighlights != null)
        {
            //stop highlight animation
            StopCoroutine(myCoroutineHighlights);
            //fade out highlights
            myCoroutineFadeHighlights = StartCoroutine("FadeHighlights");
            /*yield return myCoroutineFadeHighlights;*/
        }
        //shrink black bars
        myCoroutineBarShrink = StartCoroutine("ShrinkBlackBar");
        yield return myCoroutineBarShrink;
        //failsafe
        if (myCoroutineFadeHighlights != null)
        { StopCoroutine(myCoroutineFadeHighlights); }
        //close main panel
        CloseModalOutcome();
        yield break;
    }
    #endregion

    #endregion

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
        return outcomeCanvas.gameObject.activeSelf;
    }
    #endregion



    #region CloseModalOutcome
    /// <summary>
    /// close window
    /// </summary>
    private void CloseModalOutcome()
    {
        Debug.LogFormat("[UI] ModalOutcome.cs -> CloseModalOutcome{0}", "\n");

        //close tooltips
        GameManager.i.guiScript.SetTooltipsOff();
        //stop coroutine -> failsafe
        if (myCoroutineBarGrow != null)
        { StopCoroutine(myCoroutineBarGrow); }
        if (myCoroutineBarShrink != null)
        { StopCoroutine(myCoroutineBarShrink); }
        //set modal false
        GameManager.i.guiScript.SetIsBlocked(false, modalLevel);
        //set game state
        GameManager.i.inputScript.ResetStates(modalState);
        //reset (needs to be BEFORE toggling canvas off)
        isSpecial = false;
        //toggle canvas off
        outcomeCanvas.gameObject.SetActive(false);
        //end of turn check
        if (isAction == true)
        { GameManager.i.turnScript.UseAction(reason); }
        //tutorial outcome -> need to shift arrow
        if (isTutorial == true)
        { GameManager.i.tutorialUIScript.SetArrow(); }
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
