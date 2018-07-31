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
    [Tooltip("Flash interval (in real seconds) for InfoApp alerts over the top of Request and Meeting tabs")]
    [Range(0.1f, 1.0f)] public float flashInfoTabTime = 0.4f;

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
    [Tooltip("Used for Player or Actor having been captured (152 x 160 png)")]
    public Sprite capturedSprite;
    [Tooltip("Used for Player or Actor being released from captivity (152 x 160 png)")]
    public Sprite releasedSprite;
    [Tooltip("Used for Player being Fired by their Faction")]
    public Sprite firedSprite;
    [Tooltip("Default City Arc sprite (512 x 150 png)")]
    public Sprite cityArcDefaultSprite;
    [Tooltip("Used for message Information Alerts (152 x 160 png)")]
    public Sprite alertInformationSprite;
    [Tooltip("Used for message Warning Alerts (152 x 160 png)")]
    public Sprite alertWarningSprite;
    [Tooltip("Used for message Random (152 x 160 png)")]
    public Sprite alertRandomSprite;
    [Tooltip("Used for ai reboot messages (152 x 160 png)")]
    public Sprite aiRebootSprite;
    [Tooltip("Used for ai countermeasures (152 x 160 png)")]
    public Sprite aiCountermeasureSprite;
    [Tooltip("Used for ai alert status changes (152 x 160 png)")]
    public Sprite aiAlertSprite;
    [Tooltip("Used for ongoing effects (152 x 160 png")]
    public Sprite ongoingEffectSprite;
    [Tooltip("Used for node Crisis (152 x 160 png")]
    public Sprite nodeCrisisSprite;
    [Tooltip("Used for city loyalty changes (152 x 160 png)")]
    public Sprite cityLoyaltySprite;

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

    public void Awake()
    {
        //Check sprites are present
        Debug.Assert(vacantAuthorityActor != null, "Invalid vacantAuthorityActor (Null)");
        Debug.Assert(vacantResistanceActor != null, "Invalid vacantResistanceActor (Null)");
        Debug.Assert(errorSprite != null, "Invalid errorSprite (Null)");
        Debug.Assert(infoSprite != null, "Invalid infoSprite (Null)");
        Debug.Assert(alarmSprite != null, "Invalid alarmSprite (Null)");
        Debug.Assert(targetSuccessSprite != null, "Invalid targetSuccessSprite (Null)");
        Debug.Assert(targetFailSprite != null, "Invalid targetFailSprite (Null)");
        Debug.Assert(capturedSprite != null, "Invalid capturedSprite (Null)");
        Debug.Assert(releasedSprite != null, "Invalid releasedSprite (Null)");
        Debug.Assert(firedSprite != null, "Invalid firedSprite (Null)");
        Debug.Assert(cityArcDefaultSprite != null, "Invalid cityArcDefaultSprite (Null)");
        Debug.Assert(alertInformationSprite != null, "Invalid alertInformationSprite (Null)");
        Debug.Assert(alertWarningSprite != null, "Invalid alertWarningSprite (Null)");
        Debug.Assert(alertRandomSprite != null, "Invalid alertRandomSprite (Null)");
        Debug.Assert(aiRebootSprite != null, "Invalid aiRebootSpirte (Null)");
        Debug.Assert(aiCountermeasureSprite != null, "Invalid aiCountermeasureSprite (Null)");
        Debug.Assert(aiAlertSprite != null, "Invalid aiAlertSprite (Null)");
        Debug.Assert(ongoingEffectSprite != null, "Invalid ongoingEffectSprite (Null)");
        Debug.Assert(nodeCrisisSprite != null, "Invalid nodeCrisisSprite (Null)");
        Debug.Assert(cityLoyaltySprite != null, "Invalid cityLoyaltySprite (Null)");
    }

    /// <summary>
    /// Initialises GUI with all relevant data
    /// </summary>
    /// <param name="arrayOfActors"></param>
    public void Initialise()
    {
        #region archived code -> finding and using objects in GameManager rather than PanelManager
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
        #endregion

        //make sure blocking layers are all set to false
        isBlocked = new bool[numOfModalLevels + 1];
        for (int i = 0; i < isBlocked.Length; i++)
        { isBlocked[i] = false; }
        //event listener
        /*EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "GUIManager");*/
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "GUIManager");
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
