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
    public Button buttonLeft;
    public Button buttonMiddle;
    public Button buttonRight;

    private Image background;
    private bool isSuccess;
    private bool isRenown;                          //true if player spent renown to avert a bad result, false otherwise
    private bool isDisplayResult;                   //if true display result of die roll
    private int result;
    private int chanceOfSuccess;
    private DiceOutcome outcome;
    private DiceReturnData returnData;
    private PassThroughDiceData passData;           //only used if gear involved, igoore otherwise
    private static ModalDiceUI modalDice;
    

    private string colourDataGood;
    private string colourDataNeutral;
    private string colourDataBad;
    private string colourEnd;
        

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
                InitialiseDiceUI((Side)Param);
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
                DiceAuto() ;
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
        colourDataGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourDataNeutral = GameManager.instance.colourScript.GetColour(ColourType.dataNeutral);
        colourDataBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
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
        isRenown = false;
        isDisplayResult = true;
        textMiddle.text = "";
        passData = details.passData;
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
    /// set up sprites on modalDiceUI window for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    private void InitialiseDiceUI(Side side)
    {
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = modalPanelObject.GetComponent<Image>();
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
        returnData.isRenown = isRenown;
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
            buttonSet_3.SetActive(true);
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
        isRenown = true;
        CloseDiceUI();
        ReturnDiceData();
    }

    /// <summary>
    /// Fires event when 'Accept Fail' button is pressed (accept a bad outcome and decline to spend renown) in buttonSet_3
    /// </summary>
    private void DiceRenownNo()
    {
        isRenown = false;
        CloseDiceUI();
        ReturnDiceData();
    }
    
    //place methods above here
}
