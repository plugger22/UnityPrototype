using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Event Manager -> Enum defining all possible game events, order independent
public enum EventType
{
    None,
    //Options
    ChangeColour,
    ChangeSide,
    ChangeLevel,
    UseAction,
    ExitGame,
    //UI
    CloseActionMenu,
    OpenOutcomeWindow,
    CloseOutcomeWindow,
    CreateMoveMenu,
    CreateGearNodeMenu,
    //CloseTeamPicker
    ShowTargetNodes,
    NodeDisplay,
    ActivityDisplay,
    //Top Widget
    ChangeActionPoints,
    ChangeCityBar,
    ChangeFactionBar,
    ChangeStarLeft,
    ChangeStarMiddle,
    ChangeStarRight,
    ChangeTurn,
    //Actions
    NodeAction,
    NodeGearAction,
    TargetAction,
    InsertTeamAction,
    RecallTeamAction,
    NeutraliseTeamAction,
    GearAction,
    RecruitAction,
    MoveAction,
    ManageActorAction,
    LieLowAction,
    ActivateAction,
    GiveGearAction,
    UseGearAction,
    //decisions
    RecruitDecision,
    //turn manager
    StartTurnEarly,
    StartTurnLate,
    EndTurnAI,
    EndTurnFinal,
    NewTurn,
    //team Picker
    OpenTeamPicker,
    CloseTeamPicker,
    DeselectOtherTeams,
    ConfirmTeamActivate,
    ConfirmTeamDeactivate,
    ConfirmTeamChoice,
    //generic picker
    OpenGenericPicker,
    CloseGenericPicker,
    DeselectOtherGenerics,
    ConfirmGenericActivate,
    ConfirmGenericDeactivate,
    ConfirmGenericChoice,
    BackButtonGeneric,
    //generic picker returns
    GenericTeamRecall,
    GenericNeutraliseTeam,
    GenericGearChoice,
    GenericRecruitActorResistance,
    GenericRecruitActorAuthority,
    GenericHandleActor,
    GenericReserveActor,
    GenericDismissActor,
    GenericDisposeActor,
    //Inventory UI
    InventorySetReserve,
    InventorySetGear,
    InventoryOpenUI,
    InventoryCloseUI,
    InventoryReassure,
    InventoryThreaten,
    InventoryLetGo,
    InventoryFire,
    InventoryActiveDuty,
    //dice UI
    OpenDiceUI,
    CloseDiceUI,
    DiceIgnore,
    DiceAuto,
    DiceRoll,
    DiceConfirm,
    DiceRenownYes,
    DiceRenownNo,
    DiceBypass,
    DiceReturnMove,
    DiceReturnGear,
    //City Info
    CityInfoOpen,
    CityInfoClose,
    //AI
    Capture,
    ReleasePlayer,
    ReleaseActor
};

//EventManager to send events to listeners
//Works with IListener implementations
public class EventManager : MonoBehaviour
{
    public static EventManager instance = null;

    // Declare a delegate type for events
    public delegate void OnEvent(EventType Event_Type, Component Sender, object Param = null);

    //Array of listener objects (all objects registered to listen for events)
    private Dictionary<EventType, List<OnEvent>> dictOfListeners = new Dictionary<EventType, List<OnEvent>>();


    /// <summary>
    /// Internal initialisation
    /// </summary>
    private void Awake()
    {
        //check if instance already exists
        if (instance == null)
        { instance = this; }
        //if instance already exists and it's not this
        else if (instance != this)
        {
            //Then destroy this in order to reinforce the singleton pattern (can only ever be one instance of GameManager)
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Function to add specified listener-object to array of listeners
    /// </summary>
    /// <param name="eventType">Event to Listen for</param>
    /// <param name="Listener">Object to listen for event</param>
    public void AddListener(EventType eventType, OnEvent Listener, string methodName = "Unknown")
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
        Debug.Log(string.Format("[Evm] -> Listener Added -> type: {0}  sender: {1}{2}", eventType, methodName, "\n"));
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
        {
            /*DEBUG purposes only
            Debug.LogError(string.Format("EventManager: There are no listeners for {0}", eventType));*/
            return;
        }

        //Entry exists. Now notify appropriate listeners
        for (int i = 0; i < ListenList.Count; i++)
        {
            if (ListenList[i] != null)
            {
                Debug.Log(string.Format("[Evm]: PostNotification -> type: {0}{1}", eventType, "\n"));
                //If object is not null, then send message via delegate
                ListenList[i](eventType, Sender, Param);
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
