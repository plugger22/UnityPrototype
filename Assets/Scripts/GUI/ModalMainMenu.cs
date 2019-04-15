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
    public Text button1Text;
    public Text button2Text;
    public Text button3Text;
    public Text button4Text;
    public Text button5Text;
    public Text button6Text;
    
    [HideInInspector] public string tooltip1;
    [HideInInspector] public string tooltip2;
    [HideInInspector] public string tooltip3;
    [HideInInspector] public string tooltip4;
    [HideInInspector] public string tooltip5;


    private RectTransform rectTransform;
    private int offset;
    private int modalLevel;                                 //modal level of menu, passed in by ModalPanelDetails in SetMainMenu
    private ModalState modalState;                          //modal state to return to if main menu closed

    //colour palette
    /*private string colourEffects;
    private string colourEnd;*/

    private static ModalMainMenu modalMainMenu;
    
    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        offset = GameManager.instance.tooltipScript.tooltipOffset * 2;
        //register listener
        EventManager.instance.AddListener(EventType.CloseMainMenu, OnEvent, "ModalMainMenu");
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
            case EventType.CloseMainMenu:
                CloseMainMenu();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Activate modal Main Menu. Called by InitialiseMainMenu.
    /// </summary>
    /// <param name="details"></param>
    private void SetMainMenu(ModalGenericMenuDetails details)
    {
        //activate main menu
        modalMenuObject.SetActive(true);
        //set all states to off
        button1.gameObject.SetActive(false);
        button2.gameObject.SetActive(false);
        button3.gameObject.SetActive(false);
        button4.gameObject.SetActive(false);
        button5.gameObject.SetActive(false);
        button6.gameObject.SetActive(false);

        //set up ModalActionObject
        itemDetails.text = string.Format("{0}{1}{2}", details.itemName, "\n", details.itemDetails);
        //tooltip at top of menu -> pass through data
        ModalMenuUI modal = itemDetails.GetComponent<ModalMenuUI>();
        //There can be a max of 6 buttons (5 plus 1 x Cancel)
        int counter = 0;
        Button tempButton;
        Text title;
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
        ModalStateData package = new ModalStateData() { mainState = ModalState.MainMenu };
        GameManager.instance.inputScript.SetModalState(package);
        //block raycasts to gameobjects
        GameManager.instance.guiScript.SetIsBlocked(true, details.modalLevel);
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
                buttonTooltipHeader = "Placeholder",
                buttonTooltipMain = "Placeholder",
                buttonTooltipDetail = "Placeholder",
                action = () => { EventManager.instance.PostNotification(EventType.CloseMainMenu, this, -1, "GameManager.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button0);
        }
        //New Game button
        if (detailsMain.isNewGame == true)
        {
            EventButtonDetails button1 = new EventButtonDetails()
            {
                buttonTitle = "New Game",
                buttonTooltipHeader = "Placeholder",
                buttonTooltipMain = "Placeholder",
                buttonTooltipDetail = "Placeholder",
                action = () => { EventManager.instance.PostNotification(EventType.CloseMainMenu, this, -1, "GameManager.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button1);
        }
        //Load Game button
        if (detailsMain.isLoadGame == true)
        {
            EventButtonDetails button2 = new EventButtonDetails()
            {
                buttonTitle = "Load Game",
                buttonTooltipHeader = "Placeholder",
                buttonTooltipMain = "Placeholder",
                buttonTooltipDetail = "Placeholder",
                action = () => { EventManager.instance.PostNotification(EventType.CloseMainMenu, this, -1, "GameManager.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button2);
        }
        //Options button
        if (detailsMain.isOptions == true)
        {
            EventButtonDetails button3 = new EventButtonDetails()
            {
                buttonTitle = "Options",
                buttonTooltipHeader = "Placeholder",
                buttonTooltipMain = "Placeholder",
                buttonTooltipDetail = "Placeholder",
                action = () => { EventManager.instance.PostNotification(EventType.CloseMainMenu, this, -1, "GameManager.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button3);
        }
        //Feedback button
        if (detailsMain.isFeedback == true)
        {
            EventButtonDetails button4 = new EventButtonDetails()
            {
                buttonTitle = "Feedback",
                buttonTooltipHeader = "Placeholder",
                buttonTooltipMain = "Placeholder",
                buttonTooltipDetail = "Placeholder",
                action = () => { EventManager.instance.PostNotification(EventType.CloseMainMenu, this, -1, "GameManager.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button4);
        }
        //Customise button
        if (detailsMain.isCustomise == true)
        {
            EventButtonDetails button5 = new EventButtonDetails()
            {
                buttonTitle = "Customise",
                buttonTooltipHeader = "Placeholder",
                buttonTooltipMain = "Placeholder",
                buttonTooltipDetail = "Placeholder",
                action = () => { EventManager.instance.PostNotification(EventType.CloseMainMenu, this, -1, "GameManager.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button5);
        }
        //Credits button
        if (detailsMain.isCredits == true)
        {
            EventButtonDetails button6 = new EventButtonDetails()
            {
                buttonTitle = "Credits",
                buttonTooltipHeader = "Placeholder",
                buttonTooltipMain = "Placeholder",
                buttonTooltipDetail = "Placeholder",
                action = () => { EventManager.instance.PostNotification(EventType.CloseMainMenu, this, -1, "GameManager.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button6);
        }
        //Exit button
        if (detailsMain.isExit == true)
        {
            EventButtonDetails button7 = new EventButtonDetails()
            {
                buttonTitle = "EXIT",
                buttonTooltipHeader = "Placeholder",
                buttonTooltipMain = "Placeholder",
                buttonTooltipDetail = "Placeholder",
                action = () => { EventManager.instance.PostNotification(EventType.CloseMainMenu, this, -1, "GameManager.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button7);
        }
        //Cancel button
        if (detailsMain.isCancel == true)
        {
            EventButtonDetails button8 = new EventButtonDetails()
            {
                buttonTitle = "CANCEL",
                buttonTooltipHeader = "Placeholder",
                buttonTooltipMain = "Placeholder",
                buttonTooltipDetail = "Placeholder",
                action = () => { EventManager.instance.PostNotification(EventType.CloseMainMenu, this, -1, "GameManager.cs -> InitialiseMainMenu"); }
            };
            details.listOfButtonDetails.Add(button8);
        }
        //display background (default is none)
        GameManager.instance.modalGUIScript.SetBackground(detailsMain.background);
        //activate menu
        SetMainMenu(details);
    }


    /// <summary>
    /// close Main Menu
    /// </summary>
    public void CloseMainMenu()
    {
        modalMenuObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false, modalLevel);
        //remove highlight from node
        GameManager.instance.nodeScript.ToggleNodeHighlight();
        //close Alert UI safety check (ignored if not active)
        GameManager.instance.alertScript.CloseAlertUI();
        //close Generic toolip (eg. from button)
        GameManager.instance.tooltipGenericScript.CloseTooltip("ModalMainMenu -> CloseMainMenu");
        //set game state
        GameManager.instance.inputScript.ResetStates(modalState);
        //close background (do so regardless as not a big overhead)
        GameManager.instance.modalGUIScript.DisableBackground(Background.Start);
        Debug.LogFormat("[UI] ModalMainMenu.cs -> CloseMainMenu{0}", "\n");
    }
}


