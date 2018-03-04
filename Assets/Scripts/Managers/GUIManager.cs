﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using gameAPI;
using TMPro;

/// <summary>
/// Customises and Manages the main GUI
/// </summary>
public class GUIManager : MonoBehaviour
{
    [Tooltip("Alpha of Actor portraits when ActorStatus is 'Active'")]
    [Range(0f,1f)] public float alphaActive = 1.0f;
    [Tooltip("Alpha of Actor portraits when ActorStatus is 'InActive'")]
    [Range(0f, 1f)] public float alphaInactive = 0.45f;

    [Tooltip("Sprite to use for ActorGUI to show that the position is vacant")]
    public Sprite vacantAuthorityActor;
    [Tooltip("Sprite to use for ActorGUI to show that the position is vacant")]
    public Sprite vacantResistanceActor;

    //Actor display at bottom
    private GameObject Actor0;
    private GameObject Actor1;
    private GameObject Actor2;
    private GameObject Actor3;

    private Image picture0;
    private Image picture1;
    private Image picture2;
    private Image picture3;

    private TextMeshProUGUI type0;
    private TextMeshProUGUI type1;
    private TextMeshProUGUI type2;
    private TextMeshProUGUI type3;

    private CanvasGroup canvas0;
    private CanvasGroup canvas1;
    private CanvasGroup canvas2;
    private CanvasGroup canvas3;

    List<TextMeshProUGUI> listOfActorTypes = new List<TextMeshProUGUI>();
    List<Image> listOfActorPortraits = new List<Image>();

    private bool isBlocked;                                         //set True to selectively block raycasts onto game scene, eg. mouseover tooltips, etc.
                                                                    //to block use -> 'if (isBlocked == false)' in OnMouseDown/Over/Exit etc.

    /// <summary>
    /// Initialises GUI with all relevant data
    /// </summary>
    /// <param name="arrayOfActors"></param>
    public void Initialise()
    {

        /*List<TextMeshProUGUI> listOfActorTypes = new List<TextMeshProUGUI>();
        List<Image> listOfActorPortraits = new List<Image>();*/
        
        //get actor obj references
        Actor0 = GameObject.Find("Actor0");
        Actor1 = GameObject.Find("Actor1");
        Actor2 = GameObject.Find("Actor2");
        Actor3 = GameObject.Find("Actor3");
        //get image and text references 
        GameObject temp0 = Actor0.transform.Find("Face").gameObject;
        GameObject temp1 = Actor1.transform.Find("Face").gameObject;
        GameObject temp2 = Actor2.transform.Find("Face").gameObject;
        GameObject temp3 = Actor3.transform.Find("Face").gameObject;
        picture0 = temp0.GetComponentInChildren<Image>();
        picture1 = temp1.GetComponentInChildren<Image>();
        picture2 = temp2.GetComponentInChildren<Image>();
        picture3 = temp3.GetComponentInChildren<Image>();
        //Can get away with the easy search as there is only a single TMPro object within the master Actor Object Prefab. Can't do this with image above as > 1
        type0 = Actor0.GetComponentInChildren<TextMeshProUGUI>();
        type1 = Actor1.GetComponentInChildren<TextMeshProUGUI>();
        type2 = Actor2.GetComponentInChildren<TextMeshProUGUI>();
        type3 = Actor3.GetComponentInChildren<TextMeshProUGUI>();
        //Canvas Group references
        canvas0 = Actor0.GetComponent<CanvasGroup>();
        canvas1 = Actor1.GetComponent<CanvasGroup>();
        canvas2 = Actor2.GetComponent<CanvasGroup>();
        canvas3 = Actor3.GetComponent<CanvasGroup>();
        //populate lists
        listOfActorTypes.Add(type0);
        listOfActorTypes.Add(type1);
        listOfActorTypes.Add(type2);
        listOfActorTypes.Add(type3);
        listOfActorPortraits.Add(picture0);
        listOfActorPortraits.Add(picture1);
        listOfActorPortraits.Add(picture2);
        listOfActorPortraits.Add(picture3);
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
        //make sure raycasts are active, eg. node tooltips
        isBlocked = false;
        //event listener
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
    }

    public void InitialiseLate()
    {
        //assign actor text & sprites (type of Actor)
        UpdateActorGUI();
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
                UpdateActorGUI();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    //
    // - - - Actors - - -
    //

    /// <summary>
    /// places actor data (type and sprite) into GUI elements via lists
    /// </summary>
    public void UpdateActorGUI()
    {
        int numOfActors = GameManager.instance.actorScript.maxNumOfOnMapActors;
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(side);
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

    /// <summary>
    /// changes the alpha of an actor sprite and text
    /// </summary>
    /// <param name="actorSlotID"></param>
    /// <param name="alpha"></param>
    public void UpdateActorAlpha(int actorSlotID, float alpha)
    {
        Debug.Assert(actorSlotID > -1 && actorSlotID < GameManager.instance.actorScript.maxNumOfOnMapActors, "Invalid slotID");
        switch(actorSlotID)
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

    /// <summary>
    /// set True to selectively block raycasts onto game scene, eg. mouseover tooltips, etc.
    /// </summary>
    /// <param name="isBlocked"></param>
    public void SetIsBlocked(bool isBlocked)
    {
        this.isBlocked = isBlocked;
        Debug.Log(string.Format("GM: Blocked -> {0}{1}", isBlocked, "\n"));
    }

    public bool CheckIsBlocked()
    { return isBlocked; }


}
