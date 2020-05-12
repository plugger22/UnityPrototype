using gameAPI;
using modalAPI;
using packageAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Review UI where player is reviewed by their peers
/// </summary>
public class ModalReviewUI : MonoBehaviour
{

    //NOTE: Review Button flash global parameters are in GUIManager.cs and are passed across in 'Start'

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
                                                                //HQ Boss opinion of decisions / HQ hierarchy in enum order / subordinates in slotID order
    [Tooltip("Wait interval for coroutine to display Review Outcome (between 0 and 1 second)")]
    [Range(0, 1.0f)] public float reviewWaitTime = 0.5f;


    /*[HideInInspector] ModalReviewSubState status; EDIT: now managed via inputScript.cs -> ModalReviewState */

    private ReviewInteraction[] arrayOfInteractions;            //used for fast access to interaction components
    private GenericTooltipUI[] arrayOfTooltipsSprites;          //used for fast access to tooltip components (Sprites)
    private GenericTooltipUI[] arrayOfTooltipsTitles;           //used for fast access to tooltip components (titles)
    private GenericTooltipUI[] arrayOfTooltipsResults;          //used for fast access to tooltip components for Stars (bottomText)

    private GenericHelpTooltipUI tooltipHelpOpen;
    private GenericHelpTooltipUI tooltipHelpClose;
    private GenericHelpTooltipUI tooltipOutcomeLeft;
    private GenericHelpTooltipUI tooltipOutcomeRight;

    private static ModalReviewUI modalReviewUI;
    private ButtonInteraction buttonInteractionReview;
    private ButtonInteraction buttonInteractionExit;

    private int votesFor;
    private int votesAgainst;
    private int votesAbstained;
    private float reviewWaitTimerDefault;

    //Review button flash
    private bool isFlashReviewButton;
    private int maxReviewButtonSize;
    private int reviewButtonIncrement;
    private float reviewTimeInterval;
    private bool isGrow;                                        //true if button growing in size (coroutine), false otherwise

    private bool isOutcome;                                     //true if an outcome achieved, eg. Black mark or commendation, false if inconclusive

