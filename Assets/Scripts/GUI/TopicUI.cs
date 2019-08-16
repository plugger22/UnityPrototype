using gameAPI;
using packageAPI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles all topic decision UI matters
/// </summary>
public class TopicUI : MonoBehaviour
{
    [Header("Main")]
    public Canvas topicCanvas;
    public GameObject topicObject;

    [Header("Buttons")]
    public Button buttonOption0;
    public Button buttonOption1;
    public Button buttonOption2;
    public Button buttonOption3;
    public Button buttonIgnore;

    [Header("Texts")]
    public TextMeshProUGUI textHeader;
    public TextMeshProUGUI textMain;
    public TextMeshProUGUI textOption0;
    public TextMeshProUGUI textOption1;
    public TextMeshProUGUI textOption2;
    public TextMeshProUGUI textOption3;

    [Header("Sprite")]
    public Image topicImage;

    //button script handlers
    private ButtonInteraction buttonInteractiveOption0;
    private ButtonInteraction buttonInteractiveOption1;
    private ButtonInteraction buttonInteractiveOption2;
    private ButtonInteraction buttonInteractiveOption3;
    private ButtonInteraction buttonInteractiveIgnore;
    //tooltips
    private GenericTooltipUI tooltipOption0;
    private GenericTooltipUI tooltipOption1;
    private GenericTooltipUI tooltipOption2;
    private GenericTooltipUI tooltipOption3;
    private GenericTooltipUI tooltipIgnore;
    //options
    private TopicOption[] arrayOfOptions;
    private Button[] arrayOfButtons;
    private TextMeshProUGUI[] arrayOfOptionTexts;
    private ButtonInteraction[] arrayOfButtonInteractions;
    private GenericTooltipUI[] arrayOfTooltips;

    //data package
    private TopicUIData dataPackage;


    //static reference
    private static TopicUI topicUI;


