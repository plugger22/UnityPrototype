using gameAPI;
using modalAPI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Review UI where player is reviewed by their peers
/// </summary>
public class ModalReviewUI : MonoBehaviour
{

    public GameObject reviewObject;
    public GameObject panelObject;
    public GameObject headerObject;

    public Image panelBackground;
    public Image panelTop;
    public Image panelBottom;
    public Image panelHeader;

    public TextMeshProUGUI textTop;
    public TextMeshProUGUI textBottom;
    public TextMeshProUGUI textHeader;
    public TextMeshProUGUI outcomeLeft;
    public TextMeshProUGUI outcomeRight;

    public Button buttonReview;
    public Button buttonExit;
    public Button buttonHelpOpen;
    public Button buttonHelpClose;

    public GameObject[] arrayOfOptions;                         //place Review option UI elements here (5 x HQ options for top panel, 4 Subordinate options for bottom panel, in order Left to Right, Top to Bottom))
                                                                //HQ Boss opinion of decisions / HQ heirarchy in enum order / subordinates in slotID order
    [Tooltip("Wait interval for coroutine to display Review Outcome (between 0 and 1 second)")]
    [Range(0, 1.0f)] public float reviewWaitTime = 0.5f;


    /*[HideInInspector] ModalReviewSubState status; EDIT: now managed via inputScript.cs -> ModalReviewState */

    private ReviewInteraction[] arrayOfInteractions;            //used for fast access to interaction components
    private GenericTooltipUI[] arrayOfTooltipsSprites;          //used for fast access to tooltip components (Sprites)
    private GenericTooltipUI[] arrayOfTooltipsResults;            //used for fast access to tooltip components for Stars (bottomText)

    private GenericHelpTooltipUI tooltipHelpOpen;
    private GenericHelpTooltipUI tooltipHelpClose;

    private static ModalReviewUI modalReviewUI;
    private ButtonInteraction buttonInteractionReview;
    private ButtonInteraction buttonInteractionExit;

    private int votesFor;
    private int votesAgainst;
    private float reviewWaitTimerDefault;


    /// <summary>
    /// Static instance so the ModalReviewUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalReviewUI Instance()
    {
        if (!modalReviewUI)
        {
            modalReviewUI = FindObjectOfType(typeof(ModalReviewUI)) as ModalReviewUI;
            if (!modalReviewUI)
            { Debug.LogError("There needs to be one active ModalReviewUI script on a GameObject in your scene"); }
        }
        return modalReviewUI;
    }


    private void Awake()
    {
        reviewWaitTimerDefault = reviewWaitTime;
        //Review button event
        buttonInteractionReview = buttonReview.GetComponent<ButtonInteraction>();
        if (buttonInteractionReview != null)
        { buttonInteractionReview.SetButton(EventType.ReviewStart); }
        else { Debug.LogError("Invalid buttonInteraction Review (Null)"); }
        //Exit button event
        buttonInteractionExit = buttonExit.GetComponent<ButtonInteraction>();
        if (buttonInteractionExit != null)
        { buttonInteractionExit.SetButton(EventType.ReviewCloseUI); }
        else { Debug.LogError("Invalid buttonInteraction Exit (Null)"); }
        //help tooltips
        tooltipHelpOpen = buttonHelpOpen.GetComponent<GenericHelpTooltipUI>();
        tooltipHelpClose = buttonHelpClose.GetComponent<GenericHelpTooltipUI>();
        Debug.Assert(tooltipHelpOpen != null, "Invalid tooltipHelpOpen (Null)");
        Debug.Assert(tooltipHelpClose != null, "Invalid tooltipHelpClose (Null)");
        //inventory interaction & tooltip arrays set up
        int numOfOptions = arrayOfOptions.Length;
        arrayOfInteractions = new ReviewInteraction[numOfOptions];
        arrayOfTooltipsSprites = new GenericTooltipUI[numOfOptions];
        arrayOfTooltipsResults = new GenericTooltipUI[numOfOptions];
        for (int i = 0; i < numOfOptions; i++)
        {
            if (arrayOfOptions[i] != null)
            {
                //interaction
                ReviewInteraction interaction = arrayOfOptions[i].GetComponent<ReviewInteraction>();
                if (interaction != null)
                {
                    arrayOfInteractions[i] = interaction;
                    //tooltip -> sprite (attached to game object to prevent tooltip component masking gameobject interaction component which is needed for click detection for menu's)
                    GenericTooltipUI tooltipSprite = interaction.tooltipSprite.GetComponent<GenericTooltipUI>();
                    if (tooltipSprite != null)
                    { arrayOfTooltipsSprites[i] = tooltipSprite; }
                    else { Debug.LogError(string.Format("Invalid GenericTooltipUI for arrayOfReviewOptions[{0}] (Null)", i)); }
                    //tooltip -> result 
                    GenericTooltipUI tooltipResult = interaction.tooltipResult.GetComponent<GenericTooltipUI>();
                    if (tooltipResult != null)
                    { arrayOfTooltipsResults[i] = tooltipResult; }
                    else { Debug.LogError(string.Format("Invalid GenericTooltipUI for interaction.tooltipResult \"{0}\" (Null)", i)); }
                }
                else { Debug.LogError(string.Format("Invalid InventoryInteraction for arrayOfInventoryOptions[{0}] (Null)", i)); }
            }
            else { Debug.LogError(string.Format("Invalid arrayOfInventoryOptions[{0}] (Null)", i)); }
        }
    }

