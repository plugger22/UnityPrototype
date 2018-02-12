using gameAPI;
using modalAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles dice rolling window and all related interaction
/// </summary>
public class ModalDiceUI : MonoBehaviour
{
    public GameObject modalDiceObject;
    public GameObject modalPanelObject;
    public GameObject buttonSet_1;
    public GameObject buttonSet_2;
    public GameObject buttonSet_3;
    public TextMeshProUGUI textTop;
    public TextMeshProUGUI textMiddle;
    public Button set1_buttonLeft;
    public Button set1_buttonMiddle;
    public Button set1_buttonRight;
    public Button set2_buttonMiddle;
    public Button set3_buttonLeft;
    public Button set3_buttonRight;

    private Image background;
    private bool isSuccess;
    private bool isRenownSpent;                     //true if player spent renown to avert a bad result, false otherwise
    private bool isEnoughRenown;                    //true if Player renown > 0
    private bool isDisplayResult;                   //if true display result of die roll
    private int result;
    private int chanceOfSuccess;
    private DiceOutcome outcome;
    private DiceReturnData returnData;
    private PassThroughDiceData passData;           //only used if gear involved, igoore otherwise
    private static ModalDiceUI modalDice;

    private static GenericTooltipUI generic_1_left;
    private static GenericTooltipUI generic_1_middle;
    private static GenericTooltipUI generic_1_right;
    private static GenericTooltipUI generic_2_middle;
    private static GenericTooltipUI generic_3_left;
    private static GenericTooltipUI generic_3_right;

    private string colourResistance;
    private string colourDataGood;
    private string colourDataNeutral;
    private string colourDataBad;
    private string colourNormal;
    private string colourEnd;


