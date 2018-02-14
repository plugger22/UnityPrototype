using modalAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using gameAPI;

/// <summary>
/// Handles Modal Outcome window.
/// Uses ModalOutcomeDetails (ModalUtility.cs) to pass the appropriate details
/// </summary>
public class ModalOutcome : MonoBehaviour
{
    public TextMeshProUGUI outcomeText;
    public TextMeshProUGUI effectText;
    public Image outcomeImage;
    public Button confirmButton;
    public Sprite errorSprite;

    /*[Tooltip("Sprite used to skin Confirm button for an Authority Outcome")]
    public Sprite buttonSpriteAuthority;
    [Tooltip("Sprite used to skin Confirm button for a Rebel Outcome")]
    public Sprite buttonSpriteResistance;*/
    

    public GameObject modalOutcomeObject;
    public GameObject modalOutcomeWindow;

    private static ModalOutcome modalOutcome;
    private RectTransform rectTransform;
    private Image background;
    private CanvasGroup canvasGroup;
    private float fadeInTime;

    private bool isAction;                              //triggers 'UseAction' event on confirmation button click if true (passed in to method by ModalOutcomeDetails)

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        canvasGroup = modalOutcomeObject.GetComponent<CanvasGroup>();
        rectTransform = modalOutcomeObject.GetComponent<RectTransform>();
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
        ButtonInteraction buttonInteract = confirmButton.GetComponent<ButtonInteraction>();
        if (buttonInteract != null)
        { buttonInteract.SetEvent(EventType.CloseOutcomeWindow); }
        else { Debug.LogError("Invalid buttonInteract (Null)"); }
        //register a listener
        EventManager.instance.AddListener(EventType.OpenOutcomeWindow, OnEvent);
        EventManager.instance.AddListener(EventType.CloseOutcomeWindow, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
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
                InitialiseOutcome((Side)Param);
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

   

    //public void SetModalOutcome(Side side, string textTop, string textBottom, Sprite sprite = null)
    public void SetModalOutcome(ModalOutcomeDetails details)
    {
        Debug.Log(string.Format("Outcome: {0}{1}", details.textTop, "\n"));
        //set modal true
        GameManager.instance.guiScript.SetIsBlocked(true);
        //open panel at start, the modal window is already active on the panel
        modalOutcomeObject.SetActive(true);
        modalOutcomeWindow.SetActive(true);
        //register action status
        isAction = details.isAction;
        //set confirm button image and sprite states
        switch (details.side)
        {
            case Side.Authority:
                //set button sprites
                confirmButton.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                //set sprite transitions
                SpriteState spriteStateAuthority = new SpriteState();
                spriteStateAuthority.highlightedSprite = GameManager.instance.sideScript.button_highlight_Authority;
                spriteStateAuthority.pressedSprite = GameManager.instance.sideScript.button_Click;
                confirmButton.spriteState = spriteStateAuthority;
                break;
            case Side.Resistance:
                //set button sprites
                confirmButton.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Rebel;
                //set sprite transitions
                SpriteState spriteStateRebel = new SpriteState();
                spriteStateRebel.highlightedSprite = GameManager.instance.sideScript.button_highlight_Rebel;
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
        outcomeText.text = details.textTop;
        effectText.text = details.textBottom;
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
        //set game state
        GameManager.instance.inputScript.GameState = GameState.ModalOutcome;
        Debug.Log("UI: Open -> ModalOutcome window" + "\n");
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
    public bool CheckModalOutcomeActive()
    { return modalOutcomeObject.activeSelf; }

    /// <summary>
    /// close window
    /// </summary>
    public void CloseModalOutcome()
    {
        Debug.Log("UI: Close -> ModalOutcome window" + "\n");
        modalOutcomeObject.SetActive(false);
        //set modal false
        GameManager.instance.guiScript.SetIsBlocked(false);
        //set game state
        GameManager.instance.inputScript.GameState = GameState.Normal;
        //end of turn check
        if (isAction == true)
        { EventManager.instance.PostNotification(EventType.UseAction, this); }
    }


    public void SetOpacity(float opacity)
    { canvasGroup.alpha = opacity; }

    public float GetOpacity()
    { return canvasGroup.alpha; }

    /// <summary>
    /// set up sprites on outcome window for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    private void InitialiseOutcome(Side side)
    {
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = modalOutcomeWindow.GetComponent<Image>();
        //assign side specific sprites
        switch (side)
        {
            case Side.Authority:
                background.sprite = GameManager.instance.sideScript.outcome_backgroundAuthority;
                break;
            case Side.Resistance:
                background.sprite = GameManager.instance.sideScript.outcome_backgroundRebel;
                break;
        }
    }

}
