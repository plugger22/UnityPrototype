using System;
using System.Collections;
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
    // - - - Globals

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
    public Canvas dropDownCanvas;
    public Canvas constantsCanvas;

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
    private int constantIndex;                              //listOfConstantPlotpoints index
    private int constantLimit;                              //listOfConstantPlotpoints.Count
    private bool isSaveNeeded;                              //activates saveToFile button
    private List<Story> listOfStories;
    private List<ConstantPlotpoint> listOfConstantPlotpoints;

    //coroutines
    private IEnumerator coroutineCharacter;
    private IEnumerator coroutineDropDown;
    private IEnumerator coroutinePlotLine;
    private IEnumerator coroutineNameSet;

    //colours
    private Color32 colourYellow = new Color32(243, 253, 106, 255);
    private Color32 colourWhite = new Color32(255, 255, 255, 255);

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
    public ToolButtonInteraction saveAdventureInteraction;
    public ToolButtonInteraction deleteFileInteraction;
    public ToolButtonInteraction removeAdventureInteraction;
    public ToolButtonInteraction clearDictionaryInteraction;
    public ToolButtonInteraction exportDictionaryInteraction;
    public ToolButtonInteraction exportOrgsInteraction;
    public ToolButtonInteraction showListsInteraction;
    public ToolButtonInteraction constantsInteraction;
    public ToolButtonInteraction exitInteraction;

    public Button saveFileButton;
    public Button editStoryButton;
    public Button saveStoryButton;

    public TextMeshProUGUI themeMain1;
    public TextMeshProUGUI themeMain2;
    public TextMeshProUGUI themeMain3;
    public TextMeshProUGUI themeMain4;
    public TextMeshProUGUI themeMain5;

    public TextMeshProUGUI mainTag;
    public TextMeshProUGUI mainDate;
    public TextMeshProUGUI mainNotes;
    public TMP_InputField mainNotesInput;

    public Image MainTurnPanel;
    public Image MainSummaryPanel;

    //Summary
    public TextMeshProUGUI mainTurningPointNotes;
    public TextMeshProUGUI mainInstructions;

    //Details
    public TextMeshProUGUI mainTurnNumber;
    public TextMeshProUGUI mainTurnName;
    public TextMeshProUGUI mainTurnNotes;

    //Arrays
    public TextMeshProUGUI[] arrayOfMainTurningPoints;
    public TextMeshProUGUI[] arrayOfMainPlotPoints;
    public TextMeshProUGUI[] arrayOfMainCharacters;
    public Toggle[] arrayOfMainToggles;                      //should be 3 entries, new / development / conclusion, in that order

    //Summary -> private
    private string[] arrayOfMainTurningPointNotes = new string[5];
    private int mainSummaryIndex;                        //controls arrow up and down for turning points in summary panel
    private int mainDetailsIndex;                        //controls arrow up and down for plotPoints in details panel

    //Details -> private

    #endregion

    #region New Adventure
    [Header("New Adventure")]
    public ToolButtonInteraction saveNewInteraction;
    public ToolButtonInteraction turningPointNewInteraction;
    public ToolButtonInteraction clearNewInteraction;
    public ToolButtonInteraction returnNewInteraction;
    public ToolButtonInteraction nameSetInteraction;

    public Button newTurningButton;
    public Button newSaveButton;

    public TextMeshProUGUI themeNew1;
    public TextMeshProUGUI themeNew2;
    public TextMeshProUGUI themeNew3;
    public TextMeshProUGUI themeNew4;
    public TextMeshProUGUI themeNew5;
    public TextMeshProUGUI newNameSetHeader;

    public TMP_InputField newTag;
    public TMP_InputField newNotes;
    public TMP_InputField newDate;

    public Image NewNormalPanel;
    public Image NewSummaryPanel;

    //Summary
    public TextMeshProUGUI newTurningPointNotes;

    //collections
    public TMP_InputField[] arrayOfNewPlotLines;
    public TMP_InputField[] arrayOfNewCharacters;
    public TextMeshProUGUI[] arrayOfNewTurningPoints;

    private string[] arrayOfNewTurningPointNotes = new string[5];

    private bool isNewAdventureSave;                    //true if saving new adventure details, false if saving Turning points
    private int newSummaryIndex;                        //controls arrow up and down for turning points in summary panel
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

    public TextMeshProUGUI turnHeader;
    public TextMeshProUGUI turnNameSetHeader;
    public TextMeshProUGUI turnAdventureName;
    public TextMeshProUGUI turnName;

    public TextMeshProUGUI turnPlotPoint;
    public TextMeshProUGUI turnCharacter1;
    public TextMeshProUGUI turnCharacter2;
    public TextMeshProUGUI turnData0;
    public TextMeshProUGUI turnData1;
    public TextMeshProUGUI turnData2;
    public TextMeshProUGUI turnPlotpointNotes;

    public TextMeshProUGUI[] arrayOfTurnPlotpoints;             //handles display data (plotpoints)
    public TextMeshProUGUI[] arrayOfTurnCharacters;             //handles display data (characters)

    public TMP_InputField turnNameInput;
    public TMP_InputField turnCharacter1Input;
    public TMP_InputField turnCharacter2Input;
    public TMP_InputField turnData0Input;
    public TMP_InputField turnData1Input;
    public TMP_InputField turnData2Input;
    public TMP_InputField turnPlotNotesInput;

    private string[] arrayOfPlotpointNotes = new string[5];

    private TurningPoint turningPoint;                  //current turning point on screen
    private PlotLine plotLine;                          //current PlotLine
    private Plotpoint plotPoint;                        //current Plotpoint
    private Plotpoint plotPointNone;                    //'None' plotPoint for reference
    private Character character1;                       //current Plotpoint character1 (if any)
    private Character character2;                       //current Plotpoint character2 (if any)
    private bool isChar1MostLogical;
    private bool isChar2MostLogical;
    private bool isWaitUntilDone;                       //used for dropDownInput
    private bool isPlotpointAdminDone;                  //used for character plotpoints (admin via coroutine) and non-character plotpoints (admin via NewPlotpoint)
    private bool isTurningPointSaved;                   //true if data has been saved
    private bool isSavePlotPoint;                       //true if plotpoint needs to be saved at start of NewPlotPoint (normal). Would be false in case of ClearPlotPoint
    private int numberOfClears;                         //tracks number of times 'Clear' has been clicked. Reset by NewPlotPoint. Needed to correctly access character data for clearing

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

    #region DropDown
    /// <summary>
    /// drop down input gui elements
    /// </summary>
    public TextMeshProUGUI dropHeader;
    public TMP_Dropdown dropInput;
    public ToolButtonInteraction dropConfirmInteraction;

    private int dropDownInputInt;
    private string dropDownInputString;

    #endregion

    #region Constants
    [Header("Constants")]
    public ToolButtonInteraction constantExitInteraction;
    public ToolButtonInteraction constantSaveToDictInteraction;
    public ToolButtonInteraction constantSaveToFileInteraction;

    public ToolButtonInteraction constantInputInteraction;
    public ToolButtonInteraction constantEditInteraction;
    public ToolButtonInteraction constantDeleteInteraction;
    public ToolButtonInteraction constantDeleteCampaignInteraction;
    public ToolButtonInteraction constantFilterInteraction;
    public ToolButtonInteraction constantViewFilterInteraction;
    public ToolButtonInteraction constantViewInteraction;
    public ToolButtonInteraction constantClearInteraction;

    public Button saveToDictConstantButton;
    public Button saveToFileConstantButton;
    public Button viewFilterConstantButton;
    public Button deleteConstantButton;
    public Button deleteCampaignConstantButton;
    public Button filterConstantButton;

    public TextMeshProUGUI constantTextSmall;
    public TextMeshProUGUI constantTextLarge;
    public TextMeshProUGUI constantInstructionText;

    public TMP_InputField constantTextSmallInput;
    public TMP_InputField constantTextLargeInput;

    public TextMeshProUGUI[] arrayOfGameSummary;
    public TextMeshProUGUI[] arrayOfCampaignSummary;

    public Toggle[] arrayOfConstantScopeToggles;                    //should be 2 entries in enum.ConstantScope order
    public Toggle[] arrayOfConstantTypeToggles;                     //should be 4 entries in enum.ConstantSummaryType order
    public Toggle[] arrayOfConstantFrequencyToggles;                //should be 3 entries in enum.ConstantDistribution order

    public TextMeshProUGUI[] arrayOfConstantScopeTexts;
    public TextMeshProUGUI[] arrayOfConstantTypeTexts;
    public TextMeshProUGUI[] arrayOfConstantFrequencyTexts;

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

    // - - - Methods

    #region Initialise
    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        //canvases
        Debug.Assert(adventureCanvas != null, "Invalid adventureCanvas (Null)");
        Debug.Assert(masterCanvas != null, "Invalid masterCanvas (Null)");
        Debug.Assert(newAdventureCanvas != null, "Invalid newAdventureCanvas (Null)");
        Debug.Assert(turningPointCanvas != null, "Invalid turningPointCanvas (Null)");
        Debug.Assert(listsCanvas != null, "Invalid listsCanvas (Null)");
        Debug.Assert(dropDownCanvas != null, "Invalid dropDownCanvas (Null");
        Debug.Assert(constantsCanvas != null, "Invalid constantsCanvas (Null)");
        //main buttons
        Debug.Assert(newAdventureInteraction != null, "Invalid newAdventureInteraction (Null)");
        Debug.Assert(saveToFileInteraction != null, "Invalid saveAdventureInteraction (Null)");
        Debug.Assert(loadFromFileInteraction != null, "Invalid loadAdventureInteraction (Null)");
        Debug.Assert(editAdventureInteraction != null, "Invalid editAdventureInteraction (Null)");
        Debug.Assert(saveAdventureInteraction != null, "Invalid saveAdventureInteraction (Null)");
        Debug.Assert(removeAdventureInteraction != null, "Invalid removeAdventureInteraction (Null)");
        Debug.Assert(showListsInteraction != null, "Invalid showListInteraction (Null)");
        Debug.Assert(deleteFileInteraction != null, "Invalid deleteFileInteraction (Null)");
        Debug.Assert(clearDictionaryInteraction != null, "Invalid clearDictionaryInteraction (Null)");
        Debug.Assert(exportDictionaryInteraction != null, "Invalid exportDictionaryInteraction (Null)");
        Debug.Assert(exportOrgsInteraction != null, "Invalid exportOrgsInteraction (Null)");
        Debug.Assert(constantsInteraction != null, "Invalid constantsInteraction (Null)");
        Debug.Assert(exitInteraction != null, "Invalid ExitInteraction (Null)");
        Debug.Assert(themeMain1 != null, "Invalid theme1 (Null)");
        Debug.Assert(themeMain2 != null, "Invalid theme2 (Null)");
        Debug.Assert(themeMain3 != null, "Invalid theme3 (Null)");
        Debug.Assert(themeMain4 != null, "Invalid theme4 (Null)");
        Debug.Assert(themeMain5 != null, "Invalid theme5 (Null)");
        Debug.Assert(mainTag != null, "Invalid mainTag (Null)");
        Debug.Assert(mainNotes != null, "Invalid mainNotes (Null)");
        Debug.Assert(mainDate != null, "Invalid mainDate (Null)");
        Debug.Assert(saveFileButton != null, "Invalid saveButton (Null)");
        Debug.Assert(saveStoryButton != null, "Invalid saveStoryButton (Null)");
        Debug.Assert(editStoryButton != null, "Invalid editStoryButton (Null)");
        Debug.Assert(MainTurnPanel != null, "Invalid mainTurnPanel (Null)");
        Debug.Assert(MainSummaryPanel != null, "Invalid mainSummaryPanel (Null)");
        Debug.Assert(mainTurningPointNotes != null, "Invalid mainTurningPointNotes (Null)");
        Debug.Assert(mainInstructions != null, "Invalid mainInstructions (Null)");
        Debug.Assert(mainTurnNumber != null, "Invalid mainTurnNumber (Null)");
        Debug.Assert(mainTurnName != null, "Invalid mainTurnName (Null)");
        Debug.Assert(mainTurnNotes != null, "Invalid mainTurnNotes (Null)");
        Debug.Assert(mainNotesInput != null, "Invalid mainNotesInput (Null)");
        for (int i = 0; i < 5; i++)
        {
            if (arrayOfMainTurningPoints[i] == null) { Debug.LogErrorFormat("Invalid arrayOfMainTurningPoints[{0}] (Null)", i); }
            if (arrayOfMainPlotPoints[i] == null) { Debug.LogErrorFormat("Invalid arrayOfMainPlotPoints[{0}] (Null)", i); }
            if (arrayOfMainCharacters[i] == null) { Debug.LogErrorFormat("Invalid arrayOfMainCharacters[{0}] (Null)", i); }
        }
        for (int i = 0; i < 3; i++)
        { if (arrayOfMainToggles[i] == null) { Debug.LogErrorFormat("Invalid arrayOfToggles[{0}] (Null)"); } }
        //new adventure
        Debug.Assert(saveNewInteraction != null, "Invalid themeInteraction (Null)");
        Debug.Assert(turningPointNewInteraction != null, "Invalid turnPointInteraction (Null)");
        Debug.Assert(returnNewInteraction != null, "Invalid returnNewInteraction (Null)");
        Debug.Assert(clearNewInteraction != null, "Invalid clearNewInteraction (Null)");
        Debug.Assert(nameSetInteraction != null, "Invalid nameSetInteraction (Null)");
        Debug.Assert(newTurningButton != null, "Invalid turningPointButton (Null)");
        Debug.Assert(newSaveButton != null, "Invalid newSaveButton (Null)");
        Debug.Assert(themeNew1 != null, "Invalid theme1 (Null)");
        Debug.Assert(themeNew2 != null, "Invalid theme2 (Null)");
        Debug.Assert(themeNew3 != null, "Invalid theme3 (Null)");
        Debug.Assert(themeNew4 != null, "Invalid theme4 (Null)");
        Debug.Assert(themeNew5 != null, "Invalid theme5 (Null)");
        Debug.Assert(newNameSetHeader != null, "Invalid newNameSetHeader (Null)");
        Debug.Assert(newTag != null, "Invalid adventureTag (Null)");
        Debug.Assert(newNotes != null, "Invalid adventureNotes (Null)");
        Debug.Assert(newDate != null, "Invalid adventureDate (Null)");
        Debug.Assert(NewNormalPanel != null, "Invalid newNormalPanel (Null)");
        Debug.Assert(NewSummaryPanel != null, "Invalid newSummaryPanel (Null)");
        Debug.Assert(newTurningPointNotes != null, "Invalid newTurningPointNotes (Null)");
        for (int i = 0; i < arrayOfNewPlotLines.Length; i++)
        {
            if (arrayOfNewPlotLines[i] == null) { Debug.LogErrorFormat("Invalid arrayOfNewPlotLines[{0}] (Null)", i); }
            if (arrayOfNewCharacters[i] == null) { Debug.LogErrorFormat("Invalid arrayOfNewCharacters[{0}] (Null)", i); }
        }
        for (int i = 0; i < 5; i++)
        { if (arrayOfNewTurningPoints[i] == null) { Debug.LogErrorFormat("Invalid arrayOfNewTurningPoints[{0}] (Null)", i); } }
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
        Debug.Assert(turnNameSetHeader != null, "Invalid turnNameSetHeader (Null)");
        Debug.Assert(turnPlotPoint != null, "Invalid turnPlotPoint (Null)");
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
        Debug.Assert(turnData0Input != null, "Invalid turnData0Input (Null)");
        Debug.Assert(turnData1Input != null, "Invalid turnData1Input (Null)");
        Debug.Assert(turnData2Input != null, "Invalid turnData12nput (Null)");
        Debug.Assert(turnPlotNotesInput != null, "Invalid turnPlotNotesInput (Null)");
        Debug.Assert(turnPlotpointNotes != null, "Invalid turnPlotpointNotes (Null)");
        Debug.Assert(turnNameImage != null, "Invalid turnNameImage (Null)");
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
        //DropDown
        Debug.Assert(dropHeader != null, "Invalid dropHeader (Null)");
        Debug.Assert(dropInput != null, "Invalid dropInput (Null)");
        Debug.Assert(dropConfirmInteraction != null, "Invalid dropConfirmInteraction (Null)");
        //Constants
        Debug.Assert(constantSaveToDictInteraction != null, "Invalid constantSaveToDictInteraction (Null)");
        Debug.Assert(constantSaveToFileInteraction != null, "Invalid constantSaveToFileInteraction (Null)");
        Debug.Assert(constantInputInteraction != null, "Invalid constantInputInteraction (Null)");
        Debug.Assert(constantEditInteraction != null, "Invalid constantEditInteraction (Null)");
        Debug.Assert(constantDeleteInteraction != null, "Invalid constantDeleteInteraction (Null)");
        Debug.Assert(constantDeleteCampaignInteraction != null, "Invalid constantDeleteCampaignInteraction (Null)");
        Debug.Assert(constantViewInteraction != null, "Invalid constantViewInteraction (Null)");
        Debug.Assert(constantViewFilterInteraction != null, "Invalid constantViewFilterInteraction (Null)");
        Debug.Assert(constantClearInteraction != null, "Invalid constantClearInteraction (Null)");
        Debug.Assert(constantTextSmall != null, "Invalid constantTextSmall (Null)");
        Debug.Assert(constantTextLarge != null, "Invalid constantTextLarge (Null)");
        Debug.Assert(constantInstructionText != null, "Invalid constantInstructionText (Null)");

        for (int i = 0; i < arrayOfGameSummary.Length; i++)
        {
            if (arrayOfGameSummary[i] == null) { Debug.LogErrorFormat("Invalid arrayOfGameSummary[{0}] (Null)"); }
            if (arrayOfCampaignSummary[i] == null) { Debug.LogErrorFormat("Invalid arrayOfCampaignSummary[{0}] (Null)"); }
        }
        for (int i = 0; i < 2; i++)
        {
            if (arrayOfConstantScopeToggles[i] == null) { Debug.LogErrorFormat("Invalid arrayOfConstantScopeToggles[{0}] (Null)"); }
            if (arrayOfConstantScopeTexts[i] == null) { Debug.LogErrorFormat("Invalid arrayOfConstantScopeTexts[{0}] (Null)"); }
        }
        for (int i = 0; i < 4; i++)
        {
            if (arrayOfConstantTypeToggles[i] == null) { Debug.LogErrorFormat("Invalid arrayOfConstantTypeToggles[{0}] (Null)"); }
            if (arrayOfConstantTypeTexts[i] == null) { Debug.LogErrorFormat("Invalid arrayOfConstantTypeTexts[{0}] (Null)"); }
        }
        for (int i = 0; i < 3; i++)
        {
            if (arrayOfConstantFrequencyToggles[i] == null) { Debug.LogErrorFormat("Invalid arrayOfConstantFrequency[{0}] (Null)"); }
            if (arrayOfConstantFrequencyTexts[i] == null) { Debug.LogErrorFormat("Invalid arrayOfConstantFrequencyTexts[{0}] (Null)"); }
        }
        Debug.Assert(constantTextSmallInput != null, "Invalid constantTextSmallInput (Null)");
        Debug.Assert(constantTextLargeInput != null, "Invalid constantTextLargeInput (Null)");
        Debug.Assert(saveToDictConstantButton != null, "Invalid saveToDictButton (Null)");
        Debug.Assert(saveToFileConstantButton != null, "Invalid saveToDictButton (Null)");
        Debug.Assert(viewFilterConstantButton != null, "Invalid viewFilterButton (Null)");
        Debug.Assert(filterConstantButton != null, "Invalid filterButton (Null)");
        Debug.Assert(deleteConstantButton != null, "Invalid deleteConstantButton (Null)");
        Debug.Assert(deleteCampaignConstantButton != null, "Invalid deleteConstantButton (Null)");
        //Initialise Other
        InitialiseDelegates();
        InitialiseCanvases();
        InitialiseButtonInteractions();
        InitialiseFastAccess();
        InitialiseEvents();
    }
    #endregion

    #region InitialiseDelegates
    /// <summary>
    /// Initialise various delegates for input and dropDown fields
    /// </summary>
    private void InitialiseDelegates()
    {
        //delegate for dropDown
        dropInput.onValueChanged.AddListener(delegate { DropDownItemSelected(); });
        //delegate for New Story Name
        newTag.onValueChanged.AddListener(delegate { NewStoryNameInput(); });
        //delegates for character1 name input
        turnCharacter1Input.onSelect.AddListener(delegate { TurnCharacter1Select(); });
        turnCharacter1Input.onDeselect.AddListener(delegate { TurnCharacter1Deselect(); });
        //delegates for character2 name input
        turnCharacter2Input.onSelect.AddListener(delegate { TurnCharacter2Select(); });
        turnCharacter2Input.onDeselect.AddListener(delegate { TurnCharacter2Deselect(); });
        //delegates for data 0/1/2 input
        turnData0Input.onSelect.AddListener(delegate { TurnData0Select(); });
        turnData0Input.onDeselect.AddListener(delegate { TurnData0Select(); });
        turnData1Input.onSelect.AddListener(delegate { TurnData1Select(); });
        turnData1Input.onDeselect.AddListener(delegate { TurnData1Select(); });
        turnData2Input.onSelect.AddListener(delegate { TurnData2Select(); });
        turnData2Input.onDeselect.AddListener(delegate { TurnData2Select(); });
        //delegates for turnPlotPointNotes
        turnPlotNotesInput.onSelect.AddListener(delegate { TurnPlotNotesSelect(); });
        turnPlotNotesInput.onDeselect.AddListener(delegate { TurnPlotNotesDeselect(); });
        //delegates for constants textInputs
        constantTextSmallInput.onValueChanged.AddListener(delegate { ConstantTextInputDetected(); });
        constantTextLargeInput.onValueChanged.AddListener(delegate { ConstantTextInputDetected(); });
        //constant checkboxes -> scope
        for (int i = 0; i < arrayOfConstantScopeToggles.Length; i++)
        { arrayOfConstantScopeToggles[i].onValueChanged.AddListener(delegate { ChangeCheckBoxScopeColour(); }); }
        //constant checkboxes -> type
        for (int i = 0; i < arrayOfConstantTypeToggles.Length; i++)
        { arrayOfConstantTypeToggles[i].onValueChanged.AddListener(delegate { ChangeCheckBoxTypeColour(); }); }
        //constant checkboxes -> Frequency
        for (int i = 0; i < arrayOfConstantFrequencyToggles.Length; i++)
        { arrayOfConstantFrequencyToggles[i].onValueChanged.AddListener(delegate { ChangeCheckBoxFrequencyColour(); }); }
    }
    #endregion

    #region InitialiseCanvases
    /// <summary>
    /// Initialise all Canvases
    /// </summary>
    private void InitialiseCanvases()
    {
        adventureCanvas.gameObject.SetActive(false);
        masterCanvas.gameObject.SetActive(true);
        newAdventureCanvas.gameObject.SetActive(false);
        turningPointCanvas.gameObject.SetActive(false);
        listsCanvas.gameObject.SetActive(false);
        dropDownCanvas.gameObject.SetActive(false);
        constantsCanvas.gameObject.SetActive(false);
    }
    #endregion

    #region InitialiseButtonInteractions
    /// <summary>
    /// Button Interactions
    /// </summary>
    private void InitialiseButtonInteractions()
    {
        //main buttonInteractions
        exitInteraction.SetButton(ToolEventType.CloseAdventureUI);
        newAdventureInteraction.SetButton(ToolEventType.OpenNewAdventure);
        saveToFileInteraction.SetButton(ToolEventType.SaveToolsToFile);
        loadFromFileInteraction.SetButton(ToolEventType.LoadToolsFromFile);
        deleteFileInteraction.SetButton(ToolEventType.DeleteToolsFile);
        clearDictionaryInteraction.SetButton(ToolEventType.ClearStoryDictionary);
        exportDictionaryInteraction.SetButton(ToolEventType.ExportStoryDictionary);
        exportOrgsInteraction.SetButton(ToolEventType.ExportOrgs);
        constantsInteraction.SetButton(ToolEventType.OpenConstants);
        showListsInteraction.SetButton(ToolEventType.OpenAdventureLists);
        editAdventureInteraction.SetButton(ToolEventType.EditAdventure);
        saveAdventureInteraction.SetButton(ToolEventType.SaveAdventure);
        //new buttonInteractions
        returnNewInteraction.SetButton(ToolEventType.CloseNewAdventure);
        turningPointNewInteraction.SetButton(ToolEventType.CreateTurningPoint);
        preSaveTurnInteraction.SetButton(ToolEventType.PreSaveTurningPoint);
        saveNewInteraction.SetButton(ToolEventType.SaveAdventureToDict);
        clearNewInteraction.SetButton(ToolEventType.ClearNewAdventure);
        nameSetInteraction.SetButton(ToolEventType.NewNameSet);
        //turningPoint buttonInteractions
        plotTurnInteraction.SetButton(ToolEventType.CreatePlotpoint);
        clearTurnInteraction.SetButton(ToolEventType.ClearTurningPoint);
        saveTurnInteraction.SetButton(ToolEventType.SaveTurningPoint);
        exitTurnInteraction.SetButton(ToolEventType.CloseTurningPoint);
        //dropDown button Interactions
        dropConfirmInteraction.SetButton(ToolEventType.CloseDropDown);
        //list buttonInteractions
        returnListsInteraction.SetButton(ToolEventType.CloseAdventureLists);
        listEditInteraction.SetButton(ToolEventType.EditListItem);
        listSaveInteraction.SetButton(ToolEventType.SaveListDetails);
        for (int i = 0; i < arrayOfPlotLineInteractions.Length; i++)
        {
            arrayOfPlotLineInteractions[i].SetButton(ToolEventType.ShowPlotLineDetails, i);
            arrayOfCharacterInteractions[i].SetButton(ToolEventType.ShowCharacterDetails, i);
        }
        //constants
        constantExitInteraction.SetButton(ToolEventType.CloseConstants);
        constantInputInteraction.SetButton(ToolEventType.InputConstants);
        constantEditInteraction.SetButton(ToolEventType.EditConstants);
        constantFilterInteraction.SetButton(ToolEventType.FilterConstants);
        constantDeleteInteraction.SetButton(ToolEventType.DeleteConstants);
        constantDeleteCampaignInteraction.SetButton(ToolEventType.DeleteCampaignConstants);
        constantViewInteraction.SetButton(ToolEventType.ViewConstants);
        constantViewFilterInteraction.SetButton(ToolEventType.ViewFilterConstants);
        constantClearInteraction.SetButton(ToolEventType.ClearConstants);
        constantSaveToDictInteraction.SetButton(ToolEventType.SaveToDictConstants);
        constantSaveToFileInteraction.SetButton(ToolEventType.SaveToFileConstants);
    }
    #endregion

    #region InitaliseEventListeners
    /// <summary>
    /// Initialise all event listeners
    /// </summary>
    private void InitialiseEvents()
    {
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
        ToolEvents.i.AddListener(ToolEventType.ClearStoryDictionary, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ExportStoryDictionary, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ExportOrgs, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.OpenConstants, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ClearNewAdventure, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.EditAdventure, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.SaveAdventure, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.NewNameSet, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.NextAdventure, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.PreviousAdventure, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.MainSummaryUpArrow, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.MainSummaryDownArrow, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.OpenMainDetails, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.CloseMainDetails, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.MainDetailsUpArrow, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.MainDetailsDownArrow, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.MainDetailsRightArrow, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.MainDetailsLeftArrow, OnEvent, "AdventureUI");

        ToolEvents.i.AddListener(ToolEventType.OpenAdventureLists, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.CloseAdventureLists, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ShowPlotLineDetails, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ShowCharacterDetails, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.NextLists, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.PreviousLists, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.EditListItem, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.SaveListDetails, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.NewSummaryUpArrow, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.NewSummaryDownArrow, OnEvent, "AdventureUI");

        ToolEvents.i.AddListener(ToolEventType.CreatePlotpoint, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ClearTurningPoint, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.PreSaveTurningPoint, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.SaveTurningPoint, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.CloseTurningPoint, OnEvent, "AdventureUI");

        ToolEvents.i.AddListener(ToolEventType.CloseDropDown, OnEvent, "AdventureUI");

        ToolEvents.i.AddListener(ToolEventType.CloseConstants, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.InputConstants, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.EditConstants, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ViewConstants, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.FilterConstants, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ViewFilterConstants, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.ClearConstants, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.NextConstant, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.PreviousConstant, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.SaveToDictConstants, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.SaveToFileConstants, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.DeleteConstants, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.DeleteCampaignConstants, OnEvent, "AdventureUI");
    }
    #endregion

    #region InitialiseFastAccess
    /// <summary>
    /// Initialise all fast access
    /// </summary>
    private void InitialiseFastAccess()
    {
        //TurningPoint
        plotPointNone = ToolManager.i.toolDataScript.GetPlotpoint("None");
        Debug.Assert(plotPointNone != null, "Invalid plotPointNone (Null)");
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
            case ToolEventType.ClearStoryDictionary:
                ClearDictionary();
                break;
            case ToolEventType.ExportStoryDictionary:
                ExportDictionary();
                break;
            case ToolEventType.ExportOrgs:
                ExportOrgs();
                break;
            case ToolEventType.OpenConstants:
                OpenConstants();
                break;
            case ToolEventType.ClearNewAdventure:
                ClearNewAdventure();
                break;
            case ToolEventType.EditAdventure:
                EditAdventure();
                break;
            case ToolEventType.SaveAdventure:
                SaveAdventure();
                break;
            case ToolEventType.NewNameSet:
                GetNewNameSet();
                break;
            case ToolEventType.NextAdventure:
                NextAdventure();
                break;
            case ToolEventType.PreviousAdventure:
                PreviousAdventure();
                break;
            case ToolEventType.MainSummaryUpArrow:
                MainSummaryUpArrow();
                break;
            case ToolEventType.MainSummaryDownArrow:
                MainSummaryDownArrow();
                break;
            case ToolEventType.OpenMainDetails:
                OpenMainDetails();
                break;
            case ToolEventType.CloseMainDetails:
                CloseMainDetails();
                break;
            case ToolEventType.MainDetailsUpArrow:
                MainDetailsUpArrow();
                break;
            case ToolEventType.MainDetailsDownArrow:
                MainDetailsDownArrow();
                break;
            case ToolEventType.MainDetailsRightArrow:
                MainDetailsRightArrow();
                break;
            case ToolEventType.MainDetailsLeftArrow:
                MainDetailsLeftArrow();
                break;
            case ToolEventType.NewSummaryUpArrow:
                NewSummaryUpArrow();
                break;
            case ToolEventType.NewSummaryDownArrow:
                NewSummaryDownArrow();
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
            case ToolEventType.CloseDropDown:
                CloseDropDownInput();
                break;
            case ToolEventType.CloseConstants:
                CloseConstants();
                break;
            case ToolEventType.InputConstants:
                InputConstants();
                break;
            case ToolEventType.ViewConstants:
                ViewConstants();
                break;
            case ToolEventType.EditConstants:
                EditConstants();
                break;
            case ToolEventType.FilterConstants:
                FilterConstants();
                break;
            case ToolEventType.DeleteConstants:
                DeleteConstants();
                break;
            case ToolEventType.DeleteCampaignConstants:
                DeleteCampaignConstants();
                break;
            case ToolEventType.ViewFilterConstants:
                ViewFilterConstants();
                break;
            case ToolEventType.ClearConstants:
                ClearConstants();
                break;
            case ToolEventType.SaveToDictConstants:
                SaveToDictConstants();
                break;
            case ToolEventType.SaveToFileConstants:
                SaveToFileConstants();
                break;
            case ToolEventType.NextConstant:
                NextConstant();
                break;
            case ToolEventType.PreviousConstant:
                PreviousConstant();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion

    // - - - Modules

    #region Main
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
        //panels
        MainTurnPanel.gameObject.SetActive(false);    //redudant at present
        MainSummaryPanel.gameObject.SetActive(true);
        //load data from dictionary automatically
        LoadAdventures();
        LoadConstants();
        //Navigation
        UpdateListOfStories();
        //load up first story
        DisplayStoryNotes(false);
        DisplayStoryMain();
        //disable Save buttons
        saveFileButton.gameObject.SetActive(false);
        saveStoryButton.gameObject.SetActive(false);
        editStoryButton.gameObject.SetActive(true);
        //up and down arrow index
        mainSummaryIndex = 0;
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Main);
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Read);
        //call after setting modalType
        SetMainInstructions();
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
        saveFileButton.gameObject.SetActive(false);
        //update stories
        UpdateListOfStories();
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
    /// subMethod to handle loading constants
    /// </summary>
    private void LoadConstants()
    {
        //read data from file
        if (ToolManager.i.toolFileScript.ReadConstantsFromFile() == true)
        {
            //load data into game
            ToolManager.i.toolFileScript.ReadConstantsData();
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
    /// Export dictOfStories into a text file dataDump suitable for the Keep (write only, no ability to read this file back in)
    /// </summary>
    private void ExportDictionary()
    {
        ToolManager.i.toolFileScript.ExportToolData();
        ToolManager.i.toolFileScript.SaveExportToFile();
    }

    /// <summary>
    /// Export 10 proc-generated org specs to a file
    /// </summary>
    private void ExportOrgs()
    {
        ToolManager.i.toolFileScript.ExportOrgData();
        ToolManager.i.toolFileScript.SaveOrgExportToFile();
    }

    /// <summary>
    /// Open constants page (campaign and game scope constant plotlines and characters)
    /// </summary>
    private void OpenConstants()
    {
        masterCanvas.gameObject.SetActive(false);
        constantsCanvas.gameObject.SetActive(true);
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Constants);
        //Initialise page
        InitialiseConstants();
    }

    /// <summary>
    /// Edit story Notes
    /// </summary>
    private void EditAdventure()
    {
        //change modalType to disable arrow keys, etc.
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input);
        DisplayStoryNotes(true);
        //toggle buttons
        editStoryButton.gameObject.SetActive(false);
        saveStoryButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Save recently edited story Notes
    /// </summary>
    private void SaveAdventure()
    {
        DisplayStoryNotes(false);
        //toggle buttons
        editStoryButton.gameObject.SetActive(true);
        saveStoryButton.gameObject.SetActive(false);
        //copy input
        storyMain.notes = mainNotesInput.text;
        //update onscreen notes
        mainNotes.text = storyMain.notes;
        //restore normal modal type
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Read);
        //add story to dict, overwrite existing data if already present
        ToolManager.i.toolDataScript.AddStory(storyMain);
        Debug.LogFormat("[Tol] AdventureUI.cs -> SaveAdventure: StoryMain \"{0}\" saved to dictionary{1}", storyMain.refTag, "\n");
        //toggle main save to file button
        saveFileButton.gameObject.SetActive(true);
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

    /// <summary>
    /// Up arrow moves one up in turningPoint summary display
    /// </summary>
    private void MainSummaryUpArrow()
    {
        //roll over
        if (mainSummaryIndex == 0)
        { mainSummaryIndex = 4; }
        else { mainSummaryIndex--; }
        DisplayMainSummary(mainSummaryIndex);
    }

    /// <summary>
    /// down arrow moves one down in turningPoint summary display
    /// </summary>
    private void MainSummaryDownArrow()
    {
        //roll over
        if (mainSummaryIndex == 4)
        { mainSummaryIndex = 0; }
        else { mainSummaryIndex++; }
        DisplayMainSummary(mainSummaryIndex);
    }

    /// <summary>
    /// Up arrow moves one up one plotline in turningPoint details display
    /// </summary>
    private void MainDetailsUpArrow()
    {
        //roll over
        if (mainDetailsIndex == 0)
        { mainDetailsIndex = 4; }
        else { mainDetailsIndex--; }
        DisplayMainDetails(mainSummaryIndex, mainDetailsIndex);
    }

    /// <summary>
    /// down arrow moves one down one plotline in turningPoint details display
    /// </summary>
    private void MainDetailsDownArrow()
    {
        //roll over
        if (mainDetailsIndex == 4)
        { mainDetailsIndex = 0; }
        else { mainDetailsIndex++; }
        DisplayMainDetails(mainSummaryIndex, mainDetailsIndex);
    }

    /// <summary>
    /// right arrow switches to next turningPoint in StoryMain
    /// </summary>
    private void MainDetailsRightArrow()
    {
        //roll over
        if (mainSummaryIndex == 4)
        { mainSummaryIndex = 0; }
        else { mainSummaryIndex++; }
        mainDetailsIndex = 0;
        DisplayMainDetails(mainSummaryIndex, mainDetailsIndex, true);
    }

    /// <summary>
    /// left arrow switches to previous turningPoint in StoryMain
    /// </summary>
    private void MainDetailsLeftArrow()
    {
        //roll over
        if (mainSummaryIndex == 0)
        { mainSummaryIndex = 4; }
        else { mainSummaryIndex--; }
        mainDetailsIndex = 0;
        DisplayMainDetails(mainSummaryIndex, mainDetailsIndex, true);
    }

    /// <summary>
    /// View details of a particular TurningPoint
    /// </summary>
    private void OpenMainDetails()
    {
        //toggle panels
        MainSummaryPanel.gameObject.SetActive(false);
        MainTurnPanel.gameObject.SetActive(true);
        //change ModalType
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Details);
        //index
        mainDetailsIndex = 0;
        //call after setting modalType
        SetMainInstructions();
        //populate page
        DisplayMainDetails(mainSummaryIndex, mainDetailsIndex, true);
    }

    /// <summary>
    /// close TurningPoint details page and go back to summary
    /// </summary>
    private void CloseMainDetails()
    {
        //toggle panels
        MainTurnPanel.gameObject.SetActive(false);
        MainSummaryPanel.gameObject.SetActive(true);
        //change ModalType
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Read);
        //call after setting modalType
        SetMainInstructions();
    }

    #endregion

    #region New Adventure

    //
    // - - - New Adventure
    //

    /// <summary>
    /// open new adventure page (used only from Main page, NOT from TurningPoint page as it'd reset stuff that shouldn't be reset)
    /// </summary>
    private void OpenNewAdventure()
    {
        //create new story if none present
        if (storyNew == null)
        { storyNew = new Story(); }
        else { storyNew.Reset(); }
        //storyNew nameset
        storyNew.nameSet = ToolManager.i.adventureScript.GetNameSetInUse();
        //toggle canvases on/off
        newAdventureCanvas.gameObject.SetActive(true);
        masterCanvas.gameObject.SetActive(false);
        //zero turningPointIndex (it's a new adventure)
        turningPointIndex = 0;
        newSummaryIndex = 0;
        //redraw page
        RedrawNewAdventurePage();
        //toggle buttons
        newTurningButton.gameObject.SetActive(false);
        newSaveButton.gameObject.SetActive(false);
        //set flag
        isNewAdventureSave = true;
        //theme
        CreateTheme();
        //auto fill date
        newDate.text = GetCurrentDateString();
        storyNew.date = GetCurrentDateString();
        //nameSet currently in use
        newNameSetHeader.text = ToolManager.i.adventureScript.GetNameSetInUse();
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
        { saveFileButton.gameObject.SetActive(true); }
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Main);
        //copy storyNew across to storyMain
        storyMain = new Story(storyNew);
    }

    /// <summary>
    /// Delegate assigned to newTag (new story Name) that triggers on change to active save button
    /// </summary>
    private void NewStoryNameInput()
    { newSaveButton.gameObject.SetActive(true); }

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
        /*newSaveButton.gameObject.SetActive(true);*/
    }

    #region SaveAdventureToDict
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
            string refTagNew;
            //Only new adventure details to save
            if (isNewAdventureSave == true)
            {
                //arrays
                for (int i = 0; i < arrayOfNewPlotLines.Length; i++)
                {
                    //valid name
                    if (string.IsNullOrEmpty(arrayOfNewPlotLines[i].text) == false)
                    {
                        refTagNew = arrayOfNewPlotLines[i].text.Replace(" ", "");
                        //plotlines
                        storyNew.arrays.arrayOfPlotLines[i].tag = refTagNew;
                        storyNew.arrays.arrayOfPlotLines[i].status = StoryStatus.Data;
                        counterPlotLine++;
                        //add to list
                        PlotLine plotLine = new PlotLine() { tag = arrayOfNewPlotLines[i].text, refTag = refTagNew, listOfNotes = new List<string>() { "Seed PlotLine" } };
                        AddPlotLine(plotLine);
                    }
                    //valid name
                    if (string.IsNullOrEmpty(arrayOfNewCharacters[i].text) == false)
                    {
                        refTagNew = arrayOfNewCharacters[i].text.Replace(" ", "");
                        //characters
                        storyNew.arrays.arrayOfCharacters[i].tag = refTagNew;
                        storyNew.arrays.arrayOfCharacters[i].status = StoryStatus.Data;
                        counterCharacter++;
                        //add to list
                        Character character = new Character() { tag = arrayOfNewCharacters[i].text, refTag = refTagNew, dataCreated = "Seed Character" };
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
                //number of turningPoints
                storyNew.numTurningPoints = storyNew.arrayOfTurningPoints.Where(x => x.type != TurningPointType.None).Count();
                if (storyNew.numTurningPoints == 5)
                { storyNew.isConcluded = true; }
                //add story to dict, overwrite existing data if already present
                ToolManager.i.toolDataScript.AddStory(storyNew);
                Debug.LogFormat("[Tol] AdventureUI.cs -> SaveAdventure: StoryNew \"{0}\" saved to dictionary{1}", storyNew.refTag, "\n");
                //trigger main save to file button
                isSaveNeeded = true;
                //update navigation
                UpdateListOfStories();
                //switch on turningPoint button
                newTurningButton.gameObject.SetActive(true);
                //switch off save button
                newSaveButton.gameObject.SetActive(false);
                //open turningPoint page if story not yet concluded
                if (storyNew.isConcluded == false)
                { NewTurningPoint(); }
            }
            else { Debug.LogWarning("Invalid storyNew.tag (Null or Empty). Story not saved to Dictionary"); }
        }
        else { Debug.LogError("Invalid storyNew (Null)"); }
    }
    #endregion

    /// <summary>
    /// Up arrow moves one up in turningPoint summary display
    /// </summary>
    private void NewSummaryUpArrow()
    {
        //roll over
        if (newSummaryIndex == 0)
        { newSummaryIndex = 4; }
        else { newSummaryIndex--; }
        DisplayNewSummary(newSummaryIndex);
    }

    /// <summary>
    /// down arrow moves one down in turningPoint summary display
    /// </summary>
    private void NewSummaryDownArrow()
    {
        //roll over
        if (newSummaryIndex == 4)
        { newSummaryIndex = 0; }
        else { newSummaryIndex++; }
        DisplayNewSummary(newSummaryIndex);
    }

    /// <summary>
    /// Choose a nameset from a drop down list to be used for New Adventures
    /// </summary>
    private void GetNewNameSet()
    {
        coroutineNameSet = GetDropDownNameSet();
        StartCoroutine(coroutineNameSet);
    }

    /// <summary>
    /// handles drop down input for NameSets
    /// </summary>
    /// <returns></returns>
    IEnumerator GetDropDownNameSet()
    {
        isWaitUntilDone = false;
        //initiliase drop down list
        NameSet[] arrayOfNameSets = ToolManager.i.adventureScript.GetArrayOfNameSets();
        if (arrayOfNameSets != null)
        {
            int count = arrayOfNameSets.Length;
            if (count > 0)
            {
                //get a list of nameSet names
                List<string> listOfNameSets = new List<string>();
                for (int i = 0; i < count; i++)
                { listOfNameSets.Add(arrayOfNameSets[i].name); }
                //initialise dropDown
                InitialiseDropDownInput(listOfNameSets, "Select a NameSet to use");
                coroutineDropDown = WaitForDropDownNameSetInput();
                StartCoroutine(coroutineDropDown);
            }
            else { Debug.LogError("Invalid arrayOfNameSets (Empty)"); }
        }
        else { Debug.LogError("Invalid arrayOfNameSets (Null)"); }
        yield return new WaitUntil(() => isWaitUntilDone == true);
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
        Debug.LogFormat("[Tst] AdventureUI.cs -> NewTurningPoint: {0} - - - - - - - - - - - - - - {1}", turningPointIndex + 1, "\n");
        //toggle canvases
        turningPointCanvas.gameObject.SetActive(true);
        newAdventureCanvas.gameObject.SetActive(false);
        //reset save flag
        isTurningPointSaved = false;
        //Adventure 
        turnAdventureName.text = storyNew.tag;
        //nameSet currently in use
        turnNameSetHeader.text = ToolManager.i.adventureScript.GetNameSetInUse();
        //indexes
        plotPointIndex = 0;
        //find next empty slot in arrayOfTurningPoints 
        int index = -1;
        for (int i = 0; i < storyNew.arrayOfTurningPoints.Length; i++)
        {
            if (storyNew.arrayOfTurningPoints[i].type == TurningPointType.None)
            {
                index = i;
                break;
            }
        }
        if (turningPointIndex > -1)
        {
            // TurningPoint
            turningPointIndex = index;

            //start of Story -> new Turning point and Plotline
            if (turningPointIndex == 0)
            {
                turningPoint = new TurningPoint() { type = TurningPointType.New };
                plotLine = new PlotLine() { };
            }
        }
        else
        {
            Debug.LogWarning("Story has no blank Turning Points available, storNew.isConcluded set TRUE");
            storyNew.isConcluded = true;
            //disable plotLine button
            turnPlotpointButton.gameObject.SetActive(false);
        }
        //redraw page
        RedrawTurningPointPage();
        //set modal state
        ToolManager.i.toolInputScript.SetModalState(ToolModal.TurningPoint);
        //debug
        storyNew.lists.DebugShowCharacterList();
        storyNew.lists.DebugShowPlotLineList();
        //ready for New Plotpoint (spacebar input)
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input);
    }
    #endregion

    #region NewPlotpoint...

    #region NewPlotpoint
    /// <summary>
    /// New Plotpoint
    /// </summary>
    private void NewPlotpoint()
    {
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Process);
        // - - - Pre Admin
        turnData1.text = "";
        turnData2.text = "";
        isPlotpointAdminDone = false;
        //need to save previous data before generating new
        if (plotPointIndex > 0)
        {
            //update characters
            if (character1 != null)
            {
                if (string.IsNullOrEmpty(turnData1Input.text) == false)
                { character1.listOfNotes.Add(turnData1Input.text); }
            }
            if (character2 != null)
            {
                if (string.IsNullOrEmpty(turnData2Input.text) == false)
                { character2.listOfNotes.Add(turnData2Input.text); }
            }
            //save if flag true (normal). Would be false if ClearPlotPoint has run in the meantime
            if (isSavePlotPoint == true)
            {
                //Create and Save PlotDetails
                PlotDetails details = new PlotDetails()
                {
                    isActive = false,
                    plotPoint = plotPoint.refTag,
                    notes = turnPlotNotesInput.text,
                    character1 = character1,
                    character2 = character2,
                    type = plotPoint.type
                };
                turningPoint.arrayOfDetails[plotPointIndex - 1] = details;
            }
            Debug.LogFormat("[Tst] AdventureUI.cs -> NewPlotPoint: \"{0}\" {1} / {2} SAVED to arrayOfDetails{3}", plotPoint.tag,
                character1 != null ? character1.tag : "Nobody", character2 != null ? character2.tag : "Nobody", "\n");
        }
        //reset global characters
        character1 = null;
        character2 = null;
        //set flags
        isSavePlotPoint = true;
        numberOfClears = 0;
        //
        // - - - New PlotPoint
        //
        if (plotPointIndex < 5)
        {
            //zero out relevant fields
            ResetPlotPoint();
            //save previous notes (except for first instance as there are no notes to save)
            if (turningPointIndex > 0)
            { arrayOfPlotpointNotes[turningPointIndex - 1] = turnPlotNotesInput.text; }

            //Generate new Plotpoint
            plotPoint = GetPlotPoint();
            //Plotpoint handler
            switch (plotPoint.type)
            {
                case PlotPointType.Normal:
                    GetCharacters(plotPoint.numberOfCharacters);
                    break;
                case PlotPointType.NewCharacter:
                    Debug.LogFormat("[Tst] AdventureUI.cs -> NewPlotpoint: NewCharacter to be Generated{0}", "\n");
                    character1 = GetNewCharacter(plotPoint.special);
                    break;
                case PlotPointType.RemoveCharacter:
                    if (storyNew.lists.CheckIfAnyCharactersOnList() == true)
                    {
                        Character character = storyNew.lists.GetRandomCharacterFromList();
                        if (character != null)
                        {
                            storyNew.lists.RemoveCharacterFromList(character);
                            storyNew.arrays.RemoveCharacterFromArray(character.refTag);
                            Debug.LogFormat("[Tst] AdventureUI.cs -> NewPlotpoint: \"{0}\" REMOVED{1}", character.tag, "\n");
                        }
                        else
                        { Debug.LogFormat("[Tst] AdventureUI.cs -> RemoveCharacter: Not possible (Null character){0}", "\n"); }
                    }
                    else
                    { Debug.LogFormat("[Tst] AdventureUI.cs -> RemoveCharacter: Not possible (No characters present){0}", "\n"); }
                    break;
                case PlotPointType.Meta:
                    GetMetaPlotPoint();
                    break;
                case PlotPointType.Conclusion:
                case PlotPointType.None:
                    //do nothing -> already handled by GetPlotPoint
                    break;
                default: Debug.LogWarningFormat("Unrecognised plotPoint.type \"{0}\"", plotPoint.type); break;
            }
            //update texts
            turnPlotPoint.text = plotPoint.tag;
            turnData0.text = plotPoint.details;
            arrayOfTurnPlotpoints[plotPointIndex].text = plotPoint.tag;
            //non character related plotpoints
            if (isPlotpointAdminDone == false)
            {
                UpdateCharacterData();
                //toggle fields
                ToggleTurningPointFields(true, plotPoint.numberOfCharacters);
                //increment index
                plotPointIndex++;
                //End of turning point
                if (plotPointIndex >= 5)
                {
                    //toggle save buttons
                    turnPreSaveButton.gameObject.SetActive(true);
                    turnSaveButton.gameObject.SetActive(false);
                }
            }
            //ready for new Plotpoint (spacebar input)
            ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input);
        }
        else { Debug.LogWarning("There are already five plotponts -> Info only"); }
    }
    #endregion

    #region GetPlotPoint
    /// <summary>
    /// returns a new plotPoint. Handles all None/Conclusion cases
    /// </summary>
    /// <returns></returns>
    private Plotpoint GetPlotPoint()
    {
        Plotpoint plotPoint = null;
        //chance of a constant plotpoint 
        if (UnityEngine.Random.Range(0, 100) < ToolManager.i.adventureScript.chanceOfConstantPlotpoint)
        {
            //will return null if none found
            ConstantPlotpoint constantplotPoint = ToolManager.i.toolDataScript.GetRandomConstantPlotpoint();
            if (constantplotPoint != null)
            {
                //convert to a normal plotPoint
                plotPoint = new Plotpoint() { tag = string.Format("[Con] {0}", constantplotPoint.tag), refTag = constantplotPoint.refTag, details = constantplotPoint.details };
            }
        }
        if (plotPoint == null)
        {
            //Generate plotpoint normally -> failed constant roll or no suitable constant plotPoint available
            int numNone, numConcluded;
            int priority = ToolManager.i.adventureScript.GetThemePriority();
            ThemeType themeType = storyNew.theme.GetThemeType(priority);
            plotPoint = new Plotpoint(ToolManager.i.toolDataScript.GetRandomPlotpoint(themeType));
            //get numbers already present
            numNone = turningPoint.CheckNumberOfPlotPointType(PlotPointType.None);
            numConcluded = turningPoint.CheckNumberOfPlotPointType(PlotPointType.Conclusion);
            //special cases
            switch (plotPoint.type)
            {
                case PlotPointType.RemoveCharacter:
                    //check if there is a character to remove
                    if (storyNew.lists.CheckIfAnyCharactersOnList() == false)
                    {
                        //no characters to remove, try again (avoiding another RemoveCharacter, None, or Conclusion)
                        do
                        { GetReplacementPlotPoint(themeType); }
                        while (plotPoint.type != PlotPointType.RemoveCharacter);
                    }
                    else
                    {
                        //remove a random character
                        Character character = storyNew.lists.GetRandomCharacterFromList();
                        if (character != null)
                        {
                            storyNew.lists.RemoveCharacterFromList(character);
                            storyNew.arrays.RemoveCharacterFromArray(character.refTag);
                            Debug.LogFormat("[Tst] AdventureUI.cs -> GetPlotPoint: \"{0}\" removed{1}", character.tag, "\n");
                        }
                    }
                    break;
                case PlotPointType.Conclusion:
                    if (numConcluded > 0)
                    {
                        //can only have one Conclusion. Change any more into 'None'
                        if (numNone < 2)
                        {
                            plotPoint = new Plotpoint(plotPointNone);
                            Debug.LogFormat("[Tst] AdventureUI.cs -> GetPlotPoint: Conclusion plotPoint CHANGED to None (max 1 allowed in TurningPoint){0}", "\n");
                        }
                        else
                        {
                            //max 2 none and 1 conclusion allowed -> need a new plotpoint
                            GetReplacementPlotPoint(themeType);
                            Debug.LogFormat("[Tst] AdventureUI.cs -> GetPlotPoint: Conclusion plotPoint CHANGED to \"{0}\" (numConcluded {1}, numNone {2}){3}", plotPoint.tag, numConcluded, numNone, "\n");
                        }
                    }
                    else if (turningPoint.type == TurningPointType.New)
                    {
                        //can't have any conclusion plotpoints in a new Turning point
                        if (numNone >= 3)
                        {
                            //maxCap reached already on 'None'
                            GetReplacementPlotPoint(themeType);
                            Debug.LogFormat("[Tst] AdventureUI.cs -> GetPlotPoint: Conclusion plotPoint CHANGED (new TurningPoint) to \"{0}\" (numConcluded {1}, numNone {2}){3}", plotPoint.tag, numConcluded, numNone, "\n");
                        }
                        else
                        {
                            //change into 'None'
                            plotPoint = new Plotpoint(plotPointNone);
                            Debug.LogFormat("[Tst] AdventureUI.cs -> GetPlotPoint: Conclusion plotPoint CHANGED to None (new TurningPoint){0}", "\n");
                        }
                    }
                    break;
                case PlotPointType.None:
                    //max of 3 none in a plotline OR 2 none plus 1 conclusion
                    if (numNone < 3)
                    {
                        if (numNone == 2 && numConcluded == 1)
                        {
                            GetReplacementPlotPoint(themeType);
                            Debug.LogFormat("[Tst] AdventureUI.cs -> GetPlotPoint: None plotPoint CHANGED to \"{0}\" (numConcluded {1}, numNone {2}){3}", plotPoint.tag, numConcluded, numNone, "\n");
                        }
                    }
                    else
                    {
                        //max 3 none allowed
                        if (numNone == 3)
                        {
                            GetReplacementPlotPoint(themeType);
                            Debug.LogFormat("[Tst] AdventureUI.cs -> GetPlotPoint: None plotPoint CHANGED to \"{0}\" (numConcluded {1}, numNone {2}){3}", plotPoint.tag, numConcluded, numNone, "\n");
                        }
                    }
                    break;
            }
        }
        Debug.LogFormat("[Tst] AdventureUI.cs -> NewPlotPoint: NEW Plotpoint \"{0}\", plotPointIndex {1}{2}", plotPoint.tag, plotPointIndex, "\n");
        return plotPoint;
    }
    #endregion

    #region GetReplacementPlotPoint
    /// <summary>
    /// subMethod that generates a replacement plotPoint for GetPlotPoint when a 'None' or 'Conclusion' needs to be swapped out
    /// </summary>
    private void GetReplacementPlotPoint(ThemeType themeType)
    {
        bool isDone = false;
        do
        {
            plotPoint = new Plotpoint(ToolManager.i.toolDataScript.GetRandomPlotpoint(themeType));
            if (plotPoint.type != PlotPointType.Conclusion && plotPoint.type != PlotPointType.None)
            { isDone = true; }
        }
        while (isDone == false);
    }
    #endregion

    #region GetMetaPlotPoint
    /// <summary>
    /// Handles all Meta plotPoint matters
    /// </summary>
    private void GetMetaPlotPoint()
    {
        MetaPlotpoint metaPlotpoint = ToolManager.i.toolDataScript.GetRandomMetaPlotpoint();
        if (metaPlotpoint != null)
        {
            Debug.LogFormat("[Tst] AdventureUI.cs -> GetMetaPlotPoint: META \"{0}\", {1}{2}", metaPlotpoint.tag, metaPlotpoint.action, "\n");
            //pass over data to plotPoint (it's a new instance so it won't affect the original with the core data)
            plotPoint.refTag = metaPlotpoint.refTag;
            plotPoint.numberOfCharacters = 0;
            Character character = null;
            //resolve action
            switch (metaPlotpoint.action)
            {
                case MetaAction.CharacterExits:
                    //remove a random character
                    character = storyNew.lists.GetRandomCharacterFromList();
                    if (character != null)
                    {
                        storyNew.lists.RemoveCharacterFromList(character);
                        storyNew.arrays.RemoveCharacterFromArray(character.refTag);
                        plotPoint.tag = string.Format("[Meta] {0} Exits", character.tag);
                        plotPoint.details = "A character exits the Story";
                    }
                    else
                    {
                        //there are no characters in list -> default plotPoint (one I made up)
                        plotPoint.tag = "There's Something Out There";
                        plotPoint.details = "An Unknown character is in the background, circling around, waiting for an opportunity to act but mysteriously disappears before doing so";
                    }
                    break;
                case MetaAction.CharacterReturns:
                    //previously removed/exited character returns to adventure
                    character = storyNew.lists.GetRandomRemovedCharacter();
                    if (character != null)
                    {
                        //add character back into story
                        storyNew.lists.AddCharacterToList(character);
                        storyNew.arrays.AddCharacterToArray(new ListItem() { tag = character.refTag, status = StoryStatus.Data });
                        plotPoint.tag = string.Format("[Meta] {0} returns", character.tag);
                        plotPoint.details = "A character, who previously left, has returned to the story";
                    }
                    else
                    {
                        //there are no characters on Removed list -> default plotPoint (one I made up)
                        plotPoint.tag = "Something unusual happens";
                        plotPoint.details = "A twist in the tale, something out of Left Field";
                    }
                    break;
                case MetaAction.CharacterUpgrade:
                    //character gains 2 extra slots, if available, ignoring maxCap
                    character = storyNew.lists.GetRandomCharacterFromList();
                    if (character != null)
                    {
                        storyNew.arrays.UpgradeCharacter(new ListItem() { tag = character.tag, status = StoryStatus.Data }, 2);
                        plotPoint.tag = string.Format("[Meta] {0} Upgraded", character.tag);
                        plotPoint.details = "A character has been upgraded (up to two extra slots in array ignoring the maxCap) in importance";
                    }
                    else
                    {
                        //no character available -> default plotPoint (one I made up)
                        plotPoint.tag = "Something unusual happens";
                        plotPoint.details = "A twist in the tale, something out of Left Field";
                    }
                    break;
                case MetaAction.CharacterDowngrade:
                    //character loses 2 slots in arrayOfcharacter, and if doing so, leaves array, is removed from list and adventure
                    character = storyNew.lists.GetRandomCharacterFromList();
                    if (character != null)
                    {
                        storyNew.arrays.DowngradeCharacter(character.refTag, 2);
                        if (storyNew.arrays.CheckCharacterInArray(character.refTag) == 0)
                        { storyNew.lists.RemoveCharacterFromList(character); }
                        plotPoint.tag = string.Format("[Meta] {0} Downgraded", character.tag);
                        plotPoint.details = "A character has been downgraded in importance (loses up to 2 slots in array, removed from adventure if no more slots remaining)";
                    }
                    else
                    {
                        //no character available -> default plotPoint (one I made up)
                        plotPoint.tag = "Something unusual happens";
                        plotPoint.details = "A twist in the tale, something out of Left Field";
                    }
                    break;
                case MetaAction.CharacterStepsUp:
                    //character gains 1 extra slot, if available, ignoring maxCap
                    character = storyNew.lists.GetRandomCharacterFromList();
                    if (character != null)
                    {
                        storyNew.arrays.UpgradeCharacter(new ListItem() { tag = character.tag, status = StoryStatus.Data }, 1);
                        plotPoint.tag = string.Format("[Meta] {0} Steps Up", character.tag);
                        plotPoint.details = "A character Steps Up (gain an extra slots in array ignoring the maxCap) and becomes more important";
                    }
                    else
                    {
                        //no character available -> default plotPoint (one I made up)
                        plotPoint.tag = "Something unusual happens";
                        plotPoint.details = "A twist in the tale, something out of Left Field";
                    }
                    break;
                case MetaAction.CharacterStepsDown:
                    //character loses a slot in arrayOfcharacter, and if doing so, leaves array, is removed from list and adventure
                    character = storyNew.lists.GetRandomCharacterFromList();
                    if (character != null)
                    {
                        storyNew.arrays.DowngradeCharacter(character.refTag, 1);
                        if (storyNew.arrays.CheckCharacterInArray(character.refTag) == 0)
                        { storyNew.lists.RemoveCharacterFromList(character); }
                        plotPoint.tag = string.Format("[Meta] {0} Steps Down", character.tag);
                        plotPoint.details = "A character Steps Down in importance (loses up a slot in array, removed from adventure if no more slots remaining)";
                    }
                    else
                    {
                        //no character available -> default plotPoint (one I made up)
                        plotPoint.tag = "Something unusual happens";
                        plotPoint.details = "A twist in the tale, something out of Left Field";
                    }
                    break;
                case MetaAction.PlotLineCombo:
                    //Combines with another plotline
                    PlotLine plotLineOther = storyNew.lists.GetRandomPlotLine(plotLine.refTag);
                    if (plotLineOther != null)
                    {
                        plotPoint.tag = string.Format("[PlotLine] {0}", plotLineOther.tag);
                        plotPoint.details = "The turning point intertwines with an existing PlotLine in some manner";
                    }
                    else
                    {
                        //no plotLine available -> default plotPoint (one I made up)
                        plotPoint.tag = "Something unusual happens";
                        plotPoint.details = "A twist in the tale, something out of Left Field";
                    }
                    break;
                default: Debug.LogWarningFormat("Unrecognised metaPlotpoint.action \"{0}\"", metaPlotpoint.action); break;
            }
        }
        else { Debug.LogError("Invalid metaPlotpoint (Null)"); }
    }
    #endregion

    #endregion

    #region Character Methods...

    #region GetCharacters
    /// <summary>
    /// Get characters for the plotline (0 to 3)
    /// </summary>
    /// <param name="numOfCharacters"></param>
    private void GetCharacters(int numOfCharacters)
    {

        //reset flags prior to execution
        isChar1MostLogical = false;
        isChar2MostLogical = false;
        //how many
        switch (numOfCharacters)
        {
            case 0:
                //do nothing
                break;
            case 1:
                character1 = GetIndividualCharacter(true);
                break;
            case 2:
                //both characters will be null, you're only setting flags here for later
                character1 = GetIndividualCharacter(true);
                character2 = GetIndividualCharacter(false);
                break;
            default: Debug.LogWarningFormat("Unrecognised numOfCharacters \"{0}\"", numOfCharacters); break;
        }
        //coroutine to handle display and most logical characters (if needed)
        coroutineCharacter = GetMostLogicalCharacters();
        StartCoroutine(coroutineCharacter);
    }
    #endregion

    #region coroutine GetMostLogicalCharacter
    /// <summary>
    /// coroutine for getting Most Logical Character from drop down lists
    /// </summary>
    /// <returns></returns>
    IEnumerator GetMostLogicalCharacters()
    {
        isWaitUntilDone = true;
        //prevents doubling up on NewPlotpoint admin
        isPlotpointAdminDone = true;
        //character1 
        if (isChar1MostLogical == true)
        {
            isWaitUntilDone = false;
            GetDropDownCharacter(true);
        }
        yield return new WaitUntil(() => isWaitUntilDone == true);
        //character2
        if (isChar2MostLogical == true)
        {
            isWaitUntilDone = false;
            GetDropDownCharacter(false);
        }
        yield return new WaitUntil(() => isWaitUntilDone == true);
        //update character displays
        UpdateCharacterData();
        //toggle fields
        ToggleTurningPointFields(true, plotPoint.numberOfCharacters);
        //increment index
        plotPointIndex++;
        //End of turning point
        if (plotPointIndex >= 5)
        {
            //toggle save buttons
            turnPreSaveButton.gameObject.SetActive(true);
            turnSaveButton.gameObject.SetActive(false);
        }
        //ready for next plotPoint (spacebar input)
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input);
    }
    #endregion

    #region UpdateCharacterData
    /// <summary>
    /// Populates character data
    /// </summary>
    private void UpdateCharacterData()
    {
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
                /*//read or write field depending on whether already has data
                if (character1.listOfNotes.Count == 0)
                { turnCharacter1Input.text = character1.tag; }
                else { turnCharacter1.text = character1.tag; }*/

                turnCharacter1.text = character1.tag;
                turnCharacter1Input.text = character1.tag;
                turnData1.text = character1.dataCreated;
                turnData1Input.text = "";
                characters = character1.tag;
            }
            if (character2 != null)
            {
                /*if (character2.listOfNotes.Count == 0)
                { turnCharacter2Input.text = character2.tag; }
                else { turnCharacter2.text = character2.tag; }*/

                turnCharacter2.text = character2.tag;
                turnCharacter2Input.text = character2.tag;
                turnData2.text = character2.dataCreated;
                turnData2Input.text = "";
                characters = string.Format("{0} / {1}", characters, character2.tag);
            }
        }
        //add to turning point list of characters (next to plotpoint)
        if (characters.Length > 0)
        {
            if (plotPointIndex > -1 && plotPointIndex < arrayOfTurnCharacters.Length)
            { arrayOfTurnCharacters[plotPointIndex].text = characters; }
            else { Debug.LogWarningFormat("Invalid plotPointIndex \"{0}\" (should be between 0 and {1})", plotPointIndex, arrayOfTurnCharacters.Length); }
        }
    }
    #endregion

    #region GetIndividualCharacter
    /// <summary>
    /// Sub method of GetCharacters. Returns null if a problem. If refers to character1 or 2 depending on 'isCharacter1' true or false
    /// </summary>
    /// <returns></returns>
    private Character GetIndividualCharacter(bool isCharacter1)
    {
        Character character = null;
        ListItem item = storyNew.arrays.GetRandomCharacterFromArray();
        switch (item.status)
        {
            case StoryStatus.Data:
                Debug.LogFormat("[Tst] AdventureUI.cs -> GetIndividualCharacter: EXISTING character{0}{1}", isCharacter1 == true ? "1" : "2", "\n");
                //must be at least one entry in arrayOfCharacters in order to get an existing character
                if (storyNew.arrays.CheckDataItemsInArray() > 0)
                { character = GetExistingCharacter(item.tag); }
                else { character = GetNewCharacter(SpecialType.None); }
                break;
            case StoryStatus.Logical:
                Debug.LogFormat("[Tst] AdventureUI.cs -> GetIndividualCharacter: MOST LOGICAL character{0}{1}", isCharacter1 == true ? "1" : "2", "\n");
                //must be at least one entry in arrayOfCharacters in order to get most logical
                int count = storyNew.arrays.CheckDataItemsInArray();
                if (count > 0)
                {
                    //don't get characters, instead set flags for coroutine to deal with later
                    if (isCharacter1 == true)
                    { isChar1MostLogical = true; }
                    else
                    {
                        //if there are two characters needed and only two entries in arrayOfCharacters, second character is automatically a new character to avoid a possible duplication for character1 & 2
                        if (count > 2)
                        { isChar2MostLogical = true; }
                        else
                        { character = GetNewCharacter(SpecialType.None); }
                    }
                }
                else { character = GetNewCharacter(SpecialType.None); }
                break;
            case StoryStatus.New:
                Debug.LogFormat("[Tst] AdventureUI.cs -> GetIndividualCharacter: NEW Character{0}{1}", isCharacter1 == true ? "1" : "2", "\n");
                character = GetNewCharacter(SpecialType.None);
                break;
            default: Debug.LogWarningFormat("Unrecognised item.status \"[0}\"", item.status); break;
        }
        return character;
    }
    #endregion

    #region GetNewCharacter
    /// <summary>
    /// Returns a newly generated character, returns null if a problem, eg. no space remaining in array
    /// </summary>
    /// <returns></returns>
    private Character GetNewCharacter(SpecialType special)
    {
        Character character = null;
        //check there is space for another character
        if (storyNew.arrays.CheckSpaceInCharacterArray() == true)
        {
            //check if a constant character (could be a character, object or organisation
            if (UnityEngine.Random.Range(0, 100) < ToolManager.i.adventureScript.chanceOfConstantCharacter)
            {
                ConstantPlotpoint constantCharacter = ToolManager.i.toolDataScript.GetRandomConstantCharacter();
                if (constantCharacter != null)
                {
                    character = new Character() { tag = string.Format("[Con] {0}", constantCharacter.tag), refTag = constantCharacter.refTag, dataCreated = constantCharacter.details };
                    if (constantCharacter.type == ConstantSummaryType.Organisation) { character.special = SpecialType.Organisation; }
                    else if (constantCharacter.type == ConstantSummaryType.Object) { character.special = SpecialType.Object; }
                    else { character.special = SpecialType.None; }
                }
            }
            //Normal character generation -> constant either failed roll or no suitable characters available
            if (character == null)
            {
                switch (special)
                {
                    case SpecialType.None:
                        character = ToolManager.i.adventureScript.GetNewCharacter();
                        break;
                    case SpecialType.Organisation:
                        character = ToolManager.i.adventureScript.GetNewOrganisation();
                        break;
                    case SpecialType.OrgOrChar:
                        // 50/50 chance of either
                        if (UnityEngine.Random.Range(0, 100) < 50)
                        { character = ToolManager.i.adventureScript.GetNewCharacter(); }
                        else { character = ToolManager.i.adventureScript.GetNewOrganisation(); }
                        break;
                    case SpecialType.Object:
                        character = ToolManager.i.adventureScript.GetNewObject();
                        break;
                    default: Debug.LogWarningFormat("Unrecognised specialType \"{0}\"", special); break;
                }

                if (character != null)
                {
                    //add character to array and list
                    storyNew.arrays.AddCharacterToArray(new ListItem() { tag = character.refTag, status = StoryStatus.Data });
                    storyNew.lists.AddCharacterToList(character);
                }
                else { Debug.LogWarning("Invalid character (Null), NOT added to arrayOfCharacters, or listOfCharacters"); }
            }
        }
        else { Debug.LogWarning("Invalid New Character (No non-DATA slots remaining in arrayOfCharacters)"); }
        return character;
    }
    #endregion

    #region GetExistingCharacter
    /// <summary>
    /// Returns an existing character, returns null if a problem 
    /// </summary>
    /// <param name="refTag"></param>
    /// <returns></returns>
    private Character GetExistingCharacter(string refTag)
    {
        Character character = storyNew.lists.GetCharacterFromList(refTag);
        //add character to array
        storyNew.arrays.AddCharacterToArray(new ListItem() { tag = character.refTag, status = StoryStatus.Data });
        return character;
    }
    #endregion

    #region GetDropDownCharacter
    /// <summary>
    /// Obtains a character from a drop down list of existing characters
    /// </summary>
    /// <returns></returns>
    private void GetDropDownCharacter(bool isCharacter1)
    {
        List<Character> listOfCharacters = storyNew.lists.listOfCharacters;
        if (listOfCharacters != null)
        {
            if (listOfCharacters.Count > 0)
            {
                string header = "Unknown";
                if (isCharacter1)
                { header = string.Format("Choose Most Logical Character{0}{1}<color=\"blue\">{2}</color>{3}{4}<size=80%>{5}</size>", "\n", "\n", plotPoint.tag, "\n", "\n", plotPoint.details); }
                else
                {
                    //character2 -> show character1
                    header = string.Format("Choose Most Logical Character (<color=\"blue\">{0}){1}{2}{3}</color>{4}{5}<size=80%>{6}</size>",
                        character1.tag, "\n", "\n", plotPoint.tag, "\n", "\n", plotPoint.details);
                }
                InitialiseDropDownInput(listOfCharacters.Select(x => x.tag).ToList(), header);
                coroutineDropDown = WaitForDropDownCharacterInput(isCharacter1);
                StartCoroutine(coroutineDropDown);
            }
            else
            {
                //non-available, default to a new character
                Debug.LogFormat("[Tst] AdventureUI.cs -> GetDropDownCharacter: storyNew.listOfCharacters is EMPTY. Created a new Character instead{0}", "\n");
                if (isCharacter1 == true)
                { character1 = GetNewCharacter(SpecialType.None); }
                else { character2 = GetNewCharacter(SpecialType.None); }
            }
        }
        else { Debug.LogError("Invalid storyNew.listOfCharacters (Null)"); }
    }
    #endregion

    #endregion

    #region ClearPlotpoint...
    /// <summary>
    /// Clear out current plotPoint. 
    /// NOTE -> can only clear those wiht plotPointType 'None' or 'Normal' (redoing all the complicated stuff otherwise involves a lot of code)
    /// </summary>
    private void ClearPlotpoint()
    {
        if (plotPoint.type == PlotPointType.None || plotPoint.type == PlotPointType.Normal)
        {
            int index = plotPointIndex - 1;
            Debug.LogFormat("[Tst] AdventureUI.cs -> ClearPlotpoint: index {0} (plotPointIndex {1}, numberOfClears {2}){3}", index, plotPointIndex, numberOfClears, "\n");
            //error check for Clear button being pressed with plotPointIndex 0 (no plotPoints present)
            if (index > -1)
            {
                //clear out plotPoint and character displays
                if (arrayOfTurnPlotpoints[index].text.Length > 1)
                { arrayOfTurnPlotpoints[index].text = plotPointIndex.ToString(); }
                if (arrayOfTurnCharacters[index].text.Length > 1)
                { arrayOfTurnCharacters[index].text = plotPointIndex.ToString(); }
                arrayOfPlotpointNotes[index] = "";
                if (numberOfClears == 0)
                {
                    //check for characters (note they haven't been saved yet, not until NewPlotPoint is run, so accessing turnigPoint.arrayOfDetails isn't going to work)
                    if (character1 != null)
                    { TidyUpCharacter(character1); }
                    if (character2 != null)
                    { TidyUpCharacter(character2); }
                }
                else
                {
                    PlotDetails details = turningPoint.arrayOfDetails[index];
                    if (details != null)
                    {
                        if (details.character1 != null)
                        { TidyUpCharacter(details.character1); }
                        if (details.character2 != null)
                        { TidyUpCharacter(details.character2); }
                    }
                    else { Debug.LogErrorFormat("Invalid PlotDetails (Null) for turningPoint.arrayOfDetails[{0}]", index); }
                }
                //clear out plotPointDetails
                turningPoint.arrayOfDetails[index].Reset();
                //reset characters
                character1 = null;
                character2 = null;
                //reset data in plotPoint to prevent it being displayed after NewPlotPoint (won't be saved due to flag below but will still be present in fields and display routines will show it)
                plotPoint.Reset();
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
                //update plotLine in case of multiple clears so current plotPoint will be correct for the type check at the start
                plotPoint.tag = turningPoint.arrayOfDetails[plotPointIndex - 1].plotPoint;
                plotPoint.type = turningPoint.arrayOfDetails[plotPointIndex - 1].type;
                //clear save flag to prevent data being saved at next instance of NewPlotPoint
                isSavePlotPoint = false;
                //increment counter (gets reset by NewPlotPoint each time)
                numberOfClears++;
            }
        }
        else { Debug.LogWarningFormat("Can't clear plotPoint \"{0}\", type {1} ('None' or 'Normal' only) -> Info only", plotPoint.tag, plotPoint.type); }
    }

    /// <summary>
    /// Remove character from storyNew.arrayOfCharacters/listOfCharacters where appropriate for a cleared Plotpoint
    /// </summary>
    /// <param name="character"></param>
    private void TidyUpCharacter(Character character)
    {
        //array -> remove last entry of character, reset array entry back to default value
        for (int i = storyNew.arrays.arrayOfCharacters.Length - 1; i >= 0; i--)
        {
            ListItem item = storyNew.arrays.arrayOfCharacters[i];
            if (item.status == StoryStatus.Data)
            {
                if (item.tag.Equals(character.refTag, StringComparison.Ordinal) == true)
                {
                    //found last entry of character in array. Replace entry with default value
                    storyNew.arrays.SetCharacterArrayItemToDefault(i);
                    break;
                }
            }
        }
        //list -> if no more records of character in array then character is redundant and needs to be removed from listOfCharacters
        if (Array.Exists(storyNew.arrays.arrayOfCharacters, x => x.tag.Equals(character.refTag, StringComparison.Ordinal)) == false)
        { storyNew.lists.RemoveCharacterFromList(character); }
    }
    #endregion

    #region Save Methods...
    /// <summary>
    /// Pre Save operations and activate Save Button
    /// </summary>
    private void PreSaveTurningPoint()
    {
        //prevent spaceBar causing issues
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Process);
        //update characters
        if (character1 != null)
        {
            if (string.IsNullOrEmpty(turnData1Input.text) == false)
            { character1.listOfNotes.Add(turnData1Input.text); }
        }
        if (character2 != null)
        {
            if (string.IsNullOrEmpty(turnData2Input.text) == false)
            { character2.listOfNotes.Add(turnData2Input.text); }
        }
        // - - - Save last lot of details
        PlotDetails details = new PlotDetails()
        {
            isActive = false,
            plotPoint = plotPoint.tag,
            notes = turnPlotNotesInput.text,
            character1 = character1,
            character2 = character2,
            type = plotPoint.type
        };
        turningPoint.arrayOfDetails[plotPointIndex - 1] = details;
        Debug.LogFormat("[Tst] AdventureUI.cs -> PreSaveTurningPoint: \"{0}\" {1} / {2} SAVED to arrayOfDetails{3}", plotPoint.tag,
            character1 != null ? character1.tag : "Nobody", character2 != null ? character2.tag : "Nobody", "\n");
        //Save last set of notes
        arrayOfPlotpointNotes[plotPointIndex - 1] = turnPlotNotesInput.text;
        // - - - reset global characters
        character1 = null;
        character2 = null;
        //blank out character data
        ResetPlotPoint();
        if (turningPoint.type == TurningPointType.New)
        {
            turnName.gameObject.SetActive(false);
            turnNameInput.gameObject.SetActive(true);
            turnNameInput.text = "";
        }
        else
        {
            turnName.gameObject.SetActive(true);
            turnNameInput.gameObject.SetActive(false);
        }
        // - - - toggle fields
        ToggleTurningPointFields(false);
        /*turnPlotPoint.gameObject.SetActive(false);*/
        turnData0Input.gameObject.SetActive(true);
        //toggle save buttons
        turnPreSaveButton.gameObject.SetActive(false);
        turnSaveButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Save turning point to StoryNew
    /// </summary>
    private void SaveTurningPoint()
    {
        if (turningPoint.type == TurningPointType.New)
        {
            turningPoint.tag = turnNameInput.text;
            turningPoint.refTag = turnNameInput.text.Replace(" ", "");
        }
        //must have a name for TurningPoint
        if (string.IsNullOrEmpty(turningPoint.tag) == false)
        {
            //must have Notes for PlotLine (bottom of screen)
            if (string.IsNullOrEmpty(turnData0Input.text) == false)
            {
                turningPoint.notes = turnData0Input.text;
                //tags
                plotLine.tag = turningPoint.tag;
                plotLine.refTag = turningPoint.refTag;
                //add plotLine notes
                plotLine.listOfNotes.Add(turnData0Input.text);
                //Save PlotLine
                storyNew.arrays.AddPlotLineToArray(new ListItem() { tag = plotLine.refTag, status = StoryStatus.Data });
                storyNew.lists.AddPlotLineToList(plotLine);
                //SAVE
                storyNew.arrayOfTurningPoints[turningPointIndex] = turningPoint;
                isTurningPointSaved = true;
                Debug.LogFormat("[Tst] AdventureUI.cs -> SaveTurningPoint: turningPoint \"{0}\" SAVED{1}", turningPoint.tag, "\n");
                //clear out notes field
                turnData0Input.text = "";
                //update indexes
                turningPointIndex++;
                plotPointIndex = 0;
                //story isConcluded (maxCap of 5 turning points per story)
                if (turningPointIndex == 5)
                { storyNew.isConcluded = true; }
                //check if turningPoint concluded
                if (turningPoint.type == TurningPointType.Development)
                {
                    if (turningPoint.CheckNumberOfPlotPointType(PlotPointType.Conclusion) > 0)
                    {
                        //set turningPPoint to concluded
                        turningPoint.type = TurningPointType.Conclusion;
                        //remove plotLine
                        RemovePlotLine(turningPoint.refTag);
                    }
                }
                //exit or next turningPoint
                if (turningPointIndex < 5 && storyNew.isConcluded == false)
                {
                    //Generate new PlotLine / turningPoint
                    ListItem item = storyNew.arrays.GetRandomPlotLineFromArray();
                    Debug.LogFormat("[Tst] AdventureUI.cs -> SaveTurningPoint: ListItem tag \"{0}\", status {1} (New PlotLine){2}", item.tag, item.status, "\n");
                    if (item != null)
                    {
                        switch (item.status)
                        {
                            case StoryStatus.Data:
                                plotLine = new PlotLine(storyNew.lists.GetPlotLineFromList(item.tag));
                                if (plotLine != null)
                                {
                                    //development turning point
                                    turningPoint = new TurningPoint()
                                    {
                                        refTag = plotLine.refTag,
                                        tag = plotLine.tag,
                                        notes = "",
                                        type = TurningPointType.Development
                                    };
                                    Debug.LogFormat("[Tst] AdventureUI.cs -> SaveTurningPoint: EXISTING PlotLine  \"{0}\"{1}", plotLine.tag, "\n");
                                }
                                else
                                { Debug.LogErrorFormat("Invalid plotLine (Null) for \"{0}\"", item.tag); }
                                break;
                            case StoryStatus.Logical:
                                coroutinePlotLine = GetMostLogicalPlotLine();
                                StartCoroutine(coroutinePlotLine);
                                break;
                            case StoryStatus.New:
                                //new plotLine and turning point
                                plotLine = new PlotLine();
                                turningPoint = new TurningPoint()
                                {
                                    refTag = "",
                                    tag = "",
                                    notes = "",
                                    type = TurningPointType.New
                                };
                                Debug.LogFormat("[Tst] AdventureUI.cs -> SaveTurningPoint: NEW PlotLine {0}", "\n");
                                break;
                            default: Debug.LogWarningFormat("Unrecognised item.status \"{0}\"", item.status); break;
                        }
                        //Redraw ready for next turning point
                        RedrawTurningPointPage();
                    }
                    else { Debug.LogError("Invalid ListItem (Null) for new PlotLine"); }
                }
                else
                {
                    //story concluded -> exit
                    Debug.LogFormat("[Tst] AdventureUI.cs -> SaveTurningPoint: Story is CONCLUDED{0}", "\n");
                    CloseTurningPoint();
                }
                //switch off save button
                turnSaveButton.gameObject.SetActive(false);
                //ready for new Plotpoint (spacebar input)
                ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input);
            }
            else { Debug.LogWarning("Turning Point doesn't have any PlotLine Notes, can't be saved"); }
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
        storyNew.lists.DebugShowCharacterList();
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
        //reset New Adventure page
        RedrawNewAdventurePage();
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.New);
    }

    #endregion

    #region PlotLine Methods...
    /// <summary>
    /// Plotline concluded, remove from list and array
    /// </summary>
    /// <param name="refTag"></param>
    private void RemovePlotLine(string refTag)
    {
        //remove from list
        storyNew.lists.RemovePlotLineFromList(refTag);
        //replace in array with default values
        int counter = 0;
        for (int i = 0; i < storyNew.arrays.arrayOfPlotLines.Length; i++)
        {
            ListItem item = storyNew.arrays.arrayOfPlotLines[i];
            if (item != null)
            {
                if (item.tag.Equals(refTag, StringComparison.Ordinal) == true)
                {
                    storyNew.arrays.SetPlotLineArrayItemToDefault(i);
                    counter++;
                }
            }
            else { Debug.LogErrorFormat("Invalid ListItem (Null) in storyNew.arrays.arrayOfPlotLines[{0}]", i); }
        }
        Debug.LogFormat("[Tst] AdventureUI.cs -> RemovePlotLine: \"{0}\" plotLine replaced with default values in arrayOfPlotlines {1} times{2}", refTag, counter, "\n");
    }

    /// <summary>
    /// coroutine for getting the most logical Plotline from dropdown lists
    /// </summary>
    /// <returns></returns>
    IEnumerator GetMostLogicalPlotLine()
    {
        isWaitUntilDone = false;
        //check at least one item in listOfPlotLines
        if (storyNew.lists.listOfPlotLines.Count > 0)
        { GetDropDownPlotLine(); }
        else
        {
            //default to a New plotline 
            plotLine = new PlotLine();
            turningPoint = new TurningPoint()
            {
                refTag = "",
                tag = "",
                notes = "",
                type = TurningPointType.New
            };
            isWaitUntilDone = true;
            Debug.LogFormat("[Tst] AdventureUI.cs -> GetMostLogicalPlotLine: NEW PlotLine instead of Most Logical (listOfPlotLines is Empty) {0}", "\n");
        }
        yield return new WaitUntil(() => isWaitUntilDone == true);
        RedrawTurningPointPage();
    }

    /// <summary>
    /// Obtain a plotLine from a dropdown list of existing plotlines
    /// </summary>
    private void GetDropDownPlotLine()
    {
        InitialiseDropDownInput(storyNew.lists.listOfPlotLines.Select(x => x.tag).ToList(), "<color=\"blue\">Select Most Logical PlotLine");
        coroutineDropDown = WaitForDropDownPlotLineInput();
        StartCoroutine(coroutineDropDown);
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
        if (turningPoint.type == TurningPointType.New)
        {
            turnName.gameObject.SetActive(false);
            turnNameInput.gameObject.SetActive(false);
            turnNameImage.gameObject.SetActive(false);
        }
        else
        {
            turnName.gameObject.SetActive(true);
            turnNameImage.gameObject.SetActive(true);
            turnNameInput.gameObject.SetActive(false);
            turnName.text = turningPoint.tag;
        }
        ToggleTurningPointFields(false);
        //toggle save buttons
        turnPreSaveButton.gameObject.SetActive(false);
        turnSaveButton.gameObject.SetActive(false);
        //clear out fields
        turnPlotPoint.text = "";
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

    #region Delegates

    /// <summary>
    /// Character1 name OnSelect
    /// </summary>
    private void TurnCharacter1Select()
    {
        //swap mode to avoid space bar input generating a new plotPoint
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Edit);
    }

    /// <summary>
    /// Character1 name OnDeselect
    /// </summary>
    private void TurnCharacter1Deselect()
    {
        character1.tag = turnCharacter1Input.text;
        character1.refTag = turnCharacter1Input.text.Replace(" ", "");
        UpdateArrayOfCharacters();
        //ready for New Plotpoint (spacebar input)
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input);
    }

    /// <summary>
    /// Character2 name OnSelect
    /// </summary>
    private void TurnCharacter2Select()
    {
        //swap mode to avoid space bar input generating a new plotPoint
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Edit);
    }

    /// <summary>
    /// Character2 name OnDeselect
    /// </summary>
    private void TurnCharacter2Deselect()
    {
        character2.tag = turnCharacter2Input.text;
        character2.refTag = turnCharacter2Input.text.Replace(" ", "");
        UpdateArrayOfCharacters();
        //ready for New Plotpoint (spacebar input)
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input);
    }

    /// <summary>
    /// Data0 name OnSelect
    /// </summary>
    private void TurnData0Select()
    { ToolManager.i.toolInputScript.SetModalType(ToolModalType.Edit); }

    /// <summary>
    /// Data0 name OnDeselect
    /// </summary>
    private void TurnData0Deselect()
    { ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input); }

    /// <summary>
    /// Data1 name OnSelect
    /// </summary>
    private void TurnData1Select()
    { ToolManager.i.toolInputScript.SetModalType(ToolModalType.Edit); }

    /// <summary>
    /// Data1 name OnDeselect
    /// </summary>
    private void TurnData1Deselect()
    { ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input); }

    /// <summary>
    /// Data2 name OnSelect
    /// </summary>
    private void TurnData2Select()
    { ToolManager.i.toolInputScript.SetModalType(ToolModalType.Edit); }

    /// <summary>
    /// Data2 name OnDeselect
    /// </summary>
    private void TurnData2Deselect()
    { ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input); }

    /// <summary>
    /// Plotpoint Notes OnSelect
    /// </summary>
    private void TurnPlotNotesSelect()
    { ToolManager.i.toolInputScript.SetModalType(ToolModalType.Edit); }

    /// <summary>
    /// Plotpoint Notes name OnDeselect
    /// </summary>
    private void TurnPlotNotesDeselect()
    { ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input); }

    /// <summary>
    /// subMethod for character delegates to redo arrayOfTurnCharacters to reflect any changes in character names
    /// </summary>
    private void UpdateArrayOfCharacters()
    {
        string characters = character1.tag;
        if (character2 != null && character2.tag.Length > 0)
        { characters = string.Format("{0} / {1}", characters, character2.tag); }
        //index - 1 as has already been incremented
        arrayOfTurnCharacters[plotPointIndex - 1].text = characters;
    }

    #endregion

    #endregion

    #region Drop Down

    #region InitialiseDropDownInput
    /// <summary>
    /// Sets up drop down input prior
    /// </summary>
    private void InitialiseDropDownInput(List<string> listOfOptions, string header)
    {
        //reset input fields to defaults
        dropDownInputInt = -1;
        dropDownInputString = "";
        //set options
        if (listOfOptions != null)
        {
            dropInput.options.Clear();
            for (int i = 0; i < listOfOptions.Count; i++)
            {
                dropInput.options.Add(new TMP_Dropdown.OptionData() { text = listOfOptions[i] });
            }
        }
        else { Debug.LogError("Invalid listOfOptions (Null)"); }
        //set header
        if (string.IsNullOrEmpty(header) == false)
        { dropHeader.text = header; }
        else { Debug.LogError("Invalid header (Null or Empty)"); }
        //set index
        dropInput.value = -1;

    }
    #endregion

    #region WaitFor Methods...
    /// <summary>
    /// wait for input from drop down pop-up
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForDropDownCharacterInput(bool isCharacter1)
    {
        dropDownCanvas.gameObject.SetActive(true);
        yield return new WaitUntil(() => isWaitUntilDone == true);
        if (dropDownInputInt > -1)
        {
            if (isCharacter1 == true)
            {
                character1 = storyNew.lists.GetCharacterFromList(dropDownInputInt);
                storyNew.arrays.AddCharacterToArray(new ListItem() { tag = character1.refTag, status = StoryStatus.Data });
                Debug.LogFormat("[Tst] AdventureUI.cs -> GetIndividualCharacter: MOST LOGICAL Character1 \"{0}\" selected{1}", character1 != null ? character1.tag : "Unknown", "\n");
            }
            else
            {
                character2 = storyNew.lists.GetCharacterFromList(dropDownInputInt);
                storyNew.arrays.AddCharacterToArray(new ListItem() { tag = character2.refTag, status = StoryStatus.Data });
                Debug.LogFormat("[Tst] AdventureUI.cs -> GetIndividualCharacter: MOST LOGICAL Character2 \"{0}\" selected{1}", character2 != null ? character2.tag : "Unknown", "\n");
            }
        }
    }

    /// <summary>
    /// wait for input from drop down pop-up
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForDropDownPlotLineInput()
    {
        /*isWaitUntilDone = false;*/
        dropDownCanvas.gameObject.SetActive(true);
        yield return new WaitUntil(() => isWaitUntilDone == true);
        if (dropDownInputInt > -1)
        {
            plotLine = new PlotLine(storyNew.lists.GetPlotLineFromList(dropDownInputInt));
            storyNew.arrays.AddPlotLineToArray(new ListItem() { tag = plotLine.refTag, status = StoryStatus.Data });
            turningPoint = new TurningPoint()
            {
                refTag = plotLine.refTag,
                tag = plotLine.tag,
                notes = "",
                type = TurningPointType.Development
            };
            Debug.LogFormat("[Tst] AdventureUI.cs -> WaitForDropDownPlotLineInput: MOST LOGICAL plotLine \"{0}\"{1}", plotLine.tag, "\n");
        }
    }

    /// <summary>
    /// Wait for input from drop down pop-up
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForDropDownNameSetInput()
    {
        dropDownCanvas.gameObject.SetActive(true);
        yield return new WaitUntil(() => isWaitUntilDone == true);
        if (dropDownInputInt > -1)
        {
            //change currently used nameSet to Input choice
            ToolManager.i.adventureScript.SetNameSet(dropDownInputInt);
            //update screen header
            newNameSetHeader.text = ToolManager.i.adventureScript.GetNameSetInUse();
            //update storyNew
            storyNew.nameSet = ToolManager.i.adventureScript.GetNameSetInUse();
        }
        Debug.LogFormat("[Tst] AdventureUI.cs -> WaitForDropDownNameSetInput: Selected NameSet \"{0}\"{1}", dropDownInputString, "\n");
    }
    #endregion

    /// <summary>
    /// Gets option selected from dropDown pick list
    /// </summary>
    private void DropDownItemSelected()
    {
        int index = dropInput.value;
        //fail safe check, if button clicked and nothing selected, choose default option
        if (index < 0) { index = 0; }
        //set input values
        dropDownInputInt = index;
        dropDownInputString = dropInput.options[index].text;
        Debug.LogFormat("[Tst] AdventureUI.cs -> DropDownItemSelected: \"{0}\", index {1} SELECTED{2}", dropDownInputString, dropDownInputInt, "\n");
    }

    /// <summary>
    /// Confirm button clicked, drop down input pop-up closed
    /// </summary>
    private void CloseDropDownInput()
    {
        isWaitUntilDone = true;
        dropDownCanvas.gameObject.SetActive(false);
    }
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
        { saveFileButton.gameObject.SetActive(true); }
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

    #region Constants
    //
    // - - - Constants
    //

    /// <summary>
    /// Initialise constants page upon opening
    /// </summary>
    private void InitialiseConstants()
    {
        //toggle buttons Off
        saveToDictConstantButton.gameObject.SetActive(false);
        saveToFileConstantButton.gameObject.SetActive(false);
        viewFilterConstantButton.gameObject.SetActive(false);
        deleteConstantButton.gameObject.SetActive(false);
        deleteCampaignConstantButton.gameObject.SetActive(false);
        //display summaries
        UpdateConstantSummaries();
        SetConstantCheckboxesOff();
        ToggleConstantTextInputs(true);
        //initialise list
        InitialiseListOfConstantPlotpoints();
        //set Modal Type
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input);
        SetConstantInstructionText();
    }

    /// <summary>
    /// Close constants screen and return to
    /// </summary>
    private void CloseConstants()
    {
        constantsCanvas.gameObject.SetActive(false);
        masterCanvas.gameObject.SetActive(true);
        //check if saveToFile button should be on
        if (isSaveNeeded == true)
        { saveFileButton.gameObject.SetActive(true); }
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Main);
    }

    /// <summary>
    /// delegate -> Input detected on either textSmall or Large (item name or notes)
    /// </summary>
    private void ConstantTextInputDetected()
    {
        //activate saveToDict button
        saveToDictConstantButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Switch to input mode
    /// </summary>
    private void InputConstants()
    {
        //enable checkboxes
        ToggleConstantCheckboxes(true);
        //turn on input fields
        ToggleConstantTextInputs(true);
        //clear out input fields
        ClearConstantInputFields();
        //set Modal Type
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input);
        SetConstantInstructionText();
        //toggle delete button
        deleteConstantButton.gameObject.SetActive(false);
        deleteCampaignConstantButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// View contents of dictOfConstantPlotpoints -> ALL records
    /// </summary>
    private void ViewConstants()
    {
        //initialise list
        InitialiseListOfConstantPlotpoints();
        //disable checkboxes
        ToggleConstantCheckboxes(false);
        //set Modal Type
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Read);
        //toggle delete button
        deleteConstantButton.gameObject.SetActive(true);
        deleteCampaignConstantButton.gameObject.SetActive(true);
        //turn off text inputs
        ToggleConstantTextInputs(false);
        //show first record, if present
        DisplayConstantPlotpoint();
        SetConstantInstructionText();
    }

    /// <summary>
    /// Deletes currently constant from dictionary (need to go to main page and press 'SaveToFile' to lock in changes). Valid in Read mode only
    /// </summary>
    private void DeleteConstants()
    {
        //read mode only
        if (ToolManager.i.toolInputScript.ModalType == ToolModalType.Read)
        {
            //must be at least one record
            if (listOfConstantPlotpoints.Count > 0)
            {
                string constantTag = listOfConstantPlotpoints[constantIndex].tag;
                //delete record
                if (ToolManager.i.toolDataScript.RemoveConstantPlotpoint(listOfConstantPlotpoints[constantIndex]) == true)
                {
                    Debug.LogFormat("[Tol] AdventureUI.cs -> DeleteConstants: \"{0}\" removed from dictionary{1}", constantTag, "\n");
                    //re-initialise list
                    InitialiseListOfConstantPlotpoints();
                    //update summaries
                    UpdateConstantSummaries();
                    //display
                    SetConstantCheckboxesOff();
                    constantTextSmall.text = "";
                    constantTextLarge.text = "";
                    //activate SaveToFile button
                    saveToFileConstantButton.gameObject.SetActive(true);
                   
                }
            }
            else { Debug.LogWarning("Can't delete constantPlotpoint as none present -> Info only"); }
        }
    }

    /// <summary>
    /// deletes all campaign scope constants in dictionary (clear out ready for next campaign) .Need to go to main page and press 'SaveToFile' to lock in changes. Valid only in Red mode.
    /// </summary>
    private void DeleteCampaignConstants()
    {
        //read mode only
        if (ToolManager.i.toolInputScript.ModalType == ToolModalType.Read)
        {
            int numRemoved = ToolManager.i.toolDataScript.RemoveCampaignConstants();
            if (numRemoved > 0)
            {
                Debug.LogFormat("[Tol] AdventureUI.cs -> DeleteCampaignConstants: {0} record{1} removed from dictOfConstantPlotpoints{2}", numRemoved, numRemoved != 1 ? "s" : "", "\n");
                //re-initialise list
                InitialiseListOfConstantPlotpoints();
                //update summaries
                UpdateConstantSummaries();
                //display list from start
                DisplayConstantPlotpoint();
                //activate SaveToFile button
                saveToFileConstantButton.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Filter to create a subSet of constantPlotpoints to view. Clears out checkboxes and screen and activates ViewFilter button 
    /// </summary>
    private void FilterConstants()
    {
        //clear out input fields
        ClearConstantInputFields();
        //toggle fields off
        ToggleConstantTextInputs(false);
        //enable checkboxes
        ToggleConstantCheckboxes(true);
        //set Modal Type
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Process);
        //activate viewfilter button
        viewFilterConstantButton.gameObject.SetActive(true);
        //deactivate filter button
        filterConstantButton.gameObject.SetActive(false);
        //toggle delete button
        deleteConstantButton.gameObject.SetActive(false);
        deleteCampaignConstantButton.gameObject.SetActive(false);
        //instruction text
        SetConstantInstructionText();
    }

    /// <summary>
    /// View plotpoints according to the current settings on the Scope and Type checkboxes
    /// </summary>
    private void ViewFilterConstants()
    {
        //set filter
        InitialiseListOfConstantPlotpoints(true);
        Debug.LogFormat("[Tst] AdventureUI.cs -> ViewFilterConstants: There are {0} filtered records{1}", listOfConstantPlotpoints.Count, "\n");
        //show first record, if present
        DisplayConstantPlotpoint();
        //toggle buttons
        viewFilterConstantButton.gameObject.SetActive(false);
        filterConstantButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// switch to Edit mode for currently displayed record. Valid in Read mode only
    /// </summary>
    private void EditConstants()
    {
        if (ToolManager.i.toolInputScript.ModalType == ToolModalType.Read)
        {
            //enable checkboxes
            ToggleConstantCheckboxes(true);
            //turn on input fields
            ToggleConstantTextInputs(true);
            //set Modal Type
            ToolManager.i.toolInputScript.SetModalType(ToolModalType.Edit);
            //instruction text
            SetConstantInstructionText();
            //display current record
            DisplayConstantPlotpoint();
            //toggle delete button
            deleteConstantButton.gameObject.SetActive(false);
            deleteCampaignConstantButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// clear out currently displayed record. Valid in Input or Edit modes only
    /// </summary>
    private void ClearConstants()
    {
        if (ToolManager.i.toolInputScript.ModalType == ToolModalType.Edit || ToolManager.i.toolInputScript.ModalType == ToolModalType.Input)
        {
            ClearConstantInputFields();
            SetConstantCheckboxesOff();
            if (ToolManager.i.toolInputScript.ModalType == ToolModalType.Edit)
            {
                //activate saveToDict button that needs to be pressed in order to save changes
                saveToDictConstantButton.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// View next constantPlotPoint (working forward through listOfConstantPlotpoints)
    /// </summary>
    private void NextConstant()
    {
        constantIndex++;
        //rollover check
        if (constantIndex == constantLimit)
        { constantIndex = 0; }
        //show story
        DisplayConstantPlotpoint();
    }

    /// <summary>
    /// View previous constantPlotpoint (working back through listOfConstantPlotpoints
    /// </summary>
    private void PreviousConstant()
    {
        constantIndex--;
        //rollover check
        if (constantIndex < 0)
        { constantIndex = constantLimit - 1; }
        //show story
        DisplayConstantPlotpoint();
    }

    /// <summary>
    /// Write constant data to file
    /// </summary>
    private void SaveToFileConstants()
    {
        ToolManager.i.toolFileScript.WriteConstantsData();
        ToolManager.i.toolFileScript.SaveConstantsToFile();
        //disable button
        saveToFileConstantButton.gameObject.SetActive(false);
    }


    #region delegates
    /// <summary>
    /// delegate to change colour of checkbox (Scope) when selected/deselected
    /// </summary>
    private void ChangeCheckBoxScopeColour()
    {
        //change all to white except if selected (yellow)
        for (int i = 0; i < arrayOfConstantScopeTexts.Length; i++)
        {
            if (arrayOfConstantScopeToggles[i].isOn == true)
            { arrayOfConstantScopeTexts[i].color = colourYellow; }
            else { arrayOfConstantScopeTexts[i].color = colourWhite; }
        }
    }

    /// <summary>
    /// delegate to change colour of checkbox (Type) when selected/deselected
    /// </summary>
    private void ChangeCheckBoxTypeColour()
    {
        //change all to white except if selected (yellow)
        for (int i = 0; i < arrayOfConstantTypeTexts.Length; i++)
        {
            if (arrayOfConstantTypeToggles[i].isOn == true)
            { arrayOfConstantTypeTexts[i].color = colourYellow; }
            else { arrayOfConstantTypeTexts[i].color = colourWhite; }
        }
    }

    /// <summary>
    /// delegate to change colour of checkbox (Frequency) when selected/deselected
    /// </summary>
    private void ChangeCheckBoxFrequencyColour()
    {
        //change all to white except if selected (yellow)
        for (int i = 0; i < arrayOfConstantFrequencyTexts.Length; i++)
        {
            if (arrayOfConstantFrequencyToggles[i].isOn == true)
            { arrayOfConstantFrequencyTexts[i].color = colourYellow; }
            else { arrayOfConstantFrequencyTexts[i].color = colourWhite; }
        }
    }
    #endregion

    #region SaveToDictConstants
    /// <summary>
    /// save a ConstantPlotpoint to dictOfConstants. NOTE: this data will be lost if you don't SaveToFile before exiting
    /// </summary>
    private void SaveToDictConstants()
    {
        bool isProceed = true;
        //assign some values (irrelevant as not used but needed for code integrity)
        ConstantScope scopeActual = ConstantScope.Campaign;
        ConstantSummaryType typeActual = ConstantSummaryType.Character;
        ConstantDistribution frequencyActual = ConstantDistribution.High;
        //check all data is present -> Scope
        isProceed = false;
        for (int i = 0; i < arrayOfConstantScopeToggles.Length; i++)
        {
            if (arrayOfConstantScopeToggles[i].isOn == true)
            {
                scopeActual = (ConstantScope)i;
                isProceed = true;
                break;
            }
        }
        if (isProceed == true)
        {
            //Type
            isProceed = false;
            for (int i = 0; i < arrayOfConstantTypeToggles.Length; i++)
            {
                if (arrayOfConstantTypeToggles[i].isOn == true)
                {
                    typeActual = (ConstantSummaryType)i;
                    isProceed = true;
                    break;
                }
            }
            if (isProceed == true)
            {
                //Frequency
                isProceed = false;
                for (int i = 0; i < arrayOfConstantFrequencyToggles.Length; i++)
                {
                    if (arrayOfConstantFrequencyToggles[i].isOn == true)
                    {
                        frequencyActual = (ConstantDistribution)i;
                        isProceed = true;
                        break;
                    }
                }
                if (isProceed == true)
                {
                    //input texts
                    if (constantTextSmallInput.text.Length > 0 && constantTextLargeInput.text.Length > 0)
                    {
                        //save record
                        ConstantPlotpoint constantPlotpoint = new ConstantPlotpoint()
                        {
                            tag = constantTextSmallInput.text,
                            refTag = constantTextSmallInput.text.Replace(" ", ""),
                            details = constantTextLargeInput.text,
                            scope = scopeActual,
                            type = typeActual,
                            frequency = frequencyActual
                        };
                        //save to dict
                        ToolManager.i.toolDataScript.AddConstantPlotpoint(constantPlotpoint);
                        //clear fields ready for next record
                        SetConstantCheckboxesOff();
                        constantTextSmallInput.text = "";
                        constantTextLargeInput.text = "";
                        //update summaries
                        UpdateConstantSummaries();
                        //update list
                        InitialiseListOfConstantPlotpoints();
                        //activate SaveToFile button (main page) and deactivate SaveToDict
                        saveToDictConstantButton.gameObject.SetActive(false);
                        saveToFileConstantButton.gameObject.SetActive(true);
                        //log
                        Debug.LogFormat("[Tol] AdventureUI.cs -> SaveToDict: \"{0}\" -> {1} -> {2} -> {3} saved to dictOfConstantPlotPoints{4}", constantPlotpoint.tag, constantPlotpoint.scope,
                            constantPlotpoint.type, constantPlotpoint.frequency, "\n");
                    }
                }
                else { Debug.LogWarning("Can't save to dict as invalid Frequency (none checked)"); }
            }
            else { Debug.LogWarning("Can't save to dict as invalid Type (none checked)"); }
        }
        else { Debug.LogWarning("Can't save to dict as Invalid Scope (none checked)"); }
    }
    #endregion


    #endregion

    // - - - Assorted

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

    #region Main Utilities
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
        //populate summary panel
        DisplayMainSummary(newSummaryIndex);
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
    /// Toggle Story Notes (input or read)
    /// </summary>
    /// <param name="isInput"></param>
    private void DisplayStoryNotes(bool isInput)
    {
        if (isInput == true)
        {
            //Edit input
            mainNotesInput.gameObject.SetActive(true);
            mainNotesInput.text = storyMain.notes;
            mainNotes.gameObject.SetActive(false);
        }
        else
        {
            //Read only
            mainNotesInput.gameObject.SetActive(false);
            mainNotes.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Displays turning point details page
    /// </summary>
    /// <param name="summaryIndex"></param>
    private void DisplayMainDetails(int summaryIndex, int detailsIndex, bool isFirstOpened = false)
    {
        string characters;
        TurningPoint turningPoint = storyMain.arrayOfTurningPoints[summaryIndex];
        if (turningPoint != null)
        {
            mainTurnNumber.text = string.Format("{0}", summaryIndex + 1);
            mainTurnName.text = turningPoint.tag;
            //toggles
            switch (turningPoint.type)
            {
                case TurningPointType.New:
                    arrayOfMainToggles[0].isOn = true;
                    arrayOfMainToggles[1].isOn = false;
                    arrayOfMainToggles[2].isOn = false;
                    break;
                case TurningPointType.Development:
                    arrayOfMainToggles[0].isOn = false;
                    arrayOfMainToggles[1].isOn = true;
                    arrayOfMainToggles[2].isOn = false;
                    break;
                case TurningPointType.Conclusion:
                    arrayOfMainToggles[0].isOn = false;
                    arrayOfMainToggles[1].isOn = false;
                    arrayOfMainToggles[2].isOn = true;
                    break;
                case TurningPointType.None:
                    arrayOfMainToggles[0].isOn = false;
                    arrayOfMainToggles[1].isOn = false;
                    arrayOfMainToggles[2].isOn = false;
                    break;
            }
            //NOTE: When first, notes are for the Turning Point as a whole (In Yellow). Once you use arrow keys Up and Down it reverts to PlotPoint notes (normal colour)
            if (isFirstOpened == true)
            { mainTurnNotes.text = string.Format("<color=\"yellow\">[TurningPoint] <color=\"white\">{0}", turningPoint.notes.Length > 0 ? turningPoint.notes : "None"); }
            else
            { mainTurnNotes.text = string.Format("[Plotpoint] {0}", turningPoint.arrayOfDetails[detailsIndex].notes.Length > 0 ? turningPoint.arrayOfDetails[detailsIndex].notes : "None"); }
            //plotpoints and characters
            for (int i = 0; i < turningPoint.arrayOfDetails.Length; i++)
            {
                //plotpoints - - -

                //DEBUG (plotPoints currently in file are tags not refTags
                string plotString = turningPoint.arrayOfDetails[i].plotPoint;
                Plotpoint plotPoint = ToolManager.i.toolDataScript.GetPlotpointFromTag(plotString);

                /*Plotpoint plotPoint = ToolManager.i.toolDataScript.GetPlotpoint(turningPoint.arrayOfDetails[i].plotPoint);*/
                if (plotPoint != null)
                {
                    if (detailsIndex == i)
                    { arrayOfMainPlotPoints[i].text = string.Format("<color=\"yellow\">{0}", plotPoint.tag); }
                    else { arrayOfMainPlotPoints[i].text = plotPoint.tag; }
                }
                else
                {
                    if (detailsIndex == i)
                    { arrayOfMainPlotPoints[i].text = string.Format("<color=\"yellow\">{0}", turningPoint.arrayOfDetails[i].plotPoint); }
                    else { arrayOfMainPlotPoints[i].text = turningPoint.arrayOfDetails[i].plotPoint; }
                }
                //characters - - -
                characters = (i + 1).ToString();
                //character1
                if (turningPoint.arrayOfDetails[i].character1 != null && string.IsNullOrEmpty(turningPoint.arrayOfDetails[i].character1.tag) == false)
                {
                    characters = turningPoint.arrayOfDetails[i].character1.tag;
                    //character2
                    if (turningPoint.arrayOfDetails[i].character2 != null && string.IsNullOrEmpty(turningPoint.arrayOfDetails[i].character2.tag) == false)
                    { characters = string.Format("{0} / {1}", characters, turningPoint.arrayOfDetails[i].character2.tag); }
                }
                if (detailsIndex == i)
                { arrayOfMainCharacters[i].text = string.Format("<color=\"yellow\">{0}", characters); }
                else { arrayOfMainCharacters[i].text = characters; }
            }
        }
        else { Debug.LogWarningFormat("Invalid turningPoint (Null) for storyMain.arrayOfTurningPoints[{0}]", mainSummaryIndex); }
    }

    /// <summary>
    /// Populates Summary Panel on main adventure screen. Index refers to currently selected TurningPoint (index values -> 0 to 4)
    /// </summary>
    private void DisplayMainSummary(int index)
    {
        if (index > 4 || index < 0) { Debug.LogWarningFormat("Invalid index \"[0}\" (should be between 0 and 4)", index); }
        index = Mathf.Clamp(index, 0, 4);
        //populate panel
        for (int i = 0; i < storyMain.arrayOfTurningPoints.Length; i++)
        {
            TurningPoint turningPoint = storyMain.arrayOfTurningPoints[i];
            if (turningPoint.type != TurningPointType.None)
            {
                //valid turningPoint (current index in yellow)
                if (i == mainSummaryIndex)
                { arrayOfMainTurningPoints[i].text = string.Format("<color=\"yellow\">{0} {1}", i + 1, turningPoint.tag); }
                else
                { arrayOfMainTurningPoints[i].text = string.Format("<color=\"white\">{0} {1}", i + 1, turningPoint.tag); }
                arrayOfMainTurningPointNotes[i] = turningPoint.notes;
            }
            else
            {
                //empty turning point
                if (i == mainSummaryIndex)
                { arrayOfMainTurningPoints[i].text = string.Format("<color=\"yellow\">{0}", i + 1); }
                else { arrayOfMainTurningPoints[i].text = string.Format("<color=\"white\">{0}", i + 1); }
                arrayOfMainTurningPointNotes[i] = "";
            }
        }
        //set  note
        mainTurningPointNotes.text = arrayOfMainTurningPointNotes[index];
    }

    /// <summary>
    /// Sets instruction text at page bottom according to currently set ModalType (so call AFTER you've changed modal type)
    /// </summary>
    private void SetMainInstructions()
    {
        switch (ToolManager.i.toolInputScript.ModalType)
        {
            case ToolModalType.Read:
                mainInstructions.text = "LEFT and RIGHT ARROWS<br>to browse Adventures";
                break;
            case ToolModalType.Details:
                mainInstructions.text = "ESC to Return";
                break;
        }
    }

    #endregion

    #region New Adventure Utilities

    /// <summary>
    /// Update data for New Adventure page
    /// </summary>
    private void RedrawNewAdventurePage()
    {
        //set modalSubNew
        if (turningPointIndex == 0) { ToolManager.i.toolInputScript.SetModalSubNew(ToolModalSubNew.New); }
        else { ToolManager.i.toolInputScript.SetModalSubNew(ToolModalSubNew.Summary); }
        //set display state
        newTag.text = storyNew.tag;
        newNotes.text = storyNew.notes;
        newDate.text = storyNew.date;
        themeNew1.text = storyNew.theme.GetThemeType(1).ToString();
        themeNew2.text = storyNew.theme.GetThemeType(2).ToString();
        themeNew3.text = storyNew.theme.GetThemeType(3).ToString();
        themeNew4.text = storyNew.theme.GetThemeType(4).ToString();
        themeNew5.text = storyNew.theme.GetThemeType(5).ToString();
        if (turningPointIndex == 0)
        {
            NewNormalPanel.gameObject.SetActive(true);
            NewSummaryPanel.gameObject.SetActive(false);
            //zero out seed plotlines and characters regardless
            for (int i = 0; i < arrayOfNewPlotLines.Length; i++)
            {
                arrayOfNewPlotLines[i].text = "";
                arrayOfNewCharacters[i].text = "";
            }
        }
        else
        {
            //summary panel
            NewNormalPanel.gameObject.SetActive(false);
            NewSummaryPanel.gameObject.SetActive(true);
            //populate summary panel
            DisplayNewSummary(newSummaryIndex);

        }
    }

    /// <summary>
    /// Populates Summary Panel on new adventure screen. Index refers to currently selected TurningPoint (index values -> 0 to 4)
    /// </summary>
    private void DisplayNewSummary(int index)
    {
        if (index > 4 || index < 0) { Debug.LogWarningFormat("Invalid index \"[0}\" (should be between 0 and 4)", index); }
        index = Mathf.Clamp(index, 0, 4);
        //populate panel
        for (int i = 0; i < storyNew.arrayOfTurningPoints.Length; i++)
        {
            TurningPoint turningPoint = storyNew.arrayOfTurningPoints[i];
            if (turningPoint.type != TurningPointType.None)
            {
                //valid turningPoint (current index in yellow)
                if (i == newSummaryIndex)
                { arrayOfNewTurningPoints[i].text = string.Format("<color=\"yellow\">{0} {1}", i + 1, turningPoint.tag); }
                else
                { arrayOfNewTurningPoints[i].text = string.Format("<color=\"white\">{0} {1}", i + 1, turningPoint.tag); }
                arrayOfNewTurningPointNotes[i] = turningPoint.notes;
            }
            else
            {
                //empty turning point
                if (i == newSummaryIndex)
                { arrayOfNewTurningPoints[i].text = string.Format("<color=\"yellow\">{0}", i + 1); }
                else { arrayOfNewTurningPoints[i].text = string.Format("<color=\"white\">{0}", i + 1); }
                arrayOfNewTurningPointNotes[i] = "";
            }
        }
        //set  note
        newTurningPointNotes.text = arrayOfNewTurningPointNotes[index];
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
    private void UpdateListOfStories()
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


    #endregion

    #region List Utilities

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
                        listDataCreated.text = "";
                        listDataMe.text = plotLine.GetNotes();
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
                        listDataMe.text = character.GetNotes();
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
                        listCreatedInput.text = "";
                        listDataMe.text = plotLine.GetNotes();
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
                        listMeInput.text = "";
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
                        if (string.IsNullOrEmpty(listCreatedInput.text) == false)
                        { plotLine.listOfNotes.Add(listCreatedInput.text); }
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
                        if (string.IsNullOrEmpty(listMeInput.text) == false)
                        { character.listOfNotes.Add(listMeInput.text); }
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
        switch (listItemStatus)
        {
            case ListItemStatus.Character:
                //character has two input fields
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
                break;
            case ListItemStatus.PlotLine:
                //Plotline only has one input field
                if (isInput == true)
                {
                    //toggle normal fields off
                    listDataCreated.gameObject.SetActive(false);
                    listDataMe.gameObject.SetActive(true);
                    //toggle input On
                    listCreatedInput.gameObject.SetActive(true);
                    listMeInput.gameObject.SetActive(false);
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
                break;
        }
    }

    #endregion

    #region TurningPoint Utilities

    /// <summary>
    /// toggles data display or data input for TurningPage plotpoint and character fields. If isInput true then numOfCharacters param is used to toggle the appropriate fields
    /// </summary>
    /// <param name="isInput"></param>
    private void ToggleTurningPointFields(bool isInput, int numOfCharacters = -1)
    {
        //do regardless
        turnData1.gameObject.SetActive(true);
        turnData2.gameObject.SetActive(true);
        turnData0Input.gameObject.SetActive(false);
        //Input
        if (isInput == true)
        {
            if (numOfCharacters == -1)
            { Debug.LogError("Invalid numOfCharacters (-1), should be between 0 and 2"); }
            //always
            turnPlotpointNotes.gameObject.SetActive(false);
            turnPlotNotesInput.gameObject.SetActive(true);
            //at least one character
            if (numOfCharacters > 0)
            {
                turnCharacter1Input.gameObject.SetActive(true);
                turnData1Input.gameObject.SetActive(true);
            }
            //two characters
            if (numOfCharacters > 1)
            {
                turnCharacter2Input.gameObject.SetActive(true);
                turnData2Input.gameObject.SetActive(true);
            }
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
        turnData0.text = "Plotpoint Notes";
        turnCharacter1.text = "Character 1";
        turnCharacter2.text = "Character 2";
        turnCharacter1Input.text = "";
        turnCharacter2Input.text = "";
        turnData1.text = "";
        turnData2.text = "";
        turnData1Input.text = "";
        turnData2Input.text = "";
        turnPlotNotesInput.text = "";
    }

    #endregion

    #region Constants Utilities
    /// <summary>
    /// Calculates and displayed updated info for game and campaign summaries
    /// </summary>
    private void UpdateConstantSummaries()
    {
        Dictionary<string, ConstantPlotpoint> dictOfConstants = ToolManager.i.toolDataScript.GetDictOfConstantPlotpoints();
        if (dictOfConstants != null)
        {
            int count = dictOfConstants.Count;
            if (count > 0)
            {
                //split into two collections
                var gameScope = dictOfConstants.Where(x => x.Value.scope == ConstantScope.Game);
                var campaignScope = dictOfConstants.Where(x => x.Value.scope == ConstantScope.Campaign);
                //Plotpoints
                arrayOfGameSummary[(int)ConstantSummaryType.Plotpoint].text = gameScope.Where(x => x.Value.type == ConstantSummaryType.Plotpoint).Count().ToString();
                arrayOfCampaignSummary[(int)ConstantSummaryType.Plotpoint].text = campaignScope.Where(x => x.Value.type == ConstantSummaryType.Plotpoint).Count().ToString();
                //Characters
                arrayOfGameSummary[(int)ConstantSummaryType.Character].text = gameScope.Where(x => x.Value.type == ConstantSummaryType.Character).Count().ToString();
                arrayOfCampaignSummary[(int)ConstantSummaryType.Character].text = campaignScope.Where(x => x.Value.type == ConstantSummaryType.Character).Count().ToString();
                //Organisations
                arrayOfGameSummary[(int)ConstantSummaryType.Organisation].text = gameScope.Where(x => x.Value.type == ConstantSummaryType.Organisation).Count().ToString();
                arrayOfCampaignSummary[(int)ConstantSummaryType.Organisation].text = campaignScope.Where(x => x.Value.type == ConstantSummaryType.Organisation).Count().ToString();
                //Objects
                arrayOfGameSummary[(int)ConstantSummaryType.Object].text = gameScope.Where(x => x.Value.type == ConstantSummaryType.Object).Count().ToString();
                arrayOfCampaignSummary[(int)ConstantSummaryType.Object].text = campaignScope.Where(x => x.Value.type == ConstantSummaryType.Object).Count().ToString();
                //totals
                arrayOfGameSummary[(int)ConstantSummaryType.Total].text = gameScope.Count().ToString();
                arrayOfCampaignSummary[(int)ConstantSummaryType.Total].text = campaignScope.Count().ToString();
            }
            else
            {
                //no records, assign zero values to all
                for (int i = 0; i < arrayOfGameSummary.Length; i++)
                {
                    arrayOfGameSummary[i].text = "0";
                    arrayOfCampaignSummary[i].text = "0";
                }
            }

        }
        else { Debug.LogError("Invalid dictOfConstants (Null)"); }
    }

    /// <summary>
    /// Sets all checkboxes to Off (doesn't change enable/disable status, just blanks all checkboxes and sets labels to white colour)
    /// </summary>
    private void SetConstantCheckboxesOff()
    {
        for (int i = 0; i < arrayOfConstantScopeToggles.Length; i++)
        {
            arrayOfConstantScopeToggles[i].isOn = false;
            arrayOfConstantScopeTexts[i].color = colourWhite;
        }
        for (int i = 0; i < arrayOfConstantTypeToggles.Length; i++)
        {
            arrayOfConstantTypeToggles[i].isOn = false;
            arrayOfConstantTypeTexts[i].color = colourWhite;
        }
        for (int i = 0; i < arrayOfConstantFrequencyToggles.Length; i++)
        {
            arrayOfConstantFrequencyToggles[i].isOn = false;
            arrayOfConstantFrequencyTexts[i].color = colourWhite;
        }
    }

    /// <summary>
    /// Toggles constant checkboxes interactable true/false
    /// </summary>
    /// <param name="isOn"></param>
    private void ToggleConstantCheckboxes(bool isOn)
    {
        if (isOn == true)
        {
            //activate checkboxes
            for (int i = 0; i < arrayOfConstantScopeToggles.Length; i++)
            { arrayOfConstantScopeToggles[i].interactable = true; }
            for (int i = 0; i < arrayOfConstantTypeToggles.Length; i++)
            { arrayOfConstantTypeToggles[i].interactable = true; }
            for (int i = 0; i < arrayOfConstantFrequencyToggles.Length; i++)
            { arrayOfConstantFrequencyToggles[i].interactable = true; }
        }
        else
        {
            //disable checkboxes
            for (int i = 0; i < arrayOfConstantScopeToggles.Length; i++)
            { arrayOfConstantScopeToggles[i].interactable = false; }
            for (int i = 0; i < arrayOfConstantTypeToggles.Length; i++)
            { arrayOfConstantTypeToggles[i].interactable = false; }
            for (int i = 0; i < arrayOfConstantFrequencyToggles.Length; i++)
            { arrayOfConstantFrequencyToggles[i].interactable = false; }
        }
    }



    /// <summary>
    /// Toggles large and small text input fields on/off
    /// </summary>
    /// <param name="isInput"></param>
    private void ToggleConstantTextInputs(bool isInput)
    {
        if (isInput == true)
        {
            constantTextSmallInput.gameObject.SetActive(true);
            constantTextLargeInput.gameObject.SetActive(true);
        }
        else
        {
            constantTextSmallInput.gameObject.SetActive(false);
            constantTextLargeInput.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Clear out constant input fields
    /// </summary>
    private void ClearConstantInputFields()
    {
        constantTextSmallInput.text = "";
        constantTextLargeInput.text = "";
    }

    /// <summary>
    /// Initialise listOfConstantPlotpoints with data from dictOfConstantPlotpoints. If 'isFilteredList' true then list will be filtered according to Scope and Type settings
    /// </summary>
    private void InitialiseListOfConstantPlotpoints(bool isFilteredList = false)
    {
        Dictionary<string, ConstantPlotpoint> dictOfConstantPlotpoints = ToolManager.i.toolDataScript.GetDictOfConstantPlotpoints();
        if (dictOfConstantPlotpoints != null)
        {
            if (isFilteredList == false)
            {
                //show All records
                listOfConstantPlotpoints = dictOfConstantPlotpoints.Values.ToList();
            }
            else
            {
                //filtered list
                int scopeFilter = -1;
                int typeFilter = -1;
                //Get Scope filter (if any)
                for (int i = 0; i < arrayOfConstantScopeToggles.Length; i++)
                {
                    if (arrayOfConstantScopeToggles[i].isOn == true)
                    {
                        scopeFilter = i;
                        break;
                    }
                }
                //Get Type filter (if any)
                for (int i = 0; i < arrayOfConstantTypeToggles.Length; i++)
                {
                    if (arrayOfConstantTypeToggles[i].isOn == true)
                    {
                        typeFilter = i;
                        break;
                    }
                }
                //Get ConstantplotPoints
                var selection = dictOfConstantPlotpoints.Values;
                //scope filter
                if (scopeFilter > -1)
                { listOfConstantPlotpoints = selection.Where(x => x.scope == (ConstantScope)scopeFilter).ToList(); }
                else { Debug.LogWarningFormat("Invalid scopeFilter \"{0}\"", scopeFilter); }
                //type filter
                if (typeFilter > -1)
                { listOfConstantPlotpoints = listOfConstantPlotpoints.Where(x => x.type == (ConstantSummaryType)typeFilter).ToList(); }
                else { Debug.LogWarningFormat("Invalid typeFilter \"{0}\"", typeFilter); }
            }
            constantIndex = 0;
            constantLimit = listOfConstantPlotpoints.Count;
        }
        else { Debug.LogError("Invalid dictOfConstantPlotpoints (Null)"); }
    }

    /// <summary>
    /// displays current record in listOfConstantPlotpoints[constantIndex]. Valid only in Read or Edit mode
    /// </summary>
    private void DisplayConstantPlotpoint()
    {
        ToolModalType modalType = ToolManager.i.toolInputScript.ModalType;
        if (modalType == ToolModalType.Edit || modalType == ToolModalType.Read || modalType == ToolModalType.Process)
        {
            //check at least one record
            if (listOfConstantPlotpoints.Count > 0)
            {
                ConstantPlotpoint plotPoint = listOfConstantPlotpoints[constantIndex];
                if (plotPoint != null)
                {
                    int index;
                    //display data
                    switch (modalType)
                    {
                        case ToolModalType.Edit:
                            constantTextSmallInput.text = plotPoint.tag;
                            constantTextLargeInput.text = plotPoint.details;
                            break;
                        case ToolModalType.Process:
                        case ToolModalType.Read:
                            constantTextSmall.text = plotPoint.tag;
                            constantTextLarge.text = plotPoint.details;
                            break;
                        default: Debug.LogWarningFormat("Unrecognised modalType \"{0}\"", modalType); break;
                    }
                    //set all checkpoints OFF
                    SetConstantCheckboxesOff();
                    //scope
                    index = (int)plotPoint.scope;
                    arrayOfConstantScopeToggles[index].isOn = true;
                    arrayOfConstantScopeTexts[index].color = colourYellow;
                    //type
                    index = (int)plotPoint.type;
                    arrayOfConstantTypeToggles[index].isOn = true;
                    arrayOfConstantTypeTexts[index].color = colourYellow;
                    //frequency
                    index = (int)plotPoint.frequency;
                    arrayOfConstantFrequencyToggles[index].isOn = true;
                    arrayOfConstantFrequencyTexts[index].color = colourYellow;
                }
                else { Debug.LogErrorFormat("Invalid constantPlotpoint (Null) for listOfConstantPlotpoints[{0}]", constantIndex); }
            }
            else
            {
                //no records, clear out fields
                constantTextSmall.text = "";
                constantTextLarge.text = "";
            }
        }
    }

    /// <summary>
    /// Sets instruction text according to the current mode
    /// </summary>
    private void SetConstantInstructionText()
    {
        switch (ToolManager.i.toolInputScript.ModalType)
        {
            case ToolModalType.Input:
                constantInstructionText.text = "Click SAVE TO DICT to save Input<br>Once all input complete, return to MAIN PAGE and click SAVE TO FILE";
                break;
            case ToolModalType.Edit:
                constantInstructionText.text = "Click SAVE TO DICT to save Input<br>Once all input complete, return to MAIN PAGE and click SAVE TO FILE";
                break;
            case ToolModalType.Read:
                constantInstructionText.text = "ALL records are selected<br>LEFT and RIGHT Arrows to navigate records";
                break;
            case ToolModalType.Process:
                //filter
                constantInstructionText.text = "Filter with SCOPE and TYPE checkboxes<br>Then click FILTER VIEW and use LEFT and RIGHT Arrows to view selection";
                break;
        }
    }

    #endregion

    #endregion

    //new scripts above here
}

#endif
