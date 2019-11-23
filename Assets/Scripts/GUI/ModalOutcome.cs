using modalAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using gameAPI;
using packageAPI;

/// <summary>
/// Handles Modal Outcome window.
/// Uses ModalOutcomeDetails (ModalUtility.cs) to pass the appropriate details
/// </summary>
public class ModalOutcome : MonoBehaviour
{
    public TextMeshProUGUI topText;
    public TextMeshProUGUI bottomText;
    public Image outcomeImage;
    public Button confirmButton;
    public Button helpButton;


    /*[Tooltip("Sprite used to skin Confirm button for an Authority Outcome")]
    public Sprite buttonSpriteAuthority;
    [Tooltip("Sprite used to skin Confirm button for a Rebel Outcome")]
    public Sprite buttonSpriteResistance;*/
    

    //public GameObject modalOutcomeObject;
    public GameObject modalOutcomeWindow;

    private static ModalOutcome modalOutcome;
    private RectTransform rectTransform;
    private Image background;
    private CanvasGroup canvasGroup;
    private GenericHelpTooltipUI help;
    /*private float fadeInTime;*/
    private int modalLevel;                              //modal level of menu, passed in by ModalOutcomeDetails in SetModalOutcome
    private ModalSubState modalState;                       //modal state to return to once outcome window closed (handles modalLevel 2+ cases, ignored for rest)
    private string reason;                               //reason outcome window is being used (passed on via CloseOutcomeWindow event to UseAction event for debugging

    private bool isAction;                              //triggers 'UseAction' event on confirmation button click if true (passed in to method by ModalOutcomeDetails)

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        /*canvasGroup = modalOutcomeObject.GetComponent<CanvasGroup>();
        rectTransform = modalOutcomeObject.GetComponent<RectTransform>();*/

        //Asserts
        Debug.Assert(helpButton != null, "Invalid GenericHelpTooltipUI (Null)");
        Debug.Assert(confirmButton != null, "Invalid confirmButton (Null)");
        //Assignments
        canvasGroup = modalOutcomeWindow.GetComponent<CanvasGroup>();
        rectTransform = modalOutcomeWindow.GetComponent<RectTransform>();
        help = helpButton.GetComponent<GenericHelpTooltipUI>();
        if (help == null) { Debug.LogError("Invalid help script (Null)"); }
        /*fadeInTime = GameManager.instance.tooltipScript.tooltipFade;*/
        ButtonInteraction buttonInteract = confirmButton.GetComponent<ButtonInteraction>();
        if (buttonInteract != null)
        { buttonInteract.SetButton(EventType.CloseOutcomeWindow); }
        else { Debug.LogError("Invalid buttonInteract (Null)"); }