    /// <summary>
    /// provide a static reference to TopicUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TopicUI Instance()
    {
        if (!topicUI)
        {
            topicUI = FindObjectOfType(typeof(TopicUI)) as TopicUI;
            if (!topicUI)
            { Debug.LogError("There needs to be one active topicUI script on a GameObject in your scene"); }
        }
        return topicUI;
    }


    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
            case GameState.FollowOnInitialisation:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        //initialise arrays
        int maxNumOfOptions = GameManager.instance.topicScript.maxOptions;
        //hard coded max num of options based on how many fields in class (need to change if maxOptions increases)
        if (maxNumOfOptions > 0 && maxNumOfOptions < 5)
        {
            arrayOfOptions = new TopicOption[maxNumOfOptions];
            arrayOfButtons = new Button[maxNumOfOptions];
            arrayOfOptionTexts = new TextMeshProUGUI[maxNumOfOptions];
            arrayOfButtonInteractions = new ButtonInteraction[maxNumOfOptions];
            arrayOfTooltips = new GenericTooltipUI[maxNumOfOptions];
        }
        else { Debug.LogErrorFormat("Invalid maxOptions \"{0}\", arrays not initialised", maxNumOfOptions); }
        //UI elements
        Debug.Assert(topicCanvas != null, "Invalid topicCanvas (Null)");
        Debug.Assert(topicObject != null, "Invalid topicObject (Null)");
        Debug.Assert(buttonOption0 != null, "Invalid buttonOption0 (Null)");
        Debug.Assert(buttonOption1 != null, "Invalid buttonOption1 (Null)");
        Debug.Assert(buttonOption2 != null, "Invalid buttonOption2 (Null)");
        Debug.Assert(buttonOption3 != null, "Invalid buttonOption3 (Null)");
        Debug.Assert(buttonIgnore != null, "Invalid buttonIgnore (Null)");
        Debug.Assert(textHeader != null, "Invalid textHeader (Null)");
        Debug.Assert(textMain != null, "Invalid textMain (Null)");
        Debug.Assert(textOption0 != null, "Invalid textOption0 (Null)");
        Debug.Assert(textOption1 != null, "Invalid textOption1 (Null)");
        Debug.Assert(textOption2 != null, "Invalid textOption2 (Null)");
        Debug.Assert(textOption3 != null, "Invalid textOption3 (Null)");
        Debug.Assert(topicImage != null, "Invalid topicImage (Null)");
        //Button Interactive
        buttonInteractiveOption0 = buttonOption0.GetComponent<ButtonInteraction>();
        buttonInteractiveOption1 = buttonOption1.GetComponent<ButtonInteraction>();
        buttonInteractiveOption2 = buttonOption2.GetComponent<ButtonInteraction>();
        buttonInteractiveOption3 = buttonOption3.GetComponent<ButtonInteraction>();
        buttonInteractiveIgnore = buttonIgnore.GetComponent<ButtonInteraction>();
        Debug.Assert(buttonInteractiveOption0 != null, "Invalid buttonInteractiveOption0 (Null)");
        Debug.Assert(buttonInteractiveOption1 != null, "Invalid buttonInteractiveOption1 (Null)");
        Debug.Assert(buttonInteractiveOption2 != null, "Invalid buttonInteractiveOption2 (Null)");
        Debug.Assert(buttonInteractiveOption3 != null, "Invalid buttonInteractiveOption3 (Null)");
        Debug.Assert(buttonInteractiveIgnore != null, "Invalid buttonInteractiveIgnore (Null)");
        //Button events
        buttonInteractiveOption0?.SetButton(EventType.TopicDisplayOption);
        buttonInteractiveOption1?.SetButton(EventType.TopicDisplayOption);
        buttonInteractiveOption2?.SetButton(EventType.TopicDisplayOption);
        buttonInteractiveOption3?.SetButton(EventType.TopicDisplayOption);
        buttonInteractiveIgnore?.SetButton(EventType.TopicDisplayIgnore);
        //Tooltipos
        tooltipOption0 = buttonOption0.GetComponent<GenericTooltipUI>();
        tooltipOption1 = buttonOption1.GetComponent<GenericTooltipUI>();
        tooltipOption2 = buttonOption2.GetComponent<GenericTooltipUI>();
        tooltipOption3 = buttonOption3.GetComponent<GenericTooltipUI>();
        tooltipIgnore = buttonIgnore.GetComponent<GenericTooltipUI>();
        Debug.Assert(tooltipOption0 != null, "Invalid tooltipOption0 (Null)");
        Debug.Assert(tooltipOption1 != null, "Invalid tooltipOption1 (Null)");
        Debug.Assert(tooltipOption2 != null, "Invalid tooltipOption2 (Null)");
        Debug.Assert(tooltipOption3 != null, "Invalid tooltipOption3 (Null)");
        Debug.Assert(tooltipIgnore != null, "Invalid tooltipIgnore (Null)");
        //populate arrayOfButtons
        arrayOfButtons[0] = buttonOption0;
        arrayOfButtons[1] = buttonOption1;
        arrayOfButtons[2] = buttonOption2;
        arrayOfButtons[3] = buttonOption3;
        //populate arrayOfButtonInteractions
        arrayOfButtonInteractions[0] = buttonInteractiveOption0;
        arrayOfButtonInteractions[1] = buttonInteractiveOption1;
        arrayOfButtonInteractions[2] = buttonInteractiveOption2;
        arrayOfButtonInteractions[3] = buttonInteractiveOption3;
        //populate arrayOfOptionTexts
        arrayOfOptionTexts[0] = textOption0;
        arrayOfOptionTexts[1] = textOption1;
        arrayOfOptionTexts[2] = textOption2;
        arrayOfOptionTexts[3] = textOption3;
        //populate arrayOfTooltips
        arrayOfTooltips[0] = tooltipOption0;
        arrayOfTooltips[1] = tooltipOption1;
        arrayOfTooltips[2] = tooltipOption2;
        arrayOfTooltips[3] = tooltipOption3;
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {


    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.instance.AddListener(EventType.TopicDisplayOpen, OnEvent, "TopicUI");
        EventManager.instance.AddListener(EventType.TopicDisplayClose, OnEvent, "TopicUI");
        EventManager.instance.AddListener(EventType.TopicDisplayIgnore, OnEvent, "TopicUI");
        EventManager.instance.AddListener(EventType.StartTurnEarly, OnEvent, "TopicUI");
        EventManager.instance.AddListener(EventType.TopicDisplayOption, OnEvent, "TopicUI");
    }
    #endregion

