using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using toolsAPI;
using UnityEngine;


#if (UNITY_EDITOR)

/// <summary>
/// Runs adventure generator UI in Internal Tools scene
/// </summary>
public class AdventureUI : MonoBehaviour
{
    #region Main
    public Canvas adventureCanvas;
    public Canvas masterCanvas;
    public Canvas newAdventureCanvas;

    //Story that is current
    public Story storyMain;
    public Story storyNew;

    //static reference
    private static AdventureUI adventureUI;
    #endregion

    #region Main
    [Header("Main")]
    public ToolButtonInteraction newAdventureInteraction;
    public ToolButtonInteraction saveToFileInteraction;
    public ToolButtonInteraction loadFromFileInteraction;
    public ToolButtonInteraction editAdventureInteraction;
    public ToolButtonInteraction deleteFileInteraction;
    public ToolButtonInteraction removeAdventureInteraction;
    public ToolButtonInteraction showListsInteraction;
    public ToolButtonInteraction removePlotLineInteraction;
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
    public ToolButtonInteraction returnNewInteraction;

    public TextMeshProUGUI themeNew1;
    public TextMeshProUGUI themeNew2;
    public TextMeshProUGUI themeNew3;
    public TextMeshProUGUI themeNew4;
    public TextMeshProUGUI themeNew5;

    public TextMeshProUGUI adventureTag;
    public TextMeshProUGUI adventureNotes;
    public TextMeshProUGUI adventureDate;
    #endregion

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


    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        //error check
        Debug.Assert(adventureCanvas != null, "Invalid adventureCanvas (Null)");
        Debug.Assert(masterCanvas != null, "Invalid masterCanvas (Null)");
        Debug.Assert(newAdventureCanvas != null, "Invalid newAdventureCanvas (Null)");
        Debug.Assert(newAdventureInteraction != null, "Invalid newAdventureInteraction (Null)");
        Debug.Assert(saveToFileInteraction != null, "Invalid saveAdventureInteraction (Null)");
        Debug.Assert(loadFromFileInteraction != null, "Invalid loadAdventureInteraction (Null)");
        Debug.Assert(editAdventureInteraction != null, "Invalid editAdventureInteraction (Null)");
        Debug.Assert(removeAdventureInteraction != null, "Invalid removeAdventureInteraction (Null)");
        Debug.Assert(showListsInteraction != null, "Invalid showListInteraction (Null)");
        Debug.Assert(deleteFileInteraction != null, "Invalid deleteFileInteraction (Null)");
        Debug.Assert(removePlotLineInteraction != null, "Invalid removePlotLineInteraction (Null)");
        Debug.Assert(exitInteraction != null, "Invalid ExitInteraction (Null)");
        Debug.Assert(saveNewInteraction != null, "Invalid themeInteraction (Null)");
        Debug.Assert(turningPointNewInteraction != null, "Invalid turnPointInteraction (Null)");
        Debug.Assert(returnNewInteraction != null, "Invalid returnNewInteraction (Null)");
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
        Debug.Assert(adventureTag != null, "Invalid adventureTag (Null)");
        Debug.Assert(adventureNotes != null, "Invalid adventureNotes (Null)");
        Debug.Assert(adventureDate != null, "Invalid adventureDate (Null)");
        //switch off
        adventureCanvas.gameObject.SetActive(false);
        masterCanvas.gameObject.SetActive(true);
        newAdventureCanvas.gameObject.SetActive(false);
        //assign button events
        exitInteraction.SetButton(ToolEventType.CloseAdventureUI);
        newAdventureInteraction.SetButton(ToolEventType.OpenNewAdventure);
        returnNewInteraction.SetButton(ToolEventType.CloseNewAdventure);
        turningPointNewInteraction.SetButton(ToolEventType.CreateTurningPoint);
        saveNewInteraction.SetButton(ToolEventType.SaveAdventureToDict);
        saveToFileInteraction.SetButton(ToolEventType.SaveToolsToFile);
        loadFromFileInteraction.SetButton(ToolEventType.LoadToolsFromFile);
        deleteFileInteraction.SetButton(ToolEventType.DeleteToolsFile);

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
    }



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
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// open
    /// </summary>
    private void OpenAdventureUI()
    {
        //turn on
        ToolManager.i.toolUIScript.CloseMainMenu();
        adventureCanvas.gameObject.SetActive(true);
        //reset storyNew
        storyNew = new Story();
        //load data from dictionary automatically
        LoadAdventures();
    }


    /// <summary>
    /// close
    /// </summary>
    private void CloseAdventureUI()
    {
        ToolManager.i.toolUIScript.OpenMainMenu();
        adventureCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// open new adventure page
    /// </summary>
    private void OpenNewAdventure()
    {
        //create new story if none present
        if (storyNew == null)
        { storyNew = new Story(); }
        //toggle canvases on/off
        newAdventureCanvas.gameObject.SetActive(true);
        masterCanvas.gameObject.SetActive(false);
        //theme
        CreateTheme();
        //auto fill date
        DateTime date = DateTime.Now;
        adventureDate.text = date.ToString("d MMM yy", CultureInfo.CreateSpecificCulture("en-US"));
    }

    /// <summary>
    /// close new adventure page
    /// </summary>
    private void CloseNewAdventure()
    {
        //reset StoryNew
        storyNew = new Story();
        adventureTag.text = "";
        adventureNotes.text = "";
        adventureDate.text = "";
        themeNew1.text = "";
        themeNew2.text = "";
        themeNew3.text = "";
        themeNew4.text = "";
        themeNew5.text = "";
        //toggle canvases
        masterCanvas.gameObject.SetActive(true);
        newAdventureCanvas.gameObject.SetActive(false);

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
    /// Save Adventure (base data)
    /// </summary>
    private void SaveAdventureToDict()
    {
        if (storyNew != null)
        {
            //update data
            storyNew.tag = adventureTag.text;
            storyNew.date = adventureDate.text;
            storyNew.notes = adventureNotes.text;
            //Add story to dictionary if not already present
            if (ToolManager.i.toolDataScript.CheckStoryExists(storyNew) == false)
            {
                ToolManager.i.toolDataScript.AddStory(storyNew);
                Debug.LogFormat("[Tol] AdventureUI.cs -> SaveAdventure: StoryNew \"{0}\" saved to dictionary{1}", storyNew.tag, "\n");
            }
        }
        else { Debug.LogError("Invalid storyNew (Null)"); }
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
    }

    //subMethod to handle loading adventures and populating main screen with data from first record in dictionary
    private void LoadAdventures()
    {
        //read data from file
        if (ToolManager.i.toolFileScript.ReadToolsFromFile() == true)
        {
            //load data into game
            ToolManager.i.toolFileScript.ReadToolData();
            //load up first story in dict
            storyMain = ToolManager.i.toolDataScript.GetFirstStoryInDict();
            if (storyMain != null)
            {
                //populate data onscreen
                mainTag.text = storyMain.tag;
                mainDate.text = storyMain.date;
                mainNotes.text = storyMain.notes;
                themeMain1.text = storyMain.theme.arrayOfThemes[0].ToString();
                themeMain2.text = storyMain.theme.arrayOfThemes[4].ToString();
                themeMain3.text = storyMain.theme.arrayOfThemes[7].ToString();
                themeMain4.text = storyMain.theme.arrayOfThemes[9].ToString();
                themeMain5.text = storyMain.theme.arrayOfThemes[10].ToString();
            }
            else { Debug.LogWarning("Invalid story (Null) from firstStoryInDict"); }
        }
    }



    //new scripts above here
}

#endif
