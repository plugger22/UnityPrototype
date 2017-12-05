using System.Collections;
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

    List<TextMeshProUGUI> listOfActorTypes = new List<TextMeshProUGUI>();
    List<Image> listOfActorPortraits = new List<Image>();


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
        //populate lists
        listOfActorTypes.Add(type0);
        listOfActorTypes.Add(type1);
        listOfActorTypes.Add(type2);
        listOfActorTypes.Add(type3);
        listOfActorPortraits.Add(picture0);
        listOfActorPortraits.Add(picture1);
        listOfActorPortraits.Add(picture2);
        listOfActorPortraits.Add(picture3);
        //assign actor text & sprites (type of Actor)
        UpdateActorGUI();
        //event listener
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent);
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

    /// <summary>
    /// places actor data (type and sprite) into GUI elements via lists
    /// </summary>
    public void UpdateActorGUI()
    {
        int numOfActors = GameManager.instance.actorScript.numOfOnMapActors;
        Actor[] arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(GameManager.instance.optionScript.PlayerSide);
        if (arrayOfActors != null)
        {
            if (arrayOfActors.Length == numOfActors)
            {
                for (int i = 0; i < numOfActors; i++)
                {
                    listOfActorTypes[i].text = arrayOfActors[i].arc.name;
                    listOfActorPortraits[i].sprite = arrayOfActors[i].arc.baseSprite;
                }
            }
            else { Debug.LogWarning("Invalid number of Actors (listOfActors doesn't correspond to numOfActors). Texts not updated."); }
        }
        else { Debug.LogError("Invalid listOfActors (Null)"); }
    }

}
