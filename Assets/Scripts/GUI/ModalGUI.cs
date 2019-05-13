using System;
using System.Collections;
using System.Collections.Generic;
using gameAPI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles all modal related gui items and backgrounds (modal masks are here 'cause they can't go in a prefab)
/// </summary>
public class ModalGUI : MonoBehaviour
{
    [Header("Modal Masks")]
    [Tooltip("Panel (1 of 2) that blocks UI input when first level gamestate.ModalUI in play. Greyed, full screen and Does NOT block raycast")]
    public GameObject modal0;
    [Tooltip("Panel (2 of 2) that blocks UI input when first level gamestate.ModalUI in play. Clear, partial screen and BLOCKS raycast")]
    public GameObject modal1;
    [Tooltip("Panel (1 of 1) that blocks UI input when second level gamestate.ModalUI in play. Clear, partial screen and BLOCKS raycast")]
    public GameObject modal2;

    [Header("Backgrounds")]
    [Tooltip("Full screen background for start")]
    public Image backgroundStart;
    [Tooltip("Create a New Game background")]
    public Image backgroundNewGame;
    [Tooltip("New Game Options selected background")]
    public Image backgroundNewGameOptions;
    [Tooltip("Option screen background")]
    public Image backgroundOptions;
    [Tooltip("End Level (summary) background")]
    public Image backgroundEndLevel;
    [Tooltip("MetaGame (between levels) background")]
    public Image backgroundMetaGame;
    [Tooltip("Commence new Campaign")]
    public Image backgroundNewCampaign;
    [Tooltip("End of Campaign (no more scenarios available)")]
    public Image backgroundEndCampaign;
    [Tooltip("Resume or Load a save game")]
    public Image backgroundLoadGame;
    [Tooltip("Save Game")]
    public Image backgroundSaveGame;

    
    private int modalLevel;                                                 //level of modalUI, '0' if none, '1' if first level, '2' if second (eg. outcome window over an inventory window)

    private static ModalGUI modalGUI;