        //register a listener
        EventManager.instance.AddListener(EventType.OpenOutcomeWindow, OnEvent, "ModalOutcome");
        EventManager.instance.AddListener(EventType.CloseOutcomeWindow, OnEvent, "ModalOutcome");
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "ModalOutcome");
    }

    /// <summary>
    /// provide a static reference to tooltipNode that can be accessed from any script
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


    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //detect event type
        switch(eventType)
        {
            case EventType.OpenOutcomeWindow:
                ModalOutcomeDetails details = Param as ModalOutcomeDetails;
                //SetModalOutcome(details.side, details.textTop, details.textBottom, details.sprite);
                SetModalOutcome(details);
                break;
            case EventType.CloseOutcomeWindow:
                CloseModalOutcome();
                break;
            case EventType.ChangeSide:
                InitialiseOutcome((GlobalSide)Param);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.OpenOutcomeWindow);
    }

   

    /// <summary>
    /// Initiate Modal Outcome window
    /// </summary>
    /// <param name="details"></param>
    public void SetModalOutcome(ModalOutcomeDetails details)
    {
        if (details != null)
        {
            //ignore if autoRun true
            if (GameManager.instance.turnScript.CheckIsAutoRun() == false)
            {
                //exit any generic or node tooltips
                GameManager.instance.tooltipGenericScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcome");
                GameManager.instance.tooltipNodeScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcome");
                GameManager.instance.tooltipHelpScript.CloseTooltip("ModalOutcome.cs -> SetModalOutcome");
                reason = details.reason;
                //set help
                List<HelpData> listOfHelpData = GameManager.instance.helpScript.GetHelpData(details.help0, details.help1, details.help2, details.help3);
                if (listOfHelpData != null && listOfHelpData.Count > 0)
                {
                    helpButton.gameObject.SetActive(true);
                    help.SetHelpTooltip(listOfHelpData, 125, 280);
                }
                else { helpButton.gameObject.SetActive(false); }
                //set modal true
                GameManager.instance.guiScript.SetIsBlocked(true, details.modalLevel);
                //open panel at start, the modal window is already active on the panel
                modalOutcomeWindow.SetActive(true);
                //register action status
                isAction = details.isAction;
                //set confirm button image and sprite states
                switch (details.side.name)
                {
                    case "Authority":
                        //set button sprites
                        confirmButton.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                        //set sprite transitions
                        SpriteState spriteStateAuthority = new SpriteState();
                        spriteStateAuthority.highlightedSprite = GameManager.instance.sideScript.button_highlight_Authority;
                        spriteStateAuthority.pressedSprite = GameManager.instance.sideScript.button_Click;
                        confirmButton.spriteState = spriteStateAuthority;
                        break;
                    case "Resistance":
                        //set button sprites
                        confirmButton.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Resistance;
                        //set sprite transitions
                        SpriteState spriteStateRebel = new SpriteState();
                        spriteStateRebel.highlightedSprite = GameManager.instance.sideScript.button_highlight_Resistance;
                        spriteStateRebel.pressedSprite = GameManager.instance.sideScript.button_Click;
                        confirmButton.spriteState = spriteStateRebel;
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid side \"{0}\"", details.side));
                        break;
                }
                //set transition
                confirmButton.transition = Selectable.Transition.SpriteSwap;

                //set opacity to zero (invisible)
                //SetOpacity(0f);

                //set up modalOutcome elements
                topText.text = details.textTop;
                bottomText.text = details.textBottom;
                if (details.sprite != null)
                { outcomeImage.sprite = details.sprite; }

                //get dimensions of dynamic tooltip
                float width = rectTransform.rect.width;
                float height = rectTransform.rect.height;

                //Fixed position at screen centre
                Vector3 screenPos = new Vector3();
                screenPos.x = Screen.width / 2;
                screenPos.y = Screen.height / 2;
                //set position
                modalOutcomeWindow.transform.position = screenPos;
                //set states
                ModalStateData package = new ModalStateData() { mainState = ModalSubState.Outcome };
                GameManager.instance.inputScript.SetModalState(package);
                //pass through data for when the outcome window is closed
                modalLevel = details.modalLevel;
                modalState = details.modalState;
                Debug.LogFormat("[UI] ModalOutcome.cs -> SetModalOutcome{0}", "\n");
            }
        }
        else { Debug.LogWarning("Invalid ModalOutcomeDetails package (Null)"); }
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


    /// <summary>
    /// returns true if tooltip is active and visible
    /// </summary>
    /// <returns></returns>
    public bool CheckModalOutcomeActive()
    {
        //return modalOutcomeObject.activeSelf;
        return modalOutcomeWindow.activeSelf;
    }

    /// <summary>
    /// close window
    /// </summary>
    public void CloseModalOutcome()
    {
        Debug.LogFormat("[UI] ModalOutcome.cs -> CloseModalOutcome{0}", "\n");
        //modalOutcomeObject.SetActive(false);
        modalOutcomeWindow.SetActive(false);
        //close tooltip
        GameManager.instance.tooltipHelpScript.CloseTooltip("ModalOutcome.cs -> CloseModalOutcome");
        //set modal false
        GameManager.instance.guiScript.SetIsBlocked(false, modalLevel);
        //set game state
        GameManager.instance.inputScript.ResetStates(modalState);
        //end of turn check
        if (isAction == true)
        { EventManager.instance.PostNotification(EventType.UseAction, this, reason, "ModalOutcome.cs -> CloseModalOutcome"); }
        //auto set haltExecution to false (in case execution halted at end of turn, waiting on Compromised Gear Outcome dialogue to be sorted)
        GameManager.instance.turnScript.haltExecution = false;
        //auto set waitUntilDone for InfoPipeline (waiting on a message in the pipeline to be done)
        GameManager.instance.guiScript.waitUntilDone = false;
    }


    public void SetOpacity(float opacity)
    { canvasGroup.alpha = opacity; }

    public float GetOpacity()
    { return canvasGroup.alpha; }

    /// <summary>
    /// set up sprites on outcome window for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    private void InitialiseOutcome(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = modalOutcomeWindow.GetComponent<Image>();
        //assign side specific sprites
        switch (side.level)
        {
            case 1:
                //Authority
                background.sprite = GameManager.instance.sideScript.outcome_backgroundAuthority;
                break;
            case 2:
                //Resistance
                background.sprite = GameManager.instance.sideScript.outcome_backgroundRebel;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side.name));
                break;
        }
    }

}