    private void Awake()
    {
        //ignore button event
        ButtonInteraction buttonInteraction = set1_buttonLeft.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetEvent(EventType.DiceIgnore); }
        else { Debug.LogError("Invalid buttonInteraction Ignore (Null)"); }
        //auto button event
        buttonInteraction = set1_buttonMiddle.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetEvent(EventType.DiceAuto); }
        else { Debug.LogError("Invalid buttonInteraction Auto (Null)"); }
        //roll button event
        buttonInteraction = set1_buttonRight.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetEvent(EventType.DiceRoll); }
        else { Debug.LogError("Invalid buttonInteraction Roll (Null)"); }
        //confirm button event
        buttonInteraction = set2_buttonMiddle.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetEvent(EventType.DiceConfirm); }
        else { Debug.LogError("Invalid buttonInteraction Confirm (Null)"); }
        //Use Renown button event
        buttonInteraction = set3_buttonLeft.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetEvent(EventType.DiceRenownYes); }
        else { Debug.LogError("Invalid buttonInteraction Use Renown (Null)"); }
        //accept failure button event
        buttonInteraction = set3_buttonRight.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetEvent(EventType.DiceRenownNo); }
        else { Debug.LogError("Invalid buttonInteraction Accept Failure (Null)"); }
    }

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        //register listeners
        EventManager.instance.AddListener(EventType.OpenDiceUI, OnEvent);
        EventManager.instance.AddListener(EventType.CloseDiceUI, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.DiceIgnore, OnEvent);
        EventManager.instance.AddListener(EventType.DiceAuto, OnEvent);
        EventManager.instance.AddListener(EventType.DiceRoll, OnEvent);
        EventManager.instance.AddListener(EventType.DiceConfirm, OnEvent);
        EventManager.instance.AddListener(EventType.DiceRenownYes, OnEvent);
        EventManager.instance.AddListener(EventType.DiceRenownNo, OnEvent);
    }

    /// <summary>
    /// provide a static reference to ModalDiceUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalDiceUI Instance()
    {
        if (!modalDice)
        {
            modalDice = FindObjectOfType(typeof(ModalDiceUI)) as ModalDiceUI;
            if (!modalDice)
            { Debug.LogError("There needs to be one active modalDice script on a GameObject in your scene"); }
        }
        return modalDice;
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
        switch (eventType)
        {
            case EventType.OpenDiceUI:
                ModalDiceDetails details = Param as ModalDiceDetails;
                InitiateDiceRoller(details);
                break;
            case EventType.CloseDiceUI:
                CloseDiceUI();
                break;
            case EventType.ChangeSide:
                ChangeSides((SideEnum)Param);
                break;
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.DiceRoll:
                DiceRoll();
                break;
            case EventType.DiceIgnore:
                DiceIgnore();
                break;
            case EventType.DiceAuto:
                DiceAuto();
                break;
            case EventType.DiceConfirm:
                DiceConfirm();
                break;
            case EventType.DiceRenownYes:
                DiceRenownYes();
                break;
            case EventType.DiceRenownNo:
                DiceRenownNo();
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
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourDataGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourDataNeutral = GameManager.instance.colourScript.GetColour(ColourType.dataNeutral);
        colourDataBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// Initialise
    /// </summary>
    /// <param name="side"></param>
    public void Initialise()
    {
        //button tooltips
        generic_1_left = set1_buttonLeft.GetComponent<GenericTooltipUI>();
        generic_1_middle = set1_buttonMiddle.GetComponent<GenericTooltipUI>();
        generic_1_right = set1_buttonRight.GetComponent<GenericTooltipUI>();
        generic_2_middle = set2_buttonMiddle.GetComponent<GenericTooltipUI>();
        generic_3_left = set3_buttonLeft.GetComponent<GenericTooltipUI>();
        generic_3_right = set3_buttonRight.GetComponent<GenericTooltipUI>();
        //initilaise button tooltips -> Set 1, Left
        if (generic_1_left != null)
        {
            generic_1_left.ToolTipHeader = string.Format("{0}IGNORE{1}", colourResistance, colourEnd);
            generic_1_left.ToolTipMain = "It's not important. Let's speed this up";
            generic_1_left.ToolTipEffect = string.Format("{0}If the roll fails there is{1}{2} NO opportunity to use Renown{3}{4} to turn it into a success{5}",
                colourNormal, colourEnd, colourDataBad, colourEnd, colourNormal, colourEnd);
        }
        else { Debug.LogError("Invalid generic_1_left button IGNORE (Null)"); }
        //set 1 -> Middle
        if (generic_1_middle != null)
        {
            generic_1_middle.ToolTipHeader = string.Format("{0}AUTO{1}", colourResistance, colourEnd);
            generic_1_middle.ToolTipMain = "Switch to Auto Pilot";
            generic_1_middle.ToolTipEffect = string.Format("{0}If the roll fails, {1}{2}Renown is used AUTOMATICALLY{3}{4} to turn it into a success{5}",
                colourNormal, colourEnd, colourDataNeutral, colourEnd, colourNormal, colourEnd);
        }
        else { Debug.LogError("Invalid generic_1_middle button AUTO (Null)"); }
        //set 1 -> Right
        if (generic_1_right != null)
        {
            generic_1_right.ToolTipHeader = string.Format("{0}ROLL{1}", colourResistance, colourEnd);
            generic_1_right.ToolTipMain = "Luck be a Lady tonight";
            generic_1_right.ToolTipEffect = string.Format("{0}If the roll fails {1}{2}you can CHOOSE to use Renown{3}{4} to turn it into a success{5}",
                colourNormal, colourEnd, colourDataGood, colourEnd, colourNormal, colourEnd);
        }
        else { Debug.LogError("Invalid generic_1_right button ROLL (Null)"); }
        //set 2 -> Middle
        if (generic_2_middle != null)
        {
            generic_2_middle.ToolTipHeader = string.Format("{0}CONFIRM{1}", colourResistance, colourEnd);
            generic_2_middle.ToolTipMain = "You cannot undo what's already been done";
        }
        else { Debug.LogError("Invalid generic_2_middle button CONFIRM (Null)"); }
        //set 3 -> Left
        if (generic_3_left != null)
        {
            generic_3_left.ToolTipHeader = string.Format("{0}USE RENOWN{1}", colourResistance, colourEnd);
            generic_3_left.ToolTipMain = "Pull strings. Shake it up. Make things happen.";
            generic_3_left.ToolTipEffect = string.Format("{0}Spend Renown to turn the roll into a Success{1}", colourDataNeutral, colourEnd);
        }
        else { Debug.LogError("Invalid generic_3_left button USE RENOWN (Null)"); }
        //set 3 -> Right
        if (generic_3_right != null)
        {
            generic_3_right.ToolTipHeader = string.Format("{0}ACCEPT FAILURE{1}", colourResistance, colourEnd);
            generic_3_right.ToolTipMain = "It doesn't always go to plan. Adapt. Move on.";
            generic_3_right.ToolTipEffect = string.Format("{0}Your gear will be compromised{1}", colourDataBad, colourEnd);
        }
        else { Debug.LogError("Invalid generic_3_right button ACCEPT FAILURE (Null)"); }
    }

    /// <summary>
    /// Main method call to initiate and run dice roller
    /// </summary>
    /// <param name="details"></param>
    /// <returns></returns>
    private void InitiateDiceRoller(ModalDiceDetails details)
    {
        returnData = new DiceReturnData();
        //set defaults
        result = -1;
        outcome = DiceOutcome.Roll;
        isSuccess = false;
        isRenownSpent = false;
        isDisplayResult = true;
        if (details.topText.Length > 0) { textTop.text = details.topText; }
        textMiddle.text = "";
        passData = details.passData;
        isEnoughRenown = details.isEnoughRenown;
        //proceed
        if (details != null)
        {

            //DICE ROLLING VISUALS GO HERE

            chanceOfSuccess = details.chance;
            //start up dice roller
            SetModalDiceUI(details);
        }
        else { Debug.LogError("Invalid ModalDiceDetails (Null)"); }
    }

    /// <summary>
    /// Initialise and activate DiceUI
    /// </summary>
    /// <param name="details"></param>
    private void SetModalDiceUI(ModalDiceDetails details)
    {
        modalDiceObject.SetActive(true);
        modalPanelObject.SetActive(true);
        //show first set of buttons and hide the rest
        buttonSet_1.SetActive(true);
        buttonSet_2.SetActive(false);
        buttonSet_3.SetActive(false);
        //set game state
        GameManager.instance.inputScript.GameState = GameState.ModalDice;
        GameManager.instance.SetIsBlocked(true);
        Debug.Log("UI: Open -> ModalDiceUI" + "\n");
    }

    /// <summary>
    /// close Action Menu
    /// </summary>
    private void CloseDiceUI()
    {
        modalDiceObject.SetActive(false);
        GameManager.instance.SetIsBlocked(false);
        //set game state
        GameManager.instance.inputScript.GameState = GameState.Normal;
        GameManager.instance.SetIsBlocked(false);
        Debug.Log("UI: Close -> ModalDiceUI" + "\n");
    }



    /// <summary>
    /// Set up sprites on ModalDiceUI window for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    private void ChangeSides(SideEnum side)
    {
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = modalPanelObject.GetComponent<Image>();
        //assign side specific sprites
        switch (side)
        {
            case SideEnum.Authority:
                background.sprite = GameManager.instance.sideScript.outcome_backgroundAuthority;
                break;
            case SideEnum.Resistance:
                background.sprite = GameManager.instance.sideScript.outcome_backgroundRebel;
                break;
        }
    }

    /// <summary>
    /// Base roll mechanic
    /// </summary>
    /// <returns></returns>
    private void ProcessRoll()
    {
        result = Random.Range(0, 100);
        if (result <= chanceOfSuccess) { isSuccess = true; }
        else { isSuccess = false; }
        //display result if applicable
        if (isDisplayResult == true)
        {
            string colourResult = colourDataGood;
            string textResult = "Success!";
            if (isSuccess == false) { colourResult = colourDataBad; textResult = "Fail"; }
            textMiddle.text = string.Format("{0}{1} {2}{3}", colourResult, textResult, result, colourEnd);
        }
    }

    /// <summary>
    /// fires event that carries all dice roller & pass through data back to the relevant calling class
    /// </summary>
    private void ReturnDiceData()
    {
        returnData.result = result;
        returnData.outcome = outcome;
        returnData.isSuccess = isSuccess;
        returnData.isRenown = isRenownSpent;
        returnData.passData = passData;
        EventManager.instance.PostNotification(EventType.DiceReturn, this, returnData);
    }

    //
    // - - - Event Methods - - -
    //

    /// <summary>
    /// triggered by 'Roll' button being pressed in buttonSet_1
    /// </summary>
    private void DiceRoll()
    {
        outcome = DiceOutcome.Roll;
        ProcessRoll();
        if (isSuccess == true)
        {
            //successful roll, press confirm and exit
            buttonSet_1.SetActive(false);
            buttonSet_2.SetActive(true);
            //update text

        }
        else
        {
            //failed roll, go to renown button options
            buttonSet_1.SetActive(false);
            //renown button options provided player has > 0 renown
            if (isEnoughRenown == true)
            {
                buttonSet_2.SetActive(false);
                buttonSet_3.SetActive(true);
            }
            //no renown available, go straight to confirm
            else
            {
                buttonSet_2.SetActive(true);
                buttonSet_3.SetActive(false);
            }
        }
    }


    /// <summary>
    /// Ignore button pressed -> bypass the whole dice roller, result will be what it will be, no opportunity to tweak result with renown in buttonSet_1
    /// </summary>
    private void DiceIgnore()
    {
        outcome = DiceOutcome.Ignore;
        isDisplayResult = false;
        ProcessRoll();
        CloseDiceUI();
        ReturnDiceData();
    }

    /// <summary>
    /// Auto resolve -> bypass dice roller, a bad result will be automatically tweaked if there is enough renown to pay the bill in buttonSet_1
    /// </summary>
    private void DiceAuto()
    {
        outcome = DiceOutcome.Auto;
        isDisplayResult = false;
        ProcessRoll();
        CloseDiceUI();
        ReturnDiceData();
    }

    /// <summary>
    /// fires event when 'Confirm' button pressed (success roll, all done) in buttonSet_12
    /// </summary>
    private void DiceConfirm()
    {
        CloseDiceUI();
        ReturnDiceData();
    }

    /// <summary>
    /// fires event when 'Use Renown' button pressed (spend renown to alleviate bad outcome) in buttonSet_3
    /// </summary>
    private void DiceRenownYes()
    {
        isRenownSpent = true;
        CloseDiceUI();
        ReturnDiceData();
    }

    /// <summary>
    /// Fires event when 'Accept Fail' button is pressed (accept a bad outcome and decline to spend renown) in buttonSet_3
    /// </summary>
    private void DiceRenownNo()
    {
        isRenownSpent = false;
        CloseDiceUI();
        ReturnDiceData();
    }
    
    //place methods above here
}
