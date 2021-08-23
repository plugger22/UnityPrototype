using gameAPI;
using packageAPI;
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


    public Image powerCircle0;
    public Image powerCircle1;
    public Image powerCircle2;
    public Image powerCircle3;
    public Image powerCirclePlayer;

    public TextMeshProUGUI powerText0;
    public TextMeshProUGUI powerText1;
    public TextMeshProUGUI powerText2;
    public TextMeshProUGUI powerText3;
    public TextMeshProUGUI powerTextPlayer;

    public Image statusPanel0;
    public Image statusPanel1;
    public Image statusPanel2;
    public Image statusPanel3;

    public TextMeshProUGUI statusText0;
    public TextMeshProUGUI statusText1;
    public TextMeshProUGUI statusText2;
    public TextMeshProUGUI statusText3;

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
    private GenericTooltipUI actor0TypeTooltip;
    private GenericTooltipUI actor1TypeTooltip;
    private GenericTooltipUI actor2TypeTooltip;
    private GenericTooltipUI actor3TypeTooltip;

    private bool isPowerUI;                                                     //gives status of Info UI display (true -> Shows Power, false -> COMPATIBILITY)

    private Image[] arrayOfPowerCircles = new Image[4];                        //used for more efficient access, populated in initialise. Actors only, index is actorSlotID (0 to 3)
    private TextMeshProUGUI[] arrayOfCompatibility = new TextMeshProUGUI[4];     //compatibility
    private GenericTooltipUI[] arrayOfCompatibilityTooltips = new GenericTooltipUI[4];    //compatibility tooltips
    private GenericTooltipUI[] arrayOfTypeTooltips = new GenericTooltipUI[4]; //actorArc tooltips
    private Image[] arrayOfStatusPanels = new Image[4];
    private TextMeshProUGUI[] arrayOfStatusTexts = new TextMeshProUGUI[4];

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

        /*picture0.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 0;
        picture1.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 1;
        picture2.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 2;
        picture3.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 3;*/

        //Canvas Group references
        canvas0 = Actor0.GetComponent<CanvasGroup>();
        canvas1 = Actor1.GetComponent<CanvasGroup>();
        canvas2 = Actor2.GetComponent<CanvasGroup>();
        canvas3 = Actor3.GetComponent<CanvasGroup>();
        canvasPlayer = ActorPlayer.GetComponent<CanvasGroup>();
        //actor arc type help
        actor0TypeTooltip = type0.GetComponent<GenericTooltipUI>();
        actor1TypeTooltip = type1.GetComponent<GenericTooltipUI>();
        actor2TypeTooltip = type2.GetComponent<GenericTooltipUI>();
        actor3TypeTooltip = type3.GetComponent<GenericTooltipUI>();
        Debug.Assert(actor0TypeTooltip != null, "Invalid actor0TypeTooltip (Null)");
        Debug.Assert(actor1TypeTooltip != null, "Invalid actor1TypeTooltip (Null)");
        Debug.Assert(actor2TypeTooltip != null, "Invalid actor2TypeTooltip (Null)");
        Debug.Assert(actor3TypeTooltip != null, "Invalid actor3TypeTooltip (Null)");
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
        //status (friends and enemies)
        Debug.Assert(statusPanel0 != null, "Invalid statusPanel0 (Null)");
        Debug.Assert(statusPanel1 != null, "Invalid statusPanel1 (Null)");
        Debug.Assert(statusPanel2 != null, "Invalid statusPanel2 (Null)");
        Debug.Assert(statusPanel3 != null, "Invalid statusPanel3 (Null)");
        Debug.Assert(statusText0 != null, "Invalid statusText0 (Null)");
        Debug.Assert(statusText1 != null, "Invalid statusText1 (Null)");
        Debug.Assert(statusText2 != null, "Invalid statusText2 (Null)");
        Debug.Assert(statusText3 != null, "Invalid statusText3 (Null)");

    }

    /// <summary>
    /// Not called for LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
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
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
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
        EventManager.i.AddListener(EventType.ChangeSide, OnEvent, "ActorPanelUI");
        EventManager.i.AddListener(EventType.ActorInfo, OnEvent, "ActorPanelUI");
    }
    #endregion

    #region SubInitialiseFastAccess
    /// <summary>
    /// submethod fast Access
    /// </summary>
    private void SubInitialiseFastAccess()
    {
        //fast access
        vacantAuthorityActor = GameManager.i.spriteScript.vacantActorSprite;
        vacantResistanceActor = GameManager.i.spriteScript.vacantActorSprite;
        starFontSize = GameManager.i.guiScript.actorFontSize;
        Debug.Assert(vacantAuthorityActor != null, "Invalid vacantAuthorityActor (Null)");
        Debug.Assert(vacantResistanceActor != null, "Invalid vacantResistanceActor (Null)");
        Debug.Assert(starFontSize > -1, "Invalid starFontSize (-1)");
        //mood stars
        mood0 = GameManager.i.spriteScript.moodStar0;
        mood1 = GameManager.i.spriteScript.moodStar1;
        mood2 = GameManager.i.spriteScript.moodStar2;
        mood3 = GameManager.i.spriteScript.moodStar3;
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

        picture0.GetComponent<ActorTooltipUI>().actorSlotID = 0;
        picture1.GetComponent<ActorTooltipUI>().actorSlotID = 1;
        picture2.GetComponent<ActorTooltipUI>().actorSlotID = 2;
        picture3.GetComponent<ActorTooltipUI>().actorSlotID = 3;

        picture0.GetComponent<ActorTooltipUI>().isText = false;
        picture1.GetComponent<ActorTooltipUI>().isText = false;
        picture2.GetComponent<ActorTooltipUI>().isText = false;
        picture3.GetComponent<ActorTooltipUI>().isText = false;

        //populate lists
        List<TextMeshProUGUI> listOfActorTypes = GameManager.i.dataScript.GetListOfActorTypes();
        if (listOfActorTypes != null)
        {
            listOfActorTypes.Add(type0);
            listOfActorTypes.Add(type1);
            listOfActorTypes.Add(type2);
            listOfActorTypes.Add(type3);
        }
        else { Debug.LogError("Invalid listOfActorTypes (Null)"); }
        List<Image> listOfActorPortraits = GameManager.i.dataScript.GetListOfActorPortraits();
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
        if (GameManager.i.playerScript.sprite != null)
        { picturePlayer.sprite = GameManager.i.playerScript.sprite; }
        else { picturePlayer.sprite = GameManager.i.spriteScript.errorSprite; }
        //initialse arrayOfPowerCircles
        arrayOfPowerCircles[0] = powerCircle0;
        arrayOfPowerCircles[1] = powerCircle1;
        arrayOfPowerCircles[2] = powerCircle2;
        arrayOfPowerCircles[3] = powerCircle3;
        //initialise arrayOfCompatibility
        arrayOfCompatibility[0] = compatibility0;
        arrayOfCompatibility[1] = compatibility1;
        arrayOfCompatibility[2] = compatibility2;
        arrayOfCompatibility[3] = compatibility3;
        //initialise arrayOfTypeTooltips
        arrayOfTypeTooltips[0] = actor0TypeTooltip;
        arrayOfTypeTooltips[1] = actor1TypeTooltip;
        arrayOfTypeTooltips[2] = actor2TypeTooltip;
        arrayOfTypeTooltips[3] = actor3TypeTooltip;
        //initialise arrayOfStatusPanels
        arrayOfStatusPanels[0] = statusPanel0;
        arrayOfStatusPanels[1] = statusPanel1;
        arrayOfStatusPanels[2] = statusPanel2;
        arrayOfStatusPanels[3] = statusPanel3;
        //initialise arrayOfStatusTexts
        arrayOfStatusTexts[0] = statusText0;
        arrayOfStatusTexts[1] = statusText1;
        arrayOfStatusTexts[2] = statusText2;
        arrayOfStatusTexts[3] = statusText3;
        //array components and assignments
        for (int i = 0; i < 4; i++)
        {
            Debug.AssertFormat(arrayOfPowerCircles[i] != null, "Invalid arrayOfPowerCircles[{0}]", i);
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
        //Power UI (default true)
        if (GameManager.i.optionScript.showPower == true)
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
                SetActorInfoUI(!isPowerUI);
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
        int numOfActors = GameManager.i.actorScript.maxNumOfOnMapActors;
        GlobalSide side = GameManager.i.sideScript.PlayerSide;
        Actor[] arrayOfActors = GameManager.i.dataScript.GetCurrentActorsFixed(side);
        List<TextMeshProUGUI> listOfActorTypes = GameManager.i.dataScript.GetListOfActorTypes();
        List<Image> listOfActorPortraits = GameManager.i.dataScript.GetListOfActorPortraits();
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
                            if (GameManager.i.dataScript.CheckActorSlotStatus(index, side) == true)
                            {
                                listOfActorTypes[index].text = arrayOfActors[index].arc.name;
                                listOfActorPortraits[index].sprite = arrayOfActors[index].sprite;
                                //arc Type tooltip
                                SetActorArcTooltip(arrayOfActors[index], index);
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
                                SetVacantActorArcTooltip(index);
                            }
                        }
                        //Update Power/Compatibility indicators (switches off for Vacant actors)
                        if (GameManager.i.optionScript.showPower == true)
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
    /// Sets tooltip on actorArc type name, eg. "Heavy", explaining what that actorArc does
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="slotID"></param>
    private void SetActorArcTooltip(Actor actor, int slotID)
    {
        if (actor != null)
        {
            arrayOfTypeTooltips[slotID].tooltipHeader = string.Format("<size=115%>{0}</size>",GameManager.Formatt(actor.arc.name, ColourType.neutralText));
            arrayOfTypeTooltips[slotID].tooltipMain = GameManager.Formatt(actor.arc.summary, ColourType.salmonText);
            arrayOfTypeTooltips[slotID].tooltipDetails = actor.arc.details;
            arrayOfTypeTooltips[slotID].y_offset = 150;
        }
        else { Debug.LogErrorFormat("Invalid actor (Null) for slotID {0}", slotID); }
    }

    /// <summary>
    /// Sets tooltip for actor arcs at vacant actor slots
    /// </summary>
    /// <param name="slotID"></param>
    private void SetVacantActorArcTooltip(int slotID)
    {
        arrayOfTypeTooltips[slotID].tooltipHeader = string.Format("<size=115%>{0}</size>", GameManager.Formatt("VACANT", ColourType.neutralText));
        arrayOfTypeTooltips[slotID].tooltipMain = string.Format("{0}", GameManager.Formatt("Activate a subordinate in the Reserves", ColourType.salmonText));
        arrayOfTypeTooltips[slotID].tooltipDetails = "Missing subordinates are a handicap";
        arrayOfTypeTooltips[slotID].y_offset = 150;
    }

    /// <summary>
    /// changes the alpha of an actor sprite and text
    /// </summary>
    /// <param name="actorSlotID"></param>
    /// <param name="alpha"></param>
    public void UpdateActorAlpha(int actorSlotID, float alpha)
    {
        Debug.Assert(actorSlotID > -1 && actorSlotID < GameManager.i.actorScript.maxNumOfOnMapActors, "Invalid slotID");
        //check actor present in slot, eg. not vacant
        if (GameManager.i.dataScript.CheckActorSlotStatus(actorSlotID, GameManager.i.sideScript.PlayerSide) == true)
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
        float alpha = GameManager.i.guiScript.alphaActive;
        //set all actor alpha's to active (may still be set to inactive from previous level)
        for (int i = 0; i < GameManager.i.actorScript.maxNumOfOnMapActors; i++)
        { UpdateActorAlpha(i, alpha); }
    }

    /// <summary>
    /// Sets actor alpha to inactive for all onMap slotID's
    /// </summary>
    public void SetActorsAlphaInactive()
    {
        float alpha = GameManager.i.guiScript.alphaInactive;
        //set all actor alpha's to active (may still be set to inactive from previous level)
        for (int i = 0; i < GameManager.i.actorScript.maxNumOfOnMapActors; i++)
        { UpdateActorAlpha(i, alpha); }
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
        playerMoodTooltip.tooltipMain = string.Format("{0}  {1}", GameManager.i.guiScript.opinionIcon, GameManager.Formatt("0 to 3 Stars", ColourType.neutralText));
        string details = string.Format("You will become STRESSED if your mood goes below zero");
        playerMoodTooltip.tooltipDetails = GameManager.Formatt(details, ColourType.moccasinText);
        playerMoodTooltip.y_offset = 100;
        //player stressed UI
        playerStressedTooltip.tooltipHeader = GameManager.Formatt("Stressed", ColourType.badText);
        playerStressedTooltip.tooltipMain = GameManager.Formatt("Mood falls below Zero", ColourType.neutralText);
        details = "You can take Stress Leave or Lie Low (Right Click Player pic) to remove. You run the risk of suffering a BREAKDOWN";
        playerStressedTooltip.tooltipDetails = GameManager.Formatt(details, ColourType.moccasinText);
        playerStressedTooltip.y_offset = 100;
        //actor compatibility
        GenericTooltipData data = GameManager.i.guiScript.GetCompatibilityTooltip();
        if (data != null)
        {
            for (int i = 0; i < 4; i++)
            {
                GenericTooltipUI tooltip = arrayOfCompatibilityTooltips[i];
                if (tooltip != null)
                {

                    tooltip.tooltipHeader = data.header;
                    tooltip.tooltipMain = data.main;
                    tooltip.tooltipDetails = data.details;
                    tooltip.tooltipType = GenericTooltipType.ActorInfo;
                    tooltip.x_offset = 100;
                    tooltip.y_offset = 20;
                }
                else { Debug.LogWarningFormat("Invalid arrayOfCompatibilityTooltip[{0}] (Null)", i); }
            }
        }
        else { Debug.LogWarningFormat("Invalid GenericTooltipData (Null) for Compatibility tooltip"); }
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
            moodStars.text = GameManager.i.guiScript.GetNormalStars(mood);
        }
    }

    /// <summary>
    /// toggles actor Power/compatibility display. If 'showPower' true POWER, false COMPATIBILITY and no actor present in slot (Vacant) will auto switch off
    /// </summary>
    /// <param name="showPower"></param>
    public void SetActorInfoUI(bool showPower)
    {
        //switch off any relevant tooltip (Power/compatibility)
        GameManager.i.tooltipGenericScript.CloseTooltip("ActorPanelUI", GenericTooltipType.ActorInfo);
        //toggle
        GameManager.i.optionScript.showPower = showPower;
        GlobalSide side = GameManager.i.sideScript.PlayerSide;
        for (int index = 0; index < arrayOfPowerCircles.Length; index++)
        {
            if (showPower == true)
            {
                //display POWER
                arrayOfCompatibility[index].gameObject.SetActive(false);
                if (GameManager.i.dataScript.CheckActorSlotStatus(index, side) == true)
                { arrayOfPowerCircles[index].gameObject.SetActive(showPower); }
                else
                {
                    arrayOfPowerCircles[index].gameObject.SetActive(false);
                    UpdateActorPowerUI(index, 0);
                }
            }
            else
            {
                //display COMPATIBILITY
                arrayOfPowerCircles[index].gameObject.SetActive(false);
                if (GameManager.i.dataScript.CheckActorSlotStatus(index, side) == true)
                { arrayOfCompatibility[index].gameObject.SetActive(true); }
                else
                {
                    arrayOfCompatibility[index].gameObject.SetActive(false);
                    UpdateActorCompatibilityUI(index, 0);
                }
            }
        }

        /*//player is never vacant -> EDIT player Power shown regardless
        powerCirclePlayer.gameObject.SetActive(showPower);*/

        //update flag
        isPowerUI = showPower;
        //logging
        Debug.LogFormat("[UI] ActorPanelUI.cs -> ToggleActorInfoUI: {0}{1}", showPower == true ? "POWER" : "COMPATIBILITY", "\n");
    }

    /// <summary>
    /// returns true if actor Info UI shows POWER, false if COMPATIBILITY
    /// </summary>
    /// <returns></returns>
    public bool CheckInfoUIStatus()
    { return isPowerUI; }

    /// <summary>
    /// updates actor slot Power UI with current Power
    /// </summary>
    /// <param name="actorSlotID"></param>
    /// <param name="power"></param>
    public void UpdateActorPowerUI(int actorSlotID, int power)
    {
        Debug.Assert(actorSlotID > -1 && actorSlotID < GameManager.i.actorScript.maxNumOfOnMapActors, "Invalid actorSlotID (< 0 or >= maxNumOfOnMapActors");
        Debug.Assert(power > -1, "Invalid Power (< 0)");
        switch (actorSlotID)
        {
            case 0: powerText0.text = Convert.ToString(power); break;
            case 1: powerText1.text = Convert.ToString(power); break;
            case 2: powerText2.text = Convert.ToString(power); break;
            case 3: powerText3.text = Convert.ToString(power); break;
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
        Debug.Assert(actorSlotID > -1 && actorSlotID < GameManager.i.actorScript.maxNumOfOnMapActors, "Invalid actorSlotID (< 0 or >= maxNumOfOnMapActors");
        Debug.Assert(compatibility >= -3 && compatibility <= 3, "Invalid compatibility (< -3 or > +3)");
        switch (actorSlotID)
        {
            case 0: compatibility0.text = GameManager.i.guiScript.GetCompatibilityStars(compatibility); break;
            case 1: compatibility1.text = GameManager.i.guiScript.GetCompatibilityStars(compatibility); break;
            case 2: compatibility2.text = GameManager.i.guiScript.GetCompatibilityStars(compatibility); break;
            case 3: compatibility3.text = GameManager.i.guiScript.GetCompatibilityStars(compatibility); break;
            default: Debug.LogWarningFormat("Invalid actorSlotID {0}", actorSlotID); break;
        }
    }

    /// <summary>
    /// updates Power UI with player's current Power
    /// </summary>
    /// <param name="power"></param>
    public void UpdatePlayerPowerUI(int power)
    { powerTextPlayer.text = Convert.ToString(power); }


    //new methods above here
}