    private void Start()
    {
        //register listener
        EventManager.instance.AddListener(EventType.ReviewOpenUI, OnEvent, "ReviewInventoryUI");
        EventManager.instance.AddListener(EventType.ReviewStart, OnEvent, "ReviewInventoryUI");
        EventManager.instance.AddListener(EventType.ReviewCloseUI, OnEvent, "ReviewInventoryUI");
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
            case EventType.ReviewOpenUI:
                ReviewInputData details = Param as ReviewInputData;
                SetReviewUI(details);
                break;
            case EventType.ReviewStart:
                StartReview();
                break;
            case EventType.ReviewCloseUI:
                CloseReviewUI();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }



    /// <summary>
    /// Open Review UI
    /// </summary>
    private void SetReviewUI(ReviewInputData details)
    {
        bool errorFlag = false;
        //reset timer (may have changed previously if player skipped review)
        reviewWaitTime = reviewWaitTimerDefault;
        Debug.LogFormat("[UI] ModalReviewUI.cs -> OpenReviewUI{0}", "\n");
        //set modal status
        GameManager.instance.guiScript.SetIsBlocked(true);
        //activate main panel
        panelObject.SetActive(true);
        reviewObject.SetActive(true);
        headerObject.SetActive(true);
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //buttons
        buttonReview.gameObject.SetActive(true);
        buttonExit.gameObject.SetActive(false);
        buttonHelpOpen.gameObject.SetActive(true);
        buttonHelpClose.gameObject.SetActive(false);
        //outcome symbols
        outcomeLeft.gameObject.SetActive(false);
        outcomeRight.gameObject.SetActive(false);
        //set up modal panel & buttons to be side appropriate
        switch (playerSide.level)
        {
            case 1:
                panelBackground.sprite = GameManager.instance.sideScript.inventory_background_Authority;
                panelHeader.sprite = GameManager.instance.sideScript.header_background_Authority;
                //set button sprites
                buttonReview.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                buttonExit.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                //set sprite transitions
                SpriteState spriteStateAuthority = new SpriteState();
                spriteStateAuthority.highlightedSprite = GameManager.instance.sideScript.button_highlight_Authority;
                spriteStateAuthority.pressedSprite = GameManager.instance.sideScript.button_Click;
                buttonReview.spriteState = spriteStateAuthority;
                buttonExit.spriteState = spriteStateAuthority;
                break;
            case 2:
                panelBackground.sprite = GameManager.instance.sideScript.inventory_background_Resistance;
                panelHeader.sprite = GameManager.instance.sideScript.header_background_Resistance;
                //set button sprites
                buttonReview.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Resistance;
                buttonExit.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Resistance;
                //set sprite transitions
                SpriteState spriteStateRebel = new SpriteState();
                spriteStateRebel.highlightedSprite = GameManager.instance.sideScript.button_highlight_Resistance;
                spriteStateRebel.pressedSprite = GameManager.instance.sideScript.button_Click;
                buttonReview.spriteState = spriteStateRebel;
                buttonExit.spriteState = spriteStateRebel;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", playerSide.name));
                break;
        }
        //set texts
        textHeader.text = "Peer Review";
        textTop.text = "You are about to be assessed by your Peers";
        textBottom.text = "Press SPACE, or the COMMENCE REVIEW button";
        if (details != null)
        {
            //loop array and set options
            for (int i = 0; i < arrayOfOptions.Length; i++)
            {
                //valid option?
                if (arrayOfOptions[i] != null)
                {
                    if (arrayOfInteractions[i] != null)
                    {
                        if (arrayOfOptions[i] != null)
                        {
                            //activate option
                            arrayOfOptions[i].SetActive(true);
                            //populate option data
                            arrayOfInteractions[i].optionImage.sprite = details.arrayOfOptions[i].sprite;
                            arrayOfInteractions[i].textUpper.text = details.arrayOfOptions[i].textUpper;
                            arrayOfInteractions[i].optionData = details.arrayOfOptions[i].optionID;
                            //result
                            arrayOfInteractions[i].textResult.gameObject.SetActive(false);
                            arrayOfInteractions[i].textResult.text = details.arrayOfOptions[i].textLower;
                            //tooltip data -> sprites
                            if (arrayOfTooltipsSprites[i] != null)
                            {
                                if (details.arrayOfTooltipsSprite[i] != null)
                                {
                                    arrayOfTooltipsSprites[i].tooltipHeader = details.arrayOfTooltipsSprite[i].textHeader;
                                    arrayOfTooltipsSprites[i].tooltipMain = details.arrayOfTooltipsSprite[i].textMain;
                                    arrayOfTooltipsSprites[i].tooltipDetails = details.arrayOfTooltipsSprite[i].textDetails;
                                    arrayOfTooltipsSprites[i].x_offset = 55;
                                }
                                else { Debug.LogWarningFormat("Invalid tooltipDetailsSprite (Null) for arrayOfOptions[{0}]", i); }
                            }
                            else
                            {
                                Debug.LogError(string.Format("Invalid GenericTooltipUI (Null) in arrayOfTooltips[{0}]", i));
                            }
                            //tooltip data -> results
                            if (arrayOfTooltipsResults[i] != null)
                            {
                                if (details.arrayOfTooltipsResult[i] != null)
                                {
                                    arrayOfTooltipsResults[i].tooltipHeader = details.arrayOfTooltipsResult[i].textHeader;
                                    arrayOfTooltipsResults[i].tooltipMain = details.arrayOfTooltipsResult[i].textMain;
                                    arrayOfTooltipsResults[i].tooltipDetails = details.arrayOfTooltipsResult[i].textDetails;
                                    arrayOfTooltipsResults[i].x_offset = 55;
                                    arrayOfTooltipsResults[i].y_offset = 15;
                                }
                                else
                                { Debug.LogWarningFormat("Invalid tooltipResults (Null) in arrayOfTooltipResults[{0}]", i); }
                            }
                            else { Debug.LogWarningFormat("Invalid arrayOfTooltipsResults[{0}] (Null)", i); }
                        }
                        else
                        {
                            //invalid option, switch off
                            arrayOfOptions[i].SetActive(false);
                        }
                    }
                    else
                    {
                        //error -> Null Interaction data
                        Debug.LogErrorFormat("Invalid arrayOfInventoryOptions[\"{0}\"] optionInteraction (Null)", i);
                        errorFlag = true;
                        break;
                    }
                }
                else
                {
                    //error -> Null array
                    Debug.LogErrorFormat("Invalid arrayOfInventoryOptions[{0}] (Null)", i);
                    errorFlag = true;
                    break;
                }
            }
        }
        else
        {
            Debug.LogError("Invalid ReviewInputData (Null)");
            errorFlag = true;
        }
        //error outcome message if there is a problem
        if (errorFlag == true)
        {
            reviewObject.SetActive(false);
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.textTop = "There has been a hiccup and the information isn't available";
            outcomeDetails.textBottom = "We've called the WolfMan. He's on his way";
            outcomeDetails.side = playerSide;
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails, "ModalInventoryUI.cs -> SetInventoryUI");
        }
        else
        {
            //all good, inventory window displayed
            GameManager.instance.inputScript.ModalReviewState = ModalReviewSubState.Open;
            votesFor = details.votesFor;
            votesAgainst = details.votesAgainst;
            ModalStateData package = new ModalStateData() { mainState = ModalSubState.Review };
            GameManager.instance.inputScript.SetModalState(package);
            Debug.LogFormat("[UI] ModalInventoryUI.cs -> SetInventoryUI{0}", "\n");
        }
    }

    /// <summary>
    /// Shows actor review outcomes, one by one, once 'Commence Review' button pressed
    /// </summary>
    private void StartReview()
    {
        GameManager.instance.inputScript.ModalReviewState = ModalReviewSubState.Review;
        //deactivate review button
        buttonReview.gameObject.SetActive(false);
        buttonHelpOpen.gameObject.SetActive(false);
        textTop.text = "";
        textBottom.text = "Press SPACE to Skip";
        Debug.LogFormat("[Tst] ModalReviewUI.cs -> StartReview: Pre Coroutine{0}", "\n");
        StartCoroutine("ShowReview");
        Debug.LogFormat("[Tst] ModalReviewUI.cs -> StartReview: POST COROUTINE{0}", "\n");
        //activate exit button
        buttonExit.gameObject.SetActive(true);
    }

    /// <summary>
    /// Coroutine to sequentially display review outcomes (suspense) and, once done, review result
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowReview()
    {
        //loop array and make review outcomes visibile 
        for (int i = 0; i < arrayOfOptions.Length; i++)
        {
            //valid option?
            if (arrayOfOptions[i] != null)
            {
                if (arrayOfInteractions[i] != null)
                {
                    if (arrayOfOptions[i] != null)
                    {
                        //result
                        arrayOfInteractions[i].textResult.gameObject.SetActive(true);
                    }
                    else
                    {
                        //invalid option, switch off
                        arrayOfOptions[i].SetActive(false);
                    }
                }
                else
                {
                    //error -> Null Interaction data
                    Debug.LogErrorFormat("Invalid arrayOfInventoryOptions[\"{0}\"] optionInteraction (Null)", i);
                    break;
                }
            }
            else
            {
                //error -> Null array
                Debug.LogErrorFormat("Invalid arrayOfInventoryOptions[{0}] (Null)", i);
                break;
            }
            yield return new WaitForSeconds(reviewWaitTime);
        }
        //pause before outcome
        yield return new WaitForSeconds(reviewWaitTime);
        string outcomeText = "Unknown";
        string outcomeSymbol = "?";
        //outcome text and symbols
        if (votesAgainst >= 3)
        {
            outcomeText = string.Format("<size=120%>{0}</size> earned", GameManager.instance.colourScript.GetFormattedString("BLACK MARK", ColourType.badText));
            outcomeSymbol = string.Format("{0}", GameManager.instance.colourScript.GetFormattedString(GameManager.instance.guiScript.blackMarkChar.ToString(), ColourType.dataTerrible));
            outcomeLeft.gameObject.SetActive(true);
            outcomeRight.gameObject.SetActive(true);
            outcomeLeft.text = outcomeSymbol;
            outcomeRight.text = outcomeSymbol;
            GameManager.instance.campaignScript.ChangeBlackMarks(1, "Peer Review");
        }
        else if (votesFor >= 3)
        {
            outcomeText = string.Format("<size=120%>{0}</size> earned", GameManager.instance.colourScript.GetFormattedString("COMMENDATION", ColourType.goodText));
            outcomeSymbol = string.Format("{0}", GameManager.instance.colourScript.GetFormattedString(GameManager.instance.guiScript.commendationChar.ToString(), ColourType.dataGood));
            outcomeLeft.gameObject.SetActive(true);
            outcomeRight.gameObject.SetActive(true);
            outcomeLeft.text = outcomeSymbol;
            outcomeRight.text = outcomeSymbol;
            GameManager.instance.campaignScript.ChangeCommendations(1, "Peer Review");
        }
        else { outcomeText = string.Format("<size=120%>{0}</size> result", GameManager.instance.colourScript.GetFormattedString("INCONCLUSIVE", ColourType.neutralText)); }
        textTop.text = string.Format("Votes For {0}, Votes Against {1}{2}{3}", votesFor, votesAgainst, "\n", outcomeText);
        textBottom.text = "Press ESC or EXIT once done";
        //hand back control
        buttonHelpClose.gameObject.SetActive(true);
        GameManager.instance.inputScript.ModalReviewState = ModalReviewSubState.Close;
        yield return null;
    }

    /// <summary>
    /// Close Review UI
    /// </summary>
    private void CloseReviewUI()
    {
        reviewObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false);
        //close generic tooltip (safety check)
        GameManager.instance.tooltipGenericScript.CloseTooltip("ModalReviewUI.cs -> CloseReview");
        //set game state
        GameManager.instance.inputScript.ResetStates();
        Debug.LogFormat("[UI] ModalReviewUI.cs -> CloseReviewUI{0}", "\n");
        GameManager.instance.inputScript.ModalReviewState = ModalReviewSubState.None;
    }

}
