using modalAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attached to images in GenericPickerUI to handle clicks for option selection
/// </summary>
public class ImageInteraction : MonoBehaviour, IPointerClickHandler
{
    public Image highlightImage;

    [HideInInspector] public GenericReturnData data;
    [HideInInspector] public bool isActive;             //is the option a valid choice option or is it greyed out? (can't be selected)

    private bool isSelected;                            //has the option been selected

    /// <summary>
    /// Internal initialisation
    /// </summary>
    private void Awake()
    {
        isSelected = false;
        data = new GenericReturnData();
    }

    /// <summary>
    /// External initialisation
    /// </summary>
    private void Start()
    {
        //event listener
        EventManager.i.AddListener(EventType.DeselectOtherGenerics, OnEvent, "ImageInteraction");
    }

    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.DeselectOtherGenerics:
                DeSelectGeneric();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// Activates highlight image when clicked
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        //check option is a viable choice
        if (isActive == true)
        {
            Color tempColour = highlightImage.color;
            if (isSelected == true)
            {
                tempColour.a = 0.0f;
                isSelected = false;
                //deactivate Confirm button (only shows when a team is selected)
                EventManager.i.PostNotification(EventType.ConfirmGenericDeactivate, this, null, "GenericInteraction.cs -> OnPointerClick");
            }
            else
            {
                //deactivate any other currently selected option and switch Confirm button on
                EventManager.i.PostNotification(EventType.DeselectOtherGenerics, this, null, "GenericInteraction.cs -> OnPointerClick");
                EventManager.i.PostNotification(EventType.ConfirmGenericActivate, this, data, "GenericInteraction.cs -> OnPointerClick");
                //NOTE: Event call must be made BEFORE activating this choice otherwise it'll deactivate it immediately
                tempColour.a = 1.0f;
                isSelected = true;
            }
            //assign new values to Image
            highlightImage.color = tempColour;
        }
    }


    /// <summary>
    /// Deselect generic choice (event call) as a result of another generic choice being selected
    /// </summary>
    private void DeSelectGeneric()
    {
        Color tempColour = highlightImage.color;
        if (isSelected == true)
        {
            tempColour.a = 0.0f;
            isSelected = false;
        }
        //assign new values to Image
        highlightImage.color = tempColour;
    }
}
