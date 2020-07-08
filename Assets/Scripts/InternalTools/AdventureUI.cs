using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using toolsAPI;
using UnityEngine;
using UnityEngine.UI;


#if (UNITY_EDITOR)

/// <summary>
/// Runs adventure generator UI in Internal Tools scene
/// </summary>
public class AdventureUI : MonoBehaviour
{
    #region Overall
    //
    // - - - Overall
    //
    [Header("Overall")]
    public Canvas adventureCanvas;
    public Canvas masterCanvas;
    public Canvas newAdventureCanvas;
    public Canvas turningPointCanvas;
    public Canvas listsCanvas;

    //Stories
    private Story storyMain;
    private Story storyNew;

    //Temporary Dictionaries -> Used for Lists (populated when Lists page is open from StoryMain) and New Adventure seeding
    Dictionary<string, PlotLine> dictOfPlotLines = new Dictionary<string, PlotLine>();
    Dictionary<string, Character> dictOfCharacters = new Dictionary<string, Character>();

    //Navigation
    private int mainNavCounter;
    private int mainNavLimit;
    private int turningPointIndex;
    private int plotPointIndex;
    private bool isSaveNeeded;
    private List<Story> listOfStories;

    //static reference
    private static AdventureUI adventureUI;
    #endregion

    #region Main
    //
    // - - - Main
    //
    [Header("Main")]
    public ToolButtonInteraction newAdventureInteraction;
    public ToolButtonInteraction saveToFileInteraction;
    public ToolButtonInteraction loadFromFileInteraction;
    public ToolButtonInteraction editAdventureInteraction;
    public ToolButtonInteraction deleteFileInteraction;
    public ToolButtonInteraction removeAdventureInteraction;
    public ToolButtonInteraction clearDictionaryInteraction;
    public ToolButtonInteraction showListsInteraction;
    public ToolButtonInteraction exitInteraction;

    public Button saveButton;

    public TextMeshProUGUI themeMain1;
    public TextMeshProUGUI themeMain2;
    public TextMeshProUGUI themeMain3;
    public TextMeshProUGUI themeMain4;
    public TextMeshProUGUI themeMain5;

    public TextMeshProUGUI mainTag;
    public TextMeshProUGUI mainDate;
    public TextMeshProUGUI mainNotes;
    #endregion

    #region New Adventure
    [Header("New Adventure")]
    public ToolButtonInteraction saveNewInteraction;
    public ToolButtonInteraction turningPointNewInteraction;
    public ToolButtonInteraction clearNewInteraction;
    public ToolButtonInteraction returnNewInteraction;

    public Button newTurningButton;
    public Button newSaveButton;

    public TextMeshProUGUI themeNew1;
    public TextMeshProUGUI themeNew2;
    public TextMeshProUGUI themeNew3;
    public TextMeshProUGUI themeNew4;
    public TextMeshProUGUI themeNew5;

    public TMP_InputField newTag;
    public TMP_InputField newNotes;
    public TMP_InputField newDate;

    //collections
    public TMP_InputField[] arrayOfNewPlotLines;
    public TMP_InputField[] arrayOfNewCharacters;

    private bool isNewAdventureSave;                    //true if saving new adventure details, false if saving Turning points
    #endregion

    #region TurningPoint
    [Header("Turning Point")]
    public ToolButtonInteraction plotTurnInteraction;
    public ToolButtonInteraction clearTurnInteraction;
    public ToolButtonInteraction preSaveTurnInteraction;
    public ToolButtonInteraction saveTurnInteraction;
    public ToolButtonInteraction exitTurnInteraction;

    public Button turnPlotpointButton;
    public Button turnPreSaveButton;
    public Button turnSaveButton;

    public Image turnNameImage;                 //background panel for turning point name
    public Image turnPlotLineImage;             //background panel for plotLine2 name

    public TextMeshProUGUI turnHeader;
    public TextMeshProUGUI turnAdventureName;
    public TextMeshProUGUI turnName;

    public TextMeshProUGUI turnPlotPoint;
    public TextMeshProUGUI turnPlotLine2;
    public TextMeshProUGUI turnCharacter1;
    public TextMeshProUGUI turnCharacter2;
    public TextMeshProUGUI turnData0;
    public TextMeshProUGUI turnData1;
    public TextMeshProUGUI turnData2;
    public TextMeshProUGUI turnPlotpointNotes;

    public TextMeshProUGUI[] arrayOfTurnPlotpoints;
    public TextMeshProUGUI[] arrayOfTurnCharacters;

    public TMP_InputField turnNameInput;
    public TMP_InputField turnCharacter1Input;
    public TMP_InputField turnCharacter2Input;
    public TMP_InputField turnData1Input;
    public TMP_InputField turnData2Input;
    public TMP_InputField turnPlotNotesInput;

    private string[] arrayOfPlotpointNotes = new string[5];

    private TurningPoint turningPoint;                  //current turning point on screen
    private Plotpoint plotPoint;                        //current Plotpoint
    private Character character1;                       //current Plotpoint character1 (if any)
    private Character character2;                       //current Plotpoint character2 (if any)
    private bool isTurningPointSaved;                    //true if data has been saved

    #endregion

    #region Lists
    [Header("Lists")]
    public ToolButtonInteraction returnListsInteraction;
    public ToolButtonInteraction listEditInteraction;
    public ToolButtonInteraction listSaveInteraction;
    public TextMeshProUGUI listAdventureName;
    public TextMeshProUGUI listAdventureDate;
    public TextMeshProUGUI listName;
    public TextMeshProUGUI listDataCreated;
    public TextMeshProUGUI listDataMe;
    public TMP_InputField listCreatedInput;
    public TMP_InputField listMeInput;
    //collections
    public ToolButtonInteraction[] arrayOfPlotLineInteractions;
    public ToolButtonInteraction[] arrayOfCharacterInteractions;
    public TextMeshProUGUI[] arrayOfPlotLineTexts;
    public TextMeshProUGUI[] arrayOfCharacterTexts;
    //private
    private int maxListIndex;
    private int currentListIndex;
    private ListItemStatus listItemStatus;

    #endregion

    #region static Instance
    /// <summary>
    /// provide a static reference to AdventureUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static AdventureUI Instance()
    {
        if (!adventureUI)
        {
            adventureUI = FindObjectOfType(typeof(AdventureUI)) as AdventureUI;
            if (!adventureUI)
            { Debug.LogError("There needs to be one active adventureUI script on a GameObject in your scene"); }
        }
        return adventureUI;
    }
    #endregion

