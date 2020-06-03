using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using modalAPI;
using gameAPI;



/// <summary>
/// handles modal Main Menu
/// </summary>
public class ModalMainMenu : MonoBehaviour
{
    //public GameObject modalActionObject;
    public GameObject modalMenuObject;
    public Image modalPanel;
    public Image background;
    public Image divider;
    public TextMeshProUGUI itemDetails;
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;
    public Button button5;
    public Button button6;
    public Button button7;
    public Button button8;
    public Button button9;
    public Button button10;
    public TextMeshProUGUI button1Text;
    public TextMeshProUGUI button2Text;
    public TextMeshProUGUI button3Text;
    public TextMeshProUGUI button4Text;
    public TextMeshProUGUI button5Text;
    public TextMeshProUGUI button6Text;
    public TextMeshProUGUI button7Text;
    public TextMeshProUGUI button8Text;
    public TextMeshProUGUI button9Text;
    public TextMeshProUGUI button10Text;

    private RectTransform rectTransform;
    /*private int offset;*/
    private int modalLevel;                                 //modal level of menu, passed in by ModalPanelDetails in SetMainMenu
    private GameState gameState;                                //GameState to return to if main menu closed
    private ModalSubState modalState;                          //modal state to return to if main menu closed

    //colours
    string colourNormal;
    string colourAlert;
    string colourSide;
    string colourEnd;

    private static ModalMainMenu modalMainMenu;

    public void Awake()
    {
        Debug.Assert(modalMenuObject != null, "Invalid modalMenuObject (Null)");
        Debug.Assert(modalPanel != null, "Invalid modalPanel (Null)");
        Debug.Assert(background != null, "Invalid background (Null)");
        Debug.Assert(divider != null, "Invalid divider (Null)");
        Debug.Assert(itemDetails != null, "Invalid itemDetails (Null)");
        Debug.Assert(button1 != null, "Invalid button1 (Null)");
        Debug.Assert(button2 != null, "Invalid button2 (Null)");
        Debug.Assert(button3 != null, "Invalid button3 (Null)");
        Debug.Assert(button4 != null, "Invalid button4 (Null)");
        Debug.Assert(button5 != null, "Invalid button5 (Null)");
        Debug.Assert(button6 != null, "Invalid button6 (Null)");
        Debug.Assert(button7 != null, "Invalid button7 (Null)");
        Debug.Assert(button8 != null, "Invalid button8 (Null)");
        Debug.Assert(button9 != null, "Invalid button9 (Null)");
        Debug.Assert(button10 != null, "Invalid button9 (Null)");
        Debug.Assert(button1Text != null, "Invalid button1Text (Null)");
        Debug.Assert(button2Text != null, "Invalid button2Text (Null)");
        Debug.Assert(button3Text != null, "Invalid button3Text (Null)");
        Debug.Assert(button4Text != null, "Invalid button4Text (Null)");
        Debug.Assert(button5Text != null, "Invalid button5Text (Null)");
        Debug.Assert(button6Text != null, "Invalid button6Text (Null)");
        Debug.Assert(button7Text != null, "Invalid button7Text (Null)");
        Debug.Assert(button8Text != null, "Invalid button8Text (Null)");
        Debug.Assert(button9Text != null, "Invalid button9Text (Null)");
        Debug.Assert(button10Text != null, "Invalid button10Text (Null)");
    }


    /// <summary>
    /// Static instance so the Modal Menu can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalMainMenu Instance()
    {
        if (!modalMainMenu)
        {
            modalMainMenu = FindObjectOfType(typeof(ModalMainMenu)) as ModalMainMenu;
            if (!modalMainMenu)
            { Debug.LogError("There needs to be one active ModalMainMenu script on a GameObject in your scene"); }
        }
        return modalMainMenu;
    }

