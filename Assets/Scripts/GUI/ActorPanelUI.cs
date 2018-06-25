using gameAPI;
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

    public CanvasGroup canvas0;
    public CanvasGroup canvas1;
    public CanvasGroup canvas2;
    public CanvasGroup canvas3;
    public CanvasGroup canvasPlayer;

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
        /*//assign actorSlotID's to all Actor components
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
        picture3.GetComponent<ActorSpriteTooltipUI>().actorSlotID = 3;*/
    }


    public void Initialise()
    {
        /*//populate lists
        listOfActorTypes.Add(type0);
        listOfActorTypes.Add(type1);
        listOfActorTypes.Add(type2);
        listOfActorTypes.Add(type3);
        listOfActorPortraits.Add(picture0);
        listOfActorPortraits.Add(picture1);
        listOfActorPortraits.Add(picture2);
        listOfActorPortraits.Add(picture3);

        //Player
        typePlayer.text = "PLAYER";
        if (GameManager.instance.playerScript.sprite != null)
        { picturePlayer.sprite = GameManager.instance.playerScript.sprite; }
        else { picturePlayer.sprite = GameManager.instance.guiScript.errorSprite; }*/

        //fast access
        vacantAuthorityActor = GameManager.instance.guiScript.vacantAuthorityActor;
        vacantResistanceActor = GameManager.instance.guiScript.vacantResistanceActor;
        Debug.Assert(vacantAuthorityActor != null, "Invalid vacantAuthorityActor (Null)");
        Debug.Assert(vacantResistanceActor != null, "Invalid vacantResistanceActor (Null)");
        //initialise starting line up
        UpdateActorPanel();
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

    //new methods above here
}