    #region Initialise
    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        //error check
        Debug.Assert(adventureCanvas != null, "Invalid adventureCanvas (Null)");
        Debug.Assert(masterCanvas != null, "Invalid masterCanvas (Null)");
        Debug.Assert(newAdventureCanvas != null, "Invalid newAdventureCanvas (Null)");
        Debug.Assert(turningPointCanvas != null, "Invalid turningPointCanvas (Null)");
        Debug.Assert(listsCanvas != null, "Invalid listsCanvas (Null)");
        //main buttons
        Debug.Assert(newAdventureInteraction != null, "Invalid newAdventureInteraction (Null)");
        Debug.Assert(saveToFileInteraction != null, "Invalid saveAdventureInteraction (Null)");
        Debug.Assert(loadFromFileInteraction != null, "Invalid loadAdventureInteraction (Null)");
        Debug.Assert(editAdventureInteraction != null, "Invalid editAdventureInteraction (Null)");
        Debug.Assert(removeAdventureInteraction != null, "Invalid removeAdventureInteraction (Null)");
        Debug.Assert(showListsInteraction != null, "Invalid showListInteraction (Null)");
        Debug.Assert(deleteFileInteraction != null, "Invalid deleteFileInteraction (Null)");
        Debug.Assert(clearDictionaryInteraction != null, "Invalid clearDictionaryInteraction (Null)");
        Debug.Assert(exitInteraction != null, "Invalid ExitInteraction (Null)");
        Debug.Assert(themeMain1 != null, "Invalid theme1 (Null)");
        Debug.Assert(themeMain2 != null, "Invalid theme2 (Null)");
        Debug.Assert(themeMain3 != null, "Invalid theme3 (Null)");
        Debug.Assert(themeMain4 != null, "Invalid theme4 (Null)");
        Debug.Assert(themeMain5 != null, "Invalid theme5 (Null)");
        Debug.Assert(mainTag != null, "Invalid mainTag (Null)");
        Debug.Assert(mainNotes != null, "Invalid mainNotes (Null)");
        Debug.Assert(mainDate != null, "Invalid mainDate (Null)");
        Debug.Assert(saveButton != null, "Invalid saveButton (Null)");
        //new adventure
        Debug.Assert(saveNewInteraction != null, "Invalid themeInteraction (Null)");
        Debug.Assert(turningPointNewInteraction != null, "Invalid turnPointInteraction (Null)");
        Debug.Assert(returnNewInteraction != null, "Invalid returnNewInteraction (Null)");
        Debug.Assert(clearNewInteraction != null, "Invalid clearNewInteraction (Null)");
        Debug.Assert(newTurningButton != null, "Invalid turningPointButton (Null)");
        Debug.Assert(newSaveButton != null, "Invalid newSaveButton (Null)");
        Debug.Assert(themeNew1 != null, "Invalid theme1 (Null)");
        Debug.Assert(themeNew2 != null, "Invalid theme2 (Null)");
        Debug.Assert(themeNew3 != null, "Invalid theme3 (Null)");
        Debug.Assert(themeNew4 != null, "Invalid theme4 (Null)");
        Debug.Assert(themeNew5 != null, "Invalid theme5 (Null)");
        Debug.Assert(newTag != null, "Invalid adventureTag (Null)");
        Debug.Assert(newNotes != null, "Invalid adventureNotes (Null)");
        Debug.Assert(newDate != null, "Invalid adventureDate (Null)");
        for (int i = 0; i < arrayOfNewPlotLines.Length; i++)
        {
            if (arrayOfNewPlotLines[i] == null) { Debug.LogErrorFormat("Invalid arrayOfNewPlotLines[{0}] (Null)", i); }
            if (arrayOfNewCharacters[i] == null) { Debug.LogErrorFormat("Invalid arrayOfNewCharacters[{0}] (Null)", i); }
        }
        //turning Points
        Debug.Assert(plotTurnInteraction != null, "Invalid plotTurnInteraction");
        Debug.Assert(clearTurnInteraction != null, "Invalid clearTurnInteraction");
        Debug.Assert(preSaveTurnInteraction != null, "Invalid preSaveTurnInteraction");
        Debug.Assert(saveTurnInteraction != null, "Invalid saveTurnInteraction");
        Debug.Assert(exitTurnInteraction != null, "Invalid exitTurnInteraction");
        Debug.Assert(turnAdventureName != null, "Invalid turnAdventureName (Null)");
        Debug.Assert(turnPlotpointButton != null, "Invalid turnPlotpointButton (Null)");
        Debug.Assert(turnPreSaveButton != null, "Invalid turnPreSaveButton (Null)");
        Debug.Assert(turnSaveButton != null, "Invalid turnSaveButton (Null)");
        Debug.Assert(turnName != null, "Invalid turnName (Null)");
        Debug.Assert(turnHeader != null, "Invalid turnHeader (Null)");
        Debug.Assert(turnPlotPoint != null, "Invalid turnPlotPoint (Null)");
        Debug.Assert(turnPlotLine2 != null, "Invalid turnPlotLine2 (Null)");
        Debug.Assert(turnCharacter1 != null, "Invalid turnCharacter1 (Null)");
        Debug.Assert(turnCharacter2 != null, "Invalid turnCharacter2 (Null)");
        Debug.Assert(turnData0 != null, "Invalid turnData0 (Null)");
        Debug.Assert(turnData1 != null, "Invalid turnData1 (Null)");
        Debug.Assert(turnData2 != null, "Invalid turnData2 (Null)");
        Debug.Assert(arrayOfTurnPlotpoints != null, "Invalid arrayOfTurnPlotpoints (Null)");
        Debug.Assert(arrayOfTurnCharacters != null, "Invalid arrayOfTurnCharacters (Null)");
        Debug.Assert(arrayOfTurnPlotpoints.Length == 5, "Invalid arrayOfTurnPlotPoints (should be 5 records, exactly)");
        Debug.Assert(arrayOfTurnCharacters.Length == 5, "Invalid arrayOfTurnCharacters (should be 5 records, exactly)");
        Debug.Assert(turnNameInput != null, "Invalid turnNameInput (Null)");
        Debug.Assert(turnCharacter1Input != null, "Invalid turnCharacter1Input (Null)");
        Debug.Assert(turnCharacter2Input != null, "Invalid turnCharacter2Input (Null)");
        Debug.Assert(turnData1Input != null, "Invalid turnData1Input (Null)");
        Debug.Assert(turnData2Input != null, "Invalid turnData12nput (Null)");
        Debug.Assert(turnPlotNotesInput != null, "Invalid turnPlotNotesInput (Null)");
        Debug.Assert(turnPlotpointNotes != null, "Invalid turnPlotpointNotes (Null)");
        Debug.Assert(turnNameImage != null, "Invalid turnNameImage (Null)");
        Debug.Assert(turnPlotLine2 != null, "Invalid turnPlotLine2 (Null)");
        //lists
        Debug.Assert(returnListsInteraction != null, "Invalid returnListsInteraction (Null)");
        Debug.Assert(listEditInteraction != null, "Invalid listEditInteraction (Null)");
        Debug.Assert(listSaveInteraction != null, "Invalid listSaveInteraction (Null)");
        Debug.Assert(listAdventureName != null, "Invalid listAdventureName (Null)");
        Debug.Assert(listAdventureDate != null, "Invalid listAdventureDate (Null)");
        Debug.Assert(listName != null, "Invalid listName (Null)");
        Debug.Assert(listDataCreated != null, "Invalid listDataCreated (Null)");
        Debug.Assert(listDataMe != null, "Invalid listDataMe (Null)");
        Debug.Assert(listCreatedInput != null, "Invalid listCreatedInput (Null)");
        Debug.Assert(listMeInput != null, "Invalid listMeInput (Null)");
        Debug.AssertFormat(arrayOfPlotLineInteractions.Length == 25, "Invalid arrayOfPlotLineInteractions.Length (should be 25, is {0})", arrayOfPlotLineInteractions.Length);
        Debug.AssertFormat(arrayOfCharacterInteractions.Length == 25, "Invalid arrayOfCharacterInteractions.Length (should be 25, is {0})", arrayOfCharacterInteractions.Length);
        Debug.AssertFormat(arrayOfPlotLineTexts.Length == 25, "Invalid arrayOfPlotLineTexts.Length (should be 25, is {0})", arrayOfPlotLineTexts.Length);
        Debug.AssertFormat(arrayOfCharacterTexts.Length == 25, "Invalid arrayOfCharacterTexts.Length (should be 25, is {0})", arrayOfCharacterTexts.Length);
        maxListIndex = arrayOfPlotLineInteractions.Length;
        //switch off
        adventureCanvas.gameObject.SetActive(false);
        masterCanvas.gameObject.SetActive(true);
        newAdventureCanvas.gameObject.SetActive(false);
        turningPointCanvas.gameObject.SetActive(false);
        listsCanvas.gameObject.SetActive(false);
        //main buttonInteractions
        exitInteraction.SetButton(ToolEventType.CloseAdventureUI);
        newAdventureInteraction.SetButton(ToolEventType.OpenNewAdventure);
        saveToFileInteraction.SetButton(ToolEventType.SaveToolsToFile);
        loadFromFileInteraction.SetButton(ToolEventType.LoadToolsFromFile);
        deleteFileInteraction.SetButton(ToolEventType.DeleteToolsFile);
        clearDictionaryInteraction.SetButton(ToolEventType.ClearAdventureDictionary);
        showListsInteraction.SetButton(ToolEventType.OpenAdventureLists);
        //new buttonInteractions
        returnNewInteraction.SetButton(ToolEventType.CloseNewAdventure);
        turningPointNewInteraction.SetButton(ToolEventType.CreateTurningPoint);
        preSaveTurnInteraction.SetButton(ToolEventType.PreSaveTurningPoint);
        saveNewInteraction.SetButton(ToolEventType.SaveAdventureToDict);
        clearNewInteraction.SetButton(ToolEventType.ClearNewAdventure);