    /// <summary>
    /// Initialise (Global method)
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.StartUp:
                SubInitialiseAll();
                SubInitialiseEvents();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        SetColours();
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //register listener
        EventManager.i.AddListener(EventType.OpenMainMenu, OnEvent, "ModalMainMenu");
        EventManager.i.AddListener(EventType.CloseMainMenu, OnEvent, "ModalMainMenu");
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "NodeManager");
    }
    #endregion

    #endregion

    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch(eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.OpenMainMenu:
                CreateDefaultMainMenu();
                break;
            case EventType.CloseMainMenu:
                CloseMainMenu();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Set colour palette for tooltip
    /// </summary>
    public void SetColours()
    {
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourSide = GameManager.i.colourScript.GetColour(ColourType.blueText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }

    /// <summary>
    /// Used to display main menu with all default options, centred, over the top of whatever, without initiating a background
    /// NOTE: for more control ignore this event and call mainMenuScript.InitialiseMainMenu
    /// </summary>
    private void CreateDefaultMainMenu()
    {
        //menu only accessible if player is active
        if (GameManager.i.playerScript.status == ActorStatus.Active)
        {
            ModalMainMenuDetails detailsMain = new ModalMainMenuDetails()
            {
                alignHorizontal = AlignHorizontal.Centre,
                background = Background.None,
                isCustomise = false,
                isCredits = false
            };
            //activate menu
            InitialiseMainMenu(detailsMain);
        }
        else
        {
            //display a pop-up info window
            GameManager.i.guiScript.SetAlertMessageModalOne(AlertType.MainMenuUnavailable);
        }
    }

    /// <summary>
    /// Activate modal Main Menu. Called by InitialiseMainMenu.
    /// </summary>
    /// <param name="details"></param>
    private void SetMainMenu(ModalGenericMenuDetails details)
    {
        //game state -> save current state first
        gameState = GameManager.i.inputScript.GameState;
        GameManager.i.inputScript.GameState = GameState.MainMenu;
        //turn off node tooltip if needs be
        GameManager.i.guiScript.SetTooltipsOff();
        //activate main menu
        modalMenuObject.SetActive(true);
        //set all states to off
        button1.gameObject.SetActive(false);
        button2.gameObject.SetActive(false);
        button3.gameObject.SetActive(false);
        button4.gameObject.SetActive(false);
        button5.gameObject.SetActive(false);
        button6.gameObject.SetActive(false);
        button7.gameObject.SetActive(false);
        button8.gameObject.SetActive(false);
        button9.gameObject.SetActive(false);
        button10.gameObject.SetActive(false);
        //set up ModalActionObject
        itemDetails.text = string.Format("{0}{1}{2}", details.itemName, "\n", details.itemDetails);

        /*//tooltip at top of menu -> pass through data
        ModalMenuUI modal = itemDetails.GetComponent<ModalMenuUI>();*/

        //There can be a max of 9 buttons
        int counter = 0;
        Button tempButton;
        TextMeshProUGUI title;
        foreach (EventButtonDetails buttonDetails in details.listOfButtonDetails)
        {
            tempButton = null;
            title = null;
            counter++;
            //get the relevent UI elements
            switch (counter)
            {
                case 1:
                    tempButton = button1;
                    title = button1Text;
                    break;
                case 2:
                    tempButton = button2;
                    title = button2Text;
                    break;
                case 3:
                    tempButton = button3;
                    title = button3Text;
                    break;
                case 4:
                    tempButton = button4;
                    title = button4Text;
                    break;
                case 5:
                    tempButton = button5;
                    title = button5Text;
                    break;
                case 6:
                    tempButton = button6;
                    title = button6Text;
                    break;
                case 7:
                    tempButton = button7;
                    title = button7Text;
                    break;
                case 8:
                    tempButton = button8;
                    title = button8Text;
                    break;
                case 9:
                    tempButton = button9;
                    title = button9Text;
                    break;
                case 10:
                    tempButton = button10;
                    title = button10Text;
                    break;
                default:
                    Debug.LogWarning("To many EventButtonDetails in list!\n");
                    break;
            }
            //set up the UI elements
            if (tempButton != null && title != null)
            {
                tempButton.onClick.RemoveAllListeners();
                tempButton.onClick.AddListener(CloseMainMenu);
                tempButton.onClick.AddListener(buttonDetails.action);
                title.text = buttonDetails.buttonTitle;
                tempButton.gameObject.SetActive(true);
                GenericTooltipUI generic = tempButton.GetComponent<GenericTooltipUI>();
                generic.tooltipHeader = buttonDetails.buttonTooltipHeader;
                generic.tooltipMain = buttonDetails.buttonTooltipMain;
                generic.tooltipDetails = buttonDetails.buttonTooltipDetail;
                generic.x_offset = 40;
            }
        }

        //No need to convert coordinates (already screen coords)
        Vector3 screenPos = details.menuPos;
        //update rectTransform to get a correct height as it changes every time with the dynamic menu resizing depending on number of buttons
        Canvas.ForceUpdateCanvases();
        rectTransform = modalMenuObject.GetComponent<RectTransform>();
        //get dimensions of dynamic menu
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        //place menu in centre of given position
        screenPos.x -= width / 2;
        screenPos.y += height / 2;

        /*//calculate offset - height (default above) -> No Auto adjust for hitting screen boundaries (it's wonky)
        if (screenPos.y + height + offset < Screen.height)
        { screenPos.y += height + offset; }
        else { screenPos.y -= offset; }
        //width - default right
        if (screenPos.x + offset >= Screen.width)
        { screenPos.x -= offset + screenPos.x - Screen.width; }
        //go left if needed
        else if (screenPos.x - offset - width <= 0)
        { screenPos.x += offset - width; }
        else
        { screenPos.x += offset; }*/

        //set new position
        modalMenuObject.transform.position = screenPos;
        //set states
        ModalStateData package = new ModalStateData() { mainState = ModalSubState.MainMenu };
        GameManager.i.inputScript.SetModalState(package);
        //block raycasts to gameobjects
        GameManager.i.guiScript.SetIsBlocked(true, details.modalLevel);
        modalLevel = details.modalLevel;
        modalState = details.modalState;
        Debug.LogFormat("[UI] ModalMainMenu.cs -> SetMainMenu{0}", "\n");
    }

    /// <summary>
    /// Initialises Main menu details and passes configuration data to SetMainMenu which then fires it up. Use this method instead of SetMainMenu to display menu (enables easy set up of buttons)
    /// </summary>
    /// <param name="detailsMain"></param>
    public void InitialiseMainMenu(ModalMainMenuDetails detailsMain)
    {
        //game state -> save current state first
        gameState = GameManager.i.inputScript.GameState;
        //menu
        ModalGenericMenuDetails details = new ModalGenericMenuDetails();
        details.itemName = "Main Menu";
        details.itemDetails = "2033";
        float horizontalPos = 0f;
        float verticalPos = Screen.height / 2;
        //horizontal position can vary but vertical is always centred
        switch (detailsMain.alignHorizontal)
        {
            case AlignHorizontal.Left:
                horizontalPos = Screen.width / 3;
                break;
            case AlignHorizontal.Centre:
                horizontalPos = Screen.width / 2;
                break;
            case AlignHorizontal.Right:
                horizontalPos = Screen.width * 0.6666f;
                break;
            default:
                Debug.LogErrorFormat("Unrecognised alignHorizontal \"{0}\"", detailsMain.alignHorizontal);
                break;
        }
        //position
        details.menuPos = new Vector3(horizontalPos, verticalPos);
        //
        // - - - Configure buttons (not buttons need to be in top to bottom menu display order)
        //
        //Resume button
        if (detailsMain.isResume == true)
        {
            EventButtonDetails button0 = new EventButtonDetails()
            {
                buttonTitle = "Resume",
                buttonTooltipHeader = string.Format("{0}Resume{1}", colourSide, colourEnd),
                buttonTooltipMain = string.Format("{0}Return to Game{1}", colourNormal, colourEnd),
                buttonTooltipDetail = string.Format("{0}HQ are wondering where you've gone{1}", colourAlert, colourEnd),
                action = () => { EventManager.i.PostNotification(EventType.ResumeGame, this, -1, "ModalMainMenu.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button0);
        }
        //New Game button
        if (detailsMain.isNewGame == true)
        {
            EventButtonDetails button1 = new EventButtonDetails()
            {
                buttonTitle = "New Game",
                buttonTooltipHeader = string.Format("{0}New Game{1}", colourSide, colourEnd),
                buttonTooltipMain = string.Format("{0}Start a new game{1}", colourNormal, colourEnd),
                buttonTooltipDetail = string.Format("{0}HQ are keen to get moving{1}", colourAlert, colourEnd),
                action = () => { EventManager.i.PostNotification(EventType.CreateNewGame, this, null, "ModalMainMenu.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button1);
        }
        //Load Game button
        if (detailsMain.isLoadGame == true)
        {
            EventButtonDetails button2 = new EventButtonDetails()
            {
                buttonTitle = "Load Game",
                buttonTooltipHeader = string.Format("{0}Load Game{1}", colourSide, colourEnd),
                buttonTooltipMain = string.Format("{0}Load a saved game{1}", colourNormal, colourEnd),
                buttonTooltipDetail = string.Format("{0}HQ would like now how you manage that trick?{1}", colourAlert, colourEnd),
                action = () => { EventManager.i.PostNotification(EventType.LoadGame, this, gameState, "ModalMainMenu.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button2);
        }
        //Save Game button
        if (detailsMain.isLoadGame == true)
        {
            EventButtonDetails button3 = new EventButtonDetails()
            {
                buttonTitle = "Save Game",
                buttonTooltipHeader = string.Format("{0}Save Game{1}", colourSide, colourEnd),
                buttonTooltipMain = string.Format("{0}Save your current game{1}", colourNormal, colourEnd),
                buttonTooltipDetail = string.Format("{0}HQ are working on uploading memories{1}", colourAlert, colourEnd),
                action = () => { EventManager.i.PostNotification(EventType.SaveGame, this, gameState, "ModalMainMenu.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button3);
        }
        //Options button
        if (detailsMain.isOptions == true)
        {
            EventButtonDetails button4 = new EventButtonDetails()
            {
                buttonTitle = "Options",
                buttonTooltipHeader = string.Format("{0}Options{1}", colourSide, colourEnd),
                buttonTooltipMain = string.Format("{0}Game Options{1}", colourNormal, colourEnd),
                buttonTooltipDetail = string.Format("{0}It's good to have options{1}", colourAlert, colourEnd),
                action = () => { EventManager.i.PostNotification(EventType.CreateOptions, this, gameState, "ModalMainMenu.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button4);
        }
        //Feedback button
        if (detailsMain.isFeedback == true)
        {
            EventButtonDetails button5 = new EventButtonDetails()
            {
                buttonTitle = "Feedback",
                buttonTooltipHeader = string.Format("{0}Feedback{1}", colourSide, colourEnd),
                buttonTooltipMain = string.Format("{0}Send Feedback on bugs or design{1}", colourNormal, colourEnd),
                buttonTooltipDetail = string.Format("{0}All feedback, good or bad, is much appreciated{1}", colourAlert, colourEnd),
                action = () => { EventManager.i.PostNotification(EventType.CloseMainMenu, this, -1, "ModalMainMenu.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button5);
        }
        //Customise button
        if (detailsMain.isCustomise == true)
        {
            EventButtonDetails button6 = new EventButtonDetails()
            {
                buttonTitle = "Customise",
                buttonTooltipHeader = string.Format("{0}Customise{1}", colourSide, colourEnd),
                buttonTooltipMain = string.Format("{0}Personalise the game environment in an easy to use manner{1}", colourNormal, colourEnd),
                buttonTooltipDetail = string.Format("{0}Who doesn't like to do things there way?{1}", colourAlert, colourEnd),
                action = () => { EventManager.i.PostNotification(EventType.CloseMainMenu, this, -1, "ModalMainMenu.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button6);
        }
        //Credits button
        if (detailsMain.isCredits == true)
        {
            EventButtonDetails button7 = new EventButtonDetails()
            {
                buttonTitle = "Credits",
                buttonTooltipHeader = string.Format("{0}Credits{1}", colourSide, colourEnd),
                buttonTooltipMain = string.Format("{0}The cast of thousands who made this mighty game{1}", colourNormal, colourEnd),
                buttonTooltipDetail = string.Format("{0}Make yourself a cuppa and then sit back and roll the Credits{1}", colourAlert, colourEnd),
                action = () => { EventManager.i.PostNotification(EventType.CloseMainMenu, this, gameState, "ModalMainMenu.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button7);
        }
        //Exit button
        if (detailsMain.isInformation == true)
        {
            EventButtonDetails button8 = new EventButtonDetails()
            {
                buttonTitle = "Information",
                buttonTooltipHeader = string.Format("{0}Information{1}", colourSide, colourEnd),
                buttonTooltipMain = string.Format("{0}Find info on Game Mechanics and Game Design{1}", colourNormal, colourEnd),
                buttonTooltipDetail = string.Format("{0}Information is Power{1}", colourAlert, colourEnd),
                action = () => { EventManager.i.PostNotification(EventType.CloseMainMenu, this, -1, "ModalMainMenu.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button8);
        }
        //Cancel button
        if (detailsMain.isExit == true)
        {
            EventButtonDetails button9 = new EventButtonDetails()
            {
                buttonTitle = "EXIT",
                buttonTooltipHeader = string.Format("{0}EXIT{1}", colourSide, colourEnd),
                buttonTooltipMain = string.Format("{0}Leave the game and exit to the desktop{1}", colourNormal, colourEnd),
                buttonTooltipDetail = string.Format("{0}HQ will hold the fort until you return{1}", colourAlert, colourEnd),
                action = () => { EventManager.i.PostNotification(EventType.ExitGame, this, gameState, "ModalMainMenu.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button9);
        }
        //display background (default is none)
        GameManager.i.modalGUIScript.SetBackground(detailsMain.background);
        //activate menu
        SetMainMenu(details);
    }


    /// <summary>
    /// close Main Menu
    /// </summary>
    public void CloseMainMenu()
    {
        modalMenuObject.SetActive(false);
        GameManager.i.guiScript.SetIsBlocked(false, modalLevel);
        //close Generic toolip (eg. from button)
        GameManager.i.tooltipGenericScript.CloseTooltip("ModalMainMenu -> CloseMainMenu");
        //set modal state
        GameManager.i.inputScript.ResetStates(modalState);
        //revert to previous game state if necessary (menu option may have triggered a new game state already)
        if (GameManager.i.inputScript.GameState == GameState.MainMenu)
        { GameManager.i.inputScript.GameState = gameState; }
/*        //close background (do so regardless as not a big overhead)
        GameManager.instance.modalGUIScript.DisableBackground(Background.Start);*/
        Debug.LogFormat("[UI] ModalMainMenu.cs -> CloseMainMenu{0}", "\n");
    }

    /// <summary>
    /// returns GameState at time of player selecting the Main Menu
    /// </summary>
    /// <returns></returns>
    public GameState GetExistingGameState()
    { return gameState; }
}


