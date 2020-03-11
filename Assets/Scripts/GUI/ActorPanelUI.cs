using gameAPI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Actor Display at bottom of UI
/// </summary>
public class ActorPanelUI : MonoBehaviour
{
    //Actor display at bottom
    public GameObject ActorGroup;
    public GameObject Actor0;
    public GameObject Actor1;
    public GameObject Actor2;
    public GameObject Actor3;
    public GameObject ActorPlayer;

    public Image picture0;
    public Image picture1;
    public Image picture2;
    public Image picture3;
    public Image picturePlayer;
    public Image spriteStars;

    public TextMeshProUGUI type0;
    public TextMeshProUGUI type1;
    public TextMeshProUGUI type2;
    public TextMeshProUGUI type3;
    public TextMeshProUGUI playerStressed;
    public TextMeshProUGUI moodStars;
    public TextMeshProUGUI compatibility0;
    public TextMeshProUGUI compatibility1;
    public TextMeshProUGUI compatibility2;
    public TextMeshProUGUI compatibility3;


    public Image renownCircle0;
    public Image renownCircle1;
    public Image renownCircle2;
    public Image renownCircle3;
    public Image renownCirclePlayer;

    public TextMeshProUGUI renownText0;
    public TextMeshProUGUI renownText1;
    public TextMeshProUGUI renownText2;
    public TextMeshProUGUI renownText3;
    public TextMeshProUGUI renownTextPlayer;

    private CanvasGroup canvas0;
    private CanvasGroup canvas1;
    private CanvasGroup canvas2;
    private CanvasGroup canvas3;
    private CanvasGroup canvasPlayer;

    private Sprite mood0;
    private Sprite mood1;
    private Sprite mood2;
    private Sprite mood3;

    private GenericTooltipUI playerMoodTooltip;
    private GenericTooltipUI playerStressedTooltip;

    public bool isRenownUI;                                                     //gives status of Info UI display (true -> Shows RENOWN, false -> COMPATIBILITY)

    private Image[] arrayOfRenownCircles = new Image[4];                        //used for more efficient access, populated in initialise. Actors only, index is actorSlotID (0 to 3)
    private TextMeshProUGUI[] arrayOfCompatibility = new TextMeshProUGUI[4];     //compatibility
    private GenericTooltipUI[] arrayOfCompatibilityTooltips = new GenericTooltipUI[4];    //compatibility tooltips

    //fast access
    private Sprite vacantAuthorityActor;
    private Sprite vacantResistanceActor;
    private int starFontSize = -1;

    private static ActorPanelUI actorPanelUI;




    /// <summary>
    /// provide a static reference to ActorPanelUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ActorPanelUI Instance()
    {
        if (!actorPanelUI)
        {
            actorPanelUI = FindObjectOfType(typeof(ActorPanelUI)) as ActorPanelUI;
            if (!actorPanelUI)
            { Debug.LogError("There needs to be one active actorPanelUI script on a GameObject in your scene"); }
        }
        return actorPanelUI;
    }

    public void Awake()
    {

        picture0.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 0;
        picture1.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 1;
        picture2.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 2;
        picture3.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 3;
        //Canvas Group references
        canvas0 = Actor0.GetComponent<CanvasGroup>();
        canvas1 = Actor1.GetComponent<CanvasGroup>();
        canvas2 = Actor2.GetComponent<CanvasGroup>();
        canvas3 = Actor3.GetComponent<CanvasGroup>();
        canvasPlayer = ActorPlayer.GetComponent<CanvasGroup>();
        //mood help
        playerMoodTooltip = moodStars.GetComponent<GenericTooltipUI>();
        playerStressedTooltip = playerStressed.GetComponent<GenericTooltipUI>();
        Debug.Assert(playerMoodTooltip != null, "Invalid playerMoodHelp (Null)");
        Debug.Assert(playerStressedTooltip != null, "Invalid playerStressedHelp (Null)");
        //compatibility
        Debug.Assert(compatibility0 != null, "Invalid compatibility0 (Null)");
        Debug.Assert(compatibility1 != null, "Invalid compatibility1 (Null)");
        Debug.Assert(compatibility2 != null, "Invalid compatibility2 (Null)");
        Debug.Assert(compatibility3 != null, "Invalid compatibility3 (Null)");
    }

