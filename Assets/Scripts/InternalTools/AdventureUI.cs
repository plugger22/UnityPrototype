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
    public Story story;

    //static reference
    private static AdventureUI adventureUI;
    #endregion

    #region Master
    [Header("Master")]
    public ToolButtonInteraction newAdventureInteraction;
    public ToolButtonInteraction loadAdventureInteraction;
    public ToolButtonInteraction saveToFileInteraction;
    public ToolButtonInteraction editAdventureInteraction;
    public ToolButtonInteraction removeAdventureInteraction;
    public ToolButtonInteraction showListsInteraction;
    public ToolButtonInteraction addPlotLineInteraction;
    public ToolButtonInteraction removePlotLineInteraction;
    public ToolButtonInteraction exitInteraction;

    #endregion

    #region New Adventure
    [Header("New Adventure")]
    public ToolButtonInteraction saveNewInteraction;
    public ToolButtonInteraction turningPointNewInteraction;
    public ToolButtonInteraction returnNewInteraction;

    public TextMeshProUGUI theme1;
    public TextMeshProUGUI theme2;
    public TextMeshProUGUI theme3;
    public TextMeshProUGUI theme4;
    public TextMeshProUGUI theme5;

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
        Debug.Assert(loadAdventureInteraction != null, "Invalid loadAdventureInteraction (Null)");
        Debug.Assert(saveToFileInteraction != null, "Invalid saveAdventureInteraction (Null)");
        Debug.Assert(editAdventureInteraction != null, "Invalid editAdventureInteraction (Null)");
        Debug.Assert(removeAdventureInteraction != null, "Invalid removeAdventureInteraction (Null)");
        Debug.Assert(showListsInteraction != null, "Invalid showListInteraction (Null)");
        Debug.Assert(addPlotLineInteraction != null, "Invalid addPlotLineInteraction (Null)");
        Debug.Assert(removePlotLineInteraction != null, "Invalid removePlotLineInteraction (Null)");
        Debug.Assert(exitInteraction != null, "Invalid ExitInteraction (Null)");
        Debug.Assert(saveNewInteraction != null, "Invalid themeInteraction (Null)");
        Debug.Assert(turningPointNewInteraction != null, "Invalid turnPointInteraction (Null)");
        Debug.Assert(returnNewInteraction != null, "Invalid returnNewInteraction (Null)");
        Debug.Assert(theme1 != null, "Invalid theme1 (Null)");
        Debug.Assert(theme2 != null, "Invalid theme2 (Null)");
        Debug.Assert(theme3 != null, "Invalid theme3 (Null)");
        Debug.Assert(theme4 != null, "Invalid theme4 (Null)");
        Debug.Assert(theme5 != null, "Invalid theme5 (Null)");
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
        saveToFileInteraction.SetButton(ToolEventType.SaveDictToFile);
        //listeners
        ToolEvents.i.AddListener(ToolEventType.OpenAdventureUI, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.CloseAdventureUI, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.OpenNewAdventure, OnEvent, "AdventureUI");
        ToolEvents.i.AddListener(ToolEventType.SaveDictToFile, OnEvent, "AdventureUI");
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
                SetAdventureUI();
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
            case ToolEventType.SaveDictToFile:
                SaveToFile();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// open
    /// </summary>
    private void SetAdventureUI()
    {
        //turn on
        ToolManager.i.toolUIScript.CloseMainMenu();
        adventureCanvas.gameObject.SetActive(true);
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
        if (story == null)
        { story = new Story(); }
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
                for (int i = 0; i < listOfThemes.Count; i++)
                {
                    ThemeType themeType = listOfThemes[i];
                    //array
                    story.theme.arrayOfThemes[i] = themeType;
                    switch (i)
                    {
                        case 0:
                            story.theme.arrayOfThemes[0] = themeType;
                            story.theme.arrayOfThemes[1] = themeType;
                            story.theme.arrayOfThemes[2] = themeType;
                            story.theme.arrayOfThemes[3] = themeType;
                            //ui
                            theme1.text = string.Format("1  {0}", themeType);
                            break;
                        case 1:
                            story.theme.arrayOfThemes[4] = themeType;
                            story.theme.arrayOfThemes[5] = themeType;
                            story.theme.arrayOfThemes[6] = themeType;
                            //ui
                            theme2.text = string.Format("2  {0}", themeType);
                            break;
                        case 2:
                            story.theme.arrayOfThemes[7] = themeType;
                            story.theme.arrayOfThemes[8] = themeType;
                            //ui
                            theme3.text = string.Format("3  {0}", themeType);
                            break;
                        case 3:
                            story.theme.arrayOfThemes[9] = themeType;
                            //ui
                            theme4.text = string.Format("4  {0}", themeType);
                            break;
                        case 4:
                            story.theme.arrayOfThemes[10] = themeType;
                            //ui
                            theme5.text = string.Format("5  {0}", themeType);
                            break;
                        default: Debug.LogWarningFormat("Unrecognised index \"{0}\" for listOfThemes", i); break;
                    }
                }
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
        if (story != null)
        {
            //update data
            story.tag = adventureTag.text;
            story.date = adventureDate.text;
            story.notes = adventureNotes.text;
            //Add story to dictionary if not already present
            if (ToolManager.i.toolDataScript.CheckStoryExists(story) == false)
            {
                ToolManager.i.toolDataScript.AddStory(story);
                Debug.LogFormat("[Tol] AdventureUI.cs -> SaveAdventure: Story \"{0}\" saved to dictionary{1}", story.tag, "\n");
            }
        }
        else { Debug.LogError("Invalid story (Null)"); }
    }

    /// <summary>
    /// Write dictOfStories to file
    /// </summary>
    private void SaveToFile()
    {
        ToolManager.i.toolFileScript.WriteToolData();
        ToolManager.i.toolFileScript.SaveToolsToFile();
        Debug.LogFormat("[Tol] AdventureUI.cs -> SaveToFile: dictOfStories SAVED TO FILE{0}", "\n");
    }


    //new scripts above here
}

#endif
