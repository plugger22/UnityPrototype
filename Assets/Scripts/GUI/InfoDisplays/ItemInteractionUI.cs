using gameAPI;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// handles item interaction (click) for MainInfoApp and MetaGame items. Click on an item and display details 
/// NOTE: multi-use code for both MainInfoApp and MetaGameUI
/// </summary>
public class ItemInteractionUI : MonoBehaviour, IPointerClickHandler
{

    private int itemIndex = -1;          //assigned at start by MainInfoUI, corresponds to listOfCurrentPageData[index]
    private MajorUI majorType;           //used to determine which major UI is currently in use (class attached to prefab Item's that can be used in MainInfoApp or MetaGameUI)

    /// <summary>
    /// sets item index
    /// </summary>
    /// <param name="index"></param>
    public void SetItemIndex(int index, int maxItemNumber)
    {
        Debug.Assert(index > -1 && index < maxItemNumber, string.Format("Invalid index (must be > -1 && < {0})", maxItemNumber));
        itemIndex = index;
        /*Debug.LogFormat("[Tst] ItemInteractionUI.cs -> SetItemIndex: itemIndex {0}{1}", itemIndex, "\n");*/
    }

    public void SetUIType(MajorUI majorTypeUI)
    { majorType = majorTypeUI; }

    public int GetItemIndex()
    { return itemIndex; }

    /// <summary>
    /// Mouse click -> Show details, if any, in RHS
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (majorType)
        {
            case MajorUI.MainInfoApp:
                switch (eventData.button)
                {
                    //same for left or right click
                    case PointerEventData.InputButton.Left:
                    case PointerEventData.InputButton.Right:
                        EventManager.i.PostNotification(EventType.MainInfoShowDetails, this, itemIndex, "ItemInteractionUI.cs -> OnPointClick");
                        break;
                }
                break;
            case MajorUI.MetaGameUI:
                switch (eventData.button)
                {
                    case PointerEventData.InputButton.Left:
                        //show details
                        EventManager.i.PostNotification(EventType.MetaGameShowDetails, this, itemIndex, "ItemInteractionUI.cs -> OnPointClick"); break;
                    case PointerEventData.InputButton.Right:
                        //show details and select/deselect
                        EventManager.i.PostNotification(EventType.MetaGameShowDetails, this, itemIndex, "ItemInteractionUI.cs -> OnPointClick");
                        EventManager.i.PostNotification(EventType.MetaGameButton, this, itemIndex, "ItemInteractionUI.cs -> OnPointClick");
                        break;
                }
                break;
            default: Debug.LogWarningFormat("Invalid majorType \"{0}\"", majorType); break;
        }
    }

}
