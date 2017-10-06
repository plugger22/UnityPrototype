using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Enum defining all possible game events
//More events should be added to the list
public enum EventType { ChangeColour, ChangeSide };

//EventManager to send events to listeners
//Works with IListener implementations
public class EventManager : MonoBehaviour
{

    // Declare a delegate type for events
    public delegate void OnEvent(EventType Event_Type, Component Sender, object Param = null);

    //Array of listener objects (all objects registered to listen for events)
    private Dictionary<EventType, List<OnEvent>> dictOfListeners = new Dictionary<EventType, List<OnEvent>>();

    
    /// <summary>
    /// Function to add specified listener-object to array of listeners
    /// </summary>
    /// <param name="eventType">Event to Listen for</param>
    /// <param name="Listener">Object to listen for event</param>
    public void AddListener(EventType eventType, OnEvent Listener)
    {
        //List of listeners for this event
        List<OnEvent> ListenList = null;
        //New item to be added. Check for existing event type key. If one exists, add to list
        if (dictOfListeners.TryGetValue(eventType, out ListenList))
        {
            //List exists, so add new item
            ListenList.Add(Listener);
        }
        else
        {
            //Otherwise create new list as dictionary key
            ListenList = new List<OnEvent>();
            ListenList.Add(Listener);
            //Add to internal listeners list
            dictOfListeners.Add(eventType, ListenList); 
        }
        //Debug.Log(string.Format("EventManager -> Listener Added -> {0} for {1}{2}", eventType, Listener.GetType(), "\n"));
    }
    
    /// <summary>
    /// Function to post event to listeners
    /// </summary>
    /// <param name="eventType">Event to invoke</param>
    /// <param name="Sender">Object invoking event</param>
    /// <param name="Param">Optional argument</param>
    public void PostNotification(EventType eventType, Component Sender, object Param = null)
    {
        //Notify all listeners of an event

        //List of listeners for this event only
        List<OnEvent> ListenList = null;

        //If no event entry exists, then exit because there are no listeners to notify
        if (!dictOfListeners.TryGetValue(eventType, out ListenList))
        { return; }

        //Entry exists. Now notify appropriate listeners
        for (int i = 0; i < ListenList.Count; i++)
        {
            if (ListenList[i] != null)
            {
                //If object is not null, then send message via delegate
                ListenList[i](eventType, Sender, Param);
                //Debug.Log(string.Format("PostNotification -> eventType: {0} sender: {1}{2}", eventType, Sender.name, "\n"));
            }
        }
    }
    
    
    //Remove event type entry from dictionary, including all listeners
    public void RemoveEvent(EventType eventType)
    {
        //Remove entry from dictionary
        dictOfListeners.Remove(eventType);
    }
    
    
    //Remove all redundant entries from the Dictionary
    public void RemoveRedundancies()
    {
        //Create new dictionary
        Dictionary<EventType, List<OnEvent>> TmpListeners = new Dictionary<EventType, List<OnEvent>>();

        //Cycle through all dictionary entries
        foreach (KeyValuePair<EventType, List<OnEvent>> Item in dictOfListeners)
        {
            //Cycle through all listener objects in list, remove null objects
            for (int i = Item.Value.Count - 1; i >= 0; i--)
            {
                //If null, then remove item
                if (Item.Value[i].Equals(null))
                    Item.Value.RemoveAt(i);
            }

            //If items remain in list for this notification, then add this to tmp dictionary
            if (Item.Value.Count > 0)
                TmpListeners.Add(Item.Key, Item.Value);
        }

        //Replace listeners object with new, optimized dictionary
        dictOfListeners = TmpListeners;
    }


}