    #endregion


    /// <summary>
    /// Event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.TopicDisplayOpen:
                TopicUIData data = Param as TopicUIData;
                SetTopicDisplay(data);
                break;
            case EventType.TopicDisplayOption:
                ProcessTopicOption((int)Param);
                break;
            case EventType.TopicDisplayClose:
                CloseTopicUI((int)Param);
                break;
            case EventType.TopicDisplayIgnore:
                CloseTopicUI((int)Param);
                break;
            case EventType.StartTurnEarly:
                StartTurnEarly();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Pre processing admin
    /// </summary>
    private void StartTurnEarly()
    {
        //reset TopicUI.cs data package prior to end of turn topic processing
        dataPackage = null;
    }


    /// <summary>
    /// TopicManager.cs -> InitialiseTopicUI calls this to pass the data across. This is done rather than a direct call to SetTopicDisplay as it is activated conditionally (if data present) in the Msg Pipeline
    /// </summary>
    /// <param name="data"></param>
    public void InitialiseData(TopicUIData data)
    {
        if (data != null)
        { dataPackage = data; }
        else { Debug.LogError("Invalid TopicUIData (Null)"); }
    }

    /// <summary>
    /// returns true if a topic is available for display, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool CheckIsTopic()
    { return dataPackage == null ? false : true;  }


    /// <summary>
    /// Used by message pipeline to activate the TopicUI Display
    /// </summary>
    public void ActivateTopicDisplay()
    {
        if (dataPackage != null)
        { SetTopicDisplay(dataPackage); }
        else { Debug.LogError("Invalid dataPackage (Null)"); }
    }

    #region SetTopicDisplay
    /// <summary>
    /// Initialise topicUI display
    /// </summary>
    /// <param name="data"></param>
    private void SetTopicDisplay(TopicUIData data)
    {
        if (data != null)
        {

            //deactivate all options
            for (int i = 0; i < arrayOfButtons.Length; i++)
            { arrayOfButtons[i].gameObject.SetActive(false); }
            //topic header
            if (string.IsNullOrEmpty(data.header) == false)
            { textHeader.text = data.header; }
            else
            {
                Debug.LogWarningFormat("Invalid data.header (Null or Empty) for topic \"{0}\"", data.topicName);
                textHeader.text = "Unknown";
            }
            //topic text
            if (string.IsNullOrEmpty(data.text) == false)
            { textMain.text = data.text; }
            else
            {
                Debug.LogWarningFormat("Invalid data.text (Null or Empty) for topic \"{0}\"", data.topicName);
                textHeader.text = "Unknown";
            }
            //topic sprite
            if (data.sprite != null)
            { topicImage.sprite = data.sprite; }
            else
            {
                Debug.LogWarningFormat("Invalid data.header (Null or Empty) for topic \"{0}\"", data.topicName);
                //use default sprite
                topicImage.sprite = GameManager.instance.guiScript.topicSprite;
            }
            //options
            if (data.listOfOptions != null)
            {
                int count = data.listOfOptions.Count;
                if (count > 0)
                {
                    Debug.AssertFormat(count <= arrayOfOptions.Length, "Mismatch on option count (should be {0}, is {1})", arrayOfOptions.Length, count);
                    //populate arrayOfOptions
                    for (int i = 0; i < count; i++)
                    {
                        TopicOption option = data.listOfOptions[i];
                        if (option != null)
                        {
                            arrayOfOptions[i] = option;
                            //initialise option text
                            if (string.IsNullOrEmpty(option.text) == false)
                            { arrayOfOptionTexts[i].text = option.text; }
                            else
                            {
                                arrayOfOptionTexts[i].text = "Unknown";
                                Debug.LogWarningFormat("Invalid optionText (Null or Empty) for arrayOfOptionTexts[{0}], topic \"{1}\"", i, data.topicName);
                            }
                            //initialise option tooltip
                            if (string.IsNullOrEmpty(option.tooltipHeader) == false)
                            { arrayOfTooltips[i].tooltipHeader = option.tooltipHeader; }
                            if (string.IsNullOrEmpty(option.tooltipMain) == false)
                            { arrayOfTooltips[i].tooltipMain = option.tooltipMain; }
                            if (string.IsNullOrEmpty(option.tooltipDetails) == false)
                            { arrayOfTooltips[i].tooltipDetails = option.tooltipDetails; }
                            //button Interaction
                            arrayOfButtonInteractions[i].SetButton(EventType.TopicDisplayOption, i);
                            //initialise option
                            arrayOfButtons[i].gameObject.SetActive(true);
                        }
                        else
                        { Debug.LogWarningFormat("Invalid option (Null) in listOfOptions[{0}] for topic \"{1}\"", i, data.topicName); }
                    }
                }
                else { Debug.LogWarningFormat("Invalid listOfOptions (Empty) for topic \"{0}\"", data.topicName); }
            }
            else { Debug.LogWarningFormat("Invalid listOfOptions (Null) for topic \"{0}\"", data.topicName); }
            //ignore button
            buttonInteractiveIgnore.SetButton(EventType.TopicDisplayIgnore, -1);
            //Fixed position at screen centre
            Vector3 screenPos = new Vector3();
            screenPos.x = Screen.width / 2;
            screenPos.y = Screen.height / 2;
            //set position
            topicObject.transform.position = screenPos;
            //set states
            ModalStateData package = new ModalStateData() { mainState = ModalSubState.Topic };
            GameManager.instance.inputScript.SetModalState(package);
            GameManager.instance.guiScript.SetIsBlocked(true);
            Debug.LogFormat("[UI] MainInfoUI.cs -> SetTopicDisplay{0}", "\n");
            //initialise Canvas (switches one everything once all ready to go)
            topicCanvas.gameObject.SetActive(true);
        }
        else { Debug.LogError("Invalid TopicUIData (Null)"); }
    }
    #endregion


