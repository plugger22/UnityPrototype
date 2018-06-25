using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using gameAPI;
using TMPro;
using modalAPI;

/// <summary>
/// Customises and Manages the main GUI
/// </summary>
public class GUIManager : MonoBehaviour
{
    [Tooltip("Alpha of Actor portraits when ActorStatus is 'Active'")]
    [Range(0f,1f)] public float alphaActive = 1.0f;
    [Tooltip("Alpha of Actor portraits when ActorStatus is 'InActive'")]
    [Range(0f, 1f)] public float alphaInactive = 0.45f;
    [Tooltip("How many blocking modal levels are there? eg. the number of stackable UI levels?")]
    [Range(1,2)] public int numOfModalLevels = 2;               //NOTE: change this > 2 you'll have to tweak a few switch/case structures, search on 'modalLevel'

    [Header("Flashing")]
    [Tooltip("How long it takes, in seconds, for the flashing red security alert (WidgetTopUI) to go from zero to full opacity")]
    [Range(0.5f, 2.0f)] public float flashRedTime = 1.0f;
    [Tooltip("How long it takes, in seconds, for the flashing white alert to go from zero to full opacity")]
    [Range(0.5f, 2.0f)] public float flashAlertTime = 1.0f;
    [Tooltip("Flash interval (in real seconds) for nodes")]
    [Range(0.1f, 1.0f)] public float flashNodeTime = 0.4f;
    [Tooltip("Flash interval (in real seconds) for connections")]
    [Range(0.1f, 1.0f)] public float flashConnectiontTime = 0.4f; 

    [Header("Sprites")]
    [Tooltip("Sprite to use for ActorGUI to show that the position is vacant")]
    public Sprite vacantAuthorityActor;
    [Tooltip("Sprite to use for ActorGUI to show that the position is vacant")]
    public Sprite vacantResistanceActor;
    [Tooltip("Universal Error sprite")]
    public Sprite errorSprite;
    [Tooltip("Universal Info sprite")]
    public Sprite infoSprite;
    [Tooltip("Alarm (spotted) sprite")]
    public Sprite alarmSprite;
    [Tooltip("Used for Target attempts that succeed")]
    public Sprite targetSuccessSprite;
    [Tooltip("Used for Target attempts that fail")]
    public Sprite targetFailSprite;
    [Tooltip("Used for Player or Actor having been captured")]
    public Sprite capturedSprite;
    [Tooltip("Default City Arc sprite (512 x 150 png")]
    public Sprite cityArcDefaultSprite;


    /*//Actor display at bottom
    private GameObject Actor0;
    private GameObject Actor1;
    private GameObject Actor2;
    private GameObject Actor3;
    private GameObject ActorPlayer;

    private Image picture0;
    private Image picture1;
    private Image picture2;
    private Image picture3;
    private Image picturePlayer;

    private TextMeshProUGUI type0;
    private TextMeshProUGUI type1;
    private TextMeshProUGUI type2;
    private TextMeshProUGUI type3;
    private TextMeshProUGUI typePlayer;

    private Image renownCircle0;
    private Image renownCircle1;
    private Image renownCircle2;
    private Image renownCircle3;
    private Image renownCirclePlayer;

    private TextMeshProUGUI renownText0;
    private TextMeshProUGUI renownText1;
    private TextMeshProUGUI renownText2;
    private TextMeshProUGUI renownText3;
    private TextMeshProUGUI renownTextPlayer;

    private CanvasGroup canvas0;
    private CanvasGroup canvas1;
    private CanvasGroup canvas2;
    private CanvasGroup canvas3;
    private CanvasGroup canvasPlayer;*/

    /*List<TextMeshProUGUI> listOfActorTypes = new List<TextMeshProUGUI>();       //actors (not player)
    List<Image> listOfActorPortraits = new List<Image>();                       //actors (not player)*/

    private bool[] isBlocked;                                         //set True to selectively block raycasts onto game scene, eg. mouseover tooltips, etc.
                                                                      //to block use -> 'if (isBlocked == false)' in OnMouseDown/Over/Exit etc.
                                                                      //array corresponds to modalLevel, one block setting for each level, level 1 is isBlocked[1]

    //colour palette 
    private string colourAlert;
    private string colourGood;
    private string colourNeutral;
    //private string colourGrey;
    private string colourBad;
    //private string colourNormal;
    private string colourEnd;

