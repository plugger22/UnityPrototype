using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)

public enum ToolEventType
{
    None,
    //Tools
    QuitTools,
    //Adventure Main
    OpenAdventureUI,
    CloseAdventureUI,
    OpenNewAdventure,
    CreateTurningPoint,
    SaveToolsToFile,
    LoadToolsFromFile,
    DeleteToolsFile,
    ClearStoryDictionary,
    ExportStoryDictionary,
    ExportOrgs,
    OpenConstants,
    EditAdventure,
    SaveAdventure,
    NextAdventure,
    PreviousAdventure,
    OpenMainDetails,
    CloseMainDetails,
    MainSummaryUpArrow,
    MainSummaryDownArrow,
    MainDetailsUpArrow,
    MainDetailsDownArrow,
    MainDetailsRightArrow,
    MainDetailsLeftArrow,
    //Adventure New
    CloseNewAdventure,
    SaveAdventureToDict,
    ClearNewAdventure,
    NewSummaryUpArrow,
    NewSummaryDownArrow,
    NewNameSet,
    //Turning Point
    CloseTurningPoint,
    OpenTurningPoint,
    CreatePlotpoint,
    PreSaveTurningPoint,
    SaveTurningPoint,
    ClearTurningPoint,
    //Lists
    OpenAdventureLists,
    CloseAdventureLists,
    ShowPlotLineDetails,
    ShowCharacterDetails,
    NextLists,
    PreviousLists,
    EditListItem,
    SaveListDetails,
    //DropDown
    CloseDropDown,
    //Constants
    CloseConstants,
    InputConstants,
    EditConstants,
    ViewConstants,
    ClearConstants,
    SaveToDictConstants,
    NextConstant,
    PreviousConstant

}

/// <summary>
/// Event Manager equivalent for Internal tools
/// </summary>
public class ToolEvents : MonoBehaviour
{

    public static ToolEvents i = null;                                 //static instance of toolEvents which allows it to be accessed by any other script
                                                                       // Declare a delegate type for events
    public delegate void OnEvent(ToolEventType Event_Type, Component Sender, object Param = null);

    //Array of listener objects (all objects registered to listen for events)
    private Dictionary<ToolEventType, List<OnEvent>> dictOfListeners = new Dictionary<ToolEventType, List<OnEvent>>();

    private void Awake()
    {
        //check if instance already exists
        if (i == null)
        {
            i = this;
            /*Debug.LogFormat("[Tst] ToolEvents has been instantiated{0}", "\n");*/
        }
        //if instance already exists and it's not this
        else if (i != this)
        {
            //Then destroy this in order to reinforce the singleton pattern (can only ever be one instance of toolEvents)
            Destroy(gameObject);
            Debug.LogWarning("Can only be one instance of ToolEvents");
        }
    }

    /// <summary>
    /// Function to add specified listener-object to array of listeners
    /// </summary>
    /// <param name="eventType">Event to Listen for</param>
    /// <param name="Listener">Object to listen for event</param>
    /// <param name="methodName">Optional -> name of method posting notification (for debugging purposes)</param>
    public void AddListener(ToolEventType eventType, OnEvent Listener, string methodName = "Unknown")
    {
        //List of listeners for this event
        List<OnEvent> ListenList = null;
        //New item to be added. Check for existing event type key. If one exists, add to list
        if (dictOfListeners.TryGetValue(eventType, out ListenList))
        {
            //List exists, so add new item (if not already present -> prevents duplicates in the case of a followOn level)
            if (ListenList.Exists(x => x == Listener) == false)
            { ListenList.Add(Listener); }
        }
        else
        {
            //Otherwise create new list as dictionary key
            ListenList = new List<OnEvent>();
            ListenList.Add(Listener);
            //Add to internal listeners list
            dictOfListeners.Add(eventType, ListenList);
        }
        /*Debug.LogFormat("[Tol] -> Listener Added -> type: {0},  sender: {1}{2}", eventType, methodName, "\n");*/
    }

    /// <summary>
    /// Function to post event to listeners
    /// </summary>
    /// <param name="eventType">Event to invoke</param>
    /// <param name="Sender">Object invoking event</param>
    /// <param name="Param">Optional argument</param>
    /// <param name="methodName">Optional -> name of method posting notification (for debugging purposes)</param>
    public void PostNotification(ToolEventType eventType, Component Sender, object Param = null, string methodName = "Unknown")
    {
        //Notify all listeners of an event

        //List of listeners for this event only
        List<OnEvent> ListenList = null;

        //If no event entry exists, then exit because there are no listeners to notify
        if (!dictOfListeners.TryGetValue(eventType, out ListenList))
        {
            Debug.LogWarningFormat("ToolEvents: Invalid event (No listeners present in dictOfListeners) for \"{0}\"", eventType);
            return;
        }

        //Entry exists. Now notify appropriate listeners
        for (int i = 0; i < ListenList.Count; i++)
        {
            if (ListenList[i] != null)
            {
                if (Param != null)
                { Debug.LogFormat("[Evm]: PostNotification -> type: {0}, param: {1}, {2}, sender: {3}{4}", eventType, Param.ToString(), Param.GetType(), methodName, "\n"); }
                else { Debug.LogFormat("[Evm]: PostNotification -> type: {0}, NO param, sender: {1}{2}", eventType, methodName, "\n"); }
                //If object is not null, then send message via delegate
                ListenList[i](eventType, Sender, Param);
            }
        }
    }


    //Remove event type entry from dictionary, including all listeners
    public void RemoveEvent(ToolEventType eventType)
    {
        //Remove entry from dictionary
        dictOfListeners.Remove(eventType);
    }


    //Remove all redundant entries from the Dictionary
    public void RemoveRedundancies()
    {
        //Create new dictionary
        Dictionary<ToolEventType, List<OnEvent>> TmpListeners = new Dictionary<ToolEventType, List<OnEvent>>();

        //Cycle through all dictionary entries
        foreach (KeyValuePair<ToolEventType, List<OnEvent>> Item in dictOfListeners)
        {
            //Cycle through all listener objects in list, remove null objects
            for (int i = Item.Value.Count - 1; i >= 0; i--)
            {
                //If null, then remove item
                if (Item.Value[i] == null)
                {
                    Debug.LogWarningFormat("ToolEvents.cs -> RemoveRedundancies: eventType {0} removed from dict as Null -> Info only", Item.Key);
                    Item.Value.RemoveAt(i);
                }
            }

            //If items remain in list for this notification, then add this to tmp dictionary
            if (Item.Value.Count > 0)
                TmpListeners.Add(Item.Key, Item.Value);
        }

        //Replace listeners object with new, optimized dictionary
        dictOfListeners = TmpListeners;
    }

}

#endif
