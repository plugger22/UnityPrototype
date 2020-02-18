﻿using gameAPI;
using modalAPI;
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

    public Button buttonCancel;

    public GameObject[] arrayOfOptions;                         //place Review option UI elements here (5 x HQ options for top panel, 4 Subordinate options for bottom panel, in order Left to Right, Top to Bottom))
                                                                //HQ Boss opinion of decisions / HQ heirarchy in enum order / subordinates in slotID order
    private ReviewInteraction[] arrayOfInteractions;            //used for fast access to interaction components
    private GenericTooltipUI[] arrayOfTooltipsSprites;          //used for fast access to tooltip components (Sprites)
    private GenericTooltipUI[] arrayOfTooltipsStars;            //used for fast access to tooltip components for Stars (bottomText)

    private static ModalReviewUI modalReviewUI;
    private ButtonInteraction buttonInteraction;


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
        //cancel button event
        buttonInteraction = buttonCancel.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetButton(EventType.ReviewCloseUI); }
        else { Debug.LogError("Invalid buttonInteraction Cancel (Null)"); }
        //inventory interaction & tooltip arrays set up
        int numOfOptions = arrayOfOptions.Length;
        arrayOfInteractions = new ReviewInteraction[numOfOptions];
        arrayOfTooltipsSprites = new GenericTooltipUI[numOfOptions];
        arrayOfTooltipsStars = new GenericTooltipUI[numOfOptions];
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
                    GenericTooltipUI tooltipSprite = arrayOfOptions[i].GetComponent<GenericTooltipUI>();
                    if (tooltipSprite != null)
                    { arrayOfTooltipsSprites[i] = tooltipSprite; }
                    else { Debug.LogError(string.Format("Invalid GenericTooltipUI for arrayOfReviewOptions[{0}] (Null)", i)); }
                    //tooltip -> stars (bottomText, optional)
                    GenericTooltipUI tooltipStars = interaction.tooltipStars.GetComponent<GenericTooltipUI>();
                    if (tooltipStars != null)
                    { arrayOfTooltipsStars[i] = tooltipStars; }
                    else { Debug.LogError(string.Format("Invalid GenericTooltipUI for interaction.tooltipStars \"{0}\" (Null)", i)); }
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
                ReviewInputData details = InitialiseReviewUI();
                SetReviewUI(details);
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
    /// Initialises ReviewUI. Does so internally as the data is static
    /// </summary>
    /// <returns></returns>
    private ReviewInputData InitialiseReviewUI()
    {
        ReviewInputData data = new ReviewInputData();

        return data;
    }


    /// <summary>
    /// Open Review UI
    /// </summary>
    private void SetReviewUI(ReviewInputData details)
    {
        bool errorFlag = false;
        //set modal status
        GameManager.instance.guiScript.SetIsBlocked(true);
        //activate main panel
        panelObject.SetActive(true);
        reviewObject.SetActive(true);
        headerObject.SetActive(true);
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //populate dialogue

        //set up modal panel & buttons to be side appropriate
        switch (playerSide.level)
        {
            case 1:
                panelBackground.sprite = GameManager.instance.sideScript.inventory_background_Authority;
                panelHeader.sprite = GameManager.instance.sideScript.header_background_Authority;
                //set button sprites
                buttonCancel.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                //set sprite transitions
                SpriteState spriteStateAuthority = new SpriteState();
                spriteStateAuthority.highlightedSprite = GameManager.instance.sideScript.button_highlight_Authority;
                spriteStateAuthority.pressedSprite = GameManager.instance.sideScript.button_Click;
                buttonCancel.spriteState = spriteStateAuthority;
                break;
            case 2:
                panelBackground.sprite = GameManager.instance.sideScript.inventory_background_Resistance;
                panelHeader.sprite = GameManager.instance.sideScript.header_background_Resistance;
                //set button sprites
                buttonCancel.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Resistance;
                //set sprite transitions
                SpriteState spriteStateRebel = new SpriteState();
                spriteStateRebel.highlightedSprite = GameManager.instance.sideScript.button_highlight_Resistance;
                spriteStateRebel.pressedSprite = GameManager.instance.sideScript.button_Click;
                buttonCancel.spriteState = spriteStateRebel;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", playerSide.name));
                break;
        }
        //set texts
        textHeader.text = "Peer Review";
        textTop.text = "You are being assessed by your Peers";
        textBottom.text = "Press SPACE, or the REVIEW button";
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
                            arrayOfInteractions[i].textLower.text = details.arrayOfOptions[i].textLower;
                            arrayOfInteractions[i].optionData = details.arrayOfOptions[i].optionID;
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
                            //tooltip data -> stars
                            if (arrayOfTooltipsStars[i] != null)
                            {
                                if (details.arrayOfTooltipsStars[i] != null)
                                {
                                    arrayOfTooltipsStars[i].tooltipHeader = details.arrayOfTooltipsStars[i].textHeader;
                                    arrayOfTooltipsStars[i].tooltipMain = details.arrayOfTooltipsStars[i].textMain;
                                    arrayOfTooltipsStars[i].tooltipDetails = details.arrayOfTooltipsStars[i].textDetails;
                                    arrayOfTooltipsStars[i].x_offset = 55;
                                    arrayOfTooltipsStars[i].y_offset = 15;
                                }
                                else
                                {
                                    //this tooltip is optional, fill with blank data otherwise previously used data will be used
                                    arrayOfTooltipsStars[i].tooltipHeader = "";
                                    arrayOfTooltipsStars[i].tooltipMain = "";
                                    arrayOfTooltipsStars[i].tooltipDetails = "";
                                }
                            }
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
            ModalStateData package = new ModalStateData() { mainState = ModalSubState.Review };
            GameManager.instance.inputScript.SetModalState(package);
            Debug.LogFormat("[UI] ModalInventoryUI.cs -> SetInventoryUI{0}", "\n");
        }
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
    }

}
