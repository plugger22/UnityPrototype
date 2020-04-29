using gameAPI;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// handles item interaction (click) for RHS. Click on an item and display details in LHS  (NOTE: sides have been reversed to what is described, eg. click on LHS item and display details in RHS)
/// NOTE: multi-use code for both MainInfoApp and MetaGameUI
/// </summary>
public class MainInfoRightItemUI : MonoBehaviour, IPointerClickHandler
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
        /*Debug.LogFormat("[Tst] MainInfoRightItemUI.cs -> SetItemIndex: itemIndex {0}{1}", itemIndex, "\n");*/
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
                        EventManager.instance.PostNotification(EventType.MainInfoShowDetails, this, itemIndex, "MainInfoRightItemUI.cs -> OnPointClick");
                        break;
                }
                break;
            case MajorUI.MetaGameUI:
                switch (eventData.button)
                {
                    //same for left or right click
                    case PointerEventData.InputButton.Left:
                    case PointerEventData.InputButton.Right:
                        EventManager.instance.PostNotification(EventType.MetaGameShowDetails, this, itemIndex, "MainInfoRightItemUI.cs -> OnPointClick");
                        break;
                }
                break;
            default: Debug.LogWarningFormat("Invalid majorType \"{0}\"", majorType); break;
        }
    }

}