    /// <summary>
    /// Initialises GUI with all relevant data
    /// </summary>
    /// <param name="arrayOfActors"></param>
    public void Initialise()
    {
        /*//get actor obj references
        Actor0 = GameObject.Find("Actor0");
        Actor1 = GameObject.Find("Actor1");
        Actor2 = GameObject.Find("Actor2");
        Actor3 = GameObject.Find("Actor3");
        ActorPlayer = GameObject.Find("ActorPlayer");
        Debug.Assert(Actor0 != null, "Invalid Actor0 (Null)");
        Debug.Assert(Actor1 != null, "Invalid Actor1 (Null)");
        Debug.Assert(Actor2 != null, "Invalid Actor2 (Null)");
        Debug.Assert(Actor3 != null, "Invalid Actor3 (Null)");
        Debug.Assert(ActorPlayer != null, "Invalid ActorPlayer (Null)");
        //get sprite face references 
        GameObject temp0 = Actor0.transform.Find("Face").gameObject;
        GameObject temp1 = Actor1.transform.Find("Face").gameObject;
        GameObject temp2 = Actor2.transform.Find("Face").gameObject;
        GameObject temp3 = Actor3.transform.Find("Face").gameObject;
        GameObject tempPlayer = ActorPlayer.transform.Find("Face").gameObject;
        Debug.Assert(temp0 != null, "Invalid temp0 (Null)");
        Debug.Assert(temp1 != null, "Invalid temp1 (Null)");
        Debug.Assert(temp2 != null, "Invalid temp2 (Null)");
        Debug.Assert(temp3 != null, "Invalid temp3 (Null)");
        Debug.Assert(tempPlayer != null, "invalid tempPlayer (Null)");
        picture0 = temp0.GetComponentInChildren<Image>();
        picture1 = temp1.GetComponentInChildren<Image>();
        picture2 = temp2.GetComponentInChildren<Image>();
        picture3 = temp3.GetComponentInChildren<Image>();
        picturePlayer = tempPlayer.GetComponentInChildren<Image>();
        //get 
        //Can get away with the easy search as there is only a single TMPro object within the master Actor Object Prefab. Can't do this with image above as > 1
        type0 = Actor0.GetComponentInChildren<TextMeshProUGUI>();
        type1 = Actor1.GetComponentInChildren<TextMeshProUGUI>();
        type2 = Actor2.GetComponentInChildren<TextMeshProUGUI>();
        type3 = Actor3.GetComponentInChildren<TextMeshProUGUI>();
        typePlayer = ActorPlayer.GetComponentInChildren<TextMeshProUGUI>();

        //Canvas Group references
        canvas0 = Actor0.GetComponent<CanvasGroup>();
        canvas1 = Actor1.GetComponent<CanvasGroup>();
        canvas2 = Actor2.GetComponent<CanvasGroup>();
        canvas3 = Actor3.GetComponent<CanvasGroup>();
        canvasPlayer = ActorPlayer.GetComponent<CanvasGroup>();
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
        picture0.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 0;
        picture1.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 1;
        picture2.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 2;
        picture3.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 3;
        //Player
        typePlayer.text = "PLAYER";
        if (GameManager.instance.playerScript.sprite != null)
        { picturePlayer.sprite = GameManager.instance.playerScript.sprite; }
        else { picturePlayer.sprite = GameManager.instance.guiScript.errorSprite; }*/
        //make sure blocking layers are all set to false
        isBlocked = new bool[numOfModalLevels + 1];
        for (int i = 0; i < isBlocked.Length; i++)
        { isBlocked[i] = false; }
        //event listener
        /*EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "GUIManager");*/
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "GUIManager");
    }

    public void InitialiseLate()
    {
        //assign actor text & sprites (type of Actor)
        /*UpdateActorGUI();*/
    }

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
            /*case EventType.ChangeSide:
                UpdateActorGUI();
                break;*/
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// set colour palette for Generic Tool tip
    /// </summary>
    public void SetColours()
    {
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        //colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        //colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }


    //
    // - - - Actors - - -
    //

    /*/// <summary>
    /// places actor data (type and sprite) into GUI elements via lists (player is static so no need to update)
    /// </summary>
    public void UpdateActorGUI()
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
                        for (int i = 0; i < numOfActors; i++)
                        {
                            //check if actorSlotID has an actor first
                            if (GameManager.instance.dataScript.CheckActorSlotStatus(i, side) == true)
                            {
                                listOfActorTypes[i].text = arrayOfActors[i].arc.name;
                                listOfActorPortraits[i].sprite = arrayOfActors[i].arc.baseSprite;
                            }
                            else
                            {
                                //vacant position
                                listOfActorTypes[i].text = "VACANT";
                                switch (side.level)
                                {
                                    case 1: listOfActorPortraits[i].sprite = vacantAuthorityActor; break;
                                    case 2: listOfActorPortraits[i].sprite = vacantResistanceActor; break;
                                    default:
                                        Debug.LogError(string.Format("Invalid side.level \"{0}\" {1}", side.name, side.level));
                                        break;
                                }

                            }
                        }
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
    /// changes the alpha of the player sprite and text
    /// </summary>
    /// <param name="alpha"></param>
    public void UpdatePlayerAlpha(float alpha)
    { canvasPlayer.alpha = alpha; }*/


    //
    // - - - GUI - - -
    //

    /// <summary>
    /// set True to selectively block raycasts onto game scene, eg. mouseover tooltips, etc.
    /// NOTE: UI elements are blocked through modal panels masks (ModalGUI.cs) and node gameobjects through code (gamestate)
    /// level is default 1, only use 2 if you require a UI element to open/close ontop of another, retained, UI element. Eg menu ontop of InventoryUI
    /// </summary>
    /// <param name="isBlocked"></param>
    public void SetIsBlocked(bool isBlocked, int level = 1)
    {
        Debug.Assert(level <= numOfModalLevels, string.Format("Invalid level {0}, max is numOfModalLevels {1}", level, numOfModalLevels));
        this.isBlocked[level] = isBlocked;
        Debug.Log(string.Format("GUIManager: Blocked -> {0}, level {1}{2}", isBlocked, level, "\n"));
        GameManager.instance.modalGUIScropt.SetModalMasks(isBlocked, level);
    }

    /// <summary>
    /// checks if modal layer blocked for a particular level
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public bool CheckIsBlocked(int level = 1)
    {
        Debug.Assert(level <= numOfModalLevels, string.Format("Invalid level {0}, max is numOfModalLevels {1}", level, numOfModalLevels));
        return isBlocked[level];
    }


    /// <summary>
    /// generates an Alert messsage (generic UI) of a particular type (extend by adding to enum and providing relevant code to handle below)
    /// </summary>
    /// <param name="type"></param>
    public void SetAlertMessage(AlertType type)
    {
        ModalOutcomeDetails details = new ModalOutcomeDetails();
        details.sprite = infoSprite;
        details.side = GameManager.instance.sideScript.PlayerSide;
        switch (type)
        {
            case AlertType.SomethingWrong:
                //generic fault message
                details.textTop = string.Format("{0}Something has gone horribly wrong{1}", colourBad, colourEnd);
                details.textBottom = "We aren't sure what but we've got our best man on it";
                break;
            case AlertType.PlayerStatus:
                switch (GameManager.instance.playerScript.status)
                {
                    case ActorStatus.Captured:
                        details.textTop = string.Format("This action can't be taken because the Player has been {0}Captured{1}", colourBad, colourEnd);
                        break;
                    case ActorStatus.Inactive:
                        switch (GameManager.instance.playerScript.inactiveStatus)
                        {
                            case ActorInactive.Breakdown:
                                details.textTop = string.Format("This action can't be taken because the Player is undergoing a {0}Breakdown{1} (Stress)", 
                                    colourBad, colourEnd);
                                break;
                            default:
                                details.textTop = string.Format("{0}This action can't be taken because the Player is indisposed{1}", colourAlert, colourEnd);
                                break;
                        }
                        break;
                    default:
                        details.textTop = "This action can't be taken because the Player is indisposed";
                        break;
                }
                break;
            case AlertType.SideStatus:
                details.textTop = string.Format("{0}This action is unavailable as the AI controls this side{1}", colourAlert, colourEnd);
                break;
            case AlertType.DebugAI:
                details.textTop = "The AI has been switched OFF" ;
                details.textBottom = string.Format("The Player now has {0}<b>Manual control</b>{1} of both sides", colourNeutral, colourEnd);
                break;
            case AlertType.DebugPlayer:
                details.textTop = "The AI has been switched back ON (Authority)";
                details.textBottom = string.Format("The Player has {0}<b>Manual control</b>{1} of the Resistance side only", colourNeutral, colourEnd);
                break;
            case AlertType.HackingInitialising:
                details.textTop = "Jacking into Authority AI. Initialising Icebreakers.";
                details.textBottom = string.Format("{0}Wait one...{1}", colourNeutral, colourEnd);
                break;
            case AlertType.HackingInsufficientRenown:
                details.textTop = string.Format("You have {0}{1}Insufficient Renown{2}{3}for a Hacking attempt", "\n", colourBad, colourEnd, "\n");
                details.textBottom = string.Format("Check the colour of the Renown cost. If {0}Yellow{1} then just enough, {2}Green{3} then more than enough",
                    colourNeutral, colourEnd, colourGood, colourEnd);
                break;
            case AlertType.HackingRebootInProgress:
                details.textTop = string.Format("The AI is {0}Rebooting{1} it's Security Systems", colourBad, colourEnd);
                details.textBottom = string.Format("Hacking attempts {0}aren't possible{1} until the Reboot is complete", colourNeutral, colourEnd);
                break;
            case AlertType.HackingOffline:
                details.textTop = string.Format("The AI has been {0}Isolated{1} from all external access", colourBad, colourEnd);
                details.textBottom = string.Format("Until the AI comes back online hacking attempts {0}aren't possible{1}", colourNeutral, colourEnd);
                break;
            case AlertType.HackingIndisposed:
                details.textTop = string.Format("The AI is currently {0}inaccessible{1}", colourBad, colourEnd);
                details.textBottom = string.Format("This is a {0}temporary{1} state and is due to the Player being {2}indisposed{3}", colourNeutral, colourEnd, 
                    colourNeutral, colourEnd);
                break;
            default:
                details.textTop = "This action is unavailable";
                break;
        }
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details, "GUIManager.cs -> SetAlertMessage");
    }
}