    /// <summary>
    /// Static instance so the ModalGUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalGUI Instance()
    {
        if (!modalGUI)
        {
            modalGUI = FindObjectOfType(typeof(ModalGUI)) as ModalGUI;
            if (!modalGUI)
            { Debug.LogError("There needs to be one active ModalGUI script on a GameObject in your scene"); }
        }
        return modalGUI;
    }

    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        Debug.Assert(modal0 != null, "Invalid modal0 (Null)");
        Debug.Assert(modal1 != null, "Invalid modal (Null)");
        Debug.Assert(modal2 != null, "Invalid modal2 (Null)");
        //backgrounds
        Debug.Assert(backgroundStart != null, "Invalid backgroundStart (Null)");
        Debug.Assert(backgroundNewGame != null, "Invalid backgroundNewGame (Null)");
        Debug.Assert(backgroundNewGameOptions != null, "Invalid backgroundNewGameOptions (Null)");
        Debug.Assert(backgroundOptions != null, "Invalid backgroundOptions (Null)");
        Debug.Assert(backgroundEndLevel != null, "Invalid backgroundEndLevel (Null)");
        Debug.Assert(backgroundMetaGame != null, "Invalid backgroundMetaGame (Null)");
        Debug.Assert(backgroundNewCampaign != null, "Invalid backgroundNewCampaign (Null)");
        Debug.Assert(backgroundEndCampaign != null, "Invalid backgroundEndCampaign (Null)");
        Debug.Assert(backgroundLoadGame != null, "Invalid backgroundLoadGame (Null)");
        Debug.Assert(backgroundSaveGame != null, "Invalid backgroundSaveGame (Null)");
        //disable all backgrounds
        CloseBackgrounds();
    }

    //
    // - - - Modal levels - - -
    //

    /// <summary>
    /// Sets modal masks which are (for base level) a combination of two masks to provide an all over greyed background and a partial blocking of mouse input to UI elements
    /// level refers to modal level (can have multiple, like layers, separating UI components)
    /// </summary>
    public void SetModalMasks(bool isBlocked, int level)
    {
        switch (level)
        {
            case 1:
                if (isBlocked == true)
                {
                    //Block
                    modal0.SetActive(true);
                    modal1.SetActive(true);
                    //if already 2, don't change (eg. outcome window open on top of the InfoApp)
                    if (modalLevel != 2)
                    { modalLevel = 1; }
                }
                else
                {
                    //Unblock
                    modal0.SetActive(false);
                    modal1.SetActive(false);
                    modalLevel = 0;
                }
                break;
            case 2:
                if (isBlocked == true)
                {
                    //Block
                    modal2.SetActive(true);
                    modalLevel = 2;
                }
                else
                {
                    //Unblock
                    modal2.SetActive(false);
                    modalLevel = 1;
                }
                break;
            default:
                Debug.LogError(string.Format("Invalid modalLevel {0}", modalLevel));
                break;
        }
        Debug.LogFormat("[Inp] ModalGUI.cs -> SetModalMasks: modal0 {0}, modal1 {1}, modal2 {2}, level {3}{4}", modal0.activeSelf, modal1.activeSelf, modal2.activeSelf, modalLevel, "\n");
    }


    public int CheckModalLevel()
    { return modalLevel; }

    //
    // - - - Backgrounds - - -
    //

    /// <summary>
    /// Display background
    /// </summary>
    /// <param name="background"></param>
    public void SetBackground(Background background)
    {
        switch (background)
        {
            case Background.None:
                //default option, needs to be here
                break;
            case Background.Start:
                backgroundStart.gameObject.SetActive(true);
                break;
            case Background.NewGame:
                backgroundNewGame.gameObject.SetActive(true);
                break;
            case Background.NewGameOptions:
                backgroundNewGameOptions.gameObject.SetActive(true);
                break;
            case Background.Options:
                backgroundOptions.gameObject.SetActive(true);
                break;
            case Background.EndLevel:
                backgroundEndLevel.gameObject.SetActive(true);
                break;
            case Background.MetaGame:
                backgroundMetaGame.gameObject.SetActive(true);
                break;
            case Background.NewCampaign:
                backgroundNewCampaign.gameObject.SetActive(true);
                break;
            case Background.EndCampaign:
                backgroundEndCampaign.gameObject.SetActive(true);
                break;
            case Background.LoadGame:
                backgroundLoadGame.gameObject.SetActive(true);
                break;
            case Background.SaveGame:
                backgroundSaveGame.gameObject.SetActive(true);
                break;
            default:
                Debug.LogErrorFormat("Unrecognised background \"{0}\"", background);
                break;
        }
    }


    /// <summary>
    /// closes all backgrounds except the specified exclusion (default no exclusion, disable all)
    /// </summary>
    /// <param name="excludeBackground"></param>
    public void CloseBackgrounds(Background excludeBackground = Background.None)
    {
        foreach(Background background in Enum.GetValues(typeof(Background)))
        {
            if (background != excludeBackground)
            {
                switch (background)
                {
                    case Background.Start:
                        backgroundStart.gameObject.SetActive(false);
                        break;
                    case Background.NewGame:
                        backgroundNewGame.gameObject.SetActive(false);
                        break;
                    case Background.NewGameOptions:
                        backgroundNewGameOptions.gameObject.SetActive(false);
                        break;
                    case Background.Options:
                        backgroundOptions.gameObject.SetActive(false);
                        break;
                    case Background.EndLevel:
                        backgroundEndLevel.gameObject.SetActive(false);
                        break;
                    case Background.MetaGame:
                        backgroundMetaGame.gameObject.SetActive(false);
                        break;
                    case Background.NewCampaign:
                        backgroundNewCampaign.gameObject.SetActive(false);
                        break;
                    case Background.EndCampaign:
                        backgroundEndCampaign.gameObject.SetActive(false);
                        break;
                    case Background.LoadGame:
                        backgroundLoadGame.gameObject.SetActive(false);
                        break;
                    case Background.SaveGame:
                        backgroundSaveGame.gameObject.SetActive(false);
                        break;
                    case Background.None:
                        //do nothing
                        break;
                    default:
                        Debug.LogErrorFormat("Unrecognised background \"{0}\"", background);
                        break;
                }
            }
        }
    }


    //new methods above here
}