    /// <summary>
    /// close TopicUI display. Outcome if parameter >= 0, none if otherwise
    /// </summary>
    private void CloseTopicUI(int isOutcome)
    {
        GameManager.instance.tooltipGenericScript.CloseTooltip("MainInfoUI.cs -> CloseMainInfo");
        GameManager.instance.tooltipHelpScript.CloseTooltip("MainInfoUI.cs -> CloseMainInfo");
        topicCanvas.gameObject.SetActive(false);

        //switch of AlertUI 
        GameManager.instance.alertScript.CloseAlertUI();

        /*//set game state -> EDIT: Don't do this as you are going straight to the maininfoApp on every occasion and resetting the state allows clicks to node in the interim.
        GameManager.instance.inputScript.ResetStates();
        GameManager.instance.guiScript.SetIsBlocked(false);*/

        //set waitUntilDone false to continue with pipeline only if there is no outcome from the topic option selected
        if (isOutcome < 0)
        {
            //No outcome -> close topicUI and go straight to MainInfoApp
            GameManager.instance.guiScript.waitUntilDone = false;
        }
    }

    /// <summary>
    /// Process selected Topic Option (index range 0 -> 3 (maxOptions -1)
    /// </summary>
    /// <param name="optionIndex"></param>
    private void ProcessTopicOption(int optionIndex)
    {
        if (optionIndex >= 0 && optionIndex < GameManager.instance.topicScript.maxOptions)
        {
            //close TopicUI (with parameter > -1 to indicate outcome window required)
            CloseTopicUI(optionIndex);
            //actual processing of selected option is handled by topicManager.cs
            GameManager.instance.topicScript.ProcessOption(optionIndex);
        }
        else
        {
            Debug.LogWarningFormat("Invalid optionIndex \"{0}\" (should be >= 0 and < maxOptions). Outcome cancelled", optionIndex);
            //close TopicUI without an Outcome dialogue
            CloseTopicUI(-1);
        }
    }


    //new methods above here
}
