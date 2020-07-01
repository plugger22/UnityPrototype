﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using toolsAPI;
using UnityEngine;


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
    public Canvas listsCanvas;

    //Stories
    private Story storyMain;
    private Story storyNew;

    //Temporary Dictionaries
    Dictionary<string, PlotLine> dictOfPlotLines = new Dictionary<string, PlotLine>();
    Dictionary<string, Character> dictOfCharacters = new Dictionary<string, Character>();

    //Navigation
    private int mainNavCounter;
    private int mainNavLimit;
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

    public TextMeshProUGUI themeMain1;
    public TextMeshProUGUI themeMain2;
    public TextMeshProUGUI themeMain3;
    public TextMeshProUGUI themeMain4;
    public TextMeshProUGUI themeMain5;

    public TextMeshProUGUI mainTag;
    public TextMeshProUGUI mainNotes;
    public TextMeshProUGUI mainDate;
    #endregion

    #region New Adventure
    [Header("New Adventure")]
    public ToolButtonInteraction saveNewInteraction;
    public ToolButtonInteraction turningPointNewInteraction;
    public ToolButtonInteraction clearNewInteraction;
    public ToolButtonInteraction returnNewInteraction;

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
    public TMP_InputField listNameInput;
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
        Debug.Assert(listsCanvas != null, "Invalid listsCanvas (Null)");
        Debug.Assert(newAdventureInteraction != null, "Invalid newAdventureInteraction (Null)");
        Debug.Assert(saveToFileInteraction != null, "Invalid saveAdventureInteraction (Null)");
        Debug.Assert(loadFromFileInteraction != null, "Invalid loadAdventureInteraction (Null)");
        Debug.Assert(editAdventureInteraction != null, "Invalid editAdventureInteraction (Null)");
        Debug.Assert(removeAdventureInteraction != null, "Invalid removeAdventureInteraction (Null)");
        Debug.Assert(showListsInteraction != null, "Invalid showListInteraction (Null)");
        Debug.Assert(deleteFileInteraction != null, "Invalid deleteFileInteraction (Null)");
        Debug.Assert(clearDictionaryInteraction != null, "Invalid clearDictionaryInteraction (Null)");
        Debug.Assert(exitInteraction != null, "Invalid ExitInteraction (Null)");
        Debug.Assert(saveNewInteraction != null, "Invalid themeInteraction (Null)");
        Debug.Assert(turningPointNewInteraction != null, "Invalid turnPointInteraction (Null)");
        Debug.Assert(returnNewInteraction != null, "Invalid returnNewInteraction (Null)");
        Debug.Assert(clearNewInteraction != null, "Invalid clearNewInteraction (Null)");
        //main
        Debug.Assert(themeMain1 != null, "Invalid theme1 (Null)");
        Debug.Assert(themeMain2 != null, "Invalid theme2 (Null)");
        Debug.Assert(themeMain3 != null, "Invalid theme3 (Null)");
        Debug.Assert(themeMain4 != null, "Invalid theme4 (Null)");
        Debug.Assert(themeMain5 != null, "Invalid theme5 (Null)");
        Debug.Assert(mainTag != null, "Invalid mainTag (Null)");
        Debug.Assert(mainNotes != null, "Invalid mainNotes (Null)");
        Debug.Assert(mainDate != null, "Invalid mainDate (Null)");
        //new adventure
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
        //lists
        Debug.Assert(returnListsInteraction != null, "Invalid returnListsInteraction (Null)");
        Debug.Assert(listEditInteraction != null, "Invalid listEditInteraction (Null)");
        Debug.Assert(listSaveInteraction != null, "Invalid listSaveInteraction (Null)");
        Debug.Assert(listAdventureName != null, "Invalid listAdventureName (Null)");
        Debug.Assert(listAdventureDate != null, "Invalid listAdventureDate (Null)");
        Debug.Assert(listName != null, "Invalid listName (Null)");
        Debug.Assert(listDataCreated != null, "Invalid listDataCreated (Null)");
        Debug.Assert(listDataMe != null, "Invalid listDataMe (Null)");
        Debug.Assert(listNameInput != null, "Invalid listNameInput (Null)");
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
        listsCanvas.gameObject.SetActive(false);
        //assign button events
        exitInteraction.SetButton(ToolEventType.CloseAdventureUI);
        newAdventureInteraction.SetButton(ToolEventType.OpenNewAdventure);
        returnNewInteraction.SetButton(ToolEventType.CloseNewAdventure);
        turningPointNewInteraction.SetButton(ToolEventType.CreateTurningPoint);
        saveNewInteraction.SetButton(ToolEventType.SaveAdventureToDict);
        saveToFileInteraction.SetButton(ToolEventType.SaveToolsToFile);
        loadFromFileInteraction.SetButton(ToolEventType.LoadToolsFromFile);
        deleteFileInteraction.SetButton(ToolEventType.DeleteToolsFile);
        clearDictionaryInteraction.SetButton(ToolEventType.ClearAdventureDictionary);
        clearNewInteraction.SetButton(ToolEventType.ClearNewAdventure);
        showListsInteraction.SetButton(ToolEventType.OpenAdventureLists);
        returnListsInteraction.SetButton(ToolEventType.CloseAdventureLists);
        listEditInteraction.SetButton(ToolEventType.EditListItem);
        listSaveInteraction.SetButton(ToolEventType.SaveListDetails);
        //list buttonInteractions
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
                CreateTurningPoint();
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
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion


    #region AdventureMain
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
        //theme
        CreateTheme();
        //auto fill date
        newDate.text = GetCurrentDateString();
        storyNew.date = GetCurrentDateString();
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
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Main);
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
    /// Create New Turning Points for Adventure
    /// </summary>
    private void CreateTurningPoint()
    {

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
    }

    /// <summary>
    /// Save Adventure (base data)
    /// </summary>
    private void SaveAdventureToDict()
    {
        if (storyNew != null)
        {
            storyNew.tag = newTag.text;
            storyNew.notes = newNotes.text;
            storyNew.date = newDate.text;
            //plotlines
            for (int i = 0; i < arrayOfNewPlotLines.Length; i++)
            {
                if (string.IsNullOrEmpty(arrayOfNewPlotLines[i].text) == false)
                {
                    storyNew.lists.arrayOfPlotLines[i].tag = arrayOfNewPlotLines[i].text;
                    storyNew.lists.arrayOfPlotLines[i].status = StoryStatus.Data;
                }
                if (string.IsNullOrEmpty(arrayOfNewCharacters[i].text) == false)
                {
                    storyNew.lists.arrayOfCharacters[i].tag = arrayOfNewCharacters[i].text;
                    storyNew.lists.arrayOfCharacters[i].status = StoryStatus.Data;
                }
            }
            //save only if viable data present
            if (string.IsNullOrEmpty(storyNew.tag) == false)
            {
                //data validate date string
                if (string.IsNullOrEmpty(storyNew.date) == true)
                { storyNew.date = GetCurrentDateString(); }
                //add story to dict, overwrite existing data if already present
                ToolManager.i.toolDataScript.AddStory(storyNew);
                Debug.LogFormat("[Tol] AdventureUI.cs -> SaveAdventure: StoryNew \"{0}\" saved to dictionary{1}", storyNew.tag, "\n");
                //update navigation
                GetListOfStories();
            }
            else { Debug.LogWarning("Invalid storyNew.tag (Null or Empty). Story not saved to Dictionary"); }
        }
        else { Debug.LogError("Invalid storyNew (Null)"); }
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
        ToggleListFields(true);
        SetListInputFields();
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Edit);
    }


    /// <summary>
    /// Save an edited PlotLine or Character
    /// </summary>
    private void SaveListDetails()
    {
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
    /// <param name="tag"></param>
    /// <returns></returns>
    private PlotLine GetPlotLine(string tag)
    {
        if (dictOfPlotLines.ContainsKey(tag) == true)
        { return dictOfPlotLines[tag]; }
        else { Debug.LogWarningFormat("tag \"{0}\" not found in dictOfPlotlines", tag); }
        return null;
    }

    /// <summary>
    /// Returns character based on tag, null if a problem or not found
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    private Character GetCharacter(string tag)
    {
        if (dictOfCharacters.ContainsKey(tag) == true)
        { return dictOfCharacters[tag]; }
        else { Debug.LogWarningFormat("tag \"{0}\" not found in dictOfCharacters", tag); }
        return null;
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
            themeMain1.text = storyMain.theme.GetThemePriority(1).ToString();
            themeMain2.text = storyMain.theme.GetThemePriority(2).ToString();
            themeMain3.text = storyMain.theme.GetThemePriority(3).ToString();
            themeMain4.text = storyMain.theme.GetThemePriority(4).ToString();
            themeMain5.text = storyMain.theme.GetThemePriority(5).ToString();

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
        themeNew1.text = storyNew.theme.GetThemePriority(1).ToString();
        themeNew2.text = storyNew.theme.GetThemePriority(2).ToString();
        themeNew3.text = storyNew.theme.GetThemePriority(3).ToString();
        themeNew4.text = storyNew.theme.GetThemePriority(4).ToString();
        themeNew5.text = storyNew.theme.GetThemePriority(5).ToString();
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
        storyMain = listOfStories[mainNavCounter];
        if (storyMain != null)
        {
            //initialise quick access Dictionaries
            dictOfPlotLines.Clear();
            dictOfCharacters.Clear();
            //populate data onscreen
            RedrawMainAdventurePage();
        }
        else { Debug.LogWarningFormat("Invalid story (Null) from listOfStories[{0}]", mainNavCounter); }
    }

    /// <summary>
    /// Display lists of Plotlines and Characters on Lists Page
    /// </summary>
    private void DisplayLists()
    {
        storyMain = listOfStories[mainNavCounter];
        if (storyMain != null)
        {
            //populate temp dictionaries
            dictOfPlotLines = storyMain.listOfPlotLines.ToDictionary(k => k.tag);
            dictOfCharacters = storyMain.listOfCharacters.ToDictionary(k => k.tag);
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
                ListItem item = storyMain.lists.arrayOfPlotLines[i];
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
                item = storyMain.lists.arrayOfCharacters[i];
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
                ListItem item = storyMain.lists.arrayOfPlotLines[index];
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
                ListItem item = storyMain.lists.arrayOfCharacters[index];
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
    private void SetListInputFields()
    {
        switch (listItemStatus)
        {
            case ListItemStatus.PlotLine:
                ListItem item = storyMain.lists.arrayOfPlotLines[currentListIndex];
                if (item != null)
                {
                    PlotLine plotLine = GetPlotLine(item.tag);
                    if (plotLine != null)
                    {
                        listNameInput.text = plotLine.tag;
                        listCreatedInput.text = plotLine.dataCreated;
                        listMeInput.text = plotLine.dataMe;
                    }
                    else { Debug.LogErrorFormat("Invalid plotLine (Null) for ListItem.tag \"{0}\"", item.tag); }
                }
                else { Debug.LogErrorFormat("Invalid ListItem (Null) for arrayOfPlotLines[{0}]", currentListIndex); }
                break;
            case ListItemStatus.Character:
                item = storyMain.lists.arrayOfCharacters[currentListIndex];
                if (item != null)
                {
                    Character character = GetCharacter(item.tag);
                    if (character != null)
                    {
                        listNameInput.text = character.tag;
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
                ListItem item = storyMain.lists.arrayOfPlotLines[currentListIndex];
                if (item != null)
                {
                    //can't change name of Plotline
                    item.status = StoryStatus.Data;
                    PlotLine plotLine = GetPlotLine(item.tag);
                    if (plotLine != null)
                    {
                        plotLine.tag = listNameInput.text;
                        plotLine.dataCreated = listCreatedInput.text;
                        plotLine.dataMe = listMeInput.text;
                    }
                    else { Debug.LogErrorFormat("Invalid plotLine (Null) for item.tag \"{0}\"", item.tag); }
                }
                else { Debug.LogErrorFormat("Invalid plotLine (Null) for arrayOfPlotLines[{0}]", currentListIndex); }
                break;
            case ListItemStatus.Character:
                item = storyMain.lists.arrayOfCharacters[currentListIndex];
                if (item != null)
                {
                    //can't change name of Character
                    item.status = StoryStatus.Data;
                    Character character = GetCharacter(item.tag);
                    if (character != null)
                    {
                        character.tag = listNameInput.text;
                        character.dataCreated = listCreatedInput.text;
                        character.dataMe = listMeInput.text;
                    }
                    else { Debug.LogErrorFormat("Invalid character (Null) for item.tag \"{0}\"", item.tag); }
                }
                else { Debug.LogErrorFormat("Invalid item (Null) for arrayOfCharacters[{0}]", currentListIndex); }
                break;
            default: Debug.LogWarningFormat("Unrecognised listItemStatus \"{0}\"", listItemStatus); break;
        }
    }

    /// <summary>
    /// Toggles fields on/off for List page RHS
    /// </summary>
    /// <param name="isInput"></param>
    private void ToggleListFields(bool isInput)
    {
        if (isInput == true)
        {
            //toggle normal fields off
            listName.gameObject.SetActive(false);
            listDataCreated.gameObject.SetActive(false);
            listDataMe.gameObject.SetActive(false);
            //toggle input On
            listNameInput.gameObject.SetActive(true);
            listCreatedInput.gameObject.SetActive(true);
            listMeInput.gameObject.SetActive(true);
        }
        else
        {
            //toggle normal fields on
            listName.gameObject.SetActive(true);
            listDataCreated.gameObject.SetActive(true);
            listDataMe.gameObject.SetActive(true);
            //toggle input off
            listNameInput.gameObject.SetActive(false);
            listCreatedInput.gameObject.SetActive(false);
            listMeInput.gameObject.SetActive(false);
        }
    }

    #endregion

    //new scripts above here
}

#endif
