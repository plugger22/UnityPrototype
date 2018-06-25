using gameAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public TextMeshProUGUI type0;
    public TextMeshProUGUI type1;
    public TextMeshProUGUI type2;
    public TextMeshProUGUI type3;
    public TextMeshProUGUI typePlayer;

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

    public bool isRenownUI;                             //gives status of renown UI display (true -> On, false -> Off)

    //fast access
    private Sprite vacantAuthorityActor;
    private Sprite vacantResistanceActor;


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

    }


    public void Initialise()
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
        //fast access
        vacantAuthorityActor = GameManager.instance.guiScript.vacantAuthorityActor;
        vacantResistanceActor = GameManager.instance.guiScript.vacantResistanceActor;
        Debug.Assert(vacantAuthorityActor != null, "Invalid vacantAuthorityActor (Null)");
        Debug.Assert(vacantResistanceActor != null, "Invalid vacantResistanceActor (Null)");
        //player
        typePlayer.text = "PLAYER";
        if (GameManager.instance.playerScript.sprite != null)
        { picturePlayer.sprite = GameManager.instance.playerScript.sprite; }
        else { picturePlayer.sprite = GameManager.instance.guiScript.errorSprite; }
        //initialise starting line up
        UpdateActorPanel();
        SetActorRenownUI(false);
        //event listener
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "ActorPanelUI");
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
            case EventType.ChangeSide:
                UpdateActorPanel();
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
                                listOfActorPortraits[index].sprite = arrayOfActors[index].arc.baseSprite;
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
    { canvasPlayer.alpha = alpha; }

    /// <summary>
    /// toggles actor renown display on/off depending on status
    /// </summary>
    /// <param name="status"></param>
    public void SetActorRenownUI(bool status)
    {
        //set active/inactive
        renownCircle0.gameObject.SetActive(status);
        renownCircle1.gameObject.SetActive(status);
        renownCircle2.gameObject.SetActive(status);
        renownCircle3.gameObject.SetActive(status);
        renownCirclePlayer.gameObject.SetActive(status);
        //update flag
        isRenownUI = status;
        //logging
        Debug.LogFormat("[UI] ActorPanelUI.cs -> ToggleActorRenown: {0}{1}", status, "\n");
    }

    /// <summary>
    /// returns true if actor Renown UI is ON, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool CheckRenownUIStatus()
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
        switch(actorSlotID)
        {
            case 0:
                renownText0.text = Convert.ToString(renown);
                break;
            case 1:
                renownText1.text = Convert.ToString(renown);
                break;
            case 2:
                renownText2.text = Convert.ToString(renown);
                break;
            case 3:
                renownText3.text = Convert.ToString(renown);
                break;
            default:
                Debug.LogWarningFormat("Invalid actorSlotID {0}", actorSlotID);
                break;
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