        //turningPoint buttonInteractions
        plotTurnInteraction.SetButton(ToolEventType.CreatePlotpoint);
        clearTurnInteraction.SetButton(ToolEventType.ClearTurningPoint);
        saveTurnInteraction.SetButton(ToolEventType.SaveTurningPoint);
        exitTurnInteraction.SetButton(ToolEventType.CloseTurningPoint);
        //list buttonInteractions
        returnListsInteraction.SetButton(ToolEventType.CloseAdventureLists);
        listEditInteraction.SetButton(ToolEventType.EditListItem);
        listSaveInteraction.SetButton(ToolEventType.SaveListDetails);
        for (int i = 0; i < arrayOfPlotLineInteractions.Length; i++)
        {
            arrayOfPlotLineInteractions[i].SetButton(ToolEventType.ShowPlotLineDetails, i);
            arrayOfCharacterInteractions[i].SetButton(ToolEventType.ShowCharacterDetails, i);
        }

        //listeners
        ToolEvents.i.AddListener(ToolEventType.OpenAdventureUI, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.CloseAdventureUI, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.OpenNewAdventure, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.SaveToolsToFile, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.LoadToolsFromFile, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.DeleteToolsFile, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.CloseNewAdventure, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.SaveAdventureToDict, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.CreateTurningPoint, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ClearAdventureDictionary, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ClearNewAdventure, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.NextAdventure, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.PreviousAdventure, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.OpenAdventureLists, OnEvent, "AdventureUI");

        ToolEvents.i.AddListener(ToolEventType.CloseAdventureLists, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ShowPlotLineDetails, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ShowCharacterDetails, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.NextLists, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.PreviousLists, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.EditListItem, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.SaveListDetails, OnEvent, "AdventureUI");