    //fast Access
    private int votesMinimum = -1;


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
        tooltipOutcomeLeft = outcomeLeft.GetComponent<GenericHelpTooltipUI>();
        tooltipOutcomeRight = outcomeRight.GetComponent<GenericHelpTooltipUI>();
        Debug.Assert(tooltipHelpOpen != null, "Invalid tooltipHelpOpen (Null)");
        Debug.Assert(tooltipHelpClose != null, "Invalid tooltipHelpClose (Null)");
        Debug.Assert(tooltipOutcomeLeft != null, "Invalid tooltipOutcomeLeft (Null)");
        Debug.Assert(tooltipOutcomeRight != null, "Invalid tooltipOutcomeRight (Null)");
        //inventory interaction & tooltip arrays set up
        int numOfOptions = arrayOfOptions.Length;
        arrayOfInteractions = new ReviewInteraction[numOfOptions];
        arrayOfTooltipsSprites = new GenericTooltipUI[numOfOptions];
        arrayOfTooltipsTitles = new GenericTooltipUI[numOfOptions];
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
                    else { Debug.LogErrorFormat("Invalid GenericTooltipUI for arrayOfReviewOptions[{0}] (Null)", i); }
                    //tooltip -> title
                    GenericTooltipUI tooltipTitle = interaction.tooltipTitle.GetComponent<GenericTooltipUI>();
                    if (tooltipTitle != null)
                    { arrayOfTooltipsTitles[i] = tooltipTitle; }
                    else { Debug.LogErrorFormat("Invalid GenericTooltipUI for arrayOTooltipsTitle[{0}] (Null)", i); }
                    //tooltip -> result 
                    GenericTooltipUI tooltipResult = interaction.tooltipResult.GetComponent<GenericTooltipUI>();
                    if (tooltipResult != null)
                    { arrayOfTooltipsResults[i] = tooltipResult; }
                    else { Debug.LogErrorFormat("Invalid GenericTooltipUI for interaction.tooltipResult \"{0}\" (Null)", i); }
                }
                else { Debug.LogErrorFormat("Invalid InventoryInteraction for arrayOfInventoryOptions[{0}] (Null)", i); }
            }
            else { Debug.LogErrorFormat("Invalid arrayOfInventoryOptions[{0}] (Null)", i); }
        }
    }


    private void Start()
    {
        //fast access
        votesMinimum = GameManager.i.campaignScript.reviewMinVotes;
        Debug.Assert(votesMinimum > -1, "Invalid votesMinimum (-1)");
        //review button
        maxReviewButtonSize = GameManager.i.guiScript.reviewMaxButtonSize;
        reviewButtonIncrement = GameManager.i.guiScript.reviewButtonIncrement;
        reviewTimeInterval = GameManager.i.guiScript.reviewTimeInterval;
        Debug.Assert(maxReviewButtonSize > 0, "Invalid maxReviewButtonSize (Zero)");
        Debug.Assert(reviewButtonIncrement > 0, "Invalid reviewButtonIncrement (Zero)");
        Debug.Assert(reviewTimeInterval > 0, "Invalid reviewTimeInterval (Zero)");
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
                isFlashReviewButton = false;
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
    /// Initialises all UI generic tooltip elements
    /// </summary>
    private void InitialiseHelp()
    {
        List<HelpData> listOfHelp = GameManager.i.helpScript.GetHelpData("review_0", "review_1", "review_2");
        tooltipHelpOpen.SetHelpTooltip(listOfHelp, 150, 200);
        listOfHelp = GameManager.i.helpScript.GetHelpData("review_3", "review_4", "review_5");
        tooltipHelpClose.SetHelpTooltip(listOfHelp, 150, 200);
    }

    /// <summary>
    /// Open Review UI
    /// </summary>
    private void SetReviewUI(ReviewInputData details)
    {
        bool errorFlag = false;
        isOutcome = false;
        //reset timer (may have changed previously if player skipped review)
        reviewWaitTime = reviewWaitTimerDefault;
        Debug.LogFormat("[UI] ModalReviewUI.cs -> OpenReviewUI{0}", "\n");
        //set modal status
        GameManager.i.guiScript.SetIsBlocked(true);
        //activate main panel
        panelObject.SetActive(true);
        reviewObject.SetActive(true);
        headerObject.SetActive(true);
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        //buttons
        buttonReview.gameObject.SetActive(true);
        buttonExit.gameObject.SetActive(false);
        buttonHelpOpen.gameObject.SetActive(true);
        buttonHelpClose.gameObject.SetActive(false);
        //outcome symbols
        outcomeLeft.gameObject.SetActive(false);
        outcomeRight.gameObject.SetActive(false);
        //tooltips
        InitialiseHelp();
        //set up modal panel & buttons to be side appropriate
        switch (playerSide.level)
        {
            case 1:
                panelBackground.sprite = GameManager.i.sideScript.inventory_background_Authority;
                panelHeader.sprite = GameManager.i.sideScript.header_background_Authority;
                //set button sprites
                buttonReview.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Authority;
                buttonExit.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Authority;
                //set sprite transitions
                SpriteState spriteStateAuthority = new SpriteState();
                spriteStateAuthority.highlightedSprite = GameManager.i.sideScript.button_highlight_Authority;
                spriteStateAuthority.pressedSprite = GameManager.i.sideScript.button_Click;
                buttonReview.spriteState = spriteStateAuthority;
                buttonExit.spriteState = spriteStateAuthority;
                break;
            case 2:
                panelBackground.sprite = GameManager.i.sideScript.inventory_background_Resistance;
                panelHeader.sprite = GameManager.i.sideScript.header_background_Resistance;
                //set button sprites
                buttonReview.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Resistance;
                buttonExit.GetComponent<Image>().sprite = GameManager.i.sideScript.button_Resistance;
                //set sprite transitions
                SpriteState spriteStateRebel = new SpriteState();
                spriteStateRebel.highlightedSprite = GameManager.i.sideScript.button_highlight_Resistance;
                spriteStateRebel.pressedSprite = GameManager.i.sideScript.button_Click;
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
                            //tooltip data -> sprites & titles (identical)
                            if (arrayOfTooltipsSprites[i] != null)
                            {
                                if (details.arrayOfTooltipsSprite[i] != null)
                                {
                                    //sprites
                                    arrayOfTooltipsSprites[i].tooltipHeader = details.arrayOfTooltipsSprite[i].textHeader;
                                    arrayOfTooltipsSprites[i].tooltipMain = details.arrayOfTooltipsSprite[i].textMain;
                                    arrayOfTooltipsSprites[i].tooltipDetails = details.arrayOfTooltipsSprite[i].textDetails;
                                    arrayOfTooltipsSprites[i].x_offset = 55;
                                    arrayOfTooltipsSprites[i].y_offset = -10;
                                    //titles
                                    arrayOfTooltipsTitles[i].tooltipHeader = details.arrayOfTooltipsSprite[i].textHeader;
                                    arrayOfTooltipsTitles[i].tooltipMain = details.arrayOfTooltipsSprite[i].textMain;
                                    arrayOfTooltipsTitles[i].tooltipDetails = details.arrayOfTooltipsSprite[i].textDetails;
                                    arrayOfTooltipsTitles[i].x_offset = 55;
                                    arrayOfTooltipsTitles[i].y_offset = 50;
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
            //all good, review window displayed
            GameManager.i.inputScript.ModalReviewState = ModalReviewSubState.Open;
            votesFor = details.votesFor;
            votesAgainst = details.votesAgainst;
            votesAbstained = details.votesAbstained;
            ModalStateData package = new ModalStateData() { mainState = ModalSubState.Review };
            GameManager.i.inputScript.SetModalState(package);
            Debug.LogFormat("[UI] ModalInventoryUI.cs -> SetInventoryUI{0}", "\n");
            //flash review button
            isFlashReviewButton = true;
            StartCoroutine("ReviewButton");
        }
    }

    /// <summary>
    /// Shows actor review outcomes, one by one, once 'Commence Review' button pressed
    /// </summary>
    private void StartReview()
    {
        GameManager.i.inputScript.ModalReviewState = ModalReviewSubState.Review;
        //deactivate review button
        buttonReview.gameObject.SetActive(false);
        buttonHelpOpen.gameObject.SetActive(false);
        textTop.text = "";
        textBottom.text = "Press SPACE to Skip";
        Debug.LogFormat("[Tst] ModalReviewUI.cs -> StartReview: Pre Coroutine{0}", "\n");
        StartCoroutine("ShowReview");
        Debug.LogFormat("[Tst] ModalReviewUI.cs -> StartReview: Post Coroutine{0}", "\n");
        //stats
        GameManager.i.dataScript.StatisticIncrement(StatType.ReviewsTotal);      
    }

    /// <summary>
    /// Flashes 'Commence Review' button to gain player's attention by changing button size (gradually, up and down)
    /// </summary>
    /// <returns></returns>
    IEnumerator ReviewButton()
    {
        Vector2 originalDelta = buttonReview.image.rectTransform.sizeDelta;
        Vector2 currentDelta = originalDelta;
        float min_y = originalDelta.y;
        float max_y = min_y + maxReviewButtonSize;
        isGrow = true;
        do
        {
            if (isGrow == true)
            {
                //Growing
                if (buttonReview.image.rectTransform.sizeDelta.y < max_y)
                {
                    //increase button size by set increments
                    currentDelta.y += reviewButtonIncrement;
                    buttonReview.image.rectTransform.sizeDelta = currentDelta;
                }
                else
                {
                    //maxxed out size
                    isGrow = false;
                }
            }
            else
            {
                //Shrinking
                if (buttonReview.image.rectTransform.sizeDelta.y > min_y)
                {
                    //decrease button size by set increments
                    currentDelta.y -= reviewButtonIncrement;
                    buttonReview.image.rectTransform.sizeDelta = currentDelta;
                }
                else
                {
                    //reached min size
                    isGrow = true;
                }
            }
            //wait time interval
            yield return new WaitForSeconds(reviewTimeInterval);
        }
        while (isFlashReviewButton == true);
        //reset button to normal size
        buttonReview.image.rectTransform.sizeDelta = originalDelta;
        isGrow = false;
        yield return null;
    }

    /// <summary>
    /// Coroutine to sequentially display review outcomes (suspense) and, once done, review result
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowReview()
    {
        string text;
        //close any open help tooltips
        GameManager.i.tooltipHelpScript.CloseTooltip("ModalReviewUI.cs -> ShowReview");
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
        //outcome text and symbols -> must have a majority and have a minimum number of votes to get a campaign outcome
        if (votesAgainst > votesFor && votesAgainst >= votesMinimum)
        {
            outcomeText = string.Format("<size=120%>{0}</size> earned", GameManager.GetFormattedString("BLACK MARK", ColourType.badText));
            outcomeSymbol = string.Format("{0}", GameManager.GetFormattedString(GameManager.i.guiScript.blackmarkChar.ToString(), ColourType.dataTerrible));
            outcomeLeft.gameObject.SetActive(true);
            outcomeRight.gameObject.SetActive(true);
            outcomeLeft.text = outcomeSymbol;
            outcomeRight.text = outcomeSymbol;
            GameManager.i.campaignScript.ChangeBlackmarks(1, "Peer Review");
            isOutcome = true;
            //admin
            GameManager.i.dataScript.StatisticIncrement(StatType.ReviewBlackmarks);
            text = string.Format("Review Topic, votes FOR {0}, AGAINST {1}, Abstained {2}, outcome {3}{4}", votesFor, votesAgainst, votesAbstained, CampaignOutcome.Blackmark, "\n");
            GameManager.i.messageScript.TopicReview(text, votesFor, votesAgainst, votesAbstained, CampaignOutcome.Blackmark);
        }
        else if (votesFor > votesAgainst && votesFor >= votesMinimum)
        {
            outcomeText = string.Format("<size=120%>{0}</size> earned", GameManager.GetFormattedString("COMMENDATION", ColourType.goodText));
            outcomeSymbol = string.Format("{0}", GameManager.GetFormattedString(GameManager.i.guiScript.commendationChar.ToString(), ColourType.dataGood));
            outcomeLeft.gameObject.SetActive(true);
            outcomeRight.gameObject.SetActive(true);
            outcomeLeft.text = outcomeSymbol;
            outcomeRight.text = outcomeSymbol;
            GameManager.i.campaignScript.ChangeCommendations(1, "Peer Review");
            isOutcome = true;
            //admin
            GameManager.i.dataScript.StatisticIncrement(StatType.ReviewCommendations);
            text = string.Format("Review Topic, votes FOR {0}, AGAINST {1}, Abstained {2}, outcome {3}{4}", votesFor, votesAgainst, votesAbstained, CampaignOutcome.Commendation, "\n");
            GameManager.i.messageScript.TopicReview(text, votesFor, votesAgainst, votesAbstained, CampaignOutcome.Commendation);
        }
        else
        {
            outcomeText = string.Format("<size=120%>{0}</size> result", GameManager.GetFormattedString("INCONCLUSIVE", ColourType.neutralText));
            //admin
            GameManager.i.dataScript.StatisticIncrement(StatType.ReviewInconclusive);
            text = string.Format("Review Topic, votes FOR {0}, AGAINST {1}, Abstained {2}, outcome {3}{4}", votesFor, votesAgainst, votesAbstained, CampaignOutcome.Inconclusive, "\n");
            GameManager.i.messageScript.TopicReview(text, votesFor, votesAgainst, votesAbstained, CampaignOutcome.Inconclusive);
        }
        textTop.text = string.Format("Votes For {0}, Votes Against {1}{2}{3}", votesFor, votesAgainst, "\n", outcomeText);
        textBottom.text = "Press ESC or EXIT once done";
        //initialise outcome tooltips (if any)
        if (isOutcome == true)
        {
            List<HelpData> listOfHelp = GameManager.i.actorScript.GetOutcomeTooltip();
            tooltipOutcomeLeft.SetHelpTooltip(listOfHelp, 150, 50);
            tooltipOutcomeRight.SetHelpTooltip(listOfHelp, 150, 50);
        }
        //hand back control
        buttonHelpClose.gameObject.SetActive(true);
        buttonExit.gameObject.SetActive(true);
        GameManager.i.inputScript.ModalReviewState = ModalReviewSubState.Close;
        yield return null;
    }

    /// <summary>
    /// Close Review UI
    /// </summary>
    private void CloseReviewUI()
    {
        reviewObject.SetActive(false);
        GameManager.i.guiScript.SetIsBlocked(false);
        //close generic tooltip (safety check)
        GameManager.i.tooltipGenericScript.CloseTooltip("ModalReviewUI.cs -> CloseReview");
        //set game state
        GameManager.i.inputScript.ResetStates();
        Debug.LogFormat("[UI] ModalReviewUI.cs -> CloseReviewUI{0}", "\n");
        GameManager.i.inputScript.ModalReviewState = ModalReviewSubState.None;
        //close any open help tooltips
        GameManager.i.tooltipHelpScript.CloseTooltip("ModalReviewUI.cs -> ShowReview");
        //Close topicUI and go straight to MainInfoApp
        GameManager.i.guiScript.waitUntilDone = false;
    }

}
