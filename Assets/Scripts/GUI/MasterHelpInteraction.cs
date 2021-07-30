using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// Provides access to components for MasterHelpOptions (prefab) in ModalHelpUI.cs
/// </summary>
public class MasterHelpInteraction : MonoBehaviour, IPointerClickHandler
{
    public Image panel;
    public TextMeshProUGUI text;

    [HideInInspector] public int index;                      //each option is assigned an index which corresponds to the index for the linked lists in ModalHelpUI.cs

    /// <summary>
    /// Mouse click -> Left: Select Help item
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        //which button
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                //select that option
                EventManager.i.PostNotification(EventType.MasterHelpSelect, this, index, "MasterHelpInteraction.cs -> OnPointerClick");
                break;
        }
    }

    //new methods above here
}