        ToolEvents.i.AddListener(ToolEventType.CreatePlotpoint, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ClearTurningPoint, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.PreSaveTurningPoint, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.SaveTurningPoint, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.CloseTurningPoint, OnEvent, "AdventureUI");
    }
    #endregion

    #region Events
    /// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(ToolEventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case ToolEventType.OpenAdventureUI:
                OpenAdventureUI();
                break;
            case ToolEventType.CloseAdventureUI:
                CloseAdventureUI();
                break;
            case ToolEventType.OpenNewAdventure:
                OpenNewAdventure();
                break;
            case ToolEventType.CloseNewAdventure:
                CloseNewAdventure();
                break;
            case ToolEventType.CreateTurningPoint:
                NewTurningPoint();
                break;
            case ToolEventType.SaveAdventureToDict:
                SaveAdventureToDict();
                break;
            case ToolEventType.SaveToolsToFile:
                SaveToFile();
                break;
            case ToolEventType.LoadToolsFromFile:
                LoadFromFile();
                break;
            case ToolEventType.DeleteToolsFile:
                DeleteFile();
                break;
            case ToolEventType.ClearAdventureDictionary:
                ClearDictionary();
                break;
            case ToolEventType.ClearNewAdventure:
                ClearNewAdventure();
                break;
            case ToolEventType.NextAdventure:
                NextAdventure();
                break;
            case ToolEventType.PreviousAdventure:
                PreviousAdventure();
                break;
            case ToolEventType.OpenAdventureLists:
                OpenLists();
                break;
            case ToolEventType.CloseAdventureLists:
                CloseLists();
                break;
            case ToolEventType.ShowPlotLineDetails:
                ShowPlotLineDetails((int)Param);
                break;
            case ToolEventType.ShowCharacterDetails:
                ShowCharacterDetails((int)Param);
                break;
            case ToolEventType.NextLists:
                NextLists();
                break;
            case ToolEventType.PreviousLists:
                PreviousLists();
                break;
            case ToolEventType.EditListItem:
                EditListItem();
                break;
            case ToolEventType.SaveListDetails:
                SaveListDetails();
                break;
            case ToolEventType.CreatePlotpoint:
                NewPlotpoint();
                break;
            case ToolEventType.ClearTurningPoint:
                ClearPlotpoint();
                break;
            case ToolEventType.PreSaveTurningPoint:
                PreSaveTurningPoint();
                break;
            case ToolEventType.SaveTurningPoint:
                SaveTurningPoint();
                break;
            case ToolEventType.CloseTurningPoint:
                CloseTurningPoint();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion

    #region Main Adventure
    //
    // - - - Adventure Main
    //

    /// <summary>
    /// open
    /// </summary>
    private void OpenAdventureUI()
    {
        //turn on
        ToolManager.i.toolUIScript.CloseTools();
        adventureCanvas.gameObject.SetActive(true);
        //load data from dictionary automatically
        LoadAdventures();
        //Navigation
        GetListOfStories();
        //load up first story
        DisplayStoryMain();
        //disable Save button
        saveButton.gameObject.SetActive(false);
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Main);
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Read);
    }


    /// <summary>
    /// close
    /// </summary>
    private void CloseAdventureUI()
    {
        ToolManager.i.toolUIScript.OpenTools();
        adventureCanvas.gameObject.SetActive(false);
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Menu);
    }


    /// <summary>
    /// Write tool data to file
    /// </summary>
    private void SaveToFile()
    {
        ToolManager.i.toolFileScript.WriteToolData();
        ToolManager.i.toolFileScript.SaveToolsToFile();
        //disable button, reset flag
        isSaveNeeded = false;
        saveButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Load tool data from file (done automatically upon loading AdventureMaster)
    /// </summary>
    private void LoadFromFile()
    {
        LoadAdventures();
    }

    /// <summary>
    /// Delete tools file
    /// </summary>
    private void DeleteFile()
    {
        ToolManager.i.toolFileScript.DeleteToolsFile();
        if (storyMain != null)
        { storyMain.Reset(); }
        RedrawMainAdventurePage(true);
    }

    //subMethod to handle loading adventures and populating main screen with data from first record in dictionary
    private void LoadAdventures()
    {
        //read data from file
        if (ToolManager.i.toolFileScript.ReadToolsFromFile() == true)
        {
            //load data into game
            ToolManager.i.toolFileScript.ReadToolData();
        }
    }

    /// <summary>
    /// Empty out dictOfStories
    /// </summary>
    private void ClearDictionary()
    {
        ToolManager.i.toolDataScript.ClearDictOfStories();
        Debug.LogFormat("[Tol] AdventureUI.cs -> ClearDictionary: dictOfStories EMPTIED{0}", "\n");
    }

    /// <summary>
    /// Show next adventure in list
    /// </summary>
    private void NextAdventure()
    {
        mainNavCounter++;
        //rollover check
        if (mainNavCounter == mainNavLimit)
        { mainNavCounter = 0; }
        //show story
        DisplayStoryMain();

    }

    /// <summary>
    /// Show previous adventure in list
    /// </summary>
    private void PreviousAdventure()
    {
        mainNavCounter--;
        //rollover check
        if (mainNavCounter < 0)
        { mainNavCounter = mainNavLimit - 1; }
        //show story
        DisplayStoryMain();
    }


    #endregion

    #region New Adventure

    //
    // - - - New Adventure
    //

    /// <summary>
    /// open new adventure page
    /// </summary>
    private void OpenNewAdventure()
    {
        //create new story if none present
        if (storyNew == null)
        { storyNew = new Story(); }
        else { storyNew.Reset(); }
        //toggle canvases on/off
        newAdventureCanvas.gameObject.SetActive(true);
        masterCanvas.gameObject.SetActive(false);
        //redraw page
        RedrawNewAdventurePage();
        //switch of Turning Point button (until Adventure is SAVED)
        newTurningButton.gameObject.SetActive(false);
        //set flag
        isNewAdventureSave = true;
        //theme
        CreateTheme();
        //auto fill date
        newDate.text = GetCurrentDateString();
        storyNew.date = GetCurrentDateString();
        //clear temp dict's
        dictOfPlotLines.Clear();
        dictOfCharacters.Clear();
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.New);
    }

    /// <summary>
    /// close new adventure page
    /// </summary>
    private void CloseNewAdventure()
    {
        //toggle canvases
        masterCanvas.gameObject.SetActive(true);
        newAdventureCanvas.gameObject.SetActive(false);
        //save button on main screen
        if (isSaveNeeded == true)
        { saveButton.gameObject.SetActive(true); }
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Main);
        //copy storyNew across to storyMain
        storyMain = new Story(storyNew);
    }


    /// <summary>
    /// Create a new theme, overwrites any existing theme
    /// </summary>
    private void CreateTheme()
    {
        List<ThemeType> listOfThemes = ToolManager.i.adventureScript.GetThemes();
        if (listOfThemes != null)
        {
            if (listOfThemes.Count == 5)
            {
                storyNew.theme.arrayOfThemes[0] = listOfThemes[0];
                storyNew.theme.arrayOfThemes[1] = listOfThemes[0];
                storyNew.theme.arrayOfThemes[2] = listOfThemes[0];
                storyNew.theme.arrayOfThemes[3] = listOfThemes[0];
                themeNew1.text = string.Format("1  {0}", listOfThemes[0]);
                storyNew.theme.arrayOfThemes[4] = listOfThemes[1];
                storyNew.theme.arrayOfThemes[5] = listOfThemes[1];
                storyNew.theme.arrayOfThemes[6] = listOfThemes[1];
                themeNew2.text = string.Format("2  {0}", listOfThemes[1]);
                storyNew.theme.arrayOfThemes[7] = listOfThemes[2];
                storyNew.theme.arrayOfThemes[8] = listOfThemes[2];
                themeNew3.text = string.Format("3  {0}", listOfThemes[2]);
                storyNew.theme.arrayOfThemes[9] = listOfThemes[3];
                themeNew4.text = string.Format("4  {0}", listOfThemes[3]);
                storyNew.theme.arrayOfThemes[10] = listOfThemes[4];
                themeNew5.text = string.Format("5  {0}", listOfThemes[4]);
            }
            else { Debug.LogErrorFormat("Invalid count for listOfThemes (are {0}, should be {1})", listOfThemes.Count, 5); }
        }
        else { Debug.LogError("Invalid listOfThemes (Null)"); }
    }



    /// <summary>
    /// Clears out adventure data (excludes date and regenerates a new set of themes)
    /// </summary>
    private void ClearNewAdventure()
    {
        storyNew.tag = "";
        storyNew.notes = "";
        CreateTheme();
        RedrawNewAdventurePage();
        //toggle key buttons
        newTurningButton.gameObject.SetActive(false);
        newSaveButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Save Adventure (base data) -> Saves storyNew to ToolDataManager.cs dictOfStories
    /// </summary>
    private void SaveAdventureToDict()
    {
        if (storyNew != null)
        {
            storyNew.tag = newTag.text;
            storyNew.notes = newNotes.text;
            storyNew.date = newDate.text;
            int counterPlotLine = 0;
            int counterCharacter = 0;
            //Only new adventure details to save
            if (isNewAdventureSave == true)
            {
                //arrays
                for (int i = 0; i < arrayOfNewPlotLines.Length; i++)
                {
                    //valid name
                    if (string.IsNullOrEmpty(arrayOfNewPlotLines[i].text) == false)
                    {
                        //plotlines
                        storyNew.arrays.arrayOfPlotLines[i].tag = arrayOfNewPlotLines[i].text;
                        storyNew.arrays.arrayOfPlotLines[i].status = StoryStatus.Data;
                        counterPlotLine++;
                        //add to list
                        PlotLine plotLine = new PlotLine() { tag = arrayOfNewPlotLines[i].text };
                        AddPlotLine(plotLine);
                    }
                    //valid name
                    if (string.IsNullOrEmpty(arrayOfNewCharacters[i].text) == false)
                    {
                        //characters
                        storyNew.arrays.arrayOfCharacters[i].tag = arrayOfNewCharacters[i].text;
                        storyNew.arrays.arrayOfCharacters[i].status = StoryStatus.Data;
                        counterCharacter++;
                        //add to list
                        Character character = new Character() { tag = arrayOfNewCharacters[i].text };
                        AddCharacter(character);
                    }
                }
                //lists
                if (counterPlotLine > 0)
                {
                    //plotlines
                    storyNew.lists.listOfPlotLines.Clear();
                    storyNew.lists.listOfPlotLines.AddRange(dictOfPlotLines.Values.ToList());
                }
                if (counterCharacter > 0)
                {
                    //characters
                    storyNew.lists.listOfCharacters.Clear();
                    storyNew.lists.listOfCharacters.AddRange(dictOfCharacters.Values.ToList());
                }
            }
            //save only if viable data present
            if (string.IsNullOrEmpty(storyNew.tag) == false)
            {
                //data validate date string
                if (string.IsNullOrEmpty(storyNew.date) == true)
                { storyNew.date = GetCurrentDateString(); }
                //refTag
                storyNew.refTag = storyNew.tag.Replace(" ", "");
                //add story to dict, overwrite existing data if already present
                ToolManager.i.toolDataScript.AddStory(storyNew);
                Debug.LogFormat("[Tol] AdventureUI.cs -> SaveAdventure: StoryNew \"{0}\" saved to dictionary{1}", storyNew.refTag, "\n");
                //trigger main save to file button
                isSaveNeeded = true;
                //update navigation
                GetListOfStories();
                //switch on turningPoint button
                newTurningButton.gameObject.SetActive(true);
                //switch off save button
                newSaveButton.gameObject.SetActive(false);
            }
            else { Debug.LogWarning("Invalid storyNew.tag (Null or Empty). Story not saved to Dictionary"); }
        }
        else { Debug.LogError("Invalid storyNew (Null)"); }
    }



    #endregion

    #region Turning Point
    //
    // - - - Turning Point
    //

    #region NewTurningPoint
    /// <summary>
    /// Create New Turning Points for Adventure
    /// </summary>
    private void NewTurningPoint()
    {
        //toggle canvases
        turningPointCanvas.gameObject.SetActive(true);
        newAdventureCanvas.gameObject.SetActive(false);
        //reset save flag
        isTurningPointSaved = false;
        //redraw page
        RedrawTurningPointPage();
        //Adventure 
        turnAdventureName.text = storyNew.tag;
        //indexes
        plotPointIndex = 0;
        //new Turning point
        turningPoint = new TurningPoint() { type = TurningPointType.New };
        //set modal state
        ToolManager.i.toolInputScript.SetModalState(ToolModal.TurningPoint);
    }
    #endregion

    #region NewPlotpoint
    /// <summary>
    /// New Plotpoint
    /// </summary>
    private void NewPlotpoint()
    {
        // - - - Pre Admin
        turnData1.text = "";
        turnData2.text = "";
        //need to save previous data before generating new
        if (plotPointIndex > 0)
        {
            //update characters
            if (character1 != null)
            { character1.dataMe = turnData1Input.text; }
            if (character2 != null)
            { character2.dataMe = turnData2Input.text; }
            //Create and Save PlotDetails
            PlotDetails details = new PlotDetails()
            {
                isActive = false,
                plotPoint = plotPoint.tag,
                notes = turnPlotNotesInput.text,
                character1 = character1,
                character2 = character2
            };
            turningPoint.arrayOfDetails[plotPointIndex - 1] = details;

            Debug.LogFormat("[Tst] AdventureUI.cs -> NewPlotPoint: \"{0}\" {1} / {2} SAVED to arrayOfDetails{3}", plotPoint.tag,
                character1 != null ? character1.tag : "Nobody", character2 != null ? character2.tag : "Nobody", "\n");
        }
        //reset global characters
        character1 = null;
        character2 = null;

        // - - - New Plotline

        //check story hasn't been concluded
        if (storyNew.isConcluded == false)
        {
            //find next empty slot in array
            int index = -1;
            for (int i = 0; i < storyNew.arrayOfTurningPoints.Length; i++)
            {
                if (storyNew.arrayOfTurningPoints[i].type == TurningPointType.None)
                {
                    index = i;
                    break;
                }
            }
            if (index > -1)
            {
                if (plotPointIndex < 5)
                {
                    //zero out relevant fields
                    ResetPlotPoint();
                    //save previous notes (except for first instance as there are no notes to save)
                    if (index > 0)
                    { arrayOfPlotpointNotes[index - 1] = turnPlotNotesInput.text; }
                    // TurningPoint
                    turningPointIndex = index;
                    //Generate new Plotpoint
                    int priority = ToolManager.i.adventureScript.GetThemePriority();
                    ThemeType themeType = storyNew.theme.GetThemeType(priority);
                    plotPoint = ToolManager.i.toolDataScript.GetPlotpoint(themeType);
                    //update texts
                    turnPlotPoint.text = plotPoint.tag;
                    turnData0.text = plotPoint.details;
                    arrayOfTurnPlotpoints[plotPointIndex].text = plotPoint.tag;
                    //Plotpoint type
                    switch (plotPoint.type)
                    {
                        case PlotPointType.Normal:
                            GetCharacters(plotPoint.numberOfCharacters);
                            break;
                        case PlotPointType.Conclusion:

                            break;
                        case PlotPointType.None:

                            break;
                        case PlotPointType.NewCharacter:

                            break;
                        case PlotPointType.RemoveCharacter:

                            break;
                        case PlotPointType.Meta:

                            break;
                        default: Debug.LogWarningFormat("Unrecognised plotPoint.type \"{0}\"", plotPoint.type); break;
                    }

                    //toggle fields
                    ToggleTurningPointFields(true, plotPoint.numberOfCharacters);
                    //increment index
                    plotPointIndex++;
                    //End of turning point
                    if (plotPointIndex >= 5 || turningPoint.type == TurningPointType.Conclusion)
                    {
                        //toggle save buttons
                        turnPreSaveButton.gameObject.SetActive(true);
                        turnSaveButton.gameObject.SetActive(false);
                    }
                }
                else { Debug.LogWarning("There are already five plotponts -> Info only"); }
            }
            else
            {
                Debug.LogWarning("Story has no blank Turning Points available");
                storyNew.isConcluded = true;
                //disable plotLine button
                turnPlotpointButton.gameObject.SetActive(false);
            }
        }
        else { Debug.LogWarning("Story has been concluded -> Info only"); }
    }
    #endregion

    #region Character Methods
    /// <summary>
    /// Get characters for the plotline (0 to 3)
    /// </summary>
    /// <param name="numOfCharacters"></param>
    private void GetCharacters(int numOfCharacters)
    {
        switch (numOfCharacters)
        {
            case 0:
                //do nothing
                break;
            case 1:
                character1 = GetIndividualCharacter();
                break;
            case 2:
                character1 = GetIndividualCharacter();
                character2 = GetIndividualCharacter();
                break;
            default: Debug.LogWarningFormat("Unrecognised numOfCharacters \"{0}\"", numOfCharacters); break;
        }
        //populate fields and add to lists and arrays
        string characters = "";
        //no characters involved
        if (character1 == null && character2 == null)
        { ToggleTurningPointFields(false); }
        else
        {
            //At least one character involved
            if (character1 != null)
            {
                //read or write field depending on whether already has data
                if (string.IsNullOrEmpty(character1.dataMe) == true)
                { turnCharacter1Input.text = character1.tag; }
                else { turnCharacter1.text = character1.tag; }
                turnData1.text = character1.dataCreated;
                turnData1Input.text = character1.dataMe;
                characters = character1.tag;
            }
            if (character2 != null)
            {
                if (string.IsNullOrEmpty(character2.dataMe) == true)
                { turnCharacter2Input.text = character2.tag; }
                else { turnCharacter2.text = character2.tag; }
                turnData2.text = character2.dataCreated;
                turnData2Input.text = character2.dataMe;
                characters = string.Format("{0} / {1}", characters, character2.tag);
            }
        }
        //add to turning point list of characters (next to plotpoint)
        if (characters.Length > 0)
        { arrayOfTurnCharacters[plotPointIndex].text = characters; }
    }

    /// <summary>
    /// Sub method of GetCharacters. Returns null if a problem
    /// </summary>
    /// <returns></returns>
    private Character GetIndividualCharacter()
    {
        Character character = null;
        ListItem item = storyNew.arrays.GetCharacterFromArray();
        switch (item.status)
        {
            case StoryStatus.Data:
                character = storyNew.lists.GetCharacterFromList(item.tag);
                break;
            case StoryStatus.Logical:

                Debug.LogFormat("[Tst] AdventureUI.cs -> GetIndividualCharacter: MOST LOGICAL Character (Not implemented){0}", "\n");

                //TO DO

                break;
            case StoryStatus.New:
                character = ToolManager.i.adventureScript.GetNewCharacter();
                if (character != null)
                {
                    //add character to array and list
                    storyNew.arrays.AddCharacterToArray(new ListItem() { tag = character.refTag, status = StoryStatus.Data });
                    storyNew.lists.AddCharacterToList(character);
                }
                else { Debug.LogWarning("Invalid character (Null), NOT added to arrayOfCharacters, or listOfCharacters"); }
                break;
            default: Debug.LogWarningFormat("Unrecognised item.status \"[0}\"", item.status); break;
        }
        return character;
    }
    #endregion

    #region ClearPlotpoint
    /// <summary>
    /// Clear out current plotPoint
    /// </summary>
    private void ClearPlotpoint()
    {
        int index = plotPointIndex - 1;
        //error check for Clear button being pressed with plotPointIndex 0 (no plotPoints present)
        if (index > -1)
        {
            //clear out plotPoint
            if (arrayOfTurnPlotpoints[index].text.Length > 1)
            { arrayOfTurnPlotpoints[index].text = plotPointIndex.ToString(); }
            if (arrayOfTurnCharacters[index].text.Length > 1)
            { arrayOfTurnCharacters[index].text = plotPointIndex.ToString(); }
            arrayOfPlotpointNotes[index] = "";
            //clear out texts
            ResetPlotPoint();
            ToggleTurningPointFields(false);
            //plotPoint index back one
            plotPointIndex--;
            if (plotPointIndex < 0)
            {
                Debug.LogWarning("Invalid plotPointIndex (sub Zero)");
                plotPointIndex = 0;
            }
        }
    }
    #endregion

    #region Save Methods
    /// <summary>
    /// Pre Save operations and activate Save Button
    /// </summary>
    private void PreSaveTurningPoint()
    {
        //update characters
        if (character1 != null)
        { character1.dataMe = turnData1Input.text; }
        if (character2 != null)
        { character2.dataMe = turnData2Input.text; }
        // - - - Save last lot of details
        PlotDetails details = new PlotDetails()
        {
            isActive = false,
            plotPoint = plotPoint.tag,
            notes = turnPlotNotesInput.text,
            character1 = character1,
            character2 = character2
        };
        turningPoint.arrayOfDetails[plotPointIndex - 1] = details;
        Debug.LogFormat("[Tst] AdventureUI.cs -> NewPlotPoint: \"{0}\" {1} / {2} SAVED to arrayOfDetails{3}", plotPoint.tag,
            character1 != null ? character1.tag : "Nobody", character2 != null ? character2.tag : "Nobody", "\n");
        //Save last set of notes
        arrayOfPlotpointNotes[plotPointIndex - 1] = turnPlotNotesInput.text;
        // - - - reset global characters
        character1 = null;
        character2 = null;
        // - - - toggle fields
        turnNameInput.gameObject.SetActive(true);
        ToggleTurningPointFields(false);
        //toggle save buttons
        turnPreSaveButton.gameObject.SetActive(false);
        turnSaveButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Save turning point to StoryNew
    /// </summary>
    private void SaveTurningPoint()
    {
        //must have a name
        if (string.IsNullOrEmpty(turnNameInput.text) == false)
        {
            //Populate data (to do)
            turningPoint.tag = turnNameInput.text;
            turningPoint.refTag = turnNameInput.text.Replace(" ", "");

            //TO DO -> notes / type / isConcluded

            //TO DO -> Save PlotLine, generate new

            //SAVE
            storyNew.arrayOfTurningPoints[turningPointIndex] = turningPoint;
            isTurningPointSaved = true;
            //update indexes
            turningPointIndex++;
            plotPointIndex = 0;
            //exit or next turningPoint
            if (turningPointIndex < 5 && storyNew.isConcluded == false)
            {
                //Redraw ready for next turning point
                RedrawTurningPointPage();
            }
            else
            {
                //story concluded -> exit
                Debug.LogFormat("[Tst] AdventureUI.cs -> SaveTurningPoint: Story is CONCLUDED{0}", "\n");
                CloseTurningPoint();
            }
            //switch off save button
            turnSaveButton.gameObject.SetActive(false);
        }
        else { Debug.LogWarning("Turning Point doesn't have a name, can't be Saved"); }
    }
    #endregion

    #region Close
    /// <summary>
    /// close turningPoint page and return to New Adventure page
    /// </summary>
    private void CloseTurningPoint()
    {
        //toggle canvases
        newAdventureCanvas.gameObject.SetActive(true);
        turningPointCanvas.gameObject.SetActive(false);
        //toggle save button in New Adventure Page if data has been saved
        if (isTurningPointSaved == true)
        {
            newSaveButton.gameObject.SetActive(true);
            newTurningButton.gameObject.SetActive(false);
        }
        else
        {
            newSaveButton.gameObject.SetActive(false);
            newTurningButton.gameObject.SetActive(true);
        }
        //set flag for new Adventure save to not mess up Turning points
        isNewAdventureSave = false;
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.New);
    }

    #endregion

    #region RedrawTurningPoint
    /// <summary>
    /// Set up for new Plotpoint
    /// </summary>
    private void RedrawTurningPointPage()
    {
        //Header
        turnHeader.text = string.Format("Turning Point {0}", turningPointIndex + 1);
        //toggle fields
        turnName.gameObject.SetActive(false);
        turnNameInput.gameObject.SetActive(false);
        turnNameImage.gameObject.SetActive(false);
        turnPlotLineImage.gameObject.SetActive(false);
        turnPlotLine2.gameObject.SetActive(false);
        ToggleTurningPointFields(false);
        //toggle save buttons
        turnPreSaveButton.gameObject.SetActive(false);
        turnSaveButton.gameObject.SetActive(false);
        //clear out fields
        turnPlotPoint.text = "";
        turnPlotLine2.text = "";
        turnData0.text = "";
        turnCharacter1.text = "";
        turnCharacter2.text = "";
        turnData1.text = "";
        turnData2.text = "";
        turnData1Input.text = "";
        turnData2Input.text = "";
        turnPlotpointNotes.text = "";
        for (int i = 0; i < arrayOfTurnPlotpoints.Length; i++)
        {
            arrayOfTurnPlotpoints[i].text = string.Format("{0}", i + 1);
            arrayOfTurnCharacters[i].text = string.Format("{0}", i + 1);
            arrayOfPlotpointNotes[i] = "";
        }
    }
    #endregion

    #endregion

    #region Lists
    //
    // - - - Lists
    //

    /// <summary>
    /// Open lists of Plotlines and Characters page
    /// </summary>
    private void OpenLists()
    {
        //toggle canvases on/off
        listsCanvas.gameObject.SetActive(true);
        masterCanvas.gameObject.SetActive(false);
        //populate lists
        DisplayLists();
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Lists);
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Read);
        //toggle off input fields
        ToggleListFields(false);
        listItemStatus = ListItemStatus.None;
    }

    /// <summary>
    /// close lists of PlotLines and Characters
    /// </summary>
    private void CloseLists()
    {
        //toggle canvases on/off
        listsCanvas.gameObject.SetActive(false);
        masterCanvas.gameObject.SetActive(true);
        //update adventure display to stay in synch
        DisplayStoryMain();
        //save button on main screen
        if (isSaveNeeded == true)
        { saveButton.gameObject.SetActive(true); }
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Main);
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Read);
    }

    /// <summary>
    /// Show details of specific plotline
    /// </summary>
    /// <param name="plotLine"></param>
    private void ShowPlotLineDetails(int index)
    {
        ToggleListFields(false);
        DisplayListItem(index, true);
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Read);
    }

    /// <summary>
    /// Show details of specific character
    /// </summary>
    /// <param name="index"></param>
    private void ShowCharacterDetails(int index)
    {
        ToggleListFields(false);
        DisplayListItem(index, false);
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Read);
    }


    /// <summary>
    /// Edit a list character entry
    /// </summary>
    private void EditListItem()
    {
        ListItem item = null;
        switch (listItemStatus)
        {
            case ListItemStatus.PlotLine: item = storyMain.arrays.arrayOfPlotLines[currentListIndex]; break;
            case ListItemStatus.Character: item = storyMain.arrays.arrayOfCharacters[currentListIndex]; break;
            default: Debug.LogWarningFormat("Unrecognised listItemStatus \"{0}\"", listItemStatus); break;
        }
        if (item != null)
        {
            //can only edit if data already present
            if (item.status == StoryStatus.Data)
            {
                ToggleListFields(true);
                SetListInputFields(item);
                ToolManager.i.toolInputScript.SetModalType(ToolModalType.Edit);
            }
        }
        else { Debug.LogErrorFormat("Invalid listItem (Null) for arrayOfPlotLines[{0}]", currentListIndex); }
    }


    /// <summary>
    /// Save an edited PlotLine or Character
    /// </summary>
    private void SaveListDetails()
    {
        isSaveNeeded = true;
        SaveListInput();
        ToggleListFields(false);
        DisplayLists();
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Read);
    }

    /// <summary>
    /// Show lists for the next adventure in the list
    /// </summary>
    private void NextLists()
    {
        mainNavCounter++;
        //rollover check
        if (mainNavCounter == mainNavLimit)
        { mainNavCounter = 0; }
        //show lists
        DisplayLists();
    }

    /// <summary>
    /// Show lists for the previous adventure in the list
    /// </summary>
    private void PreviousLists()
    {
        mainNavCounter--;
        //rollover check
        if (mainNavCounter < 0)
        { mainNavCounter = mainNavLimit - 1; }
        //show story
        DisplayLists();
    }

    #endregion

    #region Dictionary Methods

    /// <summary>
    /// Returns plotline based on tag, null if a problem or not found
    /// </summary>
    /// <param name="refTag"></param>
    /// <returns></returns>
    private PlotLine GetPlotLine(string refTag)
    {
        if (dictOfPlotLines.ContainsKey(refTag) == true)
        { return dictOfPlotLines[refTag]; }
        else { Debug.LogWarningFormat("tag \"{0}\" not found in dictOfPlotlines", refTag); }
        return null;
    }

    /// <summary>
    /// Returns character based on tag, null if a problem or not found
    /// </summary>
    /// <param name="refTag"></param>
    /// <returns></returns>
    private Character GetCharacter(string refTag)
    {
        if (dictOfCharacters.ContainsKey(refTag) == true)
        { return dictOfCharacters[refTag]; }
        else { Debug.LogWarningFormat("tag \"{0}\" not found in dictOfCharacters", refTag); }
        return null;
    }

    /// <summary>
    /// Add plotline to internal temp dict
    /// </summary>
    /// <param name="plotLine"></param>
    private void AddPlotLine(PlotLine plotLine)
    {
        try
        { dictOfPlotLines.Add(plotLine.refTag, plotLine); }
        catch (ArgumentNullException)
        { Debug.LogError("Invalid plotLine (Null)"); }
        catch (ArgumentException)
        { Debug.LogWarningFormat("Duplicate plotline exists for \"{0}\", record Not added to dict -> INFO", plotLine.tag); }
    }

    /// <summary>
    /// Add character to internal temp dict
    /// </summary>
    /// <param name="character"></param>
    private void AddCharacter(Character character)
    {
        try
        { dictOfCharacters.Add(character.refTag, character); }
        catch (ArgumentNullException)
        { Debug.LogError("Invalid character (Null)"); }
        catch (ArgumentException)
        { Debug.LogWarningFormat("Duplicate character exists for \"{0}\", record Not added to dict -> INFO", character.tag); }
    }

    #endregion

    #region Utilities
    //
    // - - - Utilities
    //

    /// <summary>
    /// Update data for Main Adventure page
    /// </summary>
    private void RedrawMainAdventurePage(bool isDefault = false)
    {
        if (isDefault == false)
        {
            mainTag.text = storyMain.tag;
            mainNotes.text = storyMain.notes;
            mainDate.text = storyMain.date;
            themeMain1.text = storyMain.theme.GetThemeType(1).ToString();
            themeMain2.text = storyMain.theme.GetThemeType(2).ToString();
            themeMain3.text = storyMain.theme.GetThemeType(3).ToString();
            themeMain4.text = storyMain.theme.GetThemeType(4).ToString();
            themeMain5.text = storyMain.theme.GetThemeType(5).ToString();

        }
        else
        {
            mainTag.text = "";
            mainNotes.text = "";
            mainDate.text = "";
            themeMain1.text = "";
            themeMain2.text = "";
            themeMain3.text = "";
            themeMain4.text = "";
            themeMain5.text = "";
        }
    }

    /// <summary>
    /// Update data for New Adventure page
    /// </summary>
    private void RedrawNewAdventurePage()
    {
        newTag.text = storyNew.tag;
        newNotes.text = storyNew.notes;
        newDate.text = storyNew.date;
        themeNew1.text = storyNew.theme.GetThemeType(1).ToString();
        themeNew2.text = storyNew.theme.GetThemeType(2).ToString();
        themeNew3.text = storyNew.theme.GetThemeType(3).ToString();
        themeNew4.text = storyNew.theme.GetThemeType(4).ToString();
        themeNew5.text = storyNew.theme.GetThemeType(5).ToString();
        //zero out seed plotlines and characters regardless
        for (int i = 0; i < arrayOfNewPlotLines.Length; i++)
        {
            arrayOfNewPlotLines[i].text = "";
            arrayOfNewCharacters[i].text = "";
        }
    }

    /// <summary>
    /// returns date string with current date in format '29 Jun 20'
    /// </summary>
    /// <returns></returns>
    private string GetCurrentDateString()
    {
        DateTime date = DateTime.Now;
        return date.ToString("d MMM yy", CultureInfo.CreateSpecificCulture("en-US"));
    }

    /// <summary>
    /// Generate list used for navigation
    /// </summary>
    private void GetListOfStories()
    {
        //Generate navigation list
        listOfStories = ToolManager.i.toolDataScript.GetListOfStories();
        if (listOfStories == null) { Debug.LogError("Invalid listOfStories (Null)"); }
        else
        {
            //update navigation counters
            mainNavCounter = 0;
            mainNavLimit = listOfStories.Count;
        }
    }

    /// <summary>
    /// displays a story on Main Adventure page
    /// </summary>
    private void DisplayStoryMain()
    {
        if (listOfStories.Count > 0)
        {
            storyMain = listOfStories[mainNavCounter];
            if (storyMain != null)
            {
                //populate data onscreen
                RedrawMainAdventurePage();
            }
            else { Debug.LogWarningFormat("Invalid story (Null) from listOfStories[{0}]", mainNavCounter); }
        }
    }

    /// <summary>
    /// Display lists of Plotlines and Characters on Lists Page
    /// </summary>
    private void DisplayLists()
    {
        if (listOfStories.Count > 0)
        {
            storyMain = listOfStories[mainNavCounter];
            if (storyMain != null)
            {
                //populate temp dictionaries
                dictOfPlotLines.Clear();
                dictOfCharacters.Clear();
                dictOfPlotLines = storyMain.lists.listOfPlotLines.ToDictionary(k => k.refTag);
                dictOfCharacters = storyMain.lists.listOfCharacters.ToDictionary(k => k.refTag);
                //adventure name and date
                listAdventureName.text = storyMain.tag;
                listAdventureDate.text = storyMain.date;
                //default data for list item details
                listName.text = "";
                listDataCreated.text = "";
                listDataMe.text = "";
                //display lists
                string fade = "<alpha=#88>";
                for (int i = 0; i < arrayOfPlotLineTexts.Length; i++)
                {
                    //Plotlines
                    ListItem item = storyMain.arrays.arrayOfPlotLines[i];
                    if (item != null)
                    {
                        switch (item.status)
                        {
                            case StoryStatus.Data:
                                arrayOfPlotLineTexts[i].text = item.tag;
                                break;
                            case StoryStatus.Logical:
                                arrayOfPlotLineTexts[i].text = string.Format("{0}Choose Most Logical", fade);
                                break;
                            case StoryStatus.New:
                                arrayOfPlotLineTexts[i].text = string.Format("{0}New Plotline", fade);
                                break;
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid ListItem (Null) for storyMain.lists.arrayOfPlotLines[{0}]", i); }
                    //Characters
                    item = storyMain.arrays.arrayOfCharacters[i];
                    if (item != null)
                    {
                        switch (item.status)
                        {
                            case StoryStatus.Data:
                                arrayOfCharacterTexts[i].text = item.tag;
                                break;
                            case StoryStatus.Logical:
                                arrayOfCharacterTexts[i].text = string.Format("{0}Most Logical", fade);
                                break;
                            case StoryStatus.New:
                                arrayOfCharacterTexts[i].text = string.Format("{0}New Character", fade);
                                break;
                        }
                    }
                    else { Debug.LogErrorFormat("Invalid character (Null) for storyMain.lists.arrayOfCharacters[{0}]", i); }
                }
            }
            else { Debug.LogErrorFormat("Invalid storyMain (Null) for listOfStories[{0}]", mainNavCounter); }
        }
    }

    /// <summary>
    /// Displays a list item details on RHS list panel, either a plotLine or a character depending on 'isPlotLine
    /// </summary>
    /// <param name="isPlotLine"></param>
    private void DisplayListItem(int index, bool isPlotLine)
    {
        if (index >= 0 && index < maxListIndex)
        {
            currentListIndex = index;
            if (isPlotLine == true)
            {
                //Plotline
                listItemStatus = ListItemStatus.PlotLine;
                ListItem item = storyMain.arrays.arrayOfPlotLines[index];
                if (item != null)
                {
                    PlotLine plotLine = GetPlotLine(item.tag);
                    if (plotLine != null)
                    {
                        switch (item.status)
                        {
                            case StoryStatus.Data: listName.text = plotLine.tag; break;
                            case StoryStatus.Logical: listName.text = "Most Logical"; break;
                            case StoryStatus.New: listName.text = "New Plotline"; break;
                            default: Debug.LogWarningFormat("Unrecognised plotLine.status \"{0}\"", item.status); break;
                        }
                        listDataCreated.text = plotLine.dataCreated;
                        listDataMe.text = plotLine.dataMe;
                    }
                    else { Debug.LogErrorFormat("Invalid plotLine (Null) for ListItem.tag \"{0}\"", item.tag); }
                }
                else { Debug.LogErrorFormat("Invalid plotLine (Null) for arrayOfPlotLines[{0}]", index); }
            }
            else
            {
                //Character
                listItemStatus = ListItemStatus.Character;
                ListItem item = storyMain.arrays.arrayOfCharacters[index];
                if (item != null)
                {
                    Character character = GetCharacter(item.tag);
                    if (character != null)
                    {
                        switch (item.status)
                        {
                            case StoryStatus.Data: listName.text = character.tag; break;
                            case StoryStatus.Logical: listName.text = "Most Logical"; break;
                            case StoryStatus.New: listName.text = "New Character"; break;
                            default: Debug.LogWarningFormat("Unrecognised character.status \"{0}\"", item.status); break;
                        }
                        listDataCreated.text = character.dataCreated;
                        listDataMe.text = character.dataMe;
                    }
                    else { Debug.LogErrorFormat("Invalid character (Null) for ListItem.tag \"{0}\"", item.tag); }
                }
                else { Debug.LogErrorFormat("Invalid ListItem (Null) for arrayOfCharacters[{0}]", index); }
            }
        }
        else { Debug.LogWarningFormat("Invalid index \"{0}\" (should be between 0 and {1})", index, maxListIndex); }
    }

    /// <summary>
    /// When switching to edit mode, any existing data is displayed in the list page RHS input fields
    /// </summary>
    private void SetListInputFields(ListItem item)
    {
        switch (listItemStatus)
        {
            case ListItemStatus.PlotLine:
                //ListItem item = storyMain.arrays.arrayOfPlotLines[currentListIndex];
                if (item != null)
                {
                    PlotLine plotLine = GetPlotLine(item.tag);
                    if (plotLine != null)
                    {
                        listCreatedInput.text = plotLine.dataCreated;
                        listMeInput.text = plotLine.dataMe;
                    }
                    else { Debug.LogErrorFormat("Invalid plotLine (Null) for ListItem.tag \"{0}\"", item.tag); }
                }
                else { Debug.LogErrorFormat("Invalid ListItem (Null) for arrayOfPlotLines[{0}]", currentListIndex); }
                break;
            case ListItemStatus.Character:
                //item = storyMain.arrays.arrayOfCharacters[currentListIndex];
                if (item != null)
                {
                    Character character = GetCharacter(item.tag);
                    if (character != null)
                    {
                        listCreatedInput.text = character.dataCreated;
                        listMeInput.text = character.dataMe;
                    }
                    else { Debug.LogErrorFormat("Invalid character (Null) for ListItem.tag \"{0}\"", item.tag); }
                }
                else { Debug.LogErrorFormat("Invalid character (Null) for arrayOfCharacters[{0}]", currentListIndex); }
                break;
            default: Debug.LogWarningFormat("Unrecognised listItemStatus \"{0}\"", listItemStatus); break;
        }
    }

    /// <summary>
    /// Save input data (PlotLine or Character after Editing)
    /// </summary>
    private void SaveListInput()
    {
        switch (listItemStatus)
        {
            case ListItemStatus.PlotLine:
                ListItem item = storyMain.arrays.arrayOfPlotLines[currentListIndex];
                if (item != null)
                {
                    //can't change name of Plotline
                    item.status = StoryStatus.Data;
                    PlotLine plotLine = GetPlotLine(item.tag);
                    if (plotLine != null)
                    {
                        plotLine.dataCreated = listCreatedInput.text;
                        plotLine.dataMe = listMeInput.text;
                        //update list
                        storyMain.lists.listOfPlotLines.Clear();
                        storyMain.lists.listOfPlotLines.AddRange(dictOfPlotLines.Values.ToList());
                    }
                    else
                    {
                        Debug.LogErrorFormat("Invalid PlotLine (Null) for item.tag \"{0}\"", item.tag);
                    }
                }
                else { Debug.LogErrorFormat("Invalid plotLine (Null) for arrayOfPlotLines[{0}]", currentListIndex); }
                break;
            case ListItemStatus.Character:
                item = storyMain.arrays.arrayOfCharacters[currentListIndex];
                if (item != null)
                {
                    //can't change name of Character
                    item.status = StoryStatus.Data;
                    Character character = GetCharacter(item.tag);
                    if (character != null)
                    {
                        character.dataCreated = listCreatedInput.text;
                        character.dataMe = listMeInput.text;
                        //update list
                        storyMain.lists.listOfCharacters.Clear();
                        storyMain.lists.listOfCharacters.AddRange(dictOfCharacters.Values.ToList());
                    }
                    else { Debug.LogErrorFormat("Invalid character (Null) for item.tag \"{0}\"", item.tag); }
                }
                else { Debug.LogErrorFormat("Invalid item (Null) for arrayOfCharacters[{0}]", currentListIndex); }
                break;
            default: Debug.LogWarningFormat("Unrecognised listItemStatus \"{0}\"", listItemStatus); break;
        }
    }

    /// <summary>
    /// Toggles fields on/off for List page RHS. 
    /// </summary>
    /// <param name="isInput"></param>
    private void ToggleListFields(bool isInput)
    {
        if (isInput == true)
        {
            //toggle normal fields off
            listDataCreated.gameObject.SetActive(false);
            listDataMe.gameObject.SetActive(false);
            //toggle input On
            listCreatedInput.gameObject.SetActive(true);
            listMeInput.gameObject.SetActive(true);
        }
        else
        {
            //toggle normal fields on
            listDataCreated.gameObject.SetActive(true);
            listDataMe.gameObject.SetActive(true);
            //toggle input off
            listCreatedInput.gameObject.SetActive(false);
            listMeInput.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// toggles data display or data input for TurningPage plotpoint and character fields. If isInput true then numOfCharacters param is used to toggle the appropriate fields
    /// </summary>
    /// <param name="isInput"></param>
    private void ToggleTurningPointFields(bool isInput, int numOfCharacters = -1)
    {
        //do regardless
        turnData1.gameObject.SetActive(true);
        turnData2.gameObject.SetActive(true);
        //Input
        if (isInput == true)
        {
            if (numOfCharacters == -1)
            { Debug.LogError("Invalid numOfCharacters (-1), should be between 0 and 2"); }

            turnPlotpointNotes.gameObject.SetActive(false);
            if (numOfCharacters > 0)
            {
                turnCharacter1Input.gameObject.SetActive(true);
                turnData1Input.gameObject.SetActive(true);
            }
            if (numOfCharacters > 1)
            {
                turnCharacter2Input.gameObject.SetActive(true);
                turnData2Input.gameObject.SetActive(true);
            }
            turnPlotNotesInput.gameObject.SetActive(true);
        }
        else
        {
            //no input
            turnPlotpointNotes.gameObject.SetActive(true);
            turnCharacter1Input.gameObject.SetActive(false);
            turnCharacter2Input.gameObject.SetActive(false);
            turnData1Input.gameObject.SetActive(false);
            turnData2Input.gameObject.SetActive(false);
            turnPlotNotesInput.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Resets all relevant fields related to a Plotpoint and characters
    /// </summary>
    private void ResetPlotPoint()
    {
        turnPlotPoint.text = "";
        turnPlotLine2.text = "";
        turnData0.text = "Plotpoint Notes";
        turnCharacter1.text = "Character 1";
        turnCharacter2.text = "Character 2";
        turnCharacter1Input.text = "";
        turnCharacter2Input.text = "";
        turnData1Input.text = "";
        turnData2Input.text = "";
        turnPlotNotesInput.text = "";
    }

    #endregion

    //new scripts above here
}

#endif