    /// <summary>
    /// Not called for LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                SubInitialiseAll();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                SubInitialiseAll();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseAll();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    #region Initialisation Submethods

    #region SubInitialiseEvents
    /// <summary>
    /// subMethod events
    /// </summary>
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "ActorPanelUI");
        EventManager.instance.AddListener(EventType.ActorInfo, OnEvent, "ActorPanelUI");
    }
    #endregion

    #region SubInitialiseFastAccess
    /// <summary>
    /// submethod fast Access
    /// </summary>
    private void SubInitialiseFastAccess()
    {
        //fast access
        vacantAuthorityActor = GameManager.instance.guiScript.vacantActorSprite;
        vacantResistanceActor = GameManager.instance.guiScript.vacantActorSprite;
        starFontSize = GameManager.instance.guiScript.actorFontSize;
        Debug.Assert(vacantAuthorityActor != null, "Invalid vacantAuthorityActor (Null)");
        Debug.Assert(vacantResistanceActor != null, "Invalid vacantResistanceActor (Null)");
        Debug.Assert(starFontSize > -1, "Invalid starFontSize (-1)");
        //mood stars
        mood0 = GameManager.instance.guiScript.moodStar0;
        mood1 = GameManager.instance.guiScript.moodStar1;
        mood2 = GameManager.instance.guiScript.moodStar2;
        mood3 = GameManager.instance.guiScript.moodStar3;
        Debug.Assert(mood0 != null, "Invalid mood0 (Null)");
        Debug.Assert(mood1 != null, "Invalid mood1 (Null)");
        Debug.Assert(mood2 != null, "Invalid mood2 (Null)");
        Debug.Assert(mood3 != null, "Invalid mood3 (Null)");
    }
    #endregion

    #region SubInitialiseSessionStart
    /// <summary>
    /// subMethod session start
    /// </summary>
    private void SubInitialiseSessionStart()
    {
        //assign actorSlotID's to all Actor components
        Actor0.GetComponent<ActorHighlightUI>().actorSlotID = 0;
        Actor1.GetComponent<ActorHighlightUI>().actorSlotID = 1;
        Actor2.GetComponent<ActorHighlightUI>().actorSlotID = 2;
        Actor3.GetComponent<ActorHighlightUI>().actorSlotID = 3;

        picture0.GetComponent<ActorClickUI>().actorSlotID = 0;
        picture1.GetComponent<ActorClickUI>().actorSlotID = 1;
        picture2.GetComponent<ActorClickUI>().actorSlotID = 2;
        picture3.GetComponent<ActorClickUI>().actorSlotID = 3;

        type0.GetComponent<ActorTooltipUI>().actorSlotID = 0;
        type1.GetComponent<ActorTooltipUI>().actorSlotID = 1;
        type2.GetComponent<ActorTooltipUI>().actorSlotID = 2;
        type3.GetComponent<ActorTooltipUI>().actorSlotID = 3;

        type0.GetComponent<ActorTooltipUI>().isText = true;
        type1.GetComponent<ActorTooltipUI>().isText = true;
        type2.GetComponent<ActorTooltipUI>().isText = true;
        type3.GetComponent<ActorTooltipUI>().isText = true;

        picture0.GetComponent<ActorTooltipUI>().actorSlotID = 0;
        picture1.GetComponent<ActorTooltipUI>().actorSlotID = 1;
        picture2.GetComponent<ActorTooltipUI>().actorSlotID = 2;
        picture3.GetComponent<ActorTooltipUI>().actorSlotID = 3;

        picture0.GetComponent<ActorTooltipUI>().isText = false;
        picture1.GetComponent<ActorTooltipUI>().isText = false;
        picture2.GetComponent<ActorTooltipUI>().isText = false;
        picture3.GetComponent<ActorTooltipUI>().isText = false;

        //populate lists
        List<TextMeshProUGUI> listOfActorTypes = GameManager.instance.dataScript.GetListOfActorTypes();
        if (listOfActorTypes != null)
        {
            listOfActorTypes.Add(type0);
            listOfActorTypes.Add(type1);
            listOfActorTypes.Add(type2);
            listOfActorTypes.Add(type3);
        }
        else { Debug.LogError("Invalid listOfActorTypes (Null)"); }
        List<Image> listOfActorPortraits = GameManager.instance.dataScript.GetListOfActorPortraits();
        if (listOfActorPortraits != null)
        {
            listOfActorPortraits.Add(picture0);
            listOfActorPortraits.Add(picture1);
            listOfActorPortraits.Add(picture2);
            listOfActorPortraits.Add(picture3);
        }
        else { Debug.LogError("Invalid listOfActorPortraits (Null)"); }
        //player Stressed
        playerStressed.text = "STRESSED";
        if (GameManager.instance.playerScript.sprite != null)
        { picturePlayer.sprite = GameManager.instance.playerScript.sprite; }
        else { picturePlayer.sprite = GameManager.instance.guiScript.errorSprite; }
        //initialse arrayOfRenownCircles
        arrayOfRenownCircles[0] = renownCircle0;
        arrayOfRenownCircles[1] = renownCircle1;
        arrayOfRenownCircles[2] = renownCircle2;
        arrayOfRenownCircles[3] = renownCircle3;
        //initialise arrayOfCompatibility
        arrayOfCompatibility[0] = compatibility0;
        arrayOfCompatibility[1] = compatibility1;
        arrayOfCompatibility[2] = compatibility2;
        arrayOfCompatibility[3] = compatibility3;
        //array components and assignments
        for (int i = 0; i < 4; i++)
        {
            Debug.AssertFormat(arrayOfRenownCircles[i] != null, "Invalid arrayOfRenownCircles[{0}]", i);
            if (arrayOfCompatibility[i] != null)
            {
                //font size
                arrayOfCompatibility[i].fontSize = starFontSize;
                //tooltips
                arrayOfCompatibilityTooltips[i] = arrayOfCompatibility[i].GetComponent<GenericTooltipUI>();
                if (arrayOfCompatibilityTooltips[i] == null)
                { Debug.LogErrorFormat("Invalid arrayOfCompatibilityTooltips[{0}] (Null)", i); }
            }
            else { Debug.LogErrorFormat("ActorPanelUI.cs -> SubInitialiseSesssionStart: Invalid arrayOfCompatibility[{0}]", i); }
        }
        //player mood stars
        if (moodStars != null)
        { moodStars.fontSize = starFontSize; }
        else { Debug.LogError("Invalid moodStars (Null)"); }
        //initialise tooltips
        InitialiseTooltips();
    }
    #endregion

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
        //renown UI (default true)
        if (GameManager.instance.optionScript.showRenown == true)
        { SetActorInfoUI(true); }
        else { SetActorInfoUI(false); }
        //initialise starting line up
        UpdateActorPanel();
    }
    #endregion

    #endregion

    /// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeSide:
                UpdateActorPanel();
                break;
            case EventType.ActorInfo:
                SetActorInfoUI(!isRenownUI);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// places actor data (type and sprite) into GUI elements via lists (player is static so no need to update)
    /// </summary>
    public void UpdateActorPanel()
    {
        int numOfActors = GameManager.instance.actorScript.maxNumOfOnMapActors;
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(side);
        List<TextMeshProUGUI> listOfActorTypes = GameManager.instance.dataScript.GetListOfActorTypes();
        List<Image> listOfActorPortraits = GameManager.instance.dataScript.GetListOfActorPortraits();
        if (listOfActorTypes != null)
        {
            if (listOfActorPortraits != null)
            {
                if (arrayOfActors != null)
                {
                    if (arrayOfActors.Length == numOfActors)
                    {
                        //loop actors
                        for (int index = 0; index < numOfActors; index++)
                        {
                            //check if actorSlotID has an actor first
                            if (GameManager.instance.dataScript.CheckActorSlotStatus(index, side) == true)
                            {
                                listOfActorTypes[index].text = arrayOfActors[index].arc.name;
                                listOfActorPortraits[index].sprite = arrayOfActors[index].sprite;
                            }
                            else
                            {
                                //vacant position
                                listOfActorTypes[index].text = "VACANT";
                                switch (side.level)
                                {
                                    case 1: listOfActorPortraits[index].sprite = vacantAuthorityActor; break;
                                    case 2: listOfActorPortraits[index].sprite = vacantResistanceActor; break;
                                    default:
                                        Debug.LogError(string.Format("Invalid side.level \"{0}\" {1}", side.name, side.level));
                                        break;
                                }
                            }
                        }
                        //Update Renown/Compatibiliyt indicators (switches off for Vacant actors)
                        if (GameManager.instance.optionScript.showRenown == true)
                        { SetActorInfoUI(true); }
                        else { SetActorInfoUI(false); }
                    }
                    else { Debug.LogWarning("Invalid number of Actors (listOfActors doesn't correspond to numOfActors). Texts not updated."); }
                }
                else { Debug.LogError("Invalid listOfActors (Null)"); }
            }
            else { Debug.LogError("Invalid listOfActorPortraits (Null)"); }
        }
        else { Debug.LogError("Invalid listOfActorTypes (Null)"); }
    }

    /// <summary>
    /// changes the alpha of an actor sprite and text
    /// </summary>
    /// <param name="actorSlotID"></param>
    /// <param name="alpha"></param>
    public void UpdateActorAlpha(int actorSlotID, float alpha)
    {
        Debug.Assert(actorSlotID > -1 && actorSlotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid slotID");
        //check actor present in slot, eg. not vacant
        if (GameManager.instance.dataScript.CheckActorSlotStatus(actorSlotID, GameManager.instance.sideScript.PlayerSide) == true)
        {
            switch (actorSlotID)
            {
                case 0:
                    canvas0.alpha = alpha;
                    break;
                case 1:
                    canvas1.alpha = alpha;
                    break;
                case 2:
                    canvas2.alpha = alpha;
                    break;
                case 3:
                    canvas3.alpha = alpha;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid actorSlotID {0}", actorSlotID));
                    break;
            }
        }
    }

    /// <summary>
    /// Sets actor alpha to active for all onMap slotID's
    /// </summary>
    public void SetActorsAlphaActive()
    {
        float activeAlpha = GameManager.instance.guiScript.alphaActive;
        //set all actor alpha's to active (may still be set to inactive from previous level)
        for (int i = 0; i < GameManager.instance.actorScript.maxNumOfOnMapActors; i++)
        { UpdateActorAlpha(i, activeAlpha); }
    }

    /// <summary>
    /// changes the alpha of the player sprite and text
    /// </summary>
    /// <param name="alpha"></param>
    public void UpdatePlayerAlpha(float alpha)
    { canvasPlayer.alpha = alpha; }

    /// <summary>
    /// Initialise Actor and Player related tooltips
    /// NOTE: Using custom Generic Tooltips here rather than  Help Tooltips due to sequencing issues at startup
    /// </summary>
    public void InitialiseTooltips()
    {
        //player mood UI
        playerMoodTooltip.tooltipHeader = "Mood";
        playerMoodTooltip.tooltipMain = string.Format("{0}  {1}", GameManager.instance.guiScript.motivationIcon, GameManager.instance.colourScript.GetFormattedString("0 to 3 Stars", ColourType.neutralText));
        string details = string.Format("You will become STRESSED if your mood goes below zero");
        playerMoodTooltip.tooltipDetails = GameManager.instance.colourScript.GetFormattedString(details, ColourType.moccasinText);
        playerMoodTooltip.y_offset = 100;
        //player stressed UI
        playerStressedTooltip.tooltipHeader = GameManager.instance.colourScript.GetFormattedString("Stressed", ColourType.badText);
        playerStressedTooltip.tooltipMain = GameManager.instance.colourScript.GetFormattedString("Mood falls below Zero", ColourType.neutralText);
        details = "You can take Stress Leave or Lie Low (Right Click Player pic) to remove. You run the risk of suffering a BREAKDOWN";
        playerStressedTooltip.tooltipDetails = GameManager.instance.colourScript.GetFormattedString(details, ColourType.moccasinText);
        playerStressedTooltip.y_offset = 100;
        //
        // - - - actor compatibility
        //
        string tooltipHeader = string.Format("{0} <size=120%>{1}</size>{2}with Player", 
            GameManager.instance.guiScript.compatibilityIcon, 
            GameManager.instance.colourScript.GetFormattedString("Compatibility", ColourType.moccasinText), "\n");
        string tooltipMain = string.Format("{0} indicate a Positive Relationship{1}{2} a Negative one. The {3} of Stars indicate the {4} of the Relationship",
                    GameManager.instance.colourScript.GetFormattedString(GameManager.instance.guiScript.starIconGood, ColourType.goodText), "\n",
                    GameManager.instance.colourScript.GetFormattedString(GameManager.instance.guiScript.starIconBad, ColourType.badText),
                    GameManager.instance.colourScript.GetFormattedString("number", ColourType.salmonText),
                    GameManager.instance.colourScript.GetFormattedString("Intensity", ColourType.salmonText));
        /*string tooltipDetails = string.Format("A subordinate with {0} has a chance of {1} any {2} {3} Motivational outcomes{4}{5}With a {6} they may {7} {8} outcomes",
                    GameManager.instance.colourScript.GetFormattedString("Positive Relationship", ColourType.goodText), 
                    GameManager.instance.colourScript.GetFormattedString("ignoring", ColourType.salmonText), 
                    GameManager.instance.colourScript.GetFormattedString("BAD", ColourType.salmonText), 
                    GameManager.instance.guiScript.motivationIcon, "\n", "\n",
                    GameManager.instance.colourScript.GetFormattedString("Negative Relationship", ColourType.badText),
                    GameManager.instance.colourScript.GetFormattedString("ignore GOOD", ColourType.salmonText),
                    GameManager.instance.guiScript.motivationIcon);*/
        string tooltipDetails = string.Format("A subordinate {0} ignore {1} ({2}) or {3} ({4}) Motivational outcomes",
            GameManager.instance.colourScript.GetFormattedString("may", ColourType.salmonText),
            GameManager.instance.colourScript.GetFormattedString("GOOD", ColourType.salmonText),
            GameManager.instance.colourScript.GetFormattedString(GameManager.instance.guiScript.starIconBad, ColourType.badText),
            GameManager.instance.colourScript.GetFormattedString("BAD", ColourType.salmonText),
            GameManager.instance.colourScript.GetFormattedString(GameManager.instance.guiScript.starIconGood, ColourType.goodText));
        for (int i = 0; i < 4; i++)
        {
            GenericTooltipUI tooltip = arrayOfCompatibilityTooltips[i];
            if (tooltip != null)
            {
                tooltip.tooltipHeader = tooltipHeader;
                tooltip.tooltipMain = tooltipMain;
                tooltip.tooltipDetails = tooltipDetails;
                tooltip.tooltipType = GenericTooltipType.ActorInfo;
                tooltip.x_offset = 100;
                tooltip.y_offset = 20;
            }
            else { Debug.LogWarningFormat("Invalid arrayOfCompatibilityTooltip[{0}] (Null)", i); }
        }
    }

    /// <summary>
    /// changes the sprite for the  moodPanel to reflect player mood
    /// </summary>
    /// <param name="mood"></param>
    public void SetPlayerMoodUI(int mood, bool isStressed = false)
    {
        //sprite based mood stars
        /*if (isStressed == false)
        {
            playerStressed.gameObject.SetActive(false);
            moodStars.gameObject.SetActive(true);
            switch (mood)
            {
                case 3: moodStars.sprite = mood3; break;
                case 2: moodStars.sprite = mood2; break;
                case 1: moodStars.sprite = mood1; break;
                case 0: moodStars.sprite = mood0; break;
                default: Debug.LogWarningFormat("Unrecognised player mood \"{0}\"", mood); break;
            }
        }
        else
        {
            //stressed
            playerStressed.gameObject.SetActive(true);
            moodStars.gameObject.SetActive(false);
        }*/


        //no longer using sprites, swapped to fontAwesome stars
        spriteStars.gameObject.SetActive(false);
        if (isStressed == true)
        {
            playerStressed.gameObject.SetActive(true);
            moodStars.gameObject.SetActive(false);
        }
        else
        {
            playerStressed.gameObject.SetActive(false);
            moodStars.gameObject.SetActive(true);
            moodStars.text = GameManager.instance.guiScript.GetMotivationStars(mood);
        }
    }

    /// <summary>
    /// toggles actor renown/compatibility display. If 'showRenown' true RENOWN, false COMPATIBILITY and no actor present in slot (Vacant) will auto switch off
    /// </summary>
    /// <param name="showRenown"></param>
    public void SetActorInfoUI(bool showRenown)
    {
        //switch off any relevant tooltip (renown/compatibility)
        GameManager.instance.tooltipGenericScript.CloseTooltip("ActorPanelUI", GenericTooltipType.ActorInfo);
        //toggle
        GameManager.instance.optionScript.showRenown = showRenown;
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        for (int index = 0; index < arrayOfRenownCircles.Length; index++)
        {
            if (showRenown == true)
            {
                //display RENOWN
                arrayOfCompatibility[index].gameObject.SetActive(false);
                if (GameManager.instance.dataScript.CheckActorSlotStatus(index, side) == true)
                { arrayOfRenownCircles[index].gameObject.SetActive(showRenown); }
                else
                {
                    arrayOfRenownCircles[index].gameObject.SetActive(false);
                    UpdateActorRenownUI(index, 0);
                }
            }
            else
            {
                //display COMPATIBILITY
                arrayOfRenownCircles[index].gameObject.SetActive(false);
                if (GameManager.instance.dataScript.CheckActorSlotStatus(index, side) == true)
                { arrayOfCompatibility[index].gameObject.SetActive(true); }
                else
                {
                    arrayOfCompatibility[index].gameObject.SetActive(false);
                    UpdateActorCompatibilityUI(index, 0);
                }
            }
        }

        /*//player is never vacant -> EDIT player renown shown regardless
        renownCirclePlayer.gameObject.SetActive(showRenown);*/

        //update flag
        isRenownUI = showRenown;
        //logging
        Debug.LogFormat("[UI] ActorPanelUI.cs -> ToggleActorInfoUI: {0}{1}", showRenown == true ? "RENOWN" : "COMPATIBILITY", "\n");
    }

    /// <summary>
    /// returns true if actor Info UI shows RENOWN, false if COMPATIBILITY
    /// </summary>
    /// <returns></returns>
    public bool CheckInfoUIStatus()
    { return isRenownUI; }

    /// <summary>
    /// updates actor slot renown UI with current renown
    /// </summary>
    /// <param name="actorSlotID"></param>
    /// <param name="renown"></param>
    public void UpdateActorRenownUI(int actorSlotID, int renown)
    {
        Debug.Assert(actorSlotID > -1 && actorSlotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid actorSlotID (< 0 or >= maxNumOfOnMapActors");
        Debug.Assert(renown > -1, "Invalid renown (< 0)");
        switch (actorSlotID)
        {
            case 0: renownText0.text = Convert.ToString(renown); break;
            case 1: renownText1.text = Convert.ToString(renown); break;
            case 2: renownText2.text = Convert.ToString(renown); break;
            case 3: renownText3.text = Convert.ToString(renown); break;
            default: Debug.LogWarningFormat("Invalid actorSlotID {0}", actorSlotID); break;
        }
    }

    /// <summary>
    /// updates actor slot compatibility UI with compatibility stars
    /// </summary>
    /// <param name="actorSlotID"></param>
    /// <param name="compatibility"></param>
    public void UpdateActorCompatibilityUI(int actorSlotID, int compatibility)
    {
        Debug.Assert(actorSlotID > -1 && actorSlotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid actorSlotID (< 0 or >= maxNumOfOnMapActors");
        Debug.Assert(compatibility >= -3 && compatibility <= 3, "Invalid compatibility (< -3 or > +3)");
        switch (actorSlotID)
        {
            case 0: compatibility0.text = GameManager.instance.guiScript.GetCompatibilityStars(compatibility); break;
            case 1: compatibility1.text = GameManager.instance.guiScript.GetCompatibilityStars(compatibility); break;
            case 2: compatibility2.text = GameManager.instance.guiScript.GetCompatibilityStars(compatibility); break;
            case 3: compatibility3.text = GameManager.instance.guiScript.GetCompatibilityStars(compatibility); break;
            default: Debug.LogWarningFormat("Invalid actorSlotID {0}", actorSlotID); break;
        }
    }

    /// <summary>
    /// updates renown UI with player's current renown
    /// </summary>
    /// <param name="renown"></param>
    public void UpdatePlayerRenownUI(int renown)
    {
        renownTextPlayer.text = Convert.ToString(renown);
    }


    //new methods above here
}
